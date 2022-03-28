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

            var str = @"select OP.ID as OperationId, OP.OperationMasterId as MasterOperationId  ,OP.Code as OperationCode ,OP.UserName as OperationName, bt.Sequence , owe.EmployeeId , 
                        isnull(Sum(owe.Qty),0) as Qty ,
                      
                        ei.EmployeeCode
                        from mst.OperationVariation OP
                        left join trn.ProductionBulletinTemplateDetail bt on bt.OperationVariationId=OP.Id
                        left join trn.ProductionBulletinTemplateMaster pt on pt.Id=bt.ProductionBulletinTemplateMasterId
                        left join trn.ProductionBulletinTemplate pb on pb.Id=pt.ProductionBulletinTemplateId
                        left join ( Select owe.ProductionOrderId , owe.OperationVariationId , owe.EmployeeId, owe.Date , isnull(owep.Qty,0) as Qty 
						from dbo.OperationWiseEmployees owe 
						left join dbo.OperationWiseEmployees owep on owep.Id = owe.Id and owep.PeriodId   ='" + currPeriod + @"'
						) as owe on owe.OperationVariationId = OP.Id and owe.ProductionOrderId = pb.ProductionOrderId and owe.Date =   Convert(date, DateAdd(DAY, -1, GetDate())) 
                        left join dbo.EmployeeInformation ei on ei.SystemId = owe.EmployeeId
						where pb.ProductionOrderId='" + PId + @"'
						
						group by OP.Id , op.Code , op.UserName , bt.Sequence , owe.EmployeeId , ei.EmployeeCode , op.OperationMasterId
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

                    data[i]["Id"] = _Id;
                    dr["Id"] = _Id;
                    dr["ProcessId"] = ProcessId;
                    dr["Sequence"] = data[i]["Sequence"];
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
                c.OpenDataSetThroughAdapter("select *  from  dbo.EmployeeOperationWip where ProductionOrderId = '" + POId+ "' ", out dsSum, false, "1");
                string _SId = "";

                DataTable dtSum = dsSum.Tables[0];

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    dsSum.Tables[0].DefaultView.RowFilter = @"OperationVariationId='"+data[i]["OperationId"].ToString()+"' and OperationSequence ='"+data[i]["Sequence"].ToString()+"'";
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
                        dd["ProductionOrderId"] = POId;
                        dd["OperationVariationId"] = data[i]["OperationId"].ToString();
                        dd["OperationSequence"] = data[i]["Sequence"].ToString();
                        dd["Qty"] = clsStaticInfo.dbl(data[i]["Qty"].ToString());
                        dd["AddedBy"] = identity.Name;
                        dd["AddedDate"] = System.DateTime.Now.ToString();
                        dd["AddedFromIP"] = identity.IPAddress;
                        dd["UpdatedBy"] = identity.Name;
                        dd["UpdatedDate"] = System.DateTime.Now.ToString();
                        dd["UpdatedFromIP"] = identity.IPAddress;
                        dsSum.Tables[0].Rows.Add(dd);


                    }

                    //dsSum.Tables[0].DefaultView.RowFilter = @"OpVariationId='" + data[i]["OperationId"].ToString() + "' and OperationSequence ='" + (int.Parse(data[i]["Sequence"].ToString()) + 1).ToString() + "'";
                    //if (dsSum.Tables[0].DefaultView.Count > 0)
                    //{
                    //}

                }

                #endregion Summary

                #region Employee Production Processing Half
                DataSet dsPlan = null;
                ConnectionManager.DAL.ConManager co = new ConnectionManager.DAL.ConManager("1");
                co.OpenDataSetThroughAdapter("select *  from  dbo.EmployeeWiseProductionProcessing where Date='" + Date + "' and ProductionOrderId='" + POId + "'", out dsPlan, false, "1");

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    dsPlan.Tables[0].DefaultView.RowFilter = @"EmployeeId = '"+data[i]["EmployeeId"]+"' and OperationVariationId='"+ data[i]["OperationId"].ToString() + "'";
                    if (dsPlan.Tables[0].DefaultView.Count > 0)
                    {
                        dsPlan.Tables[0].DefaultView[0].Row.BeginEdit();
                        dsPlan.Tables[0].DefaultView[0]["Qty"] = clsStaticInfo.dbl(dsPlan.Tables[0].DefaultView[0]["Qty"].ToString()) + clsStaticInfo.dbl(data[i]["Qty"].ToString());
                        dsPlan.Tables[0].DefaultView[0].Row.EndEdit();
                    }
                    else
                    {
                        DataRow dr = dsPlan.Tables[0].NewRow();
                        dr["Id"] = (int.Parse(dsMaster.Tables[0].Rows[i]["Id"].ToString()) + i).ToString();
                        dr["Date"] = Convert.ToDateTime(Date);
                        dr["EmployeeId"] = dsMaster.Tables[0].Rows[i]["EmployeeId"];
                        dr["MasterOperationId"] = data[i]["MasterOperationId"];
                        dr["OperationVariationId"] = data[i]["OperationId"].ToString();
                        dr["ProductionOrderId"] = POId;
                        dr["Qty"] = clsStaticInfo.dbl(data[i]["Qty"].ToString());
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsPlan.Tables[0].Rows.Add(dr);
                    }
                }

               

                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster , dsSum , dsPlan);
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
                            group by ov.Code , ov.UserName , p.UserName , ei.EmployeeName , ei.EmployeeCode , we.Date , we.PeriodId  , pb.UserName, wcm.UserName ,we.ProductionOrderId 
                            order by Dates , ei.EmployeeName asc";

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

        public DataTable getReportDownload(out List<string> DynCols)
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
                
                #region 1. Getting The Current Parial Data

                DataSet dsMasterHalf = null;
                ConnectionManager.DAL.ConManager co = new ConnectionManager.DAL.ConManager("1");
                co.OpenDataSetThroughAdapter("select *  from  dbo.EmployeeWiseProductionProcessing where Date='" + Date + "'  order by EmployeeId asc, Qty desc", out dsMasterHalf, false, "1");

                DataTable dtMasterHalf = dsMasterHalf.Tables[0];

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
								left join hkp.Skill sk on sk.Id = ptd.SkillId
                                left join hkp.SkillCategory skc on skc.Id = sk.SkillCategoryId";

               

                DataTable dtTotalSpt = _sqlRepository.GetDataTable(str);

                #endregion

                #region 3. Filling in the Sequences

                if (dtMasterHalf.Rows.Count <= 0)
                {
                    throw new Exception("There is no data to Process!!");
                }
                else
                {
                    string empId = "";
                    int k = 0;
                    for (int i = 0; i < dtMasterHalf.Rows.Count; i++)
                    {
                        k++;
                        if (dtMasterHalf.Rows[i]["EmployeeId"].ToString() == empId)
                        {
                            dtMasterHalf.Rows[i]["Sequence"] = k;
                        }
                        else
                        {
                            k = 1;
                            
                            dtMasterHalf.Rows[i]["Sequence"] = k;
                        }
                        empId = dtMasterHalf.Rows[i]["EmployeeId"].ToString();
                    }

                }

                #endregion

                #region 4. Calculation and Filling of the MasterHalf DataTable

                Dictionary<string,string> dicSkillCat = new Dictionary<string, string>(); // List<Dictionary<string, string>>dicSkillCat = new List<Dictionary<string, string>>();

                for (int i = 0; i < dtMasterHalf.Rows.Count; i++)
                {
                    dtTotalSpt.DefaultView.RowFilter = @"ProductionOrderId='"+dtMasterHalf.Rows[i]["ProductionOrderId"].ToString()+"' and OperationVariationId='"+ dtMasterHalf.Rows[i]["OperationVariationId"].ToString() + "'";
                    dicSkillCat.Add(dtMasterHalf.Rows[i]["Id"].ToString() , dtTotalSpt.DefaultView[0]["SkillCatId"].ToString());
                    dtMasterHalf.Rows[i].BeginEdit();
                    dtMasterHalf.Rows[i]["StandardProcessTime"] = clsStaticInfo.dbl(dtTotalSpt.DefaultView[0]["TotalSPT"].ToString());
                    dtMasterHalf.Rows[i]["TotalSPT"] = clsStaticInfo.dbl(dtMasterHalf.Rows[i]["Qty"].ToString())*clsStaticInfo.dbl(dtTotalSpt.DefaultView[0]["TotalSPT"].ToString());
                    dtMasterHalf.Rows[i].EndEdit();
                }

                #endregion

                #region 5. Calculation of Allowances

                var str2 = @"Select epp.Id, po.EntityId , owe.ProcessId from dbo.EmployeeWiseProductionProcessing epp
                            left join trn.ProductionOrder po on po.Id = epp.ProductionOrderId
                            left join ( Select distinct owe.Date , owe.EmployeeId , owe.OperationVariationId , owe.ProcessId from dbo.OperationWiseEmployees owe
                             where Date='"+Date+ @"') owe on owe.EmployeeId = epp.EmployeeId and owe.Date = epp.Date and owe.OperationVariationId = epp.OperationVariationId
                            where epp.Date='" + Date + @"'";

                DataTable dtMainDict = _sqlRepository.GetDataTable(str2);

                var str3 = @"Select ph.Id ,ph.EffectiveDate, pp.ProcessId , pe.EntityId , pc.SkillAllowance , pc.AdditionOperationAllowance , pc.OperationSequence , pc.SkillCategoryId
                            from dbo.ProducedMinAllowanceHeader ph
                            left join dbo.ProducedMinAllowanceEntity pe on pe.HeaderId = ph.Id
                            left join dbo.ProducedMinAllowanceProcess pp on pp.HeaderId = ph.Id
                            left join dbo.ProducedMinAllowanceChild pc on pc.HeaderId = ph.Id";

                DataTable dtAllowance = _sqlRepository.GetDataTable(str3);

                for (int i = 0; i < dtMasterHalf.Rows.Count; i++)
                {

                    dtMainDict.DefaultView.RowFilter = @"Id = '" + dtMasterHalf.Rows[i]["Id"].ToString() + "'";

                    string SkillCatId = dicSkillCat[dtMasterHalf.Rows[i]["Id"].ToString()].ToString();
                    string EntityId = dtMainDict.DefaultView[0]["EntityId"].ToString();
                    string ProcessId = dtMainDict.DefaultView[0]["ProcessId"].ToString();
                    string OpSeq = dtMasterHalf.Rows[i]["Sequence"].ToString();

                    dtAllowance.DefaultView.RowFilter = @"EntityId='" + EntityId+"' and ProcessId ='"+ProcessId+ @"' 
                                                        and SkillCategoryId='"+ SkillCatId+ "' and OperationSequence='"+OpSeq+"'";
                    dtMasterHalf.Rows[i].BeginEdit();
                    dtMasterHalf.Rows[i]["SkillAllowance"] = clsStaticInfo.dbl(dtAllowance.DefaultView[0]["SkillAllowance"].ToString()) / 100;
                    dtMasterHalf.Rows[i]["AdditionalOperationAllowance"] = clsStaticInfo.dbl(dtAllowance.DefaultView[0]["AdditionOperationAllowance"].ToString()) / 100;

                    double skAll = clsStaticInfo.dbl(dtMasterHalf.Rows[i]["SkillAllowance"].ToString()) * clsStaticInfo.dbl(dtMasterHalf.Rows[i]["TotalSPT"].ToString());
                    double addAll = clsStaticInfo.dbl(dtMasterHalf.Rows[i]["AdditionalOperationAllowance"].ToString()) * clsStaticInfo.dbl(dtMasterHalf.Rows[i]["TotalSPT"].ToString());

                    dtMasterHalf.Rows[i]["AllotedProducedMin"] = clsStaticInfo.dbl(dtMasterHalf.Rows[i]["TotalSPT"].ToString()) + skAll + addAll;
                    dtMasterHalf.Rows[i].EndEdit();
                }

                #endregion

                #region 6. Getting the Rates Data

                DataSet dsRatesHealf = null;
                ConnectionManager.DAL.ConManager connec = new ConnectionManager.DAL.ConManager("1");
                connec.OpenDataSetThroughAdapter("Select * from dbo.EmployeeEfficiencyProcess where Date='" + Date + "'  order by EmployeeId asc", out dsRatesHealf, false, "1");

                DataTable dtRatesHealf = dsRatesHealf.Tables[0];

                var str4 = @"Select ih.Id , ih.EffectiveDate , ie.EntityId , ips.ProcessId , ics.Effeciency, ics.EffeciencyRate from dbo.IncentiveRateSetupHeader ih
                            left join dbo.IncentiveRateSetupEntity ie on ie.HeaderId = ih.Id
                            left join dbo.IncentiveRateSetupProcess ips on ips.HeaderId = ih.Id
                            left join dbo.IncentiveRateSetupChild ics on ics.HeaderId = ih.Id";

                DataTable dtRatesTable = _sqlRepository.GetDataTable(str4);


                var str5 = @"Select EmpSystemId , isnull(Duration,0) as Duration , WorkDate from dbo.AttdnProcessData where WorkDate = '" + Date + @"'";
                DataTable dtApd = _sqlRepository.GetDataTable(str5);

                #endregion

                #region 7. Calculation Of the Duration And Efficiency

                for (int i = 0; i < dtMasterHalf.Rows.Count; i++)
                {
                    dtApd.DefaultView.RowFilter = @"EmpSystemId ='"+dtMasterHalf.Rows[i]["EmployeeId"].ToString()+"'";
                    double Dur = clsStaticInfo.dbl(dtApd.DefaultView[0]["Duration"].ToString());

                    dtRatesHealf.DefaultView.RowFilter = @"EmployeeId='"+ dtMasterHalf.Rows[i]["EmployeeId"].ToString() + "'";
                    if (dtRatesHealf.DefaultView.Count > 0)
                    {
                        dtRatesHealf.DefaultView[0].Row.BeginEdit();
                        dtRatesHealf.DefaultView[0]["TotalSPT"] = clsStaticInfo.dbl(dtRatesHealf.DefaultView[0]["TotalSPT"].ToString()) + clsStaticInfo.dbl(dtMasterHalf.Rows[i]["TotalSPT"].ToString());
                        dtRatesHealf.DefaultView[0]["AllotedProducedMin"] = clsStaticInfo.dbl(dtRatesHealf.DefaultView[0]["AllotedProducedMin"].ToString()) + clsStaticInfo.dbl(dtMasterHalf.Rows[i]["AllotedProducedMin"].ToString());
                        dtRatesHealf.DefaultView[0]["WorkDuration"] = Dur;
                        dtRatesHealf.DefaultView[0]["NetEfficiency"] = (clsStaticInfo.dbl(dtRatesHealf.DefaultView[0]["TotalSPT"].ToString()) / Dur)*100;
                        dtRatesHealf.DefaultView[0]["GrossEfficiency"] = (clsStaticInfo.dbl(dtRatesHealf.DefaultView[0]["AllotedProducedMin"].ToString()) / Dur)*100;
                        dtRatesHealf.DefaultView[0].Row.EndEdit();
                    }
                    else
                    {
                        DataRow dr = dtRatesHealf.NewRow();
                        dr["Id"] = dtMasterHalf.Rows[i]["Id"].ToString();
                        dr["EmployeeId"] = dtMasterHalf.Rows[i]["EmployeeId"].ToString();
                        dr["TotalSPT"] = clsStaticInfo.dbl( dtMasterHalf.Rows[i]["TotalSPT"].ToString());
                        dr["AllotedProducedMin"] = clsStaticInfo.dbl( dtMasterHalf.Rows[i]["AllotedProducedMin"].ToString());
                        dr["WorkDuration"] = Dur;
                        dr["NetEfficiency"] = (clsStaticInfo.dbl(dtMasterHalf.Rows[i]["TotalSPT"].ToString())/Dur)*100;
                        dr["GrossEfficiency"] = (clsStaticInfo.dbl(dtMasterHalf.Rows[i]["AllotedProducedMin"].ToString())/Dur)*100;
                        dr["Rate"] = 0;
                        dr["Amount"] = 0;
                        dtRatesHealf.Rows.Add(dr);
                    }
                }


                #endregion

                #region 8. Calculation Of Efficiency Rate And Amount

                for (int i = 0; i < dtRatesHealf.Rows.Count; i++)
                {
                    dtMainDict.DefaultView.RowFilter = @"Id = '" + dtRatesHealf.Rows[i]["Id"].ToString() + "'";
                    string EntityId = dtMainDict.DefaultView[0]["EntityId"].ToString();
                    string ProcessId = dtMainDict.DefaultView[0]["ProcessId"].ToString();

                    dtRatesTable.DefaultView.RowFilter = @"EntityId='"+EntityId+"' and ProcessId='"+ProcessId+"'";

                    dtRatesTable.DefaultView.Sort = "Efficiency";

                    DataTable dtTemp = dtRatesTable.DefaultView.ToTable();
                    double Rate = 0.0;
                    for (int j = 0; j < dtTemp.Rows.Count; j++)
                    {
                        if (clsStaticInfo.dbl(dtRatesHealf.Rows[i]["GrossEfficiency"].ToString()) <= clsStaticInfo.dbl(dtTemp.Rows[j]["Efficiency"].ToString()))
                        {
                            Rate = clsStaticInfo.dbl(dtTemp.Rows[j]["EfficiencyRate"].ToString());
                        }
                        else
                        {
                            break;
                        }
                    }

                    dtRatesHealf.Rows[i].BeginEdit();
                    dtRatesHealf.Rows[i]["Rate"] = Rate;
                    dtRatesHealf.Rows[i]["Amount"] = clsStaticInfo.dbl(dtRatesHealf.Rows[i]["GrossEfficiency"].ToString()) * Rate;
                    dtRatesHealf.Rows[i].EndEdit();

                }


                #endregion

                var jj = 0;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
