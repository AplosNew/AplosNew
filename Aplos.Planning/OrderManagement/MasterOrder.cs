using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.OrderManagements;
using Library.Service.Enums;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Planning.OrderManagement
{

    public class MasterOrder
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        #region Constructor
        public MasterOrder()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }
        #endregion Constructor

        public void GenerateLogForTnA(string Id, Library.Service.Enums.TaskAppliedOnEnum AppliedOn)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"INSERT INTO TnALog(MasterOrderId,AddedBy,AddedDate,AddedFromIP)";
                switch (AppliedOn)
                {
                    case Library.Service.Enums.TaskAppliedOnEnum.MasterOrder:
                        sql += @"SELECT '" + Id + "',	'" + identity.Name + "',GETDATE(),'" + identity.IPAddress + "'";
                        ConManager = new ConnectionManager.clsConnectionManager();
                        ConManager.BeginTransaction();
                        ConManager.executeQuery(sql);
                        ConManager.CommitTransaction();
                        break;
                    case Library.Service.Enums.TaskAppliedOnEnum.Style:
                        sql += @"SELECT distinct moi.MasterOrderId,	'" + identity.Name + "',GETDATE(),'" + identity.IPAddress + "'  FROM trn.MasterOrderItem AS moi WHERE moi.Id='" + Id + "'";

                        ConManager = new ConnectionManager.clsConnectionManager();
                        ConManager.BeginTransaction();
                        ConManager.executeQuery(sql);
                        ConManager.CommitTransaction();
                        break;
                    case Library.Service.Enums.TaskAppliedOnEnum.SalesOrder:
                        sql += @"SELECT distinct  moi.MasterOrderId,	'" + identity.Name + "',GETDATE(),'" + identity.IPAddress + "'  FROM trn.MasterOrderItem AS moi  INNER JOIN trn.salesorder so ON so.MasterOrderItemId=moi.Id WHERE SO.Id='" + Id + "'";
                        ConManager = new ConnectionManager.clsConnectionManager();
                        ConManager.BeginTransaction();
                        ConManager.executeQuery(sql);
                        ConManager.CommitTransaction();
                        break;
                    case Library.Service.Enums.TaskAppliedOnEnum.ProductionOrder:
                        sql += @"SELECT distinct  moi.MasterOrderId,	'" + identity.Name + "',GETDATE(),'" + identity.IPAddress + @"' FROM trn.MasterOrderItem AS moi 
                                INNER JOIN trn.salesorder so ON so.MasterOrderItemId=moi.Id
                                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                WHERE pod.ProductionOrderId='" + Id + "'";
                        ConManager = new ConnectionManager.clsConnectionManager();
                        ConManager.BeginTransaction();
                        ConManager.executeQuery(sql);
                        ConManager.CommitTransaction();
                        break;
                    default:
                        break;
                }



            }
            catch (Exception ex)
            {

            }




        }

        public void RunTNASchedule()
        {
            try
            {
                string WhereClause = " AND MO.Id in (SELECT DISTINCT MasterOrderId from TnALog) ";

                Library.Service.TaskScheduler.TaskScheduler schedulerService = new Library.Service.TaskScheduler.TaskScheduler(_sqlRepository);
                //master order tasks
                string sql = @"SELECT MO.* FROM trn.MasterOrder AS mo WHERE mo.OrderStatusId<>'Closed' AND ISNULL(mo.TaskTemplateMasterId,'')<>'' " + WhereClause;

                DataTable dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {
                    try
                    {

                        DataTable dt = schedulerService.GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.MasterOrder);
                        if (dt.Rows.Count > 0)
                            schedulerService.MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.MasterOrder);

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }



                //line item related tasks
                sql = @"SELECT MOI.* FROM trn.MasterOrder AS mo 
                                INNER JOIN hkp.OrderStatus AS os ON os.Id=mo.OrderStatusId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                WHERE os.Id<>'" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"' " + WhereClause + " AND ISNULL(mo.TaskTemplateMasterId,'')<>''";

                dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {
                    try
                    {
                        DataTable dt = schedulerService.GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.Style);
                        if (dt.Rows.Count > 0)
                            schedulerService.MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.Style);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }



                //Sales Order Related Tasks
                sql = @"SELECT SO.* FROM trn.MasterOrder AS mo 
                                INNER JOIN hkp.OrderStatus AS os ON os.Id=mo.OrderStatusId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                           WHERE os.Id<>'" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"' " + WhereClause + "  AND ISNULL(mo.TaskTemplateMasterId,'')<>''";

                dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {

                    try
                    {
                        DataTable dt = schedulerService.GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.SalesOrder);
                        if (dt.Rows.Count > 0)
                            schedulerService.MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.SalesOrder);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }



                //Production Order Related Tasks
                sql = @"SELECT DISTINCT  PO.* FROM trn.MasterOrder AS mo 
                                INNER JOIN hkp.OrderStatus AS os ON os.Id=mo.OrderStatusId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                INNER JOIN trn.SalesOrder AS so2 ON so2.Id=pod.SalesOrderId
                                INNER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                                INNER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                    WHERE ISNULL(mo.TaskTemplateMasterId,'')<>'' " + WhereClause + "  AND os.Id<>'" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"' AND ps.StandardName<>'CLOSED'";

                dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {
                    try
                    {

                        DataTable dt = schedulerService.GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.ProductionOrder);
                        if (dt.Rows.Count > 0)
                            schedulerService.MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.ProductionOrder);
                    }
                    catch (Exception ex)
                    {
                        throw ex;

                    }

                }
                ConManager = new ConnectionManager.clsConnectionManager();
                ConManager.BeginTransaction();
                ConManager.executeQuery("delete from tnalog");
                ConManager.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public void SaveItemDescription(MasterOrderItem data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {

                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;

                    string sql = "SELECT * FROM TRN.MasterOrderItem WHERE Id='" + data.Id + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {

                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["BuyerItemDescription"] = data.BuyerItemDescription;
                        dr["MainRawMaterialDescription"] = data.MainRawMaterialDescription;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.Name;

                        dr.EndEdit();
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void GetSOId(string masterItemId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT ISNULL((MAX(Id)),0) Id FROM [TRN].[SalesOrder] WHERE MasterOrderItemId='" + masterItemId + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
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

        public void CopySalesOrderByMOIData(string MasterId, string masterItemId, List<Dictionary<string, object>> SKU1List, List<Dictionary<string, object>> SKU2List)
        {
            DataSet dsToSalesOrder;
            DataSet dsToFirstCharacteristics;
            DataSet dsToSecondCharacteristics;
            DataSet dsToSOCostingConfirm;
            DataSet dsToThirdCharacteristics;
            try
            {


                DataSet dsSOId;
                GetSOId(MasterId, out dsSOId);
                string NewId = dsSOId.Tables[0].Rows[0]["Id"].ToString();
                string NewSoId = string.Empty;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SalesOrder] WHERE 1=2", out dsToSalesOrder, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[FirstCharacteristics] WHERE 1=2", out dsToFirstCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SecondCharacteristics] WHERE 1=2", out dsToSecondCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[ThirdCharacteristics] WHERE 1=2", out dsToThirdCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.SOCostingConfirmation WHERE 1=2", out dsToSOCostingConfirm, false, "1");

                DataTable dtFromMaster = _sqlRepository.GetDataTable("SELECT * FROM [TRN].[SalesOrder] WHERE MasterOrderItemId='" + masterItemId + "'");
                DataTable dtFromFirstCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.FirstCharacteristics Where SalesOrderId IN(Select Id from TRN.SalesOrder Where MasterOrderItemId='" + masterItemId + "')");
                DataTable dtFromSecondCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.SecondCharacteristics Where SalesOrderId IN(Select Id from TRN.SalesOrder Where MasterOrderItemId='" + masterItemId + "')");
                DataTable dtFromThirdCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.ThirdCharacteristics Where SalesOrderId IN(Select Id from TRN.SalesOrder Where MasterOrderItemId='" + masterItemId + "')");
                DataTable dtFromSOCostingConfirm = _sqlRepository.GetDataTable("SELECT * FROM dbo.SOCostingConfirmation Where SalesOrderId IN(Select Id from TRN.SalesOrder Where MasterOrderItemId='" + masterItemId + "')");

                int SCount = 0;
                for (int m = 0; m < dtFromMaster.Rows.Count; m++)
                {
                    SCount++;
                    DataRow drSalesOrder = dsToSalesOrder.Tables[0].NewRow();
                    CopyRow(dtFromMaster.Rows[m], ref drSalesOrder);
                    drSalesOrder["Id"] = MasterId + Convert.ToInt32(NewId) + SCount;
                    NewSoId = drSalesOrder["Id"].ToString();
                    drSalesOrder["MasterOrderItemId"] = MasterId;
                    drSalesOrder["OrderStatusId"] = DBNull.Value;
                    drSalesOrder["CheckByStatus"] = "To Be Check";
                    drSalesOrder["ApprovedStatus"] = DBNull.Value;
                    drSalesOrder["CheckByDate"] = DBNull.Value;
                    drSalesOrder["ApproveByDate"] = DBNull.Value;
                    dsToSalesOrder.Tables[0].Rows.Add(drSalesOrder);

                    dtFromFirstCharacteristics.DefaultView.RowFilter = "SalesOrderId='" + dtFromMaster.Rows[m]["Id"].ToString() + "'";
                    for (int i = 0; i < dtFromFirstCharacteristics.DefaultView.Count; i++)
                    {
                        DataRow drFirstCharacteristics = dsToFirstCharacteristics.Tables[0].NewRow();
                        CopyRow(dtFromFirstCharacteristics.DefaultView[i].Row, ref drFirstCharacteristics);
                        drFirstCharacteristics["Id"] = NewSoId + (i + 1);
                        drFirstCharacteristics["SalesOrderId"] = NewSoId;


                        foreach (var item in SKU1List)
                        {
                            if (drFirstCharacteristics["CharacteristicsValueId"].ToString() == item["CharacteristicsValueId"].ToString())
                            {
                                drFirstCharacteristics["CharacteristicsValueId"] = item["ToSKU1Id"].ToString();
                            }
                            // break;
                        }

                        dsToFirstCharacteristics.Tables[0].Rows.Add(drFirstCharacteristics);

                        dtFromSecondCharacteristics.DefaultView.RowFilter = "SalesOrderId='" + dtFromMaster.Rows[m]["Id"].ToString() + "' AND FirstCharacteristicsId='" + dtFromFirstCharacteristics.DefaultView[i]["Id"] + "'";
                        for (int K = 0; K < dtFromSecondCharacteristics.DefaultView.Count; K++)
                        {
                            DataRow drSecondCharacteristics = dsToSecondCharacteristics.Tables[0].NewRow();
                            CopyRow(dtFromSecondCharacteristics.DefaultView[K].Row, ref drSecondCharacteristics);
                            drSecondCharacteristics["Id"] = NewSoId + (i + 1) + (K + 1);
                            drSecondCharacteristics["SalesOrderId"] = NewSoId;
                            drSecondCharacteristics["FirstCharacteristicsId"] = NewSoId + (i + 1);

                            foreach (var item in SKU2List)
                            {
                                if (drSecondCharacteristics["CharacteristicsValueId"].ToString() == item["CharacteristicsValueId"].ToString())
                                {
                                    drSecondCharacteristics["CharacteristicsValueId"] = item["ToSKU2Id"].ToString();
                                }
                                // break;
                            }
                            dsToSecondCharacteristics.Tables[0].Rows.Add(drSecondCharacteristics);

                            dtFromThirdCharacteristics.DefaultView.RowFilter = "SalesOrderId='" + dtFromMaster.Rows[m]["Id"].ToString() + "' AND SecondCharacteristicsId='" + dtFromSecondCharacteristics.DefaultView[K]["Id"] + "'";
                            for (int j = 0; j < dtFromThirdCharacteristics.DefaultView.Count; j++)
                            {
                                DataRow drThirdCharacteristics = dsToThirdCharacteristics.Tables[0].NewRow();
                                CopyRow(dtFromThirdCharacteristics.DefaultView[j].Row, ref drThirdCharacteristics);
                                drThirdCharacteristics["Id"] = NewSoId + (i + 1) + (j + 1);
                                drThirdCharacteristics["SalesOrderId"] = NewSoId;
                                drThirdCharacteristics["SecondCharacteristicsId"] = NewSoId + (i + 1) + (K + 1);
                                dsToThirdCharacteristics.Tables[0].Rows.Add(drThirdCharacteristics);
                            }
                        }
                    }

                    dtFromSOCostingConfirm.DefaultView.RowFilter = "SalesOrderId='" + dtFromMaster.Rows[m]["Id"].ToString() + "'";
                    for (int l = 0; l < dtFromSOCostingConfirm.DefaultView.Count; l++)
                    {
                        DataRow drSOCostingConfirm = dsToSOCostingConfirm.Tables[0].NewRow();
                        CopyRow(dtFromSOCostingConfirm.DefaultView[l].Row, ref drSOCostingConfirm);
                        drSOCostingConfirm["Id"] = NewId + (l + 1);
                        drSOCostingConfirm["SalesOrderId"] = NewId;
                        dsToSOCostingConfirm.Tables[0].Rows.Add(drSOCostingConfirm);
                    }

                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsToSalesOrder, dsToFirstCharacteristics, dsToSecondCharacteristics, dsToThirdCharacteristics, dsToSOCostingConfirm);


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void GetSalesOrderQty(string masterItemId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT SUM(Qty)Qty FROM [TRN].[SalesOrder] WHERE MasterOrderItemId='" + masterItemId + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CopySalesOrderData(string MasterId, string masterItemId, decimal TotalMOIQty)
        {
            DataSet dsToSalesOrder;
            DataSet dsToFirstCharacteristics;
            DataSet dsToSecondCharacteristics;
            DataSet dsToSOCostingConfirm;
            DataSet dsToThirdCharacteristics;
            try
            {

                DataSet dsSOId, dsSOqty;
                GetSalesOrderQty(masterItemId, out dsSOqty);
                GetSplitSalesOrderId(masterItemId, out dsSOId);
                var count = Convert.ToInt32(dsSOId.Tables[0].Rows[0]["Id"].ToString());
                count++;
                string NewId = MakePK(masterItemId, count, 2);
                decimal SOQty = Convert.ToDecimal(dsSOqty.Tables[0].Rows[0]["Qty"].ToString());


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SalesOrder] WHERE 1=2", out dsToSalesOrder, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[FirstCharacteristics] WHERE 1=2", out dsToFirstCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SecondCharacteristics] WHERE 1=2", out dsToSecondCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[ThirdCharacteristics] WHERE 1=2", out dsToThirdCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.SOCostingConfirmation WHERE 1=2", out dsToSOCostingConfirm, false, "1");

                DataTable dtFromMaster = _sqlRepository.GetDataTable("SELECT * FROM [TRN].[SalesOrder] WHERE Id='" + MasterId + "'");
                DataTable dtFromFirstCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.FirstCharacteristics WHERE SalesOrderId='" + MasterId + "'");
                DataTable dtFromSecondCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.SecondCharacteristics WHERE SalesOrderId='" + MasterId + "'");
                DataTable dtFromThirdCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.ThirdCharacteristics WHERE SalesOrderId='" + MasterId + "'");
                DataTable dtFromSOCostingConfirm = _sqlRepository.GetDataTable("SELECT * FROM dbo.SOCostingConfirmation WHERE SalesOrderId='" + MasterId + "'");

                if (TotalMOIQty < SOQty + Convert.ToDecimal(dtFromMaster.Rows[0]["Qty"].ToString()))
                {
                    throw new Exception("SO Qty can't greater than Line Item Qty.");
                }

                DataRow drSalesOrder = dsToSalesOrder.Tables[0].NewRow();
                CopyRow(dtFromMaster.Rows[0], ref drSalesOrder);
                drSalesOrder["Id"] = NewId;
                drSalesOrder["ParentId"] = MasterId;
                drSalesOrder["OrderStatusId"] = DBNull.Value;
                drSalesOrder["CheckByStatus"] = "To Be Check";
                drSalesOrder["CheckByDate"] = DBNull.Value;
                drSalesOrder["ApproveByDate"] = DBNull.Value;
                drSalesOrder["ApprovedStatus"] = DBNull.Value;
                dsToSalesOrder.Tables[0].Rows.Add(drSalesOrder);

                for (int i = 0; i < dtFromFirstCharacteristics.Rows.Count; i++)
                {
                    DataRow drFirstCharacteristics = dsToFirstCharacteristics.Tables[0].NewRow();
                    CopyRow(dtFromFirstCharacteristics.Rows[i], ref drFirstCharacteristics);
                    drFirstCharacteristics["Id"] = NewId + (i + 1);
                    drFirstCharacteristics["SalesOrderId"] = NewId;
                    dsToFirstCharacteristics.Tables[0].Rows.Add(drFirstCharacteristics);

                    dtFromSecondCharacteristics.DefaultView.RowFilter = "SalesOrderId='" + MasterId + "' AND FirstCharacteristicsId='" + dtFromFirstCharacteristics.Rows[i]["Id"] + "'";
                    for (int K = 0; K < dtFromSecondCharacteristics.DefaultView.Count; K++)
                    {
                        DataRow drSecondCharacteristics = dsToSecondCharacteristics.Tables[0].NewRow();
                        CopyRow(dtFromSecondCharacteristics.DefaultView[K].Row, ref drSecondCharacteristics);
                        drSecondCharacteristics["Id"] = NewId + (i + 1) + (K + 1);
                        drSecondCharacteristics["SalesOrderId"] = NewId;
                        drSecondCharacteristics["FirstCharacteristicsId"] = NewId + (i + 1);
                        dsToSecondCharacteristics.Tables[0].Rows.Add(drSecondCharacteristics);

                        dtFromThirdCharacteristics.DefaultView.RowFilter = "SalesOrderId='" + MasterId + "' AND SecondCharacteristicsId='" + dtFromSecondCharacteristics.Rows[K]["Id"] + "'";
                        for (int j = 0; j < dtFromThirdCharacteristics.DefaultView.Count; j++)
                        {
                            DataRow drThirdCharacteristics = dsToThirdCharacteristics.Tables[0].NewRow();
                            CopyRow(dtFromThirdCharacteristics.DefaultView[j].Row, ref drThirdCharacteristics);
                            drThirdCharacteristics["Id"] = NewId + (i + 1) + (j + 1);
                            drThirdCharacteristics["SalesOrderId"] = NewId;
                            drThirdCharacteristics["SecondCharacteristicsId"] = NewId + (i + 1) + (K + 1);
                            dsToThirdCharacteristics.Tables[0].Rows.Add(drThirdCharacteristics);
                        }
                    }
                }

                dtFromSOCostingConfirm.DefaultView.RowFilter = "SalesOrderId='" + MasterId + "'";
                for (int l = 0; l < dtFromSOCostingConfirm.DefaultView.Count; l++)
                {
                    DataRow drSOCostingConfirm = dsToSOCostingConfirm.Tables[0].NewRow();
                    CopyRow(dtFromSOCostingConfirm.DefaultView[l].Row, ref drSOCostingConfirm);
                    drSOCostingConfirm["Id"] = NewId + (l + 1);
                    drSOCostingConfirm["SalesOrderId"] = NewId;
                    dsToSOCostingConfirm.Tables[0].Rows.Add(drSOCostingConfirm);
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsToSalesOrder, dsToFirstCharacteristics, dsToSecondCharacteristics, dsToThirdCharacteristics, dsToSOCostingConfirm);


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void GetSplitSalesOrderId(string masterItemId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[SalesOrder] WHERE MasterOrderItemId='" + masterItemId + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static string MakePK(string masterId, int currentId, int padLeft)
        {
            return masterId + currentId.ToString().PadLeft(padLeft, '0');
        }
        public void SplitSalesOrderData(string masterItemId, SalesOrderMaster salesOrderMaster, IdentityParameter para)
        {
            DataSet dsToSalesOrder, dsParentSalesOrder;
            DataSet dsToFirstCharacteristics;
            DataSet dsToSecondCharacteristics;
            DataSet dsToThirdCharacteristics;
            DataSet dsToSOCostingConfirm;
            try
            {

                DataSet dsSOId;
               GetSplitSalesOrderId(masterItemId, out dsSOId);
                var count = Convert.ToInt32(dsSOId.Tables[0].Rows[0]["Id"].ToString());              
                count++;
                string NewId = MakePK(masterItemId, count, 2);


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SalesOrder] WHERE 1=2", out dsToSalesOrder, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SalesOrder] WHERE Id='" + salesOrderMaster.ParentId + "'", out dsParentSalesOrder, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[FirstCharacteristics] WHERE 1=2", out dsToFirstCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SecondCharacteristics] WHERE 1=2", out dsToSecondCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[ThirdCharacteristics] WHERE 1=2", out dsToThirdCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.SOCostingConfirmation WHERE 1=2", out dsToSOCostingConfirm, false, "1");

                DataTable dtFromMaster = _sqlRepository.GetDataTable("SELECT * FROM [TRN].[SalesOrder] WHERE Id='" + salesOrderMaster.ParentId + "'");
                DataTable dtFromFirstCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.FirstCharacteristics WHERE SalesOrderId='" + salesOrderMaster.ParentId + "'");
                DataTable dtFromSecondCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.SecondCharacteristics WHERE SalesOrderId='" + salesOrderMaster.ParentId + "'");
                DataTable dtFromThirdCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.ThirdCharacteristics WHERE SalesOrderId='" + salesOrderMaster.ParentId + "'");
                DataTable dtFromSOCostingConfirm = _sqlRepository.GetDataTable("SELECT * FROM dbo.SOCostingConfirmation WHERE SalesOrderId='" + salesOrderMaster.ParentId + "'");

                DataView dv = new DataView(dsParentSalesOrder.Tables[0]);
                dv.RowFilter = "Id='" + salesOrderMaster.ParentId + "'";

                if (dv.Count > 0)
                {
                    DataRow drmo = dv[0].Row;

                    drmo.BeginEdit();

                    drmo["Qty"] = Convert.ToDecimal(drmo["Qty"].ToString()) - salesOrderMaster.Qty;
                    drmo["UpdatedBy"] = para.UpdatedBy;
                    drmo["UpdatedDate"] = para.UpdatedDate;
                    drmo["UpdatedFromIP"] = para.UpdatedFromIP;

                    drmo.EndEdit();
                }


                DataRow drSalesOrder = dsToSalesOrder.Tables[0].NewRow();
                CopyRow(dtFromMaster.Rows[0], ref drSalesOrder);
                drSalesOrder["Id"] = NewId;
                drSalesOrder["ParentId"] = salesOrderMaster.ParentId;
                drSalesOrder["Qty"] = salesOrderMaster.Qty;
                dsToSalesOrder.Tables[0].Rows.Add(drSalesOrder);

                for (int i = 0; i < dtFromFirstCharacteristics.Rows.Count; i++)
                {
                    DataRow drFirstCharacteristics = dsToFirstCharacteristics.Tables[0].NewRow();
                    CopyRow(dtFromFirstCharacteristics.Rows[i], ref drFirstCharacteristics);
                    drFirstCharacteristics["Id"] = NewId + (i + 1);
                    drFirstCharacteristics["SalesOrderId"] = NewId;
                    drFirstCharacteristics["Qty"] = 0;
                    dsToFirstCharacteristics.Tables[0].Rows.Add(drFirstCharacteristics);

                    dtFromSecondCharacteristics.DefaultView.RowFilter = "SalesOrderId='" + salesOrderMaster.ParentId + "' AND FirstCharacteristicsId='" + dtFromFirstCharacteristics.Rows[i]["Id"] + "'";
                    for (int K = 0; K < dtFromSecondCharacteristics.DefaultView.Count; K++)
                    {
                        DataRow drSecondCharacteristics = dsToSecondCharacteristics.Tables[0].NewRow();
                        CopyRow(dtFromSecondCharacteristics.DefaultView[K].Row, ref drSecondCharacteristics);
                        drSecondCharacteristics["Id"] = NewId + (i + 1) + (K + 1);
                        drSecondCharacteristics["SalesOrderId"] = NewId;
                        drSecondCharacteristics["Qty"] = 0;
                        drSecondCharacteristics["FirstCharacteristicsId"] = NewId + (i + 1);
                        dsToSecondCharacteristics.Tables[0].Rows.Add(drSecondCharacteristics);

                        dtFromThirdCharacteristics.DefaultView.RowFilter = "SalesOrderId='" + salesOrderMaster.ParentId + "' AND SecondCharacteristicsId='" + dtFromSecondCharacteristics.Rows[K]["Id"] + "'";
                        for (int j = 0; j < dtFromThirdCharacteristics.DefaultView.Count; j++)
                        {
                            DataRow drThirdCharacteristics = dsToThirdCharacteristics.Tables[0].NewRow();
                            CopyRow(dtFromThirdCharacteristics.DefaultView[j].Row, ref drThirdCharacteristics);
                            drThirdCharacteristics["Id"] = NewId + (i + 1) + (j + 1);
                            drThirdCharacteristics["SalesOrderId"] = NewId;
                            drThirdCharacteristics["Qty"] = 0;
                            drThirdCharacteristics["SecondCharacteristicsId"] = NewId + (i + 1) + (K + 1);
                            dsToThirdCharacteristics.Tables[0].Rows.Add(drThirdCharacteristics);
                        }
                    }
                }
                dtFromSOCostingConfirm.DefaultView.RowFilter = "SalesOrderId='" + salesOrderMaster.ParentId + "'";
                for (int l = 0; l < dtFromSOCostingConfirm.DefaultView.Count; l++)
                {
                    DataRow drSOCostingConfirm = dsToSOCostingConfirm.Tables[0].NewRow();
                    CopyRow(dtFromSOCostingConfirm.DefaultView[l].Row, ref drSOCostingConfirm);
                    drSOCostingConfirm["Id"] = NewId + (l + 1);
                    drSOCostingConfirm["SalesOrderId"] = NewId;
                    dsToSOCostingConfirm.Tables[0].Rows.Add(drSOCostingConfirm);
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsParentSalesOrder,dsToSalesOrder, dsToFirstCharacteristics, dsToSecondCharacteristics, dsToThirdCharacteristics, dsToSOCostingConfirm);


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private string GetContractFundPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ContractFund", out sID);
            return sID;
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

        public void SavePackingDetailData(Dictionary<string, object> data,string MasterOrderId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM trn.MasterOrder WHERE Id='" + MasterOrderId + "'", out dsMaster, false, "1");

                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[PackingDetail] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PackingDetaial", out _Id);

                    data["Id"] = "PD" + _Id;
                    data["MasterOrderId"] = MasterOrderId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IEnumerable<object> GetProductLibrary(string ArticleId)
        {
            try
            {
                string sql = @"Select PL.Id,PL.Code,PL.ShortName,PL.StandardName, UserName=CASE WHEN PL.RecipeOrProductionGroup = 'Recipe' THEN RGM.UserName+' ('+PL.RecipeOrProductionGroup+')' ELSE PL.ProductionGroup+' ('+PL.RecipeOrProductionGroup+')' END
FROM dbo.ProductLibrary PL
LEFT JOIN[TRN].[RecipeGlobalMaster] RGM ON RGM.Id = PL.RecipeId WHERE PL.Active =1 AND PL.ArticleId='"+ ArticleId + "'";
                return _sqlRepository.GetDataCollection(sql);
            } 
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetCostingSOFormulaData(string masterOrderItemId)
        {
            try
            {
                string sql = @"SELECT A.Id,A.MasterOrderItemId,OL.Id OrderLineCostingItemId,OL.SOItemName,OL.UserName,OL.Formula,OL.FormulaId,OL.CostingType,CC.UserName CostingComponent,
                            Value=ISNULL(CASE WHEN A.Value IS NOT NULL THEN A.Value ELSE (CASE WHEN OL.ValueinDecimal=1 THEN OL.DefaultValue ELSE OL.DefaultValue/100 END) END,0)
                            ,OL.EntryState,ValueIN = ISNULL(CASE WHEN OL.ValueinDecimal=1 THEN 'Decimal' ELSE 'Percentage' END,0)
                            FROM OrderLineCostingItem AS OL
                            LEFT JOIN HKP.CostingComponent CC ON CC.Id=OL.CostingComponentId
                            OUTER APPLY (SELECT * FROM dbo.MasterOrderItemCostingRate WHERE OrderLineCostingItemId=OL.Id AND ISNULL(MasterOrderItemId,'" + masterOrderItemId + @"')='"+ masterOrderItemId + @"') A
                            ORDER BY OL.Sequence";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetCostingSORateData(string SalesOrderId, string lineId)
        {
            try
            {
                string sql = @"SELECT A.Id,OL.Id OrderLineCostingItemId,OL.SOItemName,OL.UserName,OL.Formula,OL.FormulaId,OL.CostingType,CC.UserName CostingComponent,
                            ISNULL(LR.Value,0) ItemValue,SOValue=ISNULL(CASE WHEN A.SOValue IS NULL THEN LR.[Value] ELSE A.SOValue END,0)
							,ValueDiff=ISNULL(LR.Value-(CASE WHEN A.SOValue IS NULL THEN LR.[Value] ELSE A.SOValue END),0),A.SalesOrderId,A.Remark
                            FROM OrderLineCostingItem AS OL
                            LEFT JOIN HKP.CostingComponent CC ON CC.Id=OL.CostingComponentId
                            LEFT JOIN dbo.MasterOrderItemCostingRate LR ON LR.OrderLineCostingItemId=OL.Id  AND ISNULL(LR.MasterOrderItemId,'" + lineId + @"')='" + lineId + @"'
                            OUTER APPLY (SELECT * FROM dbo.SOCostingConfirmation WHERE OrderLineCostingItemId=OL.Id AND ISNULL(SalesOrderId,'" + SalesOrderId + @"')='"+ SalesOrderId + @"') A
                            ORDER BY OL.Sequence";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> GetItemRateData(string masterOrderItemId)
        {
            try
            {
                string sql = @"SELECT * FROM MasterOrderItemCostingRate WHERE MasterOrderItemId='"+ masterOrderItemId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void ReLoadFormulaWithValue(string strFormulaID, ref DataTable dtValue, out string lblFormulaValue/*, ref DataTable dtSlrHd*/)
        {
            DataSet dsLocal = null;
            DataView dvLocal = null;
            DataView dvSlrHd = null;

            string strTemp = "";

            try
            {
                dsLocal = new DataSet();

                string strFormulaIDTemp = strFormulaID.Trim();

                lblFormulaValue = "";

                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dt = new DataTable();
                dt.TableName = "IDLIST";
                dt.Columns.Add("ID");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["ID"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {
                        dvLocal = new DataView();
                        dvLocal.Table = dtValue;

                        dvLocal.RowFilter = "OrderLineCostingItemID = '" + strTemp.Trim() + "'";
                        if (dvLocal.Count > 0)
                        {
                                strTemp = dvLocal[0]["Amount"].ToString().Trim();
                        }
                    }

                    lblFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End 

        public IEnumerable<object> GetContractByMasterOrder(string masterId)
        {
            try
            {
                string sql = @"SELECT DISTINCT C.*, P.UserName AS CustomerName,PM.UserName MarketingCommisssion FROM dbo.[Contract] C
                            JOIN TRN.MasterOrderItem I ON I.ContractId=C.Id
                            JOIN TRN.MasterOrder M ON M.Id=I.MasterOrderId
                            JOIN [HKP].[Party] AS P ON C.CustomerId=P.Id 
                            LEFT JOIN [HKP].[Party] AS PM ON C.MarketingCommisssionId=PM.Id 
                            WHERE M.Id='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetContractPercentage(string masterOrderItemId)
        {
            try
            {
                string sql = @"Select ISNULL(CF.[Percentage],0) [Percentage] from dbo.[Contract] C
                    LEFT JOIN dbo.ContractFund CF ON CF.ContractId = C.Id AND FundUtilization = 'LessCommission'
                    Where C.Id = (Select ContractId from TRN.MasterOrderItem where Id = '" + masterOrderItemId + "')";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPaymentTermChangeable(string CompanyId, string PartyId)
        {
            try
            {
                string sql = @"Select ISNULL(IsPaymentTermChangeable,0)IsPaymentTermChangeable from [HKP].[CompanyParty] Where PartyId='" + PartyId + "' AND CompanyId='" + CompanyId + "' AND PartyType='Customer'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetCostingItemCbo()
        {
            try
            {
                var sql = @"Select CI.Id,CI.UserName from [HKP].[CostingItem] CI
                            LEFT JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
                            Where CostingSegment='" + CostingSegment.DirectMaterial + "' Order By CI.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetQBOQByMasterOrderItem(string itemId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT B.*,MM.UserName MaterialMaster,MMA.StandardName Article,U.Code,C.UserName CostingItem 
                            , EntityOrVendorName= CASE WHEN B.EntityIdWithinCompany<>'' THEN EWC.UserName 
					                        WHEN B.EntityIdWithinGroup<>'' THEN EWG.UserName
					                        WHEN B.VendorId<>'' THEN PRT.UserName
					                        ELSE PRT.UserName END
                                            ,PR.UserName Process
                            FROM [dbo].[QuickBOQ] B
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=B.MaterialMasterId
                            LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=B.ArticleId
                            LEFT JOIN SCS.UnitOfMeasurement U ON U.Id=B.UoMId
                            LEFT JOIN HKP.CostingItem C ON C.Id=B.CostingItemId
                            LEFT JOIN ORG.Entity AS EWC ON B.EntityIdWithinCompany=EWC.Id
                            LEFT JOIN ORG.Entity AS EWG ON B.EntityIdWithinGroup=EWG.Id
                            LEFT JOIN HKP.Party AS PRT ON B.VendorId=PRT.Id
                            LEFT JOIN HKP.Process AS PR ON B.ProcessId=PR.Id
                            WHERE B.MasterOrderItemId='" + itemId + "' Order By B.Sequence";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMasterOrderAmountAndQty(string masterId)
        {
            string sql = @"SELECT  SUM(SI.TotalQty) TotalQty, SUM(SO.Amount)Amount,SUM(SO.Qty) Qty
                    FROM [TRN].[MasterOrderItem] AS I
                    inner join [TRN].[MasterOrder] AS A ON A.Id=I.MasterOrderId
                    LEFT JOIN (
                    Select SUM(TotalQty) TotalQty,MasterOrderId,Id FROM [TRN].[MasterOrderItem] Group By MasterOrderId,Id
                    ) SI ON SI.Id=I.Id
                    LEFT JOIN (
                    SELECT SUM(S.Qty) Qty, SUM(S.Qty*S.Rate) Amount, MOI.Id
                    FROM TRN.SalesOrder S
                    LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=S.MasterOrderItemId
                    GROUP BY MOI.Id
                    ) SO ON SO.Id=I.Id
                    WHERE I.MasterOrderId='" + masterId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetPackingDetail(string masterOderId)
        {
            string sql = @"select PD.*,EI.EmployeeName ResponsiblePerson
                                from PackingDetail PD
                                left join EmployeeInformation EI on EI.SystemId=PD.ResponsiblePersonId
                                where PD.MasterOrderId= '" + masterOderId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetPackingDetailData()
        {
            try
            {
                var sql = @"SELECT * FROM [dbo].[PackingDetail]
                            Where CostingSegment='" + CostingSegment.DirectMaterial + "' Order By CI.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetItemMaterialSKUData(string materialMasterId, string sequence)
        {
            string sql = @" SELECT CV.Id AS [Value], CV.UserName AS [Text], CV.CharacteristicsId FROM [HKP].[Characteristics] C
                             LEFT JOIN hkp.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id
                             Where CV.MaterialMasterId='" + materialMasterId + @"' AND CV.CharacteristicsId 
							 IN (SELECT MMC.CharacteristicsId  FROM [MST].[MaterialMasterCharacteristics] MMC  Where MaterialMasterId='" + materialMasterId + @"' AND MMC.Sequence=" + sequence + @"
							 ) AND C.ValueAssignmentLevel='Specific' Order by CV.UserName";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetFromItemMaterialSKU1Data(string ItemId)
        {
            string sql = @"SELECT distinct FCH.CharacteristicsValueId, CHV.UserName AS CharacteristicsValueName	                        
                        FROM [TRN].[FirstCharacteristics] AS FCH
                        JOIN [HKP].[Characteristics] AS CH ON FCH.CharacteristicsId=CH.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV ON FCH.CharacteristicsValueId=CHV.Id
                        JOIN [TRN].[SalesOrder] AS SO ON FCH.SalesOrderId=SO.Id
                        JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                        WHERE MOI.Id='" + ItemId + @"' AND ISNULL(FCH.CharacteristicsValueId,'')<>'' ORDER BY CHV.UserName";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetFromItemMaterialSKU2Data(string ItemId)
        {
            string sql = @"SELECT distinct FCH.CharacteristicsValueId, CHV.UserName AS CharacteristicsValueName
                        FROM [TRN].[SecondCharacteristics] AS FCH
                        JOIN [HKP].[Characteristics] AS CH ON FCH.CharacteristicsId=CH.Id
						LEFT JOIN [HKP].[CharacteristicsValue] AS CHV ON FCH.CharacteristicsValueId=CHV.Id
                        JOIN [TRN].[SalesOrder] AS SO ON FCH.SalesOrderId=SO.Id
                        JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       WHERE MOI.Id='" + ItemId + "' AND ISNULL(FCH.CharacteristicsValueId,'')<>'' ORDER BY CHV.UserName";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetUoMCboByProductMaster()
        {
            string sql = @"Select P.Id ProductMasterId,BUoM.Id AS Value,BUoM.UserName AS Text,CAST(1 as bit) ByDefault from [MST].[ProductMaster] P
                            LEFT JOIN SCS.UnitOfMeasurement BUoM ON BUoM.Id=P.BaseUOMId
                            Where ISNULL(BUoM.Id,'')<>''
                            UNION ALL
                            Select AUom.ProductMasterId,BUoM.Id,BUoM.UserName,CAST(0 as bit) ByDefault from MST.ProductMasterAlternativeUoM AUoM 
                            LEFT JOIN SCS.UnitOfMeasurement BUoM ON BUoM.Id=AUom.AlternativeUOMId
                            Where ISNULL(BUoM.Id,'')<>''
                            Order by ProductMasterId";
            return _sqlRepository.GetDataCollection(sql);
        }
        public IEnumerable<object> GetOrderCostingMasterTemplateDataByArticle(string articleId)
        {
            string sql = @"Select OCT.* from dbo.OrderCostingMasterTemplate OCT
                            LEFT JOIN dbo.CostingMasterTemplate CMT ON CMT.Id=OCT.CostingMasterTemplateId
                            LEFT JOIN dbo.ProductLibrary PL ON PL.CostingMasterTemplateId=CMT.Id
                            Where PL.ArticleId='"+ articleId + @"'
                            UNION
                            Select OCT.* from dbo.OrderCostingMasterTemplate OCT
                            JOIN dbo.CostingMasterTemplate CMT ON CMT.Id=OCT.CostingMasterTemplateId
                            JOIN dbo.ProductLibrary PL ON PL.CostingMasterTemplateId=CMT.Id
                            JOIN TRN.MasterOrderItem MOI ON MOI.ArticleId=PL.ArticleId
                            Where MOI.ArticleId='" + articleId + @"'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetSOData(string lineItem)
        {
            try
            {
                string CmdText = @"SELECT DISTINCT mo.MasterOrderNo,so.MasterOrderItemId
	                                ,ISNULL(so.Id,'') SOId
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
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM  (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so 
									left join TRN.ProductionOrderDetail POD on POD.SalesOrderId=so.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
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
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
							   Where so.MasterOrderItemId ='" + lineItem + "' and ISNULL(mo.MasterOrderNo,'')<>'' AND so.Id NOT IN(Select SOId From  dbo.BOMSODetail)";

                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string CreateSODataReportSheet(string masterOrderId)
        {

            ExcelEngine excelEngine = new ExcelEngine();
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtSOList = GetSODataSql(masterOrderId);

            var filePath = "";

            if (dtSOList.Rows.Count == 0)
                throw new Exception("No data found");
            // throw new Exception("To date must be above or equal to From Date.");

            worksheet.Name = "SO Data Report";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet[ROW, COL].Text = "Packing Detail Id";
            int colPackingDetailId = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "SO No.";
            int colSONo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Item Id";
            int colItemId = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;


            worksheet[ROW, COL].Text = "Master Order No.";
            int colMasterOrderNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Color";
            int colColor = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Size";
            int colSize = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Qty";
            int colQty = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Customer Ref No.";
            int colCustomerRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Own Ref No.";
            int colOwnRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Product Code";
            int colProductCode = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Customer";
            int colCustomer = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Material Description";
            int colMaterialDescription = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Article";
            int colArticle = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Remarks";
            int colRemarks = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
          
            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
           
            int StartRow = ROW;
            List<string> list = new List<string>();
            for (int i = 0; i < dtSOList.Rows.Count; i++)
            {
                ROW++;
               
                    worksheet[ROW, colPackingDetailId].Text = dtSOList.Rows[i]["PackingDetailId"].ToString();
                    worksheet[ROW, colSONo].Text = dtSOList.Rows[i]["SONo"].ToString();
                    worksheet[ROW, colItemId].Text = dtSOList.Rows[i]["MasterOrderItemId"].ToString();
                    worksheet[ROW, colMasterOrderNo].Text = dtSOList.Rows[i]["MasterOrderNo"].ToString();
                    worksheet[ROW, colColor].Text = (dtSOList.Rows[i]["Color"].ToString());
                    worksheet[ROW, colSize].Text = dtSOList.Rows[i]["Size"].ToString();

                    worksheet[ROW, colQty].Number = clsStaticInfo.dbl(dtSOList.Rows[i]["Qty"].ToString());
                    worksheet[ROW, colQty].NumberFormat = clsStaticInfo.NumberFormat(2);

                    worksheet[ROW, colCustomerRefNo].Text = dtSOList.Rows[i]["CustomerRefNo"].ToString();
                    worksheet[ROW, colOwnRefNo].Text = dtSOList.Rows[i]["OwnRefNo"].ToString();
                    worksheet[ROW, colProductCode].Text = dtSOList.Rows[i]["ProductCode"].ToString();
                    worksheet[ROW, colCustomer].Text = dtSOList.Rows[i]["Customer"].ToString();
                    worksheet[ROW, colMaterialDescription].Text = dtSOList.Rows[i]["MaterialDescription"].ToString();
                    worksheet[ROW, colArticle].Text = dtSOList.Rows[i]["Article"].ToString();
                    worksheet[ROW, colRemarks].Text = dtSOList.Rows[i]["Remark"].ToString();

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            }

            worksheet.UsedRange.WrapText = true;
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            //worksheet.FirstVisibleColumn = 1;
            //worksheet.FirstVisibleRow = 17;

            #endregion Freeze Panes

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "SO Data Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            //worksheet.Range[6, 1, 7, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[6, 1, 7, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

            var SheetName = "SODataReport";
            workbook.Version = ExcelVersion.Excel97to2003;
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
            workbook.SaveAs(filePath);
            workbook.Close();
            excelEngine.Dispose();
            return filePath;
        }
        public DataTable GetSODataSql(string masterOrderId)
        {
            var sql = @"select PD.Id PackingDetailId,PSO.SOId SONo,MOI.Id MasterOrderItemId,MO.Id MasterOrderNo
								,CV1.UserName Color,CV2.UserName Size,SKUD.Quantity Qty,PD.CustomerRefNo,PD.OwnRefNo
								,PL.Code ProductCode,P.UserName Customer,MM.UserName MaterialDescription
								,MMA.StandardName Article,MOI.Remark
								from dbo.SKUDetail SKUD --PackingDetail PD
								left join PackingTypeChild PTC on PTC.Id=SKUD.PackingTypeChildId
								left join PackingDetail PD on PD.Id=PTC.PackingDetailId
								left join [hkp].[CharacteristicsValue] CV1 on CV1.Id=SKUD.FGFirstCharacteristicsValueId								
								left join [hkp].[CharacteristicsValue] CV2 on CV2.Id=SKUD.FGSecondCharacteristicsValueId
								left join PackingSODetail PSO on PSO.PackingDetailId=PD.Id
								left join TRN.MasterOrder MO on MO.Id=PD.MasterOrderId
								left join HKP.Party P on P.Id=MO.PartyId
								left join TRN.MasterOrderItem MOI on MOI.Id=PD.MasterOrderItemId
								left join MST.MaterialMaster MM on MM.Id=MOI.MaterialMasterId
								left join MST.MaterialMasterArticle MMA on MMA.MaterialMasterId=MM.Id
								left join ProductLibrary PL on PL.Id=MOI.ProductLibraryId
                                where PD.MasterOrderId='" + masterOrderId +@"'";
            return _sqlRepository.GetDataTable(sql);
        }

        public string CreateSODataDetailReportSheet(string masterOrderId)
        {

            ExcelEngine excelEngine = new ExcelEngine();
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtSOList = GetSODataDetailSql(masterOrderId);

            var filePath = "";

            if (dtSOList.Rows.Count == 0)
                throw new Exception("No data found");
            // throw new Exception("To date must be above or equal to From Date.");

            worksheet.Name = "SO Data Detail Report";

            int COL = 1; int ROW = 5;
            int startCol = COL;


            worksheet[ROW, COL].Text = "Master Order No.";
            int colMasterOrderNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Item Id";
            int colItemId = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "User Name";
            int colUserName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Own Ref No.";
            int colOwnRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Customer Ref No.";
            int colCustomerRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Packing Level";
            int colPackingLevel = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Responsible Person";
            int colResponsiblePerson = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Packing Detail Remarks";
            int colPackingDetailRemarks = COL;
            worksheet[ROW, COL].ColumnWidth = 18;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "SO No.";
            int colSONo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "SO Remarks";
            int colSORemarks = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Packing Code";
            int colPackingCode = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Packing Type";
            int colPackingType = COL;
            worksheet[ROW, COL].ColumnWidth = 22;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Packing Type Customer Ref No.";
            int colPackingTypeCustomerRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Packing Type Remarks";
            int colPackingTypeRemarks = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Color";
            int colColor = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Size";
            int colSize = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Qty";
            int colQty = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "To Plan Qty";
            int colToPlanQty = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Plan";
            int colPlan = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
          
            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

            int StartRow = ROW;
            List<string> list = new List<string>();
            for (int i = 0; i < dtSOList.Rows.Count; i++)
            {
                ROW++;

                worksheet[ROW, colMasterOrderNo].Text = dtSOList.Rows[i]["MasterOrderNo"].ToString();
                worksheet[ROW, colItemId].Text = dtSOList.Rows[i]["MasterOrderItemId"].ToString();
                worksheet[ROW, colUserName].Text = dtSOList.Rows[i]["UserName"].ToString();
                worksheet[ROW, colOwnRefNo].Text = dtSOList.Rows[i]["OwnRefNo"].ToString();
                worksheet[ROW, colCustomerRefNo].Text = dtSOList.Rows[i]["CustomerRefNo"].ToString();
                worksheet[ROW, colPackingLevel].Text = dtSOList.Rows[i]["PackingLevel"].ToString();
                worksheet[ROW, colResponsiblePerson].Text = dtSOList.Rows[i]["ResponsiblePerson"].ToString();
                worksheet[ROW, colPackingDetailRemarks].Text = dtSOList.Rows[i]["PackingDetailRemarks"].ToString();
                worksheet[ROW, colSONo].Text = dtSOList.Rows[i]["SOId"].ToString();
                worksheet[ROW, colSORemarks].Text = dtSOList.Rows[i]["SORemarks"].ToString();
                worksheet[ROW, colPackingCode].Text = dtSOList.Rows[i]["PackingCode"].ToString();
                worksheet[ROW, colPackingType].Text = dtSOList.Rows[i]["PackingType"].ToString();
                worksheet[ROW, colPackingTypeCustomerRefNo].Text = dtSOList.Rows[i]["PackingTypeCustomerRefCode"].ToString();
                worksheet[ROW, colPackingTypeRemarks].Text = dtSOList.Rows[i]["PackingTypeRemarks"].ToString();

                worksheet[ROW, colColor].Text = (dtSOList.Rows[i]["Color"].ToString());
                worksheet[ROW, colSize].Text = dtSOList.Rows[i]["Size"].ToString();

                worksheet[ROW, colQty].Number = clsStaticInfo.dbl(dtSOList.Rows[i]["Quantity"].ToString());
                worksheet[ROW, colQty].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colToPlanQty].Number = clsStaticInfo.dbl(dtSOList.Rows[i]["ToPlanQuantity"].ToString());
                worksheet[ROW, colToPlanQty].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colPlan].Number = clsStaticInfo.dbl(dtSOList.Rows[i]["Plan"].ToString());
                worksheet[ROW, colPlan].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            }

            worksheet.UsedRange.WrapText = true;
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            //worksheet.FirstVisibleColumn = 1;
            //worksheet.FirstVisibleRow = 17;

            #endregion Freeze Panes

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "SO Data Detail Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            //worksheet.Range[6, 1, 7, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[6, 1, 7, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

            var SheetName = "SODataDetailReport";
            workbook.Version = ExcelVersion.Excel97to2003;
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
            workbook.SaveAs(filePath);
            workbook.Close();
            excelEngine.Dispose();
            return filePath;
        }
        public DataTable GetSODataDetailSql(string masterOrderId)
        {
            var sql = @"select MO.Id MasterOrderNo,MOI.Id MasterOrderItemId,PD.UserName,PD.OwnRefNo,PD.CustomerRefNo,PD.PackingLevel,EI.EmployeeName ResponsiblePerson
								,PD.Remarks PackingDetailRemarks,PSO.SOId,PSO.Remarks SORemarks,PTC.PackingCode,PT.StandardName PackingType
								,PTC.CustomerRefCode PackingTypeCustomerRefCode,PTC.Remarks PackingTypeRemarks,CV1.UserName Color,CV2.UserName Size,SKUD.Quantity
								,SKUD.ToPlanQuantity,SKUD.[Plan]
								from PackingDetail PD
								left join TRN.MasterOrderItem MOI on MOI.Id=PD.MasterOrderItemId
								left join TRN.MasterOrder MO on MO.Id=PD.MasterOrderId
								left join EmployeeInformation EI on EI.SystemId=PD.ResponsiblePersonId
								left join PackingSODetail PSO on PSO.PackingDetailId=PD.Id
								left join PackingTypeChild PTC on PTC.PackingDetailId=PD.Id
								left join HKP.PackingType PT on PT.Id=PTC.PackingTypeId
								left join SKUDetail SKUD on SKUD.PackingTypeChildId=PTC.Id
								left join [hkp].[CharacteristicsValue] CV1 on CV1.Id=SKUD.FGFirstCharacteristicsValueId								
								left join [hkp].[CharacteristicsValue] CV2 on CV2.Id=SKUD.FGSecondCharacteristicsValueId
                                where PD.MasterOrderId='" + masterOrderId + @"'";
            return _sqlRepository.GetDataTable(sql);
        }

        public IEnumerable<object> GetProductionRef(string pg)
        {
            string sql = @"Select top(1)ProductionGrouping from TRN.MasterOrderItem Where OwnReferenceNo='" + pg + "' Order BY AddedDate DESC";
            return _sqlRepository.GetDataCollection(sql);
        }

       
        public IEnumerable<object> GetSavedPackingTypeChild(string PTId)
        {
            string sql = @"select Sku.Id,Sku.FGFirstCharacteristicsValueId,CV1.UserName Color,Sku.FGSecondCharacteristicsValueId
													,CV2.UserName Size,Sku.Quantity,Sku.[Plan],Sku.ToPlanQuantity
													from  [dbo].[SKUDetail] Sku
													                        
													left join [hkp].[CharacteristicsValue] CV1 on CV1.Id=Sku.FGFirstCharacteristicsValueId								
													left join [hkp].[CharacteristicsValue] CV2 on CV2.Id=Sku.FGSecondCharacteristicsValueId
                                Where Sku.PackingTypeChildId = '" + PTId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetSavedSKUDetail(string PackingTypeId)
        {
            string sql = @"select Sku.Id,PT.Id PackingType,PT.UserName PackingType,Sku.FGFirstCharacteristicsValueId,CV1.UserName FirstCharacteristics,Sku.FGSecondCharacteristicsId
													,CV2.UserName SecondCharacteristics,Sku.Quantity,Sku.[Plan],Sku.ToPlanQuantity
													from  [dbo].[SKUDetail] Sku
													left join [dbo].[PackingTypeChild] PTC on PTC.Id=Sku.PackingTypeChildId  
													left join [hkp].[PackingType] PT on PT.Id=PTC.PackingTypeId                                
													left join [hkp].[CharacteristicsValue] CV1 on CV1.Id=Sku.FGFirstCharacteristicsValueId								
													left join [hkp].[CharacteristicsValue] CV2 on CV2.Id=Sku.FGSecondCharacteristicsId
                                Where Sku.PackingTypeChildId = '" + PackingTypeId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetSKU2List(string SOId)
        {
            string sql = @"select distinct CV.Id as Value,CV.UserName as Text
                                from trn.SecondCharacteristics sku2
                                left join HKP.CharacteristicsValue CV on CV.Id=sku2.CharacteristicsValueId
                                where sku2.SalesOrderId " + SOId + "";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetSKU1List(string SOId)
        {
            string sql = @"select distinct CV.Id as Value,CV.UserName as Text
                                from trn.FirstCharacteristics sku1
                                left join HKP.CharacteristicsValue CV on CV.Id=sku1.CharacteristicsValueId
                                where sku1.SalesOrderId " + SOId + "";
            return _sqlRepository.GetDataCollection(sql);
        }
        public IEnumerable<object> GetSavedPackingType(string PackingDetailId)
        {
            string sql = @"select PTC.*,PT.UserName PackingType
                                --,sku.Id SKUDetailId,sku.FGFirstCharacteristicsValueId
								--,CV1.UserName FirstCharacteristics,Sku.FGSecondCharacteristicsValueId
								--,CV2.UserName SecondCharacteristics,Sku.Quantity,Sku.[Plan],sku.ToPlanQuantity

                                from  [dbo].[PackingTypeChild] PTC
                                left join [hkp].[PackingType] PT on PT.Id=PTC.PackingTypeId
								--left join SKUDetail sku on sku.PackingTypeChildId=PTC.Id
								--left join [hkp].[CharacteristicsValue] CV1 on CV1.Id=Sku.FGFirstCharacteristicsValueId								
								--left join [hkp].[CharacteristicsValue] CV2 on CV2.Id=Sku.FGSecondCharacteristicsValueId
                                Where PackingDetailId='" + PackingDetailId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetSODataList(string masterid)
        {
            string sql = @"Select * from [dbo].[BOMSODetail] Where BOMDetailChild1Id IN(Select ID from BOMDetailChild1 Where BOMDetailMasterId='" + masterid + "')";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetSavedSOData(string PackingDetailId)
        {
            string sql = @"select * from  [dbo].[PackingSODetail] Where PackingDetailId='" + PackingDetailId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetAutoSequence(string itemId)
        {
            string sql = @"SELECT (ISNULL((MAX(ISNULL(Sequence,0))),0)+1) Sequence FROM [dbo].[QuickBOQ] Where MasterOrderItemId='" + itemId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public bool CheckCombination(Dictionary<string, object> data)
        {
            try
            {

                var _sql = @"SELECT * FROM [dbo].[QuickBOQ] where id<>'" + data["Id"] + "' and ArticleId='" + data["ArticleId"] + "' AND MasterOrderItemId='" + data["MasterOrderItemId"] + "' AND MaterialMasterId='" + data["MaterialMasterId"] + "' AND CostingItemId='" + data["CostingItemId"] + "'";
                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetOrderMaster(string MasterOrderId)
        {
           return _sqlRepository.GetDataTable(@"select mo.Id, mo.type, b.UserName as Buyer, p.UserName as Customer
                ,  mo.OrderYear as Year, mo.TotalQty as TotalQuantity
                    , uom.UserName as UnitOfMeasurement, mo.NoOfLineItem, mo.OrderWastagePercentage
                    , mo.ExtraOrderPercentage, mo.BuyerReferenceNo, mo.OwnReferenceNo, BDept.UserName as BuyerDepartment
                    , BDev.UserName as BuyerDevision, MoCur.Code MasterOrderCurrency from trn.MasterOrder MO 
                    left join scs.Currency MoCur on MoCur.id = mo.CurrencyId 
                    left join scs.UnitOfMeasurement UOM on uom.id = mo.TotalQtyUOMId 
                    left join hkp.buyer B on b.id = mo.buyerid 
                    left join hkp.party p on p.id = mo.partyid 
                    left join hkp.BuyerDepartment BDept on BDept.id = mo.buyerDepartmentid 
                    left join HKP.buyerdivision BDev on BDev.id = mo.BuyerDivisionId where mo.Id='" + MasterOrderId + "'");
        }

        public DataTable GetMasterOrderItem(string MasterOrderId)
        {
            return _sqlRepository.GetDataTable(@"select moi.id as MasterOrderItemNo,moi.BuyerReferenceNo
                 ,moi.OwnReferenceNo,moi.TotalQty as TotalMOIQuantity, moi.MasterOrderId
                 ,moi.OrderWastagePercentage, moi.ExtraOrderPercentage ,mm.UserName as Material ,mma.StandardName as Article, moi.Type
                 from trn.MasterOrderItem MOI
                 left join TRN.MasterOrder mo  on mo.id=moi.MasterOrderId
                 left join MST.MaterialMaster MM on mm.id = moi.MaterialMasterId
                 left join MST.MaterialMasterArticle mma on mma.id= moi.ArticleId
                 left join scs.TestingStandard ts on ts.id=moi.TestingStandardId

                 where moi.MasterOrderId='" + MasterOrderId + "'");
        }

        public DataTable GetSalesOrderItem(string MasterOrderId)
        {
            return _sqlRepository.GetDataTable(@"SELECT K.MasterOrderItemId, K.SalesOrderNo, K.PONumber, K.PODate, K.OrderStatus,
                                           K.Destination, K.UpCharge, K.MainRawMaterialInhouseDate,
                                           K.[Description], K.SOType, K.OrderCategory, K.DeliveryDate, K.ShipmentMode,
                                           K.Rate, K.Discount, K.CM, K.LSD, K.OtherRawMaterialInhouseDate, K.Reason,
                                           K.CommitmentDate, K.FirstCharacteristics, K.FirstCharacteristicsValue,
                                           K.SecondCharacteristics, K.SecondCharacteristicsValue,
                                           K.ThirdCharacteristics, K.ThirdCharacteristicsValue, sum(K.Qty) AS Qty, K.Quantity
                                      from (select so.MasterOrderItemId, so.id as SalesOrderNo
                ,cpo.PONumber , Replace(CONVERT(VARCHAR(11),  CPO.PODate, 106), ' ', '-') AS PODate
                    ,os.UserName as OrderStatus --,d.UserName as Destination
                   ,Destination= CASE WHEN so.DestinationDescription IS NULL THEN d.UserName  ELSE d.UserName+' ('+ISNULL(so.DestinationDescription,'')+')'  END
                ,so.Qty as Quantity, so.UpCharge, so.MainRawMaterialInhouseDate, so.Description
                ,so.SOType, oc.username as OrderCategory
                ,so.DeliveryDate, sm.UserName as ShipmentMode
                ,so.Rate, so.Discount,so.CM,so.LSD, so.OtherRawMaterialInhouseDate , so.Reason , so.CommitmentDate

                ,c1.username as FirstCharacteristics, isnull(fcs.ValueFreeText,CV1.UserName) as FirstCharacteristicsValue
                ,c2.username as SecondCharacteristics ,isnull(SCS.ValueFreeText, CV2.UserName) as SecondCharacteristicsValue
                ,c3.username as ThirdCharacteristics , isnull(ThirdCS.ValueFreeText,CV3.UserName) as ThirdCharacteristicsValue
                ,case when isnull(thirdCs.Id,'')<>'' THEN ThirdCs.Qty
                ELSE case when isnull(scs.id,'')<>'' THEN scs.Qty
                ELSE case when isnull(fcs.Id,'')<>'' THEN fcs.Qty
                ELSE 0 END END END AS Qty
                from trn.SalesOrder SO
                left join trn.masterorderitem moi on moi.id= so.masterorderitemid
                left join HKP.OrderCategory OC on oc.id = so.OrderCategoryId
                left join hkp.OrderStatus OS on os.id = so.OrderStatusId
                left join mst.shipMode SM on sm.id = so.shipmentModeId
                left join mst.Destination d on d.id =so.DestinationId
                left join trn.CustomerPO CPO on cpo.id =so.CustomerPOId

                left join TRN.FirstCharacteristics FCS on fcs.SalesOrderId = so.id
                left join hkp.Characteristics C1 on c1.id = fcs.CharacteristicsId
                left join HKP.CharacteristicsValue CV1 on cv1.id= fcs.CharacteristicsValueId

                left join TRN.SecondCharacteristics SCS on scs.SalesOrderId=so.id and scs.FirstCharacteristicsId=fcs.Id
                left join hkp.Characteristics C2 on c2.id = scs.CharacteristicsId
                left join HKP.CharacteristicsValue CV2 on cv2.id= scs.CharacteristicsValueId

                left join TRN.ThirdCharacteristics ThirdCS on ThirdCS.SalesOrderId=so.id and scs.id=ThirdCS.SecondCharacteristicsId
                left join hkp.Characteristics C3 on c3.id = ThirdCS.CharacteristicsId
                left join HKP.CharacteristicsValue CV3 on CV3.id= ThirdCS.CharacteristicsValueId

                 where moi.MasterOrderId='" + MasterOrderId + @"'

					 ) AS K
					 
                GROUP BY K.MasterOrderItemId, K.SalesOrderNo, K.PONumber, K.PODate, K.OrderStatus,
                       K.Destination, K.UpCharge, K.MainRawMaterialInhouseDate,
                       K.[Description], K.SOType, K.OrderCategory, K.DeliveryDate, K.ShipmentMode,
                       K.Rate, K.Discount, K.CM, K.LSD, K.OtherRawMaterialInhouseDate, K.Reason,
                       K.CommitmentDate, K.FirstCharacteristics, K.FirstCharacteristicsValue,
                       K.SecondCharacteristics, K.SecondCharacteristicsValue,
                       K.ThirdCharacteristics, K.ThirdCharacteristicsValue, K.Quantity");
        }

        public IEnumerable<object> GetCheckByList(string companyId, string column, string value, string empId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false)
                    strkey = column + " like '%" + value + "%'";
                string ca = @"select E.SystemId from dbo.AuthorizationConfig A 
                          Inner Join dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='SalesOrderCheckedBy' AND E.EmployeeStatus='Active' AND E.SystemId=" + empId + "";
                var dt = _sqlRepository.GetDataTable(ca);
                if (dt.Rows.Count == 0)
                {
                    throw new Exception("This employee is not authorize for checking.");
                }
                string sql = @"select * from (SELECT A.Id, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId, EN.UserName Entity,FORMAT(A.AddedDate,'dd-MMM-yyyy') AS CreationDate,a.AddedBy AS CreatedBy
                                    , A.OrderType, A.PartyId, P.Code CustomerCode, P.UserName AS CustomerName, A.BuyerId,B.UserName Buyer
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId,OC.UserName AS OrderCategory, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
									,MS.TotalAmount
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' AND PlantId=A.PlantId)
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage,A.BuyerDepartmentId
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.IsExtraOrderPercentage,PM.UserName ProductMaster,OS.UserName OrderStatus,A.AddedDate,A.AddedBy
                                       ,A.OwnReferenceNo,A.BuyerReferenceNo
									   ,[BuyerReferenceNoItem]=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     [OwnItem]=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									  ,A.PaymentTermId,A.PaymentTermDays,A.ExceptionalProcessId,A.ExceptionalSubProcessId
                                   
                                   ,ContractNo=STUFF((select distinct ','+CNT.ContractNo from dbo.Contract CNT
															INNER JOIN trn.SalesOrder XSO  ON XSO.ContractId=CNT.Id	  
															INNER JOIN trn.MasterOrderItem XMOI  ON XMOI.Id=XSO.MasterOrderItemId	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
									MasterLCNo=STUFF((select distinct ','+MLC.LCRef from dbo.Contract CNT
															INNER JOIN trn.SalesOrder XSO  ON XSO.ContractId=CNT.Id	  
															INNER JOIN trn.MasterOrderItem XMOI  ON XMOI.Id=XSO.MasterOrderItemId
															LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),CP.PaymentTermId DefaultPaymentTermId,RC.Process RemarksControl,URC.RemarkControlId
                            FROM [TRN].[MasterOrder] AS A
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=A.PartyId AND CP.PartyType='Customer'  AND CP.PlantId=A.PlantId
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN TRN.Commitment COM ON COM.Id=A.CommitmentId
							LEFT JOIN [MST].[ProductMaster] PM ON COM.ProductMasterId=PM.Id
                            LEFT JOIN HKP.OrderStatus OS ON OS.Id=A.OrderStatusId
                            LEFT JOIN hkp.OrderCategory AS oc ON oc.Id=a.OrderCategoryId
                            LEFT JOIN HKP.Buyer B ON B.Id=A.BuyerId
                            LEFT JOIN ORG.Entity EN ON EN.Id=A.EntityId
							LEFT JOIN(select moi.MasterOrderId,TotalAmount=SUM(SO.Qty*SO.Rate) 
									from TRN.MasterOrderItem moi
									LEFT JOIN TRN.SalesOrder so on so.MasterOrderItemId=moi.Id
									Group By moi.MasterOrderId) MS ON MS.MasterOrderId=A.Id
                            LEFT JOIN TRN.UserRemarksControl URC ON URC.MasterOrderId=A.Id
							LEFT JOIN [HKP].[RemarksControl] RC ON RC.Id=URC.RemarkControlId
                            WHERE A.CompanyId='" + companyId + @"' AND A.Id IN(Select distinct MOI.MasterOrderId from  TRN.SalesOrder SO
LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id = SO.MasterOrderItemId
LEFT JOIN TRN.MasterOrder MO ON MO.Id = MOI.MasterOrderId
Where SO.CheckByStatus = 'To Be Check')) AS TEMP WHERE " + strkey + " ORDER BY AddedDate Desc";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch ( Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> GetMasterItemForCheckList(string masterOrderId, string empId)
        {
            try
            {
                var sql = @"SELECT MOI.Id, MOI.MasterOrderId, MOI.InquiryItemId, MOI.SampleItemId, MOI.TestingStandardId
                           ,Status=STUFF((select distinct ','+case when CheckByStatus = 'Checked' then 'Checked' else 'Pending' end from trn.SalesOrder XSO 
							                                where XSO.MasterOrderItemId=MOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,CheckStatus=STUFF((select distinct ','+XSO.CheckByStatus from trn.SalesOrder XSO 
							                                where XSO.MasterOrderItemId=MOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						   ,ApproveStatus=STUFF((select distinct ','+XSO.ApprovedStatus from trn.SalesOrder XSO 
							                                where XSO.MasterOrderItemId=MOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
	                         , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	                         , MOI.ArticleId, ART.StandardName AS ArticleName
	                         , MOI.BuyerReferenceNo, MOI.OwnReferenceNo, MOI.TotalQty
	                         , MOI.OrderWastagePercentage, MOI.ExtraOrderPercentage, MOI.ProductionGrouping, MM.HSNCodeId
							 , ISNULL(HART.HasAttribute,CAST(0 AS BIT)) AS HasAttribute
                             , ISNULL((select sum(SO.Qty) from TRN.SalesOrder SO where So.MasterOrderItemId = MOI.Id),0) as SOQty
                             , ISNULL((select sum(so.Rate*so.Qty) from TRN.SalesOrder SO where So.MasterOrderItemId = MOI.Id),0) as TotalAmount
                             ,MOI.Type,MOI.IsRepeat, PM.UserName AS ProductMaster
                            --a ,MOI.ContractId,CNT.ContractNo,MLC.LCRef
							 ,MOI.BuyerItemDescription,MOI.MainRawMaterialDescription,MOI.PartyId,MOI.EntityIdWithinGroup,MOI.EntityIdWithinCompany,MOI.JobWorkType
                             , EntityOrVendorName= CASE WHEN MOI.EntityIdWithinCompany<>'' THEN EWCC.UserName +' - '+EWC.UserName 
					                        WHEN MOI.EntityIdWithinGroup<>'' THEN EWGC.UserName+' - '+EWG.UserName
					                        WHEN MOI.PartyId<>'' THEN PRT.UserName
					                        ELSE PRT.UserName END
                            ,enableJobOrOutSource=CASE WHEN MOI.[Type]='JobWork' OR MOI.[Type]='OutSource' THEN 'false' ELSE 'true' END
                            ,MOI.ProductLibraryId,MOI.FileName,MOI.Remark,MOI.OrderStatusId,MOI.UOMId
                            ,BOQNo=(Select COUNT(Id) from [dbo].[QuickBOQ] Where MasterOrderItemId=MOI.Id)
                            ,SONo=(Select COUNT(Id) from TRN.SalesOrder Where MasterOrderItemId=MOI.Id)
                            ,MOI.Consignment,MOI.OrderCostingMasterTemplateId,'' TempList,PM.Id ProductMasterId,CAST(1 as bit) ByDefault,PL.UserName ProductLibrary,OCT.UserName OrderCostingMasterTemplate,MOI.Rate,ISNULL(AA.ArticlePartyName,P.UserName) CustomerArticle
                        FROM TRN.MasterOrderItem AS MOI
                        JOIN MST.MaterialMaster AS MM ON MOI.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON MOI.ArticleId=ART.Id
						LEFT JOIN (SELECT AttributeSetLength=CASE WHEN COUNT(MaterialMasterId)>0 THEN COUNT(MaterialMasterId) ELSE 0 END
                                                , HasAttribute=CASE WHEN COUNT(MaterialMasterId)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MaterialMasterId
                                            FROM MST.MaterialMasterAttribute GROUP BY MaterialMasterId) AS HART ON HART.MaterialMasterId=MM.Id
                        LEFT JOIN dbo.ArticleAlias AA ON AA.ArticleId=ART.Id AND AA.MasterOrderItemId=MOI.Id
						LEFT JOIN HKP.Party P ON P.Id=AA.Partyid
                        LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId= MM.Id
						LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                        --LEFT JOIN dbo.Contract CNT ON CNT.Id=MOI.ContractId
						--LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId
						LEFT JOIN ORG.Entity AS EWC ON MOI.EntityIdWithinCompany=EWC.Id
						LEFT JOIN ORG.Company AS EWCC ON EWC.CompanyId=EWCC.Id
                        LEFT JOIN ORG.Entity AS EWG ON MOI.EntityIdWithinGroup=EWG.Id
						LEFT JOIN ORG.CompanyGroup AS EWGC ON EWG.CompanyGroupId=EWGC.Id
                        LEFT JOIN HKP.Party AS PRT ON MOI.PartyId=PRT.Id
                        LEFT JOIN dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId
                        LEFT JOIN dbo.OrderCostingMasterTemplate OCT ON OCT.Id=MOI.OrderCostingMasterTemplateId
                        WHERE MOI.MasterOrderId='" + masterOrderId + @"' AND MOI.Id IN(Select distinct SO.MasterOrderItemId from  TRN.SalesOrder SO
Where SO.CheckByStatus = 'To Be Check')";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetApproveList(string companyId, string column, string value, string empId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";


            string sql = @"select * from (SELECT A.Id, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId, EN.UserName Entity,FORMAT(A.AddedDate,'dd-MMM-yyyy') AS CreationDate,a.AddedBy AS CreatedBy
                                    , A.OrderType, A.PartyId, P.Code CustomerCode, P.UserName AS CustomerName, A.BuyerId,B.UserName Buyer
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId,OC.UserName AS OrderCategory, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
									,MS.TotalAmount
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' AND PlantId=A.PlantId)
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage,A.BuyerDepartmentId
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.IsExtraOrderPercentage,PM.UserName ProductMaster,OS.UserName OrderStatus,A.AddedDate,A.AddedBy
                                       ,A.OwnReferenceNo,A.BuyerReferenceNo
									   ,[BuyerReferenceNoItem]=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     [OwnItem]=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									  ,A.PaymentTermId,A.PaymentTermDays,A.ExceptionalProcessId,A.ExceptionalSubProcessId
                                   
                                   ,ContractNo=STUFF((select distinct ','+CNT.ContractNo from dbo.Contract CNT
															INNER JOIN trn.SalesOrder XSO  ON XSO.ContractId=CNT.Id	  
															INNER JOIN trn.MasterOrderItem XMOI  ON XMOI.Id=XSO.MasterOrderItemId	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
									MasterLCNo=STUFF((select distinct ','+MLC.LCRef from dbo.Contract CNT
															INNER JOIN trn.SalesOrder XSO  ON XSO.ContractId=CNT.Id	  
															INNER JOIN trn.MasterOrderItem XMOI  ON XMOI.Id=XSO.MasterOrderItemId
															LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),CP.PaymentTermId DefaultPaymentTermId,RC.Process RemarksControl,URC.RemarkControlId
                            FROM [TRN].[MasterOrder] AS A
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=A.PartyId AND CP.PartyType='Customer'  AND CP.PlantId=A.PlantId
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN TRN.Commitment COM ON COM.Id=A.CommitmentId
							LEFT JOIN [MST].[ProductMaster] PM ON COM.ProductMasterId=PM.Id
                            LEFT JOIN HKP.OrderStatus OS ON OS.Id=A.OrderStatusId
                            LEFT JOIN hkp.OrderCategory AS oc ON oc.Id=a.OrderCategoryId
                            LEFT JOIN HKP.Buyer B ON B.Id=A.BuyerId
                            LEFT JOIN ORG.Entity EN ON EN.Id=A.EntityId
							LEFT JOIN(select distinct moi.MasterOrderId,TotalAmount=SUM(SO.Qty*SO.Rate) 
									from TRN.MasterOrderItem moi
									LEFT JOIN TRN.SalesOrder so on so.MasterOrderItemId=moi.Id
									Group By moi.MasterOrderId) MS ON MS.MasterOrderId=A.Id
                            LEFT JOIN TRN.UserRemarksControl URC ON URC.MasterOrderId=A.Id
							LEFT JOIN [HKP].[RemarksControl] RC ON RC.Id=URC.RemarkControlId
                            WHERE A.CompanyId='" + companyId + @"' AND A.Id IN(Select distinct MOI.MasterOrderId from  TRN.SalesOrder SO
LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id = SO.MasterOrderItemId
LEFT JOIN TRN.MasterOrder MO ON MO.Id = MOI.MasterOrderId
Where SO.CheckByStatus = 'Checked' AND ApprovedStatus='To Be Approve' AND SO.ApproveBy='" + empId + @"')) AS TEMP WHERE " + strkey + " ORDER BY AddedDate Desc";

            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetSOListForCheck(string masterItemId)
        {
            try
            {
                var sql = @"SELECT  SO.Id,SO.ParentId
                            , SO.MasterOrderItemId
                            , MOI.MaterialMasterId
                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), SO.DeliveryDate, 106),' ','-')
                            , SO.DestinationId, D.UserName Destination
                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), SO.CommitmentDate, 106),' ','-')
                            , SO.ShipmentModeId
                            , SO.CustomerPOId
		                    , po.PONumber
                            ,SO.DestinationDescription
                            , SO.OrderStatusId, SO.OrderCategoryId
                            , SO.SOType, SO.ResponsiblePersonId
                            , SO.UpCharge, SO.Qty, SO.Rate, SO.IsFirstEntry,SO.Discount,EMP.EmployeeName ResponsiblePersonName
                            ,FORMAT (SO.LSD, 'dd-MMM-yyyy') as LSD ,FORMAT (SO.MainRawMaterialInhouseDate, 'dd-MMM-yyyy') as MainRawMaterialInhouseDate
                            ,FORMAT (SO.OtherRawMaterialInhouseDate, 'dd-MMM-yyyy') as OtherRawMaterialInhouseDate
                            ,FORMAT (SO.PlanExFactoryDate, 'dd-MMM-yyyy') as PlanExFactoryDate
                            , hasFirst=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[FirstCharacteristics] WHERE SalesOrderId=SO.Id)
                            --, (SELECT ISNULL(sum(Qty),0) total FROM(
							--		select Qty FROM  TRN.FirstCharacteristics AS FCS WHERE SO.Id= FCS.SalesOrderId
							--		union
							--		select Qty FROM TRN.SecondCharacteristics AS SCS WHERE SO.Id= SCS.SalesOrderId
							--		union
							--		select Qty FROM TRN.ThirdCharacteristics AS TCS WHERE SO.Id= TCS.SalesOrderId
							--	) SoT ) as SKUQty
                            ,(SELECT ISNULL(sum(Qty),0) FROM TRN.FirstCharacteristics AS FCS WHERE SO.Id= FCS.SalesOrderId) SKUQty
                            , isTax=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[SalesOrderTax] WHERE SalesOrderId=SO.Id)
                            ,ISNULL(POD.ProductionOrderId,'') ProductionOrderId,SO.Reason,SO.Description,SO.CM,SO.SalesOrderYear,SO.WeekNo
                            ,SO.ProductionBookedQty,SO.ProductionBookingLevel,SO.SalesExpense,SO.CM,SO.DirectMaterialCost,SO.DirectProcessCost,SO.Commission,SO.ValueLoss,SO.Other,SO.StockResponsiblePersonId,SO.ShipmentFromStock,SO.ProductionType,SEMP.EmployeeName StockResponsiblePerson,SO.PackingTypeId,PT.UserName PackingType,SO.ContractId,C.ContractNo,SO.CheckByStatus,SO.CheckByDate,SO.ApproveBy
                    FROM [TRN].[SalesOrder] AS SO
                   -- LEFT JOIN TRN.FirstCharacteristics SKU ON SKU.SalesOrderId=SO.Id
                    JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
                    LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                    LEFT JOIN dbo.EmployeeInformation AS EMP ON EMP.SystemId = SO.ResponsiblePersonId
                    LEFT JOIN dbo.EmployeeInformation AS SEMP ON SEMP.SystemId = SO.StockResponsiblePersonId
                    LEFT JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
                    LEFT JOIN [MST].[Destination] D ON D.Id=SO.DestinationId
                    LEFT JOIN HKP.PackingType PT ON PT.Id=SO.PackingTypeId
                    LEFT JOIN dbo.Contract C ON C.Id=SO.ContractId
                    WHERE SO.MasterOrderItemId='" + masterItemId + "' AND SO.CheckByStatus IN('To Be Check','Reject') AND SO.OrderCategoryId<>(Select Id from HKP.OrderCategory Where UserName='Projected')  ORDER BY SO.DeliveryDate";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSOListForApprove(string masterItemId)
        {
            try
            {
                var sql = @"SELECT  SO.Id,SO.ParentId
                            , SO.MasterOrderItemId
                            , MOI.MaterialMasterId
                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), SO.DeliveryDate, 106),' ','-')
                            , SO.DestinationId, D.UserName Destination
                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), SO.CommitmentDate, 106),' ','-')
                            , SO.ShipmentModeId
                            , SO.CustomerPOId
		                    , po.PONumber
                            ,SO.DestinationDescription
                            , SO.OrderStatusId, SO.OrderCategoryId
                            , SO.SOType, SO.ResponsiblePersonId
                            , SO.UpCharge, SO.Qty, SO.Rate, SO.IsFirstEntry,SO.Discount,EMP.EmployeeName ResponsiblePersonName
                            ,FORMAT (SO.LSD, 'dd-MMM-yyyy') as LSD ,FORMAT (SO.MainRawMaterialInhouseDate, 'dd-MMM-yyyy') as MainRawMaterialInhouseDate
                            ,FORMAT (SO.OtherRawMaterialInhouseDate, 'dd-MMM-yyyy') as OtherRawMaterialInhouseDate
                            ,FORMAT (SO.PlanExFactoryDate, 'dd-MMM-yyyy') as PlanExFactoryDate
                            , hasFirst=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[FirstCharacteristics] WHERE SalesOrderId=SO.Id)
                            --, (SELECT ISNULL(sum(Qty),0) total FROM(
							--		select Qty FROM  TRN.FirstCharacteristics AS FCS WHERE SO.Id= FCS.SalesOrderId
							--		union
							--		select Qty FROM TRN.SecondCharacteristics AS SCS WHERE SO.Id= SCS.SalesOrderId
							--		union
							--		select Qty FROM TRN.ThirdCharacteristics AS TCS WHERE SO.Id= TCS.SalesOrderId
							--	) SoT ) as SKUQty
                            ,(SELECT ISNULL(sum(Qty),0) FROM TRN.FirstCharacteristics AS FCS WHERE SO.Id= FCS.SalesOrderId) SKUQty
                            , isTax=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[SalesOrderTax] WHERE SalesOrderId=SO.Id)
                            ,ISNULL(POD.ProductionOrderId,'') ProductionOrderId,SO.Reason,SO.Description,SO.CM,SO.SalesOrderYear,SO.WeekNo
                            ,SO.ProductionBookedQty,SO.ProductionBookingLevel,SO.SalesExpense,SO.CM,SO.DirectMaterialCost,SO.DirectProcessCost,SO.Commission,SO.ValueLoss,SO.Other,SO.StockResponsiblePersonId,SO.ShipmentFromStock,SO.ProductionType,SEMP.EmployeeName StockResponsiblePerson,SO.PackingTypeId,PT.UserName PackingType,SO.ContractId,C.ContractNo,SO.CheckByStatus,SO.CheckByDate,SO.ApproveBy,SO.ApprovedStatus
                    FROM [TRN].[SalesOrder] AS SO
                   -- LEFT JOIN TRN.FirstCharacteristics SKU ON SKU.SalesOrderId=SO.Id
                    JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
                    LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                    LEFT JOIN dbo.EmployeeInformation AS EMP ON EMP.SystemId = SO.ResponsiblePersonId
                    LEFT JOIN dbo.EmployeeInformation AS SEMP ON SEMP.SystemId = SO.StockResponsiblePersonId
                    LEFT JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
                    LEFT JOIN [MST].[Destination] D ON D.Id=SO.DestinationId
                    LEFT JOIN HKP.PackingType PT ON PT.Id=SO.PackingTypeId
                    LEFT JOIN dbo.Contract C ON C.Id=SO.ContractId
                    WHERE SO.MasterOrderItemId='" + masterItemId + "' AND SO.CheckByStatus = 'Checked' AND SO.ApprovedStatus IN('To Be Approve','Reject') AND SO.OrderCategoryId<>(Select Id from HKP.OrderCategory Where UserName='Projected') ORDER BY SO.DeliveryDate";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetApproveByCboList()
        {
            var sql = @"SELECT E.SystemId As Value, E.EmployeeName As Text FROM dbo.AuthorizationConfig A 
                          INNER JOIN dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          WHERE A.ActionStatus='SalesOrderApproveBy' AND E.EmployeeStatus='Active'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCompanyCboList(string companyGroupId)
        {
            var sql = @"select Id Value,UserName Text from ORG.Company Where CompanyGroupId='"+ companyGroupId + "' AND Active=1";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetLineItemAdditionalInfoData(string lineItemId)
        {
            try
            {
                string sql = @"SELECT Flag=CAST(CASE WHEN SA.Id IS NULL THEN 0 ELSE 1 END AS bit),A.UserName,SA.Id,SA.LineItemId
,A.Id AdditionalInfoId,SA.Value,SA.Remarks,A.CharecterType,'' CharType,''datepic,A.Mandatory
FROM [HKP].[AdditionalInfo] A
OUTER APPLY(Select * from [dbo].[SalesAdditionalInfo] Where AdditionalInfoId=A.Id AND LineItemId='" + lineItemId + @"') SA
Where A.Category='LineItem'
Order By A.sequence";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSOAdditionalInfoData(string SalesOrderId)
        {
            try
            {
                string sql = @"SELECT Flag=CAST(CASE WHEN SA.Id IS NULL THEN 0 ELSE 1 END AS bit),A.UserName,SA.Id,SA.SalesOrderId
,A.Id AdditionalInfoId,SA.Value,SA.Remarks,A.CharecterType,'' CharType,''datepic,A.Mandatory
FROM [HKP].[AdditionalInfo] A
OUTER APPLY(Select * from [dbo].[SalesAdditionalInfo] Where AdditionalInfoId=A.Id AND SalesOrderId='" + SalesOrderId + @"') SA  Where A.Category='SalesOrder' Order By A.sequence";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
