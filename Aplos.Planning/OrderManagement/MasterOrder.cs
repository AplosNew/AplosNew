using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.OrderManagements;
using Library.Service.Enums;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
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

                DataTable dtFromMaster = _sqlRepository.GetDataTable("SELECT * FROM [TRN].[SalesOrder] WHERE MasterOrderItemId='" + masterItemId + "'");
                DataTable dtFromFirstCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.FirstCharacteristics Where SalesOrderId IN(Select Id from TRN.SalesOrder Where MasterOrderItemId='" + masterItemId + "')");
                DataTable dtFromSecondCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.SecondCharacteristics Where SalesOrderId IN(Select Id from TRN.SalesOrder Where MasterOrderItemId='" + masterItemId + "')");
                DataTable dtFromThirdCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.ThirdCharacteristics Where SalesOrderId IN(Select Id from TRN.SalesOrder Where MasterOrderItemId='" + masterItemId + "')");

                int SCount = 0;
                for (int m = 0; m < dtFromMaster.Rows.Count; m++)
                {
                    SCount++;
                    DataRow drSalesOrder = dsToSalesOrder.Tables[0].NewRow();
                    CopyRow(dtFromMaster.Rows[m], ref drSalesOrder);
                    drSalesOrder["Id"] = MasterId + Convert.ToInt32(NewId) + SCount;
                    NewSoId = drSalesOrder["Id"].ToString();
                    drSalesOrder["MasterOrderItemId"] = MasterId;
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
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsToSalesOrder, dsToFirstCharacteristics, dsToSecondCharacteristics, dsToThirdCharacteristics);


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void GetSalesOrderId(string masterItemId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT ISNULL((MAX(Id)+1),0) Id,SUM(Qty)Qty FROM [TRN].[SalesOrder] WHERE MasterOrderItemId='" + masterItemId + "'";
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
            DataSet dsToThirdCharacteristics;
            try
            {

                DataSet dsSOId;
                GetSalesOrderId(masterItemId, out dsSOId);
                string NewId = dsSOId.Tables[0].Rows[0]["Id"].ToString();
                decimal SOQty = Convert.ToDecimal(dsSOId.Tables[0].Rows[0]["Qty"].ToString());


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SalesOrder] WHERE 1=2", out dsToSalesOrder, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[FirstCharacteristics] WHERE 1=2", out dsToFirstCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SecondCharacteristics] WHERE 1=2", out dsToSecondCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[ThirdCharacteristics] WHERE 1=2", out dsToThirdCharacteristics, false, "1");

                DataTable dtFromMaster = _sqlRepository.GetDataTable("SELECT * FROM [TRN].[SalesOrder] WHERE Id='" + MasterId + "'");
                DataTable dtFromFirstCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.FirstCharacteristics WHERE SalesOrderId='" + MasterId + "'");
                DataTable dtFromSecondCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.SecondCharacteristics WHERE SalesOrderId='" + MasterId + "'");
                DataTable dtFromThirdCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.ThirdCharacteristics WHERE SalesOrderId='" + MasterId + "'");

                if (TotalMOIQty < SOQty + Convert.ToDecimal(dtFromMaster.Rows[0]["Qty"].ToString()))
                {
                    throw new Exception("SO Qty can't greater than Line Item Qty.");
                }

                DataRow drSalesOrder = dsToSalesOrder.Tables[0].NewRow();
                CopyRow(dtFromMaster.Rows[0], ref drSalesOrder);
                drSalesOrder["Id"] = NewId;
                drSalesOrder["ParentId"] = MasterId;
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

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsToSalesOrder, dsToFirstCharacteristics, dsToSecondCharacteristics, dsToThirdCharacteristics);


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

        public void SaveContractData(Dictionary<string, object> data, out string contractId, List<Dictionary<string, object>> funds, List<MasterOrderItem> masterOrderItem)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[Contract] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "Contract", out _Id);

                    data["Id"] = "C" + _Id;
                    data["CompanyId"] = identity.CompanyId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["CompanyId"] = identity.CompanyId;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                contractId = dsMaster.Tables[0].Rows[0]["Id"].ToString();


                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.MasterOrderItem WHERE MasterOrderId IN (" + data["MasterOrderId"] + ")", out DataSet dsMasterOrder, false, "1");

                foreach (var item in masterOrderItem)
                {
                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + item.Id + "'";

                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();

                        drmo["ContractId"] = contractId;
                        drmo["UpdatedBy"] = identity.Name;
                        drmo["UpdatedDate"] = DateTime.Now.ToString();
                        drmo["UpdatedFromIP"] = identity.IPAddress;

                        drmo.EndEdit();

                    }

                }



                #region FUND 

                DataSet dsChild;

                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ContractFund where  ContractId='" + contractId + "'", out dsChild, false, "1");
                #region data update

                if (funds != null)
                {
                    foreach (var item in funds)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = GetContractFundPK();
                            item["ContractId"] = contractId;

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsChild, dsMasterOrder);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IEnumerable<object> GetProductLibrary()
        {
            try
            {
                //string sql = @"SELECT PL.Id as Value,Text=CASE WHEN PL.RecipeOrProductionGroup = 'Recipe' THEN RGM.UserName+' ('+PL.RecipeOrProductionGroup+')' ELSE PL.ProductionGroup+' ('+PL.RecipeOrProductionGroup+')' END
                //        FROM dbo.ProductLibrary PL
                //        LEFT JOIN[TRN].[RecipeGlobalMaster] RGM ON RGM.Id = PL.RecipeId WHERE PL.Active =1";
                string sql = @"Select PL.Id,PL.Code,PL.ShortName,PL.StandardName, UserName=CASE WHEN PL.RecipeOrProductionGroup = 'Recipe' THEN RGM.UserName+' ('+PL.RecipeOrProductionGroup+')' ELSE PL.ProductionGroup+' ('+PL.RecipeOrProductionGroup+')' END
FROM dbo.ProductLibrary PL
LEFT JOIN[TRN].[RecipeGlobalMaster] RGM ON RGM.Id = PL.RecipeId WHERE PL.Active =1";
                return _sqlRepository.GetDataCollection(sql);
            } 
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
                strSQL = @"SELECT B.*,MM.UserName MaterialMaster,MMA.ShortName Article,U.Code,C.UserName CostingItem 
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
                            WHERE B.MasterOrderItemId='" + itemId + "'";
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
                        WHERE MOI.Id='" + ItemId + @"' ORDER BY CHV.UserName";
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
                       WHERE MOI.Id='" + ItemId + "' ORDER BY CHV.UserName";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetUoMCboByProductMaster()
        {
            string sql = @"Select P.Id ProductMasterId,BUoM.Id AS Value,BUoM.UserName AS Text,CAST(1 as bit) ByDefault from [MST].[ProductMaster] P                            LEFT JOIN SCS.UnitOfMeasurement BUoM ON BUoM.Id=P.BaseUOMId                            Where ISNULL(BUoM.Id,'')<>''                            UNION ALL                            Select AUom.ProductMasterId,BUoM.Id,BUoM.UserName,CAST(0 as bit) ByDefault from MST.ProductMasterAlternativeUoM AUoM                             LEFT JOIN SCS.UnitOfMeasurement BUoM ON BUoM.Id=AUom.AlternativeUOMId                            Where ISNULL(BUoM.Id,'')<>''                            Order by ProductMasterId";
            return _sqlRepository.GetDataCollection(sql);
        }

    }
}
