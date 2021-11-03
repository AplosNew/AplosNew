#region Using
using clsAttendance;
using Library.Core;
using Library.Service.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.HumanResources;
using Library.Model.Setups;
using Library.Service.Helpers;
using Library.Service;
using Library.ViewModel.Organizations;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using static Library.Service.Helpers.ReportUtility;
using System.Reflection;
using Library.Service.Enums;
using System.IO;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using Library.Service.Systems;
using Microsoft.Reporting.WinForms;
using ConnectionManager;
using Library.Model.Enums;
#endregion Using

namespace Library.Service.HumanResources
{
    public class PayRegisterBDReportService : Service<PlantWiseSalaryRegisterSortingParameters>, IPayRegisterBDReportService
    {

        //        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        //private readonly IRepositoryAsync<EmployeeInformation> _EmployeeInformationRepository;
        //private readonly IRepositoryAsync<PlantWiseSalaryRegisterSortingParameters> _PlantWiseSalaryRegisterSortingParameters;


        public PayRegisterBDReportService(
             IRepositoryAsync<PlantWiseSalaryRegisterSortingParameters> PlantWiseSalaryRegisterSortingParameters
             , IPKGeneratorService pkGeneratorService
             //IRepositoryAsync<EmployeeInformation> EmployeeInformationRepository
             //, 
             , IUnitOfWork unitOfWork
             , ISqlRepository sqlRepository
            ) : base(PlantWiseSalaryRegisterSortingParameters, unitOfWork, pkGeneratorService)
        {

            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        //        #endregion Constructor


        //        public IWorkbook EmployeeSalaryRegister(PayRegisterParamList PayRegisterParam, string paymentDate, string sqlInStatement, string sheetBasedOn, bool isActive, bool isSeperated, bool isMaternity)
        //        {
        //            string plantId = PayRegisterParam.PlantId; string unitId = PayRegisterParam.UnitId; string divisionId = PayRegisterParam.DivisionId;
        //            string departmentId = PayRegisterParam.DepartmentId; string sectionId = PayRegisterParam.SectionId;
        //            string subsectionId = PayRegisterParam.SubSectionId; string month = PayRegisterParam.Month;
        //            string year = PayRegisterParam.Year; string salaryProcessId = PayRegisterParam.SalaryProcessId;
        //            string empSystmId = PayRegisterParam.EmployeeId; string empStatus = PayRegisterParam.EmpStatus;
        //            string payGroup = PayRegisterParam.PayGroup; string userId = PayRegisterParam.userId;
        //            string categoryId = PayRegisterParam.EmpCategoryId; string paymentMode = PayRegisterParam.PaymentMode;
        //            string languageId = PayRegisterParam.LanguageId;
        //            int EmpCounter = 0;

        //            #region Variable
        //            clsReport objRpt = null;
        //            clsSalaryUtility objSalary = null;

        //            DataSet dsSlrStruct = null;
        //            DataSet dsSlrProced = null;
        //            DataSet dsLeaveInfo = null;
        //            DataSet dsLeaveType = null;
        //            DataSet dsEmpAttdnInfo = null;
        //            DataView dvEmp = null;
        //            DataView dvLeaveEmp = null;
        //            DataView dvLeaveType = null;
        //            DataView dvSlrProc = null;
        //            DataSet dsCmp = null;
        //            DataSet dsFactory = null;
        //            DataSet dsBonus = null;

        //            ExcelEngine excelEngine = null;
        //            IApplication application = null;
        //            IWorkbook workbook = null;
        //            IWorksheet sheet1 = null;
        //            ReportUtility ru = null;
        //            var grossColIndex = 0;
        //            var CTCColIndex = 0;
        //            ParamList para = new ParamList();
        //            ParamList leavePara = new ParamList();
        //            ParamList attdnProcessParam = new ParamList();

        //            var sUnit = unitId; var sDevi = divisionId; var sDept = departmentId; var sSect = sectionId; var sSbSect = subsectionId;
        //            var remCol = 0;

        //            para.UnitId = unitId; para.DivisionId = divisionId; para.DepartmentId = departmentId; para.SectionId = sectionId;
        //            para.SubSectionId = subsectionId; para.PlantId = plantId; para.EmpCategorId = categoryId;
        //            para.LanguageId = languageId;
        //            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;

        //            #endregion Variable

        //            try
        //            {
        //                ru = new ReportUtility();
        //                objRpt = new clsReport();
        //                objSalary = new clsSalaryUtility();
        //                ParaMontlyAttendance objm = new ParaMontlyAttendance();
        //                #region Variable             

        //                var FactoryName = "";
        //                var CmpName = "";

        //                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
        //                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
        //                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
        //                var fdateOfMonth = "1" + "-" + monthName + "-" + year;

        //                var labelList = ru.LocalLanguageLabelList(para.PlantId, languageId);
        //                var DTpayGroup = payRollGroup(payGroup);
        //                var DTEmployeeCatg = EmpCategory(categoryId);

        //                var localLanguage = "";
        //                var payGroupName = "";
        //                var empCategory = "";
        //                var printFont = "";
        //                bool isLocalLanguage = false;
        //                localLanguage = ru.LocalLanguageListSql(plantId, languageId, out isLocalLanguage);
        //                if (localLanguage == "Bengali")
        //                {
        //                    printFont = "SolaimanLipi";
        //                }
        //                else
        //                {
        //                    printFont = "Arial Narrow";

        //                }
        //                if (DTpayGroup.Rows.Count > 0)
        //                {
        //                    payGroupName = DTpayGroup.Rows[0]["UserName"].ToString();
        //                }
        //                if (DTEmployeeCatg.Rows.Count > 0)
        //                {
        //                    empCategory = DTEmployeeCatg.Rows[0]["UserName"].ToString();
        //                }


        //                DataView dvDaily = null;
        //                objm.AMonth = month;
        //                objm.AYear = year;
        //                objm.PlantId = plantId;
        //                objm.FDate = fdateOfMonth;
        //                objm.TDate = ldateOfMonth;
        //                var _ShiftCode = string.Empty;

        //                var salarySheetValue = 0.00;

        //                para.PlantId = plantId;

        //                leavePara.PlantId = plantId;

        //                para.EmployeeId = empSystmId;
        //                para.FromDate = fdateOfMonth;
        //                para.ToDate = ldateOfMonth;
        //                para.SalaryProcessId = salaryProcessId;
        //                para.EmpStatus = empStatus;
        //                para.PayGroup = payGroup;
        //                para.SubSectionId = subsectionId;
        //                para.SectionId = sectionId;



        //                leavePara.EmployeeId = empSystmId;
        //                leavePara.FromDate = fdateOfMonth;
        //                leavePara.SalaryProcessId = salaryProcessId;
        //                leavePara.EmpStatus = empStatus;
        //                #endregion Variable
        //                DateTime dtFrmDt = DateTime.Now;
        //                DateTime dtEndDate = DateTime.Now;

        //                string m = ru.GetMonthName(month);

        //                #region DataSet

        //                List<SalaryStructureReport> listdsSlrStruct = new List<SalaryStructureReport>();//SalaryStruct
        //                List<SalarySheetReportStructure> listdsSlrStructReport = new List<SalarySheetReportStructure>();//SalaryStruct

        //                List<SalarySheetReport> listdsSlrSheet = new List<SalarySheetReport>();//SalarySheetReport

        //                //SalaryRegisterSorting
        //                string stringSalaryRegSorting = "";
        //                stringSalaryRegSorting = objRpt.GetSortingParameters(para.CompanyGroupId, para.CompanyId, para.PlantId, "");

        //                objRpt.GetEmpSalaryStructureRegisterAndPaySlipRpt(para, paymentMode, out dsSlrStruct);  //Salary Structure
        //                objRpt.GetSalaryInfoSlrProcIDWiseForRegister(para, paymentMode, languageId, sqlInStatement, stringSalaryRegSorting, isActive, isSeperated, isMaternity, out dsSlrProced);

        //                dvEmp = new DataView();
        //                dvEmp.Table = dsSlrProced.Tables[0];
        //                var dtEmployees = dvEmp.ToTable(true, "EmpInfoSystemID", "DivisionName", "DivisionId", "SubDivision", "SubdivisionID", "UnitName", "UnitID", "DepartmentName", "DepartmentID", "SectionName", "SectionID", "SubSectionName", "SubSectionID", "LDDesignationGD", "DesignationLocal", "GradeCode", "DOJ", "EmployeeName", "EmployeeNameLocal", "FatherName", "DOS", "EmployeeCode", "BankAccNo", "IsOTEntitle");
        //                if (dtEmployees.Rows.Count == 0)
        //                {
        //                    var ex = new Exception("No Data found...");
        //                    throw (ex);
        //                }

        //                //if (dsSlrStruct.Tables[0].Rows.Count > 0)
        //                //{
        //                //    listdsSlrStruct = dsSlrStruct.Tables[0].ToList<SalaryStructureReport>();
        //                //}

        //                dvSlrProc = new DataView();
        //                dvSlrProc.Table = dsSlrProced.Tables[0];
        //                if (dsSlrProced.Tables[0].Rows.Count > 0)
        //                {
        //                    listdsSlrSheet = dsSlrProced.Tables[0].ToList<SalarySheetReport>();
        //                }

        //                if (dsSlrProced.Tables[0].Rows.Count > 0)
        //                {
        //                    listdsSlrStructReport = dsSlrProced.Tables[0].ToList<SalarySheetReportStructure>();
        //                }

        //                DataTable dsDaily = GetMonthlyDailyAttendance(_ShiftCode, objm);
        //                List<SwapColumn> _list2 = GetColDisplayName(dsDaily);


        //                objRpt.SelectedPlantWiseCompany(plantId, languageId, out dsCmp);

        //                objRpt.SelectedPlant(plantId, out dsFactory);

        //                #endregion DataSet

        //                excelEngine = new ExcelEngine();
        //                application = excelEngine.Excel;

        //                workbook = application.Workbooks.Create(1);
        //                sheet1 = workbook.Worksheets[0];
        //                sheet1.IsGridLinesVisible = true;
        //                sheet1.IsDisplayZeros = false;
        //                #region------------------Column Header------------------
        //                xlsRow = 5;
        //                xlsCol = 1;

        //                var ColSr = 0;
        //                var ColName = 0;
        //                var ColLeaveInfo = 0;
        //                var ColWorkDaysInfo = 0;
        //                var colParticulars = 0;
        //                var ColGrs = 0;
        //                #endregion------------------Column Header------------------

        //                int RowIndex = xlsRow + 1;

        //                #region ----------------------Data-----------------------
        //                var strSubDivision = "0";
        //                var strSection = "0";
        //                var strDiv = "0";
        //                var strUnit = "0";
        //                var strDepartment = "0";
        //                var strSubSection = "0";

        //                var SrNo = 0;
        //                var EmpIdPR = "";
        //                var oRU = new ReportUtility();
        //                var intRow = 0;
        //                xlsRow = RowIndex;

        //                List<SalaryHeadSequence> list = null;

        //                var np = 0;
        //                var isFirst = true;
        //                var sigCol = 0;
        //                var deptFirstRow = 0;

        //                xlsRow--;


        //                var totalDictSalaryStruct = new Dictionary<string, double>();
        //                var totalDictSalaryProcess = new Dictionary<string, double>();


        //                int endCol = 5;
        //                int colNetpayable = endCol;
        //                int colSignature = endCol;
        //                #region RegisterHeader
        //                var colex = 0;
        //                sigCol = 0;




        //                if (isFirst == false)
        //                {
        //                    xlsRow += 3;
        //                    colex = 2;
        //                }
        //                #region ------------------Column Header------------------
        //                xlsCol = 1;
        //                var lineHeader = "";
        //                if (!paymentMode.IsNullOrEmpty() && paymentMode != "null" && paymentMode != "undefined")
        //                    lineHeader += paymentMode;
        //                if (!empCategory.IsNullOrEmpty())
        //                {
        //                    if (!paymentMode.IsNullOrEmpty() && paymentMode != "null" && paymentMode != "undefined")
        //                    {
        //                        lineHeader += "~";
        //                    }
        //                    lineHeader += empCategory;

        //                }
        //                if (!payGroup.IsNullOrEmpty())
        //                {
        //                    if (!paymentMode.IsNullOrEmpty() && paymentMode != "null" && paymentMode != "undefined")
        //                    {
        //                        lineHeader += "~";
        //                    }
        //                    if (!empCategory.IsNullOrEmpty())
        //                    {
        //                        lineHeader += "~";
        //                    }
        //                    lineHeader += payGroupName;

        //                }

        //                sheet1.Range[xlsRow - 1, xlsCol].Text = lineHeader;
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.Size = 48;
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow - 1, xlsCol, xlsRow - 1, xlsCol + 3].Merge();
        //                sheet1.Range[xlsRow - 1, xlsCol].RowHeight = 52;

        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.SrNo.ToString(), "Sr. No."), sheet1, xlsRow + colex, ref xlsCol, out ColSr, 15, printFont, 90);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.EmployeeInformation.ToString(), "Employee Information"), sheet1, xlsRow + colex, ref xlsCol, out ColName, 100, printFont, 0);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.LeaveInformation.ToString(), "Leave Information"), sheet1, xlsRow + colex, ref xlsCol, out ColLeaveInfo, 35, printFont, 0);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.WorkDaysDetail.ToString(), "Work Days Detail"), sheet1, xlsRow + colex, ref xlsCol, out ColWorkDaysInfo, 60, printFont, 0);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.Particulars.ToString(), "Particulars"), sheet1, xlsRow + colex, ref xlsCol, out colParticulars, 28, printFont, 0);
        //                ColGrs = colParticulars;
        //                var _count_earning_head = 0;
        //                var _count_earning_ctchead = 0;
        //                var _count_deducting_head = 0;
        //                var _total_head_count = 0;

        //                DataView dvSalaryHead = new DataView(dsSlrProced.Tables[0]);

        //                dvSalaryHead.Sort = "HeadType desc,Sequence";

        //                DataTable dtSalaryHead = dvSalaryHead.ToTable(true, "SalaryHeadID", "SalaryHead", "SalaryHeadBangla", "HeadType", "Sequence", "HeadCategory", "IsCTCComponent", "IsGrossComponent", "IntegerInDisb", "DecimalNo");

        //                DataTable dtVPFHead = dvSalaryHead.ToTable(true, "xHeadCategory", "xSalaryHeadID", "xSalaryHead", "xIsCTCComponent", "xIsGrossComponent", "xHeadType", "IntegerInDisb", "DecimalNo");

        //                objRpt.GetBonus(month, year, out dsBonus);
        //                DataView dvBonus = new DataView(dsBonus.Tables[0]);
        //                DataTable dtBonusHead = dvBonus.ToTable(true, "SalaryHeadID", "HeadCategory", "SalaryHead", "IsCTCComponent", "IsGrossComponent", "HeadType", "Sequence");

        //                OTSBD.clsSalary.clsSalaryReport sr = new OTSBD.clsSalary.clsSalaryReport();

        //                sr.SetSheetBonus(dtBonusHead, ref dtSalaryHead);

        //                xlsRow += colex;

        //                CreateDynamicSHeadLocalLanguage(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list, labelList, printFont);
        //                xlsRow -= colex;
        //                xlsRow += intRow;
        //                intRow = 1;
        //                endCol = 5;

        //                #region Day of a month 
        //                var mnthCol = 0;
        //                mnthCol = colParticulars;

        //                var dtFrmDtInt = 1;
        //                var dtEndDateInt = 31;
        //                while (dtFrmDtInt <= dtEndDateInt)
        //                {
        //                    mnthCol += 1;
        //                    var sc = _list2.Find(r => Convert.ToInt32(r.ValueMember) == dtFrmDtInt);

        //                    var _col_index = mnthCol;
        //                    sheet1.Range[xlsRow, _col_index].Text = dtFrmDtInt.ToString();
        //                    sheet1.Range[xlsRow, _col_index].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
        //                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Bold = true;
        //                    sheet1.Range[xlsRow, _col_index].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                    sheet1.Range[xlsRow, _col_index].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.FontName = "Arial Narrow";
        //                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Size = 30;
        //                    sheet1.Range[xlsRow, _col_index].ColumnWidth = 13;
        //                    dtFrmDtInt++;
        //                }
        //                #endregion
        //                endCol = colParticulars;
        //                if (list.Count > 0)
        //                {
        //                    xlsCol++;
        //                    np = ColGrs + list.Count * 2;
        //                    endCol = np + 1;


        //                    sheet1.Range[xlsRow + 1, np - 1].Text = ru.GetLabelname(labelList, "TotalDeduction", "Total Deduction"); //totalDeduction.LocalLabel == null || totalDeduction.LocalLabel == "" ? totalDeduction.DefaultLabel : totalDeduction.LocalLabel;
        //                    sheet1.Range[xlsRow + 1, np - 1].CellStyle.ShrinkToFit = true;
        //                    sheet1.Range[xlsRow + 1, np - 1].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow + 1, np - 1].CellStyle.Font.Size = 22;
        //                    sheet1.Range[xlsRow + 1, np - 1, xlsRow + 1, np].Merge();
        //                    xlsCol++;
        //                }

        //                if (endCol <= colParticulars + 29)
        //                    endCol = colParticulars + 30;

        //                colNetpayable = endCol;

        //                sheet1.Range[xlsRow + 1, endCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.NetPayable.ToString(), "Net Payable");//netPayable.LocalLabel == null || netPayable.LocalLabel == "" ? netPayable.DefaultLabel : netPayable.LocalLabel;
        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();
        //                sheet1.Range[xlsRow + 1, endCol].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow + 1, endCol].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 1, colNetpayable].ColumnWidth = 18;
        //                sheet1.Range[xlsRow + 1, colNetpayable + 1].ColumnWidth = 18;

        //                colSignature = colNetpayable + 2;

        //                sheet1.Range[xlsRow, colSignature].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Signature.ToString(), "Signature");//empSignatgure.LocalLabel == null || empSignatgure.LocalLabel == "" ? empSignatgure.DefaultLabel : empSignatgure.LocalLabel;
        //                sheet1.Range[xlsRow, colSignature, xlsRow + 1, colSignature].Merge();
        //                sheet1.Range[xlsRow, colSignature].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, colSignature].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow, colSignature].ColumnWidth = 70;




        //                endCol = colSignature;

        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].BorderAround(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].BorderInside(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

        //                //sheet1.Range[xlsRow + 1, 1, xlsRow + 1, endCol].RowHeight = 127;

        //                endXlsCol = endCol;
        //                sigCol = endCol;
        //                xlsCol = 1;
        //                xlsRow++;
        //                EmpCounter = 0;

        //                deptFirstRow = xlsRow;
        //                #endregion

        //                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
        //                {
        //                    salarySheetValue = 0.00;
        //                    #region *****************Dept wise Subtotal Calculation*********************

        //                    #endregion*****************END OF Dept wise Subtotal Calculation*********************

        //                    #endregion ------------------Column Header------------------
        //                    //}
        //                    #region *************************Data*************************
        //                    strDiv = dtEmployees.Rows[i]["DivisionID"].ToString().Trim();
        //                    strSubDivision = dtEmployees.Rows[i]["SubdivisionID"].ToString().Trim();
        //                    strUnit = dtEmployees.Rows[i]["UnitID"].ToString().Trim();
        //                    strDepartment = dtEmployees.Rows[i]["DepartmentID"].ToString().Trim();
        //                    strSection = dtEmployees.Rows[i]["SectionID"].ToString().Trim();
        //                    strSubSection = dtEmployees.Rows[i]["SubSectionID"].ToString().Trim();

        //                    xlsRow++;
        //                    #region LeaveInformation
        //                    leavePara.EmployeeId = dtEmployees.Rows[i]["EmpInfoSystemID"].ToString();

        //                    objRpt.GetLeaveTypeLocal(leavePara, languageId, out dsLeaveType);
        //                    dvLeaveType = new DataView();

        //                    dvLeaveType.Table = dsLeaveType.Tables[0];

        //                    objRpt.GetEmpLeaveInfo(leavePara, out dsLeaveInfo);
        //                    dvLeaveEmp = new DataView();

        //                    dvLeaveEmp.Table = dsLeaveInfo.Tables[0];
        //                    var EL = GetLeaveEmp(dvLeaveEmp, "EL");
        //                    var CL = GetLeaveEmp(dvLeaveEmp, "CL");
        //                    var SL = GetLeaveEmp(dvLeaveEmp, "SL");

        //                    var locEL = GetLeaveType(dvLeaveType, "EL");
        //                    var locCL = GetLeaveType(dvLeaveType, "CL");
        //                    var locSL = GetLeaveType(dvLeaveType, "SL");


        //                    #endregion


        //                    attdnProcessParam.EmployeeId = dtEmployees.Rows[i]["EmpInfoSystemID"].ToString();
        //                    objRpt.GetMonthlyAttdnInfo(attdnProcessParam, month, year, out dsEmpAttdnInfo);

        //                    DataTable dtEmpAttdnInfo = dsEmpAttdnInfo.Tables[0];



        //                    #region EmpInfo
        //                    SrNo += 1;
        //                    EmpIdPR = dtEmployees.Rows[i]["EmpInfoSystemID"].ToString().Trim();

        //                    sheet1.Range[xlsRow, ColSr].Text = ru.cnDgt(Convert.ToString(SrNo), localLanguage);
        //                    sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                    sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    if (sheetBasedOn == "structured")
        //                    {
        //                        sheet1.Range[xlsRow, ColSr, xlsRow + 5, ColSr].Merge();
        //                    }
        //                    else
        //                    {
        //                        sheet1.Range[xlsRow, ColSr, xlsRow + 4, ColSr].Merge();
        //                    }
        //                    sheet1.Range[xlsRow, ColSr].CellStyle.Font.FontName = "Arial Narrow";
        //                    sheet1.Range[xlsRow, ColSr].CellStyle.Font.Size = 25;

        //                    //3
        //                    var _DOJ = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOJ.ToString(), "DOJ");
        //                    var _DOS = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOS.ToString(), "DOS");
        //                    var _designationLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Designation.ToString(), "Designation");
        //                    var _gradeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Grade.ToString(), "Grade");
        //                    var empName = "";
        //                    if (isLocalLanguage)
        //                    {
        //                        empName = dtEmployees.Rows[i]["EmployeeNameLocal"].ToString();
        //                    }
        //                    else
        //                    {
        //                        empName = dtEmployees.Rows[i]["EmployeeName"].ToString();

        //                    }
        //                    var empDesignation = _designationLocal + ":" + dtEmployees.Rows[i]["DesignationLocal"].ToString();
        //                    var empDOJ = _DOJ + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOJ"].ToString(), localLanguage);
        //                    var bankAccount = "";
        //                    var empDOS = string.Empty;
        //                    var empGross = string.Empty;
        //                    double grossSalaryAmount = 0.00;

        //                    var grossAmountLabel = ru.GetLabelname(labelList, LabelNameInLocalLanguage.TotalSalary.ToString(), "Gross");
        //                    DataView dvEmpGrossInfo = new DataView(dsSlrProced.Tables[0])
        //                    {
        //                        RowFilter = "EmpInfoSystemId = '" + dtEmployees.Rows[i]["EmpInfoSystemId"].ToString() + "' and SalaryHead = 'Gross'",
        //                    };
        //                    //if (dsSlrProced.Tables[0].Rows[i]["EmpInfoSystemId"].ToString() == dtEmployees.Rows[i]["EmpInfoSystemId"].ToString())
        //                    //{

        //                    //    if (dsSlrProced.Tables[0].Rows[i]["SalaryHead"].ToString().ToString() == "GROSS")
        //                    //    {
        //                    grossSalaryAmount = Convert.ToDouble(dvEmpGrossInfo[0]["EntryAmount"]);
        //                    empGross = grossAmountLabel + ":" + ru.cnDgt(grossSalaryAmount.ToString(), localLanguage).ToString();
        //                    //    }

        //                    //}


        //                    if (dtEmployees.Rows[i]["DOS"].ToString().Length > 0)
        //                    {
        //                        empDOS = _DOS + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOS"].ToString(), localLanguage); //+ Environment.NewLine;
        //                    }
        //                    if (dtEmployees.Rows[i]["BankAccNo"].ToString().Length > 0)
        //                    {
        //                        bankAccount = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bank Acc") + ":" + dtEmployees.Rows[i]["BankAccNo"].ToString(); //+ Environment.NewLine;
        //                    }
        //                    sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();

        //                    sheet1.Range[xlsRow + 1, ColName].Text = empName;
        //                    sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                    sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].BorderAround(ExcelLineStyle.Hair);
        //                    sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].CellStyle.Font.Bold = true;
        //                    sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].CellStyle.Font.Size = 50;

        //                    // Designation DOJ DOS
        //                    IRichTextString rtf1 = sheet1.Range[xlsRow + 2, ColName].RichText;
        //                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 2);
        //                    FormatText(ref sheet1, ref rtf1, empGross + " ", 25);
        //                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                    FormatText(ref sheet1, ref rtf1, _designationLocal + ":" + dtEmployees.Rows[i]["DesignationLocal"].ToString() + " ", 25);
        //                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                    FormatText(ref sheet1, ref rtf1, _gradeLocal + ":", 25); FormatText(ref sheet1, ref rtf1, dtEmployees.Rows[i]["GradeCode"].ToString() + " ", 25);
        //                    if (dtEmployees.Rows[i]["BankAccNo"].ToString().Length > 0)
        //                    {
        //                        bankAccount = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bank Acc No") + ":" + dtEmployees.Rows[i]["BankAccNo"].ToString(); //+ Environment.NewLine;
        //                        FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf1, bankAccount + " ", 25);
        //                    }

        //                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                    FormatText(ref sheet1, ref rtf1, empDOJ + " ", 25);
        //                    if (dtEmployees.Rows[i]["DOS"].ToString().Length > 0)
        //                    {
        //                        empDOS = _DOS + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOS"].ToString(), localLanguage); //+ Environment.NewLine;
        //                        FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf1, empDOS + " ", 25);
        //                    }


        //                    sheet1.Range[xlsRow + 2, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                    sheet1.Range[xlsRow + 2, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    if (sheetBasedOn == "structured")
        //                    {
        //                        sheet1.Range[xlsRow + 2, ColName, xlsRow + 5, ColName].Merge();
        //                        sheet1.Range[xlsRow + 2, ColName, xlsRow + 5, ColName].BorderAround(ExcelLineStyle.Thin);
        //                    }
        //                    else
        //                    {
        //                        sheet1.Range[xlsRow + 2, ColName, xlsRow + 4, ColName].Merge();
        //                        sheet1.Range[xlsRow + 2, ColName, xlsRow + 4, ColName].BorderAround(ExcelLineStyle.Thin);
        //                    }



        //                    //Leave Info
        //                    if (dtEmpAttdnInfo.Rows.Count > 0)
        //                    {
        //                        string lateBangla = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalLate"]).ToString(), localLanguage);
        //                        string presentBangla = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalPresentLate"]).ToString(), localLanguage);
        //                        string absentBangla = ru.cnDgt((Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalAbsent"]) - Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalLWP"])).ToString(), localLanguage);
        //                        string weekOff = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalWeekOffPlusWeekOffHoliDay"]).ToString(), localLanguage);
        //                        string holiDay = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalHoliDay"]).ToString(), localLanguage);
        //                        string leave = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalLv"]).ToString(), localLanguage);
        //                        string totalOTHr = "";//ru.cnDgt((Math.Round((Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalNormalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalExtraOTHr"])) / 60, 2).ToString()), localLanguage);
        //                        var _availedLeave = ru.GetLabelname(labelList, LabelNameInLocalLanguage.AvailedLeave.ToString(), "Availed Leave");


        //                        IRichTextString rtfLeave = sheet1.Range[xlsRow, ColLeaveInfo].RichText;
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locEL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(EL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locCL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(CL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locSL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(SL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, _availedLeave + ":" + ru.cnDgt(Math.Round(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalLv"].ToString()), 0).ToString(), localLanguage) + " ", 25);

        //                        sheet1.Range[xlsRow, ColLeaveInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                        sheet1.Range[xlsRow, ColLeaveInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        if (sheetBasedOn == "structured")
        //                        {
        //                            sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 5, ColLeaveInfo].Merge();
        //                            sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 5, ColLeaveInfo].BorderAround(ExcelLineStyle.Thin);
        //                        }
        //                        else
        //                        {

        //                            sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 4, ColLeaveInfo].Merge();
        //                            sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 4, ColLeaveInfo].BorderAround(ExcelLineStyle.Thin);
        //                        }


        //                        string payDays = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalProcDate"]).ToString(), localLanguage);
        //                        DataView dvBasic = new DataView(dsSlrProced.Tables[0]);

        //                        double otRate = 0.00;
        //                        string otRateBangla = "";//IsOTEntitled
        //                        if (Convert.ToBoolean(dtEmpAttdnInfo.Rows[0]["IsOTEntitled"]) == true)
        //                        {
        //                            totalOTHr = ru.cnDgt((Math.Round((Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalNormalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalExtraOTHr"])) / 60, 2).ToString()), localLanguage);
        //                            otRate = Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["OTRate"]);
        //                            otRateBangla = ru.cnDgt(Math.Round(otRate, 2).ToString(), localLanguage);

        //                        }
        //                        IRichTextString rtf = sheet1.Range[xlsRow, ColWorkDaysInfo].RichText;
        //                        var _payDaysLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.PayDays.ToString(), "Pay Days");
        //                        var _presentLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Present.ToString(), "Present");
        //                        var _absentLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Absent.ToString(), LabelNameInLocalLanguage.Absent.ToString());//absentLocal.LocalLabel == null || absentLocal.LocalLabel == "" ? absentLocal.DefaultLabel : absentLocal.LocalLabel;
        //                        var _lunchOutHour = ru.GetLabelname(labelList, LabelNameInLocalLanguage.LunchOutHour.ToString(), "Lunch Out Hr.");//lunchOutHour.LocalLabel == null || lunchOutHour.LocalLabel == "" ? lunchOutHour.DefaultLabel : lunchOutHour.LocalLabel;
        //                        var _late = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Late.ToString(), "Late");
        //                        var _otHour = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OTHours.ToString(), "OT Hrs");
        //                        var _otRateLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OTRate.ToString(), "OT Rate");
        //                        var _weeklyHolidays = ru.GetLabelname(labelList, LabelNameInLocalLanguage.WeeklyLeaveDays.ToString(), "Weekend");
        //                        var _holiDays = ru.GetLabelname(labelList, LabelNameInLocalLanguage.HoliDay.ToString(), "Holidays");

        //                        FormatText(ref sheet1, ref rtf, _payDaysLocal + ":" + payDays + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _presentLocal + ":" + presentBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _absentLocal + ":" + absentBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _weeklyHolidays + ":" + weekOff + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _holiDays + ":" + holiDay + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _lunchOutHour + ":" + "", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _late + ":" + lateBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        if (Convert.ToBoolean(dtEmpAttdnInfo.Rows[0]["IsOTEntitled"]) == true)
        //                        {
        //                            FormatText(ref sheet1, ref rtf, _otHour + ":" + totalOTHr + " ", 27);
        //                            FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                            FormatText(ref sheet1, ref rtf, _otRateLocal + ":" + otRateBangla + " ", 27);
        //                        }
        //                        if (sheetBasedOn == "structured")
        //                        {
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 5, ColWorkDaysInfo].Merge();
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 5, ColWorkDaysInfo].BorderAround(ExcelLineStyle.Thin);
        //                        }
        //                        else
        //                        {
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 4, ColWorkDaysInfo].Merge();
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 4, ColWorkDaysInfo].BorderAround(ExcelLineStyle.Thin);
        //                        }
        //                    }

        //                    var earnedSalary = "";// labelList.Where(r => r.DefaultLabel == LabelNameInLocalLanguage.EarnedSalary.ToString()).FirstOrDefault();
        //                    if (labelList.ContainsKey(LabelNameInLocalLanguage.EarnedSalary.ToString()))
        //                    {
        //                        earnedSalary = labelList[LabelNameInLocalLanguage.EarnedSalary.ToString()];
        //                    }
        //                    if (sheetBasedOn == "structured")
        //                    {
        //                        var _structuredSalary = ru.GetLabelname(labelList, LabelNameInLocalLanguage.StructuredSalary.ToString(), "Str Sal");
        //                        sheet1.Range[xlsRow, colParticulars].Text = _structuredSalary + "->";
        //                        sheet1.Range[xlsRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                        sheet1.Range[xlsRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        sheet1.Range[xlsRow, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                        sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Size = 24;
        //                        sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Bold = true;
        //                        xlsRow++;
        //                    }
        //                    var _earnedSalary = ru.GetLabelname(labelList, LabelNameInLocalLanguage.EarnedSalary.ToString(), "Earned Salary");
        //                    sheet1.Range[xlsRow, colParticulars].Text = _earnedSalary + "->";
        //                    sheet1.Range[xlsRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[xlsRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Size = 24;
        //                    sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Bold = true;

        //                    var _daysLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Days.ToString(), "Days");

        //                    var particular3rdRow = xlsRow + 1;

        //                    var _attendance = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Attendance.ToString(), "Attendance");

        //                    sheet1.Range[particular3rdRow, colParticulars].Text = _attendance + "->";
        //                    sheet1.Range[particular3rdRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[particular3rdRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[particular3rdRow, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[particular3rdRow, colParticulars].RowHeight = 43;
        //                    sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.Size = 17;
        //                    sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.Bold = true;

        //                    var _inTimeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.INTime.ToString(), "INTime");
        //                    var _outTimeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OUTTime.ToString(), "OUTTime");
        //                    var _OTLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OT.ToString(), "OT");

        //                    sheet1.Range[particular3rdRow + 1, colParticulars].Text = _inTimeLocal + "->";
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].RowHeight = 43;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.Size = 17;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                    sheet1.Range[particular3rdRow + 2, colParticulars].Text = _outTimeLocal + "->";
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].RowHeight = 43;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.Size = 17;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.Bold = true;

        //                    sheet1.Range[particular3rdRow + 3, colParticulars].Text = _OTLocal + "->";
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].RowHeight = 43;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.Size = 17;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.Bold = true;
        //                    #endregion
        //                    #region SalaryStructure
        //                    var _total_head_count_body = 0;
        //                    if (sheetBasedOn == "structured")
        //                    {

        //                        for (int ssi = 0; ssi < list.Count; ssi++)
        //                        {
        //                            var ob = list[ssi];
        //                            if (ob.SalaryHead.Length > 0)
        //                            {

        //                                if (ob.SalaryHeadId.ToUpper() == "CTC" || ob.SalaryHeadId.ToUpper() == "GROSS")
        //                                {
        //                                    var formula = ob.SalaryHead;
        //                                    var hId = ob.SalaryHeadId;
        //                                    _total_head_count_body++;

        //                                    sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].Formula = "=" + oRU.SetFormula(formula, xlsRow - 1);
        //                                    sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocal(ob, localLanguage);
        //                                    sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].BorderAround(ExcelLineStyle.Thin);
        //                                    sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                    sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                    sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                                    sheet1.Range[xlsRow - 1, ob.XLColIndex - 1, xlsRow - 1, ob.XLColIndex].Merge();

        //                                    if (ob.SalaryHeadId.ToUpper() == "GROSS")
        //                                    {
        //                                        grossColIndex = ob.XLColIndex;
        //                                    }
        //                                    if (ob.SalaryHeadId.ToUpper() == "CTC")
        //                                    {
        //                                        CTCColIndex = ob.XLColIndex;
        //                                    }

        //                                }//ctc , gross
        //                                else
        //                                {
        //                                    var hId = ob.SalaryHeadId;
        //                                    _total_head_count_body++;

        //                                    var _dataSlrSheetStruct = listdsSlrStructReport.Where(r => r.SalaryHeadID == hId && r.EmpInfoSystemID == EmpIdPR).FirstOrDefault();
        //                                    double _dataSlrSheetNumber = 0;

        //                                    if (ob.HeadType == "D")
        //                                    {
        //                                        if (ob.HeadCategory == "Absenteeism")
        //                                        {
        //                                            _dataSlrSheetNumber = 0.00;
        //                                        }
        //                                        else
        //                                        {
        //                                            if (_dataSlrSheetStruct != null)
        //                                                _dataSlrSheetNumber = (Convert.ToDouble(_dataSlrSheetStruct.EntryAmount.ToString()));
        //                                        }
        //                                        salarySheetValue = 0.00;
        //                                        salarySheetValue = 0.00;
        //                                        sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].Number = salarySheetValue;
        //                                        sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocal(ob, localLanguage);
        //                                        sheet1.Range[xlsRow - 1, ob.XLColIndex - 1, xlsRow - 1, ob.XLColIndex].Merge();
        //                                        sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                                    }
        //                                    else if (ob.HeadType == "E")
        //                                    {
        //                                        DataView dvBonusData = new DataView(dsBonus.Tables[0]);
        //                                        dvBonusData.RowFilter = "SalaryHeadID='" + hId + "' and EmpSystemID='" + EmpIdPR + "'";
        //                                        sheet1.Range[xlsRow, ob.XLColIndex].BorderAround(ExcelLineStyle.Thin);
        //                                        if (dvBonusData.Count > 0)
        //                                        {
        //                                            salarySheetValue = 0.00;
        //                                            salarySheetValue = Convert.ToDouble(dvBonusData[0]["Bonus"].ToString());
        //                                            sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].Number = salarySheetValue;
        //                                            sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocal(ob, localLanguage);
        //                                            sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                                            sheet1.Range[xlsRow - 1, ob.XLColIndex - 1, xlsRow - 1, ob.XLColIndex].Merge();
        //                                        }
        //                                        else
        //                                        {
        //                                            salarySheetValue = 0.00;
        //                                            if (_dataSlrSheetStruct != null)
        //                                                salarySheetValue = Convert.ToDouble(_dataSlrSheetStruct.EntryAmount.ToString());
        //                                            sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].Number = salarySheetValue;
        //                                            sheet1.Range[xlsRow - 1, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocal(ob, localLanguage);
        //                                            sheet1.Range[xlsRow - 1, ob.XLColIndex - 1, xlsRow - 1, ob.XLColIndex].Merge();
        //                                            sheet1.Range[xlsRow - 1, ob.XLColIndex].CellStyle.Font.FontName = printFont;
        //                                        }
        //                                    }

        //                                }
        //                            }//
        //                        }
        //                    }

        //                    #endregion

        //                    #region SalarySheet
        //                    _total_head_count_body = 0;
        //                    for (int ci = 0; ci < list.Count; ci++)
        //                    {
        //                        var ob = list[ci];
        //                        if (ob.SalaryHead.Length > 0)
        //                        {

        //                            if (ob.SalaryHeadId.ToUpper() == "CTC" || ob.SalaryHeadId.ToUpper() == "GROSS")
        //                            {
        //                                var formula = ob.SalaryHead;
        //                                var hId = ob.SalaryHeadId;
        //                                _total_head_count_body++;

        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].Formula = "=" + oRU.SetFormula(formula, xlsRow);
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocal(ob, localLanguage);
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].BorderAround(ExcelLineStyle.Thin);
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1, xlsRow, ob.XLColIndex].Merge();

        //                                if (ob.SalaryHeadId.ToUpper() == "GROSS")
        //                                {
        //                                    grossColIndex = ob.XLColIndex;
        //                                }
        //                                if (ob.SalaryHeadId.ToUpper() == "CTC")
        //                                {
        //                                    CTCColIndex = ob.XLColIndex;
        //                                }

        //                            }//ctc , gross
        //                            else
        //                            {
        //                                var hId = ob.SalaryHeadId;
        //                                _total_head_count_body++;

        //                                #region SalaryProcess 

        //                                var _dataSlrSheet = listdsSlrStructReport.Where(r => r.SalaryHeadID == hId && r.EmpInfoSystemID == EmpIdPR).FirstOrDefault();
        //                                double _dataSlrSheetNumber = 0;

        //                                if (ob.HeadType == "D")
        //                                {
        //                                    if (_dataSlrSheet != null)
        //                                        _dataSlrSheetNumber = (Convert.ToDouble(_dataSlrSheet.DisbusmentAmount.ToString()) * (-1));

        //                                    salarySheetValue = 0.00;
        //                                    salarySheetValue = _dataSlrSheetNumber;
        //                                    sheet1.Range[xlsRow, ob.XLColIndex - 1].Number = salarySheetValue;
        //                                    sheet1.Range[xlsRow, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocal(ob, localLanguage);
        //                                    sheet1.Range[xlsRow, ob.XLColIndex - 1, xlsRow, ob.XLColIndex].Merge();
        //                                    sheet1.Range[xlsRow, ob.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                                    getTotalAmount(ob.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                                }
        //                                else if (ob.HeadType == "E")
        //                                {
        //                                    DataView dvBonusData = new DataView(dsBonus.Tables[0]);
        //                                    dvBonusData.RowFilter = "SalaryHeadID='" + hId + "' and EmpSystemID='" + EmpIdPR + "'";
        //                                    sheet1.Range[xlsRow, ob.XLColIndex].BorderAround(ExcelLineStyle.Thin);
        //                                    if (dvBonusData.Count > 0)
        //                                    {
        //                                        salarySheetValue = 0.00;
        //                                        salarySheetValue = Convert.ToDouble(dvBonusData[0]["Bonus"].ToString());
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1].Number = salarySheetValue;
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocal(ob, localLanguage);
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1, xlsRow, ob.XLColIndex].Merge();
        //                                        getTotalAmount(ob.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                                    }
        //                                    else
        //                                    {
        //                                        salarySheetValue = 0.00;
        //                                        if (_dataSlrSheet != null)
        //                                            salarySheetValue = Convert.ToDouble(_dataSlrSheet.DisbusmentAmount.ToString());
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1].Number = salarySheetValue;
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocal(ob, localLanguage);
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1, xlsRow, ob.XLColIndex].Merge();
        //                                        sheet1.Range[xlsRow, ob.XLColIndex].CellStyle.Font.FontName = printFont;
        //                                        getTotalAmount(ob.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                                    }
        //                                }
        //                                #endregion
        //                            }
        //                        }//


        //                        #region common excel Row set up                            

        //                        sheet1.Range[xlsRow - 1, ob.XLColIndex - 1, xlsRow, ob.XLColIndex - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                        sheet1.Range[xlsRow - 1, ob.XLColIndex - 1, xlsRow, ob.XLColIndex - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        sheet1.Range[xlsRow - 1, ob.XLColIndex - 1, xlsRow, ob.XLColIndex - 1].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow - 1, ob.XLColIndex - 1, xlsRow, ob.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                        sheet1.Range[xlsRow - 1, ob.XLColIndex - 1, xlsRow, ob.XLColIndex - 1].CellStyle.Font.Size = 30;
        //                        #endregion
        //                    }//for dtSalaryHead
        //                    #endregion
        //                    //gross-deduction
        //                    var grossIndex = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
        //                    var CTCIndex = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();

        //                    var dedIndex = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.XLColIndex).FirstOrDefault();
        //                    var dedFormula = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

        //                    var grossAdd = oRU.SetFormula(grossIndex.ToString(), xlsRow);
        //                    var CTCAdd = oRU.SetFormula(CTCIndex.ToString(), xlsRow);


        //                    var grossAddSalProc = oRU.SetFormula((grossIndex - 1).ToString(), xlsRow);
        //                    var CTCAddSalProc = oRU.SetFormula((CTCIndex - 1).ToString(), xlsRow);

        //                    var dedAddSalProc = oRU.SetFormula(dedFormula, xlsRow);

        //                    var grossAddStr = oRU.SetFormula(grossIndex.ToString(), xlsRow - 1);
        //                    var CTCAddStr = oRU.SetFormula(CTCIndex.ToString(), xlsRow - 1);


        //                    var grossAddSalStr = oRU.SetFormula((grossIndex - 1).ToString(), xlsRow - 1);
        //                    var CTCAddSalStr = oRU.SetFormula((CTCIndex - 1).ToString(), xlsRow - 1);

        //                    var dedAddSalStr = oRU.SetFormula(dedFormula, xlsRow - 1);

        //                    sheet1.Range[xlsRow, np - 1].Formula = "=" + dedAddSalProc;
        //                    sheet1.Range[xlsRow, np - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                    sheet1.Range[xlsRow, np - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[xlsRow, np - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, np - 1].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, np - 1].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow, np - 1].CellStyle.Font.Size = 26;
        //                    sheet1.Range[xlsRow, np - 1, xlsRow, np].Merge();

        //                    sheet1.Range[xlsRow - 1, np - 1].Formula = "=" + dedAddSalStr;
        //                    sheet1.Range[xlsRow - 1, np - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                    sheet1.Range[xlsRow - 1, np - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[xlsRow - 1, np - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow - 1, np - 1].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow - 1, np - 1].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow - 1, np - 1].CellStyle.Font.Size = 26;
        //                    sheet1.Range[xlsRow - 1, np - 1, xlsRow - 1, np].Merge();

        //                    sheet1.Range[xlsRow, sigCol - 1].Formula = "=" + CTCAddSalProc + "-(" + dedAddSalProc + ")";//Net payable

        //                    if (sheetBasedOn == "structured")
        //                    {
        //                        sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                        sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                        sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.FontName = printFont;
        //                        sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Size = 40;
        //                        sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                        sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].Merge();

        //                        sheet1.Range[xlsRow - 1, colSignature, particular3rdRow + 2, colSignature].Merge();
        //                        sheet1.Range[xlsRow - 1, colSignature, particular3rdRow + 2, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow - 1, 1, particular3rdRow + 2, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow - 1, 1, particular3rdRow + 2, colSignature].BorderInside(ExcelLineStyle.Thin);
        //                    }
        //                    else
        //                    {
        //                        sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                        sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                        sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.FontName = printFont;
        //                        sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Size = 40;
        //                        sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                        sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].Merge();

        //                        sheet1.Range[xlsRow, colSignature, particular3rdRow + 3, colSignature].Merge();
        //                        sheet1.Range[xlsRow, colSignature, particular3rdRow + 3, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow, 1, particular3rdRow + 3, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow, 1, particular3rdRow + 3, colSignature].BorderInside(ExcelLineStyle.Thin);
        //                    }


        //                    #region DailyAttendance
        //                    dvDaily = new DataView(dsDaily);
        //                    dvDaily.RowFilter = "EmployeePK = '" + EmpIdPR + "' ";
        //                    var mnthColData = 0;
        //                    mnthColData = colParticulars;

        //                    var dtFrmDtIntData = 1;
        //                    var dtEndDateIntData = 31;
        //                    while (dtFrmDtIntData <= dtEndDateIntData)
        //                    {
        //                        mnthColData += 1;
        //                        var sc = _list2.Find(r => Convert.ToInt32(r.ValueMember) == dtFrmDtIntData);

        //                        var _day_status = "";
        //                        var _col_index = mnthColData;

        //                        if (sc != null && dvDaily.Count > 0)
        //                        {
        //                            _day_status = dvDaily[0][sc.DisplayMember].ToString();
        //                            _col_index += sc.ColIndex;

        //                            string[] dayStatusList = _day_status.Split(',');


        //                            var dayStatus = "";
        //                            if (dayStatusList[0] == "LV" || dayStatusList[0] == "W" || dayStatusList[0] == "H")
        //                            {
        //                                Array.Clear(dayStatusList, 1, dayStatusList.Length - 1);
        //                            }

        //                            for (int dst = 0; dst < dayStatusList.Length; dst++)
        //                            {
        //                                if (dayStatusList[dst] == "L")
        //                                {
        //                                    dayStatus = Convert.ToString(dayStatusList[dst]).Replace('L', 'P');
        //                                }
        //                                else
        //                                {
        //                                    dayStatus = dayStatusList[dst];
        //                                }
        //                                sheet1.Range[particular3rdRow + dst, _col_index].Text = dayStatus;
        //                                sheet1.Range[particular3rdRow + dst, _col_index].BorderAround(ExcelLineStyle.Thin);
        //                                sheet1.Range[particular3rdRow + dst, _col_index].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                                sheet1.Range[particular3rdRow + dst, _col_index].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[particular3rdRow + dst, _col_index].CellStyle.Font.FontName = "Arial Narrow";
        //                                sheet1.Range[particular3rdRow + dst, _col_index].CellStyle.Font.Size = 17;
        //                            }
        //                        }//date
        //                        else
        //                        {
        //                            for (int dst = 0; dst < 4; dst++)
        //                            {
        //                                sheet1.Range[particular3rdRow + dst, _col_index].Text = "";
        //                                sheet1.Range[particular3rdRow + dst, _col_index].BorderAround(ExcelLineStyle.Thin);
        //                                sheet1.Range[particular3rdRow + dst, _col_index].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                                sheet1.Range[particular3rdRow + dst, _col_index].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[particular3rdRow + dst, _col_index].CellStyle.Font.FontName = "Arial Narrow";
        //                                sheet1.Range[particular3rdRow + dst, _col_index].CellStyle.Font.Size = 17;
        //                            }
        //                        }
        //                        dtFrmDt = dtFrmDt.AddDays(1);
        //                        dtFrmDtIntData++;
        //                    }
        //                    #endregion
        //                    xlsRow++;
        //                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;
        //                    xlsRow++;
        //                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;
        //                    xlsCol = np;
        //                    #region Border Setup
        //                    xlsRow = particular3rdRow + 4;
        //                    EmpCounter++;
        //                    if ((EmpCounter % 7) == 0)
        //                    {
        //                        sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);
        //                    }
        //                    #endregion
        //                    #endregion *************************Data End*************************

        //                    xlsRow--;

        //                }//for emp count //colSalaryDistrHead
        //                #region Summation of all Salary head
        //                sheet1.Range[xlsRow + 1, colParticulars].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Total.ToString(), "Total");
        //                sheet1.Range[xlsRow + 1, colParticulars].NumberFormat = oRU.NumberFormatDecimalZero();
        //                sheet1.Range[xlsRow + 1, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, colParticulars].RowHeight = 40;
        //                sheet1.Range[xlsRow + 1, colParticulars].CellStyle.Font.Size = 28;
        //                sheet1.Range[xlsRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                foreach (var item in totalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
        //                {
        //                    try
        //                    {
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].Number = Convert.ToDouble(item.Value);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].Merge();
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].RowHeight = 40;
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].CellStyle.Font.Size = 28;
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].CellStyle.Font.Bold = true;
        //                    }
        //                    catch (Exception exe)
        //                    {
        //                        throw exe;
        //                    }
        //                }//Loop End Last Summation in SalaryPorcess



        //                var grossIndexSubStructOSide = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
        //                var ctcIndexSubStructOSide = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();

        //                var grossSubStructOside = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.SalaryHead).FirstOrDefault();
        //                var ctcSubStructOside = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.SalaryHead).FirstOrDefault();

        //                var dedFormulaStructOSide = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

        //                var grossAddSubOSide = oRU.SetFormula((grossIndexSubStructOSide - 1).ToString(), xlsRow + 1);
        //                var ctcAddSubOSide = oRU.SetFormula((ctcIndexSubStructOSide - 1).ToString(), xlsRow + 1);
        //                var dedAddStructOSide = oRU.SetFormula(dedFormulaStructOSide, xlsRow + 1);

        //                sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].Formula = "=" + oRU.SetFormula(grossSubStructOside, xlsRow + 1);
        //                sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage); ;

        //                sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].CellStyle.Font.Size = 28;
        //                sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1, xlsRow + 1, grossIndexSubStructOSide].Merge();

        //                sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].Formula = "=" + oRU.SetFormula(ctcSubStructOside, xlsRow + 1);
        //                sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage); ;
        //                sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].BorderAround(ExcelLineStyle.Thin);


        //                sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].CellStyle.Font.Size = 28;
        //                sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1, xlsRow + 1, ctcIndexSubStructOSide].Merge();


        //                var dedAddSubSalStructOSide = oRU.SetFormula(grossSubStructOside, xlsRow + 1);

        //                sheet1.Range[xlsRow + 1, np - 1].Formula = "=" + dedAddStructOSide;//Total Deduction
        //                sheet1.Range[xlsRow + 1, np - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                sheet1.Range[xlsRow + 1, np - 1].CellStyle.Font.Size = 28;
        //                sheet1.Range[xlsRow + 1, np - 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 1, np - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                sheet1.Range[xlsRow + 1, np - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow + 1, np - 1, xlsRow + 1, np].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, np - 1, xlsRow + 1, np].Merge();

        //                sheet1.Range[xlsRow + 1, colNetpayable].Formula = "=" + ctcAddSubOSide + "-(" + dedAddStructOSide + ")";//Net Payable
        //                sheet1.Range[xlsRow + 1, colNetpayable].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                sheet1.Range[xlsRow + 1, colNetpayable].CellStyle.Font.Size = 28;
        //                sheet1.Range[xlsRow + 1, colNetpayable].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 1, colNetpayable].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                sheet1.Range[xlsRow + 1, colNetpayable].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();

        //                sheet1.Range[xlsRow + 20, ColName].Text = "HR Executive";
        //                sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, ColName].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                //sheet1.Range[xlsRow + 8, ColLeaveInfo, xlsRow + 8, ColWorkDaysInfo].Borders(ExcelLineStyle.Thin);

        //                int numberOfColumns = colSignature - colParticulars;

        //                int remainCell = numberOfColumns - 24;
        //                var unmargedCell = closestNumber(remainCell, 3) / 3;
        //                int firstColumn = ColWorkDaysInfo + 1;
        //                sheet1.Range[xlsRow + 20, firstColumn].Text = "HR Manager";
        //                sheet1.Range[xlsRow + 20, firstColumn].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, firstColumn].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, firstColumn, xlsRow + 20, firstColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //                sheet1.Range[xlsRow + 20, firstColumn, xlsRow + 20, firstColumn + 7].Merge();

        //                int secondColumn = firstColumn + 7 + unmargedCell;
        //                sheet1.Range[xlsRow + 20, secondColumn].Text = "Head of HR";
        //                sheet1.Range[xlsRow + 20, secondColumn].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, secondColumn].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, secondColumn, xlsRow + 20, secondColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                sheet1.Range[xlsRow + 20, secondColumn, xlsRow + 20, secondColumn + 7].Merge();

        //                int thirdColumn = secondColumn + 7 + unmargedCell;
        //                sheet1.Range[xlsRow + 20, thirdColumn].Text = "Accounts Manager";
        //                sheet1.Range[xlsRow + 20, thirdColumn].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, thirdColumn].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, thirdColumn, xlsRow + 20, thirdColumn + 7].Merge();
        //                sheet1.Range[xlsRow + 20, thirdColumn, xlsRow + 20, thirdColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                sheet1.Range[xlsRow + 20, colSignature - 1].Text = "CFO/CEO";
        //                sheet1.Range[xlsRow + 20, colSignature - 1].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, colSignature - 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, colSignature - 1, xlsRow + 20, colSignature].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;


        //                sheet1.Range[xlsRow + 20, colSignature - 1, xlsRow + 20, colSignature].Merge();
        //                //sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].RowHeight = 153;
        //                sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


        //                #endregion
        //                #endregion ----------------------Data End-----------------------
        //                #region ******************Report Header******************
        //                objRpt.SelectedPlantWiseCompany(plantId, languageId, out dsCmp);
        //                xlsRow = 1;
        //                xlsCol = 1;

        //                FactoryName = string.Empty;

        //                var FactoryAddress = string.Empty;

        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyNameLocal"].ToString();
        //                }
        //                else
        //                {
        //                    CmpName = "";
        //                }
        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantNameLocal"].ToString();
        //                }
        //                else
        //                {
        //                    FactoryName = "";
        //                }
        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddressLocal"].ToString();
        //                }
        //                else
        //                {
        //                    FactoryAddress = "";
        //                }
        //                sheet1.Range[xlsRow, 1].Text = FactoryName;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 55;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = printFont;

        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 63;

        //                xlsRow++;
        //                sheet1.Range[xlsRow, 1].Text = FactoryAddress;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 30;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 44;
        //                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

        //                sheet1.Range[xlsRow - 1, endXlsCol].Text = "Print Date: " + DateTime.Now.ToString("dd-MMM-yyy") + Environment.NewLine + "Payment Date:" + paymentDate;
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Font.Size = 24;
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Font.FontName = "ArialNarrow";
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

        //                string yearLocal = ru.cnDgt(Convert.ToDateTime(para.FromDate).Year.ToString(), localLanguage);

        //                xlsRow += 1;
        //                sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.SalarySheet.ToString(), "Salary Sheet") + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), localLanguage) + "-" + yearLocal; //_payRegisterLocal + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), "Bengali") + "," + yearLocal;
        //                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 35;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 51;
        //                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;



        //                #endregion ******************Report Header******************
        //                #region Freeze Panes
        //                sheet1.UsedRange["A7"].FreezePanes();
        //                sheet1.FirstVisibleColumn = 1;
        //                sheet1.FirstVisibleRow = 5;
        //                #endregion

        //                #region UsedRange Alignment
        //                sheet1.UsedRange.WrapText = true;
        //                //sheet1.UsedRange.is;
        //                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
        //                #endregion UsedRange Alignment

        //                string vv = "abc@dter" + DateTime.Now.ToString("dd-MMM-yyyy") + plantId;
        //                sheet1.Protect(vv, ExcelSheetProtection.All);
        //                #region Page Setup
        //                sheet1.PageSetup.TopMargin = 0.2;
        //                sheet1.PageSetup.BottomMargin = 0.7;

        //                sheet1.PageSetup.PrintTitleRows = "$A$1:$IV$6";
        //                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&15" + "Page " + "&p" + " of " + "&N";
        //                sheet1.PageSetup.LeftMargin = 0.5;
        //                sheet1.PageSetup.RightMargin = 0.2;
        //                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
        //                sheet1.PageSetup.FitToPagesTall = 0;
        //                sheet1.PageSetup.FitToPagesWide = 1;
        //                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperLegal;

        //                sheet1.Name = "EmpPayRegister" + para.SalaryProcessId;
        //                #endregion          
        //                return workbook;
        //            }

        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //            finally
        //            {
        //                objRpt = null;
        //                excelEngine = null;
        //                application = null;
        //                workbook = null;
        //                sheet1 = null;
        //            }
        //        }

        //        public IWorkbook EmployeeSalaryRegisterWithStructure(PayRegisterParamList PayRegisterParam, string paymentDate, string sqlInStatement, bool isActive, bool isSeperated, bool isMaternity)
        //        {
        //            string plantId = PayRegisterParam.PlantId;
        //            string unitId = PayRegisterParam.UnitId;
        //            string divisionId = PayRegisterParam.DivisionId;
        //            string departmentId = PayRegisterParam.DepartmentId;
        //            string sectionId = PayRegisterParam.SectionId;
        //            string subsectionId = PayRegisterParam.SubSectionId;
        //            string month = PayRegisterParam.Month;
        //            string year = PayRegisterParam.Year;
        //            string salaryProcessId = PayRegisterParam.SalaryProcessId;
        //            string empSystmId = PayRegisterParam.EmployeeId;
        //            string empStatus = PayRegisterParam.EmpStatus;
        //            string payGroup = PayRegisterParam.PayGroup;
        //            string userId = PayRegisterParam.userId;
        //            string categoryId = PayRegisterParam.EmpCategoryId;
        //            string paymentMode = PayRegisterParam.PaymentMode;
        //            string languageId = PayRegisterParam.LanguageId;

        //            #region Variable
        //            clsReport objRpt = null;
        //            clsSalaryUtility objSalary = null;

        //            DataSet dsSlrStruct = null;
        //            DataSet dsSlrProced = null;
        //            DataSet dsLeaveInfo = null;
        //            DataSet dsLeaveType = null;

        //            DataSet dsEmpAttdnInfo = null;
        //            DataView dvEmp = null;
        //            DataView dvLeaveEmp = null;
        //            DataView dvLeaveType = null;
        //            DataView dvSlrProc = null;
        //            DataSet dsCmp = null;
        //            DataSet dsFactory = null;
        //            DataSet dsBonus = null;

        //            ExcelEngine excelEngine = null;
        //            IApplication application = null;
        //            IWorkbook workbook = null;
        //            IWorksheet sheet1 = null;
        //            ReportUtility ru = null;
        //            var grossColIndex = 0;
        //            var CTCColIndex = 0;
        //            ParamList para = new ParamList();
        //            ParamList leavePara = new ParamList();
        //            ParamList attdnProcessParam = new ParamList();

        //            var sUnit = unitId;
        //            var sDevi = divisionId;
        //            var sDept = departmentId;
        //            var sSect = sectionId;
        //            var sSbSect = subsectionId;
        //            var remCol = 0;

        //            para.UnitId = unitId;
        //            para.DivisionId = divisionId;
        //            para.DepartmentId = departmentId;
        //            para.SectionId = sectionId;
        //            para.SubSectionId = subsectionId;
        //            para.PlantId = plantId;
        //            para.EmpCategorId = categoryId;
        //            para.LanguageId = languageId;
        //            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
        //            DataTable dtLunchOuthr = null;
        //            #endregion Variable

        //            try
        //            {
        //                ru = new ReportUtility();
        //                objRpt = new clsReport();
        //                objSalary = new clsSalaryUtility();
        //                ParaMontlyAttendance objm = new ParaMontlyAttendance();
        //                #region Variable             

        //                var FactoryName = "";
        //                var CmpName = "";

        //                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
        //                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
        //                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
        //                var fdateOfMonth = "1" + "-" + monthName + "-" + year;

        //                Dictionary<string, string> labelList = ru.LocalLanguageLabelList(para.PlantId, languageId);
        //                var DTpayGroup = payRollGroup(payGroup);
        //                var DTEmployeeCatg = EmpCategory(categoryId);

        //                var localLanguage = "";
        //                var payGroupName = "";
        //                var empCategory = "";
        //                var printFont = "";
        //                bool isLocalLanguage = false;
        //                localLanguage = ru.LocalLanguageListSql(plantId, languageId, out isLocalLanguage);
        //                if (localLanguage == "Bengali")
        //                {
        //                    printFont = "SolaimanLipi";
        //                }
        //                else
        //                {
        //                    printFont = "Arial Narrow";

        //                }
        //                if (DTpayGroup.Rows.Count > 0)
        //                {
        //                    payGroupName = DTpayGroup.Rows[0]["UserName"].ToString();
        //                }
        //                if (DTEmployeeCatg.Rows.Count > 0)
        //                {
        //                    empCategory = DTEmployeeCatg.Rows[0]["UserName"].ToString();
        //                }


        //                DataView dvDaily = null;
        //                objm.AMonth = month;
        //                objm.AYear = year;
        //                objm.PlantId = plantId;
        //                objm.FDate = fdateOfMonth;
        //                objm.TDate = ldateOfMonth;
        //                var _ShiftCode = string.Empty;

        //                var salarySheetValue = 0.00;

        //                para.PlantId = plantId;

        //                leavePara.PlantId = plantId;

        //                para.EmployeeId = empSystmId;
        //                para.FromDate = fdateOfMonth;
        //                para.ToDate = ldateOfMonth;
        //                para.SalaryProcessId = salaryProcessId;
        //                para.EmpStatus = empStatus;
        //                para.PayGroup = payGroup;
        //                para.SubSectionId = subsectionId;
        //                para.SectionId = sectionId;



        //                leavePara.EmployeeId = empSystmId;
        //                leavePara.FromDate = fdateOfMonth;
        //                leavePara.SalaryProcessId = salaryProcessId;
        //                leavePara.EmpStatus = empStatus;
        //                #endregion Variable
        //                DateTime dtFrmDt = DateTime.Now;
        //                DateTime dtEndDate = DateTime.Now;

        //                string m = ru.GetMonthName(month);

        //                #region DataSet
        //                string stringSalaryRegSorting = "";
        //                stringSalaryRegSorting = objRpt.GetSortingParameters(para.CompanyGroupId, para.CompanyId, para.PlantId, "");

        //                List<SalaryStructureReport> listdsSlrStruct = new List<SalaryStructureReport>();//SalaryStruct
        //                List<SalarySheetReportStructure> listdsSlrSheet = new List<SalarySheetReportStructure>();//SalarySheetReport


        //                objRpt.GetEmpSalaryStructureRegisterAndPaySlipRpt(para, paymentMode, out dsSlrStruct);  //Salary Structure
        //                objRpt.GetSalaryInfoSlrProcIDWiseForRegister(para, paymentMode, languageId, sqlInStatement, stringSalaryRegSorting, isActive, isSeperated, isMaternity, out dsSlrProced);
        //                dvSlrProc = new DataView();
        //                dvSlrProc.Table = dsSlrProced.Tables[0];
        //                if (dsSlrProced.Tables[0].Rows.Count > 0)
        //                {
        //                    listdsSlrSheet = dsSlrProced.Tables[0].ToList<SalarySheetReportStructure>();
        //                }

        //                dvEmp = new DataView();
        //                dvEmp.Table = dsSlrProced.Tables[0];
        //                var dtEmployees = dvEmp.ToTable(true, "EmpInfoSystemID", "DivisionName", "DivisionId", "SubDivision", "SubdivisionID", "UnitName", "UnitID", "DepartmentName", "DepartmentID", "SectionName", "SectionID", "SubSectionName", "SubSectionID", "LDDesignationGD", "DesignationLocal", "GradeCode", "DOJ", "EmployeeName", "EmployeeNameLocal", "FatherName", "DOS", "EmployeeCode", "BankAccNo", "IsOTEntitle");

        //                //var dtEmployees = dvEmp.ToTable(true, "SystemID", "Division", "DivisionId", "SubDivision", "SubdivisionID", "Unit", "UnitID", "Department", "DepartmentID", "Section", "SectionID", "SubSection", "SubSectionID", "LDDesignationGD", "DesignationLocal", "GradeCode", "DOJ", "EmployeeName", "EmployeeNameLocal", "FatherName", "DOS", "EmployeeCode", "SalaryHeadValue", "BankAccNo", "IsOTEntitle");
        //                if (dtEmployees.Rows.Count == 0)
        //                {
        //                    var ex = new Exception("No Data found...");
        //                    throw (ex);
        //                }

        //                if (dsSlrStruct.Tables[0].Rows.Count > 0)
        //                {
        //                    listdsSlrStruct = dsSlrStruct.Tables[0].ToList<SalaryStructureReport>();
        //                }



        //                DataTable dsDaily = GetMonthlyDailyAttendance(_ShiftCode, objm);
        //                List<SwapColumn> _list2 = GetColDisplayName(dsDaily);


        //                objRpt.SelectedPlantWiseCompany(plantId, languageId, out dsCmp);

        //                objRpt.SelectedPlant(plantId, out dsFactory);

        //                #endregion DataSet

        //                excelEngine = new ExcelEngine();
        //                application = excelEngine.Excel;

        //                workbook = application.Workbooks.Create(1);
        //                sheet1 = workbook.Worksheets[0];
        //                sheet1.IsGridLinesVisible = true;
        //                sheet1.IsDisplayZeros = false;
        //                #region------------------Column Header------------------
        //                xlsRow = 5;
        //                xlsCol = 1;

        //                var ColSr = 0;
        //                var ColName = 0;
        //                var ColLeaveInfo = 0;
        //                var ColWorkDaysInfo = 0;
        //                var colParticulars = 0;
        //                var ColGrs = 0;
        //                #endregion------------------Column Header------------------

        //                int RowIndex = xlsRow + 1;

        //                #region ----------------------Data-----------------------
        //                var strSubDivision = "0";
        //                var strSection = "0";
        //                var strDiv = "0";
        //                var strUnit = "0";
        //                var strDepartment = "0";
        //                var strSubSection = "0";

        //                var SrNo = 0;
        //                var EmpIdPR = "";
        //                var oRU = new ReportUtility();
        //                var intRow = 0;
        //                xlsRow = RowIndex;

        //                List<SalaryHeadSequenceStructure> list = null;

        //                var np = 0;
        //                var isFirst = true;
        //                var sigCol = 0;
        //                var deptFirstRow = 0;

        //                xlsRow--;


        //                var totalDictSalaryStruct = new Dictionary<string, double>();
        //                var totalDictSalaryProcess = new Dictionary<string, double>();


        //                int endCol = 5;
        //                int colNetpayable = endCol;
        //                int colSignature = endCol;
        //                #region RegisterHeader
        //                var colex = 0;
        //                sigCol = 0;




        //                if (isFirst == false)
        //                {
        //                    xlsRow += 3;
        //                    colex = 2;
        //                }
        //                #region ------------------Column Header------------------
        //                xlsCol = 1;
        //                var lineHeader = "";
        //                if (!paymentMode.IsNullOrEmpty() && paymentMode != "null" && paymentMode != "undefined")
        //                    lineHeader += paymentMode;
        //                if (!empCategory.IsNullOrEmpty())
        //                {
        //                    if (!paymentMode.IsNullOrEmpty() && paymentMode != "null" && paymentMode != "undefined")
        //                    {
        //                        lineHeader += "~";
        //                    }
        //                    lineHeader += empCategory;

        //                }
        //                if (!payGroup.IsNullOrEmpty())
        //                {
        //                    if (!paymentMode.IsNullOrEmpty() && paymentMode != "null" && paymentMode != "undefined")
        //                    {
        //                        lineHeader += "~";
        //                    }
        //                    if (!empCategory.IsNullOrEmpty())
        //                    {
        //                        lineHeader += "~";
        //                    }
        //                    lineHeader += payGroupName;

        //                }

        //                sheet1.Range[xlsRow - 1, xlsCol].Text = lineHeader;
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.Size = 48;
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow - 1, xlsCol, xlsRow - 1, xlsCol + 3].Merge();
        //                sheet1.Range[xlsRow - 1, xlsCol].RowHeight = 52;

        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.SrNo.ToString(), "Sr. No."), sheet1, xlsRow + colex, ref xlsCol, out ColSr, 15, printFont, 90);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.EmployeeInformation.ToString(), "Employee Information"), sheet1, xlsRow + colex, ref xlsCol, out ColName, 100, printFont, 0);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.LeaveInformation.ToString(), "Leave Information"), sheet1, xlsRow + colex, ref xlsCol, out ColLeaveInfo, 35, printFont, 0);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.WorkDaysDetail.ToString(), "Work Days Detail"), sheet1, xlsRow + colex, ref xlsCol, out ColWorkDaysInfo, 60, printFont, 0);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.Particulars.ToString(), "Particulars"), sheet1, xlsRow + colex, ref xlsCol, out colParticulars, 28, printFont, 0);
        //                ColGrs = colParticulars;
        //                var _count_earning_head = 0;
        //                var _count_earning_ctchead = 0;
        //                var _count_deducting_head = 0;
        //                var _total_head_count = 0;

        //                DataView dvSalaryHead = new DataView(dsSlrProced.Tables[0]);

        //                dvSalaryHead.Sort = "HeadType desc,Sequence";

        //                DataTable dtSalaryHead = dvSalaryHead.ToTable(true, "SalaryHeadID", "SalaryHead", "SalaryHeadBangla", "HeadType", "Sequence", "HeadCategory", "IsCTCComponent", "IsGrossComponent", "IntegerInDisb", "DecimalNo", "IsNetPayEffect");

        //                DataTable dtVPFHead = dvSalaryHead.ToTable(true, "xHeadCategory", "xSalaryHeadID", "xSalaryHead", "xIsCTCComponent", "xIsGrossComponent", "xHeadType", "IntegerInDisb", "DecimalNo");

        //                objRpt.GetBonus(month, year, out dsBonus);
        //                DataView dvBonus = new DataView(dsBonus.Tables[0]);
        //                DataTable dtBonusHead = dvBonus.ToTable(true, "SalaryHeadID", "HeadCategory", "SalaryHead", "IsCTCComponent", "IsGrossComponent", "HeadType", "Sequence");

        //                OTSBD.clsSalary.clsSalaryReport sr = new OTSBD.clsSalary.clsSalaryReport();

        //                sr.SetSheetBonus(dtBonusHead, ref dtSalaryHead);

        //                xlsRow += colex;

        //                CreateDynamicSHeadLocalLanguageStruct(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list, labelList, printFont);
        //                xlsRow -= colex;
        //                xlsRow += intRow;
        //                intRow = 1;

        //                endCol = 5;

        //                #region Day of a month 
        //                var mnthCol = 0;
        //                mnthCol = colParticulars;

        //                var dtFrmDtInt = 1;
        //                var dtEndDateInt = 31;
        //                while (dtFrmDtInt <= dtEndDateInt)
        //                {
        //                    mnthCol += 1;
        //                    var sc = _list2.Find(r => Convert.ToInt32(r.ValueMember) == dtFrmDtInt);

        //                    var _col_index = mnthCol;
        //                    sheet1.Range[xlsRow, _col_index].Text = dtFrmDtInt.ToString();
        //                    sheet1.Range[xlsRow, _col_index].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
        //                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Bold = true;
        //                    sheet1.Range[xlsRow, _col_index].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                    sheet1.Range[xlsRow, _col_index].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.FontName = "Arial Narrow";
        //                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Size = 30;
        //                    sheet1.Range[xlsRow, _col_index].ColumnWidth = 13;
        //                    dtFrmDtInt++; ;
        //                }
        //                #endregion
        //                endCol = colParticulars;
        //                if (list.Count > 0)
        //                {
        //                    xlsCol++;
        //                    np = ColGrs + list.Count * 2;
        //                    endCol = np + 1;


        //                    sheet1.Range[xlsRow + 1, np - 1].Text = ru.GetLabelname(labelList, "TotalDeduction", "Total Deduction"); //totalDeduction.LocalLabel == null || totalDeduction.LocalLabel == "" ? totalDeduction.DefaultLabel : totalDeduction.LocalLabel;
        //                    sheet1.Range[xlsRow + 1, np - 1].CellStyle.ShrinkToFit = true;
        //                    sheet1.Range[xlsRow + 1, np - 1].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow + 1, np - 1].CellStyle.Font.Size = 22;
        //                    sheet1.Range[xlsRow + 1, np - 1, xlsRow + 1, np].Merge();
        //                    xlsCol++;
        //                }

        //                if (endCol <= colParticulars + 29)
        //                    endCol = colParticulars + 30;

        //                colNetpayable = endCol;

        //                sheet1.Range[xlsRow + 1, endCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.NetPayable.ToString(), "Net Payable");//netPayable.LocalLabel == null || netPayable.LocalLabel == "" ? netPayable.DefaultLabel : netPayable.LocalLabel;
        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();
        //                sheet1.Range[xlsRow + 1, endCol].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow + 1, endCol].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 1, colNetpayable].ColumnWidth = 18;
        //                sheet1.Range[xlsRow + 1, colNetpayable + 1].ColumnWidth = 18;



        //                colSignature = colNetpayable + 2;

        //                sheet1.Range[xlsRow, colSignature].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Signature.ToString(), "Signature");//empSignatgure.LocalLabel == null || empSignatgure.LocalLabel == "" ? empSignatgure.DefaultLabel : empSignatgure.LocalLabel;
        //                sheet1.Range[xlsRow, colSignature, xlsRow + 1, colSignature].Merge();
        //                sheet1.Range[xlsRow, colSignature].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, colSignature].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow, colSignature].ColumnWidth = 70;

        //                endCol = colSignature;

        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].BorderAround(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].BorderInside(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, endCol].RowHeight = 127;

        //                endXlsCol = endCol;
        //                sigCol = endCol;
        //                xlsCol = 1;
        //                xlsRow++;
        //                int EmpCounter = 0;

        //                deptFirstRow = xlsRow;
        //                var _dataSlrSheetNumberCTC = 0.00;
        //                var ctcSalarydd = 0.00;
        //                var totalCTCSalary = 0.00;
        //                #endregion

        //                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
        //                {
        //                    salarySheetValue = 0.00;
        //                    ctcSalarydd = 0.00;

        //                    #endregion ------------------Column Header------------------

        //                    #region *************************Data*************************
        //                    strDiv = dtEmployees.Rows[i]["DivisionID"].ToString().Trim();
        //                    strSubDivision = dtEmployees.Rows[i]["SubdivisionID"].ToString().Trim();
        //                    strUnit = dtEmployees.Rows[i]["UnitID"].ToString().Trim();
        //                    strDepartment = dtEmployees.Rows[i]["DepartmentID"].ToString().Trim();
        //                    strSection = dtEmployees.Rows[i]["SectionID"].ToString().Trim();
        //                    strSubSection = dtEmployees.Rows[i]["SubSectionID"].ToString().Trim();

        //                    xlsRow++;
        //                    #region LeaveInformation
        //                    leavePara.EmployeeId = dtEmployees.Rows[i]["EmpInfoSystemID"].ToString();

        //                    objRpt.GetLeaveTypeLocal(leavePara, languageId, out dsLeaveType);
        //                    dvLeaveType = new DataView();

        //                    dvLeaveType.Table = dsLeaveType.Tables[0];

        //                    objRpt.GetEmpLeaveInfo(leavePara, out dsLeaveInfo);
        //                    dvLeaveEmp = new DataView();

        //                    dvLeaveEmp.Table = dsLeaveInfo.Tables[0];
        //                    var EL = GetLeaveEmp(dvLeaveEmp, "EL");
        //                    var CL = GetLeaveEmp(dvLeaveEmp, "CL");
        //                    var SL = GetLeaveEmp(dvLeaveEmp, "SL");

        //                    var locEL = GetLeaveType(dvLeaveType, "EL");
        //                    var locCL = GetLeaveType(dvLeaveType, "CL");
        //                    var locSL = GetLeaveType(dvLeaveType, "SL");


        //                    #endregion


        //                    attdnProcessParam.EmployeeId = dtEmployees.Rows[i]["EmpInfoSystemID"].ToString();
        //                    objRpt.GetMonthlyAttdnInfo(attdnProcessParam, month, year, out dsEmpAttdnInfo);

        //                    DataTable dtEmpAttdnInfo = dsEmpAttdnInfo.Tables[0];

        //                    dtLunchOuthr = GetLunchOutHour(dtEmployees.Rows[i]["EmpInfoSystemID"].ToString(), fdateOfMonth, ldateOfMonth);//Get lunch Out Hr

        //                    #region EmpInfo
        //                    SrNo += 1;
        //                    EmpIdPR = dtEmployees.Rows[i]["EmpInfoSystemID"].ToString().Trim();

        //                    sheet1.Range[xlsRow, ColSr].Text = ru.cnDgt(Convert.ToString(SrNo), localLanguage);
        //                    sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                    sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, ColSr, xlsRow + 4, ColSr].Merge();
        //                    sheet1.Range[xlsRow, ColSr, xlsRow + 4, ColSr].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, ColSr].CellStyle.Font.FontName = "Arial Narrow";
        //                    sheet1.Range[xlsRow, ColSr].CellStyle.Font.Size = 25;

        //                    //3
        //                    var _DOJ = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOJ.ToString(), "DOJ");
        //                    var _DOS = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOS.ToString(), "DOS");
        //                    var _designationLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Designation.ToString(), "Designation");
        //                    var _gradeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Grade.ToString(), "Grade");
        //                    var empName = "";
        //                    if (isLocalLanguage)
        //                    {
        //                        empName = dtEmployees.Rows[i]["EmployeeNameLocal"].ToString();
        //                    }
        //                    else
        //                    {
        //                        empName = dtEmployees.Rows[i]["EmployeeName"].ToString();

        //                    }
        //                    var empDesignation = _designationLocal + ":" + dtEmployees.Rows[i]["DesignationLocal"].ToString();
        //                    var empDOJ = _DOJ + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOJ"].ToString(), localLanguage);
        //                    var bankAccount = "";
        //                    var empDOS = string.Empty;
        //                    if (dtEmployees.Rows[i]["DOS"].ToString().Length > 0)
        //                    {
        //                        empDOS = _DOS + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOS"].ToString(), localLanguage); //+ Environment.NewLine;
        //                    }
        //                    if (dtEmployees.Rows[i]["BankAccNo"].ToString().Length > 0)
        //                    {
        //                        bankAccount = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bank Acc No") + ":" + dtEmployees.Rows[i]["BankAccNo"].ToString(); //+ Environment.NewLine;
        //                    }


        //                    sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString() + Environment.NewLine + empName;

        //                    sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                    sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, ColName].BorderAround(ExcelLineStyle.Hair);
        //                    sheet1.Range[xlsRow, ColName].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow, ColName].CellStyle.Font.Bold = true;
        //                    sheet1.Range[xlsRow, ColName].CellStyle.Font.Size = 50;

        //                    // Designation DOJ DOS
        //                    IRichTextString rtf1 = sheet1.Range[xlsRow + 1, ColName].RichText;
        //                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                    FormatText(ref sheet1, ref rtf1, _designationLocal + ":" + dtEmployees.Rows[i]["DesignationLocal"].ToString() + " ", 25);
        //                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                    FormatText(ref sheet1, ref rtf1, _gradeLocal + ":", 25); FormatText(ref sheet1, ref rtf1, dtEmployees.Rows[i]["GradeCode"].ToString() + " ", 25);
        //                    if (dtEmployees.Rows[i]["BankAccNo"].ToString().Length > 0)
        //                    {
        //                        bankAccount = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bank Acc No") + ":" + dtEmployees.Rows[i]["BankAccNo"].ToString(); //+ Environment.NewLine;
        //                        FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf1, bankAccount + " ", 25);
        //                    }

        //                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                    FormatText(ref sheet1, ref rtf1, empDOJ + " ", 25);
        //                    if (dtEmployees.Rows[i]["DOS"].ToString().Length > 0)
        //                    {
        //                        empDOS = _DOS + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOS"].ToString(), localLanguage); //+ Environment.NewLine;
        //                        FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf1, empDOS + " ", 25);
        //                    }


        //                    sheet1.Range[xlsRow + 1, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                    sheet1.Range[xlsRow + 1, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow + 1, ColName, xlsRow + 4, ColName].Merge();
        //                    sheet1.Range[xlsRow + 1, ColName, xlsRow + 4, ColName].BorderAround(ExcelLineStyle.Thin);

        //                    //Leave Info
        //                    if (dtEmpAttdnInfo.Rows.Count > 0)
        //                    {
        //                        var lunchOutHr = 0.00;
        //                        if (dtLunchOuthr.Rows.Count > 0)
        //                        {
        //                            lunchOutHr = Convert.ToDouble(dtLunchOuthr.Rows[0]["LunchOutHour"].ToString());
        //                        }
        //                        string lateBangla = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalLate"]).ToString(), localLanguage);
        //                        string presentBangla = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalPresentLate"]).ToString(), localLanguage);
        //                        string absentBangla = ru.cnDgt((Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalAbsent"]) - Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalLWP"])).ToString(), localLanguage);
        //                        string weekOff = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalWeekOffPlusWeekOffHoliDay"]).ToString(), localLanguage);
        //                        string holiDay = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalHoliDay"]).ToString(), localLanguage);
        //                        string leave = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalLv"]).ToString(), localLanguage);
        //                        string totalOTHr = "";//ru.cnDgt((Math.Round((Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalNormalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalExtraOTHr"])) / 60, 2).ToString()), localLanguage);
        //                        string lunchOutHrLocal = ru.cnDgt(lunchOutHr.ToString(), localLanguage);
        //                        var _availedLeave = ru.GetLabelname(labelList, LabelNameInLocalLanguage.AvailedLeave.ToString(), "Availed Leave");

        //                        IRichTextString rtfLeave = sheet1.Range[xlsRow, ColLeaveInfo].RichText;
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locEL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(EL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locCL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(CL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locSL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(SL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, _availedLeave + ":" + ru.cnDgt(Math.Round(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalLv"].ToString()), 0).ToString(), localLanguage) + " ", 25);

        //                        sheet1.Range[xlsRow, ColLeaveInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                        sheet1.Range[xlsRow, ColLeaveInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 4, ColLeaveInfo].Merge();
        //                        sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 4, ColLeaveInfo].BorderAround(ExcelLineStyle.Thin);

        //                        string payDays = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalProcDate"]).ToString(), localLanguage);
        //                        DataView dvBasic = new DataView(dsSlrProced.Tables[0]);
        //                        dvBasic.RowFilter = "SalaryHead='Basic' and EmpInfoSystemID='" + EmpIdPR + "' and IsOTEntitle = true";//For OT Rate Calculation


        //                        double otRate = 0.00;
        //                        string otRateBangla = "";
        //                        if (Convert.ToBoolean(dtEmpAttdnInfo.Rows[0]["IsOTEntitled"]) == true)
        //                        {
        //                            totalOTHr = ru.cnDgt((Math.Round((Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalNormalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalExtraOTHr"])) / 60, 2).ToString()), localLanguage);
        //                            otRate = Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["OTRate"]);
        //                            otRateBangla = ru.cnDgt(Math.Round(otRate, 2).ToString(), localLanguage);
        //                        }
        //                        IRichTextString rtf = sheet1.Range[xlsRow, ColWorkDaysInfo].RichText;

        //                        var _payDaysLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.PayDays.ToString(), "Pay Days");
        //                        var _presentLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Present.ToString(), "Present");
        //                        var _absentLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Absent.ToString(), LabelNameInLocalLanguage.Absent.ToString());//absentLocal.LocalLabel == null || absentLocal.LocalLabel == "" ? absentLocal.DefaultLabel : absentLocal.LocalLabel;
        //                        var _lunchOutHour = ru.GetLabelname(labelList, LabelNameInLocalLanguage.LunchOutHour.ToString(), "Lunch Out Hr.: ");//lunchOutHour.LocalLabel == null || lunchOutHour.LocalLabel == "" ? lunchOutHour.DefaultLabel : lunchOutHour.LocalLabel;
        //                        var _late = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Late.ToString(), "Late");
        //                        var _otHour = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OTHours.ToString(), "OT Hrs");
        //                        var _otRateLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OTRate.ToString(), "OT Rate");
        //                        var _weeklyHolidays = ru.GetLabelname(labelList, LabelNameInLocalLanguage.WeeklyLeaveDays.ToString(), "Weekend");
        //                        var _holiDays = ru.GetLabelname(labelList, LabelNameInLocalLanguage.HoliDay.ToString(), "Holidays");

        //                        FormatText(ref sheet1, ref rtf, _payDaysLocal + ":" + payDays + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _presentLocal + ":" + presentBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _absentLocal + ":" + absentBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _weeklyHolidays + ":" + weekOff + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _holiDays + ":" + holiDay + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _lunchOutHour + ":" + lunchOutHrLocal + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _late + ":" + lateBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        if (Convert.ToBoolean(dtEmpAttdnInfo.Rows[0]["IsOTEntitled"]) == true)
        //                        {
        //                            FormatText(ref sheet1, ref rtf, _otHour + ":" + totalOTHr + " ", 27);
        //                            FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                            FormatText(ref sheet1, ref rtf, _otRateLocal + ":" + otRateBangla + " ", 27);
        //                        }


        //                        sheet1.Range[xlsRow, ColWorkDaysInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                        sheet1.Range[xlsRow, ColWorkDaysInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 4, ColWorkDaysInfo].Merge();
        //                        sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 4, ColWorkDaysInfo].BorderAround(ExcelLineStyle.Thin);
        //                    }

        //                    //var earnedSalary = labelList.Where(r => r.DefaultLabel == LabelNameInLocalLanguage.EarnedSalary.ToString()).FirstOrDefault();
        //                    var earnedSalary = "";// labelList.Where(r => r.DefaultLabel == LabelNameInLocalLanguage.EarnedSalary.ToString()).FirstOrDefault();
        //                    if (labelList.ContainsKey(LabelNameInLocalLanguage.EarnedSalary.ToString()))
        //                    {
        //                        earnedSalary = labelList[LabelNameInLocalLanguage.EarnedSalary.ToString()];
        //                    }
        //                    var _earnedSalary = ru.GetLabelname(labelList, LabelNameInLocalLanguage.EarnedSalary.ToString(), "Earned Salary");
        //                    sheet1.Range[xlsRow, colParticulars].Text = _earnedSalary + "->";
        //                    sheet1.Range[xlsRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[xlsRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Size = 24;
        //                    sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Bold = true;

        //                    var _daysLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Days.ToString(), "Days");

        //                    var particular3rdRow = xlsRow + 1;

        //                    var _attendance = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Attendance.ToString(), "Attendance");

        //                    sheet1.Range[particular3rdRow, colParticulars].Text = _attendance + "->";
        //                    sheet1.Range[particular3rdRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[particular3rdRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[particular3rdRow, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[particular3rdRow, colParticulars].RowHeight = 43;
        //                    sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.Size = 17;
        //                    sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.Bold = true;

        //                    var _inTimeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.INTime.ToString(), "INTime");
        //                    var _outTimeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OUTTime.ToString(), "OUTTime");
        //                    var _OTLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OT.ToString(), "OT");

        //                    sheet1.Range[particular3rdRow + 1, colParticulars].Text = _inTimeLocal + "->";
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].RowHeight = 43;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.Size = 17;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                    sheet1.Range[particular3rdRow + 2, colParticulars].Text = _outTimeLocal + "->";
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].RowHeight = 43;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.Size = 17;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.Bold = true;

        //                    sheet1.Range[particular3rdRow + 3, colParticulars].Text = _OTLocal + "->";
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].RowHeight = 43;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.Size = 17;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.Bold = true;
        //                    #endregion

        //                    var _total_head_count_body = 0;
        //                    for (int ci = 0; ci < list.Count; ci++)
        //                    {
        //                        var ob = list[ci];
        //                        if (ob.SalaryHead.Length > 0)
        //                        {
        //                            if (ob.IsNetPayEffect == true && ob.IsCTCComponent == true)//CTC CALCULATION
        //                            {

        //                                var hId = ob.SalaryHeadId;
        //                                var _dataSlrSheet = listdsSlrSheet.Where(r => r.SalaryHeadID == hId && r.EmpInfoSystemID == EmpIdPR).FirstOrDefault();
        //                                if (_dataSlrSheet != null)
        //                                {
        //                                    _dataSlrSheetNumberCTC = clsStaticInfo.dbl(_dataSlrSheet.DisbusmentAmount.ToString());
        //                                    ctcSalarydd += _dataSlrSheetNumberCTC;
        //                                }


        //                            }

        //                            if (ob.SalaryHeadId.ToUpper() == "CTC")
        //                            {
        //                                var formula = ob.SalaryHead;
        //                                var hIdd = ob.SalaryHeadId;
        //                                _total_head_count_body++;
        //                                totalCTCSalary += ctcSalarydd;
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].Number = ctcSalarydd;
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocalStructure(ob, localLanguage);
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].BorderAround(ExcelLineStyle.Thin);
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1, xlsRow, ob.XLColIndex].Merge();

        //                                CTCColIndex = ob.XLColIndex;

        //                            }
        //                            if (ob.SalaryHeadId.ToUpper() == "GROSS")
        //                            {
        //                                var formula = ob.SalaryHead;
        //                                var hId = ob.SalaryHeadId;
        //                                _total_head_count_body++;

        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].Formula = "=" + oRU.SetFormula(formula, xlsRow);
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocalStructure(ob, localLanguage);
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].BorderAround(ExcelLineStyle.Thin);
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                                sheet1.Range[xlsRow, ob.XLColIndex - 1, xlsRow, ob.XLColIndex].Merge();

        //                                if (ob.SalaryHeadId.ToUpper() == "GROSS")
        //                                {
        //                                    grossColIndex = ob.XLColIndex;
        //                                }
        //                            }//ctc , gross
        //                            else
        //                            {
        //                                var hId = ob.SalaryHeadId;
        //                                _total_head_count_body++;

        //                                #region SalaryProcess 

        //                                var _dataSlrSheet = listdsSlrSheet.Where(r => r.SalaryHeadID == hId && r.EmpInfoSystemID == EmpIdPR).FirstOrDefault();
        //                                double _dataSlrSheetNumber = 0;

        //                                if (ob.HeadType == "D")
        //                                {
        //                                    if (_dataSlrSheet != null)
        //                                        _dataSlrSheetNumber = (Convert.ToDouble(_dataSlrSheet.DisbusmentAmount.ToString()) * (-1));

        //                                    salarySheetValue = 0.00;
        //                                    salarySheetValue = _dataSlrSheetNumber;
        //                                    sheet1.Range[xlsRow, ob.XLColIndex - 1].Number = salarySheetValue;
        //                                    sheet1.Range[xlsRow, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocalStructure(ob, localLanguage);
        //                                    sheet1.Range[xlsRow, ob.XLColIndex - 1, xlsRow, ob.XLColIndex].Merge();
        //                                    sheet1.Range[xlsRow, ob.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                                    getTotalAmount(ob.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                                }
        //                                else if (ob.HeadType == "E")
        //                                {
        //                                    DataView dvBonusData = new DataView(dsBonus.Tables[0]);
        //                                    dvBonusData.RowFilter = "SalaryHeadID='" + hId + "' and EmpSystemID='" + EmpIdPR + "'";
        //                                    sheet1.Range[xlsRow, ob.XLColIndex].BorderAround(ExcelLineStyle.Thin);
        //                                    if (dvBonusData.Count > 0)
        //                                    {
        //                                        salarySheetValue = 0.00;
        //                                        salarySheetValue = Convert.ToDouble(dvBonusData[0]["Bonus"].ToString());
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1].Number = salarySheetValue;
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocalStructure(ob, localLanguage);
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1, xlsRow, ob.XLColIndex].Merge();
        //                                        getTotalAmount(ob.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                                    }
        //                                    else
        //                                    {
        //                                        salarySheetValue = 0.00;
        //                                        if (_dataSlrSheet != null)
        //                                            salarySheetValue = Convert.ToDouble(_dataSlrSheet.EntryAmount.ToString());
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1].Number = salarySheetValue;
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocalStructure(ob, localLanguage);
        //                                        sheet1.Range[xlsRow, ob.XLColIndex - 1, xlsRow, ob.XLColIndex].Merge();
        //                                        sheet1.Range[xlsRow, ob.XLColIndex].CellStyle.Font.FontName = printFont;
        //                                        getTotalAmount(ob.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                                    }
        //                                }
        //                                #endregion
        //                            }
        //                        }//
        //                        #region common excel Row set up                            

        //                        sheet1.Range[xlsRow, ob.XLColIndex - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                        sheet1.Range[xlsRow, ob.XLColIndex - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        sheet1.Range[xlsRow, ob.XLColIndex - 1].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow, ob.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                        sheet1.Range[xlsRow, ob.XLColIndex - 1].CellStyle.Font.Size = 30;
        //                        #endregion
        //                    }//for dtSalaryHead

        //                    //gross-deduction
        //                    var grossIndex = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
        //                    var CTCIndex = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();

        //                    var dedIndex = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.XLColIndex).FirstOrDefault();
        //                    var dedFormula = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

        //                    var grossAdd = oRU.SetFormula(grossIndex.ToString(), xlsRow);
        //                    var CTCAdd = oRU.SetFormula(CTCIndex.ToString(), xlsRow);


        //                    var grossAddSalProc = oRU.SetFormula((grossIndex - 1).ToString(), xlsRow);
        //                    var CTCAddSalProc = oRU.SetFormula((CTCIndex - 1).ToString(), xlsRow);

        //                    var dedAddSalProc = oRU.SetFormula(dedFormula, xlsRow);

        //                    sheet1.Range[xlsRow, np - 1].Formula = "=" + dedAddSalProc;
        //                    sheet1.Range[xlsRow, np - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                    sheet1.Range[xlsRow, np - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[xlsRow, np - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, np - 1].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, np - 1].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow, np - 1].CellStyle.Font.Size = 26;
        //                    sheet1.Range[xlsRow, np - 1, xlsRow, np].Merge();


        //                    sheet1.Range[xlsRow, sigCol - 1].Formula = "=" + CTCAddSalProc + "-(" + dedAddSalProc + ")";//Net payable

        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Size = 40;
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].Merge();

        //                    sheet1.Range[xlsRow, colSignature, particular3rdRow + 3, colSignature].Merge();
        //                    sheet1.Range[xlsRow, colSignature, particular3rdRow + 3, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, 1, particular3rdRow + 3, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, 1, particular3rdRow + 3, colSignature].BorderInside(ExcelLineStyle.Thin);

        //                    #region DailyAttendance
        //                    dvDaily = new DataView(dsDaily);
        //                    dvDaily.RowFilter = "EmployeePK = '" + EmpIdPR + "' ";
        //                    var mnthColData = 0;
        //                    mnthColData = colParticulars;

        //                    var dtFrmDtIntData = 1;
        //                    var dtEndDateIntData = 31;
        //                    while (dtFrmDtIntData <= dtEndDateIntData)
        //                    {
        //                        mnthColData += 1;
        //                        var sc = _list2.Find(r => Convert.ToInt32(r.ValueMember) == dtFrmDtIntData);

        //                        var _day_status = "";
        //                        var _col_index = mnthColData;

        //                        if (sc != null && dvDaily.Count > 0)
        //                        {
        //                            _day_status = dvDaily[0][sc.DisplayMember].ToString();
        //                            _col_index += sc.ColIndex;

        //                            string[] dayStatusList = _day_status.Split(',');

        //                            var dayStatus = "";
        //                            if (dayStatusList[0] == "LV" || dayStatusList[0] == "W" || dayStatusList[0] == "H")
        //                            {
        //                                try
        //                                {
        //                                    Array.Clear(dayStatusList, 1, dayStatusList.Length - 1);
        //                                }
        //                                catch (Exception)
        //                                {

        //                                    throw;
        //                                }
        //                            }

        //                            for (int dst = 0; dst < dayStatusList.Length; dst++)
        //                            {
        //                                if (dayStatusList[dst] == "L")
        //                                {
        //                                    dayStatus = Convert.ToString(dayStatusList[dst]).Replace('L', 'P');
        //                                }
        //                                else
        //                                {
        //                                    dayStatus = dayStatusList[dst];
        //                                }
        //                                sheet1.Range[particular3rdRow + dst, _col_index].Text = dayStatus;
        //                                sheet1.Range[particular3rdRow + dst, _col_index].BorderAround(ExcelLineStyle.Thin);
        //                                sheet1.Range[particular3rdRow + dst, _col_index].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                                sheet1.Range[particular3rdRow + dst, _col_index].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[particular3rdRow + dst, _col_index].CellStyle.Font.FontName = "Arial Narrow";
        //                                sheet1.Range[particular3rdRow + dst, _col_index].CellStyle.Font.Size = 17;
        //                            }
        //                        }//date
        //                        else
        //                        {
        //                            for (int dst = 0; dst < 4; dst++)
        //                            {
        //                                sheet1.Range[particular3rdRow + dst, _col_index].Text = "";

        //                            }
        //                        }
        //                        dtFrmDt = dtFrmDt.AddDays(1);
        //                        dtFrmDtIntData++;
        //                    }
        //                    #endregion
        //                    xlsRow++;
        //                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;
        //                    xlsRow++;
        //                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;
        //                    xlsCol = np;
        //                    #region Border Setup
        //                    xlsRow = particular3rdRow + 4;
        //                    EmpCounter++;
        //                    if ((EmpCounter % 6) == 0)
        //                    {
        //                        sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);
        //                    }
        //                    #endregion
        //                    #endregion *************************Data End*************************

        //                    xlsRow--;

        //                }//for emp count //colSalaryDistrHead
        //                #region Summation of all Salary head
        //                sheet1.Range[xlsRow + 1, colParticulars].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Total.ToString(), "Total");
        //                sheet1.Range[xlsRow + 1, colParticulars].NumberFormat = oRU.NumberFormatDecimalZero();
        //                sheet1.Range[xlsRow + 1, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, colParticulars].RowHeight = 40;
        //                sheet1.Range[xlsRow + 1, colParticulars].CellStyle.Font.Size = 28;
        //                sheet1.Range[xlsRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                foreach (var item in totalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
        //                {
        //                    try
        //                    {
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].Number = Convert.ToDouble(item.Value);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].Merge();
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].RowHeight = 40;
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].CellStyle.Font.Size = 28;
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].CellStyle.Font.Bold = true;
        //                    }
        //                    catch (Exception exe)
        //                    {
        //                        throw exe;
        //                    }
        //                }//Loop End Last Summation in SalaryPorcess



        //                var grossIndexSubStructOSide = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
        //                var ctcIndexSubStructOSide = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();

        //                var grossSubStructOside = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.SalaryHead).FirstOrDefault();
        //                var ctcSubStructOside = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.SalaryHead).FirstOrDefault();

        //                var dedFormulaStructOSide = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

        //                var grossAddSubOSide = oRU.SetFormula((grossIndexSubStructOSide - 1).ToString(), xlsRow + 1);
        //                var ctcAddSubOSide = oRU.SetFormula((ctcIndexSubStructOSide - 1).ToString(), xlsRow + 1);
        //                var dedAddStructOSide = oRU.SetFormula(dedFormulaStructOSide, xlsRow + 1);

        //                sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].Formula = "=" + oRU.SetFormula(grossSubStructOside, xlsRow + 1);
        //                sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage); ;

        //                sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].CellStyle.Font.Size = 28;
        //                sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1, xlsRow + 1, grossIndexSubStructOSide].Merge();

        //                sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].Number = totalCTCSalary; //"=" + oRU.SetFormula(ctcSubStructOside, xlsRow + 1);
        //                sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage); ;
        //                sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].BorderAround(ExcelLineStyle.Thin);


        //                sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].CellStyle.Font.Size = 28;
        //                sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1, xlsRow + 1, ctcIndexSubStructOSide].Merge();


        //                var dedAddSubSalStructOSide = oRU.SetFormula(grossSubStructOside, xlsRow + 1);

        //                sheet1.Range[xlsRow + 1, np - 1].Formula = "=" + dedAddStructOSide;//Total Deduction
        //                sheet1.Range[xlsRow + 1, np - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                sheet1.Range[xlsRow + 1, np - 1].CellStyle.Font.Size = 28;
        //                sheet1.Range[xlsRow + 1, np - 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 1, np - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                sheet1.Range[xlsRow + 1, np - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow + 1, np - 1, xlsRow + 1, np].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, np - 1, xlsRow + 1, np].Merge();

        //                sheet1.Range[xlsRow + 1, colNetpayable].Formula = "=" + totalCTCSalary + "-(" + dedAddStructOSide + ")";//Net Payable
        //                sheet1.Range[xlsRow + 1, colNetpayable].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                sheet1.Range[xlsRow + 1, colNetpayable].CellStyle.Font.Size = 28;
        //                sheet1.Range[xlsRow + 1, colNetpayable].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 1, colNetpayable].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                sheet1.Range[xlsRow + 1, colNetpayable].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();

        //                sheet1.Range[xlsRow + 20, ColName].Text = "HR Executive";
        //                sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, ColName].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                //sheet1.Range[xlsRow + 8, ColLeaveInfo, xlsRow + 8, ColWorkDaysInfo].Borders(ExcelLineStyle.Thin);

        //                int numberOfColumns = colSignature - colParticulars;

        //                int remainCell = numberOfColumns - 24;
        //                var unmargedCell = closestNumber(remainCell, 3) / 3;
        //                int firstColumn = ColWorkDaysInfo + 1;
        //                sheet1.Range[xlsRow + 20, firstColumn].Text = "HR Manager";
        //                sheet1.Range[xlsRow + 20, firstColumn].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, firstColumn].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, firstColumn, xlsRow + 20, firstColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //                sheet1.Range[xlsRow + 20, firstColumn, xlsRow + 20, firstColumn + 7].Merge();

        //                int secondColumn = firstColumn + 7 + unmargedCell;
        //                sheet1.Range[xlsRow + 20, secondColumn].Text = "Head of HR";
        //                sheet1.Range[xlsRow + 20, secondColumn].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, secondColumn].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, secondColumn, xlsRow + 20, secondColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                sheet1.Range[xlsRow + 20, secondColumn, xlsRow + 20, secondColumn + 7].Merge();

        //                int thirdColumn = secondColumn + 7 + unmargedCell;
        //                sheet1.Range[xlsRow + 20, thirdColumn].Text = "Accounts Manager";
        //                sheet1.Range[xlsRow + 20, thirdColumn].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, thirdColumn].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, thirdColumn, xlsRow + 20, thirdColumn + 7].Merge();
        //                sheet1.Range[xlsRow + 20, thirdColumn, xlsRow + 20, thirdColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                sheet1.Range[xlsRow + 20, colSignature - 1].Text = "CFO/CEO";
        //                sheet1.Range[xlsRow + 20, colSignature - 1].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, colSignature - 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, colSignature - 1, xlsRow + 20, colSignature].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;


        //                sheet1.Range[xlsRow + 20, colSignature - 1, xlsRow + 20, colSignature].Merge();
        //                sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].RowHeight = 153;
        //                sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


        //                #endregion
        //                #endregion ----------------------Data End-----------------------
        //                #region ******************Report Header******************
        //                objRpt.SelectedPlantWiseCompany(plantId, languageId, out dsCmp);
        //                xlsRow = 1;
        //                xlsCol = 1;

        //                FactoryName = string.Empty;

        //                var FactoryAddress = string.Empty;

        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyNameLocal"].ToString();
        //                }
        //                else
        //                {
        //                    CmpName = "";
        //                }
        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantNameLocal"].ToString();
        //                }
        //                else
        //                {
        //                    FactoryName = "";
        //                }
        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddressLocal"].ToString();
        //                }
        //                else
        //                {
        //                    FactoryAddress = "";
        //                }
        //                sheet1.Range[xlsRow, 1].Text = FactoryName;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 55;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = printFont;

        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 63;

        //                xlsRow++;
        //                sheet1.Range[xlsRow, 1].Text = FactoryAddress;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 30;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 44;
        //                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

        //                sheet1.Range[xlsRow - 1, endXlsCol].Text = "Print Date: " + DateTime.Now.ToString("dd-MMM-yyy") + Environment.NewLine + "Payment Date:" + paymentDate;
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Font.Size = 24;
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Font.FontName = "ArialNarrow";
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

        //                string yearLocal = ru.cnDgt(Convert.ToDateTime(para.FromDate).Year.ToString(), localLanguage);

        //                xlsRow += 1;
        //                sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.SalarySheet.ToString(), "Salary Sheet") + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), localLanguage) + "-" + yearLocal; //_payRegisterLocal + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), "Bengali") + "," + yearLocal;
        //                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 35;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 51;
        //                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;



        //                #endregion ******************Report Header******************
        //                #region Freeze Panes
        //                sheet1.UsedRange["A7"].FreezePanes();
        //                sheet1.FirstVisibleColumn = 1;
        //                sheet1.FirstVisibleRow = 5;
        //                #endregion

        //                #region UsedRange Alignment
        //                sheet1.UsedRange.WrapText = true;
        //                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
        //                #endregion UsedRange Alignment
        //                sheet1.Protect("abc@iderd!" + DateTime.Now.ToString("dd-MMM-yyyy"), ExcelSheetProtection.All);
        //                #region Page Setup
        //                sheet1.PageSetup.TopMargin = 0.2;
        //                sheet1.PageSetup.BottomMargin = 0.7;

        //                sheet1.PageSetup.PrintTitleRows = "$A$1:$IV$6";
        //                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&15" + "Page " + "&p" + " of " + "&N";
        //                sheet1.PageSetup.LeftMargin = 0.5;
        //                sheet1.PageSetup.RightMargin = 0.2;
        //                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
        //                sheet1.PageSetup.FitToPagesTall = 0;
        //                sheet1.PageSetup.FitToPagesWide = 1;
        //                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperLegal;

        //                sheet1.Name = "EmpPayRegister" + para.SalaryProcessId;
        //                #endregion          
        //                return workbook;
        //            }

        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //            finally
        //            {
        //                objRpt = null;
        //                excelEngine = null;
        //                application = null;
        //                workbook = null;
        //                sheet1 = null;
        //            }
        //        }

        //        public IWorkbook EmployeeSalaryRegisterWithStructureNew(PayRegisterParamList PayRegisterParam, string paymentDate, string printDate, string sqlInStatement, bool isActive, bool isSeperated, bool isMaternity)
        //        {
        //            string plantId = PayRegisterParam.PlantId;
        //            string unitId = PayRegisterParam.UnitId;
        //            string divisionId = PayRegisterParam.DivisionId;
        //            string departmentId = PayRegisterParam.DepartmentId;
        //            string sectionId = PayRegisterParam.SectionId;
        //            string subsectionId = PayRegisterParam.SubSectionId;
        //            string month = PayRegisterParam.Month;
        //            string year = PayRegisterParam.Year;
        //            string salaryProcessId = PayRegisterParam.SalaryProcessId;
        //            string empSystmId = PayRegisterParam.EmployeeId;
        //            string empStatus = PayRegisterParam.EmpStatus;
        //            string payGroup = PayRegisterParam.PayGroup;
        //            string userId = PayRegisterParam.userId;
        //            string categoryId = PayRegisterParam.EmpCategoryId;
        //            string paymentMode = PayRegisterParam.PaymentMode;
        //            string languageId = PayRegisterParam.LanguageId;

        //            #region Variable
        //            clsReport objRpt = null;
        //            clsSalaryUtility objSalary = null;

        //            DataSet dsSlrProced = null;
        //            DataSet dsLeaveInfo = null;
        //            DataSet dsLeaveType = null;

        //            DataSet dsEmpAttdnInfo = null;
        //            DataView dvEmp = null;
        //            DataView dvLeaveEmp = null;
        //            DataView dvLeaveType = null;
        //            DataView dvSlrProc = null;
        //            DataSet dsCmp = null;
        //            DataSet dsFactory = null;
        //            DataSet dsBonus = null;

        //            ExcelEngine excelEngine = null;
        //            IApplication application = null;
        //            IWorkbook workbook = null;
        //            IWorksheet sheet1 = null;
        //            ReportUtility ru = null;
        //            ParamList para = new ParamList();
        //            ParamList leavePara = new ParamList();
        //            ParamList attdnProcessParam = new ParamList();

        //            var sUnit = unitId;
        //            var sDevi = divisionId;
        //            var sDept = departmentId;
        //            var sSect = sectionId;
        //            var sSbSect = subsectionId;

        //            para.UnitId = unitId;
        //            para.DivisionId = divisionId;
        //            para.DepartmentId = departmentId;
        //            para.SectionId = sectionId;
        //            para.SubSectionId = subsectionId;
        //            para.PlantId = plantId;
        //            para.EmpCategorId = categoryId;
        //            para.LanguageId = languageId;
        //            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
        //            DataTable dtLunchOuthr = null;
        //            DataTable dtSalaryHead = null;
        //            #endregion Variable

        //            try
        //            {
        //                Dictionary<string, SalaryHeadSequence> strListNew = null;
        //                ru = new ReportUtility();
        //                objRpt = new clsReport(_sqlRepository);
        //                objSalary = new clsSalaryUtility();
        //                ParaMontlyAttendance objm = new ParaMontlyAttendance();
        //                #region Variable             

        //                var FactoryName = "";
        //                var CmpName = "";

        //                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
        //                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
        //                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
        //                var fdateOfMonth = "1" + "-" + monthName + "-" + year;

        //                var labelList = ru.LocalLanguageLabelList(para.PlantId, languageId);
        //                var DTpayGroup = payRollGroup(payGroup);
        //                var DTEmployeeCatg = EmpCategory(categoryId);

        //                var localLanguage = "";
        //                var payGroupName = "";
        //                var empCategory = "";
        //                var printFont = "";
        //                bool isLocalLanguage = false;
        //                localLanguage = ru.LocalLanguageListSql(plantId, languageId, out isLocalLanguage);
        //                if (localLanguage == "Bengali")
        //                {
        //                    printFont = "SolaimanLipi";
        //                }
        //                else
        //                {
        //                    printFont = "Arial Narrow";

        //                }
        //                if (DTpayGroup.Rows.Count > 0)
        //                {
        //                    payGroupName = DTpayGroup.Rows[0]["UserName"].ToString();
        //                }
        //                if (DTEmployeeCatg.Rows.Count > 0)
        //                {
        //                    empCategory = DTEmployeeCatg.Rows[0]["UserName"].ToString();
        //                }


        //                DataView dvDaily = null;
        //                objm.AMonth = month;
        //                objm.AYear = year;
        //                objm.PlantId = plantId;
        //                objm.FDate = fdateOfMonth;
        //                objm.TDate = ldateOfMonth;
        //                var _ShiftCode = string.Empty;

        //                var salarySheetValue = 0.00;

        //                para.PlantId = plantId;

        //                leavePara.PlantId = plantId;

        //                para.EmployeeId = empSystmId;
        //                para.FromDate = fdateOfMonth;
        //                para.ToDate = ldateOfMonth;
        //                para.SalaryProcessId = salaryProcessId;
        //                para.EmpStatus = empStatus;
        //                para.PayGroup = payGroup;
        //                para.SubSectionId = subsectionId;
        //                para.SectionId = sectionId;
        //                para.CompanyGroupId = PayRegisterParam.CompanyGroupId;

        //                para.CompanyId = PayRegisterParam.CompanyId;

        //                leavePara.EmployeeId = empSystmId;
        //                leavePara.FromDate = fdateOfMonth;
        //                leavePara.SalaryProcessId = salaryProcessId;
        //                leavePara.EmpStatus = empStatus;
        //                #endregion Variable
        //                DateTime dtFrmDt = DateTime.Now;
        //                DateTime dtEndDate = DateTime.Now;

        //                string m = ru.GetMonthName(month);

        //                #region DataSet

        //                //List<SalaryStructureReport> listdsSlrStruct = new List<SalaryStructureReport>();//SalaryStruct
        //                List<SalarySheetReportStructure> listdsSlrSheet = new List<SalarySheetReportStructure>();//SalarySheetReport
        //                string stringSalaryRegSorting = "";

        //                stringSalaryRegSorting = objRpt.GetSortingParameters(para.CompanyGroupId, para.CompanyId, para.PlantId, "");
        //                //objRpt.GetEmpSalaryStructureRegisterAndPaySlipRpt(para, paymentMode, out dsSlrStruct);  //Salary Structure
        //                //objRpt.GetSalaryInfoSlrProcIDWiseForRegister(para, paymentMode, languageId, sqlInStatement, stringSalaryRegSorting, isActive, isSeperated, isMaternity, out dsSlrProced);
        //                DataSet dsEmpLoyeeInfo = new DataSet();


        //                GetEmployeeInfoDetailPayRollGroup(para, para.CompanyGroupId, para.CompanyId, plantId, fdateOfMonth, ldateOfMonth, languageId, stringSalaryRegSorting, null, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo);//Sql Query For Salary  Data
        //                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetailPayRollGroup(para, para.CompanyGroupId, para.CompanyId, plantId, fdateOfMonth, ldateOfMonth, languageId, null, isActive, isSeperated, isMaternity, out dtSalaryHead);

        //                DataTable dtEmployees = dsEmpLoyeeInfo.Tables[0];

        //                //dvSlrProc = new DataView();
        //                //dvSlrProc.Table = dsSlrProced.Tables[0];
        //                //if (dsSlrProced.Tables[0].Rows.Count > 0)
        //                //{
        //                //    listdsSlrSheet = dsSlrProced.Tables[0].ToList<SalarySheetReportStructure>();
        //                //}

        //                //dvEmp = new DataView();
        //                //dvEmp.Table = dsSlrProced.Tables[0];
        //                //var dtEmployees = dvEmp.ToTable(true, "EmpInfoSystemID", "DivisionName", "DivisionId", "SubDivision", "SubdivisionID", "UnitName", "UnitID", "DepartmentName", "DepartmentID", "SectionName", "SectionID", "SubSectionName", "SubSectionID", "LDDesignationGD", "DesignationLocal", "GradeCode", "DOJ", "EmployeeName", "EmployeeNameLocal", "FatherName", "DOS", "EmployeeCode", "BankAccNo", "IsOTEntitle");

        //                //var dtEmployees = dvEmp.ToTable(true, "SystemID", "Division", "DivisionId", "SubDivision", "SubdivisionID", "Unit", "UnitID", "Department", "DepartmentID", "Section", "SectionID", "SubSection", "SubSectionID", "LDDesignationGD", "DesignationLocal", "GradeCode", "DOJ", "EmployeeName", "EmployeeNameLocal", "FatherName", "DOS", "EmployeeCode", "SalaryHeadValue", "BankAccNo", "IsOTEntitle");
        //                if (dtEmployees.Rows.Count == 0)
        //                {
        //                    var ex = new Exception("No Data found...");
        //                    throw (ex);
        //                }

        //                //if (dsSlrStruct.Tables[0].Rows.Count > 0)
        //                //{
        //                //    listdsSlrStruct = dsSlrStruct.Tables[0].ToList<SalaryStructureReport>();
        //                //}


        //                DataTable dsDaily = GetMonthlyDailyAttendance(_ShiftCode, objm);
        //                List<SwapColumn> _list2 = GetColDisplayName(dsDaily);


        //                objRpt.SelectedPlantWiseCompany(plantId, languageId, out dsCmp);

        //                objRpt.SelectedPlant(plantId, out dsFactory);

        //                #endregion DataSet

        //                excelEngine = new ExcelEngine();
        //                application = excelEngine.Excel;

        //                workbook = application.Workbooks.Create(1);
        //                sheet1 = workbook.Worksheets[0];
        //                sheet1.IsGridLinesVisible = true;
        //                sheet1.IsDisplayZeros = false;
        //                #region------------------Column Header------------------
        //                xlsRow = 5;
        //                xlsCol = 1;

        //                var ColSr = 0;
        //                var ColName = 0;
        //                var ColLeaveInfo = 0;
        //                var ColWorkDaysInfo = 0;
        //                var colParticulars = 0;
        //                var ColGrs = 0;
        //                #endregion------------------Column Header------------------

        //                int RowIndex = xlsRow + 1;

        //                #region ----------------------Data-----------------------
        //                var strSubDivision = "0";
        //                var strSection = "0";
        //                var strDiv = "0";
        //                var strUnit = "0";
        //                var strDepartment = "0";
        //                var strSubSection = "0";

        //                var SrNo = 0;
        //                var EmpIdPR = "";
        //                var oRU = new ReportUtility();
        //                var intRow = 0;
        //                xlsRow = RowIndex;

        //                //List<SalaryHeadSequence> list = null;

        //                var np = 0;
        //                var isFirst = true;
        //                var sigCol = 0;
        //                var deptFirstRow = 0;

        //                xlsRow--;


        //                var totalDictSalaryStruct = new Dictionary<string, double>();
        //                var totalDictSalaryProcess = new Dictionary<string, double>();
        //                var totalNetPayDisbusmentAmount = 0.00;

        //                int endCol = 5;
        //                int colNetpayable = endCol;
        //                int colSignature = endCol;
        //                #region RegisterHeader
        //                var colex = 0;
        //                sigCol = 0;




        //                if (isFirst == false)
        //                {
        //                    xlsRow += 3;
        //                    colex = 2;
        //                }
        //                #region ------------------Column Header------------------
        //                xlsCol = 1;
        //                var lineHeader = "";
        //                if (!paymentMode.IsNullOrEmpty() && paymentMode != "null" && paymentMode != "undefined")
        //                    lineHeader += paymentMode;
        //                if (!empCategory.IsNullOrEmpty())
        //                {
        //                    if (!paymentMode.IsNullOrEmpty() && paymentMode != "null" && paymentMode != "undefined")
        //                    {
        //                        lineHeader += "~";
        //                    }
        //                    lineHeader += empCategory;

        //                }
        //                if (!payGroup.IsNullOrEmpty())
        //                {
        //                    if (!paymentMode.IsNullOrEmpty() && paymentMode != "null" && paymentMode != "undefined")
        //                    {
        //                        lineHeader += "~";
        //                    }
        //                    if (!empCategory.IsNullOrEmpty())
        //                    {
        //                        lineHeader += "~";
        //                    }
        //                    lineHeader += payGroupName;

        //                }

        //                sheet1.Range[xlsRow - 1, xlsCol].Text = lineHeader;
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.Size = 48;
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow - 1, xlsCol, xlsRow - 1, xlsCol + 3].Merge();
        //                sheet1.Range[xlsRow - 1, xlsCol].RowHeight = 52;

        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.SrNo.ToString(), "Sr. No."), sheet1, xlsRow + colex, ref xlsCol, out ColSr, 15, printFont, 90);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.EmployeeInformation.ToString(), "Employee Information"), sheet1, xlsRow + colex, ref xlsCol, out ColName, 100, printFont, 0);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.LeaveInformation.ToString(), "Leave Information"), sheet1, xlsRow + colex, ref xlsCol, out ColLeaveInfo, 35, printFont, 0);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.WorkDaysDetail.ToString(), "Work Days Detail"), sheet1, xlsRow + colex, ref xlsCol, out ColWorkDaysInfo, 60, printFont, 0);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.Particulars.ToString(), "Particulars"), sheet1, xlsRow + colex, ref xlsCol, out colParticulars, 28, printFont, 0);
        //                ColGrs = colParticulars;
        //                var _count_earning_head = 0;
        //                var _count_earning_ctchead = 0;
        //                var _count_deducting_head = 0;
        //                var _total_head_count = 0;

        //                //DataView dvSalaryHead = new DataView(dsSlrProced.Tables[0]);

        //                //dvSalaryHead.Sort = "HeadType desc,Sequence";

        //                // DataTable dtSalaryHead = dvSalaryHead.ToTable(true, "SalaryHeadID", "SalaryHead", "SalaryHeadBangla", "HeadType", "Sequence", "HeadCategory", "IsCTCComponent", "IsGrossComponent", "IntegerInDisb", "DecimalNo", "IsNetPayEffect", "PartOfNetPay");

        //                //DataTable dtVPFHead = dvSalaryHead.ToTable(true, "xHeadCategory", "xSalaryHeadID", "xSalaryHead", "xIsCTCComponent", "xIsGrossComponent", "xHeadType", "IntegerInDisb", "DecimalNo");

        //                //objRpt.GetBonus(month, year, out dsBonus);
        //                //DataView dvBonus = new DataView(dsBonus.Tables[0]);
        //                //DataTable dtBonusHead = dvBonus.ToTable(true, "SalaryHeadID", "HeadCategory", "SalaryHead", "IsCTCComponent", "IsGrossComponent", "HeadType", "Sequence");

        //                //OTSBD.clsSalary.clsSalaryReport sr = new OTSBD.clsSalary.clsSalaryReport();

        //                // sr.SetSheetBonus(dtBonusHead, ref dtSalaryHead);

        //                xlsRow += colex;

        //                CreateDynamicSHeadLocalLanguageStructNewWithAttendance(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out strListNew, labelList, printFont, false, false, false);

        //                //CreateDynamicSHeadLocalLanguageStructNewWithAttendance(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list, labelList, printFont);
        //                xlsRow -= colex;
        //                xlsRow += intRow;
        //                intRow = 1;

        //                endCol = 5;

        //                #region Day of a month 
        //                var mnthCol = 0;
        //                mnthCol = colParticulars;

        //                var dtFrmDtInt = 1;
        //                var dtEndDateInt = 31;
        //                while (dtFrmDtInt <= dtEndDateInt)
        //                {
        //                    mnthCol += 1;
        //                    var sc = _list2.Find(r => Convert.ToInt32(r.ValueMember) == dtFrmDtInt);

        //                    var _col_index = mnthCol;
        //                    sheet1.Range[xlsRow, _col_index].Text = dtFrmDtInt.ToString();
        //                    sheet1.Range[xlsRow, _col_index].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
        //                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Bold = true;
        //                    sheet1.Range[xlsRow, _col_index].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                    sheet1.Range[xlsRow, _col_index].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.FontName = "Arial Narrow";
        //                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Size = 30;
        //                    sheet1.Range[xlsRow, _col_index].ColumnWidth = 13;
        //                    dtFrmDtInt++; ;
        //                }
        //                #endregion
        //                endCol = colParticulars;
        //                if (strListNew.Count > 0)
        //                {
        //                    xlsCol++;
        //                    np = ColGrs + strListNew.Count * 2;
        //                    endCol = np + 1;


        //                    //sheet1.Range[xlsRow + 1, np - 1].Text = ru.GetLabelname(labelList, "TotalDeduction", "Total Deduction"); //totalDeduction.LocalLabel == null || totalDeduction.LocalLabel == "" ? totalDeduction.DefaultLabel : totalDeduction.LocalLabel;
        //                    //sheet1.Range[xlsRow + 1, np - 1].CellStyle.ShrinkToFit = true;
        //                    //sheet1.Range[xlsRow + 1, np - 1].CellStyle.Font.FontName = printFont;
        //                    //sheet1.Range[xlsRow + 1, np - 1].CellStyle.Font.Size = 22;
        //                    //sheet1.Range[xlsRow + 1, np - 1, xlsRow + 1, np].Merge();
        //                    xlsCol++;
        //                }

        //                if (endCol <= colParticulars + 29)
        //                    endCol = colParticulars + 30;

        //                colNetpayable = endCol;

        //                sheet1.Range[xlsRow + 1, endCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.NetPayable.ToString(), "Net Payable");//netPayable.LocalLabel == null || netPayable.LocalLabel == "" ? netPayable.DefaultLabel : netPayable.LocalLabel;
        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();
        //                sheet1.Range[xlsRow + 1, endCol].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow + 1, endCol].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 1, colNetpayable].ColumnWidth = 18;
        //                sheet1.Range[xlsRow + 1, colNetpayable + 1].ColumnWidth = 18;



        //                colSignature = colNetpayable + 2;

        //                sheet1.Range[xlsRow, colSignature].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Signature.ToString(), "Signature");//empSignatgure.LocalLabel == null || empSignatgure.LocalLabel == "" ? empSignatgure.DefaultLabel : empSignatgure.LocalLabel;
        //                sheet1.Range[xlsRow, colSignature, xlsRow + 1, colSignature].Merge();
        //                sheet1.Range[xlsRow, colSignature].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, colSignature].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow, colSignature].ColumnWidth = 70;

        //                endCol = colSignature;

        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].BorderAround(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].BorderInside(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, endCol].RowHeight = 127;

        //                endXlsCol = endCol;
        //                sigCol = endCol;
        //                xlsCol = 1;
        //                xlsRow++;
        //                int EmpCounter = 0;

        //                deptFirstRow = xlsRow;

        //                #endregion

        //                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
        //                {
        //                    salarySheetValue = 0.00;
        //                    //ctcSalarydd = 0.00;

        //                    #endregion ------------------Column Header------------------

        //                    #region *************************Data*************************
        //                    //strDiv = dtEmployees.Rows[i]["DivisionID"].ToString().Trim();
        //                    //strSubDivision = dtEmployees.Rows[i]["SubdivisionID"].ToString().Trim();
        //                    //strUnit = dtEmployees.Rows[i]["UnitID"].ToString().Trim();
        //                    //strDepartment = dtEmployees.Rows[i]["DepartmentID"].ToString().Trim();
        //                    //strSection = dtEmployees.Rows[i]["SectionID"].ToString().Trim();
        //                    //strSubSection = dtEmployees.Rows[i]["SubSectionID"].ToString().Trim();

        //                    xlsRow++;
        //                    #region LeaveInformation
        //                    leavePara.EmployeeId = dtEmployees.Rows[i]["SystemID"].ToString();

        //                    objRpt.GetLeaveTypeLocal(leavePara, languageId, out dsLeaveType);
        //                    dvLeaveType = new DataView();

        //                    dvLeaveType.Table = dsLeaveType.Tables[0];

        //                    objRpt.GetEmpLeaveInfoLaila(leavePara, out dsLeaveInfo);
        //                    dvLeaveEmp = new DataView();

        //                    dvLeaveEmp.Table = dsLeaveInfo.Tables[0];
        //                    var EL = GetLeaveEmp(dvLeaveEmp, "EL");
        //                    var CL = GetLeaveEmp(dvLeaveEmp, "CL");
        //                    var SL = GetLeaveEmp(dvLeaveEmp, "SL");

        //                    var locEL = GetLeaveType(dvLeaveType, "EL");
        //                    var locCL = GetLeaveType(dvLeaveType, "CL");
        //                    var locSL = GetLeaveType(dvLeaveType, "SL");


        //                    #endregion


        //                    attdnProcessParam.EmployeeId = dtEmployees.Rows[i]["SystemID"].ToString();
        //                    objRpt.GetMonthlyAttdnInfo(attdnProcessParam, month, year, out dsEmpAttdnInfo);

        //                    DataTable dtEmpAttdnInfo = dsEmpAttdnInfo.Tables[0];

        //                    dtLunchOuthr = GetLunchOutHour(dtEmployees.Rows[i]["SystemID"].ToString(), fdateOfMonth, ldateOfMonth);//Get lunch Out Hr

        //                    #region EmpInfo
        //                    SrNo += 1;
        //                    EmpIdPR = dtEmployees.Rows[i]["SystemID"].ToString().Trim();

        //                    sheet1.Range[xlsRow, ColSr].Text = ru.cnDgt(Convert.ToString(SrNo), localLanguage);
        //                    sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                    sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, ColSr, xlsRow + 4, ColSr].Merge();
        //                    sheet1.Range[xlsRow, ColSr, xlsRow + 4, ColSr].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, ColSr].CellStyle.Font.FontName = "Arial Narrow";
        //                    sheet1.Range[xlsRow, ColSr].CellStyle.Font.Size = 25;

        //                    //3
        //                    var _DOJ = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOJ.ToString(), "DOJ");
        //                    var _DOS = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOS.ToString(), "DOS");
        //                    var _designationLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Designation.ToString(), "Designation");
        //                    var _gradeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Grade.ToString(), "Grade");
        //                    var empName = "";
        //                    if (isLocalLanguage)
        //                    {
        //                        empName = dtEmployees.Rows[i]["EmployeeNameLocal"].ToString();
        //                    }
        //                    else
        //                    {
        //                        empName = dtEmployees.Rows[i]["EmployeeName"].ToString();

        //                    }
        //                    var empDesignation = _designationLocal + ":" + dtEmployees.Rows[i]["DesignationLocal"].ToString();
        //                    var empDOJ = _DOJ + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOJ"].ToString(), localLanguage);
        //                    var bankAccount = "";
        //                    var empDOS = string.Empty;
        //                    if (dtEmployees.Rows[i]["DOS"].ToString().Length > 0)
        //                    {
        //                        empDOS = _DOS + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOS"].ToString(), localLanguage); //+ Environment.NewLine;
        //                    }
        //                    if (dtEmployees.Rows[i]["BankAccNo"].ToString().Length > 0)
        //                    {
        //                        bankAccount = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bank Acc") + ":" + dtEmployees.Rows[i]["BankAccNo"].ToString(); //+ Environment.NewLine;
        //                    }


        //                    sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString() + Environment.NewLine + empName;

        //                    sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                    sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, ColName].BorderAround(ExcelLineStyle.Hair);
        //                    sheet1.Range[xlsRow, ColName].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow, ColName].CellStyle.Font.Bold = true;
        //                    sheet1.Range[xlsRow, ColName].CellStyle.Font.Size = 50;

        //                    // Designation DOJ DOS
        //                    IRichTextString rtf1 = sheet1.Range[xlsRow + 1, ColName].RichText;
        //                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                    FormatText(ref sheet1, ref rtf1, _designationLocal + ":" + dtEmployees.Rows[i]["DesignationLocal"].ToString() + " ", 25);
        //                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                    FormatText(ref sheet1, ref rtf1, _gradeLocal + ":", 25); FormatText(ref sheet1, ref rtf1, dtEmployees.Rows[i]["GradeCode"].ToString() + " ", 25);
        //                    if (dtEmployees.Rows[i]["BankAccNo"].ToString().Length > 0)
        //                    {
        //                        bankAccount = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bank Acc No") + ":" + dtEmployees.Rows[i]["BankAccNo"].ToString(); //+ Environment.NewLine;
        //                        FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf1, bankAccount + " ", 25);
        //                    }

        //                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                    FormatText(ref sheet1, ref rtf1, empDOJ + " ", 25);
        //                    if (dtEmployees.Rows[i]["DOS"].ToString().Length > 0)
        //                    {
        //                        empDOS = _DOS + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOS"].ToString(), localLanguage); //+ Environment.NewLine;
        //                        FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf1, empDOS + " ", 25);
        //                    }


        //                    sheet1.Range[xlsRow + 1, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                    sheet1.Range[xlsRow + 1, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow + 1, ColName, xlsRow + 4, ColName].Merge();
        //                    sheet1.Range[xlsRow + 1, ColName, xlsRow + 4, ColName].BorderAround(ExcelLineStyle.Thin);

        //                    //Leave Info
        //                    if (dtEmpAttdnInfo.Rows.Count > 0)
        //                    {
        //                        var lunchOutHr = 0.00;
        //                        if (dtLunchOuthr.Rows.Count > 0)
        //                        {
        //                            lunchOutHr = Convert.ToDouble(dtLunchOuthr.Rows[0]["LunchOutHour"].ToString());
        //                        }
        //                        string lateBangla = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalLate"]).ToString(), localLanguage);
        //                        string presentBangla = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalPresentLate"]).ToString(), localLanguage);
        //                        string absentBangla = ru.cnDgt((Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalAbsent"]) - Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalLWP"])).ToString(), localLanguage);
        //                        string weekOff = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalWeekOffPlusWeekOffHoliDay"]).ToString(), localLanguage);
        //                        string holiDay = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalHoliDay"]).ToString(), localLanguage);
        //                        string leave = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalLv"]).ToString(), localLanguage);
        //                        string totalOTHr = "";//ru.cnDgt((Math.Round((Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalNormalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalExtraOTHr"])) / 60, 2).ToString()), localLanguage);
        //                        string lunchOutHrLocal = ru.cnDgt(lunchOutHr.ToString(), localLanguage);
        //                        var _availedLeave = ru.GetLabelname(labelList, LabelNameInLocalLanguage.AvailedLeave.ToString(), "Availed Leave");

        //                        IRichTextString rtfLeave = sheet1.Range[xlsRow, ColLeaveInfo].RichText;
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locEL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(EL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locCL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(CL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locSL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(SL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, _availedLeave + ":" + ru.cnDgt(Math.Round(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalLv"].ToString()), 0).ToString(), localLanguage) + " ", 25);

        //                        sheet1.Range[xlsRow, ColLeaveInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                        sheet1.Range[xlsRow, ColLeaveInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 4, ColLeaveInfo].Merge();
        //                        sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 4, ColLeaveInfo].BorderAround(ExcelLineStyle.Thin);

        //                        string payDays = ru.cnDgt(Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalProcDate"]).ToString(), localLanguage);
        //                        //DataView dvBasic = new DataView(dsSlrProced.Tables[0]);
        //                        //dvBasic.RowFilter = "SalaryHead='Basic' and EmpInfoSystemID='" + EmpIdPR + "' and IsOTEntitle = true";//For OT Rate Calculation


        //                        double otRate = 0.00;
        //                        string otRateBangla = "";
        //                        if (Convert.ToBoolean(dtEmpAttdnInfo.Rows[0]["IsOTEntitled"]) == true)
        //                        {
        //                            totalOTHr = ru.cnDgt((Math.Round((Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalNormalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalExtraOTHr"])) / 60, 2).ToString()), localLanguage);
        //                            otRate = Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["OTRate"]);
        //                            otRateBangla = ru.cnDgt(Math.Round(otRate, 2).ToString(), localLanguage);
        //                        }
        //                        IRichTextString rtf = sheet1.Range[xlsRow, ColWorkDaysInfo].RichText;

        //                        var _payDaysLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.PayDays.ToString(), "Pay Days");
        //                        var _presentLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Present.ToString(), "Present");
        //                        var _absentLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Absent.ToString(), LabelNameInLocalLanguage.Absent.ToString());//absentLocal.LocalLabel == null || absentLocal.LocalLabel == "" ? absentLocal.DefaultLabel : absentLocal.LocalLabel;
        //                        var _lunchOutHour = ru.GetLabelname(labelList, LabelNameInLocalLanguage.LunchOutHour.ToString(), "Lunch Out Hr.: ");//lunchOutHour.LocalLabel == null || lunchOutHour.LocalLabel == "" ? lunchOutHour.DefaultLabel : lunchOutHour.LocalLabel;
        //                        var _late = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Late.ToString(), "Late");
        //                        var _otHour = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OTHours.ToString(), "OT Hrs");
        //                        var _otRateLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OTRate.ToString(), "OT Rate");
        //                        var _weeklyHolidays = ru.GetLabelname(labelList, LabelNameInLocalLanguage.WeeklyLeaveDays.ToString(), "Weekend");
        //                        var _holiDays = ru.GetLabelname(labelList, LabelNameInLocalLanguage.HoliDay.ToString(), "Holidays");

        //                        FormatText(ref sheet1, ref rtf, _payDaysLocal + ":" + payDays + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _presentLocal + ":" + presentBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _absentLocal + ":" + absentBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _weeklyHolidays + ":" + weekOff + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _holiDays + ":" + holiDay + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _lunchOutHour + ":" + lunchOutHrLocal + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _late + ":" + lateBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        if (Convert.ToBoolean(dtEmpAttdnInfo.Rows[0]["IsOTEntitled"]) == true)
        //                        {
        //                            FormatText(ref sheet1, ref rtf, _otHour + ":" + totalOTHr + " ", 27);
        //                            FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                            FormatText(ref sheet1, ref rtf, _otRateLocal + ":" + otRateBangla + " ", 27);
        //                        }


        //                        sheet1.Range[xlsRow, ColWorkDaysInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                        sheet1.Range[xlsRow, ColWorkDaysInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 4, ColWorkDaysInfo].Merge();
        //                        sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 4, ColWorkDaysInfo].BorderAround(ExcelLineStyle.Thin);
        //                    }

        //                    //var earnedSalary = labelList.Where(r => r.DefaultLabel == LabelNameInLocalLanguage.EarnedSalary.ToString()).FirstOrDefault();
        //                    var earnedSalary = "";// labelList.Where(r => r.DefaultLabel == LabelNameInLocalLanguage.EarnedSalary.ToString()).FirstOrDefault();
        //                    if (labelList.ContainsKey(LabelNameInLocalLanguage.EarnedSalary.ToString()))
        //                    {
        //                        earnedSalary = labelList[LabelNameInLocalLanguage.EarnedSalary.ToString()];
        //                    }
        //                    var _earnedSalary = ru.GetLabelname(labelList, LabelNameInLocalLanguage.EarnedSalary.ToString(), "Earned Salary");
        //                    sheet1.Range[xlsRow, colParticulars].Text = _earnedSalary + "->";
        //                    sheet1.Range[xlsRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[xlsRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Size = 24;
        //                    sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Bold = true;

        //                    var _daysLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Days.ToString(), "Days");

        //                    var particular3rdRow = xlsRow + 1;

        //                    var _attendance = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Attendance.ToString(), "Attendance");

        //                    sheet1.Range[particular3rdRow, colParticulars].Text = _attendance + "->";
        //                    sheet1.Range[particular3rdRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[particular3rdRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[particular3rdRow, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[particular3rdRow, colParticulars].RowHeight = 43;
        //                    sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.Size = 17;
        //                    sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.Bold = true;

        //                    var _inTimeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.INTime.ToString(), "INTime");
        //                    var _outTimeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OUTTime.ToString(), "OUTTime");
        //                    var _OTLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OT.ToString(), "OT");

        //                    sheet1.Range[particular3rdRow + 1, colParticulars].Text = _inTimeLocal + "->";
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].RowHeight = 43;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.Size = 17;
        //                    sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                    sheet1.Range[particular3rdRow + 2, colParticulars].Text = _outTimeLocal + "->";
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].RowHeight = 43;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.Size = 17;
        //                    sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.Bold = true;

        //                    sheet1.Range[particular3rdRow + 3, colParticulars].Text = _OTLocal + "->";
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].RowHeight = 43;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.Size = 17;
        //                    sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.Bold = true;
        //                    #endregion

        //                    var _total_head_count_body = 0;
        //                    //for (int ci = 0; ci < strListNew.Count; ci++)
        //                    //{


        //                    //var ob = strListNew[ci];
        //                    //if (ob.SalaryHead.Length > 0)
        //                    //{

        //                    //var hId = ob.SalaryHeadId;
        //                    _total_head_count_body++;

        //                    #region SalaryProcess 

        //                    if (dicEmpSalry.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
        //                    {
        //                        List<DataRow> drSalaryHeadCollection = dicEmpSalry[dtEmployees.Rows[i]["EmpSystemID"].ToString()];


        //                        for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
        //                        {
        //                            if (drSalaryHeadCollection[CI]["SalaryHead"].ToString().Contains("Tax"))
        //                            {

        //                            }
        //                            try
        //                            {
        //                                SalaryHeadSequence xx = new SalaryHeadSequence();

        //                                if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
        //                                {
        //                                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
        //                                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable].NumberFormat = ru.GetDecimalFormatlocalNetPay(Convert.ToBoolean(drSalaryHeadCollection[CI]["IntegerInDisb"].ToString()), Convert.ToInt32(drSalaryHeadCollection[CI]["DecimalNo"].ToString()), localLanguage);
        //                                    getTotalAmount(colNetpayable.ToString(), Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()), ref totalDictSalaryProcess);

        //                                }
        //                                if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "GROSS")
        //                                {

        //                                }
        //                                try
        //                                {
        //                                    xx = strListNew[drSalaryHeadCollection[CI]["SalaryHeadID"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();

        //                                }
        //                                catch (Exception)
        //                                {
        //                                    xx = null;
        //                                }
        //                                if (xx != null)
        //                                {
        //                                    var slrStructureAmount = 0.00;
        //                                    if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
        //                                    {
        //                                        slrStructureAmount = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * -1;
        //                                        //getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref subTotalDictSalaryStruct);//SubTotal
        //                                        //getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref totalDictSalaryStruct);
        //                                        sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                        sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                        sheet1.Range[xlsRow, xx.XLColIndex].Number = slrStructureAmount; //; clsStaticInfo.dbl(drSalaryHeadCollection[CI]["EntryAmount"].ToString());// * (-1);
        //                                        getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref totalDictSalaryProcess);

        //                                    }

        //                                    else
        //                                    {

        //                                        if (bplib.clsWebLib.GetBoolData(drSalaryHeadCollection[CI]["IsGrossComponent"]))
        //                                        {
        //                                            slrStructureAmount = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["EntryAmount"].ToString());
        //                                            //getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref subTotalDictSalaryStruct);//SubTotal
        //                                            //getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref totalDictSalaryStruct);
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].Number = slrStructureAmount; //; clsStaticInfo.dbl(drSalaryHeadCollection[CI]["EntryAmount"].ToString());// * (-1);
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                            getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref totalDictSalaryProcess);
        //                                        }
        //                                        else
        //                                        {
        //                                            slrStructureAmount = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
        //                                            //getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref subTotalDictSalaryStruct);//SubTotal
        //                                            //getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref totalDictSalaryStruct);
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].Number = slrStructureAmount; //; clsStaticInfo.dbl(drSalaryHeadCollection[CI]["EntryAmount"].ToString());// * (-1);
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                            getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref totalDictSalaryProcess);

        //                                        }
        //                                    }

        //                                    //sheet1.Range[xlsRow, xx.XLColIndex].Number = slrStructureAmount;
        //                                    sheet1.Range[xlsRow, xx.XLColIndex, xlsRow, xx.XLColIndex].NumberFormat = ru.GetDecimalFormatlocal(xx, localLanguage);
        //                                    sheet1.Range[xlsRow, xx.XLColIndex - 1, xlsRow, xx.XLColIndex].Merge();// = ru.GetDecimalFormatlocal(xx, localLanguage);

        //                                    sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                    sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                    sheet1.Range[xlsRow, xx.XLColIndex].BorderAround(ExcelLineStyle.Thin);
        //                                    sheet1.Range[xlsRow, xx.XLColIndex].CellStyle.Font.FontName = printFont;
        //                                    sheet1.Range[xlsRow, xx.XLColIndex - 1, xlsRow, xx.XLColIndex].CellStyle.Font.Size = 30;

        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            {

        //                                throw ex;
        //                            }


        //                        }

        //                    }


        //                    //var _dataSlrSheet = listdsSlrSheet.Where(r => r.SalaryHeadID == hId && r.EmpInfoSystemID == EmpIdPR).FirstOrDefault();
        //                    //double _dataSlrSheetNumber = 0;

        //                    //if (ob.HeadType == "D")
        //                    //{
        //                    //    if (_dataSlrSheet != null)
        //                    //        _dataSlrSheetNumber = (Convert.ToDouble(_dataSlrSheet.DisbusmentAmount.ToString()) * (-1));

        //                    //    salarySheetValue = 0.00;
        //                    //    salarySheetValue = _dataSlrSheetNumber;
        //                    //    sheet1.Range[xlsRow, ob.XLColIndex - 1].Number = salarySheetValue;
        //                    //    sheet1.Range[xlsRow, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocal(ob, localLanguage);
        //                    //    sheet1.Range[xlsRow, ob.XLColIndex - 1, xlsRow, ob.XLColIndex].Merge();
        //                    //    sheet1.Range[xlsRow, ob.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                    //    getTotalAmount(ob.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                    //}
        //                    //else if (ob.HeadType == "E")
        //                    //{

        //                    //    salarySheetValue = 0.00;
        //                    //    if (_dataSlrSheet != null)
        //                    //    {
        //                    //        if (ob.IsGrossComponent == true)
        //                    //            salarySheetValue = Convert.ToDouble(_dataSlrSheet.EntryAmount.ToString());
        //                    //        else
        //                    //            salarySheetValue = Convert.ToDouble(_dataSlrSheet.DisbusmentAmount.ToString());

        //                    //    }
        //                    //    sheet1.Range[xlsRow, ob.XLColIndex - 1].Number = salarySheetValue;
        //                    //    sheet1.Range[xlsRow, ob.XLColIndex - 1].NumberFormat = ru.GetDecimalFormatlocal(ob, localLanguage);
        //                    //    sheet1.Range[xlsRow, ob.XLColIndex - 1, xlsRow, ob.XLColIndex].Merge();
        //                    //    sheet1.Range[xlsRow, ob.XLColIndex].CellStyle.Font.FontName = printFont;
        //                    //    getTotalAmount(ob.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                    //}

        //                    #endregion
        //                    //}//
        //                    #region common excel Row set up                            


        //                    #endregion
        //                    //}//for dtSalaryHead

        //                    //DataView dvSheetNetPay = new DataView(dsSlrProced.Tables[0]);
        //                    //dvSheetNetPay.RowFilter = "HeadCategory = 'Net Payable' and EmpInfoSystemID=" + EmpIdPR + "";

        //                    //if (dvSheetNetPay.Count > 0)
        //                    //{
        //                    //    sheet1.Range[xlsRow, sigCol - 1].Number = Convert.ToDouble(dvSheetNetPay[0]["DisbusmentAmount"].ToString()); //+ Environment.NewLine + dvSheetNetPay[0]["PaymentMode"].ToString();

        //                    //    totalNetPayDisbusmentAmount += Convert.ToDouble(dvSheetNetPay[0]["DisbusmentAmount"].ToString());
        //                    //}

        //                    //"=" + CTCAddSalProc + "-(" + dedAddSalProc + ")";//Net payable

        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Size = 40;
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                    sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].Merge();

        //                    sheet1.Range[xlsRow, colSignature, particular3rdRow + 3, colSignature].Merge();
        //                    sheet1.Range[xlsRow, colSignature, particular3rdRow + 3, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, 1, particular3rdRow + 3, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, 1, particular3rdRow + 3, colSignature].BorderInside(ExcelLineStyle.Thin);

        //                    #region DailyAttendance
        //                    dvDaily = new DataView(dsDaily);
        //                    dvDaily.RowFilter = "EmployeePK = '" + EmpIdPR + "' ";
        //                    var mnthColData = 0;
        //                    mnthColData = colParticulars;

        //                    var dtFrmDtIntData = 1;
        //                    var dtEndDateIntData = 31;
        //                    while (dtFrmDtIntData <= dtEndDateIntData)
        //                    {
        //                        mnthColData += 1;
        //                        var sc = _list2.Find(r => Convert.ToInt32(r.ValueMember) == dtFrmDtIntData);

        //                        var _day_status = "";
        //                        var _col_index = mnthColData;

        //                        if (sc != null && dvDaily.Count > 0)
        //                        {
        //                            _day_status = dvDaily[0][sc.DisplayMember].ToString();
        //                            _col_index += sc.ColIndex;

        //                            string[] dayStatusList = _day_status.Split(',');

        //                            var dayStatus = "";
        //                            if (dayStatusList[0] == "LV" || dayStatusList[0] == "W" || dayStatusList[0] == "H" || dayStatusList[0] == "A")
        //                            {
        //                                try
        //                                {
        //                                    Array.Clear(dayStatusList, 1, dayStatusList.Length - 1);
        //                                }
        //                                catch (Exception)
        //                                {

        //                                    throw;
        //                                }
        //                            }

        //                            for (int dst = 0; dst < dayStatusList.Length; dst++)
        //                            {
        //                                if (dayStatusList[dst] == "L")
        //                                {
        //                                    dayStatus = Convert.ToString(dayStatusList[dst]).Replace('L', 'P');
        //                                }
        //                                else
        //                                {
        //                                    dayStatus = dayStatusList[dst];
        //                                }
        //                                sheet1.Range[particular3rdRow + dst, _col_index].Text = dayStatus;
        //                                sheet1.Range[particular3rdRow + dst, _col_index].BorderAround(ExcelLineStyle.Thin);
        //                                sheet1.Range[particular3rdRow + dst, _col_index].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                                sheet1.Range[particular3rdRow + dst, _col_index].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[particular3rdRow + dst, _col_index].CellStyle.Font.FontName = "Arial Narrow";
        //                                sheet1.Range[particular3rdRow + dst, _col_index].CellStyle.Font.Size = 17;
        //                            }
        //                        }//date
        //                        else
        //                        {
        //                            for (int dst = 0; dst < 4; dst++)
        //                            {
        //                                sheet1.Range[particular3rdRow + dst, _col_index].Text = "";

        //                            }
        //                        }
        //                        dtFrmDt = dtFrmDt.AddDays(1);
        //                        dtFrmDtIntData++;
        //                    }
        //                    #endregion
        //                    xlsRow++;
        //                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;
        //                    xlsRow++;
        //                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
        //                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;
        //                    xlsCol = np;
        //                    #region Border Setup
        //                    xlsRow = particular3rdRow + 4;
        //                    EmpCounter++;
        //                    if ((EmpCounter % 6) == 0)
        //                    {
        //                        sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);
        //                    }
        //                    #endregion
        //                    #endregion *************************Data End*************************

        //                    xlsRow--;

        //                }//for emp count //colSalaryDistrHead
        //                #region Summation of all Salary head
        //                sheet1.Range[xlsRow + 1, colParticulars].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Total.ToString(), "Total");
        //                sheet1.Range[xlsRow + 1, colParticulars].NumberFormat = oRU.NumberFormatDecimalZero();
        //                sheet1.Range[xlsRow + 1, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, colParticulars].RowHeight = 40;
        //                sheet1.Range[xlsRow + 1, colParticulars].CellStyle.Font.Size = 28;
        //                sheet1.Range[xlsRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                foreach (var item in totalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
        //                {
        //                    try
        //                    {
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].Number = Convert.ToDouble(item.Value);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].Merge();
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].RowHeight = 40;
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].CellStyle.Font.Size = 28;
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].CellStyle.Font.Bold = true;
        //                    }
        //                    catch (Exception exe)
        //                    {
        //                        throw exe;
        //                    }
        //                }//Loop End Last Summation in SalaryPorcess



        //                //var grossIndexSubStructOSide = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
        //                //var ctcIndexSubStructOSide = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();

        //                //var grossSubStructOside = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.SalaryHead).FirstOrDefault();
        //                //var ctcSubStructOside = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.SalaryHead).FirstOrDefault();

        //                //var dedFormulaStructOSide = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

        //                //var grossAddSubOSide = oRU.SetFormula((grossIndexSubStructOSide - 1).ToString(), xlsRow + 1);
        //                //var ctcAddSubOSide = oRU.SetFormula((ctcIndexSubStructOSide - 1).ToString(), xlsRow + 1);
        //                //var dedAddStructOSide = oRU.SetFormula(dedFormulaStructOSide, xlsRow + 1);

        //                //sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].Formula = "=" + oRU.SetFormula(grossSubStructOside, xlsRow + 1);
        //                //sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].BorderAround(ExcelLineStyle.Thin);
        //                //sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage); ;

        //                //sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].CellStyle.Font.Size = 28;
        //                //sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].CellStyle.Font.Bold = true;
        //                //sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1, xlsRow + 1, grossIndexSubStructOSide].Merge();

        //                //sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].Number = totalCTCSalary; //"=" + oRU.SetFormula(ctcSubStructOside, xlsRow + 1);
        //                //sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage); ;
        //                //sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].BorderAround(ExcelLineStyle.Thin);


        //                //sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].CellStyle.Font.Size = 28;
        //                //sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1].CellStyle.Font.Bold = true;
        //                //sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide - 1, xlsRow + 1, ctcIndexSubStructOSide].Merge();


        //                //var dedAddSubSalStructOSide = oRU.SetFormula(grossSubStructOside, xlsRow + 1);

        //                //sheet1.Range[xlsRow + 1, np - 1].Formula = "=" + dedAddStructOSide;//Total Deduction
        //                //sheet1.Range[xlsRow + 1, np - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                //sheet1.Range[xlsRow + 1, np - 1].CellStyle.Font.Size = 28;
        //                //sheet1.Range[xlsRow + 1, np - 1].CellStyle.Font.Bold = true;
        //                //sheet1.Range[xlsRow + 1, np - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                //sheet1.Range[xlsRow + 1, np - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                //sheet1.Range[xlsRow + 1, np - 1, xlsRow + 1, np].BorderAround(ExcelLineStyle.Thin);
        //                //sheet1.Range[xlsRow + 1, np - 1, xlsRow + 1, np].Merge();

        //                sheet1.Range[xlsRow + 1, colNetpayable].Number = Convert.ToDouble(totalNetPayDisbusmentAmount);//"=" + totalCTCSalary + "-(" + dedAddStructOSide + ")";//Net Payable
        //                sheet1.Range[xlsRow + 1, colNetpayable].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                sheet1.Range[xlsRow + 1, colNetpayable].CellStyle.Font.Size = 28;
        //                sheet1.Range[xlsRow + 1, colNetpayable].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 1, colNetpayable].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                sheet1.Range[xlsRow + 1, colNetpayable].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();

        //                sheet1.Range[xlsRow + 20, ColName].Text = "Prepared By";
        //                sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, ColName].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                //sheet1.Range[xlsRow + 8, ColLeaveInfo, xlsRow + 8, ColWorkDaysInfo].Borders(ExcelLineStyle.Thin);

        //                //int numberOfColumns = colSignature - colParticulars;

        //                //int remainCell = numberOfColumns - 24;
        //                //var unmargedCell = closestNumber(remainCell, 3) / 3;
        //                sheet1.Range[xlsRow + 20, ColName].Text = "HR Executive";
        //                sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, ColName].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                //sheet1.Range[xlsRow + 8, ColLeaveInfo, xlsRow + 8, ColWorkDaysInfo].Borders(ExcelLineStyle.Thin);

        //                int numberOfColumns = colSignature - colParticulars;

        //                int remainCell = numberOfColumns - 24;
        //                var unmargedCell = closestNumber(remainCell, 3) / 3;
        //                int firstColumn = ColWorkDaysInfo + 1;
        //                sheet1.Range[xlsRow + 20, firstColumn].Text = "HR Manager";
        //                sheet1.Range[xlsRow + 20, firstColumn].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, firstColumn].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, firstColumn, xlsRow + 20, firstColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //                sheet1.Range[xlsRow + 20, firstColumn, xlsRow + 20, firstColumn + 7].Merge();

        //                int secondColumn = firstColumn + 7 + unmargedCell;
        //                sheet1.Range[xlsRow + 20, secondColumn].Text = "Head of HR";
        //                sheet1.Range[xlsRow + 20, secondColumn].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, secondColumn].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, secondColumn, xlsRow + 20, secondColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                sheet1.Range[xlsRow + 20, secondColumn, xlsRow + 20, secondColumn + 7].Merge();

        //                int thirdColumn = secondColumn + 7 + unmargedCell;
        //                sheet1.Range[xlsRow + 20, thirdColumn].Text = "Accounts Manager";
        //                sheet1.Range[xlsRow + 20, thirdColumn].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, thirdColumn].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, thirdColumn, xlsRow + 20, thirdColumn + 7].Merge();
        //                sheet1.Range[xlsRow + 20, thirdColumn, xlsRow + 20, thirdColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                sheet1.Range[xlsRow + 20, colSignature - 1].Text = "CFO/CEO";
        //                sheet1.Range[xlsRow + 20, colSignature - 1].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, colSignature - 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, colSignature - 1, xlsRow + 20, colSignature].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;


        //                sheet1.Range[xlsRow + 20, colSignature - 1, xlsRow + 20, colSignature].Merge();
        //                sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].RowHeight = 153;
        //                sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


        //                #endregion
        //                #endregion ----------------------Data End-----------------------
        //                #region ******************Report Header******************
        //                objRpt.SelectedPlantWiseCompany(plantId, languageId, out dsCmp);
        //                xlsRow = 1;
        //                xlsCol = 1;

        //                FactoryName = string.Empty;

        //                var FactoryAddress = string.Empty;

        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyNameLocal"].ToString();
        //                }
        //                else
        //                {
        //                    CmpName = "";
        //                }
        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantNameLocal"].ToString();
        //                }
        //                else
        //                {
        //                    FactoryName = "";
        //                }
        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddressLocal"].ToString();
        //                }
        //                else
        //                {
        //                    FactoryAddress = "";
        //                }
        //                sheet1.Range[xlsRow, 1].Text = FactoryName;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 55;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = printFont;

        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 63;

        //                xlsRow++;
        //                sheet1.Range[xlsRow, 1].Text = FactoryAddress;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 30;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 44;
        //                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

        //                sheet1.Range[xlsRow - 1, endXlsCol].Text = "Print Date: " + DateTime.Now.ToString("dd-MMM-yyy") + Environment.NewLine + "Payment Date:" + paymentDate;
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Font.Size = 24;
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Font.FontName = "ArialNarrow";
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

        //                string yearLocal = ru.cnDgt(Convert.ToDateTime(para.FromDate).Year.ToString(), localLanguage);

        //                xlsRow += 1;
        //                sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.SalarySheet.ToString(), "Salary Sheet") + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), localLanguage) + "-" + yearLocal; //_payRegisterLocal + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), "Bengali") + "," + yearLocal;
        //                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 35;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 51;
        //                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;



        //                #endregion ******************Report Header******************
        //                #region Freeze Panes
        //                sheet1.UsedRange["A7"].FreezePanes();
        //                sheet1.FirstVisibleColumn = 1;
        //                sheet1.FirstVisibleRow = 5;
        //                #endregion

        //                #region UsedRange Alignment
        //                sheet1.UsedRange.WrapText = true;
        //                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
        //                #endregion UsedRange Alignment
        //                //sheet1.Protect("abc@dter" + DateTime.Now.ToString("dd-MMM-yyyy") + plantId, ExcelSheetProtection.All);
        //                #region Page Setup
        //                sheet1.PageSetup.TopMargin = 0.2;
        //                sheet1.PageSetup.BottomMargin = 0.7;

        //                sheet1.PageSetup.PrintTitleRows = "$A$1:$IV$6";
        //                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&15" + "Page " + "&p" + " of " + "&N";
        //                sheet1.PageSetup.LeftMargin = 0.5;
        //                sheet1.PageSetup.RightMargin = 0.2;
        //                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
        //                sheet1.PageSetup.FitToPagesTall = 0;
        //                sheet1.PageSetup.FitToPagesWide = 1;
        //                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperLegal;

        //                sheet1.Name = "EmpPayRegister" + para.SalaryProcessId;
        //                #endregion          
        //                return workbook;
        //            }

        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //            finally
        //            {
        //                objRpt = null;
        //                excelEngine = null;
        //                application = null;
        //                workbook = null;
        //                sheet1 = null;
        //            }
        //        }

        //        public IWorkbook NewEmployeeSalaryRegisterWithStructure(string companyGroupId, string companyId, string plantId, string year, string month, string languageId, string paymentDate, string printDate, string fromDate, string toDate, string groupBy, Dictionary<string, string> parameters, string salaryProcessId, string sheetBasedOn, bool withAttendance, string paperSize, string docGrouping, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity, bool onlyEarning)
        //        {
        //            #region Variable
        //            clsReport objRpt = null;
        //            clsSalaryUtility objSalary = null;
        //            DataSet dsLeaveType = null;
        //            DataView dvLeaveType = null;
        //            DataSet dsCmp = null;
        //            DataSet dsFactory = null;
        //            DataSet dsEmpLoyeeInfo = null;
        //            var StartDayCol = 0;
        //            DataTable dtSalaryHead = null;

        //            ExcelEngine excelEngine = null;
        //            IApplication application = null;
        //            IWorkbook workbook = null;
        //            IWorksheet sheet1 = null;
        //            ReportUtility ru = null;
        //            ParamList para = new ParamList();
        //            ParamList leavePara = new ParamList();
        //            ParamList attdnProcessParam = new ParamList();

        //            var pageHeightDelemeter = 0;
        //            var groupWisePageHeightDelemeter = 0;
        //            var empModulasFactor = 0;


        //            para.PlantId = plantId;
        //            para.LanguageId = languageId;
        //            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
        //            var slrStartCol = 0;
        //            #endregion Variable
        //            try
        //            {
        //                ru = new ReportUtility();
        //                objRpt = new clsReport(_sqlRepository);
        //                objSalary = new clsSalaryUtility();
        //                ParaMontlyAttendance objm = new ParaMontlyAttendance();
        //                #region Variable             

        //                var FactoryName = "";
        //                var CmpName = "";

        //                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
        //                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
        //                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
        //                var fdateOfMonth = "1" + "-" + monthName + "-" + year;

        //                Dictionary<string, string> labelList = ru.LocalLanguageLabelList(para.PlantId, languageId);

        //                var localLanguage = "";
        //                var printFont = "";
        //                bool isLocalLanguage = false;
        //                localLanguage = ru.LocalLanguageListSql(plantId, languageId, out isLocalLanguage);
        //                if (localLanguage == "Bengali")
        //                {
        //                    printFont = "SolaimanLipi";
        //                }
        //                else
        //                {
        //                    printFont = "Arial Narrow";
        //                }
        //                objm.AMonth = month;
        //                objm.AYear = year;
        //                objm.PlantId = plantId;
        //                objm.FDate = fdateOfMonth;
        //                objm.TDate = ldateOfMonth;
        //                var _ShiftCode = string.Empty;
        //                var salarySheetValue = 0.00;
        //                para.PlantId = plantId;
        //                leavePara.PlantId = plantId;
        //                para.FromDate = fdateOfMonth;
        //                para.ToDate = ldateOfMonth;
        //                para.SalaryProcessId = salaryProcessId;
        //                leavePara.FromDate = fdateOfMonth;
        //                leavePara.SalaryProcessId = salaryProcessId;
        //                #endregion Variable
        //                DateTime dtFrmDt = DateTime.Now;
        //                DateTime dtEndDate = DateTime.Now;
        //                double totalNetPayDisbusmentAmount = 0.00;
        //                double subTotalNetPayDisbusmentAmount = 0.00;
        //                double totalBankPayDisbusmentAmount = 0.00;
        //                double totalCashPayDisbusmentAmount = 0.00;
        //                double subTotalBankPayDisbusmentAmount = 0.00;
        //                double subTotalCashPayDisbusmentAmount = 0.00;


        //                bool ExcludeFatherName = false;
        //                bool ExcludeNonpayable_Notional = false;
        //                bool ExcludeTotalGross = false;
        //                bool ExcludeCTC = false;

        //                int StructreAndEarningExceptAttendance = 0;
        //                int EarningExceptAttendance = 0;
        //                int StructureAndEarningWithAttendance = 0;
        //                int EarningWithAttendance = 0;

        //                GetPayRegisgeterConfig(companyId, plantId, out ExcludeFatherName, out ExcludeNonpayable_Notional, out ExcludeTotalGross, out ExcludeCTC);
        //                GetPayRegisgeterRowPerPage(companyId, plantId, out StructreAndEarningExceptAttendance, out EarningExceptAttendance, out StructureAndEarningWithAttendance, out EarningWithAttendance);

        //                if (withAttendance && sheetBasedOn == "structured")
        //                {
        //                    empModulasFactor = StructureAndEarningWithAttendance;
        //                }
        //                if (!withAttendance && sheetBasedOn == "structured")
        //                {

        //                    empModulasFactor = StructreAndEarningExceptAttendance;
        //                }
        //                if (withAttendance && sheetBasedOn != "structured")
        //                {
        //                    empModulasFactor = EarningWithAttendance;
        //                }
        //                if (onlyEarning)
        //                {
        //                    empModulasFactor = EarningExceptAttendance;
        //                }

        //                DataTable dtSigConfig = _sqlRepository.GetDataTable(@"SELECT * FROM PayRegisterSignatoryField WHERE PlantId = '" + plantId + @"' ORDER BY Sequence");
        //                string m = ru.GetMonthName(month);

        //                #region DataSet               

        //                #region Sorting Parameters
        //                string stringSalaryRegSorting = "";

        //                stringSalaryRegSorting = objRpt.GetSortingParameters(companyGroupId, companyId, plantId, groupBy);
        //                #endregion

        //                GetEmployeeInfoDetail(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, languageId, stringSalaryRegSorting, parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo);//Sql Query For Salary  Data
        //                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetail(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, languageId, parameters, isActive, isSeperated, isMaternity, out dtSalaryHead);

        //                var dtEmployees = dsEmpLoyeeInfo.Tables[0];
        //                if (dtEmployees.Rows.Count == 0)
        //                {
        //                    var ex = new Exception("No Data found...");
        //                    throw (ex);
        //                }
        //                List<SwapColumn> _list2 = new List<SwapColumn>();
        //                Dictionary<string, List<DataRow>> dicAttendance = new Dictionary<string, List<DataRow>>();
        //                if (withAttendance == true)
        //                {
        //                    dicAttendance = GetMonthlyDailyAttendanceDic(objm, parameters);
        //                }

        //                objRpt.SelectedPlantWiseCompany(plantId, languageId, out dsCmp);

        //                objRpt.SelectedPlant(plantId, out dsFactory);

        //                #endregion DataSet

        //                excelEngine = new ExcelEngine();
        //                application = excelEngine.Excel;

        //                workbook = application.Workbooks.Create(1);
        //                sheet1 = workbook.Worksheets[0];
        //                sheet1.IsGridLinesVisible = true;
        //                sheet1.IsDisplayZeros = false;

        //                #region------------------Column Header------------------
        //                xlsRow = 5;
        //                xlsCol = 1;

        //                var ColSr = 0;
        //                var ColName = 0;
        //                var ColLeaveInfo = 0;
        //                var ColWorkDaysInfo = 0;
        //                var colParticulars = 0;
        //                var ColGrs = 0;
        //                #endregion------------------Column Header------------------

        //                int RowIndex = xlsRow + 1;

        //                #region ----------------------Data-----------------------

        //                var strGroupBySel = "";
        //                var SrNo = 0;
        //                var EmpIdPR = "";
        //                var oRU = new ReportUtility();
        //                var intRow = 0;
        //                xlsRow = RowIndex;

        //                List<SalaryHeadSequence> list = null;

        //                var np = 0;
        //                var isFirst = true;
        //                var sigCol = 0;
        //                var deptFirstRow = 0;

        //                xlsRow--;
        //                Dictionary<string, SalaryHeadSequence> strListNew = null;

        //                var totalDictSalaryStruct = new Dictionary<string, double>();
        //                var totalDictSalaryProcess = new Dictionary<string, double>();

        //                var subTotalDictSalaryStruct = new Dictionary<string, double>();
        //                var subTotalDictSalaryProcess = new Dictionary<string, double>();

        //                int secondRowHeight = 0;
        //                int endCol = 5;
        //                int colNetpayable = endCol;
        //                int colSignature = endCol;
        //                #region RegisterHeader
        //                var colex = 0;
        //                sigCol = 0;


        //                OTSBD.clsSalary.clsSalaryReport sr = new OTSBD.clsSalary.clsSalaryReport();
        //                var titleRow = "6";
        //                #region ------------------Column Header Will be deleted------------------
        //                //if (string.IsNullOrEmpty(groupBy))
        //                //{
        //                xlsCol = 1;
        //                var lineHeader = "";
        //                if (!string.IsNullOrEmpty(docGrouping))
        //                {
        //                    lineHeader = docGrouping;
        //                }
        //                sheet1.Range[xlsRow - 1, xlsCol].Text = lineHeader;
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.Size = 48;
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow - 1, xlsCol, xlsRow - 1, xlsCol + 3].Merge();
        //                sheet1.Range[xlsRow - 1, xlsCol].RowHeight = 52;



        //                pageHeightDelemeter += 52;
        //                groupWisePageHeightDelemeter += 52;
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.SrNo.ToString(), "Sr. No."), sheet1, xlsRow + colex, ref xlsCol, out ColSr, 15, printFont, 90, 35);

        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.EmployeeInformation.ToString(), "Employee Information"), sheet1, xlsRow + colex, ref xlsCol, out ColName, 130, printFont, 0, 35);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.LeaveInformation.ToString(), "Leave Information"), sheet1, xlsRow + colex, ref xlsCol, out ColLeaveInfo, 50, printFont, 0, 35);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.WorkDaysDetail.ToString(), "Work Days Detail"), sheet1, xlsRow + colex, ref xlsCol, out ColWorkDaysInfo, 50, printFont, 0, 35);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.Particulars.ToString(), "Particulars"), sheet1, xlsRow + colex, ref xlsCol, out colParticulars, 28, printFont, 0, 35);
        //                ColGrs = colParticulars;
        //                var _count_earning_head = 0;
        //                var _count_earning_ctchead = 0;
        //                var _count_deducting_head = 0;
        //                var _total_head_count = 0;

        //                xlsRow += colex;
        //                if (withAttendance)
        //                {
        //                    slrStartCol = xlsCol;
        //                    CreateDynamicSHeadLocalLanguageStructNewWithAttendance(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out strListNew, labelList, printFont, ExcludeNonpayable_Notional, ExcludeTotalGross, ExcludeCTC);
        //                }
        //                if (!withAttendance)
        //                {

        //                    slrStartCol = xlsCol;
        //                    CreateDynamicSHeadLocalLanguageStructNewWithAttendance(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out strListNew, labelList, printFont, ExcludeNonpayable_Notional, ExcludeTotalGross, ExcludeCTC);

        //                    //CreateDynamicSHeadLocalLanguageStructNew(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out strListNew, labelList, printFont);

        //                    if (string.IsNullOrEmpty(groupBy))
        //                    {
        //                        sheet1.Range[xlsRow, colParticulars + 1].Text = "Earning";
        //                        sheet1.Range[xlsRow, colParticulars + 1].RowHeight = 63;
        //                        pageHeightDelemeter += 63;
        //                        groupWisePageHeightDelemeter += 63;
        //                        sheet1.Range[xlsRow, colParticulars + 1].CellStyle.Font.Size = 35;

        //                        sheet1.Range[xlsRow, colParticulars + 1, xlsRow, _count_earning_head + 4].Merge();

        //                        sheet1.Range[xlsRow, _count_earning_head + 5].Text = "Deduction";
        //                        sheet1.Range[xlsRow, _count_earning_head + 5].CellStyle.Font.Size = 35;
        //                        sheet1.Range[xlsRow, _count_earning_head + 5, xlsRow, _count_earning_head + 4 + _count_deducting_head].Merge();
        //                    }
        //                }
        //                xlsRow -= colex;


        //                #region Day of a month 
        //                StartDayCol = colParticulars;
        //                if (withAttendance == true)
        //                {
        //                    var mnthCol = 0;
        //                    mnthCol = colParticulars;
        //                    var dtFrmDtInt = 1;
        //                    var dtEndDateInt = 31;
        //                    while (dtFrmDtInt <= dtEndDateInt)
        //                    {
        //                        mnthCol += 1;
        //                        var sc = _list2.Find(r => Convert.ToInt32(r.ValueMember) == dtFrmDtInt);

        //                        var _col_index = mnthCol;
        //                        sheet1.Range[xlsRow, _col_index].Text = dtFrmDtInt.ToString();
        //                        sheet1.Range[xlsRow, _col_index].BorderAround(ExcelLineStyle.Hair);
        //                        sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
        //                        sheet1.Range[xlsRow, _col_index].CellStyle.Font.Bold = true;
        //                        sheet1.Range[xlsRow, _col_index].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                        sheet1.Range[xlsRow, _col_index].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        sheet1.Range[xlsRow, _col_index].CellStyle.Font.FontName = "Arial Narrow";
        //                        sheet1.Range[xlsRow, _col_index].CellStyle.Font.Size = 30;
        //                        sheet1.Range[xlsRow, _col_index].ColumnWidth = 13;
        //                        dtFrmDtInt++;
        //                    }
        //                }
        //                xlsRow += intRow;
        //                intRow = 1;
        //                endCol = 5;
        //                #endregion
        //                endCol = colParticulars;
        //                if (strListNew.Count > 0)
        //                {
        //                    xlsCol++;
        //                    np = ColGrs + strListNew.Count * 2;
        //                    endCol = np + 1;
        //                    xlsCol++;
        //                }
        //                if (withAttendance)
        //                {
        //                    if (endCol <= colParticulars + 29)
        //                        endCol = colParticulars + 30;

        //                }

        //                colNetpayable = endCol;

        //                sheet1.Range[xlsRow + 1, endCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.NetPayable.ToString(), "Net Payable");
        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();
        //                sheet1.Range[xlsRow + 1, endCol].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow + 1, endCol].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 1, colNetpayable].ColumnWidth = 18;
        //                sheet1.Range[xlsRow + 1, colNetpayable + 1].ColumnWidth = 18;


        //                int colPaymentMode = colNetpayable + 2;



        //                int colBankPaymentPercentage = 0;
        //                int colCashPaymentPercentage = 0;

        //                DataTable dtbankCash = _sqlRepository.GetDataTable("SELECT * FROM EmployeeWiseBankCashAmount WHERE PlantId = '" + plantId + "' AND MonthNo = '" + month + @"' AND YearNo  ='" + year + @"'");


        //                if (dtbankCash.Rows.Count > 0)
        //                {
        //                    colBankPaymentPercentage = colPaymentMode;
        //                    sheet1.Range[xlsRow, colBankPaymentPercentage].Text = "Bank";
        //                    sheet1.Range[xlsRow, colBankPaymentPercentage, xlsRow + 1, colBankPaymentPercentage].Merge();
        //                    sheet1.Range[xlsRow, colBankPaymentPercentage].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow, colBankPaymentPercentage].ColumnWidth = 33;
        //                    sheet1.Range[xlsRow, colBankPaymentPercentage].CellStyle.Font.Size = 34;

        //                    colCashPaymentPercentage = colBankPaymentPercentage + 1;
        //                    sheet1.Range[xlsRow, colCashPaymentPercentage].Text = "Cash";
        //                    sheet1.Range[xlsRow, colCashPaymentPercentage, xlsRow + 1, colCashPaymentPercentage].Merge();
        //                    sheet1.Range[xlsRow, colCashPaymentPercentage].CellStyle.Font.FontName = printFont;
        //                    sheet1.Range[xlsRow, colCashPaymentPercentage].ColumnWidth = 33;
        //                    sheet1.Range[xlsRow, colCashPaymentPercentage].CellStyle.Font.Size = 34;


        //                    colPaymentMode = colCashPaymentPercentage + 1;
        //                }

        //                sheet1.Range[xlsRow, colPaymentMode].Text = "";
        //                sheet1.Range[xlsRow, colPaymentMode, xlsRow + 1, colPaymentMode].Merge();
        //                sheet1.Range[xlsRow, colPaymentMode].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, colPaymentMode].ColumnWidth = 10;
        //                colSignature = colPaymentMode + 1;

        //                sheet1.Range[xlsRow, colSignature].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Signature.ToString(), "Signature");
        //                sheet1.Range[xlsRow, colSignature, xlsRow + 1, colSignature].Merge();
        //                sheet1.Range[xlsRow, colSignature].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, colSignature].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow, colSignature].ColumnWidth = 70;

        //                endCol = colSignature;

        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].BorderAround(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].BorderInside(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

        //                sheet1.Range[xlsRow - 1, endCol - 3].Text = ru.GetLabelname(labelList, "Days in The Month", "Days in The Month") + ": " + daysInMonth;
        //                sheet1.Range[xlsRow - 1, endCol - 3].CellStyle.Font.FontName = "Arial Narrow";
        //                sheet1.Range[xlsRow - 1, endCol - 3].CellStyle.Font.Size = 35;
        //                sheet1.Range[xlsRow - 1, endCol - 3, xlsRow - 1, endCol].Merge();
        //                sheet1.Range[xlsRow - 1, endCol - 3, xlsRow - 1, endCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                sheet1.Range[xlsRow - 1, endCol - 3, xlsRow - 1, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

        //                endXlsCol = endCol;
        //                sigCol = endCol;
        //                xlsCol = 1;


        //                xlsRow++;
        //                //}
        //                int EmpCounter = 0;

        //                #endregion
        //                objRpt.GetLeaveTypeLocal(leavePara, languageId, out dsLeaveType);
        //                dvLeaveType = new DataView();

        //                dvLeaveType.Table = dsLeaveType.Tables[0];

        //                Dictionary<string, List<DataRow>> dicLeave = null;
        //                GetEmpLeaveInfo(leavePara, parameters, out dicLeave);
        //                var bankAccount = "";
        //                var empDOS = string.Empty;
        //                var empGross = string.Empty;
        //                double grossSalaryAmount = 0.00;

        //                var EL = 0.00;
        //                var CL = 0.00;
        //                var SL = 0.00;
        //                var locEL = GetLeaveType(dvLeaveType, "EL");
        //                var locCL = GetLeaveType(dvLeaveType, "CL");
        //                var locSL = GetLeaveType(dvLeaveType, "SL");
        //                bool pageBreakRequired = false;
        //                try
        //                {
        //                    for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
        //                    {
        //                        try
        //                        {
        //                            if (string.IsNullOrEmpty(groupBy) == false)
        //                            {

        //                                var groupCount = 0;
        //                                if ((string.Compare(strGroupBySel.ToUpper(), dtEmployees.Rows[i][groupBy + "ID"].ToString().Trim().ToUpper())) != 0)
        //                                {
        //                                    var ex = 0;
        //                                    if (isFirst == false)
        //                                    {

        //                                        sheet1.Range[xlsRow + 1, colParticulars].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Total.ToString(), "SubTotal");
        //                                        sheet1.Range[xlsRow + 1, colParticulars].NumberFormat = oRU.NumberFormatDecimalZero();
        //                                        sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].Merge();
        //                                        sheet1.Range[xlsRow + 1, colParticulars].BorderAround(ExcelLineStyle.Hair);
        //                                        sheet1.Range[xlsRow + 1, colParticulars].RowHeight = 45;
        //                                        sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                        pageHeightDelemeter += 40;
        //                                        groupWisePageHeightDelemeter += 40;
        //                                        sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].CellStyle.Font.Size = 32;
        //                                        sheet1.Range[xlsRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                                        foreach (var item in subTotalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
        //                                        {
        //                                            try
        //                                            {
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].Number = Convert.ToDouble(item.Value);
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Hair);
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].BorderInside(ExcelLineStyle.Hair);

        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].Merge();
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].RowHeight = 40;
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].CellStyle.Font.Size = 32;
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].CellStyle.Font.Bold = true;
        //                                            }
        //                                            catch (Exception exe)
        //                                            {
        //                                                throw exe;
        //                                            }
        //                                        }//Loop End Last Summation in SalaryPorcess totalNetPayDisbusmentAmount
        //                                        sheet1.Range[xlsRow + 1, colNetpayable].Number = subTotalNetPayDisbusmentAmount;
        //                                        sheet1.Range[xlsRow + 1, colNetpayable].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);

        //                                        if (dtbankCash.Rows.Count > 0)
        //                                        {
        //                                            sheet1.Range[xlsRow + 1, colBankPaymentPercentage].Number = subTotalBankPayDisbusmentAmount;
        //                                            sheet1.Range[xlsRow + 1, colBankPaymentPercentage].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                                            sheet1.Range[xlsRow + 1, colCashPaymentPercentage].Number = subTotalCashPayDisbusmentAmount;
        //                                            sheet1.Range[xlsRow + 1, colCashPaymentPercentage].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                                            sheet1.Range[xlsRow + 1, colCashPaymentPercentage, xlsRow + 1, colCashPaymentPercentage + 1].Merge();
        //                                            sheet1.Range[xlsRow + 1, colBankPaymentPercentage, xlsRow + 1, colBankPaymentPercentage + 1].Merge();

        //                                            sheet1.Range[xlsRow + 1, colBankPaymentPercentage, xlsRow + 1, colCashPaymentPercentage].CellStyle.Font.Size = 28;
        //                                            sheet1.Range[xlsRow + 1, colBankPaymentPercentage, xlsRow + 1, colCashPaymentPercentage].CellStyle.Font.Bold = true;
        //                                            sheet1.Range[xlsRow + 1, colBankPaymentPercentage, xlsRow + 1, colCashPaymentPercentage].BorderAround(ExcelLineStyle.Hair);
        //                                        }

        //                                        sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();
        //                                        sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].CellStyle.Font.Size = 28;
        //                                        sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                                        sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].BorderAround(ExcelLineStyle.Hair);

        //                                        ex = 1;
        //                                        //subTotalDictSalaryStruct = null;
        //                                        //subTotalDictSalaryProcess = null;
        //                                        subTotalDictSalaryStruct = new Dictionary<string, double>();
        //                                        subTotalDictSalaryProcess = new Dictionary<string, double>();
        //                                        subTotalNetPayDisbusmentAmount = 0;


        //                                        totalBankPayDisbusmentAmount = 0.00;
        //                                        totalCashPayDisbusmentAmount = 0.00;
        //                                        subTotalBankPayDisbusmentAmount = 0.00;
        //                                        subTotalCashPayDisbusmentAmount = 0.00;

        //                                        sheet1.HPageBreaks.Add(sheet1[xlsRow + 2, 1]);//Page Break after each group
        //                                        pageHeightDelemeter = 0;
        //                                        groupWisePageHeightDelemeter = 0;
        //                                        EmpCounter = 0;
        //                                        //empModulasFactor = empNoPerPage;
        //                                        //}
        //                                    }
        //                                    #region ------------------Column Header------------------
        //                                    xlsCol = 1;

        //                                    sheet1.Range[xlsRow + ex - 1, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

        //                                    xlsRow += ex;
        //                                    if (withAttendance)
        //                                    {

        //                                        titleRow = "6";
        //                                    }
        //                                    if (!withAttendance)
        //                                    {
        //                                        titleRow = "5";
        //                                    }

        //                                    endCol = 5;
        //                                    xlsCol = 1;

        //                                    sheet1.Range[xlsRow + 1, 1].Text = groupBy + " :-" + dtEmployees.Rows[i][groupBy + "Name"].ToString();
        //                                    sheet1.Range[xlsRow + 1, 1, xlsRow + 1, 5].Merge();
        //                                    sheet1.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;
        //                                    sheet1.Range[xlsRow + 1, 1].CellStyle.Font.Size = 48;
        //                                    sheet1.Range[xlsRow + 1, 1].RowHeight = 50;
        //                                    pageHeightDelemeter += 100;
        //                                    groupWisePageHeightDelemeter += 100;
        //                                    xlsRow++;

        //                                    deptFirstRow = xlsRow;
        //                                    #endregion

        //                                    if (isFirst == true)
        //                                    {
        //                                        isFirst = false;
        //                                    }
        //                                }
        //                                groupCount++;
        //                                strGroupBySel = dtEmployees.Rows[i][groupBy + "ID"].ToString();
        //                            }
        //                        }
        //                        catch (Exception)
        //                        {
        //                            throw;
        //                        }
        //                        salarySheetValue = 0.00;

        //                        #endregion ------------------Column Header------------------

        //                        #region *************************Data*************************


        //                        xlsRow++;
        //                        #region LeaveInformation

        //                        EL = 0.00;
        //                        CL = 0.00;
        //                        SL = 0.00;

        //                        try
        //                        {
        //                            List<DataRow> drEmpLeave = dicLeave[dtEmployees.Rows[i]["EmpSystemID"].ToString()];
        //                            for (int li = 0; li < drEmpLeave.Count(); li++)
        //                            {
        //                                if (drEmpLeave[li]["Code"].ToString().ToUpper() == "EL")
        //                                    EL = clsStaticInfo.dbl(drEmpLeave[li]["AvailedLeave"].ToString());
        //                                if (drEmpLeave[li]["Code"].ToString().ToUpper() == "CL")
        //                                    CL = clsStaticInfo.dbl(drEmpLeave[li]["AvailedLeave"].ToString());
        //                                if (drEmpLeave[li]["Code"].ToString().ToUpper() == "SL")
        //                                    SL = clsStaticInfo.dbl(drEmpLeave[li]["AvailedLeave"].ToString());
        //                            }
        //                        }
        //                        catch (Exception)
        //                        {


        //                        }
        //                        #endregion


        //                        #region EmpInfo
        //                        SrNo += 1;
        //                        EmpIdPR = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();

        //                        sheet1.Range[xlsRow, ColSr].Text = ru.cnDgt(Convert.ToString(SrNo), localLanguage);
        //                        sheet1.Range[xlsRow, ColSr].RowHeight = 88;
        //                        pageHeightDelemeter += 150;
        //                        groupWisePageHeightDelemeter += 150;
        //                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        if (sheetBasedOn == "structured")
        //                        {
        //                            if (withAttendance == false)
        //                            {
        //                                sheet1.Range[xlsRow, ColSr, xlsRow + 1, ColSr].Merge();

        //                            }
        //                            else
        //                            {
        //                                sheet1.Range[xlsRow, ColSr, xlsRow + 5, ColSr].Merge();
        //                            }
        //                        }

        //                        else if (onlyEarning)
        //                        {
        //                            sheet1.Range[xlsRow, ColSr, xlsRow + 1, ColSr].Merge();
        //                            sheet1.Range[xlsRow, ColSr, xlsRow + 1, ColSr].BorderAround(ExcelLineStyle.Hair);


        //                        }

        //                        else
        //                        {
        //                            sheet1.Range[xlsRow, ColSr, xlsRow + 4, ColSr].Merge();
        //                        }
        //                        sheet1.Range[xlsRow, ColSr].CellStyle.Font.FontName = "Arial Narrow";
        //                        sheet1.Range[xlsRow, ColSr].CellStyle.Font.Size = 25;

        //                        //3
        //                        var _DOJ = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOJ.ToString(), "DOJ");
        //                        var _DOS = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOS.ToString(), "DOS");
        //                        var _designationLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Designation.ToString(), "Designation");
        //                        var _fatherName = ru.GetLabelname(labelList, LabelNameInLocalLanguage.FatherName.ToString(), "Father Name");
        //                        var _gradeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Grade.ToString(), "Grade");
        //                        var empName = "";
        //                        if (isLocalLanguage)
        //                        {
        //                            empName = dtEmployees.Rows[i]["EmployeeNameLocal"].ToString();
        //                        }
        //                        else
        //                        {
        //                            empName = dtEmployees.Rows[i]["EmployeeName"].ToString();

        //                        }
        //                        var empDesignation = _designationLocal + ":" + dtEmployees.Rows[i]["DesignationLocal"].ToString();
        //                        var empDOJ = _DOJ + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOJ"].ToString(), localLanguage);
        //                        var empFatherName = _fatherName + ":" + dtEmployees.Rows[i]["FatherName"].ToString();
        //                        bankAccount = "";
        //                        empDOS = string.Empty;
        //                        empGross = string.Empty;
        //                        grossSalaryAmount = 0.00;
        //                        int earnedSalaryRowHeight = 0;

        //                        grossSalaryAmount = clsStaticInfo.dbl(dtEmployees.Rows[i]["GrossAmount"].ToString());

        //                        var grossAmountLabel = ru.GetLabelname(labelList, LabelNameInLocalLanguage.TotalSalary.ToString(), "Gross");

        //                        empGross = grossAmountLabel + ":" + ru.cnDgt(grossSalaryAmount.ToString(), localLanguage).ToString();

        //                        if (dtEmployees.Rows[i]["DOS"].ToString().Length > 0)
        //                        {
        //                            empDOS = _DOS + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOS"].ToString(), localLanguage); //+ Environment.NewLine;
        //                        }
        //                        if (dtEmployees.Rows[i]["BankAccNo"].ToString().Length > 0)
        //                        {
        //                            bankAccount = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bank Acc.") + ":" + dtEmployees.Rows[i]["BankAccNo"].ToString(); //+ Environment.NewLine;
        //                        }
        //                        if (withAttendance == false)
        //                        {
        //                            // With Structure and Earned only
        //                            secondRowHeight = 140;
        //                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString() + "::" + empName;
        //                            sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                            sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, ColName].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, ColName].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow, ColName].CellStyle.Font.Bold = true;
        //                            sheet1.Range[xlsRow, ColName].CellStyle.Font.Size = 50;
        //                            sheet1.Range[xlsRow, ColName].RowHeight = 90;
        //                            pageHeightDelemeter = 140;
        //                            groupWisePageHeightDelemeter += 140;
        //                            sheet1.Range[xlsRow, ColName].ColumnWidth = 150;

        //                            //sheet1.Range[xlsRow + 1, ColName].Text = empGross + "%OA" + _designationLocal+ "%OA"+ _gradeLocal+"%OA"+ bankAccount+"%OA"+ empDOJ+"%OA"+empDOS;

        //                            IRichTextString rtf1 = sheet1.Range[xlsRow + 1, ColName].RichText;
        //                            if (ExcludeFatherName == false)
        //                            {
        //                                if (!string.IsNullOrEmpty(dtEmployees.Rows[i]["FatherName"].ToString()))
        //                                {
        //                                    FormatText(ref sheet1, ref rtf1, empFatherName, 30);
        //                                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                                    secondRowHeight += 25;
        //                                }
        //                            }
        //                            FormatText(ref sheet1, ref rtf1, _designationLocal + ":" + dtEmployees.Rows[i]["DesignationLocal"].ToString() + " ", 30);
        //                            FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                            FormatText(ref sheet1, ref rtf1, _gradeLocal + ":", 30); FormatText(ref sheet1, ref rtf1, dtEmployees.Rows[i]["GradeCode"].ToString() + " ", 30);
        //                            if (dtEmployees.Rows[i]["BankAccNo"].ToString().Length > 0)
        //                            {
        //                                bankAccount = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bnk Acc") + ":" + dtEmployees.Rows[i]["BankAccNo"].ToString(); //+ Environment.NewLine;
        //                                FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                                FormatText(ref sheet1, ref rtf1, bankAccount + " ", 30);
        //                                secondRowHeight += 25;
        //                            }
        //                            FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                            FormatText(ref sheet1, ref rtf1, empDOJ + " ", 30);
        //                            if (dtEmployees.Rows[i]["DOS"].ToString().Length > 0)
        //                            {
        //                                empDOS = _DOS + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOS"].ToString(), localLanguage); //+ Environment.NewLine;
        //                                FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                                FormatText(ref sheet1, ref rtf1, empDOS + " ", 30);
        //                                secondRowHeight += 25;
        //                            }
        //                            sheet1.Range[xlsRow + 1, ColName].RowHeight = secondRowHeight;//secondRowHeight;
        //                            earnedSalaryRowHeight = secondRowHeight;

        //                            pageHeightDelemeter = 135;
        //                            groupWisePageHeightDelemeter += 135;
        //                            sheet1.Range[xlsRow + 1, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                            sheet1.Range[xlsRow + 1, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        }
        //                        else
        //                        {
        //                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();

        //                            sheet1.Range[xlsRow + 1, ColName].Text = empName;
        //                            sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                            sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].CellStyle.Font.Bold = true;
        //                            sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].CellStyle.Font.Size = 30;
        //                            sheet1.Range[xlsRow, ColName].ColumnWidth = 65;
        //                            sheet1.Range[xlsRow + 1, ColName].ColumnWidth = 65;

        //                            IRichTextString rtf1 = sheet1.Range[xlsRow + 2, ColName].RichText;

        //                            FormatText(ref sheet1, ref rtf1, empGross + " ", 25);
        //                            FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                            if (ExcludeFatherName == false)
        //                            {
        //                                if (!string.IsNullOrEmpty(dtEmployees.Rows[i]["FatherName"].ToString()))
        //                                {
        //                                    FormatText(ref sheet1, ref rtf1, empFatherName, 30);
        //                                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                                }
        //                            }
        //                            FormatText(ref sheet1, ref rtf1, _designationLocal + ":" + dtEmployees.Rows[i]["DesignationLocal"].ToString() + " ", 25);
        //                            FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                            FormatText(ref sheet1, ref rtf1, _gradeLocal + ":", 25); FormatText(ref sheet1, ref rtf1, dtEmployees.Rows[i]["GradeCode"].ToString() + " ", 25);
        //                            if (dtEmployees.Rows[i]["BankAccNo"].ToString().Length > 0)
        //                            {
        //                                bankAccount = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bank Acc") + ":" + dtEmployees.Rows[i]["BankAccNo"].ToString(); //+ Environment.NewLine;
        //                                FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                                FormatText(ref sheet1, ref rtf1, bankAccount + " ", 25);
        //                            }
        //                            if (dtEmployees.Rows[i]["DOS"].ToString().Length > 0)
        //                            {
        //                                empDOS = "|| " + _DOS + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOS"].ToString(), localLanguage); //+ Environment.NewLine;

        //                            }
        //                            FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                            FormatText(ref sheet1, ref rtf1, empDOJ + empDOS + " ", 25);

        //                            sheet1.Range[xlsRow + 2, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                            sheet1.Range[xlsRow + 2, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        }
        //                        if (onlyEarning)
        //                        {


        //                            sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 1, ColLeaveInfo].Merge();
        //                            sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 1, ColLeaveInfo].BorderAround(ExcelLineStyle.Hair);

        //                        }
        //                        else if (sheetBasedOn == "structured")
        //                        {
        //                            if (withAttendance == false)
        //                            {
        //                                sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 1, ColLeaveInfo].Merge();
        //                                sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 1, ColLeaveInfo].BorderAround(ExcelLineStyle.Hair);
        //                            }
        //                            else
        //                            {
        //                                sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 5, ColLeaveInfo].Merge();
        //                                sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 5, ColLeaveInfo].BorderAround(ExcelLineStyle.Hair);
        //                                sheet1.Range[xlsRow + 2, ColName, xlsRow + 5, ColName].Merge();
        //                                sheet1.Range[xlsRow + 2, ColName, xlsRow + 5, ColName].BorderAround(ExcelLineStyle.Hair);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            sheet1.Range[xlsRow + 2, ColName, xlsRow + 4, ColName].Merge();
        //                            sheet1.Range[xlsRow + 2, ColName, xlsRow + 4, ColName].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow + 2, ColName, xlsRow + 4, ColName].IndentLevel = 1;
        //                        }
        //                        //Leave Info
        //                        //if (dtEmpAttdnInfo.Rows.Count > 0)
        //                        //{
        //                        string lateBangla = ru.cnDgt(Convert.ToDouble(dtEmployees.Rows[i]["TotalLate"]).ToString(), localLanguage);
        //                        string lwpBangla = ru.cnDgt(Convert.ToDouble(dtEmployees.Rows[i]["TotalLWP"]).ToString(), localLanguage);
        //                        double presentLate = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalPresent"].ToString()) + clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLate"].ToString());
        //                        string presentBangla = ru.cnDgt(Convert.ToString(presentLate), localLanguage);
        //                        string absentBangla = ru.cnDgt((Convert.ToDouble(dtEmployees.Rows[i]["TotalAbsent"]) - Convert.ToDouble(dtEmployees.Rows[i]["TotalLWP"])).ToString(), localLanguage);
        //                        double weekOffHoliday = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOffHoliDay"].ToString()) + clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());

        //                        string weekOff = ru.cnDgt(weekOffHoliday.ToString(), localLanguage);
        //                        string holiDay = ru.cnDgt(Convert.ToDouble(dtEmployees.Rows[i]["TotalHoliDay"]).ToString(), localLanguage);
        //                        string leave = ru.cnDgt(Convert.ToDouble(dtEmployees.Rows[i]["TotalLv"]).ToString(), localLanguage);
        //                        string totalOTHr = "";//ru.cnDgt((Math.Round((Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalNormalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalExtraOTHr"])) / 60, 2).ToString()), localLanguage);
        //                        var _availedLeave = ru.GetLabelname(labelList, LabelNameInLocalLanguage.AvailedLeave.ToString(), "Availed Leave");
        //                        var _late = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Late.ToString(), "Late");
        //                        var _LWP = ru.GetLabelname(labelList, LabelNameInLocalLanguage.LWP.ToString(), "LWP");
        //                        var _lunchOutHour = ru.GetLabelname(labelList, LabelNameInLocalLanguage.LunchOutHour.ToString(), "Lunch Out Hr");//lunchOutHour.LocalLabel == null || lunchOutHour.LocalLabel == "" ? lunchOutHour.DefaultLabel : lunchOutHour.LocalLabel;
        //                        var _otHour = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OTHours.ToString(), "OT Hrs");
        //                        var _otRateLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OTRate.ToString(), "OT Rate");

        //                        double otRate = 0.00;
        //                        string otRateBangla = "";//IsOTEntitled
        //                        if (Convert.ToBoolean(dtEmployees.Rows[i]["IsOTEntitled"]) == true)
        //                        {
        //                            totalOTHr = ru.cnDgt((Math.Round((Convert.ToDouble(dtEmployees.Rows[i]["TotalOTHr"]) + Convert.ToDouble(dtEmployees.Rows[i]["TotalNormalOTHr"]) + Convert.ToDouble(dtEmployees.Rows[i]["TotalExtraOTHr"])) / 60, 2).ToString()), localLanguage);
        //                            otRate = Convert.ToDouble(dtEmployees.Rows[i]["OTRate"]);
        //                            otRateBangla = ru.cnDgt(Math.Round(otRate, 2).ToString(), localLanguage);
        //                            earnedSalaryRowHeight += 26;
        //                        }
        //                        IRichTextString rtfLeave = sheet1.Range[xlsRow, ColLeaveInfo].RichText;
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locEL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(EL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locCL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(CL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locSL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(SL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, _availedLeave + ":" + ru.cnDgt(Math.Round(Convert.ToDouble(dtEmployees.Rows[i]["TotalLv"].ToString()), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, _LWP + ":" + lwpBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, _lunchOutHour + ":" + "", 27);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);

        //                        sheet1.Range[xlsRow, ColLeaveInfo].IndentLevel = 1;

        //                        sheet1.Range[xlsRow, ColLeaveInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                        sheet1.Range[xlsRow, ColLeaveInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        if (sheetBasedOn == "structured")
        //                        {
        //                            if (withAttendance == false)
        //                            {
        //                                sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 1, ColLeaveInfo].Merge();
        //                                sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 1, ColLeaveInfo].BorderAround(ExcelLineStyle.Hair);
        //                            }
        //                            else
        //                            {
        //                                sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 5, ColLeaveInfo].Merge();
        //                                sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 5, ColLeaveInfo].BorderAround(ExcelLineStyle.Hair);
        //                            }
        //                        }
        //                        else if (onlyEarning)
        //                        {

        //                            sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 1, ColLeaveInfo].Merge();
        //                            sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 1, ColLeaveInfo].BorderAround(ExcelLineStyle.Hair);
        //                        }
        //                        else
        //                        {
        //                            sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 4, ColLeaveInfo].Merge();
        //                            sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 4, ColLeaveInfo].BorderAround(ExcelLineStyle.Hair);
        //                        }
        //                        string payDays = "";
        //                        if (!String.IsNullOrEmpty(dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
        //                        {
        //                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
        //                            {
        //                                payDays = ru.cnDgt((Convert.ToDouble(dtEmployees.Rows[i]["TotalProcDate"]) - Convert.ToDouble(dtEmployees.Rows[i]["TotalWeekOff"]) - Convert.ToDouble(dtEmployees.Rows[i]["TotalHoliDay"]) - Convert.ToDouble(dtEmployees.Rows[i]["TotalAbsent"])).ToString(), localLanguage);
        //                            }
        //                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
        //                            {
        //                                payDays = ru.cnDgt((Convert.ToDouble(dtEmployees.Rows[i]["TotalProcDate"]) - Convert.ToDouble(dtEmployees.Rows[i]["TotalWeekOff"]) - Convert.ToDouble(dtEmployees.Rows[i]["TotalAbsent"])).ToString(), localLanguage);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            payDays = ru.cnDgt((Convert.ToDouble(dtEmployees.Rows[i]["TotalProcDate"]) - Convert.ToDouble(dtEmployees.Rows[i]["TotalAbsent"])).ToString(), localLanguage);
        //                        }

        //                        IRichTextString rtf = sheet1.Range[xlsRow, ColWorkDaysInfo].RichText;
        //                        var _payDaysLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.PayDays.ToString(), "Pay Days");
        //                        var _presentLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Present.ToString(), "Present");
        //                        var _absentLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Absent.ToString(), LabelNameInLocalLanguage.Absent.ToString());//absentLocal.LocalLabel == null || absentLocal.LocalLabel == "" ? absentLocal.DefaultLabel : absentLocal.LocalLabel;

        //                        var _weeklyHolidays = ru.GetLabelname(labelList, LabelNameInLocalLanguage.WeeklyLeaveDays.ToString(), "Weekend");
        //                        var _holiDays = ru.GetLabelname(labelList, LabelNameInLocalLanguage.HoliDay.ToString(), "Holidays");


        //                        FormatText(ref sheet1, ref rtf, _payDaysLocal + ":" + payDays + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _presentLocal + ":" + presentBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _absentLocal + ":" + absentBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _weeklyHolidays + ":" + weekOff + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _holiDays + ":" + holiDay + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _late + ":" + lateBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        if (Convert.ToBoolean(dtEmployees.Rows[i]["IsOTEntitled"]) == true)
        //                        {
        //                            FormatText(ref sheet1, ref rtf, _otHour + ":" + totalOTHr + " ", 27);
        //                            FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                            FormatText(ref sheet1, ref rtf, _otRateLocal + ":" + otRateBangla + " ", 27);
        //                        }
        //                        sheet1.Range[xlsRow, ColWorkDaysInfo].IndentLevel = 1;
        //                        if (sheetBasedOn == "structured")
        //                        {
        //                            if (withAttendance == false)
        //                            {
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 1, ColWorkDaysInfo].Merge();
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 1, ColWorkDaysInfo].BorderAround(ExcelLineStyle.Hair);
        //                            }
        //                            else
        //                            {
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 5, ColWorkDaysInfo].Merge();
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 5, ColWorkDaysInfo].BorderAround(ExcelLineStyle.Hair);
        //                            }

        //                        }
        //                        else if (onlyEarning)
        //                        {
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 1, ColWorkDaysInfo].Merge();
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 1, ColWorkDaysInfo].BorderAround(ExcelLineStyle.Hair);
        //                        }
        //                        else
        //                        {
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 4, ColWorkDaysInfo].Merge();
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 4, ColWorkDaysInfo].BorderAround(ExcelLineStyle.Hair);
        //                        }
        //                        //}

        //                        var earnedSalary = "";// labelList.Where(r => r.DefaultLabel == LabelNameInLocalLanguage.EarnedSalary.ToString()).FirstOrDefault();
        //                        if (labelList.ContainsKey(LabelNameInLocalLanguage.EarnedSalary.ToString()))
        //                        {
        //                            earnedSalary = labelList[LabelNameInLocalLanguage.EarnedSalary.ToString()];
        //                        }
        //                        if (sheetBasedOn == "structured")
        //                        {
        //                            var _structuredSalary = ru.GetLabelname(labelList, LabelNameInLocalLanguage.StructuredSalary.ToString(), "Str Sal");
        //                            sheet1.Range[xlsRow, colParticulars].Text = _structuredSalary + "->";
        //                            sheet1.Range[xlsRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[xlsRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, colParticulars].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Size = 24;
        //                            sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Bold = true;

        //                            if (withAttendance == false)
        //                            {
        //                                //sheet1.Range[xlsRow, colParticulars].RowHeight = 150;//tarek

        //                            }
        //                            else
        //                            {
        //                                sheet1.Range[xlsRow, colParticulars].RowHeight = 68;//tarek
        //                                earnedSalaryRowHeight = 70;


        //                            }
        //                            xlsRow++;
        //                        }
        //                        else if (onlyEarning)
        //                        {

        //                            //var _structuredSalary = ru.GetLabelname(labelList, LabelNameInLocalLanguage.StructuredSalary.ToString(), "Str Sal");
        //                            //sheet1.Range[xlsRow, colParticulars].Text = _structuredSalary + "->";
        //                            //sheet1.Range[xlsRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            //sheet1.Range[xlsRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            //sheet1.Range[xlsRow, colParticulars].BorderAround(ExcelLineStyle.Hair);
        //                            //sheet1.Range[xlsRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                            //sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Size = 24;
        //                            //sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Bold = true;

        //                            if (withAttendance == false)
        //                            {
        //                                //sheet1.Range[xlsRow, colParticulars].RowHeight = 150;//tarek

        //                            }
        //                            else
        //                            {
        //                                sheet1.Range[xlsRow, colParticulars].RowHeight = 68;//tarek
        //                                earnedSalaryRowHeight = 70;


        //                            }
        //                            xlsRow++;

        //                        }
        //                        else
        //                        {
        //                            earnedSalaryRowHeight = 52;
        //                        }
        //                        if (onlyEarning)
        //                        {

        //                            var _earnedSalary = ru.GetLabelname(labelList, LabelNameInLocalLanguage.EarnedSalary.ToString(), "Earned Salary");
        //                            sheet1.Range[xlsRow, colParticulars].Text = _earnedSalary + "->";
        //                            sheet1.Range[xlsRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[xlsRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, colParticulars].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Size = 24;
        //                            sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Bold = true;
        //                            sheet1.Range[xlsRow - 1, colParticulars, xlsRow, colParticulars].Merge();


        //                            sheet1.Range[xlsRow, colParticulars].RowHeight = earnedSalaryRowHeight;//tarek

        //                        }
        //                        else
        //                        {
        //                            var _earnedSalary = ru.GetLabelname(labelList, LabelNameInLocalLanguage.EarnedSalary.ToString(), "Earned Salary");
        //                            sheet1.Range[xlsRow, colParticulars].Text = _earnedSalary + "->";
        //                            sheet1.Range[xlsRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[xlsRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, colParticulars].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Size = 24;
        //                            sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Bold = true;

        //                            sheet1.Range[xlsRow, colParticulars].RowHeight = earnedSalaryRowHeight;//tarek
        //                        }

        //                        if (withAttendance == false)
        //                        {
        //                            //sheet1.Range[xlsRow, colParticulars].RowHeight = 150;//tarek

        //                        }
        //                        else
        //                        {
        //                            if (sheetBasedOn == "structured")
        //                            {
        //                                //  sheet1.Range[xlsRow, colParticulars].RowHeight = 52;//tarek
        //                            }
        //                            else
        //                            {
        //                                sheet1.Range[xlsRow, colParticulars].RowHeight = 100;//tarek
        //                            }

        //                        }
        //                        var particular3rdRow = xlsRow;
        //                        if (withAttendance == true)
        //                        {
        //                            var _daysLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Days.ToString(), "Days");

        //                            particular3rdRow = xlsRow + 1;

        //                            var _attendance = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Attendance.ToString(), "Attendance");

        //                            sheet1.Range[particular3rdRow, colParticulars].Text = _attendance + "->";
        //                            sheet1.Range[particular3rdRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[particular3rdRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[particular3rdRow, colParticulars].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[particular3rdRow, colParticulars].RowHeight = 50;
        //                            pageHeightDelemeter += 43;
        //                            groupWisePageHeightDelemeter += 43;
        //                            sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.Size = 17;
        //                            sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.Bold = true;

        //                            var _inTimeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.INTime.ToString(), "INTime");
        //                            var _outTimeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OUTTime.ToString(), "OUTTime");
        //                            var _OTLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OT.ToString(), "OT");

        //                            sheet1.Range[particular3rdRow + 1, colParticulars].Text = _inTimeLocal + "->";
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].RowHeight = 50;
        //                            pageHeightDelemeter += 43;
        //                            groupWisePageHeightDelemeter += 43;
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.Size = 17;
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                            sheet1.Range[particular3rdRow + 2, colParticulars].Text = _outTimeLocal + "->";
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].RowHeight = 50;
        //                            pageHeightDelemeter = 43;
        //                            groupWisePageHeightDelemeter += 43;
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.Size = 17;
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.Bold = true;

        //                            sheet1.Range[particular3rdRow + 3, colParticulars].Text = _OTLocal + "->";
        //                            sheet1.Range[particular3rdRow + 3, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[particular3rdRow + 3, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[particular3rdRow + 3, colParticulars].RowHeight = 50;

        //                            sheet1.Range[particular3rdRow + 3, colParticulars].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.Size = 17;
        //                            sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.Bold = true;
        //                        }
        //                        #endregion
        //                        #region SalaryStructure

        //                        if (dicEmpSalry.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
        //                        {
        //                            List<DataRow> drSalaryHeadCollection = dicEmpSalry[dtEmployees.Rows[i]["EmpSystemID"].ToString()];

        //                            if (sheetBasedOn == "structured")
        //                            {
        //                                for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
        //                                {
        //                                    if (drSalaryHeadCollection[CI]["SalaryHead"].ToString().Contains("Tax"))
        //                                    {

        //                                    }
        //                                    try
        //                                    {
        //                                        SalaryHeadSequence xx = new SalaryHeadSequence();

        //                                        if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
        //                                        {
        //                                            continue;
        //                                        }
        //                                        if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "GROSS")
        //                                        {

        //                                        }
        //                                        try
        //                                        {
        //                                            xx = strListNew[drSalaryHeadCollection[CI]["SalaryHeadID"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();

        //                                        }
        //                                        catch (Exception)
        //                                        {
        //                                            xx = null;
        //                                        }
        //                                        if (xx != null)
        //                                        {
        //                                            var slrStructureAmount = 0.00;
        //                                            if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
        //                                            {
        //                                                slrStructureAmount = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["EntryAmount"].ToString());
        //                                                getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref subTotalDictSalaryStruct);//SubTotal
        //                                                getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref totalDictSalaryStruct);
        //                                                sheet1.Range[xlsRow - 1, xx.XLColIndex].Number = slrStructureAmount; //; clsStaticInfo.dbl(drSalaryHeadCollection[CI]["EntryAmount"].ToString());// * (-1);
        //                                            }

        //                                            else
        //                                            {
        //                                                slrStructureAmount = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["EntryAmount"].ToString());
        //                                                getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref subTotalDictSalaryStruct);//SubTotal
        //                                                getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref totalDictSalaryStruct);
        //                                            }

        //                                            sheet1.Range[xlsRow - 1, xx.XLColIndex].Number = slrStructureAmount;
        //                                            sheet1.Range[xlsRow - 1, xx.XLColIndex - 1, xlsRow - 1, xx.XLColIndex].NumberFormat = ru.GetDecimalFormatlocal(xx, localLanguage);
        //                                        }
        //                                    }
        //                                    catch (Exception ex)
        //                                    {

        //                                        throw ex;
        //                                    }

        //                                }
        //                                var slrStartCol2 = slrStartCol;
        //                                for (int isl = 0; isl < strListNew.Count; isl++)
        //                                {
        //                                    sheet1.Range[xlsRow - 1, slrStartCol2 + isl, xlsRow - 1, slrStartCol2 + isl + 1].Merge();
        //                                    sheet1.Range[xlsRow - 1, slrStartCol2 + isl, xlsRow - 1, slrStartCol2 + isl + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                    sheet1.Range[xlsRow - 1, slrStartCol2 + isl, xlsRow - 1, slrStartCol2 + isl + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                    sheet1.Range[xlsRow - 1, slrStartCol2 + isl, xlsRow - 1, slrStartCol2 + isl + 1].CellStyle.Font.FontName = printFont;
        //                                    slrStartCol2++;
        //                                }
        //                            }

        //                            #endregion
        //                            #region ONly Earning

        //                            if (onlyEarning)
        //                            {
        //                                for (int ci = 0; ci < drSalaryHeadCollection.Count; ci++)
        //                                {

        //                                    try
        //                                    {
        //                                        salarySheetValue = 0.00;

        //                                        if (drSalaryHeadCollection[ci]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
        //                                        {

        //                                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].Number = Convert.ToDouble(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString());
        //                                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = ru.GetDecimalFormatlocalNetPay(Convert.ToBoolean(drSalaryHeadCollection[ci]["IntegerInDisb"].ToString()), Convert.ToInt32(drSalaryHeadCollection[ci]["DecimalNo"].ToString()), localLanguage);
        //                                            //sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].IndentLevel = 2;
        //                                            sheet1.Range[xlsRow - 1, colPaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
        //                                            sheet1.Range[xlsRow - 1, colPaymentMode].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                            sheet1.Range[xlsRow - 1, colPaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                            sheet1.Range[xlsRow - 1, colPaymentMode, xlsRow, colPaymentMode].Merge();
        //                                            sheet1.Range[xlsRow - 1, colPaymentMode].BorderAround(ExcelLineStyle.Hair);
        //                                            sheet1.Range[xlsRow - 1, colPaymentMode].CellStyle.Font.FontName = printFont;
        //                                            sheet1.Range[xlsRow - 1, colPaymentMode].CellStyle.Font.Size = 34;
        //                                            sheet1.Range[xlsRow - 1, colPaymentMode].CellStyle.Font.Bold = true;
        //                                            sheet1.Range[xlsRow - 1, colPaymentMode].CellStyle.Rotation = 180;

        //                                            subTotalNetPayDisbusmentAmount += Convert.ToDouble(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString());
        //                                            totalNetPayDisbusmentAmount += Convert.ToDouble(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString());

        //                                            if (dtbankCash.Rows.Count > 0)
        //                                            {
        //                                                dtbankCash.DefaultView.RowFilter = "EmpSystemId = '" + dtEmployees.Rows[i]["EmpsystemId"].ToString() + @"'";

        //                                                if (dtbankCash.DefaultView.Count > 0)
        //                                                {
        //                                                    sheet1.Range[xlsRow, colBankPaymentPercentage, xlsRow, colBankPaymentPercentage].Number = Convert.ToDouble(dtbankCash.DefaultView[0]["BankAmount"].ToString());
        //                                                    sheet1.Range[xlsRow, colBankPaymentPercentage, xlsRow, colBankPaymentPercentage].NumberFormat = ru.GetDecimalFormatlocalNetPay(Convert.ToBoolean(drSalaryHeadCollection[ci]["IntegerInDisb"].ToString()), Convert.ToInt32(drSalaryHeadCollection[ci]["DecimalNo"].ToString()), localLanguage);
        //                                                    sheet1.Range[xlsRow, colBankPaymentPercentage].CellStyle.Font.Size = 34;
        //                                                    sheet1.Range[xlsRow, colBankPaymentPercentage].CellStyle.Font.FontName = printFont;


        //                                                    sheet1.Range[xlsRow, colCashPaymentPercentage, xlsRow, colCashPaymentPercentage].Number = Convert.ToDouble(dtbankCash.DefaultView[0]["CashAmount"].ToString());
        //                                                    sheet1.Range[xlsRow, colCashPaymentPercentage, xlsRow, colCashPaymentPercentage].NumberFormat = ru.GetDecimalFormatlocalNetPay(Convert.ToBoolean(drSalaryHeadCollection[ci]["IntegerInDisb"].ToString()), Convert.ToInt32(drSalaryHeadCollection[ci]["DecimalNo"].ToString()), localLanguage);
        //                                                    sheet1.Range[xlsRow, colCashPaymentPercentage].CellStyle.Font.Size = 34;
        //                                                    sheet1.Range[xlsRow, colCashPaymentPercentage].CellStyle.Font.FontName = printFont;
        //                                                    totalBankPayDisbusmentAmount += clsStaticInfo.dbl(dtbankCash.DefaultView[0]["BankAmount"].ToString());
        //                                                    totalCashPayDisbusmentAmount += clsStaticInfo.dbl(dtbankCash.DefaultView[0]["CashAmount"].ToString());
        //                                                    subTotalBankPayDisbusmentAmount += clsStaticInfo.dbl(dtbankCash.DefaultView[0]["BankAmount"].ToString());
        //                                                    subTotalCashPayDisbusmentAmount += clsStaticInfo.dbl(dtbankCash.DefaultView[0]["CashAmount"].ToString());


        //                                                }

        //                                            }

        //                                            //}
        //                                            continue;
        //                                        }

        //                                        SalaryHeadSequence xx = new SalaryHeadSequence();//strListNew[drSalaryHeadCollection[ci]["SalaryHeadID"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
        //                                        try
        //                                        {
        //                                            xx = strListNew[drSalaryHeadCollection[ci]["SalaryHeadID"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();

        //                                        }
        //                                        catch (Exception)
        //                                        {

        //                                            xx = null;
        //                                        }
        //                                        if (xx != null)
        //                                        {

        //                                            if (drSalaryHeadCollection[ci]["HeadType"].ToString() == "D")
        //                                            {
        //                                                salarySheetValue = clsStaticInfo.dbl(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString()) * (-1);
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref subTotalDictSalaryProcess);//SubTotal
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                                            }
        //                                            else
        //                                            {
        //                                                salarySheetValue = clsStaticInfo.dbl(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString());
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref subTotalDictSalaryProcess);//SubTotal
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                                            }
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = ru.GetDecimalFormatlocal(xx, localLanguage);
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].Number = salarySheetValue;
        //                                            //sheet1.Range[xlsRow, xx.XLColIndex - 1, xlsRow, xx.XLColIndex].Merge();
        //                                            sheet1.Range[xlsRow - 1, xx.XLColIndex - 1, xlsRow, xx.XLColIndex].Merge();

        //                                            sheet1.Range[xlsRow, xx.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                                            sheet1.Range[xlsRow - 1, xx.XLColIndex - 1, xlsRow, xx.XLColIndex].CellStyle.Font.Size = 40;
        //                                            //sheet1.Range[xlsRow - 1, xx.XLColIndex - 1, xlsRow, xx.XLColIndex].IndentLevel = 2;


        //                                        }

        //                                        //sheet1.Range[xlsRow - 1, xx.XLColIndex - 1, xlsRow, xx.XLColIndex].Merge();
        //                                    }
        //                                    catch (Exception ex)
        //                                    {

        //                                        throw ex;
        //                                    }


        //                                }//for dtSalaryHead
        //                            }




        //                            #endregion
        //                            #region SalarySheet

        //                            if (sheetBasedOn == "structured" || withAttendance)
        //                            {
        //                                for (int ci = 0; ci < drSalaryHeadCollection.Count; ci++)
        //                                {

        //                                    try
        //                                    {
        //                                        salarySheetValue = 0.00;

        //                                        if (drSalaryHeadCollection[ci]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
        //                                        {

        //                                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].Number = Convert.ToDouble(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString());
        //                                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = ru.GetDecimalFormatlocalNetPay(Convert.ToBoolean(drSalaryHeadCollection[ci]["IntegerInDisb"].ToString()), Convert.ToInt32(drSalaryHeadCollection[ci]["DecimalNo"].ToString()), localLanguage);

        //                                            sheet1.Range[xlsRow, colPaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
        //                                            sheet1.Range[xlsRow, colPaymentMode].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                            sheet1.Range[xlsRow, colPaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                            sheet1.Range[xlsRow, colPaymentMode].BorderAround(ExcelLineStyle.Hair);
        //                                            sheet1.Range[xlsRow, colPaymentMode].CellStyle.Font.FontName = printFont;
        //                                            sheet1.Range[xlsRow, colPaymentMode].CellStyle.Font.Size = 34;
        //                                            sheet1.Range[xlsRow, colPaymentMode].CellStyle.Font.Bold = true;
        //                                            sheet1.Range[xlsRow, colPaymentMode].CellStyle.Rotation = 180;

        //                                            if (dtbankCash.Rows.Count > 0)
        //                                            {
        //                                                dtbankCash.DefaultView.RowFilter = "EmpSystemId = '" + dtEmployees.Rows[i]["EmpsystemId"].ToString() + @"'";

        //                                                if (dtbankCash.DefaultView.Count > 0)
        //                                                {
        //                                                    sheet1.Range[xlsRow, colBankPaymentPercentage, xlsRow, colBankPaymentPercentage].Number = Convert.ToDouble(dtbankCash.DefaultView[0]["BankAmount"].ToString());
        //                                                    sheet1.Range[xlsRow, colBankPaymentPercentage, xlsRow, colBankPaymentPercentage].NumberFormat = ru.GetDecimalFormatlocalNetPay(Convert.ToBoolean(drSalaryHeadCollection[ci]["IntegerInDisb"].ToString()), Convert.ToInt32(drSalaryHeadCollection[ci]["DecimalNo"].ToString()), localLanguage);
        //                                                    sheet1.Range[xlsRow, colBankPaymentPercentage].CellStyle.Font.Size = 34;
        //                                                    sheet1.Range[xlsRow, colBankPaymentPercentage].CellStyle.Font.FontName = printFont;


        //                                                    sheet1.Range[xlsRow, colCashPaymentPercentage, xlsRow, colCashPaymentPercentage].Number = Convert.ToDouble(dtbankCash.DefaultView[0]["CashAmount"].ToString());
        //                                                    sheet1.Range[xlsRow, colCashPaymentPercentage, xlsRow, colCashPaymentPercentage].NumberFormat = ru.GetDecimalFormatlocalNetPay(Convert.ToBoolean(drSalaryHeadCollection[ci]["IntegerInDisb"].ToString()), Convert.ToInt32(drSalaryHeadCollection[ci]["DecimalNo"].ToString()), localLanguage);
        //                                                    sheet1.Range[xlsRow, colCashPaymentPercentage].CellStyle.Font.Size = 34;
        //                                                    sheet1.Range[xlsRow, colCashPaymentPercentage].CellStyle.Font.FontName = printFont;

        //                                                    totalBankPayDisbusmentAmount += clsStaticInfo.dbl(dtbankCash.DefaultView[0]["BankAmount"].ToString());
        //                                                    totalCashPayDisbusmentAmount += clsStaticInfo.dbl(dtbankCash.DefaultView[0]["CashAmount"].ToString());
        //                                                    subTotalBankPayDisbusmentAmount += clsStaticInfo.dbl(dtbankCash.DefaultView[0]["BankAmount"].ToString());
        //                                                    subTotalCashPayDisbusmentAmount += clsStaticInfo.dbl(dtbankCash.DefaultView[0]["CashAmount"].ToString());
        //                                                }

        //                                            }



        //                                            //sheet1.Range[xlsRow, colPaymentMode].IndentLevel = 2;


        //                                            subTotalNetPayDisbusmentAmount += Convert.ToDouble(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString());
        //                                            totalNetPayDisbusmentAmount += Convert.ToDouble(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString());
        //                                            //}
        //                                            continue;
        //                                        }

        //                                        SalaryHeadSequence xx = new SalaryHeadSequence();//strListNew[drSalaryHeadCollection[ci]["SalaryHeadID"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
        //                                        try
        //                                        {
        //                                            xx = strListNew[drSalaryHeadCollection[ci]["SalaryHeadID"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();

        //                                        }
        //                                        catch (Exception)
        //                                        {

        //                                            xx = null;
        //                                        }
        //                                        if (xx != null)
        //                                        {

        //                                            if (drSalaryHeadCollection[ci]["HeadType"].ToString() == "D")
        //                                            {
        //                                                salarySheetValue = clsStaticInfo.dbl(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString()) * (-1);
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref subTotalDictSalaryProcess);//SubTotal
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                                            }
        //                                            else
        //                                            {
        //                                                salarySheetValue = clsStaticInfo.dbl(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString());
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref subTotalDictSalaryProcess);//SubTotal
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                                            }
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = ru.GetDecimalFormatlocal(xx, localLanguage);
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].Number = salarySheetValue;
        //                                            sheet1.Range[xlsRow, xx.XLColIndex].CellStyle.ShrinkToFit = true;


        //                                            sheet1.Range[xlsRow, xx.XLColIndex - 1, xlsRow, xx.XLColIndex].Merge();
        //                                            //sheet1.Range[xlsRow, xx.XLColIndex].IndentLevel = 1;

        //                                            sheet1.Range[xlsRow, xx.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                                            if (sheetBasedOn.ToUpper() == "structured".ToUpper())
        //                                            {
        //                                                sheet1.Range[xlsRow - 1, xx.XLColIndex - 1, xlsRow, xx.XLColIndex].CellStyle.Font.Size = 40;
        //                                            }
        //                                            else
        //                                            {
        //                                                sheet1.Range[xlsRow, xx.XLColIndex - 1, xlsRow, xx.XLColIndex - 1].CellStyle.Font.Size = 40;

        //                                            }
        //                                        }


        //                                    }
        //                                    catch (Exception ex)
        //                                    {

        //                                        throw ex;
        //                                    }


        //                                }//for dtSalaryHead 
        //                            }
        //                            var slrStartCol3 = slrStartCol;
        //                            for (int isl = 0; isl < strListNew.Count; isl++)
        //                            {
        //                                sheet1.Range[xlsRow, slrStartCol3 + isl, xlsRow, slrStartCol3 + isl + 1].Merge();
        //                                sheet1.Range[xlsRow, slrStartCol3 + isl, xlsRow, slrStartCol3 + isl + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                sheet1.Range[xlsRow, slrStartCol3 + isl, xlsRow, slrStartCol3 + isl + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow, slrStartCol3 + isl, xlsRow, slrStartCol3 + isl + 1].CellStyle.Font.FontName = printFont;
        //                                slrStartCol3++;
        //                            }
        //                        }

        //                        #endregion

        //                        if (sheetBasedOn == "structured")
        //                        {
        //                            if (withAttendance == false)
        //                            {

        //                                //empModulasFactor = empNoPerPage;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].BorderAround(ExcelLineStyle.Hair);
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.FontName = printFont;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Size = 40;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].Merge();
        //                                if (dtbankCash.Rows.Count > 0)
        //                                {

        //                                    sheet1.Range[xlsRow - 1, colBankPaymentPercentage, xlsRow, colBankPaymentPercentage].Merge();
        //                                    sheet1.Range[xlsRow - 1, colCashPaymentPercentage, xlsRow, colCashPaymentPercentage].Merge();
        //                                    sheet1.Range[xlsRow - 1, colBankPaymentPercentage, xlsRow, colCashPaymentPercentage].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                    sheet1.Range[xlsRow - 1, colBankPaymentPercentage, xlsRow, colCashPaymentPercentage].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                    sheet1.Range[xlsRow - 1, colBankPaymentPercentage, xlsRow, colCashPaymentPercentage].CellStyle.Font.Bold = false;

        //                                }

        //                                sheet1.Range[xlsRow - 1, colSignature, xlsRow, colSignature].Merge();
        //                                sheet1.Range[xlsRow - 1, colSignature, xlsRow, colSignature].BorderAround(ExcelLineStyle.Hair);
        //                                sheet1.Range[xlsRow - 1, 1, xlsRow, colSignature].BorderAround(ExcelLineStyle.Hair);
        //                                sheet1.Range[xlsRow - 1, 1, xlsRow, colSignature].BorderInside(ExcelLineStyle.Hair);
        //                            }
        //                            else
        //                            {

        //                                // empModulasFactor = empNoPerPage;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].BorderAround(ExcelLineStyle.Hair);
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.FontName = printFont;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Size = 40;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].Merge();
        //                                if (dtbankCash.Rows.Count > 0)
        //                                {

        //                                    sheet1.Range[xlsRow - 1, colBankPaymentPercentage, xlsRow, colBankPaymentPercentage].Merge();
        //                                    sheet1.Range[xlsRow - 1, colCashPaymentPercentage, xlsRow, colCashPaymentPercentage].Merge();
        //                                    sheet1.Range[xlsRow - 1, colBankPaymentPercentage, xlsRow, colCashPaymentPercentage].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                    sheet1.Range[xlsRow - 1, colBankPaymentPercentage, xlsRow, colCashPaymentPercentage].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                    sheet1.Range[xlsRow - 1, colBankPaymentPercentage, xlsRow, colCashPaymentPercentage].CellStyle.Font.Bold = false;

        //                                }
        //                                sheet1.Range[xlsRow - 1, colSignature, particular3rdRow + 2, colSignature].Merge();
        //                                sheet1.Range[xlsRow - 1, colSignature, particular3rdRow + 2, colSignature].BorderAround(ExcelLineStyle.Hair);
        //                                sheet1.Range[xlsRow - 1, 1, particular3rdRow + 2, colSignature].BorderAround(ExcelLineStyle.Hair);
        //                                sheet1.Range[xlsRow - 1, 1, particular3rdRow + 2, colSignature].BorderInside(ExcelLineStyle.Hair);

        //                            }
        //                        }
        //                        else if (onlyEarning)
        //                        {


        //                            //empModulasFactor = empNoPerPage;
        //                            sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                            sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Size = 40;
        //                            sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                            sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].Merge();
        //                            if (dtbankCash.Rows.Count > 0)
        //                            {

        //                                sheet1.Range[xlsRow - 1, colBankPaymentPercentage, xlsRow, colBankPaymentPercentage].Merge();
        //                                sheet1.Range[xlsRow - 1, colCashPaymentPercentage, xlsRow, colCashPaymentPercentage].Merge();
        //                                sheet1.Range[xlsRow - 1, colBankPaymentPercentage, xlsRow, colCashPaymentPercentage].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                sheet1.Range[xlsRow - 1, colBankPaymentPercentage, xlsRow, colCashPaymentPercentage].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow - 1, colBankPaymentPercentage, xlsRow, colCashPaymentPercentage].CellStyle.Font.Bold = false;

        //                            }
        //                            sheet1.Range[xlsRow - 1, colSignature, xlsRow, colSignature].Merge();
        //                            sheet1.Range[xlsRow - 1, colSignature, xlsRow, colSignature].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow - 1, 1, xlsRow, colSignature].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow - 1, 1, xlsRow, colSignature].BorderInside(ExcelLineStyle.Hair);


        //                        }
        //                        else
        //                        {
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Size = 40;
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].Merge();
        //                            if (dtbankCash.Rows.Count > 0)
        //                            {

        //                                sheet1.Range[xlsRow, colBankPaymentPercentage, xlsRow, colBankPaymentPercentage].Merge();
        //                                sheet1.Range[xlsRow, colCashPaymentPercentage, xlsRow, colCashPaymentPercentage].Merge();
        //                                sheet1.Range[xlsRow, colBankPaymentPercentage, xlsRow, colCashPaymentPercentage].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                sheet1.Range[xlsRow, colBankPaymentPercentage, xlsRow, colCashPaymentPercentage].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow, colBankPaymentPercentage, xlsRow, colCashPaymentPercentage].CellStyle.Font.Bold = false;

        //                            }
        //                            sheet1.Range[xlsRow, colSignature, particular3rdRow + 3, colSignature].Merge();
        //                            sheet1.Range[xlsRow, colSignature, particular3rdRow + 3, colSignature].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, 1, particular3rdRow + 3, colSignature].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, 1, particular3rdRow + 3, colSignature].BorderInside(ExcelLineStyle.Hair);
        //                        }

        //                        #region DailyAttendance
        //                        if (withAttendance == true)
        //                        {
        //                            //dvDaily = new DataView(dsDaily);
        //                            //dvDaily.RowFilter = "EmployeePK = '" + EmpIdPR + "' ";
        //                            var mnthColData = 0;
        //                            mnthColData = colParticulars;

        //                            //var dtFrmDtIntData = 1;
        //                            //var dtEndDateIntData = 31;

        //                            #region MyRegion
        //                            try
        //                            {
        //                                if (dicAttendance.ContainsKey(dtEmployees.Rows[i]["EmpSystemId"].ToString()))
        //                                {


        //                                    List<DataRow> drData = dicAttendance[dtEmployees.Rows[i]["EmpSystemId"].ToString()];

        //                                    foreach (DataRow item in drData)
        //                                    {
        //                                        try
        //                                        {

        //                                            sheet1[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].Text = item["DayStatus"].ToString();
        //                                            sheet1[particular3rdRow + 1, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].Text = item["InTime"].ToString();
        //                                            sheet1[particular3rdRow + 2, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].Text = item["OutTime"].ToString();
        //                                            sheet1[particular3rdRow + 3, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].Text = item["OTHr"].ToString();


        //                                            sheet1.Range[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), particular3rdRow + 3, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                                            sheet1.Range[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), particular3rdRow + 3, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                            sheet1.Range[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), particular3rdRow + 3, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Font.FontName = "Arial Narrow";
        //                                            sheet1.Range[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), particular3rdRow + 3, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Font.Size = 17;

        //                                            sheet1.Range[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), particular3rdRow + 3, colSignature - 1].BorderAround(ExcelLineStyle.Hair);
        //                                            sheet1.Range[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), particular3rdRow + 3, colSignature - 1].BorderInside(ExcelLineStyle.Hair);

        //                                        }
        //                                        catch
        //                                        {


        //                                        }
        //                                    }

        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            {

        //                                throw ex;
        //                            }
        //                            #endregion

        //                            xlsRow = particular3rdRow + 4;

        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;
        //                            xlsCol = np;
        //                            EmpCounter++;
        //                            if (EmpCounter % dtEmployees.Rows.Count == 0)
        //                            {
        //                                continue;
        //                            }

        //                            if ((EmpCounter % empModulasFactor) == 0)
        //                            {
        //                                if (!string.IsNullOrEmpty(groupBy))
        //                                {
        //                                    if (i < dtEmployees.Rows.Count - 1)
        //                                    {
        //                                        if (strGroupBySel == dtEmployees.Rows[i + 1][groupBy + "ID"].ToString())
        //                                        {
        //                                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Hair;
        //                                            xlsRow++;
        //                                            sheet1[xlsRow, 1].RowHeight = 2;

        //                                            sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);

        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Hair;
        //                                    xlsRow++;
        //                                    sheet1[xlsRow, 1].RowHeight = 2;

        //                                    sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);

        //                                }
        //                            }
        //                            xlsRow--;

        //                        }
        //                        else
        //                        {
        //                            xlsRow++;
        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;
        //                            xlsCol = np;
        //                            #region Border Setup
        //                            EmpCounter++;
        //                            if (withAttendance)
        //                            {
        //                                //if (sheetBasedOn == "structured")
        //                                //{
        //                                if (EmpCounter % dtEmployees.Rows.Count == 0)
        //                                {
        //                                    continue;
        //                                }

        //                                if ((EmpCounter % empModulasFactor) == 0)
        //                                {
        //                                    if (!string.IsNullOrEmpty(groupBy))
        //                                    {
        //                                        if (i < dtEmployees.Rows.Count - 1)
        //                                        {
        //                                            if (strGroupBySel == dtEmployees.Rows[i + 1][groupBy + "ID"].ToString())
        //                                            {
        //                                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Hair;
        //                                                xlsRow++;//Page Break Last Border was not visible. That's why We increase one extra row.
        //                                                sheet1[xlsRow, 1].RowHeight = 2;

        //                                                sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);

        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Hair;
        //                                        xlsRow++;
        //                                        sheet1[xlsRow, 1].RowHeight = 2;

        //                                        sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);

        //                                    }
        //                                }
        //                                //}


        //                            }
        //                            else
        //                            {
        //                                if (EmpCounter % dtEmployees.Rows.Count == 0)
        //                                {
        //                                    continue;
        //                                }

        //                                if ((EmpCounter % (empModulasFactor)) == 0)// for structure and Earning Only 9 Employee Per Page
        //                                {
        //                                    if (!string.IsNullOrEmpty(groupBy))
        //                                    {
        //                                        if (i < dtEmployees.Rows.Count - 1)
        //                                        {
        //                                            if (strGroupBySel == dtEmployees.Rows[i + 1][groupBy + "ID"].ToString())
        //                                            {
        //                                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Hair;
        //                                                xlsRow++;
        //                                                sheet1[xlsRow, 1].RowHeight = 2;
        //                                                sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);

        //                                            }

        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Hair;
        //                                        xlsRow++;
        //                                        sheet1[xlsRow, 1].RowHeight = 2;
        //                                        sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);

        //                                    }


        //                                }
        //                            }


        //                            #endregion
        //                            #endregion *************************Data End*************************
        //                            xlsRow--;

        //                        }
        //                        #endregion
        //                    }
        //                }
        //                catch (Exception ex)
        //                {

        //                    throw ex;
        //                }

        //                #region Summation of all Salary head

        //                sheet1.Range[xlsRow + 1, colParticulars].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Total.ToString(), "SubTotal");
        //                sheet1.Range[xlsRow + 1, colParticulars].NumberFormat = oRU.NumberFormatDecimalZero();
        //                //sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].Merge();
        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].BorderAround(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                sheet1.Range[xlsRow + 1, colParticulars].RowHeight = 45;
        //                pageHeightDelemeter = 45;
        //                groupWisePageHeightDelemeter += 45;

        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].CellStyle.Font.Size = 32;
        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                foreach (var item in subTotalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
        //                {
        //                    try
        //                    {
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].Number = Convert.ToDouble(item.Value);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].Merge();
        //                    }
        //                    catch (Exception exe)
        //                    {
        //                        throw exe;
        //                    }
        //                }//Loop End Last Summation in SalaryPorcess totalNetPayDisbusmentAmount


        //                if (dtbankCash.Rows.Count > 0)
        //                {
        //                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage].Number = subTotalBankPayDisbusmentAmount;
        //                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                    sheet1.Range[xlsRow + 1, colCashPaymentPercentage].Number = subTotalCashPayDisbusmentAmount;
        //                    sheet1.Range[xlsRow + 1, colCashPaymentPercentage].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);

        //                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage, xlsRow + 1, colCashPaymentPercentage].CellStyle.Font.Size = 32;
        //                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage, xlsRow + 1, colCashPaymentPercentage].CellStyle.Font.Bold = true;
        //                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage, xlsRow + 1, colCashPaymentPercentage].BorderAround(ExcelLineStyle.Hair);
        //                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage, xlsRow + 1, colCashPaymentPercentage].BorderInside(ExcelLineStyle.Hair);


        //                }
        //                sheet1.Range[xlsRow + 1, colNetpayable].Number = subTotalNetPayDisbusmentAmount;
        //                sheet1.Range[xlsRow + 1, colNetpayable].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);

        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].CellStyle.Font.Size = 32;
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].BorderAround(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].BorderInside(ExcelLineStyle.Hair);



        //                //subTotalDictSalaryStruct = null;
        //                //subTotalDictSalaryProcess = null;

        //                subTotalNetPayDisbusmentAmount = 0;

        //                subTotalBankPayDisbusmentAmount = 0;
        //                subTotalCashPayDisbusmentAmount = 0;

        //                xlsRow += 1;
        //                sheet1.Range[xlsRow + 1, colParticulars].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Total.ToString(), "Total");
        //                sheet1.Range[xlsRow + 1, colParticulars].NumberFormat = oRU.NumberFormatDecimalZero();
        //                //sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].Merge();
        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                sheet1.Range[xlsRow + 1, colParticulars].BorderAround(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow + 1, colParticulars].RowHeight = 40;
        //                pageHeightDelemeter += 40;
        //                groupWisePageHeightDelemeter += 40;

        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].CellStyle.Font.Size = 24;
        //                sheet1.Range[xlsRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                foreach (var item in totalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
        //                {
        //                    try
        //                    {
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].Number = Convert.ToDouble(item.Value);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].Merge();

        //                    }
        //                    catch (Exception exe)
        //                    {
        //                        throw exe;
        //                    }
        //                }//Loop End Last Summation in SalaryPorcess totalNetPayDisbusmentAmount

        //                if (dtbankCash.Rows.Count > 0)
        //                {
        //                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage].Number = totalBankPayDisbusmentAmount;
        //                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                    sheet1.Range[xlsRow + 1, colCashPaymentPercentage].Number = totalCashPayDisbusmentAmount;
        //                    sheet1.Range[xlsRow + 1, colCashPaymentPercentage].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);

        //                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage, xlsRow + 1, colCashPaymentPercentage].CellStyle.Font.Size = 24;
        //                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage, xlsRow + 1, colCashPaymentPercentage].CellStyle.Font.Bold = true;
        //                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage, xlsRow + 1, colCashPaymentPercentage].BorderAround(ExcelLineStyle.Hair);
        //                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage, xlsRow + 1, colCashPaymentPercentage].BorderInside(ExcelLineStyle.Hair);
        //                }

        //                sheet1.Range[xlsRow + 1, colNetpayable].Number = totalNetPayDisbusmentAmount;
        //                sheet1.Range[xlsRow + 1, colNetpayable].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);

        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].BorderAround(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].BorderInside(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].CellStyle.Font.Size = 24;

        //                int numberOfColumns = colSignature - colParticulars;

        //                int remainCell = 0;
        //                //if (sheetBasedOn == "structured" && withAttendance == false)
        //                //{
        //                //remainCell = numberOfColumns - 24;

        //                int startingColumn = colParticulars;
        //                int lastColumn = colSignature - 1;
        //                int perUnit = (lastColumn - startingColumn) / 3;
        //                int UsedRange = 6;
        //                for (int si = 0; si < dtSigConfig.Rows.Count; si++)
        //                {
        //                    if (si == 0)
        //                    {
        //                        sheet1.Range[xlsRow + 20, ColName].Text = dtSigConfig.Rows[si]["FieldName"].ToString();
        //                        sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Size = 50;
        //                        sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Bold = true;
        //                        sheet1.Range[xlsRow + 20, ColName].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Hair;
        //                    }
        //                    if (si == 4)
        //                    {
        //                        sheet1.Range[xlsRow + 20, colSignature - 1].Text = dtSigConfig.Rows[si]["FieldName"].ToString();
        //                        sheet1.Range[xlsRow + 20, colSignature - 1].CellStyle.Font.Size = 50;
        //                        sheet1.Range[xlsRow + 20, colSignature - 1].CellStyle.Font.Bold = true;
        //                        sheet1.Range[xlsRow + 20, colSignature - 1, xlsRow + 20, colSignature].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Hair;


        //                        sheet1.Range[xlsRow + 20, colSignature - 1, xlsRow + 20, colSignature].Merge();
        //                        sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].RowHeight = 88;//Update
        //                        sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                        sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                    }

        //                    if (si > 0 && si < 4)
        //                    {
        //                        sheet1.Range[xlsRow + 20, startingColumn].Text = dtSigConfig.Rows[si]["FieldName"].ToString();
        //                        sheet1.Range[xlsRow + 20, startingColumn].CellStyle.Font.Size = 50;
        //                        sheet1.Range[xlsRow + 20, startingColumn].CellStyle.Font.Bold = true;
        //                        sheet1.Range[xlsRow + 20, startingColumn, xlsRow + 20, startingColumn + UsedRange].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Hair;
        //                        sheet1.Range[xlsRow + 20, startingColumn, xlsRow + 20, startingColumn + UsedRange].Merge();

        //                        startingColumn += perUnit;
        //                    }
        //                }



        //                #endregion
        //                #endregion ----------------------Data End-----------------------

        //                #region ******************Report Header******************
        //                objRpt.SelectedPlantWiseCompany(plantId, languageId, out dsCmp);
        //                xlsRow = 1;
        //                xlsCol = 1;

        //                FactoryName = string.Empty;

        //                var FactoryAddress = string.Empty;

        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyNameLocal"].ToString();
        //                }
        //                else
        //                {
        //                    CmpName = "";
        //                }
        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantNameLocal"].ToString();
        //                }
        //                else
        //                {
        //                    FactoryName = "";
        //                }
        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddressLocal"].ToString();
        //                    if (FactoryAddress == "")
        //                    {
        //                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();

        //                    }
        //                }
        //                else
        //                {
        //                    FactoryAddress = "";
        //                }
        //                sheet1.Range[xlsRow, 1].Text = CmpName + " ( " + FactoryName + " )";
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 40;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;

        //                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = printFont;

        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 50;

        //                xlsRow++;
        //                sheet1.Range[xlsRow, 1].Text = FactoryAddress;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 35;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 38;
        //                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

        //                sheet1.Range[xlsRow - 1, endXlsCol].Text = "Print Date: " + printDate + Environment.NewLine + "Payment Date:" + paymentDate;
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Font.Size = 24;
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Font.FontName = "ArialNarrow";
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

        //                string yearLocal = ru.cnDgt(Convert.ToDateTime(para.FromDate).Year.ToString(), localLanguage);

        //                xlsRow += 1;
        //                sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.SalarySheet.ToString(), "Pay Register") + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), localLanguage) + "-" + yearLocal; //_payRegisterLocal + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), "Bengali") + "," + yearLocal;
        //                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 40;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 50;
        //                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;



        //                #endregion ******************Report Header******************

        //                #region Freeze Panes
        //                sheet1.UsedRange["A7"].FreezePanes();
        //                sheet1.FirstVisibleColumn = 1;
        //                sheet1.FirstVisibleRow = 5;
        //                #endregion

        //                #region UsedRange Alignment
        //                sheet1.UsedRange.WrapText = true;
        //                //sheet1.UsedRange.is;
        //                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
        //                #endregion UsedRange Alignment


        //                #region Page Setup
        //                //sheet1.PageSetup.TopMargin = 0.2;
        //                //sheet1.PageSetup.BottomMargin = 0.7;
        //                //if (!string.IsNullOrEmpty(groupBy))
        //                //{
        //                //    sheet1.PageSetup.PrintTitleRows = "$A$1:$IV$" + titleRow;

        //                //}
        //                //else
        //                //{
        //                //    sheet1.PageSetup.PrintTitleRows = "$A$1:$IV$" + titleRow;

        //                //}
        //                sheet1.PageSetup.PrintTitleRows = "$1:$" + titleRow;

        //                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&15" + "Page " + "&p" + " of " + "&N";
        //                sheet1.PageSetup.LeftMargin = 0.3;
        //                sheet1.PageSetup.RightMargin = 0.2;
        //                sheet1.PageSetup.TopMargin = 0.2;
        //                sheet1.PageSetup.BottomMargin = 0.0;
        //                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
        //                sheet1.PageSetup.FitToPagesTall = 0;
        //                sheet1.PageSetup.FitToPagesWide = 1;

        //                if (paperSize == "Legal")
        //                {
        //                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperLegal;
        //                    //sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
        //                }
        //                if (paperSize == "A4")
        //                {
        //                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
        //                }


        //                sheet1.Name = "EmpPayRegister" + para.SalaryProcessId;
        //                #endregion

        //                return workbook;
        //            }

        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //            finally
        //            {
        //                objRpt = null;
        //                excelEngine = null;
        //                application = null;
        //                workbook = null;
        //                sheet1 = null;
        //            }
        //        }

        //        public IWorkbook ComEmployeeSalaryRegisterWithStructure(string companyGroupId, string companyId, string plantId, string year, string month, string languageId, string paymentDate, string printDate, string fromDate, string toDate, string groupBy, Dictionary<string, string> parameters, string salaryProcessId, string sheetBasedOn, bool withAttendance, string paperSize, string docGrouping, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity)
        //        {
        //            #region Variable
        //            clsReport objRpt = null;
        //            clsSalaryUtility objSalary = null;
        //            DataSet dsSlrProced = null;
        //            DataSet dsLeaveInfo = null;
        //            DataSet dsLeaveType = null;
        //            DataSet dsEmpAttdnInfo = null;
        //            DataView dvEmp = null;
        //            DataView dvLeaveEmp = null;
        //            DataView dvLeaveType = null;

        //            DataView dvSlrProc = null;
        //            DataSet dsCmp = null;
        //            DataSet dsFactory = null;
        //            DataSet dsEmpLoyeeInfo = null;
        //            var StartDayCol = 0;
        //            DataTable dtSalaryHead = null;

        //            ExcelEngine excelEngine = null;
        //            IApplication application = null;
        //            IWorkbook workbook = null;
        //            IWorksheet sheet1 = null;
        //            ReportUtility ru = null;
        //            ParamList para = new ParamList();
        //            ParamList leavePara = new ParamList();
        //            ParamList attdnProcessParam = new ParamList();

        //            var pageHeightDelemeter = 0;
        //            var pageHeaderHeightDelemeter = 159;
        //            var groupWisePageHeightDelemeter = 0;
        //            var empModulasFactor = 0;

        //            para.PlantId = plantId;
        //            para.LanguageId = languageId;
        //            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
        //            var slrStartCol = 0;
        //            sheetBasedOn = "structured";
        //            withAttendance = false;
        //            #endregion Variable
        //            try
        //            {

        //                string sql1 = "select * From ComplianceAttendanceSetting where PlantId ='" + plantId + @"' ";
        //                DataTable dtValidation = _sqlRepository.GetDataTable(sql1);
        //                if (dtValidation.Rows.Count < 1)
        //                {
        //                    Exception ex = new Exception("OT Settings are incomplete .");
        //                    throw (ex);
        //                }


        //                ru = new ReportUtility();
        //                objRpt = new clsReport(_sqlRepository);
        //                objSalary = new clsSalaryUtility();
        //                ParaMontlyAttendance objm = new ParaMontlyAttendance();


        //                int StructreAndEarningExceptAttendance = 0;
        //                int EarningExceptAttendance = 0;
        //                int StructureAndEarningWithAttendance = 0;
        //                int EarningWithAttendance = 0;

        //                //GetPayRegisgeterConfig(companyId, plantId, out ExcludeFatherName, out ExcludeNonpayable_Notional, out ExcludeTotalGross, out ExcludeCTC);
        //                GetPayRegisgeterRowPerPage(companyId, plantId, out StructreAndEarningExceptAttendance, out EarningExceptAttendance, out StructureAndEarningWithAttendance, out EarningWithAttendance);

        //                if (withAttendance && sheetBasedOn == "structured")
        //                {
        //                    empModulasFactor = StructureAndEarningWithAttendance;
        //                }
        //                if (!withAttendance && sheetBasedOn == "structured")
        //                {

        //                    empModulasFactor = StructreAndEarningExceptAttendance;
        //                }
        //                if (withAttendance && sheetBasedOn != "structured")
        //                {
        //                    empModulasFactor = EarningWithAttendance;
        //                }




        //                #region Variable             

        //                var FactoryName = "";
        //                var CmpName = "";

        //                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
        //                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
        //                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
        //                var fdateOfMonth = "1" + "-" + monthName + "-" + year;

        //                Dictionary<string, string> labelList = ru.LocalLanguageLabelList(para.PlantId, languageId);

        //                var localLanguage = "";
        //                var printFont = "";
        //                bool isLocalLanguage = false;
        //                localLanguage = ru.LocalLanguageListSql(plantId, languageId, out isLocalLanguage);
        //                if (localLanguage == "Bengali")
        //                {
        //                    printFont = "SolaimanLipi";
        //                }
        //                else
        //                {
        //                    printFont = "Arial Narrow";
        //                }
        //                objm.AMonth = month;
        //                objm.AYear = year;
        //                objm.PlantId = plantId;
        //                objm.FDate = fdateOfMonth;
        //                objm.TDate = ldateOfMonth;
        //                var _ShiftCode = string.Empty;
        //                var salarySheetValue = 0.00;
        //                para.PlantId = plantId;
        //                leavePara.PlantId = plantId;
        //                para.FromDate = fdateOfMonth;
        //                para.ToDate = ldateOfMonth;
        //                para.SalaryProcessId = salaryProcessId;
        //                leavePara.FromDate = fdateOfMonth;
        //                leavePara.SalaryProcessId = salaryProcessId;
        //                #endregion Variable
        //                DateTime dtFrmDt = DateTime.Now;
        //                DateTime dtEndDate = DateTime.Now;
        //                double totalNetPayDisbusmentAmount = 0.00;
        //                double subTotalNetPayDisbusmentAmount = 0.00;

        //                bool ExcludeFatherName = false;
        //                bool ExcludeNonpayable_Notional = false;
        //                bool ExcludeTotalGross = false;
        //                bool ExcludeCTC = false;

        //                GetPayRegisgeterConfig(companyId, plantId, out ExcludeFatherName, out ExcludeNonpayable_Notional, out ExcludeTotalGross, out ExcludeCTC);
        //                string m = ru.GetMonthName(month);


        //                #region DataSet               

        //                #region Sorting Parameters
        //                string stringSalaryRegSorting = "";

        //                stringSalaryRegSorting = objRpt.GetSortingParameters(companyGroupId, companyId, plantId, groupBy);
        //                #endregion

        //                GetEmployeeInfoDetailCom(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, languageId, stringSalaryRegSorting, parameters, isActive, isSeperated, isMaternity, sa, ca, userId, out dsEmpLoyeeInfo);//Sql Query For Salary  Data
        //                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetail(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, languageId, parameters, isActive, isSeperated, isMaternity, out dtSalaryHead);

        //                var dtEmployees = dsEmpLoyeeInfo.Tables[0];
        //                if (dtEmployees.Rows.Count == 0)
        //                {
        //                    var ex = new Exception("No Data found...");
        //                    throw (ex);
        //                }
        //                List<SwapColumn> _list2 = new List<SwapColumn>();
        //                Dictionary<string, List<DataRow>> dicAttendance = new Dictionary<string, List<DataRow>>();
        //                if (withAttendance == true)
        //                {
        //                    dicAttendance = GetMonthlyDailyAttendanceDic(objm, parameters);
        //                }

        //                objRpt.SelectedPlantWiseCompany(plantId, languageId, out dsCmp);

        //                objRpt.SelectedPlant(plantId, out dsFactory);

        //                #endregion DataSet

        //                excelEngine = new ExcelEngine();
        //                application = excelEngine.Excel;

        //                workbook = application.Workbooks.Create(1);
        //                sheet1 = workbook.Worksheets[0];
        //                sheet1.IsGridLinesVisible = true;
        //                sheet1.IsDisplayZeros = false;

        //                #region------------------Column Header------------------
        //                xlsRow = 5;
        //                xlsCol = 1;

        //                var ColSr = 0;
        //                var ColName = 0;
        //                var ColLeaveInfo = 0;
        //                var ColWorkDaysInfo = 0;
        //                var colParticulars = 0;
        //                var ColGrs = 0;
        //                #endregion------------------Column Header------------------

        //                int RowIndex = xlsRow + 1;

        //                #region ----------------------Data-----------------------

        //                var strGroupBySel = "";
        //                var SrNo = 0;
        //                var EmpIdPR = "";
        //                var oRU = new ReportUtility();
        //                var intRow = 0;
        //                xlsRow = RowIndex;

        //                List<SalaryHeadSequence> list = null;

        //                var np = 0;
        //                var isFirst = true;
        //                var sigCol = 0;
        //                var deptFirstRow = 0;

        //                xlsRow--;
        //                Dictionary<string, SalaryHeadSequence> strListNew = null;

        //                var totalDictSalaryStruct = new Dictionary<string, double>();
        //                var totalDictSalaryProcess = new Dictionary<string, double>();

        //                var subTotalDictSalaryStruct = new Dictionary<string, double>();
        //                var subTotalDictSalaryProcess = new Dictionary<string, double>();


        //                int endCol = 5;
        //                int colNetpayable = endCol;
        //                int colSignature = endCol;
        //                #region RegisterHeader
        //                var colex = 0;
        //                sigCol = 0;


        //                OTSBD.clsSalary.clsSalaryReport sr = new OTSBD.clsSalary.clsSalaryReport();
        //                var titleRow = "6";
        //                #region ------------------Column Header Will be deleted------------------
        //                //if (string.IsNullOrEmpty(groupBy))
        //                //{
        //                xlsCol = 1;
        //                var lineHeader = "";
        //                if (!string.IsNullOrEmpty(docGrouping))
        //                {
        //                    lineHeader = docGrouping;
        //                }
        //                sheet1.Range[xlsRow - 1, xlsCol].Text = lineHeader;
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.Size = 48;
        //                sheet1.Range[xlsRow - 1, xlsCol].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow - 1, xlsCol, xlsRow - 1, xlsCol + 3].Merge();
        //                sheet1.Range[xlsRow - 1, xlsCol].RowHeight = 52;



        //                pageHeightDelemeter += 52;
        //                groupWisePageHeightDelemeter += 52;
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.SrNo.ToString(), "Sr. No."), sheet1, xlsRow + colex, ref xlsCol, out ColSr, 15, printFont, 90, 35);

        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.EmployeeInformation.ToString(), "Employee Information"), sheet1, xlsRow + colex, ref xlsCol, out ColName, 130, printFont, 0, 35);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.LeaveInformation.ToString(), "Leave Information"), sheet1, xlsRow + colex, ref xlsCol, out ColLeaveInfo, 50, printFont, 0, 35);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.WorkDaysDetail.ToString(), "Work Days Detail"), sheet1, xlsRow + colex, ref xlsCol, out ColWorkDaysInfo, 50, printFont, 0, 35);
        //                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.Particulars.ToString(), "Particulars"), sheet1, xlsRow + colex, ref xlsCol, out colParticulars, 28, printFont, 0, 35);
        //                ColGrs = colParticulars;
        //                var _count_earning_head = 0;
        //                var _count_earning_ctchead = 0;
        //                var _count_deducting_head = 0;
        //                var _total_head_count = 0;

        //                xlsRow += colex;
        //                if (withAttendance)
        //                {
        //                    slrStartCol = xlsCol;
        //                    CreateDynamicSHeadLocalLanguageStructNewWithAttendance(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out strListNew, labelList, printFont, ExcludeNonpayable_Notional, ExcludeTotalGross, ExcludeCTC);
        //                }
        //                if (!withAttendance)
        //                {

        //                    slrStartCol = xlsCol;
        //                    CreateDynamicSHeadLocalLanguageStructNewWithAttendance(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out strListNew, labelList, printFont, ExcludeNonpayable_Notional, ExcludeTotalGross, ExcludeCTC);

        //                    //CreateDynamicSHeadLocalLanguageStructNew(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out strListNew, labelList, printFont);

        //                    if (string.IsNullOrEmpty(groupBy))
        //                    {
        //                        sheet1.Range[xlsRow, colParticulars + 1].Text = "Earning";
        //                        sheet1.Range[xlsRow, colParticulars + 1].RowHeight = 63;
        //                        pageHeightDelemeter += 63;
        //                        groupWisePageHeightDelemeter += 63;
        //                        sheet1.Range[xlsRow, colParticulars + 1].CellStyle.Font.Size = 35;

        //                        sheet1.Range[xlsRow, colParticulars + 1, xlsRow, _count_earning_head + 4].Merge();

        //                        sheet1.Range[xlsRow, _count_earning_head + 5].Text = "Deduction";
        //                        sheet1.Range[xlsRow, _count_earning_head + 5].CellStyle.Font.Size = 35;
        //                        sheet1.Range[xlsRow, _count_earning_head + 5, xlsRow, _count_earning_head + 4 + _count_deducting_head].Merge();
        //                    }
        //                }
        //                xlsRow -= colex;


        //                #region Day of a month 
        //                StartDayCol = colParticulars;
        //                if (withAttendance == true)
        //                {
        //                    var mnthCol = 0;
        //                    mnthCol = colParticulars;
        //                    var dtFrmDtInt = 1;
        //                    var dtEndDateInt = 31;
        //                    while (dtFrmDtInt <= dtEndDateInt)
        //                    {
        //                        mnthCol += 1;
        //                        var sc = _list2.Find(r => Convert.ToInt32(r.ValueMember) == dtFrmDtInt);

        //                        var _col_index = mnthCol;
        //                        sheet1.Range[xlsRow, _col_index].Text = dtFrmDtInt.ToString();
        //                        sheet1.Range[xlsRow, _col_index].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
        //                        sheet1.Range[xlsRow, _col_index].CellStyle.Font.Bold = true;
        //                        sheet1.Range[xlsRow, _col_index].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                        sheet1.Range[xlsRow, _col_index].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        sheet1.Range[xlsRow, _col_index].CellStyle.Font.FontName = "Arial Narrow";
        //                        sheet1.Range[xlsRow, _col_index].CellStyle.Font.Size = 30;
        //                        sheet1.Range[xlsRow, _col_index].ColumnWidth = 13;
        //                        dtFrmDtInt++;
        //                    }
        //                }
        //                xlsRow += intRow;
        //                intRow = 1;
        //                endCol = 5;
        //                #endregion
        //                endCol = colParticulars;
        //                if (strListNew.Count > 0)
        //                {
        //                    xlsCol++;
        //                    np = ColGrs + strListNew.Count * 2;
        //                    endCol = np + 1;
        //                    xlsCol++;
        //                }
        //                if (withAttendance)
        //                {
        //                    if (endCol <= colParticulars + 29)
        //                        endCol = colParticulars + 30;

        //                }

        //                colNetpayable = endCol;

        //                sheet1.Range[xlsRow + 1, endCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.NetPayable.ToString(), "Net Payable");//netPayable.LocalLabel == null || netPayable.LocalLabel == "" ? netPayable.DefaultLabel : netPayable.LocalLabel;
        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();
        //                sheet1.Range[xlsRow + 1, endCol].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow + 1, endCol].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 1, colNetpayable].ColumnWidth = 18;
        //                sheet1.Range[xlsRow + 1, colNetpayable + 1].ColumnWidth = 18;


        //                int colPaymentMode = colNetpayable + 2;
        //                sheet1.Range[xlsRow, colPaymentMode].Text = "";//empSignatgure.LocalLabel == null || empSignatgure.LocalLabel == "" ? empSignatgure.DefaultLabel : empSignatgure.LocalLabel;
        //                sheet1.Range[xlsRow, colPaymentMode, xlsRow + 1, colPaymentMode].Merge();
        //                sheet1.Range[xlsRow, colPaymentMode].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, colPaymentMode].ColumnWidth = 10;



        //                colSignature = colPaymentMode + 1;

        //                sheet1.Range[xlsRow, colSignature].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Signature.ToString(), "Signature");//empSignatgure.LocalLabel == null || empSignatgure.LocalLabel == "" ? empSignatgure.DefaultLabel : empSignatgure.LocalLabel;
        //                sheet1.Range[xlsRow, colSignature, xlsRow + 1, colSignature].Merge();
        //                sheet1.Range[xlsRow, colSignature].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, colSignature].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow, colSignature].ColumnWidth = 70;

        //                endCol = colSignature;

        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].BorderAround(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].BorderInside(ExcelLineStyle.Hair);
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow + 1, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

        //                sheet1.Range[xlsRow - 1, endCol - 3].Text = ru.GetLabelname(labelList, "Days in The Month", "Days in The Month") + ": " + daysInMonth;
        //                sheet1.Range[xlsRow - 1, endCol - 3].CellStyle.Font.FontName = "Arial Narrow";
        //                sheet1.Range[xlsRow - 1, endCol - 3].CellStyle.Font.Size = 35;
        //                sheet1.Range[xlsRow - 1, endCol - 3, xlsRow - 1, endCol].Merge();
        //                sheet1.Range[xlsRow - 1, endCol - 3, xlsRow - 1, endCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                sheet1.Range[xlsRow - 1, endCol - 3, xlsRow - 1, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

        //                endXlsCol = endCol;
        //                sigCol = endCol;
        //                xlsCol = 1;


        //                xlsRow++;
        //                //}
        //                int EmpCounter = 0;

        //                #endregion
        //                objRpt.GetLeaveTypeLocal(leavePara, languageId, out dsLeaveType);
        //                dvLeaveType = new DataView();

        //                dvLeaveType.Table = dsLeaveType.Tables[0];
        //                var OTAmountDeduct = 0.00;
        //                var OTAmountAdd = 0.00;

        //                Dictionary<string, List<DataRow>> dicLeave = null;
        //                GetEmpLeaveInfo(leavePara, parameters, out dicLeave);
        //                var bankAccount = "";
        //                var empDOS = string.Empty;
        //                var empGross = string.Empty;
        //                double grossSalaryAmount = 0.00;

        //                var EL = 0.00;
        //                var CL = 0.00;
        //                var SL = 0.00;
        //                var locEL = GetLeaveType(dvLeaveType, "EL");
        //                var locCL = GetLeaveType(dvLeaveType, "CL");
        //                var locSL = GetLeaveType(dvLeaveType, "SL");
        //                bool pageBreakRequired = false;
        //                try
        //                {
        //                    for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
        //                    {
        //                        OTAmountDeduct = 0.00;
        //                        OTAmountAdd = 0.00;

        //                        try
        //                        {
        //                            if (string.IsNullOrEmpty(groupBy) == false)
        //                            {

        //                                var groupCount = 0;
        //                                if ((string.Compare(strGroupBySel.ToUpper(), dtEmployees.Rows[i][groupBy + "ID"].ToString().Trim().ToUpper())) != 0)
        //                                {
        //                                    var ex = 0;
        //                                    if (isFirst == false)
        //                                    {

        //                                        sheet1.Range[xlsRow + 1, colParticulars].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Total.ToString(), "SubTotal");
        //                                        sheet1.Range[xlsRow + 1, colParticulars].NumberFormat = oRU.NumberFormatDecimalZero();
        //                                        sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].Merge();
        //                                        sheet1.Range[xlsRow + 1, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                                        sheet1.Range[xlsRow + 1, colParticulars].RowHeight = 45;
        //                                        sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                        pageHeightDelemeter += 40;
        //                                        groupWisePageHeightDelemeter += 40;
        //                                        sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].CellStyle.Font.Size = 32;
        //                                        sheet1.Range[xlsRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                                        foreach (var item in subTotalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
        //                                        {
        //                                            try
        //                                            {
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].Number = Convert.ToDouble(item.Value);
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].BorderInside(ExcelLineStyle.Thin);

        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].Merge();
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].RowHeight = 40;
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].CellStyle.Font.Size = 32;
        //                                                sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].CellStyle.Font.Bold = true;
        //                                            }
        //                                            catch (Exception exe)
        //                                            {
        //                                                throw exe;
        //                                            }
        //                                        }//Loop End Last Summation in SalaryPorcess totalNetPayDisbusmentAmount
        //                                        sheet1.Range[xlsRow + 1, colNetpayable].Number = subTotalNetPayDisbusmentAmount;
        //                                        sheet1.Range[xlsRow + 1, colNetpayable].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);

        //                                        sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();
        //                                        sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].CellStyle.Font.Size = 28;
        //                                        sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                                        sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].BorderAround(ExcelLineStyle.Thin);

        //                                        ex = 1;
        //                                        //subTotalDictSalaryStruct = null;
        //                                        //subTotalDictSalaryProcess = null;
        //                                        subTotalDictSalaryStruct = new Dictionary<string, double>();
        //                                        subTotalDictSalaryProcess = new Dictionary<string, double>();
        //                                        subTotalNetPayDisbusmentAmount = 0;
        //                                        //EmpCounter = 0;

        //                                        //if (EmpCounter % 5 == 0 && groupCount % 3 == 0)
        //                                        //{
        //                                        sheet1.HPageBreaks.Add(sheet1[xlsRow + 2, 1]);//Page Break after each group
        //                                        pageHeightDelemeter = 0;
        //                                        groupWisePageHeightDelemeter = 0;
        //                                        EmpCounter = 0;
        //                                        //empModulasFactor = 10;
        //                                        //}
        //                                    }
        //                                    #region ------------------Column Header------------------
        //                                    xlsCol = 1;

        //                                    sheet1.Range[xlsRow + ex - 1, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

        //                                    xlsRow += ex;
        //                                    if (withAttendance)
        //                                    {

        //                                        titleRow = "6";
        //                                    }
        //                                    if (!withAttendance)
        //                                    {
        //                                        titleRow = "5";
        //                                    }

        //                                    endCol = 5;
        //                                    xlsCol = 1;

        //                                    sheet1.Range[xlsRow + 1, 1].Text = groupBy + " :-" + dtEmployees.Rows[i][groupBy + "Name"].ToString();
        //                                    sheet1.Range[xlsRow + 1, 1, xlsRow + 1, 5].Merge();
        //                                    sheet1.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;
        //                                    sheet1.Range[xlsRow + 1, 1].CellStyle.Font.Size = 48;
        //                                    sheet1.Range[xlsRow + 1, 1].RowHeight = 50;
        //                                    pageHeightDelemeter += 100;
        //                                    groupWisePageHeightDelemeter += 100;
        //                                    xlsRow++;

        //                                    deptFirstRow = xlsRow;
        //                                    #endregion

        //                                    if (isFirst == true)
        //                                    {
        //                                        isFirst = false;
        //                                    }
        //                                }
        //                                groupCount++;
        //                                strGroupBySel = dtEmployees.Rows[i][groupBy + "ID"].ToString();
        //                            }
        //                        }
        //                        catch (Exception)
        //                        {
        //                            throw;
        //                        }
        //                        salarySheetValue = 0.00;

        //                        #endregion ------------------Column Header------------------

        //                        #region *************************Data*************************


        //                        xlsRow++;
        //                        #region LeaveInformation

        //                        EL = 0.00;
        //                        CL = 0.00;
        //                        SL = 0.00;

        //                        try
        //                        {
        //                            List<DataRow> drEmpLeave = dicLeave[dtEmployees.Rows[i]["EmpSystemID"].ToString()];
        //                            for (int li = 0; li < drEmpLeave.Count(); li++)
        //                            {
        //                                if (drEmpLeave[li]["Code"].ToString().ToUpper() == "EL")
        //                                    EL = clsStaticInfo.dbl(drEmpLeave[li]["AvailedLeave"].ToString());
        //                                if (drEmpLeave[li]["Code"].ToString().ToUpper() == "CL")
        //                                    CL = clsStaticInfo.dbl(drEmpLeave[li]["AvailedLeave"].ToString());
        //                                if (drEmpLeave[li]["Code"].ToString().ToUpper() == "SL")
        //                                    SL = clsStaticInfo.dbl(drEmpLeave[li]["AvailedLeave"].ToString());
        //                            }
        //                        }
        //                        catch (Exception)
        //                        {


        //                        }
        //                        #endregion


        //                        #region EmpInfo
        //                        SrNo += 1;
        //                        EmpIdPR = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();

        //                        sheet1.Range[xlsRow, ColSr].Text = ru.cnDgt(Convert.ToString(SrNo), localLanguage);
        //                        sheet1.Range[xlsRow, ColSr].RowHeight = 88;
        //                        pageHeightDelemeter += 150;
        //                        groupWisePageHeightDelemeter += 150;
        //                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        if (sheetBasedOn == "structured")
        //                        {
        //                            if (withAttendance == false)
        //                            {
        //                                sheet1.Range[xlsRow, ColSr, xlsRow + 1, ColSr].Merge();

        //                            }
        //                            else
        //                            {
        //                                sheet1.Range[xlsRow, ColSr, xlsRow + 5, ColSr].Merge();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            sheet1.Range[xlsRow, ColSr, xlsRow + 4, ColSr].Merge();
        //                        }
        //                        sheet1.Range[xlsRow, ColSr].CellStyle.Font.FontName = "Arial Narrow";
        //                        sheet1.Range[xlsRow, ColSr].CellStyle.Font.Size = 25;

        //                        //3
        //                        var _DOJ = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOJ.ToString(), "DOJ");
        //                        var _DOS = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOS.ToString(), "DOS");
        //                        var _designationLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Designation.ToString(), "Designation");
        //                        var _fatherName = ru.GetLabelname(labelList, LabelNameInLocalLanguage.FatherName.ToString(), "Father Name");
        //                        var _gradeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Grade.ToString(), "Grade");
        //                        var empName = "";
        //                        if (isLocalLanguage)
        //                        {
        //                            empName = dtEmployees.Rows[i]["EmployeeNameLocal"].ToString();
        //                        }
        //                        else
        //                        {
        //                            empName = dtEmployees.Rows[i]["EmployeeName"].ToString();

        //                        }
        //                        var empDesignation = _designationLocal + ":" + dtEmployees.Rows[i]["DesignationLocal"].ToString();
        //                        var empDOJ = _DOJ + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOJ"].ToString(), localLanguage);
        //                        var empFatherName = _fatherName + ":" + dtEmployees.Rows[i]["FatherName"].ToString();
        //                        bankAccount = "";
        //                        empDOS = string.Empty;
        //                        empGross = string.Empty;
        //                        grossSalaryAmount = 0.00;
        //                        int earnedSalaryRowHeight = 0;

        //                        grossSalaryAmount = clsStaticInfo.dbl(dtEmployees.Rows[i]["GrossAmount"].ToString());

        //                        var grossAmountLabel = ru.GetLabelname(labelList, LabelNameInLocalLanguage.TotalSalary.ToString(), "Gross");

        //                        empGross = grossAmountLabel + ":" + ru.cnDgt(grossSalaryAmount.ToString(), localLanguage).ToString();

        //                        if (dtEmployees.Rows[i]["DOS"].ToString().Length > 0)
        //                        {
        //                            empDOS = _DOS + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOS"].ToString(), localLanguage); //+ Environment.NewLine;
        //                        }
        //                        if (dtEmployees.Rows[i]["BankAccNo"].ToString().Length > 0)
        //                        {
        //                            bankAccount = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bank Acc No") + ":" + dtEmployees.Rows[i]["BankAccNo"].ToString(); //+ Environment.NewLine;
        //                        }
        //                        if (withAttendance == false)
        //                        {
        //                            // With Structure and Earned only
        //                            int secondRowHeight = 140;
        //                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString() + "::" + empName;
        //                            sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                            sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, ColName].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, ColName].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow, ColName].CellStyle.Font.Bold = true;
        //                            sheet1.Range[xlsRow, ColName].CellStyle.Font.Size = 50;
        //                            sheet1.Range[xlsRow, ColName].RowHeight = 90;
        //                            pageHeightDelemeter = 140;
        //                            groupWisePageHeightDelemeter += 140;
        //                            sheet1.Range[xlsRow, ColName].ColumnWidth = 150;

        //                            //sheet1.Range[xlsRow + 1, ColName].Text = empGross + "%OA" + _designationLocal+ "%OA"+ _gradeLocal+"%OA"+ bankAccount+"%OA"+ empDOJ+"%OA"+empDOS;

        //                            IRichTextString rtf1 = sheet1.Range[xlsRow + 1, ColName].RichText;
        //                            if (ExcludeFatherName == false)
        //                            {
        //                                if (!string.IsNullOrEmpty(dtEmployees.Rows[i]["FatherName"].ToString()))
        //                                {
        //                                    FormatText(ref sheet1, ref rtf1, empFatherName, 30);
        //                                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                                    secondRowHeight += 25;
        //                                }
        //                            }
        //                            FormatText(ref sheet1, ref rtf1, _designationLocal + ":" + dtEmployees.Rows[i]["DesignationLocal"].ToString() + " ", 30);
        //                            FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                            FormatText(ref sheet1, ref rtf1, _gradeLocal + ":", 30); FormatText(ref sheet1, ref rtf1, dtEmployees.Rows[i]["GradeCode"].ToString() + " ", 30);
        //                            if (dtEmployees.Rows[i]["BankAccNo"].ToString().Length > 0)
        //                            {
        //                                bankAccount = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bnk Acc") + ":" + dtEmployees.Rows[i]["BankAccNo"].ToString(); //+ Environment.NewLine;
        //                                FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                                FormatText(ref sheet1, ref rtf1, bankAccount + " ", 30);
        //                                secondRowHeight += 25;
        //                            }
        //                            FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                            FormatText(ref sheet1, ref rtf1, empDOJ + " ", 30);
        //                            if (dtEmployees.Rows[i]["DOS"].ToString().Length > 0)
        //                            {
        //                                empDOS = _DOS + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOS"].ToString(), localLanguage); //+ Environment.NewLine;
        //                                FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                                FormatText(ref sheet1, ref rtf1, empDOS + " ", 30);
        //                                secondRowHeight += 25;
        //                            }
        //                            sheet1.Range[xlsRow + 1, ColName].RowHeight = secondRowHeight;//secondRowHeight;
        //                            earnedSalaryRowHeight = secondRowHeight;

        //                            pageHeightDelemeter = 135;
        //                            groupWisePageHeightDelemeter += 135;
        //                            sheet1.Range[xlsRow + 1, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                            sheet1.Range[xlsRow + 1, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        }
        //                        else
        //                        {
        //                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();

        //                            sheet1.Range[xlsRow + 1, ColName].Text = empName;
        //                            sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                            sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].BorderAround(ExcelLineStyle.Hair);
        //                            sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].CellStyle.Font.Bold = true;
        //                            sheet1.Range[xlsRow, ColName, xlsRow + 1, ColName].CellStyle.Font.Size = 30;
        //                            sheet1.Range[xlsRow, ColName].ColumnWidth = 65;
        //                            sheet1.Range[xlsRow + 1, ColName].ColumnWidth = 65;

        //                            IRichTextString rtf1 = sheet1.Range[xlsRow + 2, ColName].RichText;

        //                            FormatText(ref sheet1, ref rtf1, empGross + " ", 25);
        //                            FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                            if (ExcludeFatherName == false)
        //                            {
        //                                if (!string.IsNullOrEmpty(dtEmployees.Rows[i]["FatherName"].ToString()))
        //                                {
        //                                    FormatText(ref sheet1, ref rtf1, empFatherName, 30);
        //                                    FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                                }
        //                            }
        //                            FormatText(ref sheet1, ref rtf1, _designationLocal + ":" + dtEmployees.Rows[i]["DesignationLocal"].ToString() + " ", 25);
        //                            FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                            FormatText(ref sheet1, ref rtf1, _gradeLocal + ":", 25); FormatText(ref sheet1, ref rtf1, dtEmployees.Rows[i]["GradeCode"].ToString() + " ", 25);
        //                            if (dtEmployees.Rows[i]["BankAccNo"].ToString().Length > 0)
        //                            {
        //                                bankAccount = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bank Acc No") + ":" + dtEmployees.Rows[i]["BankAccNo"].ToString(); //+ Environment.NewLine;
        //                                FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                                FormatText(ref sheet1, ref rtf1, bankAccount + " ", 25);
        //                            }
        //                            if (dtEmployees.Rows[i]["DOS"].ToString().Length > 0)
        //                            {
        //                                empDOS = _DOS + ":" + ru.GetFormatedDate(dtEmployees.Rows[i]["DOS"].ToString(), localLanguage); //+ Environment.NewLine;

        //                            }
        //                            FormatText(ref sheet1, ref rtf1, Environment.NewLine, 6);
        //                            FormatText(ref sheet1, ref rtf1, empDOJ + "||" + empDOS + " ", 25);

        //                            sheet1.Range[xlsRow + 2, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                            sheet1.Range[xlsRow + 2, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        }
        //                        if (sheetBasedOn == "structured")
        //                        {

        //                            if (withAttendance == false)
        //                            {
        //                                // sheet1.Range[xlsRow + 2, ColName, xlsRow + 5, ColName].Merge();
        //                                // sheet1.Range[xlsRow + 2, ColName, xlsRow + 5, ColName].BorderAround(ExcelLineStyle.Thin);
        //                            }
        //                            else
        //                            {
        //                                sheet1.Range[xlsRow + 2, ColName, xlsRow + 5, ColName].Merge();
        //                                sheet1.Range[xlsRow + 2, ColName, xlsRow + 5, ColName].BorderAround(ExcelLineStyle.Thin);
        //                            }

        //                        }
        //                        else
        //                        {
        //                            sheet1.Range[xlsRow + 2, ColName, xlsRow + 4, ColName].Merge();
        //                            sheet1.Range[xlsRow + 2, ColName, xlsRow + 4, ColName].BorderAround(ExcelLineStyle.Thin);
        //                        }
        //                        //Leave Info
        //                        //if (dtEmpAttdnInfo.Rows.Count > 0)
        //                        //{
        //                        string lateBangla = ru.cnDgt(Convert.ToDouble(dtEmployees.Rows[i]["TotalLate"]).ToString(), localLanguage);
        //                        string lwpBangla = ru.cnDgt(Convert.ToDouble(dtEmployees.Rows[i]["TotalLWP"]).ToString(), localLanguage);
        //                        double presentLate = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalPresent"].ToString()) + clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLate"].ToString());
        //                        string presentBangla = ru.cnDgt(Convert.ToString(presentLate), localLanguage);
        //                        string absentBangla = ru.cnDgt((Convert.ToDouble(dtEmployees.Rows[i]["TotalAbsent"]) - Convert.ToDouble(dtEmployees.Rows[i]["TotalLWP"])).ToString(), localLanguage);
        //                        double weekOffHoliday = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOffHoliDay"].ToString()) + clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());

        //                        string weekOff = ru.cnDgt(weekOffHoliday.ToString(), localLanguage);
        //                        string holiDay = ru.cnDgt(Convert.ToDouble(dtEmployees.Rows[i]["TotalHoliDay"]).ToString(), localLanguage);
        //                        string leave = ru.cnDgt(Convert.ToDouble(dtEmployees.Rows[i]["TotalLv"]).ToString(), localLanguage);
        //                        string totalOTHr = "";//ru.cnDgt((Math.Round((Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalNormalOTHr"]) + Convert.ToDouble(dtEmpAttdnInfo.Rows[0]["TotalExtraOTHr"])) / 60, 2).ToString()), localLanguage);
        //                        var _availedLeave = ru.GetLabelname(labelList, LabelNameInLocalLanguage.AvailedLeave.ToString(), "Availed Leave");
        //                        var _late = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Late.ToString(), "Late");
        //                        var _LWP = ru.GetLabelname(labelList, LabelNameInLocalLanguage.LWP.ToString(), "LWP");
        //                        var _lunchOutHour = ru.GetLabelname(labelList, LabelNameInLocalLanguage.LunchOutHour.ToString(), "Lunch Out Hr");//lunchOutHour.LocalLabel == null || lunchOutHour.LocalLabel == "" ? lunchOutHour.DefaultLabel : lunchOutHour.LocalLabel;
        //                        var _otHour = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OTHours.ToString(), "OT Hrs");
        //                        var _otRateLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OTRate.ToString(), "OT Rate");

        //                        double otRate = 0.00;
        //                        string otRateBangla = "";//IsOTEntitled
        //                        if (Convert.ToBoolean(dtEmployees.Rows[i]["IsOTEntitled"]) == true)
        //                        {
        //                            //totalOTHr = ru.cnDgt((Math.Round((Convert.ToDouble(dtEmployees.Rows[i]["TotalOTHr"]) + Convert.ToDouble(dtEmployees.Rows[i]["TotalNormalOTHr"]) + Convert.ToDouble(dtEmployees.Rows[i]["TotalExtraOTHr"])) / 60, 2).ToString()), localLanguage);
        //                            totalOTHr = ru.cnDgt((Math.Round((Convert.ToDouble(dtEmployees.Rows[i]["TotalOTHr"])) / 60, 2).ToString()), localLanguage);


        //                            otRate = Convert.ToDouble(dtEmployees.Rows[i]["OTRate"]);
        //                            otRateBangla = ru.cnDgt(Math.Round(otRate, 2).ToString(), localLanguage);
        //                        }
        //                        IRichTextString rtfLeave = sheet1.Range[xlsRow, ColLeaveInfo].RichText;
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locEL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(EL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locCL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(CL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, locSL + ":" + ru.cnDgt(Math.Round(Convert.ToDecimal(SL), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, _availedLeave + ":" + ru.cnDgt(Math.Round(Convert.ToDouble(dtEmployees.Rows[i]["TotalLv"].ToString()), 0).ToString(), localLanguage) + " ", 25);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, _LWP + ":" + lwpBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtfLeave, _lunchOutHour + ":" + "", 27);
        //                        FormatText(ref sheet1, ref rtfLeave, Environment.NewLine, 6);

        //                        sheet1.Range[xlsRow, ColLeaveInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                        sheet1.Range[xlsRow, ColLeaveInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        if (sheetBasedOn == "structured")
        //                        {
        //                            if (withAttendance == false)
        //                            {
        //                                sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 1, ColLeaveInfo].Merge();
        //                                sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 1, ColLeaveInfo].BorderAround(ExcelLineStyle.Thin);
        //                            }
        //                            else
        //                            {
        //                                sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 5, ColLeaveInfo].Merge();
        //                                sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 5, ColLeaveInfo].BorderAround(ExcelLineStyle.Thin);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 4, ColLeaveInfo].Merge();
        //                            sheet1.Range[xlsRow, ColLeaveInfo, xlsRow + 4, ColLeaveInfo].BorderAround(ExcelLineStyle.Thin);
        //                        }


        //                        string payDays = ru.cnDgt((Convert.ToDouble(dtEmployees.Rows[i]["TotalProcDate"]) - Convert.ToDouble(dtEmployees.Rows[i]["TotalAbsent"])).ToString(), localLanguage);



        //                        IRichTextString rtf = sheet1.Range[xlsRow, ColWorkDaysInfo].RichText;
        //                        var _payDaysLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.PayDays.ToString(), "Pay Days");
        //                        var _presentLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Present.ToString(), "Present");
        //                        var _absentLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Absent.ToString(), LabelNameInLocalLanguage.Absent.ToString());//absentLocal.LocalLabel == null || absentLocal.LocalLabel == "" ? absentLocal.DefaultLabel : absentLocal.LocalLabel;

        //                        var _weeklyHolidays = ru.GetLabelname(labelList, LabelNameInLocalLanguage.WeeklyLeaveDays.ToString(), "Weekend");
        //                        var _holiDays = ru.GetLabelname(labelList, LabelNameInLocalLanguage.HoliDay.ToString(), "Holidays");


        //                        FormatText(ref sheet1, ref rtf, _payDaysLocal + ":" + payDays + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _presentLocal + ":" + presentBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _absentLocal + ":" + absentBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _weeklyHolidays + ":" + weekOff + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _holiDays + ":" + holiDay + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        FormatText(ref sheet1, ref rtf, _late + ":" + lateBangla + " ", 27);
        //                        FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                        if (Convert.ToBoolean(dtEmployees.Rows[i]["IsOTEntitled"]) == true)
        //                        {
        //                            FormatText(ref sheet1, ref rtf, _otHour + ":" + totalOTHr + " ", 27);
        //                            FormatText(ref sheet1, ref rtf, Environment.NewLine, 6);
        //                            FormatText(ref sheet1, ref rtf, _otRateLocal + ":" + otRateBangla + " ", 27);
        //                        }

        //                        if (sheetBasedOn == "structured")
        //                        {
        //                            if (withAttendance == false)
        //                            {
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 1, ColWorkDaysInfo].Merge();
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 1, ColWorkDaysInfo].BorderAround(ExcelLineStyle.Thin);
        //                            }
        //                            else
        //                            {
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 5, ColWorkDaysInfo].Merge();
        //                                sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 5, ColWorkDaysInfo].BorderAround(ExcelLineStyle.Thin);
        //                            }

        //                        }
        //                        else
        //                        {
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 4, ColWorkDaysInfo].Merge();
        //                            sheet1.Range[xlsRow, ColWorkDaysInfo, xlsRow + 4, ColWorkDaysInfo].BorderAround(ExcelLineStyle.Thin);
        //                        }
        //                        //}

        //                        var earnedSalary = "";// labelList.Where(r => r.DefaultLabel == LabelNameInLocalLanguage.EarnedSalary.ToString()).FirstOrDefault();
        //                        if (labelList.ContainsKey(LabelNameInLocalLanguage.EarnedSalary.ToString()))
        //                        {
        //                            earnedSalary = labelList[LabelNameInLocalLanguage.EarnedSalary.ToString()];
        //                        }
        //                        if (sheetBasedOn == "structured")
        //                        {
        //                            var _structuredSalary = ru.GetLabelname(labelList, LabelNameInLocalLanguage.StructuredSalary.ToString(), "Str Sal");
        //                            sheet1.Range[xlsRow, colParticulars].Text = _structuredSalary + "->";
        //                            sheet1.Range[xlsRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[xlsRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                            sheet1.Range[xlsRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Size = 24;
        //                            sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Bold = true;

        //                            if (withAttendance == false)
        //                            {
        //                                //sheet1.Range[xlsRow, colParticulars].RowHeight = 150;//tarek

        //                            }
        //                            else
        //                            {
        //                                sheet1.Range[xlsRow, colParticulars].RowHeight = 52;//tarek
        //                                earnedSalaryRowHeight = 52;


        //                            }
        //                            xlsRow++;
        //                        }
        //                        else
        //                        {
        //                            earnedSalaryRowHeight = 52;
        //                        }

        //                        var _earnedSalary = ru.GetLabelname(labelList, LabelNameInLocalLanguage.EarnedSalary.ToString(), "Earned Salary");
        //                        sheet1.Range[xlsRow, colParticulars].Text = _earnedSalary + "->";
        //                        sheet1.Range[xlsRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                        sheet1.Range[xlsRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                        sheet1.Range[xlsRow, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                        sheet1.Range[xlsRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                        sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Size = 24;
        //                        sheet1.Range[xlsRow, colParticulars].CellStyle.Font.Bold = true;

        //                        sheet1.Range[xlsRow, colParticulars].RowHeight = earnedSalaryRowHeight;//tarek
        //                        if (withAttendance == false)
        //                        {
        //                            //sheet1.Range[xlsRow, colParticulars].RowHeight = 150;//tarek

        //                        }
        //                        else
        //                        {
        //                            if (sheetBasedOn == "structured")
        //                            {
        //                                //  sheet1.Range[xlsRow, colParticulars].RowHeight = 52;//tarek
        //                            }
        //                            else
        //                            {
        //                                sheet1.Range[xlsRow, colParticulars].RowHeight = 100;//tarek
        //                            }

        //                        }
        //                        var particular3rdRow = xlsRow;
        //                        if (withAttendance == true)
        //                        {
        //                            var _daysLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Days.ToString(), "Days");

        //                            particular3rdRow = xlsRow + 1;

        //                            var _attendance = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Attendance.ToString(), "Attendance");

        //                            sheet1.Range[particular3rdRow, colParticulars].Text = _attendance + "->";
        //                            sheet1.Range[particular3rdRow, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[particular3rdRow, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[particular3rdRow, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                            sheet1.Range[particular3rdRow, colParticulars].RowHeight = 50;
        //                            pageHeightDelemeter += 43;
        //                            groupWisePageHeightDelemeter += 43;
        //                            sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.Size = 17;
        //                            sheet1.Range[particular3rdRow, colParticulars].CellStyle.Font.Bold = true;

        //                            var _inTimeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.INTime.ToString(), "INTime");
        //                            var _outTimeLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OUTTime.ToString(), "OUTTime");
        //                            var _OTLocal = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OT.ToString(), "OT");

        //                            sheet1.Range[particular3rdRow + 1, colParticulars].Text = _inTimeLocal + "->";
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].RowHeight = 50;
        //                            pageHeightDelemeter += 43;
        //                            groupWisePageHeightDelemeter += 43;
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.Size = 17;
        //                            sheet1.Range[particular3rdRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                            sheet1.Range[particular3rdRow + 2, colParticulars].Text = _outTimeLocal + "->";
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].RowHeight = 50;
        //                            pageHeightDelemeter = 43;
        //                            groupWisePageHeightDelemeter += 43;
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.Size = 17;
        //                            sheet1.Range[particular3rdRow + 2, colParticulars].CellStyle.Font.Bold = true;

        //                            sheet1.Range[particular3rdRow + 3, colParticulars].Text = _OTLocal + "->";
        //                            sheet1.Range[particular3rdRow + 3, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[particular3rdRow + 3, colParticulars].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[particular3rdRow + 3, colParticulars].RowHeight = 50;

        //                            sheet1.Range[particular3rdRow + 3, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                            sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.Size = 17;
        //                            sheet1.Range[particular3rdRow + 3, colParticulars].CellStyle.Font.Bold = true;
        //                        }
        //                        #endregion
        //                        #region SalaryStructure

        //                        if (dicEmpSalry.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
        //                        {
        //                            List<DataRow> drSalaryHeadCollection = dicEmpSalry[dtEmployees.Rows[i]["EmpSystemID"].ToString()];

        //                            if (sheetBasedOn == "structured")
        //                            {
        //                                for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
        //                                {
        //                                    if (drSalaryHeadCollection[CI]["SalaryHead"].ToString().Contains("Tax"))
        //                                    {

        //                                    }
        //                                    try
        //                                    {
        //                                        SalaryHeadSequence xx = new SalaryHeadSequence();


        //                                        try
        //                                        {
        //                                            xx = strListNew[drSalaryHeadCollection[CI]["SalaryHeadID"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();

        //                                        }
        //                                        catch (Exception)
        //                                        {
        //                                            xx = null;
        //                                        }
        //                                        if (xx != null)
        //                                        {
        //                                            var slrStructureAmount = 0.00;
        //                                            if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
        //                                            {
        //                                                continue;
        //                                            }
        //                                            else if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "GROSS")
        //                                            {

        //                                            }
        //                                            else if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
        //                                            {
        //                                                slrStructureAmount = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["EntryAmount"].ToString());
        //                                                getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref subTotalDictSalaryStruct);//SubTotal
        //                                                getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref totalDictSalaryStruct);
        //                                                sheet1.Range[xlsRow - 1, xx.XLColIndex].Number = slrStructureAmount; //; clsStaticInfo.dbl(drSalaryHeadCollection[CI]["EntryAmount"].ToString());// * (-1);
        //                                            }
        //                                            else
        //                                            {
        //                                                slrStructureAmount = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["EntryAmount"].ToString());
        //                                                getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref subTotalDictSalaryStruct);//SubTotal
        //                                                getTotalAmount(xx.XLColIndex.ToString(), slrStructureAmount, ref totalDictSalaryStruct);
        //                                            }

        //                                            sheet1.Range[xlsRow - 1, xx.XLColIndex].Number = slrStructureAmount;
        //                                            sheet1.Range[xlsRow - 1, xx.XLColIndex - 1, xlsRow - 1, xx.XLColIndex].NumberFormat = ru.GetDecimalFormatlocal(xx, localLanguage);
        //                                        }
        //                                    }
        //                                    catch (Exception ex)
        //                                    {

        //                                        throw ex;
        //                                    }

        //                                }
        //                                var slrStartCol2 = slrStartCol;
        //                                for (int isl = 0; isl < strListNew.Count; isl++)
        //                                {
        //                                    sheet1.Range[xlsRow - 1, slrStartCol2 + isl, xlsRow - 1, slrStartCol2 + isl + 1].Merge();
        //                                    sheet1.Range[xlsRow - 1, slrStartCol2 + isl, xlsRow - 1, slrStartCol2 + isl + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                    sheet1.Range[xlsRow - 1, slrStartCol2 + isl, xlsRow - 1, slrStartCol2 + isl + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                    sheet1.Range[xlsRow - 1, slrStartCol2 + isl, xlsRow - 1, slrStartCol2 + isl + 1].CellStyle.Font.FontName = printFont;
        //                                    slrStartCol2++;
        //                                }
        //                            }

        //                            #endregion

        //                            #region SalarySheet

        //                            for (int ci = 0; ci < drSalaryHeadCollection.Count; ci++)
        //                            {

        //                                try
        //                                {
        //                                    salarySheetValue = 0.00;

        //                                    if (drSalaryHeadCollection[ci]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
        //                                    {

        //                                        sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].Number = (clsStaticInfo.dbl(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString()) - OTAmountDeduct) + OTAmountAdd;
        //                                        sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = ru.GetDecimalFormatlocalNetPay(Convert.ToBoolean(drSalaryHeadCollection[ci]["IntegerInDisb"].ToString()), Convert.ToInt32(drSalaryHeadCollection[ci]["DecimalNo"].ToString()), localLanguage);

        //                                        sheet1.Range[xlsRow, colPaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
        //                                        sheet1.Range[xlsRow, colPaymentMode].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                        sheet1.Range[xlsRow, colPaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                        sheet1.Range[xlsRow, colPaymentMode].BorderAround(ExcelLineStyle.Thin);
        //                                        sheet1.Range[xlsRow, colPaymentMode].CellStyle.Font.FontName = printFont;
        //                                        sheet1.Range[xlsRow, colPaymentMode].CellStyle.Font.Size = 34;
        //                                        sheet1.Range[xlsRow, colPaymentMode].CellStyle.Font.Bold = true;
        //                                        sheet1.Range[xlsRow, colPaymentMode].CellStyle.Rotation = 180;

        //                                        subTotalNetPayDisbusmentAmount += (clsStaticInfo.dbl(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString()) - OTAmountDeduct) + OTAmountAdd;
        //                                        totalNetPayDisbusmentAmount += (clsStaticInfo.dbl(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString()) - OTAmountDeduct) + OTAmountAdd;
        //                                        //}
        //                                        continue;
        //                                    }

        //                                    SalaryHeadSequence xx = new SalaryHeadSequence();//strListNew[drSalaryHeadCollection[ci]["SalaryHeadID"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
        //                                    try
        //                                    {
        //                                        xx = strListNew[drSalaryHeadCollection[ci]["SalaryHeadID"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();

        //                                    }
        //                                    catch (Exception)
        //                                    {

        //                                        xx = null;
        //                                    }
        //                                    if (xx != null)
        //                                    {

        //                                        if (drSalaryHeadCollection[ci]["HeadType"].ToString() == "D")
        //                                        {
        //                                            salarySheetValue = clsStaticInfo.dbl(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString()) * (-1);
        //                                            getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref subTotalDictSalaryProcess);//SubTotal
        //                                            getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                                        }
        //                                        else
        //                                        {
        //                                            if (drSalaryHeadCollection[ci]["HeadCategory"].ToString().ToUpper() == "OverTime".ToUpper())
        //                                            {
        //                                                OTAmountDeduct = clsStaticInfo.dbl(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString());
        //                                                totalOTHr = Math.Round((Convert.ToDouble(dtEmployees.Rows[i]["TotalOTHr"])) / 60, 2).ToString();//ru.cnDgt().ToString()), localLanguage);

        //                                                salarySheetValue = Convert.ToDouble(totalOTHr) * otRate;
        //                                                OTAmountAdd = salarySheetValue;
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref subTotalDictSalaryProcess);//SubTotal
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);

        //                                            }
        //                                            else if (drSalaryHeadCollection[ci]["HeadCategory"].ToString().ToUpper() == "Total Gross".ToUpper())
        //                                            {

        //                                                salarySheetValue = (clsStaticInfo.dbl(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString()) - OTAmountDeduct) + OTAmountAdd;
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref subTotalDictSalaryProcess);//SubTotal
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);

        //                                            }
        //                                            else if (drSalaryHeadCollection[ci]["HeadCategory"].ToString().ToUpper() == "CTC".ToUpper())
        //                                            {

        //                                                salarySheetValue = (clsStaticInfo.dbl(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString()) - OTAmountDeduct) + OTAmountAdd;
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref subTotalDictSalaryProcess);//SubTotal
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);

        //                                            }
        //                                            else
        //                                            {
        //                                                salarySheetValue = clsStaticInfo.dbl(drSalaryHeadCollection[ci]["DisbusmentAmount"].ToString());
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref subTotalDictSalaryProcess);//SubTotal
        //                                                getTotalAmount(xx.XLColIndex.ToString(), salarySheetValue, ref totalDictSalaryProcess);
        //                                            }

        //                                        }
        //                                        sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = ru.GetDecimalFormatlocal(xx, localLanguage);
        //                                        sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                        sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                        sheet1.Range[xlsRow, xx.XLColIndex].Number = salarySheetValue;
        //                                        sheet1.Range[xlsRow, xx.XLColIndex - 1, xlsRow, xx.XLColIndex].Merge();
        //                                        sheet1.Range[xlsRow, xx.XLColIndex - 1].CellStyle.Font.FontName = printFont;
        //                                        if (sheetBasedOn.ToUpper() == "structured".ToUpper())
        //                                        {
        //                                            sheet1.Range[xlsRow - 1, xx.XLColIndex - 1, xlsRow, xx.XLColIndex].CellStyle.Font.Size = 40;
        //                                        }
        //                                        else
        //                                        {
        //                                            sheet1.Range[xlsRow, xx.XLColIndex - 1, xlsRow, xx.XLColIndex - 1].CellStyle.Font.Size = 40;

        //                                        }
        //                                    }


        //                                }
        //                                catch (Exception ex)
        //                                {

        //                                    throw ex;
        //                                }

        //                                #region common excel Row set up                            

        //                                #endregion
        //                            }//for dtSalaryHead
        //                            var slrStartCol3 = slrStartCol;
        //                            for (int isl = 0; isl < strListNew.Count; isl++)
        //                            {
        //                                sheet1.Range[xlsRow, slrStartCol3 + isl, xlsRow, slrStartCol3 + isl + 1].Merge();
        //                                sheet1.Range[xlsRow, slrStartCol3 + isl, xlsRow, slrStartCol3 + isl + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                sheet1.Range[xlsRow, slrStartCol3 + isl, xlsRow, slrStartCol3 + isl + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow, slrStartCol3 + isl, xlsRow, slrStartCol3 + isl + 1].CellStyle.Font.FontName = printFont;
        //                                slrStartCol3++;
        //                            }
        //                        }

        //                        #endregion

        //                        if (sheetBasedOn == "structured")
        //                        {
        //                            if (withAttendance == false)
        //                            {

        //                                //empModulasFactor = 10;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].BorderAround(ExcelLineStyle.Thin);
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.FontName = printFont;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Size = 40;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].Merge();

        //                                sheet1.Range[xlsRow - 1, colSignature, xlsRow, colSignature].Merge();
        //                                sheet1.Range[xlsRow - 1, colSignature, xlsRow, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                                sheet1.Range[xlsRow - 1, 1, xlsRow, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                                sheet1.Range[xlsRow - 1, 1, xlsRow, colSignature].BorderInside(ExcelLineStyle.Thin);
        //                            }
        //                            else
        //                            {

        //                                //empModulasFactor = 5;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].BorderAround(ExcelLineStyle.Thin);
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.FontName = printFont;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Size = 40;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                                sheet1.Range[xlsRow - 1, colNetpayable, xlsRow, colNetpayable + 1].Merge();

        //                                sheet1.Range[xlsRow - 1, colSignature, particular3rdRow + 2, colSignature].Merge();
        //                                sheet1.Range[xlsRow - 1, colSignature, particular3rdRow + 2, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                                sheet1.Range[xlsRow - 1, 1, particular3rdRow + 2, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                                sheet1.Range[xlsRow - 1, 1, particular3rdRow + 2, colSignature].BorderInside(ExcelLineStyle.Thin);

        //                            }
        //                        }
        //                        else
        //                        {
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].BorderAround(ExcelLineStyle.Thin);
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Size = 40;
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                            sheet1.Range[xlsRow, colNetpayable, xlsRow, colNetpayable + 1].Merge();

        //                            sheet1.Range[xlsRow, colSignature, particular3rdRow + 3, colSignature].Merge();
        //                            sheet1.Range[xlsRow, colSignature, particular3rdRow + 3, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                            sheet1.Range[xlsRow, 1, particular3rdRow + 3, colSignature].BorderAround(ExcelLineStyle.Thin);
        //                            sheet1.Range[xlsRow, 1, particular3rdRow + 3, colSignature].BorderInside(ExcelLineStyle.Thin);
        //                        }

        //                        #region DailyAttendance
        //                        if (withAttendance == true)
        //                        {
        //                            //dvDaily = new DataView(dsDaily);
        //                            //dvDaily.RowFilter = "EmployeePK = '" + EmpIdPR + "' ";
        //                            var mnthColData = 0;
        //                            mnthColData = colParticulars;

        //                            //var dtFrmDtIntData = 1;
        //                            //var dtEndDateIntData = 31;

        //                            #region MyRegion
        //                            try
        //                            {
        //                                if (dicAttendance.ContainsKey(dtEmployees.Rows[i]["EmpSystemId"].ToString()))
        //                                {


        //                                    List<DataRow> drData = dicAttendance[dtEmployees.Rows[i]["EmpSystemId"].ToString()];

        //                                    foreach (DataRow item in drData)
        //                                    {
        //                                        try
        //                                        {

        //                                            sheet1[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].Text = item["DayStatus"].ToString();
        //                                            sheet1[particular3rdRow + 1, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].Text = item["InTime"].ToString();
        //                                            sheet1[particular3rdRow + 2, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].Text = item["OutTime"].ToString();
        //                                            sheet1[particular3rdRow + 3, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].Text = item["OTHr"].ToString();


        //                                            sheet1.Range[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), particular3rdRow + 3, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                                            sheet1.Range[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), particular3rdRow + 3, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                                            sheet1.Range[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), particular3rdRow + 3, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Font.FontName = "Arial Narrow";
        //                                            sheet1.Range[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), particular3rdRow + 3, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Font.Size = 17;

        //                                            sheet1.Range[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), particular3rdRow + 3, colSignature - 1].BorderAround(ExcelLineStyle.Thin);
        //                                            sheet1.Range[particular3rdRow + 0, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), particular3rdRow + 3, colSignature - 1].BorderInside(ExcelLineStyle.Thin);

        //                                        }
        //                                        catch
        //                                        {


        //                                        }
        //                                    }

        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            {

        //                                throw ex;
        //                            }
        //                            #endregion

        //                            xlsRow = particular3rdRow + 4;

        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;
        //                            xlsCol = np;
        //                            EmpCounter++;

        //                            if ((EmpCounter % empModulasFactor) == 0)
        //                            {
        //                                if (!string.IsNullOrEmpty(groupBy))
        //                                {
        //                                    if (i < dtEmployees.Rows.Count - 1)
        //                                    {
        //                                        if (strGroupBySel == dtEmployees.Rows[i + 1][groupBy + "ID"].ToString())
        //                                        {
        //                                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
        //                                            xlsRow++;
        //                                            sheet1[xlsRow, 1].RowHeight = 2;

        //                                            sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);

        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
        //                                    xlsRow++;
        //                                    sheet1[xlsRow, 1].RowHeight = 2;

        //                                    sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);

        //                                }
        //                            }
        //                            xlsRow--;

        //                        }
        //                        else
        //                        {
        //                            xlsRow++;
        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
        //                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;
        //                            xlsCol = np;
        //                            #region Border Setup
        //                            EmpCounter++;
        //                            if (withAttendance)
        //                            {
        //                                //if (sheetBasedOn == "structured")
        //                                //{

        //                                if ((EmpCounter % empModulasFactor) == 0)
        //                                {
        //                                    if (!string.IsNullOrEmpty(groupBy))
        //                                    {
        //                                        if (i < dtEmployees.Rows.Count - 1)
        //                                        {
        //                                            if (strGroupBySel == dtEmployees.Rows[i + 1][groupBy + "ID"].ToString())
        //                                            {
        //                                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
        //                                                xlsRow++;//Page Break Last Border was not visible. That's why We increase one extra row.
        //                                                sheet1[xlsRow, 1].RowHeight = 2;

        //                                                sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);

        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
        //                                        xlsRow++;
        //                                        sheet1[xlsRow, 1].RowHeight = 2;

        //                                        sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);

        //                                    }
        //                                }
        //                                //}


        //                            }
        //                            else
        //                            {

        //                                if ((EmpCounter % empModulasFactor) == 0)
        //                                {
        //                                    if (!string.IsNullOrEmpty(groupBy))
        //                                    {
        //                                        if (i < dtEmployees.Rows.Count - 1)
        //                                        {
        //                                            if (strGroupBySel == dtEmployees.Rows[i + 1][groupBy + "ID"].ToString())
        //                                            {
        //                                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
        //                                                xlsRow++;
        //                                                sheet1[xlsRow, 1].RowHeight = 2;
        //                                                sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);

        //                                            }

        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
        //                                        xlsRow++;
        //                                        sheet1[xlsRow, 1].RowHeight = 2;
        //                                        sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);

        //                                    }

        //                                    pageHeightDelemeter = 0;
        //                                    pageHeaderHeightDelemeter = 0;
        //                                    //empModulasFactor = 10;

        //                                    EmpCounter = 0;

        //                                }
        //                            }


        //                            #endregion
        //                            #endregion *************************Data End*************************
        //                            xlsRow--;

        //                        }
        //                        #endregion
        //                    }
        //                }
        //                catch (Exception ex)
        //                {

        //                    throw ex;
        //                }

        //                #region Summation of all Salary head

        //                sheet1.Range[xlsRow + 1, colParticulars].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Total.ToString(), "SubTotal");
        //                sheet1.Range[xlsRow + 1, colParticulars].NumberFormat = oRU.NumberFormatDecimalZero();
        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].Merge();
        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                sheet1.Range[xlsRow + 1, colParticulars].RowHeight = 45;
        //                pageHeightDelemeter = 45;
        //                groupWisePageHeightDelemeter += 45;

        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].CellStyle.Font.Size = 32;
        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                foreach (var item in subTotalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
        //                {
        //                    try
        //                    {
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].Number = Convert.ToDouble(item.Value);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].Merge();
        //                    }
        //                    catch (Exception exe)
        //                    {
        //                        throw exe;
        //                    }
        //                }//Loop End Last Summation in SalaryPorcess totalNetPayDisbusmentAmount
        //                sheet1.Range[xlsRow + 1, colNetpayable].Number = subTotalNetPayDisbusmentAmount;
        //                sheet1.Range[xlsRow + 1, colNetpayable].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);

        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].CellStyle.Font.Size = 32;
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].BorderInside(ExcelLineStyle.Thin);



        //                //subTotalDictSalaryStruct = null;
        //                //subTotalDictSalaryProcess = null;

        //                subTotalNetPayDisbusmentAmount = 0;

        //                xlsRow += 1;
        //                sheet1.Range[xlsRow + 1, colParticulars].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Total.ToString(), "Total");
        //                sheet1.Range[xlsRow + 1, colParticulars].NumberFormat = oRU.NumberFormatDecimalZero();
        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].Merge();
        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //                sheet1.Range[xlsRow + 1, colParticulars].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, colParticulars].RowHeight = 40;
        //                pageHeightDelemeter += 40;
        //                groupWisePageHeightDelemeter += 40;

        //                sheet1.Range[xlsRow + 1, 1, xlsRow + 1, colParticulars].CellStyle.Font.Size = 24;
        //                sheet1.Range[xlsRow + 1, colParticulars].CellStyle.Font.Bold = true;

        //                foreach (var item in totalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
        //                {
        //                    try
        //                    {
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].Number = Convert.ToDouble(item.Value);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
        //                        sheet1.Range[xlsRow + 1, Convert.ToInt32(item.Key) - 1, xlsRow + 1, Convert.ToInt32(item.Key)].Merge();

        //                    }
        //                    catch (Exception exe)
        //                    {
        //                        throw exe;
        //                    }
        //                }//Loop End Last Summation in SalaryPorcess totalNetPayDisbusmentAmount
        //                sheet1.Range[xlsRow + 1, colNetpayable].Number = totalNetPayDisbusmentAmount;
        //                sheet1.Range[xlsRow + 1, colNetpayable].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);

        //                sheet1.Range[xlsRow + 1, colNetpayable, xlsRow + 1, colNetpayable + 1].Merge();
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].BorderAround(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].BorderInside(ExcelLineStyle.Thin);
        //                sheet1.Range[xlsRow + 1, colParticulars, xlsRow + 1, colNetpayable + 1].CellStyle.Font.Size = 24;

        //                sheet1.Range[xlsRow + 20, ColName].Text = "Prepared By";
        //                sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, ColName].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                int numberOfColumns = colSignature - colParticulars;

        //                int remainCell = 0;
        //                //if (sheetBasedOn == "structured" && withAttendance == false)
        //                //{
        //                remainCell = numberOfColumns - 24;
        //                //}
        //                var unmargedCell = closestNumber(remainCell, 3) / 3;
        //                int firstColumn = ColWorkDaysInfo + 1;
        //                sheet1.Range[xlsRow + 20, firstColumn].Text = "Checked by";
        //                sheet1.Range[xlsRow + 20, firstColumn].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, firstColumn].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, firstColumn, xlsRow + 20, firstColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //                sheet1.Range[xlsRow + 20, firstColumn, xlsRow + 20, firstColumn + 7].Merge();

        //                int secondColumn = firstColumn + 7 + unmargedCell;
        //                sheet1.Range[xlsRow + 20, secondColumn].Text = "Head of HR";
        //                sheet1.Range[xlsRow + 20, secondColumn].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, secondColumn].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, secondColumn, xlsRow + 20, secondColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                sheet1.Range[xlsRow + 20, secondColumn, xlsRow + 20, secondColumn + 7].Merge();

        //                int thirdColumn = secondColumn + 7 + unmargedCell;
        //                sheet1.Range[xlsRow + 20, thirdColumn].Text = "Head of Accoounts";
        //                sheet1.Range[xlsRow + 20, thirdColumn].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, thirdColumn].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, thirdColumn, xlsRow + 20, thirdColumn + 7].Merge();
        //                sheet1.Range[xlsRow + 20, thirdColumn, xlsRow + 20, thirdColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                sheet1.Range[xlsRow + 20, colSignature - 1].Text = "Approved By";
        //                sheet1.Range[xlsRow + 20, colSignature - 1].CellStyle.Font.Size = 50;
        //                sheet1.Range[xlsRow + 20, colSignature - 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow + 20, colSignature - 1, xlsRow + 20, colSignature].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;


        //                sheet1.Range[xlsRow + 20, colSignature - 1, xlsRow + 20, colSignature].Merge();
        //                sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].RowHeight = 88;//Update
        //                sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                //sheet1.Range[xlsRow + 20, ColName].Text = "Prepared By";
        //                //sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Size = 50;
        //                //sheet1.Range[xlsRow + 20, ColName].CellStyle.Font.Bold = true;
        //                //sheet1.Range[xlsRow + 20, ColName, xlsRow + 20, ColName + 1].Merge();

        //                //sheet1.Range[xlsRow + 20, ColName, xlsRow + 20, ColName + 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                ////sheet1.Range[xlsRow + 8, ColLeaveInfo, xlsRow + 8, ColWorkDaysInfo].Borders(ExcelLineStyle.Thin);

        //                //int numberOfColumns = colSignature - colParticulars;

        //                //int remainCell = numberOfColumns - 24;
        //                //var unmargedCell = closestNumber(remainCell, 2) / 2;
        //                //int firstColumn = ColWorkDaysInfo + 3;
        //                //sheet1.Range[xlsRow + 20, firstColumn].Text = "Checked By";
        //                //sheet1.Range[xlsRow + 20, firstColumn].CellStyle.Font.Size = 50;
        //                //sheet1.Range[xlsRow + 20, firstColumn].CellStyle.Font.Bold = true;
        //                //sheet1.Range[xlsRow + 20, firstColumn, xlsRow + 20, firstColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //                //sheet1.Range[xlsRow + 20, firstColumn, xlsRow + 20, firstColumn + 7].Merge();

        //                //int secondColumn = firstColumn + 10 + unmargedCell;
        //                //sheet1.Range[xlsRow + 20, secondColumn].Text = "Departmental Head";
        //                //sheet1.Range[xlsRow + 20, secondColumn].CellStyle.Font.Size = 50;
        //                //sheet1.Range[xlsRow + 20, secondColumn].CellStyle.Font.Bold = true;
        //                //sheet1.Range[xlsRow + 20, secondColumn, xlsRow + 20, secondColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                //sheet1.Range[xlsRow + 20, secondColumn, xlsRow + 20, secondColumn + 7].Merge();

        //                ////int thirdColumn = secondColumn + 7 + unmargedCell;
        //                ////sheet1.Range[xlsRow + 20, thirdColumn].Text = "Accounts Manager";
        //                ////sheet1.Range[xlsRow + 20, thirdColumn].CellStyle.Font.Size = 50;
        //                ////sheet1.Range[xlsRow + 20, thirdColumn].CellStyle.Font.Bold = true;
        //                ////sheet1.Range[xlsRow + 20, thirdColumn, xlsRow + 20, thirdColumn + 7].Merge();
        //                ////sheet1.Range[xlsRow + 20, thirdColumn, xlsRow + 20, thirdColumn + 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

        //                //sheet1.Range[xlsRow + 20, colSignature - 2].Text = "Authorized By";
        //                //sheet1.Range[xlsRow + 20, colSignature - 2].CellStyle.Font.Size = 50;
        //                //sheet1.Range[xlsRow + 20, colSignature - 2].CellStyle.Font.Bold = true;
        //                //sheet1.Range[xlsRow + 20, colSignature - 2, xlsRow + 20, colSignature].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //                //sheet1.Range[xlsRow + 20, colSignature - 2, xlsRow + 20, colSignature].Merge();

        //                //sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].RowHeight = 153;
        //                //sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                //sheet1.Range[xlsRow + 20, 1, xlsRow + 20, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;



        //                #endregion
        //                #endregion ----------------------Data End-----------------------

        //                #region ******************Report Header******************
        //                objRpt.SelectedPlantWiseCompany(plantId, languageId, out dsCmp);
        //                xlsRow = 1;
        //                xlsCol = 1;

        //                FactoryName = string.Empty;

        //                var FactoryAddress = string.Empty;

        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyNameLocal"].ToString();
        //                }
        //                else
        //                {
        //                    CmpName = "";
        //                }
        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantNameLocal"].ToString();
        //                }
        //                else
        //                {
        //                    FactoryName = "";
        //                }
        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddressLocal"].ToString();
        //                    if (FactoryAddress == "")
        //                    {
        //                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();

        //                    }
        //                }
        //                else
        //                {
        //                    FactoryAddress = "";
        //                }
        //                sheet1.Range[xlsRow, 1].Text = CmpName + " ( " + FactoryName + " )";
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 40;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;

        //                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = printFont;

        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 50;

        //                xlsRow++;
        //                sheet1.Range[xlsRow, 1].Text = FactoryAddress;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 20;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 32;
        //                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

        //                sheet1.Range[xlsRow - 1, endXlsCol].Text = "Print Date: " + printDate + Environment.NewLine + "Payment Date:" + paymentDate;
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Font.Size = 24;
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Font.FontName = "ArialNarrow";
        //                sheet1.Range[xlsRow - 1, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

        //                string yearLocal = ru.cnDgt(Convert.ToDateTime(para.FromDate).Year.ToString(), localLanguage);

        //                xlsRow += 1;
        //                sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.SalarySheet.ToString(), "Pay Register") + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), localLanguage) + "-" + yearLocal; //_payRegisterLocal + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), "Bengali") + "," + yearLocal;
        //                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
        //                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 40;
        //                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
        //                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 50;
        //                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;



        //                #endregion ******************Report Header******************

        //                #region Freeze Panes
        //                sheet1.UsedRange["A7"].FreezePanes();
        //                sheet1.FirstVisibleColumn = 1;
        //                sheet1.FirstVisibleRow = 5;
        //                #endregion

        //                #region UsedRange Alignment
        //                sheet1.UsedRange.WrapText = true;
        //                //sheet1.UsedRange.is;
        //                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
        //                #endregion UsedRange Alignment


        //                #region Page Setup
        //                //sheet1.PageSetup.TopMargin = 0.2;
        //                //sheet1.PageSetup.BottomMargin = 0.7;
        //                //if (!string.IsNullOrEmpty(groupBy))
        //                //{
        //                //    sheet1.PageSetup.PrintTitleRows = "$A$1:$IV$" + titleRow;

        //                //}
        //                //else
        //                //{
        //                //    sheet1.PageSetup.PrintTitleRows = "$A$1:$IV$" + titleRow;

        //                //}
        //                sheet1.PageSetup.PrintTitleRows = "$1:$" + titleRow;

        //                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&15" + "Page " + "&p" + " of " + "&N";
        //                sheet1.PageSetup.LeftMargin = 0.3;
        //                sheet1.PageSetup.RightMargin = 0.2;
        //                sheet1.PageSetup.TopMargin = 0.2;
        //                sheet1.PageSetup.BottomMargin = 0.0;
        //                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
        //                sheet1.PageSetup.FitToPagesTall = 0;
        //                sheet1.PageSetup.FitToPagesWide = 1;

        //                if (paperSize == "Legal")
        //                {
        //                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperLegal;
        //                    //sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
        //                }
        //                if (paperSize == "A4")
        //                {
        //                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
        //                }


        //                sheet1.Name = "EmpPayRegister" + para.SalaryProcessId;
        //                #endregion

        //                return workbook;
        //            }

        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //            finally
        //            {
        //                objRpt = null;
        //                excelEngine = null;
        //                application = null;
        //                workbook = null;
        //                sheet1 = null;
        //            }
        //        }

        //        public void GetPayRegisgeterConfig(string companyId, string plantId, out bool ExcludeFatherName, out bool ExcludeNonpayable_Notional, out bool ExcludeTotalGross, out bool ExcludeCTC)
        //        {
        //            ExcludeFatherName = false;
        //            ExcludeNonpayable_Notional = false;
        //            ExcludeTotalGross = false;
        //            ExcludeCTC = false;
        //            try
        //            {

        //                DataTable dtCheckConfig = null;
        //                string sql = @"SELECT * FROM PayRegisterReportConfig where PlantId = '" + plantId + @"'";
        //                dtCheckConfig = _sqlRepository.GetDataTable(sql);
        //                for (int i = 0; i < dtCheckConfig.Rows.Count; i++)
        //                {
        //                    if (dtCheckConfig.Rows[i]["FieldName"].ToString().ToUpper().Trim() == PayRegisterCofigEnum.ExcludeFatherName.ToString().ToUpper())
        //                    {
        //                        ExcludeFatherName = bplib.clsWebLib.GetBoolData(dtCheckConfig.Rows[i]["Applicable"].ToString());
        //                    }
        //                    if (dtCheckConfig.Rows[i]["FieldName"].ToString().ToUpper().Trim() == PayRegisterCofigEnum.ExcludeNonPayableNotional.ToString().ToUpper())
        //                    {
        //                        ExcludeNonpayable_Notional = bplib.clsWebLib.GetBoolData(dtCheckConfig.Rows[i]["Applicable"].ToString());
        //                    }
        //                    if (dtCheckConfig.Rows[i]["FieldName"].ToString().ToUpper().Trim() == PayRegisterCofigEnum.ExcludeTotalGross.ToString().ToUpper())
        //                    {
        //                        ExcludeTotalGross = bplib.clsWebLib.GetBoolData(dtCheckConfig.Rows[i]["Applicable"].ToString());
        //                    }
        //                    if (dtCheckConfig.Rows[i]["FieldName"].ToString().ToUpper().Trim() == PayRegisterCofigEnum.ExcludeCTC.ToString().ToUpper())
        //                    {
        //                        ExcludeCTC = bplib.clsWebLib.GetBoolData(dtCheckConfig.Rows[i]["Applicable"].ToString());
        //                    }

        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }

        //        public void GetPayRegisgeterRowPerPage(string companyId, string plantId, out int StructreAndEarningExceptAttendance, out int EarningExceptAttendance, out int StructureAndEarningWithAttendance, out int EarningWithAttendance)
        //        {
        //            StructreAndEarningExceptAttendance = 9;
        //            EarningExceptAttendance = 9;
        //            StructureAndEarningWithAttendance = 6;
        //            EarningWithAttendance = 6;
        //            try
        //            {

        //                DataTable dtRowPerPage = null;
        //                string sql = @"SELECT * FROM PayRegisterRowPerPage where PlantId = '" + plantId + @"'";
        //                dtRowPerPage = _sqlRepository.GetDataTable(sql);
        //                for (int i = 0; i < dtRowPerPage.Rows.Count; i++)
        //                {
        //                    if (dtRowPerPage.Rows[i]["Setting"].ToString().ToUpper().Trim() == PayRegisterSettingsPerPage.StructreAndEarningExceptAttendance.ToString().ToUpper())
        //                    {
        //                        StructreAndEarningExceptAttendance = Convert.ToInt32(dtRowPerPage.Rows[i]["NumberOfRowsPerPage"].ToString());
        //                    }
        //                    if (dtRowPerPage.Rows[i]["Setting"].ToString().ToUpper().Trim() == PayRegisterSettingsPerPage.EarningExceptAttendance.ToString().ToUpper())
        //                    {
        //                        EarningExceptAttendance = Convert.ToInt32(dtRowPerPage.Rows[i]["NumberOfRowsPerPage"].ToString());
        //                    }
        //                    if (dtRowPerPage.Rows[i]["Setting"].ToString().ToUpper().Trim() == PayRegisterSettingsPerPage.StructureAndEarningWithAttendance.ToString().ToUpper())
        //                    {
        //                        StructureAndEarningWithAttendance = Convert.ToInt32(dtRowPerPage.Rows[i]["NumberOfRowsPerPage"].ToString());
        //                    }
        //                    if (dtRowPerPage.Rows[i]["Setting"].ToString().ToUpper().Trim() == PayRegisterSettingsPerPage.EarningWithAttendance.ToString().ToUpper())
        //                    {
        //                        EarningWithAttendance = Convert.ToInt32(dtRowPerPage.Rows[i]["NumberOfRowsPerPage"].ToString());
        //                    }

        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }

        //        public void GetEmployeePaySliptRdlcReport(string companyGroupId, string companyId, string plantId, string year, string month, string languageId, string paymentDate, string printDate, string fromDate, string toDate, string groupBy, string user, Dictionary<string, string> parameters)
        //        {

        //            string PdfLocation = string.Empty;
        //            try
        //            {


        //                clsReport objRpt = null;

        //                DataSet dsSlrProc = null;
        //                DataView dvSlrProc = null;
        //                DataSet dsHeading = null;
        //                DataSet dsCmp = null;
        //                DataSet dsFactory = null;

        //                objRpt = new clsReport();

        //                ParamList para = new ParamList();

        //                para.PlantId = plantId;
        //                para.FromDate = fromDate;
        //                para.ToDate = toDate;
        //                para.LanguageId = languageId;
        //                #region DataSet
        //                string sortingParameters = "";
        //                sortingParameters = objRpt.GetSortingParameters(para.CompanyGroupId, para.CompanyId, para.PlantId, groupBy);

        //                objRpt.GetSalaryInfoSlrProcIDWisePayGrpForReportNew(para, sortingParameters, parameters, out dsSlrProc);
        //                DataSet dsGrade = null;
        //                objRpt.GetGrade(para.EmployeeId, para.PayGroup, month, year, parameters, out dsGrade);
        //                dvSlrProc = new DataView();
        //                dvSlrProc.Table = dsSlrProc.Tables[0];

        //                objRpt.GetPlantWiseCompany(plantId, languageId, out dsCmp);

        //                objRpt.SelectedPlant(plantId, out dsFactory);

        //                #endregion DataSet

        //                string CompanyName = string.Empty;
        //                string CompanyAddress = string.Empty;
        //                if (dsCmp.Tables[0].Rows.Count > 0)
        //                {
        //                    CompanyName = dsCmp.Tables[0].Rows[0]["CompanyNameLocal"].ToString();
        //                    CompanyAddress = dsCmp.Tables[0].Rows[0]["CompanyAddressLocal"].ToString();
        //                }
        //                else
        //                {
        //                    CompanyAddress = "";
        //                    CompanyName = "";
        //                }

        //                ReportUtility oReportUtility = new ReportUtility();

        //                LocalReport localReport = new LocalReport();
        //                localReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/Payrolls/PayslipReport.rdlc");
        //                //localReport.ReportPath = Server.MapPath("/EmployeePaymentReport.rdlc");


        //                ReportDataSource reportDataSource = new ReportDataSource();
        //                reportDataSource.Name = "PayrollsDataSet";

        //                reportDataSource.Value = dsSlrProc.Tables[0];
        //                string TotalAmmountInWord = string.Empty;
        //                string PayslipName = string.Empty;
        //                switch (dsSlrProc.Tables[0].Rows[0]["LanguageName"].ToString())
        //                {
        //                    case "English":
        //                        PayslipName = month + "-" + year;
        //                        break;
        //                    case "Bengali":
        //                        PayslipName = bplib.clsWebLib.GetMonthNameBangla(month) + "-" + string.Concat(year);
        //                        break;


        //                    default:
        //                        PayslipName = month + "-" + year;
        //                        break;
        //                }

        //                ReportParameter[] parameter = new ReportParameter[]
        //                {
        //                    new ReportParameter("PayslipName", PayslipName),
        //                    new ReportParameter("CompanyAddress", CompanyAddress),
        //                    new ReportParameter("CompanyName", CompanyName)

        //                };
        //                localReport.SetParameters(parameter);
        //                //reportDataSource.Value = db.OnlineApplications.Where(x => x.StudentCode == "2019-Three-001").FirstOrDefault();

        //                localReport.DataSources.Add(reportDataSource);

        //                string ReportType = "pdf";
        //                string reportType = ReportType;
        //                String mimeType = string.Empty;
        //                String encoding = string.Empty;
        //                String extension = ReportType == "Excel" ? "xlsx" : "pdf";
        //                //String extension =  "png";
        //                Warning[] warnings = null;
        //                string[] streamids = null;
        //                Byte[] bytes = null;

        //                bytes = localReport.Render(reportType, "", out mimeType, out encoding, out extension, out streamids, out warnings);
        //                //string PDFPath = new DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("~/")) + "Reports\\PDF\\";
        //                string fileName = DateTime.Now.ToString("dd-MMM-yyyy") + "_" + user + "_SalaryPaySlipRdlc.pdf";
        //                //string fileName = "iDCard" + DateTime.Now.ToFileTime() + ".png";
        //                //bool IsExitsPDF = System.IO.File.Exists(PDFPath + fileName);
        //                string savepath = ResourcesPathReader.SavePdfDocUrl();
        //                //ShowLog(savepath); 

        //                //fileName = DateTime.Now.AddDays(-1).ToString("dd-MMM-yyyy") + "_" + (string)Session["USER"] + "_SalaryPaySlipRdlc.pdf";
        //                if (File.Exists(savepath + fileName))
        //                {
        //                    try
        //                    {
        //                        File.Delete(savepath + fileName);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        //Do something
        //                    }
        //                }



        //                FileStream fs = new FileStream(savepath + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        //                byte[] data = new byte[fs.Length];
        //                fs.Write(bytes, 0, bytes.Length);
        //                fs.Close();
        //                //var keyname = System.Configuration.ConfigurationManager.AppSettings["APP_NAME"];
        //                //PdfLocation =   keyname+"/PDF/" + fileName;
        //                //PdfLocation = "/Reports/PDF/" + fileName;

        //                //report.Attributes["src"] = PdfLocation;
        //                //ViewBag.ReportPath = PdfLocation;
        //                //string path = Server.MapPath("/Reports/PDF/");
        //                //string fileName = string.Empty;
        //                //fileName = "EmployeePayslipReport" + DateTime.Now.AddDays(-1).ToString("dd-MMM-yyyy")  + ".pdf"; ;

        //                string path = ResourcesPathReader.GetPdfDocUrl();

        //                //fileName = DateTime.Now.AddDays(-1).ToString("dd-MMM-yyyy") + "_" + (string)Session["USER"] + "_SalaryPaySlipRdlc.pdf";
        //                //if (File.Exists(path + fileName))
        //                //{
        //                //    try
        //                //    {
        //                //        File.Delete(path + fileName);
        //                //    }
        //                //    catch (Exception ex)
        //                //    {
        //                //        //Do something
        //                //    }
        //                //}
        //                //ShowLog(path.ToString() + "and" + savepath.ToString());


        //                //report.Attributes.Add("src", path + fileName);

        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;

        //            }




        //            //  var workbook = _employeeReportService.GetEmployeePayment(out string reportFileName, identity.CompanyId, identity.PlantName, voucherId);
        //            //return File(PdfLocation, "application/pdf");
        //            //return View("~/Areas/Accounts/Views/EmployeePaymentReport.cshtml");

        //        }

        //        private void FormatText(ref IWorksheet sheet1, ref IRichTextString rtf, string NewText, double FontSize)
        //        {
        //            IFont font = sheet1.Workbook.CreateFont();
        //            font.Color = ExcelKnownColors.Black;
        //            font.Size = FontSize;

        //            int oldPos = 0;
        //            if (rtf.Text.Length > 0)
        //                oldPos = rtf.Text.Length - 1;

        //            rtf.Append(NewText, font);
        //            rtf.SetFont(oldPos, (oldPos + NewText.Length) - 1, font);
        //        }
        //        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        //        {
        //            ColIndex = 0;
        //            sheet.Range[xlsRow + 1, xlsCol].Text = text;
        //            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 4;
        //            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
        //            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 7;
        //            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 7;
        //            ColIndex = xlsCol;
        //            xlsCol += 1;
        //        }

        //        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        //        {
        //            ColIndex = 0;
        //            sheet.Range[xlsRow + 1, xlsCol].Text = text;
        //            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
        //            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
        //            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 24;

        //            ColIndex = xlsCol;
        //            xlsCol += 1;
        //        }
        //        private void SetCellValueBangla(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width, string printFont, int rotationDegree)
        //        {
        //            ColIndex = 0;
        //            sheet.Range[xlsRow + 1, xlsCol].Text = text;
        //            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
        //            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = printFont;
        //            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Rotation = rotationDegree;
        //            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 24;

        //            ColIndex = xlsCol;
        //            xlsCol += 1;
        //        }
        //        private void SetCellValueBangla(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width, string printFont, int rotationDegree, double fontSize)
        //        {
        //            ColIndex = 0;
        //            sheet.Range[xlsRow + 1, xlsCol].Text = text;
        //            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
        //            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = printFont;
        //            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Rotation = rotationDegree;
        //            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = fontSize;
        //            ColIndex = xlsCol;
        //            xlsCol += 1;
        //        }

        //        private void SetCellValueRotate(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        //        {
        //            ColIndex = 0;
        //            sheet.Range[xlsRow + 1, xlsCol].Text = text;
        //            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
        //            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
        //            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 24;
        //            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Rotation = 90;
        //            ColIndex = xlsCol;
        //            xlsCol += 1;
        //        }

        //        private void CreateDynamicSHead(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequence> list)
        //        {
        //            try
        //            {
        //                list = new List<SalaryHeadSequence>();
        //                _total_head_count = 0;
        //                _count_earning_head = 0;
        //                _count_deducting_head = 0;
        //                _count_earning_ctchead = 0;
        //                int countGrossPostion = 0;
        //                string grossFormula = "";
        //                string deductionFormula = "";
        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region loop gross e
        //                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
        //                    {
        //                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        {
        //                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()))
        //                            {
        //                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
        //                                {
        //                                    _total_head_count++;
        //                                    countGrossPostion++;
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = "Arial Narrow";
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Size = 24;
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();

        //                                    if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                                    {
        //                                        sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                                    }
        //                                    xlsCol += 2;
        //                                    SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                                    salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                                    if (grossFormula.Length == 0)
        //                                    {
        //                                        grossFormula += salaryHeadSequence.XLColIndex.ToString();
        //                                    }
        //                                    else
        //                                    {
        //                                        grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                                    }
        //                                    countGrossPostion++;

        //                                    salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                                    salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                                    salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
        //                                    salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                                    salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                                    salaryHeadSequence.Sequence = ci;
        //                                    salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper();
        //                                    salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                                    //if (grossFormula.Length == 0)
        //                                    //{
        //                                    //    grossFormula += salaryHeadSequence.XLColIndex.ToString();
        //                                    //}
        //                                    //else
        //                                    //{
        //                                    //    grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                                    //}
        //                                    list.Add(salaryHeadSequence);

        //                                    _count_earning_head += 2;

        //                                }
        //                            }//IsGrossComponent
        //                        }//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                xlsCol += 1;
        //                countGrossPostion++;


        //                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].Text = "Gross";

        //                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = "Arial Narrow";
        //                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Size = 24;
        //                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();
        //                countGrossPostion++;
        //                _count_earning_head++;
        //                SalaryHeadSequence salaryHSGross = new SalaryHeadSequence();

        //                salaryHSGross.SalaryHead = grossFormula;
        //                salaryHSGross.SalaryHeadId = "Gross";
        //                salaryHSGross.XLColIndex = ColGrs + countGrossPostion;
        //                list.Add(salaryHSGross);

        //                int countCTCPosition = countGrossPostion;

        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region loop ctc
        //                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
        //                    {
        //                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        {
        //                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsCTCComponent"].ToString()) == true && bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()) == false)
        //                            {
        //                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
        //                                {
        //                                    _total_head_count++;
        //                                    countCTCPosition++;

        //                                    sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();

        //                                    sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.FontName = "Arial Narrow";
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Size = 24;
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;


        //                                    if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                                    {
        //                                        sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                                    }
        //                                    xlsCol += 2;
        //                                    SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                                    salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

        //                                    if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.FESTIVAL_BONUS)//FESTIVAL_BONUS
        //                                    {
        //                                        salaryHeadSequence.HeadCategory = bplib.clsWebLib.FESTIVAL_BONUS;
        //                                    }

        //                                    if (grossFormula.Length == 0)
        //                                    {
        //                                        grossFormula += salaryHeadSequence.XLColIndex.ToString();
        //                                    }
        //                                    else
        //                                    {
        //                                        grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                                    }
        //                                    countCTCPosition++;
        //                                    salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                                    salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                                    salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
        //                                    salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                                    salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                                    salaryHeadSequence.Sequence = ci;
        //                                    salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;



        //                                    list.Add(salaryHeadSequence);

        //                                    _count_earning_ctchead += 2;
        //                                }
        //                            }//IsCTCComponent
        //                        }//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                xlsCol += 1;

        //                countCTCPosition++;
        //                _count_earning_ctchead++;
        //                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].Text = "CTC";
        //                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Size = 24;
        //                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
        //                countCTCPosition++;

        //                SalaryHeadSequence salaryHSCTC = new SalaryHeadSequence();

        //                salaryHSCTC.SalaryHead = grossFormula;
        //                salaryHSCTC.SalaryHeadId = "CTC";
        //                salaryHSCTC.XLColIndex = ColGrs + countCTCPosition;
        //                list.Add(salaryHSCTC);

        //                int countDeductionPosition = countCTCPosition;

        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region deduction
        //                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
        //                    {
        //                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        {
        //                            if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
        //                            {
        //                                _total_head_count++;
        //                                countDeductionPosition++;

        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 24;
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = "Arial Narrow";
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;


        //                                if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                                {
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                                }
        //                                xlsCol += 2;
        //                                SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                                salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                                if (deductionFormula.Length == 0)
        //                                {
        //                                    deductionFormula += salaryHeadSequence.XLColIndex.ToString();
        //                                }
        //                                else
        //                                {
        //                                    deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                                }

        //                                countDeductionPosition++;

        //                                salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                                salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                                salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
        //                                salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                                salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                                salaryHeadSequence.Sequence = ci;
        //                                salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                                //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.PFHEADCATEGORY)//vpf
        //                                //{
        //                                salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
        //                                //}

        //                                list.Add(salaryHeadSequence);

        //                                _count_deducting_head += 2;
        //                            }
        //                        }//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                SalaryHeadSequence salaryHSDed = new SalaryHeadSequence();

        //                salaryHSDed.SalaryHead = deductionFormula;
        //                salaryHSDed.SalaryHeadId = "Deduction";
        //                salaryHSDed.XLColIndex = ColGrs + countDeductionPosition;
        //                list.Add(salaryHSDed);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //        /// <summary>
        //        /// Dynamic Salary Head Local Language
        //        /// </summary>
        //        /// <param name="dtSalaryHead"></param>
        //        /// <param name="_total_head_count"></param>
        //        /// <param name="sheet1"></param>
        //        /// <param name="xlsRow"></param>
        //        /// <param name="xlsCol"></param>
        //        /// <param name="ColGrs"></param>
        //        /// <param name="_count_earning_head"></param>
        //        /// <param name="_count_deducting_head"></param>
        //        /// <param name="_count_earning_ctchead"></param>
        //        /// <param name="list"></param>
        //        /// <param name="labelList"></param>
        //        /// <param name="printFont"></param>
        //        private void CreateDynamicSHeadLocalLanguage(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequence> list, Dictionary<string, string> labelList, string printFont)
        //        {
        //            try
        //            {
        //                var ru = new ReportUtility();

        //                list = new List<SalaryHeadSequence>();
        //                _total_head_count = 0;
        //                _count_earning_head = 0;
        //                _count_deducting_head = 0;
        //                _count_earning_ctchead = 0;
        //                int countGrossPostion = 0;
        //                string grossFormula = "";
        //                string deductionFormula = "";
        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region loop gross e
        //                    if (dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString().Trim().Length > 0)
        //                    {
        //                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        {
        //                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()))
        //                            {
        //                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
        //                                {
        //                                    _total_head_count++;
        //                                    countGrossPostion++;
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = printFont;
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Size = 24;
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();

        //                                    if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                                    {
        //                                        sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                                    }
        //                                    xlsCol += 2;
        //                                    SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                                    salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                                    if (grossFormula.Length == 0)
        //                                    {
        //                                        grossFormula += salaryHeadSequence.XLColIndex.ToString();
        //                                    }
        //                                    else
        //                                    {
        //                                        grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                                    }
        //                                    countGrossPostion++;

        //                                    salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                                    salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                                    salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                                    salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                                    salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                                    salaryHeadSequence.Sequence = ci;
        //                                    salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper();
        //                                    salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                                    list.Add(salaryHeadSequence);
        //                                    _count_earning_head += 2;
        //                                }
        //                            }//IsGrossComponent
        //                        }//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                xlsCol += 1;
        //                countGrossPostion++;


        //                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.TotalSalary.ToString(), "GROSS");

        //                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Size = 24;
        //                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();
        //                countGrossPostion++;
        //                _count_earning_head++;
        //                SalaryHeadSequence salaryHSGross = new SalaryHeadSequence();

        //                salaryHSGross.SalaryHead = grossFormula;
        //                salaryHSGross.SalaryHeadId = "Gross";
        //                salaryHSGross.XLColIndex = ColGrs + countGrossPostion;
        //                list.Add(salaryHSGross);

        //                int countCTCPosition = countGrossPostion;

        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region loop ctc
        //                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
        //                    {
        //                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        {
        //                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsCTCComponent"].ToString()) == true && bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()) == false)
        //                            {
        //                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
        //                                {
        //                                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().ToUpper() == "Total Gross".ToUpper())
        //                                    {

        //                                    }
        //                                    else
        //                                    {
        //                                        _total_head_count++;
        //                                        countCTCPosition++;

        //                                        sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();

        //                                        sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.FontName = printFont;
        //                                        sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Size = 24;
        //                                        sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
        //                                        sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;


        //                                        if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                                        {
        //                                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                                        }
        //                                        xlsCol += 2;
        //                                        SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                                        salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

        //                                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.FESTIVAL_BONUS)//FESTIVAL_BONUS
        //                                        {
        //                                            salaryHeadSequence.HeadCategory = bplib.clsWebLib.FESTIVAL_BONUS;
        //                                        }

        //                                        if (grossFormula.Length == 0)
        //                                        {
        //                                            grossFormula += salaryHeadSequence.XLColIndex.ToString();
        //                                        }
        //                                        else
        //                                        {
        //                                            grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                                        }
        //                                        countCTCPosition++;
        //                                        salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                                        salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                                        salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                                        salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                                        salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                                        salaryHeadSequence.Sequence = ci;
        //                                        salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;



        //                                        list.Add(salaryHeadSequence);

        //                                        _count_earning_ctchead += 2;
        //                                    }

        //                                }
        //                            }//IsCTCComponent
        //                        }//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                xlsCol += 1;

        //                countCTCPosition++;
        //                _count_earning_ctchead++;
        //                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.CTC.ToString(), "CTC");
        //                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Size = 24;
        //                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
        //                countCTCPosition++;

        //                SalaryHeadSequence salaryHSCTC = new SalaryHeadSequence();

        //                salaryHSCTC.SalaryHead = grossFormula;
        //                salaryHSCTC.SalaryHeadId = "CTC";
        //                salaryHSCTC.XLColIndex = ColGrs + countCTCPosition;
        //                list.Add(salaryHSCTC);

        //                int countDeductionPosition = countCTCPosition;

        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region deduction
        //                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
        //                    {
        //                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "TOTAL DEDUCTION" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        {
        //                            if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
        //                            {
        //                                _total_head_count++;
        //                                countDeductionPosition++;

        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 24;
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = printFont;
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;


        //                                if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                                {
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                                }
        //                                xlsCol += 2;
        //                                SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                                salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                                if (deductionFormula.Length == 0)
        //                                {
        //                                    deductionFormula += salaryHeadSequence.XLColIndex.ToString();
        //                                }
        //                                else
        //                                {
        //                                    deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                                }

        //                                countDeductionPosition++;

        //                                salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                                salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                                salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                                salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                                salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                                salaryHeadSequence.Sequence = ci;
        //                                salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                                //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.PFHEADCATEGORY)//vpf
        //                                //{
        //                                salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
        //                                //}

        //                                list.Add(salaryHeadSequence);

        //                                _count_deducting_head += 2;
        //                            }
        //                        }//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                SalaryHeadSequence salaryHSDed = new SalaryHeadSequence();

        //                salaryHSDed.SalaryHead = deductionFormula;
        //                salaryHSDed.SalaryHeadId = "Deduction";
        //                salaryHSDed.HeadType = "Deduction";
        //                salaryHSDed.XLColIndex = ColGrs + countDeductionPosition;
        //                list.Add(salaryHSDed);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //        /// <summary>
        //        /// for Pay register 
        //        /// </summary>
        //        /// <param name="dtSalaryHead"></param>
        //        /// <param name="_total_head_count"></param>
        //        /// <param name="sheet1"></param>
        //        /// <param name="xlsRow"></param>
        //        /// <param name="xlsCol"></param>
        //        /// <param name="ColGrs"></param>
        //        /// <param name="_count_earning_head"></param>
        //        /// <param name="_count_deducting_head"></param>
        //        /// <param name="_count_earning_ctchead"></param>
        //        /// <param name="list"></param>
        //        /// <param name="labelList"></param>
        //        /// <param name="printFont"></param>
        //        private void CreateDynamicSHeadLocalLanguageStruct(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequenceStructure> list, Dictionary<string, string> labelList, string printFont)
        //        {
        //            try
        //            {
        //                var ru = new ReportUtility();

        //                list = new List<SalaryHeadSequenceStructure>();
        //                _total_head_count = 0;
        //                _count_earning_head = 0;
        //                _count_deducting_head = 0;
        //                _count_earning_ctchead = 0;
        //                int countGrossPostion = 0;
        //                string grossFormula = "";
        //                string deductionFormula = "";
        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region loop gross e
        //                    if (dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString().Trim().Length > 0)
        //                    {
        //                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        {
        //                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()))
        //                            {
        //                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
        //                                {
        //                                    _total_head_count++;
        //                                    countGrossPostion++;
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = printFont;
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Size = 24;
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();

        //                                    if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                                    {
        //                                        sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                                    }
        //                                    xlsCol += 2;
        //                                    SalaryHeadSequenceStructure salaryHeadSequence = new SalaryHeadSequenceStructure();
        //                                    salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                                    if (grossFormula.Length == 0)
        //                                    {
        //                                        grossFormula += salaryHeadSequence.XLColIndex.ToString();
        //                                    }
        //                                    else
        //                                    {
        //                                        grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                                    }
        //                                    countGrossPostion++;

        //                                    salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                                    salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                                    salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                                    salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                                    salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                                    salaryHeadSequence.Sequence = ci;
        //                                    salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper();
        //                                    salaryHeadSequence.IsNetPayEffect = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsNetPayEffect"].ToString());
        //                                    salaryHeadSequence.IsCTCComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsCTCComponent"].ToString());

        //                                    salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                                    list.Add(salaryHeadSequence);
        //                                    _count_earning_head += 2;
        //                                }
        //                            }//IsGrossComponent
        //                        }//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                xlsCol += 1;
        //                countGrossPostion++;


        //                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.TotalSalary.ToString(), "GROSS");

        //                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = printFont;
        //                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Size = 24;
        //                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();
        //                countGrossPostion++;
        //                _count_earning_head++;
        //                SalaryHeadSequenceStructure salaryHSGross = new SalaryHeadSequenceStructure();

        //                salaryHSGross.SalaryHead = grossFormula;
        //                salaryHSGross.SalaryHeadId = "Gross";
        //                salaryHSGross.XLColIndex = ColGrs + countGrossPostion;
        //                list.Add(salaryHSGross);

        //                int countCTCPosition = countGrossPostion;

        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region loop ctc
        //                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
        //                    {
        //                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        {
        //                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsCTCComponent"].ToString()) == true && bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()) == false)
        //                            {
        //                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
        //                                {
        //                                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().ToUpper() == "Total Gross".ToUpper())
        //                                    {

        //                                    }
        //                                    else
        //                                    {
        //                                        _total_head_count++;
        //                                        countCTCPosition++;

        //                                        sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();

        //                                        sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.FontName = printFont;
        //                                        sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Size = 24;
        //                                        sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
        //                                        sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;


        //                                        if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                                        {
        //                                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                                        }
        //                                        xlsCol += 2;
        //                                        SalaryHeadSequenceStructure salaryHeadSequence = new SalaryHeadSequenceStructure();
        //                                        salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

        //                                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.FESTIVAL_BONUS)//FESTIVAL_BONUS
        //                                        {
        //                                            salaryHeadSequence.HeadCategory = bplib.clsWebLib.FESTIVAL_BONUS;
        //                                        }

        //                                        if (grossFormula.Length == 0)
        //                                        {
        //                                            grossFormula += salaryHeadSequence.XLColIndex.ToString();
        //                                        }
        //                                        else
        //                                        {
        //                                            grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                                        }
        //                                        countCTCPosition++;
        //                                        salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                                        salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                                        salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                                        salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                                        salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                                        salaryHeadSequence.Sequence = ci;
        //                                        salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

        //                                        salaryHeadSequence.IsNetPayEffect = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsNetPayEffect"].ToString());
        //                                        salaryHeadSequence.IsCTCComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsCTCComponent"].ToString());

        //                                        list.Add(salaryHeadSequence);

        //                                        _count_earning_ctchead += 2;
        //                                    }

        //                                }
        //                            }//IsCTCComponent
        //                        }//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                xlsCol += 1;

        //                countCTCPosition++;
        //                _count_earning_ctchead++;
        //                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.CTC.ToString(), "CTC");
        //                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Size = 24;
        //                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
        //                countCTCPosition++;

        //                SalaryHeadSequenceStructure salaryHSCTC = new SalaryHeadSequenceStructure();

        //                salaryHSCTC.SalaryHead = grossFormula;
        //                salaryHSCTC.SalaryHeadId = "CTC";
        //                salaryHSCTC.XLColIndex = ColGrs + countCTCPosition;
        //                list.Add(salaryHSCTC);

        //                int countDeductionPosition = countCTCPosition;

        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region deduction
        //                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
        //                    {
        //                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "TOTAL DEDUCTION" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        {
        //                            if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
        //                            {
        //                                _total_head_count++;
        //                                countDeductionPosition++;

        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 24;
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = printFont;
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;


        //                                if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                                {
        //                                    sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                                }
        //                                xlsCol += 2;
        //                                SalaryHeadSequenceStructure salaryHeadSequence = new SalaryHeadSequenceStructure();
        //                                salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                                if (deductionFormula.Length == 0)
        //                                {
        //                                    deductionFormula += salaryHeadSequence.XLColIndex.ToString();
        //                                }
        //                                else
        //                                {
        //                                    deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                                }

        //                                countDeductionPosition++;

        //                                salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                                salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                                salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                                salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                                salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                                salaryHeadSequence.IsNetPayEffect = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsNetPayEffect"].ToString());
        //                                salaryHeadSequence.IsCTCComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsCTCComponent"].ToString());
        //                                salaryHeadSequence.Sequence = ci;
        //                                salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                                //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.PFHEADCATEGORY)//vpf
        //                                //{
        //                                salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
        //                                //}

        //                                list.Add(salaryHeadSequence);

        //                                _count_deducting_head += 2;
        //                            }
        //                        }//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                SalaryHeadSequenceStructure salaryHSDed = new SalaryHeadSequenceStructure();

        //                salaryHSDed.SalaryHead = deductionFormula;
        //                salaryHSDed.SalaryHeadId = "Deduction";
        //                salaryHSDed.HeadType = "Deduction";
        //                salaryHSDed.XLColIndex = ColGrs + countDeductionPosition;
        //                list.Add(salaryHSDed);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //        /// <summary>
        //        /// Without attendance Jindal (India)s
        //        /// </summary>
        //        /// <param name="dtSalaryHead"></param>
        //        /// <param name="_total_head_count"></param>
        //        /// <param name="sheet1"></param>
        //        /// <param name="xlsRow"></param>
        //        /// <param name="xlsCol"></param>
        //        /// <param name="ColGrs"></param>
        //        /// <param name="_count_earning_head"></param>
        //        /// <param name="_count_deducting_head"></param>
        //        /// <param name="_count_earning_ctchead"></param>
        //        /// <param name="list"></param>
        //        /// <param name="labelList"></param>
        //        /// <param name="printFont"></param>
        //        private void CreateDynamicSHeadLocalLanguageStructNew(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequence> list, IEnumerable<LabelList> labelList, string printFont)
        //        {
        //            try
        //            {
        //                var ru = new ReportUtility();

        //                list = new List<SalaryHeadSequence>();
        //                _total_head_count = 0;
        //                _count_earning_head = 0;
        //                _count_deducting_head = 0;
        //                _count_earning_ctchead = 0;
        //                int countGrossPostion = 0;
        //                string grossFormula = "";
        //                string deductionFormula = "";
        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region loop gross e
        //                    if (dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString().Trim().Length > 0)
        //                    {
        //                        //bool isOkay = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "GROSS" ;
        //                        if ((dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && Convert.ToInt32(dtSalaryHead.Rows[ci]["PartOfNetPay"]) == 1) || dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "GROSS")
        //                        {
        //                            _total_head_count++;
        //                            countGrossPostion++;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Size = 35;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].ColumnWidth = 20;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion + 1].ColumnWidth = 22;

        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;


        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();

        //                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                            {
        //                                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                            }
        //                            xlsCol += 2;
        //                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                            salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                            if (grossFormula.Length == 0)
        //                            {
        //                                grossFormula += salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            else
        //                            {
        //                                grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            countGrossPostion++;

        //                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                            salaryHeadSequence.Sequence = ci;
        //                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper();


        //                            salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                            list.Add(salaryHeadSequence);
        //                            _count_earning_head += 2;
        //                        }
        //                        //}//IsGrossComponent
        //                        //}//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                xlsCol += 1;

        //                _count_earning_head++;


        //                int countCTCPosition = countGrossPostion;



        //                int countDeductionPosition = countCTCPosition;

        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region deduction
        //                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
        //                    {
        //                        //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "TOTAL DEDUCTION" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        //{
        //                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
        //                        {
        //                            _total_head_count++;
        //                            countDeductionPosition++;

        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 35;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].ColumnWidth = 15;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition + 1].ColumnWidth = 22;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;

        //                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                            {
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                            }
        //                            xlsCol += 2;
        //                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                            if (deductionFormula.Length == 0)
        //                            {
        //                                deductionFormula += salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            else
        //                            {
        //                                deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                            }

        //                            countDeductionPosition++;

        //                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                            salaryHeadSequence.Sequence = ci;
        //                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                            //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.PFHEADCATEGORY)//vpf
        //                            //{
        //                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
        //                            //}

        //                            list.Add(salaryHeadSequence);

        //                            _count_deducting_head += 2;
        //                        }
        //                        //}//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for

        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }

        //        private void CreateDynamicSHeadLocalLanguageStructNew(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out Dictionary<string, SalaryHeadSequence> list, IEnumerable<LabelList> labelList, string printFont)
        //        {
        //            try
        //            {
        //                var ru = new ReportUtility();
        //                list = new Dictionary<string, SalaryHeadSequence>();

        //                _total_head_count = 0;
        //                _count_earning_head = 0;
        //                _count_deducting_head = 0;
        //                _count_earning_ctchead = 0;
        //                int countGrossPostion = 0;
        //                string grossFormula = "";
        //                string deductionFormula = "";
        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region loop gross e
        //                    if (dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString().Trim().Length > 0)
        //                    {
        //                        //bool isOkay = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "GROSS" ;
        //                        if ((dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && Convert.ToInt32(dtSalaryHead.Rows[ci]["PartOfNetPay"]) == 1) || dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "GROSS")
        //                        {
        //                            _total_head_count++;
        //                            countGrossPostion++;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Size = 35;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].ColumnWidth = 20;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion + 1].ColumnWidth = 22;

        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;


        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();

        //                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                            {
        //                                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                            }
        //                            xlsCol += 2;
        //                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                            salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                            if (grossFormula.Length == 0)
        //                            {
        //                                grossFormula += salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            else
        //                            {
        //                                grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            countGrossPostion++;

        //                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                            salaryHeadSequence.Sequence = ci;
        //                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper();


        //                            salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);

        //                            _count_earning_head += 2;
        //                        }
        //                        //}//IsGrossComponent
        //                        //}//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                xlsCol += 1;

        //                _count_earning_head++;


        //                int countCTCPosition = countGrossPostion;



        //                int countDeductionPosition = countCTCPosition;

        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region deduction
        //                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
        //                    {
        //                        //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "TOTAL DEDUCTION" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        //{
        //                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
        //                        {
        //                            _total_head_count++;
        //                            countDeductionPosition++;

        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 35;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].ColumnWidth = 15;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition + 1].ColumnWidth = 22;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;

        //                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                            {
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                            }
        //                            xlsCol += 2;
        //                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                            if (deductionFormula.Length == 0)
        //                            {
        //                                deductionFormula += salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            else
        //                            {
        //                                deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                            }

        //                            countDeductionPosition++;

        //                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                            salaryHeadSequence.Sequence = ci;
        //                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                            //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.PFHEADCATEGORY)//vpf
        //                            //{
        //                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
        //                            //}

        //                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);


        //                            _count_deducting_head += 2;
        //                        }
        //                        //}//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for

        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }

        //        private void XCreateDynamicSHeadLocalLanguageStructNew(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequence> list, IEnumerable<LabelList> labelList, string printFont)
        //        {
        //            try
        //            {
        //                var ru = new ReportUtility();

        //                list = new List<SalaryHeadSequence>();
        //                _total_head_count = 0;
        //                _count_earning_head = 0;
        //                _count_deducting_head = 0;
        //                _count_earning_ctchead = 0;
        //                int countGrossPostion = 0;
        //                string grossFormula = "";
        //                string deductionFormula = "";
        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region loop gross e
        //                    if (dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString().Trim().Length > 0)
        //                    {
        //                        //bool isOkay = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "GROSS" ;
        //                        if ((dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && Convert.ToInt32(dtSalaryHead.Rows[ci]["PartOfNetPay"]) == 1) || dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "GROSS")
        //                        {
        //                            _total_head_count++;
        //                            countGrossPostion++;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Size = 15;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].ColumnWidth = 15;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion + 1].ColumnWidth = 22;

        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;


        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();

        //                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                            {
        //                                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                            }
        //                            xlsCol += 2;
        //                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                            salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                            if (grossFormula.Length == 0)
        //                            {
        //                                grossFormula += salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            else
        //                            {
        //                                grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            countGrossPostion++;

        //                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                            salaryHeadSequence.Sequence = ci;
        //                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper();


        //                            salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                            list.Add(salaryHeadSequence);
        //                            _count_earning_head += 1;
        //                        }
        //                        //}//IsGrossComponent
        //                        //}//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                xlsCol += 1;

        //                _count_earning_head++;


        //                int countCTCPosition = countGrossPostion;



        //                int countDeductionPosition = countCTCPosition;

        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region deduction
        //                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
        //                    {
        //                        //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "TOTAL DEDUCTION" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        //{
        //                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
        //                        {
        //                            _total_head_count++;
        //                            countDeductionPosition++;

        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 15;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].ColumnWidth = 15;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition + 1].ColumnWidth = 22;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;

        //                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                            {
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                            }
        //                            xlsCol += 2;
        //                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                            if (deductionFormula.Length == 0)
        //                            {
        //                                deductionFormula += salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            else
        //                            {
        //                                deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                            }

        //                            countDeductionPosition++;

        //                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                            salaryHeadSequence.Sequence = ci;
        //                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                            //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.PFHEADCATEGORY)//vpf
        //                            //{
        //                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
        //                            //}

        //                            list.Add(salaryHeadSequence);

        //                            _count_deducting_head += 1;
        //                        }
        //                        //}//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for

        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //        /// <summary>
        //        /// with Attendance bangladesh
        //        /// </summary>
        //        /// <param name="dtSalaryHead"></param>
        //        /// <param name="_total_head_count"></param>
        //        /// <param name="sheet1"></param>
        //        /// <param name="xlsRow"></param>
        //        /// <param name="xlsCol"></param>
        //        /// <param name="ColGrs"></param>
        //        /// <param name="_count_earning_head"></param>
        //        /// <param name="_count_deducting_head"></param>
        //        /// <param name="_count_earning_ctchead"></param>
        //        /// <param name="list"></param>
        //        /// <param name="labelList"></param>
        //        /// <param name="printFont"></param>
        //        private void CreateDynamicSHeadLocalLanguageStructNewWithAttendance(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequence> list, Dictionary<string, string> labelList, string printFont)
        //        {
        //            try
        //            {
        //                var ru = new ReportUtility();

        //                list = new List<SalaryHeadSequence>();
        //                _total_head_count = 0;
        //                _count_earning_head = 0;
        //                _count_deducting_head = 0;
        //                _count_earning_ctchead = 0;
        //                int countGrossPostion = 0;
        //                string grossFormula = "";
        //                string deductionFormula = "";
        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region loop gross e
        //                    if (dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString().Trim().Length > 0)
        //                    {
        //                        //bool isOkay = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "GROSS" ;
        //                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "Net Payable".ToUpper() && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "TOTAL GROSS".ToUpper())
        //                        {
        //                            _total_head_count++;
        //                            countGrossPostion++;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Size = 35;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].ColumnWidth = 22;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion + 1].ColumnWidth = 22;

        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;


        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();

        //                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                            {
        //                                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                            }
        //                            xlsCol += 2;
        //                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                            salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                            if (grossFormula.Length == 0)
        //                            {
        //                                grossFormula += salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            else
        //                            {
        //                                grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            countGrossPostion++;

        //                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                            salaryHeadSequence.Sequence = ci;
        //                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper();
        //                            salaryHeadSequence.IsGrossComponent = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString());


        //                            salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                            //list.Add(salaryHeadSequence);
        //                            list.Add(salaryHeadSequence);

        //                            _count_earning_head += 2;
        //                        }
        //                        //}//IsGrossComponent
        //                        //}//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                xlsCol += 1;

        //                _count_earning_head++;


        //                int countCTCPosition = countGrossPostion;



        //                int countDeductionPosition = countCTCPosition;

        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region deduction
        //                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
        //                    {
        //                        //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "TOTAL DEDUCTION" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        //{
        //                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
        //                        {
        //                            _total_head_count++;
        //                            countDeductionPosition++;

        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 35;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].ColumnWidth = 22;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition + 1].ColumnWidth = 22;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;

        //                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                            {
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                            }
        //                            xlsCol += 2;
        //                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                            if (deductionFormula.Length == 0)
        //                            {
        //                                deductionFormula += salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            else
        //                            {
        //                                deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                            }

        //                            countDeductionPosition++;

        //                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                            salaryHeadSequence.Sequence = ci;
        //                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                            salaryHeadSequence.IsGrossComponent = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString());
        //                            //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.PFHEADCATEGORY)//vpf
        //                            //{
        //                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
        //                            //}

        //                            //list.Add(salaryHeadSequence);
        //                            list.Add(salaryHeadSequence);

        //                            _count_deducting_head += 2;
        //                        }
        //                        //}//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for

        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }

        //        private void CreateDynamicSHeadLocalLanguageStructNewWithAttendance(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out Dictionary<string, SalaryHeadSequence> list, Dictionary<string, string> labelList, string printFont, bool ExcludeNonpayable_Notional, bool ExcludeTotalGross, bool ExcludeCTC)
        //        {
        //            try
        //            {
        //                var ru = new ReportUtility();

        //                list = new Dictionary<string, SalaryHeadSequence>();
        //                _total_head_count = 0;
        //                _count_earning_head = 0;
        //                _count_deducting_head = 0;
        //                _count_earning_ctchead = 0;
        //                int countGrossPostion = 0;
        //                string grossFormula = "";
        //                string deductionFormula = "";


        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region loop gross e
        //                    if (dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString().Trim().Length > 0)
        //                    {

        //                        // if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "Net Payable".ToUpper() && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "TOTAL GROSS".ToUpper())
        //                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "Net Payable".ToUpper())
        //                        {

        //                            //if (ExcludeNonpayable_Notional == true && (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["PartOfNetPay"].ToString()) == false))
        //                            //    continue;
        //                            if (ExcludeNonpayable_Notional == true && (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["PartOfNetPay"].ToString()) == false))
        //                            {
        //                                if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "GROSS")
        //                                {

        //                                }

        //                                else if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "TOTAL GROSS")
        //                                {
        //                                    if (ExcludeTotalGross == true && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "TOTAL GROSS".ToUpper())
        //                                        continue;
        //                                    else
        //                                    {

        //                                    }
        //                                }

        //                                else if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "CTC")
        //                                {
        //                                    if (ExcludeCTC == true && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "CTC".ToUpper())
        //                                        continue;
        //                                    else
        //                                    {

        //                                    }
        //                                }
        //                                else
        //                                    continue;




        //                            }
        //                            else
        //                            {
        //                                if (ExcludeTotalGross == true && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "TOTAL GROSS".ToUpper())
        //                                    continue;
        //                                if (ExcludeCTC == true && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "CTC".ToUpper())
        //                                    continue;
        //                            }



        //                            _total_head_count++;
        //                            countGrossPostion++;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Size = 35;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].ColumnWidth = 22;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion + 1].ColumnWidth = 22;

        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;


        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();

        //                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                            {
        //                                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                            }
        //                            xlsCol += 2;
        //                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                            salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                            if (grossFormula.Length == 0)
        //                            {
        //                                grossFormula += salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            else
        //                            {
        //                                grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            countGrossPostion++;

        //                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                            salaryHeadSequence.Sequence = ci;
        //                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper();
        //                            salaryHeadSequence.IsGrossComponent = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString());


        //                            salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                            //list.Add(salaryHeadSequence);
        //                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);

        //                            _count_earning_head += 2;
        //                        }
        //                        //}//IsGrossComponent
        //                        //}//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                xlsCol += 1;

        //                _count_earning_head++;


        //                int countCTCPosition = countGrossPostion;



        //                int countDeductionPosition = countCTCPosition;

        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region deduction
        //                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
        //                    {
        //                        //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "TOTAL DEDUCTION" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        //{
        //                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
        //                        {
        //                            _total_head_count++;
        //                            countDeductionPosition++;

        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 35;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].ColumnWidth = 22;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition + 1].ColumnWidth = 22;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;

        //                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                            {
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                            }
        //                            xlsCol += 2;
        //                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                            if (deductionFormula.Length == 0)
        //                            {
        //                                deductionFormula += salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            else
        //                            {
        //                                deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                            }

        //                            countDeductionPosition++;

        //                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                            salaryHeadSequence.Sequence = ci;
        //                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                            salaryHeadSequence.IsGrossComponent = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString());
        //                            //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.PFHEADCATEGORY)//vpf
        //                            //{
        //                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
        //                            //}

        //                            //list.Add(salaryHeadSequence);
        //                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);

        //                            _count_deducting_head += 2;
        //                        }
        //                        //}//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for

        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //        private void CreateDynamicSHeadLocalLanguageStructNewWithAttendance(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out Dictionary<string, SalaryHeadSequence> list, IEnumerable<LabelList> labelList, string printFont)
        //        {
        //            try
        //            {
        //                var ru = new ReportUtility();

        //                list = new Dictionary<string, SalaryHeadSequence>();
        //                _total_head_count = 0;
        //                _count_earning_head = 0;
        //                _count_deducting_head = 0;
        //                _count_earning_ctchead = 0;
        //                int countGrossPostion = 0;
        //                string grossFormula = "";
        //                string deductionFormula = "";
        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region loop gross e
        //                    if (dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString().Trim().Length > 0)
        //                    {
        //                        //bool isOkay = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "GROSS" ;
        //                        //if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" )
        //                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "Net Payable".ToUpper() && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "TOTAL GROSS".ToUpper())
        //                        {
        //                            _total_head_count++;
        //                            countGrossPostion++;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Size = 35;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].ColumnWidth = 22;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion + 1].ColumnWidth = 22;

        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;


        //                            sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();

        //                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                            {
        //                                sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                            }
        //                            xlsCol += 2;
        //                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                            salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                            if (grossFormula.Length == 0)
        //                            {
        //                                grossFormula += salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            else
        //                            {
        //                                grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            countGrossPostion++;

        //                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                            salaryHeadSequence.Sequence = ci;
        //                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper();
        //                            salaryHeadSequence.IsGrossComponent = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString());


        //                            salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
        //                            //list.Add(salaryHeadSequence);
        //                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);

        //                            _count_earning_head += 2;
        //                        }
        //                        //}//IsGrossComponent
        //                        //}//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for
        //                xlsCol += 1;

        //                _count_earning_head++;


        //                int countCTCPosition = countGrossPostion;



        //                int countDeductionPosition = countCTCPosition;

        //                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
        //                {
        //                    #region deduction
        //                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
        //                    {
        //                        //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "TOTAL DEDUCTION" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
        //                        //{
        //                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
        //                        {
        //                            _total_head_count++;
        //                            countDeductionPosition++;

        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 35;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = printFont;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].ColumnWidth = 22;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition + 1].ColumnWidth = 22;
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
        //                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;

        //                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
        //                            {
        //                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
        //                            }
        //                            xlsCol += 2;
        //                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
        //                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                            if (deductionFormula.Length == 0)
        //                            {
        //                                deductionFormula += salaryHeadSequence.XLColIndex.ToString();
        //                            }
        //                            else
        //                            {
        //                                deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
        //                            }

        //                            countDeductionPosition++;

        //                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
        //                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
        //                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHeadBangla"].ToString();
        //                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
        //                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
        //                            salaryHeadSequence.Sequence = ci;
        //                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
        //                            salaryHeadSequence.IsGrossComponent = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString());
        //                            //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.PFHEADCATEGORY)//vpf
        //                            //{
        //                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
        //                            //}

        //                            //list.Add(salaryHeadSequence);
        //                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);

        //                            _count_deducting_head += 2;
        //                        }
        //                        //}//CTC/Gross
        //                    }//SalaryHead 
        //                    #endregion
        //                }//for

        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //        private decimal GetLeaveEmp(DataView dvEmpLeaveInfo, string leaveCode)
        //        {
        //            var basicValue = 0.00m;
        //            try
        //            {

        //                var basic = from r in dvEmpLeaveInfo.ToTable().AsEnumerable()
        //                            where r.Field<string>("code") == leaveCode
        //                            select r;
        //                if (basic.Count() > 0)
        //                {
        //                    DataTable dtt = basic.CopyToDataTable();
        //                    basicValue = Convert.ToDecimal(dtt.Rows[0]["AvailedLeave"].ToString());
        //                }
        //                return basicValue;
        //            }
        //            catch (Exception)
        //            {
        //                throw;
        //            }
        //        }

        //        private string GetLeaveType(DataView dvLeaveType, string leaveCode)
        //        {
        //            var localLeaveType = string.Empty;
        //            try
        //            {

        //                var basic = from r in dvLeaveType.ToTable().AsEnumerable()
        //                            where r.Field<string>("code") == leaveCode
        //                            select r;
        //                if (basic.Count() > 0)
        //                {
        //                    DataTable dtt = basic.CopyToDataTable();
        //                    localLeaveType = dtt.Rows[0]["lName"].ToString();
        //                }
        //                return localLeaveType;
        //            }
        //            catch (Exception)
        //            {
        //                throw;
        //            }
        //        }


        //        private decimal GetLWPEmp(DataView dvEmpLeaveInfo, string LeaveType)
        //        {
        //            var basicValue = 0.00m;
        //            try
        //            {

        //                var basic = from r in dvEmpLeaveInfo.ToTable().AsEnumerable()
        //                            where r.Field<string>("LeaveType") == LeaveType
        //                            select r;
        //                if (basic.Count() > 0)
        //                {

        //                    DataTable dtt = basic.CopyToDataTable();
        //                    basicValue = Convert.ToDecimal(dtt.Rows[0]["AvailedLeave"].ToString());


        //                }
        //                return basicValue;
        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }
        //        }

        //        private void getTotal(ref IWorksheet sheet1, int xlsRow, int xlsCol, int Row_Total_Start, int Row_Total_end, ReportUtility ru)
        //        {
        //            try
        //            {

        //                sheet1.Range[xlsRow, xlsCol].Formula = "=SUM(" + ru.GetColumnNameForXls(xlsCol) + Row_Total_Start + ":" + ru.GetColumnNameForXls(xlsCol) + (Row_Total_end) + ")";
        //                sheet1.Range[xlsRow, xlsCol].NumberFormat = ru.NumberFormatDecimalFour();
        //                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
        //                sheet1.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);

        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }
        //        }
        //        private void getFormulaValue(int startValue, int lastValue, List<SalaryHeadSequence> list, out string structureCell, out string salaryCell)
        //        {
        //            try
        //            {
        //                ReportUtility ru = new ReportUtility();
        //                structureCell = string.Empty;
        //                salaryCell = string.Empty;
        //                for (int i = 0; i < list.Count; i++)
        //                {
        //                    //var cCount = lastValue - startValue;
        //                    for (int c = startValue; c < lastValue; c += 2)
        //                    {
        //                        structureCell = ru.GetColumnNameForXls(list[i].XLColIndex) + c;
        //                        salaryCell = ru.GetColumnNameForXls(list[i].XLColIndex) + c + 1;
        //                    }
        //                }
        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }
        //        }
        //        private void getTotalAmount(string colIndex, double Amount, ref Dictionary<string, double> dict)
        //        {
        //            try
        //            {
        //                if (dict.ContainsKey(colIndex))//If has Same head
        //                {
        //                    var value = dict[colIndex];
        //                    double totalAmount = Convert.ToDouble(Amount) + Convert.ToDouble(value);
        //                    dict[colIndex] = totalAmount;

        //                }
        //                else // If New Head
        //                {
        //                    dict.Add(colIndex, Amount);
        //                }

        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }
        //        }
        //        string GetDecimalFormat(SalaryHeadSequence shs)
        //        {
        //            try
        //            {
        //                var ob = new ReportUtility();
        //                if (shs.IsInt)
        //                {
        //                    return ob.NumberFormatInt();
        //                }
        //                else
        //                {
        //                    return ob.GetDynamicDecimalPlace(shs.DecimalNo);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //        public class PayRegisterParamList : BaseModel
        //        {

        //            public string EmployeeId { get; set; }
        //            public string payGroup { get; set; }
        //            public string userId { get; set; }
        //            public string Month { get; set; }
        //            public string Year { get; set; }
        //            public string PlantId { get; set; }
        //            public string UnitId { get; set; }
        //            public string DivisionId { get; set; }
        //            public string DepartmentId { get; set; }
        //            public string SectionId { get; set; }
        //            public string SubSectionId { get; set; }
        //            public string LineId { get; set; }
        //            public string SubSecStrucId { get; set; }
        //            public string EmpCategoryId { get; set; }
        //            public string DesignationGroupId { get; set; }
        //            public string DesignationId { get; set; }
        //            public string FromDate { get; set; }
        //            public string EmpStatus { get; set; }
        //            public string SalaryProcessId { get; set; }
        //            public string CompanyGroupId { get; set; }
        //            public string CompanyId { get; set; }
        //            public string ToDate { get; set; }
        //            public string PayGroup { get; set; }
        //            public string SystemID { get; set; }
        //            public string PaymentMode { get; set; }
        //            public string LanguageId { get; set; }
        //            public string SystemAdmin { get; set; }
        //            public string ControlAdmin { get; set; }
        //        }
        //        public IEnumerable<ComboModel> GetSalaryprocessIdCbo(string compnayGroupId, string companyId, string plantId, string MonthNo, string YearNo, string IsCompleteMonth)
        //        {
        //            var plant = string.Empty;
        //            var strSQL = string.Empty;
        //            if (plantId == null)
        //            {
        //                plant = "";
        //                strSQL = @"SELECT * FROM SalaryProcMaster
        //                                    WHERE MonthNo = '" + MonthNo + @"' AND YearNo = '" + YearNo + @"' and SystemID IN (select SlrProcMstSystemID  from SalaryProcChild ) --AND IsCompleteMonth = " + IsCompleteMonth + @"
        //                            --GROUP BY SalaryProcID";
        //            }
        //            else
        //            {

        //                strSQL = @"SELECT * FROM SalaryProcMaster
        //                                      WHERE SystemID IN (SELECT SlrProcMstSystemID FROM SalaryProcChild
        //                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
        //                                        AND MonthNo = '" + MonthNo + @"' AND YearNo = '" + YearNo + @"' --AND IsCompleteMonth = " + IsCompleteMonth + @"
        //                                      --GROUP BY SalaryProcID";
        //            }

        //            return _sqlRepository.GetCombo(strSQL, "SystemId", "Description");
        //        }


        //        public IEnumerable<ComboModel> GetPayGroupCbo(bool sa, bool ca, string userId)
        //        {

        //            try
        //            {
        //                var plant = string.Empty;
        //                var strSQL = string.Empty;
        //                if (ca == true || sa == true)
        //                {
        //                    strSQL = @"SELECT * FROM (
        //			          SELECT 'ALL' AS Id,'ALL' AS UserName ,-1 AS Sequence
        //			             UNION ALL
        //						select distinct   ISNULL(PG.Id,'NOGROUP') Id, ISNULL(PG.UserName,'No Group') UserName ,PG.Sequence 
        //							 from EmployeeInformation EEI left join
        //							  MST.PayrollGroupMaster  PGM  ON PGM.EmployeeId = EEI.SystemId  left join
        //							  HKP.PayrollGroup PG  ON PGM.PayrollGroupId = PG.Id  ) AS K
        //							ORDER BY sequence";
        //                }
        //                else
        //                {
        //                    strSQL = @"SELECT HPG.Id, HPG.UserName 
        //                            FROM HKP.PayrollGroup HPG
        //                            WHERE Id IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup WHERE UserId = '" + userId + @"') ORDER BY HPG.Sequence";
        //                }

        //                return _sqlRepository.GetCombo(strSQL, "Id", "UserName");
        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }


        //        }
        //        private DataTable GetMonthlyDailyAttendance(string attdnType, ParaMontlyAttendance objm)
        //        {
        //            try
        //            {

        //                var strSql = @"DECLARE @sql_ nvarchar(max)

        //                                    select  EmployeePK,WorkDate
        //                                    ,ISNULL(ISNULL(DayStatus,'')+ 
        //								     ','+ISNULL(InTime,'')+','+ISNULL(OutTime,'')+','+ISNULL(OTHr,''),  '')  DayStatus

        //                                    INTO #tempOT
        //                                    from 
        //                                    (
        //                                    SELECT A.* FROM
        //	                                (SELECT E.systemId EmployeePK,E.EmployeeCode, E.EmployeeName, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ,
        //                                            D.UserName Designation, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
        //                                            S.UserName Section, SB.UserName SubSection, L.UserName Line,  REPLACE(CONVERT(VARCHAR(11), AD.WorkDate, 113), ' ', '-') PDate,
        //                                            AD.DayStatus, FORMAT(InTime, 'hh.mm tt') InTime, ARIN.DeviceID InDeviceID, FORMAT(OutTime, 'hh.mm tt') OutTime,
        //                                            AROUT.DeviceID OutDeviceID,CONVERT(VARCHAR(10),CONVERT(DECIMAL(18,2),FinalOT.TotalOTHr/60), 108) OTHr, LT.UserName LvShortName
        //											,AD.WorkDate, DD.UserName GivenDesignation
        //                                    FROM dbo.EmployeeInformation E
        //                                                INNER JOIN dbo.AttdnProcessData AD ON E.SystemID = AD.EmpSystemID
        //                                                LEFT JOIN dbo.AttdnRawData ARIN ON AD.InTimeRowID = ARIN.RowID
        //                                                LEFT JOIN dbo.AttdnRawData AROUT ON AD.OutTimeRowID = AROUT.RowID
        //											    LEFT JOIN dbo.FinalOT FinalOT ON  FinalOT.EmpSystemID = E.SystemId AND AD.WorkDate = FinalOT.WorkDate                                                
        //                                                LEFT JOIN dbo.LeaveType LT ON AD.LTSystemID = LT.Id
        //                                                LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
        //                                                LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
        //                                                LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
        //                                                LEFT JOIN ORG.Section S ON E.SectionID = S.Id
        //                                                LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
        //                                                LEFT JOIN ORG.Line L ON E.LineID = L.Id
        //                                                LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
        //												LEFT JOIN HKP.Designation DD ON E.GivenDesignationId = DD.Id
        //                                    WHERE AD.PlantID = '" + objm.PlantId + @"' AND AD.WorkDate BETWEEN '" + objm.FDate + @"' AND '" + objm.TDate + @"' 
        //                                    AND (E.EmployeeStatus='Active' OR E.dos>'" + objm.FDate + @"' OR e.dos IS NULL)";



        //                strSql = strSql + @") A
        //                         GROUP BY A.EmployeeCode, A.EmployeeName, A.DOJ, A.Designation, A.Unit, A.Division, A.Department,
        //		                            A.Section, A.SubSection, A.Line, A.PDate, A.DayStatus, A.InTime, A.InDeviceID, A.OutTime,
        //                                    A.OutDeviceID, A.LvShortName, WorkDate, GivenDesignation,A.OTHr, A.EmployeePK,A.OTHr


        //                            ) TT
        //	                            DECLARE @sql nvarchar(max),
        //                                    @col nvarchar(max)

        //                            SELECT @col = (
        //                                SELECT DISTINCT ','+QUOTENAME(REPLACE(CONVERT(VARCHAR(11), WorkDate, 113), ' ', '-'))	
        //                                FROM #tempOT 
        //                                FOR XML PATH ('')
        //                            )

        //                            SELECT @sql = N'
        //                            (SELECT *
        //                            FROM #tempOT
        //                            PIVOT (
        //                                MAX([DayStatus]) FOR [WorkDate] IN ('+STUFF(@col,1,1,'')+')
        //                            ) as pvt)'

        //                            EXEC sp_executesql @sql
        //                            drop table #tempOT";
        //                return _sqlRepository.GetDataTable(strSql);
        //            }
        //            catch (Exception ex)
        //            {

        //                throw ex;
        //            }
        //        }

        //        private Dictionary<string, List<DataRow>> GetMonthlyDailyAttendanceDic(ParaMontlyAttendance objm, Dictionary<string, string> parameters)
        //        {
        //            try
        //            {

        //                var strSql = @"SELECT A.* FROM
        //	                                (SELECT E.systemId EmpSystemId,E.EmployeeCode, E.EmployeeName, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ,
        //                                            D.UserName Designation, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
        //                                            S.UserName Section, SB.UserName SubSection, L.UserName Line,  REPLACE(CONVERT(VARCHAR(11), AD.WorkDate, 113), ' ', '-') PDate,
        //                                            AD.DayStatus, FORMAT(InTime, 'hh.mm tt') InTime, ARIN.DeviceID InDeviceID, FORMAT(OutTime, 'hh.mm tt') OutTime,
        //                                            AROUT.DeviceID OutDeviceID,CONVERT(VARCHAR(10),CONVERT(DECIMAL(18,2),FinalOT.TotalOTHr/60), 108) OTHr, LT.UserName LvShortName
        //											,AD.WorkDate, DD.UserName GivenDesignation,DATEPART(day,ad.WorkDate) AS D
        //                                    FROM dbo.EmployeeInformation E
        //                                                INNER JOIN dbo.AttdnProcessData AD ON E.SystemID = AD.EmpSystemID
        //                                                LEFT JOIN dbo.AttdnRawData ARIN ON AD.InTimeRowID = ARIN.RowID
        //                                                LEFT JOIN dbo.AttdnRawData AROUT ON AD.OutTimeRowID = AROUT.RowID
        //											    LEFT JOIN dbo.FinalOT FinalOT ON  FinalOT.EmpSystemID = E.SystemId AND AD.WorkDate = FinalOT.WorkDate                                                
        //                                                LEFT JOIN dbo.LeaveType LT ON AD.LTSystemID = LT.Id
        //                                                LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
        //                                                LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
        //                                                LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
        //                                                LEFT JOIN ORG.Section S ON E.SectionID = S.Id
        //                                                LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
        //                                                LEFT JOIN ORG.Line L ON E.LineID = L.Id
        //                                                LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
        //												LEFT JOIN HKP.Designation DD ON E.GivenDesignationId = DD.Id
        //                                    WHERE AD.PlantID = '" + objm.PlantId + @"' AND AD.WorkDate BETWEEN '" + objm.FDate + @"' AND '" + objm.TDate + @"' 
        //                                    AND (E.DOS is null or E.DOS >= '" + objm.FDate + @"')									
        //									";
        //                if (parameters.Count > 0)
        //                {
        //                    if (parameters.Keys.ElementAt(0) != "")
        //                    {
        //                        strSql += @" AND E.SystemID IN(" + parameters["EmpSystemId"] + ")";
        //                    }
        //                }
        //                strSql += ") A ORDER BY EmpSystemId";
        //                DataTable dt = _sqlRepository.GetDataTable(strSql);

        //                Dictionary<string, List<DataRow>> dicShift = new Dictionary<string, List<DataRow>>();
        //                List<DataRow> _data = new List<DataRow>();
        //                string empId = "";
        //                for (int i = 0; i < dt.Rows.Count; i++)
        //                {
        //                    if (empId != dt.Rows[i]["EmpSystemId"].ToString())
        //                    {
        //                        _data = new List<DataRow>();
        //                        dicShift.Add(dt.Rows[i]["EmpSystemId"].ToString(), _data);
        //                    }
        //                    _data.Add(dt.Rows[i]);

        //                    empId = dt.Rows[i]["EmpSystemId"].ToString();
        //                }

        //                return dicShift;
        //            }
        //            catch (Exception ex)
        //            {

        //                throw ex;
        //            }
        //        }


        //        List<SwapColumn> GetColDisplayName(DataTable dslocal)
        //        {
        //            List<SwapColumn> list = null;
        //            try
        //            {
        //                list = new List<SwapColumn>();
        //                for (int i = 0; i < dslocal.Columns.Count; i++)
        //                {
        //                    var c = dslocal.Columns[i].ColumnName;
        //                    if (c.ToUpper() != "EMPLOYEEPK")
        //                    {
        //                        string _date = Convert.ToDateTime(c).ToString("dd-MMM-yyyy");
        //                        string _day = Convert.ToDateTime(c).ToString("dd");
        //                        SwapColumn ob = new SwapColumn();
        //                        ob.DisplayMember = _date;
        //                        ob.ValueMember = _day;
        //                        list.Add(ob);
        //                    }//if
        //                }
        //                return list;
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }


        //        public DataTable payRollGroup(string payRollGroupId)
        //        {
        //            try
        //            {
        //                var strSQL = @"select * from HKP.PayrollGroup where Id = '" + payRollGroupId + @"'";
        //                return _sqlRepository.GetDataTable(strSQL);
        //            }
        //            catch (Exception)
        //            {
        //                throw;
        //            }
        //        }

        //        public DataTable EmpCategory(string empCatgId)
        //        {
        //            try
        //            {
        //                var strSQL = @"select * from HKP.EmployeeCategory where Id = '" + empCatgId + @"'";
        //                return _sqlRepository.GetDataTable(strSQL);
        //            }
        //            catch (Exception)
        //            {
        //                throw;
        //            }
        //        }

        //        public DataTable GetBonusData(string PayRollGroupId, string BonusPointId)
        //        {
        //            try
        //            {
        //                var indexS = BonusPointId.IndexOf("__");
        //                var policyid = BonusPointId.Substring(0, indexS);
        //                var cutoffdate = BonusPointId.Substring(indexS + 2);

        //                string wc = string.Empty;
        //                wc = " where m.systemid in (select SystemID from BonusPaymentActualMaster " +
        //                    "where BonusSystemID='" + policyid + "' and EffectiveDate='" + cutoffdate + "') ";
        //                //where m.systemid in (    select SystemID from BonusPaymentActualMaster where BonusSystemID='' and EffectiveDate='')

        //                string sqlText = @"SELECT e.SystemId,e.EmployeeCode,EmployeeName,ISNULL(d.UserName,dg.UserName) Designation,hg.EntryAmount Gross,hb.EntryAmount [Basic]
        //                --,DOJ
        //                ,Replace(CONVERT(VARCHAR(11), DOJ, 106), ' ', '-') DOJ
        //                ,b.ServiceLenght
        //                ,b.BonusAmount,b.Remarks 
        //                --,b.BonusPercentage
        //               ,PG.StandardName PayRollGroupName,PG.Id PayRollGroupId
        //                ,min(b.BonusPercentageValue) BonusPercentage
        //                FROM EmployeeInformation e
        //                left join hkp.LegalDesignation d on e.LegalDesignationId=d.id


        //                LEFT JOIN HKP.Designation DG on DG.Id=E.GivenDesignationId

        //                Left join MST.payrollgroupmaster PM on PM.EmployeeId=E.SystemId---
        //                Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId----


        //                ---------------------gross
        //                left join (
        //                select m.EmpInfoSystemID,d.EntryAmount from SalaryInfoDefineMaster m
        //                left join SalaryInfoDefine d on m.SystemID=d.SalaryID
        //                left join SalaryHead h on h.SalaryHeadID=d.SalaryHeadID
        //                 where h.HeadCategory='Gross'
        //                 and m.IsApproved=1
        //                ) hg on hg.EmpInfoSystemID=e.SystemId
        //                ----------------------basic
        //                left join (
        //                select m.EmpInfoSystemID,d.EntryAmount from SalaryInfoDefineMaster m
        //                left join SalaryInfoDefine d on m.SystemID=d.SalaryID
        //                left join SalaryHead h on h.SalaryHeadID=d.SalaryHeadID
        //                 where h.HeadCategory='Basic'
        //                 and m.IsApproved=1
        //                ) hb on hb.EmpInfoSystemID=e.SystemId
        //                ----------------Bonus
        //                left join 
        //                (
        //                SELECT d.BonusAmount,d.ServiceLenght,mm.BonusSystemID BonusPolicyMasterID,d.EmpSystemID,mm.Remarks
        //                ,bpd.BonusPercentage   
        //                ,BonusPercentageValue= case 
        //                when bpd.IsFixed=1 then bpd.FixedAmount
        //                when bpd.IsPercentage=1 then bpd.BonusPercentage
        //                else bpd.BonusPercentage/bpd.DivisionFactor
        //                end
        //                FROM BonusPaymentActualMaster mm
        //                left join (
        //                				SELECT max(effectivedate) effectivedate   FROM BonusPaymentActualMaster m
        //                				" + wc + @"
        //                			) m on mm.effectivedate=m.effectivedate

        //                left join BonusPaymentActual d on mm.SystemID=d.BnsMstSystemID

        //                ----------------for %------------

        //                left join BonusPolicyMaster bpm on bpm.SystemID=mm.BonusSystemID
        //                left join BonusPolicyDetail bpd on bpd.BPMSystemID=bpm.SystemID and d.ServiceLenght between bpd.MinBonusAmt and bpd.MaxServLen

        //                ) b ON b.EmpSystemID=e.SystemId


        //                WHERE e.EmployeeStatus<>'Separated'AND PG.Id='" + PayRollGroupId + @"'
        //                GROUP BY e.SystemId,e.EmployeeCode,EmployeeName,d.UserName ,hg.EntryAmount ,hb.EntryAmount,doj,b.ServiceLenght
        //                ,b.BonusAmount, PG.StandardName ,PG.Id,dg.UserName,b.Remarks
        //               ORDER BY CONVERT(INT,e.EmployeeCode)";

        //                return _sqlRepository.GetDataTable(sqlText);
        //            }
        //            catch (Exception ex)
        //            {

        //                throw ex;
        //            }


        //        }

        //        public static int closestNumber(int n, int m)
        //        {
        //            // find the quotient 
        //            int q = n / m;

        //            // 1st possible closest number 
        //            int n1 = m * q;

        //            // 2nd possible closest number 
        //            int n2 = (n * m) > 0 ? (m * (q + 1)) : (m * (q - 1));

        //            // if true, then n1 is the required closest number 
        //            if (Math.Abs(n - n1) < Math.Abs(n - n2))
        //                return n1;

        //            // else n2 is the required closest number 
        //            return n2;
        //        }
        //        public DataTable GetLunchOutHour(string empSystemId, string fromDate, string toDate)
        //        {
        //            try
        //            {
        //                string strSql = "";
        //                strSql = @"SELECT EmpSystemId,SUM(LunchOutHour) LunchOutHour FROM LunchOutHour  
        //                WHERE EmpSystemId = '" + empSystemId + @"' and WorkDate Between '" + fromDate + @"' and '" + toDate + @"'
        //                    GROUP BY EmpSystemId";

        //                return _sqlRepository.GetDataTable(strSql);
        //            }
        //            catch (Exception ex)
        //            {

        //                throw ex;
        //            }
        //        }
        //        public IEnumerable<OrgStructureListViewModel> OrgStructureList(string CompanyGroupId)
        //        {
        //            try
        //            {
        //                var strSQL = @"  SELECT DISTINCT u.StandardName ColumnName,IsNULL(e.RType,'position') as Rtype,e.Sequence eSequence,p.Sequence pSequence from (
        //                           SELECT  DISTINCT StandardName from [ORG].[StructureRelationship] as ee where CompanyGroupId='" + CompanyGroupId + @"' and  RType = 'Entity'  union
        //                           SELECT  DISTINCT StandardName from [ORG].[StructureRelationship] as pp where CompanyGroupId='" + CompanyGroupId + @"' and  RType = 'position' ) u
        //                           LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Entity' ) e on e.StandardName = u.StandardName
        //						   LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Position' ) p on p.StandardName = u.StandardName";
        //                return _sqlRepository.GetModelCollection<OrgStructureListViewModel>(strSQL);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //        public IEnumerable<object> GetEmpInfo(string companyGroupId, string companyId, string plantId, string effectiveDate, string monthNo, string YearNo, string salaryProcessId, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity)
        //        {
        //            try
        //            {
        //                var wcPayrollGroup = "";
        //                var wcSalaryProcess = "";
        //                var salaryProcessJoin = "";
        //                var salaryProcessColumn = "";
        //                string wcEmpStatus = " Where (1=0 ";

        //                if (isActive == true && isSeperated == true && isMaternity == true)
        //                {
        //                    wcEmpStatus = " Where (1=1 ";
        //                }
        //                else
        //                {
        //                    if (isActive == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
        //                    }
        //                    if (isSeperated == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
        //                    }
        //                    if (isMaternity == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";
        //                    }
        //                }
        //                wcEmpStatus += ")";
        //                if (sa == true || ca == true)
        //                {
        //                    wcPayrollGroup = @"";
        //                }
        //                else
        //                {
        //                    string inPayrollGroup = "' '";
        //                    DataTable dtPayRollGrpEmpId = _sqlRepository.GetDataTable("SELECT employeeid FROM MST.PayrollGroupMaster WHERE PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"') AND PlantID = '" + plantId + @"'");
        //                    DataTable dtNotPayRollGrpEmpId = _sqlRepository.GetDataTable(@"SELECT SystemId FROM EmployeeInformation E 
        //                    WHERE SystemId NOT IN (SELECT employeeid from MST.PayrollGroupMaster where PlantID = '" + plantId + @"')  AND E.PlantID = '" + plantId + @"'");

        //                    if (dtPayRollGrpEmpId.Rows.Count > 0)
        //                    {
        //                        for (int i = 0; i < dtPayRollGrpEmpId.Rows.Count; i++)
        //                        {
        //                            inPayrollGroup += ",'" + dtPayRollGrpEmpId.Rows[i]["employeeid"].ToString() + "'";
        //                        }
        //                        if (dtNotPayRollGrpEmpId.Rows.Count > 0)
        //                        {
        //                            for (int i = 0; i < dtNotPayRollGrpEmpId.Rows.Count; i++)
        //                            {
        //                                inPayrollGroup += ",'" + dtNotPayRollGrpEmpId.Rows[i]["SystemId"].ToString() + "'";
        //                            }
        //                        }
        //                        wcPayrollGroup = @"AND E.SystemId  IN (" + inPayrollGroup + @")";
        //                    }
        //                    else
        //                    {
        //                        wcPayrollGroup = @"";
        //                    }

        //                }
        //                string strSql = @"SELECT SystemID FROM SalaryProcMaster
        //                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
        //                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
        //                                        AND MonthNo =  MONTH('" + effectiveDate + @"') AND YearNo =  YEAR('" + effectiveDate + @"')";

        //                DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);
        //                salaryProcessId = "''";
        //                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
        //                {
        //                    salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
        //                }
        //                wcSalaryProcess = @" AND SPC.SlrProcMstSystemID IN( " + salaryProcessId + @"  )";


        //                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
        //                var param = string.Empty;
        //                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
        //                    param = "E.GroupID='" + companyGroupId + "'AND  SPLD.PlantId='" + plantId + "'";
        //                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
        //                    param = "E.GroupID='" + companyGroupId + "'";

        //                var cmdText = @"SELECT [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* fROM (  SELECT   dISTINCT   
        //                                     isnull(e.SystemId,'') EmpSystemId
        //									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
        //                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
        //                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
        //                                    ,ISNULL(mpb.EntityId,'') EntityId
        //									,ISNULL(mpb.PositionId,'') PositionId                                     
        //                                    ,isnull(ld.UserName,'') Designation                                       
        //									,ISNULL(Department.UserName,'') Department 
        //									,ISNULL(Division.UserName,'') Division 
        //									,ISNULL(EmpC.UserName,'') EmployeeCategory
        //									,ISNULL(Plant.UserName,'') Plant 
        //									,ISNULL(Section.UserName,'') Section 
        //									,ISNULL(SubSection.UserName,'') SubSection 
        //									,ISNULL(Unit.UserName,'') Unit 
        //                                    ,ISNULL(eL.UserName,'') Line
        //                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
        //                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
        //									,CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') THEN 'Separated' else 'Active' end CurrentMonthEmployeeStatus
        //                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
        //                                    " + salaryProcessColumn + @"
        //									,ISNULL(PG.UserName,'') PayRollGroup
        //                                   , Case when Isnull(SPM.SalaryProcFlag,'') = '' THEN 'Regular' else SalaryProcFlag end SalaryProcFlag
        //                                    ,ISNULL(jl.JobLocation, '') JobLocation
        //									,ISNULL(SPLD.PaymentMode,'') PaymentMode
        //									,ISNULL(bb.UserName,'') BankName
        //                                    ,EmployeeCodePreFix,EmployeeCodeNumeric 
        //                                    ,Case when ISNULL(SalaryProcFlag,'') = '' THEN '' else ISNULL(st.UserName,'') end SeperationType
        //                                    FROM EmployeeInformation e

        //                                     --JOIN (
        //                                    --SELECT DISTINCT EmpInfoSystemID,SlrProcMstSystemID,PlantID ,m.Description,m.SalaryProcFlag
        //                                    --FROM SalaryProcChild c
        //                                    --JOIN SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
        //                                    --WHERE SlrProcMstSystemID in (select systemid from SalaryProcMaster where MonthNo= MONTH('" + effectiveDate + @"') and YearNo=YEAR('" + effectiveDate + @"'))
        //                                    --AND PlantID = '" + plantId + @"'
        //                                    --) SPM ON spm.EmpInfoSystemID=e.SystemId
        //									 --JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId  IN( " + salaryProcessId + "  )    AND e.SystemId = SPLD.EmpSystemId  --SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId and SPLD.PlantId = '" + plantId + @"' 
        //                                     --JOIN SalaryProcMaster SPM ON SPM.SystemID IN(" + salaryProcessId + ") --= SPC.SlrProcMstSystemID AND spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')

        //--LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId AND SPC.PlantID = '" + plantId + @"'
        //                                    JOIN (
        //                                     SELECT DISTINCT EmpInfoSystemID,SlrProcMstSystemID,PlantID ,m.Description,m.SalaryProcFlag
        //                                    FROM SalaryProcChild c
        //                                   INNER JOIN SalaryProcMaster m on M.MonthNo= MONTH('" + effectiveDate + @"') AND M.YearNo=YEAR('" + effectiveDate + @"') AND M.SystemID=C.SlrProcMstSystemID
        //                                    --WHERE SlrProcMstSystemID in (select systemid from SalaryProcMaster where MonthNo= MONTH('" + effectiveDate + @"') and YearNo=YEAR('" + effectiveDate + @"'))

        //                                        AND PlantID = '" + plantId + @"'
        //                                    ) SPM ON spm.EmpInfoSystemID=e.SystemId
        //									  JOIN SalaryProcessLogDetail SPLD ON 
        //									 --SPLD.SalaryProcessId  IN( '','M-2020337','M-2020338'  )    
        //									  SPLD.SalaryProcessId=SPM.SlrProcMstSystemID
        //									 AND SPM.EmpInfoSystemID = SPLD.EmpSystemId 
        //                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=SPLD.DesignationId
        //                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
        //									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
        //                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=SPLD.LegalDesignationId

        //                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=SPLD.BudgetCode
        //									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
        //                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
        //                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
        //                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
        //                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
        //                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
        //                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
        //                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId

        //                                    LEFT OUTER JOIN ORG.Line eL on eL.id=mpb.LineId
        //                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
        //                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = SPLD.EmployeeCategoryId
        //			                        LEFT JOIN TRN.Resignation  Resig ON  Resig.Id =   (SELECT  TOP 1 id FROM TRN.Resignation WHERE EmployeeId = E.SystemId order by AddedDate desc) 
        //									LEFT JOIN HKP.SeparationType st on st.Id = resig.SeparationTypeId
        //                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
        //                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
        //									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
        //                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
        //									left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
        //									left join [HKP].[Bank] bb on bb.Id = SPLD.BankSystemID

        //                                     WHERE " + param + @"
        //                                            " + wcPayrollGroup + @"                                   
        //                                     ) DD " + wcEmpStatus + " ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric ";
        //                return _sqlRepository.GetDataCollection(cmdText);
        //            }
        //            catch (Exception)
        //            {
        //                throw;
        //            }
        //        }

        public void InsertORUpdate(IEnumerable<PlantWiseSalaryRegisterSortingParameters> entities, string companyGroupId, string companyId, string plantId)
        {
            var flag = false;
            try
            {
                string strDelete = "";
                DataSet dsMaster = new DataSet();
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                strDelete = "Delete from [dbo].[PlantWiseSalaryRegisterSortingParameters]";
                _sqlRepository.ExecuteSqlCommand(strDelete);
                //if (entities == null)
                //    throw new CustomException("Please insert legal designation");
                con.OpenDataSetThroughAdapter("select * from PlantWiseSalaryRegisterSortingParameters", out dsMaster, false, "1");
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber(nameof(PlantWiseSalaryRegisterSortingParameters), PKGeneratorEnum.Auto, DateTime.Now);
                foreach (var item in entities)
                {

                    pk.MaxNumber++;
                    item.Id = pk.MaxNumber.ToString();
                    item.CompanyGroupId = companyGroupId;
                    item.CompanyId = companyId;
                    item.PlantId = plantId;
                    InsertGraph(item);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        //        public List<SalaryRegisterSorting> GetPlantWiseSalaryRegisterSortingParameters(string companyGroupId, string companyId, string plantId)
        //        {
        //            try
        //            {
        //                string strSQL = "";
        //                strSQL = @"SELECT Parameter,Sequence FROM PlantWiseSalaryRegisterSortingParameters WHERE CompanyGroupId = '" + companyGroupId + @"' AND CompanyId = '" + companyId + @"' AND PlantId = '" + plantId + "'";
        //                return _sqlRepository.GetModelCollection<SalaryRegisterSorting>(strSQL, null);
        //            }
        //            catch (Exception ex)
        //            {

        //                throw ex;
        //            }


        //        }

        //        public void GetEmployeeInfoDetail(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string languageId, string stringSalaryRegSorting, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef)
        //        {
        //            string strSQL;
        //            ConnectionManager.DAL.ConManager objCon;
        //            var _wc = string.Empty;
        //            try
        //            {
        //                string wcEmpStatus = " Where (1=0 ";

        //                if (isActive == true && isSeperated == true && isMaternity == true)
        //                {
        //                    wcEmpStatus = " Where (1=1 ";
        //                }
        //                else
        //                {
        //                    if (isActive == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
        //                    }
        //                    if (isSeperated == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
        //                    }
        //                    if (isMaternity == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

        //                    }
        //                }

        //                string salaryProcessID = "' '";

        //                DataTable dtSalPrcId = _sqlRepository.GetDataTable(@"SELECT SystemID FROM SalaryProcMaster
        //                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
        //                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
        //                                        AND MonthNo =  MONTH('" + toDate + @"') AND YearNo =  YEAR('" + toDate + @"') ");


        //                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
        //                {
        //                    salaryProcessID += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
        //                }


        //                wcEmpStatus += ")";

        //                strSQL = @"SELECT * FROM (SELECT DISTINCT  E.SystemID,  E.EmployeeCode , E.EmployeeName,ISNULL(E.EmployeeNameLocal,E.EmployeeName) EmployeeNameLocal,E.FatherName,ISNULL(SPM.EntryAmount,0) GrossAmount,  Format(E.DOJ,'dd-MMM-yyy') DOJ, Format(E.DOB,'dd-MMM-yyy') DOB,Format(E.DOS,'dd-MMM-yyy') DOS, E.EmployeeStatus,SPLD.PaymentMode,
        //											--DG.UserName DesignationGroupName
        //											 E.DesignationSystemID, --GVDE.UserName GivenDesignationName,
        //											'' UserGroupSystemID, E.PlantID, F.UserName PlantName, F.Sequence PlantSequence, E.UnitID
        //											,Unit.UserName UnitName,Unit.Sequence UnitSequence, Division.Id DivisionID,
        //											 Division.UserName DivisionName,Division.Sequence DivisionSequence
        //											,Department.Id DepartmentID, Department.UserName DepartmentName,Department.Sequence DepartmentSequence,
        //											Section.Id SectionID, Section.UserName SectionName,Section.Sequence SectionSequence,
        //											 SubSection.Id SubSectionID, SubSection.UserName SubSectionName,SubSection.Sequence SubSectionSequence,EC.Id EmployeeCategorySystemID
        //											,EC.UserName EmpCategoryName,EC.Sequence EmployeeCategorySequence--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
        //                                            ,ENT.UserName EntitySequence,e.SalaryRuleMasterSystemID,L.UserName Line, LD.UserName LegalDesignation--,eoe.IsOTEntitle

        //										   ,ISNULL(LD.Id,'') DesignationId,LD.UserName DesignationName,LD.Sequence DesignationSequence,ISNULL(EC.Id,'') EmployeeCategoryId
        //                                           ,ISNULL(EC.UserName,'') EmployeeCategoryName,ISNULL(EC.WorkingDaysInAMonth,'') WorkingDaysInAMonth
        //                                           , ISNULL(LD.UserName,'') LDDesignationGD,LSalGr.Code GradeCode,E.EmployeeCodePreFix, E.EmployeeCodeNumeric
        //										   ,CASE WHEN MONTH(DOS) =  MONTH('" + toDate + @"')  AND YEAR(DOS) = YEAR('" + toDate + @"') THEN 'Separated' else 'Active' end CurrentMonthEmployeeStatus
        //                                           , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
        //		                                   ,ISNULL(LocLangLD.Name,LD.UserName) DesignationLocal
        //										   ,E.LineId,L.Sequence LineSequence, esic.ESICNo,pf.UANNo,SPLD.BankAccNo,bb.BankName, '' BankNameFull
        //                                           ,MMDSA.*
        //                                           FROM EmployeeInformation E
        // JOIN (
        //                                    SELECT DISTINCT EmpInfoSystemID,SlrProcMstSystemID,PlantID ,m.Description,m.SalaryProcFlag,DisbusmentAmount,EntryAmount
        //                                    FROM SalaryProcChild c
        //                                    JOIN SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
        //                                         join SalaryHead SH ON SH.SalaryHeadID = C.SalaryHeadID where HeadCategory = 'GROSS'
        //                                    AND SlrProcMstSystemID in (select systemid from SalaryProcMaster where MonthNo= MONTH('" + toDate + @"') and YearNo=YEAR('" + toDate + @"'))
        //                                    AND PlantID = '" + plantId + @"'
        //                                    ) SPM ON spm.EmpInfoSystemID=e.SystemId
        //									 JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId  IN( " + salaryProcessID + "  )    AND e.SystemId = SPLD.EmpSystemId  --SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId and SPLD.PlantId = '" + plantId + @"' 
        //                                     --JOIN SalaryProcMaster SPM ON SPM.SystemID IN(" + salaryProcessID + ") --= SPC.SlrProcMstSystemID AND spm.MonthNo = Month('" + toDate + @"') and spm.YearNo = Year('" + toDate + @"')

        //												--LEFT OUTER JOIN (Select EmpInfoSystemID,EntryAmount,DisbusmentAmount,SlrProcMstSystemID from SalaryProcChild SPC Left join SalaryHead SH ON SH.SalaryHeadID = SPC.SalaryHeadID where HeadCategory = 'GROSS') SPC ON SPC.EmpInfoSystemID = E.SystemId
        //                                            --LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID and spm.MonthNo = Month('" + toDate + @"') and spm.YearNo = Year('" + toDate + @"')

        //                                                --INNER JOin SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId

        //												LEFT JOIN org.Plant F ON SPLD.PlantID = F.Id

        //                                                LEFT JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = SPLD.BudgetCode
        //								                LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId

        //									LEFT OUTER JOIN ORG.Position PO ON MB.PositionId=PO.Id
        //                                    LEFT OUTER JOIN ORG.Entity EN ON MB.EntityId=EN.Id
        //                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
        //                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
        //                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
        //                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
        //                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
        //                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
        //                                    Left join org.Line L on L.Id = MB.LineId
        //												LEFT JOIN HKP.LegalDesignation LD ON LD.Id=SPLD.LegalDesignationId
        //												LEFT JOIN HKP.Designation GVD ON GVD.Id=E.GivenDesignationId
        //                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LD.Id and E.PlantId = LSGD.PlantId
        //                                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = SPLD.LegalSalaryGradeId  --and E.PlantId = LSalGr.PlantId

        //                                    LEFT JOIN [HKP].EmployeeCategory EC ON EC.Id = SPLD.EmployeeCategoryId



        //                                                LEFT JOIN HKP.LocalLanguage LocLangLD ON LocLangLD.LegalDesignationId = SPLD.LegalDesignationId AND LocLangLD.LanguageId = '" + languageId + @"'

        //												 INNER JOIN
        //		                                    (
        //												select EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
        //										,ISNULL(TotalLv,0) TotalLv
        //										,ISNULL(TotalMLv,0) TotalMLv,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv,ISNULL(TotalWeekOff,0) + ISNULL(TotalWeekOffHoliDay,0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
        //										,ISNULL(TotalOTHr,0) TotalOTHr,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr,ISNULL(WeekOffOTHr,0) WeekOffOTHr
        //										,ISNULL(HoliDayOTHr,0) HoliDayOTHr,ISNULL(TotalLWP,0) TotalLWP,ISNULL(IsOTEntitled,0) IsOTEntitled,ISNULL(OTRate,0) OTRate,ISNULL(TotalHoliDay,0) TotalHoliDay
        //										  FROM SalaryProceAttdnData MMDSA
        //                                            WHERE MMDSA.MonthNo = MONTH('" + toDate + @"') AND
        //						                               MMDSA.YearNo = YEAR('" + toDate + @"')
        //											) MMDSA ON E.SystemId = MMDSA.EmpSystemID 
        //											    LEFT JOIN 
        //                                           		(
        //                                           		 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
        //                                           			FROM EmployeeInformation E   
        //                                           					LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
        //                                           					LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
        //                                                                                               AND E.PlantId = gd.PlantId
        //                                           					LEFT JOIN (
        //                                           								SELECT MAX(EffectiveDate)EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
        //                                           									FROM MST.LegalSalaryStructure 
        //                                           									WHERE EffectiveDate <= '" + toDate + @"'
        //                                           								GROUP BY LegalSalaryGradeId, EmployeeLocationId 
        //                                           							  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
        //                                           					LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
        //                                                                                           AND SS.EmployeeLocationId = S.EmployeeLocationId 
        //                                                                                           AND SS.EffectiveDate = S.EffectiveDate
        //                                           					LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 																							
        //                                           					left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId
        //                                           			GROUP BY E.SystemId,LSG.UserName 
        //                                           		) MW ON MW.SystemId = E.SystemId
        //                                                LEFT JOIN (
        //                                                SELECT bb.UserName BankName,b.BankAccNo,b.EmpSystemID FROM [dbo].[EmployeeBankInfo] b
        //                                                LEFT JOIN hkp.BankBranch bb ON b.BankBranchId=bb.Id
        //                                                ) BB ON BB.EmpSystemID = E.SystemId
        //                                                LEFT JOIN
        //												( SELECT ed.DocNumber UANNo,ED.EmpSystemID FROM 
        //												EmployeeDocument ED 
        //												INNER JOIN (SELECT * FROM HKP.ComplianceDocument WHERE ProfileType = 'PF') CD ON CD.Id = ED.ComplianceDocumentId 
        //												) pf ON E.SystemId = pf.EmpSystemID
        //												LEFT JOIN
        //												( SELECT ed.DocNumber ESICNo,ED.EmpSystemID FROM 
        //												EmployeeDocument ED 
        //												INNER JOIN (SELECT * FROM HKP.ComplianceDocument WHERE ProfileType = 'ESIC') CD ON CD.Id = ED.ComplianceDocumentId 
        //												) ESIC ON E.SystemId = esic.EmpSystemID 

        //                                    WHERE  E.GroupID='" + companyGroupId + @"'  AND spld.PlantId='" + plantId + @"'
        //                                               ";

        //                if (parameters.Count > 0)
        //                {
        //                    if (parameters.Keys.ElementAt(0) != "")
        //                    {
        //                        strSQL += @" AND E.SystemID IN(" + parameters["EmpSystemId"] + ")";
        //                    }
        //                }
        //                strSQL += @")dd " + wcEmpStatus + @"";

        //                strSQL += stringSalaryRegSorting.Replace("EmpBasic.", "");


        //                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
        //                con.getDataSet(strSQL, out dsRef);


        //            }
        //            catch (Exception ex)
        //            {
        //                throw (ex);
        //            }
        //            finally
        //            {
        //                objCon = null;
        //            }
        //        }//End Function



        //        public Dictionary<string, List<DataRow>> GetEmployeeSalaryInfoDetail(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string languageId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataTable distinctSalaryHead)
        //        {
        //            string strSQL;
        //            DataSet dsRef = null;
        //            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
        //            distinctSalaryHead = new DataTable("Tmp");
        //            try
        //            {

        //                string wcEmpStatus = " Where (1=0 ";

        //                if (isActive == true && isSeperated == true && isMaternity == true)
        //                {
        //                    wcEmpStatus = " Where (1=1 ";
        //                }
        //                else
        //                {
        //                    if (isActive == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='Regular' ";
        //                    }
        //                    if (isSeperated == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
        //                    }
        //                    if (isMaternity == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

        //                    }
        //                }
        //                DataTable dtslProcId = _sqlRepository.GetDataTable(@" SELECT SystemID FROM SalaryProcMaster
        //                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
        //                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
        //                                        AND MonthNo = Month('" + toDate + @"') AND YearNo = Year('" + toDate + @"') ");
        //                string inSalaryProcParam = "' '";

        //                for (int i = 0; i < dtslProcId.Rows.Count; i++)
        //                {
        //                    inSalaryProcParam += ",'" + dtslProcId.Rows[i]["SystemID"].ToString() + "'";
        //                }

        //                wcEmpStatus += ")";

        //                strSQL = @"SELECT EmpSlr.*,ISNULL(PSH.Sequence,99) Sequence,ISNULL(crc.IsDecimalInDisb,0) IsDecimalInDisb,ISNULL(CRC.IntegerInDisb,1) IntegerInDisb,ISNULL(CRC.DecimalNo,0) DecimalNo FROM(SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
        //                                                    SPC.EmpInfoSystemID , SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
        //                                                    SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
        //                                                    SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
        //                                                    CRE.Name AS PlantWiseExchangeCR, EXR.ToCurrencyBuying ExchangeRate, SPM.AmtDefinitionCurrencyID,
        //                                                    CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect, ISNULL(SH.IsCTCComponent,0) IsCTCComponent, ISNULL(SH.IsGrossComponent,0) IsGrossComponent
        //                                                    , sh.SalaryHead,ISNULL(ISNULL(BSH.Name,SH.SalaryHead),'') SalaryHeadBangla, sh.HeadCategory, sh.HeadType, ISNULL(SH.PartOfNetPay,0) PartOfNetPay
        //													, Case when Isnull(SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
        //                                     FROM SalaryProcChild SPC

        //                                        left JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
        //                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
        //                                                        LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL AND LanguageId = '" + languageId + @"') AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID --BanglaSalaryHead
        //                                                        LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id
        //                                                        LEFT JOIN (
        //                                                                   SELECT* FROM ExchangerateDateWiseForHR
        //                                                                   WHERE FromDate IN (SELECT MAX(FromDate) FromDate FROM SalaryProcMaster
        //                                                                                                           WHERE SystemID IN(" + inSalaryProcParam + @")
        //																  )) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode
        //                                                                                            AND SPC.PlantID = Exr.PlantID
        //                                                        LEFT JOIN SCS.Currency CRE ON EXR.FromCurrencyCode = CRE.Id

        //                                                        WHERE ISNULL(SPC.SlrProcMstSystemID,'')  IN(" + inSalaryProcParam + @")) EmpSlr--ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID

        //                                            Inner join EmployeeInformation EEI ON EEI.SystemId = EmpSlr.EmpInfoSystemID

        //                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID

        //                                        LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID  AND SRG.SalaryHeadID = EmpSlr.SalaryHeadID
        //                                        LEFT JOIN(SELECT* FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId = '" + plantId + @"') PSH
        //                                                                       ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
        //                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = EmpSlr.SalaryHeadID

        //                                                WHERE EEI.GroupID = '" + companyGroupId + @"' AND  EmpSlr.PlantId = '" + plantId + @"' 
        //                  ";
        //                if (parameters.Count > 0)
        //                {
        //                    if (parameters.Keys.ElementAt(0) != "")
        //                    {
        //                        strSQL += @" AND EmpSlr.EmpInfoSystemID IN(" + parameters["EmpSystemId"] + ")";
        //                    }
        //                }

        //                strSQL += "ORDER BY EmpSlr.EmpInfoSystemID,Sequence";

        //                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
        //                con.getDataSet(strSQL, out dsRef);

        //                distinctSalaryHead = dsRef.Tables[0].DefaultView.ToTable(true, "SalaryHeadID", "SalaryHead", "SalaryHeadBangla", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo", "IsGrossComponent", "IsCTCComponent", "PartOfNetPay");
        //                distinctSalaryHead.DefaultView.Sort = "Sequence";
        //                distinctSalaryHead = distinctSalaryHead.DefaultView.ToTable();

        //                DataTable dt = dsRef.Tables[0];
        //                List<DataRow> _data = new List<DataRow>();
        //                string empId = "";
        //                for (int i = 0; i < dt.Rows.Count; i++)
        //                {
        //                    if (empId != dt.Rows[i]["EmpInfoSystemID"].ToString())
        //                    {
        //                        _data = new List<DataRow>();
        //                        dicBonus.Add(dt.Rows[i]["EmpInfoSystemID"].ToString(), _data);
        //                    }
        //                    _data.Add(dt.Rows[i]);

        //                    empId = dt.Rows[i]["EmpInfoSystemID"].ToString();
        //                }
        //                return dicBonus;
        //            }
        //            catch (Exception ex)
        //            {
        //                throw (ex);
        //            }
        //            finally
        //            {
        //                //objCon = null;
        //            }
        //        }//End Function

        //        #region For Laila GetEmployeeSalaryInfoDetailPayRollGroup

        //        public void GetEmployeeInfoDetailPayRollGroup(ParamList para, string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string languageId, string stringSalaryRegSorting, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef)
        //        {
        //            string strSQL;
        //            ConnectionManager.DAL.ConManager objCon;
        //            var _wc = string.Empty;
        //            try
        //            {
        //                string wcEmpStatus = " Where (1=0 ";

        //                if (isActive == true && isSeperated == true && isMaternity == true)
        //                {
        //                    wcEmpStatus = " Where (1=1 ";
        //                }
        //                else
        //                {
        //                    if (isActive == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
        //                    }
        //                    if (isSeperated == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
        //                    }
        //                    if (isMaternity == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

        //                    }
        //                }
        //                string _wcPayrollGroup = "";

        //                if (para.EmployeeId != "" && para.EmployeeId != null)
        //                {
        //                    _wcPayrollGroup = "  SYSTEMID IN (" + para.EmployeeId + @")";
        //                }
        //                else
        //                {
        //                    if (para.PayGroup.ToUpper() != "NO GROUP" && para.PayGroup != "null")
        //                    {
        //                        _wcPayrollGroup = @"AND E.SystemID  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId = '" + para.PayGroup + @"')";
        //                    }
        //                    if (para.PayGroup.ToUpper() == "NO GROUP")
        //                    {
        //                        _wcPayrollGroup = @"AND E.SystemID NOT IN ( SELECT employeeid from MST.PayrollGroupMaster)";
        //                    }
        //                    if (para.PayGroup.ToUpper() == "ALL")
        //                    {
        //                        _wcPayrollGroup = @"";
        //                    }

        //                }
        //                string salaryProcessID = "' '";

        //                DataTable dtSalPrcId = _sqlRepository.GetDataTable(@"SELECT SystemID FROM SalaryProcMaster
        //                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
        //                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
        //                                        AND MonthNo =  MONTH('" + toDate + @"') AND YearNo =  YEAR('" + toDate + @"') ");


        //                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
        //                {
        //                    salaryProcessID += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
        //                }


        //                wcEmpStatus += ")";

        //                strSQL = @"SELECT * FROM (SELECT DISTINCT  E.SystemID,  E.EmployeeCode , E.EmployeeName,ISNULL(E.EmployeeNameLocal,E.EmployeeName) EmployeeNameLocal,E.FatherName,ISNULL(SPM.EntryAmount,0) GrossAmount,  Format(E.DOJ,'dd-MMM-yyy') DOJ, Format(E.DOB,'dd-MMM-yyy') DOB,Format(E.DOS,'dd-MMM-yyy') DOS, E.EmployeeStatus,SPLD.PaymentMode,
        //											--DG.UserName DesignationGroupName
        //											 E.DesignationSystemID, --GVDE.UserName GivenDesignationName,
        //											'' UserGroupSystemID, E.PlantID, F.UserName PlantName, F.Sequence PlantSequence, E.UnitID
        //											,Unit.UserName UnitName,Unit.Sequence UnitSequence, Division.Id DivisionID,
        //											 Division.UserName DivisionName,Division.Sequence DivisionSequence
        //											,Department.Id DepartmentID, Department.UserName DepartmentName,Department.Sequence DepartmentSequence,
        //											Section.Id SectionID, Section.UserName SectionName,Section.Sequence SectionSequence,
        //											 SubSection.Id SubSectionID, SubSection.UserName SubSectionName,SubSection.Sequence SubSectionSequence,EC.Id EmployeeCategorySystemID
        //											,EC.UserName EmpCategoryName,EC.Sequence EmployeeCategorySequence--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
        //                                            ,ENT.UserName EntitySequence,e.SalaryRuleMasterSystemID,L.UserName Line, LD.UserName LegalDesignation--,eoe.IsOTEntitle

        //										   ,ISNULL(LD.Id,'') DesignationId,LD.UserName DesignationName,LD.Sequence DesignationSequence,ISNULL(EC.Id,'') EmployeeCategoryId
        //                                           ,ISNULL(EC.UserName,'') EmployeeCategoryName,ISNULL(EC.WorkingDaysInAMonth,'') WorkingDaysInAMonth
        //                                           , ISNULL(LD.UserName,'') LDDesignationGD,LSalGr.Code GradeCode,E.EmployeeCodePreFix, E.EmployeeCodeNumeric
        //										   ,CASE WHEN MONTH(DOS) =  MONTH('" + toDate + @"')  AND YEAR(DOS) = YEAR('" + toDate + @"') THEN 'Separated' else 'Active' end CurrentMonthEmployeeStatus
        //                                           , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
        //		                                   ,ISNULL(LocLangLD.Name,LD.UserName) DesignationLocal
        //										   ,E.LineId,L.Sequence LineSequence, esic.ESICNo,pf.UANNo,SPLD.BankAccNo,bb.BankName, '' BankNameFull
        //                                           ,MMDSA.*
        //                                           FROM EmployeeInformation E
        // JOIN (
        //                                    SELECT DISTINCT EmpInfoSystemID,SlrProcMstSystemID,PlantID ,m.Description,m.SalaryProcFlag,DisbusmentAmount,EntryAmount
        //                                    FROM SalaryProcChild c
        //                                    JOIN SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
        //                                         join SalaryHead SH ON SH.SalaryHeadID = C.SalaryHeadID where HeadCategory = 'GROSS'
        //                                    AND SlrProcMstSystemID in (" + salaryProcessID + @"  )
        //                                    AND PlantID = '" + plantId + @"'
        //                                    ) SPM ON spm.EmpInfoSystemID=e.SystemId
        //									 JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId  IN( " + salaryProcessID + "  )    AND e.SystemId = SPLD.EmpSystemId  --SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId and SPLD.PlantId = '" + plantId + @"' 
        //                                     --JOIN SalaryProcMaster SPM ON SPM.SystemID IN(" + salaryProcessID + ") --= SPC.SlrProcMstSystemID AND spm.MonthNo = Month('" + toDate + @"') and spm.YearNo = Year('" + toDate + @"')

        //												--LEFT OUTER JOIN (Select EmpInfoSystemID,EntryAmount,DisbusmentAmount,SlrProcMstSystemID from SalaryProcChild SPC Left join SalaryHead SH ON SH.SalaryHeadID = SPC.SalaryHeadID where HeadCategory = 'GROSS') SPC ON SPC.EmpInfoSystemID = E.SystemId
        //                                            --LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID and spm.MonthNo = Month('" + toDate + @"') and spm.YearNo = Year('" + toDate + @"')

        //                                                --INNER JOin SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId

        //												LEFT JOIN org.Plant F ON SPLD.PlantID = F.Id

        //                                                LEFT JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = SPLD.BudgetCode
        //								                LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId

        //									LEFT OUTER JOIN ORG.Position PO ON MB.PositionId=PO.Id
        //                                    LEFT OUTER JOIN ORG.Entity EN ON MB.EntityId=EN.Id
        //                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
        //                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
        //                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
        //                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
        //                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
        //                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
        //                                    Left join org.Line L on L.Id = MB.LineId
        //												LEFT JOIN HKP.LegalDesignation LD ON LD.Id=SPLD.LegalDesignationId
        //												LEFT JOIN HKP.Designation GVD ON GVD.Id=E.GivenDesignationId
        //                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LD.Id and E.PlantId = LSGD.PlantId
        //                                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = SPLD.LegalSalaryGradeId  --and E.PlantId = LSalGr.PlantId

        //                                    LEFT JOIN [HKP].EmployeeCategory EC ON EC.Id = SPLD.EmployeeCategoryId



        //                                                LEFT JOIN HKP.LocalLanguage LocLangLD ON LocLangLD.LegalDesignationId = SPLD.LegalDesignationId AND LocLangLD.LanguageId = '" + languageId + @"'

        //												 INNER JOIN
        //		                                    (
        //												select EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
        //										,ISNULL(TotalLv,0) TotalLv
        //										,ISNULL(TotalMLv,0) TotalMLv,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv,ISNULL(TotalWeekOff,0) + ISNULL(TotalWeekOffHoliDay,0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
        //										,ISNULL(TotalOTHr,0) TotalOTHr,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr,ISNULL(WeekOffOTHr,0) WeekOffOTHr
        //										,ISNULL(HoliDayOTHr,0) HoliDayOTHr,ISNULL(TotalLWP,0) TotalLWP,ISNULL(IsOTEntitled,0) IsOTEntitled,ISNULL(OTRate,0) OTRate,ISNULL(TotalHoliDay,0) TotalHoliDay
        //										  FROM SalaryProceAttdnData MMDSA
        //                                            WHERE MMDSA.MonthNo = MONTH('" + toDate + @"') AND
        //						                               MMDSA.YearNo = YEAR('" + toDate + @"')
        //											) MMDSA ON E.SystemId = MMDSA.EmpSystemID 
        //											    LEFT JOIN 
        //                                           		(
        //                                           		 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
        //                                           			FROM EmployeeInformation E   
        //                                           					LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
        //                                           					LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
        //                                                                                               AND E.PlantId = gd.PlantId
        //                                           					LEFT JOIN (
        //                                           								SELECT MAX(EffectiveDate)EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
        //                                           									FROM MST.LegalSalaryStructure 
        //                                           									WHERE EffectiveDate <= '" + toDate + @"'
        //                                           								GROUP BY LegalSalaryGradeId, EmployeeLocationId 
        //                                           							  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
        //                                           					LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
        //                                                                                           AND SS.EmployeeLocationId = S.EmployeeLocationId 
        //                                                                                           AND SS.EffectiveDate = S.EffectiveDate
        //                                           					LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 																							
        //                                           					left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId
        //                                           			GROUP BY E.SystemId,LSG.UserName 
        //                                           		) MW ON MW.SystemId = E.SystemId
        //                                                LEFT JOIN (
        //                                                SELECT bb.UserName BankName,b.BankAccNo,b.EmpSystemID FROM [dbo].[EmployeeBankInfo] b
        //                                                LEFT JOIN hkp.BankBranch bb ON b.BankBranchId=bb.Id
        //                                                ) BB ON BB.EmpSystemID = E.SystemId
        //                                                LEFT JOIN
        //												( SELECT ed.DocNumber UANNo,ED.EmpSystemID FROM 
        //												EmployeeDocument ED 
        //												INNER JOIN (SELECT * FROM HKP.ComplianceDocument WHERE ProfileType = 'PF') CD ON CD.Id = ED.ComplianceDocumentId 
        //												) pf ON E.SystemId = pf.EmpSystemID
        //												LEFT JOIN
        //												( SELECT ed.DocNumber ESICNo,ED.EmpSystemID FROM 
        //												EmployeeDocument ED 
        //												INNER JOIN (SELECT * FROM HKP.ComplianceDocument WHERE ProfileType = 'ESIC') CD ON CD.Id = ED.ComplianceDocumentId 
        //												) ESIC ON E.SystemId = esic.EmpSystemID 

        //                                    WHERE  E.GroupID='" + companyGroupId + @"'  AND spld.PlantId='" + plantId + @"' " + _wcPayrollGroup + @"
        //                                               ";
        //                if (parameters != null)
        //                {
        //                    if (parameters.Count > 0)
        //                    {
        //                        if (parameters.Keys.ElementAt(0) != "")
        //                        {
        //                            strSQL += @" AND E.SystemID IN(" + parameters["EmpSystemId"] + ")";
        //                        }
        //                    }
        //                }

        //                strSQL += @")dd " + wcEmpStatus + @"";

        //                strSQL += stringSalaryRegSorting.Replace("EmpBasic.", "");


        //                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
        //                con.getDataSet(strSQL, out dsRef);


        //            }
        //            catch (Exception ex)
        //            {
        //                throw (ex);
        //            }
        //            finally
        //            {
        //                objCon = null;
        //            }
        //        }//End Function

        //        public Dictionary<string, List<DataRow>> GetEmployeeSalaryInfoDetailPayRollGroup(ParamList para, string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string languageId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataTable distinctSalaryHead)
        //        {
        //            string strSQL;
        //            DataSet dsRef = null;
        //            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
        //            distinctSalaryHead = new DataTable("Tmp");
        //            try
        //            {

        //                string wcEmpStatus = " Where (1=0 ";

        //                if (isActive == true && isSeperated == true && isMaternity == true)
        //                {
        //                    wcEmpStatus = " Where (1=1 ";
        //                }
        //                else
        //                {
        //                    if (isActive == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='Regular' ";
        //                    }
        //                    if (isSeperated == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
        //                    }
        //                    if (isMaternity == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

        //                    }
        //                }
        //                DataTable dtslProcId = _sqlRepository.GetDataTable(@" SELECT SystemID FROM SalaryProcMaster
        //                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
        //                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
        //                                        AND MonthNo = Month('" + toDate + @"') AND YearNo = Year('" + toDate + @"') ");
        //                string inSalaryProcParam = "' '";

        //                for (int i = 0; i < dtslProcId.Rows.Count; i++)
        //                {
        //                    inSalaryProcParam += ",'" + dtslProcId.Rows[i]["SystemID"].ToString() + "'";
        //                }

        //                wcEmpStatus += ")";
        //                string _wcPayrollGroup = "";

        //                if (para.EmployeeId != "" && para.EmployeeId != null)
        //                {
        //                    _wcPayrollGroup = "  SYSTEMID IN (" + para.EmployeeId + @")";
        //                }
        //                else
        //                {
        //                    if (para.PayGroup.ToUpper() != "NO GROUP" && para.PayGroup != "null")
        //                    {
        //                        _wcPayrollGroup = @"AND EmpInfoSystemID  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId = '" + para.PayGroup + @"')";
        //                    }
        //                    if (para.PayGroup.ToUpper() == "NO GROUP")
        //                    {
        //                        _wcPayrollGroup = @"AND EmpInfoSystemID NOT IN ( SELECT employeeid from MST.PayrollGroupMaster)";
        //                    }
        //                    if (para.PayGroup.ToUpper() == "ALL")
        //                    {
        //                        _wcPayrollGroup = @"";
        //                    }

        //                }

        //                strSQL = @"SELECT EmpSlr.*,ISNULL(PSH.Sequence,99) Sequence,ISNULL(crc.IsDecimalInDisb,0) IsDecimalInDisb,ISNULL(CRC.IntegerInDisb,1) IntegerInDisb,ISNULL(CRC.DecimalNo,0) DecimalNo FROM(SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
        //                                                    SPC.EmpInfoSystemID , SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
        //                                                    SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
        //                                                    SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
        //                                                    CRE.Name AS PlantWiseExchangeCR, EXR.ToCurrencyBuying ExchangeRate, SPM.AmtDefinitionCurrencyID,
        //                                                    CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect, ISNULL(SH.IsCTCComponent,0) IsCTCComponent, ISNULL(SH.IsGrossComponent,0) IsGrossComponent
        //                                                    , sh.SalaryHead,ISNULL(ISNULL(BSH.Name,SH.SalaryHead),'') SalaryHeadBangla, sh.HeadCategory, sh.HeadType, ISNULL(SH.PartOfNetPay,0) PartOfNetPay
        //													, Case when Isnull(SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
        //                                     FROM SalaryProcChild SPC

        //                                        LEFT JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
        //                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
        //                                                        LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL AND LanguageId = '" + languageId + @"') AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID --BanglaSalaryHead
        //                                                        LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id
        //                                                        LEFT JOIN (
        //                                                                   SELECT* FROM ExchangerateDateWiseForHR
        //                                                                   WHERE FromDate IN (SELECT MAX(FromDate) FromDate FROM SalaryProcMaster
        //                                                                                                           WHERE SystemID IN(" + inSalaryProcParam + @")
        //																  )) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode
        //                                                                                            AND SPC.PlantID = Exr.PlantID
        //                                                        LEFT JOIN SCS.Currency CRE ON EXR.FromCurrencyCode = CRE.Id

        //                                                        WHERE ISNULL(SPC.SlrProcMstSystemID,'')  IN(" + inSalaryProcParam + @")) EmpSlr 

        //                                            INNER JOIN EmployeeInformation EEI ON EEI.SystemId = EmpSlr.EmpInfoSystemID

        //                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID

        //                                        LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID  AND SRG.SalaryHeadID = EmpSlr.SalaryHeadID
        //                                        LEFT JOIN(SELECT* FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId = '" + plantId + @"') PSH
        //                                                                       ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
        //                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = EmpSlr.SalaryHeadID

        //                                                WHERE EEI.GroupID = '" + companyGroupId + @"' AND  EmpSlr.PlantId = '" + plantId + @"' " + _wcPayrollGroup + @" ";
        //                if (parameters != null)
        //                {
        //                    if (parameters.Count > 0)
        //                    {
        //                        if (parameters.Keys.ElementAt(0) != "")
        //                        {
        //                            strSQL += @" AND EmpSlr.EmpInfoSystemID IN(" + parameters["EmpSystemId"] + ")";
        //                        }
        //                    }
        //                }

        //                strSQL += "ORDER BY EmpSlr.EmpInfoSystemID,Sequence";

        //                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
        //                con.getDataSet(strSQL, out dsRef);

        //                distinctSalaryHead = dsRef.Tables[0].DefaultView.ToTable(true, "SalaryHeadID", "SalaryHead", "SalaryHeadBangla", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo", "IsGrossComponent", "IsCTCComponent", "PartOfNetPay");
        //                distinctSalaryHead.DefaultView.Sort = "Sequence";
        //                distinctSalaryHead = distinctSalaryHead.DefaultView.ToTable();

        //                DataTable dt = dsRef.Tables[0];
        //                List<DataRow> _data = new List<DataRow>();
        //                string empId = "";
        //                for (int i = 0; i < dt.Rows.Count; i++)
        //                {
        //                    if (empId != dt.Rows[i]["EmpInfoSystemID"].ToString())
        //                    {
        //                        _data = new List<DataRow>();
        //                        dicBonus.Add(dt.Rows[i]["EmpInfoSystemID"].ToString(), _data);
        //                    }
        //                    _data.Add(dt.Rows[i]);

        //                    empId = dt.Rows[i]["EmpInfoSystemID"].ToString();
        //                }
        //                return dicBonus;
        //            }
        //            catch (Exception ex)
        //            {
        //                throw (ex);
        //            }
        //            finally
        //            {
        //                //objCon = null;
        //            }
        //        }//End Function

        //        #endregion

        //        public void GetEmployeeInfoDetailCom(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string languageId, string stringSalaryRegSorting, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool sa, bool ca, string userId, out DataSet dsRef)
        //        {
        //            string strSQL;
        //            ConnectionManager.DAL.ConManager objCon;
        //            string sql1 = "select * From ComplianceAttendanceSetting where PlantId ='" + plantId + @"' ";
        //            DataTable dtValidation = _sqlRepository.GetDataTable(sql1);

        //            string wcBasedOnSetting = "1 = 1 ";
        //            string OTCase = "";

        //            if (bplib.clsWebLib.GetBoolData(dtValidation.Rows[0]["IsNoPunchOnWeekOffForOTEntitle"]))
        //            {
        //                wcBasedOnSetting += "AND (DT.Category IN ('Present','Late','Half Day') AND DT.OriginalDayType != 'W')";
        //            }
        //            else
        //            {
        //                OTCase += @"WHEN (Category IN ('Present','Late','Half Day') AND OriginalDayType = 'W') THEN FOT.TotalOTHr";
        //            }
        //            if (bplib.clsWebLib.GetBoolData(dtValidation.Rows[0]["IsNoPunchOnHolidayForOTEntitle"]))
        //            {
        //                wcBasedOnSetting += @"AND (DT.Category IN ('Present','Late','Half Day') AND DT.OriginalDayType != 'H')";
        //            }
        //            else
        //            {
        //                OTCase += " WHEN ( Category IN ('Present','Late','Half Day') AND OriginalDayType = 'H') THEN FOT.TotalOTHr";

        //            }

        //            var _wc = string.Empty;
        //            try
        //            {
        //                string wcEmpStatus = " Where (1=0 ";

        //                if (isActive == true && isSeperated == true && isMaternity == true)
        //                {
        //                    wcEmpStatus = " Where (1=1 ";
        //                }
        //                else
        //                {
        //                    if (isActive == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
        //                    }
        //                    if (isSeperated == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
        //                    }
        //                    if (isMaternity == true)
        //                    {
        //                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

        //                    }
        //                }

        //                wcEmpStatus += ")";


        //                var wcPayrollGroup = "";
        //                DataTable dtslProcId = _sqlRepository.GetDataTable(@"SELECT SystemID FROM SalaryProcMaster
        //                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
        //                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
        //                                        AND MonthNo =  MONTH('" + toDate + @"') AND YearNo =  YEAR('" + toDate + @"') ");
        //                string inSalaryProcParam = "' '";

        //                for (int i = 0; i < dtslProcId.Rows.Count; i++)
        //                {
        //                    inSalaryProcParam += ",'" + dtslProcId.Rows[i]["SystemID"].ToString() + "'";
        //                }


        //                // wcSalaryProcess = @" AND SPC.SlrProcMstSystemID IN( " + inSalaryProcParam + @" )";

        //                if (sa == true || ca == true)
        //                {
        //                    wcPayrollGroup = @"";
        //                }
        //                else
        //                {
        //                    string inPayrollGroup = "' '";
        //                    DataTable dtPayRollGrpEmpId = _sqlRepository.GetDataTable("SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"') AND PlantID = '" + plantId + @"'");

        //                    DataTable dtNotPayRollGrpEmpId = _sqlRepository.GetDataTable("SELECT SystemId FROM EmployeeInformation E WHERE SystemId NOT IN (SELECT employeeid from MST.PayrollGroupMaster where PlantID = '" + plantId + @"')  AND E.PlantID = '" + plantId + @"'");

        //                    if (dtPayRollGrpEmpId.Rows.Count > 0)
        //                    {
        //                        for (int i = 0; i < dtPayRollGrpEmpId.Rows.Count; i++)
        //                        {
        //                            inPayrollGroup += ",'" + dtPayRollGrpEmpId.Rows[i]["employeeid"].ToString() + "'";
        //                        }
        //                        if (dtNotPayRollGrpEmpId.Rows.Count > 0)
        //                        {
        //                            for (int i = 0; i < dtNotPayRollGrpEmpId.Rows.Count; i++)
        //                            {
        //                                inPayrollGroup += ",'" + dtNotPayRollGrpEmpId.Rows[i]["SystemId"].ToString() + "'";
        //                            }
        //                        }
        //                    }
        //                    wcPayrollGroup = @"AND E.SystemId  IN (" + inPayrollGroup + @")";
        //                }

        //                strSQL = @"SELECT * FROM (SELECT DISTINCT  E.SystemID,  E.EmployeeCode , E.EmployeeName,ISNULL(E.EmployeeNameLocal,E.EmployeeName) EmployeeNameLocal,E.FatherName,ISNULL(SPC.DisbusmentAmount,0) GrossAmount,  Format(E.DOJ,'dd-MMM-yyy') DOJ, Format(E.DOB,'dd-MMM-yyy') DOB,Format(E.DOS,'dd-MMM-yyy') DOS, E.EmployeeStatus,SPLD.PaymentMode,
        //											--DG.UserName DesignationGroupName
        //											 E.DesignationSystemID, --GVDE.UserName GivenDesignationName,
        //											'' UserGroupSystemID, E.PlantID, F.UserName PlantName, F.Sequence PlantSequence, E.UnitID
        //											,Unit.UserName UnitName,Unit.Sequence UnitSequence, Division.Id DivisionID,
        //											 Division.UserName DivisionName,Division.Sequence DivisionSequence
        //											,Department.Id DepartmentID, Department.UserName DepartmentName,Department.Sequence DepartmentSequence,
        //											Section.Id SectionID, Section.UserName SectionName,Section.Sequence SectionSequence,
        //											 SubSection.Id SubSectionID, SubSection.UserName SubSectionName,SubSection.Sequence SubSectionSequence,EC.Id EmployeeCategorySystemID
        //											,EC.UserName EmpCategoryName,EC.Sequence EmployeeCategorySequence--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
        //                                            ,ENT.UserName EntitySequence,e.SalaryRuleMasterSystemID,L.UserName Line, LD.UserName LegalDesignation--,eoe.IsOTEntitle
        //										   ,ISNULL(LD.Id,'') DesignationId,LD.UserName DesignationName,LD.Sequence DesignationSequence,ISNULL(EC.Id,'') EmployeeCategoryId,ISNULL(EC.UserName,'') EmployeeCategoryName
        //                                           , ISNULL(LD.UserName,'') LDDesignationGD,LSalGr.Code GradeCode,E.EmployeeCodePreFix, E.EmployeeCodeNumeric
        //										   ,CASE WHEN MONTH(DOS) =  MONTH('" + toDate + @"')  AND YEAR(DOS) = YEAR('" + toDate + @"') THEN 'Separated' else 'Active' end CurrentMonthEmployeeStatus
        //                                           , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
        //		                                   ,ISNULL(LocLangLD.Name,LD.UserName) DesignationLocal
        //										   ,E.LineId,L.Sequence LineSequence, esic.ESICNo,pf.UANNo,SPLD.BankAccNo,bb.BankName, '' BankNameFull
        //                                           ,MMDSA.*
        //                                           FROM EmployeeInformation E
        //												LEFT OUTER JOIN (Select EmpInfoSystemID,DisbusmentAmount,SlrProcMstSystemID from SalaryProcChild SPC Left join SalaryHead SH ON SH.SalaryHeadID = SPC.SalaryHeadID where HeadCategory = 'GROSS') SPC ON SPC.EmpInfoSystemID = E.SystemId
        //                                            LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID and spm.MonthNo = Month('" + toDate + @"') and spm.YearNo = Year('" + toDate + @"')
        //										        INNER JOin SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId
        //												LEFT JOIN org.Plant F ON SPLD.PlantID = F.Id
        //									            LEFT JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = SPLD.BudgetCode
        //								                LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
        //    								LEFT OUTER JOIN ORG.Position PO ON MB.PositionId=PO.Id
        //                                    LEFT OUTER JOIN ORG.Entity EN ON MB.EntityId=EN.Id
        //                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
        //                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
        //                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
        //                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
        //                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
        //                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
        //                                    LEFT JOIN ORG.Line L ON L.Id = MB.LineId
        //												LEFT JOIN HKP.LegalDesignation LD ON LD.Id=SPLD.LegalDesignationId
        //												LEFT JOIN HKP.Designation GVD ON GVD.Id=E.GivenDesignationId
        //                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LD.Id and E.PlantId = LSGD.PlantId
        //                                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = SPLD.LegalSalaryGradeId  --and E.PlantId = LSalGr.PlantId
        //                                    LEFT JOIN [HKP].EmployeeCategory EC ON EC.Id = SPLD.EmployeeCategoryId
        //                                                LEFT JOIN HKP.LocalLanguage LocLangLD ON LocLangLD.LegalDesignationId = SPLD.LegalDesignationId AND LocLangLD.LanguageId = '" + languageId + @"'
        //												 INNER JOIN
        //		                                    (
        //												SELECT MMDSA.EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
        //										,ISNULL(TotalLv,0) TotalLv
        //										,ISNULL(TotalMLv,0) TotalMLv,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv,ISNULL(TotalWeekOff,0) + ISNULL(TotalWeekOffHoliDay,0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
        //                                        ,ISNULL(OT.TotalOTHr,0) TotalOTHr,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr,ISNULL(WeekOffOTHr,0) WeekOffOTHr
        //										,ISNULL(HoliDayOTHr,0) HoliDayOTHr,ISNULL(TotalLWP,0) TotalLWP,ISNULL(IsOTEntitled,0) IsOTEntitled,ISNULL(OTRate,0) OTRate,ISNULL(TotalHoliDay,0) TotalHoliDay
        //										  FROM SalaryProceAttdnData MMDSA
        //											LEFT JOIN 
        //											(SELECT SUM(TOT) TotalOTHr,EmpSystemID,PlantID FROM (
        //                                            SELECT 
        //                                            CASE " + OTCase + @"
        //                            WHEN FOT.TotalOTHr > CAS.MaxOTPerDay then CAS.MaxOTPerDay 

        //                                                else FOT.TotalOTHr end TOT
        //                                            ,FOT.EmpSystemID,FOT.WorkDate,FOT.PlantID,FOT.TotalOTHr
        //                                             FROM FinalOT FOT LEFT JOIN 
        //                                            ComplianceAttendanceSetting CAS ON CAS.CompanyGroupId = FOT.GroupID  AND CAS.PlantID = '" + plantId + @"'
        //	                                        LEFT JOIN AttdnProcessData APD  ON APD.WorkDate = FOT.WorkDate and apd.EmpSystemID = FOT.EmpSystemID
        //											LEFT JOIN DayType DT  ON DT.DayType = APD.DayStatus 
        //                                            where " + wcBasedOnSetting + @"
        //                                            ) dd
        //                                            WHERE WorkDate BETWEEN '" + fromDate + @"' and '" + toDate + @"' and PlantID = '" + plantId + @"'
        //                                            GROUP BY EmpSystemID,PlantID ) OT ON OT.EmpSystemID = MMDSA.EmpSystemID
        //                                            WHERE MMDSA.MonthNo = MONTH('" + toDate + @"') AND
        //						                               MMDSA.YearNo = YEAR('" + toDate + @"')
        //											) MMDSA ON E.SystemId = MMDSA.EmpSystemID 
        //											    LEFT JOIN 
        //                                           		(
        //                                           		 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
        //                                           			FROM EmployeeInformation E   
        //                                           					LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
        //                                           					LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
        //                                                                                               AND E.PlantId = gd.PlantId
        //                                           					LEFT JOIN (
        //                                           								SELECT MAX(EffectiveDate)EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
        //                                           									FROM MST.LegalSalaryStructure 
        //                                           									WHERE EffectiveDate <= '" + toDate + @"'
        //                                           								GROUP BY LegalSalaryGradeId, EmployeeLocationId 
        //                                           							  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
        //                                           					LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
        //                                                                                           AND SS.EmployeeLocationId = S.EmployeeLocationId 
        //                                                                                           AND SS.EffectiveDate = S.EffectiveDate
        //                                           					LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 																							
        //                                           					left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId
        //                                           			GROUP BY E.SystemId,LSG.UserName 
        //                                           		) MW ON MW.SystemId = E.SystemId
        //                                                LEFT JOIN (
        //                                                SELECT bb.UserName BankName,b.BankAccNo,b.EmpSystemID FROM [dbo].[EmployeeBankInfo] b
        //                                                LEFT JOIN hkp.BankBranch bb ON b.BankBranchId=bb.Id
        //                                                ) BB ON BB.EmpSystemID = E.SystemId
        //                                                LEFT JOIN
        //												( SELECT ed.DocNumber UANNo,ED.EmpSystemID FROM 
        //												EmployeeDocument ED 
        //												INNER JOIN (SELECT * FROM HKP.ComplianceDocument WHERE ProfileType = 'PF') CD ON CD.Id = ED.ComplianceDocumentId 
        //												) pf ON E.SystemId = pf.EmpSystemID
        //												LEFT JOIN
        //												( SELECT ed.DocNumber ESICNo,ED.EmpSystemID FROM 
        //												EmployeeDocument ED 
        //												INNER JOIN (SELECT * FROM HKP.ComplianceDocument WHERE ProfileType = 'ESIC') CD ON CD.Id = ED.ComplianceDocumentId 
        //												) ESIC ON E.SystemId = esic.EmpSystemID 
        //                                    WHERE  E.GroupID='" + companyGroupId + @"' and E.CompanyId  = '" + companyId + @"' AND E.PlantId='" + plantId + @"'
        //                                    " + wcPayrollGroup + @"
        //                                               AND SPC.SlrProcMstSystemID IN( " + inSalaryProcParam + @"
        //                                            )";

        //                if (parameters.Count > 0)
        //                {
        //                    if (parameters.Keys.ElementAt(0) != "")
        //                    {
        //                        strSQL += @" AND E.SystemID IN(" + parameters["EmpSystemId"] + ")";
        //                    }
        //                }
        //                strSQL += @")dd " + wcEmpStatus + @"";

        //                strSQL += stringSalaryRegSorting.Replace("EmpBasic.", "");


        //                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
        //                con.getDataSet(strSQL, out dsRef);


        //            }
        //            catch (Exception ex)
        //            {
        //                throw (ex);
        //            }
        //            finally
        //            {
        //                objCon = null;
        //            }
        //        }//End Function

        //        public void GetEmpLeaveInfo(ParamList leavePara, Dictionary<string, string> parameters, out Dictionary<string, List<DataRow>> dicLeave)
        //        {
        //            var paraDate = Convert.ToDateTime(leavePara.FromDate);
        //            dicLeave = null;
        //            try
        //            {

        //                var days = DateTime.DaysInMonth(paraDate.Year, paraDate.Month);//Number of Days in a month
        //                string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(paraDate.Month);//Month Name from Month No
        //                var lastDate = days + "-" + monthNameString + "-" + paraDate.Year;
        //                var firstDate = "1" + "-" + monthNameString + "-" + paraDate.Year;
        //                DataSet dsRef = null;
        //                string strSQL = string.Empty;
        //                clsStaticInfo obs = null;
        //                dicLeave = new Dictionary<string, List<DataRow>>();

        //                obs = new clsStaticInfo();
        //                strSQL = @"SELECT 
        //	                        LTR.EmpSystemID,LT.Code,LT.LeaveType
        //	                        ,SUM(LTD.LeaveDuration) AvailedLeave	                     
        //                             FROM LeaveTransaction LTR                              
        //                            Inner   JOIN LeaveType LT ON  LTR.LTSystemID = LT.Id 
        //							inner JOIN LeaveTransactionDetails LTD ON LTD.LvTrnsSystemID =LTR.SystemID 
        //							LEFT JOIN AttdnProcessData APD ON --APD.LTSystemID = LT.Id  and 

        //							APD.EmpSystemID = LTR.EmpSystemID AND FORMAT( apd.WorkDate,'dd-MMM-yyyy')= FORMAT(ltd.WorkDate,'dd-MMM-yyyy')
        //                            Left Join DayType DT ON DT.DayType = APD.DayStatus
        //							WHERE LTD.IsAvailed = 1 AND LTR.IsApproved = 1 
        //							AND DT.Category = 'Leave' 
        //							AND LTD.WorkDate BETWEEN '" + firstDate + @"' AND  '" + lastDate + @"' ";
        //                if (parameters.Count > 0)
        //                {
        //                    if (parameters.Keys.ElementAt(0) != "")
        //                    {
        //                        strSQL += @" AND LTR.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";
        //                    }
        //                }

        //                strSQL += " GROUP BY LTR.EmpSystemID,LT.Code,LT.LeaveType";


        //                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
        //                con.getDataSet(strSQL, out dsRef);
        //                DataTable dt = dsRef.Tables[0];
        //                List<DataRow> _data = new List<DataRow>();
        //                string empId = "";
        //                string leaveCode = "";
        //                for (int i = 0; i < dt.Rows.Count; i++)
        //                {
        //                    if (empId != dt.Rows[i]["EmpSystemID"].ToString())
        //                    {
        //                        _data = new List<DataRow>();
        //                        if (dicLeave.ContainsKey(dt.Rows[i]["EmpSystemID"].ToString()) == false)
        //                        {
        //                            dicLeave.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
        //                        }
        //                    }


        //                    _data.Add(dt.Rows[i]);
        //                    empId = dt.Rows[i]["EmpSystemID"].ToString();
        //                    leaveCode = dt.Rows[i]["Code"].ToString();
        //                }

        //                //return dicLeave;
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //            finally
        //            {

        //            }
        //        }//End Function  
    }
}