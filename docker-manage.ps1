# MeetMe Docker Management Script for PowerShell
param(
    [Parameter(Position=0)]
    [string]$Command = "help",
    [Parameter(Position=1)]
    [string]$Service = ""
)

# Colors for output
function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Check if Docker is running
function Test-Docker {
    try {
        docker info | Out-Null
        Write-Success "Docker is running"
        return $true
    }
    catch {
        Write-Error "Docker is not running. Please start Docker Desktop."
        return $false
    }
}

# Production commands
function Start-Production {
    Write-Warning "Starting MeetMe in production mode..."
    if (-not (Test-Docker)) { return }
    
    docker-compose up -d
    Write-Success "Services started. Access the app at http://localhost:4200"
}

function Stop-Production {
    Write-Warning "Stopping production services..."
    docker-compose down
    Write-Success "Services stopped"
}

function Show-ProductionLogs {
    docker-compose logs -f
}

# Development commands
function Start-Development {
    Write-Warning "Starting MeetMe in development mode..."
    if (-not (Test-Docker)) { return }
    
    docker-compose -f docker-compose.dev.yml up -d
    Write-Success "Development services started with hot reload"
    Write-Success "Frontend: http://localhost:4200"
    Write-Success "Backend: http://localhost:5000"
}

function Stop-Development {
    Write-Warning "Stopping development services..."
    docker-compose -f docker-compose.dev.yml down
    Write-Success "Development services stopped"
}

function Show-DevelopmentLogs {
    docker-compose -f docker-compose.dev.yml logs -f
}

# Database commands
function Reset-Database {
    Write-Warning "Resetting database (this will delete all data)..."
    $confirmation = Read-Host "Are you sure? (y/N)"
    
    if ($confirmation -eq 'y' -or $confirmation -eq 'Y') {
        docker-compose down -v
        docker volume rm meetme-20_meetme_db_data 2>$null
        docker volume rm meetme-20_meetme_db_data_dev 2>$null
        Write-Success "Database reset complete"
    }
    else {
        Write-Warning "Database reset cancelled"
    }
}

# Build commands
function Build-All {
    Write-Warning "Building all services..."
    docker-compose build --no-cache
    Write-Success "All services built"
}

function Build-Api {
    Write-Warning "Building API service..."
    docker-compose build --no-cache meetme-api
    Write-Success "API service built"
}

function Build-Client {
    Write-Warning "Building client service..."
    docker-compose build --no-cache meetme-client
    Write-Success "Client service built"
}

# Cleanup commands
function Invoke-Cleanup {
    Write-Warning "Cleaning up Docker resources..."
    docker-compose down --rmi all -v
    docker system prune -f
    Write-Success "Cleanup complete"
}

# Status command
function Show-Status {
    Write-Host "MeetMe Docker Status:" -ForegroundColor Cyan
    Write-Host "====================" -ForegroundColor Cyan
    docker-compose ps
    Write-Host ""
    Write-Host "Volumes:"
    docker volume ls | Select-String "meetme"
    Write-Host ""
    Write-Host "Networks:"
    docker network ls | Select-String "meetme"
}

# Debug commands
function Show-Logs {
    param([string]$Service = "")
    
    if ($Service) {
        Write-Warning "Showing logs for service: $Service"
        docker-compose logs -f $Service
    } else {
        Write-Warning "Showing logs for all services..."
        docker-compose logs -f
    }
}

function Enter-Container {
    param([string]$Service)
    
    if (-not $Service) {
        Write-Error "Please specify a service: api, client, or db"
        Write-Host "Examples:"
        Write-Host "  .\docker-manage.ps1 shell api"
        Write-Host "  .\docker-manage.ps1 shell client"
        Write-Host "  .\docker-manage.ps1 shell db"
        return
    }
    
    $containerName = switch ($Service) {
        "api" { "meetme-backend" }
        "client" { "meetme-frontend" } 
        "db" { "meetme-database" }
        default { $Service }
    }
    
    Write-Warning "Entering container: $containerName"
    docker exec -it $containerName /bin/bash
}

