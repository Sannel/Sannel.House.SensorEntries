param()

. "$PSScriptRoot/_common.ps1"

$version = GetVersion

$buildType = SetBuildType
$imageName = GetImageName

$data = ". `"`$PSScriptRoot/_common.ps1`"`n"
$data = $data + "`$image=`"${env:DOCKER_REGISTRY}${imageName}:$buildType-$version-`"`n";
$data = $data + "TryLogin`n";
$data = $data + "`$image= `$image + (GetArchString)`n";
$data = $data + "return docker push `$image";

$data | Out-File "$PSScriptRoot/push-image.ps1" -Encoding utf8 -Force