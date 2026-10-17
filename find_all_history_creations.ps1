$file = "c:\Users\osama\OneDrive\Desktop\erp project\SmartAndPro.ERP.Inventory\SmartAndPro.ERP.Application\Services\Projects\ProjectService.cs"
$content = Get-Content -Path $file
for ($i = 0; $i -lt $content.Count; $i++) {
    if ($content[$i] -like "*new ProjectHistory*") {
        Write-Host "--- Match at line $($i + 1) ---"
        for ($j = [Math]::Max(0, $i - 2); $j -le [Math]::Min($content.Count - 1, $i + 12); $j++) {
            Write-Host "$($j + 1): $($content[$j])"
        }
    }
}