function Test-Services {
    Write-Host "Testing MeetMe Services:" -ForegroundColor Cyan
    Write-Host "========================" -ForegroundColor Cyan
    
    # Test database
    Write-Host ""
    Write-Host "Testing Database Connection..."
    try {
        $dbTest = docker exec meetme-database /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1" 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Database is accessible"
        } else {
            Write-Error "Database connection failed"
        }
    } catch {
        Write-Error "Database container not running"
    }
    
    # Test API
    Write-Host ""
    Write-Host "Testing API..."
    try {
        $apiTest = Invoke-WebRequest -Uri "http://localhost:5000/health" -Method GET -TimeoutSec 5 2>$null
        if ($apiTest.StatusCode -eq 200) {
            Write-Success "API is responding"
        } else {
            Write-Warning "API returned status: $($apiTest.StatusCode)"
        }
    } catch {
        Write-Error "API is not responding at http://localhost:5000"
    }
    
    # Test Frontend
    Write-Host ""
    Write-Host "Testing Frontend..."
    try {
        $frontendTest = Invoke-WebRequest -Uri "http://localhost:4200" -Method GET -TimeoutSec 5 2>$null
        if ($frontendTest.StatusCode -eq 200) {
            Write-Success "Frontend is responding"
        } else {
            Write-Warning "Frontend returned status: $($frontendTest.StatusCode)"
        }
    } catch {
        Write-Error "Frontend is not responding at http://localhost:4200"
    }
}

# Help command
function Show-Help {
    Write-Host "MeetMe Docker Management" -ForegroundColor Cyan
    Write-Host "=======================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usage: .\docker-manage.ps1 [command] [service]"
    Write-Host ""
    Write-Host "Production Commands:"
    Write-Host "  prod:up     - Start production services"
    Write-Host "  prod:down   - Stop production services"
    Write-Host "  prod:logs   - View production logs"
    Write-Host ""
    Write-Host "Development Commands:"
    Write-Host "  dev:up      - Start development services (with hot reload)"
    Write-Host "  dev:down    - Stop development services"
    Write-Host "  dev:logs    - View development logs"
    Write-Host ""
    Write-Host "Database Commands:"
    Write-Host "  db:reset    - Reset database (deletes all data)"
    Write-Host ""
    Write-Host "Build Commands:"
    Write-Host "  build:all   - Build all services"
    Write-Host "  build:api   - Build API service only"
    Write-Host "  build:client - Build client service only"
    Write-Host ""
    Write-Host "Utility Commands:"
    Write-Host "  status      - Show service status"
    Write-Host "  test        - Test all services connectivity"
    Write-Host "  logs [service] - Show logs (optional: specify service)"
    Write-Host "  shell [service] - Enter container shell (api/client/db)"
    Write-Host "  cleanup     - Remove all containers, images, and volumes"
    Write-Host "  help        - Show this help message"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\docker-manage.ps1 dev:up"
    Write-Host "  .\docker-manage.ps1 prod:logs"
    Write-Host "  .\docker-manage.ps1 logs api"
    Write-Host "  .\docker-manage.ps1 shell api"
    Write-Host "  .\docker-manage.ps1 test"
    Write-Host "  .\docker-manage.ps1 status"
}

# Main command handler
switch ($Command) {
    "prod:up" { Start-Production }
    "prod:down" { Stop-Production }
    "prod:logs" { Show-ProductionLogs }
    "dev:up" { Start-Development }
    "dev:down" { Stop-Development }
    "dev:logs" { Show-DevelopmentLogs }
    "db:reset" { Reset-Database }
    "build:all" { Build-All }
    "build:api" { Build-Api }
    "build:client" { Build-Client }
    "status" { Show-Status }
    "test" { Test-Services }
    "logs" { Show-Logs -Service $Service }
    "shell" { Enter-Container -Service $Service }
    "cleanup" { Invoke-Cleanup }
    "help" { Show-Help }
    default {
        Write-Error "Unknown command: $Command"
        Show-Help
    }
}
