$connString = "Server=localhost;Database=ERP;Integrated Security=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME LIKE '%Pharmacy%'"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output $reader.GetString(0)
}
$conn.Close()
