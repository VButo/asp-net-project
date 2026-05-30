# Sitemap

Ovaj fajl popisuje rute, HTTP metode, odgovarajuće kontrolere/akcije i pripadajuće view-e u aplikaciji.

## Global

- **Conventional route**: `{controller=Home}/{action=Index}/{id?}`
- **Attribute routing**: `app.MapControllers()` omogućuje rute deklarirane kroz `[HttpGet(...)]`, `[HttpPost(...)]` atribute.

## Kontroleri i rute

- **HomeController**
  - GET `` or `home` → `HomeController.Index()` → Views/Home/Index.cshtml
  - GET `privacy` → `HomeController.Privacy()` → redirects to `WorkspacesController.Index`
  - GET `workspace-overview` → `HomeController.WorkspaceDetails()` → returns a View()
  - GET `request-overview` → `HomeController.RequestDetails()` → returns a View()
  - GET `builder` → `HomeController.RequestBuilder()` → redirects to `RequestBuilderController.Index`

- **WorkspacesController**
  - GET `workspaces` → `WorkspacesController.Index()` → Views/Workspaces/Index.cshtml
  - POST `workspaces/create` → `WorkspacesController.Create(ApiWorkspace)` → Form POST for creating a workspace

- **WorkspaceDetailsController**
  - GET `workspace-details` → `WorkspaceDetailsController.Details()` → Views/WorkspaceDetails/Details.cshtml
  - GET `workspace-details/{workspaceId:int}` → `WorkspaceDetailsController.Details(int)` → partial view `_WorkspaceDetails` when AJAX

- **CollectionController**
  - GET `collections` → `CollectionController.Index()` → Views/Collection/Index.cshtml
  - GET `collections/{id:int}` → `CollectionController.Details(int)` → Views/Collection/Details.cshtml
  - POST `collections/create` → `CollectionController.Create(ApiCollection)` → Form POST creating a collection

- **RequestController**
  - GET `requests` → `RequestController.Index()` → Views/Request/Index.cshtml

- **RequestDetailsController**
  - GET `request-details` → `RequestDetailsController.Details()` → Views/RequestDetails/Details.cshtml
  - GET `request-details/{requestId:int}` → `RequestDetailsController.Details(int)` → partial view `_RequestDetails` when AJAX

- **RequestBuilderController**
  - GET `request-builder` → `RequestBuilderController.Index()` → Views/RequestBuilder/Index.cshtml
  - GET `request-builder/{requestId:int}` → `RequestBuilderController.Index(int)` → returns existing request or new model; partial `_RequestBuilder` when AJAX
  - POST `request-builder/save` → `RequestBuilderController.Save(ApiRequest)` → Form POST to save request

- **EnvironmentController**
  - GET `environments` → `EnvironmentController.Index()` → Views/Environment/Index.cshtml (lists all environments)
  - GET `environments/{workspaceId:int}` → `EnvironmentController.Index(int)` → filters by workspace
  - POST `environments/save` → `EnvironmentController.Save(ApiEnvironment)` → Form POST to save environment

## Notes

- Several actions return partial views when the request contains header `X-Requested-With: XMLHTTPRequest` (AJAX modal/partial rendering). Examples: `RequestBuilder`, `RequestDetails`, `WorkspaceDetails`.
- Shortcut routes on `HomeController` (`/builder`, `/workspace-overview`, `/request-overview`) are present to provide friendly navigation; some return redirects to the canonical controller routes.
- The default conventional route still applies for controller/action/id combinations not covered by attribute routes.

---
Generated on 2026-05-30.
