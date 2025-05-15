-- Secondary server

-- After creating table
select * from rental_log; -- It worked

-- After insering a record using stored procedure
select * from rental_log; -- It worked

-- After updating rental_log in primary, checking the update log in secondary
select * from rental_update_log; -- Trigger insertions are updated in secondary too