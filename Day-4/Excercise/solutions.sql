-- 1) List all orders with the customer name and the employee who handled the order.
-- (Join Orders, Customers, and Employees)
SELECT
    O.OrderID AS OrderID,
    C.ContactName AS CustomerName,
    E.FirstName + ' ' + E.LastName AS EmployeeName
FROM
    Orders AS O
JOIN
    Customers AS C ON C.CustomerID = O.CustomerID
JOIN
    Employees AS E ON E.EmployeeID = O.EmployeeID
ORDER BY
    OrderID;

-- 2) Get a list of products along with their category and supplier name.
-- (Join Products, Categories, and Suppliers)
SELECT
    P.ProductID,
    P.ProductName,
    C.CategoryName,
    S.CompanyName
FROM
    Products AS P
JOIN
    Categories AS C ON P.CategoryID = C.CategoryID
JOIN
    Suppliers AS S ON P.SupplierID = S.SupplierID;

-- 3) Show all orders and the products included in each order with quantity and unit price.
-- (Join Orders, Order Details, Products)
SELECT
    O.OrderID,
    P.ProductName,
    OD.Quantity,
    OD.UnitPrice
FROM
    Orders AS O
JOIN
    "Order Details" AS OD ON O.OrderID = OD.OrderID
JOIN
    Products AS P ON OD.ProductID = P.ProductID
ORDER BY
    O.OrderID;

-- 4) List employees who report to other employees (manager-subordinate relationship).
-- (Self join on Employees)
SELECT
    e.EmployeeID,
    e.FirstName + ' ' + e.LastName AS EmployeeName,
    mng.FirstName + ' ' + mng.LastName AS ManagerName
FROM
    Employees AS e
JOIN
    Employees AS mng ON e.ReportsTo = mng.EmployeeID;

-- 5) Display each customer and their total order count.
-- (Join Customers and Orders, then GROUP BY)
SELECT
    o.CustomerID,
    MAX(c.ContactName) AS CustomerName,
    COUNT(o.OrderID) AS NumberOfOrders
FROM
    Orders AS o
JOIN
    Customers AS c ON c.CustomerID = o.CustomerID
GROUP BY
    o.CustomerID
ORDER BY
    NumberOfOrders DESC;

-- 6) Find the average unit price of products per category.
-- Use AVG() with GROUP BY
SELECT
    p.CategoryID,
    MAX(c.CategoryName) AS Category,
    AVG(p.UnitPrice) AS AveragePrice
FROM
    Products AS p
JOIN
    Categories AS c ON c.CategoryID = p.CategoryID
GROUP BY
    p.CategoryID
ORDER BY
    AveragePrice;

-- 7) List customers where the contact title starts with 'Owner'.
-- Use LIKE or LEFT(ContactTitle, 5)
SELECT
    *
FROM
    Customers
WHERE
    ContactTitle LIKE 'Owner%';

-- 8) Show the top 5 most expensive products.
-- Use ORDER BY UnitPrice DESC and TOP 5
SELECT TOP (5) *
FROM
    Products
ORDER BY
    UnitPrice DESC;

-- 9) Return the total sales amount (quantity × unit price) per order.
-- Use SUM(OrderDetails.Quantity * OrderDetails.UnitPrice) and GROUP BY
SELECT
    OrderID,
    SUM(Quantity * UnitPrice) AS SalesAmount
FROM
    "Order Details"
GROUP BY
    OrderID
ORDER BY
    SalesAmount DESC;

-- 10) Create a stored procedure that returns all order details for a given Customer ID.
-- Input: @CustomerID
CREATE OR ALTER PROCEDURE proc_GetOrdersByCustId (@CustomerID NVARCHAR(10))
AS
BEGIN
    SELECT
        od.OrderID,
        od.ProductID,
        p.ProductName,
        od.UnitPrice,
        od.Quantity,
        od.Discount
    FROM
        "Order Details" AS od
    JOIN
        Products AS p ON od.ProductID = p.ProductID
    JOIN
        Orders AS o ON od.OrderID = o.OrderID
    WHERE
        o.CustomerID = @CustomerID;
END;
-- Example:
EXEC proc_GetOrdersByCustId 'ALFKI';

