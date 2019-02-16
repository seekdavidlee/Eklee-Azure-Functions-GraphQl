param([Parameter(Mandatory=$True)][string]$SubscriptionId, [switch]$ForceLogin)

function DoLogin($workingDirUserProfile){
	Write-Host "Connecting to Azure Account..."
	Connect-AzureRmAccount -ErrorAction Stop
	Save-AzureRmContext -Path $workingDirUserProfile -Force
}

$userProfilePath = $env:USERPROFILE + "\.azuretool"
$workingDirUserProfile = "$userProfilePath\userprofile.json"

if (Test-Path $workingDirUserProfile) {

	if ($ForceLogin) {
		Write-Host "Forcing login"
		DoLogin -workingDirUserProfile $workingDirUserProfile
	}else {
		Write-Host "Importing from $workingDirUserProfile"
		Import-AzureRmContext -Path $workingDirUserProfile	
	}
} else {
	Write-Host "Creating directory path $userProfilePath"
	$dir = New-Item -Force -ItemType directory -Path $userProfilePath
	$dir.attributes="Hidden"
	DoLogin -workingDirUserProfile $workingDirUserProfile
}

$list = Get-AzureRmSubscription | Where-Object { $_.Id -eq $SubscriptionId }
if ($list.Length -eq 0) {
	throw "Please provide a valid subscription."
}

try {
	Select-AzureRmSubscription -SubscriptionId $SubscriptionId -ErrorAction Stop
	Write-Host "You are logged in."	
}
catch {	
	if ($_.Exception.Message -contains "Please provide a valid tenant or a valid subscription.") {
		Write-Host "Login has expired."
		DoLogin -workingDirUserProfile $workingDirUserProfile
		Select-AzureRmSubscription -SubscriptionId $SubscriptionId -ErrorAction Stop
	}
}