param(
	[Parameter(Mandatory=$True)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$True)][string]$AccountName)

Add-Type -AssemblyName System.Web
 
# generate authorization key
Function Generate-MasterKeyAuthorizationSignature
{
[CmdletBinding()]
Param
(
[Parameter(Mandatory=$true)][String]$verb,
[Parameter(Mandatory=$true)][String]$resourceLink,
[Parameter(Mandatory=$true)][String]$resourceType,
[Parameter(Mandatory=$true)][String]$dateTime,
[Parameter(Mandatory=$true)][String]$key,
[Parameter(Mandatory=$true)][String]$keyType,
[Parameter(Mandatory=$true)][String]$tokenVersion
)

If ($resourceLink -eq $resourceType) { 
        $resourceLink = "" 
}

$hmacSha256 = New-Object System.Security.Cryptography.HMACSHA256
$hmacSha256.Key = [System.Convert]::FromBase64String($key)
 
$payLoad = "$($verb.ToLowerInvariant())`n$($resourceType.ToLowerInvariant())`n$resourceLink`n$($dateTime.ToLowerInvariant())`n`n"
$hashPayLoad = $hmacSha256.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($payLoad))
$signature = [System.Convert]::ToBase64String($hashPayLoad);
 
[System.Web.HttpUtility]::UrlEncode("type=$keyType&ver=$tokenVersion&sig=$signature")
}

$resource = Get-AzResource `
    -ResourceType "Microsoft.DocumentDb/databaseAccounts" `
    -ResourceGroupName $ResourceGroupName `
    -ResourceName $AccountName `
    -ApiVersion 2015-04-08

$primaryMasterKey = (Invoke-AzResourceAction `
    -Action listKeys `
    -ResourceId $resource.ResourceId `
    -ApiVersion 2015-04-08 `
	-Force).primaryMasterKey

$Verb = "GET"
$ResourceType = "dbs";
$ResourceLink = "dbs"

$dateTime = [DateTime]::UtcNow.ToString("r")
$authHeader = Generate-MasterKeyAuthorizationSignature -verb $Verb -resourceLink $ResourceLink -resourceType $ResourceType -key $primaryMasterKey -keyType "master" -tokenVersion "1.0" -dateTime $dateTime
$headers = @{authorization=$authHeader;"x-ms-version"="2017-02-22";"x-ms-documentdb-isquery"="True";"x-ms-date"=$dateTime}
$contentType= "application/query+json"
$url = "https://$AccountName.documents.azure.com/$ResourceLink"

$response = Invoke-WebRequest -Method $Verb -Uri $url -ContentType $contentType -Headers $headers | ConvertFrom-Json
$ids = $response.Databases | select -Property id

Write-Host "Removing document databases..."

$ids | foreach {
	$id = $_.id

	Write-Host "Deleting $id"

	$Verb = "DELETE"
	$ResourceType = "dbs";
	$ResourceLink = "dbs/$id"

	$dateTime = [DateTime]::UtcNow.ToString("r")
	$authHeader = Generate-MasterKeyAuthorizationSignature -verb $Verb -resourceLink $ResourceLink -resourceType $ResourceType -key $primaryMasterKey -keyType "master" -tokenVersion "1.0" -dateTime $dateTime
	$headers = @{authorization=$authHeader;"x-ms-version"="2015-08-06";"x-ms-date"=$dateTime}
	$url = "https://$AccountName.documents.azure.com/$ResourceLink"

	$response = Invoke-WebRequest -Method $Verb -Uri $url -Headers $headers
	$response
}
