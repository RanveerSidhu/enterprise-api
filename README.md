EnterpriseApp – ASP.NET Core Clean Architecture API

A production-ready ASP.NET Core Web API built using Clean Architecture, featuring JWT authentication with refresh tokens, role-based authorization, EF Core, SQL Server, and Dockerized deployment.

--------------------------------------------------
FEATURES
--------------------------------------------------
- Clean Architecture (API / Application / Domain / Infrastructure)
- RESTful CRUD APIs
- JWT Authentication
- Refresh Tokens
- Role-based Authorization
- Global Exception Handling
- Entity Framework Core + Migrations
- SQL Server integration
- Swagger / OpenAPI
- Dockerized (multi-stage build)

--------------------------------------------------
ARCHITECTURE
--------------------------------------------------
API
  - Controllers

Application
  - DTOs
  - Interfaces
  - Services

Domain
  - Entities
  - Interfaces

Infrastructure
  - Persistence (DbContext, Migrations)
  - Repositories

--------------------------------------------------
AUTHENTICATION & SECURITY
--------------------------------------------------
- JWT Access Tokens (short-lived)
- Refresh Tokens stored in database
- Role-based authorization using claims
- Stateless authentication
- Centralized exception handling

--------------------------------------------------
TECH STACK
--------------------------------------------------
- ASP.NET Core 9
- Entity Framework Core
- SQL Server
- JWT Bearer Authentication
- Swagger / OpenAPI
- Docker (Linux containers)

--------------------------------------------------
RUNNING LOCALLY (WITHOUT DOCKER)
--------------------------------------------------
1. Update connection string in appsettings.json
2. Apply migrations:
   dotnet ef database update
3. Run the API:
   dotnet run --project src/Api
4. Open Swagger:
   https://localhost:<port>/swagger

--------------------------------------------------
RUNNING WITH DOCKER
--------------------------------------------------
Build image:
docker build -t enterprise-api .

Run container:
docker run -d -p 8080:8080
-e ASPNETCORE_ENVIRONMENT=Development
-e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=EnterpriseAppDb;User Id=apiuser;Password=StrongPassword@123;TrustServerCertificate=True"
enterprise-api

Open Swagger:
http://localhost:8080/swagger

NOTE:
Docker uses SQL authentication. Windows authentication is not supported in Linux containers.

--------------------------------------------------
API ENDPOINTS
--------------------------------------------------
POST   /api/auth/login     - Login & get JWT
POST   /api/auth/refresh   - Refresh access token
POST   /api/auth/logout    - Logout
GET    /api/users          - Get users (Admin only)
POST   /api/users          - Create user
PUT    /api/users/{id}     - Update user
DELETE /api/users/{id}     - Delete user

--------------------------------------------------
KEY LEARNINGS
--------------------------------------------------
- Implemented JWT authentication with refresh tokens
- Applied role-based authorization
- Used EF Core migrations for schema versioning
- Solved Docker + SQL Server authentication issues
- Built a production-ready Docker image

--------------------------------------------------
AUTHOR
--------------------------------------------------
Ranveer Singh Sidhu
Full-Stack .NET Developer
Open to freelance, remote, and full-time opportunities
