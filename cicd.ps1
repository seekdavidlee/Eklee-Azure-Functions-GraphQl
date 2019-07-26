param([switch]$testunit, [switch]$testint, [switch]$skippackage,
	[switch]$skipfunctest, 
	[Parameter(Mandatory=$False)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$False)][string]$Name,	
	[Parameter(Mandatory=$False)][string]$SubscriptionId,	
	[Parameter(Mandatory=$False)][string]$BuildConfiguration)

	$ErrorActionPreference = "Stop"

	if (!$BuildConfiguration){
		$buildConfig = "Release"		
	} else {
		$buildConfig = $BuildConfiguration
	}
	
	Write-Host "BuildConfiguration = $buildConfig"
	
	dotnet test .\Eklee.Azure.Functions.GraphQl.Tests\Eklee.Azure.Functions.GraphQl.Tests.csproj --filter Category=Unit
	
	if ($lastexitcode -ne 0){
		return;
	}

	if ($testunit) { 
		return;
	}

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
	.\ConfigureTestLocalSettings.ps1 -SourceRootDir (Get-Location).Path -ResourceGroupName $ResourceGroupName -Name $Name
	dotnet test .\Eklee.Azure.Functions.GraphQl.Tests\Eklee.Azure.Functions.GraphQl.Tests.csproj --filter Category=Integration
	
	if ($lastexitcode -ne 0){
		return;
	}

	if ($testint) {
		.\Reset.ps1 -ResourceGroupName $ResourceGroupName -Name $Name -SubscriptionId $SubscriptionId
		return;
	}

	.\Reset.ps1 -ResourceGroupName $ResourceGroupName -Name $Name -SubscriptionId $SubscriptionId

	$app = "Eklee.Azure.Functions.GraphQl"	

	$currentDir = Get-Location

	if (!$skipfunctest) {
		pushd Examples\Eklee.Azure.Functions.GraphQl.Example
		dotnet build --configuration=$buildConfig
		popd

		pushd Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$buildConfig\netstandard2.0
		npm install --save-dev azure-functions-core-tools
		npm install --save-dev newman
		popd

		Start-Process -WorkingDirectory Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$buildConfig\netstandard2.0 -FilePath node_modules\.bin\func -ArgumentList "host start"

		Start-Sleep -s 10

		$func = Get-Process -Name func

		$reportFileName = (Get-Date).ToString("yyyyMMddHHmmss") + ".json"

		pushd Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$buildConfig\netstandard2.0
		node_modules\.bin\newman run ..\..\..\..\..\tests\Eklee.Azure.Functions.GraphQl.postman_collection.json -e ..\..\..\..\..\tests\Eklee.Azure.Functions.GraphQl.Local.postman_environment.json --reporters cli,json --reporter-json-export "$currentDir\$reportFileName"
		popd

		$report = (Get-Content "$currentDir\$reportFileName" | Out-String | ConvertFrom-Json)	

		$failures = $report.run.failures.length
		Write-Host "Failures: $failures"
	} else {
		# Override.
		$failures = 0
	}
	
	if ($failures -gt 0) {
		Write-Host "Failed!" -ForegroundColor red
	} else {
		
		if ($func) {
			Write-Host "Killing job"
			Stop-Process $func
		}

		Write-Host "All good!" -ForegroundColor green

		if ($skippackage){
			Write-Host "Skip building nuget package..."
		} else {
			Write-Host "Generating nuget package"

			pushd .\$app
			dotnet clean --configuration $buildConfig
			dotnet build --configuration $buildConfig
			Move-Item -Path bin\$buildConfig\netstandard2.0\bin\$app.dll -Destination bin\$buildConfig\netstandard2.0\$app.dll
			Remove-Item -Path bin\$buildConfig\netstandard2.0\bin -Recurse
			popd
			Remove-Item $currentDir\*.nupkg
			Copy-Item $currentDir\LICENSE $currentDir\LICENSE.txt
			nuget.exe pack $app\$app.csproj -Properties Configuration=$buildConfig -IncludeReferencedProjects
			Remove-Item $currentDir\LICENSE.txt

			if ($reportFileName) {
				Remove-Item $currentDir\$reportFileName
			}
		}
	}
	.\Reset.ps1 -ResourceGroupName $ResourceGroupName -Name $Name -SubscriptionId $SubscriptionId
	