#!/usr/local/bin/pwsh
param(
)

$target = "devices"
. "$PSScriptRoot/_common.ps1"

SetBuildType
CleanDevFiles

$version = GetVersion
TryLogin
SetDockerComposeVariables
$image="${env:DOCKER_REGISTRY}${imageName}:$buildType-$version-${env:SANNEL_ARCH}"


return docker push $image


