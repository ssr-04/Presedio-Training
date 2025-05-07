-- Stored procedures
CREATE PROCEDURE proc_FirstProcedure
AS
BEGIN
	PRINT 'Hello World!'
END
GO
EXEC proc_FirstProcedure

CREATE TABLE Products(
	Id INT IDENTITY(1,1) CONSTRAINT Pk_productId PRIMARY KEY,
	name NVARCHAR(100) NOT NULL,
	details NVARCHAR(max)
);

CREATE PROCEDURE proc_InsertProducts(@pName NVARCHAR(100), @pDetails NVARCHAR(max))
AS
BEGIN
	INSERT INTO Products(name,details) VALUES(@pName, @pDetails);
END;

-- Execute the stored procedure
EXEC proc_InsertProducts @pName = 'Laptop', @pDetails = '{"brand":"Dell","spec":{"ram":"16GB","cpu":"i5"}}';

SELECT * FROM Products;

-- Something with JSON
SELECT JSON_QUERY(details, '$.spec') as Specification from Products;

-- Let's update the RAM using stored procedure
CREATE PROCEDURE proc_UpdateProductRam(@pId int,@newValue varchar(20))
AS
BEGIN
   update Products set details = JSON_MODIFY(details, '$.spec.ram',@newValue) where Id = @pId
END

EXEC proc_UpdateProductRam 1, '24GB'
SELECT * FROM Products;

SELECT 
	Id, name, JSON_VALUE(details, '$.brand') AS Brand_name
FROM Products;

-- retrieve based on the json data
EXECUTE proc_InsertProducts 'Laptop','{"brand":"HP","spec":{"ram":"16GB","cpu":"i7"}}'

SELECT * FROM Products WHERE 
  try_cast(json_value(details,'$.spec.cpu') as NVARCHAR(20)) ='i7'

-- Bulk Insertion from JSON
DECLARE @jsondata NVARCHAR(max) = '
[
  {
    "userId": 1,
    "id": 1,
    "title": "sunt aut facere repellat provident occaecati excepturi optio reprehenderit",
    "body": "quia et suscipit\nsuscipit recusandae consequuntur expedita et cum\nreprehenderit molestiae ut ut quas totam\nnostrum rerum est autem sunt rem eveniet architecto"
  },
  {
    "userId": 1,
    "id": 2,
    "title": "qui est esse",
    "body": "est rerum tempore vitae\nsequi sint nihil reprehenderit dolor beatae ea dolores neque\nfugiat blanditiis voluptate porro vel nihil molestiae ut reiciendis\nqui aperiam non debitis possimus qui neque nisi nulla"
  }
]'

CREATE TABLE Posts (
	Id INT PRIMARY KEY,
	userId INT,
	Title NVARCHAR(100),
	Body NVARCHAR(max)
);

CREATE PROCEDURE proc_BulkInsertPosts(@jsondata NVARCHAR(max))
AS
BEGIN
	INSERT INTO Posts(userId, Id, Title, Body)
	SELECT userId, id, title, body from openjson(@jsondata)
	WITH (userId INT, id INT, title varchar(100), body varchar(100))
END

EXECUTE proc_BulkInsertPosts 
'
[
  {
    "userId": 1,
    "id": 1,
    "title": "sunt aut facere repellat provident occaecati excepturi optio reprehenderit",
    "body": "quia et suscipit\nsuscipit recusandae consequuntur expedita et cum\nreprehenderit molestiae ut ut quas totam\nnostrum rerum est autem sunt rem eveniet architecto"
  },
  {
    "userId": 1,
    "id": 2,
    "title": "qui est esse",
    "body": "est rerum tempore vitae\nsequi sint nihil reprehenderit dolor beatae ea dolores neque\nfugiat blanditiis voluptate porro vel nihil molestiae ut reiciendis\nqui aperiam non debitis possimus qui neque nisi nulla"
  }
]';

SELECT * FROM Posts;

-- Create a procedure that brings post by taking user_id as parameter
CREATE PROCEDURE proc_GetPostsByUserId (@pUserId INT)
AS
BEGIN
    SELECT *
    FROM Posts
    WHERE userId = @pUserId;
END;
GO

EXEC proc_GetPostsByUserId 1;

-- Checking by passing variable as param
DECLARE @user_id INT = 1;
EXEC proc_GetPostsByUserId @user_id;