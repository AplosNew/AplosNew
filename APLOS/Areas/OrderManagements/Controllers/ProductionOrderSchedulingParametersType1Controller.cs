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
using OTSBD;
using System.Linq;
using Library.Service.Enums;
using System.Text;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using Microsoft.AspNet.SignalR.Client;
using Library.Service.TaskScheduler;
using Library.Data;
using Library.Service.Helpers;
using Library.Model.Enums;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class productionOrderSchedulingParametersType1Controller : BaseController
    {
        public enum PlanningStatus { TOSTART, FREEZE, RUNNING, ACTIVE, CLOSED };
        private EnumPlanningTypes ScreenPlanningType = EnumPlanningTypes.PlanningType1;

        #region Constructor

        private readonly IProductionOrderService _productionOrderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public productionOrderSchedulingParametersType1Controller(IProductionOrderService productionOrderService, IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
            _productionOrderService = productionOrderService;



        }
        #endregion

        #region -- Pages
        private void SendNotification(string Message, int Current = 0, int Total = 0)
        {
            try
            {
                var _identitySignal = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (Current == 0)
                    clsMobileNotification.SendMessage(_identitySignal.CompanyGroupId, _identitySignal.PlantId, _identitySignal.UserId, Message);
                else
                {
                    Message += string.Format("  [{0}/{1}]", Current, Total);

                    clsMobileNotification.SendMessage(_identitySignal.CompanyGroupId, _identitySignal.PlantId, _identitySignal.UserId, Message);
                }

            }
            catch (Exception ex)
            {

            }

        }
        public async Task<ActionResult> Aplos()
        {
            return await Task.Factory.StartNew(() =>
            {

                SendNotification("Ready to Simulate");
                return View();
            });

        }

        public async Task<ActionResult> AplosNew()
        {
            return await Task.Factory.StartNew(() =>
            {

                SendNotification("Ready to Simulate");
                return View();
            });

        }
        public async Task<ActionResult> Type2()
        {
            return await Task.Factory.StartNew(() =>
            {

                SendNotification("Ready to Simulate");
                return View();
            });

        }
        #endregion

        #region -- Operations
        [Authorize, HttpGet]
        public JsonResult GetProcessForPlanning()
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT distinct p.* FROM PlanningTypes AS pt 
                                INNER JOIN hkp.Process AS p ON p.Id=pt.BaseProcessId
                                WHERE PT.PlanningType='" + ScreenPlanningType.ToString() + "' AND pt.CompanyGroupId='" + identity.CompanyGroupId + "'  AND pt.PlantId='" + identity.PlantId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public JsonResult GetArticle(string POID)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select  MA.StandardName as Text,MA.Id as Value from trn.ProductionOrderDetail Pod 
                                                            left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId=so.Id
                                                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                            left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                            where Pod.ProductionOrderId='" + POID + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public ActionResult GetList(string baseprocessid, string entityid, string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (
                        SELECT  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
                        ISNULL(PO.Qty,0) AS POQuantity,ISNULL(PO.PlannedQty,0) AS SOQuantity,ISNULL(SO.SOQuantity,0) AS SavedQuantity,t1.Color,FORMAT(t1.LSD,'dd-MMM-yyyy') AS LSD,FORMAT(t1.CommitmentDate,'dd-MMM-yyyy') AS CommitmentDate,t1.TargetPerDay
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
                                                    pod.ProductionOrderId, sum(so.Qty) AS SOQuantity, Min(so.DeliveryDate) FirstShipmentDate
													,Max(so.DeliveryDate) LastShipmentDate,
                                                    MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
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
													,Material=STUFF((select distinct ', '+mm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                     ,Article=STUFF((select distinct ', '+mm.StandardName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													,Product=STUFF((select distinct ', '+Pm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													,ProductCategory=STUFF((select distinct ', '+pc.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
from trn.ProductionOrderDetail AS pod JOIN  trn.SalesOrder SO ON pod.SalesOrderId=so.Id group by pod.ProductionOrderId) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE isnull(s.username,'') IN ('" + PlanningStatus.ACTIVE.ToString() + @"','" + PlanningStatus.RUNNING.ToString() + @"') AND  PO.entityid='" + entityid + @"' and PO.Id IN (SELECT DISTINCT pops.ProductionOrderId
                            FROM trn.ProductionOrderProcessSet AS pops WHERE pops.ProcessId = '" + baseprocessid
                            + @"') ) AS TEMP WHERE " + strkey + " ORDER BY ProductionPriority";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult GetListNew(string baseprocessid, string entityid, string column, string value)
        {
            string entityId = "'" + entityid.Replace(",", "','") + "'";//replaced with ""
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (
                        SELECT  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
                        ISNULL(PO.Qty,0) AS POQuantity,ISNULL(PO.PlannedQty,0) AS SOQuantity,ISNULL(SO.SOQuantity,0) AS SavedQuantity,t1.Color,FORMAT(t1.LSD,'dd-MMM-yyyy') AS LSD,FORMAT(t1.CommitmentDate,'dd-MMM-yyyy') AS CommitmentDate,t1.TargetPerDay
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
                                                    pod.ProductionOrderId, sum(so.Qty) AS SOQuantity, Min(so.DeliveryDate) FirstShipmentDate
													,Max(so.DeliveryDate) LastShipmentDate,
                                                    MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
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
													,Material=STUFF((select distinct ', '+mm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                     ,Article=STUFF((select distinct ', '+mm.StandardName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													,Product=STUFF((select distinct ', '+Pm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													,ProductCategory=STUFF((select distinct ', '+pc.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
from trn.ProductionOrderDetail AS pod JOIN  trn.SalesOrder SO ON pod.SalesOrderId=so.Id group by pod.ProductionOrderId) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE isnull(s.username,'') IN ('" + PlanningStatus.ACTIVE.ToString() + @"','" + PlanningStatus.RUNNING.ToString() + @"') AND  PO.entityid IN(" + entityId + @") and PO.PlanningTypeProcessId ='" + baseprocessid + @"' ) AS TEMP WHERE " + strkey + " ORDER BY ProductionPriority";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProductionReference(string productionOrderId)
        {
            Library.Planning.PlanningType1.PlanningType1Scheduler sch = new Library.Planning.PlanningType1.PlanningType1Scheduler();

            return Json(_sqlRepository.GetDataCollection(sch.GetProductionReference(productionOrderId), null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdatePriority(List<Dictionary<string, object>> data)
        {

            try
            {
                if (data == null)
                    throw new Exception("No data changed!!!");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                for (int i = 0; i < data.Count; i++)
                {
                    con.executeQuery("update ProductionOrderSchedulingParametersType1 SET ProductionPriority =" + clsStaticInfo.dbl(data[i]["ProductionPriority"].ToString()) + " WHERE ProductionOrderID='" + data[i]["Id"].ToString() + "' ");
                }

                con.CommitTransaction();

                return Json(new { Error = false, Message = "Priority successfully reinitialized" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }


        [HttpPost, Authorize]
        public ActionResult GetWorkcenterWisePlanningSummary(string EntityId)
        {

            Library.Planning.PlanningType1.PlanningType1Scheduler sch = new Library.Planning.PlanningType1.PlanningType1Scheduler();

            return Json(_sqlRepository.GetDataCollection(sch.GetAllWorkcenterWisePlanningSummary(EntityId), null), JsonRequestBehavior.AllowGet);
        }




        [HttpPost, Authorize]
        public ActionResult GetSingleWorkcenterWisePlanningSummary(string WorkCenterId)
        {

            Library.Planning.PlanningType1.PlanningType1Scheduler sch = new Library.Planning.PlanningType1.PlanningType1Scheduler();

            return Json(_sqlRepository.GetDataCollection(sch.GetSingleWorkcenterWisePlanningSummary(WorkCenterId), null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetSingleWorkcenterWiseTargetSummaryByDate(string WorkCenterId, string Date)
        {

            Library.Planning.PlanningType1.PlanningType1Scheduler sch = new Library.Planning.PlanningType1.PlanningType1Scheduler();

            return Json(_sqlRepository.GetDataCollection(sch.GetSingleWorkcenterWiseTargetSummaryByDate(WorkCenterId, Date), null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetNewList(string column, string value, string baseprocessid, string entityid)
        {
            string strKey = "1=1";
            if (column != "")
                strKey = column + " LIKE '%" + clsStaticInfo.nullrecorder(value) + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (
                        SELECT 'INCLUDE' AS WCPreferenceType,1 AS RunningOrderBlockSize, PO.Id, PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, 
EN.UserName AS EntityName, S.UserName AS ProductionStatusName,isnull(PO.Qty,0) AS POQuantity,ISNULL(PO.PlannedQty,0) AS SOQuantity,SO.*
                                FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId
                                                   ,Material=STUFF((select distinct ', '+mm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                    ,NoOfArticle=(select COUNT(mm.StandardName) from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId)
                                                    ,Article=STUFF((select distinct ', '+mm.StandardName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												   ,Product=STUFF((select distinct ', '+Pm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

												   ,ProductCategory=STUFF((select distinct ', '+pc.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												   
												   ,ProductMasterId=STUFF((select distinct ', '+pm.id from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

												   ,
                                                     FORMAT(Min(sO.LSD),'dd-MMM-yyyy') AS LSD,FORMAT(max(SO.PlanExFactoryDate),'dd-MMM-yyyy') AS PlanExFactoryDate ,
                                                    sum(so.Qty) AS TotalSOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate
                                                    --,SUM((isnull(SO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
													,PlannedQty=(Select SUM((isnull(XSO.qty, 0) * (1 + (isnull(xmoi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(xmoi.OrderWastagePercentage, 0))))
															from trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
															 where pod.ProductionOrderId=Xpod.ProductionOrderId)

                                                    ,MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
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
                                                     
from trn.ProductionOrderDetail AS pod JOIN  trn.SalesOrder SO ON pod.SalesOrderId=so.Id group by pod.ProductionOrderId) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                        WHERE po.Id NOT IN (SELECT ProductionOrderSchedulingParametersType1.ProductionOrderID
                      FROM ProductionOrderSchedulingParametersType1)
                            AND 
isnull(S.username,'')<>'" + PlanningStatus.CLOSED.ToString() + @"' AND  po.entityid='" + entityid + @"' and PO.Id IN (SELECT DISTINCT pops.ProductionOrderId
                            FROM trn.ProductionOrderProcessSet AS pops WHERE pops.ProcessId = '" + baseprocessid + @"')) AS TEMP where " + strKey;

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetPONewList(string column, string value, string baseprocessid, string entityid)
        {
            string entityId = "'" + entityid.Replace(",", "','") + "'";//replaced with ""
            string strKey = "1=1";
            if (column != "")
                strKey = column + " LIKE '%" + clsStaticInfo.nullrecorder(value) + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (
                        SELECT 'INCLUDE' AS WCPreferenceType,1 AS RunningOrderBlockSize, PO.Id, PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, 
EN.UserName AS EntityName, S.UserName AS ProductionStatusName,isnull(PO.Qty,0) AS POQuantity,ISNULL(PO.PlannedQty,0) AS SOQuantity,SO.*
                                FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId
                                                   ,Material=STUFF((select distinct ', '+mm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                    ,NoOfArticle=(select COUNT(mm.StandardName) from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId)
                                                    ,Article=STUFF((select distinct ', '+mm.StandardName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												   ,Product=STUFF((select distinct ', '+Pm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

												   ,ProductCategory=STUFF((select distinct ', '+pc.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												   
												   ,ProductMasterId=STUFF((select distinct ', '+pm.id from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

												   ,
                                                     FORMAT(Min(sO.LSD),'dd-MMM-yyyy') AS LSD,FORMAT(max(SO.PlanExFactoryDate),'dd-MMM-yyyy') AS PlanExFactoryDate ,
                                                    sum(so.Qty) AS TotalSOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate
                                                    --,SUM((isnull(SO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
													,PlannedQty=(Select SUM((isnull(XSO.qty, 0) * (1 + (isnull(xmoi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(xmoi.OrderWastagePercentage, 0))))
															from trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
															 where pod.ProductionOrderId=Xpod.ProductionOrderId)

                                                    ,MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
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
                                                     
from trn.ProductionOrderDetail AS pod JOIN  trn.SalesOrder SO ON pod.SalesOrderId=so.Id group by pod.ProductionOrderId) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                        WHERE po.Id NOT IN (SELECT ProductionOrderSchedulingParametersType1.ProductionOrderID
                      FROM ProductionOrderSchedulingParametersType1)
                            AND 
isnull(S.username,'')<>'" + PlanningStatus.CLOSED.ToString() + @"' AND  po.entityid IN(" + entityId + @") and PO.PlanningTypeProcessId ='" + baseprocessid + @"') AS TEMP where " + strKey;

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetSalesOrderList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_productionOrderService.GetSalesOrderList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getProductionOrderParameters(string productionOrderID)
        {


            string sql = @"SELECT T1.*,PO.ProductionStatusId FROM (SELECT 
                            '1' AS ID) AS K 
                            LEFT OUTER JOIN
                             [ProductionOrderSchedulingParametersType1] T1 ON T1.ProductionOrderID='" + productionOrderID + @"'
                            left outer join trn.productionorder PO ON PO.Id='" + productionOrderID + @"'
                            ";

            List<Dictionary<string, object>> _data = _sqlRepository.GetDataCollection(sql);

            if (string.IsNullOrEmpty(_data[0]["ID"].ToString()))
            {
                string _masterOrderParams = @"SELECT FORMAT(MIN(so.LSD),'dd-MMM-yyyy') AS LSD,FORMAT(MAX(so.CommitmentDate),'dd-MMM-yyyy') AS CommitmentDate,
                                                 FORMAT(MIN(so.MainRawMaterialInhouseDate),'dd-MMM-yyyy') AS MainRawMaterialInhouseDate, FORMAT(MAX(so.OtherRawMaterialInhouseDate),'dd-MMM-yyyy') AS OtherRawMaterialInhouseDate
                                                  FROM trn.ProductionOrderDetail AS pod
                                                INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                                WHERE pod.ProductionOrderId='" + productionOrderID + @"'";


                DataTable dt = _sqlRepository.GetDataTable(_masterOrderParams);
                if (dt.Rows.Count > 0)
                {
                    _data[0]["LSD"] = dt.Rows[0]["LSD"].ToString();
                    _data[0]["CommitmentDate"] = dt.Rows[0]["CommitmentDate"].ToString();
                    _data[0]["MainRawMaterialInhouseDate"] = dt.Rows[0]["MainRawMaterialInhouseDate"].ToString();
                    _data[0]["OtherRawMaterialInhouseDate"] = dt.Rows[0]["OtherRawMaterialInhouseDate"].ToString();


                }
            }

            return Json(_data, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult getProductMasterParameters(string productionOrderID, string entityid, string baseprocessid)
        {


            string sql = @"SELECT SSS.*,PD.ProductMasterId AS Id,pm.UserName AS ProductName,pc.UserName AS ProductCategory,puc.UserName AS ProductSubCategory, 
                            pme.NoOfWorkStation, pme.EfficencyPercentage AS Efficiency,pme.StandardWorkingHours PlanWorkingHoursPerDay, pme.SPT,
                            MLD.[Value] AS MinimumLineDays,format((SELECT min(SO.MainRawMaterialInhouseDate) AS MainRawMaterialInhouseDate
                                        FROM trn.SalesOrder AS so
                                        INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id where pod.ProductionOrderId='" + productionOrderID + @"'),'dd-MMM-yyyy') AS MainRawMaterialInhouseDate,
                                   format( (SELECT min(SO.OtherRawMaterialInhouseDate) AS OtherRawMaterialInhouseDate
                                        FROM trn.SalesOrder AS so
                                        INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id where pod.ProductionOrderId='" + productionOrderID + @"'),'dd-MMM-yyyy') AS OtherRawMaterialInhouseDate,
                                   format( (SELECT min(SO.LSD) AS LSD
                                        FROM trn.SalesOrder AS so
                                        INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id where pod.ProductionOrderId='" + productionOrderID + @"'),'dd-MMM-yyyy') AS LSD,
                                     format((SELECT MAX(SO.CommitmentDate) AS CommitmentDate
                                                        FROM trn.SalesOrder AS so
                                        INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id where pod.ProductionOrderId='" + productionOrderID + @"'),'dd-MMM-yyyy') AS CommitmentDate,
                                                                    PM.FirstdayOutPut AS FirstDayOutPut,PM.IncrementValue,PM.DaysToReachTheTarget AS DayToReachTheTarget,
                                CASE WHEN ISNULL(PD.IsFixed,'')='FIXED' THEN 'FIXED' ELSE 'PERCENTAGE' END AS IncrementType
                                    FROM [TRN].[ProductDefinition] PD
                                LEFT OUTER JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId AND PM.BaseProcessId='" + baseprocessid + @"'
                                LEFT OUTER JOIN [HKP].[ProductCategory] PC ON pc.Id=pm.ProductCategoryId
                                    LEFT OUTER JOIN [HKP].[ProductSubCategory] PUC ON PUC.Id=pm.ProductSubCategoryId
                                LEFT OUTER JOIN [TRN].[ProductMasterEfficency] PME ON pme.ProductMasterId=pm.Id AND pme.EfficencyName='Planning'
                                LEFT OUTER JOIN dbo.EntityConfig con ON 1=1 and con.EntityId='" + entityid + @"' AND con.StandardName='" + EntityConfigParameter.StandardWorkingHoursPerDay + @"'
                                   LEFT OUTER JOIN dbo.EntityConfig MLD ON 1=1 and MLD.EntityId='" + entityid + @"' AND MLD.StandardName='" + EntityConfigParameter.MinimumLineDays + @"'
                        
                                   CROSS JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,ma.StandardName AS Article, --PM.UserName AS Product,pc.UserName AS ProductCategory,
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
                                                 WHERE pod.ProductionOrderId='" + productionOrderID + @"'
                                                    group by pod.ProductionOrderId,mm.userName,ma.StandardName,PM.UserName,pc.UserName) AS SSS
                        WHERE PD.MaterialMasterId IN (
	
                                    SELECT DISTINCT moi.MaterialMasterId FROM [TRN].[ProductionOrderDetail] D
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=d.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                    WHERE d.ProductionOrderId='" + productionOrderID + @"'
                                    )    ";

            string sqlBulletinData = @"SELECT tm.RequiredStdTarget,tm.PlannedHoursPerDay, tm.MaxNoOfWS,sum(d.TotalSPT) AS SPT
									,SUM(D.AllotedWorkstation) AS TotalWS
									  FROM trn.ProductionBulletinTemplate AS T
									INNER JOIN  trn.ProductionBulletinTemplateMaster AS TM ON t.Id=tm.ProductionBulletinTemplateId
									INNER JOIN trn.ProductionBulletinTemplateDetail AS D ON d.ProductionBulletinTemplateMasterId=TM.Id
									WHERE t.ProductionOrderId='" + productionOrderID + "' AND TM.ProcessId='" + baseprocessid + @"'
									GROUP BY tm.RequiredStdTarget,tm.PlannedHoursPerDay, tm.MaxNoOfWS";

            return Json(new { MainData = _sqlRepository.GetDataCollection(sql), BulletinData = _sqlRepository.GetDataCollection(sqlBulletinData) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getProductMasterParametersNew(string productionOrderID, string entityid, string baseprocessid)
        {


            string sql = @"SELECT SSS.*,PD.ProductMasterId AS Id,pm.UserName AS ProductName,pc.UserName AS ProductCategory,puc.UserName AS ProductSubCategory, 
                            pme.NoOfWorkStation, pme.EfficencyPercentage AS Efficiency,pme.StandardWorkingHours PlanWorkingHoursPerDay, pme.SPT,
                            MLD.[Value] AS MinimumLineDays,format((SELECT min(SO.MainRawMaterialInhouseDate) AS MainRawMaterialInhouseDate
                                        FROM trn.SalesOrder AS so
                                        INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id where pod.ProductionOrderId='" + productionOrderID + @"'),'dd-MMM-yyyy') AS MainRawMaterialInhouseDate,
                                   format( (SELECT min(SO.OtherRawMaterialInhouseDate) AS OtherRawMaterialInhouseDate
                                        FROM trn.SalesOrder AS so
                                        INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id where pod.ProductionOrderId='" + productionOrderID + @"'),'dd-MMM-yyyy') AS OtherRawMaterialInhouseDate,
                                   format( (SELECT min(SO.LSD) AS LSD
                                        FROM trn.SalesOrder AS so
                                        INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id where pod.ProductionOrderId='" + productionOrderID + @"'),'dd-MMM-yyyy') AS LSD,
                                     format((SELECT MAX(SO.CommitmentDate) AS CommitmentDate
                                                        FROM trn.SalesOrder AS so
                                        INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id where pod.ProductionOrderId='" + productionOrderID + @"'),'dd-MMM-yyyy') AS CommitmentDate,
                                                                    PM.FirstdayOutPut AS FirstDayOutPut,PM.IncrementValue,PM.DaysToReachTheTarget AS DayToReachTheTarget,
                                CASE WHEN ISNULL(PD.IsFixed,'')='FIXED' THEN 'FIXED' ELSE 'PERCENTAGE' END AS IncrementType
                                    FROM [TRN].[ProductDefinition] PD
                                LEFT OUTER JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId AND PM.BaseProcessId='" + baseprocessid + @"'
                                LEFT OUTER JOIN [HKP].[ProductCategory] PC ON pc.Id=pm.ProductCategoryId
                                    LEFT OUTER JOIN [HKP].[ProductSubCategory] PUC ON PUC.Id=pm.ProductSubCategoryId
                                LEFT OUTER JOIN [TRN].[ProductMasterEfficency] PME ON pme.ProductMasterId=pm.Id AND pme.EfficencyName='Planning'
                                LEFT OUTER JOIN dbo.EntityConfig con ON 1=1 and con.EntityId='" + entityid + @"' AND con.StandardName='" + EntityConfigParameter.StandardWorkingHoursPerDay + @"'
                                   LEFT OUTER JOIN dbo.EntityConfig MLD ON 1=1 and MLD.EntityId='" + entityid + @"' AND MLD.StandardName='" + EntityConfigParameter.MinimumLineDays + @"'
                        
                                   CROSS JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,ma.StandardName AS Article, --PM.UserName AS Product,pc.UserName AS ProductCategory,
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
                                                 WHERE pod.ProductionOrderId='" + productionOrderID + @"'
                                                    group by pod.ProductionOrderId,mm.userName,ma.StandardName,PM.UserName,pc.UserName) AS SSS
                        WHERE PD.MaterialMasterId IN (
	
                                    SELECT DISTINCT moi.MaterialMasterId FROM [TRN].[ProductionOrderDetail] D
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=d.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                    WHERE d.ProductionOrderId='" + productionOrderID + @"'
                                    )    ";

            string sqlBulletinData = @"SELECT tm.RequiredStdTarget,tm.PlannedHoursPerDay, tm.MaxNoOfWS,sum(d.TotalSPT) AS SPT
									,SUM(D.AllotedWorkstation) AS TotalWS
									  FROM trn.ProductionBulletinTemplate AS T
									INNER JOIN  trn.ProductionBulletinTemplateMaster AS TM ON t.Id=tm.ProductionBulletinTemplateId
									INNER JOIN trn.ProductionBulletinTemplateDetail AS D ON d.ProductionBulletinTemplateMasterId=TM.Id
									WHERE t.ProductionOrderId='" + productionOrderID + "' AND TM.ProcessId='" + baseprocessid + @"'
									GROUP BY tm.RequiredStdTarget,tm.PlannedHoursPerDay, tm.MaxNoOfWS";

            return Json(new { MainData = _sqlRepository.GetDataCollection(sql), BulletinData = _sqlRepository.GetDataCollection(sqlBulletinData) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionRecipeMaterialList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionRecipeMaterialList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionOrderProcessSetList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionOrderProcessSetList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionOrderEntityList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionOrderEntityList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetWorkCenterList(string entityIds, string processid)
        {
            DataTable dt = _sqlRepository.GetDataTable(EntityList());
            string[] entityidList = new string[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
                entityidList[i] = dt.Rows[i]["Id"].ToString();

            return Json(_productionOrderService.GetWorkCenterList(entityidList, processid), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetType2WorkCenterList(string entityIds, string processid, string wcgId)
        {
            DataTable dt = _sqlRepository.GetDataTable(EntityList());
            string[] entityidList = new string[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
                entityidList[i] = dt.Rows[i]["Id"].ToString();

            return Json(_productionOrderService.GetType2WorkCenterList(entityidList, processid, wcgId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetWorkCenterNewList(string entityIds, string processid)
        {
            DataTable dt = _sqlRepository.GetDataTable(EntityList());
            string[] entityidList = new string[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
                entityidList[i] = dt.Rows[i]["Id"].ToString();

            return Json(_productionOrderService.GetWorkCenterList(entityidList, processid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductWorkCenterList(string productId, string entityId, string processid)
        {
            string sql = @"SELECT wc.* FROM   [SCS].[WorkCenterMasterProductPriority] PP
                                INNER JOIN scs.WorkCenterMaster AS WC ON wc.id=pp.WorkCenterMasterId
                                WHERE WC.ProcessId='" + processid + "' AND pp.ProductMasterId='" + productId + "' AND WC.EntityId='" + entityId + @"'
                                ORDER BY pp.[Priority]";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductWorkCenterNewList(string productId, string entityId, string processid)
        {
            string sql = @"SELECT wc.* FROM   [SCS].[WorkCenterMasterProductPriority] PP
                                INNER JOIN scs.WorkCenterMaster AS WC ON wc.id=pp.WorkCenterMasterId
                                WHERE WC.ProcessId='" + processid + "' AND pp.ProductMasterId='" + productId + "' AND WC.EntityId='" + entityId + @"'
                                ORDER BY pp.[Priority]";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionOrderWorkCenterList(string productionOrderId)
        {
            var _sql = @"SELECT PWCM.Id,e.UserName AS Entity,p.UserName AS Plant, PWCM.ProductionOrderId, PWCM.WorkCenterMasterId, WCM.Code, WCM.UserName
            FROM[TRN].[ProductionOrderWorkCenter] AS PWCM
                           JOIN[SCS].[WorkCenterMaster] AS WCM ON PWCM.WorkCenterMasterId = WCM.Id
                           INNER JOIN org.Entity AS e ON e.Id = wcm.EntityId
                           INNER JOIN org.Plant AS p ON p.Id = wcm.PlantId
                           WHERE PWCM.ProductionOrderId = '" + productionOrderId + "' ORDER BY p.UserName,e.UserName,wcm.sequence";

            return Json(_sqlRepository.GetDataCollection(_sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetRunningOrderWorkCenterList(string productionOrderId)
        {

            var _sql = @"SELECT PWCM.Id,e.UserName AS Entity,p.UserName AS Plant, PWCM.ProductionOrderId, PWCM.WorkCenterMasterId, WCM.Code, WCM.UserName,PWCM.isResidualApplicable,PWCM.Qty
                                FROM [TRN].[RunningOrderWorkCenter] AS PWCM
                                JOIN [SCS].[WorkCenterMaster] AS WCM ON PWCM.WorkCenterMasterId = WCM.Id
                                INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                                INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                                WHERE PWCM.ProductionOrderId='" + productionOrderId + "' ORDER BY p.UserName,e.UserName,wcm.sequence";
            return Json(_sqlRepository.GetDataCollection(_sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProductionOrderSchedulingParametersType1 parameter, string ProductionStatusId, List<ProductionOrderWorkCenter> workcenterlist, List<RunningOrderWorkCenter> runningworkcenterlist)
        {

            try
            {
                try
                {
                    if (parameter.RunningOrderBlockSize == 0)
                        parameter.RunningOrderBlockSize = 1;
                }
                catch (Exception ex)
                {


                }
                saveData(parameter, ProductionStatusId, runningworkcenterlist, workcenterlist);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        //[HttpPost]
        //public JsonResult Edit(ProductionOrder master, IEnumerable<ProductionOrderDetail> detaillist
        //     , IEnumerable<ProductionOrderProcessSet> processSetlist
        //    , IEnumerable<ProductionOrderEntity> entitylist
        //    , IEnumerable<ProductionOrderWorkCenter> workcenterlist)
        //{
        //    _productionOrderService.UpdateGraph(master, detaillist, processSetlist, entitylist, workcenterlist);
        //    return Json(new { Message = AplosMessage.Insert });
        //}

        [Authorize]
        public JsonResult Delete(string masterid)
        {
            _productionOrderService.DeleteGraph(masterid);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost, Authorize]
        public JsonResult Menu()
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT 'FRAME-'+id AS id,UserName AS MenuText FROM [MMS].[MenuFrame]";
                var _dataM = _sqlRepository.GetDataCollection(sql);

                sql = @"SELECT 'GROUP-'+mg.id AS id,'FRAME-'+m.Id AS pid,mg.UserName AS MenuText FROM mms.MenuFrame M
CROSS JOIN [MMS].[MenuGroup] MG
UNION ALL
SELECT 'SUBGROUP-'+mg.id AS id,'GROUP-'+m.Id AS pid,mg.UserName AS MenuText FROM mms.[MenuGroup] M
CROSS JOIN [MMS].[MenuSubGroup] MG
UNION ALL
SELECT mm.MenuId AS id,'FRAME-'+mm.MenuFrameId AS pid,m.UserName AS MenuText
  FROM mst.MenuMaster AS mm 
  INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')='' AND ISNULL(mm.MenuSubGroupId,'')=''
UNION ALL
SELECT mm.MenuId AS id,'GROUP-'+mm.MenuGroupId AS pid,m.UserName AS MenuText
  FROM mst.MenuMaster AS mm 
  INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')<>'' AND ISNULL(mm.MenuSubGroupId,'')=''
UNION ALL
SELECT mm.MenuId AS id,'SUBGROUP-'+mm.MenuSubGroupId AS pid,m.UserName AS MenuText
  FROM mst.MenuMaster AS mm 
  INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')<>'' AND ISNULL(mm.MenuSubGroupId,'')<>''";
                var _dataC = _sqlRepository.GetDataCollection(sql);

                return Json(new { MASTER = _dataM, DATA = _dataC, Error = false, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        public string EntityList()
        {
            string sql = @"";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (identity.IsSysAdmin)
            {
                sql = @"SELECT distinct E.Id,E.PlantId,P.UserName AS PlantName,e.Code,e.UserName AS UserName
                        FROM [ORG].[Entity] E
                            LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E.Id
                            LEFT JOIN org.Plant AS p ON p.Id=e.PlantId
                            LEFT JOIN org.Company AS c ON c.Id=e.CompanyId
                            WHERE  e.Id IN (
                        SELECT ept.EntityId FROM hkp.EntityProcessTag AS ept WHERE ept.ProcessId IN (SELECT pt.BaseProcessId FROM PlanningTypes AS pt)) AND E.[Active]=1 AND e.CompanyId='" + identity.CompanyId + @"'
                        ORDER BY e.Code";

                return sql;
            }

            sql = @"SELECT  distinct E2.Id,e2.PlantId,P.UserName AS PlantName,e2.Code,e2.UserName AS UserName  FROM [SEC].[UserEntity] E
                        LEFT JOIN org.Entity AS e2 ON e2.Id=e.EntityId
                        LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E2.Id
                        LEFT JOIN org.Plant AS p ON p.Id=e.PlantId
                        LEFT JOIN org.Company AS c ON c.Id=e.CompanyId
                        WHERE E.UserId='" + identity.UserId + @"' AND e2.Id IN (
SELECT ept.EntityId FROM hkp.EntityProcessTag AS ept WHERE ept.ProcessId IN (SELECT pt.BaseProcessId FROM PlanningTypes AS pt)) AND E2.[Active]=1 ORDER BY E2.Code";
            return sql;
        }
        [HttpPost, Authorize]
        public JsonResult GetAllEntityForPlanningType1()
        {
            try
            {


                return Json(_sqlRepository.GetDataCollection(EntityList()), JsonRequestBehavior.AllowGet);


                throw new Exception("No entity configurations was found in the system for the current user");
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost, Authorize]
        public JsonResult GetAllEntityForPlanningType1Process(string processId)
        {
            try
            {
                string sql = @"";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                sql = @"SELECT e.* FROM PlanningTypes AS pt 
INNER JOIN [ORG].[Entity] E on E.id=pt.EntityId
WHERE PT.PlanningType='PlanningType1' AND pt.CompanyGroupId='"+identity.CompanyGroupId+"'  AND pt.PlantId='"+identity.PlantId+"' And pt.BaseProcessId='"+processId+"'";



                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost, Authorize]
        public JsonResult GetSPTEfficiencySlab(string EntityId)
        {
            try
            {


                return Json(_sqlRepository.GetDataCollection("SELECT * FROM SPTEfficiencySlab AS ss WHERE ss.PlantId=(SELECT TOP 1 PlantId FROM org.Entity AS e WHERE e.Id='" + EntityId + @"') ORDER BY Minimum,Maximum"), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }



        [HttpPost, Authorize]
        public JsonResult GetAllEntity()
        {
            try
            {

                string sql = @"";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (identity.IsSysAdmin)
                {
                    sql = @"SELECT distinct E.Id,E.PlantId,P.UserName AS PlantName,e.Code,e.UserName AS UserName
                        FROM [ORG].[Entity] E
                            LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E.Id
                            LEFT JOIN org.Plant AS p ON p.Id=e.PlantId
                            LEFT JOIN org.Company AS c ON c.Id=e.CompanyId
                            WHERE ECC.IsProductionEntity=1 AND E.[Active]=1 AND e.CompanyId='" + identity.CompanyId + @"'
                        ORDER BY e.Code";

                    return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                }

                sql = @"SELECT  distinct E2.Id,e2.PlantId,P.UserName AS PlantName,e2.Code,e2.UserName AS UserName  FROM [SEC].[UserEntity] E
                        LEFT JOIN org.Entity AS e2 ON e2.Id=e.EntityId
                        LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E2.Id
                        LEFT JOIN org.Plant AS p ON p.Id=e.PlantId
                        LEFT JOIN org.Company AS c ON c.Id=e.CompanyId
                        WHERE E.UserId='" + identity.UserId + @"' AND ECC.IsProductionEntity=1 AND E2.[Active]=1 ORDER BY E2.Code";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);


                throw new Exception("No entity configurations was found in the system for the current user");
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost, Authorize]
        public JsonResult GetEntity()
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.General.Organization.OrganizationAuthorization orgAuth = new Library.General.Organization.OrganizationAuthorization();
                return Json(orgAuth.GetEntityByUser(identity.PlantId, identity.UserId, identity.IsSysAdmin), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult GetPlanStatus()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.General.Organization.OrganizationAuthorization orgAuth = new Library.General.Organization.OrganizationAuthorization();
                return Json(orgAuth.GetPlanStatus(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetSampleReport()
        {

            var excelEngine = new ExcelEngine();
            var application = excelEngine.Excel;
            var workbook = application.Workbooks.Create(3);
            var sheet1 = workbook.Worksheets[0];

            sheet1[1, 1].Text = "Tarek";
            workbook.Version = ExcelVersion.Excel2013;


            workbook.SaveAs(DateTime.Now.ToString("yyMMdd") + " Payment Receipt Voucher.xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);


            return null;
        }

        #endregion

        #region freeze line


        [HttpGet, Authorize]
        public ActionResult FreezeConfig(string entityid)
        {
            try
            {
                string sql = "select * from TRN.FreezeConfigPlanningType1 where entityid='" + entityid + "'";


                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        [HttpGet, Authorize]
        public ActionResult SaveFreezeConfig(string entityid, string date)
        {
            try
            {
                if (bplib.clsWebLib.IsDateOK(date) == false)
                    throw new Exception("Invalid freeze date. Allowed format is dd-MMM-yyyy (eg. 01-Jan-2019)");

                if (Convert.ToDateTime(date) < DateTime.Now)
                    throw new Exception("Freeze date cannot be earlier than current date");


                string sql = "select * from TRN.FreezeConfigPlanningType1 where entityid='" + entityid + "'";

                DataSet dsData = null;

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.getDataSet(sql, out dsData);
                con.CommitTransaction();


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (dsData.Tables[0].Rows.Count == 0)
                {
                    string systemid = "";
                    bplib.clsGenID id = new bplib.clsGenID();
                    id.GenID("FREEZE CONFIG", out systemid);

                    DataRow dr = dsData.Tables[0].NewRow();

                    dr["Id"] = "FRZ-" + systemid;
                    dr["EntityId"] = entityid;
                    dr["FreezeDate"] = date;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dsData.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsData.Tables[0].Rows[0];

                    dr.BeginEdit();
                    dr["FreezeDate"] = date;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();

                }
                con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.SaveData(ref dsData);
                con.CommitTransaction();


                return Json(new { Error = false, Message = "Freeze date updated successfully" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        #endregion freeze line

        #region Production Plan Simulation
        private void saveData(ProductionOrderSchedulingParametersType1 data, string ProductionStatusId, List<RunningOrderWorkCenter> runningworkcenterlist, List<ProductionOrderWorkCenter> workcenterlist)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                if (bplib.clsWebLib.IsDateOK(data.LSD) == false)
                    throw new Exception("Invalid date format for LSD. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                if (bplib.clsWebLib.IsDateOK(data.CommitmentDate) == false)
                    throw new Exception("Invalid date format for Commitment Date. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                if (Convert.ToDateTime(data.CommitmentDate) < Convert.ToDateTime(data.LSD))
                    throw new Exception("Commitment date cannot be earlier than Late Start Date(LSD)");

                if (bplib.clsWebLib.IsDateOK(data.MainRawMaterialInhouseDate) == false)
                    throw new Exception("Invalid date format for main raw material inhouse date. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                if (bplib.clsWebLib.IsDateOK(data.OtherRawMaterialInhouseDate) == false)
                    throw new Exception("Invalid date format for other raw material inhouse date. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                if (Convert.ToDateTime(data.LSD) < Convert.ToDateTime(data.MainRawMaterialInhouseDate))
                    throw new Exception("LSD cannot be earlier than Main Raw Material Inhouse Date)");

                if (Convert.ToDateTime(data.LSD) < Convert.ToDateTime(data.OtherRawMaterialInhouseDate))
                    throw new Exception("LSD cannot be earlier than Other Raw Material Inhouse Date)");

                #region default running order workcenters
                //here validate and localize running order workcenters
                if (string.IsNullOrEmpty(ProductionStatusId))
                    throw new Exception("Production order status cannot be blank");

                DataTable dtTempStatus = _sqlRepository.GetDataTable("SELECT * FROM hkp.ProductionStatus AS ps WHERE ps.Id='" + ProductionStatusId + "'");


                if (dtTempStatus.Rows[0]["StandardName"].ToString().ToUpper() == PlanningStatus.RUNNING.ToString())
                {
                    if (runningworkcenterlist == null || runningworkcenterlist.Count == 0)
                    {

                        dtTempStatus = _sqlRepository.GetDataTable("SELECT DISTINCT ppt.WorkCenterMasterId FROM ProductionPlanningType1 AS ppt WHERE ppt.ProductionOrderID='" + data.ProductionOrderID + "'");
                        if (dtTempStatus.Rows.Count == 0)
                        {
                            if (workcenterlist == null || workcenterlist.Count == 0)
                                throw new Exception("Please provide running order workcenter preference as the production order has been marked as 'Running' and no plan data/workcenter preference found to generate workcenter preference for this order");

                            dtTempStatus = _sqlRepository.GetDataTable("SELECT DISTINCT ppt.WorkCenterMasterId FROM trn.ProductionOrderWorkCenter AS ppt WHERE 1=2");
                            foreach (var item in workcenterlist)
                            {
                                DataRow dr = dtTempStatus.NewRow();
                                dr["WorkCenterMasterId"] = item.WorkCenterMasterId;
                                dtTempStatus.Rows.Add(dr);
                            }
                        }

                        runningworkcenterlist = new List<RunningOrderWorkCenter>();
                        for (int i = 0; i < dtTempStatus.Rows.Count; i++)
                        {
                            runningworkcenterlist.Add(new RunningOrderWorkCenter { ProductionOrderId = data.ProductionOrderID.ToString(), WorkCenterMasterId = dtTempStatus.Rows[i]["WorkCenterMasterId"].ToString() });
                        }
                    }

                }

                #endregion default running order workcenters
                DataTable dtTempStatusNew = _sqlRepository.GetDataTable("SELECT * FROM hkp.ProductionStatus AS ps WHERE ps.Id='" + ProductionStatusId + "'");
                string ClosingDate = "NULL";
                if (dtTempStatusNew.Rows[0]["StandardName"].ToString().ToUpper() == productionOrderSchedulingParametersType1Controller.PlanningStatus.CLOSED.ToString())
                    ClosingDate = "'" + DateTime.Now.ToString() + "'";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();

                objCon.executeQuery("Update trn.ProductionOrder set ClosingDate=" + ClosingDate + @", ProductionStatusId='" + ProductionStatusId + "' where Id='" + data.ProductionOrderID + "'");


                objCon.CommitTransaction();


                string sql = "select * from ProductionOrderSchedulingParametersType1 where ProductionOrderID='" + data.ProductionOrderID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["ProductionOrderID"] = data.ProductionOrderID;

                    dr["NoOfWorkStation"] = data.NoOfWorkStation;
                    dr["Efficiency"] = data.Efficiency;
                    dr["SPT"] = data.SPT;


                    dr["PlanWorkingHoursPerDay"] = data.PlanWorkingHoursPerDay;
                    dr["FirstDayOutPut"] = data.FirstDayOutPut;
                    //dr["PlanTargetPerHour"] = data.PlanTargetPerHour;
                    dr["IncrementValue"] = data.IncrementValue;
                    dr["IncrementType"] = data.IncrementType;
                    dr["DayToReachTheTarget"] = data.DayToReachTheTarget;
                    dr["RunningOrderBlockSize"] = data.RunningOrderBlockSize;


                    dr["WCPreferenceType"] = data.WCPreferenceType;

                    dr["LSD"] = data.LSD;
                    dr["CommitmentDate"] = data.CommitmentDate;

                    dr["MainRawMaterialInhouseDate"] = data.MainRawMaterialInhouseDate;
                    dr["OtherRawMaterialInhouseDate"] = data.OtherRawMaterialInhouseDate;

                    dr["ConsiderHourFromWorkCenter"] = data.ConsiderHourFromWorkCenter;
                    dr["ConsiderWorkStationsFromWorkCenter"] = data.ConsiderWorkStationsFromWorkCenter;

                    dr["ProductionPriority"] = data.ProductionPriority;
                    dr["TargetPerHour"] = data.TargetPerHour;
                    dr["TargetPerDay"] = data.TargetPerDay;
                    dr["MinimumLineDays"] = data.MinimumLineDays;
                    dr["RequiredLineDays"] = data.RequiredLineDays;
                    dr["RequiredNoOfLines"] = data.RequiredNoOfLines;
                    dr["AllocatedLines"] = data.AllocatedLines;

                    dr["Color"] = bplib.clsWebLib.RetValidLen(data.Color);
                    dr["Qty"] = data.Qty;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);


                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["NoOfWorkStation"] = data.NoOfWorkStation;
                    dr["Efficiency"] = data.Efficiency;
                    dr["SPT"] = data.SPT;


                    dr["PlanWorkingHoursPerDay"] = data.PlanWorkingHoursPerDay;
                    dr["FirstDayOutPut"] = data.FirstDayOutPut;
                    //dr["PlanTargetPerHour"] = data.PlanTargetPerHour;
                    dr["IncrementValue"] = data.IncrementValue;
                    dr["IncrementType"] = data.IncrementType;
                    dr["DayToReachTheTarget"] = data.DayToReachTheTarget;

                    dr["WCPreferenceType"] = data.WCPreferenceType;
                    dr["RunningOrderBlockSize"] = data.RunningOrderBlockSize;

                    dr["LSD"] = data.LSD;
                    dr["CommitmentDate"] = data.CommitmentDate;

                    dr["MainRawMaterialInhouseDate"] = data.MainRawMaterialInhouseDate;
                    dr["OtherRawMaterialInhouseDate"] = data.OtherRawMaterialInhouseDate;

                    dr["ConsiderHourFromWorkCenter"] = data.ConsiderHourFromWorkCenter;
                    dr["ConsiderWorkStationsFromWorkCenter"] = data.ConsiderWorkStationsFromWorkCenter;


                    dr["ProductionPriority"] = data.ProductionPriority;
                    dr["TargetPerHour"] = data.TargetPerHour;
                    dr["TargetPerDay"] = data.TargetPerDay;
                    dr["MinimumLineDays"] = data.MinimumLineDays;
                    dr["RequiredLineDays"] = data.RequiredLineDays;
                    dr["RequiredNoOfLines"] = data.RequiredNoOfLines;
                    dr["AllocatedLines"] = data.AllocatedLines;

                    dr["Color"] = bplib.clsWebLib.RetValidLen(data.Color);
                    dr["Qty"] = data.Qty;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();

                }

                DataSet dsWorkcenter = null;
                sql = "SELECT * FROM [TRN].[ProductionOrderWorkCenter] where ProductionOrderID='" + data.ProductionOrderID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsWorkcenter, false, "1");

                if (workcenterlist == null)
                {
                    while (dsWorkcenter.Tables[0].DefaultView.Count > 0)
                        dsWorkcenter.Tables[0].DefaultView[0].Delete();
                }
                else
                {

                    for (int i = 0; i < dsWorkcenter.Tables[0].Rows.Count; i++)
                    {
                        List<ProductionOrderWorkCenter> filterdata = workcenterlist.Where(ee => ee.WorkCenterMasterId == dsWorkcenter.Tables[0].Rows[i]["WorkcenterMasterID"].ToString()).ToList();
                        if (filterdata == null || filterdata.Count == 0)
                        {
                            dsWorkcenter.Tables[0].Rows[i].Delete();
                        }

                    }

                    string SystemID = "";
                    for (int i = 0; i < workcenterlist.Count; i++)
                    {
                        dsWorkcenter.Tables[0].DefaultView.RowFilter = "WorkcenterMasterID='" + workcenterlist[i].WorkCenterMasterId + "'";
                        if (dsWorkcenter.Tables[0].DefaultView.Count == 0)
                        {
                            if (SystemID == "")
                            {
                                bplib.clsGenID id = new bplib.clsGenID();
                                id.GenID(System.DateTime.Now.ToShortDateString(), "PR WC", out SystemID);
                            }
                            DataRow dr = dsWorkcenter.Tables[0].NewRow();

                            dr["id"] = SystemID + "-" + (i + 1).ToString();
                            dr["ProductionOrderID"] = data.ProductionOrderID;
                            dr["WorkcenterMasterID"] = workcenterlist[i].WorkCenterMasterId;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dsWorkcenter.Tables[0].Rows.Add(dr);
                        }
                    }

                }

                DataSet dsRunningWorkcenter = null;
                sql = "SELECT * FROM [TRN].[RunningOrderWorkCenter] where ProductionOrderID='" + data.ProductionOrderID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRunningWorkcenter, false, "1");

                if (runningworkcenterlist == null)
                {
                    while (dsRunningWorkcenter.Tables[0].DefaultView.Count > 0)
                        dsRunningWorkcenter.Tables[0].DefaultView[0].Delete();
                }
                else
                {

                    for (int i = 0; i < dsRunningWorkcenter.Tables[0].Rows.Count; i++)
                    {
                        List<RunningOrderWorkCenter> filterdata = runningworkcenterlist.Where(ee => ee.WorkCenterMasterId == dsRunningWorkcenter.Tables[0].Rows[i]["WorkcenterMasterID"].ToString()).ToList();
                        if (filterdata == null || filterdata.Count == 0)
                        {
                            dsRunningWorkcenter.Tables[0].Rows[i].Delete();
                        }

                    }

                    string SystemID = "";
                    for (int i = 0; i < runningworkcenterlist.Count; i++)
                    {
                        dsRunningWorkcenter.Tables[0].DefaultView.RowFilter = "WorkcenterMasterID='" + runningworkcenterlist[i].WorkCenterMasterId + "'";
                        if (dsRunningWorkcenter.Tables[0].DefaultView.Count == 0)
                        {
                            if (SystemID == "")
                            {
                                bplib.clsGenID id = new bplib.clsGenID();
                                id.GenID(System.DateTime.Now.ToShortDateString(), "PRUNNING WC", out SystemID);
                            }
                            DataRow dr = dsRunningWorkcenter.Tables[0].NewRow();

                            dr["id"] = SystemID + "-" + (i + 1).ToString();
                            dr["ProductionOrderID"] = data.ProductionOrderID;
                            dr["WorkcenterMasterID"] = runningworkcenterlist[i].WorkCenterMasterId;
                            dr["isResidualApplicable"] = bplib.clsWebLib.GetBoolData(runningworkcenterlist[i].isResidualApplicable);
                            dr["Qty"] = clsStaticInfo.dbl(runningworkcenterlist[i].Qty);

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dsRunningWorkcenter.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dsRunningWorkcenter.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["isResidualApplicable"] = bplib.clsWebLib.GetBoolData(runningworkcenterlist[i].isResidualApplicable);
                            dr["Qty"] = clsStaticInfo.dbl(runningworkcenterlist[i].Qty);
                            dr.EndEdit();

                        }
                    }

                }


                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsRunningWorkcenter, dsWorkcenter);

                Library.Service.TaskScheduler.TaskScheduler schedule = new Library.Service.TaskScheduler.TaskScheduler(_sqlRepository);
                schedule.UpdateTaskStatus();
                //Production Order Related Tasks
                string tsql = @"SELECT distinct TaskTemplateMasterId,P.EntityId  FROM trn.MasterOrder AS mo 
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
								inner join trn.ProductionOrderDetail D ON D.SalesOrderId=SO.Id
                                inner join trn.ProductionOrder P ON P.Id=D.ProductionOrderId
                           WHERE d.ProductionOrderId='" + data.ProductionOrderID + "'";
                DataTable dtSO = _sqlRepository.GetDataTable(tsql);
                string TaskTemplateMasterId = dtSO.Rows[0]["TaskTemplateMasterId"].ToString();

                DataTable dt = schedule.GetDataSourceProdOrderNew(data.ProductionOrderID, dtSO.Rows[0]["EntityId"].ToString(), TaskAppliedOnEnum.ProductionOrder);
                if (dt.Rows.Count > 0)
                    schedule.MakeTNAMaster(dt, data.ProductionOrderID, TaskAppliedOnEnum.ProductionOrder);

            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }
        // private ActionResult ProductionPlanSimulation(string productionOrderID, string processID)
        int DaysToBeAddedForLineChange = 3;//VG Sir
        StringBuilder sbLog = new StringBuilder();

        // string entityid = "4";
        [HttpGet]
        public async Task<ActionResult> ProductionPlanSimulationNew(string entityid, string processid)
        {
            return await Task.Factory.StartNew(() =>
            {
                try
                {

                    string EntityIds = "" + entityid + "";
                    string _sql = @"SELECT distinct WCM.EntityId
                                  from (SELECT distinct W.ProductionOrderId,W.WorkCenterMasterId FROM trn.ProductionOrderWorkCenter AS W
                                UNION
                                SELECT distinct W.ProductionOrderId,W.WorkCenterMasterId FROM trn.RunningOrderWorkCenter AS W
                                ) AS W
                                JOIN trn.ProductionOrder AS po ON po.Id=w.ProductionOrderId
                                join scs.WorkCenterMaster AS wcm ON wcm.Id=w.WorkCenterMasterId
                                INNER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId
                                WHERE  (po.EntityId IN(" + entityid + @") OR WCM.EntityId IN(" + entityid + @")) 
                            AND ps.UserName NOT IN ('" + PlanningStatus.CLOSED.ToString() + @"')
UNION
SELECT distinct po.EntityId FROM 
trn.ProductionOrderWorkCenter W
JOIN trn.ProductionOrder AS po ON W.ProductionOrderId=po.Id
JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=w.WorkCenterMasterId
INNER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId
WHERE WCM.EntityId IN(" + entityid + @") AND ps.UserName NOT IN ('" + PlanningStatus.CLOSED.ToString() + @"')
UNION
SELECT distinct po.EntityId FROM 
trn.RunningOrderWorkCenter W
JOIN trn.ProductionOrder AS po ON W.ProductionOrderId=po.Id
JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=w.WorkCenterMasterId
INNER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId
WHERE WCM.EntityId IN(" + entityid + @") AND ps.UserName NOT IN ('" + PlanningStatus.CLOSED.ToString() + @"')";

                    DataTable dt = _sqlRepository.GetDataTable(_sql);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        EntityIds += ",'" + dt.Rows[i]["EntityId"].ToString() + "'";
                    }
                    ProductionPlanSimulationAlgorithm(entityid, EntityIds, processid, out DataTable productionOrders);


                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        Error = true,
                        Message = ex.Message
                    }, JsonRequestBehavior.AllowGet);
                }
                finally
                {

                }

                return Json(new { Error = false, Message = "Success" }, JsonRequestBehavior.AllowGet);
            });
        }



        [HttpGet]
        public async Task<ActionResult> ProductionPlanSimulation(string entityid, string processid)
        {
            return await Task.Factory.StartNew(() =>
            {
                var po = "";
                try
                {

                    string EntityIds = "'" + entityid + "'";
                    string _sql = @"SELECT distinct WCM.EntityId
                                                      from (SELECT distinct W.ProductionOrderId,W.WorkCenterMasterId FROM trn.ProductionOrderWorkCenter AS W
                                                    UNION
                                                    SELECT distinct W.ProductionOrderId,W.WorkCenterMasterId FROM trn.RunningOrderWorkCenter AS W
                                                    ) AS W
                                                    JOIN trn.ProductionOrder AS po ON po.Id=w.ProductionOrderId
                                                    join scs.WorkCenterMaster AS wcm ON wcm.Id=w.WorkCenterMasterId
                                                    INNER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId
                                                    WHERE  (po.EntityId='" + entityid + @"' AND WCM.EntityId='" + entityid + @"') 
                                                AND ps.UserName NOT IN ('" + PlanningStatus.CLOSED.ToString() + @"')
                    UNION
                    SELECT distinct po.EntityId FROM 
                    trn.ProductionOrderWorkCenter W
                    JOIN trn.ProductionOrder AS po ON W.ProductionOrderId=po.Id
                    JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=w.WorkCenterMasterId
                    INNER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId
                    WHERE WCM.EntityId='" + entityid + @"' AND PO.EntityId='" + entityid + @"' AND ps.UserName NOT IN ('" + PlanningStatus.CLOSED.ToString() + @"')
                    UNION
                    SELECT distinct po.EntityId FROM 
                    trn.RunningOrderWorkCenter W
                    JOIN trn.ProductionOrder AS po ON W.ProductionOrderId=po.Id
                    JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=w.WorkCenterMasterId
                    INNER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId
                    WHERE WCM.EntityId='" + entityid + @"' AND PO.EntityId='" + entityid + @"'  AND ps.UserName NOT IN ('" + PlanningStatus.CLOSED.ToString() + @"')";

                    DataTable dt = _sqlRepository.GetDataTable(_sql);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        EntityIds += ",'" + dt.Rows[i]["EntityId"].ToString() + "'";
                    }
                    ProductionPlanSimulationAlgorithm(entityid, EntityIds, processid, out DataTable productionOrders);

                    Library.Service.TaskScheduler.TaskScheduler schedule = new Library.Service.TaskScheduler.TaskScheduler(_sqlRepository);
                    schedule.UpdateTaskStatus();
                    //Production Order Related Tasks

                    for (int i = 0; i < productionOrders.Rows.Count; i++)
                    {
                        po = productionOrders.Rows[i]["ProductionOrderID"].ToString();

                        string sql = @"SELECT distinct TaskTemplateMasterId FROM trn.MasterOrder AS mo 
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                           WHERE so.id IN(Select SalesOrderId From TRN.ProductionOrderDetail Where ProductionOrderId='" + productionOrders.Rows[i]["ProductionOrderID"].ToString() + "')";
                        DataTable dtSO = _sqlRepository.GetDataTable(sql);
                        string TaskTemplateMasterId = dtSO.Rows[0]["TaskTemplateMasterId"].ToString();

                        DataTable dtt = schedule.GetDataSourceMasterOrderNew(productionOrders.Rows[i]["ProductionOrderID"].ToString(), TaskAppliedOnEnum.ProductionOrder);
                        if (dtt.Rows.Count > 0)
                            schedule.MakeTNAMaster(dtt, productionOrders.Rows[i]["ProductionOrderID"].ToString(), TaskAppliedOnEnum.ProductionOrder);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        Error = true,
                        Message = ex.Message
                    }, JsonRequestBehavior.AllowGet);
                }
                finally
                {

                }

                return Json(new { Error = false, Message = "Success" }, JsonRequestBehavior.AllowGet);
            });
        }



        public void ProductionPlanSimulationAlgorithm(string entityid, string ProcessingEntities, string processid, out DataTable productionOrders)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            productionOrders = null;
            DataSet dsToData;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

            con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[ProductionPlanningArchive] WHERE 1=2", out dsToData, false, "1");
            DataTable dtFromData = _sqlRepository.GetDataTable(@"select * FROM ProductionPlanningType1 where EntityId IN (" + ProcessingEntities + @") AND CAST(AddedDate AS DATE) < CAST(GETDATE() AS DATE)");
            for (int j = 0; j < dtFromData.DefaultView.Count; j++)
            {
                DataRow drData = dsToData.Tables[0].NewRow();
                CopyRow(dtFromData.DefaultView[j].Row, ref drData);
                dsToData.Tables[0].Rows.Add(drData);
            }

            clsStaticInfo clsStatic = new clsStaticInfo();
            clsStatic.SaveDataSets(dsToData);

            Library.General.Setups.ProcessLock _lock = new Library.General.Setups.ProcessLock(identity.Name, Library.General.Setups.ProcessLockId.PlanningType1, entityid);
            _lock.LockProcess();
            try
            {
                SendNotification("-------------Starting Simulation-------------");
                DataTable dtWCValidation = _sqlRepository.GetDataTable(@"SELECT wcm.Id,ed.StartDate,wcm.UserName FROM scs.WorkCenterMaster AS wcm 
                                        LEFT JOIN scs.WorkCenterMasterEffectiveDate AS ED ON ed.WorkCenterMasterId=wcm.Id AND ed.Id=(SELECT TOP 1 Id FROM scs.WorkCenterMasterEffectiveDate WHERE WorkCenterMasterId=wcm.Id ORDER BY StartDate DESC)
                                        WHERE wcm.EntityId IN (" + ProcessingEntities + @")  AND wcm.ProcessId='" + processid + @"'  AND wcm.Active=1");

                if (dtWCValidation.Rows.Count == 0)
                    throw new Exception("No workcenter found. Please create workcenters and try again");

                string WithoutEffectiveDate = "";
                for (int i = 0; i < dtWCValidation.Rows.Count; i++)
                {
                    if (dtWCValidation.Rows[i]["StartDate"].ToString() != "")
                    {
                        WithoutEffectiveDate = dtWCValidation.Rows[i]["StartDate"].ToString();
                        break;
                    }
                }

                if (WithoutEffectiveDate == "")
                    throw new Exception("No workcenter was found with effective date. Please set effective date for workcenters");

                Dictionary<string, DataTable> dicWorkCenterRunningHours = WorkCenterRunningHours();

                //first close all production order having all sales order closed
                _sqlRepository.ExecuteSqlCommand(@"UPDATE  trn.ProductionOrder SET ProductionStatusId = (SELECT TOP 1 Id FROM hkp.ProductionStatus AS ps WHERE ps.StandardName='Closed')
                                                FROM trn.ProductionOrder PO 

                                                WHERE PO.Id IN (
			                                                SELECT PO.Id AS ProductionOrderId FROM trn.ProductionOrder AS po 
			                                                INNER JOIN hkp.ProductionStatus ps ON ps.Id=po.ProductionStatusId
			                                                LEFT OUTER JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=po.Id
			                                                AND pod.Id = (
			                                                SELECT TOP 1 pod.Id FROM trn.ProductionOrderDetail AS pod
			                                                INNER JOIN trn.SalesOrder AS so ON so.id=pod.SalesOrderId 
			                                                INNER JOIN hkp.OrderStatus AS os ON os.Id=so.OrderStatusId
			                                                WHERE os.Id='" + Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"' AND pod.ProductionOrderId=po.Id)
			                                                WHERE ISNULL(pod.Id,'')='' AND ps.StandardName<>'Closed'
                                                ) AND po.EntityId IN (" + ProcessingEntities + @")");

                _sqlRepository.ExecuteSqlCommand(@"DELETE FROM ProductionPlanningType1 WHERE ProductionOrderID IN (
                                                SELECT po.Id FROM trn.ProductionOrder AS po
                                                INNER JOIN ProductionPlanningType1 AS ppt ON ppt.ProductionOrderID=po.Id
                                                INNER JOIN hkp.ProductionStatus AS ps ON po.ProductionStatusId=ps.Id
                                                WHERE ps.UserName='" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"' AND po.EntityId IN (" + ProcessingEntities + @")
                                                )");

                string runningsql = @"SELECT DISTINCT po.Id FROM trn.ProductionOrder AS po
                                    LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                                    LEFT OUTER JOIN trn.RunningOrderWorkCenter AS r ON po.Id=r.ProductionOrderId
                                    WHERE ISNULL(r.Id,'')='' AND ps.UserName='running' AND po.EntityId IN (" + ProcessingEntities + @")";
                DataTable dtCheck = _sqlRepository.GetDataTable(runningsql);
                if (dtCheck.Rows.Count > 0)
                {
                    string ids = "";
                    for (int i = 0; i < dtCheck.Rows.Count; i++)
                    {
                        if (ids == "")
                            ids = dtCheck.Rows[i]["id"].ToString();
                        else
                            ids += "," + dtCheck.Rows[i]["id"].ToString();
                    }

                    new Exception("The following production orders are running but no workcenter was defined: " + ids);
                }

                _sqlRepository.ExecuteSqlCommand(@"delete FROM ProductionPlanningType1 where EntityId IN (" + ProcessingEntities + @")");

                Dictionary<string, double> DicBalanceWorkcenterHours = new Dictionary<string, double>();

                DataTable dtWorkCenter = dtAllAvailableWrokcenters(ProcessingEntities, processid);
                dtWorkCenter.Columns.Add("CURRENT_APPLICABLE");
                dtWorkCenter.Columns.Add("ACTUAL_APPLICABLE");
                dtWorkCenter.Columns.Add("AlreadyBooked", typeof(double));
                dtWorkCenter.Columns.Add("isResidualApplicable", typeof(bool));
                dtWorkCenter.DefaultView.RowFilter = null;
                DataTable dvDistinctEntity = dtWorkCenter.DefaultView.ToTable(true, "EntityId");

                Dictionary<string, DataTable> dicCalendar = dtProductionCalendar(System.DateTime.Now, 1500, processid, ProcessingEntities);
                DataTable dtCalendar = new DataTable("Temp");
                productionOrders = dtProductionParameters(ProcessingEntities);
                for (int i = 0; i < productionOrders.Rows.Count; i++)
                {
                    var poid = productionOrders.Rows[i]["ProductionOrderID"].ToString();
                    if (poid == "2519")
                    {

                    }
                    dtCalendar = dicCalendar[productionOrders.Rows[i]["EntityId"].ToString()];

                    sbLog = new StringBuilder();
                    SendNotification("Simulating production order#" + productionOrders.Rows[i]["ProductionOrderID"].ToString(), i, productionOrders.Rows.Count);
                    sbLog.AppendLine("Starting simulation for production order#" + productionOrders.Rows[i]["ProductionOrderID"].ToString());
                    DateTime startDate = Convert.ToDateTime(Convert.ToDateTime(productionOrders.Rows[i]["LSD"].ToString()).ToString("dd-MMM-yyyy"));
                    DateTime LSD = Convert.ToDateTime(Convert.ToDateTime(productionOrders.Rows[i]["LSD"].ToString()).ToString("dd-MMM-yyyy"));
                    double DaysToReachTheTarget = clsStaticInfo.dbl(productionOrders.Rows[i]["DayToReachTheTarget"].ToString());
                    DaysToBeAddedForLineChange = (int)DaysToReachTheTarget - 1;

                    DataTable dtCurrentWorkCenter = dtAvailableWrokcenters(productionOrders.Rows[i]["ProductionOrderID"].ToString(), productionOrders.Rows[i]["ProductionStatusName"].ToString(), processid);

                    StringCollection strColMultipleProductionInSingleLine = new StringCollection();
                    if (dtCurrentWorkCenter.Rows.Count == 0)
                    {
                        foreach (DataRow item in dtWorkCenter.Rows)
                        {
                            if (strColMultipleProductionInSingleLine.Contains(item["WorkCenterMasterId"].ToString()) == false)
                            {
                                strColMultipleProductionInSingleLine.Add(item["WorkCenterMasterId"].ToString());


                                dtWorkCenter.DefaultView.RowFilter = "WorkCenterMasterId='" + item["WorkCenterMasterId"].ToString() + "'";
                                if (dtWorkCenter.DefaultView.Count == 1)
                                {
                                    item["CURRENT_APPLICABLE"] = "YES";
                                    item["ACTUAL_APPLICABLE"] = "YES";
                                    item["AlreadyBooked"] = 0;

                                }
                                else if (dtWorkCenter.DefaultView.Count > 1)
                                {
                                    dtWorkCenter.DefaultView.RowFilter = "WorkCenterMasterId='" + item["WorkCenterMasterId"].ToString() + "' AND MaterialMasterId='" + productionOrders.Rows[i]["MaterialMasterId"].ToString() + "'";
                                    if (dtWorkCenter.DefaultView.Count == 1)
                                    {
                                        dtWorkCenter.DefaultView[0].Row["CURRENT_APPLICABLE"] = "YES";
                                        dtWorkCenter.DefaultView[0].Row["ACTUAL_APPLICABLE"] = "YES";
                                        item["AlreadyBooked"] = 0;
                                    }
                                    else
                                    {
                                        dtWorkCenter.DefaultView.RowFilter = "WorkCenterMasterId='" + item["WorkCenterMasterId"].ToString() + "'";
                                        dtWorkCenter.DefaultView[0].Row["CURRENT_APPLICABLE"] = "YES";
                                        dtWorkCenter.DefaultView[0].Row["ACTUAL_APPLICABLE"] = "YES";
                                        item["AlreadyBooked"] = 0;
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        for (int WC = 0; WC < dtCurrentWorkCenter.Rows.Count; WC++)
                        {
                            if (strColMultipleProductionInSingleLine.Contains(dtCurrentWorkCenter.Rows[WC]["WorkCenterMasterId"].ToString()) == false)
                            {
                                strColMultipleProductionInSingleLine.Add(dtCurrentWorkCenter.Rows[WC]["WorkCenterMasterId"].ToString());

                                dtWorkCenter.DefaultView.RowFilter = "WorkCenterMasterId='" + dtCurrentWorkCenter.Rows[WC]["WorkCenterMasterId"].ToString() + "'";
                                if (dtWorkCenter.DefaultView.Count == 1)
                                {
                                    dtWorkCenter.DefaultView[0].Row["CURRENT_APPLICABLE"] = "YES";
                                    dtWorkCenter.DefaultView[0].Row["ACTUAL_APPLICABLE"] = "YES";
                                    dtWorkCenter.DefaultView[0].Row["AlreadyBooked"] = clsStaticInfo.dbl(dtCurrentWorkCenter.Rows[WC]["AlreadyBooked"].ToString());
                                    dtWorkCenter.DefaultView[0].Row["CurrentPRQty"] = clsStaticInfo.dbl(dtCurrentWorkCenter.Rows[WC]["CurrentPRQty"].ToString());
                                    dtWorkCenter.DefaultView[0].Row["isResidualApplicable"] = dtCurrentWorkCenter.Rows[WC]["isResidualApplicable"];

                                }
                                else if (dtWorkCenter.DefaultView.Count > 1)
                                {

                                    DataRow[] dr = dtWorkCenter.Select("WorkCenterMasterId='" + dtCurrentWorkCenter.Rows[WC]["WorkCenterMasterId"].ToString() + "' AND MaterialMasterId='" + productionOrders.Rows[i]["MaterialMasterId"].ToString() + "'");
                                    if (dr.Length == 1)
                                    {
                                        dr[0]["CURRENT_APPLICABLE"] = "YES";
                                        dr[0]["ACTUAL_APPLICABLE"] = "YES";
                                        dr[0]["AlreadyBooked"] = clsStaticInfo.dbl(dtCurrentWorkCenter.Rows[WC]["AlreadyBooked"].ToString());
                                        dr[0]["CurrentPRQty"] = clsStaticInfo.dbl(dtCurrentWorkCenter.Rows[WC]["CurrentPRQty"].ToString());
                                        dr[0]["isResidualApplicable"] = dtCurrentWorkCenter.Rows[WC]["isResidualApplicable"];


                                    }
                                    else
                                    {
                                        dr = dtWorkCenter.Select("WorkCenterMasterId='" + dtCurrentWorkCenter.Rows[WC]["WorkCenterMasterId"].ToString() + "'");
                                        dr[0]["CURRENT_APPLICABLE"] = "YES";
                                        dr[0]["ACTUAL_APPLICABLE"] = "YES";
                                        dr[0]["AlreadyBooked"] = clsStaticInfo.dbl(dtCurrentWorkCenter.Rows[WC]["AlreadyBooked"].ToString());
                                        dr[0]["CurrentPRQty"] = clsStaticInfo.dbl(dtCurrentWorkCenter.Rows[WC]["CurrentPRQty"].ToString());
                                        dr[0]["isResidualApplicable"] = dtCurrentWorkCenter.Rows[WC]["isResidualApplicable"];

                                    }

                                }
                            }

                        }

                    }

                    StringCollection strMaxAllocatedLines = new StringCollection();
                    double AllocatedLines = clsStaticInfo.dbl(productionOrders.Rows[i]["AllocatedLines"].ToString()); sbLog.AppendLine("Total Allocated Lines" + AllocatedLines);
                    double TotalLineDays = Math.Ceiling(clsStaticInfo.dbl(productionOrders.Rows[i]["RequiredLineDays"].ToString()));
                    double MinimumLineDays = clsStaticInfo.dbl(productionOrders.Rows[i]["MinimumLineDays"].ToString());


                    if (productionOrders.Rows[i]["ProductionStatusName"].ToString().ToUpper() == PlanningStatus.RUNNING.ToString())
                    {
                        AllocatedLines = clsStaticInfo.dbl(dtWorkCenter.Compute("COUNT(WorkCenterMasterId)", "CURRENT_APPLICABLE='YES'").ToString());
                        MinimumLineDays = clsStaticInfo.dbl(productionOrders.Rows[i]["RunningOrderBlockSize"].ToString());
                        DaysToBeAddedForLineChange = (int)MinimumLineDays;
                    }
                    sbLog.AppendLine("Minimum workcenter days" + MinimumLineDays);

                    double TotalOrderQuantity = (int)clsStaticInfo.dbl(productionOrders.Rows[i]["SOQuantity"].ToString()); sbLog.AppendLine("Total order qty" + TotalOrderQuantity);
                    double TempTotalOrderQty = TotalOrderQuantity;


                    double TargetPerDay = (int)clsStaticInfo.dbl(productionOrders.Rows[i]["TargetPerDay"].ToString());
                    List<ProductionBlock> _ProductionBlock = new List<ProductionBlock>();
                    StringCollection sbNoOfLineUtilization = new StringCollection();

                    int dayCount = 0;
                    string LastProductionLineID = "";
                    DataRow BestLine = null;
                    int Index = -1;
                    DateTime LSDForLine = startDate;
                    bool isBuildUpRequired = false;
                    int blockCount = 0;
                    while (TotalOrderQuantity > 0)
                    {

                        Index++;
                        bool isStyleChanged = false;
                        if (dayCount % MinimumLineDays == 0)
                        {
                            BestLine = null;//to determine the best line for each rotation, ignoring residual values plotting on the same line. delete this line if you want to assign residial value for current best line
                            isBuildUpRequired = false;
                            blockCount++;
                            sbLog.AppendLine("Start plotting block" + blockCount);
                            Index = 0;//important, resetting the calendar
                            DateTime tempdate = LSD;

                            GetPrefferedWorkcenter(dtWorkCenter, ref TotalOrderQuantity, TempTotalOrderQty);
                            #region Scan for each date starting from it's LSD to maximum block date to get the available best line to fit starting date


                            int tempCalendarIndex = -1;
                            do
                            {
                                //GetPrefferedWorkcenter(dtWorkCenter, ref TotalOrderQuantity, TempTotalOrderQty);

                                tempCalendarIndex++;
                                startDate = Convert.ToDateTime(Convert.ToDateTime(productionOrders.Rows[i]["LSD"].ToString()).ToString("dd-MMM-yyyy"));
                                //predict whether the last portion is less or equal to minimum line days
                                //because we the don't want to change the line

                                if (BestLine != null)
                                {
                                    dtCalendar = dicCalendar[BestLine["EntityId"].ToString()];

                                    dtCalendar.DefaultView.RowFilter = "WorkingDate>#" + Convert.ToDateTime(BestLine["LastProductionDate"].ToString()).ToString("dd-MMM-yyyy") + "#";
                                    if (dtCalendar.DefaultView.Count == 0)
                                    {
                                        throw new Exception("Production calendar does not support date after " + Convert.ToDateTime(BestLine["LastProductionDate"].ToString()).ToString("dd-MMM-yyyy"));
                                    }

                                }
                                else
                                {
                                    for (int ENT = 0; ENT < dvDistinctEntity.Rows.Count; ENT++)
                                    {
                                        dtCalendar = dicCalendar[dvDistinctEntity.Rows[ENT]["EntityId"].ToString()];

                                        dtCalendar.DefaultView.RowFilter = "WorkingDate>#" + startDate + "#";
                                        if (dtCalendar.DefaultView.Count == 0)
                                        {
                                            throw new Exception("Production calendar does not support date after " + startDate);
                                        }
                                    }
                                    BestLine = drBestLine(tempdate, productionOrders.Rows[i]["MaterialMasterId"].ToString(), dtWorkCenter, LastProductionLineID);
                                    dtCalendar = dicCalendar[BestLine["EntityId"].ToString()];
                                    dtCalendar.DefaultView.RowFilter = "WorkingDate>#" + startDate + "#";
                                }

                                double tempQty = TotalOrderQuantity;
                                int tempDayCount = 0;

                                //determining how many days to take to finish the production
                                while (tempQty > 0)
                                {
                                    tempDayCount++;
                                    try
                                    {
                                        tempQty = tempQty - getTarget(ref isBuildUpRequired, productionOrders.Rows[i], BestLine, tempDayCount, dtCalendar.DefaultView[tempCalendarIndex].Row, dicWorkCenterRunningHours, out double _STP, out double _AHP);// TargetPerDay;

                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("Production calendar does not support date after " + dtCalendar.DefaultView[tempCalendarIndex - 1]["ProductionDate"].ToString());
                                    }
                                }

                                if (tempDayCount <= MinimumLineDays)
                                {
                                    sbLog.AppendLine("Last block will run for [" + tempDayCount + "] which is less or equal to minimum workcenter days[" + MinimumLineDays + "], therefore no alter between lines");

                                    dtWorkCenter.DefaultView.RowFilter = "isnull(isResidualApplicable,0)=1 AND CURRENT_APPLICABLE='YES'";
                                    if (dtWorkCenter.DefaultView.Count > 0)
                                    {

                                        BestLine = dtWorkCenter.Select("WorkCenterMasterId='" + dtWorkCenter.DefaultView[0]["WorkCenterMasterId"].ToString() + "'")[0];
                                    }
                                    break;
                                }
                                //}

                                //else determine the best line for that production

                                BestLine = drBestLine(tempdate, productionOrders.Rows[i]["MaterialMasterId"].ToString(), dtWorkCenter, LastProductionLineID);
                                if (BestLine != null)
                                {
                                    //after allocating first line, we are resetting the production start time as LSD for each block of production
                                    if (Convert.ToDateTime(BestLine["LastProductionDate"].ToString()) <= LSD)
                                        startDate = LSD;
                                    break;
                                }

                                tempdate = tempdate.AddDays(1);
                            } while (tempdate < startDate);
                            #endregion Scan for each date starting from it's LSD to maximum block date to get the available best line to fit starting date

                            if (BestLine == null)
                            {
                                sbLog.AppendLine("No available best workcenter found!!!! ALLOCATION TERMINATED!!!");
                                break;
                            }


                            LastProductionLineID = BestLine["WorkcenterMasterID"].ToString();

                            if (strMaxAllocatedLines.Contains(LastProductionLineID) == false)
                                strMaxAllocatedLines.Add(LastProductionLineID);

                            //shift LSD to future date if best line's last production date is later on LSD
                            if (startDate <= Convert.ToDateTime(BestLine["LastProductionDate"].ToString()))
                                startDate = Convert.ToDateTime(BestLine["LastProductionDate"].ToString()).AddDays(1);


                            // DateTime LSDForLine = startDate;
                            dtCalendar.DefaultView.RowFilter = "WorkingDate>=#" + startDate.ToString("dd-MMM-yyyy") + "#";


                            if (productionOrders.Rows[i]["MaterialMasterId"].ToString() != BestLine["MaterialMasterId"].ToString())
                            {
                                BestLine["LastStyleRunningFor"] = "0";
                                isStyleChanged = true;
                            }
                            BestLine["MaterialMasterID"] = productionOrders.Rows[i]["MaterialMasterId"].ToString();

                        }


                        isBuildUpRequired = false;
                        ProductionBlock entry = new ProductionBlock();

                        try
                        {
                            LSDForLine = Convert.ToDateTime(dtCalendar.DefaultView[Index]["WorkingDate"].ToString());//there is no relationship but index number

                        }
                        catch (Exception ex)
                        {
                            string Error = string.Format("System cannot render calendar after {0} for production order#{1}",
                               LSDForLine.ToString("dd-MMM-yyyy"),
                                productionOrders.Rows[i]["ProductionOrderID"].ToString()).ToString();
                            throw new Exception(Error);
                        }

                        entry.ProductionDate = LSDForLine;
                        entry.WorkCenterMasterId = BestLine["WorkCenterMasterId"].ToString();
                        entry.MaterialMasterId = productionOrders.Rows[i]["MaterialMasterId"].ToString();
                        entry.EntityId = BestLine["EntityId"].ToString();//productionOrders.Rows[i]["EntityId"].ToString();
                        entry.ProcessID = BestLine["ProcessID"].ToString();
                        entry.ProductionOrderId = productionOrders.Rows[i]["ProductionOrderId"].ToString();

                        entry.ProductionHours = clsStaticInfo.dbl(dtCalendar.DefaultView[Index]["WorkingHours"].ToString()) + clsStaticInfo.dbl(dtCalendar.DefaultView[Index]["OTHours"].ToString());// clsStaticInfo.dbl(productionOrders.Rows[i]["PlanWorkingHoursPerDay"].ToString());
                        if (bplib.clsWebLib.GetBoolData(productionOrders.Rows[i]["ConsiderHourFromWorkCenter"].ToString()) == true)
                            entry.ProductionHours = clsStaticInfo.dbl(BestLine["StandardTimePerDay"].ToString());

                        entry.BlockNo = blockCount;
                        TargetPerDay = getTarget(ref isBuildUpRequired, productionOrders.Rows[i], BestLine, Index + 1, dtCalendar.DefaultView[Index].Row, dicWorkCenterRunningHours, out double StandardTargetPerDay, out double ActualHoursPerDay);//index+1=n'th day of production

                        entry.isBuildUp = isBuildUpRequired;
                        if (TotalOrderQuantity < TargetPerDay)
                        {
                            entry.Quantity = TotalOrderQuantity;
                        }
                        else
                        {
                            entry.Quantity = TargetPerDay;
                        }
                        //if (entry.ProductionOrderId == "20118" && entry.WorkCenterMasterId == "3")
                        //{

                        //}
                        //if (entry.ProductionOrderId == "201166")
                        //{

                        //}
                        if (strMaxAllocatedLines.Contains(entry.WorkCenterMasterId) == false)
                            strMaxAllocatedLines.Add(entry.WorkCenterMasterId);

                        entry.Quantity = Math.Round(AllocatedQty(ref LSDForLine, entry.WorkCenterMasterId, DicBalanceWorkcenterHours, ActualHoursPerDay, StandardTargetPerDay, entry.Quantity, entry.isBuildUp));

                        if (productionOrders.Rows[i]["MaterialMasterId"].ToString() != BestLine["MaterialMasterId"].ToString())
                        {
                            sbLog.AppendLine("Style changed");
                            entry.isStyleChange = true;

                        }
                        entry.isStyleChange = isStyleChanged;
                        _ProductionBlock.Add(entry);


                        BestLine["LastProductionDate"] = LSDForLine.ToString("dd-MMM-yyyy");
                        DataRow[] drSameLine = dtWorkCenter.Select("WorkCenterMasterId='" + BestLine["WorkCenterMasterId"].ToString() + "'");
                        foreach (DataRow drTempSameLine in drSameLine)
                        {
                            drTempSameLine["LastProductionDate"] = LSDForLine.ToString("dd-MMM-yyyy");
                        }

                        TotalOrderQuantity = TotalOrderQuantity - entry.Quantity;
                        TempTotalOrderQty = TempTotalOrderQty - entry.Quantity;
                        BestLine["AlreadyBooked"] = clsStaticInfo.dbl(BestLine["AlreadyBooked"].ToString()) + entry.Quantity;



                        BestLine["LastStyleRunningFor"] = clsStaticInfo.dbl(BestLine["LastStyleRunningFor"].ToString()) + 1;

                        dayCount++;


                        //taking only number of workcenters based on AllocatedLines
                        if (productionOrders.Rows[i]["ProductionStatusName"].ToString().ToUpper() != PlanningStatus.RUNNING.ToString())
                        {
                            if (strMaxAllocatedLines.Count == AllocatedLines)
                            {
                                for (int w = 0; w < dtWorkCenter.Rows.Count; w++)
                                {
                                    dtWorkCenter.Rows[w]["CURRENT_APPLICABLE"] = "NO";
                                    dtWorkCenter.Rows[w]["ACTUAL_APPLICABLE"] = "NO";
                                    if (strMaxAllocatedLines.Contains(dtWorkCenter.Rows[w]["WorkCenterMasterId"].ToString()))
                                    {
                                        dtWorkCenter.Rows[w]["ACTUAL_APPLICABLE"] = "YES";
                                        dtWorkCenter.Rows[w]["CURRENT_APPLICABLE"] = "YES";
                                    }
                                }
                            }
                        }

                        if (TotalOrderQuantity <= 0)
                        {
                            GetPrefferedWorkcenter(dtWorkCenter, ref TotalOrderQuantity, TempTotalOrderQty);
                            dayCount = 0;
                            BestLine = null;
                        }




                    }

                    sbLog.AppendLine("End of plotting block#" + blockCount);
                    foreach (DataRow item in dtWorkCenter.Rows)
                    {
                        item["CURRENT_APPLICABLE"] = "NO";
                        item["ACTUAL_APPLICABLE"] = "NO";
                        item["CurrentPRQty"] = "0";
                        item["AlreadyBooked"] = "0";
                        item["isResidualApplicable"] = false;

                    }

                    //saving final PR data
                    saveProductionPlan(_ProductionBlock, productionOrders.Rows[i]["ProductionOrderID"].ToString(), entityid, processid);


                    //Library.Service.TaskScheduler.TaskScheduler schedule = new Library.Service.TaskScheduler.TaskScheduler(_sqlRepository);
                    //schedule.UpdateTaskStatus();
                    ////Production Order Related Tasks

                    //string sql = @"SELECT TaskTemplateMasterId FROM trn.MasterOrder AS mo 
                    //            INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                    //            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                    //       WHERE so.id IN(Select SalesOrderId From TRN.ProductionOrderDetail Where ProductionOrderId='"+ productionOrders.Rows[i]["ProductionOrderID"].ToString() + "')";
                    //DataTable dtSO = _sqlRepository.GetDataTable(sql);
                    //string TaskTemplateMasterId = dtSO.Rows[0]["TaskTemplateMasterId"].ToString();

                    //DataTable dt = schedule.GetDataSourceMasterOrderNew(productionOrders.Rows[i]["ProductionOrderID"].ToString(), TaskAppliedOnEnum.ProductionOrder);
                    //if (dt.Rows.Count > 0)
                    //    schedule.MakeTNAMaster(dt, productionOrders.Rows[i]["ProductionOrderID"].ToString(), TaskAppliedOnEnum.ProductionOrder);

                }

                SendNotification("Distributing production quantity in sales orders and calculating expected completion date");
                Library.OrderManagement.Production.ExpectedSOWiseDateService expectedSO = new Library.OrderManagement.Production.ExpectedSOWiseDateService();
                expectedSO.ExpectedSOWiseProductionCompletionSave(entityid);

                SendNotification("Simulation Completed");
                _lock.UnlockProcess();
            }
            catch (Exception ex)
            {
                SendNotification(ex.ToString());
                _lock.UnlockProcess();
                string x = ex.Message;
                throw (ex);
            }
            finally
            {

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

        private double AllocatedQty(ref DateTime LSD, string WCId, Dictionary<string, double> WCList, double Hour, double TargetPerDay, double CurrentQty, bool BuildUp)
        {
            //if (CurrentQty == 52)
            //{

            //}
            //if (CurrentQty == 1420)
            //{

            //}


            TargetPerDay = Math.Round(TargetPerDay * Hour);
            double BalanceHourForCurrent = Hour - (Hour / TargetPerDay * CurrentQty);
            double CurrentUtilization = (Hour / TargetPerDay * CurrentQty);


            if (WCList.ContainsKey(WCId))
            {
                if (WCList[WCId] - 0.05 <= 0)
                {
                    WCList.Remove(WCId);
                    return CurrentQty;
                }
            }

            if (BuildUp)
            {
                if (WCList.ContainsKey(WCId))
                {
                    double d = CurrentQty;

                    if (CurrentUtilization > WCList[WCId])
                    {
                        if ((CurrentQty / Hour) * WCList[WCId] >= 1)
                            d = (CurrentQty / Hour) * WCList[WCId];

                    }
                    //WCList[WCId] = WCList[WCId] - CurrentUtilization;

                    //if (WCList[WCId] - 0.05 >= 0)//there is still room for another planning for that day
                    //    LSD = LSD.AddDays(-1);
                    WCList.Remove(WCId);
                    return d;
                }
                else
                {
                    if (WCList.ContainsKey(WCId))
                        WCList.Remove(WCId);

                    return CurrentQty;
                }
            }
            else
            {
                if (WCList.ContainsKey(WCId) == false)
                {
                    if (BalanceHourForCurrent > 0.05)
                    {
                        LSD = LSD.AddDays(-1);
                        WCList.Add(WCId, BalanceHourForCurrent);
                    }
                    return CurrentQty;
                }
                else
                {
                    double d = CurrentQty;
                    //to produce CurrentQty, how many hours required
                    if (CurrentUtilization > WCList[WCId])
                    {
                        d = (CurrentQty / Hour) * WCList[WCId];
                    }

                    WCList[WCId] = WCList[WCId] - CurrentUtilization;
                    if (WCList[WCId] - 0.05 >= 0)//there is still room for another planning for that day
                        LSD = LSD.AddDays(-1);


                    return d;
                }


            }

        }
        private void GetPrefferedWorkcenter(DataTable dtWorkcenter, ref double RemainingOrdQtyToPlot, double TotalPRQtyRemaining)
        {
            //dtWorkCenter.Columns.Add("CURRENT_APPLICABLE");
            //dtWorkCenter.Columns.Add("ACTUAL_APPLICABLE");
            //dtWorkCenter.Columns.Add("AlreadyBooked", typeof(double));
            //currentPRQty
            //see if there only one current applicable which is running and plotting
            for (int i = 0; i < dtWorkcenter.Rows.Count; i++)
            {
                dtWorkcenter.Rows[i]["CURRENT_APPLICABLE"] = "NO";
            }
            dtWorkcenter.DefaultView.RowFilter = "ACTUAL_APPLICABLE='YES'";
            for (int i = 0; i < dtWorkcenter.DefaultView.Count; i++)
            {
                if (clsStaticInfo.dbl(dtWorkcenter.DefaultView[i]["currentPRQty"].ToString()) > clsStaticInfo.dbl(dtWorkcenter.DefaultView[i]["AlreadyBooked"].ToString()))
                {
                    dtWorkcenter.DefaultView[i]["CURRENT_APPLICABLE"] = "YES";
                    RemainingOrdQtyToPlot = clsStaticInfo.dbl(dtWorkcenter.DefaultView[i]["currentPRQty"].ToString()) - clsStaticInfo.dbl(dtWorkcenter.DefaultView[i]["AlreadyBooked"].ToString());
                    return;
                }
            }

            for (int i = 0; i < dtWorkcenter.Rows.Count; i++)
            {
                dtWorkcenter.Rows[i]["CURRENT_APPLICABLE"] = "NO";
            }
            for (int i = 0; i < dtWorkcenter.DefaultView.Count; i++)
            {
                if (clsStaticInfo.dbl(dtWorkcenter.DefaultView[i]["currentPRQty"].ToString()) == 0)
                {
                    dtWorkcenter.DefaultView[i]["CURRENT_APPLICABLE"] = "YES";
                }
            }
            dtWorkcenter.DefaultView.RowFilter = "CURRENT_APPLICABLE='YES'";
            if (dtWorkcenter.DefaultView.Count == 0)
            {
                for (int i = 0; i < dtWorkcenter.Rows.Count; i++)
                {
                    dtWorkcenter.Rows[i]["CURRENT_APPLICABLE"] = dtWorkcenter.Rows[i]["ACTUAL_APPLICABLE"].ToString();
                }
                RemainingOrdQtyToPlot = TotalPRQtyRemaining;
                return;
                //dtWorkcenter.DefaultView.RowFilter = "ACTUAL_APPLICABLE='YES' AND isnull(isResidualApplicable,0)=1";
                //if (dtWorkcenter.DefaultView.Count > 0)
                //{
                //    //return residual row
                //    dtWorkcenter.DefaultView[0]["CURRENT_APPLICABLE"] = "YES";
                //    RemainingOrdQtyToPlot = TotalPRQtyRemaining;
                //    return;
                //}
                //else
                //{
                //dtWorkcenter.DefaultView.RowFilter = "ACTUAL_APPLICABLE='YES'";
                ////return first row
                //if (dtWorkcenter.DefaultView.Count > 0)
                //{
                //    //return residual row
                //    dtWorkcenter.DefaultView[0]["CURRENT_APPLICABLE"] = "YES";
                //    RemainingOrdQtyToPlot = TotalPRQtyRemaining;
                //    return;
                //}

                //}
            }
            else
            {
                //dtWorkcenter.DefaultView.RowFilter = "ACTUAL_APPLICABLE='YES'";
                //for (int i = 0; i < dtWorkcenter.Rows.Count; i++)
                //{
                //    dtWorkcenter.Rows[i]["CURRENT_APPLICABLE"] = dtWorkcenter.Rows[i]["ACTUAL_APPLICABLE"].ToString();
                //}

                RemainingOrdQtyToPlot = TotalPRQtyRemaining;
            }


        }
        private double getTarget(ref bool isBuildUpRequired, DataRow drOrderConfig, DataRow drPreferredWC, int currentDay, DataRow drCalendar, Dictionary<string, DataTable> dicWorkCenterRunningHours, out double StandardTargetPerDay, out double ActualPlanHourPerDay)
        {
            isBuildUpRequired = false;
            double TargetPerDay = clsStaticInfo.dbl(drOrderConfig["TargetPerHour"].ToString());
            double FirstDayOutput = clsStaticInfo.dbl(drOrderConfig["FirstDayOutput"].ToString());

            //proportional qty as per no.of workstations
            if (bplib.clsWebLib.GetBoolData(drOrderConfig["ConsiderWorkStationsFromWorkCenter"].ToString()) == true)
            {
                TargetPerDay = TargetPerDay * (clsStaticInfo.dbl(drPreferredWC["NoOfWorkStation"].ToString()) / clsStaticInfo.dbl(drOrderConfig["NoOfWorkStation"].ToString()));
                FirstDayOutput = FirstDayOutput * (clsStaticInfo.dbl(drPreferredWC["NoOfWorkStation"].ToString()) / clsStaticInfo.dbl(drOrderConfig["NoOfWorkStation"].ToString()));
            }
            StandardTargetPerDay = TargetPerDay;


            double IncrementValue = clsStaticInfo.dbl(drOrderConfig["IncrementValue"].ToString());
            string IncrementType = drOrderConfig["IncrementType"].ToString();

            double WorkingHours = clsStaticInfo.dbl(drCalendar["WorkingHours"].ToString()) + clsStaticInfo.dbl(drCalendar["OTHours"].ToString()); //clsStaticInfo.dbl(drOrderConfig["PlanWorkingHoursPerDay"].ToString());
            if (bplib.clsWebLib.GetBoolData(drOrderConfig["ConsiderHourFromWorkCenter"].ToString()) == true)
                WorkingHours = GetWorkcenterHour(drPreferredWC["WorkCenterMasterId"].ToString(), drCalendar["WorkingDate"].ToString(), clsStaticInfo.dbl(drPreferredWC["StandardTimePerDay"].ToString()), dicWorkCenterRunningHours); //clsStaticInfo.dbl(drPreferredWC["StandardTimePerDay"].ToString());


            ActualPlanHourPerDay = WorkingHours;
            //if (WorkingHours > 10)
            //{

            //}

            currentDay = (int)clsStaticInfo.dbl(drPreferredWC["LastStyleRunningFor"].ToString()) + 1;


            try
            {
                double incrementedTarget = FirstDayOutput;
                for (int i = 1; i < currentDay; i++)
                {
                    if (IncrementType.ToUpper() == "FIXED")
                        incrementedTarget += IncrementValue;
                    else
                        incrementedTarget += incrementedTarget / 100 * IncrementValue;


                    if (incrementedTarget >= TargetPerDay)
                    {
                        isBuildUpRequired = false;
                        return Math.Round(TargetPerDay * WorkingHours);
                    }
                }

                if (incrementedTarget < TargetPerDay)
                    isBuildUpRequired = true;

                if (FirstDayOutput > TargetPerDay)
                    return Math.Round(TargetPerDay * WorkingHours);
                else
                    return Math.Round(incrementedTarget * WorkingHours);
            }
            catch (Exception)
            {

            }



            return TargetPerDay;
        }
        private double getTargetBackup(DataRow drOrderConfig, DataRow drPreferredWC, int currentDay)
        {
            double TargetPerDay = clsStaticInfo.dbl(drOrderConfig["TargetPerDay"].ToString());

            double FirstDayOutput = clsStaticInfo.dbl(drOrderConfig["FirstDayOutput"].ToString()) * clsStaticInfo.dbl(drOrderConfig["PlanWorkingHoursPerDay"].ToString());

            double IncrementValue = clsStaticInfo.dbl(drOrderConfig["IncrementValue"].ToString());
            string IncrementType = drOrderConfig["IncrementType"].ToString();

            //current production day =1 but this style might running in this line for longer
            // currentDay += (int)clsStaticInfo.dbl(drPreferredWC["LastStyleRunningFor"].ToString());
            currentDay = currentDay + (int)clsStaticInfo.dbl(drPreferredWC["LastStyleRunningFor"].ToString());
            try
            {
                double incrementedTarget = FirstDayOutput;
                for (int i = 1; i < currentDay; i++)
                {
                    if (IncrementType.ToUpper() == "FIXED")
                        incrementedTarget += IncrementValue * clsStaticInfo.dbl(drOrderConfig["PlanWorkingHoursPerDay"].ToString());
                    else
                        incrementedTarget += incrementedTarget / 100 * IncrementValue;


                    if (incrementedTarget >= TargetPerDay)
                        return TargetPerDay;
                }
                if (FirstDayOutput > TargetPerDay)
                    return TargetPerDay;
                else
                    return incrementedTarget;
            }
            catch (Exception)
            {

            }



            return TargetPerDay;
        }
        private void saveProductionPlan(List<ProductionBlock> entry, string productionOrderID, string entityid, string processid)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataSet dsMaster;
            string sql = @"SELECT * FROM ProductionPlanningType1 t1 WHERE EntityID='" + entityid + "' AND t1.ProductionOrderID ='" + productionOrderID + "' AND ProcessID='" + processid + "'";
            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

            while (dsMaster.Tables[0].DefaultView.Count > 0)
            {
                dsMaster.Tables[0].DefaultView[0].Delete();
            }


            DataRow dr;

            for (int i = 0; i < entry.Count; i++)
            {

                dr = dsMaster.Tables[0].NewRow();

                dr["ProductionOrderID"] = entry[i].ProductionOrderId;
                dr["WorkCenterMasterId"] = entry[i].WorkCenterMasterId;
                dr["MaterialMasterId"] = entry[i].MaterialMasterId;
                dr["EntityId"] = entry[i].EntityId;
                dr["ProcessID"] = entry[i].ProcessID;
                dr["ProductionDate"] = entry[i].ProductionDate;
                dr["Quantity"] = entry[i].Quantity;
                dr["ProductionHours"] = entry[i].ProductionHours;
                dr["isBuildUp"] = entry[i].isBuildUp;
                dr["isStyleChange"] = entry[i].isStyleChange;
                dr["BlockNo"] = entry[i].BlockNo;

                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = System.DateTime.Now.ToString();
                dr["AddedFromIP"] = identity.IPAddress;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;

                dsMaster.Tables[0].Rows.Add(dr);

            }



            clsStaticInfo clsStatic = new clsStaticInfo();
            clsStatic.SaveDataSets(dsMaster);
        }

        private DataRow drBestLine(DateTime LSD, string ProductID, DataTable WorkcenterList, string LastProductionLine = "")
        {
            //getting best line, find nearest production line with same product and ProductionStartDate(LSD) date
            //LastProductionDate
            //for same product
            //DataRow[] rows = WorkcenterList.Select("MaterialMasterId='" + ProductID + "' AND CURRENT_APPLICABLE='YES'");
            DataView dv = new DataView(WorkcenterList);
            DataTable dtTemp = dv.ToTable();

            DataView dvtemp = new DataView(dtTemp);
            dvtemp.RowFilter = "CURRENT_APPLICABLE='YES' AND LastProductionDate<=#" + LSD.ToString("dd-MMM-yyyy") + "#";
            DataRow[] rows = dvtemp.ToTable().Select();// dtTemp.Select("CURRENT_APPLICABLE='YES' AND LastProductionDate<=#" + LSD.ToString("dd-MMM-yyyy") + "#");
            DataRow[] rowsFinal = WorkcenterList.Select("CURRENT_APPLICABLE='YES'");



            int LowestGapIndex = -1;
            long MinimumGapInTicks = long.MaxValue;
            if (rows.Length > 0)
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    DateTime dtLastProductionDate = Convert.ToDateTime(rows[i]["LastProductionDate"].ToString());
                    if (LastProductionLine != "")
                        if (LastProductionLine != rows[i]["WorkcenterMasterID"].ToString())
                        {
                            WorkcenterList.DefaultView.RowFilter = "WorkcenterMasterID='" + LastProductionLine + "' AND CURRENT_APPLICABLE='YES' ";
                            if (WorkcenterList.DefaultView.Count > 0)
                                if (WorkcenterList.DefaultView[0]["MaterialMasterID"].ToString().ToUpper() != rows[i]["MaterialMasterID"].ToString().ToUpper())
                                {
                                    dtLastProductionDate = dtLastProductionDate.AddDays(DaysToBeAddedForLineChange);


                                    //additional
                                    DataView dvLocalTemp = new DataView(dtTemp);
                                    dvLocalTemp.RowFilter = "CURRENT_APPLICABLE='YES' AND LastProductionDate>#" + LSD.ToString("dd-MMM-yyyy") + "#";
                                    DataRow[] LocalRows = dvLocalTemp.ToTable().Select();// dtTemp.Select("CURRENT_APPLICABLE='YES' AND LastProductionDate<=#" + LSD.ToString("dd-MMM-yyyy") + "#");
                                    foreach (DataRow item in LocalRows)
                                    {
                                        if (Math.Abs(LSD.Ticks - dtLastProductionDate.Ticks) < Math.Abs(LSD.Ticks - Convert.ToDateTime((item["LastProductionDate"])).Ticks))
                                        {
                                            LowestGapIndex = i;
                                        }
                                    }

                                }
                        }

                    //if (Math.Abs(LSD.Ticks - dtLastProductionDate.Ticks) < MinimumGapInTicks)
                    //{
                    //    MinimumGapInTicks = Math.Abs(LSD.Ticks - dtLastProductionDate.Ticks);
                    //    LowestGapIndex = i;
                    //}
                    if (dtLastProductionDate <= LSD)
                    {
                        if (Math.Abs(LSD.Ticks - dtLastProductionDate.Ticks) < MinimumGapInTicks)
                        {
                            MinimumGapInTicks = Math.Abs(LSD.Ticks - dtLastProductionDate.Ticks);
                            LowestGapIndex = i;
                        }
                    }

                }

                if (LowestGapIndex > -1)
                {
                    if (ProductID != rows[LowestGapIndex]["MaterialMasterId"].ToString())
                    {
                        rows[LowestGapIndex]["LastStyleRunningFor"] = "0";
                        //rowsFinal[LowestGapIndex]["LastStyleRunningFor"] = "-1";
                    }
                    else
                    {
                        //we don't need continuous buildup for orders
                        //so, set LastStyleRunningFor=max buildup days
                        //therefore system will not calculated buildup days anymore
                        rows[LowestGapIndex]["LastStyleRunningFor"] = DaysToBeAddedForLineChange;

                    }
                    //rowsFinal[LowestGapIndex]["LastStyleRunningFor"] = clsStaticInfo.dbl(rowsFinal[LowestGapIndex]["LastStyleRunningFor"].ToString()) + 1;
                    foreach (DataRow itemFinal in rowsFinal)
                    {
                        if (itemFinal["WorkCenterMasterId"].ToString() == rows[LowestGapIndex]["WorkCenterMasterId"].ToString())
                        {
                            DataRow[] dr = WorkcenterList.Select("WorkCenterMasterId='" + itemFinal["WorkCenterMasterId"].ToString() + "'");
                            foreach (DataRow item in dr)
                            {
                                item["LastProductionDate"] = itemFinal["LastProductionDate"];
                            }

                            return itemFinal;
                        }

                    }
                    // return rows[LowestGapIndex];
                }


            }


            //after
            dvtemp = new DataView(dtTemp);
            dvtemp.RowFilter = "CURRENT_APPLICABLE='YES' AND LastProductionDate>#" + LSD.ToString("dd-MMM-yyyy") + "#";
            rows = dvtemp.ToTable().Select();// dtTemp.Select("CURRENT_APPLICABLE='YES' AND LastProductionDate<=#" + LSD.ToString("dd-MMM-yyyy") + "#");


            LowestGapIndex = -1;
            MinimumGapInTicks = long.MaxValue;
            if (rows.Length > 0)
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    DateTime dtLastProductionDate = Convert.ToDateTime(rows[i]["LastProductionDate"].ToString());
                    if (LastProductionLine != "")
                        if (LastProductionLine != rows[i]["WorkcenterMasterID"].ToString())
                        {
                            WorkcenterList.DefaultView.RowFilter = "CURRENT_APPLICABLE='YES' AND WorkcenterMasterID='" + LastProductionLine + "'";
                            if (WorkcenterList.DefaultView.Count > 0)
                                if (WorkcenterList.DefaultView[0]["MaterialMasterID"].ToString().ToUpper() != rows[i]["MaterialMasterID"].ToString().ToUpper())
                                {
                                    dtLastProductionDate = dtLastProductionDate.AddDays(DaysToBeAddedForLineChange);

                                }

                        }

                    //if (Math.Abs(LSD.Ticks - dtLastProductionDate.Ticks) < MinimumGapInTicks)
                    //{
                    //    MinimumGapInTicks = Math.Abs(LSD.Ticks - dtLastProductionDate.Ticks);
                    //    LowestGapIndex = i;
                    //}
                    if (dtLastProductionDate > LSD)
                    {
                        if (Math.Abs(LSD.Ticks - dtLastProductionDate.Ticks) < MinimumGapInTicks)
                        {
                            MinimumGapInTicks = Math.Abs(LSD.Ticks - dtLastProductionDate.Ticks);
                            LowestGapIndex = i;
                        }
                    }

                }

                if (LowestGapIndex > -1)
                {
                    if (ProductID != rows[LowestGapIndex]["MaterialMasterId"].ToString())
                    {
                        rows[LowestGapIndex]["LastStyleRunningFor"] = "0";
                        //rowsFinal[LowestGapIndex]["LastStyleRunningFor"] = "-1";
                    }
                    else
                    {
                        //we don't need continuous buildup for orders
                        //so, set LastStyleRunningFor=max buildup days
                        //therefore system will not calculated buildup days anumore
                        rows[LowestGapIndex]["LastStyleRunningFor"] = DaysToBeAddedForLineChange;

                    }
                    //rowsFinal[LowestGapIndex]["LastStyleRunningFor"] = clsStaticInfo.dbl(rowsFinal[LowestGapIndex]["LastStyleRunningFor"].ToString()) + 1;

                    foreach (DataRow itemFinal in rowsFinal)
                    {
                        if (itemFinal["WorkCenterMasterId"].ToString() == rows[LowestGapIndex]["WorkCenterMasterId"].ToString())
                        {
                            DataRow[] dr = WorkcenterList.Select("WorkCenterMasterId='" + itemFinal["WorkCenterMasterId"].ToString() + "'");
                            foreach (DataRow item in dr)
                            {
                                item["LastProductionDate"] = itemFinal["LastProductionDate"];
                            }
                            return itemFinal;
                        }


                    }
                    //return rows[LowestGapIndex];
                }


            }


            return null;
        }


        private Dictionary<string, DataTable> WorkCenterRunningHours()
        {
            string _sql = @"SELECT  W.Id,ISNULL(D.StartDate,CONVERT(DATE,GETDATE())) AS StartDate,ISNULL(D.Hour,w.MaxTimePerDay) AS RunningHour FROM scs.WorkCenterMaster AS w
                        LEFT JOIN scs.WorkCenterMasterEffectiveDate AS D ON d.WorkCenterMasterId=w.Id
                        WHERE w.Active=1
                        ORDER BY id,ISNULL(D.StartDate,CONVERT(DATE,GETDATE())) DESC";

            DataTable dt = _sqlRepository.GetDataTable(_sql);
            Dictionary<string, DataTable> data = new Dictionary<string, DataTable>();
            string Id = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Id != dt.Rows[i]["Id"].ToString())
                {

                    dt.DefaultView.RowFilter = "Id='" + dt.Rows[i]["Id"].ToString() + @"'";
                    data.Add(dt.Rows[i]["Id"].ToString(), dt.DefaultView.ToTable());
                }
                Id = dt.Rows[i]["Id"].ToString();
            }
            return data;
        }
        private double GetWorkcenterHour(string WCId, string Date, double StandardTimePerDay, Dictionary<string, DataTable> WCList)
        {
            try
            {
                DataTable dt = WCList[WCId];
                dt.DefaultView.RowFilter = "StartDate<=#" + Convert.ToDateTime(Date).ToString("dd-MMM-yyyy") + "#";
                if (dt.DefaultView.Count > 0)
                    return clsStaticInfo.dbl(dt.DefaultView[0]["RunningHour"].ToString());

                return StandardTimePerDay;
            }
            catch (Exception ex)
            {

                throw (ex);
            }


        }

        private DataTable dtAvailableWrokcenters(string productionOrderID, string ProductionStatusName, string processid)
        {
            //for running
            string sql = @"SELECT DISTINCT  WS.*,convert(bit,0) as isResidualApplicable FROM ProductionPlanningType1 AS w
                        INNER JOIN [SCS].[WorkCenterMaster] WS ON ws.Id=w.WorkCenterMasterId
                       INNER JOIN trn.FreezeConfigPlanningType1 AS F ON f.EntityId=w.EntityID AND f.FreezeDate 
                       BETWEEN (SELECT MIN(ProductionDate) FROM ProductionPlanningType1 WHERE ProductionOrderId='" + productionOrderID + @"') 
                       AND (SELECT MAX(ProductionDate) FROM ProductionPlanningType1 WHERE ProductionOrderId='" + productionOrderID + @"') 
                       WHERE W.ProductionOrderId='" + productionOrderID + @"' AND WS.ProcessID='" + processid + @"'";

            DataTable dtWorkCenter = _sqlRepository.GetDataTable(sql);

            //freeze
            if (dtWorkCenter.Rows.Count == 0)
            {
                //sql = @"SELECT ws.* FROM [TRN].[RunningOrderWorkCenter] W
                //        INNER JOIN [SCS].[WorkCenterMaster] WS ON ws.Id=w.WorkCenterMasterId
                //        INNER JOIN ProductionOrderSchedulingParametersType1 AS T ON t.ProductionOrderID=w.ProductionOrderId
                //        INNER JOIN trn.ProductionOrder po ON po.Id=t.ProductionOrderID
                //        LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                //WHERE  ps.UserName='" + PlanningStatus.RUNNING.ToString() + @"' AND W.ProductionOrderId='" + productionOrderID + "' AND WS.ProcessID='" + processid + "'";
                sql = @"SELECT ws.*,isnull(W.isResidualApplicable,0) AS isResidualApplicable FROM [TRN].[RunningOrderWorkCenter] W
                        INNER JOIN [SCS].[WorkCenterMaster] WS ON ws.Id=w.WorkCenterMasterId
                        INNER JOIN ProductionOrderSchedulingParametersType1 AS T ON t.ProductionOrderID=w.ProductionOrderId
                        INNER JOIN trn.ProductionOrder po ON po.Id=t.ProductionOrderID
                        LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                WHERE  ps.UserName='" + PlanningStatus.RUNNING.ToString() + @"' AND W.ProductionOrderId='" + productionOrderID + "' AND WS.ProcessID='" + processid + "'";

                dtWorkCenter = _sqlRepository.GetDataTable(sql);
            }

            if (dtWorkCenter.Rows.Count == 0)
            {
                //excluded WC
                DataTable dsExcludeWorkCenter = null;
                sql = @"SELECT ws.*,convert(bit,0) as isResidualApplicable FROM [TRN].[ProductionOrderWorkCenter] W
                        INNER JOIN [SCS].[WorkCenterMaster] WS ON ws.Id=w.WorkCenterMasterId
                        INNER JOIN ProductionOrderSchedulingParametersType1 AS t ON t.ProductionOrderID=w.ProductionOrderId
                WHERE t.WCPreferenceType='EXCLUDE' AND W.ProductionOrderId='" + productionOrderID + "' AND WS.ProcessID='" + processid + "'";
                dsExcludeWorkCenter = _sqlRepository.GetDataTable(sql);
                if (dsExcludeWorkCenter.Rows.Count > 0)
                {
                    sql = @"SELECT WC.*,convert(bit,0) as isResidualApplicable FROM  [SCS].[WorkCenterMaster] WC 

                                WHERE wc.[Active]=1 and  WC.ProcessId ='" + processid + @"' AND WC.EntityId IN (
                                SELECT DISTINCT d.EntityId FROM [TRN].ProductionOrder D
                               
                                WHERE d.Id='" + productionOrderID + @"'
                                ) AND WC.Id NOT IN (SELECT ws.Id FROM [TRN].[ProductionOrderWorkCenter] W
                        INNER JOIN [SCS].[WorkCenterMaster] WS ON ws.Id=w.WorkCenterMasterId
                        INNER JOIN ProductionOrderSchedulingParametersType1 AS t ON t.ProductionOrderID=w.ProductionOrderId
                WHERE t.WCPreferenceType='EXCLUDE' AND W.ProductionOrderId='" + productionOrderID + "' AND WS.ProcessID='" + processid + @"')
                                ";
                    dtWorkCenter = _sqlRepository.GetDataTable(sql);
                }
                else
                {
                    if (dtWorkCenter.Rows.Count == 0)
                    {
                        sql = @"SELECT ws.*,convert(bit,0) as isResidualApplicable FROM [TRN].[ProductionOrderWorkCenter] W
                        INNER JOIN [SCS].[WorkCenterMaster] WS ON ws.Id=w.WorkCenterMasterId
                        INNER JOIN ProductionOrderSchedulingParametersType1 AS t ON t.ProductionOrderID=w.ProductionOrderId
                WHERE t.WCPreferenceType='INCLUDE' AND W.ProductionOrderId='" + productionOrderID + "' AND WS.ProcessID='" + processid + "'";
                        dtWorkCenter = _sqlRepository.GetDataTable(sql);
                    }

                    if (dtWorkCenter.Rows.Count == 0)
                    {
                        sbLog.AppendLine("No workcenter preference was defined in production order\r\nSearching in product preference...");
                        sql = @"SELECT WC.*,convert(bit,0) as isResidualApplicable FROM [SCS].[WorkCenterMasterProductPriority] WP 
                                INNER JOIN [SCS].[WorkCenterMaster] WC ON wc.Id=wp.WorkCenterMasterId

                                WHERE WP.ProductMasterId IN (
                                SELECT DISTINCT pd.ProductMasterId FROM [TRN].[ProductionOrderDetail] D
                                INNER JOIN trn.SalesOrder AS so ON so.Id=d.SalesOrderId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                INNER JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId

                                WHERE d.ProductionOrderId='" + productionOrderID + @"'
                                ) AND WC.ProcessID='" + processid + @"' AND WC.EntityId IN (
                                SELECT DISTINCT d.EntityId FROM [TRN].ProductionOrder D
                                WHERE d.Id='" + productionOrderID + @"'
                                ) 
                                ORDER BY WP.Priority ASC";
                        dtWorkCenter = _sqlRepository.GetDataTable(sql);

                    }
                    else
                    {
                        for (int i = 0; i < dtWorkCenter.Rows.Count; i++)
                            sbLog.AppendLine("workcenter preference found at production order [" + dtWorkCenter.Rows[i]["username"].ToString() + "]");

                    }
                    if (dtWorkCenter.Rows.Count == 0)
                    {
                        sbLog.AppendLine("No workcenter preference was defined in product configuration");
                        sql = @"SELECT WC.*,convert(bit,0) as isResidualApplicable FROM  [SCS].[WorkCenterMaster] WC 

                                WHERE wc.[Active]=1 and WC.ProcessId ='" + processid + @"' AND WC.EntityId IN (
                                SELECT DISTINCT d.EntityId FROM [TRN].ProductionOrder D
                               
                                WHERE d.Id='" + productionOrderID + @"'
                                ) 
                                ";
                        dtWorkCenter = _sqlRepository.GetDataTable(sql);

                    }
                    else
                    {
                        for (int i = 0; i < dtWorkCenter.Rows.Count; i++)
                            sbLog.AppendLine("workcenter preference found at product configuration [" + dtWorkCenter.Rows[i]["username"].ToString() + "]");


                    }
                }
            }
            //final block for all workcenters with last production date
            string workcenterlist = "''";
            for (int i = 0; i < dtWorkCenter.Rows.Count; i++)
                workcenterlist += ",'" + dtWorkCenter.Rows[i]["ID"].ToString() + "'";

            if (ProductionStatusName.ToUpper() == PlanningStatus.RUNNING.ToString().ToUpper())
            {
                sql = @"SELECT WC.Id AS WorkCenterMasterId, p.MaterialMasterId,WC.ProcessID,isnull(RWC.isResidualApplicable,0) AS isResidualApplicable,
                    isnull(RWC.Qty,0) AS CurrentPRQty,
                    ISNULL(prd.Quantity,0) AlreadyBooked,
                    FORMAT(ISNULL(p.ProductionDate,dateadd(DAY,-1, CONVERT(DATE, CASE WHEN ISNULL(e.StartDate,'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"')<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' THEN '" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' ELSE e.StartDate END))),'dd-MMM-yyyy') AS LastProductionDate,0 AS LastStyleRunningFor,
                           p.Quantity--, p.ProductionHours
                      FROM [SCS].[WorkCenterMaster] WC 
                        left outer join trn.RunningOrderWorkCenter RWC on RWC.WorkCenterMasterId=WC.ID and RWC.ProductionOrderId='" + productionOrderID + @"'
                         left outer join (SELECT t.WorkCenterMasterId,t.ProductionOrderId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t
                                         WHERE t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
                                         GROUP BY  t.WorkCenterMasterId,t.ProductionOrderId ) AS PRD oN prd.ProductionOrderId=RWC.ProductionOrderId and PRD.WorkCenterMasterId=RWC.WorkCenterMasterId       

INNER JOIN (SELECT WorkCenterMasterId,MAX(StartDate) AS StartDate FROM [SCS].[WorkCenterMasterEffectiveDate] GROUP BY WorkCenterMasterId) E ON e.WorkCenterMasterId=WC.Id
                    LEFT OUTER JOIN 
                    (
		                 SELECT  * FROM ( SELECT dense_rank() OVER (PARTITION BY t.WorkCenterMasterId ORDER BY t.ProductionDate DESC) AS RANK,
					                    t.WorkCenterMasterId,t.ProductionDate,t.Quantity,t.MaterialMasterId
					                    FROM (SELECT t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t 
					                    INNER JOIN [TRN].[ProductionOrder] P ON p.Id=t.ProductionOrderID
					                    LEFT OUTER JOIN (SELECT DISTINCT pod.ProductionOrderId,mm.Id AS MaterialMasterId
					                                       FROM trn.ProductionOrderDetail AS pod
														INNER JOIN trn.SalesOrder AS so ON pod.SalesOrderId=so.Id
														INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
														INNER JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId) AS K ON k.ProductionOrderId=p.Id
                                        where  t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'					                   
                                        GROUP BY t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId) AS T
		                    ) AS K WHERE K.[RANK]=1
                    ) AS P ON p.WorkCenterMasterId=wc.Id
                    WHERE WC.[Active]=1 AND WC.Id IN (" + workcenterlist + ")";
                dtWorkCenter = _sqlRepository.GetDataTable(sql);
            }
            else
            {
                sql = @"SELECT WC.Id AS WorkCenterMasterId, p.MaterialMasterId,WC.ProcessID,convert(bit,0) AS isResidualApplicable,
                    0 AS CurrentPRQty,
                    ISNULL(prd.Quantity,0) AlreadyBooked,
                    FORMAT(ISNULL(p.ProductionDate,dateadd(DAY,-1, CONVERT(DATE, CASE WHEN ISNULL(e.StartDate,'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"')<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' THEN '" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' ELSE e.StartDate END))),'dd-MMM-yyyy') AS LastProductionDate,0 AS LastStyleRunningFor,
                           p.Quantity--, p.ProductionHours
                      FROM [SCS].[WorkCenterMaster] WC 
                        left outer join trn.ProductionOrderWorkCenter RWC on RWC.WorkCenterMasterId=WC.ID and RWC.ProductionOrderId='" + productionOrderID + @"'
                         left outer join (SELECT t.WorkCenterMasterId,t.ProductionOrderId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t
                                         WHERE t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
                                         GROUP BY  t.WorkCenterMasterId,t.ProductionOrderId ) AS PRD oN prd.ProductionOrderId=RWC.ProductionOrderId and PRD.WorkCenterMasterId=RWC.WorkCenterMasterId       

                            INNER JOIN (SELECT WorkCenterMasterId,MAX(StartDate) AS StartDate FROM [SCS].[WorkCenterMasterEffectiveDate] GROUP BY WorkCenterMasterId) E ON e.WorkCenterMasterId=WC.Id
                    LEFT OUTER JOIN 
                    (
		                 SELECT  * FROM ( SELECT dense_rank() OVER (PARTITION BY t.WorkCenterMasterId ORDER BY t.ProductionDate DESC) AS RANK,
					                    t.WorkCenterMasterId,t.ProductionDate,t.Quantity,t.MaterialMasterId
					                    FROM (SELECT t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t 
					                    INNER JOIN [TRN].[ProductionOrder] P ON p.Id=t.ProductionOrderID
					                    LEFT OUTER JOIN (SELECT DISTINCT pod.ProductionOrderId,mm.Id AS MaterialMasterId
					                                       FROM trn.ProductionOrderDetail AS pod
														INNER JOIN trn.SalesOrder AS so ON pod.SalesOrderId=so.Id
														INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
														INNER JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId) AS K ON k.ProductionOrderId=p.Id
                                        where  t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'					                   
                                        GROUP BY t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId) AS T
		                    ) AS K WHERE K.[RANK]=1
                    ) AS P ON p.WorkCenterMasterId=wc.Id
                    WHERE WC.[Active]=1 AND WC.Id IN (" + workcenterlist + ")";
                dtWorkCenter = _sqlRepository.GetDataTable(sql);

            }
            string sqlLastRunningDays = @"SELECT *
                    FROM ( SELECT dense_rank() OVER (PARTITION BY t.WorkCenterMasterId ORDER BY t.ProductionDate DESC) AS LW,
					                    t.WorkCenterMasterId,t.ProductionDate,t.Quantity, t.MaterialMasterId
                           from (SELECT t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t 
					                    INNER JOIN [TRN].[ProductionOrder] P ON p.Id=t.ProductionOrderID
					                    LEFT OUTER JOIN (SELECT DISTINCT pod.ProductionOrderId,mm.Id AS MaterialMasterId
					                                       FROM trn.ProductionOrderDetail AS pod
														INNER JOIN trn.SalesOrder AS so ON pod.SalesOrderId=so.Id
														INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
														INNER JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId) AS K ON k.ProductionOrderId=p.Id
                                        WHERE p.Id='" + productionOrderID + @"' AND t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
					                    GROUP BY t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId) AS T
                        ) AS T  
                    WHERE LW<=(SELECT MAX(T1.DayToReachTheTarget)
                   FROM ProductionOrderSchedulingParametersType1 AS T1 
                  INNER JOIN trn.ProductionOrder AS po ON t1.ProductionOrderID=po.Id WHERE po.Id='" + productionOrderID + @"' )";


            DataTable dtWorkCenterProductionHistory = _sqlRepository.GetDataTable(sqlLastRunningDays);
            DataView dvtemp = new DataView(dtWorkCenterProductionHistory);

            Dictionary<string, int> distinctWorkCenter = new Dictionary<string, int>();
            for (int i = 0; i < dtWorkCenterProductionHistory.Rows.Count; i++)
            {
                if (distinctWorkCenter.ContainsKey(dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString()) == false)
                {
                    dvtemp.RowFilter = "WorkCenterMasterId='" + dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() + "'";
                    string materialmasterid = dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString();
                    int ReverseCountDays = 0;
                    for (int R = 0; R < dvtemp.Count; R++)
                    {
                        if (materialmasterid == dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString())
                            ReverseCountDays++;
                        else
                            break;
                    }


                    dtWorkCenter.DefaultView.RowFilter = "WorkCenterMasterId='" + dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() + "'";
                    if (dtWorkCenter.DefaultView.Count > 0)
                        dtWorkCenter.DefaultView[0].Row["LastStyleRunningFor"] = ReverseCountDays;

                }

            }

            return dtWorkCenter;
        }
        private DataTable dtAllAvailableWrokcenters(string entityid, string processid)
        {

            string sql = @"SELECT WC.* FROM  [SCS].[WorkCenterMaster] WC WHERE WC.EntityId IN(" + entityid + @") AND WC.ProcessId='" + processid + @"' AND WC.[Active]=1 
                                ";
            DataTable dtWorkCenter = _sqlRepository.GetDataTable(sql);



            //final block for all workcenters with last production date
            string workcenterlist = "''";
            for (int i = 0; i < dtWorkCenter.Rows.Count; i++)
                workcenterlist += ",'" + dtWorkCenter.Rows[i]["ID"].ToString() + "'";


            //GETDATE()-1
            sql = @"SELECT WC.Id AS WorkCenterMasterId,WC.EntityId,WC.UserName AS WorkCenter,WC.StandardTimePerDay,WC.NoOfWorkStation, p.MaterialMasterId,WC.ProcessID,
                   -- FORMAT(ISNULL(p.ProductionDate,dateadd(DAY,-1, CONVERT(DATE, CASE WHEN ISNULL(e.StartDate,'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"')<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' THEN '" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' ELSE e.StartDate END))),'dd-MMM-yyyy') AS LastProductionDate,0 AS LastStyleRunningFor,
                    FORMAT(dateadd(DAY,-1, CONVERT(DATE, CASE WHEN ISNULL(e.StartDate,'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"')<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' THEN '" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' ELSE e.StartDate END)),'dd-MMM-yyyy') AS LastProductionDate,0 AS LastStyleRunningFor,
                           p.Quantity,0 AS CurrentPRQty--, p.ProductionHours
                      FROM [SCS].[WorkCenterMaster] WC 
                      INNER JOIN (SELECT WorkCenterMasterId,MAX(StartDate) AS StartDate FROM [SCS].[WorkCenterMasterEffectiveDate] GROUP BY WorkCenterMasterId) E ON e.WorkCenterMasterId=WC.Id
                    LEFT OUTER JOIN 
                    (
		                    SELECT  * FROM (	
					                    SELECT dense_rank() OVER (PARTITION BY t.WorkCenterMasterId ORDER BY t.ProductionDate DESC) AS RANK,
					                    t.WorkCenterMasterId,t.ProductionDate,t.Quantity,t.MaterialMasterId
					                    FROM (SELECT t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t 
					                    INNER JOIN [TRN].[ProductionOrder] P ON p.Id=t.ProductionOrderID
					                    LEFT OUTER JOIN (SELECT DISTINCT pod.ProductionOrderId,mm.Id AS MaterialMasterId
FROM trn.ProductionOrderDetail AS pod
INNER JOIN(SELECT distinct pod.ProductionOrderId,(select top(1) SalesOrderId from trn.ProductionOrderDetail where ProductionOrderId=POD.ProductionOrderId) SOId
FROM trn.ProductionOrderDetail POD) A ON A.ProductionOrderId=pod.ProductionOrderId
INNER JOIN trn.SalesOrder AS so ON A.SOId=so.Id
INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
INNER JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId) AS K ON k.ProductionOrderId=p.Id
                                    where t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
					                    GROUP BY t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId) AS T
		                    ) AS K WHERE K.[RANK]=1
                    ) AS P ON p.WorkCenterMasterId=wc.Id
                    WHERE WC.[Active]=1 AND WC.Id IN (" + workcenterlist + ")";
            dtWorkCenter = _sqlRepository.GetDataTable(sql);



            string sqlLastRunningDays = @"SELECT *
                    FROM ( SELECT dense_rank() OVER (PARTITION BY t.WorkCenterMasterId ORDER BY t.ProductionDate DESC) AS LW,
					                    t.WorkCenterMasterId,t.ProductionDate,t.Quantity,0 AS CurrentPRQty, t.MaterialMasterId
                           from (SELECT t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t 
					                    INNER JOIN [TRN].[ProductionOrder] P ON p.Id=t.ProductionOrderID
					                    LEFT OUTER JOIN (SELECT DISTINCT pod.ProductionOrderId,mm.Id AS MaterialMasterId
FROM trn.ProductionOrderDetail AS pod
INNER JOIN(SELECT distinct pod.ProductionOrderId,(select top(1) SalesOrderId from trn.ProductionOrderDetail where ProductionOrderId=POD.ProductionOrderId) SOId
FROM trn.ProductionOrderDetail POD) A ON A.ProductionOrderId=pod.ProductionOrderId
INNER JOIN trn.SalesOrder AS so ON A.SOId=so.Id
INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
INNER JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId) AS K ON k.ProductionOrderId=p.Id
                                        WHERE p.EntityId IN(" + entityid + @") AND t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
					                    GROUP BY t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId) AS T
                        ) AS T  
                    WHERE LW<=(SELECT MAX(T1.DayToReachTheTarget*10)
                   FROM ProductionOrderSchedulingParametersType1 AS T1 
                  INNER JOIN trn.ProductionOrder AS po ON t1.ProductionOrderID=po.Id WHERE po.EntityId IN(" + entityid + @")
                    )";


            DataTable dtWorkCenterProductionHistory = _sqlRepository.GetDataTable(sqlLastRunningDays);
            DataView dvtemp = new DataView(dtWorkCenterProductionHistory);

            Dictionary<string, int> distinctWorkCenter = new Dictionary<string, int>();

            for (int i = 0; i < dtWorkCenterProductionHistory.Rows.Count; i++)
            {

                if (distinctWorkCenter.ContainsKey(dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() + dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString()) == false)
                {
                    //if (dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() == "7")
                    //{

                    //}

                    dvtemp.RowFilter = "MaterialMasterId='" + dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString() + "' AND WorkCenterMasterId='" + dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() + "'";
                    string materialmasterid = dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString();
                    int ReverseCountDays = 0;
                    int PreviousRowSerial = 0;
                    for (int R = 0; R < dvtemp.Count; R++)
                    {
                        PreviousRowSerial++;
                        if (clsStaticInfo.dbl(dvtemp[R]["LW"].ToString()) == PreviousRowSerial)
                        {
                            if (materialmasterid == dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString())
                                ReverseCountDays++;
                            else
                                break;
                        }
                    }


                    dtWorkCenter.DefaultView.RowFilter = "MaterialMasterId='" + dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString() + "' AND WorkCenterMasterId='" + dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() + "'";
                    if (dtWorkCenter.DefaultView.Count > 0)
                        dtWorkCenter.DefaultView[0].Row["LastStyleRunningFor"] = ReverseCountDays;


                    distinctWorkCenter.Add(dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() + dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString(), ReverseCountDays);
                }

            }




            return dtWorkCenter;
        }

        private DataTable dtType2AllAvailableWrokcenters(string entityid, string processid)
        {

            string sql = @"SELECT WC.* FROM  [SCS].[WorkCenterMaster] WC WHERE WC.EntityId IN(" + entityid + @") AND WC.ProcessId='" + processid + @"' AND WC.[Active]=1 
                                ";
            DataTable dtWorkCenter = _sqlRepository.GetDataTable(sql);



            //final block for all workcenters with last production date
            string workcenterlist = "''";
            for (int i = 0; i < dtWorkCenter.Rows.Count; i++)
                workcenterlist += ",'" + dtWorkCenter.Rows[i]["ID"].ToString() + "'";


            //GETDATE()-1
            sql = @"SELECT WC.Id AS WorkCenterMasterId,WC.EntityId,WC.UserName AS WorkCenter,WC.StandardTimePerDay,WC.NoOfWorkStation, p.MaterialMasterId,WC.ProcessID,
                   -- FORMAT(ISNULL(p.ProductionDate,dateadd(DAY,-1, CONVERT(DATE, CASE WHEN ISNULL(e.StartDate,'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"')<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' THEN '" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' ELSE e.StartDate END))),'dd-MMM-yyyy') AS LastProductionDate,0 AS LastStyleRunningFor,
                    FORMAT(dateadd(DAY,-1, CONVERT(DATE, CASE WHEN ISNULL(e.StartDate,'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"')<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' THEN '" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' ELSE e.StartDate END)),'dd-MMM-yyyy') AS LastProductionDate,0 AS LastStyleRunningFor,
                           p.Quantity,0 AS CurrentPRQty--, p.ProductionHours
                      FROM [SCS].[WorkCenterMaster] WC 
                      INNER JOIN (SELECT WorkCenterMasterId,MAX(StartDate) AS StartDate FROM [SCS].[WorkCenterMasterEffectiveDate] GROUP BY WorkCenterMasterId) E ON e.WorkCenterMasterId=WC.Id
                    LEFT OUTER JOIN 
                    (
		                    SELECT  * FROM (	
					                    SELECT dense_rank() OVER (PARTITION BY t.WorkCenterMasterId ORDER BY t.ProductionDate DESC) AS RANK,
					                    t.WorkCenterMasterId,t.ProductionDate,t.Quantity,t.MaterialMasterId
					                    FROM (SELECT t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t 
					                    INNER JOIN [TRN].[ProductionOrderType2] P ON p.Id=t.ProductionOrderID
					                    LEFT OUTER JOIN (SELECT DISTINCT pod.ProductionOrderId,mm.Id AS MaterialMasterId
FROM trn.ProductionOrderType2Detail AS pod
INNER JOIN(SELECT distinct pod.ProductionOrderId,(select top(1) SalesOrderId from trn.ProductionOrderType2Detail where ProductionOrderId=POD.ProductionOrderId) SOId
FROM trn.ProductionOrderType2Detail POD) A ON A.ProductionOrderId=pod.ProductionOrderId
INNER JOIN trn.SalesOrder AS so ON A.SOId=so.Id
INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
INNER JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId) AS K ON k.ProductionOrderId=p.Id
                                    where t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
					                    GROUP BY t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId) AS T
		                    ) AS K WHERE K.[RANK]=1
                    ) AS P ON p.WorkCenterMasterId=wc.Id
                    WHERE WC.[Active]=1 AND WC.Id IN (" + workcenterlist + ")";
            dtWorkCenter = _sqlRepository.GetDataTable(sql);



            string sqlLastRunningDays = @"SELECT *
                    FROM ( SELECT dense_rank() OVER (PARTITION BY t.WorkCenterMasterId ORDER BY t.ProductionDate DESC) AS LW,
					                    t.WorkCenterMasterId,t.ProductionDate,t.Quantity,0 AS CurrentPRQty, t.MaterialMasterId
                           from (SELECT t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t 
					                    INNER JOIN [TRN].[ProductionOrder] P ON p.Id=t.ProductionOrderID
					                    LEFT OUTER JOIN (SELECT DISTINCT pod.ProductionOrderId,mm.Id AS MaterialMasterId
FROM trn.ProductionOrderType2Detail AS pod
INNER JOIN(SELECT distinct pod.ProductionOrderId,(select top(1) SalesOrderId from trn.ProductionOrderType2Detail where ProductionOrderId=POD.ProductionOrderId) SOId
FROM trn.ProductionOrderType2Detail POD) A ON A.ProductionOrderId=pod.ProductionOrderId
INNER JOIN trn.SalesOrder AS so ON A.SOId=so.Id
INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
INNER JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId) AS K ON k.ProductionOrderId=p.Id
                                        WHERE p.EntityId IN(" + entityid + @") AND t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
					                    GROUP BY t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId) AS T
                        ) AS T  
                    WHERE LW<=(SELECT MAX(T1.DayToReachTheTarget*10)
                   FROM ProductionOrderSchedulingParametersType2 AS T1 
                  INNER JOIN trn.ProductionOrderType2 AS po ON t1.ProductionOrderID=po.Id WHERE po.EntityId IN(" + entityid + @")
                    )";


            DataTable dtWorkCenterProductionHistory = _sqlRepository.GetDataTable(sqlLastRunningDays);
            DataView dvtemp = new DataView(dtWorkCenterProductionHistory);

            Dictionary<string, int> distinctWorkCenter = new Dictionary<string, int>();

            for (int i = 0; i < dtWorkCenterProductionHistory.Rows.Count; i++)
            {

                if (distinctWorkCenter.ContainsKey(dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() + dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString()) == false)
                {
                    //if (dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() == "7")
                    //{

                    //}

                    dvtemp.RowFilter = "MaterialMasterId='" + dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString() + "' AND WorkCenterMasterId='" + dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() + "'";
                    string materialmasterid = dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString();
                    int ReverseCountDays = 0;
                    int PreviousRowSerial = 0;
                    for (int R = 0; R < dvtemp.Count; R++)
                    {
                        PreviousRowSerial++;
                        if (clsStaticInfo.dbl(dvtemp[R]["LW"].ToString()) == PreviousRowSerial)
                        {
                            if (materialmasterid == dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString())
                                ReverseCountDays++;
                            else
                                break;
                        }
                    }


                    dtWorkCenter.DefaultView.RowFilter = "MaterialMasterId='" + dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString() + "' AND WorkCenterMasterId='" + dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() + "'";
                    if (dtWorkCenter.DefaultView.Count > 0)
                        dtWorkCenter.DefaultView[0].Row["LastStyleRunningFor"] = ReverseCountDays;


                    distinctWorkCenter.Add(dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() + dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString(), ReverseCountDays);
                }

            }




            return dtWorkCenter;
        }

        private DataTable dtProductionParameters(string entityid)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
            connection.BeginTransaction();
            connection.executeQuery(@"update  trn.ProductionOrder  SET Qty = k.OrderQty,PlannedQty =k.PlannedQty
                                        FROM trn.ProductionOrder AS po
                                        INNER JOIN (
                                        select pod.ProductionOrderId,
                                        SUM(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty,
                                        --SUM(CEILING((so.Qty*(1+(moi.ExtraOrderPercentage/100)))*(1+(moi.OrderWastagePercentage/100)))) AS PlannedQty,
                                                                    sum(SO.Qty) AS OrderQty 

					                                        from trn.ProductionOrderDetail POD 
                                                                    left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                                                                    left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                                    INNER JOIN hkp.OrderStatus AS os ON os.Id=so.OrderStatusId
                            
                                                            WHERE os.Id='" + Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"'
                                        GROUP BY pod.ProductionOrderId
                                    ) AS K ON k.ProductionOrderId=po.Id");
            connection.CommitTransaction();

            connection = new ConnectionManager.clsConnection();
            connection.BeginTransaction();
            connection.executeQuery(@"delete FROM ProductionPlanningType1 WHERE ProductionOrderID IN (
                                SELECT po.Id FROM trn.ProductionOrder AS po
                                INNER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId
                                WHERE ps.UserName NOT IN ('" + PlanningStatus.ACTIVE.ToString() + @"','" + PlanningStatus.RUNNING.ToString() + @"')
                                )");
            connection.CommitTransaction();

            string sql = @"SELECT  PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,mm.MaterialMasterId,
                            --ISNULL(PO.PlannedQty,0)-ISNULL(PRODPR.ProductionQtyAtPR,0) AS SOQuantity,ISNULL(PRODPR.ProductionQtyAtPR,0) AS ProductionQty,
                            ISNULL(CASE WHEN ISNULL(T1.Qty,0)>0 THEN T1.Qty ELSE PO.PlannedQty END,0)-(ISNULL(PRODPR.ProductionQtyAtPR,0)-ISNULL(PRDQ.ProductionBookedQty,0)) AS SOQuantity,ISNULL(PRODPR.ProductionQtyAtPR,0) AS ProductionQty,
                           t1.*
                                                            FROM [TRN].[ProductionOrder] AS PO
                                                        JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                                                        INNER JOIN (
														SELECT DISTINCT pod.ProductionOrderId,mm.Id AS MaterialMasterId
FROM trn.ProductionOrderDetail AS pod
INNER JOIN(SELECT distinct pod.ProductionOrderId,(select top(1) SalesOrderId from trn.ProductionOrderDetail where ProductionOrderId=POD.ProductionOrderId) SOId
FROM trn.ProductionOrderDetail POD) A ON A.ProductionOrderId=pod.ProductionOrderId
INNER JOIN trn.SalesOrder AS so ON A.SOId=so.Id
INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
INNER JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId
							) AS MM ON mm.ProductionOrderId=po.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.ProductionStatusId = PS.Id
                            INNER JOIN ProductionOrderSchedulingParametersType1 t1 ON t1.ProductionOrderID=po.Id
                            LEFT OUTER  JOIN (SELECT pod.ProductionOrderId,SUM(so.Qty) AS Qty
                                                FROM trn.SalesOrder AS so
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id GROUP BY pod.ProductionOrderId) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                           
							--production at PR Level
							LEFT OUTER JOIN (
												SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
											FROM  trn.ProductionSummary S 
											WHERE  CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PRODPR ON  PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
							
                            left outer join (SELECT pod.ProductionOrderId,
                                sum(isnull(so.ProductionBookedQty,0)) ProductionBookedQty
                                 FROM trn.SalesOrder AS so
                                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id

                                GROUP BY pod.ProductionOrderId
                            ) AS PRDQ ON PRDQ.ProductionOrderId=T1.ProductionOrderId
							

                            WHERE 
                       po.EntityId IN(" + entityid + @")  AND ps.UserName IN ('" + PlanningStatus.ACTIVE.ToString() + @"','" + PlanningStatus.RUNNING.ToString() + @"')
                            ORDER BY ps.UserName DESC, t1.ProductionPriority ASC";
            DataTable _dtProductionParameters = _sqlRepository.GetDataTable(sql);

            return _dtProductionParameters;
        }
        private Dictionary<string, DataTable> dtProductionCalendar(DateTime startDate, int noOfWorkingDays, string processid, string entityid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from (SELECT 
dense_rank() OVER (PARTITION BY ppc.EntityID ORDER BY EntityID,ppc.WorkingDate)
AS rnk,ppc.*
                                     FROM ProductionPlanningCalendar AS ppc 
                            WHERE ppc.WorkingDate>='" + startDate.ToString("dd-MMM-yyyy") + @"' AND ppc.WorkingHours>0 
                            AND ppc.ProcessID='" + processid + @"' AND ppc.EntityID IN(" + entityid + @")) as ppc WHERE rnk<=" + noOfWorkingDays + @"
                            ORDER BY  ppc.EntityID, ppc.WorkingDate ASC";
            DataTable _dtProductionParameters = _sqlRepository.GetDataTable(sql);
            if (_dtProductionParameters.Rows.Count == 0)
                throw new Exception("No calendar was defined for selected entity");

            Dictionary<string, DataTable> dicCalendar = new Dictionary<string, DataTable>();
            DataTable dt = _dtProductionParameters.Clone();
            string _entityId = "";
            for (int i = 0; i < _dtProductionParameters.Rows.Count; i++)
            {
                if (_entityId != _dtProductionParameters.Rows[i]["EntityID"].ToString())
                {
                    dt = _dtProductionParameters.Clone();
                    dicCalendar.Add(_dtProductionParameters.Rows[i]["EntityID"].ToString(), dt);
                }
                dt.ImportRow(_dtProductionParameters.Rows[i]);

                _entityId = _dtProductionParameters.Rows[i]["EntityID"].ToString();
            }

            return dicCalendar;


        }

        private Dictionary<string, DataTable> dtProductionType2Calendar(DateTime startDate, int noOfWorkingDays, string processid, string entityid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from (SELECT 
dense_rank() OVER (PARTITION BY ppc.EntityID ORDER BY EntityID,ppc.WorkingDate)
AS rnk,ppc.*
                                     FROM ProductionPlanningType2Calendar AS ppc 
                            WHERE ppc.WorkingDate>='" + startDate.ToString("dd-MMM-yyyy") + @"' AND ppc.WorkingHours>0 
                            AND ppc.ProcessID='" + processid + @"' AND ppc.EntityID IN(" + entityid + @")) as ppc WHERE rnk<=" + noOfWorkingDays + @"
                            ORDER BY  ppc.EntityID, ppc.WorkingDate ASC";
            DataTable _dtProductionParameters = _sqlRepository.GetDataTable(sql);
            if (_dtProductionParameters.Rows.Count == 0)
                throw new Exception("No calendar was defined for selected entity");

            Dictionary<string, DataTable> dicCalendar = new Dictionary<string, DataTable>();
            DataTable dt = _dtProductionParameters.Clone();
            string _entityId = "";
            for (int i = 0; i < _dtProductionParameters.Rows.Count; i++)
            {
                if (_entityId != _dtProductionParameters.Rows[i]["EntityID"].ToString())
                {
                    dt = _dtProductionParameters.Clone();
                    dicCalendar.Add(_dtProductionParameters.Rows[i]["EntityID"].ToString(), dt);
                }
                dt.ImportRow(_dtProductionParameters.Rows[i]);

                _entityId = _dtProductionParameters.Rows[i]["EntityID"].ToString();
            }

            return dicCalendar;


        }

        #region Production Plan Simulation Visualization
        int maxDaysToShow = 30;

        [HttpPost, Authorize]
        public JsonResult GetScheduleData(string entityid, string processid, int year, int month, int day)
        {
            //CONVERT(INT, pt.WorkCenterMasterId)
            month = month + 1;

            DateTime startDate = new DateTime(year, month, day);
            DateTime endDate = new DateTime(year, month, day).AddDays(maxDaysToShow);


            string sql = @"SELECT K.FilterData, K.seq, K.Id, K.Entity, K.WorkCenterMasterId, K.WorkCenter,
       K.ProductionOrderID, K.Quantity, K.Color, K.ProductionDate, K.isBuildUp,
       K.isStyleChange, K.planningStatus, K.[Description], K.[Subject],
       K.StartTime,CASE WHEN K.seq>1 THEN FORMAT( CONVERT(DATE,K.ProductionDate),'MMM dd yyyy hh:mm:ss tt') ELSE  K.EndTime END AS EndTime,
        CASE WHEN K.seq>1 THEN CONVERT(BIT,0) ELSE CONVERT(BIT,1) END AS AllDay, K.Recurrence, K.AppTaskId, K.ParentId,
       K.FailedToCommitmentDate FROM (SELECT 1 AS FilterData,  DENSE_RANK() OVER (PARTITION BY 
                                                         PT.WorkCenterMasterId,
                                                         PT.ProductionDate ORDER BY PT.ID) AS seq, pt.Id,e.UserName AS Entity,pt.WorkCenterMasterId,wc.UserName AS WorkCenter,pt.ProductionOrderID,pt.Quantity,t1.Color,
                            FORMAT(pt.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,PT.isBuildUp,PT.isStyleChange,
                      CASE WHEN ISNULL(FC.Id,'')='' THEN upper(ps.UserName) ELSE 'FREEZE' END AS planningStatus,
                            --CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Description],
                            --CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Subject],
                            --pt.ProductionOrderID AS [Description],
                            --pt.ProductionOrderID AS [Subject],
                            ' ' AS [Description],
                            ' ' AS [Subject],
                            FORMAT(pt.ProductionDate,'MMM dd yyyy hh:mm:ss tt') AS StartTime,
                            FORMAT(pt.ProductionDate,'MMM dd yyyy')+' 11:59:59 PM' AS EndTime,
                             CONVERT(BIT,0) AS AllDay,CONVERT(BIT,0) AS Recurrence, pt.Id AS AppTaskId,pt.ID AS ParentId, 
                            convert(bit,case when fail.LastPlanDate> t1.CommitmentDate then 1 else 0 end) as FailedToCommitmentDate
--'UTC +06:00' AS [EndTimeZone],'UTC +06:00' AS [StartTimeZone]
                            --Mon Jun 22 2019 23:59:00
                              FROM ProductionPlanningType1 AS PT
                            LEFT OUTER JOIN [TRN].[ProductionOrder] P ON p.Id=pt.ProductionOrderID
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T1 ON t1.ProductionOrderID=pt.ProductionOrderID
                            LEFT OUTER JOIN [SCS].[WorkCenterMaster] WC ON wc.Id=pt.WorkCenterMasterId
                            LEFT OUTER JOIN [MST].[MaterialMaster] MM ON mm.Id=pt.MaterialMasterId
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=P.ProductionStatusId
                            LEFT OUTER JOIN trn.FreezeConfigPlanningType1 AS FC ON fc.EntityId=pt.EntityID AND fc.FreezeDate 
											BETWEEN (SELECT MIN(ProductionDate) FROM ProductionPlanningType1 WHERE ProductionOrderID=pt.ProductionOrderID)
											AND (SELECT MAX(ProductionDate) FROM ProductionPlanningType1 WHERE ProductionOrderID=pt.ProductionOrderID)
                            
                            LEFT OUTER JOIN (select ProductionOrderID,Max(ProductionDate) AS LastPlanDate  from ProductionPlanningType1 group by productionOrderID) 
							AS FAIL on fail.ProductionOrderID=pt.ProductionOrderID
                            INNER JOIN org.Entity AS e ON e.Id=p.EntityId
                            WHERE pt.ProductionDate between '" + startDate.ToString("dd-MMM-yyyy") + @"' and '" + endDate.ToString("dd-MMM-yyyy") + @"' AND WC.EntityID='" + entityid + @"' AND WC.processid='" + processid + @"'
                            ) AS K 
--WHERE K.seq=1
ORDER BY k.Entity,K.WorkCenterMasterId,CONVERT(DATE, K.ProductionDate) ";

            //    string sql = @"SELECT  pt.Id,e.UserName AS Entity,WC.Id AS WorkCenterMasterId,wc.UserName AS WorkCenter,pt.ProductionOrderID,pt.Quantity,t1.Color,
            //                    FORMAT(pt.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,
            //                    CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Description],
            //                    CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Subject],
            //                    FORMAT(pt.ProductionDate,'MMM dd yyyy hh:mm:ss tt') AS StartTime,
            //                    FORMAT(pt.ProductionDate,'MMM dd yyyy')+' 11:59:59 PM' AS EndTime,
            //                    'true' AS AllDay,'false' AS Recurrence, pt.Id AS AppTaskId,pt.ID AS ParentId--, 
            //                      FROM [SCS].[WorkCenterMaster] WC
            //LEFT OUTER JOIN  ProductionPlanningType1 AS PT ON wc.Id=pt.WorkCenterMasterId
            //                    LEFT OUTER JOIN [TRN].[ProductionOrder] P ON p.Id=pt.ProductionOrderID
            //                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T1 ON t1.ProductionOrderID=pt.ProductionOrderID

            //                    LEFT OUTER JOIN [MST].[MaterialMaster] MM ON mm.Id=pt.MaterialMasterId
            //                    LEFT OUTER JOIN org.Entity AS e ON e.Id=p.EntityId
            //                    ORDER BY e.UserName,pt.WorkCenterMasterId,pt.ProductionDate";

            //DataTable _dtProductionParameters = _sqlRepository.GetDataTable(sql);



            string sqlWC = @" SELECT wc.Id,WC.Sequence,
                       CASE WHEN ISNULL(E.StartDate,'')='' THEN wc.UserName+' (Missing Start Date)'  ELSE wc.UserName + ' ('+isnull(ei.EmployeeName, '')+')'  END AS UserName
                                  FROM [SCS].[WorkCenterMaster] WC 
                            LEFT OUTER JOIN (SELECT WorkCenterMasterId,MAX(StartDate) AS StartDate FROM [SCS].[WorkCenterMasterEffectiveDate] GROUP BY WorkCenterMasterId) E ON e.WorkCenterMasterId=wc.Id
                            LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=wc.ResponsiblePersonId
                             WHERE WC.EntityID='" + entityid
                + @"' AND WC.[Active]=1 AND WC.ProcessId='" + processid + "' ORDER BY WC.Sequence";
            DataTable _dtWC = _sqlRepository.GetDataTable(sqlWC);


            string sqlFreeze = "select * from  trn.FreezeConfigPlanningType1 WHERE EntityId='" + entityid
              + @"' ";

            DateTime freezedate = System.DateTime.Now.AddYears(-100);
            DataTable dtFreeze = _sqlRepository.GetDataTable(sqlFreeze);
            if (dtFreeze.Rows.Count > 0)
                freezedate = Convert.ToDateTime(dtFreeze.Rows[0]["FreezeDate"].ToString());


            List<GroupData> groupData = new List<GroupData>();
            for (int i = 0; i < _dtWC.Rows.Count; i++)
            {
                groupData.Add(new GroupData
                {
                    id = _dtWC.Rows[i]["Id"].ToString(),
                    text = _dtWC.Rows[i]["UserName"].ToString()

                });
            }


            string sqlWORKDAYDATA = @"SELECT distinct FORMAT(ppc.WorkingDate, 'dddd') AS WorkingDays
                                    FROM ProductionPlanningCalendar AS ppc WHERE ISNULL(ppc.WorkingHours,0)>0 AND ppc.EntityID = '" + entityid + "'";

            DataTable dtWorkDays = _sqlRepository.GetDataTable(sqlWORKDAYDATA);
            List<string> days = new List<string>();
            for (int i = 0; i < dtWorkDays.Rows.Count; i++)
            {
                days.Add(dtWorkDays.Rows[i]["WorkingDays"].ToString());
            }
            var jsondata = Json(new { FREEZEDATE = freezedate.ToString("dd-MMM-yyyy"), DATA = _sqlRepository.GetDataCollection(sql), WORKDAYDATA = days, GROUPDATA = groupData }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost, Authorize]
        public JsonResult GetScheduleDataFiltered(string entityid, string processid, int year, int month, int day, Dictionary<string, string> parameters)
        {
            //CONVERT(INT, pt.WorkCenterMasterId)
            month = month + 1;

            DateTime startDate = new DateTime(year, month, day);
            DateTime endDate = new DateTime(year, month, day).AddDays(maxDaysToShow);


            string sql = @"SELECT CASE WHEN ISNULL(FIL.ProductionOrderId,0)=0 THEN 0 ELSE 1 END AS FilterData, K.seq, K.Id, K.Entity, K.WorkCenterMasterId, K.WorkCenter,
       K.ProductionOrderID, K.Quantity, K.Color, K.ProductionDate, K.isBuildUp,
       K.isStyleChange, K.planningStatus, K.[Description], K.[Subject],
       K.StartTime,CASE WHEN K.seq>1 THEN FORMAT( CONVERT(DATE,K.ProductionDate),'MMM dd yyyy hh:mm:ss tt') ELSE  K.EndTime END AS EndTime,
        CASE WHEN K.seq>1 THEN CONVERT(BIT,0) ELSE CONVERT(BIT,1) END AS AllDay, K.Recurrence, K.AppTaskId, K.ParentId,
       K.FailedToCommitmentDate FROM (SELECT 1 AS FilterData,  DENSE_RANK() OVER (PARTITION BY 
                                                         PT.WorkCenterMasterId,
                                                         PT.ProductionDate ORDER BY PT.ID) AS seq, pt.Id,e.UserName AS Entity,pt.WorkCenterMasterId,wc.UserName AS WorkCenter,pt.ProductionOrderID,pt.Quantity,t1.Color,
                            FORMAT(pt.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,PT.isBuildUp,PT.isStyleChange,
                      CASE WHEN ISNULL(FC.Id,'')='' THEN upper(ps.UserName) ELSE 'FREEZE' END AS planningStatus,WC.EntityID,pt.ProcessID,
                            --CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Description],
                            --CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Subject],
                            --pt.ProductionOrderID AS [Description],
                            --pt.ProductionOrderID AS [Subject],
                            ' ' AS [Description],
                            ' ' AS [Subject],
                            FORMAT(pt.ProductionDate,'MMM dd yyyy hh:mm:ss tt') AS StartTime,
                            FORMAT(pt.ProductionDate,'MMM dd yyyy')+' 11:59:59 PM' AS EndTime,
                             CONVERT(BIT,0) AS AllDay,CONVERT(BIT,0) AS Recurrence, pt.Id AS AppTaskId,pt.ID AS ParentId, 
                            convert(bit,case when fail.LastPlanDate> t1.CommitmentDate then 1 else 0 end) as FailedToCommitmentDate
--'UTC +06:00' AS [EndTimeZone],'UTC +06:00' AS [StartTimeZone]
                            --Mon Jun 22 2019 23:59:00
                              FROM ProductionPlanningType1 AS PT
                            LEFT OUTER JOIN [TRN].[ProductionOrder] P ON p.Id=pt.ProductionOrderID
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T1 ON t1.ProductionOrderID=pt.ProductionOrderID
                            LEFT OUTER JOIN [SCS].[WorkCenterMaster] WC ON wc.Id=pt.WorkCenterMasterId
                            LEFT OUTER JOIN [MST].[MaterialMaster] MM ON mm.Id=pt.MaterialMasterId
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=P.ProductionStatusId
                            LEFT OUTER JOIN trn.FreezeConfigPlanningType1 AS FC ON fc.EntityId=pt.EntityID AND fc.FreezeDate 
											BETWEEN (SELECT MIN(ProductionDate) FROM ProductionPlanningType1 WHERE ProductionOrderID=pt.ProductionOrderID)
											AND (SELECT MAX(ProductionDate) FROM ProductionPlanningType1 WHERE ProductionOrderID=pt.ProductionOrderID)
                            
                            LEFT OUTER JOIN (select ProductionOrderID,Max(ProductionDate) AS LastPlanDate  from ProductionPlanningType1 group by productionOrderID) 
							AS FAIL on fail.ProductionOrderID=pt.ProductionOrderID
                            INNER JOIN org.Entity AS e ON e.Id=p.EntityId
                            ) AS K 
                            
left outer join (select distinct po.Id AS ProductionOrderId,p1.WorkCenterMasterId
from trn.ProductionOrder PO
				inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
				INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID and ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
	
				left outer join scs.WorkCenterMaster WC on wc.id=p1.WorkCenterMasterId
			
				INNER JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=PO.Id
				LEFT OUTER JOIN trn.SalesOrder SO ON so.Id=pod.SalesOrderId
				left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
				left outer join mst.MaterialMaster mm on mm.id=p1.MaterialMasterId
				--LEFT OUTER JOIN MST.MaterialMasterArticle MMR ON mmr.Id=moi.ArticleId
 
				left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
				left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
				left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

				left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
				left outer join [HKP].Buyer B on B.Id=MO.BuyerId
				left outer join [HKP].[Party] p on P.Id=MO.PartyId

				left outer join org.Entity E on e.Id=p1.EntityID
				LEFT OUTER JOIN org.Unit AS u ON u.Id=e.UnitId
				left outer join org.Plant PLN on pln.Id=PO.PlantId
		

WHERE  
isnull(wc.Id,'') IN(" + parameters["WorkCenterId"] + @") AND 
isnull(PO.Id,'') IN(" + parameters["ProductOrderId"] + @") AND 
isnull(MO.Id,'') IN(" + parameters["MasterOrderNo"] + @") AND 
isnull(MO.BuyerReferenceNo,'') IN(" + parameters["BuyerOrderNo"] + @") AND 
isnull(moi.BuyerReferenceNo,'') IN(" + parameters["BuyerItemNo"] + @") AND 

isnull(PM.Id,'') IN(" + parameters["ProductMasterId"] + @") AND 
isnull(PC.Id,'') IN (" + parameters["ProductCategoryId"] + @") AND
isnull(p1.MaterialMasterId,'') IN(" + parameters["MaterialMasterId"] + @") AND 
isnull(moi.ArticleId,'') IN (" + parameters["ArticleId"] + @") AND
isnull(b.Id,'') IN(" + parameters["BuyerId"] + @") AND 
isnull(p.Id,'') IN (" + parameters["CustomerId"] + @") AND 
isnull(WC.AccountHolder,'') IN (" + parameters["AccountHolderId"] + @") AND 
isnull(WC.AccountInCharge,'') IN (" + parameters["AccountInchargeId"] + @") AND
isnull(po.ProductionStatusId,'') IN (" + parameters["ProductionStatusId"] + @")


            ) AS FIL ON FIL.ProductionOrderId=K.ProductionOrderID AND FIL.WorkCenterMasterId=K.WorkCenterMasterId



                            LEFT OUTER JOIN (select ProductionOrderID,Max(ProductionDate) AS LastPlanDate  from ProductionPlanningType1 group by productionOrderID) 
							AS FAIL on fail.ProductionOrderID=K.ProductionOrderID
                            INNER JOIN org.Entity AS e ON e.Id=K.EntityId
                            WHERE CONVERT(DATE,K.ProductionDate) between '" + startDate.ToString("dd-MMM-yyyy") + @"' and '" + endDate.ToString("dd-MMM-yyyy") + @"' AND K.EntityID='" + entityid + @"' AND K.processid='" + processid + @"'
                            ORDER BY e.UserName,K.WorkCenterMasterId,K.ProductionDate ";



            string sqlWC = @"  SELECT wc.Id,
                            CASE WHEN ISNULL(E.StartDate,'')='' THEN wc.UserName+' (Missing Start Date)'  ELSE wc.UserName + ' ('+isnull(ei.EmployeeName, '')+')'  END AS UserName
                              FROM [SCS].[WorkCenterMaster] WC 
                            LEFT OUTER JOIN (SELECT WorkCenterMasterId,MAX(StartDate) AS StartDate FROM [SCS].[WorkCenterMasterEffectiveDate] GROUP BY WorkCenterMasterId) E ON e.WorkCenterMasterId=wc.Id
                            LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=wc.ResponsiblePersonId WHERE WC.EntityID='" + entityid
                + @"' AND WC.ProcessId='" + processid + "' AND WC.[Active]=1 ORDER BY WC.UserName";
            DataTable _dtWC = _sqlRepository.GetDataTable(sqlWC);


            string sqlFreeze = "select * from  trn.FreezeConfigPlanningType1 WHERE EntityId='" + entityid
              + @"' ";

            DateTime freezedate = System.DateTime.Now.AddYears(-100);
            DataTable dtFreeze = _sqlRepository.GetDataTable(sqlFreeze);
            if (dtFreeze.Rows.Count > 0)
                freezedate = Convert.ToDateTime(dtFreeze.Rows[0]["FreezeDate"].ToString());


            List<GroupData> groupData = new List<GroupData>();
            for (int i = 0; i < _dtWC.Rows.Count; i++)
            {
                groupData.Add(new GroupData
                {
                    id = _dtWC.Rows[i]["Id"].ToString(),
                    text = _dtWC.Rows[i]["UserName"].ToString()

                });
            }


            string sqlWORKDAYDATA = @"SELECT distinct FORMAT(ppc.WorkingDate, 'dddd') AS WorkingDays
                                    FROM ProductionPlanningCalendar AS ppc WHERE ISNULL(ppc.WorkingHours,0)>0 AND ppc.EntityID = '" + entityid + "'";

            DataTable dtWorkDays = _sqlRepository.GetDataTable(sqlWORKDAYDATA);
            List<string> days = new List<string>();
            for (int i = 0; i < dtWorkDays.Rows.Count; i++)
            {
                days.Add(dtWorkDays.Rows[i]["WorkingDays"].ToString());
            }

            var jsondata = Json(new { FREEZEDATE = freezedate.ToString("dd-MMM-yyyy"), DATA = _sqlRepository.GetDataCollection(sql), WORKDAYDATA = days, GROUPDATA = groupData }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [HttpPost, Authorize]
        public JsonResult GetNewScheduleData(string entityid, string processid, int year, int month, int day)
        {
            //CONVERT(INT, pt.WorkCenterMasterId)
            month = month + 1;

            DateTime startDate = new DateTime(year, month, day);
            DateTime endDate = new DateTime(year, month, day).AddDays(maxDaysToShow);


            string sql = @"SELECT K.FilterData, K.seq, K.Id, K.Entity, K.WorkCenterMasterId, K.WorkCenter,
       K.ProductionOrderID, K.Quantity, K.Color, K.ProductionDate, K.isBuildUp,
       K.isStyleChange, K.planningStatus, K.[Description], K.[Subject],
       K.StartTime,CASE WHEN K.seq>1 THEN FORMAT( CONVERT(DATE,K.ProductionDate),'MMM dd yyyy hh:mm:ss tt') ELSE  K.EndTime END AS EndTime,
        CASE WHEN K.seq>1 THEN CONVERT(BIT,0) ELSE CONVERT(BIT,1) END AS AllDay, K.Recurrence, K.AppTaskId, K.ParentId,
       K.FailedToCommitmentDate FROM (SELECT 1 AS FilterData,  DENSE_RANK() OVER (PARTITION BY 
                                                         PT.WorkCenterMasterId,
                                                         PT.ProductionDate ORDER BY PT.ID) AS seq, pt.Id,e.UserName AS Entity,pt.WorkCenterMasterId,wc.UserName AS WorkCenter,pt.ProductionOrderID,pt.Quantity,t1.Color,
                            FORMAT(pt.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,PT.isBuildUp,PT.isStyleChange,
                      CASE WHEN ISNULL(FC.Id,'')='' THEN upper(ps.UserName) ELSE 'FREEZE' END AS planningStatus,
                            --CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Description],
                            --CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Subject],
                            --pt.ProductionOrderID AS [Description],
                            --pt.ProductionOrderID AS [Subject],
                            ' ' AS [Description],
                            ' ' AS [Subject],
                            FORMAT(pt.ProductionDate,'MMM dd yyyy hh:mm:ss tt') AS StartTime,
                            FORMAT(pt.ProductionDate,'MMM dd yyyy')+' 11:59:59 PM' AS EndTime,
                             CONVERT(BIT,0) AS AllDay,CONVERT(BIT,0) AS Recurrence, pt.Id AS AppTaskId,pt.ID AS ParentId, 
                            convert(bit,case when fail.LastPlanDate> t1.CommitmentDate then 1 else 0 end) as FailedToCommitmentDate
--'UTC +06:00' AS [EndTimeZone],'UTC +06:00' AS [StartTimeZone]
                            --Mon Jun 22 2019 23:59:00
                              FROM ProductionPlanningType1 AS PT
                            LEFT OUTER JOIN [TRN].[ProductionOrder] P ON p.Id=pt.ProductionOrderID
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T1 ON t1.ProductionOrderID=pt.ProductionOrderID
                            LEFT OUTER JOIN [SCS].[WorkCenterMaster] WC ON wc.Id=pt.WorkCenterMasterId
                            LEFT OUTER JOIN [MST].[MaterialMaster] MM ON mm.Id=pt.MaterialMasterId
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=P.ProductionStatusId
                            LEFT OUTER JOIN trn.FreezeConfigPlanningType1 AS FC ON fc.EntityId=pt.EntityID AND fc.FreezeDate 
											BETWEEN (SELECT MIN(ProductionDate) FROM ProductionPlanningType1 WHERE ProductionOrderID=pt.ProductionOrderID)
											AND (SELECT MAX(ProductionDate) FROM ProductionPlanningType1 WHERE ProductionOrderID=pt.ProductionOrderID)
                            
                            LEFT OUTER JOIN (select ProductionOrderID,Max(ProductionDate) AS LastPlanDate  from ProductionPlanningType1 group by productionOrderID) 
							AS FAIL on fail.ProductionOrderID=pt.ProductionOrderID
                            INNER JOIN org.Entity AS e ON e.Id=p.EntityId
                            WHERE pt.ProductionDate between '" + startDate.ToString("dd-MMM-yyyy") + @"' and '" + endDate.ToString("dd-MMM-yyyy") + @"' AND WC.EntityID IN(" + entityid + @") AND WC.processid='" + processid + @"'
                            ) AS K 
--WHERE K.seq=1
ORDER BY k.Entity,K.WorkCenterMasterId,CONVERT(DATE, K.ProductionDate) ";

            //    string sql = @"SELECT  pt.Id,e.UserName AS Entity,WC.Id AS WorkCenterMasterId,wc.UserName AS WorkCenter,pt.ProductionOrderID,pt.Quantity,t1.Color,
            //                    FORMAT(pt.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,
            //                    CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Description],
            //                    CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Subject],
            //                    FORMAT(pt.ProductionDate,'MMM dd yyyy hh:mm:ss tt') AS StartTime,
            //                    FORMAT(pt.ProductionDate,'MMM dd yyyy')+' 11:59:59 PM' AS EndTime,
            //                    'true' AS AllDay,'false' AS Recurrence, pt.Id AS AppTaskId,pt.ID AS ParentId--, 
            //                      FROM [SCS].[WorkCenterMaster] WC
            //LEFT OUTER JOIN  ProductionPlanningType1 AS PT ON wc.Id=pt.WorkCenterMasterId
            //                    LEFT OUTER JOIN [TRN].[ProductionOrder] P ON p.Id=pt.ProductionOrderID
            //                    LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T1 ON t1.ProductionOrderID=pt.ProductionOrderID

            //                    LEFT OUTER JOIN [MST].[MaterialMaster] MM ON mm.Id=pt.MaterialMasterId
            //                    LEFT OUTER JOIN org.Entity AS e ON e.Id=p.EntityId
            //                    ORDER BY e.UserName,pt.WorkCenterMasterId,pt.ProductionDate";

            //DataTable _dtProductionParameters = _sqlRepository.GetDataTable(sql);



            string sqlWC = @" SELECT wc.Id,WC.Sequence,
                       CASE WHEN ISNULL(E.StartDate,'')='' THEN wc.UserName+' (Missing Start Date)'  ELSE wc.UserName + ' ('+isnull(ei.EmployeeName, '')+')'  END AS UserName
                                  FROM [SCS].[WorkCenterMaster] WC 
                            LEFT OUTER JOIN (SELECT WorkCenterMasterId,MAX(StartDate) AS StartDate FROM [SCS].[WorkCenterMasterEffectiveDate] GROUP BY WorkCenterMasterId) E ON e.WorkCenterMasterId=wc.Id
                            LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=wc.ResponsiblePersonId
                             WHERE WC.EntityID IN(" + entityid + @") AND WC.[Active]=1 AND WC.ProcessId='" + processid + "' ORDER BY WC.Sequence";
            DataTable _dtWC = _sqlRepository.GetDataTable(sqlWC);


            string sqlFreeze = "select * from  trn.FreezeConfigPlanningType1 WHERE EntityId IN(" + entityid + ") ";

            DateTime freezedate = System.DateTime.Now.AddYears(-100);
            DataTable dtFreeze = _sqlRepository.GetDataTable(sqlFreeze);
            if (dtFreeze.Rows.Count > 0)
                freezedate = Convert.ToDateTime(dtFreeze.Rows[0]["FreezeDate"].ToString());


            List<GroupData> groupData = new List<GroupData>();
            for (int i = 0; i < _dtWC.Rows.Count; i++)
            {
                groupData.Add(new GroupData
                {
                    id = _dtWC.Rows[i]["Id"].ToString(),
                    text = _dtWC.Rows[i]["UserName"].ToString()

                });
            }


            string sqlWORKDAYDATA = @"SELECT distinct FORMAT(ppc.WorkingDate, 'dddd') AS WorkingDays
                                    FROM ProductionPlanningCalendar AS ppc WHERE ISNULL(ppc.WorkingHours,0)>0 AND ppc.EntityID IN(" + entityid + ")";

            DataTable dtWorkDays = _sqlRepository.GetDataTable(sqlWORKDAYDATA);
            List<string> days = new List<string>();
            for (int i = 0; i < dtWorkDays.Rows.Count; i++)
            {
                days.Add(dtWorkDays.Rows[i]["WorkingDays"].ToString());
            }
            var jsondata = Json(new { FREEZEDATE = freezedate.ToString("dd-MMM-yyyy"), DATA = _sqlRepository.GetDataCollection(sql), WORKDAYDATA = days, GROUPDATA = groupData }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost, Authorize]
        public JsonResult GetNewScheduleDataFiltered(string entityid, string processid, int year, int month, int day, Dictionary<string, string> parameters)
        {
            //CONVERT(INT, pt.WorkCenterMasterId)
            month = month + 1;

            DateTime startDate = new DateTime(year, month, day);
            DateTime endDate = new DateTime(year, month, day).AddDays(maxDaysToShow);


            string sql = @"SELECT CASE WHEN ISNULL(FIL.ProductionOrderId,0)=0 THEN 0 ELSE 1 END AS FilterData, K.seq, K.Id, K.Entity, K.WorkCenterMasterId, K.WorkCenter,
       K.ProductionOrderID, K.Quantity, K.Color, K.ProductionDate, K.isBuildUp,
       K.isStyleChange, K.planningStatus, K.[Description], K.[Subject],
       K.StartTime,CASE WHEN K.seq>1 THEN FORMAT( CONVERT(DATE,K.ProductionDate),'MMM dd yyyy hh:mm:ss tt') ELSE  K.EndTime END AS EndTime,
        CASE WHEN K.seq>1 THEN CONVERT(BIT,0) ELSE CONVERT(BIT,1) END AS AllDay, K.Recurrence, K.AppTaskId, K.ParentId,
       K.FailedToCommitmentDate FROM (SELECT 1 AS FilterData,  DENSE_RANK() OVER (PARTITION BY 
                                                         PT.WorkCenterMasterId,
                                                         PT.ProductionDate ORDER BY PT.ID) AS seq, pt.Id,e.UserName AS Entity,pt.WorkCenterMasterId,wc.UserName AS WorkCenter,pt.ProductionOrderID,pt.Quantity,t1.Color,
                            FORMAT(pt.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,PT.isBuildUp,PT.isStyleChange,
                      CASE WHEN ISNULL(FC.Id,'')='' THEN upper(ps.UserName) ELSE 'FREEZE' END AS planningStatus,WC.EntityID,pt.ProcessID,
                            --CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Description],
                            --CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Subject],
                            --pt.ProductionOrderID AS [Description],
                            --pt.ProductionOrderID AS [Subject],
                            ' ' AS [Description],
                            ' ' AS [Subject],
                            FORMAT(pt.ProductionDate,'MMM dd yyyy hh:mm:ss tt') AS StartTime,
                            FORMAT(pt.ProductionDate,'MMM dd yyyy')+' 11:59:59 PM' AS EndTime,
                             CONVERT(BIT,0) AS AllDay,CONVERT(BIT,0) AS Recurrence, pt.Id AS AppTaskId,pt.ID AS ParentId, 
                            convert(bit,case when fail.LastPlanDate> t1.CommitmentDate then 1 else 0 end) as FailedToCommitmentDate
--'UTC +06:00' AS [EndTimeZone],'UTC +06:00' AS [StartTimeZone]
                            --Mon Jun 22 2019 23:59:00
                              FROM ProductionPlanningType1 AS PT
                            LEFT OUTER JOIN [TRN].[ProductionOrder] P ON p.Id=pt.ProductionOrderID
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T1 ON t1.ProductionOrderID=pt.ProductionOrderID
                            LEFT OUTER JOIN [SCS].[WorkCenterMaster] WC ON wc.Id=pt.WorkCenterMasterId
                            LEFT OUTER JOIN [MST].[MaterialMaster] MM ON mm.Id=pt.MaterialMasterId
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=P.ProductionStatusId
                            LEFT OUTER JOIN trn.FreezeConfigPlanningType1 AS FC ON fc.EntityId=pt.EntityID AND fc.FreezeDate 
											BETWEEN (SELECT MIN(ProductionDate) FROM ProductionPlanningType1 WHERE ProductionOrderID=pt.ProductionOrderID)
											AND (SELECT MAX(ProductionDate) FROM ProductionPlanningType1 WHERE ProductionOrderID=pt.ProductionOrderID)
                            
                            LEFT OUTER JOIN (select ProductionOrderID,Max(ProductionDate) AS LastPlanDate  from ProductionPlanningType1 group by productionOrderID) 
							AS FAIL on fail.ProductionOrderID=pt.ProductionOrderID
                            INNER JOIN org.Entity AS e ON e.Id=p.EntityId
                            ) AS K 
                            
left outer join (select distinct po.Id AS ProductionOrderId,p1.WorkCenterMasterId
from trn.ProductionOrder PO
				inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
				INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID and ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
	
				left outer join scs.WorkCenterMaster WC on wc.id=p1.WorkCenterMasterId
			
				INNER JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=PO.Id
				LEFT OUTER JOIN trn.SalesOrder SO ON so.Id=pod.SalesOrderId
				left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
				left outer join mst.MaterialMaster mm on mm.id=p1.MaterialMasterId
				--LEFT OUTER JOIN MST.MaterialMasterArticle MMR ON mmr.Id=moi.ArticleId
 
				left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
				left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
				left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

				left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
				left outer join [HKP].Buyer B on B.Id=MO.BuyerId
				left outer join [HKP].[Party] p on P.Id=MO.PartyId

				left outer join org.Entity E on e.Id=p1.EntityID
				LEFT OUTER JOIN org.Unit AS u ON u.Id=e.UnitId
				left outer join org.Plant PLN on pln.Id=PO.PlantId
		

WHERE  
isnull(wc.Id,'') IN(" + parameters["WorkCenterId"] + @") AND 
isnull(PO.Id,'') IN(" + parameters["ProductOrderId"] + @") AND 
isnull(MO.Id,'') IN(" + parameters["MasterOrderNo"] + @") AND 
isnull(MO.BuyerReferenceNo,'') IN(" + parameters["BuyerOrderNo"] + @") AND 
isnull(moi.BuyerReferenceNo,'') IN(" + parameters["BuyerItemNo"] + @") AND 

isnull(PM.Id,'') IN(" + parameters["ProductMasterId"] + @") AND 
isnull(PC.Id,'') IN (" + parameters["ProductCategoryId"] + @") AND
isnull(p1.MaterialMasterId,'') IN(" + parameters["MaterialMasterId"] + @") AND 
isnull(moi.ArticleId,'') IN (" + parameters["ArticleId"] + @") AND
isnull(b.Id,'') IN(" + parameters["BuyerId"] + @") AND 
isnull(p.Id,'') IN (" + parameters["CustomerId"] + @") AND 
isnull(WC.AccountHolder,'') IN (" + parameters["AccountHolderId"] + @") AND 
isnull(WC.AccountInCharge,'') IN (" + parameters["AccountInchargeId"] + @") AND
isnull(po.ProductionStatusId,'') IN (" + parameters["ProductionStatusId"] + @")


            ) AS FIL ON FIL.ProductionOrderId=K.ProductionOrderID AND FIL.WorkCenterMasterId=K.WorkCenterMasterId



                            LEFT OUTER JOIN (select ProductionOrderID,Max(ProductionDate) AS LastPlanDate  from ProductionPlanningType1 group by productionOrderID) 
							AS FAIL on fail.ProductionOrderID=K.ProductionOrderID
                            INNER JOIN org.Entity AS e ON e.Id=K.EntityId
                            WHERE CONVERT(DATE,K.ProductionDate) between '" + startDate.ToString("dd-MMM-yyyy") + @"' and '" + endDate.ToString("dd-MMM-yyyy") + @"' AND K.EntityID IN(" + entityid + @") AND K.processid='" + processid + @"'
                            ORDER BY e.UserName,K.WorkCenterMasterId,K.ProductionDate ";



            string sqlWC = @"  SELECT wc.Id,
                            CASE WHEN ISNULL(E.StartDate,'')='' THEN wc.UserName+' (Missing Start Date)'  ELSE wc.UserName + ' ('+isnull(ei.EmployeeName, '')+')'  END AS UserName
                              FROM [SCS].[WorkCenterMaster] WC 
                            LEFT OUTER JOIN (SELECT WorkCenterMasterId,MAX(StartDate) AS StartDate FROM [SCS].[WorkCenterMasterEffectiveDate] GROUP BY WorkCenterMasterId) E ON e.WorkCenterMasterId=wc.Id
                            LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=wc.ResponsiblePersonId WHERE WC.EntityID IN('" + entityid + @") AND WC.ProcessId='" + processid + "' AND WC.[Active]=1 ORDER BY WC.UserName";
            DataTable _dtWC = _sqlRepository.GetDataTable(sqlWC);


            string sqlFreeze = "select * from  trn.FreezeConfigPlanningType1 WHERE EntityId IN('" + entityid + @")";

            DateTime freezedate = System.DateTime.Now.AddYears(-100);
            DataTable dtFreeze = _sqlRepository.GetDataTable(sqlFreeze);
            if (dtFreeze.Rows.Count > 0)
                freezedate = Convert.ToDateTime(dtFreeze.Rows[0]["FreezeDate"].ToString());


            List<GroupData> groupData = new List<GroupData>();
            for (int i = 0; i < _dtWC.Rows.Count; i++)
            {
                groupData.Add(new GroupData
                {
                    id = _dtWC.Rows[i]["Id"].ToString(),
                    text = _dtWC.Rows[i]["UserName"].ToString()

                });
            }


            string sqlWORKDAYDATA = @"SELECT distinct FORMAT(ppc.WorkingDate, 'dddd') AS WorkingDays
                                    FROM ProductionPlanningCalendar AS ppc WHERE ISNULL(ppc.WorkingHours,0)>0 AND ppc.EntityID IN('" + entityid + @")";

            DataTable dtWorkDays = _sqlRepository.GetDataTable(sqlWORKDAYDATA);
            List<string> days = new List<string>();
            for (int i = 0; i < dtWorkDays.Rows.Count; i++)
            {
                days.Add(dtWorkDays.Rows[i]["WorkingDays"].ToString());
            }

            var jsondata = Json(new { FREEZEDATE = freezedate.ToString("dd-MMM-yyyy"), DATA = _sqlRepository.GetDataCollection(sql), WORKDAYDATA = days, GROUPDATA = groupData }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [HttpPost, Authorize]
        public JsonResult GetNewProductionPlanningData(string planrowid, string ProductionOrderId, string processid)
        {
            string SelectedProductionOrder = ProductionOrderId;
            //CONVERT(INT, pt.WorkCenterMasterId)
            if (string.IsNullOrEmpty(planrowid) == false)
            {
                DataTable dt = _sqlRepository.GetDataTable("select top 1 ProductionOrderID from ProductionPlanningType1  where id = '" + planrowid + @"'");
                if (dt.Rows.Count > 0)
                    ProductionOrderId = dt.Rows[0]["ProductionOrderID"].ToString();
            }

            string sqlWCDATA = @"SELECT  WC.Sequence, t1.ProductionOrderID, t1.WorkCenterMasterId, wc.UserName AS WorkCenter,e.username as Entity,
							FORMAT(po.Lsd,'dd-MMM-yyyy') AS LSD,
                            FORMAT(po.CommitmentDate,'dd-MMM-yyyy') AS CommitmentDate,
							DATEDIFF(DAY,po.LSD,MIN(t1.ProductionDate)) AS DIFF,
							case when DATEDIFF(DAY,po.CommitmentDate,MAX(t1.ProductionDate))>0 THEN  DATEDIFF(DAY,po.CommitmentDate,MAX(t1.ProductionDate)) ELSE NULL END AS DelayedProductionDaysOnCommitmentDate,
                            FORMAT(MIN(t1.ProductionDate),'dd-MMM-yyyy') AS FromDate,
                            FORMAT(MAX(t1.ProductionDate),'dd-MMM-yyyy') AS ToDate,
                            SUM(t1.Quantity) AS PlannedQuantity

                            FROM ProductionPlanningType1 T1
							LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS po ON po.ProductionOrderID = t1.ProductionOrderID
                            LEFT OUTER JOIN  [SCS].[WorkCenterMaster] WC ON t1.WorkCenterMasterId=wc.Id
                            left join org.entity e on e.id=wc.entityid
                            WHERE t1.ProductionOrderID='" + ProductionOrderId + @"' --AND ProductionDate>=GETDATE()
                            GROUP BY  WC.Sequence,e.username,t1.ProductionOrderID, po.Lsd,po.CommitmentDate, t1.WorkCenterMasterId, wc.UserName
                            order by WC.Sequence
                            ";


            string sqlPRODDATA = @"SELECT  WC.Sequence,t1.ProductionOrderID,t1.WorkCenterMasterId, wc.UserName AS WorkCenter,e.username as Entity,
                            FORMAT(MIN(t1.ProductionDate),'dd-MMM-yyyy') AS FromDate,
                            FORMAT(MAX(t1.ProductionDate),'dd-MMM-yyyy') AS ToDate,
                            SUM(t1.Quantity) AS ProductionQuantity

                            FROM 
                            trn.ProductionSummary T1
                            LEFT OUTER JOIN  [SCS].[WorkCenterMaster] WC ON t1.WorkCenterMasterId=wc.Id
                            left join org.entity e on e.id=wc.entityid
                            WHERE t1.ProductionOrderID='" + ProductionOrderId + @"' AND WC.ProcessId='" + processid + @"' 
                            GROUP BY e.username, WC.Sequence,t1.ProductionOrderID,t1.WorkCenterMasterId, wc.UserName
                            order by WC.Sequence
                            ";

            string sqlRowData = @"select WC.Sequence,T1.Id, mm.UserName AS Material,t1.WorkCenterMasterId,T1.ProductionOrderId,WC.entityid,wc.UserName AS WorkCenter,
                                PM.UserName AS ProductName,PD.ProductMasterId,T1.Quantity,T1.ProductionHours,e.username as Entity,
                                FORMAT(t1.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,SO.Qty AS TotalQuantity,mm.[Image] AS MaterialImage
                            FROM ProductionPlanningType1 T1
                            LEFT OUTER JOIN  [SCS].[WorkCenterMaster] WC ON t1.WorkCenterMasterId=wc.Id
                            left join org.entity e on e.id=wc.entityid
                            LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=t1.MaterialMasterId
                            LEFT OUTER JOIN [TRN].[ProductDefinition] PD ON mm.Id=pd.MaterialMasterId
                            LEFT OUTER JOIN [MST].[ProductMaster] PM on pm.ID=PD.ProductMasterId
                            LEFT OUTER  JOIN (SELECT pod.ProductionOrderId,SUM(so.Qty) AS Qty
                                                FROM trn.SalesOrder AS so
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id GROUP BY pod.ProductionOrderId) 
                            AS SO ON so.ProductionOrderId=T1.ProductionOrderId
                            WHERE t1.ID='" + planrowid + @"' 
                            order by WC.Sequence";



            string sqlPRDATA = @"select T1.*,upper(ps.username) AS PlanningStatus,PO.PicFileName from ProductionOrderSchedulingParametersType1 T1
                                inner join trn.productionorder po on po.id=t1.productionorderID
                                 LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                            where ProductionOrderID = '" + ProductionOrderId + @"'";

            string sqlSTYLEDATA = @"SELECT distinct moi.BuyerReferenceNo
                                      FROM trn.ProductionOrderDetail AS pod
                                    LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                    WHERE isnull(moi.BuyerReferenceNo,'')<>''
                                    AND pod.ProductionOrderId='" + ProductionOrderId + @"'";

            Library.Planning.PlanningType1.PlanningType1Scheduler scheduler = new Library.Planning.PlanningType1.PlanningType1Scheduler();
            string SameDayPlanningData = scheduler.GetSameDayPlanningSummary(planrowid, SelectedProductionOrder);

            return Json(new
            {
                WCDATA = _sqlRepository.GetDataCollection(sqlWCDATA),
                WPRODDATA = _sqlRepository.GetDataCollection(sqlPRODDATA),
                WSTYLEDATA = _sqlRepository.GetDataCollection(sqlSTYLEDATA),
                SAMEDAYDATA = _sqlRepository.GetDataCollection(SameDayPlanningData),

                ROWDATA = _sqlRepository.GetDataCollection(sqlRowData),
                PRDATA = _sqlRepository.GetDataCollection(sqlPRDATA)
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public JsonResult GetProductionPlanningData(string planrowid, string ProductionOrderId, string processid)
        {
            string SelectedProductionOrder = ProductionOrderId;
            //CONVERT(INT, pt.WorkCenterMasterId)
            if (string.IsNullOrEmpty(planrowid) == false)
            {
                DataTable dt = _sqlRepository.GetDataTable("select top 1 ProductionOrderID from ProductionPlanningType1  where id = '" + planrowid + @"'");
                if (dt.Rows.Count > 0)
                    ProductionOrderId = dt.Rows[0]["ProductionOrderID"].ToString();
            }

            string sqlWCDATA = @"SELECT  WC.Sequence, t1.ProductionOrderID, t1.WorkCenterMasterId, wc.UserName AS WorkCenter,e.username as Entity,
							FORMAT(po.Lsd,'dd-MMM-yyyy') AS LSD,
                            FORMAT(po.CommitmentDate,'dd-MMM-yyyy') AS CommitmentDate,
							DATEDIFF(DAY,po.LSD,MIN(t1.ProductionDate)) AS DIFF,
							case when DATEDIFF(DAY,po.CommitmentDate,MAX(t1.ProductionDate))>0 THEN  DATEDIFF(DAY,po.CommitmentDate,MAX(t1.ProductionDate)) ELSE NULL END AS DelayedProductionDaysOnCommitmentDate,
                            FORMAT(MIN(t1.ProductionDate),'dd-MMM-yyyy') AS FromDate,
                            FORMAT(MAX(t1.ProductionDate),'dd-MMM-yyyy') AS ToDate,
                            SUM(t1.Quantity) AS PlannedQuantity

                            FROM ProductionPlanningType1 T1
							LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS po ON po.ProductionOrderID = t1.ProductionOrderID
                            LEFT OUTER JOIN  [SCS].[WorkCenterMaster] WC ON t1.WorkCenterMasterId=wc.Id
                            left join org.entity e on e.id=wc.entityid
                            WHERE t1.ProductionOrderID='" + ProductionOrderId + @"' --AND ProductionDate>=GETDATE()
                            GROUP BY  WC.Sequence,e.username,t1.ProductionOrderID, po.Lsd,po.CommitmentDate, t1.WorkCenterMasterId, wc.UserName
                            order by WC.Sequence
                            ";


            string sqlPRODDATA = @"SELECT  WC.Sequence,t1.ProductionOrderID,t1.WorkCenterMasterId, wc.UserName AS WorkCenter,e.username as Entity,
                            FORMAT(MIN(t1.ProductionDate),'dd-MMM-yyyy') AS FromDate,
                            FORMAT(MAX(t1.ProductionDate),'dd-MMM-yyyy') AS ToDate,
                            SUM(t1.Quantity) AS ProductionQuantity

                            FROM 
                            trn.ProductionSummary T1
                            LEFT OUTER JOIN  [SCS].[WorkCenterMaster] WC ON t1.WorkCenterMasterId=wc.Id
                            left join org.entity e on e.id=wc.entityid
                            WHERE t1.ProductionOrderID='" + ProductionOrderId + @"' AND WC.ProcessId='" + processid + @"' 
                            GROUP BY e.username, WC.Sequence,t1.ProductionOrderID,t1.WorkCenterMasterId, wc.UserName
                            order by WC.Sequence
                            ";

            string sqlRowData = @"select WC.Sequence,T1.Id, mm.UserName AS Material,t1.WorkCenterMasterId,T1.ProductionOrderId,WC.entityid,wc.UserName AS WorkCenter,
                                PM.UserName AS ProductName,PD.ProductMasterId,T1.Quantity,T1.ProductionHours,e.username as Entity,
                                FORMAT(t1.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,SO.Qty AS TotalQuantity,mm.[Image] AS MaterialImage
                            FROM ProductionPlanningType1 T1
                            LEFT OUTER JOIN  [SCS].[WorkCenterMaster] WC ON t1.WorkCenterMasterId=wc.Id
                            left join org.entity e on e.id=wc.entityid
                            LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=t1.MaterialMasterId
                            LEFT OUTER JOIN [TRN].[ProductDefinition] PD ON mm.Id=pd.MaterialMasterId
                            LEFT OUTER JOIN [MST].[ProductMaster] PM on pm.ID=PD.ProductMasterId
                            LEFT OUTER  JOIN (SELECT pod.ProductionOrderId,SUM(so.Qty) AS Qty
                                                FROM trn.SalesOrder AS so
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id GROUP BY pod.ProductionOrderId) 
                            AS SO ON so.ProductionOrderId=T1.ProductionOrderId
                            WHERE t1.ID='" + planrowid + @"' 
                            order by WC.Sequence";



            string sqlPRDATA = @"select T1.*,upper(ps.username) AS PlanningStatus,PO.PicFileName from ProductionOrderSchedulingParametersType1 T1
                                inner join trn.productionorder po on po.id=t1.productionorderID
                                 LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                            where ProductionOrderID = '" + ProductionOrderId + @"'";

            string sqlSTYLEDATA = @"SELECT distinct moi.BuyerReferenceNo
                                      FROM trn.ProductionOrderDetail AS pod
                                    LEFT OUTER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                    LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                    WHERE isnull(moi.BuyerReferenceNo,'')<>''
                                    AND pod.ProductionOrderId='" + ProductionOrderId + @"'";

            Library.Planning.PlanningType1.PlanningType1Scheduler scheduler = new Library.Planning.PlanningType1.PlanningType1Scheduler();
            string SameDayPlanningData = scheduler.GetSameDayPlanningSummary(planrowid, SelectedProductionOrder);

            return Json(new
            {
                WCDATA = _sqlRepository.GetDataCollection(sqlWCDATA),
                WPRODDATA = _sqlRepository.GetDataCollection(sqlPRODDATA),
                WSTYLEDATA = _sqlRepository.GetDataCollection(sqlSTYLEDATA),
                SAMEDAYDATA = _sqlRepository.GetDataCollection(SameDayPlanningData),

                ROWDATA = _sqlRepository.GetDataCollection(sqlRowData),
                PRDATA = _sqlRepository.GetDataCollection(sqlPRDATA)
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetStyleData(string styleName, string entityid)
        {


            string sql = @"SELECT format(ppt.ProductionDate,'dd/MMM') AS PlanDate,SUM(ppt.Quantity) AS PlanQty ,
                            isnull(SUM(prod.ProductionQty),0)AS ProductionQty,
                            CASE WHEN isnull(SUM(prod.ProductionQty),0)>0 THEN isnull(SUM(prod.ProductionQty),0)-SUM(ppt.Quantity) ELSE 0 END AS  Variance 
				FROM ProductionPlanningSnapshot2Type1 AS ppt
				   LEFT OUTER JOIN (SELECT ppt.EntityId,  ppt.ProductionOrderID,ppt.ProductionDate,ppt.ProcessId,SUM(ppt.Quantity) AS ProductionQty
                                                   FROM trn.ProductionSummary AS ppt
                                                 GROUP BY  ppt.EntityId, ppt.ProcessId,ppt.ProductionOrderID,ppt.ProductionDate) AS PROD ON 
                                                 ppt.ProductionOrderID=prod.ProductionOrderID
                                                 AND ppt.ProductionDate=prod.ProductionDate
                                                 AND ppt.ProcessID=prod.ProcessId
                                                 AND ppt.EntityID=prod.EntityId
                                WHERE ppt.ProductionOrderID IN (
                                SELECT pod.ProductionOrderId FROM trn.ProductionOrderDetail AS pod
                                INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                WHERE moi.BuyerReferenceNo='" + styleName + "' AND ppt.EntityID='" + entityid + @"'

                                )
                             group by ppt.ProductionDate
                           ";



            return Json(new
            {
                STYLEGRAPH = _sqlRepository.GetDataCollection(sql),

            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getProductMasterParametersDisplay(string productionOrderID, string entityid)
        {


            string sql = @"SELECT pm.Id,pm.UserName AS ProductName,pc.UserName AS ProductCategory,puc.UserName AS ProductSubCategory, 
                            pme.NoOfWorkStation, pme.EfficencyPercentage AS Efficiency,pme.StandardWorkingHours PlanWorkingHoursPerDay, pme.SPT,
                            MLD.[Value] AS MinimumLineDays,
                            PM.FirstdayOutPut AS FirstDayOutPut,PM.IncrementValue,PM.DaysToReachTheTarget AS DayToReachTheTarget,
                                CASE WHEN ISNULL(PD.IsFixed,'')='FIXED' THEN 'FIXED' ELSE 'PERCENTAGE' END AS IncrementType
                                    FROM [TRN].[ProductDefinition] PD
                                LEFT OUTER JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                                LEFT OUTER JOIN [HKP].[ProductCategory] PC ON pc.Id=pm.ProductCategoryId
                                    LEFT OUTER JOIN [HKP].[ProductSubCategory] PUC ON PUC.Id=pm.ProductSubCategoryId
                                LEFT OUTER JOIN [TRN].[ProductMasterEfficency] PME ON pme.ProductMasterId=pm.Id AND pme.EfficencyName='Planning'
                                LEFT OUTER JOIN dbo.EntityConfig con ON 1=1 and con.EntityId='" + entityid + @"' AND con.StandardName='" + EntityConfigParameter.StandardWorkingHoursPerDay + @"'
                                   LEFT OUTER JOIN dbo.EntityConfig MLD ON 1=1 and MLD.EntityId='" + entityid + @"' AND MLD.StandardName='" + EntityConfigParameter.MinimumLineDays + @"'
                         WHERE PD.MaterialMasterId IN (
	
                                    SELECT DISTINCT moi.MaterialMasterId FROM [TRN].[ProductionOrderDetail] D
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=d.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                    WHERE d.ProductionOrderId='" + productionOrderID + @"'
                                    )";

            string sqlProduction = @"SELECT t1.*,t2.FirstInputDate,so.Qty AS SOQuantity
  FROM [ProductionOrderSchedulingParametersType1] T1
LEFT OUTER  JOIN (SELECT pod.ProductionOrderId,SUM(so.Qty) AS Qty
                                                FROM trn.SalesOrder AS so
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id GROUP BY pod.ProductionOrderId) AS SO ON so.ProductionOrderId=T1.ProductionOrderID
LEFT OUTER JOIN (SELECT p.ProductionOrderID,FORMAT(MIN(p.ProductionDate),'dd-MMM-yyyy') AS FirstInputDate
                   FROM trn.ProductionSummary AS p GROUP BY p.ProductionOrderID) AS T2 ON t1.ProductionOrderID=t2.ProductionOrderID  where t1.ProductionOrderID='" + productionOrderID + "'";

            string sqlPRODUCTPARAMSWorkCenterList = @"SELECT wc.* FROM   [SCS].[WorkCenterMasterProductPriority] PP
                                INNER JOIN scs.WorkCenterMaster AS WC ON wc.id=pp.WorkCenterMasterId
                                WHERE pp.ProductMasterId IN (
	
                                    SELECT DISTINCT pd.ProductMasterId FROM [TRN].[ProductionOrderDetail] D
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=d.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                    INNER JOIN [TRN].[ProductDefinition] PD ON moi.MaterialMasterId=pd.MaterialMasterId
                                    WHERE d.ProductionOrderId='" + productionOrderID + @"'
                                    ) AND WC.EntityId='" + entityid + @"'
                                ORDER BY pp.[Priority]";

            string sqlPRODUCTIONPARAMSWorkCenterList = @"SELECT ws.* FROM [TRN].[ProductionOrderWorkCenter] W
                        INNER JOIN [SCS].[WorkCenterMaster] WS ON ws.Id=w.WorkCenterMasterId WHERE W.ProductionOrderId='" + productionOrderID + "'";

            return Json(new
            {
                PRODUCTPARAMS = _sqlRepository.GetDataCollection(sql),
                PRODUCTIONPARAMS = _sqlRepository.GetDataCollection(sqlProduction),
                PRODUCTPARAMSWorkCenterList = _sqlRepository.GetDataCollection(sqlPRODUCTPARAMSWorkCenterList),
                PRODUCTIONPARAMSWorkCenterList = _sqlRepository.GetDataCollection(sqlPRODUCTIONPARAMSWorkCenterList)
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult getWorkcenterParametersDisplay(string WorkCenterMasterId)
        {


            string sql = @"SELECT  wc.Id, wc.Code, wc.UserName AS WorkCenter, wc.Capacity, wc.PlanEfficiency,
                               wc.MaxTimePerDay, wc.StandardTimePerDay, wc.PlanBudgetCapacityPerDay,
                               wc.DailyFixedCost, wc.VariableCost, wc.SPT, wc.CM, wc.NoOfWorkStation,
                               wc.MonthlyNoOfDays,uom.UserName AS UOM,c.Name AS Currency,ei.EmployeeName AS ResponsiblePerson,
                               ei2.EmployeeName AS Mentor
                          FROM [SCS].[WorkCenterMaster] WC
                        LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=wc.ResponsiblePersonId
                        LEFT OUTER JOIN EmployeeInformation AS ei2 ON ei2.SystemId=wc.MentorId
                        LEFT OUTER JOIN hkp.Process AS p ON p.Id=wc.ProcessId
                        LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=wc.UoMId
                        LEFT OUTER JOIN scs.Currency AS c ON c.Id=wc.CurrencyId
                        WHERE WC.[Active]=1 AND wc.Id='" + WorkCenterMasterId + @"'";

            string sqlWORKCENTERProductList = @"SELECT pm.Id, PM.Code,pc.UserName AS ProductCategory,psc.UserName AS ProductSubCategory,pm.UserName AS ProductName
                        FROM [SCS].[WorkCenterMasterProductPriority] PP
                                INNER JOIN mst.ProductMaster AS pm ON pm.Id=pp.ProductMasterId
                             LEFT OUTER JOIN hkp.ProductCategory AS pc ON pc.Id=pm.ProductCategoryId
                             LEFT OUTER JOIN hkp.ProductSubCategory PSC ON psc.Id=pm.ProductSubCategoryId
                      WHERE pp.WorkCenterMasterId='" + WorkCenterMasterId + @"'
                      ORDER BY PSC.Sequence";

            return Json(new
            {
                WORKCENTERPARAMS = _sqlRepository.GetDataCollection(sql),
                WORKCENTERProductList = _sqlRepository.GetDataCollection(sqlWORKCENTERProductList)
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetProductWorkCenterListDisplay(string productId, string entityId)
        {
            string sql = @"SELECT wc.* FROM   [SCS].[WorkCenterMasterProductPriority] PP
                                INNER JOIN scs.WorkCenterMaster AS WC ON wc.id=pp.WorkCenterMasterId
                                WHERE pp.ProductMasterId='" + productId + "' AND WC.EntityId='" + entityId + @"'
                                ORDER BY pp.[Priority]";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetProductionPlanGraph(string orderid, string workcentrid)
        {
            string sql = @"  SELECT FORMAT(ProductionDate,'dd/MMM') AS PlanDate,SUM(T1.Quantity) AS Quantity
				    FROM ProductionPlanningType1 T1 WHERE T1.ProductionOrderID='" + orderid + @"' 
                    AND T1.WorkCenterMasterId='" + workcentrid + @"' GROUP BY T1.ProductionDate ORDER BY T1.ProductionDate ASC";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetProductionPlanGraphPRWise(string orderid)
        {
            string sql = @"  SELECT FORMAT(ProductionDate,'dd/MM') AS PlanDate,SUM(T1.Quantity) AS Quantity
				    FROM ProductionPlanningType1 T1 WHERE T1.ProductionOrderID='" + orderid + @"'  GROUP BY T1.ProductionDate  ORDER BY T1.ProductionDate ASC";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetProductionGraph(string orderid, string workcentrid)
        {
            string sql = @"SELECT FORMAT(t1.ProductionDate,'dd/MMM') AS ProductionDate,sum(T1.Quantity) AS Quantity,isnull(trg.Quantity,0) AS TargetQty
				    FROM trn.ProductionSummary T1 
                    --LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS TRG ON trg.ProductionOrderID=t1.ProductionOrderId AND t1.WorkCenterMasterId=trg.WorkCenterMasterId AND t1.ProductionDate=trg.ProductionDate
                    LEFT OUTER JOIN trn.DailyProductionTarget AS TRG ON trg.ProductionOrderID=t1.ProductionOrderId AND t1.WorkCenterMasterId=trg.WorkCenterMasterId AND t1.ProductionDate=trg.TargetDate

                    WHERE T1.ProductionOrderID='" + orderid
                    + @"' AND T1.WorkCenterMasterId='" + workcentrid + @"'   GROUP BY t1.ProductionDate,trg.Quantity  ORDER BY T1.ProductionDate ASC";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetProductionGraphPRWise(string orderid)
        {
            string sql = @"SELECT FORMAT(t1.ProductionDate,'dd/MMM') AS ProductionDate,sum(T1.Quantity) AS Quantity,isnull(trg.Quantity,0) AS TargetQty
				    FROM trn.ProductionSummary T1 
                    LEFT OUTER JOIN (
                    	select ProductionOrderID,TargetDate AS ProductionDate,SUM(Quantity) AS Quantity 
                        from trn.DailyProductionTarget 
                    	GROUP BY ProductionOrderID,TargetDate
                    )  AS TRG ON trg.ProductionOrderID=t1.ProductionOrderId AND t1.ProductionDate=trg.ProductionDate

                    WHERE T1.ProductionOrderID='" + orderid + @"' AND t1.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID='" + orderid + @"')
                    GROUP BY t1.ProductionDate,trg.Quantity  ORDER BY T1.ProductionDate ASC";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetProductionRecipeMaterialListDisplay(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionRecipeMaterialList(productionOrderId), JsonRequestBehavior.AllowGet);
        }


        #endregion Production Plan Simulation Visualization

        #endregion Production Plan Simulation



        #region productionPlan Snapshot



        [HttpPost, Authorize]
        public JsonResult LoadFilterSQL()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT * FROM (
                                        SELECT DISTINCT 
                                        isnull(po.Id,'') ProductOrderId,isnull(mo.Id,'')AS MasterOrderNo,isnull(mo.BuyerReferenceNo,'') AS BuyerOrderNo,isnull(moi.BuyerReferenceNo,'') AS BuyerItemNo,
                                        wc.Sequence, isnull(wc.Id,'') AS WorkCenterId,isnull(wc.UserName,'') AS WorkCenter,isnull(e.Id,'') AS EntityId,isnull(e.UserName,'') Entity,
                                        isnull(pm.Id,'') ProductMasterId,isnull(pm.UserName,'') ProductMaster,isnull(pc.Id,'') ProductCategoryId,isnull(pc.UserName,'') ProductCategory,
                                        isnull(mm.Id,'') MaterialMasterId,isnull(mm.UserName ,'')MaterialMaster,isnull(MMr.Id,'') ArticleId,isnull(mmr.ShortName,'') Article,isnull(b.Id,'') BuyerId,isnull(b.UserName,'') Buyer,
                                        isnull(p.Id,'') CustomerId,isnull(p.UserName,'') Customer,
                                        isnull(acci.SystemId,'') AccountInchargeId, isnull(ACCI.EmployeeName,'') AS AccountIncharge,isnull(acch.SystemId,'') AccountHolderId, isnull(acch.EmployeeName,'') AS AccountHolder,
                                        isnull(ps.Id,'') AS ProductionStatusId, isnull(ps.UserName,'') AS ProductionStatus
                                        from trn.ProductionOrder PO
				                                inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
				                                INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID and ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
	
				                                left outer join scs.WorkCenterMaster WC on wc.id=p1.WorkCenterMasterId
				                                LEFT OUTER JOIN EmployeeInformation AS ACCI ON ACCI.SystemId=wc.AccountInCharge
				                                LEFT OUTER JOIN EmployeeInformation AS ACCH ON ACCH.SystemId=wc.AccountHolder

				                                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=PO.Id
				                                LEFT OUTER JOIN trn.SalesOrder SO ON so.Id=pod.SalesOrderId
				                                left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
				                                left outer join mst.MaterialMaster mm on mm.id=p1.MaterialMasterId
				                                LEFT OUTER JOIN MST.MaterialMasterArticle MMR ON mmr.Id=moi.ArticleId
 
				                                left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
				                                left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
				                                left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

				                                left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
				                                left outer join [HKP].Buyer B on B.Id=MO.BuyerId
				                                left outer join [HKP].[Party] p on P.Id=MO.PartyId

				                                left outer join org.Entity E on e.Id=p1.EntityID
				                                LEFT OUTER JOIN org.Unit AS u ON u.Id=e.UnitId
				                                left outer join org.Plant PLN on pln.Id=PO.PlantId
				                                LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
				
                                WHERE po.PlantId='" + identity.PlantId + @"'
                                ) AS KK";





            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }



        [HttpPost, Authorize]
        public ActionResult SaveSnapshot(ProductionPlanningSnapshotMasterType1 t1)
        {

            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select * from ProductionPlanningSnapshotMasterType1 where SnapshotName='" + t1.SnapshotName + "'";
                DataTable dt = _sqlRepository.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                    throw new Exception("Snapshot has already been taken using this name " + t1.SnapshotName);



                DataSet dsMaster, dsChild, dsSource;
                sql = @"select * from ProductionPlanningSnapshotMasterType1 where ID='" + t1.ID + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                sql = @"select * from ProductionPlanningSnapshotType1 where 1=2";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsChild, false, "1");

                sql = @"select * from ProductionPlanningType1 where EntityID='" + t1.EntityID + "' AND ProcessID='" + t1.ProcessID + "' AND ProductionDate>='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsSource, false, "1");

                bplib.clsGenID id = new bplib.clsGenID();
                string systemid = "";
                id.GenIDYearly(DateTime.Now.ToShortDateString(), "PRODUCTION PLAN SNAPSHOT", out systemid);
                systemid = systemid.Replace("-", "");
                systemid = systemid.Substring(2);

                DataRow dr = dsMaster.Tables[0].NewRow();
                dr["ID"] = systemid;
                dr["EntityID"] = t1.EntityID;
                dr["ProcessID"] = t1.ProcessID;
                dr["SnapshotDesc"] = t1.SnapshotDesc;
                dr["SnapshotName"] = t1.SnapshotName;
                dr["SnapshotDate"] = System.DateTime.Now.ToString("dd-MMM-yyyy");

                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = System.DateTime.Now.ToString();
                dr["AddedFromIP"] = identity.IPAddress;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;
                dsMaster.Tables[0].Rows.Add(dr);

                dr = null;

                for (int ROW = 0; ROW < dsSource.Tables[0].Rows.Count; ROW++)
                {
                    dr = dsChild.Tables[0].NewRow();
                    for (int COL = 0; COL < dsSource.Tables[0].Columns.Count; COL++)
                    {
                        if (dsSource.Tables[0].Columns[COL].ColumnName.ToString().ToUpper() == "ID")
                            continue;

                        dr[dsSource.Tables[0].Columns[COL].ColumnName] = bplib.clsWebLib.RetValidLen(dsSource.Tables[0].Rows[ROW][dsSource.Tables[0].Columns[COL].ColumnName].ToString());
                    }

                    dr["ProductionPlanningSnapshotMasterType1"] = systemid;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsChild.Tables[0].Rows.Add(dr);
                }


                clsStaticInfo clsStatic = new clsStaticInfo();
                clsStatic.SaveDataSets(dsMaster, dsChild);

                return Json(new { Message = "Snapshot taken successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }



        }

        [HttpPost, Authorize]
        public JsonResult LoadSnapshot(string id, string entityid, string processid)
        {
            //CONVERT(INT, pt.WorkCenterMasterId)

            string sql = @"SELECT  pt.Id,e.UserName AS Entity,pt.WorkCenterMasterId,wc.UserName AS WorkCenter,pt.ProductionOrderID,pt.Quantity,t1.Color,
                            FORMAT(pt.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,PT.isBuildUp,PT.isStyleChange,
                            upper(Ps.username) AS planningStatus,
                            --CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Description],
                            --CONCAT('Pord. Ord#',pt.ProductionOrderID,' Quantity:',CONVERT(INT,pt.Quantity),' Material:',mm.UserName) AS [Subject],
                            pt.ProductionOrderID AS [Description],
                            pt.ProductionOrderID AS [Subject],
                            FORMAT(pt.ProductionDate,'MMM dd yyyy hh:mm:ss tt') AS StartTime,
                            FORMAT(pt.ProductionDate,'MMM dd yyyy')+' 11:59:59 PM' AS EndTime,
                            'true' AS AllDay,'false' AS Recurrence, pt.Id AS AppTaskId,pt.ID AS ParentId, 
                            convert(bit,case when fail.LastPlanDate> t1.CommitmentDate then 1 else 0 end) as FailedToCommitmentDate
--'UTC +06:00' AS [EndTimeZone],'UTC +06:00' AS [StartTimeZone]
                            --Mon Jun 22 2019 23:59:00
                              FROM ProductionPlanningSnapshotType1 AS PT
                            LEFT OUTER JOIN [TRN].[ProductionOrder] P ON p.Id=pt.ProductionOrderID
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=P.ProductionStatusId
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T1 ON t1.ProductionOrderID=pt.ProductionOrderID
                            LEFT OUTER JOIN [SCS].[WorkCenterMaster] WC ON wc.Id=pt.WorkCenterMasterId
                            LEFT OUTER JOIN [MST].[MaterialMaster] MM ON mm.Id=pt.MaterialMasterId
                            LEFT OUTER JOIN (select ProductionOrderID,Max(ProductionDate) AS LastPlanDate  from ProductionPlanningSnapshotType1 group by productionOrderID) 
							AS FAIL on fail.ProductionOrderID=pt.ProductionOrderID
                            INNER JOIN org.Entity AS e ON e.Id=p.EntityId
                            WHERE  PT.ProductionPlanningSnapshotMasterType1='" + id + @"'
                            ORDER BY e.UserName,pt.WorkCenterMasterId,pt.ProductionDate ";




            string sqlWC = @" SELECT wc.Id,
                            CASE WHEN ISNULL(E.StartDate,'')='' THEN wc.UserName+' (Missing Start Date)' ELSE wc.UserName + ' ('+ei.NickName+')' END AS UserName
                              FROM [SCS].[WorkCenterMaster] WC 
                            LEFT OUTER JOIN (SELECT WorkCenterMasterId,MAX(StartDate) AS StartDate FROM [SCS].[WorkCenterMasterEffectiveDate] GROUP BY WorkCenterMasterId) E ON e.WorkCenterMasterId=wc.Id
                            LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=wc.ResponsiblePersonId WHERE WC.[Active]=1 AND  WC.EntityID='" + entityid
                + @"' AND WC.ProcessId='" + processid + "' ORDER BY WC.UserName";
            DataTable _dtWC = _sqlRepository.GetDataTable(sqlWC);


            List<GroupData> groupDatas = new List<GroupData>();
            for (int i = 0; i < _dtWC.Rows.Count; i++)
            {
                groupDatas.Add(new GroupData
                {
                    id = _dtWC.Rows[i]["Id"].ToString(),
                    text = _dtWC.Rows[i]["UserName"].ToString()

                });
            }


            string sqlWORKDAYDATA = @"SELECT distinct FORMAT(ppc.WorkingDate, 'dddd') AS WorkingDays
                                    FROM ProductionPlanningCalendar AS ppc WHERE ISNULL(ppc.WorkingHours,0)>0 AND ppc.EntityID = '" + entityid + "'";

            DataTable dtWorkDays = _sqlRepository.GetDataTable(sqlWORKDAYDATA);
            List<string> days = new List<string>();
            for (int i = 0; i < dtWorkDays.Rows.Count; i++)
            {
                days.Add(dtWorkDays.Rows[i]["WorkingDays"].ToString());
            }

            return Json(new { DATA = _sqlRepository.GetDataCollection(sql), WORKDAYDATA = days, GROUPDATA = groupDatas }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult LoadSnapshotList(string entityid, string processid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT top 200 ppsmt.ID,format(ppsmt.AddedDate,'dd-MMM-yyyy hh:mm:ss tt') AS SnapshotDate, 
                            ppsmt.SnapshotName, ppsmt.SnapshotDesc
                         FROM ProductionPlanningSnapshotMasterType1 AS ppsmt 
                where addedby='" + identity.Name + @"' AND entityid='" + entityid + "' and processid='" + processid + @"'
                        order by ppsmt.AddedDate DESC";





            return Json(new { DATA = _sqlRepository.GetDataCollection(sql) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult LoadNewSnapshotList(string entityid, string processid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT top 200 ppsmt.ID,format(ppsmt.AddedDate,'dd-MMM-yyyy hh:mm:ss tt') AS SnapshotDate, 
                            ppsmt.SnapshotName, ppsmt.SnapshotDesc
                         FROM ProductionPlanningSnapshotMasterType1 AS ppsmt 
                where addedby='" + identity.Name + @"' AND entityid='" + entityid + "' and processid='" + processid + @"'
                        order by ppsmt.AddedDate DESC";





            return Json(new { DATA = _sqlRepository.GetDataCollection(sql) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult RestoreSnapshot(string MasterId)
        {
            try
            {
                if (string.IsNullOrEmpty(MasterId))
                    throw new Exception("Please select a snapshot to restore");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"delete from ProductionPlanningType1";
                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.executeQuery(sql);
                connection.CommitTransaction();


                DataSet dsSource, dsDestination;
                connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.getDataSet("select * from ProductionPlanningType1 where 1=2", out dsDestination);
                connection.CommitTransaction();


                connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.getDataSet("SELECT * FROM ProductionPlanningSnapshotType1 AS ppst WHERE ppst.ProductionPlanningSnapshotMasterType1='" + MasterId + @"'", out dsSource);
                connection.CommitTransaction();

                for (int ROW = 0; ROW < dsSource.Tables[0].Rows.Count; ROW++)
                {
                    DataRow dr = dsDestination.Tables[0].NewRow();
                    for (int COL = 0; COL < dsSource.Tables[0].Columns.Count; COL++)
                    {
                        if (dsDestination.Tables[0].Columns.Contains(dsSource.Tables[0].Columns[COL].ColumnName))
                            dr[dsSource.Tables[0].Columns[COL].ColumnName] = dsSource.Tables[0].Rows[ROW][COL];
                    }
                    dr["Id"] = DBNull.Value;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsDestination.Tables[0].Rows.Add(dr);
                }


                clsStaticInfo info = new clsStaticInfo();



                info.SaveDataSets(dsDestination);

                return Json(new { Error = false, Message = "Restore successful" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveSnapshot2_backup()
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"delete from ProductionPlanningSnapshot2Type1 where productiondate>='" + DateTime.Now.ToString("dd-MMM-yyyy") + "'";
                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.executeQuery(sql);
                connection.CommitTransaction();


                DataSet dsSource, dsDestination;
                connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.getDataSet("select * from ProductionPlanningSnapshot2Type1 where 1=2", out dsDestination);
                connection.CommitTransaction();


                connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.getDataSet("SELECT * FROM ProductionPlanningType1 AS ppst  where productiondate>='" + DateTime.Now.ToString("dd-MMM-yyyy") + "'", out dsSource);
                connection.CommitTransaction();

                for (int ROW = 0; ROW < dsSource.Tables[0].Rows.Count; ROW++)
                {
                    DataRow dr = dsDestination.Tables[0].NewRow();
                    for (int COL = 0; COL < dsSource.Tables[0].Columns.Count; COL++)
                    {
                        if (dsDestination.Tables[0].Columns.Contains(dsSource.Tables[0].Columns[COL].ColumnName))
                            dr[dsSource.Tables[0].Columns[COL].ColumnName] = dsSource.Tables[0].Rows[ROW][COL];
                    }
                    dr["Id"] = DBNull.Value;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsDestination.Tables[0].Rows.Add(dr);
                }


                clsStaticInfo info = new clsStaticInfo();



                info.SaveDataSets(dsDestination);

                return Json(new { Error = false, Message = "Snapshot-2 has been saved successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public JsonResult SaveSnapshot2(ProductionPlanningSnapshot2MasterType1 t1)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"delete from ProductionPlanningSnapshot2Type1 where productiondate>='" + DateTime.Now.ToString("dd-MMM-yyyy") + "'";
                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.executeQuery(sql);
                connection.CommitTransaction();


                DataSet dsMaster, dsSource, dsDestination;

                connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.getDataSet(@"select * from ProductionPlanningSnapshot2MasterType1 where 1=2", out dsMaster);
                connection.CommitTransaction();

                connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.getDataSet("select * from ProductionPlanningSnapshot2Type1 where 1=2", out dsDestination);
                connection.CommitTransaction();


                connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.getDataSet("SELECT * FROM ProductionPlanningType1 AS ppst  where productiondate>='" + DateTime.Now.ToString("dd-MMM-yyyy") + "'", out dsSource);
                connection.CommitTransaction();

                #region
                bplib.clsGenID id = new bplib.clsGenID();
                string systemid = "";
                id.GenIDYearly(DateTime.Now.ToShortDateString(), "PRODUCTION PLAN SNAPSHOT", out systemid);
                systemid = systemid.Replace("-", "");
                systemid = systemid.Substring(2);

                DataRow dr = dsMaster.Tables[0].NewRow();
                dr["ID"] = systemid;
                dr["EntityID"] = t1.EntityID;
                dr["ProcessID"] = t1.ProcessID;
                dr["SnapshotDesc"] = t1.SnapshotDesc;
                dr["SnapshotName"] = t1.SnapshotName;
                dr["SnapshotDate"] = System.DateTime.Now.ToString("dd-MMM-yyyy");
                dr["SnapshotTakenBy"] = t1.SnapshotTakenBy;

                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = DateTime.Now.ToString();
                dr["AddedFromIP"] = identity.IPAddress;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;
                dsMaster.Tables[0].Rows.Add(dr);
                #endregion
                dr = null;

                for (int ROW = 0; ROW < dsSource.Tables[0].Rows.Count; ROW++)
                {
                    dr = dsDestination.Tables[0].NewRow();
                    for (int COL = 0; COL < dsSource.Tables[0].Columns.Count; COL++)
                    {
                        if (dsDestination.Tables[0].Columns.Contains(dsSource.Tables[0].Columns[COL].ColumnName))
                            dr[dsSource.Tables[0].Columns[COL].ColumnName] = dsSource.Tables[0].Rows[ROW][COL];
                    }
                    dr["Id"] = DBNull.Value;
                    dr["ProductionPlanningSnapshot2MasterType1Id"] = systemid;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsDestination.Tables[0].Rows.Add(dr);
                }


                clsStaticInfo info = new clsStaticInfo();



                info.SaveDataSets(dsMaster, dsDestination);

                return Json(new { Error = false, Message = "Snapshot-2 has been saved successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }




        #endregion productionPlan Snapshot


        #region OS2
        [HttpGet, Authorize]
        public ActionResult OS2xls(string entityid, string processid)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;

            try
            {
                DataTable dt;
                getOS2(out dt);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[1].Name = "OS2";
                sheet = workbook.Worksheets[0];


                int ROW = 1; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Work Center";
                sheet[ROW, COL].ColumnWidth = 8;
                int colWorkCenter = COL;
                COL++;
                sheet[ROW, COL].Text = "Target Date";
                sheet[ROW, COL].ColumnWidth = 8;
                int colTargetDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order ID";
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductionOrderID = COL;
                COL++;
                sheet[ROW, COL].Text = "Planned Qty For The Day";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colPlannedQtyForTheDay = COL;
                COL++;
                sheet[ROW, COL].Text = "SPT";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colSPT = COL;

                COL++;
                sheet[ROW, COL].Text = "Product Category";
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Raw Material Inhouse Date";
                sheet[ROW, COL].ColumnWidth = 8;
                int colMainRawMaterialInhouseDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Other Raw Material Inhouse Date";
                sheet[ROW, COL].ColumnWidth = 8;
                int colOtherRawMaterialInhouseDate = COL;


                COL++;
                sheet[ROW, COL].Text = "Production Priority";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductionPriority = COL;
                COL++;
                sheet[ROW, COL].Text = "buyer";
                sheet[ROW, COL].ColumnWidth = 8;
                int colbuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "Line Item";
                sheet[ROW, COL].ColumnWidth = 8;
                int colStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Style Group";
                sheet[ROW, COL].ColumnWidth = 8;
                int colStyleGroup = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 8;
                int colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Status";
                sheet[ROW, COL].ColumnWidth = 8;
                int colOrderStatus = COL;
                COL++;



                sheet[ROW, COL].Text = "FOB";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colFOB = COL;
                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colCM = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Qty";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colOrderQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Order Qty";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colPlanOrderQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Unit";
                sheet[ROW, COL].ColumnWidth = 8;
                int colUnit = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 8;
                int colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Account Incharge";
                sheet[ROW, COL].ColumnWidth = 8;
                int colAccountIncharge = COL;
                COL++;
                sheet[ROW, COL].Text = "Account Holder";
                sheet[ROW, COL].ColumnWidth = 8;
                int colAccountHolder = COL;
                COL++;
                sheet[ROW, COL].Text = "Commitment Date";
                sheet[ROW, COL].ColumnWidth = 8;
                int colCommitmentDate = COL;
                COL++;
                sheet[ROW, COL].Text = "First Delivery Date";
                sheet[ROW, COL].ColumnWidth = 8;
                int colFirstDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Last Delivery Date";
                sheet[ROW, COL].ColumnWidth = 8;
                int colLastDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "ProductionC ompletion Date";
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductionCompletionDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Base Process Completion Date";
                sheet[ROW, COL].ColumnWidth = 8;
                int colBaseProcessCompletionDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Line Start Date AtWC";
                sheet[ROW, COL].ColumnWidth = 8;
                int colLineStartDateAtWC = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Start Date AtPR";
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductionStartDateAtPR = COL;
                COL++;
                sheet[ROW, COL].Text = "Last Planning Date WC";
                sheet[ROW, COL].ColumnWidth = 8;
                int colLastPlanningDateWC = COL;
                COL++;
                sheet[ROW, COL].Text = "Last Planning DateP R";
                sheet[ROW, COL].ColumnWidth = 8;
                int colLastPlanningDatePR = COL;

                COL++;
                sheet[ROW, COL].Text = "Workcenter Target Per Day";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colLineTargetPerDay = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Qty (WC)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductionQtyAtWC = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Qty (PR)";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductionQtyAtPR = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Status";
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductionStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "No Of Work Station";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colNoOfWorkStation = COL;
                COL++;
                sheet[ROW, COL].Text = "LSD";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colLSD = COL;
                COL++;
                sheet[ROW, COL].Text = "Working Hours";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colWorkingHours = COL;
                COL++;
                sheet[ROW, COL].Text = "Booked CM";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colBookedCM = COL;


                #endregion columns

                int endCol = COL;

                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(150, 250, 150);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //text fields
                    sheet[ROW, colAccountHolder].Text = dt.Rows[i]["AccountHolder"].ToString();
                    sheet[ROW, colAccountIncharge].Text = dt.Rows[i]["AccountIncharge"].ToString();
                    sheet[ROW, colbuyer].Text = dt.Rows[i]["buyer"].ToString();
                    sheet[ROW, colEntity].Text = dt.Rows[i]["Entity"].ToString();
                    sheet[ROW, colMasterOrderNo].Text = dt.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, colOrderStatus].Text = dt.Rows[i]["OrderStatus"].ToString();
                    sheet[ROW, colProductCategory].Text = dt.Rows[i]["ProductCategory"].ToString();
                    sheet[ROW, colProductionOrderID].Text = dt.Rows[i]["ProductionOrderID"].ToString();
                    sheet[ROW, colProductionStatus].Text = dt.Rows[i]["ProductionStatus"].ToString();
                    sheet[ROW, colStyleGroup].Text = dt.Rows[i]["StyleGroup"].ToString();
                    sheet[ROW, colStyleNo].Text = dt.Rows[i]["StyleNo"].ToString();
                    sheet[ROW, colUnit].Text = dt.Rows[i]["Unit"].ToString();
                    sheet[ROW, colWorkCenter].Text = dt.Rows[i]["WorkCenter"].ToString();

                    //number fields
                    sheet[ROW, colBookedCM].Number = clsStaticInfo.dbl(dt.Rows[i]["BookedCM"].ToString());
                    sheet[ROW, colCM].Number = clsStaticInfo.dbl(dt.Rows[i]["CM"].ToString());
                    sheet[ROW, colFOB].Number = clsStaticInfo.dbl(dt.Rows[i]["FOB"].ToString());
                    sheet[ROW, colLineTargetPerDay].Number = clsStaticInfo.dbl(dt.Rows[i]["LineTargetPerDay"].ToString());
                    sheet[ROW, colNoOfWorkStation].Number = clsStaticInfo.dbl(dt.Rows[i]["NoOfWorkStation"].ToString());
                    sheet[ROW, colOrderQty].Number = clsStaticInfo.dbl(dt.Rows[i]["OrderQty"].ToString());
                    sheet[ROW, colPlannedQtyForTheDay].Number = clsStaticInfo.dbl(dt.Rows[i]["PlannedQtyForTheDay"].ToString());
                    sheet[ROW, colPlanOrderQty].Number = clsStaticInfo.dbl(dt.Rows[i]["PlanOrderQty"].ToString());
                    sheet[ROW, colProductionPriority].Number = clsStaticInfo.dbl(dt.Rows[i]["ProductionPriority"].ToString());
                    sheet[ROW, colProductionQtyAtPR].Number = clsStaticInfo.dbl(dt.Rows[i]["ProductionQtyAtPR"].ToString());
                    sheet[ROW, colProductionQtyAtWC].Number = clsStaticInfo.dbl(dt.Rows[i]["ProductionQtyAtWC"].ToString());
                    sheet[ROW, colSPT].Number = clsStaticInfo.dbl(dt.Rows[i]["SPT"].ToString());
                    sheet[ROW, colWorkingHours].Number = clsStaticInfo.dbl(dt.Rows[i]["WorkingHours"].ToString());


                    //date fields
                    sheet[ROW, colBaseProcessCompletionDate].Text = GetDate(dt.Rows[i]["BaseProcessCompletionDate"].ToString());
                    sheet[ROW, colCommitmentDate].Text = GetDate(dt.Rows[i]["CommitmentDate"].ToString());
                    sheet[ROW, colFirstDeliveryDate].Text = GetDate(dt.Rows[i]["FirstDeliveryDate"].ToString());
                    sheet[ROW, colLastDeliveryDate].Text = GetDate(dt.Rows[i]["LastDeliveryDate"].ToString());
                    sheet[ROW, colLastPlanningDatePR].Text = GetDate(dt.Rows[i]["LastPlanningDatePR"].ToString());
                    sheet[ROW, colLastPlanningDateWC].Text = GetDate(dt.Rows[i]["LastPlanningDateWC"].ToString());
                    sheet[ROW, colLineStartDateAtWC].Text = GetDate(dt.Rows[i]["LineStartDateAtWC"].ToString());
                    sheet[ROW, colMainRawMaterialInhouseDate].Text = GetDate(dt.Rows[i]["MainRawMaterialInhouseDate"].ToString());
                    sheet[ROW, colOtherRawMaterialInhouseDate].Text = GetDate(dt.Rows[i]["OtherRawMaterialInhouseDate"].ToString());
                    sheet[ROW, colProductionCompletionDate].Text = GetDate(dt.Rows[i]["ProductionCompletionDate"].ToString());
                    sheet[ROW, colProductionStartDateAtPR].Text = GetDate(dt.Rows[i]["ProductionStartDateAtPR"].ToString());
                    sheet[ROW, colTargetDate].Text = GetDate(dt.Rows[i]["TargetDate"].ToString());
                    sheet[ROW, colLSD].Text = GetDate(dt.Rows[i]["LSD"].ToString());



                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }


                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;
                workbook.Version = ExcelVersion.Excel97to2003;
                string strFileName = "OS2.xls";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
                excelEngine.Dispose();
            }
            catch (Exception ex)
            {


            }


            return null;
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

        private void getOS2(out DataTable dtOS2)
        {

            dtOS2 = new DataTable();
            try
            {
                string sql = @"select 
                            WC.UserName as WorkCenter,p1.ProductionDate AS TargetDate,po.Id AS ProductionOrderID,
                             p1.Quantity AS PlannedQtyForTheDay,t1.SPT,t1.ProductionPriority,
                              buyer=STUFF((select distinct ','+XB.UserName from 
	                                                                                trn.SalesOrder XSO 
		                                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                                                left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                                                where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                              StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
	                                                                             trn.SalesOrder XSO 
		                                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                   
			                                                                                where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                    
                             StyleGroup=STUFF((select distinct ','+xmoi.ProductionGrouping from 
	                                                                             trn.SalesOrder XSO 
		                                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                   
			                                                                                where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                    
                            MasterOrderNo=STUFF((select distinct ','+XMO.MasterOrderNo from 
	                                                                                trn.SalesOrder XSO 
		                                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                                               left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                                                                where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                    
                            OrderStatus=STUFF((select distinct ','+os.UserName from 
	                                                                                trn.SalesOrder XSO 
		                                                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                                               left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                                               left outer join [HKP].[OrderStatus] OS on OS.id=XMO.OrderStatusId
			                                                                                where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                                                    
                            ord.FOB,ord.CM,t1.TargetPerDay AS LineTargetPerDay,ord.OrderQty,ord.PlannedQty AS PlanOrderQty,prodWC.ProductionQtyAtWC,prodpr.ProductionQtyAtPR,u.UserName AS Unit,e.UserName AS Entity,
                            ACCI.EmployeeName AS AccountIncharge,acch.EmployeeName AS AccountHolder,ord.FirstDeliveryDate,ord.LastDeliveryDate,ord.ProductionCompletionDate,
                            DATEADD(DD,ISNULL(PRDTIME.LastDayOfProduction,0)*-1, ord.LastDeliveryDate) AS BaseProcessCompletionDate,

                            ord.CM*p1.Quantity AS BookedCM,t1.NoOfWorkStation,t1.LSD,t1.CommitmentDate,t1.MainRawMaterialInhouseDate,
                            t1.OtherRawMaterialInhouseDate,
                            --ord.OrderStatus,
                            PRODWC.LineStartDateAtWC,PRODPR.ProductionStartDateAtPR,p3.LastPlanningDateWC,p2.LastPlanningDatePR,
                            pc.UserName AS ProductCategory,
                            --targetdate,earlyStartDate,
                            p1.ProductionHours AS WorkingHours

                            from trn.ProductionOrder PO
                            inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
                            INNER join ProductionPlanningType1 p1 on p1.ProductionOrderID=t1.ProductionOrderID and ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)


                            left outer join scs.WorkCenterMaster WC on wc.id=p1.WorkCenterMasterId
                            LEFT OUTER JOIN EmployeeInformation AS ACCI ON ACCI.SystemId=wc.AccountInCharge
                            LEFT OUTER JOIN EmployeeInformation AS ACCH ON ACCH.SystemId=wc.AccountHolder

                            LEFT OUTER JOIN (SELECT MAX(days) AS LastDayOfProduction,pops.ProductionOrderId 
                              FROM trn.ProductionOrderProcessSet AS pops WHERE pops.Symbol='+'
                            GROUP BY pops.ProductionOrderId) AS PRDTIME ON prdtime.ProductionOrderId=po.Id

                            left outer join mst.MaterialMaster mm on mm.id=p1.MaterialMasterId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                            left outer join org.Entity E  on e.Id=p1.EntityID
                            LEFT OUTER JOIN org.Unit AS u ON u.Id=e.UnitId
                            left outer join org.Plant PLN on pln.Id=PO.PlantId


                            --planning at PR Level
                            left outer join (
				                            SELECT P1.ProductionOrderID,P1.ProcessID,p1.WorkCenterMasterId,
				                            mIN(ProductionDate) AS ProductionStartDate,max(ProductionDate) AS LastPlanningDatePR 
				                            FROM ProductionPlanningType1 p1 group by  P1.ProductionOrderID,P1.ProcessID,p1.WorkCenterMasterId) as p2 
				                            on p2.ProductionOrderID=t1.ProductionOrderID  and p1.WorkCenterMasterId=p2.WorkCenterMasterId
				                            and p2.ProcessID=(select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and  ProductionOrderID=p1.ProductionOrderID)
                            --planning at WC Level
                            left outer join (
				                            SELECT P1.ProcessID,p1.WorkCenterMasterId,
				                            mIN(ProductionDate) AS ProductionStartDate,max(ProductionDate) AS LastPlanningDateWC 
				                            FROM ProductionPlanningType1 p1 group by  P1.ProcessID,p1.WorkCenterMasterId) as p3 
				                            on p1.WorkCenterMasterId=p3.WorkCenterMasterId
				                            and p3.ProcessID=(select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and  ProductionOrderID=p1.ProductionOrderID)

                            --production at WC Level
                            LEFT OUTER JOIN (
				                            SELECT s.EntityId,s.WorkCenterMasterId,s.ProcessId,SUM(d.Qty) AS ProductionQtyAtWC,MIN(s.ProductionDate) AS LineStartDateAtWC
				                            FROM  trn.ProductionOrderDetail POD 
				                            inner join trn.ProductionSummary S ON pod.SalesOrderId=s.SalesOrderId
				                            INNER JOIN trn.ProductionSummaryDetail AS D  ON d.ProductionSummaryId=s.Id
				                            GROUP BY  s.WorkCenterMasterId,s.EntityId,s.ProcessId
                            ) AS PRODWC ON  p1.EntityID=PRODWC.EntityId AND PRODWC.WorkCenterMasterId=p1.WorkCenterMasterId AND p1.ProcessID=PRODWC.ProcessId

                            --production at PR Level
                            LEFT OUTER JOIN (
				                            SELECT pod.ProductionOrderId,s.ProcessId,SUM(d.Qty) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
				                            FROM  trn.ProductionOrderDetail POD 
				                            inner join trn.ProductionSummary S ON pod.SalesOrderId=s.SalesOrderId
				                            INNER JOIN trn.ProductionSummaryDetail AS D  ON d.ProductionSummaryId=s.Id
				                            GROUP BY  pod.ProductionOrderId,s.ProcessId
                            ) AS PRODPR ON  PRODPR.ProductionOrderId=p1.ProductionOrderID AND p1.ProcessID=PRODPR.ProcessId


                            left outer join (
                            select POD.ProductionOrderId,
                            SUM(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty,
                            --SUM(ceiling((so.Qty*(1+(moi.ExtraOrderPercentage/100)))*(1+(moi.OrderWastagePercentage/100)))) AS PlannedQty,
                            min(so.DeliveryDate) AS FirstDeliveryDate,
                            max(so.DeliveryDate) AS LastDeliveryDate,
                            MAX(so.CommitmentDate) AS ProductionCompletionDate,
                            AVG(c.FOB) AS FOB,AVG(c.CM) AS CM,
                            sum(SO.Qty) AS OrderQty from trn.ProductionOrderDetail POD 
                            left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                            left outer join [HKP].[Party] p on P.Id=MO.plantID
                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            --left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
                            group by POD.ProductionOrderId--,MO.MasterOrderNo,b.UserName,os.UserName
                            ) AS ORD on ord.ProductionOrderID=PO.Id
                            order by wc.Code,p1.ProductionDate";





                dtOS2 = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);

            }

        }



        #endregion OS2


        #region PriorityUpdate


        [HttpPost, Authorize]
        public ActionResult SaveFileList(List<Dictionary<string, object>> data)
        {
            try
            {
                Library.Planning.PlanningType1.PlanningType1Scheduler sch = new Library.Planning.PlanningType1.PlanningType1Scheduler();

                sch.SaveFileList(data);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetSampleReports(ReportFormat reportFormat, string Entity)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string date = DateTime.Now.Date.ToString("dd-MMM");//.Substring(0, DateTime.Now.Date.ToString().Length - 12);
            var reportFileName = "ProrityReport-" + date;
            var workbook = GetPriorityReport(Entity);
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

        private IWorkbook GetPriorityReport(string Entity)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            var sheet2 = workbook.Worksheets[1];
            Library.Planning.PlanningType1.PlanningType1Scheduler sch = new Library.Planning.PlanningType1.PlanningType1Scheduler();
            /// Sheet 1 
            DataTable data = sch.getCurrentPriority(Entity);

            sheet.Name = "Current-Priority";



            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            #region Headers
            //report.SetHeaderText(ref sheet, ROW, COL, "Employee Id ", 12, ExcelHAlign.HAlignLeft);
            //int ColEmpSystemId = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ProductionId", 8, ExcelHAlign.HAlignLeft);
            int ColProdId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Status", 8, ExcelHAlign.HAlignLeft);
            int ColStat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ProductionPriority", 8, ExcelHAlign.HAlignLeft);
            int ColPr = COL;
            COL++;

            endCol = COL;
            #endregion Headers
            ROW++;
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                //sheet[ROW, ColEmpSystemId].Text = data.Rows[i]["EmpSystemId"].ToString();
                sheet[ROW, ColProdId].Text = data.Rows[i]["ProductionId"].ToString();
                sheet[ROW, ColStat].Text = data.Rows[i]["Status"].ToString();
                sheet[ROW, ColPr].Text = data.Rows[i]["ProductionPriority"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            report.PageSetup(ref sheet2, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }


        [HttpPost, Authorize]
        public ActionResult ImportData()
        {
            string path;

            try
            {
                var file = Request.Files["file"];
                //string plantId = Request.Files["plantId"].ToString();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFile(out path);
                var data = ReadData(path);

                var json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public List<PriorityUp> ReadData(string path)
        {

            DataSet dsExcel = null;
            try
            {
                List<PriorityUp> data = new List<PriorityUp>();
                List<PriorityUp> dataToUpdate = new List<PriorityUp>();
                List<object> ret = new List<object>();
                ReadFile(path, out dsExcel);

                data = dsExcel.Tables[0].ToList<PriorityUp>();

                if (data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        var isNumeric = double.TryParse(data[i].ProductionPriority, out double n);
                        if (isNumeric == false)
                        {
                            throw new Exception("The Priority is not a number!! Please check - " + (i + 2));
                        }
                    }

                }

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void ReadFile(string path, out DataSet dsExcel)
        {
            FileInfo docFile;
            dsExcel = null;
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = excelEngine.Excel.Workbooks.Open(path);
                DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                dsExcel = new DataSet();
                dsExcel.Tables.Add(dt);
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    //exception += "\r\nTrying to delete";
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


        public void SaveFile(out string path)
        {
            path = "";
            try
            {
                var file = Request.Files["file"];
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {
                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetOTManualFile(), file.FileName);
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
        #endregion PriorityUpdate

        #region Type2
        [Authorize]
        public ActionResult GetType2List(string baseprocessid, string entityid, string column, string value)
        {
            string entityId = "'" + entityid.Replace(",", "','") + "'";//replaced with ""
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (
                        SELECT  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
                        ISNULL(PO.Qty,0) AS POQuantity,ISNULL(PO.PlannedQty,0) AS SOQuantity,ISNULL(SO.SOQuantity,0) AS SavedQuantity,t1.Color,FORMAT(t1.LSD,'dd-MMM-yyyy') AS LSD,FORMAT(t1.CommitmentDate,'dd-MMM-yyyy') AS CommitmentDate,t1.TargetPerDay
                                ,T1.ProductionPriority ,so.Material, so.Product,t1.Qty,
                                so.ProductCategory, so.FirstShipmentDate,
                                so.LastShipmentDate, so.buyer, so.BuyerRefNo,
                                so.OwnRefNo, so.StyleNo, so.OwnStyleNo, so.SONo,
                                so.SODesc,So.MasterOrderId,
                                so.Customer,so.article,PRODPR.ProductionQtyAtPR 
                                   ,ISNULL(CASE WHEN ISNULL(T1.Qty,0)>0 THEN T1.Qty ELSE PO.PlannedQty END,0)-(ISNULL(PRODPR.ProductionQtyAtPR,0)-ISNULL(PRDQ.ProductionBookedQty,0)) AS ToBePlanQty
                                  			
  
                            FROM [TRN].[ProductionOrderType2] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            INNER JOIN ProductionOrderSchedulingParametersType2 t1 ON t1.ProductionOrderID=po.Id

                             LEFT OUTER JOIN (
												SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
											FROM  trn.ProductionSummary S 
											--WHERE  CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PRODPR ON  PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
							 left outer join (SELECT pod.ProductionOrderId,
                                sum(isnull(so.ProductionBookedQty,0)) ProductionBookedQty
                                 FROM trn.SalesOrder AS so
                                INNER JOIN trn.ProductionOrderType2Detail AS pod ON pod.SalesOrderId=so.Id

                                GROUP BY pod.ProductionOrderId
                            ) AS PRDQ ON PRDQ.ProductionOrderId=T1.ProductionOrderId
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId, sum(so.Qty) AS SOQuantity, Min(so.DeliveryDate) FirstShipmentDate
													,Max(so.DeliveryDate) LastShipmentDate,
                                                    MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											 
					                                BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													,Material=STUFF((select distinct ', '+mm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                     ,Article=STUFF((select distinct ', '+mm.StandardName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													,Product=STUFF((select distinct ', '+Pm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													,ProductCategory=STUFF((select distinct ', '+pc.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
from trn.ProductionOrderType2Detail AS pod JOIN  trn.SalesOrder SO ON pod.SalesOrderId=so.Id group by pod.ProductionOrderId) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE isnull(s.username,'') IN ('" + PlanningStatus.ACTIVE.ToString() + @"','" + PlanningStatus.RUNNING.ToString() + @"') AND  PO.entityid IN(" + entityId + @") and PO.PlanningTypeProcessId ='" + baseprocessid + @"' ) AS TEMP WHERE " + strkey + " ORDER BY ProductionPriority";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetPOType2List(string column, string value, string baseprocessid, string entityid)
        {
            string entityId = "'" + entityid.Replace(",", "','") + "'";//replaced with ""
            string strKey = "1=1";
            if (column != "")
                strKey = column + " LIKE '%" + clsStaticInfo.nullrecorder(value) + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (
                        SELECT 'INCLUDE' AS WCPreferenceType,1 AS RunningOrderBlockSize, PO.Id, PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, 
EN.UserName AS EntityName, S.UserName AS ProductionStatusName,isnull(PO.Qty,0) AS POQuantity,ISNULL(PO.PlannedQty,0) AS SOQuantity,SO.*
                                FROM [TRN].[ProductionOrderType2] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId
                                                   ,Material=STUFF((select distinct ', '+mm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                    ,NoOfArticle=(select COUNT(mm.StandardName) from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId)
                                                    ,Article=STUFF((select distinct ', '+mm.StandardName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMasterarticle mm on mm.id=XMOI.ArticleId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												   ,Product=STUFF((select distinct ', '+Pm.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

												   ,ProductCategory=STUFF((select distinct ', '+pc.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												   
												   ,ProductMasterId=STUFF((select distinct ', '+pm.id from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join mst.MaterialMaster mm on mm.id=XMOI.MaterialMasterId
															left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

												   ,
                                                     FORMAT(Min(sO.LSD),'dd-MMM-yyyy') AS LSD,FORMAT(max(SO.PlanExFactoryDate),'dd-MMM-yyyy') AS PlanExFactoryDate ,
                                                    sum(so.Qty) AS TotalSOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate
                                                    --,SUM((isnull(SO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
													,PlannedQty=(Select SUM((isnull(XSO.qty, 0) * (1 + (isnull(xmoi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(xmoi.OrderWastagePercentage, 0))))
															from trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
															 where pod.ProductionOrderId=Xpod.ProductionOrderId)

                                                    ,MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											 
					                                BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderType2Detail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                     
from trn.ProductionOrderType2Detail AS pod JOIN  trn.SalesOrder SO ON pod.SalesOrderId=so.Id group by pod.ProductionOrderId) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                        WHERE po.Id NOT IN (SELECT ProductionOrderSchedulingParametersType2.ProductionOrderID
                      FROM ProductionOrderSchedulingParametersType2)
                            AND 
isnull(S.username,'')<>'" + PlanningStatus.CLOSED.ToString() + @"' AND  po.entityid IN(" + entityId + @") and PO.PlanningTypeProcessId ='" + baseprocessid + @"') AS TEMP where " + strKey;

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetAllWorkcenterWisePlanningType2Summary(string EntityId)
        {

            Library.Planning.PlanningType1.PlanningType1Scheduler sch = new Library.Planning.PlanningType1.PlanningType1Scheduler();

            return Json(_sqlRepository.GetDataCollection(sch.GetAllWorkcenterWisePlanningType2Summary(EntityId), null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetSingleWorkcenterWisePlanningType2Summary(string WorkCenterId)
        {

            Library.Planning.PlanningType1.PlanningType1Scheduler sch = new Library.Planning.PlanningType1.PlanningType1Scheduler();

            return Json(_sqlRepository.GetDataCollection(sch.GetSingleWorkcenterWisePlanningType2Summary(WorkCenterId), null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateType2Priority(List<Dictionary<string, object>> data)
        {

            try
            {
                if (data == null)
                    throw new Exception("No data changed!!!");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                for (int i = 0; i < data.Count; i++)
                {
                    con.executeQuery("update ProductionOrderSchedulingParametersType2 SET ProductionPriority =" + clsStaticInfo.dbl(data[i]["ProductionPriority"].ToString()) + " WHERE ProductionOrderID='" + data[i]["Id"].ToString() + "' ");
                }

                con.CommitTransaction();

                return Json(new { Error = false, Message = "Priority successfully reinitialized" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        [HttpGet, Authorize]
        public JsonResult GetProductionType2RecipeMaterialList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionOrderType2MaterialList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult LoadType2FilterSQL()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT * FROM (
                                        SELECT DISTINCT 
                                        isnull(po.Id,'') ProductOrderId,isnull(mo.Id,'')AS MasterOrderNo,isnull(mo.BuyerReferenceNo,'') AS BuyerOrderNo,isnull(moi.BuyerReferenceNo,'') AS BuyerItemNo,
                                        wc.Sequence, isnull(wc.Id,'') AS WorkCenterId,isnull(wc.UserName,'') AS WorkCenter,isnull(e.Id,'') AS EntityId,isnull(e.UserName,'') Entity,
                                        isnull(pm.Id,'') ProductMasterId,isnull(pm.UserName,'') ProductMaster,isnull(pc.Id,'') ProductCategoryId,isnull(pc.UserName,'') ProductCategory,
                                        isnull(mm.Id,'') MaterialMasterId,isnull(mm.UserName ,'')MaterialMaster,isnull(MMr.Id,'') ArticleId,isnull(mmr.ShortName,'') Article,isnull(b.Id,'') BuyerId,isnull(b.UserName,'') Buyer,
                                        isnull(p.Id,'') CustomerId,isnull(p.UserName,'') Customer,
                                        isnull(acci.SystemId,'') AccountInchargeId, isnull(ACCI.EmployeeName,'') AS AccountIncharge,isnull(acch.SystemId,'') AccountHolderId, isnull(acch.EmployeeName,'') AS AccountHolder,
                                        isnull(ps.Id,'') AS ProductionStatusId, isnull(ps.UserName,'') AS ProductionStatus
                                        from trn.ProductionOrderType2 PO
				                                inner join ProductionOrderSchedulingParametersType2 T1 on t1.ProductionOrderID=po.Id
				                                INNER join ProductionPlanningType2 p1 on p1.ProductionOrderID=t1.ProductionOrderID and ProcessID=(select ProcessId from trn.ProductionOrderType2ProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
	
				                                left outer join scs.WorkCenterMaster WC on wc.id=p1.WorkCenterMasterId
				                                LEFT OUTER JOIN EmployeeInformation AS ACCI ON ACCI.SystemId=wc.AccountInCharge
				                                LEFT OUTER JOIN EmployeeInformation AS ACCH ON ACCH.SystemId=wc.AccountHolder

				                                INNER JOIN trn.ProductionOrderType2Detail AS pod ON pod.ProductionOrderId=PO.Id
				                                LEFT OUTER JOIN trn.SalesOrder SO ON so.Id=pod.SalesOrderId
				                                left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
				                                left outer join mst.MaterialMaster mm on mm.id=p1.MaterialMasterId
				                                LEFT OUTER JOIN MST.MaterialMasterArticle MMR ON mmr.Id=moi.ArticleId
 
				                                left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
				                                left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
				                                left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

				                                left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
				                                left outer join [HKP].Buyer B on B.Id=MO.BuyerId
				                                left outer join [HKP].[Party] p on P.Id=MO.PartyId

				                                left outer join org.Entity E on e.Id=p1.EntityID
				                                LEFT OUTER JOIN org.Unit AS u ON u.Id=e.UnitId
				                                left outer join org.Plant PLN on pln.Id=PO.PlantId
				                                LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
				
                                WHERE po.PlantId='" + identity.PlantId + @"'
                                ) AS KK";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getProductMasterType2Parameters(string productionOrderID, string entityid, string baseprocessid)
        {


            string sql = @"SELECT SSS.*,PD.ProductMasterId AS Id,pm.UserName AS ProductName,pc.UserName AS ProductCategory,puc.UserName AS ProductSubCategory, 
                            pme.NoOfWorkStation, pme.EfficencyPercentage AS Efficiency,pme.StandardWorkingHours PlanWorkingHoursPerDay, pme.SPT,
                            MLD.[Value] AS MinimumLineDays,format((SELECT min(SO.MainRawMaterialInhouseDate) AS MainRawMaterialInhouseDate
                                        FROM trn.SalesOrder AS so
                                        INNER JOIN trn.ProductionOrderType2Detail AS pod ON pod.SalesOrderId=so.Id where pod.ProductionOrderId='" + productionOrderID + @"'),'dd-MMM-yyyy') AS MainRawMaterialInhouseDate,
                                   format( (SELECT min(SO.OtherRawMaterialInhouseDate) AS OtherRawMaterialInhouseDate
                                        FROM trn.SalesOrder AS so
                                        INNER JOIN trn.ProductionOrderType2Detail AS pod ON pod.SalesOrderId=so.Id where pod.ProductionOrderId='" + productionOrderID + @"'),'dd-MMM-yyyy') AS OtherRawMaterialInhouseDate,
                                   format( (SELECT min(SO.LSD) AS LSD
                                        FROM trn.SalesOrder AS so
                                        INNER JOIN trn.ProductionOrderType2Detail AS pod ON pod.SalesOrderId=so.Id where pod.ProductionOrderId='" + productionOrderID + @"'),'dd-MMM-yyyy') AS LSD,
                                     format((SELECT MAX(SO.CommitmentDate) AS CommitmentDate
                                                        FROM trn.SalesOrder AS so
                                        INNER JOIN trn.ProductionOrderType2Detail AS pod ON pod.SalesOrderId=so.Id where pod.ProductionOrderId='" + productionOrderID + @"'),'dd-MMM-yyyy') AS CommitmentDate,
                                                                    PM.FirstdayOutPut AS FirstDayOutPut,PM.IncrementValue,PM.DaysToReachTheTarget AS DayToReachTheTarget,
                                CASE WHEN ISNULL(PD.IsFixed,'')='FIXED' THEN 'FIXED' ELSE 'PERCENTAGE' END AS IncrementType
                                    FROM [TRN].[ProductDefinition] PD
                                LEFT OUTER JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId AND PM.BaseProcessId='" + baseprocessid + @"'
                                LEFT OUTER JOIN [HKP].[ProductCategory] PC ON pc.Id=pm.ProductCategoryId
                                    LEFT OUTER JOIN [HKP].[ProductSubCategory] PUC ON PUC.Id=pm.ProductSubCategoryId
                                LEFT OUTER JOIN [TRN].[ProductMasterEfficency] PME ON pme.ProductMasterId=pm.Id AND pme.EfficencyName='Planning'
                                LEFT OUTER JOIN dbo.EntityConfig con ON 1=1 and con.EntityId='" + entityid + @"' AND con.StandardName='" + EntityConfigParameter.StandardWorkingHoursPerDay + @"'
                                   LEFT OUTER JOIN dbo.EntityConfig MLD ON 1=1 and MLD.EntityId='" + entityid + @"' AND MLD.StandardName='" + EntityConfigParameter.MinimumLineDays + @"'
                        
                                   CROSS JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,ma.StandardName AS Article, --PM.UserName AS Product,pc.UserName AS ProductCategory,
                                                     min(so.DeliveryDate) AS FirstShipmentDate,  max(so.DeliveryDate) AS LastShipmentDate,
                                                    sum(so.Qty) AS Qty,
                                                    MasterOrderId =STUFF((select distinct ','+XMOI.Id from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
                                                    BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderType2Detail AS podx ON podx.SalesOrderId=sox.Id                                                
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
                                                      JOIN trn.ProductionOrderType2Detail AS pod ON pod.SalesOrderId=so.Id
                                                    left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                    left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                    left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
													LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                 WHERE pod.ProductionOrderId='" + productionOrderID + @"'
                                                    group by pod.ProductionOrderId,mm.userName,ma.StandardName,PM.UserName,pc.UserName) AS SSS
                        WHERE PD.MaterialMasterId IN (
	
                                    SELECT DISTINCT moi.MaterialMasterId FROM [TRN].[ProductionOrderType2Detail] D
                                    INNER JOIN trn.SalesOrder AS so ON so.Id=d.SalesOrderId
                                    INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                    WHERE d.ProductionOrderId='" + productionOrderID + @"'
                                    )    ";

            string sqlBulletinData = @"SELECT tm.RequiredStdTarget,tm.PlannedHoursPerDay, tm.MaxNoOfWS,sum(d.TotalSPT) AS SPT
									,SUM(D.AllotedWorkstation) AS TotalWS
									  FROM trn.ProductionBulletinTemplate AS T
									INNER JOIN  trn.ProductionBulletinTemplateMaster AS TM ON t.Id=tm.ProductionBulletinTemplateId
									INNER JOIN trn.ProductionBulletinTemplateDetail AS D ON d.ProductionBulletinTemplateMasterId=TM.Id
									WHERE t.ProductionOrderId='" + productionOrderID + "' AND TM.ProcessId='" + baseprocessid + @"'
									GROUP BY tm.RequiredStdTarget,tm.PlannedHoursPerDay, tm.MaxNoOfWS";

            return Json(new { MainData = _sqlRepository.GetDataCollection(sql), BulletinData = _sqlRepository.GetDataCollection(sqlBulletinData) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> ProductionType2PlanSimulation(string entityid, string processid)
        {
            return await Task.Factory.StartNew(() =>
            {
                try
                {

                    string EntityIds = "" + entityid + "";
                    string _sql = @"SELECT distinct WCM.EntityId
                                  from (SELECT distinct W.ProductionOrderId,W.WorkCenterMasterId FROM trn.ProductionOrderType2WorkCenter AS W
                                UNION
                                SELECT distinct W.ProductionOrderId,W.WorkCenterMasterId FROM trn.RunningOrderType2WorkCenter AS W
                                ) AS W
                                join [dbo].[ProductionOrderSchedulingParametersType2] spo on spo.ID=W.ProductionOrderId
JOIN trn.ProductionOrderType2 AS po ON po.Id=spo.ProductionOrderId
                                join scs.WorkCenterMaster AS wcm ON wcm.Id=w.WorkCenterMasterId
                                INNER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId
                                WHERE  (po.EntityId IN(" + entityid + @") OR WCM.EntityId IN(" + entityid + @")) 
                            AND ps.UserName NOT IN ('" + PlanningStatus.CLOSED.ToString() + @"')
UNION
SELECT distinct po.EntityId FROM 
trn.ProductionOrderType2WorkCenter W
join [dbo].[ProductionOrderSchedulingParametersType2] spo on spo.ID=W.ProductionOrderId
JOIN trn.ProductionOrderType2 AS po ON po.Id=spo.ProductionOrderId
JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=w.WorkCenterMasterId
INNER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId
WHERE WCM.EntityId IN(" + entityid + @") AND ps.UserName NOT IN ('" + PlanningStatus.CLOSED.ToString() + @"')
UNION
SELECT distinct po.EntityId FROM 
trn.RunningOrderType2WorkCenter W
join [dbo].[ProductionOrderSchedulingParametersType2] spo on spo.ID=W.ProductionOrderId
JOIN trn.ProductionOrderType2 AS po ON po.Id=spo.ProductionOrderId
JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=w.WorkCenterMasterId
INNER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId
WHERE WCM.EntityId IN(" + entityid + @") AND ps.UserName NOT IN ('" + PlanningStatus.CLOSED.ToString() + @"')";

                    DataTable dt = _sqlRepository.GetDataTable(_sql);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        EntityIds += ",'" + dt.Rows[i]["EntityId"].ToString() + "'";
                    }
                    ProductionPlanType2SimulationAlgorithm(entityid, EntityIds, processid);
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        Error = true,
                        Message = ex.Message
                    }, JsonRequestBehavior.AllowGet);
                }
                finally
                {

                }

                return Json(new { Error = false, Message = "Success" }, JsonRequestBehavior.AllowGet);
            });
        }

        public void ProductionPlanType2SimulationAlgorithm(string entityid, string ProcessingEntities, string processid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.General.Setups.ProcessLock _lock = new Library.General.Setups.ProcessLock(identity.Name, Library.General.Setups.ProcessLockId.PlanningType1, entityid);
            _lock.LockProcess();
            try
            {
                SendNotification("-------------Starting Simulation-------------");
                DataTable dtWCValidation = _sqlRepository.GetDataTable(@"SELECT wcm.Id,ed.StartDate,wcm.UserName FROM scs.WorkCenterMaster AS wcm 
                                        LEFT JOIN scs.WorkCenterMasterEffectiveDate AS ED ON ed.WorkCenterMasterId=wcm.Id AND ed.Id=(SELECT TOP 1 Id FROM scs.WorkCenterMasterEffectiveDate WHERE WorkCenterMasterId=wcm.Id ORDER BY StartDate DESC)
                                        WHERE wcm.EntityId IN (" + ProcessingEntities + @")  AND wcm.ProcessId='" + processid + @"'  AND wcm.Active=1");

                if (dtWCValidation.Rows.Count == 0)
                    throw new Exception("No workcenter found. Please create workcenters and try again");

                string WithoutEffectiveDate = "";
                for (int i = 0; i < dtWCValidation.Rows.Count; i++)
                {
                    if (dtWCValidation.Rows[i]["StartDate"].ToString() != "")
                    {
                        WithoutEffectiveDate = dtWCValidation.Rows[i]["StartDate"].ToString();
                        break;
                    }
                }

                if (WithoutEffectiveDate == "")
                    throw new Exception("No workcenter was found with effective date. Please set effective date for workcenters");

                Dictionary<string, DataTable> dicWorkCenterRunningHours = WorkCenterRunningHours();

                //first close all production order having all sales order closed
                _sqlRepository.ExecuteSqlCommand(@"UPDATE  trn.ProductionOrderType2 SET ProductionStatusId = (SELECT TOP 1 Id FROM hkp.ProductionStatus AS ps WHERE ps.StandardName='Closed')
                                                FROM trn.ProductionOrderType2 PO 

                                                WHERE PO.Id IN (
			                                                SELECT PO.Id AS ProductionOrderId FROM [dbo].[ProductionOrderSchedulingParametersType2] spo 
INNER JOIN trn.ProductionOrderType2 AS po ON po.Id=spo.ProductionOrderId
			                                                INNER JOIN hkp.ProductionStatus ps ON ps.Id=po.ProductionStatusId
			                                                LEFT OUTER JOIN trn.ProductionOrderType2Detail AS pod ON pod.ProductionOrderId=po.Id
			                                                AND pod.Id = (
			                                                SELECT TOP 1 pod.Id FROM trn.ProductionOrderType2Detail AS pod
			                                                INNER JOIN trn.SalesOrder AS so ON so.id=pod.SalesOrderId 
			                                                INNER JOIN hkp.OrderStatus AS os ON os.Id=so.OrderStatusId
			                                                WHERE os.Id='" + Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"' AND pod.ProductionOrderId=po.Id)
			                                                WHERE ISNULL(pod.Id,'')='' AND ps.StandardName<>'Closed'
                                                )");

                _sqlRepository.ExecuteSqlCommand(@"DELETE FROM ProductionPlanningType2 WHERE ProductionOrderID IN (
                                               SELECT po.Id FROM [dbo].[ProductionOrderSchedulingParametersType2] spo
                                                INNER JOIN trn.ProductionOrderType2 AS po ON po.id=spo.ProductionOrderID
                                                INNER JOIN ProductionPlanningType2 AS ppt ON ppt.ProductionOrderID=spo.ID
                                                INNER JOIN hkp.ProductionStatus AS ps ON po.ProductionStatusId=ps.Id
                                                WHERE ps.UserName='" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"'
                                                )");

                string runningsql = @"SELECT DISTINCT po.Id FROM [dbo].[ProductionOrderSchedulingParametersType2] spo
                                    INNER JOIN trn.ProductionOrderType2 AS po ON po.id=spo.ProductionOrderID
                                    LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                                    LEFT OUTER JOIN trn.RunningOrderType2WorkCenter AS r ON spo.Id=r.ProductionOrderId
                                    WHERE ISNULL(r.Id,'')='' AND ps.UserName='running' AND po.EntityId IN (" + ProcessingEntities + @")";
                DataTable dtCheck = _sqlRepository.GetDataTable(runningsql);
                if (dtCheck.Rows.Count > 0)
                {
                    string ids = "";
                    for (int i = 0; i < dtCheck.Rows.Count; i++)
                    {
                        if (ids == "")
                            ids = dtCheck.Rows[i]["id"].ToString();
                        else
                            ids += "," + dtCheck.Rows[i]["id"].ToString();
                    }

                    new Exception("The following production orders are running but no workcenter was defined: " + ids);
                }

                _sqlRepository.ExecuteSqlCommand(@"delete FROM ProductionPlanningType2 where EntityId IN (" + ProcessingEntities + @")");

                Dictionary<string, double> DicBalanceWorkcenterHours = new Dictionary<string, double>();

                DataTable dtWorkCenter = dtType2AllAvailableWrokcenters(ProcessingEntities, processid);
                dtWorkCenter.Columns.Add("CURRENT_APPLICABLE");
                dtWorkCenter.Columns.Add("ACTUAL_APPLICABLE");
                dtWorkCenter.Columns.Add("AlreadyBooked", typeof(double));
                dtWorkCenter.Columns.Add("isResidualApplicable", typeof(bool));
                dtWorkCenter.DefaultView.RowFilter = null;
                DataTable dvDistinctEntity = dtWorkCenter.DefaultView.ToTable(true, "EntityId");

                Dictionary<string, DataTable> dicCalendar = dtProductionType2Calendar(System.DateTime.Now, 1500, processid, ProcessingEntities);
                DataTable dtCalendar = new DataTable("Temp");
                DataTable productionOrders = dtProductionType2Parameters(ProcessingEntities);
                for (int i = 0; i < productionOrders.Rows.Count; i++)
                {

                    dtCalendar = dicCalendar[productionOrders.Rows[i]["EntityId"].ToString()];

                    sbLog = new StringBuilder();
                    SendNotification("Simulating production order#" + productionOrders.Rows[i]["ProductionOrderID"].ToString(), i, productionOrders.Rows.Count);
                    sbLog.AppendLine("Starting simulation for production order#" + productionOrders.Rows[i]["ProductionOrderID"].ToString());
                    DateTime startDate = Convert.ToDateTime(Convert.ToDateTime(productionOrders.Rows[i]["LSD"].ToString()).ToString("dd-MMM-yyyy"));
                    DateTime LSD = Convert.ToDateTime(Convert.ToDateTime(productionOrders.Rows[i]["LSD"].ToString()).ToString("dd-MMM-yyyy"));
                    double DaysToReachTheTarget = clsStaticInfo.dbl(productionOrders.Rows[i]["DayToReachTheTarget"].ToString());
                    DaysToBeAddedForLineChange = (int)DaysToReachTheTarget - 1;

                    DataTable dtCurrentWorkCenter = dtType2AvailableWrokcenters(productionOrders.Rows[i]["ProductionOrderID"].ToString(), productionOrders.Rows[i]["ProductionStatusName"].ToString(), processid);

                    StringCollection strColMultipleProductionInSingleLine = new StringCollection();
                    if (dtCurrentWorkCenter.Rows.Count == 0)
                    {
                        foreach (DataRow item in dtWorkCenter.Rows)
                        {
                            if (strColMultipleProductionInSingleLine.Contains(item["WorkCenterMasterId"].ToString()) == false)
                            {
                                strColMultipleProductionInSingleLine.Add(item["WorkCenterMasterId"].ToString());


                                dtWorkCenter.DefaultView.RowFilter = "WorkCenterMasterId='" + item["WorkCenterMasterId"].ToString() + "'";
                                if (dtWorkCenter.DefaultView.Count == 1)
                                {
                                    item["CURRENT_APPLICABLE"] = "YES";
                                    item["ACTUAL_APPLICABLE"] = "YES";
                                    item["AlreadyBooked"] = 0;

                                }
                                else if (dtWorkCenter.DefaultView.Count > 1)
                                {
                                    dtWorkCenter.DefaultView.RowFilter = "WorkCenterMasterId='" + item["WorkCenterMasterId"].ToString() + "' AND MaterialMasterId='" + productionOrders.Rows[i]["MaterialMasterId"].ToString() + "'";
                                    if (dtWorkCenter.DefaultView.Count == 1)
                                    {
                                        dtWorkCenter.DefaultView[0].Row["CURRENT_APPLICABLE"] = "YES";
                                        dtWorkCenter.DefaultView[0].Row["ACTUAL_APPLICABLE"] = "YES";
                                        item["AlreadyBooked"] = 0;
                                    }
                                    else
                                    {
                                        dtWorkCenter.DefaultView.RowFilter = "WorkCenterMasterId='" + item["WorkCenterMasterId"].ToString() + "'";
                                        dtWorkCenter.DefaultView[0].Row["CURRENT_APPLICABLE"] = "YES";
                                        dtWorkCenter.DefaultView[0].Row["ACTUAL_APPLICABLE"] = "YES";
                                        item["AlreadyBooked"] = 0;
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        for (int WC = 0; WC < dtCurrentWorkCenter.Rows.Count; WC++)
                        {
                            if (strColMultipleProductionInSingleLine.Contains(dtCurrentWorkCenter.Rows[WC]["WorkCenterMasterId"].ToString()) == false)
                            {
                                strColMultipleProductionInSingleLine.Add(dtCurrentWorkCenter.Rows[WC]["WorkCenterMasterId"].ToString());

                                dtWorkCenter.DefaultView.RowFilter = "WorkCenterMasterId='" + dtCurrentWorkCenter.Rows[WC]["WorkCenterMasterId"].ToString() + "'";
                                if (dtWorkCenter.DefaultView.Count == 1)
                                {
                                    dtWorkCenter.DefaultView[0].Row["CURRENT_APPLICABLE"] = "YES";
                                    dtWorkCenter.DefaultView[0].Row["ACTUAL_APPLICABLE"] = "YES";
                                    dtWorkCenter.DefaultView[0].Row["AlreadyBooked"] = clsStaticInfo.dbl(dtCurrentWorkCenter.Rows[WC]["AlreadyBooked"].ToString());
                                    dtWorkCenter.DefaultView[0].Row["CurrentPRQty"] = clsStaticInfo.dbl(dtCurrentWorkCenter.Rows[WC]["CurrentPRQty"].ToString());
                                    dtWorkCenter.DefaultView[0].Row["isResidualApplicable"] = dtCurrentWorkCenter.Rows[WC]["isResidualApplicable"];

                                }
                                else if (dtWorkCenter.DefaultView.Count > 1)
                                {

                                    DataRow[] dr = dtWorkCenter.Select("WorkCenterMasterId='" + dtCurrentWorkCenter.Rows[WC]["WorkCenterMasterId"].ToString() + "' AND MaterialMasterId='" + productionOrders.Rows[i]["MaterialMasterId"].ToString() + "'");
                                    if (dr.Length == 1)
                                    {
                                        dr[0]["CURRENT_APPLICABLE"] = "YES";
                                        dr[0]["ACTUAL_APPLICABLE"] = "YES";
                                        dr[0]["AlreadyBooked"] = clsStaticInfo.dbl(dtCurrentWorkCenter.Rows[WC]["AlreadyBooked"].ToString());
                                        dr[0]["CurrentPRQty"] = clsStaticInfo.dbl(dtCurrentWorkCenter.Rows[WC]["CurrentPRQty"].ToString());
                                        dr[0]["isResidualApplicable"] = dtCurrentWorkCenter.Rows[WC]["isResidualApplicable"];


                                    }
                                    else
                                    {
                                        dr = dtWorkCenter.Select("WorkCenterMasterId='" + dtCurrentWorkCenter.Rows[WC]["WorkCenterMasterId"].ToString() + "'");
                                        dr[0]["CURRENT_APPLICABLE"] = "YES";
                                        dr[0]["ACTUAL_APPLICABLE"] = "YES";
                                        dr[0]["AlreadyBooked"] = clsStaticInfo.dbl(dtCurrentWorkCenter.Rows[WC]["AlreadyBooked"].ToString());
                                        dr[0]["CurrentPRQty"] = clsStaticInfo.dbl(dtCurrentWorkCenter.Rows[WC]["CurrentPRQty"].ToString());
                                        dr[0]["isResidualApplicable"] = dtCurrentWorkCenter.Rows[WC]["isResidualApplicable"];

                                    }

                                }
                            }

                        }

                    }

                    StringCollection strMaxAllocatedLines = new StringCollection();
                    double AllocatedLines = clsStaticInfo.dbl(productionOrders.Rows[i]["AllocatedLines"].ToString()); sbLog.AppendLine("Total Allocated Lines" + AllocatedLines);
                    double TotalLineDays = Math.Ceiling(clsStaticInfo.dbl(productionOrders.Rows[i]["RequiredLineDays"].ToString()));
                    double MinimumLineDays = clsStaticInfo.dbl(productionOrders.Rows[i]["MinimumLineDays"].ToString());


                    if (productionOrders.Rows[i]["ProductionStatusName"].ToString().ToUpper() == PlanningStatus.RUNNING.ToString())
                    {
                        AllocatedLines = clsStaticInfo.dbl(dtWorkCenter.Compute("COUNT(WorkCenterMasterId)", "CURRENT_APPLICABLE='YES'").ToString());
                        MinimumLineDays = clsStaticInfo.dbl(productionOrders.Rows[i]["RunningOrderBlockSize"].ToString());
                        DaysToBeAddedForLineChange = (int)MinimumLineDays;
                    }
                    sbLog.AppendLine("Minimum workcenter days" + MinimumLineDays);

                    double TotalOrderQuantity = (int)clsStaticInfo.dbl(productionOrders.Rows[i]["SOQuantity"].ToString()); sbLog.AppendLine("Total order qty" + TotalOrderQuantity);
                    double TempTotalOrderQty = TotalOrderQuantity;


                    double TargetPerDay = (int)clsStaticInfo.dbl(productionOrders.Rows[i]["TargetPerDay"].ToString());
                    List<ProductionBlock> _ProductionBlock = new List<ProductionBlock>();
                    StringCollection sbNoOfLineUtilization = new StringCollection();

                    int dayCount = 0;
                    string LastProductionLineID = "";
                    DataRow BestLine = null;
                    int Index = -1;
                    DateTime LSDForLine = startDate;
                    bool isBuildUpRequired = false;
                    int blockCount = 0;
                    while (TotalOrderQuantity > 0)
                    {

                        Index++;
                        bool isStyleChanged = false;
                        if (dayCount % MinimumLineDays == 0)
                        {
                            BestLine = null;//to determine the best line for each rotation, ignoring residual values plotting on the same line. delete this line if you want to assign residial value for current best line
                            isBuildUpRequired = false;
                            blockCount++;
                            sbLog.AppendLine("Start plotting block" + blockCount);
                            Index = 0;//important, resetting the calendar
                            DateTime tempdate = LSD;

                            GetPrefferedWorkcenter(dtWorkCenter, ref TotalOrderQuantity, TempTotalOrderQty);
                            #region Scan for each date starting from it's LSD to maximum block date to get the available best line to fit starting date


                            int tempCalendarIndex = -1;
                            do
                            {
                                //GetPrefferedWorkcenter(dtWorkCenter, ref TotalOrderQuantity, TempTotalOrderQty);

                                tempCalendarIndex++;
                                startDate = Convert.ToDateTime(Convert.ToDateTime(productionOrders.Rows[i]["LSD"].ToString()).ToString("dd-MMM-yyyy"));
                                //predict whether the last portion is less or equal to minimum line days
                                //because we the don't want to change the line

                                if (BestLine != null)
                                {
                                    dtCalendar = dicCalendar[BestLine["EntityId"].ToString()];

                                    dtCalendar.DefaultView.RowFilter = "WorkingDate>#" + Convert.ToDateTime(BestLine["LastProductionDate"].ToString()).ToString("dd-MMM-yyyy") + "#";
                                    if (dtCalendar.DefaultView.Count == 0)
                                    {
                                        throw new Exception("Production calendar does not support date after " + Convert.ToDateTime(BestLine["LastProductionDate"].ToString()).ToString("dd-MMM-yyyy"));
                                    }

                                }
                                else
                                {
                                    for (int ENT = 0; ENT < dvDistinctEntity.Rows.Count; ENT++)
                                    {
                                        dtCalendar = dicCalendar[dvDistinctEntity.Rows[ENT]["EntityId"].ToString()];

                                        dtCalendar.DefaultView.RowFilter = "WorkingDate>#" + startDate + "#";
                                        if (dtCalendar.DefaultView.Count == 0)
                                        {
                                            throw new Exception("Production calendar does not support date after " + startDate);
                                        }
                                    }
                                    BestLine = drBestLine(tempdate, productionOrders.Rows[i]["MaterialMasterId"].ToString(), dtWorkCenter, LastProductionLineID);
                                    dtCalendar = dicCalendar[BestLine["EntityId"].ToString()];
                                    dtCalendar.DefaultView.RowFilter = "WorkingDate>#" + startDate + "#";
                                }

                                double tempQty = TotalOrderQuantity;
                                int tempDayCount = 0;

                                //determining how many days to take to finish the production
                                while (tempQty > 0)
                                {
                                    tempDayCount++;
                                    try
                                    {
                                        tempQty = tempQty - getTarget(ref isBuildUpRequired, productionOrders.Rows[i], BestLine, tempDayCount, dtCalendar.DefaultView[tempCalendarIndex].Row, dicWorkCenterRunningHours, out double _STP, out double _AHP);// TargetPerDay;

                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("Production calendar does not support date after " + dtCalendar.DefaultView[tempCalendarIndex - 1]["ProductionDate"].ToString());
                                    }
                                }

                                if (tempDayCount <= MinimumLineDays)
                                {
                                    sbLog.AppendLine("Last block will run for [" + tempDayCount + "] which is less or equal to minimum workcenter days[" + MinimumLineDays + "], therefore no alter between lines");

                                    dtWorkCenter.DefaultView.RowFilter = "isnull(isResidualApplicable,0)=1 AND CURRENT_APPLICABLE='YES'";
                                    if (dtWorkCenter.DefaultView.Count > 0)
                                    {

                                        BestLine = dtWorkCenter.Select("WorkCenterMasterId='" + dtWorkCenter.DefaultView[0]["WorkCenterMasterId"].ToString() + "'")[0];
                                    }
                                    break;
                                }
                                //}

                                //else determine the best line for that production

                                BestLine = drBestLine(tempdate, productionOrders.Rows[i]["MaterialMasterId"].ToString(), dtWorkCenter, LastProductionLineID);
                                if (BestLine != null)
                                {
                                    //after allocating first line, we are resetting the production start time as LSD for each block of production
                                    if (Convert.ToDateTime(BestLine["LastProductionDate"].ToString()) <= LSD)
                                        startDate = LSD;
                                    break;
                                }

                                tempdate = tempdate.AddDays(1);
                            } while (tempdate < startDate);
                            #endregion Scan for each date starting from it's LSD to maximum block date to get the available best line to fit starting date

                            if (BestLine == null)
                            {
                                sbLog.AppendLine("No available best workcenter found!!!! ALLOCATION TERMINATED!!!");
                                break;
                            }


                            LastProductionLineID = BestLine["WorkcenterMasterID"].ToString();

                            if (strMaxAllocatedLines.Contains(LastProductionLineID) == false)
                                strMaxAllocatedLines.Add(LastProductionLineID);

                            //shift LSD to future date if best line's last production date is later on LSD
                            if (startDate <= Convert.ToDateTime(BestLine["LastProductionDate"].ToString()))
                                startDate = Convert.ToDateTime(BestLine["LastProductionDate"].ToString()).AddDays(1);


                            // DateTime LSDForLine = startDate;
                            dtCalendar.DefaultView.RowFilter = "WorkingDate>=#" + startDate.ToString("dd-MMM-yyyy") + "#";


                            if (productionOrders.Rows[i]["MaterialMasterId"].ToString() != BestLine["MaterialMasterId"].ToString())
                            {
                                BestLine["LastStyleRunningFor"] = "0";
                                isStyleChanged = true;
                            }
                            BestLine["MaterialMasterID"] = productionOrders.Rows[i]["MaterialMasterId"].ToString();

                        }


                        isBuildUpRequired = false;
                        ProductionBlock entry = new ProductionBlock();

                        try
                        {
                            LSDForLine = Convert.ToDateTime(dtCalendar.DefaultView[Index]["WorkingDate"].ToString());//there is no relationship but index number

                        }
                        catch (Exception ex)
                        {
                            string Error = string.Format("System cannot render calendar after {0} for production order#{1}",
                               LSDForLine.ToString("dd-MMM-yyyy"),
                                productionOrders.Rows[i]["ProductionOrderID"].ToString()).ToString();
                            throw new Exception(Error);
                        }

                        entry.ProductionDate = LSDForLine;
                        entry.WorkCenterMasterId = BestLine["WorkCenterMasterId"].ToString();
                        entry.MaterialMasterId = productionOrders.Rows[i]["MaterialMasterId"].ToString();
                        entry.EntityId = BestLine["EntityId"].ToString();//productionOrders.Rows[i]["EntityId"].ToString();
                        entry.ProcessID = BestLine["ProcessID"].ToString();
                        entry.ProductionOrderId = productionOrders.Rows[i]["ProductionOrderId"].ToString();

                        entry.ProductionHours = clsStaticInfo.dbl(dtCalendar.DefaultView[Index]["WorkingHours"].ToString()) + clsStaticInfo.dbl(dtCalendar.DefaultView[Index]["OTHours"].ToString());// clsStaticInfo.dbl(productionOrders.Rows[i]["PlanWorkingHoursPerDay"].ToString());
                        if (bplib.clsWebLib.GetBoolData(productionOrders.Rows[i]["ConsiderHourFromWorkCenter"].ToString()) == true)
                            entry.ProductionHours = clsStaticInfo.dbl(BestLine["StandardTimePerDay"].ToString());

                        entry.BlockNo = blockCount;
                        TargetPerDay = getTarget(ref isBuildUpRequired, productionOrders.Rows[i], BestLine, Index + 1, dtCalendar.DefaultView[Index].Row, dicWorkCenterRunningHours, out double StandardTargetPerDay, out double ActualHoursPerDay);//index+1=n'th day of production

                        entry.isBuildUp = isBuildUpRequired;
                        if (TotalOrderQuantity < TargetPerDay)
                        {
                            entry.Quantity = TotalOrderQuantity;
                        }
                        else
                        {
                            entry.Quantity = TargetPerDay;
                        }
                        //if (entry.ProductionOrderId == "20118" && entry.WorkCenterMasterId == "3")
                        //{

                        //}
                        //if (entry.ProductionOrderId == "201166")
                        //{

                        //}
                        if (strMaxAllocatedLines.Contains(entry.WorkCenterMasterId) == false)
                            strMaxAllocatedLines.Add(entry.WorkCenterMasterId);

                        entry.Quantity = Math.Round(AllocatedQty(ref LSDForLine, entry.WorkCenterMasterId, DicBalanceWorkcenterHours, ActualHoursPerDay, StandardTargetPerDay, entry.Quantity, entry.isBuildUp));

                        if (productionOrders.Rows[i]["MaterialMasterId"].ToString() != BestLine["MaterialMasterId"].ToString())
                        {
                            sbLog.AppendLine("Style changed");
                            entry.isStyleChange = true;

                        }
                        entry.isStyleChange = isStyleChanged;
                        _ProductionBlock.Add(entry);


                        BestLine["LastProductionDate"] = LSDForLine.ToString("dd-MMM-yyyy");
                        DataRow[] drSameLine = dtWorkCenter.Select("WorkCenterMasterId='" + BestLine["WorkCenterMasterId"].ToString() + "'");
                        foreach (DataRow drTempSameLine in drSameLine)
                        {
                            drTempSameLine["LastProductionDate"] = LSDForLine.ToString("dd-MMM-yyyy");
                        }

                        TotalOrderQuantity = TotalOrderQuantity - entry.Quantity;
                        TempTotalOrderQty = TempTotalOrderQty - entry.Quantity;
                        BestLine["AlreadyBooked"] = clsStaticInfo.dbl(BestLine["AlreadyBooked"].ToString()) + entry.Quantity;



                        BestLine["LastStyleRunningFor"] = clsStaticInfo.dbl(BestLine["LastStyleRunningFor"].ToString()) + 1;

                        dayCount++;


                        //taking only number of workcenters based on AllocatedLines
                        if (productionOrders.Rows[i]["ProductionStatusName"].ToString().ToUpper() != PlanningStatus.RUNNING.ToString())
                        {
                            if (strMaxAllocatedLines.Count == AllocatedLines)
                            {
                                for (int w = 0; w < dtWorkCenter.Rows.Count; w++)
                                {
                                    dtWorkCenter.Rows[w]["CURRENT_APPLICABLE"] = "NO";
                                    dtWorkCenter.Rows[w]["ACTUAL_APPLICABLE"] = "NO";
                                    if (strMaxAllocatedLines.Contains(dtWorkCenter.Rows[w]["WorkCenterMasterId"].ToString()))
                                    {
                                        dtWorkCenter.Rows[w]["ACTUAL_APPLICABLE"] = "YES";
                                        dtWorkCenter.Rows[w]["CURRENT_APPLICABLE"] = "YES";
                                    }
                                }
                            }
                        }

                        if (TotalOrderQuantity <= 0)
                        {
                            GetPrefferedWorkcenter(dtWorkCenter, ref TotalOrderQuantity, TempTotalOrderQty);
                            dayCount = 0;
                            BestLine = null;
                        }




                    }

                    sbLog.AppendLine("End of plotting block#" + blockCount);
                    foreach (DataRow item in dtWorkCenter.Rows)
                    {
                        item["CURRENT_APPLICABLE"] = "NO";
                        item["ACTUAL_APPLICABLE"] = "NO";
                        item["CurrentPRQty"] = "0";
                        item["AlreadyBooked"] = "0";
                        item["isResidualApplicable"] = false;

                    }

                    //saving final PR data
                    saveProductionPlanType2(_ProductionBlock, productionOrders.Rows[i]["ProductionOrderID"].ToString(), entityid, processid);
                }
                SendNotification("Distributing production quantity in sales orders and calculating expected completion date");
                Library.OrderManagement.Production.ExpectedSOWiseDateService expectedSO = new Library.OrderManagement.Production.ExpectedSOWiseDateService();
                expectedSO.Type2ExpectedSOWiseProductionCompletionSave(entityid);

                SendNotification("Simulation Completed");
                _lock.UnlockProcess();
            }
            catch (Exception ex)
            {
                SendNotification(ex.ToString());
                _lock.UnlockProcess();
                string x = ex.Message;
                throw (ex);
            }
            finally
            {

            }

        }

        private DataTable dtProductionType2Parameters(string entityid)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
            connection.BeginTransaction();
            connection.executeQuery(@"update  trn.ProductionOrderType2  SET Qty = k.OrderQty,PlannedQty =k.PlannedQty
                                        FROM trn.ProductionOrderType2 AS po
                                        INNER JOIN (
                                        select pod.ProductionOrderId,
                                        SUM(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty,
                                        --SUM(CEILING((so.Qty*(1+(moi.ExtraOrderPercentage/100)))*(1+(moi.OrderWastagePercentage/100)))) AS PlannedQty,
                                                                    sum(SO.Qty) AS OrderQty 

					                                        from trn.ProductionOrderType2Detail POD 
                                                                    left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                                                                    left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                                    INNER JOIN hkp.OrderStatus AS os ON os.Id=so.OrderStatusId
                            
                                                            WHERE os.Id='" + Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"'
                                        GROUP BY pod.ProductionOrderId
                                    ) AS K ON k.ProductionOrderId=po.Id");
            connection.CommitTransaction();

            connection = new ConnectionManager.clsConnection();
            connection.BeginTransaction();
            connection.executeQuery(@"delete FROM ProductionPlanningType2 WHERE ProductionOrderID IN (
                                              SELECT T2.Id FROM ProductionOrderSchedulingParametersType2 AS T2 
                                INNER JOIN trn.ProductionOrderType2 AS po ON T2.ProductionOrderID=po.Id
                                INNER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId
                                WHERE ps.UserName NOT IN ('" + PlanningStatus.ACTIVE.ToString() + @"','" + PlanningStatus.RUNNING.ToString() + @"')
                                )");
            connection.CommitTransaction();

            string sql = @"SELECT  PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,mm.MaterialMasterId,
                            --ISNULL(PO.PlannedQty,0)-ISNULL(PRODPR.ProductionQtyAtPR,0) AS SOQuantity,ISNULL(PRODPR.ProductionQtyAtPR,0) AS ProductionQty,
                            --ISNULL(CASE WHEN ISNULL(T1.Qty,0)>0 THEN T1.Qty ELSE PO.PlannedQty END,0)-(ISNULL(PRODPR.ProductionQtyAtPR,0)-ISNULL(PRDQ.ProductionBookedQty,0)) AS SOQuantity
                            T1.Qty SOQuantity
                            ,ISNULL(PRODPR.ProductionQtyAtPR,0) AS ProductionQty,
                           t1.*
                                                            FROM [TRN].[ProductionOrderType2] AS PO
                                                        JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                                                        INNER JOIN (
														SELECT DISTINCT pod.ProductionOrderId,mm.Id AS MaterialMasterId
FROM trn.ProductionOrderType2Detail AS pod
INNER JOIN(SELECT distinct pod.ProductionOrderId,(select top(1) SalesOrderId from trn.ProductionOrderType2Detail where ProductionOrderId=POD.ProductionOrderId) SOId
FROM trn.ProductionOrderType2Detail POD) A ON A.ProductionOrderId=pod.ProductionOrderId
INNER JOIN trn.SalesOrder AS so ON A.SOId=so.Id
INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
INNER JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId
							) AS MM ON mm.ProductionOrderId=po.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.ProductionStatusId = PS.Id
                            INNER JOIN ProductionOrderSchedulingParametersType2 t1 ON t1.ProductionOrderID=po.Id
                            LEFT OUTER  JOIN (SELECT pod.ProductionOrderId,SUM(so.Qty) AS Qty
                                                FROM trn.SalesOrder AS so
                            INNER JOIN trn.ProductionOrderType2Detail AS pod ON pod.SalesOrderId=so.Id GROUP BY pod.ProductionOrderId) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                           
							--production at PR Level
							LEFT OUTER JOIN (
												SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
											FROM  trn.ProductionSummary S 
											WHERE  CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PRODPR ON  PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
							
                            left outer join (SELECT pod.ProductionOrderId,
                                sum(isnull(so.ProductionBookedQty,0)) ProductionBookedQty
                                 FROM trn.SalesOrder AS so
                                INNER JOIN trn.ProductionOrderType2Detail AS pod ON pod.SalesOrderId=so.Id

                                GROUP BY pod.ProductionOrderId
                            ) AS PRDQ ON PRDQ.ProductionOrderId=T1.ProductionOrderId
							

                            WHERE 
                            po.EntityId IN(" + entityid + @")  AND ps.UserName IN ('" + PlanningStatus.ACTIVE.ToString() + @"','" + PlanningStatus.RUNNING.ToString() + @"')
                            ORDER BY ps.UserName DESC, t1.ProductionPriority ASC";
            DataTable _dtProductionParameters = _sqlRepository.GetDataTable(sql);

            return _dtProductionParameters;
        }

        private DataTable dtType2AvailableWrokcenters(string productionOrderID, string ProductionStatusName, string processid)
        {
            //for running
            string sql = @"SELECT DISTINCT  WS.*,convert(bit,0) as isResidualApplicable FROM ProductionPlanningType2 AS w
                        INNER JOIN [SCS].[WorkCenterMaster] WS ON ws.Id=w.WorkCenterMasterId
                       INNER JOIN trn.FreezeConfigPlanningType2 AS F ON f.EntityId=w.EntityID AND f.FreezeDate 
                       BETWEEN (SELECT MIN(ProductionDate) FROM ProductionPlanningType2 WHERE ProductionOrderId='" + productionOrderID + @"') 
                       AND (SELECT MAX(ProductionDate) FROM ProductionPlanningType2 WHERE ProductionOrderId='" + productionOrderID + @"') 
                       WHERE W.ProductionOrderId='" + productionOrderID + @"' AND WS.ProcessID='" + processid + @"'";

            DataTable dtWorkCenter = _sqlRepository.GetDataTable(sql);

            //freeze
            if (dtWorkCenter.Rows.Count == 0)
            {

                sql = @"SELECT ws.*,isnull(W.isResidualApplicable,0) AS isResidualApplicable FROM [TRN].[RunningOrderType2WorkCenter] W
                        INNER JOIN [SCS].[WorkCenterMaster] WS ON ws.Id=w.WorkCenterMasterId
                        INNER JOIN ProductionOrderSchedulingParametersType2 AS T ON t.ProductionOrderID=w.ProductionOrderId
                        INNER JOIN trn.ProductionOrder po ON po.Id=t.ProductionOrderID
                        LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                WHERE  ps.UserName='" + PlanningStatus.RUNNING.ToString() + @"' AND W.ProductionOrderId='" + productionOrderID + "' AND WS.ProcessID='" + processid + "'";

                dtWorkCenter = _sqlRepository.GetDataTable(sql);
            }

            if (dtWorkCenter.Rows.Count == 0)
            {
                //excluded WC
                DataTable dsExcludeWorkCenter = null;
                sql = @"SELECT ws.*,convert(bit,0) as isResidualApplicable FROM [TRN].[ProductionOrderType2WorkCenter] W
                        INNER JOIN [SCS].[WorkCenterMaster] WS ON ws.Id=w.WorkCenterMasterId
                        INNER JOIN ProductionOrderSchedulingParametersType2 AS t ON t.ID=w.ProductionOrderId
                WHERE t.WCPreferenceType='EXCLUDE' AND W.ProductionOrderId='" + productionOrderID + "' AND WS.ProcessID='" + processid + "'";
                dsExcludeWorkCenter = _sqlRepository.GetDataTable(sql);
                if (dsExcludeWorkCenter.Rows.Count > 0)
                {
                    sql = @"SELECT WC.*,convert(bit,0) as isResidualApplicable FROM  [SCS].[WorkCenterMaster] WC 
                                WHERE wc.[Active]=1 and  WC.ProcessId ='" + processid + @"' AND WC.EntityId IN (
                               SELECT DISTINCT d.EntityId FROM [TRN].ProductionOrderType2 D  
INNER JOIN ProductionOrderSchedulingParametersType2 AS t ON t.ProductionOrderID=D.Id                              
                                WHERE t.Id='" + productionOrderID + @"'
                                ) AND WC.Id NOT IN (SELECT ws.Id FROM [TRN].[ProductionOrderType2WorkCenter] W
                        INNER JOIN [SCS].[WorkCenterMaster] WS ON ws.Id=w.WorkCenterMasterId
                        INNER JOIN ProductionOrderSchedulingParametersType2 AS t ON t.ID=w.ProductionOrderId
                WHERE t.WCPreferenceType='EXCLUDE' AND W.ProductionOrderId='" + productionOrderID + "' AND WS.ProcessID='" + processid + @"')
                                ";
                    dtWorkCenter = _sqlRepository.GetDataTable(sql);
                }
                else
                {
                    if (dtWorkCenter.Rows.Count == 0)
                    {
                        sql = @"SELECT ws.*,convert(bit,0) as isResidualApplicable FROM [TRN].[ProductionOrderType2WorkCenter] W
                        INNER JOIN [SCS].[WorkCenterMaster] WS ON ws.Id=w.WorkCenterMasterId
                        INNER JOIN ProductionOrderSchedulingParametersType2 AS t ON t.ID=w.ProductionOrderId
                WHERE t.WCPreferenceType='INCLUDE' AND W.ProductionOrderId='" + productionOrderID + "' AND WS.ProcessID='" + processid + "'";
                        dtWorkCenter = _sqlRepository.GetDataTable(sql);
                    }

                    if (dtWorkCenter.Rows.Count == 0)
                    {
                        sbLog.AppendLine("No workcenter preference was defined in production order\r\nSearching in product preference...");
                        sql = @"SELECT WC.*,convert(bit,0) as isResidualApplicable FROM [SCS].[WorkCenterMasterProductPriority] WP 
                                INNER JOIN [SCS].[WorkCenterMaster] WC ON wc.Id=wp.WorkCenterMasterId

                                WHERE WP.ProductMasterId IN (
                                SELECT DISTINCT pd.ProductMasterId FROM [TRN].[ProductionOrderType2Detail] D
                                INNER JOIN trn.SalesOrder AS so ON so.Id=d.SalesOrderId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                INNER JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId

                                WHERE d.ProductionOrderId='" + productionOrderID + @"'
                                ) AND WC.ProcessID='" + processid + @"' AND WC.EntityId IN (
                                SELECT DISTINCT d.EntityId FROM [TRN].ProductionOrder D
                                WHERE d.Id='" + productionOrderID + @"'
                                ) 
                                ORDER BY WP.Priority ASC";
                        dtWorkCenter = _sqlRepository.GetDataTable(sql);

                    }
                    else
                    {
                        for (int i = 0; i < dtWorkCenter.Rows.Count; i++)
                            sbLog.AppendLine("workcenter preference found at production order [" + dtWorkCenter.Rows[i]["username"].ToString() + "]");

                    }
                    if (dtWorkCenter.Rows.Count == 0)
                    {
                        sbLog.AppendLine("No workcenter preference was defined in product configuration");
                        sql = @"SELECT WC.*,convert(bit,0) as isResidualApplicable FROM  [SCS].[WorkCenterMaster] WC 

                                WHERE wc.[Active]=1 and WC.ProcessId ='" + processid + @"' AND WC.EntityId IN (
                                SELECT DISTINCT d.EntityId FROM [TRN].ProductionOrder D
                               
                                WHERE d.Id='" + productionOrderID + @"'
                                ) 
                                ";
                        dtWorkCenter = _sqlRepository.GetDataTable(sql);

                    }
                    else
                    {
                        for (int i = 0; i < dtWorkCenter.Rows.Count; i++)
                            sbLog.AppendLine("workcenter preference found at product configuration [" + dtWorkCenter.Rows[i]["username"].ToString() + "]");


                    }
                }
            }
            //final block for all workcenters with last production date
            string workcenterlist = "''";
            for (int i = 0; i < dtWorkCenter.Rows.Count; i++)
                workcenterlist += ",'" + dtWorkCenter.Rows[i]["ID"].ToString() + "'";

            if (ProductionStatusName.ToUpper() == PlanningStatus.RUNNING.ToString().ToUpper())
            {
                sql = @"SELECT WC.Id AS WorkCenterMasterId, p.MaterialMasterId,WC.ProcessID,isnull(RWC.isResidualApplicable,0) AS isResidualApplicable,
                    isnull(RWC.Qty,0) AS CurrentPRQty,
                    ISNULL(prd.Quantity,0) AlreadyBooked,
                    FORMAT(ISNULL(p.ProductionDate,dateadd(DAY,-1, CONVERT(DATE, CASE WHEN ISNULL(e.StartDate,'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"')<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' THEN '" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' ELSE e.StartDate END))),'dd-MMM-yyyy') AS LastProductionDate,0 AS LastStyleRunningFor,
                           p.Quantity--, p.ProductionHours
                      FROM [SCS].[WorkCenterMaster] WC 
                        left outer join trn.RunningOrderWorkCenter RWC on RWC.WorkCenterMasterId=WC.ID and RWC.ProductionOrderId='" + productionOrderID + @"'
                         left outer join (SELECT t.WorkCenterMasterId,t.ProductionOrderId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t
                                         WHERE t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
                                         GROUP BY  t.WorkCenterMasterId,t.ProductionOrderId ) AS PRD oN prd.ProductionOrderId=RWC.ProductionOrderId and PRD.WorkCenterMasterId=RWC.WorkCenterMasterId       

INNER JOIN (SELECT WorkCenterMasterId,MAX(StartDate) AS StartDate FROM [SCS].[WorkCenterMasterEffectiveDate] GROUP BY WorkCenterMasterId) E ON e.WorkCenterMasterId=WC.Id
                    LEFT OUTER JOIN 
                    (
		                 SELECT  * FROM ( SELECT dense_rank() OVER (PARTITION BY t.WorkCenterMasterId ORDER BY t.ProductionDate DESC) AS RANK,
					                    t.WorkCenterMasterId,t.ProductionDate,t.Quantity,t.MaterialMasterId
					                    FROM (SELECT t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t 
					                    INNER JOIN [TRN].[ProductionOrderType2] P ON p.Id=t.ProductionOrderID
					                    LEFT OUTER JOIN (SELECT DISTINCT pod.ProductionOrderId,mm.Id AS MaterialMasterId
					                                       FROM trn.ProductionOrderType2Detail AS pod
														INNER JOIN trn.SalesOrder AS so ON pod.SalesOrderId=so.Id
														INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
														INNER JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId) AS K ON k.ProductionOrderId=p.Id
                                        where  t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'					                   
                                        GROUP BY t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId) AS T
		                    ) AS K WHERE K.[RANK]=1
                    ) AS P ON p.WorkCenterMasterId=wc.Id
                    WHERE WC.[Active]=1 AND WC.Id IN (" + workcenterlist + ")";
                dtWorkCenter = _sqlRepository.GetDataTable(sql);
            }
            else
            {
                sql = @"SELECT WC.Id AS WorkCenterMasterId, p.MaterialMasterId,WC.ProcessID,convert(bit,0) AS isResidualApplicable,
                    0 AS CurrentPRQty,
                    ISNULL(prd.Quantity,0) AlreadyBooked,
                    FORMAT(ISNULL(p.ProductionDate,dateadd(DAY,-1, CONVERT(DATE, CASE WHEN ISNULL(e.StartDate,'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"')<='" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' THEN '" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"' ELSE e.StartDate END))),'dd-MMM-yyyy') AS LastProductionDate,0 AS LastStyleRunningFor,
                           p.Quantity--, p.ProductionHours
                      FROM [SCS].[WorkCenterMaster] WC 
                        left outer join trn.ProductionOrderType2WorkCenter RWC on RWC.WorkCenterMasterId=WC.ID and RWC.ProductionOrderId='" + productionOrderID + @"'
                         left outer join (SELECT t.WorkCenterMasterId,t.ProductionOrderId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t
                                         WHERE t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
                                         GROUP BY  t.WorkCenterMasterId,t.ProductionOrderId ) AS PRD oN prd.ProductionOrderId=RWC.ProductionOrderId and PRD.WorkCenterMasterId=RWC.WorkCenterMasterId       

                            INNER JOIN (SELECT WorkCenterMasterId,MAX(StartDate) AS StartDate FROM [SCS].[WorkCenterMasterEffectiveDate] GROUP BY WorkCenterMasterId) E ON e.WorkCenterMasterId=WC.Id
                    LEFT OUTER JOIN 
                    (
		                 SELECT  * FROM ( SELECT dense_rank() OVER (PARTITION BY t.WorkCenterMasterId ORDER BY t.ProductionDate DESC) AS RANK,
					                    t.WorkCenterMasterId,t.ProductionDate,t.Quantity,t.MaterialMasterId
					                    FROM (SELECT t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t 
					                    INNER JOIN [TRN].[ProductionOrderType2] P ON p.Id=t.ProductionOrderID
					                    LEFT OUTER JOIN (SELECT DISTINCT pod.ProductionOrderId,mm.Id AS MaterialMasterId
					                                       FROM trn.ProductionOrderType2Detail AS pod
														INNER JOIN trn.SalesOrder AS so ON pod.SalesOrderId=so.Id
														INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
														INNER JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId) AS K ON k.ProductionOrderId=p.Id
                                        where  t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'					                   
                                        GROUP BY t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId) AS T
		                    ) AS K WHERE K.[RANK]=1
                    ) AS P ON p.WorkCenterMasterId=wc.Id
                    WHERE WC.[Active]=1 AND WC.Id IN (" + workcenterlist + ")";
                dtWorkCenter = _sqlRepository.GetDataTable(sql);

            }
            string sqlLastRunningDays = @"SELECT *
                    FROM ( SELECT dense_rank() OVER (PARTITION BY t.WorkCenterMasterId ORDER BY t.ProductionDate DESC) AS LW,
					                    t.WorkCenterMasterId,t.ProductionDate,t.Quantity, t.MaterialMasterId
                           from (SELECT t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId,SUM(t.Quantity) AS Quantity
					                      FROM trn.ProductionSummary t 
					                    INNER JOIN [TRN].[ProductionOrderType2] P ON p.Id=t.ProductionOrderID
					                    LEFT OUTER JOIN (SELECT DISTINCT pod.ProductionOrderId,mm.Id AS MaterialMasterId
					                                       FROM trn.ProductionOrderType2Detail AS pod
														INNER JOIN trn.SalesOrder AS so ON pod.SalesOrderId=so.Id
														INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
														INNER JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId) AS K ON k.ProductionOrderId=p.Id
                                        WHERE p.Id='" + productionOrderID + @"' AND t.ProductionDate<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
					                    GROUP BY t.WorkCenterMasterId,t.ProductionDate,k.MaterialMasterId) AS T
                        ) AS T  
                    WHERE LW<=(SELECT MAX(T1.DayToReachTheTarget)
                   FROM ProductionOrderSchedulingParametersType2 AS T1 
                  INNER JOIN trn.ProductionOrderType2 AS po ON t1.ProductionOrderID=po.Id WHERE po.Id='" + productionOrderID + @"' )";


            DataTable dtWorkCenterProductionHistory = _sqlRepository.GetDataTable(sqlLastRunningDays);
            DataView dvtemp = new DataView(dtWorkCenterProductionHistory);

            Dictionary<string, int> distinctWorkCenter = new Dictionary<string, int>();
            for (int i = 0; i < dtWorkCenterProductionHistory.Rows.Count; i++)
            {
                if (distinctWorkCenter.ContainsKey(dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString()) == false)
                {
                    dvtemp.RowFilter = "WorkCenterMasterId='" + dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() + "'";
                    string materialmasterid = dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString();
                    int ReverseCountDays = 0;
                    for (int R = 0; R < dvtemp.Count; R++)
                    {
                        if (materialmasterid == dtWorkCenterProductionHistory.Rows[i]["MaterialMasterId"].ToString())
                            ReverseCountDays++;
                        else
                            break;
                    }


                    dtWorkCenter.DefaultView.RowFilter = "WorkCenterMasterId='" + dtWorkCenterProductionHistory.Rows[i]["WorkCenterMasterId"].ToString() + "'";
                    if (dtWorkCenter.DefaultView.Count > 0)
                        dtWorkCenter.DefaultView[0].Row["LastStyleRunningFor"] = ReverseCountDays;

                }

            }

            return dtWorkCenter;
        }

        private void saveProductionPlanType2(List<ProductionBlock> entry, string productionOrderID, string entityid, string processid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataSet dsMaster;
            string sql = @"SELECT * FROM ProductionPlanningType2 t1 WHERE EntityID='" + entityid + "' AND t1.ProductionOrderID ='" + productionOrderID + "' AND ProcessID='" + processid + "'";
            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

            while (dsMaster.Tables[0].DefaultView.Count > 0)
            {
                dsMaster.Tables[0].DefaultView[0].Delete();
            }


            DataRow dr;

            for (int i = 0; i < entry.Count; i++)
            {

                dr = dsMaster.Tables[0].NewRow();

                dr["ProductionOrderID"] = entry[i].ProductionOrderId;
                dr["WorkCenterMasterId"] = entry[i].WorkCenterMasterId;
                dr["MaterialMasterId"] = entry[i].MaterialMasterId;
                dr["EntityId"] = entry[i].EntityId;
                dr["ProcessID"] = entry[i].ProcessID;
                dr["ProductionDate"] = entry[i].ProductionDate;
                dr["Quantity"] = entry[i].Quantity;
                dr["ProductionHours"] = entry[i].ProductionHours;
                dr["isBuildUp"] = entry[i].isBuildUp;
                dr["isStyleChange"] = entry[i].isStyleChange;
                dr["BlockNo"] = entry[i].BlockNo;

                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = System.DateTime.Now.ToString();
                dr["AddedFromIP"] = identity.IPAddress;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;

                dsMaster.Tables[0].Rows.Add(dr);

            }



            clsStaticInfo clsStatic = new clsStaticInfo();
            clsStatic.SaveDataSets(dsMaster);
        }


        #endregion

    }

    public class PriorityUp
    {

        public string ProductionId { get; set; }
        public string Status { get; set; }
        public string ProductionPriority { get; set; }

    }
    public class RunningOrderWorkCenter
    {
        #region Scalar Properties

        public string Id { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        ///
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        ///
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        ///
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

        #region Navigation Property

        public ProductionOrder ProductionOrder { get; set; }
        public string ProductionOrderId { get; set; }
        public string WorkCenterMasterId { get; set; }

        #endregion Navigation Property
        public bool? isResidualApplicable { get; set; }
        public double Qty { get; set; }
    }
    public class ProductionPlanningSnapshotMasterType1 : BaseModel
    {
        public string ID { get; set; } = "";
        public string EntityID { get; set; } = "";
        public string ProcessID { get; set; } = "";
        public string SnapshotName { get; set; } = "";
        public string SnapshotDesc { get; set; } = "";

    }
    public class ProductionPlanningSnapshot2MasterType1 : BaseModel
    {
        public string ID { get; set; } = "";
        public string EntityID { get; set; } = "";
        public string ProcessID { get; set; } = "";
        public string SnapshotName { get; set; } = "";
        public string SnapshotDesc { get; set; } = "";
        public string SnapshotTakenBy { get; set; } = "";

    }

    public class ProductionBlock
    {
        public string WorkCenterMasterId { get; set; } = "";
        public string EntityId { get; set; } = "";
        public string ProcessID { get; set; } = "";

        public string MaterialMasterId { get; set; } = "";
        public string ProductionOrderId { get; set; } = "";
        public DateTime ProductionDate = System.DateTime.Now;
        public bool isBuildUp = false;
        public bool isStyleChange = false;
        public double Quantity { get; set; } = 0;
        public double ProductionHours { get; set; } = 0;

        public int BlockNo { get; set; } = 0;

    }
    public class GroupData
    {
        public string text { get; set; } = "";

        public string id { get; set; } = "";
        public string groupId { get; set; } = "1";
        public string color { get; set; } = "ffaa00";

    }
    public class AvailableLines
    {
        public string WorkCenterMasterId { get; set; } = "";
        public string MaterialMasterId { get; set; } = "";
        public string ProductionOrderId { get; set; } = "";
        public DateTime LastProductionDate { get; set; } = System.DateTime.Now;
        public string ProductionStatus { get; set; } = "New";
        public double RemainingQuantity { get; set; } = 0;

        public double ProductionHours { get; set; } = 0;
        public DateTime ProductionDate { get; set; } = System.DateTime.Now;


    }

    public class ProductionOrderSchedulingParametersType1 : BaseModel
    {
        public string ID { get; set; } = "";
        public string ProductionOrderID { get; set; } = null;
        public double NoOfWorkStation { get; set; } = 0;
        public double Efficiency { get; set; } = 0;
        public double SPT { get; set; } = 0;
        public double PlanWorkingHoursPerDay { get; set; } = 0;
        public double FirstDayOutPut { get; set; } = 0;
        public double RunningOrderBlockSize { get; set; } = 1;
        // public double PlanTargetPerHour { get; set; } = 0;
        public double IncrementValue { get; set; } = 0;
        public string IncrementType { get; set; } = "";
        public double DayToReachTheTarget { get; set; } = 0;
        public string LSD { get; set; } = "";
        public string WCPreferenceType { get; set; } = "INCLUDE";
        //public string PlanningStatus { get; set; } = "TOSTART";
        public string CommitmentDate { get; set; } = "";
        public string MainRawMaterialInhouseDate { get; set; } = "";
        public string OtherRawMaterialInhouseDate { get; set; } = "";
        public double ProductionPriority { get; set; } = 0;
        public double TargetPerHour { get; set; } = 0;
        public double TargetPerDay { get; set; } = 0;
        public double MinimumLineDays { get; set; } = 0;
        public double RequiredLineDays { get; set; } = 0;
        public double RequiredNoOfLines { get; set; } = 0;
        public double AllocatedLines { get; set; } = 0;
        public string Color { get; set; } = "";
        public double Qty { get; set; } = 0;
        public bool ConsiderHourFromWorkCenter { get; set; } = false;
        public bool ConsiderWorkStationsFromWorkCenter { get; set; } = false;
    }
}