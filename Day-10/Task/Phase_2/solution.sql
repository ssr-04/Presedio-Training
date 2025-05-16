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
----------------------------------------------------------------------------------------------