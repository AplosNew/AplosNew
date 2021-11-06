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
    public class YearPresentDaysSummaryService
    {

        ISqlRepository _sqlRepository;
        public YearPresentDaysSummaryService()
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
                            SuS.UserName as SubSection,ld.UserName as Designation,
                            (select RowId from AttdnProcessData x where x.EmpSystemID=a.EmpSystemID
                            and WorkDate=DATEADD(day,-3,a.workdate)and
                            WorkDate between '"+ date + @"' and GETDATE())Past3rdDay,
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
                            where SandwichFlag='2'
                            and WorkDate between '" + date + @"' and GETDATE() and YEAR(workdate)='"+year+@"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
 