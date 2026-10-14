-- УП06, вариант 22 — Автоцентр «Плюс»
IF DB_ID(N'AutoCenterPlusDB') IS NULL
    CREATE DATABASE AutoCenterPlusDB;
GO
USE AutoCenterPlusDB;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO
GO

IF OBJECT_ID(N'dbo.Customer', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customer (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customer PRIMARY KEY,
        Code NVARCHAR(20) NULL,
        Name NVARCHAR(200) NOT NULL,
        INN VARCHAR(12) NULL,
        Address NVARCHAR(500) NULL,
        Phone NVARCHAR(30) NULL,
        Email NVARCHAR(100) NULL,
        IsBuyer BIT NOT NULL CONSTRAINT DF_Customer_IsBuyer DEFAULT (0),
        IsSalesman BIT NOT NULL CONSTRAINT DF_Customer_IsSalesman DEFAULT (0),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Customer_CreatedAt DEFAULT (SYSUTCDATETIME()),
        Note NVARCHAR(500) NULL
    );
END;
IF OBJECT_ID(N'dbo.Product', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Product (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Product PRIMARY KEY,
        Code NVARCHAR(20) NULL,
        Name NVARCHAR(250) NOT NULL,
        Unit NVARCHAR(20) NOT NULL CONSTRAINT CK_Product_Unit CHECK (Unit IN (N'шт',N'кг',N'л',N'м',N'т',N'мин',N'ч',N'нормочас'))
    );
END;
IF OBJECT_ID(N'dbo.Material', N'U') IS NULL
    CREATE TABLE dbo.Material (Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Material PRIMARY KEY, Name NVARCHAR(200) NOT NULL, Unit NVARCHAR(20) NOT NULL, Price DECIMAL(12,2) NOT NULL CONSTRAINT CK_Material_Price CHECK (Price >= 0));
IF OBJECT_ID(N'dbo.Operation', N'U') IS NULL
    CREATE TABLE dbo.Operation (Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Operation PRIMARY KEY, Name NVARCHAR(200) NOT NULL, Unit NVARCHAR(20) NOT NULL, Price DECIMAL(12,2) NOT NULL CONSTRAINT CK_Operation_Price CHECK (Price >= 0));
IF OBJECT_ID(N'dbo.Specification', N'U') IS NULL
    CREATE TABLE dbo.Specification (Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Specification PRIMARY KEY, ProductId INT NOT NULL CONSTRAINT UQ_Specification_Product UNIQUE, CONSTRAINT FK_Specification_Product FOREIGN KEY (ProductId) REFERENCES dbo.Product(Id) ON DELETE CASCADE);
IF OBJECT_ID(N'dbo.SpecMaterial', N'U') IS NULL
    CREATE TABLE dbo.SpecMaterial (Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SpecMaterial PRIMARY KEY, SpecId INT NOT NULL, MaterialId INT NOT NULL, Norm DECIMAL(10,4) NOT NULL CONSTRAINT CK_SpecMaterial_Norm CHECK (Norm > 0), CONSTRAINT FK_SpecMaterial_Specification FOREIGN KEY (SpecId) REFERENCES dbo.Specification(Id) ON DELETE CASCADE, CONSTRAINT FK_SpecMaterial_Material FOREIGN KEY (MaterialId) REFERENCES dbo.Material(Id), CONSTRAINT UQ_SpecMaterial UNIQUE (SpecId, MaterialId));
IF OBJECT_ID(N'dbo.SpecOperation', N'U') IS NULL
    CREATE TABLE dbo.SpecOperation (Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SpecOperation PRIMARY KEY, SpecId INT NOT NULL, OperationId INT NOT NULL, Norm DECIMAL(10,4) NOT NULL CONSTRAINT CK_SpecOperation_Norm CHECK (Norm > 0), CONSTRAINT FK_SpecOperation_Specification FOREIGN KEY (SpecId) REFERENCES dbo.Specification(Id) ON DELETE CASCADE, CONSTRAINT FK_SpecOperation_Operation FOREIGN KEY (OperationId) REFERENCES dbo.Operation(Id), CONSTRAINT UQ_SpecOperation UNIQUE (SpecId, OperationId));
IF OBJECT_ID(N'dbo.[Order]', N'U') IS NULL
    CREATE TABLE dbo.[Order] (Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Order PRIMARY KEY, CustomerId INT NOT NULL, OrderDate DATE NOT NULL CONSTRAINT DF_Order_OrderDate DEFAULT (CONVERT(date, GETDATE())), TotalAmount DECIMAL(14,2) NOT NULL CONSTRAINT DF_Order_TotalAmount DEFAULT (0), CONSTRAINT CK_Order_TotalAmount CHECK (TotalAmount >= 0), CONSTRAINT FK_Order_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer(Id));
IF OBJECT_ID(N'dbo.OrderDetail', N'U') IS NULL
    CREATE TABLE dbo.OrderDetail (Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderDetail PRIMARY KEY, OrderId INT NOT NULL, ProductId INT NOT NULL, Quantity DECIMAL(10,2) NOT NULL CONSTRAINT CK_OrderDetail_Quantity CHECK (Quantity > 0), CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (OrderId) REFERENCES dbo.[Order](Id) ON DELETE CASCADE, CONSTRAINT FK_OrderDetail_Product FOREIGN KEY (ProductId) REFERENCES dbo.Product(Id), CONSTRAINT UQ_OrderDetail_OrderProduct UNIQUE (OrderId, ProductId));
IF OBJECT_ID(N'dbo.[User]', N'U') IS NULL
    CREATE TABLE dbo.[User] (Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_User PRIMARY KEY, Login NVARCHAR(50) NOT NULL CONSTRAINT UQ_User_Login UNIQUE, PasswordHash NVARCHAR(256) NOT NULL, Salt NVARCHAR(64) NOT NULL, Role NVARCHAR(20) NOT NULL CONSTRAINT CK_User_Role CHECK (Role IN (N'Admin',N'Manager',N'Operator')), FullName NVARCHAR(200) NULL);
IF OBJECT_ID(N'dbo.OrderHistory', N'U') IS NULL
    CREATE TABLE dbo.OrderHistory (Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderHistory PRIMARY KEY, OrderId INT NOT NULL, ChangeDate DATETIME2 NOT NULL CONSTRAINT DF_OrderHistory_ChangeDate DEFAULT (SYSUTCDATETIME()), OldStatus NVARCHAR(50) NULL, NewStatus NVARCHAR(50) NULL, ChangedBy INT NULL, CONSTRAINT FK_OrderHistory_Order FOREIGN KEY (OrderId) REFERENCES dbo.[Order](Id) ON DELETE CASCADE);
IF OBJECT_ID(N'dbo.CustomerDiscount', N'U') IS NULL
    CREATE TABLE dbo.CustomerDiscount (Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerDiscount PRIMARY KEY, CustomerId INT NOT NULL, OrderId INT NOT NULL CONSTRAINT UQ_CustomerDiscount_Order UNIQUE, DiscountAmount DECIMAL(12,2) NOT NULL CONSTRAINT CK_CustomerDiscount_Amount CHECK (DiscountAmount >= 0), CONSTRAINT FK_CustomerDiscount_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer(Id), CONSTRAINT FK_CustomerDiscount_Order FOREIGN KEY (OrderId) REFERENCES dbo.[Order](Id));
GO

-- Импорт исходного справочника через OPENJSON (ключ addres сохранён как в JSON).
DECLARE @CustomersJson NVARCHAR(MAX) = N'[
	{
		"id": "000000001",
		"name": "ООО \"Поставка\"",
		"inn": "",
		"addres": "г.Пятигорск",
		"phone": "+79198634592",
		"salesman": true,
		"buyer": true
	},
	{
		"id": "000000002",
		"name": "ООО \"Кинотеатр Квант\"",
		"inn": "26320045123",
		"addres": "г. Железноводск, ул. Мира, 123",
		"phone": "+79884581555",
		"salesman": true,
		"buyer": false
	},
	{
		"id": "000000008",
		"name": "ООО \"Новый JDTO\"",
		"inn": "26320045111",
		"addres": "г. Железноводсу",
		"phone": "+79884581555",
		"salesman": true,
		"buyer": false
	},
	{
		"id": "000000003",
		"name": "ООО \"Ромашка\"",
		"inn": "4140784214",
		"addres": "г. Омск, ул. Строителей, 294",
		"phone": "+79882584546",
		"salesman": false,
		"buyer": true
	},
	{
		"id": "000000009",
		"name": "ООО \"Ипподром\"",
		"inn": "5874045632",
		"addres": "г. Уфа, ул. Набережная,  37",
		"phone": "+79627486389",
		"salesman": true,
		"buyer": true
	},
	{
		"id": "000000010",
		"name": "ООО \"Ассоль\"",
		"inn": "2629011278",
		"addres": "г. Калуга, ул. Пушкина, 94",
		"phone": "+79184572398",
		"salesman": false,
		"buyer": true
	}
]';
INSERT INTO dbo.Customer (Code, Name, INN, Address, Phone, IsBuyer, IsSalesman)
SELECT source.Code, source.Name, NULLIF(source.INN, ''), source.Address, source.Phone, source.IsBuyer, source.IsSalesman
FROM OPENJSON(@CustomersJson)
WITH (Code NVARCHAR(20) '$.id', Name NVARCHAR(200) '$.name', INN VARCHAR(12) '$.inn', Address NVARCHAR(500) '$.addres', Phone NVARCHAR(30) '$.phone', IsBuyer BIT '$.buyer', IsSalesman BIT '$.salesman') AS source
WHERE NOT EXISTS (SELECT 1 FROM dbo.Customer AS existing WHERE existing.Code = source.Code);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Customer') AND name = N'UX_Customer_INN')
    CREATE UNIQUE INDEX UX_Customer_INN ON dbo.Customer(INN) WHERE INN IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Customer') AND name = N'UX_Customer_Code')
    CREATE UNIQUE INDEX UX_Customer_Code ON dbo.Customer(Code) WHERE Code IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Product') AND name = N'UX_Product_Code')
    CREATE UNIQUE INDEX UX_Product_Code ON dbo.Product(Code) WHERE Code IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Product') AND name = N'UX_Product_Name')
    CREATE UNIQUE INDEX UX_Product_Name ON dbo.Product(Name);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Order') AND name = N'IX_Order_OrderDate')
    CREATE INDEX IX_Order_OrderDate ON dbo.[Order](OrderDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OrderDetail') AND name = N'IX_OrderDetail_ProductId')
    CREATE INDEX IX_OrderDetail_ProductId ON dbo.OrderDetail(ProductId);
GO

-- Справочники (не менее 10 тестовых записей в каждом).
INSERT INTO dbo.Product(Code, Name, Unit)
SELECT v.Code, v.Name, N'шт' FROM (VALUES
(N'НФ-00000027',N'Диагностика подвески'),(N'НФ-00000028',N'Замена свечей зажигания'),(N'НФ-00000029',N'Замена масла ДВС'),(N'НФ-00000030',N'Замена тормозных колодок'),(N'НФ-00000031',N'Компьютерная диагностика'),(N'НФ-00000032',N'Развал-схождение'),(N'НФ-00000033',N'Замена аккумулятора'),(N'НФ-00000034',N'Шиномонтаж (комплект)'),(N'НФ-00000035',N'Замена антифриза'),(N'НФ-00000036',N'Химчистка салона'),(N'НФ-00000037',N'Замена ремня ГРМ')) AS v(Code,Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Product p WHERE p.Code=v.Code);
INSERT INTO dbo.Material(Name, Unit, Price)
SELECT v.Name,v.Unit,v.Price FROM (VALUES
(N'Масло моторное 5W-40',N'л',450.00),(N'Фильтр масляный',N'шт',350.00),(N'Колодки тормозные',N'комплект',1200.00),(N'Тормозная жидкость',N'л',280.00),(N'Свечи зажигания',N'шт',250.00),(N'Фильтр воздушный',N'шт',320.00),(N'Фильтр салонный',N'шт',280.00),(N'Аккумулятор 60 Ач',N'шт',5200.00),(N'Ремень ГРМ',N'шт',1100.00),(N'Антифриз G12',N'л',180.00),(N'Жидкость омывайка',N'л',60.00)) AS v(Name,Unit,Price)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Material m WHERE m.Name=v.Name);
INSERT INTO dbo.Operation(Name, Unit, Price)
SELECT v.Name,N'нормочас',v.Price FROM (VALUES
(N'Подъём автомобиля',150.00),(N'Демонтаж колеса',200.00),(N'Дефектовка подвески',350.00),(N'Замер люфтов',250.00),(N'Свободный доступ',100.00),(N'Замена свечи зажигания',300.00),(N'Проверка момента затяжки',150.00),(N'Замена масла',280.00),(N'Замена фильтра',200.00),(N'Утилизация отработанного масла',120.00),(N'Контроль уровня жидкостей',100.00)) AS v(Name,Price)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Operation o WHERE o.Name=v.Name);
GO
INSERT INTO dbo.Specification(ProductId)
SELECT p.Id FROM dbo.Product p WHERE NOT EXISTS (SELECT 1 FROM dbo.Specification s WHERE s.ProductId=p.Id);
GO
INSERT INTO dbo.SpecMaterial(SpecId,MaterialId,Norm)
SELECT s.Id,m.Id,v.Norm FROM (VALUES
(N'Диагностика подвески',N'Жидкость омывайка',0.5),(N'Замена свечей зажигания',N'Свечи зажигания',4.0),(N'Замена масла ДВС',N'Масло моторное 5W-40',4.0),(N'Замена масла ДВС',N'Фильтр масляный',1.0),(N'Замена тормозных колодок',N'Колодки тормозные',1.0),(N'Замена тормозных колодок',N'Тормозная жидкость',0.5),(N'Замена аккумулятора',N'Аккумулятор 60 Ач',1.0),(N'Замена ремня ГРМ',N'Ремень ГРМ',1.0),(N'Замена антифриза',N'Антифриз G12',5.0)) AS v(ProductName,MaterialName,Norm)
JOIN dbo.Product p ON p.Name=v.ProductName JOIN dbo.Specification s ON s.ProductId=p.Id JOIN dbo.Material m ON m.Name=v.MaterialName
WHERE NOT EXISTS (SELECT 1 FROM dbo.SpecMaterial sm WHERE sm.SpecId=s.Id AND sm.MaterialId=m.Id);
INSERT INTO dbo.SpecOperation(SpecId,OperationId,Norm)
SELECT s.Id,o.Id,v.Norm FROM (VALUES
(N'Диагностика подвески',N'Подъём автомобиля',0.2),(N'Диагностика подвески',N'Демонтаж колеса',0.4),(N'Диагностика подвески',N'Дефектовка подвески',0.8),(N'Диагностика подвески',N'Замер люфтов',0.5),(N'Замена свечей зажигания',N'Свободный доступ',0.3),(N'Замена свечей зажигания',N'Замена свечи зажигания',1.2),(N'Замена свечей зажигания',N'Проверка момента затяжки',0.2),(N'Замена масла ДВС',N'Подъём автомобиля',0.2),(N'Замена масла ДВС',N'Замена масла',0.5),(N'Замена масла ДВС',N'Замена фильтра',0.3),(N'Замена масла ДВС',N'Утилизация отработанного масла',0.2),(N'Замена тормозных колодок',N'Демонтаж колеса',0.4),(N'Замена тормозных колодок',N'Замена фильтра',0.8),(N'Замена тормозных колодок',N'Контроль уровня жидкостей',0.2)) AS v(ProductName,OperationName,Norm)
JOIN dbo.Product p ON p.Name=v.ProductName JOIN dbo.Specification s ON s.ProductId=p.Id JOIN dbo.Operation o ON o.Name=v.OperationName
WHERE NOT EXISTS (SELECT 1 FROM dbo.SpecOperation so WHERE so.SpecId=s.Id AND so.OperationId=o.Id);
GO
-- Дополнительные клиенты для проверки CRUD и отчёта; вместе с JSON и ООО «Фермер» — 10 записей.
INSERT INTO dbo.Customer(Code,Name,INN,Address,Phone,IsBuyer,IsSalesman)
SELECT v.Code,v.Name,v.INN,v.Address,v.Phone,1,0 FROM (VALUES
('000000012',N'Тестовый клиент 1','7701000001',N'г. Миасс',N'+70000000001'),('000000013',N'Тестовый клиент 2','7701000002',N'г. Миасс',N'+70000000002'),('000000014',N'Тестовый клиент 3','7701000003',N'г. Миасс',N'+70000000003')) AS v(Code,Name,INN,Address,Phone)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Customer c WHERE c.Code=v.Code);
IF NOT EXISTS (SELECT 1 FROM dbo.Customer WHERE Code='000000011')
    INSERT INTO dbo.Customer(Code,Name,INN,Address,Phone,IsBuyer) VALUES('000000011',N'ООО "Фермер"','2632066145',N'г. Миасс, ул. Романенко, 18',N'+79198634500',1);
