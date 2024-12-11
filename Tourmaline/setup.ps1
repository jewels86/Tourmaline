$CurrentPath = (Get-Location).Path
$env:Path += ";$CurrentPath"
[Environment]::SetEnvironmentVariable("Path", $env:Path, [EnvironmentVariableTarget]::Machine)
