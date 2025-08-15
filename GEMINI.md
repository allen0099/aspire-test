# GEMINI.md

## Project Overview

This project is a .NET Aspire application that provides a starting point for building applications with Keycloak for authentication and authorization. It consists of five main projects:

*   **AppHost:** The main entry point for the application. It orchestrates the different services, including the API, the database, the Keycloak container, and the WebApp.
*   **Api:** A Web API project that is secured with Keycloak. It provides a sample endpoint and is configured to use a SQL Server database.
*   **Database:** A class library that contains the Entity Framework Core `DbContext` for the application.
*   **ServiceDefaults:** A project that contains the default service configurations for the application.
*   **WebApp:** A Vite-based React application for the frontend.

The project uses .NET 9.0 and is configured to use centrally managed package versions. It leverages the following key technologies:

*   **.NET Aspire:** For orchestrating the different services.
*   **Keycloak:** For authentication and authorization.
*   **Entity Framework Core:** For database access.
*   **SQL Server:** As the database provider.
*   **React:** For the frontend, built with Vite and TypeScript.
*   **OpenTelemetry:** For observability.
*   **Scalar:** For OpenAPI documentation.

## Building and Running

To build and run the project, you can use the following command from the root directory:

```bash
dotnet run --project ./AppHost/
```

This will start the Keycloak container, the SQL Server instance, the API, and the WebApp. The API will be available at the address specified in the launchSettings.json file, and the WebApp will be available at http://localhost:5170.

## Development Conventions

*   **Authentication:** The API is secured with Keycloak. All endpoints require authentication by default.
*   **Database:** The project uses Entity Framework Core for database access. The database schema is defined in the `Database` project.
*   **Configuration:** The application is configured using the `appsettings.json` files in each project.
*   **Dependencies:** The project uses centrally managed package versions, which are defined in the `Directory.Packages.props` file.
*   **Frontend:** The frontend is a Vite-based React application. You can run it in development mode using `npm run dev` from the `WebApp` directory.
