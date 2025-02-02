using Aplos.Controllers;
using ConnectionManager;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeInFoReportController : BaseController
    {
        #region Constructor

        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        public EmployeeInFoReportController(
              IAttendanceManagementService AttendanceManagementService, IEmployeeProfileService employeeProfileService, ISqlRepository R
            )
        {
            _AttendanceManagementService = AttendanceManagementService;
            _employeeProfileService = employeeProfileService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -----------------------------------Excel Report--------------------------------------------------


        [HttpGet, Authorize]
        public ActionResult EmployeeInFoIndexReport(ReportFormat reportFormat, string radioValue, bool IsCheck, bool LA, bool TBS)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "EmployeeInformation";
            var workbook = EmployeeInFoIndexReportWorkSheet(radioValue, IsCheck, LA, TBS);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

            //workbook.Version = ExcelVersion.Excel97to2003;
            //workbook.SaveAs(reportFileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            return null;
        }

        private IWorkbook EmployeeInFoIndexReportWorkSheet(string radioValue, bool IsCheck, bool LA, bool TBS)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet1 = workbook.Worksheets[0];

            sheet1.Name = "EmployeeInformation";

            #region Variable
            clsReport objRpt = null;
            DataSet dsEmpInfo = null;
            DataView dvEmpID = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsEntityPosition = null;
            Dictionary<string, DataRow> dsShiftAndEffectiveDate, dsTodayShift, dsAlignWithCompany, dsMinimumWage, dsIndividualOT, dsBonus, dsGross; //, dsCTC
            IApplication application = null;
            ReportUtility oRU = null;
            int xlsRow = 1, xlsCol = 1;
            int _FirstVisibleRow = 0;
            bool IsBudgetCodeApplicable = false;

            #endregion Variable

            int ROW = 6;
            int endCol = 1;
            int COL = 1;
            int startRow = 0;
            #region DataSet
            objRpt = new clsReport();
            oRU = new ReportUtility();

            var plantWiseData = GetPlantWiseHRMSSetting();

            GetEmployeesData(identity.CompanyId, radioValue.ToString(), IsCheck, LA, TBS, out dsEmpInfo);
            objRpt.GetEntityPositionInfo(identity.CompanyId, out dsEntityPosition);
            objRpt.GetEmployeesTodaysShift(out dsTodayShift);
            objRpt.GetEmployeesShiftAndEffectiveDate(out dsShiftAndEffectiveDate);
            objRpt.GetEmployeesWeekOffEffectiveDateAlignWithCompany(out dsAlignWithCompany);
            objRpt.GetEmployeesMinimumWage(out dsMinimumWage);
            objRpt.GetEmployeesIndividualOTEntitlement(out dsIndividualOT);
            objRpt.GetEmployeesBonus(out dsBonus);
            objRpt.GetEmployeesGrossSalary(out dsGross);
            //objRpt.GetEmployeesCTCSalary(out dsCTC);

            dvEmpID = new DataView();
            dvEmpID.Table = dsEmpInfo.Tables[0];

            DataView dvEntity = new DataView(dsEntityPosition.Tables[0]);
            dvEntity.RowFilter = "EP='E'";
            dvEntity.Sort = "Sequence";
            DataTable dtEntity = dvEntity.ToTable(true, "UserName", "Sequence");

            DataView dvPosition = new DataView(dsEntityPosition.Tables[0]);
            dvPosition.RowFilter = "EP='P'";
            dvPosition.Sort = "Sequence";
            DataTable dtPosition = dvPosition.ToTable(true, "UserName", "Sequence");

            DataView dvBC = new DataView(dsEmpInfo.Tables[0]);
            DataTable dtBC = dvBC.ToTable(true, "IsPositionCodeApplicable");
            for (int i = 0; i < dtBC.Rows.Count; i++)
            {
                IsBudgetCodeApplicable = bplib.clsWebLib.GetBoolData(dsEmpInfo.Tables[0].Rows[i]["IsPositionCodeApplicable"].ToString());
                if (IsBudgetCodeApplicable)
                {
                    break;
                }
            }

            objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
            //objRpt.SelectedPlant(identity.PlantId, out dsFactory);

            #endregion DataSet

            if (dvEmpID.Count > 0)
            {
                xlsRow = 5;

                _FirstVisibleRow = xlsRow;

                #region variable
                string companyId = identity.CompanyId;
                int cSystemID = 0;
                int cEmployeeId = 0;
                int cEmployeeCode = 0;
                int cBudgetCode = 0;
                int cName = 0;
                int cDOJ = 0;
                int cProbPeriod = 0;
                int cDOC = 0;
                int cDOB = 0;
                int cDOS = 0;
                int cCompany = 0;
                int cDepartment = 0;
                int cPaygroup = 0;
                int cGivenDesignation = 0;
                int cLD = 0;
                int cGivenDesignationGroup = 0;
                int cGivenSalaryRule = 0;
                int cSalaryRule = 0;
                int cWeekOff = 0;
                int cIFSCCode = 0;
                //bc
                int cEntityCode = 0;
                int cEntity = 0;
                //po
                int cPositionCode = 0;
                int cDirectManpowerCost = 0;
                int cPosition = 0;
                int cpDesignation = 0;
                int cGN = 0;
                int cNID = 0;
                int cTIN = 0;
                int cES = 0;
                int cJL = 0;
                int cPA = 0;
                int cPA2 = 0;
                int cPhone = 0;
                int cPic = 0;
                int cFP = 0;
                int cN = 0;
                int cEC = 0;
                int cEL = 0;
                int cBL = 0;
                int cBS = 0;
                int cG = 0;
                int cMW = 0;
                int cPH = 0;
                int cOT = 0;
                int cDOT = 0;
                int cPM = 0;
                int cB = 0;
                int cBN = 0;
                int cPF = 0;
                int cESIC = 0;
                int cBonus = 0;
                int colPF = 0;
                int colESIC = 0;
                int colOM = 0;
                int cEmployeeCurrentStatus = 0;
                int cBloodGroup = 0;
                int cReligion = 0;
                int cCaste = 0;
                int ColAttendanceGroup = 0;
                int ColGS = 0;
                int ColSFT = 0;
                int ColAttnBns = 0;
                int cShift = 0;
                int cRoster = 0;
                int cEDate = 0;
                int cWEDate = 0;
                int cMaxOtHour = 0;
                int cAlignWithCC = 0;
                int cIndv = 0;
                int cFather = 0;
                int cMother = 0;
                int cSpouse = 0;
                int cParAddress = 0;
                int cParAddress2 = 0;
                int cPresThana = 0;
                int cPresCity = 0;
                int cParArea = 0;
                int cPresArea = 0;
                int cPresDistrict = 0;
                int cPresState = 0;
                int cPresCountry = 0;
                int cParmThana = 0;
                int cParmArea = 0;
                int cParmCity = 0;
                int cParmDistrict = 0;
                int cParmState = 0;
                int cParmCountry = 0;
                int cDirect = 0;
                int cContractor = 0;
                int cMultipleOperation = 0;
                int cSingleOperation = 0;
                int cTenureMonth = 0;
                //int cCTC = 0;
                int cREN = 0;
                int cREC = 0;
                
                #endregion variable

                int endXlsCol = 0;
                xlsRow++;
                xlsCol = 1;
                #region Column Header
                

                oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Employee info", ExcelHAlign.HAlignCenter);
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SystemID"); cSystemID = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeId"); cEmployeeId = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Code"); cEmployeeCode = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Name", 25); cName = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Father Name", 25); cFather = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Mother Name", 25); cMother = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Spouse Name", 25); cSpouse = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Present Address1", 40); cPA = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Present Address2", 40); cPA2 = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Pres. Area"); cPresArea = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Pres. Thana"); cPresThana = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Pres. City"); cPresCity = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Pres. District"); cPresDistrict = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Pres. State/Division"); cPresState = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Pres. Country"); cPresCountry = xlsCol; xlsCol++;

                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Permanent Address1", 40); cParAddress = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Permanent Address2", 40); cParAddress2 = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Per. Area"); cParArea = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Per. Thana"); cParmThana = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Per. City"); cParmCity = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Per. District"); cParmDistrict = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Per. State/Division"); cParmState = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Per. Country"); cParmCountry = xlsCol; xlsCol++;

                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Contractor Name", 15); cContractor = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Gender", 7); cGN = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Religion", 8); cReligion = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Caste", 8); cCaste = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Blood Group", 11); cBloodGroup = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PhoneNo"); cPhone = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Picture", 7); cPic = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Card Number"); cN = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Finger Print", 11); cFP = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "NID"); cNID = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "TIN"); cTIN = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOB"); cDOB = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOJ"); cDOJ = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Tenure(Month)", 14); cTenureMonth = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOS"); cDOS = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "P.Period", 8); cProbPeriod = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOC"); cDOC = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Status"); cES = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Current Status", 19); cEmployeeCurrentStatus = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Today's Shift"); ColSFT = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shift Effective Date"); cEDate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Roster Shift Name"); cRoster = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Assign Shift Name", 30); cShift = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Week Off", 25); cWeekOff = xlsCol; xlsCol++;
                //oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Week Off Effective Date"); cWEDate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Align With Company", 11); cAlignWithCC = xlsCol; xlsCol++;
               // oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Individual Week Off", 9); cIndv = xlsCol; xlsCol++;
               //  oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Min. OT"); cMaxOtHour = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Job Location", 14); cJL = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Company", 25); cCompany = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Department", 30); cDepartment = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Payroll Group", 17); cPaygroup = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Attendance Group", 18); ColAttendanceGroup = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Std. Designation", 25); cGivenDesignation = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Legal/Given Designation", 25); cLD = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Std. Designation Group", 25); cGivenDesignationGroup = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Emp. Category"); cEC = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Ref Emp Code"); cREC = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Ref Emp Name"); cREN = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Operation Code", 15); cSingleOperation = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Multiple Operation Code", 15); cMultipleOperation = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Salary Rule", 25); cSalaryRule = xlsCol; xlsCol++;

                //oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Salary Rule (GD)", 25); cGivenSalaryRule = xlsCol; xlsCol++;

                if (IsBudgetCodeApplicable)
                {
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BudgetCode", 11); cBudgetCode = xlsCol; xlsCol++;
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol - 1].Merge();

                    //bc
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Entity info", ExcelHAlign.HAlignCenter);
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Entity Code", 10); cEntityCode = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Entity", 20); cEntity = xlsCol; xlsCol++;
                    for (int i = 0; i < dtEntity.Rows.Count; i++)
                    {
                        oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, dtEntity.Rows[i]["UserName"].ToString(), 25); xlsCol++;
                    }
                    sheet1.Range[xlsRow - 1, cEntityCode, xlsRow - 1, xlsCol - 1].Merge();

                    //po
                    oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Position info", ExcelHAlign.HAlignCenter);
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Position Code", 10); cPositionCode = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Position", 45); cPosition = xlsCol; xlsCol++;
                    for (int i = 0; i < dtPosition.Rows.Count; i++)
                    {
                        oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, dtPosition.Rows[i]["UserName"].ToString(), 15); xlsCol++;
                    }
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", 15); cpDesignation = xlsCol; xlsCol++;

                    sheet1.Range[xlsRow - 1, cPositionCode, xlsRow - 1, cpDesignation].Merge();
                }//IsBudgetCodeApplicable

                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Budgeted Line"); cBL = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Budgeted Shift"); cBS = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Direct Manpower Cost", 25); cDirectManpowerCost = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Direct Manpower", 25); cDirect = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Location"); cEL = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Grade"); cG = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Minimum Wage"); cMW = xlsCol; xlsCol++;

                if (plantWiseData.Rows.Count > 0)
                {
                    if (Convert.ToBoolean(plantWiseData.Rows[0]["IsSalaryStructureShowInEIReport"]) == true)
                    {
                        oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Gross Amount"); ColGS = xlsCol; xlsCol++;
                    }
                }
                //oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "CTC Amount"); cCTC = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Individual OT Entitlement"); cOT = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation OT Entitlement"); cDOT = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Paid Hours"); cPH = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Payment Mode"); cPM = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Bank Name", 25); cBN = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Account No"); cB = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "IFSC Code", 25); cIFSCCode = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PF", 6); colPF = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PF No"); cPF = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ESIC", 6); colESIC = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ESIC No"); cESIC = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Bonus", 6); cBonus = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Attendance Bonus", 25); ColAttnBns = xlsCol; xlsCol++;
                xlsCol--;
                endXlsCol = xlsCol;
                xlsRow++;
                startRow = xlsRow;
                #endregion

                for (int i = 0; i < dsEmpInfo.Tables[0].Rows.Count; i++)
                {
                    oRU.SetText(ref sheet1, xlsRow, cSystemID, dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cEmployeeId, dsEmpInfo.Tables[0].Rows[i]["EmployeeId"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cEmployeeCode, dsEmpInfo.Tables[0].Rows[i]["EmployeeCode"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cName, dsEmpInfo.Tables[0].Rows[i]["EmployeeName"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cFather, dsEmpInfo.Tables[0].Rows[i]["FatherName"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cMother, dsEmpInfo.Tables[0].Rows[i]["MotherName"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cSpouse, dsEmpInfo.Tables[0].Rows[i]["SpouseName"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPA, dsEmpInfo.Tables[0].Rows[i]["PresentAddress1"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPA2, dsEmpInfo.Tables[0].Rows[i]["PresentAddress2"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPresArea, dsEmpInfo.Tables[0].Rows[i]["PresentArea"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPresThana, dsEmpInfo.Tables[0].Rows[i]["PresThana"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPresCity, dsEmpInfo.Tables[0].Rows[i]["PresCity"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPresDistrict, dsEmpInfo.Tables[0].Rows[i]["PresDistrict"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPresState, dsEmpInfo.Tables[0].Rows[i]["PresState"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPresCountry, dsEmpInfo.Tables[0].Rows[i]["PresCountry"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cParAddress, dsEmpInfo.Tables[0].Rows[i]["ParmanentAddress1"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cParAddress2, dsEmpInfo.Tables[0].Rows[i]["ParmanentAddress2"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cParArea, dsEmpInfo.Tables[0].Rows[i]["ParmanentArea"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cParmThana, dsEmpInfo.Tables[0].Rows[i]["ParmThana"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cParmCity, dsEmpInfo.Tables[0].Rows[i]["ParmCity"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cParmDistrict, dsEmpInfo.Tables[0].Rows[i]["ParmDistrict"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cParmState, dsEmpInfo.Tables[0].Rows[i]["ParmState"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cParmCountry, dsEmpInfo.Tables[0].Rows[i]["ParmCountry"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cGN, dsEmpInfo.Tables[0].Rows[i]["GenderID"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cContractor, dsEmpInfo.Tables[0].Rows[i]["ContractorName"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cReligion, dsEmpInfo.Tables[0].Rows[i]["Religion"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cCaste, dsEmpInfo.Tables[0].Rows[i]["Caste"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cBloodGroup, dsEmpInfo.Tables[0].Rows[i]["BloodGroup"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPhone, dsEmpInfo.Tables[0].Rows[i]["CellPhnNo"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPic, dsEmpInfo.Tables[0].Rows[i]["Picture"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cN, dsEmpInfo.Tables[0].Rows[i]["CardNumber"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cFP, dsEmpInfo.Tables[0].Rows[i]["Fingerprint"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cNID, dsEmpInfo.Tables[0].Rows[i]["NationalID"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cTIN, dsEmpInfo.Tables[0].Rows[i]["TIN"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cDOB, dsEmpInfo.Tables[0].Rows[i]["DOB"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cDOJ, dsEmpInfo.Tables[0].Rows[i]["DOJ"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cDOS, dsEmpInfo.Tables[0].Rows[i]["DOS"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cProbPeriod, dsEmpInfo.Tables[0].Rows[i]["ProbationPeriod"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cDOC, dsEmpInfo.Tables[0].Rows[i]["DOC"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cWeekOff, dsEmpInfo.Tables[0].Rows[i]["WeekOff"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cIFSCCode, dsEmpInfo.Tables[0].Rows[i]["IFSCCode"].ToString());
                    if (bplib.clsWebLib.GetBoolData(dsEmpInfo.Tables[0].Rows[i]["IsConfirmed"].ToString()) == false)
                    {
                        sheet1.Range[xlsRow, cDOC].CellStyle.Font.Color = ExcelKnownColors.Red;
                    }
                    oRU.SetText(ref sheet1, xlsRow, cES, dsEmpInfo.Tables[0].Rows[i]["EmployeeStatus"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cEmployeeCurrentStatus, dsEmpInfo.Tables[0].Rows[i]["EmployeeCurrentStatus"].ToString());

                    oRU.SetText(ref sheet1, xlsRow, cJL, dsEmpInfo.Tables[0].Rows[i]["JobLocation"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cCompany, dsEmpInfo.Tables[0].Rows[i]["Company"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cDepartment, dsEmpInfo.Tables[0].Rows[i]["Department"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPaygroup, dsEmpInfo.Tables[0].Rows[i]["PayrollGroup"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, ColAttendanceGroup, dsEmpInfo.Tables[0].Rows[i]["AttendanceGroup"].ToString());
                    //oRU.SetText(ref sheet1, xlsRow, cGivenDesignation, dsEmpInfo.Tables[0].Rows[i]["GivenDesignation"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cGivenDesignation, dsEmpInfo.Tables[0].Rows[i]["StandardDesignation"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cLD, dsEmpInfo.Tables[0].Rows[i]["LegalDesignation"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cGivenDesignationGroup, dsEmpInfo.Tables[0].Rows[i]["StandardDesignationGroup"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cEC, dsEmpInfo.Tables[0].Rows[i]["EmployeeCategory"].ToString());

                    if (dsEmpInfo.Tables[0].Rows[i]["Operation"].ToString() == "Operation Master")
                    {
                        oRU.SetText(ref sheet1, xlsRow, cSingleOperation, dsEmpInfo.Tables[0].Rows[i]["OperationMasterCode"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cMultipleOperation, dsEmpInfo.Tables[0].Rows[i]["MultipleOperationMaster"].ToString());
                    }
                    if (dsEmpInfo.Tables[0].Rows[i]["Operation"].ToString() == "Operation Variation")
                    {
                        oRU.SetText(ref sheet1, xlsRow, cSingleOperation, dsEmpInfo.Tables[0].Rows[i]["OperationVariationCode"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cMultipleOperation, dsEmpInfo.Tables[0].Rows[i]["MultipleOperationVariation"].ToString());
                    }

                    oRU.SetText(ref sheet1, xlsRow, cDOT, dsEmpInfo.Tables[0].Rows[i]["DesignationOT"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPH, dsEmpInfo.Tables[0].Rows[i]["PaidHours"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPM, dsEmpInfo.Tables[0].Rows[i]["PaymentMode"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cBN, dsEmpInfo.Tables[0].Rows[i]["BankName"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cB, dsEmpInfo.Tables[0].Rows[i]["BankAccNo"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cREC, dsEmpInfo.Tables[0].Rows[i]["RefEmpCode"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cREN, dsEmpInfo.Tables[0].Rows[i]["Ref1Name"].ToString());

                    oRU.SetText(ref sheet1, xlsRow, cDirectManpowerCost, dsEmpInfo.Tables[0].Rows[i]["DirectManpowerCost"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cDirect, dsEmpInfo.Tables[0].Rows[i]["Direct"].ToString());
                    if (bplib.clsWebLib.GetBoolData(dsEmpInfo.Tables[0].Rows[i]["IsPositionCodeApplicable"].ToString()))
                    {
                        oRU.SetText(ref sheet1, xlsRow, cBudgetCode, dsEmpInfo.Tables[0].Rows[i]["BudgetCode"].ToString());
                        //entity
                        oRU.SetText(ref sheet1, xlsRow, cEntityCode, dsEmpInfo.Tables[0].Rows[i]["EntityCode"].ToString());

                        oRU.SetText(ref sheet1, xlsRow, cEntity, dsEmpInfo.Tables[0].Rows[i]["Entity"].ToString());

                        for (int c = 0; c < dtEntity.Rows.Count; c++)
                        {
                            string _colname = dtEntity.Rows[c]["UserName"].ToString();
                            oRU.SetText(ref sheet1, xlsRow, cEntity + c + 1, dsEmpInfo.Tables[0].Rows[i]["e" + _colname].ToString());
                        }

                        //position
                        oRU.SetText(ref sheet1, xlsRow, cPositionCode, dsEmpInfo.Tables[0].Rows[i]["PositionCode"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cPosition, dsEmpInfo.Tables[0].Rows[i]["Position"].ToString());
                        for (int c = 0; c < dtPosition.Rows.Count; c++)
                        {
                            string _colname = dtPosition.Rows[c]["UserName"].ToString();
                            oRU.SetText(ref sheet1, xlsRow, cPosition + c + 1, dsEmpInfo.Tables[0].Rows[i]["p" + _colname].ToString());
                        }
                        oRU.SetText(ref sheet1, xlsRow, cpDesignation, dsEmpInfo.Tables[0].Rows[i]["pDesignation"].ToString());
                    }//is bc applicable

                    oRU.SetText(ref sheet1, xlsRow, cBL, dsEmpInfo.Tables[0].Rows[i]["BudgetLine"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cBS, dsEmpInfo.Tables[0].Rows[i]["BudgetShift"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cEL, dsEmpInfo.Tables[0].Rows[i]["EmployeeLocation"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, ColAttnBns, dsEmpInfo.Tables[0].Rows[i]["AttenBnsPolicyName"].ToString());

                    oRU.SetText(ref sheet1, xlsRow, colPF, dsEmpInfo.Tables[0].Rows[i]["PF"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cPF, dsEmpInfo.Tables[0].Rows[i]["PFNumber"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, colESIC, dsEmpInfo.Tables[0].Rows[i]["ESIC"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cESIC, dsEmpInfo.Tables[0].Rows[i]["ESICNumber"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cSalaryRule, dsEmpInfo.Tables[0].Rows[i]["SalaryRuleName"].ToString());

                    oRU.SetText(ref sheet1, xlsRow, cG, dsEmpInfo.Tables[0].Rows[i]["Grade"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cTenureMonth, dsEmpInfo.Tables[0].Rows[i]["TenureMonth"].ToString());

                    // from Other DataSet

                    if (dsTodayShift.ContainsKey(dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()))
                    {
                        DataRow drTemp = dsTodayShift[dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()];
                        oRU.SetText(ref sheet1, xlsRow, ColSFT, drTemp["ShiftDefinationDescription"].ToString());
                    }

                    if (dsShiftAndEffectiveDate.ContainsKey(dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()))
                    {
                        DataRow drTemp = dsShiftAndEffectiveDate[dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()];
                        oRU.SetText(ref sheet1, xlsRow, cEDate, drTemp["EffectiveDate"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cRoster, drTemp["ShiftRosterDescription"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cShift, drTemp["ShiftName"].ToString());
                    }

                    if (dsAlignWithCompany.ContainsKey(dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()))
                    {
                        DataRow drTemp = dsAlignWithCompany[dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()];

                       // oRU.SetText(ref sheet1, xlsRow, cWEDate, drTemp["WeekOffEffectiveDate"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cAlignWithCC, drTemp["AlignWithCC"].ToString());
                        /*oRU.SetText(ref sheet1, xlsRow, cIndv, drTemp["FstOffDay"].ToString());
                        if (drTemp["AlignWithCC"].ToString() == "Yes")
                        {
                            oRU.SetText(ref sheet1, xlsRow, cIndv, "");
                        }
                        else
                        {
                            oRU.SetText(ref sheet1, xlsRow, cIndv, drTemp["FstOffDay"].ToString());
                        }*/
                    }

                    if (dsMinimumWage.ContainsKey(dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()))
                    {
                        DataRow drTemp = dsMinimumWage[dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()];

                        oRU.SetText(ref sheet1, xlsRow, cMW, drTemp["SalaryHeadValue"].ToString());
                    }

                    if (dsBonus.ContainsKey(dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()))
                    {
                        DataRow drTemp = dsBonus[dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()];

                        oRU.SetText(ref sheet1, xlsRow, cBonus, drTemp["BONUS"].ToString());
                    }

                    if (plantWiseData.Rows.Count>0)
                    {
                        if (Convert.ToBoolean(plantWiseData.Rows[0]["IsSalaryStructureShowInEIReport"]) == true)
                        {
                            if (dsGross.ContainsKey(dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()))
                    {
                        DataRow drTemp = dsGross[dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()];

                        oRU.SetText(ref sheet1, xlsRow, ColGS, drTemp["DefineAmount"].ToString());
                    }
                        }
                    }


                    //if (dsCTC.ContainsKey(dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()))
                    //{
                    //    DataRow drTemp = dsCTC[dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()];

                    //    oRU.SetText(ref sheet1, xlsRow, cCTC, drTemp["DefineAmount"].ToString());
                    //}

                    if (dsIndividualOT.ContainsKey(dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()))
                    {
                        DataRow drTemp = dsIndividualOT[dsEmpInfo.Tables[0].Rows[i]["SystemID"].ToString()];

                        oRU.SetText(ref sheet1, xlsRow, cOT, drTemp["OverTime"].ToString());
                    }

                    xlsRow++;
                }
                sheet1.AutoFilters.FilterRange = sheet1.Range[startRow - 1, 1, xlsRow, endXlsCol];
                string CmpName = string.Empty;
                string CompanyImage = string.Empty;
                string FactoryName = string.Empty;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    CompanyImage = dsCmp.Tables[0].Rows[0]["CompanyImage"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                #region ******************Report Header******************
                try
                {
                    string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), CompanyImage);  // IDCardEng.xlsx
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
                string FactoryAddress = string.Empty;


                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    FactoryName = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 8;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Employee Information";
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                string strRptDateRange = "";
                strRptDateRange = "";
                sheet1.Range[xlsRow, 3].Text = strRptDateRange;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                //#region Page Setup

                //sheet1.PageSetup.TopMargin = 0.5;
                //sheet1.PageSetup.BottomMargin = 0.7;
                //sheet1.PageSetup.PrintTitleRows = "$1:$5";
                //sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                //sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                //sheet1.PageSetup.LeftMargin = 0.5;
                //sheet1.PageSetup.RightMargin = 0.2;
                //sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                //sheet1.PageSetup.FitToPagesTall = 0;
                //sheet1.PageSetup.FitToPagesWide = 1;
                //sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                //#endregion Page Setup

            }
            else
            {

            }

            //var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;
            ROW++;



            //sheet1.UsedRange.NumberFormat = "#,##0.000";
            //sheet1.UsedRange.WrapText = true;
            //sheet1.UsedRange.CellStyle.Font.Size = 8;
            //report.CompanyHeader(ref sheet1, endCol, "Employee Information", identity.CompanyId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }


        public DataTable GetPlantWiseHRMSSetting()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"select IsSalaryStructureShowInEIReport from PlantWiseHRMSSetting 
                            where PlantID = '"+identity.PlantId+"'";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void GetEmployeesData(string CompanyId, string radioValue, bool IsCheck, bool LA, bool TBS, out DataSet dsRef)
        {
            string strSQL;
            string wc = string.Empty;
            string c = string.Empty;
            string plant = string.Empty;
            string CS = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (IsCheck == true)
                {
                    plant = "";
                }
                else
                {
                    plant = " and e.PlantId='" + identity.PlantId + @"' ";
                }
                if (radioValue == "Active")
                {
                    wc = " and e.EmployeeStatus='Active'";
                }
                else if (radioValue == "Separated")
                {
                    wc = " and e.EmployeeStatus='Separated'";
                }
                else
                {
                    wc = "";
                }

                if (TBS == false && LA == false)
                {
                    CS = "AND ISNULL(e.EmployeeCurrentStatus,'') NOT IN('TBS','LONG ABSENTEEISM')";
                }
                else if (LA == true && TBS == false)
                {
                    CS = "AND ISNULL(e.EmployeeCurrentStatus, '') NOT IN('TBS')";
                }
                else if (LA == false && TBS == true)
                {
                    CS = "AND ISNULL(e.EmployeeCurrentStatus, '') NOT IN('LONG ABSENTEEISM')";
                }

                strSQL = @"SELECT E.SystemId, E.EmployeeId, E.EmployeeCode,E.EmployeeName,E.FatherName,E.MotherName,E.SpouseName,E.PresentAddress1,E.PresentAddress2, PRPS.UserName PresThana
                            ,PRCT.UserName PresCity, PRD.UserName PresDistrict, PRST.UserName PresState, PRC.UserName PresCountry,E.ParmanentAddress1,E.ParmanentAddress2,PPS.UserName ParmThana,E.TIN
                            ,PCT.UserName ParmCity,PDS.UserName ParmDistrict,PST.UserName ParmState,PC.UserName ParmCountry,E.ParmanentArea,E.PresentArea,PRT.UserName ContractorName,E.GenderID,Reli.UserName Religion
                            ,BG.UserName BloodGroup,e.CellPhnNo,Picture = CASE WHEN E.EmpPicPath IS NULL THEN 'NO' WHEN (E.EmpPicPath IS NOT NULL) THEN 'YES' ELSE 'NO' END
                            ,E.CardNumber,Fingerprint = CASE WHEN FP.Id IS NULL THEN 'NO' WHEN (FP.Id IS NOT NULL) THEN 'YES' ELSE 'NO' END,E.NationalID,FORMAT(E.DOB,'dd-MMM-yyyy') DOB
                            ,FORMAT(E.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(E.DOS,'dd-MMM-yyyy') DOS,ProbationPeriod = CASE WHEN e.DOCIsDay = 1 THEN e.DOCDay ELSE e.DOCMonth * 30 END
                            ,FORMAT(E.DOC,'dd-MMM-yyyy') DOC,e.IsConfirmed,E.EmployeeStatus,E.EmployeeCurrentStatus
                            ,PD.PaidHours, EB.BankAccNo, B.UserName BankName, E.PaymentMode,J.JobLocation,C.UserName Company,egdsg.UserName StandardDesignation
                            ,ld.UserName LegalDesignation,DG.UserName StandardDesignationGroup,EC.UserName EmployeeCategory,mpb.Code BudgetCode,DesignationOT = CASE WHEN DC.IsOTEntitled = 0 THEN 'NO' WHEN DC.IsOTEntitled = 1 THEN 'YES' END
                            ,ELC.UserName EmployeeLocation, BL.UserName BudgetLine, SD.ShiftDefinationDescription BudgetShift,GD.Grade,ISNULL(PG.UserName, 'No Group') PayrollGroup,ag.UserName AttendanceGroup
                            ,en.Code EntityCode, en.UserName Entity, div.UserName eDivision, sdiv.UserName eSubdivision,u.UserName eUnit, p.UserName ePlant,ps.Code PositionCode, ps.UserName Position, pdept.UserName Department, pdept.UserName pDepartment, pdiv.UserName pDivision, psdiv.UserName pSubdivision, pss.UserName pSubsection, 
                            xps.UserName pSection, dsg.UserName pDesignation,DirectManpowerCost = CASE WHEN ps.DirectManpowerCost = 1 THEN 'YES' ELSE 'NO' END,Direct = CASE WHEN ps.IsDirect = 1 THEN 'YES' ELSE 'NO' END
                            ,srm.SalaryRuleName, abpm.AttenBnsPolicyName,PLC.Operation,OV.Code OperationVariationCode,OM.Code OperationMasterCode 
                                ,MultipleOperationVariation=STUFF((select ', '+P.Code 
                                     FROM EmployeeOperation BTP 
									   join MSt.OperationVariation P ON P.Id=BTP.OperationVariationId
                                        WHERE E.SystemId=BTP.EmpSystemId order by BTP.Sequence for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,MultipleOperationMaster=STUFF((select ', '+P.Code 
                                    FROM EmployeeOperation BTP 
									   join MSt.OperationMaster P ON P.Id=BTP.OperationMasterId
                                        WHERE E.SystemId=BTP.EmpSystemId order by BTP.Sequence for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                        , PF.DocNumber PFNumber, ESIC.DocNumber ESICNumber, PF = CASE WHEN PFE.EmpSystemID IS NULL THEN '' WHEN PFE.IsEligible = 1 THEN 'YES' ELSE 'NO' END
                        , ESIC = CASE WHEN ESICE.EmpSystemID IS NULL THEN 'NO' WHEN ESICE.IsEligible = 1 THEN 'YES' ELSE 'NO' END,  IsPositionCodeApplicable=1
                        ,TenureMonth=DATEDIFF(month, FORMAT(DOJ,'dd-MMM-yyyy'), FORMAT(GETDATE(),'dd-MMM-yyyy')),Ref.RefEmpCode,REF.Ref1Name,REF.Ref1CellPhnNo,
					   case When
					   (select top 1 WOH.STANDARDNAME from EmployeeWeeklyOff wo
							left join WeekOffHeader WOH on WOH.Id = WO.WOHeaderId
								where wo.EmpSystemID = e.SystemId
								order by effectivedate desc) is null then  (select Top 1 DefaultWeekOff from dbo.PlantWiseHRMSSetting)
								else (select top 1 WOH.STANDARDNAME from EmployeeWeeklyOff wo
							left join WeekOffHeader WOH on WOH.Id = WO.WOHeaderId
								where wo.EmpSystemID = e.SystemId
								order by effectivedate desc)
					   end WeekOff,  EB.IFSCCode,EADD.UserName Caste
                            FROM EmployeeInformation e
							LEFT JOIN MST.ManpowerBudget mpb ON mpb.Id = e.BudgetCode
                            LEFT JOIN ORG.Company C ON C.Id = E.CompanyId
							LEFT JOIN HKP.Party PRT ON PRT.Id = E.VendorId
							LEFT JOIN SCS.PlantConfig PLC ON PLC.PlantId=E.PlantId AND PLC.CompanyId=E.CompanyId
                            LEFT JOIN [SCS].[Religion] Reli ON Reli.Id = e.ReligionId
							LEFT JOIN MST.OperationVariation OV ON OV.Id = e.OperationVariationId
							LEFT JOIN [MST].[OperationMaster] OM ON OM.Id = E.OperationMasterId
                            LEFT JOIN HKP.BloodGroup BG ON BG.Id = E.BloodGroupID
                            
                            LEFT JOIN EmployeeBankInfo EB ON EB.EmpSystemID = E.SystemId
							AND EB.RowID=(Select top(1) RowID from EmployeeBankInfo Where EmpSystemID=EB.EmpSystemID AND  IsApproved=1 Order BY DateAdded DESC)
                            LEFT JOIN HKP.Bank B ON B.Id = EB.BankSystemID
                            LEFT JOIN SCS.Country PC ON PC.Id = E.ParmCountryID
                            LEFT JOIN SCS.[State] PST ON PST.Id = E.ParmStateId
                            LEFT JOIN SCS.District PDS ON PDS.Id = E.ParmDistrictID
                            LEFT JOIN SCS.City PCT ON PCT.Id = E.ParmCityID
                            LEFT JOIN SCS.PoliceStation PPS ON PPS.Id = E.ParmThanaID
                            LEFT JOIN SCS.Country PRC ON PRC.Id = E.PresCountryID
                            LEFT JOIN SCS.[State] PRST ON PRST.Id = E.PresStateId
                            LEFT JOIN SCS.District PRD ON PRD.Id = E.PresDistrictID
                            LEFT JOIN SCS.City PRCT ON PRCT.Id = E.PresCityID
                            LEFT JOIN SCS.PoliceStation PRPS ON PRPS.Id = E.PresThanaID
                            LEFT JOIN JobLocation J ON J.systemid = E.JobLocationID
							LEFT JOIN hkp.Designation egdsg ON egdsg.id = e.GivenDesignationId
                            LEFT JOIN HKP.LegalDesignation ld ON ld.Id = e.LegalDesignationId
							LEFT JOIN MST.DesignationMasterLegalDesignation DMLD ON DMLD.LegalDesignationId=E.LegalDesignationId
							LEFT JOIN mst.DesignationMaster DGM ON DGM.Id = DMLD.DesignationMasterId
							LEFT JOIN [HKP].[EmployeeCategory] EC ON EC.Id=DGM.EmployeeCategoryId
							LEFT JOIN HKP.DesignationGroup DG ON DG.Id = DGM.DesignationGroupId
							LEFT JOIN SCS.DesignationMasterConfiguration DC ON DGM.Id = DC.DesignationMasterId AND DC.PlantId=E.PlantId
							LEFT JOIN SalaryRuleMaster srm ON srm.SystemId = dc.SalaryRuleMasterId
	                        LEFT JOIN [dbo].[AttdnBonusPmtPolicyMaster] abpm ON abpm.ID = dc.AttdnBonusPmtPolicyMasterId
                            LEFT JOIN [dbo].[EmployeeFPInformation] FP ON FP.EmpSystemId=e.SystemId
                                AND FP.Id=(SELECT TOP 1 ID FROM [dbo].[EmployeeFPInformation] EII WHERE EII.EmpSystemId=e.SystemId)
							LEFT JOIN [MST].[PaidHoursEmployeeAssign] PD ON PD.EmployeeId = e.SystemId
							LEFT JOIN HKP.EmployeeLocation ELC ON ELC.Id = mpb.EmployeeLocationId
							LEFT JOIN dbo.ShiftDefination SD ON SD.SystemID = mpb.ShiftDefinationId
                            LEFT JOIN [ORG].[Line] BL ON BL.Id = mpb.LineId
							LEFT JOIN ORG.Entity en ON en.Id = mpb.EntityId
							LEFT JOIN ORG.Plant p ON p.id = en.PlantId
							LEFT JOIN ORG.Division div ON div.id = en.DivisionId
                            LEFT JOIN ORG.SubDivision sdiv ON sdiv.id = en.SubDivisionId
							LEFT JOIN ORG.Unit u ON u.id = en.UnitId

							LEFT JOIN ORG.Position ps ON ps.Id = mpb.PositionId
                            LEFT JOIN ORG.Division pdiv ON pdiv.id = ps.DivisionId
                            LEFT JOIN ORG.SubDivision psdiv ON psdiv.id = ps.SubDivisionId
							LEFT JOIN ORG.Department pdept ON pdept.id = ps.DepartmentId                            
                            LEFT JOIN ORG.Section xps ON xps.id = ps.SectionId
                            LEFT JOIN ORG.SubSection pss ON pss.id = ps.SubSectionId
                            LEFT JOIN HKP.Designation dsg ON dsg.id = ps.DesignationId

							 LEFT JOIN [dbo].[EmployeeAttendanceGroup] eag ON eag.EmployeeId = e.SystemId
							 AND eag.Id=(SELECT TOP 1 ID FROM [dbo].[EmployeeAttendanceGroup] EAGM WHERE EAGM.EmployeeId=e.SystemId)
                            LEFT JOIN [dbo].[AttendanceGroup] ag ON ag.Id = eag.AttendanceGroupId
                            LEFT JOIN [HKP].[EmployeeAddInfoDetail] EADD ON EADD.Id = e.CasteId
							  LEFT JOIN MST.PayrollGroupMaster PGM ON PGM.EmployeeId = E.SystemId
							  AND PGM.Id=(SELECT TOP 1 ID FROM MST.PayrollGroupMaster EPGM WHERE EPGM.EmployeeId=e.SystemId)
                              LEFT JOIN HKP.PayrollGroup PG ON PG.ID = PGM.PayrollGroupId
                            LEFT JOIN (
	                            SELECT LSGD.PlantId, LSGD.LegalDesignationId, LS.UserName Grade
	                            FROM [MST].[LegalSalaryGradeDesignation] LSGD
	                            JOIN [SCS].[LegalSalaryGrade] LS ON LS.Id = LSGD.LegalSalaryGradeId and ls.PlantId=lsgd.PlantId
	                            ) GD ON GD.PlantId = E.PlantId AND GD.LegalDesignationId = E.LegalDesignationId
                        LEFT JOIN (
	                            SELECT ed.EmpSystemID, ed.DocNumber
	                            FROM EmployeeDocument ed WHERE ed.ComplianceDocumentId = (
			                            SELECT TOP (1) Id FROM hkp.ComplianceDocument WHERE ProfileType = 'PF'
			                            )
	                            ) PF ON PF.EmpSystemID = E.SystemId
                            LEFT JOIN (
	                            SELECT ed.EmpSystemID, ed.DocNumber
	                            FROM EmployeeDocument ed WHERE ComplianceDocumentId = (
			                            SELECT Id FROM hkp.ComplianceDocument WHERE ProfileType = 'ESIC'
			                            )
	                            ) ESIC ON ESIC.EmpSystemID = E.SystemId

								LEFT JOIN (
	                            SELECT IsEligible, SalaryStructureId, EmpSystemId, m.EffectiveDate
	                            FROM [dbo].[EmployeeEligibleForSalaryHeadEnum] n
	                            LEFT JOIN (
		                            SELECT SystemID, EffectiveDate, EmpInfoSystemID
		                            FROM SalaryInfoDefineMaster
		                            UNION
		                            SELECT SystemID, EffectiveDate, EmpInfoSystemID
		                            FROM SalaryInfoBackMaster
		                            ) mm ON mm.SystemID = n.SalaryStructureId
	                            INNER JOIN (
		                            SELECT MAX(EffectiveDate) EffectiveDate, EmpInfoSystemID
		                            FROM (
			                            SELECT EffectiveDate, EmpInfoSystemID
			                            FROM SalaryInfoDefineMaster
			                            WHERE IsApproved = 1 AND EffectiveDate <= GETDATE()
			                            UNION
			                            SELECT EffectiveDate, EmpInfoSystemID
			                            FROM SalaryInfoBackMaster
			                            WHERE IsApproved = 1 AND EffectiveDate <= GETDATE()
			                            ) x
		                            GROUP BY EmpInfoSystemID
		                            ) m ON mm.EffectiveDate = m.EffectiveDate AND m.EmpInfoSystemID = mm.EmpInfoSystemID
	                            WHERE SalaryHeadEnum = 'PF' AND IsEligible = 1
	                            ) PFE ON PFE.EmpSystemID = E.SystemId
                            LEFT JOIN (
	                            SELECT IsEligible, SalaryStructureId, EmpSystemId, m.EffectiveDate
	                            FROM [dbo].[EmployeeEligibleForSalaryHeadEnum] n
	                            LEFT JOIN (
		                            SELECT SystemID, EffectiveDate, EmpInfoSystemID
		                            FROM SalaryInfoDefineMaster
		                            UNION
		                            SELECT SystemID, EffectiveDate, EmpInfoSystemID
		                            FROM SalaryInfoBackMaster
		                            ) mm ON mm.SystemID = n.SalaryStructureId
	                            INNER JOIN (
		                            SELECT MAX(EffectiveDate) EffectiveDate, EmpInfoSystemID
		                            FROM (
			                            SELECT EffectiveDate, EmpInfoSystemID
			                            FROM SalaryInfoDefineMaster
			                            WHERE IsApproved = 1 AND EffectiveDate <= GETDATE()
			                            UNION
			                            SELECT EffectiveDate, EmpInfoSystemID
			                            FROM SalaryInfoBackMaster
			                            WHERE IsApproved = 1 AND EffectiveDate <= GETDATE()
			                            ) x
		                            GROUP BY EmpInfoSystemID
		                            ) m ON mm.EffectiveDate = m.EffectiveDate AND m.EmpInfoSystemID = mm.EmpInfoSystemID
	                            WHERE SalaryHeadEnum = 'ESIC' AND IsEligible = 1
	                            ) ESICE ON ESICE.EmpSystemID = E.SystemId
                            LEFT JOIN PlantWiseHRMSSetting hs ON hs.PlantID = e.PlantId
LEFT JOIN (SELECT R.EmpSystemID,B.EmployeeCode RefEmpCode,R.Ref1Name,R.Ref1CellPhnNo FROM [dbo].[EmpReferenceInformation] R LEFT JOIN dbo.EmployeeInformation B ON B.SystemId=R.RefEmpSystemID) REF ON REF.EmpSystemID=E.SystemId
                            WHERE E.EmpType <> 'Guest'  " + wc + @" " + CS + @" " + plant + @"
                            ORDER BY ISNULL(e.EmployeeCodePreFix,''), e.EmployeeCodeNumeric";

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
        }//End Function
        #endregion--------------------------------------------Xls Report End----------------------------------------------------
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, string Text)
        {
            //if (string.IsNullOrEmpty(Text) == false)
            //{
            sheet.Range[xlsRow, xlsCol].Text = Text;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            //}
        }
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, double Number)
        {
            //if (string.IsNullOrEmpty(Text) == false)
            //{
            sheet.Range[xlsRow, xlsCol].Number = Number;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            //}
        }

        private void SetHeadText(IWorksheet sheet, int xlsRow, int xlsCol, string text)
        {
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
        }
        private void SetHeadText(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.AliceBlue;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ColIndex = xlsCol;
            xlsCol += 1;
        }

        private void SetHeaderValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            //sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void CreateDynamicMonthHead(DataTable dtMonthList, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColStart, out List<FiscalYearMonthSequence> list)
        {
            try
            {
                list = new List<FiscalYearMonthSequence>();
                _total_head_count = 0;

                int countGross = 0;
                string grossFormula = "";
                string deductionFormula = "";
                for (int ci = 0; ci < dtMonthList.Rows.Count; ci++)
                {
                    _total_head_count++;
                    countGross++;
                    sheet1.Range[xlsRow, ColStart + countGross].Text = dtMonthList.Rows[ci]["MonthName"].ToString().Substring(0, 3) + "," + dtMonthList.Rows[ci]["MonthYear"].ToString().Substring(2, 2);
                    sheet1.Range[xlsRow, ColStart + countGross].ColumnWidth = 8;
                    sheet1.Range[xlsRow, ColStart + countGross].CellStyle.Font.Bold = true;
                    //sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                    sheet1.Range[xlsRow, ColStart + countGross].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColStart + countGross].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, ColStart + countGross].BorderAround(ExcelLineStyle.Thin);

                    FiscalYearMonthSequence fiscalYearMonthSequence = new FiscalYearMonthSequence();
                    fiscalYearMonthSequence.MonthName = dtMonthList.Rows[ci]["MonthName"].ToString();
                    fiscalYearMonthSequence.MonthNo = dtMonthList.Rows[ci]["MonthNumber"].ToString();
                    fiscalYearMonthSequence.LastDayOfMonth = dtMonthList.Rows[ci]["LastDayOfMonth"].ToString();
                    fiscalYearMonthSequence.MonthYear = dtMonthList.Rows[ci]["MonthYear"].ToString();
                    fiscalYearMonthSequence.XLColIndex = ColStart + countGross;

                    list.Add(fiscalYearMonthSequence);
                    xlsCol += 1;
                }//for         
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
