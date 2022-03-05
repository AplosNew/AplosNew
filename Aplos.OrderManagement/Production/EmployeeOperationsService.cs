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

        public IEnumerable<object> GetOperationsData(string PId)
        {
            var str = @"select OP.ID as OperationId, OP.Code as OperationCode ,OP.UserName as OperationName, bt.Sequence , owe.* , ei.EmployeeCode
                        from mst.OperationVariation OP
                        left join trn.ProductionBulletinTemplateDetail bt on bt.OperationVariationId=OP.Id
                        left join trn.ProductionBulletinTemplateMaster pt on pt.Id=bt.ProductionBulletinTemplateMasterId
                        left join trn.ProductionBulletinTemplate pb on pb.Id=pt.ProductionBulletinTemplateId
                        left join dbo.OperationWiseEmployees owe on owe.OperationVariationId = OP.Id and owe.ProductionOrderId = pb.ProductionOrderId
                        left join dbo.EmployeeInformation ei on ei.SystemId = owe.EmployeeId
                        where pb.ProductionOrderId='"+PId+@"'
                        order by Sequence";
            return _sqlRepository.GetDataCollection(str);
        }



        public void saveData(List<Dictionary<string, object>> data, string WorkCenter, string ProcessId, string ShiftId, string POId, string Date)
        {
            try
            {
                DataSet dsMaster;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string TableName = "dbo.OperationWiseEmployees";
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
                        
                      
                        data[i]["Period1"] = getNum(data[i]["Period1"]);
                        data[i]["Period2"] = getNum(data[i]["Period2"]);
                        data[i]["Period3"] = getNum(data[i]["Period3"]);
                        data[i]["Period4"] = getNum(data[i]["Period4"]);
                        data[i]["Period5"] = getNum(data[i]["Period5"]);
                        data[i]["Period6"] = getNum(data[i]["Period6"]);
                    }
                    else
                    {
                        throw new Exception("The Employee Code in Serial " + data[i]["Serial"] + " is not Present");
                    }
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
                    dr["Period1"] = data[i]["Period1"];
                    dr["Period2"] =data[i]["Period2"];
                    dr["Period3"] =data[i]["Period3"];
                    dr["Period4"] =data[i]["Period4"];
                    dr["Period5"] =data[i]["Period5"];
                    dr["Period6"] =data[i]["Period6"];
                    dr["Remarks"] = data[i]["Remarks"];
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
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
    }
}
