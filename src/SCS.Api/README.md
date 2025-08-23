# Smart City Surveillance (SCS) API

A comprehensive Smart City Surveillance API built with .NET 9, featuring real-time alarm system monitoring, premise management, and user authentication.

## Prerequisites

- **.NET 9 SDK** - Download from [Microsoft .NET](https://dotnet.microsoft.com/download/dotnet/9.0)
- **PostgreSQL Database** - Local installation or cloud instance
- **AWS Account** - For S3, SES, and SQS services
- **Git** - For version control

## Project Structure

```
SCS.Api/
├── SCS.Api.App/          # Main API application
├── SCS.Api.Domain/       # Domain models and entities
├── SCS.Api.UnitTests/    # Unit tests
└── SCS.Api.sln          # Solution file
```

## Configuration

### Required Settings

Before running the application, you need to configure the following settings in `appsettings.json` or use User Secrets for sensitive data.

#### 1. Database Connection
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=your-server;Database=your-database;User Id=your-username;Password=your-password;TrustServerCertificate=True"
}
```

#### 2. JWT Authentication Settings
```json
"JwtSettings": {
  "Secret": "your-jwt-secret-key-minimum-32-characters",
  "ExpiryMinutes": 600,
  "Issuer": "SCS.Api",
  "Audience": "SCS.Api"
}
```

#### 3. AWS Configuration
```json
"AWS": {
  "Region": "your-aws-region",
  "AccessKey": "your-aws-access-key",
  "SecretKey": "your-aws-secret-key",
  "QueueUrl": "your-sqs-queue-url",
  "BucketName": "your-s3-bucket-name"
}
```

#### 4. Email Configuration
```json
"Email": {
  "From": "your-email@domain.com"
}
```

### Using User Secrets (Recommended for Development)

For security, use User Secrets to store sensitive configuration:

```bash
# Navigate to the API project directory
cd SCS.Api.App

# Set database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=scs;User Id=postgres;Password=yourpassword;TrustServerCertificate=True"

# Set JWT secret
dotnet user-secrets set "JwtSettings:Secret" "your-secure-jwt-secret-key-32-chars-min"

# Set AWS credentials (if using AWS services)
dotnet user-secrets set "AWS:AccessKey" "your-aws-access-key"
dotnet user-secrets set "AWS:SecretKey" "your-aws-secret-key"

# Set email configuration
dotnet user-secrets set "Email:From" "your-email@domain.com"
```

## Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/huynguyen-95/SCS.git Smart-City-Surveillance
cd Smart-City-Surveillance/src/SCS.Api
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Database Setup
Ensure your PostgreSQL database is running and the connection string is configured correctly.

### 4. Run the Application

#### Development (HTTP)
```bash
cd SCS.Api.App
dotnet run --launch-profile http
```
The API will be available at: `http://localhost:5158`

#### Development (HTTPS)
```bash
cd SCS.Api.App
dotnet run --launch-profile https
```
The API will be available at: `https://localhost:7236` and `http://localhost:5158`

#### Using Solution File
```bash
# From the solution root directory
dotnet run --project SCS.Api.App
```

### 5. Access API Documentation
When running in development mode, you can access the API documentation at:
- **Scalar UI**: `https://localhost:7236/scalar/v1` (HTTPS profile)
- **OpenAPI**: `https://localhost:7236/openapi/v1.json`

### 6. SignalR Hub Connection
The application includes a SignalR hub for real-time alarm system notifications:
- **Hub Endpoint**: `wss://localhost:7236/alarm-system-hub` (HTTPS) or `ws://localhost:5158/alarm-system-hub` (HTTP)
- **Connection**: Connect with `groupId` query parameter to receive premise-specific notifications
- **Example**: `wss://localhost:7236/alarm-system-hub?groupId=1`

## Running Tests

### Run All Tests
```bash
# From solution root
dotnet test

# Or specifically target the test project
dotnet test SCS.Api.UnitTests
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Test Categories
```bash
# Run tests from a specific namespace
dotnet test --filter "FullyQualifiedName~SCS.Api.UnitTests.Features"

# Run tests with specific display name pattern
dotnet test --filter "DisplayName~Authentication"
```

## Development

### Building the Solution
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build SCS.Api.App

# Build for release
dotnet build --configuration Release
```

