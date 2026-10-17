$ports = @(5001, 5032, 7219)
foreach ($port in $ports) {
    $connections = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    if ($connections) {
        Write-Host "Port $port is in use by PID:"
        $connections | Format-Table LocalAddress, LocalPort, OwningProcess, State -AutoSize
        foreach ($conn in $connections) {
            $proc = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
            if ($proc) {
                Write-Host "Process Details: PID=$($proc.Id), Name=$($proc.Name), Path=$($proc.Path)"
            }
        }
    } else {
        Write-Host "Port $port is free."
    }
}
