<#
.SYNOPSIS
    Executa cenarios de teste k6 do KRT Bank
.EXAMPLE
    .\tests\k6\run-tests.ps1 -Scenario smoke
    .\tests\k6\run-tests.ps1 -Scenario load
    .\tests\k6\run-tests.ps1 -Scenario stress
    .\tests\k6\run-tests.ps1 -Scenario spike
    .\tests\k6\run-tests.ps1 -Scenario soak
    .\tests\k6\run-tests.ps1 -Scenario breakpoint
    .\tests\k6\run-tests.ps1 -Scenario all
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('smoke', 'load', 'stress', 'spike', 'soak', 'breakpoint', 'all')]
    [string]$Scenario,

    [string]$BaseUrl = "http://localhost:5000",
    [switch]$UseDocker
)

$ErrorActionPreference = "Stop"
$testDir = "tests\k6"
$reportDir = "$testDir\reports"
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"

Write-Host ""
Write-Host "  KRT Bank - k6 Performance Tests" -ForegroundColor Cyan
Write-Host "  Scenario: $Scenario" -ForegroundColor White
Write-Host "  Base URL: $BaseUrl" -ForegroundColor Gray
Write-Host "  Timestamp: $timestamp" -ForegroundColor Gray
Write-Host ""

function Run-K6Test {
    param([string]$Name, [string]$File)

    Write-Host "  [$Name] Starting..." -ForegroundColor Yellow
    $reportFile = "$reportDir\${Name}_${timestamp}.json"

    if ($UseDocker) {
        docker run --rm -i `
            --network host `
            -v "${PWD}\${testDir}:/scripts" `
            -e BASE_URL=$BaseUrl `
            grafana/k6 run `
            --out json=/scripts/reports/${Name}_${timestamp}.json `
            --summary-export=/scripts/reports/${Name}_${timestamp}_summary.json `
            /scripts/scenarios/${File}
    } else {
        k6 run `
            --env BASE_URL=$BaseUrl `
            --out json=$reportFile `
            --summary-export="${reportDir}\${Name}_${timestamp}_summary.json" `
            "$testDir\scenarios\$File"
    }

    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [$Name] PASSED" -ForegroundColor Green
    } else {
        Write-Host "  [$Name] FAILED (exit code: $LASTEXITCODE)" -ForegroundColor Red
    }
    Write-Host ""
}

$scenarios = @{
    'smoke'      = 'smoke.js'
    'load'       = 'load.js'
    'stress'     = 'stress.js'
    'spike'      = 'spike.js'
    'soak'       = 'soak.js'
    'breakpoint' = 'breakpoint.js'
}

if ($Scenario -eq 'all') {
    foreach ($s in @('smoke', 'load', 'stress', 'spike')) {
        Run-K6Test -Name $s -File $scenarios[$s]
    }
    Write-Host "  Soak e Breakpoint nao incluidos no 'all' (longa duracao)" -ForegroundColor DarkYellow
    Write-Host "  Execute separadamente: -Scenario soak | -Scenario breakpoint" -ForegroundColor Gray
} else {
    Run-K6Test -Name $Scenario -File $scenarios[$Scenario]
}

Write-Host "  Reports salvos em: $reportDir\" -ForegroundColor Cyan
