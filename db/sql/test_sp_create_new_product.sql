DECLARE @json_data NVARCHAR(MAX)
SET @json_data = '[{"StoreItemId": 2, "Quantity": 1, "UnitPrice": 25}
					,{"StoreItemId": 3, "Quantity": 10, "UnitPrice": 15}]'
DECLARE @CustomerId INT = 1;
EXECUTE [dbo].CREATE_NEW_PURCHASE @CustomerId, @json_data