param(
	[Parameter(Mandatory=$True)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$True)][string]$Name,	
	[Parameter(Mandatory=$True)][string]$SubscriptionId)

.\Login.ps1 -SubscriptionId $SubscriptionId
.\ResetSearch.ps1 -ResourceGroupName $ResourceGroupName -ServiceName $Name
.\ResetDocumentDb.ps1 -ResourceGroupName $ResourceGroupName -AccountName $Name
.\ResetTableStorage -ResourceGroupName $ResourceGroupName -AccountName $Name