using Bharuwa.Erp.Common;
using Bharuwa.Erp.Common.FMS;
using Bharuwa.Erp.Entities.FMS;
using Bharuwa.Erp.Services.FMS.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bharuwa.Erp.API.FMS.Controllers.V1
{
    /// <summary>
    /// Customer management API version 1.0
    /// Provides basic customer operations and dashboard functionality
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/customer")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class CustomerV1Controller : VersionAwareController
    {
        private readonly ICustomerManagementDal _customerManagement;

        public CustomerV1Controller(
            ICustomerManagementDal customerManagementDal,
            IVersionManagementService versionService,
            ILogger<CustomerV1Controller> logger) 
            : base(versionService, logger)
        {
            _customerManagement = customerManagementDal;
        }

        /// <summary>
        /// Gets dashboard counts for customer
        /// </summary>
        /// <param name="fromDate">Start date for filtering (optional)</param>
        /// <param name="toDate">End date for filtering (optional)</param>
        /// <returns>Dashboard count data</returns>
        [HttpGet("dashboard/counts")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> GetDashboardCounts(
            [FromQuery] string fromDate = null, 
            [FromQuery] string toDate = null)
        {
            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Getting dashboard counts for customer from {FromDate} to {ToDate}", 
                    fromDate, toDate);
                
                var result = await _customerManagement.GetDashboardCountsForCustomer(fromDate, toDate);
                return result;
            }, "Dashboard counts retrieved successfully");
        }

        /// <summary>
        /// Gets detailed dashboard information for a specific category
        /// </summary>
        /// <param name="category">Category to filter by</param>
        /// <param name="fromDate">Start date for filtering (optional)</param>
        /// <param name="toDate">End date for filtering (optional)</param>
        /// <returns>Detailed dashboard data</returns>
        [HttpGet("dashboard/details")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> GetDashboardDetails(
            [FromQuery] string category,
            [FromQuery] string fromDate = null, 
            [FromQuery] string toDate = null)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentException("Category parameter is required"), 
                    "Category parameter cannot be null or empty"));
            }

            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Getting dashboard details for category {Category} from {FromDate} to {ToDate}", 
                    category, fromDate, toDate);
                
                var result = await _customerManagement.GetDashboardDetailsForCustomer(category, fromDate, toDate);
                return result;
            }, "Dashboard details retrieved successfully");
        }

        /// <summary>
        /// Gets all tracking details for customer with filtering options
        /// </summary>
        /// <param name="filters">Tracking filters</param>
        /// <returns>Filtered tracking details</returns>
        [HttpPost("tracking")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> GetAllTrackingDetails([FromBody] TrackingFilters filters)
        {
            if (filters == null)
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentNullException(nameof(filters)), 
                    "Tracking filters are required"));
            }

            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return validationResult;
            }

            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Getting tracking details with filters: {Filters}", 
                    System.Text.Json.JsonSerializer.Serialize(filters));
                
                var result = await _customerManagement.GetAllTrackingDetailsForCustomer(filters);
                return result;
            }, "Tracking details retrieved successfully");
        }

        /// <summary>
        /// Gets all vehicle categories available for vehicle requests
        /// </summary>
        /// <returns>List of vehicle categories</returns>
        [HttpGet("vehicle-categories")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> GetAllVehicleCategories()
        {
            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Getting all vehicle categories for vehicle requests");
                
                var result = await _customerManagement.GetAllVehicleCategoryForVehicleRequest();
                return result;
            }, "Vehicle categories retrieved successfully");
        }
    }
}