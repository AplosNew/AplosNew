using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.OrderManagement.BOM
{
    public class BOMReports
    {
        public enum BOMLevel
        {
            SO,
            Item,
            Order
        }
        private readonly SqlRepository _sqlRepository = new SqlRepository();
        private string CellAddr(int Col, int Row)
        {
            return clsStaticInfo.GetxlsCol(Col) + Row.ToString();
        }
        //Get Master order report
        public IWorkbook GetMasterOrderReport(string MasterOrderItemId, BOMLevel Level, bool isMatrix = true)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;
            IWorkbook workbook = application.Workbooks.Create(1);
            //Load the existing Excel workbook into IWorkbook
            if (Level == BOMLevel.Item)
                workbook = application.Workbooks.Create(3);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            try
            {
                DataTable dtOrderMaster = _sqlRepository.GetDataTable(@"select mo.Id, mo.type, b.UserName as Buyer, p.UserName as Customer
                ,  mo.OrderYear as Year, mo.TotalQty as TotalQuantity
                    , uom.UserName as UnitOfMeasurement, mo.NoOfLineItem, mo.OrderWastagePercentage
                    , mo.ExtraOrderPercentage, mo.BuyerReferenceNo, mo.OwnReferenceNo, BDept.UserName as BuyerDepartment
                    , BDev.UserName as BuyerDevision, MoCur.Code MasterOrderCurrency from trn.MasterOrder MO 
                    left join scs.Currency MoCur on MoCur.id = mo.CurrencyId 
                    left join scs.UnitOfMeasurement UOM on uom.id = mo.TotalQtyUOMId 
                    left join hkp.buyer B on b.id = mo.buyerid 
                    left join hkp.party p on p.id = mo.partyid 
                    left join hkp.BuyerDepartment BDept on BDept.id = mo.buyerDepartmentid 
                    left join HKP.buyerdivision BDev on BDev.id = mo.BuyerDivisionId where mo.Id=(SELECT MasterOrderId from trn.MasterOrderItem where Id='" + MasterOrderItemId + "')");
                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                DataTable dtMasterOrderItem = _sqlRepository.GetDataTable(@"select moi.id as MasterOrderItemNo,moi.BuyerReferenceNo
                 ,moi.OwnReferenceNo,moi.TotalQty as TotalMOIQuantity, moi.MasterOrderId,c.ContractNo,ml.LCRef
                 ,moi.OrderWastagePercentage, moi.ExtraOrderPercentage ,mm.UserName as Material ,mma.StandardName as Article, moi.Type
                 from trn.MasterOrderItem MOI
                 left join TRN.MasterOrder mo  on mo.id=moi.MasterOrderId
                 left join MST.MaterialMaster MM on mm.id = moi.MaterialMasterId
                 left join MST.MaterialMasterArticle mma on mma.id= moi.ArticleId
                 left join scs.TestingStandard ts on ts.id=moi.TestingStandardId
LEFT JOIN TRN.SalesOrder so on moi.Id=so.MasterOrderItemId
                 LEFT JOIN [Contract] AS c ON c.Id=so.ContractId
                 LEFT JOIN MasterLC AS ml ON ml.Id=c.MasterLCId
                 where moi.Id='" + MasterOrderItemId + "'");


                DataTable dtSalesOrderItem = _sqlRepository.GetDataTable(@"select so.MasterOrderItemId, so.id as SalesOrderNo,cpo.PONumber,os.UserName as OrderStatus,d.UserName as Destination
                ,so.Qty as Quantity, so.UpCharge, so.MainRawMaterialInhouseDate, so.Description
                ,so.SOType, oc.username as OrderCategory
                ,so.DeliveryDate, sm.UserName as ShipmentMode
                ,so.Rate, so.Discount,so.CM,so.LSD, so.OtherRawMaterialInhouseDate , so.Reason , so.CommitmentDate

                ,c1.username as FirstCharacteristics, isnull(fcs.ValueFreeText,CV1.UserName) as FirstCharacteristicsValue
                ,c2.username as SecondCharacteristics ,isnull(SCS.ValueFreeText, CV2.UserName) as SecondCharacteristicsValue
                ,c3.username as ThirdCharacteristics , isnull(ThirdCS.ValueFreeText,CV3.UserName) as ThirdCharacteristicsValue
                ,case when isnull(thirdCs.Id,'')<>'' THEN ThirdCs.Qty
                ELSE case when isnull(scs.id,'')<>'' THEN scs.Qty
                ELSE case when isnull(fcs.Id,'')<>'' THEN fcs.Qty
                ELSE 0 END END END AS Qty
                from trn.SalesOrder SO
                left join trn.masterorderitem moi on moi.id= so.masterorderitemid
                left join HKP.OrderCategory OC on oc.id = so.OrderCategoryId
                left join hkp.OrderStatus OS on os.id = so.OrderStatusId
                left join mst.shipMode SM on sm.id = so.shipmentModeId
                left join mst.Destination d on d.id =so.DestinationId
                left join trn.CustomerPO CPO on cpo.id =so.CustomerPOId

                left join TRN.FirstCharacteristics FCS on fcs.SalesOrderId = so.id
                left join hkp.Characteristics C1 on c1.id = fcs.CharacteristicsId
                left join HKP.CharacteristicsValue CV1 on cv1.id= fcs.CharacteristicsValueId

                left join TRN.SecondCharacteristics SCS on scs.SalesOrderId=so.id and scs.FirstCharacteristicsId=fcs.Id
                left join hkp.Characteristics C2 on c2.id = scs.CharacteristicsId
                left join HKP.CharacteristicsValue CV2 on cv2.id= scs.CharacteristicsValueId

                left join TRN.ThirdCharacteristics ThirdCS on ThirdCS.SalesOrderId=so.id and scs.id=ThirdCS.SecondCharacteristicsId
                left join hkp.Characteristics C3 on c3.id = ThirdCS.CharacteristicsId
                left join HKP.CharacteristicsValue CV3 on CV3.id= ThirdCS.CharacteristicsValueId

                     where moi.Id='" + MasterOrderItemId + "'");





                DataTable dtBOMData = new DataTable();
                if (Level == BOMLevel.SO)
                {
                    worksheet.Name = "Detail";
                    //worksheet.Name = "BOM-SO Level";
                    string strsql = @"SELECT b.Id, b.MasterOrderItemId,moi.MasterOrderId,moi.OwnReferenceNo,moi.BuyerReferenceNo, b.VendorId,b.SalesOrderId,
                                 mm.UserName AS Material,mma.StandardName AS Article,p.UserName AS Vendor,b.SKUDesc,


                                v1.UserName AS CharVal1,v2.UserName AS CharVal2,v3.UserName AS CharVal3,convert(bit,isnull(b.isParent,0)) AS isParent,
                                convert(bit,isnull(b.isChild,0)) AS isChild,PR.UserName AS Process,
                               CONVERT(BIT, isnull(b.RequiredQtyApproved,0)) AS RequiredQtyApproved,CONVERT(BIT, isnull(b.IncompleteMaterial,0)) AS IncompleteMaterial,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
                                b.BOMQty,b.RequiredQty,b.RequiredQtyPO,uom.UserName AS UOM,uomm.UserName AS ParentUOM,
                                POUOM.UserName AS POUOM,
                                b.RMDescription,	b.RMCustomerSpec,	b.RMVendorSpec
								,ISNULL(po.POQTY,0) POQTY,ISNULL(grn.GRNQty,0) GRNQty,
								
								 SO1 =STUFF((select distinct ','+xv1.UserName
								               from BOQFGMapping AS XM	 
										       JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
								               JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.FirstCharacteristicsValueId
								             WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO2 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.SecondCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO3 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.ThirdCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								POIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.PurchaseOrderDetail AS XB2 ON xb2.Id=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								GRNIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.InventoryReceiveDetail AS XB2 ON xb2.PODetailsId=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                  FROM BOQ AS b
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS POuom ON POuom.Id=b.POUoMId
                                LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                                LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
                                LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                                LEFT OUTER JOIN trn.masterorder MO ON MO.Id=moi.MasterOrderId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uomm ON uomm.Id=mo.TotalQtyUOMId
                                LEFT JOIN hkp.Process AS pr ON pr.Id=b.ProcessId

                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId
								Left outer join (	select BOQDetailId,sum(POBOQQty) as POQTY from trn.POBOQMAP  GRoup by BOQDetailId) PO On PO.BOQDetailId = b.Id
								Left outer join (	
								SELECT a.BOQDetailId , sum(b.TransactionQty) GRNQty 
				FROM trn.POBOQMAP a
				INNER JOIN trn.InventoryReceiveDetail b ON a.PODetailId=b.PODetailsId GRoup by BOQDetailId
								
								) GRN On GRN.BOQDetailId = b.Id

                                WHERE b.MasterOrderItemId='" + MasterOrderItemId + @"' and isnull(B.Id,'') NOT IN (select ParentId from BOQ where isnull(ParentId,'')<>'' AND MasterOrderItemId='" + MasterOrderItemId + @"')
                                ORDER BY isnull(b.Sequence,0),b.SalesOrderId";
                    dtBOMData = _sqlRepository.GetDataTable(strsql);
                }
                else if (Level == BOMLevel.Item)
                {
                    //worksheet.Name = "BOM-Item Level";
                    worksheet.Name = "Summary";
                    string strsql = @"select B.Sequence, B.MasterOrderItemId, B.MasterOrderId, B.OwnReferenceNo,
       B.BuyerReferenceNo, B.VendorId, B.Material, B.Article, B.Vendor, B.SKUDesc,B.POIds, B.GRNIds,
       B.CharVal1, B.CharVal2, B.CharVal3, B.isParent, B.isChild, B.Process,
       B.Consumption, B.WastagePer, B.UOM, B.ParentUOM, B.POUOM, B.RMDescription,
       B.RMCustomerSpec, B.RMVendorSpec, B.SO1, B.SO2, B.SO3,
       sum(b.BOMQty) AS BOMQty,sum(b.RequiredQty) AS RequiredQty, SUM(b.RequiredQtyPO) AS RequiredQtyPO, sum(b.OrderQty) AS OrderQty,sum(b.PlanOrderQty) AS PlanOrderQty
       ,SUM(ISNULL(b.POQTY,0)) POQTY,SUM(ISNULL(b.GRNQty,0)) GRNQty
  from (SELECT  b.Sequence,b.MasterOrderItemId,moi.MasterOrderId,moi.OwnReferenceNo,moi.BuyerReferenceNo, b.VendorId,
                                 mm.UserName AS Material,mma.StandardName AS Article,p.UserName AS Vendor,b.SKUDesc,
                                v1.UserName AS CharVal1,v2.UserName AS CharVal2,v3.UserName AS CharVal3,convert(bit,isnull(b.isParent,0)) AS isParent,
                                convert(bit,isnull(b.isChild,0)) AS isChild,PR.UserName AS Process,
                               b.Consumption,b.WastagePer,
                                uom.UserName AS UOM,uomm.UserName AS ParentUOM, POUOM.UserName AS POUOM,
                                b.RMDescription,	b.RMCustomerSpec,	b.RMVendorSpec,
								b.BOMQty,b.RequiredQty,b.RequiredQtyPO, b.OrderQty,PlanOrderQty,
                                ISNULL(po.POQTY,0) POQTY,ISNULL(grn.GRNQty,0) GRNQty,
								 SO1 =STUFF((select distinct ','+xv1.UserName
								          from BOQFGMapping AS XM	
										  JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
								          JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.FirstCharacteristicsValueId
								          WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO2 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.SecondCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO3 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.ThirdCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								POIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.PurchaseOrderDetail AS XB2 ON xb2.Id=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								GRNIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.InventoryReceiveDetail AS XB2 ON xb2.PODetailsId=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                  FROM BOQ AS b
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS POuom ON POuom.Id=b.POUoMId

                                LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                                LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
                                LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                                LEFT OUTER JOIN trn.masterorder MO ON MO.Id=moi.MasterOrderId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uomm ON uomm.Id=mo.TotalQtyUOMId
                                LEFT JOIN hkp.Process AS pr ON pr.Id=b.ProcessId

                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId
                                LEFT OUTER JOIN (	select BOQDetailId,sum(POBOQQty) as POQTY from trn.POBOQMAP  GRoup by BOQDetailId) PO On PO.BOQDetailId = b.Id
								LEFT OUTER JOIN (	
								SELECT a.BOQDetailId , sum(b.TransactionQty) GRNQty 
				                    FROM trn.POBOQMAP a
				                    INNER JOIN trn.InventoryReceiveDetail b ON a.PODetailId=b.PODetailsId GRoup by BOQDetailId
								
								) GRN On GRN.BOQDetailId = b.Id

                                WHERE b.MasterOrderItemId='" + MasterOrderItemId + @"' and isnull(B.Id,'') NOT IN (select ParentId from BOQ where isnull(ParentId,'')<>'' AND MasterOrderItemId='" + MasterOrderItemId + @"')
                        ) AS B
                                GROUP BY B.Sequence,B.POIds, B.GRNIds, B.MasterOrderItemId, B.MasterOrderId, B.OwnReferenceNo,
       B.BuyerReferenceNo, B.VendorId, B.Material, B.Article, B.Vendor, B.SKUDesc,
       B.CharVal1, B.CharVal2, B.CharVal3, B.isParent, B.isChild, B.Process,
       B.Consumption, B.WastagePer, B.UOM, B.ParentUOM, B.POUOM, B.RMDescription,
       B.RMCustomerSpec, B.RMVendorSpec, B.SO1, B.SO2, B.SO3
                                
                                ORDER BY isnull(b.Sequence,0)";
                    dtBOMData = _sqlRepository.GetDataTable(strsql);


                }
                int ROW = 6; int COL = 1;

                //foreach (ExcelKnownColors val in Enum.GetValues(typeof(ExcelKnownColors)))
                //{
                //    worksheet[ROW, 1].CellStyle.Interior.ColorIndex = val;
                //    worksheet[ROW, 1].Text = val.ToString();
                //    ROW++;
                //}

                int MasterOrderDetailsStartRow = ROW;
                worksheet[ROW, COL].Text = "Master Order Details:";
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                ROW++;

                int leftColumnCaption = COL;
                int leftColumnValue = leftColumnCaption + 1;

                int MiddleColumnCaption = leftColumnValue + 2;
                int MiddleColumnValue = MiddleColumnCaption + 1;

                int RightColumnCaption = MiddleColumnValue + 2;
                int RightColumnValue = RightColumnCaption + 1;

                //Master Order.............................................................
                //worksheet[ROW, leftColumnCaption].Text = "Master Order No";
                //worksheet[ROW, leftColumnValue].Text = "MasterOrderNo";

                worksheet[ROW, leftColumnCaption].Text = "Order#";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Id"].ToString();
                // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                worksheet.Range[ROW, leftColumnCaption, ROW, leftColumnValue].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Buyer.Ref";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["BuyerReferenceNo"].ToString();
                worksheet.Range[ROW, MiddleColumnCaption, ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, RightColumnCaption].Text = "Own.Ref";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["OwnReferenceNo"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                ROW++;

                worksheet[ROW, leftColumnCaption].Text = "Buyer";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Buyer"].ToString();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Buyer Dep.";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["BuyerDepartment"].ToString();
                worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, RightColumnCaption].Text = "Buyer Div";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["BuyerDevision"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                ROW++;

                worksheet[ROW, leftColumnCaption].Text = "Customer";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Customer"].ToString();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Currency";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["MasterOrderCurrency"].ToString();
                worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;
                ROW++;


                worksheet[ROW, leftColumnCaption].Text = "Total Order Quantity";
                worksheet[ROW, leftColumnValue].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[0]["TotalQuantity"].ToString());
                worksheet[ROW, leftColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                // worksheet[ROW, leftColumnValue, ROW , leftColumnValue].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, leftColumnValue, ROW, leftColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                //worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Bold = true;
                worksheet[ROW, leftColumnValue].HorizontalAlignment = ExcelHAlign.HAlignRight;

                worksheet[ROW, leftColumnValue + 1].Text = dtOrderMaster.Rows[0]["UnitOfMeasurement"].ToString();
                //worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet.Range[MasterOrderDetailsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom44;



                ROW += 2;


                //Master Order Item....................................................................................................
                StringCollection strColSO = new StringCollection();

                for (int i = 0; i < dtMasterOrderItem.Rows.Count; i++)
                {
                    int MasterItemsStartRow = ROW; // row 12
                    worksheet[ROW, COL].Text = "Item Id:"; //col 1
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["MasterOrderItemNo"].ToString();
                    worksheet[ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                    worksheet.Range[ROW, COL, ROW, leftColumnValue].CellStyle.Font.Bold = true;
                    ROW++;


                    // int MasterItemsStartRow = ROW;
                    strColSO = new StringCollection();
                    // worksheet[ROW, leftColumnCaption].Text = "Items Details";



                    worksheet[ROW, leftColumnCaption].Text = "Material";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["Material"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Article";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["Article"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Contract#";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["ContractNo"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, MiddleColumnCaption].Text = "LC Ref";
                    worksheet[ROW, MiddleColumnValue].Text = dtMasterOrderItem.Rows[i]["LCRef"].ToString();
                    worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Buyer Ref";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["BuyerReferenceNo"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, MiddleColumnCaption].Text = "Own Ref";
                    worksheet[ROW, MiddleColumnValue].Text = dtMasterOrderItem.Rows[i]["OwnReferenceNo"].ToString();
                    worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, RightColumnCaption].Text = "Qty";
                    worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                    worksheet[ROW, RightColumnValue].Number = clsStaticInfo.dbl(dtMasterOrderItem.Rows[i]["TotalMOIQuantity"].ToString());
                    //worksheet.Range[ROW, RightColumnValue, ROW, RightColumnValue].CellStyle.Font.Bold = true;
                    // worksheet[ROW, RightColumnValue].CellStyle.Font.Bold = true;
                    worksheet[ROW, RightColumnValue, ROW, RightColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                    worksheet.Range[MasterItemsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom18;
                    ROW++;

                    if (Level == BOMLevel.SO)
                    {
                        dtSalesOrderItem.DefaultView.RowFilter = "MasterOrderItemId='" + dtMasterOrderItem.Rows[i]["MasterOrderItemNo"].ToString() + "'";
                        DataTable dtSalesOrderFilteredByItem = dtSalesOrderItem.DefaultView.ToTable();
                        for (int KK = 0; KK < dtSalesOrderItem.DefaultView.Count; KK++)
                        {


                            if (strColSO.Contains(dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString()))
                                continue;
                            int SOStartRow = ROW;  //row 16
                            worksheet[ROW, COL].Text = "Sales Order Details & Breakdown:";
                            worksheet[ROW, COL].CellStyle.Font.Bold = true;
                            ROW++;

                            // int SOStartRow = ROW;

                            strColSO.Add(dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString());

                            worksheet[ROW, leftColumnCaption].Text = "SO No";
                            worksheet[ROW, leftColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString();
                            worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                            worksheet[ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                            worksheet[ROW, leftColumnValue].CellStyle.Font.Bold = true;


                            worksheet[ROW, MiddleColumnCaption].Text = "Del. Date";
                            worksheet[ROW, MiddleColumnValue].Text = Convert.ToDateTime(dtSalesOrderItem.DefaultView[KK]["DeliveryDate"].ToString()).ToString("dd-MMM-yyyy");
                            worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                            worksheet[ROW, RightColumnCaption].Text = "Qty";
                            worksheet[ROW, RightColumnValue].Number = clsStaticInfo.dbl(dtSalesOrderItem.DefaultView[KK]["Quantity"].ToString());
                            worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                            worksheet[ROW, RightColumnValue, ROW, RightColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                            // worksheet[ROW, RightColumnValue].CellStyle.Font.Bold = true;
                            ROW++;

                            worksheet[ROW, leftColumnCaption].Text = "Dest.";
                            worksheet[ROW, leftColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["Destination"].ToString();
                            worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;


                            worksheet[ROW, MiddleColumnCaption].Text = "Ship Mode";
                            worksheet[ROW, MiddleColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["ShipmentMode"].ToString();
                            worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                            worksheet[ROW, RightColumnCaption].Text = "Ord. Status";
                            worksheet[ROW, RightColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["OrderStatus"].ToString();
                            worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                            worksheet.Range[SOStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom19;

                            ROW++;

                            dtSalesOrderFilteredByItem.DefaultView.RowFilter = "SalesOrderNo='" + dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString() + "'"; //????
                            DataTable dtBreakdownData = dtSalesOrderFilteredByItem.DefaultView.ToTable();
                            DrawSOBreakdownData(dtBreakdownData, worksheet, ref ROW, isMatrix);

                            ROW++;
                            //BOM Data here
                            dtBOMData.DefaultView.RowFilter = "SalesOrderId='" + dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString() + "'";
                            DrawBOMData(dtBOMData.DefaultView.ToTable(), worksheet, ref ROW);

                            ROW++;
                        }
                    }
                    else
                    {
                        DrawBOMData(dtBOMData, worksheet, ref ROW);
                    }

                    ROW += 2; // Gap for Material
                }

                int endCol = RightColumnValue;


                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.CellStyle.Font.Size = 8f;
                worksheet.UsedRange.WrapText = true;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref worksheet, endCol, "Master Order#" + dtOrderMaster.Rows[0]["Id"].ToString(), identity.PlantId);
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;
                return workbook;

            }
            catch (Exception ex)
            {
                throw (ex);

            }
        }

        public IWorkbook OrderLevelBOMReport(string MasterOrderId, BOMLevel Level, bool isMatrix = true)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;
            IWorkbook workbook = application.Workbooks.Create(1);
            //Load the existing Excel workbook into IWorkbook
            if (Level == BOMLevel.Item)
                workbook = application.Workbooks.Create(3);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];


            try
            {
                DataTable dtOrderMaster = _sqlRepository.GetDataTable(@"select mo.Id, mo.type, b.UserName as Buyer, p.UserName as Customer
                ,  mo.OrderYear as Year, mo.TotalQty as TotalQuantity
                    , uom.UserName as UnitOfMeasurement, mo.NoOfLineItem, mo.OrderWastagePercentage
                    , mo.ExtraOrderPercentage, mo.BuyerReferenceNo, mo.OwnReferenceNo, BDept.UserName as BuyerDepartment
                    , BDev.UserName as BuyerDevision, MoCur.Code MasterOrderCurrency from trn.MasterOrder MO 
                    left join scs.Currency MoCur on MoCur.id = mo.CurrencyId 
                    left join scs.UnitOfMeasurement UOM on uom.id = mo.TotalQtyUOMId 
                    left join hkp.buyer B on b.id = mo.buyerid 
                    left join hkp.party p on p.id = mo.partyid 
                    left join hkp.BuyerDepartment BDept on BDept.id = mo.buyerDepartmentid 
                    left join HKP.buyerdivision BDev on BDev.id = mo.BuyerDivisionId where mo.Id='" + MasterOrderId + "'");
                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");
                worksheet.Name = "BOM-MO Level";
                string strsql = @"SELECT K.OwnReferenceNo,k.BuyerReferenceNo,k.VendorId,k.Material,
k.Article,k.Vendor,k.SKUDesc,k.CharVal1,k.CharVal2,k.CharVal3,k.isParent,k.isChild,k.Process,
SUM(k.OrderQty) OrderQty,SUM(k.PlanOrderQty) PlanOrderQty,AVG(k.Consumption) Consumption,AVG(k.WastagePer) WastagePer,
SUM(k.BOMQty) BOMQty,SUM(k.RequiredQty) RequiredQty,SUM(k.RequiredQtyPO) RequiredQtyPO,k.UOM,k.ParentUOM,k.POUOM,k.RMDescription,k.RMCustomerSpec
,k.RMVendorSpec,SUM(k.POQTY) POQTY,SUM(k.GRNQty) GRNQty,k.MOIIds
 From ( SELECT moi.OwnReferenceNo,moi.BuyerReferenceNo, b.VendorId,
                                 mm.UserName AS Material,mma.StandardName AS Article,p.UserName AS Vendor,b.SKUDesc,

                                v1.UserName AS CharVal1,v2.UserName AS CharVal2,v3.UserName AS CharVal3
								,convert(bit,isnull(b.isParent,0)) AS isParent,
                                convert(bit,isnull(b.isChild,0)) AS isChild,PR.UserName AS Process
                               ,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
                                b.BOMQty,b.RequiredQty,b.RequiredQtyPO,uom.UserName AS UOM,uomm.UserName AS ParentUOM,
                                POUOM.UserName AS POUOM,
                                b.RMDescription,	b.RMCustomerSpec,	b.RMVendorSpec
								,ISNULL(po.POQTY,0) POQTY,ISNULL(grn.GRNQty,0) GRNQty,
	                          MOIIds=STUFF((select distinct ','+xMOI.Id
									       from BOQ QX 
							              Join TRN.MasterOrderItem xMOI on xmoi.Id=qx.MasterOrderItemId
                                        WHERE MO.id=xMOI.MasterOrderId and qx.ArticleId=b.ArticleId 
											and isnull(qx.FirstCharacteristicsValueId,'')=isnull(b.FirstCharacteristicsValueId,'')
											and isnull(qx.SecondCharacteristicsValueId,'')=isnull(b.SecondCharacteristicsValueId,'')
											and isnull(qx.ThirdCharacteristicsValueId,'')=isnull(b.ThirdCharacteristicsValueId,'')
											and isnull(qx.RMDescription,'')=isnull(b.RMDescription,'')
											and isnull(qx.RMCustomerSpec,'')=isnull(b.RMCustomerSpec,'')
											and isnull(qx.RMVendorSpec,'')=isnull(b.RMVendorSpec,'')
										for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								FROM BOQ AS b
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS POuom ON POuom.Id=b.POUoMId
                                LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                                LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
                                LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                                LEFT OUTER JOIN trn.masterorder MO ON MO.Id=moi.MasterOrderId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uomm ON uomm.Id=mo.TotalQtyUOMId
                                LEFT JOIN hkp.Process AS pr ON pr.Id=b.ProcessId

                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId
								Left outer join (select BOQDetailId,sum(POBOQQty) as POQTY from trn.POBOQMAP  GRoup by BOQDetailId) PO On PO.BOQDetailId = b.Id
								Left outer join (	
								SELECT a.BOQDetailId , sum(b.TransactionQty) GRNQty 
				FROM trn.POBOQMAP a
				INNER JOIN trn.InventoryReceiveDetail b ON a.PODetailId=b.PODetailsId GRoup by BOQDetailId
								
								) GRN On GRN.BOQDetailId = b.Id

                                WHERE MO.Id='" + MasterOrderId + @"' and isnull(B.Id,'') NOT IN (select ParentId from BOQ 
								JOIn TRN.MasterOrderItem MOI ON MOI.Id=BOQ.MasterOrderItemId
								where isnull(ParentId,'')<>'' AND MO.Id='" + MasterOrderId + @"'
                            
                                )) AS K 
								GROUP BY K.OwnReferenceNo,k.BuyerReferenceNo,k.VendorId,k.Material,
k.Article,k.Vendor,k.SKUDesc,k.CharVal1,k.CharVal2,k.CharVal3,k.isParent,k.isChild,k.Process
,k.UOM,k.ParentUOM,k.POUOM,k.RMDescription,k.RMCustomerSpec,k.RMVendorSpec,k.MOIIds";
                DataTable dtBOMData = new DataTable();
                dtBOMData = _sqlRepository.GetDataTable(strsql);
                int ROW = 6; int COL = 1;

                //foreach (ExcelKnownColors val in Enum.GetValues(typeof(ExcelKnownColors)))
                //{
                //    worksheet[ROW, 1].CellStyle.Interior.ColorIndex = val;
                //    worksheet[ROW, 1].Text = val.ToString();
                //    ROW++;
                //}

                int MasterOrderDetailsStartRow = ROW;
                worksheet[ROW, COL].Text = "Master Order Details:";
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                ROW++;

                int leftColumnCaption = COL;
                int leftColumnValue = leftColumnCaption + 1;

                int MiddleColumnCaption = leftColumnValue + 2;
                int MiddleColumnValue = MiddleColumnCaption + 1;

                int RightColumnCaption = MiddleColumnValue + 2;
                int RightColumnValue = RightColumnCaption + 1;

                //Master Order.............................................................
                //worksheet[ROW, leftColumnCaption].Text = "Master Order No";
                //worksheet[ROW, leftColumnValue].Text = "MasterOrderNo";

                worksheet[ROW, leftColumnCaption].Text = "Order#";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Id"].ToString();
                // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                worksheet.Range[ROW, leftColumnCaption, ROW, leftColumnValue].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Buyer.Ref";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["BuyerReferenceNo"].ToString();
                worksheet.Range[ROW, MiddleColumnCaption, ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, RightColumnCaption].Text = "Own.Ref";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["OwnReferenceNo"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                ROW++;

                worksheet[ROW, leftColumnCaption].Text = "Buyer";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Buyer"].ToString();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Buyer Dep.";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["BuyerDepartment"].ToString();
                worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, RightColumnCaption].Text = "Buyer Div";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["BuyerDevision"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                ROW++;

                worksheet[ROW, leftColumnCaption].Text = "Customer";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Customer"].ToString();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Currency";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["MasterOrderCurrency"].ToString();
                worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;
                ROW++;


                worksheet[ROW, leftColumnCaption].Text = "Total Order Quantity";
                worksheet[ROW, leftColumnValue].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[0]["TotalQuantity"].ToString());
                worksheet[ROW, leftColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                worksheet[ROW, leftColumnCaption].WrapText = true;
                // worksheet[ROW, leftColumnValue, ROW , leftColumnValue].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, leftColumnValue, ROW, leftColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                //worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Bold = true;
                worksheet[ROW, leftColumnValue].HorizontalAlignment = ExcelHAlign.HAlignRight;

                worksheet[ROW, leftColumnValue + 1].Text = dtOrderMaster.Rows[0]["UnitOfMeasurement"].ToString();
                //worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet.Range[MasterOrderDetailsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom44;



                ROW += 2;


                //Master Order Item....................................................................................................
                StringCollection strColSO = new StringCollection();

                OrderLevelBOMData(dtBOMData, worksheet, ref ROW);
                ROW += 2; // Gap for Material
                int endCol = RightColumnValue;


                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.CellStyle.Font.Size = 8f;
                //worksheet.UsedRange.WrapText = true;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref worksheet, endCol, "Master Order#" + dtOrderMaster.Rows[0]["Id"].ToString(), identity.PlantId);
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;
                return workbook;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IWorkbook OrderItemLevelBOMReport(string MasterOrderItemId, BOMLevel Level, bool isMatrix = true)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;
            IWorkbook workbook = application.Workbooks.Create(1);
            //Load the existing Excel workbook into IWorkbook
            if (Level == BOMLevel.Item)
                workbook = application.Workbooks.Create(3);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];


            try
            {
                DataTable dtOrderMaster = _sqlRepository.GetDataTable(@"select mo.Id, MOI.Id MasterOrderItemId,mo.type, b.UserName as Buyer, p.UserName as Customer
                ,  mo.OrderYear as Year, mo.TotalQty as TotalQuantity
                    , uom.UserName as UnitOfMeasurement, mo.NoOfLineItem, mo.OrderWastagePercentage
                    , mo.ExtraOrderPercentage, mo.BuyerReferenceNo, mo.OwnReferenceNo, BDept.UserName as BuyerDepartment
                    , BDev.UserName as BuyerDevision, MoCur.Code MasterOrderCurrency 
					from trn.MasterOrderItem MOI
                    left join TRN.MasterOrder MO on MO.Id=MOI.MasterOrderId 					
                    left join scs.Currency MoCur on MoCur.id = mo.CurrencyId 
                    left join scs.UnitOfMeasurement UOM on uom.id = mo.TotalQtyUOMId 
                    left join hkp.buyer B on b.id = mo.buyerid 
                    left join hkp.party p on p.id = mo.partyid 
                    left join hkp.BuyerDepartment BDept on BDept.id = mo.buyerDepartmentid 
                    left join HKP.buyerdivision BDev on BDev.id = mo.BuyerDivisionId where MOI.Id='" + MasterOrderItemId + "'");
                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");
                worksheet.Name = "BOM-MO Level";
                string strsql = @"SELECT K.OwnReferenceNo,k.BuyerReferenceNo,k.VendorId,k.Material,
k.Article,k.Vendor,k.SKUDesc,k.CharVal1,k.CharVal2,k.CharVal3,k.isParent,k.isChild,k.Process,
SUM(k.OrderQty) OrderQty,SUM(k.PlanOrderQty) PlanOrderQty,AVG(k.Consumption) Consumption,AVG(k.WastagePer) WastagePer,
SUM(k.BOMQty) BOMQty,SUM(k.RequiredQty) RequiredQty,SUM(k.RequiredQtyPO) RequiredQtyPO,k.UOM,k.ParentUOM,k.POUOM,k.RMDescription,k.RMCustomerSpec
,k.RMVendorSpec,SUM(k.POQTY) POQTY,SUM(k.GRNQty) GRNQty,k.MOIIds
 From ( SELECT moi.OwnReferenceNo,moi.BuyerReferenceNo, b.VendorId,
                                 mm.UserName AS Material,mma.StandardName AS Article,p.UserName AS Vendor,b.SKUDesc,

                                v1.UserName AS CharVal1,v2.UserName AS CharVal2,v3.UserName AS CharVal3
								,convert(bit,isnull(b.isParent,0)) AS isParent,
                                convert(bit,isnull(b.isChild,0)) AS isChild,PR.UserName AS Process
                               ,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
                                b.BOMQty,b.RequiredQty,b.RequiredQtyPO,uom.UserName AS UOM,uomm.UserName AS ParentUOM,
                                POUOM.UserName AS POUOM,
                                b.RMDescription,	b.RMCustomerSpec,	b.RMVendorSpec
								,ISNULL(po.POQTY,0) POQTY,ISNULL(grn.GRNQty,0) GRNQty,
	                          MOIIds=STUFF((select distinct ','+xMOI.Id
									       from BOQ QX 
							              Join TRN.MasterOrderItem xMOI on xmoi.Id=qx.MasterOrderItemId
                                        WHERE MO.id=xMOI.MasterOrderId and qx.ArticleId=b.ArticleId 
											and isnull(qx.FirstCharacteristicsValueId,'')=isnull(b.FirstCharacteristicsValueId,'')
											and isnull(qx.SecondCharacteristicsValueId,'')=isnull(b.SecondCharacteristicsValueId,'')
											and isnull(qx.ThirdCharacteristicsValueId,'')=isnull(b.ThirdCharacteristicsValueId,'')
											and isnull(qx.RMDescription,'')=isnull(b.RMDescription,'')
											and isnull(qx.RMCustomerSpec,'')=isnull(b.RMCustomerSpec,'')
											and isnull(qx.RMVendorSpec,'')=isnull(b.RMVendorSpec,'')
										for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								FROM BOQ AS b
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS POuom ON POuom.Id=b.POUoMId
                                LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                                LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
                                LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                                LEFT OUTER JOIN trn.masterorder MO ON MO.Id=moi.MasterOrderId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uomm ON uomm.Id=mo.TotalQtyUOMId
                                LEFT JOIN hkp.Process AS pr ON pr.Id=b.ProcessId

                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId
								Left outer join (select BOQDetailId,sum(POBOQQty) as POQTY from trn.POBOQMAP  GRoup by BOQDetailId) PO On PO.BOQDetailId = b.Id
								Left outer join (	
								SELECT a.BOQDetailId , sum(b.TransactionQty) GRNQty 
				FROM trn.POBOQMAP a
				INNER JOIN trn.InventoryReceiveDetail b ON a.PODetailId=b.PODetailsId GRoup by BOQDetailId
								
								) GRN On GRN.BOQDetailId = b.Id

                               WHERE moi.Id='" + MasterOrderItemId + @"' and isnull(B.Id,'') NOT IN (select ParentId from BOQ 
								JOIn TRN.MasterOrderItem MOI ON MOI.Id=BOQ.MasterOrderItemId
								where isnull(ParentId,'')<>'' AND MOI.Id='" + MasterOrderItemId + @"'
                            
                                )) AS K 
								GROUP BY K.OwnReferenceNo,k.BuyerReferenceNo,k.VendorId,k.Material,
k.Article,k.Vendor,k.SKUDesc,k.CharVal1,k.CharVal2,k.CharVal3,k.isParent,k.isChild,k.Process
,k.UOM,k.ParentUOM,k.POUOM,k.RMDescription,k.RMCustomerSpec,k.RMVendorSpec,k.MOIIds";
                DataTable dtBOMData = new DataTable();
                dtBOMData = _sqlRepository.GetDataTable(strsql);
                int ROW = 6; int COL = 1;

                //foreach (ExcelKnownColors val in Enum.GetValues(typeof(ExcelKnownColors)))
                //{
                //    worksheet[ROW, 1].CellStyle.Interior.ColorIndex = val;
                //    worksheet[ROW, 1].Text = val.ToString();
                //    ROW++;
                //}

                int MasterOrderDetailsStartRow = ROW;
                worksheet[ROW, COL].Text = "Master Order Item:";
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                ROW++;

                int leftColumnCaption = COL;
                int leftColumnValue = leftColumnCaption + 1;

                int MiddleColumnCaption = leftColumnValue + 2;
                int MiddleColumnValue = MiddleColumnCaption + 1;

                int RightColumnCaption = MiddleColumnValue + 2;
                int RightColumnValue = RightColumnCaption + 1;

                //Master Order.............................................................
                //worksheet[ROW, leftColumnCaption].Text = "Master Order No";
                //worksheet[ROW, leftColumnValue].Text = "MasterOrderNo";

                worksheet[ROW, leftColumnCaption].Text = "Order#";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Id"].ToString();
                // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                worksheet.Range[ROW, leftColumnCaption, ROW, leftColumnValue].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Buyer.Ref";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["BuyerReferenceNo"].ToString();
                worksheet.Range[ROW, MiddleColumnCaption, ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, RightColumnCaption].Text = "Own.Ref";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["OwnReferenceNo"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                ROW++;

                worksheet[ROW, leftColumnCaption].Text = "Buyer";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Buyer"].ToString();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Buyer Dep.";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["BuyerDepartment"].ToString();
                worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, RightColumnCaption].Text = "Buyer Div";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["BuyerDevision"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                ROW++;

                worksheet[ROW, leftColumnCaption].Text = "Customer";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Customer"].ToString();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Currency";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["MasterOrderCurrency"].ToString();
                worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;
                ROW++;


                worksheet[ROW, leftColumnCaption].Text = "Total Order Quantity";
                worksheet[ROW, leftColumnValue].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[0]["TotalQuantity"].ToString());
                worksheet[ROW, leftColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                // worksheet[ROW, leftColumnValue, ROW , leftColumnValue].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, leftColumnValue, ROW, leftColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                //worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Bold = true;
                worksheet[ROW, leftColumnValue].HorizontalAlignment = ExcelHAlign.HAlignRight;

                worksheet[ROW, leftColumnValue + 1].Text = dtOrderMaster.Rows[0]["UnitOfMeasurement"].ToString();
                //worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet.Range[MasterOrderDetailsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom44;



                ROW += 2;


                //Master Order Item....................................................................................................
                StringCollection strColSO = new StringCollection();

                OrderLevelBOMData(dtBOMData, worksheet, ref ROW);
                ROW += 2; // Gap for Material
                int endCol = RightColumnValue;


                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.CellStyle.Font.Size = 8f;
                //worksheet.UsedRange.WrapText = true;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref worksheet, endCol, "Master Order#" + dtOrderMaster.Rows[0]["Id"].ToString(), identity.PlantId);
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;
                return workbook;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IWorkbook ConevelBOMReport(string ContractId)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;
            IWorkbook workbook = application.Workbooks.Create(1);
            //Load the existing Excel workbook into IWorkbook
            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];


            try
            {
                DataTable dtOrderMaster = _sqlRepository.GetDataTable(@"select mo.Id, mo.type, b.UserName as Buyer, p.UserName as Customer,mo.PartyId
				                , c.ContractNo,so.ContractId,   c.MasterLCId, ml.LCRef As MasterLCNo, moi.id as MasterOrderItemNo
                                ,  mo.OrderYear as Year, mo.TotalQty as TotalQuantity
                                    , uom.UserName as UnitOfMeasurement, mo.NoOfLineItem, mo.OrderWastagePercentage
                                    , mo.ExtraOrderPercentage, mo.BuyerReferenceNo, mo.OwnReferenceNo, BDept.UserName as BuyerDepartment
                                    , BDev.UserName as BuyerDevision, MoCur.Code MasterOrderCurrency
			    	
					                 from trn.MasterOrder MO 
                                    left join scs.Currency MoCur on MoCur.id = mo.CurrencyId 
                                    left join scs.UnitOfMeasurement UOM on uom.id = mo.TotalQtyUOMId 
                                    left join hkp.buyer B on b.id = mo.buyerid 
                                    left join hkp.party p on p.id = mo.partyid 
                                    left join hkp.BuyerDepartment BDept on BDept.id = mo.buyerDepartmentid 
                                    left join HKP.buyerdivision BDev on BDev.id = mo.BuyerDivisionId 
					                left join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
LEFT JOIN TRN.SalesOrder so on moi.Id=so.MasterOrderItemId
					                left join Contract C on c.id = so.ContractId
					                left join MasterLC ml on ml.Id = c.MasterLCId
                                    where c.Id='" + ContractId + "'");

                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");
                worksheet.Name = "BOM-Contact Level";
                string strsql = @"SELECT K.OwnReferenceNo,k.BuyerReferenceNo,k.VendorId,k.Material,
k.Article,k.Vendor,k.SKUDesc,k.CharVal1,k.CharVal2,k.CharVal3,k.isParent,k.isChild,k.Process,k.RequiredQtyApproved,k.IncompleteMaterial,
SUM(k.OrderQty) OrderQty,SUM(k.PlanOrderQty) PlanOrderQty,AVG(k.Consumption) Consumption,AVG(k.WastagePer) WastagePer,
SUM(k.BOMQty) BOMQty,SUM(k.RequiredQty) RequiredQty,SUM(k.RequiredQtyPO) RequiredQtyPO,k.UOM,k.ParentUOM,k.POUOM,k.RMDescription,k.RMCustomerSpec
,k.RMVendorSpec,SUM(k.POQTY) POQTY,SUM(k.GRNQty) GRNQty,k.SO1,k.SO2,k.SO3,k.POIds,k.GRNIds,k.MOIIds
 From ( SELECT moi.OwnReferenceNo,moi.BuyerReferenceNo, b.VendorId,
                                 mm.UserName AS Material,mma.StandardName AS Article,p.UserName AS Vendor,b.SKUDesc,

                                v1.UserName AS CharVal1,v2.UserName AS CharVal2,v3.UserName AS CharVal3,convert(bit,isnull(b.isParent,0)) AS isParent,
                                convert(bit,isnull(b.isChild,0)) AS isChild,PR.UserName AS Process,
                               CONVERT(BIT, isnull(b.RequiredQtyApproved,0)) AS RequiredQtyApproved
                               ,CONVERT(BIT, isnull(b.IncompleteMaterial,0)) AS IncompleteMaterial
                               ,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
                                b.BOMQty,b.RequiredQty,b.RequiredQtyPO,uom.UserName AS UOM,uomm.UserName AS ParentUOM,
                                POUOM.UserName AS POUOM,
                                b.RMDescription,	b.RMCustomerSpec,	b.RMVendorSpec
								,ISNULL(po.POQTY,0) POQTY,ISNULL(grn.GRNQty,0) GRNQty,
								
								 SO1 =STUFF((select distinct ','+xv1.UserName
								               from BOQFGMapping AS XM	 
										       JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
								               JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.FirstCharacteristicsValueId
								             WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO2 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.SecondCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO3 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.ThirdCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								POIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.PurchaseOrderDetail AS XB2 ON xb2.Id=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								GRNIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.InventoryReceiveDetail AS XB2 ON xb2.PODetailsId=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
	  MOIIds=STUFF((select distinct ','+xMOI.Id
							              FROM TRN.MasterOrderItem xMOI 
                                        WHERE MO.Id=xMOI.MasterOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                  FROM BOQ AS b
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS POuom ON POuom.Id=b.POUoMId
                                LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                                LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
                                LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                                LEFT OUTER JOIN trn.masterorder MO ON MO.Id=moi.MasterOrderId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uomm ON uomm.Id=mo.TotalQtyUOMId
                                LEFT JOIN hkp.Process AS pr ON pr.Id=b.ProcessId

                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId
								Left outer join (select BOQDetailId,sum(POBOQQty) as POQTY from trn.POBOQMAP  GRoup by BOQDetailId) PO On PO.BOQDetailId = b.Id
								Left outer join (	
								SELECT a.BOQDetailId , sum(b.TransactionQty) GRNQty 
				FROM trn.POBOQMAP a
				INNER JOIN trn.InventoryReceiveDetail b ON a.PODetailId=b.PODetailsId GRoup by BOQDetailId
								
								) GRN On GRN.BOQDetailId = b.Id

                                WHERE so.ContractId ='" + ContractId + @"'
                                 and isnull(B.Id,'') NOT IN (select ParentId from BOQ 
								JOIn TRN.MasterOrderItem MOI ON MOI.Id=BOQ.MasterOrderItemId
								where isnull(ParentId,'')<>'' AND MOI.ContractId='" + ContractId + @"'
                            
                                )) AS K GROUP BY K.OwnReferenceNo,k.BuyerReferenceNo,k.VendorId,k.Material,
k.Article,k.Vendor,k.SKUDesc,k.CharVal1,k.CharVal2,k.CharVal3,k.isParent,k.isChild,k.Process,k.RequiredQtyApproved,k.IncompleteMaterial
,k.UOM,k.ParentUOM,k.POUOM,k.RMDescription,k.RMCustomerSpec
,k.RMVendorSpec,k.SO1,k.SO2,k.SO3,k.POIds,k.GRNIds,k.MOIIds";
                DataTable dtConData = new DataTable();
                dtConData = _sqlRepository.GetDataTable(strsql);
                int ROW = 6; int COL = 1;

                //foreach (ExcelKnownColors val in Enum.GetValues(typeof(ExcelKnownColors)))
                //{
                //    worksheet[ROW, 1].CellStyle.Interior.ColorIndex = val;
                //    worksheet[ROW, 1].Text = val.ToString();
                //    ROW++;
                //}
                int MasterOrderDetailsStartRow = ROW;
                worksheet[ROW, COL].Text = "Sales Contract Details:";
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                ROW++;

                int leftColumnCaption = COL;
                int leftColumnValue = leftColumnCaption + 1;

                //int MiddleColumnCaption = leftColumnValue + 2;
                int MiddleColumnCaption = leftColumnValue + 1;
                int MiddleColumnValue = MiddleColumnCaption + 1;

                int RightColumnCaption = MiddleColumnValue + 1;
                int RightColumnValue = RightColumnCaption + 1;

                //Contract.............................................................

                worksheet[ROW, leftColumnCaption].Text = "ContractNo#";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["ContractNo"].ToString();
                worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                worksheet[ROW, leftColumnValue].ColumnWidth = 16;
                worksheet.Range[ROW, leftColumnCaption, ROW, leftColumnValue].CellStyle.Font.Bold = true;
                worksheet[ROW, leftColumnCaption].ColumnWidth = 16;

                worksheet[ROW, MiddleColumnCaption].Text = "Customer";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["Customer"].ToString();
                worksheet.Range[ROW, MiddleColumnCaption, ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;
                worksheet[ROW, MiddleColumnCaption].ColumnWidth = 10;
                worksheet[ROW, MiddleColumnValue].ColumnWidth = 14;

                worksheet[ROW, RightColumnCaption].Text = "Master LC No.";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["MasterLCNo"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                worksheet[ROW, RightColumnCaption].ColumnWidth = 10;
                worksheet[ROW, RightColumnValue].ColumnWidth = 13;
                ROW++;

                worksheet.Range[MasterOrderDetailsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom44;



                ROW += 2;


                //Master Order Item....................................................................................................
                StringCollection strColSO = new StringCollection();

                //OrderLevelBOMData(dtConData, worksheet, ref ROW);
                ContactLevelBOMData(dtConData, worksheet, ref ROW);
                ROW += 2; // Gap for Material
                int endCol = RightColumnValue;


                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.CellStyle.Font.Size = 8f;
                worksheet.UsedRange.WrapText = true;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref worksheet, endCol, "Master Order#" + dtOrderMaster.Rows[0]["Id"].ToString(), identity.PlantId);
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;
                return workbook;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void DrawSOBreakdownData(DataTable dtData, IWorksheet sheet, ref int ROW, bool Matrix = true)
        {

            string FirstCharacteristicsName = "";
            string SecondCharacteristicsName = "";
            string ThirdCharacteristicsName = "";

            DataView dvDistinctCharName = new DataView(dtData.DefaultView.ToTable(true, "FirstCharacteristics")); //all yellow ??
            if (dvDistinctCharName.Count > 0)
                FirstCharacteristicsName = dvDistinctCharName[0]["FirstCharacteristics"].ToString();

            dvDistinctCharName = new DataView(dtData.DefaultView.ToTable(true, "SecondCharacteristics"));
            if (dvDistinctCharName.Count > 0)
                SecondCharacteristicsName = dvDistinctCharName[0]["SecondCharacteristics"].ToString();


            dvDistinctCharName = new DataView(dtData.DefaultView.ToTable(true, "ThirdCharacteristics"));
            if (dvDistinctCharName.Count > 0)
                ThirdCharacteristicsName = dvDistinctCharName[0]["ThirdCharacteristics"].ToString();


            if (FirstCharacteristicsName == "" && SecondCharacteristicsName == "" && ThirdCharacteristicsName == "")
                return;

            if (FirstCharacteristicsName != "" && SecondCharacteristicsName == "" && ThirdCharacteristicsName == "")
            {
                PrintSingleDimensionData(dtData, sheet, FirstCharacteristicsName, ref ROW);
            }

            if (FirstCharacteristicsName != "" && SecondCharacteristicsName != "" && ThirdCharacteristicsName == "")
            {
                if (Matrix == true)
                    PrintMatrixData(dtData, sheet, ref ROW);
                else
                    PrintLinearData(dtData, sheet, ref ROW);
            }


        }
        void PrintSingleDimensionData(DataTable dtData, IWorksheet sheet, string FirstCharacteristicsName, ref int ROW)
        {
            int COL = 1;
            sheet[ROW, COL].Text = FirstCharacteristicsName;  // Heading FirstCharacteristicsName ??? 
            int ColCharValue = COL;
            COL++;
            sheet[ROW, COL].Text = "Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            int colQuantity = COL;

            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            ROW++;

            int StartRow = ROW;
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                sheet[ROW, ColCharValue].Text = dtData.Rows[i]["FirstCharacteristicsValue"].ToString();
                sheet[ROW, colQuantity].Number = clsStaticInfo.dbl(dtData.Rows[i]["Qty"].ToString());
                //sheet[ROW, colQuantity].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, colQuantity].NumberFormat = clsStaticInfo.NumberFormat();
                ROW++;
            }
            sheet[ROW, colQuantity].Formula = "SUM(" + CellAddr(colQuantity, StartRow) + ":" + CellAddr(colQuantity, ROW - 1) + ")";
            // sheet[StartRow, colQuantity, ROW, colQuantity].NumberFormat = "#,##0.00;(#,##0.00)";
            sheet[StartRow, colQuantity, ROW, colQuantity].NumberFormat = clsStaticInfo.NumberFormat();
            //sheet[ROW, colQuantity].NumberFormat =clsStaticInfo.NumberFormat(); //do
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true; //?
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent; //?
        }
        void PrintMatrixData(DataTable dtData, IWorksheet sheet, ref int ROW)
        {

            if (dtData.Rows.Count == 0)
                return;

            int COL = 0;

            COL++;  // 0+1=1 FG Color/FG Size Row 19
            sheet[ROW, COL].Text = dtData.Rows[0]["FirstCharacteristics"].ToString() + "/" + dtData.Rows[0]["SecondCharacteristics"].ToString();
            int colFirstChar = COL;// colFirstChar=FG Color/FG Size
            int colFirstSecCharValue = colFirstChar + 1;

            DataView dvDistinctSecondCharateristicsValues = new DataView(dtData.DefaultView.ToTable(true, "SecondCharacteristicsValue"));
            Dictionary<string, int> dicColumnIndex = new Dictionary<string, int>();
            for (int i = 0; i < dvDistinctSecondCharateristicsValues.Count; i++)
            {
                COL++;
                sheet[ROW, COL].Text = dvDistinctSecondCharateristicsValues[i]["SecondCharacteristicsValue"].ToString();
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                dicColumnIndex.Add(dvDistinctSecondCharateristicsValues[i]["SecondCharacteristicsValue"].ToString(), COL);
                //sheet[ROW, COL].NumberFormat = "#,##0.00;(#,##0.00)";
                // sheet[ROW, COL].NumberFormat = "#,##0.00;(#,##0.00)";
                //sheet[ROW, COL].NumberFormat =clsStaticInfo.NumberFormat([Precision=);
                // sheet[ROW, COL].CellStyle.Font.Bold = true;

            }

            COL++;
            sheet[ROW, COL].Text = "Total Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            int colTotal = COL;

            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true; //row 19 of heading 
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            StringCollection strCol = new StringCollection();
            int StartRow = ROW; //row 20
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                if (strCol.Contains(dtData.Rows[i]["FirstCharacteristicsValue"].ToString()) == false)
                {
                    strCol.Add(dtData.Rows[i]["FirstCharacteristicsValue"].ToString());

                    sheet[ROW, colFirstChar].Text = dtData.Rows[i]["FirstCharacteristicsValue"].ToString();


                    dtData.DefaultView.RowFilter = "FirstCharacteristicsValue='" + dtData.Rows[i]["FirstCharacteristicsValue"].ToString() + "'";
                    for (int SL = 0; SL < dtData.DefaultView.Count; SL++)
                    {
                        sheet[ROW, dicColumnIndex[dtData.DefaultView[SL]["SecondCharacteristicsValue"].ToString()]].Number = clsStaticInfo.dbl(dtData.DefaultView[SL]["Qty"].ToString());
                        //sheet[ROW, dicColumnIndex[dtData.DefaultView[SL]["SecondCharacteristicsValue"].ToString()]].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, dicColumnIndex[dtData.DefaultView[SL]["SecondCharacteristicsValue"].ToString()]].NumberFormat = clsStaticInfo.NumberFormat();
                    }
                    //int colFirstSecCharValue  = colFirstChar + 1;
                    sheet[ROW, colTotal].Formula = "SUM(" + CellAddr(colFirstSecCharValue, ROW) + ":" + CellAddr(colTotal - 1, ROW) + ")";
                    sheet[ROW, colTotal].NumberFormat = clsStaticInfo.NumberFormat();
                    sheet[ROW, colTotal].CellStyle.Font.Bold = true;


                    ROW++;
                }
            }

            sheet[ROW, colFirstChar].Text = "Total Qty"; //row 21
            sheet[ROW, colFirstChar].CellStyle.Font.Bold = true;
            for (int colSum = colFirstSecCharValue; colSum <= colTotal; colSum++)
            {
                sheet[ROW, colSum].Formula = "SUM(" + CellAddr(colSum, StartRow) + ":" + CellAddr(colSum, ROW - 1) + ")";
                //sheet[ROW, colSum].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, colSum].NumberFormat = clsStaticInfo.NumberFormat();
            }
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            //sheet[ROW, endCol].NumberFormat = "#,##0.00;(#,##0.00)";
            sheet[ROW, endCol].NumberFormat = clsStaticInfo.NumberFormat();

            sheet[StartRow, colFirstChar + 1, ROW, colTotal - 1].NumberFormat = clsStaticInfo.NumberFormat();

            sheet.Range[StartRow - 1, colTotal, ROW, colTotal].CellStyle.Font.Bold = true; //???
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
        }
        void PrintLinearData(DataTable dtData, IWorksheet sheet, ref int ROW)
        {

            if (dtData.Rows.Count == 0)
                return;

            int COL = 0;

            COL++;
            sheet[ROW, COL].Text = dtData.Rows[0]["FirstCharacteristics"].ToString();
            int colFirstChar = COL;
            COL++;
            sheet[ROW, COL].Text = dtData.Rows[0]["SecondCharacteristics"].ToString();
            int colSecondChar = COL;
            COL++;
            sheet[ROW, COL].Text = "Qty";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            // sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true;
            sheet[ROW, COL].CellStyle.Font.Bold = true;
            int colQuantity = COL;


            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            StringCollection strCol = new StringCollection();
            int StartRow = ROW;
            for (int i = 0; i < dtData.Rows.Count; i++)
            {


                sheet[ROW, colFirstChar].Text = dtData.Rows[i]["FirstCharacteristicsValue"].ToString();
                sheet[ROW, colSecondChar].Text = dtData.Rows[i]["SecondCharacteristicsValue"].ToString();
                sheet[ROW, colQuantity].Number = clsStaticInfo.dbl(dtData.Rows[i]["Qty"].ToString());


                ROW++;

            }

            sheet[ROW, colFirstChar].Text = "Total Qty";
            sheet[ROW, colFirstChar].CellStyle.Font.Bold = true;

            sheet[ROW, colQuantity].Formula = "SUM(" + CellAddr(colQuantity, StartRow) + ":" + CellAddr(colQuantity, ROW - 1) + ")";
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            sheet[ROW, colQuantity].NumberFormat = clsStaticInfo.NumberFormat();

            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[StartRow - 1, colQuantity, ROW, colQuantity].CellStyle.Font.Bold = true;
        }

        private void DrawBOMData(DataTable dtData, IWorksheet sheet, ref int ROW)
        {

            ROW++;
            sheet[ROW, 1].Text = "BOM Items";
            sheet[ROW, 1].CellStyle.Font.Bold = true;
            ROW++;
            int COL = 1;
            sheet[ROW, COL].Text = "Sl. No";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 6;
            int colSlNo = COL;
            COL++;
            sheet[ROW, COL].Text = "Material";
            sheet[ROW, COL].ColumnWidth = 16;
            int colMaterial = COL;
            COL++;
            sheet[ROW, COL].Text = "Article";
            sheet[ROW, COL].ColumnWidth = 16;
            int colArticle = COL;
            COL++;
            sheet[ROW, COL].Text = "RM Desc";
            sheet[ROW, COL].ColumnWidth = 16;
            int colRMDescription = COL;
            COL++;
            sheet[ROW, COL].Text = "Process";
            sheet[ROW, COL].ColumnWidth = 10;
            int colProcess = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharVal1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharVal2 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharVal3 = COL;
            sheet.Range[ROW - 1, colCharVal1].Text = "RM SKU";
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].Merge();
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Green;
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].CellStyle.Font.Color = ExcelKnownColors.White;



            COL++;
            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharValSO1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharValSO2 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharValSO3 = COL;

            sheet.Range[ROW - 1, colCharValSO1].Text = "FG SKU";
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].Merge();
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].CellStyle.Font.Color = ExcelKnownColors.White;

            COL++;
            sheet[ROW, COL].Text = "Customer Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMCustomerSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMVendorSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU Desc";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUDesc = COL;
            COL++;
            sheet[ROW, COL].Text = "BOQ";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colBOMQty = COL;
            COL++;
            sheet[ROW, COL].Text = "Booking Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colRequiredQty = COL;
            COL++;
            sheet[ROW, COL].Text = "UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Booking Qty (PO UOM)";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colRequiredQtyInPOUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "PO Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colPOQty = COL;
            COL++;
            sheet[ROW, COL].Text = "GRN Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colGRNQty = COL;
            COL++;
            sheet[ROW, COL].Text = "Balance To Purchase";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 14;
            int colBalTOProduce = COL;
            COL++;
            sheet[ROW, COL].Text = "Balance To Recieve";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 14;
            int colBalTORecieve = COL;
            COL++;
            sheet[ROW, COL].Text = "PO UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colPOUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Consumption";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colConsumption = COL;
            COL++;
            sheet[ROW, COL].Text = "Wastage Per";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colWastagePer = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor";
            sheet[ROW, COL].ColumnWidth = 10;
            int colVendor = COL;
            COL++;
            sheet[ROW, COL].Text = "PO NOs";
            sheet[ROW, COL].ColumnWidth = 12;
            int colPOIds = COL;
            COL++;
            sheet[ROW, COL].Text = "GRN Nos";
            sheet[ROW, COL].ColumnWidth = 12;
            int colGRNIds = COL;


            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true; //row 19 of heading 
            sheet.Range[ROW, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            int StartRow = ROW;
            for (int i = 0; i < dtData.Rows.Count; i++)
            {


                sheet[ROW, colSlNo].Number = (i + 1);
                sheet[ROW, colMaterial].Text = dtData.Rows[i]["Material"].ToString();
                sheet[ROW, colArticle].Text = dtData.Rows[i]["Article"].ToString();
                sheet[ROW, colRMDescription].Text = dtData.Rows[i]["RMDescription"].ToString();

                sheet[ROW, colCharVal1].Text = dtData.Rows[i]["CharVal1"].ToString();
                sheet[ROW, colCharVal2].Text = dtData.Rows[i]["CharVal2"].ToString();
                sheet[ROW, colCharVal3].Text = dtData.Rows[i]["CharVal3"].ToString();

                sheet[ROW, colCharValSO1].Text = dtData.Rows[i]["SO1"].ToString();
                sheet[ROW, colCharValSO2].Text = dtData.Rows[i]["SO2"].ToString();
                sheet[ROW, colCharValSO3].Text = dtData.Rows[i]["SO3"].ToString();

                sheet[ROW, colRMCustomerSpec].Text = dtData.Rows[i]["RMCustomerSpec"].ToString();
                sheet[ROW, colRMVendorSpec].Text = dtData.Rows[i]["RMVendorSpec"].ToString();
                sheet[ROW, colSKUDesc].Text = dtData.Rows[i]["SKUDesc"].ToString();
                sheet[ROW, colVendor].Text = dtData.Rows[i]["Vendor"].ToString();
                sheet[ROW, colUOM].Text = dtData.Rows[i]["UOM"].ToString();
                sheet[ROW, colPOUOM].Text = dtData.Rows[i]["POUOM"].ToString();
                sheet[ROW, colProcess].Text = dtData.Rows[i]["Process"].ToString();
                sheet[ROW, colPOIds].Text = dtData.Rows[i]["POIds"].ToString();
                sheet[ROW, colGRNIds].Text = dtData.Rows[i]["GRNIds"].ToString();


                sheet[ROW, colBOMQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["BOMQty"].ToString());
                sheet[ROW, colRequiredQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["RequiredQty"].ToString());
                sheet[ROW, colPOQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["POQTY"].ToString());
                sheet[ROW, colGRNQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["GRNQty"].ToString());
                sheet[ROW, colRequiredQtyInPOUOM].Number = clsStaticInfo.dbl(dtData.Rows[i]["RequiredQtyPO"].ToString());
                sheet[ROW, colBalTOProduce].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colRequiredQtyInPOUOM) + ROW.ToString() + " - " + clsStaticInfo.GetxlsCol(colPOQty) + ROW.ToString() + ")";
                sheet[ROW, colBalTORecieve].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colPOQty) + ROW.ToString() + " - " + clsStaticInfo.GetxlsCol(colGRNQty) + ROW.ToString() + ")";

                sheet[ROW, colConsumption].Number = clsStaticInfo.dbl(dtData.Rows[i]["Consumption"].ToString());
                sheet[ROW, colWastagePer].Number = clsStaticInfo.dbl(dtData.Rows[i]["WastagePer"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            sheet.Range[StartRow, 1, ROW, endCol].WrapText = true;


            sheet.AutoFilters.FilterRange = sheet.Range[StartRow - 1, 1, ROW, endCol];

            for (int C = 0; C < endCol; C++)
            {
                IAutoFilter filter = sheet.AutoFilters[C];
            }


            sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
            sheet.Range[StartRow, colBOMQty, ROW, colBOMQty].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[StartRow, colRequiredQty, ROW, colRequiredQty].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[StartRow, colConsumption, ROW, colConsumption].NumberFormat = clsStaticInfo.NumberFormat(4);
            sheet.Range[StartRow, colWastagePer, ROW, colWastagePer].NumberFormat = clsStaticInfo.NumberFormat(2);

        }

        private void OrderLevelBOMData(DataTable dtData, IWorksheet sheet, ref int ROW)
        {

            ROW++;
            sheet[ROW, 1].Text = "BOM Items";
            sheet[ROW, 1].CellStyle.Font.Bold = true;
            ROW++;
            int COL = 1;
            sheet[ROW, COL].Text = "Sl. No";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 6;
            int colSlNo = COL;
            COL++;
            sheet[ROW, COL].Text = "Material";
            sheet[ROW, COL].ColumnWidth = 16;
            int colMaterial = COL;
            COL++;
            sheet[ROW, COL].Text = "Article";
            sheet[ROW, COL].ColumnWidth = 16;
            int colArticle = COL;
            COL++;
            sheet[ROW, COL].Text = "RM Desc";
            sheet[ROW, COL].ColumnWidth = 16;
            int colRMDescription = COL;
            COL++;
            sheet[ROW, COL].Text = "Process";
            sheet[ROW, COL].ColumnWidth = 10;
            int colProcess = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharVal1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharVal2 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharVal3 = COL;

            COL++;

            sheet[ROW, COL].Text = "Customer Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMCustomerSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "MOI No's";
            sheet[ROW, COL].ColumnWidth = 12;
            int colMOI = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMVendorSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU Desc";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUDesc = COL;
            COL++;
            sheet[ROW, COL].Text = "BOQ";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colBOMQty = COL;
            COL++;
            sheet[ROW, COL].Text = "Booking Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colRequiredQty = COL;
            COL++;
            sheet[ROW, COL].Text = "UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Booking Qty (PO UOM)";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colRequiredQtyInPOUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "PO Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colPOQty = COL;
            COL++;
            sheet[ROW, COL].Text = "GRN Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colGRNQty = COL;
            COL++;
            sheet[ROW, COL].Text = "Balance To Purchase";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 14;
            int colBalTOProduce = COL;
            COL++;
            sheet[ROW, COL].Text = "Balance To Recieve";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 14;
            int colBalTORecieve = COL;
            COL++;
            sheet[ROW, COL].Text = "PO UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colPOUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Consumption";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colConsumption = COL;
            COL++;
            sheet[ROW, COL].Text = "Wastage Per";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colWastagePer = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor";
            sheet[ROW, COL].ColumnWidth = 10;
            int colVendor = COL;



            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true; //row 19 of heading 
            sheet.Range[ROW, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            int StartRow = ROW;
            for (int i = 0; i < dtData.Rows.Count; i++)
            {


                sheet[ROW, colSlNo].Number = (i + 1);
                sheet[ROW, colMaterial].Text = dtData.Rows[i]["Material"].ToString();
                sheet[ROW, colArticle].Text = dtData.Rows[i]["Article"].ToString();
                sheet[ROW, colRMDescription].Text = dtData.Rows[i]["RMDescription"].ToString();


                sheet[ROW, colMOI].Text = dtData.Rows[i]["MOIIds"].ToString();
                sheet[ROW, colRMCustomerSpec].Text = dtData.Rows[i]["RMCustomerSpec"].ToString();
                sheet[ROW, colRMVendorSpec].Text = dtData.Rows[i]["RMVendorSpec"].ToString();
                sheet[ROW, colSKUDesc].Text = dtData.Rows[i]["SKUDesc"].ToString();
                sheet[ROW, colVendor].Text = dtData.Rows[i]["Vendor"].ToString();
                sheet[ROW, colUOM].Text = dtData.Rows[i]["UOM"].ToString();
                sheet[ROW, colPOUOM].Text = dtData.Rows[i]["POUOM"].ToString();
                sheet[ROW, colProcess].Text = dtData.Rows[i]["Process"].ToString();

                sheet[ROW, colCharVal1].Text = dtData.Rows[i]["CharVal1"].ToString();
                sheet[ROW, colCharVal2].Text = dtData.Rows[i]["CharVal2"].ToString();
                sheet[ROW, colCharVal3].Text = dtData.Rows[i]["CharVal3"].ToString();

                sheet[ROW, colBOMQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["BOMQty"].ToString());
                sheet[ROW, colRequiredQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["RequiredQty"].ToString());
                sheet[ROW, colPOQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["POQTY"].ToString());
                sheet[ROW, colGRNQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["GRNQty"].ToString());
                sheet[ROW, colRequiredQtyInPOUOM].Number = clsStaticInfo.dbl(dtData.Rows[i]["RequiredQtyPO"].ToString());
                sheet[ROW, colBalTOProduce].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colRequiredQtyInPOUOM) + ROW.ToString() + " - " + clsStaticInfo.GetxlsCol(colPOQty) + ROW.ToString() + ")";
                sheet[ROW, colBalTORecieve].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colPOQty) + ROW.ToString() + " - " + clsStaticInfo.GetxlsCol(colGRNQty) + ROW.ToString() + ")";

                sheet[ROW, colConsumption].Number = clsStaticInfo.dbl(dtData.Rows[i]["Consumption"].ToString());
                sheet[ROW, colWastagePer].Number = clsStaticInfo.dbl(dtData.Rows[i]["WastagePer"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            sheet.Range[StartRow, 1, ROW, endCol].WrapText = true;


            sheet.AutoFilters.FilterRange = sheet.Range[StartRow - 1, 1, ROW, endCol];

            for (int C = 0; C < endCol; C++)
            {
                IAutoFilter filter = sheet.AutoFilters[C];
            }


            sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
            sheet.Range[StartRow, colBOMQty, ROW, colBOMQty].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[StartRow, colRequiredQty, ROW, colRequiredQty].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[StartRow, colConsumption, ROW, colConsumption].NumberFormat = clsStaticInfo.NumberFormat(4);
            sheet.Range[StartRow, colWastagePer, ROW, colWastagePer].NumberFormat = clsStaticInfo.NumberFormat(2);

        }

        private void ContactLevelBOMData(DataTable dtData, IWorksheet sheet, ref int ROW)
        {

            ROW++;
            sheet[ROW, 1].Text = "BOM Items";
            sheet[ROW, 1].CellStyle.Font.Bold = true;
            ROW++;
            int COL = 1;
            sheet[ROW, COL].Text = "Sl. No";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 6;
            int colSlNo = COL;
            COL++;
            sheet[ROW, COL].Text = "Material";
            sheet[ROW, COL].ColumnWidth = 16;
            int colMaterial = COL;
            COL++;
            sheet[ROW, COL].Text = "Article";
            sheet[ROW, COL].ColumnWidth = 16;
            int colArticle = COL;
            COL++;
            sheet[ROW, COL].Text = "RM Desc";
            sheet[ROW, COL].ColumnWidth = 16;
            int colRMDescription = COL;
            COL++;
            sheet[ROW, COL].Text = "Process";
            sheet[ROW, COL].ColumnWidth = 10;
            int colProcess = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharVal1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharVal2 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharVal3 = COL;
            sheet.Range[ROW - 1, colCharVal1].Text = "RM SKU";
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].Merge();
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Green;
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].CellStyle.Font.Color = ExcelKnownColors.White;



            COL++;
            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharValSO1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharValSO2 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharValSO3 = COL;

            sheet.Range[ROW - 1, colCharValSO1].Text = "FG SKU";
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].Merge();
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].CellStyle.Font.Color = ExcelKnownColors.White;

            COL++;
            sheet[ROW, COL].Text = "Customer Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMCustomerSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "MOI No's";
            sheet[ROW, COL].ColumnWidth = 12;
            int colMOI = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMVendorSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU Desc";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUDesc = COL;
            COL++;
            sheet[ROW, COL].Text = "BOQ";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colBOMQty = COL;
            COL++;
            sheet[ROW, COL].Text = "Booking Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colRequiredQty = COL;
            COL++;
            sheet[ROW, COL].Text = "UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Booking Qty (PO UOM)";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colRequiredQtyInPOUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "PO Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colPOQty = COL;
            COL++;
            sheet[ROW, COL].Text = "GRN Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colGRNQty = COL;
            COL++;
            sheet[ROW, COL].Text = "Balance To Purchase";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 14;
            int colBalTOProduce = COL;
            COL++;
            sheet[ROW, COL].Text = "Balance To Recieve";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 14;
            int colBalTORecieve = COL;
            COL++;
            sheet[ROW, COL].Text = "PO UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colPOUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Consumption";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colConsumption = COL;
            COL++;
            sheet[ROW, COL].Text = "Wastage Per";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colWastagePer = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor";
            sheet[ROW, COL].ColumnWidth = 10;
            int colVendor = COL;
            COL++;
            sheet[ROW, COL].Text = "PO NOs";
            sheet[ROW, COL].ColumnWidth = 12;
            int colPOIds = COL;
            COL++;
            sheet[ROW, COL].Text = "GRN Nos";
            sheet[ROW, COL].ColumnWidth = 12;
            int colGRNIds = COL;


            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true; //row 19 of heading 
            sheet.Range[ROW, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            int StartRow = ROW;
            for (int i = 0; i < dtData.Rows.Count; i++)
            {


                sheet[ROW, colSlNo].Number = (i + 1);
                sheet[ROW, colMaterial].Text = dtData.Rows[i]["Material"].ToString();
                sheet[ROW, colArticle].Text = dtData.Rows[i]["Article"].ToString();
                sheet[ROW, colRMDescription].Text = dtData.Rows[i]["RMDescription"].ToString();

                sheet[ROW, colCharVal1].Text = dtData.Rows[i]["CharVal1"].ToString();
                sheet[ROW, colCharVal2].Text = dtData.Rows[i]["CharVal2"].ToString();
                sheet[ROW, colCharVal3].Text = dtData.Rows[i]["CharVal3"].ToString();

                sheet[ROW, colCharValSO1].Text = dtData.Rows[i]["SO1"].ToString();
                sheet[ROW, colCharValSO2].Text = dtData.Rows[i]["SO2"].ToString();
                sheet[ROW, colCharValSO3].Text = dtData.Rows[i]["SO3"].ToString();
                sheet[ROW, colMOI].Text = dtData.Rows[i]["MOIIds"].ToString();
                sheet[ROW, colRMCustomerSpec].Text = dtData.Rows[i]["RMCustomerSpec"].ToString();
                sheet[ROW, colRMVendorSpec].Text = dtData.Rows[i]["RMVendorSpec"].ToString();
                sheet[ROW, colSKUDesc].Text = dtData.Rows[i]["SKUDesc"].ToString();
                sheet[ROW, colVendor].Text = dtData.Rows[i]["Vendor"].ToString();
                sheet[ROW, colUOM].Text = dtData.Rows[i]["UOM"].ToString();
                sheet[ROW, colPOUOM].Text = dtData.Rows[i]["POUOM"].ToString();
                sheet[ROW, colProcess].Text = dtData.Rows[i]["Process"].ToString();
                sheet[ROW, colPOIds].Text = dtData.Rows[i]["POIds"].ToString();
                sheet[ROW, colGRNIds].Text = dtData.Rows[i]["GRNIds"].ToString();


                sheet[ROW, colBOMQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["BOMQty"].ToString());
                sheet[ROW, colRequiredQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["RequiredQty"].ToString());
                sheet[ROW, colPOQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["POQTY"].ToString());
                sheet[ROW, colGRNQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["GRNQty"].ToString());
                sheet[ROW, colRequiredQtyInPOUOM].Number = clsStaticInfo.dbl(dtData.Rows[i]["RequiredQtyPO"].ToString());
                sheet[ROW, colBalTOProduce].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colRequiredQtyInPOUOM) + ROW.ToString() + " - " + clsStaticInfo.GetxlsCol(colPOQty) + ROW.ToString() + ")";
                sheet[ROW, colBalTORecieve].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colPOQty) + ROW.ToString() + " - " + clsStaticInfo.GetxlsCol(colGRNQty) + ROW.ToString() + ")";

                sheet[ROW, colConsumption].Number = clsStaticInfo.dbl(dtData.Rows[i]["Consumption"].ToString());
                sheet[ROW, colWastagePer].Number = clsStaticInfo.dbl(dtData.Rows[i]["WastagePer"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            sheet.Range[StartRow, 1, ROW, endCol].WrapText = true;


            sheet.AutoFilters.FilterRange = sheet.Range[StartRow - 1, 1, ROW, endCol];

            for (int C = 0; C < endCol; C++)
            {
                IAutoFilter filter = sheet.AutoFilters[C];
            }


            sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
            sheet.Range[StartRow, colBOMQty, ROW, colBOMQty].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[StartRow, colRequiredQty, ROW, colRequiredQty].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[StartRow, colConsumption, ROW, colConsumption].NumberFormat = clsStaticInfo.NumberFormat(4);
            sheet.Range[StartRow, colWastagePer, ROW, colWastagePer].NumberFormat = clsStaticInfo.NumberFormat(2);

        }

        public void DrawBOMTemplateData(IWorksheet sheet, string MasterOrderItemId)
        {
            string sql = @"SELECT att.BOMMasterId, b.[Description] AS BOMDesc,mmf.UserName FGMaterial,mmaf.StandardName AS FGArticle, mm.UserName RMMaterial,mma.StandardName AS RMArticle,p.UserName AS Vendor,
                                    pr.UserName AS Process,uom.UserName AS UOM,bs.[Description] AS SKUDesc,
                                    bd.[Description], bd.CustomerSpec, bd.VendorSpec, bd.Consumption, bd.WastagePer,
                                    bd.IsSKUCommon AS RMSKUCommon,

                                    --COMMON RM MAPPING---
                                    RMC1.UserName AS RM1CHAR,RMC2.UserName AS RM2CHAR,RMC3.UserName AS RM3CHAR,
                                    RMV1.UserName AS RMV1CHARVAL,RMV2.UserName AS RMV2CHARVAL,RMV3.UserName AS RMV3CHARVAL,
                                    --FG MAPPING
                                    FGC1.UserName AS FG1CHAR,FGC2.UserName AS FG2CHAR,FGC3.UserName AS FG3CHAR,
                                    FGV1.UserName AS FGV1CHARVAL,FGV2.UserName AS FGV2CHARVAL,FGV3.UserName AS FGV3CHARVAL,

                                    BS.IsFirstCharacteristicCommon, BS.IsSecondCharacteristicCommon,BS.IsThirdCharacteristicCommon,

                                    SKUC1.UserName AS SKU1CHAR,SKUC2.UserName AS SKU2CHAR,SKUC3.UserName AS SKU3CHAR,
                                    SKUV1.UserName AS SKUV1CHARVAL,SKUV2.UserName AS SKUV2CHARVAL,SKUV3.UserName AS SKUV3CHARVAL
                                    --------------------

                                     FROM BOMMasterAttachmentWithItem AS ATT
                                    INNER JOIN BOMMaster AS b ON b.Id=att.BOMMasterId
                                    INNER JOIN BOMAttachmentDetail AS bd ON bd.BOMMasterAttachmentWithItemId=att.Id
                                    LEFT JOIN BOMAttachmentSKUMapping AS bs ON bs.BOMAttachmentDetailId=bd.Id 
					                                    AND  isnull(bs.FGFirstCharacteristicsValueId,'') IN (
						                                    SELECT '' UNION
					                                    SELECT fc.CharacteristicsValueId FROM trn.FirstCharacteristics AS fc
					                                    INNER JOIN trn.SalesOrder AS so ON so.Id=fc.SalesOrderId
					                                    WHERE so.MasterOrderItemId='" + MasterOrderItemId + @"'
					                                    )

						                                    AND  isnull(bs.FGSecondCharacteristicsValueId,'') IN (
							                                    SELECT '' UNION
						                                    SELECT sc.CharacteristicsValueId FROM trn.SecondCharacteristics AS sc 
						                                    INNER JOIN trn.SalesOrder AS so ON so.Id=sc.SalesOrderId
						                                    WHERE so.MasterOrderItemId='" + MasterOrderItemId + @"'
						                                    )


					                                    AND  isnull(bs.FGFirstCharacteristicsValueId,'') IN (
						                                    SELECT '' UNION
					                                    SELECT fc.CharacteristicsValueId FROM trn.ThirdCharacteristics AS  fc
					                                    INNER JOIN trn.SalesOrder AS so ON so.Id=fc.SalesOrderId
					                                    WHERE so.MasterOrderItemId='" + MasterOrderItemId + @"'
						                                    )

                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=att.MasterOrderItemId
                                    LEFT OUTER JOIN mst.MaterialMaster AS mmf ON mmf.Id=moi.MaterialMasterId
                                    LEFT OUTER JOIN mst.MaterialMasterArticle AS mmaf ON mmaf.Id=moi.ArticleId


                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV1 ON RMv1.Id=bd.FirstCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV2 ON RMv2.Id=bd.SecondCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV3 ON RMv3.Id=bd.ThirdCharacteristicsValueId

                                    LEFT OUTER JOIN [HKP].[Characteristics] RMC1 ON RMC1.Id=RMV1.CharacteristicsId
                                    LEFT OUTER JOIN [HKP].[Characteristics] RMC2 ON RMC2.Id=RMV2.CharacteristicsId
                                    LEFT OUTER JOIN [HKP].[Characteristics] RMC3 ON RMC3.Id=RMV3.CharacteristicsId


                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] FGV1 ON FGv1.Id=bs.FGFirstCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] FGV2 ON FGv2.Id=bs.FGSecondCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] FGV3 ON FGv3.Id=bs.FGThirdCharacteristicsValueId

                                    LEFT JOIN BOMAttachmentSKUMapping AS bscf ON bs.BOMAttachmentDetailId=bd.Id AND bscf.id =(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping WHERE BOMAttachmentDetailId=bd.Id AND isnull(IsFirstCharacteristicCommon,0)=1)
                                    LEFT JOIN BOMAttachmentSKUMapping AS bscs ON bs.BOMAttachmentDetailId=bd.Id AND bscs.id =(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping WHERE BOMAttachmentDetailId=bd.Id AND isnull(IsSecondCharacteristicCommon,0)=1)
                                    LEFT JOIN BOMAttachmentSKUMapping AS bsct ON bs.BOMAttachmentDetailId=bd.Id AND bsct.id =(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping WHERE BOMAttachmentDetailId=bd.Id AND isnull(IsthirdCharacteristicCommon,0)=1)
                                    LEFT OUTER JOIN [HKP].[Characteristics] FGC1 ON FGC1.Id=isnull(bscf.FGFirstCharacteristicsId,FGV1.CharacteristicsId)
                                    LEFT OUTER JOIN [HKP].[Characteristics] FGC2 ON FGC2.Id=isnull(bscs.FGSecondCharacteristicsId,FGV2.CharacteristicsId)
                                    LEFT OUTER JOIN [HKP].[Characteristics] FGC3 ON FGC3.Id=isnull(bsct.FGThirdCharacteristicsId,FGV3.CharacteristicsId)

                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV1 ON SKUv1.Id=bs.RMFirstCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV2 ON SKUv2.Id=bs.RMSecondCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV3 ON SKUv3.Id=bs.RMThirdCharacteristicsValueId

                                    LEFT OUTER JOIN [HKP].[Characteristics] SKUC1 ON SKUC1.Id=SKUV1.CharacteristicsId
                                    LEFT OUTER JOIN [HKP].[Characteristics] SKUC2 ON SKUC2.Id=SKUV2.CharacteristicsId
                                    LEFT OUTER JOIN [HKP].[Characteristics] SKUC3 ON SKUC3.Id=SKUV3.CharacteristicsId

                                    LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=bd.RMMaterialMasterId
                                    LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=bd.RMArticleId
                                    LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=BD.UoMId
                                    LEFT OUTER JOIN HKP.Party P ON p.Id=bd.VendorId
                                    LEFT JOIN hkp.Process AS pr ON pr.Id=bd.ProcessId

                                    WHERE att.MasterOrderItemId='" + MasterOrderItemId + @"'
                                    ORDER BY bd.Sequence";

            sheet.Name = "BOM Template";

            DataTable dtData = _sqlRepository.GetDataTable(sql);
            int ROW = 1;
            sheet[ROW, 1].Text = "BOM Template Items";
            sheet.Range[ROW, 1, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet[ROW, 1].CellStyle.Font.Bold = true;
            ROW++;
            sheet[ROW, 1].Text = "BOM Id";
            sheet[ROW, 3].Text = dtData.Rows[0]["BOMMasterId"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "BOM Desc";
            sheet[ROW, 3].Text = dtData.Rows[0]["BOMDesc"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "Material";
            sheet[ROW, 3].Text = dtData.Rows[0]["FGMaterial"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "Article";
            sheet[ROW, 3].Text = dtData.Rows[0]["FGArticle"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW += 2;
            int COL = 1;
            sheet[ROW, COL].Text = "Sl. No";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 6;
            int colSlNo = COL;
            COL++;
            sheet[ROW, COL].Text = "Material";
            sheet[ROW, COL].ColumnWidth = 16;
            int colMaterial = COL;
            COL++;
            sheet[ROW, COL].Text = "Article";
            sheet[ROW, COL].ColumnWidth = 40;
            int colArticle = COL;
            COL++;
            sheet[ROW, COL].Text = "RM Desc";
            sheet[ROW, COL].ColumnWidth = 22;
            int colRMDescription = COL;

            COL++;
            sheet[ROW, COL].Text = "Customer Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMCustomerSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMVendorSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU Desc";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUDesc = COL;
            COL++;
            sheet[ROW, COL].Text = "Cons.";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colConsumption = COL;
            COL++;
            sheet[ROW, COL].Text = "UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Wast.%";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colWastagePer = COL;
            COL++;
            sheet[ROW, COL].Text = "Process";
            sheet[ROW, COL].ColumnWidth = 8;
            int colProcess = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor";
            sheet[ROW, COL].ColumnWidth = 20;
            int colVendor = COL;
            COL++;
            sheet[ROW, COL].Text = "Common SKU";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMSKUCommon = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV2 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV3 = COL;

            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].Merge();
            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].Text = "COMMON SKU";
            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colRMV1, ROW, colRMV3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;

            COL++;
            sheet[ROW, COL].Text = "FG SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV1 = COL;
            COL++;
            sheet[ROW, COL].Text = "RM SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV1 = COL;
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].Merge();
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].Text = "FG SKU-1 MAPPING";
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV1, ROW, colSKUV1].CellStyle.Interior.ColorIndex = ExcelKnownColors.LightGreen;



            COL++;
            sheet[ROW, COL].Text = "FG SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV2 = COL;

            COL++;
            sheet[ROW, COL].Text = "RM SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV2 = COL;

            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].Merge();
            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].Text = "FG SKU-2 MAPPING";
            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV2, ROW, colSKUV2].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;



            COL++;
            sheet[ROW, COL].Text = "FG SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV3 = COL;
            COL++;
            sheet[ROW, COL].Text = "RM SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV3 = COL;

            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].Merge();
            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].Text = "FG SKU-3 MAPPING";
            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV3, ROW, colSKUV3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_orange;

            sheet.Range[ROW - 1, 1, ROW, COL].CellStyle.Font.Bold = true; //row 19 of heading 
            sheet.Range[ROW - 1, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, colRMV1 - 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            int StartRow = ROW; //row 20
            for (int i = 0; i < dtData.Rows.Count; i++)
            {


                sheet[ROW, colSlNo].Number = (i + 1);
                sheet[ROW, colMaterial].Text = dtData.Rows[i]["RMMaterial"].ToString();
                sheet[ROW, colArticle].Text = dtData.Rows[i]["RMArticle"].ToString();
                sheet[ROW, colRMDescription].Text = dtData.Rows[i]["Description"].ToString();

                sheet[ROW, colRMCustomerSpec].Text = dtData.Rows[i]["CustomerSpec"].ToString();
                sheet[ROW, colRMVendorSpec].Text = dtData.Rows[i]["VendorSpec"].ToString();
                sheet[ROW, colSKUDesc].Text = dtData.Rows[i]["SKUDesc"].ToString();
                sheet[ROW, colVendor].Text = dtData.Rows[i]["Vendor"].ToString();
                sheet[ROW, colUOM].Text = dtData.Rows[i]["UOM"].ToString();
                sheet[ROW, colProcess].Text = dtData.Rows[i]["Process"].ToString();

                sheet[ROW, colRMSKUCommon].Text = dtData.Rows[i]["RMSKUCommon"].ToString();

                sheet[ROW, colRMV1].Text = dtData.Rows[i]["RMV1CHARVAL"].ToString();
                sheet[ROW, colRMV2].Text = dtData.Rows[i]["RMV2CHARVAL"].ToString();
                sheet[ROW, colRMV3].Text = dtData.Rows[i]["RMV3CHARVAL"].ToString();



                sheet[ROW, colFGV1].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsFirstCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["FG1CHAR"].ToString() + "]" : dtData.Rows[i]["FGV1CHARVAL"].ToString();
                sheet[ROW, colFGV2].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsSecondCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["FG2CHAR"].ToString() + "]" : dtData.Rows[i]["FGV2CHARVAL"].ToString();
                sheet[ROW, colFGV3].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsThirdCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["FG3CHAR"].ToString() + "]" : dtData.Rows[i]["FGV3CHARVAL"].ToString();


                sheet[ROW, colSKUV1].Text = dtData.Rows[i]["SKUV1CHARVAL"].ToString();
                sheet[ROW, colSKUV2].Text = dtData.Rows[i]["SKUV2CHARVAL"].ToString();
                sheet[ROW, colSKUV3].Text = dtData.Rows[i]["SKUV3CHARVAL"].ToString();

                sheet[ROW, colConsumption].Number = clsStaticInfo.dbl(dtData.Rows[i]["Consumption"].ToString());
                sheet[ROW, colWastagePer].Number = clsStaticInfo.dbl(dtData.Rows[i]["WastagePer"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            sheet.IsGridLinesVisible = false;

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

            sheet.Range[8, 4].FreezePanes();

            sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
            sheet.Range[StartRow, colConsumption, ROW, colConsumption].NumberFormat = clsStaticInfo.NumberFormat(4);
            sheet.Range[StartRow, colWastagePer, ROW, colWastagePer].NumberFormat = clsStaticInfo.NumberFormat(2);

        }
        public void DrawBOMTemplateDataSubMaterial(IWorksheet sheet, string MasterOrderItemId)
        {
            string sql = @"SELECT att.BOMMasterId, b.[Description] AS BOMDesc, mmf.UserName FGMaterial,mmaf.StandardName AS FGArticle, mm.UserName RMMaterial,mma.StandardName AS RMArticle,

                                        submm.UserName SubRMMaterial,submma.StandardName AS SubRMArticle,subp.UserName AS Vendor,
                                        subpr.UserName AS Process,subuom.UserName AS UOM,ADCS.[Description] AS SKUDesc,
                                        adc.[Description], adc.CustomerSpec, adc.VendorSpec, adc.Consumption, adc.WastagePer,
                                        adc.IsSKUCommon AS RMSKUCommon,



                                        --COMMON RM MAPPING---
                                        RMC1.UserName AS RM1CHAR,RMC2.UserName AS RM2CHAR,RMC3.UserName AS RM3CHAR,
                                        RMV1.UserName AS RMV1CHARVAL,RMV2.UserName AS RMV2CHARVAL,RMV3.UserName AS RMV3CHARVAL,

                                        --ADCS.IsFirstCharacteristicCommon, ADCS.IsSecondCharacteristicCommon,ADCS.IsThirdCharacteristicCommon,

                                        SKUC1.UserName AS SKU1CHAR,SKUC2.UserName AS SKU2CHAR,SKUC3.UserName AS SKU3CHAR,
                                        SKUV1.UserName AS SKUV1CHARVAL,SKUV2.UserName AS SKUV2CHARVAL,SKUV3.UserName AS SKUV3CHARVAL,


                                        --COMMON RM MAPPING FOR SUB MATERIAL---
                                        SUBRMC1.UserName AS SUBRM1CHAR,SUBRMC2.UserName AS SUBRM2CHAR,SUBRMC3.UserName AS SUBRM3CHAR,
                                        SUBRMV1.UserName AS SUBRMV1CHARVAL,SUBRMV2.UserName AS SUBRMV2CHARVAL,SUBRMV3.UserName AS SUBRMV3CHARVAL,

                                        ADCS.IsFirstCharacteristicCommon, ADCS.IsSecondCharacteristicCommon,ADCS.IsThirdCharacteristicCommon,

                                        SUBSKUC1.UserName AS SUBSKU1CHAR,SUBSKUC2.UserName AS SUBSKU2CHAR,SUBSKUC3.UserName AS SUBSKU3CHAR,
                                        SUBSKUV1.UserName AS SUBSKUV1CHARVAL,SUBSKUV2.UserName AS SUBSKUV2CHARVAL,SUBSKUV3.UserName AS SUBSKUV3CHARVAL
                                        --------------------

                                         FROM BOMMasterAttachmentWithItem AS ATT
                                        INNER JOIN BOMMaster AS b ON b.Id=att.BOMMasterId
                                        INNER JOIN BOMAttachmentDetail AS bd ON bd.BOMMasterAttachmentWithItemId=att.Id
                                        LEFT JOIN BOMAttachmentSKUMapping AS bs ON bs.BOMAttachmentDetailId=bd.Id 
					                                        AND  isnull(bs.FGFirstCharacteristicsValueId,'') IN (
						                                        SELECT '' UNION
					                                        SELECT fc.CharacteristicsValueId FROM trn.FirstCharacteristics AS fc
					                                        INNER JOIN trn.SalesOrder AS so ON so.Id=fc.SalesOrderId
					                                        WHERE so.MasterOrderItemId='" + MasterOrderItemId + @"'
					                                        )

						                                        AND  isnull(bs.FGSecondCharacteristicsValueId,'') IN (
							                                        SELECT '' UNION
						                                        SELECT sc.CharacteristicsValueId FROM trn.SecondCharacteristics AS sc 
						                                        INNER JOIN trn.SalesOrder AS so ON so.Id=sc.SalesOrderId
						                                        WHERE so.MasterOrderItemId='" + MasterOrderItemId + @"'
						                                        )


					                                        AND  isnull(bs.FGFirstCharacteristicsValueId,'') IN (
						                                        SELECT '' UNION
					                                        SELECT fc.CharacteristicsValueId FROM trn.ThirdCharacteristics AS  fc
					                                        INNER JOIN trn.SalesOrder AS so ON so.Id=fc.SalesOrderId
					                                        WHERE so.MasterOrderItemId='" + MasterOrderItemId + @"'
						                                        )

                                        INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=att.MasterOrderItemId
                                        LEFT OUTER JOIN mst.MaterialMaster AS mmf ON mmf.Id=moi.MaterialMasterId
                                        LEFT OUTER JOIN mst.MaterialMasterArticle AS mmaf ON mmaf.Id=moi.ArticleId


                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV1 ON RMv1.Id=bd.FirstCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV2 ON RMv2.Id=bd.SecondCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV3 ON RMv3.Id=bd.ThirdCharacteristicsValueId

                                        LEFT OUTER JOIN [HKP].[Characteristics] RMC1 ON RMC1.Id=RMV1.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] RMC2 ON RMC2.Id=RMV2.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] RMC3 ON RMC3.Id=RMV3.CharacteristicsId

                                        LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=bd.RMMaterialMasterId
                                        LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=bd.RMArticleId
                                        LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=BD.UoMId
                                        LEFT OUTER JOIN HKP.Party P ON p.Id=bd.VendorId
                                        LEFT JOIN hkp.Process AS pr ON pr.Id=bd.ProcessId

                                        JOIN AttachmentDetailConsumption AS adc ON adc.BOMAttachmentDetailId=bd.Id
                                        LEFT JOIN mst.MaterialMaster AS submm ON submm.Id=adc.RMMaterialMasterId
                                        LEFT JOIN mst.MaterialMasterArticle AS submma ON submma.Id=adc.RMArticleId
                                        LEFT JOIN scs.UnitOfMeasurement AS subuom ON subuom.Id=adc.UoMId
                                        LEFT JOIN HKP.Party subP ON subp.Id=adc.VendorId
                                        LEFT JOIN hkp.Process AS subpr ON subpr.Id=adc.ProcessId 

                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBRMV1 ON SUBRMV1.Id=ADC.FirstCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBRMV2 ON SUBRMV2.Id=ADC.SecondCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBRMV3 ON SUBRMV3.Id=ADC.ThirdCharacteristicsValueId

                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBRMC1 ON SUBRMC1.Id=SUBRMV1.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBRMC2 ON SUBRMC2.Id=SUBRMV2.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBRMC3 ON SUBRMC3.Id=SUBRMV3.CharacteristicsId


                                        LEFT JOIN AttachmentDetailConsumptionSKUMapping AS adcs ON adcs.AttachmentDetailConsumptionId=adc.Id


                                       LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBSKUV1 ON SUBSKUV1.Id=ADCS.SubFirstCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBSKUV2 ON SUBSKUV2.Id=ADCS.SubSecondCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBSKUV3 ON SUBSKUV3.Id=ADCS.SubThirdCharacteristicsValueId

                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBSKUC1 ON SUBSKUC1.Id=SUBSKUV1.CharacteristicsId OR SUBSKUC1.Id=(SELECT TOP 1 RMFirstCharacteristicsId FROM AttachmentDetailConsumptionSKUMapping WHERE AttachmentDetailConsumptionId=adc.Id AND isnull(IsFirstCharacteristicCommon,0)=1)
                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBSKUC2 ON SUBSKUC2.Id=SUBSKUV2.CharacteristicsId OR SUBSKUC1.Id=(SELECT TOP 1 RMSecondCharacteristicsId FROM AttachmentDetailConsumptionSKUMapping WHERE AttachmentDetailConsumptionId=adc.Id AND isnull(IsSecondCharacteristicCommon,0)=1)
                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBSKUC3 ON SUBSKUC3.Id=SUBSKUV3.CharacteristicsId OR SUBSKUC1.Id=(SELECT TOP 1 RMThirdCharacteristicsId FROM AttachmentDetailConsumptionSKUMapping WHERE AttachmentDetailConsumptionId=adc.Id AND isnull(IsThirdCharacteristicCommon,0)=1)



                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV1 ON SKUv1.Id=ADCS.RMFirstCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV2 ON SKUv2.Id=ADCS.RMSecondCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV3 ON SKUv3.Id=ADCS.RMThirdCharacteristicsValueId

                                        LEFT OUTER JOIN [HKP].[Characteristics] SKUC1 ON SKUC1.Id=SKUV1.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] SKUC2 ON SKUC2.Id=SKUV2.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] SKUC3 ON SKUC3.Id=SKUV3.CharacteristicsId

                                        WHERE att.MasterOrderItemId='" + MasterOrderItemId + @"'
                                        ORDER BY bd.Sequence";

            sheet.Name = "BOM Template Sub Material";

            DataTable dtData = _sqlRepository.GetDataTable(sql);

            if (dtData.Rows.Count == 0)
                return;

            int ROW = 1;
            sheet[ROW, 1].Text = "BOM Template Items (Sub Material)";
            sheet.Range[ROW, 1, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet[ROW, 1].CellStyle.Font.Bold = true;
            ROW++;
            sheet[ROW, 1].Text = "BOM Id";
            sheet[ROW, 3].Text = dtData.Rows[0]["BOMMasterId"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "BOM Desc";
            sheet[ROW, 3].Text = dtData.Rows[0]["BOMDesc"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "Material";
            sheet[ROW, 3].Text = dtData.Rows[0]["FGMaterial"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "Article";
            sheet[ROW, 3].Text = dtData.Rows[0]["FGArticle"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW += 2;
            int COL = 1;
            sheet[ROW, COL].Text = "Sl. No";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 6;
            int colSlNo = COL;
            COL++;
            sheet[ROW, COL].Text = "Sub Material";
            sheet[ROW, COL].ColumnWidth = 16;
            int colSubMaterial = COL;
            COL++;
            sheet[ROW, COL].Text = "Sub  Article";
            sheet[ROW, COL].ColumnWidth = 40;
            int colSubArticle = COL;
            COL++;
            sheet[ROW, COL].Text = "Parent Material";
            sheet[ROW, COL].ColumnWidth = 16;
            int colMaterial = COL;
            COL++;
            sheet[ROW, COL].Text = "Parent Article";
            sheet[ROW, COL].ColumnWidth = 40;
            int colArticle = COL;
            COL++;
            sheet[ROW, COL].Text = "RM Desc";
            sheet[ROW, COL].ColumnWidth = 22;
            int colRMDescription = COL;

            COL++;
            sheet[ROW, COL].Text = "Customer Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMCustomerSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMVendorSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU Desc";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUDesc = COL;
            COL++;
            sheet[ROW, COL].Text = "Cons.";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colConsumption = COL;
            COL++;
            sheet[ROW, COL].Text = "UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Wast.%";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colWastagePer = COL;
            COL++;
            sheet[ROW, COL].Text = "Process";
            sheet[ROW, COL].ColumnWidth = 8;
            int colProcess = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor";
            sheet[ROW, COL].ColumnWidth = 20;
            int colVendor = COL;
            COL++;
            sheet[ROW, COL].Text = "Common SKU";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMSKUCommon = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV2 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV3 = COL;

            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].Merge();
            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].Text = "COMMON SKU";
            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colRMV1, ROW, colRMV3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;

            COL++;
            sheet[ROW, COL].Text = "RM SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SUB RM SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV1 = COL;
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].Merge();
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].Text = "RM SKU-1 MAPPING";
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV1, ROW, colSKUV1].CellStyle.Interior.ColorIndex = ExcelKnownColors.LightGreen;



            COL++;
            sheet[ROW, COL].Text = "RM SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV2 = COL;

            COL++;
            sheet[ROW, COL].Text = "SUB RM SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV2 = COL;

            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].Merge();
            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].Text = "RM SKU-2 MAPPING";
            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV2, ROW, colSKUV2].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;



            COL++;
            sheet[ROW, COL].Text = "RM SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV3 = COL;
            COL++;
            sheet[ROW, COL].Text = "SUB RM SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV3 = COL;

            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].Merge();
            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].Text = "RM SKU-3 MAPPING";
            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV3, ROW, colSKUV3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_orange;

            sheet.Range[ROW - 1, 1, ROW, COL].CellStyle.Font.Bold = true; //row 19 of heading 
            sheet.Range[ROW - 1, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, colRMV1 - 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            int StartRow = ROW; //row 20
            for (int i = 0; i < dtData.Rows.Count; i++)
            {


                sheet[ROW, colSlNo].Number = (i + 1);
                sheet[ROW, colMaterial].Text = dtData.Rows[i]["RMMaterial"].ToString();
                sheet[ROW, colArticle].Text = dtData.Rows[i]["RMArticle"].ToString();
                sheet[ROW, colSubMaterial].Text = dtData.Rows[i]["SubRMMaterial"].ToString();
                sheet[ROW, colSubArticle].Text = dtData.Rows[i]["SubRMArticle"].ToString();
                sheet[ROW, colRMDescription].Text = dtData.Rows[i]["Description"].ToString();

                sheet[ROW, colRMCustomerSpec].Text = dtData.Rows[i]["CustomerSpec"].ToString();
                sheet[ROW, colRMVendorSpec].Text = dtData.Rows[i]["VendorSpec"].ToString();
                sheet[ROW, colSKUDesc].Text = dtData.Rows[i]["SKUDesc"].ToString();
                sheet[ROW, colVendor].Text = dtData.Rows[i]["Vendor"].ToString();
                sheet[ROW, colUOM].Text = dtData.Rows[i]["UOM"].ToString();
                sheet[ROW, colProcess].Text = dtData.Rows[i]["Process"].ToString();

                sheet[ROW, colRMSKUCommon].Text = dtData.Rows[i]["RMSKUCommon"].ToString();

                sheet[ROW, colRMV1].Text = dtData.Rows[i]["SUBRMV1CHARVAL"].ToString();
                sheet[ROW, colRMV2].Text = dtData.Rows[i]["SUBRMV2CHARVAL"].ToString();
                sheet[ROW, colRMV3].Text = dtData.Rows[i]["SUBRMV3CHARVAL"].ToString();



                sheet[ROW, colFGV1].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsFirstCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["SUBSKU1CHAR"].ToString() + "]" : dtData.Rows[i]["SKUV1CHARVAL"].ToString();
                sheet[ROW, colFGV2].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsSecondCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["SUBSKU2CHAR"].ToString() + "]" : dtData.Rows[i]["SKUV2CHARVAL"].ToString();
                sheet[ROW, colFGV3].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsThirdCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["SUBSKU3CHAR"].ToString() + "]" : dtData.Rows[i]["SKUV3CHARVAL"].ToString();


                sheet[ROW, colSKUV1].Text = dtData.Rows[i]["SUBSKUV1CHARVAL"].ToString();
                sheet[ROW, colSKUV2].Text = dtData.Rows[i]["SUBSKUV2CHARVAL"].ToString();
                sheet[ROW, colSKUV3].Text = dtData.Rows[i]["SUBSKUV3CHARVAL"].ToString();

                sheet[ROW, colConsumption].Number = clsStaticInfo.dbl(dtData.Rows[i]["Consumption"].ToString());
                sheet[ROW, colWastagePer].Number = clsStaticInfo.dbl(dtData.Rows[i]["WastagePer"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            sheet.IsGridLinesVisible = false;

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

            sheet.Range[8, 4].FreezePanes();

            sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
            sheet.Range[StartRow, colConsumption, ROW, colConsumption].NumberFormat = clsStaticInfo.NumberFormat(4);
            sheet.Range[StartRow, colWastagePer, ROW, colWastagePer].NumberFormat = clsStaticInfo.NumberFormat(2);

        }

        #region BOMReportItemandSalesOrder
        public IWorkbook GetMasterOrderReports(string MasterOrderId, BOMLevel Level, bool isMatrix = true)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;
            IWorkbook workbook = application.Workbooks.Create(1);
            //Load the existing Excel workbook into IWorkbook
            if (Level == BOMLevel.Item)
                workbook = application.Workbooks.Create(3);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            try
            {
                DataTable dtOrderMaster = _sqlRepository.GetDataTable(@"select mo.Id, mo.type, b.UserName as Buyer, p.UserName as Customer
                ,  mo.OrderYear as Year, mo.TotalQty as TotalQuantity
                    , uom.UserName as UnitOfMeasurement, mo.NoOfLineItem, mo.OrderWastagePercentage
                    , mo.ExtraOrderPercentage, mo.BuyerReferenceNo, mo.OwnReferenceNo, BDept.UserName as BuyerDepartment
                    , BDev.UserName as BuyerDevision, MoCur.Code MasterOrderCurrency from trn.MasterOrder MO 
                    left join scs.Currency MoCur on MoCur.id = mo.CurrencyId 
                    left join scs.UnitOfMeasurement UOM on uom.id = mo.TotalQtyUOMId 
                    left join hkp.buyer B on b.id = mo.buyerid 
                    left join hkp.party p on p.id = mo.partyid 
                    left join hkp.BuyerDepartment BDept on BDept.id = mo.buyerDepartmentid 
                    left join HKP.buyerdivision BDev on BDev.id = mo.BuyerDivisionId where mo.Id='" + MasterOrderId + "'");
                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                DataTable dtMasterOrderItem = _sqlRepository.GetDataTable(@"select moi.id as MasterOrderItemNo,moi.BuyerReferenceNo
                 ,moi.OwnReferenceNo,moi.TotalQty as TotalMOIQuantity, moi.MasterOrderId,c.ContractNo,ml.LCRef
                 ,moi.OrderWastagePercentage, moi.ExtraOrderPercentage ,mm.UserName as Material ,mma.StandardName as Article, moi.Type
                 from trn.MasterOrderItem MOI
                 left join TRN.MasterOrder mo  on mo.id=moi.MasterOrderId
                 left join MST.MaterialMaster MM on mm.id = moi.MaterialMasterId
                 left join MST.MaterialMasterArticle mma on mma.id= moi.ArticleId
                 left join scs.TestingStandard ts on ts.id=moi.TestingStandardId
LEFT JOIN TRN.SalesOrder so on moi.Id=so.MasterOrderItemId
                 LEFT JOIN [Contract] AS c ON c.Id=so.ContractId
                 LEFT JOIN MasterLC AS ml ON ml.Id=c.MasterLCId
                 where mo.Id='" + MasterOrderId + "'");


                DataTable dtSalesOrderItem = _sqlRepository.GetDataTable(@"select so.MasterOrderItemId, so.id as SalesOrderNo,cpo.PONumber,os.UserName as OrderStatus,d.UserName as Destination
                ,so.Qty as Quantity, so.UpCharge, so.MainRawMaterialInhouseDate, so.Description
                ,so.SOType, oc.username as OrderCategory
                ,so.DeliveryDate, sm.UserName as ShipmentMode
                ,so.Rate, so.Discount,so.CM,so.LSD, so.OtherRawMaterialInhouseDate , so.Reason , so.CommitmentDate

                ,c1.username as FirstCharacteristics, isnull(fcs.ValueFreeText,CV1.UserName) as FirstCharacteristicsValue
                ,c2.username as SecondCharacteristics ,isnull(SCS.ValueFreeText, CV2.UserName) as SecondCharacteristicsValue
                ,c3.username as ThirdCharacteristics , isnull(ThirdCS.ValueFreeText,CV3.UserName) as ThirdCharacteristicsValue
                ,case when isnull(thirdCs.Id,'')<>'' THEN ThirdCs.Qty
                ELSE case when isnull(scs.id,'')<>'' THEN scs.Qty
                ELSE case when isnull(fcs.Id,'')<>'' THEN fcs.Qty
                ELSE 0 END END END AS Qty
                from trn.SalesOrder SO
                left join trn.masterorderitem moi on moi.id= so.masterorderitemid
                left join HKP.OrderCategory OC on oc.id = so.OrderCategoryId
                left join hkp.OrderStatus OS on os.id = so.OrderStatusId
                left join mst.shipMode SM on sm.id = so.shipmentModeId
                left join mst.Destination d on d.id =so.DestinationId
                left join trn.CustomerPO CPO on cpo.id =so.CustomerPOId

                left join TRN.FirstCharacteristics FCS on fcs.SalesOrderId = so.id
                left join hkp.Characteristics C1 on c1.id = fcs.CharacteristicsId
                left join HKP.CharacteristicsValue CV1 on cv1.id= fcs.CharacteristicsValueId

                left join TRN.SecondCharacteristics SCS on scs.SalesOrderId=so.id and scs.FirstCharacteristicsId=fcs.Id
                left join hkp.Characteristics C2 on c2.id = scs.CharacteristicsId
                left join HKP.CharacteristicsValue CV2 on cv2.id= scs.CharacteristicsValueId

                left join TRN.ThirdCharacteristics ThirdCS on ThirdCS.SalesOrderId=so.id and scs.id=ThirdCS.SecondCharacteristicsId
                left join hkp.Characteristics C3 on c3.id = ThirdCS.CharacteristicsId
                left join HKP.CharacteristicsValue CV3 on CV3.id= ThirdCS.CharacteristicsValueId

                     where moi.MasterOrderId='" + MasterOrderId + "'");





                DataTable dtBOMData = new DataTable();
                if (Level == BOMLevel.SO)
                {
                    //worksheet.Name = "DetailBOM-SO Level";
                    worksheet.Name = "Detail";
                    string strsql = @"SELECT b.Id, b.MasterOrderItemId,moi.MasterOrderId,moi.OwnReferenceNo,moi.BuyerReferenceNo, b.VendorId,b.SalesOrderId,
                                 mm.UserName AS Material,mma.StandardName AS Article,p.UserName AS Vendor,b.SKUDesc,


                                v1.UserName AS CharVal1,v2.UserName AS CharVal2,v3.UserName AS CharVal3,convert(bit,isnull(b.isParent,0)) AS isParent,
                                convert(bit,isnull(b.isChild,0)) AS isChild,PR.UserName AS Process,
                               CONVERT(BIT, isnull(b.RequiredQtyApproved,0)) AS RequiredQtyApproved,CONVERT(BIT, isnull(b.IncompleteMaterial,0)) AS IncompleteMaterial,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
                                b.BOMQty,b.RequiredQty,b.RequiredQtyPO,uom.UserName AS UOM,uomm.UserName AS ParentUOM,
                                POUOM.UserName AS POUOM,
                                b.RMDescription,	b.RMCustomerSpec,	b.RMVendorSpec
								,ISNULL(po.POQTY,0) POQTY,ISNULL(grn.GRNQty,0) GRNQty,
								
								 SO1 =STUFF((select distinct ','+xv1.UserName
								               from BOQFGMapping AS XM	 
										       JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
								               JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.FirstCharacteristicsValueId
								             WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO2 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.SecondCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO3 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.ThirdCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								POIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.PurchaseOrderDetail AS XB2 ON xb2.Id=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								GRNIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.InventoryReceiveDetail AS XB2 ON xb2.PODetailsId=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                  FROM BOQ AS b
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS POuom ON POuom.Id=b.POUoMId
                                LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                                LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
                                LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                                LEFT OUTER JOIN trn.masterorder MO ON MO.Id=moi.MasterOrderId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uomm ON uomm.Id=mo.TotalQtyUOMId
                                LEFT JOIN hkp.Process AS pr ON pr.Id=b.ProcessId

                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId
								Left outer join (	select BOQDetailId,sum(POBOQQty) as POQTY from trn.POBOQMAP  GRoup by BOQDetailId) PO On PO.BOQDetailId = b.Id
								Left outer join (	
								SELECT a.BOQDetailId , sum(b.TransactionQty) GRNQty 
				FROM trn.POBOQMAP a
				INNER JOIN trn.InventoryReceiveDetail b ON a.PODetailId=b.PODetailsId GRoup by BOQDetailId
								
								) GRN On GRN.BOQDetailId = b.Id

                                WHERE MO.Id='" + MasterOrderId + @"' and isnull(B.Id,'') NOT IN (select ParentId from BOQ where isnull(ParentId,'')<>'' AND MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where MasterOrderId='" + MasterOrderId + @"'))
                                ORDER BY isnull(b.Sequence,0),b.SalesOrderId";
                    dtBOMData = _sqlRepository.GetDataTable(strsql);
                }
                else if (Level == BOMLevel.Item)
                {
                    //worksheet.Name = "BOM-Item Level";
                    worksheet.Name = "Summary";
                    string strsql = @"select B.Sequence, B.MasterOrderItemId, B.MasterOrderId, B.OwnReferenceNo,
       B.BuyerReferenceNo, B.VendorId, B.Material, B.Article, B.Vendor, B.SKUDesc,B.POIds, B.GRNIds,
       B.CharVal1, B.CharVal2, B.CharVal3, B.isParent, B.isChild, B.Process,
       B.Consumption, B.WastagePer, B.UOM, B.ParentUOM, B.POUOM, B.RMDescription,
       B.RMCustomerSpec, B.RMVendorSpec, B.SO1, B.SO2, B.SO3,
       sum(b.BOMQty) AS BOMQty,sum(b.RequiredQty) AS RequiredQty, SUM(b.RequiredQtyPO) AS RequiredQtyPO, sum(b.OrderQty) AS OrderQty,sum(b.PlanOrderQty) AS PlanOrderQty
       ,SUM(ISNULL(b.POQTY,0)) POQTY,SUM(ISNULL(b.GRNQty,0)) GRNQty
  from (SELECT  b.Sequence,b.MasterOrderItemId,moi.MasterOrderId,moi.OwnReferenceNo,moi.BuyerReferenceNo, b.VendorId,
                                 mm.UserName AS Material,mma.StandardName AS Article,p.UserName AS Vendor,b.SKUDesc,
                                v1.UserName AS CharVal1,v2.UserName AS CharVal2,v3.UserName AS CharVal3,convert(bit,isnull(b.isParent,0)) AS isParent,
                                convert(bit,isnull(b.isChild,0)) AS isChild,PR.UserName AS Process,
                               b.Consumption,b.WastagePer,
                                uom.UserName AS UOM,uomm.UserName AS ParentUOM, POUOM.UserName AS POUOM,
                                b.RMDescription,	b.RMCustomerSpec,	b.RMVendorSpec,
								b.BOMQty,b.RequiredQty,b.RequiredQtyPO, b.OrderQty,PlanOrderQty,
                                ISNULL(po.POQTY,0) POQTY,ISNULL(grn.GRNQty,0) GRNQty,
								 SO1 =STUFF((select distinct ','+xv1.UserName
								          from BOQFGMapping AS XM	
										  JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
								          JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.FirstCharacteristicsValueId
								          WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO2 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.SecondCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO3 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.ThirdCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								POIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.PurchaseOrderDetail AS XB2 ON xb2.Id=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								GRNIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.InventoryReceiveDetail AS XB2 ON xb2.PODetailsId=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                  FROM BOQ AS b
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS POuom ON POuom.Id=b.POUoMId

                                LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                                LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
                                LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                                LEFT OUTER JOIN trn.masterorder MO ON MO.Id=moi.MasterOrderId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uomm ON uomm.Id=mo.TotalQtyUOMId
                                LEFT JOIN hkp.Process AS pr ON pr.Id=b.ProcessId

                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId
                                LEFT OUTER JOIN (	select BOQDetailId,sum(POBOQQty) as POQTY from trn.POBOQMAP  GRoup by BOQDetailId) PO On PO.BOQDetailId = b.Id
								LEFT OUTER JOIN (	
								SELECT a.BOQDetailId , sum(b.TransactionQty) GRNQty 
				                    FROM trn.POBOQMAP a
				                    INNER JOIN trn.InventoryReceiveDetail b ON a.PODetailId=b.PODetailsId GRoup by BOQDetailId
								
								) GRN On GRN.BOQDetailId = b.Id

                                WHERE MO.Id='" + MasterOrderId + @"' and isnull(B.Id,'') NOT IN (select ParentId from BOQ where isnull(ParentId,'')<>'' AND MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where MasterOrderId='" + MasterOrderId + @"'))
                        ) AS B
                                GROUP BY B.Sequence,B.POIds, B.GRNIds, B.MasterOrderItemId, B.MasterOrderId, B.OwnReferenceNo,
       B.BuyerReferenceNo, B.VendorId, B.Material, B.Article, B.Vendor, B.SKUDesc,
       B.CharVal1, B.CharVal2, B.CharVal3, B.isParent, B.isChild, B.Process,
       B.Consumption, B.WastagePer, B.UOM, B.ParentUOM, B.POUOM, B.RMDescription,
       B.RMCustomerSpec, B.RMVendorSpec, B.SO1, B.SO2, B.SO3
                                
                                ORDER BY isnull(b.Sequence,0)";
                    dtBOMData = _sqlRepository.GetDataTable(strsql);


                }
                int ROW = 6; int COL = 1;

                //foreach (ExcelKnownColors val in Enum.GetValues(typeof(ExcelKnownColors)))
                //{
                //    worksheet[ROW, 1].CellStyle.Interior.ColorIndex = val;
                //    worksheet[ROW, 1].Text = val.ToString();
                //    ROW++;
                //}

                int MasterOrderDetailsStartRow = ROW;
                worksheet[ROW, COL].Text = "Master Order Details:";
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                ROW++;

                int leftColumnCaption = COL;
                int leftColumnValue = leftColumnCaption + 1;

                int MiddleColumnCaption = leftColumnValue + 2;
                int MiddleColumnValue = MiddleColumnCaption + 1;

                int RightColumnCaption = MiddleColumnValue + 2;
                int RightColumnValue = RightColumnCaption + 1;

                //Master Order.............................................................
                //worksheet[ROW, leftColumnCaption].Text = "Master Order No";
                //worksheet[ROW, leftColumnValue].Text = "MasterOrderNo";

                worksheet[ROW, leftColumnCaption].Text = "Order#";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Id"].ToString();
                // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                worksheet.Range[ROW, leftColumnCaption, ROW, leftColumnValue].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Buyer.Ref";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["BuyerReferenceNo"].ToString();
                worksheet.Range[ROW, MiddleColumnCaption, ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, RightColumnCaption].Text = "Own.Ref";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["OwnReferenceNo"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                ROW++;

                worksheet[ROW, leftColumnCaption].Text = "Buyer";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Buyer"].ToString();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Buyer Dep.";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["BuyerDepartment"].ToString();
                worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, RightColumnCaption].Text = "Buyer Div";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["BuyerDevision"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                ROW++;

                worksheet[ROW, leftColumnCaption].Text = "Customer";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Customer"].ToString();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Currency";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["MasterOrderCurrency"].ToString();
                worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;
                ROW++;


                worksheet[ROW, leftColumnCaption].Text = "Total Order Quantity";
                worksheet[ROW, leftColumnValue].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[0]["TotalQuantity"].ToString());
                worksheet[ROW, leftColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                // worksheet[ROW, leftColumnValue, ROW , leftColumnValue].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, leftColumnValue, ROW, leftColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                //worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Bold = true;
                worksheet[ROW, leftColumnValue].HorizontalAlignment = ExcelHAlign.HAlignRight;

                worksheet[ROW, leftColumnValue + 1].Text = dtOrderMaster.Rows[0]["UnitOfMeasurement"].ToString();
                //worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet.Range[MasterOrderDetailsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom44;



                ROW += 2;


                //Master Order Item....................................................................................................
                StringCollection strColSO = new StringCollection();

                for (int i = 0; i < dtMasterOrderItem.Rows.Count; i++)
                {
                    int MasterItemsStartRow = ROW; // row 12
                    worksheet[ROW, COL].Text = "Item Id:"; //col 1
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["MasterOrderItemNo"].ToString();
                    worksheet[ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                    worksheet.Range[ROW, COL, ROW, leftColumnValue].CellStyle.Font.Bold = true;
                    ROW++;


                    // int MasterItemsStartRow = ROW;
                    strColSO = new StringCollection();
                    // worksheet[ROW, leftColumnCaption].Text = "Items Details";



                    worksheet[ROW, leftColumnCaption].Text = "Material";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["Material"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Article";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["Article"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Contract#";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["ContractNo"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, MiddleColumnCaption].Text = "LC Ref";
                    worksheet[ROW, MiddleColumnValue].Text = dtMasterOrderItem.Rows[i]["LCRef"].ToString();
                    worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Buyer Ref";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["BuyerReferenceNo"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, MiddleColumnCaption].Text = "Own Ref";
                    worksheet[ROW, MiddleColumnValue].Text = dtMasterOrderItem.Rows[i]["OwnReferenceNo"].ToString();
                    worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, RightColumnCaption].Text = "Qty";
                    worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                    worksheet[ROW, RightColumnValue].Number = clsStaticInfo.dbl(dtMasterOrderItem.Rows[i]["TotalMOIQuantity"].ToString());
                    //worksheet.Range[ROW, RightColumnValue, ROW, RightColumnValue].CellStyle.Font.Bold = true;
                    // worksheet[ROW, RightColumnValue].CellStyle.Font.Bold = true;
                    worksheet[ROW, RightColumnValue, ROW, RightColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                    worksheet.Range[MasterItemsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom18;
                    ROW++;

                    if (Level == BOMLevel.SO)
                    {
                        dtSalesOrderItem.DefaultView.RowFilter = "MasterOrderItemId='" + dtMasterOrderItem.Rows[i]["MasterOrderItemNo"].ToString() + "'";
                        DataTable dtSalesOrderFilteredByItem = dtSalesOrderItem.DefaultView.ToTable();
                        for (int KK = 0; KK < dtSalesOrderItem.DefaultView.Count; KK++)
                        {


                            if (strColSO.Contains(dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString()))
                                continue;
                            int SOStartRow = ROW;  //row 16
                            worksheet[ROW, COL].Text = "Sales Order Details & Breakdown:";
                            worksheet[ROW, COL].CellStyle.Font.Bold = true;
                            ROW++;

                            // int SOStartRow = ROW;

                            strColSO.Add(dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString());

                            worksheet[ROW, leftColumnCaption].Text = "SO No";
                            worksheet[ROW, leftColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString();
                            worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                            worksheet[ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                            worksheet[ROW, leftColumnValue].CellStyle.Font.Bold = true;


                            worksheet[ROW, MiddleColumnCaption].Text = "Del. Date";
                            worksheet[ROW, MiddleColumnValue].Text = Convert.ToDateTime(dtSalesOrderItem.DefaultView[KK]["DeliveryDate"].ToString()).ToString("dd-MMM-yyyy");
                            worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                            worksheet[ROW, RightColumnCaption].Text = "Qty";
                            worksheet[ROW, RightColumnValue].Number = clsStaticInfo.dbl(dtSalesOrderItem.DefaultView[KK]["Quantity"].ToString());
                            worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                            worksheet[ROW, RightColumnValue, ROW, RightColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                            // worksheet[ROW, RightColumnValue].CellStyle.Font.Bold = true;
                            ROW++;

                            worksheet[ROW, leftColumnCaption].Text = "Dest.";
                            worksheet[ROW, leftColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["Destination"].ToString();
                            worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;


                            worksheet[ROW, MiddleColumnCaption].Text = "Ship Mode";
                            worksheet[ROW, MiddleColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["ShipmentMode"].ToString();
                            worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                            worksheet[ROW, RightColumnCaption].Text = "Ord. Status";
                            worksheet[ROW, RightColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["OrderStatus"].ToString();
                            worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                            worksheet.Range[SOStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom19;

                            ROW++;

                            dtSalesOrderFilteredByItem.DefaultView.RowFilter = "SalesOrderNo='" + dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString() + "'"; //????
                            DataTable dtBreakdownData = dtSalesOrderFilteredByItem.DefaultView.ToTable();
                            DrawSOBreakdownData(dtBreakdownData, worksheet, ref ROW, isMatrix);

                            ROW++;
                            //BOM Data here
                            dtBOMData.DefaultView.RowFilter = "SalesOrderId='" + dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString() + "'";
                            DrawBOMData(dtBOMData.DefaultView.ToTable(), worksheet, ref ROW);

                            ROW++;
                        }
                    }
                    else
                    {
                        DrawBOMData(dtBOMData, worksheet, ref ROW);
                    }

                    ROW += 2; // Gap for Material
                }

                int endCol = RightColumnValue;


                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.CellStyle.Font.Size = 8f;
                worksheet.UsedRange.WrapText = true;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref worksheet, endCol, "Master Order#" + dtOrderMaster.Rows[0]["Id"].ToString(), identity.PlantId);
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;
                return workbook;

            }
            catch (Exception ex)
            {
                throw (ex);

            }
        }

        public void GetDrawBOMTemplateDataReports(IWorksheet sheet, string MasterOrderId)
        {
            string sql = @"SELECT att.BOMMasterId, b.[Description] AS BOMDesc,mmf.UserName FGMaterial,mmaf.StandardName AS FGArticle, mm.UserName RMMaterial,mma.StandardName AS RMArticle,p.UserName AS Vendor,
                                    pr.UserName AS Process,uom.UserName AS UOM,bs.[Description] AS SKUDesc,
                                    bd.[Description], bd.CustomerSpec, bd.VendorSpec, bd.Consumption, bd.WastagePer,
                                    bd.IsSKUCommon AS RMSKUCommon,

                                    --COMMON RM MAPPING---
                                    RMC1.UserName AS RM1CHAR,RMC2.UserName AS RM2CHAR,RMC3.UserName AS RM3CHAR,
                                    RMV1.UserName AS RMV1CHARVAL,RMV2.UserName AS RMV2CHARVAL,RMV3.UserName AS RMV3CHARVAL,
                                    --FG MAPPING
                                    FGC1.UserName AS FG1CHAR,FGC2.UserName AS FG2CHAR,FGC3.UserName AS FG3CHAR,
                                    FGV1.UserName AS FGV1CHARVAL,FGV2.UserName AS FGV2CHARVAL,FGV3.UserName AS FGV3CHARVAL,

                                    BS.IsFirstCharacteristicCommon, BS.IsSecondCharacteristicCommon,BS.IsThirdCharacteristicCommon,

                                    SKUC1.UserName AS SKU1CHAR,SKUC2.UserName AS SKU2CHAR,SKUC3.UserName AS SKU3CHAR,
                                    SKUV1.UserName AS SKUV1CHARVAL,SKUV2.UserName AS SKUV2CHARVAL,SKUV3.UserName AS SKUV3CHARVAL
                                    --------------------

                                     FROM BOMMasterAttachmentWithItem AS ATT
                                    INNER JOIN BOMMaster AS b ON b.Id=att.BOMMasterId
                                    INNER JOIN BOMAttachmentDetail AS bd ON bd.BOMMasterAttachmentWithItemId=att.Id
                                    LEFT JOIN BOMAttachmentSKUMapping AS bs ON bs.BOMAttachmentDetailId=bd.Id 
					                                    AND  isnull(bs.FGFirstCharacteristicsValueId,'') IN (
						                                    SELECT '' UNION
					                                    SELECT fc.CharacteristicsValueId FROM trn.FirstCharacteristics AS fc
					                                    INNER JOIN trn.SalesOrder AS so ON so.Id=fc.SalesOrderId
					                                    WHERE so.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where MasterOrderId='" + MasterOrderId + @"')
					                                    )

						                                    AND  isnull(bs.FGSecondCharacteristicsValueId,'') IN (
							                                    SELECT '' UNION
						                                    SELECT sc.CharacteristicsValueId FROM trn.SecondCharacteristics AS sc 
						                                    INNER JOIN trn.SalesOrder AS so ON so.Id=sc.SalesOrderId
						                                    WHERE so.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where MasterOrderId='" + MasterOrderId + @"')
						                                    )


					                                    AND  isnull(bs.FGFirstCharacteristicsValueId,'') IN (
						                                    SELECT '' UNION
					                                    SELECT fc.CharacteristicsValueId FROM trn.ThirdCharacteristics AS  fc
					                                    INNER JOIN trn.SalesOrder AS so ON so.Id=fc.SalesOrderId
					                                    WHERE so.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where MasterOrderId='" + MasterOrderId + @"')
						                                    )

                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=att.MasterOrderItemId
                                    LEFT OUTER JOIN mst.MaterialMaster AS mmf ON mmf.Id=moi.MaterialMasterId
                                    LEFT OUTER JOIN mst.MaterialMasterArticle AS mmaf ON mmaf.Id=moi.ArticleId


                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV1 ON RMv1.Id=bd.FirstCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV2 ON RMv2.Id=bd.SecondCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV3 ON RMv3.Id=bd.ThirdCharacteristicsValueId

                                    LEFT OUTER JOIN [HKP].[Characteristics] RMC1 ON RMC1.Id=RMV1.CharacteristicsId
                                    LEFT OUTER JOIN [HKP].[Characteristics] RMC2 ON RMC2.Id=RMV2.CharacteristicsId
                                    LEFT OUTER JOIN [HKP].[Characteristics] RMC3 ON RMC3.Id=RMV3.CharacteristicsId


                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] FGV1 ON FGv1.Id=bs.FGFirstCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] FGV2 ON FGv2.Id=bs.FGSecondCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] FGV3 ON FGv3.Id=bs.FGThirdCharacteristicsValueId

                                    LEFT JOIN BOMAttachmentSKUMapping AS bscf ON bs.BOMAttachmentDetailId=bd.Id AND bscf.id =(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping WHERE BOMAttachmentDetailId=bd.Id AND isnull(IsFirstCharacteristicCommon,0)=1)
                                    LEFT JOIN BOMAttachmentSKUMapping AS bscs ON bs.BOMAttachmentDetailId=bd.Id AND bscs.id =(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping WHERE BOMAttachmentDetailId=bd.Id AND isnull(IsSecondCharacteristicCommon,0)=1)
                                    LEFT JOIN BOMAttachmentSKUMapping AS bsct ON bs.BOMAttachmentDetailId=bd.Id AND bsct.id =(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping WHERE BOMAttachmentDetailId=bd.Id AND isnull(IsthirdCharacteristicCommon,0)=1)
                                    LEFT OUTER JOIN [HKP].[Characteristics] FGC1 ON FGC1.Id=isnull(bscf.FGFirstCharacteristicsId,FGV1.CharacteristicsId)
                                    LEFT OUTER JOIN [HKP].[Characteristics] FGC2 ON FGC2.Id=isnull(bscs.FGSecondCharacteristicsId,FGV2.CharacteristicsId)
                                    LEFT OUTER JOIN [HKP].[Characteristics] FGC3 ON FGC3.Id=isnull(bsct.FGThirdCharacteristicsId,FGV3.CharacteristicsId)

                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV1 ON SKUv1.Id=bs.RMFirstCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV2 ON SKUv2.Id=bs.RMSecondCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV3 ON SKUv3.Id=bs.RMThirdCharacteristicsValueId

                                    LEFT OUTER JOIN [HKP].[Characteristics] SKUC1 ON SKUC1.Id=SKUV1.CharacteristicsId
                                    LEFT OUTER JOIN [HKP].[Characteristics] SKUC2 ON SKUC2.Id=SKUV2.CharacteristicsId
                                    LEFT OUTER JOIN [HKP].[Characteristics] SKUC3 ON SKUC3.Id=SKUV3.CharacteristicsId

                                    LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=bd.RMMaterialMasterId
                                    LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=bd.RMArticleId
                                    LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=BD.UoMId
                                    LEFT OUTER JOIN HKP.Party P ON p.Id=bd.VendorId
                                    LEFT JOIN hkp.Process AS pr ON pr.Id=bd.ProcessId

                                    WHERE att.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where MasterOrderId='" + MasterOrderId + @"')
                                    ORDER BY bd.Sequence";

            sheet.Name = "BOM Template";

            DataTable dtData = _sqlRepository.GetDataTable(sql);
            int ROW = 1;
            sheet[ROW, 1].Text = "BOM Template Items";
            sheet.Range[ROW, 1, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet[ROW, 1].CellStyle.Font.Bold = true;
            ROW++;
            sheet[ROW, 1].Text = "BOM Id";
            sheet[ROW, 3].Text = dtData.Rows[0]["BOMMasterId"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "BOM Desc";
            sheet[ROW, 3].Text = dtData.Rows[0]["BOMDesc"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "Material";
            sheet[ROW, 3].Text = dtData.Rows[0]["FGMaterial"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "Article";
            sheet[ROW, 3].Text = dtData.Rows[0]["FGArticle"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW += 2;
            int COL = 1;
            sheet[ROW, COL].Text = "Sl. No";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 6;
            int colSlNo = COL;
            COL++;
            sheet[ROW, COL].Text = "Material";
            sheet[ROW, COL].ColumnWidth = 16;
            int colMaterial = COL;
            COL++;
            sheet[ROW, COL].Text = "Article";
            sheet[ROW, COL].ColumnWidth = 40;
            int colArticle = COL;
            COL++;
            sheet[ROW, COL].Text = "RM Desc";
            sheet[ROW, COL].ColumnWidth = 22;
            int colRMDescription = COL;

            COL++;
            sheet[ROW, COL].Text = "Customer Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMCustomerSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMVendorSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU Desc";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUDesc = COL;
            COL++;
            sheet[ROW, COL].Text = "Cons.";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colConsumption = COL;
            COL++;
            sheet[ROW, COL].Text = "UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Wast.%";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colWastagePer = COL;
            COL++;
            sheet[ROW, COL].Text = "Process";
            sheet[ROW, COL].ColumnWidth = 8;
            int colProcess = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor";
            sheet[ROW, COL].ColumnWidth = 20;
            int colVendor = COL;
            COL++;
            sheet[ROW, COL].Text = "Common SKU";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMSKUCommon = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV2 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV3 = COL;

            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].Merge();
            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].Text = "COMMON SKU";
            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colRMV1, ROW, colRMV3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;

            COL++;
            sheet[ROW, COL].Text = "FG SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV1 = COL;
            COL++;
            sheet[ROW, COL].Text = "RM SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV1 = COL;
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].Merge();
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].Text = "FG SKU-1 MAPPING";
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV1, ROW, colSKUV1].CellStyle.Interior.ColorIndex = ExcelKnownColors.LightGreen;



            COL++;
            sheet[ROW, COL].Text = "FG SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV2 = COL;

            COL++;
            sheet[ROW, COL].Text = "RM SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV2 = COL;

            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].Merge();
            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].Text = "FG SKU-2 MAPPING";
            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV2, ROW, colSKUV2].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;



            COL++;
            sheet[ROW, COL].Text = "FG SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV3 = COL;
            COL++;
            sheet[ROW, COL].Text = "RM SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV3 = COL;

            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].Merge();
            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].Text = "FG SKU-3 MAPPING";
            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV3, ROW, colSKUV3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_orange;

            sheet.Range[ROW - 1, 1, ROW, COL].CellStyle.Font.Bold = true; //row 19 of heading 
            sheet.Range[ROW - 1, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, colRMV1 - 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            int StartRow = ROW; //row 20
            for (int i = 0; i < dtData.Rows.Count; i++)
            {


                sheet[ROW, colSlNo].Number = (i + 1);
                sheet[ROW, colMaterial].Text = dtData.Rows[i]["RMMaterial"].ToString();
                sheet[ROW, colArticle].Text = dtData.Rows[i]["RMArticle"].ToString();
                sheet[ROW, colRMDescription].Text = dtData.Rows[i]["Description"].ToString();

                sheet[ROW, colRMCustomerSpec].Text = dtData.Rows[i]["CustomerSpec"].ToString();
                sheet[ROW, colRMVendorSpec].Text = dtData.Rows[i]["VendorSpec"].ToString();
                sheet[ROW, colSKUDesc].Text = dtData.Rows[i]["SKUDesc"].ToString();
                sheet[ROW, colVendor].Text = dtData.Rows[i]["Vendor"].ToString();
                sheet[ROW, colUOM].Text = dtData.Rows[i]["UOM"].ToString();
                sheet[ROW, colProcess].Text = dtData.Rows[i]["Process"].ToString();

                sheet[ROW, colRMSKUCommon].Text = dtData.Rows[i]["RMSKUCommon"].ToString();

                sheet[ROW, colRMV1].Text = dtData.Rows[i]["RMV1CHARVAL"].ToString();
                sheet[ROW, colRMV2].Text = dtData.Rows[i]["RMV2CHARVAL"].ToString();
                sheet[ROW, colRMV3].Text = dtData.Rows[i]["RMV3CHARVAL"].ToString();



                sheet[ROW, colFGV1].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsFirstCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["FG1CHAR"].ToString() + "]" : dtData.Rows[i]["FGV1CHARVAL"].ToString();
                sheet[ROW, colFGV2].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsSecondCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["FG2CHAR"].ToString() + "]" : dtData.Rows[i]["FGV2CHARVAL"].ToString();
                sheet[ROW, colFGV3].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsThirdCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["FG3CHAR"].ToString() + "]" : dtData.Rows[i]["FGV3CHARVAL"].ToString();


                sheet[ROW, colSKUV1].Text = dtData.Rows[i]["SKUV1CHARVAL"].ToString();
                sheet[ROW, colSKUV2].Text = dtData.Rows[i]["SKUV2CHARVAL"].ToString();
                sheet[ROW, colSKUV3].Text = dtData.Rows[i]["SKUV3CHARVAL"].ToString();

                sheet[ROW, colConsumption].Number = clsStaticInfo.dbl(dtData.Rows[i]["Consumption"].ToString());
                sheet[ROW, colWastagePer].Number = clsStaticInfo.dbl(dtData.Rows[i]["WastagePer"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            sheet.IsGridLinesVisible = false;

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

            sheet.Range[8, 4].FreezePanes();

            sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
            sheet.Range[StartRow, colConsumption, ROW, colConsumption].NumberFormat = clsStaticInfo.NumberFormat(4);
            sheet.Range[StartRow, colWastagePer, ROW, colWastagePer].NumberFormat = clsStaticInfo.NumberFormat(2);

        }

        public void GetDrawBOMTemplateDataSubMaterials(IWorksheet sheet, string MasterOrderId)
        {
            string sql = @"SELECT att.BOMMasterId, b.[Description] AS BOMDesc, mmf.UserName FGMaterial,mmaf.StandardName AS FGArticle, mm.UserName RMMaterial,mma.StandardName AS RMArticle,

                                        submm.UserName SubRMMaterial,submma.StandardName AS SubRMArticle,subp.UserName AS Vendor,
                                        subpr.UserName AS Process,subuom.UserName AS UOM,ADCS.[Description] AS SKUDesc,
                                        adc.[Description], adc.CustomerSpec, adc.VendorSpec, adc.Consumption, adc.WastagePer,
                                        adc.IsSKUCommon AS RMSKUCommon,



                                        --COMMON RM MAPPING---
                                        RMC1.UserName AS RM1CHAR,RMC2.UserName AS RM2CHAR,RMC3.UserName AS RM3CHAR,
                                        RMV1.UserName AS RMV1CHARVAL,RMV2.UserName AS RMV2CHARVAL,RMV3.UserName AS RMV3CHARVAL,

                                        --ADCS.IsFirstCharacteristicCommon, ADCS.IsSecondCharacteristicCommon,ADCS.IsThirdCharacteristicCommon,

                                        SKUC1.UserName AS SKU1CHAR,SKUC2.UserName AS SKU2CHAR,SKUC3.UserName AS SKU3CHAR,
                                        SKUV1.UserName AS SKUV1CHARVAL,SKUV2.UserName AS SKUV2CHARVAL,SKUV3.UserName AS SKUV3CHARVAL,


                                        --COMMON RM MAPPING FOR SUB MATERIAL---
                                        SUBRMC1.UserName AS SUBRM1CHAR,SUBRMC2.UserName AS SUBRM2CHAR,SUBRMC3.UserName AS SUBRM3CHAR,
                                        SUBRMV1.UserName AS SUBRMV1CHARVAL,SUBRMV2.UserName AS SUBRMV2CHARVAL,SUBRMV3.UserName AS SUBRMV3CHARVAL,

                                        ADCS.IsFirstCharacteristicCommon, ADCS.IsSecondCharacteristicCommon,ADCS.IsThirdCharacteristicCommon,

                                        SUBSKUC1.UserName AS SUBSKU1CHAR,SUBSKUC2.UserName AS SUBSKU2CHAR,SUBSKUC3.UserName AS SUBSKU3CHAR,
                                        SUBSKUV1.UserName AS SUBSKUV1CHARVAL,SUBSKUV2.UserName AS SUBSKUV2CHARVAL,SUBSKUV3.UserName AS SUBSKUV3CHARVAL
                                        --------------------

                                         FROM BOMMasterAttachmentWithItem AS ATT
                                        INNER JOIN BOMMaster AS b ON b.Id=att.BOMMasterId
                                        INNER JOIN BOMAttachmentDetail AS bd ON bd.BOMMasterAttachmentWithItemId=att.Id
                                        LEFT JOIN BOMAttachmentSKUMapping AS bs ON bs.BOMAttachmentDetailId=bd.Id 
					                                        AND  isnull(bs.FGFirstCharacteristicsValueId,'') IN (
						                                        SELECT '' UNION
					                                        SELECT fc.CharacteristicsValueId FROM trn.FirstCharacteristics AS fc
					                                        INNER JOIN trn.SalesOrder AS so ON so.Id=fc.SalesOrderId
					                                        WHERE so.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where MasterOrderId='" + MasterOrderId + @"')
					                                        )

						                                        AND  isnull(bs.FGSecondCharacteristicsValueId,'') IN (
							                                        SELECT '' UNION
						                                        SELECT sc.CharacteristicsValueId FROM trn.SecondCharacteristics AS sc 
						                                        INNER JOIN trn.SalesOrder AS so ON so.Id=sc.SalesOrderId
						                                        WHERE so.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where MasterOrderId='" + MasterOrderId + @"')
						                                        )


					                                        AND  isnull(bs.FGFirstCharacteristicsValueId,'') IN (
						                                        SELECT '' UNION
					                                        SELECT fc.CharacteristicsValueId FROM trn.ThirdCharacteristics AS  fc
					                                        INNER JOIN trn.SalesOrder AS so ON so.Id=fc.SalesOrderId
					                                        WHERE so.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where MasterOrderId='" + MasterOrderId + @"')
						                                        )

                                        INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=att.MasterOrderItemId
                                        LEFT OUTER JOIN mst.MaterialMaster AS mmf ON mmf.Id=moi.MaterialMasterId
                                        LEFT OUTER JOIN mst.MaterialMasterArticle AS mmaf ON mmaf.Id=moi.ArticleId


                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV1 ON RMv1.Id=bd.FirstCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV2 ON RMv2.Id=bd.SecondCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV3 ON RMv3.Id=bd.ThirdCharacteristicsValueId

                                        LEFT OUTER JOIN [HKP].[Characteristics] RMC1 ON RMC1.Id=RMV1.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] RMC2 ON RMC2.Id=RMV2.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] RMC3 ON RMC3.Id=RMV3.CharacteristicsId

                                        LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=bd.RMMaterialMasterId
                                        LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=bd.RMArticleId
                                        LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=BD.UoMId
                                        LEFT OUTER JOIN HKP.Party P ON p.Id=bd.VendorId
                                        LEFT JOIN hkp.Process AS pr ON pr.Id=bd.ProcessId

                                        JOIN AttachmentDetailConsumption AS adc ON adc.BOMAttachmentDetailId=bd.Id
                                        LEFT JOIN mst.MaterialMaster AS submm ON submm.Id=adc.RMMaterialMasterId
                                        LEFT JOIN mst.MaterialMasterArticle AS submma ON submma.Id=adc.RMArticleId
                                        LEFT JOIN scs.UnitOfMeasurement AS subuom ON subuom.Id=adc.UoMId
                                        LEFT JOIN HKP.Party subP ON subp.Id=adc.VendorId
                                        LEFT JOIN hkp.Process AS subpr ON subpr.Id=adc.ProcessId 

                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBRMV1 ON SUBRMV1.Id=ADC.FirstCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBRMV2 ON SUBRMV2.Id=ADC.SecondCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBRMV3 ON SUBRMV3.Id=ADC.ThirdCharacteristicsValueId

                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBRMC1 ON SUBRMC1.Id=SUBRMV1.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBRMC2 ON SUBRMC2.Id=SUBRMV2.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBRMC3 ON SUBRMC3.Id=SUBRMV3.CharacteristicsId


                                        LEFT JOIN AttachmentDetailConsumptionSKUMapping AS adcs ON adcs.AttachmentDetailConsumptionId=adc.Id


                                       LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBSKUV1 ON SUBSKUV1.Id=ADCS.SubFirstCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBSKUV2 ON SUBSKUV2.Id=ADCS.SubSecondCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBSKUV3 ON SUBSKUV3.Id=ADCS.SubThirdCharacteristicsValueId

                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBSKUC1 ON SUBSKUC1.Id=SUBSKUV1.CharacteristicsId OR SUBSKUC1.Id=(SELECT TOP 1 RMFirstCharacteristicsId FROM AttachmentDetailConsumptionSKUMapping WHERE AttachmentDetailConsumptionId=adc.Id AND isnull(IsFirstCharacteristicCommon,0)=1)
                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBSKUC2 ON SUBSKUC2.Id=SUBSKUV2.CharacteristicsId OR SUBSKUC1.Id=(SELECT TOP 1 RMSecondCharacteristicsId FROM AttachmentDetailConsumptionSKUMapping WHERE AttachmentDetailConsumptionId=adc.Id AND isnull(IsSecondCharacteristicCommon,0)=1)
                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBSKUC3 ON SUBSKUC3.Id=SUBSKUV3.CharacteristicsId OR SUBSKUC1.Id=(SELECT TOP 1 RMThirdCharacteristicsId FROM AttachmentDetailConsumptionSKUMapping WHERE AttachmentDetailConsumptionId=adc.Id AND isnull(IsThirdCharacteristicCommon,0)=1)



                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV1 ON SKUv1.Id=ADCS.RMFirstCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV2 ON SKUv2.Id=ADCS.RMSecondCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV3 ON SKUv3.Id=ADCS.RMThirdCharacteristicsValueId

                                        LEFT OUTER JOIN [HKP].[Characteristics] SKUC1 ON SKUC1.Id=SKUV1.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] SKUC2 ON SKUC2.Id=SKUV2.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] SKUC3 ON SKUC3.Id=SKUV3.CharacteristicsId

                                        WHERE att.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where MasterOrderId='" + MasterOrderId + @"')
                                        ORDER BY bd.Sequence";

            sheet.Name = "BOM Template Sub Material";

            DataTable dtData = _sqlRepository.GetDataTable(sql);

            if (dtData.Rows.Count == 0)
                return;

            int ROW = 1;
            sheet[ROW, 1].Text = "BOM Template Items (Sub Material)";
            sheet.Range[ROW, 1, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet[ROW, 1].CellStyle.Font.Bold = true;
            ROW++;
            sheet[ROW, 1].Text = "BOM Id";
            sheet[ROW, 3].Text = dtData.Rows[0]["BOMMasterId"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "BOM Desc";
            sheet[ROW, 3].Text = dtData.Rows[0]["BOMDesc"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "Material";
            sheet[ROW, 3].Text = dtData.Rows[0]["FGMaterial"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "Article";
            sheet[ROW, 3].Text = dtData.Rows[0]["FGArticle"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW += 2;
            int COL = 1;
            sheet[ROW, COL].Text = "Sl. No";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 6;
            int colSlNo = COL;
            COL++;
            sheet[ROW, COL].Text = "Sub Material";
            sheet[ROW, COL].ColumnWidth = 16;
            int colSubMaterial = COL;
            COL++;
            sheet[ROW, COL].Text = "Sub  Article";
            sheet[ROW, COL].ColumnWidth = 40;
            int colSubArticle = COL;
            COL++;
            sheet[ROW, COL].Text = "Parent Material";
            sheet[ROW, COL].ColumnWidth = 16;
            int colMaterial = COL;
            COL++;
            sheet[ROW, COL].Text = "Parent Article";
            sheet[ROW, COL].ColumnWidth = 40;
            int colArticle = COL;
            COL++;
            sheet[ROW, COL].Text = "RM Desc";
            sheet[ROW, COL].ColumnWidth = 22;
            int colRMDescription = COL;

            COL++;
            sheet[ROW, COL].Text = "Customer Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMCustomerSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMVendorSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU Desc";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUDesc = COL;
            COL++;
            sheet[ROW, COL].Text = "Cons.";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colConsumption = COL;
            COL++;
            sheet[ROW, COL].Text = "UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Wast.%";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colWastagePer = COL;
            COL++;
            sheet[ROW, COL].Text = "Process";
            sheet[ROW, COL].ColumnWidth = 8;
            int colProcess = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor";
            sheet[ROW, COL].ColumnWidth = 20;
            int colVendor = COL;
            COL++;
            sheet[ROW, COL].Text = "Common SKU";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMSKUCommon = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV2 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV3 = COL;

            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].Merge();
            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].Text = "COMMON SKU";
            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colRMV1, ROW, colRMV3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;

            COL++;
            sheet[ROW, COL].Text = "RM SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SUB RM SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV1 = COL;
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].Merge();
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].Text = "RM SKU-1 MAPPING";
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV1, ROW, colSKUV1].CellStyle.Interior.ColorIndex = ExcelKnownColors.LightGreen;



            COL++;
            sheet[ROW, COL].Text = "RM SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV2 = COL;

            COL++;
            sheet[ROW, COL].Text = "SUB RM SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV2 = COL;

            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].Merge();
            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].Text = "RM SKU-2 MAPPING";
            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV2, ROW, colSKUV2].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;



            COL++;
            sheet[ROW, COL].Text = "RM SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV3 = COL;
            COL++;
            sheet[ROW, COL].Text = "SUB RM SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV3 = COL;

            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].Merge();
            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].Text = "RM SKU-3 MAPPING";
            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV3, ROW, colSKUV3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_orange;

            sheet.Range[ROW - 1, 1, ROW, COL].CellStyle.Font.Bold = true; //row 19 of heading 
            sheet.Range[ROW - 1, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, colRMV1 - 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            int StartRow = ROW; //row 20
            for (int i = 0; i < dtData.Rows.Count; i++)
            {


                sheet[ROW, colSlNo].Number = (i + 1);
                sheet[ROW, colMaterial].Text = dtData.Rows[i]["RMMaterial"].ToString();
                sheet[ROW, colArticle].Text = dtData.Rows[i]["RMArticle"].ToString();
                sheet[ROW, colSubMaterial].Text = dtData.Rows[i]["SubRMMaterial"].ToString();
                sheet[ROW, colSubArticle].Text = dtData.Rows[i]["SubRMArticle"].ToString();
                sheet[ROW, colRMDescription].Text = dtData.Rows[i]["Description"].ToString();

                sheet[ROW, colRMCustomerSpec].Text = dtData.Rows[i]["CustomerSpec"].ToString();
                sheet[ROW, colRMVendorSpec].Text = dtData.Rows[i]["VendorSpec"].ToString();
                sheet[ROW, colSKUDesc].Text = dtData.Rows[i]["SKUDesc"].ToString();
                sheet[ROW, colVendor].Text = dtData.Rows[i]["Vendor"].ToString();
                sheet[ROW, colUOM].Text = dtData.Rows[i]["UOM"].ToString();
                sheet[ROW, colProcess].Text = dtData.Rows[i]["Process"].ToString();

                sheet[ROW, colRMSKUCommon].Text = dtData.Rows[i]["RMSKUCommon"].ToString();

                sheet[ROW, colRMV1].Text = dtData.Rows[i]["SUBRMV1CHARVAL"].ToString();
                sheet[ROW, colRMV2].Text = dtData.Rows[i]["SUBRMV2CHARVAL"].ToString();
                sheet[ROW, colRMV3].Text = dtData.Rows[i]["SUBRMV3CHARVAL"].ToString();



                sheet[ROW, colFGV1].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsFirstCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["SUBSKU1CHAR"].ToString() + "]" : dtData.Rows[i]["SKUV1CHARVAL"].ToString();
                sheet[ROW, colFGV2].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsSecondCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["SUBSKU2CHAR"].ToString() + "]" : dtData.Rows[i]["SKUV2CHARVAL"].ToString();
                sheet[ROW, colFGV3].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsThirdCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["SUBSKU3CHAR"].ToString() + "]" : dtData.Rows[i]["SKUV3CHARVAL"].ToString();


                sheet[ROW, colSKUV1].Text = dtData.Rows[i]["SUBSKUV1CHARVAL"].ToString();
                sheet[ROW, colSKUV2].Text = dtData.Rows[i]["SUBSKUV2CHARVAL"].ToString();
                sheet[ROW, colSKUV3].Text = dtData.Rows[i]["SUBSKUV3CHARVAL"].ToString();

                sheet[ROW, colConsumption].Number = clsStaticInfo.dbl(dtData.Rows[i]["Consumption"].ToString());
                sheet[ROW, colWastagePer].Number = clsStaticInfo.dbl(dtData.Rows[i]["WastagePer"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            sheet.IsGridLinesVisible = false;

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

            sheet.Range[8, 4].FreezePanes();

            sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
            sheet.Range[StartRow, colConsumption, ROW, colConsumption].NumberFormat = clsStaticInfo.NumberFormat(4);
            sheet.Range[StartRow, colWastagePer, ROW, colWastagePer].NumberFormat = clsStaticInfo.NumberFormat(2);

        }

        #endregion

        #region BOMReportByContractLevelItemandSalesOrder

        public IWorkbook GetMasterOrderByContractReports(string ContractId, BOMLevel Level, bool isMatrix = true)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;
            IWorkbook workbook = application.Workbooks.Create(1);
            //Load the existing Excel workbook into IWorkbook
            if (Level == BOMLevel.Item)
                workbook = application.Workbooks.Create(3);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            try
            {
                DataTable dtOrderMaster = _sqlRepository.GetDataTable(@"select mo.Id, mo.type, b.UserName as Buyer, p.UserName as Customer
                    , mo.OrderYear as Year, mo.TotalQty as TotalQuantity
                    , uom.UserName as UnitOfMeasurement, mo.NoOfLineItem, mo.OrderWastagePercentage
                    , mo.ExtraOrderPercentage, mo.BuyerReferenceNo, mo.OwnReferenceNo, BDept.UserName as BuyerDepartment
                    , BDev.UserName as BuyerDevision, MoCur.Code MasterOrderCurrency from trn.MasterOrder MO 
                    LEFT JOIN SCS.Currency MoCur on MoCur.id = mo.CurrencyId 
                    LEFT JOIN SCS.UnitOfMeasurement UOM on uom.id = mo.TotalQtyUOMId 
                    LEFT JOIN HKP.Buyer B on b.id = mo.buyerid 
                    LEFT JOIN HKP.Party p on p.id = mo.partyid 
                    LEFT JOIN HKP.BuyerDepartment BDept on BDept.id = mo.buyerDepartmentid 
                    LEFT JOIN HKP.BuyerDivision BDev on BDev.id = mo.BuyerDivisionId 
                    LEFT JOIN TRN.MasterOrderItem MOI  on MOI.MasterOrderId=MO.Id
					WHERE MOI.ContractId='" + ContractId + "'");
                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                DataTable dtMasterOrderItem = _sqlRepository.GetDataTable(@"select moi.id as MasterOrderItemNo,moi.BuyerReferenceNo
                 ,moi.OwnReferenceNo,moi.TotalQty as TotalMOIQuantity, moi.MasterOrderId,c.ContractNo,ml.LCRef
                 ,moi.OrderWastagePercentage, moi.ExtraOrderPercentage ,mm.UserName as Material ,mma.StandardName as Article, moi.Type
                 from trn.MasterOrderItem MOI
                 left join TRN.MasterOrder mo  on mo.id=moi.MasterOrderId
                 left join MST.MaterialMaster MM on mm.id = moi.MaterialMasterId
                 left join MST.MaterialMasterArticle mma on mma.id= moi.ArticleId
                 left join scs.TestingStandard ts on ts.id=moi.TestingStandardId
LEFT JOIN TRN.SalesOrder so on moi.Id=so.MasterOrderItemId
                 LEFT JOIN [Contract] AS c ON c.Id=so.ContractId
                 LEFT JOIN MasterLC AS ml ON ml.Id=c.MasterLCId
				 WHERE MOI.ContractId='" + ContractId + "'");

                DataTable dtSalesOrderItem = _sqlRepository.GetDataTable(@"select so.MasterOrderItemId, so.id as SalesOrderNo,cpo.PONumber,os.UserName as OrderStatus,d.UserName as Destination
                ,so.Qty as Quantity, so.UpCharge, so.MainRawMaterialInhouseDate, so.Description
                ,so.SOType, oc.username as OrderCategory
                ,so.DeliveryDate, sm.UserName as ShipmentMode
                ,so.Rate, so.Discount,so.CM,so.LSD, so.OtherRawMaterialInhouseDate , so.Reason , so.CommitmentDate

                ,c1.username as FirstCharacteristics, isnull(fcs.ValueFreeText,CV1.UserName) as FirstCharacteristicsValue
                ,c2.username as SecondCharacteristics ,isnull(SCS.ValueFreeText, CV2.UserName) as SecondCharacteristicsValue
                ,c3.username as ThirdCharacteristics , isnull(ThirdCS.ValueFreeText,CV3.UserName) as ThirdCharacteristicsValue
                ,case when isnull(thirdCs.Id,'')<>'' THEN ThirdCs.Qty
                ELSE case when isnull(scs.id,'')<>'' THEN scs.Qty
                ELSE case when isnull(fcs.Id,'')<>'' THEN fcs.Qty
                ELSE 0 END END END AS Qty
                from trn.SalesOrder SO
                LEFT JOIN TRN.masterorderitem moi on moi.id= so.masterorderitemid
                LEFT JOIN HKP.OrderCategory OC on oc.id = so.OrderCategoryId
                LEFT JOIN HKP.OrderStatus OS on os.id = so.OrderStatusId
                LEFT JOIN MST.shipMode SM on sm.id = so.shipmentModeId
                LEFT JOIN MST.Destination d on d.id =so.DestinationId
                LEFT JOIN TRN.CustomerPO CPO on cpo.id =so.CustomerPOId

                LEFT JOIN TRN.FirstCharacteristics FCS on fcs.SalesOrderId = so.id
                LEFT JOIN HKP.Characteristics C1 on c1.id = fcs.CharacteristicsId
                LEFT JOIN HKP.CharacteristicsValue CV1 on cv1.id= fcs.CharacteristicsValueId

                LEFT JOIN TRN.SecondCharacteristics SCS on scs.SalesOrderId=so.id and scs.FirstCharacteristicsId=fcs.Id
                LEFT JOIN HKP.Characteristics C2 on c2.id = scs.CharacteristicsId
                LEFT JOIN HKP.CharacteristicsValue CV2 on cv2.id= scs.CharacteristicsValueId

                LEFT JOIN TRN.ThirdCharacteristics ThirdCS on ThirdCS.SalesOrderId=so.id and scs.id=ThirdCS.SecondCharacteristicsId
                LEFT JOIN HKP.Characteristics C3 on c3.id = ThirdCS.CharacteristicsId
                LEFT JOIN HKP.CharacteristicsValue CV3 on CV3.id= ThirdCS.CharacteristicsValueId
                WHERE MOI.ContractId='" + ContractId + "'");

                DataTable dtBOMData = new DataTable();
                if (Level == BOMLevel.SO)
                {
                    //worksheet.Name = "DetailBOM-SO Level";
                    worksheet.Name = "Detail";
                    string strsql = @"SELECT b.Id, b.MasterOrderItemId,moi.MasterOrderId,moi.OwnReferenceNo,moi.BuyerReferenceNo, b.VendorId,b.SalesOrderId,
                                 mm.UserName AS Material,mma.StandardName AS Article,p.UserName AS Vendor,b.SKUDesc,

                                v1.UserName AS CharVal1,v2.UserName AS CharVal2,v3.UserName AS CharVal3,convert(bit,isnull(b.isParent,0)) AS isParent,
                                convert(bit,isnull(b.isChild,0)) AS isChild,PR.UserName AS Process,
                               CONVERT(BIT, isnull(b.RequiredQtyApproved,0)) AS RequiredQtyApproved,CONVERT(BIT, isnull(b.IncompleteMaterial,0)) AS IncompleteMaterial,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
                                b.BOMQty,b.RequiredQty,b.RequiredQtyPO,uom.UserName AS UOM,uomm.UserName AS ParentUOM,
                                POUOM.UserName AS POUOM,
                                b.RMDescription,	b.RMCustomerSpec,	b.RMVendorSpec
								,ISNULL(po.POQTY,0) POQTY,ISNULL(grn.GRNQty,0) GRNQty,
								
								 SO1 =STUFF((select distinct ','+xv1.UserName
								               from BOQFGMapping AS XM	 
										       JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
								               JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.FirstCharacteristicsValueId
								             WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO2 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.SecondCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO3 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.ThirdCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								POIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.PurchaseOrderDetail AS XB2 ON xb2.Id=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								GRNIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.InventoryReceiveDetail AS XB2 ON xb2.PODetailsId=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                  FROM BOQ AS b
                                LEFT OUTER JOIN MST.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                                LEFT OUTER JOIN MST.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                                LEFT OUTER JOIN SCS.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                                LEFT OUTER JOIN SCS.UnitOfMeasurement AS POuom ON POuom.Id=b.POUoMId
                                LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                                LEFT OUTER JOIN TRN.SalesOrder AS so ON so.Id=b.SalesOrderId
                                LEFT OUTER JOIN TRN.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                                LEFT OUTER JOIN TRN.masterorder MO ON MO.Id=moi.MasterOrderId
                                LEFT OUTER JOIN SCS.UnitOfMeasurement AS uomm ON uomm.Id=mo.TotalQtyUOMId
                                LEFT JOIN HKP.Process AS pr ON pr.Id=b.ProcessId

                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId
								Left outer join (	select BOQDetailId,sum(POBOQQty) as POQTY from trn.POBOQMAP  GRoup by BOQDetailId) PO On PO.BOQDetailId = b.Id
								Left outer join (	
								SELECT a.BOQDetailId , sum(b.TransactionQty) GRNQty 
				FROM trn.POBOQMAP a
				INNER JOIN trn.InventoryReceiveDetail b ON a.PODetailId=b.PODetailsId GRoup by BOQDetailId
								
								) GRN On GRN.BOQDetailId = b.Id

                                WHERE so.ContractId='" + ContractId + @"' and isnull(B.Id,'') NOT IN (select ParentId from BOQ where isnull(ParentId,'')<>'' AND MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where ContractId='" + ContractId + @"'))
                                ORDER BY isnull(b.Sequence,0),b.SalesOrderId";
                    dtBOMData = _sqlRepository.GetDataTable(strsql);
                }
                else if (Level == BOMLevel.Item)
                {
                    //worksheet.Name = "BOM-Item Level";
                    worksheet.Name = "Summary";
                    string strsql = @"select B.Sequence, B.MasterOrderItemId, B.MasterOrderId, B.OwnReferenceNo,
       B.BuyerReferenceNo, B.VendorId, B.Material, B.Article, B.Vendor, B.SKUDesc,B.POIds, B.GRNIds,
       B.CharVal1, B.CharVal2, B.CharVal3, B.isParent, B.isChild, B.Process,
       B.Consumption, B.WastagePer, B.UOM, B.ParentUOM, B.POUOM, B.RMDescription,
       B.RMCustomerSpec, B.RMVendorSpec, B.SO1, B.SO2, B.SO3,
       sum(b.BOMQty) AS BOMQty,sum(b.RequiredQty) AS RequiredQty, SUM(b.RequiredQtyPO) AS RequiredQtyPO, sum(b.OrderQty) AS OrderQty,sum(b.PlanOrderQty) AS PlanOrderQty
       ,SUM(ISNULL(b.POQTY,0)) POQTY,SUM(ISNULL(b.GRNQty,0)) GRNQty
  from (SELECT  b.Sequence,b.MasterOrderItemId,moi.MasterOrderId,moi.OwnReferenceNo,moi.BuyerReferenceNo, b.VendorId,
                                 mm.UserName AS Material,mma.StandardName AS Article,p.UserName AS Vendor,b.SKUDesc,
                                v1.UserName AS CharVal1,v2.UserName AS CharVal2,v3.UserName AS CharVal3,convert(bit,isnull(b.isParent,0)) AS isParent,
                                convert(bit,isnull(b.isChild,0)) AS isChild,PR.UserName AS Process,
                               b.Consumption,b.WastagePer,
                                uom.UserName AS UOM,uomm.UserName AS ParentUOM, POUOM.UserName AS POUOM,
                                b.RMDescription,	b.RMCustomerSpec,	b.RMVendorSpec,
								b.BOMQty,b.RequiredQty,b.RequiredQtyPO, b.OrderQty,PlanOrderQty,
                                ISNULL(po.POQTY,0) POQTY,ISNULL(grn.GRNQty,0) GRNQty,
								 SO1 =STUFF((select distinct ','+xv1.UserName
								          from BOQFGMapping AS XM	
										  JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
								          JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.FirstCharacteristicsValueId
								          WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO2 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.SecondCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO3 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.ThirdCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								POIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.PurchaseOrderDetail AS XB2 ON xb2.Id=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								GRNIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.InventoryReceiveDetail AS XB2 ON xb2.PODetailsId=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                  FROM BOQ AS b
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS POuom ON POuom.Id=b.POUoMId

                                LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                                LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
                                LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                                LEFT OUTER JOIN trn.masterorder MO ON MO.Id=moi.MasterOrderId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uomm ON uomm.Id=mo.TotalQtyUOMId
                                LEFT JOIN hkp.Process AS pr ON pr.Id=b.ProcessId

                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId
                                LEFT OUTER JOIN (	select BOQDetailId,sum(POBOQQty) as POQTY from trn.POBOQMAP  GRoup by BOQDetailId) PO On PO.BOQDetailId = b.Id
								LEFT OUTER JOIN (	
								SELECT a.BOQDetailId , sum(b.TransactionQty) GRNQty 
				                    FROM trn.POBOQMAP a
				                    INNER JOIN trn.InventoryReceiveDetail b ON a.PODetailId=b.PODetailsId GRoup by BOQDetailId
								
								) GRN On GRN.BOQDetailId = b.Id

                                WHERE so.ContractId='" + ContractId + @"' and isnull(B.Id,'') NOT IN (select ParentId from BOQ where isnull(ParentId,'')<>'' AND MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where ContractId='" + ContractId + @"'))
                        ) AS B
                                GROUP BY B.Sequence,B.POIds, B.GRNIds, B.MasterOrderItemId, B.MasterOrderId, B.OwnReferenceNo,
       B.BuyerReferenceNo, B.VendorId, B.Material, B.Article, B.Vendor, B.SKUDesc,
       B.CharVal1, B.CharVal2, B.CharVal3, B.isParent, B.isChild, B.Process,
       B.Consumption, B.WastagePer, B.UOM, B.ParentUOM, B.POUOM, B.RMDescription,
       B.RMCustomerSpec, B.RMVendorSpec, B.SO1, B.SO2, B.SO3
                                
                                ORDER BY isnull(b.Sequence,0)";
                    dtBOMData = _sqlRepository.GetDataTable(strsql);


                }
                int ROW = 6; int COL = 1;

                //foreach (ExcelKnownColors val in Enum.GetValues(typeof(ExcelKnownColors)))
                //{
                //    worksheet[ROW, 1].CellStyle.Interior.ColorIndex = val;
                //    worksheet[ROW, 1].Text = val.ToString();
                //    ROW++;
                //}

                int MasterOrderDetailsStartRow = ROW;
                worksheet[ROW, COL].Text = "Master Order Details:";
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                ROW++;

                int leftColumnCaption = COL;
                int leftColumnValue = leftColumnCaption + 1;

                int MiddleColumnCaption = leftColumnValue + 2;
                int MiddleColumnValue = MiddleColumnCaption + 1;

                int RightColumnCaption = MiddleColumnValue + 2;
                int RightColumnValue = RightColumnCaption + 1;

                //Master Order.............................................................
                //worksheet[ROW, leftColumnCaption].Text = "Master Order No";
                //worksheet[ROW, leftColumnValue].Text = "MasterOrderNo";

                worksheet[ROW, leftColumnCaption].Text = "Order#";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Id"].ToString();
                // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                worksheet.Range[ROW, leftColumnCaption, ROW, leftColumnValue].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Buyer.Ref";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["BuyerReferenceNo"].ToString();
                worksheet.Range[ROW, MiddleColumnCaption, ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, RightColumnCaption].Text = "Own.Ref";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["OwnReferenceNo"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                ROW++;

                worksheet[ROW, leftColumnCaption].Text = "Buyer";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Buyer"].ToString();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Buyer Dep.";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["BuyerDepartment"].ToString();
                worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, RightColumnCaption].Text = "Buyer Div";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["BuyerDevision"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                ROW++;

                worksheet[ROW, leftColumnCaption].Text = "Customer";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Customer"].ToString();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Currency";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["MasterOrderCurrency"].ToString();
                worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;
                ROW++;


                worksheet[ROW, leftColumnCaption].Text = "Total Order Quantity";
                worksheet[ROW, leftColumnValue].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[0]["TotalQuantity"].ToString());
                worksheet[ROW, leftColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                // worksheet[ROW, leftColumnValue, ROW , leftColumnValue].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, leftColumnValue, ROW, leftColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                //worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Bold = true;
                worksheet[ROW, leftColumnValue].HorizontalAlignment = ExcelHAlign.HAlignRight;

                worksheet[ROW, leftColumnValue + 1].Text = dtOrderMaster.Rows[0]["UnitOfMeasurement"].ToString();
                //worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet.Range[MasterOrderDetailsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom44;



                ROW += 2;


                //Master Order Item....................................................................................................
                StringCollection strColSO = new StringCollection();

                for (int i = 0; i < dtMasterOrderItem.Rows.Count; i++)
                {
                    int MasterItemsStartRow = ROW; // row 12
                    worksheet[ROW, COL].Text = "Item Id:"; //col 1
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["MasterOrderItemNo"].ToString();
                    worksheet[ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                    worksheet.Range[ROW, COL, ROW, leftColumnValue].CellStyle.Font.Bold = true;
                    ROW++;


                    // int MasterItemsStartRow = ROW;
                    strColSO = new StringCollection();
                    // worksheet[ROW, leftColumnCaption].Text = "Items Details";



                    worksheet[ROW, leftColumnCaption].Text = "Material";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["Material"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Article";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["Article"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Contract#";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["ContractNo"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, MiddleColumnCaption].Text = "LC Ref";
                    worksheet[ROW, MiddleColumnValue].Text = dtMasterOrderItem.Rows[i]["LCRef"].ToString();
                    worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Buyer Ref";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["BuyerReferenceNo"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, MiddleColumnCaption].Text = "Own Ref";
                    worksheet[ROW, MiddleColumnValue].Text = dtMasterOrderItem.Rows[i]["OwnReferenceNo"].ToString();
                    worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, RightColumnCaption].Text = "Qty";
                    worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                    worksheet[ROW, RightColumnValue].Number = clsStaticInfo.dbl(dtMasterOrderItem.Rows[i]["TotalMOIQuantity"].ToString());
                    //worksheet.Range[ROW, RightColumnValue, ROW, RightColumnValue].CellStyle.Font.Bold = true;
                    // worksheet[ROW, RightColumnValue].CellStyle.Font.Bold = true;
                    worksheet[ROW, RightColumnValue, ROW, RightColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                    worksheet.Range[MasterItemsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom18;
                    ROW++;

                    if (Level == BOMLevel.SO)
                    {
                        dtSalesOrderItem.DefaultView.RowFilter = "MasterOrderItemId='" + dtMasterOrderItem.Rows[i]["MasterOrderItemNo"].ToString() + "'";
                        DataTable dtSalesOrderFilteredByItem = dtSalesOrderItem.DefaultView.ToTable();
                        for (int KK = 0; KK < dtSalesOrderItem.DefaultView.Count; KK++)
                        {


                            if (strColSO.Contains(dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString()))
                                continue;
                            int SOStartRow = ROW;  //row 16
                            worksheet[ROW, COL].Text = "Sales Order Details & Breakdown:";
                            worksheet[ROW, COL].CellStyle.Font.Bold = true;
                            ROW++;

                            // int SOStartRow = ROW;

                            strColSO.Add(dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString());

                            worksheet[ROW, leftColumnCaption].Text = "SO No";
                            worksheet[ROW, leftColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString();
                            worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                            worksheet[ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                            worksheet[ROW, leftColumnValue].CellStyle.Font.Bold = true;


                            worksheet[ROW, MiddleColumnCaption].Text = "Del. Date";
                            worksheet[ROW, MiddleColumnValue].Text = Convert.ToDateTime(dtSalesOrderItem.DefaultView[KK]["DeliveryDate"].ToString()).ToString("dd-MMM-yyyy");
                            worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                            worksheet[ROW, RightColumnCaption].Text = "Qty";
                            worksheet[ROW, RightColumnValue].Number = clsStaticInfo.dbl(dtSalesOrderItem.DefaultView[KK]["Quantity"].ToString());
                            worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                            worksheet[ROW, RightColumnValue, ROW, RightColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                            // worksheet[ROW, RightColumnValue].CellStyle.Font.Bold = true;
                            ROW++;

                            worksheet[ROW, leftColumnCaption].Text = "Dest.";
                            worksheet[ROW, leftColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["Destination"].ToString();
                            worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;


                            worksheet[ROW, MiddleColumnCaption].Text = "Ship Mode";
                            worksheet[ROW, MiddleColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["ShipmentMode"].ToString();
                            worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                            worksheet[ROW, RightColumnCaption].Text = "Ord. Status";
                            worksheet[ROW, RightColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["OrderStatus"].ToString();
                            worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                            worksheet.Range[SOStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom19;

                            ROW++;

                            dtSalesOrderFilteredByItem.DefaultView.RowFilter = "SalesOrderNo='" + dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString() + "'"; //????
                            DataTable dtBreakdownData = dtSalesOrderFilteredByItem.DefaultView.ToTable();
                            DrawSOBreakdownData(dtBreakdownData, worksheet, ref ROW, isMatrix);

                            ROW++;
                            //BOM Data here
                            dtBOMData.DefaultView.RowFilter = "SalesOrderId='" + dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString() + "'";
                            DrawBOMData(dtBOMData.DefaultView.ToTable(), worksheet, ref ROW);

                            ROW++;
                        }
                    }
                    else
                    {
                        DrawBOMData(dtBOMData, worksheet, ref ROW);
                    }

                    ROW += 2; // Gap for Material
                }

                int endCol = RightColumnValue;


                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.CellStyle.Font.Size = 8f;
                worksheet.UsedRange.WrapText = true;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref worksheet, endCol, "Master Order#" + dtOrderMaster.Rows[0]["Id"].ToString(), identity.PlantId);
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;
                return workbook;

            }
            catch (Exception ex)
            {
                throw (ex);

            }
        }

        public void GetDrawBOMTemplateByContractDataReports(IWorksheet sheet, string ContractId)
        {
            string sql = @"SELECT att.BOMMasterId, b.[Description] AS BOMDesc,mmf.UserName FGMaterial,mmaf.StandardName AS FGArticle, mm.UserName RMMaterial,mma.StandardName AS RMArticle,p.UserName AS Vendor,
                                    pr.UserName AS Process,uom.UserName AS UOM,bs.[Description] AS SKUDesc,
                                    bd.[Description], bd.CustomerSpec, bd.VendorSpec, bd.Consumption, bd.WastagePer,
                                    bd.IsSKUCommon AS RMSKUCommon,

                                    --COMMON RM MAPPING---
                                    RMC1.UserName AS RM1CHAR,RMC2.UserName AS RM2CHAR,RMC3.UserName AS RM3CHAR,
                                    RMV1.UserName AS RMV1CHARVAL,RMV2.UserName AS RMV2CHARVAL,RMV3.UserName AS RMV3CHARVAL,
                                    --FG MAPPING
                                    FGC1.UserName AS FG1CHAR,FGC2.UserName AS FG2CHAR,FGC3.UserName AS FG3CHAR,
                                    FGV1.UserName AS FGV1CHARVAL,FGV2.UserName AS FGV2CHARVAL,FGV3.UserName AS FGV3CHARVAL,

                                    BS.IsFirstCharacteristicCommon, BS.IsSecondCharacteristicCommon,BS.IsThirdCharacteristicCommon,

                                    SKUC1.UserName AS SKU1CHAR,SKUC2.UserName AS SKU2CHAR,SKUC3.UserName AS SKU3CHAR,
                                    SKUV1.UserName AS SKUV1CHARVAL,SKUV2.UserName AS SKUV2CHARVAL,SKUV3.UserName AS SKUV3CHARVAL
                                    --------------------

                                     FROM BOMMasterAttachmentWithItem AS ATT
                                    INNER JOIN BOMMaster AS b ON b.Id=att.BOMMasterId
                                    INNER JOIN BOMAttachmentDetail AS bd ON bd.BOMMasterAttachmentWithItemId=att.Id
                                    LEFT JOIN BOMAttachmentSKUMapping AS bs ON bs.BOMAttachmentDetailId=bd.Id 
					                                    AND  isnull(bs.FGFirstCharacteristicsValueId,'') IN (
						                                    SELECT '' UNION
					                                    SELECT fc.CharacteristicsValueId FROM trn.FirstCharacteristics AS fc
					                                    INNER JOIN trn.SalesOrder AS so ON so.Id=fc.SalesOrderId
					                                    WHERE so.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where ContractId='" + ContractId + @"')
					                                    )

						                                    AND  isnull(bs.FGSecondCharacteristicsValueId,'') IN (
							                                    SELECT '' UNION
						                                    SELECT sc.CharacteristicsValueId FROM trn.SecondCharacteristics AS sc 
						                                    INNER JOIN trn.SalesOrder AS so ON so.Id=sc.SalesOrderId
						                                    WHERE so.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where ContractId='" + ContractId + @"')
						                                    )


					                                    AND  isnull(bs.FGFirstCharacteristicsValueId,'') IN (
						                                    SELECT '' UNION
					                                    SELECT fc.CharacteristicsValueId FROM trn.ThirdCharacteristics AS  fc
					                                    INNER JOIN trn.SalesOrder AS so ON so.Id=fc.SalesOrderId
					                                    WHERE so.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where ContractId='" + ContractId + @"')
						                                    )

                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=att.MasterOrderItemId
                                    LEFT OUTER JOIN mst.MaterialMaster AS mmf ON mmf.Id=moi.MaterialMasterId
                                    LEFT OUTER JOIN mst.MaterialMasterArticle AS mmaf ON mmaf.Id=moi.ArticleId


                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV1 ON RMv1.Id=bd.FirstCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV2 ON RMv2.Id=bd.SecondCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV3 ON RMv3.Id=bd.ThirdCharacteristicsValueId

                                    LEFT OUTER JOIN [HKP].[Characteristics] RMC1 ON RMC1.Id=RMV1.CharacteristicsId
                                    LEFT OUTER JOIN [HKP].[Characteristics] RMC2 ON RMC2.Id=RMV2.CharacteristicsId
                                    LEFT OUTER JOIN [HKP].[Characteristics] RMC3 ON RMC3.Id=RMV3.CharacteristicsId


                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] FGV1 ON FGv1.Id=bs.FGFirstCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] FGV2 ON FGv2.Id=bs.FGSecondCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] FGV3 ON FGv3.Id=bs.FGThirdCharacteristicsValueId

                                    LEFT JOIN BOMAttachmentSKUMapping AS bscf ON bs.BOMAttachmentDetailId=bd.Id AND bscf.id =(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping WHERE BOMAttachmentDetailId=bd.Id AND isnull(IsFirstCharacteristicCommon,0)=1)
                                    LEFT JOIN BOMAttachmentSKUMapping AS bscs ON bs.BOMAttachmentDetailId=bd.Id AND bscs.id =(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping WHERE BOMAttachmentDetailId=bd.Id AND isnull(IsSecondCharacteristicCommon,0)=1)
                                    LEFT JOIN BOMAttachmentSKUMapping AS bsct ON bs.BOMAttachmentDetailId=bd.Id AND bsct.id =(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping WHERE BOMAttachmentDetailId=bd.Id AND isnull(IsthirdCharacteristicCommon,0)=1)
                                    LEFT OUTER JOIN [HKP].[Characteristics] FGC1 ON FGC1.Id=isnull(bscf.FGFirstCharacteristicsId,FGV1.CharacteristicsId)
                                    LEFT OUTER JOIN [HKP].[Characteristics] FGC2 ON FGC2.Id=isnull(bscs.FGSecondCharacteristicsId,FGV2.CharacteristicsId)
                                    LEFT OUTER JOIN [HKP].[Characteristics] FGC3 ON FGC3.Id=isnull(bsct.FGThirdCharacteristicsId,FGV3.CharacteristicsId)

                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV1 ON SKUv1.Id=bs.RMFirstCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV2 ON SKUv2.Id=bs.RMSecondCharacteristicsValueId
                                    LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV3 ON SKUv3.Id=bs.RMThirdCharacteristicsValueId

                                    LEFT OUTER JOIN [HKP].[Characteristics] SKUC1 ON SKUC1.Id=SKUV1.CharacteristicsId
                                    LEFT OUTER JOIN [HKP].[Characteristics] SKUC2 ON SKUC2.Id=SKUV2.CharacteristicsId
                                    LEFT OUTER JOIN [HKP].[Characteristics] SKUC3 ON SKUC3.Id=SKUV3.CharacteristicsId

                                    LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=bd.RMMaterialMasterId
                                    LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=bd.RMArticleId
                                    LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=BD.UoMId
                                    LEFT OUTER JOIN HKP.Party P ON p.Id=bd.VendorId
                                    LEFT JOIN hkp.Process AS pr ON pr.Id=bd.ProcessId

                                    WHERE att.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where ContractId='" + ContractId + @"')
                                    ORDER BY bd.Sequence";

            sheet.Name = "BOM Template";

            DataTable dtData = _sqlRepository.GetDataTable(sql);
            int ROW = 1;
            sheet[ROW, 1].Text = "BOM Template Items";
            sheet.Range[ROW, 1, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet[ROW, 1].CellStyle.Font.Bold = true;
            ROW++;
            sheet[ROW, 1].Text = "BOM Id";
            sheet[ROW, 3].Text = dtData.Rows[0]["BOMMasterId"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "BOM Desc";
            sheet[ROW, 3].Text = dtData.Rows[0]["BOMDesc"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "Material";
            sheet[ROW, 3].Text = dtData.Rows[0]["FGMaterial"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "Article";
            sheet[ROW, 3].Text = dtData.Rows[0]["FGArticle"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW += 2;
            int COL = 1;
            sheet[ROW, COL].Text = "Sl. No";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 6;
            int colSlNo = COL;
            COL++;
            sheet[ROW, COL].Text = "Material";
            sheet[ROW, COL].ColumnWidth = 16;
            int colMaterial = COL;
            COL++;
            sheet[ROW, COL].Text = "Article";
            sheet[ROW, COL].ColumnWidth = 40;
            int colArticle = COL;
            COL++;
            sheet[ROW, COL].Text = "RM Desc";
            sheet[ROW, COL].ColumnWidth = 22;
            int colRMDescription = COL;

            COL++;
            sheet[ROW, COL].Text = "Customer Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMCustomerSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMVendorSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU Desc";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUDesc = COL;
            COL++;
            sheet[ROW, COL].Text = "Cons.";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colConsumption = COL;
            COL++;
            sheet[ROW, COL].Text = "UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Wast.%";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colWastagePer = COL;
            COL++;
            sheet[ROW, COL].Text = "Process";
            sheet[ROW, COL].ColumnWidth = 8;
            int colProcess = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor";
            sheet[ROW, COL].ColumnWidth = 20;
            int colVendor = COL;
            COL++;
            sheet[ROW, COL].Text = "Common SKU";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMSKUCommon = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV2 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV3 = COL;

            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].Merge();
            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].Text = "COMMON SKU";
            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colRMV1, ROW, colRMV3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;

            COL++;
            sheet[ROW, COL].Text = "FG SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV1 = COL;
            COL++;
            sheet[ROW, COL].Text = "RM SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV1 = COL;
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].Merge();
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].Text = "FG SKU-1 MAPPING";
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV1, ROW, colSKUV1].CellStyle.Interior.ColorIndex = ExcelKnownColors.LightGreen;



            COL++;
            sheet[ROW, COL].Text = "FG SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV2 = COL;

            COL++;
            sheet[ROW, COL].Text = "RM SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV2 = COL;

            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].Merge();
            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].Text = "FG SKU-2 MAPPING";
            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV2, ROW, colSKUV2].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;



            COL++;
            sheet[ROW, COL].Text = "FG SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV3 = COL;
            COL++;
            sheet[ROW, COL].Text = "RM SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV3 = COL;

            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].Merge();
            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].Text = "FG SKU-3 MAPPING";
            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV3, ROW, colSKUV3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_orange;

            sheet.Range[ROW - 1, 1, ROW, COL].CellStyle.Font.Bold = true; //row 19 of heading 
            sheet.Range[ROW - 1, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, colRMV1 - 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            int StartRow = ROW; //row 20
            for (int i = 0; i < dtData.Rows.Count; i++)
            {


                sheet[ROW, colSlNo].Number = (i + 1);
                sheet[ROW, colMaterial].Text = dtData.Rows[i]["RMMaterial"].ToString();
                sheet[ROW, colArticle].Text = dtData.Rows[i]["RMArticle"].ToString();
                sheet[ROW, colRMDescription].Text = dtData.Rows[i]["Description"].ToString();

                sheet[ROW, colRMCustomerSpec].Text = dtData.Rows[i]["CustomerSpec"].ToString();
                sheet[ROW, colRMVendorSpec].Text = dtData.Rows[i]["VendorSpec"].ToString();
                sheet[ROW, colSKUDesc].Text = dtData.Rows[i]["SKUDesc"].ToString();
                sheet[ROW, colVendor].Text = dtData.Rows[i]["Vendor"].ToString();
                sheet[ROW, colUOM].Text = dtData.Rows[i]["UOM"].ToString();
                sheet[ROW, colProcess].Text = dtData.Rows[i]["Process"].ToString();

                sheet[ROW, colRMSKUCommon].Text = dtData.Rows[i]["RMSKUCommon"].ToString();

                sheet[ROW, colRMV1].Text = dtData.Rows[i]["RMV1CHARVAL"].ToString();
                sheet[ROW, colRMV2].Text = dtData.Rows[i]["RMV2CHARVAL"].ToString();
                sheet[ROW, colRMV3].Text = dtData.Rows[i]["RMV3CHARVAL"].ToString();



                sheet[ROW, colFGV1].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsFirstCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["FG1CHAR"].ToString() + "]" : dtData.Rows[i]["FGV1CHARVAL"].ToString();
                sheet[ROW, colFGV2].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsSecondCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["FG2CHAR"].ToString() + "]" : dtData.Rows[i]["FGV2CHARVAL"].ToString();
                sheet[ROW, colFGV3].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsThirdCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["FG3CHAR"].ToString() + "]" : dtData.Rows[i]["FGV3CHARVAL"].ToString();


                sheet[ROW, colSKUV1].Text = dtData.Rows[i]["SKUV1CHARVAL"].ToString();
                sheet[ROW, colSKUV2].Text = dtData.Rows[i]["SKUV2CHARVAL"].ToString();
                sheet[ROW, colSKUV3].Text = dtData.Rows[i]["SKUV3CHARVAL"].ToString();

                sheet[ROW, colConsumption].Number = clsStaticInfo.dbl(dtData.Rows[i]["Consumption"].ToString());
                sheet[ROW, colWastagePer].Number = clsStaticInfo.dbl(dtData.Rows[i]["WastagePer"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            sheet.IsGridLinesVisible = false;

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

            sheet.Range[8, 4].FreezePanes();

            sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
            sheet.Range[StartRow, colConsumption, ROW, colConsumption].NumberFormat = clsStaticInfo.NumberFormat(4);
            sheet.Range[StartRow, colWastagePer, ROW, colWastagePer].NumberFormat = clsStaticInfo.NumberFormat(2);

        }

        public void GetDrawBOMTemplateByContractDataSubMaterials(IWorksheet sheet, string ContractId)
        {
            string sql = @"SELECT att.BOMMasterId, b.[Description] AS BOMDesc, mmf.UserName FGMaterial,mmaf.StandardName AS FGArticle, mm.UserName RMMaterial,mma.StandardName AS RMArticle,

                                        submm.UserName SubRMMaterial,submma.StandardName AS SubRMArticle,subp.UserName AS Vendor,
                                        subpr.UserName AS Process,subuom.UserName AS UOM,ADCS.[Description] AS SKUDesc,
                                        adc.[Description], adc.CustomerSpec, adc.VendorSpec, adc.Consumption, adc.WastagePer,
                                        adc.IsSKUCommon AS RMSKUCommon,



                                        --COMMON RM MAPPING---
                                        RMC1.UserName AS RM1CHAR,RMC2.UserName AS RM2CHAR,RMC3.UserName AS RM3CHAR,
                                        RMV1.UserName AS RMV1CHARVAL,RMV2.UserName AS RMV2CHARVAL,RMV3.UserName AS RMV3CHARVAL,

                                        --ADCS.IsFirstCharacteristicCommon, ADCS.IsSecondCharacteristicCommon,ADCS.IsThirdCharacteristicCommon,

                                        SKUC1.UserName AS SKU1CHAR,SKUC2.UserName AS SKU2CHAR,SKUC3.UserName AS SKU3CHAR,
                                        SKUV1.UserName AS SKUV1CHARVAL,SKUV2.UserName AS SKUV2CHARVAL,SKUV3.UserName AS SKUV3CHARVAL,


                                        --COMMON RM MAPPING FOR SUB MATERIAL---
                                        SUBRMC1.UserName AS SUBRM1CHAR,SUBRMC2.UserName AS SUBRM2CHAR,SUBRMC3.UserName AS SUBRM3CHAR,
                                        SUBRMV1.UserName AS SUBRMV1CHARVAL,SUBRMV2.UserName AS SUBRMV2CHARVAL,SUBRMV3.UserName AS SUBRMV3CHARVAL,

                                        ADCS.IsFirstCharacteristicCommon, ADCS.IsSecondCharacteristicCommon,ADCS.IsThirdCharacteristicCommon,

                                        SUBSKUC1.UserName AS SUBSKU1CHAR,SUBSKUC2.UserName AS SUBSKU2CHAR,SUBSKUC3.UserName AS SUBSKU3CHAR,
                                        SUBSKUV1.UserName AS SUBSKUV1CHARVAL,SUBSKUV2.UserName AS SUBSKUV2CHARVAL,SUBSKUV3.UserName AS SUBSKUV3CHARVAL
                                        --------------------

                                         FROM BOMMasterAttachmentWithItem AS ATT
                                        INNER JOIN BOMMaster AS b ON b.Id=att.BOMMasterId
                                        INNER JOIN BOMAttachmentDetail AS bd ON bd.BOMMasterAttachmentWithItemId=att.Id
                                        LEFT JOIN BOMAttachmentSKUMapping AS bs ON bs.BOMAttachmentDetailId=bd.Id 
					                                        AND  isnull(bs.FGFirstCharacteristicsValueId,'') IN (
						                                        SELECT '' UNION
					                                        SELECT fc.CharacteristicsValueId FROM trn.FirstCharacteristics AS fc
					                                        INNER JOIN trn.SalesOrder AS so ON so.Id=fc.SalesOrderId
					                                        WHERE so.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where ContractId='" + ContractId + @"')
					                                        )

						                                        AND  isnull(bs.FGSecondCharacteristicsValueId,'') IN (
							                                        SELECT '' UNION
						                                        SELECT sc.CharacteristicsValueId FROM trn.SecondCharacteristics AS sc 
						                                        INNER JOIN trn.SalesOrder AS so ON so.Id=sc.SalesOrderId
						                                        WHERE so.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where ContractId='" + ContractId + @"')
						                                        )


					                                        AND  isnull(bs.FGFirstCharacteristicsValueId,'') IN (
						                                        SELECT '' UNION
					                                        SELECT fc.CharacteristicsValueId FROM trn.ThirdCharacteristics AS  fc
					                                        INNER JOIN trn.SalesOrder AS so ON so.Id=fc.SalesOrderId
					                                        WHERE so.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where ContractId='" + ContractId + @"')
						                                        )

                                        INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=att.MasterOrderItemId
                                        LEFT OUTER JOIN mst.MaterialMaster AS mmf ON mmf.Id=moi.MaterialMasterId
                                        LEFT OUTER JOIN mst.MaterialMasterArticle AS mmaf ON mmaf.Id=moi.ArticleId


                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV1 ON RMv1.Id=bd.FirstCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV2 ON RMv2.Id=bd.SecondCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] RMV3 ON RMv3.Id=bd.ThirdCharacteristicsValueId

                                        LEFT OUTER JOIN [HKP].[Characteristics] RMC1 ON RMC1.Id=RMV1.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] RMC2 ON RMC2.Id=RMV2.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] RMC3 ON RMC3.Id=RMV3.CharacteristicsId

                                        LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=bd.RMMaterialMasterId
                                        LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=bd.RMArticleId
                                        LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=BD.UoMId
                                        LEFT OUTER JOIN HKP.Party P ON p.Id=bd.VendorId
                                        LEFT JOIN hkp.Process AS pr ON pr.Id=bd.ProcessId

                                        JOIN AttachmentDetailConsumption AS adc ON adc.BOMAttachmentDetailId=bd.Id
                                        LEFT JOIN mst.MaterialMaster AS submm ON submm.Id=adc.RMMaterialMasterId
                                        LEFT JOIN mst.MaterialMasterArticle AS submma ON submma.Id=adc.RMArticleId
                                        LEFT JOIN scs.UnitOfMeasurement AS subuom ON subuom.Id=adc.UoMId
                                        LEFT JOIN HKP.Party subP ON subp.Id=adc.VendorId
                                        LEFT JOIN hkp.Process AS subpr ON subpr.Id=adc.ProcessId 

                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBRMV1 ON SUBRMV1.Id=ADC.FirstCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBRMV2 ON SUBRMV2.Id=ADC.SecondCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBRMV3 ON SUBRMV3.Id=ADC.ThirdCharacteristicsValueId

                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBRMC1 ON SUBRMC1.Id=SUBRMV1.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBRMC2 ON SUBRMC2.Id=SUBRMV2.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBRMC3 ON SUBRMC3.Id=SUBRMV3.CharacteristicsId


                                        LEFT JOIN AttachmentDetailConsumptionSKUMapping AS adcs ON adcs.AttachmentDetailConsumptionId=adc.Id


                                       LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBSKUV1 ON SUBSKUV1.Id=ADCS.SubFirstCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBSKUV2 ON SUBSKUV2.Id=ADCS.SubSecondCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SUBSKUV3 ON SUBSKUV3.Id=ADCS.SubThirdCharacteristicsValueId

                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBSKUC1 ON SUBSKUC1.Id=SUBSKUV1.CharacteristicsId OR SUBSKUC1.Id=(SELECT TOP 1 RMFirstCharacteristicsId FROM AttachmentDetailConsumptionSKUMapping WHERE AttachmentDetailConsumptionId=adc.Id AND isnull(IsFirstCharacteristicCommon,0)=1)
                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBSKUC2 ON SUBSKUC2.Id=SUBSKUV2.CharacteristicsId OR SUBSKUC1.Id=(SELECT TOP 1 RMSecondCharacteristicsId FROM AttachmentDetailConsumptionSKUMapping WHERE AttachmentDetailConsumptionId=adc.Id AND isnull(IsSecondCharacteristicCommon,0)=1)
                                        LEFT OUTER JOIN [HKP].[Characteristics] SUBSKUC3 ON SUBSKUC3.Id=SUBSKUV3.CharacteristicsId OR SUBSKUC1.Id=(SELECT TOP 1 RMThirdCharacteristicsId FROM AttachmentDetailConsumptionSKUMapping WHERE AttachmentDetailConsumptionId=adc.Id AND isnull(IsThirdCharacteristicCommon,0)=1)



                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV1 ON SKUv1.Id=ADCS.RMFirstCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV2 ON SKUv2.Id=ADCS.RMSecondCharacteristicsValueId
                                        LEFT OUTER JOIN [HKP].[CharacteristicsValue] SKUV3 ON SKUv3.Id=ADCS.RMThirdCharacteristicsValueId

                                        LEFT OUTER JOIN [HKP].[Characteristics] SKUC1 ON SKUC1.Id=SKUV1.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] SKUC2 ON SKUC2.Id=SKUV2.CharacteristicsId
                                        LEFT OUTER JOIN [HKP].[Characteristics] SKUC3 ON SKUC3.Id=SKUV3.CharacteristicsId

                                        WHERE att.MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where ContractId='" + ContractId + @"')
                                        ORDER BY bd.Sequence";

            sheet.Name = "BOM Template Sub Material";

            DataTable dtData = _sqlRepository.GetDataTable(sql);

            if (dtData.Rows.Count == 0)
                return;

            int ROW = 1;
            sheet[ROW, 1].Text = "BOM Template Items (Sub Material)";
            sheet.Range[ROW, 1, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet[ROW, 1].CellStyle.Font.Bold = true;
            ROW++;
            sheet[ROW, 1].Text = "BOM Id";
            sheet[ROW, 3].Text = dtData.Rows[0]["BOMMasterId"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "BOM Desc";
            sheet[ROW, 3].Text = dtData.Rows[0]["BOMDesc"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "Material";
            sheet[ROW, 3].Text = dtData.Rows[0]["FGMaterial"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            sheet[ROW, 1].Text = "Article";
            sheet[ROW, 3].Text = dtData.Rows[0]["FGArticle"].ToString();
            sheet.Range[ROW, 1, ROW, 2].Merge();
            sheet.Range[ROW, 3, ROW, 5].Merge();
            sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW += 2;
            int COL = 1;
            sheet[ROW, COL].Text = "Sl. No";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 6;
            int colSlNo = COL;
            COL++;
            sheet[ROW, COL].Text = "Sub Material";
            sheet[ROW, COL].ColumnWidth = 16;
            int colSubMaterial = COL;
            COL++;
            sheet[ROW, COL].Text = "Sub  Article";
            sheet[ROW, COL].ColumnWidth = 40;
            int colSubArticle = COL;
            COL++;
            sheet[ROW, COL].Text = "Parent Material";
            sheet[ROW, COL].ColumnWidth = 16;
            int colMaterial = COL;
            COL++;
            sheet[ROW, COL].Text = "Parent Article";
            sheet[ROW, COL].ColumnWidth = 40;
            int colArticle = COL;
            COL++;
            sheet[ROW, COL].Text = "RM Desc";
            sheet[ROW, COL].ColumnWidth = 22;
            int colRMDescription = COL;

            COL++;
            sheet[ROW, COL].Text = "Customer Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMCustomerSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMVendorSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU Desc";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUDesc = COL;
            COL++;
            sheet[ROW, COL].Text = "Cons.";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colConsumption = COL;
            COL++;
            sheet[ROW, COL].Text = "UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Wast.%";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colWastagePer = COL;
            COL++;
            sheet[ROW, COL].Text = "Process";
            sheet[ROW, COL].ColumnWidth = 8;
            int colProcess = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor";
            sheet[ROW, COL].ColumnWidth = 20;
            int colVendor = COL;
            COL++;
            sheet[ROW, COL].Text = "Common SKU";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMSKUCommon = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV2 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMV3 = COL;

            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].Merge();
            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].Text = "COMMON SKU";
            sheet.Range[ROW - 1, colRMV1, ROW - 1, colRMV3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colRMV1, ROW, colRMV3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;

            COL++;
            sheet[ROW, COL].Text = "RM SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SUB RM SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV1 = COL;
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].Merge();
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].Text = "RM SKU-1 MAPPING";
            sheet.Range[ROW - 1, colFGV1, ROW - 1, colSKUV1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV1, ROW, colSKUV1].CellStyle.Interior.ColorIndex = ExcelKnownColors.LightGreen;



            COL++;
            sheet[ROW, COL].Text = "RM SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV2 = COL;

            COL++;
            sheet[ROW, COL].Text = "SUB RM SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV2 = COL;

            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].Merge();
            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].Text = "RM SKU-2 MAPPING";
            sheet.Range[ROW - 1, colFGV2, ROW - 1, colSKUV2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV2, ROW, colSKUV2].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;



            COL++;
            sheet[ROW, COL].Text = "RM SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colFGV3 = COL;
            COL++;
            sheet[ROW, COL].Text = "SUB RM SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUV3 = COL;

            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].Merge();
            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].Text = "RM SKU-3 MAPPING";
            sheet.Range[ROW - 1, colFGV3, ROW - 1, colSKUV3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colFGV3, ROW, colSKUV3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_orange;

            sheet.Range[ROW - 1, 1, ROW, COL].CellStyle.Font.Bold = true; //row 19 of heading 
            sheet.Range[ROW - 1, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, colRMV1 - 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            int StartRow = ROW; //row 20
            for (int i = 0; i < dtData.Rows.Count; i++)
            {


                sheet[ROW, colSlNo].Number = (i + 1);
                sheet[ROW, colMaterial].Text = dtData.Rows[i]["RMMaterial"].ToString();
                sheet[ROW, colArticle].Text = dtData.Rows[i]["RMArticle"].ToString();
                sheet[ROW, colSubMaterial].Text = dtData.Rows[i]["SubRMMaterial"].ToString();
                sheet[ROW, colSubArticle].Text = dtData.Rows[i]["SubRMArticle"].ToString();
                sheet[ROW, colRMDescription].Text = dtData.Rows[i]["Description"].ToString();

                sheet[ROW, colRMCustomerSpec].Text = dtData.Rows[i]["CustomerSpec"].ToString();
                sheet[ROW, colRMVendorSpec].Text = dtData.Rows[i]["VendorSpec"].ToString();
                sheet[ROW, colSKUDesc].Text = dtData.Rows[i]["SKUDesc"].ToString();
                sheet[ROW, colVendor].Text = dtData.Rows[i]["Vendor"].ToString();
                sheet[ROW, colUOM].Text = dtData.Rows[i]["UOM"].ToString();
                sheet[ROW, colProcess].Text = dtData.Rows[i]["Process"].ToString();

                sheet[ROW, colRMSKUCommon].Text = dtData.Rows[i]["RMSKUCommon"].ToString();

                sheet[ROW, colRMV1].Text = dtData.Rows[i]["SUBRMV1CHARVAL"].ToString();
                sheet[ROW, colRMV2].Text = dtData.Rows[i]["SUBRMV2CHARVAL"].ToString();
                sheet[ROW, colRMV3].Text = dtData.Rows[i]["SUBRMV3CHARVAL"].ToString();



                sheet[ROW, colFGV1].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsFirstCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["SUBSKU1CHAR"].ToString() + "]" : dtData.Rows[i]["SKUV1CHARVAL"].ToString();
                sheet[ROW, colFGV2].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsSecondCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["SUBSKU2CHAR"].ToString() + "]" : dtData.Rows[i]["SKUV2CHARVAL"].ToString();
                sheet[ROW, colFGV3].Text = bplib.clsWebLib.GetBoolData(dtData.Rows[i]["IsThirdCharacteristicCommon"].ToString()) == true ? "[ALL " + dtData.Rows[i]["SUBSKU3CHAR"].ToString() + "]" : dtData.Rows[i]["SKUV3CHARVAL"].ToString();


                sheet[ROW, colSKUV1].Text = dtData.Rows[i]["SUBSKUV1CHARVAL"].ToString();
                sheet[ROW, colSKUV2].Text = dtData.Rows[i]["SUBSKUV2CHARVAL"].ToString();
                sheet[ROW, colSKUV3].Text = dtData.Rows[i]["SUBSKUV3CHARVAL"].ToString();

                sheet[ROW, colConsumption].Number = clsStaticInfo.dbl(dtData.Rows[i]["Consumption"].ToString());
                sheet[ROW, colWastagePer].Number = clsStaticInfo.dbl(dtData.Rows[i]["WastagePer"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            sheet.IsGridLinesVisible = false;

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

            sheet.Range[8, 4].FreezePanes();

            sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
            sheet.Range[StartRow, colConsumption, ROW, colConsumption].NumberFormat = clsStaticInfo.NumberFormat(4);
            sheet.Range[StartRow, colWastagePer, ROW, colWastagePer].NumberFormat = clsStaticInfo.NumberFormat(2);

        }
        #endregion



        #region Upload Data

        public IWorkbook Download(string Id, string SelectionType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;
            IWorkbook workbook = application.Workbooks.Create(3);
            //Load the existing Excel workbook into IWorkbook

            IWorksheet worksheet = workbook.Worksheets[0];


            string WhereClause = @"       WHERE so.ContractId='" + Id + @"' and isnull(B.Id,'') 
                            NOT IN (select ParentId from BOQ where isnull(ParentId,'')<>'' 
                            AND MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where ContractId='" + Id + @"'))
                      ";
            if (SelectionType.ToUpper() == "ITEM")
            {
                WhereClause = @"       WHERE moi.Id='" + Id + @"' and isnull(B.Id,'') 
                            NOT IN (select ParentId from BOQ where isnull(ParentId,'')<>'' 
                            AND MasterOrderItemId IN(SELECT Id FROM trn.MasterOrderItem Where Id='" + Id + @"'))
                      ";
            }

            string strsql = @"SELECT b.Id,b.MaterialMasterId,b.CurrencyId,b.ArticleId,b.POUoMId,b.BaseUoMId,b.UoMId, b.MasterOrderItemId,moi.MasterOrderId,moi.OwnReferenceNo,moi.BuyerReferenceNo, b.VendorId,b.SalesOrderId,
                                 mm.UserName AS Material,mma.StandardName AS Article,p.UserName AS Vendor,b.SKUDesc,

                                v1.UserName AS CharVal1,v2.UserName AS CharVal2,v3.UserName AS CharVal3,convert(bit,isnull(b.isParent,0)) AS isParent,
                                convert(bit,isnull(b.isChild,0)) AS isChild,PR.UserName AS Process,
                               CONVERT(BIT, isnull(b.RequiredQtyApproved,0)) AS RequiredQtyApproved,CONVERT(BIT, isnull(b.IncompleteMaterial,0)) AS IncompleteMaterial,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
                                b.BOMQty,b.RequiredQty,b.RequiredQtyPO,uom.UserName AS UOM,uomm.UserName AS ParentUOM,
                                POUOM.UserName AS POUOM,b.Rate,c.Code AS Currency,
                                CONCAT(b.RMDescription, CASE WHEN ISNULL(bp.Id,'')<>'' THEN CONCAT(b.RMDescription,'(',mmp.UserName,'-',mmap.StandardName,')') ELSE '' END) AS RMDescription,
                                b.RMCustomerSpec,	b.RMVendorSpec
								,ISNULL(po.POQTY,0) POQTY,ISNULL(grn.GRNQty,0) GRNQty,
								
								 SO1 =STUFF((select distinct ','+xv1.UserName
								               from BOQFGMapping AS XM	 
										       JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
								               JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.FirstCharacteristicsValueId
								             WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO2 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.SecondCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								SO3 =STUFF((select distinct ','+xv1.UserName
										from BOQFGMapping AS XM	 
										JOIN BOQDetail AS XB2 ON xb2.Id=xm.BOQDetailId
										JOIN [HKP].[CharacteristicsValue] XV1 ON xv1.Id=XM.ThirdCharacteristicsValueId
										WHERE XB2.BOQId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								POIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.PurchaseOrderDetail AS XB2 ON xb2.Id=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								GRNIds =STUFF((select distinct ','+XB2.InventoryReceiveId
										from trn.POBOQMAP a	 
										JOIN trn.InventoryReceiveDetail AS XB2 ON xb2.PODetailsId=a.PODetailId
										WHERE a.BOQDetailId=b.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                  FROM BOQ AS b
                                LEFT JOIN boq bp ON bp.ParentId=b.Id
                                LEFT OUTER JOIN MST.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                                LEFT OUTER JOIN MST.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                                
                                LEFT OUTER JOIN MST.MaterialMaster AS mmp ON mmp.Id=bp.MaterialMasterId
                                LEFT OUTER JOIN MST.MaterialMasterArticle AS mmap ON mmap.Id=b.ArticleId

                                LEFT OUTER JOIN SCS.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                                LEFT OUTER JOIN SCS.UnitOfMeasurement AS POuom ON POuom.Id=b.POUoMId
                                LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                                LEFT OUTER JOIN TRN.SalesOrder AS so ON so.Id=b.SalesOrderId
                                LEFT OUTER JOIN TRN.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                                LEFT OUTER JOIN TRN.masterorder MO ON MO.Id=moi.MasterOrderId
                                LEFT OUTER JOIN SCS.UnitOfMeasurement AS uomm ON uomm.Id=mo.TotalQtyUOMId
                                LEFT JOIN HKP.Process AS pr ON pr.Id=b.ProcessId
								LEFT JOIN scs.Currency AS c ON c.Id=b.CurrencyId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId
								Left outer join (	select BOQDetailId,sum(POBOQQty) as POQTY from trn.POBOQMAP  GRoup by BOQDetailId) PO On PO.BOQDetailId = b.Id
								Left outer join (	
								SELECT a.BOQDetailId , sum(b.TransactionQty) GRNQty 
				FROM trn.POBOQMAP a
				INNER JOIN trn.InventoryReceiveDetail b ON a.PODetailId=b.PODetailsId GRoup by BOQDetailId
								
								) GRN On GRN.BOQDetailId = b.Id

                           " + WhereClause + @" 
                        ORDER BY isnull(b.Sequence,0),b.SalesOrderId";
            DataTable dtData = _sqlRepository.GetDataTable(strsql);


            if (dtData.Rows.Count == 0)
                throw new Exception("No data found");

            DataTable dtCurrency = _sqlRepository.GetDataTable(@"  SELECT c.* FROM scs.CurrencyTransaction AS ct
                               JOIN scs.Currency AS c ON c.Id=ct.CurrencyId
                               WHERE ct.CompanyId='" + identity.CompanyId + @"'");

            int ROW = 6; int COL = 1;


            //Master Order Item....................................................................................................
            StringCollection strColSO = new StringCollection();
            IWorksheet sheet = workbook.Worksheets[0];


            int colType = COL; sheet[ROW, COL].Text = "Type"; COL++;
            int colId = COL; sheet[ROW, COL].Text = "Id"; COL++;
            int colMaterialMasterId = COL; sheet[ROW, COL].Text = "MaterialMasterId"; COL++;
            int colArticleId = COL; sheet[ROW, COL].Text = "ArticleId"; COL++;
            int colPOUoMId = COL; sheet[ROW, COL].Text = "POUoMId"; COL++;
            int colBaseUoMId = COL; sheet[ROW, COL].Text = "BaseUoMId"; COL++;
            int colUoMId = COL; sheet[ROW, COL].Text = "UoMId"; COL++;
            int colMasterOrderItemId = COL; sheet[ROW, COL].Text = "MasterOrderItemId"; COL++;
            int colMasterOrderId = COL; sheet[ROW, COL].Text = "MasterOrderId"; COL++;
            int colVendorId = COL; sheet[ROW, COL].Text = "VendorId"; COL++;
            int colCurrencyId = COL; sheet[ROW, COL].Text = "CurrencyId"; COL++;
            sheet.Range[1, 1, 1, COL - 1].ColumnWidth = 0;

            sheet[ROW, COL].Text = "Sl. No";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 6;
            int colSlNo = COL;
            COL++;
            sheet[ROW, COL].Text = "Material";
            sheet[ROW, COL].ColumnWidth = 16;
            int colMaterial = COL;
            COL++;
            sheet[ROW, COL].Text = "Article";
            sheet[ROW, COL].ColumnWidth = 16;
            int colArticle = COL;
            COL++;
            sheet[ROW, COL].Text = "Own Reference No";
            sheet[ROW, COL].ColumnWidth = 10;
            int colOwnReferenceNo = COL;
            COL++;
            sheet[ROW, COL].Text = "Buyer Reference No";
            sheet[ROW, COL].ColumnWidth = 10;
            int colBuyerReferenceNo = COL;
            COL++;
            sheet[ROW, COL].Text = "Sales Order Id";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSalesOrderId = COL;
            COL++;
            sheet[ROW, COL].Text = "RM Desc";
            sheet[ROW, COL].ColumnWidth = 16;
            int colRMDescription = COL;
            COL++;
            sheet[ROW, COL].Text = "Process";
            sheet[ROW, COL].ColumnWidth = 10;
            int colProcess = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharVal1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharVal2 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharVal3 = COL;
            sheet.Range[ROW - 1, colCharVal1].Text = "RM SKU";
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].Merge();
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Green;
            sheet.Range[ROW - 1, colCharVal1, ROW - 1, COL].CellStyle.Font.Color = ExcelKnownColors.White;



            COL++;
            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharValSO1 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharValSO2 = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU3";
            sheet[ROW, COL].ColumnWidth = 10;
            int colCharValSO3 = COL;

            sheet.Range[ROW - 1, colCharValSO1].Text = "FG SKU";
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].Merge();
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW - 1, colCharValSO1, ROW - 1, COL].CellStyle.Font.Color = ExcelKnownColors.White;

            COL++;
            sheet[ROW, COL].Text = "Customer Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMCustomerSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor Spec";
            sheet[ROW, COL].ColumnWidth = 10;
            int colRMVendorSpec = COL;
            COL++;
            sheet[ROW, COL].Text = "SKU Desc";
            sheet[ROW, COL].ColumnWidth = 10;
            int colSKUDesc = COL;
            COL++;
            sheet[ROW, COL].Text = "BOQ";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colBOMQty = COL;
            COL++;
            sheet[ROW, COL].Text = "Booking Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colRequiredQty = COL;
            COL++;
            sheet[ROW, COL].Text = "UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colRequiredQtyInPOUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Rate";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colRate = COL;
            COL++;
            sheet[ROW, COL].Text = "Currency";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet[ROW, COL].ColumnWidth = 9;
            int colCurrency = COL;
            COL++;
            sheet[ROW, COL].Text = "PO Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colPOQty = COL;
            COL++;
            sheet[ROW, COL].Text = "GRN Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colGRNQty = COL;
            COL++;
            sheet[ROW, COL].Text = "Balance To Purchase";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 14;
            int colBalTOProduce = COL;
            COL++;
            sheet[ROW, COL].Text = "Balance To Recieve";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 14;
            int colBalTORecieve = COL;
            COL++;
            sheet[ROW, COL].Text = "PO UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int colPOUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Consumption";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 9;
            int colConsumption = COL;
            COL++;
            sheet[ROW, COL].Text = "Wastage Per";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 8;
            int colWastagePer = COL;
            COL++;
            sheet[ROW, COL].Text = "Vendor";
            sheet[ROW, COL].ColumnWidth = 10;
            int colVendor = COL;
            COL++;
            sheet[ROW, COL].Text = "PO NOs";
            sheet[ROW, COL].ColumnWidth = 12;
            int colPOIds = COL;
            COL++;
            sheet[ROW, COL].Text = "GRN Nos";
            sheet[ROW, COL].ColumnWidth = 12;
            int colGRNIds = COL;


            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true; //row 19 of heading 
            sheet.Range[ROW, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_blue;
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            int endCol = COL;
            ROW++;

            int StartRow = ROW;
            for (int i = 0; i < dtData.Rows.Count; i++)
            {



                sheet[ROW, colType].Text = "DATA";
                sheet[ROW, colId].Text = dtData.Rows[i]["Id"].ToString();
                sheet[ROW, colMaterialMasterId].Text = dtData.Rows[i]["MaterialMasterId"].ToString();
                sheet[ROW, colArticleId].Text = dtData.Rows[i]["ArticleId"].ToString();
                sheet[ROW, colPOUoMId].Text = dtData.Rows[i]["POUoMId"].ToString();
                sheet[ROW, colBaseUoMId].Text = dtData.Rows[i]["BaseUoMId"].ToString();
                sheet[ROW, colUoMId].Text = dtData.Rows[i]["UoMId"].ToString();
                sheet[ROW, colMasterOrderItemId].Text = dtData.Rows[i]["MasterOrderItemId"].ToString();
                sheet[ROW, colMasterOrderId].Text = dtData.Rows[i]["MasterOrderId"].ToString();
                sheet[ROW, colOwnReferenceNo].Text = dtData.Rows[i]["OwnReferenceNo"].ToString();
                sheet[ROW, colBuyerReferenceNo].Text = dtData.Rows[i]["BuyerReferenceNo"].ToString();
                sheet[ROW, colSalesOrderId].Text = dtData.Rows[i]["SalesOrderId"].ToString();

                sheet[ROW, colVendorId].Text = dtData.Rows[i]["VendorId"].ToString();
                sheet[ROW, colCurrencyId].Text = dtData.Rows[i]["CurrencyId"].ToString();


                sheet[ROW, colSlNo].Number = (i + 1);
                sheet[ROW, colMaterial].Text = dtData.Rows[i]["Material"].ToString();
                sheet[ROW, colArticle].Text = dtData.Rows[i]["Article"].ToString();
                sheet[ROW, colRMDescription].Text = dtData.Rows[i]["RMDescription"].ToString();

                sheet[ROW, colCharVal1].Text = dtData.Rows[i]["CharVal1"].ToString();
                sheet[ROW, colCharVal2].Text = dtData.Rows[i]["CharVal2"].ToString();
                sheet[ROW, colCharVal3].Text = dtData.Rows[i]["CharVal3"].ToString();

                sheet[ROW, colCharValSO1].Text = dtData.Rows[i]["SO1"].ToString();
                sheet[ROW, colCharValSO2].Text = dtData.Rows[i]["SO2"].ToString();
                sheet[ROW, colCharValSO3].Text = dtData.Rows[i]["SO3"].ToString();

                sheet[ROW, colRMCustomerSpec].Text = dtData.Rows[i]["RMCustomerSpec"].ToString();
                sheet[ROW, colRMVendorSpec].Text = dtData.Rows[i]["RMVendorSpec"].ToString();
                sheet[ROW, colSKUDesc].Text = dtData.Rows[i]["SKUDesc"].ToString();
                sheet[ROW, colVendor].Text = dtData.Rows[i]["Vendor"].ToString();
                sheet[ROW, colUOM].Text = dtData.Rows[i]["UOM"].ToString();
                sheet[ROW, colPOUOM].Text = dtData.Rows[i]["POUOM"].ToString();
                sheet[ROW, colProcess].Text = dtData.Rows[i]["Process"].ToString();
                sheet[ROW, colPOIds].Text = dtData.Rows[i]["POIds"].ToString();
                sheet[ROW, colGRNIds].Text = dtData.Rows[i]["GRNIds"].ToString();


                sheet[ROW, colBOMQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["BOMQty"].ToString());
                sheet[ROW, colRequiredQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["RequiredQty"].ToString());
                sheet[ROW, colPOQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["POQTY"].ToString());
                sheet[ROW, colGRNQty].Number = clsStaticInfo.dbl(dtData.Rows[i]["GRNQty"].ToString());
                sheet[ROW, colRequiredQtyInPOUOM].Number = clsStaticInfo.dbl(dtData.Rows[i]["RequiredQtyPO"].ToString());
                sheet[ROW, colBalTOProduce].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colRequiredQtyInPOUOM) + ROW.ToString() + " - " + clsStaticInfo.GetxlsCol(colPOQty) + ROW.ToString() + ")";
                sheet[ROW, colBalTORecieve].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colPOQty) + ROW.ToString() + " - " + clsStaticInfo.GetxlsCol(colGRNQty) + ROW.ToString() + ")";

                sheet[ROW, colConsumption].Number = clsStaticInfo.dbl(dtData.Rows[i]["Consumption"].ToString());
                sheet[ROW, colWastagePer].Number = clsStaticInfo.dbl(dtData.Rows[i]["WastagePer"].ToString());



                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[ROW, colRequiredQtyInPOUOM, ROW, colCurrency].CellStyle.Locked = false;


                sheet.Range[ROW, colRequiredQtyInPOUOM].CellStyle.Interior.ColorIndex = ExcelKnownColors.White;
                sheet.Range[ROW, colRate].CellStyle.Interior.ColorIndex = ExcelKnownColors.White;
                sheet.Range[ROW, colCurrency].CellStyle.Interior.ColorIndex = ExcelKnownColors.White;


                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }


            #region Currency
            if (dtCurrency.Rows.Count > 0)
            {
                IWorksheet sheetShade = workbook.Worksheets[1];
                int STOROW = 1;
                for (int i = 0; i < dtCurrency.Rows.Count; i++)
                {
                    sheetShade[STOROW, 1].Text = dtCurrency.Rows[i]["Code"].ToString();
                    STOROW++;
                }

                STOROW--;
                IName name1 = workbook.Names.Add("Currency");
                name1.RefersToRange = sheet.Range[StartRow, colCurrency, ROW - 1, colCurrency];
                name1.Value = "Sheet2!$A$1:$A$" + STOROW.ToString();

                sheet.Range[StartRow, colCurrency, ROW - 1, colCurrency].DataValidation.IsEmptyCellAllowed = true;
                sheet.Range[StartRow, colCurrency, ROW - 1, colCurrency].DataValidation.AllowType = ExcelDataType.User;
                sheet.Range[StartRow, colCurrency, ROW - 1, colCurrency].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                sheet.Range[StartRow, colCurrency, ROW - 1, colCurrency].DataValidation.FirstDateTime = new DateTime(1, 1, 1, 0, 0, 0);
                sheet.Range[StartRow, colCurrency, ROW - 1, colCurrency].DataValidation.FirstFormula = "Currency";
            }

            #endregion Currency

            #region Validations
            sheet.Range[StartRow, colRequiredQtyInPOUOM, ROW - 1, colRequiredQtyInPOUOM].DataValidation.IsEmptyCellAllowed = true;
            sheet.Range[StartRow, colRequiredQtyInPOUOM, ROW - 1, colRequiredQtyInPOUOM].DataValidation.AllowType = ExcelDataType.Decimal;
            sheet.Range[StartRow, colRequiredQtyInPOUOM, ROW - 1, colRequiredQtyInPOUOM].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
            sheet.Range[StartRow, colRequiredQtyInPOUOM, ROW - 1, colRequiredQtyInPOUOM].DataValidation.FirstFormula = "0";
            sheet.Range[StartRow, colRequiredQtyInPOUOM, ROW - 1, colRequiredQtyInPOUOM].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
            sheet.Range[StartRow, colRequiredQtyInPOUOM, ROW - 1, colRequiredQtyInPOUOM].DataValidation.ErrorBoxText = "Only positive numbers are allowed for Quantity";
            sheet.Range[StartRow, colRequiredQtyInPOUOM, ROW - 1, colRequiredQtyInPOUOM].DataValidation.ErrorBoxTitle = "Number Error";


            sheet.Range[StartRow, colRate, ROW - 1, colRate].DataValidation.IsEmptyCellAllowed = true;
            sheet.Range[StartRow, colRate, ROW - 1, colRate].DataValidation.AllowType = ExcelDataType.Decimal;
            sheet.Range[StartRow, colRate, ROW - 1, colRate].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
            sheet.Range[StartRow, colRate, ROW - 1, colRate].DataValidation.FirstFormula = "0";
            sheet.Range[StartRow, colRate, ROW - 1, colRate].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
            sheet.Range[StartRow, colRate, ROW - 1, colRate].DataValidation.ErrorBoxText = "Only positive numbers are allowed for Rate";
            sheet.Range[StartRow, colRate, ROW - 1, colRate].DataValidation.ErrorBoxTitle = "Number Error";
            #endregion Validations



            sheet.Range[StartRow, 1, ROW, endCol].WrapText = true;


            sheet.AutoFilters.FilterRange = sheet.Range[StartRow - 1, 1, ROW, endCol];

            for (int C = 0; C < endCol; C++)
            {
                IAutoFilter filter = sheet.AutoFilters[C];
            }


            sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
            sheet.Range[StartRow, colBOMQty, ROW, colBOMQty].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[StartRow, colRequiredQty, ROW, colRequiredQty].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[StartRow, colConsumption, ROW, colConsumption].NumberFormat = clsStaticInfo.NumberFormat(4);
            sheet.Range[StartRow, colWastagePer, ROW, colWastagePer].NumberFormat = clsStaticInfo.NumberFormat(2);



            sheet.Protect(bplib.clsWebLib.REPORT_LOCK_PASSWORD, ExcelSheetProtection.Filtering | ExcelSheetProtection.All);
            workbook.Worksheets[1].Protect(bplib.clsWebLib.REPORT_LOCK_PASSWORD);
            workbook.Protect(false, true, bplib.clsWebLib.REPORT_LOCK_PASSWORD);

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;
            worksheet.UsedRange.WrapText = true;


            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, SelectionType.ToLower() + "#" + Id, identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;
            return workbook;

        }

        public void UploadData(System.Web.HttpPostedFileBase file)
        {
            try
            {
                SaveFiles(file, out string FilePath);
                ReadFile(FilePath, out DataTable dtData);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //validations
                //currency:
                DataTable dtCurrency = _sqlRepository.GetDataTable(@"  SELECT c.* FROM scs.CurrencyTransaction AS ct
                               JOIN scs.Currency AS c ON c.Id=ct.CurrencyId
                               WHERE ct.CompanyId='" + identity.CompanyId + @"'");


                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    OTSBD.clsStaticInfo.numericValidation(dtData.Rows[i]["Qty"].ToString(), false, false, false, "Quantity");
                    OTSBD.clsStaticInfo.numericValidation(dtData.Rows[i]["Rate"].ToString(), false, false, false, "Rate");


                    if (clsStaticInfo.dbl(dtData.Rows[i]["Rate"].ToString()) > 0)
                    {
                        if (string.IsNullOrEmpty(dtData.Rows[i]["Currency"].ToString()) == true)
                        {
                            throw new Exception("Rate has been provided without currency");
                        }
                    }

                    dtCurrency.DefaultView.RowFilter = "Code='" + dtData.Rows[i]["Currency"].ToString().Trim() + "'";
                    if (string.IsNullOrEmpty(dtData.Rows[i]["Currency"].ToString()) == false)
                    {
                        if (dtCurrency.DefaultView.Count > 0)
                            dtData.Rows[i]["CurrencyId"] = dtCurrency.DefaultView[0]["Id"].ToString();
                        else
                            throw new Exception("Provided currency does not exists in the system for selected company [Provided Currency: " + dtData.Rows[i]["Currency"].ToString().Trim() + @"]");
                    }

                }




                //update qty
                Library.General.Conversions.UOMConversion uom = new General.Conversions.UOMConversion();
                ConnectionManager.clsConnectionManager ConManager = new ConnectionManager.clsConnectionManager(600);
                ConManager.BeginTransaction();

                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    string sql = @"UPDATE BOQ SET Rate = " + clsStaticInfo.dbl(dtData.Rows[i]["Rate"].ToString()) + @" ,CurrencyId =" + UpdateString(dtData.Rows[i]["CurrencyId"]) + @"
                            ,RequiredQtyPO = " + clsStaticInfo.dbl(dtData.Rows[i]["Qty"].ToString()) + @"
                            ,RequiredQty = " + uom.Convert(dtData.Rows[i]["MaterialMasterId"].ToString(), dtData.Rows[i]["POUoMId"].ToString(), dtData.Rows[i]["UoMId"].ToString(), clsStaticInfo.dbl(dtData.Rows[i]["Qty"].ToString())).ToString("F4") + @"
                            ,RequiredQtyBase = " + uom.Convert(dtData.Rows[i]["MaterialMasterId"].ToString(), dtData.Rows[i]["POUoMId"].ToString(), dtData.Rows[i]["BaseUoMId"].ToString(), clsStaticInfo.dbl(dtData.Rows[i]["Qty"].ToString())).ToString("F4")
                     + " WHERE Id='" + dtData.Rows[i]["Id"].ToString() + "'";


                    ConManager.executeQuery(sql);

                }

                ConManager.CommitTransaction();


            }
            catch (Exception ex)
            {
                throw ex;

            }


        }



        public void SaveFiles(System.Web.HttpPostedFileBase file, out string path)
        {
            path = "";
            try
            {
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {
                    }
                    else
                        throw new Exception("Required excel file");
                }
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void ReadFile(string path, out DataTable dtExcel)
        {
            FileInfo docFile;
            dtExcel = null;
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = excelEngine.Excel.Workbooks.Open(path);

                workbook.Worksheets[0].UsedRange.ColumnWidth = 10;
                //DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                dtExcel = workbook.Worksheets[0].ExportDataTable(6, 1, 10000, workbook.Worksheets[0].UsedRange.Columns.Length, ExcelExportDataTableOptions.ColumnNames);
                dtExcel.DefaultView.RowFilter = "isnull(Type,'')='DATA'";
                dtExcel = dtExcel.DefaultView.ToTable();

                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
            }
            catch (Exception ex)
            {
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
                throw (ex);
            }
        }
        private string UpdateString(object FieldValue)
        {
            if (FieldValue == null)
                return "Null";

            if (string.IsNullOrEmpty(FieldValue.ToString()))
                return "Null";

            if (FieldValue.ToString().ToLower() == "null")
                return "NULL";

            return "'" + FieldValue + "'";
        }
        #endregion Upload Data

    }
}
