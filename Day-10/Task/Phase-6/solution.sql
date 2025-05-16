------------------------------- Security & Roles ---------------------------------------------------
-- Phase 6: Security & Roles

/* Note:
    To check the usage of the roles we need to create users in the CLI and assign them the roles,
    For simplicity sake, only the queries for roles creation with the specifications were
    given below
*/

-- 1. Create a `readonly_user` role:
--    * Can run `SELECT` on `students`, `courses`, and `certificates`
--    * Cannot `INSERT`, `UPDATE`, or `DELETE`


create role readonly_user;

grant select on table students to readonly_user;
grant select on table courses to readonly_user;
grant select on table certificates to readonly_user;

grant usage on schema public to readonly_user;
-- Allows the role to see and access objects within the schema

----------------------------------------------------------------------------------------------
-- 2. Create a `data_entry_user` role:
--    * Can `INSERT` into `students`, `enrollments`
--    * Cannot modify certificates directly

create role data_entry_user;

grant insert on table students to data_entry_user;
grant insert on table enrollments to data_entry_user;

grant usage on sequence students_student_id_seq to data_entry_user;
grant usage on sequence enrollments_enrollment_id_seq to data_entry_user;
-- as the tables have serial type of it is necessary to give sequence privillege

grant usage on schema public to data_entry_user;
-- Allows the role to see and access objects within the schema
-----------------------------------------------------------------------------------------------