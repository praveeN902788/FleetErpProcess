using Bharuwa.Erp.Common;
using Bharuwa.Erp.Data;
using ERPServer.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace Bharuwa.Erp.API.FMS
{
    public class BasicController(IDbContext dbContext, LoginHelper login) : ApiBaseController
    {
        private IDbContext _dbContext = dbContext;
        private LoginHelper _login = login;

        [HttpGet]
        [Route("api/v1/authConfiguration")]
        public async Task<IActionResult> V1configureAuthentication()
        {

            try
            {
                if (_dbContext is null)
                {
                    return Unauthorized();
                }
                return new OkObjectResult(new FunctionResponse { status = "ok", result = new { userProfile = _dbContext.UserProfile, systemsettings = _dbContext.Settings, userMenus = await _login.getuserMenus(_dbContext.UserProfile.UserType, _dbContext.UserProfile.UserLevel) } });

            }
            catch (Exception ex)
            {

                return new BadRequestObjectResult(ex.Message);
            }

        }

    }
}
