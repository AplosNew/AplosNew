using bplib;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class SandwichProcessService
    {

        ISqlRepository _sqlRepository;
        public SandwichProcessService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetEmployeeInformation(string month, string year)
        {
            try
            {
                int dd = 01;
                string jj = month.ToString() + "-" + dd.ToString() + "-" + year.ToString();
                string date = DateTime.Parse(jj).ToString("dd-MMM-yyyy");
               
                var sql = @"select EmpSystemID,e.EmployeeCode,p.UserName as Plant,p.Id as PlantId,
                            format(WorkDate,'dd-MMM-yyyy')WorkDate,DayStatus,dp.UserName
                            as Department,s.UserName as Section,
                            SuS.UserName as SubSection,ld.UserName as Designation,SandwichFlag as TodayFlag,
                (select SandwichFlag from AttdnProcessData where WorkDate=DATEADD(day,-1,a.WorkDate) 
                and EmpSystemID=a.EmpSystemID)PrevDayFlag
                from attdnprocessdata a
                left join EmployeeInformation e on e.SystemId=a.EmpSystemID
                left join org.Plant p on p.Id=e.PlantId
                left join org.Section s on s.Id=e.SectionId
                LEFT JOIN ORG.Department DP ON DP.Id = E.DepartmentId
                LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = E.SubSectionID
                left join hkp.LegalDesignation ld on ld.Id=e.LegalDesignationId
                where a.WorkDate between '" + date+@"' and GETDATE() and
                SandwichReprocess=1 order by EmpSystemID,Workdate,SandwichFlag asc";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public DataTable SandWichDataTable(string PlantId, string month, string year)
        {
            try
            {
                int dd = 01;
                string jj = month.ToString() + "-" + dd.ToString() + "-" + year.ToString();
                string date = DateTime.Parse(jj).ToString("dd-MMM-yyyy");
                var str = @"select EmpSystemID,a.RowId, e.EmployeeCode,format(WorkDate, 'dd-MMM-yyyy')WorkDate,DayStatus,SandwichFlag as TodayFlag,
                (select SandwichFlag from AttdnProcessData where WorkDate = DATEADD(day, -1, a.WorkDate)
                and EmpSystemID = a.EmpSystemID
                and a.PlantID = '" + PlantId+ @"')PrevDayFlag
                  from attdnprocessdata a
                left join EmployeeInformation e on e.SystemId = a.EmpSystemID
                where a.WorkDate between '" + date+@"' and GETDATE() and
                SandwichReprocess = 1 and e.PlantID = '"+PlantId+@"'
                order by EmpSystemID,Workdate,SandwichFlag asc";

                DataTable dtTemp = _sqlRepository.GetDataTable(str);
                return dtTemp;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void Process(string PlantId, string month, string year)
        {
            try
            {                
                DataTable SandwichData; // Build DataTable For Sandwich Process
                SandwichData = SandWichDataTable(PlantId, month, year);
                if (SandwichData.Rows.Count > 0)
                {
                    #region DataTable Generation 
                    
                    StringCollection StrDistinctWorkDate = new StringCollection();
                    var StringDates = new List<DateTime>();

                    for (int i = 0; i < SandwichData.Rows.Count; i++)
                    {
                        string WkDate = clsWebLib.RetValidLen(SandwichData.Rows[i][@"WorkDate"]).ToString();
                        if (StrDistinctWorkDate.Contains(WkDate))
                        {
                            continue;
                        }

                        StrDistinctWorkDate.Add(WkDate);
                        StringDates.Add(Convert.ToDateTime(WkDate));
                    }
                    string MaxDate = StringDates.Max(date => date).ToString("dd-MMM-yyyy");
                    string MinDate = StringDates.Min(date => date).ToString("dd-MMM-yyyy");

                    #endregion


                    for (int i = 0; i < SandwichData.Rows.Count; i++)
                    {                        
                       
                    }


                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void CheckerFunction(ref string TempMaster,ref string MainMaster, string Value)
        {
            if (TempMaster.Contains(Value))
            {
                return;
            }
            else
            {
                TempMaster += ",'" + Value + "'";
                MainMaster += ",'" + Value + "'";
            }
        }


        public void ManualFlagRows(string Past3rdDay,string Past2ndDay, string Yesterday, string Today,string Tomorrow,string Future2ndDay,string Future3rdDay)
        {

            try
            {
               // Command to Update the ManualFlag Trigger

               var sql = @"update attdnprocessdata set manualflag=1,LockedBy=null,LockedDate=null,IsLock=0,
                IsOTComfirm=0,OTComfirmBy=null,DateOTComfirm=null
                where rowid in("+Past3rdDay+") or RowId in("+Past2ndDay+") or RowId in("+Yesterday+") " +
                " or RowId in("+Today+") or RowId in("+Tomorrow+") or RowId in("+Future2ndDay+") or RowId in("+Future3rdDay+")";
                
                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
    }
}
 