using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class OutPunchConfigurationService
    {

        ISqlRepository _sqlRepository;
        public OutPunchConfigurationService()
        {
            _sqlRepository = new SqlRepository();
        }
    
        public IEnumerable<object> Get(string id)
        {
            try
            {
                var str = "select * from dbo.OutpunchConfigurationHeader where Id = '"+id+"'";


                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> GetChild(string id)
        {
            try
            {
                var str = "select * from dbo.OutpunchConfigurationChild where MasterId = '" + id + "'";


                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> GetList()
        {
            try
            {
                string sql = @"select top 100 * from (SELECT * FROM dbo.OutpunchConfigurationHeader) AS TEMP  order by sequence";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public Dictionary<string, object>  Create(Dictionary<string, object> data, List<Dictionary<string, object>> child)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.OutpunchConfigurationHeader where PlantId='" + data["PlantId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Plant already exists!!!");

                con.OpenDataSetThroughAdapter("select * from dbo.OutpunchConfigurationHeader where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from dbo.OutpunchConfigurationHeader where Id='" + data["Id"] + "'", out dsMaster, false, "1");


                // Updating the InPunchStart and LastOutPunch Times
                DateTime hIn = Convert.ToDateTime(data["InPunchStartTime"].ToString());
                DateTime hOut = Convert.ToDateTime(data["LastPunchOutTime"].ToString());
                string hInn = hIn.ToString("h:mm tt");
                string hOutt = hOut.ToString("h:mm tt");
                data["InPunchLimit"] = Convert.ToDateTime(hInn.ToString());
                data["OutPunchLimit"] = Convert.ToDateTime(hOutt.ToString());
                //

                DateTime dtInT = Convert.ToDateTime(data["InPunchStartTime"].ToString());
                DateTime dtOutT = Convert.ToDateTime(data["LastPunchOutTime"].ToString());

                //Converting the Date Time for Child
                for(int i = 0; i<child.Count;i++)
                {
                    Dictionary<string, object> k = child[i];
                    DateTime kIn = Convert.ToDateTime(k["InPunchLimit"].ToString());
                    DateTime kOut = Convert.ToDateTime(k["OutPunchLimit"].ToString());
                    string kInn = kIn.ToString("h:mm tt");
                    string kOutt = kOut.ToString("h:mm tt");
                    k["InPunchLimit"] = Convert.ToDateTime(kInn.ToString());
                    k["OutPunchLimit"]= Convert.ToDateTime(kOutt.ToString());
                }

                if( (dtOutT-dtInT).TotalHours < 11)
                {
                    dtOutT = dtOutT.AddDays(1);
                    data["LastPunchOutTime"] = dtOutT;
                }

               

                DateTime chInT = DateTime.Now;
                DateTime chOutT = DateTime.Now;

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("dbo.OutpunchConfigurationHeader", out _Id);

                    
                        
                    

                    data["Id"] = "S" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                //Saving of the Child Data

                string TableName1 = "dbo.OutPunchConfigurationChild";
                DataSet dsChild;
                ConnectionManager.DAL.ConManager con1 = new ConnectionManager.DAL.ConManager("1");
                con1.OpenDataSetThroughAdapter("select * from " + TableName1 + " where MasterId='" + data["Id"] + "'", out dsChild, false, "1");
                if (dsChild.Tables[0].Rows.Count == 0)
                {
                    int indexa = 0;
                    for (int i = 0; i < child.Count; i++)
                    {

                        Dictionary<string, object> jj = child[i];

                         chInT = Convert.ToDateTime(jj["InPunchLimit"].ToString());
                         chOutT = Convert.ToDateTime(jj["OutPunchLimit"].ToString());
                        //if (chInT > chOutT || chInT == chOutT)
                        //{
                        //    chOutT = chOutT.AddDays(1);
                        //    jj["OutPunchLimit"] = chOutT;
                        //}
                        //if (chOutT > dtOutT || chOutT< chInT)
                        //{
                        //    throw new Exception("The Punch Limits are not correct for Sequence - " + jj["Sequence"]);
                        //}
                        //if(chInT < dtInT || chInT > chOutT)
                        //{
                        //    throw new Exception("The Punch Limits are not correct for Sequence - " + jj["Sequence"]);
                        //}
                        if (chInT > chOutT || chInT == chOutT)
                        {
                            chOutT = chOutT.AddDays(1);
                            jj["OutPunchLimit"] = chOutT;
                        }
                        if (chOutT > dtOutT || chOutT < chInT)
                        {
                            throw new Exception("The Punch Limits are not correct for Sequence - " + jj["Sequence"]);
                        }

                        if (chInT < dtInT)
                        {
                            chInT = chInT.AddDays(1);
                            jj["InPunchLimit"] = chInT;
                            chOutT = chOutT.AddDays(1);
                            jj["OutPunchLimit"] = chOutT;

                            if (chInT > chOutT || chOutT > dtOutT || chInT > dtOutT)
                            {
                                throw new Exception("The Punch Limits are not correct for Sequence - " + jj["Sequence"]);
                            }

                        }


                        indexa++;
                        jj["MasterId"] = data["Id"];
                        jj["Id"] = data["Id"] + indexa.ToString().PadLeft(2, '0');
                        
                        AddNewRow(dsChild.Tables[0], jj);
                    }
                }
                else
                {
                    for (int i = 0; i < dsChild.Tables[0].Rows.Count; i++)
                    {
                        dsChild.Tables[0].Rows[i].Delete();
                    }
                    dsChild.AcceptChanges();

                    int indexa = 0;
                    for (int i = 0; i < child.Count; i++)
                    {
                        Dictionary<string, object> jj = child[i];
                        chOutT = Convert.ToDateTime(jj["OutPunchLimit"].ToString());
                        chInT = Convert.ToDateTime(jj["InPunchLimit"].ToString());
                        //ChIN - dtInt < days+1  Cint < chout >
                        if (chInT > chOutT || chInT == chOutT)
                        {
                            chOutT = chOutT.AddDays(1);
                            jj["OutPunchLimit"] = chOutT;
                        }
                        if (chOutT > dtOutT || chOutT < chInT)
                        {
                            throw new Exception("The Punch Limits are not correct for Sequence - " + jj["Sequence"]);
                        }
                        
                        if(chInT < dtInT)
                        {
                            chInT = chInT.AddDays(1);
                            jj["InPunchLimit"] = chInT;
                            chOutT = chOutT.AddDays(1);
                            jj["OutPunchLimit"] = chOutT;

                            if(chInT > chOutT || chOutT > dtOutT || chInT > dtOutT)
                            {
                                throw new Exception("The Punch Limits are not correct for Sequence - " + jj["Sequence"]);
                            }

                        }
                        
                        

                        indexa++;
                        jj["MasterId"] = data["Id"];
                        jj["Id"] = data["Id"] + indexa.ToString().PadLeft(2, '0');

                        AddNewRow(dsChild.Tables[0], jj);
                    }
                    var sqls = @"Delete from " + TableName1 + " where MasterId = '" + data["Id"] + "'";

                    ConnectionManager.DAL.ConManager objCone = null;
                    objCone = new ConnectionManager.DAL.ConManager("1");
                    objCone.OpenConnection("1");
                    objCone.BeginTransaction();

                    objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                    objCone.CommitTransaction();

                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild);

                return  data;

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        public void Delete(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.OutpunchConfigurationHeader where id='" + id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {

                throw ex;

            }
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
    }
    
}
