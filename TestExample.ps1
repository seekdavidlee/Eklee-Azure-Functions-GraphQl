param(
	[Parameter(Mandatory = $True)][string]$Path,
	[Parameter(Mandatory = $True)][string]$BuildConfig,
	[Parameter(Mandatory = $True)][string]$ReportDir,
	[Parameter(Mandatory = $True)][string]$EnvironmentPath,
	[Parameter(Mandatory = $True)][string]$Name,
	[Parameter(Mandatory = $True)][string]$Location)

$WorkingDirectory = "$Path\Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$BuildConfig\netstandard2.0"

$StackName = ($Name + $env:Build_BuildNumber).Replace(".", "")

Compress-Archive -Path "$WorkingDirectory\*" -DestinationPath "$WorkingDirectory\Deploy.zip"

az extension add -n application-insights

az deployment group create `
	--name $StackName `
	--resource-group $Name `
	--template-file Templates/app.json `
	--parameters plan_name=$StackName location=$Location 

$content = Get-Content -Path "$Path\Examples\Eklee.Azure.Functions.GraphQl.Example\local.settings.json" | ConvertFrom-Json

$documentUrl = $content.DocumentDb.Url
$documentKey = $content.DocumentDb.Key
$searchName = $content.Search.ServiceName
$searchApiKey = $content.Search.ApiKey
$tableStorageConnectionString = $content.TableStorage.ConnectionString
$audience = $content.Security.Audience
$issuers = $content.Security.Issuers
$issuer1 = $content.Tenants[0].Issuer
$issuer2 = $content.Tenants[1].Issuer

az functionapp config appsettings set -n $StackName -g $Name --settings "GraphQl:EnableMetrics=true" "GraphQl:ExposeExceptions=true" `
	"DocumentDb:Url=$documentUrl" `
	"DocumentDb:Key=$documentKey" `
	"DocumentDb:RequestUnits=400" `
	"Search:ServiceName=$searchName" `
	"Search:ApiKey=$searchApiKey" `
	"TableStorage:ConnectionString=$tableStorageConnectionString" `
	"Security:Audience=$audience" `
	"Security:Issuers=$issuers" `
	"Tenants:0:Issuer=$issuer1" `
	"Tenants:0:DocumentDb:Url=$documentUrl" `
	"Tenants:0:DocumentDb:Key=$documentKey" `
	"Tenants:0:DocumentDb:RequestUnits=400" `
	"Tenants:0:Search:ServiceName=$searchName" `
	"Tenants:0:Search:ApiKey=$searchApiKey" `
	"Tenants:0:TableStorage:ConnectionString=$tableStorageConnectionString" `
	"Tenants:1:Issuer=$issuer2" `
	"Tenants:1:DocumentDb:Url=$documentUrl" `
	"Tenants:1:DocumentDb:Key=$documentKey" `
	"Tenants:1:DocumentDb:RequestUnits=400" `
	"Tenants:1:Search:ServiceName=$searchName" `
	"Tenants:1:Search:ApiKey=$searchApiKey" `
	"Tenants:1:TableStorage:ConnectionString=$tableStorageConnectionString" | Out-Null

az functionapp deployment source config-zip -g $Name -n $StackName --src "$WorkingDirectory\Deploy.zip"

Push-Location $WorkingDirectory
npm install --save-dev newman
Pop-Location

$content = (Get-Content -Path "$Path\Tests\Eklee.Azure.Functions.GraphQl.Local.postman_environment.json").Replace("http://localhost:7071", "https://$StackName.azurewebsites.net")
$content | Out-File "$Path\Tests\Eklee.Azure.Functions.GraphQl.Local.postman_environment.json" -Encoding ASCII

$reportFilePath = "$ReportDir/report.xml"
Push-Location $Path\Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$BuildConfig\netstandard2.0
node_modules\.bin\newman run ..\..\..\..\..\tests\Eklee.Azure.Functions.GraphQl.postman_collection.json -e "$EnvironmentPath\Tests\Eklee.Azure.Functions.GraphQl.Local.postman_environment.json" --reporters 'cli,junit' --reporter-junit-export $reportFilePath
Pop-Location