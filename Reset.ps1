param(
	[Parameter(Mandatory=$True)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$True)][string]$Name,	
	[Parameter(Mandatory=$True)][string]$SubscriptionId)

$context = Get-AzContext
if (!$context) {
	Connect-AzAccount -SubscriptionId $SubscriptionId
}else {
	if ($context.SubscriptionId -ne $SubscriptionId){
		Clear-AzContext -Force
		Connect-AzAccount -SubscriptionId $SubscriptionId
	}
}

.\ResetSearch.ps1 -ResourceGroupName $ResourceGroupName -ServiceName $Name
.\ResetDocumentDb.ps1 -ResourceGroupName $ResourceGroupName -AccountName $Name
.\ResetTableStorage -ResourceGroupName $ResourceGroupName -AccountName $Name