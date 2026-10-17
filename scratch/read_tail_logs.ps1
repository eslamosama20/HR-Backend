$logPath = "C:\Users\osama\OneDrive\Desktop\erp project\SmartAndPro.ERP.Inventory\SmartAndPro.ERP.WebAPI\logs.json"
if (Test-Path $logPath) {
    Get-Content $logPath -Tail 50 | Out-Host
} else {
    Write-Host "Log file not found."
}
