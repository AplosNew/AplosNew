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
    public class OTConfirmationProcessService
    {

        ISqlRepository _sqlRepository;
        public OTConfirmationProcessService()
        {
            _sqlRepository = new SqlRepository();
        }


        public IEnumerable<object> getDayTypes()
        {
            try
            {
                var str = @"Select * from dbo.DayType";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public object getFilters()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var str = @"Select p.Id as PlantId , p.UserName as Plant, e.Id as EntityId , e.UserName as Entity
                            from Org.Entity e 
                            left join org.Plant p on p.Id = e.PlantId where p.CompanyId = '"+identity.CompanyId+@"'
                            ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}