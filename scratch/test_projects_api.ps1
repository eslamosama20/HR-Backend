try {
    $response = Invoke-RestMethod -Uri "http://localhost:5001/api/projects" -Method Get -SkipHeaderValidation
    Write-Host "Success! Response:"
    $response | ConvertTo-Json -Depth 5 | Write-Host
}
catch {
    Write-Host "Error status code: $($_.Exception.Response.StatusCode.Value__)"
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $body = $reader.ReadToEnd()
    Write-Host "Error Body: $body"
}
