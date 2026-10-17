import requests
import json

url = "http://localhost:5001/api/v1/pharmacy/restricted-drugs/dispense"

token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkBhZG1pbi5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiMGNkOGRlMTEtMTE3Mi00MDdiLWRkM2MtMDhkZGE4YTNlZGM3IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZ2l2ZW5uYW1lIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9zdXJuYW1lIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOlsiU3lzdGVtLkFkbWluIiwiQWRtaW4iXSwiZXhwIjoxNzgwNDc2MDI1LCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo0NDM2MC8ifQ.nD6KJ5HXLz6fEmkgAxQoz5nhsfQOVk0LL1wyCidq8x4"

headers = {
    "Content-Type": "application/json",
    "Authorization": f"Bearer {token}"
}

payload = {
  "itemCode": "DRUG-1002",
  "itemName": "Morphine 10mg",
  "quantity": 1.0,
  "batchNo": "B-MOR-01",
  "storeDetailsId": "73eeb854-1206-4af1-9564-08ddce0a8073",
  "patientName": "Jane Doe",
  "doctorName": "Dr. House",
  "pharmacistName": "Pharmacist Dave",
  "authorizationNo": "AUTH-9921",
  "dispenseReason": "Post-operative pain relief",
  "restrictionType": "Narcotics"
}

resp = requests.post(url, json=payload, headers=headers)
print("Status Code:", resp.status_code)
print("Response Body:", resp.text)
