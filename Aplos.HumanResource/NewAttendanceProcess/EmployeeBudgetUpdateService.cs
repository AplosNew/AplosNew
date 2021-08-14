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
    public class EmployeeBudgetUpdateService
    {

        ISqlRepository _sqlRepository;
        public EmployeeBudgetUpdateService()
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
            catch(Exception e)
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

    

        public void SaveFileList(List<Dictionary<string,object>> data , string plantId)
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
                        jj["Id"] = _Id ;

                        AddNewRow(dsMaster.Tables[0], jj, addedname, addeddate);
                    }


                }

                var sqls = @"Delete re from dbo.RosterEmployee re
                                left join dbo.EmployeeInformation ei on ei.SystemId = re.EmpSystemId
                                where ei.plantId = '" + plantId+@"'";

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
       
        public DataTable getEmployeeRosterFile(string plantId )
        {
            try
            {
                var str = @"Select re.* , ei.EmployeeCode , ei.SystemId from dbo.RosterEmployee re 
                            left join dbo.EmployeeInformation ei on ei.SystemId = re.EmpSystemId
                            where ei.PlantId = '" + plantId+"'";

                return _sqlRepository.GetDataTable(str);
            }
            catch(Exception e)
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
            catch(Exception e)
            {
                throw e;
            }
        }

       
    }
}