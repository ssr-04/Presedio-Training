/*
Locking Mechanism
PostgreSQL automatically applies locks, but you can control them manually when needed.

Types of Locks

MVCC VS Locks
MVCC allows readers and writers to work together without blocking.
Locks are needed when multiple writers try to touch the same row or table.

Simple Rule of Locks
Readers don’t block each other.
Writers block other writers on the same row.


Row-Level Locking (Default Behavior) / Implicit Lock
Two Users updating the same row
-- Trans A
*/
BEGIN;
UPDATE products
SET price = 500
WHERE id = 1;
-- Trans A holds a lock on row id = 1

-- Trans B
BEGIN;
UPDATE products
SET price = 600
WHERE id = 1;

/*
Result:
B waits until A commits or rollbacks
Row Level Locking
*/

-- Table-Level Locks / Explicit Table Lock
1. ACCESS SHARE -- select
-- Allows reads and writes

2. ROW SHARE
-- SELECT ... FOR UPDATE -> lock the selected row for later update

BEGIN;
LOCK TABLE products
IN ACCESS SHARE MODE;
-- Allows other SELECTS, even INSERT/DELETE at the same time.

BEGIN;
LOCK TABLE products
IN ROW SHARE MODE;
-- SELECT .. FOR UPDATE, reads are allowed, conflicting row locks are blocked, writes allowed

3. EXCLUSIVE
-- Blocks writes (INSERT, UPDATE, DELETE) but allows reads (SELECT)

BEGIN;
LOCK TABLE products
IN EXCLUSIVE MODE;

4. ACCESS EXCLUSIVE  -- Most agressive lock 
-- Blocks everything, Used by ALTER TABLE, DROP TABLE, TRUNCATE, 
-- Internally used by DDL.


-- A
BEGIN;
LOCK TABLE products IN ACCESS EXCLUSIVE MODE;
-- Table is now fully locked!

-- B
SLEECT * FROM products;
-- B will wait until A commits or rollbacks.

-- Explicit Row Locks --> SELECT ... FOR UPDATE
-- A
BEGIN;
SELECT * FROM products
WHERE id = 1
FOR UPDATE;
-- Row id = 1 is now locked

-- B
BEGIN;
UPDATE products
SET price = 700
WHERE id = 1;
-- B is blocked until A finishes.

-- SELECT ... FOR UPDATE locks the row early so no one can change it midway.
-- Banking, Ticket Booking, Inventory Management Systems
/*
A deadlock happens when:
Transaction A waits for B
Transaction B waits for A
They both wait forever.

-- Trans A
*/
BEGIN;
UPDATE products
SET price = 500
WHERE id = 1;
-- A locks row 1

-- Trans B
BEGIN;
UPDATE products
SET price = 600
WHERE id = 2;
-- B locks row 2

-- Trans A
UPDATE products
SET price = 500
WHERE id = 2;
-- A locks row 2 (already locked by B)

-- Trans B
UPDATE products
SET price = 600
WHERE id = 1
--B locks row 1 (already locked by A)

/*
psql detects a deadlock
ERROR: deadlock detected
It automatically aborts a transaction to resolve deadlock.
*/

-- Advisory Lock / Custom Locks
-- Get a lock with ID 12345
SELECT pg_advisory_lock(12345);

-- critical ops

-- Releas the lock
SELECT pg_advisory_unlock(12345);

---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------