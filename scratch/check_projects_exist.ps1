$connectionString = "Server=localhost;Database=ErpMain;Integrated Security=True;TrustServerCertificate=True;Encrypt=False"
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
try {
    $connection.Open()
    Write-Host "Connected successfully to ERP database!"
    
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM Projects WHERE IsDeleted = 0"
    $count = $cmd.ExecuteScalar()
    Write-Host "Number of active projects in database: $count"

    $cmd.CommandText = "SELECT Id, ProjectName, ProjectNumber, IsDeleted, IsActive FROM Projects"
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
    $dt = New-Object System.Data.DataTable
    $adapter.Fill($dt) | Out-Null
    $dt | Format-Table -AutoSize
}
catch {
    Write-Error $_.Exception.Message
}
finally {
    $connection.Close()
}
