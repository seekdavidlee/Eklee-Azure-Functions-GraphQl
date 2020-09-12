param([switch]$testunit, [switch]$testint, [switch]$skippackage,
	[switch]$skipfunctest, 
	[Parameter(Mandatory=$False)][string]$ResourceGroupName, 
	[Parameter(Mandatory=$False)][string]$Name,	
	[Parameter(Mandatory=$False)][string]$SubscriptionId,	
	[Parameter(Mandatory=$False)][string]$BuildConfiguration,	
	[Parameter(Mandatory=$False)][string]$IncrementVersionType)

	$ErrorActionPreference = "Stop"
	
	dotnet test .\Eklee.Azure.Functions.GraphQl.Tests\Eklee.Azure.Functions.GraphQl.Tests.csproj --filter Category=Unit
	
	if ($lastexitcode -ne 0){
		return;
	}

	if ($testunit) { 
		return;
	}

	if (!$SubscriptionId) {
		Write-Host "SubscriptionId is required."
		return
	}

	if (!$ResourceGroupName) {
		Write-Host "ResourceGroupName is required."
		return
	}

	if (!$Name) {
		Write-Host "Name is required."
		return
	}

	# Note: We are following: https://semver.org/
	# Given a version number MAJOR.MINOR.PATCH, increment the:

	# MAJOR version when you make incompatible API changes,
	# MINOR version when you add functionality in a backwards-compatible manner, and
	# PATCH version when you make backwards-compatible bug fixes.

	$versions = (Get-Content version.txt).Split(".")
	$major = [int]$versions[0]
	$minor = [int]$versions[1]
	$patch = [int]$versions[2]
	
	if (!$BuildConfiguration){
		$buildConfig = "Release"
		
		if (!$IncrementVersionType) {
			# Assume we are doing a minor release
			$minor = $minor + 1
		} else {
			if ($IncrementVersionType.ToLower() -eq "major") {
				$major = $major + 1
				$minor = 0
				$patch = 0
			}

			if ($IncrementVersionType.ToLower() -eq "minor") {
				$minor = $minor + 1
				$patch = 0
			}

			if ($IncrementVersionType.ToLower() -eq "patch") {
				$patch = $patch + 1
			}
		}

	} else {
		$buildConfig = $BuildConfiguration

		if (!$IncrementVersionType){
			# Assume we are doing a patch release
			$patch = $patch + 1
		} else {
			if ($IncrementVersionType.ToLower() -eq "major") {
				$major = $major + 1
				$minor = 0
				$patch = 0
			}

			if ($IncrementVersionType.ToLower() -eq "minor") {
				$minor = $minor + 1
				$patch = 0
			}

			if ($IncrementVersionType.ToLower() -eq "patch") {
				$patch = $patch + 1
			}		
		}
	}
	
	Write-Host "BuildConfiguration = $buildConfig"
	$version = "$major.$minor.$patch"

	Write-Host "Version = $version"

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
		Push-Location Examples\Eklee.Azure.Functions.GraphQl.Example
		dotnet build --configuration=$buildConfig -p:Version=$version
		Pop-Location

		Push-Location Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$buildConfig\netstandard2.0
		npm install --save-dev azure-functions-core-tools@3
		npm install --save-dev newman
		Pop-Location

		Start-Process -WorkingDirectory Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$buildConfig\netstandard2.0 -FilePath node_modules\.bin\func -ArgumentList "host start"

		Start-Sleep -s 10

		$func = Get-Process -Name func

		$reportFileName = (Get-Date).ToString("yyyyMMddHHmmss") + ".json"
		$reportFilePath = "$currentDir\.reports\$reportFileName"
		New-Item -ItemType Directory -Force -Path "$currentDir\.reports"

		Push-Location Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$buildConfig\netstandard2.0
		node_modules\.bin\newman run ..\..\..\..\..\tests\Eklee.Azure.Functions.GraphQl.postman_collection.json -e ..\..\..\..\..\tests\Eklee.Azure.Functions.GraphQl.Local.postman_environment.json --reporters cli,json --reporter-json-export $reportFilePath
		Pop-Location

		$report = (Get-Content $reportFilePath | Out-String | ConvertFrom-Json)	

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

			Push-Location .\$app
			dotnet clean --configuration $buildConfig
			dotnet build --configuration $buildConfig -p:Version=$version
			Move-Item -Path bin\$buildConfig\netstandard2.0\bin\$app.dll -Destination bin\$buildConfig\netstandard2.0\$app.dll
			Remove-Item -Path bin\$buildConfig\netstandard2.0\bin -Recurse
			Pop-Location
			Remove-Item $currentDir\*.nupkg
			Copy-Item $currentDir\LICENSE $currentDir\LICENSE.txt
			nuget.exe pack $app\$app.csproj -Properties Configuration=$buildConfig -IncludeReferencedProjects -Version $version
			Set-Content -Path .\version.txt -Value $version	#Update new version number
			Remove-Item $currentDir\LICENSE.txt

			if ($reportFileName) {
				Remove-Item $reportFilePath
			}
		}
	}
	.\Reset.ps1 -ResourceGroupName $ResourceGroupName -Name $Name -SubscriptionId $SubscriptionId