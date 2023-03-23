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
        public ActionResult LoadItemDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select *,(select P.UserName from HKP.Process P where P.Id=ID.ProcessId) as Process from [MST].[ItemDetails] ID";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadItemDetailsEditData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select *,(select P.UserName from HKP.Process P where P.Id=ID.ProcessId) as Process from [MST].[ItemDetails] ID where ID.Id='" + ItemId + @"'";
            return Json(new { item = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=ei.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetDailyTarget(string EntityId, string ProcessId, string TargetDate, string ProductionShiftId, string HeaderResponsiblePersonId, string HeaderInchargeId, string HeaderPlanHour)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT  RM.Id,RM.ProcessId,RM.EntityId,RM.ProductionShiftId,RM.TargetDate,wc.Id as WorkCenterMasterId,CAST (CASE WHEN RM.Id IS NULL THEN 0 ELSE 1 END AS bit) AS Active,wc.UserName as Line,WC.Id WorkCenterMasterId,WC.NoOfWorkStation WorkStation,
isnull(RM.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId+ @"' and WorkCenterMasterID=WC.Id order by AddedDate desc)) as ProductionOrderId,
isnull(RM.LotNumber,(select top 1 LotNumber from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId+ @"' and WorkCenterMasterId=wc.Id order by AddedDate desc)) as LotNumber,
Article=STUFF((select distinct ','+MA.StandardName from trn.ProductionOrderDetail Pod 
                                                            left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId=so.Id
                                                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                            left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                            where Pod.ProductionOrderId=isnull(RM.ProductionOrderId,(select top 1 ProductionOrderId from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"'  and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId+ @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
isnull(RM.SMV,PS.SPT) as SMV,isnull(RM.PlanHours,(select top 1 PlanHours from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc)) as PlanHours,
--isnull(RM.TargetFD,ceiling((60/PS.SPT)*isnull(RM.PlanHours," + HeaderPlanHour + @")*PS.NoOfWorkStation)) as TargetFD,
isnull(RM.TargetFD,(select top 1 RM.TargetFD from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc)) as TargetFD,
isnull(R.EmployeeName,(select EmployeeName from EmployeeInformation where SystemId = (select top 1 ResponsiblePersonId from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as ResponsiblePerson,
isnull(R.SystemId,(select SystemId from EmployeeInformation where SystemId = (select top 1 ResponsiblePersonId from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as ResponsiblePersonId,
isnull(I.EmployeeName,(select EmployeeName from EmployeeInformation where SystemId =(select top 1 InChargeId from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as InCharge,
isnull(I.SystemId,(select SystemId from EmployeeInformation where SystemId =(select top 1 InChargeId from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as InChargeId,

RM.Remarks,isnull(RM.Efficiency,(select top 1 Efficiency from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc)) as Efficiency,isnull(RM.TargetProductionFP,(select top 1 TargetProductionFP from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc)) as TargetProductionFP
FROM  SCS.WorkCenterMaster wc 
                        LEFT JOIN TRN.RunningMachineSetUpTarget RM ON RM.WorkCenterMasterId=wc.Id AND RM.ProcessId = '" + ProcessId + @"'  
                        AND  RM.EntityId='" + EntityId + @"' AND RM.TargetDate='"+ TargetDate + "'  AND RM.ProductionShiftId ='" + ProductionShiftId+ @"' 
                        LEFT JOIN trn.ProductionOrder AS PO ON PO.ID=isnull(RM.ProductionOrderId,(select top 1 ProductionOrderId from TRN.RunningMachineSetUpTarget where ProcessId = '" + ProcessId + @"'  and EntityId='" + EntityId + @"' and ProductionShiftId ='" + ProductionShiftId+ @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))
						LEFT JOIN EmployeeInformation R ON RM.ResponsiblePersonId=R.SystemId
                        LEFT JOIN EmployeeInformation I ON RM.InChargeId=I.SystemId
						left join ProductionOrderSchedulingParametersType1 PS ON PS.ProductionOrderID=isnull(RM.ProductionOrderId,(select top 1 ProductionOrderId from TRN.RunningMachineSetUpTarget where WorkCenterMasterID=WC.Id order by AddedDate desc))
						where wc.ProcessId = '" + ProcessId + @"'  and wc.EntityId = '" + EntityId + @"' ORDER BY wc.Sequence";
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
        public JsonResult createSingleRow(List<Dictionary<string, object>> DailyTargetData, string TargetDate, string EntityId, string ProcessId)
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
                return Json(new { Id = Id, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost, Authorize]
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


        [HttpPost, Authorize]
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
                                   ,ISNULL(CASE WHEN ISNULL(T1.Qty,0)>0 THEN T1.Qty ELSE PO.PlannedQty END,0)-(ISNULL(PRODPR.ProductionQtyAtPR,0)-ISNULL(PRDQ.ProductionBookedQty,0)) AS ToBePlanQty,CEILING((60/t1.SPT)*("+ PlanHours +@")*t1.NoOfWorkStation) as TargetFD
                                  			
  
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
                            WHERE isnull(s.username,'') IN ('ACTIVE','RUNNING') AND  (PO.entityid='" + entityid + @"' OR isnull(PO.Id,'') IN 
                                    ( SELECT distinct P.ProductionOrderId
                                             FROM trn.ProductionOrderWorkCenter AS p
                                           JOIN scs.WorkCenterMaster AS w ON w.Id=p.WorkCenterMasterId
                                           WHERE w.EntityId='" + entityid + @"'
                                           
                                           UNION
                                           
                                                 
                                           SELECT distinct P.ProductionOrderId
                                             FROM trn.RunningOrderWorkCenter  AS p
                                           JOIN scs.WorkCenterMaster AS w ON w.Id=p.WorkCenterMasterId
                                           WHERE w.EntityId='" + entityid + @"')) and PO.Id IN (SELECT DISTINCT pops.ProductionOrderId
                            FROM trn.ProductionOrderProcessSet AS pops WHERE pops.ProcessId = '" + processId + @"') ";



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