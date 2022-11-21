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
using System.Linq;
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


        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            //return Json(filters(), JsonRequestBehavior.AllowGet);
            JsonResult json = Json(filters(), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        public IEnumerable<object> filters()
        {
            try
            {
                var sql = @"SELECT * FROM ( SELECT  
                                        isnull(e.Id,'') AS EntityId,isnull(e.UserName,'') Entity,
                                        isnull(ps.Id,'') AS ProductionStatusId, isnull(ps.UserName,'') AS ProductionStatus
										,PO.Id ProductionOrderId,PRS.LotNumber
                                      , PRS.ResponsiblePersonId,EI.EmployeeName ResponsiblePerson,PRS.ProductLibraryId, PL.Code ProductCode
                                                   , Buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),																												
		
													 BuyerId=STUFF((select distinct ','+XB.Id from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

																
                                                    CustomerId=STUFF((select distinct ','+XP.Id from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),   
                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                                                 

                                        from TRN.ProductionSummary PRS
												left join trn.ProductionOrder PO ON PO.Id=PRS.ProductionOrderId
												left join dbo.EmployeeInformation EI ON EI.SystemId=PRS.ResponsiblePersonId
				                                left outer join org.Entity E on e.Id=PO.EntityID
				                                LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
												LEFT OUTER JOIN dbo.ProductLibrary PL ON PL.Id=PRS.ProductLibraryId
                              WHERE  PS.UserName<>'Closed'
                                ) AS KK	";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetPOWiseProductionStatusData()
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



            var jsondata = Json(_sqlRepository.GetDataCollection(strSql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

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
        private void SetDate(IRange Cell, string s)
        {
            if (string.IsNullOrEmpty(s))
                return;

            try
            {
                Cell.DateTime = Convert.ToDateTime(s);
            }
            catch (Exception)
            {
                return;
            }
        }
        private string CellAddr(int Col, int Row)
        {
            return clsStaticInfo.GetxlsCol(Col) + Row.ToString();
        }

        [HttpPost, Authorize]
        public ActionResult ProductionDataXls(Dictionary<string, string> parameters)
        {
            try
            {
                string fileName = "";
                fileName = ProductionDataReport(parameters, "Production Report");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetViewData(Dictionary<string, string> parameters)
        {
            try
            {
                string fileName = "";
                fileName = ProductionDataReport(parameters, "Production Report");
                var data = ReadData(fileName);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
                //return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string ProductionDataReport(Dictionary<string, string> parameters, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[0].Name = "ProductionData";
                sheet = workbook.Worksheets[0];
                DataTable data;
                ReportSQL(parameters, out data);

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;

                COL++;
                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProcess = COL;

                COL++;
                sheet[ROW, COL].Text = "PO Process Sequence";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPOProcessSeq = COL;

                COL++;
                sheet[ROW, COL].Text = "Standard Process Sequence";
                sheet[ROW, COL].ColumnWidth = 16;
                int colStandardProcessSeq = COL;

                COL++;
                sheet[ROW, COL].Text = "BaseProcessApplicable";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBaseProcessApplicable = COL;

                COL++;
                sheet[ROW, COL].Text = "Work Center";
                sheet[ROW, COL].ColumnWidth = 16;
                int colWorkCenter = COL;

                COL++;
                sheet[ROW, COL].Text = "Shift";
                sheet[ROW, COL].ColumnWidth = 16;
                int colShift = COL;

                COL++;
                sheet[ROW, COL].Text = "Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlanDate = COL;

                COL++;
                int colstart = COL;
                sheet[ROW, COL].Text = "Prod. Order No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionOrderID = COL;

                COL++;
                sheet[ROW, COL].Text = "PO Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPOStatus = COL;

                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colbuyer = COL;

                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomer = COL;

                COL++;
                sheet[ROW, COL].Text = "LotNumber";
                sheet[ROW, COL].ColumnWidth = 16;
                int colLotNumber = COL;

                COL++;
                sheet[ROW, COL].Text = "Own Order No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colOwnOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Item No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Item No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colOwnStyleNo = COL;

                COL++;
                sheet[ROW, COL].Text = "Sales Order Ids(PR)";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderIds = COL;

                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProduct = COL;

                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 28;
                int colArticle = COL;

                COL++;
                sheet[ROW, COL].Text = "Work Station";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colWorkStation = COL;

                COL++;
                sheet[ROW, COL].Text = "Working Hours";
                sheet[ROW, COL].ColumnWidth = 16;
                int colActualWorkHours = COL;

                COL++;
                sheet[ROW, COL].Text = "Production Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualQty = COL;

                COL++;
                sheet[ROW, COL].Text = "WIP";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colWIP = COL;

                COL++;
                sheet[ROW, COL].Text = "UpToDate Production";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colUpToDate = COL;

                COL++;
                sheet[ROW, COL].Text = "Current Production";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colCurrent = COL;

                COL++;
                sheet[ROW, COL].Text = "First Book Date";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colFirstProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "Last Book Date";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLastProBookDate = COL;

                COL++;
                sheet[ROW, COL].Text = "First Shipment Date";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colFirstshipmentDate = COL;

                COL++;
                sheet[ROW, COL].Text = "Last Shipment Date";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLastshipmentDate = COL;

                COL++;
                sheet[ROW, COL].Text = "UptoDate Production(%)";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colUptoDateProduction = COL;

                COL++;
                sheet[ROW, COL].Text = "Relay Process";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colRelayProcess = COL;

                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;
                int LastRow = ROW + (data.Rows.Count - 1);
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, colPlanDate].Text = GetDate(data.Rows[i]["ActualDate"].ToString());
                    //sheet[ROW, colPlanDate].Text = data.Rows[i]["ActualDate"].ToString();
                    sheet[ROW, colEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, colProcess].Text = data.Rows[i]["Process"].ToString();
                    sheet[ROW, colPOProcessSeq].Number = clsStaticInfo.dbl(data.Rows[i]["POProcessSequence"].ToString());
                    sheet[ROW, colStandardProcessSeq].Number = clsStaticInfo.dbl(data.Rows[i]["StandardProcessSequence"].ToString());
                    sheet[ROW, colBaseProcessApplicable].Text = data.Rows[i]["BaseProcess"].ToString();
                    sheet[ROW, colWorkCenter].Text = data.Rows[i]["WorkCenter"].ToString();

                    sheet[ROW, colProductionOrderID].Number = clsStaticInfo.dbl(data.Rows[i]["PONo"].ToString());
                    sheet[ROW, colProduct].Text = data.Rows[i]["Product"].ToString();
                    sheet[ROW, colArticle].Text = data.Rows[i]["Article"].ToString();
                    sheet[ROW, colbuyer].Text = data.Rows[i]["buyer"].ToString();
                    sheet[ROW, colCustomer].Text = data.Rows[i]["Customer"].ToString();
                    sheet[ROW, colLotNumber].Text = data.Rows[i]["LotNumber"].ToString();
                    sheet[ROW, colOwnOrderNo].Text = data.Rows[i]["OwnOrderNo"].ToString();
                    sheet[ROW, colStyleNo].Text = data.Rows[i]["StyleNo"].ToString();
                    sheet[ROW, colOwnStyleNo].Text = data.Rows[i]["OwnStyleNo"].ToString();
                    sheet[ROW, colSalesOrderIds].Text = data.Rows[i]["SalesOrderIds"].ToString();

                    sheet[ROW, colShift].Text = data.Rows[i]["ProductionShift"].ToString();
                    sheet[ROW, colWorkStation].Number = clsStaticInfo.dbl(data.Rows[i]["NoOfWorkStation"].ToString());
                    sheet[ROW, colActualWorkHours].Number = clsStaticInfo.dbl(data.Rows[i]["ProductionHours"].ToString());
                    sheet[ROW, colActualQty].Number = clsStaticInfo.dbl(data.Rows[i]["ActualQty"].ToString());
                    sheet[ROW, colPOStatus].Text = data.Rows[i]["POStatus"].ToString();
                    sheet[ROW, colFirstProBookDate].Text = data.Rows[i]["FirstBookDate"].ToString();
                    sheet[ROW, colLastProBookDate].Text = data.Rows[i]["LastBookDate"].ToString();
                    sheet[ROW, colFirstshipmentDate].Text = data.Rows[i]["FirstShipmentDate"].ToString();
                    sheet[ROW, colLastshipmentDate].Text = data.Rows[i]["LastShipmentDate"].ToString();
                    sheet[ROW, colUptoDateProduction].Number = clsStaticInfo.dbl(data.Rows[i]["UptoDateProPercentage"].ToString());

                    //sheet[ROW, colWIP].Formula = "SUMIFS($" + clsStaticInfo.GetxlsCol(colPlanTarget) + "$" + StartRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colPlanTarget) + ROW.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPRNo) + "$" + StartRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colPRNo) + ROW.ToString() + "," + clsStaticInfo.GetxlsCol(colPRNo) + ROW.ToString() + ",$" + clsStaticInfo.GetxlsCol(colDate) + "$" + StartRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colDate) + ROW.ToString() + "," + clsStaticInfo.GetxlsCol(colDate) + ROW.ToString() + ")";

                    //sheet[ROW, colWIP].Formula = "IF(MAX($" + clsStaticInfo.GetxlsCol(colPlanDate) + @")<> startRow.ToString() + @",0,IF(" + startRow + @" = 1, 0, SUMIFS(" + clsStaticInfo.GetxlsCol(colActualQty) + @", " + clsStaticInfo.GetxlsCol(colProductionOrderID) + @", " + startRow + @", " + clsStaticInfo.GetxlsCol(colPOProcessSeq) + @", " + startRow + @") - SUMIFS(" + clsStaticInfo.GetxlsCol(colActualQty) + @", " + clsStaticInfo.GetxlsCol(colProductionOrderID) + @", " + startRow + @", " + clsStaticInfo.GetxlsCol(colPOProcessSeq) + @", " + startRow + @")))";

                    //    sheet[ROW, colWIP].Formula = "IF(MAX($H$7:$H$1799<>H40),0,IF(C40=1,0,SUMIFS($T$7:$T$1799,$C$7:$C$1799,C40-1,$I$7:$I$1799,I40)-SUMIFS($T$7:$T$1799,$C$7:$C$1799,C40,$I$7:$I$1799,I40)))";

                    sheet[ROW, colWIP].Formula = "IF(MAX($" + clsStaticInfo.GetxlsCol(colPlanDate) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPlanDate) + "$" + LastRow.ToString() + "<>" + clsStaticInfo.GetxlsCol(colPlanDate) + ROW.ToString() + "),0,IF(" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + ROW.ToString() + "=1,0,SUMIFS($" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + LastRow.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colPOProcessSeq) + ROW.ToString() + "-1,$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colProductionOrderID) + startRow.ToString() + ")-SUMIFS($" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + LastRow.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colPOProcessSeq) + ROW.ToString() + ",$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colProductionOrderID) + startRow.ToString() + ")))";

                    sheet[ROW, colUpToDate].Formula = "SUMIFS($" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + LastRow.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colPOProcessSeq) + ROW.ToString() + "-1,$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colProductionOrderID) + startRow.ToString() + "))";

                    sheet[ROW, colCurrent].Formula = "SUMIFS($" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colActualQty) + "$" + LastRow.ToString() + ",$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colPOProcessSeq) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colPOProcessSeq) + ROW.ToString() + ",$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + startRow.ToString() + ":$" + clsStaticInfo.GetxlsCol(colProductionOrderID) + "$" + LastRow.ToString() + "," + clsStaticInfo.GetxlsCol(colProductionOrderID) + startRow.ToString() + ")";

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "PO Wise Production Status Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;




                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ReportSQL(Dictionary<string, string> parameters, out DataTable data)
        {
            try
            {
                string partyId = "AND 1=1";
                if (!string.IsNullOrEmpty(parameters["CustomerId"].ToString()))
                {
                    partyId = "AND XMO.PartyId in(" + parameters["CustomerId"] + @")";
                }
                string sql = @"SELECT distinct PP.Id,trke.UserName AS Entity,PP.ProductionOrderID PONo,pp.WorkCenterMasterId,POPS.[Sequence] POProcessSequence 
,wcm.UserName AS WorkCenter ,CPL.UserName AS ProductionShift,PP.ProductionDate AS ActualDate,pp.Quantity AS ActualQty,
isnull(p.UserName,FSFG.UserName) AS Process,p.Sequence StandardProcessSequence,ISNULL(pp.StandardName,ord.Article ) Article                  
,ord.Product,BaseProcess= CASE WHEN P.IsProductionProcess=1 THEN 'Yes' ELSE '' END,PS.UserName POStatus,
FLB.FirstBookDate,FLB.LastBookDate,ORD.FirstShipmentDate,ORD.LastShipmentDate,
PlannedQty=CASE WHEN POPS.Qty=0 THEN (CASE WHEN PT1.Qty=0 THEN PO.PlannedQty ELSE PT1.Qty END) ELSE POPS.Qty END
,UptoDateProPercentage=(pp.Quantity/(CASE WHEN POPS.Qty=0 THEN (CASE WHEN PT1.Qty=0 THEN PO.PlannedQty ELSE PT1.Qty END) ELSE POPS.Qty END))/100,PP.LotNumber
--additional info
		,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pp.ProductionOrderId=Xpod.ProductionOrderId " + partyId + @" for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')	
,buyer=STUFF((select distinct ','+XB.UserName from 
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
                            
FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId,  ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,COUNT(*) AS ProductionHours,SUM(ps.Quantity) AS Quantity,PS.ResponsiblePersonId,PS.LotNumber

FROM trn.ProductionSummary AS ps 
left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,PS.ResponsiblePersonId,PS.LotNumber
) AS pp
LEFT JOIN (Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS FirstBookDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS LastBookDate,ProcessId from TRN.ProductionSummary GROUP BY ProcessId) FLB ON FLB.ProcessId=PP.ProcessId
LEFT JOIN dbo.ShiftDefination CPL ON cpl.SystemId=pp.ProductionShiftId
--LEFT JOIN(Select FORMAT(MIN(DeliveryDate),'dd-MMM-yyyy') FirstShipmentDate,  FORMAT(MAX(DeliveryDate),'dd-MMM-yyyy') LastShipmentDate,Id from trn.SalesOrder Group BY Id) AS so ON so.Id=pp.SalesOrderId
LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=pp.ProductionOrderID
LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID=pp.ProductionOrderId AND sn.ProductionDate=pp.ProductionDate AND sn.WorkCenterMasterId=pp.WorkCenterMasterId AND sn.EntityID=pp.EntityId
LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
LEFT OUTER JOIN hkp.SFGInventory AS FSFG ON FSFG.Id=pp.FromSFGInventoryId
                        
left outer join ProductionPlanningType1 AS ppt on ppt.ProductionOrderID=pp.ProductionOrderId AND ppt.WorkCenterMasterId=PP.WorkCenterMasterId AND  ppt.ProcessId=PP.ProcessId AND ppt.EntityId=pp.EntityId and ppt.ProductionDate=PP.ProductionDate
left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
LEFT JOIN trn.ProductionOrderProcessSet POPS ON POPS.ProductionOrderId=PO.Id AND POPS.ProcessId=pp.ProcessId
left outer join (
select POD.ProductionOrderId,mm.UserName AS Material,MA.StandardName AS Article,PM.UserName AS Product,PC.UserName AS ProductCategory,FLSD.FirstShipmentDate,FLSD.LastShipmentDate,
SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate* so.Qty ELSE  so.Rate* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(so.Qty) AS FOB,
SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM* so.Qty ELSE  so.CM* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(SO.Qty) AS CM
from trn.ProductionOrderDetail POD 
left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
LEFT JOIN(Select FORMAT(MIN(DeliveryDate),'dd-MMM-yyyy') FirstShipmentDate,  FORMAT(MAX(DeliveryDate),'dd-MMM-yyyy') LastShipmentDate,Id from trn.SalesOrder Group BY Id) AS FLSD ON FLSD.Id=pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
left join MasterOrderExchangeRates RT ON RT.TransactionId=MO.Id
left JOIN org.Company AS com ON com.Id=mo.CompanyId
LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + parameters["EntityId"] + @"))
LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + parameters["EntityId"] + @"))
LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId
left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
group by mm.UserName,MA.StandardName,PM.UserName,PC.UserName,POD.ProductionOrderId,FLSD.FirstShipmentDate,FLSD.LastShipmentDate
) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
                                            Where TRKE.Id in(" + parameters["EntityId"] + @")
AND ISNULL(PP.ResponsiblePersonId,'') in(" + parameters["ResponsiblePersonId"] + @")
AND ps.Id in(" + parameters["ProductionStatusId"] + @") order by ActualDate ";


                data = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        private void SaveReportData(List<Dictionary<string, object>> grnDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsMaster, dsDetail, dsGRNDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("SELECT * FROM BPDT.FabricRollManagementChild WHERE FabricRollManagementMasterId =''", out dsDetail, false, "1");
                int count = 0;
                foreach (var item in grnDetailList)
                {
                    count++;
                    DataView dv = new DataView(dsDetail.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        // item["Id"] = masterId + "-" + count;
                        //item["FabricRollManagementMasterId"] = masterId;

                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsDetail);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public List<Dictionary<string, object>> ReadData(string path)
        {
            List<Dictionary<string, object>> data = null;
            //string path = "";
            DataSet dsExcel = null;
            try
            {
                data = new List<Dictionary<string, object>>();
                //SaveFile(out path);
                ReadFile(path, out dsExcel);
                data = dsExcel.Tables[0].ToList<Dictionary<string, object>>();
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
                //DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                DataTable dt = workbook.Worksheets[0].ExportDataTable(6, 1, 50000, 27, ExcelExportDataTableOptions.ColumnNames);
                dt.DefaultView.RowFilter = "isnull(Entity,'')<>''";
                dt = dt.DefaultView.ToTable();
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
    }
}