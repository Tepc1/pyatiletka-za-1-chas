-- Продолжение seed TsehMebeliDB (спецификации, пользователи, заказы, представления)
USE TsehMebeliDB;
GO

INSERT INTO dbo.SpecMaterial(SpecId,MaterialId,Norm)
SELECT s.Id,m.Id,v.Norm FROM (VALUES
(N'Шкаф-купе 2-дверный',N'ЛДСП 16 мм белый',4.5),(N'Шкаф-купе 2-дверный',N'Кромка ПВХ 2 мм',12.0),
(N'Шкаф-купе 2-дверный',N'Петля накладная',4.0),(N'Шкаф-купе 2-дверный',N'Штанга для одежды',1.2),
(N'Шкаф-купе 3-дверный',N'ЛДСП 16 мм дуб сонома',6.5),(N'Шкаф-купе 3-дверный',N'Кромка ПВХ 2 мм',18.0),
(N'Шкаф-купе 3-дверный',N'Петля накладная',6.0),(N'Комод 4 ящика',N'ЛДСП 16 мм белый',2.8),
(N'Комод 4 ящика',N'Направляющие шариковые 450 мм',4.0),(N'Комод 4 ящика',N'Ручка мебельная',4.0),
(N'Тумба под ТВ',N'ЛДСП 16 мм дуб сонома',1.8),(N'Тумба под ТВ',N'Кромка ПВХ 0.4 мм',6.0),
(N'Стол письменный',N'ЛДСП 16 мм белый',2.2),(N'Стол письменный',N'Кромка ПВХ 2 мм',5.0),
(N'Кухонный гарнитур прямой',N'ЛДСП 16 мм белый',8.0),(N'Кухонный гарнитур прямой',N'МДФ 19 мм',3.0),
(N'Кухонный гарнитур прямой',N'Направляющие шариковые 450 мм',6.0),(N'Кухонный гарнитур угловой',N'ЛДСП 16 мм дуб сонома',12.0),
(N'Кухонный гарнитур угловой',N'МДФ 19 мм',4.5),(N'Стеллаж книжный',N'ЛДСП 16 мм белый',3.5),
(N'Тумба прикроватная',N'ЛДСП 16 мм белый',0.9),(N'Тумба прикроватная',N'Ручка мебельная',1.0),
(N'Шкаф для одежды',N'ЛДСП 16 мм дуб сонома',5.0),(N'Шкаф для одежды',N'Штанга для одежды',1.5),
(N'Стол обеденный',N'ЛДСП 16 мм дуб сонома',2.5),(N'Стол обеденный',N'Кромка ПВХ 2 мм',4.0)) AS v(ProductName,MaterialName,Norm)
JOIN dbo.Product p ON p.Name=v.ProductName JOIN dbo.Specification s ON s.ProductId=p.Id JOIN dbo.Material m ON m.Name=v.MaterialName
WHERE NOT EXISTS (SELECT 1 FROM dbo.SpecMaterial sm WHERE sm.SpecId=s.Id AND sm.MaterialId=m.Id);

INSERT INTO dbo.SpecOperation(SpecId,OperationId,Norm)
SELECT s.Id,o.Id,v.Norm FROM (VALUES
(N'Шкаф-купе 2-дверный',N'Раскрой ЛДСП',1.5),(N'Шкаф-купе 2-дверный',N'Кромкование',1.0),
(N'Шкаф-купе 2-дверный',N'Присадка (сверление)',0.8),(N'Шкаф-купе 2-дверный',N'Сборка корпуса',2.0),
(N'Шкаф-купе 2-дверный',N'Установка фурнитуры',0.5),(N'Шкаф-купе 3-дверный',N'Раскрой ЛДСП',2.0),
(N'Шкаф-купе 3-дверный',N'Кромкование',1.5),(N'Шкаф-купе 3-дверный',N'Сборка корпуса',2.5),
(N'Комод 4 ящика',N'Раскрой ЛДСП',0.8),(N'Комод 4 ящика',N'Присадка (сверление)',0.6),
(N'Комод 4 ящика',N'Сборка корпуса',1.2),(N'Комод 4 ящика',N'Установка фурнитуры',0.4),
(N'Тумба под ТВ',N'Раскрой ЛДСП',0.5),(N'Тумба под ТВ',N'Сборка корпуса',0.8),
(N'Стол письменный',N'Раскрой ЛДСП',0.6),(N'Стол письменный',N'Кромкование',0.4),(N'Стол письменный',N'Сборка корпуса',0.7),
(N'Кухонный гарнитур прямой',N'Раскрой ЛДСП',3.0),(N'Кухонный гарнитур прямой',N'Кромкование',2.0),(N'Кухонный гарнитур прямой',N'Сборка корпуса',4.0),
(N'Кухонный гарнитур угловой',N'Раскрой ЛДСП',4.0),(N'Кухонный гарнитур угловой',N'Сборка корпуса',5.0),
(N'Стеллаж книжный',N'Раскрой ЛДСП',1.0),(N'Стеллаж книжный',N'Сборка корпуса',1.5),
(N'Тумба прикроватная',N'Раскрой ЛДСП',0.3),(N'Тумба прикроватная',N'Сборка корпуса',0.5),
(N'Шкаф для одежды',N'Раскрой ЛДСП',1.8),(N'Шкаф для одежды',N'Сборка корпуса',2.2),
(N'Стол обеденный',N'Раскрой ЛДСП',0.7),(N'Стол обеденный',N'Кромкование',0.5),(N'Стол обеденный',N'Сборка корпуса',0.9)) AS v(ProductName,OperationName,Norm)
JOIN dbo.Product p ON p.Name=v.ProductName JOIN dbo.Specification s ON s.ProductId=p.Id JOIN dbo.Operation o ON o.Name=v.OperationName
WHERE NOT EXISTS (SELECT 1 FROM dbo.SpecOperation so WHERE so.SpecId=s.Id AND so.OperationId=o.Id);
GO

INSERT INTO dbo.Customer(Code,Name,INN,Address,Phone,IsBuyer,IsSalesman)
SELECT v.Code,v.Name,v.INN,v.Address,v.Phone,1,0 FROM (VALUES
('000000012',N'Тестовый клиент 1','7701000001',N'г. Миасс',N'+70000000001'),
('000000013',N'Тестовый клиент 2','7701000002',N'г. Миасс',N'+70000000002'),
('000000014',N'Тестовый клиент 3','7701000003',N'г. Миасс',N'+70000000003')) AS v(Code,Name,INN,Address,Phone)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Customer c WHERE c.Code=v.Code);
IF NOT EXISTS (SELECT 1 FROM dbo.Customer WHERE Code='000000011')
    INSERT INTO dbo.Customer(Code,Name,INN,Address,Phone,IsBuyer) VALUES('000000011',N'ООО "Мебель-Дом"','2632066145',N'г. Миасс, ул. Романенко, 18',N'+79198634500',1);
GO

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
