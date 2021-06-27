#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Aplos.Controllers;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.OrderManagement.OrderControl;
using System;
using System.IO;
using Library.Data;
using Library.Service.Helpers;
using System.Collections.Generic;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class BOQUploadController : BaseController
    {
        #region Constructor
        /// <summary>   The skillCategoryService service. </summary>
        private readonly IDestinationService _skillCategoryService;
        private readonly ICompanyGroupDestinationService _companyGroupDestinationService;

        public BOQUploadController(IDestinationService skillCategoryService, ICompanyGroupDestinationService companyGroupDestinationService)
        {
            _skillCategoryService = skillCategoryService;
            _companyGroupDestinationService = companyGroupDestinationService;
        }
        #endregion

        #region -- Pages
       
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsBOQUpload r = new clsBOQUpload();
            IWorkbook workbook = r.GetSampleFile(identity.Name,identity.CompanyId,identity.PlantId);
            var reportFileName = "BOQ upload Sample File";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        }

        [HttpPost, Authorize]
        public JsonResult ImportData()
        {
            string path;
            clsBOQUpload objR = null;
            try
            {
                objR = new clsBOQUpload();
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFile(out path);
                var data = objR.ReadData(identity.PlantId, path);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public void SaveFile(out string path)
        {
            path = "";
            try
            {
                var file = Request.Files["file"];
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {
                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetBOQUploadData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost, Authorize]
        public JsonResult Save(List<BOQData> BOQData)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //BP.AddedBy = identity.Name;
                //BP.AddedFromIP = identity.Name;
                clsBOQUpload p = new clsBOQUpload();
                p.SaveMaster(BOQData);
                return Json(new { Error = false, Data = BOQData, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion
    }
}