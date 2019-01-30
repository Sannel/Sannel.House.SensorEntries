#!/usr/local/bin/pwsh
param()

. "$PSScriptRoot/_common.ps1"

SetBuildType

$version = GetVersion

return RunDockerCompose "down" $version ""