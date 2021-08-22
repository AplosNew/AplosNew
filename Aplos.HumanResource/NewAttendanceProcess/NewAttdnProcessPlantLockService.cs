using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.EmployeeServices;
using bplib;
using Newtonsoft.Json;
using Library.Crosscutting.Security;
using System.Threading;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class NewAttdnProcessPlantLockService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public NewAttdnProcessPlantLockService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        public IEnumerable<object> GetUnLockedEmployees(string Date)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"select e.EmployeeCode,e.EmployeeName,a.EmpSystemID,format(a.WorkDate,'yyyy-MMM-dd')WorkDate,
                a.DayStatus,a.IsLock,a.LockedBy,
                ent.UserName as Entity,u.UserName as Unit,format(e.DOJ,'yyyy-MMM-dd')DOJ,
                s.UserName as Section,ss.UserName as SubSection,dept.UserName as Department
                FROM AttdnProcessData A left join EmployeeInformation e on e.SystemId=a.EmpSystemID
                LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = e.BudgetCode
                LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId    
                LEFT JOIN [ORG].[Unit] u ON u.Id = ENT.UnitId
                LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId  
                LEFT JOIN [ORG].[Department] dept ON dept.Id = POS.DepartmentId
                LEFT JOIN [ORG].[Section] s ON s.Id = POS.SectionId
                LEFT JOIN [ORG].[SubSection] ss ON ss.Id = POS.SubSectionId                           
                where WorkDate='" + Date + @"' and e.EmployeeStatus='Active'
                and IsLock=0 AND a.PlantID='" + identity.PlantId + "'";
               
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetLockedEmployees(string Date)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"select e.EmployeeCode,e.EmployeeName,a.EmpSystemID,format(a.WorkDate,'yyyy-MMM-dd')WorkDate,
                a.DayStatus,a.IsLock,a.LockedBy,
                ent.UserName as Entity,u.UserName as Unit,format(e.DOJ,'yyyy-MMM-dd')DOJ,
                s.UserName as Section,ss.UserName as SubSection,dept.UserName as Department
                FROM AttdnProcessData A left join EmployeeInformation e on e.SystemId=a.EmpSystemID
                LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = e.BudgetCode
                LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId    
                LEFT JOIN [ORG].[Unit] u ON u.Id = ENT.UnitId
                LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId  
                LEFT JOIN [ORG].[Department] dept ON dept.Id = POS.DepartmentId
                LEFT JOIN [ORG].[Section] s ON s.Id = POS.SectionId
                LEFT JOIN [ORG].[SubSection] ss ON ss.Id = POS.SubSectionId                           
                where WorkDate='"+Date+@"' and e.EmployeeStatus='Active'
                and IsLock=1 AND a.PlantID='"+identity.PlantId+"'";
               
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

    
}

