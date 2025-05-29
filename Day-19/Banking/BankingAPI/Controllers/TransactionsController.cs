using System.Net;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")] 
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> MakeDeposit([FromBody] MakeDepositDto depositDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var transaction = await _transactionService.MakeDepositAsync(depositDto);
            return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.TransactionId }, transaction);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message }); 
        }
        catch (KeyNotFoundException ex) 
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while processing the deposit.", detail = ex.Message });
        }
    }


    [HttpPost("withdrawal")]
    [ProducesResponseType(typeof(TransactionResponseDto), (int)HttpStatusCode.Created)] 
    [ProducesResponseType((int)HttpStatusCode.BadRequest)] 
    [ProducesResponseType((int)HttpStatusCode.NotFound)] 
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)] 
    public async Task<IActionResult> MakeWithdrawal([FromBody] MakeWithdrawalDto withdrawalDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var transaction = await _transactionService.MakeWithdrawalAsync(withdrawalDto);
            return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.TransactionId }, transaction);
        }
        catch (InvalidOperationException ex) 
        {
            return BadRequest(new { message = ex.Message }); 
        }
        catch (KeyNotFoundException ex) 
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while processing the withdrawal.", detail = ex.Message });
        }
    }

    
    [HttpPost("transfer")]
    public async Task<IActionResult> MakeTransfer([FromBody] MakeTransferDto transferDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var transactions = await _transactionService.MakeTransferAsync(transferDto);

            return StatusCode((int)HttpStatusCode.Created, transactions);
        }
        catch (InvalidOperationException ex) 
        {
            return BadRequest(new { message = ex.Message }); 
        }
        catch (KeyNotFoundException ex) 
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while processing the transfer.", detail = ex.Message });
        }
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return NotFound(new { message = $"Transaction with ID '{id}' not found." });
            }
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while retrieving the transaction.", detail = ex.Message });
        }
    }

    
    [HttpGet("account/{accountId}")]
    public async Task<IActionResult> GetAccountTransactions(Guid accountId, [FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        try
        {
            var transactions = await _transactionService.GetAccountTransactionsAsync(accountId, startDate, endDate);
            if (!transactions.Any())
            {
                return Ok(new List<TransactionResponseDto>()); 
            }
            return Ok(transactions);
        }
        catch (KeyNotFoundException ex) 
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while retrieving account transactions.", detail = ex.Message });
        }
    }
}