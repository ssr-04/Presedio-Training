----------------------------------------- Cursors --------------------------------------
-- 1) Write a cursor to list all customers and how many rentals each made. 
-- Insert these into a summary table.
create table customer_summary(
	customer_id int primary key,
	name text,
	NoOfRentals int
);

do $$
	declare 
		rec record;
		cur cursor for
			select 
				r.customer_id, 
				max(c.first_name || ' ' || c.last_name) as customer_name,
				count(*) as NoOfRentals
			from rental r
			join customer c on r.customer_id = c.customer_id
			group by r.customer_id
			order by 1;
	begin
		open cur;
		loop
			fetch cur into rec;
			exit when not found;

			insert into customer_summary
			values(rec.customer_id, rec.customer_name, rec.NoOfRentals);
		end loop;
		close cur;
	end;
$$

select * from customer_summary;

----------------------------------------------------------------------------------------
-- 2) Using a cursor, print the titles of films in the 'Comedy' category rented more than 10 times.

do $$
	declare 
		rec record;
		cur cursor for
			select 
				f.film_id, f.title, max(c.name) as Category, count(*) as Rentalcount
			from film f
			join film_category fc on fc.film_id = f.film_id
			join category c on c.category_id = fc.category_id
			join inventory i on i.film_id = f.film_id
			join rental r on r.inventory_id = i.inventory_id
			where c.name = 'Comedy'
			group by f.film_id
			having count(*) > 10
			order by film_id;
	begin
		open cur;
		loop
			fetch cur into rec;
			exit when not found;
				RAISE NOTICE 'ID: %, Name: %, RentalCount: %',rec.film_id, rec.title, rec.RentalCount;
		end loop;
		close cur;
	end;
$$
--------------------------------------------------------------------------------------	
-- 3) Create a cursor to go through each store 
-- and count the number of distinct films available, and insert results into a report table.

create table store_film_report (
    store_id integer primary key,
    distinct_film_count integer
);

do $$
	declare 
		rec record;
		cur cursor for
			select 
				store_id, count(distinct film_id) as distinct_films
			from inventory
			group by store_id
			order by 1;
	begin
		open cur;
		loop
			fetch cur into rec;
			exit when not found;

			insert into store_film_report
			values(rec.store_id, rec.distinct_films);
		end loop;
		close cur;
	end;
$$		

select * from store_film_report;

--------------------------------------------------------------------------------------
-- 4) Loop through all customers who haven't rented in the last 6 months 
-- and insert their details into an inactive_customers table.
create table inactive_customers(
	customer_id int primary key,
	name text,
	last_rental timestamp
);

do $$
	declare 
		rec record;
		cur cursor for
			select 
				c.customer_id,
				max(c.first_name || ' ' || c.last_name) as customer_name,
				max(r.rental_date) as last_rental
			from customer c
			left join rental r on c.customer_id = r.customer_id
			where r.rental_date < now() - interval '6 months' or r.rental_id is null
			group by c.customer_id
			order by 1;
	begin
		open cur;
		loop
			fetch cur into rec;
			exit when not found;

			insert into inactive_customers
			values(rec.customer_id, rec.customer_name, rec.last_rental);
		end loop;
		close cur;
	end;
$$

select * from inactive_customers;

--------------------------------------------------------------------------------------
-- 5) Loop through all films and update the rental rate by +1 for the films where rental count < 5 (using stored procedure)
create or replace procedure proc_update_rental_rate()
language plpgsql
as $$
declare
	rec record;
	cur_film_rental_count cursor for
		select f.film_id, f.rental_rate, count(r.rental_id) as rental_count
		from film f left join inventory i on f.film_id = i.film_id
		left join rental r on i.inventory_id = r.inventory_id
		group by f.film_id, f.rental_rate;

begin
	open cur_film_rental_count;
	loop
		fetch cur_film_rental_count into rec;
		exit when not found;
		if rec.rental_count < 5 then
			update film set rental_rate = rental_rate + 1
			where film_id = rec.film_id;

			raise notice 'updated film with id %. The new rental rate is %',rec.film_id, rec.rental_rate+1;
		end if;
	end loop;
	close cur_film_rental_count;
