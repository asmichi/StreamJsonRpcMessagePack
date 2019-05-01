
# Copyright (c) @asmichi (https://github.com/asmichi). Licensed under the MIT License. See LICENCE in the project root for details.

param(
    [parameter()]
    [switch]
    $RetailRelease = $false
)

Set-StrictMode -Version latest
$ErrorActionPreference = "Stop"

function Exec {
    param(
        [parameter(Mandatory = $true)]
        [scriptblock]
        $cmd
    )

    & $cmd

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Command failed with exit code ${LASTEXITCODE}: $cmd"
    }
}

$worktreeRoot = Resolve-Path "$PSScriptRoot\.."
$slnFile = "$worktreeRoot\src\StreamJsonRpcMessagePack.sln"

$commitHash = (git rev-parse HEAD)
$shortCommitHash = $commitHash.Substring(0, 10)
$commitCount = (git rev-list --count HEAD)
$baseVersion = Get-Content "$worktreeRoot\build\Version.txt"
$assemblyVersion = "$baseVersion.0"
$fileVersion = $assemblyVersion
$informationalVersion = "$fileVersion+g$shortCommitHash"
$packageVersion = if ($RetailRelease) { $baseVersion } else { "$baseVersion-pre.$commitCount+g$shortCommitHash" }

$commonBuildOptions = @("-nologo",
    "--verbosity:quiet",
    "-p:Platform=AnyCPU",
    "--configuration",
    "Release",
    "-p:Version=$assemblyVersion",
    "-p:PackageVersion=$packageVersion",
    "-p:FileVersion=$fileVersion",
    "-p:AssemblyVersion=$assemblyVersion",
    "-p:InformationalVersion=$informationalVersion"
)

Exec { dotnet restore --verbosity:quiet $slnFile }
Exec { dotnet build $commonBuildOptions $slnFile }
Exec { dotnet test $commonBuildOptions "$worktreeRoot\src\JsonRpcMessagePackFormatter.Test\JsonRpcMessagePackFormatter.Test.csproj" }

Exec {
    nuget pack `
        -Verbosity quiet -ForceEnglishOutput `
        -Version $packageVersion `
        -BasePath "$worktreeRoot\bin\JsonRpcMessagePackFormatter\AnyCPU\Release" `
        -OutputDirectory "$worktreeRoot\bin\nupkg" `
        -Properties commitHash=$commitHash `
        "$worktreeRoot\build\nuspec\Asmichi.StreamJsonRpcMessagePack.nuspec"
}
