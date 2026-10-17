$connString = "Server=localhost;Database=ERP;Integrated Security=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT StoreDetailsId, ItemCode, BatchNo, Quantity, IsActive, ExpiryDate FROM PharmacyBatch WHERE ItemCode = 'DRUG-1002'"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output ("StoreDetailsId: " + $reader.GetGuid(0).ToString() + " | ItemCode: " + $reader.GetString(1) + " | BatchNo: " + $reader.GetString(2) + " | Qty: " + $reader.GetDecimal(3).ToString() + " | IsActive: " + $reader.GetBoolean(4).ToString() + " | ExpiryDate: " + $reader.GetDateTime(5).ToString())
}
$conn.Close()
