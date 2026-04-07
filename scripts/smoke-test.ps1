param(
    [string]$BaseUrl = "http://localhost:8080"
)

$ErrorActionPreference = "Stop"

Write-Host "Running MemberCare smoke test against $BaseUrl" -ForegroundColor Cyan

# 1) Health check
$health = Invoke-RestMethod -Uri "$BaseUrl/v1/health" -Method Get
Write-Host "Health: $($health.status), DB: $($health.database)"

# 2) Login
$authBody = @{ username = "admin"; password = "test" } | ConvertTo-Json
$auth = Invoke-RestMethod -Uri "$BaseUrl/v1/auth/login" -Method Post -Headers @{ "Content-Type" = "application/json" } -Body $authBody
if (-not $auth.accessToken) { throw "Login failed: no access token returned" }
Write-Host "Login: ok"

$headers = @{ "Authorization" = "Bearer $($auth.accessToken)" }

# 3) Authorized endpoint checks
$dashboard = Invoke-RestMethod -Uri "$BaseUrl/v1/dashboard/summary" -Method Get -Headers $headers
$members = Invoke-RestMethod -Uri "$BaseUrl/v1/members" -Method Get -Headers $headers

Write-Host "Dashboard summary:" -ForegroundColor Green
$dashboard | ConvertTo-Json -Depth 5

Write-Host "Members totalCount: $($members.totalCount)"
Write-Host "Smoke test completed successfully." -ForegroundColor Green
