using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/[controller]")]
public class DoctorController : ControllerBase
{
    static List<Doctor> doctors = new List<Doctor>
    {
        new Doctor{Id=101,Name="Ramu"},
        new Doctor{Id=102,Name="Somu"},
    };

    [HttpGet]
    public ActionResult<IEnumerable<Doctor>> GetDoctors()
    {
        return Ok(doctors);
    }

    [HttpPost]
    public ActionResult<Doctor> PostDoctor([FromBody] Doctor doctor)
    {
        doctors.Add(doctor);
        return Created("", doctor);
    }

    [HttpGet("{id}")]
    public ActionResult<Doctor> GetDoctor(int id)
    {
        var doctor = doctors.FirstOrDefault(d => d.Id == id);
        if (doctor == null)
        {
            return NotFound();
        }
        return Ok(doctor);
    }

    [HttpPut("{id}")]
    public ActionResult<Doctor> PutDoctor(int id, [FromBody] Doctor updatedDoctor)
    {
        var existingDoctor = doctors.FirstOrDefault(d => d.Id == id);

        if (existingDoctor == null)
        {
            return NotFound($"Doctor with Id {id} not found."); // HTTP 404
        }

        existingDoctor.Name = updatedDoctor.Name;

        return NoContent(); //204- Common for update or else send back 200- updated doctor
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteDoctor(int id)
    {
        var doctorsRemovedCount = doctors.RemoveAll(d => d.Id == id);

        if (doctorsRemovedCount == 0)
        {
            return NotFound($"Doctor with Id {id} not found."); // HTTP 404
        }

        return NoContent(); // 204 No Content (Common for successful delete)
    }
}