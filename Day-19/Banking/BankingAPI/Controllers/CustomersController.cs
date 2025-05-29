using System.Net;
using Microsoft.AspNetCore.Mvc;

[ApiController] 
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto customerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdCustomer = await _customerService.CreateCustomerAsync(customerDto);
            
            return CreatedAtAction(nameof(GetCustomerById), new { id = createdCustomer.CustomerId }, createdCustomer);
        }
        catch (InvalidOperationException ex) 
        {
            return Conflict(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while creating the customer.", detail = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCustomers()
    {
        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while retrieving customers.", detail = ex.Message });
        }
    }

    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = $"Customer with ID '{id}' not found." });
            }
            return Ok(customer);
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while retrieving the customer.", detail = ex.Message });
        }
    }


    [HttpGet("nationalId/{nationalId}")]
    public async Task<IActionResult> GetCustomerByNationalId(string nationalId)
    {
        try
        {
            var customer = await _customerService.GetCustomerByNationalIdAsync(nationalId);
            if (customer == null)
            {
                return NotFound(new { message = $"Customer with National ID '{nationalId}' not found." });
            }
            return Ok(customer);
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while retrieving the customer by National ID.", detail = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerDto customerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedCustomer = await _customerService.UpdateCustomerAsync(id, customerDto);
            return Ok(updatedCustomer);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (InvalidOperationException ex) 
        {
            return Conflict(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while updating the customer.", detail = ex.Message });
        }
    }

    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        try
        {
            await _customerService.DeleteCustomerAsync(id);
            return NoContent(); 
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (InvalidOperationException ex) 
        {
            return BadRequest(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while deleting the customer.", detail = ex.Message });
        }
    }
}