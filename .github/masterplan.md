# Master Plan: BookStore Vertical Slice API + React Client

## 1. Mål

Ett demoprojekt som illustrerar **vertical-slice / single-file-arkitektur** med FastEndpoints.

- **Backend:** .NET 10 Web API + EF Core In-Memory (seedad vid start → klona och kör direkt, ingen DB-installation).
- **Domän:** BookStore (Books + Orders).
- **Frontend:** React-klient med list- och CRUD-modaler.
- **Tester:** Integrationstester (xUnit + `WebApplicationFactory`) som kör mot en isolerad in-memory-databas per testkörning.

## 2. Låsta beslut

| Beslut | Val |
| --- | --- |
| Root-namespace | `BookStore` (justera inklistrad helper `Northwind.Helpers` → `BookStore.Helpers`) |
| Id-typ | `int` |
| API-utforskare | `Swagger` (FastEndpoints.Swagger) över OpenAPI |
| GetAll-endpoints | Returnerar `PagedResult<T>` (visar delade helpers/klasser) |
| Order ↔ Book | `OrderItem` med Quantity + fryst UnitPrice; TotalPrice beräknas på servern |
| Seed-data | Böcker + ordrar, idempotent vid uppstart |
| Datalagring | EF Core In-Memory (nollställs vid omstart) |
| Endpoint-stil | Ett FastEndpoints-endpoint = en fil; Request + Response DTO:er i samma fil |
| Validering | FastEndpoints `Validator<T>` (FluentValidation), auto-upptäcks och körs före handlern |
| Frontend-stack | Vite + React + TypeScript + axios, modal-baserad CRUD |
| Teststack | xUnit + FluentAssertions + `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory<Program>`), EF Core In-Memory per test |

## 3. Datamodell

```mermaid
erDiagram
    BOOK ||--o{ ORDERITEM : "refereras av"
    ORDER ||--|{ ORDERITEM : "innehåller"

    BOOK {
        int Id
        string Title
        string Author
        string Genre
        string Description
        decimal Price
    }
    ORDER {
        int Id
        string UserEmail
        decimal TotalPrice
    }
    ORDERITEM {
        int BookId
        int Quantity
        decimal UnitPrice
    }
```

- **Book:** Id (int), Title, Author, Genre, Description, Price (decimal)
- **Order:** Id (int), UserEmail, OrderItems (lista), TotalPrice (decimal, beräknas på servern)
- **OrderItem:** BookId (int), Quantity (int), UnitPrice (decimal, fryst). LineTotal = UnitPrice × Quantity.
- **TotalPrice** = Σ (UnitPrice × Quantity). Klienten skickar aldrig pris.

## 4. Order-skapandeflödet

Klienten skickar `{ UserEmail, items:[{ BookId, Quantity }] }`.

```mermaid
sequenceDiagram
    participant K as React-modal
    participant E as CreateOrder-endpoint
    participant DB as AppDbContext (In-Memory)

    K->>E: POST /api/orders { userEmail, items[] }
    E->>E: validera (email ifylld, ≥1 item, qty > 0) via Validator<T>
    E->>DB: hämta böcker där Id ∈ items.BookId
    DB-->>E: böcker med priser
    E->>E: verifiera att alla bookId finns (annars 400)
    E->>E: bygg OrderItems, frys UnitPrice = aktuellt bokpris
    E->>E: TotalPrice = Σ (UnitPrice × Quantity)
    E->>DB: Orders.Add(order) + SaveChangesAsync()
    DB-->>E: order får Id
    E-->>K: 201 Created + CreateOrderResponse
```

## 5. Seed-data

- 6–8 böcker spridda över genrer (Fiction, Sci-Fi, Tech, Fantasy, …) med priser.
- 2–3 ordrar som refererar de seedade böckerna med antal, TotalPrice förberäknat.
- Idempotent: seedar bara om databasen är tom. Körs vid uppstart.

## 6. Struktur

