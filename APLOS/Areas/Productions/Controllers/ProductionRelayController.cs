#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;
using System;
using OTSBD;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionRelayController : BaseController
    {
        #region Constructor
        /// <summary>   The CostingTypesService service. </summary>
        private readonly ISqlRepository _sqlRepository;
        public ProductionRelayController(ISqlRepository R)
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

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT CostingType AS [Value], UserName AS [Text] FROM [dbo].[CostingTypes]"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT * FROM [dbo].[CostingTypes]"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(List<Dictionary<string, object>> ProductionRelayData)
        {
            try
            {
                if (clsStaticInfo.nullrecorder(ProductionRelayData) == "" )
                {
                    throw new Exception("Please select at least one production process.");
                }

                DataSet dsProductionRelay;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager conBin = new ConnectionManager.DAL.ConManager("1");
                conBin.OpenDataSetThroughAdapter(@"Select * from trn.ProductionOrderProcessSet", out dsProductionRelay, false, "1");

                string IncompleteProductionOrder = "";
                for (int i = 0; i < ProductionRelayData.Count; i++)
                {
                    if (clsStaticInfo.nullrecorder(ProductionRelayData[i]["PPRId"]) != "")
                    {
                        if (bplib.clsWebLib.GetBoolData(ProductionRelayData[i]["IsCompleted"]) == false)
                        {
                            if (IncompleteProductionOrder == "")
                            {
                                IncompleteProductionOrder = ProductionRelayData[i]["Id"].ToString();
                            }
                            else
                            {
                                IncompleteProductionOrder += "," + ProductionRelayData[i]["Id"].ToString();
                            }

                            throw new Exception("Previous process is not completed therefore cannot complete the current production order");
                        }
                    }
                }

                for (int i = 0; i < ProductionRelayData.Count; i++)
                {

                    dsProductionRelay.Tables[0].DefaultView.RowFilter = "Id='" + ProductionRelayData[i]["PSSId"] + "'";
                    if (dsProductionRelay.Tables[0].DefaultView.Count > 0)
                    {
                        //edit
                        DataRow dr = dsProductionRelay.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["IsCompleted"] = true;
                        dr["CompletedBy"] = identity.Name;
                        dr["CompletionEntryDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();

                        dr.EndEdit();

                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsProductionRelay);

                return Json(new { Error = false, Data = ProductionRelayData,/* Sequence = GetSequence(),*/ Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

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
        [Authorize, HttpGet]
        public ActionResult GetProductionRelay(string EntityId, string ProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT 
convert(bit,0) AS Checked, pss.Id PSSId,ppr.Id PPRId, convert(bit ,isnull(ppr.IsCompleted,0)) AS IsCompleted,P.UserName PreviousProcess,PPR.CompletedBy ClosedBy
,Format(PPR.CompletionEntryDate,'dd-MMM-yyyy') ClosedDate ,Format(PPR.StartDate,'dd-MMM-yyyy') PreviousProcessStartDate
,  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
isnull(CurrentProcessPR.ProductionQtyAtPR,0) ProducedQty
,Variance=case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end-isnull(CurrentProcessPR.ProductionQtyAtPR,0),
    ISNULL(PO.Qty,0) AS POQuantity,ISNULL(SO.PlannedQty,0) AS PlannedQty,ISNULL(SO.OrderQty,0) AS OrderQty,so.Material,
     so.ProductCategory,so.Product,
		ActualQTY=	case when ISNULL(st.Qty,0)>isnull(PO.Qty,0) then st.Qty else po.Qty end,							
                                Format(so.LastShipmentDate,'dd-MMM-yyyy') LastShipmentDate, so.article,CurrentProcessPR.ProductionQtyAtPR
								,Format(CurrentProcessPR.ProductionStartDateAtPR,'dd-MMM-yyyy') ProductionStartDateAtPR,
								Format(PSS.StartDate,'dd-MMM-yyyy') StartDate,Format(st.LSD,'dd-MMM-yyyy') LSD,
PreviousProcessPR.ProductionQtyAtPR,
								      MasterOrderId =STUFF((select distinct ','+XMOI.Id from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
                                                    BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                                  			
  
                            FROM [TRN].[ProductionOrder] AS PO
							left outer join ProductionOrderSchedulingParametersType1 st on st.ProductionOrderID=PO.Id
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
							left join trn.ProductionOrderProcessSet PSS ON PSS.ProductionOrderId=PO.Id
							left join trn.ProductionOrderProcessSet PPR ON PSS.ProductionOrderId=PO.Id 
							and PPR.id=(select A.Id from (
							select DENSE_RANK() over (partition by p.ProductionOrderId order by P.[sequence] desc) AS RNK,P.*
							from TRN.ProductionOrderProcessSet P 
							
							where p.ProductionOrderId=PSS.ProductionOrderId AND P.Sequence<PSS.sequence) AS A where a.RNK=1)
							left outer join HKP.Process P on p.Id=PPR.ProcessId					

                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDateAtPR,MAX(s.ProductionDate) AS ProductionEndDateAtPR
											FROM  trn.ProductionSummary S 
											where s.ProcessId='" + ProcessId + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS CurrentProcessPR ON  CurrentProcessPR.ProductionOrderId=po.id AND CurrentProcessPR.ProcessId=PSS.ProcessId
							
                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDate,MAX(s.ProductionDate) AS ProductionEndDate
											FROM  trn.ProductionSummary S 
											where s.ProcessId='" + ProcessId + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PreviousProcessPR ON  PreviousProcessPR.ProductionOrderId=po.id AND PreviousProcessPR.ProcessId=PPR.ProcessId
							 
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,ma.StandardName AS Article, PM.UserName AS Product,pc.UserName AS ProductCategory,
                                                     min(so.DeliveryDate) AS FirstShipmentDate,  max(so.DeliveryDate) AS LastShipmentDate,
                                                    sum(so.Qty) AS OrderQty,
                                                    SUM(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty
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
                            WHERE isnull(s.username,'') IN ('RUNNING') 
                            AND ((ISNULL(ppr.Id,'')<>'' AND ISNULL(ppr.StartDate,'')<>'') OR ISNULL(ppr.Id,'')='')
                            AND  PO.entityid='" + EntityId + @"' and  PSS.ProcessId = '" + ProcessId + @"' and isnull(pss.IsCompleted,0)=0";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult GetProductionRelayClosed(string EntityId, string ProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT 
convert(bit,0) AS Checked, pss.Id PSSId,ppr.Id PPRId, convert(bit ,isnull(PSS.IsCompleted,0)) AS IsCompleted,P.UserName PreviousProcess,PPR.CompletedBy ClosedBy
,Format(PPR.CompletionEntryDate,'dd-MMM-yyyy') ClosedDate ,Format(PPR.StartDate,'dd-MMM-yyyy') PreviousProcessStartDate
,  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
isnull(CurrentProcessPR.ProductionQtyAtPR,0) ProducedQty
,Variance=case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end-isnull(CurrentProcessPR.ProductionQtyAtPR,0),
    ISNULL(PO.Qty,0) AS POQuantity,ISNULL(SO.PlannedQty,0) AS PlannedQty,ISNULL(SO.OrderQty,0) AS OrderQty,so.Material,
     so.ProductCategory,so.Product,
		ActualQTY=	case when ISNULL(st.Qty,0)>isnull(PO.Qty,0) then st.Qty else po.Qty end,							
                                Format(so.LastShipmentDate,'dd-MMM-yyyy') LastShipmentDate, so.article,CurrentProcessPR.ProductionQtyAtPR
								,Format(CurrentProcessPR.ProductionStartDateAtPR,'dd-MMM-yyyy') ProductionStartDateAtPR,
								Format(PSS.StartDate,'dd-MMM-yyyy') StartDate,Format(st.LSD,'dd-MMM-yyyy') LSD,
PreviousProcessPR.ProductionQtyAtPR,
								      MasterOrderId =STUFF((select distinct ','+XMOI.Id from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
                                                    BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                                  			
  
                            FROM [TRN].[ProductionOrder] AS PO
							left outer join ProductionOrderSchedulingParametersType1 st on st.ProductionOrderID=PO.Id
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
						 join trn.ProductionOrderProcessSet PSS ON PSS.ProductionOrderId=PO.Id
							left join trn.ProductionOrderProcessSet PPR ON PSS.ProductionOrderId=PO.Id 
							and PPR.id=(select A.Id from (
							select DENSE_RANK() over (partition by p.ProductionOrderId order by P.[sequence] desc) AS RNK,P.*
							from TRN.ProductionOrderProcessSet P 
							
							where p.ProductionOrderId=PSS.ProductionOrderId AND P.Sequence<PSS.sequence) AS A where a.RNK=1)
							left outer join HKP.Process P on p.Id=PPR.ProcessId					

                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDateAtPR,MAX(s.ProductionDate) AS ProductionEndDateAtPR
											FROM  trn.ProductionSummary S 
											where s.ProcessId='" + ProcessId+ @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS CurrentProcessPR ON  CurrentProcessPR.ProductionOrderId=po.id AND CurrentProcessPR.ProcessId=PSS.ProcessId
							
                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDate,MAX(s.ProductionDate) AS ProductionEndDate
											FROM  trn.ProductionSummary S 
											where s.ProcessId='" + ProcessId + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PreviousProcessPR ON  PreviousProcessPR.ProductionOrderId=po.id AND PreviousProcessPR.ProcessId=PPR.ProcessId
							 
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,ma.StandardName AS Article, PM.UserName AS Product,pc.UserName AS ProductCategory,
                                                     min(so.DeliveryDate) AS FirstShipmentDate,  max(so.DeliveryDate) AS LastShipmentDate,
                                                    sum(so.Qty) AS OrderQty,
                                                    SUM(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty
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
                            WHERE isnull(s.username,'') IN ('RUNNING') AND  PO.entityid='" + EntityId+@"' and  PSS.ProcessId = '" + ProcessId + @"' and isnull(pss.IsCompleted,0)=1";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        #endregion
    }


}