param(
	[Parameter(Mandatory=$True)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$True)][string]$Name,
	[Parameter(Mandatory=$True)][string]$SourceRootDir)

$ErrorActionPreference = "Stop"

function ConvertSecretToPlainText($Secret) {

	$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Secret)
	return [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)
}
	
$documentDbUrl = "https://$Name.documents.azure.com/"

$resource = Get-AzResource `
	-ResourceType "Microsoft.DocumentDb/databaseAccounts" `
	-ResourceGroupName $ResourceGroupName `
	-ResourceName $Name `
	-ApiVersion 2020-04-01

$primaryMasterKey = (Invoke-AzResourceAction `
	-Action listKeys `
	-ResourceId $resource.ResourceId `
	-ApiVersion 2020-04-01 `
	-Force).primaryMasterKey

$resource = Get-AzResource `
    -ResourceType "Microsoft.Search/searchServices" `
    -ResourceGroupName $ResourceGroupName `
    -ResourceName $Name `
    -ApiVersion 2020-04-01

# Get the primary admin API key for search.
$primaryKey = (Invoke-AzResourceAction `
    -Action listAdminKeys `
    -ResourceId $resource.ResourceId `
    -ApiVersion 2020-08-01 `
	-Force).PrimaryKey

# Get storage key.
$accountKey = (Get-AzStorageAccountKey -ResourceGroupName $ResourceGroupName -Name $Name).Value[0] 
$connectionString = "DefaultEndpointsProtocol=https;AccountName=$Name;AccountKey=$accountKey;EndpointSuffix=core.windows.net"

$outFile = "$SourceRootDir\Eklee.Azure.Functions.GraphQl.Tests\local.settings.json"

$settings = @{ Search = @{ ServiceName="$Name"; ApiKey="$primaryKey" }; DocumentDb = @{ Key="$primaryMasterKey";Url="$documentDbUrl";RequestUnits="400" }; TableStorage=@{ConnectionString="$connectionString"} } | ConvertTo-Json -Depth 10
$settings | Out-File $outFile -Encoding ASCII

Write-Host "Configuration file: $outFile"

$values = @( @{ "key" = "endpoint"; "value" = "http://localhost:7071"; "enabled" = "true" } )

$environmentFile = @{ "id"="d9a0b2d1-5c39-4671-83fb-e9a1f7f404a1"; "name" = "Eklee.Azure.Functions.Http.Local"; "values" = $values }

Get-AzKeyVaultSecret -VaultName $Name| ForEach-Object {
    $keyName = $_.Name

    if ($keyName.StartsWith("postman-")) {

        $keyVaultKeyName = $keyName.Replace("postman-","")
        Write-Host "Processing $keyVaultKeyName"

        $secret = (Get-AzKeyVaultSecret -VaultName $Name -Name $keyName)

        if (!$secret){
            Write-Host "Unable to find $keyVaultKeyName in $Name"
        } else {
			$text = ConvertSecretToPlainText -Secret $secret.SecretValue
            $environmentFile.values += @{ "key" = $keyVaultKeyName; "value" = $text; "enabled" = "true" }
        }          
    }
}

$environmentFile | ConvertTo-Json | Out-File $SourceRootDir\Tests\Eklee.Azure.Functions.GraphQl.Local.postman_environment.json -Encoding ASCII

$localSettingsFileContent = '{
	"IsEncrypted": false,
	"Values": {
		"AzureWebJobsStorage": "%StorageConnection%",
		"FUNCTIONS_WORKER_RUNTIME": "dotnet"
	},
	"GraphQl": {
		"EnableMetrics": "true",
		"ExposeExceptions":  "true" 
	},
	"DocumentDb": {
		"Url": "%DocumentDbUrl%",
		"Key": "%DocumentDbKey%",
		"RequestUnits": "400"
	},
	"Search": {
		"ServiceName": "%SearchServiceName%",
		"ApiKey": "%SearchServiceKey%"
	},
	"TableStorage": {
		"ConnectionString": "%StorageConnection%"
	},
	"Security": {
		"Audience": "%audienceId%",
		"Issuers": "%issuer1% %issuer2%"
	},
	"Tenants": [
		{
			"Issuer": "%issuer1%",
			"DocumentDb": {
				"Key": "%DocumentDbKey%",
				"Url": "%DocumentDbUrl%",
				"RequestUnits": "400"
			},
			"Search": {
				"ServiceName": "%SearchServiceName%",
				"ApiKey": "%SearchServiceKey%"
			},
			"TableStorage": {
				"ConnectionString": "%StorageConnection%"
			}
		},
		{
			"Issuer": "%issuer2%",
			"DocumentDb": {
				"Key": "%DocumentDbKey%",
				"Url": "%DocumentDbUrl%",
				"RequestUnits": "400"
			},
			"Search": {
				"ServiceName": "%SearchServiceName%",
				"ApiKey": "%SearchServiceKey%"
			},
			"TableStorage": {
				"ConnectionString": "%StorageConnection%"
			}
		}
	]
}'

$audienceId = ConvertSecretToPlainText -Secret (Get-AzKeyVaultSecret -VaultName $Name -Name "local-audienceId").SecretValue
$issuer1 = ConvertSecretToPlainText -Secret (Get-AzKeyVaultSecret -VaultName $Name -Name "local-issuer1").SecretValue
$issuer2 = ConvertSecretToPlainText -Secret (Get-AzKeyVaultSecret -VaultName $Name -Name "local-issuer2").SecretValue

$localSettingsFileContent = $localSettingsFileContent.Replace("%audienceId%", $audienceId)
$localSettingsFileContent = $localSettingsFileContent.Replace("%issuer1%", $issuer1)
$localSettingsFileContent = $localSettingsFileContent.Replace("%issuer2%", $issuer2)
$localSettingsFileContent = $localSettingsFileContent.Replace("%DocumentDbKey%", $primaryMasterKey)
$localSettingsFileContent = $localSettingsFileContent.Replace("%DocumentDbUrl%", $documentDbUrl )
$localSettingsFileContent = $localSettingsFileContent.Replace("%SearchServiceName%", $Name)
$localSettingsFileContent = $localSettingsFileContent.Replace("%SearchServiceKey%", $primaryKey)
$localSettingsFileContent = $localSettingsFileContent.Replace("%StorageConnection%", $connectionString)

$localSettingsFileContent | Out-File $SourceRootDir\Examples\Eklee.Azure.Functions.GraphQl.Example\local.settings.json -Encoding ASCII