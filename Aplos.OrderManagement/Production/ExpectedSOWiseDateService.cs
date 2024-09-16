using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Syncfusion.XlsIO;

namespace Library.OrderManagement.Production
{
    public class ExpectedSOWiseDateService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public ExpectedSOWiseDateService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }


        public void ExpectedSOWiseProductionCompletionSave(string entityid)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {
                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");


                Dictionary<string, List<DataRow>> dicProductionQtyDistribution;
                DataTable dt, dtOrderMaster;
                getSalesOrderDistribution(System.DateTime.Now.ToString("dd-MMM-yyyy"), entityid, out dicProductionQtyDistribution, out dt);

                getOrderMaster(entityid, out dtOrderMaster);


                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(5);
                workbook.Worksheets[3].Name = "OS3 Data";
                sheet = workbook.Worksheets[3];


                int ROW = 1; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "EntityId";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "SalesOrderId";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderId = COL;
                COL++;

                sheet[ROW, COL].Text = "ProductionOrderId";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionOrderId = COL;
                COL++;


                sheet[ROW, COL].Text = "ExpectedCompletionDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colExpectedCompletionDate = COL;
                COL++;
                sheet[ROW, COL].Text = "ProducedQty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAvailableProducedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanQty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAvailablePlanQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalAvailableQty = COL;

                #endregion columns

                int endCol = COL;



                ROW++;

                int startRow = ROW;

                string ExpectedProductionStartDate = "";
                double PRCumulativePlanQty = 0;
                string PRId = "";
                for (int i = 0; i < dtOrderMaster.Rows.Count; i++)
                {
                    if (dtOrderMaster.Rows[i]["ProductionOrderId"].ToString() == "23350")
                    {

                    }
                    if (PRId != dtOrderMaster.Rows[i]["ProductionOrderId"].ToString())
                    {
                        PRCumulativePlanQty = 0;
                        ExpectedProductionStartDate = "";
                    }
                    PRId = dtOrderMaster.Rows[i]["ProductionOrderId"].ToString();

                    PRCumulativePlanQty += clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PlannedQty"].ToString());
                    dtOrderMaster.Rows[i]["CummPlannedQty"] = PRCumulativePlanQty;


                    sheet[ROW, colEntity].Text = dtOrderMaster.Rows[i]["EntityId"].ToString();
                    sheet[ROW, colProductionOrderId].Text = dtOrderMaster.Rows[i]["ProductionOrderId"].ToString();

                    sheet[ROW, colSalesOrderId].Text = dtOrderMaster.Rows[i]["SalesOrderId"].ToString();

                    //if (dtOrderMaster.Rows[i]["ProductionOrderId"].ToString() == "20104")
                    //{

                    //ProductionStartDate
                    //}

                    if (dicProductionQtyDistribution.ContainsKey(dtOrderMaster.Rows[i]["ProductionOrderId"].ToString()))
                    {
                        DataRow dr = GetExpectedCompletionDate(PRCumulativePlanQty, dicProductionQtyDistribution[dtOrderMaster.Rows[i]["ProductionOrderId"].ToString()]);
                        if (dr != null)
                        {
                            if (ExpectedProductionStartDate == "")
                                ExpectedProductionStartDate = GetDate(dr["ProductionStartDate"].ToString());

                            sheet[ROW, colExpectedCompletionDate].Text = GetDate(dr["ProductionDate"].ToString());
                            sheet[ROW, colExpectedCompletionDate].NumberFormat = "dd-MMM-yyyy";
                            sheet[ROW, colAvailableProducedQty].Number = clsStaticInfo.dbl(dr["CummProductionQty"].ToString());
                            sheet[ROW, colAvailablePlanQty].Number = clsStaticInfo.dbl(dr["CummPlanQty"].ToString());
                            sheet[ROW, colTotalAvailableQty].Number = clsStaticInfo.dbl(dr["CummPlanQty"].ToString()) + clsStaticInfo.dbl(dr["CummProductionQty"].ToString());

                            ExpectedProductionStartDate = GetDate(dr["ProductionDate"].ToString());
                        }

                    }


                    ROW++;

                }

                DataTable dtTrial = sheet.ExportDataTable(1, 1, ROW, endCol, ExcelExportDataTableOptions.ColumnNames);
                dtTrial.Columns.Add("Id", typeof(string));

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.ExpectedSOWiseProductionCompletion  where EntityId in (" + entityid + @")", out dsMaster, false, "1");
                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }
                DataRow da = null;
                for (var i = 0; i < dtTrial.Rows.Count; i++)
                {
                    da = dsMaster.Tables[0].NewRow();
                    string id = "" + dtTrial.Rows[i]["EntityId"].ToString() + "" + i;
                    dtTrial.Rows[i]["Id"] = id;

                    da["Id"] = dtTrial.Rows[i]["Id"];
                    da["EntityId"] = dtTrial.Rows[i]["EntityId"];
                    da["ProductionOrderId"] = dtTrial.Rows[i]["ProductionOrderId"];
                    da["SalesOrderId"] = dtTrial.Rows[i]["SalesOrderId"];
                    da["ExpectedCompletionDate"] = dtTrial.Rows[i]["ExpectedCompletionDate"];
                    da["Quantity"] = dtTrial.Rows[i]["Qty"];

                    dsMaster.Tables[0].Rows.Add(da);
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }


        private void getSalesOrderDistribution(string date, string entityid, out Dictionary<string, List<DataRow>> dicDistributedSO, out DataTable dt)
        {

            string sql = @"
                                select D.*,MMN.ProductionStartDate,0 AS CummProductionQty,0 AS CummPlanQty,ISNULL(d.ProductionQty,0)+ISNULL(d.PlanQty,0) AS TotalQty,0 AS CummTotalQty  
                                from (SELECT p1.ProductionOrderID,FORMAT(p1.ProductionDate,'dd-MMM-yyyy')AS ProductionDate,0 AS ProductionQty,SUM(p1.Quantity) AS PlanQty
                                                   from ProductionPlanningType1 p1 
                                                 WHERE p1.ProductionDate>='" + date + @"'  AND P1.EntityId in (" + entityid + @")
                                                 GROUP BY  p1.ProductionDate,p1.ProductionOrderID
                 
                                                 UNION ALL
                 
                                                 SELECT s.ProductionOrderId,FORMAT(s.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,SUM(s.Quantity) AS ProductionQty,0 AS PlanQty
				                                FROM  trn.ProductionSummary S 
					                                WHERE S.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=S.ProductionOrderID) AND  S.EntityId in (" + entityid + @") AND CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<CONVERT(DATETIME,'" + date + @"')
				                                GROUP BY  s.ProductionOrderId,s.ProductionDate
                                ) AS D 
                                left join (
                                   select ProductionOrderID,FORMAT(MIN(ProductionDate),'dd-MMM-yyyy')  AS ProductionStartDate 
                                    from ( SELECT p1.ProductionOrderID,MIN(p1.ProductionDate) AS ProductionDate
                                                   from ProductionPlanningType1 p1 
                                                 WHERE p1.ProductionDate>='" + date + @"'  AND P1.EntityId in (" + entityid + @")
                                                 GROUP BY  p1.ProductionOrderID
                 
                                                 UNION ALL
                 
                                                 SELECT s.ProductionOrderId,MIN(s.ProductionDate) AS ProductionDate
				                                FROM  trn.ProductionSummary S 
					                                WHERE S.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=S.ProductionOrderID) AND  S.EntityId in (" + entityid + @") AND CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<CONVERT(DATETIME,'" + date + @"')
				                                GROUP BY  s.ProductionOrderId) AS K group by ProductionOrderID

                                    ) AS MMN ON MMN.ProductionOrderId=D.ProductionOrderId

                                INNER JOIN trn.ProductionOrder AS po ON po.Id=d.ProductionOrderID
                                INNER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                                WHERE PO.Id IN (SELECT DISTINCT p.ProductionOrderId FROM trn.ProductionOrderDetail AS p
                                            JOIN trn.SalesOrder AS so ON so.Id=p.SalesOrderId
                                            WHERE so.OrderStatusId<>'Closed')
                                ORDER BY D.ProductionOrderID,convert(date,D.ProductionDate)

                            ";


            dt = _sqlRepository.GetDataTable(sql);
            dicDistributedSO = new Dictionary<string, List<DataRow>>();
            List<DataRow> row = new List<DataRow>();

            string Id = ""; double CummProductionQty = 0; double CummPlanQty = 0; double CummTotalQty = 0;
            string ProductionEndDate = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Id != dt.Rows[i]["ProductionOrderID"].ToString())
                {
                    CummProductionQty = 0; CummPlanQty = 0; CummTotalQty = 0;
                    row = new List<DataRow>();
                    dicDistributedSO.Add(dt.Rows[i]["ProductionOrderID"].ToString(), row);

                    ProductionEndDate = dt.Rows[i]["ProductionDate"].ToString();
                }

                dt.Rows[i]["ProductionStartDate"] = ProductionEndDate;

                CummProductionQty += clsStaticInfo.dbl(dt.Rows[i]["ProductionQty"].ToString());
                CummPlanQty += clsStaticInfo.dbl(dt.Rows[i]["PlanQty"].ToString());
                CummTotalQty += clsStaticInfo.dbl(dt.Rows[i]["TotalQty"].ToString());
                ProductionEndDate = dt.Rows[i]["ProductionDate"].ToString();

                dt.Rows[i]["CummProductionQty"] = CummProductionQty;
                dt.Rows[i]["CummPlanQty"] = CummPlanQty;
                dt.Rows[i]["CummTotalQty"] = CummTotalQty;

                row.Add(dt.Rows[i]);

                Id = dt.Rows[i]["ProductionOrderID"].ToString();
            }


        }

        public void Type2ExpectedSOWiseProductionCompletionSave(string entityid)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {
                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");


                Dictionary<string, List<DataRow>> dicProductionQtyDistribution;
                DataTable dt, dtOrderMaster;
                getType2SalesOrderDistribution(System.DateTime.Now.ToString("dd-MMM-yyyy"), entityid, out dicProductionQtyDistribution, out dt);

                getType2OrderMaster(entityid, out dtOrderMaster);


                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(5);
                workbook.Worksheets[3].Name = "OS3 Data";
                sheet = workbook.Worksheets[3];


                int ROW = 1; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "EntityId";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "SalesOrderId";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderId = COL;
                COL++;

                sheet[ROW, COL].Text = "ProductionOrderId";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionOrderId = COL;
                COL++;


                sheet[ROW, COL].Text = "ExpectedCompletionDate";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colExpectedCompletionDate = COL;
                COL++;
                sheet[ROW, COL].Text = "ProducedQty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAvailableProducedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "PlanQty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAvailablePlanQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colTotalAvailableQty = COL;

                #endregion columns

                int endCol = COL;



                ROW++;

                int startRow = ROW;

                string ExpectedProductionStartDate = "";
                double PRCumulativePlanQty = 0;
                string PRId = "";
                for (int i = 0; i < dtOrderMaster.Rows.Count; i++)
                {
                    if (dtOrderMaster.Rows[i]["ProductionOrderId"].ToString() == "23350")
                    {

                    }
                    if (PRId != dtOrderMaster.Rows[i]["ProductionOrderId"].ToString())
                    {
                        PRCumulativePlanQty = 0;
                        ExpectedProductionStartDate = "";
                    }
                    PRId = dtOrderMaster.Rows[i]["ProductionOrderId"].ToString();

                    PRCumulativePlanQty += clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PlannedQty"].ToString());
                    dtOrderMaster.Rows[i]["CummPlannedQty"] = PRCumulativePlanQty;


                    sheet[ROW, colEntity].Text = dtOrderMaster.Rows[i]["EntityId"].ToString();
                    sheet[ROW, colProductionOrderId].Text = dtOrderMaster.Rows[i]["ProductionOrderId"].ToString();

                    sheet[ROW, colSalesOrderId].Text = dtOrderMaster.Rows[i]["SalesOrderId"].ToString();

                    //if (dtOrderMaster.Rows[i]["ProductionOrderId"].ToString() == "20104")
                    //{

                    //ProductionStartDate
                    //}

                    if (dicProductionQtyDistribution.ContainsKey(dtOrderMaster.Rows[i]["ProductionOrderId"].ToString()))
                    {
                        DataRow dr = GetExpectedCompletionDate(PRCumulativePlanQty, dicProductionQtyDistribution[dtOrderMaster.Rows[i]["ProductionOrderId"].ToString()]);
                        if (dr != null)
                        {
                            if (ExpectedProductionStartDate == "")
                                ExpectedProductionStartDate = GetDate(dr["ProductionStartDate"].ToString());

                            sheet[ROW, colExpectedCompletionDate].Text = GetDate(dr["ProductionDate"].ToString());
                            sheet[ROW, colExpectedCompletionDate].NumberFormat = "dd-MMM-yyyy";
                            sheet[ROW, colAvailableProducedQty].Number = clsStaticInfo.dbl(dr["CummProductionQty"].ToString());
                            sheet[ROW, colAvailablePlanQty].Number = clsStaticInfo.dbl(dr["CummPlanQty"].ToString());
                            sheet[ROW, colTotalAvailableQty].Number = clsStaticInfo.dbl(dr["CummPlanQty"].ToString()) + clsStaticInfo.dbl(dr["CummProductionQty"].ToString());

                            ExpectedProductionStartDate = GetDate(dr["ProductionDate"].ToString());
                        }

                    }


                    ROW++;

                }

                DataTable dtTrial = sheet.ExportDataTable(1, 1, ROW, endCol, ExcelExportDataTableOptions.ColumnNames);
                dtTrial.Columns.Add("Id", typeof(string));

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.ExpectedSOWiseProductionCompletion  where EntityId in (" + entityid + @")", out dsMaster, false, "1");
                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }
                DataRow da = null;
                for (var i = 0; i < dtTrial.Rows.Count; i++)
                {
                    da = dsMaster.Tables[0].NewRow();
                    string id = "" + dtTrial.Rows[i]["EntityId"].ToString() + "" + i;
                    dtTrial.Rows[i]["Id"] = id;

                    da["Id"] = dtTrial.Rows[i]["Id"];
                    da["EntityId"] = dtTrial.Rows[i]["EntityId"];
                    da["ProductionOrderId"] = dtTrial.Rows[i]["ProductionOrderId"];
                    da["SalesOrderId"] = dtTrial.Rows[i]["SalesOrderId"];
                    da["ExpectedCompletionDate"] = dtTrial.Rows[i]["ExpectedCompletionDate"];
                    da["Quantity"] = dtTrial.Rows[i]["Qty"];

                    dsMaster.Tables[0].Rows.Add(da);
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

        private void getType2SalesOrderDistribution(string date, string entityid, out Dictionary<string, List<DataRow>> dicDistributedSO, out DataTable dt)
        {

            string sql = @"
                                select D.*,MMN.ProductionStartDate,0 AS CummProductionQty,0 AS CummPlanQty,ISNULL(d.ProductionQty,0)+ISNULL(d.PlanQty,0) AS TotalQty,0 AS CummTotalQty  
                                from (SELECT p1.ProductionOrderID,FORMAT(p1.ProductionDate,'dd-MMM-yyyy')AS ProductionDate,0 AS ProductionQty,SUM(p1.Quantity) AS PlanQty
                                                   from ProductionPlanningType1 p1 
                                                 WHERE p1.ProductionDate>='" + date + @"'  AND P1.EntityId in (" + entityid + @")
                                                 GROUP BY  p1.ProductionDate,p1.ProductionOrderID
                 
                                                 UNION ALL
                 
                                                 SELECT s.ProductionOrderId,FORMAT(s.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,SUM(s.Quantity) AS ProductionQty,0 AS PlanQty
				                                FROM  trn.ProductionSummary S 
					                                WHERE S.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=S.ProductionOrderID) AND  S.EntityId in (" + entityid + @") AND CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<CONVERT(DATETIME,'" + date + @"')
				                                GROUP BY  s.ProductionOrderId,s.ProductionDate
                                ) AS D 
                                left join (
                                   select ProductionOrderID,FORMAT(MIN(ProductionDate),'dd-MMM-yyyy')  AS ProductionStartDate 
                                    from ( SELECT p1.ProductionOrderID,MIN(p1.ProductionDate) AS ProductionDate
                                                   from ProductionPlanningType1 p1 
                                                 WHERE p1.ProductionDate>='" + date + @"'  AND P1.EntityId in (" + entityid + @")
                                                 GROUP BY  p1.ProductionOrderID
                 
                                                 UNION ALL
                 
                                                 SELECT s.ProductionOrderId,MIN(s.ProductionDate) AS ProductionDate
				                                FROM  trn.ProductionSummary S 
					                                WHERE S.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=S.ProductionOrderID) AND  S.EntityId in (" + entityid + @") AND CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<CONVERT(DATETIME,'" + date + @"')
				                                GROUP BY  s.ProductionOrderId) AS K group by ProductionOrderID

                                    ) AS MMN ON MMN.ProductionOrderId=D.ProductionOrderId

                                INNER JOIN trn.ProductionOrder AS po ON po.Id=d.ProductionOrderID
                                INNER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                                WHERE PO.Id IN (SELECT DISTINCT p.ProductionOrderId FROM trn.ProductionOrderDetail AS p
                                            JOIN trn.SalesOrder AS so ON so.Id=p.SalesOrderId
                                            WHERE so.OrderStatusId<>'Closed')
                                ORDER BY D.ProductionOrderID,convert(date,D.ProductionDate)

                            ";


            dt = _sqlRepository.GetDataTable(sql);
            dicDistributedSO = new Dictionary<string, List<DataRow>>();
            List<DataRow> row = new List<DataRow>();

            string Id = ""; double CummProductionQty = 0; double CummPlanQty = 0; double CummTotalQty = 0;
            string ProductionEndDate = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Id != dt.Rows[i]["ProductionOrderID"].ToString())
                {
                    CummProductionQty = 0; CummPlanQty = 0; CummTotalQty = 0;
                    row = new List<DataRow>();
                    dicDistributedSO.Add(dt.Rows[i]["ProductionOrderID"].ToString(), row);

                    ProductionEndDate = dt.Rows[i]["ProductionDate"].ToString();
                }

                dt.Rows[i]["ProductionStartDate"] = ProductionEndDate;

                CummProductionQty += clsStaticInfo.dbl(dt.Rows[i]["ProductionQty"].ToString());
                CummPlanQty += clsStaticInfo.dbl(dt.Rows[i]["PlanQty"].ToString());
                CummTotalQty += clsStaticInfo.dbl(dt.Rows[i]["TotalQty"].ToString());
                ProductionEndDate = dt.Rows[i]["ProductionDate"].ToString();

                dt.Rows[i]["CummProductionQty"] = CummProductionQty;
                dt.Rows[i]["CummPlanQty"] = CummPlanQty;
                dt.Rows[i]["CummTotalQty"] = CummTotalQty;

                row.Add(dt.Rows[i]);

                Id = dt.Rows[i]["ProductionOrderID"].ToString();
            }


        }

        private void getOrderMaster(string entityid, out DataTable dtOrderMaster)
        {
            //string sql = @"
            //                SELECT so.Id AS SalesOrderId, b.UserName AS Buyer,ei.EmployeeName AS ResponsiblePerson,mo.MasterOrderNo,mm.UserName AS Material,
            //                POD.ProductionOrderId,OC.UserName AS OrderCategory,os.UserName AS OrderStatus,ps.UserName  AS productionStatus,                          
            //                pc.UserName AS ProductCategory,  pm.UserName AS Product,
            //                so.DeliveryDate,so.CommitmentDate,so.Qty AS SOQty, cp.PONumber,format(cp.PODate,'dd-MMM-yyyy') AS PODate

            //                  FROM trn.MasterOrder MO
            //                left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
            //                INNER join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
            //                LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
            //                LEFT OUTER JOIN TRN.ProductionOrderDetail AS pod ON POD.SalesOrderId=SO.Id
            //                LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
            //                LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId


            //                left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
            //                left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
            //                left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
            //                left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

            //                left outer join [HKP].[Party] p on P.Id=MO.plantID
            //                left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
            //                left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
            //                left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
            //                left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
            //                left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
            //                left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
            //                left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
            //                left outer join mst.Destination DEST on dest.Id=so.DestinationId
            //                left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
            //                left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
            //                left outer join hkp.Season S on s.id=mo.SeasonId
            //                left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId

            //                WHERE os.Id='"+ Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"' AND mo.EntityId='" + entityid + @"'
            //ORDER BY b.UserName,so.DeliveryDate,POD.ProductionOrderId,SO.ID";


            string sql = @" SELECT trkp.UserName AS Plant,trke.UserName AS Entity,trke.Id as EntityId,so.Id AS SalesOrderId, b.UserName AS Buyer,ei.EmployeeName AS ResponsiblePerson,mo.MasterOrderNo,mm.UserName AS Material,
                           OC.UserName AS OrderCategory,os.UserName AS OrderStatus,   MA.StandardName AS Article,                   
                            pc.UserName AS ProductCategory,  pm.UserName AS Product,moi.BuyerReferenceNo,MOI.OwnReferenceNo,
                            mo.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo OwnOrderNo,SO.Description AS SODesc,
                            mm.Id AS MaterialRowId,pod.ProductionOrderId,CASE WHEN isnull(sed.ID,0)<>0 THEN 'YES' ELSE 'NO' END AS isProductionScheduled,
                            so.DeliveryDate,so.CommitmentDate,so.Qty AS SOQty, cp.PONumber,format(cp.PODate,'dd-MMM-yyyy') AS PODate,ps.UserName AS ProductionStatus,
                            CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) AS PlannedQty,0 AS CummPlannedQty,
                           
                            --CEILING((so.Qty*(1+(moi.ExtraOrderPercentage/100)))*(1+(moi.OrderWastagePercentage/100))) AS PlannedQty,0 AS CummPlannedQty,
                            PO.Qty AS PRQty,case when isnull(SED.Qty,0)=0 THEN PO.PlannedQty ELSE  SED.Qty END AS PRActualPlannedQty,
                            PO.PlannedQty AS PRPlannedQty,P.UserName AS Customer
                              FROM trn.MasterOrder MO
                            left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
                            INNER join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
                            LEFT OUTER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS SED ON sed.ProductionOrderID=pod.ProductionOrderId
                            LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                            LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
                           

                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PO.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId

                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
							 --LEFT OUTER JOIN hkp.ProductGroup AS pg ON pg.Id=moi.pro
                           
                            left outer join [HKP].[Party] p on P.Id=MO.PartyId
                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId

                            WHERE PO.Id IN (SELECT DISTINCT p.ProductionOrderId FROM trn.ProductionOrderDetail AS p
                                            JOIN trn.SalesOrder AS so ON so.Id=p.SalesOrderId
                                            WHERE so.OrderStatusId<>'Closed') AND PO.EntityId IN (" + entityid + @")
            ORDER BY trkp.UserName,trke.UserName,trke.Id, pod.ProductionOrderId,so.DeliveryDate,SO.ID";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);


        }

        private void getType2OrderMaster(string entityid, out DataTable dtOrderMaster)
        {
         
            string sql = @" SELECT trkp.UserName AS Plant,trke.UserName AS Entity,trke.Id as EntityId,so.Id AS SalesOrderId, b.UserName AS Buyer,ei.EmployeeName AS ResponsiblePerson,mo.MasterOrderNo,mm.UserName AS Material,
                           OC.UserName AS OrderCategory,os.UserName AS OrderStatus,   MA.StandardName AS Article,                   
                            pc.UserName AS ProductCategory,  pm.UserName AS Product,moi.BuyerReferenceNo,MOI.OwnReferenceNo,
                            mo.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo OwnOrderNo,SO.Description AS SODesc,
                            mm.Id AS MaterialRowId,pod.ProductionOrderId,CASE WHEN isnull(sed.ID,0)<>0 THEN 'YES' ELSE 'NO' END AS isProductionScheduled,
                            so.DeliveryDate,so.CommitmentDate,so.Qty AS SOQty, cp.PONumber,format(cp.PODate,'dd-MMM-yyyy') AS PODate,ps.UserName AS ProductionStatus,
                            CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) AS PlannedQty,0 AS CummPlannedQty,
                           
                            --CEILING((so.Qty*(1+(moi.ExtraOrderPercentage/100)))*(1+(moi.OrderWastagePercentage/100))) AS PlannedQty,0 AS CummPlannedQty,
                            PO.Qty AS PRQty,case when isnull(SED.Qty,0)=0 THEN PO.PlannedQty ELSE  SED.Qty END AS PRActualPlannedQty,
                            PO.PlannedQty AS PRPlannedQty,P.UserName AS Customer
                              FROM trn.MasterOrder MO
                            left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
                            INNER join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
                            LEFT OUTER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS SED ON sed.ProductionOrderID=pod.ProductionOrderId
                            LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                            LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
                           

                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PO.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId

                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
							 --LEFT OUTER JOIN hkp.ProductGroup AS pg ON pg.Id=moi.pro
                           
                            left outer join [HKP].[Party] p on P.Id=MO.PartyId
                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId

                            WHERE PO.Id IN (SELECT DISTINCT p.ProductionOrderId FROM trn.ProductionOrderDetail AS p
                                            JOIN trn.SalesOrder AS so ON so.Id=p.SalesOrderId
                                            WHERE so.OrderStatusId<>'Closed') AND PO.EntityId IN (" + entityid + @")
            ORDER BY trkp.UserName,trke.UserName,trke.Id, pod.ProductionOrderId,so.DeliveryDate,SO.ID";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);


        }

        private DataRow GetExpectedCompletionDate(double RequiredQty, List<DataRow> Data)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                if (clsStaticInfo.dbl(Data[i]["CummTotalQty"].ToString()) >= RequiredQty)
                {
                    return Data[i];
                }
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
    }
}




