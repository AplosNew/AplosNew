using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Library.Planning.OrderManagement
{
    public class Order
    {
        private readonly SqlRepository _sqlRepository;

        public object JsonRequestBehavior { get; private set; }

        public Order()
        {
            _sqlRepository = new SqlRepository();

        }


        public IEnumerable<object> filters()
        {
            try
            {
                var sql = @" SELECT * FROM (
                                         SELECT DISTINCT 
                                        isnull(e.Id,'') AS EntityId,isnull(e.UserName,'') Entity,
										pln.Id PLantId,Pln.UserName Plant,
                                        isnull(ps.Id,'') AS ProductionStatusId, isnull(ps.UserName,'') AS ProductionStatus
										
                                                   , Buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

													 BuyerId=STUFF((select distinct ','+XB.Id from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

																
                                                    CustomerId=STUFF((select distinct ','+XP.Id from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),   
                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                                                 

                                        from trn.ProductionOrder PO
				                                inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
				                                INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID and ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
				                                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=PO.Id
				                                LEFT OUTER JOIN trn.SalesOrder SO ON so.Id=pod.SalesOrderId
				                                left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
				                                left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
				                                left outer join [HKP].Buyer B on B.Id=MO.BuyerId
				                                left outer join [HKP].[Party] p on P.Id=MO.PartyId

				                                left outer join org.Entity E on e.Id=p1.EntityID
				                                LEFT OUTER JOIN org.Unit AS u ON u.Id=e.UnitId
				                                left outer join org.Plant PLN on pln.Id=PO.PlantId
				                                LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                                WHERE  mo.OrderStatusId<>'Closed' and mo.OrderStatusId<>'Cancelled'
                                ) AS KK";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
        public void OrderReport(Dictionary<string, string> parameters, string fromDate, string toDate, string dateType)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[0].Name = "Data";
                sheet = workbook.Worksheets[0];

                //DataTable dtOrder = _sqlRepository.GetDataTable(sql);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 16;
                int colResponsiblePerson = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Reference No.";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyerRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 22;
                int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Delivery Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Ex Factory Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanExFactoryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer Group";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomerAccountGroup = COL;
                COL++;

                sheet[ROW, COL].Text = "Material ROW ID";
                sheet[ROW, COL].ColumnWidth = 22;
                int colMaterialRowId = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 22;
                int colMaterial = COL;
                COL++;

                sheet[ROW, COL].Text = "Product Category";
                sheet[ROW, COL].ColumnWidth = 14;
                int colProductCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 14;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 14;
                int colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order Creation Date";
                sheet[ROW, COL].ColumnWidth = 14;
                int colMasterOrderCreationDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Id";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "PR No";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colRate= COL;
                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colCM = COL;
                COL++;
                sheet[ROW, COL].Text = "SPT";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSPT = COL;
                COL++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int colRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSOQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Shipped Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colShippedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Bal Shipment";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBalShipment = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlan = COL;
                COL++;
                sheet[ROW, COL].Text = "To Plan";
                sheet[ROW, COL].ColumnWidth = 16;
                int colToPlan = COL;
                COL++;
                sheet[ROW, COL].Text = "Process Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProcessStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Product Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductCode = COL;
                //COL++;
                //sheet[ROW, COL].Text = "Product";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int colProduct = COL;
                //COL++;
                //sheet[ROW, COL].Text = "Material";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Ref";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOwnRef = COL;
                COL++;
                sheet[ROW, COL].Text = "Description";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDescription = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOtherRawMaterialInhouseDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Material Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colMainMaterialRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Other Raw Material Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOtherRawMaterialRemarks = COL;
                COL++;


                sheet[ROW, COL].Text = "Input Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colInputStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Line Target";
                sheet[ROW, COL].ColumnWidth = 12;
                int colLineTarget = COL;
                COL++;
                sheet[ROW, COL].Text = "No of Line Plan";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colNoOfLinePlan = COL;
                COL++;

                sheet[ROW, COL].Text = "Priority";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPriority = COL;
                COL++;
                sheet[ROW, COL].Text = "Line No.";
                sheet[ROW, COL].ColumnWidth = 12;
                int colLineNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Value";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOrderValue = COL;
                COL++;
                sheet[ROW, COL].Text = "CM Value";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOrderStatus = COL;
                //COL++;
                //sheet[ROW, COL].Text = "Remarks";
                //sheet[ROW, COL].ColumnWidth = 12;
                //int colRemarks = COL;
               

                #endregion columns

                int endCol = COL;
                //sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
                //sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                //for (int i = 0; i < dtOrder.Rows.Count; i++)
                //{
                //    sheet[ROW, colPlant].Text = dtOrder.Rows[i]["Plant"].ToString();
                //    sheet[ROW, colEntity].Text = dtOrder.Rows[i]["Entity"].ToString();
                //    sheet[ROW, colBuyer].Text = dtOrder.Rows[i]["Buyer"].ToString();
                //    sheet[ROW, colCustomer].Text = dtOrder.Rows[i]["Customer"].ToString();
                //    sheet[ROW, colCustomerAccountGroup].Text = dtOrder.Rows[i]["CustomerAccountGroup"].ToString();
                //    sheet[ROW, colCommitmentDate].Text = GetDate(dtOrder.Rows[i]["CommitmentDate"].ToString());
                //    sheet[ROW, colDeliveryDate].Text = GetDate(dtOrder.Rows[i]["DeliveryDate"].ToString());
                //    sheet[ROW, colMasterOrderNo].Text = dtOrder.Rows[i]["MasterOrderNo"].ToString();
                //    sheet[ROW, colMaterial].Text = dtOrder.Rows[i]["Material"].ToString();
                //    sheet[ROW, colProductCategory].Text = dtOrder.Rows[i]["ProductCategory"].ToString();
                //    sheet[ROW, colProduct].Text = dtOrder.Rows[i]["Product"].ToString();
                //    sheet[ROW, colSalesOrderDesc].Text = dtOrder.Rows[i]["SODesc"].ToString();
                //    sheet[ROW, colUOM].Text = dtOrder.Rows[i]["UOM"].ToString();
                //    sheet[ROW, colCurrency].Text = dtOrder.Rows[i]["Currency"].ToString();
                //    sheet[ROW, colMasterOrderCreationDate].Text = dtOrder.Rows[i]["MasterOrderCreationDate"].ToString();


                //    sheet[ROW, colBulletinId].Text = dtOrder.Rows[i]["BulletinId"].ToString();
                //    sheet[ROW, colTotalSPT].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["TotalSPT"].ToString());
                //    sheet[ROW, colNoOfWS].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["NoOfWS"].ToString());
                //    sheet[ROW, colContractId].Text = dtOrder.Rows[i]["ContractId"].ToString();
                //    sheet[ROW, colContractName].Text = dtOrder.Rows[i]["ContractName"].ToString();
                //    sheet[ROW, colLCNo].Text = dtOrder.Rows[i]["LCNo"].ToString();


                //    sheet[ROW, colArticle].Text = dtOrder.Rows[i]["Article"].ToString();
                //    sheet[ROW, colOwnReferenceNo].Text = dtOrder.Rows[i]["OwnReferenceNo"].ToString();
                //    sheet[ROW, colBuyerReferenceNo].Text = dtOrder.Rows[i]["BuyerReferenceNo"].ToString();

                //    sheet[ROW, colBuyerOrderNo].Text = dtOrder.Rows[i]["BuyerOrderNo"].ToString();
                //    sheet[ROW, colOwnOrderNo].Text = dtOrder.Rows[i]["OwnOrderNo"].ToString();


                //    sheet[ROW, colMaterialRowId].Text = dtOrder.Rows[i]["MaterialRowId"].ToString();
                //    sheet[ROW, colProductionOrderId].Text = dtOrder.Rows[i]["ProductionOrderId"].ToString();

                //    sheet[ROW, colProductionOrderRemarks].Text = dtOrder.Rows[i]["Remarks"].ToString();
                //    if (dtOrder.Rows[i]["ProductionOrderId"].ToString().Trim() == "")
                //        sheet[ROW, colProductionOrderRemarks].Text = "Yet to plan";

                //    sheet[ROW, colProductionStatus].Text = dtOrder.Rows[i]["ProductionStatus"].ToString();

                //    sheet[ROW, colReason].Text = dtOrder.Rows[i]["Reason"].ToString();


                //    sheet[ROW, colOrderCategory].Text = dtOrder.Rows[i]["OrderCategory"].ToString();
                //    sheet[ROW, colOrderStatus].Text = dtOrder.Rows[i]["OrderStatus"].ToString();
                //    sheet[ROW, colSOCategory].Text = dtOrder.Rows[i]["SOCategory"].ToString();
                //    sheet[ROW, colSOStatus].Text = dtOrder.Rows[i]["SOStatus"].ToString();
                //    sheet[ROW, colResponsiblePerson].Text = dtOrder.Rows[i]["ResponsiblePerson"].ToString();
                //    sheet[ROW, colType].Text = dtOrder.Rows[i]["Type"].ToString();
                //    sheet[ROW, colSOQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["SOQty"].ToString());
                //    sheet[ROW, colSalesOrderId].Text = dtOrder.Rows[i]["SalesOrderId"].ToString();
                //    sheet[ROW, colPONo].Text = dtOrder.Rows[i]["PONumber"].ToString();
                //    sheet[ROW, colPODate].Text = dtOrder.Rows[i]["PODate"].ToString();


                //    sheet[ROW, colPlannedQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["PlannedQty"].ToString());
                //    sheet[ROW, colFOB].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["FOB"].ToString());
                //    sheet[ROW, colCM].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["CM"].ToString());
                //    sheet[ROW, colDiff].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["Diff"].ToString());

                //    sheet[ROW, colOrderAmount].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["OrderAmount"].ToString());
                //    sheet[ROW, colCMAmount].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["CMAmount"].ToString());

                //    sheet[ROW, colSOAddedDate].Text = dtOrder.Rows[i]["SOAddedDate"].ToString();
                //    sheet[ROW, colMainRawMaterialInhouseDate].Text = dtOrder.Rows[i]["MainRawMaterialInhouseDate"].ToString();
                //    sheet[ROW, colOtherRawMaterialInhouseDate].Text = dtOrder.Rows[i]["OtherRawMaterialInhouseDate"].ToString();
                //    sheet[ROW, colLSD].Text = dtOrder.Rows[i]["LSD"].ToString();

                //    sheet[ROW, colDeliveryMonth].Formula = string.Concat("MONTH(", CellAddr(colDeliveryDate, ROW), ")");
                //    sheet[ROW, colCommitmentMonth].Formula = string.Concat("MONTH(", CellAddr(colCommitmentDate, ROW), ")");


                //    sheet[ROW, colDeliveryMonth].Formula = "CONCATENATE(Month(" + CellAddr(colDeliveryDate, ROW) + "),\"/\",Year(" + CellAddr(colDeliveryDate, ROW) + "))";
                //    sheet[ROW, colCommitmentMonth].Formula = "CONCATENATE(Month(" + CellAddr(colCommitmentDate, ROW) + "),\"/\",Year(" + CellAddr(colCommitmentDate, ROW) + "))";


                //    sheet[ROW, colPRBookedQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["PRBookedQuantity"].ToString());
                //    sheet[ROW, colSOBookedQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["SOBookedQuantity"].ToString());
                //    sheet[ROW, colTotalPRProducedQty].Formula = CellAddr(colPRBookedQty, ROW) + "+" + CellAddr(colSOBookedQty, ROW);
                //    sheet[ROW, colPRPlanQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["PRPlanQty"].ToString());


                //    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                //    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                //    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                //    ROW++;

                //}


                //sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                //sheet.UsedRange.WrapText = true;
                //sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.UsedRange["A7"].FreezePanes();

                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //ReportUtility reportUtility = new ReportUtility();
                //reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Order Report", identity.CompanyId, identity.CompanyName, "");

                //reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Order Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;


                #region Sheet Report
                workbook.Worksheets[1].Name = "Report";
                sheet = workbook.Worksheets[1];

                //DataTable dtOrder = _sqlRepository.GetDataTable(sql);

                 ROW = 6;  COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 16;
                 colResponsiblePerson = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                 colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                 colBuyer = COL;
                COL++;

                sheet[ROW, COL].Text = "Buyer Reference No.";
                sheet[ROW, COL].ColumnWidth = 16;
                 colBuyerRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 22;
                 colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Delivery Date";
                sheet[ROW, COL].ColumnWidth = 12;
                 colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Ex Factory Date";
                sheet[ROW, COL].ColumnWidth = 12;
                 colPlanExFactoryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer Group";
                sheet[ROW, COL].ColumnWidth = 16;
                 colCustomerAccountGroup = COL;
                COL++;

                sheet[ROW, COL].Text = "Material ROW ID";
                sheet[ROW, COL].ColumnWidth = 22;
                 colMaterialRowId = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 22;
                 colMaterial = COL;
                COL++;

                sheet[ROW, COL].Text = "Product Category";
                sheet[ROW, COL].ColumnWidth = 14;
                 colProductCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 14;
                 colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 14;
                 colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order Creation Date";
                sheet[ROW, COL].ColumnWidth = 14;
                 colMasterOrderCreationDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Id";
                sheet[ROW, COL].ColumnWidth = 16;
                 colSalesOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Status";
                sheet[ROW, COL].ColumnWidth = 16;
                 colSalesOrderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "PR No";
                sheet[ROW, COL].ColumnWidth = 12;
                 colProductionOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Status";
                sheet[ROW, COL].ColumnWidth = 12;
                 colProductionStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                 colRate = COL;
                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                 colCM = COL;
                COL++;
                sheet[ROW, COL].Text = "SPT";
                sheet[ROW, COL].ColumnWidth = 10;
                 colSPT = COL;
                COL++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                 colRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                 colSOQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Shipped Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                 colShippedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Bal Shipment";
                sheet[ROW, COL].ColumnWidth = 16;
                 colBalShipment = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan";
                sheet[ROW, COL].ColumnWidth = 16;
                 colPlan = COL;
                COL++;
                sheet[ROW, COL].Text = "To Plan";
                sheet[ROW, COL].ColumnWidth = 16;
                 colToPlan = COL;
          

                #endregion columns

                 endCol = COL;
                //sheet.Range[ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
                //sheet.Range[ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                 startRow = ROW;

                //for (int i = 0; i < dtOrder.Rows.Count; i++)
                //{
                //    sheet[ROW, colPlant].Text = dtOrder.Rows[i]["Plant"].ToString();
                //    sheet[ROW, colEntity].Text = dtOrder.Rows[i]["Entity"].ToString();
                //    sheet[ROW, colBuyer].Text = dtOrder.Rows[i]["Buyer"].ToString();
                //    sheet[ROW, colCustomer].Text = dtOrder.Rows[i]["Customer"].ToString();
                //    sheet[ROW, colCustomerAccountGroup].Text = dtOrder.Rows[i]["CustomerAccountGroup"].ToString();
                //    sheet[ROW, colCommitmentDate].Text = GetDate(dtOrder.Rows[i]["CommitmentDate"].ToString());
                //    sheet[ROW, colDeliveryDate].Text = GetDate(dtOrder.Rows[i]["DeliveryDate"].ToString());
                //    sheet[ROW, colMasterOrderNo].Text = dtOrder.Rows[i]["MasterOrderNo"].ToString();
                //    sheet[ROW, colMaterial].Text = dtOrder.Rows[i]["Material"].ToString();
                //    sheet[ROW, colProductCategory].Text = dtOrder.Rows[i]["ProductCategory"].ToString();
                //    sheet[ROW, colProduct].Text = dtOrder.Rows[i]["Product"].ToString();
                //    sheet[ROW, colSalesOrderDesc].Text = dtOrder.Rows[i]["SODesc"].ToString();
                //    sheet[ROW, colUOM].Text = dtOrder.Rows[i]["UOM"].ToString();
                //    sheet[ROW, colCurrency].Text = dtOrder.Rows[i]["Currency"].ToString();
                //    sheet[ROW, colMasterOrderCreationDate].Text = dtOrder.Rows[i]["MasterOrderCreationDate"].ToString();


                //    sheet[ROW, colBulletinId].Text = dtOrder.Rows[i]["BulletinId"].ToString();
                //    sheet[ROW, colTotalSPT].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["TotalSPT"].ToString());
                //    sheet[ROW, colNoOfWS].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["NoOfWS"].ToString());
                //    sheet[ROW, colContractId].Text = dtOrder.Rows[i]["ContractId"].ToString();
                //    sheet[ROW, colContractName].Text = dtOrder.Rows[i]["ContractName"].ToString();
                //    sheet[ROW, colLCNo].Text = dtOrder.Rows[i]["LCNo"].ToString();


                //    sheet[ROW, colArticle].Text = dtOrder.Rows[i]["Article"].ToString();
                //    sheet[ROW, colOwnReferenceNo].Text = dtOrder.Rows[i]["OwnReferenceNo"].ToString();
                //    sheet[ROW, colBuyerReferenceNo].Text = dtOrder.Rows[i]["BuyerReferenceNo"].ToString();

                //    sheet[ROW, colBuyerOrderNo].Text = dtOrder.Rows[i]["BuyerOrderNo"].ToString();
                //    sheet[ROW, colOwnOrderNo].Text = dtOrder.Rows[i]["OwnOrderNo"].ToString();


                //    sheet[ROW, colMaterialRowId].Text = dtOrder.Rows[i]["MaterialRowId"].ToString();
                //    sheet[ROW, colProductionOrderId].Text = dtOrder.Rows[i]["ProductionOrderId"].ToString();

                //    sheet[ROW, colProductionOrderRemarks].Text = dtOrder.Rows[i]["Remarks"].ToString();
                //    if (dtOrder.Rows[i]["ProductionOrderId"].ToString().Trim() == "")
                //        sheet[ROW, colProductionOrderRemarks].Text = "Yet to plan";

                //    sheet[ROW, colProductionStatus].Text = dtOrder.Rows[i]["ProductionStatus"].ToString();

                //    sheet[ROW, colReason].Text = dtOrder.Rows[i]["Reason"].ToString();


                //    sheet[ROW, colOrderCategory].Text = dtOrder.Rows[i]["OrderCategory"].ToString();
                //    sheet[ROW, colOrderStatus].Text = dtOrder.Rows[i]["OrderStatus"].ToString();
                //    sheet[ROW, colSOCategory].Text = dtOrder.Rows[i]["SOCategory"].ToString();
                //    sheet[ROW, colSOStatus].Text = dtOrder.Rows[i]["SOStatus"].ToString();
                //    sheet[ROW, colResponsiblePerson].Text = dtOrder.Rows[i]["ResponsiblePerson"].ToString();
                //    sheet[ROW, colType].Text = dtOrder.Rows[i]["Type"].ToString();
                //    sheet[ROW, colSOQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["SOQty"].ToString());
                //    sheet[ROW, colSalesOrderId].Text = dtOrder.Rows[i]["SalesOrderId"].ToString();
                //    sheet[ROW, colPONo].Text = dtOrder.Rows[i]["PONumber"].ToString();
                //    sheet[ROW, colPODate].Text = dtOrder.Rows[i]["PODate"].ToString();


                //    sheet[ROW, colPlannedQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["PlannedQty"].ToString());
                //    sheet[ROW, colFOB].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["FOB"].ToString());
                //    sheet[ROW, colCM].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["CM"].ToString());
                //    sheet[ROW, colDiff].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["Diff"].ToString());

                //    sheet[ROW, colOrderAmount].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["OrderAmount"].ToString());
                //    sheet[ROW, colCMAmount].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["CMAmount"].ToString());

                //    sheet[ROW, colSOAddedDate].Text = dtOrder.Rows[i]["SOAddedDate"].ToString();
                //    sheet[ROW, colMainRawMaterialInhouseDate].Text = dtOrder.Rows[i]["MainRawMaterialInhouseDate"].ToString();
                //    sheet[ROW, colOtherRawMaterialInhouseDate].Text = dtOrder.Rows[i]["OtherRawMaterialInhouseDate"].ToString();
                //    sheet[ROW, colLSD].Text = dtOrder.Rows[i]["LSD"].ToString();

                //    sheet[ROW, colDeliveryMonth].Formula = string.Concat("MONTH(", CellAddr(colDeliveryDate, ROW), ")");
                //    sheet[ROW, colCommitmentMonth].Formula = string.Concat("MONTH(", CellAddr(colCommitmentDate, ROW), ")");


                //    sheet[ROW, colDeliveryMonth].Formula = "CONCATENATE(Month(" + CellAddr(colDeliveryDate, ROW) + "),\"/\",Year(" + CellAddr(colDeliveryDate, ROW) + "))";
                //    sheet[ROW, colCommitmentMonth].Formula = "CONCATENATE(Month(" + CellAddr(colCommitmentDate, ROW) + "),\"/\",Year(" + CellAddr(colCommitmentDate, ROW) + "))";


                //    sheet[ROW, colPRBookedQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["PRBookedQuantity"].ToString());
                //    sheet[ROW, colSOBookedQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["SOBookedQuantity"].ToString());
                //    sheet[ROW, colTotalPRProducedQty].Formula = CellAddr(colPRBookedQty, ROW) + "+" + CellAddr(colSOBookedQty, ROW);
                //    sheet[ROW, colPRPlanQty].Number = clsStaticInfo.dbl(dtOrder.Rows[i]["PRPlanQty"].ToString());


                //    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                //    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                //    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                //    ROW++;

                //}


                //sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                //sheet.UsedRange.WrapText = true;
                //sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.UsedRange["A7"].FreezePanes();

                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //ReportUtility reportUtility = new ReportUtility();
                //reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Order Report", identity.CompanyId, identity.CompanyName, "");

                //reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + startRow.ToString()].FreezePanes();

                 identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                 reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Order Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;



                #endregion


                string strFileName = "OrderReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();


            }
            catch (Exception ex)
            {
                throw ex;

            }

        
        }

        private void Json(string message, object allowGet)
        {
            throw new NotImplementedException();
        }

        // private string OrderCostingProductInfoSQL(string OrderCostingId)
        // {
        //     var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //     return @"select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
        //,pc.UserName as ProductCategory
        //,psc.UserName as ProductSubCategory,ct.UserName AS CostingTypeName
        //                      ,pm.CostingType,eff.StandardWorkingHours AS StandardWorkingHoursForProduct
        //,c.Code Currency,u.UserName UnitOfMeasurement
        //from OrderCostingMasterTemplate qcm 
        //left outer join SCS.Currency c on c.Id=qcm.CurrencyId
        //left join SCS.UnitOfMeasurement u on u.Id=qcm.UOM
        //                     left join [HKP].[Party] p ON p.Id = qcm.CustomerId
        //                     left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
        //left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
        //left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
        //LEFT JOIN [TRN].[ProductMasterEfficency] EFF ON eff.ProductMasterId=qcm.ProductMasterId AND EfficencyName='Costing'  
        //LEFT OUTER JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType
        //                     WHERE QCM.ID='" + OrderCostingId + @"'";

        // }


        public struct Total
        {
            public string Key ;
            public double total;
            public Total(double tot)
            {
                Key = "Total";
                total = tot;
            }
        }

        public List<Dictionary<string, object>> getSlabData(Dictionary<string,string>parameters , string group , out List<Object> totalArr , out List<double[]> chart , string value , string analysis, string type)
        {
            try
            {
                var str = "";
                string filter = "";
                string select = "";
                string groupBy = "";
                string ids = "";
                string date = "";
                string val = "";
                string DDate = "";
                string Dtype = "";
                // The Chart Type
                if(type == "ProductionD")
                {
                    Dtype = "(SELECT MAX(xp1.ProductionDate) FROM ProductionPlanningType1 Xp1 WHERE Xp1.ProductionOrderID = pod.ProductionOrderID)";
                }
                if(type == "ToD")
                {
                    Dtype = "GETDATE()";
                }
                //To set date String
                switch (analysis)
                {
                    case "DeliveryD":
                        date = date + "DeliveryDate";
                        DDate = DDate + "so.DeliveryDate";
                        break;
                    case "CommitmentD":
                        date = date + "CommitmentDate";
                        DDate = DDate + "so.CommitmentDate";
                        break;
                    case "ExFactoryD":
                        date = date + "DDate";
                        DDate = DDate + "case when so.PlanExFactoryDate is null then so.CommitmentDate else PlanExFactoryDate end";
                        break;

                }
                //To Set Filter String
                if (parameters.ContainsKey("PlantId"))
                {
                    string ents = "";
                    if(parameters["ERespId"] != "'',''")
                    {
                        ents = ents +" and e.employeeId in (" +parameters["ERespId"]+")";
                    }

                    filter = @" and isnull(mo.ResponsiblePersonId,'') in(" + parameters["MResId"] + @") and isnull(e.Id,'') in (" + parameters["EntityId"] + @") and isnull(p.Id,'') in (" + parameters["PlantId"] + @") 
                                and isnull(so.OrderStatusId,'') in ( " + parameters["Status"] + @") and isnull(mo.PartyId,'') in (" + parameters["CustomerId"] + @") " + ents + @"";
                }
                
                //To set Select and groupBY string and ids
                if(group == "Delivery")
                {
                        select = @"Select DATENAME(m," + date + @") as Months , YEAR(" + date + @") as Years ,";
                        groupBy = @" group by  DATENAME(MONTH," + date + @") , YEAR(" + date + @") , DATEPART(m," + date + @") , OrderStatusId
                order by Years , DATEPART(m," + date + @")";
                    
                    
                }
                else
                {
                    select = @"Select isnull("+group+",'Not Alotted') as "+group+"  , col,";
                    groupBy = @"group by "+group+ ",col, OrderStatusId order by " + group+"";
                    if(group == "Entity")
                    {
                        ids = ",e.Id as col";
                    }
                    if (group == "Customers")
                    {
                        ids = ",mo.PartyId as col";
                    }
                    if (group == "MResp")
                    {
                        ids = ",mo.ResponsiblePersonId as col";
                    }
                    if (group == "EResp")
                    {
                        ids = ",e.employeeId as col";
                    }
                }

                //To set val  String
                switch(value)
                {
                    case "SO":
                        val = val + "1";
                        break;
                    case "SOQTY":
                        val = val + "Qty";
                        break;
                    case "SORT":
                        val = val + "Qty*Rate";
                        break;
                    case "SOCM":
                        val = val + "Qty*CM";
                        break;
                }
                

                    
                str = @""+select+ @"sum(case when EarlyOrLateBy<-30 then " + val + @" else 0 end) LN30, sum(case when EarlyOrLateBy>-31 and EarlyOrLateBy<-20 then " + val + @" else 0 end) LN30T20
                                , sum(case when EarlyOrLateBy>-21 and EarlyOrLateBy<-10 then " + val+ @" else 0 end) LN20T10, sum(case when EarlyOrLateBy>-11 and EarlyOrLateBy<-5 then " + val + @" else 0 end) LN10T5
                                , sum(case when EarlyOrLateBy>-6 and EarlyOrLateBy<0 then " + val + @" else 0 end) LN5T0, sum(case when EarlyOrLateBy=0 then " + val + @" else 0 end) E0
                                , sum(case when EarlyOrLateBy>0 and EarlyOrLateBy<6 then " + val + @" else 0 end) G0T5, sum(case when EarlyOrLateBy>5 and EarlyOrLateBy<11 then " + val + @" else 0 end) G5T10
                                , sum(case when EarlyOrLateBy>10 and EarlyOrLateBy<16 then " + val + @" else 0 end) G10T15, sum(case when EarlyOrLateBy>15 and EarlyOrLateBy<21 then " + val + @" else 0 end) G15T20
                                , sum(case when EarlyOrLateBy>20 and EarlyOrLateBy<31 then " + val + @" else 0 end) G20T30, sum(case when EarlyOrLateBy>30 then " + val + @" else 0 end) G30
                                ,sum(case when ProductionDate is null then " + val + @" else 0 end) nodates
                                , sum(case when ProductionOrderId is null then " + val + @" else 0 end) NotAlotted
								, sum(case when AddedDate >= DATEADD(DAY, -3 , GETDATE()) then " + val + @" else 0 end) daysthree 
                                ,OrderStatusId
                                from
                                (
                                 Select distinct so.Id ,so.Qty , so.Rate , so.CM , so.DeliveryDate,so.AddedDate , so.CommitmentDate , pod.ProductionOrderID , (SELECT MAX(xp1.ProductionDate) FROM ProductionPlanningType1 Xp1 WHERE Xp1.ProductionOrderID=pod.ProductionOrderID) as ProductionDate, so.OrderStatusId as OrderStatusId ,
                                DateDiff(Day,"+Dtype+", " + DDate+ @") as EarlyOrLateBy , prt.Username as customers , e.UserName as Entity ,  (case when so.PlanExFactoryDate is null then so.CommitmentDate else PlanExFactoryDate end) as DDate , emp.EmployeeName as MResp,
								ee.EmployeeName as EResp " + ids+ @"
                                from trn.MasterOrder mo 
								left join hkp.orderstatus os on os.Id = mo.OrderStatusId
								left outer join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
								inner join trn.SalesOrder so on so.MasterOrderItemId = moi.Id
								left outer join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
								left outer join org.entity e on e.Id = mo.EntityId
								left outer join org.Plant p on p.Id = mo.PlantId
								left outer join hkp.Party prt on prt.Id = mo.PartyId
								left outer join dbo.EmployeeInformation emp on emp.SystemId = mo.ResponsiblePersonId
								left outer join dbo.EmployeeInformation ee on ee.SystemId = e.EmployeeId
								where os.id<> 'Closed' and os.Id <>'Cancelled' and so.OrderStatusId not in ('Closed','Cancelled')
                                " + filter+@"
                                ) as da
                                " + groupBy+"";

                
                //Making of the required Datatable for the SlabGrid
                DataTable tr = _sqlRepository.GetDataTable(str);
                DataTable tt = tr.Clone();
               List<Object> newArr = new List<object>();
                int ini = 2;

                double[] Active = new double[14] ;
                double[] Pending = new double[14] ;
                double[] ToClose = new double[14];
                double[] ToDispatch = new double[14];
                double[] ProductionComplete = new double[14];



                string[] columnNames = tr.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName)
                                 .ToArray();

                //The Stacked Chart Values
                for (int i = 0; i <tr.Rows.Count; i++)
                {
                    if(tr.Rows[i]["OrderStatusId"].ToString() == "Active")
                    {
                        for(int j = 2;j<14; j++)
                        {
                            Active[j] = Active[j] + OTSBD.clsStaticInfo.dbl(tr.Rows[i][j].ToString());
                        }
                    }
                    if (tr.Rows[i]["OrderStatusId"].ToString() == "Pending")
                    {
                        for (int j = 2; j < 14; j++)
                        {
                            Pending[j] = Pending[j] + OTSBD.clsStaticInfo.dbl(tr.Rows[i][j].ToString());
                        }
                    }
                    if (tr.Rows[i]["OrderStatusId"].ToString() == "ToClose")
                    {
                        for (int j = 2; j < 14; j++)
                        {
                            ToClose[j] = ToClose[j] + OTSBD.clsStaticInfo.dbl(tr.Rows[i][j].ToString());
                        }
                    }
                    if (tr.Rows[i]["OrderStatusId"].ToString() == "ToShip")
                    {
                        for (int j = 2; j < 14; j++)
                        {
                            ToDispatch[j] = ToDispatch[j] + OTSBD.clsStaticInfo.dbl(tr.Rows[i][j].ToString());
                        }
                    }
                    if (tr.Rows[i]["OrderStatusId"].ToString() == "ProductionComplete")
                    {
                        for (int j = 2; j < 14; j++)
                        {
                            ProductionComplete[j] = ProductionComplete[j] + OTSBD.clsStaticInfo.dbl(tr.Rows[i][j].ToString());
                        }
                    }
                }
                List<double[]> list = new List<double[]>();
                list.Add(Active);
                list.Add(Pending);
                list.Add(ToClose);
                list.Add(ToDispatch);
                list.Add(ProductionComplete);

                chart = list;

                DataRow dr = null;
                int roww = 0;
                string ch = "";
                for (int i = 0; i<tr.Rows.Count; i++)
                {
                    if(tr.Rows[i][0].ToString() != "")
                    {
                        
                        if(tr.Rows[i][0].ToString() != ch)
                        {
                            dr = tt.NewRow();
                            dr[columnNames[0]] = tr.Rows[i][0].ToString();
                            dr[columnNames[1]] = tr.Rows[i][1].ToString();

                            for (int j = 2; j < 17; j++)
                            {
                                dr[columnNames[j]] = 0;
                            }
                            tt.Rows.Add(dr);
                            roww++;
                        }

                        for(int j = 2; j<17; j++)
                        {
                            double sum = OTSBD.clsStaticInfo.dbl(tt.Rows[roww - 1][j].ToString()) + OTSBD.clsStaticInfo.dbl(tr.Rows[i][j].ToString());
                            tt.Rows[roww - 1][j] = sum; 
                        }

                        ch = tr.Rows[i][0].ToString();
                    }
                }



                //Finding the total of the Row
                if(type == "ProductionD")
                {
                    for (int i = 0; i < tt.Rows.Count; i++)
                    {
                        double jj = 0;
                        for (int j = ini; j < 15; j++)
                        {
                            jj = jj + OTSBD.clsStaticInfo.dbl(tt.Rows[i][j].ToString());
                        }
                        Total t = new Total(jj);
                        newArr.Add(t);

                    }
                }
                else
                {
                    for (int i = 0; i < tt.Rows.Count; i++)
                    {
                        double jj = 0;
                        for (int j = ini; j < 14; j++)
                        {
                            jj = jj + OTSBD.clsStaticInfo.dbl(tt.Rows[i][j].ToString());
                        }
                        Total t = new Total(jj);
                        newArr.Add(t);

                    }
                }
                
                totalArr = newArr;

                tt.Columns.Add("RowTotal", typeof(decimal));
                
                    for (int i = 0; i < tt.Rows.Count; i++)
                    {
                        double jj = 0;
                        for (int j = ini; j < 14; j++)
                        {
                            jj = jj + OTSBD.clsStaticInfo.dbl(tt.Rows[i][j].ToString());
                        }
                        tt.Rows[i]["RowTotal"] = jj;
                    }
                
                

                return Library.Service.Helpers.DataTableExtensions.DataTableToJson(tt);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}