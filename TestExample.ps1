param(
	[Parameter(Mandatory = $True)][string]$Path,
	[Parameter(Mandatory = $True)][string]$BuildConfig,
	[Parameter(Mandatory = $True)][string]$ReportDir,
	[Parameter(Mandatory = $True)][string]$EnvironmentPath,
	[Parameter(Mandatory = $True)][string]$Name,
	[Parameter(Mandatory = $True)][string]$Location)

$WorkingDirectory = "$Path\Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$BuildConfig\netstandard2.0"
Write-Host "Working Directory $WorkingDirectory"
$StackName = ($Name + $env:Build_BuildNumber).Replace(".", "")
$Tags = '"stackname=' + $StackName + '"'

Write-Host "Stackname: $StackName"

Compress-Archive -Path "$WorkingDirectory\*" -DestinationPath "$WorkingDirectory\Deploy.zip"

az extension add -n application-insights

az monitor app-insights component create --app $StackName --location $Location --kind web -g $Name --application-type web --tags $Tags | Out-Null
az storage account create --resource-group $Name --name $StackName --tags $Tags | Out-Null
az functionapp create --consumption-plan-location $Location --name $StackName --os-type Windows --resource-group $Name --runtime dotnet --storage-account $StackName --app-insights $StackName --tags $Tags | Out-Null

$content = (Get-Content -Path "$WorkingDirectory\local.settings.json" | ConvertFrom-Json).Values

$documentUrl = $content.DocumentDb.Url
$documentKey = $content.DocumentDb.Key
$searchName = $content.Search.ServiceName
$searchApiKey = $content.Search.ApiKey

az functionapp config appsettings set -n $StackName -g $StackName --settings "GraphQl:EnableMetrics=true" "GraphQl:ExposeExceptions=true" `
	"DocumentDb:Url=$documentUrl" `
	"DocumentDb:Key=$documentKey" `
	"DocumentDb:RequestUnits=400" `
	"Search:ServiceName=$searchName" `
	"Search:ApiKey=$searchApiKey"

az functionapp deployment source config-zip -g $StackName -n $StackName --src "$WorkingDirectory\Deploy.zip"

Push-Location $WorkingDirectory
#npm install --save-dev azure-functions-core-tools@3
npm install --save-dev newman
Pop-Location

#Get-ChildItem -Path $WorkingDirectory

#Start-Process -WorkingDirectory $WorkingDirectory -FilePath "$WorkingDirectory\node_modules\.bin\func" -ArgumentList @("start","--no-build") -RedirectStandardOutput output.txt -RedirectStandardError err.txt

#Start-Sleep -s 10

#$func = Get-Process -Name func

#if (!$func) {
#	Write-Host "func not found"
#	Get-Content -Path $Path\err.txt
#	return
#}
#else {
#	Write-Host "func is found"
#	Get-Content -Path $Path\output.txt
#	Get-Content -Path $Path\err.txt
#}

#$reportFilePath = "$ReportDir/report.xml"
#Push-Location $Path\Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$BuildConfig\netstandard2.0
#node_modules\.bin\newman run ..\..\..\..\..\tests\Eklee.Azure.Functions.GraphQl.postman_collection.json -e "$EnvironmentPath\Tests\Eklee.Azure.Functions.GraphQl.Local.postman_environment.json" --reporters 'cli,junit' --reporter-junit-export $reportFilePath
#Pop-Location