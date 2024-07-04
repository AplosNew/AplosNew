using System;
using Library.Service.EmployeeServices;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Library.HumanResource;
using Aplos.HumanResource;
using APLOS;

namespace Aplos.Controllers
{
   // [BasicAuthentication]
    public class MyAppPaySlipController : ApiController
    {
        MyAppPaySlipService _slip = new MyAppPaySlipService();
        public MyAppPaySlipController()
        {
            _slip = new MyAppPaySlipService();
        }
       
        [HttpGet]
        public IHttpActionResult GetData(string CGId, string CompId, string plantId, string month, string year, string EmpId, string languageId, bool isActive, bool isSep, bool isMaternity)
        {
            try
            {
                var result = _slip.GetEmployeePaySlip(CGId,CompId,plantId,month,year,EmpId,languageId,isActive,isSep,isMaternity);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }


    }
}
