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
    public class WeekOffUpdatesService
    {

        ISqlRepository _sqlRepository;
        public WeekOffUpdatesService()
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

        public IEnumerable<object> getCurrentList()
        {
            try
            {
                var str = @"Select ROW_NUMBER() OVER(ORDER BY ew.EffectiveDate desc) as Rows,ew.EmpSystemId, ew.WOHeaderId,format(ew.EffectiveDate,'dd-MMM-yyyy') as EffectiveDate
                            , ew.Id, ei.SystemId,ei.EmployeeCode,
                            ei.EmployeeName, wo.UserName as WOName
                            from dbo.EmployeeWeeklyOff ew
                            left join dbo.EmployeeInformation ei on ei.SystemId = ew.EmpSystemId
                            left join dbo.WeekOffHeader wo on wo.Id = ew.WOHeaderId
                            order by CAST(ew.EffectiveDate as Date) desc
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

    

        public void SaveFileList(List<Dictionary<string,object>> data )
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();
                string TableName = "dbo.EmployeeWeeklyOff";
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

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
       
        public DataTable getEmployeeWeekRosterFile( )
        {
            try
            {
                var str = @"Select ei.EmployeeCode , ew.WOHeaderId as RosterId,format(ew.EffectiveDate,'dd-MMM-yyyy') as EffectiveDate
from dbo.EmployeeWeeklyOff ew
left join dbo.EmployeeInformation ei on ei.SystemId = ew.EmpSystemId
                            ";

                return _sqlRepository.GetDataTable(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public DataTable getEmployeesAll()
        {
            try
            {
                var str = @"Select SystemId, EmployeeCode from dbo.EmployeeInformation
                           ";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable getRostersFile()
        {
            try
            {
                var str = @"Select * from dbo.WeekOffHeader";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public List<string> getRostersList()
        {
            try
            {
                var str1 = @"Select Id from dbo.WeekOffHeader ";
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