using Bharuwa.Erp.Common;
using Bharuwa.Erp.Common.FMS;
using Bharuwa.Erp.Common.PMS;
using Bharuwa.Erp.Entities.FMS;
using Bharuwa.Erp.Services.FMS;
using Bharuwa.Erp.Services.FMS.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Bharuwa.Erp.API.FMS.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CustomerController(ICustomerManagementDal customerManagementDal) : ApiBaseController
    {
        private readonly ICustomerManagementDal _customerManagement = customerManagementDal;

        [HttpGet("GetDashboardCountsForCustomer")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetDashboardCountsForCustomer([FromQuery] string Fromdate = null, string Todate = null)
        {

            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _customerManagement.GetDashboardCountsForCustomer(Fromdate, Todate);
                return result;
            });
        }

        [HttpGet("GetDashboardDetailsForCustomer")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetDashboardDetailsForCustomer([FromQuery] string category , [FromQuery] string Fromdate = null, string Todate = null)
        {

            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _customerManagement.GetDashboardDetailsForCustomer(category, Fromdate, Todate);
                return result;
            });
        }

        [HttpPost("GetAllTrackingDetailsForCustomer")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllTrackingDetailsForCustomer([FromBody] TrackingFilters options)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _customerManagement.GetAllTrackingDetailsForCustomer(options);
                return result;
            });
        }

        [HttpGet("GetAllVehicleCategoryForVehicleRequest")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllVehicleCategoryForVehicleRequest()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _customerManagement.GetAllVehicleCategoryForVehicleRequest();
                return result;
            });
        }
        [HttpGet("GetAllVehicleCategoryForVehicleRequest")]
        [ApiVersion("2.0")]
        public async Task<IActionResult> GetAllVehicleCategoryForVehicleRequestV2([FromQuery] string type)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _customerManagement.GetAllVehicleCategoryForVehicleRequestV2(type);
                return result;
            });
        }

    }
}
