using Bharuwa.Erp.Common;
using Microsoft.AspNetCore.Mvc;

namespace Bharuwa.Erp.API.FMS.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected APIResponseDto CreateResponse()
        {
            var apiVersion = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";

            return new APIResponseDto
            {
                ApiVersion = apiVersion
            };
        }
    }
}