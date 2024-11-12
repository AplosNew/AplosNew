#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Attendances.Controllers
{
    public class SpecialDutyController : BaseController
    {

        string TableName = "dbo.SpecialDuty";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public SpecialDutyController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

     
        public ActionResult Report()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetList(string workDate)
        {

            string sql = @"Select ISNULL(SD.IsApproved,0)IsApproved,SD.Id,E.SystemId EmpSystemId,E.EmployeeCode,E.EmployeeName,FORMAT(SD.WorkDate,'dd-MMM-yyyy')WorkDate,CONVERT(varchar(15),CAST(SD.Intime AS TIME),100) InTime
,CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100) OutTime,ISNULL(SD.InputMinute,0)InputMinute
,ABS(DATEDIFF(MINUTE, SD.InTime, SD.OutTime)) AS CalculatedMinute
,ApprovedMinute=CASE WHEN ISNULL(SD.InputMinute,0)<ABS(DATEDIFF(MINUTE, SD.InTime, SD.OutTime)) THEN ISNULL(SD.InputMinute,0) ELSE ABS(DATEDIFF(MINUTE, SD.InTime, SD.OutTime)) END
,LD.UserName LegalDesignation,DEPT.UserName AS Department ,DV.UserName AS Division,SC.UserName AS Section,SS.UserName SubSection
 ,FORMAT(E.DOJ,'dd-MMM-yyyy') DOJ,EC.UserName EmployeeCategory
,E.EmployeeStatus
from dbo.SpecialDuty SD
LEFT JOIN EmployeeInformation E ON E.SystemId=SD.EmpSystemId
LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=PR.DesignationId
LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
LEFT JOIN ORG.Division DV ON E.DivisionId = DV.Id
LEFT JOIN ORG.Section SC ON E.SectionId = SC.Id
LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
LEFT JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
Where WorkDate='" + workDate + @"' AND ISNULL(SD.IsApproved,0)=0";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSDApprovedData(string workDate)
        {

            string sql = @"Select ISNULL(SD.IsApproved,0)IsApproved,SD.Id,E.SystemId EmpSystemId,E.EmployeeCode,E.EmployeeName,FORMAT(SD.WorkDate,'dd-MMM-yyyy')WorkDate,CONVERT(varchar(15),CAST(SD.Intime AS TIME),100) InTime
,CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100) OutTime,ISNULL(SD.InputMinute,0)InputMinute,SD.CalculatedMinute,SD.ApprovedMinute
,LD.UserName LegalDesignation,DEPT.UserName AS Department ,DV.UserName AS Division,SC.UserName AS Section,SS.UserName SubSection
 ,FORMAT(E.DOJ,'dd-MMM-yyyy') DOJ,EC.UserName EmployeeCategory
,E.EmployeeStatus
from dbo.SpecialDuty SD
LEFT JOIN EmployeeInformation E ON E.SystemId=SD.EmpSystemId
LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=PR.DesignationId
LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
LEFT JOIN ORG.Division DV ON E.DivisionId = DV.Id
LEFT JOIN ORG.Section SC ON E.SectionId = SC.Id
LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
LEFT JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
Where WorkDate='" + workDate + @"' AND ISNULL(SD.IsApproved,0)=1";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveSDData(List<Dictionary<string, object>> data, string workDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("Select * from dbo.SpecialDuty Where WorkDate='" + workDate + "' AND ISNULL(IsApproved,0)=0", out dsChild, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count > 0)
                        {

                            DataRow drmo = dv[0].Row;
                            drmo["IsApproved"] = true;
                            drmo["ApproveBy"] = identity.EmployeeId;
                            drmo["UpdatedBy"] = identity.Name;
                            drmo["UpdatedDate"] = DateTime.Now.ToString();
                            drmo["UpdatedFromIP"] = identity.IPAddress;
                            EditRow(drmo, item);
                        }
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsChild);
                }


                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
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

        public ActionResult GetSDDataInDateRange(string fromDate, string toDate)
        {
            try
            {
                var sql = @"Select Count(SD.Id)EarnedDays,Count(AD.EmpSystemID) AvailedDays,Balance=Count(SD.Id)-Count(AD.EmpSystemID),SD.EmpSystemID
,E.EmployeeCode,E.EmployeeName,LD.UserName LegalDesignation,DEPT.UserName AS Department ,DV.UserName AS Division,SC.UserName AS Section
,SS.UserName SubSection ,FORMAT(E.DOJ,'dd-MMM-yyyy') DOJ,EC.UserName EmployeeCategory,E.EmployeeStatus
From dbo.SpecialDuty SD
LEFT JOIN dbo.AttdnProcessData AD ON AD.EmpSystemID=SD.EmpSystemId AND AD.WorkDate=SD.WorkDate AND AD.DayStatus='OD'
LEFT JOIN EmployeeInformation E ON E.SystemId=SD.EmpSystemId
LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=PR.DesignationId
LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
LEFT JOIN ORG.Division DV ON E.DivisionId = DV.Id
LEFT JOIN ORG.Section SC ON E.SectionId = SC.Id
LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
LEFT JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
Where SD.WorkDate between '" + fromDate + @"' AND '" + fromDate + @"' AND SD.IsApproved=1 Group By SD.EmpSystemId
,E.EmployeeCode,E.EmployeeName,LD.UserName,DEPT.UserName,DV.UserName,SC.UserName
,SS.UserName,FORMAT(E.DOJ,'dd-MMM-yyyy'),EC.UserName,E.EmployeeStatus";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string GetSpecialDutyReportInDateRangexlx(DataTable data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Special Duty Report";
                sheet = workbook.Worksheets[0];
                int ROW = 5; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Employee Code"; sheet[ROW, COL].ColumnWidth = 10; int ColEmployeeCode = COL; COL++;
                sheet[ROW, COL].Text = "Employee Name"; sheet[ROW, COL].ColumnWidth = 20; int ColEmployeeName = COL; COL++;
                sheet[ROW, COL].Text = "Legal Designation"; sheet[ROW, COL].ColumnWidth = 20; int ColLD = COL; COL++;
                sheet[ROW, COL].Text = "Department"; sheet[ROW, COL].ColumnWidth = 12; int ColDepartment = COL; COL++;
                sheet[ROW, COL].Text = "Division"; sheet[ROW, COL].ColumnWidth = 12; int ColDivision = COL; COL++;
                sheet[ROW, COL].Text = "Section"; sheet[ROW, COL].ColumnWidth = 12; int ColSection = COL; COL++;
                sheet[ROW, COL].Text = "SubSection"; sheet[ROW, COL].ColumnWidth = 12; int ColSSection = COL; COL++;
                sheet[ROW, COL].Text = "DOJ"; sheet[ROW, COL].ColumnWidth = 12; int ColDOJ = COL; COL++;
                sheet[ROW, COL].Text = "Employee Category"; sheet[ROW, COL].ColumnWidth = 15; int ColEC = COL; COL++;
                sheet[ROW, COL].Text = "Employee Status"; sheet[ROW, COL].ColumnWidth = 15; int ColEStatus = COL; COL++;
                sheet[ROW, COL].Text = "Earned Days"; sheet[ROW, COL].ColumnWidth = 15; int ColEarnedDays = COL; COL++;
                sheet[ROW, COL].Text = "Availed Days"; sheet[ROW, COL].ColumnWidth = 15; int ColAvailedDays = COL; COL++;
                sheet[ROW, COL].Text = "Balance"; sheet[ROW, COL].ColumnWidth = 15; int ColBalance = COL;
                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                ROW++;

                int startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, ColSSection].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColDivision].Text = data.Rows[i]["Division"].ToString();
                    sheet[ROW, ColLD].Text = data.Rows[i]["LegalDesignation"].ToString();
                    sheet[ROW, ColDOJ].Text = data.Rows[i]["DOJ"].ToString();
                    sheet[ROW, ColEC].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, ColEStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                    sheet[ROW, ColEarnedDays].Number = clsStaticInfo.dbl(data.Rows[i]["EarnedDays"].ToString());
                    sheet[ROW, ColEarnedDays].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColAvailedDays].Number = clsStaticInfo.dbl(data.Rows[i]["AvailedDays"].ToString());
                    sheet[ROW, ColAvailedDays].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColBalance].Number = clsStaticInfo.dbl(data.Rows[i]["Balance"].ToString());
                    sheet[ROW, ColBalance].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                //reportUtility.PlantHeader(ref sheet, endCol, "Good Work Report", identity.PlantId);
                reportUtility.CompanyHeader(ref sheet, endCol, "Special Duty Report", identity.CompanyId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                //sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetSpecialDutyReportInDateRange(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("ID") || item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;

                    dt.Columns.Add(item);
                }


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("ID") || item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;

                        dr[item] = data[i][item];
                    }

                    dt.Rows.Add(dr);
                }
                //string filename = GridToExcelReportUpd(dt, "", reportFileName);

                string fileName = "";
                fileName = GetSpecialDutyReportInDateRangexlx(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}