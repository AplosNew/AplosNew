#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System;
using Syncfusion.XlsIO;
using Library.Data.Sql;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class WIPReportController : BaseController
    {
        #region Constructor
        /// <summary>   The ProductionStatusService service. </summary>

        SqlRepository _sqlRepository;
        public WIPReportController()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region Operation
        [HttpGet, Authorize]
        public ActionResult GetWipReport(string Date)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = identity.PlantId + "WIP" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                //string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();
                var workbook = _wipReport.GetWIPReportLineWiseNew(identity.CompanyId, identity.PlantId, Date);
                workbook.Version = ExcelVersion.Excel2013;
                //workbook.SaveAs(fullPath);

                workbook.SaveAs(fileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }
        [HttpGet, Authorize]
        public ActionResult GetWipReportPivot(string Date)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = identity.PlantId + "WIP" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                //string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();
                var workbook = _wipReport.GetWIPReportLineWiseNewPivot(identity.CompanyId, identity.PlantId, Date);
                workbook.Version = ExcelVersion.Excel2013;
                //workbook.SaveAs(fullPath);

                workbook.SaveAs(fileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }
        [HttpGet, Authorize]
        public ActionResult GetWipReportProcessWise(string ProcessId, string Date)
        {
            try
            {
                if (string.IsNullOrEmpty(ProcessId) || ProcessId == "null")
                    throw new Exception("Please select Process");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = identity.PlantId + "WIP" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                //string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();
                var workbook = _wipReport.GetWIPReportLineWiseNew(identity.CompanyId, identity.PlantId, Date);
                //_wipReport.GetWIPReportProcessWise(ProcessId, Date);


                workbook.Version = ExcelVersion.Excel2013;
                //workbook.SaveAs(fullPath);

                workbook.SaveAs(fileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetProcessList()
        {
            try
            {

                return Json(_sqlRepository.GetDataCollection(@"SELECT p.Id,p.UserName
                            FROM hkp.Process AS p WHERE p.IsProductionProcess=1 AND p.[Active]=1 ORDER BY p.Sequence"), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }
        #endregion

    }
}