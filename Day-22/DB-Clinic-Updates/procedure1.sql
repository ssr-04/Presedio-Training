create or replace function get_doctors_by_specialityName(p_speciality_name varchar)
returns table(
	"Id" INT,
    "Name" VARCHAR(25),
    "Email" VARCHAR(75),
    "Phone" VARCHAR(20),
    "Status" VARCHAR(50),
    "YearsOfExperience" REAL,
    "IsDeleted" BOOLEAN
)
as
$$
begin
	return query
	select
        d."Id",
        d."Name",
        d."Email",
        d."Phone",
        d."Status",
        d."YearsOfExperience",
        d."IsDeleted"
    from
        "Doctors" d
    join
        "DoctorSpecialities" ds on d."Id" = ds."DoctorId"
	join
		"Specialities" s on ds."SpecialityId" = s."Id"
    where
        s."Name" = p_speciality_name
        AND d."IsDeleted" = FALSE;
END;
$$ language plpgsql;

select * from get_doctors_by_specialityName('Neurology');

