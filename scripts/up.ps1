#!/usr/local/bin/pwsh
param(
	[switch]$DevicesOnly,
	[Switch]$Users
)

. "$PSScriptRoot/_common.ps1"

SetBuildType

$target = "";
if($DevicesOnly)
{
	$target = "devices";
}
elseif($Users)
{
	$target = "users";
}

$version = GetVersion

return RunDockerCompose "up" $version $target