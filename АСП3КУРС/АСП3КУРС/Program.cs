using AutoMapper;
using Microsoft.EntityFrameworkCore;
using АСП3курс.Data;
using АСП3курс.Mapping;
using АСП3курс.Middleware;
using АСП3курс.Models;
using АСП3курс.Repositories;
using АСП3курс.Services;

namespace АСП3курс
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // DbContext регистрируется как Scoped: один экземпляр на HTTP-запрос.
            // Благодаря этому все репозитории и сервисы запроса работают с одним
            // контекстом, а значит один SaveChangesAsync сохраняет изменения сразу
            // нескольких сущностей (без UnitOfWork и явных транзакций).
            builder.Services.AddDbContext<OnlinePharmacyP824Context>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // AutoMapper — Singleton: конфигурация маппинга неизменяема и потокобезопасна,
            // поэтому её выгодно создать один раз на всё приложение.
            builder.Services.AddSingleton<IMapper>(sp =>
            {
                var configuration = new MapperConfiguration(
                    cfg => cfg.AddProfile<MappingProfile>(),
                    sp.GetRequiredService<ILoggerFactory>());

                return configuration.CreateMapper();
            });

            // Репозитории — Scoped: живут в рамках запроса и используют общий DbContext.
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Сервисы — Scoped: содержат бизнес-логику и обращаются к репозиториям
            // и AutoMapper; на каждый запрос создаётся свой экземпляр.
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<ICartItemService, CartItemService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IOrderItemService, OrderItemService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IReservationService, ReservationService>();

            var app = builder.Build();

            // Идемпотентно приводит схему существующей БД в соответствие с моделью
            // (колонка Products.stock_quantity и таблица Reservations).
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<OnlinePharmacyP824Context>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    await DatabaseSchemaInitializer.EnsureUpdatedAsync(context, logger);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Не удалось автоматически обновить схему базы данных.");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
