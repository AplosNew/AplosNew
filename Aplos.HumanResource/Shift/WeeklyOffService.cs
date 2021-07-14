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

namespace Library.HumanResource.Shift
{
    public class WeeklyOffService
    {

        ISqlRepository _sqlRepository;
        public WeeklyOffService()
        {
            _sqlRepository = new SqlRepository();
        }


        public List<Dictionary<string, object>> ShiftDefinationSearch(string PlantId)
        {
            //ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT SystemID ShiftDefinationID, ShiftDefinationName, ShiftDefinationDescription, ShiftType, SequenceNo ShiftSequence, CONVERT(VARCHAR(10), InTime, 108) AS InTime,
                                        InTimeStartMargin, LateMargin, AbsentEndMargin, CONVERT(VARCHAR(10), OutTime, 108) AS OutTime,
                                        OutTimeEndMargin, OTStartTime, CONVERT(VARCHAR(10), BreakStratTime, 108) AS BreakStratTime,
                                        CONVERT(VARCHAR(10), BreakEndTime, 108) AS BreakEndTime, BreakPeriod, WorkingHour, IsActive, DefaultShift, IsGapInclude
                                FROM ShiftDefination WHERE  PlantID = '" + PlantId + @"' Order By ShiftDefinationName";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }

        public IEnumerable<object> getMaster()
        {
            try
            {
                var str = @"Select wo.*, format(wo.AddedDate,'dd-MMM-yyyy') as CreationDate
                            from dbo.WeekOffHeader wo";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getDateChild(string Id)
        {
            try
            {
                var str = @"Select Id, WOHeaderId, format(EffectiveDate,'dd-MMM-yyyy') as EffectiveDate from dbo.WeekOffEffectiveDate where WOHeaderId = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getDayChild(string Id)
        {
            try
            {
                var str = @"Select Id ,WOHeaderId,WOSequence, Day, DayType   from dbo.WeekOffChild where WOHeaderId = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        // The Section for Saving And Updating of Data
        public void AddNewRow(DataTable dt, Dictionary<string, object> sourceData, string addedname, string addeddate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            dr["AddedBy"] = addedname;
            dr["AddedDate"] = addeddate;
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }

        public string saveMasters(Dictionary<string, object> Master, List<Dictionary<string, object>> Effective)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string TableName = "dbo.WeekOffHeader";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Master["Id"] + "'", out dsMaster, false, "1");

                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();

                string DateId = ((DateTime.Now.Year).ToString()).Substring(2);
                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    addedname = identity.Name;
                    addeddate = System.DateTime.Now.ToString();
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    Master["Id"] = DateId + _Id.ToString().PadLeft(4, '0');
                    AddNewRow(dsMaster.Tables[0], Master, addedname, addeddate);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["StandardName"] = Master["StandardName"];
                    dr["ShortName"] = Master["ShortName"];
                    dr["Description"] = Master["Description"];
                    dr["Remarks"] = Master["Remarks"];
                    dr["Active"] = Master["Active"];
                    dr["UserName"] = Master["UserName"];
                    dr["AddedBy"] = dsMaster.Tables[0].Rows[0]["AddedBy"];
                    dr["AddedDate"] = dsMaster.Tables[0].Rows[0]["AddedDate"];
                    dr["AddedFromIP"] = dsMaster.Tables[0].Rows[0]["AddedFromIP"];
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }

                #endregion data update

                // The Effective Date Child Table entry


                string TableName1 = "dbo.WeekOffEffectiveDate";
                DataSet dsChild;
                ConnectionManager.DAL.ConManager con1 = new ConnectionManager.DAL.ConManager("1");
                con1.OpenDataSetThroughAdapter("select * from " + TableName1 + " where WOHeaderId='" + Master["Id"] + "'", out dsChild, false, "1");
                if (dsChild.Tables[0].Rows.Count == 0)
                {
                    int indexa = 0;
                    for (int i = 0; i < Effective.Count; i++)
                    {
                        Dictionary<string, object> jj = Effective[i];
                        indexa++;
                        jj["WOHeaderId"] = Master["Id"];
                        jj["Id"] = Master["Id"] + indexa.ToString().PadLeft(2, '0');
                        addedname = identity.Name;
                        addeddate = System.DateTime.Now.ToString();
                        AddNewRow(dsChild.Tables[0], jj, addedname, addeddate);
                    }
                }
                else
                {
                    
                    addedname = dsChild.Tables[0].Rows[0]["AddedBy"].ToString();
                    addeddate = dsChild.Tables[0].Rows[0]["AddedDate"].ToString();
                    for (int i = 0; i < dsChild.Tables[0].Rows.Count; i++)
                    {
                        dsChild.Tables[0].Rows[i].Delete();
                    }
                    dsChild.AcceptChanges();

                    int indexa = 0;
                    for (int i = 0; i < Effective.Count; i++)
                    {
                        Dictionary<string, object> jj = Effective[i];
                        indexa++;
                        jj["WOHeaderId"] = Master["Id"];
                        jj["Id"] = Master["Id"] + indexa.ToString().PadLeft(2, '0');

                        AddNewRow(dsChild.Tables[0], jj, addedname, addeddate);
                    }

                    var sqls = @"Delete from " + TableName1 + " where WOHeaderId = '" + Master["Id"] + "'";

                    ConnectionManager.DAL.ConManager objCone = null;
                    objCone = new ConnectionManager.DAL.ConManager("1");
                    objCone.OpenConnection("1");
                    objCone.BeginTransaction();

                    objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                    objCone.CommitTransaction();
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild);
                return Master["Id"].ToString();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //public string deleteMaster(string id)
        //{
        //    try
        //    {


        //        string TableName = "dbo.RosterPatternHeader";
        //        if (string.IsNullOrEmpty(id))
        //            throw new Exception("Select entry first");
        //        ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
        //        con.BeginTransaction();
        //        con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
        //        con.CommitTransaction();

        //        return "Success";

        //    }
        //    catch (Exception ex)
        //    {

        //        return ex.Message;

        //    }
        //}

        public void SaveDays(List<Dictionary<string, object>> Week , string HeaderId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();
                string TableName = "dbo.WeekOffChild";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where WOHeaderId='" + HeaderId + "'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    if(Week != null)
                    {
                        int indexa = 0;
                        for (int i = 0; i < Week.Count; i++)
                        {
                            Dictionary<string, object> jj = Week[i];
                            indexa++;
                            jj["Id"] = jj["WOHeaderId"] + indexa.ToString().PadLeft(2, '0');

                            AddNewRow(dsMaster.Tables[0], jj, addedname, addeddate);
                        }
                    }
                    else
                    {
                        throw new Exception("Please First Add Days!!");
                    }

                   
                }
                else
                {
                    if(Week != null)
                    {
                        addedname = dsMaster.Tables[0].Rows[0]["AddedBy"].ToString();
                        addeddate = dsMaster.Tables[0].Rows[0]["AddedDate"].ToString();
                        for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                        {
                            dsMaster.Tables[0].Rows[i].Delete();
                        }
                        dsMaster.AcceptChanges();

                        int indexa = 0;
                        for (int i = 0; i < Week.Count; i++)
                        {
                            Dictionary<string, object> jj = Week[i];
                            indexa++;
                            jj["Id"] = jj["WOHeaderId"] + indexa.ToString().PadLeft(2, '0');

                            AddNewRow(dsMaster.Tables[0], jj, addedname, addeddate);
                        }

                    }

                    var sqls = @"Delete from " + TableName + " where WOHeaderId = '" + HeaderId + "'";

                    ConnectionManager.DAL.ConManager objCone = null;
                    objCone = new ConnectionManager.DAL.ConManager("1");
                    objCone.OpenConnection("1");
                    objCone.BeginTransaction();

                    objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                    objCone.CommitTransaction();
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

       
    }
}