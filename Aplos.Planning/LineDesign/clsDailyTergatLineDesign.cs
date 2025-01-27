using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Planning.LineDesign
{
    public class clsDailyTergatLineDesign
    {
        ISqlRepository _sqlRepository;
        public clsDailyTergatLineDesign()
        {
            _sqlRepository = new SqlRepository();
        }
        public void CopyFromTable(string entityid, string processId, string ProductionDate, Dictionary<string, object> SelectedLine)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string NewId = "";

                string sql = @"SELECT po.id
                                	,LL.Id AS CurrentLayoutId
                                	,LLP.Id AS PreviousLayoutId
                                	,l.Id AS BulletinTemplateLayoutId
                                FROM trn.ProductionOrder po
                                LEFT JOIN LineLayoutDailyTarget LL ON LL.ProcessId = '" + processId + @"'                                
                                    AND LL.ProductionOrderId = po.Id                                
                                    AND LL.TargetDate = '" + ProductionDate + @"'                                
                                    AND LL.WorkCenterMasterId = '" + SelectedLine["WorkCenterMasterId"] + @"'
                                LEFT JOIN LineLayoutDailyTarget LLP ON LLP.ProcessId = '" + processId + @"'                                
                                    AND LLP.ProductionOrderId = po.Id                                
                                    AND LLP.TargetDate = (
                                        SELECT TOP 1 X.TargetDate
                                        FROM LineLayoutDailyTarget x                                
                                        WHERE x.ProductionOrderId = po.id                                
                                            AND x.ProcessId = '" + processId + @"'                                
                                            AND x.WorkCenterMasterId = '" + SelectedLine["WorkCenterMasterId"] + @"'                                
                                            AND x.TargetDate < '" + ProductionDate + @"'                             
                                        ORDER BY x.TargetDate DESC
                                		)
                                	AND LLP.WorkCenterMasterId = '" + SelectedLine["WorkCenterMasterId"] + @"'
                                LEFT JOIN LineLayoutByProductionBulletin l ON l.ProcessId = '" + processId + @"'
                                
                                    AND l.ProductionOrderId = po.Id
                                WHERE Po.Id = '" + SelectedLine["PRNo"] + @"'";

                DataTable dt = _sqlRepository.GetDataTable(sql);

                if (dt.Rows[0]["CurrentLayoutId"].ToString() != "")
                    throw new Exception("You already have the Layout");
                if (dt.Rows[0]["PreviousLayoutId"].ToString() == "" && dt.Rows[0]["BulletinTemplateLayoutId"].ToString() == "")
                    throw new Exception("No source layout was found");

                DataSet LineLayoutDailyTarget, LineLayoutDailyTargetData;

                con.OpenDataSetThroughAdapter("select * from LineLayoutDailyTarget where 1=2", out LineLayoutDailyTarget, false, "1");
                con.OpenDataSetThroughAdapter("select * from LineLayoutDailyTargetData where 1=2", out LineLayoutDailyTargetData, false, "1");

                DataTable LineLayoutByProductionBulletin = null;
                DataTable LineLayoutByProductionBulletinData = null;

                if (dt.Rows[0]["PreviousLayoutId"].ToString() != "")
                {
                    LineLayoutByProductionBulletin = _sqlRepository.GetDataTable("select * from LineLayoutDailyTarget WHERE Id='" + dt.Rows[0]["PreviousLayoutId"].ToString() + "'");
                    LineLayoutByProductionBulletinData = _sqlRepository.GetDataTable("select * from LineLayoutDailyTargetData WHERE LineLayoutDailyTargetId='" + dt.Rows[0]["PreviousLayoutId"].ToString() + "'");
                }
                else
                {
                    LineLayoutByProductionBulletin = _sqlRepository.GetDataTable("select * from LineLayoutByProductionBulletin WHERE Id='" + dt.Rows[0]["BulletinTemplateLayoutId"].ToString() + "'");
                    LineLayoutByProductionBulletinData = _sqlRepository.GetDataTable("select * from LineLayoutByProductionBulletinData WHERE LineLayoutByProductionBulletinId='" + dt.Rows[0]["BulletinTemplateLayoutId"].ToString() + "'");
                }

                NewId = GetPK("LineLayoutDailyTarget");

                CopyDataTable(LineLayoutByProductionBulletin, LineLayoutDailyTarget.Tables[0], NewId);
                CopyDataTable(LineLayoutByProductionBulletinData, LineLayoutDailyTargetData.Tables[0], NewId);

                DataRow dr = LineLayoutDailyTarget.Tables[0].Rows[0];
                dr["TargetDate"] = ProductionDate;
                dr["WorkCenterMasterId"] = SelectedLine["WorkCenterMasterId"];
                dr["ProcessId"] = processId;
                dr["EntityId"] = entityid;

                NewId = dr["Id"].ToString();
                SetForeignKey(LineLayoutDailyTargetData, "LineLayoutDailyTargetId", NewId);

                OTSBD.clsStaticInfo info = new OTSBD.clsStaticInfo();
                info.SaveDataSets(LineLayoutDailyTarget, LineLayoutDailyTargetData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetPK(string TableName)
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), TableName, out sID);
            return sID;
        }
        private void CopyDataTable(DataTable dtSource, DataTable dtDestination, string PK)
        {
            int Index = 0;
            foreach (DataRow drSource in dtSource.Rows)
            {
                Index++;
                DataRow drDestination = dtDestination.NewRow();
                CopyRow(drSource, ref drDestination);
                if (PK != "")
                    drDestination["Id"] = PK + Index;
                dtDestination.Rows.Add(drDestination);
            }
        }
        private void SetForeignKey(DataSet ds, string ColumnName, string KeyValue)
        {
            foreach (DataRow drSource in ds.Tables[0].Rows)
            {
                drSource[ColumnName] = KeyValue;

            }
        }
        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = drSource[drSource.Table.Columns[COL].ColumnName];

                }
                catch (Exception ex)
                {
                }
                try
                {
                    drDestination["AddedBy"] = identity.Name;
                    drDestination["AddedDate"] = DateTime.Now;
                    drDestination["AddedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedBy"] = identity.Name;
                    drDestination["UpdatedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedDate"] = DateTime.Now;

                }
                catch (Exception ex)
                {
                }
            }

        }

        public List<Dictionary<string, object>> GetDesign(string WorkCenterMasterId, string ProductionOrderId, string TargetDate)
        {

            try
            {
                string sql = @"select * from LineLayoutDailyTarget where WorkCenterMasterId = '" + WorkCenterMasterId + @"' AND  ProductionOrderId='" + ProductionOrderId + @"' AND TargetDate='" + TargetDate + @"'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetBottleneck(string WorkCenterMasterId, string ProductionOrderId, string TargetDate, string ProcessId, out List<Dictionary<string, object>> StripLine, out List<Dictionary<string, object>> Data)
        {

            try
            {
                string sql = @"SELECT BT.ProductionOrderId--,TG1.HourlyTargetAtHundredPercent,TGD.HourlyTarget
                                ,100.00 AS hundredPercent
                                ,CONVERT(DECIMAL(18,2),TGD.HourlyTarget/TG1.HourlyTargetAtHundredPercent*100) AS HourlyTargetPercent
                                ,tm.BottleNeckPercentage
                                ,LowerBoundValue=CASE WHEN ISNULL(CONVERT(DECIMAL(18,2),TGD.HourlyTarget/TG1.HourlyTargetAtHundredPercent*100),0)<tm.BottleNeckPercentage THEN
                                ISNULL(CONVERT(DECIMAL(18,2),TGD.HourlyTarget/TG1.HourlyTargetAtHundredPercent*100),0) ELSE ISNULL(tm.BottleNeckPercentage,0) END
                                ,LowerBoundText=CASE WHEN ISNULL(CONVERT(DECIMAL(18,2),TGD.HourlyTarget/TG1.HourlyTargetAtHundredPercent*100),0)<tm.BottleNeckPercentage THEN
                                'Hourly Target' ELSE 'Bottleneck' END

                                ,UpperBoundValue=CASE WHEN ISNULL(CONVERT(DECIMAL(18,2),TGD.HourlyTarget/TG1.HourlyTargetAtHundredPercent*100),0)>tm.BottleNeckPercentage THEN
                                ISNULL(CONVERT(DECIMAL(18,2),TGD.HourlyTarget/TG1.HourlyTargetAtHundredPercent*100),0) ELSE ISNULL(tm.BottleNeckPercentage,0) END
                                ,UpperBoundText=CASE WHEN ISNULL(CONVERT(DECIMAL(18,2),TGD.HourlyTarget/TG1.HourlyTargetAtHundredPercent*100),0)>tm.BottleNeckPercentage THEN
                                'Hourly Target' ELSE 'Bottleneck' END

                                FROM

                            trn.ProductionBulletinTemplateMaster AS TM 
                            JOIN trn.ProductionBulletinTemplate BT ON bt.Id=tm.ProductionBulletinTemplateId

                            LEFT JOIN (SELECT BTD.ProductionBulletinTemplateMasterId,CONVERT(INT, 60/SUM(BTD.TotalSPT)*SUM(AllotedManpower)) AS HourlyTargetAtHundredPercent 
                            FROM TRN.ProductionBulletinTemplateDetail BTD
                            GROUP BY BTD.ProductionBulletinTemplateMasterId) AS TG1 ON  tg1.ProductionBulletinTemplateMasterId=TM.Id

                            LEFT JOIN (SELECT dpt.ProductionOrderId,dpt.QuantityPerHour AS HourlyTarget FROM trn.DailyProductionTarget AS dpt 
                                       WHERE dpt.ProductionOrderId='" + ProductionOrderId + @"' AND dpt.WorkCenterMasterID='" + WorkCenterMasterId + @"' AND dpt.TargetDate='" + TargetDate + @"'
                                        )
                            AS TGD ON tgd.ProductionOrderId=bt.ProductionOrderId
                            WHERE BT.ProductionOrderId='" + ProductionOrderId + @"' AND tm.ProcessId='" + ProcessId + @"'  ";

                StripLine = _sqlRepository.GetDataCollection(sql);


                sql = @"SELECT TG.*,ISNULL(CONVERT(DECIMAL(18,2),(prd.ProductionQuantity/p.Sequence)/tg1.HourlyTargetAtHundredPercent*100),0) AS AverageProduction,p.Sequence
                              FROM (SELECT DISTINCT  ov.Id,ov.UserName AS Operationvariation,isnull(OV.Color,'#2E86C1') AS Color
                              FROM LineLayoutDailyTarget AS T
                            LEFT JOIN LineLayoutDailyTargetData AS D ON d.LineLayoutDailyTargetId=t.Id
                            LEFT JOIN mst.OperationVariation AS ov ON ov.Id=d.OperationVariationId

                            WHERE t.ProductionOrderId='" + ProductionOrderId + @"' AND t.WorkCenterMasterID='" + WorkCenterMasterId + @"' AND t.TargetDate='" + TargetDate + @"'
                            ) AS TG
                            LEFT JOIN 

                            (SELECT t.OperationVariationId,SUM(t.Quantity) AS ProductionQuantity FROM trn.DailyProduction AS T
                            WHERE t.ProductionOrderId='" + ProductionOrderId + @"' AND t.WorkCenterMasterID='" + WorkCenterMasterId + @"' AND t.ProductionDate='" + TargetDate + @"'
                            GROUP BY t.OperationVariationId
                            ) AS PRD ON prd.OperationVariationId=tg.Id
                            LEFT JOIN hkp.ProductionBookingPeriod AS P ON p.Id=(SELECT TOP 1 Id FROM hkp.ProductionBookingPeriod AS X WHERE CONVERT(DATE,CONCAT(FORMAT(GETDATE(),'dd-MMM-yyyy'),' ', FORMAT(X.EndTime,'hh:mm:ss tt')))<=GETDATE() ORDER BY X.EndTime DESC)
                            LEFT JOIN (SELECT CONVERT(INT, 60/SUM(BTD.TotalSPT)*SUM(AllotedManpower)) AS HourlyTargetAtHundredPercent 
                            FROM TRN.ProductionBulletinTemplateDetail BTD
                            JOIN trn.ProductionBulletinTemplateMaster AS TM ON btd.ProductionBulletinTemplateMasterId=tm.Id
                            JOIN trn.ProductionBulletinTemplate BT ON bt.Id=tm.ProductionBulletinTemplateId
                                       WHERE bt.ProductionOrderId='" + ProductionOrderId + @"' AND TM.ProcessId='" + ProcessId + @"' ) AS TG1 ON  1=1";


                Data = _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SaveData(List<Html> Nodes, string Design, string WorkCenterMasterId, string ProductionOrderId, string TargetDate, out DataSet dsData)
        {
            bplib.clsGenID objGenID = null;
            string idFromDB = "";
            string idFromDBC = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            List<addInfo> HtmlsInfo = new List<addInfo>();
            for (int i = 0; i < Nodes.Count; i++)
            {
                try
                {
                    Html TempHtml = Nodes[i];
                    if (TempHtml == null)
                        continue;

                    if (TempHtml.addInfo == null)
                        continue;

                    if (string.IsNullOrEmpty(TempHtml.addInfo.OperationVariationId))
                        continue;

                    HtmlsInfo.Add(TempHtml.addInfo);
                }
                catch (Exception ex)
                {

                }
            }

            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("select L.EmployeeSystemId,E.EmployeeCode,E.EmployeeName,FixedAssetRegisterId from LineLayoutDailyTargetData L  " +
                @" left join EmployeeInformation E on e.SystemId=L.EmployeeSystemId " +
                @" WHERE L.LineLayoutDailyTargetId=(select top 1 Id from LineLayoutDailyTarget " +
                "where WorkCenterMasterId <> '" + WorkCenterMasterId + @"' AND TargetDate='" + TargetDate + @"')", out DataSet dsVaidationChild, false, "1");

            for (int j = 0; j < HtmlsInfo.Count; j++)
            {
                if (string.IsNullOrEmpty(HtmlsInfo[j].EmployeeId) == false)
                {

                    dsVaidationChild.Tables[0].DefaultView.RowFilter = "EmployeeSystemId='" + HtmlsInfo[j].EmployeeId + "'";
                    if (dsVaidationChild.Tables[0].DefaultView.Count > 0)
                        throw new Exception("employee has already tagged with other work center for the day [" + dsVaidationChild.Tables[0].DefaultView[0]["EmployeeCode"].ToString() + @"-" + dsVaidationChild.Tables[0].DefaultView[0]["EmployeeName"].ToString() + @"]");

                    var xy = HtmlsInfo.Where(H => H.EmployeeId == HtmlsInfo[j].EmployeeId).ToList();
                    if (xy.Count > 1)
                    {
                        throw new Exception("Duplicate employee found in layout " + xy[0].EmployeeCode + "-" + xy[0].EmployeeName);
                    }
                }

                if (string.IsNullOrEmpty(HtmlsInfo[j].FixedAssetRegisterId) == false)
                {

                    dsVaidationChild.Tables[0].DefaultView.RowFilter = "FixedAssetRegisterId='" + HtmlsInfo[j].FixedAssetRegisterId + "'";
                    if (dsVaidationChild.Tables[0].DefaultView.Count > 0)
                        throw new Exception("Machine has already tagged with other work center for the day [" + dsVaidationChild.Tables[0].DefaultView[0]["FixedAssetRegisterId"].ToString() + "]");

                    var xy = HtmlsInfo.Where(H => H.FixedAssetRegisterId == HtmlsInfo[j].FixedAssetRegisterId).ToList();
                    if (xy.Count > 1)
                    {
                        throw new Exception("Duplicate machine found in layout " + xy[0].FixedAssetRegisterId);
                    }
                }
            }


            DataSet dsMaster, dsChild;
            con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("select * from LineLayoutDailyTarget where WorkCenterMasterId = '" + WorkCenterMasterId + @"' AND  ProductionOrderId='" + ProductionOrderId + @"' AND TargetDate='" + TargetDate + @"'", out dsMaster, false, "1");
            con.OpenDataSetThroughAdapter("select * from LineLayoutDailyTargetData where LineLayoutDailyTargetId=(select top 1 Id from LineLayoutDailyTarget where WorkCenterMasterId = '" + WorkCenterMasterId + @"' AND  ProductionOrderId='" + ProductionOrderId + @"' AND TargetDate='" + TargetDate + @"')", out dsChild, false, "1");

            string PrimaryKey = "";
            if (dsMaster.Tables[0].Rows.Count > 0)
            {

                DataRow dr = dsMaster.Tables[0].Rows[0];
                PrimaryKey = dr["Id"].ToString();
                dr.BeginEdit();
                dr["Layout"] = Design;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = DateTime.Now;
                dr["UpdatedFromIP"] = identity.IPAddress;
                dr.EndEdit();
            }

            //delete missing items from db
            while (dsChild.Tables[0].DefaultView.Count > 0)
                dsChild.Tables[0].DefaultView[0].Delete();



            string ChildPK = "";
            for (int i = 0; i < HtmlsInfo.Count; i++)
            {
                if (ChildPK == "")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "LineLayoutByProductionBulletinData", out idFromDBC);
                    ChildPK = "LD-" + idFromDBC;
                }
                DataRow dr = dsChild.Tables[0].NewRow();
                dr["Id"] = ChildPK + "-" + (i + 1);
                dr["LineLayoutDailyTargetId"] = PrimaryKey;
                dr["OperationId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].OperationId));
                dr["MaterialMasterId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].MaterialMasterId));
                dr["ArticleId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].ArticleId));
                dr["OperationVariationId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].OperationVariationId));
                dr["OperationId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].OperationId));

                dr["EmployeeSystemId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].EmployeeId));
                dr["FixedAssetRegisterId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].FixedAssetRegisterId));
                dr["Sequence"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].Sequence));

                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = DateTime.Now;
                dr["AddedFromIP"] = identity.IPAddress;

                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = DateTime.Now;
                dr["UpdatedFromIP"] = identity.IPAddress;
                dsChild.Tables[0].Rows.Add(dr);
            }
            OTSBD.clsStaticInfo SaveInfo = new OTSBD.clsStaticInfo();
            SaveInfo.SaveDataSets(dsMaster, dsChild);

            DataSet dsDailyProduction;

            GetTotalHandOrMachineData(ProductionOrderId, TargetDate, WorkCenterMasterId, out dsData);
            con.OpenDataSetThroughAdapter("SELECT * FROM trn.DailyProductionTarget AS dpt WHERE dpt.TargetDate='" + TargetDate + "' AND dpt.ProductionOrderId='" + ProductionOrderId + "' AND dpt.WorkCenterMasterID='" + WorkCenterMasterId + "'", out dsDailyProduction, false, "1");

            if (dsData.Tables[0].Rows.Count > 0)
            {
                DataRow drx = dsDailyProduction.Tables[0].Rows[0];
                drx.BeginEdit();
                drx["ManPowerWithMachine"] = dsData.Tables[0].Rows[0]["TotalMachine"].ToString();
                drx["ManPowerWithHand"] = dsData.Tables[0].Rows[0]["TotalHand"].ToString();
                drx.EndEdit();
            }
            SaveInfo.SaveDataSets(dsDailyProduction);
        }
        //void SaveDailyProduction(ref DataSet dsSaveBonusMaster, DataSet dsData)
        //{
        //    DataView _dvSave = null;
        //    try
        //    {
        //        _dvSave = new DataView(dsSaveBonusMaster.Tables[0]);

        //        DataRow drx = _dvSave[0].Row;
        //        drx.BeginEdit();
        //        drx["ManPowerWithMachineBulletin"] = dsData.Tables[0].Rows[0]["TotalMachine"].ToString();
        //        drx["ManPowerWithHandBulletin"] = dsData.Tables[0].Rows[0]["TotalHand"].ToString();
        //        drx.EndEdit();

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        public void GetTotalHandOrMachineData(string ProductionOrderId, string TargetDate, string WorkCenterMasterId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SUM( CASE WHEN ISNULL(o.IsMachineRequired,'')='M'  THEN 1 ELSE 0 END) AS TotalMachine,
		                          SUM( CASE WHEN ISNULL(o.IsMachineRequired,'')!='M' THEN 1 ELSE 0 END) AS TotalHand
                                        FROM [MST].[OperationVariation] OV
                                        join LineLayoutDailyTargetData AS lldtd ON lldtd.OperationVariationId = OV.Id
                                        JOIN  LineLayoutDailyTarget AS lldt ON lldt.Id = lldtd.LineLayoutDailyTargetId
                                        LEFT JOIN [MST].[MaterialMasterArticle] M ON M.Id = OV.ArticleId
                                        LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=m.MaterialMasterId
                                        LEFT JOIN [MST].[Operation] O ON O.Id = OV.OperationId
                                        LEFT JOIN hkp.ProductionSystem AS ps ON ps.Id=o.ProductionSystemId
                                        where lldt.ProductionOrderId='" + ProductionOrderId + "' " +
                                        "AND lldt.TargetDate='" + TargetDate + "' AND lldt.WorkCenterMasterId='" + WorkCenterMasterId + "' ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void SaveProductionData(List<Dictionary<string, object>> HtmlsInfo, string WorkCenterMasterId, string ProductionOrderId, string TargetDate)
        {
            bplib.clsGenID objGenID = null;
            string idFromDBC = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            DataSet dsMaster, dsChild;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("SELECT * FROM trn.DailyProduction AS dp where 1=2", out dsChild, false, "1");

            string ProductionTime = TargetDate + " " + System.DateTime.Now.ToString("hh:mm:ss tt");
            string PeriodId = @"SELECT TOP 1 PX.Id
                                                      FROM hkp.ProductionBookingPeriod PX WHERE CONVERT(DATETIME,FORMAT(CONVERT(DATETIME,'" + ProductionTime + @"'),'dd-MMM-yyyy hh:mm tt')) BETWEEN 
                                CONVERT(DATETIME,CONCAT(FORMAT(CONVERT(DATETIME,'" + ProductionTime + @"'),'dd-MMM-yyyy'),' ',FORMAT(PX.StartTime,'hh:mm tt')))  AND
                                CONVERT(DATETIME,CONCAT(FORMAT(CONVERT(DATETIME,'" + ProductionTime + @"'),'dd-MMM-yyyy'),' ',FORMAT(PX.EndTime,'hh:mm tt')))

                                 ORDER BY px.StartTime ASC";
            con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter(PeriodId, out DataSet dsPeriod, false, "1");
            string ProductionPeriodId = "";
            if (dsPeriod.Tables[0].Rows.Count > 0)
                ProductionPeriodId = dsPeriod.Tables[0].Rows[0]["Id"].ToString();

            string ChildPK = "";
            for (int i = 0; i < HtmlsInfo.Count; i++)
            {
                if (OTSBD.clsStaticInfo.dbl(HtmlsInfo[i]["CurrentQuantity"]) == 0)
                    continue;

                if (OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i]["EmployeeId"]) == "")
                    continue;

                if (OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i]["OperationVariationId"]) == "")
                    continue;

                if (ChildPK == "")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "DailyProduction", out idFromDBC);
                    ChildPK = "DPD-" + idFromDBC;
                }
                DataRow dr = dsChild.Tables[0].NewRow();
                dr["Id"] = ChildPK + "-" + (i + 1);

                dr["PlantID"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(identity.PlantId));
                dr["WorkCenterMasterID"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(WorkCenterMasterId));
                dr["ResponsiblePersonID"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i]["EmployeeId"].ToString()));
                dr["EmployeeInformationSystemID"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i]["EmployeeId"].ToString()));
                dr["ProductionDate"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(TargetDate));
                dr["Quantity"] = OTSBD.clsStaticInfo.dbl(HtmlsInfo[i]["CurrentQuantity"]);
                dr["ProductionTime"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(ProductionTime));
                dr["ProductionOrderId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(ProductionOrderId));
                dr["ProductionBookingPeriodId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(ProductionPeriodId));
                dr["OperationVariationId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i]["OperationVariationId"].ToString()));

                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = DateTime.Now;
                //dr["AddedFromIP"] = identity.IPAddress;

                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = DateTime.Now;
                //dr["UpdatedFromIP"] = identity.IPAddress;
                dsChild.Tables[0].Rows.Add(dr);
            }

            OTSBD.clsStaticInfo SaveInfo = new OTSBD.clsStaticInfo();
            SaveInfo.SaveDataSets(dsChild);
        }

        public List<Dictionary<string, object>> SearchEmployee(string column, string value, string OperationId, string OperationVariationId, string TargetDate)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            string sql = @"
                      select top 10000 * from (  
                        SELECT distinct Emp.SystemID AS Id,dt.WorkCenterMasterId,apd.DayStatus,ISNULL(dt2.ColorCode,'#000000') AS DayColor,
CONVERT(BIT,CASE WHEN ISNULL(dtd.EmployeeSystemId,'')='' THEN 0 ELSE 1 END) AS IsAssigned,
CASE WHEN ISNULL(dtd.EmployeeSystemId,'')='' THEN 'Unassigned' ELSE CONCAT('Assigned to ',wcm.UserName,' for ',ov.UserName) END AS AssignmentStatus,
                        EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
						isnull(D.UserName,'') Designation,
                                        OtherSkills =STUFF((select distinct ','+ovx.UserName 
                                        FROM EmployeeOperation AS eox
				                        JOIN mst.OperationVariation AS ovx ON ovx.Id=eox.OperationVariationId                                             
			                            where eox.EmpSystemId=EMP.SystemId and eox.OperationVariationId<>EMP.OperationVariationId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            join (
                            	SELECT EmpSystemId,OperationVariationId FROM EmployeeOperation 
								UNION SELECT SystemId,OperationVariationId FROM EmployeeInformation
                            )AS eo ON EO.EmpSystemId=EMP.SystemId AND EO.OperationVariationId='" + OperationVariationId + @"'
                            LEFT JOIN LineLayoutDailyTargetData AS DTD ON dtD.EmployeeSystemId=emp.SystemId 
														AND dtD.LineLayoutDailyTargetId IN (SELECT Id FROM LineLayoutDailyTarget AS X Where x.TargetDate='" + TargetDate + @"')
							LEFT JOIN LineLayoutDailyTarget		DT ON dt.Id=dtd.LineLayoutDailyTargetId		
							LEFT JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=dt.WorkCenterMasterId
							LEFT JOIN mst.OperationVariation AS ov ON ov.Id=dtd.OperationVariationId
							LEFT JOIN AttdnProcessData AS apd ON apd.EmpSystemID=emp.SystemId AND apd.WorkDate='" + TargetDate + @"'
							LEFT JOIN DayType AS dt2 ON dt2.DayType=apd.DayStatus
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
   
                        WHERE emp.EmployeeStatus='Active'  
                ) AS TEMP where " + strkey + " Order By IsAssigned,Id";





            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> SearchFixedAsset(string column, string value, string ArticleId)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            string sql = @"
                      select * from (  
                     SELECT far.Id,far.Model,mma.StandardName AS Article,mm.UserName AS Material, far.SerialNo, format(far.CapitalizationDate,'dd-MMM-yyyyy') CapitalizationDate,format(far.InvoiceDate,'dd-MMM-yyyyy') InvoiceDate,
                           far.YearOfManufacture, far.YearOfInstallation, far.[Description],CONCAT(far.Id,'/',far.[Description]) AS FixedAssetDesc,
                           far.AssetNo, far.[Status], far.Remarks,cm.UserName AS Vendor,B.UserName AS Brand,c.UserName AS CountryOfOrigin,FR.UserName FixedAssetMaster,
                           A.UserName AS FixedAssetActivity
                      FROM trn.FixedAssetRegister AS far
                          LEFT JOIN MST.MaterialMaster MM ON far.MaterialMasterId= MM.Id
                                   LEFT JOIN MST.MaterialMasterArticle MMA ON far.MaterialMasterArticleId= MMA.Id

                                LEFT JOIN hkp.Party AS cm ON cm.Id=far.VendorId
                                LEFT JOIN SCS.Brand B ON b.Id=far.BrandId
                                LEFT JOIN SCS.Country C ON c.Id=far.CountryOfOriginId
                                LEFT JOIN MST.FixedAssetMaster FR ON fr.Id=far.FixedAssetMasterId
                                LEFT JOIN HKP.Activity A ON A.Id=far.FAActivityId

                                WHERE far.MaterialMasterArticleId='" + ArticleId + @"' and ISNULL(far.DisposedVoucherId,'')=''
                ) AS TEMP where " + strkey + " Order By SerialNo";





            return _sqlRepository.GetDataCollection(sql);
        }
        public List<List<Dictionary<string, object>>> GetEmployeeCard(string EmployeeId, string OperationVariationId, string AssetRegisterId, string TargetDate)
        {

            List<List<Dictionary<string, object>>> data = new List<List<Dictionary<string, object>>>();
            string sql = @"SELECT distinct Emp.SystemID AS Id,apd.DayStatus,ISNULL(dt2.ColorCode,'#000000') AS DayColor,
                        FORMAT(apd.InTime,'dd-MMM-yyyy hh:mm:ss tt') AS InTime,FORMAT(apd.OutTime,'dd-MMM-yyyy hh:mm:ss tt') AS OutTime,sd.UserName AS ShiftName,

                        EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
						isnull(D.UserName,'') Designation,
                                        Skills =STUFF((select distinct ','+ovx.UserName 
                                        FROM EmployeeOperation AS eox
				                        JOIN mst.OperationVariation AS ovx ON ovx.Id=eox.OperationVariationId                                             
			                            where eox.EmpSystemId=EMP.SystemId 	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
                            NULL AS SkillList,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                          	LEFT JOIN AttdnProcessData AS apd ON apd.EmpSystemID=emp.SystemId AND apd.WorkDate='" + TargetDate + @"'
							LEFT JOIN DayType AS dt2 ON dt2.DayType=apd.DayStatus
							LEFT JOIN ShiftDefination AS sd ON sd.SystemID=apd.ShiftSystemID
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
   
                        WHERE emp.SystemId='" + EmployeeId + @"'";

            List<Dictionary<string, object>> _tempData = _sqlRepository.GetDataCollection(sql);
            sql = @"select * from (
                                    SELECT  ovx.UserName  AS SkillName,0 Sequence,0 CycleTime
                                                                            FROM employeeInformation AS eox
				                                                            JOIN mst.OperationVariation AS ovx ON ovx.Id=eox.OperationVariationId                                             
			                                                                where eox.SystemId='" + EmployeeId + @"'
                                        UNION all			                            
                                    
                                        select ovx.UserName  AS SkillName,eox.Sequence,eox.CycleTime
                                        FROM EmployeeOperation AS eox
				                        JOIN mst.OperationVariation AS ovx ON ovx.Id=eox.OperationVariationId                                             
			                            where eox.EmpSystemId='" + EmployeeId + @"' ) AS K ORDER BY Sequence";

            if (_tempData.Count > 0)
                _tempData[0]["SkillList"] = _sqlRepository.GetDataCollection(sql);
            data.Add(_tempData);



            data.Add(_sqlRepository.GetDataCollection(GetOperationVariationSql(OperationVariationId)));


            sql = @"    
                        SELECT far.Id,far.Model,mma.StandardName AS Article,mm.UserName AS Material, far.SerialNo, format(far.CapitalizationDate,'dd-MMM-yyyyy') CapitalizationDate,format(far.InvoiceDate,'dd-MMM-yyyyy') InvoiceDate,
                           far.YearOfManufacture, far.YearOfInstallation, far.[Description],CONCAT(far.Id,'/',far.[Description]) AS FixedAssetDesc,
                           far.AssetNo, far.[Status], far.Remarks,cm.UserName AS Vendor,B.UserName AS Brand,c.UserName AS CountryOfOrigin,FR.UserName FixedAssetMaster,
                           A.UserName AS FixedAssetActivity,mma.ShortName AS ArticleShortName
                      FROM 
                            mst.operationvariation ov
                         left join   trn.FixedAssetRegister AS far on far.MaterialMasterArticleId=ov.ArticleId and far.Id='" + AssetRegisterId + @"'
                                   LEFT JOIN MST.MaterialMasterArticle MMA ON ov.ArticleId= MMA.Id
                          LEFT JOIN MST.MaterialMaster MM ON mma.MaterialMasterId= MM.Id

                                LEFT JOIN hkp.Party AS cm ON cm.Id=far.VendorId
                                LEFT JOIN SCS.Brand B ON b.Id=far.BrandId
                                LEFT JOIN SCS.Country C ON c.Id=far.CountryOfOriginId
                                LEFT JOIN MST.FixedAssetMaster FR ON fr.Id=far.FixedAssetMasterId
                                LEFT JOIN HKP.Activity A ON A.Id=far.FAActivityId

                                WHERE ov.Id='" + OperationVariationId + @"'";

            data.Add(_sqlRepository.GetDataCollection(sql));

            return data;
        }
        public List<List<Dictionary<string, object>>> GetOperationVariationCard(string OperationVariationId)
        {

            List<List<Dictionary<string, object>>> data = new List<List<Dictionary<string, object>>>();


            data.Add(_sqlRepository.GetDataCollection(GetOperationVariationSql(OperationVariationId)));



            return data;
        }

        public List<Dictionary<string, object>> UpdateEmployeeAttendanceAndProductionInfo(string EmployeeId, string TargetDate)
        {


            string sql = @"SELECT ei.SystemId AS EmployeeId,apd.DayStatus,dt.ColorCode AS DayColor,ISNULL(dp.Quantity,0) AS ProductionQuantity
                                  FROM EmployeeInformation AS ei
                                LEFT JOIN AttdnProcessData AS apd ON apd.EmpSystemID=ei.SystemId AND apd.WorkDate='" + TargetDate + @"'
                                LEFT JOIN (SELECT dp.EmployeeInformationSystemID,SUM(dp.Quantity) AS Quantity
                                             from trn.DailyProduction AS dp 
                                           WHERE  dp.ProductionDate='" + TargetDate + @"' AND dp.EmployeeInformationSystemID IN(" + EmployeeId + @")
                                           GROUP BY dp.EmployeeInformationSystemID) AS dp ON dp.EmployeeInformationSystemID=ei.SystemId
                                LEFT JOIN DayType AS dt ON dt.DayType=apd.DayStatus
                                WHERE ei.SystemId IN (" + EmployeeId + @") ";

            return _sqlRepository.GetDataCollection(sql);
        }

        private string GetOperationVariationSql(string OperationVariationId)
        {

            string sql = @"SELECT o.UserName AS Operation,ov.UserName AS OperationVariation,mma.StandardName AS Article,s.UserName AS Skill,mm.UserName AS Material,o.IsMachineRequired,mma.RPM,sc.UserName AS StitchCode,
                        ov.SubOperationSAM, ov.AdditionalSAMSymbol, ov.AdditionalSAM, ov.Frequency,oc.UserName AS OperationCategory,
                        ov.MachineAllowance, ov.SPI, ov.Code, ov.TotalSAM, ov.AdditionalAllowance,
                        ov.VASFINALSAM,mma.ShortName AS ArticleShortName
                          FROM mst.OperationVariation AS ov
                        LEFT JOIN mst.Operation AS o ON o.Id=ov.OperationId
                        LEFT JOIN hkp.OperationCategory AS oc ON oc.Id=o.OperationCategoryId
                        LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=ov.ArticleId
                        LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=mma.MaterialMasterId
                        LEFT JOIN hkp.Skill AS s ON s.Id=ov.SkillId
                        LEFT JOIN hkp.StitchCode AS sc ON sc.Id=mma.StitchCodeId
                        WHERE ov.Id='" + OperationVariationId + @"'";

            return sql;
        }
    }
}

