$connString = "Server=localhost;Database=ERP;Integrated Security=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()

# 1. Ensure Patient exists
$patientId = "1a8eeb54-1206-4af1-9564-08ddce0a80aa"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT COUNT(*) FROM PharmacyPatient WHERE Id = '$patientId'"
$exists = $cmd.ExecuteScalar()
if ($exists -eq 0) {
    $cmd.CommandText = "INSERT INTO PharmacyPatient (Id, PatientCode, FullName, Age, Gender, Dob, NationalId, Phone, Mobile, Address, Email, HasInsurance, ActivePrescriptions, IsActive, CreatedAt, CreatedBy) 
                        VALUES ('$patientId', 'PT-1001', 'Test Patient', 30, 'Male', '1996-01-01', '1000200030', '+966500000001', '+966500000001', 'Riyadh', 'patient@test.com', 0, 0, 1, GETUTCDATE(), '00000000-0000-0000-0000-000000000000')"
    $cmd.ExecuteNonQuery()
    Write-Host "Created Patient: $patientId"
} else {
    Write-Host "Patient already exists: $patientId"
}

# 2. Ensure Doctor exists
$doctorId = "2b8eeb54-1206-4af1-9564-08ddce0a80bb"
$cmd.CommandText = "SELECT COUNT(*) FROM PharmacyDoctor WHERE Id = '$doctorId'"
$exists = $cmd.ExecuteScalar()
if ($exists -eq 0) {
    $cmd.CommandText = "INSERT INTO PharmacyDoctor (Id, Name, Specialization, LicenseNo, Mobile, IsActive, CreatedAt, CreatedBy) 
                        VALUES ('$doctorId', 'Dr. John Watson', 'General Medicine', 'LIC-9988', '+966500000002', 1, GETUTCDATE(), '00000000-0000-0000-0000-000000000000')"
    $cmd.ExecuteNonQuery()
    Write-Host "Created Doctor: $doctorId"
} else {
    Write-Host "Doctor already exists: $doctorId"
}

# 3. Ensure active Batch exists for item 'af0e0501-449e-4617-a9b2-031f893fad4d' (Emperor Fish) in store '73eeb854-1206-4af1-9564-08ddce0a8073'
$storeDetailsId = "73eeb854-1206-4af1-9564-08ddce0a8073"
$itemId = "af0e0501-449e-4617-a9b2-031f893fad4d"
$itemCode = "10110019"
$batchNo = "B-TEST-2026"

$cmd.CommandText = "SELECT COUNT(*) FROM PharmacyBatch WHERE StoreDetailsId = '$storeDetailsId' AND BatchNo = '$batchNo' AND ItemCode = '$itemCode'"
$exists = $cmd.ExecuteScalar()
if ($exists -eq 0) {
    $cmd.CommandText = "INSERT INTO PharmacyBatch (Id, StoreDetailsId, ItemCode, ItemName, BatchNo, ExpiryDate, Quantity, MinQuantity, StoreName, Status, IsActive, CreatedAt, CreatedBy) 
                        VALUES (NEWID(), '$storeDetailsId', '$itemCode', 'Emperor Fish', '$batchNo', '2028-12-31T00:00:00Z', 100.0, 5.0, 'Main Store', 'Active', 1, GETUTCDATE(), '00000000-0000-0000-0000-000000000000')"
    $cmd.ExecuteNonQuery()
    Write-Host "Created Batch: $batchNo"
} else {
    # Ensure it is active, not expired and has stock
    $cmd.CommandText = "UPDATE PharmacyBatch SET IsActive = 1, Quantity = 100.0, ExpiryDate = '2028-12-31T00:00:00Z' WHERE StoreDetailsId = '$storeDetailsId' AND BatchNo = '$batchNo' AND ItemCode = '$itemCode'"
    $cmd.ExecuteNonQuery()
    Write-Host "Updated Batch to be active with stock: $batchNo"
}

