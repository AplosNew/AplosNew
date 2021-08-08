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
    public class NewAttendanceDashboardServices
    {

        ISqlRepository _sqlRepository;
        public NewAttendanceDashboardServices()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getFilters()
        {
            try
            {
                var str = @"Select distinct mp.EntityId as EntityId , e.Id as EId , e.UserName as Entity, mp.PositionId ,pos.Id as PosId , pos.UserName as PositionName,
                            p.id as PlantId, p.UserName as Plant , div.Id as DivisionId , div.UserName as Division,
                            sd.SystemID as ShiftId , sd.UserName as Shift,
                            dept.Id as DepartmentId , dept.UserName as Department,
                            sec.id as SectionId , sec.UserName as Section , ssec.Id as SubSectionId , ssec.UserName as SubSection,
                            ei.SystemId as EmpId , ei.EmployeeName as ResponsiblePerson, 
                            ag.Id as AttendGroupId , ag.UserName as AttendGroup , wg.Id as WorkGroupId , wg.UserName as WorkGroup,
                            mp.PRBudgetCode , mp.ROBudgetCode
                            from mst.ManpowerBudget mp
                            left join org.Position pos on pos.Id = mp.PositionId
                            left join org.Entity e on e.Id = mp.entityId
                            left join org.Plant p on p.Id = e.PlantId
                            left join org.Division div on div.Id = pos.DivisionId
                            left join org.Section sec on sec.Id = pos.SectionId
                            left join org.SubSection ssec on ssec.Id = pos.SubSectionId
                            left join org.Department dept on dept.Id = pos.DepartmentId
                            left join dbo.AttendanceGroup ag on ag.Id = mp.AttendanceGroupId
                            left join hkp.WorkGroup wg on wg.id = mp.WorkGroupId
                            left join dbo.EmployeeInformation ei on ei.SystemId = mp.ResponsiblePerson
                            left join dbo.ShiftDefination sd on sd.SystemID = mp.ShiftDefinationId
                            ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getGridData(string Date)
        {
            try
            {
                DateTime date = Convert.ToDateTime(Date);
                var str = @"Select BudgetId , Count( distinct EmpSystemID) as OnRoll,
                            Sum(Case When InStatus = 'IN' then 1 else 0 end) as InStat,
                            Sum(Case When EarlyLateIn ='EI' then 1 else 0 end) as EarlyIn,
                            Sum(Case When EarlyLateIn='LI'then 1 else 0 end) as LateIn,
                            Sum(Case When InStatus ='IM' then 1 else 0 end) as InMissing,
                            Sum(Case When IsOD=1 then 1 else 0 end) as OD,
                            Sum(Case when DayStatus='W' or DayStatus='H' or DayStatus='AH' or DayStatus='CW' then 1 else 0 end) as DayStatus,
                            Sum(Case when LeaveStatus is not null then 1 else 0 end) as Leave,
                            Sum(Case When InStatus ='O' then 1 else 0 end) as Other
                            from dbo.AttdnProcessData
                            where WorkDate = '"+date+@"'
                            group by BudgetId";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }


    }
}