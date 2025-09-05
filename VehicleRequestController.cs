using Bharuwa.Erp.Common;
using Bharuwa.Erp.Entities.FMS;
using Bharuwa.Erp.Services.FMS.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bharuwa.Erp.API.FMS.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class VehicleRequestController(IFleetVehicleBookingRequest iFleetVehicleBookingRequest) : ApiBaseController
    {
        private readonly IFleetVehicleBookingRequest _iFleetVehicleBookingRequest = iFleetVehicleBookingRequest;

        [HttpPost("saveVehicleBookingRequest")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> SaveVehicleBookingRequestAsync([FromBody] FleetVehicleBookingRequest fleetVehicleBookingRequest)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.SaveVehicleBookingRequestAsync(fleetVehicleBookingRequest);
                return result;
            });
        }

        [HttpGet("getVehicleBookingRequestById/{id:int}")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetVehicleBookingRequestByIdAsyncV1(int id)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.GetVehicleBookingRequestByIdAsync(id);
                return result;
            });
        }

        [HttpGet("getVehicleBookingRequestById/{id:int}")]
        [ApiVersion("2.0")]
        public async Task<IActionResult> GetVehicleBookingRequestByIdAsyncV2(int id)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.GetVehicleBookingRequestByIdAsync(id);
                return result;
            });
        }

        [HttpGet("getAllVehicleBookingRequestPagedList")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllVehicleBookingRequestPagedListAsync()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.GetAllVehicleBookingRequestPagedListAsync();
                return result;
            });
        }

        [HttpGet("getAllVehicleBookingRequestPagedList")]
        [ApiVersion("2.0")]
        public async Task<IActionResult> GetAllVehicleBookingRequestPagedListV2Async()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.GetAllVehicleBookingRequestPagedListV2Async();
                return result;
            });
        }

        [HttpGet("getActiveVehicleMasterList/{vehicleType:int}")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetTypeWiseActiveVehicleMasterListAsync(int vehicleType)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.GetTypeWiseActiveVehicleMasterListAsync(vehicleType);
                return result;
            });
        }

        [HttpGet("getAllActiveRouteMasterPagedList")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllActiveRouteMasterPagedListAsync()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.GetRouteVehicleMasterListAsync();
                return result;
            });
        }

        [HttpGet("GetActiveCityListAsync")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetActiveCityListAsync()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.GetActiveCityListAsync();
                return result;
            });
        }

        //Sarvodaya configuration api
        [HttpPost("saveVehicleBookingRequestNew")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> saveVehicleBookingRequestNew([FromBody] FleetVehicleBookingRequestNew fleetVehicleBookingRequest)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.SaveVehicleBookingRequestNewAsync(fleetVehicleBookingRequest);
                return result;
            });
        }

        [HttpGet("getAllVehicleBookingRequestPagedListNew")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> getAllVehicleBookingRequestPagedListNew([FromQuery] string MobileNumber,string? FromDate= null, string? ToDate = null)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.getAllVehicleBookingRequestPagedListNew(MobileNumber,FromDate,ToDate);
                return result;
            });
        }

        [HttpGet("getAllVehicleBookingRequestPagedListNewbyId")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> getAllVehicleBookingRequestPagedListNewbyId([FromQuery] string VehicleBookingCode,string MobileNumber)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.getAllVehicleBookingRequestPagedListNewbyId(VehicleBookingCode, MobileNumber);
                return result;
            });
        }

        [HttpGet("getEstimatedTimeArrival")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> getEstimatedTimeArrival([FromQuery] string VehicleBookingCode,string MobileNumber)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.getEstimatedTimeArrival(VehicleBookingCode, MobileNumber);
                return result;
            });
        }

        [HttpGet("getActiveVehicleMasterListByVehicleNumber/{VehicleNumber}")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetActiveVehicleMasterListByVehicleNumber(string VehicleNumber)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.GetActiveVehicleMasterListByVehicleNumber(VehicleNumber);
                return result;
            });
        }

        [HttpGet("ShiftMasterByShiftGroupId")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> ShiftMasterByShiftGroupId(int shiftGroupId)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.ShiftMasterActiveListByShiftGroup(shiftGroupId);
                return result;
            });
        }

        [HttpGet("ShiftGroup")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> ShiftGroup()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.ShiftGroup();
                return result;
            });
        }

        [HttpGet("VehicleCheckList")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> VehicleCheckList([FromQuery] int VehicleId)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.VehicleCheckList(VehicleId);
                return result;
            });
        }

        [HttpGet("getAllVehicleCheckListPagedList")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> getAllVehicleCheckListPagedList()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.getAllVehicleCheckListPagedList();
                return result;
            });
        }

        [HttpGet("getAllDriverCheckListPagedList")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> getAllDriverCheckListPagedList()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.getAllDriverCheckListPagedList();
                return result;
            });
        }


        [HttpGet("GetAllVehicleRequestSavedList")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllVehicleRequestSavedList()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBookingRequest.GetAllVehicleRequestSavedList();
                return result;
            });
        }

    }
}
