using Bharuwa.Erp.Common;
using Bharuwa.Erp.Services.FMS.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bharuwa.Erp.API.FMS.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class FleetNotificationController(IFleetNotification iFleetNotification) : ApiBaseController
    {
        private readonly IFleetNotification _iFleetNotification = iFleetNotification;

        [HttpPost("MarkNotificationAsRead")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> MarkNotificationAsReadAsync([FromQuery] int notificationId)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetNotification.MarkNotificationAsReadAsync(notificationId);
                return result;
            });
        }

        [HttpPost("MarkNotificationAsReadAsyncByDriver")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> MarkNotificationAsReadAsyncByDriver([FromQuery] int notificationId)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetNotification.MarkNotificationAsReadAsyncByDriver(notificationId);
                return result;
            });
        }

        [HttpGet("GetAllNotificationsList")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllNotificationsListAsync()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetNotification.GetAllNotificationsAsync();
                return result;
            });
        }

        [HttpGet("GetAllCustomerNotificationsList")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetAllCustomerNotificationsListAsync([FromQuery] string MobileNumber)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetNotification.GetAllCustomerNotificationsAsync(MobileNumber);
                return result;
            });
        }
    }
}
