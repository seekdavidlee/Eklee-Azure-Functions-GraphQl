param(
	[Parameter(Mandatory = $True)][string]$Name)

$StackName = ($Name + $env:Build_BuildNumber).Replace(".", "")

$resources = az resource list --tag stackName=$StackName | ConvertFrom-Json

$funcId = ($resources | Where-Object { $_.type -eq "Microsoft.Web/sites" }).id

az resource delete --ids $funcId

$resources | Where-Object { $_.type -ne "Microsoft.Web/sites" } | ForEach-Object { az resource delete --ids $_.id }