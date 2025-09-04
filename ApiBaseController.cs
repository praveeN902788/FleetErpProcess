using Bharuwa.Erp.Common;
using ERPServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;

namespace Bharuwa.Erp.API.FMS
{
    [BasicAuthentication]
    public class ApiBaseController : Controller
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext _context, ActionExecutionDelegate next)
        {
            await base.OnActionExecutionAsync(_context, next);
        }

        public async Task<IActionResult> ResponseWrapperAsync<T>(Func<Task<T>> func)
        {
            try
            {
                T result = await func();
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResponseFileWrapperAsync(Func<Task<IActionResult>> func)
        {
            try
            {
                IActionResult result = await func();
                return result;
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        private IActionResult HandleError(Exception ex)
        {
            return ex switch
            {
                ArgumentNullException _ => new BadRequestObjectResult(new { message = "A required parameter was null." }),
                ArgumentException _ => new BadRequestObjectResult(new { message = "Invalid argument provided." }),
                KeyNotFoundException _ => new NotFoundObjectResult(new { message = "The requested resource was not found." }),
                UnauthorizedAccessException _ => new UnauthorizedObjectResult(new { message = "You do not have permission to perform this action." }),
                InvalidOperationException _ => new BadRequestObjectResult(new { message = "The request could not be processed due to an invalid operation." }),
                BposException _ => new BadRequestObjectResult(ex.Message),
                SqlException _ => new BadRequestObjectResult(ex.Message),
                TimeoutException _ => new StatusCodeResult(504),
                _ => StatusCode(500, new { message = "An unexpected error occurred. Please try again later." }),
            };
        }
    }
}
