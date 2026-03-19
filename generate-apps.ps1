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
    One or more app names to generate. Valid values: no-skills, dotnet-webapi, dotnet-artisan, managedcode-dotnet-skills.
    If not specified, all four apps are generated.

.EXAMPLE
    .\generate-apps.ps1
    # Generates all four variants.

.EXAMPLE
    .\generate-apps.ps1 -Apps no-skills
    # Generates only the no-skills variant.

.EXAMPLE
    .\generate-apps.ps1 -Apps dotnet-webapi, dotnet-artisan
    # Generates the dotnet-webapi and dotnet-artisan variants.

.EXAMPLE
    .\generate-apps.ps1 -Apps managedcode-dotnet-skills
    # Generates only the managedcode-dotnet-skills variant.
#>
param(
    [string]$Path = $PWD,

    [ValidateSet('no-skills', 'dotnet-webapi', 'dotnet-artisan', 'managedcode-dotnet-skills')]
    [string[]]$Apps
)

$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Helpers – temporarily register / unregister skill directories in config.json
# ---------------------------------------------------------------------------
function Get-CopilotConfigPath {
    $configDir = if ($env:COPILOT_HOME) { $env:COPILOT_HOME } else { Join-Path $HOME '.copilot' }
    Join-Path $configDir 'config.json'
}

function Add-SkillDirectory {
    <#
    .SYNOPSIS
        Adds one or more absolute paths to skill_directories in the Copilot config.
    .DESCRIPTION
        Reads ~/.copilot/config.json, appends any paths not already present to the
        skill_directories array, and writes the file back. Returns the list of paths
        that were actually added (for later removal).
    #>
    param([string[]]$Paths)

    $configPath = Get-CopilotConfigPath
    $config = Get-Content $configPath -Raw | ConvertFrom-Json

    if (-not (Get-Member -InputObject $config -Name 'skill_directories' -MemberType NoteProperty)) {
        $config | Add-Member -NotePropertyName 'skill_directories' -NotePropertyValue @()
    }

    $added = @()
    foreach ($p in $Paths) {
        $abs = (Resolve-Path $p).Path
        if ($config.skill_directories -notcontains $abs) {
            $config.skill_directories = @($config.skill_directories) + $abs
            $added += $abs
        }
    }

    if ($added.Count -gt 0) {
        $config | ConvertTo-Json -Depth 10 | Set-Content $configPath -Encoding utf8NoBOM
        Write-Host "  Registered skill dir(s): $($added -join ', ')" -ForegroundColor DarkGray
    }
    return $added
}

function Remove-SkillDirectory {
    <#
    .SYNOPSIS
        Removes previously-added paths from skill_directories in the Copilot config.
    #>
    param([string[]]$Paths)

    if (-not $Paths -or $Paths.Count -eq 0) { return }

    $configPath = Get-CopilotConfigPath
    $config = Get-Content $configPath -Raw | ConvertFrom-Json

    if (-not (Get-Member -InputObject $config -Name 'skill_directories' -MemberType NoteProperty)) {
        return
    }

    $config.skill_directories = @($config.skill_directories | Where-Object { $_ -notin $Paths })
    $config | ConvertTo-Json -Depth 10 | Set-Content $configPath -Encoding utf8NoBOM
    Write-Host "  Unregistered skill dir(s): $($Paths -join ', ')" -ForegroundColor DarkGray
}

# ---------------------------------------------------------------------------
# Run definitions
# ---------------------------------------------------------------------------
Push-Location $Path
try {
    $allRuns = [ordered]@{
        'no-skills' = @{
            Label       = 'No skills'
            Folder      = 'src-no-skills'
            Prompt      = 'Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in `src` put them in `src-no-skills`. Do NOT use any skills during this process.'
            CopilotArgs = @()
            SkillDirs   = @()
        }
        'dotnet-webapi' = @{
            Label       = 'dotnet-webapi skill'
            Folder      = 'src-dotnet-webapi'
            Prompt      = 'Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in `src` put them in `src-dotnet-webapi`. Use the `dotnet-webapi` but do NOT use any other skills.'
            CopilotArgs = @()
            SkillDirs   = @('contrib\skills\dotnet-webapi')
        }
        'dotnet-artisan' = @{
            Label       = 'dotnet-artisan skill'
            Folder      = 'src-dotnet-artisan'
            Prompt      = 'Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in `src` put them in `src-dotnet-artisan`. Use the `dotnet-artisan` skills but do NOT use the `dotnet-webapi` skill.'
            CopilotArgs = @('--plugin-dir', 'contrib\plugins\dotnet-artisan')
            SkillDirs   = @()
        }
        'managedcode-dotnet-skills' = @{
            Label       = 'managedcode-dotnet-skills'
            Folder      = 'src-managedcode-dotnet-skills'
            Prompt      = 'Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in `src` put them in `src-managedcode-dotnet-skills`. Use the `managedcode-dotnet-skills` skills but do NOT use any other skills.'
            CopilotArgs = @()
            SkillDirs   = @('contrib\skills\managedcode-dotnet-skills')
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

        # Register skill directories for this run (if any)
        $addedSkills = @()
        if ($run.SkillDirs.Count -gt 0) {
            $addedSkills = Add-SkillDirectory -Paths $run.SkillDirs
        }

        try {
            # Build argument list: -p <prompt> --yolo [extra args]
            $copilotArgs = @('-p', $run.Prompt, '--yolo') + $run.CopilotArgs
            copilot @copilotArgs

            if ($LASTEXITCODE -ne 0) {
                Write-Error "Copilot failed for '$($run.Label)' with exit code $LASTEXITCODE"
            }
        }
        finally {
            # Always unregister skill directories, even on failure
            if ($addedSkills.Count -gt 0) {
                Remove-SkillDirectory -Paths $addedSkills
            }
        }

        Write-Host "Completed: $($run.Label)" -ForegroundColor Green
    }

    Write-Host "`nAll generations complete." -ForegroundColor Green
}
finally {
    Pop-Location
}
