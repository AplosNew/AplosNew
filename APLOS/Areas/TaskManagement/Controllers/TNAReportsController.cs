using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System.Data;
using Syncfusion.XlsIO;
using System.Web;
using System.IO;
using Library.Service.Helpers;
using OTSBD;
using System.Linq;
using Library.Service.Enums;
using System.Collections.Specialized;

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class TNAReportsController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;


        public TNAReportsController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion


        [HttpPost, Authorize]
        public ActionResult GetList(Dictionary<string, object> filterSettings)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string DueDate = "TT.OriginalSequentialEndDate";//or ATO.DueDate

            string filter = " ";
            if (filterSettings["ActiveStatus"].ToString() != "All")
            {
                if (filterSettings["ActiveStatus"].ToString() == "Closed")
                    filter += " AND TM.CurrentStatus='" + filterSettings["ActiveStatus"].ToString() + "'";
                else
                    filter += " AND isnull(TM.CurrentStatus,'')<>'" + filterSettings["ActiveStatus"].ToString() + "'";
            }
            if (filterSettings["DateSelection"].ToString() != "WITHOUTDATE")
            {
                if (filterSettings["DateSelection"].ToString() == "WITHDATE")
                {
                    if (filterSettings["ActiveStatus"].ToString() == "Closed")
                        filter += " AND TM.ClosingDate between '" + filterSettings["FromDate"].ToString() + "' AND '" + filterSettings["ToDate"].ToString() + "'";
                    else
                        filter += " AND " + DueDate + " between '" + filterSettings["FromDate"].ToString() + "' AND '" + filterSettings["ToDate"].ToString() + "'";

                }
                else if (filterSettings["DateSelection"].ToString() == "WITHOUTDATE")
                {
                    if (filterSettings["ActiveStatus"].ToString() == "Closed")
                        filter += " AND TM.ClosingDate<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";
                    else
                        filter += " AND " + DueDate + "<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";

                }
            }

            string sql = @"SELECT * FROM (SELECT DISTINCT
                                t.ProcessId,MO.BuyerId, MO.Buyer,   MO.MasterOrderId, isnull(MO.StyleNo,'') AS StyleNo, isnull(MO.SONo,'') AS SONo, isnull(MO.PRNo,'') AS PRNo,
                                pr.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,
                                isnull(T.TaskCategoryId,'')TaskCategoryId,isnull(T.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                p.UserName AS ProcessName,
                                isnull(DTO.UserName,'') AS Department,isnull(EATO.EmployeeName,'') AS AssignToEmployeeName,isnull(EAB.EmployeeName,'') AS AssignByEmployeeName,
                                isnull(tc.UserName,'') AS TaskCategory,isnull(tsc.UserName,'') AS TaskSubCategory
                                ,LineItemReference
                                 FROM TaskManagerMaster AS tm
                                inner join (" + TNAOrderColumns() + @") AS MO on TaskManagerMasterId=Tm.Id
                                INNER JOIN TNATasks AS TT ON TT.Id=tm.TNATasksId
                                LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
                                LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'

                                LEFT OUTER JOIN EmployeeInformation AS EAB ON eab.SystemId=ab.ResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId

                                LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						        LEFT JOIN ORG.Position pr ON pr.Id=MB.PositionId
								LEFT OUTER JOIN org.Department AS DTO ON dto.Id=pr.DepartmentId

                                LEFT OUTER JOIN MasterOrderTaskTemplate AS mott ON mott.Id=MO.TaskTemplateId
                                LEFT OUTER JOIN TaskMaster AS T ON t.Id=mott.TaskMasterId

                                INNER JOIN hkp.TaskCategory AS tc ON T.TaskCategoryId=tc.Id AND TC.Active=1
                                INNER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id=T.TaskSubCategoryId and tsc.Active=1

                                LEFT OUTER JOIN hkp.Process AS p ON p.Id=t.ProcessId
                                               WHERE eab.PlantId='" + identity.PlantId + @"' " + filter + @"
                                ) AS K ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

         //   return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetResult(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataTable dtFinal = new DataTable();
            DataTable dtColumnList = new DataTable();


            makeDataTable(Filter, FilterFields, out dtFinal, out dtColumnList);


            return Json(new { MAINDATA = CustomJsonResultService.DataTableToJson(dtFinal), COLUMNS = CustomJsonResultService.DataTableToJson(dtColumnList) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetExcelReport(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataTable dtFinal = new DataTable();
                DataTable dtColumnList = new DataTable();
                ExcelEngine excelEngine = new ExcelEngine();


                IWorkbook workbook = makeExcel(Filter, FilterFields, out dtFinal, out dtColumnList);


                workbook.Version = ExcelVersion.Excel2013;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "TNA.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost, Authorize]
        public ActionResult GetExcelTasksReport(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = GetTNAStatusReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, identity.EmployeeId, identity.Name, Filter, FilterFields);

                workbook.Version = ExcelVersion.Excel2016;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "TNA Reports.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost, Authorize]
        public ActionResult GetExcelTasksReportException(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = GetTNAStatusReportException(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, identity.EmployeeId, identity.Name, Filter, FilterFields);

                workbook.Version = ExcelVersion.Excel2013;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "TNA Reports.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost, Authorize]
        public ActionResult GetTaskListResult(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
        {
            DataTable dtFinal = new DataTable();


            GetTNAStatusReportsData(out dtFinal, Filter, FilterFields);

            var jsondata = Json(new { MAINDATA = CustomJsonResultService.DataTableToJson(dtFinal) }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        private void makeDataTable(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields, out DataTable dtFinal, out DataTable dsMasterOrderTaskMasters)
        {
            dtFinal = new DataTable();
            dsMasterOrderTaskMasters = new DataTable();
            try
            {
                DataTable dtMasterOrderTasks = new DataTable();

                MasterOrderDataTables(Filter, FilterFields, out dtMasterOrderTasks, out dsMasterOrderTaskMasters);



                string id = "";
                Dictionary<string, List<DataRow>> TaskList = new Dictionary<string, List<DataRow>>();
                List<DataRow> Data = new List<DataRow>();
                for (int i = 0; i < dtMasterOrderTasks.Rows.Count; i++)
                {
                    if (id != dtMasterOrderTasks.Rows[i]["KEY"].ToString())
                    {
                        Data = new List<DataRow>();
                        TaskList.Add(dtMasterOrderTasks.Rows[i]["KEY"].ToString(), Data);

                    }

                    Data.Add(dtMasterOrderTasks.Rows[i]);

                    id = dtMasterOrderTasks.Rows[i]["KEY"].ToString();
                }


                //preparing the final structure
                dtFinal = dtMasterOrderTasks.Clone();
                Dictionary<string, int> dicColIndex = new Dictionary<string, int>();
                for (int i = 0; i < dsMasterOrderTaskMasters.Rows.Count; i++)
                {
                    try
                    {
                        dtFinal.Columns.Add("" + dsMasterOrderTaskMasters.Rows[i]["Id"].ToString() + "");
                        dicColIndex.Add(dsMasterOrderTaskMasters.Rows[i]["Id"].ToString(), dtFinal.Columns.Count - 1);

                    }
                    catch (Exception ex)
                    {


                    }
                }

                //plotting data
                foreach (var item in TaskList.Keys)
                {
                    try
                    {
                        DataRow dr = dtFinal.NewRow();
                        foreach (DataColumn col in TaskList[item][0].Table.Columns)
                            dr[col.ColumnName] = clsStaticInfo.nullrecorder(TaskList[item][0][col.ColumnName].ToString());//copying existing single data

                        List<DataRow> TotalRows = TaskList[item];
                        for (int i = 0; i < TotalRows.Count; i++)
                        {
                            dr[dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]] = clsStaticInfo.nullrecorder(dr[dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]].ToString() + " " + TotalRows[i]["DueDate"].ToString());
                        }

                        dtFinal.Rows.Add(dr);
                    }
                    catch (Exception ex)
                    {


                    }

                }
            }
            catch (Exception ex)
            {

            }

        }
        private IWorkbook makeExcel(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields, out DataTable dtFinal, out DataTable dsMasterOrderTaskMasters)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;


            dtFinal = new DataTable();
            dsMasterOrderTaskMasters = new DataTable();
            try
            {
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[0].Name = "TNA";
                sheet = workbook.Worksheets[0];

                IFont TNAFontRed = workbook.CreateFont();
                IFont TNAFontGreen = workbook.CreateFont();
                IFont TNAFontBlue = workbook.CreateFont();
                IFont TNAFontOrange = workbook.CreateFont();
                IFont TNAFontBlueViolet = workbook.CreateFont();
                IFont TNAFont200200000 = workbook.CreateFont();


                DataTable dtMasterOrderTasks = new DataTable();

                MasterOrderDataTables(Filter, FilterFields, out dtMasterOrderTasks, out dsMasterOrderTaskMasters);

                if (dtMasterOrderTasks.Rows.Count == 0)
                    throw new Exception("No data found");

                int ROW = 1; int COL = 1;

                #region Legends

                int LeftCol = 1; int RightCol = 4;

                //closed in due time

                colorTextExcelCell(sheet[ROW, LeftCol], TNAFontGreen, "●", System.Drawing.Color.Green);
                sheet[ROW, LeftCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, LeftCol + 1].Text = "Completed on Due Time";
                sheet[ROW, LeftCol + 1, ROW, LeftCol + 2].Merge();


                colorTextExcelCell(sheet[ROW, RightCol], TNAFontBlue, "●", System.Drawing.Color.Blue);
                sheet[ROW, RightCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, RightCol + 1].Text = "Completed after Due Time";
                sheet[ROW, RightCol + 1, ROW, RightCol + 2].Merge();


                sheet[ROW, COL].RowHeight = 24;
                ROW++;
                colorTextExcelCell(sheet[ROW, LeftCol], TNAFontOrange, "●", System.Drawing.Color.Orange);
                sheet[ROW, LeftCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, LeftCol + 1].Text = "Due";
                sheet[ROW, LeftCol + 1, ROW, LeftCol + 2].Merge();


                colorTextExcelCell(sheet[ROW, RightCol], TNAFontRed, "●", System.Drawing.Color.Red);
                sheet[ROW, RightCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, RightCol + 1].Text = "Over Due";
                sheet[ROW, RightCol + 1, ROW, RightCol + 2].Merge();


                sheet[ROW, COL].RowHeight = 24;
                sheet[ROW - 1, LeftCol, ROW, RightCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                ROW++;
                colorTextExcelCell(sheet[ROW, LeftCol], TNAFontBlueViolet, "●", System.Drawing.Color.BlueViolet);
                sheet[ROW, LeftCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, LeftCol + 1].Text = "Task for Today";
                sheet[ROW, LeftCol + 1, ROW, LeftCol + 2].Merge();


                colorTextExcelCell(sheet[ROW, RightCol], TNAFont200200000, "■", System.Drawing.Color.FromArgb(200, 200, 0));
                sheet[ROW, RightCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, RightCol + 1].Text = "Milestone";
                sheet[ROW, RightCol + 1, ROW, RightCol + 2].Merge();

                sheet[ROW, COL].RowHeight = 24;
                sheet[ROW - 1, LeftCol, ROW, RightCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                #endregion Legends



                ROW = 5; COL = 1;

                #region columns


                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 14;
                int colBuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "Order#";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOrder = COL;
                COL++;
                sheet[ROW, COL].Text = "Type";
                sheet[ROW, COL].ColumnWidth = 12;
                int colType = COL;
                COL++;
                sheet[ROW, COL].Text = "Style";
                sheet[ROW, COL].ColumnWidth = 10;
                int colStyle = COL;
                COL++;
                sheet[ROW, COL].Text = "SO#";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSO = COL;
                COL++;
                sheet[ROW, COL].Text = "Line Item Ref#";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLIR = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order#";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionOrder = COL;

                sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(255, 242, 204);
                sheet.Range[ROW, 1, ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                int EndOfStaticColumns = COL;

                string id = "";
                Dictionary<string, List<DataRow>> TaskList = new Dictionary<string, List<DataRow>>();
                List<DataRow> Data = new List<DataRow>();
                for (int i = 0; i < dtMasterOrderTasks.Rows.Count; i++)
                {
                    if (id != dtMasterOrderTasks.Rows[i]["KEY"].ToString())
                    {
                        Data = new List<DataRow>();
                        TaskList.Add(dtMasterOrderTasks.Rows[i]["KEY"].ToString(), Data);

                    }

                    Data.Add(dtMasterOrderTasks.Rows[i]);

                    id = dtMasterOrderTasks.Rows[i]["KEY"].ToString();
                }


                //preparing the final structure
                dtFinal = dtMasterOrderTasks.Clone();
                Dictionary<string, int> dicColIndex = new Dictionary<string, int>();
                int StartCollapseCol = COL + 1;
                string CatId = "";
                for (int i = 0; i < dsMasterOrderTaskMasters.Rows.Count; i++)
                {
                    try
                    {

                        if (CatId != dsMasterOrderTaskMasters.Rows[i]["TaskSubCategoryId"].ToString())
                        {
                            if (i > 0)
                            {
                                try
                                {

                                    //sheet[2, StartCollapseCol, 2, COL].Group(ExcelGroupBy.ByColumns);
                                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].Text = dsMasterOrderTaskMasters.Rows[i - 1]["TaskSubCategory"].ToString();
                                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].CellStyle.Font.Bold = true;
                                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].CellStyle.Interior.Color = System.Drawing.Color.LightGoldenrodYellow;
                                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);

                                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].Merge();



                                    StartCollapseCol = COL + 1;
                                }
                                catch (Exception ex)
                                {

                                }


                            }




                        }
                        COL++;

                        dtFinal.Columns.Add("" + dsMasterOrderTaskMasters.Rows[i]["Id"].ToString() + "");
                        dicColIndex.Add(dsMasterOrderTaskMasters.Rows[i]["Id"].ToString(), COL);

                        sheet[ROW, COL].Text = dsMasterOrderTaskMasters.Rows[i]["UserDefineTask"].ToString();
                        sheet[ROW, COL].ColumnWidth = 8;
                        sheet[ROW, COL].RowHeight = 64;
                        sheet[ROW, COL].CellStyle.Rotation = 90;
                        sheet[ROW, COL].WrapText = true;
                        sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (bplib.clsWebLib.GetBoolData(dsMasterOrderTaskMasters.Rows[i]["IsTaskMilestone"].ToString()) == true)
                            sheet.Range[ROW, COL].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(200, 200, 0);
                        else
                            sheet.Range[ROW, COL].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(255, 242, 204);


                    }
                    catch (Exception ex)
                    {


                    }
                    CatId = dsMasterOrderTaskMasters.Rows[i]["TaskSubCategoryId"].ToString();
                }

                try
                {
                    //sheet[2, StartCollapseCol, 2, COL].Group(ExcelGroupBy.ByColumns);
                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].Text = dsMasterOrderTaskMasters.Rows[dsMasterOrderTaskMasters.Rows.Count - 1]["TaskSubCategory"].ToString();
                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].CellStyle.Font.Bold = true;
                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].CellStyle.Interior.Color = System.Drawing.Color.LightGoldenrodYellow;
                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);


                    if (bplib.clsWebLib.GetBoolData(dsMasterOrderTaskMasters.Rows[dsMasterOrderTaskMasters.Rows.Count - 1]["IsTaskMilestone"].ToString()) == true)
                        sheet.Range[ROW, COL].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(200, 200, 0);
                    else
                        sheet.Range[ROW, COL].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(255, 242, 204);

                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].Merge();

                }
                catch (Exception ex)
                {

                }
                #endregion columns

                int endCol = COL;



                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, EndOfStaticColumns + 1, ROW, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, EndOfStaticColumns + 1, ROW, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, 1, ROW, endCol].RowHeight = 80;
                sheet.Range[ROW - 1, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                ROW++;

                //plotting data
                int StartRow = ROW;

                //Add rich-text Excel comment
                IFont fontCaption = workbook.CreateFont();
                fontCaption.Bold = true;
                IFont fontRegular = workbook.CreateFont();
                fontRegular.Bold = false;

                foreach (var item in TaskList.Keys)
                {
                    try
                    {


                        sheet[ROW, colBuyer].Text = clsStaticInfo.nullrecorder(TaskList[item][0]["Buyer"].ToString());
                        sheet[ROW, colType].Text = clsStaticInfo.nullrecorder(TaskList[item][0]["TNAType"].ToString());
                        sheet[ROW, colOrder].Text = clsStaticInfo.nullrecorder(TaskList[item][0]["MasterOrderId"].ToString());
                        sheet[ROW, colStyle].Text = clsStaticInfo.nullrecorder(TaskList[item][0]["StyleNo"].ToString());
                        sheet[ROW, colSO].Text = clsStaticInfo.nullrecorder(TaskList[item][0]["SONo"].ToString());
                        sheet[ROW, colLIR].Text = clsStaticInfo.nullrecorder(TaskList[item][0]["LineItemReference"].ToString());
                        sheet[ROW, colProductionOrder].Text = clsStaticInfo.nullrecorder(TaskList[item][0]["PRNo"].ToString());




                        List<DataRow> TotalRows = TaskList[item];
                        for (int i = 0; i < TotalRows.Count; i++)
                        {
                            //sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]].Text += "●";

                            DateTime dtDueDate = Convert.ToDateTime(TotalRows[i]["DueDate"].ToString());


                            if (TotalRows[i]["ClosingDate"].ToString() != "")
                            {
                                DateTime ClosingDate = Convert.ToDateTime(TotalRows[i]["ClosingDate"].ToString());

                                //closed in due time
                                if (dtDueDate >= ClosingDate && TotalRows[i]["ClosingDate"].ToString() != "")
                                    colorTextExcelCell(sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]], TNAFontGreen, "●", System.Drawing.Color.Green);

                                //closed but exceeding due time
                                if (dtDueDate < ClosingDate && TotalRows[i]["ClosingDate"].ToString() != "")
                                    colorTextExcelCell(sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]], TNAFontBlue, "●", System.Drawing.Color.Blue);

                                sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(200, 255, 200);
                            }

                            //task for today
                            if (dtDueDate == Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")) && TotalRows[i]["ClosingDate"].ToString() == "")
                                colorTextExcelCell(sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]], TNAFontBlueViolet, "●", System.Drawing.Color.BlueViolet);


                            //future due date & not completed
                            if (dtDueDate > Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")) && TotalRows[i]["ClosingDate"].ToString() == "")
                                colorTextExcelCell(sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]], TNAFontOrange, "●", System.Drawing.Color.Orange);


                            //due date exceeding and not completed
                            if (dtDueDate < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")) && TotalRows[i]["ClosingDate"].ToString() == "")
                                colorTextExcelCell(sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]], TNAFontRed, "●", System.Drawing.Color.Red);

                            sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]].CellStyle.Font.Size = 30f;
                            sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]].VerticalAlignment = ExcelVAlign.VAlignCenter;



                            IRange range = sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]];

                            ICommentShape shape = range.AddComment();

                            shape.RichText.Append(string.Format("Original Start Date:{0}", TotalRows[i]["OriginalSequentialStartDate"].ToString()) + Environment.NewLine, fontCaption);
                            shape.RichText.Append(string.Format("Original End Date:{0} ", TotalRows[i]["OriginalSequentialEndDate"].ToString()) + Environment.NewLine, fontCaption);

                            shape.RichText.Append("Assigned To" + Environment.NewLine, fontCaption);
                            shape.RichText.Append(TotalRows[i]["AssignTo"].ToString() + Environment.NewLine, fontRegular);

                            shape.RichText.Append("Expected Due Date" + Environment.NewLine, fontCaption);
                            shape.RichText.Append(TotalRows[i]["TempEndDate"].ToString() + Environment.NewLine, fontRegular);

                            shape.RichText.Append("Commitment Date" + Environment.NewLine, fontCaption);
                            shape.RichText.Append(TotalRows[i]["CommitmentDate"].ToString() + Environment.NewLine + Environment.NewLine, fontRegular);

                            shape.RichText.Append("Completion Date" + Environment.NewLine, fontCaption);
                            shape.RichText.Append(TotalRows[i]["ClosingDate"].ToString() + Environment.NewLine + Environment.NewLine, fontRegular);


                            shape.IsTextLocked = false;
                            shape.AutoSize = false;

                            shape.Height = 400;
                            shape.Width = 300;
                        }

                        sheet.Range[ROW, 1, ROW, endCol].RowHeight = 24;
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        ROW++;
                    }
                    catch (Exception ex)
                    {


                    }

                }

                sheet.Range[StartRow, 1, ROW, EndOfStaticColumns].CellStyle.Font.Size = 8f;
                sheet.Range[StartRow, 1, ROW, EndOfStaticColumns].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.IsGridLinesVisible = true;
                sheet.UsedRange[clsStaticInfo.GetxlsCol(EndOfStaticColumns + 1) + "6"].FreezePanes();


                #region filter
                sheet.AutoFilters.FilterRange = sheet.Range[StartRow - 1, 1, ROW, EndOfStaticColumns];
                //Column index to which auto-filter must be applied
                if (Filter["ReportLevel"].ToString() == "ALL")
                {
                    IAutoFilter filter = sheet.AutoFilters[colType - 1];
                    filter.AddTextFilter("Order");

                    //Specify first condition
                    IAutoFilterCondition firstCondition = filter.FirstCondition;
                    firstCondition.ConditionOperator = ExcelFilterCondition.Contains;
                    firstCondition.String = "Order";
                }

                //Auto fit the second column
                //sheet.Range[StartRow - 1, 1, ROW, EndOfStaticColumns].EntireColumn.AutofitColumns();

                #endregion filter

                sheet.Range[StartRow, 1, ROW, EndOfStaticColumns].WrapText = true;
                makeExcelSheet2(dtMasterOrderTasks, dsMasterOrderTaskMasters, workbook.Worksheets[1], fontCaption, fontRegular);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return workbook;
        }
        private void makeExcelSheet2(DataTable dtMasterOrderTasks, DataTable dsMasterOrderTaskMasters, IWorksheet sheet, IFont fontCaption, IFont fontRegular)
        {

            try
            {
                sheet.Name = "TNA-With Date";


                int ROW = 1; int COL = 1;

                #region Legends

                int LeftCol = 1; int RightCol = 4;

                //closed in due time
                IFont TNAFontRed = sheet.Workbook.CreateFont();
                IFont TNAFontGreen = sheet.Workbook.CreateFont();
                IFont TNAFontBlue = sheet.Workbook.CreateFont();
                IFont TNAFontOrange = sheet.Workbook.CreateFont();
                IFont TNAFontBlueViolet = sheet.Workbook.CreateFont();
                IFont TNAFont200200000 = sheet.Workbook.CreateFont();


                colorTextExcelCell(sheet[ROW, LeftCol], TNAFontGreen, "●", System.Drawing.Color.Green);
                sheet[ROW, LeftCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, LeftCol + 1].Text = "Completed on Due Time";
                sheet[ROW, LeftCol + 1, ROW, LeftCol + 2].Merge();


                colorTextExcelCell(sheet[ROW, RightCol], TNAFontBlue, "●", System.Drawing.Color.Blue);
                sheet[ROW, RightCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, RightCol + 1].Text = "Completed after Due Time";
                sheet[ROW, RightCol + 1, ROW, RightCol + 2].Merge();


                sheet[ROW, COL].RowHeight = 24;
                ROW++;
                colorTextExcelCell(sheet[ROW, LeftCol], TNAFontOrange, "●", System.Drawing.Color.Orange);
                sheet[ROW, LeftCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, LeftCol + 1].Text = "Due";
                sheet[ROW, LeftCol + 1, ROW, LeftCol + 2].Merge();


                colorTextExcelCell(sheet[ROW, RightCol], TNAFontRed, "●", System.Drawing.Color.Red);
                sheet[ROW, RightCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, RightCol + 1].Text = "Over Due";
                sheet[ROW, RightCol + 1, ROW, RightCol + 2].Merge();


                sheet[ROW, COL].RowHeight = 24;
                sheet[ROW - 1, LeftCol, ROW, RightCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

                colorTextExcelCell(sheet[ROW, LeftCol], TNAFontBlueViolet, "●", System.Drawing.Color.BlueViolet);
                sheet[ROW, LeftCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, LeftCol + 1].Text = "Task for Today";
                sheet[ROW, LeftCol + 1, ROW, LeftCol + 2].Merge();

                colorTextExcelCell(sheet[ROW, RightCol], TNAFont200200000, "■", System.Drawing.Color.FromArgb(200, 200, 0));
                sheet[ROW, RightCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, RightCol + 1].Text = "Milestone";
                sheet[ROW, RightCol + 1, ROW, RightCol + 2].Merge();

                sheet[ROW, COL].RowHeight = 24;
                sheet[ROW - 1, LeftCol, ROW, RightCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

                #endregion Legends



                ROW = 5; COL = 1;
                #region columns


                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 14;
                int colBuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "Order#";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOrder = COL;
                COL++;
                sheet[ROW, COL].Text = "Type";
                sheet[ROW, COL].ColumnWidth = 12;
                int colType = COL;
                COL++;
                sheet[ROW, COL].Text = "Style";
                sheet[ROW, COL].ColumnWidth = 10;
                int colStyle = COL;
                COL++;
                sheet[ROW, COL].Text = "SO#";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSO = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order#";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionOrder = COL;

                sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(255, 242, 204);
                sheet.Range[ROW, 1, ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                int EndOfStaticColumns = COL;

                string id = "";
                Dictionary<string, List<DataRow>> TaskList = new Dictionary<string, List<DataRow>>();
                List<DataRow> Data = new List<DataRow>();
                for (int i = 0; i < dtMasterOrderTasks.Rows.Count; i++)
                {
                    if (id != dtMasterOrderTasks.Rows[i]["KEY"].ToString())
                    {
                        Data = new List<DataRow>();
                        TaskList.Add(dtMasterOrderTasks.Rows[i]["KEY"].ToString(), Data);

                    }

                    Data.Add(dtMasterOrderTasks.Rows[i]);

                    id = dtMasterOrderTasks.Rows[i]["KEY"].ToString();
                }


                //preparing the final structure
                DataTable dtFinal = dtMasterOrderTasks.Clone();
                Dictionary<string, int> dicColIndex = new Dictionary<string, int>();
                int StartCollapseCol = COL + 1;
                string CatId = "";
                for (int i = 0; i < dsMasterOrderTaskMasters.Rows.Count; i++)
                {
                    try
                    {

                        if (CatId != dsMasterOrderTaskMasters.Rows[i]["TaskSubCategoryId"].ToString())
                        {
                            if (i > 0)
                            {
                                try
                                {


                                    //sheet[2, StartCollapseCol, 2, COL].Group(ExcelGroupBy.ByColumns);
                                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].Text = dsMasterOrderTaskMasters.Rows[i - 1]["TaskSubCategory"].ToString();
                                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].CellStyle.Font.Bold = true;
                                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].CellStyle.Interior.Color = System.Drawing.Color.LightGoldenrodYellow;
                                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);

                                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].Merge();


                                    StartCollapseCol = COL + 1;
                                }
                                catch (Exception ex)
                                {

                                }


                            }




                        }
                        COL++;



                        dtFinal.Columns.Add("" + dsMasterOrderTaskMasters.Rows[i]["Id"].ToString() + "");
                        dicColIndex.Add(dsMasterOrderTaskMasters.Rows[i]["Id"].ToString(), COL);

                        sheet[ROW, COL].Text = dsMasterOrderTaskMasters.Rows[i]["UserDefineTask"].ToString();
                        sheet[ROW, COL].ColumnWidth = 8;
                        sheet[ROW, COL].RowHeight = 64;
                        sheet[ROW, COL].CellStyle.Rotation = 90;
                        sheet[ROW, COL].WrapText = true;
                        sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (bplib.clsWebLib.GetBoolData(dsMasterOrderTaskMasters.Rows[i]["IsTaskMilestone"].ToString()) == true)
                            sheet.Range[ROW, COL].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(200, 200, 0);
                        else
                            sheet.Range[ROW, COL].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(255, 242, 204);


                    }
                    catch (Exception ex)
                    {


                    }
                    CatId = dsMasterOrderTaskMasters.Rows[i]["TaskSubCategoryId"].ToString();
                }

                try
                {
                    //sheet[2, StartCollapseCol, 2, COL].Group(ExcelGroupBy.ByColumns);
                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].Text = dsMasterOrderTaskMasters.Rows[dsMasterOrderTaskMasters.Rows.Count - 1]["TaskSubCategory"].ToString();
                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].CellStyle.Font.Bold = true;
                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].CellStyle.Interior.Color = System.Drawing.Color.LightGoldenrodYellow;
                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);

                    sheet[ROW - 1, StartCollapseCol, ROW - 1, COL].Merge();


                    if (bplib.clsWebLib.GetBoolData(dsMasterOrderTaskMasters.Rows[dsMasterOrderTaskMasters.Rows.Count - 1]["IsTaskMilestone"].ToString()) == true)
                        sheet.Range[ROW, COL].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(200, 200, 0);
                    else
                        sheet.Range[ROW, COL].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(255, 242, 204);

                }
                catch (Exception ex)
                {

                }
                #endregion columns

                int endCol = COL;


                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, EndOfStaticColumns + 1, ROW, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, EndOfStaticColumns + 1, ROW, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, 1, ROW, endCol].RowHeight = 80;
                sheet.Range[ROW - 1, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                ROW++;

                //plotting data
                int StartRow = ROW;
                foreach (var item in TaskList.Keys)
                {
                    try
                    {


                        sheet[ROW, colBuyer].Text = clsStaticInfo.nullrecorder(TaskList[item][0]["Buyer"].ToString());
                        sheet[ROW, colType].Text = clsStaticInfo.nullrecorder(TaskList[item][0]["TNAType"].ToString());
                        sheet[ROW, colOrder].Text = clsStaticInfo.nullrecorder(TaskList[item][0]["MasterOrderId"].ToString());
                        sheet[ROW, colStyle].Text = clsStaticInfo.nullrecorder(TaskList[item][0]["StyleNo"].ToString());
                        sheet[ROW, colSO].Text = clsStaticInfo.nullrecorder(TaskList[item][0]["SONo"].ToString());
                        sheet[ROW, colProductionOrder].Text = clsStaticInfo.nullrecorder(TaskList[item][0]["PRNo"].ToString());


                        List<DataRow> TotalRows = TaskList[item];
                        for (int i = 0; i < TotalRows.Count; i++)
                        {
                            DateTime dtDueDate = Convert.ToDateTime(TotalRows[i]["DueDate"].ToString());

                            //if (clsStaticInfo.nullrecorder(sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]].Text) == "")
                            //    sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]].Text = TotalRows[i]["DueDate"].ToString();
                            //else
                            //    sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]].Text += ", " + TotalRows[i]["DueDate"].ToString();


                            if (TotalRows[i]["ClosingDate"].ToString() != "")
                            {
                                DateTime ClosingDate = Convert.ToDateTime(TotalRows[i]["ClosingDate"].ToString());

                                //closed in due time
                                if (dtDueDate >= ClosingDate && TotalRows[i]["ClosingDate"].ToString() != "")
                                    colorTextExcelCellDueDate(sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]], TotalRows[i]["DueDate"].ToString(), System.Drawing.Color.Green);

                                //closed but exceeding due time
                                if (dtDueDate < ClosingDate && TotalRows[i]["ClosingDate"].ToString() != "")
                                    colorTextExcelCellDueDate(sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]], TotalRows[i]["DueDate"].ToString(), System.Drawing.Color.Blue);


                                sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(200, 255, 200);
                            }

                            //task for today
                            if (dtDueDate == Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")) && TotalRows[i]["ClosingDate"].ToString() == "")
                                colorTextExcelCellDueDate(sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]], TotalRows[i]["DueDate"].ToString(), System.Drawing.Color.BlueViolet);


                            //future due date & not completed
                            if (dtDueDate > Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")) && TotalRows[i]["ClosingDate"].ToString() == "")
                                colorTextExcelCellDueDate(sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]], TotalRows[i]["DueDate"].ToString(), System.Drawing.Color.Orange);


                            //due date exceeding and not completed
                            if (dtDueDate < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")) && TotalRows[i]["ClosingDate"].ToString() == "")
                                colorTextExcelCellDueDate(sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]], TotalRows[i]["DueDate"].ToString(), System.Drawing.Color.Red);



                            IRange range = sheet[ROW, dicColIndex[TotalRows[i]["TaskMasterId"].ToString()]];

                            ICommentShape shape = range.AddComment();
                            //shape.RichText.Append("Task Name" + Environment.NewLine, fontCaption);
                            //shape.RichText.Append(TotalRows[i]["TaskDescription"].ToString() + Environment.NewLine, fontRegular);
                            shape.RichText.Append(string.Format("Original Start Date:{0}", TotalRows[i]["OriginalSequentialStartDate"].ToString()) + Environment.NewLine, fontCaption);
                            shape.RichText.Append(string.Format("Original End Date:{0} ", TotalRows[i]["OriginalSequentialEndDate"].ToString()) + Environment.NewLine, fontCaption);

                            shape.RichText.Append("Assigned To" + Environment.NewLine, fontCaption);
                            shape.RichText.Append(TotalRows[i]["AssignTo"].ToString() + Environment.NewLine, fontRegular);

                            shape.RichText.Append("Expected Due Date" + Environment.NewLine, fontCaption);
                            shape.RichText.Append(TotalRows[i]["TempEndDate"].ToString() + Environment.NewLine, fontRegular);

                            shape.RichText.Append("Commitment Date" + Environment.NewLine, fontCaption);
                            shape.RichText.Append(TotalRows[i]["CommitmentDate"].ToString() + Environment.NewLine + Environment.NewLine, fontRegular);

                            shape.RichText.Append("Completion Date" + Environment.NewLine, fontCaption);
                            shape.RichText.Append(TotalRows[i]["ClosingDate"].ToString() + Environment.NewLine + Environment.NewLine, fontRegular);


                            shape.IsTextLocked = false;
                            shape.AutoSize = false;

                            shape.Height = 400;
                            shape.Width = 300;
                        }

                        sheet.Range[ROW, 1, ROW, endCol].RowHeight = 24;
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.ShrinkToFit = true;
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);




                        ROW++;
                    }
                    catch (Exception ex)
                    {


                    }

                }


                sheet[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet[StartRow, 1, ROW, endCol].WrapText = true;
                sheet.Range[StartRow, 1, ROW, EndOfStaticColumns].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.IsGridLinesVisible = false;


                #region filter
                sheet.AutoFilters.FilterRange = sheet.Range[StartRow - 1, 1, ROW, EndOfStaticColumns];
                //Column index to which auto-filter must be applied
                IAutoFilter filter = sheet.AutoFilters[colType - 1];
                filter.AddTextFilter("Order");

                //Specify first condition
                IAutoFilterCondition firstCondition = filter.FirstCondition;
                firstCondition.ConditionOperator = ExcelFilterCondition.Contains;
                firstCondition.String = "Order";

                //Auto fit the second column
                //sheet.Range[StartRow - 1, 1, ROW, EndOfStaticColumns].EntireColumn.AutofitColumns();

                #endregion filter

                sheet.UsedRange[clsStaticInfo.GetxlsCol(EndOfStaticColumns + 1) + "6"].FreezePanes();
            }
            catch (Exception ex)
            {

            }

        }

        private void colorTextExcelCell(IRange Cell, IFont TNAFont, string Text, System.Drawing.Color Color, float fontSize = 30f)
        {

            Cell.Text += Text;
            IRichTextString richText = Cell.RichText;
            //Formatting first 4 characters.
            TNAFont.Bold = true;
            TNAFont.Size = fontSize;
            TNAFont.RGBColor = Color;
            richText.SetFont(richText.Text.Length - Text.Length, richText.Text.Length - 1, TNAFont);
        }
        private void colorTextExcelCellDueDate(IRange Cell, string Text, System.Drawing.Color Color, float fontSize = 8f)
        {
            if (clsStaticInfo.nullrecorder(Cell.Text) != "")
                Text = " " + Text;

            Cell.Text += Text;
            IRichTextString richText = Cell.RichText;
            //Formatting first 4 characters.
            IFont TNAFont = Cell.Worksheet.Workbook.CreateFont();
            TNAFont.Bold = true;
            TNAFont.Size = fontSize;
            TNAFont.RGBColor = Color;
            richText.SetFont(richText.Text.Length - Text.Length, richText.Text.Length - 1, TNAFont);
        }

        private void MasterOrderDataTablesOriginal(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields, out DataTable MainData, out DataTable TemplateData)
        {
            string FilterText = " WHERE 1=1 ";
            if (FilterFields != null)
            {
                for (int i = 0; i < FilterFields.Count; i++)
                {
                    FilterText += " AND isnull(" + FilterFields[i]["Key"].ToString() + ",'') IN (" + FilterFields[i]["Value"].ToString() + ")  ";
                }

            }
            string TaskTypeFilter = "";
            if (Filter["ReportLevel"].ToString() != "ALL")
                TaskTypeFilter = "WHERE tao.TaskAppliedOnEnum='" + Filter["ReportLevel"].ToString() + "'";


            if (Filter["ActiveStatus"].ToString() != "All")
            {
                if (Filter["ActiveStatus"].ToString() == "Closed")
                    TaskTypeFilter += " AND TM.CurrentStatus='" + Filter["ActiveStatus"].ToString() + "'";
                else
                    TaskTypeFilter += " AND isnull(TM.CurrentStatus,'')<>'" + Filter["ActiveStatus"].ToString() + "'";
            }
            if (Filter["DateSelection"].ToString() != "WITHOUTDATE")
            {
                if (Filter["DateSelection"].ToString() == "WITHDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";
                    else
                        TaskTypeFilter += " AND ATO.DueDate between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";

                }
                else if (Filter["DateSelection"].ToString() == "WITHOUTDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";
                    else
                        TaskTypeFilter += " AND ATO.DueDate<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";

                }
            }


            string sql = @"SELECT K.*
                                  FROM (SELECT 
                                TAM.ProcessId,CASE WHEN tm.CurrentStatus='Closed' THEN format(tm.ClosingDate,'dd-MMM-yyyy') ELSE NULL END AS ClosingDate,
                                pr.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,
                                isnull(TAM.TaskCategoryId,'')TaskCategoryId,isnull(TAM.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                mott.TaskMasterId,format(ato.DueDate,'dd-MMM-yyyy') AS DueDate,
	                            tm.TaskDescription,format(ISNULL(ATO.RevisedCommitmentDate,ISNULL(ATO.CommitmentDate,NULL)),'dd-MMM-yyyy') AS CommitmentDate,
								EAB.EmployeeName AS AssignBy,EATO.EmployeeName AS AssignTo,
                                MO.*

                                 FROM TaskManagerMaster AS tm
                                    inner join (" + TNAOrderColumns() + @") AS MO on MO.TaskManagerMasterId=tm.Id

                                LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
                                LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'
                                INNER JOIN TNATasks AS TT ON TT.Id=tm.TNATasksId
                                LEFT OUTER JOIN EmployeeInformation AS EAB ON eab.SystemId=ab.ResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId

                               LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						        LEFT JOIN ORG.Position pr ON pr.Id=MB.PositionId
								LEFT OUTER JOIN org.Department AS DTO ON dto.Id=pr.DepartmentId

                              
                                LEFT OUTER JOIN MasterOrderTaskTemplate AS mott ON mott.Id=MO.TaskTemplateId
                                LEFT OUTER JOIN TaskMaster AS TAM ON TAM.Id=mott.TaskMasterId
                                INNER JOIN hkp.TaskCategory AS tc ON TAM.TaskCategoryId=tc.Id AND TC.Active=1
                                INNER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id=TAM.TaskSubCategoryId AND TSC.Active=1

                                LEFT OUTER JOIN hkp.Process AS p ON p.Id=TAM.ProcessId
                                INNER JOIN hkp.TaskAppliedOn AS tao ON tao.Id=tam.TaskAppliedOnId
                                    " + TaskTypeFilter + @"
                                ) AS K " + FilterText + @" ORDER BY Buyer, MasterOrderId,RNK,StyleNo,SONo,PRNo";

            MainData = _sqlRepository.GetDataTable(sql);


            //if (FilterFields != null)
            //{
            //    for (int i = 0; i < FilterFields.Count; i++)
            //    {
            //        if (FilterFields[i]["Key"].ToString() == "TaskCategoryId")
            //        {
            //            if (TaskTypeFilter == "")
            //                TaskTypeFilter = " Where isnull(tc.Id,'') IN(" + FilterFields[i]["Value"].ToString() + ") ";
            //            else
            //                TaskTypeFilter = " AND isnull(tc.Id,'') IN(" + FilterFields[i]["Value"].ToString() + ") ";
            //        }
            //    }
            //}

            StringCollection strCol = new StringCollection();
            string IdCollection = "''";
            for (int i = 0; i < MainData.Rows.Count; i++)
            {
                if (MainData.Rows[i]["TaskMasterId"].ToString() != "")
                {
                    if (strCol.Contains(MainData.Rows[i]["TaskMasterId"].ToString()) == false)
                    {

                        IdCollection += ",'" + MainData.Rows[i]["TaskMasterId"].ToString() + "'";

                        strCol.Add(MainData.Rows[i]["TaskMasterId"].ToString());
                    }
                }
            }
            TaskTypeFilter = " AND isnull(TM.Id,'') IN(" + IdCollection + ") ";


            TemplateData = _sqlRepository.GetDataTable(@"SELECT TM.*,tc.UserName AS TaskCategory,TSc.UserName AS TaskSubCategory FROM TaskMaster AS tm 
                                    INNER JOIN hkp.TaskAppliedOn AS tao ON tao.Id=tm.TaskAppliedOnId
                                    INNER JOIN hkp.TaskCategory AS tc ON tm.TaskCategoryId=tc.Id
                                    INNER JOIN hkp.TaskSubCategory AS tsc ON tm.TaskSubCategoryId=tsc.Id
                                    WHERE 1=1 " + TaskTypeFilter + @"
                                    ORDER BY TSC.Sequence ASC,TC.Sequence ASC, tm.Sequence ASC");




        }
        private void MasterOrderDataTables(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields, out DataTable MainData, out DataTable TemplateData)
        {
            string DueDate = "TT.OriginalSequentialEndDate";//or ATO.DueDate
            string FilterText = " WHERE 1=1 ";
            if (FilterFields != null)
            {
                for (int i = 0; i < FilterFields.Count; i++)
                {
                    FilterText += " AND isnull(" + FilterFields[i]["Key"].ToString() + ",'') IN (" + FilterFields[i]["Value"].ToString() + ")  ";
                }

            }
            string TaskTypeFilter = "";
            if (Filter["ReportLevel"].ToString() != "ALL")
                TaskTypeFilter = "WHERE tao.TaskAppliedOnEnum='" + Filter["ReportLevel"].ToString() + "'";


            if (Filter["ActiveStatus"].ToString() != "All")
            {
                if (Filter["ActiveStatus"].ToString() == "Closed")
                    TaskTypeFilter += " AND TM.CurrentStatus='" + Filter["ActiveStatus"].ToString() + "'";
                else
                    TaskTypeFilter += " AND isnull(TM.CurrentStatus,'')<>'" + Filter["ActiveStatus"].ToString() + "'";
            }
            if (Filter["DateSelection"].ToString() != "WITHOUTDATE")
            {
                if (Filter["DateSelection"].ToString() == "WITHDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";
                    else
                        TaskTypeFilter += " AND " + DueDate + " between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";

                }
                else if (Filter["DateSelection"].ToString() == "WITHOUTDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";
                    else
                        TaskTypeFilter += " AND " + DueDate + "<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";

                }
            }


            string sql = @"SELECT K.*
                                  FROM (SELECT 
                                TAM.ProcessId,CASE WHEN tm.CurrentStatus='Closed' THEN format(tm.ClosingDate,'dd-MMM-yyyy') ELSE NULL END AS ClosingDate,
                                pr.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,
                                isnull(TAM.TaskCategoryId,'')TaskCategoryId,isnull(TAM.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                mott.TaskMasterId,format(" + DueDate + @",'dd-MMM-yyyy') AS DueDate,
                                FORMAT(TT.TempStartDate,'dd-MMM-yyyy') AS TempStartDate,FORMAT(TT.TempEndDate,'dd-MMM-yyyy') AS TempEndDate,
                                FORMAT(TT.ActualStartDate,'dd-MMM-yyyy') AS ActualStartDate,FORMAT(TT.ActualEndDate,'dd-MMM-yyyy') AS ActualEndDate,
                                FORMAT(TT.OriginalSequentialStartDate,'dd-MMM-yyyy') AS OriginalSequentialStartDate,	FORMAT(TT.OriginalSequentialEndDate,'dd-MMM-yyyy') AS OriginalSequentialEndDate,

	                            tm.TaskDescription,format(ISNULL(ATO.RevisedCommitmentDate,ISNULL(ATO.CommitmentDate,NULL)),'dd-MMM-yyyy') AS CommitmentDate,
								EAB.EmployeeName AS AssignBy,EATO.EmployeeName AS AssignTo,
                                MO.*

                                 FROM TaskManagerMaster AS tm
                                    inner join (" + TNAOrderColumns() + @") AS MO on MO.TaskManagerMasterId=tm.Id

                                LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
                                LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'
                                INNER JOIN TNATasks AS TT ON TT.Id=tm.TNATasksId
                                INNER JOIN MasterOrderTaskTemplate AS TOM ON TOM.Id=TT.TaskTemplateId
                                LEFT OUTER JOIN EmployeeInformation AS EAB ON eab.SystemId=ab.ResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId

                               LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						        LEFT JOIN ORG.Position pr ON pr.Id=MB.PositionId
								LEFT OUTER JOIN org.Department AS DTO ON dto.Id=pr.DepartmentId

                              
                                LEFT OUTER JOIN MasterOrderTaskTemplate AS mott ON mott.Id=MO.TaskTemplateId
                                LEFT OUTER JOIN TaskMaster AS TAM ON TAM.Id=mott.TaskMasterId
                                INNER JOIN hkp.TaskCategory AS tc ON TAM.TaskCategoryId=tc.Id AND TC.Active=1
                                INNER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id=TAM.TaskSubCategoryId AND TSC.Active=1

                                LEFT OUTER JOIN hkp.Process AS p ON p.Id=TAM.ProcessId
                                INNER JOIN hkp.TaskAppliedOn AS tao ON tao.Id=tam.TaskAppliedOnId
                                    " + TaskTypeFilter + @"
                                ) AS K " + FilterText + @" ORDER BY Buyer, MasterOrderId,RNK,StyleNo,SONo,PRNo";

            MainData = _sqlRepository.GetDataTable(sql);


            //if (FilterFields != null)
            //{
            //    for (int i = 0; i < FilterFields.Count; i++)
            //    {
            //        if (FilterFields[i]["Key"].ToString() == "TaskCategoryId")
            //        {
            //            if (TaskTypeFilter == "")
            //                TaskTypeFilter = " Where isnull(tc.Id,'') IN(" + FilterFields[i]["Value"].ToString() + ") ";
            //            else
            //                TaskTypeFilter = " AND isnull(tc.Id,'') IN(" + FilterFields[i]["Value"].ToString() + ") ";
            //        }
            //    }
            //}

            StringCollection strCol = new StringCollection();
            string IdCollection = "''";
            for (int i = 0; i < MainData.Rows.Count; i++)
            {
                if (MainData.Rows[i]["TaskMasterId"].ToString() != "")
                {
                    if (strCol.Contains(MainData.Rows[i]["TaskMasterId"].ToString()) == false)
                    {

                        IdCollection += ",'" + MainData.Rows[i]["TaskMasterId"].ToString() + "'";

                        strCol.Add(MainData.Rows[i]["TaskMasterId"].ToString());
                    }
                }
            }
            TaskTypeFilter = " AND isnull(TM.Id,'') IN(" + IdCollection + ") ";


            TemplateData = _sqlRepository.GetDataTable(@"SELECT TM.*,tc.UserName AS TaskCategory,TSc.UserName AS TaskSubCategory FROM TaskMaster AS tm 
                                    INNER JOIN hkp.TaskAppliedOn AS tao ON tao.Id=tm.TaskAppliedOnId
                                    INNER JOIN hkp.TaskCategory AS tc ON tm.TaskCategoryId=tc.Id
                                    INNER JOIN hkp.TaskSubCategory AS tsc ON tm.TaskSubCategoryId=tsc.Id
                                    --WHERE 1=1 " + TaskTypeFilter + @"
                                    WHERE 1=1 AND  isnull(TM.Id,'') IN (Select distinct TaskMasterId from MasterOrderTaskTemplate)
                                    ORDER BY TSC.Sequence ASC,TC.Sequence ASC, tm.Sequence ASC");




        }
        private string TNAOrderColumns()
        {
            string s = @"SELECT 1 AS RNK, 'Order' AS TNAType,  (ISNULL(MasterOrderId,'')+tm.Id+'ORDER') AS [KEY],tm.Id AS TaskManagerMasterId,b.Id AS BuyerId,tt.TaskTemplateId, b.UserName AS Buyer,mo.Id  AS MasterOrderId,
                                    StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
				                                        trn.MasterOrderItem XMOI 	                                                   
				                                    where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
				LineItemReference=STUFF((select distinct ','+so.LineItemReference from 
				                                        trn.MasterOrderItem XMOI 	 
				                                        INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id                                                  
				                                    where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                    SONo=STUFF((select distinct ','+so.Id from 
				                                        trn.MasterOrderItem XMOI 	 
				                                        INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id                                                  
				                                    where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
				
                                    PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
				                                        trn.MasterOrderItem XMOI 	 
				                                        INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id   
				                                        INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id                                               
				                                    where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				
                                        FROM TaskManagerMaster AS tm
                                    INNER JOIN TNATasks AS TT ON TT.Id=tm.TNATasksId
                                    INNER JOIN TNAMaster AS T ON t.Id=tt.TNAMasterId AND isnull(t.MasterOrderId,'')<>''
                                    INNER JOIN trn.MasterOrder AS mo ON mo.Id=t.MasterOrderId
                                    LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId

                                    UNION

                                    SELECT 2 AS RNK, 'Style' AS TNAType, (ISNULL(MOI.BuyerReferenceNo,'')+tm.Id+'STYLE') AS [KEY], tm.Id AS TaskManagerMasterId,b.Id AS BuyerId,tt.TaskTemplateId, b.UserName AS Buyer,mo.Id  AS MasterOrderId,
                                StyleNo=MOI.BuyerReferenceNo,
				LineItemReference=STUFF((select distinct ','+so.LineItemReference from 
				                                 trn.MasterOrderItem XMOI 	 
				                                 INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id                                                  
				                                where MOI.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                SONo=STUFF((select distinct ','+so.Id from 
				                                 trn.MasterOrderItem XMOI 	 
				                                 INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id                                                  
				                                where MOI.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
				
                                PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
				                                 trn.MasterOrderItem XMOI 	 
				                                 INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id   
				                                 INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id                                               
				                                where MOI.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				
                                 FROM TaskManagerMaster AS tm
                                INNER JOIN TNATasks AS TT ON TT.Id=tm.TNATasksId
                                INNER JOIN TNAMaster AS T ON t.Id=tt.TNAMasterId AND isnull(t.MasterOrderItemId,'')<>''
                                inner join trn.MasterOrderItem MOI on MOI.Id=t.MasterOrderItemId
                                INNER JOIN trn.MasterOrder AS mo ON mo.Id=MOI.MasterOrderId
                                LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                
                                UNION

                              SELECT  3 AS RNK, 'Sales Order' AS TNAType, (ISNULL(so.Id,'')+tm.Id+'SO') AS [KEY],tm.Id AS TaskManagerMasterId,b.Id AS BuyerId,tt.TaskTemplateId, b.UserName AS Buyer,mo.Id  AS MasterOrderId,
                                StyleNo=MOI.BuyerReferenceNo,
				so.LineItemReference,
                                SONo=so.Id,
				
                                PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
				                                  trn.ProductionOrderDetail AS pod                                              
				                                where SO.Id=POD.SalesOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				
                                 FROM TaskManagerMaster AS tm
                                INNER JOIN TNATasks AS TT ON TT.Id=tm.TNATasksId
                                INNER JOIN TNAMaster AS T ON t.Id=tt.TNAMasterId AND isnull(t.SalesOrderId,'')<>''
                                INNER JOIN trn.salesorder SO ON so.Id=t.SalesOrderId
                                inner join trn.MasterOrderItem MOI on MOI.Id=so.MasterOrderItemId
                                INNER JOIN trn.MasterOrder AS mo ON mo.Id=MOI.MasterOrderId
                                LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                
                                UNION

                               SELECT 4 AS RNK, 'Prod. Order' AS TNAType, (ISNULL(PR.ProductionOrderId,'')+tm.Id+'PR') AS [KEY],tm.Id AS TaskManagerMasterId,
                                PR.BuyerId,tt.TaskTemplateId,   PR.Buyer,
                                PR.MasterOrderId,
                                PR.StyleNo,pr.LineItemReference,pr.SONo,
				
				
                                pr.ProductionOrderId AS PRNo
				
                                 FROM TaskManagerMaster AS tm
                                INNER JOIN TNATasks AS TT ON TT.Id=tm.TNATasksId
                                INNER JOIN TNAMaster AS T ON t.Id=tt.TNAMasterId  AND isnull(t.ProductionOrderId,'')<>''
                                INNER JOIN trn.ProductionOrder AS po ON PO.Id=t.ProductionOrderId
                                INNER JOIN
                                (
                                			SELECT distinct po.Id AS ProductionOrderId,
                                			b.Id AS BuyerId,b.UserName AS Buyer,
                                			
                                			 MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
											 
											 ,StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                	 ,LineItemReference=STUFF((select distinct ','+sox.LineItemReference from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                			  ,SONo=STUFF((select distinct ','+sox.Id from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				                                
														 FROM trn.ProductionOrder PO
										INNER JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=po.Id AND pod.Id=(SELECT TOP 1 Id FROM trn.ProductionOrderDetail AS px WHERE px.ProductionOrderId=po.Id)
                                		INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                		inner join trn.MasterOrderItem MOI on MOI.Id=so.MasterOrderItemId
										INNER JOIN trn.MasterOrder AS mo ON mo.Id=MOI.MasterOrderId
										LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                ) AS PR ON pr.ProductionOrderId=po.Id";

            return s;
        }

        private void MasterOrderDataTablesForGrid(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields, out DataTable MainData)
        {
            string DueDate = "TT.OriginalSequentialEndDate";//or ATO.DueDate
            string FilterText = " WHERE 1=1";
            if (FilterFields != null)
            {
                for (int i = 0; i < FilterFields.Count; i++)
                {
                    FilterText += " AND isnull(RTRIM(LTRIM(" + FilterFields[i]["Key"].ToString() + ")),'') IN (" + FilterFields[i]["Value"].ToString().Replace("' ", "'").Replace("', '", "','").Replace(", ", ",") + ")  ";
                }

            }
            string TaskTypeFilter = "";
            if (Filter["ReportLevel"].ToString() != "ALL")
                TaskTypeFilter = "WHERE  tao.TaskAppliedOnEnum='" + Filter["ReportLevel"].ToString() + "'";


            if (Filter["ActiveStatus"].ToString() != "All")
            {
                if (Filter["ActiveStatus"].ToString() == "Closed")
                    TaskTypeFilter += " AND TM.CurrentStatus='" + Filter["ActiveStatus"].ToString() + "'";
                else
                    TaskTypeFilter += " AND isnull(TM.CurrentStatus,'')<>'Closed'";
            }
            if (Filter["DateSelection"].ToString() != "WITHOUTDATE")
            {
                if (Filter["DateSelection"].ToString() == "WITHDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";
                    else
                        TaskTypeFilter += " AND " + DueDate + " between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";

                }
                else if (Filter["DateSelection"].ToString() == "WITHOUTDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";
                    else
                        TaskTypeFilter += " AND " + DueDate + "<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";

                }
            }


            string sql = @"SELECT K.*
                                  FROM (SELECT 
                                TAM.ProcessId,CASE WHEN tm.CurrentStatus='Closed' THEN format(tm.ClosingDate,'dd-MMM-yyyy') ELSE NULL END AS ClosingDate,
                                CASE WHEN tm.CurrentStatus='Closed' THEN isnull(USRCL.FullName,isnull(EACL.EmployeeName,TM.ClosedBy)) ELSE NULL END AS ClosedBy,
                                pr.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,TM.CurrentStatus,
                               mott.Sequence, isnull(TAM.TaskCategoryId,'')TaskCategoryId,isnull(TAM.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                tc.UserName AS Category,tsc.UserName as SubCategory,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat,
                                format(" + DueDate + @",'dd-MMM-yyyy') AS DueDate,
                                format(OriginalSequentialStartDate,'dd-MMM-yyyy') AS OriginalSequentialStartDate,	format(OriginalSequentialEndDate,'dd-MMM-yyyy') AS OriginalSequentialEndDate,
                                format(TempStartDate,'dd-MMM-yyyy') AS TempStartDate,	format(TempEndDate,'dd-MMM-yyyy') AS TempEndDate,
                                concat(TM.TaskType,'/',MO.Dependency) AS TaskType,
                                datediff(day," + DueDate + @",TM.closingDate) AS EarlyOrLateBy,
	                            tm.TaskDescription AS Task,format(ISNULL(ATO.RevisedCommitmentDate,ISNULL(ATO.CommitmentDate,NULL)),'dd-MMM-yyyy') AS CommitmentDate,
								EAB.EmployeeName AS AssignBy,EATO.EmployeeName AS AssignTo,TTD.DependentDatesEnum,TTD.TaskDependentOn,FORMAT(TT.DependentDate,'dd-MMM-yyyy')DependentDate,
                                MO.*
                                 FROM TaskManagerMaster AS tm
                                    inner join (" + TNATasks() + @") AS MO on MO.TaskMasterId=tm.Id
                                INNER JOIN TNATasks AS TT ON TT.Id=tm.TNATasksId
                                LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
                                LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'

                                LEFT OUTER JOIN EmployeeInformation AS EAB ON eab.SystemId=ab.ResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS EACL ON EACL.SystemId=TM.ClosedBy
                                LEFT OUTER JOIN SEC.[USER] AS USRCL ON USRCL.UserId=TM.ClosedBy

                               LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						        LEFT JOIN ORG.Position pr ON pr.Id=MB.PositionId
								LEFT OUTER JOIN org.Department AS DTO ON dto.Id=pr.DepartmentId
                                left outer join TaskComments TSK on TSK.TaskManagerMasterId=TM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TM.ID ORDER BY T.CreatedTime DESC)

                              
                                LEFT OUTER JOIN MasterOrderTaskTemplate AS mott ON mott.Id=MO.TaskTemplateId
                                LEFT OUTER JOIN TaskMaster AS TAM ON TAM.Id=mott.TaskMasterId
                                INNER JOIN hkp.TaskCategory AS tc ON TAM.TaskCategoryId=tc.Id AND TC.Active=1
                                INNER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id=TAM.TaskSubCategoryId AND TSC.Active=1

                                LEFT OUTER JOIN hkp.Process AS p ON p.Id=TAM.ProcessId
                                INNER JOIN hkp.TaskAppliedOn AS tao ON tao.Id=tam.TaskAppliedOnId
                                left join  HKP.TaskDependentDates AS TTD on TTD.id=mott.TaskDependentDatesId
                                    " + TaskTypeFilter + @"
                                ) AS K " + FilterText + @"  ORDER BY Buyer,StyleNo,SONo,PRNo";

            MainData = _sqlRepository.GetDataTable(sql);





        }
        private void MasterOrderDataTablesForGridException(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields, out DataTable MainData)
        {
            string DueDate = "TT.OriginalSequentialEndDate";//or ATO.DueDate
            string FilterText = " WHERE 1=1 ";
            if (FilterFields != null)
            {
                for (int i = 0; i < FilterFields.Count; i++)
                {
                    FilterText += " AND isnull(RTRIM(LTRIM(" + FilterFields[i]["Key"].ToString() + ")),'') IN (" + FilterFields[i]["Value"].ToString().Replace("' ", "'").Replace("', '", "','").Replace(", ", ",") + ")  ";
                }

            }
            string TaskTypeFilter = "";
            if (Filter["ReportLevel"].ToString() != "ALL")
                TaskTypeFilter = "WHERE tao.TaskAppliedOnEnum='" + Filter["ReportLevel"].ToString() + "'";


            if (Filter["ActiveStatus"].ToString() != "All")
            {
                if (Filter["ActiveStatus"].ToString() == "Closed")
                    TaskTypeFilter += " AND TM.CurrentStatus='" + Filter["ActiveStatus"].ToString() + "'";
                else
                    TaskTypeFilter += " AND isnull(TM.CurrentStatus,'')<>'Closed'";
            }
            if (Filter["DateSelection"].ToString() != "WITHOUTDATE")
            {
                if (Filter["DateSelection"].ToString() == "WITHDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";
                    else
                        TaskTypeFilter += " AND " + DueDate + " between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";

                }
                else if (Filter["DateSelection"].ToString() == "WITHOUTDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";
                    else
                        TaskTypeFilter += " AND " + DueDate + "<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";

                }
            }


            string sql = @"SELECT K.*
                                  FROM (SELECT 
                                TAM.ProcessId,CASE WHEN tm.CurrentStatus='Closed' THEN format(tm.ClosingDate,'dd-MMM-yyyy') ELSE NULL END AS ClosingDate,
                                CASE WHEN tm.CurrentStatus='Closed' THEN isnull(USRCL.FullName,isnull(EACL.EmployeeName,TM.ClosedBy)) ELSE NULL END AS ClosedBy,
                                pr.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,TM.CurrentStatus,
                               mott.Sequence, isnull(TAM.TaskCategoryId,'')TaskCategoryId,isnull(TAM.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                tc.UserName AS Category,tsc.UserName as SubCategory,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChat,
                                format(" + DueDate + @",'dd-MMM-yyyy') AS DueDate,
                                format(OriginalSequentialStartDate,'dd-MMM-yyyy') AS OriginalSequentialStartDate,	format(OriginalSequentialEndDate,'dd-MMM-yyyy') AS OriginalSequentialEndDate,
                                format(TempStartDate,'dd-MMM-yyyy') AS TempStartDate,	format(TempEndDate,'dd-MMM-yyyy') AS TempEndDate,
                                concat(TM.TaskType,'/',MO.Dependency) AS TaskType,
                                datediff(day," + DueDate + @",TM.closingDate) AS EarlyOrLateBy,
	                            tm.TaskDescription AS Task,format(ISNULL(ATO.RevisedCommitmentDate,ISNULL(ATO.CommitmentDate,NULL)),'dd-MMM-yyyy') AS CommitmentDate,
								EAB.EmployeeName AS AssignBy,EATO.EmployeeName AS AssignTo,
                                MO.*
                                 FROM TaskManagerMaster AS tm
                                    inner join (" + TNATasks() + @") AS MO on MO.TaskMasterId=tm.Id
                                INNER JOIN TNATasks AS TT ON TT.Id=tm.TNATasksId
                                LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
                                LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'

                                LEFT OUTER JOIN EmployeeInformation AS EAB ON eab.SystemId=ab.ResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS EACL ON EACL.SystemId=TM.ClosedBy
                                LEFT OUTER JOIN SEC.[USER] AS USRCL ON USRCL.UserId=TM.ClosedBy

                                LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						        LEFT JOIN ORG.Position pr ON pr.Id=MB.PositionId
								LEFT OUTER JOIN org.Department AS DTO ON dto.Id=pr.DepartmentId
                                left outer join TaskComments TSK on TSK.TaskManagerMasterId=TM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TM.ID ORDER BY T.CreatedTime DESC)

                              
                                LEFT OUTER JOIN MasterOrderTaskTemplate AS mott ON mott.Id=MO.TaskTemplateId
                                LEFT OUTER JOIN TaskMaster AS TAM ON TAM.Id=mott.TaskMasterId
                                INNER JOIN hkp.TaskCategory AS tc ON TAM.TaskCategoryId=tc.Id AND TC.Active=1
                                INNER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id=TAM.TaskSubCategoryId AND TSC.Active=1

                                LEFT OUTER JOIN hkp.Process AS p ON p.Id=TAM.ProcessId
                                INNER JOIN hkp.TaskAppliedOn AS tao ON tao.Id=tam.TaskAppliedOnId
                                    " + TaskTypeFilter + @"
                                ) AS K " + FilterText + @" AND isnull(CommitmentDate,'')<>'' AND dueDate<>CommitmentDate ORDER BY Buyer,StyleNo,SONo,PRNo";

            MainData = _sqlRepository.GetDataTable(sql);





        }

        private string TNATasks()
        {
            string sql = @"SELECT  'Order' AS Dependency, tt.TaskTemplateId,TMMM.Id AS TaskMasterId, 
                                    	     MO.MasterOrderNo AS MasterOrderId,MO.BuyerId,
                             B.UserName AS Buyer
                            
                            ,StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
                            trn.MasterOrderItem XMOI 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            SONo=STUFF((select distinct ','+so.Id from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            LineItemReference=STUFF((select distinct ','+so.LineItemReference from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            SOQty=(select sum(SO.Qty) from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId),

                            PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                          
                            ,Department=bd.UserName,Division=bd2.UserName
                            FROM TaskManagerMaster AS TMMM

                              INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMMM.TNATasksId 
                            LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                            inner JOIN [TRN].[MasterOrder] AS MO ON MO.Id = TM.MasterOrderId
                             LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    						
                            UNION

                            SELECT  'Item' AS Dependency, tt.TaskTemplateId, TMM.Id AS TaskMasterId,
                             MO.MasterOrderNo,B.Id, B.UserName AS Buyer
                            ,StyleNo= MOI.BuyerReferenceNo,
                            SONo=STUFF((select distinct ','+so.Id from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            LineItemReference=STUFF((select distinct ','+so.LineItemReference from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            SOQty=(select sum(so.Qty) from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId),

                            PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
                            where MO.Id=XMOI.MasterOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,Department=bd.UserName,Division=bd2.UserName
                            FROM TaskManagerMaster AS TMM

                            LEFT OUTER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
                            inner JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                            inner JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = TM.MasterOrderItemId
                            LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
                            LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId 
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    

						
                            UNION 

                            SELECT 'Sales Order' AS Dependency, tt.TaskTemplateId, TMM.Id AS TaskMasterId,
                               MO.MasterOrderNo,B.Id, B.UserName AS Buyer
                            ,StyleNo= MOI.BuyerReferenceNo
                            ,SONo=so.Id
                            ,so.LineItemReference
                            ,SOQty=SO.Qty
                            ,PRNo=STUFF((select distinct ','+xpod.ProductionOrderId from  trn.ProductionOrderDetail AS xpod
                            where xpod.SalesOrderId = so.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                          
                            ,Department=bd.UserName,Division=bd2.UserName
                            FROM TaskManagerMaster AS TMM

                              INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
                            LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                            inner JOIN [TRN].[SalesOrder] AS SO ON SO.Id =  TM.SalesOrderId
                            LEFT OUTER JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = SO.MasterOrderItemId
                            LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
                            LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    
                            UNION 

                          

                            SELECT 'Prod. Order' AS Dependency,tt.TaskTemplateId, TMM.Id AS TaskMasterId, 
                               pr.MasterOrderId,PR.BuyerId,pr.Buyer,pr.StyleNo, pr.SONo,pr.LineItemReference,PR.SOQty, pr.ProductionOrderId
                            ,Department=bd.UserName,Division=bd2.UserName
				
                                 FROM TaskManagerMaster AS tmm
                                INNER JOIN TNATasks AS TT ON TT.Id=tmm.TNATasksId
                                INNER JOIN TNAMaster AS T ON t.Id=tt.TNAMasterId  AND isnull(t.ProductionOrderId,'')<>''
                                    INNER JOIN trn.ProductionOrder AS po ON PO.Id=t.ProductionOrderId
                                INNER JOIN
                                (
                                			SELECT distinct po.Id AS ProductionOrderId,mo.BuyerDepartmentId,mo.BuyerDivisionId,
                                			b.Id AS BuyerId,b.UserName AS Buyer,
                                			
                                			 MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
											 
											 ,StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                	
                                			  ,SONo=STUFF((select distinct ','+sox.Id from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                                ,LineItemReference=STUFF((select distinct ','+sox.LineItemReference from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                                ,SOQty=(select sum(sox.Qty) from 
														 trn.MasterOrderItem XMOI 	 
														 INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
														 INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
														where podx.ProductionOrderId=po.Id)
				                                
														 FROM trn.ProductionOrder PO
										INNER JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=po.Id AND pod.Id=(SELECT TOP 1 Id FROM trn.ProductionOrderDetail AS px WHERE px.ProductionOrderId=po.Id)
                                		INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                		inner join trn.MasterOrderItem MOI on MOI.Id=so.MasterOrderItemId
										INNER JOIN trn.MasterOrder AS mo ON mo.Id=MOI.MasterOrderId
										LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                ) AS PR ON pr.ProductionOrderId=po.Id
                                
                                LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
								LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
								LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=PR.BuyerDepartmentId   
								LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=PR.BuyerDivisionId  ";




            return sql;
        }

        private Dictionary<string, List<DataRow>> GetSqlTaskComments(Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
        {
            string FilterText = " WHERE 1=1 ";
            if (FilterFields != null)
            {
                for (int i = 0; i < FilterFields.Count; i++)
                {
                    FilterText += " AND isnull(" + FilterFields[i]["Key"].ToString() + ",'') IN (" + FilterFields[i]["Value"].ToString() + ")  ";
                }

            }
            string TaskTypeFilter = "";
            if (Filter["ReportLevel"].ToString() != "ALL")
                TaskTypeFilter = "WHERE tao.TaskAppliedOnEnum='" + Filter["ReportLevel"].ToString() + "'";


            if (Filter["ActiveStatus"].ToString() != "All")
            {
                if (Filter["ActiveStatus"].ToString() == "Closed")
                    TaskTypeFilter += " AND TM.CurrentStatus='" + Filter["ActiveStatus"].ToString() + "'";
                else
                    TaskTypeFilter += " AND isnull(TM.CurrentStatus,'')<>'" + Filter["ActiveStatus"].ToString() + "'";
            }
            if (Filter["DateSelection"].ToString() != "WITHOUTDATE")
            {
                if (Filter["DateSelection"].ToString() == "WITHDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";
                    else
                        TaskTypeFilter += " AND ATO.DueDate between '" + Filter["FromDate"].ToString() + "' AND '" + Filter["ToDate"].ToString() + "'";

                }
                else if (Filter["DateSelection"].ToString() == "WITHOUTDATE")
                {
                    if (Filter["ActiveStatus"].ToString() == "Closed")
                        TaskTypeFilter += " AND TM.ClosingDate<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";
                    else
                        TaskTypeFilter += " AND ATO.DueDate<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";

                }
            }
            string sql = @"SELECT K.*
                                  FROM (SELECT 
                               Tm.ID TaskManagerMasterId, TAM.ProcessId,CASE WHEN tm.CurrentStatus='Closed' THEN format(tm.ClosingDate,'dd-MMM-yyyy') ELSE NULL END AS ClosingDate,
                                pr.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,TM.CurrentStatus,
                                isnull(TAM.TaskCategoryId,'')TaskCategoryId,isnull(TAM.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                tc.UserName AS Category,tsc.UserName as SubCategory,
                                format(ato.DueDate,'dd-MMM-yyyy') AS DueDate,concat(TM.TaskType,'/',MO.Dependency) AS TaskType,
                                datediff(day,ATO.duedate,TM.closingDate) AS EarlyOrLateBy,
                                FORMAT(tcom.CreatedTime,'dd-MMM-yyyy HH:mm:ss tt') AS CreatedTime,ei.EmployeeName AS CommentedBy,
                                    tcom.CommentText,
	                            tm.TaskDescription AS Task,format(ISNULL(ATO.RevisedCommitmentDate,ISNULL(ATO.CommitmentDate,NULL)),'dd-MMM-yyyy') AS CommitmentDate,
								EAB.EmployeeName AS AssignBy,EATO.EmployeeName AS AssignTo,
                                MO.*
                                 FROM TaskManagerMaster AS tm
                                    inner join (" + TNATasks() + @") AS MO on MO.TaskMasterId=tm.Id

                                INNER JOIN TaskComments AS tcom ON tcom.TaskManagerMasterId=tm.Id
                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=tcom.CreatedById

                                LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
                                LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'

                                LEFT OUTER JOIN EmployeeInformation AS EAB ON eab.SystemId=ab.ResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId

                                LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
						        LEFT JOIN ORG.Position pr ON pr.Id=MB.PositionId
								LEFT OUTER JOIN org.Department AS DTO ON dto.Id=pr.DepartmentId

                              
                                LEFT OUTER JOIN MasterOrderTaskTemplate AS mott ON mott.Id=MO.TaskTemplateId
                                LEFT OUTER JOIN TaskMaster AS TAM ON TAM.Id=mott.TaskMasterId
                                INNER JOIN hkp.TaskCategory AS tc ON TAM.TaskCategoryId=tc.Id AND TC.Active=1
                                INNER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id=TAM.TaskSubCategoryId AND TSC.Active=1

                                LEFT OUTER JOIN hkp.Process AS p ON p.Id=TAM.ProcessId
                                INNER JOIN hkp.TaskAppliedOn AS tao ON tao.Id=tam.TaskAppliedOnId
                                    " + TaskTypeFilter + @"
                                ) AS K " + FilterText + @"   order by TaskManagerMasterId,convert(datetime,CreatedTime)";

            
            DataTable dt = _sqlRepository.GetDataTable(sql);

            Dictionary<string, List<DataRow>> dicComments = new Dictionary<string, List<DataRow>>();
            List<DataRow> Data = new List<DataRow>();
            string id = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (id != dt.Rows[i]["TaskManagerMasterId"].ToString())
                {
                    Data = new List<DataRow>();
                    dicComments.Add(dt.Rows[i]["TaskManagerMasterId"].ToString(), Data);
                }
                Data.Add(dt.Rows[i]);

                id = dt.Rows[i]["TaskManagerMasterId"].ToString();
            }

            return dicComments;
        }

        private void GetTNAStatusReportsData(out DataTable dtTna, Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
        {

            MasterOrderDataTablesForGrid(Filter, FilterFields, out dtTna);

            dtTna.Columns.Add("EarlyBy", typeof(int));
            dtTna.Columns.Add("LateBy", typeof(int));
            for (int i = 0; i < dtTna.Rows.Count; i++)
            {
                if (dtTna.Rows[i]["CurrentStatus"].ToString().ToUpper() == "CLOSED" && dtTna.Rows[i]["ClosingDate"].ToString() != "")
                {
                    try
                    {
                        DateTime dtDueDate = Convert.ToDateTime(dtTna.Rows[i]["DueDate"].ToString());
                        DateTime dtClosingDate = Convert.ToDateTime(dtTna.Rows[i]["ClosingDate"].ToString());
                        if (dtClosingDate < dtDueDate)
                            dtTna.Rows[i]["EarlyBy"] = Math.Abs(clsStaticInfo.dateDiff(dtClosingDate.ToString("dd-MMM-yyyy"), dtDueDate.ToString("dd-MMM-yyyy")));
                        if (dtClosingDate > dtDueDate)
                            dtTna.Rows[i]["LateBy"] = Math.Abs(clsStaticInfo.dateDiff(dtDueDate.ToString("dd-MMM-yyyy"), dtClosingDate.ToString("dd-MMM-yyyy")));
                    }
                    catch (Exception)
                    {

                        
                    }
                    
                }
            }
        }
        private void GetTNAStatusReportsDataException(out DataTable dtTna, Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
        {

            MasterOrderDataTablesForGridException(Filter, FilterFields, out dtTna);

            dtTna.Columns.Add("EarlyBy", typeof(int));
            dtTna.Columns.Add("LateBy", typeof(int));
            for (int i = 0; i < dtTna.Rows.Count; i++)
            {
                if (dtTna.Rows[i]["CurrentStatus"].ToString().ToUpper() == "CLOSED" && dtTna.Rows[i]["ClosingDate"].ToString() != "")
                {

                    DateTime dtDueDate = Convert.ToDateTime(dtTna.Rows[i]["DueDate"].ToString());
                    DateTime dtClosingDate = Convert.ToDateTime(dtTna.Rows[i]["ClosingDate"].ToString());
                    if (dtClosingDate < dtDueDate)
                        dtTna.Rows[i]["EarlyBy"] = Math.Abs(clsStaticInfo.dateDiff(dtClosingDate.ToString("dd-MMM-yyyy"), dtDueDate.ToString("dd-MMM-yyyy")));
                    if (dtClosingDate > dtDueDate)
                        dtTna.Rows[i]["LateBy"] = Math.Abs(clsStaticInfo.dateDiff(dtDueDate.ToString("dd-MMM-yyyy"), dtClosingDate.ToString("dd-MMM-yyyy")));
                }
            }
        }
        public IWorkbook GetTNAStatusReport(string CompanyGroupId, string CompanyId, string PlantId, string PlantName, string EmployeeId, string UserName, Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();

            DataTable dtTNA = null;

            DataSet dsCmp = null;

            DataSet dsFactory = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string FactoryAddress = string.Empty;
            string OTConsiderOn = string.Empty;
            #endregion
            var SLNo = 1;
            try
            {
                objRpt = new clsReport();


                ExcelEngine excelEngine = null;
              
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);

                #region Get Data Query
                GetTNAStatusReportsData(out dtTNA, Filter, FilterFields);
                if (dtTNA.Rows.Count == 0)
                    throw new Exception("No data found");
                Dictionary<string, List<DataRow>> dicComments = GetSqlTaskComments(Filter, FilterFields);
                #endregion

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";

                var isl = 0;
               

                int colTaskType = 0;
                int colTask = 0;
                int colAssignBy = 0;
                int colTaskDependentOn = 0;
                int colDependentDt = 0;
                int colDependentDatesOn = 0;
                int colAssignTo = 0;
                int colDueDate = 0;
                int colCommitmentDate = 0;
                int colMasterOrderNo = 0;
                int colStyleNo = 0;
                int colSONo = 0;
                int colPRNo = 0;
                int colSubCategory = 0;
                int colCategory = 0;
                int colEarlyBy = 0;
                int colLateBy = 0;
                int colClosingDate = 0;

                objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);

                objRpt.SelectedPlant(PlantId, out dsFactory);

                workbook = application.Workbooks.Create(1);
                workbook.Version = ExcelVersion.Excel2016;
                #region Task List

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 6;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "SL";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                xlsCol += 1;
                int TaskSequence = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Seq";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol += 1;
                colDueDate = xlsCol;
                sheet1.Range[xlsRow, colDueDate].Text = "Due Date";
                sheet1.Range[xlsRow, colDueDate].ColumnWidth = 12;
                xlsCol += 1;
                int colExpectedCompletionDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Expec. Compl. Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol += 1;
                int colCurrentStatus = xlsCol;
                sheet1.Range[xlsRow, colCurrentStatus].Text = "Current Status";
                sheet1.Range[xlsRow, colCurrentStatus].ColumnWidth = 12;

                xlsCol += 1;
                colTaskType = xlsCol;
                sheet1.Range[xlsRow, colTaskType].Text = "Task Type";
                sheet1.Range[xlsRow, colTaskType].ColumnWidth = 10;

                xlsCol += 1;
                colTask = xlsCol;
                sheet1.Range[xlsRow, colTask].Text = "Task";
                sheet1.Range[xlsRow, colTask].ColumnWidth = 70;

                xlsCol += 1;
                colAssignTo = xlsCol;
                sheet1.Range[xlsRow, colAssignTo].Text = "Assigned To";
                sheet1.Range[xlsRow, colAssignTo].ColumnWidth = 25;
                xlsCol += 1;
                int colLastChat = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Last Activity";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;

                xlsCol += 1;
                colCategory = xlsCol;
                sheet1.Range[xlsRow, colCategory].Text = "Category";
                sheet1.Range[xlsRow, colCategory].ColumnWidth = 14;

                xlsCol += 1;
                colSubCategory = xlsCol;
                sheet1.Range[xlsRow, colSubCategory].Text = "Sub Category";
                sheet1.Range[xlsRow, colSubCategory].ColumnWidth = 14;



                xlsCol += 1;
                colAssignBy = xlsCol;
                sheet1.Range[xlsRow, colAssignBy].Text = "Assigned By";
                sheet1.Range[xlsRow, colAssignBy].ColumnWidth = 25;

                xlsCol += 1;
                colDependentDt = xlsCol;
                sheet1.Range[xlsRow, colDependentDt].Text = "Dependent Date";
                sheet1.Range[xlsRow, colDependentDt].ColumnWidth = 12;

                xlsCol += 1;
                colDependentDatesOn = xlsCol;
                sheet1.Range[xlsRow, colDependentDatesOn].Text = "Dependent DatesOn";
                sheet1.Range[xlsRow, colDependentDatesOn].ColumnWidth = 25;

                xlsCol += 1;
                colTaskDependentOn = xlsCol;
                sheet1.Range[xlsRow, colTaskDependentOn].Text = "Task DependentOn";
                sheet1.Range[xlsRow, colTaskDependentOn].ColumnWidth = 25;


                xlsCol += 1;
                colCommitmentDate = xlsCol;
                sheet1.Range[xlsRow, colCommitmentDate].Text = "Commitment Date";
                sheet1.Range[xlsRow, colCommitmentDate].ColumnWidth = 12;


                xlsCol += 1;
                colClosingDate = xlsCol;
                sheet1.Range[xlsRow, colClosingDate].Text = "Closing Date";
                sheet1.Range[xlsRow, colClosingDate].ColumnWidth = 12;
                xlsCol += 1;
                int colClosedBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Closed By";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;

                xlsCol += 1;
                colEarlyBy = xlsCol;
                sheet1.Range[xlsRow, colEarlyBy].Text = "Early By";
                sheet1.Range[xlsRow, colEarlyBy].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colEarlyBy].ColumnWidth = 9;

                xlsCol += 1;
                colLateBy = xlsCol;
                sheet1.Range[xlsRow, colLateBy].Text = "Late By";
                sheet1.Range[xlsRow, colEarlyBy].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colLateBy].ColumnWidth = 9;

                xlsCol += 1;
                int colBuyer = xlsCol;
                sheet1.Range[xlsRow, colBuyer].Text = "Buyer";
                sheet1.Range[xlsRow, colBuyer].ColumnWidth = 12;

                xlsCol += 1;
                int colDepartment = xlsCol;
                sheet1.Range[xlsRow, colDepartment].Text = "Department";
                sheet1.Range[xlsRow, colDepartment].ColumnWidth = 12;

                xlsCol += 1;
                int colDivision = xlsCol;
                sheet1.Range[xlsRow, colDivision].Text = "Division";
                sheet1.Range[xlsRow, colDivision].ColumnWidth = 12;

                xlsCol += 1;
                colMasterOrderNo = xlsCol;
                sheet1.Range[xlsRow, colMasterOrderNo].Text = "Master Order No";
                sheet1.Range[xlsRow, colMasterOrderNo].ColumnWidth = 16;

                xlsCol += 1;
                colStyleNo = xlsCol;
                sheet1.Range[xlsRow, colStyleNo].Text = "Line Item";
                sheet1.Range[xlsRow, colStyleNo].ColumnWidth = 30;

                xlsCol += 1;
                colSONo = xlsCol;
                sheet1.Range[xlsRow, colSONo].Text = "SO No";
                sheet1.Range[xlsRow, colSONo].ColumnWidth = 30;

                xlsCol += 1;
                int colLIR = xlsCol;
                sheet1.Range[xlsRow, colLIR].Text = "Line Item Ref#";
                sheet1.Range[xlsRow, colLIR].ColumnWidth = 30;

                xlsCol += 1;
                int colSOQty = xlsCol;
                sheet1.Range[xlsRow, colSOQty].Text = "SO Qty";
                sheet1.Range[xlsRow, colSOQty].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colSOQty].ColumnWidth = 10;

                xlsCol += 1;
                colPRNo = xlsCol;
                sheet1.Range[xlsRow, colPRNo].Text = "PR No";
                sheet1.Range[xlsRow, colPRNo].ColumnWidth = 30;


                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------
                int StartRow = xlsRow;

                IStyle color1 = workbook.Styles.Add("E6F0FF"); color1.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#E6F0FF");
                IStyle color2 = workbook.Styles.Add("FFF4E6"); color2.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#FFF4E6");
                IStyle color3 = workbook.Styles.Add("F5FFE6"); color3.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#F5FFE6");
                IStyle color4 = workbook.Styles.Add("52b3d9"); color4.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#52b3d9");
                IStyle color5 = workbook.Styles.Add("2ecc71"); color5.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#2ecc71");

                //Add rich-text Excel comment
                IFont fontCaption = workbook.CreateFont();
                fontCaption.Size = 8f;
                IFont fontRegular = workbook.CreateFont();
                fontRegular.Italic = true;
                fontRegular.Size = 6f;
                for (int i = 0; i < dtTNA.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------
                    if (SLNo == 65531)
                    {

                    }

                    sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet1.Range[xlsRow, TaskSequence].Number = clsStaticInfo.dbl(dtTNA.Rows[i]["Sequence"].ToString());
                    sheet1.Range[xlsRow, colTaskType].Text = dtTNA.Rows[i]["TaskType"].ToString();
                    sheet1.Range[xlsRow, colTask].Text = dtTNA.Rows[i]["Task"].ToString();
                    sheet1.Range[xlsRow, colAssignBy].Text = dtTNA.Rows[i]["AssignBy"].ToString();
                    sheet1.Range[xlsRow, colDependentDt].Text = dtTNA.Rows[i]["DependentDate"].ToString();
                    sheet1.Range[xlsRow, colDependentDatesOn].Text = dtTNA.Rows[i]["DependentDatesEnum"].ToString();
                    sheet1.Range[xlsRow, colTaskDependentOn].Text = dtTNA.Rows[i]["TaskDependentOn"].ToString();
                    sheet1.Range[xlsRow, colAssignTo].Text = dtTNA.Rows[i]["AssignTo"].ToString();
                    sheet1.Range[xlsRow, colClosedBy].Text = dtTNA.Rows[i]["ClosedBy"].ToString();
                    
                    sheet1.Range[xlsRow, colDueDate].Text = dtTNA.Rows[i]["DueDate"].ToString();
                    sheet1.Range[xlsRow, colExpectedCompletionDate].Text = dtTNA.Rows[i]["TempEndDate"].ToString();
                    sheet1.Range[xlsRow, colClosingDate].Text = dtTNA.Rows[i]["ClosingDate"].ToString();
                    sheet1.Range[xlsRow, colCommitmentDate].Text = dtTNA.Rows[i]["CommitmentDate"].ToString();

                    //clsStaticInfo.SetDate(sheet1[xlsRow, colDueDate], dtTNA.Rows[i]["DueDate"].ToString());
                    //clsStaticInfo.SetDate(sheet1[xlsRow, colExpectedCompletionDate], dtTNA.Rows[i]["TempEndDate"].ToString());
                    //clsStaticInfo.SetDate(sheet1[xlsRow, colClosingDate], dtTNA.Rows[i]["ClosingDate"].ToString());
                    //clsStaticInfo.SetDate(sheet1[xlsRow, colCommitmentDate], dtTNA.Rows[i]["CommitmentDate"].ToString());

                    sheet1.Range[xlsRow, colLastChat].Text = dtTNA.Rows[i]["LastChat"].ToString();


                    sheet1.Range[xlsRow, colBuyer].Text = dtTNA.Rows[i]["Buyer"].ToString();
                    sheet1.Range[xlsRow, colDepartment].Text = dtTNA.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, colDivision].Text = dtTNA.Rows[i]["Division"].ToString();
                    sheet1.Range[xlsRow, colCurrentStatus].Text = dtTNA.Rows[i]["CurrentStatus"].ToString();

                    sheet1.Range[xlsRow, colSOQty].Number = clsStaticInfo.dbl(clsStaticInfo.dbl(dtTNA.Rows[i]["SOQty"].ToString()).ToString("F0"));

                    sheet1.Range[xlsRow, colMasterOrderNo].Text = dtTNA.Rows[i]["MasterOrderId"].ToString();

                    sheet1.Range[xlsRow, colStyleNo].Text = dtTNA.Rows[i]["StyleNo"].ToString();

                    sheet1.Range[xlsRow, colSONo].Text = dtTNA.Rows[i]["SONo"].ToString();
                    sheet1.Range[xlsRow, colLIR].Text = dtTNA.Rows[i]["LineItemReference"].ToString();

                    sheet1.Range[xlsRow, colPRNo].Text = dtTNA.Rows[i]["PRNo"].ToString();

                    sheet1.Range[xlsRow, colSubCategory].Text = dtTNA.Rows[i]["SubCategory"].ToString();

                    sheet1.Range[xlsRow, colCategory].Text = dtTNA.Rows[i]["Category"].ToString();

                    double earlyOrLate = clsStaticInfo.dbl(dtTNA.Rows[i]["EarlyOrLateBy"].ToString());

                    double earlyBy = 0;
                    double lateBy = 0;
                    if (earlyOrLate < 0)
                    {
                        earlyBy = Math.Abs(earlyOrLate);
                    }
                    else if (earlyOrLate > 0)
                    {
                        lateBy = Math.Abs(earlyOrLate);
                    }


                    try
                    {


                        //today's task
                        DateTime DueDate = Convert.ToDateTime(dtTNA.Rows[i]["DueDate"].ToString());
                        DateTime CurrentDate = Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy"));
                        if (DueDate == CurrentDate)
                            sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle = color1;//.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#E6F0FF");


                        //overdue
                        if (DueDate < CurrentDate)
                            sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle = color2;//.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#FFF4E6");

                        //overdue
                        if (DueDate > CurrentDate)
                            sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle = color3;//.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#F5FFE6");




                        if (dtTNA.Rows[i]["CurrentStatus"].ToString().ToUpper() == "CLOSED")
                        {
                            DateTime ClosingDate = Convert.ToDateTime(dtTNA.Rows[i]["ClosingDate"].ToString());
                            //late closed
                            if (DueDate < ClosingDate)
                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle = color4;//.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#52b3d9");



                            //early closed
                            if (DueDate >= ClosingDate)
                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle = color5;//.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#2ecc71");

                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    #region Comments

                    if (dicComments.ContainsKey(dtTNA.Rows[i]["TaskMasterId"].ToString()))
                    {
                        IRange range = sheet1[xlsRow, colTask];
                        ICommentShape shape = range.AddComment();

                        for (int COMM = 0; COMM < dicComments[dtTNA.Rows[i]["TaskMasterId"].ToString()].Count; COMM++)
                        {
                            DataRow drTempComment = dicComments[dtTNA.Rows[i]["TaskMasterId"].ToString()][COMM];
                            shape.RichText.Append(drTempComment["CommentedBy"].ToString() + " says :" + drTempComment["CommentText"].ToString(), fontCaption);
                            shape.RichText.Append(" " + drTempComment["CreatedTime"].ToString() + Environment.NewLine + Environment.NewLine, fontRegular);
                            shape.IsTextLocked = false;
                            shape.AutoSize = false;

                            shape.Height += 30;
                            shape.Width = 300;
                        }

                    }

                    #endregion Comments

                    sheet1.Range[xlsRow, colEarlyBy].Number = earlyBy;
                    sheet1.Range[xlsRow, colLateBy].Number = lateBy;

                    xlsRow++;
                    SLNo++;
                }
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;
                //sheet1.Range[StartRow, 1, xlsRow - 1, endXlsCol].CellStyle.Font.Size = 8f;
                ////sheet1.AutoFilters.FilterRange = sheet1.Range[StartRow - 1, 1, xlsRow, endXlsCol];
                ////Specify first condition

                //sheet1.Range[StartRow, colClosingDate, xlsRow, colClosingDate].CellStyle.NumberFormat = "dd-MMM-yyyy";
                //sheet1.Range[StartRow, colCommitmentDate, xlsRow, colCommitmentDate].CellStyle.NumberFormat = "dd-MMM-yyyy";
                //sheet1.Range[StartRow, colDueDate, xlsRow, colDueDate].CellStyle.NumberFormat = "dd-MMM-yyyy";
                //sheet1.Range[StartRow, colExpectedCompletionDate, xlsRow, colExpectedCompletionDate].CellStyle.NumberFormat = "dd-MMM-yyyy";
                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************
                xlsRow = 1;
                FactoryName = string.Empty;

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
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "TNA List: ";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                
                #endregion ******************Report Header******************


                #region Freeze Panes

                //sheet1.IsDisplayZeros = false;
                //sheet1.UsedRange["A7"].FreezePanes();
                //sheet1.FirstVisibleColumn = 1;
                //sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment
                //sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet1.IsDisplayZeros = false;
                //sheet1.UsedRange.WrapText = true;
                //sheet1.Range["A1"].CellStyle.Font.Size = 14;
                //sheet1.Range["A2"].CellStyle.Font.Size = 10;
                //sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + UserName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Task List";

                #endregion Page Setup

                #endregion  ManualOutTime



                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook GetTNAStatusReportException(string CompanyGroupId, string CompanyId, string PlantId, string PlantName, string EmployeeId, string UserName, Dictionary<string, object> Filter, List<Dictionary<string, object>> FilterFields)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();

            DataTable dtTNA = null;

            DataSet dsCmp = null;

            DataSet dsFactory = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string FactoryAddress = string.Empty;
            string OTConsiderOn = string.Empty;
            #endregion

            try
            {
                objRpt = new clsReport();


                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);

                #region Get Data Query
                GetTNAStatusReportsDataException(out dtTNA, Filter, FilterFields);
                if (dtTNA.Rows.Count == 0)
                    throw new Exception("No data found");
                Dictionary<string, List<DataRow>> dicComments = GetSqlTaskComments(Filter, FilterFields);
                #endregion

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";

                var isl = 0;
                var SLNo = 1;

                int colTaskType = 0;
                int colTask = 0;
                int colAssignBy = 0;
                int colAssignTo = 0;
                int colDueDate = 0;
                int colCommitmentDate = 0;
                int colMasterOrderNo = 0;
                int colStyleNo = 0;
                int colSONo = 0;
                int colPRNo = 0;
                int colSubCategory = 0;
                int colCategory = 0;
                int colEarlyBy = 0;
                int colLateBy = 0;
                int colClosingDate = 0;

                objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);

                objRpt.SelectedPlant(PlantId, out dsFactory);

                workbook = application.Workbooks.Create(1);

                #region Task List

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 6;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "SL";
                sheet1.Range[xlsRow, isl].ColumnWidth = 7;
                xlsCol += 1;
                int TaskSequence = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Seq";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                xlsCol += 1;
                colDueDate = xlsCol;
                sheet1.Range[xlsRow, colDueDate].Text = "Due Date";
                sheet1.Range[xlsRow, colDueDate].ColumnWidth = 12;
                xlsCol += 1;
                int colExpectedCompletionDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Expec. Compl. Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol += 1;
                int colCurrentStatus = xlsCol;
                sheet1.Range[xlsRow, colCurrentStatus].Text = "Current Status";
                sheet1.Range[xlsRow, colCurrentStatus].ColumnWidth = 12;

                xlsCol += 1;
                colTaskType = xlsCol;
                sheet1.Range[xlsRow, colTaskType].Text = "Task Type";
                sheet1.Range[xlsRow, colTaskType].ColumnWidth = 10;

                xlsCol += 1;
                colTask = xlsCol;
                sheet1.Range[xlsRow, colTask].Text = "Task";
                sheet1.Range[xlsRow, colTask].ColumnWidth = 70;

                xlsCol += 1;
                colAssignTo = xlsCol;
                sheet1.Range[xlsRow, colAssignTo].Text = "Assigned To";
                sheet1.Range[xlsRow, colAssignTo].ColumnWidth = 25;
                xlsCol += 1;
                int colLastChat = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Last Activity";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;

                xlsCol += 1;
                colCategory = xlsCol;
                sheet1.Range[xlsRow, colCategory].Text = "Category";
                sheet1.Range[xlsRow, colCategory].ColumnWidth = 14;

                xlsCol += 1;
                colSubCategory = xlsCol;
                sheet1.Range[xlsRow, colSubCategory].Text = "Sub Category";
                sheet1.Range[xlsRow, colSubCategory].ColumnWidth = 14;



                xlsCol += 1;
                colAssignBy = xlsCol;
                sheet1.Range[xlsRow, colAssignBy].Text = "Assigned By";
                sheet1.Range[xlsRow, colAssignBy].ColumnWidth = 25;




                xlsCol += 1;
                colCommitmentDate = xlsCol;
                sheet1.Range[xlsRow, colCommitmentDate].Text = "Commitment Date";
                sheet1.Range[xlsRow, colCommitmentDate].ColumnWidth = 12;


                xlsCol += 1;
                colClosingDate = xlsCol;
                sheet1.Range[xlsRow, colClosingDate].Text = "Closing Date";
                sheet1.Range[xlsRow, colClosingDate].ColumnWidth = 12;
                xlsCol += 1;
                int colClosedBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Closed By";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol += 1;
                colEarlyBy = xlsCol;
                sheet1.Range[xlsRow, colEarlyBy].Text = "Early By";
                sheet1.Range[xlsRow, colEarlyBy].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colEarlyBy].ColumnWidth = 9;

                xlsCol += 1;
                colLateBy = xlsCol;
                sheet1.Range[xlsRow, colLateBy].Text = "Late By";
                sheet1.Range[xlsRow, colEarlyBy].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colLateBy].ColumnWidth = 9;

                xlsCol += 1;
                int colBuyer = xlsCol;
                sheet1.Range[xlsRow, colBuyer].Text = "Buyer";
                sheet1.Range[xlsRow, colBuyer].ColumnWidth = 12;

                xlsCol += 1;
                int colDepartment = xlsCol;
                sheet1.Range[xlsRow, colDepartment].Text = "Department";
                sheet1.Range[xlsRow, colDepartment].ColumnWidth = 12;

                xlsCol += 1;
                int colDivision = xlsCol;
                sheet1.Range[xlsRow, colDivision].Text = "Division";
                sheet1.Range[xlsRow, colDivision].ColumnWidth = 12;

                xlsCol += 1;
                colMasterOrderNo = xlsCol;
                sheet1.Range[xlsRow, colMasterOrderNo].Text = "Master Order No";
                sheet1.Range[xlsRow, colMasterOrderNo].ColumnWidth = 16;

                xlsCol += 1;
                colStyleNo = xlsCol;
                sheet1.Range[xlsRow, colStyleNo].Text = "Line Item";
                sheet1.Range[xlsRow, colStyleNo].ColumnWidth = 30;

                xlsCol += 1;
                colSONo = xlsCol;
                sheet1.Range[xlsRow, colSONo].Text = "SO No";
                sheet1.Range[xlsRow, colSONo].ColumnWidth = 30;
                xlsCol += 1;
                int colSOQty = xlsCol;
                sheet1.Range[xlsRow, colSOQty].Text = "SO Qty";
                sheet1.Range[xlsRow, colSOQty].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colSOQty].ColumnWidth = 10;

                xlsCol += 1;
                colPRNo = xlsCol;
                sheet1.Range[xlsRow, colPRNo].Text = "PR No";
                sheet1.Range[xlsRow, colPRNo].ColumnWidth = 30;


                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------
                int StartRow = xlsRow;

                IStyle color1 = workbook.Styles.Add("E6F0FF"); color1.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#E6F0FF");
                IStyle color2 = workbook.Styles.Add("FFF4E6"); color2.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#FFF4E6");
                IStyle color3 = workbook.Styles.Add("F5FFE6"); color3.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#F5FFE6");
                IStyle color4 = workbook.Styles.Add("52b3d9"); color4.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#52b3d9");
                IStyle color5 = workbook.Styles.Add("2ecc71"); color5.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#2ecc71");

                //Add rich-text Excel comment
                IFont fontCaption = workbook.CreateFont();
                fontCaption.Size = 8f;
                IFont fontRegular = workbook.CreateFont();
                fontRegular.Italic = true;
                fontRegular.Size = 6f;
                for (int i = 0; i < dtTNA.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------
                    sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet1.Range[xlsRow, TaskSequence].Number = clsStaticInfo.dbl(dtTNA.Rows[i]["Sequence"].ToString());

                    sheet1.Range[xlsRow, colTaskType].Text = dtTNA.Rows[i]["TaskType"].ToString();
                    sheet1.Range[xlsRow, colTask].Text = dtTNA.Rows[i]["Task"].ToString();
                    sheet1.Range[xlsRow, colAssignBy].Text = dtTNA.Rows[i]["AssignBy"].ToString();
                    sheet1.Range[xlsRow, colAssignTo].Text = dtTNA.Rows[i]["AssignTo"].ToString();
                    sheet1.Range[xlsRow, colClosedBy].Text = dtTNA.Rows[i]["ClosedBy"].ToString();

                    clsStaticInfo.SetDate(sheet1[xlsRow, colDueDate], dtTNA.Rows[i]["DueDate"].ToString());
                    clsStaticInfo.SetDate(sheet1[xlsRow, colExpectedCompletionDate], dtTNA.Rows[i]["TempEndDate"].ToString());
                    clsStaticInfo.SetDate(sheet1[xlsRow, colClosingDate], dtTNA.Rows[i]["ClosingDate"].ToString());
                    clsStaticInfo.SetDate(sheet1[xlsRow, colCommitmentDate], dtTNA.Rows[i]["CommitmentDate"].ToString());

                    sheet1.Range[xlsRow, colLastChat].Text = dtTNA.Rows[i]["LastChat"].ToString();


                    sheet1.Range[xlsRow, colBuyer].Text = dtTNA.Rows[i]["Buyer"].ToString();
                    sheet1.Range[xlsRow, colDepartment].Text = dtTNA.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, colDivision].Text = dtTNA.Rows[i]["Division"].ToString();
                    sheet1.Range[xlsRow, colCurrentStatus].Text = dtTNA.Rows[i]["CurrentStatus"].ToString();

                    sheet1.Range[xlsRow, colSOQty].Number = clsStaticInfo.dbl(clsStaticInfo.dbl(dtTNA.Rows[i]["SOQty"].ToString()).ToString("F0"));

                    sheet1.Range[xlsRow, colMasterOrderNo].Text = dtTNA.Rows[i]["MasterOrderId"].ToString();

                    sheet1.Range[xlsRow, colStyleNo].Text = dtTNA.Rows[i]["StyleNo"].ToString();

                    sheet1.Range[xlsRow, colSONo].Text = dtTNA.Rows[i]["SONo"].ToString();

                    sheet1.Range[xlsRow, colPRNo].Text = dtTNA.Rows[i]["PRNo"].ToString();

                    sheet1.Range[xlsRow, colSubCategory].Text = dtTNA.Rows[i]["SubCategory"].ToString();

                    sheet1.Range[xlsRow, colCategory].Text = dtTNA.Rows[i]["Category"].ToString();

                    double earlyOrLate = clsStaticInfo.dbl(dtTNA.Rows[i]["EarlyOrLateBy"].ToString());

                    double earlyBy = 0;
                    double lateBy = 0;
                    if (earlyOrLate < 0)
                    {
                        earlyBy = Math.Abs(earlyOrLate);
                    }
                    else if (earlyOrLate > 0)
                    {
                        lateBy = Math.Abs(earlyOrLate);
                    }


                    try
                    {


                        //today's task
                        DateTime DueDate = Convert.ToDateTime(dtTNA.Rows[i]["DueDate"].ToString());
                        DateTime CurrentDate = Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy"));
                        if (DueDate == CurrentDate)
                            sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle = color1;//.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#E6F0FF");


                        //overdue
                        if (DueDate < CurrentDate)
                            sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle = color2;//.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#FFF4E6");

                        //overdue
                        if (DueDate > CurrentDate)
                            sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle = color3;//.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#F5FFE6");




                        if (dtTNA.Rows[i]["CurrentStatus"].ToString().ToUpper() == "CLOSED")
                        {
                            DateTime ClosingDate = Convert.ToDateTime(dtTNA.Rows[i]["ClosingDate"].ToString());
                            //late closed
                            if (DueDate < ClosingDate)
                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle = color4;//.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#52b3d9");



                            //early closed
                            if (DueDate >= ClosingDate)
                                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle = color5;//.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#2ecc71");

                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    #region Comments

                    if (dicComments.ContainsKey(dtTNA.Rows[i]["TaskMasterId"].ToString()))
                    {
                        IRange range = sheet1[xlsRow, colTask];
                        ICommentShape shape = range.AddComment();

                        for (int COMM = 0; COMM < dicComments[dtTNA.Rows[i]["TaskMasterId"].ToString()].Count; COMM++)
                        {
                            DataRow drTempComment = dicComments[dtTNA.Rows[i]["TaskMasterId"].ToString()][COMM];
                            shape.RichText.Append(drTempComment["CommentedBy"].ToString() + " says :" + drTempComment["CommentText"].ToString(), fontCaption);
                            shape.RichText.Append(" " + drTempComment["CreatedTime"].ToString() + Environment.NewLine + Environment.NewLine, fontRegular);
                            shape.IsTextLocked = false;
                            shape.AutoSize = false;

                            shape.Height += 30;
                            shape.Width = 300;
                        }

                    }

                    #endregion Comments

                    sheet1.Range[xlsRow, colEarlyBy].Number = earlyBy;
                    sheet1.Range[xlsRow, colLateBy].Number = lateBy;

                    xlsRow++;
                    SLNo++;
                }
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;
                sheet1.Range[StartRow, 1, xlsRow - 1, endXlsCol].CellStyle.Font.Size = 8f;
                sheet1.AutoFilters.FilterRange = sheet1.Range[StartRow - 1, 1, xlsRow, endXlsCol];
                //Specify first condition

                sheet1.Range[StartRow, colClosingDate, xlsRow, colClosingDate].CellStyle.NumberFormat = "dd-MMM-yyyy";
                sheet1.Range[StartRow, colCommitmentDate, xlsRow, colCommitmentDate].CellStyle.NumberFormat = "dd-MMM-yyyy";
                sheet1.Range[StartRow, colDueDate, xlsRow, colDueDate].CellStyle.NumberFormat = "dd-MMM-yyyy";
                sheet1.Range[StartRow, colExpectedCompletionDate, xlsRow, colExpectedCompletionDate].CellStyle.NumberFormat = "dd-MMM-yyyy";
                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************
                xlsRow = 1;
                FactoryName = string.Empty;

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
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Exception TNA List: ";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************


                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment
                sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange.WrapText = true;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + UserName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Task List";
                #endregion Page Setup

                #endregion  ManualOutTime



                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}