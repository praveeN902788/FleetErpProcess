using Bharuwa.Erp.API.FMS.Helpers;
using Bharuwa.Erp.Common;
using Bharuwa.Erp.Data;
using Bharuwa.Erp.Entities;
using Bharuwa.Erp.Entities.DailyCheckList;
using Bharuwa.Erp.Entities.DriverDailyCheckList;
using Bharuwa.Erp.Entities.FMS;
using Bharuwa.Erp.Queries;
using Bharuwa.Erp.Services.FMS.Interfaces;
using Dapper;
using ERPServer.DataAccess.Manufacturing.Transactions;
using ERPServer.DataAccess.Manufacturing.Vehicles.DailyCheckList.Interfaces;
using ImsPosLibraryCore.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Dynamic;

namespace Bharuwa.Erp.API.FMS.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class DailyCheckListController(IFleetVehicleBookingRequest iFleetVehicleBookingRequest, IDbContext dbContext, IHostEnvironment environment, IVehicleDailyCheckListService vehicleDailyCheckListService, IDriverDailyCheckListService driverDailyCheckListService) : ApiBaseController
    {
        private readonly IFleetVehicleBookingRequest _iFleetVehicleBookingRequest = iFleetVehicleBookingRequest;
        private readonly IDbContext _dbContext = dbContext;
        private readonly IHostEnvironment _environment = environment;
        private readonly IVehicleDailyCheckListService _dal = vehicleDailyCheckListService;
        private readonly IDriverDailyCheckListService _dals = driverDailyCheckListService;

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

        [HttpGet("GetVoucherNo/{VoucherName}")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> GetVoucherNo(string VoucherName)
        {
            return await ResponseWrapperAsync(async () =>
            {
                dynamic param = new ExpandoObject();
                string devicemapQuery = @"
            SELECT * 
            FROM (
                SELECT 
                    ISNULL(VS.Id,0) AS Id,
                    ISNULL(P.NAME,'') AS PlantName,
                    ISNULL(D.DeviceName,'') AS DeviceName,
                    VT.VoucherName,
                    ISNULL(VT.VoucherPrefix,'') AS DefaultVoucherPrefix,
                    ISNULL(VT.VoucherSuffix,'') AS DefaultVoucherSuffix,
                    ISNULL(VS.VoucherPrefix, ISNULL(VT.VoucherPrefix,'')) AS VoucherPrefix,
                    ISNULL(VS.VoucherSuffix, ISNULL(VT.VoucherSuffix,'')) AS VoucherSuffix,
                    ISNULL(VS.VoucherTypeId, VT.Id) AS VoucherTypeId,
                    VS.PlantId,
                    VS.DeviceId,
                    D.DeviceCode
                FROM VoucherType VT
                LEFT JOIN VoucherSeries VS
                    ON VT.Id = VS.VoucherTypeId 
                    AND VS.PlantId = 1 
                    AND VS.DeviceId = 1
                LEFT JOIN Plants P
                    ON VS.PlantId = P.ID
                LEFT JOIN Devices D
                    ON VS.DeviceId = D.Id 
                    AND VS.PlantId = D.PlantId
                WHERE VT.Id = @Id
            ) A 
            ORDER BY VoucherTypeId";

                param.DeviceMapVoucherSeriesObject = await _dbContext
                    .userConnection()
                    .QueryFirstOrDefaultAsync<DeviceMapVoucherSeries>(devicemapQuery, new { Id = VoucherName });
                param.Mode = "New";
                param.VchrNo = null;
                param.Guid = Guid.NewGuid().ToString();
                param.VoucherType = param.DeviceMapVoucherSeriesObject?.VoucherTypeId ?? 0;
                param.DeviceCode = _dbContext.UserProfile.DeviceCode;
                param.DeviceId = _dbContext.UserProfile.DeviceId;
                param.Division = _dbContext.UserProfile.userDivision;
                param.Mwarehouse = "Main Warehouse";
                param.PhiscalId = _dbContext.UserProfile.CompanyInfo.PhiscalID;
                param.PlantId = _dbContext.UserProfile.CurrentPlantId;
                param.VoucherName = param.DeviceMapVoucherSeriesObject?.VoucherName ?? "";
                param.VoucherPrefix = param.DeviceMapVoucherSeriesObject?.VoucherPrefix ?? "";
                JsonSerializerSettings jss = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                TrnMainBaseModel trnMain = JsonConvert.DeserializeObject<TrnMainBaseModel>(
                    JsonConvert.SerializeObject(param, jss), jss
                );
                int curno = await getVoucherNo(trnMain);
                string Vchrno = $"{trnMain.VoucherPrefix}{curno}{_dbContext.UserProfile.DeviceCode}{_dbContext.UserProfile.CompanyInfo.PhiscalID.Replace("/", "")}";
                return new
                {
                    CurrentNo = curno,
                    VoucherNo = Vchrno
                };
            });
        }

        private async Task<int> getVoucherNo(TrnMainBaseModel trnMain)
        {
            using (var connection = _dbContext.userConnection())
            {
                 connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int curno = await connection.ExecuteScalarAsync<int>(
                            QueryStore.QueryString(DbQueryEnumType.GetVoucherNo),
                            new
                            {
                                CompanyId = _dbContext.UserProfile.CompanyInfo.COMPANYID,
                                Division = trnMain.DIVISION,
                                VoucherType = trnMain.VoucherType,
                                PhiscalId = trnMain.PhiscalID,
                                VoucherPrefix = trnMain.VoucherPrefix,
                                PlantId = _dbContext.UserProfile.CurrentPlantId,
                                DeviceId = _dbContext.UserProfile.DeviceId,
                                DeviceCode = _dbContext.UserProfile.DeviceCode
                            },
                            transaction: transaction
                        );

                        if (curno == 0)
                        {
                            curno = await connection.ExecuteScalarAsync<int>(
                                QueryStore.QueryString(DbQueryEnumType.InitialiseVoucherNo),
                                new
                                {
                                    CompanyId = _dbContext.UserProfile.CompanyInfo.COMPANYID,
                                    Division = trnMain.DIVISION,
                                    VoucherType = trnMain.VoucherType,
                                    PhiscalId = trnMain.PhiscalID,
                                    CreatedBy = _dbContext.UserName,
                                    VoucherPrefix = trnMain.VoucherPrefix,
                                    PlantId = _dbContext.UserProfile.CurrentPlantId,
                                    DeviceId = _dbContext.UserProfile.DeviceId,
                                    DeviceCode = _dbContext.UserProfile.DeviceCode
                                },
                                transaction: transaction
                            );
                        }
                        transaction.Commit();
                        return curno;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        [HttpPost("api/Fleet/SaveVehicleDailyCheckList")]
        [ApiVersion("1.0")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveVehicleDailyCheckList(List<IFormFile> files, [FromForm][ModelBinder(typeof(FormDataJsonModelBinder))] VehicleDailyCheckList data)
        {
            ModelState.Clear();
            return await ResponseWrapperAsync(async () =>
            {
                try
                {
                    data.VoucherType = VoucherTypeEnum.VehicleDailyCheckList;
                    data.VoucherPrefix = "VDC";
                    data.CreationFrom = "Device";
                    if (files?.Count > 0 && data.DailyVehicleCheckLists?.Count > 0)
                    {
                        SaveFileInFolder saveFile = new SaveFileInFolder();
                        int fileIndex = 0;

                        foreach (var checklist in data.DailyVehicleCheckLists)
                        {
                            if (checklist.Attachments?.Count > 0)
                            {
                                var newAttachments = new List<VehicleCheckListAttachment>();

                                foreach (var attachment in checklist.Attachments)
                                {
                                    if (fileIndex < files.Count)
                                    {
                                        var file = files[fileIndex];
                                        if (file.Length > 10 * 1024 * 1024) // 10MB limit
                                        {
                                            throw new Exception($"File {file.FileName} exceeds maximum size of 10MB");
                                        }
                                        string folder;
                                        var fileExtension = Path.GetExtension(file.FileName).ToLower();
                                        var contentType = file.ContentType.ToLower();

                                        if (contentType.StartsWith("video/") ||
                                            fileExtension == ".mp4" || fileExtension == ".avi" || fileExtension == ".mkv")
                                        {
                                            folder = "VehicleDailyCheckList/Videos";
                                        }
                                        else if (contentType.StartsWith("image/"))
                                        {
                                            folder = "VehicleDailyCheckList/Images";
                                        }
                                        else
                                        {
                                            folder = "VehicleDailyCheckList/Documents";
                                        }
                                        var newAttachment = new VehicleCheckListAttachment
                                        {
                                            FileName = file.FileName,
                                            FileType = file.ContentType,
                                            FileSize = file.Length,
                                            CreatedOn = DateTime.Now,
                                            FilePath = saveFile.GetSavedFilePath(_environment.ContentRootPath, folder, file, _dbContext.UserProfile.CompanyInfo.COMPANYID.ToString()),
                                            FileData = await ConvertToByteArrayAsync(file)
                                        };

                                        newAttachments.Add(newAttachment);
                                        fileIndex++;
                                    }
                                    else if (!attachment.IsNewFile)
                                    {
                                        newAttachments.Add(attachment);
                                    }
                                }

                                checklist.Attachments = newAttachments;
                            }
                        }
                    }

                    FunctionResponse res = await _dal.SaveVehicleDailyCheckList(data);
                    return res;
                }
                catch (Exception ex)
                {
                    return new FunctionResponse
                    {
                        status = "error",
                        message = $"Error processing attachments: {ex.Message}"
                    };
                }
            });
        }


        private async Task<byte[]> ConvertToByteArrayAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        [HttpPost("api/Fleet/SaveDriverDailyCheckList")]
        [ApiVersion("1.0")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveDriverDailyCheckList(List<IFormFile> files, [FromForm][ModelBinder(typeof(FormDataJsonModelBinder))] DriverDailyCheckList data)
        {
            ModelState.Clear();

            return await ResponseWrapperAsync(async () =>
            {
                try
                {
                    data.VoucherType = VoucherTypeEnum.VehicleDailyDriverChecklist;
                    data.VoucherPrefix = "VDD";
                    data.CreationFrom = "Device";
                    if (files?.Count > 0 && data.DailyDriverCheckLists?.Count > 0)
                    {
                        SaveFileInFolder saveFile = new SaveFileInFolder();
                        int fileIndex = 0;

                        foreach (var checklist in data.DailyDriverCheckLists)
                        {
                            if (checklist.Attachments?.Count > 0)
                            {
                                var newAttachments = new List<DriverCheckListAttachment>();

                                foreach (var attachment in checklist.Attachments)
                                {
                                    if (fileIndex < files.Count)
                                    {
                                        var file = files[fileIndex];
                                        if (file.Length > 10 * 1024 * 1024) // 10MB limit
                                        {
                                            throw new Exception($"File {file.FileName} exceeds maximum size of 10MB");
                                        }
                                        string folder;
                                        var fileExtension = Path.GetExtension(file.FileName).ToLower();
                                        var contentType = file.ContentType.ToLower();

                                        if (contentType.StartsWith("video/") ||
                                            fileExtension == ".mp4" || fileExtension == ".avi" || fileExtension == ".mkv")
                                        {
                                            folder = "DriverDailyCheckList/Videos";
                                        }
                                        else if (contentType.StartsWith("image/"))
                                        {
                                            folder = "DriverDailyCheckList/Images";
                                        }
                                        else
                                        {
                                            folder = "DriverDailyCheckList/Documents";
                                        }
                                        var newAttachment = new DriverCheckListAttachment
                                        {
                                            FileName = file.FileName,
                                            FileType = file.ContentType,
                                            FileSize = file.Length,
                                            CreatedOn = DateTime.Now,
                                            FilePath = saveFile.GetSavedFilePath(_environment.ContentRootPath, folder, file, _dbContext.UserProfile.CompanyInfo.COMPANYID.ToString()),
                                            FileData = await ConvertToByteArrayAsync(file)
                                        };

                                        newAttachments.Add(newAttachment);
                                        fileIndex++;
                                    }
                                    else if (!attachment.IsNewFile)
                                    {
                                        newAttachments.Add(attachment);
                                    }
                                }

                                checklist.Attachments = newAttachments;
                            }
                        }
                    }

                    FunctionResponse res = await _dals.SaveDriverDailyCheckList(data);
                    return res;
                }
                catch (Exception ex)
                {
                    return new FunctionResponse
                    {
                        status = "error",
                        message = $"Error processing attachments: {ex.Message}"
                    };
                }
            });
        }


    }
}