### Publishing the Application
```bash
# Publish for deployment
dotnet publish SCS.Api.App --configuration Release --output ./publish

# Or use the configured task
dotnet publish SCS.Api.App/SCS.Api.App.csproj --configuration Release --output "D:\Projects\PublishApps\SCS-Api"
```

## Docker Deployment

### Prerequisites for Docker
- **Docker Desktop** - Download from [Docker](https://www.docker.com/products/docker-desktop/)
- Ensure Docker is running on your system

### Building the Docker Image

#### Build from Solution Root (Recommended)
```bash
# Navigate to the solution root directory (contains both SCS.Api.App and SCS.Api.Domain)
cd /path/to/Smart-City-Surveillance/src/SCS.Api

# Build the Docker image using the Dockerfile in SCS.Api.App
docker build -f SCS.Api.App/Dockerfile -t scs-api-app .
```

### Running the Docker Container

#### Basic Run (Development Environment)
```bash
# Run with default settings (Development environment with Scalar API docs)
docker run -d -p 8080:8080 -p 8081:8081 --name scs-api-app scs-api-app
```

#### Run with Custom Environment Variables
```bash
# Override specific configuration values at runtime
docker run -d -p 8080:8080 -p 8081:8081 \
  -e JwtSettings__Secret="your-custom-jwt-secret" \
  -e ConnectionStrings__DefaultConnection="your-custom-db-connection" \
  -e AWS__Region="us-west-2" \
  -e AWS__BucketName="your-custom-bucket" \
  --name scs-api-app scs-api-app
```

### Accessing the Dockerized Application

Once the container is running, you can access:

- **API Base URL**: http://localhost:8080
- **API Documentation (Development only)**: http://localhost:8080/scalar/v1
- **OpenAPI JSON**: http://localhost:8080/openapi/v1.json
- **SignalR Hub**: ws://localhost:8080/hubs/alarm-system

### Docker Configuration Details

The Dockerfile is configured with:
- **Environment**: Development (enables Scalar API documentation)
- **Ports**: 8080 (HTTP) and 8081 (HTTPS)
- **Configuration Override**: Environment variables override `appsettings.json` values
- **Multi-stage Build**: Optimized for production deployment

#### Environment Variables in Docker

The following environment variables are pre-configured in the Docker image and can be overridden:

```bash
# AWS Configuration
AWS__Region=ap-southeast-1
AWS__AccessKey=<your-access-key>
AWS__SecretKey=<your-secret-key>
AWS__QueueUrl=<your-queue-url>
AWS__BucketName=<your-bucket-name>

# Database Configuration
ConnectionStrings__DefaultConnection=<your-connection-string>

# Application Environment
ASPNETCORE_ENVIRONMENT=Development
```

## Features

- **Authentication & Authorization**: JWT-based authentication system
- **Alarm System**: Real-time alarm monitoring and alerts
- **Premise Management**: CRUD operations for premises and incidents
- **User Management**: User registration and profile management
- **Security Guard Operations**: Role-based access for security personnel
- **Real-time Communication**: SignalR hub for live updates
- **File Upload**: AWS S3 integration for file storage
- **Email Notifications**: AWS SES integration for email services
- **Message Queuing**: AWS SQS integration for asynchronous processing

## API Endpoints

### Authentication
- `POST /api/authentication/login` - User authentication and JWT token generation

### Alarm System
- `POST /api/alarm-system/simulate-alert` - Simulate alarm system alerts

### Premises
- `GET /api/premise` - Get list of all premises
- `GET /api/premise/{id}/incidents` - Get incident list for a specific premise

### Security Guard Operations
- `POST /api/security-guard/incidents/{premiseId}` - Capture an incident for a premise (with file upload)
- `POST /api/security-guard/dispatch-to-premise` - Dispatch security guard to a premise

### User Management
- `GET /api/users` - Get list of all users
- `POST /api/users` - Add a new user to the system

### SignalR Real-time Communication
- **Hub**: `/alarm-system-hub` - Real-time alarm notifications
- **Connection**: Connect with `groupId` query parameter to receive premise-specific alerts
- **Example**: `wss://localhost:7236/alarm-system-hub?groupId=1`
- **Events**: Receives live alarm system alerts and status updates

## Technologies Used

- **.NET 9** - Core framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM with PostgreSQL
- **JWT Authentication** - Security
- **SignalR** - Real-time communication
- **FluentValidation** - Input validation
- **AWS SDK** - Cloud services integration
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework