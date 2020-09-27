param(
	[Parameter(Mandatory=$True)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$True)][string]$Name,
	[Parameter(Mandatory=$True)][string]$SourceRootDir)

$ErrorActionPreference = "Stop"

$documentDbUrl = "https://localhost:8081"

$primaryMasterKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="

$resource = Get-AzResource `
    -ResourceType "Microsoft.Search/searchServices" `
    -ResourceGroupName $ResourceGroupName `
    -ResourceName $Name `
    -ApiVersion 2015-08-19

# Get the primary admin API key for search
$primaryKey = (Invoke-AzResourceAction `
    -Action listAdminKeys `
    -ResourceId $resource.ResourceId `
    -ApiVersion 2015-08-19 `
	-Force).PrimaryKey

$connectionString = "UseDevelopmentStorage=true"

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
            $environmentFile.values += @{ "key" = $keyVaultKeyName; "value" = $secret.SecretValueText; "enabled" = "true" }
        }
       
        
    }
}

$environmentFile | ConvertTo-Json | Out-File $SourceRootDir\Tests\Eklee.Azure.Functions.GraphQl.Local.postman_environment.json