using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class CustomExceptionFilter : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        context.Result = new BadRequestObjectResult(new ErrorObjectDTO
        {
            ErrorNumber = 500,
            ErrorMessage = context.Exception.Message
        });
    }
}

// DTO for error response
public class ErrorObjectDTO
{
    public int ErrorNumber { get; set; }
    public required string ErrorMessage { get; set; }
}