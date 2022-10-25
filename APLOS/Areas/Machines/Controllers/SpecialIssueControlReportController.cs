using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;
using Library.HumanResource.NewAttendanceProcess;

namespace Aplos.Areas.Machines.Controllers
{
    public class SpecialIssueControlReportController : BaseController
    {
        #region Constructor

        private readonly IAttendanceManagementService _AttendanceManagementService;
        ResudeceStatusReportService rsr = new ResudeceStatusReportService();
        private readonly ISqlRepository _sqlRepository;
        public SpecialIssueControlReportController(IAttendanceManagementService AttendanceManagementService, ISqlRepository R)
        {
            _AttendanceManagementService = AttendanceManagementService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region Special Issue Control Register    

        [Authorize, HttpGet]
        public JsonResult GetShiftList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select SystemID as Value,UserName as Text from ShiftDefination where IsActive=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSpecialIssueDetailsList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from SpecialIssueDetails";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSpecialIssueSummaryList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from SpecialIssueSummary";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadIssueItemDetailsList(string FromDate, string ToDate, string Shift)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct '' Id,SIC.SpecialIssueName,SII.SpecialIssueItem,format(SICU.Date,'dd-MMM-yyyy') as Date,
SICU.Shift,(select PeriodName from (select PeriodName,format(Time,'HH:mm:tt') as FromTime,isnull(lead(format(Time,'HH:mm:tt'))Over(order by Sequence),format(Time+2,'HH:mm:tt')) as ToTime from  MST.SpecialIssueDefinePeriod) P where format(SICU.Time,'HH:mm:tt') between P.FromTime and P.ToTime) Period,
format(SICU.Time,'hh:mm:tt') as Time,SII.SampleSize,SIUI.Value,SIUI.Remarks,SIUI.ConfidenceLevel 
from TRN.SpecialIssueControl SIC
left join TRN.SpecialIssueItem SII ON SII.SpecialIssueControlId=SIC.Id
left join TRN.SpecialIssueControlUpdate SICU ON SICU.IssueId=SIC.Id
left join TRN.SpecialIssueUpdateItem SIUI ON SIUI.ICUId=SICU.Id and SIUI.SICItemId=SII.Id
where 
format(SICU.Date,'dd-MMM-yyyy') between '" + FromDate + "' and '" + ToDate + "' and SICU.Shift='" + Shift + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadIssueItemSummaryList(string FromDate, string ToDate, string Shift)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select X.SpecialIssueName,X.SpecialIssueItem,Sum(X.SampleSize) as SampleSize,Sum(Value) as Value,convert(decimal(18,2),Sum(X.SampleSize)/Sum(Value)) as Percentage from (select distinct '' Id,SIC.SpecialIssueName,SII.SpecialIssueItem,format(SICU.Date, 'dd-MMM-yyyy') as Date,SICU.Shift,(select PeriodName from(select PeriodName, format(Time,'HH:mm:tt') as FromTime,isnull(lead(format(Time, 'HH:mm:tt'))Over(order by Sequence), format(Time + 2, 'HH:mm:tt')) as ToTime from MST.SpecialIssueDefinePeriod) P where format(SICU.Time, 'HH:mm:tt') between P.FromTime and P.ToTime) Period,format(SICU.Time, 'hh:mm:tt') as Time,SII.SampleSize,SIUI.Value,SIUI.Remarks,SIUI.ConfidenceLevel from TRN.SpecialIssueControl SIC left join TRN.SpecialIssueItem SII ON SII.SpecialIssueControlId = SIC.Id left join TRN.SpecialIssueControlUpdate SICU ON SICU.IssueId = SIC.Id left join TRN.SpecialIssueUpdateItem SIUI ON SIUI.ICUId = SICU.Id and SIUI.SICItemId = SII.Id where Date between '"+ FromDate + "' and '" + ToDate + "' and Shift = '" + Shift + "') X group by X.SpecialIssueItem,X.SpecialIssueName";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "SpecialIssueDetails";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (DataList != null)
                {
                    objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "", out dsProdBooked, false, "1");
                    DataView dv = new DataView(dsProdBooked.Tables[0]);
                    if(dv.Count > 0)
                    {
                        objCon.BeginTransaction();
                        objCon.executeQuery("delete from " + TableName + "");
                        objCon.CommitTransaction();

                    }
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    foreach (var item in DataList)
                    {
                        
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);
                        item["Id"] = "SID" + _Id;
                        item["UpdatedBy"] = identity.Name;
                        item["UpdatedDate"] = System.DateTime.Now.ToString();
                        item["UpdatedFromIP"] = identity.IPAddress;
                        AddNewRow(dsProdBooked.Tables[0], item);
                       
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateSummary(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "SpecialIssueSummary";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (DataList != null)
                {
                    objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "", out dsProdBooked, false, "1");
                    DataView dv = new DataView(dsProdBooked.Tables[0]);
                    if (dv.Count > 0)
                    {
                        objCon.BeginTransaction();
                        objCon.executeQuery("delete from " + TableName + "");
                        objCon.CommitTransaction();

                    }
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    foreach (var item in DataList)
                    {

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);
                        item["Id"] = "SIS" + _Id;
                        item["UpdatedBy"] = identity.Name;
                        item["UpdatedDate"] = System.DateTime.Now.ToString();
                        item["UpdatedFromIP"] = identity.IPAddress;
                        AddNewRow(dsProdBooked.Tables[0], item);

                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        #endregion
    }
}
