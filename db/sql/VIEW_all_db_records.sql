use [StoreDB]

select * from
PURCHASE
inner join
customer
on purchase.CustomerId = customer.CustomerId

inner join 
PURCHASE_STORE_ITEM
on purchase.PurchaseId = PURCHASE_STORE_ITEM.PurchaseId
inner join 
STORE_ITEM
on PURCHASE_STORE_ITEM.StoreItemId = store_item.StoreItemId

select * from purchase

select * from PURCHASE_STORE_ITEM