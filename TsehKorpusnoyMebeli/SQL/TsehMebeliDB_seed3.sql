-- Продолжение seed TsehMebeliDB (заказы, представления, процедуры)
USE TsehMebeliDB;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.[Order] o JOIN dbo.Customer c ON c.Id=o.CustomerId WHERE c.Code='000000011' AND o.OrderDate='2025-07-01')
BEGIN
    DECLARE @MainId INT=(SELECT Id FROM dbo.Customer WHERE Code='000000011');
    INSERT dbo.[Order](CustomerId,OrderDate) VALUES(@MainId,'2025-07-01');
    DECLARE @MainOrderId INT=CONVERT(INT,SCOPE_IDENTITY());
    INSERT dbo.OrderDetail(OrderId,ProductId,Quantity)
    SELECT @MainOrderId,p.Id,v.Quantity FROM (VALUES
    (N'Шкаф-купе 2-дверный',3.0),(N'Комод 4 ящика',5.0),(N'Тумба под ТВ',2.0)) v(ProductName,Quantity)
    JOIN dbo.Product p ON p.Name=v.ProductName;
END;
IF NOT EXISTS (SELECT 1 FROM dbo.[Order] o JOIN dbo.Customer c ON c.Id=o.CustomerId WHERE c.Code='000000003')
BEGIN
    INSERT dbo.[Order](CustomerId,OrderDate) SELECT Id,'2025-07-05' FROM dbo.Customer WHERE Code='000000003';
    INSERT dbo.OrderDetail(OrderId,ProductId,Quantity) SELECT SCOPE_IDENTITY(),Id,2 FROM dbo.Product WHERE Name=N'Кухонный гарнитур прямой';
END;
IF NOT EXISTS (SELECT 1 FROM dbo.[Order] o JOIN dbo.Customer c ON c.Id=o.CustomerId WHERE c.Code='000000010')
BEGIN
    INSERT dbo.[Order](CustomerId,OrderDate) SELECT Id,'2025-07-10' FROM dbo.Customer WHERE Code='000000010';
    INSERT dbo.OrderDetail(OrderId,ProductId,Quantity) SELECT SCOPE_IDENTITY(),Id,4 FROM dbo.Product WHERE Name=N'Стол письменный';
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

INSERT INTO dbo.CustomerDiscount(CustomerId,OrderId,DiscountAmount)
SELECT c.Id,o.Id,v.Discount FROM (VALUES('000000011',15000.00),('000000003',5000.00),('000000010',0.00)) v(Code,Discount)
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

UPDATE o SET TotalAmount=COALESCE(t.TotalCost,0) FROM dbo.[Order] o OUTER APPLY (SELECT SUM(TotalPositionCost) AS TotalCost FROM dbo.v_OrderFullCost f WHERE f.OrderId=o.Id) t;
GO

SELECT 'Customer' AS TableName,COUNT(*) AS Cnt FROM dbo.Customer
UNION ALL SELECT 'Product',COUNT(*) FROM dbo.Product
UNION ALL SELECT 'Material',COUNT(*) FROM dbo.Material
UNION ALL SELECT 'Operation',COUNT(*) FROM dbo.Operation
UNION ALL SELECT 'Specification',COUNT(*) FROM dbo.Specification
UNION ALL SELECT 'SpecMaterial',COUNT(*) FROM dbo.SpecMaterial
UNION ALL SELECT 'SpecOperation',COUNT(*) FROM dbo.SpecOperation
UNION ALL SELECT 'Order',COUNT(*) FROM dbo.[Order]
UNION ALL SELECT 'OrderDetail',COUNT(*) FROM dbo.OrderDetail
UNION ALL SELECT 'User',COUNT(*) FROM dbo.[User];
SELECT * FROM dbo.v_OrderFullCost ORDER BY OrderId,ProductName;
SELECT * FROM dbo.v_ClientOrderAnalysis ORDER BY ClientName,OrderId,ProductName;
GO
