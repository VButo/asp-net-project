# Semantic Model

Ovaj dokument sažima domenski model aplikacije (entiteti, ključna svojstva, i veze).

## Sažetak entiteta

- **ApiWorkspace**
  - Id (PK), Name, Description, OwnerUserId (FK)
  - Navigation: `OwnerUser` (User), `Collections` (ICollection<ApiCollection>), `Environments` (ICollection<ApiEnvironment>), `Members` (ICollection<WorkspaceMembership>)
  - Veze: jedan `Workspace` ima mnogo `Collections`, `Environments`, i `WorkspaceMembership`.

- **User**
  - Id (PK), Username, Email, Role, IsActive, CreatedAt
  - Navigation: `WorkspacesOwned` (ICollection<ApiWorkspace>), `WorkspaceMemberships` (ICollection<WorkspaceMembership>)
  - Veze: jedan `User` može biti vlasnik više `Workspace`-a i član više `Workspace`-a.

- **WorkspaceMembership**
  - Id (PK), WorkspaceId (FK), UserId (FK), Role, CreatedAt
  - Navigation: `Workspace`, `User`
  - Veze: povezuje `User` i `Workspace` (many-to-many kroz membership entitet).

- **ApiCollection**
  - Id (PK), Name, Description, WorkspaceId (FK), CreatedAt
  - Navigation: `Workspace`, `Requests` (ICollection<ApiRequest>)
  - Veze: svaki `Collection` pripada jednom `Workspace` i sadrži više `Request`-ova.

- **ApiRequest**
  - Id (PK), Name, Method (HttpMethodType), Url, CollectionId (FK), CreatedAt, Body
  - Navigation: `Collection`, `Headers` (ICollection<ApiHeader>), `Responses` (ICollection<ApiResponse>), `Executors` (ICollection<ApiRequestExecutor>), `TagMaps` (ICollection<RequestTagMap>), `EnvironmentLinks` (ICollection<RequestEnvironmentLink>)
  - Veze: request pripada `Collection`; ima mnogo `Headers`, `Responses`, i veza prema `Tags` i `Environments`.

- **ApiHeader**
  - Id (PK), RequestId (FK), Key, Value, IsActive, IsSecret
  - Navigation: `Request`

- **ApiResponse**
  - Id (PK), RequestId (FK), StatusCode, Body, Headers (serialized), ReceivedAt
  - Navigation: `Request`

- **ApiRequestExecutor**
  - Id (PK), RequestId (FK), ExecutedAt, DurationMs, Success, RawResponse
  - Navigation: `Request`
  - Veze: povijest izvršavanja pojedinog `Request`-a.

- **ApiEnvironment**
  - Id (PK), Name, BaseUrl, Type (EnvironmentType), WorkspaceId (FK), IsActive
  - Navigation: `Workspace`, `Variables` (ICollection<EnvironmentVariable>)
  - Veze: `Workspace` -> `Environments` (1:N). Environment sadrži skup `EnvironmentVariable`.

- **EnvironmentVariable**
  - Id (PK), EnvironmentId (FK), Key, Value, IsSecret, LastUpdatedAt
  - Navigation: `Environment`

- **RequestTag**
  - Id (PK), Name, CreatedAt
  - Navigation: `TagMaps` (ICollection<RequestTagMap>)

- **RequestTagMap**
  - Id (PK), RequestId (FK), TagId (FK)
  - Navigation: `Request`, `Tag`
  - Veze: implementira many-to-many između `ApiRequest` i `RequestTag`.

- **RequestEnvironmentLink**
  - Id (PK), RequestId (FK), EnvironmentId (FK), OverrideBaseUrl, Notes
  - Navigation: `Request`, `Environment`
  - Veze: povezuje `Request` i `ApiEnvironment` (npr. specifična konfiguracija requesta za env).


## Ključna pravila i napomene

- Svi navigacijski property-ji su `virtual` i kolekcije su `ICollection<T>` (inicializirane kao `HashSet<T>`), pripremljeno za EF Core lazy loading / proxy scenarije.
- Veze se po potrebi modeliraju direktnim FK poljima (`WorkspaceId`, `CollectionId`, `RequestId`, ...) i odgovarajućim navigacijama.
- Mnogi-do-mnogih odnosi implementirani su kroz mapirajuće entitete (`RequestTagMap`, `WorkspaceMembership`) umjesto implicitnih EF many-to-many konvencija, što omogućava dodatna polja na poveznicama (npr. role, createdAt).
- `ApiRequestExecutor` i `ApiResponse` čuvaju povijest izvršavanja i odgovora; nisu kritične za konzistenciju modela, ali su korisne za audit i debugging.

## ER pregled (tekstualni)

Workspace 1 --- * Collection
Workspace 1 --- * ApiEnvironment
Workspace 1 --- * WorkspaceMembership
User 1 --- * Workspace (Owner)
User 1 --- * WorkspaceMembership
Collection 1 --- * ApiRequest
ApiRequest 1 --- * ApiHeader
ApiRequest 1 --- * ApiResponse
ApiRequest 1 --- * ApiRequestExecutor
ApiRequest * --- * RequestTag (preko RequestTagMap)
ApiRequest * --- * ApiEnvironment (preko RequestEnvironmentLink)
ApiEnvironment 1 --- * EnvironmentVariable

## Preporuke

- Ako treba, mogu generirati dijagram (Mermaid) zasnovan na ovim vezama i ubaciti u ovaj dokument.
- Mogu automatski izvući i uključiti sva svojstva svakog modela iz `Models/` ako želiš potpuni, polje-po-polje izlist.

---
Generated on 2026-05-30.
