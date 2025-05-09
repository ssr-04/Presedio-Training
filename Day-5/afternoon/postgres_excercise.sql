-- ------------------------------------------------ Cursor-Based Questions (5) ------------------------------------------------------

-- 1) Write a cursor that loops through all films and prints titles longer than 120 minutes.
do $$
declare
	film_record record;
	film_cursor cursor for
		select
			title, length
		from
			film
		where
			length > 120;
begin
	open film_cursor;
	loop
		fetch film_cursor into film_record;
		exit when not found;
		raise notice 'Film Title: %', film_record.title;
	end loop;
	close film_cursor;
end$$
-- ---------------------------------------------------------------------------------------------------------------------------------
-- 2) Create a cursor that iterates through all customers and counts how many rentals each made.
do $$
declare
	customer_rental_data record;
	custom_rental_cursor cursor for
		select 
			r.customer_id, c.first_name, c.last_name, count(*) as rental_count
		from
			rental r
		join 
			customer c on r.customer_id = c.customer_id
		group by 
			r.customer_id, c.first_name, c.last_name
		order by 
			r.customer_id;
begin
	open custom_rental_cursor;
	loop 
		fetch custom_rental_cursor into customer_rental_data;
		exit when not found;
		raise notice 'CustomerID: % - Name: % % - Rentals: %',
			customer_rental_data.customer_id,
			customer_rental_data.first_name,
			customer_rental_data.last_name,
			customer_rental_data.rental_count;
	end loop;
	close custom_rental_cursor;
end$$
-- ----------------------------------------------------------------------------------------------------------------------------------
-- 3) using a cursor, update rental rates: Increase rental rate by $1 for films with less than 5 rentals.
do $$
declare
    film_record record;
    rental_count integer;
    film_cursor cursor for
        select film_id, title, rental_rate from film;
begin
    open film_cursor;
    loop
        fetch film_cursor into film_record;
        exit when not found;
		
        select count(r.rental_id)
        into rental_count
        from rental r
        join inventory i on r.inventory_id = i.inventory_id
        where i.film_id = film_record.film_id;
		
        if rental_count < 5 then
            update film
            set rental_rate = film_record.rental_rate + 1
            where film_id = film_record.film_id;
			
            raise notice 'updated rental rate for film "%" (id: %) from % to %',
                           film_record.title,
                           film_record.film_id,
                           film_record.rental_rate,
                           film_record.rental_rate + 1;
        end if;
    end loop;
    close film_cursor;
	commit;
end $$;
-- ---------------------------------------------------------------------------------------------------------------------------------
-- 4) Create a function using a cursor that collects titles of all films from a particular category.
create or replace function GetFlimsByCategory(p_category_name varchar)
returns setof text --resultant set
language plpgsql
as $$
declare
	film_title text;
	film_cursor cursor(category_name varchar) for
		select f.title
		from film f
		join film_category fc on f.film_id = fc.film_id
		join category c on fc.category_id = c.category_id
		where c.name = category_name;
begin
	open film_cursor(p_category_name);

	loop
		fetch film_cursor into film_title;
		exit when not found;

		return next film_title; --appends to result set
	end loop;
	close film_cursor;

	return; --returns the result set
end;
$$;

select GetFlimsByCategory('Action');
-- ---------------------------------------------------------------------------------------------------------------------------------
-- 5) Loop through all stores and count how many distinct films are available in each store using a cursor.
do $$
declare
	store_record record;
	store_film_cursor cursor for
		select
			s.store_id,
			a.address,
			count(distinct i.film_id) as film_count
		from store s
		join address a on s.address_id = a.address_id
		join inventory i on s.store_id = i.store_id
		group by s.store_id, a.address
		order by s.store_id;
begin
	 open store_film_cursor;
	 loop
		 fetch store_film_cursor into store_record;
		 exit when not found;
		 raise notice 'Store ID: %, Address: %, Distinct Films: %',
            store_record.store_id,
            store_record.address,
            store_record.film_count;
	end loop;
	close store_film_cursor;
end $$;
 
-- ------------------------------------------- Trigger-Based Questions (5) --------------------------------------------------------

-- 1) Write a trigger that logs whenever a new customer is inserted.
create table customer_log (
    log_id serial primary key,
    event_type varchar(10) not null,
    event_timestamp timestamp default now(),
    customer_id integer not null,
    customer_first_name varchar(45),
    customer_last_name varchar(45)
);

create or replace function log_new_customer()
returns trigger
language plpgsql
as $$
begin
    insert into customer_log (event_type, customer_id, customer_first_name, customer_last_name)
    values ('insert', new.customer_id, new.first_name, new.last_name);
    return new;
end;
$$;

create trigger log_customer_insert
after insert on customer
for each row
execute function log_new_customer();

