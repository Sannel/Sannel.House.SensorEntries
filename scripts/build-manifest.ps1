param()

. "$PSScriptRoot/_common.ps1"

$version = GetVersion

$buildType = SetBuildType
$imageName = GetImageName

$data = ". `"`$PSScriptRoot/_common.ps1`"`n";
$data = $data + "`$combinedTag=`"${env:DOCKER_REGISTRY}${imageName}:$buildType-$version`"`n";
$data = $data + "`$arm=`"${env:DOCKER_REGISTRY}${imageName}:$buildType-$version-linux-arm32`"`n";
$data = $data + "`$x64=`"${env:DOCKER_REGISTRY}${imageName}:$buildType-$version-linux-x64`"`n";
$data = $data + "`$win=`"${env:DOCKER_REGISTRY}${imageName}:$buildType-$version-win-x64`"`n";
$data = $data + "TryLogin`n";
$data = $data + "docker manifest create `$combinedTag `$arm `$x64 `$win`n";
$data = $data + "docker manifest push `$combinedTag`n";
$data = $data + "`$combinedTag=`"${env:DOCKER_REGISTRY}${imageName}:$buildType`"`n";
$data = $data + "docker manifest create `$combinedTag `$arm `$x64 `$win`n";
$data = $data + "docker manifest push `$combinedTag`n";

if($buildType -eq "release")
{
	$data = $data + "`$combinedTag=`"${env:DOCKER_REGISTRY}${imageName}:latest`"`n";
	$data = $data + "docker manifest create `$combinedTag `$arm `$x64 `$win`n";
	$data = $data + "docker manifest push `$combinedTag`n";
}
$data | Out-File "$PSScriptRoot/push-manifest.ps1" -Encoding utf8 -Force