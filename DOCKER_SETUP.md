# Docker Setup for MeetMe 2.0

This document explains how to run the MeetMe application using Docker and Docker Compose.

## Prerequisites

- Docker Desktop installed and running
- Docker Compose v2+

## Services

The application consists of three services:

1. **meetme-db**: SQL Server 2022 Express database
2. **meetme-api**: .NET 9.0 Web API backend
3. **meetme-client**: Angular frontend with Nginx

## Quick Start

### Production Mode

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

### Development Mode (with hot reload)

```bash
# Build and start all services in development mode
docker-compose -f docker-compose.dev.yml up -d

# View logs
docker-compose -f docker-compose.dev.yml logs -f

# Stop all services
docker-compose -f docker-compose.dev.yml down
```

## Service Access

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5000
- **Database**: localhost:1433 (sa/YourStrong@Passw0rd)

## Database Setup

The database will be automatically created when the API starts. If you need to run migrations manually:

```bash
# Enter the API container
docker-compose exec meetme-api bash

# Run migrations (if using EF Core)
dotnet ef database update
```

## Environment Variables

Key environment variables you can customize:

### Database
- `SA_PASSWORD`: SQL Server SA password (default: YourStrong@Passw0rd)

### API
- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Production)
- `ConnectionStrings__DefaultConnection`: Database connection string

## Volumes

- `meetme_db_data`: Persistent database storage
- `meetme_db_data_dev`: Development database storage

## Networks

All services communicate through the `meetme-network` bridge network.

## Troubleshooting

### Database Connection Issues
1. Wait for the database health check to pass
2. Check if the SA password meets SQL Server requirements
3. Ensure the connection string is correct

### API Not Starting
1. Check database is healthy: `docker-compose ps`
2. View API logs: `docker-compose logs meetme-api`
3. Verify connection string environment variables

### Frontend Build Issues
1. Check if node_modules are properly installed
2. Verify Angular build output in logs
3. Check nginx configuration

### Port Conflicts
If you have conflicts with the default ports, modify the port mappings in docker-compose.yml:

```yaml
ports:
  - "YOUR_PORT:CONTAINER_PORT"
```

## Development Tips

### Hot Reload
- Development mode enables hot reload for both frontend and backend
- Frontend changes will be reflected immediately
- Backend changes trigger automatic restart

### Debugging
- Use `docker-compose logs <service-name>` to view specific service logs
- Use `docker-compose exec <service-name> bash` to enter a container
- Set breakpoints in your IDE and attach to the running container

### Database Management
- Use SQL Server Management Studio or Azure Data Studio
- Connect to: localhost:1433, sa/YourStrong@Passw0rd
- Database name: MeetMeDb

## Cleanup

To remove everything including volumes:

```bash
# Stop and remove containers, networks, and volumes
docker-compose down -v

# Remove images (optional)
docker-compose down --rmi all -v
```
