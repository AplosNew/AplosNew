#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Biometrics;
using System.Collections.Generic;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Model.Attendances;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Web.Script.Serialization;
using System;
using clsAttendance;
using Library.Data.Sql;
using System.IO;
using Library.Data;
using Library.Service.Helpers;
using Newtonsoft.Json;
using System.Data.OleDb;
using Syncfusion.XlsIO;
using System.Text.RegularExpressions;
using System.Globalization;
using Library.Model.Enums;
using Library.Service.HumanResources;
using Library.HumanResource.Attendance.Manual;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class ManualAttendanceFileUploadController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IAttendanceManagementService _AttendanceManagementService;

        public ManualAttendanceFileUploadController(
               ISqlRepository sqlRepository,
               IAttendanceManagementService AttendanceManagementService

            )
        {

            _sqlRepository = sqlRepository;
            _AttendanceManagementService = AttendanceManagementService;

        }
        #endregion


        public ActionResult Aplos()
        {
            return View();
        }

        #region --GET--
        [HttpGet, Authorize]
        public ActionResult GetMaster()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsManualAttendanceFileUpload ep = new clsManualAttendanceFileUpload();
                return Json(ep.GetMaster(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat, string date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsManualAttendanceFileUpload r = new clsManualAttendanceFileUpload();
            IWorkbook workbook = r.GetSampleFile(identity.Name);
            var reportFileName = "Manual Attendance File Upload Sample File";
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
        #endregion
        [HttpPost]
        public JsonResult Create(FormCollection form)
        {
            var pre = form["ManualAttdnFile"];
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var Manualfile = JsonConvert.DeserializeObject<ManualAttdnFile>(pre, settings);
            var file = Request.Files["file"];
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() != ".xls" && extension.ToLower() != ".xlsx")
                {
                    throw new CustomException(Resources.ImageUploadError);
                }


                clsManualAttendanceFileUpload p = new clsManualAttendanceFileUpload();
                p.Save(file.FileName, extension, Manualfile, out DataSet dsMaster);
                var path = Path.Combine(ResourcesPathReader.GetManualAttendanceFilePath(), dsMaster.Tables[0].Rows[0]["FileId"].ToString());

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
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult DeleteMaster(string Id, string File)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sql = "SELECT * FROM [dbo].[ManualAttdnFile] WHERE Id='" + Id +"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows[0]["FileStatus"].ToString().ToUpper() == "UPLOADED")
                {
                    
                        ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                        con.BeginTransaction();
                        con.executeQuery("delete from ManualAttdnFile where Id='" + Id + "'");
                        con.CommitTransaction();

                        var path = Path.Combine(ResourcesPathReader.GetManualAttendanceFilePath(), File);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                            //file.SaveAs(path);
                        }
                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = "Already In process..!" }, JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}