USE [StoreDB]
GO

INSERT INTO [dbo].[STORE_ITEM]
           ([Manufacturer]
           ,[ProductName]
           ,[QuantityAvailable]
           ,[Price]
           ,[ImagePath])
     VALUES
			('Samsung', 'Galaxy S24 Ultra', 10, 1299.99, 'SGS24U.JPG'),
			('Samsung', 'Galaxy S24 Plus', 25, 1099.99, 'SGS24P.JPG'),
			('Samsung', 'Galaxy S24', 35, 999.99, 'SGS24.JPG'),
			('Apple', 'iPhone 15 Pro Max', 15, 1299.99, 'AI15PM.JPG'),
			('Apple', 'iPhone 15 Pro', 20, 1099.99, 'AI15P.JPG'),
			('Apple', 'iPhone 15', 12, 899.99, 'AI15.JPG'),
			('Google', 'Pixel 7 Pro', 18, 1199.99, 'GP7P.JPG'),
			('Google', 'Pixel 7', 30, 999.99, 'GP7.JPG'),
			('OnePlus', 'OnePlus 12R', 22, 1099.99, 'OP12R.JPG'),
			('OnePlus', 'OnePlus 12', 17, 899.99, 'OP12.JPG'),
			('Xiaomi', '12S Ultra', 28, 1199.99, 'X12SU.JPG'),
			('Xiaomi', '12', 33, 999.99, 'X12.JPG'),
			('Huawei', 'Mate 50 Pro', 14, 1199.99, 'HM50P.JPG'),
			('Huawei', 'Mate 50', 19, 999.99, 'HM50.JPG'),
			('Sony', 'Xperia Pro I 5G', 25, 1099.99, 'SXPI5G.JPG'),
			('Sony', 'Xperia 5 IV', 20, 999.99, 'SX5IV.JPG'),
			('Motorola', 'Moto G 5G 2023', 12, 149.99, 'MMG5G23.JPG'),
			('Motorola', 'Moto G Power 2023', 18, 199.99, 'MMGP5G23.JPG'),
			('Motorola', 'Razr+ 2023', 12, 999.99, 'MRP23.JPG'),
			('Nokia', 'C300', 18, 119.99, 'NC300.JPG')
GO

