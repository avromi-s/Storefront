DECLARE @json_data NVARCHAR(MAX)
SET @json_data = '{"StoreItemId": "1", "Quantity": 1, "UnitPrice": 25}'
DECLARE @CustomerId INT = 1;
EXECUTE [dbo].CREATE_NEW_PURCHASE @CustomerId, @json_data