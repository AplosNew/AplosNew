#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.General.TaskScheduler;
using Library.Model.Enums;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class POWiseProductionStatusReportController : BaseController
    {


        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        TasksService tasksService = new TasksService();
        public POWiseProductionStatusReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }


        //[HttpGet, Authorize]
        //public ActionResult getFiltersData(string fromDate, string todate)
        //{
        //    try
        //    {
               
        //            try
        //            {
        //                var sql = @"";

        //                //return _sqlRepository.GetDataCollection(sql);
        //            }
        //            catch (Exception e)
        //            {
        //                throw e;
        //            }

        //        //return Json(tasksService.getFiltersData(fromDate, todate), JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        [HttpPost, Authorize]
        public DataTable GetPOWiseProductionStatusData()
        {
                string strSql = "";

                strSql = @"SELECT distinct PP.Id,trke.UserName AS Entity,PP.ProductionOrderID PONo--,POPS.[Sequence] POProcessSequence 
                            ,wcm.UserName AS WorkCenter ,CPL.UserName AS ProductionShift,FORMAT(PP.ProductionDate,'dd-MMM-yyyy') AS ActualDate,pp.Quantity AS ActualQty,
                            isnull(p.UserName,FSFG.UserName) AS Process,p.Sequence StandardProcessSequence,ISNULL(pp.StandardName,ord.Article ) Article                  
                            ,ord.Product,
                            --additional info
			                     buyer=STUFF((select distinct ','+XB.UserName from 
			                            trn.SalesOrder XSO 
			                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                            where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            SalesOrderIds=STUFF((select distinct ','+XSO.Id from 
			                        trn.SalesOrder XSO 
			                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                        where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                        BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

			
		                                StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
                                        OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            pt1.NoOfWorkStation,sn.ProductionHours  AS PlanHours,
                            ISNULL(ppt.ProductionHours,0) ProductionHours
                            
                            FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId,  ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,COUNT(*) AS ProductionHours,SUM(ps.Quantity) AS Quantity
                                    FROM trn.ProductionSummary AS ps 
                                  left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
                                  LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
      		                            --WHERE ps.ProductionDate BETWEEN '01-Nov-2022' AND '30-Nov-2022' AND ps.EntityID in ('','14','15') 
      		                            --AND ps.ProcessId=(select XX.ProcessId from trn.ProductionOrderProcessSet AS XX where XX.IsBaseProcess=1 and XX.ProductionOrderID=ps.ProductionOrderId)
                                  GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId
                            ) AS pp
                            LEFT JOIN dbo.ShiftDefination CPL ON cpl.SystemId=pp.ProductionShiftId
                            LEFT JOIN trn.SalesOrder AS so ON so.Id=pp.SalesOrderId
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=pp.ProductionOrderID
                            LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID=pp.ProductionOrderId AND sn.ProductionDate=pp.ProductionDate AND sn.WorkCenterMasterId=pp.WorkCenterMasterId AND sn.EntityID=pp.EntityId
                            LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS FSFG ON FSFG.Id=pp.FromSFGInventoryId
                           
                            LEFT OUTER JOIN scs.WorkCenterMaster AS Twcm ON Twcm.Id=pp.ToWorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS TSFG ON TSFG.Id=pp.ToSFGInventoryId
                        
                            left outer join ProductionPlanningType1 AS ppt on ppt.ProductionOrderID=pp.ProductionOrderId AND ppt.WorkCenterMasterId=PP.WorkCenterMasterId AND  ppt.ProcessId=PP.ProcessId AND ppt.EntityId=pp.EntityId and ppt.ProductionDate=PP.ProductionDate
                            --left outer join ProductionPlanningCalendar AS ppc on ppc.ProcessId=PP.ProcessId AND ppc.EntityId=pp.EntityId and PPC.WorkingDate=PP.ProductionDate
                            left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
							LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
							--LEFT JOIN trn.ProductionOrderProcessSet POPS ON POPS.ProductionOrderId=PO.Id
                             left outer join (
                                                        select POD.ProductionOrderId,mm.UserName AS Material,MA.StandardName AS Article,PM.UserName AS Product,PC.UserName AS ProductCategory,
                                                          SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate* so.Qty ELSE  so.Rate* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(so.Qty) AS FOB,
                                                          SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM* so.Qty ELSE  so.CM* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(SO.Qty) AS CM
                                                        from trn.ProductionOrderDetail POD 
                                                        left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                                                        left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                        left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                                                        left join MasterOrderExchangeRates RT ON RT.TransactionId=MO.Id
                                                        left JOIN org.Company AS com ON com.Id=mo.CompanyId
                                                        LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN ('','14','15'))
                                                        LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN ('','14','15'))
                                                        LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId
                                                        left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                                        LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                        left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                        left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                        left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                                        group by mm.UserName,MA.StandardName,PM.UserName,PC.UserName,POD.ProductionOrderId
                                              ) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId";

                return _sqlRepository.GetDataTable(strSql);

                //var jsondata = Json(CustomJsonResultService.DataTableToJson(dtTask), JsonRequestBehavior.AllowGet);
                //jsondata.MaxJsonLength = int.MaxValue;
                //return jsondata;
            
        }
       // public DataTable GetPOWPSData()
       // {
       //     string strSql = "";

       //     strSql = @"SELECT distinct PP.Id,trke.UserName AS Entity,PP.ProductionOrderID PONo--,POPS.[Sequence] POProcessSequence 
       //                     ,wcm.UserName AS WorkCenter ,CPL.UserName AS ProductionShift,FORMAT(PP.ProductionDate,'dd-MMM-yyyy') AS ActualDate,pp.Quantity AS ActualQty,
       //                     isnull(p.UserName,FSFG.UserName) AS Process,p.Sequence StandardProcessSequence,ISNULL(pp.StandardName,ord.Article ) Article                  
       //                     ,ord.Product,
       //                     --additional info
			    //                 buyer=STUFF((select distinct ','+XB.UserName from 
			    //                        trn.SalesOrder XSO 
			    //                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			    //                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			    //                        left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			    //                        left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			    //                        where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       //                     SalesOrderIds=STUFF((select distinct ','+XSO.Id from 
			    //                    trn.SalesOrder XSO 
			    //                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			    //                    where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

       //                                 BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
			    //                            trn.SalesOrder XSO 
			    //                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			    //                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			    //                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			    //                            where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       //                              OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
			    //                            trn.SalesOrder XSO 
			    //                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			    //                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			    //                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			    //                            where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

			
		     //                           StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
			    //                            trn.SalesOrder XSO 
			    //                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			    //                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			    //                            where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
       //                                 OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
			    //                            trn.SalesOrder XSO 
			    //                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			    //                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			    //                            where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       //                     pt1.NoOfWorkStation,sn.ProductionHours  AS PlanHours,
       //                     ISNULL(ppt.ProductionHours,0) ProductionHours
                            
       //                     FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId,  ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,COUNT(*) AS ProductionHours,SUM(ps.Quantity) AS Quantity
       //                             FROM trn.ProductionSummary AS ps 
       //                           left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
       //                           LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
      	//	                            --WHERE ps.ProductionDate BETWEEN '01-Nov-2022' AND '30-Nov-2022' AND ps.EntityID in ('','14','15') 
      	//	                            --AND ps.ProcessId=(select XX.ProcessId from trn.ProductionOrderProcessSet AS XX where XX.IsBaseProcess=1 and XX.ProductionOrderID=ps.ProductionOrderId)
       //                           GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId
       //                     ) AS pp
       //                     LEFT JOIN dbo.ShiftDefination CPL ON cpl.SystemId=pp.ProductionShiftId
       //                     LEFT JOIN trn.SalesOrder AS so ON so.Id=pp.SalesOrderId
       //                     LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=pp.ProductionOrderID
       //                     LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID=pp.ProductionOrderId AND sn.ProductionDate=pp.ProductionDate AND sn.WorkCenterMasterId=pp.WorkCenterMasterId AND sn.EntityID=pp.EntityId
       //                     LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
       //                     LEFT OUTER JOIN hkp.SFGInventory AS FSFG ON FSFG.Id=pp.FromSFGInventoryId
                           
       //                     LEFT OUTER JOIN scs.WorkCenterMaster AS Twcm ON Twcm.Id=pp.ToWorkCenterMasterId
       //                     LEFT OUTER JOIN hkp.SFGInventory AS TSFG ON TSFG.Id=pp.ToSFGInventoryId
                        
       //                     left outer join ProductionPlanningType1 AS ppt on ppt.ProductionOrderID=pp.ProductionOrderId AND ppt.WorkCenterMasterId=PP.WorkCenterMasterId AND  ppt.ProcessId=PP.ProcessId AND ppt.EntityId=pp.EntityId and ppt.ProductionDate=PP.ProductionDate
       //                     --left outer join ProductionPlanningCalendar AS ppc on ppc.ProcessId=PP.ProcessId AND ppc.EntityId=pp.EntityId and PPC.WorkingDate=PP.ProductionDate
       //                     left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
							//LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
       //                     LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
       //                     LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
							//--LEFT JOIN trn.ProductionOrderProcessSet POPS ON POPS.ProductionOrderId=PO.Id
       //                      left outer join (
       //                                                 select POD.ProductionOrderId,mm.UserName AS Material,MA.StandardName AS Article,PM.UserName AS Product,PC.UserName AS ProductCategory,
       //                                                   SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate* so.Qty ELSE  so.Rate* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(so.Qty) AS FOB,
       //                                                   SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM* so.Qty ELSE  so.CM* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(SO.Qty) AS CM
       //                                                 from trn.ProductionOrderDetail POD 
       //                                                 left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
       //                                                 left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
       //                                                 left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
       //                                                 left join MasterOrderExchangeRates RT ON RT.TransactionId=MO.Id
       //                                                 left JOIN org.Company AS com ON com.Id=mo.CompanyId
       //                                                 LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN ('','14','15'))
       //                                                 LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN ('','14','15'))
       //                                                 LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId
       //                                                 left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
       //                                                 LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
       //                                                 left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
       //                                                 left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
       //                                                 left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
       //                                                 group by mm.UserName,MA.StandardName,PM.UserName,PC.UserName,POD.ProductionOrderId
       //                                       ) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId";

       //     return _sqlRepository.GetDataTable(strSql);
       // }

        [HttpPost, Authorize]
        public ActionResult GetTaskManagementReport(Dictionary<string, string> parameters, string fromDate, string todate, Dictionary<string, string> model, string EmpIds)
        {

            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string fileName = "";

                if (model["State"] == "EmployeeWise")
                {
                    fileName = GetTaskManagementReportXL(parameters, fromDate, todate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, "Task", model, EmpIds);
                }
                else if (model["State"] == "DepartmentWise")
                {
                    fileName = GetTaskManagementDeptReportXL(parameters, fromDate, todate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, "Task", model);
                }
                else
                {
                    fileName = GetTaskManagementDesignationGroupReportXL(parameters, fromDate, todate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, "Task", model);
                }

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


        }

        public string GetTaskManagementReportXL(Dictionary<string, string> parameters, string fromDate, string todate, string CompanyGroupId, string CompanyId, string PlantId, string SheetName, Dictionary<string, string> model, string EmpIds)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                var reportUtility = new ReportUtility();
                workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Worksheets[0].Name = SheetName;
                sheet = workbook.Worksheets[0];
                DataTable data;


                //Add rich-text Excel comment
                IFont fontCaption = workbook.CreateFont();
                fontCaption.Size = 8f;
                IFont fontRegular = workbook.CreateFont();
                fontRegular.Italic = true;
                fontRegular.Size = 6f;

                // ExcelEngine excelEngine = null;

                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + CompanyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtTask = null;
                dtTask = tasksService.GetTaskManagementData(fromDate, todate, parameters, model, EmpIds);
                if (dtTask.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(CompanyId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(PlantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                #region Header
                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                int StartRow = xlsRow;
                sheet1[xlsRow, xlsCol].Text = "SL. No";
                int colSLNO = xlsCol;
                sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                xlsCol++;

                int iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Employee Code";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol++;

                int iEmployeeName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Employee Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                int iDesignation = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Designation";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                int colDepartment = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Department";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                //xlsCol++;

                //xlsCol++;
                //int iTaskCreated = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Task Created";

                //xlsCol++;
                //int iPerTotalTask = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "% Of Total Task";
                //IRange range = sheet1[xlsRow, xlsCol];
                //ICommentShape shape = range.AddComment();
                //shape.RichText.Append("Emp Task Created FP / Total Task Created  FP", fontCaption);
                //shape.IsTextLocked = false;
                //shape.AutoSize = false;

               

                xlsCol++;

                int iTaskDue = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Due";

                xlsCol++;

                int iPerOfDueTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "% Of Due Task";
                IRange range1 = sheet1[xlsRow, xlsCol];
                ICommentShape shape1 = range1.AddComment();
                shape1.RichText.Append("Emp Due Task FP / Total Due Task FP", fontCaption);
                shape1.IsTextLocked = false;
                shape1.AutoSize = false;


                xlsCol++;
                int iTaskUnread = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Unread";
                
                xlsCol++;
                int iTaskCompletedOnTime = xlsCol;
                sheet1[xlsRow, xlsCol].Text = "Task Completed-On Time";


                xlsCol++;
                int iTaskCompletedLate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Completed-Late";

                xlsCol++;
                int iEarlyTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Early Task";


                xlsCol++;
                int iTaskCompletedFP = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Completed FP";


                xlsCol++;
                int iOverdueTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Overdue Task";
                //IRange rangeODT = sheet1[xlsRow, xlsCol];
                //ICommentShape shapeODT = rangeODT.AddComment();
                //shapeODT.RichText.Append("TaskDue - OnTimeTask - LateTasks", fontCaption);
                //shapeODT.IsTextLocked = false;
                //shapeODT.AutoSize = false;



                xlsCol++;
                int iPeriviousPeriodOverdueTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Perivious Period Overdue Task";

                xlsCol++;
                int iPerformance = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Performance";
                IRange range2 = sheet1[xlsRow, xlsCol];
                ICommentShape shape2 = range2.AddComment();
                shape2.RichText.Append("(Task Completed On Time*2+Task Completed Late*1+Early Task*2)-Task Unread", fontCaption);
                shape2.IsTextLocked = false;
                shape2.AutoSize = false;

                xlsCol++;
                int iTotalStoryPoints = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Total Story Points";

                xlsCol++;
                int iCompletedStoryPoints = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Completed Story Points";

                xlsCol++;
                int iCheckBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "CheckBy";

                xlsCol++;
                int iCrossCheckBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "CrossCheckBy";

                xlsCol++;
                int iApproveBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "ApproveBy";

                endXlsCol = xlsCol;


                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 6, xlsRow, endXlsCol].ColumnWidth = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 38;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Color = ExcelKnownColors.Black;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Thick);
                #endregion

                string voucherNo = "";
                /// string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                // string totalFormula = "";

                //string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;

                //double TotalCreatedTask = clsStaticInfo.dbl(dtTask.Compute("SUM(CreatedTask)", null));
                double TotalTaskDue = clsStaticInfo.dbl(dtTask.Compute("SUM(TaskDue)", null));

                for (int i = 0; i < dtTask.Rows.Count; i++)
                {
                    sheet1[xlsRow, colSLNO].Number = (i + 1);
                    sheet1.Range[xlsRow, iEmployeeCode].Text = dtTask.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, iEmployeeName].Text = dtTask.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, iDesignation].Text = dtTask.Rows[i]["LegalDesignation"].ToString();
                    sheet1.Range[xlsRow, colDepartment].Text = dtTask.Rows[i]["Department"].ToString();
                    //sheet1.Range[xlsRow, iTaskCreated].Number = clsStaticInfo.dbl(dtTask.Rows[i]["CreatedTask"].ToString());

                    //double PerTotalTask = Math.Round((Convert.ToDouble(dtTask.Rows[i]["CreatedTask"].ToString()) / TotalCreatedTask) * 100, 2);
                    //sheet1.Range[xlsRow, iPerTotalTask].Number = Math.Round(PerTotalTask);//formula 
                    //sheet1.Range[xlsRow, iPerTotalTask].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet1.Range[xlsRow, iTaskUnread].Number = clsStaticInfo.dbl(dtTask.Rows[i]["UnRead"].ToString());
                    sheet1.Range[xlsRow, iTaskDue].Number = clsStaticInfo.dbl(dtTask.Rows[i]["TaskDue"].ToString());

                    double PerTaskDue = Math.Round((clsStaticInfo.dbl(dtTask.Rows[i]["TaskDue"].ToString()) / TotalTaskDue) * 100, 2);
                    sheet1.Range[xlsRow, iPerOfDueTask].Number = clsStaticInfo.dbl(Math.Round(PerTaskDue));//formula 
                    sheet1.Range[xlsRow, iPerOfDueTask].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet1.Range[xlsRow, iTaskCompletedOnTime].Number = clsStaticInfo.dbl(dtTask.Rows[i]["OnTimeTask"].ToString());
                    sheet1.Range[xlsRow, iTaskCompletedLate].Number = clsStaticInfo.dbl(dtTask.Rows[i]["LateTask"].ToString());

                    sheet1[xlsRow, iEarlyTask].Number = clsStaticInfo.dbl(dtTask.Rows[i]["EarlyTask"].ToString());
                    sheet1[xlsRow, iTaskCompletedFP].Number = clsStaticInfo.dbl(dtTask.Rows[i]["OnTimeTask"].ToString()) + clsStaticInfo.dbl(dtTask.Rows[i]["LateTask"].ToString()) + clsStaticInfo.dbl(dtTask.Rows[i]["EarlyTask"].ToString());
                    sheet1[xlsRow, iOverdueTask].Number = clsStaticInfo.dbl(dtTask.Rows[i]["OverdueTask"].ToString());

                    sheet1[xlsRow, iPeriviousPeriodOverdueTask].Number = clsStaticInfo.dbl(dtTask.Rows[i]["PeriviousPeriodOverdueTask"].ToString());

                    sheet1[xlsRow, iPerformance].Number = (((Convert.ToDouble(dtTask.Rows[i]["OnTimeTask"].ToString()) * 2) + (Convert.ToDouble(dtTask.Rows[i]["LateTask"].ToString()) * 1) + (Convert.ToDouble(dtTask.Rows[i]["EarlyTask"].ToString()) * 2)) - (Convert.ToDouble(dtTask.Rows[i]["UnRead"].ToString())));//formula

                    sheet1[xlsRow, iTotalStoryPoints].Number = clsStaticInfo.dbl(dtTask.Rows[i]["TaskDue"].ToString()) * 2;

                    sheet1[xlsRow, iCompletedStoryPoints].Number = (clsStaticInfo.dbl(dtTask.Rows[i]["OnTimeTask"].ToString()) + clsStaticInfo.dbl(dtTask.Rows[i]["LateTask"].ToString()) + clsStaticInfo.dbl(dtTask.Rows[i]["EarlyTask"].ToString())) * 2;

                    sheet1.Range[xlsRow, iCheckBy].Number = clsStaticInfo.dbl(dtTask.Rows[i]["CheckBy"].ToString());
                    sheet1.Range[xlsRow, iCrossCheckBy].Number = clsStaticInfo.dbl(dtTask.Rows[i]["CrossCheckBy"].ToString());
                    sheet1.Range[xlsRow, iApproveBy].Number = clsStaticInfo.dbl(dtTask.Rows[i]["ApproveBy"].ToString());

                    xlsRow++;
                }
                sheet1.Range[xlsRow, 5].Text = "TOTAL";
                sheet1.Range[xlsRow, 5].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, 5].CellStyle.Font.Bold = true;

                sheet.Range[xlsRow, colSLNO, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Thick);
                sheet.Range[startRow, colSLNO, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Thick);

                //formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskCreated) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskCreated) + (xlsRow - 1) + ")";
                //sheet1[xlsRow, iTaskCreated, xlsRow, iTaskCreated].Formula = formula;
                //sheet1.Range[xlsRow, iTaskCreated, xlsRow, iTaskCreated].CellStyle.Font.Bold = true;

                //formula = "SUM(" + clsStaticInfo.GetxlsCol(iPerTotalTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPerTotalTask) + (xlsRow - 1) + ")";
                //sheet1[xlsRow, iPerTotalTask, xlsRow, iPerTotalTask].Formula = formula;
                //sheet1.Range[xlsRow, iPerTotalTask, xlsRow, iPerTotalTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskUnread) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskUnread) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskUnread, xlsRow, iTaskUnread].Formula = formula;
                sheet1.Range[xlsRow, iTaskUnread, xlsRow, iTaskUnread].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskDue) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskDue) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskDue, xlsRow, iTaskDue].Formula = formula;
                sheet1.Range[xlsRow, iTaskDue, xlsRow, iTaskDue].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iPerOfDueTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPerOfDueTask) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iPerOfDueTask, xlsRow, iPerOfDueTask].Formula = formula;
                sheet1.Range[xlsRow, iPerOfDueTask, xlsRow, iPerOfDueTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskCompletedOnTime) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskCompletedOnTime) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskCompletedOnTime, xlsRow, iTaskCompletedOnTime].Formula = formula;
                sheet1.Range[xlsRow, iTaskCompletedOnTime, xlsRow, iTaskCompletedOnTime].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskCompletedLate) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskCompletedLate) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskCompletedLate, xlsRow, iTaskCompletedLate].Formula = formula;
                sheet1.Range[xlsRow, iTaskCompletedLate, xlsRow, iTaskCompletedLate].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iOverdueTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iOverdueTask) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iOverdueTask, xlsRow, iOverdueTask].Formula = formula;
                sheet1.Range[xlsRow, iOverdueTask, xlsRow, iOverdueTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iEarlyTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iEarlyTask) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iEarlyTask, xlsRow, iEarlyTask].Formula = formula;
                sheet1.Range[xlsRow, iEarlyTask, xlsRow, iEarlyTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iPeriviousPeriodOverdueTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPeriviousPeriodOverdueTask) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iPeriviousPeriodOverdueTask, xlsRow, iPeriviousPeriodOverdueTask].Formula = formula;
                sheet1.Range[xlsRow, iPeriviousPeriodOverdueTask, xlsRow, iPeriviousPeriodOverdueTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iPerformance) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPerformance) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iPerformance, xlsRow, iPerformance].Formula = formula;
                sheet1.Range[xlsRow, iPerformance, xlsRow, iPerformance].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTotalStoryPoints) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTotalStoryPoints) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTotalStoryPoints, xlsRow, iTotalStoryPoints].Formula = formula;
                sheet1.Range[xlsRow, iTotalStoryPoints, xlsRow, iTotalStoryPoints].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iCompletedStoryPoints) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iCompletedStoryPoints) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iCompletedStoryPoints, xlsRow, iCompletedStoryPoints].Formula = formula;
                sheet1.Range[xlsRow, iCompletedStoryPoints, xlsRow, iCompletedStoryPoints].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskCompletedFP) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskCompletedFP) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskCompletedFP, xlsRow, iTaskCompletedFP].Formula = formula;
                sheet1.Range[xlsRow, iTaskCompletedFP, xlsRow, iTaskCompletedFP].CellStyle.Font.Bold = true;
              

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iCheckBy) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iCheckBy) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iCheckBy, xlsRow, iCheckBy].Formula = formula;
                sheet1.Range[xlsRow, iCheckBy, xlsRow, iCheckBy].CellStyle.Font.Bold = true;


                formula = "SUM(" + clsStaticInfo.GetxlsCol(iCrossCheckBy) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iCrossCheckBy) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iCrossCheckBy, xlsRow, iCrossCheckBy].Formula = formula;
                sheet1.Range[xlsRow, iCrossCheckBy, xlsRow, iCrossCheckBy].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iApproveBy) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iApproveBy) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iApproveBy, xlsRow, iApproveBy].Formula = formula;
                sheet1.Range[xlsRow, iApproveBy, xlsRow, iApproveBy].CellStyle.Font.Bold = true;


                sheet1.AutoFilters.FilterRange = sheet1.Range[StartRow - 1, 1, xlsRow, endXlsCol];
                #region ******************Report Header******************

                xlsRow = 1;
                FactoryName = string.Empty;
                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Management Report From Date: " + fromDate + " To Date: " + todate;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + SheetName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskManagementReportEmployeeWise.xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }

        public string GetTaskManagementDeptReportXL(Dictionary<string, string> parameters, string fromDate, string todate, string CompanyGroupId, string CompanyId, string PlantId, string SheetName, Dictionary<string, string> model)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                var reportUtility = new ReportUtility();
                workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Worksheets[0].Name = SheetName;
                sheet = workbook.Worksheets[0];
                DataTable data;


                //Add rich-text Excel comment
                IFont fontCaption = workbook.CreateFont();
                fontCaption.Size = 8f;
                IFont fontRegular = workbook.CreateFont();
                fontRegular.Italic = true;
                fontRegular.Size = 6f;

                // ExcelEngine excelEngine = null;

                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + CompanyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtTask = null;
                dtTask = tasksService.GetTaskManagementDepartmentData(fromDate, todate, parameters, model);
                if (dtTask.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(CompanyId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(PlantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                #region Header
                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                int StartRow = xlsRow;
                sheet1[xlsRow, xlsCol].Text = "SL. No";
                int colSLNO = xlsCol;
                sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;


                int iDesignation = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Department";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                int colNoOfEmp = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "No Of Employee";

                //xlsCol++;
                //int iTaskCreated = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Task Created";

                //xlsCol++;
                //int iPerTotalTask = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "% Of Total Task";
                //IRange range = sheet1[xlsRow, xlsCol];
                //ICommentShape shape = range.AddComment();
                //shape.RichText.Append("Emp Task Created FP / Total Task Created  FP", fontCaption);
                //shape.IsTextLocked = false;
                //shape.AutoSize = false;

              

                xlsCol++;

                int iTaskDue = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Due";

                xlsCol++;

                int iPerOfDueTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "% Of Due Task";
                IRange range1 = sheet1[xlsRow, xlsCol];
                ICommentShape shape1 = range1.AddComment();
                shape1.RichText.Append("Emp Due Task FP / Total Due Task FP", fontCaption);
                shape1.IsTextLocked = false;
                shape1.AutoSize = false;
                xlsCol++;

                int iTaskUnread = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Unread";

                int iTaskCompletedOnTime = xlsCol;
                xlsCol++;
                sheet1[xlsRow, xlsCol].Text = "Task Completed-On Time";


                xlsCol++;
                int iTaskCompletedLate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Completed-Late";

                xlsCol++;
                int iEarlyTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Eary Task";

                xlsCol++;
                int iTaskCompletedFP = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Completed FP";

                xlsCol++;

                int iOverdueTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Overdue Task";
                //IRange rangeODT = sheet1[xlsRow, xlsCol];
                //ICommentShape shapeODT = rangeODT.AddComment();
                //shapeODT.RichText.Append("TaskDue - OnTimeTask - LateTasks", fontCaption);
                //shapeODT.IsTextLocked = false;
                //shapeODT.AutoSize = false;

                xlsCol++;
                int iPeriviousPeriodOverdueTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Perivious Period Overdue Task";


                xlsCol++;
                int iPerformance = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Performance";
                IRange range2 = sheet1[xlsRow, xlsCol];
                ICommentShape shape2 = range2.AddComment();
                shape2.RichText.Append("(Task Completed On Time*2+Task Completed Late*1+Early Task*2)-Task Unread", fontCaption);
                shape2.IsTextLocked = false;
                shape2.AutoSize = false;

                xlsCol++;
                int iTotalStoryPoints = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Total Story Points";

                xlsCol++;
                int iCompletedStoryPoints = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Completed Story Points";

                xlsCol++;
                int iCheckBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "CheckBy";

                xlsCol++;
                int iCrossCheckBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "CrossCheckBy";

                xlsCol++;
                int iApproveBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "ApproveBy";

                //xlsCol++;
                //int iAvgStorypoints = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "AvgStorypoints";
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, colNoOfEmp, xlsRow, endXlsCol].ColumnWidth = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 38;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Color = ExcelKnownColors.Black;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Thick);

                #endregion
                string voucherNo = "";
                /// string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                // string totalFormula = "";

                //string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;

                //double TotalCreatedTask = clsStaticInfo.dbl(dtTask.Compute("SUM(CreatedTask)", null));
                double TotalTaskDue = clsStaticInfo.dbl(dtTask.Compute("SUM(TaskDue)", null));

                for (int i = 0; i < dtTask.Rows.Count; i++)
                {
                    sheet1[xlsRow, colSLNO].Number = (i + 1);
                    sheet1.Range[xlsRow, iDesignation].Text = dtTask.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, colNoOfEmp].Number = clsStaticInfo.dbl(dtTask.Rows[i]["NoOfEmp"].ToString());
                    //sheet1.Range[xlsRow, iTaskCreated].Number = clsStaticInfo.dbl(dtTask.Rows[i]["CreatedTask"].ToString());

                    //double PerTotalTask = Math.Round((Convert.ToDouble(dtTask.Rows[i]["CreatedTask"].ToString()) / TotalCreatedTask) * 100,2);
                    //sheet1.Range[xlsRow, iPerTotalTask].Number = Math.Round(PerTotalTask);//formula 
                    //sheet1.Range[xlsRow, iPerTotalTask].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet1.Range[xlsRow, iTaskUnread].Number = clsStaticInfo.dbl(dtTask.Rows[i]["UnRead"].ToString());
                    sheet1.Range[xlsRow, iTaskDue].Number = clsStaticInfo.dbl(dtTask.Rows[i]["TaskDue"].ToString());

                    double PerTaskDue = Math.Round((Convert.ToDouble(dtTask.Rows[i]["TaskDue"].ToString()) / TotalTaskDue) * 100,2);
                    sheet1.Range[xlsRow, iPerOfDueTask].Number = Math.Round(PerTaskDue);//formula 
                    sheet1.Range[xlsRow, iPerOfDueTask].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet1.Range[xlsRow, iTaskCompletedOnTime].Number = clsStaticInfo.dbl(dtTask.Rows[i]["OnTimeTask"].ToString());
                    sheet1.Range[xlsRow, iTaskCompletedLate].Number = clsStaticInfo.dbl(dtTask.Rows[i]["LateTask"].ToString());
                    sheet1[xlsRow, iEarlyTask].Number = clsStaticInfo.dbl(dtTask.Rows[i]["EarlyTask"].ToString());
                    sheet1[xlsRow, iTaskCompletedFP].Number = clsStaticInfo.dbl(dtTask.Rows[i]["OnTimeTask"].ToString()) + clsStaticInfo.dbl(dtTask.Rows[i]["LateTask"].ToString()) + clsStaticInfo.dbl(dtTask.Rows[i]["EarlyTask"].ToString());

                    sheet1[xlsRow, iOverdueTask].Number = clsStaticInfo.dbl(dtTask.Rows[i]["OverdueTask"].ToString());

                    sheet1[xlsRow, iPeriviousPeriodOverdueTask].Number = clsStaticInfo.dbl(dtTask.Rows[i]["PeriviousPeriodOverdueTask"].ToString());

                    sheet1[xlsRow, iPerformance].Number = (((Convert.ToDouble(dtTask.Rows[i]["OnTimeTask"].ToString()) * 2) + (Convert.ToDouble(dtTask.Rows[i]["LateTask"].ToString()) * 1) + (Convert.ToDouble(dtTask.Rows[i]["EarlyTask"].ToString()) * 2)) - (Convert.ToDouble(dtTask.Rows[i]["UnRead"].ToString())));//formula

                    sheet1[xlsRow, iTotalStoryPoints].Number = clsStaticInfo.dbl(dtTask.Rows[i]["TaskDue"].ToString()) * 2;
                    sheet1[xlsRow, iCompletedStoryPoints].Number = (clsStaticInfo.dbl(dtTask.Rows[i]["OnTimeTask"].ToString()) + clsStaticInfo.dbl(dtTask.Rows[i]["LateTask"].ToString()) + clsStaticInfo.dbl(dtTask.Rows[i]["EarlyTask"].ToString())) * 2;
                    //sheet1[xlsRow, iAvgStorypoints].Number = clsStaticInfo.dbl(dtTask.Rows[i]["AvgStorypoints"].ToString());
                    sheet1.Range[xlsRow, iCheckBy].Number = clsStaticInfo.dbl(dtTask.Rows[i]["CheckBy"].ToString());
                    sheet1.Range[xlsRow, iCrossCheckBy].Number = clsStaticInfo.dbl(dtTask.Rows[i]["CrossCheckBy"].ToString());
                    sheet1.Range[xlsRow, iApproveBy].Number = clsStaticInfo.dbl(dtTask.Rows[i]["ApproveBy"].ToString());
                    xlsRow++;
                }
                sheet1.Range[xlsRow, 2].Text = "TOTAL";
                sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;

                sheet.Range[xlsRow, colSLNO, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Thick);
                sheet.Range[startRow, colSLNO, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Thick);

                #region SUM
                formula = "SUM(" + clsStaticInfo.GetxlsCol(colNoOfEmp) + perStartRow + ":" + clsStaticInfo.GetxlsCol(colNoOfEmp) + (xlsRow - 1) + ")";
                sheet1[xlsRow, colNoOfEmp, xlsRow, colNoOfEmp].Formula = formula;
                sheet1.Range[xlsRow, colNoOfEmp, xlsRow, colNoOfEmp].CellStyle.Font.Bold = true;

                //formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskCreated) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskCreated) + (xlsRow - 1) + ")";
                //sheet1[xlsRow, iTaskCreated, xlsRow, iTaskCreated].Formula = formula;
                //sheet1.Range[xlsRow, iTaskCreated, xlsRow, iTaskCreated].CellStyle.Font.Bold = true;

                //formula = "SUM(" + clsStaticInfo.GetxlsCol(iPerTotalTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPerTotalTask) + (xlsRow - 1) + ")";
                //sheet1[xlsRow, iPerTotalTask, xlsRow, iPerTotalTask].Formula = formula;
                //sheet1.Range[xlsRow, iPerTotalTask, xlsRow, iPerTotalTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskUnread) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskUnread) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskUnread, xlsRow, iTaskUnread].Formula = formula;
                sheet1.Range[xlsRow, iTaskUnread, xlsRow, iTaskUnread].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskDue) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskDue) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskDue, xlsRow, iTaskDue].Formula = formula;
                sheet1.Range[xlsRow, iTaskDue, xlsRow, iTaskDue].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iPerOfDueTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPerOfDueTask) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iPerOfDueTask, xlsRow, iPerOfDueTask].Formula = formula;
                sheet1.Range[xlsRow, iPerOfDueTask, xlsRow, iPerOfDueTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskCompletedOnTime) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskCompletedOnTime) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskCompletedOnTime, xlsRow, iTaskCompletedOnTime].Formula = formula;
                sheet1.Range[xlsRow, iTaskCompletedOnTime, xlsRow, iTaskCompletedOnTime].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskCompletedLate) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskCompletedLate) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskCompletedLate, xlsRow, iTaskCompletedLate].Formula = formula;
                sheet1.Range[xlsRow, iTaskCompletedLate, xlsRow, iTaskCompletedLate].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iOverdueTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iOverdueTask) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iOverdueTask, xlsRow, iOverdueTask].Formula = formula;
                sheet1.Range[xlsRow, iOverdueTask, xlsRow, iOverdueTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iEarlyTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iEarlyTask) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iEarlyTask, xlsRow, iEarlyTask].Formula = formula;
                sheet1.Range[xlsRow, iEarlyTask, xlsRow, iEarlyTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iPeriviousPeriodOverdueTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPeriviousPeriodOverdueTask) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iPeriviousPeriodOverdueTask, xlsRow, iPeriviousPeriodOverdueTask].Formula = formula;
                sheet1.Range[xlsRow, iPeriviousPeriodOverdueTask, xlsRow, iPeriviousPeriodOverdueTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iPerformance) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPerformance) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iPerformance, xlsRow, iPerformance].Formula = formula;
                sheet1.Range[xlsRow, iPerformance, xlsRow, iPerformance].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTotalStoryPoints) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTotalStoryPoints) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTotalStoryPoints, xlsRow, iTotalStoryPoints].Formula = formula;
                sheet1.Range[xlsRow, iTotalStoryPoints, xlsRow, iTotalStoryPoints].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iCompletedStoryPoints) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iCompletedStoryPoints) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iCompletedStoryPoints, xlsRow, iCompletedStoryPoints].Formula = formula;
                sheet1.Range[xlsRow, iCompletedStoryPoints, xlsRow, iCompletedStoryPoints].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskCompletedFP) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskCompletedFP) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskCompletedFP, xlsRow, iTaskCompletedFP].Formula = formula;
                sheet1.Range[xlsRow, iTaskCompletedFP, xlsRow, iTaskCompletedFP].CellStyle.Font.Bold = true;

                //formula = "SUM(" + clsStaticInfo.GetxlsCol(iAvgStorypoints) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iAvgStorypoints) + (xlsRow - 1) + ")";
                //sheet1[xlsRow, iAvgStorypoints, xlsRow, iAvgStorypoints].Formula = formula;
                //sheet1.Range[xlsRow, iAvgStorypoints, xlsRow, iAvgStorypoints].CellStyle.Font.Bold = true;
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iCheckBy) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iCheckBy) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iCheckBy, xlsRow, iCheckBy].Formula = formula;
                sheet1.Range[xlsRow, iCheckBy, xlsRow, iCheckBy].CellStyle.Font.Bold = true;


                formula = "SUM(" + clsStaticInfo.GetxlsCol(iCrossCheckBy) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iCrossCheckBy) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iCrossCheckBy, xlsRow, iCrossCheckBy].Formula = formula;
                sheet1.Range[xlsRow, iCrossCheckBy, xlsRow, iCrossCheckBy].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iApproveBy) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iApproveBy) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iApproveBy, xlsRow, iApproveBy].Formula = formula;
                sheet1.Range[xlsRow, iApproveBy, xlsRow, iApproveBy].CellStyle.Font.Bold = true;

                #endregion

                sheet1.AutoFilters.FilterRange = sheet1.Range[StartRow - 1, 1, xlsRow, endXlsCol];

                #region ******************Report Header******************

                xlsRow = 1;
                FactoryName = string.Empty;
                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Management Report From Date: " + fromDate + " To Date: " + todate;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + SheetName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskManagementReportDepartmentWise .xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }

        public string GetTaskManagementDesignationGroupReportXL(Dictionary<string, string> parameters, string fromDate, string todate, string CompanyGroupId, string CompanyId, string PlantId, string SheetName, Dictionary<string, string> model)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                var reportUtility = new ReportUtility();
                workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Worksheets[0].Name = SheetName;
                sheet = workbook.Worksheets[0];
                DataTable data;


                //Add rich-text Excel comment
                IFont fontCaption = workbook.CreateFont();
                fontCaption.Size = 8f;
                IFont fontRegular = workbook.CreateFont();
                fontRegular.Italic = true;
                fontRegular.Size = 6f;

                // ExcelEngine excelEngine = null;

                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + CompanyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtTask = null;
                dtTask = tasksService.GetTaskManagementDesignatinGroupData(fromDate, todate, parameters, model);
                if (dtTask.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(CompanyId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(PlantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                #region Header

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;
                int StartRow = xlsRow;
                sheet1[xlsRow, xlsCol].Text = "SL. No";
                int colSLNO = xlsCol;
                sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;


                int iDesignation = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Designation Group";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                int colNoOfEmp = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "No Of Employee";

                //xlsCol++;
                //int iTaskCreated = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Task Created";

                //xlsCol++;
                //int iPerTotalTask = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "% Of Total Task";
                //IRange range = sheet1[xlsRow, xlsCol];
                //ICommentShape shape = range.AddComment();
                //shape.RichText.Append("Emp Task Created FP / Total Task Created  FP", fontCaption);
                //shape.IsTextLocked = false;
                //shape.AutoSize = false;

               

                xlsCol++;

                int iTaskDue = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Due";

                xlsCol++;

                int iPerOfDueTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "% Of Due Task";
                IRange range1 = sheet1[xlsRow, xlsCol];
                ICommentShape shape1 = range1.AddComment();
                shape1.RichText.Append("Emp Due Task FP / Total Due Task FP", fontCaption);
                shape1.IsTextLocked = false;
                shape1.AutoSize = false;

                xlsCol++;
                int iTaskUnread = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Unread";
                xlsCol++;

                int iTaskCompletedOnTime = xlsCol;
                sheet1[xlsRow, xlsCol].Text = "Task Completed-On Time";


                xlsCol++;
                int iTaskCompletedLate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Completed-Late";

                xlsCol++;
                int iEarlyTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Early Task";

                xlsCol++;
                int iTaskCompletedFP = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Completed FP";
                xlsCol++;

                int iOverdueTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Overdue Task";
                //IRange rangeODT = sheet1[xlsRow, xlsCol];
                //ICommentShape shapeODT = rangeODT.AddComment();
                //shapeODT.RichText.Append("TaskDue - OnTimeTask - LateTasks", fontCaption);
                //shapeODT.IsTextLocked = false;
                //shapeODT.AutoSize = false;





                xlsCol++;
                int iPeriviousPeriodOverdueTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Perivious Period Overdue Task";


                xlsCol++;
                int iPerformance = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Performance";
                IRange range2 = sheet1[xlsRow, xlsCol];
                ICommentShape shape2 = range2.AddComment();
                shape2.RichText.Append("(Task Completed On Time*2+Task Completed Late*1+Early Task*2)-Task Unread", fontCaption);
                shape2.IsTextLocked = false;
                shape2.AutoSize = false;

                xlsCol++;
                int iTotalStoryPoints = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Total Story Points";

                xlsCol++;
                int iCompletedStoryPoints = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Completed Story Points";

                xlsCol++;
                int iCheckBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "CheckBy";

                xlsCol++;
                int iCrossCheckBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "CrossCheckBy";

                xlsCol++;
                int iApproveBy = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "ApproveBy";

                //xlsCol++;
                //int iAvgStorypoints = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "AvgStorypoints";
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, colNoOfEmp, xlsRow, endXlsCol].ColumnWidth = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 38;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Color = ExcelKnownColors.Black;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Thick);

                #endregion

                string voucherNo = "";
                /// string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                // string totalFormula = "";

                //string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;

                //double TotalCreatedTask = clsStaticInfo.dbl(dtTask.Compute("SUM(CreatedTask)", null));
                double TotalTaskDue = clsStaticInfo.dbl(dtTask.Compute("SUM(TaskDue)", null));

                for (int i = 0; i < dtTask.Rows.Count; i++)
                {
                    sheet1[xlsRow, colSLNO].Number = (i + 1);
                    sheet1.Range[xlsRow, iDesignation].Text = dtTask.Rows[i]["DesignationGroup"].ToString();
                    sheet1.Range[xlsRow, colNoOfEmp].Number = clsStaticInfo.dbl(dtTask.Rows[i]["NoOfEmp"].ToString());
                    //sheet1.Range[xlsRow, iTaskCreated].Number = clsStaticInfo.dbl(dtTask.Rows[i]["CreatedTask"].ToString());

                    //double PerTotalTask = Math.Round((Convert.ToDouble(dtTask.Rows[i]["CreatedTask"].ToString()) / TotalCreatedTask) * 100,2);
                    //sheet1.Range[xlsRow, iPerTotalTask].Number = Math.Round(PerTotalTask);//formula 
                    //sheet1.Range[xlsRow, iPerTotalTask].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet1.Range[xlsRow, iTaskUnread].Number = clsStaticInfo.dbl(dtTask.Rows[i]["UnRead"].ToString());
                    sheet1.Range[xlsRow, iTaskDue].Number = clsStaticInfo.dbl(dtTask.Rows[i]["TaskDue"].ToString());

                    double PerTaskDue = Math.Round((Convert.ToDouble(dtTask.Rows[i]["TaskDue"].ToString()) / TotalTaskDue) * 100,2);
                    sheet1.Range[xlsRow, iPerOfDueTask].Number = Math.Round(PerTaskDue);//formula 
                    sheet1.Range[xlsRow, iPerOfDueTask].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet1.Range[xlsRow, iTaskCompletedOnTime].Number = clsStaticInfo.dbl(dtTask.Rows[i]["OnTimeTask"].ToString());
                    sheet1.Range[xlsRow, iTaskCompletedLate].Number = clsStaticInfo.dbl(dtTask.Rows[i]["LateTask"].ToString());
                    sheet1[xlsRow, iEarlyTask].Number = clsStaticInfo.dbl(dtTask.Rows[i]["EarlyTask"].ToString());

                    sheet1[xlsRow, iTaskCompletedFP].Number = clsStaticInfo.dbl(dtTask.Rows[i]["OnTimeTask"].ToString())+ clsStaticInfo.dbl(dtTask.Rows[i]["LateTask"].ToString())+ clsStaticInfo.dbl(dtTask.Rows[i]["EarlyTask"].ToString());

                    sheet1[xlsRow, iOverdueTask].Number = clsStaticInfo.dbl(dtTask.Rows[i]["OverdueTask"].ToString());

                    sheet1[xlsRow, iPeriviousPeriodOverdueTask].Number = clsStaticInfo.dbl(dtTask.Rows[i]["PeriviousPeriodOverdueTask"].ToString());

                    sheet1[xlsRow, iPerformance].Number = (((Convert.ToDouble(dtTask.Rows[i]["OnTimeTask"].ToString()) * 2) + (Convert.ToDouble(dtTask.Rows[i]["LateTask"].ToString()) * 1) + (Convert.ToDouble(dtTask.Rows[i]["EarlyTask"].ToString()) * 2)) - (Convert.ToDouble(dtTask.Rows[i]["UnRead"].ToString())));//formula

                    sheet1[xlsRow, iTotalStoryPoints].Number = clsStaticInfo.dbl(dtTask.Rows[i]["TaskDue"].ToString()) * 2;
                    sheet1[xlsRow, iCompletedStoryPoints].Number = (clsStaticInfo.dbl(dtTask.Rows[i]["OnTimeTask"].ToString()) + clsStaticInfo.dbl(dtTask.Rows[i]["LateTask"].ToString()) + clsStaticInfo.dbl(dtTask.Rows[i]["EarlyTask"].ToString())) * 2;
                    //sheet1[xlsRow, iAvgStorypoints].Number = clsStaticInfo.dbl(dtTask.Rows[i]["AvgStorypoints"].ToString());
                    sheet1.Range[xlsRow, iCheckBy].Number = clsStaticInfo.dbl(dtTask.Rows[i]["CheckBy"].ToString());
                    sheet1.Range[xlsRow, iCrossCheckBy].Number = clsStaticInfo.dbl(dtTask.Rows[i]["CrossCheckBy"].ToString());
                    sheet1.Range[xlsRow, iApproveBy].Number = clsStaticInfo.dbl(dtTask.Rows[i]["ApproveBy"].ToString());
                    xlsRow++;
                }

                sheet1.Range[xlsRow, 2].Text = "TOTAL";
                sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;

                sheet.Range[xlsRow, colSLNO, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Thick);
                sheet.Range[startRow, colSLNO, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Thick);

                #region SUM

                formula = "SUM(" + clsStaticInfo.GetxlsCol(colNoOfEmp) + perStartRow + ":" + clsStaticInfo.GetxlsCol(colNoOfEmp) + (xlsRow - 1) + ")";
                sheet1[xlsRow, colNoOfEmp, xlsRow, colNoOfEmp].Formula = formula;
                sheet1.Range[xlsRow, colNoOfEmp, xlsRow, colNoOfEmp].CellStyle.Font.Bold = true;

                //formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskCreated) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskCreated) + (xlsRow - 1) + ")";
                //sheet1[xlsRow, iTaskCreated, xlsRow, iTaskCreated].Formula = formula;
                //sheet1.Range[xlsRow, iTaskCreated, xlsRow, iTaskCreated].CellStyle.Font.Bold = true;
                //formula = "SUM(" + clsStaticInfo.GetxlsCol(iPerTotalTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPerTotalTask) + (xlsRow - 1) + ")";
                //sheet1[xlsRow, iPerTotalTask, xlsRow, iPerTotalTask].Formula = formula;
                //sheet1.Range[xlsRow, iPerTotalTask, xlsRow, iPerTotalTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskUnread) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskUnread) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskUnread, xlsRow, iTaskUnread].Formula = formula;
                sheet1.Range[xlsRow, iTaskUnread, xlsRow, iTaskUnread].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskDue) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskDue) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskDue, xlsRow, iTaskDue].Formula = formula;
                sheet1.Range[xlsRow, iTaskDue, xlsRow, iTaskDue].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iPerOfDueTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPerOfDueTask) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iPerOfDueTask, xlsRow, iPerOfDueTask].Formula = formula;
                sheet1.Range[xlsRow, iPerOfDueTask, xlsRow, iPerOfDueTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskCompletedOnTime) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskCompletedOnTime) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskCompletedOnTime, xlsRow, iTaskCompletedOnTime].Formula = formula;
                sheet1.Range[xlsRow, iTaskCompletedOnTime, xlsRow, iTaskCompletedOnTime].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskCompletedLate) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskCompletedLate) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskCompletedLate, xlsRow, iTaskCompletedLate].Formula = formula;
                sheet1.Range[xlsRow, iTaskCompletedLate, xlsRow, iTaskCompletedLate].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iOverdueTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iOverdueTask) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iOverdueTask, xlsRow, iOverdueTask].Formula = formula;
                sheet1.Range[xlsRow, iOverdueTask, xlsRow, iOverdueTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iEarlyTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iEarlyTask) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iEarlyTask, xlsRow, iEarlyTask].Formula = formula;
                sheet1.Range[xlsRow, iEarlyTask, xlsRow, iEarlyTask].CellStyle.Font.Bold = true;


                formula = "SUM(" + clsStaticInfo.GetxlsCol(iPeriviousPeriodOverdueTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPeriviousPeriodOverdueTask) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iPeriviousPeriodOverdueTask, xlsRow, iPeriviousPeriodOverdueTask].Formula = formula;
                sheet1.Range[xlsRow, iPeriviousPeriodOverdueTask, xlsRow, iPeriviousPeriodOverdueTask].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iPerformance) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPerformance) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iPerformance, xlsRow, iPerformance].Formula = formula;
                sheet1.Range[xlsRow, iPerformance, xlsRow, iPerformance].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTotalStoryPoints) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTotalStoryPoints) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTotalStoryPoints, xlsRow, iTotalStoryPoints].Formula = formula;
                sheet1.Range[xlsRow, iTotalStoryPoints, xlsRow, iTotalStoryPoints].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iCompletedStoryPoints) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iCompletedStoryPoints) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iCompletedStoryPoints, xlsRow, iCompletedStoryPoints].Formula = formula;
                sheet1.Range[xlsRow, iCompletedStoryPoints, xlsRow, iCompletedStoryPoints].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskCompletedFP) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskCompletedFP) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iTaskCompletedFP, xlsRow, iTaskCompletedFP].Formula = formula;
                sheet1.Range[xlsRow, iTaskCompletedFP, xlsRow, iTaskCompletedFP].CellStyle.Font.Bold = true;

                //formula = "SUM(" + clsStaticInfo.GetxlsCol(iAvgStorypoints) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iAvgStorypoints) + (xlsRow - 1) + ")";
                //sheet1[xlsRow, iAvgStorypoints, xlsRow, iAvgStorypoints].Formula = formula;
                //sheet1.Range[xlsRow, iAvgStorypoints, xlsRow, iAvgStorypoints].CellStyle.Font.Bold = true;
                formula = "SUM(" + clsStaticInfo.GetxlsCol(iCheckBy) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iCheckBy) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iCheckBy, xlsRow, iCheckBy].Formula = formula;
                sheet1.Range[xlsRow, iCheckBy, xlsRow, iCheckBy].CellStyle.Font.Bold = true;


                formula = "SUM(" + clsStaticInfo.GetxlsCol(iCrossCheckBy) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iCrossCheckBy) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iCrossCheckBy, xlsRow, iCrossCheckBy].Formula = formula;
                sheet1.Range[xlsRow, iCrossCheckBy, xlsRow, iCrossCheckBy].CellStyle.Font.Bold = true;

                formula = "SUM(" + clsStaticInfo.GetxlsCol(iApproveBy) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iApproveBy) + (xlsRow - 1) + ")";
                sheet1[xlsRow, iApproveBy, xlsRow, iApproveBy].Formula = formula;
                sheet1.Range[xlsRow, iApproveBy, xlsRow, iApproveBy].CellStyle.Font.Bold = true;
                #endregion
                sheet1.AutoFilters.FilterRange = sheet1.Range[StartRow - 1, 1, xlsRow, endXlsCol];

                #region ******************Report Header******************

                xlsRow = 1;
                FactoryName = string.Empty;
                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Management Report From Date: " + fromDate + " To Date: " + todate;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + SheetName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskManagementReportDesignationGroupWise.xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
    }
}