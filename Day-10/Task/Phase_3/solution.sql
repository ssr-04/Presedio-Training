---------------------------- Joins ----------------------------------------------------------
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