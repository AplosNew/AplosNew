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
    public class DailyTargetController : BaseController
    {
        #region Constructor
        /// <summary>   The CostingTypesService service. </summary>
        private readonly ISqlRepository _sqlRepository;
        clsDailyTergatLineDesign DT = new clsDailyTergatLineDesign();
        public DailyTargetController(ISqlRepository R)
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
            try
            {

                for (int i = 0; i < DailyTargetData.Count; i++)
                {
                    if (bplib.clsWebLib.GetBoolData(DailyTargetData[i]["Active"]) == false)
                        continue;

                    string LineNo = " For Workcenter:" + DailyTargetData[i]["Line"];

                    if (clsStaticInfo.nullrecorder(DailyTargetData[i]["PRNo"]) == "")
                        throw new Exception("Select production order" + LineNo);


                    if (clsStaticInfo.dbl(DailyTargetData[i]["ManPowerWithMachine"]) < 0)
                        throw new Exception("Man Power With Machine cannot be negative" + LineNo);

                    if (clsStaticInfo.dbl(DailyTargetData[i]["ManPowerWithHand"]) < 0)
                        throw new Exception("Man Power Without Machine cannot be negative" + LineNo);

                    double totalManpower = clsStaticInfo.dbl(DailyTargetData[i]["ManPowerWithMachine"]) + clsStaticInfo.dbl(DailyTargetData[i]["ManPowerWithHand"]);
                    if (totalManpower <= 0)
                        throw new Exception("Man Power is required" + LineNo);


                    if (clsStaticInfo.dbl(DailyTargetData[i]["SMV"]) <= 0)
                        throw new Exception("SPT is required" + LineNo);

                    if (clsStaticInfo.dbl(DailyTargetData[i]["TotalHour"]) <= 0)
                        throw new Exception("Target Hour is required" + LineNo);

                    if (clsStaticInfo.dbl(DailyTargetData[i]["Quantity"]) <= 0)
                        throw new Exception("Daily Target Quantity is required" + LineNo);

                }


                DataSet dsDailyTarget;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager conBin = new ConnectionManager.DAL.ConManager("1");
                conBin.OpenDataSetThroughAdapter(@"select * from TRN.DailyProductionTarget where TargetDate='" + TargetDate + @"' and 
                        WorkCenterMasterID in (select Id from SCS.WorkCenterMaster WCM where WCM.EntityId='" + EntityId + @"' AND WCM.ProcessId='" + ProcessId + @"' )", out dsDailyTarget, false, "1");

                string DailyTargetId = "";
                for (int i = 0; i < DailyTargetData.Count; i++)
                {
                    if (bplib.clsWebLib.GetBoolData(DailyTargetData[i]["Active"]) == false)
                        continue;

                    double totalManpower = clsStaticInfo.dbl(DailyTargetData[i]["ManPowerWithMachine"]) + clsStaticInfo.dbl(DailyTargetData[i]["ManPowerWithHand"]);

                    dsDailyTarget.Tables[0].DefaultView.RowFilter = "ID='" + DailyTargetData[i]["DailyProductionTargetID"] + "'";
                    if (dsDailyTarget.Tables[0].DefaultView.Count > 0)
                    {

                        //edit
                        DataRow dr = dsDailyTarget.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["PlantId"] = identity.PlantId;
                        dr["ManPowerWithMachine"] = clsStaticInfo.dbl(DailyTargetData[i]["ManPowerWithMachine"]);
                        dr["ManPowerWithHand"] = clsStaticInfo.dbl(DailyTargetData[i]["ManPowerWithHand"]);
                        dr["Manpower"] = totalManpower;
                        dr["SMV"] = clsStaticInfo.dbl(DailyTargetData[i]["SMV"]);
                        dr["TotalHour"] = clsStaticInfo.dbl(DailyTargetData[i]["TotalHour"]);
                        dr["QuantityPerHour"] = clsStaticInfo.dbl(DailyTargetData[i]["QuantityPerHour"]);
                        dr["Quantity"] = (int)(clsStaticInfo.dbl(DailyTargetData[i]["QuantityPerHour"]) * clsStaticInfo.dbl(DailyTargetData[i]["TotalHour"]));
                        dr["MaterialMasterId"] = DailyTargetData[i]["MaterialMasterId"];
                        dr["MaterialMasterArticleId"] = DailyTargetData[i]["MaterialMasterArticleId"];
                        dr["EmployeeId"] = DailyTargetData[i]["EmployeeId"];

                        dr["PlantID"] = identity.PlantId;
                        dr["ProductionOrderId"] = DailyTargetData[i]["PRNo"];
                        dr["isBuildUp"] = false;
                        dr["WorkCenterMasterID"] = DailyTargetData[i]["WorkCenterMasterId"];
                        dr["TargetDate"] = TargetDate;
                        dr["IsManual"] = true;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr.EndEdit();

                    }
                    else
                    {


                        dsDailyTarget.Tables[0].DefaultView.RowFilter = "ProductionOrderId='" + DailyTargetData[i]["PRNo"] + "' and   WorkCenterMasterID = '" + DailyTargetData[i]["WorkCenterMasterId"] + "'";
                        if (dsDailyTarget.Tables[0].DefaultView.Count > 0)
                        {
                            //edit
                            DataRow dr = dsDailyTarget.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["PlantId"] = identity.PlantId;
                            dr["ManPowerWithMachine"] = clsStaticInfo.dbl(DailyTargetData[i]["ManPowerWithMachine"]);
                            dr["ManPowerWithHand"] = clsStaticInfo.dbl(DailyTargetData[i]["ManPowerWithHand"]);
                            dr["Manpower"] = totalManpower;
                            dr["SMV"] = clsStaticInfo.dbl(DailyTargetData[i]["SMV"]);
                            dr["TotalHour"] = clsStaticInfo.dbl(DailyTargetData[i]["TotalHour"]);
                            dr["QuantityPerHour"] = clsStaticInfo.dbl(DailyTargetData[i]["QuantityPerHour"]);
                            dr["Quantity"] = (int)(clsStaticInfo.dbl(DailyTargetData[i]["QuantityPerHour"]) * clsStaticInfo.dbl(DailyTargetData[i]["TotalHour"]));
                            dr["MaterialMasterId"] = DailyTargetData[i]["MaterialMasterId"];
                            dr["MaterialMasterArticleId"] = DailyTargetData[i]["MaterialMasterArticleId"];
                            dr["EmployeeId"] = DailyTargetData[i]["EmployeeId"];

                            dr["PlantID"] = identity.PlantId;
                            dr["ProductionOrderId"] = DailyTargetData[i]["PRNo"];
                            dr["isBuildUp"] = false;
                            dr["WorkCenterMasterID"] = DailyTargetData[i]["WorkCenterMasterId"];
                            dr["TargetDate"] = TargetDate;
                            dr["IsManual"] = true;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr.EndEdit();
                        }
                        else
                        {
                            //addnew
                            if (DailyTargetId == "")
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID("TRN.DailyProductionTarget", out DailyTargetId);
                            }
                            DataRow dr = dsDailyTarget.Tables[0].NewRow();

                            dr["ID"] = DailyTargetId + "-" + (i + 1).ToString();
                            dr["PlantId"] = identity.PlantId;

                            dr["ManPowerWithMachine"] = clsStaticInfo.dbl(DailyTargetData[i]["ManPowerWithMachine"]);
                            dr["ManPowerWithHand"] = clsStaticInfo.dbl(DailyTargetData[i]["ManPowerWithHand"]);
                            dr["Manpower"] = totalManpower;
                            dr["SMV"] = clsStaticInfo.dbl(DailyTargetData[i]["SMV"]);
                            dr["TotalHour"] = clsStaticInfo.dbl(DailyTargetData[i]["TotalHour"]);
                            dr["QuantityPerHour"] = clsStaticInfo.dbl(DailyTargetData[i]["QuantityPerHour"]);
                            dr["Quantity"] = (int)(clsStaticInfo.dbl(DailyTargetData[i]["QuantityPerHour"]) * clsStaticInfo.dbl(DailyTargetData[i]["TotalHour"]));
                            dr["MaterialMasterId"] = DailyTargetData[i]["MaterialMasterId"];
                            dr["MaterialMasterArticleId"] = DailyTargetData[i]["MaterialMasterArticleId"];
                            dr["EmployeeId"] = DailyTargetData[i]["EmployeeId"];

                            dr["PlantID"] = identity.PlantId;
                            dr["ProductionOrderId"] = DailyTargetData[i]["PRNo"];
                            dr["isBuildUp"] = false;
                            dr["WorkCenterMasterID"] = DailyTargetData[i]["WorkCenterMasterId"];
                            dr["TargetDate"] = TargetDate;
                            dr["IsManual"] = true;


                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dsDailyTarget.Tables[0].Rows.Add(dr);

                        }
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsDailyTarget);

                return Json(new { Error = false, Data = DailyTargetData,/* Sequence = GetSequence(),*/ Message = AplosMessage.Insert });

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
        public ActionResult GetDailyTarget(string EntityId, string ProcessId, string ProductionDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select convert(bit,1) AS Active, pod.SalesOrderId,PO.id PRNo,MMA.Id MaterialMasterArticleId,MM.Id MaterialMasterId,WCM.Id WorkCenterMasterId,
                                MM.UserName AS Material,MMA.StandardName AS Article,convert(bit,isnull(DPT.IsManual,0)) AS IsManual
                                ,DPT.ID DailyProductionTargetID,WCM.UserName Line ,DPT.ManPowerWithMachine,DPT.ManPowerWithHand,DPT.Manpower
								,DPT.SMV,DPT.Quantity,DPT.QuantityPerHour,DPT.TotalHour,DPT.TargetDate,SO.CustomerPOId,EI.EmployeeName ,DPT.EmployeeId,
								
						BuyerItemNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
															
		 							MasterOrderId =STUFF((select distinct ','+XMOI.Id from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
                                                    BuyerOrderRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnOrderRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
											                         	 trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

					                          	BuyerItemNo=STUFF((select distinct ', '+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnItemNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
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

                                                    Buyer=STUFF((select distinct ','+XB.UserName from 
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

													CustomerPONo=STUFF((select distinct ', '+CPO.PONumber from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
			                                                    where POD.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                    ,convert(bit, case when isnull(LLD.Id,'')<>'' then 1 else 0 end ) As HasLayout
                                                        ,convert(bit, case when isnull(LLD.Id,'')<>'' then 0 else 
                                                     	CASE WHEN ISNULL(LLP.Id,'')<>'' OR ISNULL(l.Id,'')<>'' THEN 1 ELSE 0 END
                                                     	 end ) As CanCopy,
prs.username as ProcessName,resp.EmployeeName as ResponsiblePersonName,pbt.Id ProductionBulletinId

                                from SCS.WorkCenterMaster WCM 
                                left join employeeinformation resp on resp.systemid=wcm.ResponsiblePersonId
                                left join hkp.process prs on prs.id=wcm.processid
                                left outer join  TRN.DailyProductionTarget DPT on dpt.WorkCenterMasterID=WCM.Id  and  DPT.TargetDate='" + ProductionDate + @"'
                                LEFT JOIN DBO.EmployeeInformation EI ON EI.SystemId=DPT.EmployeeId
                                left outer join  TRN.ProductionOrder PO on PO.Id=DPT.ProductionOrderId  
                                left join trn.ProductionOrderDetail POD ON POD.ProductionOrderId=po.Id and pod.Id=(select TOP 1 Id from TRN.ProductionOrderDetail D where D.ProductionOrderId=PO.Id)
                                left join trn.SalesOrder SO ON SO.Id=POD.SalesOrderId
                                left join trn.MasterOrderItem MOI ON MOI.Id=so.MasterOrderItemId
                                Left join LineLayoutDailyTarget LLD on LLD.WorkCenterMasterId=DPT.WorkCenterMasterId and LLD.TargetDate=DPT.TargetDate and LLD.ProductionOrderId=DPT.ProductionOrderId
                                LEFT JOIN trn.ProductionBulletinTemplate AS pbt on pbt.ProductionOrderId=PO.Id 
                                LEFT JOIN trn.ProductionBulletinTemplateMaster AS M ON m.ProductionBulletinTemplateId=pbt.Id AND m.ProcessId=WCM.ProcessId
                                LEFT JOIN LineLayoutDailyTarget LLP ON LLP.ProcessId = '" + ProcessId + @"' 
                                
                                    AND LLP.ProductionOrderId = po.Id                                
                                    AND LLP.TargetDate = (
                                        SELECT TOP 1 X.TargetDate
                                        FROM LineLayoutDailyTarget x                                
                                        WHERE x.ProductionOrderId = po.id                                
                                            AND x.ProcessId = '" + ProcessId + @"'                                
                                            AND x.WorkCenterMasterId = dpt.WorkCenterMasterID                                
                                            AND x.TargetDate < '" + ProductionDate + @"'                             
                                        ORDER BY x.TargetDate DESC
                                		)
                                	AND LLP.WorkCenterMasterId = dpt.WorkCenterMasterID
                                LEFT JOIN LineLayoutByProductionBulletin l ON l.ProcessId = '" + ProcessId + @"'
                                  AND l.ProductionOrderId = po.Id



                                left join mst.MaterialMaster MM ON MM.Id=MOI.MaterialMasterId
                                left join mst.MaterialMasterArticle MMA ON MMA.Id=MOI.ArticleId                               
                                where WCM.EntityId='" + EntityId + @"' AND WCM.ProcessId='" + ProcessId + @"' ORDER BY WCM.Sequence ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetProductionOrderPOPUp(string entityid, string processId)
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