# PowerShell script to seed paid services via direct HTTP POST
$apiUrl = "http://localhost:5001/api/PublicPaidService"

# Service 1: Transportation between Makkah and Madinah
$json1 = @'
{
  "nameEn": "Transportation between Makkah and Madinah",
  "nameAr": "نقل بين مكة والمدينة",
  "descriptionEn": "Comfortable transportation between the two holy cities",
  "descriptionAr": "خدمة نقل مريحة بين الحرمين الشريفين",
  "icon": "pi pi-car",
  "serviceTypeCode": "TRANSPORT",
  "displayOrder": 1,
  "isActive": true
}
'@

# Service 2: Madinah Tours
$json2 = @'
{
  "nameEn": "Madinah Tours",
  "nameAr": "مزارات المدينة",
  "descriptionEn": "Guided tours of historical sites in Madinah",
  "descriptionAr": "جولات إرشادية للمواقع التاريخية في المدينة المنورة",
  "icon": "pi pi-compass",
  "serviceTypeCode": "TOUR",
  "displayOrder": 2,
  "isActive": true
}
'@

# Service 3: Makkah Tours
$json3 = @'
{
  "nameEn": "Makkah Tours",
  "nameAr": "مزارات مكة",
  "descriptionEn": "Guided tours of historical sites in Makkah",
  "descriptionAr": "جولات إرشادية للمواقع التاريخية في مكة المكرمة",
  "icon": "pi pi-map-marker",
  "serviceTypeCode": "TOUR",
  "displayOrder": 3,
  "isActive": true
}
'@

# Service 4: Transport from Madinah to Makkah to Muzdalifah
$json4 = @'
{
  "nameEn": "Transport from Madinah to Makkah to Muzdalifah",
  "nameAr": "النقل من المدينة لمكة لمزدلفة",
  "descriptionEn": "Transportation service from Madinah to Makkah to Muzdalifah",
  "descriptionAr": "خدمة نقل من المدينة المنورة لمكة المكرمة لمزدلفة",
  "icon": "pi pi-directions",
  "serviceTypeCode": "TRANSPORT",
  "displayOrder": 4,
  "isActive": true
}
'@

# Service 5: Hotel to Airport Transfer
$json5 = @'
{
  "nameEn": "Hotel to Airport Transfer",
  "nameAr": "النقل من الفندق للمطار",
  "descriptionEn": "Private transfer from hotel to airport",
  "descriptionAr": "نقل خاص من الفندق إلى المطار",
  "icon": "pi pi-send",
  "serviceTypeCode": "TRANSPORT",
  "displayOrder": 5,
  "isActive": true
}
'@

# Service 6: Train Station to Hotel Transfer
$json6 = @'
{
  "nameEn": "Train Station to Hotel Transfer",
  "nameAr": "النقل من محطة القطار إلى الفندق",
  "descriptionEn": "Private transfer from train station to hotel",
  "descriptionAr": "نقل خاص من محطة القطار إلى الفندق",
  "icon": "pi pi-building",
  "serviceTypeCode": "TRANSPORT",
  "displayOrder": 6,
  "isActive": true
}
'@

$services = @($json1, $json2, $json3, $json4, $json5, $json6)
$count = 0

Write-Host "Seeding 6 paid services..." -ForegroundColor Cyan

foreach ($json in $services) {
    try {
        $response = Invoke-RestMethod -Uri $apiUrl -Method Post -Body $json -ContentType "application/json; charset=utf-8"
        $count++
        Write-Host "✓ Service $count created successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Service $($count+1) failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nVerifying..." -ForegroundColor Cyan
$allServices = Invoke-RestMethod -Uri $apiUrl -Method Get
Write-Host "Total services in database: $($allServices.Count)" -ForegroundColor Yellow
