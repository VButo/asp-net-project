# API Tester

ASP.NET Core 8 MVC application for organizing and running saved HTTP requests. It uses Entity Framework Core, MySQL, ASP.NET Core Identity, role-based authorization, file attachments, global search, and an optional AI request-drafting assistant.

## Requirements

- .NET 8 SDK
- MySQL 8
- Python 3 only when using the included MCP server

## Local setup

Set the database connection string without committing credentials:

```powershell
$env:ConnectionStrings__ApiTesterDb="server=localhost;port=3306;database=api_tester;user=api_tester;password=change-me"
dotnet ef database update
dotnet run --project API-tester.csproj
```

The launch profile may choose a different local port. Open the URL printed by `dotnet run`.

Build and test:

```powershell
dotnet restore API-tester.sln
dotnet build API-tester.sln --no-restore
dotnet test API-tester.Tests/API-tester.Tests.csproj --no-build
```

## Production configuration

Required:

- `ConnectionStrings__ApiTesterDb`: MySQL connection string.
- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://+:8080` when running the container.

Recommended:

- `SeedAdminEmail`: initial admin email.
- `SeedAdminPassword`: strong initial admin password; rotate it after first login.
- `Authentication__Google__ClientId` and `Authentication__Google__ClientSecret`: Google OAuth.
- `AI__ApiKey`: enables provider-backed AI request drafting.
- `AI__Endpoint`: optional compatible chat-completions endpoint.
- `AI__Model`: optional model name; defaults to `gpt-4.1-mini`.

Do not put passwords, OAuth secrets, AI keys, or production connection strings in `appsettings*.json`.

## Cloud or VM deployment

Build and run the container:

```powershell
docker build -t api-tester .
docker run --rm -p 8080:8080 `
  -e ASPNETCORE_ENVIRONMENT=Production `
  -e ConnectionStrings__ApiTesterDb="server=YOUR_DB_HOST;port=3306;database=api_tester;user=api_tester;password=YOUR_PASSWORD" `
  -e SeedAdminEmail="admin@example.com" `
  -e SeedAdminPassword="REPLACE_WITH_A_STRONG_PASSWORD" `
  api-tester
```

Build command for a non-container VM:

```powershell
dotnet publish API-tester.csproj -c Release -o publish
```

Start command:

```powershell
dotnet publish/API-tester.dll
```

Before starting a production release, apply migrations from a trusted deployment job:

```powershell
dotnet ef database update --project API-tester.csproj
```

Use a managed MySQL service or a VM-local MySQL installation, terminate TLS at the cloud load balancer/reverse proxy, mount persistent storage for `wwwroot/uploads`, and configure health/availability monitoring. The application does not automatically alter the production schema at startup.

## Authentication and authorization

Only `/`, `/login`, `/register`, and the external OAuth callback are anonymous application routes. `/` redirects to `/login` or `/dashboard`. Other MVC and API routes require an Identity cookie; create/update operations require `Admin` or `Manager`, and destructive operations generally require `Admin`.

Login, registration, and Google login redirect to `/dashboard`. Logout invalidates the Identity cookie and returns to `/login`.

## AI request drafting

Request Builder contains an **AI Request Draft** prompt. It generates a draft method, URL, headers, and body for review. It never sends the request automatically.

Without `AI__ApiKey`, a deterministic local fallback handles common prompts and clearly reports that fallback mode is active. Provider failures also fall back safely. Request prompts are not logged, and credentials should never be entered into prompts.

## Global search

Use the authenticated navbar search or `/search`. It searches workspaces, collections, request names/URLs/methods/tags, environments, and tags. Results link to the relevant detail or editor screen.

## Logging

The application uses structured ASP.NET Core logging. Startup, login/logout, mutating HTTP requests, request execution success/failure, HTTP 4xx/5xx responses, and unhandled exceptions are logged. Request bodies, passwords, cookies, authorization headers, tokens, and API keys are not logged.

Locally, view logs in the `dotnet run` console. In Docker or cloud hosting, use container stdout/stderr:

```powershell
docker logs CONTAINER_NAME
```

Connect stdout to the provider's log service for retention and alerting.

## MCP server

The workspace includes a dependency-free stdio MCP server at `mcp/api_tester_mcp.py` and VS Code configuration at `.vscode/mcp.json`. It exposes:

- `listWorkspaces`
- `listCollections`
- `listRequests`
- `getRequest`
- `createRequest`

Start the web app, sign in, then copy the complete Identity cookie pair from browser developer tools, for example:

```text
.AspNetCore.Identity.Application=COOKIE_VALUE
```

In VS Code run **MCP: List Servers**, start `apiTester`, and provide that cookie when prompted. Set `API_TESTER_URL` in `.vscode/mcp.json` if the app is not on port 5000.

The MCP bridge intentionally does not expose arbitrary request execution. It uses the same API authorization rules as the browser and cannot bypass roles. Treat the cookie as a secret and revoke it by logging out.

## Playwright

Playwright is not currently installed. The integration suite exercises the HTTP pipeline, validation, authorization, CRUD APIs, and routing. A browser E2E suite remains optional setup rather than silently adding a Node toolchain to this .NET repository.

## Uploaded files

Request attachments are stored under `wwwroot/uploads/requests`. Use a persistent volume on a VM/container. For multiple application instances, replace local storage with shared object storage.
