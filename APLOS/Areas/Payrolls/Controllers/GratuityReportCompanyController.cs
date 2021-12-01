using Aplos.Controllers;
using ConnectionManager;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;
using static Library.Service.HumanResources.PayRegisterBDReportService;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class GratuityReportCompanyController : BaseController
    {
        #region Constructor

        private readonly IPayRegisterBDReportService _payRegisterBDReportService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;



        public GratuityReportCompanyController(
              IPayRegisterBDReportService payRegisterBDReportService, IEmployeeProfileService employeeProfileService,
              ISqlRepository sqlRepository
            )
        {
            _payRegisterBDReportService = payRegisterBDReportService;
            _employeeProfileService = employeeProfileService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations
        [HttpPost, Authorize]
        public ActionResult XlsEmployeeGratuity(string calculationDate, string payrollGroup, string employeeSystemId, string reportType)
        {
            string fileName = "";
            #region Variable
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsReport objRpt = null;
            int slCount = 0;

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            DataSet dsEmpGratuity = null;
            DataTable dtEmpGratuity = null;


            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            #endregion Variable

            try
            {
                ru = new ReportUtility();

                objRpt = new clsReport();

                #region Variable
                ParamList para = new ParamList();
                ParamList leavePara = new ParamList();
                ParamList attdnProcessParam = new ParamList();

                var FactoryName = "";
                var CmpName = "";

                para.PlantId = identity.PlantId;
                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), identity.CompanyId + ".jpg");  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion

                // para.EmpStatus = ddlStatus.SelectedValue.Trim();          
                #endregion Variable
                var oRU = new ReportUtility();

                var colSr = 0;
                var colEmpCode = 0;
                var colEmpName = 0;
                var colDOB = 0;
                var colDOJ = 0;
                var colSalEliGratuity = 0;
                var colEmpFatherName = 0;
                var colBasic = 0;
                var colMonth = 0;
                var colYear = 0;
                var colDays = 0;

                #region DataSet
                DataTable dtGratuityPolicy = new DataTable();
                string salaryHeadIDString = "";
                DataSet dsSlrProc = null;
                SalaryHeadGratuity(identity.CompanyId, out salaryHeadIDString, out dtGratuityPolicy);

                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmpSalaryInformationRpt(identity.CompanyId, calculationDate, payrollGroup, salaryHeadIDString, out dsSlrProc);
                Dictionary<string, DataRow> dicGRTpolicy = GetGratuityPolicy(identity.CompanyId);
                var _systemAdmin = identity.IsSysAdmin.ToString().Trim();
                var _controlAdmin = identity.IsControlAdmin.ToString().Trim();
                GetCompanyEmpGratuityInfo(calculationDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, payrollGroup, employeeSystemId, _systemAdmin, _controlAdmin, out dsEmpGratuity);
                double eligibleYear = 0.00;
                dtEmpGratuity = dsEmpGratuity.Tables[0];
                dtEmpGratuity.Columns.Add("EligibleYear", typeof(double));
                if (dtGratuityPolicy.Rows.Count == 0)
                {
                    throw new Exception("No Policy Found");
                }
                if (dtEmpGratuity.Rows.Count == 0)
                {
                    throw new Exception("No Data Found");
                }
                for (int i = 0; i < dtEmpGratuity.Rows.Count; i++)
                {
                    try
                    {
                        if (dtEmpGratuity.Rows[i]["EmployeeCode"].ToString() == "10006354")
                        {

                        }
                        double totalYear = 0.00;
                        //if (dtEmpGratuity.Rows[i]["EmployeeCode"].ToString() == "1088")
                        //{

                        //}//totalMonthAfterYear
                        if (Convert.ToInt32(dtEmpGratuity.Rows[i]["totalMonthAfterYear"]) >= 6)
                        {
                            totalYear += 0.5;
                        }
                        totalYear += Convert.ToDouble(dtEmpGratuity.Rows[i]["totalYear"].ToString());

                        //if(dtEmpGratuity.Rows[i]["totalYear"].ToString())

                        dtGratuityPolicy.DefaultView.RowFilter = totalYear + ">=MaturityFromYear	AND " + totalYear + "<=MaturityToYear and plantId = '"+ dtEmpGratuity.Rows[i]["PlantID"].ToString() + "' ";

                        //dtGratuityPolicy.DefaultView.RowFilter =  "MaturityFromYear>=" + dtEmpGratuity.Rows[i]["totalYear"].ToString() + "	AND MaturityToYear <=" + dtEmpGratuity.Rows[i]["totalYear"].ToString();

                        //dtGratuityPolicy.DefaultView.RowFilter = eligibleYear + ">=MaturityFromYear	AND " + eligibleYear + "<=MaturityToYear";
                        if (dtGratuityPolicy.DefaultView.Count > 0)
                        {
                            DataRow Rowformula = dtGratuityPolicy.DefaultView[0].Row;
                            dtEmpGratuity.Rows[i]["EligibleYear"] = gratuityYear(Convert.ToDateTime(dtEmpGratuity.Rows[i]["DOJ"].ToString()), Convert.ToDateTime(calculationDate), Convert.ToInt32(dtEmpGratuity.Rows[i]["totalYear"]), Convert.ToInt32(dtEmpGratuity.Rows[i]["totalMonthAfterYear"]), Convert.ToBoolean(Rowformula["IsRoudingSixMonth"]));

                        }


                    }
                    catch (Exception ex)
                    {


                    }

                }
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                xlsRow = 6;
                xlsCol = 1;
                #region------------------Column Header------------------

                SetHeaderValue("S.No.", sheet1, xlsRow, ref xlsCol, out colSr, 6);
                SetHeaderValue("EmpCode", sheet1, xlsRow, ref xlsCol, out colEmpCode, 9);
                SetHeaderValue("Name", sheet1, xlsRow, ref xlsCol, out colEmpName, 25);
                SetHeaderValue("Father Name", sheet1, xlsRow, ref xlsCol, out colEmpFatherName, 25);
                SetHeaderValue("Plant", sheet1, xlsRow, ref xlsCol, out var colPlantName, 25);
                SetHeaderValue("Gratuity No", sheet1, xlsRow, ref xlsCol, out int colGratuityNo, 25);
                SetHeaderValue("Policy No", sheet1, xlsRow, ref xlsCol, out int colPolicyNo, 25);

                SetHeaderValue("DOB", sheet1, xlsRow, ref xlsCol, out colDOB, 12);
                SetHeaderValue("DOJ", sheet1, xlsRow, ref xlsCol, out colDOJ, 12);
                SetHeaderValue("Year", sheet1, xlsRow, ref xlsCol, out colYear, 6);
                SetHeaderValue("Month", sheet1, xlsRow, ref xlsCol, out colMonth, 6);
                SetHeaderValue("Days", sheet1, xlsRow, ref xlsCol, out colDays, 5);
                SetHeaderValue("Salary Eligible for Gratuity", sheet1, xlsRow, ref xlsCol, out colBasic, 13);
                SetHeaderValue("Gratuity Amount", sheet1, xlsRow, ref xlsCol, out colSalEliGratuity, 15);
                endXlsCol = colSalEliGratuity;

                sheet1.Range[xlsRow, colSr, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                #endregion------------------Column Header------------------
                var fPanRow = xlsRow + 1;//Freeze pan starting rows

                #region Data to Excel Column
                string strReplace = "";
                xlsRow++;
                DataRow drGratuityPolicy = null;
                for (int i = 0; i < dtEmpGratuity.Rows.Count; i++)
                {

                    try
                    {

                        if (dtEmpGratuity.Rows[i]["EmployeeCode"].ToString() == "10006354")
                        {

                        }

                        var eligibleGratuityAmount = 0.00;
                        eligibleYear = clsStaticInfo.dbl(dtEmpGratuity.Rows[i]["EligibleYear"].ToString());
                        DateTime doj = Convert.ToDateTime(dtEmpGratuity.Rows[i]["DOJ"].ToString());
                        if (dtGratuityPolicy.Rows.Count > 0)
                        {
                            try
                            {
                                strReplace = "";

                                double totalYear = 0.00;



                                if (Convert.ToInt32(dtEmpGratuity.Rows[i]["totalMonthAfterYear"]) >= 6)
                                {
                                    totalYear += 0.5;
                                }
                                totalYear += Convert.ToDouble(dtEmpGratuity.Rows[i]["totalYear"].ToString());


                                dtGratuityPolicy.DefaultView.RowFilter = totalYear + ">=MaturityFromYear AND " + totalYear + "<=MaturityToYear and plantId = '" + dtEmpGratuity.Rows[i]["PlantID"].ToString() + "'";

                                if (dtGratuityPolicy.DefaultView.Count > 0)
                                {

                                    DataRow Rowformula = dtGratuityPolicy.DefaultView[0].Row;
                                    strReplace = Rowformula["MaturityFormulaDesID"].ToString();
                                    List<DataRow> drSalary = dicEmpSalry[dtEmpGratuity.Rows[i]["EmpSystemID"].ToString()];


                                    //eligibleYear =  gratuityYear(Convert.ToDateTime(dtEmpGratuity.Rows[i]["DOJ"].ToString()), Convert.ToDateTime(calculationDate), Convert.ToInt32(dtEmpGratuity.Rows[i]["totalYear"]), Convert.ToInt32(dtEmpGratuity.Rows[i]["totalMonthAfterYear"]), Convert.ToBoolean(Rowformula["IsRoudingSixMonth"]));

                                    for (int ic = 0; ic < drSalary.Count; ic++)
                                    {
                                        strReplace = strReplace.Replace(drSalary[ic]["SalaryHeadID"].ToString().ToUpper(), drSalary[ic]["EntryAmount"].ToString());

                                    }
                                    //eligibleGratuityAmount 
                                    object value = null;
                                    try
                                    {

                                        DataTable dt = new DataTable();
                                        value = dt.Compute(strReplace, "");
                                        eligibleGratuityAmount = Convert.ToInt32(value);

                                        eligibleGratuityAmount = eligibleGratuityAmount * Convert.ToDouble(eligibleYear);

                                    }
                                    catch (Exception ex)
                                    {


                                    }
                                    finally
                                    {
                                        //oRU.SetText(ref sheet1, xlsRow, E_BRATE, Convert.ToInt32(value));

                                    }
                                }
                                else
                                {
                                    continue;
                                }



                            }
                            catch (Exception ex)
                            {


                            }
                        }

                        slCount++;
                        ru.SetSLText(ref sheet1, xlsRow, colSr, slCount);
                        ru.SetText(ref sheet1, xlsRow, colEmpCode, dtEmpGratuity.Rows[i]["EmployeeCode"].ToString());
                        ru.SetText(ref sheet1, xlsRow, colEmpName, dtEmpGratuity.Rows[i]["EmployeeName"].ToString());
                        ru.SetText(ref sheet1, xlsRow, colEmpFatherName, dtEmpGratuity.Rows[i]["FatherName"].ToString()); //colInsuranceNo = xlsCol; xlsCol++;
                        ru.SetText(ref sheet1, xlsRow, colPlantName, dtEmpGratuity.Rows[i]["PlantName"].ToString()); //colInsuranceNo = xlsCol; xlsCol++;
                        ru.SetText(ref sheet1, xlsRow, colDOB, dtEmpGratuity.Rows[i]["DOB"].ToString());//dtEmpInfo.Tables[0].Rows[i][""].ToString()
                        ru.SetText(ref sheet1, xlsRow, colDOJ, dtEmpGratuity.Rows[i]["DOJ"].ToString());

                        if (dicGRTpolicy.ContainsKey(dtEmpGratuity.Rows[i]["EmpSystemID"].ToString()))
                        {
                            drGratuityPolicy = dicGRTpolicy[dtEmpGratuity.Rows[i]["EmpSystemID"].ToString()];

                            ru.SetText(ref sheet1, xlsRow, colGratuityNo, drGratuityPolicy["GratuityNo"].ToString());
                            ru.SetText(ref sheet1, xlsRow, colPolicyNo, drGratuityPolicy["PolicyNo"].ToString());
                        }

                        var year = @"""y""";
                        var month = @"""ym""";
                        var days = @"""md""";

                        var calcDate = $@"""{calculationDate}""";
                        var yearFormula = "= DATEDIF(" + ru.GetColumnNameForXls(colDOJ) + xlsRow + "," + calcDate + "," + year + ")";
                        var monthFormula = "= DATEDIF(" + ru.GetColumnNameForXls(colDOJ) + xlsRow + "," + calcDate + "," + month + ")";
                        var dayFormula = "= DATEDIF(" + ru.GetColumnNameForXls(colDOJ) + xlsRow + "," + calcDate + "," + days + ")+1";

                        if (reportType == "EXCEL") //For XL File
                        {
                            ru.SetColFormula(ref sheet1, xlsRow, colYear, yearFormula, false);
                            ru.SetColFormula(ref sheet1, xlsRow, colMonth, monthFormula, false);
                            ru.SetColFormula(ref sheet1, xlsRow, colDays, dayFormula, false);

                        }
                        if (reportType == "PDF") //For PDF File
                        {
                            DateTime Now = DateTime.Now;
                            DateTime DOJ = Convert.ToDateTime(dtEmpGratuity.Rows[i]["DOJ"].ToString());

                            int Years = new DateTime(DateTime.Now.Subtract(DOJ).Ticks).Year - 1;

                            ru.SetSLText(ref sheet1, xlsRow, colYear, Years);
                            DateTime PastYearDate = DOJ.AddYears(Years);
                            int Months = 0;
                            for (int D = 1; D <= 12; D++)
                            {
                                if (PastYearDate.AddMonths(D) == calculationDate.ToDate())
                                {
                                    Months = D;
                                    break;
                                }
                                else if (PastYearDate.AddMonths(D) >= calculationDate.ToDate())
                                {
                                    Months = D - 1;
                                    break;
                                }
                            }
                            ru.SetSLText(ref sheet1, xlsRow, colMonth, Months);
                            int Days = calculationDate.ToDate().Subtract(PastYearDate.AddMonths(Months)).Days + 1;
                            ru.SetSLText(ref sheet1, xlsRow, colDays, Days);

                        }


                        //ru.SetNumberText(ref sheet1, xlsRow, colBasic, Convert.ToDouble(dtEmpGratuity.Rows[i]["EntryAmount"]).ToString("#,##0.00"));
                        sheet1.Range[xlsRow, colBasic].Number = Convert.ToDouble(dtEmpGratuity.Rows[i]["EntryAmount"]);// + Environment.NewLine + totalPayDay;
                        sheet1.Range[xlsRow, colBasic].NumberFormat = GetDecimalFormat(Convert.ToBoolean(dtEmpGratuity.Rows[i]["IntegerInDisb"]), Convert.ToInt32(dtEmpGratuity.Rows[i]["DecimalNo"]));
                        sheet1.Range[xlsRow, colBasic].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colBasic].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, colBasic].BorderAround(ExcelLineStyle.Hair);

                        ru.SetNumberText(ref sheet1, xlsRow, colSalEliGratuity, Convert.ToDouble(eligibleGratuityAmount).ToString("#,##0.00;"));

                        xlsRow++;
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }
                #endregion

                #region ******************Report Header******************
                sheet1.Range[fPanRow, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[fPanRow, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);


                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                xlsRow = 1;
                xlsCol = 1;

                try
                {

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
                catch (Exception ex)
                {
                }
                FactoryName = string.Empty;
                var FactoryAddress = string.Empty;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                //xlsRow += 1;
                //if (dsCmp.Tables[0].Rows.Count > 0)
                //{
                //    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                //}
                //else
                //{
                //    FactoryName = "";
                //}
                //sheet1.Range[xlsRow, 3].Text = FactoryName;
                //sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 13;
                //sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 13;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Gratuity Satement As Of :" + calculationDate;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                //sheet1.Range[xlsRow, 1].Text = "Report Ref No:";
                //sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 10;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                //sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes
                sheet1.UsedRange["A" + fPanRow].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 5;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$6";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "GratuityStatement";
                #endregion

                workbook.Version = ExcelVersion.Excel2016;
                
                if (reportType.ToUpper() == "EXCEL")
                {
                    fileName = "GratuityStatement.xls";

                    string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                    workbook.Version = ExcelVersion.Excel2016;
                    workbook.SaveAs(fullPath);

                }
                if (reportType.ToUpper() == "PDF")
                {
                    var converter = new ExcelToPdfConverter(workbook);
                    var pdfDoc = converter.Convert();
                    fileName = "GratuityStatement.pdf";
                    string fullPathPDF = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                    pdfDoc.Save(fullPathPDF);
                }
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
            finally
            {
                objRpt = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }
        }//End Function
        public void GetCompanyEmpGratuityInfo(string gratuityCalcDate, string companyGroupId, string comapnyId, string plantId, string payGrp, string employeeId, string SystemAdmin, string ControlAdmin, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            var strSql = string.Empty;
            clsStaticInfo obs = null;
            try
            {
                obs = new clsStaticInfo();
                strSql = @"SELECT *
                                    FROM (
                                    	SELECT Convert(DECIMAL(18, 2), CAST(DATEDIFF(mm, A.DOJ, '" + gratuityCalcDate + @"') AS VARCHAR(4))) / 12 CompareDate
                                    		,A.EmployeeCode EmployeeCodeS
                                    		,Convert(DECIMAL(18, 2), CAST(DATEDIFF(mm, A.DOJ, '" + gratuityCalcDate + @"') AS VARCHAR(4))) / 12 totalYear
                                    		,Convert(DECIMAL(18, 2), CAST(DATEDIFF(mm, A.DOJ, '" + gratuityCalcDate + @"') AS VARCHAR(4))) - (Convert(DECIMAL(18, 2), CAST(DATEDIFF(mm, A.DOJ, '" + gratuityCalcDate + @"') AS VARCHAR(4))) / 12) * 12 totalMonthAfterYear
                                    		,*
                                    	FROM (
                                    		SELECT E.SystemID EmpSystemID
                                    			,E.EmployeeCode
                                    			,E.EmployeeName
                                    			,REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB
                                    			,E.FatherName
                                    			,E.MotherName
                                    			,E.EmpType EmployeeType
                                    			,E.EmploymentType EmploymentNature
                                    			,E.NationalID
                                    			,E.GenderID GenderName
                                    			,REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ
                                    			,REPLACE(Convert(VARCHAR(11), E.DOC, 106), ' ', '-') AS DOC
                                    			,eact.IsOutSider
                                    			,EC.UserName AS EmpCategory
                                    			,Cm.UserName CompanyName
                                    			,Cm.Id CompanyId
                                    			,CAM.Address1
                                    			,CAM.Address2
                                    			,E.EmployeeCategorySystemID
                                    			,E.UnitID
                                    			,E.DivisionID
                                    			,E.DepartmentID
                                    			,E.DesignationSystemID
                                    			,E.SectionID
                                    			,E.SubSectionID
                                    			,E.LineID
                                    			,E.DesignationGroupID
                                    			,E.SubSecStrucSystemID
                                    			,E.EmployeeStatus
                                    			,P.UserName PlantName
                                    			,(PAM.[Address1] + ', ' + PAM.[Address2] + ', ' + PAMC.UserName + ' - ' + PAM.Postcode) FactoryAddress
                                    			,GC.Id GroupID
                                    			,GC.UserName GroupName
                                    			,(CGAM.[Address1] + ', ' + CGAM.[Address2] + ', ' + CT.UserName + ' - ' + CGAM.Postcode + ', Contact: ' + CGAM.Phone) GroupAddress
                                    			,E.PlantID
                                    			,BK.UserName BankNameShort
                                    			,E.BankAccNo
                                    			,EmpSlr.SalaryHeadID
                                    			,SH.SalaryHead
                                    			,--SNULL(pSH.Sequence, 99) Sequence,
                                    			SH.HeadType
                                    			,SH.HeadCategory
                                    			,EmpSlr.EntryCurrencyID
                                    			,EmpSlr.EntryAmount
                                    			,EmpSlr.DefineCurrencyID
                                    			,EmpSlr.DefineAmount
                                    			,EmpSlr.AmtDefinationCurrencyID
                                    			,EmpSlr.AmtDefinationRate
                                    			,SH.IsCTCComponent
                                    			,SH.IsGrossComponent
                                    			,EmpSlr.EmpInfoSystemID
                                    			,MW.SalaryHeadValue
                                    			,ISNULL(CRC.IntegerInDisb, 1) IntegerInDisb
                                    			,ISNULL(CRC.DecimalNo, 0) DecimalNo
                                    			,MW.Grade
                                    			,gpd.MaturityFromYear
                                    			,gpd.MaturityToYear
                                    		FROM EmployeeInformation AS E
                                    		LEFT JOIN GratuityPolicyDetails AS gpd ON gpd.plantId = e.PlantId
                                    		LEFT JOIN ORG.Plant AS p ON E.PlantId = p.Id
                                    		LEFT JOIN ORG.Company AS Cm ON E.CompanyID = Cm.Id
                                    		LEFT JOIN ORG.CompanyGroup AS GC ON E.GroupID = GC.Id
                                    		LEFT JOIN HKP.Bank AS BK ON E.BankSystemID = BK.Id
                                    		LEFT JOIN MST.AddressMaster AS CAM ON Cm.AddressMasterId = CAM.Id
                                    		LEFT JOIN MST.AddressMaster AS PAM ON P.AddressMasterId = PAM.Id
                                    		LEFT JOIN MST.AddressMaster AS CGAM ON GC.AddressMasterId = CGAM.Id
                                    		LEFT JOIN SCS.City AS PAMC ON PAM.CityId = PAMC.Id
                                    		LEFT JOIN SCS.City AS CT ON CGAM.CityId = CT.Id
                                    		LEFT JOIN (
                                    			SELECT ECT.Id
                                    				,ECT.UserName
                                    				,DM.DesignationId
                                    			FROM [HKP].[EmployeeCategory] ECT
                                    			LEFT JOIN MST.DesignationMaster DM ON ECT.Id = DM.EmployeeCategoryId
                                    			) EC ON EC.DesignationId = E.GivenDesignationId
                                    		LEFT JOIN (
                                    			SELECT E.SystemID
                                    				,SUM(SV.SalaryHeadValue) SalaryHeadValue
                                    				,LSG.UserName Grade
                                    			FROM EmployeeInformation E
                                    			LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
                                    			LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId
                                    				AND E.PlantId = gd.PlantId
                                    			LEFT JOIN (
                                    				SELECT MAX(EffectiveDate) EffectiveDate
                                    					,LegalSalaryGradeId
                                    					,EmployeeLocationId
                                    				FROM MST.LegalSalaryStructure
                                    				WHERE EffectiveDate <= '" + gratuityCalcDate + @"'
                                    				GROUP BY LegalSalaryGradeId
                                    					,EmployeeLocationId
                                    				) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId
                                    				AND S.EmployeeLocationId = B.EmployeeLocationId
                                    			LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId
                                    				AND SS.EmployeeLocationId = S.EmployeeLocationId
                                    				AND SS.EffectiveDate = S.EffectiveDate
                                    			LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id
                                    			LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSG.Id = S.LegalSalaryGradeId
                                    			GROUP BY E.SystemId
                                    				,LSG.UserName
                                    			) MW ON MW.SystemId = E.SystemId
                                    		LEFT JOIN (
                                    			SELECT *
                                    			FROM (
                                    				SELECT MST.EmpInfoSystemID
                                    					,EmpSlr.SalaryHeadID
                                    					,EmpSlr.EntryCurrencyID
                                    					,EmpSlr.EntryAmount
                                    					,EmpSlr.DefineCurrencyID
                                    					,EmpSlr.DefineAmount
                                    					,EmpSlr.AmtDefinitionCurrencyID AmtDefinationCurrencyID
                                    					,EmpSlr.AmtDefinitionRate AmtDefinationRate
                                    					,MST.SalaryRuleMasterSystemID
                                    				FROM SalaryInfoDefine EmpSlr
                                    				INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID
                                    					AND MST.IsApproved = 1
                                    				) A
                                    			
                                    			UNION
                                    			
                                    			(
                                    				SELECT M.EmpSystemID EmpInfoSystemID
                                    					,D.SalaryHeadID
                                    					,CRC.AmtEntryCurrency EntryCurrencyID
                                    					,D.Value EntryAmount
                                    					,CRC.AmtDefinitionCurrency DefineCurrencyID
                                    					,D.Value DefineAmount
                                    					,CRC.AmtDefinitionCurrency AmtDefinationCurrencyID
                                    					,1 AmtDefinationRate
                                    					,MST.SalaryRuleMasterSystemID
                                    				FROM [BonusPolicyMonthlyRetainStrcEmpWiseCalculation] M
                                    				INNER JOIN [BonusPolicyMonthlyRetainDistributionStrcPmt] D ON M.ID = D.BnsPlyMntRetainID
                                    				INNER JOIN SalaryInfoDefineMaster MST ON M.EmpSystemID = MST.EmpInfoSystemID
                                    					AND MST.IsApproved = 1
                                    				LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = MST.SalaryRuleMasterSystemID
                                    				LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = SRM.CurrencyRuleSystemID
                                    					AND CRC.SalaryHeadID = D.SalaryHeadID
                                    				WHERE M.MonthNo = DATEPART(MONTH, '" + gratuityCalcDate + @"')
                                    					AND M.YearNo = DATEPART(YEAR, '" + gratuityCalcDate + @"') 
                                    				)
                                    			) EmpSlr ON E.SystemID = EmpSlr.EmpInfoSystemID
                                    		LEFT JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
                                    		LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EmpSlr.SalaryRuleMasterSystemID
                                    		LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID
                                    			AND CRC.SalaryHeadID = SH.SalaryHeadID
                                    		LEFT JOIN [dbo].[EmployeeCodeType] eact ON eact.Id = e.EmployeeCodeTypeId
                                    		) A
                                    	WHERE EmployeeStatus = 'Active'
                                    		AND a.IsOutSider = 0
                                    		AND isnull(EmpInfoSystemID, '') <> ''
                                    		AND GroupID = '" + companyGroupId + @"'
                                    		AND CompanyId = '" + comapnyId + @"' 
                                    		AND HeadCategory = 'Basic'
                                    	) xx
                                    WHERE xx.CompareDate BETWEEN xx.MaturityFromYear
                                    		AND xx.MaturityToYear
                                    ";

                if (!string.IsNullOrEmpty(employeeId))
                {
                    strSql = strSql + @" AND EmpSystemID IN (" + employeeId + ")";
                }
                #region--Pay Group--

                if (payGrp.ToUpper() == "ALL".ToUpper())
                {

                    if (SystemAdmin.ToUpper() == "TRUE" || ControlAdmin.ToUpper() == "TRUE")
                    {
                        strSql = strSql + "";
                    }
                    else
                    {
                        strSql = strSql + @" AND EmpSystemID  =''";
                        throw new Exception("Please Select a Pay Group");

                    }

                }
                else if (payGrp.ToUpper().Trim() != "NOGROUP")
                {
                    strSql = strSql + @" AND EmpSystemID  IN (
													 select employeeid from MST.PayrollGroupMaster where PayrollGroupId = '" + payGrp + @"')";
                }
                if (payGrp.ToUpper().Trim() == "NOGROUP")
                {
                    strSql = strSql + @" AND EmpSystemID NOT IN (
													 select employeeid from MST.PayrollGroupMaster)";
                }
                #endregion--Pay Group--
                strSql = strSql + @"
                        ORDER BY EmployeeCodeS";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public Dictionary<string, List<DataRow>> GetEmpSalaryInformationRpt(string plantId, string effectiveDate, string payRollGroup, string salaryHeadId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();

            string strSql = string.Empty;
            clsStaticInfo obs = null;
            try
            {

                obs = new clsStaticInfo();
                //              strSql = @"SELECT * FROM
                //                        (
                //                         SELECT E.SystemID EmpSystemID,  E.EmployeeCode EmployeeCode, E.EmployeeName, REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB,
                //                             E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID,
                //                             E.GenderID GenderName, REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ,
                //                                REPLACE(Convert(VARCHAR(11), E.DOS, 106), ' ', '-') AS DOS,
                //                             REPLACE(Convert(VARCHAR(11), E.DOC, 106), ' ', '-') AS DOC,ISNULL(LG.UserName,'') LegalDesignation
                //						   , E.EmployeeStatus,
                //                             P.UserName PlantName, (PAM.[Address1] + ', ' + PAM.[Address2] + ', ' + PAMC.UserName + ' - ' + PAM.Postcode) FactoryAddress,
                //                             GC.UserName GroupName, (CGAM.[Address1] + ', ' + CGAM.[Address2] + ', ' + CT.UserName + ' - ' + CGAM.Postcode + ', Contact: ' + CGAM.Phone) GroupAddress,
                //                             E.PlantID, BK.UserName BankNameShort, E.BankAccNo, 
                //						  EmpSlr.SalaryHeadID, SH.SalaryHead --, ISNULL(PSH.Sequence, 99) Sequence
                //                              , SH.HeadType, ISNULL(SH.HeadCategory,'') HeadCategory, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount,
                //                             EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, EmpSlr.AmtDefinitionCurrencyID, EmpSlr.AmtDefinationRate
                //                             , EmpSlr.EmpInfoSystemID, MW.SalaryHeadValue                                
                //                           ,CRC.IntegerInDisb, CRC.DecimalNo, MW.Grade,CRC.IsDecimalInDisb IsDecimal
                //                              ,ISNULL(E.GenderID,'') Gender,ISNULL(LSalGr.Code,'') GradeCode,E.CompanyId


                //									,ISNULL(PG.UserName,'') PayRollGroup

                //                                  ,ISNULL(jl.JobLocation, '') JobLocation
                //							,ISNULL(e.PaymentMode,'') PaymentMode
                //							,ISNULL(bb.UserName,'') BankName
                //		            FROM (SELECT * FROM EmployeeInformation  WHERE (EmployeeStatus != 'Separated' or DOS is null or DOS >='" + effectiveDate + @"')) AS E

                //                                         LEFT JOIN[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode

                //                                          LEFT JOIN ORG.Line L ON MB.LineID = L.Id
                //									LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
                //										  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
                //							LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
                //                                  LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

                //							LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId

                //                                          LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
                //                                          LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LG.Id and E.PlantId = LSGD.PlantId
                //                                          LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = LSGD.LegalSalaryGradeId  and E.PlantId = LSalGr.PlantId

                //									LEFT JOIN ORG.Plant AS p ON E.PlantId = p.Id
                //									LEFT JOIN ORG.Company AS Cm ON E.CompanyID = Cm.Id
                //									LEFT JOIN ORG.CompanyGroup AS GC ON E.GroupID = GC.Id
                //									LEFT JOIN HKP.Bank AS BK ON E.BankSystemID = BK.Id
                //									LEFT JOIN MST.AddressMaster AS CAM ON Cm.AddressMasterId = CAM.Id
                //									LEFT JOIN MST.AddressMaster AS PAM ON P.AddressMasterId = PAM.Id
                //									LEFT JOIN MST.AddressMaster AS CGAM ON GC.AddressMasterId = CGAM.Id
                //									LEFT JOIN SCS.City AS PAMC ON PAM.CityId = PAMC.Id
                //									LEFT JOIN SCS.City AS CT ON CGAM.CityId = CT.Id
                //                                          LEFT JOIN mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
                //                                          LEFT JOIN mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                //                                          LEFT JOIN hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                //									LEFT JOIN 
                //											(
                //											 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
                //												FROM EmployeeInformation E   
                //														LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
                //														LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                //                                                                                              AND E.PlantId = gd.PlantId
                //														LEFT JOIN (
                //																	SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
                //																		FROM MST.LegalSalaryStructure 
                //																		WHERE EffectiveDate <= '" + effectiveDate + @"'
                //																	GROUP BY LegalSalaryGradeId, EmployeeLocationId 
                //																  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
                //														LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                //                                                                                          AND SS.EmployeeLocationId = S.EmployeeLocationId 
                //                                                                                          AND SS.EffectiveDate = S.EffectiveDate
                //														LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                //                                                              left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
                //												GROUP BY E.SystemId,LSG.UserName
                //											) MW ON MW.SystemId = E.SystemId


                //										INNER JOIN (
                //											SELECT * FROM
                //														(
                //														 --SELECT MST.EmpInfoSystemID, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
                //															--	EmpSlr.AmtDefinitionCurrencyID AmtDefinationCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID
                //														-- FROM SalaryInfoDefine EmpSlr
                //															--	INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID 
                //                                                                 Select SalaryDetails.* from  ( SELECT MAX(EffectiveDate) EffectiveDate,EmpInfoSystemID--,SalaryHead,SalaryHeadID,EntryCurrencyID
                //FROM (
                //          SELECT MST.EmpInfoSystemID,SH.SalaryHead, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
                //		EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID,MST.EffectiveDate
                //			FROM SalaryInfoDefine EmpSlr
                //			INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID AND MST.IsApproved = 1 
                //			left outer join SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID 
                //			--where EmpInfoSystemID = '1800118'
                //			UNION
                //			SELECT SBM.EmpInfoSystemID,SH.SalaryHead,SIB.SalaryHeadID,SIB.EntryCurrencyID,SIB.EntryAmount,SIB.DefineCurrencyID,SIB.DefineAmount
                //			,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, SIB.AmtDefinitionRate, SBM.SalaryRuleMasterSystemID,SBM.EffectiveDate from SalaryInfoBack SIB
                //			INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID 
                //			left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID  AND SH.SalaryHeadID in (" + salaryHeadId + @") 
                //			--where EmpInfoSystemID = '1800118'
                //                      )dd where EffectiveDate <= '" + effectiveDate + @"' 					

                //			GROUP BY EmpInfoSystemID) effDateSalary


                //			Inner JOIN

                //          ( SELECT EmpInfoSystemID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount 
                //	,AmtDefinitionCurrencyID , AmtDefinationRate, SalaryRuleMasterSystemID,EffectiveDate
                //           FROM (
                //          SELECT MST.EmpInfoSystemID,SH.SalaryHead, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
                //		EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID,MST.EffectiveDate
                //			FROM SalaryInfoDefine EmpSlr
                //			INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID AND MST.IsApproved = 1
                //			LEFT OUTER JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID AND SH.SalaryHeadID in (" + salaryHeadId + @") 
                //		--	WHERE EmpInfoSystemID = '1800118'
                //			UNION
                //			SELECT SBM.EmpInfoSystemID,SH.SalaryHead,SIB.SalaryHeadID,SIB.EntryCurrencyID,SIB.EntryAmount,SIB.DefineCurrencyID,SIB.DefineAmount
                //			,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, SIB.AmtDefinitionRate, SBM.SalaryRuleMasterSystemID,SBM.EffectiveDate from SalaryInfoBack SIB
                //			INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID 
                //			left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID AND SH.SalaryHeadID in (" + salaryHeadId + @") 
                //		--	where EmpInfoSystemID = '1800118'
                //              )dd where EffectiveDate <= '" + effectiveDate + @"'  ) SalaryDetails ON effDateSalary.EffectiveDate= SalaryDetails.EffectiveDate and effDateSalary.EmpInfoSystemID = SalaryDetails.EmpInfoSystemID



                //                                                                -----------------------AND MST.IsApproved = 1---------------------
                //														) A

                //											) EmpSlr ON E.SystemID = EmpSlr.EmpInfoSystemID
                //								LEFT JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
                //								--LEFT JOIN (SELECT * FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId='" + plantId + @"') PSH ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID

                //								LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EmpSlr.SalaryRuleMasterSystemID 
                //								--LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID	AND SRG.SalaryHeadID = SH.SalaryHeadID									
                //                                      LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID


                //                       ) A  where  ISNULL(EmpInfoSystemID,'')<>'' AND CompanyId = '" + plantId + @"' AND
                //                          Convert(date ,DOJ) <='" + effectiveDate + @"' AND (DOS IS NULL OR DOS >='" + effectiveDate + @"') AND SalaryHeadID in (" + salaryHeadId + @") 
                //                          AND CAST(DATEDIFF(mm, A.DOJ, '" + effectiveDate + @"') AS varchar(4))/12 between 
                //					(
                //					 SELECT Min(Convert(Int,Convert(decimal(18,2),GPD.MaturityFromYear))) FROM GratuityPolicyMaster GPM 
                //                              left join org.Plant pp on pp.Id=GPM.plantId
                //	                    LEFT JOIN GratuityPolicyDetails GPD ON GPM.Id = GPD.GratuityPolicyMasterId
                //	                    WHERE pp.CompanyId = '" + plantId + @"'
                //					)
                //					AND
                //					(
                //					 SELECT Max(Convert(Int,Convert(decimal(18,2),GPD.MaturityFromYear))) FROM GratuityPolicyMaster GPM 
                //                                      left join org.Plant ppp on ppp.Id=GPM.plantId
                //	                    LEFT JOIN GratuityPolicyDetails GPD ON GPM.Id = GPD.GratuityPolicyMasterId
                //	                    WHERE ppp.CompanyId = '" + plantId + @"'
                //					)
                //                      ";
                //              strSql = strSql + @" ORDER BY EmployeeCode";

                strSql = @"SELECT *
                                FROM (
                                	SELECT CONVERT(DECIMAL(18,2), CAST(DATEDIFF(mm, A.DOJ, '" + effectiveDate + @"') AS VARCHAR(4))) / 12 CompareDate
                                		,*
                                	FROM (
                                		SELECT E.SystemID EmpSystemID
                                			,E.EmployeeCode EmployeeCode
                                			,E.EmployeeName
                                			,REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB
                                			,E.FatherName
                                			,E.MotherName
                                			,E.EmpType EmployeeType
                                			,E.EmploymentType EmploymentNature
                                			,E.NationalID
                                			,E.GenderID GenderName
                                			,REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ
                                			,eact.IsOutSider
                                			,REPLACE(Convert(VARCHAR(11), E.DOS, 106), ' ', '-') AS DOS
                                			,REPLACE(Convert(VARCHAR(11), E.DOC, 106), ' ', '-') AS DOC
                                			,ISNULL(LG.UserName, '') LegalDesignation
                                			,E.EmployeeStatus
                                			,P.UserName PlantName
                                			,(PAM.[Address1] + ', ' + PAM.[Address2] + ', ' + PAMC.UserName + ' - ' + PAM.Postcode) FactoryAddress
                                			,GC.UserName GroupName
                                			,(CGAM.[Address1] + ', ' + CGAM.[Address2] + ', ' + CT.UserName + ' - ' + CGAM.Postcode + ', Contact: ' + CGAM.Phone) GroupAddress
                                			,E.PlantID
                                			,BK.UserName BankNameShort
                                			,E.BankAccNo
                                			,EmpSlr.SalaryHeadID
                                			,SH.SalaryHead --, ISNULL(PSH.Sequence, 99) Sequence
                                			,SH.HeadType
                                			,ISNULL(SH.HeadCategory, '') HeadCategory
                                			,EmpSlr.EntryCurrencyID
                                			,EmpSlr.EntryAmount
                                			,EmpSlr.DefineCurrencyID
                                			,EmpSlr.DefineAmount
                                			,EmpSlr.AmtDefinitionCurrencyID
                                			,EmpSlr.AmtDefinationRate
                                			,EmpSlr.EmpInfoSystemID
                                			,MW.SalaryHeadValue
                                			,CRC.IntegerInDisb
                                			,CRC.DecimalNo
                                			,MW.Grade
                                			,CRC.IsDecimalInDisb IsDecimal
                                			,ISNULL(E.GenderID, '') Gender
                                			,ISNULL(LSalGr.Code, '') GradeCode
                                			,E.CompanyId
                                			,ISNULL(PG.UserName, '') PayRollGroup
                                			,ISNULL(jl.JobLocation, '') JobLocation
                                			,ISNULL(e.PaymentMode, '') PaymentMode
                                			,ISNULL(bb.UserName, '') BankName
                                			,gpd.MaturityFromYear
                                			,gpd.MaturityToYear
                                		FROM (
                                			SELECT *
                                			FROM EmployeeInformation
                                			WHERE (
                                					EmployeeStatus != 'Separated'
                                					OR DOS IS NULL
                                					OR DOS >= '" + effectiveDate + @"'
                                					)
                                			) AS E
                                		LEFT JOIN GratuityPolicyDetails AS gpd ON gpd.plantId = e.PlantId
                                		LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = E.BudgetCode
                                		LEFT JOIN ORG.Line L ON MB.LineID = L.Id
                                		LEFT JOIN [dbo].[JobLocation] jl ON jl.SystemID = E.JobLocationID
                                		LEFT JOIN [dbo].[EmployeeBankInfo] ebi ON ebi.EmpSystemID = e.SystemId
                                		LEFT JOIN [HKP].[Bank] bb ON bb.Id = ebi.BankSystemID
                                		LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                		LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                		LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
                                		LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LG.Id
                                			AND E.PlantId = LSGD.PlantId
                                		LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = LSGD.LegalSalaryGradeId
                                			AND E.PlantId = LSalGr.PlantId
                                		LEFT JOIN ORG.Plant AS p ON E.PlantId = p.Id
                                		LEFT JOIN ORG.Company AS Cm ON E.CompanyID = Cm.Id
                                		LEFT JOIN ORG.CompanyGroup AS GC ON E.GroupID = GC.Id
                                		LEFT JOIN HKP.Bank AS BK ON E.BankSystemID = BK.Id
                                		LEFT JOIN MST.AddressMaster AS CAM ON Cm.AddressMasterId = CAM.Id
                                		LEFT JOIN MST.AddressMaster AS PAM ON P.AddressMasterId = PAM.Id
                                		LEFT JOIN MST.AddressMaster AS CGAM ON GC.AddressMasterId = CGAM.Id
                                		LEFT JOIN SCS.City AS PAMC ON PAM.CityId = PAMC.Id
                                		LEFT JOIN SCS.City AS CT ON CGAM.CityId = CT.Id
                                		LEFT JOIN mst.DesignationMasterLegalDesignation m ON m.LegalDesignationId = e.LegalDesignationId
                                		LEFT JOIN mst.DesignationMaster dm ON dm.id = m.DesignationMasterId
                                		LEFT JOIN hkp.EmployeeCategory ec ON ec.Id = dm.EmployeeCategoryId
                                		LEFT JOIN (
                                			SELECT E.SystemID
                                				,SUM(SV.SalaryHeadValue) SalaryHeadValue
                                				,LSG.UserName Grade
                                			FROM EmployeeInformation E
                                			LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
                                			LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId
                                				AND E.PlantId = gd.PlantId
                                			LEFT JOIN (
                                				SELECT MAX(EffectiveDate) EffectiveDate
                                					,LegalSalaryGradeId
                                					,EmployeeLocationId
                                				FROM MST.LegalSalaryStructure
                                				WHERE EffectiveDate <= '" + effectiveDate + @"'
                                				GROUP BY LegalSalaryGradeId
                                					,EmployeeLocationId
                                				) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId
                                				AND S.EmployeeLocationId = B.EmployeeLocationId
                                			LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId
                                				AND SS.EmployeeLocationId = S.EmployeeLocationId
                                				AND SS.EffectiveDate = S.EffectiveDate
                                			LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id
                                			LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSG.Id = S.LegalSalaryGradeId
                                			GROUP BY E.SystemId
                                				,LSG.UserName
                                			) MW ON MW.SystemId = E.SystemId
                                		INNER JOIN (
                                			SELECT *
                                			FROM (
                                				--SELECT MST.EmpInfoSystemID, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
                                				--	EmpSlr.AmtDefinitionCurrencyID AmtDefinationCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID
                                				-- FROM SalaryInfoDefine EmpSlr
                                				--	INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID 
                                				SELECT SalaryDetails.*
                                				FROM (
                                					SELECT MAX(EffectiveDate) EffectiveDate
                                						,EmpInfoSystemID --,SalaryHead,SalaryHeadID,EntryCurrencyID
                                					FROM (
                                						SELECT MST.EmpInfoSystemID
                                							,SH.SalaryHead
                                							,EmpSlr.SalaryHeadID
                                							,EmpSlr.EntryCurrencyID
                                							,EmpSlr.EntryAmount
                                							,EmpSlr.DefineCurrencyID
                                							,EmpSlr.DefineAmount
                                							,EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID
                                							,EmpSlr.AmtDefinitionRate AmtDefinationRate
                                							,MST.SalaryRuleMasterSystemID
                                							,MST.EffectiveDate
                                						FROM SalaryInfoDefine EmpSlr
                                						INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID
                                							AND MST.IsApproved = 1
                                						LEFT OUTER JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
                                						--where EmpInfoSystemID = '1800118'
                                						
                                						UNION
                                						
                                						SELECT SBM.EmpInfoSystemID
                                							,SH.SalaryHead
                                							,SIB.SalaryHeadID
                                							,SIB.EntryCurrencyID
                                							,SIB.EntryAmount
                                							,SIB.DefineCurrencyID
                                							,SIB.DefineAmount
                                							,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID
                                							,SIB.AmtDefinitionRate
                                							,SBM.SalaryRuleMasterSystemID
                                							,SBM.EffectiveDate
                                						FROM SalaryInfoBack SIB
                                						INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID
                                						LEFT OUTER JOIN SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID
                                							AND SH.SalaryHeadID IN (" + salaryHeadId + @")
                                							--where EmpInfoSystemID = '1800118'
                                						) dd
                                					WHERE EffectiveDate <= '" + effectiveDate + @"'
                                					GROUP BY EmpInfoSystemID
                                					) effDateSalary
                                				INNER JOIN (
                                					SELECT EmpInfoSystemID
                                						,SalaryHeadID
                                						,EntryCurrencyID
                                						,EntryAmount
                                						,DefineCurrencyID
                                						,DefineAmount
                                						,AmtDefinitionCurrencyID
                                						,AmtDefinationRate
                                						,SalaryRuleMasterSystemID
                                						,EffectiveDate
                                					FROM (
                                						SELECT MST.EmpInfoSystemID
                                							,SH.SalaryHead
                                							,EmpSlr.SalaryHeadID
                                							,EmpSlr.EntryCurrencyID
                                							,EmpSlr.EntryAmount
                                							,EmpSlr.DefineCurrencyID
                                							,EmpSlr.DefineAmount
                                							,EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID
                                							,EmpSlr.AmtDefinitionRate AmtDefinationRate
                                							,MST.SalaryRuleMasterSystemID
                                							,MST.EffectiveDate
                                						FROM SalaryInfoDefine EmpSlr
                                						INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID
                                							AND MST.IsApproved = 1
                                						LEFT OUTER JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
                                							AND SH.SalaryHeadID IN (" + salaryHeadId + @")
                                						--	WHERE EmpInfoSystemID = '1800118'
                                						
                                						UNION
                                						
                                						SELECT SBM.EmpInfoSystemID
                                							,SH.SalaryHead
                                							,SIB.SalaryHeadID
                                							,SIB.EntryCurrencyID
                                							,SIB.EntryAmount
                                							,SIB.DefineCurrencyID
                                							,SIB.DefineAmount
                                							,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID
                                							,SIB.AmtDefinitionRate
                                							,SBM.SalaryRuleMasterSystemID
                                							,SBM.EffectiveDate
                                						FROM SalaryInfoBack SIB
                                						INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID
                                						LEFT OUTER JOIN SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID
                                							AND SH.SalaryHeadID IN (" + salaryHeadId + @")
                                							--	where EmpInfoSystemID = '1800118'
                                						) dd
                                					WHERE EffectiveDate <= '" + effectiveDate + @"'
                                					) SalaryDetails ON effDateSalary.EffectiveDate = SalaryDetails.EffectiveDate
                                					AND effDateSalary.EmpInfoSystemID = SalaryDetails.EmpInfoSystemID
                                					-----------------------AND MST.IsApproved = 1---------------------
                                				) A
                                			) EmpSlr ON E.SystemID = EmpSlr.EmpInfoSystemID
                                		LEFT JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
                                		--LEFT JOIN (SELECT * FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId='C20201') PSH ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
                                		LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EmpSlr.SalaryRuleMasterSystemID
                                		--LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID	AND SRG.SalaryHeadID = SH.SalaryHeadID									
                                		LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID
                                			AND CRC.SalaryHeadID = SH.SalaryHeadID
                                		LEFT JOIN [dbo].[EmployeeCodeType] eact ON eact.Id = e.EmployeeCodeTypeId
                                		) A
                                	WHERE ISNULL(EmpInfoSystemID, '') <> ''
                                		AND CompanyId = '" + plantId + @"'
                                		AND A.IsOutSider = 0
                                		AND Convert(DATE, DOJ) <= '" + effectiveDate + @"'
                                		AND (
                                			DOS IS NULL
                                			OR DOS >= '" + effectiveDate + @"'
                                			)
                                		AND SalaryHeadID IN (" + salaryHeadId + @")
                                	) xx
                                WHERE xx.compareDate BETWEEN xx.MaturityFromYear
                                		AND xx.MaturityToYear
                                ORDER BY EmployeeCode";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpSystemID"].ToString();
                }

                return dicBonus;
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

        public Dictionary<string, DataRow> GetGratuityPolicy(string plantId)
        {
            Dictionary<string, DataRow> dicGrpolicy = new Dictionary<string, DataRow>();


            string strSql = @"SELECT IGP.PolicyNo GratuityNo,GIA.AgreementNo PolicyNo,
						 IGP.EmployeeSystemId  FROM IndividualGratuityPolicy IGP
						 Inner join EmployeeInformation EEI ON EEI.SystemId = IGP.EmployeeSystemId
						 inner JOIN GratuityInsuranceAgreement GIA ON GIA.Id = IGP.AgreementId   WHERE EEI.CompanyId = '" + plantId + @"'";

            DataTable dt = _sqlRepository.GetDataTable(strSql);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dicGrpolicy.ContainsKey(dt.Rows[i]["EmployeeSystemId"].ToString()))
                {
                    continue;
                }
                dicGrpolicy.Add(dt.Rows[i]["EmployeeSystemId"].ToString(), dt.Rows[i]);
            }
            return dicGrpolicy;
        }
        private void SalaryHeadGratuity(string plantId, out string resultString, out DataTable dtGratuityPolicy)
        {
            string strsql = "";

            strsql = @"SELECT GPD.*,GPM.IsRoudingSixMonth FROM GratuityPolicyMaster GPM 
			                    LEFT JOIN GratuityPolicyDetails GPD ON GPM.Id = GPD.GratuityPolicyMasterId
			                    LEFT JOIN ORG.Plant p on p.Id=GPD.plantId
			                    WHERE p.CompanyId = '" + plantId + @"'";
            DataTable dtGratuity = _sqlRepository.GetDataTable(strsql);
            dtGratuityPolicy = dtGratuity;

            List<DataRow> _data = new List<DataRow>();
            // resultString = "";
            resultString = "''";
            for (int i = 0; i < dtGratuity.Rows.Count; i++)
            {
                string subject = dtGratuity.Rows[i]["MaturityFormulaDesID"].ToString();


                string[] allTexts = subject.Split(' ');

                for (int ri = 0; ri < allTexts.Length; ri++)
                {
                    if (allTexts[ri].Trim() != "")
                        resultString += ",'" + allTexts[ri] + "'";
                }


            }

            //return dicGratuity;


        }

        string GetDecimalFormat(bool isInt, int decimalNo)
        {
            try
            {
                var ob = new ReportUtility();
                if (isInt == true)
                {
                    return ob.NumberFormatInt();
                }
                else
                {
                    return ob.GetDynamicDecimalPlace(decimalNo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private int gratuityYear(DateTime DOJ, DateTime calcDate, int year, int month, bool IsRoudingSixMonth)
        {
            try
            {
                var numberOfYear = year;
                var newDojYear = DOJ.AddYears(year);
                var newDoj = newDojYear;
                //var newDoj = newDojYear.AddMonths(month - 1);
                if (IsRoudingSixMonth)
                {
                    newDoj = newDojYear.AddMonths(6);
                    if (newDoj < calcDate)
                    {
                        numberOfYear = numberOfYear + 1;
                    }
                }


                //var days = calcDate.Subtract(newDoj).Days +1;

                //if ((month - 1) == 5)
                //{
                //    if (days > 30)
                //    {
                //        int calcMonthDays = DateTime.DaysInMonth(calcDate.Year, calcDate.Month);
                //        var prevCalcDate = calcDate.AddMonths(-1);

                //        int prevCalcMonthDays = DateTime.DaysInMonth(prevCalcDate.Year, (prevCalcDate.Month));

                //        numberOfYear = getYearFromMonth(days, prevCalcMonthDays, numberOfYear);
                //    }
                //}
                return numberOfYear;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        private string GetSalaryheadValue(DataView dvBioDvAC, string catType)
        {
            var basicValue = string.Empty;
            try
            {

                var basic = from r in dvBioDvAC.ToTable().AsEnumerable()
                            where r.Field<string>("cat") == catType
                            select r;
                if (basic.Count() > 0)
                {

                    DataTable dtt = basic.CopyToDataTable();
                    basicValue = dtt.Rows[0]["DisbusmentAmount"].ToString();


                }
                return basicValue;
            }
            catch (Exception)
            {

                throw;
            }
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


        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = 4;
            ColIndex = xlsCol;
        }
        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol)
        {

            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = 4;
        }


        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            ColIndex = xlsCol;
            xlsCol += 1;
        }


        private string GetEmployeeName(DataView dvBioDvAC, string EmpCode)
        {
            var Employeename = string.Empty;
            try
            {

                var EmployeList = from r in dvBioDvAC.ToTable().AsEnumerable()
                                  where r.Field<string>("EmployeeCode") == EmpCode
                                  select r;
                if (EmployeList.Count() > 0)
                {

                    DataTable dtt = EmployeList.CopyToDataTable();
                    Employeename = dtt.Rows[0]["EmployeeName"].ToString();

                }
                return Employeename;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string GetWorkingDate(DataView dvBioDvAC, string EmpCode)
        {
            var WorkingDays = string.Empty;
            try
            {

                var workDaysList = from r in dvBioDvAC.ToTable().AsEnumerable()
                                   where r.Field<string>("EmployeeCode") == EmpCode
                                   select r;
                if (workDaysList.Count() > 0)
                {

                    DataTable dtt = workDaysList.CopyToDataTable();
                    WorkingDays = dtt.Rows[0]["workingDays"].ToString();

                }
                return WorkingDays;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private string GetPFNo(DataView dvBioDvAC, string EmpCode)
        {
            var PFNo = string.Empty;
            try
            {

                var pfList = from r in dvBioDvAC.ToTable().AsEnumerable()
                             where r.Field<string>("EmployeeCode") == EmpCode
                             select r;
                if (pfList.Count() > 0)
                {

                    DataTable dtt = pfList.CopyToDataTable();
                    PFNo = dtt.Rows[0]["DocNumber"].ToString();

                }
                return PFNo;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private string GetEmpAge(DataView dvBioDvAC, string EmpCode)
        {
            var Age = string.Empty;
            try
            {

                var AgeList = from r in dvBioDvAC.ToTable().AsEnumerable()
                              where r.Field<string>("EmployeeCode") == EmpCode
                              select r;
                if (AgeList.Count() > 0)
                {

                    DataTable dtt = AgeList.CopyToDataTable();
                    Age = dtt.Rows[0]["Age"].ToString();

                }
                return Age;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string GetDuration(string dti, string dto, string intime, string outtime)
        {
            string res = string.Empty;
            try
            {
                // string vDate = Convert.ToDateTime(sDate).ToString("dd-MMM-yyyy");

                if (string.IsNullOrEmpty(intime) == false && string.IsNullOrEmpty(outtime) == false)
                {
                    string vintime = Convert.ToDateTime(intime).ToString("HH:mm:ss");
                    string vouttime = Convert.ToDateTime(outtime).ToString("HH:mm:ss");
                    var x = (Convert.ToDateTime(dto) - (Convert.ToDateTime(dti)));
                    res = x.ToString().Substring(0, 5);
                    //res = (Convert.ToDateTime(dto)-(Convert.ToDateTime(dti))).ToString().Substring(0, 5);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        string GetDecimalFormat(SalaryHeadSequence shs)
        {
            try
            {
                var ob = new ReportUtility();
                if (shs.IsInt)
                {
                    return ob.NumberFormatInt();
                }
                else
                {
                    return ob.GetDynamicDecimalPlace(shs.DecimalNo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion -- Operations


    }
}