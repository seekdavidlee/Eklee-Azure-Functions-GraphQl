param(
    [Parameter(Mandatory = $False)][string]$BuildConfiguration,	
    [Parameter(Mandatory = $False)][string]$IncrementVersionType)

# Note: We are following: https://semver.org/
# Given a version number MAJOR.MINOR.PATCH, increment the:

# MAJOR version when you make incompatible API changes,
# MINOR version when you add functionality in a backwards-compatible manner, and
# PATCH version when you make backwards-compatible bug fixes.

$versions = (Get-Content version.txt).Split(".")
$major = [int]$versions[0]
$minor = [int]$versions[1]
$patch = [int]$versions[2]
	
if (!$BuildConfiguration) {
    $buildConfig = "Release"
		
    if (!$IncrementVersionType) {
        # Assume we are doing a minor release
        $minor = $minor + 1
    }
    else {
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
else {
    $buildConfig = $BuildConfiguration

    if (!$IncrementVersionType) {
        # Assume we are doing a patch release
        $patch = $patch + 1
    }
    else {
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

$version = "$major.$minor.$patch"

Write-Host "##vso[task.setvariable variable=buildConfig;isOutput=true]$buildConfig"
Write-Host "##vso[task.setvariable variable=version;isOutput=true]$version"