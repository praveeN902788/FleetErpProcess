using Bharuwa.Erp.Common;
using Bharuwa.Erp.Common.FMS;
using Bharuwa.Erp.Entities.FMS;
using Bharuwa.Erp.Services.FMS.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bharuwa.Erp.API.FMS.Controllers.V1
{
    /// <summary>
    /// Fleet management API version 1.0
    /// Provides basic fleet operations, driver management, and ride tracking
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/fleet")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class FleetV1Controller : VersionAwareController
    {
        private readonly IFleetManagementDal _fleetManagement;

        public FleetV1Controller(
            IFleetManagementDal fleetManagementDal,
            IVersionManagementService versionService,
            ILogger<FleetV1Controller> logger) 
            : base(versionService, logger)
        {
            _fleetManagement = fleetManagementDal;
        }

        /// <summary>
        /// Gets fleet driver details for rides by driver ID
        /// </summary>
        /// <param name="driverId">Driver ID</param>
        /// <param name="fromDate">Start date for filtering (optional)</param>
        /// <param name="toDate">End date for filtering (optional)</param>
        /// <returns>Driver details and ride information</returns>
        [HttpGet("drivers/{driverId:int}/rides")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        [ProducesResponseType(typeof(APIResponseDto), 404)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> GetDriverDetailsForRides(
            int driverId,
            [FromQuery] string fromDate = null, 
            [FromQuery] string toDate = null)
        {
            if (driverId <= 0)
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentException("Driver ID must be greater than 0"), 
                    "Invalid driver ID provided"));
            }

            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Getting fleet driver details for driver {DriverId} from {FromDate} to {ToDate}", 
                    driverId, fromDate, toDate);
                
                var result = await _fleetManagement.GetFleetDriverDetailsByIdAsync(driverId, fromDate, toDate);
                return result;
            }, "Driver details retrieved successfully");
        }

        /// <summary>
        /// Gets fleet driver details for rides by vehicle booking code
        /// </summary>
        /// <param name="vehicleBookingCode">Vehicle booking code</param>
        /// <returns>Driver details for the specific booking</returns>
        [HttpGet("drivers/by-booking/{vehicleBookingCode}")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        [ProducesResponseType(typeof(APIResponseDto), 404)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> GetDriverDetailsByBookingCode(string vehicleBookingCode)
        {
            if (string.IsNullOrWhiteSpace(vehicleBookingCode))
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentException("Vehicle booking code is required"), 
                    "Vehicle booking code cannot be null or empty"));
            }

            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Getting fleet driver details for booking code {BookingCode}", 
                    vehicleBookingCode);
                
                var result = await _fleetManagement.GetFleetDriverDetailsForRidesByVehicleBookingCodeAsync(vehicleBookingCode);
                return result;
            }, "Driver details retrieved successfully");
        }

        /// <summary>
        /// Adds a review about journey and driver
        /// </summary>
        /// <param name="reviewRequest">Review details</param>
        /// <returns>Review submission result</returns>
        [HttpPost("reviews")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> AddReview([FromBody] ReviewRequest reviewRequest)
        {
            if (reviewRequest == null)
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentNullException(nameof(reviewRequest)), 
                    "Review request is required"));
            }

            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return validationResult;
            }

            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Adding review for booking {BookingCode} with rating {Rating}", 
                    reviewRequest.VehicleBookingCode, reviewRequest.Rating);
                
                // Convert to the expected format for the service
                var reviewData = new
                {
                    VehicleBookingCode = reviewRequest.VehicleBookingCode,
                    Rating = reviewRequest.Rating,
                    Comment = reviewRequest.Comment,
                    DriverId = reviewRequest.DriverId
                };
                
                var result = await _fleetManagement.AddReviewAboutJourneyAndDriver(reviewData);
                return result;
            }, "Review added successfully");
        }

        /// <summary>
        /// Gets fleet statistics and summary information
        /// </summary>
        /// <param name="fromDate">Start date for statistics (optional)</param>
        /// <param name="toDate">End date for statistics (optional)</param>
        /// <returns>Fleet statistics</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> GetFleetStatistics(
            [FromQuery] string fromDate = null, 
            [FromQuery] string toDate = null)
        {
            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Getting fleet statistics from {FromDate} to {ToDate}", 
                    fromDate, toDate);
                
                // This would be implemented with actual statistics logic
                var result = new
                {
                    TotalDrivers = 0,
                    ActiveRides = 0,
                    CompletedRides = 0,
                    AverageRating = 0.0,
                    DateRange = new { FromDate = fromDate, ToDate = toDate }
                };
                
                return result;
            }, "Fleet statistics retrieved successfully");
        }

        /// <summary>
        /// Gets driver performance metrics
        /// </summary>
        /// <param name="driverId">Driver ID</param>
        /// <param name="fromDate">Start date for metrics (optional)</param>
        /// <param name="toDate">End date for metrics (optional)</param>
        /// <returns>Driver performance metrics</returns>
        [HttpGet("drivers/{driverId:int}/performance")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        [ProducesResponseType(typeof(APIResponseDto), 404)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> GetDriverPerformance(
            int driverId,
            [FromQuery] string fromDate = null, 
            [FromQuery] string toDate = null)
        {
            if (driverId <= 0)
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentException("Driver ID must be greater than 0"), 
                    "Invalid driver ID provided"));
            }

            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Getting performance metrics for driver {DriverId} from {FromDate} to {ToDate}", 
                    driverId, fromDate, toDate);
                
                // This would be implemented with actual performance metrics logic
                var result = new
                {
                    DriverId = driverId,
                    TotalRides = 0,
                    AverageRating = 0.0,
                    OnTimePercentage = 0.0,
                    CustomerSatisfaction = 0.0,
                    DateRange = new { FromDate = fromDate, ToDate = toDate }
                };
                
                return result;
            }, "Driver performance metrics retrieved successfully");
        }
    }

    #region Request Models

    public class ReviewRequest
    {
        [Required]
        public string VehicleBookingCode { get; set; }
        
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
        
        public string Comment { get; set; }
        
        [Required]
        public int DriverId { get; set; }
    }

    #endregion
}