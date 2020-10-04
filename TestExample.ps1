param(
	[Parameter(Mandatory=$True)][string]$Path,
	[Parameter(Mandatory=$True)][string]$BuildConfig,
	[Parameter(Mandatory=$True)][string]$ReportDir)

Push-Location $Path\Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$BuildConfig\netstandard2.0
npm install --save-dev azure-functions-core-tools@3
npm install --save-dev newman
Pop-Location

Start-Process -WorkingDirectory $Path\Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$BuildConfig\netstandard2.0 -FilePath node_modules\.bin\func -ArgumentList "host start"

Start-Sleep -s 10

$func = Get-Process -Name func

$reportFilePath = "$ReportDir/report.xml"
Push-Location $Path\Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$BuildConfig\netstandard2.0
node_modules\.bin\newman run ..\..\..\..\..\tests\Eklee.Azure.Functions.GraphQl.postman_collection.json -e ..\..\..\..\..\tests\Eklee.Azure.Functions.GraphQl.Local.postman_environment.json --reporters cli,junit --reporter-junit-export $reportFilePath
Pop-Location