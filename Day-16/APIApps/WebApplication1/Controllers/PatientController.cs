using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/[controller]")]
public class PatientController : ControllerBase
{
    private static List<Patient> patients = new List<Patient>
    {
        new Patient { Id = 101, Name = "Mr.Darcy", DateOfBirth = new DateTime(1985, 10, 14), Ailment = "Common Cold" }, //Just some names for Pride & Prejustice
        new Patient { Id = 102, Name = "Ms.Elizabeth", DateOfBirth = new DateTime(1970, 5, 20), Ailment = "Headache" }
    };

    [HttpGet]
    public ActionResult<IEnumerable<Patient>> GetPatients()
    {
        return Ok(patients);
    }

    [HttpGet("{id}")]
    public ActionResult<Patient> GetPatient(int id)
    {
        var patient = patients.FirstOrDefault(p => p.Id == id);

        if (patient == null)
        {
            return NotFound($"Patient with Id {id} not found.");
        }
        return Ok(patient);
    }

    [HttpPost]
    public ActionResult<Patient> PostPatient([FromBody] Patient patient)
    {
        //Validation check done in Patient model (Maybe not can have a custom validation check in middleware when using DTo)
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (patients.Any(p => p.Id == patient.Id))
        {
            // returns http 409 conflict 9if a patient with this id already exists)
            return Conflict($"Patient with Id {patient.Id} already exists.");
        }

        patients.Add(patient);

        // returns http 201 created, with a 'location' header pointing to the new resource
        // as well as the created patient object in the body.
        return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient); //commonly used

    }

    [HttpPut("{id}")]
    public ActionResult PutPatient(int id, [FromBody] Patient updatedPatient)
    {
        // just basic check on id given in param and body
        if (id != updatedPatient.Id)
        {
            return BadRequest("Patient ID in URL does not match ID in request body.");
        }

        // validation same as before (done in patient model)
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingPatient = patients.FirstOrDefault(p => p.Id == id);

        if (existingPatient == null)
        {
            return NotFound($"Patient with Id {id} not found.");
        }

        // udating properties of the existing patient
        existingPatient.Name = updatedPatient.Name;
        existingPatient.DateOfBirth = updatedPatient.DateOfBirth;
        existingPatient.Ailment = updatedPatient.Ailment;

        return NoContent(); // maybe can send the modified patient with ok response
    }
    
    [HttpDelete("{id}")]
    public ActionResult DeletePatient(int id)
    {
        // using remove all to delete all patient with that Id (not ideal but safe), returns num of deleted entries
        var patientsRemovedCount = patients.RemoveAll(p => p.Id == id);

        if (patientsRemovedCount == 0)
        {
            return NotFound($"Patient with Id {id} not found.");
        }

        return NoContent(); // usual response post deletion
    }
}