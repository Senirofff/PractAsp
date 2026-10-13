using Microsoft.EntityFrameworkCore;
using АСП3курс.Models;

namespace АСП3курс.Data;

/// <summary>
/// Приводит схему существующей базы данных в соответствие с моделью:
/// добавляет колонку остатка товара и таблицу резервирований.
/// Операции идемпотентны и выполняются только если объектов ещё нет.
/// </summary>
public static class DatabaseSchemaInitializer
{
    public const string Script = @"
IF COL_LENGTH('dbo.Products', 'stock_quantity') IS NULL
BEGIN
    ALTER TABLE [dbo].[Products] ADD [stock_quantity] INT NOT NULL
        CONSTRAINT [DF_Products_stock_quantity] DEFAULT (0);
END;

IF OBJECT_ID('dbo.Reservations', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Reservations] (
        [id_reservation] INT IDENTITY(1,1) NOT NULL,
        [id_user]        INT NOT NULL,
        [id_product]     INT NOT NULL,
        [quantity]       INT NOT NULL,
        [status]         VARCHAR(20) NOT NULL CONSTRAINT [DF_Reservations_status] DEFAULT ('Active'),
        [created_at]     DATETIME NOT NULL CONSTRAINT [DF_Reservations_created_at] DEFAULT (GETDATE()),
        [released_at]    DATETIME NULL,
        CONSTRAINT [PK_Reservations] PRIMARY KEY ([id_reservation]),
        CONSTRAINT [FK_Reservations_Users] FOREIGN KEY ([id_user]) REFERENCES [dbo].[Users] ([id_user]),
        CONSTRAINT [FK_Reservations_Products] FOREIGN KEY ([id_product]) REFERENCES [dbo].[Products] ([id_product])
    );
END;";

    public static async Task EnsureUpdatedAsync(OnlinePharmacyP824Context context, ILogger logger)
    {
        await context.Database.ExecuteSqlRawAsync(Script);
        logger.LogInformation("Схема БД проверена: колонка stock_quantity и таблица Reservations готовы к работе.");
    }
}
