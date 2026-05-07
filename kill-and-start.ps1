# Kill all dotnet processes
Write-Host "Killing all dotnet processes..." -ForegroundColor Yellow
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

# Kill by port
Write-Host "Checking port 5248..." -ForegroundColor Yellow
$process = Get-NetTCPConnection -LocalPort 5248 -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "Found process on port 5248: PID $($process.OwningProcess)" -ForegroundColor Cyan
    Stop-Process -Id $process.OwningProcess -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
}

# Verify port is free
$portInUse = Get-NetTCPConnection -LocalPort 5248 -ErrorAction SilentlyContinue
if ($portInUse) {
    Write-Host "⚠️ Port 5248 still in use, waiting..." -ForegroundColor Red
    Start-Sleep -Seconds 5
} else {
    Write-Host "✓ Port 5248 is free" -ForegroundColor Green
}

# Start Chatbot API
Write-Host "`nStarting Chatbot API..." -ForegroundColor Yellow
Set-Location "F:\source\2-github\ez-sales-bot"

# Start in background
$apiProcess = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project backend/src/Services/Chatbot/EzSalesBot.Chatbot.Api --no-build" `
    -NoNewWindow `
    -PassThru

Write-Host "API started with PID $($apiProcess.Id)" -ForegroundColor Green
Start-Sleep -Seconds 10

# Test health
Write-Host "`nTesting API health..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "http://localhost:5248/health" -Method Get
    Write-Host "✓ API Health: $health" -ForegroundColor Green
} catch {
    Write-Host "✗ API not responding yet" -ForegroundColor Red
}

Write-Host "`n✓ Ready for testing!" -ForegroundColor Green
