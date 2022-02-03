using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
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
    public class clsNewEarnLeaveReport
    {
        ISqlRepository _sqlRepository;
        public clsNewEarnLeaveReport()
        {
            _sqlRepository = new SqlRepository();
        }
        public string GetReport(string FromDate, string ToDate)
        {
            #region Variable

            clsReport objRpt = null;

            ReportUtility oru = new ReportUtility();

            DataSet dsAttn = null;

            DataView dvAttn = null;
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

            var report = new ReportUtility();
            var iSl = 0;
            var iEmpCode = 0;
            var iEmpName = 0;
            var iGender = 0;
            var iDOJ = 0;
            var iFromDate = 0;
            var iToDate = 0;
            var iEmpCategory = 0;
            var iDepartment = 0;
            var iSection = 0;
            var iSubSection = 0;
            var iLine = 0;
            var iDesignation = 0;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string companyId = identity.CompanyId;
                objRpt = new clsReport();


                #region DataSet
                getEmployee(FromDate,ToDate, out dsAttn);

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

                    int strCount = 0;

                    #region ------------------Column Header------------------
                    xlsCol = 1;

                    iSl = xlsCol;
                    sheet1.Range[xlsRow, iSl].Text = "Sl No.";
                    sheet1.Range[xlsRow, iSl].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iSl].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iSl].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iSl].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    iEmpCode = xlsCol;
                    sheet1.Range[xlsRow, iEmpCode].Text = "Employee Code";
                    sheet1.Range[xlsRow, iEmpCode].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iEmpCode].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iEmpCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    iEmpName = xlsCol;
                    sheet1.Range[xlsRow, iEmpName].Text = "Employee Name";
                    sheet1.Range[xlsRow, iEmpName].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iEmpName].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iEmpName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iEmpName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    iGender = xlsCol;
                    sheet1.Range[xlsRow, iGender].Text = "Gender";
                    sheet1.Range[xlsRow, iGender].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iGender].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iGender].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iGender].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    iDOJ = xlsCol;
                    sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet1.Range[xlsRow, iDOJ].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iDOJ].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iDOJ].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;


                    iFromDate = xlsCol;
                    sheet1.Range[xlsRow, iFromDate].Text = "From Date";
                    sheet1.Range[xlsRow, iFromDate].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iFromDate].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iFromDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iFromDate].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    iToDate = xlsCol;
                    sheet1.Range[xlsRow, iToDate].Text = "To Date";
                    sheet1.Range[xlsRow, iToDate].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iToDate].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iToDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iToDate].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;


                    iEmpCategory = xlsCol;
                    sheet1.Range[xlsRow, iEmpCategory].Text = "Emp. Category";
                    sheet1.Range[xlsRow, iEmpCategory].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iEmpCategory].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    iEmpCategory = xlsCol;
                    sheet1.Range[xlsRow, iEmpCategory].Text = "Emp. Category";
                    sheet1.Range[xlsRow, iEmpCategory].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iEmpCategory].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    iDepartment = xlsCol;
                    sheet1.Range[xlsRow, iDepartment].Text = "Department";
                    sheet1.Range[xlsRow, iDepartment].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iDepartment].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iDepartment].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iDepartment].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    iSection = xlsCol;
                    sheet1.Range[xlsRow, iSection].Text = "Section";
                    sheet1.Range[xlsRow, iSection].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iSection].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iSection].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iSection].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    iSubSection = xlsCol;
                    sheet1.Range[xlsRow, iSubSection].Text = "Sub Section";
                    sheet1.Range[xlsRow, iSubSection].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iSubSection].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iSubSection].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iSubSection].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    iLine = xlsCol;
                    sheet1.Range[xlsRow, iLine].Text = "Line";
                    sheet1.Range[xlsRow, iLine].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iLine].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iLine].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iLine].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    iDesignation = xlsCol;
                    sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet1.Range[xlsRow, iDesignation].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iDesignation].RowHeight = 4.70;
                    sheet1.Range[xlsRow, iDesignation].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iDesignation].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    //iLine = xlsCol;
                    //sheet1.Range[xlsRow, iLine].Text = "Line";
                    //sheet1.Range[xlsRow, iLine].ColumnWidth = 4.70;
                    //sheet1.Range[xlsRow, iLine].RowHeight = 4.70;
                    //sheet1.Range[xlsRow, iLine].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, iLine].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //xlsCol += 1;
                    
                    endXlsCol = xlsCol;

                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                    xlsCol = 1;
                    xlsRow += 1;
                    #endregion ------------------Column Header------------------
                    strCount = 0;
                    for (int i = 0; i < dvAttn.Count; i++)
                    {
                        xlsCol = 1;
                        xlsRow += intRow;
                        intRow = 1;
                        #region ----------------------Data-----------------------
                        strCount ++;
                        sheet1.Range[xlsRow, iSl].Text = strCount.ToString();
                        sheet1.Range[xlsRow, iEmpCode].Text = dvAttn[i]["EmployeeCode"].ToString();

                        #endregion ----------------------Data-----------------------



                        xlsRow++;
                        
                    }

                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;
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
                    sheet1.Range[xlsRow, 3].Text = "Earn Leave Payment Amount Status";
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
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

                    sheet1.Name = "Earn Leave";
                    #endregion             

                    workbook.Version = ExcelVersion.Excel97to2003;
                    report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);

                    // return workbook;

                    var filePath = "";
                    var SheetName = "Earn Leave";
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
        public void getEmployee(string FromDate, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT ei.EmployeeCode,ei.EmployeeName,ei.GenderID,FORMAT(ei.DOJ,'dd-MMM-yyyy')DOJ
                                    ,En.UserName EmployeeCategory,dp.UserName Department,SE.UserName Section
                                    ,ISNULL(Li.UserName,'') Line
                                    ,Deg.UserName Designation
                                    FROM LeaveEncashmentTransaction L
                                    JOIN TRN.EmployeeLeaveSummary AS els ON els.EmployeeId=L.EmpSystemId AND L.EncashmentDate = els.ToDate
                                    JOIN EmployeeInformation AS ei ON ei.SystemId = L.EmpSystemId AND ei.SystemId=els.EmployeeId
                                    LEFT JOIN MST.ManpowerBudget PMB ON ei.BudgetCode = PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                    LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id
                                    LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                                    LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = L.LegalDesignationId
                                    LEFT join [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                                    left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                                    left join HKP.Designation DeG on DeG.Id=dm.DesignationId
                                    left join HKP.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
                                    left join ORG.Section SE on SE.Id=PR.SectionId
                                    LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                    LEFT JOIN ORG.Line AS Li ON Li.Id= PMB.LineId";
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
    }
}
