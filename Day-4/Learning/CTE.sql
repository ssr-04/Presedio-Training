-- COMMON TABLE EXPRESSION (CTE)

-- Note the CTE and the related query should be executed together
WITH cteAuthors
AS
(SELECT 
	au_id, concat(au_fname, ' ', au_lname) AS AuthorName
FROM authors)

SELECT * FROM cteauthors;

-- Trying Pagination with CTE
DECLARE @pageNo INT = 2, @pageSize INT = 10;
WITH ctePaginatedTitles AS
( SELECT 
	title_id, title, price, ROW_NUMBER() OVER (ORDER BY price DESC) as RowNum
  FROM titles
)

SELECT * FROM ctePaginatedTitles WHERE RowNum between ((@pageNo-1)*@pageSize+1) and (@pageNo*@pageSize)

-- Trying pagination as stored procedure
CREATE OR ALTER PROCEDURE GetPaginatedTitles
(
    @PageNo INT = 1, 
    @PageSize INT = 10
)
AS
BEGIN
	SELECT 
		title_id, title, price
	from titles
	ORDER BY price DESC
	offset ((@PageNo-1)*@PageSize) rows fetch next @PageSize rows only -- Better than using ROW_NUMBER
END

EXEC GetPaginatedTitles 2,5