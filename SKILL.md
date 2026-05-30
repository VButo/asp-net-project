# API-tester SKILL

This SKILL file documents the repository purpose, key conventions, developer commands, and the assistant capabilities (how the AI should help with this project). It is written in English.

## Project Overview

API-tester is a small ASP.NET Core 8.0 Razor application for organizing API workspaces, collections, requests, and environment configurations. It uses EF Core (Pomelo MySQL provider) and follows an EF-ready domain model: navigation properties are `virtual` and collection properties are `ICollection<T>` initialized as `HashSet<T>`.

Key folders:
- `Models/` — domain entities (workspaces, collections, requests, environments, tags, users, etc.).
- `Controllers/` — MVC controllers with attribute routes for primary UI pages.
- `Views/` — Razor views and partials.
- `Data/` — `AppDbContext` and database setup.

## Conventions

- EF Core: explicit FK properties (e.g., `WorkspaceId`, `CollectionId`) and navigations; many-to-many modeled via join entities (e.g., `RequestTagMap`, `WorkspaceMembership`).
- Routing: attribute routing is used for primary pages (e.g., `[HttpGet("workspaces")]`), while `MapControllerRoute` provides a conventional fallback (`{controller=Home}/{action=Index}/{id?}`).
- Partial views are returned when header `X-Requested-With: XMLHTTPRequest` is present for modal/AJAX requests.

## Common Developer Commands

Build and run locally:
```bash
dotnet build
dotnet run --urls http://localhost:5271
```

Entity Framework migrations (if needed):
```bash
dotnet ef migrations add <Name>
dotnet ef database update
```

If binaries are locked by a previous run, terminate running process (Task Manager or `taskkill /IM API-tester.exe /F`).

## Important Files
- `Program.cs` — app startup, DbContext registration, `app.MapControllers()` and default route.
- `semantic-model.md` — generated domain model summary.
- `sitemap.md` — generated route map and view mapping.

## Assistant / Automation Capabilities

When working on this repository, the assistant should:

- Respect existing code style and minimal changes principle.
- Use `apply_patch` to edit files and `dotnet build` / `dotnet run` to verify changes where applicable.
- Update or create documentation files (`semantic-model.md`, `sitemap.md`, `SKILL.md`) when requested.
- When modifying models, ensure navigation properties are `virtual` and collections are `ICollection<T>`.
- When adding routes, prefer attribute routing and avoid collisions with `HomeController` shortcut routes.
- When changing controller behavior that affects views, update corresponding Razor views in `Views/`.

## Lab / Deliverable Notes

- Deliverables already generated: `semantic-model.md`, `sitemap.md`.
- If requested, assistant can generate a Mermaid diagram, create migration scripts, or add README instructions.

---
Generated on 2026-05-30.
