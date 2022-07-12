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

        public IEnumerable<object> viewOTCompensatory()
        {
            try
            {
                var sql = @"";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex) {
                throw ex;
            }
        }
    }
}
