using Bharuwa.Erp.Common;
using Bharuwa.Erp.Common.FMS;
using Bharuwa.Erp.Common.PMS;
using Bharuwa.Erp.Entities.FMS;
using Bharuwa.Erp.Services.EDI_Integrations;
using Bharuwa.Erp.Services.FMS;
using Bharuwa.Erp.Services.FMS.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Bharuwa.Erp.API.FMS.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PdfController(IPdfService pdfService) : ApiBaseController
    {
        private readonly IPdfService _pdfService = pdfService;

        //[HttpGet("customer/{id}")]
        //public async Task<IActionResult> GenerateCustomerPdf(int id)
        //{
        //    try
        //    {

        //        var pdfBytes =  _pdfService.GenerateConsignmentNotePdf(id);

        //        return File(pdfBytes, "application/pdf", $"Customer_{id}_{DateTime.Now:yyyyMMdd}.pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Failed to generate PDF: {ex.Message}");
        //    }
        //}
    }
    
}
