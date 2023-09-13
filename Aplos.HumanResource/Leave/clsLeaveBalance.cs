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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Leave
{
    public class clsLeaveBalance
    {
        ISqlRepository _sqlRepository;
        public clsLeaveBalance()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetEmp(string plantId, string companyId, string calYearId)
        {
            string _FromDate = string.Empty;
            string _ToDate = string.Empty;
            var dsCalYear = GetCalYearInfo(calYearId);
            if (dsCalYear.Tables[0].Rows.Count > 0)
            {
                _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
            }
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,LGD.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        , L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodeNumeric, EMP.FatherName,FORMAT( EMP.DOB,'dd-MMM-yyyy')DOB,ec.UserName EmpCategory,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
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
                                        WHERE emp.PlantID='" + plantId + @"'  and EMP.CompanyId='" + companyId + @"' and EMP.EmployeeStatus='Active' 
                                        and EMP.DOJ <= ( '" + _ToDate + @"') and (emp.DOS is null or emp.DOS >= '" + _FromDate + @"')
                                        ORDER BY EmployeeCodePreFix,EMP.EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IWorkbook XlsLeaveBalanceRpt(string PlantId, string sGroup, string Year)
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
            string _ToDate = string.Empty;
            string _YearNo = string.Empty;

            // var esic = GetESICEligibleEmployee(EmpSystemID);
            var dsCalYear = GetCalYear(Year);
            if (dsCalYear.Tables[0].Rows.Count > 0)
            {
                _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
                _YearNo = dsCalYear.Tables[0].Rows[0]["YearNo"].ToString();
            }

            workbook.Version = ExcelVersion.Excel2016;
            objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);
            objRpt.SelectedPlant(PlantId, out dsFactory);
            var sheet = workbook.Worksheets[0];

            sheet.Name = "LeaveRegisterReport";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;
            var dsLvAllo = GetLeaveBalanceType(sGroup, PlantId, Year);

            var finalList = LoadGrdAllocatedLvDetails(dsLvAllo);

            #region Headers


            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 12, ExcelHAlign.HAlignLeft);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 25, ExcelHAlign.HAlignLeft);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DOJ", 25, ExcelHAlign.HAlignLeft);
            int ColDoj = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 15, ExcelHAlign.HAlignLeft);
            int ColLegalDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 15, ExcelHAlign.HAlignLeft);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeCategory", 20, ExcelHAlign.HAlignLeft);
            int ColEmployeeCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Leave Name", 20, ExcelHAlign.HAlignLeft);
            int ColLeaveName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Brought Forward", 20, ExcelHAlign.HAlignLeft);
            int ColBroughtForward = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Current Year Allocation", 20, ExcelHAlign.HAlignLeft);
            int ColCurrentAllocation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Leave Days Allowed", 20, ExcelHAlign.HAlignLeft);
            int ColLeaveDays = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Applied", 20, ExcelHAlign.HAlignLeft);
            int ColApplied = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Availed", 20, ExcelHAlign.HAlignLeft);
            int ColAvailed = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Encashed", 20, ExcelHAlign.HAlignLeft);
            int ColEncashedInbetween = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Balance", 20, ExcelHAlign.HAlignLeft);
            int ColBalance = COL;

            sheet.Range[ROW, 1, ROW, COL].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
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

            foreach (var item in finalList)
            {
                sheet[ROW, ColEmployeeCode].Text = item.EmployeeCode;
                sheet[ROW, ColEmployeeName].Text = item.EmployeeName;
                sheet[ROW, ColDoj].Text = item.DOJ;
                sheet[ROW, ColLegalDesignation].Text = item.Designation;
                sheet[ROW, ColDepartment].Text = item.Department;
                sheet[ROW, ColEmployeeCategory].Text = item.EmployeeCategory;
                sheet[ROW, ColLeaveName].Text = item.LeaveName;
                sheet[ROW, ColCurrentAllocation].Number = (double)(item.CurrentAllocation);
                sheet[ROW, ColBroughtForward].Number = (double)(item.BroughtForward);
                sheet[ROW, ColLeaveDays].Number = (double)(item.LeaveDays);
                sheet[ROW, ColApplied].Number = (double)(item.Applied);
                sheet[ROW, ColAvailed].Number = (double)(item.Availed);
                sheet[ROW, ColEncashedInbetween].Number = (double)(item.EncashedInbetween);
                sheet[ROW, ColBalance].Number = (double)(item.Balance);

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }

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


                    if (_ob_source.LeaveType.ToString().Trim().ToUpper() != "EARN")
                    {
                        if (proDataCurrentYear == false)
                        {
                            #region 01
                            if (IsBroughtForwardAdd)
                            {

                                _ob_r.LeaveDays = Convert.ToDecimal(_ob_source.CurrentAllocationDCBS) + BroughtForward;
                                _ob_r.Balance = Convert.ToDecimal(_ob_source.CurrentAllocationDCBS) + BroughtForward - Convert.ToDecimal(_ob_source.Applied);
                                //TotalEarn = BroughtForward + DaysCanBeSanctioned;
                            }
                            else
                            {
                                //TotalEarn = DaysCanBeSanctioned;
                                _ob_r.LeaveDays = Convert.ToDecimal(_ob_source.CurrentAllocationDCBS);
                                _ob_r.Balance = Convert.ToDecimal(_ob_source.CurrentAllocationDCBS) - Convert.ToDecimal(_ob_source.Applied);
                            }
                            //_ob_r.LeaveDays = Convert.ToDecimal(_ob_source.CurrentAllocationDCBS);
                            //_ob_r.Balance = Convert.ToDecimal(_ob_source.CurrentAllocationDCBS.ToString().Trim()) - Convert.ToDecimal(_ob_source.Applied.ToString().Trim());

                            #endregion
                        }
                        else
                        {
                            #region 02

                            _ob_r.LeaveDays = TotalEarn;
                            _ob_r.Balance = TotalEarn - Convert.ToDecimal(_ob_source.Applied.ToString().Trim());

                            #endregion
                        }
                    }
                    else
                    {
                        _ob_r.LeaveDays = TotalEarn;
                        _ob_r.Balance = TotalEarn - Convert.ToDecimal(_ob_source.Applied.ToString().Trim()) - EncashedInbetween;

                    }
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
        public IEnumerable<EmployeeDetails> xLoadGrdAllocatedLvDetails(DataSet dsLvAllo)
        {
            //DataSet dsLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            try
            {

                dvLocal = new DataView();
                dvLocal.Table = dsLvAllo.Tables[0];
                bool proDataPrevYear = false;
                bool proDataCurrentYear = false;
                bool isAvailExceptionAllowed = false;
                List<object> ss = new List<object>();

                object ob = new object { };

                for (int i = 0; i < dsLvAllo.Tables[0].Rows.Count; i++)
                {
                    //dvLocal.RowFilter = "LvPolDetailsSystemID = '" + dsLvAllo.Tables[0].Rows[i]["LvPolDetailsSystemID"].ToString().Trim() + "'";
                    dvLocal.RowFilter = "SystemID = '" + dsLvAllo.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                    if (dvLocal.Count == 1)
                    {
                        drLocal = dvLocal[0].Row;
                        drLocal.BeginEdit();
                        //proDataPrevYear = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsProrataPreviousyear"].ToString());
                        proDataCurrentYear = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsProratacurrentyear"].ToString());
                        isAvailExceptionAllowed = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsAvailExceptionAllowedOnSpecialAppeal"].ToString());
                        drLocal["EmployeeCode"] = dsLvAllo.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim();
                        drLocal["EmployeeName"] = dsLvAllo.Tables[0].Rows[i]["EmployeeName"].ToString().Trim();
                        drLocal["Designation"] = dsLvAllo.Tables[0].Rows[i]["Designation"].ToString().Trim();
                        drLocal["Department"] = dsLvAllo.Tables[0].Rows[i]["Department"].ToString().Trim();
                        drLocal["EmployeeCategory"] = dsLvAllo.Tables[0].Rows[i]["EmployeeCategory"].ToString().Trim();
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

                                drLocal["LeaveDays"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocation"].ToString().Trim());
                                drLocal["Balance"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocation"].ToString().Trim()) - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim());

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

                var list = new List<EmployeeDetails>();
                list = dsLvAllo.Tables[0].ToList<EmployeeDetails>();
                //list = ConvertDataTable<LeaveTransactionVM>(dsLvAllo.Tables[0]);
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

        public DataSet GetLeaveBalanceType(string sGroupID, string sPlantID, string calYearId)
        {

            try
            {
                string _FromDate = string.Empty;
                string _ToDate = string.Empty;

                // var esic = GetESICEligibleEmployee(EmpSystemID);
                var dsCalYear = GetCalYearInfo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                    _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
                }
                else
                {
                    throw new Exception("No Year found...");
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

                    CmdText = @"SELECT	els.LeaveTypeId LTSystemID,els.CalanderYearID,EMP.EmployeeName,EMP.EmployeeCode, PL.UserName PlantName, D.UserName Designation, DEPT.UserName Department, ec.UserName EmployeeCategory, ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed,
										 els.Id SystemID,format(EMP.DOJ,'dd-MMM-yyyy')DOJ,                                         
                                         els.EmployeeID,lt.UserName LeaveName,lt.Description LeaveDescription
                                         ,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear,
                                          ISNULL(ltd.IsAvailExceptionAllowedOnSpecialAppeal,0)IsAvailExceptionAllowedOnSpecialAppeal,										 
                                        ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation
,DaysCanBeSanctioned=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end
 ,CurrentAllocationDCBS=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end,
                                       --BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
									     ISNULL(ALP.BroughtForward,0)BroughtForward,
                                         --ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
										 LeaveDays=ISNULL(CASE WHEN LT.LeaveType='Earn' THEN ALD.Opening ELSE ISNULL(els.DaysCanBeSanctioned, 0) END,0),
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,els.EncashedInbetween,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType


                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------   
                            ,IsBroughtForwardAdd=CASE WHEN 1=1 THEN  
                            	CASE WHEN
                            	-----------------------------------DOJorDOC start -----------------------------------------------------------
								CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            										 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter, emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter, emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter, emp.DOJ ) END
										   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   							 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter, emp.DOC  )
																		  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter, emp.DOC  )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter, emp.DOC  )
										   						END
                                       END
---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
	> GETDATE() then 
		    CONVERT(BIT,0)------No
        ELSE  CONVERT(BIT,1) END---Yes
ELSE CONVERT(BIT,0) END  ---No
,Balance= ISNULL(CASE WHEN LT.LeaveType='Earn' THEN 
(case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end)+ISNULL(ALP.BroughtForward,0)- ISNULL(tav.av, 0)
ELSE (ISNULL(els.DaysCanBeSanctioned, 0)- ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0)) END,0)
----------------------------------------------------------------------------------------------------------------------
                                          FROM (select * from trn.EmployeeLeaveSummary where CalanderYearId='" + calYearId + @"' and PlantId ='" + sPlantID + @"'
										  UNION 
										 select S.* from trn.EmployeeLeaveSummary S
                                                        JOIN  trn.EmployeeLeaveSummary SS ON S.Id=ss.Id
                                                        AND S.Id=(SELECT TOP 1 SX.Id FROM trn.EmployeeLeaveSummary SX WHERE ss.EmployeeId=SX.EmployeeId AND ss.LeaveTypeId=SX.LeaveTypeId ORDER BY sx.ToDate DESC)
                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
                                                        where lp.EncashmentBasis<>'CalanderYear'
										  ) els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
										  LEFT JOIN
											(
										  select BroughtForward=CASE WHEN A.Opening=0 THEN A.Adjustment ELSE A.Opening END,A.EmployeeId,A.LeaveTypeId from dbo.AnnualLeaveDataPast A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
											)ALP ON ALP.EmployeeId=els.EmployeeId AND lt.Id=ALP.LeaveTypeId
											LEFT JOIN
											(
										   select A.Opening,A.EmployeeId,A.LeaveTypeId,FORMAT(LY.FromDate,'dd-MMM-yyyy')FromDate,FORMAT(LY.ToDate,'dd-MMM-yyyy')ToDate from dbo.AnnualLeaveDataCurrent A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
										  LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
											)ALD ON ALD.EmployeeId=els.EmployeeId AND lt.Id=ALD.LeaveTypeId
											
										 left outer join (
															select sum(LTD.LeaveDuration) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
															Left Join LeaveTransactionDetails LTD on m.SystemID=LTD.LvTrnsSystemID
                            where  (WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"') 
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
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (--1
										 select d.*,e.SystemId empsystemid from dbo.EmployeeInformation e
inner join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = e.LegalDesignationId
inner join MST.DesignationMaster dm on dm.Id = dml.DesignationMasterId
inner join SCS.DesignationMasterConfiguration DC on DC.DesignationMasterId = dm.Id and dc.PlantId = e.PlantId
inner join LeavePolicyMaster lm on lm.SystemID = dc.LeavePolicyMasterId
inner join dbo.LeavePolicyDetail d on d.LPMSystemID = lm.SystemID
)ltd on ltd.LTSystemID = lt.Id and ltd.empsystemid=els.EmployeeId

										left JOIN EmployeeInformation AS emp ON emp.SystemId  = els.EmployeeId and els.PlantId ='" + sPlantID + @"' 
										LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId                                        
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=EMP.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
WHERE (emp.DOS is null or emp.DOS >= '" + _FromDate + @"') and EMP.EmployeeStatus='Active' and (emp.DOJ <= '" + _ToDate + @"') and PL.Id='" + sPlantID + @"' AND LT.UserName NOT LIKE '%Maternity%'"

                };
                parameters.sort = "LeaveName";
                parameters.order = "ASC";
                return _sqlRepository.GetGridData(parameters).Source;
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

        #endregion

        //first dictionary will contain employeeId,second one will hold the leave type wise balances
        public Dictionary<string, Dictionary<string, DataRow>> GetLeaveBalance(int MonthNo, int YearNo, string EmployeeIds, string UpToDate = "", string LeaveTypeIds = "")
        {
            //int dhruv = 0;
            try
            {
                if (string.IsNullOrEmpty(UpToDate))
                    UpToDate = new DateTime(YearNo, MonthNo, DateTime.DaysInMonth(YearNo, MonthNo)).ToString("dd-MMM-yyyy");

                if (LeaveTypeIds != "")
                    LeaveTypeIds = " AND lt.Id IN (" + LeaveTypeIds + @") ";
                string sql = @"SELECT lt.Sequence, ELs.EmployeeId, els.LeaveTypeId,lt.LeaveType, lt.Code AS LeaveCode,
                                els.CurrentYearAllocation,els.CarryForwardOpeningBalance,els.BroughtForward,
                                SUM(ISNULL(CR.EarnedValue,0))CurrentPeriodEarned,SUM(ISNULL(cr.AvailedValue,0)) AS CurrentPeriodAvailed,
                                 CASE WHEN lt.LeaveType<>'Earn' THEN  ISNULL(els.CurrentYearAllocation,0)+ISNULL(els.CarryForwardOpeningBalance,0)+ISNULL(els.BroughtForward,0)+SUM(ISNULL(CR.EarnedValue,0)) 
                                 ELSE ISNULL(els.CarryForwardOpeningBalance,0)+ISNULL(els.BroughtForward,0)+SUM(ISNULL(CR.EarnedValue,0))  END
                                 AS UpToDateEarned,

                                CASE WHEN lt.LeaveType<>'Earn' THEN ISNULL(els.CurrentYearAllocation,0)+ISNULL(els.CarryForwardOpeningBalance,0)+ISNULL(els.BroughtForward,0)+SUM(ISNULL(CR.EarnedValue,0))-SUM(ISNULL(cr.AvailedValue,0)) 
                                ELSE ISNULL(els.CarryForwardOpeningBalance,0)+ISNULL(els.BroughtForward,0)+SUM(ISNULL(CR.EarnedValue,0))-SUM(ISNULL(cr.AvailedValue,0)) END
								AS ClosingBalance

                                FROM trn.EmployeeLeaveSummary AS els 
                                LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=els.EmployeeId
                                JOIN YearlyCalendar AS yc ON  yc.PlantId=ei.PlantId and yc.Id=els.CalanderYearId AND yc.YearNo=(SELECT TOP 1 y.YearNo FROM YearlyCalendar Y WHERE y.PlantId=ei.PlantId AND y.YearNo<=" + YearNo + @" ORDER BY y.YearNo DESC)
                                LEFT JOIN (
                                SELECT d.YearNo,d.MonthNo, d.EmployeeSystemId,D.LeaveTypeId,SUM( d.EarnedValue) AS EarnedValue, SUM(d.AvailedValue) AS AvailedValue
			                                  FROM SalaryProcessMonthlyLeaveData D
			                                GROUP BY d.YearNo,d.MonthNo, d.EmployeeSystemId,D.LeaveTypeId
                                ) AS CR ON cr.EmployeeSystemId=els.EmployeeId AND cr.LeaveTypeId=els.LeaveTypeId AND DATEFROMPARTS(CR.YearNo,CR.MonthNo,1) BETWEEN yc.FromDate AND '" + UpToDate + @"'
                                LEFT JOIN LeaveType AS lt ON lt.Id=els.LeaveTypeId

                                WHERE yc.YearNo='" + YearNo + @"'
                                AND els.EmployeeId IN (" + EmployeeIds + @")" + LeaveTypeIds + @" 


                                GROUP BY lt.Sequence, ELs.EmployeeId, els.LeaveTypeId,lt.LeaveType, lt.Code,els.CurrentYearAllocation,els.CarryForwardOpeningBalance,els.BroughtForward--,,

                                ORDER BY  ELs.EmployeeId,lt.Sequence";

                DataTable dt = _sqlRepository.GetDataTable(sql);

                Dictionary<string, Dictionary<string, DataRow>> empLeaveData = new Dictionary<string, Dictionary<string, DataRow>>();
                Dictionary<string, DataRow> LeaveData = new Dictionary<string, DataRow>();
                string employeeId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (i == 1116)
                    { 

                    }
                    //dhruv = i;
                    string empx = dt.Rows[i]["EmployeeId"].ToString();
                    if (employeeId != dt.Rows[i]["EmployeeId"].ToString())
                    {
                        LeaveData = new Dictionary<string, DataRow>();
                        empLeaveData.Add(dt.Rows[i]["EmployeeId"].ToString(), LeaveData);
                    }

                    LeaveData.Add(dt.Rows[i]["LeaveTypeId"].ToString(), dt.Rows[i]);

                    employeeId = dt.Rows[i]["EmployeeId"].ToString();
                }

                return empLeaveData;
            }catch(Exception ex)
            {
                //var x = dhruv;
                throw ex;
            }
        }

        public Dictionary<string, Dictionary<string, DataRow>> GetLeaveBalance(DateTime UpToDate, string EmployeeIds)
        {
            int YearNo = Convert.ToDateTime(UpToDate).Year;
            int MonthNo = Convert.ToDateTime(UpToDate).Month;

            
            return GetLeaveBalance(MonthNo, YearNo, EmployeeIds, UpToDate.ToString("dd-MMM-yyyy")); 
        }
        public Dictionary<string, Dictionary<string, DataRow>> GetLeaveBalanceOnlyEarned(int MonthNo, int YearNo, string EmployeeIds)
        {

            DataTable dt = _sqlRepository.GetDataTable("SELECT * FROM LeaveType AS lt WHERE lt.Code IN ('CL','PL')");
            string leaveTypeIds = "''";
            foreach (DataRow item in dt.Rows)
                leaveTypeIds += ",'" + item["Id"].ToString() + @"'";

            
            return GetLeaveBalance(MonthNo, YearNo, EmployeeIds, "", leaveTypeIds); 
        }

    }
}


public class EmployeeDetails
{
    public string PlantName { get; set; }
    public string EmployeeName { get; set; }
    public string DOJ { get; set; }
    public string EmployeeCode { get; set; }
    public string Designation { get; set; }
    public string Department { get; set; }
    public string EmployeeCategory { get; set; }
    public string LeaveName { get; set; }
    public decimal CurrentAllocation { get; set; }
    public decimal BroughtForward { get; set; }
    public decimal LeaveDays { get; set; }
    public decimal Applied { get; set; }
    public decimal Availed { get; set; }
    //public decimal BroughtForward { get; set; }
    public decimal Balance { get; set; }
    public decimal EncashedInbetween { get; set; }
}
public class EmployeeDetailsVM
{
    public string SystemID { get; set; }
    public string PlantName { get; set; }
    public string EmployeeName { get; set; }
    public string DOJ { get; set; }
    public string EmployeeCode { get; set; }
    public string Designation { get; set; }
    public string Department { get; set; }
    public string EmployeeCategory { get; set; }
    public string LeaveName { get; set; }
    public string LeaveType { get; set; }
    public decimal CurrentAllocation { get; set; }
    public decimal CurrentAllocationDCBS { get; set; }
    public decimal BroughtForward { get; set; }
    public decimal LeaveDays { get; set; }
    public decimal DaysCanBeSanctioned { get; set; }
    public decimal Applied { get; set; }
    public decimal Availed { get; set; }
    //public decimal BroughtForward { get; set; }
    public decimal Balance { get; set; }
    public decimal EncashedInbetween { get; set; }
    public bool IsProratacurrentyear { get; set; }
    public bool IsAvailExceptionAllowedOnSpecialAppeal { get; set; }
    public bool IsBroughtForwardAdd { get; set; }
}