```text
src/BookStore.Api/
  Endpoints/Books/   GetAllBooks, GetBookById, CreateBook, UpdateBook, DeleteBook   (en fil var)
  Endpoints/Orders/  GetAllOrders, GetOrderById, CreateOrder, UpdateOrder, DeleteOrder
  Helpers/Paging.cs          (ListRequest{Page,PageSize}, PagedResult<T>, ToPagedResultAsync)
  Helpers/globalusings.cs    (global using FastEndpoints, FluentValidation)
  Models/Data/               AppDbContext, Book, Order, OrderItem, DbSeeder
  Program.cs                 (DI, EF InMemory, CORS, FastEndpoints + Swagger, kör seed)
client/  (Vite React TS)      Books-sida + Orders-sida, var sin lista + CRUD-modal, axios api-klient
BookStore.IntegrationTests/  (xUnit)
  Infrastructure/BookStoreWebApplicationFactory.cs  (delad test-host, byter EF-registrering mot isolerad in-memory-DB)
  Endpoints/Books/CreateBookTests.cs                (slice-tester som speglar endpoint-strukturen)
  GlobalUsings.cs                                   (Xunit, FluentAssertions, EF Core)
.http-fil + kort README som speglar approachen
```

## 7. Implementationssteg

### Fas 1 – Backend-grund

1. Scaffolda solution + `BookStore.Api` (.NET 10). Paket: `FastEndpoints`, `FastEndpoints.Swagger`, `Microsoft.EntityFrameworkCore.InMemory`.
2. Modeller (`Book`, `Order`, `OrderItem`) + `AppDbContext` (DbSets) i `Models/Data`.
3. `Helpers/Paging.cs`: `ListRequest`, `PagedResult<T>`, `ToPagedResultAsync`. *(parallellt med 2)*
4. `Helpers/globalusings.cs`: globala usings för `FastEndpoints` och `FluentValidation`. *(parallellt med 2)*
5. `DbSeeder`: 6–8 böcker + 2–3 ordrar, idempotent. *(beror på 2)*

### Fas 2 – Slices

6. Books-slices (5 filer): egna Request/Response-typer i samma fil. GetAll returnerar `PagedResult<BookDto>`. *(beror på 2–4)*
7. Orders-slices (5 filer). Create/Update slår upp bokpriser, fryser UnitPrice, räknar TotalPrice. *(beror på 2–5)*
8. `Program.cs`: registrera DbContext (InMemory), CORS för React, `AddFastEndpoints()` + Swagger, kör seed vid uppstart.

### Fas 3 – Klient & test

9. React-klient: Vite + TS, axios api-klient, Books-sida (lista + CRUD-modal), Orders-sida (lista + CRUD-modal med antal per rad). *(beror på API-kontraktet 6–8)*
10. `.http`-fil för manuell test + kort README.
11. Integrationstester (`BookStore.IntegrationTests`): `WebApplicationFactory<Program>` som byter EF-registreringen mot en isolerad in-memory-DB per körning, slice-tester per endpoint (börjar med `CreateBook`). *(beror på 6–8)*

## 8. Återanvändbara mönster

- **`Helpers/Paging.cs`** – `PagedResult<T>` + `ToPagedResultAsync` används av alla GetAll-slices (delad klass).
- **`Helpers/globalusings.cs`** – globala usings (`FastEndpoints`, `FluentValidation`) så varje slice slipper upprepa dem.
- Varje slice ärver `Endpoint<TRequest, TResponse>` (eller `Endpoint<TRequest>`), injicerar `AppDbContext`, har route-attribut (`[Http...]`) på klassen och svarar via `Send.*Async`.

## 9. Verifiering

1. `dotnet build` + `dotnet run` → Swagger UI svarar, seed-data syns i `GET /api/books` och `GET /api/orders`.
2. `.http`: skapa bok → `POST /api/orders` med 2 items → bekräfta att TotalPrice = Σ(UnitPrice × Quantity).
3. `npm run dev` i `client/` → skapa/redigera/radera böcker och ordrar via modaler; paginering syns i listan.
4. `dotnet test` → integrationstesterna kör mot en isolerad in-memory-DB och verifierar endpoint-beteendet (t.ex. `CreateBook` returnerar `200 OK` vid giltig request och `400 BadRequest` när titel saknas).

## 10. Scope-gränser

- **Ingår:** Books CRUD, Orders CRUD, paginering, seed, Swagger, React-klient, integrationstester (xUnit).
- **Utelämnas (tills vidare):** auth/identity, persistent DB, API-versionering.
