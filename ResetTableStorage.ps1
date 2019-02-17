param(
	[Parameter(Mandatory=$True)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$True)][string]$AccountName)

$storageAccount = Get-AzureRmStorageAccount `
    -ResourceGroupName $ResourceGroupName `
	-Name $AccountName

$ctx = $storageAccount.Context
$names = Get-AzureStorageTable –Context $ctx | select Name

Write-Host "Removing tables..."

$names | foreach {
	$name = $_.Name
	Write-Host "Removing $name"
	Remove-AzureStorageTable -Name $name –Context $ctx -Force
}