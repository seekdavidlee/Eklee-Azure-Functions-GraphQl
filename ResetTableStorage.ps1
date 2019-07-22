param(
	[Parameter(Mandatory=$True)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$True)][string]$AccountName)

$storageAccount = Get-AzStorageAccount `
    -ResourceGroupName $ResourceGroupName `
	-Name $AccountName

$ctx = $storageAccount.Context
$names = Get-AzStorageTable –Context $ctx | select Name

Write-Host "Removing tables..."

$names | foreach {
	$name = $_.Name
	Write-Host "Removing $name"
	Remove-AzStorageTable -Name $name –Context $ctx -Force
}