using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using Library.ViewModel.HR;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Leave
{
    public class clsLeaveBalanceToDate
    {
        ISqlRepository _sqlRepository;
        public clsLeaveBalanceToDate()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetEmp(string plantId, string companyId, string calYearId, string ToDate)
        {
            string _FromDate = string.Empty;
            string _ToDate = ToDate;

            var startFromDate = Convert.ToDateTime(ToDate);
            var y = startFromDate.Year;
            _FromDate = "1-Jan-" + y;

            var dsCalYear = GetCalYearInfo(calYearId);
            if (dsCalYear.Tables[0].Rows.Count > 0)
            {
                _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                // _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
            }
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT p.Id PlantId,p.UserName Plant,Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.BudgetCode,E.UserName EntityName,LGD.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        , L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodeNumeric, EMP.FatherName,FORMAT( EMP.DOB,'dd-MMM-yyyy')DOB,ec.UserName EmpCategory,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        Left join ORG.Plant p on p.Id = emp.PlantId
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EMP.LegalDesignationId
										LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
										left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
										 left join HKP.Designation DeG on DeG.Id=dm.DesignationId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                        WHERE emp.PlantID in (" + plantId + @")  and EMP.CompanyId='" + companyId + @"' and EMP.EmployeeStatus='Active' 
                                        and EMP.DOJ <= ( '" + _ToDate + @"') and (emp.DOS is null or emp.DOS >= '" + _FromDate + @"')
                                        ORDER BY EmployeeCodePreFix,EMP.EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IWorkbook XlsLeaveBalanceRpt(string PlantId, string sGroup, string Year, string ToDate, string empIds)
        {
            clsReport objRpt = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            objRpt = new clsReport();
            var workbook = report.GetWorkbook(ref excelEngine, 1);

            string _FromDate = string.Empty;
            string _ToDate = ToDate;
            string _YearNo = string.Empty;
            var startFromDate = Convert.ToDateTime(ToDate);
            var y = startFromDate.Year;
            _FromDate = "1-Jan-" + y;
            // var esic = GetESICEligibleEmployee(EmpSystemID);
            var dsCalYear = GetCalYear(Year);
            if (dsCalYear.Tables[0].Rows.Count > 0)
            {
                _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                //  _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
                _YearNo = dsCalYear.Tables[0].Rows[0]["YearNo"].ToString();
            }

            workbook.Version = ExcelVersion.Excel2016;
            objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
            objRpt.SelectedPlant(identity.PlantId, out dsFactory);
            var sheet = workbook.Worksheets[0];

            sheet.Name = "LeaveRegisterReport";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;
            var dsLvAllo = GetLeaveBalanceTypeData(sGroup, PlantId, Year, _ToDate, empIds);

            // var finalList = LoadGrdAllocatedLvDetails(dsLvAllo);

            #region Headers


            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 12, ExcelHAlign.HAlignLeft);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 25, ExcelHAlign.HAlignLeft);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 12, ExcelHAlign.HAlignLeft);
            int ColPlant = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DOJ", 10, ExcelHAlign.HAlignLeft);
            int ColDoj = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 12, ExcelHAlign.HAlignLeft);
            int ColLegalDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 20, ExcelHAlign.HAlignLeft);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Cat.", 10, ExcelHAlign.HAlignLeft);
            int ColEmployeeCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Leave Name", 12, ExcelHAlign.HAlignLeft);
            int ColLeaveName = COL;
            COL++;
            //report.SetHeaderText(ref sheet, ROW, COL, "Brought Forward", 10, ExcelHAlign.HAlignRight);
            //int ColBroughtForward = COL;
            //COL++;
            //report.SetHeaderText(ref sheet, ROW, COL, "Carry Forward OB", 10, ExcelHAlign.HAlignRight);
            //int ColCarryForward = COL;
            //COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Opeaning Balance(" + _FromDate + ")", 10, ExcelHAlign.HAlignRight);
            int ColOpeningBalance = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Allocated (As on " + ToDate + ")", 10, ExcelHAlign.HAlignRight);
            int ColCurrentAllocation = COL;
            COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Leave Days Allowed", 10, ExcelHAlign.HAlignRight);
            //int ColLeaveDays = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Applied", 10, ExcelHAlign.HAlignRight);
            int ColApplied = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Availed", 10, ExcelHAlign.HAlignRight);
            int ColAvailed = COL;
            COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Encashed", 10, ExcelHAlign.HAlignRight);
            //int ColEncashedInbetween = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Closing Balance(" + ToDate + ")", 10, ExcelHAlign.HAlignRight);
            int ColBalance = COL;

            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
            sheet.Range[ROW, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true;

            endCol = COL;
            #endregion Headers

            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;
            ROW++;

            foreach (DataRow item in dsLvAllo.Rows)
            {

                sheet[ROW, ColEmployeeCode].Text = item["EmployeeCode"].ToString();
                sheet[ROW, ColEmployeeName].Text = item["EmployeeName"].ToString();
                sheet[ROW, ColPlant].Text = item["PlantName"].ToString();
                sheet[ROW, ColDoj].Text = item["DOJ"].ToString();
                sheet[ROW, ColLegalDesignation].Text = item["Designation"].ToString();
                sheet[ROW, ColDepartment].Text = item["Department"].ToString();
                sheet[ROW, ColEmployeeCategory].Text = item["EmployeeCategory"].ToString();
                sheet[ROW, ColLeaveName].Text = item["LeaveName"].ToString();
                //sheet[ROW, ColBroughtForward].Number = clsStaticInfo.dbl(item["BroughtForward"].ToString());
                //sheet[ROW, ColCarryForward].Number = clsStaticInfo.dbl(item["CarryForwardOpeningBalance"].ToString());
                //sheet[ROW, ColOpeningBalance].Number = clsStaticInfo.dbl(item["BroughtForward"].ToString()) + clsStaticInfo.dbl(item["CarryForwardOpeningBalance"].ToString());
                sheet[ROW, ColOpeningBalance].Number = clsStaticInfo.dbl(item["BroughtForward"].ToString());

                sheet[ROW, ColCurrentAllocation].Number = clsStaticInfo.dbl(item["CurrentYearAllocation"].ToString());
                //sheet[ROW, ColLeaveDays].Number = clsStaticInfo.dbl(item["DaysCanBeSanctioned"].ToString());
                sheet[ROW, ColApplied].Number = clsStaticInfo.dbl(item["AppliedDays"].ToString());
                sheet[ROW, ColAvailed].Number = clsStaticInfo.dbl(item["AvailedDays"].ToString());
                sheet[ROW, ColBalance].Number = clsStaticInfo.dbl(item["Balance"].ToString());
                // sheet[ROW, ColEncashedInbetween].Number = clsStaticInfo.dbl(item["YearEndEncash"].ToString());
                //sheet[ROW, ColBalance].Formula = clsStaticInfo.GetxlsCol(ColOpeningBalance) + ROW + "+" + clsStaticInfo.GetxlsCol(ColCurrentAllocation) + ROW
                // + "-" + clsStaticInfo.GetxlsCol(ColAvailed) + ROW;

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }

            sheet.UsedRange.NumberFormat = clsStaticInfo.NumberFormat(2);

            #region Line Setup

            sheet.Range[6, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[6, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[6, 1, ROW - 1, endCol].WrapText = true;

            #endregion Line Setup

            #region ******************Report Header******************
            try
            {
                string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), identity.CompanyId + ".jpg");  // IDCardEng.xlsx
                Image companyLogo = Image.FromFile(strPath);
                if (companyLogo != null)
                {
                    double totalWidth = sheet.GetColumnWidth(1) + sheet.GetColumnWidth(2);
                    int totalWidthPixel = (int)(totalWidth * 7.25);
                    int totalheight = (int)((sheet.GetRowHeight(1) + sheet.GetRowHeight(2) + sheet.GetRowHeight(3) + sheet.GetRowHeight(3)) * 1.50);

                    companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                    IPictureShape pic = null;

                    pic = sheet.Pictures.AddPicture(1, 1, companyLogo);

                }


            }
            catch (Exception)
            {


            }

            ROW = 1;
            COL = 1;

            string FactoryName = string.Empty;
            string CmpName = "";
            string FactoryAddress = string.Empty;
            int SheetIndex = 0;
            if (dsCmp.Tables[0].Rows.Count > 0)
            {
                CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
            }
            else
            {
                CmpName = "";
            }
            sheet.Range[ROW, 3].Text = CmpName;
            sheet.Range[ROW, 3, COL, endCol].Merge();
            sheet.Range[ROW, 3].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 3].CellStyle.Font.Size = 12;
            sheet.Range[ROW, 3, COL, endCol].RowHeight = 20;
            sheet.Range[ROW, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[ROW, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, 3, COL, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            ROW += 1;
            if (dsFactory.Tables[0].Rows.Count > 0)
            {

                FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
            }
            else
            {
                FactoryName = "";
            }
            sheet.Range[ROW, 3].Text = FactoryName;
            sheet.Range[ROW, 3, ROW, endCol].Merge();
            sheet.Range[ROW, 3].CellStyle.Font.Size = 10;
            sheet.Range[ROW, 3, ROW, endCol].RowHeight = 20;
            sheet.Range[ROW, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[ROW, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, 3, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            ROW += 1;
            if (dsFactory.Tables[0].Rows.Count > 0)
            {
                FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
            }
            else
            {
                FactoryAddress = "";
            }
            sheet.Range[ROW, 3].Text = FactoryAddress;
            sheet.Range[ROW, 3, ROW, endCol].Merge();
            sheet.Range[ROW, 3].CellStyle.Font.Size = 22;
            sheet.Range[ROW, 3, ROW, endCol].RowHeight = 17;
            sheet.Range[ROW, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[ROW, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, 3, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            ROW += 1;
            sheet.Range[ROW, 3].Text = "Leave Register Report: " + _FromDate + " To " + _ToDate;
            sheet.Range[ROW, 3, ROW, endCol].Merge();
            sheet.Range[ROW, 3].CellStyle.Font.Size = 10;
            sheet.Range[ROW, 3, ROW, endCol].RowHeight = 20;
            sheet.Range[ROW, 3].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[ROW, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, 3, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            #endregion ******************Report Header******************

            #region Freeze Panes

            sheet.IsDisplayZeros = false;
            sheet.UsedRange["A7"].FreezePanes();
            sheet.FirstVisibleColumn = 1;
            sheet.FirstVisibleRow = 6;

            #endregion Freeze Panes

            #region UsedRange Alignment

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            sheet.Range["A1"].CellStyle.Font.Size = 14;
            sheet.Range["A2"].CellStyle.Font.Size = 10;
            sheet.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

            #endregion UsedRange Alignment

            #region Page Setup
            sheet.PageSetup.TopMargin = 0.5;
            sheet.PageSetup.BottomMargin = 0.7;
            sheet.PageSetup.PrintTitleRows = "$1:$5";
            sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
            sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
            sheet.PageSetup.LeftMargin = 0.5;
            sheet.PageSetup.RightMargin = 0.2;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
            sheet.PageSetup.FitToPagesTall = 0;
            sheet.PageSetup.FitToPagesWide = 1;
            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.IsDisplayZeros = false;
            #endregion Page Setup

            return workbook;
        }

        #region Leave Balance
        public IEnumerable<EmployeeDetails> LoadGrdAllocatedLvDetails(DataSet dsLvAllo)
        {
            //DataSet dsLocal = null;
            //DataRow drLocal = null;
            //DataView dvLocal = null;
            try
            {

                //dvLocal = new DataView();
                //dvLocal.Table = dsLvAllo.Tables[0];
                //bool proDataPrevYear = false;
                bool proDataCurrentYear = false;
                bool isAvailExceptionAllowed = false;
                List<EmployeeDetails> _rt = new List<EmployeeDetails>();

                var list_loop = new List<EmployeeDetailsVM>();
                list_loop = dsLvAllo.Tables[0].ToList<EmployeeDetailsVM>();

                object ob = new object { };

                for (int i = 0; i < list_loop.Count; i++)
                {
                    EmployeeDetailsVM _ob_source = list_loop[i];
                    EmployeeDetails _ob_r = new EmployeeDetails();
                    //dvLocal.RowFilter = "LvPolDetailsSystemID = '" + dsLvAllo.Tables[0].Rows[i]["LvPolDetailsSystemID"].ToString().Trim() + "'";
                    //dvLocal.RowFilter = "SystemID = '" + list_loop[i].SystemID.ToString().Trim() + "'";
                    //if (dvLocal.Count == 1)
                    //{
                    //drLocal = dvLocal[0].Row;
                    //drLocal.BeginEdit();
                    //proDataPrevYear = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsProrataPreviousyear"].ToString());

                    proDataCurrentYear = Convert.ToBoolean(_ob_source.IsProratacurrentyear);
                    //proDataCurrentYear = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsProratacurrentyear"].ToString());

                    //isAvailExceptionAllowed = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsAvailExceptionAllowedOnSpecialAppeal"].ToString());
                    isAvailExceptionAllowed = Convert.ToBoolean(_ob_source.IsAvailExceptionAllowedOnSpecialAppeal);
                    //drLocal["EmployeeCode"] = dsLvAllo.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim();
                    _ob_r.EmployeeCode = _ob_source.EmployeeCode;
                    _ob_r.EmployeeName = _ob_source.EmployeeName;
                    _ob_r.PlantName = _ob_source.PlantName;
                    _ob_r.DOJ = _ob_source.DOJ;
                    //drLocal["EmployeeName"] = dsLvAllo.Tables[0].Rows[i]["EmployeeName"].ToString().Trim();
                    _ob_r.Designation = _ob_source.Designation;
                    //drLocal["Designation"] = dsLvAllo.Tables[0].Rows[i]["Designation"].ToString().Trim();
                    _ob_r.Department = _ob_source.Department;
                    _ob_r.CurrentAllocation = _ob_source.CurrentAllocation;
                    //drLocal["Department"] = dsLvAllo.Tables[0].Rows[i]["Department"].ToString().Trim();
                    _ob_r.EmployeeCategory = _ob_source.EmployeeCategory;
                    _ob_r.LeaveName = _ob_source.LeaveName;
                    //drLocal["EmployeeCategory"] = dsLvAllo.Tables[0].Rows[i]["EmployeeCategory"].ToString().Trim();
                    _ob_r.Applied = _ob_source.Applied;
                    //drLocal["Applied"] = dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim();
                    _ob_r.Availed = _ob_source.Availed;
                    _ob_r.BroughtForward = Convert.ToDecimal(_ob_source.BroughtForward);
                    decimal DaysCanBeSanctioned = 0;

                    decimal BroughtForward = Convert.ToDecimal(_ob_source.BroughtForward);
                    DaysCanBeSanctioned = Convert.ToDecimal(_ob_source.DaysCanBeSanctioned);

                    decimal EncashedInbetween = 0;
                    if (!string.IsNullOrEmpty(_ob_source.EncashedInbetween.ToString()))
                    {
                        EncashedInbetween = Convert.ToDecimal(_ob_source.EncashedInbetween);
                    }
                    _ob_r.EncashedInbetween = EncashedInbetween;
                    bool IsBroughtForwardAdd = true;
                    IsBroughtForwardAdd = Convert.ToBoolean(_ob_source.IsBroughtForwardAdd.ToString());
                    decimal TotalEarn = 0;
                    if (IsBroughtForwardAdd)
                    {
                        TotalEarn = BroughtForward + DaysCanBeSanctioned;
                    }
                    else
                    {
                        TotalEarn = DaysCanBeSanctioned;
                    }


                    //if (_ob_source.LeaveType.ToString().Trim().ToUpper() != "EARN")
                    //{
                    //    if (proDataCurrentYear == false)
                    //    {
                    #region 01
                    if (IsBroughtForwardAdd)
                    {
                        _ob_r.Balance = Convert.ToDecimal(_ob_source.CurrentAllocationDCBS) + BroughtForward - Convert.ToDecimal(_ob_source.Availed) - _ob_r.EncashedInbetween;

                        _ob_r.LeaveDays = Convert.ToDecimal(_ob_r.Balance) - Convert.ToDecimal(_ob_source.Applied);
                        //TotalEarn = BroughtForward + DaysCanBeSanctioned;
                    }
                    else
                    {
                        //TotalEarn = DaysCanBeSanctioned;
                        _ob_r.Balance = Convert.ToDecimal(_ob_source.CurrentAllocationDCBS) - Convert.ToDecimal(_ob_source.Availed) - _ob_r.EncashedInbetween;
                        _ob_r.LeaveDays = Convert.ToDecimal(_ob_r.Balance) - Convert.ToDecimal(_ob_source.Applied);

                    }
                    //_ob_r.LeaveDays = Convert.ToDecimal(_ob_source.CurrentAllocationDCBS);
                    //_ob_r.Balance = Convert.ToDecimal(_ob_source.CurrentAllocationDCBS.ToString().Trim()) - Convert.ToDecimal(_ob_source.Applied.ToString().Trim());

                    #endregion
                    //    }
                    //    else
                    //    {
                    //        #region 02

                    //        _ob_r.LeaveDays = TotalEarn;
                    //        _ob_r.Balance = TotalEarn - Convert.ToDecimal(_ob_source.Applied.ToString().Trim());

                    //        #endregion
                    //    }
                    //}
                    //else
                    //{
                    //    _ob_r.LeaveDays = TotalEarn;
                    //    _ob_r.Balance = TotalEarn - Convert.ToDecimal(_ob_source.Applied.ToString().Trim()) - EncashedInbetween;

                    //}
                    //drLocal.EndEdit();
                    //}

                    _rt.Add(_ob_r);
                }

                //var list = new List<EmployeeDetails>();
                //list = dsLvAllo.Tables[0].ToList<EmployeeDetails>();
                //return list;
                //list = ConvertDataTable<LeaveTransactionVM>(dsLvAllo.Tables[0]);
                return _rt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //dsLvAllo = null;
            }
        }//End Function

        public DataSet GetLeaveBalanceType_BACKUP(string sGroupID, string sPlantID, string calYearId, string ToDate)
        {

            try
            {
                string _FromDate = string.Empty;
                string _ToDate = ToDate;
                string CalToDate = string.Empty;
                var startFromDate = Convert.ToDateTime(ToDate);
                var y = startFromDate.Year;
                _FromDate = "1-Jan-" + y;
                // var esic = GetESICEligibleEmployee(EmpSystemID);
                var dsCalYear = GetCalYearInfo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                    CalToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
                }
                else
                {
                    //throw new Exception("No Year found...");
                }
                #region -- For esic leave --
                //var esic = GetESICEligibleEmployeeFromEnum(EmpSystemID, _FromDate);

                //                if (esic.Tables[0].Rows.Count > 0)
                //                {
                //                    GridParameter parameters = null;
                //                    parameters = new GridParameter
                //                    {
                //                        ExportType = "DATASET",
                //                        CmdText = @"SELECT	els.CalanderYearID,ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed,
                //										 els.Id SystemID,
                //                                         els.LeaveTypeId LTSystemID,
                //                                         els.EmployeeID,
                //										 lt.UserName LeaveName,
                //										 lt.Description LeaveDescription,
                //                                         ltd.SystemID LvPolDetailsSystemID,
                //                                         --ltd.IsProrataPreviousyear,
                //                                         ltd.IsProratacurrentyear,
                //                                         els.DaysCanBeSanctioned, els.EncashedInbetween,
                //                                         ltd.IsAvailExceptionAllowedOnSpecialAppeal,
                //										 0.00 Balance,
                //                                        ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation,
                //                                        ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
                //										 --all carry forward
                //                                         --ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                //                                         BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
                //                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
                //										 --applied +applied ob
                //                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
                //										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                //                                         --0 Availed,
                //										  --Availed +Availed ob
                //                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,
                //										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType

                //-----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
                //---,IsBroughtForwardAdd=CASE WHEN LT.LeaveType='Earn' THEN  
                //,IsBroughtForwardAdd=CASE WHEN 1=1 THEN  
                //	CASE WHEN
                //	-----------------------------------DOJorDOC start -----------------------------------------------------------
                //								CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                //                            										 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
                //																	      WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
                //																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
                //										   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
                //										   							 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
                //																		  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
                //																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
                //										   						END
                //                                       END
                //---------------------------------------DOJorDOC start  end-------------------------------------------------------

                //	> GETDATE() then 
                //		    CONVERT(BIT,0)------No
                //        ELSE  CONVERT(BIT,1) END---Yes
                //ELSE CONVERT(BIT,0) END  ---No

                //----------------------------------------------------------------------------------------------------------------------



                //                                          FROM (select * from trn.EmployeeLeaveSummary where CalanderYearId='" + calYearId + @"' and EmployeeId ='" + EmpSystemID + @"' ) els
                //										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
                //                                        LEFT JOIN EmployeeInformation AS emp ON emp.SystemId  = els.EmployeeId
                //										 left outer join (
                //															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m 
                //                                           where  (FromDate between '" + _FromDate + @"' and '" + _ToDate + @"') and (ToDate between '" + _FromDate + @"' and '" + _ToDate + @"')
                //                                           group by EmpSystemID,LTSystemID
                //														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
                //										 left outer join (
                //																select sum(c) av,EmpSystemID,LTSystemID from
                //																(
                //																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
                //																	left outer join
                //																		(
                //																			select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
                //																			IsAvailed = 1  and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                //                                                                            group by LvTrnsSystemID
                //																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
                //																)x group by EmpSystemID,LTSystemID
                //														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
                //										 left outer join (
                //															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
                //																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where	IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
                //																group by EmpSystemID,LTSystemID
                //														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                //                                         left outer join (select * from dbo.LeavePolicyDetail
                //																 where LPMSystemID =
                //																 (--w
                //																 select LeavePolicyMasterId from 
                //																		 (
                //																				SELECT DC.LeavePolicyMasterId,dm.DesignationId 
                //																										FROM MST.DesignationMaster DM
                //																										LEFT JOIN SCS.DesignationMasterConfiguration DC 
                //																													ON DM.Id=DC.DesignationMasterId
                //																						where dc.plantid='" + sPlantID + @"'

                //																		 ) dm where dm.DesignationId =(select givendesignationId 
                //																									 from dbo.EmployeeInformation 
                //																									 where SystemId='" + EmpSystemID + @"')
                //																	)--w
                //                                                 ) ltd on ltd.LTSystemID = lt.Id
                //                                                WHERE els.EmployeeID = '" + EmpSystemID + @"'                                             
                //                                              AND CalanderYearID = '" + calYearId + @"'
                //                                             AND els.LeaveTypeId IN ( --IN


                //                                            SELECT LT.ID FROM dbo.ESICPolicyLeaveType AS EPLT
                //                  LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                //                  WHERE
                //                  EPLT.LeaveTypeID IN
                //                   (
                //                     SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                //                  LEFT JOIN  (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                //LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId WHERE DC.PlantId='" + sPlantID + @"') AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                //                  LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                //                  WHERE EI.SystemID='" + EmpSystemID + @"' AND EI.GroupID='" + sGroupID + @"' AND EI.PlantID='" + sPlantID + @"'
                //                   )
                //                AND
                //                EPLT.ESICPolicyMasterID IN (
                //                 SELECT DM.ESICPolicyMasterID FROM (SELECT DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                //LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                //WHERE DC.PlantId='" + sPlantID + @"') DM
                //                 WHERE DM.DesignationId IN (
                //                  SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + EmpSystemID + @"'
                //                  )
                //                )

                //                                            				)--IN"
                //                    };
                //                    return _sqlRepository.GetGridData(parameters).Source;
                //                }
                //                else
                //                {
                #endregion
                GridParameter parameters = null;
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"SELECT	els.CalanderYearID,PL.Id PlantId,PL.UserName PlantName,EMP.EmployeeName,EMP.EmployeeCode, D.UserName Designation, DEPT.UserName Department, ec.UserName EmployeeCategory, ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed,
										 els.Id SystemID,format(EMP.DOJ,'dd-MMM-yyyy')DOJ,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID
                                         --ISNULL(ltd.IsProrataPreviousyear,0)IsProrataPreviousyear,
                                         ,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear,
                                         --els.DaysCanBeSanctioned,
                                          ISNULL(ltd.IsAvailExceptionAllowedOnSpecialAppeal,0)IsAvailExceptionAllowedOnSpecialAppeal,
										 0.00 Balance,
                                        ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation
,DaysCanBeSanctioned=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end
 ,CurrentAllocationDCBS=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end,
                                         --ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         --ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                                         BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,els.EncashedInbetween,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType


                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
                            ---,IsBroughtForwardAdd=CASE WHEN LT.LeaveType='Earn' THEN
                            ,IsBroughtForwardAdd=CASE WHEN 1=1 THEN  
                            	CASE WHEN
                            	-----------------------------------DOJorDOC start -----------------------------------------------------------
								CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            										 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
										   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   							 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
																		  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
										   						END
                                       END
---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
	> GETDATE() then 
		    CONVERT(BIT,0)------No
        ELSE  CONVERT(BIT,1) END---Yes
ELSE CONVERT(BIT,0) END  ---No

----------------------------------------------------------------------------------------------------------------------
                                          FROM (select * from trn.EmployeeLeaveSummary where CalanderYearId in (select Id from YearlyCalendar where PlantId in (" + sPlantID + @") and YearNo =year('" + ToDate + "')) and PlantId in (" + sPlantID + @") --and EmployeeId IN( '206835','206828' )
										  ) els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
                            where  (FromDate between '" + _FromDate + @"' and '" + CalToDate + @"') and (FromDate between '" + _FromDate + @"' and '" + ToDate + @"')
                                                    group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 and WorkDate between '" + _FromDate + @"' and '" + ToDate + @"'
                                                                        group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where	IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (--1
										 select d.*,e.SystemId empsystemid from dbo.EmployeeInformation e
inner join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = e.LegalDesignationId
inner join MST.DesignationMaster dm on dm.Id = dml.DesignationMasterId
inner join SCS.DesignationMasterConfiguration DC on DC.DesignationMasterId = dm.Id and dc.PlantId = e.PlantId
inner join LeavePolicyMaster lm on lm.SystemID = dc.LeavePolicyMasterId and e.PlantId=lm.PlantId
inner join dbo.LeavePolicyDetail d on d.LPMSystemID = lm.SystemID


										 --select * from dbo.LeavePolicyDetail
											--					 where LPMSystemID =
											--					 (--w
											--					 select LeavePolicyMasterId from 
											--							 (
											--									SELECT DC.LeavePolicyMasterId,dm.DesignationId 
											--															FROM MST.DesignationMaster DM
											--															LEFT JOIN SCS.DesignationMasterConfiguration DC 
											--																		ON DM.Id=DC.DesignationMasterId
											--											where dc.plantid='202020'

											--							 ) dm where dm.DesignationId =(select givendesignationId 
											--														 from dbo.EmployeeInformation 
											--														 where SystemId IN( '206835','206828' ))
											--						)--w
                                                 )--1
												 ltd on ltd.LTSystemID = lt.Id and ltd.empsystemid=els.EmployeeId

										left JOIN EmployeeInformation AS emp ON emp.SystemId  = els.EmployeeId -- and els.PlantId in (" + sPlantID + @") 
										LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId                                        
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=EMP.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId

                                                WHERE 
												(emp.DOS is null or emp.DOS >= '" + _FromDate + @"') and
												(emp.DOJ <= '" + ToDate + @"') and emp.PlantId in (" + sPlantID + @") 
												--and els.EmployeeID IN( '206835','206828' )
                                              AND 
											  CalanderYearID in(select Id from YearlyCalendar where PlantId in (" + sPlantID + @")  and YearNo =year('" + ToDate + @"'))
                                              --AND els.LeaveTypeId IN 
                                            --(select id from LeaveType where IsGeneral=1 or IsESIC = 1) 
                                            AND lt.LeaveType <>'Maternity' and lt.Code in('CL','PL')
											"
                };
                parameters.sort = "LeaveName";
                parameters.order = "ASC";
                return _sqlRepository.GetGridData(parameters).Source;
                //}

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function
        public DataTable GetLeaveBalanceTypeData(string sGroupID, string sPlantID, string calYearId, string ToDate, string empIds)
        {

            try
            {
                string _FromDate = string.Empty;
                string _ToDate = ToDate;
                string CalToDate = string.Empty;
                var startFromDate = Convert.ToDateTime(ToDate);
                var y = startFromDate.Year;
                _FromDate = "1-Jan-" + y;
                // var esic = GetESICEligibleEmployee(EmpSystemID);
                var dsCalYear = GetCalYearInfo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                    CalToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
                }
                else
                {
                    //throw new Exception("No Year found...");
                }
                string _xsql = @"
                                SELECT ei.SystemId, ei.EmployeeCode,ei.EmployeeName,FORMAT(ei.DOJ,'dd-MMM-yyyy') AS DOJ,p.UserName AS PlantName,D.UserName AS Designation,
                                DEPT.UserName AS Department,ct.UserName AS EmployeeCategory,LT.UserName AS LeaveName,
                                B.CurrentYearAllocation, B.BroughtForward, B.CarryForwardOpeningBalance, B.DaysCanBeSanctioned, B.AppliedDays,
                                B.AvailedDays, B.YearEndEncash,APL.LeaveDuration AS AppliedLeave
                            FROM (		SELECT BAL.EmployeeId, BAL.LeaveTypeId,
								       SUM(BAL.CurrentYearAllocation) AS CurrentYearAllocation,
								        SUM(BAL.BroughtForward) AS BroughtForward,SUM(BAL.CarryForwardOpeningBalance) AS CarryForwardOpeningBalance,   SUM(BAL.DaysCanBeSanctioned) AS DaysCanBeSanctioned,
								        SUM(BAL.AppliedDays) AS AppliedDays,  SUM(BAL.AvailedDays) AS AvailedDays,
								        SUM(BAL.YearEndEncash) AS YearEndEncash
								  FROM (
								SELECT l.EmployeeId, L.LeaveTypeId,Lt.LeaveType,
								CASE WHEN lt.LeaveType='EARN' THEN 0 ELSE ISNULL(l.CurrentYearAllocation,0) END CurrentYearAllocation,l.BroughtForward,l.CarryForwardOpeningBalance,l.DaysCanBeSanctioned,l.AppliedDays,0 AvailedDays,L.YearEndEncash
                                  from EmployeeInformation EI
                                Join YearlyCalendar AS C ON C.PlantId=EI.PlantId
                                JOIN trn.EmployeeLeaveSummary L ON l.CalanderYearId=c.Id and L.EmployeeId=EI.SystemId
                                LEFT JOIN LeaveType AS lt ON lt.Id=l.LeaveTypeId
                                WHERE  '" + _FromDate + @"' BETWEEN c.FromDate AND c.ToDate AND lt.Code IN ('PL','CL')
                                 AND ei.PlantId IN (" + sPlantID + @") 
                                
                                UNION ALL

                                SELECT APD.EmpSystemID, l.LeaveTypeId,Lt.LeaveType,CONVERT(DECIMAL(18,4),CASE WHEN EncashWorkingDaysQty>0 THEN CONVERT(DECIMAL(18,4), EncashEarnLeaveQty)/CONVERT(DECIMAL(18,4),EncashWorkingDaysQty) ELSE 0 END) * l.EarnValue AS ActualEarnedLeave,
                                0 BroughtForward,0 CarryForwardOpeningBalance,0 DaysCanBeSanctioned,0 AppliedDays,l.AvailedValue AS AvailedDays,0 YearEndEncash
                                

                                 FROM AttdnProcessData AS apd
                                LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=apd.EmpSystemID
                                JOIN DayTypeWithValues AS ds ON ds.code=apd.DayStatus AND ds.HeaderId=apd.DayStatusHeaderId
                                JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                                LEFT JOIN LeaveType AS lt ON lt.Id=L.LeaveTypeId

                                 LEFT JOIN LeavePolicyDetail AS lpd ON lpd.LPMSystemID=apd.LeavePolicyMasterId AND lpd.LTSystemID=l.LeaveTypeId
                                WHERE 
                                apd.WorkDate BETWEEN '" + _FromDate + @"' AND '" + ToDate + @"'
                                AND lt.Code IN ('PL','CL') AND ei.PlantId IN (" + sPlantID + @")
								) AS BAL
								GROUP BY BAL.EmployeeId, BAL.LeaveTypeId
								
                    ) AS B
                    LEFT JOIN LeaveType AS lt ON lt.Id=b.LeaveTypeId
                    LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=B.EmployeeId
                    LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                    LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                    LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                    LEFT JOIN hkp.EmployeeCategory CT ON ct.Id=dm.EmployeeCategoryId
                    LEFT JOIN org.Plant AS p ON p.Id=ei.PlantId
                              
                    LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                    LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id         

                    LEFT JOIN (
                                SELECT T.EmpSystemID,T.LTSystemID,SUM(D.LeaveDuration) AS LeaveDuration FROM LeaveTransaction AS T
                                JOIN LeaveTransactionDetails AS D ON d.LvTrnsSystemID=t.SystemID
                                WHERE D.WorkDate>GETDATE() AND D.WorkDate<='" + ToDate + @"'
                                AND T.IsApproved=1 AND t.PlantID IN(" + sPlantID + @") GROUP BY  T.EmpSystemID,T.LTSystemID
                    ) APL ON apl.EmpSystemID=B.EmployeeId AND apl.LTSystemID=B.LeaveTypeId

                    order by ei.EmployeeCode";

                string _sql = @"Select * from (SELECT ei.SystemId,ei.EmployeeCode,ei.EmployeeName,FORMAT(ei.DOJ,'dd-MMM-yyyy') AS DOJ,p.UserName AS PlantName,D.UserName AS Designation,
DEPT.UserName AS Department,ct.UserName AS EmployeeCategory, lt.UserName LeaveName
										 ,	DaysCanBeSanctioned=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end
 ,CurrentYearAllocation=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end,
                                        CurrentAllocation=ISNULL(CASE WHEN LT.LeaveType='Earn' THEN ALD.Opening ELSE ISNULL(els.CurrentYearAllocation, 0) END,0)
										,0 YearEndEncash,''AppliedLeave
										,CarryForwardOpeningBalance=CASE WHEN LT.LeaveType='Earn' THEN	
												ISNULL(ALP.PBroughtForward, 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
                                         BroughtForward=CASE WHEN LT.LeaveType='Earn' THEN	
												ISNULL(ALP.PBroughtForward, 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END
										  ,LeaveDays=(CASE WHEN LT.LeaveType='Earn' THEN	
												ISNULL(ALP.PBroughtForward, 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)+(case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end)
										   ,ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) AppliedDays
										   ,ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) AvailedDays
										 ,Balance=((CASE WHEN LT.LeaveType='Earn' THEN	
												ISNULL(ALP.PBroughtForward, 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)+(case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end))-(ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0))

----------------------------------------------------------------------------------------------------------------------
                                          FROM (    select S.* from trn.EmployeeLeaveSummary S
                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
														where CalanderYearId IN (Select Id from YearlyCalendar  where '" + _FromDate + @"' BETWEEN FromDate AND ToDate)
														AND S.EmployeeId " + empIds + @" AND lp.EncashmentBasis='CalanderYear'

                                                        UNION

                                                        select S.* from trn.EmployeeLeaveSummary S
                                                        JOIN  trn.EmployeeLeaveSummary SS ON S.Id=ss.Id
                                                        AND S.Id=(SELECT TOP 1 SX.Id FROM trn.EmployeeLeaveSummary SX WHERE ss.EmployeeId=SX.EmployeeId AND ss.LeaveTypeId=SX.LeaveTypeId ORDER BY sx.ToDate DESC)
                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
														Join YearlyCalendar AS C ON C.PlantId=EI.PlantId
                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
                                                        where S.EmployeeId " + empIds + @" AND lp.EncashmentBasis<>'CalanderYear') els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId AND lt.Code IN ('PL','CL')
LEFT JOIN
											(
										   select A.Opening,A.EmployeeId,A.LeaveTypeId,FORMAT(LY.FromDate,'dd-MMM-yyyy')FromDate,FORMAT(LY.ToDate,'dd-MMM-yyyy')ToDate from dbo.AnnualLeaveDataCurrent A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
										  LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
											)ALD ON ALD.EmployeeId=els.EmployeeId AND lt.Id=ALD.LeaveTypeId

 LEFT JOIN
											(
										  select PBroughtForward=CASE WHEN A.Opening=0 THEN A.Adjustment ELSE A.Opening END,A.EmployeeId,A.LeaveTypeId from dbo.AnnualLeaveDataPast A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
											)ALP ON ALP.EmployeeId=els.EmployeeId AND lt.Id=ALP.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
                            where  (FromDate between '" + _FromDate + @"' and '" + ToDate + @"') and (ToDate between '" + _FromDate + @"' and '" + ToDate + @"')
                                                    group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 and WorkDate between '" + _FromDate + @"' and '" + ToDate + @"'
                                                                        group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId FROM MST.DesignationMaster DM
																				LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId where dc.plantid='202034'
 ) dm where dm.DesignationId =(select givendesignationId  from dbo.EmployeeInformation  where SystemId " + empIds + @")
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
LEFT JOIN EmployeeInformation AS ei ON ei.SystemId  = els.EmployeeId
LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
LEFT JOIN hkp.EmployeeCategory CT ON ct.Id=dm.EmployeeCategoryId
LEFT JOIN org.Plant AS p ON p.Id=ei.PlantId                              
LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
 WHERE els.EmployeeID " + empIds + @"   AND lt.Code IN ('PL','CL'))A";


                return _sqlRepository.GetDataTable(_sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function
        public List<Dictionary<string, object>> GetLeaveBalanceType(string EmployeeSystemId, string calYearId)
        {

            try
            {
                string _FromDate = string.Empty;
                string _ToDate = string.Empty;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                // var esic = GetESICEligibleEmployee(EmpSystemID);
                var dsCalYear = GetCalYearInfo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                    _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
                }
                else
                {

                    DataTable dtCalendar = _sqlRepository.GetDataTable("select * from YearlyCalendar where YearNo=" + DateTime.Now.Year.ToString() + @" AND PlantId='" + identity.PlantId + "'");
                    if (dtCalendar.Rows.Count > 0)
                    {
                        _FromDate = dtCalendar.Rows[0]["FromDate"].ToString();
                        _ToDate = dtCalendar.Rows[0]["ToDate"].ToString();
                    }
                }
                var _LFromDate = Convert.ToDateTime(_FromDate).AddYears(-1);
                var _LToDate = Convert.ToDateTime(_ToDate).AddYears(-1);

                //                string X_sql = @"SELECT ei.SystemId,B.LeaveTypeId, ei.EmployeeCode,ei.EmployeeName,FORMAT(ei.DOJ,'dd-MMM-yyyy') AS DOJ,p.UserName AS PlantName,D.UserName AS Designation,
                //                                DEPT.UserName AS Department,ct.UserName AS EmployeeCategory,LT.UserName AS LeaveName,CurrentYearAllocation=ISNULL(CASE WHEN LT.LeaveType='Earn' THEN ALD.Opening ELSE ISNULL(B.CurrentYearAllocation, 0) END,0)								
                //								, BroughtForward= CASE WHEN  ISNULL(AL.PBroughtForward,0)=0 THEN B.BroughtForward ELSE AL.PBroughtForward END								
                //								, B.CarryForwardOpeningBalance, B.DaysCanBeSanctioned, B.AppliedDays,
                //                                B.AvailedDays, B.YearEndEncash,isnull(APL.LeaveDuration,0) AS AppliedLeave,APP.LeaveDuration AS AllFutureAppliedLeave,
                //                                --isnull(B.CarryForwardOpeningBalance,0)+(CASE WHEN  ISNULL(AL.PBroughtForward,0)=0 THEN B.BroughtForward ELSE AL.PBroughtForward END)+isnull(B.CurrentYearAllocation,0)-isnull(B.AvailedDays,0) AS ClosingBalance
                //                               (ISNULL(CASE WHEN  ISNULL(AL.PBroughtForward,0)=0 THEN B.BroughtForward ELSE AL.PBroughtForward END, 0)+B.DaysCanBeSanctioned)-isnull(B.AvailedDays,0) AS ClosingBalance
                //                            FROM (		SELECT BAL.EmployeeId, BAL.LeaveTypeId,
                //								       SUM(BAL.CurrentYearAllocation) AS CurrentYearAllocation,
                //								        SUM(BAL.BroughtForward) AS BroughtForward,SUM(BAL.CarryForwardOpeningBalance) AS CarryForwardOpeningBalance,   SUM(BAL.DaysCanBeSanctioned) AS DaysCanBeSanctioned,
                //								        SUM(BAL.AppliedDays) AS AppliedDays,  SUM(BAL.AvailedDays) AS AvailedDays,
                //								        SUM(BAL.YearEndEncash) AS YearEndEncash
                //								  FROM (
                //								SELECT l.EmployeeId, L.LeaveTypeId,Lt.LeaveType,
                //								CASE WHEN lt.LeaveType='EARN' THEN 0 ELSE ISNULL(l.CurrentYearAllocation,0) END CurrentYearAllocation,l.BroughtForward,l.CarryForwardOpeningBalance

                //,DaysCanBeSanctioned=case when lpd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(lpd.LvCanAvailQuantity,0)
                //																   when lpd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(lpd.LvCanAvailQuantity,0) * Isnull(L.DaysCanBeSanctioned,0))/100
                //																   else Isnull(L.DaysCanBeSanctioned,0) end
                //,l.AppliedDays,0 AvailedDays,L.YearEndEncash
                //                                  from EmployeeInformation EI
                //                                JOIN trn.EmployeeLeaveSummary L ON L.EmployeeId=EI.SystemId 
                //                                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                //								LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                //								LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                //								  JOIN LeavePolicyDetail AS lpd ON lpd.LPMSystemID=dmc.LeavePolicyMasterId AND lpd.LTSystemID=l.LeaveTypeId
                //                                LEFT JOIN LeaveType AS lt ON lt.Id=lpd.LTSystemID
                //                                WHERE  '" + _FromDate + @"' BETWEEN L.FromDate AND L.ToDate
                //                                 AND ei.SystemId='" + EmployeeSystemId + @"' AND CalanderYearId='" + calYearId + @"'  AND lpd.EncashmentBasis='CalanderYear'

                //                                UNION ALL

                //                                SELECT APD.EmpSystemID, l.LeaveTypeId,Lt.LeaveType,CONVERT(DECIMAL(18,4),CASE WHEN EncashWorkingDaysQty>0 THEN CONVERT(DECIMAL(18,4), EncashEarnLeaveQty)/CONVERT(DECIMAL(18,4),EncashWorkingDaysQty) ELSE 0 END) * l.EarnValue AS ActualEarnedLeave,
                //                                0 BroughtForward,0 CarryForwardOpeningBalance,0 DaysCanBeSanctioned,0 AppliedDays,l.AvailedValue AS AvailedDays,0 YearEndEncash


                //                                 FROM AttdnProcessData AS apd
                //                                JOIN EmployeeInformation AS ei ON ei.SystemId=apd.EmpSystemID
                //                                JOIN DayTypeWithValues AS ds ON ds.code=apd.DayStatus AND ds.HeaderId=apd.DayStatusHeaderId
                //                                JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                //                                LEFT JOIN LeaveType AS lt ON lt.Id=L.LeaveTypeId

                //                                 JOIN LeavePolicyDetail AS lpd ON lpd.LPMSystemID=apd.LeavePolicyMasterId AND lpd.LTSystemID=l.LeaveTypeId
                //                                WHERE 
                //                                apd.WorkDate BETWEEN '" + _FromDate + @"' AND '" + _ToDate + @"'
                //                                AND ei.SystemId='" + EmployeeSystemId + @"' 

                //	UNION ALL

                //								 select S.EmployeeId EmpSystemID,s.LeaveTypeId,lt.LeaveType,0 ActualEarnedLeave,0 BroughtForward, 0 CarryForwardOpeningBalance
                //								 ,DaysCanBeSanctioned=case when lp.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(lp.LvCanAvailQuantity,0)
                //																   when lp.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(lp.LvCanAvailQuantity,0) * Isnull(s.DaysCanBeSanctioned,0))/100
                //																   else Isnull(s.DaysCanBeSanctioned,0) end
                //								,0 AppliedDays,0.AvailedDays,0 YearEndEncash
                //								 from trn.EmployeeLeaveSummary S
                //                                                        JOIN  trn.EmployeeLeaveSummary SS ON S.Id=ss.Id
                //                                                        AND S.Id=(SELECT TOP 1 SX.Id FROM trn.EmployeeLeaveSummary SX WHERE ss.EmployeeId=SX.EmployeeId AND ss.LeaveTypeId=SX.LeaveTypeId ORDER BY sx.ToDate DESC)
                //                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
                //                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                //                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                //                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
                //														  LEFT JOIN LeaveType AS lt ON lt.Id=lp.LTSystemID
                //                                                        where S.EmployeeId ='" + EmployeeSystemId + @"'  AND lp.EncashmentBasis<>'CalanderYear'


                //								) AS BAL
                //								GROUP BY BAL.EmployeeId, BAL.LeaveTypeId

                //                    ) AS B
                //                    LEFT JOIN LeaveType AS lt ON lt.Id=b.LeaveTypeId
                //                    LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=B.EmployeeId
                //                    LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                //                    LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                //                    LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                //                    LEFT JOIN hkp.EmployeeCategory CT ON ct.Id=dm.EmployeeCategoryId
                //                    LEFT JOIN org.Plant AS p ON p.Id=ei.PlantId

                //                    LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                //                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                //                    LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                //                    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id         
                //LEFT JOIN
                //											(
                //										  select A.Opening,A.EmployeeId,A.LeaveTypeId,FORMAT(LY.FromDate,'dd-MMM-yyyy')FromDate,FORMAT(LY.ToDate,'dd-MMM-yyyy')ToDate from dbo.AnnualLeaveDataCurrent A
                //										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
                //										  LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
                //											)ALD ON ALD.EmployeeId=ei.SystemId AND lt.Id=ALD.LeaveTypeId

                // LEFT JOIN
                //											(
                //										  select PBroughtForward=CASE WHEN A.Opening=0 THEN A.Adjustment ELSE A.Opening END,A.EmployeeId,A.LeaveTypeId from dbo.AnnualLeaveDataPast A
                //										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
                //											)AL ON AL.EmployeeId=ei.SystemId AND lt.Id=AL.LeaveTypeId


                //                    LEFT JOIN (
                //                                SELECT T.EmpSystemID,T.LTSystemID,SUM(D.LeaveDuration) AS LeaveDuration FROM LeaveTransaction AS T
                //                                JOIN LeaveTransactionDetails AS D ON d.LvTrnsSystemID=t.SystemID
                //                                WHERE D.WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"' and D.IsAvailed=1
                //                                AND T.IsApproved=1 GROUP BY  T.EmpSystemID,T.LTSystemID
                //                    ) APL ON apl.EmpSystemID=B.EmployeeId AND apl.LTSystemID=B.LeaveTypeId
                //                    LEFT JOIN (
                //                                SELECT T.EmpSystemID,T.LTSystemID,SUM(D.LeaveDuration) AS LeaveDuration FROM LeaveTransaction AS T
                //                                JOIN LeaveTransactionDetails AS D ON d.LvTrnsSystemID=t.SystemID
                //                                WHERE D.WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                //                                GROUP BY  T.EmpSystemID,T.LTSystemID
                //                    ) APP ON APP.EmpSystemID=B.EmployeeId AND APP.LTSystemID=B.LeaveTypeId
                //                    where LT.UserName NOT LIKE '%Maternity%'
                //                    order by ei.EmployeeCode";
                string _sql = @"Select * from (SELECT ei.SystemId,lt.Id LeaveTypeId,ei.EmployeeCode,ei.EmployeeName,FORMAT(ei.DOJ,'dd-MMM-yyyy') AS DOJ,p.UserName AS PlantName,D.UserName AS Designation,
DEPT.UserName AS Department,ct.UserName AS EmployeeCategory, lt.UserName LeaveName
										 ,	DaysCanBeSanctioned=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end
 ,CurrentYearAllocation=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end,
                                        CurrentAllocation=ISNULL(CASE WHEN LT.LeaveType='Earn' THEN ALD.Opening ELSE ISNULL(els.CurrentYearAllocation, 0) END,0)
										,0 YearEndEncash,''AppliedLeave
										,CarryForwardOpeningBalance=CASE WHEN LT.LeaveType='Earn' THEN	
												ISNULL(ALP.PBroughtForward, 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
                                         BroughtForward=CASE WHEN LT.LeaveType='Earn' THEN	
												ISNULL(ALP.PBroughtForward, 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END
										  ,LeaveDays=(CASE WHEN LT.LeaveType='Earn' THEN	
												ISNULL(ALP.PBroughtForward, 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)+(case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end)
										   ,ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) AppliedDays
										   ,ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) AvailedDays
                                            ,ISNULL(tav.av, 0)AllFutureAppliedLeave
										 ,ClosingBalance=((CASE WHEN LT.LeaveType='Earn' THEN	
												ISNULL(ALP.PBroughtForward, 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)+(case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end))-(ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0))

----------------------------------------------------------------------------------------------------------------------
                                          FROM (    select S.* from trn.EmployeeLeaveSummary S
                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
														where CalanderYearId IN (Select Id from YearlyCalendar  where '" + _FromDate + @"' BETWEEN FromDate AND ToDate)
														AND S.EmployeeId ='" + EmployeeSystemId + @"' AND lp.EncashmentBasis='CalanderYear'

                                                        UNION

                                                        select S.* from trn.EmployeeLeaveSummary S
                                                        JOIN  trn.EmployeeLeaveSummary SS ON S.Id=ss.Id
                                                        AND S.Id=(SELECT TOP 1 SX.Id FROM trn.EmployeeLeaveSummary SX WHERE ss.EmployeeId=SX.EmployeeId AND ss.LeaveTypeId=SX.LeaveTypeId ORDER BY sx.ToDate DESC)
                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
														Join YearlyCalendar AS C ON C.PlantId=EI.PlantId
                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
                                                        where S.EmployeeId ='" + EmployeeSystemId + @"' AND lp.EncashmentBasis<>'CalanderYear') els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
LEFT JOIN
											(
										   select A.Opening,A.EmployeeId,A.LeaveTypeId,FORMAT(LY.FromDate,'dd-MMM-yyyy')FromDate,FORMAT(LY.ToDate,'dd-MMM-yyyy')ToDate from dbo.AnnualLeaveDataCurrent A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
										  LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										  Where LY.FromDate between'" + _FromDate + @"' and '" + _ToDate + @"' AND LY.ToDate between'" + _FromDate + @"' and '" + _ToDate + @"' AND A.EmployeeId='" + EmployeeSystemId + @"'
											)ALD ON ALD.EmployeeId=els.EmployeeId AND lt.Id=ALD.LeaveTypeId

 LEFT JOIN
											(
										  select PBroughtForward=CASE WHEN A.Opening=0 THEN A.Adjustment ELSE A.Opening END,A.EmployeeId,A.LeaveTypeId from dbo.AnnualLeaveDataPast A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
                                           LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										  Where LY.FromDate between'" + _LFromDate + @"' and '" + _LToDate + @"' AND LY.ToDate between'" + _LFromDate + @"' and '" + _LToDate + @"' AND A.EmployeeId='" + EmployeeSystemId + @"'
											)ALP ON ALP.EmployeeId=els.EmployeeId AND lt.Id=ALP.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
                            where  (FromDate between '" + _FromDate + @"' and '" + _ToDate + @"') and (ToDate between '" + _FromDate + @"' and '" + _ToDate + @"')
                                                    group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID,m.EmpSystemID from  dbo.LeaveTransaction m
LEFT JOIN dbo.LeaveTransactionDetails d ON d.LvTrnsSystemID=m.SystemId
where d.IsAvailed = 1 and d.WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
group by d.LvTrnsSystemID,m.EmpSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId FROM MST.DesignationMaster DM
																				LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId where dc.plantid='" + identity.PlantId + @"'
 ) dm where dm.DesignationId =(select givendesignationId  from dbo.EmployeeInformation  where SystemId ='" + EmployeeSystemId + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
LEFT JOIN EmployeeInformation AS ei ON ei.SystemId  = els.EmployeeId
LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
LEFT JOIN hkp.EmployeeCategory CT ON ct.Id=dm.EmployeeCategoryId
LEFT JOIN org.Plant AS p ON p.Id=ei.PlantId                              
LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
 WHERE els.EmployeeID ='" + EmployeeSystemId + @"'   AND LT.UserName NOT LIKE '%Maternity%')A";
                return _sqlRepository.GetDataCollection(_sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function

        public List<Dictionary<string, object>> GetLeaveBalanceTypeNew(string EmpSystemID, string calYearId,string sPlantID)
        {
            try
            {
                string _FromDate = string.Empty;
                string _ToDate = string.Empty;
                string _sql = null;
                DataSet dsCalYear = GetCalYearInfo(calYearId);
                DataSet dsCalYearNo = GetCalYearInfo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                    _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();

                }
                else if (dsCalYearNo.Tables[0].Rows.Count > 0)
                {
                    calYearId = dsCalYearNo.Tables[0].Rows[0]["Id"].ToString();
                    _FromDate = dsCalYearNo.Tables[0].Rows[0]["FromDate"].ToString();
                    _ToDate = dsCalYearNo.Tables[0].Rows[0]["ToDate"].ToString();
                }
                else
                {
                    throw new Exception("No Year found...");
                }
                var esic = GetESICEligibleEmployeeFromEnum(EmpSystemID, DateTime.Now.ToString("dd-MMM-yyyy"));

                var lastYear = Convert.ToDateTime(_FromDate).AddYears(-1);

                var _LFromDate = Convert.ToDateTime(_FromDate).AddYears(-1);
                var _LToDate = Convert.ToDateTime(_ToDate).AddYears(-1);

                if (esic.Tables[0].Rows.Count > 0)
                {
                    _sql = @"SELECT els.CalanderYearID,ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed
                                        ,FromDate=FORMAT(ELS.FromDate,'dd-MMM-yyyy')
										,ToDate=FORMAT(ELS.ToDate,'dd-MMM-yyyy')
										 ,els.Id SystemID,
                                         els.LeaveTypeId,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID,
                                         --ltd.IsProrataPreviousyear,
                                         ltd.IsProratacurrentyear
                                       ,DaysCanBeSanctioned=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END
,CurrentAllocationDCBS= case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END

                                        ,els.EncashedInbetween
                                        ,ltd.IsAvailExceptionAllowedOnSpecialAppeal,
                                        CurrentAllocation=ISNULL(els.CurrentYearAllocation, 0),
                                        ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         --ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                                         BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,                                        

                                         LeaveDays=ISNULL(els.DaysCanBeSanctioned, 0),
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,
                                            ISNULL(R.Rejected,0)Rejected,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType
                                            ,ISNULL(tav.av, 0)AllFutureAppliedLeave
                                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
                                            ---,IsBroughtForwardAdd=CASE WHEN LT.LeaveType='Earn' THEN  
                                            ,IsBroughtForwardAdd=CASE WHEN 1=1 THEN  
                                            	CASE WHEN
                                            	-----------------------------------DOJorDOC start -----------------------------------------------------------
                                            								CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                                                                        										 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
                                            																	      WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
                                            																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
                                            										   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
                                            										   							 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
                                            																		  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
                                            																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
                                            										   						END
                                                                                   END
                                            ---------------------------------------DOJorDOC start  end-------------------------------------------------------
                                            	
                                            	> GETDATE() then 
                                            		    CONVERT(BIT,0)------No
                                                    ELSE  CONVERT(BIT,1) END---Yes
                                            ELSE CONVERT(BIT,0) END  ---No
                                            ,Earned=CAST (0 AS decimal(18,2))
,ClosingBalance=(case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END)-ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0)
                                            ----------------------------------------------------------------------------------------------------------------------



                                          FROM (
                                                        select S.* from trn.EmployeeLeaveSummary S
                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
                                                        where CalanderYearId='" + calYearId + @"' and S.EmployeeId ='" + EmpSystemID + @"' AND lp.EncashmentBasis='CalanderYear') els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
                                        LEFT JOIN EmployeeInformation AS emp ON emp.SystemId  = els.EmployeeId


        LEFT JOIN (Select Sum(LTD.LeaveDuration) Rejected,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' AND '" + _ToDate + @"' AND IsCancel=1
                                                            group by LT.EmpSystemID,LT.LTSystemID)R ON R.EmpSystemID = els.EmployeeId  and R.LTSystemId = els.LeaveTypeId
										 left outer join (
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
															Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
															Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
															group by LT.EmpSystemID,LT.LTSystemID
														 )ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (														
															Select Sum(LTD.LeaveDuration) av,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
															Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
															Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"' and LTD.IsAvailed=1
															group by LT.EmpSystemID,LT.LTSystemID
														  )tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId

										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (SELECT DC.LeavePolicyMasterId,dm.DesignationId FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
																						where dc.plantid='" + sPlantID + @"'
																		 ) dm where dm.DesignationId =(select givendesignationId from dbo.EmployeeInformation where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
                                                WHERE els.EmployeeID = '" + EmpSystemID + @"'
                                              --AND CalanderYearID = '" + calYearId + @"'
                                             AND els.LeaveTypeId IN ( --IN

                                            SELECT LT.ID FROM dbo.ESICPolicyLeaveType AS EPLT
                                                      LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                                                      WHERE
                                                      EPLT.LeaveTypeID IN
                                                       (
                                                         SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                                                      LEFT JOIN  (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId WHERE DC.PlantId='" + sPlantID + @"') AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                                                      LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                                                      WHERE EI.SystemID='" + EmpSystemID + @"' AND EI.PlantID='" + sPlantID + @"'
                                                       )
                                                    AND
                                                    EPLT.ESICPolicyMasterID IN (
                                                     SELECT DM.ESICPolicyMasterID FROM (SELECT DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                    WHERE DC.PlantId='" + sPlantID + @"') DM
                                                     WHERE DM.DesignationId IN (SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + EmpSystemID + @"'))) AND LT.UserName NOT LIKE '%Maternity%'

UNION ALL
 Select A.CalanderYearID,CAST (0 AS BIT) IsExceptionAllowed,A.FromDate,A.ToDate
 ,a.SystemID,A.LTSystemID LeaveTypeId,A.EmployeeId EmployeeID,A.LeaveName, A.LeaveDescription,ltd.SystemID LvPolDetailsSystemID,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear
 ,DaysCanBeSanctioned=CAST (B.CarryForward AS decimal(18,2))
 ,CurrentAllocationDCBS=CAST (B.CarryForward AS decimal(18,2)),0 EncashedInbetween,CAST (0 AS BIT) IsAvailExceptionAllowedOnSpecialAppeal
 ,CurrentAllocation=(select ((SELECT DATEDIFF(day,  cast(YEAR('" + _FromDate + @"') as char(4)),  cast(YEAR('" + _FromDate + @"')+1 as char(4))))-COUNT(d.OffDayDate))/ltd.EncashWorkingDaysQty
		                                from scs.OffDayDetail d
		                                inner join scs.OffDayMaster m on d.offdaymasterid=m.id and OffDayType in ('H','W') and m.PlantId='" + sPlantID + @"'
		                                where d.PlantId='" + sPlantID + @"'
		                                and d.OffDayDate between '" + _FromDate + @"' AND '" + _ToDate + @"') 
,CAST (0 AS decimal(18,2)) PreviousYearCarryForward
 --,BroughtForward=CASE WHEN A.Opening>B.CarryForward THEN A.Opening WHEN B.BroughtForward>B.CarryForward THEN B.BroughtForward ELSE B.CarryForward END
 ,BroughtForward=CAST (A.Opening AS decimal(18,2))
 
  ,LeaveDays=ROUND(CAST (A.Opening +CAST(case when ltd.EncashWorkingDaysQty >0 
then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty 
else 0 END AS decimal(18,2))-ISNULL(tav.av, 0)AS decimal(18,2)),0)
 ,ISNULL(ltrn.ldays, 0) Applied,ISNULL(tav.av, 0) Availed,ISNULL(R.Rejected,0) Rejected,ISNULL(acApl.ldays,0) ldays,A.LeaveType,ISNULL(tav.av, 0)AllFutureAppliedLeave,CAST (0 AS BIT) IsBroughtForwardAdd
 ,(round((CAST(case when ltd.EncashWorkingDaysQty >0 then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty else 0 END AS decimal(18,2))) * 2, 0)/2) as Earned

,ClosingBalance=ROUND(CAST (A.Opening +CAST(case when ltd.EncashWorkingDaysQty >0 
then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty 
else 0 END AS decimal(18,2))-ISNULL(tav.av, 0)AS decimal(18,2)),0)
 from (
 select A.LeaveYearId CalanderYearID,0 IsExceptionAllowed,a.Id SystemID,A.LeaveTypeId LTSystemID,A.EmployeeId EmployeeID,lt.UserName LeaveName, lt.Description LeaveDescription,lt.LeaveType
 ,FORMAT(LY.FromDate,'dd-MMM-yyyy')FromDate,FORMAT(LY.ToDate,'dd-MMM-yyyy')ToDate,A.Opening 
 from dbo.AnnualLeaveDataCurrent A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
										  LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										  Where LY.FromDate between'" + _FromDate + @"' AND '" + _ToDate + @"' 
										  AND LY.ToDate between'" + _FromDate + @"' AND '" + _ToDate + @"'
                                           AND A.EmployeeId='" + EmpSystemID + @"'
										  ) A  
LEFT JOIN (
select A.EmployeeId,A.LeaveTypeId,lt.UserName LeaveName,A.CarryForward from dbo.AnnualLeaveDataPast A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
                                        LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										 Where LY.FromDate between '" + _LFromDate + @"' AND '" + _LToDate + @"' 
										  AND LY.ToDate between '" + _LFromDate + @"' AND '" + _LToDate + @"' AND A.EmployeeId='" + EmpSystemID + @"')B ON B.EmployeeId=A.EmployeeId  AND A.LTSystemID=B.LeaveTypeId

left outer join (select ltd.* from dbo.LeavePolicyDetail ltd
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																			SELECT DC.LeavePolicyMasterId,dm.DesignationId FROM MST.DesignationMaster DM
																			LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId where dc.plantid='" + sPlantID + @"'
																		 ) dm where dm.DesignationId =(select givendesignationId  from dbo.EmployeeInformation where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = A.LTSystemID

LEFT JOIN (Select Sum(LTD.LeaveDuration) Rejected,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' AND '" + _ToDate + @"' AND IsCancel=1
                                                            group by LT.EmpSystemID,LT.LTSystemID)R ON R.EmpSystemID = A.EmployeeId  and R.LTSystemId = A.LTSystemID
										 left outer join (
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                            group by LT.EmpSystemID,LT.LTSystemID
														)ltrn on ltrn.EmpSystemID = A.EmployeeId and ltrn.LTSystemId = A.LTSystemID
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                                        group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = A.EmployeeId and tav.LTSystemId = A.LTSystemID
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = A.EmployeeId and acApl.LTSystemId = A.LTSystemID
left join (SELECT EmpSystemID,SUM(l.EarnValue)EarnDays,T.Id as LeaveId,ei.PlantId
				FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd ON apd.EmpSystemID=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                JOIN LeaveType T ON t.Id=L.LeaveTypeId
                where apd.workdate between '" + _FromDate + @"' and '" + _ToDate + @"'
                and EI.PlantID='" + sPlantID + @"' and t.LeaveType='Earn'
                group by EmpSystemID,t.Id,ei.plantid
                ) as Masterx on Masterx.EmpSystemID=A.EmployeeId and Masterx.PlantId='" + sPlantID + @"' and Masterx.LeaveId=A.LTSystemID
				left join (Select top(1) md.* from  ManualLeaveData md
				JOIN LeaveType T ON t.Id=md.LeaveTypeId AND t.LeaveType='Earn'
				Where md.EmployeeId='" + EmpSystemID + @"'order by md.addeddate desc
				)med on med.EmployeeId=A.EmployeeId
Where A.EmployeeId='" + EmpSystemID + "'";

                }
                else
                {
                    _sql = @"SELECT els.CalanderYearID, ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed
                                        ,FromDate=FORMAT(ELS.FromDate,'dd-MMM-yyyy')
										,ToDate=FORMAT(ELS.ToDate,'dd-MMM-yyyy')
										 ,els.Id SystemID,
                                         els.LeaveTypeId,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID
                                         ,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear
                                     ,DaysCanBeSanctioned= case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END
,CurrentAllocationDCBS=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END
										,els.EncashedInbetween
                                         ,CAST (ISNULL(ltd.IsAvailExceptionAllowedOnSpecialAppeal,0) AS BIT)IsAvailExceptionAllowedOnSpecialAppeal,
                                        CurrentAllocation=ISNULL(els.CurrentYearAllocation, 0),
                                         ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         --ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                                         BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
            --                             BroughtForward=CASE WHEN LT.LeaveType='Earn' THEN	
												--ISNULL(ALP.PBroughtForward, 
												-- CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												-- ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  --ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,


                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
-- LeaveDays=ISNULL(CASE WHEN LT.LeaveType='Earn' THEN ALD.Opening ELSE ISNULL(els.DaysCanBeSanctioned, 0) END,0),
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,
                                         ISNULL(R.Rejected,0)Rejected,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType
                                         ,ISNULL(tav.av, 0)AllFutureAppliedLeave

-----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
---,IsBroughtForwardAdd=CASE WHEN LT.LeaveType='Earn' THEN
,IsBroughtForwardAdd=CASE WHEN 1=1 THEN  
	CASE WHEN
	-----------------------------------DOJorDOC start -----------------------------------------------------------
								CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            										 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
										   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   							 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
																		  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
										   						END
                                       END
---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
	> GETDATE() then 
		    CONVERT(BIT,0)------No
        ELSE  CONVERT(BIT,1) END---Yes
ELSE CONVERT(BIT,0) END  ---No
,Earned=CAST (0 AS decimal(18,2))
,ClosingBalance=(case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END)-ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0)
----------------------------------------------------------------------------------------------------------------------
                                          FROM (    select S.* from trn.EmployeeLeaveSummary S
                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
                                                        where CalanderYearId='" + calYearId + @"' and S.EmployeeId ='" + EmpSystemID + @"' AND lp.EncashmentBasis='CalanderYear'
														) els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
        LEFT JOIN (Select Sum(LTD.LeaveDuration) Rejected,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' AND '" + _ToDate + @"' AND IsCancel=1
                                                            group by LT.EmpSystemID,LT.LTSystemID)R ON R.EmpSystemID = els.EmployeeId  and R.LTSystemId = els.LeaveTypeId
										 left outer join (
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                            group by LT.EmpSystemID,LT.LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                                        group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId 
																										FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC 
																													ON DM.Id=DC.DesignationMasterId
																						where dc.plantid='" + sPlantID + @"'

																		 ) dm where dm.DesignationId =(select givendesignationId 
																									 from dbo.EmployeeInformation 
																									 where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
LEFT JOIN EmployeeInformation AS emp ON emp.SystemId  = els.EmployeeId
                                                WHERE els.EmployeeID = '" + EmpSystemID + @"'
                                              
                                              AND els.LeaveTypeId not IN (select id from LeaveType where IsESIC=1 and IsGeneral=0) AND LT.UserName NOT LIKE '%Maternity%'

UNION ALL
 Select A.CalanderYearID,CAST (0 AS BIT) IsExceptionAllowed,A.FromDate,A.ToDate
 ,a.SystemID,A.LTSystemID LeaveTypeId,A.EmployeeId EmployeeID,A.LeaveName, A.LeaveDescription,ltd.SystemID LvPolDetailsSystemID,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear
 ,DaysCanBeSanctioned=CAST (B.CarryForward AS decimal(18,2))
 ,CurrentAllocationDCBS=CAST (B.CarryForward AS decimal(18,2)),0 EncashedInbetween,CAST (0 AS BIT) IsAvailExceptionAllowedOnSpecialAppeal
 ,CurrentAllocation=(select ((SELECT DATEDIFF(day,  cast(YEAR('" + _FromDate + @"') as char(4)),  cast(YEAR('" + _FromDate + @"')+1 as char(4))))-COUNT(d.OffDayDate))/ltd.EncashWorkingDaysQty
		                                from scs.OffDayDetail d
		                                inner join scs.OffDayMaster m on d.offdaymasterid=m.id and OffDayType in ('H','W') and m.PlantId='" + sPlantID + @"'
		                                where d.PlantId='" + sPlantID + @"'
		                                and d.OffDayDate between '" + _FromDate + @"' AND '" + _ToDate + @"') 
,CAST (0 AS decimal(18,2)) PreviousYearCarryForward
 --,BroughtForward=CASE WHEN A.Opening>B.CarryForward THEN A.Opening WHEN B.BroughtForward>B.CarryForward THEN B.BroughtForward ELSE B.CarryForward END
 ,BroughtForward=CAST (A.Opening AS decimal(18,2))
 
  ,LeaveDays=ROUND(CAST (A.Opening +CAST(case when ltd.EncashWorkingDaysQty >0 
then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty 
else 0 END AS decimal(18,2))-ISNULL(tav.av, 0)AS decimal(18,2)),0)
 ,ISNULL(ltrn.ldays, 0) Applied,ISNULL(tav.av, 0) Availed,ISNULL(R.Rejected,0) Rejected,ISNULL(acApl.ldays,0) ldays,A.LeaveType,ISNULL(tav.av, 0)AllFutureAppliedLeave,CAST (0 AS BIT) IsBroughtForwardAdd
 ,(round((CAST(case when ltd.EncashWorkingDaysQty >0 then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty else 0 END AS decimal(18,2))) * 2, 0)/2) as Earned

,ClosingBalance=ROUND(CAST (A.Opening +CAST(case when ltd.EncashWorkingDaysQty >0 
then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty 
else 0 END AS decimal(18,2))-ISNULL(tav.av, 0)AS decimal(18,2)),0)
 from (
 select A.LeaveYearId CalanderYearID,0 IsExceptionAllowed,a.Id SystemID,A.LeaveTypeId LTSystemID,A.EmployeeId EmployeeID,lt.UserName LeaveName, lt.Description LeaveDescription,lt.LeaveType
 ,FORMAT(LY.FromDate,'dd-MMM-yyyy')FromDate,FORMAT(LY.ToDate,'dd-MMM-yyyy')ToDate,A.Opening 
 from dbo.AnnualLeaveDataCurrent A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
										  LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										  Where LY.FromDate between'" + _FromDate + @"' AND '" + _ToDate + @"' 
										  AND LY.ToDate between'" + _FromDate + @"' AND '" + _ToDate + @"'
										  ) A  
LEFT JOIN (
select BroughtForward=CASE WHEN A.Adjustment=0 THEN A.Opening ELSE A.Adjustment END,A.EmployeeId,A.LeaveTypeId,lt.UserName LeaveName,A.CarryForward from dbo.AnnualLeaveDataPast A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
                                        LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										  Where LY.FromDate between '" + _LFromDate + @"' AND '" + _LToDate + @"' 
										  AND LY.ToDate between '" + _LFromDate + @"' AND '" + _LToDate + @"')B ON B.EmployeeId=A.EmployeeId  AND A.LTSystemID=B.LeaveTypeId

left outer join (select ltd.* from dbo.LeavePolicyDetail ltd
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																			SELECT DC.LeavePolicyMasterId,dm.DesignationId FROM MST.DesignationMaster DM
																			LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId where dc.plantid='" + sPlantID + @"'
																		 ) dm where dm.DesignationId =(select givendesignationId  from dbo.EmployeeInformation where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = A.LTSystemID

LEFT JOIN (Select Sum(LTD.LeaveDuration) Rejected,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' AND '" + _ToDate + @"' AND IsCancel=1
                                                            group by LT.EmpSystemID,LT.LTSystemID)R ON R.EmpSystemID = A.EmployeeId  and R.LTSystemId = A.LTSystemID
										 left outer join (
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                            group by LT.EmpSystemID,LT.LTSystemID
														)ltrn on ltrn.EmpSystemID = A.EmployeeId and ltrn.LTSystemId = A.LTSystemID
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                                        group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = A.EmployeeId and tav.LTSystemId = A.LTSystemID
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = A.EmployeeId and acApl.LTSystemId = A.LTSystemID
left join (SELECT EmpSystemID,SUM(l.EarnValue)EarnDays,T.Id as LeaveId,ei.PlantId
				FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd ON apd.EmpSystemID=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                JOIN LeaveType T ON t.Id=L.LeaveTypeId
                where apd.workdate between '" + _FromDate + @"' and '" + _ToDate + @"'
                and EI.PlantID='" + sPlantID + @"' and t.LeaveType='Earn'
                group by EmpSystemID,t.Id,ei.plantid
                ) as Masterx on Masterx.EmpSystemID=A.EmployeeId and Masterx.PlantId='" + sPlantID + @"' and Masterx.LeaveId=A.LTSystemID
				left join (Select top(1) md.* from  ManualLeaveData md
				JOIN LeaveType T ON t.Id=md.LeaveTypeId AND t.LeaveType='Earn'
				Where md.EmployeeId='" + EmpSystemID + @"' order by md.addeddate desc
				)med on med.EmployeeId=A.EmployeeId
Where A.EmployeeId='" + EmpSystemID + "'";

                }

                return _sqlRepository.GetDataCollection(_sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function

        public DataSet GetESICEligibleEmployeeFromEnum(string empSystemId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT IsEligible,SalaryStructureId,EmpSystemId,m.EffectiveDate
                                  FROM [dbo].[EmployeeEligibleForSalaryHeadEnum] n
                                  left join (select SystemID,EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster
                                  union
                                  select SystemID,EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster
                                  )
                                   mm on mm.SystemID=n.SalaryStructureId
                                  inner join (
                                  select MAX(EffectiveDate)EffectiveDate,EmpInfoSystemID from (
                                  select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster where IsApproved=1 
                                  union
                                   select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster where IsApproved=1 
                                   ) x 
                                   group by EmpInfoSystemID
                                  )m on mm.EffectiveDate=m.EffectiveDate and m.EmpInfoSystemID=mm.EmpInfoSystemID
                                  where SalaryHeadEnum='ESIC' and mm.EmpInfoSystemID='" + empSystemId + @"'  and IsEligible=1
                                 "
            };//and EffectiveDate<='" + FromDate + @"'
              //var data = _sqlRepository.GetDataCollection(CmdText);
            return _sqlRepository.GetGridData(parameters).Source;
        }

        public List<Dictionary<string, object>> GetLeaveBalanceTypeApp(string EmployeeSystemId, string calYearId)
        {

            try
            {
                string _FromDate = string.Empty;
                string _ToDate = string.Empty;
                string _sql = "";
                var esic = GetESICEligibleEmployeeFromEnum(EmployeeSystemId);
                var dsCalYear = GetCalYearInfo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                    _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
                }
                var _LFromDate = Convert.ToDateTime(_FromDate).AddYears(-1);
                var _LToDate = Convert.ToDateTime(_ToDate).AddYears(-1);
                if (esic.Tables[0].Rows.Count > 0)
                {
                    _sql = @"Select ei.SystemId,A.LTSystemID LeaveTypeId,ei.EmployeeCode,ei.EmployeeName,FORMAT(ei.DOJ,'dd-MMM-yyyy') AS DOJ,p.UserName AS PlantName,D.UserName AS Designation,
DEPT.UserName AS Department,ct.UserName AS EmployeeCategory, A.LeaveName,A.DaysCanBeSanctioned,A.CurrentAllocation CurrentYearAllocation,A.CurrentAllocation,0 YearEndEncash,A.Applied AppliedLeave
,A.PreviousYearCarryForward CarryForwardOpeningBalance,A.BroughtForward,A.LeaveDays,A.Applied AppliedDays,A.Availed AvailedDays,A.Rejected,A.Balance ClosingBalance
 from (SELECT els.CalanderYearID,ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed
                                        ,FromDate=FORMAT(ELS.FromDate,'dd-MMM-yyyy')
										,ToDate=FORMAT(ELS.ToDate,'dd-MMM-yyyy')
										 ,els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID,
                                         --ltd.IsProrataPreviousyear,
                                         ltd.IsProratacurrentyear
                                       ,DaysCanBeSanctioned=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END
,CurrentAllocationDCBS= case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END

                                        ,els.EncashedInbetween
                                        ,ltd.IsAvailExceptionAllowedOnSpecialAppeal,
                                        CurrentAllocation=ISNULL(els.CurrentYearAllocation, 0),
                                        ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         --ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                                         BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,                                        

                                         LeaveDays=ISNULL(els.DaysCanBeSanctioned, 0),
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,
                                            ISNULL(R.Rejected,0)Rejected,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType
                                            
                                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
                                            ---,IsBroughtForwardAdd=CASE WHEN LT.LeaveType='Earn' THEN  
                                            ,IsBroughtForwardAdd=CASE WHEN 1=1 THEN  
                                            	CASE WHEN
                                            	-----------------------------------DOJorDOC start -----------------------------------------------------------
                                            								CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                                                                        										 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
                                            																	      WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
                                            																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
                                            										   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
                                            										   							 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
                                            																		  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
                                            																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
                                            										   						END
                                                                                   END
                                            ---------------------------------------DOJorDOC start  end-------------------------------------------------------
                                            	
                                            	> GETDATE() then 
                                            		    CONVERT(BIT,0)------No
                                                    ELSE  CONVERT(BIT,1) END---Yes
                                            ELSE CONVERT(BIT,0) END  ---No
                                            ,Earned=CAST (0 AS decimal(18,2))
,Balance=(case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END)-ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0)
                                            ----------------------------------------------------------------------------------------------------------------------



                                          FROM (
                                                        select S.* from trn.EmployeeLeaveSummary S
                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
                                                        where CalanderYearId='" + calYearId + @"' and S.EmployeeId ='" + EmployeeSystemId + @"' AND lp.EncashmentBasis='CalanderYear') els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
                                        LEFT JOIN EmployeeInformation AS emp ON emp.SystemId  = els.EmployeeId


        LEFT JOIN (Select Sum(LTD.LeaveDuration) Rejected,LT.EmpSystemId,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' AND '" + _ToDate + @"' AND IsCancel=1
                                                            group by LT.EmpSystemId,LT.LTSystemID)R ON R.EmpSystemId = els.EmployeeId  and R.LTSystemId = els.LeaveTypeId
										 left outer join (
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemId,LT.LTSystemID  from LeaveTransaction LT
															Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
															Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
															group by LT.EmpSystemId,LT.LTSystemID
														 )ltrn on ltrn.EmpSystemId = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (														
															Select Sum(LTD.LeaveDuration) av,LT.EmpSystemId,LT.LTSystemID  from LeaveTransaction LT
															Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
															Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"' and LTD.IsAvailed=1
															group by LT.EmpSystemId,LT.LTSystemID
														  )tav on tav.EmpSystemId = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId

										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemId,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemId,LTSystemID
														  )acApl  on acApl.EmpSystemId = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (SELECT DC.LeavePolicyMasterId,dm.DesignationId FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
																		 ) dm where dm.DesignationId =(select givendesignationId from dbo.EmployeeInformation where SystemId='" + EmployeeSystemId + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
                                                WHERE els.EmployeeID = '" + EmployeeSystemId + @"'
                                              --AND CalanderYearID = '" + calYearId + @"'
                                             AND els.LeaveTypeId IN ( --IN

                                            SELECT LT.ID FROM dbo.ESICPolicyLeaveType AS EPLT
                                                      LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                                                      WHERE
                                                      EPLT.LeaveTypeID IN
                                                       (
                                                         SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                                                      LEFT JOIN  (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId) AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                                                      LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                                                      WHERE EI.SystemID='" + EmployeeSystemId + @"'
                                                       )
                                                    AND
                                                    EPLT.ESICPolicyMasterID IN (
                                                     SELECT DM.ESICPolicyMasterID FROM (SELECT DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId) DM
                                                     WHERE DM.DesignationId IN (SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + EmployeeSystemId + @"'))) AND LT.UserName NOT LIKE '%Maternity%'

UNION ALL
 Select A.CalanderYearID,CAST (0 AS BIT) IsExceptionAllowed,A.FromDate,A.ToDate
 ,a.SystemID,A.LTSystemID,A.EmployeeId EmployeeID,A.LeaveName, A.LeaveDescription,ltd.SystemID LvPolDetailsSystemID,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear
 ,DaysCanBeSanctioned=CAST (B.CarryForward AS decimal(18,2))
 ,CurrentAllocationDCBS=CAST (B.CarryForward AS decimal(18,2)),0 EncashedInbetween,CAST (0 AS BIT) IsAvailExceptionAllowedOnSpecialAppeal
 ,CurrentAllocation=(select ((SELECT DATEDIFF(day,  cast(YEAR('" + _FromDate + @"') as char(4)),  cast(YEAR('" + _FromDate + @"')+1 as char(4))))-COUNT(d.OffDayDate))/ltd.EncashWorkingDaysQty
		                                from scs.OffDayDetail d
		                                inner join scs.OffDayMaster m on d.offdaymasterid=m.id and OffDayType in ('H','W')
		                                and d.OffDayDate between '" + _FromDate + @"' AND '" + _ToDate + @"') 
,CAST (0 AS decimal(18,2)) PreviousYearCarryForward
 --,BroughtForward=CASE WHEN A.Opening>B.CarryForward THEN A.Opening WHEN B.BroughtForward>B.CarryForward THEN B.BroughtForward ELSE B.CarryForward END
 ,BroughtForward=CAST (A.Opening AS decimal(18,2))
 
  ,LeaveDays=ROUND(CAST (A.Opening +CAST(case when ltd.EncashWorkingDaysQty >0 
then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty 
else 0 END AS decimal(18,2))-ISNULL(tav.av, 0)AS decimal(18,2)),0)
 ,ISNULL(ltrn.ldays, 0) Applied,ISNULL(tav.av, 0) Availed,ISNULL(R.Rejected,0) Rejected,ISNULL(acApl.ldays,0) ldays,A.LeaveType,CAST (0 AS BIT) IsBroughtForwardAdd
 ,(round((CAST(case when ltd.EncashWorkingDaysQty >0 then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty else 0 END AS decimal(18,2))) * 2, 0)/2) as Earned

,Balance=ROUND(CAST (A.Opening +CAST(case when ltd.EncashWorkingDaysQty >0 
then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty 
else 0 END AS decimal(18,2))-ISNULL(tav.av, 0)AS decimal(18,2)),0)
 from (
 select A.LeaveYearId CalanderYearID,0 IsExceptionAllowed,a.Id SystemID,A.LeaveTypeId LTSystemID,A.EmployeeId EmployeeID,lt.UserName LeaveName, lt.Description LeaveDescription,lt.LeaveType
 ,FORMAT(LY.FromDate,'dd-MMM-yyyy')FromDate,FORMAT(LY.ToDate,'dd-MMM-yyyy')ToDate,A.Opening 
 from dbo.AnnualLeaveDataCurrent A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
										  LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										  Where LY.FromDate between'" + _FromDate + @"' AND '" + _ToDate + @"' 
										  AND LY.ToDate between'" + _FromDate + @"' AND '" + _ToDate + @"'
                                           AND A.EmployeeId='" + EmployeeSystemId + @"'
										  ) A  
LEFT JOIN (
select A.EmployeeId,A.LeaveTypeId,lt.UserName LeaveName,A.CarryForward from dbo.AnnualLeaveDataPast A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
                                        LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										 Where LY.FromDate between '" + _LFromDate + @"' AND '" + _LToDate + @"' 
										  AND LY.ToDate between '" + _LFromDate + @"' AND '" + _LToDate + @"' AND A.EmployeeId='" + EmployeeSystemId + @"')B ON B.EmployeeId=A.EmployeeId  AND A.LTSystemID=B.LeaveTypeId

left outer join (select ltd.* from dbo.LeavePolicyDetail ltd
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																			SELECT DC.LeavePolicyMasterId,dm.DesignationId FROM MST.DesignationMaster DM
																			LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
																		 ) dm where dm.DesignationId =(select givendesignationId  from dbo.EmployeeInformation where SystemId='" + EmployeeSystemId + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = A.LTSystemID

LEFT JOIN (Select Sum(LTD.LeaveDuration) Rejected,LT.EmpSystemId,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' AND '" + _ToDate + @"' AND IsCancel=1
                                                            group by LT.EmpSystemId,LT.LTSystemID)R ON R.EmpSystemId = A.EmployeeId  and R.LTSystemId = A.LTSystemID
										 left outer join (
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemId,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                            group by LT.EmpSystemId,LT.LTSystemID
														)ltrn on ltrn.EmpSystemId = A.EmployeeId and ltrn.LTSystemId = A.LTSystemID
										 left outer join (
																select sum(c) av,EmpSystemId,LTSystemID from
																(
																	select m.EmpSystemId,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                                        group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemId,LTSystemID
														)tav on tav.EmpSystemId = A.EmployeeId and tav.LTSystemId = A.LTSystemID
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemId,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemId,LTSystemID
														  )acApl  on acApl.EmpSystemId = A.EmployeeId and acApl.LTSystemId = A.LTSystemID
left join (SELECT EmpSystemId,SUM(l.EarnValue)EarnDays,T.Id as LeaveId,ei.PlantId
				FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd ON apd.EmpSystemId=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                JOIN LeaveType T ON t.Id=L.LeaveTypeId
                where apd.workdate between '" + _FromDate + @"' and '" + _ToDate + @"' and t.LeaveType='Earn'
                group by EmpSystemId,t.Id,ei.plantid
                ) as Masterx on Masterx.EmpSystemId=A.EmployeeId and Masterx.LeaveId=A.LTSystemID
				left join (Select top(1) md.* from  ManualLeaveData md
				JOIN LeaveType T ON t.Id=md.LeaveTypeId AND t.LeaveType='Earn'
				Where md.EmployeeId='" + EmployeeSystemId + @"'order by md.addeddate desc
				)med on med.EmployeeId=A.EmployeeId
Where A.EmployeeId='" + EmployeeSystemId + @"')A
LEFT JOIN EmployeeInformation AS ei ON ei.SystemId = A.EmployeeId
LEFT JOIN[MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId = ei.LegalDesignationId
LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId = de.DesignationMasterId AND dmc.PlantId = ei.PlantId
LEFT JOIN mst.DesignationMaster AS dm ON dm.Id = dmc.DesignationMasterId
LEFT JOIN hkp.EmployeeCategory CT ON ct.Id = dm.EmployeeCategoryId
LEFT JOIN org.Plant AS p ON p.Id = ei.PlantId
LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode = PMB.Id
LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id";
                }
                else
                {
                    _sql = @"Select ei.SystemId,A.LTSystemID LeaveTypeId,ei.EmployeeCode,ei.EmployeeName,FORMAT(ei.DOJ,'dd-MMM-yyyy') AS DOJ,p.UserName AS PlantName,D.UserName AS Designation,
DEPT.UserName AS Department,ct.UserName AS EmployeeCategory, A.LeaveName,A.DaysCanBeSanctioned,A.CurrentAllocation CurrentYearAllocation,A.CurrentAllocation,0 YearEndEncash,A.Applied AppliedLeave
,A.PreviousYearCarryForward CarryForwardOpeningBalance,A.BroughtForward,A.LeaveDays,A.Applied AppliedDays,A.Availed AvailedDays,A.Rejected,A.Balance ClosingBalance
 from (SELECT els.CalanderYearID, ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed
                                        ,FromDate=FORMAT(ELS.FromDate,'dd-MMM-yyyy')
										,ToDate=FORMAT(ELS.ToDate,'dd-MMM-yyyy')
										 ,els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID
                                         ,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear
                                     ,DaysCanBeSanctioned= case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END
,CurrentAllocationDCBS=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END
										,els.EncashedInbetween
                                         ,CAST (ISNULL(ltd.IsAvailExceptionAllowedOnSpecialAppeal,0) AS BIT)IsAvailExceptionAllowedOnSpecialAppeal,
                                        CurrentAllocation=ISNULL(els.CurrentYearAllocation, 0),
                                         ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         --ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                                         BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
            --                             BroughtForward=CASE WHEN LT.LeaveType='Earn' THEN	
												--ISNULL(ALP.PBroughtForward, 
												-- CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												-- ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  --ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,


                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
-- LeaveDays=ISNULL(CASE WHEN LT.LeaveType='Earn' THEN ALD.Opening ELSE ISNULL(els.DaysCanBeSanctioned, 0) END,0),
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,
                                         ISNULL(R.Rejected,0)Rejected,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType


-----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
---,IsBroughtForwardAdd=CASE WHEN LT.LeaveType='Earn' THEN
,IsBroughtForwardAdd=CASE WHEN 1=1 THEN  
	CASE WHEN
	-----------------------------------DOJorDOC start -----------------------------------------------------------
								CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            										 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
										   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   							 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
																		  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
										   						END
                                       END
---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
	> GETDATE() then 
		    CONVERT(BIT,0)------No
        ELSE  CONVERT(BIT,1) END---Yes
ELSE CONVERT(BIT,0) END  ---No
,Earned=CAST (0 AS decimal(18,2))
,Balance=(case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END)-ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0)
----------------------------------------------------------------------------------------------------------------------
                                          FROM (    select S.* from trn.EmployeeLeaveSummary S
                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
                                                        where CalanderYearId='" + calYearId + @"' and S.EmployeeId ='" + EmployeeSystemId + @"' AND lp.EncashmentBasis='CalanderYear'
														) els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
        LEFT JOIN (Select Sum(LTD.LeaveDuration) Rejected,LT.EmpSystemId,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' AND '" + _ToDate + @"' AND IsCancel=1
                                                            group by LT.EmpSystemId,LT.LTSystemID)R ON R.EmpSystemId = els.EmployeeId  and R.LTSystemId = els.LeaveTypeId
										 left outer join (
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemId,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                            group by LT.EmpSystemId,LT.LTSystemID
														)ltrn on ltrn.EmpSystemId = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemId,LTSystemID from
																(
																	select m.EmpSystemId,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                                        group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemId,LTSystemID
														)tav on tav.EmpSystemId = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemId,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemId,LTSystemID
														  )acApl  on acApl.EmpSystemId = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId

																		 ) dm where dm.DesignationId =(select givendesignationId from dbo.EmployeeInformation where SystemId='" + EmployeeSystemId + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
LEFT JOIN EmployeeInformation AS emp ON emp.SystemId  = els.EmployeeId
                                                WHERE els.EmployeeID = '" + EmployeeSystemId + @"'
                                              
                                              AND els.LeaveTypeId not IN (select id from LeaveType where IsESIC=1 and IsGeneral=0) AND LT.UserName NOT LIKE '%Maternity%'

UNION ALL
 Select A.CalanderYearID,CAST (0 AS BIT) IsExceptionAllowed,A.FromDate,A.ToDate
 ,a.SystemID,A.LTSystemID,A.EmployeeId EmployeeID,A.LeaveName, A.LeaveDescription,ltd.SystemID LvPolDetailsSystemID,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear
 ,DaysCanBeSanctioned=CAST (B.CarryForward AS decimal(18,2))
 ,CurrentAllocationDCBS=CAST (B.CarryForward AS decimal(18,2)),0 EncashedInbetween,CAST (0 AS BIT) IsAvailExceptionAllowedOnSpecialAppeal
 ,CurrentAllocation=(select ((SELECT DATEDIFF(day,  cast(YEAR('" + _FromDate + @"') as char(4)),  cast(YEAR('" + _FromDate + @"')+1 as char(4))))-COUNT(d.OffDayDate))/ltd.EncashWorkingDaysQty
		                                from scs.OffDayDetail d
		                                inner join scs.OffDayMaster m on d.offdaymasterid=m.id and OffDayType in ('H','W')
		                                and d.OffDayDate between '" + _FromDate + @"' AND '" + _ToDate + @"') 
,CAST (0 AS decimal(18,2)) PreviousYearCarryForward
 --,BroughtForward=CASE WHEN A.Opening>B.CarryForward THEN A.Opening WHEN B.BroughtForward>B.CarryForward THEN B.BroughtForward ELSE B.CarryForward END
 ,BroughtForward=CAST (A.Opening AS decimal(18,2))
 
  ,LeaveDays=ROUND(CAST (A.Opening +CAST(case when ltd.EncashWorkingDaysQty >0 
then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty 
else 0 END AS decimal(18,2))-ISNULL(tav.av, 0)AS decimal(18,2)),0)
 ,ISNULL(ltrn.ldays, 0) Applied,ISNULL(tav.av, 0) Availed,ISNULL(R.Rejected,0) Rejected,ISNULL(acApl.ldays,0) ldays,A.LeaveType,CAST (0 AS BIT) IsBroughtForwardAdd
 ,(round((CAST(case when ltd.EncashWorkingDaysQty >0 then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty else 0 END AS decimal(18,2))) * 2, 0)/2) as Earned

,Balance=ROUND(CAST (A.Opening +CAST(case when ltd.EncashWorkingDaysQty >0 
then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty 
else 0 END AS decimal(18,2))-ISNULL(tav.av, 0)AS decimal(18,2)),0)
 from (
 select A.LeaveYearId CalanderYearID,0 IsExceptionAllowed,a.Id SystemID,A.LeaveTypeId LTSystemID,A.EmployeeId EmployeeID,lt.UserName LeaveName, lt.Description LeaveDescription,lt.LeaveType
 ,FORMAT(LY.FromDate,'dd-MMM-yyyy')FromDate,FORMAT(LY.ToDate,'dd-MMM-yyyy')ToDate,A.Opening 
 from dbo.AnnualLeaveDataCurrent A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
										  LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										  Where LY.FromDate between'" + _FromDate + @"' AND '" + _ToDate + @"' 
										  AND LY.ToDate between'" + _FromDate + @"' AND '" + _ToDate + @"'
										  ) A  
LEFT JOIN (
select BroughtForward=CASE WHEN A.Adjustment=0 THEN A.Opening ELSE A.Adjustment END,A.EmployeeId,A.LeaveTypeId,lt.UserName LeaveName,A.CarryForward from dbo.AnnualLeaveDataPast A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
                                        LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										  Where LY.FromDate between '" + _LFromDate + @"' AND '" + _LToDate + @"' 
										  AND LY.ToDate between '" + _LFromDate + @"' AND '" + _LToDate + @"')B ON B.EmployeeId=A.EmployeeId  AND A.LTSystemID=B.LeaveTypeId

left outer join (select ltd.* from dbo.LeavePolicyDetail ltd
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																			SELECT DC.LeavePolicyMasterId,dm.DesignationId FROM MST.DesignationMaster DM
																			LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
																		 ) dm where dm.DesignationId =(select givendesignationId  from dbo.EmployeeInformation where SystemId='" + EmployeeSystemId + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = A.LTSystemID

LEFT JOIN (Select Sum(LTD.LeaveDuration) Rejected,LT.EmpSystemId,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' AND '" + _ToDate + @"' AND IsCancel=1
                                                            group by LT.EmpSystemId,LT.LTSystemID)R ON R.EmpSystemId = A.EmployeeId  and R.LTSystemId = A.LTSystemID
										 left outer join (
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemId,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                            group by LT.EmpSystemId,LT.LTSystemID
														)ltrn on ltrn.EmpSystemId = A.EmployeeId and ltrn.LTSystemId = A.LTSystemID
										 left outer join (
																select sum(c) av,EmpSystemId,LTSystemID from
																(
																	select m.EmpSystemId,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                                        group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemId,LTSystemID
														)tav on tav.EmpSystemId = A.EmployeeId and tav.LTSystemId = A.LTSystemID
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemId,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemId,LTSystemID
														  )acApl  on acApl.EmpSystemId = A.EmployeeId and acApl.LTSystemId = A.LTSystemID
left join (SELECT EmpSystemId,SUM(l.EarnValue)EarnDays,T.Id as LeaveId,ei.PlantId
				FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd ON apd.EmpSystemId=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                JOIN LeaveType T ON t.Id=L.LeaveTypeId
                where apd.workdate between '" + _FromDate + @"' and '" + _ToDate + @"' and t.LeaveType='Earn'
                group by EmpSystemId,t.Id,ei.plantid
                ) as Masterx on Masterx.EmpSystemId=A.EmployeeId and Masterx.LeaveId=A.LTSystemID
				left join (Select top(1) md.* from  ManualLeaveData md
				JOIN LeaveType T ON t.Id=md.LeaveTypeId AND t.LeaveType='Earn'
				Where md.EmployeeId='" + EmployeeSystemId + @"' order by md.addeddate desc
				)med on med.EmployeeId=A.EmployeeId
Where A.EmployeeId='" + EmployeeSystemId + @"')A
LEFT JOIN EmployeeInformation AS ei ON ei.SystemId = A.EmployeeId
LEFT JOIN[MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId = ei.LegalDesignationId
LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId = de.DesignationMasterId AND dmc.PlantId = ei.PlantId
LEFT JOIN mst.DesignationMaster AS dm ON dm.Id = dmc.DesignationMasterId
LEFT JOIN hkp.EmployeeCategory CT ON ct.Id = dm.EmployeeCategoryId
LEFT JOIN org.Plant AS p ON p.Id = ei.PlantId
LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode = PMB.Id
LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id";
                }
                return _sqlRepository.GetDataCollection(_sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function
        public DataSet GetESICEligibleEmployeeFromEnum(string empSystemId, string FromDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT IsEligible,SalaryStructureId,EmpSystemId,m.EffectiveDate
                                  FROM [dbo].[EmployeeEligibleForSalaryHeadEnum] n
                                  left join (select SystemID,EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster
                                  union
                                  select SystemID,EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster
                                  )
                                   mm on mm.SystemID=n.SalaryStructureId
                                  inner join (
                                  select MAX(EffectiveDate)EffectiveDate,EmpInfoSystemID from (
                                  select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster where IsApproved=1 
                                  union
                                   select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster where IsApproved=1 
                                   ) x 
                                   group by EmpInfoSystemID
                                  )m on mm.EffectiveDate=m.EffectiveDate and m.EmpInfoSystemID=mm.EmpInfoSystemID
                                  where SalaryHeadEnum='ESIC' and mm.EmpInfoSystemID='" + empSystemId + @"'  and IsEligible=1
                                 "
            };//and EffectiveDate<='" + FromDate + @"'
            //var data = _sqlRepository.GetDataCollection(CmdText);
            return _sqlRepository.GetGridData(parameters).Source;
        }
        public DataSet GetCalYearInfo(string CalYearId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"select * from YearlyCalendar WHERE ID='" + CalYearId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }//End Function

        public DataSet GetCalYear(string CalYearId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"select format(FromDate,'dd-MMM-yyyy')FromDate,format(ToDate,'dd-MMM-yyyy')ToDate, year(ToDate)YearNo from YearlyCalendar WHERE ID='" + CalYearId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }//End Function
        public DataSet GetLeaveBalanceType(string sGroupID, string sPlantID, string EmpSystemID, string calYearId, string ToDate)
        {

            try
            {
                string _FromDate = string.Empty;
                string _ToDate = ToDate;
                var startFromDate = Convert.ToDateTime(ToDate);
                var y = startFromDate.Year;
                _FromDate = "1-Jan-" + y;
                // var esic = GetESICEligibleEmployee(EmpSystemID);
                var dsCalYear = GetCalYearInfo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                    //  _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
                }
                else
                {
                    //throw new Exception("No Year found...");
                }
                var esic = GetESICEligibleEmployeeFromEnum(EmpSystemID, ToDate);

                if (esic.Tables[0].Rows.Count > 0)
                {
                    GridParameter parameters = null;
                    parameters = new GridParameter
                    {
                        ExportType = "DATASET",
                        CmdText = @"SELECT	els.CalanderYearID,ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed,
										 els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID,
                                         --ltd.IsProrataPreviousyear,
                                         ltd.IsProratacurrentyear
                                        ---,els.DaysCanBeSanctioned
										 ,DaysCanBeSanctioned=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end
                                                                    ,CurrentAllocationDCBS=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end

                                        ,els.EncashedInbetween
                                        ,ltd.IsAvailExceptionAllowedOnSpecialAppeal,
										 0.00 Balance,
                                        ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation,
                                        ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         --ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                                         ---BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
                                         BroughtForward=
										 CASE WHEN LT.LeaveType='Earn' THEN
										 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END
										 
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END


                                         ,ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType
                                            
                                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
                                            ---,IsBroughtForwardAdd=CASE WHEN LT.LeaveType='Earn' THEN  
                                            ,IsBroughtForwardAdd=CASE WHEN 1=1 THEN  
                                            	CASE WHEN
                                            	-----------------------------------DOJorDOC start -----------------------------------------------------------
                                            								CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                                                                        										 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
                                            																	      WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
                                            																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
                                            										   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
                                            										   							 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
                                            																		  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
                                            																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
                                            										   						END
                                                                                   END
                                            ---------------------------------------DOJorDOC start  end-------------------------------------------------------
                                            	
                                            	> GETDATE() then 
                                            		    CONVERT(BIT,0)------No
                                                    ELSE  CONVERT(BIT,1) END---Yes
                                            ELSE CONVERT(BIT,0) END  ---No
                                            
                                            ----------------------------------------------------------------------------------------------------------------------



                                          FROM (select * from trn.EmployeeLeaveSummary where CalanderYearId in (select Id from YearlyCalendar where PlantId in (select PlantId from EmployeeInformation where SystemId='" + EmpSystemID + "')  and YearNo =year('" + _ToDate + "')) and PlantId in  (select PlantId from EmployeeInformation where SystemId='" + EmpSystemID + "') and EmployeeId ='" + EmpSystemID + @"' ) els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
                                        LEFT JOIN EmployeeInformation AS emp ON emp.SystemId  = els.EmployeeId
										 left outer join (
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
															Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
															Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
															group by LT.EmpSystemID,LT.LTSystemID
														 )ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (														
															Select Sum(LTD.LeaveDuration) av,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
															Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
															Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"' and LTD.IsAvailed=1
															group by LT.EmpSystemID,LT.LTSystemID
														  )tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where	IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId 
																										FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC 
																													ON DM.Id=DC.DesignationMasterId
																						where dc.plantid in (select PlantId from EmployeeInformation where SystemId='" + EmpSystemID + @"')

																		 ) dm where dm.DesignationId =(select givendesignationId 
																									 from dbo.EmployeeInformation 
																									 where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
                                                WHERE els.EmployeeID = '" + EmpSystemID + @"'
                                              AND CalanderYearID in(select Id from YearlyCalendar where PlantId in (select PlantId from EmployeeInformation where SystemId='" + EmpSystemID + "')  and YearNo =year('" + ToDate + @"'))
                                             AND els.LeaveTypeId IN ( --IN


                                            SELECT LT.ID FROM dbo.ESICPolicyLeaveType AS EPLT
                                                      LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                                                      WHERE LT.Code in('CL','PL') AND
                                                      EPLT.LeaveTypeID IN
                                                       (
                                                         SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                                                      LEFT JOIN  (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId WHERE DC.PlantId in (select PlantId from EmployeeInformation where SystemId='" + EmpSystemID + @"')) AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                                                      LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                                                      WHERE EI.SystemID='" + EmpSystemID + @"' AND EI.GroupID='" + sGroupID + @"' AND EI.PlantID in (select PlantId from EmployeeInformation where SystemId='" + EmpSystemID + @"')
                                                       )
                                                    AND
                                                    EPLT.ESICPolicyMasterID IN (
                                                     SELECT DM.ESICPolicyMasterID FROM (SELECT DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                    WHERE DC.PlantId in (select PlantId from EmployeeInformation where SystemId='" + EmpSystemID + @"')) DM
                                                     WHERE DM.DesignationId IN (
                                                      SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + EmpSystemID + @"'
                                                      )
                                                    )

                                            				                                    )--IN

"
                    };
                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else
                {
                    GridParameter parameters = null;
                    parameters = new GridParameter
                    {
                        ExportType = "DATASET",
                        CmdText = @"SELECT	els.CalanderYearID, ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed,
										 els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID
                                         --ISNULL(ltd.IsProrataPreviousyear,0)IsProrataPreviousyear,
                                         ,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear
                                        
                                         ---,els.DaysCanBeSanctioned
										 ,DaysCanBeSanctioned=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end
 ,CurrentAllocationDCBS=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end

                                        ,ISNULL(ltd.IsAvailExceptionAllowedOnSpecialAppeal,0)IsAvailExceptionAllowedOnSpecialAppeal,
										 0.00 Balance,
                                        ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation,
                                         --ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         --ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                                         ---BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
                                         BroughtForward=
										 CASE WHEN LT.LeaveType='Earn' THEN
										 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END
										 
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END


                                         ,ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,els.EncashedInbetween,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType


-----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
---,IsBroughtForwardAdd=CASE WHEN LT.LeaveType='Earn' THEN
,IsBroughtForwardAdd=CASE WHEN 1=1 THEN  
	CASE WHEN
	-----------------------------------DOJorDOC start -----------------------------------------------------------
								CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            										 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
										   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   							 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
																		  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
										   						END
                                       END
---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
	> GETDATE() then 
		    CONVERT(BIT,0)------No
        ELSE  CONVERT(BIT,1) END---Yes
ELSE CONVERT(BIT,0) END  ---No

----------------------------------------------------------------------------------------------------------------------
                                          FROM (select * from trn.EmployeeLeaveSummary where CalanderYearId in (select Id from YearlyCalendar where PlantId in (select PlantId from EmployeeInformation where SystemId='" + EmpSystemID + "')  and YearNo =year('" + _ToDate + "')) and PlantId in  (select PlantId from EmployeeInformation where SystemId='" + EmpSystemID + "') and EmployeeId ='" + EmpSystemID + @"' ) els

                                         left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
                            where  (FromDate between '" + _FromDate + @"' and '" + _ToDate + @"') and (ToDate between '" + _FromDate + @"' and '" + _ToDate + @"')
                                                    group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                                        group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where	IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId 
																										FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC 
																													ON DM.Id=DC.DesignationMasterId
																						where dc.plantid in (select PlantId from EmployeeInformation where SystemId='" + EmpSystemID + @"')

																		 ) dm where dm.DesignationId =(select givendesignationId 
																									 from dbo.EmployeeInformation 
																									 where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
LEFT JOIN EmployeeInformation AS emp ON emp.SystemId  = els.EmployeeId
                                                WHERE els.EmployeeID = '" + EmpSystemID + @"'                                             
                                              AND CalanderYearID in(select Id from YearlyCalendar where PlantId in (select PlantId from EmployeeInformation where SystemId='" + EmpSystemID + "')  and YearNo =year('" + ToDate + @"'))
                                              AND els.LeaveTypeId not IN 
                                            (select id from LeaveType where IsESIC=1 and IsGeneral=0) and lt.Code in('CL','PL') AND lt.LeaveType <>'Maternity'"
                    };
                    parameters.sort = "LeaveName";
                    parameters.order = "ASC";
                    return _sqlRepository.GetGridData(parameters).Source;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function

        public IEnumerable<object> LoadGrdAllocatedLvDetails(string companyGroupId, string plantId, string employeeId, string calanderYearId, string ToDate)
        {
            //DataSet dsLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            try
            {
                var dsLvAllo = GetLeaveBalanceType(companyGroupId, plantId, employeeId, calanderYearId, ToDate);

                dvLocal = new DataView();
                dvLocal.Table = dsLvAllo.Tables[0];
                bool proDataPrevYear = false;
                bool proDataCurrentYear = false;
                bool isAvailExceptionAllowed = false;
                List<object> ss = new List<object>();

                object ob = new object { };

                for (int i = 0; i < dsLvAllo.Tables[0].Rows.Count; i++)
                {
                    dvLocal.RowFilter = "LvPolDetailsSystemID = '" + dsLvAllo.Tables[0].Rows[i]["LvPolDetailsSystemID"].ToString().Trim() + "'";
                    if (dvLocal.Count == 1)
                    {
                        drLocal = dvLocal[0].Row;
                        drLocal.BeginEdit();
                        //proDataPrevYear = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsProrataPreviousyear"].ToString());
                        proDataCurrentYear = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsProratacurrentyear"].ToString());
                        isAvailExceptionAllowed = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsAvailExceptionAllowedOnSpecialAppeal"].ToString());

                        drLocal["Applied"] = dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim();
                        drLocal["Availed"] = dsLvAllo.Tables[0].Rows[i]["Availed"].ToString().Trim();
                        drLocal["BroughtForward"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["BroughtForward"].ToString().Trim());
                        decimal DaysCanBeSanctioned = 0;

                        decimal BroughtForward = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["BroughtForward"].ToString().Trim());
                        DaysCanBeSanctioned = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["DaysCanBeSanctioned"].ToString().Trim());

                        decimal EncashedInbetween = 0;
                        if (!string.IsNullOrEmpty(dsLvAllo.Tables[0].Rows[i]["EncashedInbetween"].ToString()))
                        {
                            EncashedInbetween = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["EncashedInbetween"].ToString().Trim());
                        }
                        drLocal["EncashedInbetween"] = EncashedInbetween;
                        bool IsBroughtForwardAdd = true;
                        IsBroughtForwardAdd = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsBroughtForwardAdd"].ToString());
                        decimal TotalEarn = 0;
                        if (IsBroughtForwardAdd)
                        {
                            TotalEarn = BroughtForward + DaysCanBeSanctioned;
                        }
                        else
                        {
                            TotalEarn = DaysCanBeSanctioned;
                        }


                        if (dsLvAllo.Tables[0].Rows[i]["LeaveType"].ToString().Trim().ToUpper() != "EARN")
                        {
                            if (proDataCurrentYear == false)
                            {
                                #region 01

                                //drLocal["LeaveDays"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim());
                                //drLocal["Balance"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim()) - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim());
                                if (IsBroughtForwardAdd)
                                {

                                    drLocal["LeaveDays"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim()) + BroughtForward;
                                    drLocal["Balance"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim()) + BroughtForward - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim());
                                    //TotalEarn = BroughtForward + DaysCanBeSanctioned;
                                }
                                else
                                {
                                    //TotalEarn = DaysCanBeSanctioned;
                                    drLocal["LeaveDays"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim());
                                    drLocal["Balance"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim()) - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim());
                                }


                                #endregion
                            }
                            else
                            {
                                #region 02

                                drLocal["LeaveDays"] = TotalEarn;
                                drLocal["Balance"] = TotalEarn - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim());

                                #endregion
                            }
                        }
                        else
                        {
                            drLocal["LeaveDays"] = TotalEarn;
                            drLocal["Balance"] = TotalEarn - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim()) - EncashedInbetween;

                        }



                        drLocal.EndEdit();

                    }
                }

                var list = new List<LeaveTransactionVM>();
                list = ConvertDataTable<LeaveTransactionVM>(dsLvAllo.Tables[0]);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //dsLvAllo = null;
            }
        }//End Function

        private static T GetItem<T>(DataRow dr)
        {
            var temp = typeof(T);
            var obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        if (dr[column.ColumnName] == DBNull.Value)
                            dr[column.ColumnName] = "";
                        pro.SetValue(obj, dr[column.ColumnName], null);
                        break;
                    }
                }
            }
            return obj;
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            try
            {
                var data = new List<T>();
                foreach (DataRow row in dt.Rows)
                {
                    var item = GetItem<T>(row);
                    data.Add(item);
                }
                return data;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        #endregion

    }
}
