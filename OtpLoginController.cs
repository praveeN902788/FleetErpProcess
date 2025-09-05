using Bharuwa.Erp.API.FMS.Helpers;
using Bharuwa.Erp.Common;
using Bharuwa.Erp.Common.FMS;
using Bharuwa.Erp.Common.PMS;
using Bharuwa.Erp.Data;
using Bharuwa.Erp.Entities.FMS;
using Bharuwa.Erp.Services.FMS;
using Bharuwa.Erp.Services.FMS.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;

namespace Bharuwa.Erp.API.FMS.Controllers
{
   // [AllowAnonymous]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class OtpLoginController(IOtpLoginDal otpLoginDal, IHostEnvironment environment, IDbContext dbContext) : ApiBaseController
    {
        private readonly IOtpLoginDal _otpLogin = otpLoginDal;
        private readonly IHostEnvironment _environment = environment;

        private readonly IDbContext _dbContext = dbContext;

        [AllowAnonymous]
        [HttpPost("GenerateMobileOTP")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GenerateMobileOTP([FromBody] FleetOtp login)
        {

            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _otpLogin.GenerateMobileOTP(login);
                return result;
            });
        }

        [AllowAnonymous]
        [HttpPost("LoginWithOTP")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> MarkAsReadMobileOTP([FromBody] FleetOtp login)
        {

            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _otpLogin.MarkAsReadMobileOTP(login);
                return result;
            });
        }
        [AllowAnonymous]
        [HttpPost("UserRegistration")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> UserRegistration([FromBody] RegisterUser userMaster)
        {

            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _otpLogin.UserRegistration(userMaster);
                return result;
            });
        }

        [AllowAnonymous]
        [HttpPost("UserRegistration")]
        [ApiVersion("2.0")]
        public async Task<IActionResult> UserRegistration(IFormFile file, [FromForm] RegisterUser userMaster)
        {

            return await ResponseWrapperAsync(async () =>
            {
                if (file != null)
                {
                    SaveFileInFolder saveFile = new();
                    userMaster.ProfileImage = saveFile.GetSavedFilePath(_environment.ContentRootPath, "UserProfile", file, _dbContext.userConnection().Database);
                }
                APIResponseDto result = await _otpLogin.UserRegistration(userMaster);
                return result;
            });
        }

        [HttpPost("EditUserProfile")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> EditUserProfile(IFormFile? file, [FromForm] RegisterUser userMaster)
        {

            return await ResponseWrapperAsync(async () =>
            {
                if (file != null)
                {
                    SaveFileInFolder saveFile = new();
                    userMaster.ProfileImage = saveFile.GetSavedFilePath(_environment.ContentRootPath, "UserProfile", file, _dbContext.userConnection().Database);
                }
                APIResponseDto result = await _otpLogin.EditUserProfile(userMaster);
                return result;
            });
        }

    }
}
