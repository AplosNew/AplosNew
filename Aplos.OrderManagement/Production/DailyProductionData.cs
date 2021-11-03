using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;

namespace Library.OrderManagement.Production
{
    public class DailyProductionData
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public DailyProductionData()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        public IEnumerable<object> GetOperation(string ProdOrderId)
        {
            try
            {
                var Sql = @"select OP.ID as Value,OP.UserName as Text from mst.OperationVariation OP
                            join trn.ProductionBulletinTemplateDetail bt on bt.OperationVariationId=OP.Id
                            join trn.ProductionBulletinTemplateMaster pt on pt.Id=bt.ProductionBulletinTemplateMasterId
                            join trn.ProductionBulletinTemplate pb on pb.Id=pt.ProductionBulletinTemplateId
                            where pb.ProductionOrderId='" + ProdOrderId + "'";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string Create(IEnumerable<DailyProduction> DataToSave)
        {

            try
            {
                DataSet dsMaster;
                string TableName = "TRN.DailyProduction";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<DailyProduction> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                string _Id = "";

                foreach (DailyProduction item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].Id == null)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "PS" + _Id;
                        dr["PlantId"] = item.PlantId;
                        dr["ProcessId"] = item.ProcessId;
                        dr["SalesOrderMasterId"] = item.SalesOrderMasterID;
                        dr["MaterialMasterId"] = item.MaterialMasterId;
                        dr["MaterialMasterArticleId"] = item.MaterialMasterArticleId;
                        dr["WorkCenterMasterId"] = item.WorkCenterMasterId;
                        dr["ProductionDate"] = item.ProductionDate;
                        dr["Grade"] = item.Grade;
                        dr["EntityId"] = item.EntityId;
                        dr["ShiftId"] = item.ShiftId;
                        dr["Quantity"] = item.Quantity;
                        dr["ProductionBookingPeriodId"] = item.ProductionBookingPeriodId;
                        dr["ProductionOrderId"] = item.ProductionOrderId;
                        dr["OperationVariationId"] = item.OperationVariationId;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["Remarks"] = item.Remarks;
                        dr["EmployeeInformationSystemID"] = item.EmployeeInformationSystemID;
                        dr["RefNo"] = item.RefNo;
                        dr["WorkStationDailyID"] = item.WorkStationDailyID;
                        dr["FLAG"] = item.FLAG;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();


                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                return MasterId;


            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IEnumerable<object> GetListAPIforProduction(string ProdnDate, string ProcessId, string EntityId, string ShiftId, string WkId)
        {
            try
            {
                var Sql = @"select SUM(ps.Quantity)Quantity,ps.OperationVariationId,ps.ProductionOrderId, p.UserName as Plant,ps.PlantId,ps.ProcessId,Pr.UserName as Process ,ent.UserName as Entity,ent.Id as EntId,opv.UserName as Operation
                                                            from TRN.DailyProduction ps
                                                            left join ORG.Plant p on ps.PlantId = p.Id
                                                            left join HKP.Process pr on ps.ProcessId = pr.Id
                                                            left join org.Entity ent on ps.EntityId= ent.Id
                                                            left join mst.CompliedShiftGrouping sh on ps.ShiftId=sh.Id
                                                            left join mst.OperationVariation opv on opv.Id=ps.OperationVariationId
                                                            left join dbo.EmployeeInformation emp
                                                            on ps.EmployeeInformationSystemID = emp.SystemId
                                                            where isnull(ps.ProductionDate, '') = '" + ProdnDate + "'" +
                                                           "and isnull(ps.ProcessId,'')= '" + ProcessId + "' and isnull(ps.EntityId,'')= '" + EntityId + "'and isnull(ps.ShiftId,'')= '" + ShiftId + "'" +
                                                          " and isnull(ps.WorkCenterMasterID,'')= '" + WkId + "' GROUP BY ps.OperationVariationId," +
                                                          "p.UserName,ps.PlantId,ps.ProcessId,Pr.UserName,ent.UserName,ent.Id,opv.UserName,ps.ProductionOrderId";

                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception)
            {
                throw;
            }



        }

        public IEnumerable<object> GetWk(string AddedBy)
        {
            try
            {
                var Sql = @"select distinct ope.WorkCenterId as Value,wk.UserName as Text from dbo.OperationWiseEmployee ope 
                            left join scs.WorkCenterMaster wk on ope.WorkCenterId=wk.Id
                            where ope.AddedBy='" + AddedBy + "'";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetOP(string AddedBy, string WkId)
        {
            try
            {
                var Sql = @"select distinct ope.OperationVariationId as Value,ov.UserName as Text from dbo.OperationWiseEmployee ope 
                left join mst.OperationVariation ov on ope.OperationVariationId=ov.Id
                where ope.AddedBy='" + AddedBy + "' and ope.WorkCenterId='" + WkId + "'";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmp(string AddedBy, string WkId, string OPId)
        {
            try
            {
                var Sql = @"select distinct ope.EmployeeId as Value,emp.EmployeeName as Text from dbo.OperationWiseEmployee ope 
                    left join dbo.EmployeeInformation emp on ope.EmployeeId=emp.SystemId
                    where ope.AddedBy='" + AddedBy + "' and ope.WorkCenterId='" + WkId + "' and ope.OperationVariationId='" + OPId + "'";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string Delete(IEnumerable<DailyProduction> DataToDelete)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                DataSet dsMaster;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                foreach (var item in DataToDelete)
                {
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        objCon.OpenDataSetThroughAdapter("select * from TRN.DailyProduction where Id= '" + item.Id + "' ", out dsMaster, false, "1");
                        if (dsMaster.Tables[0].Rows.Count > 0)
                        {
                            objCon.ExecuteNonQueryWrapper("Delete FROM TRN.DailyProduction where Id='" + item.Id + "'", true, "1");
                        }
                    }

                }

                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                return ex.ToString();

            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
            return "";
        }

        public IEnumerable<object> GetDetailProductionList(string ProdnDate, string EntityId, string ProcessId, string ShiftId, string WkId, string PoId, string OPId)
        {
            try
            {

                var sql = @"select dp.Id, CAST(dp.AddedDate as time) Time,Buyer =STUFF((select distinct ','+XB.UserName from
                                    trn.SalesOrder XSO                                        
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        LEFT JOIN TRN.ProductionOrder po on po.Id=Xpod.ProductionOrderId
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                        left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                        left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                                                                        
                                            where po.Id=" + PoId + @" for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')  ,                   
  dp.Quantity,(emp.EmployeeName) Employee,dp.EmployeeInformationSystemID as EmpId, opv.UserName as Operation,dp.AddedBy,dp.SalesOrderMasterID as SOId,dp.ProductionOrderId as POId,E.UserName as Entity,
                                    Pr.UserName as Process,
                                    Wc.UserName as WorkCenter,
                                    csg.Description as Shift from TRN.DailyProduction dp
                                                                    left join ORG.Entity E on dp.EntityId=E.Id
                                                                    left join HKP.Process pr on dp.ProcessId=pr.Id
                                                                    left join SCS.WorkCenterMaster wc on dp.WorkCenterMasterId=wc.Id
																	left join mst.OperationVariation opv on dp.OperationVariationId=opv.Id
                                                                    left join MST.CompliedShiftGrouping csg on dp.ShiftId=csg.Id
                                                                    left join dbo.EmployeeInformation emp on dp.EmployeeInformationSystemID=emp.SystemId                      
                            where isnull(dp.ProductionDate, '') = '" + ProdnDate + "'and isnull(dp.EntityId,'') = '" + EntityId + "'and isnull(dp.ProcessId,'')= '" + ProcessId + "' and isnull(dp.ShiftId,'')= '" + ShiftId + "'and isnull(dp.WorkCenterMasterId,'')='" + WkId + "'and isnull(dp.ProductionOrderId,'')='" + PoId + "' and isnull(dp.OperationVariationId,'')='" + OPId + "' ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public string CreateOp(IEnumerable<operationwise> DataToSavex)
        {

            try
            {
                DataSet dsMaster;
                string TableName = "dbo.OperationWiseEmployee";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                if (DataToSavex.Count() == 0)
                    return "";

                List<operationwise> items = DataToSavex.ToList();

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                string _Id = "";


                foreach (operationwise item in DataToSavex)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].Id == null)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "OP" + _Id;
                        dr["WorkCenterId"] = item.WorkCenterId;
                        dr["EntryDate"] = item.EntryDate;
                        dr["OperationVariationId"] = item.OperationVariationId;
                        dr["EmployeeId"] = item.EmployeeId;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                return MasterId;
            }
            catch (Exception ex)
            {
                return ex.ToString();
                //  throw (ex);
            }
        }


        public string DeleteOp(IEnumerable<operationwise> DataToDelete)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                DataSet dsMaster;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                foreach (var item in DataToDelete)
                {
                    if (!string.IsNullOrEmpty(item.WorkCenterId) && !string.IsNullOrEmpty(item.AddedBy) && !string.IsNullOrEmpty(item.OperationVariationId))
                    {
                        objCon.OpenDataSetThroughAdapter("select * from dbo.OperationWiseEmployee where WorkCenterId='" + item.WorkCenterId + "'and AddedBy='" + item.AddedBy + "'and OperationVariationId='" + item.OperationVariationId + "'  ", out dsMaster, false, "1");
                        if (dsMaster.Tables[0].Rows.Count > 0)
                        {
                            objCon.ExecuteNonQueryWrapper("Delete FROM dbo.OperationWiseEmployee where WorkCenterId='" + item.WorkCenterId + "'and AddedBy='" + item.AddedBy + "'and OperationVariationId='" + item.OperationVariationId + "'", true, "1");
                        }
                    }

                }

                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                return ex.ToString();

            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
            return "";
        }

        public IEnumerable<object> GetOPSkill(string Operation)
        {
            try
            {
                var Sql = @"select OP.SkillId,OP.OperationMasterId from MST.OperationVariation OP where OP.UserName='" + Operation + "'";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

}


    public class DailyProduction
    {

        #region Scalar Properties

        public string Id { get; set; }
        public DateTime? ProductionDate { get; set; }
        public decimal Quantity { get; set; }
        public string ShiftId { get; set; }
        public string EntityId { get; set; }
        public string ProductionBookingPeriodId { get; set; }
        public string Grade { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string Remarks { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        public string AddedBy { get; set; }

        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }


        #endregion Audit Properties

        #region Navigation

        public string PlantId { get; set; }
        public string ProcessId { get; set; }
        public string SalesOrderMasterID { get; set; }
        public string ProductionOrderId { get; set; }
        public string MaterialMasterId { get; set; }
        public string MaterialMasterArticleId { get; set; }
        public string WorkCenterMasterId { get; set; }
        public string OperationVariationId { get; set; }
        public string EmployeeInformationSystemID { get; set; }
        public string RefNo { get; set; }
        public string WorkStationDailyID { get; set; }
        public string FLAG { get; set; }


        #endregion Navigation

    }

    public class operationwise
    {
        #region Scalar Properties
        public string Id { get; set; }
        public DateTime? EntryDate { get; set; }
        public string WorkCenterId { get; set; }
        public string OperationVariationId { get; set; }
        public string EmployeeId { get; set; }

        #endregion Scalar Properties 

        #region Audit Properties

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

    }




