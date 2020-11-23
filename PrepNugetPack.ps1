param(
    [Parameter(Mandatory = $True)][string]$Path)

$app = "Eklee.Azure.Functions.GraphQl"

Move-Item -Path "$Path\$app\bin\Release\netstandard2.0\bin\$app.dll" -Destination "bin\Release\netstandard2.0\$app.dll"
Remove-Item -Path "$Path\$app\bin\Release\netstandard2.0\bin" -Recurse
Copy-Item "$Path\LICENSE" "$Path\LICENSE.txt"