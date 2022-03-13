using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Linq;

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


        public IEnumerable<object> GetWorkCenter()
        {
            try
            {
                var Sql = @"select distinct ope.WorkCenterId as Value,wk.UserName as Text from dbo.OperationWiseEmployee ope 
                            left join scs.WorkCenterMaster wk on ope.WorkCenterId=wk.Id";
                //where ope.AddedBy='" + AddedBy + "'
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetProcess()
        {
            try
            {
                // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = "Select Id as Value , UserName as Text from hkp.process order by UserName asc";
                return _sqlRepository.GetDataCollection(str);
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
                // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = "Select id as Value , UserName as Text , StartTime , EndTime from hkp.ProductionBookingPeriod order by StartTime asc";
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

        public IEnumerable<object> GetPOs(string wk)
        {
            try
            {
                var str = @"Select distinct po.Id
                            from Scs.WorkCenterMaster wc
                            left join org.Entity e on e.ID = wc.EntityId
                            left join trn.ProductionOrder po on po.EntityId = e.Id
                            where wc.Id = '" + wk + @"' and po.ProductionStatusId = '20191'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetOperationsData(string PId , string Period)
        {
            //Filling the PeriodId
            string currPeriod = "";
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



            var str = @"select OP.ID as OperationId, OP.Code as OperationCode ,OP.UserName as OperationName, bt.Sequence , owe.EmployeeId , 
                        Sum(owe.Qty) as Qty ,
                        --Sum(owe.Period1) as Period1c , Sum(owe.Period2) as Period2 , Sum(owe.Period2) as Period2c , Sum(owe.Period3) as Period3 ,Sum(owe.Period3) as Period3c , 
                        --Sum(owe.Period4) as Period4 ,Sum(owe.Period4) as Period4c , Sum(owe.Period5) as Period5 , Sum(owe.Period5) as Period5c , 
                        --Sum(owe.Period6) as Period6 , Sum(owe.Period6) as Period6c , 
                        ei.EmployeeCode
                        from mst.OperationVariation OP
                        left join trn.ProductionBulletinTemplateDetail bt on bt.OperationVariationId=OP.Id
                        left join trn.ProductionBulletinTemplateMaster pt on pt.Id=bt.ProductionBulletinTemplateMasterId
                        left join trn.ProductionBulletinTemplate pb on pb.Id=pt.ProductionBulletinTemplateId
                        left join dbo.OperationWiseEmployees owe on owe.OperationVariationId = OP.Id and owe.ProductionOrderId = pb.ProductionOrderId
                        left join dbo.EmployeeInformation ei on ei.SystemId = owe.EmployeeId
						where pb.ProductionOrderId='"+PId+@"'  --and owe.PeriodId = '"+ currPeriod + @"' and owe.Date =  Convert(date, DateAdd(DAY, -1, GetDate())) 
						group by OP.Id , op.Code , op.UserName , bt.Sequence , owe.EmployeeId , ei.EmployeeCode
                        order by Sequence";
            return _sqlRepository.GetDataCollection(str);
        }



        public void saveData(List<Dictionary<string, object>> data, string WorkCenter, string ProcessId, string ShiftId, string POId, string Date , string PeriodId)
        {
            try
            {
                DataSet dsMaster;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string TableName = "dbo.OperationWiseEmployees";

                #region Detail
                
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select *  from dbo.OperationWiseEmployees where 1 = 2 ", out dsMaster, false, "1");


                //Filling the EmpSystemIds

                var empStr = @"Select distinct SystemId , EmployeeCode from dbo.EmployeeInformation";
                DataTable dt = _sqlRepository.GetDataTable(empStr);
                for (int i = 0; i < data.Count; i++)
                {
                    dt.DefaultView.RowFilter = @"EmployeeCode = '" +data[i]["EmployeeCode"].ToString() + "'";
                    if(dt.DefaultView.Count > 0)
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


                for( int i = 0;i < data.Count; i++)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    dr["Id"] = _Id;
                    dr["ProcessId"] = ProcessId;
                    dr["ShiftId"] = ShiftId;
                    dr["WorkCenterId"] = WorkCenter;
                    dr["ProductionOrderId"] = POId;
                    dr["OperationVariationId"] = data[i]["OperationId"];
                    dr["EmployeeId"] = data[i]["EmployeeId"];
                    dr["Date"] = Convert.ToDateTime(Date.ToString());
                    dr["Qty"] = data[i]["Qty"];
                    dr["PeriodId"] = currPeriod;                   
                    dr["Remarks"] = data[i]["Remarks"];
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                #endregion Detail

                #region Summary
                DataSet dsSum;
                ConnectionManager.DAL.ConManager c = new ConnectionManager.DAL.ConManager("1");
                c.OpenDataSetThroughAdapter("select *  from dbo.OperationWiseEmployeesSummary where ProductionOrderId = '"+POId+"' and WorkCenterId='"+WorkCenter+"'", out dsSum, false, "1");
                string _SId = "";
               

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    dsSum.Tables[0].DefaultView.RowFilter = @"OperationVariationId='"+data[i]["OperationId"].ToString()+"' and Sequence ='"+data[i]["Sequence"].ToString()+"'";
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
                        genid.GenID("dbo.OperationWiseEmployeesSummary", out _SId);
                        dd["Id"] = _SId;
                        dd["WorkCenterId"] = WorkCenter;
                        dd["ProductionOrderId"] = POId;
                        dd["OperationVariationId"] = data[i]["OperationId"].ToString();
                        dd["Sequence"] = data[i]["Sequence"].ToString();
                        dd["Qty"] = clsStaticInfo.dbl(data[i]["Qty"].ToString());
                        dd["AddedBy"] = identity.Name;
                        dd["AddedDate"] = System.DateTime.Now.ToString();
                        dd["AddedFromIP"] = identity.IPAddress;
                        dd["UpdatedBy"] = identity.Name;
                        dd["UpdatedDate"] = System.DateTime.Now.ToString();
                        dd["UpdatedFromIP"] = identity.IPAddress;
                        dsSum.Tables[0].Rows.Add(dd);

//Select id as Value , UserName as Text , StartTime , EndTime from hkp.ProductionBookingPeriod where CONVERT(VARCHAR(8), StartTime, 108) <= Convert(varchar(8), '07:20', 108)
//and CONVERT(VARCHAR(8), EndTime, 108) >= Convert(varchar(8), '07:20', 108)
//order by EndTime desc

                    }
                }

                #endregion Summary

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster , dsSum);
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
                if (clsStaticInfo.dbl(num) > 999)
                {
                    throw new Exception("Period Value Greater than 999");
                }
                else
                {
                    return clsStaticInfo.dbl(num);
                }
            }
            return 0;
        }

        public IEnumerable<object> getReportView( out List<string> Cols)
        {
            try
            {
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
                            group by ov.Code , ov.UserName , p.UserName , ei.EmployeeName , ei.EmployeeCode , we.Date , we.PeriodId  , pb.UserName, wcm.UserName ,we.ProductionOrderId ";

                DataTable dtAll = _sqlRepository.GetDataTable(str);

                //Getting the Periods
                List<string> ltPer = new List<string>();
                DataTable periods = dtAll.DefaultView.ToTable(true , "Periods");
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
                DataRow dr = null;
                for (int i = 0; i < dtAll.Rows.Count; i++)
                {
                    if (dtAll.Rows[i]["OperationCode"].ToString() != opCode && dtAll.Rows[i]["EmployeeCode"].ToString() != empCode)
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

                }

                Cols = ltPer;

                return Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtNew);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
