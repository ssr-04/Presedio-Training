/* Phase-1 Solution:
    -- Database: edtech_platform

-- Table: students
-- Stores information about students.
    student_id PRIMARY KEY, -- Unique for each student
    first_name NOT NULL,
    last_name NOT NULL,
    email NOT NULL UNIQUE, -- must be unique
    registration_date NOT NULL DEFAULT CURRENT_DATE,
    phone_number,


-- Table: trainers
-- Stores information about trainers.
    trainer_id PRIMARY KEY, -- Unique for each trainer
    first_name NOT NULL,
    last_name NOT NULL,
    email NOT NULL UNIQUE, -- Email must be unique
    hire_date DATE,
    bio/expertise TEXT

-- Table category
-- Stores categories of the courses offered
    category_id PRIMARY KEY, -- Unique for each course
    category_name not null,

-- Table: courses
-- Stores information about the courses offered.
    course_id PRIMARY KEY, -- Unique for each course
    course_name NOT NULL UNIQUE, -- Course names maybe be unique?
    category_id NOT NULL,
    description TEXT,
    duration_days INT CHECK (duration_days > 0), -- Duration in days
    price DECIMAL(10, 2) CHECK (price >= 0), -- Course price
    trainer_id INT NOT NULL, -- Foreign key linking to the trainer

    CONSTRAINT fk_course_trainer
        FOREIGN KEY (trainer_id)
        REFERENCES trainers (trainer_id)

    CONSTRAINT fk_course_category
        FOREIGN KEY (category_id)
        REFERENCES trainers (category_id)


-- Table: enrollments
-- Links students to courses they are enrolled in. (Many-to-Many relationship).
    enrollment_id PRIMARY KEY, -- Surrogate key
    student_id NOT NULL, -- Foreign key linking to student
    course_id NOT NULL, -- Foreign key linking to course
    enrollment_date NOT NULL DEFAULT CURRENT_DATE,
    completion_status NOT NULL DEFAULT 'In Progress', -- e.g., 'In Progress', 'Completed', 'Dropped', 'Failed'
    completion_date, -- Date when the course was completed
    final_grade DECIMAL(3, 2), -- Final grade

    -- student can only enroll in the same course once
    UNIQUE (student_id, course_id),

    CONSTRAINT fk_enrollment_student
        FOREIGN KEY (student_id)
        REFERENCES students (student_id)

    CONSTRAINT fk_enrollment_course
        FOREIGN KEY (course_id)
        REFERENCES courses (course_id)

    CHECK (completion_status IN ('In Progress', 'Completed', 'Dropped', 'Failed')) -- Valid statuses


-- Table: certificate
-- Records certificates issued to students for completed courses.
    certificate_id PRIMARY KEY, -- Unique for each issued certificate
    enrollment_id NOT NULL, -- Foreign key linking to the enrollment
    serial_number NOT NULL UNIQUE, -- Required: Each certificate has a unique serial number
    issue_date DATE NOT NULL DEFAULT CURRENT_DATE,

    CONSTRAINT fk_cert_instance_enrollment
        FOREIGN KEY (enrollment_id)
        REFERENCES enrollments (enrollment_id)
*/
----------------------------------------------------------------------------------------------------

------------------------------ Phase-2 ----------------------------------------------------------
-------------------------------- DDL --------------------------------------------------------
-- Table: Students
create table students (
	student_id serial primary key,
	first_name varchar(100) not null,
	last_name varchar(100) not null,
	email varchar(255) not null unique,
	phone varchar(50),
	registration_date date not null default current_date
);

CREATE INDEX idx_students_email ON students (email);
--------------------------------------------------------------------------------------------
-- Table: Trainers
create table trainers (
	trainer_id serial primary key,
	first_name varchar(100) not null,
	last_name varchar(100) not null,
	email varchar(255) not null unique,
	hire_date date,
	bio text -- expertise
);
CREATE INDEX idx_trainers_email ON trainers (email); 
--------------------------------------------------------------------------------------------
-- Table: Category
-- Stores the category for the courses
create table category (
	category_id serial primary key,
	category_name varchar(100) not null unique,
	description text
);
CREATE INDEX idx_category_name ON category (category_name); 
--------------------------------------------------------------------------------------------
-- Table: Courses
create table courses (
	course_id serial primary key,
	course_name varchar(255) not null unique,
	category_id int not null,
	description text,
	duration_days int check (duration_days > 0),
	price decimal(10,2) check (price >= 0),
	trainer_id int not null,

	constraint fk_course_trainer
	foreign key (trainer_id) 
	references trainers(trainer_id)
	on delete restrict, -- Stops deletion of trainer associated

	constraint fk_course_category
	foreign key (category_id)
	references category(category_id)
	on delete restrict
);

