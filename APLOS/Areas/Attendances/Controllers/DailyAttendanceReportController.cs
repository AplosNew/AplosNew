#region Using
using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.HumanResource.Attendance;
using Library.HumanResource.Payroll.Allowance;
using Library.Model.Employees;
using Library.Model.Setups;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class DailyAttendanceReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        private readonly IManpowerAttendanceSummary _manpowerAttendanceSummary;
        private IRepositoryAsync<MailReceiverDetail> mailReceiverDetailRepository;

        public DailyAttendanceReportController(
              IStoppageService stoppageService,
              ISqlRepository sqlRepository,
              IManpowerAttendanceSummary manpowerAttendanceSummary
            )
        {
            _stoppageService = stoppageService;
            _sqlRepository = sqlRepository;
            _manpowerAttendanceSummary = manpowerAttendanceSummary;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region --Page Loading Get Function--

        [HttpGet, Authorize]
        public JsonResult GetEmpDataa()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string employeeId = string.Empty;
            employeeId = identity.EmployeeId;
            var str = @"
                        select m.ROBudgetCode BudgetCode,m.Id From mst.ManpowerBudget m
                        where m.ROBudgetCode in (select m.Code from EmployeeInformation e
                        left join mst.ManpowerBudget m on m.Id=e.BudgetCode
                        Where e.SystemId='" + employeeId + "')";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEmpData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string employeeId = string.Empty;
            employeeId = identity.EmployeeId;
            var st = @"select e.EmployeeCode,e.EmployeeName,e.BudgetCode,m.Id from employeeinformation e
                            left join MST.ManpowerBudget m on m.Id = e.BudgetCode
                         Where e.SystemId='" + employeeId + "'";
            return Json(_sqlRepository.GetDataCollection(st), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetRoBudgetCodeData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string employeeId = string.Empty;
            employeeId = identity.EmployeeId;
            var str = @"select ee.SystemId, ee.EmployeeCode,ee.EmployeeName,m.Id,m.Code  
						From mst.ManpowerBudget m 
                        left join org.Entity e on e.Id=m.EntityId
						inner join EmployeeInformation ee on ee.BudgetCode = m.Id and ee.EmployeeStatus = 'Active'
                        where e.PlantId='" + identity.PlantId + "' and ISNULL(m.ROBudgetCode,'')<>''";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetData(string date, string ROId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string employeeId = string.Empty;
            employeeId = identity.EmployeeId;
            string wc = "";
            if (string.IsNullOrEmpty(ROId) || ROId == "null")
            {
                wc = "";
            }
            else
            {
                //wc = "and mpb.ROBudgetCode in (select BudgetCode from EmployeeInformation Where SystemId='" + employeeId + "')";
                wc = @"and mpb.ROBudgetCode in ('" + ROId + "')";
            }

            var str = @"Select distinct isnull(E.UserName, '') as Entity
                        , dep.UserName as Department
                        ,LG.Id DesignationId,LG.UserName Designation
                        ,ec.UserName EmpCategory
                        ,ec.Id EmpCategoryId 
                        ,sec.UserName as Section
                        ,ssec.UserName as SubSection
                        ,ISNULL( L.UserName,'') Line
                        ,L.Id LineId
                        ,E.Id as EntityId 
                        ,dep.Id as DepId 
                        ,sec.Id as SecId 
                        ,ssec.Id as SubSecId,J.JobLocation,J.SystemID JobLocationId
                        from org.Position p
                        left join mst.ManpowerBudget mpb on mpb.PositionId = p.Id
                        left join org.Entity e on e.Id = mpb.EntityId
                        left join org.Section sec on sec.id = p.SectionId
                        left join org.SubSection ssec on ssec.Id = p.SubSectionId
                        left join org.Department dep on dep.Id = p.DepartmentId
						LEFT JOIN  (select distinct LegalDesignationId,BudgetCode,LineId,DOJ,DOS,JobLocationID from  dbo.EmployeeInformation ) ei on ei.BudgetCode = mpb.Id 
						left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
						left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
						left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
						left join hkp.LegalDesignation LG on LG.Id = ei.LegalDesignationId
                        LEFT JOIN org.Line L ON L.Id = mpb.LineId
                        LEFT JOIN JobLocation J ON J.SystemID = ei.JobLocationID
						where ei.BudgetCode is not null
                        " + wc + @"
                        and e.PlantId='" + identity.PlantId + @"' and ei.DOJ <= ( '" + date + @"') and (ei.DOS is null or ei.DOS >= '" + date + @"')";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetShift()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsDiscreteAllowanceReport ep = new clsDiscreteAllowanceReport();
                return Json(ep.GetShift(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion

        #region ---Attendance Daily Status Report---
        [HttpPost, Authorize]
        public JsonResult DailyAttendanceStatusReport(string workDate, string shift, string Entity, string Dept, string Ydate, string Sec, string SSec, string empCategoryList, string designationList, string lineList, string dayStatus, bool WithFatherName, string JobLocation,bool IsWithLine)
        {
            try
            {
                string LineId = string.Empty;
                if (Convert.ToDateTime(workDate) < Convert.ToDateTime(Ydate))
                {
                    throw new Exception("PreviousDay Cannot greater then Selected Date");
                }
                if (lineList.Contains("'null'"))
                {
                    LineId = lineList.Replace("'null'", "''");
                }
                else
                {
                    LineId = lineList;
                }
                var sft = dayStatus.Split(',');
                string Dstatus = "";
                foreach (var item in sft)
                {
                    Dstatus += "*" + item + "*,";
                }
                Dstatus = Dstatus.Replace('*', '"');

                string ShiftId = "'" + shift.Replace(",", "','") + "'";//replaced with ""
                string fileName = "";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DailyAttendanceReport ep = new DailyAttendanceReport(mailReceiverDetailRepository);
                fileName = ep.GetDailyAttendanceEmpInformation(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, "Attendance Daily Status", "", workDate, ShiftId, Entity, Dept, Ydate, Sec, SSec, empCategoryList, designationList, LineId, Dstatus, WithFatherName, JobLocation,IsWithLine);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region --- Daily Day Status Report---
        [HttpPost, Authorize]
        public JsonResult DailyDayStatusReport(string workDate, string sDepID, string PrevWorkDate, string sSecID, string sSubSecID, string sLineID, string dayStatus, string Dep, string Sec, string employeeCategory, string shift, string entity, string designationList, bool WithFatherName, string JobLocation, bool IsWithLine)
        {
            try
            {
                string LineId = string.Empty;
                if (sLineID.Contains("'null'"))
                {
                    LineId = sLineID.Replace("'null'", "''");
                }
                else
                {
                    LineId = sLineID;
                }
                if (Convert.ToDateTime(workDate) < Convert.ToDateTime(PrevWorkDate))
                {
                    throw new Exception("PreviousDay Cannot greater then Selected Date");
                }
                string Dstatus = "'" + dayStatus.Replace(",", "','") + "'";//replaced with ""
                string ShiftId = "'" + shift.Replace(",", "','") + "'";//replaced with 

                if (Dstatus.Contains("Other") && Dstatus.Length > 7)
                {
                    throw new Exception("Only [Other] accpeted..!");
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var workbook = DailyDayStatus(identity.PlantId, PrevWorkDate, identity.CompanyId, workDate, sDepID, sSecID, sSubSecID, LineId, Dstatus, Dep, Sec, employeeCategory, ShiftId, entity, designationList, WithFatherName, JobLocation,IsWithLine);
                return Json(new { FileName = workbook, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public string DailyDayStatus(string PlantId, string PrevWorkDate, string companyId, string TextFromDate, string sDepID, string sSecID, string sSubSecID, string sLineID, string dayStatus, string Dep, string Sec, string employeeCategory, string shift, string entity, string designationList, bool WithFatherName, string JobLocation, bool IsWithLine)
        {
            #region Variable

            clsReport objRpt = null;
            clsReport objRptD = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsHeading = null;
            string yot = string.Empty;//OTConsiderOn
            string tot = string.Empty;//OTConsiderOn
            DataSet dsAttn = null;
            DataSet dsEmp = null;
            DataView dvAttn = null;
            DataSet dsFactory = null;
            DataView dvEmp = null;
            DataSet dslocal = null;
            DataSet dsCmp = null;
            clsReport objDlySts = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            int cLateBy = 0;
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string sOfficeInTime = "00:00:00";
            string sInTime = "00:00:00";
            var report = new ReportUtility();
            objDlySts = new clsReport();
            DataSet dsExtraAbsent = null;
            DataView dvExtraAbsent = null;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objRpt = new clsReport();

                var ob = new clsStaticInfo();

                //objRpt.GetExtraAbsentForDaily(identity.PlantId, workDate, out dsExtraAbsent);
                //dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);
                //bool IsExtraAbsent = false;
                #region Variable


                ParaAttendanceReport op = new global::ParaAttendanceReport();
                op.PlantId = identity.PlantId;
                op.ADate = TextFromDate;
                #endregion Variable

                if (string.IsNullOrEmpty(TextFromDate.Trim()) == true || bplib.clsWebLib.IsDateOK(TextFromDate.Trim()) == false)
                {
                    Exception ex = new Exception("Please define Attendance Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }

                #region DataSet

                string ddlDept = sDepID;
                string ddlSec = sSecID;
                string ddlSbSec = sSubSecID;
                string ddll = sLineID;

                DailyAttendanceReport ep = new DailyAttendanceReport(mailReceiverDetailRepository);

                ep.GetDailyDayStatusS(TextFromDate, PrevWorkDate, PlantId, ddlDept, ddlSec, ddlSbSec, ddll, dayStatus, employeeCategory, shift, entity, designationList, JobLocation, out dslocal);

                dvAttn = new DataView();
                dvAttn.Table = dslocal.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId.Trim(), out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                if (dvAttn.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 7;
                    int intRow = 0;

                    string strSubSec = "0";
                    string strSec = "0";
                    string strUnit = "0";
                    int strCount = 0;
                    string strLateBy = "00:00:00";

                    #region ------------------Column Header------------------
                    xlsCol = 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Sl No.";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Employee Code";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8.50;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Employee Name";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    if (WithFatherName == true)
                    {
                        sheet1.Range[xlsRow, xlsCol].Text = "Father Name";
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                    }

                    sheet1.Range[xlsRow, xlsCol].Text = "Department";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "Section";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "Subsection";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "Designation";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    if (IsWithLine)
                    {
                        sheet1.Range[xlsRow, xlsCol].Text = "Line";
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                    }

                    sheet1.Range[xlsRow, xlsCol].Text = "Shift Name";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Shift InTime";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Shift OutTime";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "InTime";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "OutTime";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Status";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int p = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Late By";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int q = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Yesterday Status";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 9;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int y = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Yesterday OT";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 9;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int yo = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Emp. Signature";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 31;
                    sheet1.Range[xlsRow, xlsCol].RowHeight = 60;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int t = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Incharge Signature";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 31;
                    sheet1.Range[xlsRow, xlsCol].RowHeight = 60;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int I = xlsCol;

                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    endXlsCol = xlsCol;
                    xlsCol = 1;
                    xlsRow += 1;
                    #endregion ------------------Column Header------------------
                    //strCount = 0;
                    for (int i = 0; i < dvAttn.Count; i++)
                    {

                        xlsCol = 1;
                        #region ----------------------Data-----------------------

                        strCount += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = strCount;
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeCode"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeName"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        if (WithFatherName == true)
                        {
                            sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["FatherName"].ToString().ToUpper();
                            sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                        }


                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Department"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Section"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["SubSection"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Designation"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        if (IsWithLine)
                        {
                            sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Line"].ToString().ToUpper();
                            sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                        }
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftName"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftIn"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftOut"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["InTime"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (bplib.clsWebLib.GetBoolData(dvAttn[i]["IsManualInTime"].ToString().Trim()))
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Orange;
                        }
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["OutTime"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (bplib.clsWebLib.GetBoolData(dvAttn[i]["IsManualOutTime"].ToString().Trim()))
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Orange;
                        }


                        xlsCol += 1;
                        sheet1.Range[xlsRow, p].Text = dvAttn[i]["TodayStatus"].ToString().Trim();
                        sheet1.Range[xlsRow, p].RowHeight = 13;
                        sheet1.Range[xlsRow, p].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, p].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (dvAttn[i]["TodayStatus"].ToString() == "L")
                        {
                            #region Late by min

                            sOfficeInTime = "00:00:00";
                            sInTime = "00:00:00";
                            strLateBy = "00:00:00";

                            if (dvAttn[i]["iintime"].ToString().Trim() != "")
                            {
                                sInTime = dvAttn[i]["iintime"].ToString().Trim() + ":00";
                            }
                            strLateBy = "00:00";
                            if (dvAttn[i]["iShiftIn"].ToString().Trim() != "" && sInTime != "00:00:00")
                            {
                                sOfficeInTime = dvAttn[i]["iShiftIn"].ToString().Trim() + ":00";
                                strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString();
                            }

                            //oru.SetText(ref sheet1, xlsRow, cLateBy, strLateBy);
                            xlsCol += 1;
                            sheet1.Range[xlsRow, q].Text = strLateBy;
                            sheet1.Range[xlsRow, q].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, q].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            #endregion Late by min
                        }

                        xlsCol += 1;
                        sheet1.Range[xlsRow, y].Text = dvAttn[i]["PrvDayStatus"].ToString().Trim();
                        sheet1.Range[xlsRow, y].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, y].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (!string.IsNullOrEmpty(dvAttn[i]["YesterdayOTHr"].ToString()))
                        {
                            oru.GetOT(dvAttn[i]["OTConsiderOn"].ToString(), dvAttn[i]["YesterdayOTHr"].ToString(), out yot);
                            if (yot == "0:00")
                            {
                                yot = "";
                            }
                        }

                        xlsCol += 1;
                        sheet1.Range[xlsRow, yo].Text = yot;
                        sheet1.Range[xlsRow, yo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, yo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsRow += 1;

                        #region Line Setup
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].RowHeight = 60;
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].WrapText = true;
                        #endregion


                        #endregion ----------------------Data-----------------------

                    }

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);


                        }


                    }
                    catch (Exception)
                    {

                    }

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    string FactoryAddress = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = CmpName;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 30;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                        //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = FactoryName;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 26;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "Day Status Report";
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "Attendance Date:- " + TextFromDate;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 9;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 7;
                    #endregion

                    #region Page Setup

                    sheet1.Name = "DayStatus";
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    sheet1.PageSetup.PrintTitleRows = "$1:$5";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    #endregion             

                    workbook.Version = ExcelVersion.Excel97to2003;
                    report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);

                    // return workbook;

                    var filePath = "";
                    var SheetName = "";
                    //return workbook;
                    workbook.Version = ExcelVersion.Excel97to2003;
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
                    workbook.SaveAs(filePath);
                    workbook.Close();
                    excelEngine.Dispose();
                    return filePath;
                }
                else
                {
                    throw new Exception("No Data found...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        #endregion

        #region ---Attendance Daily Day Status Count Report---
        [HttpPost, Authorize]
        public JsonResult DailyStatusCount(string workDate, string shift, string Entity, string Dept, string Ydate, string Sec, string SSec, string designationList, string empCategoryList, string lineList, string dayStatus, bool WithFatherName, string JobLocation, bool IsWithLine)
        {
            try
            {
                string LineId = string.Empty;
                if (lineList.Contains("'null'"))
                {
                    LineId = lineList.Replace("'null'", "''");
                }
                else
                {
                    LineId = lineList;
                }
                string Dstatus = "'" + dayStatus.Replace(",", "','") + "'";//replaced with ""
                string ShiftIds = "'" + shift.Replace(",", "','") + "'";//replaced with ""
                if (Convert.ToDateTime(workDate) < Convert.ToDateTime(Ydate))
                {
                    throw new Exception("PreviousDay Cannot greater then Selected Date");
                }

                string fileName = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                fileName = DailyStatusCountReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, "Daily Status Count", "", workDate, ShiftIds, Entity, Dept, Ydate, Sec, SSec, designationList, empCategoryList, LineId, Dstatus, WithFatherName, JobLocation,IsWithLine);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string DailyStatusCountReport(string CGId, string CompanyId, string PlantId, string SheetName1, string s1, string workDate, string shift, string Entity, string Dept, string Ydate, string Sec, string SSec, string designationList, string empCategoryList, string LineId, string Dstatus, bool WithFatherName, string JobLocation, bool IsWithLine)
        {
            #region Variable

            clsReport objRpt = null;
            clsReport objRptD = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsHeading = null;
            string yot = string.Empty;//OTConsiderOn
            string tot = string.Empty;//OTConsiderOn
            DataSet dsAttn = null;
            DataSet dsEmp = null;
            DataView dvAttn = null;
            DataView dvEmp = null;

            DataSet dsFactory = null;
            DataSet dsCmp = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string sOfficeInTime = "00:00:00";
            string sInTime = "00:00:00";
            var report = new ReportUtility();

            DataSet dsExtraAbsent = null;
            DataView dvExtraAbsent = null;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string companyId = identity.CompanyId;
                objRpt = new clsReport();

                var ob = new clsStaticInfo();

                objRpt.GetExtraAbsentForDaily(identity.PlantId, workDate, out dsExtraAbsent);
                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);
                bool IsExtraAbsent = false;
                #region Variable


                ParaAttendanceReport op = new global::ParaAttendanceReport();
                op.PlantId = identity.PlantId;
                op.ADate = workDate;
                #endregion Variable

                if (string.IsNullOrEmpty(workDate.Trim()) == true || bplib.clsWebLib.IsDateOK(workDate.Trim()) == false)
                {
                    Exception ex = new Exception("Please define Attendance Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }

                #region DataSet
                getEmployee(CGId, CompanyId, PlantId, workDate, shift, Entity, Dept, Ydate, Sec, SSec, designationList, empCategoryList, LineId, Dstatus, JobLocation, out dsAttn);

                dvAttn = new DataView();
                dvAttn.Table = dsAttn.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId.Trim(), out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                #endregion DataSet

                if (dvAttn.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 7;
                    int intRow = 0;

                    string strSubSec = "0";
                    //string strSec = "0";
                   // string strUnit = "0";
                    int strCount = 0;
                    //string strLateBy = "00:00:00";

                    #region ------------------Column Header------------------
                    xlsCol = 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Sl No.";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Employee Code";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8.50;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Employee Name";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 24;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    if (WithFatherName == true)
                    {
                        sheet1.Range[xlsRow, xlsCol].Text = "Father Name";
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 24;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                    }

                    sheet1.Range[xlsRow, xlsCol].Text = "Department";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 26;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "Section";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 26;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "Subsection";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 26;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "Designation";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    if (IsWithLine)
                    {
                        sheet1.Range[xlsRow, xlsCol].Text = "Line";
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 24;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                    }

                    sheet1.Range[xlsRow, xlsCol].Text = "Shift Name";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 17;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Shift InTime";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Shift OutTime";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "InTime";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "OutTime";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "OnRole";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Present";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int p = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Late";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int l = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Absent";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int a = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Leave";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int lv = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "MLv";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int mlv = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "WeekOff";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int w = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Half Day";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int o = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Holiday";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int h = xlsCol;


                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Yesterday Status";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int y = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Yesterday OT";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int yo = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Today OT";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int t = xlsCol;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    endXlsCol = xlsCol;
                    xlsCol = 1;
                    xlsRow += 1;
                    #endregion ------------------Column Header------------------
                    strCount = 0;
                    for (int i = 0; i < dvAttn.Count; i++)
                    {
                        xlsCol = 1;

                        xlsRow += intRow;
                        intRow = 1;

                        if (strSubSec.ToUpper() == "GENERAL")
                        {

                        }
                        #region ----------------------Data-----------------------

                        strCount += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = strCount;
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeCode"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeName"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        if (WithFatherName == true)
                        {
                            sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["FatherName"].ToString().ToUpper();
                            sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                        }
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Department"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Section"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["SubSection"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Designation"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        if (IsWithLine)
                        {
                            sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Line"].ToString().ToUpper();
                            sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                        }
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftName"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftIn"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftOut"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["InTime"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (bplib.clsWebLib.GetBoolData(dvAttn[i]["IsManualInTime"].ToString().Trim()))
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Orange;
                        }
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["OutTime"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (bplib.clsWebLib.GetBoolData(dvAttn[i]["IsManualOutTime"].ToString().Trim()))
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Orange;
                        }
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Number =clsStaticInfo.dbl(dvAttn[i]["OnRole"].ToString().Trim());
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                      

                        if (dvAttn[i]["Category"].ToString() == "Present")

                        {
                            xlsCol += 1;
                            sheet1.Range[xlsRow, p].Number = 1;
                            sheet1.Range[xlsRow, p].RowHeight = 13;
                            sheet1.Range[xlsRow, p].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, p].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        }
                        else if (dvAttn[i]["Category"].ToString() == "Late")
                        {
                            xlsCol += 1;
                            sheet1.Range[xlsRow, l].Number = 1;
                            sheet1.Range[xlsRow, l].RowHeight = 13;
                            sheet1.Range[xlsRow, l].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, l].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        else if (dvAttn[i]["Category"].ToString() == "Absent")
                        {
                            xlsCol += 1;
                            sheet1.Range[xlsRow, a].Number = 1;
                            sheet1.Range[xlsRow, a].RowHeight = 13;
                            sheet1.Range[xlsRow, a].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, a].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        }
                        else if (dvAttn[i]["Category"].ToString() == "Leave")
                        {
                            if (dvAttn[i]["DayStatus"].ToString().ToUpper() == "MLV" || dvAttn[i]["DayStatus"].ToString().ToUpper() == "ML")
                            {
                                xlsCol += 1;
                                sheet1.Range[xlsRow, mlv].Number = 1;
                                sheet1.Range[xlsRow, mlv].RowHeight = 13;
                                sheet1.Range[xlsRow, mlv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, mlv].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            }
                            else
                            {
                                xlsCol += 1;
                                sheet1.Range[xlsRow, lv].Number = 1;
                                sheet1.Range[xlsRow, lv].RowHeight = 13;
                                sheet1.Range[xlsRow, lv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, lv].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }
                        }

                        else if (dvAttn[i]["Category"].ToString().ToUpper() == "WEEKEND")
                        {
                            xlsCol += 1;
                            sheet1.Range[xlsRow, w].Number = 1;
                            sheet1.Range[xlsRow, w].RowHeight = 13;
                            sheet1.Range[xlsRow, w].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, w].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        }

                        else if (dvAttn[i]["Category"].ToString().ToUpper() == "HALF DAY")
                        {
                            xlsCol += 1;
                            sheet1.Range[xlsRow, o].Number = 1;
                            sheet1.Range[xlsRow, o].RowHeight = 13;
                            sheet1.Range[xlsRow, o].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, o].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        }
                        else
                        {
                            xlsCol += 1;
                            sheet1.Range[xlsRow, h].Number = 1;
                            sheet1.Range[xlsRow, h].RowHeight = 13;
                            sheet1.Range[xlsRow, h].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, h].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }

                        xlsCol += 1;
                        sheet1.Range[xlsRow, y].Text = dvAttn[i]["PrvDayStatus"].ToString().Trim();
                        sheet1.Range[xlsRow, y].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, y].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (!string.IsNullOrEmpty(dvAttn[i]["YesterdayOTHr"].ToString()))
                        {
                            oru.GetOT(dvAttn[i]["OTConsiderOn"].ToString(), dvAttn[i]["YesterdayOTHr"].ToString(), out yot);
                            if (yot == "0:00")
                            {
                                yot = "";
                            }
                        }

                        xlsCol += 1;
                        sheet1.Range[xlsRow, yo].Text = yot;
                        sheet1.Range[xlsRow, yo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, yo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (!string.IsNullOrEmpty(dvAttn[i]["TodaysOT"].ToString()))
                        {
                            oru.GetOT(dvAttn[i]["OTConsiderOn"].ToString(), dvAttn[i]["TodaysOT"].ToString(), out tot);
                            if (tot == "0:00")
                            {
                                tot = "";
                            }
                        }

                        xlsCol += 1;
                        sheet1.Range[xlsRow, t].Text = tot;
                        sheet1.Range[xlsRow, t].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, t].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        // xlsRow += 1;

                        #endregion ----------------------Data-----------------------


                    }

                    #region Line Setup
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].WrapText = true;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);


                        }


                    }
                    catch (Exception)
                    {


                    }

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    string FactoryAddress = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = CmpName;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 30;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                        //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = FactoryName;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 26;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "Daily Status Count Report";
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "Attendance Date:- " + workDate;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 9;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 9;
                    #endregion

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    //sheet1.PageSetup.PrintTitleRows = "$1:$2";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId.Trim() + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                    sheet1.Name = "DailyStatusCount";
                    #endregion             

                    workbook.Version = ExcelVersion.Excel97to2003;
                    report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);

                    // return workbook;

                    var filePath = "";
                    var SheetName = "";
                    //return workbook;
                    workbook.Version = ExcelVersion.Excel97to2003;
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
                    workbook.SaveAs(filePath);
                    workbook.Close();
                    excelEngine.Dispose();
                    return filePath;
                }
                else
                {
                    throw new Exception("No Data found...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        public string DailyStatusCountReportNew(string CGId, string CompanyId, string PlantId, string SheetName1, string s1, string workDate, string shift, string Entity, string Dept, string Ydate, string Sec, string SSec, string designationList, string empCategoryList, string LineId, string Dstatus, bool WithFatherName, string JobLocation, bool IsWithLine)
        {
            #region Variable

            clsReport objRpt = null;
            clsReport objRptD = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsHeading = null;
            string yot = string.Empty;//OTConsiderOn
            string tot = string.Empty;//OTConsiderOn
            DataSet dsAttn = null;
            DataSet dsEmp = null;
            DataView dvAttn = null;
            DataView dvEmp = null;

            DataSet dsFactory = null;
            DataSet dsCmp = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string sOfficeInTime = "00:00:00";
            string sInTime = "00:00:00";
            var report = new ReportUtility();

            DataSet dsExtraAbsent = null;
            DataView dvExtraAbsent = null;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string companyId = identity.CompanyId;
                objRpt = new clsReport();

                var ob = new clsStaticInfo();

                objRpt.GetExtraAbsentForDaily(identity.PlantId, workDate, out dsExtraAbsent);
                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);
                bool IsExtraAbsent = false;
                #region Variable


                ParaAttendanceReport op = new global::ParaAttendanceReport();
                op.PlantId = identity.PlantId;
                op.ADate = workDate;
                #endregion Variable

                if (string.IsNullOrEmpty(workDate.Trim()) == true || bplib.clsWebLib.IsDateOK(workDate.Trim()) == false)
                {
                    Exception ex = new Exception("Please define Attendance Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }

                #region DataSet
                getEmployee(CGId, CompanyId, PlantId, workDate, shift, Entity, Dept, Ydate, Sec, SSec, designationList, empCategoryList, LineId, Dstatus, JobLocation, out dsAttn);

                dvAttn = new DataView();
                dvAttn.Table = dsAttn.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId.Trim(), out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                #endregion DataSet

                if (dvAttn.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(2);
                    sheet1 = workbook.Worksheets[1];
                    sheet1.IsGridLinesVisible = true;
                    ReportUtility reportUtility = new ReportUtility();
                    xlsRow = 7;
                    int intRow = 0;
                    int startRow = 0;
                    string strSubSec = "0";
                    //string strSec = "0";
                    // string strUnit = "0";
                    int strCount = 0;
                    int colLine = 0;
                    //string strLateBy = "00:00:00";

                    #region ------------------Column Header------------------
                    xlsCol = 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Sl No.";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Employee Code";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8.50;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Employee Name";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 24;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    if (WithFatherName == true)
                    {
                        sheet1.Range[xlsRow, xlsCol].Text = "Father Name";
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 24;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                    }

                    sheet1.Range[xlsRow, xlsCol].Text = "Department";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 26;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                   int colDpt = xlsCol;
                    xlsCol += 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "Section";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 26;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "Subsection";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 26;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "Designation";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    if (IsWithLine)
                    {
                        sheet1.Range[xlsRow, xlsCol].Text = "Line";
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 24;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        colLine = xlsCol;
                        xlsCol += 1;
                    }

                    sheet1.Range[xlsRow, xlsCol].Text = "Shift Name";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 17;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int colShiftName = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Shift InTime";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Shift OutTime";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "InTime";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "OutTime";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "OnRole";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Present";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int p = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Late";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int l = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Absent";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int a = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Leave";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int lv = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "MLv";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int mlv = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "WeekOff";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int w = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Half Day";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int o = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Holiday";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int h = xlsCol;


                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Yesterday Status";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int y = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Yesterday OT";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int yo = xlsCol;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Today OT";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int t = xlsCol;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    endXlsCol = xlsCol;
                    xlsCol = 1;
                    xlsRow += 1;
                    #endregion ------------------Column Header------------------
                    strCount = 0;
                    startRow = 8;
                    for (int i = 0; i < dvAttn.Count; i++)
                    {
                        xlsCol = 1;

                        xlsRow += intRow;
                        intRow = 1;
                         
                        if (strSubSec.ToUpper() == "GENERAL")
                        {

                        }
                        #region ----------------------Data-----------------------

                        strCount += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = strCount;
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeCode"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeName"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        if (WithFatherName == true)
                        {
                            sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["FatherName"].ToString().ToUpper();
                            sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                        }
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Department"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Section"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["SubSection"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Designation"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        if (IsWithLine)
                        {
                            sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Line"].ToString().ToUpper();
                            sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                        }
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftName"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftIn"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftOut"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["InTime"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (bplib.clsWebLib.GetBoolData(dvAttn[i]["IsManualInTime"].ToString().Trim()))
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Orange;
                        }
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["OutTime"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (bplib.clsWebLib.GetBoolData(dvAttn[i]["IsManualOutTime"].ToString().Trim()))
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Orange;
                        }
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = clsStaticInfo.dbl(dvAttn[i]["OnRole"].ToString().Trim());
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (dvAttn[i]["Category"].ToString() == "Present")

                        {
                            xlsCol += 1;
                            sheet1.Range[xlsRow, p].Number = 1;
                            sheet1.Range[xlsRow, p].RowHeight = 13;
                            sheet1.Range[xlsRow, p].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, p].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        }
                        else if (dvAttn[i]["Category"].ToString() == "Late")
                        {
                            xlsCol += 1;
                            sheet1.Range[xlsRow, l].Number = 1;
                            sheet1.Range[xlsRow, l].RowHeight = 13;
                            sheet1.Range[xlsRow, l].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, l].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        else if (dvAttn[i]["Category"].ToString() == "Absent")
                        {
                            xlsCol += 1;
                            sheet1.Range[xlsRow, a].Number = 1;
                            sheet1.Range[xlsRow, a].RowHeight = 13;
                            sheet1.Range[xlsRow, a].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, a].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        }
                        else if (dvAttn[i]["Category"].ToString() == "Leave")
                        {
                            if (dvAttn[i]["DayStatus"].ToString().ToUpper() == "MLV" || dvAttn[i]["DayStatus"].ToString().ToUpper() == "ML")
                            {
                                xlsCol += 1;
                                sheet1.Range[xlsRow, mlv].Number = 1;
                                sheet1.Range[xlsRow, mlv].RowHeight = 13;
                                sheet1.Range[xlsRow, mlv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, mlv].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            }
                            else
                            {
                                xlsCol += 1;
                                sheet1.Range[xlsRow, lv].Number = 1;
                                sheet1.Range[xlsRow, lv].RowHeight = 13;
                                sheet1.Range[xlsRow, lv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, lv].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }
                        }

                        else if (dvAttn[i]["Category"].ToString().ToUpper() == "WEEKEND")
                        {
                            xlsCol += 1;
                            sheet1.Range[xlsRow, w].Number = 1;
                            sheet1.Range[xlsRow, w].RowHeight = 13;
                            sheet1.Range[xlsRow, w].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, w].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        }

                        else if (dvAttn[i]["Category"].ToString().ToUpper() == "HALF DAY")
                        {
                            xlsCol += 1;
                            sheet1.Range[xlsRow, o].Number = 1;
                            sheet1.Range[xlsRow, o].RowHeight = 13;
                            sheet1.Range[xlsRow, o].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, o].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        }
                        else
                        {
                            xlsCol += 1;
                            sheet1.Range[xlsRow, h].Number = 1;
                            sheet1.Range[xlsRow, h].RowHeight = 13;
                            sheet1.Range[xlsRow, h].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, h].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }

                        xlsCol += 1;
                        sheet1.Range[xlsRow, y].Text = dvAttn[i]["PrvDayStatus"].ToString().Trim();
                        sheet1.Range[xlsRow, y].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, y].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (!string.IsNullOrEmpty(dvAttn[i]["YesterdayOTHr"].ToString()))
                        {
                            oru.GetOT(dvAttn[i]["OTConsiderOn"].ToString(), dvAttn[i]["YesterdayOTHr"].ToString(), out yot);
                            if (yot == "0:00")
                            {
                                yot = "";
                            }
                        }

                        xlsCol += 1;
                        sheet1.Range[xlsRow, yo].Text = yot;
                        sheet1.Range[xlsRow, yo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, yo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (!string.IsNullOrEmpty(dvAttn[i]["TodaysOT"].ToString()))
                        {
                            oru.GetOT(dvAttn[i]["OTConsiderOn"].ToString(), dvAttn[i]["TodaysOT"].ToString(), out tot);
                            if (tot == "0:00")
                            {
                                tot = "";
                            }
                        }

                        xlsCol += 1;
                        sheet1.Range[xlsRow, t].Text = tot;
                        sheet1.Range[xlsRow, t].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, t].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        // xlsRow += 1;

                        #endregion ----------------------Data-----------------------


                    }
                    int endxlsRow = xlsRow;
                   
                    #region Line Setup
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].WrapText = true;

                    IListObject table = sheet1.ListObjects.Create("Table1", sheet1.Range[7, 1, endxlsRow, endXlsCol]);
                    table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);


                        }


                    }
                    catch (Exception)
                    {


                    }

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    string FactoryAddress = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = CmpName;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 30;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                        //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = FactoryName;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 26;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "Daily Status Count Report";
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "Attendance Date:- " + workDate;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 9;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 9;
                    #endregion

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    //sheet1.PageSetup.PrintTitleRows = "$1:$2";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId.Trim() + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                    sheet1.Name = "DailyStatusCount";
                    #endregion             

                    //workbook.Version = ExcelVersion.Excel97to2003;
                    report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);

                    // return workbook;

                    var filePath = "";
                    var SheetName = "";
                    //return workbook;
                    //workbook.Version = ExcelVersion.Excel97to2003;

                    #region Pivot
                    //  DataTable dtDistinctParameter = dtOrder.DefaultView.ToTable(true, "Parameter");

                    string fPath = fPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "OrderTempReport" + identity.UserId + ".xlsx";

                    workbook.SaveAs(fPath);
                    workbook = application.Workbooks.Open(fPath);
                    try { System.IO.File.Delete(fPath); } catch (Exception) { }

                    workbook.Worksheets[0].Name = "Report";

                    IWorksheet pivotSheet = workbook.Worksheets[0];
                    IPivotCache cache = workbook.PivotCaches.Add(workbook.Worksheets[1][startRow - 1, 1, endxlsRow - 1, endXlsCol]);
                    IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A8"], cache);

                    pivotTable.Fields[colDpt - 1].Axis = PivotAxisTypes.Row;
                    if (IsWithLine)
                    {
                        pivotTable.Fields[colLine - 1].Axis = PivotAxisTypes.Row; 
                    }
                    pivotTable.Fields[colShiftName - 1].Axis = PivotAxisTypes.Column;


                    IPivotField field = pivotTable.Fields[p - 1];
                    field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                    pivotTable.DataFields.Add(field, "Present", PivotSubtotalTypes.Sum);

                    for (int i = 0; i < pivotTable.Fields.Count; i++)
                    {
                        if (i == colDpt - 1 || i == colLine - 1 || i == colShiftName - 1)
                            pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                    }

                    pivotTable.ShowRowGrand = false;
                    pivotTable.ShowDrillIndicators = false;
                    pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable.Options.NullString = "";
                    pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                    sheet1 = workbook.Worksheets[0];
                    reportUtility.CompanyPlantHeaderNew(ref sheet1, 1, "DailyStatusCountReport", identity.CompanyId, identity.CompanyName, "");

                    reportUtility.PageSetup(ref sheet1, 6, ExcelPageOrientation.Landscape);
                    sheet1[xlsRow,xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[1, 1, 6, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                    sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet1.IsGridLinesVisible = false;
                    workbook.Worksheets[0].UsedRange["A8"].FreezePanes();


                    #endregion Buyer Summary

                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                    workbook.SaveAs(filePath);
                    workbook.Close();
                    excelEngine.Dispose();
                    return filePath;
                }
                else
                {
                    throw new Exception("No Data found...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        string GetDuration(string dti, string dto, string intime, string outtime)
        {
            string res = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(intime) == false && string.IsNullOrEmpty(outtime) == false)
                {
                    string vintime = Convert.ToDateTime(intime).ToString("HH:mm:ss");
                    string vouttime = Convert.ToDateTime(outtime).ToString("HH:mm:ss");
                    var x = (Convert.ToDateTime(dto) - (Convert.ToDateTime(dti)));
                    res = x.ToString().Substring(0, 5);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void getEmployee(string companyGroupId, string companyId, string plantId, string workDate, string shift, string Entity, string Dept, string Ydate, string Sec, string SSec, string designationList, string empCategoryList, string LineId, string Dstatus, string JobLocation, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string secSQL = string.Empty;
            string xxy = string.Empty;
            clsStaticInfo obs = null;
            string ShiftIds_WC = "";
            string XJobLocation = string.Empty;
            try
            {
                if (shift != "ALL" && shift != "''")
                {
                    ShiftIds_WC = " and sd.SystemID in (" + shift + ") ";
                }

                if (Dstatus != null)
                {
                    if (Dstatus.ToUpper() != "ALL" && Dstatus != "null" && Dstatus != "" && Dstatus != "''")
                    {
                        xxy = " and dt.Category in (" + Dstatus + ")";
                    }
                }
                XJobLocation += " And J.SystemID in (" + JobLocation + ")";
                obs = new clsStaticInfo();
                strSql = @" select e.SystemId
                                            from EmployeeInformation e
                                            left join mst.ManpowerBudget mp on mp.id=e.BudgetCode
											left join org.Entity en on en.id=mp.EntityId    
											left join ORG.Position p on p.Id = mp.PositionId
											left join org.Department dep on dep.Id = p.DepartmentId
											left join org.Section s on s.Id = p.SectionId
											left join org.SubSection ss on ss.Id = p.SubSectionId                                       
                                            LEFT JOIN org.Line L ON L.Id = mp.LineId
                                            LEFT JOIN hkp.LegalDesignation LG ON e.LegalDesignationId = LG.Id 
											left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id
											left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId
											left join HKP.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId
                                            
											where   e.PlantId='" + plantId + @"' and e.DOJ <= ( '" + workDate + @"') and (e.DOS is null or e.DOS >= '" + workDate + @"')";


                if (Dept != "ALL")
                {
                    strSql = strSql + @" AND dep.Id in ( " + Dept + ")";
                }
                if (Sec != "ALL")
                {
                    strSql = strSql + @" AND s.Id in (" + Sec + ")";
                }
                if (SSec != "ALL")
                {
                    strSql = strSql + @" AND ss.Id in (" + SSec + ")";
                }

                if (empCategoryList != "ALL")
                {
                    strSql = strSql + @" AND ec.Id in (" + empCategoryList + ")";
                }

                if (Entity != "ALL")
                {
                    strSql = strSql + @" AND en.Id in (" + Entity + ")";
                }
                if (LineId != "ALL" && LineId != "''")
                {
                    strSql = strSql + @" AND isnull(L.Id,'') in (" + LineId + ")";
                }
                if (designationList != "ALL" && designationList != "''")
                {
                    strSql = strSql + @" AND LG.Id in (" + designationList + ")";
                }

                secSQL = @"SELECT e.SystemId,e.EmployeeCode,e.FatherName
								,dep.username Department
                                , e.EmployeeName
								,sd.UserName ShiftName
                                , ShiftIn  = CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100)
						     END							
								,ShiftOut = CASE                                   
                           WHEN cs.OutTime IS NULL
                           THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
                           ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                           END
                                , FORMAT(CAST(ap.InTime AS datetime2), N'hh:mm tt') InTime
								,FORMAT(CAST( ap.OutTime AS datetime2), N'hh:mm tt') OutTime
	                            ,  REPLACE(CONVERT(VARCHAR(11), ap.WorkDate, 113), ' ', '-') PDate
	                            , ap.DayStatus,dt.Category
	                            , ap.OTHr TodaysOT
                        , LG.UserName Designation
                         , kk.PrvDayStatus
						,kk.YesterdayOTHr,ap.IsManualInTime,ap.IsManualOutTime,hr.OTConsiderOn
                        ,ISNULL(L.UserName,'') Line,s.UserName Section,ss.UserName SubSection,ISNULL(TN.TotalNumber,0) OnRole
                        from EmployeeInformation e
                        left join AttdnProcessData ap on ap.EmpSystemID = e.SystemId
                        left join DayType dt on dt.DayType = ap.DayStatus
                        LEFT JOIN dbo.ShiftDefination SD ON ap.ShiftSystemID = SD.SystemID
                        LEFT OUTER JOIN ShiftTimeChgMaster AS cs ON ap.WorkDate BETWEEN cs.FromDate AND cs.ToDate AND sd.SystemID=cs.ShiftDefinationID
                                            left join mst.ManpowerBudget mp on mp.id = e.BudgetCode
                                            LEFT JOIN(Select SUM(TotalNumber)TotalNumber,ManpowerBudgetId from mst.ManpowerBudgetDetail Group BY ManpowerBudgetId) TN ON TN.ManpowerBudgetId=MP.Id
                                            left join org.Entity en on en.id = mp.EntityId
                                            left join ORG.Position p on p.Id = mp.PositionId
                                            left join org.Department dep on dep.Id = p.DepartmentId
                                            left join org.Section s on s.Id = p.SectionId
                                            LEFT JOIN PlantWiseHRMSSetting hr on HR.PlantID=e.PlantId
                                            left join org.SubSection ss on ss.Id = p.SubSectionId
                                            LEFT JOIN org.Line L ON L.Id = mp.LineId
                                            LEFT JOIN JobLocation J ON J.SystemID = e.JobLocationID
                                            LEFT JOIN hkp.LegalDesignation LG ON e.LegalDesignationId = LG.Id
                                            left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id
                                            left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId
                                            left join HKP.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId                                            
                                            left join(select yap.DayStatus PrvDayStatus, yap.OTHr YesterdayOTHr, yap.EmpSystemID from AttdnProcessData yap where yap.WorkDate = '" + Ydate + @"') kk on kk.EmpSystemID = e.SystemId
                                            where  ap.WorkDate='" + workDate + @"' and e.SystemId in (" + strSql + ")  " + ShiftIds_WC + " " + xxy + " " + XJobLocation + "";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(secSQL, out dsRef);
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        #endregion
    }
}