insert into customer values(901, 2, 'Harry', 'Potter', 'spell@with.com', 530, true, now(),now(), 1);

 select * from customer_log;
-- ---------------------------------------------------------------------------------------------------------------------------------
-- 2) Create a trigger that prevents inserting a payment of amount 0.
 create or replace function check_payment_amount()
returns trigger
language plpgsql
as $$
begin
    if new.amount = 0 then
        raise exception 'payment amount cannot be zero.';
    end if;
    return new;
end;
$$;

create trigger prevent_zero_payment
before insert on payment
for each row
execute function check_payment_amount();

insert into payment values(190002, 1, 1, 3000, 0, now());

select * from payment where payment_id=190002;
-- ---------------------------------------------------------------------------------------------------------------------------------
-- 3) Set up a trigger to automatically set last_update on the film table before update.

create or replace function set_film_last_update()
returns trigger
language plpgsql
as $$
begin
    new.last_update = now();
    return new;
end;
$$;

create trigger set_film_update_timestamp
before update on film
for each row
execute function set_film_last_update();

update film
set rental_duration = rental_duration + 1
where film_id = 1;

select last_update from film where film_id=1;
-- ---------------------------------------------------------------------------------------------------------------------------------
-- 4) Create a trigger to log changes in the inventory table (insert/delete).
 create table inventory_log (
    log_id serial primary key,
    event_type varchar(10) not null, -- 'insert' or 'delete'
    event_timestamp timestamp default now(),
    inventory_id integer,
    film_id integer,
    store_id integer
);

create or replace function log_inventory_changes()
returns trigger
language plpgsql
as $$
begin
    IF (TG_OP = 'INSERT') THEN
        insert into inventory_log (event_type, inventory_id, film_id, store_id)
        values ('insert', new.inventory_id, new.film_id, new.store_id);
		return new;
    ELSIF (TG_OP = 'DELETE') THEN
        insert into inventory_log (event_type, inventory_id, film_id, store_id)
        values ('delete', old.inventory_id, old.film_id, old.store_id);
        return old;
    end if;
	return null;
end;
$$;


create or replace trigger log_inventory_insert
after insert on inventory
for each row
execute function log_inventory_changes();

create or replace trigger log_inventory_delete
after delete on inventory
for each row
execute function log_inventory_changes();

insert into inventory values (90014, 1, 1, now());
select * from inventory where inventory_id=90014;
select * from inventory_log;
-- ---------------------------------------------------------------------------------------------------------------------------------
-- 5) Write a trigger that ensures a rental can’t be made for a customer who owes more than $50.
 create or replace function check_customer_rental_limit()
returns trigger
language plpgsql
as $$
declare
    total_unreturned_rental_rate decimal(5,2);
begin
    select coalesce(sum(f.rental_rate), 0)
    into total_unreturned_rental_rate
    from rental r
    join inventory i on r.inventory_id = i.inventory_id
    join film f on i.film_id = f.film_id
    where r.customer_id = new.customer_id
      and r.return_date is null; 
    if total_unreturned_rental_rate > 50.00 then
        raise exception 'customer % currently has unreturned rentals exceeding the $50 limit (total unreturned rental rate: $%). cannot make a new rental.',
                        new.customer_id, total_unreturned_rental_rate;
    end if;
    return new;
end;
$$;

----- There are no customers with rental limit over 50, max is cust_id=60 with 11.98
-- ---------------------------------------------------------------------------------------------------------------------------------
-- ------------------------------------------- Transaction-Based Questions (5) ---------------------------------------------

-- 1) Write a transaction that inserts a customer and an initial rental in one atomic operation.
begin;
do $$
declare
    new_customer_id integer;
    rental_inventory_id integer := (select inventory_id from inventory limit 1);
    rental_staff_id smallint := (select staff_id from staff limit 1);
    new_customer_address_id integer := 1; 
    new_customer_store_id smallint := 1; 
begin
    insert into customer (store_id, first_name, last_name, email, address_id, activebool, create_date, last_update, active)
    values (
        new_customer_store_id,
        'john',
        'doe',
		'test@gmail.com',
        new_customer_address_id,
        true,
        now(),
        now(),
        1
    )
    returning customer_id into new_customer_id;

    raise notice 'inserted new customer with id: %', new_customer_id;
	
    insert into rental (rental_date, inventory_id, customer_id, return_date, staff_id, last_update)
    values (
        now(),
        rental_inventory_id,
        new_customer_id,
		null,
        rental_staff_id,
        now()
    );

    raise notice 'inserted initial rental for customer id: %', new_customer_id;

end $$;

commit;

