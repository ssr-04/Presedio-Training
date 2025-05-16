------------------------------- Functions ---------------------------------------------------
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