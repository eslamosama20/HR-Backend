$connString = "Server=localhost;Database=ERP;Integrated Security=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PharmacyInsuranceClaim'"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ($reader.GetString(0) + " (" + $reader.GetString(1) + ")")
}
$conn.Close()