CREATE INDEX idx_courses_trainer_id ON courses (trainer_id); -- Easier on joins
CREATE INDEX idx_courses_category_id ON courses (category_id);
CREATE INDEX idx_courses_course_name ON courses (course_name); 
--------------------------------------------------------------------------------------------
-- Table enrollments
-- Links students to courses they are enrolled in. (Many-to-Many relationship).
create table enrollments (
	enrollment_id serial primary key,
	student_id int not null,
	course_id int not null,
	enrollment_date date not null default current_date,
	-- completion_status varchar(50) not null default 'in progress', -- e.g., 'in progress', 'completed', 'dropped', 'failed'
    -- completion_date date,
    -- final_grade decimal(5, 2),

	unique (student_id, course_id), -- student can take only once

	constraint fk_enrollment_student
	foreign key (student_id)
	references students(student_id),

	constraint fk_enrollment_course
	foreign key (course_id)
	references courses (course_id)
	
);

CREATE INDEX idx_enrollments_student_id ON enrollments (student_id);
CREATE INDEX idx_enrollments_course_id ON enrollments (course_id); 
--------------------------------------------------------------------------------------------
-- Table: Certificate
-- Records certificates issued to students for enrollments.
create table certificate (
	certificate_id serial primary key,
	enrollment_id int not null,
	serial_number varchar(100) not null unique,
	issue_date date not null default current_date,

	constraint fk_certificate_enrollment
	foreign key (enrollment_id)
	references enrollments (enrollment_id)
	on delete cascade
);

CREATE INDEX idx_certificate_enrollment_id ON certificate (enrollment_id); 
CREATE INDEX idx_certificate_serial_number ON certificate (serial_number); 
--------------------------------------------------------------------------------------------
------------------------------- Sample data -----------------------------------------------
-- Inserting Sample Data

-- Students
INSERT INTO students (first_name, last_name, email, phone, registration_date) VALUES
('Harry', 'Potter', 'harry.potter@hogwarts.ac.uk', '07700100001', '1991-09-01'),
('Hermione', 'Granger', 'hermione.granger@hogwarts.ac.uk', NULL, '1991-09-01'),
('Ron', 'Weasley', 'ron.weasley@hogwarts.ac.uk', '07700100002', '1991-09-01'),
('Draco', 'Malfoy', 'draco.malfoy@hogwarts.ac.uk', NULL, '1991-09-01'),
('Luna', 'Lovegood', 'luna.lovegood@hogwarts.ac.uk', NULL, '1992-09-01'),
('Neville', 'Longbottom', 'neville.longbottom@hogwarts.ac.uk', NULL, '1991-09-01');

select * from students;
---------------------------------------------------------------------------------------------
-- Trainers
INSERT INTO trainers (first_name, last_name, email, hire_date, bio) VALUES
('Albus', 'Dumbledore', 'headmaster@hogwarts.ac.uk', '1970-01-10', 'Headmaster. Master of Transfiguration.'),
('Severus', 'Snape', 'snape@hogwarts.ac.uk', '1981-08-01', 'Potions Master, Head of Slytherin.'),
('Minerva', 'McGonagall', 'mcgonagall@hogwarts.ac.uk', '1956-12-01', 'Head of Gryffindor, Transfiguration Professor.'),
('Filius', 'Flitwick', 'flitwick@hogwarts.ac.uk', '1975-04-18', 'Charms Master, Head of Ravenclaw.'),
('Pomona', 'Sprout', 'sprout@hogwarts.ac.uk', '1976-09-01', 'Herbology Professor, Head of Hufflepuff.'),
('Rubeus', 'Hagrid', 'hagrid@hogwarts.ac.uk', '1993-09-01', 'Keeper of Keys and Grounds, Professor of Care of Magical Creatures.');

select * from trainers;
---------------------------------------------------------------------------------------------
-- Categories
INSERT INTO category (category_name, description) VALUES
('Potions', 'The art of brewing magical concoctions.'),
('Transfiguration', 'Changing the form or appearance of an object or person.'),
('Charms', 'Spells that add properties to an object or creature.'),
('Herbology', 'The study of magical plants.'),
('Care of Magical Creatures', 'Learning how to care for magical beasts.'),
('Defence Against the Dark Arts', 'Methods to defend against dark magic.');

select * from category;
----------------------------------------------------------------------------------------------
-- Courses
INSERT INTO courses (course_name, category_id, description, duration_days, price, trainer_id) VALUES
('Potions Year 1', 1, 'Introduction to basic potions.', 30, 50.00, 2),
('Transfiguration Year 1', 2, 'Introductory transfiguration spells.', 30, 50.00, 3),
('Charms Year 1', 3, 'Basic charm work.', 30, 50.00, 4), -- Flitwick (ID 4), 
('Herbology Year 1', 4, 'Introduction to magical plants.', 30, 50.00, 5), 
('Care of Magical Creatures Year 3', 5, 'Study of less dangerous magical creatures.', 30, 60.00, 6), 
('Defence Against the Dark Arts Year 1', 6, 'Basic defensive spells and theory.', 30, 55.00, 1); 

