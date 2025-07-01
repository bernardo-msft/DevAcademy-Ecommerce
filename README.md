# Ecommerce API Project

This project is a backend API for an e-commerce platform, built with .NET and following Clean Architecture principles. It includes features like JWT authentication, data persistence with Entity Framework Core, caching with Redis, and file handling with Azure Blob Storage.

## Architecture

The solution is structured using **Clean Architecture**, which separates concerns and makes the system more maintainable, testable, and scalable. The core principle is the **Dependency Rule**, which states that source code dependencies can only point inwards. Nothing in an inner layer can know anything at all about something in an outer layer.

-   **`Ecommerce.Domain`**: This is the core of the application. It contains all the business entities (e.g., `User`, `Product`), value objects, enums, and domain-specific logic. It has no dependencies on any other layer.
-   **`Ecommerce.Application`**: This layer contains the application's use cases and business logic. It orchestrates the data flow between the API and the Domain, using DTOs (Data Transfer Objects) and service interfaces (e.g., [`IAuthService`](src/Ecommerce.Application/Interfaces/Services/IAuthService.cs)). It depends on the Domain layer but is independent of the UI and Infrastructure.
-   **`Ecommerce.Infrastructure`**: This layer handles all external concerns, such as database access (Entity Framework), caching (Redis), and file storage (Azure Blob Storage). It provides concrete implementations for the interfaces defined in the Application layer. It depends on the Application layer.
-   **`Ecommerce.API`**: This is the presentation layer, which exposes the application's functionality through a RESTful API. It handles HTTP requests, routes them to the appropriate services in the Application layer, and manages user authentication and authorization.

## Features

-   **RESTful API**: A complete set of endpoints for managing products, users, and other e-commerce functionalities.
-   **JWT Authentication**: Secure endpoints using JSON Web Tokens (JWT) for authentication and authorization.
-   **EF Core & SQL Server**: Data persistence is handled using Entity Framework Core with a SQL Server database.
-   **Redis Caching**: Integrated with Redis for caching and for maintaining a token blacklist to securely handle user logouts.
-   **Azure Blob Storage**: Manages file storage, such as product images, using Azure Blob Storage.
-   **Dependency Injection**: Extensively uses .NET's built-in dependency injection to manage dependencies and promote loose coupling.

## Core Concepts and Design Patterns

This project leverages several design patterns to ensure a clean, scalable, and maintainable codebase.

### Repository and Unit of Work Patterns

-   **Repository Pattern**: This pattern abstracts the data access logic. Instead of scattering data access code (like EF Core queries) throughout the application, we define repository interfaces in the `Application` layer (e.g., `IUserRepository`) and implement them in the `Infrastructure` layer. This decouples the business logic from the data access technology.
-   **Unit of Work Pattern**: This pattern manages atomic operations involving multiple repositories. The [`IUnitOfWork`](src/Ecommerce.Application/Interfaces/IUnitOfWork.cs) interface groups multiple repository operations into a single transaction. When `CompleteAsync()` is called, all changes are saved to the database together, ensuring data consistency.

### Dependency Injection (DI)

Dependency Injection is used throughout the application to achieve Inversion of Control (IoC). Instead of creating dependencies, classes receive them through their constructor.

-   **Configuration**: Services are registered in the DI container in the `DependencyInjection.cs` files for each layer and in the `Program.cs` file for the API. For example, in [`AuthService`](src/Ecommerce.Application/Services/AuthService.cs), `IUnitOfWork`, `IPasswordHasher`, and `ITokenService` are injected via the constructor.
-   **Benefits**: This makes the code more modular, easier to test (by allowing mock implementations to be injected), and more flexible.

### Service Layer Pattern

The `Ecommerce.Application` project acts as a **Service Layer**. It encapsulates the application's business logic and use cases. Services like [`AuthService`](src/Ecommerce.Application/Services/AuthService.cs) orchestrate operations, calling repositories and other services to fulfill a request from the presentation layer.

### Options Pattern

