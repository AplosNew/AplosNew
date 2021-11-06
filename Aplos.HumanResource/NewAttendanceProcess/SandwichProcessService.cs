using bplib;
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
    public class SandwichProcessService
    {

        ISqlRepository _sqlRepository;
        public SandwichProcessService()
        {
            _sqlRepository = new SqlRepository();
        }


        public IEnumerable<object> GetEmployeeInformation(string month , string year)
        {
            try
            {
                int dd = 01;
                string jj = month.ToString() + "-" + dd.ToString() + "-" + year.ToString();
                string date = DateTime.Parse(jj).ToString("dd-MMM-yyyy");
                var str = @"select EmpSystemID,e.EmployeeCode,p.UserName as Plant,p.Id as PlantId,
                            format(WorkDate,'dd-MMM-yyyy')WorkDate,DayStatus,dp.UserName
                            as Department,s.UserName as Section,
                            SuS.UserName as SubSection,ld.UserName as Designation
                            from AttdnProcessData a
                            left join EmployeeInformation e on e.SystemId=a.EmpSystemID
                            left join org.Plant p on p.Id=e.PlantId
                            left join org.Section s on s.Id=e.SectionId
                            LEFT JOIN ORG.Department DP ON DP.Id = E.DepartmentId
                            LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = E.SubSectionID
                            left join hkp.LegalDesignation ld on ld.Id=e.LegalDesignationId
                            where SandwichFlag='2'
                            and WorkDate between '" + date + @"' and GETDATE() and YEAR(workdate)='"+year+@"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SandWichDataSet(string PlantId, string month, string year, out DataSet ds)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                int dd = 01;
                string jj = month.ToString() + "-" + dd.ToString() + "-" + year.ToString();
                string date = DateTime.Parse(jj).ToString("dd-MMM-yyyy");
                var str = @"select EmpSystemID,e.EmployeeCode,p.UserName as Plant,p.Id as PlantId,
                            format(WorkDate,'dd-MMM-yyyy')WorkDate,DayStatus,dp.UserName
                            as Department,s.UserName as Section,
                            SuS.UserName as SubSection,ld.UserName as Designation,
                            (select RowId from AttdnProcessData x where x.EmpSystemID=a.EmpSystemID
                            and WorkDate=DATEADD(day,-3,a.workdate)and
                            WorkDate between '" + date + @"' and GETDATE())Past3rdDay,
                            (select RowId from AttdnProcessData x where x.EmpSystemID=a.EmpSystemID
                            and WorkDate=DATEADD(day,-2,a.workdate) and
                            WorkDate between '" + date + @"' and GETDATE())Past2ndDay,
                            (select RowId from AttdnProcessData x where x.EmpSystemID=a.EmpSystemID
                            and WorkDate=DATEADD(day,-1,a.workdate) and
                            WorkDate between '" + date + @"' and GETDATE())PastDay,RowId as Today,
                            (select RowId from AttdnProcessData x where x.EmpSystemID=a.EmpSystemID
                            and WorkDate=DATEADD(day,1,a.workdate) and
                            WorkDate between '" + date + @"' and GETDATE())Tomorrow,
                            (select RowId from AttdnProcessData x where x.EmpSystemID=a.EmpSystemID
                            and WorkDate=DATEADD(day,2,a.workdate) and
                            WorkDate between '" + date + @"' and GETDATE())Future2ndDay,
                            Future3rdDay=(select RowId from AttdnProcessData x where x.EmpSystemID=a.EmpSystemID
                            and WorkDate=DATEADD(day,3,a.workdate) and
                            WorkDate between '" + date + @"' and GETDATE())
                            from AttdnProcessData a
                            left join EmployeeInformation e on e.SystemId=a.EmpSystemID
                            left join org.Plant p on p.Id=e.PlantId
                            left join org.Section s on s.Id=e.SectionId
                            LEFT JOIN ORG.Department DP ON DP.Id = E.DepartmentId
                            LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = E.SubSectionID
                            left join hkp.LegalDesignation ld on ld.Id=e.LegalDesignationId
                            where SandwichFlag='2' and e.PlantId='" + PlantId + @"'
                            and WorkDate between '" + date + @"' and GETDATE() and YEAR(workdate)='" + year + @"'";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(str, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void Process(string PlantId, string month, string year)
        {
            string TempMaster = "''", TodayMaster = "''", YesterdayMaster = "''", Back2ndDayMaster = "''", Back3rdDayMaster = "''";
            string TomorrowMaster = "''", Tomorrow2DayMaster = "''", Tomorrow3DayMaster = "''"; 
            DataSet SandwichData; // Build DataSet For Sandwich Process
            SandWichDataSet(PlantId,month,year,out SandwichData);
            if (SandwichData.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < SandwichData.Tables[0].Rows.Count; i++)
                {
                    string Past3rdDay = clsWebLib.RetValidLen(SandwichData.Tables[0].Rows[i][@"Past3rdDay"]).ToString();
                    string Past2ndDay = clsWebLib.RetValidLen(SandwichData.Tables[0].Rows[i][@"Past2ndDay"]).ToString();
                    string PastDay = clsWebLib.RetValidLen(SandwichData.Tables[0].Rows[i][@"PastDay"]).ToString();
                    string Today = clsWebLib.RetValidLen(SandwichData.Tables[0].Rows[i][@"Today"]).ToString();
                    string Tomorrow = clsWebLib.RetValidLen(SandwichData.Tables[0].Rows[i][@"Tomorrow"]).ToString();
                    string Future2ndDay = clsWebLib.RetValidLen(SandwichData.Tables[0].Rows[i][@"Future2ndDay"]).ToString();
                    string Future3rdDay = clsWebLib.RetValidLen(SandwichData.Tables[0].Rows[i][@"Future3rdDay"]).ToString();

                    // Unique RowId Finding Region
                    if(Past3rdDay!="")
                    {
                        CheckerFunction(ref TempMaster,ref Back3rdDayMaster, Past3rdDay);
                    }
                    if (Past2ndDay != "")
                    {
                        CheckerFunction(ref TempMaster, ref Back2ndDayMaster, Past2ndDay);
                    }
                    if (PastDay != "")
                    { 
                        CheckerFunction(ref TempMaster, ref YesterdayMaster, PastDay);
                    }
                    if (Today != "")
                    {
                        CheckerFunction(ref TempMaster, ref TodayMaster, Today);
                    }
                    if(Tomorrow !="")
                    {
                        CheckerFunction(ref TempMaster, ref TomorrowMaster, Tomorrow);
                    }
                    if (Future2ndDay != "")
                    {
                        CheckerFunction(ref TempMaster, ref Tomorrow2DayMaster, Future2ndDay);
                    }
                    if (Future3rdDay != "")
                    {
                        CheckerFunction(ref TempMaster, ref Tomorrow3DayMaster, Future3rdDay);
                    }
                }

                #region Manual Flag Update
                ManualFlagRows(Back3rdDayMaster, Back2ndDayMaster, YesterdayMaster, TodayMaster, TomorrowMaster, Tomorrow2DayMaster, Tomorrow3DayMaster);
                #endregion

                #region Calling Manual Process
                NewAttendanceProcessService ap = new NewAttendanceProcessService();
                ap.ManualScheduler(PlantId);
                #endregion

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
 