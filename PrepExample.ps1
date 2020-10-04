param(
	[Parameter(Mandatory=$True)][string]$Path,
	[Parameter(Mandatory=$True)][string]$BuildConfig)

Push-Location $Path\Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$BuildConfig\netstandard2.0
npm install --save-dev azure-functions-core-tools@3
npm install --save-dev newman
Pop-Location

Start-Process -WorkingDirectory $Path\Examples\Eklee.Azure.Functions.GraphQl.Example\bin\$BuildConfig\netstandard2.0 -FilePath node_modules\.bin\func -ArgumentList "host start"

Start-Sleep -s 10

$func = Get-Process -Name func