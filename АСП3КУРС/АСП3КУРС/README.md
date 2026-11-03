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
| `Mapping/MappingExpressionExtensions.cs` | `IgnoreNullMembers()` — частичное обновление (PATCH) |
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

> `DELETE` связанной сущности (например, товара, на который ссылаются позиции
> заказов/корзин или резервирования) возвращает **409** — БД запрещает удаление
> из-за ограничения внешнего ключа (обрабатывается как `DbUpdateException`, не 500).

## Частичное обновление (PATCH)

Маппинги `PatchXDto → X` настраиваются методом `IgnoreNullMembers()`: свойства DTO,
равные `null`, не изменяют сущность. Используется `PreCondition`, а не `Condition`,
потому что в AutoMapper 16 `Condition == false` для значимых типов записывает в
назначение `0`/`false`, тогда как `PreCondition == false` полностью пропускает
маппинг члена и сохраняет исходное значение.

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

## Запуск API и React

Запустите API из каталога с файлом проекта.

```powershell
dotnet run --project .\АСП3курс.csproj --launch-profile http
```

API слушает `http://localhost:5057`. В режиме Development Swagger доступен по
`http://localhost:5057/swagger`. Нажмите **Authorize** и вставьте значение
`accessToken` из ответа регистрации или входа. Замки отмечают защищённые операции.

Настройте `ConnectionStrings:DefaultConnection` для существующей базы SQL Server
с таблицами аптеки. При запуске API добавляет роли `Admin` и `User` в таблицу
`Roles` при их отсутствии. В Development без `Jwt__SigningKey` создаётся
случайный ключ на время процесса, поэтому после перезапуска нужно войти снова.
Для постоянного ключа перед запуском задайте `Jwt__SigningKey` через переменную
окружения или хранилище секретов. Используйте случайный ключ длиной от 32 байт.
За пределами Development ключ обязателен.

В отдельном терминале из каталога `frontend` выполните:

```powershell
npm ci
npm run dev
```

Откройте `http://localhost:5173`. Vite и API работают на разных портах. Origin
клиента задаёт `Cors:FrontendOrigin` в `appsettings.json`. Адрес API для клиента
можно переопределить через `VITE_API_URL` в `frontend/.env.local`.

## Доступ к API

| Операция | Доступ |
|---|---|
| `POST /api/auth/register`, `POST /api/auth/login` | Публичный. В ответе JWT на 30 минут |
| `GET /api/products…`, `GET /api/categories…` | Публичное чтение каталога |
| `GET /api/users/{id}`, `PATCH /api/users/{id}/profile` | Владелец с ролью `User` или любой `Admin` |
| Остальные CRUD-операции | Политика `Admin` |

Регистрация назначает `User`. DTO административных операций записи принимает
`password`, который сервер хеширует через `PasswordHasher<User>`. Для
существующей записи с несовместимым форматом хеша потребуется сброс пароля.
Смена роли или деактивация учётной записи прекращает действие выданных ей токенов.

Чтобы назначить первого администратора, зарегистрируйте отдельную учётную запись
через клиент и выполните в своей базе SQL Server запрос, подставив её логин.
Затем войдите под этой записью заново.

```sql
UPDATE u
SET id_role = r.id_role
FROM dbo.Users AS u
CROSS JOIN dbo.Roles AS r
WHERE u.login = 'your_admin_login' AND r.title = 'Admin';
```

## Проверка в браузере

1. Зарегистрируйте два тестовых профиля и запомните их ID. Войдите под первым
   профилем. Кнопка **Мой профиль** возвращает `200`, изменение собственного
   имени и фамилии — `204`.
2. Введите ID второго профиля. Открытие и изменение чужого профиля возвращают
   `403`. **Список пользователей (Admin)** также возвращает `403`. Под
   администратором список и доступ к обоим профилям разрешены.
3. Нажмите **Запрос без токена** для проверки `401`. Для проверки `429`
   отправьте шесть попыток входа в пределах минуты с одного IP. Лимит считает
   все запросы входа, включая успешные.
4. Нажмите **Открыть клиент через другой origin**. На `http://127.0.0.1:5173`
   отправьте форму входа. Браузер блокирует запрос после preflight `OPTIONS`,
   поскольку по умолчанию разрешён `http://localhost:5173`. В Console и Network
   виден отказ CORS. На странице отображается отсутствие доступного HTTP-ответа.
