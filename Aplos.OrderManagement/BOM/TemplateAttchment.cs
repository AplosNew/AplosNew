using bplib;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Payrolls.SalaryProcessActive;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.OrderManagement.BOM
{
    public class TemplateAttchment
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        CustomIdentity identity;
        #region Constructor
        public TemplateAttchment()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
            identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        }
        #endregion Constructor

        public List<Dictionary<string, object>> LoadAllTemplate(bool Assigned, string column, string value)
        {


            string strkey = "";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            string AdditionalWhereClause = " AND isnull(BOMMasterId,'')='' ";
            if (Assigned == true)
            {
                AdditionalWhereClause = " AND isnull(BOMMasterId,'')<>'' ";
            }

            AdditionalWhereClause += " AND " + strkey;

            string sql = @"SELECT top 100 * FROM (SELECT  mai.BOMMasterId,BOM.[Description] AS BomDesc,bmm.UserName AS BOMMaterial,bma.StandardName AS BOMArticle,
       moi.Id AS MasterOrderItemId, moi.MasterOrderId, moi.BuyerReferenceNo BuyerItemNo, moi.OwnReferenceNo OwnItemNo,mo.BuyerReferenceNo AS BuyerOrderNo,mo.OwnReferenceNo AS OwnOrderNo,
       moi.MaterialMasterId, moi.ArticleId,pm.UserName AS Product,pc.UserName AS ProductCategory,mm.UserName AS Material,ma.StandardName AS Article,
       b.UserName AS Buyer,p.UserName AS Customer,c.ContractNo,ml.LCRef,MOI.AddedBy,Format(MOI.AddedDate,'dd-MMM-yyyy') AS AddedDate,
       convert(bit,CASE WHEN  ISNULL((SELECT COUNT(*) AS SalesOrderCount FROM trn.SalesOrder WHERE MasterOrderItemId=moi.Id),0)<>ISNULL((SELECT count(DISTINCT SalesOrderId) AS SalesOrderCount FROM BOQDetail WHERE MasterOrderItemId=moi.Id),0) THEN 0 ELSE 1 END) AS HasBOQ
        ,MO.Type,isnull(moi.Consignment,0) AS Consignment,
        CASE WHEN isnull(moi.Consignment,0)=1 THEN
	        CASE WHEN ISNULL(eout.Id,'')<>'' THEN CONCAT(POUT.UserName,'(',EOUT.UserName,')') ELSE TOUT.UserName END
        ELSE
	     CONCAT(POWN.UserName,'(',EOWN.UserName,')') END AS PurchaseAuthority
        --convert(bit,case when isnull(BOQ.Id,'')='' then 0 else 1 end) AS HasBOQ
                              FROM trn.MasterOrder MO
                            join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
                            LEFT JOIN BOMMasterAttachmentWithItem MAI ON mai.MasterOrderItemId=moi.Id
                            left join BOQ on BOQ.MasterOrderItemId=MAI.MasterOrderItemId and BOQ.Id=(select top 1 Id from BOQ where MasterOrderItemId=MAI.MasterOrderItemId)
                            LEFT JOIN BOMMaster AS BOM ON BOM.Id=mai.BOMMasterId
                            left outer join mst.MaterialMaster bmm on bmm.id=BOM.FGMaterialMasterId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] bMA ON bma.Id=BOM.FGArticleId
                            
                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
							 --LEFT OUTER JOIN hkp.ProductGroup AS pg ON pg.Id=moi.pro
                            LEFT JOIN [Contract] AS c ON c.Id=moi.ContractId
                            LEFT JOIN MasterLC AS ml ON ml.Id=c.MasterLCId
                           
                            left outer join [HKP].[Party] p on P.Id=MO.PartyId
                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
							LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=MO.TotalQtyUOMId
							LEFT OUTER JOIN scs.Currency AS cur ON cur.Id=mo.CurrencyId 

							LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
							LEFT JOIN org.Plant AS POUT ON POUT.Id=EOUT.PlantId
							LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
							LEFT JOIN org.Plant AS POWN ON POWN.Id=MO.PlantId
							LEFT JOIN org.Entity AS EOWN ON EOWN.Id=MO.EntityId

                            where MO.PlantId='" + identity.PlantId + "') AS K where 1=1 " + AdditionalWhereClause + " Order By MasterOrderId";

            return _sqlRepository.GetDataCollection(sql);

        }
        public List<Dictionary<string, object>> SearchMasterOrder(string column, string value)
        {


            string strkey = "";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;



            string sql = @"SELECT top 100 * FROM (SELECT A.Id, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId
                                    , A.OrderType, A.PartyId, P.UserName AS CustomerName, A.BuyerId,B.UserName Buyer
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId,OC.UserName AS OrderCategory, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' AND PlantId=A.PlantId)
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage,A.BuyerDepartmentId
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.IsExtraOrderPercentage,PM.UserName ProductMaster,OS.UserName OrderStatus
                                      ,A.OwnReferenceNo,A.BuyerReferenceNo
                                    ,[BuyerItem]=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     [OwnItem]=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                    ContractNo=STUFF((select distinct ','+CNT.ContractNo from dbo.Contract CNT
															INNER JOIN trn.MasterOrderItem XMOI  ON XMOI.ContractId=CNT.Id	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
									MasterLCNo=STUFF((select distinct ','+MLC.LCRef from dbo.Contract CNT
															INNER JOIN TRN.MasterOrderItem XMOI  ON XMOI.ContractId=CNT.Id
															LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            FROM [TRN].[MasterOrder] AS A
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN TRN.Commitment COM ON COM.Id=A.CommitmentId
							LEFT JOIN [MST].[ProductMaster] PM ON COM.ProductMasterId=PM.Id
                            LEFT JOIN HKP.OrderStatus OS ON OS.Id=A.OrderStatusId
                            LEFT JOIN hkp.OrderCategory AS oc ON oc.Id=a.OrderCategoryId
                            LEFT JOIN HKP.Buyer B ON B.Id=A.BuyerId
                            WHERE A.PlantId='" + identity.PlantId + "' AND OrderType='ExternalOrder' AND A.OrderStatusId='Active') AS K where " + strkey + " Order By Id";

            return _sqlRepository.GetDataCollection(sql);

        }
        public List<Dictionary<string, object>> SearchProductionOrder(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (
                        SELECT  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
                        ISNULL(PO.Qty,0) AS POQuantity,ISNULL(PO.PlannedQty,0) AS SOQuantity,ISNULL(SO.Qty,0) AS SavedQuantity,t1.Color,FORMAT(t1.LSD,'dd-MMM-yyyy') AS LSD,FORMAT(t1.CommitmentDate,'dd-MMM-yyyy') AS CommitmentDate,t1.TargetPerDay
                                ,T1.ProductionPriority ,so.Material, so.Product,t1.Qty,
                                so.ProductCategory, so.FirstShipmentDate,
                                so.LastShipmentDate, so.buyer, so.BuyerRefNo,
                                so.OwnRefNo, so.StyleNo, so.OwnStyleNo, so.SONo,
                                so.SODesc,So.MasterOrderId,
                                so.Customer,so.article,PRODPR.ProductionQtyAtPR 
                                   ,ISNULL(CASE WHEN ISNULL(T1.Qty,0)>0 THEN T1.Qty ELSE PO.PlannedQty END,0)-(ISNULL(PRODPR.ProductionQtyAtPR,0)-ISNULL(PRDQ.ProductionBookedQty,0)) AS ToBePlanQty
                                  			
  
                            FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            INNER JOIN ProductionOrderSchedulingParametersType1 t1 ON t1.ProductionOrderID=po.Id

                             LEFT OUTER JOIN (
												SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
											FROM  trn.ProductionSummary S 
											--WHERE  CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PRODPR ON  PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
							 left outer join (SELECT pod.ProductionOrderId,
                                sum(isnull(so.ProductionBookedQty,0)) ProductionBookedQty
                                 FROM trn.SalesOrder AS so
                                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id

                                GROUP BY pod.ProductionOrderId
                            ) AS PRDQ ON PRDQ.ProductionOrderId=T1.ProductionOrderId
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,ma.StandardName AS Article, PM.UserName AS Product,pc.UserName AS ProductCategory,
                                                     min(so.DeliveryDate) AS FirstShipmentDate,  max(so.DeliveryDate) AS LastShipmentDate,
                                                    sum(so.Qty) AS Qty,
                                                    MasterOrderId =STUFF((select distinct ','+XMOI.Id from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
                                                    BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                      from 
 
 
                                                     trn.SalesOrder SO 
                                                      JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                                    left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                    left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                    left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
													LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                    group by pod.ProductionOrderId,mm.userName,ma.StandardName,PM.UserName,pc.UserName) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                           Where PO.PlantId='" + identity.PlantId + @"'  ) AS TEMP WHERE " + strkey + " ORDER BY ProductionPriority";

            return _sqlRepository.GetDataCollection(sql, null);
        }
        public List<Dictionary<string, object>> GetSalesOrderList(string Id, string FLAG)
        {
            string SOWhereClause = "";
            if (FLAG == "MASTERORDER")
            {
                SOWhereClause = @"SELECT so.Id FROM trn.MasterOrderItem AS moi
                        INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                        WHERE moi.MasterOrderId='" + Id + @"'";
            }
            else
            {
                SOWhereClause = @"SELECT pod.SalesOrderId FROM trn.ProductionOrderDetail AS pod WHERE pod.ProductionOrderId='" + Id + @"'";
            }

            try
            {
                var sql = @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,Convert(bit,0) AS Checked
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId
	                            , SO.Id, SO.Id AS SalesOrderId, P.UserName AS Customer
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , DEST.UserName AS DestinationName, SHP.UserName AS ShipmentModeName
	                            , PO.PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description
	                            , Flag = CAST(0 AS BIT)
                       FROM [TRN].[SalesOrder] AS SO 
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                       WHERE SO.Id IN (" + SOWhereClause + @") ORDER BY  MOI.MaterialMasterId,MOI.ArticleId";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)

            {
                throw new Exception(ex.Message);
            }
        }
        public List<Dictionary<string, object>> GetBOMItemListForReport(string SalesOrderIds)
        {

            string sql = @"SELECT DISTINCT * FROM (SELECT DENSE_RANK() OVER (PARTITION BY b.MaterialMasterId,b.ArticleId ORDER BY b.Sequence) Sequence, convert(bit,0) AS Checked, b.MaterialMasterId,b.ArticleId, b.MasterOrderItemId,
                          concat(b.MaterialMasterId,b.ArticleId, b.VendorId) AS Id, b.VendorId,p.UserName AS Vendor,mgm.UserName AS MaterialGroup,
                                mm.UserName AS Material,mma.StandardName AS Article,b.Rate,b.CurrencyId,c.Code AS Currency

                                FROM BOQ AS b
                            LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                            LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                            LEFT OUTER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=mm.MaterialGroupMasterId
                            LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                            LEFT JOIN scs.Currency AS c ON c.Id=b.CurrencyId
                            WHERE b.SalesOrderId IN (" + SalesOrderIds + @") and isnull(B.isParent,0)=0
                             ) AS K WHERE K.Sequence=1";
            return _sqlRepository.GetDataCollection(sql, null);
        }
        public List<Dictionary<string, object>> GetBOMItemListForReportByMasterOrderItemId(string MasterOrderItemId)
        {

            string sql = @"SELECT DISTINCT * FROM (SELECT DENSE_RANK() OVER (PARTITION BY b.MaterialMasterId,b.ArticleId ORDER BY b.Sequence) Sequence, convert(bit,0) AS Checked, b.MaterialMasterId,b.ArticleId, b.MasterOrderItemId,
                          concat(b.MaterialMasterId,b.ArticleId, b.VendorId) AS Id, b.VendorId,p.UserName AS Vendor,mgm.UserName AS MaterialGroup,
                                mm.UserName AS Material,mma.StandardName AS Article,b.Rate,b.CurrencyId,c.Code AS Currency

                                FROM BOQ AS b
                            LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                            LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                            LEFT OUTER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=mm.MaterialGroupMasterId
                            LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                            LEFT JOIN scs.Currency AS c ON c.Id=b.CurrencyId
                            WHERE b.MasterOrderItemId ='" + MasterOrderItemId + @"' and isnull(B.isParent,0)=0
                             ) AS K WHERE K.Sequence=1";
            return _sqlRepository.GetDataCollection(sql, null);
        }



        public List<Dictionary<string, object>> GetBOMList(string column, string value, string ArticleId, bool loadAll)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                if (string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

            string FilterByArticle = "";
            if (loadAll == false)
            {
                FilterByArticle = " WHERE BOM.FGArticleId='" + ArticleId + "'";
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (SELECT  BOM.Id,BOM.[Description] AS BomDesc,bmm.UserName AS BOMMaterial,bma.StandardName AS BOMArticle,BOM.AddedBy,Format(BOM.AddedDate,'dd-MMM-yyyy') AS AddedDate
                            FROM  BOMMaster AS BOM 
                            left outer join mst.MaterialMaster bmm on bmm.id=BOM.FGMaterialMasterId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] bMA ON bma.Id=BOM.FGArticleId " + FilterByArticle + ") AS TEMP WHERE " + strkey + "";



            return _sqlRepository.GetDataCollection(sql, null);
        }
        public List<Dictionary<string, object>> GetBOMItemList(string MasterOrderItemId, bool isParent)
        {
            string parent = isParent == true ? "1" : "0";
            string sql = @"SELECT b.Id,b.MaterialMasterId,b.ArticleId, b.MasterOrderItemId,moi.MasterOrderId,moi.OwnReferenceNo,moi.BuyerReferenceNo, b.VendorId,b.SalesOrderId,
                                 mm.UserName AS Material,mma.StandardName AS Article,p.UserName AS Vendor,b.SKUDesc,
                                concat(mmp.UserName,' ',BOQP.RMDescription) AS ParentMaterial,mmap.StandardName AS ParentArticle,
                                v1.UserName AS CharVal1,v2.UserName AS CharVal2,v3.UserName AS CharVal3,convert(bit,isnull(b.isParent,0)) AS isParent,
                                convert(bit,isnull(b.isChild,0)) AS isChild,b.UoMId, b.BaseUoMId, b.POUoMId,
                               CONVERT(BIT, isnull(b.RequiredQtyApproved,0)) AS RequiredQtyApproved,CONVERT(BIT, isnull(b.IncompleteMaterial,0)) AS IncompleteMaterial,
                                isnull(BOQP.OrderQty,b.OrderQty) AS OrderQty,isnull(BOQP.PlanOrderQty,b.PlanOrderQty) AS PlanOrderQty,b.Consumption,b.WastagePer,
                                b.BOMQty,b.RequiredQty,b.RequiredQtyPO,uom.UserName AS UOM,uomp.UserName AS POUOM,uomm.UserName AS ParentUOM,
                                b.RMDescription,	b.RMCustomerSpec,	b.RMVendorSpec,CUR.Code AS Currency,B.Rate

                                  FROM BOQ AS b
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uomP ON uomP.Id=b.POUoMId
                                LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                                LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
                                LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                                LEFT OUTER JOIN trn.masterorder MO ON MO.Id=moi.MasterOrderId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uomm ON uomm.Id=mo.TotalQtyUOMId
                                LEFT OUTER JOIN BOQ AS BOQP ON BOQP.Id=B.ParentId
                                LEFT OUTER JOIN mst.MaterialMaster AS mmp ON mmp.Id=BOQP.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mmap ON mmap.Id=BOQP.ArticleId
                                LEFT OUTER JOIN scs.Currency CUR ON CUR.Id=B.CurrencyId

                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

                                WHERE b.MasterOrderItemId='" + MasterOrderItemId + @"' and isnull(B.isParent,0)=" + parent + @"
                                ORDER BY isnull(BOQP.Sequence,0),isnull(b.Sequence,0),b.SalesOrderId";


            return _sqlRepository.GetDataCollection(sql, null);
        }

        public List<Dictionary<string, object>> GetBOMItemListWithRate(string MasterOrderItemId)
        {

            string sql = @"select ROW_NUMBER() OVER (ORDER BY K.MaterialMasterId) AS RowId,k.*,CASE WHEN TotalMaterial=TotalApproved THEN 'FULL' ELSE CASE WHEN TotalApproved>0 AND  TotalMaterial<>TotalApproved THEN 'PARTIAL' ELSE 'NONE' END END AS RequiredQtyApprovedFlag

                                    from (SELECT b.MaterialMasterId,b.ArticleId, b.MasterOrderItemId,b.VendorId,p.UserName AS Vendor,b.POUoMId,
                                mm.UserName AS Material,mma.StandardName AS Article,b.Rate,b.CurrencyId,c.Code AS Currency,uomp.UserName AS POUOM
                                ,sum(b.RequiredQtyPO) AS RequiredQtyPO,
                                COUNT(*) AS TotalMaterial,SUM(CASE WHEN b.RequiredQtyApproved=1 THEN 1 ELSE 0 END) AS TotalApproved,
                                case when SUM(case when isnull(IsMainMaterial,0)=1 THEN 1 else 0 END)>0 THEN convert(bit,1) ELSE convert(bit,0) END AS IsMainMaterial

                                FROM BOQ AS b
                            LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                            LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                            LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                            LEFT OUTER JOIN scs.UnitOfMeasurement AS uomP ON uomP.Id=b.POUoMId
                            LEFT JOIN scs.Currency AS c ON c.Id=b.CurrencyId
                            WHERE b.MasterOrderItemId='" + MasterOrderItemId + @"' and isnull(B.isParent,0)=0
                                group by  b.MaterialMasterId,b.ArticleId, b.MasterOrderItemId,b.VendorId,p.UserName,b.POUoMId,
                                mm.UserName,mma.StandardName,b.Rate,b.CurrencyId,c.Code,uomp.UserName
                            ) AS K
                            ORDER BY K.Material";
            return _sqlRepository.GetDataCollection(sql, null);
        }
        public List<Dictionary<string, object>> GetBOMItemListChild(string MasterOrderItemId, string ParentId)
        {
            string sql = @"SELECT b.Id, b.MasterOrderItemId,moi.MasterOrderId,moi.OwnReferenceNo,moi.BuyerReferenceNo, b.VendorId,b.SalesOrderId,
                                 mm.UserName AS Material,mma.StandardName AS Article,p.UserName AS Vendor,b.SKUDesc,
                                v1.UserName AS CharVal1,v2.UserName AS CharVal2,v3.UserName AS CharVal3,convert(bit,isnull(b.isParent,0)) AS isParent,
                                convert(bit,isnull(b.isChild,0)) AS isChild,
                               CONVERT(BIT, isnull(b.RequiredQtyApproved,0)) AS RequiredQtyApproved,CONVERT(BIT, isnull(b.IncompleteMaterial,0)) AS IncompleteMaterial,b.OrderQty,b.PlanOrderQty,b.Consumption,b.WastagePer,
                                b.BOMQty,b.RequiredQty,uom.UserName AS UOM,uomm.UserName AS ParentUOM,
                                b.RMDescription,	b.RMCustomerSpec,	b.RMVendorSpec

                                  FROM BOQ AS b
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
                                LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
                                LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
                                LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
                                LEFT OUTER JOIN trn.masterorder MO ON MO.Id=moi.MasterOrderId
                                LEFT OUTER JOIN scs.UnitOfMeasurement AS uomm ON uomm.Id=mo.TotalQtyUOMId

                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
                                LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

                                WHERE b.MasterOrderItemId='" + MasterOrderItemId + @"' and B.Id=(select ParentId from BOQ where Id='" + ParentId + @"')
                                ORDER BY b.Sequence,b.SalesOrderId";


            return _sqlRepository.GetDataCollection(sql, null);
        }
        public void UpdateBomRequiredQty(List<Dictionary<string, object>> data)
        {

            try
            {
                Library.General.Conversions.UOMConversion uom = new General.Conversions.UOMConversion();


                ConManager.BeginTransaction();

                for (int i = 0; i < data.Count; i++)
                {

                    ConManager.executeQuery(@"UPDATE BOQ SET 
                            RequiredQtyPO = " + clsStaticInfo.dbl(data[i]["RequiredQtyPO"].ToString()) + @"
                            ,RequiredQty = " + uom.Convert(data[i]["MaterialMasterId"].ToString(), data[i]["POUoMId"].ToString(), data[i]["UoMId"].ToString(), clsStaticInfo.dbl(data[i]["RequiredQtyPO"].ToString())).ToString("F4") + @"
                            ,RequiredQtyBase = " + uom.Convert(data[i]["MaterialMasterId"].ToString(), data[i]["POUoMId"].ToString(), data[i]["BaseUoMId"].ToString(), clsStaticInfo.dbl(data[i]["RequiredQtyPO"].ToString())).ToString("F4")
                           + " WHERE Id=" + UpdateString(data[i]["Id"]) + "");
                }

                ConManager.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public void UpdateBomRequiredQtyRate(List<Dictionary<string, object>> data)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                for (int i = 0; i < data.Count; i++)
                {
                    try
                    {
                        strSQL = @"UPDATE BOQ SET Rate = " + clsStaticInfo.dbl(data[i]["Rate"].ToString()) + @" ,CurrencyId =" + UpdateString(data[i]["CurrencyId"])
                                                   + @" WHERE POUoMId='" + data[i]["POUoMId"] + "' AND ArticleId='" + data[i]["ArticleId"] + "' AND MasterOrderItemId='" + data[i]["MasterOrderItemId"] + @"'";
                        objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                    }
                    catch (Exception ex)
                    {


                    }

                    try
                    {
                        strSQL = @"UPDATE BOQ SET VendorId = " + UpdateString(data[i]["VendorId"]) + " WHERE POUoMId='" + data[i]["POUoMId"] + "' AND ArticleId='" + data[i]["ArticleId"] + "' AND MasterOrderItemId='" + data[i]["MasterOrderItemId"] + @"'";
                        objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                    }
                    catch (Exception ex)
                    {


                    }

                }



                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
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

        private string UpdateStringEqual(object FieldValue)
        {
            if (FieldValue == null)
                return "''";

            if (string.IsNullOrEmpty(FieldValue.ToString()))
                return "''";

            if (FieldValue.ToString().ToLower() == "null")
                return "''";

            return "'" + FieldValue + "'";
        }

        public List<Dictionary<string, object>> GetAttachedBOMWithAttachment(string BOMMasterAttachmentWithItemId)
        {

            string sql = @"SELECT bi.Id,b.FGMaterialMasterId, b.FGArticleId, b.[Description]
                            FROM BOMMasterAttachmentWithItem AS BI
                            INNER JOIN BOMMaster AS b ON b.Id=bi.BOMMasterId
                            WHERE bi.Id='" + BOMMasterAttachmentWithItemId + @"'";



            return _sqlRepository.GetDataCollection(sql, null);
        }

        public void saveAttachment(Dictionary<string, object> Data)
        {
            try
            {

                DataSet dsMaster, dsDetail, dsDestination, dsSKU, dsConDetail, dsConSKU;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from BOMMasterAttachmentWithItem where MasterOrderItemId='" + Data["MasterOrderItemId"].ToString() + "'", out dsMaster, false, "1");

                string _bomAttachmentId = "";

                #region Attachment Item
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "BOM Attachment Item", out _bomAttachmentId);
                    _bomAttachmentId = "BOMAT-" + _bomAttachmentId;



                    Data["Id"] = _bomAttachmentId;
                    AddNewRow(dsMaster.Tables[0], Data);
                }
                else
                {
                    _bomAttachmentId = Data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], Data);
                }
                #endregion Attachment Item


                CopyAttachmentTemplate(dsMaster.Tables[0].Rows[0]["BOMMasterId"].ToString(), dsMaster.Tables[0].Rows[0]["Id"].ToString(), out dsDetail, out dsDestination, out dsSKU, out dsConDetail, out dsConSKU);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail, dsDestination, dsSKU, dsConDetail, dsConSKU);

            }
            catch (Exception ex)
            {

                throw ex;
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
        public void UntagAttachment(string MasterOrderItemId)
        {
            try
            {
                //we have to implement untag functionality to keep BOQ item which is used for procurement



                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE from AttachmentDetailConsumptionSKUMapping where AttachmentDetailConsumptionId IN (select Id from AttachmentDetailConsumption where BOMAttachmentDetailId IN (select Id from BOMAttachmentDetail where BOMMasterAttachmentWithItemId IN (select Id from BOMMasterAttachmentWithItem where MasterOrderItemId='" + MasterOrderItemId + "')))");
                con.executeQuery("DELETE from AttachmentDetailConsumption where BOMAttachmentDetailId IN (select Id from BOMAttachmentDetail where BOMMasterAttachmentWithItemId IN (select Id from BOMMasterAttachmentWithItem where MasterOrderItemId='" + MasterOrderItemId + "'))");
                con.executeQuery("DELETE from BOMAttachmentSKUMapping where BOMAttachmentDetailId IN (select Id from BOMAttachmentDetail where BOMMasterAttachmentWithItemId IN (select Id from BOMMasterAttachmentWithItem where MasterOrderItemId='" + MasterOrderItemId + "'))");
                con.executeQuery("DELETE from BOMAttachmentDestination where BOMAttachmentDetailId IN (select Id from BOMAttachmentDetail where BOMMasterAttachmentWithItemId IN (select Id from BOMMasterAttachmentWithItem where MasterOrderItemId='" + MasterOrderItemId + "'))");
                con.executeQuery("DELETE from BOMAttachmentDetail where BOMMasterAttachmentWithItemId IN (select Id from BOMMasterAttachmentWithItem where MasterOrderItemId='" + MasterOrderItemId + "')");
                con.executeQuery("DELETE from BOMMasterAttachmentWithItem where MasterOrderItemId='" + MasterOrderItemId + "'");

                con.executeQuery("DELETE from BOQFGMapping where BOQDetailId IN (select Id from BOQDetail where BOQId IN (select Id from BOQ where MasterOrderItemId='" + MasterOrderItemId + "'))");




                ////con.executeQuery("DELETE from BOQDetail where BOQId IN (select Id from BOQ where MasterOrderItemId='" + MasterOrderItemId + "')");
                con.executeQuery(@"DELETE from BOQDetail where BOQId IN (SELECT Id FROM BOQ AS b where MasterOrderItemId='" + MasterOrderItemId + @"' AND Id NOT IN (SELECT b.Id FROM BOQ AS b 
                                              INNER JOIN trn.POBOQMAP AS p ON p.BOQDetailId=b.Id	
                                              WHERE b.MasterOrderItemId='" + MasterOrderItemId + @"'
                                                UNION
                                                SELECT Id FROM BOQ AS b WHERE Id IN (SELECT b.ParentId FROM BOQ AS b 
                                                                                              INNER JOIN trn.POBOQMAP AS p ON p.BOQDetailId=b.Id	
                                                                                              WHERE b.MasterOrderItemId='" + MasterOrderItemId + @"')

                                                UNION
                                                SELECT Id FROM BOQ AS b WHERE isnull(b.RequiredQtyApproved,0)=1 AND b.MasterOrderItemId='" + MasterOrderItemId + @"'
                                                UNION
                                                SELECT Id FROM BOQ AS b WHERE b.MasterOrderItemId='" + MasterOrderItemId + @"' AND Id 
                                                IN (SELECT ParentId FROM BOQ AS b WHERE isnull(b.RequiredQtyApproved,0)=1 AND b.MasterOrderItemId='" + MasterOrderItemId + @"')))");


                con.executeQuery(@"DELETE FROM BOQ where MasterOrderItemId='" + MasterOrderItemId + @"' AND Id NOT IN (SELECT b.Id FROM BOQ AS b 
                                              INNER JOIN trn.POBOQMAP AS p ON p.BOQDetailId=b.Id	
                                              WHERE b.MasterOrderItemId='" + MasterOrderItemId + @"'
                                                UNION
                                                SELECT Id FROM BOQ AS b WHERE Id IN (SELECT b.ParentId FROM BOQ AS b 
                                                                                              INNER JOIN trn.POBOQMAP AS p ON p.BOQDetailId=b.Id	
                                                                                              WHERE b.MasterOrderItemId='" + MasterOrderItemId + @"')

                                                UNION
                                                SELECT Id FROM BOQ AS b WHERE isnull(b.RequiredQtyApproved,0)=1 AND b.MasterOrderItemId='" + MasterOrderItemId + @"'
                                                UNION
                                                SELECT Id FROM BOQ AS b WHERE b.MasterOrderItemId='" + MasterOrderItemId + @"' AND Id 
                                                IN (SELECT ParentId FROM BOQ AS b WHERE isnull(b.RequiredQtyApproved,0)=1 AND b.MasterOrderItemId='" + MasterOrderItemId + @"'))");

                

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                if (ex.Message.ToString().ToLower().Contains("foreign"))
                    throw new Exception("Reference data exists, cannot delete data");
                else
                    throw ex;
            }


        }
        public void ApprovalRequireQty(string Id, bool Approve)
        {
            try
            {
                string _approve = "1";
                if (Approve == true)
                    _approve = "0";

                //we have to implement untag functionality to keep BOQ item which is used for procurement

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("UPDATE BOQ SET RequiredQtyApproved=" + _approve + " where Id='" + Id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {

                throw ex;
            }


        }
        public void ApprovalRequireQtyMaterial(string MasterOrderItemId, string VendorId, string MaterialMasterId, string ArticleId, bool Approve)
        {
            try
            {
                string _approve = "0";
                if (Approve == true)
                    _approve = "1";

                //we have to implement untag functionality to keep BOQ item which is used for procurement

                string s = @"UPDATE BOQ SET RequiredQtyApproved=" + _approve + " where MasterOrderItemId='" + MasterOrderItemId + @"'
                                    and isnull(VendorId,'')=" + UpdateStringEqual(VendorId) + @" and isnull(MaterialMasterId,'')='" + MaterialMasterId + @"' AND isnull(ArticleId,'')=" + UpdateStringEqual(ArticleId) + @"";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery(s);
                con.CommitTransaction();

            }
            catch (Exception ex)
            {

                throw ex;
            }


        }
        public void UpdateMaterialFlag(string MasterOrderItemId, string VendorId, string MaterialMasterId, string ArticleId, bool isMainMaterial)
        {
            try
            {
                string _isMainMaterial = "0";
                if (isMainMaterial == true)
                    _isMainMaterial = "1";

                //we have to implement untag functionality to keep BOQ item which is used for procurement

                string s = @"UPDATE BOQ SET IsMainMaterial=" + _isMainMaterial + " where MasterOrderItemId='" + MasterOrderItemId + @"'
                                    and isnull(VendorId,'')=" + UpdateStringEqual(VendorId) + @" and isnull(MaterialMasterId,'')='" + MaterialMasterId + @"' AND isnull(ArticleId,'')=" + UpdateStringEqual(ArticleId) + @"";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery(s);
                con.CommitTransaction();

            }
            catch (Exception ex)
            {

                throw ex;
            }


        }



        public void CopyAttachmentTemplate(string BOMMasterId, string BOMMasterAttachmentWithItemId, out DataSet BOMAttachmentDetail, out DataSet BOMAttachmentDestination, out DataSet BOMAttachmentSKUMapping, out DataSet BOMAttachmentDetailConsumption, out DataSet BOMAttachmentDetailConsumptionSKUMapping)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string NewId = "";
            try
            {

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from BOMAttachmentDetail where 1=2", out BOMAttachmentDetail, false, "1");
                con.OpenDataSetThroughAdapter("select * from BOMAttachmentDestination where 1=2", out BOMAttachmentDestination, false, "1");
                con.OpenDataSetThroughAdapter("select * from BOMAttachmentSKUMapping where 1=2", out BOMAttachmentSKUMapping, false, "1");
                con.OpenDataSetThroughAdapter("select * from AttachmentDetailConsumption where 1=2", out BOMAttachmentDetailConsumption, false, "1");
                con.OpenDataSetThroughAdapter("select * from AttachmentDetailConsumptionSKUMapping where 1=2", out BOMAttachmentDetailConsumptionSKUMapping, false, "1");



                DataTable BOMDetail = _sqlRepository.GetDataTable("select * from BOMDetail WHERE BOMMasterId='" + BOMMasterId + "'");
                DataTable BOMDestination = _sqlRepository.GetDataTable("select * from BOMDestination WHERE BOMDetailId IN (select Id from BOMDetail WHERE BOMMasterId='" + BOMMasterId + "')");
                DataTable BOMSKUMapping = _sqlRepository.GetDataTable("select * from BOMSKUMapping WHERE BOMDetailId IN (select Id from BOMDetail WHERE BOMMasterId='" + BOMMasterId + "')");
                DataTable DetailConsumption = _sqlRepository.GetDataTable("select * from DetailConsumption WHERE BOMDetailId IN (select Id from BOMDetail WHERE BOMMasterId='" + BOMMasterId + "')");
                DataTable DetailConsumptionSKUMapping = _sqlRepository.GetDataTable("select * from DetailConsumptionSKUMapping WHERE DetailConsumptionId IN (select Id from DetailConsumption WHERE BOMDetailId IN (select Id from BOMDetail WHERE BOMMasterId='" + BOMMasterId + "'))");


                NewId = GetPK("BOMDetail");
                for (int i = 0; i < BOMDetail.Rows.Count; i++)
                {
                    DataRow drDetailDestination = BOMAttachmentDetail.Tables[0].NewRow();
                    CopyRow(BOMDetail.Rows[i], ref drDetailDestination);
                    drDetailDestination["Id"] = NewId + "-" + (i + 1);
                    drDetailDestination["BOMMasterAttachmentWithItemId"] = BOMMasterAttachmentWithItemId;
                    BOMAttachmentDetail.Tables[0].Rows.Add(drDetailDestination);


                    BOMSKUMapping.DefaultView.RowFilter = "BOMDetailId='" + BOMDetail.Rows[i]["Id"].ToString() + "'";
                    for (int K = 0; K < BOMSKUMapping.DefaultView.Count; K++)
                    {

                        DataRow drDetailSKUDestination = BOMAttachmentSKUMapping.Tables[0].NewRow();
                        CopyRow(BOMSKUMapping.DefaultView[K].Row, ref drDetailSKUDestination);
                        drDetailSKUDestination["Id"] = NewId + "-" + (i + 1) + "-" + (K + 1);
                        drDetailSKUDestination["BOMAttachmentDetailId"] = NewId + "-" + (i + 1);

                        BOMAttachmentSKUMapping.Tables[0].Rows.Add(drDetailSKUDestination);
                    }

                    BOMDestination.DefaultView.RowFilter = "BOMDetailId='" + BOMDetail.Rows[i]["Id"].ToString() + "'";
                    for (int K = 0; K < BOMDestination.DefaultView.Count; K++)
                    {

                        DataRow drDestDestination = BOMAttachmentDestination.Tables[0].NewRow();
                        CopyRow(BOMDestination.DefaultView[K].Row, ref drDestDestination);
                        drDestDestination["Id"] = NewId + "-" + (i + 1) + "-" + (K + 1);
                        drDestDestination["BOMAttachmentDetailId"] = NewId + "-" + (i + 1);

                        BOMAttachmentDestination.Tables[0].Rows.Add(drDestDestination);
                    }



                    DetailConsumption.DefaultView.RowFilter = "BOMDetailId='" + BOMDetail.Rows[i]["Id"].ToString() + "'";
                    for (int K = 0; K < DetailConsumption.DefaultView.Count; K++)
                    {

                        DataRow drDetailConsumptionDestination = BOMAttachmentDetailConsumption.Tables[0].NewRow();
                        CopyRow(DetailConsumption.DefaultView[K].Row, ref drDetailConsumptionDestination);
                        drDetailConsumptionDestination["Id"] = NewId + "-" + (i + 1) + "-" + (K + 1);
                        drDetailConsumptionDestination["BOMAttachmentDetailId"] = NewId + "-" + (i + 1);
                        BOMAttachmentDetailConsumption.Tables[0].Rows.Add(drDetailConsumptionDestination);



                        DetailConsumptionSKUMapping.DefaultView.RowFilter = "DetailConsumptionId='" + DetailConsumption.DefaultView[K]["Id"].ToString() + "'";
                        for (int M = 0; M < DetailConsumptionSKUMapping.DefaultView.Count; M++)
                        {
                            DataRow drAttachmentDetailConsumptionSKUMappingDestination = BOMAttachmentDetailConsumptionSKUMapping.Tables[0].NewRow();
                            CopyRow(DetailConsumptionSKUMapping.DefaultView[M].Row, ref drAttachmentDetailConsumptionSKUMappingDestination);
                            drAttachmentDetailConsumptionSKUMappingDestination["Id"] = NewId + "-" + (i + 1) + "-" + (K + 1) + "-" + (M + 1);
                            drAttachmentDetailConsumptionSKUMappingDestination["AttachmentDetailConsumptionId"] = drDetailConsumptionDestination["Id"];

                            BOMAttachmentDetailConsumptionSKUMapping.Tables[0].Rows.Add(drAttachmentDetailConsumptionSKUMappingDestination);

                        }

                    }
                }



            }
            catch (Exception ex)
            {

                throw ex;
            }


        }
        public void CopyBOMTemplate(string BOMMasterId)
        {
            DataSet BOMAttachmentMaster;
            DataSet BOMAttachmentDetail;
            DataSet BOMAttachmentSKUMapping;
            DataSet BOMAttachmentDestination;
            DataSet BOMAttachmentDetailConsumption;
            DataSet BOMAttachmentDetailConsumptionSKUMapping;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string NewId = "";
            try
            {

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from BOMMaster where 1=2", out BOMAttachmentMaster, false, "1");
                con.OpenDataSetThroughAdapter("select * from BOMDetail where 1=2", out BOMAttachmentDetail, false, "1");
                con.OpenDataSetThroughAdapter("select * from BOMSKUMapping where 1=2", out BOMAttachmentSKUMapping, false, "1");
                con.OpenDataSetThroughAdapter("select * from BOMDestination where 1=2", out BOMAttachmentDestination, false, "1");
                con.OpenDataSetThroughAdapter("select * from DetailConsumption where 1=2", out BOMAttachmentDetailConsumption, false, "1");
                con.OpenDataSetThroughAdapter("select * from DetailConsumptionSKUMapping where 1=2", out BOMAttachmentDetailConsumptionSKUMapping, false, "1");


                DataTable BOMMaster = _sqlRepository.GetDataTable("select * from BOMMaster WHERE Id='" + BOMMasterId + "'");
                DataTable BOMDetail = _sqlRepository.GetDataTable("select * from BOMDetail WHERE BOMMasterId='" + BOMMasterId + "'");
                DataTable BOMSKUMapping = _sqlRepository.GetDataTable("select * from BOMSKUMapping WHERE BOMDetailId IN (select Id from BOMDetail WHERE BOMMasterId='" + BOMMasterId + "')");
                DataTable BOMDestination = _sqlRepository.GetDataTable("select * from BOMDestination WHERE BOMDetailId IN (select Id from BOMDetail WHERE BOMMasterId='" + BOMMasterId + "')");
                DataTable DetailConsumption = _sqlRepository.GetDataTable("select * from DetailConsumption WHERE BOMDetailId IN (select Id from BOMDetail WHERE BOMMasterId='" + BOMMasterId + "')");
                DataTable DetailConsumptionSKUMapping = _sqlRepository.GetDataTable("select * from DetailConsumptionSKUMapping WHERE DetailConsumptionId IN (select Id from DetailConsumption WHERE BOMDetailId IN (select Id from BOMDetail WHERE BOMMasterId='" + BOMMasterId + "'))");



                NewId = GetGeneralPK();
                DataRow drBOMDestination = BOMAttachmentMaster.Tables[0].NewRow();
                CopyRow(BOMMaster.Rows[0], ref drBOMDestination);
                drBOMDestination["Id"] = NewId;
                drBOMDestination["Description"] = BOMMaster.Rows[0]["Description"].ToString() + "-Copy";
                BOMAttachmentMaster.Tables[0].Rows.Add(drBOMDestination);

                for (int i = 0; i < BOMDetail.Rows.Count; i++)
                {
                    DataRow drDetailDestination = BOMAttachmentDetail.Tables[0].NewRow();
                    CopyRow(BOMDetail.Rows[i], ref drDetailDestination);
                    drDetailDestination["Id"] = NewId + "-" + (i + 1);
                    drDetailDestination["BOMMasterId"] = NewId;
                    BOMAttachmentDetail.Tables[0].Rows.Add(drDetailDestination);


                    BOMSKUMapping.DefaultView.RowFilter = "BOMDetailId='" + BOMDetail.Rows[i]["Id"].ToString() + "'";
                    for (int K = 0; K < BOMSKUMapping.DefaultView.Count; K++)
                    {

                        DataRow drDetailSKUDestination = BOMAttachmentSKUMapping.Tables[0].NewRow();
                        CopyRow(BOMSKUMapping.DefaultView[K].Row, ref drDetailSKUDestination);
                        drDetailSKUDestination["Id"] = NewId + "-" + (i + 1) + "-" + (K + 1);
                        drDetailSKUDestination["BOMDetailId"] = NewId + "-" + (i + 1);

                        BOMAttachmentSKUMapping.Tables[0].Rows.Add(drDetailSKUDestination);
                    }


                    BOMDestination.DefaultView.RowFilter = "BOMDetailId='" + BOMDetail.Rows[i]["Id"].ToString() + "'";
                    for (int K = 0; K < BOMDestination.DefaultView.Count; K++)
                    {

                        DataRow drBOMDesDestination = BOMAttachmentDestination.Tables[0].NewRow();
                        CopyRow(BOMDestination.DefaultView[K].Row, ref drBOMDesDestination);
                        drBOMDesDestination["Id"] = NewId + "-" + (i + 1) + "-" + (K + 1);
                        drBOMDesDestination["BOMDetailId"] = NewId + "-" + (i + 1);

                        BOMAttachmentDestination.Tables[0].Rows.Add(drBOMDesDestination);
                    }

                    DetailConsumption.DefaultView.RowFilter = "BOMDetailId='" + BOMDetail.Rows[i]["Id"].ToString() + "'";
                    for (int K = 0; K < DetailConsumption.DefaultView.Count; K++)
                    {

                        DataRow drDetailConsumptionDestination = BOMAttachmentDetailConsumption.Tables[0].NewRow();
                        CopyRow(DetailConsumption.DefaultView[K].Row, ref drDetailConsumptionDestination);
                        drDetailConsumptionDestination["Id"] = NewId + "-" + (i + 1) + "-" + (K + 1);
                        drDetailConsumptionDestination["BOMDetailId"] = NewId + "-" + (i + 1);
                        BOMAttachmentDetailConsumption.Tables[0].Rows.Add(drDetailConsumptionDestination);



                        DetailConsumptionSKUMapping.DefaultView.RowFilter = "DetailConsumptionId='" + DetailConsumption.DefaultView[K]["Id"].ToString() + "'";
                        for (int M = 0; M < DetailConsumptionSKUMapping.DefaultView.Count; M++)
                        {
                            DataRow drAttachmentDetailConsumptionSKUMappingDestination = BOMAttachmentDetailConsumptionSKUMapping.Tables[0].NewRow();
                            CopyRow(DetailConsumptionSKUMapping.DefaultView[M].Row, ref drAttachmentDetailConsumptionSKUMappingDestination);
                            drAttachmentDetailConsumptionSKUMappingDestination["Id"] = NewId + "-" + (i + 1) + "-" + (K + 1) + "-" + (M + 1);
                            drAttachmentDetailConsumptionSKUMappingDestination["DetailConsumptionId"] = drDetailConsumptionDestination["Id"];

                            BOMAttachmentDetailConsumptionSKUMapping.Tables[0].Rows.Add(drAttachmentDetailConsumptionSKUMappingDestination);

                        }

                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(BOMAttachmentMaster, BOMAttachmentDetail, BOMAttachmentSKUMapping, BOMAttachmentDestination, BOMAttachmentDetailConsumption, BOMAttachmentDetailConsumptionSKUMapping);
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        private string GetPK(string TableName)
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), TableName, out sID);
            return sID;
        }
        private void SetForeignKey(DataSet ds, string ColumnName, string KeyValue)
        {
            foreach (DataRow drSource in ds.Tables[0].Rows)
            {
                drSource[ColumnName] = KeyValue;

            }
        }
        private void CopyDataTable(DataTable dtSource, DataTable dtDestination, string PK)
        {
            int Index = 0;
            foreach (DataRow drSource in dtSource.Rows)
            {
                Index++;
                DataRow drDestination = dtDestination.NewRow();
                CopyRow(drSource, ref drDestination);
                if (PK != "")
                    drDestination["Id"] = PK + Index;
                dtDestination.Rows.Add(drDestination);
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

        private string GetGeneralPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "BOM", out idFromDB);
            systemID = idFromDB;
            sID = systemID.Trim();
            return sID;

        }


        private string BOMReportSql(string ItemIds, string MasterOrderItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT mm.MaterialGroupMasterId, b.MaterialMasterId,B.isParent,b.ArticleId,
mm.UserName AS Material,mma.StandardName AS Article,b.SKUDesc,mgm.UserName AS MaterialGroup,b.IncompleteMaterial,
v1.UserName AS CharVal1,v2.UserName AS CharVal2,v3.UserName AS CharVal3,
b.UoMId, b.BaseUoMId, b.POUoMId,
sum(distinct case when isnull(b.RequiredQtyApproved,0)=1 then isnull(BOQP.OrderQty,b.OrderQty) else 0 end) AS OrderQty,
sum(distinct case when isnull(b.RequiredQtyApproved,0)=1 then isnull(BOQP.PlanOrderQty,b.PlanOrderQty) else 0 end) AS PlanOrderQty,
sum(case when isnull(b.RequiredQtyApproved,0)=1 then b.BOMQty else 0 end) AS BOMQty,
sum(case when isnull(b.RequiredQtyApproved,0)=1 then b.RequiredQty else 0 end) AS RequiredQty,
sum(case when isnull(b.RequiredQtyApproved,0)=1 then  b.RequiredQtyPO else 0 end) AS RequiredQtyPO
,uom.UserName AS UOM,uomp.UserName AS POUOM,uomm.UserName AS ParentUOM,SUM(PO.POBOQQty) AS POQty,0 AS GRNQty,
b.RMDescription, b.RMCustomerSpec, b.RMVendorSpec,b.Rate,
sum(b.rate*case when isnull(b.RequiredQtyApproved,0)=1 then b.RequiredQtyPO else 0 end) AS Amount, c.Name AS Currency

FROM BOQ AS b
LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=b.MaterialMasterId
LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=b.ArticleId
LEFT JOIN mst.Destination AS d ON d.Id=b.DestinationId
LEFT JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=mm.MaterialGroupMasterId
LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=b.UoMId
LEFT OUTER JOIN scs.UnitOfMeasurement AS uomP ON uomP.Id=b.POUoMId
LEFT OUTER JOIN HKP.Party P ON p.Id=b.VendorId
LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=b.SalesOrderId
LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=b.MasterOrderItemId
LEFT OUTER JOIN trn.masterorder MO ON MO.Id=moi.MasterOrderId
LEFT OUTER JOIN scs.UnitOfMeasurement AS uomm ON uomm.Id=mo.TotalQtyUOMId
LEFT OUTER JOIN BOQ AS BOQP ON BOQP.Id=B.ParentId
LEFT JOIN scs.Currency AS c ON c.Id=b.CurrencyId

LEFT JOIN (SELECT p2.BOQDetailId, sum(p2.POBOQQty) AS POBOQQty FROM trn.POBOQMAP AS p2 GROUP BY  p2.BOQDetailId) AS PO ON po.BOQDetailId=b.Id


LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id=b.FirstCharacteristicsValueId
LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id=b.SecondCharacteristicsValueId
LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id=b.ThirdCharacteristicsValueId

WHERE b.MasterOrderItemId='" + MasterOrderItemId + @"' AND b.ArticleId IN (" + ItemIds + @") and isnull(B.isParent,0)=0


GROUP BY B.isParent, mm.MaterialGroupMasterId, b.MaterialMasterId,b.ArticleId,
mm.UserName,mma.StandardName,b.SKUDesc,mgm.UserName,b.IncompleteMaterial,
v1.UserName,v2.UserName,v3.UserName,
b.UoMId, b.BaseUoMId, b.POUoMId,uom.UserName,uomp.UserName,uomm.UserName,
b.RMDescription, b.RMCustomerSpec, b.RMVendorSpec,b.Rate, c.Name

ORDER BY mm.MaterialGroupMasterId, b.MaterialMasterId,b.ArticleId,b.SKUDesc, v1.UserName,v2.UserName,v3.UserName";

        }
        public void BOMReport(string ItemIds, string MasterOrderItemId)
        {
            try
            {

                string sql = BOMReportSql(ItemIds, MasterOrderItemId);
                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "BOM Report";

                DataTable dtBOMReport = _sqlRepository.GetDataTable(sql);

                if (dtBOMReport.Rows.Count == 0)
                    throw new Exception("No data found");


                IStyle style = workbook.Styles.Add("FontStyle");
                style.Font.Size = 8f;

                IStyle styleRed = workbook.Styles.Add("FontStyleIncompleteMaterial");
                style.Font.Size = 8f;
                styleRed.Interior.ColorIndex = ExcelKnownColors.Red;


                int colSlNo = 1;
                int colMaterial = 1;
                int colArticle = 1;
                int colSpecification = 1;
                int colSKU1 = 1;
                int colSKU2 = 1;
                int colSKU3 = 1;
                int colQty = 1;
                int colBOMQty = 1;
                int colRequiredQty = 1;
                int colPOQty = 1;
                int colGRNQty = 1;
                int colUnitPrice = 1;
                int colTotalPrice = 1;
                int colRemark = 1;
                int endCol = 1;
                int colUOM = 1;
                int colPOUOM = 1;
                int colCurrency = 1;
                int colParentUOM = 1;
                int COL = 1;
                int SlNo = 1;
                int ROW = 6;

                string MaterialGroupId = "";
                int StartRow = ROW;
                int GroupRow = ROW;
                for (int i = 0; i < dtBOMReport.Rows.Count; i++)
                {

                    if (MaterialGroupId != dtBOMReport.Rows[i]["MaterialGroupMasterId"].ToString())
                    {
                        ROW++;
                        COL = 1;


                        sheet[ROW, COL].Text = "Material Group : " + dtBOMReport.Rows[i]["MaterialGroup"].ToString();
                        SlNo = 0;
                        ROW++;

                        sheet[ROW, COL].Text = "Sl No.";
                        sheet[ROW, COL].ColumnWidth = 4;
                        colSlNo = COL;
                        COL++;

                        sheet[ROW, COL].Text = "Material";
                        sheet[ROW, COL].ColumnWidth = 20;
                        colMaterial = COL;
                        COL++;
                        sheet[ROW, COL].Text = "Article";
                        sheet[ROW, COL].ColumnWidth = 20;
                        colArticle = COL;
                        COL++;
                        sheet[ROW, COL].Text = "Specification";
                        sheet[ROW, COL].ColumnWidth = 10;
                        colSpecification = COL;
                        COL++;
                        sheet[ROW, COL].Text = "SKU1";
                        sheet[ROW, COL].ColumnWidth = 8;
                        colSKU1 = COL;
                        COL++;
                        sheet[ROW, COL].Text = "SKU2";
                        sheet[ROW, COL].ColumnWidth = 8;
                        colSKU2 = COL;
                        COL++;
                        sheet[ROW, COL].Text = "SKU3";
                        sheet[ROW, COL].ColumnWidth = 8;
                        colSKU3 = COL;
                        COL++;
                        sheet[ROW, COL].Text = "FG Qty";
                        sheet[ROW, COL].ColumnWidth = 10;
                        sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                        colQty = COL;
                        COL++;
                        sheet[ROW, COL].Text = "UOM";
                        sheet[ROW, COL].ColumnWidth = 6;
                        colParentUOM = COL;
                        COL++;
                        sheet[ROW, COL].Text = "BOQ";
                        sheet[ROW, COL].ColumnWidth = 10;
                        sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                        colBOMQty = COL;
                        COL++;
                        sheet[ROW, COL].Text = "UOM";
                        sheet[ROW, COL].ColumnWidth = 6;
                        colUOM = COL;
                        COL++;
                        sheet[ROW, COL].Text = "Booking Qty";
                        sheet[ROW, COL].ColumnWidth = 10;
                        sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                        colRequiredQty = COL;
                        COL++;
                        sheet[ROW, COL].Text = "PO Qty";
                        sheet[ROW, COL].ColumnWidth = 10;
                        sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                        colPOQty = COL;
                        COL++;
                        sheet[ROW, COL].Text = "GRN Qty";
                        sheet[ROW, COL].ColumnWidth = 10;
                        sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                        colGRNQty = COL;

                        COL++;
                        sheet[ROW, COL].Text = "UOM";
                        sheet[ROW, COL].ColumnWidth = 6;
                        colPOUOM = COL;
                        COL++;
                        sheet[ROW, COL].Text = "Unit Price";
                        sheet[ROW, COL].ColumnWidth = 10;
                        sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                        colUnitPrice = COL;
                        COL++;
                        sheet[ROW, COL].Text = "Total Price";
                        sheet[ROW, COL].ColumnWidth = 10;
                        sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                        colTotalPrice = COL;
                        COL++;
                        sheet[ROW, COL].Text = "Currency";
                        sheet[ROW, COL].ColumnWidth = 6;
                        colCurrency = COL;
                        COL++;
                        sheet[ROW, COL].Text = "Remark";
                        sheet[ROW, COL].ColumnWidth = 14;
                        colRemark = COL;


                        endCol = COL;
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);


                        sheet.Range[ROW - 1, 1, ROW - 1, endCol].Merge();
                        sheet.Range[ROW - 1, 1, ROW - 1, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[ROW - 1, 1, ROW - 1, endCol].CellStyle.Font.Bold = true;

                        sheet.Range[ROW - 1, 1, ROW, endCol].CellStyle.Font.Size = 10;
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9;

                        ROW++;
                        GroupRow = ROW;
                    }

                    MaterialGroupId = dtBOMReport.Rows[i]["MaterialGroupMasterId"].ToString();


                    sheet[ROW, colSlNo].Number = (SlNo + 1);
                    SlNo++;

                    sheet[ROW, colMaterial].Text = dtBOMReport.Rows[i]["Material"].ToString();
                    sheet[ROW, colArticle].Text = dtBOMReport.Rows[i]["Article"].ToString();
                    sheet[ROW, colSpecification].Text = dtBOMReport.Rows[i]["RMDescription"].ToString();
                    sheet[ROW, colSKU1].Text = dtBOMReport.Rows[i]["CharVal1"].ToString();
                    sheet[ROW, colSKU2].Text = dtBOMReport.Rows[i]["CharVal2"].ToString();
                    sheet[ROW, colSKU3].Text = dtBOMReport.Rows[i]["CharVal3"].ToString();
                    sheet[ROW, colQty].Number = clsStaticInfo.dbl(dtBOMReport.Rows[i]["OrderQty"].ToString());
                    sheet[ROW, colBOMQty].Number = clsStaticInfo.dbl(dtBOMReport.Rows[i]["BOMQty"].ToString());
                    sheet[ROW, colUOM].Text = dtBOMReport.Rows[i]["UOM"].ToString();

                    sheet[ROW, colPOQty].Number = clsStaticInfo.dbl(dtBOMReport.Rows[i]["POQty"].ToString());
                    sheet[ROW, colGRNQty].Number = clsStaticInfo.dbl(dtBOMReport.Rows[i]["GRNQty"].ToString());
                    sheet[ROW, colRequiredQty].Number = clsStaticInfo.dbl(dtBOMReport.Rows[i]["RequiredQtyPO"].ToString());
                    sheet[ROW, colPOUOM].Text = dtBOMReport.Rows[i]["POUOM"].ToString();
                    sheet[ROW, colParentUOM].Text = dtBOMReport.Rows[i]["ParentUOM"].ToString();
                    


                    sheet[ROW, colUnitPrice].Number = clsStaticInfo.dbl(dtBOMReport.Rows[i]["Rate"].ToString());
                    sheet[ROW, colTotalPrice].Number = clsStaticInfo.dbl(dtBOMReport.Rows[i]["Amount"].ToString());
                    sheet[ROW, colCurrency].Text = dtBOMReport.Rows[i]["Currency"].ToString();


                    //  sheet[ROW, colRemark].Text = dtBOMReport.Rows[i]["Amount"].ToString();


                    sheet.Range[ROW, 1, ROW, endCol].CellStyle = style;
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            
                    if (bplib.clsWebLib.GetBoolData(dtBOMReport.Rows[i]["IncompleteMaterial"].ToString()))
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle = styleRed;
                    ROW++;

                }

                sheet.Range[1, colQty, ROW, colQty].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[1, colRequiredQty, ROW, colRequiredQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[1, colBOMQty, ROW, colBOMQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[1, colPOQty, ROW, colPOQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[1, colGRNQty, ROW, colGRNQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[1, colUnitPrice, ROW, colUnitPrice].NumberFormat = clsStaticInfo.NumberFormat(4);
                sheet.Range[1, colTotalPrice, ROW, colTotalPrice].NumberFormat = clsStaticInfo.NumberFormat(2);

                //sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
                //sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;


                sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "BOM", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "BOM.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }




    }
}