-- 11) Write a stored procedure that inserts a new product.
-- Inputs: ProductName, SupplierID, CategoryID, UnitPrice, etc.
CREATE OR ALTER PROCEDURE proc_InsertNewProductFromJson (
    @ProductJson NVARCHAR(MAX)
)
AS
BEGIN
    INSERT INTO Products (
        ProductName,
        SupplierID,
        CategoryID,
        QuantityPerUnit,
        UnitPrice,
        UnitsInStock,
        UnitsOnOrder,
        ReorderLevel,
        Discontinued
    )
    SELECT
        JSON_VALUE(@ProductJson, '$.ProductName'),
        JSON_VALUE(@ProductJson, '$.SupplierID'),
        JSON_VALUE(@ProductJson, '$.CategoryID'),
        JSON_VALUE(@ProductJson, '$.QuantityPerUnit'),
        JSON_VALUE(@ProductJson, '$.UnitPrice'),
        JSON_VALUE(@ProductJson, '$.UnitsInStock'),
        JSON_VALUE(@ProductJson, '$.UnitsOnOrder'),
        JSON_VALUE(@ProductJson, '$.ReorderLevel'),
        JSON_VALUE(@ProductJson, '$.Discontinued');
END;

DECLARE @ProductData NVARCHAR(MAX) =
'{
    "ProductName": "Nike shoes",
    "SupplierID": 2,
    "CategoryID": 3,
    "QuantityPerUnit": 10,
    "UnitPrice": 35.75,
    "UnitsInStock": 75,
    "UnitsOnOrder": 5,
    "ReorderLevel": 20,
    "Discontinued": 0
}';

EXEC proc_InsertNewProductFromJson @ProductJson = @ProductData;

-- 12) Create a stored procedure that returns total sales per employee.
-- Join Orders, Order Details, and Employees
CREATE OR ALTER PROCEDURE GetTotalSalesPerEmployee
AS
BEGIN
    SELECT
        e.EmployeeID,
        MAX(e.FirstName + ' ' + e.LastName) AS EmployeeName,
        MAX(e.Title) AS Title,
        SUM(od.UnitPrice * od.Quantity) AS TotalSales
    FROM
        Orders AS o
    JOIN
        Employees AS e ON e.EmployeeID = o.EmployeeID
    JOIN
        "Order Details" AS od ON o.OrderID = od.OrderID
    GROUP BY
        e.EmployeeID
    ORDER BY
        TotalSales DESC;
END;

EXEC GetTotalSalesPerEmployee;

-- 13) Use a CTE to rank products by unit price within each category.
-- Use ROW_NUMBER() or RANK() with PARTITION BY CategoryID
WITH ProductRanking AS (
    SELECT
        ProductID,
        ProductName,
        CategoryID,
        UnitPrice,
        RANK() OVER(PARTITION BY CategoryID ORDER BY UnitPrice DESC) AS RankNum
		-- ROW_NUMBER() OVER(PARTITION BY CategoryID ORDER BY UnitPrice DESC) AS RowNum
    FROM
        Products
)
SELECT
    ProductID,
    ProductName,
    CategoryID,
    UnitPrice,
    RankNum
FROM
    ProductRanking
ORDER BY
    CategoryID,
    RankNum;

-- 14) Create a CTE to calculate total revenue per product and filter products with revenue > 10,000.
WITH ProductRevenue AS (
    SELECT
        p.ProductID,
        max(p.ProductName) as ProductName,
        SUM(od.UnitPrice * od.Quantity * (1 - od.Discount)) AS TotalRevenue
    FROM
        Products AS p
    JOIN
        "Order Details" AS od ON p.ProductID = od.ProductID
    GROUP BY
        p.ProductID
)
SELECT
    ProductID,
    ProductName,
    TotalRevenue
FROM
    ProductRevenue
WHERE
    TotalRevenue > 10000
ORDER BY
    TotalRevenue DESC;

-- 15) Use a CTE with recursion to display employee hierarchy.
-- Start from top-level employee (ReportsTo IS NULL) and drill down
WITH EmployeeHierarchy AS (
    -- Initial
    SELECT
        EmployeeID,
        FirstName + ' ' + LastName AS EmployeeName,
        Title,
        ReportsTo,
        0 AS HierarchyLevel
    FROM
        Employees
    WHERE
        ReportsTo IS NULL

    UNION ALL

    -- Recursion
    SELECT
        e.EmployeeID,
        e.FirstName + ' ' + e.LastName AS EmployeeName,
        e.Title,
        e.ReportsTo,
        eh.HierarchyLevel + 1 AS HierarchyLevel
    FROM
        Employees AS e
    INNER JOIN
        EmployeeHierarchy AS eh ON e.ReportsTo = eh.EmployeeID
)
SELECT
    EmployeeID,
    EmployeeName,
    Title,
    ReportsTo,
    HierarchyLevel
FROM
    EmployeeHierarchy
ORDER BY
    HierarchyLevel;