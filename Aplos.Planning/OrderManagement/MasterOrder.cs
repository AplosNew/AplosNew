using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Enums;
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
    }
}
