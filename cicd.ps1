param([switch]$testunit, [switch]$testint, [switch]$skippackage, 
	[Parameter(Mandatory=$False)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$False)][string]$Name,	
	[Parameter(Mandatory=$False)][string]$SubscriptionId)

	$ErrorActionPreference = "Stop"

	$buildConfig = "Release"

	dotnet test .\Eklee.Azure.Functions.GraphQl.Tests\Eklee.Azure.Functions.GraphQl.Tests.csproj --filter Category=Unit
	
	if ($lastexitcode -ne 0){
		return;
	}

	if ($testunit) { 
		return;
	}
	.\Login.ps1 -SubscriptionId $SubscriptionId
	.\ConfigureTestLocalSettings.ps1 -SourceRootDir (Get-Location).Path -ResourceGroupName $ResourceGroupName -Name $Name
	dotnet test .\Eklee.Azure.Functions.GraphQl.Tests\Eklee.Azure.Functions.GraphQl.Tests.csproj --filter Category=Integration
	
	if ($lastexitcode -ne 0){
		return;
	}

	if ($testint) { 
		return;
	}

	$app = "Eklee.Azure.Functions.GraphQl"	

	$currentDir = Get-Location

	pushd Examples\Eklee.Azure.Functions.GraphQl.Example
	dotnet build --configuration=$buildConfig
	popd

	pushd Examples\Eklee.Azure.Functions.GraphQl.Example\bin\Release\netstandard2.0
	npm install --save-dev azure-functions-core-tools
	npm install --save-dev newman
	popd
	$hostJob = Start-Job -ScriptBlock {
		param([string]$currentDir)
		Write-Host $currentDir
		cd $currentDir
		Get-Location
		pushd Examples\Eklee.Azure.Functions.GraphQl.Example\bin\Release\netstandard2.0
		node_modules\.bin\func host start
	} -ArgumentList $currentDir
	
	Start-Sleep -s 10

	Receive-Job -Name $hostJob.Name

	Write-Host "C= $currentDir"

	$reportFileName = (Get-Date).ToString("yyyyMMddHHmmss") + ".json"

	pushd Examples\Eklee.Azure.Functions.GraphQl.Example\bin\Release\netstandard2.0
	node_modules\.bin\newman run ..\..\..\..\..\tests\Eklee.Azure.Functions.GraphQl.postman_collection.json -e ..\..\..\..\..\tests\Eklee.Azure.Functions.GraphQl.Local.postman_environment.json --reporters cli,json --reporter-json-export "$currentDir\$reportFileName"
	popd

	$report = (Get-Content "$currentDir\$reportFileName" | Out-String | ConvertFrom-Json)	

	Write-Host "Stopping Jobs"

	Stop-Job $hostJob

	$failures = $report.run.failures.length
	Write-Host "Failures: $failures"
	if ($failures -gt 0) {
		Write-Host "Failed!" -ForegroundColor red
	} else {
		Write-Host "All good!" -ForegroundColor green

		if ($skippackage){
			Write-Host "Skip building nuget package..."
		} else {
			Write-Host "Generating nuget package"

			pushd .\$app
			dotnet clean --configuration $buildConfig
			dotnet build --configuration $buildConfig
			Move-Item -Path bin\Release\netstandard2.0\bin\$app.dll -Destination bin\Release\netstandard2.0\$app.dll
			Remove-Item -Path bin\Release\netstandard2.0\bin -Recurse
			popd
			Remove-Item $currentDir\*.nupkg
			Copy-Item $currentDir\LICENSE $currentDir\LICENSE.txt
			nuget.exe pack $app\$app.csproj -Properties Configuration=$buildConfig -IncludeReferencedProjects
			Remove-Item $currentDir\LICENSE.txt
			Remove-Item $currentDir\$reportFileName
		}
	}
	.\Reset.ps1 -ResourceGroupName $ResourceGroupName -Name $Name -SubscriptionId $SubscriptionId
	