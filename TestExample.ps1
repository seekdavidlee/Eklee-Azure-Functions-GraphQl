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

$funcExist = [System.IO.File]::Exists("$WorkingDirectory\node_modules\.bin\func")
if (!$funcExist) {
	Write-Host "Func file is missing!"
	return
}

& "$WorkingDirectory\node_modules\.bin\func" host start

Start-Sleep -s 10

$func = Get-Process -Name func

if (!$func) {
	Write-Host "func not found"
	return
}

$reportFilePath = "$ReportDir/report.xml"
Push-Location $Path\Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$BuildConfig\netstandard2.0
node_modules\.bin\newman run ..\..\..\..\..\tests\Eklee.Azure.Functions.GraphQl.postman_collection.json -e "$EnvironmentPath\Tests\Eklee.Azure.Functions.GraphQl.Local.postman_environment.json" --reporters 'cli,junit' --reporter-junit-export $reportFilePath
Pop-Location

