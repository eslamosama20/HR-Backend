$connectionString = "Server=localhost;Database=ErpMain;Integrated Security=True;TrustServerCertificate=True;Encrypt=False"
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
try {
    $connection.Open()
    Write-Host "Connected successfully to ERP database!"
    
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT TOP 10 * FROM ProjectPriceQuotes"
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
    $dt = New-Object System.Data.DataTable
    $adapter.Fill($dt) | Out-Null
    Write-Host "`nProjectPriceQuotes:"
    $dt | Format-Table -AutoSize

    $cmd.CommandText = "SELECT TOP 10 Id, ProjectId, QuoteNumber, QuoteAmount FROM ProjectPriceQuotes"
    $dt2 = New-Object System.Data.DataTable
    $adapter2 = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
    $adapter2.Fill($dt2) | Out-Null
    $dt2 | Format-Table -AutoSize
}
catch {
    Write-Error $_.Exception.Message
}
finally {
    $connection.Close()
}
