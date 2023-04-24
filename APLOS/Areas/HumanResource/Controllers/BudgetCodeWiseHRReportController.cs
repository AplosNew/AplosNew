using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Security.Core;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class BudgetCodeWiseHRReportController : Controller
    {
        private readonly SqlRepository _sqlRepository;
        public BudgetCodeWiseHRReportController()
        {
            _sqlRepository = new SqlRepository();
        }

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GetHRReportMasterList()
        {
            try
            {
                var sql = @"select Id Value, UserName Text from HKP.HRReportMaster";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetDataOnFavouriteFilter(string filterId)
        {
            try
            {
                string filter = "'" + filterId.Replace(",", "','") + "'";//replaced with ""

                var sql = @"select HRM.Id, E.UserName Entity, HRM.Active, HRM.Active isSelected, D.UserName Division, DT.UserName Department, S.UserName Section, SS.UserName SubSection, DSG.UserName Designation, A.UserName Activity,SDF.UserName [Shift], P.Code PositionCode
, P.UserName Position ,BGT.Code BudgetCode, BGT.Id ManpowerBudgetId, HRG.UserGroup, HRG.UserSubGroup, HRG.Grade
from HKP.HRReportMaster HR
left join [TRN].[HRReportMasterChild] HRM on HRM.HRReportMasterId = HR.Id
left join HKP.HRReportGroupMaster HRG on HRG.Id = HRM.UserGroupId
left join MST.ManpowerBudget BGT on BGT.Id = HRM.ManpowerBudgetId 
left join ORG.Entity E on E.Id = BGT.EntityId
left join MST.BudgetMasterActivity BMA on BGT.ROBudgetCode = BMA.BudgetMasterId
left join HKP.Activity A on BMA.ActivityId = A.Id
left join dbo.ShiftDefination SDF on BGT.ShiftDefinationId = SDF.SystemID
left join ORG.Position P on BGT.PositionId = P.Id
left join ORG.Division D on P.DivisionId  = D.Id
left join ORG.Department DT on P.DepartmentId = DT.Id
left join ORG.Section S on P.SectionId = S.Id
left join ORG.SubSection SS on P.SubSectionId = SS.Id
left join ORG.Division DSN on P.DivisionId = DSN.Id
left join HKP.Designation DSG ON P.DesignationId = DSG.Id
where HR.Id in (" + filter + ")";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetProcess()
        {
            try
            {
                var sql = @"select Id Value, UserName Text from HKP.Process order by Text ASC";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetBudgetCodeWiseReport()
        {
            var sql = @"select distinct EMP.SystemID,EMP.EmployeeCode EMPCode, EMP.EmployeeName EmployeeName, FORMAT(EMP.DOJ, 'dd-MMM-yyyy')DOJ, FORMAT(EMP.DOS, 'dd-MMM-yyyy') DOS, EMP.EmployeeStatus Status ,EMP.EmployeeCurrentStatus CurrentStatus, 
                UN.UserName Entity, DP.StandardName Department, SC.Sequence SecSeq, SC.StandardName Section, SBC.Sequence SubScSeq,SBC.StandardName SubSection, 
                DSG.Sequence DesgSeq, DSG.StandardName Designation, Activity, GDSG.StandardName GivenDesignation,x.StandardName Category, POS.UserReportGroup, 
                POS.Code PositionCode, MBGT.Code BudgetCode, sd.ShiftDefinationName Shift,ST.StandardName [State], emp.CellPhnNo MobileNo,(select top 1 WOH.STANDARDNAME from EmployeeWeeklyOff wo
                left join WeekOffHeader WOH on WOH.Id = WO.WOHeaderId
                where wo.EmpSystemID = emp.SystemId
                order by effectivedate desc) as WeekOff, RG.StandardName Residence, RM.ResidenceNumber ,TG.StandardName Transport,POS.UserName Position, PG.UserName PositionGroup,
                (select TOP 1 APD.InStatus from AttdnProcessData APD where APD.EmpSystemID = Emp.SystemId ORDER BY APD.InStatus DESC
                ) as [InStatus], (select TOP 1 APD.DayStatus from AttdnProcessData APD where APD.EmpSystemID = Emp.SystemId ORDER BY APD.DayStatus DESC
                ) as [DayStatus]
                from EmployeeInformation EMP --AttdnProcessData APD  
                --left join EmployeeInformation EMP on EMP.SystemId = APD.EmpSystemID --and APD.EmpSystemID in (EMP.SystemId)
                LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                LEFT JOIN HKP.Process PRC on PRC.Id = POS.ProcessId
                LEFT JOIN HKP.ProcessGroup PG on PG.Id = PRC.ProcessGroupId
                left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                left join ORG.Entity UN on UN.Id = MBGT.EntityId
                left join ORG.Department DP on DP.ID = POS.DepartmentId
                left join ORG.Section SC on SC.Id = POS.SectionId
                left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                LEFT JOIN hkp.Designation DSG on DSG.id = POS.DesignationId
                LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                LEFT JOIN MST.DesignationMasterLegalDesignation DMLD on DMLD.LegalDesignationId = GDSG.Id
                left join mst.DesignationMaster dm on dm.Id = DMLD.DesignationMasterId
                left join scs.designationmasterconfiguration dmc on dmc.designationmasterid = dm.id
                LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=dm.DesignationGroupId
                left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
                left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
                left join ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId
                LEFT JOIN ResidenceAllocatedEmployees RAE on RAE.EmployeeSystemId = EMP.SystemId and RAE.isOccupied = 1
                LEFT JOIN ResidenceMaster RM on RM.Id = RAE.ResidenceId
                left join TransportGroup TG on TG.Id = EMP.TransportGroupId
                left join employeecodetype ect on ect.id = emp.employeecodetypeid
                left join hkp.Process PR on PR.Id = POS.ProcessId
                left join scs.District DT on DT.Id = emp.ParmDistrictID
                left join scs.[State] ST on ST.Id = EMP.ParmStateId


                where emp.employeecode is not null --and emp.employeestatus = 'Active' 
                and emp.employeecode NOT IN (2222229, 2222230)";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        private string GetDate(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            try
            {
                return Convert.ToDateTime(s).ToString("dd-MMM-yyyy");
            }
            catch (Exception)
            {
                return "";
            }
        }


        [HttpPost, Authorize]
        public ActionResult ProductionDataXls(List<Dictionary<string, object>> data, string reportFileName)
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
                fileName = HRReportMasterDataReport(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string HRReportMasterDataReport(DataTable data, string ReportHeader, string reportFileName)
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
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[1].Name = "POData";
                sheet = workbook.Worksheets[1];

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;

                int colstart = COL;
                sheet[ROW, COL].Text = "SubSecSeq";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSubSecSeq = COL;
                COL++;

                
                sheet[ROW, COL].Text = "SubSec";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSubSec = COL;
                COL++;

                sheet[ROW, COL].Text = "Activity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colActivity = COL;
                COL++;

                sheet[ROW, COL].Text = "Shift";
                sheet[ROW, COL].ColumnWidth = 16;
                int colShift = COL;
                COL++;

                sheet[ROW, COL].Text = "BgtCode";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBgtCode = COL;
                COL++;

                sheet[ROW, COL].Text = "In Missing";
                sheet[ROW, COL].ColumnWidth = 80;
                int colInMissing = COL;
                COL++;

                sheet[ROW, COL].Text = "WeekOff";
                sheet[ROW, COL].ColumnWidth = 80;
                int colWeekOff = COL;
                COL++;

                sheet[ROW, COL].Text = "Deployment";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDeployment = COL;
                COL++;

                sheet[ROW, COL].Text = "MPBudget";
                sheet[ROW, COL].ColumnWidth = 16;
                int colMPBudget = COL;
                COL++;

                sheet[ROW, COL].Text = "IN";
                sheet[ROW, COL].ColumnWidth = 16;
                int colIN = COL;
                COL++;

                sheet[ROW, COL].Text = "IM";
                sheet[ROW, COL].ColumnWidth = 16;
                int colIM = COL;
                COL++;

                sheet[ROW, COL].Text = "WeeklyOff";
                sheet[ROW, COL].ColumnWidth = 16;
                int colWeekyOff = COL;
                COL++;

                sheet[ROW, COL].Text = "OnRoll / Short Access";
                sheet[ROW, COL].ColumnWidth = 16;
                int colOnRoleShortAccess = COL;
                COL++;

                sheet[ROW, COL].Text = "Actual Short / Excess";
                sheet[ROW, COL].ColumnWidth = 16;
                int colActualShortExcess = COL;
                COL++;


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
                    sheet[ROW, colEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, colSubSecSeq].Text = data.Rows[i]["SubSecSeq"].ToString();
                    sheet[ROW, colSubSec].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, colActivity].Text = data.Rows[i]["Activity"].ToString();
                    sheet[ROW, colShift].Text = data.Rows[i]["Shift"].ToString();
                    sheet[ROW, colBgtCode].Text = data.Rows[i]["BudgetCode"].ToString();
                    sheet[ROW, colWeekOff].Text = data.Rows[i]["WeekOff"].ToString();
                    sheet[ROW, colDeployment].Text = data.Rows[i]["Deployment"].ToString();
                    sheet[ROW, colMPBudget].Text = data.Rows[i]["MPBudget"].ToString();
                    sheet[ROW, colIN].Text = data.Rows[i]["IN"].ToString();
                    sheet[ROW, colIM].Text = data.Rows[i]["IM"].ToString();
                    sheet[ROW, colWeekyOff].Text = data.Rows[i]["WeeklyOff"].ToString();
                    sheet[ROW, colOnRoleShortAccess].Text = data.Rows[i]["OnRoleShortAccess"].ToString();
                    sheet[ROW, colActualShortExcess].Text = data.Rows[i]["ActualShortExcess"].ToString();
                    

                  

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "PO Wise Production Status Report", identity.PlantId);
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

                #region Pivot

                string fPath = fPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "PO Wise Production Status Report" + identity.UserId + ".xlsx";

                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }

                workbook.Worksheets[0].Name = "HRReportMaster";

                IWorksheet pivotSheet = workbook.Worksheets[0];
                IPivotCache cache = workbook.PivotCaches.Add(workbook.Worksheets[1][startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);

               

                pivotTable.Fields[colSubSecSeq - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSubSec - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colActivity - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colShift - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBgtCode - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colWeekOff - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDeployment - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colMPBudget - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colIN - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colIM - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colWeekyOff - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOnRoleShortAccess - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colActualShortExcess - 1].Axis = PivotAxisTypes.Column;


                //IPivotField field = pivotTable.Fields[colEntity - 1];
                //field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                //pivotTable.DataFields.Add(field, "ActualQty", PivotSubtotalTypes.None);

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    //if (i == colProcess - 1 || i == colEntity - 1 || i == colWorkCenter - 1)
                    //    continue;
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }

                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "HR Report Master", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
                workbook.Worksheets[0].UsedRange["A7"].FreezePanes();


                #endregion Buyer Summary

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