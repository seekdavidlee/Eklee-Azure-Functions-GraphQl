param(
    [Parameter(Mandatory = $True)][string]$Path)

$app = "Eklee.Azure.Functions.GraphQl"
$filePath = "$Path\$app\bin\Release\netstandard2.0\bin\$app.dll"

if (![System.IO.File]::Exists($filePath)){
    Write-Host "Missing $filePath"
    Get-ChildItem -Path "$Path\$app\bin\Release\netstandard2.0\bin"
}

Move-Item -Path $filePath -Destination "$Path\$app\bin\Release\netstandard2.0\$app.dll"
Remove-Item -Path "$Path\$app\bin\Release\netstandard2.0\bin" -Recurse
Copy-Item "$Path\LICENSE" "$Path\LICENSE.txt"