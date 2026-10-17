$connString = "Server=localhost;Integrated Security=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT name FROM sys.databases"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output $reader.GetString(0)
}
$conn.Close()
