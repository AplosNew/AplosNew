using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Security.Core;
using Library.Service.Employees;
using Library.Service.Helpers;
using Syncfusion.Presentation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;


namespace Aplos.Areas.Materials.Controllers
{
    public class LOTCreationController : Controller
    {
        private readonly SqlRepository _sqlRepository;
        public LOTCreationController()
        {
            _sqlRepository = new SqlRepository();
        }
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GetProductionOrderDataList()
        {
           
            string CmdText = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
								  ,PlannedQty=CASE WHEN PQ.Qty=0 THEN PO.PlannedQty ELSE PO.PlannedQty END
                            --,((CASE WHEN PQ.Qty=0 THEN PO.PlannedQty ELSE PO.PlannedQty END)-ISNULL(CEILING(PRS.TotalProductionQty),0)) RemainingQty
                             ,ISNULL((CASE WHEN ISNULL(PPS.Qty,0)=0 THEN ISNULL(PQ.Qty,PO.PlannedQty) ELSE PO.PlannedQty*PPS.Qty/100 END)-ISNULL(CEILING(PRS.TotalProductionQty), 0),0) RemainingQty
                            , ISNULL(CEILING(PRS.TotalProductionQty),0)TotalProductionQty
									,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                ,Material=STUFF((select distinct ','+MM.UserName from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN trn.MasterOrderItem moi ON moi.Id = XSO.MasterOrderItemId
						                                         LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                             ,Article=STUFF((select distinct ','+MMA.StandardName  from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN trn.MasterOrderItem moi ON moi.Id = XSO.MasterOrderItemId
						                                         LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
																 LEFT JOIN MST.MaterialMasterArticle AS mma on mma.MaterialMasterId=MM.Id
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                   PRS.LotNumber
                                   --,PRS.ResponsiblePerson
								   FROM TRN.ProductionOrder PO 
                                   LEFT JOIN TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderID = PO.Id
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID=PO.Id
								  LEFT JOIN 
								  (    SELECT SUM(PS.Quantity) TotalProductionQty,PS.ProductionOrderId,PS.LotNumber
                                       --,(select EmployeeName from EmployeeInformation where SystemId=PS.ResponsiblePersonId) as ResponsiblePerson
                                       FROM [TRN].[ProductionSummary] PS  GROUP BY PS.ProductionOrderId,PS.LotNumber
                                       --,PS.ResponsiblePersonId
                                  ) AS PRS ON PRS.ProductionOrderId = PO.Id
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory--,SO.Qty
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								   WHERE PS.UserName IN('Running','To Close') Order by PD.Description,PD.BuyerOrder";

           
            return Json(_sqlRepository.GetDataCollection(CmdText), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPO()
        {
            string poSql = @"select PO.Id Value, PO.Id Text from TRN.ProductionOrder  PO
                            left join HKP.ProductionStatus PS on PS.Id = PO.ProductionStatusId
                            where PS.UserName in ('Running', 'To Close')";
            return Json(_sqlRepository.GetDataCollection(poSql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetArticle(string poid)
        {
            string sql = @"select MMA.Id Value, MMA.StandardName Text from MST.MaterialMasterArticle MMA
                            left join TRN.MasterOrderItem MSI on MSI.ArticleId = MMA.Id
                            left join TRN.SalesOrder SO on SO.MasterOrderItemId = MSI.Id
                            left join TRN.ProductionOrderDetail POD on POD.SalesOrderId = SO.Id
                            left join TRN.ProductionOrder PO on PO.Id = POD.ProductionOrderId
                            where PO.Id = '" + poid + @"' 
                            order by StandardName";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetProductCode(string articleid)
        {
            string sql = @"
                    Select PL.Id Value,  PL.Code, ArticleId from ProductLibrary PL
                    where PL.ArticleId = '" + articleid + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProcessSet()
        {
            string qry = @"select P.Id ProcessId, P.UserName Process, PPS.Qty, PO.Id POId FROM TRN.ProductionOrder PO 
                            LEFT JOIN TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderID = PO.Id
                            left join HKP.Process P on P.Id = PPS.ProcessId
                            LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
                            where PS.UserName in ('Running', 'To Close') and PO.Id = 23628";
            return Json(_sqlRepository.GetDataCollection(qry), JsonRequestBehavior.AllowGet);
        }

        //public ActionResult Save(List<Dictionary<string, object>> datalist)
        //{

        //    string TableName = "LOTCreation";
        //    DataSet ds;
        //    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

        //    con.OpenDataSetThroughAdapter("select * from "+ TableName + " where");
        //}
    }
}