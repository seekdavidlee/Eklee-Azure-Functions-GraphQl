param(
    [Parameter(Mandatory = $True)][string]$BuildId)

# Note: We are following: https://semver.org/
# Given a version number MAJOR.MINOR.PATCH, increment the:

# MAJOR version when you make incompatible API changes,
# MINOR version when you add functionality in a backwards-compatible manner OR bugfixes

$versions = (Get-Content version.txt).Split(".")
$major = [int]$versions[0]
$minor = [int]$versions[1]
$patch = [int]($BuildId.Split(".")[1])

$version = "$major.$minor.$patch"

Write-Host "##vso[task.setvariable variable=buildConfig;isOutput=true]Release"
Write-Host "##vso[task.setvariable variable=version;isOutput=true]$version"