-- https://neon.tech/postgresql/postgresql-getting-started/postgresql-sample-database
-- -----------------------------------------------------------------
-- SELECT Queries

-- List all films with their length and rental rate, sorted by length descending.
-- Columns: title, length, rental_rate
SELECT
    title,
    length,
    rental_rate
FROM
    public.film
ORDER BY
    length DESC;

-- Find the top 5 customers who have rented the most films.
-- Hint: Use the rental and customer tables.
SELECT
    r.customer_id,
    MAX(CONCAT(c.first_name, ' ', c.last_name)) AS CustomerName,
    COUNT(*) AS RentalCount
FROM
    public.rental r
JOIN
    public.customer c ON c.customer_id = r.customer_id
GROUP BY
    r.customer_id
ORDER BY
    RentalCount DESC
LIMIT 5;

-- Display all films that have never been rented.
-- Hint: Use LEFT JOIN between film and inventory → rental.
SELECT
    f.film_id,
    f.title,
    f.description,
    f.release_year,
    f.rental_rate,
    f.replacement_cost,
    f.rating,
    f.special_features
FROM
    public.film f
LEFT JOIN
    public.inventory i ON f.film_id = i.film_id
LEFT JOIN
    public.rental r ON i.inventory_id = r.inventory_id
WHERE
    r.rental_id IS NULL;

-- JOIN Queries

-- List all actors who appeared in the film ‘Academy Dinosaur’.
-- Tables: film, film_actor, actor
SELECT
    f.title,
    CONCAT(a.first_name, ' ', a.last_name) AS Actors
FROM
    public.film f
JOIN
    public.film_actor fa ON f.film_id = fa.film_id
JOIN
    public.actor a ON fa.actor_id = a.actor_id
WHERE
    f.title = 'Academy Dinosaur'
ORDER BY
    Actors;

-- List each customer along with the total number of rentals they made and the total amount paid.
-- Tables: customer, rental, payment
SELECT
    r.customer_id,
    COUNT(*) AS RentalCount,
    SUM(p.amount) AS AmountPaid
FROM
    public.rental r
JOIN
    public.customer c ON r.customer_id = c.customer_id
JOIN
    public.payment p ON p.rental_id = r.rental_id
GROUP BY
    r.customer_id
ORDER BY
    r.customer_id;

-- CTE-Based Queries

-- Using a CTE, show the top 3 rented movies by number of rentals.
-- Columns: title, rental_count
WITH cte_Top3Rentals AS (
    SELECT
        f.title,
        COUNT(*) AS rental_count
    FROM
        public.rental r
    JOIN
        public.inventory i ON i.inventory_id = r.inventory_id
    JOIN
        public.film f ON f.film_id = i.film_id
    GROUP BY
        f.title
    ORDER BY
        rental_count DESC
    LIMIT 3
)
SELECT * FROM cte_Top3Rentals;

-- Find customers who have rented more than the average number of films.
-- Use a CTE to compute the average rentals per customer, then filter.
WITH CustomerRentalCount AS (
    SELECT
        customer_id,
        COUNT(*) AS rental_count
    FROM
        public.rental
    GROUP BY
        customer_id
),
AvgRentalPerCustomer AS (
    SELECT
        AVG(rental_count) AS AvgRental
    FROM
        CustomerRentalCount
)
SELECT
    crc.customer_id,
    crc.rental_count
FROM
    CustomerRentalCount crc, AvgRentalPerCustomer avg_crc
WHERE
    crc.rental_count > avg_crc.AvgRental
ORDER BY
	crc.customer_id;

-- Function Questions
-- Write a function that returns the total number of rentals for a given customer ID.
-- Function: get_total_rentals(customer_id INT)
CREATE OR REPLACE FUNCTION get_total_rentals(p_customer_id INT)
RETURNS INTEGER
AS $$
DECLARE
    total_rentals INTEGER;
BEGIN
    SELECT COUNT(*)
    INTO total_rentals
    FROM public.rental
    WHERE customer_id = p_customer_id;
    RETURN total_rentals;
END;
$$ LANGUAGE plpgsql;

SELECT get_total_rentals(1) as NoOfRentals;

-- Stored Procedure Questions
-- Write a stored procedure that updates the rental rate of a film by film ID and new rate.
-- Procedure: update_rental_rate(film_id INT, new_rate NUMERIC)
CREATE OR REPLACE PROCEDURE update_rental_rate(
    p_film_id INT,
    p_new_rate NUMERIC
)
AS $$
BEGIN
    UPDATE public.film
    SET rental_rate = p_new_rate
    WHERE film_id = p_film_id;

END;
$$ LANGUAGE plpgsql;
CALL update_rental_rate(133, 6.99); 

SELECT * FROM film WHERE film_id = 133;

-- Stored Procedure Questions
-- Write a procedure to list overdue rentals (return date is NULL and rental date older than 7 days).
-- Procedure: get_overdue_rentals() that selects relevant columns.
CREATE OR REPLACE PROCEDURE get_overdue_rentals()
AS $$
BEGIN
    SELECT
        rental_id,
        rental_date,
        inventory_id,
        customer_id,
        return_date
    FROM
        rental
    WHERE
        return_date IS NULL
        AND rental_date < (NOW() - INTERVAL '7 days');
END;
$$ LANGUAGE plpgsql;

CALL get_overdue_rentals();

-- Procedure causes error in postgres so sticking back to functions
drop function get_overdue_rentals
CREATE OR REPLACE FUNCTION get_overdue_rentals()
RETURNS TABLE (
    rental_id INT,
    rental_date TIMESTAMP WITHOUT TIME ZONE,
    inventory_id INT,
    customer_id SMALLINT, -- Corrected type based on the error
    return_date TIMESTAMP WITHOUT TIME ZONE
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        r.rental_id,
        r.rental_date,
        r.inventory_id,
        r.customer_id,
        r.return_date
    FROM
        rental r
    WHERE
        r.return_date IS NULL
        AND r.rental_date < (NOW() - INTERVAL '7 days');
END;
$$;
SELECT * FROM get_overdue_rentals();