select * from courses;
----------------------------------------------------------------------------------------------
-- Enrollments
INSERT INTO enrollments (student_id, course_id, enrollment_date) VALUES
(1, 1, '1991-09-01'), 
(1, 2, '1991-09-01'), 
(1, 3, '1991-09-01'), 
(2, 1, '1991-09-01'), 
(2, 2, '1991-09-01'), 
(2, 3, '1991-09-01'), 
(3, 2, '1991-09-01'), 
(3, 3, '1991-09-01'), 
(3, 4, '1991-09-01'),
(4, 2, '1991-09-01'), 
(4, 4, '1991-09-01'), 
(4, 6, '1991-09-01'),
(1, 5, '1993-09-01'), 
(2, 5, '1993-09-01'), 
(3, 5, '1993-09-01'),
(5, 1, '1992-09-01'), 
(5, 3, '1992-09-01'), 
(6, 1, '1991-09-01'),
(6, 4, '1991-09-01');

select * from enrollments;
----------------------------------------------------------------------------------------------
INSERT INTO certificate (enrollment_id, serial_number, issue_date) VALUES

(1, 'HW-POR-1992-001', '1992-06-30'), 
(6, 'HW-CHARMS-1992-002', '1992-06-30'),
(8, 'HW-CHARMS-1992-003', '1992-06-30'), 
(16, 'HW-POTIONS-1992-004', '1992-06-30'), 
(9, 'HW-HERBO-1992-005', '1992-06-30'); 

select * from certificate;
------------------------------------------------------------------------------------------------
------------------------------------ Phase-3 ---------------------------------------------------
------------------------------------- Joins ----------------------------------------------------
-- 1) List students and the courses they enrolled in

select 
	e.enrollment_id,
	s.first_name || ' ' || s.last_name as student_name,
	c.course_name,
	c.description,
	c.duration_days,
	e.enrollment_date
from enrollments e
join students s on e.student_id = s.student_id
join courses c on e.course_id = c.course_id
order by e.enrollment_id, e.student_id, e.course_id;

---------------------------------------------------------------------------------------------
-- 2) Show students who received certificates with trainer names

select 
	s.first_name || ' ' || s.last_name as student_name,
	cs.course_name,
	t.first_name || ' ' || t.last_name as trainer_name,
	c.serial_number,
	c.issue_date
from certificate c
join enrollments e on c.enrollment_id = e.enrollment_id
join students s on s.student_id = e.student_id
join courses cs on cs.course_id = e.course_id
join trainers t on t.trainer_id = cs.trainer_id
order by 1;

---------------------------------------------------------------------------------------------
-- 3) Count number of students per course

select 
	c.course_id,
	c.course_name,
	count(e.student_id) as No_of_enrollments
from courses c
left join enrollments e on c.course_id = e.course_id
group by c.course_id, c.course_name
order by 1;

--------------------------------------------------------------------------------------------
------------------------------------- Phase-4 ----------------------------------------------
------------------------------------ Functions ---------------------------------------------
-- 1) Create `get_certified_students(course_id INT)`
-- → Returns a list of students who completed the given course and received certificates.
create or replace function fn_get_certified_students (p_course_id int)
returns table (
	student_id int,
	student_name text,
	student_email varchar,
	course_name varchar,
	certificate_serial varchar,
	certificate_issue_date date
)
as $$
begin
	return query
	select 
		s.student_id,
		s.first_name || ' ' || s.last_name,
		s.email,
		cs.course_name,
		c.serial_number,
		c.issue_date
	from certificate c
	join enrollments e on e.enrollment_id = c.enrollment_id
	join students s on s.student_id = e.student_id
	join courses cs on cs.course_id = e.course_id
	where e.course_id = p_course_id
	order by 1;
end;
$$ language plpgsql;

-- Testing
select * from fn_get_certified_students(1);

-----------------------------------------------------------------------------------------------
------------------------------- Stored Procedure ----------------------------------------------
-- 1) Create `sp_enroll_student(p_student_id, p_course_id)`
-- → Inserts into `enrollments` and conditionally adds a certificate if completed (simulate with status flag).

create or replace procedure sp_enroll_student(
	p_student_id int,
	p_course_id int,
	p_issue_certificate boolean default false
)
as $$
declare
	v_enrollment_id int;
	v_certificate_id int := null; -- default
	v_serial_number varchar(100);
