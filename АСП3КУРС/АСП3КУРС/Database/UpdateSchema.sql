-- Обновление схемы базы данных online_pharmacy_P824 для слоистого проекта АСП3курс.
-- Скрипт идемпотентен: повторный запуск не вызовет ошибок.
-- Приложение также применяет этот скрипт автоматически при запуске
-- (см. Data/DatabaseSchemaInitializer.cs).

IF COL_LENGTH('dbo.Products', 'stock_quantity') IS NULL
BEGIN
    ALTER TABLE [dbo].[Products] ADD [stock_quantity] INT NOT NULL
        CONSTRAINT [DF_Products_stock_quantity] DEFAULT (0);
END;
GO

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
END;
GO
