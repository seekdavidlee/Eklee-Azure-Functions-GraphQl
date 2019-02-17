param(
	[Parameter(Mandatory=$True)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$True)][string]$Name,
	[Parameter(Mandatory=$True)][string]$SourceRootDir)

$documentDbUrl = "https://$Name.documents.azure.com/"

$resource = Get-AzureRmResource `
	-ResourceType "Microsoft.DocumentDb/databaseAccounts" `
	-ResourceGroupName $ResourceGroupName `
	-ResourceName $Name `
	-ApiVersion 2015-04-08

$primaryMasterKey = (Invoke-AzureRmResourceAction `
	-Action listKeys `
	-ResourceId $resource.ResourceId `
	-ApiVersion 2015-04-08 `
	-Force).primaryMasterKey

$settings = @{ Search = @{ ServiceName=""; ApiKey="" }; DocumentDb = @{ Key="$primaryMasterKey";Url="$documentDbUrl";RequestUnits="400" }; TableStorage=@{ConnectionString=""} }
Write-Host $settings
$settings | Out-File $SourceRootDir\Eklee.Azure.Functions.GraphQl.Tests\local.settings.json