end;
$$;
call proc_update_rental_rate();
-----------------------------------------------------------------------------------------
--------------------------------------- Transactions --------------------------
-- 1) Write a transaction that inserts a new customer, adds their rental, 
-- and logs the payment – all atomically.
do $$
begin
	insert into customer 
		values(1111, 2, 'Test', 'Test', 'test@gmail.com', 6, true, now(), now(), 1);

	insert into rental
		values(16052, now(), 1525, 1111, now() + interval '6 months', 2, now());

	insert into payment
		values(190002, 1111, 2, 1525, 7.99, now());

	RAISE NOTICE 'Transaction completed successfully.';
exception
	when others then
		RAISE NOTICE 'Transaction failed: %', SQLERRM;
end;
$$;
-- Note: Even though it doesnt have explicit begin and commit/rollback, the entire do block
-- is treated as one transaction in pgsql so all executes or fails together (rolled back)
-------------------------------------------------------------------------------------------
------------ Writing same inside stored procedure -------------------------------------------
create or replace procedure proc_create_customer_rental_payment(
p_first_name text, p_last_name text, p_email text, p_address_id int,
p_inventory_id int, p_store_id int,
p_staff_id int, p_amount numeric
)
language plpgsql
as $$
declare
	v_customer_id int;
	v_rental_id int;
begin
	begin
		INSERT INTO customer (store_id, first_name, last_name, email, address_id, active, create_date)
		values (p_store_id, p_first_name, p_last_name, p_email, p_address_id, 1, current_date)
		returning customer_id into v_customer_id;

		INSERT INTO rental (rental_date, inventory_id, customer_id, staff_id)
		VALUES (CURRENT_TIMESTAMP, p_inventory_id, v_customer_id, p_staff_id)
		RETURNING rental_id INTO v_rental_id;
		
		INSERT INTO payment (customer_id, staff_id, rental_id, amount, payment_date)
		VALUES (v_customer_id, p_staff_id, v_rental_id, p_amount, CURRENT_TIMESTAMP);
	exception
		when others then
			raise notice 'Transaction failed: %', sqlerrm;
	end;
end;
$$;

call proc_create_customer_rental_payment ('Ram','Som','ram_som@gmail.com',1,1,1,1,10);
call proc_create_customer_rental_payment ('Ram','Som','ram_som@gmail.com',1,1,1,1,0);
---------------------------------------------------------------------------------------------
-- 2) Simulate a transaction where one update fails (e.g., invalid rental ID), 
-- and ensure the entire transaction rolls back.
do $$
begin
    -- simulate a successful update
    update rental
    set return_date = now()
    where rental_id = 170; -- this id exists

    -- simulate a failed update
    update rental
    set return_date = 'Nothing' -- invalid
    where rental_id = 180; 

    raise notice 'transaction completed successfully.';
exception
    when others then
        raise notice 'transaction failed: %, all changes rolled back.', sqlerrm;
        -- implicit rollback of the whole block
end;
$$;
-- Let's check if entirely rolled back
select * from rental where rental_id=170; -- It's rolled back

-------------------------------------------------------------------------------------------
-- 3) Use SAVEPOINT to update multiple payment amounts. Roll back only one payment 
-- update using ROLLBACK TO SAVEPOINT.
begin;

	update payment
	set amount = 122
	where payment_id = 17507;
	
	-- set a savepoint before the second update
	savepoint sp1;
	
	-- second update - we will simulate failure and roll it back
	update payment
	set amount = 'invalid'  -- causes an error: invalid input syntax
	where payment_id = 17508;

-- commit;
rollback to savepoint sp1;
select * from payment where payment_id = 17507;
-----------------------------------------------------------------------------------------
-- 4) Perform a transaction that transfers inventory from one store to another 
-- (delete + insert) safely.

select * from inventory where inventory_id = 2;
do $$
begin
    -- -- delete the inventory_id = 2 from store 1
    -- delete from inventory where inventory_id = 2;

    -- -- insert the inventory_id = 2 to store 2
    -- insert into inventory values(2, 1, 2, now());
	
	----- The above way doesn't work because of rental_inventory_id_fkey ----
	update inventory set store_id = 2 where inventory_id = 2;

    raise notice 'transaction completed successfully.';
