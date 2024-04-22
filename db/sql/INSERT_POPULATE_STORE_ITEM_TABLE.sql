USE [StoreDB]
GO

INSERT INTO [dbo].[STORE_ITEM]
           ([Manufacturer]
           ,[ProductName]
           ,[QuantityAvailable]
           ,[Price]
           ,[ImageUrl])
     VALUES
			('Samsung', 'Galaxy S24 Ultra', 10, 1299.99, NULL),
			('Samsung', 'Galaxy S24 Plus', 25, 1099.99, NULL),
			('Samsung', 'Galaxy S24', 35, 999.99, NULL),
			('Apple', 'iPhone 15 Pro Max', 15, 1299.99, NULL),
			('Apple', 'iPhone 15 Pro', 20, 1099.99, NULL),
			('Apple', 'iPhone 15', 12, 899.99, NULL),
			('Google', 'Pixel 7 Pro', 18, 1199.99, NULL),
			('Google', 'Pixel 7', 30, 999.99, NULL),
			('OnePlus', 'OnePlus 12 Pro', 22, 1099.99, NULL),
			('OnePlus', 'OnePlus 12', 17, 899.99, NULL),
			('Xiaomi', 'Mi 12 Ultra', 28, 1199.99, NULL),
			('Xiaomi', 'Mi 12', 33, 999.99, NULL),
			('Huawei', 'Mate 50 Pro', 14, 1199.99, NULL),
			('Huawei', 'Mate 50', 19, 999.99, NULL),
			('Sony', 'Xperia 3 Pro', 25, 1099.99, NULL),
			('Sony', 'Xperia 3', 20, 899.99, NULL),
			('LG', 'LG V90 ThinQ', 15, 999.99, NULL),
			('LG', 'LG G10', 20, 799.99, NULL),
			('Motorola', 'Moto Edge 30 Pro', 12, 999.99, NULL),
			('Motorola', 'Moto Edge 30', 18, 799.99, NULL)
GO

