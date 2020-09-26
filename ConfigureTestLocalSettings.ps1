param(
	[Parameter(Mandatory=$True)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$True)][string]$Name,
	[Parameter(Mandatory=$True)][string]$SourceRootDir)

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

$settings = @{ Search = @{ ServiceName="$Name"; ApiKey="$primaryKey" }; DocumentDb = @{ Key="$primaryMasterKey";Url="$documentDbUrl";RequestUnits="400" }; TableStorage=@{ConnectionString="$connectionString"} } | ConvertTo-Json -Depth 10
$settings | Out-File $SourceRootDir\Eklee.Azure.Functions.GraphQl.Tests\local.settings.json -Encoding ASCII