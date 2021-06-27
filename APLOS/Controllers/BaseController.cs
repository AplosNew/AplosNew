using Aplos.Filters;
using Aplos.Helpers;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    [CustomActionFilter, CustomHandleError, CustomAuthorize]
    public class BaseController : Controller
    {
        protected static JsonResult CustomJson(object data)
        {
            return new CustomJsonResult { Data = data };
        }

        public ActionResult RenderReportAsPdf(IWorkbook workbook, string fileName, bool isOpen = true)
        {
            try
            {
                using (var converter = new ExcelToPdfConverter(workbook))
                {
                    var pdfDocument = new PdfDocument();
                    ExcelToPdfConverterSettings _settings = new ExcelToPdfConverterSettings();
                    _settings.AutoDetectComplexScript = true;
                    _settings.EmbedFonts = true;
                    _settings.LayoutOptions = LayoutOptions.FitAllColumnsOnOnePage;

                    pdfDocument = converter.Convert(_settings);

                    if (isOpen == true)
                        pdfDocument.Save(fileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Open);
                    else
                        pdfDocument.Save(fileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);

                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult RenderReportAsExcel(IWorkbook workbook, string fileName)
        {
            workbook.SaveAs(fileName + ".xls", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
        public ActionResult RenderReportAsExcelx(IWorkbook workbook, string fileName)
        {
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
    }
}