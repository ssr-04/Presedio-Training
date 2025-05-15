-- Enabling the the pgcrypto extension (Only once per database)
create extension if not exists pgcrypto;

------------------------------------------------------------------------------------

/*
Procedures are better suited for executing sequences of SQL statements, 
performing transaction control, and operations that have side effects 
that don't necessarily map to returning a result set 
(like complex batch processing or using RAISE NOTICE for logging).
*/

------------------------------------------------------------------------------------
-- Function to encrypt text (BEST AS FUNCTION as can we invoke in queries)
create or replace function fn_encrypt_text(
	plain_text TEXT,
	encryption_key TEXT
)
returns BYTEA -- encrpted binary data (return type of pgp_sym_encrypt)
as $$
begin
	return pgp_sym_encrypt(plain_text, encryption_key);
end;
$$ language plpgsql;

------------------------------------------------------------------------------------
-- Function to compare two encrypted texts (BEST AS FUNCTION as it retuns just boolean)
create or replace function fn_compare_encrypted(
	encrypted_text1 BYTEA,
	encrypted_text2 BYTEA,
	encryption_key TEXT
)
returns BOOLEAN
as $$
declare
	decrypted_text1 TEXT;
	decrypted_text2 TEXT;
begin
	-- Decryption can fail if invalid key or corrupt data so better to have error handling
	begin
		decrypted_text1 := pgp_sym_decrypt(encrypted_text1, encryption_key);
	exception when others then
		decrypted_text1 := null;
	end;

	begin
		decrypted_text2 := pgp_sym_decrypt(encrypted_text2, encryption_key);
	exception when others then
		decrypted_text2 := null;
	end;

	-- Return comparison of both decrypted values
	return decrypted_text1 is not null AND decrypted_text2 is not null AND
		decrypted_text1 = decrypted_text2;
end;
$$ language plpgsql;
------------------------------------------------------------------------------------
-- Testing both functions
-- True case
select 
	fn_compare_encrypted(fn_encrypt_text('password', 'some_secret'),
	fn_encrypt_text('password', 'some_secret'), 'some_secret');
-- False case
select 
	fn_compare_encrypted(fn_encrypt_text('password', 'some_secret'),
	fn_encrypt_text('password1', 'some_secret'), 'some_secret');
-- edge case
select 
	fn_compare_encrypted(fn_encrypt_text('password', 'some_secret'),
	fn_encrypt_text('password', 'some_secret'), 'some_secret1');
------------------------------------------------------------------------------------

-- Function to partially mask text (BEST AS FUNCTION as we can invoke in query)
create or replace function fn_mask_text(
	input_text TEXT
)
returns TEXT
as $$
declare
	text_length INT;
begin
	text_length := length(input_text); -- In-built to get the length of string	

	IF text_length <= 4 then
		return input_text; --first and last two makes 4 so return the input back
	else
		return substring(input_text,1,2) || repeat('*', text_length-4) || substring(input_text, text_length-1, 2);
		-- Note: Indexing starts with 1 and substring takes (input, start, no_of_chars)
	end if;
end;
$$ language plpgsql;

-- Testing masking function
select fn_mask_text('Ram'); -- 4 chars so returns all
select fn_mask_text('hello'); -- 5 chars
select fn_mask_text('test1@gmail.com'); -- general case with > 4 chars
select fn_mask_text(''); -- edge case (might be raise error? not needed here)

------------------------------------------------------------------------------------
-- Creating tables
create table address(
	address_id int primary key,
	address_line text,
	city text,
	state text,
	country text
);
insert into address values(1, '123, a street', 'city a', 'state a', 'country a');
insert into address values(2, '456, b street', 'city b', 'state b', 'country b');
insert into address values(3, '789, c street', 'city c', 'state c', 'country c');
insert into address values(4, '12, d street', 'city d', 'state d', 'country d');
insert into address values(5, '89, e street', 'city e', 'state e', 'country e');

create table store(
	store_id int primary key,
	contact_person text,
	email text,
	phone text,
	address_id int references address(address_id)
);
insert into store values(1, 'owner 1', 'store1@gmail.com', '9876543210', 4);
insert into store values(2, 'owner 2', 'store2@gmail.com', '9876543210', 5);

create table customer(
	customer_id SERIAL primary key,
	name TEXT,
	email BYTEA,
	address_id int references address(address_id),
	store_id int references store(store_id)
);

------------------------------------------------------------------------------------
-- Stored procedure to insert masked customer (BEST AS PROCEDURE as it involves transaction)
drop procedure sp_insert_masked;
create or replace procedure sp_insert_masked(
	in p_name TEXT,
	in p_email TEXT,
	in p_address_id INT,
	in p_store_id INT,
	in encryption_key TEXT
	
)
as $$
declare
	masked_name TEXT;
	encrypted_email BYTEA;
begin
	-- Processing the inputs (Masking & encryption)
	select fn_mask_text(p_name) into masked_name;
	select fn_encrypt_text(p_email, encryption_key) into encrypted_email;

	-- For possible error handling
	begin
		insert into customer(name, email, address_id, store_id)
		values(masked_name, encrypted_email, p_address_id, p_store_id);
	exception when others then
		raise notice 'Something went wrong: %', sqlerrm;
	end;
end;
$$ language plpgsql;

-- Testing the procedure
call sp_insert_masked('Harry Potter', 'harrypotter@wizards.com', 1, 1, 'some_secret');
call sp_insert_masked('Hermione Granger', 'hermione@wizards.com', 2, 2, 'some_secret');
call sp_insert_masked('Albus Dumbledore', 'dumbledore@dean.com', 3, 1, 'some_secret');

-- Testing
select * from customer;
------------------------------------------------------------------------------------
-- Procedure to read masked customers with decrypted emails (BEST as Procedure as it involves query)
create or replace procedure sp_read_masked_customers(
	in encryption_key TEXT
)
as $$
declare
	customer_record record;
	decrypted_email TEXT;
	cur cursor for 
		select * from customer;
begin
	open cur;
	loop
		fetch cur into customer_record;
		exit when not found;
		-- Exception handling
		begin
			SELECT pgp_sym_decrypt(customer_record.email, encryption_key) INTO decrypted_email;
		exception
            when others then
                decrypted_email := 'decryption error';
		end;
		
		raise notice 'Customer ID: %, Masked name: %, Decrypted email: %', 
		customer_record.customer_id, customer_record.name, decrypted_email;
	end loop;
	close cur;
end;
$$ language plpgsql;

call sp_read_masked_customers('some_secret');

--------------------------------------------------------------------------