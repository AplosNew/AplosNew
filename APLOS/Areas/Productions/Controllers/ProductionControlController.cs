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
using Syncfusion.XlsIO;
using Syncfusion.Pdf;
using Library.Service.HumanResources;
using Library.HumanResource.NewAttendanceProcess;
using Library.Data;
using Syncfusion.ExcelToPdfConverter;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionControlController : BaseController
    {
        #region Constructor
        /// <summary>   The CostingTypesService service. </summary>
        private readonly ISqlRepository _sqlRepository;
        private readonly IAttendanceManagementService _AttendanceManagementService;

        clsDailyTergatLineDesign DT = new clsDailyTergatLineDesign();
        public ProductionControlController(IAttendanceManagementService AttendanceManagementService, ISqlRepository R)
        {
            _AttendanceManagementService = AttendanceManagementService;
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
        public JsonResult Create(List<Dictionary<string, object>> DailyTargetData, string TargetDate, string ProductionDate, string EntityId, string ProcessId, string ProductionShiftId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "TRN.ProductionControl";
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
                            item["ProcessId"] = ProcessId;
                            item["EntityId"] = EntityId;
                            item["ProductionShiftId"] = ProductionShiftId;
                            item["ProductionDate"] = ProductionDate;
                            item["TargetDate"] = TargetDate;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            item["PlantId"] = identity.PlantId;
                            item["ProcessId"] = ProcessId;
                            item["EntityId"] = EntityId;
                            item["ProductionShiftId"] = ProductionShiftId;
                            item["ProductionDate"] = ProductionDate;
                            item["TargetDate"] = TargetDate;
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
        public ActionResult GetProductionControl(string EntityId, string ProcessId, string TargetDate, string ShiftId, string ProductionDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select B.Id,B.ProcessId,B.EntityId,B.ProductionShiftId,B.TargetDate,B.ProductionDate,B.WorkCenterMasterId,B.Active,B.Line,B.Production,B.ProductionOrderId,B.Article,B.ControlPeriodName,B.FieldValue
into #tempPC from 
(select A.Id,A.ProcessId,A.EntityId,A.ProductionShiftId,A.TargetDate,A.ProductionDate,A.WorkCenterMasterId,A.Active,A.Line,A.Production,A.ProductionOrderId,A.Article,A.ControlPeriodName,A.FieldValue from
(SELECT distinct pc.Id,pc.ProcessId,pc.EntityId,pc.ProductionShiftId,pc.TargetDate,pc.ProductionDate,wc.Id as WorkCenterMasterId,CAST (CASE WHEN pc.Id IS NULL THEN 0 ELSE 1 END AS bit) AS Active,wc.UserName as Line,'' Production,
                        isnull(pc.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionControl where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId='" + ShiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc)) as ProductionOrderId,
						  Article=STUFF((select distinct ','+MA.StandardName from trn.ProductionOrderDetail Pod 
                                                            left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId=so.Id
                                                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                            left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                            where Pod.ProductionOrderId=isnull(pc.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionControl where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId='" + ShiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
															CP.ControlPeriodName,CP.FieldValue
	                    FROM  SCS.WorkCenterMaster wc 
                        LEFT JOIN TRN.ProductionControl pc ON pc.WorkCenterMasterId=wc.Id AND pc.ProcessId = '" + ProcessId + @"' 
                        AND  pc.EntityId='" + EntityId + @"' AND pc.ProductionDate='" + ProductionDate + @"'  AND pc.ProductionShiftId='" + ShiftId + @"' 
                        LEFT JOIN trn.ProductionOrder AS PO ON PO.ID=isnull(pc.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionControl where ProcessId = '" + ProcessId + @"' and EntityId='" + EntityId + @"' and ProductionShiftId='" + ShiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))
                        LEFT OUTER JOIN [HKP].[ControlPeriod] CP ON 1=1
where wc.ProcessId = '" + ProcessId + @"' and wc.EntityId = '" + EntityId + @"')A 
)B order by B.Line
DECLARE @sql nvarchar(max), @col nvarchar(max)

 SELECT @col = (
 SELECT DISTINCT ',' + QUOTENAME(REPLACE(CONVERT(VARCHAR(40), ControlPeriodName, 113), ' ', '-'))

 FROM #tempPC 
                                FOR XML PATH('')
                            )                             SELECT @sql = N'
 (SELECT *
 FROM #tempPC
                            PIVOT(
 MAX([FieldValue]) FOR[ControlPeriodName] IN('+STUFF(@col,1,1,'')+')
 ) as pvt)' 

 EXEC sp_executesql @sql
 drop table #tempPC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadControlPeriodDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id,ControlPeriodName,Format(FromDate,'dd-MMM-yyyy') as FromDate,Format(ToDate,'dd-MMM-yyyy') as ToDate,format(FromTime,'hh:mm tt') as FromTime,format(ToTime,'hh:mm tt') as ToTime,Minute,Remarks,Active from [HKP].[ControlPeriod] order by SequenceNo";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createControlPeriod(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[HKP].[ControlPeriod]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "CP" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            
                            DataRow drpb = dv[0].Row;
                            if(item["Active"] is true)
                            { 
                            DateTime FromDt = Convert.ToDateTime(item["FromDate"]);
                            DateTime ToDt = Convert.ToDateTime(item["ToDate"]);
                            TimeSpan t = ToDt.Subtract(FromDt);
                            int N = t.Days;
                            TimeSpan ts;
                            DateTime date1 = Convert.ToDateTime(item["FromTime"]);
                            DateTime date2 = Convert.ToDateTime(item["ToTime"]);
                            DateTime NextDayDate = date2.AddDays(N);
                            if (FromDt == ToDt)
                            {
                                ts = date2 - date1;
                            }
                            else
                            {
                                DateTime NextDayDate2 = date2.AddDays(N);
                                ts = NextDayDate2 - date1;
                            }
                            TimeSpan Nd = NextDayDate - date1;
                            int minutes = (int)Nd.TotalMinutes;

                            if (minutes >= 720 || minutes < 0)
                            {
                                item["ToTime"] = NextDayDate;
                                item["Minute"] = Nd.TotalMinutes;
                            }
                            else
                            {
                                item["ToTime"] = date2;
                                item["Minute"] = ts.TotalMinutes;
                            }
                                item["FieldValue"] = 0;
                            EditRow(drpb, item);
                            }
                            else
                            {
                                DateTime FromDt = DateTime.Now;
                                DateTime ToDt = DateTime.Now; ;
                                TimeSpan t = ToDt.Subtract(FromDt);
                                int N = t.Days;
                                TimeSpan ts;
                                DateTime date1 = DateTime.Now;
                                DateTime date2 = DateTime.Now;
                                DateTime NextDayDate = date2.AddDays(N);
                                if (FromDt == ToDt)
                                {
                                    ts = date2 - date1;
                                }
                                else
                                {
                                    DateTime NextDayDate2 = date2.AddDays(N);
                                    ts = NextDayDate2 - date1;
                                }
                                TimeSpan Nd = NextDayDate - date1;
                                int minutes = (int)Nd.TotalMinutes;

                                if (minutes >= 720 || minutes < 0)
                                {
                                    item["ToTime"] = NextDayDate;
                                    item["Minute"] = Nd.TotalMinutes;
                                    
                                }
                                else
                                {
                                    item["ToTime"] = date2;
                                    item["Minute"] = ts.TotalMinutes;
                                    
                                }
                                item["Remarks"] = "";
                                item["FieldValue"] = "NULL";
                                EditRow(drpb, item);
                            }
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

        [HttpGet, Authorize]
        public ActionResult GetProductionJobCardReportView(string ProductionControlId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetProductionJobCardReports(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, ProductionControlId);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                return RenderReportAsPdf(workbook, reportFileName);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetRunningMachineJobCardReportView(string EntityId, string ProcessId, string TargetDate, string ShiftId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetRunningMachineJobCardReports(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, EntityId, ProcessId, TargetDate, ShiftId);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                return RenderReportAsPdf(workbook, reportFileName);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        public new ActionResult RenderReportAsPdf(IWorkbook workbook, string fileName, bool isOpen = true)
        {
            try
            {
                using (var converter = new ExcelToPdfConverter(workbook))
                {
                    var pdfDocument = new PdfDocument();
                    ExcelToPdfConverterSettings _settings = new ExcelToPdfConverterSettings();
                    _settings.AutoDetectComplexScript = true;
                    _settings.EmbedFonts = true;
                    _settings.LayoutOptions = LayoutOptions.FitAllColumnsOnOnePage;

                    pdfDocument = converter.Convert(_settings);

                    if (isOpen == true)
                        pdfDocument.Save(fileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Open);
                    else
                        pdfDocument.Save(fileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);

                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
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