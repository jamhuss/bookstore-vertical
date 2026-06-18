# BookStore — Vertical Slice Architecture Demo

A small, **self-contained** full-stack demo that illustrates the **Vertical Slice / single-file
endpoint** approach (built on [FastEndpoints](https://fast-endpoints.com/))
in ASP.NET Core, paired with a React client.

The goal is to show how a feature can live in **one file** — its route, request/response DTOs,
validation and handler all together — instead of being spread across controllers, services,
repositories and DTO folders. Everything runs **in-memory**, so you can clone the repo and run it
immediately without any database setup.

---

## What's inside

| Project | Stack | Purpose |
|---|---|---|
| `BookStore.Api` | .NET 10, ASP.NET Core, FastEndpoints, EF Core In-Memory, FluentValidation, Swagger | REST API organised by vertical slices |
| `bookstore.client` | React 19, Vite, TypeScript, axios | UI for managing books and orders |

### Domain

- **Book** — `Title`, `Author`, `Genre`, `Description`, `Price`.
- **Order** — `UserEmail`, `TotalPrice` (computed), and a list of order lines.
- **OrderItem** — links a book to an order with a `Quantity` and a **frozen** `UnitPrice`
  (so historic orders keep their price even if the book's price later changes).

The in-memory database is **seeded at startup** with 8 books and 2 orders, and **resets on every
restart**.

---

## Architecture: vertical slices

Instead of layering by *technical concern* (Controllers / Services / Repositories), the API is
organised by *feature*. Each use case is one class in its own file under `Endpoints/`:

```mermaid
flowchart LR
    subgraph Traditional["Layered (horizontal)"]
        direction TB
        C[Controllers] --> S[Services] --> R[Repositories] --> DB1[(DB)]
    end
    subgraph Vertical["Vertical slices"]
        direction TB
        A[CreateBook.cs<br/>route + DTOs + validation + handler] --> DB2[(DB)]
        B[GetAllBooks.cs] --> DB2
        E[CreateOrder.cs] --> DB2
    end
```

A typical slice (`Endpoints/Books/CreateBook.cs`) contains everything for that endpoint. The
route is declared with an attribute on the class, request/response DTOs and the FluentValidation
validator live in the same file, and FastEndpoints discovers and wires them all automatically:

```csharp
[HttpPost("api/books")]
[AllowAnonymous]
public class CreateBook : Endpoint<CreateBookRequest, CreateBookResponse>
{
    private readonly AppDbContext _db;

    public CreateBook(AppDbContext db) => _db = db;

    public override async Task HandleAsync(CreateBookRequest request, CancellationToken ct = default)
    {
        // ... build the book, save, then:
        await Send.OkAsync(dto, cancellation: ct);
    }
}

// Request + response DTOs live in the SAME file:
public class CreateBookRequest { public string Title { get; set; } = ""; /* ... */ }
public class CreateBookResponse : CreateBookRequest { public int Id { get; set; } }

// FluentValidation validator also lives in the slice and is auto-registered:
public class CreateBookValidator : Validator<CreateBookRequest> { /* ... */ }
```

**Why this layout?** Adding or changing a feature is a *local* change to one file. There are no
shared service/repository abstractions to ripple through — only genuinely shared helpers live
outside the slices (in `Helpers/`).

### Validation

FastEndpoints discovers every `Validator<TRequest>` in the assembly and runs it **before**
`HandleAsync` is called. If validation fails, the request is short-circuited with a `400` and a
problem-details payload automatically — there is no manual `ValidateAsync` call or `IValidator<T>`
injection in the handlers.

### Composite binding

FastEndpoints binds a **single** request object from multiple sources at once. In
`Endpoints/Orders/AddBookToOrder.cs`, `OrderId` comes from the route template while `BookId` and
`Quantity` are bound from the request body — no custom binding attribute required:

```csharp
[HttpPost("api/orders/{OrderId:int}/books")]
[AllowAnonymous]
public class AddBookToOrder : Endpoint<AddBookToOrder.AddBookRequest, AddBookToOrder.OrderDto>
{
    public override async Task HandleAsync(AddBookRequest request, CancellationToken ct = default) { /* ... */ }

    // OrderId comes from the path, BookId + Quantity from the body:
    public record AddBookRequest(int OrderId, int BookId, int Quantity);
}
```

---

## API surface

| Method | Route | Slice |
|---|---|---|
| GET | `/api/books` | `GetAllBooks` (paged) |
| GET | `/api/books/{id}` | `GetBookById` |
| POST | `/api/books` | `CreateBook` |
| PUT | `/api/books/{id}` | `UpdateBook` |
| DELETE | `/api/books/{id}` | `DeleteBook` |
| GET | `/api/orders` | `GetAllOrders` (paged) |
| GET | `/api/orders/{id}` | `GetOrderById` |
| POST | `/api/orders` | `CreateOrder` |
| PUT | `/api/orders/{id}` | `UpdateOrder` |
| DELETE | `/api/orders/{id}` | `DeleteOrder` |
| POST | `/api/orders/{orderId}/books` | `AddBookToOrder` (composite route + body binding) |

Interactive docs (Swagger UI) are available in Development at **`/swagger`**, reading the OpenAPI
document generated by FastEndpoints.

---

## File structure

```
northwind-vertical/
├─ BookStore.slnx                     # Solution (API + client)
│
├─ BookStore.Api/                     # ── ASP.NET Core API ──
│  ├─ Program.cs                      # DI, EF In-Memory, CORS, FastEndpoints, Swagger, startup seed
│  ├─ BookStore.Api.csproj
│  ├─ BookStore.Api.http              # Manual request examples
│  │
│  ├─ Endpoints/                      # One folder per entity, one file per use case
│  │  ├─ Books/
│  │  │  ├─ GetAllBooks.cs            # paged list
│  │  │  ├─ GetBookById.cs
│  │  │  ├─ CreateBook.cs             # + FluentValidation validator (auto-registered)
│  │  │  ├─ UpdateBook.cs
│  │  │  └─ DeleteBook.cs
│  │  └─ Orders/
│  │     ├─ GetAllOrders.cs
│  │     ├─ GetOrderById.cs
│  │     ├─ CreateOrder.cs            # looks up prices, freezes UnitPrice, computes total
│  │     ├─ UpdateOrder.cs
│  │     ├─ DeleteOrder.cs
│  │     └─ AddBookToOrder.cs         # composite route + body binding
│  │
│  ├─ Helpers/                        # Genuinely shared cross-slice code
│  │  ├─ Paging.cs                    # ListRequest, PagedResult<T>, ToPagedResultAsync
│  │  └─ globalusings.cs              # global usings (FastEndpoints, FluentValidation)
│  │
│  ├─ Models/Data/                    # Entities + EF context
│  │  ├─ Book.cs
│  │  ├─ Order.cs
│  │  ├─ OrderItem.cs                 # computed LineTotal = UnitPrice * Quantity
│  │  ├─ AppDbContext.cs
│  │  └─ DbSeeder.cs                  # idempotent seed (8 books, 2 orders)
│  │
│  └─ Properties/launchSettings.json  # http://localhost:5212, auto-opens Swagger
│
└─ bookstore.client/                  # ── React + Vite client ──
   ├─ vite.config.ts                  # dev server :56789, proxies /api → :5212
   ├─ package.json
   └─ src/
      ├─ main.tsx                     # React entry point
      ├─ App.tsx                      # tab navigation: Books / Orders
      ├─ App.css, index.css           # styling
      ├─ types.ts                     # TS types mirroring API contracts
      ├─ api.ts                       # axios client (booksApi, ordersApi) + error mapping
      ├─ components/
      │  ├─ Modal.tsx                 # generic modal shell (backdrop + header)
      │  ├─ BookModal.tsx             # self-contained create/edit-book form
      │  └─ OrderModal.tsx            # self-contained create/edit-order form
      └─ pages/
         ├─ BooksPage.tsx            # list + pagination, opens BookModal
         └─ OrdersPage.tsx           # list + pagination, opens OrderModal
```

### Client design

The pages own only **list / pagination state** and which item is being edited. The
create/edit forms are extracted into **self-contained modal components** (`BookModal`,
`OrderModal`) that manage their own form state and call the API directly. The parent simply
decides *when* a modal is open and *what* it edits:

```tsx
<BookModal open={modalOpen} book={editingBook /* null = new */} onClose={...} onSaved={...} />
```

---

## Running it

You need **two terminals** — the API and the client run side by side. The Vite dev server proxies
`/api` calls to the API, so no CORS configuration is needed for local development.

**1. API** (opens the Swagger UI automatically):

```bash
cd BookStore.Api
dotnet run
# → API at http://localhost:5212
# → Swagger UI at http://localhost:5212/swagger
```

**2. Client:**

```bash
cd bookstore.client
npm install      # first time only
npm run dev
# → http://localhost:56789
```

Open <http://localhost:56789>, then use the **Books** and **Orders** tabs to create, edit and
delete records. Validation errors from the API (FluentValidation) are surfaced in the modal forms.

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (18+)

---

## Tech notes

- **EF Core In-Memory** — no database to install; data lives in process memory and resets on
  restart. Swapping to a real provider is a one-line change in `Program.cs`.
- **FastEndpoints** — each slice is an `Endpoint<TRequest, TResponse>` with its route declared via
  an `[Http...]` attribute; endpoints are discovered and mapped by `AddFastEndpoints()` /
  `UseFastEndpoints()`.
- **FluentValidation** — validators derive from FastEndpoints' `Validator<T>`, are auto-discovered,
  and run before the handler; each one lives inside its slice, so adding validation stays a local
  change.
- **Swagger** — OpenAPI document and Swagger UI are provided by `FastEndpoints.Swagger`, only
  mapped in the Development environment.
- **HTTPS redirect** is skipped in Development so the plain-HTTP Vite proxy works without 307
  redirects.
