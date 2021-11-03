using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Attendances.Controllers
{
    public class RawDataDownloadController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;
       
        public RawDataDownloadController(
              IMaternityLeavePolicyService LeavePolicyService,
               IAttendanceManagementService AttendanceManagementService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _AttendanceManagementService = AttendanceManagementService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet]
        public ActionResult GetRawData(string WorkDate, string someText, ReportFormat reportFormat, bool istextformat)
        {
            try
            {
                if (istextformat)
                {
                  return  GetAttendanceRawDataReport(reportFormat, WorkDate);
                }
                else
                {
                    return GetRawDataFormat(WorkDate, someText);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        private string GetPadding(string iv,int SEED)
        {
            while (iv.Length < SEED)
            {
                iv = "0" + iv;
            }
            return iv;
        }
        public void GetRaw(out DataSet dsRef, string pmonth, string pyear,string WorkDate, string plantId,string CompanyId)
        {
            //string _date = "01-" + pmonth + "-" + pyear;
            //DateTime _lastdate = Convert.ToDateTime(_date).AddMonths(1).AddDays(-1);
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"      select a.DeviceID,format(a.pDate,'yyyyMMdd')as pDate ,format(a.PTime,'HHmm')PTime, e.EmployeeCode ,a.LogDownLoadNum,e.EmployeeCodeNumeric
                                    from [dbo].[AttdnRawData] a
                                   inner join EmployeeInformation as e on e.SystemId=a.LogDownLoadNum
                                WHERE a.PDate='" + WorkDate+ "' and e.PlantId='" + plantId + "' order by a.pDate ,a.PTime";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function

        #endregion -- Operations  


      
        public ActionResult GetAttendanceRawDataReport(ReportFormat reportFormat, string WorkDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetAttendanceRawDataReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId, identity.PlantName, WorkDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Attendance Raw Data Report";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        public ActionResult GetRawDataFormat(string WorkDate, string someText)
        {
            DataSet dsEmpInfo = null;
            clsReport objRpt = null;
            try
            {
                string month = Convert.ToDateTime(WorkDate).ToString("MMM");
                string year = Convert.ToDateTime(WorkDate).ToString("yyyy");

                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objRpt = new clsReport();

                GetRaw(out dsEmpInfo, month, year, WorkDate, identity.PlantId, identity.CompanyId);

                string attachment = "attachment; filename=" + someText + "Raw.txt";
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.ClearHeaders();
                System.Web.HttpContext.Current.Response.ClearContent();
                System.Web.HttpContext.Current.Response.AddHeader("content-disposition", attachment);
                System.Web.HttpContext.Current.Response.ContentType = "application/txt";
                StringBuilder builder = new StringBuilder();
                List<string> rows = new List<string>();



                var currentRow = string.Empty;
                for (int i = 0; i < dsEmpInfo.Tables[0].Rows.Count; i++)
                {
                    var EmployeeCode = dsEmpInfo.Tables[0].Rows[i]["EmployeeCodeNumeric"].ToString();
                    var Device = dsEmpInfo.Tables[0].Rows[i]["DeviceID"].ToString();
                    //var DeviceID = dsEmpInfo.Tables[0].Rows[i]["DeviceID"].ToString();
                    var pd = dsEmpInfo.Tables[0].Rows[i]["pdate"].ToString();
                    var pt = dsEmpInfo.Tables[0].Rows[i]["ptime"].ToString();
                    string EmployeeCodePadded = GetPadding(EmployeeCode, 11);
                    string DevicePadded = GetPadding(Device, 3);

                    currentRow = DevicePadded + pd + pt + "01" + EmployeeCodePadded;

                    rows.Add(string.Join("", currentRow));
                }


                builder.Append(string.Join(Environment.NewLine, rows.ToArray()));
                Response.Write(builder.ToString());
                Response.End();
                return Json(new { FileName = attachment, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}