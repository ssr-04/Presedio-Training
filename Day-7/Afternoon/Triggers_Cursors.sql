-- Triggers
create table audit_logs(
	audit_id serial primary key,
	table_name text,
	fieldname text,
	old_value text,
	new_value text,
	update_date timestamp
);

create or replace function UpdateAuditLogs()
returns trigger
as $$
begin
	insert into audit_logs(table_name, fieldname, old_value, new_value, update_date)
	values('Customer', 'email', OLD.email, NEW.email, current_timestamp);
	return new;
end;
$$ language plpgsql

create or replace trigger trg_log_customer_email_change
after update
on customer
for each row
execute function UpdateAuditLogs();

select * from customer;
update customer set email = 'test@gmail.com' where customer_id=1;
select * from audit_logs;

-- Generic trigger
create or replace function genericupdateauditlogs()
returns trigger
as $$
declare
    audited_fieldname text;
    old_field_value text;
    new_field_value text;
begin
    audited_fieldname := tg_argv[0];

    EXECUTE FORMAT('SELECT ($1).%I::TEXT',
                    AUDITED_FIELDNAME)
    INTO OLD_FIELD_VALUE
    USING OLD;
	EXECUTE FORMAT('SELECT ($1).%I::TEXT',
                    AUDITED_FIELDNAME)
    INTO NEW_FIELD_VALUE
    USING NEW;

    insert into audit_logs(table_name, fieldname, old_value, new_value, update_date)
    values(tg_table_name, audited_fieldname, old_field_value, new_field_value, current_timestamp);

    return new;
end;
$$ language plpgsql;

create or replace trigger trg_log_customer_email_change
before update
on customer
for each row
execute function GenericUpdateAuditLogs('email');

update customer set email = 'test1@gmail.com' where customer_id=1;
select * from audit_logs;

-- Cursors
do $$
declare
    rental_record record;
    rental_cursor cursor for
        select r.rental_id, c.first_name, c.last_name, r.rental_date
        from rental r
        join customer c on r.customer_id = c.customer_id
        order by r.rental_id;
begin
    open rental_cursor;

    loop
        fetch rental_cursor into rental_record;
        exit when not found;

        raise notice 'rental id: %, customer: % %, date: %',
                     rental_record.rental_id,
                     rental_record.first_name,
                     rental_record.last_name,
                     rental_record.rental_date;
    end loop;

    close rental_cursor;
end;
$$;
---------------------------------------------------------------------
create table rental_tax_log(
	rental_id int,
	customer_name text,
	rental_date timestamp,
	amount numeric,
	tax numeric
);

select * from rental_tax_log;

do $$
declare
	rec record;
	cur cursor for
		select r.rental_id,
			c.first_name || ' ' || c.last_name as customer_name,
			r.rental_date,
			p.amount
		from rental r
		join payment p on r.rental_id = p.rental_id
		join customer c on r.customer_id = c.customer_id;
	begin
		open cur;
		loop
			fetch cur into rec;
			exit when not found;

			insert into rental_tax_log values(
rec.rental_id, rec.customer_name, rec.rental_date, rec.amount, rec.amount*0.10
			);
		end loop;
	close cur;
end;
$$;

select * from rental_tax_log;
