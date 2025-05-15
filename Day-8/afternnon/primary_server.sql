-- Primary server

-- Creating table

CREATE TABLE rental_log (

    log_id SERIAL PRIMARY KEY,

    rental_time TIMESTAMP,

    customer_id INT,

    film_id INT,

    amount NUMERIC,

    logged_on TIMESTAMP DEFAULT CURRENT_TIMESTAMP

);

select * from rental_log;

-- Creating stored procedure

CREATE OR REPLACE PROCEDURE sp_add_rental_log(

    p_customer_id INT,

    p_film_id INT,

    p_amount NUMERIC

)

LANGUAGE plpgsql

AS $$

BEGIN

    INSERT INTO rental_log (rental_time, customer_id, film_id, amount)

    VALUES (CURRENT_TIMESTAMP, p_customer_id, p_film_id, p_amount);

EXCEPTION WHEN OTHERS THEN

    RAISE NOTICE 'Error occurred: %', SQLERRM;

END;

$$;

-- Calling the procedure on the primary:
 
CALL sp_add_rental_log(1, 100, 4.99);

-- Verifying

Select * from rental_log;

-- Creating table to log changes to the rental_log

create table rental_update_log (
	update_log_id serial primary key,
	rental_log_id int not null,
	old_amount numeric,
	new_amount numeric,
	updated_at timestamp default current_timestamp
);

create or replace function log_rental_update()
returns trigger as $$
begin
	if old.amount is distinct from new.amount then
		insert into rental_update_log (rental_log_id, old_amount, new_amount)
		values (old.log_id, old.amount, new.amount);
	end if;

	return new;
end;
$$ language plpgsql;

create trigger trg_rental_update
after update on rental_log
for each row
execute function log_rental_update();

update rental_log
set amount = 5.99
where log_id = 1;

-- Let's check
select * from rental_update_log;