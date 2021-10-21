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
                                    AND LL.TargetDate = '"+ ProductionDate + @"'                                
                                    AND LL.WorkCenterMasterId = '"+ SelectedLine["WorkCenterMasterId"] + @"'
                                LEFT JOIN LineLayoutDailyTarget LLP ON LLP.ProcessId = '" + processId + @"'                                
                                    AND LLP.ProductionOrderId = po.Id                                
                                    AND LLP.TargetDate = (
                                        SELECT TOP 1 X.TargetDate
                                        FROM LineLayoutDailyTarget x                                
                                        WHERE x.ProductionOrderId = po.id                                
                                            AND x.ProcessId = '" + processId + @"'                                
                                            AND x.WorkCenterMasterId = '" + SelectedLine["WorkCenterMasterId"] + @"'                                
                                            AND x.TargetDate < LL.TargetDate                                
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

        public List<Dictionary<string, object>> GetDesign(string BulletinId)
        {

            try
            {
                string sql = @"select * from LineLayoutDailyTarget where ProductionBulletinTemplateMasterId = '" + BulletinId + "' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
