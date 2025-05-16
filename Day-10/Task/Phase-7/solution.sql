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