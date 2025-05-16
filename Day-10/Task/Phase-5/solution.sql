------------------------------- Cursors ---------------------------------------------------
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