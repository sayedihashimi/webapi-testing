#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates three variants of the apps by calling the Copilot CLI with different skill configurations.

.DESCRIPTION
    Deletes the src-no-skills, src-dotnet-webapi, and src-dotnet-artisan folders,
    then invokes the Copilot CLI three times with different prompts and skill settings.

.PARAMETER Path
    The folder to generate files in. Defaults to the current working directory.
#>
param(
    [string]$Path = $PWD
)

$ErrorActionPreference = 'Stop'

Push-Location $Path
try {
    # Clean up existing output folders
    $folders = @('src-no-skills', 'src-dotnet-webapi', 'src-dotnet-artisan')
    foreach ($folder in $folders) {
        if (Test-Path $folder) {
            Write-Host "Removing $folder ..." -ForegroundColor Yellow
            Remove-Item -Recurse -Force $folder
        }
    }

    # Define the three copilot invocations
    $runs = @(
        @{
            Label  = 'No skills'
            Prompt = 'Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in `src` put them in `src-no-skills`. Do NOT use any skills during this process.'
        },
        @{
            Label  = 'dotnet-webapi skill'
            Prompt = 'Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in `src` put them in `src-dotnet-webapi`. Use the `dotnet-webapi` but do NOT use any other skills.'
        },
        @{
            Label  = 'dotnet-artisan skill'
            Prompt = 'Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in `src` put them in `src-dotnet-artisan`. Use the `dotnet-artisan` skills but do NOT use the `dotnet-webapi` skill.'
        }
    )

    foreach ($run in $runs) {
        Write-Host "`n========================================" -ForegroundColor Cyan
        Write-Host "Starting: $($run.Label)" -ForegroundColor Cyan
        Write-Host "========================================" -ForegroundColor Cyan

        copilot -p $run.Prompt --yolo

        if ($LASTEXITCODE -ne 0) {
            Write-Error "Copilot failed for '$($run.Label)' with exit code $LASTEXITCODE"
        }

        Write-Host "Completed: $($run.Label)" -ForegroundColor Green
    }

    Write-Host "`nAll generations complete." -ForegroundColor Green
}
finally {
    Pop-Location
}
