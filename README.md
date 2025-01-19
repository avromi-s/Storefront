# Storefront

This project demoes a storefront allowing purchase of various store items.
Customers create accounts and can make purchases of one or more items; purchase prices are deducted from the customer's balance. Customers can pay to their balance and view previous order history.

The project consists of a Windows Forms app that displays the application and a SQL Server database that is used to store and retrieve all persistent data (customers, logins, purchases, store items). LINQ to SQL is used in the application for all queries.

Created for a school project.

***
### To run the project:
- Create a new database in SQL Server
- In the database, run:
    - [db/sql/CREATE_ALL_TABLES.sql](https://github.com/avrohom-schneierson/Storefront/blob/a2ad00de680819ed1041f5ca6096c08ee814db08/db/sql/CREATE_ALL_TABLES.sql),
    - [db/sql/CREATE_SP_CREATE_PURCHASE.sql](https://github.com/avrohom-schneierson/Storefront/blob/a2ad00de680819ed1041f5ca6096c08ee814db08/db/sql/CREATE_SP_CREATE_PURCHASE.sql)
    - [db/sql/INSERT_POPULATE_STORE_ITEM_TABLE.sql](https://github.com/avrohom-schneierson/Storefront/blob/a2ad00de680819ed1041f5ca6096c08ee814db08/db/sql/INSERT_POPULATE_STORE_ITEM_TABLE.sql)
- Download and run the program from the [releases](https://github.com/avromi-s/Storefront/releases), or build it from source

![image](https://github.com/user-attachments/assets/adca2844-33fd-49bf-9895-aa66470cdb9f)