select * from customer where first_name = 'john' and last_name = 'doe';
select * from rental where customer_id = (select customer_id from customer where first_name = 'john' and last_name = 'doe' order by create_date desc limit 1);
-- ---------------------------------------------------------------------------------------------------------------------------------
-- 2) Simulate a failure in a multi-step transaction (update film + insert into inventory) and roll back.
begin;
do $$
declare
    demo_film_id integer := (select film_id from film limit 1); 
    demo_inventory_film_id integer := (select film_id from film offset 1 limit 1); 
    demo_store_id smallint := (select store_id from store limit 1);
    invalid_film_id integer := 99999;
begin

    raise notice '--- starting transaction to simulate failure & rollback ---';

    -- 1: perform a valid update
    raise notice 'attempting to update rental_duration for film id: %', demo_film_id;
    update film
    set rental_duration = rental_duration + 5
    where film_id = demo_film_id;
    -- not yet saved.
    raise notice 'successfully updated rental_duration for film id: %', demo_film_id;


    -- 2: perform a valid insert into inventory
    raise notice 'attempting to insert a valid inventory item for film id: % at store id: %', demo_inventory_film_id, demo_store_id;
    insert into inventory (film_id, store_id)
    values (demo_inventory_film_id, demo_store_id);
    -- not yet saved too.
    raise notice 'successfully inserted a valid inventory item.';


    -- 3: invalid insert into inventory
    -- will fail
    raise notice 'attempting to insert an invalid inventory item with film id: %', invalid_film_id;
    insert into inventory (film_id, store_id)
    values (invalid_film_id, demo_store_id);

exception
    when foreign_key_violation then
        raise notice 'caught a foreign key violation. rolling back the transaction.';
    when others then
        raise notice 'caught another type of error: %', sqlerrm;
end $$;

-- explicitly rollback
rollback;

-- it should be its original value before the transaction started.
select film_id, rental_duration from film where film_id = demo_film_id;
select inventory_id, film_id, store_id from inventory
where film_id = demo_inventory_film_id and store_id = demo_store_id
order by inventory_id desc limit 5; 
-- ---------------------------------------------------------------------------------------------------------------------------------
-- 3) Create a transaction that transfers an inventory item from one store to another.
select * from inventory;
BEGIN; -- Starting transaction
DO $$
DECLARE
    inventory_item_id_to_transfer INTEGER := 1; 
    destination_store_id_param SMALLINT := 2;
BEGIN

    RAISE NOTICE '--- Attempting to transfer inventory item ID % to Store % ---',
                   inventory_item_id_to_transfer, destination_store_id_param;

    -- Updating the store_id of the inventory item
    UPDATE inventory
    SET store_id = destination_store_id_param
    WHERE inventory_id = inventory_item_id_to_transfer;
    
    RAISE NOTICE 'Successfully updated inventory item ID % to Store %.', inventory_item_id_to_transfer, destination_store_id_param;

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'An error occurred during the update transaction: %', SQLERRM;

END $$;
COMMIT;

SELECT inventory_id, film_id, store_id FROM inventory WHERE inventory_id = 1; 
-- ---------------------------------------------------------------------------------------------------------------------------------
-- 5) Demonstrate SAVEPOINT and ROLLBACK TO SAVEPOINT by updating payment amounts, then undoing one.

-- Check initial values
SELECT payment_id, amount FROM payment WHERE payment_id IN (17503, 17504, 17505, 17506);

-- Start transaction
BEGIN;

-- Step 1: Update first payment
UPDATE payment
SET amount = amount + 1.00
WHERE payment_id = 17503;

-- Step 2: Update second payment
UPDATE payment
SET amount = amount + 2.00
WHERE payment_id = 17504;

-- Step 3: Set savepoint
SAVEPOINT sp_before_undo;

-- Step 4: Update third payment (to be undone)
UPDATE payment
SET amount = amount - 100.00
WHERE payment_id = 17505;

-- Step 5: Rollback to savepoint (undo previous update)
ROLLBACK TO SAVEPOINT sp_before_undo;

-- Step 6: Update fourth payment (this one stays)
UPDATE payment
SET amount = amount + 4.00
WHERE payment_id = 17506;

-- Commit transaction
COMMIT;

-- Final check
SELECT payment_id, amount FROM payment WHERE payment_id IN (17503, 17504, 17505, 17506);
-- ---------------------------------------------------------------------------------------------------------------------------------
-- 5) Write a transaction that deletes a customer and all associated rentals and payments, ensuring atomicity.
begin;
do $$
declare
    target_customer_id integer := 5;
begin
    -- delete payments associated with the customer's rentals
    delete from payment
    where rental_id in (
        select rental_id from rental where customer_id = target_customer_id
    );

    -- delete rentals made by the customer
    delete from rental
    where customer_id = target_customer_id;

    -- finally, delete the customer
    delete from customer
    where customer_id = target_customer_id;
end $$;

commit;
select * from customer where customer_id = 5;
-- ---------------------------------------------------------------------------------------------------------------------------------