exception
    when others then
        raise notice 'transaction failed: %, all changes rolled back.', sqlerrm;
        -- implicit rollback of the whole block
end;
$$;
select * from inventory where inventory_id = 2;
-----------------------------------------------------------------------------------
-- 5) Create a transaction that deletes a customer and all associated records 
-- (rental, payment), ensuring referential integrity.

begin;
-- Order of deletion = payment -> rental -> customer
delete from payment
where customer_id = 1;

delete from rental
where customer_id = 1;

delete from customer
where customer_id = 1;

commit;

select * from customer where customer_id = 1;
-------------------------------------------------------------------------------------
--------------------------------------------- Triggers -----------------------------------
-- 1) Create a trigger to prevent inserting payments of zero or negative amount.
create or replace function prevent_invalid_payment()
returns trigger
as $$
begin
	if new.amount <= 0 then
		raise exception 'Payment amount must be greater than zero.';
	end if;
	return new;
end;
$$ language plpgsql;

create trigger check_payment_amount
before insert on payment
for each row
execute function prevent_invalid_payment();

insert into payment (payment_id, customer_id, staff_id, rental_id, amount, payment_date)
values (999999, 5, 1, 1, 0.00, now());

insert into payment (payment_id, customer_id, staff_id, rental_id, amount, payment_date)
values (999998, 10, 1, 1, -5.00, now());

insert into payment (payment_id, customer_id, staff_id, rental_id, amount, payment_date)
values (999999, 11, 1, 1, 5.00, now());
-- It works
----------------------------------------------------------------------------------
-- 2) Set up a trigger that automatically 
-- updates last_update on the film table when the title or rental rate is changed.
create or replace function update_last_update()
returns trigger as $$
begin
	if new.title is distinct from old.title
       or new.rental_rate is distinct from old.rental_rate then
        new.last_update := now();
    else
        new.last_update := old.last_update; -- prevents unnecessary update
    end if;
    return new;
end;
$$ language plpgsql;

create or replace trigger trg_update_last_update
before update on film
for each row
execute function update_last_update();

select * from film where film_id=5; -- should be updated

update film
set title = 'New African Egg'
where film_id = 5;

select * from film where film_id=5;

update film
set rental_rate = 2.99
where film_id = 3;

select * from film where film_id=3; -- same so shouldnt be updated

------------------------------------------------------------------------------------------
-- 3) Write a trigger that inserts a log into rental_log 
-- whenever a film is rented more than 3 times in a week.

create table if not exists rental_log (
    log_id serial primary key,
    film_id integer not null,
    rental_count integer not null,
    log_date timestamp default now()
);


create or replace function log_frequent_rentals()
returns trigger as $$
declare
	c_film_id integer;
	rental_count integer;
begin
	select film_id into c_film_id
	from inventory
	where inventory_id = new.inventory_id;

	select count(*) into rental_count
	from rental r
	join inventory i on r.inventory_id = i.inventory_id
	where i.film_id = c_film_id
	and r.rental_date >= now() - interval '7 days';

	if rental_count > 3 then
		insert into rental_log (film_id, rental_count)
		values (c_film_id, rental_count);
	end if;

	return new;
end;
$$ language plpgsql;

create or replace trigger trg_log_frequent_rental
after insert on rental
for each row
execute function log_frequent_rentals();

select * from rental;
insert into rental(rental_date, inventory_id, customer_id, return_date, staff_id, last_update)
	values (now(), 1525, 10, now() + interval '7 days', 2, now());
insert into rental(rental_date, inventory_id, customer_id, return_date, staff_id, last_update)
	values (now(), 1525, 8, now() + interval '7 days', 2, now());
insert into rental(rental_date, inventory_id, customer_id, return_date, staff_id, last_update)
	values (now(), 1525, 13, now() + interval '7 days', 1, now());
insert into rental(rental_date, inventory_id, customer_id, return_date, staff_id, last_update)
	values (now(), 1525, 19, now() + interval '7 days', 2, now());

SELECT * FROM rental_log ORDER BY log_date DESC;

---------------------------------------------------------------------------------------------
