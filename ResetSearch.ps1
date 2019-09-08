param(
	[Parameter(Mandatory=$True)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$True)][string]$ServiceName)

$resource = Get-AzResource `
    -ResourceType "Microsoft.Search/searchServices" `
    -ResourceGroupName $ResourceGroupName `
    -ResourceName $ServiceName `
    -ApiVersion 2015-08-19

# Get the primary admin API key
$primaryKey = (Invoke-AzResourceAction `
    -Action listAdminKeys `
    -ResourceId $resource.ResourceId `
    -ApiVersion 2015-08-19 `
	-Force).PrimaryKey

$headers = @{ "api-key" = $primaryKey}

$url = "https://$ServiceName.search.windows.net/indexes?api-version=2019-05-06"

$response = Invoke-WebRequest -Method GET -Uri $url -ContentType "application/json" -Headers $headers | ConvertFrom-Json

$names = $response.value | select -Property name

Write-Host "Removing searches..."

$names | foreach {
	$name = $_.name
	Write-Host "Deleting $name"
	$url = "https://$ServiceName.search.windows.net/indexes/" + $name + "?api-version=2017-11-11"
	Write-Host "Invoking $url"
	Invoke-WebRequest -Method DELETE -Uri $url -Headers $headers
}
