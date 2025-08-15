# GEMINI.md

## Project Overview

This project is a .NET Aspire application that provides a starting point for building applications with Keycloak for authentication and authorization. It consists of four main projects:

*   **AppHost:** The main entry point for the application. It orchestrates the different services, including the API, the database, and the Keycloak container.
*   **Api:** A Web API project that is secured with Keycloak. It provides a sample endpoint and is configured to use a SQL Server database.
*   **Database:** A class library that contains the Entity Framework Core `DbContext` for the application. The database schema is currently empty.
*   **ServiceDefaults:** A project that contains the default service configurations for the application.

The project uses .NET 9.0 and is configured to use centrally managed package versions. It leverages the following key technologies:

*   **.NET Aspire:** For orchestrating the different services.
*   **Keycloak:** For authentication and authorization.
*   **Entity Framework Core:** For database access.
*   **SQL Server:** As the database provider.
*   **OpenTelemetry:** For observability.
*   **Scalar:** For OpenAPI documentation.

## Building and Running

To build and run the project, you can use the following command from the root directory:

```bash
dotnet run --project ./AppHost/
```

This will start the Keycloak container, the SQL Server instance, and the API. The API will be available at the address specified in the launchSettings.json file.

## Development Conventions

*   **Authentication:** The API is secured with Keycloak. All endpoints require authentication by default.
*   **Database:** The project uses Entity Framework Core for database access. The database schema is defined in the `Database` project.
*   **Configuration:** The application is configured using the `appsettings.json` files in each project.
*   **Dependencies:** The project uses centrally managed package versions, which are defined in the `Directory.Packages.props` file.
