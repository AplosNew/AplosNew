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
       
    }
}