using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.EmployeeServices;
using bplib;
using Newtonsoft.Json;
using Library.Service.Helpers;
using System.IO;
using Syncfusion.XlsIO;
using System.Drawing;
using ConnectionManager;
using Library.Data;

namespace Library.HumanResource.NewOTProcess
{
    public class HourlyOTReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public HourlyOTReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        public IWorkbook GetHourlyOT(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string FromDate, string ToDate)
        {

            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsHourlyOffDutyTag = null;
            DataTable dtHourlyOffDutyTag = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataView dvOT = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            #endregion
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                GetHourotReport(FromDate, ToDate, PlantId,out dsHourlyOffDutyTag);
                dtHourlyOffDutyTag = dsHourlyOffDutyTag.Tables[0];

                objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);
                objRpt.SelectedPlant(PlantId, out dsFactory);
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                var iName = 0;
                var iEmployeeCode = 0;
                var iSubSection = 0;
                var iSection = 0;
                var iDurationH = 0;
                var iLine = 0;
                var iWorkDate = 0;
                var iDOJ = 0;
                var iDepartment = 0;
                var iDesignation = 0;
                var iDayStatus = 0;
                var iGender = 0;
                var isl = 0;
               
                var SLNo = 1;

                if (dsHourlyOffDutyTag.Tables[0].Rows.Count == 0)
                {
                    throw new CustomException("No Data Found....");
                }
                #region Hourly Ot

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 6;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "SL";
                sheet1.Range[xlsRow, isl].ColumnWidth = 7;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCode].Text = "Emp Code";
                sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                xlsCol += 1;
                iName = xlsCol;
                sheet1.Range[xlsRow, iName].Text = "Emp Name";
                sheet1.Range[xlsRow, iName].ColumnWidth = 18;

                xlsCol += 1;
                iDOJ = xlsCol;
                sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                sheet1.Range[xlsRow, iDOJ].ColumnWidth = 20;

                xlsCol += 1;
                iWorkDate = xlsCol;
                sheet1.Range[xlsRow, iWorkDate].Text = "Work Date";
                sheet1.Range[xlsRow, iWorkDate].ColumnWidth = 20;

                xlsCol += 1;
                iDayStatus = xlsCol;
                sheet1.Range[xlsRow, iDayStatus].Text = "Day Status";
                sheet1.Range[xlsRow, iDayStatus].ColumnWidth = 20;

                xlsCol += 1;
                iDepartment = xlsCol;
                sheet1.Range[xlsRow, iDepartment].Text = "Department";
                sheet1.Range[xlsRow, iDepartment].ColumnWidth = 30;

                xlsCol += 1;
                iDesignation = xlsCol;
                sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                sheet1.Range[xlsRow, iDesignation].ColumnWidth = 30;

                xlsCol += 1;
                iGender = xlsCol;
                sheet1.Range[xlsRow, iGender].Text = "Gender";
                sheet1.Range[xlsRow, iGender].ColumnWidth = 30;

                xlsCol += 1;
                iSection = xlsCol;
                sheet1.Range[xlsRow, iSection].Text = "Section";
                sheet1.Range[xlsRow, iSection].ColumnWidth = 14;

                xlsCol += 1;
                iSubSection = xlsCol;
                sheet1.Range[xlsRow, iSubSection].Text = "Sub Section";
                sheet1.Range[xlsRow, iSubSection].ColumnWidth = 16;

                xlsCol += 1;
                iLine = xlsCol;
                sheet1.Range[xlsRow, iLine].Text = "Line";
                sheet1.Range[xlsRow, iLine].ColumnWidth = 12;
                               
                xlsCol += 1;
                iDurationH = xlsCol;
                sheet1.Range[xlsRow, iDurationH].Text = "Duration";
                sheet1.Range[xlsRow, iDurationH].ColumnWidth = 20;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------

                for (int i = 0; i < dtHourlyOffDutyTag.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------  

                    dvOT = new DataView();
                    dvOT.Table = dsHourlyOffDutyTag.Tables[0];
                    string yot = string.Empty;
                    oru.GetOT(dsHourlyOffDutyTag.Tables[0].Rows[i]["OTConsiderOn"].ToString(), dsHourlyOffDutyTag.Tables[0].Rows[i]["Duration"].ToString(), out yot);

                    sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet1.Range[xlsRow, iEmployeeCode].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, iName].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, iDOJ].Text = dtHourlyOffDutyTag.Rows[i]["DOJ"].ToString();
                    sheet1.Range[xlsRow, iWorkDate].Text = dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString();
                    sheet1.Range[xlsRow, iDepartment].Text = dtHourlyOffDutyTag.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, iSection].Text = dtHourlyOffDutyTag.Rows[i]["Section"].ToString();
                    sheet1.Range[xlsRow, iSubSection].Text = dtHourlyOffDutyTag.Rows[i]["SubSection"].ToString();
                    sheet1.Range[xlsRow, iLine].Text = dtHourlyOffDutyTag.Rows[i]["Line"].ToString();
                    sheet1.Range[xlsRow, iDurationH].Text = yot.ToString();
                    sheet1.Range[xlsRow, iDayStatus].Text = dtHourlyOffDutyTag.Rows[i]["DayStatus"].ToString();
                    sheet1.Range[xlsRow, iDayStatus].Text = dtHourlyOffDutyTag.Rows[i]["DayStatus"].ToString();
                    sheet1.Range[xlsRow, iDesignation].Text = dtHourlyOffDutyTag.Rows[i]["Designation"].ToString();
                    sheet1.Range[xlsRow, iGender].Text = dtHourlyOffDutyTag.Rows[i]["GenderId"].ToString();
                 
                    xlsRow++;
                    SLNo++;
                }
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************

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
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Hourly OT From : " + FromDate + " To " + ToDate;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Hourly Ot";
                #endregion Page Setup


                #endregion  Attendance Summary Status

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetHourotReport(string FromDate, string ToDate, string plantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @" select ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(ap.WorkDate,'dd-MMM-yyyy')WorkDate,s.UserName as Section,sb.UserName as SubSection,ld.UserName Designation
                                      ,d.UserName Department,ap.DayStatus,ei.GenderID,ap.AdditionalOT as Duration,
                                      ap.EmpSystemId,FORMAT(ap.WorkDate,'dd-MMM-yyyy hh:mm tt') WorkDate,
									  l.UserName as Line
                                      ,(cast(ap.AdditionalOT as decimal(18,2) )/60)as DurationH,
									  hr.OTConsiderOn                                        
                                      From AttdnProcessData ap 
                                      LEFT JOIN EmployeeInformation ei on ei.SystemId=ap.EmpSystemId
                                      LEFT JOIN [ORG].[Section] s on s.Id=ei.SectionId
                                      LEFT JOIN [ORG].[SubSection] sb on sb.Id=ei.SubSectionId
                                      LEFT JOIN [HKP].[LegalDesignation] ld on ld.Id=ei.LegalDesignationId
                                      LEFT JOIN [ORG].[Department] d on d.Id=ei.DepartmentId
                                      LEFT JOIN [ORG].[Line] l on l.Id=ei.LineId
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=ap.PlantId     
                                    where ap.WorkDate 
									between '"+FromDate+"' and '"+ToDate+@"'  
									and ei.PlantId='"+plantId+@"' and ap.AdditionalOT is not null
                                    order by ei.EmployeeCode,ap.WorkDate";
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



    }
}

