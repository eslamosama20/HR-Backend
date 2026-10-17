$connectionString = "Server=localhost;Database=ErpMain;Integrated Security=True;TrustServerCertificate=True;Encrypt=False"
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
try {
    $connection.Open()
    Write-Host "Connected successfully to ERP database!"
    
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT Id, ProjectName, ProjectNumber, QuoteNumber, QuoteDate, QuotationId FROM Projects WHERE Id = 'cbde160f-afc2-4b01-9fdf-36fd5d09fd28'"
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
    $dt = New-Object System.Data.DataTable
    $adapter.Fill($dt) | Out-Null
    Write-Host "`nProject row:"
    $dt | Format-Table -AutoSize
}
catch {
    Write-Error $_.Exception.Message
}
finally {
    $connection.Close()
}
