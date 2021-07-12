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
using Syncfusion.XlsIO;
using System.IO;
using Library.Service.Helpers;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionDashboardController : BaseController
    {
        #region Constructor
        /// <summary>   The CostingTypesService service. </summary>
        private readonly ISqlRepository _sqlRepository;
        public ProductionDashboardController(ISqlRepository R)
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
        public ActionResult GetProcessList()
        {
            string sql = new Library.OrderManagement.Production.WIPReport().GetAllProcessAndInventory();
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult ProductionEfficiencyReport(string PlantId, string entityid, string Date)
        {

            try
            {
                Library.Planning.OrderManagement.Bulletin bulletin = new Library.Planning.OrderManagement.Bulletin();
                bulletin.ProductionEfficiencyReport(PlantId, entityid, Date);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpGet, Authorize]
        public ActionResult GetAllCompaniesAndPlants()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.OrderManagement.Production.WIPReport repo = new Library.OrderManagement.Production.WIPReport();
            return Json(new
            {
                Plant = repo.GetAllPlants(),
                Company = repo.GetAllCompanies(),
                PlantId = identity.PlantId,
                CompanyId = identity.CompanyId,
                BaseProcessId = repo.GetType1ProcessId()
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProcessWiseProduction(string PlantId, string EntityId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            string sql = new Library.Planning.PlanningType1.ProductionDashboard().GetProcessWiseProduction(PlantId, EntityId, _date);
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProductionOrderWiseProduction(string PlantId, string EntityId, string ProcessId, DateTime date)
        {
            //.. GetProductionLisPRWiseDashboard
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();
            _wipReport.GetProductionLisPRWiseDashboard(PlantId, ProcessId, _date, out DataTable dt);

            if (string.IsNullOrEmpty(EntityId) == false && EntityId != "null")
            {
                dt.DefaultView.RowFilter = "EntityId='" + EntityId + @"'";
                dt = dt.DefaultView.ToTable();
            }

            dt.DefaultView.RowFilter = "isnull(InQuantityToday,0)>0 OR isnull(OutQuantityToday,0)>0 OR isnull(KillQuantityToday,0)>0";
            dt = dt.DefaultView.ToTable();

            return Json(Helpers.CustomJsonResult.DataTableToJson(dt), JsonRequestBehavior.AllowGet);


            //string sql = new Library.Planning.PlanningType1.ProductionDashboard().GetProductionOrderWiseProduction(EntityId, ProcessId, _date);
            //return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetWorkCenterWiseWIP(string PlantId, string EntityId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();
            _wipReport.GetProductionListWithoutWCRowProcessWiseDashboard(PlantId, ProcessId, _date, out DataTable dt);

            //DataTable data = new Library.Planning.PlanningType1.ProductionDashboard().GetWorkCenterWiseWIP(PlantId, EntityId, dt);
            DataTable data = new Library.Planning.PlanningType1.ProductionDashboard().GetWorkCenterWiseWIPForGraph(PlantId, EntityId, ProcessId, dt);

            return Json(Helpers.CustomJsonResult.DataTableToJson(data), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetWorkCenterWiseWIPForGraph(string PlantId, string EntityId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();
            _wipReport.GetProductionListWithoutWCRowProcessWiseDashboard(PlantId, ProcessId, _date, out DataTable dt);

            DataTable data = new Library.Planning.PlanningType1.ProductionDashboard().GetWorkCenterWiseWIPForGraph(PlantId, EntityId, ProcessId, dt);

            return Json(Helpers.CustomJsonResult.DataTableToJson(data), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetDailyPlanVsProduction(string PlantId, string EntityId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();

            var _DailyPlanVsProduction = _wipReport.GetDailyPlanVsProduction(PlantId, EntityId, _date);
            string _ProcessName = _wipReport.GetType1ProcessName();
            return Json(new { PlanVsProductionWC = _DailyPlanVsProduction, ProcessName = _ProcessName }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetDailyLast30DaysPlanVsProduction(string PlantId, string EntityId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();

            var _DailyLast30DaysPlanVsProduction = _wipReport.GetDailyLast30DaysPlanVsProduction(PlantId, EntityId, _date);
            return Json(new { PlanVsProduction30 = _DailyLast30DaysPlanVsProduction }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLastDaysPlanVsProductionStatistics(string PlantId, string EntityId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();

            var dayStatistics = _wipReport.GetLastDaysPlanVsProductionStatistics(PlantId, EntityId, _date);
            return Json(dayStatistics, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProfitability(string PlantId, string EntityId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.Planning.PlanningType1.ProductionDashboard _wipReport = new Library.Planning.PlanningType1.ProductionDashboard();
            string query = _wipReport.GetProfitability(PlantId, EntityId, _date);
            return Json(_sqlRepository.GetDataCollection(query), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProfitabilityReport(string PlantId, string EntityId, DateTime date)
        {
            try
            {
                string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
                Library.Planning.PlanningType1.ProductionDashboard _wipReport = new Library.Planning.PlanningType1.ProductionDashboard();
                _wipReport.ProfitabilityReport(PlantId, EntityId, _date);
                //string query = _wipReport.GetProfitability(PlantId, EntityId, _date);
                return null;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetPRWiseWIP(string PlantId, string EntityId, string ProcessId, string WorkCenterMasterId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();
            _wipReport.GetProductionListWithoutWCRowProcessWiseDashboard(PlantId, ProcessId, _date, out DataTable dt);



            DataTable data = new Library.Planning.PlanningType1.ProductionDashboard().GetPRWiseWIP(EntityId, WorkCenterMasterId, dt);

            return Json(Helpers.CustomJsonResult.DataTableToJson(data), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetInWC(string FDUD, string PlantId, string EntityId, string WorkCenterMasterId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetInWC(FDUD, EntityId, WorkCenterMasterId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOutWC(string FDUD, string PlantId, string EntityId, string WorkCenterMasterId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetOutWC(FDUD, EntityId, WorkCenterMasterId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetKillWC(string FDUD, string PlantId, string EntityId, string WorkCenterMasterId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetKillWC(FDUD, EntityId, WorkCenterMasterId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetInPO(string FDUD, string ProductionOrderId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetInPO(FDUD, ProductionOrderId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOutPO(string FDUD, string ProductionOrderId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetOutPO(FDUD, ProductionOrderId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetKillPO(string FDUD, string ProductionOrderId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetKillPO(FDUD, ProductionOrderId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetInWCPO(string FDUD, string ProductionOrderId, string WorkCenterMasterId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetInWCPO(FDUD, ProductionOrderId, WorkCenterMasterId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOutWCPO(string FDUD, string ProductionOrderId, string WorkCenterMasterId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetOutWCPO(FDUD, ProductionOrderId, WorkCenterMasterId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetKillWCPO(string FDUD, string ProductionOrderId, string WorkCenterMasterId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetKillWCPO(FDUD, ProductionOrderId, WorkCenterMasterId, _date), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetProductionRelay(string PlantId, string EntityId, string ProcessId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(EntityId) || EntityId.ToUpper() == "NULL")
            {
                string sql = @"SELECT convert(bit,case when  PLST.processId='" + ProcessId + @"' then 1 else 0 END) AS IsLastProcess,PSS.Remarks,
convert(bit,0) AS Checked, pss.Id PSSId,ppr.Id PPRId,NPR.Id NPRId ,convert(bit ,isnull(ppr.IsCompleted,0)) AS IsCompleted,P.UserName PreviousProcess
,CP.UserName CurrentProcess,NP.UserName NextProcess,PPR.CompletedBy ClosedBy
,Format(PPR.CompletionEntryDate,'dd-MMM-yyyy') ClosedDate ,Format(PPR.StartDate,'dd-MMM-yyyy') PreviousProcessStartDate
,  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
isnull(CurrentProcessPR.ProductionQtyAtPR,0) ProducedQty
,Variance=case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end-isnull(CurrentProcessPR.ProductionQtyAtPR,0),
ISNULL(PO.Qty,0) AS POQuantity,ISNULL(SO.PlannedQty,0) AS PlannedQty,ISNULL(SO.OrderQty,0) AS OrderQty,so.Material,
     so.ProductCategory,so.Product,
		ActualQTY=	case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end,							
                                Format(so.LastShipmentDate,'dd-MMM-yyyy') LastShipmentDate, so.article
								,Format(CurrentProcessPR.ProductionStartDateAtPR,'dd-MMM-yyyy') ProductionStartDateAtPR,
								Format(PSS.StartDate,'dd-MMM-yyyy') StartDate,Format(PSS.EndDate,'dd-MMM-yyyy') EndDate,Format(st.LSD,'dd-MMM-yyyy') LSD,
								PSS.CompletedBy ,
 isnull( PreviousProcessPR.ProductionQtyAtPR,0) PreviousProcessQty,
isnull (NextProcessPR.ProductionQtyAtPR,0) NextProcessQty,
CurrentProcessWIP=isnull(CurrentProcessPR.ProductionQtyAtPR,0)-isnull (NextProcessPR.ProductionQtyAtPR,0),
PreviousProcessWIP=isnull( PreviousProcessPR.ProductionQtyAtPR,0)-isnull(CurrentProcessPR.ProductionQtyAtPR,0),

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
							left outer join HKP.Process CP on CP.Id=PSS.ProcessId				

							left join trn.ProductionOrderProcessSet PLST ON PLST.ProductionOrderId=PO.Id and PLST.Id=(select top 1 Id from trn.ProductionOrderProcessSet XP where XP.ProductionOrderId=PO.Id order by XP.Sequence DESC)
							left join trn.ProductionOrderProcessSet PPR ON PSS.ProductionOrderId=PO.Id 
							and PPR.id=(select A.Id from (
							select DENSE_RANK() over (partition by p.ProductionOrderId order by P.[sequence] desc) AS RNK,P.*
							from TRN.ProductionOrderProcessSet P 							
							where p.ProductionOrderId=PSS.ProductionOrderId AND P.Sequence<PSS.sequence) AS A where a.RNK=1)
							left outer join HKP.Process P on p.Id=PPR.ProcessId

						    left join trn.ProductionOrderProcessSet NPR ON PSS.ProductionOrderId=PO.Id 
							and NPR.id=(select A.Id from (
							select DENSE_RANK() over (partition by p.ProductionOrderId order by P.[sequence] ASC) AS RNK,P.*
							from TRN.ProductionOrderProcessSet P 							
							where p.ProductionOrderId=PSS.ProductionOrderId AND P.Sequence>PSS.sequence) AS A where a.RNK=1)
							left outer join HKP.Process NP on NP.Id=NPR.ProcessId

                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDateAtPR,MAX(s.ProductionDate) AS ProductionEndDateAtPR
											FROM  trn.ProductionSummary S 
											where s.ProcessId='"+ProcessId+@"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS CurrentProcessPR ON  CurrentProcessPR.ProductionOrderId=po.id AND CurrentProcessPR.ProcessId=PSS.ProcessId
							
                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDate,MAX(s.ProductionDate) AS ProductionEndDate
											FROM  trn.ProductionSummary S											
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PreviousProcessPR ON  PreviousProcessPR.ProductionOrderId=po.id AND PreviousProcessPR.ProcessId=PPR.ProcessId
							 
							  LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDate,MAX(s.ProductionDate) AS ProductionEndDate
											FROM  trn.ProductionSummary S											
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS NextProcessPR ON  NextProcessPR.ProductionOrderId=po.id AND NextProcessPR.ProcessId=NPR.ProcessId
							 
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
                            AND ((ISNULL(ppr.Id,'')<>'' AND ISNULL(ppr.StartDate,'')<>'') OR ISNULL(ppr.Id,'')=''  OR ISNULL(ppr.IsCompleted,0)=1)
                             AND EN.PlantId ='" + PlantId + @"' AND PSS.ProcessId = '" + ProcessId + @"' and isnull(pss.IsCompleted,0)=0 
                            ORDER BY st.LSD";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

            }
            else
            {
                string sql = @"SELECT convert(bit,case when  PLST.processId='"+ProcessId+ @"' then 1 else 0 END) AS IsLastProcess,PSS.Remarks,
convert(bit,0) AS Checked, pss.Id PSSId,ppr.Id PPRId,NPR.Id NPRId ,convert(bit ,isnull(ppr.IsCompleted,0)) AS IsCompleted,P.UserName PreviousProcess
,CP.UserName CurrentProcess,NP.UserName NextProcess,PPR.CompletedBy ClosedBy
,Format(PPR.CompletionEntryDate,'dd-MMM-yyyy') ClosedDate ,Format(PPR.StartDate,'dd-MMM-yyyy') PreviousProcessStartDate
,  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
isnull(CurrentProcessPR.ProductionQtyAtPR,0) ProducedQty
,Variance=case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end-isnull(CurrentProcessPR.ProductionQtyAtPR,0),
ISNULL(PO.Qty,0) AS POQuantity,ISNULL(SO.PlannedQty,0) AS PlannedQty,ISNULL(SO.OrderQty,0) AS OrderQty,so.Material,
     so.ProductCategory,so.Product,
		ActualQTY=	case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end,							
                                Format(so.LastShipmentDate,'dd-MMM-yyyy') LastShipmentDate, so.article
								,Format(CurrentProcessPR.ProductionStartDateAtPR,'dd-MMM-yyyy') ProductionStartDateAtPR,
								Format(PSS.StartDate,'dd-MMM-yyyy') StartDate,Format(PSS.EndDate,'dd-MMM-yyyy') EndDate,Format(st.LSD,'dd-MMM-yyyy') LSD,
								PSS.CompletedBy ,
 isnull( PreviousProcessPR.ProductionQtyAtPR,0) PreviousProcessQty,
isnull (NextProcessPR.ProductionQtyAtPR,0) NextProcessQty,
CurrentProcessWIP=isnull(CurrentProcessPR.ProductionQtyAtPR,0)-isnull (NextProcessPR.ProductionQtyAtPR,0),
PreviousProcessWIP=isnull( PreviousProcessPR.ProductionQtyAtPR,0)-isnull(CurrentProcessPR.ProductionQtyAtPR,0),

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
							left outer join HKP.Process CP on CP.Id=PSS.ProcessId				

							left join trn.ProductionOrderProcessSet PLST ON PLST.ProductionOrderId=PO.Id and PLST.Id=(select top 1 Id from trn.ProductionOrderProcessSet XP where XP.ProductionOrderId=PO.Id order by XP.Sequence DESC)
							left join trn.ProductionOrderProcessSet PPR ON PSS.ProductionOrderId=PO.Id 
							and PPR.id=(select A.Id from (
							select DENSE_RANK() over (partition by p.ProductionOrderId order by P.[sequence] desc) AS RNK,P.*
							from TRN.ProductionOrderProcessSet P 							
							where p.ProductionOrderId=PSS.ProductionOrderId AND P.Sequence<PSS.sequence) AS A where a.RNK=1)
							left outer join HKP.Process P on p.Id=PPR.ProcessId

						    left join trn.ProductionOrderProcessSet NPR ON PSS.ProductionOrderId=PO.Id 
							and NPR.id=(select A.Id from (
							select DENSE_RANK() over (partition by p.ProductionOrderId order by P.[sequence] ASC) AS RNK,P.*
							from TRN.ProductionOrderProcessSet P 							
							where p.ProductionOrderId=PSS.ProductionOrderId AND P.Sequence>PSS.sequence) AS A where a.RNK=1)
							left outer join HKP.Process NP on NP.Id=NPR.ProcessId

                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDateAtPR,MAX(s.ProductionDate) AS ProductionEndDateAtPR
											FROM  trn.ProductionSummary S 
											where s.ProcessId='" + ProcessId + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS CurrentProcessPR ON  CurrentProcessPR.ProductionOrderId=po.id AND CurrentProcessPR.ProcessId=PSS.ProcessId
							
                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDate,MAX(s.ProductionDate) AS ProductionEndDate
											FROM  trn.ProductionSummary S											
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PreviousProcessPR ON  PreviousProcessPR.ProductionOrderId=po.id AND PreviousProcessPR.ProcessId=PPR.ProcessId
							 
							  LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDate,MAX(s.ProductionDate) AS ProductionEndDate
											FROM  trn.ProductionSummary S											
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS NextProcessPR ON  NextProcessPR.ProductionOrderId=po.id AND NextProcessPR.ProcessId=NPR.ProcessId
							 
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
                            AND ((ISNULL(ppr.Id,'')<>'' AND ISNULL(ppr.StartDate,'')<>'') OR ISNULL(ppr.Id,'')=''  OR ISNULL(ppr.IsCompleted,0)=1)
                             AND EN.PlantId ='" + PlantId + @"' AND  PO.entityid='" + EntityId + @"' and  PSS.ProcessId = '" + ProcessId + @"' and isnull(pss.IsCompleted,0)=0 
                            ORDER BY st.LSD
							";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

            }

        }

        [HttpGet, Authorize]
        public ActionResult GetProductionRelayReport(string PlantId, string EntityId, string ProcessId)
        {
            try
            {

                ProductionRelayReport(PlantId, EntityId, ProcessId);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void ProductionRelayReport(string PlantId, string EntityId, string ProcessId)
        {
            try
            {
                string sql = GetProductionRelayReportSQL(PlantId, EntityId, ProcessId);
                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Production Relay";

                DataTable dtPurchaseLc = _sqlRepository.GetDataTable(sql);

                int ROW = 6;
                int COL = 1;


                sheet[ROW, COL].Text = "Sl No.";
                sheet[ROW, COL].ColumnWidth = 5;
                int colSlNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Prd.Order ID";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPRId = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";

                sheet[ROW, COL].ColumnWidth = 15;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 15;
                int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 12;
                int colCurrentProcess = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Item#";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colBuyerItem = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Item#";
                sheet[ROW, COL].ColumnWidth = 15;
                int colOwnItem = COL;
                COL++;
                sheet[ROW, COL].Text = "LSD";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLSD = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Qty";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, COL].ColumnWidth = 10;
                int colOrderQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Order Qty";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, COL].ColumnWidth = 10;
                int colPlanOrderQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Actual Qty";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colActualQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Produced Qty";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colProducedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Variance";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colVariance = COL;

                COL++;
                sheet[ROW, COL].Text = "WIP";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colCurrentProcessWIP = COL;

                COL++;
                sheet[ROW, COL].Text = "Start Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colStartDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Master Order#";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colMasterOrder = COL;
                COL++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 15;
                int colRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPreviousProcess = COL;
                COL++;
                sheet[ROW, COL].Text = "Qty";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;

                COL++;
                sheet[ROW, COL].Text = "WIP";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colPreviousProcessWIP = COL;

                sheet[ROW, COL].ColumnWidth = 10;
                int colQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Previous Process Start Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPreviousProcessStartDate  = COL;
                COL++;
                sheet[ROW, COL].Text = "Completion Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCompletionDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Completed By";
                sheet[ROW, COL].ColumnWidth = 15;
                int colCompletedBy = COL;



                sheet.Range[5, colSlNo, 5, colRemarks].Merge();
                sheet.Range[5, colSlNo, 5, colRemarks].Text = "Current Process";

                sheet.Range[5, colSlNo, 5, colRemarks].CellStyle.Font.Bold = true;
                sheet.Range[5, colSlNo, 5, colRemarks].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                sheet.Range[5, colSlNo, 5, colRemarks].BorderAround(ExcelLineStyle.Thick);
                sheet.Range[5, colSlNo, 5, colRemarks].BorderInside(ExcelLineStyle.Thick);

                sheet.Range[5, colPreviousProcess, 5, colCompletedBy].Merge();
                sheet.Range[5, colPreviousProcess, 5, colCompletedBy].Text = "Previous Process";

                sheet.Range[5, colPreviousProcess, 5, colCompletedBy].CellStyle.Font.Bold = true;
                sheet.Range[5, colPreviousProcess, 5, colCompletedBy].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                sheet.Range[5, colPreviousProcess, 5, colCompletedBy].BorderAround(ExcelLineStyle.Thick);
                sheet.Range[5, colPreviousProcess, 5, colCompletedBy].BorderInside(ExcelLineStyle.Thick);
               
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                int StartRow = ROW; //row 20
                for (int i = 0; i < dtPurchaseLc.Rows.Count; i++)
                {
                    sheet[ROW, colSlNo].Number = (i + 1);

                    sheet[ROW, colPRId].Text = dtPurchaseLc.Rows[i]["Id"].ToString();
                    sheet[ROW, colMaterial].Text = dtPurchaseLc.Rows[i]["Material"].ToString();
                    sheet[ROW, colArticle].Text = dtPurchaseLc.Rows[i]["article"].ToString();
                    sheet[ROW, colCurrentProcess].Text = dtPurchaseLc.Rows[i]["CurrentProcess"].ToString();
                    sheet[ROW, colBuyerItem].Text = dtPurchaseLc.Rows[i]["StyleNo"].ToString();
                    sheet[ROW, colOwnItem].Text = dtPurchaseLc.Rows[i]["OwnStyleNo"].ToString();
                    sheet[ROW, colLSD].Text = dtPurchaseLc.Rows[i]["LSD"].ToString();
                    sheet[ROW, colOrderQty].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["OrderQty"].ToString());
                    sheet[ROW, colPlanOrderQty].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["PlannedQty"].ToString());
                    sheet[ROW, colActualQty].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["ActualQTY"].ToString());
                    sheet[ROW, colProducedQty].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["ProducedQty"].ToString());
                    sheet[ROW, colVariance].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["Variance"].ToString());
                    sheet[ROW, colCurrentProcessWIP].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["CurrentProcessWIP"].ToString());
                    sheet[ROW, colStartDate].Text = dtPurchaseLc.Rows[i]["StartDate"].ToString();
                    sheet[ROW, colRemarks].Text = dtPurchaseLc.Rows[i]["Remarks"].ToString();
                    sheet[ROW, colPreviousProcess].Text = dtPurchaseLc.Rows[i]["PreviousProcess"].ToString();
                    sheet[ROW, colQty].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["PreviousProcessQty"].ToString());
                    sheet[ROW, colPreviousProcessWIP].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["PreviousProcessWIP"].ToString());
                    sheet[ROW, colPreviousProcessStartDate].Text = dtPurchaseLc.Rows[i]["PreviousProcessStartDate"].ToString();
                    sheet[ROW, colCompletionDate].Text = dtPurchaseLc.Rows[i]["ClosedDate"].ToString();
                    sheet[ROW, colCompletedBy].Text = dtPurchaseLc.Rows[i]["ClosedBy"].ToString();



                    if (clsStaticInfo.nullrecorder(dtPurchaseLc.Rows[i]["ClosedDate"]) != "" && clsStaticInfo.nullrecorder(dtPurchaseLc.Rows[i]["StartDate"]) == "")
                    {
                        sheet.Range[ROW, colSlNo, ROW, colCompletedBy].CellStyle.Interior.ColorIndex = ExcelKnownColors.Red2;
                    }
                    if (clsStaticInfo.nullrecorder(dtPurchaseLc.Rows[i]["PreviousProcessStartDate"]) != "" && clsStaticInfo.nullrecorder(dtPurchaseLc.Rows[i]["StartDate"]) == "")
                    {
                        sheet.Range[ROW, colSlNo, ROW, colCompletedBy].CellStyle.Interior.ColorIndex = ExcelKnownColors.Orange;
                    }
                    if (clsStaticInfo.nullrecorder(dtPurchaseLc.Rows[i]["PreviousProcessStartDate"]) != "" && clsStaticInfo.nullrecorder(dtPurchaseLc.Rows[i]["StartDate"]) != "")
                    {
                        sheet.Range[ROW, colSlNo, ROW, colCompletedBy].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_green;
                    }
                    if (clsStaticInfo.nullrecorder(dtPurchaseLc.Rows[i]["ClosedDate"]) != "" && clsStaticInfo.nullrecorder(dtPurchaseLc.Rows[i]["StartDate"]) != "")
                    {
                        sheet.Range[ROW, colSlNo, ROW, colCompletedBy].CellStyle.Interior.ColorIndex = ExcelKnownColors.Aqua;
                    }

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }

                sheet.Range[StartRow, colOrderQty, ROW, colOrderQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colPlanOrderQty, ROW, colPlanOrderQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colActualQty, ROW, colActualQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colProducedQty, ROW, colProducedQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colVariance, ROW, colVariance].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colQty, ROW, colQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();

                sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colSlNo, ROW, colSlNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Production Relay", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "Production Relay.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private string GetProductionRelayReportSQL(string PlantId, string EntityId, string ProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(EntityId) || EntityId.ToUpper() == "NULL")
            {
               return @"SELECT convert(bit,case when  PLST.processId='" + ProcessId + @"' then 1 else 0 END) AS IsLastProcess,PSS.Remarks,
convert(bit,0) AS Checked, pss.Id PSSId,ppr.Id PPRId,NPR.Id NPRId ,convert(bit ,isnull(ppr.IsCompleted,0)) AS IsCompleted,P.UserName PreviousProcess
,CP.UserName CurrentProcess,NP.UserName NextProcess,PPR.CompletedBy ClosedBy
,Format(PPR.CompletionEntryDate,'dd-MMM-yyyy') ClosedDate ,Format(PPR.StartDate,'dd-MMM-yyyy') PreviousProcessStartDate
,  PO.Id,PO.EntityId,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
isnull(CurrentProcessPR.ProductionQtyAtPR,0) ProducedQty
,Variance=case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end-isnull(CurrentProcessPR.ProductionQtyAtPR,0),
ISNULL(PO.Qty,0) AS POQuantity,ISNULL(SO.PlannedQty,0) AS PlannedQty,ISNULL(SO.OrderQty,0) AS OrderQty,so.Material,
     so.ProductCategory,so.Product,
		ActualQTY=	case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end,							
                                Format(so.LastShipmentDate,'dd-MMM-yyyy') LastShipmentDate, so.article
								,Format(CurrentProcessPR.ProductionStartDateAtPR,'dd-MMM-yyyy') ProductionStartDateAtPR,
								Format(PSS.StartDate,'dd-MMM-yyyy') StartDate,Format(PSS.EndDate,'dd-MMM-yyyy') EndDate,Format(st.LSD,'dd-MMM-yyyy') LSD,
								PSS.CompletedBy ,
 isnull( PreviousProcessPR.ProductionQtyAtPR,0) PreviousProcessQty,
isnull (NextProcessPR.ProductionQtyAtPR,0) NextProcessQty,
CurrentProcessWIP=isnull(CurrentProcessPR.ProductionQtyAtPR,0)-isnull (NextProcessPR.ProductionQtyAtPR,0),
PreviousProcessWIP=isnull( PreviousProcessPR.ProductionQtyAtPR,0)-isnull(CurrentProcessPR.ProductionQtyAtPR,0),

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
							left outer join HKP.Process CP on CP.Id=PSS.ProcessId				

							left join trn.ProductionOrderProcessSet PLST ON PLST.ProductionOrderId=PO.Id and PLST.Id=(select top 1 Id from trn.ProductionOrderProcessSet XP where XP.ProductionOrderId=PO.Id order by XP.Sequence DESC)
							left join trn.ProductionOrderProcessSet PPR ON PSS.ProductionOrderId=PO.Id 
							and PPR.id=(select A.Id from (
							select DENSE_RANK() over (partition by p.ProductionOrderId order by P.[sequence] desc) AS RNK,P.*
							from TRN.ProductionOrderProcessSet P 							
							where p.ProductionOrderId=PSS.ProductionOrderId AND P.Sequence<PSS.sequence) AS A where a.RNK=1)
							left outer join HKP.Process P on p.Id=PPR.ProcessId

						    left join trn.ProductionOrderProcessSet NPR ON PSS.ProductionOrderId=PO.Id 
							and NPR.id=(select A.Id from (
							select DENSE_RANK() over (partition by p.ProductionOrderId order by P.[sequence] ASC) AS RNK,P.*
							from TRN.ProductionOrderProcessSet P 							
							where p.ProductionOrderId=PSS.ProductionOrderId AND P.Sequence>PSS.sequence) AS A where a.RNK=1)
							left outer join HKP.Process NP on NP.Id=NPR.ProcessId

                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDateAtPR,MAX(s.ProductionDate) AS ProductionEndDateAtPR
											FROM  trn.ProductionSummary S 
											where s.ProcessId='" + ProcessId + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS CurrentProcessPR ON  CurrentProcessPR.ProductionOrderId=po.id AND CurrentProcessPR.ProcessId=PSS.ProcessId
							
                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDate,MAX(s.ProductionDate) AS ProductionEndDate
											FROM  trn.ProductionSummary S											
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PreviousProcessPR ON  PreviousProcessPR.ProductionOrderId=po.id AND PreviousProcessPR.ProcessId=PPR.ProcessId
							 
							  LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDate,MAX(s.ProductionDate) AS ProductionEndDate
											FROM  trn.ProductionSummary S											
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS NextProcessPR ON  NextProcessPR.ProductionOrderId=po.id AND NextProcessPR.ProcessId=NPR.ProcessId
							 
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
                            AND ((ISNULL(ppr.Id,'')<>'' AND ISNULL(ppr.StartDate,'')<>'') OR ISNULL(ppr.Id,'')=''  OR ISNULL(ppr.IsCompleted,0)=1)
                             AND EN.PlantId ='" + PlantId + @"' AND PSS.ProcessId = '" + ProcessId + @"' and isnull(pss.IsCompleted,0)=0 
                            ORDER BY st.LSD";
                

            }
            else
            {
               return @"SELECT convert(bit,case when  PLST.processId='" + ProcessId + @"' then 1 else 0 END) AS IsLastProcess,PSS.Remarks,
convert(bit,0) AS Checked, pss.Id PSSId,ppr.Id PPRId,NPR.Id NPRId ,convert(bit ,isnull(ppr.IsCompleted,0)) AS IsCompleted,P.UserName PreviousProcess
,CP.UserName CurrentProcess,NP.UserName NextProcess,PPR.CompletedBy ClosedBy
,Format(PPR.CompletionEntryDate,'dd-MMM-yyyy') ClosedDate ,Format(PPR.StartDate,'dd-MMM-yyyy') PreviousProcessStartDate
,  PO.Id,PO.EntityId,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
isnull(CurrentProcessPR.ProductionQtyAtPR,0) ProducedQty
,Variance=case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end-isnull(CurrentProcessPR.ProductionQtyAtPR,0),
ISNULL(PO.Qty,0) AS POQuantity,ISNULL(SO.PlannedQty,0) AS PlannedQty,ISNULL(SO.OrderQty,0) AS OrderQty,so.Material,
     so.ProductCategory,so.Product,
		ActualQTY=	case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end,							
                                Format(so.LastShipmentDate,'dd-MMM-yyyy') LastShipmentDate, so.article
								,Format(CurrentProcessPR.ProductionStartDateAtPR,'dd-MMM-yyyy') ProductionStartDateAtPR,
								Format(PSS.StartDate,'dd-MMM-yyyy') StartDate,Format(PSS.EndDate,'dd-MMM-yyyy') EndDate,Format(st.LSD,'dd-MMM-yyyy') LSD,
								PSS.CompletedBy ,
 isnull( PreviousProcessPR.ProductionQtyAtPR,0) PreviousProcessQty,
isnull (NextProcessPR.ProductionQtyAtPR,0) NextProcessQty,
CurrentProcessWIP=isnull(CurrentProcessPR.ProductionQtyAtPR,0)-isnull (NextProcessPR.ProductionQtyAtPR,0),
PreviousProcessWIP=isnull( PreviousProcessPR.ProductionQtyAtPR,0)-isnull(CurrentProcessPR.ProductionQtyAtPR,0),

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
							left outer join HKP.Process CP on CP.Id=PSS.ProcessId				

							left join trn.ProductionOrderProcessSet PLST ON PLST.ProductionOrderId=PO.Id and PLST.Id=(select top 1 Id from trn.ProductionOrderProcessSet XP where XP.ProductionOrderId=PO.Id order by XP.Sequence DESC)
							left join trn.ProductionOrderProcessSet PPR ON PSS.ProductionOrderId=PO.Id 
							and PPR.id=(select A.Id from (
							select DENSE_RANK() over (partition by p.ProductionOrderId order by P.[sequence] desc) AS RNK,P.*
							from TRN.ProductionOrderProcessSet P 							
							where p.ProductionOrderId=PSS.ProductionOrderId AND P.Sequence<PSS.sequence) AS A where a.RNK=1)
							left outer join HKP.Process P on p.Id=PPR.ProcessId

						    left join trn.ProductionOrderProcessSet NPR ON PSS.ProductionOrderId=PO.Id 
							and NPR.id=(select A.Id from (
							select DENSE_RANK() over (partition by p.ProductionOrderId order by P.[sequence] ASC) AS RNK,P.*
							from TRN.ProductionOrderProcessSet P 							
							where p.ProductionOrderId=PSS.ProductionOrderId AND P.Sequence>PSS.sequence) AS A where a.RNK=1)
							left outer join HKP.Process NP on NP.Id=NPR.ProcessId

                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDateAtPR,MAX(s.ProductionDate) AS ProductionEndDateAtPR
											FROM  trn.ProductionSummary S 
											where s.ProcessId='" + ProcessId + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS CurrentProcessPR ON  CurrentProcessPR.ProductionOrderId=po.id AND CurrentProcessPR.ProcessId=PSS.ProcessId
							
                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDate,MAX(s.ProductionDate) AS ProductionEndDate
											FROM  trn.ProductionSummary S											
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PreviousProcessPR ON  PreviousProcessPR.ProductionOrderId=po.id AND PreviousProcessPR.ProcessId=PPR.ProcessId
							 
							  LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDate,MAX(s.ProductionDate) AS ProductionEndDate
											FROM  trn.ProductionSummary S											
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS NextProcessPR ON  NextProcessPR.ProductionOrderId=po.id AND NextProcessPR.ProcessId=NPR.ProcessId
							 
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
                            AND ((ISNULL(ppr.Id,'')<>'' AND ISNULL(ppr.StartDate,'')<>'') OR ISNULL(ppr.Id,'')=''  OR ISNULL(ppr.IsCompleted,0)=1)
                             AND EN.PlantId ='" + PlantId + @"' AND  PO.entityid='" + EntityId + @"' and  PSS.ProcessId = '" + ProcessId + @"' and isnull(pss.IsCompleted,0)=0 
                            ORDER BY st.LSD";                

            }


        }



        #endregion
    }
}