USE [StoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER TRIGGER UPDATE_INVENTORY_QUANTITY_ON_PURCHASE
   ON  [dbo].[PURCHASE_STORE_ITEM]
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	BEGIN TRANSACTION;
		DECLARE StoreItemPkIds CURSOR FOR (SELECT StoreItemId from inserted);  -- todo upto also need quantity here
		OPEN StoreItemPkIds;
		DECLARE @StoreItemPkId INT;
	
		FETCH StoreItemPkIds INTO @StoreItemPkId;
		WHILE @@FETCH_STATUS = 0
		BEGIN
			-- For each inserted store_item, remove the quantity inserted
			DECLARE @NewQuantity INT;
			DECLARE @PreviousQuantity INT;
			SET @PreviousQuantity = (SELECT QuantityAvailable FROM STORE_ITEM WHERE StoreItemId = @StoreItemPkId)
			SET @NewQuantity = @PreviousQuantity - -- todo upto get quantity of inserted purchase_store_item and subtract
			UPDATE STORE_ITEM SET Q
		END
		CLOSE StoreItemPkIds;
		DEALLOCATE StoreItemPkIds;
	COMMIT TRANSACTION;
END
GO