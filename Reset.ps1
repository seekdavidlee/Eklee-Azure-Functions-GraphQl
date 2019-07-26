param(
	[Parameter(Mandatory=$True)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$True)][string]$Name,	
	[Parameter(Mandatory=$True)][string]$SubscriptionId)

$context = Get-AzContext
if (!$context) {
	Write-Host "No context found."
	Connect-AzAccount -SubscriptionId $SubscriptionId -ErrorAction Stop
}else {
	$subscriptions = Get-AzSubscription -WarningAction SilentlyContinue -WarningVariable status
	if ($status) {
		Write-Host "Your login has expired."
		Connect-AzAccount -SubscriptionId $SubscriptionId -ErrorAction Stop
	} else {
		$subscription = $subscriptions | Where { $_.Id -eq $SubscriptionId }
		if (!$subscription){
			Write-Host "No subscriptions match Id $SubscriptionId ."
			Clear-AzContext -Force
			Connect-AzAccount -SubscriptionId $SubscriptionId -ErrorAction Stop			
		}
	}
}

.\ResetSearch.ps1 -ResourceGroupName $ResourceGroupName -ServiceName $Name
.\ResetDocumentDb.ps1 -ResourceGroupName $ResourceGroupName -AccountName $Name
.\ResetTableStorage -ResourceGroupName $ResourceGroupName -AccountName $Name