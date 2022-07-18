using Library.Core;
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

namespace Library.HumanResource.NewOTProcess
{
    public class OTCompensatoryService
    {
        ISqlRepository _sqlRepository;

        public OTCompensatoryService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getEntity()
        {
            try
            {
                var sql = "select Id as Value, UserName as Text from ORG.Entity ORDER BY Text";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEmployeeType()
        {
            try
            {
                var sql = "select Id as Value, UserName as Text from HKP.EmployeeCategory ORDER BY Text ";
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
                var sql = "select Id as Value, UserName as text from ORG.Department";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getSection()
        {
            try
            {
                var sql = "select Id as Value, UserName as Text from ORG.Section ORDER BY Text";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getSubSection()
        {
            try
            {
                var sql = "select Id as Value, UserName as Text from ORG.SubSection ORDER BY Text";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getOTReason()
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

        public IEnumerable<object> viewOTCompensatory(string un, string ec, string dp, string sc, string sbc)
        {
            try
            {
                var sql = @"select EMP.SystemId, EMP.EmployeeName, DP.Id as DepartmentId, DP.UserName as Department ,
GDSG.Id as LegalDesignationId, GDSG.UserName as LegalDesignation, Un.Id as UN, EC.Id as EC,
DP.Id as DP, SC.Id  as SC, SBC.Id as SBC
from dbo.EmployeeInformation EMP
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
left join ORG.Entity UN on UN.Id = MBGT.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
left join hkp.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId
where un.Id = '"+un+"' and ec.Id = '"+ec+"' and dp.Id = '"+dp+"' and sc.Id = '"+sc+"' and sbc.Id = '"+sbc+"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex) {
                throw ex;
            }
        }
    }
}
