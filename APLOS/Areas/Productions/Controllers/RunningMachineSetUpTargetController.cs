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
using Library.Planning.LineDesign;
using Library.Data;
using System.Linq;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class RunningMachineSetUpTargetController : BaseController
    {
        #region Constructor
        /// <summary>   The CostingTypesService service. </summary>
        private readonly ISqlRepository _sqlRepository;
        clsDailyTergatLineDesign DT = new clsDailyTergatLineDesign();
        public RunningMachineSetUpTargetController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            //DataTable dtPlant = _sqlRepository.GetDataTable("Select * from ORG.Plant");
            //Library.Service.Productions.ProductionBooking.ProductionServices scheduler = new Library.Service.Productions.ProductionBooking.ProductionServices(_sqlRepository);
            //for (int i = 0; i < dtPlant.Rows.Count; i++)
            //{
            //    scheduler.UpdateDailyTarget(DateTime.Now.ToString("dd-MMM-yyyy"), dtPlant.Rows[i]["Id"].ToString());
            //}

            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetProcessItemList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from HKP.Process where Active=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetProcessParameterList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from HKP.Process where Active=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadItemDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select *,(select P.UserName from HKP.Process P where P.Id=ID.ProcessId) as Process from [MST].[ItemDetails] ID";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadParameterDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select *,(select P.UserName from HKP.Process P where P.Id=ID.ProcessId) as Process from [MST].[ParameterDetails] ID";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadItemDetailsEditData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select *,(select P.UserName from HKP.Process P where P.Id=ID.ProcessId) as Process from [MST].[ItemDetails] ID where ID.Id='" + ItemId + @"'";
            return Json(new { item = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadParameterDetailsEditData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select *,(select P.UserName from HKP.Process P where P.Id=ID.ProcessId) as Process from [MST].[ParameterDetails] ID where ID.Id='" + ParameterId + @"'";
            return Json(new { parameter = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult createItem(Dictionary<string, object> ItemData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ItemDetails] where ItemName='" + ItemData["ItemName"] + "' and ProcessId='"+ ItemData["ProcessId"] + "'", out DataSet dsItemDetailsItemNameValidation, false, "1");

                DataSet dsItemDetails;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ItemDetails] where Id='" + ItemData["Id"] + "'", out dsItemDetails, false, "1");
                string _Id = "";

                #region data update
                if (dsItemDetails.Tables[0].Rows.Count == 0)
                {
                    if (dsItemDetailsItemNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Item Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("ItemDetails", out _Id);
                        _Id = "QAG" + _Id;
                        ItemData["Id"] = _Id;
                        AddNewRow(dsItemDetails.Tables[0], ItemData);
                    }
                }
                else
                {
                    _Id = ItemData["Id"].ToString();
                    EditRow(dsItemDetails.Tables[0].Rows[0], ItemData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsItemDetails);

                return Json(new { Error = false, Data = ItemData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpPost]
        public JsonResult createParameter(Dictionary<string, object> ParameterData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ParameterDetails] where ParameterName='" + ParameterData["ParameterName"] + "' and ProcessId='" + ParameterData["ProcessId"] + "'", out DataSet dsParameterDetailsParameterNameValidation, false, "1");

                DataSet dsParameterDetails;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ParameterDetails] where Id='" + ParameterData["Id"] + "'", out dsParameterDetails, false, "1");
                string _Id = "";

                #region data update
                if (dsParameterDetails.Tables[0].Rows.Count == 0)
                {
                    if (dsParameterDetailsParameterNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Parameter Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("ParameterDetails", out _Id);
                        _Id = "PD" + _Id;
                        ParameterData["Id"] = _Id;
                        AddNewRow(dsParameterDetails.Tables[0], ParameterData);
                    }
                }
                else
                {
                    _Id = ParameterData["Id"].ToString();
                    EditRow(dsParameterDetails.Tables[0].Rows[0], ParameterData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsParameterDetails);

                return Json(new { Error = false, Data = ParameterData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, mb.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=mb.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetSalesOrder(string entityid, string workCenterMasterId, string productionLevel, string processId, string ProductionOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT DISTINCT mo.MasterOrderNo
	                                ,ISNULL(so.Id,'') SOId
                                    ,ISNULL(moi.Id,'') MOIId
									,Format(so.DeliveryDate,'dd-MMM-yyyy') as DeliveryDate
									,PM.Code as ProductCode
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                               	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
	                                ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description,s.DeliveryDate
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description,s.DeliveryDate
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
	                                FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.SalesOrderId,PS.ProcessId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id AND PRS.ProcessId = '" + processId + @"'
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id AND WC.WorkCenterMasterId = '" + workCenterMasterId + @"'
                                LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PO.EntityId = '" + entityid + @"'	AND PS.UserName = 'Running'	AND POSP.ProcessId = '" + processId + "' AND PO.Id='" + ProductionOrderId + "'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetMasterOrderItem(string entityid, string workCenterMasterId, string productionLevel, string processId, string ProductionOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT DISTINCT mo.MasterOrderNo,so.MasterOrderItemId
	                                ,ISNULL(so.Id,'') SOId
                                    ,PM.Code as ProductCode
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                               	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
	                                ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
	                                FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.SalesOrderId,PS.ProcessId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id AND PRS.ProcessId = '" + processId + @"'
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id AND WC.WorkCenterMasterId = '" + workCenterMasterId + @"'
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PO.EntityId = '" + entityid + @"'	AND PS.UserName = 'Running'	AND POSP.ProcessId = '" + processId + "' AND PO.Id='" + ProductionOrderId + "'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetProductCode(string entityid, string workCenterMasterId, string productionLevel, string processId, string ProductionOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD,PD.MOIId
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer,PD.ProductCode 
                                   ,ISNULL(PD.BuyerOrder,'') BuyerOrder,ISNULL(PD.OwnOrder,'') OwnOrder,ISNULL(PD.BuyerItem,'') BuyerItem,ISNULL(PD.OwnItem,'') OwnItem,PD.Description,PD.PONumber,PD.MaterialMasterId,PD.MaterialMaster,PD.ArticleId,PD.Article
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory,PM.Code as ProductCode
								   ,MM.Id MaterialMasterId,mm.UserName MaterialMaster,MMA.Id ArticleId,ISNULL(mma.StandardName, '') Article,ISNULL(moi.Id,'') MOIId
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
                                   LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								   WHERE PO.EntityId='" + entityid + "' AND PS.UserName = 'Running'  AND PO.Id='" + ProductionOrderId + "'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetDailyTarget(string EntityId, string ProcessId, string TargetDate, string ProductionShiftId, string HeaderResponsiblePersonId, string HeaderInchargeId, string HeaderPlanHour, string HeaderEfficiency)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT  RM.Id,RM.ProcessId,RM.EntityId,RM.ProductionShiftId,RM.TargetDate,wc.Id as WorkCenterMasterId,CAST (CASE WHEN RM.Id IS NULL THEN 0 ELSE 1 END AS bit) AS Active,wc.UserName as Line,WC.Id WorkCenterMasterId,WC.NoOfWorkStation WorkStation,
isnull(RM.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId+ @"' and WorkCenterMasterID=WC.Id order by AddedDate desc)) as ProductionOrderId,
isnull(RM.LotNumber,(select top 1 LotNumber from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId+ @"' and WorkCenterMasterId=wc.Id order by AddedDate desc)) as LotNumber,
isnull(PPS.ProductionBookingLevel,(select ProductionBookingLevel from hkp.EntityProcessTag where EntityId='" + EntityId + "' and ProcessId='" + ProcessId + @"')) as BookingLevel,
RM.SalesOrderId,
(select MA.StandardName from trn.salesorder SO
left outer join trn.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
where SO.Id=RM.SalesOrderId) as SOArticle,RM.MasterOrderItemId,(select MA.StandardName from trn.MasterOrderItem MOI
left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
where MOI.Id=RM.MasterOrderItemId) as MOIArticle,(select MA.StandardName from trn.MasterOrderItem MOI
left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
where MOI.Id=RM.MasterOrderItemId) as ProductCodeArticle,
Article=STUFF((select distinct ','+MA.StandardName from trn.ProductionOrderDetail Pod 
                                                            left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId=so.Id
                                                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                            left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                            where Pod.ProductionOrderId=isnull(RM.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
isnull(RM.SMV,PS.SPT) as SMV,
isnull(RM.PlanHours,'" + HeaderPlanHour + @"') as PlanHours,
isnull(RM.TargetFD,(60/PS.SPT*WC.NoOfWorkStation * " + HeaderPlanHour + @" * " + HeaderEfficiency + @") / 100) as TargetFD,
isnull(R.EmployeeName,(select EmployeeName from EmployeeInformation where SystemId = (select top 1 ResponsiblePersonId from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as ResponsiblePerson,
isnull(R.SystemId,(select SystemId from EmployeeInformation where SystemId = (select top 1 ResponsiblePersonId from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as ResponsiblePersonId,
isnull(I.EmployeeName,(select EmployeeName from EmployeeInformation where SystemId =(select top 1 InChargeId from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as InCharge,
isnull(I.SystemId,(select SystemId from EmployeeInformation where SystemId =(select top 1 InChargeId from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as InChargeId,

RM.Remarks,
isnull(RM.Efficiency,'" + HeaderEfficiency + @"') as Efficiency,
isnull(RM.TargetProductionFP,(60/PS.SPT*WC.NoOfWorkStation * " + HeaderPlanHour + @")) as TargetProductionFP,
isnull(SM.SumMinute,0) as SumMin
FROM  SCS.WorkCenterMaster wc 
                        LEFT JOIN TRN.RunningMachineSetUpTarget RM ON RM.WorkCenterMasterId=wc.Id AND RM.ProcessId = '" + ProcessId + @"'  
                        AND  RM.EntityId='" + EntityId + @"' AND RM.TargetDate='"+ TargetDate + "'  AND RM.ProductionShiftId ='" + ProductionShiftId+ @"' 
                        LEFT JOIN trn.ProductionOrder AS PO ON PO.ID=isnull(RM.ProductionOrderId,(select top 1 ProductionOrderId from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"'  and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId+ @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))
						LEFT JOIN EmployeeInformation R ON RM.ResponsiblePersonId=R.SystemId
                        LEFT JOIN EmployeeInformation I ON RM.InChargeId=I.SystemId
                        LEFT JOIN TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderID = PO.Id AND PPS.ProcessId = '" + ProcessId + @"'
                        LEFT JOIN (select ISNULL(sum(Minute),0) as SumMinute,WorkCenterId,RMSTargetId from RMSTargetDetentionTransaction MT where MT.ProcessId='" + ProcessId + @"' and MT.EntityId = '" + EntityId + @"' AND MT.Date='" + TargetDate + @"'  AND MT.ShiftId='" + ProductionShiftId + @"'
                        group by WorkCenterId,RMSTargetId) SM ON SM.WorkCenterId=wc.Id and SM.RMSTargetId=RM.Id
						left join ProductionOrderSchedulingParametersType1 PS ON PS.ProductionOrderID=isnull(RM.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))
						where wc.Active=1 and wc.ProcessId = '" + ProcessId + @"'  and wc.EntityId = '" + EntityId + @"' ORDER BY wc.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessItemList(string ProcessId,string RMSTargetId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select IV.IsActive,IV.Id,IV.SNo,IV.ItemValue,IV.Remarks,ID.Id as ItemId,ID.ItemName,(select UserName from hkp.process where Id=ID.ProcessId) as Process
from MST.ItemDetails ID
left join [TRN].[RMSTargetItemValue] IV ON IV.ItemId=ID.Id and RMSTargetId='"+ RMSTargetId + @"'
where ProcessId='" + ProcessId +"'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessReasonList(string ProcessId, string ProductionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select RV.Id,RV.ReasonValue,RV.Remarks,RD.Id as ReasonId,RD.ReasonName
from MST.ReasonDetails RD
left join [TRN].[RMSTargetReasonValue] RV ON RV.ReasonId=RD.Id and ProductionId='" + ProductionId + @"'
where RD.ProcessId='" + ProcessId + "'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessParameterList(string ProcessId, string ProductionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select distinct '' as Id,isnull(PV.SNo,(select top 1 SNo from [TRN].[RMSTargetParameterValue] where ParameterId=PD.Id order by AddedDate desc)) as SNo,
isnull(PV.ParameterValue,(select top 1 ParameterValue from [TRN].[RMSTargetParameterValue] where ParameterId=PD.Id order by AddedDate desc)) as ParameterValue,
isnull(PV.Remarks,(select top 1 Remarks from [TRN].[RMSTargetParameterValue] where ParameterId=PD.Id order by AddedDate desc)) as Remarks,
PD.Id as ParameterId,
PD.ParameterName,
isnull(format(PV.ParameterDate,'dd-MMM-yyyy'), (select top 1 format(ParameterDate,'dd-MMM-yyyy') from [TRN].[RMSTargetParameterValue] where ParameterId=PD.Id order by AddedDate desc)) as ParameterDate,
Day(getdate()-isnull(format(PV.ParameterDate,'dd-MMM-yyyy'), (select top 1 format(ParameterDate,'dd-MMM-yyyy') from [TRN].[RMSTargetParameterValue] where ParameterId=PD.Id order by AddedDate desc))) as ParameterDays,
isnull(PV.AddedBy,(select top 1 AddedBy from [TRN].[RMSTargetParameterValue] where ParameterId=PD.Id order by AddedDate desc)) as AddedBy
--,isnull(PV.UpdatedBy,(select top 1 UpdatedBy from [TRN].[RMSTargetParameterValue] where ParameterId=PD.Id order by AddedDate desc)) as UpdatedBy
from MST.ParameterDetails PD
left join [TRN].[RMSTargetParameterValue] PV ON PV.ParameterId=PD.Id and PV.ProductionId='" + ProductionId + @"'
where PD.ProcessId='" + ProcessId + "'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetTargetProductioinDiff(string RMSTargetId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select (TargetFD-TargetProductionFP) as  DifferenceFP from TRN.RunningMachineSetUpTarget where Id='"+ RMSTargetId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

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
        public JsonResult Create(List<Dictionary<string, object>> DailyTargetData, string TargetDate, string EntityId, string ProcessId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[RunningMachineSetUpTarget]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (DailyTargetData != null)
                {
                    foreach (var item in DailyTargetData)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "PCD" + _Id;
                            item["PlantId"] = identity.PlantId;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            item["PlantId"] = identity.PlantId;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new {Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult createSingleRow(Dictionary<string, object> DailyTargetData, string TargetDate, string EntityId, string ProcessId)
        {
            
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsRunningMachineSetUpTarget;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from TRN.RunningMachineSetUpTarget where Id='" + DailyTargetData["Id"] + "'", out dsRunningMachineSetUpTarget, false, "1");
                string _Id = "", Id = string.Empty;

                #region data update
                if (dsRunningMachineSetUpTarget.Tables[0].Rows.Count == 0)
                {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("[TRN].[RunningMachineSetUpTarget]", out _Id);
                        _Id = "PCD" + _Id;
                    DailyTargetData["Id"] = _Id;
                    DailyTargetData["PlantId"] = identity.PlantId;
                    Id = DailyTargetData["Id"].ToString();
                    AddNewRow(dsRunningMachineSetUpTarget.Tables[0], DailyTargetData);

                }
                else
                {
                    _Id = DailyTargetData["Id"].ToString();
                    DailyTargetData["PlantId"] = identity.PlantId;
                    Id = DailyTargetData["Id"].ToString();
                    EditRow(dsRunningMachineSetUpTarget.Tables[0].Rows[0], DailyTargetData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRunningMachineSetUpTarget);

                return Json(new { Id = Id, Error = false, Data = DailyTargetData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult UpdateSingleRow(List<Dictionary<string, object>> DailyTargetData, string TargetDate, string EntityId, string ProcessId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[RunningMachineSetUpTarget]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (DailyTargetData != null)
                {
                    foreach (var item in DailyTargetData)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "PCD" + _Id;
                            item["PlantId"] = identity.PlantId;
                            Id = item["Id"].ToString();
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            item["PlantId"] = identity.PlantId;
                            Id = item["Id"].ToString();
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Id = Id, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult CreateItemValue(List<Dictionary<string, object>> RMSTargetItemData)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[RMSTargetItemValue]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (RMSTargetItemData != null)
                {
                    foreach (var item in RMSTargetItemData)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "TIV" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        [HttpPost]
        public JsonResult createReasonValue(List<Dictionary<string, object>> ProductionReasonData)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[RMSTargetReasonValue]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (ProductionReasonData != null)
                {
                    foreach (var item in ProductionReasonData)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "RRV" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult createParameterValue(List<Dictionary<string, object>> ProductionParameterData)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[RMSTargetParameterValue]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (ProductionParameterData != null)
                {
                    foreach (var item in ProductionParameterData)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "RPV" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult createSinglePValue(Dictionary<string, object> ProductionParameterData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[RMSTargetParameterValue] where ProductionId='" + ProductionParameterData["ProductionId"] + "' and ParameterId ='" + ProductionParameterData["ParameterId"] + "'", out DataSet dsRMSTargetParameterValueValidation, false, "1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsRMSTargetParameterValue;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[RMSTargetParameterValue] where Id='" + ProductionParameterData["Id"] + "'", out dsRMSTargetParameterValue, false, "1");
                string _Id = "", Id = string.Empty;

                #region data update
                if (dsRMSTargetParameterValue.Tables[0].Rows.Count == 0)
                {
                    if (dsRMSTargetParameterValueValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("This Record Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("[TRN].[RMSTargetParameterValue]", out _Id);
                        ProductionParameterData["Id"] = "RPV" + _Id;
                        AddNewRow(dsRMSTargetParameterValue.Tables[0], ProductionParameterData);
                    }

                }
                else
                {
                    _Id = ProductionParameterData["Id"].ToString();
                    EditRow(dsRMSTargetParameterValue.Tables[0].Rows[0], ProductionParameterData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRMSTargetParameterValue);

                return Json(new { Error = false, Data = ProductionParameterData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetProcessDetentionData(string processId, string entityId, string productionDate, string shiftId, string workcenter, string RMSTargetId)
        {
            try
            {
                string sql = "";
                string DetentionTypeListsql = "";
                string DetentionListsql = "";
                sql = @"
SELECT CAST (CASE WHEN MMT.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,MMT.Sequence,MMT.Id, MMT.EntityId, MMT.DetentionId, MMT.DetentionTypeId, MMT.ProcessId, MMT.DepartmentId, MMT.ShiftId, MMT.ResponsiblePersonId as ResponsiblePersonId, 
MMT.Remark, MMT.AddedBy, MMT.AddedDate, MMT.AddedFromIP, MMT.UpdatedBy, MMT.UpdatedDate, MMT.UpdatedFromIP
,E.UserName Entity,D.UserName DepartmentName,DM.DetentionUserName Detention,FORMAT(MMT.Date,'dd-MMM-yyyy')[Date],P.UserName Process
										,MMT.Minute as [Minute],SD.UserName Shift,
										EI.EmployeeName ResponsiblePerson,EI.EmployeeCode ResponsiblePersonCode,MMT.Remark,MMT.WorkCenterId,WC.UserName as WorkCenter,MMT.RMSTargetId
			                            from RMSTargetDetentionTransaction MMT
			                            left join ORG.Entity E on E.Id=MMT.EntityId
										left join ORG.Department D on D.Id=MMT.DepartmentId
										left join DetentionMaster DM on DM.Id=MMT.DetentionId
										left join HKP.Process P on P.Id=MMT.ProcessId
										left join ShiftDefination SD on SD.SystemID=MMT.ShiftId
										left Join SCS.WorkCenterMaster WC on WC.id=MMT.WorkCenterId
										left join EmployeeInformation EI on EI.SystemId=MMT.ResponsiblePersonId
                where MMT.EntityId = '" + entityId + "' and MMT.ProcessId = '" + processId + "'  and MMT.Date = '" + productionDate + "' and MMT.ShiftId = '" + shiftId + "' and MMT.WorkCenterId = '" + workcenter + "' and MMT.RMSTargetId='"+ RMSTargetId + "'";


                //return _sqlRepository.GetDataCollection(sql, null);

                DetentionTypeListsql = @"select DT.UserName As Text, DT.Id As Value from MachineMasterTransaction MMT 
                                         left outer join hkp.DetentionType DT ON DT.id=MMT.DetentionTypeId";

                DetentionListsql = @"Select DM.DetentionUserName As Text, DM.Id As Value,DM.IsAssetApplicable,DM.IsWorkCenterApplicable from MachineMasterTransaction MMT 
                                     left outer join  DetentionMaster DM ON DM.id=MMT.DetentionId";

                List<Dictionary<string, object>> MainList = _sqlRepository.GetDataCollection(sql);
                List<Dictionary<string, object>> detentiontypelist = _sqlRepository.GetDataCollection(DetentionTypeListsql);
                List<Dictionary<string, object>> detentionList = _sqlRepository.GetDataCollection(DetentionListsql);
                for (int i = 0; i < MainList.Count; i++)
                {
                    try
                    {
                        //List<Dictionary<string, object>> k = detentiontypelist.ToList();
                        List<Dictionary<string, object>> k = detentiontypelist.Where(ee => clsStaticInfo.nullrecorder(ee["Value"]) == clsStaticInfo.nullrecorder(MainList[i]["DetentionTypeId"])).ToList();
                        MainList[i]["DetentionTypeList"] = k;

                    }
                    catch (Exception)
                    {

                    }

                    try
                    {
                        List<Dictionary<string, object>> m = detentionList.Where(ee => clsStaticInfo.nullrecorder(ee["Value"]) == clsStaticInfo.nullrecorder(MainList[i]["DetentionId"])).ToList();


                        MainList[i]["DetentionList"] = m;
                    }
                    catch (Exception)
                    {

                    }


                }
                return Json(MainList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return Json(_productionSummaryData.GetProcessDetentionData(processId, entityId, productionDate, shiftId, workcenter), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createDetentionWC(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "RMSTargetDetentionTransaction";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);
                        if (item["DetentionId"] == null)
                        {

                            throw new CustomException("Detention should not be blank!");
                        }
                        else
                        {
                            if (dv.Count == 0)
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID(TableName, out _Id);
                                item["Id"] = "RDT" + _Id;
                                AddNewRow(dsProdBooked.Tables[0], item);
                            }
                            else
                            {
                                DataRow drpb = dv[0].Row;
                                EditRow(drpb, item);
                            }
                            clsStaticInfo obj = new clsStaticInfo();
                            obj.SaveDataSets(dsProdBooked);
                        }
                    }
                }
                else
                {
                    throw new CustomException("Please enter atleast one row and proceed!");
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public ActionResult DeleteMasterWC(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();

                conC.BeginTransaction();
                conC.executeQuery("delete from TRN.RMSTargetReasonValue where ProductionId ='" + id + @"'");
                conC.executeQuery("delete from TRN.RMSTargetItemValue where RMSTargetId ='" + id + @"'");
                conC.executeQuery("delete from RMSTargetDetentionTransaction where RMSTargetId ='" + id + @"'");
                conC.executeQuery("delete from TRN.RunningMachineSetUpTarget where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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
        public ActionResult LoadProcessDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN QMP.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,QMP.Id,P.Id ProcessId,P.UserName Process,P.Code,QAG.Id as ActivityGroupId,QAG.ActivityGroupName,QMP.Remarks
                            from hkp.Process P
							LEFT JOIN [MST].[QualityManagementProcess] QMP ON QMP.ProcessId=P.Id
							LEFT JOIN  MST.QualityManagementActivityGroup QAG ON QAG.Id=QMP.ActivityGroupId
                            where P.Active = 1 order by QMP.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetProductionOrderPOPUp(string entityid, string processId, string PlanHours)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"
                        SELECT  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
                        ISNULL(PO.Qty,0) AS POQuantity,ISNULL(PO.PlannedQty,0) AS SOQuantity,ISNULL(SO.Qty,0) AS SavedQuantity,t1.Color,FORMAT(t1.LSD,'dd-MMM-yyyy') AS LSD,FORMAT(t1.CommitmentDate,'dd-MMM-yyyy') AS CommitmentDate,t1.TargetPerDay
                                ,T1.ProductionPriority ,so.Material,so.Article,so.MaterialMasterId,so.ArticleId, so.Product,t1.Qty,t1.SPT,
                                so.ProductCategory, so.FirstShipmentDate,
                                so.LastShipmentDate, so.buyer, so.BuyerRefNo,
                                so.OwnRefNo, so.StyleNo, so.OwnStyleNo, so.SONo,
                                so.SODesc,So.MasterOrderId,
                                so.Customer,so.article,PRODPR.ProductionQtyAtPR,So.BuyerItemNo,SO.CustomerPONo
                                   ,ISNULL(CASE WHEN ISNULL(T1.Qty,0)>0 THEN T1.Qty ELSE PO.PlannedQty END,0)-(ISNULL(PRODPR.ProductionQtyAtPR,0)-ISNULL(PRDQ.ProductionBookedQty,0)) AS ToBePlanQty
                              ,PRODPR.LotNumber    			
  
                            FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            INNER JOIN ProductionOrderSchedulingParametersType1 t1 ON t1.ProductionOrderID=po.Id

                             LEFT OUTER JOIN (
												SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR,s.LotNumber
											FROM  trn.ProductionSummary S 
											--WHERE  CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<'" + System.DateTime.Now.ToString("dd-MMM-yyyy") + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId,s.LotNumber
							) AS PRODPR ON  PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
							 left outer join (SELECT pod.ProductionOrderId,
                                sum(isnull(so.ProductionBookedQty,0)) ProductionBookedQty
                                 FROM trn.SalesOrder AS so
                                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id

                                GROUP BY pod.ProductionOrderId
                            ) AS PRDQ ON PRDQ.ProductionOrderId=T1.ProductionOrderId
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,MMA.StandardName AS Article,MOI.MaterialMasterId,MOI.ArticleId, PM.UserName AS Product,pc.UserName AS ProductCategory,
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
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    BuyerItemNo=STUFF((select distinct ', '+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 


                                                     CustomerPONo=STUFF((select distinct ', '+CPO.PONumber from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
			                                                    where POD.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                                      from 
 
 
                                                     trn.SalesOrder SO 
                                                      JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                                    left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                    left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                    left outer join mst.MaterialMasterArticle mma on mma.id=MOI.ArticleId

                                                    left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
													LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                    group by pod.ProductionOrderId,mm.userName,MMA.StandardName,MOI.MaterialMasterId,MOI.ArticleId,ma.StandardName,PM.UserName,pc.UserName) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE isnull(s.username,'') IN ('RUNNING') ";
            //--AND  (PO.entityid='" + entityid + @"' OR isnull(PO.Id,'') IN 
            //                       -- ( SELECT distinct P.ProductionOrderId
            //                              --   FROM trn.ProductionOrderWorkCenter AS p
            //                              -- JOIN scs.WorkCenterMaster AS w ON w.Id=p.WorkCenterMasterId
            //                             --  WHERE w.EntityId='" + entityid + @"'
                                           
            //                               UNION
                                           
                                                 
            //                               SELECT distinct P.ProductionOrderId
            //                                 FROM trn.RunningOrderWorkCenter  AS p
            //                               JOIN scs.WorkCenterMaster AS w ON w.Id=p.WorkCenterMasterId
            //                               WHERE w.EntityId='" + entityid + @"')) and PO.Id IN (SELECT DISTINCT pops.ProductionOrderId
            //                FROM trn.ProductionOrderProcessSet AS pops WHERE pops.ProcessId = '" + processId + @"') ";



            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetLineLayoutData(string entityid, string processId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @" ";



            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Copy function
        [HttpPost, Authorize]
        public ActionResult CopyFromTable(string entityid, string processId, string ProductionDate, Dictionary<string, object> SelectedLine)
        {
            try
            {

                DT.CopyFromTable(entityid, processId, ProductionDate, SelectedLine);
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult GetSaveData(string WorkCenterMasterId, string ProductionOrderId, string TargetDate)
        {
            return Json(DT.GetDesign(WorkCenterMasterId, ProductionOrderId, TargetDate), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetBottleneck(string WorkCenterMasterId, string ProductionOrderId, string TargetDate, string ProcessId)
        {
            DT.GetBottleneck(WorkCenterMasterId, ProductionOrderId, TargetDate, ProcessId, out List<Dictionary<string, object>> StripLine, out List<Dictionary<string, object>> Data);
            return Json(new { StripLine = StripLine, GraphData = Data }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult SaveProductionData(List<Dictionary<string, object>> ProductionData, string WorkCenterMasterId, string ProductionOrderId, string TargetDate)
        {
            try
            {
                DT.SaveProductionData(ProductionData, WorkCenterMasterId, ProductionOrderId, TargetDate);
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost]
        public JsonResult SaveDiagram(List<Html> Nodes, string Design, string WorkCenterMasterId, string ProductionOrderId, string TargetDate)
        {
            try
            {
                DataSet dsData;
                DT.SaveData(Nodes, Design, WorkCenterMasterId, ProductionOrderId, TargetDate, out dsData);
                return Json(new { Error = false, Data = Helpers.CustomJsonResult.DataTableToJson(dsData.Tables[0]), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }


            //return Json(new { data=dsData ,Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult SearchEmployee(string column, string value, string OperationId, string OperationVariationId, string TargetDate)
        {
            return Json(DT.SearchEmployee(column, value, OperationId, OperationVariationId, TargetDate), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult SearchFixedAsset(string column, string value, string ArticleId)
        {
            return Json(DT.SearchFixedAsset(column, value, ArticleId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetEmployeeCard(string EmployeeId, string OperationVariationId, string AssetRegisterId, string TargetDate)
        {
            return Json(DT.GetEmployeeCard(EmployeeId, OperationVariationId, AssetRegisterId, TargetDate), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult UpdateEmployeeAttendanceAndProductionInfo(string EmployeeId, string TargetDate)
        {
            return Json(DT.UpdateEmployeeAttendanceAndProductionInfo(EmployeeId, TargetDate), JsonRequestBehavior.AllowGet);
        }
        #endregion

    }


}