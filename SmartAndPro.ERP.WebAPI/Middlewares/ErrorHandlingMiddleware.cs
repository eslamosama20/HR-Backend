using Newtonsoft.Json;
using SmartAndPro.ERP.SharedKernel.Exceptions;

namespace SmartAndPro.ERP.WebAPI.Middlewares;

public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware( ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case BusinessRuleViolationException brvEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = brvEx.Message;
                response.Errors = brvEx.Errors.Select(e => new ErrorDetail
                {
                    RuleId = e.RuleId,
                    ErrorMessage = e.ErrorMessage,
                    ErrorCode = e.ErrorCode,
                    Severity = e.Severity.ToString()
                }).ToList();
                _logger.LogWarning(exception, "Business rule violation: {Message}", exception.Message);
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = exception.Message;
                _logger.LogError(exception, "An unexpected error occurred: {Message}", exception.Message);
                break;



        }

        await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}

public class ErrorResponse
{
    public string Message { get; set; }
    public List<ErrorDetail> Errors { get; set; } = new List<ErrorDetail>();
}

public class ErrorDetail
{
    public string RuleId { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorCode { get; set; }
    public string Severity { get; set; }
}
