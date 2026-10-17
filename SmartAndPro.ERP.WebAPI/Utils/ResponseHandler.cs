using System.Net;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.WebAPI.Utils;

public static class ResponseHandler
{
    public static IActionResult HandleResult<T>(this OperationResponse<T> result, bool isPaginated = false) where T : class
    {
        if (result is null) throw new NullReferenceException();
        return result.ErrorCode switch
        {
            ErrorCodes.Success => new OkObjectResult(isPaginated ? result : result.Data),
            ErrorCodes.GeneralFail => new ContentResult 
            {
                StatusCode = (int) HttpStatusCode.InternalServerError,
                Content = result.Message ?? "An error occurred while processing your request.",
                ContentType = "text/plain"
            },
            ErrorCodes.NotFound => new NotFoundObjectResult(result.Message),
            ErrorCodes.Unauthorized => new UnauthorizedObjectResult(result.Message),
            ErrorCodes.Forbidden => new  ContentResult 
            {
                StatusCode = (int) HttpStatusCode.Forbidden,
                Content = result.Message ?? "You are not authorized to access this resource.",
                ContentType = "text/plain"
            },
            ErrorCodes.Conflict => new ObjectResult(new { message = result.Message ?? "A conflict occurred while processing your request." })
            {
                StatusCode = (int) HttpStatusCode.Conflict
            },
            ErrorCodes.Validation => new BadRequestObjectResult(result.Message),
            ErrorCodes.UnprocessableEntity => new ContentResult 
            {
                StatusCode = (int) HttpStatusCode.UnprocessableEntity,
                Content = result.Message ?? "The request could not be processed.",
                ContentType = "text/plain"
            },
            ErrorCodes.NoContent => new NoContentResult(),
            ErrorCodes.NoRecordsUpdated => new BadRequestObjectResult(result.Message), 
            ErrorCodes.BadRequest => new BadRequestObjectResult(result.Message),
            _ => new NoContentResult()
        };
    }
    
}