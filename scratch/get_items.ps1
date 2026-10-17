$connString = "Server=localhost;Database=ERP;Integrated Security=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TOP 5 Id, ReferenceCode, EnglishName FROM MainPurchaseClass"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ($reader.GetGuid(0).ToString() + " | " + $reader.GetString(1) + " | " + $reader.GetString(2))
}
$conn.Close()
