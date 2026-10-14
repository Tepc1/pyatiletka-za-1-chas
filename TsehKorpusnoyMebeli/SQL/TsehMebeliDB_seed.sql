-- TsehMebeliDB — данные, индексы, представления и процедуры (выполнять после схемы TsehMebeliDB.sql)
USE TsehMebeliDB;
GO

DECLARE @CustomersJson NVARCHAR(MAX) = N'[
	{"id": "000000001", "name": "ООО \"Поставка\"", "inn": "", "addres": "г.Пятигорск", "phone": "+79198634592", "salesman": true, "buyer": true},
	{"id": "000000002", "name": "ООО \"Кинотеатр Квант\"", "inn": "26320045123", "addres": "г. Железноводск, ул. Мира, 123", "phone": "+79884581555", "salesman": true, "buyer": false},
	{"id": "000000008", "name": "ООО \"Новый JDTO\"", "inn": "26320045111", "addres": "г. Железноводсу", "phone": "+79884581555", "salesman": true, "buyer": false},
	{"id": "000000003", "name": "ООО \"Ромашка\"", "inn": "4140784214", "addres": "г. Омск, ул. Строителей, 294", "phone": "+79882584546", "salesman": false, "buyer": true},
	{"id": "000000009", "name": "ООО \"Ипподром\"", "inn": "5874045632", "addres": "г. Уфа, ул. Набережная,  37", "phone": "+79627486389", "salesman": true, "buyer": true},
	{"id": "000000010", "name": "ООО \"Ассоль\"", "inn": "2629011278", "addres": "г. Калуга, ул. Пушкина, 94", "phone": "+79184572398", "salesman": false, "buyer": true}
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

INSERT INTO dbo.Product(Code, Name, Unit)
SELECT v.Code, v.Name, N'шт' FROM (VALUES
(N'НФ-00000011',N'Шкаф-купе 2-дверный'),(N'НФ-00000012',N'Шкаф-купе 3-дверный'),(N'НФ-00000013',N'Комод 4 ящика'),
(N'НФ-00000014',N'Тумба под ТВ'),(N'НФ-00000015',N'Стол письменный'),(N'НФ-00000016',N'Кухонный гарнитур прямой'),
(N'НФ-00000017',N'Кухонный гарнитур угловой'),(N'НФ-00000018',N'Стеллаж книжный'),(N'НФ-00000019',N'Тумба прикроватная'),
(N'НФ-00000020',N'Шкаф для одежды'),(N'НФ-00000021',N'Стол обеденный')) AS v(Code,Name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Product p WHERE p.Code=v.Code);

INSERT INTO dbo.Material(Name, Unit, Price)
SELECT v.Name,v.Unit,v.Price FROM (VALUES
(N'ЛДСП 16 мм белый',N'м2',850.00),(N'ЛДСП 16 мм дуб сонома',N'м2',920.00),(N'МДФ 19 мм',N'м2',1100.00),
(N'Кромка ПВХ 0.4 мм',N'м',18.00),(N'Кромка ПВХ 2 мм',N'м',35.00),(N'Петля накладная',N'шт',45.00),
(N'Направляющие шариковые 450 мм',N'комплект',280.00),(N'Ручка мебельная',N'шт',65.00),(N'Штанга для одежды',N'м',120.00),
(N'Зеркало 4 мм',N'м2',650.00),(N'Клей ПВА-D3',N'кг',95.00)) AS v(Name,Unit,Price)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Material m WHERE m.Name=v.Name);

INSERT INTO dbo.Operation(Name, Unit, Price)
SELECT v.Name,N'нормочас',v.Price FROM (VALUES
(N'Раскрой ЛДСП',350.00),(N'Кромкование',280.00),(N'Присадка (сверление)',320.00),(N'Сборка корпуса',400.00),
(N'Установка фурнитуры',250.00),(N'Установка зеркала',300.00),(N'Упаковка',150.00),(N'Контроль качества',200.00),
(N'Покраска МДФ',450.00),(N'Фрезерование',380.00),(N'Шлифовка',220.00)) AS v(Name,Price)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Operation o WHERE o.Name=v.Name);
GO

INSERT INTO dbo.Specification(ProductId)
SELECT p.Id FROM dbo.Product p WHERE NOT EXISTS (SELECT 1 FROM dbo.Specification s WHERE s.ProductId=p.Id);
GO
