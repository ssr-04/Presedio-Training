Cursors 
Write a cursor to list all customers and how many rentals each made. Insert these into a summary table.

Using a cursor, print the titles of films in the 'Comedy' category rented more than 10 times.

Create a cursor to go through each store and count the number of distinct films available, and insert results into a report table.

Loop through all customers who haven't rented in the last 6 months and insert their details into an inactive_customers table.
--------------------------------------------------------------------------

Transactions 
Write a transaction that inserts a new customer, adds their rental, and logs the payment – all atomically.

Simulate a transaction where one update fails (e.g., invalid rental ID), and ensure the entire transaction rolls back.

Use SAVEPOINT to update multiple payment amounts. Roll back only one payment update using ROLLBACK TO SAVEPOINT.

Perform a transaction that transfers inventory from one store to another (delete + insert) safely.

Create a transaction that deletes a customer and all associated records (rental, payment), ensuring referential integrity.
----------------------------------------------------------------------------

Triggers
Create a trigger to prevent inserting payments of zero or negative amount.

Set up a trigger that automatically updates last_update on the film table when the title or rental rate is changed.

Write a trigger that inserts a log into rental_log whenever a film is rented more than 3 times in a week.
------------------------------------------------------------------------------