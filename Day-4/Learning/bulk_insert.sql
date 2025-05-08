-- Bulk insertion from a file path

CREATE TABLE People(
	id INT PRIMARY KEY,
	name VARCHAR(20),
	age INT
);

CREATE OR ALTER PROCEDURE proc_BulkInsert(@filepath NVARCHAR(500))
AS
BEGIN
	DECLARE @InsertQuery NVARCHAR(max)
	
	SET @InsertQuery = 'BULK INSERT People FROM ''' + @filepath + '''
	with(
		FIRSTROW = 2,
		FIELDTERMINATOR = '','',
		ROWTERMINATOR = ''\n''
	)'

	EXEC sp_executesql @InsertQuery
END

EXEC proc_BulkInsert 'C:\Users\VC\Documents\Learning\Internship-training\Presedio-Training\Day-4\Learning\data.csv'

SELECT * FROM People;

-- Let's add exception handling

--First let's clear stuff
DROP PROCEDURE proc_BulkInsert;
TRUNCATE TABLE People;

CREATE TABLE BulkInsertLogs(
	id INT IDENTITY(1,1) PRIMARY KEY,
	filepath nvarchar(500) NOT NULL,
	status nvarchar(50) NOT NULL CONSTRAINT chk_status CHECK(status in ('Success', 'Failed')),
	message nvarchar(max),
	InsertedOn DateTime DEFAULT GetDate()
);

CREATE OR ALTER PROCEDURE proc_BulkInsert(@filepath NVARCHAR(500))
AS
BEGIN
	DECLARE @InsertQuery NVARCHAR(max)
	
	SET @InsertQuery = 'BULK INSERT People FROM ''' + @filepath + '''
	with(
		FIRSTROW = 2,
		FIELDTERMINATOR = '','',
		ROWTERMINATOR = ''\n''
	)'
	BEGIN TRY
		EXEC sp_executesql @InsertQuery;
		INSERT INTO BulkInsertLogs(filepath, status, message)
		VALUES(@filepath, 'Success', 'Bulk Insertion is successfull');
	END TRY
	BEGIN CATCH
		INSERT INTO BulkInsertLogs(filepath, status, message)
		VALUES(@filepath, 'Failed', ERROR_MESSAGE());
	END CATCH
END

EXEC proc_BulkInsert 'C:\Users\invalid_path.csv';

SELECT * FROM BulkInsertLogs;

EXEC proc_BulkInsert 'C:\Users\VC\Documents\Learning\Internship-training\Presedio-Training\Day-4\Learning\data.csv'

SELECT * FROM BulkInsertLogs;