GO
-- Учебные пользователи. Хэш = SHA-256(UTF-8(пароль + соль)); смените пароли перед реальным использованием.
DECLARE @AdminSalt VARCHAR(36)=CONVERT(VARCHAR(36),NEWID());
IF NOT EXISTS (SELECT 1 FROM dbo.[User] WHERE Login=N'admin')
    INSERT INTO dbo.[User](Login,PasswordHash,Salt,Role,FullName) VALUES(N'admin',CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',CONVERT(VARCHAR(200),'Admin123'+@AdminSalt)),2),@AdminSalt,N'Admin',N'Администратор ИС');
DECLARE @ManagerSalt VARCHAR(36)=CONVERT(VARCHAR(36),NEWID());
IF NOT EXISTS (SELECT 1 FROM dbo.[User] WHERE Login=N'manager')
    INSERT INTO dbo.[User](Login,PasswordHash,Salt,Role,FullName) VALUES(N'manager',CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',CONVERT(VARCHAR(200),'Manager123'+@ManagerSalt)),2),@ManagerSalt,N'Manager',N'Менеджер Петров С.А.');
DECLARE @OperatorSalt VARCHAR(36)=CONVERT(VARCHAR(36),NEWID());
IF NOT EXISTS (SELECT 1 FROM dbo.[User] WHERE Login=N'operator')
    INSERT INTO dbo.[User](Login,PasswordHash,Salt,Role,FullName) VALUES(N'operator',CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',CONVERT(VARCHAR(200),'Operator123'+@OperatorSalt)),2),@OperatorSalt,N'Operator',N'Оператор Иванова М.И.');
GO
-- Заказ варианта 22 и отдельные заказы для проверки скидок 0% и >20%.
IF NOT EXISTS (SELECT 1 FROM dbo.[Order] o JOIN dbo.Customer c ON c.Id=o.CustomerId WHERE c.Code='000000011' AND o.OrderDate='2025-07-01')
BEGIN
    DECLARE @FarmerId INT=(SELECT Id FROM dbo.Customer WHERE Code='000000011');
    INSERT dbo.[Order](CustomerId,OrderDate) VALUES(@FarmerId,'2025-07-01');
    DECLARE @FarmerOrderId INT=CONVERT(INT,SCOPE_IDENTITY());
    INSERT dbo.OrderDetail(OrderId,ProductId,Quantity)
    SELECT @FarmerOrderId,p.Id,v.Quantity FROM (VALUES(N'Диагностика подвески',22.0),(N'Замена свечей зажигания',46.0),(N'Замена масла ДВС',8.0)) v(ProductName,Quantity) JOIN dbo.Product p ON p.Name=v.ProductName;
END;
IF NOT EXISTS (SELECT 1 FROM dbo.[Order] o JOIN dbo.Customer c ON c.Id=o.CustomerId WHERE c.Code='000000003')
BEGIN
    INSERT dbo.[Order](CustomerId,OrderDate) SELECT Id,'2025-07-05' FROM dbo.Customer WHERE Code='000000003';
    INSERT dbo.OrderDetail(OrderId,ProductId,Quantity) SELECT SCOPE_IDENTITY(),Id,5 FROM dbo.Product WHERE Name=N'Замена тормозных колодок';
END;
IF NOT EXISTS (SELECT 1 FROM dbo.[Order] o JOIN dbo.Customer c ON c.Id=o.CustomerId WHERE c.Code='000000010')
BEGIN
    INSERT dbo.[Order](CustomerId,OrderDate) SELECT Id,'2025-07-10' FROM dbo.Customer WHERE Code='000000010';
    INSERT dbo.OrderDetail(OrderId,ProductId,Quantity) SELECT SCOPE_IDENTITY(),Id,2 FROM dbo.Product WHERE Name=N'Замена аккумулятора';
END;
GO
CREATE OR ALTER VIEW dbo.v_OrderFullCost
AS
SELECT o.Id AS OrderId,c.Id AS CustomerId,c.Name AS CustomerName,c.INN AS CustomerINN,o.OrderDate,
       p.Id AS ProductId,COALESCE(p.Code,CONVERT(VARCHAR(20),p.Id)) AS ProductCode,p.Name AS ProductName,p.Unit,od.Quantity,
       COALESCE(mc.MaterialsCost,0) AS MaterialsCostPerUnit,COALESCE(oc.OperationsCost,0) AS OperationsCostPerUnit,
       od.Quantity*(COALESCE(mc.MaterialsCost,0)+COALESCE(oc.OperationsCost,0)) AS TotalPositionCost
FROM dbo.[Order] o JOIN dbo.Customer c ON c.Id=o.CustomerId JOIN dbo.OrderDetail od ON od.OrderId=o.Id
JOIN dbo.Product p ON p.Id=od.ProductId LEFT JOIN dbo.Specification s ON s.ProductId=p.Id
OUTER APPLY (SELECT SUM(m.Price*sm.Norm) AS MaterialsCost FROM dbo.SpecMaterial sm JOIN dbo.Material m ON m.Id=sm.MaterialId WHERE sm.SpecId=s.Id) mc
OUTER APPLY (SELECT SUM(op.Price*so.Norm) AS OperationsCost FROM dbo.SpecOperation so JOIN dbo.Operation op ON op.Id=so.OperationId WHERE so.SpecId=s.Id) oc;
GO
CREATE OR ALTER PROCEDURE dbo.sp_CalculateOrderCost @OrderId INT, @TotalCost DECIMAL(14,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @TotalCost=COALESCE(SUM(TotalPositionCost),0) FROM dbo.v_OrderFullCost WHERE OrderId=@OrderId;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_UpdateOrderTotal @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.[Order] SET TotalAmount=COALESCE((SELECT SUM(TotalPositionCost) FROM dbo.v_OrderFullCost WHERE OrderId=@OrderId),0) WHERE Id=@OrderId;
END;
GO
-- Фиксированные скидки только для демонстрационных заказов; скидка распределяется по позициям пропорционально сумме.
INSERT INTO dbo.CustomerDiscount(CustomerId,OrderId,DiscountAmount)
SELECT c.Id,o.Id,v.Discount FROM (VALUES('000000011',8000.00),('000000003',3000.00),('000000010',0.00)) v(Code,Discount)
JOIN dbo.Customer c ON c.Code=v.Code JOIN dbo.[Order] o ON o.CustomerId=c.Id
WHERE NOT EXISTS (SELECT 1 FROM dbo.CustomerDiscount d WHERE d.OrderId=o.Id);
GO
CREATE OR ALTER VIEW dbo.v_ClientOrderAnalysis
AS
WITH Lines AS
(
    SELECT f.CustomerId AS ClientId, f.CustomerName AS ClientName, f.ProductName, f.ProductCode,
           f.Quantity, f.Unit, f.MaterialsCostPerUnit+f.OperationsCostPerUnit AS Price, f.TotalPositionCost AS TotalSum,
           f.OrderId, COALESCE(d.DiscountAmount,0) AS OrderDiscount,
           SUM(f.TotalPositionCost) OVER(PARTITION BY f.OrderId) AS OrderAmount
    FROM dbo.v_OrderFullCost f LEFT JOIN dbo.CustomerDiscount d ON d.OrderId=f.OrderId AND d.CustomerId=f.CustomerId
)
SELECT ClientId,ClientName,ProductName,ProductCode,Quantity,Unit,Price,
       CASE WHEN OrderAmount=0 THEN CONVERT(DECIMAL(12,2),0) ELSE CONVERT(DECIMAL(12,2),ROUND(OrderDiscount*TotalSum/NULLIF(OrderAmount,0),2)) END AS Discount,
       TotalSum,CASE WHEN OrderAmount=0 THEN CONVERT(DECIMAL(9,2),0) ELSE CONVERT(DECIMAL(9,2),ROUND(OrderDiscount*100.0/NULLIF(OrderAmount,0),2)) END AS DiscountPercent,OrderId
FROM Lines;
GO
-- Итоговые суммы сохраняются в заказах после создания расчётного представления.
UPDATE o SET TotalAmount=COALESCE(t.TotalCost,0) FROM dbo.[Order] o OUTER APPLY (SELECT SUM(TotalPositionCost) AS TotalCost FROM dbo.v_OrderFullCost f WHERE f.OrderId=o.Id) t;
GO
-- Проверка заполнения таблиц и расчёта стоимости.
SELECT 'Customer' AS TableName,COUNT(*) AS Cnt FROM dbo.Customer UNION ALL SELECT 'Product',COUNT(*) FROM dbo.Product UNION ALL SELECT 'Material',COUNT(*) FROM dbo.Material UNION ALL SELECT 'Operation',COUNT(*) FROM dbo.Operation UNION ALL SELECT 'Specification',COUNT(*) FROM dbo.Specification UNION ALL SELECT 'SpecMaterial',COUNT(*) FROM dbo.SpecMaterial UNION ALL SELECT 'SpecOperation',COUNT(*) FROM dbo.SpecOperation UNION ALL SELECT 'Order',COUNT(*) FROM dbo.[Order] UNION ALL SELECT 'OrderDetail',COUNT(*) FROM dbo.OrderDetail UNION ALL SELECT 'User',COUNT(*) FROM dbo.[User];
SELECT * FROM dbo.v_OrderFullCost ORDER BY OrderId,ProductName;
SELECT * FROM dbo.v_ClientOrderAnalysis ORDER BY ClientName,OrderId,ProductName;
GO
/*
ОБНОВЛЕНИЕ И РЕЗЕРВНОЕ КОПИРОВАНИЕ (выполняются отдельно после настройки путей и прав):

-- Уже включённые в начальную схему изменения: Customer.Note, Product.Name NVARCHAR(250), OrderHistory.
-- Для отдельной файловой группы задайте существующий путь, разрешённый службе SQL Server:
-- ALTER DATABASE AutoCenterPlusDB ADD FILEGROUP FG_Indexes;
-- ALTER DATABASE AutoCenterPlusDB ADD FILE (NAME=AutoCenterPlus_Indexes,FILENAME='D:\SQLData\AutoCenterPlus_Indexes.ndf',SIZE=20MB,FILEGROWTH=10MB) TO FILEGROUP FG_Indexes;
-- CREATE UNIQUE INDEX UX_Customer_INN ON dbo.Customer(INN) WHERE INN IS NOT NULL WITH (DROP_EXISTING=ON) ON FG_Indexes;

-- FULL еженедельно; DIFF ежедневно; LOG каждые 2 часа. Папку предварительно создайте и выдайте права SQL Server:
-- BACKUP DATABASE AutoCenterPlusDB TO DISK='D:\SQLBackups\AutoCenterPlusDB_FULL.bak' WITH INIT,COMPRESSION,CHECKSUM,STATS=10;
-- BACKUP DATABASE AutoCenterPlusDB TO DISK='D:\SQLBackups\AutoCenterPlusDB_DIFF.bak' WITH DIFFERENTIAL,INIT,COMPRESSION,CHECKSUM,STATS=10;
-- BACKUP LOG AutoCenterPlusDB TO DISK='D:\SQLBackups\AutoCenterPlusDB_LOG.trn' WITH INIT,COMPRESSION,CHECKSUM,STATS=10;
-- RESTORE VERIFYONLY FROM DISK='D:\SQLBackups\AutoCenterPlusDB_FULL.bak';
-- Для BACKUP LOG сначала настройте модель восстановления FULL, затем выполните полный backup.
-- Анализ производительности: SET STATISTICS IO ON; SET STATISTICS TIME ON; SELECT * FROM dbo.v_OrderFullCost WHERE OrderId=1; SET STATISTICS IO OFF; SET STATISTICS TIME OFF;
*/
