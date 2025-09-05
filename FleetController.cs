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
    public class FleetController(IFleetManagementDal iFleetManagementDal) : ApiBaseController
    {
        private readonly IFleetManagementDal _iFleetManagementDal = iFleetManagementDal;

        [HttpGet("getFleetDriverDetailsForRides/{driverId:int}")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetFleetDriverDetailsForRidesAsync(int driverId, [FromQuery] string FromDate = null, string ToDate = null)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetFleetDriverDetailsByIdAsync(driverId, FromDate, ToDate);
                return result;
            });
        }

        [HttpGet("getFleetDriverDetailsForRides/{driverId:int}")]
        [ApiVersion("2.0")]
        public async Task<IActionResult> GetFleetDriverDetailsForRidesAsyncUpgrade(int driverId, [FromQuery] string FromDate = null, string ToDate = null)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetFleetDriverDetailsByIdUpdate(driverId, FromDate, ToDate);
                return result;
            });
        }

        [HttpGet("getFleetDriverDetailsForRidesByVehicleBookingCode/{vehicleBookingCode}")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetFleetDriverDetailsForRidesByVehicleBookingCodeAsync(string VehicleBookingCode)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetFleetDriverDetailsForRidesByVehicleBookingCodeAsync(VehicleBookingCode);
                return result;
            });
        }

        [HttpPost("AddReviewAboutJourneyAndDriver")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> AddReviewAboutJourneyAndDriverAsync([FromBody] FleetManagement fleetManagement)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.AddReviewAboutJourneyAndDriverAsync(fleetManagement);
                return result;
            });
        }

        [HttpGet("getAllActiveExpenseMasterPagedList")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllActiveExpenseMasterPagedListAsync()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetAllActiveExpenseMasterPagedListAsync();
                return result;
            });
        }

        [HttpPost("RaiseComplainByDriver")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> RaiseComplainByDriverAsync([FromBody] RaiseComplainByDriverRequestDto data)
        {
            //var raiseComplainByDriverRequestDto = new RaiseComplainByDriverRequestDto()
            //{
            //    VehicleBookingCode = data["VehicleBookingCode"]?.ToString(),
            //    ComplainRaise = data["ComplainRaise"]?.ToString()
            //};
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.RaiseComplainByDriverAsync(data);
                return result;
            });
        }

        [HttpPost("StartRides")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> StartRidesAsync([FromBody] RideRequestDto rideRequestDto)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.StartRidesAsync(rideRequestDto);
                return result;
            });
        }

        [HttpPost("CompletedRides")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> CompletedRidesAsync([FromBody] RideRequestDto rideRequestDto)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.CompletedRidesAsync(rideRequestDto);
                return result;
            });
        }

        [HttpPost("CancelRides")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> CancelRidesAsync([FromBody] CancelRideRequestDto rideRequestDto)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.CancelRidesAsync(rideRequestDto);
                return result;
            });
        }

        [HttpGet("getAllPassengerRides")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllPassengerRidesAsync()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetAllPassengerRidesAsync();
                return result;
            });
        }



        [HttpPost("SaveLocationDataForTracking")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> SaveLocationDataForTracking([FromBody] List<FMSUserLocationDataDto> locations)
        {

            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.SaveLocationDataFromAndroidAsync(locations);
                return result;
            });
        }

        [HttpGet("GetDashboardDataForFleet")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetDashboardDataForFleet([FromQuery] string Fromdate = null, string Todate = null)
        {

            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetDashboardDataForFleet(Fromdate,Todate);
                return result;
            });
        }


        [HttpGet("GetExpenseDashboardData")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetExpenseDashboardData([FromQuery] string Fromdate = null, string Todate = null)
        {

            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetExpenseDashboardData(Fromdate, Todate);
                return result;
            });
        }


        [HttpGet("GetDashboardCountsForFleet")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetDashboardCountsForFleet([FromQuery] string Fromdate = null, string Todate = null)
        {

            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetDashboardCountsForFleet(Fromdate, Todate);
                return result;
            });
        }


        [HttpGet("GetAllUpcomingTrips")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllUpcomingTrips([FromQuery] string Fromdate = null, string Todate = null)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetAllUpcomingTrips(Fromdate, Todate);
                return result;
            });
        }

        [HttpGet("GetAllCompletedTrips")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllCompletedTrips([FromQuery] string Fromdate = null, string Todate = null)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetAllCompletedTrips(Fromdate, Todate);
                return result;
            });
        }

        [HttpGet("GetAllRunningTrips")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllRunningTrips([FromQuery] string Fromdate = null, string Todate = null)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetAllRunningTrips(Fromdate, Todate);
                return result;
            });
        }



        [HttpGet("GetAllAvailableVehicles")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllAvailableVehicles()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetAllAvailableVehicles();
                return result;
            });
        }


        [HttpGet("GetAllVehiclesDetails")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllVehiclesDetails()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetAllVehiclesDetails();
                return result;
            });
        }

        [HttpGet("GetAllExpenseDetails")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllExpenseDetails([FromQuery] string Fromdate = null, string Todate = null)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetAllExpenseDetails(Fromdate, Todate);
                return result;
            });
        }

        [HttpPost("GetAllTrackingDetails")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllTrackingDetails([FromBody] TrackingFilters options)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetAllTrackingDetails(options);
                return result;
            });
        }

        [HttpGet("GetAllCNoteDetailsbyId")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllCNoteDetailsbyId([FromQuery] int id)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetVehicleCNoteById(id);
                return result;
            });
        }

        [HttpGet("GetAllTHCNoteDetailsbyId")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllTHCNoteDetailsbyId([FromQuery] int id)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetManagementDal.GetVehicleTHCNoteById(id);
                return result;
            });
        }

    }
}
