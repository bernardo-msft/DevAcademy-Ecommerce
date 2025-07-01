namespace Ecommerce.API.Middleware;

public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string? Details { get; set; } // Optional: for stack trace in development

    public ApiErrorResponse(int statusCode, string message, string? details = null)
    {
        StatusCode = statusCode;
        Message = message;
        Details = details;
    }
}