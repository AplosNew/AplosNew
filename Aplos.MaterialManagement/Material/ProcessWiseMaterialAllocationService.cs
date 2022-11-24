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

namespace Library.MaterialManagement.Material
{
    public class ProcessWiseMaterialAllocationService
    {
        private readonly SqlRepository _sqlRepository;
       
            
        
        public ProcessWiseMaterialAllocationService()
        {
            _sqlRepository = new SqlRepository();
        }
        public IEnumerable<object> getEmployee()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string str = @"select EMP.EmployeeCode as Code, EMP.SystemId, EMP.EmployeeName, SC.UserName as Section, GDSG.UserName as Designation, UN.UserName as Entity
                                from EmployeeInformation EMP
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
                                left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId



                                left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
                                left join SalaryRuleMaster SRM on srm.systemid = emp.salaryrulemastersystemid
                                left join ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId
                                left join TransportGroup TG on TG.Id = EMP.TransportGroupId
                                where EMP.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
