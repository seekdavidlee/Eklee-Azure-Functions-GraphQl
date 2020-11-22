param(
	[Parameter(Mandatory = $True)][string]$Path,
	[Parameter(Mandatory = $True)][string]$BuildConfig,
	[Parameter(Mandatory = $True)][string]$ReportDir,
	[Parameter(Mandatory = $True)][string]$EnvironmentPath)

$WorkingDirectory = "$Path\Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$BuildConfig\netstandard2.0"
Write-Host "Working Directory $WorkingDirectory"

Push-Location $WorkingDirectory
npm install --save-dev azure-functions-core-tools@3
npm install --save-dev newman
Pop-Location

Get-ChildItem -Path $WorkingDirectory

#Start-Process -WorkingDirectory $WorkingDirectory -FilePath "$WorkingDirectory\node_modules\.bin\func" -ArgumentList @("start") -RedirectStandardOutput output.txt -RedirectStandardError err.txt
& "$WorkingDirectory\node_modules\.bin\func" start --no-build

Start-Sleep -s 10

$reportFilePath = "$ReportDir/report.xml"
Push-Location $Path\Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$BuildConfig\netstandard2.0
node_modules\.bin\newman run ..\..\..\..\..\tests\Eklee.Azure.Functions.GraphQl.postman_collection.json -e "$EnvironmentPath\Tests\Eklee.Azure.Functions.GraphQl.Local.postman_environment.json" --reporters 'cli,junit' --reporter-junit-export $reportFilePath
Pop-Location

