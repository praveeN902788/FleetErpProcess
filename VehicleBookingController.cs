using Bharuwa.Erp.API.FMS.Helpers;
using Bharuwa.Erp.Common;
using Bharuwa.Erp.Data;
using Bharuwa.Erp.Entities;
using Bharuwa.Erp.Entities.FMS;
using Bharuwa.Erp.Services.FMS.Interfaces;
using ERPServer.DataAccess.Manufacturing.General;
using ERPServer.DataAccess.Manufacturing.General.CheckListMasters;
using ERPServer.DataAccess.Manufacturing.Vehicles.VehicleBookingRequestRequests;
using ERPServer.DataAccess.Manufacturing.Vehicles.VehicleBookings;
using ERPServer.DataAccess.Manufacturing.Vehicles.VehicleCategory;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Bharuwa.Erp.API.FMS.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class VehicleBookingController(IFleetVehicleBooking iFleetVehicleBooking, IVehicleBookingService vehicleBooking, IDbContext dbContext, IHostEnvironment environment, ICheckListMasterDal checkListMasterDal, IVehicleCategoryDal divCon, IVehicleBookingRequestService vehicleBookingRequestService, IGeneralDal generalDal) : ApiBaseController
    {
        private readonly IFleetVehicleBooking _iFleetVehicleBooking = iFleetVehicleBooking;
        private readonly IVehicleBookingService _vehicleBookingService = vehicleBooking;
        private readonly IDbContext _dbContext = dbContext;
        private readonly IHostEnvironment _environment = environment;
        private readonly ICheckListMasterDal _checkListMasterDal = checkListMasterDal;
        private readonly IVehicleCategoryDal _divCon = divCon;
        private readonly IVehicleBookingRequestService _reqBooking = vehicleBookingRequestService;
        private readonly IGeneralDal _genDal = generalDal;

        [HttpPost("SaveVehicleBooking")]
        [ApiVersion("1.0")]
      public async Task<IActionResult> SaveVehicleBookingAsync(
     List<IFormFile> files,
     [FromForm][ModelBinder(typeof(FormDataJsonModelBinder))] VehicleBooking data)
        {
            try
            {
                if (files?.Count > 0)
                {
                    SaveFileInFolder saveFile = new SaveFileInFolder();

                    foreach (var item in data.ReferenceDocumentLinks)
                    {
                        var file = files.FirstOrDefault(a => a.FileName == item.DocumentFileName);
                        if (file != null)
                        {
                            item.DocumentFilePath = saveFile.GetSavedFilePath(
                                _environment.ContentRootPath,
                                "VehicleBooking",
                                file,
                                _dbContext.UserProfile.CompanyInfo.COMPANYID.ToString());
                        }
                    }

                    foreach (var item in data.VehicleBookingCheckLists)
                    {
                        var file = files.FirstOrDefault(a => a.FileName == item.FileName);
                        if (file != null)
                        {
                            item.FileName = saveFile.GetSavedFilePath(
                                _environment.ContentRootPath,
                                "VehicleBooking",
                                file,
                                _dbContext.UserProfile.CompanyInfo.COMPANYID.ToString());
                        }
                    }
                }

                data.ProdList ??= new List<VehicleBookingProdEntries>();
                data.RateFinalizeList ??= new List<ERPServer.DataAccess.Manufacturing.Vehicles.VehicleBookings.RateFinalizeList>();
                data.ReferenceDocumentLinks ??= new List<ReferenceDocumentLink>();
                data.AssignedStaff ??= new List<AssignedStaff>();

                FunctionResponse res = await _vehicleBookingService.saveVehicleBooking(data);

                APIResponseDto aPIResponseDto = new APIResponseDto
                {
                    StatusCode = res.status == "ok" ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                    Message = res.message,
                    Result = res.result
                };

                if (res.status == "ok")
                    return Ok(aPIResponseDto);

                return BadRequest(aPIResponseDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseDto
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Result = null
                });
            }
        }


        [HttpPost("SaveCheckList")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> SaveCheckList(
     List<IFormFile> files,
     [FromForm][ModelBinder(typeof(FormDataJsonModelBinder))] List<VehicleBookingCheckList> data)
        {
            try
            {
                if (files?.Count > 0 && data != null && data.Any())
                {
                    SaveFileInFolder saveFile = new SaveFileInFolder();

                    foreach (var item in data)
                    {
                        var file = files.FirstOrDefault(a => a.FileName == item.FileName);
                        if (file != null)
                        {
                            item.FileName = saveFile.GetSavedFilePath(
                                _environment.ContentRootPath,
                                "VehicleBooking",
                                file,
                                _dbContext.UserProfile.CompanyInfo.COMPANYID.ToString()
                            );
                        }
                    }
                }

                FunctionResponse res = await _vehicleBookingService.UpdateVehicleBookingCheckList(data);

                var response = new APIResponseDto
                {
                    StatusCode = res.status == "ok" ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                    Message = res.message,
                    Result = res.result
                };

                return res.status == "ok" ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseDto
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Result = null
                });
            }
        }



        [HttpGet("getActiveDriverMasterPagedList")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetActiveDriverMasterPagedListAsync()
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBooking.GetActiveDriverMasterPagedListAsync();
                return result;
            });
        }

        [HttpGet("getActiveDriverMasterPagedListWithCheckList")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetActiveDriverMasterPagedListWithCheckList(int DriverId)
        {
            return await ResponseWrapperAsync(async () =>
            {
                APIResponseDto result = await _iFleetVehicleBooking.GetActiveDriverMasterPagedListWithCheckList(DriverId);
                return result;
            });
        }

        [HttpGet("GetBookingEntryCheckList")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetBookingEntryCheckList(int Category)
        {
            return await ResponseWrapperAsync(async () =>
            {
                string tranName = "Vehicle Booking";
                FunctionResponse res = await _checkListMasterDal.GetCheckListByNameAndCategory(tranName, Category);

                APIResponseDto aPIResponseDto = new APIResponseDto
                {
                    StatusCode = res.status == "ok" ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                    Message = res.message,
                    Result = res.result
                };
                return aPIResponseDto;
            });
        }

        [HttpGet("VehicleCategories")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> VehicleCategories()
        {
            return await ResponseWrapperAsync(async () =>
            {
                FunctionResponse res = await _divCon.getAllActiveVehicleCategory();

                APIResponseDto aPIResponseDto = new APIResponseDto
                {
                    StatusCode = res.status == "ok" ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                    Message = res.message,
                    Result = res.result
                };

                return aPIResponseDto;
            });
        }

        [HttpGet("VehicleCategories")]
        [ApiVersion("2.0")]
        public async Task<IActionResult> VehicleCategoriesV2([FromQuery] string type)
        {
            return await ResponseWrapperAsync(async () =>
            {
                FunctionResponse res = await _divCon.GetAllActiveVehicleCategoryByFtlType(type);

                APIResponseDto aPIResponseDto = new APIResponseDto
                {
                    StatusCode = res.status == "ok" ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                    Message = res.message,
                    Result = res.result
                };

                return aPIResponseDto;
            });
        }

        [HttpPost("SaveVehicleBookingRequests")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> SaveVehicleBookingRequestsAsync([FromBody] VehicleBookingRequest data)
        {
            try
            {

                data.VoucherPrefix = "VBR";
                data.VoucherType = VoucherTypeEnum.VehicleBookingRequest;
                data.Username = _dbContext.UserProfile.username;
                data.UserId = _dbContext.UserProfile.Id.ToString();
                data.RateFinalizeList ??= new List<ERPServer.DataAccess.Manufacturing.Vehicles.VehicleBookingRequestRequests.RateFinalizeList>();
                data.OthersDetails ??= new List<OthersDetails>();

                FunctionResponse res = await _reqBooking.saveVehicleBookingRequest(data);

                APIResponseDto aPIResponseDto = new APIResponseDto
                {
                    StatusCode = res.status == "ok" ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                    Message = res.message,
                    Result = res.result
                };

                if (res.status == "ok")
                    return Ok(aPIResponseDto);

                return BadRequest(aPIResponseDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseDto
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Result = null
                });
            }
        }


        [HttpGet("GetEmployees")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetEmployees()
        {
            return await ResponseWrapperAsync(async () =>
            {
                var res = await _genDal.getEmployee();

                return res;
            });
        }


    }
}
