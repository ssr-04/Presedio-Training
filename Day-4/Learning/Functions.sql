-- Functions
-- (Need to return a value)

-- Scalar valued function
CREATE FUNCTION fn_CalculateTax(@basePrice FLOAT, @taxPercentage FLOAT)
RETURNS FLOAT
AS
BEGIN
	RETURN (@basePrice + (@basePrice * @taxPercentage/100))
END

SELECT dbo.fn_CalculateTax(100, 10);

SELECT title, price, dbo.fn_CalculateTax(price, 12) FROM titles;

-- Table valued functions
CREATE FUNCTION fn_filterMaxPrice(@maxprice FLOAT)
RETURNS TABLE
AS
RETURN
(
    SELECT title, price
    FROM titles
    WHERE price < @maxprice
);


SELECT * FROM dbo.fn_filterMaxPrice(20);
