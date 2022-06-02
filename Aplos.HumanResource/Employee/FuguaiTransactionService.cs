using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Linq;

namespace Library.HumanResource.Employee
{
    public class FuguaiTransactionService
    {
        private readonly SqlRepository _sqlRepository;
        public FuguaiTransactionService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getEntity()
        {
            try 
            {
                var sql = @"select e.Id as Value, e.UserName as Text from ORG.Entity e";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getObservedBy()
        {
            try
            {
                var sql = @"select emp.EmployeeCode, emp.EmployeeName, e.UserName from ORG.Entity e
                            left join dbo.EmployeeInformation emp on emp.SystemId = e.EmployeeId
                            where e.Id = '112'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getCategory() 
        {
            try
            {
                var sql = @"select z.Id as Value, z.Category as Text from hkp.ZoneMaster z";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getDepartment()
        {
            try
            {
                var sql = @"select d.Id as Value, d.UserName as Text from org.Department d";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getResponsiblePerson()
        {
            try
            {
                var sql = @"select e.EmployeeName from dbo.EmployeeInformation e
                            left join org.Department d on d.Id = e.DepartmentId
                            where d.Id = '202024'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getProcess()
        {
            try
            {
                var sql = @"";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getMachine()
        {
            try
            {
                var sql = @"select mm.Id as Value, mm.UserName as Text from dbo.MachineMasterProcess msp
                            left join MST.MachineMaster mm on mm.Id = msp.MachineMasterId
                            left join hkp.Process p on p.Id = msp.ProcessId
                            where p.Id = '202036'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getMachineRef()
        {
            try
            {
                var sql = @"";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
