using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class DayStatusService
    {

        ISqlRepository _sqlRepository;
        public DayStatusService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getPlants()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"Select Username as Text , Id as Value from ORG.Plant --where CompanyId = '" + identity.CompanyId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getOTRateList()
        {
            var str = @"Select Username+ ' - '+ FormulaDes as Text , Id as Value , FormulaDes as Formula from dbo.OTFormula ";
            return _sqlRepository.GetDataCollection(str);
        }

        public IEnumerable<object> getEmployees()
        {
            try
            {
                var str = @"select EmployeeCode , SystemId , EmployeeName from dbo.EmployeeInformation";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getEmpType()
        {
            try
            {
                var str = @"Select Username as Text , Id as Value from hkp.EmployeeCategory";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getMaster()
        {
            try
            {
                var str = @"Select * from dbo.DayStatusHeader";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getDayTypes()
        {
            try
            {
                var str = @"Select * from dbo.DayType";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getChildData(string MasterId)
        {
            try
            {
                var sql = @"Select * from dbo.DayStatusPlantChild where HeaderId ='" + MasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getDefaultDayStatus()
        {
            try
            {
                var str = @"Select Id as Value , UserName as Text from hkp.DefaultDayStatus";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // ********************************** The DataBase Operations 
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

        public Dictionary<string, object> saveMaster(Dictionary<string, object> Master)
        {
            try
            {
                string TableName = "dbo.DayStatusMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Master["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    Master["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], Master);
                }
                else
                {
                    _Id = Master["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], Master);
                }

                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Master;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Dictionary<string, object> saveChild(Dictionary<string, object> Child)
        {
            try
            {
                string TableName = "dbo.DayStatusPlantChild";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where PlantId ='" + Child["PlantId"] + "' and EmpTypeId ='" + Child["EmpTypeId"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    Child["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], Child);
                }
                else
                {
                    throw new Exception("Already same Combination is Present!");
                }

                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Child;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public string deleteMaster(string id)
        {
            try
            {


                string TableName = "dbo.DayStatusMaster";
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }

        public string DeleteChild(string id)
        {
            try
            {
                string TableName = "dbo.DayStatusPlantChild";
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
                con.CommitTransaction();
                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }

        public double GetSequence()
        {
            string TableName = "dbo.DayStatusHeader";
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }


        //******************************************* New Operations 

        //Getting the Header
        public IEnumerable<object> getHeader()
        {
            try
            {
                var str = @"Select * from dbo.DayStatusHeader";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Saving the New Header
        public Dictionary<string, object> saveHeader(Dictionary<string, object> Header)
        {
            try
            {
                string TableName = "dbo.DayStatusHeader";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id<>'" + Header["Id"] + "' and UserName='" + Header["UserName"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same UserName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id<>'" + Header["Id"] + "' and StandardName='" + Header["StandardName"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same StandardName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Header["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    Header["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], Header);
                }
                else
                {
                    _Id = Header["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], Header);
                }

                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Header;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //Getting the Header Auto Sequence
        public double GetSequenceHeader()
        {
            string TableName = "dbo.DayStatusHeader";
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }


        // Getting the Day Type Child 
        public IEnumerable<object> getDayTypeChild(string Id)
        {
            try
            {
                var str = @"Select * from dbo.DayTypeWithValues where HeaderId ='" + Id + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Saving the Day Type With Values

        public Dictionary<string, object> saveDayTypeChild(Dictionary<string, object> Header , List<Dictionary<string,object>> Leave)
        {
            try
            {
                string TableName = "dbo.DayTypeWithValues";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where HeaderId='" + Header["HeaderId"] + "' and DayType='" + Header["DayType"] + "' and Id<>'"+Header["Id"]+"'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same Day Type is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Header["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    Header["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], Header);
                }
                else
                {
                    _Id = Header["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], Header);
                }

                #endregion data update


                //Leave Day Type Code

                DataSet dsMaster1;
                ConnectionManager.DAL.ConManager con1 = new ConnectionManager.DAL.ConManager("1");
                con1.OpenDataSetThroughAdapter("select * from dbo.LeaveDayType where DayTypeWithValuesId='" + Header["Id"] + @"'", out dsMaster1, false, "1");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                for (int i = 0; i < Leave.Count; i++)
                {
                    //Data[i]["LDTId"].ToString();
                    dsMaster1.Tables[0].DefaultView.RowFilter = @"Id = '" + bplib.clsWebLib.RetValidLen(Leave[i]["LDTId"]).ToString() + "'";
                    if (dsMaster1.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = dsMaster1.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["EarnValue"] = clsStaticInfo.dbl(Leave[i]["EarnValue"].ToString());
                        dr["AvailedValue"] = clsStaticInfo.dbl(Leave[i]["AvailedValue"].ToString());
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                    else
                    {
                        DataRow dr = dsMaster1.Tables[0].NewRow();
                        string _Id1 = "";
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.LeaveDayType", out _Id1);
                        dr["Id"] = "LVDT-" + _Id1;
                        dr["DayTypeWithValuesId"] = Header["Id"];
                        dr["EarnValue"] = clsStaticInfo.dbl(Leave[i]["EarnValue"].ToString());
                        dr["AvailedValue"] = clsStaticInfo.dbl(Leave[i]["AvailedValue"].ToString());
                        dr["LeaveTypeId"] = Leave[i]["LTId"].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsMaster1.Tables[0].Rows.Add(dr);
                    }
                }
                //

                clsStaticInfo _info = new clsStaticInfo();
                //_info.SaveDataSets(dsMaster, dsMaster1);
                return Header;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //Getting the Day Status Child Auto Sequence
        public double GetAutoSequenceDayStatus()
        {
            string TableName = "dbo.DayStatus";
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        // Saving the Day Type With Values

        public Dictionary<string, object> saveDayStatusChild(Dictionary<string, object> Header)
        {
            try
            {
                string TableName = "dbo.DayStatus";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id<>'" + Header["Id"] + "' and Code='"+Header["Code"]+ "'  and HeaderId = '"+Header["HeaderId"]+"'", out dsMaster, false, "1");

                if(dsMaster.Tables[0].Rows.Count>0)
                {
                    throw new Exception("The Same Code is already Present!!");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Header["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    // Sequence
                    Header["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], Header);
                }
                else
                {
                    _Id = Header["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], Header);
                }

                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Header;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Getting the Day Status Child Data
        public IEnumerable<object> getDayStatusChild(string HeaderId)
        {
            try
            {
                var str= @"Select * from dbo.DayStatus where HeaderId='"+HeaderId+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        // ********************************************* Leave Day Type

        //Getting all the Day Types With Values
        public IEnumerable<object> getleaveDayTypes(string DayTypeWithValuesId)
        {
            try
            {
                var str = @"Select  lt.LeaveType , lt.Id as LTId , ld.Id as LDTId, isnull(ld.EarnValue,0.0) as EarnValue, isnull(ld.AvailedValue,0.0) as AvailedValue ,lt.UserName as Leave
                            from dbo.LeaveType lt
                            left join dbo.LeaveDayType ld on ld.LeaveTypeId =lt.Id and ld.DayTypeWithValuesId = '" + DayTypeWithValuesId+@"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        //Saving the Leave Day Type List
        public void saveLeaveDayType(List<Dictionary<string, object>> Data, string DayTypeWithValuesId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.LeaveDayType where DayTypeWithValuesId='"+DayTypeWithValuesId+@"'", out dsMaster, false, "1");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                for (int i = 0; i< Data.Count;i++)
                {
                    //Data[i]["LDTId"].ToString();
                    dsMaster.Tables[0].DefaultView.RowFilter = @"Id = '" + bplib.clsWebLib.RetValidLen(Data[i]["LDTId"]).ToString() + "'";
                    if(dsMaster.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["EarnValue"] = clsStaticInfo.dbl(Data[i]["EarnValue"].ToString());
                        dr["AvailedValue"] = clsStaticInfo.dbl(Data[i]["AvailedValue"].ToString());
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        string _Id = "";
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.LeaveDayType", out _Id);
                        dr["Id"] = "LVDT-" + _Id;
                        dr["DayTypeWithValuesId"] = DayTypeWithValuesId;
                        dr["EarnValue"] = clsStaticInfo.dbl(Data[i]["EarnValue"].ToString());
                        dr["AvailedValue"] = clsStaticInfo.dbl(Data[i]["AvailedValue"].ToString());
                        dr["LeaveTypeId"] = Data[i]["LTId"].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                }

                clsStaticInfo ins = new clsStaticInfo();
                ins.SaveDataSets(dsMaster);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
 