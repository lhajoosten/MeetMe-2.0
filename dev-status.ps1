# MeetMe Development Startup Script

Write-Host "Starting MeetMe Development Environment..." -ForegroundColor Green

# Function to check if a port is in use
function Test-Port {
    param([int]$Port)
    try {
        $connection = New-Object System.Net.Sockets.TcpClient("localhost", $Port)
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

# Check if API is running
Write-Host "Checking API status..." -ForegroundColor Yellow
if (Test-Port 7114) {
    Write-Host "API is running on https://localhost:7114" -ForegroundColor Green
} else {
    Write-Host "API is not running. Please start the API first:" -ForegroundColor Red
    Write-Host "   1. Open terminal in src/MeetMe.API" -ForegroundColor White
    Write-Host "   2. Run: dotnet run" -ForegroundColor White
    Write-Host ""
}

# Check if Angular dev server is running
Write-Host "Checking Angular dev server status..." -ForegroundColor Yellow
if (Test-Port 4200) {
    Write-Host "Angular dev server is running on http://localhost:4200" -ForegroundColor Green
} else {
    Write-Host "Angular dev server is not running. Please start it:" -ForegroundColor Red
    Write-Host "   1. Open terminal in src/MeetMe.Client" -ForegroundColor White
    Write-Host "   2. Run: npm run start" -ForegroundColor White
    Write-Host ""
}

Write-Host ""
Write-Host "Configuration Summary:" -ForegroundColor Cyan
Write-Host "- Frontend: http://localhost:4200" -ForegroundColor White
Write-Host "- API: https://localhost:7114" -ForegroundColor White
Write-Host "- Proxy: /api/* -> https://localhost:7114" -ForegroundColor White
Write-Host "- CORS: Enabled for development" -ForegroundColor White
Write-Host ""
Write-Host "Available endpoints:" -ForegroundColor Cyan
Write-Host "- Swagger UI: https://localhost:7114/swagger" -ForegroundColor White
Write-Host "- Auth Login: POST /api/auth/login" -ForegroundColor White
Write-Host "- Auth Register: POST /api/auth/register" -ForegroundColor White
Write-Host "- Meetings: GET /api/meetings" -ForegroundColor White
Write-Host ""

if ((Test-Port 7114) -and (Test-Port 4200)) {
    Write-Host "Both services are running! You can now test the application." -ForegroundColor Green
    Write-Host "- Frontend: http://localhost:4200" -ForegroundColor White
    Write-Host "- API Docs: https://localhost:7114/index.html" -ForegroundColor White
} else {
    Write-Host "Please start the missing services before testing." -ForegroundColor Yellow
}
