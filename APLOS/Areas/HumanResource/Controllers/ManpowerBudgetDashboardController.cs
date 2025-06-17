using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Organizations;
using Library.ViewModel.Accounts;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ManpowerBudgetDashboardController : BaseController
    {
        // private readonly IManpowerBudgetDashboardService _hrDashboardService;

        private readonly Library.HumanResource.Dashboard.HRDashboardService _HRDashboard;
        private readonly Library.HumanResource.Dashboard.ManPowerBudgetDashboardService _hrDashboardService;

        public ManpowerBudgetDashboardController()
        {
            _hrDashboardService = new Library.HumanResource.Dashboard.ManPowerBudgetDashboardService();
            _HRDashboard = new Library.HumanResource.Dashboard.HRDashboardService();
        }

        //public ManpowerBudgetDashboardController(IManpowerBudgetDashboardService hrDashboardService)
        //{
        //    _hrDashboardService = hrDashboardService;
        //}
            
       
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult Report()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetGroupWiseCompanyList(string date, string status, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_hrDashboardService.GroupWiseCompanyList(identity.CompanyGroupId,date, status, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetDrillDownListJSON(string CompanyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboard.OrgStructureListColList(identity.CompanyGroupId,CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCompanyDrillDownListJSON(string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hrDashboardService.CompanyWiseDrillDownList(identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetDetailDrillDownTable(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_hrDashboardService.DetailDrillDownTable(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalEmployeeSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string status, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_hrDashboardService.ModalGroupWiseEmlpoyeeList(identity.CompanyGroupId, ChartColumnList, seq, status, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult ModalEmployeeDetail(IEnumerable<ChartColumnList> chartColumnList, string companyId, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_hrDashboardService.ModalEmlpoyeeListDetail(chartColumnList, identity.CompanyGroupId, companyId, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_hrDashboardService.ModalEmlpoyeeListDetail(chartColumnList, identity.CompanyGroupId, companyId, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalBudgetSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            JsonResult jsondata =  Json(_hrDashboardService.ModalBudgetSummary(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

            //return Json(_hrDashboardService.ModalBudgetSummary(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalBudgetDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var jsondata = Json(_hrDashboardService.ModalBudgetDetail(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, parameters,identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_hrDashboardService.ModalBudgetDetail(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalExcessSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status,string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hrDashboardService.ModalExcessSummary(ChartColumnList, identity.CompanyGroupId, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalExcessDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hrDashboardService.ModalExcessDetail(ChartColumnList, identity.CompanyGroupId, seq, date, status, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalShortSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hrDashboardService.ModalShortSummary(ChartColumnList, identity.CompanyGroupId, seq, date, status, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalShortDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hrDashboardService.ModalShortDetail(ChartColumnList, identity.CompanyGroupId, seq, date, status, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult BudgetCodeWiseEmpList(IEnumerable<ChartColumnList> ChartColumnList, string budgetCode, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hrDashboardService.BudgetCodeWiseEmpList(ChartColumnList, identity.CompanyGroupId, budgetCode, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult WPBudgetCodeWiseEmpList(IEnumerable<ChartColumnList> ChartColumnList, string budgetCode)
        {
            return Json(_hrDashboardService.WpBudgetCodeWiseEmpList(ChartColumnList, budgetCode), JsonRequestBehavior.AllowGet);
        }

        public ActionResult OnRoleEmployeeReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;
                    dt.Columns.Add(item);
                }
                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;
                        dr[item] = data[i][item];
                    }
                    dt.Rows.Add(dr);
                }
                string fileName = "";
                fileName = _hrDashboardService.CreateOnRoleEmployeeReportSheet(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult BudgetEmployeeReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;
                    dt.Columns.Add(item);
                }
                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;
                        dr[item] = data[i][item];
                    }
                    dt.Rows.Add(dr);
                }
                string fileName = "";
                fileName = _hrDashboardService.CreateBudgetEmployeeReportReportSheet(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult ShortEmployeeReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;
                    dt.Columns.Add(item);
                }
                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;
                        dr[item] = data[i][item];
                    }
                    dt.Rows.Add(dr);
                }
                string fileName = "";
                fileName = _hrDashboardService.CreateShortEmployeeReportReportSheet(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult ExcessEmployeeReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;
                    dt.Columns.Add(item);
                }
                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;
                        dr[item] = data[i][item];
                    }
                    dt.Rows.Add(dr);
                }
                string fileName = "";
                fileName = _hrDashboardService.CreateExcessEmployeeReportReportSheet(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult getMBWiseFilters()
        {
            JsonResult json = Json(_hrDashboardService.MBWisefiltersData(), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost, Authorize]
        public ActionResult MBWiseData(Dictionary<string, string> parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(_hrDashboardService.GetMBWiseSql(parameters));
                var jsondata = Json(new { NewData, Message = AplosMessage.Success });
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetMBWiseReportDataXls(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                if (data == null)
                {
                    throw new Exception("No Data found.");
                }
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;

                    dt.Columns.Add(item);
                }


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if ( item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;

                        dr[item] = data[i][item];
                    }

                    dt.Rows.Add(dr);
                }
                string fileName = "";
                fileName = GetMBWiseReport(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetMBWiseReport(DataTable data, string ReportHeader, string reportFileName)
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
                workbook.Worksheets[0].Name = "Data";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Division"; sheet[ROW, COL].ColumnWidth = 10; int colDivision = COL; COL++;
                sheet[ROW, COL].Text = "Entity"; sheet[ROW, COL].ColumnWidth = 10; int colEntity = COL; COL++;
                sheet[ROW, COL].Text = "Department"; sheet[ROW, COL].ColumnWidth = 15; int colDepartment = COL; COL++;
                sheet[ROW, COL].Text = "Section"; sheet[ROW, COL].ColumnWidth = 12; int colSection = COL; COL++;
                sheet[ROW, COL].Text = "SubSection"; sheet[ROW, COL].ColumnWidth = 13; int colSubSection = COL; COL++;
                sheet[ROW, COL].Text = "Designation"; sheet[ROW, COL].ColumnWidth = 10; int colDesignation = COL; COL++;
                sheet[ROW, COL].Text = "ShiftName"; sheet[ROW, COL].ColumnWidth = 17; int colShiftName = COL; COL++;
                sheet[ROW, COL].Text = "Line"; sheet[ROW, COL].ColumnWidth = 8; int colLine= COL; COL++;
                sheet[ROW, COL].Text = "Process"; sheet[ROW, COL].ColumnWidth = 11; int colProcess = COL; COL++;
                sheet[ROW, COL].Text = "BudgetCode"; sheet[ROW, COL].ColumnWidth = 10; int colCode= COL; COL++;
                sheet[ROW, COL].Text = "Budgeted"; sheet[ROW, COL].ColumnWidth = 9; int colBudgeted = COL; COL++;
                sheet[ROW, COL].Text = "OnRoll"; sheet[ROW, COL].ColumnWidth = 7; int colOnRoll = COL; COL++;
                sheet[ROW, COL].Text = "Deployment"; sheet[ROW, COL].ColumnWidth = 10; int colDeployment = COL; COL++;
                sheet[ROW, COL].Text = "Short"; sheet[ROW, COL].ColumnWidth = 6; int colShort = COL; COL++;
                sheet[ROW, COL].Text = "Excess"; sheet[ROW, COL].ColumnWidth = 7; int colExcess = COL; 

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
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, colDivision].Text = data.Rows[i]["Division"].ToString();
                    sheet[ROW, colEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, colDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, colSection].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, colSubSection].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, colDesignation].Text = data.Rows[i]["Designation"].ToString();
                    sheet[ROW, colShiftName].Text = data.Rows[i]["ShiftName"].ToString();
                    sheet[ROW, colLine].Text = data.Rows[i]["Line"].ToString();
                    sheet[ROW, colProcess].Text = data.Rows[i]["Process"].ToString();
                    sheet[ROW, colCode].Text = data.Rows[i]["Code"].ToString();
                    sheet[ROW, colBudgeted].Number = clsStaticInfo.dbl(data.Rows[i]["Budgeted"].ToString());
                    sheet[ROW, colOnRoll].Number = clsStaticInfo.dbl(data.Rows[i]["OnRoll"].ToString());
                    sheet[ROW, colDeployment].Number = clsStaticInfo.dbl(data.Rows[i]["Deployment"].ToString());
                    sheet[ROW, colShort].Number = clsStaticInfo.dbl(data.Rows[i]["Short"].ToString());
                    sheet[ROW, colExcess].Number = clsStaticInfo.dbl(data.Rows[i]["Excess"].ToString());
                    sheet.Range[ROW, colBudgeted, ROW, colExcess].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, colBudgeted, ROW, colExcess].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Manpower Budget Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;


                

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
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

    }
}