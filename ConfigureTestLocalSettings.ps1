param(
	[Parameter(Mandatory=$True)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$True)][string]$Name,
	[Parameter(Mandatory=$True)][string]$SourceRootDir)

$documentDbUrl = "https://$Name.documents.azure.com/"

$resource = Get-AzResource `
	-ResourceType "Microsoft.DocumentDb/databaseAccounts" `
	-ResourceGroupName $ResourceGroupName `
	-ResourceName $Name `
	-ApiVersion 2015-04-08

$primaryMasterKey = (Invoke-AzResourceAction `
	-Action listKeys `
	-ResourceId $resource.ResourceId `
	-ApiVersion 2015-04-08 `
	-Force).primaryMasterKey

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

$storageAccount = Get-AzStorageAccount `
    -ResourceGroupName $ResourceGroupName `
	-Name $Name

$ctx = $storageAccount.Context

$accountKey = (Get-AzStorageAccountKey -ResourceGroupName $ResourceGroupName -Name $Name).Value[0] 
$connectionString = "DefaultEndpointsProtocol=https;AccountName=$Name;AccountKey=$accountKey;EndpointSuffix=core.windows.net"

$settings = @{ Search = @{ ServiceName="$Name"; ApiKey="$primaryKey" }; DocumentDb = @{ Key="$primaryMasterKey";Url="$documentDbUrl";RequestUnits="400" }; TableStorage=@{ConnectionString="$connectionString"} } | ConvertTo-Json -Depth 10
$settings | Out-File $SourceRootDir\Eklee.Azure.Functions.GraphQl.Tests\local.settings.json -Encoding ASCII