USE [StoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- This procedure is used for the full creation of a purchase. It does so by creating a PURCHASE and the linked 
-- PURCHASE_STORE_ITEMs based on the provided arguments.
-- This procedure also reduces the QuantityAvailable of the STORE_ITEMs purchased and the Balance of the Customer
-- based on the PURCHASE's details.
CREATE OR ALTER PROCEDURE [dbo].CREATE_NEW_PURCHASE
	@CustomerId INT,
	@PurchasedStoreItems PurchaseStoreItem READONLY
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
		-- Insert PURCHASE record so we can associate the PURCHASE_STORE_ITEMS with its PK
		-- We will update the PURCHASE record with the total price and quantity after iterating
		DECLARE @PurchaseId INT;
		DECLARE @PurchaseTotalQuantity INT = 0;
		DECLARE @PurchaseTotalPrice MONEY = 0;

		SET @PurchaseId = NEXT VALUE FOR [dbo].[PURCHASE].[PurchaseId];
		INSERT INTO [dbo].[PURCHASE]
           ([PurchaseId]
		   ,[CustomerId]
           ,[TotalQuantity]
           ,[TotalPrice]
           ,[PurchaseDateTime])
		 VALUES
			   (@PurchaseId
			   ,@CustomerId
			   ,-1
			   ,-1
			   ,GETDATE());

		-- For each PurchasedStoreItem:
		--		- insert it into the PURCHASE_STORE_ITEM table and link it to this purchase
		--		- remove the purchased quantity from the STORE_ITEM
		--		- reduce customer quantity by price * quantity (or total up and remove all after)
		DECLARE StoreItems CURSOR FOR (SELECT StoreItemId, Quantity, UnitPrice from @PurchasedStoreItems);
		OPEN StoreItems;
		DECLARE @StoreItemId INT;
		DECLARE @QuantityPurchased INT;
		DECLARE @UnitPrice MONEY;
	
		FETCH NEXT FROM StoreItems INTO @StoreItemId, @QuantityPurchased, @UnitPrice;
		WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO [dbo].[PURCHASE_STORE_ITEM]
				   ([PurchaseId]
				   ,[StoreItemId]
				   ,[Quantity]
				   ,[UnitPrice])
			 VALUES
				   (@PurchaseId
				   ,@StoreItemId
				   ,@QuantityPurchased
				   ,@UnitPrice)

			-- Reduce quantity of STORE_ITEM purchased
			DECLARE @NewQuantity INT;
			DECLARE @PreviousQuantity INT;
			SET @PreviousQuantity = (SELECT QuantityAvailable FROM STORE_ITEM WHERE StoreItemId = @StoreItemId);
			SET @NewQuantity = @PreviousQuantity - @QuantityPurchased;
			UPDATE STORE_ITEM SET QuantityAvailable = @NewQuantity WHERE StoreItemId = @StoreItemId;

			-- Reduce balance of customer for this STORE_ITEM purchased
			DECLARE @NewBalance MONEY;
			DECLARE @PreviousBalance MONEY;
			SET @PreviousBalance = (SELECT Balance FROM CUSTOMER WHERE CustomerId = @CustomerId);
			SET @NewBalance = @PreviousBalance - (@UnitPrice * @QuantityPurchased);
			UPDATE CUSTOMER SET Balance = @NewBalance WHERE CustomerId = @CustomerId;

			SET @PurchaseTotalQuantity += @QuantityPurchased;
			SET @PurchaseTotalPrice += (@UnitPrice * @QuantityPurchased);
		END
		CLOSE StoreItemPkIds;
		DEALLOCATE StoreItemPkIds;

		-- Update the PURCHASE record with the total price and quantity after iterating
		UPDATE [dbo].[PURCHASE]
		   SET [TotalQuantity] = @PurchaseTotalQuantity
			  ,[TotalPrice] = @PurchaseTotalPrice
		 WHERE PurchaseId = @PurchaseId;
	COMMIT TRANSACTION;
END
GO