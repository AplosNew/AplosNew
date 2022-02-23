using System;
using Library.Service.EmployeeServices;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APLOS;
using Library.HumanResource.Employee;
using Library.Service.Setups;

namespace Aplos.Controllers
{
    [BasicAuthentication]
    public class VisitorServiceController : ApiController
    {
        private readonly IMailSenderService _mailSenderService;
        FactoryVisitorService _emp ;
        public VisitorServiceController(IMailSenderService mailSenderService)
        {
            _mailSenderService = mailSenderService;
            _emp = new FactoryVisitorService(_mailSenderService);
        }

        [HttpPost]
        public string SaveEmpVisit([FromBody] IEnumerable<VisitorModel> DataToSave)
        {
            try
            {
                string Id = _emp.SaveEmployeeVisit(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpGet]
        public IHttpActionResult GetTodayMineList(string EmpId)
        {
            try
            {
                var result = _emp.GetTodayMineList(EmpId);
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
      
        [HttpGet]
        public IHttpActionResult GetExpectedInList()
        {
            try
            {
                var result = _emp.GetExpectedInList();
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
        
        [HttpGet]
        public IHttpActionResult GetExpectedOutList()
        {
            try
            {
                var result = _emp.GetExpectedOutList();
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
     
        [HttpPost]
        public string SaveInOutTime([FromBody] IEnumerable<VisitorModel> DataToSave)
        {
            try
            {
                string Id = _emp.SaveInOutTime(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

    }
}
