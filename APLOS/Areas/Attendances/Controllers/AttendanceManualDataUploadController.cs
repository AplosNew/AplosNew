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
    public class AttendanceManualDataUploadController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IAttendanceManagementService _AttendanceManagementService;

        public AttendanceManualDataUploadController(
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


        [HttpGet, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat, string date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsManulAttendanceUpload r = new clsManulAttendanceUpload(identity, _sqlRepository);
            IWorkbook workbook = r.GetSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, date);
            var reportFileName = "Attendance Manual Data upload Sample File";
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
            clsManualAttendanceUploadUtility objR = null;
            try
            {
                objR = new clsManualAttendanceUploadUtility();
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
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
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
        private DataSet ReadExcelToTable(string path)
        {

            //Connection String

            //string connstring = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties='Excel 8.0;HDR=NO;IMEX=1';";
            //the same name 
            string connstring = "Provider = Microsoft.JET.OLEDB.4.0; Data Source = " + path + "; Extended Properties = 'Excel 8.0;HDR=NO;IMEX=1'; ";

            using (OleDbConnection conn = new OleDbConnection(connstring))
            {
                conn.Open();
                //Get All Sheets Name
                DataTable sheetsName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "Table" });

                //Get the First Sheet Name
                string firstSheetName = sheetsName.Rows[0][2].ToString();
                firstSheetName = "Sheet1$";
                //Query String 
                string sql = string.Format("SELECT * FROM [{0}]", firstSheetName);
                OleDbDataAdapter ada = new OleDbDataAdapter(sql, connstring);
                DataSet set = new DataSet();
                ada.Fill(set);
                return set;
            }
        }

        [HttpPost]
        public ActionResult SaveAttendanceManualData(List<AttendanceManualData> _listUI,string fromDate,string toDate)
        {
            List<AttendanceProcessData> _finalList = new List<AttendanceProcessData>();
            RT _rt = new RT();
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsManulAttendanceUpload obj = new clsManulAttendanceUpload(identity,_sqlRepository);
                obj.CreateFinalList(_listUI, fromDate, toDate,out _finalList);
                if(_finalList.Count>0)
                {
              /// _rt=  obj.Save(_finalList);
                }
                if(_rt.IsError)
                {
                    //return Json(new { Error = true, Message = "Error occured", Data = DataToBeSaved }, JsonRequestBehavior.AllowGet);
                    return Json(new { Message = _rt.msg, Error = true, Data = _rt.data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = false, Message = _rt.msg, Data = _rt.data }, JsonRequestBehavior.AllowGet);
                    //return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception ex)
            {                
                return Json(new { Message = ex.Message, Error = true, Data = _rt.data }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                
            }           
        }

    }
}