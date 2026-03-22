#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs the full skill evaluation pipeline for the ASP.NET Core Web API example.

.DESCRIPTION
    Executes generate → verify → analyze for all scenarios and configurations
    defined in eval.yaml.

.PARAMETER SkipGenerate
    Skip the generation step (useful when re-running verify + analyze on existing output).

.PARAMETER AnalyzeOnly
    Skip both generate and verify, only run analysis.

.PARAMETER Config
    Path to the eval config file. Defaults to eval.yaml in this directory.

.EXAMPLE
    ./run-pipeline.ps1
    ./run-pipeline.ps1 -SkipGenerate
    ./run-pipeline.ps1 -AnalyzeOnly
#>
[CmdletBinding()]
param(
    [switch]$SkipGenerate,
    [switch]$AnalyzeOnly,
    [string]$Config = "eval.yaml"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptDir = $PSScriptRoot
Push-Location $scriptDir

try {
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host " ASP.NET Core Web API - Skill Eval" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Config : $Config"
    Write-Host "Directory: $scriptDir"
    Write-Host ""

    $cmd = @("python", "-m", "skill_eval", "--config", $Config, "run")

    if ($AnalyzeOnly) {
        $cmd += "--analyze-only"
        Write-Host "Mode: analyze only" -ForegroundColor Yellow
    }
    elseif ($SkipGenerate) {
        $cmd += "--skip-generate"
        Write-Host "Mode: skip generate (verify + analyze)" -ForegroundColor Yellow
    }
    else {
        Write-Host "Mode: full pipeline (generate → verify → analyze)" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "Running: $($cmd -join ' ')" -ForegroundColor DarkGray
    Write-Host ""

    & $cmd[0] $cmd[1..($cmd.Length - 1)]
    $exitCode = $LASTEXITCODE

    if ($exitCode -eq 0) {
        Write-Host ""
        Write-Host "Pipeline completed successfully." -ForegroundColor Green
    }
    else {
        Write-Host ""
        Write-Host "Pipeline failed with exit code $exitCode." -ForegroundColor Red
    }

    exit $exitCode
}
finally {
    Pop-Location
}
