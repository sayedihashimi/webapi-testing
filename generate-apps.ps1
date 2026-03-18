#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates variants of the apps by calling the Copilot CLI with different skill configurations.

.DESCRIPTION
    Deletes and regenerates one or more src-* folders by invoking the Copilot CLI
    with different prompts and skill settings.

.PARAMETER Path
    The folder to generate files in. Defaults to the current working directory.

.PARAMETER Apps
    One or more app names to generate. Valid values: no-skills, dotnet-webapi, dotnet-artisan.
    If not specified, all three apps are generated.

.EXAMPLE
    .\generate-apps.ps1
    # Generates all three variants.

.EXAMPLE
    .\generate-apps.ps1 -Apps no-skills
    # Generates only the no-skills variant.

.EXAMPLE
    .\generate-apps.ps1 -Apps dotnet-webapi, dotnet-artisan
    # Generates the dotnet-webapi and dotnet-artisan variants.
#>
param(
    [string]$Path = $PWD,

    [ValidateSet('no-skills', 'dotnet-webapi', 'dotnet-artisan')]
    [string[]]$Apps
)

$ErrorActionPreference = 'Stop'

Push-Location $Path
try {
    # Define all available copilot invocations keyed by app name
    $allRuns = [ordered]@{
        'no-skills' = @{
            Label  = 'No skills'
            Folder = 'src-no-skills'
            Prompt = 'Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in `src` put them in `src-no-skills`. Do NOT use any skills during this process.'
        }
        'dotnet-webapi' = @{
            Label  = 'dotnet-webapi skill'
            Folder = 'src-dotnet-webapi'
            Prompt = 'Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in `src` put them in `src-dotnet-webapi`. Use the `dotnet-webapi` but do NOT use any other skills.'
        }
        'dotnet-artisan' = @{
            Label  = 'dotnet-artisan skill'
            Folder = 'src-dotnet-artisan'
            Prompt = 'Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in `src` put them in `src-dotnet-artisan`. Use the `dotnet-artisan` skills but do NOT use the `dotnet-webapi` skill.'
        }
    }

    # If no apps specified, run all of them
    if (-not $Apps -or $Apps.Count -eq 0) {
        $Apps = @($allRuns.Keys)
    }

    # Build the list of runs to execute
    $runs = foreach ($app in $Apps) {
        $allRuns[$app]
    }

    # Clean up only the folders that will be regenerated
    foreach ($run in $runs) {
        $folder = $run.Folder
        if (Test-Path $folder) {
            Write-Host "Removing $folder ..." -ForegroundColor Yellow
            Remove-Item -Recurse -Force $folder
        }
    }

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
