using Bharuwa.Erp.Common;
using Bharuwa.Erp.Common.FMS;
using Bharuwa.Erp.Common.PMS;
using Bharuwa.Erp.Entities.FMS;
using Bharuwa.Erp.Services.FMS.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Bharuwa.Erp.API.FMS.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ManagerAppController(IManagerAppDal iFleetManagementDal) : ApiBaseController
    {
        private readonly IManagerAppDal _iFleetManagementDal = iFleetManagementDal;

        [HttpGet("GetAllDriversRides")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllDriversRides([FromQuery] string FromDate = null, string ToDate = null)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetAllDriversRides(FromDate, ToDate);
                return result;
            });
        }

        [HttpGet("GetDriverDetailsRidesByVehicleBookingCode/{vehicleBookingCode}")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetDriverDetailsRidesByVehicleBookingCode(string VehicleBookingCode)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetDriverDetailsRidesByVehicleBookingCode(VehicleBookingCode);
                return result;
            });
        }


        [HttpGet("GetAdminDashboard")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAdminDashboard([FromQuery] string FromDate = null, string ToDate = null)
        {

            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetAdminDashboard(FromDate, ToDate);
                return result;
            });
        }


    }
}
