using Microsoft.AspNetCore.Mvc;
using System.Net;

[ApiController]
    [Route("api/[controller]")] 
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto accountDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdAccount = await _accountService.CreateAccountAsync(accountDto);
                return CreatedAtAction(nameof(GetAccountById), new { id = createdAccount.AccountId }, createdAccount);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while creating the account.", detail = ex.Message });
            }
        }

    
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(Guid id)
        {
            try
            {
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                {
                    return NotFound(new { message = $"Account with ID '{id}' not found." });
                }
                return Ok(account);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while retrieving the account.", detail = ex.Message });
            }
        }

        [HttpGet("byCustomerId/{customerId}")]
        public async Task<IActionResult> GetAccountsByCustomerId(Guid customerId)
        {
            try
            {
                var accounts = await _accountService.GetAccountsByCustomerIdAsync(customerId);
                if (!accounts.Any())
                {
                    return NotFound(new { message = $"No active accounts found for customer ID '{customerId}'." });
                }
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while retrieving accounts for the customer.", detail = ex.Message });
            }
        }

        [HttpGet("byAccountNumber/{accountNumber}")]
        public async Task<IActionResult> GetAccountByAccountNumber(string accountNumber)
        {
            try
            {
                var account = await _accountService.GetAccountByAccountNumberAsync(accountNumber);
                if (account == null)
                {
                    return NotFound(new { message = $"Account with number '{accountNumber}' not found." });
                }
                return Ok(account);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while retrieving the account by number.", detail = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            try
            {
                await _accountService.DeleteAccountAsync(id);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while deleting the account.", detail = ex.Message });
            }
        }
    }