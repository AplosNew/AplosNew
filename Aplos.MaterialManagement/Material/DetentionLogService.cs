using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.MaterialManagement.Material
{
   
    public class DetentionLogService
    {
        SqlRepository _sqlRepository;
        public DetentionLogService()
        {
            _sqlRepository = new SqlRepository();
        }
        #region Entity Specific
        public IEnumerable<object> GetEntity()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"";

                sql = @"select Id as Value, UserName as Text from org.Entity
                        where Active = 1 order by Text";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion Entity Specific

        // Workcenter
        public IEnumerable<object> GetWorkCenter()
        {
            try
            {
                var sql = @"select WM.StandardName Text, WM.Id Value from SCS.WorkCenterMaster WM
                            
                            where WM.Active = 'true'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        // Get Department List
        public IEnumerable<object> GetDetentionDepartment()
        {
            try
            {
                string sql = @"select distinct DD.DepartmentId Value, D.StandardName Text from DetentionMasterDepartment DD
                              left outer join ORG.Department D on D.id = DD.DepartmentId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        // Responsible Person
        public IEnumerable<object> GetDetentionResponsible(string detentionId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string str = @"select distinct E.SystemId as ResponsiblePersonId,E.EmployeeCode,E.EmployeeName as ResponsiblePerson,DEP.UserName AS Department,S.UserName as Section,
                           SS.UserName as SubSection,DEG.UserName AS [LegalDesignation],DR.DetentionMasterId from DetentionMasterResponsible DR
                           left join EmployeeInformation AS E ON E.SystemId=DR.ResponsibleMasterId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=E.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.id=E.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=E.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
                            where DetentionMasterId='" + detentionId + "'";

                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        // Detention Type
        public IEnumerable<object> getDetentionTypeListByDepartment(string departmentid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select distinct DT.UserName As Text, DT.Id As Value from DetentionMasterDepartment DD
                        left join DetentionMaster DM ON DM.Id=DD.DetentionMasterId
                        left join hkp.DetentionType DT ON DT.id=DM.DetentionTypeId
            --where DepartmentId='" + departmentid + "'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        // Detention Master based on Detention Type
        public IEnumerable<object> getDetention(string processId)
        {
            try
            {
                var sql = @"select Distinct DM.DetentionUserName as Text, DM.Id as Value 
                            from DetentionMaster DM
                            LEFT JOIN DetentionMasterProcess DMP on DMP.DetentionMasterId = DM.Id
                            LEFT JOIN HKp.Process P on P.Id = DMP.ProcessId
                            where P.Id = '"+ processId + "' order by Text ASC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getProcessList()
        {
            try
            {
                var sql = @"select distinct DMP.ProcessId Value, P.UserName Text from  DetentionMasterProcess DMP
                            left join HKP.Process P on P.Id = DMP.ProcessId";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }



}
