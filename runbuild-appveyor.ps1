$buildNumber = [int]$Env:APPVEYOR_BUILD_NUMBER
write-host "Build number $buildNumber" -fore white
$pullRequestNumber = [int]$Env:APPVEYOR_PULL_REQUEST_NUMBER
write-host "Pull request build number $pullRequestNumber" -fore white
$formattedBuildNumber = $buildNumber.ToString("D6")
$nugetPrerelease = "build$formattedBuildNumber"
Build\runbuild.ps1 -properties @{"nugetPrerelease"=$nugetPrerelease;}