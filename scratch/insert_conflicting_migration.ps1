$connectionString = "Server=localhost;Database=ERP;Integrated Security=True;TrustServerCertificate=True;Encrypt=False"
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
try {
    $connection.Open()
    Write-Host "Connected successfully to ERP database!"
    
    $cmd = $connection.CreateCommand()
    # Check if migration exists in history
    $cmd.CommandText = "SELECT COUNT(*) FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260721032755_Add-New-features'"
    $exists = $cmd.ExecuteScalar()
    if ($exists -eq 0) {
        Write-Host "Migration not found in history. Inserting it..."
        $cmd.CommandText = "INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20260721032755_Add-New-features', '8.0.0')"
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "Inserted successfully!"
    } else {
        Write-Host "Migration already exists in history."
    }
}
catch {
    Write-Error $_.Exception.Message
}
finally {
    $connection.Close()
}