begin
	-- Exception handling
	begin
		insert into enrollments (student_id, course_id, enrollment_date)
		values (p_student_id, p_course_id, current_date)
		returning enrollment_id into v_enrollment_id;

		RAISE NOTICE 'Successfully enrolled student % in course % with enrollment ID %.',
		p_student_id, p_course_id, v_enrollment_id;

		if p_issue_certificate then
			v_serial_number := 'HW-NEW-' || v_enrollment_id::TEXT || '-' || TO_CHAR(current_date, 'YYYY-MM-DD');

			insert into certificate (enrollment_id, serial_number, issue_date)
				values (v_enrollment_id, v_serial_number, current_date)
			returning certificate_id into v_certificate_id;

			RAISE NOTICE 'Issued certificate with ID % and Serial % for enrollment %.', 
			v_certificate_id, v_serial_number, v_enrollment_id;
		end if;
	exception
		when others then
			RAISE NOTICE 'Enrollment failed: %', sqlerrm;
	end;

end;
$$ language plpgsql;

-- Testing

call sp_enroll_student(5, 2);		

select * from enrollments;

call sp_enroll_student(6, 5, TRUE);

select * from enrollments;
select * from certificate;
----------------------------------------------------------------------------------------------
-------------------------------------- Phase-5 -----------------------------------------------
-------------------------------------- Cursors -----------------------------------------------
-- phase 5: Cursor
-- Use a cursor to:
-- * Loop through all students in a course
-- * Print name and email of those who do not yet have certificates

create or replace function list_uncertified_students(p_course_id int)
returns void -- return nothing
as $$
declare
	rec record;
	v_course_name varchar(100) := NULL;
	v_has_certificate boolean;
	enrollment_cursor cursor for
		select
			s.student_id,
			s.first_name || ' ' || s.last_name as name,
			s.email,
			e.enrollment_id
		from students s
		join enrollments e on s.student_id = e.student_id
		where e.course_id = p_course_id;
begin
	select course_name from courses where course_id = p_course_id
	 into v_course_name;

	if v_course_name is not null then
		RAISE INFO '--- Listing Uncertified Students for Course ID % : %---', p_course_id, v_course_name;

		open enrollment_cursor;

		loop
			fetch enrollment_cursor into rec;

			exit when not found;

			select exists (
	            select 1 
	            from certificate
	            where enrollment_id = rec.enrollment_id
        	) into v_has_certificate; 

			if not v_has_certificate then
				RAISE INFO 'Student: % (ID %), Email: %, Enrollment ID: %', 
				rec.name, rec.student_id, rec.email, rec.enrollment_id;
			end if;
		end loop;

		close enrollment_cursor;

		RAISE INFO '----------------------------------------------------';
	else
		RAISE INFO 'Sorry, the course doesnt exists';
	end if;
end;
$$ language plpgsql;

-- Testing

select list_uncertified_students(2);

-----------------------------------------------------------------------------------------------
------------------------------------- Phase-6 -------------------------------------------------
---------------------------------- Security & Roles -------------------------------------------
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
-------------------------------------- Phase-7 ------------------------------------------------
------------------------------- Transactions and Atomicity ---------------------------------------
-- Phase 7: Transactions and Atomicity
-- Write a transaction block that:
-- * Enrolls a student
-- * Issues a certificate
-- * Fails if certificate generation fails (rollback)

/*
	we can do this as a procedure, but the same has been done in Phase-4 Task
	so in this, only atomicity of transaction is demostrated using the 
	do..block with begin and exception handling
*/

DO $$
DECLARE
    p_student_id INT := 5; --  Student ID
    p_course_id INT := 4;  --  Course ID

    v_enrollment_id INT;
    v_serial_number VARCHAR(100);

BEGIN
    RAISE INFO '--- Starting Transaction: Enroll Student and Issue Certificate ---';
    RAISE INFO 'Attempting to enroll student ID % in course ID %...', p_student_id, p_course_id;

    -- 1: Insert into enrollments
    INSERT INTO enrollments (student_id, course_id, enrollment_date)
    VALUES (p_student_id, p_course_id, CURRENT_DATE)
    RETURNING enrollment_id INTO v_enrollment_id;

    RAISE INFO 'Enrollment record created with ID: %', v_enrollment_id;

    -- 2: Generate a unique serial number for the certificate
    v_serial_number := 'HW-NEW-' || v_enrollment_id::TEXT || '-' || TO_CHAR(current_date, 'YYYY-MM-DD');

    RAISE INFO 'Generated certificate serial number: %', v_serial_number;

    -- 3: Insert into certificates
    INSERT INTO certificate (enrollment_id, serial_number, issue_date)
    VALUES (v_enrollment_id, v_serial_number, CURRENT_DATE);

    RAISE INFO 'Certificate issued successfully for enrollment ID %.', v_enrollment_id;

    RAISE INFO '--- Transaction Successful: Enrollment and Certificate Recorded ---';
	
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Transaction Rolled Back due to an unexpected error: %', SQLERRM;
END $$;

-----------------------------------------------------------------------------------------------