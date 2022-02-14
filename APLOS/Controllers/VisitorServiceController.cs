using System;
using Library.Service.EmployeeServices;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APLOS;
using Library.HumanResource.Employee;

namespace Aplos.Controllers
{
    [BasicAuthentication]
    public class VisitorServiceController : ApiController
    {
        FactoryVisitorService _emp = new FactoryVisitorService();
        public VisitorServiceController()
        {
            _emp = new FactoryVisitorService();
        }

        [HttpPost]
        public string SaveExpectedVisit([FromBody] IEnumerable<VisitorModel> DataToSave)
        {
            try
            {
                string Id = _emp.SaveExpectedVisit(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

    }
}
