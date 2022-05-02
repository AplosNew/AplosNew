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
    public class ScatteredWeekService
    {

        ISqlRepository _sqlRepository;
        public ScatteredWeekService()
        {
            _sqlRepository = new SqlRepository();
        }

        #region Scattered Week Definition

        #region Frist Tab Getting Operations
        public IEnumerable<object> getWeeksList()
        {
            try
            {
                var str = @"Select Id as value , UserName as text from dbo.WeekOffHeader";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getCurrentWeekDef()
        {
            try
            {
                var str = @"Select Day, WOHeaderId from dbo.ScatteredWeekDefinition";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        public string SaveAllDef(Dictionary<string, string> data)
        {
            try
            {
                string TableName = " dbo.ScatteredWeekDefinition";
                DataSet dsMaster;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + "", out dsMaster, false, "1");

                string _Id = "";
                //List<string> jj = new List<string>(data.Keys);
                //List<string> kk = new List<string>(data.Values);
                List<String> myKeys = data.Keys.ToList();
                List<String> myValues = data.Values.ToList();
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    for (var i = 0; i < myKeys.Count; i++)
                    {
                        genid.GenID(TableName, out _Id);
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["Id"] = "ETO" + _Id;
                        dr["DayNos"] = i + 1;
                        dr["Day"] = myKeys[i].ToString();
                        dr["WOHeaderId"] = myValues[i].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                }
                else
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = @"Day='" + myKeys[i].ToString() + "'";
                        dsMaster.Tables[0].DefaultView[0].Row.BeginEdit();
                        dsMaster.Tables[0].DefaultView[0].Row["WOHeaderId"] = myValues[i].ToString();
                        dsMaster.Tables[0].DefaultView[0].Row["UpdatedBy"] = identity.Name;
                        dsMaster.Tables[0].DefaultView[0].Row["UpdatedDate"] = System.DateTime.Now.ToString();
                        dsMaster.Tables[0].DefaultView[0].Row["UpdatedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].DefaultView[0].Row.EndEdit();

                    }

                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }


        #endregion

        #region Scattered Week Master

        #region GetOperations
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

        public IEnumerable<object> getCurrWeeksList(string HeaderId)
        {
            try
            {
                var str = "";
                if(HeaderId==null)
                {
                     str = @"Select 0 as isApplicable,dd.* from 
                            (Select swd.Id,swd.Day , swd.WOHeaderId , sc.HeaderId
                            from dbo.ScatteredWeekDefinition swd
                            left join dbo.ScatteredWeekChild sc on sc.ScatteredWeekDefinitionId = swd.Id
                            left join dbo.ScatteredWeekHeader sh on sh.Id = sc.HeaderId
                            ) dd";
                }
                else
                {
                    str = @"Select (Case when dd.HeaderId is null then 0 else 1 end) isApplicable,* from
                            (
                            Select swd.* , 
                            (Select HeaderId from dbo.ScatteredWeekChild 
                            where ScatteredWeekDefinitionId = swd.Id and HeaderId = '" + HeaderId + @"') as HeaderId
                            from dbo.ScatteredWeekDefinition swd
                            )dd";
                }

                
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getMasterData()
        {
            try
            {
                var str = @"Select swh.*,p.UserName as PlantName , c.Id as CompanyId , c.UserName as Company
                            from dbo.ScatteredWeekHeader swh
                            left join Org.Plant p on p.Id = swh.PlantId
                            left join Org.Company c on c.Id = p.CompanyId";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

       
        #endregion

        #region Saving

        public string Create(Dictionary<string, string> masterData, List<Dictionary<string, string>> childData)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                #region MasterTable
                string TableName = " dbo.ScatteredWeekHeader";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where PlantId='" + masterData["PlantId"] + "' AND  UserName='" + masterData["UserName"] + "' And Id<>'" + masterData["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Entry already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + masterData["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                string masterID = "";
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "SW" + _Id;
                    masterID = dr["Id"].ToString();
                    dr["PlantId"] = masterData["PlantId"];
                    dr["StandardName"] = masterData["StandardName"];
                    dr["UserName"] = masterData["UserName"];
                    dr["MaxBudgetNumber"] = clsStaticInfo.dbl(masterData["MaxBudgetNumber"]);
                    dr["Remarks"] = masterData["Remarks"];
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    _Id = masterData["Id"].ToString();
                    masterID = masterData["Id"].ToString();
                    dsMaster.Tables[0].Rows[0].BeginEdit();
                    dsMaster.Tables[0].Rows[0]["Id"] = masterData["Id"].ToString();
                    dsMaster.Tables[0].Rows[0]["PlantId"] = masterData["PlantId"];
                    dsMaster.Tables[0].Rows[0]["UserName"] = masterData["UserName"];
                    dsMaster.Tables[0].Rows[0]["StandardName"] = masterData["StandardName"];
                    dsMaster.Tables[0].Rows[0]["MaxBudgetNumber"] = clsStaticInfo.dbl(masterData["MaxBudgetNumber"]);
                    dsMaster.Tables[0].Rows[0]["Remarks"] = masterData["Remarks"];
                    dsMaster.Tables[0].Rows[0]["UpdatedBy"] = identity.Name;
                    dsMaster.Tables[0].Rows[0]["UpdatedDate"] = System.DateTime.Now.ToString();
                    dsMaster.Tables[0].Rows[0]["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows[0].EndEdit();
                }
                #endregion data update

                #endregion

                #region ChildTable

                string TableName1 = "dbo.ScatteredWeekChild";
                DataSet dsChild;
                ConnectionManager.DAL.ConManager conChild = new ConnectionManager.DAL.ConManager("1");
                conChild.OpenDataSetThroughAdapter("select * from " + TableName1 + " where HeaderId='" + masterID + "'", out dsChild, false, "1");

                while(dsChild.Tables[0].DefaultView.Count > 0)
                {
                    dsChild.Tables[0].DefaultView[0].Delete();
                }

                for(int i = 0;i<childData.Count;i++)
                {
                    var j = childData[i];
                    DataRow dr = dsChild.Tables[0].NewRow();
                    dr["Id"] = masterID + i.ToString();
                    dr["HeaderId"] = masterID;
                    dr["ScatteredWeekDefinitionId"] = j["Id"];
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsChild.Tables[0].Rows.Add(dr);
                }

                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster,dsChild);

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
                
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                ConnectionManager.clsConnection con1 = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.ScatteredWeekChild where HeaderId='" + id + "'");
                con.CommitTransaction();
                con1.BeginTransaction();
                con1.executeQuery("delete from dbo.ScatteredWeekHeader where Id='" + id + "'");
                con1.CommitTransaction();
                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }

        #endregion

        #endregion

    }

}