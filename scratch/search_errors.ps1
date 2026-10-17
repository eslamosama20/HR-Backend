$logPath = "C:\Users\osama\.gemini\antigravity-ide\brain\0843c7e3-59f0-471c-b8dc-214aca8c9a33\.system_generated\tasks\task-8161.log"
if (Test-Path $logPath) {
    Get-Content $logPath | Select-String -Pattern "fail", "error", "exception", "1171", "DeleteInvoice" -SimpleMatch | Select-Object -Last 30 | Out-Host
} else {
    Write-Host "Log file not found."
}
