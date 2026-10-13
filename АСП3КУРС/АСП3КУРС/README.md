# АСП3курс — слоистый ASP.NET Core Web API (Repository + Service + Controller, AutoMapper)

Проект интернет-аптеки, переведённый на слоистую архитектуру. Доступ к данным
вынесен в репозитории, бизнес-логика — в сервисы, контроллеры стали «тонкими».
Преобразование сущностей в DTO и обратно выполняет AutoMapper (ручных методов
маппинга нет).

## Слои

```
HTTP-запрос
   └─> Controllers   – только вызывают сервис и переводят результат в HTTP-статус
         └─> Services – бизнес-логика + AutoMapper, без HTTP-типов и статус-кодов
               └─> Repositories (IRepository<T>/Repository<T>) – доступ к данным
                     └─> OnlinePharmacyP824Context (EF Core, SQL Server)
```

| Папка | Назначение |
|-------|-----------|
| `Models` | Сущности EF Core (включая новую `Reservation`) |
| `DTOs` | DTO для чтения (с вложенными связанными данными) и для записи (Id связей) |
| `Mapping/MappingProfile.cs` | Единая конфигурация AutoMapper |
| `Repositories` | `IRepository<T>` / `Repository<T>` — обобщённый доступ к данным |
| `Services` | `IXService` / `XService` — бизнес-логика для всех 9 сущностей |
| `Controllers` | Тонкие контроллеры, результат сервиса → HTTP-статус |
| `Middleware` | Преобразование исключений сервисов в ProblemDetails (400/404/409/500) |
| `Data` | `DatabaseSchemaInitializer` — идемпотентное обновление схемы БД |

## Регистрация в DI и время жизни

В `Program.cs`:

| Компонент | Время жизни | Почему |
|-----------|-------------|--------|
| `OnlinePharmacyP824Context` | **Scoped** | Один контекст на HTTP-запрос. EF Core `DbContext` не потокобезопасен и должен переиспользоваться в пределах запроса. |
| `IRepository<>` → `Repository<>` | **Scoped** | Репозитории разделяют один и тот же `DbContext` запроса, поэтому изменения сразу нескольких сущностей сохраняются **одним** `SaveChangesAsync`. |
| `ICategoryService` … `IReservationService` | **Scoped** | Бизнес-логика в рамках одного запроса; использует scoped-репозитории. |
| `IMapper` (AutoMapper) | **Singleton** | Конфигурация маппинга неизменяема и потокобезопасна — создаётся один раз. |

> Отдельного `UnitOfWork` и явных транзакций нет: атомарность бизнес-операции
> обеспечивается тем, что все репозитории запроса используют общий scoped `DbContext`.

## Связи между сущностями

- `Category 1 ── * Product`
- `Product 1 ── * CartItem` , `Cart 1 ── * CartItem`
- `Order 1 ── * OrderItem` , `Product 1 ── * OrderItem`
- `User 1 ── * Order` , `Role 1 ── * User` , `User 1 ── 1 Cart`
- `User 1 ── * Reservation` , `Product 1 ── * Reservation`

## Бизнес-операция и её отмена (две связанные сущности, один SaveChangesAsync)

Сущность `Reservation` — факт бизнес-операции: **кто** (`IdUser`), **что**
(`IdProduct`, `Quantity`) и **когда** (`CreatedAt`) зарезервировал товар.

1. **Резервирование** — `POST /api/reservations`
   - проверяет, что пользователь существует и активен, товар существует, а
     количества хватает (`StockQuantity >= Quantity`);
   - уменьшает `Product.StockQuantity` и создаёт запись `Reservation` со статусом `Active`;
   - **одним** `SaveChangesAsync` сохраняются оба изменения.
   - ошибки: нет товара/пользователя → **404**; не хватает остатка / пользователь
     деактивирован → **409** с понятным сообщением (не 500).

2. **Отмена резервирования** — `POST /api/reservations/{id}/release`
   - находит **активную** запись; если её нет → **404**;
   - возвращает `Product.StockQuantity`, закрывает запись (`Status = Released`, `ReleasedAt`);
   - **одним** `SaveChangesAsync` сохраняются оба изменения.

## CRUD

Для каждой из 9 сущностей (`categories`, `products`, `carts`, `cartitems`, `orders`,
`orderitems`, `users`, `roles`, `reservations`) реализованы эндпоинты:

| Метод | Маршрут | Результат |
|-------|---------|-----------|
| GET | `/api/{entity}?page=1&pageSize=10` | 200 + заголовок `X-Total-Count` |
| GET | `/api/{entity}/{id}` | 200 / 404 |
| POST | `/api/{entity}` | 201 + `Location` |
| PUT | `/api/{entity}/{id}` | 204 / 404 |
| PATCH | `/api/{entity}/{id}` | 204 / 404 |
| DELETE | `/api/{entity}/{id}` | 204 / 404 |

Для `reservations` метод `POST` выполняет бизнес-операцию резервирования.

## Отдельные DTO для чтения и записи

- Чтение (`ProductDto`, `OrderDto`, `CartDto`, `UserDto`, `ReservationDto`, …) —
  содержат вложенные связанные данные (`Product.Category`, `Order.Items`, `Cart.User`,
  `User.Role`, `Reservation.Product`).
- Запись (`CreateXDto`, `UpdateXDto`, `PatchXDto`) — содержат **Id** связанных сущностей.

## Схема базы данных

Изменения относительно исходной БД (выполняются автоматически при запуске, скрипт —
`Database/UpdateSchema.sql`):

- `Products.stock_quantity INT NOT NULL DEFAULT 0` — остаток товара;
- новая таблица `Reservations` (`id_reservation`, `id_user`, `id_product`, `quantity`,
  `status`, `created_at`, `released_at`).

## Запуск

```powershell
dotnet run --project .\АСП3курс.csproj
```

Строка подключения — `ConnectionStrings:DefaultConnection` в `appsettings.json`.
Swagger UI доступен по `/swagger` в режиме Development.
