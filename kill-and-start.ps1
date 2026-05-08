# Clinic SaaS local helper.
# Script này chỉ dọn dotnet process/port local khi cần chạy lại service đã được scaffold.
# Không start service cụ thể vì repo hiện chưa có backend runtime hoàn chỉnh.

param(
    [int[]]$Ports = @(5000, 5001, 5002, 5173, 5174, 5175)
)

Write-Host "Stopping local dotnet processes..." -ForegroundColor Yellow
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

foreach ($port in $Ports) {
    Write-Host "Checking port $port..." -ForegroundColor Yellow
    $connections = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    foreach ($connection in $connections) {
        Write-Host "Found process on port $port: PID $($connection.OwningProcess)" -ForegroundColor Cyan
        Stop-Process -Id $connection.OwningProcess -Force -ErrorAction SilentlyContinue
    }
}

Write-Host ""
Write-Host "Done. Start Clinic SaaS services only after temp/plan.md is approved and apps/services exist." -ForegroundColor Green
