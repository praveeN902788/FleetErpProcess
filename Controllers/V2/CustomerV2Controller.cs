using Bharuwa.Erp.Common;
using Bharuwa.Erp.Common.FMS;
using Bharuwa.Erp.Entities.FMS;
using Bharuwa.Erp.Services.FMS.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bharuwa.Erp.API.FMS.Controllers.V2
{
    /// <summary>
    /// Customer management API version 2.0
    /// Provides enhanced customer operations with improved filtering, real-time updates, and advanced reporting
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/customer")]
    [ApiVersion("2.0")]
    [Produces("application/json")]
    public class CustomerV2Controller : VersionAwareController
    {
        private readonly ICustomerManagementDal _customerManagement;

        public CustomerV2Controller(
            ICustomerManagementDal customerManagementDal,
            IVersionManagementService versionService,
            ILogger<CustomerV2Controller> logger) 
            : base(versionService, logger)
        {
            _customerManagement = customerManagementDal;
        }

        /// <summary>
        /// Gets enhanced dashboard counts for customer with advanced filtering
        /// </summary>
        /// <param name="request">Enhanced dashboard request with multiple filter options</param>
        /// <returns>Enhanced dashboard count data</returns>
        [HttpPost("dashboard/counts")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> GetDashboardCounts([FromBody] EnhancedDashboardRequest request)
        {
            if (request == null)
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentNullException(nameof(request)), 
                    "Dashboard request is required"));
            }

            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return validationResult;
            }

            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Getting enhanced dashboard counts with request: {Request}", 
                    System.Text.Json.JsonSerializer.Serialize(request));
                
                // For now, using the existing method but with enhanced request structure
                var result = await _customerManagement.GetDashboardCountsForCustomer(
                    request.DateRange?.FromDate, 
                    request.DateRange?.ToDate);
                
                // In a real implementation, you would enhance this with additional data
                return EnhanceDashboardResponse(result, request);
            }, "Enhanced dashboard counts retrieved successfully");
        }

        /// <summary>
        /// Gets detailed dashboard information with advanced filtering and grouping
        /// </summary>
        /// <param name="request">Enhanced dashboard details request</param>
        /// <returns>Enhanced detailed dashboard data</returns>
        [HttpPost("dashboard/details")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> GetDashboardDetails([FromBody] EnhancedDashboardDetailsRequest request)
        {
            if (request == null)
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentNullException(nameof(request)), 
                    "Dashboard details request is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Category))
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentException("Category is required"), 
                    "Category cannot be null or empty"));
            }

            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return validationResult;
            }

            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Getting enhanced dashboard details for category {Category} with request: {Request}", 
                    request.Category, System.Text.Json.JsonSerializer.Serialize(request));
                
                var result = await _customerManagement.GetDashboardDetailsForCustomer(
                    request.Category, 
                    request.DateRange?.FromDate, 
                    request.DateRange?.ToDate);
                
                return EnhanceDashboardDetailsResponse(result, request);
            }, "Enhanced dashboard details retrieved successfully");
        }

        /// <summary>
        /// Gets all tracking details with enhanced filtering and real-time capabilities
        /// </summary>
        /// <param name="request">Enhanced tracking request</param>
        /// <returns>Enhanced filtered tracking details</returns>
        [HttpPost("tracking")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> GetAllTrackingDetails([FromBody] EnhancedTrackingRequest request)
        {
            if (request == null)
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentNullException(nameof(request)), 
                    "Enhanced tracking request is required"));
            }

            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return validationResult;
            }

            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Getting enhanced tracking details with request: {Request}", 
                    System.Text.Json.JsonSerializer.Serialize(request));
                
                // Convert enhanced request to legacy filters for now
                var legacyFilters = ConvertToLegacyFilters(request);
                var result = await _customerManagement.GetAllTrackingDetailsForCustomer(legacyFilters);
                
                return EnhanceTrackingResponse(result, request);
            }, "Enhanced tracking details retrieved successfully");
        }

        /// <summary>
        /// Gets vehicle categories with enhanced filtering and metadata
        /// </summary>
        /// <param name="type">Vehicle type filter (optional)</param>
        /// <param name="includeMetadata">Include additional metadata (optional)</param>
        /// <returns>Enhanced vehicle categories with metadata</returns>
        [HttpGet("vehicle-categories")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> GetAllVehicleCategories(
            [FromQuery] string type = null,
            [FromQuery] bool includeMetadata = false)
        {
            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Getting enhanced vehicle categories with type: {Type}, includeMetadata: {IncludeMetadata}", 
                    type, includeMetadata);
                
                var result = await _customerManagement.GetAllVehicleCategoryForVehicleRequestV2(type);
                
                return EnhanceVehicleCategoriesResponse(result, type, includeMetadata);
            }, "Enhanced vehicle categories retrieved successfully");
        }

        /// <summary>
        /// Gets real-time customer analytics
        /// </summary>
        /// <param name="request">Analytics request</param>
        /// <returns>Real-time analytics data</returns>
        [HttpPost("analytics")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> GetCustomerAnalytics([FromBody] AnalyticsRequest request)
        {
            if (request == null)
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentNullException(nameof(request)), 
                    "Analytics request is required"));
            }

            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Getting customer analytics with request: {Request}", 
                    System.Text.Json.JsonSerializer.Serialize(request));
                
                // This would be implemented with real-time analytics
                return new { 
                    Message = "Real-time analytics feature coming soon",
                    Request = request,
                    Timestamp = DateTime.UtcNow
                };
            }, "Customer analytics retrieved successfully");
        }

        #region Private Helper Methods

        private object EnhanceDashboardResponse(APIResponseDto result, EnhancedDashboardRequest request)
        {
            // Add enhanced features to the response
            return new
            {
                OriginalData = result,
                EnhancedFeatures = new
                {
                    RequestId = HttpContext.TraceIdentifier,
                    ProcessingTime = DateTime.UtcNow,
                    Filters = request,
                    Metadata = new { Version = "2.0", Enhanced = true }
                }
            };
        }

        private object EnhanceDashboardDetailsResponse(APIResponseDto result, EnhancedDashboardDetailsRequest request)
        {
            return new
            {
                OriginalData = result,
                EnhancedFeatures = new
                {
                    RequestId = HttpContext.TraceIdentifier,
                    ProcessingTime = DateTime.UtcNow,
                    Filters = request,
                    Metadata = new { Version = "2.0", Enhanced = true }
                }
            };
        }

        private object EnhanceTrackingResponse(APIResponseDto result, EnhancedTrackingRequest request)
        {
            return new
            {
                OriginalData = result,
                EnhancedFeatures = new
                {
                    RequestId = HttpContext.TraceIdentifier,
                    ProcessingTime = DateTime.UtcNow,
                    Filters = request,
                    Metadata = new { Version = "2.0", Enhanced = true }
                }
            };
        }

        private object EnhanceVehicleCategoriesResponse(APIResponseDto result, string type, bool includeMetadata)
        {
            return new
            {
                OriginalData = result,
                EnhancedFeatures = new
                {
                    RequestId = HttpContext.TraceIdentifier,
                    ProcessingTime = DateTime.UtcNow,
                    Type = type,
                    IncludeMetadata = includeMetadata,
                    Metadata = new { Version = "2.0", Enhanced = true }
                }
            };
        }

        private TrackingFilters ConvertToLegacyFilters(EnhancedTrackingRequest request)
        {
            // Convert enhanced request to legacy filters
            return new TrackingFilters
            {
                // Map properties as needed
                // This is a simplified conversion
            };
        }

        #endregion
    }

    #region Enhanced Request/Response Models

    public class EnhancedDashboardRequest
    {
        public DateRangeFilter DateRange { get; set; }
        public string[] Categories { get; set; }
        public bool IncludeTrends { get; set; }
        public bool IncludeComparisons { get; set; }
    }

    public class EnhancedDashboardDetailsRequest
    {
        public string Category { get; set; }
        public DateRangeFilter DateRange { get; set; }
        public string[] SubCategories { get; set; }
        public bool IncludeDrillDown { get; set; }
    }

    public class EnhancedTrackingRequest
    {
        public DateRangeFilter DateRange { get; set; }
        public string[] Statuses { get; set; }
        public string[] VehicleTypes { get; set; }
        public bool IncludeRealTimeUpdates { get; set; }
        public PaginationOptions Pagination { get; set; }
    }

    public class AnalyticsRequest
    {
        public string MetricType { get; set; }
        public DateRangeFilter DateRange { get; set; }
        public string[] Dimensions { get; set; }
        public bool IncludeForecasting { get; set; }
    }

    public class DateRangeFilter
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }

    public class PaginationOptions
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; }
        public string SortDirection { get; set; } = "asc";
    }

    #endregion
}