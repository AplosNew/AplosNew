using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System.IO;
using System.Drawing;
using System.Collections.Specialized;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class CompanyWiseBankSheetController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public CompanyWiseBankSheetController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public JsonResult GetData(string month, string year, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string strSqlStruc = "";
            DataSet dsRef;
            DataTable dtslProcId = null;
            ConnectionManager.DAL.ConManager objCon;
            strSqlStruc = @"SELECT SystemID FROM SalaryProcMaster SPM
                                      WHERE SPM.SystemID IN (SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        ) AND SPM.MonthNo = '" + month + @"' AND SPM.YearNo='" + year + @"' ";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(strSqlStruc, out dsRef, false, "1");
            dtslProcId = dsRef.Tables[0];
            string inSalaryProcParam = "' '";
            for (int i = 0; i < dtslProcId.Rows.Count; i++)
            {
                inSalaryProcParam += ",'" + dtslProcId.Rows[i]["SystemID"].ToString() + "'";
            }

            string empStatus = " and (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                empStatus = " and (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    empStatus += " OR case when  ISNULL(SalaryProcFlag,'Regular') ='' then 'Regular' else ISNULL(SalaryProcFlag,'Regular') end = 'Regular' ";
                }
                if (isSeperated == true)
                {
                    empStatus += " OR ISNULL(SalaryProcFlag,'Regular') ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    empStatus += " OR ISNULL(SalaryProcFlag,'Regular') ='MLV_PRE'";

                }
            }
            empStatus += ")";

            var str = @"SELECT distinct sp.PaymentMode,isnull(b.Id,'') BankId,isnull(b.UserName,'') BankName
								,p.Id PlantId, p.UserName PlantName
								,En.Id EntityId,En.UserName Entity
								,Dp.Id DepartmentId,DP.UserName DepartmentName
                                ,LGD.Id DesignationId,LGD.UserName DesignationName
								,SE.Id SectionId, SE.UserName SectionName
								,SuS.Id SubSectionId, SuS.UserName SubSectionName
								,Ec.Id EmployeeCategoryId,EC.UserName EmployeeCategory
								
								FROM SalaryProcessLogDetail sp
								Left join SalaryProcMaster spm on spm.SystemID=sp.SalaryProcessId
								Left join HKP.Bank b on b.Id= sp.BankSystemID and b.Id = sp.BankSystemID
                                INNER JOIN EmployeeInformation AS EI ON EI.SystemId = sp.EmpSystemID
								LEFT JOIN MST.ManpowerBudget PMB ON sp.BudgetCode = PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                        LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id
                        LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = sp.LegalDesignationId
                        left join HKP.Designation DeG on DeG.Id=sp.DesignationId
                        left join HKP.EmployeeCategory EC on EC.Id=sp.EmployeeCategoryId
                        left join ORG.Section SE on SE.Id=PR.SectionId
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
						left join ORG.Plant p on p.Id=ei.PlantId
						where p.CompanyId='C20201' and SPM.SystemID IN  (" + inSalaryProcParam + ") " + empStatus + @" ";
            var jsondata = Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #region --- Report---
        [HttpPost, Authorize]
        public JsonResult CwReport(string PaymentModeList, string BankList, string PlantList, string EntityList, string DepartmentList, string DesignationList, string SectionList, string SubSectionList, string Month, string Year, bool isActive, bool isSeperated, bool isMaternity, string MonthName)
        {
            try
            {
                string PaymentListSeperated = PaymentModeList;
                //var PaymentList = PaymentModeList.Split(',');
                //string PaymentListSeperated = "";
                //foreach (var item in PaymentList)
                //{
                //    PaymentListSeperated += "*" + item + "*,";
                //}
                //PaymentListSeperated = PaymentListSeperated.Replace('*', '"');
                string fileName = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                fileName = GetReport(PaymentListSeperated, BankList, PlantList, EntityList, DepartmentList, DesignationList, SectionList, SubSectionList, Month, Year, isActive, isSeperated, isMaternity, MonthName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetReport(string PaymentModeList, string BankList, string PlantList, string EntityList, string DepartmentList, string DesignationList, string SectionList, string SubSectionList, string Month, string Year, bool isActive, bool isSeperated, bool isMaternity, string MonthName)
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

                StringCollection PaymentNewMoodList = new StringCollection();

                //var PaymentListSeperated = PaymentModeList.Split(',');
                //foreach (var item in PaymentListSeperated)
                //{
                //    if (item != "")
                //    {
                //        String[] Pay = new String[] { item };
                //        PaymentNewMoodList.AddRange(Pay);

                //    }
                //}




                #region DataSet

                GetReportData(PaymentModeList, BankList, PlantList, EntityList, DepartmentList, DesignationList, SectionList, SubSectionList, Month, Year, isActive, isSeperated, isMaternity, out dslocal);

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
                    sheet1.Range[xlsRow, xlsCol].Text = "Plant";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Employee Code";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Employee Name";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "Account No.";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "IFSC Code";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Bank Name";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Net Salary";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Employee Type";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;



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

                        xlsRow += intRow;
                        intRow = 1;
                        #region ----------------------Data-----------------------

                        strCount += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = strCount;
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["PlantName"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
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

                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["BankAccountNo"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["IFSCCode"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["BankName"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = Convert.ToDouble(dvAttn[i]["NetSalary"].ToString());
                        sheet1.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeType"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;

                        #region Line Setup
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].RowHeight = 30;
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
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), identity.CompanyId + ".jpg");  // IDCardEng.xlsx
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
                    sheet1.Range[xlsRow, 3].Text = "CompanyWiseBankSheet";
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "CompanyWiseBankSheet Report, Month:- " + MonthName + " Year:-" + Year;
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

                    sheet1.Name = "CompanyWiseBankSheet";
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

        public void GetReportData(string PaymentModeList, string BankList, string PlantList, string EntityList, string DepartmentList, string DesignationList, string SectionList, string SubSectionList, string Month, string Year, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string secSQL = string.Empty;
            string xxy = string.Empty;
            string Check = string.Empty;
            clsStaticInfo obs = null;
            string ShiftIds_WC = "";
            string XJobLocation = string.Empty;
            try
            {
                string strSqlStruc = "";
                DataSet dsRefs;
                DataTable dtslProcId = null;
                strSqlStruc = @"SELECT SystemID FROM SalaryProcMaster SPM
                                      WHERE SPM.SystemID IN (SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        ) AND SPM.MonthNo = '" + Month + @"' AND SPM.YearNo='" + Year + @"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSqlStruc, out dsRefs, false, "1");
                dtslProcId = dsRefs.Tables[0];
                string inSalaryProcParam = "' '";
                for (int i = 0; i < dtslProcId.Rows.Count; i++)
                {
                    inSalaryProcParam += ",'" + dtslProcId.Rows[i]["SystemID"].ToString() + "'";
                }

                string empStatus = " and (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    empStatus = " and (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        empStatus += " OR case when  ISNULL(SalaryProcFlag,'Regular') ='' then 'Regular' else ISNULL(SalaryProcFlag,'Regular') end = 'Regular' ";
                    }
                    if (isSeperated == true)
                    {
                        empStatus += " OR ISNULL(SalaryProcFlag,'Regular') ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        empStatus += " OR ISNULL(SalaryProcFlag,'Regular') ='MLV_PRE'";

                    }
                }
                empStatus += ")";
                if (PaymentModeList == "'Cash'")
                {
                    Check = @" --and bank.Id in (" + BankList + @")";
                }
                else
                {
                    Check = @"and bank.Id in (" + BankList + @")";
                }

                strSql = @" select p.UserName PlantName,EI.EmployeeCode,EI.EmployeeName
						,NetSalary = case when spcc.PaymentMode='Bank' then (ISNULL(spcc.SalaryPercentage,0)*ISNULL(spc.DisbusmentAmount,0))/100 else ISNULL(SPC.DisbusmentAmount,0) end
						,spcc.BankAccNo BankAccountNo,spcc.IFSCCode,spcc.MICRCode,spcc.SalaryPercentage,Bank.UserName BankName, ec.UserName as EmployeeType
						From SalaryProcChild SPC
                        INNER JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID 
						left join SalaryProcessLogDetail spcc on spcc.SalaryProcessId = SPC.SlrProcMstSystemID and spcc.EmpSystemId=SPC.EmpInfoSystemID
						INNER JOIN SalaryHead SH ON SH.SalaryHeadID = SPC.SalaryHeadID 
						INNER JOIN EmployeeInformation EI ON EI.SystemId = SPC.EmpInfoSystemID 
						LEFT JOIN EmployeeBankInfo EBI ON EI.SystemId = EBI.EmpSystemID
						LEFT JOIN HKP.Bank Bank ON Bank.Id = EBI.BankSystemID
						LEFT JOIN MST.ManpowerBudget PMB ON spcc.BudgetCode = PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                        LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id
                        LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = spcc.LegalDesignationId
                        left join HKP.Designation DeG on DeG.Id=spcc.DesignationId
                        left join HKP.EmployeeCategory EC on EC.Id=spcc.EmployeeCategoryId
                        left join ORG.Section SE on SE.Id=PR.SectionId
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
						left join ORG.Plant p on p.Id=ei.PlantId
						where SPM.SystemID IN  (" + inSalaryProcParam + @") " + empStatus + @"
                        and spcc.PaymentMode in (" + PaymentModeList + @")   "+ Check +@" 
						and SH.HeadCategory = 'Net Payable'
						and ei.SystemId in (
						
						select e.SystemId
                                            from EmployeeInformation e
                                            left join mst.ManpowerBudget mp on mp.id=e.BudgetCode
											left join org.Entity en on en.id=mp.EntityId    
											left join ORG.Position p on p.Id = mp.PositionId
											left join org.Department dep on dep.Id = p.DepartmentId
											left join org.Section s on s.Id = p.SectionId
											left join org.SubSection ss on ss.Id = p.SubSectionId  
                                            LEFT JOIN hkp.LegalDesignation LG ON e.LegalDesignationId = LG.Id 
											left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id
											left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId
											left join HKP.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId
											where e.plantid in (" + PlantList + @")
											and en.Id in (" + EntityList + @") 
											and dep.Id in (" + DepartmentList + @") 
						and LG.Id in (" + DesignationList + @") 
						and s.Id in (" + SectionList + @") 
						and ss.id in (" + SubSectionList + @")
						)";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSql, out dsRef);
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