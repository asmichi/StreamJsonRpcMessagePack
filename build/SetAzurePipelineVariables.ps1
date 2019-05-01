
# Copyright (c) @asmichi (https://github.com/asmichi). Licensed under the MIT License. See LICENCE in the project root for details.

Set-StrictMode -Version latest

$version = Get-Content "$PSScriptRoot\Version.txt"
Write-Host "##vso[task.setvariable variable=PackageVersion]$version"
