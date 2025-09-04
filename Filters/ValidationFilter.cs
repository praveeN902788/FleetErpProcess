using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;

namespace Bharuwa.Erp.API.FMS.Filters
{
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new ValidationError
                    {
                        Field = x.Key,
                        Messages = x.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    })
                    .ToArray();

                var response = new ValidationErrorResponse
                {
                    Message = "Validation failed",
                    Errors = errors,
                    TraceId = context.HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                };

                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed after execution
        }
    }

    public class ValidationErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public ValidationError[] Errors { get; set; } = Array.Empty<ValidationError>();
        public string TraceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class ValidationError
    {
        public string Field { get; set; } = string.Empty;
        public string[] Messages { get; set; } = Array.Empty<string>();
    }
}