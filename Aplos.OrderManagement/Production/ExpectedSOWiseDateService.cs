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
            try
            {
               
                ConnectionManager.DAL.ConManager objCon;
                string ExpectedDate = "";
                DataTable dtSOComplete, dtOrderMaster;
                GetProductionOrderMaster(out dtOrderMaster, entityid);
                GetSOCompletionData(out dtSOComplete, entityid);
                DataSet dsMaster;
                string sql = "SELECT * FROM TRN.SalesOrder Where OrderStatusId NOT IN('Hold','Cancelled')";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                for (int i = 0; i < dtSOComplete.Rows.Count; i++)
                {
                    try
                    {
                        
                        DataRow dr = GetExpectedSOCompletionDate(clsStaticInfo.dbl(dtSOComplete.Rows[i]["SoCommqty"].ToString()), dtSOComplete.Rows[i]["ProductionOrderId"].ToString(), dtOrderMaster);

                        if (dr != null)
                        {
                            ExpectedDate = GetDate(dr["Date"].ToString());
                            dtSOComplete.Rows[i]["ExDate"] = ExpectedDate;
                            dtSOComplete.Rows[i]["Quantity"] = Convert.ToInt32(dr["Quantity"]);

                            TimeSpan dts = Convert.ToDateTime(ExpectedDate) - Convert.ToDateTime(dtSOComplete.Rows[i]["DeliveryDate"].ToString());
                            dtSOComplete.Rows[i]["EarlyOrLateBy"] = dts.Days;
                        }
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }

                }

                var result = dtSOComplete.AsEnumerable()
.Where(r => DateTime.TryParse(r["ExDate"]?.ToString(), out _)) // filter valid dates
.GroupBy(r => r.Field<string>("SOId"))
.Select(g => g.OrderBy(r => DateTime.Parse(r["ExDate"].ToString())).First());

                DataTable minDt = result.Any() ? result.CopyToDataTable() : dtSOComplete.Clone();

                foreach (DataRow row in minDt.Rows)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + row["SOId"].ToString() + "'";
                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.BeginEdit();

                        drmo["SoProdCompDate"] = row["ExDate"];
                        drmo["EarlyOrLateBy"] = row["EarlyOrLateBy"].ToString();
                        drmo.EndEdit();
                    }
                }

               
                DataSet dtTrial;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.ExpectedSOWiseProductionCompletion  where EntityId in (" + entityid + @")", out dtTrial, false, "1");
                while (dtTrial.Tables[0].DefaultView.Count > 0)
                {
                    dtTrial.Tables[0].DefaultView[0].Delete();
                }
                int _count = 0;
                DataRow da = null;
                foreach (DataRow row in minDt.Rows)
                {
                    _count++;
                    da = dtTrial.Tables[0].NewRow();
                   
                    da["Id"] = entityid + "-" + _count;
                    da["EntityId"] = entityid;
                    da["ProductionOrderId"] = row["ProductionOrderId"];
                    da["SalesOrderId"] = row["SOId"];
                    da["ExpectedCompletionDate"] = row["ExDate"];
                    da["Quantity"] = row["Quantity"];

                    dtTrial.Tables[0].Rows.Add(da);

                }
                                
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dtTrial);
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

        public void GetProductionOrderMaster(out DataTable dtOrderMaster, string entityid)
        {
            try
            {
                string sql = @"Select * from(Select row_number() over (partition by po.Id order by po.Id,A.Date) as Seq
,po.Id POId,sc.ID ScheduleId,PS.UserName POStatus,FORMAT(PO.AddedDate,'dd-MMM-yyyy')POCreationDate ,FORMAT(BASEP.BaseProcProdStartDate,'dd-MMM-yyyy')BaseProcProdStartDate,FORMAT(BASEP.BaseProductionEndDate,'dd-MMM-yyyy')BaseProductionEndDate
,FORMAT(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy')BaseProcPlanStartDate,FORMAT(Type1.BaseProcPlanEndDate,'dd-MMM-yyyy')BaseProcPlanEndDate
,POStartDate=FORMAT(case when Type1.BaseProcPlanStartDate is null or BASEP.BaseProcProdStartDate  <  Type1.BaseProcPlanStartDate then BASEP.BaseProcProdStartDate else Type1.BaseProcPlanStartDate end,'dd-MMM-yyyy')
,POCompletionDate=FORMAT((case when Type1.BaseProcPlanEndDate is null or BASEP.BaseProductionEndDate  > Type1.BaseProcPlanEndDate then BASEP.BaseProductionEndDate else Type1.BaseProcPlanEndDate end ),'dd-MMM-yyyy')
,COUNT(SO.id) NoOfSO
,FORMAT(A.Date,'dd-MMM-yyyy') Date

,PlanningStatus=CASE WHEN FORMAT(case when Type1.BaseProcPlanStartDate is null or BASEP.BaseProcProdStartDate  <  Type1.BaseProcPlanStartDate then BASEP.BaseProcProdStartDate else Type1.BaseProcPlanStartDate end,'dd-MMM-yyyy') IS NULL 
OR FORMAT((case when Type1.BaseProcPlanEndDate is null or BASEP.BaseProductionEndDate  > Type1.BaseProcPlanEndDate then BASEP.BaseProductionEndDate else Type1.BaseProcPlanEndDate end ),'dd-MMM-yyyy') IS NULL OR SC.Id IS NULL THEN 'Schedule Missing' ELSE 'Schedule' END
,POCompletion= CASE WHEN A.Date<= GETDATE() Then 'Complete' else 'Scheduled' END 
,A.ProdQty,A.PlanQty,AvailableQty= CASE WHEN ISNULL(A.ProdQty,0)>0 THEN A.ProdQty ELSE A.PlanQty END

,CumProdQty=SUM(CASE WHEN ISNULL(A.ProdQty,0)>0 THEN A.ProdQty ELSE A.PlanQty END) OVER(PARTITION BY PO.ID ORDER BY A.Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)
,Quantity= A.ProdQty+A.PlanQty

FROM trn.SalesOrder SO
LEFT JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=so.Id
LEFT JOIN TRN.ProductionOrder PRD ON PRD.Id=POD.ProductionOrderId
LEFT JOIN(Select MIN(ProductionDate)BaseProcProdStartDate,MAX(ProductionDate)BaseProductionEndDate,A.ProductionOrderId 
FROM TRN.ProductionSummary A
left join TRN.ProductionOrderProcessSet B ON B.ProductionOrderId=A.ProductionOrderId  AND B.ProcessId=A.ProcessId Where B.IsBaseProcess=1
Group By A.ProductionOrderId) BASEP ON BASEP.ProductionOrderId=POD.ProductionOrderId

LEFT JOIN(Select MIN(A.ProductionDate)BaseProcPlanStartDate,MAX(A.ProductionDate)BaseProcPlanEndDate,A.ProductionOrderId 
From ProductionPlanningType1 A
Group By A.ProductionOrderId) Type1 ON Type1.ProductionOrderId=POD.ProductionOrderId
LEFT JOIN TRN.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN dbo.ProductionOrderSchedulingParametersType1 SC ON Sc.ProductionOrderID=PO.Id
LEFT JOIN(
Select B.* from
(
Select PS.ProductionOrderId POId,PS.ProductionDate Date,SUM(Quantity)ProdQty,0 PlanQty from TRN.ProductionOrder PO
LEFT JOIN TRN.ProductionSummary PS ON PS.ProductionOrderId=PO.Id
left join TRN.ProductionOrderProcessSet A ON A.ProductionOrderId=PS.ProductionOrderId  AND PS.ProcessId=A.ProcessId Where A.IsBaseProcess=1 Group BY PS.ProductionOrderId,PS.ProductionDate
UNION
Select DISTINCT PO.Id POId,T1.ProductionDate Date, 0 ProdQty,SUM(T1.Quantity) PlanQty 
from TRN.ProductionOrder PO
LEFT JOIN dbo.ProductionPlanningType1 T1 ON T1.ProductionOrderID=PO.Id
Group BY PO.Id,T1.ProductionDate
)B Where ISNULL(B.Date,'')<>'' 
)A ON A.POId=PO.Id

Where SO.OrderStatusId NOT IN('Cancelled','Closed') and pod.ProductionOrderId<>'' AND PRD.EntityId='" + entityid + @"'  AND A.Date<>''
GROUP BY po.Id,BASEP.BaseProcProdStartDate,BASEP.BaseProductionEndDate,Type1.BaseProcPlanStartDate,Type1.BaseProcPlanEndDate
,A.Date,sc.ID,PS.UserName,PO.AddedDate,A.ProdQty,A.PlanQty)x";
                dtOrderMaster = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetSOCompletionData(out DataTable dt, string entityid)
        {
            try
            {
                string sql = @"SELECT row_number() over (partition by POD.ProductionOrderId order by POD.ProductionOrderId,SO.DeliveryDate,SO.Qty,SO.Id) as Seq,
POD.ProductionOrderId,SO.OrderStatusId SOStatus,m.[Days]
,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy')DeliveryDate,SO.Id SOId,SO.Qty SOQty
,SoCommqty=SUM(SO.Qty) OVER (PARTITION BY POD.ProductionOrderId ORDER BY SO.DeliveryDate ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)
,P.UserName Customer,MOI.BuyerReferenceNo,moi.OwnReferenceNo,moi.Id LineitemId,MMA.StandardName Article,PL.Code ProductCode
,ProductLibraryDetail=STUFF((select distinct ','+MA.Code+'-'+MA.AttributeValue from
												[dbo].ProductLibraryAttribute MA												
												where MA.ProductLibraryId=PL.Id for xml path('') ), 1, 1, '')

,PS.UserName POStatus,FORMAT(SO.PlanExFactoryDate,'dd-MMM-yyyy')ExFactoryDate,FORMAT(SO.CommitmentDate,'dd-MMM-yyyy')CommitmentDate,RP.EmployeeName ResponsiblePerson,E.UserName Entity,DiffComEx=CASE  WHEN SO.CommitmentDate IS NULL THEN DATEDIFF(DAY,PlanExFactoryDate,GETDATE()) ELSE DATEDIFF(DAY,SO.CommitmentDate,GETDATE()) END,'' ExDate,''EarlyOrLateBy,so.OrderStatusId,0 Quantity
from trn.SalesOrder SO
left join TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
left join TRN.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet M ON m.ProductionOrderId=POD.ProductionOrderId
AND m.Id=(SELECT TOP 1 ID FROM TRN.ProductionOrderProcessSet EII WHERE EII.ProductionOrderId=POD.ProductionOrderId ORDER BY EII.Sequence DESC)
LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
LEFT JOIN TRN.MasterOrder MO ON MO.Id=MOI.MasterOrderId
LEFT JOIN HKP.Party P ON P.Id=MO.PartyId
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=MOI.ArticleId
LEFT JOIN [dbo].[ProductLibrary] PL ON PL.Id=MOI.ProductLibraryId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN dbo.EmployeeInformation RP ON RP.SystemId=SO.ResponsiblePersonId
LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
Where  SO.OrderStatusId NOT IN('Cancelled','Hold') AND POD.ProductionOrderId<>'' and PO.EntityId='" + entityid+"'";

                dt = _sqlRepository.GetDataTable(sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataRow GetExpectedSOCompletionDate(double RequiredQty, string POId, DataTable Data)
        {
            for (int i = 0; i < Data.Rows.Count; i++)
            {
                if (Data.Rows[i]["POId"].ToString() == POId)
                {

                    if (clsStaticInfo.dbl(Data.Rows[i]["CumProdQty"].ToString()) >= RequiredQty)
                    {
                        return Data.Rows[i];
                    }
                }
            }


            return null;
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




