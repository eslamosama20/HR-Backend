$connectionString = "Server=localhost;Database=ErpMain;Integrated Security=True;TrustServerCertificate=True;Encrypt=False"
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
try {
    $connection.Open()
    Write-Host "Connected successfully to ERP database!"
    
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProjectPriceQuotes'"
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
    $dt = New-Object System.Data.DataTable
    $adapter.Fill($dt) | Out-Null
    Write-Host "`nColumns in ProjectPriceQuotes:"
    $dt | Format-Table -AutoSize
}
catch {
    Write-Error $_.Exception.Message
}
finally {
    $connection.Close()
}
