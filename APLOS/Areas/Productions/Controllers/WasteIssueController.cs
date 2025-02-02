#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.OrderManagement.Production;
using Library.Data;
using System.Reflection;
using Library.Service.Logs;
using Library.Service.Processes;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class WasteIssueController : BaseController
    {
        private readonly IProcessService _processService;
        WasteMasterService ws = new WasteMasterService();
        string TableName = "dbo.WasteMaster";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public WasteIssueController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }


        [Authorize, HttpPost]
        public ActionResult GetWaste(string entityId, string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select CAST(case when WID.Id IS NULL then 0 else 1 end as bit) Active,WID.Id,WTD.Id WasteTransactionDataId,ROW_NUMBER() OVER (ORDER BY WID.Id) AS Sequence,WM.Category WasteCategory,WM.SubCategory WasteSubCategory,WM.ItemName
				                    ,UOM.UserName UOM,WTD.Quantity StockQty,WM.StandardRate StdRate,(WTD.Quantity*WM.StandardRate) StdValue
				                    ,ISNULL(WID.IssueQty,0) IssueQty,ISNULL(WID.Rate,0) Rate,WID.ProcessId,P.UserName Process,WID.Remarks,(ISNULL(WID.IssueQty,0) * ISNULL(WID.Rate,0))as IssueValue
									,ISNULL((WTD.Quantity-(WID.IssueQty+ISNULL(WIDS.OtherQty,0))),0) as BalanceStock
									,((ISNULL(WTD.Quantity,0)*ISNULL(WM.StandardRate,0))-(ISNULL(WID.IssueQty,0)*ISNULL(WID.Rate,0))) as BalanceStkValue,ISNULL(WIDS.OtherQty,0) OtherQty
									,WTD.WasteLocationId,MS.UserName WasteLocation
				                    from WasteTransactionData WTD
									left join HKP.MaterialStorage MS on MS.Id=WTD.WasteLocationId
				                    left join WasteMaster WM on WM.Id=WTD.WasteMasterId
				                    left join SCS.UnitOfMeasurement UOM on UOM.Id=WM.UOMId
									LEFT JOIN WasteIssueDetails WID ON WID.WasteTransactionDataId=WTD.Id AND WID.WasteIssueId='" + Id + @"'
									LEFT JOIN (select sum(IssueQty) OtherQty,WasteTransactionDataId,WasteIssueId from WasteIssueDetails group by WasteTransactionDataId,WasteIssueId) WIDS ON WIDS.WasteTransactionDataId=WTD.Id
									AND WIDS.WasteIssueId<> '" + Id + @"'

                                    left join HKP.Process P on P.Id=WID.ProcessId
				                    where EntityId='" + entityId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }



        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

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

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
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

        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> WasteData)
        {


            try
            {
                DataSet dsWasteDetail, dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from WasteIssue where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";
                string MasterId = string.Empty;
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "WasteIssue", out _Id);

                    data["Id"] = _Id;
                    MasterId = data["Id"].ToString();
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    MasterId = _Id;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                string id = "";
                for (int i = 0; i < WasteData.Count; i++)
                {
                    if (id == "")
                    {
                        id = "'" + WasteData[i]["Id"] + "'";
                    }
                    else
                    {
                        id += ",'" + WasteData[i]["Id"] + "'";
                    }
                }

                con.OpenDataSetThroughAdapter("select * from WasteIssueDetails where Id in (" + id + ")", out dsWasteDetail, false, "1");

                string WasteId = "";
                for (int i = 0; i < WasteData.Count; i++)
                {
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    dsWasteDetail.Tables[0].DefaultView.RowFilter = "Id='" + WasteData[i]["Id"] + @"'";
                    if (dsWasteDetail.Tables[0].DefaultView.Count > 0)
                    {
                        //edit
                        DataRow dr = dsWasteDetail.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["WasteIssueId"] = MasterId;
                        dr["WasteTransactionDataId"] = WasteData[i]["WasteTransactionDataId"];
                        dr["IssueQty"] = WasteData[i]["IssueQty"];
                        dr["Rate"] = WasteData[i]["Rate"];
                        dr["IssueValue"] = WasteData[i]["IssueValue"];
                        dr["ProcessId"] = WasteData[i]["ProcessId"];
                        dr["Remarks"] = WasteData[i]["Remarks"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                    else
                    {
                        //addnew

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("WasteIssueDetails", out WasteId);

                        DataRow dr = dsWasteDetail.Tables[0].NewRow();

                        dr["Id"] = "M-" + WasteId + "-" + (i + 1);
                        dr["WasteIssueId"] = MasterId;
                        dr["WasteTransactionDataId"] = WasteData[i]["WasteTransactionDataId"];
                        dr["IssueQty"] = WasteData[i]["IssueQty"];
                        dr["Rate"] = WasteData[i]["Rate"];
                        dr["IssueValue"] = WasteData[i]["IssueValue"];
                        dr["ProcessId"] = WasteData[i]["ProcessId"];
                        dr["Remarks"] = WasteData[i]["Remarks"];
                        
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsWasteDetail.Tables[0].Rows.Add(dr);

                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsWasteDetail);
                return Json(new { Error = false, Message = AplosMessage.Updated, Id = _Id });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            string strUSQL, wasteSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                //talkingSQL = "delete from dbo.MeetingTalkingPoint Where MeetingItemHeaderId='" + id + "'";
                //suggestionSQL = "delete from dbo.MeetingSuggestion Where MeetingItemHeaderId='" + id + "'";
                //actionSQL = "delete from dbo.MeetingActionablePoints Where MeetingItemHeaderId='" + id + "'";
                wasteSQL = "delete from dbo.WasteIssueDetails Where WasteIssueId='" + id + "'";
                strUSQL = "delete dbo.WasteIssue Where Id='" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                //objCon.ExecuteNonQueryWrapper(talkingSQL, true, "1");
                //objCon.ExecuteNonQueryWrapper(suggestionSQL, true, "1");
                //objCon.ExecuteNonQueryWrapper(actionSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(wasteSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strUSQL, true, "1");
                objCon.CommitTransaction();

                return Json(new { Message = AplosMessage.Deleted });
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }


        [Authorize, HttpPost]
        public ActionResult getEntity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"Select e.Id as EntityId, e.UserName as EntityName , p.UserName as Plant, c.UserName as Company from org.Entity e
                                left join org.Plant p on p.Id = e.PlantId
                                left join org.Company c on c.Id = p.CompanyId";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeListByWhom(GridParameter parameters, string plantId, string partyAccountGroupId, string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId))
            {
                plantId = identity.PlantId;
            }
            return Json(GetEmployeeListByWhom(parameters, identity.CompanyId, plantId, partyAccountGroupId, partyId), JsonRequestBehavior.AllowGet);
        }

        public GridModel GetEmployeeListByWhom(GridParameter parameters, string companyId, string plantId, string partyAccountGroupId, string partyId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.SystemId, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [Designation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.Designation AS DEG ON DEG.Id=EI.GivenDesignationID
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            WHERE EI.CompanyId='" + companyId + "' AND EI.PlantId='" + plantId + "' AND EI.EmployeeStatus='Active'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        [HttpPost,Authorize]
        public ActionResult GetWasteMasterData()
        {
            string sql = @"select WI.*,E.Id EntityId,E.UserName Entity,EI.EmployeeCode PreparedByCode,EI.EmployeeName PreparedBy,EmpI.EmployeeCode ApprovedByCode
												,EmpI.EmployeeName ApprovedBy,EmpInfo.EmployeeCode CheckedByCode,EmpInfo.EmployeeName CheckedBy
					                            from WasteIssue WI
					                            left join ORG.Entity E on E.Id=WI.EntityId
					                            left join EmployeeInformation EI on EI.SystemId=WI.PreparedById
					                            left join EmployeeInformation EmpI on EmpI.SystemId=WI.ApprovedById
					                            left join EmployeeInformation EmpInfo on EmpInfo.SystemId=WI.CheckedById";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetWasteReport(string Id)
        {
            try
            {
                string fileName = "";
                fileName = WasteReport(Id, "WasteReport");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string WasteReport(string Id, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                var report = new ReportUtility();
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Waste Report";
                sheet = workbook.Worksheets[0];

                DataTable data= WasteReportSQL(Id);
                var header = WasteMasterReportSQL(Id);

                int ROW = 5; int COL = 1;

                #region Header
                report.SetMasterHeaderText(ref sheet, ROW, 1, "Issue Id");
                sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, header["IssueId"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(5) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 6, "Entity");
                sheet[ROW, 6].ColumnWidth = 25;
                sheet.Range[ROW, 6].VerticalAlignment = ExcelVAlign.VAlignTop; 
                sheet.Range[ROW, 6].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 7, header["Entity"].ToString());
                sheet[report.GetColumnNameForXls(7) + ROW + ":" + report.GetColumnNameForXls(10) + ROW].Merge();
                sheet[ROW, 7].ColumnWidth = 25;
                sheet.Range[ROW, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;


                report.SetMasterHeaderText(ref sheet, ROW, 1, "User Reference");
                sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, header["UserReference"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(5) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 6, "Purpose");
                sheet[ROW, 6].ColumnWidth = 25;
                sheet.Range[ROW, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 6].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 7, header["Purpose"].ToString());
                sheet[report.GetColumnNameForXls(7) + ROW + ":" + report.GetColumnNameForXls(10) + ROW].Merge();
                sheet[ROW, 7].ColumnWidth = 25;
                sheet.Range[ROW, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "Date");
                sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, header["Date"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(5) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 6, "Prepared By");
                sheet[ROW, 6].ColumnWidth = 25;
                sheet.Range[ROW, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 6].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 7, header["PreparedBy"].ToString());
                sheet[report.GetColumnNameForXls(7) + ROW + ":" + report.GetColumnNameForXls(10) + ROW].Merge();
                sheet[ROW, 7].ColumnWidth = 25;
                sheet.Range[ROW, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "Checked By");
                sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, header["CheckedBy"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(5) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                report.SetMasterHeaderText(ref sheet, ROW, 6, "Approved By");
                sheet[ROW, 6].ColumnWidth = 25;
                sheet.Range[ROW, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 6].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 7, header["ApprovedBy"].ToString());
                sheet[report.GetColumnNameForXls(7) + ROW + ":" + report.GetColumnNameForXls(10) + ROW].Merge();
                sheet[ROW, 7].ColumnWidth = 25;
                sheet.Range[ROW, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
                ROW++;

                report.SetMasterHeaderText(ref sheet, ROW, 1, "Remark");
                sheet[ROW, 1].ColumnWidth = 20;
                sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                report.SetText(ref sheet, ROW, 2, header["Remark"].ToString());
                sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(5) + ROW].Merge();
                sheet[ROW, 2].ColumnWidth = 20;
                sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                ROW++;
                ROW++;

                #endregion

                #region columns
                sheet[ROW, COL].Text = "Id";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColId = COL;
                COL++;

                sheet[ROW, COL].Text = "Sequence";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColSequence = COL;
                COL++;

                sheet[ROW, COL].Text = "ItemName";
                sheet[ROW, COL].ColumnWidth = 30;
                int ColItemName = COL;
                COL++;
                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColProcess = COL;
                COL++;
                sheet[ROW, COL].Text = "Waste Category";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColWasteCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Waste Sub Category";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColWasteSubCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Stock Quantity";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColStockQty = COL;
                COL++;
                sheet[ROW, COL].Text = "UOM";
                sheet[ROW, COL].ColumnWidth = 8;
                int ColUOM = COL;
                COL++;
                sheet[ROW, COL].Text = "Standard Rate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColStdRate = COL;
                COL++;
                sheet[ROW, COL].Text = "Standard Value";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColStdValue = COL;
                COL++;
                sheet[ROW, COL].Text = "Issue Quantity";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColIssueQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Balance Stock";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColBalanceStock = COL;
                COL++;
                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColRate = COL;
                COL++;
                sheet[ROW, COL].Text = "Issue Value";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColIssueValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Balance Stock Value";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColBalanceStockValue = COL;
                

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
                    sheet[ROW, ColId].Text = data.Rows[i]["Id"].ToString();
                    sheet[ROW, ColSequence].Text = data.Rows[i]["Sequence"].ToString();
                    sheet[ROW, ColWasteCategory].Text = data.Rows[i]["WasteCategory"].ToString();
                    sheet[ROW, ColWasteSubCategory].Text = data.Rows[i]["WasteSubCategory"].ToString();
                    sheet[ROW, ColItemName].Text = data.Rows[i]["ItemName"].ToString();
                    sheet[ROW, ColUOM].Text = data.Rows[i]["UOM"].ToString();
                    
                    sheet[ROW, ColStockQty].Number = clsStaticInfo.dbl(data.Rows[i]["StockQty"].ToString());
                    sheet[ROW, ColStockQty].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet[ROW, ColStdRate].Number = clsStaticInfo.dbl(data.Rows[i]["StdRate"].ToString());
                    sheet[ROW, ColStdRate].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet[ROW, ColStdValue].Number = clsStaticInfo.dbl(data.Rows[i]["StdValue"].ToString());
                    sheet[ROW, ColStdValue].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet[ROW, ColIssueQty].Number = clsStaticInfo.dbl(data.Rows[i]["IssueQty"].ToString());
                    sheet[ROW, ColIssueQty].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet[ROW, ColBalanceStock].Number = clsStaticInfo.dbl(data.Rows[i]["BalanceStock"].ToString());
                    sheet[ROW, ColBalanceStock].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data.Rows[i]["Rate"].ToString());
                    sheet[ROW, ColRate].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet[ROW, ColIssueValue].Number = clsStaticInfo.dbl(data.Rows[i]["IssueValue"].ToString());
                    sheet[ROW, ColIssueValue].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);


                    sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                    sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();

                    sheet[ROW, ColBalanceStockValue].Number = clsStaticInfo.dbl(data.Rows[i]["BalanceStkValue"].ToString());
                    sheet[ROW, ColBalanceStockValue].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Waste Report", identity.PlantId);
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
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;




                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
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

        private DataTable WasteReportSQL(string Id)
        {
            try
            {

                string strSQL = @"select ROW_NUMBER() OVER (ORDER BY WID.Id) AS Sequence,WID.Id,WID.WasteIssueId,WM.Category WasteCategory,WM.SubCategory WasteSubCategory,WM.ItemName
									,UOM.UserName UOM,WTD.Quantity StockQty,WM.StandardRate StdRate,(WTD.Quantity*WM.StandardRate) StdValue
									,ISNULL(WID.IssueQty,0) IssueQty,ISNULL(WID.Rate,0) Rate,WID.ProcessId,P.UserName Process,WID.Remarks,(ISNULL(WID.IssueQty,0) * ISNULL(WID.Rate,0))as IssueValue
									,ISNULL((WTD.Quantity-WID.IssueQty),0) as BalanceStock
									,((ISNULL(WTD.Quantity,0)*ISNULL(WM.StandardRate,0))-(ISNULL(WID.IssueQty,0)*ISNULL(WID.Rate,0))) as BalanceStkValue
									
									from WasteIssueDetails WID
									left join WasteTransactionData WTD on WTD.Id=WID.WasteTransactionDataId
									left join WasteMaster WM on WM.Id=WTD.WasteMasterId
									left join SCS.UnitOfMeasurement UOM on UOM.Id=WM.UOMId
                                    left join HKP.Process P on P.Id=WID.ProcessId

									where WID.WasteIssueId='" + Id + @"'";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        
        private Dictionary<string,object> WasteMasterReportSQL(string Id)
        {
            try
            {

                string strSQL = @"select WI.Id IssueId,E.UserName Entity,WI.Purpose,FORMAT(WI.Date,'dd-MMM-yyyy') Date,EI.EmployeeName PreparedBy,EmpI.EmployeeName ApprovedBy
						                            ,EmpInfo.EmployeeName CheckedBy,WI.Remarks Remark,WI.UserReference
						                            from WasteIssue WI
						                            left join ORG.Entity E on E.Id=WI.EntityId
						                            left join EmployeeInformation EI on EI.SystemId=WI.PreparedById
						                            left join EmployeeInformation EmpI on EmpI.SystemId=WI.ApprovedById
						                            left join EmployeeInformation EmpInfo on EmpInfo.SystemId=WI.CheckedById

									                where WI.Id='" + Id + @"'";

                return _sqlRepository.GetData(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        [HttpGet, Authorize]
        public ActionResult DownloadUsingFullPath(string FullPath, string fileName)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                //string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(FullPath);
                try
                {
                    System.IO.File.Delete(FullPath);
                }
                catch (Exception)
                {
                }

                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;

            }
            catch (Exception ex)
            {


            }
            return null;
        }
    }
}