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
    public class RosterPatternService
    {

        ISqlRepository _sqlRepository;
        public RosterPatternService()
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

        public IEnumerable<object> getPlants(string cmp)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"Select Username as Text , Id as Value from ORG.Plant where CompanyId = '" + cmp + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getCompany()
        {
            try
            {
               
                var str = @"Select Username as Text , Id as Value from ORG.Company ";
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
                var str = @"Select rp.Id, rp.ShortName, rp.StandardName, rp.UserName,rp.Description, rp.Remarks,rp.Active, PlantId, p.Username as PlantName, format(rp.AddedDate,'dd-MMM-yyyy') as CreationDate
                            from dbo.RosterPatternHeader rp
                            left join org.plant p on p.Id = rp.PlantId ";
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
                var str = @"Select Id, RPHeaderId, format(EffectiveDate,'dd-MMM-yyyy') as EffectiveDate from dbo.RosterEffectiveDate where RPHeaderId = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getShiftChild(string Id)
        {
            try
            {
                var str = @"Select rpc.Id ,RPHeaderId, ShiftDefinitionID, Days31 as ShiftSequence, Days31, Days30,Days29,Days28 , sd.ShiftDefinationName as ShiftName ,rpc.WeeklyStatusId,WS.UserName WeeklyStatusName
                    FROM dbo.RosterPatternChild rpc
                    LEFT JOIN dbo.ShiftDefination sd on sd.SystemID = rpc.ShiftDefinitionID
                    LEFT JOIN HKP.WeeklyStatus WS ON WS.Id=rpc.WeeklyStatusId
                    WHERE RPHeaderId = '" + Id + "' ";
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
                catch (Exception)
                {
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

                string TableName = "dbo.RosterPatternHeader";
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
                    dr["PlantId"] = Master["PlantId"];
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


                string TableName1 = "dbo.RosterEffectiveDate";
                DataSet dsChild;
                ConnectionManager.DAL.ConManager con1 = new ConnectionManager.DAL.ConManager("1");
                con1.OpenDataSetThroughAdapter("select * from " + TableName1 + " where RPHeaderId='" + Master["Id"] + "'", out dsChild, false, "1");
                if (dsChild.Tables[0].Rows.Count == 0)
                {
                    int indexa = 0;
                    for (int i = 0; i < Effective.Count; i++)
                    {
                        Dictionary<string, object> jj = Effective[i];
                        indexa++;
                        jj["RPHeaderId"] = Master["Id"];
                        jj["Id"] = Master["Id"] + indexa.ToString().PadLeft(2, '0');
                        addedname = identity.Name;
                        addeddate = System.DateTime.Now.ToString();
                        AddNewRow(dsChild.Tables[0], jj, addedname, addeddate);
                    }
                }
                else
                {
                    var sqls = @"Delete from " + TableName1 + " where RPHeaderId = '" + Master["Id"] + "'";

                    ConnectionManager.DAL.ConManager objCone = null;
                    objCone = new ConnectionManager.DAL.ConManager("1");
                    objCone.OpenConnection("1");
                    objCone.BeginTransaction();

                    objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                    objCone.CommitTransaction();
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
                        jj["RPHeaderId"] = Master["Id"];
                        jj["Id"] = Master["Id"] + indexa.ToString().PadLeft(2, '0');

                        AddNewRow(dsChild.Tables[0], jj, addedname, addeddate);
                    }

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

        public void saveShifts(List<Dictionary<string, object>> Shifts , string HeaderId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();
                string TableName = "dbo.RosterPatternChild";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where RPHeaderId='" + HeaderId + "'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    if(Shifts != null)
                    {
                        int indexa = 0;
                        for (int i = 0; i < Shifts.Count; i++)
                        {
                            Dictionary<string, object> jj = Shifts[i];
                            indexa++;
                            jj["Id"] = jj["RPHeaderId"] + indexa.ToString().PadLeft(2, '0');

                            AddNewRow(dsMaster.Tables[0], jj, addedname, addeddate);
                        }
                    }
                    else
                    {
                        throw new Exception("Please First Add Shifts!!");
                    }

                   
                }
                else
                {
                    if(Shifts != null)
                    {
                        addedname = dsMaster.Tables[0].Rows[0]["AddedBy"].ToString();
                        addeddate = dsMaster.Tables[0].Rows[0]["AddedDate"].ToString();
                        for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                        {
                            dsMaster.Tables[0].Rows[i].Delete();
                        }
                        dsMaster.AcceptChanges();

                        int indexa = 0;
                        for (int i = 0; i < Shifts.Count; i++)
                        {
                            Dictionary<string, object> jj = Shifts[i];
                            indexa++;
                            jj["Id"] = jj["RPHeaderId"] + indexa.ToString().PadLeft(2, '0');

                            AddNewRow(dsMaster.Tables[0], jj, addedname, addeddate);
                        }

                    }

                    var sqls = @"Delete from " + TableName + " where RPHeaderId = '" + HeaderId + "'";

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

        //The Apis for the 2nd Page

        public IEnumerable<object> getCurrentList(string plantId)
        {
            try
            {
                var str = @"Select ROW_NUMBER() Over(Order by BudgetId) as Rows,rb.* from dbo.RosterBudget rb
                            left join dbo.RosterPatternHeader rh on rh.Id = rb.RosterId
                            where rh.plantId = '" + plantId+"'";
                return (_sqlRepository.GetDataCollection(str));
            }
            catch(Exception e)
            {
                throw e;
            }
        }


        public void SaveFileList(List<Dictionary<string,object>> data , string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();
                string TableName = "dbo.RosterBudget";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where 1 = 2", out dsMaster, false, "1");

                string _Id = "";
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    int indexa = 0;
                    for (int i = 0; i < data.Count; i++)
                    {
                        Dictionary<string, object> jj = data[i];
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);
                        indexa++;
                        jj["Id"] = _Id ;

                        AddNewRow(dsMaster.Tables[0], jj, addedname, addeddate);
                    }


                }

                var sqls = @"Delete rb from dbo.RosterBudget rb
                                left join dbo.RosterPatternHeader rp on rp.Id = rb.RosterId
                                where plantId = '"+plantId+@"'";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                objCone.CommitTransaction();

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
       
        public DataTable getRosterBudgetFile(string plantId )
        {
            try
            {
                var str = @"Select isnull(rb.RosterId,'') as RosterId, mb.Id as BudgetId , mb.Code as BudgetCode , pl.UserName as Plant ,e.UserName as Entity, p.UserName as Position
                            from mst.ManPowerBudget mb
                            left join org.Entity e on e.Id = mb.EntityId
                            left join org.Plant pl on pl.Id = e.PlantId
                            left join org.Position p on p.Id = mb.PositionId
                            left join dbo.RosterBudget rb on rb.BudgetId = mb.Id
                            where pl.Id = '"+plantId+@"'
                            ";

                return _sqlRepository.GetDataTable(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public List<string> getRostersList(string plantId)
        {
            try
            {
                var str1 = @"Select Id from dbo.RosterPatternHeader where PlantId = '" + plantId + "'";
                DataTable dt = _sqlRepository.GetDataTable(str1);

                List<string> roster = new List<string>();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        roster.Add(dt.Rows[i]["Id"].ToString());
                    }
                }

                return roster;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// ///////
        /// </summary>
        //Roster Process Trial To See how it works
        ////////??
        ///
        /////
        public void Add(DataTable dt, Dictionary<string, object> sourceData)
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
            dr["DateAdded"] = System.DateTime.Now.ToString(); ;
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["DateUpdated"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }


        public void run(string PlantId)
        {
            try
            {
                clsStaticInfo _info = new clsStaticInfo();
                //var sql1 = @"Select * from Org.Plant";
                //DataTable dt = new DataTable();
                //

                //dt = _sqlRepository.GetDataTable(sql1);
                ////1st Loop which works Plant Wise and gets all the rosters of that particular plant
                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                var sql2 = @"Select * from dbo.RosterPatternHeader where PlantId = '" +PlantId + "'";
                    DataTable RosterTable = new DataTable();
                    RosterTable = _sqlRepository.GetDataTable(sql2);
                    if (RosterTable.Rows.Count > 0)
                    {
                        //Loop to go through all the Rosters in a Plant
                        for (int j = 0; j < RosterTable.Rows.Count; j++)
                        {

                            //Getting all the Shifts Child 
                            var sql3 = @"Select * from dbo.RosterPatternChild where RPHeaderId = '" + RosterTable.Rows[j]["Id"].ToString() + "' order by ShiftSequence";
                            DataTable ShiftsTable = new DataTable();
                            ShiftsTable = _sqlRepository.GetDataTable(sql3);



                            int maxSeq = ShiftsTable.Rows.Count;
                            if (maxSeq == 0)
                            {
                                continue;
                            }
                            else
                            {
                                string _Id = "";
                                //Get the top Nearest Effective Date
                                DateTime Today = DateTime.Now;
                                String noww = DateTime.Now.ToString("dd-MMM-yyyy");
                                var sql4 = @"Select top 1 ed.*, rp.PlantId from dbo.RosterEffectiveDate ed
                                                left join dbo.RosterPatternHeader rp on rp.Id = ed.RPHeaderId
                                                 where RPHeaderId = '" + RosterTable.Rows[j]["Id"].ToString() + "' and EffectiveDate <= '" + noww + "' order by EffectiveDate desc";

                                DataTable EffectiveDateTable = new DataTable();
                                EffectiveDateTable = _sqlRepository.GetDataTable(sql4);

                                //Getting all the rows from the Process table
                                var sql5 = @"Select top 1 * from dbo.RosterPatternProcess where RPHeaderId = '" + RosterTable.Rows[j]["Id"].ToString() + "' and PlantId = '" +PlantId+"' order by WorkDate";
                                DataTable ProcessTable = new DataTable();
                                ProcessTable = _sqlRepository.GetDataTable(sql5);

                                //Dictionary and DataSet Initialization
                                DataSet ds;
                                ConnectionManager.DAL.ConManager cona = new ConnectionManager.DAL.ConManager("1");
                                cona.OpenDataSetThroughAdapter("select * from RosterPatternProcess where 1 = 2", out ds, false, "1");

                                Dictionary<string, object> dict = InitializeMyDictionary();

                                // Conditions...
                                int DateDifference = -1;
                                if (EffectiveDateTable.Rows.Count > 0)
                                {
                                    DateTime EffecDate = Convert.ToDateTime(EffectiveDateTable.Rows[0]["EffectiveDate"].ToString());
                                    DateDifference = (int)(Today - EffecDate).Days;
                                }

                                if (DateDifference == 0)// If today is an Effective Date
                                {
                                    bplib.clsGenID genid = new bplib.clsGenID();
                                    genid.GenID("dbo.RosterPatternProcess", out _Id);
                                    dict["Id"] = "RP" + _Id;
                                    dict["RPHeaderId"] = RosterTable.Rows[j]["Id"].ToString();
                                    dict["PlantId"] = PlantId;
                                    dict["WorkDate"] = Convert.ToDateTime(Today);
                                    dict["ShiftDefinationID"] = ShiftsTable.Rows[0]["ShiftDefinitionID"].ToString();
                                    dict["ShiftSequence"] = ShiftsTable.Rows[0]["ShiftSequence"].ToString();
                                    Add(ds.Tables[0], dict);
                                }
                                else
                                {
                                    //Check for the nearest Previous Date;
                                    if (EffectiveDateTable.Rows.Count > 0)
                                    {
                                        DateTime EffecDates = Convert.ToDateTime(EffectiveDateTable.Rows[0]["EffectiveDate"].ToString());
                                        double DayDiffs = (Today - EffecDates).Days;
                                        int Seq = (int)(DayDiffs % maxSeq); // The Sequence of Shift to be inserted Today

                                        bplib.clsGenID genid = new bplib.clsGenID();
                                        genid.GenID("dbo.RosterPatternProcess", out _Id);
                                        dict["Id"] = "RP" + _Id;
                                        dict["RPHeaderId"] = RosterTable.Rows[j]["Id"].ToString();
                                        dict["PlantId"] = PlantId;
                                        dict["WorkDate"] = Convert.ToDateTime(Today);
                                        dict["ShiftDefinationID"] = ShiftsTable.Rows[Seq]["ShiftDefinitionID"].ToString();
                                        dict["ShiftSequence"] = ShiftsTable.Rows[Seq]["ShiftSequence"].ToString();
                                        //We will make the Row and insert into the Table.
                                        Add(ds.Tables[0], dict);
                                    }
                                    else // In case there are no previous date Either, it will be an Exceptional Case.
                                    {
                                        continue;
                                    }

                                }


                                _info.SaveDataSets(ds);
                            }

                        }
                    }

                //}

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static Dictionary<string, object> InitializeMyDictionary()
        {
            Dictionary<string, object> ds = new Dictionary<string, object>
        {
            { "Id", "" },
            { "PlantId", "" },
            { "RPHeaderId", "" },
            { "WorkDate", "" },
            { "ShiftDefinationID", "" },
            { "ShiftSequence", "" },
        };
            return ds;
        }


        public void runTest(string PlantId)
        {
            try
            {
                clsStaticInfo _info = new clsStaticInfo();
               
                var sql2 = @"Select * from dbo.RosterPatternHeader where PlantId = '" + PlantId + "'";
                DataTable RosterTable = new DataTable();
                RosterTable = _sqlRepository.GetDataTable(sql2);
                if (RosterTable.Rows.Count > 0)
                {
                    //Loop to go through all the Rosters in a Plant
                    for (int j = 0; j < RosterTable.Rows.Count; j++)
                    {
                        string DaysCol = "Days"+DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month).ToString();

                        //Getting all the Shifts Child 
                        var sql3 = @"Select *, "+DaysCol+" as ShiftSequence from dbo.RosterPatternChild where RPHeaderId = '" + RosterTable.Rows[j]["Id"].ToString() + "' order by ShiftSequence";
                        DataTable ShiftsTable = new DataTable();
                        ShiftsTable = _sqlRepository.GetDataTable(sql3);

                        //Getting the Max Sequence through the Use of Dynamic Months
                        var maxS = @"Select top 1 * from dbo.RosterPatternChild where RPHeaderId = '" + RosterTable.Rows[j]["Id"].ToString() + "' order by "+DaysCol+@"";
                        DataTable MaxSTable = new DataTable();
                        MaxSTable = _sqlRepository.GetDataTable(maxS);

                        
                        if (MaxSTable.Rows.Count == 0)
                        {
                            continue;
                        }
                        else
                        {
                            int maxSeq = int.Parse(MaxSTable.Rows[0][DaysCol].ToString());
                            string _Id = "";
                            //Get the top Nearest Effective Date
                            DateTime Today = DateTime.Now;
                            String noww = DateTime.Now.ToString("dd-MMM-yyyy");
                            var sql4 = @"Select top 1 ed.*, rp.PlantId from dbo.RosterEffectiveDate ed
                                                left join dbo.RosterPatternHeader rp on rp.Id = ed.RPHeaderId
                                                 where RPHeaderId = '" + RosterTable.Rows[j]["Id"].ToString() + "' and EffectiveDate <= '" + noww + "' order by EffectiveDate desc";

                            DataTable EffectiveDateTable = new DataTable();
                            EffectiveDateTable = _sqlRepository.GetDataTable(sql4);

                            //Getting all the rows from the Process table
                            var sql5 = @"Select top 1 * from dbo.RosterPatternProcess where RPHeaderId = '" + RosterTable.Rows[j]["Id"].ToString() + "' and PlantId = '" + PlantId + "' order by WorkDate";
                            DataTable ProcessTable = new DataTable();
                            ProcessTable = _sqlRepository.GetDataTable(sql5);

                            //Dictionary and DataSet Initialization
                            DataSet ds;
                            ConnectionManager.DAL.ConManager cona = new ConnectionManager.DAL.ConManager("1");
                            cona.OpenDataSetThroughAdapter("select * from RosterPatternProcess where 1 = 2", out ds, false, "1");

                            Dictionary<string, object> dict = InitializeMyDictionary();

                            // Conditions...
                            int DateDifference = -1;
                            if (EffectiveDateTable.Rows.Count > 0)
                            {
                                DateTime EffecDate = Convert.ToDateTime(EffectiveDateTable.Rows[0]["EffectiveDate"].ToString());
                                DateDifference = (int)(Today - EffecDate).Days;
                            }

                            if (DateDifference == 0)// If today is an Effective Date
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID("dbo.RosterPatternProcess", out _Id);
                                dict["Id"] = "RP" + _Id;
                                dict["RPHeaderId"] = RosterTable.Rows[j]["Id"].ToString();
                                dict["PlantId"] = PlantId;
                                dict["WorkDate"] = Convert.ToDateTime(Today);
                                dict["ShiftDefinationID"] = ShiftsTable.Rows[0]["ShiftDefinitionID"].ToString();
                                dict["ShiftSequence"] = ShiftsTable.Rows[0]["ShiftSequence"].ToString();
                                Add(ds.Tables[0], dict);
                            }
                            else
                            {
                                //Check for the nearest Previous Date;
                                if (EffectiveDateTable.Rows.Count > 0)
                                {
                                    DateTime EffecDates = Convert.ToDateTime(EffectiveDateTable.Rows[0]["EffectiveDate"].ToString());
                                    double DayDiffs = (Today - EffecDates).Days;
                                    int Seq = (int)(DayDiffs % maxSeq); // The Sequence of Shift to be inserted Today

                                    bplib.clsGenID genid = new bplib.clsGenID();
                                    genid.GenID("dbo.RosterPatternProcess", out _Id);
                                    dict["Id"] = "RP" + _Id;
                                    dict["RPHeaderId"] = RosterTable.Rows[j]["Id"].ToString();
                                    dict["PlantId"] = PlantId;
                                    dict["WorkDate"] = Convert.ToDateTime(Today);
                                    dict["ShiftDefinationID"] = ShiftsTable.Rows[Seq]["ShiftDefinitionID"].ToString();
                                    dict["ShiftSequence"] = ShiftsTable.Rows[Seq]["ShiftSequence"].ToString();
                                    //We will make the Row and insert into the Table.
                                    Add(ds.Tables[0], dict);
                                }
                                else // In case there are no previous date Either, it will be an Exceptional Case.
                                {
                                    continue;
                                }

                            }


                            _info.SaveDataSets(ds);
                        }

                    }
                }

                //}

            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }

    public class RosterUpdatesService
    {

        ISqlRepository _sqlRepository;
        public RosterUpdatesService()
        {
            _sqlRepository = new SqlRepository();
        }


        public IEnumerable<object> getPlants()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"Select Username as Text , Id as Value from ORG.Plant where CompanyId = '" + identity.CompanyId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getCurrentList(string plantId)
        {
            try
            {
                var str = @"Select ROW_NUMBER() OVER(ORDER BY Id ASC) as Rows,re.EmpSystemId, re.RosterId, re.Id, ei.SystemId,ei.EmployeeCode from dbo.RosterEmployee re
                            left join dbo.EmployeeInformation ei on ei.SystemId = re.EmpSystemId
                                ";
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
                catch (Exception)
                {
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



        //The Apis for the 2nd Page



        public void SaveFileList(List<Dictionary<string, object>> data, string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();
                string TableName = "dbo.RosterEmployee";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where 1 = 2", out dsMaster, false, "1");

                string _Id = "";
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    int indexa = 0;
                    for (int i = 0; i < data.Count; i++)
                    {
                        Dictionary<string, object> jj = data[i];
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);
                        indexa++;
                        jj["Id"] = _Id;

                        AddNewRow(dsMaster.Tables[0], jj, addedname, addeddate);
                    }


                }

                var sqls = @"Delete re from dbo.RosterEmployee re
                                left join dbo.EmployeeInformation ei on ei.SystemId = re.EmpSystemId
                                where ei.plantId = '" + plantId + @"'";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                objCone.CommitTransaction();

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable getEmployeeRosterFile(string plantId)
        {
            try
            {
                var str = @"Select re.* , ei.EmployeeCode , ei.SystemId from dbo.RosterEmployee re 
                            left join dbo.EmployeeInformation ei on ei.SystemId = re.EmpSystemId
                            where ei.PlantId = '" + plantId + "'";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable getEmployeesAll(string plantId)
        {
            try
            {
                var str = @"Select SystemId, EmployeeCode from dbo.EmployeeInformation
                            where PlantId = '" + plantId + "'";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable getRostersFile(string plantId)
        {
            try
            {
                var str = @"Select * from dbo.RosterPatternHeader
                            where PlantId = '" + plantId + "'";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public List<string> getRostersList(string plantId)
        {
            try
            {
                var str1 = @"Select Id from dbo.RosterPatternHeader where PlantId = '" + plantId + "'";
                DataTable dt = _sqlRepository.GetDataTable(str1);

                List<string> roster = new List<string>();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        roster.Add(dt.Rows[i]["Id"].ToString());
                    }
                }

                return roster;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


    }
}