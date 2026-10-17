$connString = "Server=localhost;Database=ERP;Integrated Security=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()

# 1. Check if MainPurchaseClass for DRUG-1002 exists
$cmd.CommandText = "SELECT Id FROM MainPurchaseClass WHERE ReferenceCode = 'DRUG-1002'"
$itemId = $cmd.ExecuteScalar()

if ($itemId -eq $null) {
    # If it doesn't exist, we generate a new ID and insert it
    $itemId = [guid]::NewGuid().ToString()
    $cmd.CommandText = "INSERT INTO MainPurchaseClass (Id, ReferenceCode, ArabicName, EnglishName, IsPharmacyItem, ItemType, IsActive, CreatedAt, CreatedBy, MeasurementUnitId) 
                        VALUES ('$itemId', 'DRUG-1002', 'Morphine 10mg', 'Morphine 10mg', 1, 'Medicine', 1, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000')"
    $cmd.ExecuteNonQuery()
    Write-Host "Created MainPurchaseClass for DRUG-1002: $itemId"
} else {
    $itemId = $itemId.ToString()
    Write-Host "MainPurchaseClass for DRUG-1002 already exists with ID: $itemId"
}

# 2. Ensure active Batch exists for 'DRUG-1002' with batchNo 'B-MOR-01' in stores
$stores = @("73eeb854-1206-4af1-9564-08ddce0a8073", "00000000-0000-0000-0000-000000000001")
$batchNo = "B-MOR-01"

foreach ($storeDetailsId in $stores) {
    $cmd.CommandText = "SELECT COUNT(*) FROM PharmacyBatch WHERE StoreDetailsId = '$storeDetailsId' AND BatchNo = '$batchNo' AND ItemCode = 'DRUG-1002'"
    $exists = $cmd.ExecuteScalar()
    if ($exists -eq 0) {
        $cmd.CommandText = "INSERT INTO PharmacyBatch (Id, StoreDetailsId, ItemCode, ItemName, BatchNo, ExpiryDate, Quantity, MinQuantity, StoreName, Status, IsActive, CreatedAt, CreatedBy) 
                            VALUES (NEWID(), '$storeDetailsId', 'DRUG-1002', 'Morphine 10mg', '$batchNo', '2028-12-31T00:00:00Z', 10.0, 1.0, 'Main Store', 'Active', 1, GETUTCDATE(), '00000000-0000-0000-0000-000000000000')"
        $cmd.ExecuteNonQuery()
        Write-Host "Created Batch for DRUG-1002 in store $storeDetailsId"
    } else {
        $cmd.CommandText = "UPDATE PharmacyBatch SET IsActive = 1, Quantity = 10.0, ExpiryDate = '2028-12-31T00:00:00Z' WHERE StoreDetailsId = '$storeDetailsId' AND BatchNo = '$batchNo' AND ItemCode = 'DRUG-1002'"
        $cmd.ExecuteNonQuery()
        Write-Host "Updated Batch to be active with stock for DRUG-1002 in store $storeDetailsId"
    }
}

$conn.Close()
Write-Host "🎉 Seeding for DRUG-1002 completed successfully!" -ForegroundColor Green
