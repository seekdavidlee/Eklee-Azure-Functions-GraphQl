param(
	[Parameter(Mandatory = $True)][string]$ResourceGroupName, 
	[Parameter(Mandatory = $True)][string]$Name)

$ErrorActionPreference = "Stop"

function ResetSearch {
 param(
		[Parameter(Mandatory = $True)][string]$ResourceGroupName, 
		[Parameter(Mandatory = $True)][string]$ServiceName)

	Write-Host "Resetting Search"

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

	$headers = @{ "api-key" = $primaryKey }

	$url = "https://$ServiceName.search.windows.net/indexes?api-version=2019-05-06"

	$response = Invoke-WebRequest -Method GET -Uri $url -ContentType "application/json" -Headers $headers | ConvertFrom-Json

	$names = $response.value | Select-Object -Property name

	Write-Host "Removing searches..."

	$names | ForEach-Object {
		$name = $_.name
		Write-Host "Deleting $name"
		$url = "https://$ServiceName.search.windows.net/indexes/" + $name + "?api-version=2017-11-11"
		Write-Host "Invoking $url"
		Invoke-WebRequest -Method DELETE -Uri $url -Headers $headers
	}
}

function GenerateMasterKeyAuthorizationSignature {
	[CmdletBinding()]
	Param
	(
		[Parameter(Mandatory = $true)][String]$verb,
		[Parameter(Mandatory = $true)][String]$resourceLink,
		[Parameter(Mandatory = $true)][String]$resourceType,
		[Parameter(Mandatory = $true)][String]$dateTime,
		[Parameter(Mandatory = $true)][String]$key,
		[Parameter(Mandatory = $true)][String]$keyType,
		[Parameter(Mandatory = $true)][String]$tokenVersion
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

function ResetDocumentDb {

	Write-Host "Resetting Document DB"

	Add-Type -AssemblyName System.Web
 
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
	$authHeader = GenerateMasterKeyAuthorizationSignature -verb $Verb -resourceLink $ResourceLink -resourceType $ResourceType -key $primaryMasterKey -keyType "master" -tokenVersion "1.0" -dateTime $dateTime
	$headers = @{authorization = $authHeader; "x-ms-version" = "2017-02-22"; "x-ms-documentdb-isquery" = "True"; "x-ms-date" = $dateTime }
	$contentType = "application/query+json"
	$url = "https://$AccountName.documents.azure.com/$ResourceLink"
	
	$response = Invoke-WebRequest -Method $Verb -Uri $url -ContentType $contentType -Headers $headers | ConvertFrom-Json
	$ids = $response.Databases | Select-Object -Property id
	
	Write-Host "Removing document databases..."
	
	$ids | ForEach-Object {
		$id = $_.id
	
		Write-Host "Deleting $id"
	
		$Verb = "DELETE"
		$ResourceType = "dbs";
		$ResourceLink = "dbs/$id"
	
		$dateTime = [DateTime]::UtcNow.ToString("r")
		$authHeader = Generate-MasterKeyAuthorizationSignature -verb $Verb -resourceLink $ResourceLink -resourceType $ResourceType -key $primaryMasterKey -keyType "master" -tokenVersion "1.0" -dateTime $dateTime
		$headers = @{authorization = $authHeader; "x-ms-version" = "2015-08-06"; "x-ms-date" = $dateTime }
		$url = "https://$AccountName.documents.azure.com/$ResourceLink"
	
		$response = Invoke-WebRequest -Method $Verb -Uri $url -Headers $headers
		$response
	}
}

function ResetTableStorage {
	param(
		[Parameter(Mandatory = $True)][string]$ResourceGroupName, 
		[Parameter(Mandatory = $True)][string]$AccountName)

	Write-Host "Resetting Table Storage"
	
	$storageAccount = Get-AzStorageAccount `
		-ResourceGroupName $ResourceGroupName `
		-Name $AccountName
	
	$ctx = $storageAccount.Context
	$names = Get-AzStorageTable -Context $ctx | Select-Object Name
	
	Write-Host "Removing tables..."
	
	$names | ForEach-Object {
		$name = $_.Name
		Write-Host "Removing $name"
		Remove-AzStorageTable -Name $name -Context $ctx -Force
	}
}

ResetSearch -ResourceGroupName $ResourceGroupName -ServiceName $Name
ResetDocumentDb -ResourceGroupName $ResourceGroupName -AccountName $Name
ResetTableStorage -ResourceGroupName $ResourceGroupName -AccountName $Name