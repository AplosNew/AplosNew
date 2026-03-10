#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.OrderManagements;
using Library.OrderManagement.Production;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.OrderManagements;

using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class DispatchMasterController : BaseController
    {
        #region Constructor
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly ISqlRepository _sqlRepository;
        public DispatchMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult DispatchPlan()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetDispatchEntityProcessSettingData(string EntityId)
        {
            return Json(_productionSummaryData.GetDispatchEntityProcessSettingData(EntityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Insert(Dictionary<string, object> data, List<Dictionary<string, object>> selectedSalesOrderList)
        {
            SaveData(data, selectedSalesOrderList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        private void SaveData(Dictionary<string, object> data, List<Dictionary<string, object>> selectedSalesOrderList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsMaster, dsDispatchDetail, dsDispatchDetailSO, dsDispatchSKUMaster, dsDispatchSKUDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[DispatchMaster] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id, _DispatchDetailId = "";
                string masterId = "";
                string dispatchDetailId = "";

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchMaster", out _Id);

                    data["Id"] = "DM" + _Id;
                    data["PlantId"] = identity.PlantId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["PlantId"] = identity.PlantId;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.DispatchDetail WHERE DispatchMasterId ='" + data["Id"] + "'", out dsDispatchDetail, false, "1");

                if (dsDispatchDetail.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchDetail", out _DispatchDetailId);

                    DataRow dr = dsDispatchDetail.Tables[0].NewRow();

                    dr["Id"] = "DD" + _DispatchDetailId;
                    dr["DispatchMasterId"] = masterId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsDispatchDetail.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsDispatchDetail.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                dispatchDetailId = dsDispatchDetail.Tables[0].Rows[0]["Id"].ToString();

                // DispatchDetailSO
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.DispatchDetailSO WHERE DispatchDetailId ='" + dispatchDetailId + "'", out dsDispatchDetailSO, false, "1");

                foreach (var item in selectedSalesOrderList)
                {
                    DataView dv = new DataView(dsDispatchDetailSO.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = GetDispatchOrderItemPK();
                        item["DispatchDetailId"] = dispatchDetailId;

                        AddNewRow(dsDispatchDetailSO.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                }

                // DispatchSKUMaster & DispatchSKUDetail
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.DispatchSKUMaster WHERE DispatchDetailId ='" + dispatchDetailId + "'", out dsDispatchSKUMaster, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.DispatchSKUDetail WHERE 1=2", out dsDispatchSKUDetail, false, "1");

                DataTable Detail = _sqlRepository.GetDataTable("SELECT * FROM [dbo].[PackingContentMaster] where Id IN(SELECT PackingContentMasterId FROM [dbo].[PackingChild] Where IsConfirmed=1 AND ISNULL(DispatchSKUMasterId,'')='')");

                for (int i = 0; i < Detail.Rows.Count; i++)
                {
                    DataRow drDetailDestination = dsDispatchSKUMaster.Tables[0].NewRow();
                    CopyRow(Detail.Rows[i], ref drDetailDestination);
                    drDetailDestination["Id"] = dispatchDetailId + "-" + (i + 1);
                    drDetailDestination["DispatchDetailId"] = dispatchDetailId;
                    dsDispatchSKUMaster.Tables[0].Rows.Add(drDetailDestination);

                    //Process.DefaultView.RowFilter = "ProductionBulletinTemplateMasterId='" + Detail.Rows[i]["Id"].ToString() + "'";
                    //for (int K = 0; K < Process.DefaultView.Count; K++)
                    //{
                    //    GetOperationMasterByOperationVariation(Process.DefaultView[K].Row["OperationVariationId"].ToString(), out DataSet dsOperationMaster);

                    //    DataRow drDetailSKUDestination = ProductionBulletinTemplateDetail.Tables[0].NewRow();
                    //    CopyRow(Process.DefaultView[K].Row, ref drDetailSKUDestination);
                    //    drDetailSKUDestination["Id"] = NewId + "-" + (i + 1) + "-" + (K + 1);
                    //    drDetailSKUDestination["ProductionBulletinTemplateMasterId"] = NewId + "-" + (i + 1);

                    //    if (string.IsNullOrEmpty(dsOperationMaster.Tables[0].Rows[0]["OperationMasterId"].ToString()))
                    //    {
                    //        drDetailSKUDestination["OperationMasterId"] = DBNull.Value;
                    //    }
                    //    else
                    //    {
                    //        drDetailSKUDestination["OperationMasterId"] = dsOperationMaster.Tables[0].Rows[0]["OperationMasterId"].ToString();
                    //    }

                    //    ProductionBulletinTemplateDetail.Tables[0].Rows.Add(drDetailSKUDestination);
                    //}

                }


                clsStaticInfo obj1 = new clsStaticInfo();
                obj1.SaveDataSets(dsMaster, dsDispatchDetail, dsDispatchDetailSO, dsDispatchSKUMaster);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = drSource[drSource.Table.Columns[COL].ColumnName];

                }
                catch (Exception ex)
                {
                }
                try
                {
                    drDestination["AddedBy"] = identity.Name;
                    drDestination["AddedDate"] = DateTime.Now;
                    drDestination["AddedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedBy"] = identity.Name;
                    drDestination["UpdatedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedDate"] = DateTime.Now;

                }
                catch (Exception ex)
                {
                }
            }

        }

        private string GetDispatchMaterialPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchMaterial", out sID);
            return sID;
        }
        private string GetDispatchOrderItemPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchDetailSO", out sID);
            return sID;
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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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

        [HttpGet, Authorize]
        public ActionResult GetSOList(string customerId)
        {
            try
            {
                return Json(_productionSummaryData.GetSOList(customerId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDispatchDetailSOList(string masterId)
        {
            try
            {
                return Json(_productionSummaryData.GetDispatchDetailSOList(masterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                return Json(_productionSummaryData.GetDispatchMasterList(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetAllConfirmedPackingContentData()
        {
            return Json(_productionSummaryData.GetAllConfirmedPackingContentData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPackingChildDataList(string MasterId)
        {
            string sql = @"SELECT 0 AS [Check],P.*, [State]=CASE WHEN P.IsConfirmed=1 THEN 1 ELSE 0 END FROM [dbo].[PackingChild] P WHERE P.PackingContentMasterId='" + MasterId + "' AND P.IsConfirmed=1";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #region DispatchPlan
        [HttpPost]
        public JsonResult DispatchPlanInsert(Dictionary<string, object> data)
        {
            SaveDispatchPlanData(data);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }
        private void SaveDispatchPlanData(Dictionary<string, object> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[DispatchPlanMaster] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id;
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DispatchPlanMaster", out _Id);

                    data["Id"] = "DP" + _Id;
                    data["PlantId"] = identity.PlantId;
                    data["ByWhom"] = identity.UserId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["PlantId"] = identity.PlantId;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = GetSampleFileServiceMaster(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Dispatch Plan upload Sample File";

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        }

        public DataTable GetServiceMasterGLData()
        {
            var cmdText = @"select p.code +' - '+p.UserName CustomerName,so.Id SoId,so.OrderStatusId SOStatus,pod.ProductionOrderId POId,ps.UserName POStatus
            ,dpc.DispatchPlanMasterId,so.Qty SOQty,dpc.DispatchPlanQty,so.Qty-dpc.DispatchPlanQty BalanceToDispatch
            ,((so.Qty-dpc.DispatchPlanQty)/ so.Qty)*100 BalancePercentage
            ,dpm.PlantId,dpm.YearNo,dpm.MonthNo,dpm.ResponsiblePersonId,oc.CriticalityLevel  OrderCriticalityLevel
            ,dpm.PlanNo ,NULL DispatchCommitmentDate,NULL DispatchCategory ,NULL  Remark,NULL OrderRemark
            FROM TRN.SalesOrder so  
            LEFT JOIN dbo.OrderControl oc on oc.SalesOrderId=so.Id
            LEFT JOIN trn.MasterOrderItem moi on moi.Id=so.MasterOrderItemId
            LEFT JOIN trn.MasterOrder mo on mo.Id=moi.MasterOrderId
            LEFT JOIN trn.ProductionOrderDetail pod on pod.SalesOrderId=so.Id
            LEFT JOIN trn.ProductionOrder po on po.Id=pod.ProductionOrderId
            LEFT JOIN hkp.ProductionStatus ps on ps.Id=po.ProductionStatusId 
            LEFT JOIN  dbo.DispatchPlanChild dpc on dpc.SOId=so.Id
            LEFT JOIN dbo.DispatchPlanMaster dpm on dpm.Id=dpc.DispatchPlanMasterId
            LEFT JOIN hkp.Party p on p.Id=mo.PartyId
            WHERE SO.OrderStatusId not in ('Closed','Cancelled')";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public void CreateSource(string[] Arr, int Col, string Header, ref IWorksheet sheetSource)
        {
            try
            {
                ReportUtility ru = new ReportUtility();
                ru.SetText(ref sheetSource, 1, Col, Header);
                for (int i = 0; i < Arr.Length; i++)
                {
                    var un = Arr[i].ToString();
                    int k = i + 2;
                    ru.SetText(ref sheetSource, k, Col, un);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook GetSampleFileServiceMaster(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName)
        {
            #region declare
            OTSBD.clsReport objRpt = null;
            OTSBD.clsStaticInfo objStatic = null;
            objStatic = new OTSBD.clsStaticInfo();
            string OTConsiderOn = string.Empty;

            #endregion
            try
            {
                ReportUtility ru = new ReportUtility();

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = ru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new OTSBD.clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;

                #region Lunch Out
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                IWorksheet sheetSource = null;
                sheetSource = workbook.Worksheets[1];
                xlsRow = 1;

                #region ------------------Column Header------------------


                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "CustomerName"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 16; int colCustomerName = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SoId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 11; int colSoId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SOStatus"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 11; int colSOStatus = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "POId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19; int colPOId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "POStatus"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15; int colPOStatus = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DispatchPlanMasterId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 22; int colDispatchPlanMasterId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SOQty"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 17; int colSOQty = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DispatchPlanQty"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30; int colDispatchPlanQty = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BalanceToDispatch"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 16; int colBalanceToDispatch = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BalancePercentage"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20; int colBalancePercentage = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PlantId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25; int colPlantId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "YearNo"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10; int colYearNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "MonthNo"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40; int colMonthNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ResponsiblePersonId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40; int colResponsiblePersonId = xlsCol;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OrderCriticalityLevel"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40; int colOrderCriticalityLevel = xlsCol;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PlanNo"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40; int colPlanNo = xlsCol;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DispatchCommitmentDate"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40; int colDispatchCommitmentDate = xlsCol;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DispatchCategory"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40; int colDispatchCategory = xlsCol;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remark"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40; int colRemark = xlsCol;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OrderRemark"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40; int colOrderRemark = xlsCol;
                //string[] _EntryLevel = { "Trainee", "NonTrainee" };
                //CreateSource(_EntryLevel, 20, "DispatchCategory", ref sheetSource); int colDispatchCategory = 20;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                xlsRow++;

                //sheet1.Range[xlsRow, colPurchaseApplicable, xlsRow, colPurchaseApplicable].DataValidation.AllowType = ExcelDataType.Integer;
                //sheet1.Range[xlsRow, colSalesApplicable, xlsRow, colSalesApplicable].DataValidation.AllowType = ExcelDataType.Integer;
                //sheet1.Range[xlsRow, colIndependentApplicable, xlsRow, colIndependentApplicable].DataValidation.AllowType = ExcelDataType.Integer;

                #endregion ------------------Column Header------------------

                DataTable dtData = GetServiceMasterGLData();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    sheet1[xlsRow, colCustomerName].Text = dtData.Rows[i]["CustomerName"].ToString();
                    sheet1[xlsRow, colSoId].Text = dtData.Rows[i]["SoId"].ToString();
                    sheet1[xlsRow, colSOStatus].Text = dtData.Rows[i]["SOStatus"].ToString();
                    sheet1[xlsRow, colPOId].Text = dtData.Rows[i]["POId"].ToString();
                    sheet1[xlsRow, colPOStatus].Text = dtData.Rows[i]["POStatus"].ToString();
                    sheet1[xlsRow, colDispatchPlanMasterId].Text = dtData.Rows[i]["DispatchPlanMasterId"].ToString();
                    sheet1[xlsRow, colSOQty].Text = dtData.Rows[i]["SOQty"].ToString();
                    sheet1[xlsRow, colDispatchPlanQty].Text = dtData.Rows[i]["DispatchPlanQty"].ToString();
                    sheet1[xlsRow, colBalanceToDispatch].Text = dtData.Rows[i]["BalanceToDispatch"].ToString();
                    sheet1[xlsRow, colBalancePercentage].Text = dtData.Rows[i]["BalancePercentage"].ToString();
                    sheet1[xlsRow, colPlantId].Text = dtData.Rows[i]["PlantId"].ToString();
                    sheet1[xlsRow, colYearNo].Text = dtData.Rows[i]["YearNo"].ToString();
                    sheet1[xlsRow, colMonthNo].Text = dtData.Rows[i]["MonthNo"].ToString();
                    sheet1[xlsRow, colResponsiblePersonId].Text = dtData.Rows[i]["ResponsiblePersonId"].ToString();
                    sheet1[xlsRow, colOrderCriticalityLevel].Text = dtData.Rows[i]["OrderCriticalityLevel"].ToString();
                    sheet1[xlsRow, colPlanNo].Text = dtData.Rows[i]["PlanNo"].ToString();
                    sheet1[xlsRow, colDispatchCommitmentDate].Text = dtData.Rows[i]["DispatchCommitmentDate"].ToString();
                    sheet1[xlsRow, colOrderRemark].Text = dtData.Rows[i]["OrderRemark"].ToString();
                    sheet1[xlsRow, colDispatchCategory].Text = dtData.Rows[i]["DispatchCategory"].ToString();
                    //ru.SetList(ref sheet1, xlsRow, 20, xlsCol, sheetSource, colDispatchCategory, _EntryLevel.Length); xlsCol += 1;
                    xlsRow++;
                }


                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 10;
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
                sheet1.Name = "Sheet1";
                #endregion Page Setup

                #endregion  Lunch Out

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
        #endregion
    }

}