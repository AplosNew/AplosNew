using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Linq;
using System.Collections.Specialized;

namespace Library.OrderManagement.Production
{
    public class EmployeeOperationsService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public EmployeeOperationsService()
        {
            _sqlRepository = new SqlRepository();

        }
        #endregion Constructor

        public IEnumerable<object> GetEntity()
        {
            try
            {
                var Sql = @"Select distinct e.Id as Value , e.UserName as Text from trn.ProductionOrder po
                            left join org.Entity e on e.Id = po.EntityId
                            left join hkp.EntityProcessTag ett on ett.EntityId = e.Id";
                //where ope.AddedBy='" + AddedBy + "'
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetWorkCenter(string PId, string entityId)
        {
            try
            {
                var Sql = @"select distinct Id as Value , UserName as Text from SCS.WorkCenterMaster where ProcessId = '" + PId + "' AND EntityId='"+ entityId + @"'";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetProcess(string EId)
        {
            try
            {
                // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //var str = @"
                //            Select distinct p.Id as Value , p.UserName as Text
                //            from TRN.ProductionBulletinTemplateMaster ptm
                //            left
                //            join hkp.process p on p.ID = ptm.ProcessId
                //            order by UserName asc";
                var str = @"Select distinct p.Id as Value , p.UserName as Text
                            from hkp.Process p
                                left join hkp.EntityProcessTag ept on ept.ProcessId = p.Id
                                left join org.Entity e on e.Id = ept.EntityId
                                where e.Id = '" + EId + @"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPeriod(out string CurrPer)
        {
            try
            {
                // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = "Select id as Value , UserName as Text , StartTime , EndTime from hkp.ProductionBookingPeriod order by StartTime asc";

                var periodSql = @"Select id as Value , UserName as Text , StartTime , EndTime from hkp.ProductionBookingPeriod where CONVERT(VARCHAR(8), StartTime, 108) <= Convert(varchar(8), GETDATE(), 108)
                                        and CONVERT(VARCHAR(8), EndTime, 108) >= Convert(varchar(8), GETDATE(), 108)
                                        order by EndTime desc";
                DataTable dtPeriod = _sqlRepository.GetDataTable(periodSql);
                CurrPer = null;
                if (dtPeriod.Rows.Count > 0)
                {
                    CurrPer = dtPeriod.Rows[0]["Value"].ToString();

                }

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetShift()
        {
            try
            {
                // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = "Select SystemID as Value , ShiftDefinationDescription as Text from dbo.ShiftDefination";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPOs(string entityId)
        {
            try
            {
                //var dd = @"Select Id from hkp.ProductionStatus where UserName like 'Run%'";
                //DataTable dtId = _sqlRepository.GetDataTable(dd);
                //var str = @"Select distinct po.Id
                //            from Scs.WorkCenterMaster wc
                //            left join org.Entity e on e.ID = wc.EntityId
                //            left join trn.ProductionOrder po on po.EntityId = e.Id
                //            where wc.Id = '" + wk + @"' and po.ProductionStatusId = '" + dtId.Rows[0]["Id"].ToString() + "'";
                string str = @"select PO.Id from TRN.ProductionOrder PO
LEFT JOIN HKP.ProductionStatus PS ON PS.id=PO.ProductionStatusId
Where PS.UserName='Running' AND PO.EntityId='"+ entityId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getPODetails(string POId)
        {
            try
            {
                var str = @"Select mo.BuyerReferenceNo , mo.OwnReferenceNo , moi.ArticleId , mma.StandardName as Article
                            from trn.ProductionOrder po
                            left join trn.ProductionOrderDetail pod on pod.ProductionOrderId = po.Id
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            left join mst.MaterialMasterArticle mma on mma.Id = moi.ArticleId
                            where po.id= '" + POId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetEmps()
        {
            try
            {
                var str = @"Select SystemId ,EmployeeCode , EmployeeName from dbo.EmployeeInformation";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetResp(string WKId)
        {
            try
            {
                var str = @"Select top 1 owe.ResponsiblePersonId , ei.EmployeeName from dbo.OperationWiseEmployees owe
                            left join dbo.EmployeeInformation ei on ei.SystemId = owe.ResponsiblePersonId
                            where owe.WorkCenterId = '" + WKId + @"'
                            order by owe.Date desc";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetOperationsData(string PId, string Period, string ProcessId, string WorkCenterId)
        {
            //Filling the PeriodId
            string currPeriod = "";
            string workCenId = "";
            if (Period == null)
            {
                var periodSql = @"Select id as Value , UserName as Text , StartTime , EndTime from hkp.ProductionBookingPeriod where CONVERT(VARCHAR(8), StartTime, 108) <= Convert(varchar(8), GETDATE(), 108)
                                        and CONVERT(VARCHAR(8), EndTime, 108) >= Convert(varchar(8), GETDATE(), 108)
                                        order by EndTime desc";

                DataTable dtPeriod = _sqlRepository.GetDataTable(periodSql);
                if (dtPeriod.Rows.Count <= 0)
                {
                    throw new Exception("Please Select the period Manually!! There is no defined Period for the current time!!");
                }
                currPeriod = dtPeriod.Rows[0]["Value"].ToString();
            }
            else
            {
                currPeriod = Period;
            }
            if (WorkCenterId == null)
            {
                workCenId = "";
            }
            else
            {
                workCenId = " WHERE owe.workcenterId='" + WorkCenterId + @"'";
            }
            DataSet dsMaster, dsPS;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("select EmployeeOperationBackDateAllow from dbo.PlantWiseHRMSSetting Where PlantID='" + identity.PlantId + "' ", out dsPS, false, "1");
            int ad = Convert.ToInt32(dsPS.Tables[0].Rows[0]["EmployeeOperationBackDateAllow"]);
            var str = @"select OP.ID as OperationId,OO.UserName OperationName, OP.OperationMasterId as MasterOperationId  ,OP.Code as OperationCode ,OP.UserName as OperationName, bt.Sequence , owe.EmployeeId , isnull(o.WIP,0) as WIP,
                        isnull(Sum(owe.Qty),0) as Qty ,
                      
                        ei.EmployeeCode , ei.EmployeeName as EmpName
                        from mst.OperationVariation OP left join mst.Operation OO on OO.Id=op.OperationId
                        left join trn.ProductionBulletinTemplateDetail bt on bt.OperationVariationId=OP.Id
                        left join trn.ProductionBulletinTemplateMaster pt on pt.Id=bt.ProductionBulletinTemplateMasterId
                        left join trn.ProductionBulletinTemplate pb on pb.Id=pt.ProductionBulletinTemplateId
                        left join ( Select owe.ProductionOrderId , owe.OperationVariationId , owe.EmployeeId, owe.Date , isnull(owep.Qty,0) as Qty 
						from dbo.OperationWiseEmployees owe 
						left join dbo.OperationWiseEmployees owep on owep.Id = owe.Id and owep.PeriodId   ='" + currPeriod + "' "+ workCenId + @"
						) as owe on owe.OperationVariationId = OP.Id and owe.ProductionOrderId = pb.ProductionOrderId and owe.Date >   Convert(date, DateAdd(DAY, -" +  ad + @", GetDate())) 
                        left join dbo.EmployeeInformation ei on ei.SystemId = owe.EmployeeId
                        left join dbo.EmployeeOperationWip o on o.OperationVariationId = op.Id and o.ProductionOrderId = pb.ProductionOrderId and o.ProcessId = pt.ProcessId
						where pb.ProductionOrderId='" + PId + @"' and pt.ProcessId ='" + ProcessId + @"'
						
						group by OP.Id , op.Code , op.UserName,OO.UserName , bt.Sequence , owe.EmployeeId , ei.EmployeeCode , op.OperationMasterId , o.WIP , ei.EmployeeName
                        order by Sequence";

            //      var str = @"select OP.ID as OperationId, OP.Code as OperationCode ,OP.UserName as OperationName, bt.Sequence , owe.EmployeeId , 
            //                  Sum(owe.Qty) as Qty ,
            //                  --Sum(owe.Period1) as Period1c , Sum(owe.Period2) as Period2 , Sum(owe.Period2) as Period2c , Sum(owe.Period3) as Period3 ,Sum(owe.Period3) as Period3c , 
            //                  --Sum(owe.Period4) as Period4 ,Sum(owe.Period4) as Period4c , Sum(owe.Period5) as Period5 , Sum(owe.Period5) as Period5c , 
            //                  --Sum(owe.Period6) as Period6 , Sum(owe.Period6) as Period6c , 
            //                  ei.EmployeeCode
            //                  from mst.OperationVariation OP
            //                  left join trn.ProductionBulletinTemplateDetail bt on bt.OperationVariationId=OP.Id
            //                  left join trn.ProductionBulletinTemplateMaster pt on pt.Id=bt.ProductionBulletinTemplateMasterId
            //                  left join trn.ProductionBulletinTemplate pb on pb.Id=pt.ProductionBulletinTemplateId
            //                  left join ( Select owe.* from dbo.OperationWiseEmployees owe ) as owe on owe.OperationVariationId = OP.Id and owe.ProductionOrderId = pb.ProductionOrderId and owe.Date =   Convert(date, DateAdd(DAY, -1, GetDate())) 
            //                  and owe.PeriodId ='" + currPeriod + @"'
            //                  left join dbo.EmployeeInformation ei on ei.SystemId = owe.EmployeeId
            //where pb.ProductionOrderId='" + PId + @"'
            //                  group by OP.Id , op.Code , op.UserName , bt.Sequence , owe.EmployeeId , ei.EmployeeCode
            //                  order by Sequence
            //                 ";

            //       from mst.OperationVariation OP
            //                  left join trn.ProductionBulletinTemplateDetail bt on bt.OperationVariationId=OP.Id
            //                  left join trn.ProductionBulletinTemplateMaster pt on pt.Id=bt.ProductionBulletinTemplateMasterId
            //                  left join trn.ProductionBulletinTemplate pb on pb.Id=pt.ProductionBulletinTemplateId
            //                  left join dbo.OperationWiseEmployees owe on owe.OperationVariationId = OP.Id and owe.ProductionOrderId = pb.ProductionOrderId
            //                  left join dbo.EmployeeInformation ei on ei.SystemId = owe.EmployeeId
            //where pb.ProductionOrderId='" + PId+@"'  --and owe.PeriodId = '"+ currPeriod + @"' and owe.Date =  Convert(date, DateAdd(DAY, -1, GetDate())) 
            //group by OP.Id , op.Code , op.UserName , bt.Sequence , owe.EmployeeId , ei.EmployeeCode
            //                  order by Sequence


            return _sqlRepository.GetDataCollection(str);
        }



        public void saveData(List<Dictionary<string, object>> data, string WorkCenter, string ProcessId, string ShiftId, string POId, string Date, string PeriodId, string ResponsiblePersonId,string plantId)
        {
            try
            {
                DataSet dsMaster,dsPS;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string TableName = "dbo.OperationWiseEmployees";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select EmployeeOperationBackDateAllow from dbo.PlantWiseHRMSSetting Where PlantID='" + plantId + "' ", out dsPS, false, "1");
                if (string.IsNullOrEmpty(dsPS.Tables[0].Rows[0]["EmployeeOperationBackDateAllow"].ToString()))
                {
                    throw new Exception("Please Define Employee Operation Back Date Entry Allow Days in Plant Wise HRMS Setting.");
                }
                else
                {
                    int ad =Convert.ToInt32(dsPS.Tables[0].Rows[0]["EmployeeOperationBackDateAllow"]);
                    var yesterday = DateTime.Today.AddDays(-ad);
                    if (Convert.ToDateTime(Date) < yesterday)
                    {
                        throw new Exception("Please select Date properly! Today or Yesterday's data can be added/updated.");
                    }
                }


               

                #region Detail

              
                con.OpenDataSetThroughAdapter("select *  from dbo.OperationWiseEmployees where 1 = 2 ", out dsMaster, false, "1");


                //Filling the EmpSystemIds

                var empStr = @"Select distinct SystemId , EmployeeCode from dbo.EmployeeInformation";
                DataTable dt = _sqlRepository.GetDataTable(empStr);
                for (int i = 0; i < data.Count; i++)
                {
                    dt.DefaultView.RowFilter = @"EmployeeCode = '" + data[i]["EmployeeCode"].ToString() + "'";
                    if (dt.DefaultView.Count > 0)
                    {
                        data[i]["EmployeeId"] = dt.DefaultView[0]["SystemId"].ToString();


                        data[i]["Qty"] = getNum(data[i]["Qty"]);
                    }
                    else
                    {
                        throw new Exception("The Employee Code in Serial " + data[i]["Serial"] + " is not Present");
                    }
                }

                //Filling the PeriodId
                string currPeriod = "";
                if (PeriodId == null)
                {
                    var periodSql = @"Select id as Value , UserName as Text , StartTime , EndTime from hkp.ProductionBookingPeriod where CONVERT(VARCHAR(8), StartTime, 108) <= Convert(varchar(8), GETDATE(), 108)
                                        and CONVERT(VARCHAR(8), EndTime, 108) >= Convert(varchar(8), GETDATE(), 108)
                                        order by EndTime desc";

                    DataTable dtPeriod = _sqlRepository.GetDataTable(periodSql);
                    if (dtPeriod.Rows.Count <= 0)
                    {
                        throw new Exception("Please Select the period Manually!! There is no defined Period for the current time!!");
                    }
                    currPeriod = dtPeriod.Rows[0]["Value"].ToString();
                }
                else
                {
                    currPeriod = PeriodId;
                }

                //Filling the DsMaster DataSet for saving
                string _Id = "";


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data[i]["Id"] = "OP" + _Id;
                    dr["Id"] = "OP" + _Id;
                    dr["ProcessId"] = ProcessId;
                    dr["ShiftId"] = ShiftId;
                    dr["WorkCenterId"] = WorkCenter;
                    dr["ProductionOrderId"] = POId;
                    dr["OperationVariationId"] = data[i]["OperationId"];
                    dr["EmployeeId"] = data[i]["EmployeeId"];
                    dr["ResponsiblePersonId"] = ResponsiblePersonId;
                    dr["Date"] = Convert.ToDateTime(Date.ToString());
                    dr["Qty"] = data[i]["Qty"];
                    dr["PeriodId"] = currPeriod;
                    dr["Remarks"] = data[i]["Remarks"];
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                #endregion Detail

                #region WIP
                DataSet dsSum;
                ConnectionManager.DAL.ConManager c = new ConnectionManager.DAL.ConManager("1");
                c.OpenDataSetThroughAdapter("select *  from  dbo.EmployeeOperationWip where ProductionOrderId = '" + POId + "' and ProcessId ='" + ProcessId + "' order by Cast(OperationSequence AS int) asc", out dsSum, false, "1");
                string _SId = "";

                DataTable dtSum = dsSum.Tables[0];

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    dsSum.Tables[0].DefaultView.RowFilter = @"OperationVariationId='" + data[i]["OperationId"].ToString() + "' and OperationSequence ='" + data[i]["Sequence"].ToString() + "'";
                    if (dsSum.Tables[0].DefaultView.Count > 0)
                    {
                        dsSum.Tables[0].DefaultView[0].Row.BeginEdit();
                        dsSum.Tables[0].DefaultView[0]["Qty"] = clsStaticInfo.dbl(dsSum.Tables[0].DefaultView[0]["Qty"].ToString()) + clsStaticInfo.dbl(data[i]["Qty"].ToString());
                        dsSum.Tables[0].DefaultView[0].Row.EndEdit();
                    }
                    else
                    {
                        DataRow dd = dsSum.Tables[0].NewRow();
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.EmployeeOperationWip", out _SId);
                        dd["Id"] = "OW" + _SId;
                        dd["ProductionOrderId"] = POId;
                        dd["OperationVariationId"] = data[i]["OperationId"].ToString();
                        dd["OperationSequence"] = data[i]["Sequence"].ToString();
                        dd["ProcessId"] = ProcessId;
                        dd["Qty"] = clsStaticInfo.dbl(data[i]["Qty"].ToString());
                        dd["AddedBy"] = identity.Name;
                        dd["AddedDate"] = System.DateTime.Now.ToString();
                        dd["AddedFromIP"] = identity.IPAddress;
                        dd["UpdatedBy"] = identity.Name;
                        dd["UpdatedDate"] = System.DateTime.Now.ToString();
                        dd["UpdatedFromIP"] = identity.IPAddress;
                        dsSum.Tables[0].Rows.Add(dd);


                    }


                }

                //For WIP
                for (int i = 0; i < dsSum.Tables[0].Rows.Count; i++)
                {
                    if (clsStaticInfo.dbl(dsSum.Tables[0].Rows[i]["OperationSequence"].ToString()) == 1)
                    {
                        dsSum.Tables[0].Rows[i].BeginEdit();
                        dsSum.Tables[0].Rows[i]["WIP"] = 0;
                        dsSum.Tables[0].Rows[i].EndEdit();
                    }
                    else
                    {
                        dsSum.Tables[0].Rows[i].BeginEdit();
                        dsSum.Tables[0].Rows[i]["WIP"] = clsStaticInfo.dbl(dsSum.Tables[0].Rows[i - 1]["Qty"].ToString()) - clsStaticInfo.dbl(dsSum.Tables[0].Rows[i]["Qty"].ToString());
                        if (clsStaticInfo.dbl(dsSum.Tables[0].Rows[i]["WIP"].ToString()) < 0)
                        {
                            throw new Exception("WIP is Exceeding in Operation Sequence - " + dsSum.Tables[0].Rows[i]["OperationSequence"].ToString());
                        }
                        dsSum.Tables[0].Rows[i].EndEdit();
                    }
                }

                #endregion Summary

                #region Employee Production Processing Half
                DataSet dsPlan = null;
                ConnectionManager.DAL.ConManager co = new ConnectionManager.DAL.ConManager("1");
                co.OpenDataSetThroughAdapter("select *  from  dbo.EmployeeWiseProductionProcessing where Date='" + Date + "' and ProductionOrderId='" + POId + "'", out dsPlan, false, "1");

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    dsPlan.Tables[0].DefaultView.RowFilter = @"EmployeeId = '" + data[i]["EmployeeId"] + "' and OperationVariationId='" + data[i]["OperationId"].ToString() + "'";
                    if (dsPlan.Tables[0].DefaultView.Count > 0)
                    {
                        dsPlan.Tables[0].DefaultView[0].Row.BeginEdit();
                        dsPlan.Tables[0].DefaultView[0]["Qty"] = clsStaticInfo.dbl(dsPlan.Tables[0].DefaultView[0]["Qty"].ToString()) + clsStaticInfo.dbl(data[i]["Qty"].ToString());
                        dsPlan.Tables[0].DefaultView[0].Row.EndEdit();
                    }
                    else
                    {
                        DataRow dr = dsPlan.Tables[0].NewRow();
                        dr["Id"] = dsMaster.Tables[0].Rows[i]["Id"].ToString() + i.ToString();
                        dr["Date"] = Convert.ToDateTime(Date);
                        dr["EmployeeId"] = dsMaster.Tables[0].Rows[i]["EmployeeId"];
                        dr["MasterOperationId"] = data[i]["MasterOperationId"];
                        dr["OperationVariationId"] = data[i]["OperationId"].ToString();
                        dr["ProductionOrderId"] = POId;
                        dr["Qty"] = clsStaticInfo.dbl(data[i]["Qty"].ToString());
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsPlan.Tables[0].Rows.Add(dr);
                    }
                }



                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsSum, dsPlan);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void saveRowItemData(List<Dictionary<string, object>> data, string WorkCenter, string ProcessId, string ShiftId, string POId, string Date, string PeriodId, string ResponsiblePersonId, string plantId, string NxtOPVariationId)
        {
            try
            {
                DataSet dsMaster, dsPS;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string TableName = "dbo.OperationWiseEmployees";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select EmployeeOperationBackDateAllow from dbo.PlantWiseHRMSSetting Where PlantID='" + plantId + "' ", out dsPS, false, "1");
                if (string.IsNullOrEmpty(dsPS.Tables[0].Rows[0]["EmployeeOperationBackDateAllow"].ToString()))
                {
                    throw new Exception("Please Define Employee Operation Back Date Entry Allow Days in Plant Wise HRMS Setting.");
                }
                else
                {
                    int ad = Convert.ToInt32(dsPS.Tables[0].Rows[0]["EmployeeOperationBackDateAllow"]);
                    var yesterday = DateTime.Today.AddDays(-ad);
                    if (Convert.ToDateTime(Date) < yesterday)
                    {
                        throw new Exception("Please select Date properly! Today or Yesterday's data can be added/updated.");
                    }
                }




                #region Detail


                con.OpenDataSetThroughAdapter("select *  from dbo.OperationWiseEmployees where 1 = 2 ", out dsMaster, false, "1");


                //Filling the EmpSystemIds

                var empStr = @"Select distinct SystemId , EmployeeCode from dbo.EmployeeInformation";
                DataTable dt = _sqlRepository.GetDataTable(empStr);
                for (int i = 0; i < data.Count; i++)
                {
                    dt.DefaultView.RowFilter = @"EmployeeCode = '" + data[i]["EmployeeCode"].ToString() + "'";
                    if (dt.DefaultView.Count > 0)
                    {
                        data[i]["EmployeeId"] = dt.DefaultView[0]["SystemId"].ToString();


                        data[i]["Qty"] = getNum(data[i]["Qty"]);
                    }
                    else
                    {
                        throw new Exception("The Employee Code in Serial " + data[i]["Serial"] + " is not Present");
                    }
                }

                //Filling the PeriodId
                string currPeriod = "";
                if (PeriodId == null)
                {
                    var periodSql = @"Select id as Value , UserName as Text , StartTime , EndTime from hkp.ProductionBookingPeriod where CONVERT(VARCHAR(8), StartTime, 108) <= Convert(varchar(8), GETDATE(), 108)
                                        and CONVERT(VARCHAR(8), EndTime, 108) >= Convert(varchar(8), GETDATE(), 108)
                                        order by EndTime desc";

                    DataTable dtPeriod = _sqlRepository.GetDataTable(periodSql);
                    if (dtPeriod.Rows.Count <= 0)
                    {
                        throw new Exception("Please Select the period Manually!! There is no defined Period for the current time!!");
                    }
                    currPeriod = dtPeriod.Rows[0]["Value"].ToString();
                }
                else
                {
                    currPeriod = PeriodId;
                }

                //Filling the DsMaster DataSet for saving
                string _Id = "";
                string _OperationVariationId = "";
                string _MasterOperationId = "";
                double _tempQty = 0;
                int _secq = 0;


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data[i]["Id"] = "OP" + _Id;
                    dr["Id"] = "OP" + _Id;
                    dr["ProcessId"] = ProcessId;
                    dr["ShiftId"] = ShiftId;
                    dr["WorkCenterId"] = WorkCenter;
                    dr["ProductionOrderId"] = POId;
                    dr["OperationVariationId"] = data[i]["OperationId"];
                    dr["EmployeeId"] = data[i]["EmployeeId"];
                    dr["ResponsiblePersonId"] = ResponsiblePersonId;
                    dr["Date"] = Convert.ToDateTime(Date.ToString());
                    dr["Qty"] = data[i]["Qty"];
                    dr["PeriodId"] = currPeriod;
                    dr["Remarks"] = data[i]["Remarks"];
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                    _OperationVariationId=data[i]["OperationId"].ToString();
                    _MasterOperationId = data[i]["MasterOperationId"].ToString();
                    _secq = Convert.ToInt32(data[i]["Sequence"].ToString());
                    _tempQty+= clsStaticInfo.dbl(data[i]["Qty"].ToString());
                }
                #endregion Detail

                #region WIP
                DataSet dsSum;
                ConnectionManager.DAL.ConManager c = new ConnectionManager.DAL.ConManager("1");
                c.OpenDataSetThroughAdapter("select *  from  dbo.EmployeeOperationWip where ProductionOrderId = '" + POId + "' and ProcessId ='" + ProcessId + "' and OperationVariationId in ('"+ _OperationVariationId + "','"+ NxtOPVariationId + @"') order by Cast(OperationSequence AS int) asc", out dsSum, false, "1");
                string _SId = "";

                DataTable dtSum = dsSum.Tables[0];

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    dsSum.Tables[0].DefaultView.RowFilter = @"OperationVariationId='" + _OperationVariationId + "' and ProductionOrderId = '" + POId + "' ";
                    if (dsSum.Tables[0].DefaultView.Count > 0)
                    {
                        dsSum.Tables[0].DefaultView[0].Row.BeginEdit();
                        dsSum.Tables[0].DefaultView[0]["Qty"] = clsStaticInfo.dbl(dsSum.Tables[0].DefaultView[0]["Qty"].ToString()) + clsStaticInfo.dbl(dsMaster.Tables[0].Rows[i]["Qty"].ToString());
                        dsSum.Tables[0].DefaultView[0].Row.EndEdit();
                    }
                    dsSum.Tables[0].DefaultView.RowFilter = @"OperationVariationId='" + NxtOPVariationId + "' and ProductionOrderId = '" + POId + "' ";
                    if(dsSum.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dd = dsSum.Tables[0].NewRow();
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.EmployeeOperationWip", out _SId);
                        dd["Id"] = "OW" + _SId;
                        dd["ProductionOrderId"] = POId;
                        dd["OperationVariationId"] = NxtOPVariationId;
                        dd["OperationSequence"] = _secq+1;
                        dd["ProcessId"] = ProcessId;
                        dd["Qty"] = _tempQty;
                        dd["WIP"] = 0;
                        dd["AddedBy"] = identity.Name;
                        dd["AddedDate"] = System.DateTime.Now.ToString();
                        dd["AddedFromIP"] = identity.IPAddress;
                        dd["UpdatedBy"] = identity.Name;
                        dd["UpdatedDate"] = System.DateTime.Now.ToString();
                        dd["UpdatedFromIP"] = identity.IPAddress;
                        dsSum.Tables[0].Rows.Add(dd);
                    }
                }

               // For WIP
                for (int i = 0; i < dsSum.Tables[0].Rows.Count; i++)
                {
                    if (clsStaticInfo.dbl(dsSum.Tables[0].Rows[i]["OperationSequence"].ToString()) == 1)
                    {
                        dsSum.Tables[0].Rows[i].BeginEdit();
                        dsSum.Tables[0].Rows[i]["WIP"] = 0;
                        dsSum.Tables[0].Rows[i].EndEdit();
                    }
                    else
                    {
                        dsSum.Tables[0].Rows[i].BeginEdit();
                        if (dsSum.Tables[0].Rows[i]["OperationVariationId"].ToString()== _OperationVariationId)
                        {
                            dsSum.Tables[0].Rows[i]["WIP"] =  clsStaticInfo.dbl(dsSum.Tables[0].Rows[i]["WIP"].ToString())-_tempQty ;

                        }
                        else if (dsSum.Tables[0].Rows[i]["OperationVariationId"].ToString() == NxtOPVariationId)
                        {
                            dsSum.Tables[0].Rows[i]["WIP"] = _tempQty + clsStaticInfo.dbl(dsSum.Tables[0].Rows[i]["WIP"].ToString());

                        }
                        //if (clsStaticInfo.dbl(dsSum.Tables[0].Rows[i]["WIP"].ToString()) < 0)
                        //{
                        //    throw new Exception("WIP is Exceeding in Operation Sequence - " + dsSum.Tables[0].Rows[i]["OperationSequence"].ToString());
                        //}
                        dsSum.Tables[0].Rows[i].EndEdit();
                    }
                }

                #endregion Summary

                #region Employee Production Processing Half
                DataSet dsPlan = null;
                ConnectionManager.DAL.ConManager co = new ConnectionManager.DAL.ConManager("1");
                co.OpenDataSetThroughAdapter("select *  from  dbo.EmployeeWiseProductionProcessing where Date='" + Date + "' and ProductionOrderId='" + POId + "'", out dsPlan, false, "1");

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    dsPlan.Tables[0].DefaultView.RowFilter = @"EmployeeId = '" + dsMaster.Tables[0].Rows[i]["EmployeeId"] + "' and OperationVariationId='" + _OperationVariationId + "'";
                    if (dsPlan.Tables[0].DefaultView.Count > 0)
                    {
                        dsPlan.Tables[0].DefaultView[0].Row.BeginEdit();
                        dsPlan.Tables[0].DefaultView[0]["Qty"] = clsStaticInfo.dbl(dsPlan.Tables[0].DefaultView[0]["Qty"].ToString()) + _tempQty;
                        dsPlan.Tables[0].DefaultView[0].Row.EndEdit();
                    }
                    else
                    {
                        DataRow dr = dsPlan.Tables[0].NewRow();
                        dr["Id"] = dsMaster.Tables[0].Rows[i]["Id"].ToString() + i.ToString();
                        dr["Date"] = Convert.ToDateTime(Date);
                        dr["EmployeeId"] = dsMaster.Tables[0].Rows[i]["EmployeeId"];
                        dr["MasterOperationId"] = _MasterOperationId;
                        dr["OperationVariationId"] = _OperationVariationId;
                        dr["ProductionOrderId"] = POId;
                        dr["Qty"] = _tempQty;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsPlan.Tables[0].Rows.Add(dr);
                    }
                }



                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsSum, dsPlan);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private double getNum(object num)
        {
            if (bplib.clsWebLib.RetValidLen(num).ToString() != "")
            {
                if (clsStaticInfo.dbl(num) > 9999)
                {
                    throw new Exception("Period Value Greater than 9999");
                }
                else
                {
                    return clsStaticInfo.dbl(num);
                }
            }
            return 0;
        }

        public IEnumerable<object> getReportView(out List<string> Cols, string Date , string Wkc)
        {
            try
            {
                string wkcS = "1=1";
                if(Wkc != null)
                {
                    wkcS = "we.WorkCenterId = '"+Wkc+@"'";
                }

                var str = @"Select ov.Code as OperationCode , ov.UserName as OperationName, p.UserName as Process , wcm.UserName as WorkCenter ,we.ProductionOrderId , 
                            ei.EmployeeName , ei.EmployeeCode ,
                            format(we.Date , 'dd-MMM-yyyy') as Dates , we.PeriodId , pb.UserName as Periods , Sum(we.Qty) as Qty
                            --Select *
                            from dbo.OperationWiseEmployees we
                            left join hkp.ProductionBookingPeriod pb on pb.Id = we.PeriodId
                            left join hkp.Process p on p.Id = we.ProcessId
                            left join SCS.WorkCenterMaster wcm on wcm.Id = we.WorkCenterId
                            left join mst.OperationVariation ov on ov.Id = we.OperationVariationId
                            left join dbo.EmployeeInformation ei on ei.SystemId = we.EmployeeId
                            where we.Date = '" + Date + @"'  and "+wkcS+@"
                            group by ov.Code , ov.UserName , p.UserName , ei.EmployeeName , ei.EmployeeCode , we.Date , we.PeriodId  , pb.UserName, wcm.UserName ,we.ProductionOrderId 
                            order by Dates , ei.EmployeeName asc";

                DataTable dtAll = _sqlRepository.GetDataTable(str);

                //Getting the Periods
                List<string> ltPer = new List<string>();
                DataTable periods = dtAll.DefaultView.ToTable(true, "Periods");
                for (int i = 0; i < periods.Rows.Count; i++)
                {
                    ltPer.Add(periods.Rows[i]["Periods"].ToString());
                }

                ltPer = ltPer.OrderBy(k => k).ToList();

                DataTable dtNew = new DataTable();
                dtNew.Columns.Add("OperationCode", typeof(string));
                dtNew.Columns.Add("OperationName", typeof(string));
                dtNew.Columns.Add("Process", typeof(string));
                dtNew.Columns.Add("WorkCenter", typeof(string));
                dtNew.Columns.Add("ProductionOrderId", typeof(string));
                dtNew.Columns.Add("EmployeeCode", typeof(string));
                dtNew.Columns.Add("EmployeeName", typeof(string));
                dtNew.Columns.Add("Date", typeof(string));
                for (int i = 0; i < ltPer.Count; i++)
                {
                    dtNew.Columns.Add(ltPer[i], typeof(double));
                }

                //Filling the DataTable
                string opCode = "";
                string empCode = "";
                DateTime datess = Convert.ToDateTime("01-Jan-1990");
                DataRow dr = null;
                for (int i = 0; i < dtAll.Rows.Count; i++)
                {
                    if (dtAll.Rows[i]["OperationCode"].ToString() != opCode || dtAll.Rows[i]["EmployeeCode"].ToString() != empCode || Convert.ToDateTime(dtAll.Rows[i]["Dates"].ToString()) != datess)
                    {
                        dr = dtNew.NewRow();
                        dr["OperationCode"] = dtAll.Rows[i]["OperationCode"].ToString();
                        dr["OperationName"] = dtAll.Rows[i]["OperationName"].ToString();
                        dr["Process"] = dtAll.Rows[i]["Process"].ToString();
                        dr["WorkCenter"] = dtAll.Rows[i]["WorkCenter"].ToString();
                        dr["ProductionOrderId"] = dtAll.Rows[i]["ProductionOrderId"].ToString();
                        dr["EmployeeCode"] = dtAll.Rows[i]["EmployeeCode"].ToString();
                        dr["EmployeeName"] = dtAll.Rows[i]["EmployeeName"].ToString();
                        dr["Date"] = dtAll.Rows[i]["Dates"].ToString();

                        for (int j = 0; j < ltPer.Count; j++)
                        {
                            dr[ltPer[j]] = 0;
                        }

                        dtNew.Rows.Add(dr);
                    }

                    dr[dtAll.Rows[i]["Periods"].ToString()] = OTSBD.clsStaticInfo.dbl(dtAll.Rows[i]["Qty"].ToString());

                    opCode = dtAll.Rows[i]["OperationCode"].ToString();
                    empCode = dtAll.Rows[i]["EmployeeCode"].ToString();
                    datess = Convert.ToDateTime(dtAll.Rows[i]["Dates"].ToString());

                }

                Cols = ltPer;

                return Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtNew);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Get Employee DropDown

        #region Report Tab Download Function

        public DataTable getReportDownload(out List<string> DynCols, string Date , string Wkc)
        {
            try
            {
                string wkcS = "1=1";
                if (Wkc != null)
                {
                    wkcS = "we.WorkCenterId = '" + Wkc + @"'";
                }
                var str = @"Select ov.Code as OperationCode , ov.UserName as OperationName, p.UserName as Process , wcm.UserName as WorkCenter ,we.ProductionOrderId , 
                            ei.EmployeeName , ei.EmployeeCode ,
                            format(we.Date , 'dd-MMM-yyyy') as Dates , we.PeriodId , pb.UserName as Periods , Sum(we.Qty) as Qty
                            --Select *
                            from dbo.OperationWiseEmployees we
                            left join hkp.ProductionBookingPeriod pb on pb.Id = we.PeriodId
                            left join hkp.Process p on p.Id = we.ProcessId
                            left join SCS.WorkCenterMaster wcm on wcm.Id = we.WorkCenterId
                            left join mst.OperationVariation ov on ov.Id = we.OperationVariationId
                            left join dbo.EmployeeInformation ei on ei.SystemId = we.EmployeeId
                            where we.Date = '" + Date + @"'  and " + wkcS + @"
                            group by ov.Code , ov.UserName , p.UserName , ei.EmployeeName , ei.EmployeeCode , we.Date , we.PeriodId  , pb.UserName, wcm.UserName ,we.ProductionOrderId 
                            order by Dates , ei.EmployeeName asc";

                DataTable dtAll = _sqlRepository.GetDataTable(str);

                //Getting the Periods
                List<string> ltPer = new List<string>();
                DataTable periods = dtAll.DefaultView.ToTable(true, "Periods");
                for (int i = 0; i < periods.Rows.Count; i++)
                {
                    ltPer.Add(periods.Rows[i]["Periods"].ToString());
                }

                ltPer = ltPer.OrderBy(k => k).ToList();

                DataTable dtNew = new DataTable();
                dtNew.Columns.Add("OperationCode", typeof(string));
                dtNew.Columns.Add("OperationName", typeof(string));
                dtNew.Columns.Add("Process", typeof(string));
                dtNew.Columns.Add("WorkCenter", typeof(string));
                dtNew.Columns.Add("ProductionOrderId", typeof(string));
                dtNew.Columns.Add("EmployeeCode", typeof(string));
                dtNew.Columns.Add("EmployeeName", typeof(string));
                dtNew.Columns.Add("Qty", typeof(string));
                dtNew.Columns.Add("Dates", typeof(string));
                for (int i = 0; i < ltPer.Count; i++)
                {
                    dtNew.Columns.Add(ltPer[i], typeof(double));
                }

                //Filling the DataTable
                string opCode = "";
                string empCode = "";
                DateTime datess = Convert.ToDateTime("01-Jan-1990");
                DataRow dr = null;
                for (int i = 0; i < dtAll.Rows.Count; i++)
                {
                    if (dtAll.Rows[i]["OperationCode"].ToString() != opCode || dtAll.Rows[i]["EmployeeCode"].ToString() != empCode || Convert.ToDateTime(dtAll.Rows[i]["Dates"].ToString()) != datess)
                    {

                        dr = dtNew.NewRow();
                        dr["OperationCode"] = dtAll.Rows[i]["OperationCode"].ToString();
                        dr["OperationName"] = dtAll.Rows[i]["OperationName"].ToString();
                        dr["Process"] = dtAll.Rows[i]["Process"].ToString();
                        dr["WorkCenter"] = dtAll.Rows[i]["WorkCenter"].ToString();
                        dr["ProductionOrderId"] = dtAll.Rows[i]["ProductionOrderId"].ToString();
                        dr["EmployeeCode"] = dtAll.Rows[i]["EmployeeCode"].ToString();
                        dr["EmployeeName"] = dtAll.Rows[i]["EmployeeName"].ToString();
                        dr["Qty"] = dtAll.Rows[i]["Qty"].ToString();
                        dr["Dates"] = dtAll.Rows[i]["Dates"].ToString();

                        for (int j = 0; j < ltPer.Count; j++)
                        {
                            dr[ltPer[j]] = 0;
                        }

                        dtNew.Rows.Add(dr);
                    }

                    dr[dtAll.Rows[i]["Periods"].ToString()] = OTSBD.clsStaticInfo.dbl(dtAll.Rows[i]["Qty"].ToString());

                    opCode = dtAll.Rows[i]["OperationCode"].ToString();
                    empCode = dtAll.Rows[i]["EmployeeCode"].ToString();
                    datess = Convert.ToDateTime(dtAll.Rows[i]["Dates"].ToString());

                }

                DynCols = ltPer;

                return dtNew;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #endregion Report Tab Download Function
        public void processAll(string Date)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                #region 1. Getting The Current Parial Data

                DataSet dsMasterHalf = null;
                ConnectionManager.DAL.ConManager co = new ConnectionManager.DAL.ConManager("1");
                co.OpenDataSetThroughAdapter("select *  from  dbo.EmployeeWiseProductionProcessing where Date='" + Date + "'  order by EmployeeId asc, Qty desc", out dsMasterHalf, false, "1");

                //DataTable dsMasterHalf.Tables[0] = dsMasterHalf.Tables[0];

                #endregion

                #region 2. Getting the TotalSPT Data

                //var str1 = @"Select pt.ProductionOrderId , ptd.OperationVariationId ,skc.Id as SkillCatId,  skc.UserName , ptd.TotalSPT as SkillCat
                //            from trn.ProductionBulletinTemplateDetail ptd
                //            left join trn.ProductionBulletinTemplateMaster pm on pm.Id = ptd.ProductionBulletinTemplateMasterId
                //            left join trn.ProductionBulletinTemplate pt on pt.Id = pm.ProductionBulletinTemplateId
                //            left join hkp.Skill sk on sk.Id = ptd.SkillId
                //            left join hkp.SkillCategory skc on skc.Id = sk.SkillCategoryId";

                var str = @"Select distinct ptd.OperationVariationId , pm.ProcessId , ptd.TotalSPT , skc.Id as SkillCatId,  skc.UserName ,pt.ProductionOrderId  from trn.ProductionBulletinTemplateDetail ptd
                                left join  trn.ProductionBulletinTemplateMaster pm on pm.Id  = ptd.ProductionBulletinTemplateMasterId
                                left join  trn.ProductionBulletinTemplate pt on pt.Id = pm.ProductionBulletinTemplateId
								left join hkp.Skill sk on sk.Id = ptd.operationMasterId
                                left join hkp.SkillCategory skc on skc.Id = sk.SkillCategoryId";



                DataTable dtTotalSpt = _sqlRepository.GetDataTable(str);

                #endregion

                #region 3. Filling in the Sequences

                if (dsMasterHalf.Tables[0].Rows.Count <= 0)
                {
                    throw new Exception("There is no data to Process!!");
                }
                else
                {
                    string empId = "";
                    int k = 0;
                    for (int i = 0; i < dsMasterHalf.Tables[0].Rows.Count; i++)
                    {
                        k++;
                        if (dsMasterHalf.Tables[0].Rows[i]["EmployeeId"].ToString() == empId)
                        {
                            dsMasterHalf.Tables[0].Rows[i]["Sequence"] = k;
                        }
                        else
                        {
                            k = 1;

                            dsMasterHalf.Tables[0].Rows[i]["Sequence"] = k;
                        }
                        empId = dsMasterHalf.Tables[0].Rows[i]["EmployeeId"].ToString();
                    }

                }

                #endregion

                #region 4. Calculation and Filling of the MasterHalf DataTable TOTALSPT

                Dictionary<string, string> dicSkillCat = new Dictionary<string, string>(); // List<Dictionary<string, string>>dicSkillCat = new List<Dictionary<string, string>>();

                for (int i = 0; i < dsMasterHalf.Tables[0].Rows.Count; i++)
                {
                    dtTotalSpt.DefaultView.RowFilter = @"ProductionOrderId='" + dsMasterHalf.Tables[0].Rows[i]["ProductionOrderId"].ToString() + "' and OperationVariationId='" + dsMasterHalf.Tables[0].Rows[i]["OperationVariationId"].ToString() + "'";
                    dicSkillCat.Add(dsMasterHalf.Tables[0].Rows[i]["Id"].ToString(), dtTotalSpt.DefaultView[0]["SkillCatId"].ToString());
                    dsMasterHalf.Tables[0].Rows[i].BeginEdit();
                    dsMasterHalf.Tables[0].Rows[i]["StandardProcessTime"] = clsStaticInfo.dbl(dtTotalSpt.DefaultView[0]["TotalSPT"].ToString());
                    dsMasterHalf.Tables[0].Rows[i]["TotalSPT"] = clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["Qty"].ToString()) * clsStaticInfo.dbl(dtTotalSpt.DefaultView[0]["TotalSPT"].ToString());
                    dsMasterHalf.Tables[0].Rows[i].EndEdit();
                }

                #endregion

                #region 5. Calculation of Allowances

                var strDate = @"Select top 1 ph.Id , ph.EffectiveDate from dbo.ProducedMinAllowanceHeader ph where EffectiveDate <= '" + Date + @"' order by EffectiveDate desc";
                DataTable dtDates = _sqlRepository.GetDataTable(strDate);

                string whereAllow = "";
                if (dtDates.Rows.Count <= 0)
                {
                    whereAllow = "where 1 = 2";
                }
                else
                {
                    whereAllow = " where ph.Id = '" + dtDates.Rows[0]["Id"].ToString() + "'";
                }

                var str2 = @"Select epp.Id, epp.EmployeeId,po.EntityId , owe.ShiftId ,owe.ProcessId from dbo.EmployeeWiseProductionProcessing epp
                            left join trn.ProductionOrder po on po.Id = epp.ProductionOrderId
                            left join ( Select distinct owe.Date ,owe.ShiftId, owe.EmployeeId , owe.OperationVariationId , owe.ProcessId from dbo.OperationWiseEmployees owe
                             where Date='" + Date + @"') owe on owe.EmployeeId = epp.EmployeeId and owe.Date = epp.Date and owe.OperationVariationId = epp.OperationVariationId
                            where epp.Date='" + Date + @"'";

                DataTable dtMainDict = _sqlRepository.GetDataTable(str2);

                var str3 = @"Select ph.Id ,ph.EffectiveDate, pp.ProcessId , pe.EntityId , pc.SkillAllowance , pc.AdditionOperationAllowance , pc.OperationSequence , pc.SkillCategoryId
                            from dbo.ProducedMinAllowanceHeader ph
                            left join dbo.ProducedMinAllowanceEntity pe on pe.HeaderId = ph.Id
                            left join dbo.ProducedMinAllowanceProcess pp on pp.HeaderId = ph.Id
                            left join dbo.ProducedMinAllowanceChild pc on pc.HeaderId = ph.Id
                             " + whereAllow;

                DataTable dtAllowance = _sqlRepository.GetDataTable(str3);


                //Special Allowance
                var str4 = @"Select * from mst.Operationvariation where isSpecialOperation = 1";
                DataTable dtSpecialOperations = _sqlRepository.GetDataTable(str4);

                var spOpTable = @"Select sp.*,
                                    (Select top 1 format(sr.EffectiveDate ,'dd-MMM-yyyy') as Effective
                                    from dbo.SpecialOperationRateDates sr
                                    where sr.HeaderId =sp.Id
                                    and sr.EffectiveDate <= '" + Date + @"'
                                    order by EffectiveDate desc) as EffectiveDate
                                    from dbo.SpecialOperationRate sp";
                DataTable dtSpecialAllowanceRate = _sqlRepository.GetDataTable(spOpTable);

                for (int i = 0; i < dsMasterHalf.Tables[0].Rows.Count; i++)
                {

                    dtMainDict.DefaultView.RowFilter = @"Id = '" + dsMasterHalf.Tables[0].Rows[i]["Id"].ToString() + "'";
                    double dtSkillVal = 0.0;
                    double dtAddOpVal = 0.0;
                    double dtSpOpVal = 0.0;
                    string SkillCatId = dicSkillCat[dsMasterHalf.Tables[0].Rows[i]["Id"].ToString()].ToString();
                    string EntityId = dtMainDict.DefaultView[0]["EntityId"].ToString();
                    string ProcessId = dtMainDict.DefaultView[0]["ProcessId"].ToString();
                    string OpSeq = dsMasterHalf.Tables[0].Rows[i]["Sequence"].ToString();

                    //Getting the Operation
                    dtSpecialOperations.DefaultView.RowFilter = @"Id='" + dsMasterHalf.Tables[0].Rows[i]["OperationVariationId"] + "'";
                    if (dtSpecialOperations.DefaultView.Count > 0)
                    {
                        dtSpecialAllowanceRate.DefaultView.RowFilter = @"EntityId='" + EntityId + "' and ProcessId='" + ProcessId + "'";
                        if (dtSpecialAllowanceRate.DefaultView.Count > 0)
                        {
                            dtSpOpVal = clsStaticInfo.dbl(dtSpecialAllowanceRate.DefaultView[0]["AllowancePercentage"].ToString());
                        }
                    }



                    dtAllowance.DefaultView.RowFilter = @"EntityId='" + EntityId + "' and ProcessId ='" + ProcessId + @"' 
                                                        and SkillCategoryId='" + SkillCatId + "' and OperationSequence='" + OpSeq + "'";
                    if (dtAllowance.DefaultView.Count > 0)
                    {
                        dtSkillVal = clsStaticInfo.dbl(dtAllowance.DefaultView[0]["SkillAllowance"].ToString());
                        dtAddOpVal = clsStaticInfo.dbl(dtAllowance.DefaultView[0]["AdditionOperationAllowance"].ToString());
                    }

                    dsMasterHalf.Tables[0].Rows[i].BeginEdit();
                    //Special Operation Addition

                    dsMasterHalf.Tables[0].Rows[i]["SpecialOperationAllowanceRate"] = dtSpOpVal / 100;


                    dsMasterHalf.Tables[0].Rows[i]["SkillAllowanceRate"] = dtSkillVal / 100;
                    dsMasterHalf.Tables[0].Rows[i]["AdditionalOperationAllowanceRate"] = dtAddOpVal / 100;

                    double spAll = clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["SpecialOperationAllowanceRate"].ToString()) * clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["TotalSPT"].ToString());
                    double skAll = clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["SkillAllowanceRate"].ToString()) * clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["TotalSPT"].ToString());
                    double addAll = clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["AdditionalOperationAllowanceRate"].ToString()) * clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["TotalSPT"].ToString());
                    dsMasterHalf.Tables[0].Rows[i]["SpecialOperationAllowance"] = spAll;
                    dsMasterHalf.Tables[0].Rows[i]["SkillAllowance"] = skAll;
                    dsMasterHalf.Tables[0].Rows[i]["AdditionalOperationAllowance"] = addAll;


                    dsMasterHalf.Tables[0].Rows[i]["AllotedProducedMin"] = clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["TotalSPT"].ToString()) + skAll + addAll + spAll;
                    dsMasterHalf.Tables[0].Rows[i].EndEdit();
                }

                #endregion

                #region 6. Getting the Rates Data

                DataSet dsRatesHealf = null;
                ConnectionManager.DAL.ConManager connec = new ConnectionManager.DAL.ConManager("1");
                connec.OpenDataSetThroughAdapter("Select * from dbo.EmployeeEfficiencyProcess where Date='" + Date + "'  order by EmployeeId asc", out dsRatesHealf, false, "1");

                int count = dsRatesHealf.Tables[0].Rows.Count;
                //Overwrite The dsRatesHealf Table
                while (dsRatesHealf.Tables[0].DefaultView.Count > 0)
                {
                    dsRatesHealf.Tables[0].DefaultView[0].Delete();
                }

                //DataTable dsRatesHealf.Tables[0] = dsRatesHealf.Tables[0];
                var strInDate = @"Select top 1 Id , EffectiveDate from dbo.IncentiveRateSetupHeader where EffectiveDate <= '" + Date + "' order by EffectiveDate desc";
                DataTable dtInDates = _sqlRepository.GetDataTable(strInDate);
                string whereRates = "";
                if (dtInDates.Rows.Count <= 0)
                {
                    whereRates = "where 1 = 2";
                }
                else
                {
                    whereRates = "where ih.Id = '" + dtInDates.Rows[0]["Id"].ToString() + "'";
                }


                var str5 = @"Select ih.Id , ih.EffectiveDate , ie.EntityId , ips.ProcessId , ics.Effeciency, ics.EffeciencyRate from dbo.IncentiveRateSetupHeader ih
                            left join dbo.IncentiveRateSetupEntity ie on ie.HeaderId = ih.Id
                            left join dbo.IncentiveRateSetupProcess ips on ips.HeaderId = ih.Id
                            left join dbo.IncentiveRateSetupChild ics on ics.HeaderId = ih.Id
                            " + whereRates;

                DataTable dtRatesTable = _sqlRepository.GetDataTable(str5);


                var str6 = @"Select EmpSystemId , isnull(Duration,0) as Durations, ShiftHoursWithoutOT ,(ShiftHoursWithoutOT + isnull(AdditionalOT,0) + ISNULL(StandardOT,0)) as Duration  , WorkDate from dbo.AttdnProcessData where WorkDate = '" + Date + @"'";
                DataTable dtApd = _sqlRepository.GetDataTable(str6);

                #endregion

                #region 7. Calculation Of the Duration And Efficiency
                //Getting the EmployeeTimeOut Applicables
                var strEmpTApp = @"Select EntityId , ProcessId , IsApplicable from dbo.EmployeeTimeOutApplicable";
                DataTable dtEmpTOApp = _sqlRepository.GetDataTable(strEmpTApp);

                //Getting the Employee Time Out Durations
                var strEmpTimeOut = @"Select EmployeeId , WorkDate , Sum(Duration) as Duration from dbo.EmployeesTimeOut where WorkDate = '" + Date + @"' group by EmployeeId , WorkDate";
                DataTable dtEmpTimeOut = _sqlRepository.GetDataTable(strEmpTimeOut);

                for (int i = 0; i < dsMasterHalf.Tables[0].Rows.Count; i++)
                {
                    dtApd.DefaultView.RowFilter = @"EmpSystemId ='" + dsMasterHalf.Tables[0].Rows[i]["EmployeeId"].ToString() + "'";
                    dtEmpTimeOut.DefaultView.RowFilter = @"EmployeeId='" + dsMasterHalf.Tables[0].Rows[i]["EmployeeId"].ToString() + "'";
                    dtMainDict.DefaultView.RowFilter = @"EmployeeId='" + dsMasterHalf.Tables[0].Rows[i]["EmployeeId"].ToString() + "'";

                    dtEmpTOApp.DefaultView.RowFilter = @"EntityId='" + dtMainDict.DefaultView[0]["EntityId"].ToString() + "' and ProcessId='" + dtMainDict.DefaultView[0]["ProcessId"].ToString() + "'";

                    double TODur = 0.0;
                    if (dtEmpTOApp.DefaultView.Count > 0)
                    {
                        if ((bool)dtEmpTOApp.DefaultView[0]["IsApplicable"] == true)
                        {
                            if (dtEmpTimeOut.DefaultView.Count > 0)
                            {
                                TODur = clsStaticInfo.dbl(dtEmpTimeOut.DefaultView[0]["Duration"].ToString());
                            }
                        }
                    }


                    double Dur = 1.0;
                    double Durs = 0.0;
                    double NetDur = 1.0;
                    double NetDurs = 0.0;
                    if (dtApd.DefaultView.Count > 0)
                    {
                        if(clsStaticInfo.dbl(dtApd.DefaultView[0]["Duration"].ToString()) > 0)
                        {
                            Dur = clsStaticInfo.dbl(dtApd.DefaultView[0]["Duration"].ToString());
                            Durs = clsStaticInfo.dbl(dtApd.DefaultView[0]["Duration"].ToString());

                            NetDur = Durs - TODur;
                            NetDurs = Durs - TODur;
                        }
                        
                    }



                    dsRatesHealf.Tables[0].DefaultView.RowFilter = @"EmployeeId='" + dsMasterHalf.Tables[0].Rows[i]["EmployeeId"].ToString() + "'";
                    if (dsRatesHealf.Tables[0].DefaultView.Count > 0)
                    {
                        dsRatesHealf.Tables[0].DefaultView[0].Row.BeginEdit();
                        dsRatesHealf.Tables[0].DefaultView[0]["TotalSPT"] = clsStaticInfo.dbl(dsRatesHealf.Tables[0].DefaultView[0]["TotalSPT"].ToString()) + clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["TotalSPT"].ToString());
                        dsRatesHealf.Tables[0].DefaultView[0]["AllotedProducedMin"] = clsStaticInfo.dbl(dsRatesHealf.Tables[0].DefaultView[0]["AllotedProducedMin"].ToString()) + clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["AllotedProducedMin"].ToString());
                        dsRatesHealf.Tables[0].DefaultView[0]["WorkDuration"] = Durs;
                        dsRatesHealf.Tables[0].DefaultView[0]["EmployeesTimeOutDuration"] = TODur;
                        dsRatesHealf.Tables[0].DefaultView[0]["NetDuration"] = Durs - TODur;
                        dsRatesHealf.Tables[0].DefaultView[0]["NetEfficiency"] = clsStaticInfo.dbl(String.Format("{0:0.00}", (clsStaticInfo.dbl(dsRatesHealf.Tables[0].DefaultView[0]["TotalSPT"].ToString()) / NetDur) * 100));
                        dsRatesHealf.Tables[0].DefaultView[0]["GrossEfficiency"] = (clsStaticInfo.dbl(dsRatesHealf.Tables[0].DefaultView[0]["AllotedProducedMin"].ToString()) / NetDur) * 100;
                        dsRatesHealf.Tables[0].DefaultView[0].Row.EndEdit();
                    }
                    else
                    {
                        DataRow dr = dsRatesHealf.Tables[0].NewRow();
                        dr["Id"] = dsMasterHalf.Tables[0].Rows[i]["Id"].ToString();
                        dr["EmployeeId"] = dsMasterHalf.Tables[0].Rows[i]["EmployeeId"].ToString();
                        dr["TotalSPT"] = clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["TotalSPT"].ToString());
                        dr["AllotedProducedMin"] = clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["AllotedProducedMin"].ToString());
                        dr["WorkDuration"] = Durs;
                        dr["EmployeesTimeOutDuration"] = TODur;
                        dr["NetDuration"] = NetDurs;
                        dr["NetEfficiency"] = clsStaticInfo.dbl(String.Format("{0:0.00}",(clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["TotalSPT"].ToString()) / NetDur) * 100));
                        dr["GrossEfficiency"] = (clsStaticInfo.dbl(dsMasterHalf.Tables[0].Rows[i]["AllotedProducedMin"].ToString()) / NetDur) * 100;
                        dr["Date"] = Convert.ToDateTime(Date);
                        dr["Rate"] = 0;
                        dr["Amount"] = 0;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsRatesHealf.Tables[0].Rows.Add(dr);
                    }
                }


                #endregion

                #region 8. Calculation Of Efficiency Rate And Amount

                //ShiftsTable
                var strShift = @"Select SystemID , ShiftDuration from dbo.ShiftDefination ";
                DataTable dtShifts = _sqlRepository.GetDataTable(strShift);

                for (int i = count; i < dsRatesHealf.Tables[0].Rows.Count; i++)
                {


                    dtMainDict.DefaultView.RowFilter = @"Id = '" + dsRatesHealf.Tables[0].Rows[i]["Id"].ToString() + "'";
                    string EntityId = dtMainDict.DefaultView[0]["EntityId"].ToString();
                    string ProcessId = dtMainDict.DefaultView[0]["ProcessId"].ToString();
                    string ShiftId = dtMainDict.DefaultView[0]["ShiftId"].ToString();

                    dtRatesTable.DefaultView.RowFilter = @"EntityId='" + EntityId + "' and ProcessId='" + ProcessId + "'";

                    dtRatesTable.DefaultView.Sort = "Effeciency";

                    DataTable dtTemp = dtRatesTable.DefaultView.ToTable();
                    double Rate = 0.0;
                    for (int j = dtTemp.Rows.Count - 1; j >= 0; j--)
                    {
                        if (clsStaticInfo.dbl(dsRatesHealf.Tables[0].Rows[i]["GrossEfficiency"].ToString()) >= clsStaticInfo.dbl(dtTemp.Rows[j]["Effeciency"].ToString()))
                        {
                            Rate = clsStaticInfo.dbl(dtTemp.Rows[j]["EffeciencyRate"].ToString());

                            break;
                        }
                    }

                    //ShiftDuration and Calculation of Final Amount
                    dtShifts.DefaultView.RowFilter = @"SystemID='" + ShiftId + "'";



                    dsRatesHealf.Tables[0].Rows[i].BeginEdit();
                    dsRatesHealf.Tables[0].Rows[i]["Rate"] = Rate;

                    dsRatesHealf.Tables[0].Rows[i]["Amount"] = clsStaticInfo.dbl(dsRatesHealf.Tables[0].Rows[i]["GrossEfficiency"].ToString()) * Rate;
                    dsRatesHealf.Tables[0].Rows[i]["DurationPercentage"] = (clsStaticInfo.dbl(dsRatesHealf.Tables[0].DefaultView[0]["NetDuration"].ToString()) / clsStaticInfo.dbl(dtShifts.DefaultView[0]["ShiftDuration"].ToString())) * 100;
                    dsRatesHealf.Tables[0].Rows[i]["FinalAmount"] = (clsStaticInfo.dbl(dsRatesHealf.Tables[0].Rows[i]["DurationPercentage"].ToString()) / 100) * clsStaticInfo.dbl(dsRatesHealf.Tables[0].Rows[i]["Amount"].ToString());

                    dsRatesHealf.Tables[0].Rows[i].EndEdit();

                }


                #endregion


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMasterHalf, dsRatesHealf);


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region getProcessDownload
        public DataTable getProcessDownload(string FromDate, string ToDate)
        {
            try
            {
                //var str = @"Select format(eep.Date,'dd-MMM-yyyy') as Date, eep.EmployeeId, ei.EmployeeCode, ei.EmployeeName , eep.WorkDuration , eep.EmployeesTimeOutDuration , eep.NetDuration , eep.TotalSPT as ProducedMin,
                //            eep.AllotedProducedMin as GrossProducedMin , eep.NetEfficiency , eep.GrossEfficiency, eep.Rate , eep.Amount , eep.FinalAmount
                //            from dbo.EmployeeEfficiencyProcess eep
                //            left join dbo.EmployeeInformation ei on ei.SystemId = eep.EmployeeId
                //            where Date = '" + Date+@"'
                //             ";

                var str = @"
                            Select * from 
                            (
                            Select ei.SystemId , ei.EmployeeCode , ei.EmployeeName ,apd.Duration , format(apd.WorkDate,'dd-MMM-yyyy') as WorksDate
                            from dbo.AttdnProcessData apd
                            left join dbo.EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                            right join dbo.EmployeeOperationBudget eob on eob.BudgetId = ei.BudgetCode
                            where apd.WorkDate between '" + FromDate + @"' and '"+ToDate+ @"' and ei.EmployeeStatus='Active'
                            ) as dd
                            left join 
                            (Select format(eep.Date,'dd-MMM-yyyy') as Date, eep.EmployeeId, ei.EmployeeCode as EmpCodes, ei.EmployeeName as EmpNames , eep.WorkDuration , (Case when eep.EmployeesTimeOutDuration > 0 then eep.EmployeesTimeOutDuration else null end) as EmployeesTimeOutDuration , eep.NetDuration , eep.TotalSPT as ProducedMin,
                            eep.AllotedProducedMin as GrossProducedMin , eep.NetEfficiency , eep.GrossEfficiency, (Case when eep.Rate > 0 then eep.Rate else null end) Rate , (Case when eep.Amount > 0 then eep.Amount else null end) Amount , (Case when eep.FinalAmount > 0 then eep.FinalAmount else null end) FinalAmount
                            
                                                        from dbo.EmployeeEfficiencyProcess eep
                                                        left join dbo.EmployeeInformation ei on ei.SystemId = eep.EmployeeId
                                                        where Date between '" + FromDate + @"' and '" + ToDate + @"'
                            ) as aa on aa.EmployeeId = dd.SystemId and aa.Date = dd.WorksDate
                            order by  CAST( dd.WorksDate as datetime) asc , aa.EmployeeId desc ";

                #region TestCode
                // var str = @"select format(ewpp.Date, 'dd-MMM-yyyy') as Date, ei.EmployeeCode, ei.EmployeeName, 
                //             ov.UserName, ov.Code, ewpp.ProductionOrderId as poId, ewpp.Sequence, ewpp.Qty, ewpp.StandardProcessTime, ewpp.TotalSPT,
                //             ewpp.SkillAllowance, ewpp.AdditionalOperationAllowance, ewpp.AllotedProducedMin
                //             from dbo.EmployeeWiseProductionProcessing ewpp
                //             left join dbo.EmployeeInformation ei on ei.SystemId = ewpp.EmployeeId
                //             left join mst.OperationVariation ov on ov.OperationMasterId = ewpp.MasterOperationId";

                // DataTable dtAll = _sqlRepository.GetDataTable(str);

                // //Getting the Periods
                // List<string> ltPer = new List<string>();
                // DataTable periods = dtAll.DefaultView.ToTable(true, "poId");
                // for (int i = 0; i < periods.Rows.Count; i++)
                // {
                //     ltPer.Add(periods.Rows[i]["poId"].ToString());
                // }

                // ltPer = ltPer.OrderBy(k => k).ToList();

                // DataTable dtNew = new DataTable();
                // dtNew.Columns.Add("Date", typeof(string));
                // dtNew.Columns.Add("EmployeeCode", typeof(string));
                // dtNew.Columns.Add("EmployeeName", typeof(string));
                // dtNew.Columns.Add("UserName", typeof(string));
                // dtNew.Columns.Add("Code", typeof(string));
                // dtNew.Columns.Add("poId", typeof(string));
                // dtNew.Columns.Add("Qty", typeof(string));
                // dtNew.Columns.Add("StandardProcessTime", typeof(string));
                // dtNew.Columns.Add("TotalSPT", typeof(string));
                // dtNew.Columns.Add("SkillAllowance", typeof(string));
                // dtNew.Columns.Add("AdditionalOperationAllowance", typeof(string));
                // dtNew.Columns.Add("AllotedProducedMin", typeof(string));


                // for (int i = 0; i < ltPer.Count; i++)
                // {
                //     dtNew.Columns.Add(ltPer[i], typeof(double));
                // }

                // //Filling the DataTable
                //// string opCode = "";
                // string empCode = "";
                // DateTime datess = Convert.ToDateTime("01-Jan-1990");
                // DataRow dr = null;
                // for (int i = 0; i < dtAll.Rows.Count; i++)
                // {
                //     if (dtAll.Rows[i]["EmployeeCode"].ToString() != empCode || Convert.ToDateTime(dtAll.Rows[i]["Date"].ToString()) != datess)
                //     {

                //         dr = dtNew.NewRow();
                //         dr["Date"] = dtAll.Rows[i]["Date"].ToString();
                //         dr["EmployeeCode"] = dtAll.Rows[i]["EmployeeCode"].ToString();
                //         dr["EmployeeName"] = dtAll.Rows[i]["EmployeeName"].ToString();
                //         dr["UserName"] = dtAll.Rows[i]["UserName"].ToString();

                //         dr["Code"] = dtAll.Rows[i]["Code"].ToString();
                //         dr["poId"] = dtAll.Rows[i]["poId"].ToString();
                //         //dr["Sequence"] = dtAll.Rows[i]["Sequence"].ToString();
                //         dr["Qty"] = dtAll.Rows[i]["Qty"].ToString();
                //         dr["StandardProcessTime"] = dtAll.Rows[i]["StandardProcessTime"].ToString();
                //         dr["TotalSPT"] = dtAll.Rows[i]["TotalSPT"].ToString();
                //         dr["SkillAllowance"] = dtAll.Rows[i]["SkillAllowance"].ToString();
                //         dr["AdditionalOperationAllowance"] = dtAll.Rows[i]["AdditionalOperationAllowance"].ToString();
                //         dr["AllotedProducedMin"] = dtAll.Rows[i]["AllotedProducedMin"].ToString();





                //         for (int j = 0; j < ltPer.Count; j++)
                //         {
                //             dr[ltPer[j]] = 0;
                //         }

                //         dtNew.Rows.Add(dr);
                //     }

                //     //dr[dtAll.Rows[i]["Periods"].ToString()] = OTSBD.clsStaticInfo.dbl(dtAll.Rows[i]["Qty"].ToString());

                //    // opCode = dtAll.Rows[i]["OperationCode"].ToString();
                //     empCode = dtAll.Rows[i]["EmployeeCode"].ToString();
                //     datess = Convert.ToDateTime(dtAll.Rows[i]["Date"].ToString());

                // }

                // DynCols = ltPer;
                #endregion

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion getProcessDownload

        #region getEmployeeWorkDurationReport
        public DataTable getEmployeeWorkDurationReport(string FromDate, string ToDate)
        {
            try
            {
                //var str = @"Select format(owe.Date,'dd-MMM-yyyy') as WorkDate ,owe.ShiftId ,
                //            (Select top 1 wc.UserName from dbo.OperationWiseEmployees owes
                //            left join scs.WorkCenterMaster wc on wc.Id = owes.WorkCenterId
                //            where owes.EmployeeId = owe.EmployeeId and owes.Date = owe.Date
                //            order by owe.Qty desc) as WorkCenter
                //            ,sd.UserName as ShiftName , owe.ProductionOrderId , owe.EmployeeId , ei.EmployeeCode, ei.EmployeeName , owe.OperationVariationId , ov.Code as OperationCode , owe.Qty , skc.UserName as SkillCategory , epp.TotalSPT , epp.SkillAllowance, epp.AdditionalOperationAllowance, epp.SpecialOperationAllowance ,epp.AllotedProducedMin , owe.Remarks ,
                //            (Select top 1 mo.BuyerReferenceNo
                //            from trn.ProductionOrderDetail pod 
                //            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                //            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                //            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                //            left join mst.MaterialMasterArticle mma on mma.ID = moi.ArticleId
                //            where pod.ProductionOrderId = owe.ProductionOrderId) as BuyerRef,
                //            (Select top 1 mo.OwnReferenceNo
                //            from trn.ProductionOrderDetail pod 
                //            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                //            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                //            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                //            left join mst.MaterialMasterArticle mma on mma.ID = moi.ArticleId
                //            where pod.ProductionOrderId = owe.ProductionOrderId) as OwnRef,
                //            (Select top 1 mma.Code 
                //            from trn.ProductionOrderDetail pod 
                //            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                //            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                //            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                //            left join mst.MaterialMasterArticle mma on mma.ID = moi.ArticleId
                //            where pod.ProductionOrderId = owe.ProductionOrderId) as ArticleCode
                //            from dbo.OperationWiseEmployees owe
                //            left join dbo.EmployeeWiseProductionProcessing epp on epp.Date = owe.Date and epp.EmployeeId = owe.EmployeeId
                //            left join mst.OperationVariation ov on ov.ID = owe.OperationVariationId
                //            left join hkp.Skill sk on sk.ID = ov.SkillId
                //            left join hkp.SkillCategory skc on skc.ID = sk.SkillCategoryId
                //            left join dbo.EmployeeInformation ei on ei.SystemId = owe.EmployeeId
                //            left join dbo.ShiftDefination sd on sd.SystemID = owe.ShiftId
                //            where owe.Date = '" + Date+@"'
                //            "; 

                var str = @"
                            Select * from 
                            (
                            Select ei.SystemId , ei.EmployeeCode , ei.EmployeeName ,apd.Duration, format(apd.WorkDate,'dd-MMM-yyyy') as WorksDate
                            from dbo.AttdnProcessData apd
                            left join dbo.EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                            right join dbo.EmployeeOperationBudget eob on eob.BudgetId = ei.BudgetCode
                            where apd.WorkDate between '" + FromDate + @"' and '" + ToDate + @"' and ei.EmployeeStatus='Active'
                            ) as dd
                            left join 
                            (Select format(owe.Date,'dd-MMM-yyyy') as WorkDate ,owe.ShiftId ,
                            (Select top 1 wc.UserName from dbo.OperationWiseEmployees owes
                            left join scs.WorkCenterMaster wc on wc.Id = owes.WorkCenterId
                            where owes.EmployeeId = owe.EmployeeId and owes.Date = owe.Date
                            order by owes.Qty desc) as WorkCenter
                            ,sd.UserName as ShiftName , owe.ProductionOrderId , epp.StandardProcessTime , ov.UserName as OperationName ,owe.EmployeeId , ei.EmployeeCode as EmpCode, ei.EmployeeName as EmpName, owe.OperationVariationId , ov.Code as OperationCode , owe.Qty , skc.UserName as SkillCategory , epp.TotalSPT , (Case when epp.SkillAllowance > 0 then epp.SkillAllowance else null end) as SkillAllowance, (Case when epp.AdditionalOperationAllowance > 0 then epp.AdditionalOperationAllowance else null end) AdditionalOperationAllowance, (Case when epp.SpecialOperationAllowance > 0 then epp.SpecialOperationAllowance else null end) SpecialOperationAllowance ,epp.AllotedProducedMin ,(Select top 1 Remarks from dbo.OperationWiseEmployees owes
							where owes.EmployeeId = owe.EmployeeId and owes.Date = owe.Date
							order by owes.Qty desc)Remarks ,
                            (Select top 1 mo.BuyerReferenceNo
                            from trn.ProductionOrderDetail pod 
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            left join mst.MaterialMasterArticle mma on mma.ID = moi.ArticleId
                            where pod.ProductionOrderId = owe.ProductionOrderId) as BuyerRef,
                            (Select top 1 mo.OwnReferenceNo
                            from trn.ProductionOrderDetail pod 
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            left join mst.MaterialMasterArticle mma on mma.ID = moi.ArticleId
                            where pod.ProductionOrderId = owe.ProductionOrderId) as OwnRef,
                            (Select top 1 mma.Code 
                            from trn.ProductionOrderDetail pod 
                            left join trn.SalesOrder so on so.Id = pod.SalesOrderId
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            left join mst.MaterialMasterArticle mma on mma.ID = moi.ArticleId
                            where pod.ProductionOrderId = owe.ProductionOrderId) as ArticleCode
                            from (
							Select Date , ShiftId , EmployeeId , OperationVariationId,ProductionOrderId,Sum(Qty) as Qty
							from dbo.OperationWiseEmployees 
							group by Date , ShiftId , EmployeeId , OperationVariationId,ProductionOrderId
							) owe
                            left join dbo.EmployeeWiseProductionProcessing epp on epp.Date = owe.Date and epp.EmployeeId = owe.EmployeeId
                            left join mst.OperationVariation ov on ov.ID = owe.OperationVariationId
                            left join hkp.Skill sk on sk.ID = ov.SkillId
                            left join hkp.SkillCategory skc on skc.ID = sk.SkillCategoryId
                            left join dbo.EmployeeInformation ei on ei.SystemId = owe.EmployeeId
                            left join dbo.ShiftDefination sd on sd.SystemID = owe.ShiftId
                            where owe.Date  between '" + FromDate + @"' and '" + ToDate + @"' and epp.TotalSPT is not null
                            ) as aa on aa.EmployeeId = dd.SystemId and aa.WorkDate = dd.WorksDate
                            order by  CAST( dd.WorksDate as datetime) asc , aa.EmployeeId desc ";


                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion getEmployeeWorkDurationReport

    }

    public class EmployeeOperationsAPIService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public EmployeeOperationsAPIService()
        {
            _sqlRepository = new SqlRepository();

        }
        #endregion Constructor


        public IEnumerable<object> BalanceChecker(string ProdOrderId, string ProcessId, string Curr, string Prev)
        {
            try
            {
                var Sql = @"select dd.*,(dd.Qty-dd.CurrentBooking) as Balance 
                from (select OP.UserName as PrevOperation,bt.Sequence as PrevSeq,SUM(ope.Qty)Qty,
                isnull((select isnull(SUM(ope.qty),'0') from 
                OperationWiseEmployees ope 
                left join mst.OperationVariation OP on op.Id=ope.OperationVariationId
                            join trn.ProductionBulletinTemplateDetail bt on bt.OperationVariationId=OP.Id
                            join trn.ProductionBulletinTemplateMaster pt on pt.Id=bt.ProductionBulletinTemplateMasterId
                            join trn.ProductionBulletinTemplate pb on pb.Id=pt.ProductionBulletinTemplateId
                            where pb.ProductionOrderId='" + ProdOrderId + @"' AND bt.Sequence='" + Curr + @"' and
							PT.ProcessId='" + ProcessId + @"'
                            ),'0')as CurrentBooking
                            from 
                            OperationWiseEmployees ope 
                            left join mst.OperationVariation OP on op.Id=ope.OperationVariationId
                            join trn.ProductionBulletinTemplateDetail bt on bt.OperationVariationId=OP.Id
                            join trn.ProductionBulletinTemplateMaster pt on pt.Id=bt.ProductionBulletinTemplateMasterId
                            join trn.ProductionBulletinTemplate pb on pb.Id=pt.ProductionBulletinTemplateId
                            where pb.ProductionOrderId='" + ProdOrderId + @"' AND bt.Sequence='" + Prev + @"' and
							PT.ProcessId='" + ProcessId + @"' 
							group by bt.Sequence,op.UserName
							) as dd
							order by dd.PrevSeq";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetOperation(string ProdOrderId, string ProcessId)
        {
            try
            {
                var Sql = @"select OP.ID as Value,OP.UserName as Text,op.Code,bt.Sequence,OP.OperationMasterId from mst.OperationVariation OP
                            join trn.ProductionBulletinTemplateDetail bt on bt.OperationVariationId=OP.Id
                            join trn.ProductionBulletinTemplateMaster pt on pt.Id=bt.ProductionBulletinTemplateMasterId
                            join trn.ProductionBulletinTemplate pb on pb.Id=pt.ProductionBulletinTemplateId
                            where pb.ProductionOrderId='" + ProdOrderId + @"' AND PT.ProcessId='" + ProcessId + "'ORDER BY BT.Sequence";
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
                var Sql = @"select distinct ope.EmployeeId as Value,emp.EmployeeCode,emp.EmployeeName as Text 
                from dbo.OperationWiseEmployees ope 
                    left join dbo.EmployeeInformation emp on ope.EmployeeId=emp.SystemId
                    where ope.AddedBy='" + AddedBy + "' and ope.WorkCenterId='" + WkId + "' and ope.OperationVariationId='" + OPId + "'";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPeriod()
        {
            try
            {
                var Sql = @"Select id as Value , UserName as Text , StartTime , EndTime from hkp.ProductionBookingPeriod where CONVERT(VARCHAR(8), StartTime, 108) <= Convert(varchar(8), GETDATE(), 108)
                                        and CONVERT(VARCHAR(8), EndTime, 108) >= Convert(varchar(8), GETDATE(), 108)
                                        order by EndTime desc";
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
                #region 1st Table Data Filling

                DataSet dsMaster, dsSum, dsPlan;
                string TableName = "dbo.OperationWiseEmployees";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<DailyProduction> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where 1=2", out dsMaster, false, "1");

                string _Id = "";
                string OpSeq = "", OpMasterId = "";

                foreach (DailyProduction item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].Id == null)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        OpSeq = item.OperationSeq;
                        OpMasterId = item.OperationMasterId;
                        dr["Id"] = "OP" + _Id;
                        dr["ProcessId"] = item.ProcessId;
                        dr["WorkCenterId"] = item.WorkCenterId;
                        dr["Date"] = item.Date;
                        dr["ShiftId"] = item.ShiftId;
                        dr["Qty"] = item.Qty;
                        dr["PeriodId"] = item.PeriodId;
                        dr["ProductionOrderId"] = item.ProductionOrderId;
                        dr["OperationVariationId"] = item.OperationVariationId;
                        dr["Remarks"] = item.Remarks;
                        dr["EmployeeId"] = item.EmployeeId;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                }

                #endregion

                #region 2nd Table Data Filling  

                ConnectionManager.DAL.ConManager c = new ConnectionManager.DAL.ConManager("1");
                c.OpenDataSetThroughAdapter("select *  from  dbo.EmployeeOperationWip where ProductionOrderId = '" + items[0].ProductionOrderId + "' and ProcessId ='" + items[0].ProcessId + "' order by Cast(OperationSequence AS int) asc", out dsSum, false, "1");
                string _SId = "";

                DataTable dtSum = dsSum.Tables[0];

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    dsSum.Tables[0].DefaultView.RowFilter = @"OperationVariationId='" + dsMaster.Tables[0].Rows[i]["OperationVariationId"].ToString() + "' and OperationSequence ='" + OpSeq + "'";
                    if (dsSum.Tables[0].DefaultView.Count > 0)
                    {
                        dsSum.Tables[0].DefaultView[0].Row.BeginEdit();
                        dsSum.Tables[0].DefaultView[0]["Qty"] = clsStaticInfo.dbl(dsSum.Tables[0].DefaultView[0]["Qty"].ToString()) + clsStaticInfo.dbl(dsMaster.Tables[0].Rows[i]["Qty"].ToString());
                        dsSum.Tables[0].DefaultView[0]["UpdatedBy"] = dsMaster.Tables[0].Rows[i]["AddedBy"].ToString();
                        dsSum.Tables[0].DefaultView[0]["UpdatedDate"] = DateTime.Now.ToString();
                        dsSum.Tables[0].DefaultView[0]["UpdatedFromIP"] = dsMaster.Tables[0].Rows[i]["AddedFromIP"].ToString();
                        dsSum.Tables[0].DefaultView[0].Row.EndEdit();
                    }
                    else
                    {
                        DataRow dd = dsSum.Tables[0].NewRow();
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.EmployeeOperationWip", out _SId);
                        dd["Id"] = "OW" + _SId;
                        dd["ProductionOrderId"] = dsMaster.Tables[0].Rows[i]["ProductionOrderId"].ToString();
                        dd["OperationVariationId"] = dsMaster.Tables[0].Rows[i]["OperationVariationId"].ToString();
                        dd["OperationSequence"] = OpSeq;
                        dd["ProcessId"] = dsMaster.Tables[0].Rows[i]["ProcessId"].ToString();
                        dd["Qty"] = clsStaticInfo.dbl(dsMaster.Tables[0].Rows[i]["Qty"].ToString());
                        dd["AddedBy"] = dsMaster.Tables[0].Rows[i]["AddedBy"].ToString();
                        dd["AddedDate"] = DateTime.Now.ToString();
                        dd["AddedFromIP"] = dsMaster.Tables[0].Rows[i]["AddedFromIP"].ToString();
                        dsSum.Tables[0].Rows.Add(dd);
                    }

                }

                #endregion

                #region 3rd Table Data Filling 

                for (int i = 0; i < dsSum.Tables[0].Rows.Count; i++)
                {
                    if (clsStaticInfo.dbl(dsSum.Tables[0].Rows[i]["OperationSequence"].ToString()) == 1)
                    {
                        dsSum.Tables[0].Rows[i].BeginEdit();
                        dsSum.Tables[0].Rows[i]["WIP"] = 0;
                        dsSum.Tables[0].Rows[i].EndEdit();
                    }
                    else
                    {
                        dsSum.Tables[0].Rows[i].BeginEdit();
                        dsSum.Tables[0].Rows[i]["WIP"] = clsStaticInfo.dbl(dsSum.Tables[0].Rows[i]["Qty"].ToString()) - clsStaticInfo.dbl(dsSum.Tables[0].Rows[i - 1]["Qty"].ToString());
                        dsSum.Tables[0].Rows[i].EndEdit();
                    }
                }

                DateTime Datex = Convert.ToDateTime(items[0].Date);
                ConnectionManager.DAL.ConManager co = new ConnectionManager.DAL.ConManager("1");
                co.OpenDataSetThroughAdapter("select *  from  dbo.EmployeeWiseProductionProcessing where Date='" + Datex.ToString("dd-MMM-yyyy") + "' and ProductionOrderId='" + items[0].ProductionOrderId + "'", out dsPlan, false, "1");

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    dsPlan.Tables[0].DefaultView.RowFilter = @"EmployeeId = '" + dsMaster.Tables[0].Rows[i]["EmployeeId"] + "' and OperationVariationId='" + dsMaster.Tables[0].Rows[i]["OperationVariationId"].ToString() + "'";
                    if (dsPlan.Tables[0].DefaultView.Count > 0)
                    {
                        dsPlan.Tables[0].DefaultView[0].Row.BeginEdit();
                        dsPlan.Tables[0].DefaultView[0]["Qty"] = clsStaticInfo.dbl(dsPlan.Tables[0].DefaultView[0]["Qty"].ToString()) + clsStaticInfo.dbl(dsMaster.Tables[0].Rows[i]["Qty"].ToString());
                        dsPlan.Tables[0].DefaultView[0]["UpdatedBy"] = dsMaster.Tables[0].Rows[i]["AddedBy"].ToString();
                        dsPlan.Tables[0].DefaultView[0]["UpdatedDate"] = DateTime.Now.ToString();
                        dsPlan.Tables[0].DefaultView[0]["UpdatedFromIP"] = dsMaster.Tables[0].Rows[i]["AddedFromIP"].ToString();
                        dsPlan.Tables[0].DefaultView[0].Row.EndEdit();
                    }
                    else
                    {
                        DataRow dr = dsPlan.Tables[0].NewRow();
                        dr["Id"] = dsMaster.Tables[0].Rows[i]["Id"].ToString() + i.ToString();
                        dr["Date"] = Convert.ToDateTime(dsMaster.Tables[0].Rows[i]["Date"].ToString());
                        dr["EmployeeId"] = dsMaster.Tables[0].Rows[i]["EmployeeId"];
                        dr["MasterOperationId"] = OpMasterId.ToString();
                        dr["OperationVariationId"] = dsMaster.Tables[0].Rows[i]["OperationVariationId"].ToString();
                        dr["ProductionOrderId"] = dsMaster.Tables[0].Rows[i]["ProductionOrderId"].ToString();
                        dr["Qty"] = clsStaticInfo.dbl(dsMaster.Tables[0].Rows[i]["Qty"].ToString());
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["AddedBy"] = dsMaster.Tables[0].Rows[i]["AddedBy"].ToString();
                        dr["AddedFromIP"] = dsMaster.Tables[0].Rows[i]["AddedFromIP"].ToString();
                        dsPlan.Tables[0].Rows.Add(dr);
                    }
                }

                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsSum, dsPlan);
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
                var Sql = @"select SUM(ps.Qty)Qty,FORMAT(ps.Date,'dd-MMM-yyyy')Date,ps.OperationVariationId,ps.ProductionOrderId, ps.ProcessId,Pr.UserName as Process ,
                opv.UserName as Operation,sh.UserName as ProductionShift,ps.WorkCenterId
                                                            from OperationWiseEmployees ps
															left join trn.ProductionOrder po on po.Id=ps.ProductionOrderId
                                                            left join HKP.Process pr on ps.ProcessId = pr.Id
                                                            left join dbo.ShiftDefination sh on ps.ShiftId=sh.SystemID
                                                            left join mst.OperationVariation opv on opv.Id=ps.OperationVariationId
                                                            left join dbo.EmployeeInformation emp
                                                            on ps.EmployeeId = emp.SystemId
                                                            where isnull(ps.Date, '') = '" + ProdnDate + @"'
                                                            and isnull(ps.ProcessId,'')= '" + ProcessId + @"' and 
															isnull(ps.ShiftId,'')= '" + ShiftId + @"'
                                                            and isnull(ps.WorkCenterId,'')= '" + WkId + "' and ISNULL(po.EntityId,'')='" + EntityId + @"'
															GROUP BY ps.OperationVariationId,
                                                            ps.ProcessId,ps.Date,Pr.UserName,opv.UserName,ps.ProductionOrderId,sh.UserName,ps.WorkCenterId";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception)
            {
                throw;
            }



        }

        public void GetQty(string ProcessId, string PO, string Seq, out DataSet ds)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sqlx = @"select *  from  dbo.EmployeeOperationWip where 
                ProductionOrderId = '" + PO + "' and ProcessId ='" + ProcessId + "' and OperationSequence='" + Seq + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public IEnumerable<object> GetDetailProductionList(string ProdnDate, string EntityId, string ProcessId, string ShiftId, string WkId, string PoId, string OPId)
        {
            try
            {
                var sqlx = @"select dp.Id, CAST(dp.AddedDate as time) Time,Buyer =STUFF((select distinct ','+XB.UserName from
                                    trn.SalesOrder XSO                                        
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        LEFT JOIN TRN.ProductionOrder po on po.Id=Xpod.ProductionOrderId
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                        left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                        left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId                                                                     
                                        where po.Id=" + PoId + @" and po.EntityId='" + EntityId + @"' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')  ,   
                                BuyerReferenceNo =STUFF((select distinct ','+XMO.BuyerReferenceNo from
                                    trn.SalesOrder XSO                                        
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        LEFT JOIN TRN.ProductionOrder po on po.Id=Xpod.ProductionOrderId
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                        left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                        left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId                                                                     
                                        where po.Id='" + PoId + @"' 
										and po.EntityId='" + EntityId + @"' for 
										xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')  ,
                                    dp.Qty,(emp.EmployeeName) Employee,dp.EmployeeId as EmpId, opv.id as OperationId,
                                    opv.UserName as Operation,dp.AddedBy,dp.ProductionOrderId as POId,Pr.UserName as Process,
                                    Wc.UserName as WorkCenter,sh.UserName as Shift from 
									dbo.OperationWiseEmployees dp
                                    left join HKP.Process pr on dp.ProcessId=pr.Id
                                    left join SCS.WorkCenterMaster wc on dp.WorkCenterId=wc.Id
                                    left join mst.OperationVariation opv on dp.OperationVariationId=opv.Id
                                    left join dbo.ShiftDefination sh on dp.ShiftId=sh.SystemID
                                    left join dbo.EmployeeInformation emp on dp.EmployeeId=emp.SystemId                      
                                    where isnull(dp.Date, '') = '" + ProdnDate + @"'
                                    and isnull(dp.ProcessId,'')= '" + ProcessId + "' and isnull(dp.ShiftId,'')= '" + ShiftId + @"' and 
                                    isnull(dp.WorkCenterId,'')='" + WkId + "' and isnull(dp.ProductionOrderId,'')='" + PoId + @"' 
                                    and isnull(dp.OperationVariationId,'')='" + OPId + "'";

                return _sqlRepository.GetDataCollection(sqlx, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

    }


    public class EmployeeTimeOutService
    {

        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public EmployeeTimeOutService()
        {
            _sqlRepository = new SqlRepository();

        }
        #endregion Constructor

        #region GetOperations

        public IEnumerable<object> getEmployees()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"Select SystemId , EmployeeCode , EmployeeName from dbo.EmployeeInformation where PlantId = '" + identity.PlantId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEmpTimeOut(string EmpId, string Date)
        {
            try
            {
                var str = @"Select Convert(varchar(10),Cast(FromTime as Time),100) as Frm , Convert(varchar(10),Cast(ToTime as Time),100) as Trm , Duration from dbo.EmployeesTimeOut where WorkDate = '" + Date + "' and EmployeeId ='" + EmpId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Saving

        private string RemoveWhitespace(string str)
        {
            var jj = str.Split(default(char));
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        public void Create(string EmployeeId, string Date, string FromTime, string ToTime)
        {
            try
            {
                string Frm = RemoveWhitespace(FromTime);
                string Trm = RemoveWhitespace(ToTime);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var Quer = "select * from dbo.EmployeesTimeOut where WorkDate='" + Date + "' and EmployeeId='" + EmployeeId + "' and Convert(varchar(10),Cast(FromTime as Time),100)='" + Frm + "' and Convert(varchar(10),Cast(ToTime as Time),100)='" + Trm + "'";
                con.OpenDataSetThroughAdapter(Quer, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Already an entry for the Time Slot is Present!!");
                }

                con.OpenDataSetThroughAdapter("select * from dbo.EmployeesTimeOut where WorkDate='" + Date + "' and EmployeeId='" + EmployeeId + "' ", out dsMaster, false, "1");

                DateTime FTime = Convert.ToDateTime(FromTime);
                DateTime TTime = Convert.ToDateTime(ToTime);

                double Dur = (TTime - FTime).TotalMinutes;

                if (Dur < 0)
                {
                    throw new Exception("To Time Cannot be Less than From Time");
                }
                string _Id = "";
                DataRow dr = dsMaster.Tables[0].NewRow();
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("dbo.EmployeesTimeOut", out _Id);
                dr["Id"] = _Id;
                dr["EmployeeId"] = EmployeeId;
                dr["WorkDate"] = Convert.ToDateTime(Date);
                dr["FromTime"] = Convert.ToDateTime(FromTime);
                dr["ToTime"] = Convert.ToDateTime(ToTime);
                dr["Duration"] = Dur;
                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = System.DateTime.Now.ToString();
                dr["AddedFromIP"] = identity.IPAddress;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;
                dsMaster.Tables[0].Rows.Add(dr);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


    }
}
