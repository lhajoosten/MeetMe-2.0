# 🎉 MeetMe 2.0

A modern, enterprise-grade event management platform built with Clean Architecture and Domain-Driven Design principles. MeetMe enables seamless event creation, attendee management, and real-time collaboration for meetups and gatherings. This project showcases best practices in software architecture, testing, and API design using .NET 9. The platform is rewritten from an old legacy school project to a new, robust architecture that emphasizes maintainability, scalability, and performance.

## ✨ Key Features

### 🎯 Event Management
- **Create & Schedule Events**: Rich event creation with location, datetime, and capacity management
- **Attendance Management**: Join/leave events with status tracking (Confirmed, Maybe, Not Attending)
- **Smart Capacity Control**: Automatic capacity validation and attendee limits
- **Event Discovery**: Advanced filtering and search capabilities
- **Real-time Updates**: Domain events for immediate system responsiveness

### 👥 User Experience
- **Role-based Access**: Creator and attendee permissions
- **Profile Management**: Comprehensive user profiles with bio and contact info
- **Event Dashboard**: Personalized views for created and joined events
- **Activity Tracking**: Complete audit trail for all user actions

### 🏗️ Technical Excellence
- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and API layers
- **CQRS Pattern**: Command Query Responsibility Segregation for optimal performance
- **Domain Events**: Event-driven architecture for loose coupling
- **Comprehensive Testing**: Unit, integration, and domain tests with 95%+ coverage
- **API-First Design**: RESTful APIs with OpenAPI/Swagger documentation

## 🏛️ Architecture

```
src/
├── MeetMe.API/              # Web API Layer (Controllers, Configuration)
├── MeetMe.Application/      # Application Layer (CQRS, DTOs, Validation)
├── MeetMe.Domain/          # Domain Layer (Entities, Value Objects, Events)
└── MeetMe.Infrastructure/  # Infrastructure Layer (Data Access, External Services)

tests/
├── MeetMe.Domain.Tests/     # Domain Logic Unit Tests
├── MeetMe.Application.Tests/# Application Layer Tests
└── MeetMe.API.Tests/       # API Integration Tests
```

### 🎯 Domain Model
- **Meeting**: Core aggregate with location, datetime, and capacity
- **User**: Identity management with roles and profiles
- **Attendance**: Join/leave tracking with status management
- **Value Objects**: Email, Location, MeetingDateTime for data integrity
- **Domain Events**: MeetingCreated, UserJoined, UserLeft events

## 🛠️ Tech Stack

### Backend (.NET 9)
- **ASP.NET Core Web API** - RESTful API endpoints
- **Entity Framework Core** - Data access and migrations  
- **MediatR** - CQRS and request/response pipeline
- **FluentValidation** - Input validation and business rules
- **AutoMapper** - Object-to-object mapping
- **JWT Authentication** - Secure token-based auth
- **Serilog** - Structured logging and diagnostics

### Database
- **SQL Server** - Primary data store
- **Entity Framework Migrations** - Database versioning
- **Domain-Driven Design** - Rich domain models with business logic

### Testing & Quality
- **xUnit** - Unit and integration testing framework
- **FluentAssertions** - Expressive test assertions
- **Moq** - Mocking framework for dependencies
- **Test Coverage** - Comprehensive test suite with high coverage

## 📡 API Endpoints

### Meetings
```http
GET    /api/meetings                 # Get all meetings (with filtering)
POST   /api/meetings                 # Create new meeting
GET    /api/meetings/{id}            # Get meeting details
PUT    /api/meetings/{id}            # Update meeting
DELETE /api/meetings/{id}            # Cancel meeting
```

### Attendance
```http
POST   /api/attendances/join         # Join a meeting
POST   /api/attendances/leave        # Leave a meeting
GET    /api/attendances/my-meetings  # Get user's meetings
```

### Users & Authentication
```http
POST   /api/auth/login               # User authentication
POST   /api/auth/register            # User registration
GET    /api/users/profile            # Get user profile
PUT    /api/users/profile            # Update user profile
```

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Setup Instructions

1. **Clone the repository**
```bash
git clone https://github.com/lhajoosten/MeetMe-2.0.git
cd MeetMe-2.0
```

2. **Configure Database**
```bash
# Update connection string in appsettings.json
# Run database migrations
dotnet ef database update --project src/MeetMe.Infrastructure
```

3. **Configure JWT Settings**
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-here",
    "Issuer": "MeetMe.API",
    "Audience": "MeetMe.Client",
    "ExpirationInHours": 24
  }
}
```

4. **Run the Application**
```bash
# Start the API
dotnet run --project src/MeetMe.API

# API will be available at https://localhost:5001
# Swagger UI at https://localhost:5001/index.html
```

5. **Run Tests**
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🧪 Testing Strategy

### Domain Tests
- Entity behavior and business rules
- Value object validation
- Domain event verification
- Aggregate consistency checks

### Application Tests  
- Command and query handlers
- Validation logic
- Service integrations
- Repository patterns

### API Tests
- Controller endpoints
- Authentication flows
- Request/response validation
- Error handling scenarios

## 📚 Key Design Patterns

### Clean Architecture
- **Domain Layer**: Business entities and rules
- **Application Layer**: Use cases and orchestration  
- **Infrastructure Layer**: External concerns and persistence
- **API Layer**: Controllers and HTTP concerns

### CQRS (Command Query Responsibility Segregation)
- **Commands**: Write operations (Create, Update, Delete)
- **Queries**: Read operations with optimized DTOs
- **Handlers**: Dedicated handlers for each operation
- **Validation**: FluentValidation for input validation

### Domain-Driven Design
- **Aggregates**: Consistency boundaries (Meeting, User)
- **Value Objects**: Immutable data containers
- **Domain Events**: Decoupled communication
- **Repository Pattern**: Data access abstraction

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow Clean Architecture principles
- Write comprehensive tests for new features
- Use domain events for cross-aggregate communication
- Maintain high code coverage (>90%)
- Follow SOLID principles and DDD patterns

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Clean Architecture by Robert C. Martin
- Domain-Driven Design by Eric Evans
- .NET Community for excellent tooling and frameworks
