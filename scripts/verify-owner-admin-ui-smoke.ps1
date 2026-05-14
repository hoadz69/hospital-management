Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$FrontendRoot = Join-Path $RepoRoot "frontend"
$RunRoot = Join-Path $RepoRoot "temp\agent-runner"

New-Item -ItemType Directory -Force $RunRoot | Out-Null

$outLog = Join-Path $RunRoot "owner-admin-vite.out.log"
$errLog = Join-Path $RunRoot "owner-admin-vite.err.log"

$existing = Get-NetTCPConnection -LocalPort 5175 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
if ($existing) {
    throw "Port 5175 is already in use; refusing to attach to unknown dev server."
}

$process = Start-Process `
    -FilePath "cmd.exe" `
    -ArgumentList @("/d", "/c", "cd /d `"$FrontendRoot`" && npm run dev:owner") `
    -RedirectStandardOutput $outLog `
    -RedirectStandardError $errLog `
    -WindowStyle Hidden `
    -PassThru

try {
    $ready = $false
    for ($i = 0; $i -lt 40; $i++) {
        try {
            $root = Invoke-WebRequest -UseBasicParsing -Uri "http://127.0.0.1:5175/" -TimeoutSec 2
            if ($root.StatusCode -eq 200 -and $root.Content.Contains('id="app"')) {
                $ready = $true
                break
            }
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }

    if (-not $ready) {
        throw "Owner Admin Vite dev server did not become ready on port 5175."
    }

    $routes = @(
        "/dashboard",
        "/clinics",
        "/clinics/create",
        "/clinics/tenant-aurora-dental"
    )

    foreach ($route in $routes) {
        $response = Invoke-WebRequest -UseBasicParsing -Uri ("http://127.0.0.1:5175" + $route) -TimeoutSec 5
        if ($response.StatusCode -ne 200) {
            throw "Route $route returned HTTP $($response.StatusCode)."
        }

        if (-not $response.Content.Contains('id="app"')) {
            throw "Route $route did not return Vite app shell."
        }

        Write-Output "PASS $route HTTP 200 + #app"
    }

    Write-Output "Owner Admin route smoke PASS."
}
finally {
    $listeners = Get-NetTCPConnection -LocalPort 5175 -State Listen -ErrorAction SilentlyContinue
    foreach ($listener in $listeners) {
        Stop-Process -Id $listener.OwningProcess -Force -ErrorAction SilentlyContinue
    }

    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }

    Start-Sleep -Milliseconds 500
}