The Options pattern is used to manage application settings. Configuration from [`appsettings.json`](src/Ecommerce.API/appsettings.json) is bound to strongly-typed classes (e.g., `JwtSettings`, `AzureStorage`). This is configured in `Program.cs` and allows services to access configuration in a type-safe manner by injecting `IOptions<T>`.

### Data Transfer Objects (DTOs)

DTOs are used to transfer data between layers, especially between the `Application` and `API` layers.
-   **Purpose**: They prevent exposing internal domain models directly to the client. This helps to avoid over-posting vulnerabilities and allows the API's data contract to evolve independently of the domain model. Examples include `UserDto`, `UserRegistrationDto`, and `UserLoginDto`.

## Authentication and Authorization

Authentication is implemented using **JSON Web Tokens (JWT)**.

### Authentication Flow

1.  **Registration**: A new user is registered via the [`RegisterAsync`](src/Ecommerce.Application/Services/AuthService.cs) method in `AuthService`. The password is not stored in plain text; instead, a hash is generated and saved.
2.  **Login**: The user provides credentials, which are validated by the [`LoginAsync`](src/Ecommerce.Application/Interfaces/Services/IAuthService.cs) method.
3.  **Token Generation**: If credentials are valid, the `TokenService` generates a JWT. This token contains claims about the user, such as their ID, email, and role.
4.  **Token Usage**: The client receives the JWT and must include it in the `Authorization` header of subsequent requests to protected endpoints (e.g., `Authorization: Bearer <token>`).
5.  **Token Validation**: The ASP.NET Core JWT Bearer authentication middleware, configured in [`Program.cs`](src/Ecommerce.API/Program.cs), automatically validates the token on each request. It checks the signature, expiration, issuer, and audience.
6.  **Logout and Token Blacklisting**: To handle logouts securely, the JWT is added to a blacklist in Redis when the user logs out. A custom service checks this blacklist on each request to ensure that a logged-out token cannot be reused, even if it hasn't expired.

## Getting Started

### Prerequisites

-   .NET 9 SDK 
-   SQL Server (LocalDB is configured by default)
-   Redis instance 
-   Azure Storage Account

### Configuration

1.  Navigate to `src/Ecommerce.API/`.
2.  The file [`appsettings.Development.json`](src/Ecommerce.API/appsettings.Development.json) contains the development-specific configuration. Make sure the connection strings and other settings are correctly configured for your local environment.

    **Example `appsettings.Development.json`:**
    ```json
    {
      "ConnectionStrings": {
        "EcommerceConnection": "Server=(localdb)\\mssqllocaldb;Database=EcommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true",
        "RedisConnection": "YOUR_REDIS_CONNECTION_STRING"
      },
      "JwtSettings": {
        "Secret": "YOUR_SUPER_SECRET_KEY_THAT_IS_LONG_ENOUGH",
        "Issuer": "EcommerceAPI",
        "Audience": "EcommerceAPIClient",
        "MinutesToExpire": "60"
      },
      "AzureStorage": {
        "ConnectionString": "YOUR_AZURE_STORAGE_CONNECTION_STRING",
        "ContainerName": "products"
      }
    }
    ```

### Running the Application

1.  Open a terminal in the root directory.
2.  Run the API project:
    ```sh
    dotnet run --project src/Ecommerce.API/Ecommerce.API.csproj
    ```
3.  The API will be available at `https://localhost:port` and `http://localhost:port`, where `port` is specified in `src/Ecommerce.API/Properties/launchSettings.json`.

## Project Structure

```
/
├── Ecommerce.sln
├── .gitignore
├── README.md
└── src/
    ├── Ecommerce.API/          # Presentation Layer (ASP.NET Core API)
    ├── Ecommerce.Application/  # Application Layer (Business Logic)
    ├── Ecommerce.Domain/       # Domain Layer (Entities and Core Logic)
    └── Ecommerce.Infrastructure/ # Infrastructure Layer (Data, Caching, etc.)
└── tests/
    └── Ecommerce.API.Tests/    # Test Project
```