# 4. Create a test prescription (Insurance Enabled)
$prescriptionId = [guid]::NewGuid().ToString()
$rxNo = "RX-" + (Get-Date -Format "yyyyMMdd") + "-" + (Get-Random -Minimum 1000 -Maximum 9999)

$cmd.CommandText = "INSERT INTO PharmacyPrescription (Id, RxNo, IssueDate, PatientName, PatientId, DoctorName, DoctorId, BranchId, StoreId, PaymentType, Priority, Status, InsuranceEnabled, ItemsCount, SubTotal, DiscountTotal, TaxTotal, GrandTotal, PatientPayable, IsActive, CreatedAt, CreatedBy, StoreDetailsId, InsuranceCoveragePercentage, InsurancePatientSharePercentage, InsuranceCoveredAmount, InsurancePatientPayableAmount) 
                    VALUES ('$prescriptionId', '$rxNo', GETUTCDATE(), 'Test Patient', '$patientId', 'Dr. John Watson', '$doctorId', '00000000-0000-0000-0000-000000000001', 'STORE-01', 'Insurance', 'Normal', 'Pending', 1, 1, 10.0, 0.0, 1.5, 11.5, 2.3, 1, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', '$storeDetailsId', 80.0, 20.0, 9.2, 2.3)"
$cmd.ExecuteNonQuery()

# 5. Add item to the prescription
$cmd.CommandText = "INSERT INTO PharmacyPrescriptionItem (Id, PrescriptionId, PrescriptionRxNo, [LineNo], ItemCode, ItemName, RequestedQty, DispensedQty, StoreId, BatchId, BatchNo, ExpiryDate, UnitPrice, DiscountAmount, TaxAmount, LineTotal, AllowSubstitute, IsControlled, StoreDetailsId, IsActive, CreatedAt, CreatedBy, ItemId, AvailabilityStatus) 
                    VALUES (NEWID(), '$prescriptionId', '$rxNo', 1, '$itemCode', 'Emperor Fish', 1.0, 0.0, 'STORE-01', 'batch-id-01', '$batchNo', '2028-12-31T00:00:00Z', 10.0, 0.0, 1.5, 11.5, 0, 0, '$storeDetailsId', 1, GETUTCDATE(), '00000000-0000-0000-0000-000000000000', '$itemId', 'Available')"
$cmd.ExecuteNonQuery()

# 6. Add associated Insurance Claim
$claimId = [guid]::NewGuid().ToString()
$cmd.CommandText = "INSERT INTO PharmacyInsuranceClaim (Id, ClaimNo, RxNo, PatientName, Insurer, TotalAmount, ApprovedAmount, Status, ReferenceNo, SubmittedAt, StoreDetailsId, IsActive, CreatedBy, CreatedAt, CoveragePercentage, InsuranceCoveredAmount, InsuranceMemberNo, NphiesStatus, NphiesSubmittedAt, PatientShareAmount, PrescriptionId) 
                    VALUES ('$claimId', '$rxNo-CLM', '$rxNo', 'Test Patient', 'Bupa', 11.5, 0.0, 'Submitted', 'REF-12345', GETUTCDATE(), '$storeDetailsId', 1, '00000000-0000-0000-0000-000000000000', GETUTCDATE(), 80.0, 9.2, 'MEM-99988', 'Submitted', GETUTCDATE(), 2.3, '$prescriptionId')"
$cmd.ExecuteNonQuery()

$conn.Close()

Write-Host "`n🎉 Prescription and Insurance Claim generated successfully!" -ForegroundColor Green
Write-Host "Prescription ID: " -NoNewline
Write-Host "$prescriptionId" -ForegroundColor Yellow
Write-Host "Prescription No (RxNo): " -NoNewline
Write-Host "$rxNo" -ForegroundColor Yellow
Write-Host "Please set the 'prescriptionId' variable in Postman to: $prescriptionId" -ForegroundColor Cyan
