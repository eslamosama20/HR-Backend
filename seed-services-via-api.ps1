# Script to seed paid services via API
$apiBaseUrl = "http://localhost:5001/api/PublicPaidService"

# Get token from localStorage (user needs to be logged in)
Write-Host "Note: Make sure you are logged in to the admin panel first!" -ForegroundColor Yellow

$services = @(
    @{
        nameEn = "Transportation between Makkah and Madinah"
        nameAr = "نقل بين مكة والمدينة"
        descriptionEn = "Comfortable transportation between the two holy cities"
        descriptionAr = "خدمة نقل مريحة بين الحرمين الشريفين"
        icon = "pi pi-car"
        serviceTypeCode = "TRANSPORT"
        displayOrder = 1
        isActive = $true
    },
    @{
        nameEn = "Madinah Tours"
        nameAr = "مزارات المدينة"
        descriptionEn = "Guided tours of historical sites in Madinah"
        descriptionAr = "جولات إرشادية للمواقع التاريخية في المدينة المنورة"
        icon = "pi pi-compass"
        serviceTypeCode = "TOUR"
        displayOrder = 2
        isActive = $true
    },
    @{
        nameEn = "Makkah Tours"
        nameAr = "مزارات مكة"
        descriptionEn = "Guided tours of historical sites in Makkah"
        descriptionAr = "جولات إرشادية للمواقع التاريخية في مكة المكرمة"
        icon = "pi pi-map-marker"
        serviceTypeCode = "TOUR"
        displayOrder = 3
        isActive = $true
    },
    @{
        nameEn = "Transport from Madinah to Makkah to Muzdalifah"
        nameAr = "النقل من المدينة لمكة لمزدلفة"
        descriptionEn = "Transportation service from Madinah to Makkah to Muzdalifah"
        descriptionAr = "خدمة نقل من المدينة المنورة لمكة المكرمة لمزدلفة"
        icon = "pi pi-directions"
        serviceTypeCode = "TRANSPORT"
        displayOrder = 4
        isActive = $true
    },
    @{
        nameEn = "Hotel to Airport Transfer"
        nameAr = "النقل من الفندق للمطار"
        descriptionEn = "Private transfer from hotel to airport"
        descriptionAr = "نقل خاص من الفندق إلى المطار"
        icon = "pi pi-send"
        serviceTypeCode = "TRANSPORT"
        displayOrder = 5
        isActive = $true
    },
    @{
        nameEn = "Train Station to Hotel Transfer"
        nameAr = "النقل من محطة القطار إلى الفندق"
        descriptionEn = "Private transfer from train station to hotel"
        descriptionAr = "نقل خاص من محطة القطار إلى الفندق"
        icon = "pi pi-building"
        serviceTypeCode = "TRANSPORT"
        displayOrder = 6
        isActive = $true
    }
)

Write-Host "Creating $($services.Count) paid services..." -ForegroundColor Cyan

foreach ($service in $services) {
    $body = $service | ConvertTo-Json -Depth 10
    
    try {
        # For testing, we'll call without authorization
        # In production, you need to add Authorization header
        $response = Invoke-RestMethod -Uri $apiBaseUrl -Method Post -Body $body -ContentType "application/json"
        Write-Host "✓ Created: $($service.nameEn)" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Failed: $($service.nameEn) - $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nDone! Verifying..." -ForegroundColor Cyan
$allServices = Invoke-RestMethod -Uri $apiBaseUrl -Method Get
Write-Host "Total services in database: $($allServices.Count)" -ForegroundColor Yellow
