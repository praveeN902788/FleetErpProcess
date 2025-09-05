using Bharuwa.Erp.API.FMS.Helpers;
using Bharuwa.Erp.Common;
using Bharuwa.Erp.Data;
using Bharuwa.Erp.Entities;
using Bharuwa.Erp.Entities.FMS;
using Bharuwa.Erp.Services.FMS.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bharuwa.Erp.API.FMS.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ExpensesController(IExpensesDal iExpensesDal, IHostEnvironment environment, IDbContext dbContext) : ApiBaseController
    {
        private readonly IExpensesDal _iExpensesDal = iExpensesDal;

        private readonly IHostEnvironment _environment = environment;

        private readonly IDbContext _dbContext = dbContext;

        [HttpPost("AddExpenses")]
        [ApiVersion("1.0")]
        public async Task<IActionResult> AddExpensesAsync(List<IFormFile> file, [FromForm] ExpensesRequest expenses)
        {
            return await ResponseWrapperAsync(async () =>
            {
                var referenceDocuments = new List<ReferenceDocumentLink>();
                var savedFilePaths = new List<string>();

                if (file?.Count > 0)
                {
                    SaveFileInFolder saveFile = new SaveFileInFolder();

                    foreach (var uploadedFile in file)
                    {
                        string folder;
                        string fileExtension = Path.GetExtension(uploadedFile.FileName)?.ToLower();
                        if (fileExtension == ".mp4" || fileExtension == ".avi" || fileExtension == ".mkv")
                        {
                            folder = "FleetExpenses/Videos";
                        }
                        else
                        {
                            folder = "FleetExpenses/Documents";
                        }

                        var savedFilePath = saveFile.GetSavedFilePath(
                            _environment.ContentRootPath,
                            folder,
                            uploadedFile,
                            _dbContext.UserProfile.CompanyInfo.COMPANYID.ToString()
                        );

                        referenceDocuments.Add(new ReferenceDocumentLink
                        {
                            DocumentFileName = uploadedFile.FileName,
                            DocumentFilePath = savedFilePath
                        });
                    }
                }

                var newExpense = new Expenses
                {
                    ExpenseId = expenses.ExpenseId,
                    ExpenseName = expenses.ExpenseName,
                    ExpenseCode = expenses.ExpenseCode,
                    ExpenseAmount = expenses.ExpenseAmount,
                    VehicleBookingCode = expenses.VehicleBookingCode,
                    VehicleId = expenses.VehicleId,
                    Latitude = expenses.Latitude,
                    Longitude = expenses.Longitude,
                    ReferenceDocumentLinks = referenceDocuments
                };

                APIResponseDto result = await _iExpensesDal.AddExpensesAsync(newExpense);
                return result;
            });
        }

    }
}
