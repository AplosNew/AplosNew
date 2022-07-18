using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Employee
{
   public class FurniturePolicyService
    {
        private readonly SqlRepository _sqlRepository;
        public FurniturePolicyService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getFurnitureMaster()
        {
            try
            {
                var sql = @"select distinct UserName as Text from HKP.furnitureMaster";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getDesignationMaster()
        {
            try
            {
                var sql = @"select DISTINCT d.UserName as Text from MST.DesignationMaster d ORDER BY Text";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getFurnitureGridView(string username)
        {
            try
            {
                var sql = @"select fm.* from HKP.furnitureMaster fm where fm.UserName = '"+ username + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getDesignationGridView(string username)
        {
            try
            {
                var sql = @"select dm.*, dg.UserName as DesignationGroup, dsg.UserName as Designation, ec.UserName as EmployeeCategory from MST.DesignationMaster dm
left join HKP.Designation dg on dg.Id = dm.DesignationId
left join HKP.DesignationGroup dsg on dsg.Id = dm.DesignationGroupId 
left join HKP.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
 where dm.UserName = '" + username + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
