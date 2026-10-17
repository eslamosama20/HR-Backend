$connString = "Server=localhost;Database=ERP;Integrated Security=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()

$tables = @("PharmacyPatient", "PharmacyDoctor", "PharmacyBatch", "PharmacyPrescription", "PharmacyPrescriptionItem")
foreach ($t in $tables) {
    Write-Output "=== $t Columns ==="
    $cmd.CommandText = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$t'"
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Output ($reader.GetString(0) + " (" + $reader.GetString(1) + ")")
    }
    $reader.Close()
    Write-Output ""
}
$conn.Close()
