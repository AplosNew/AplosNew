using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APLOS;
using Library.HumanResource.Employee;
using Library.Service.Setups;

namespace Aplos.Controllers
{
    [BasicAuthentication]
    public class VehicleRequistionController : ApiController
    {
        private readonly IMailSenderService _mailSenderService;
        VehicleRequistionService veh ;
        public VehicleRequistionController(IMailSenderService mailSenderService)
        {
            _mailSenderService = mailSenderService;
            veh = new VehicleRequistionService(_mailSenderService);
        }       

        [HttpGet]
        public IHttpActionResult GetToLocation(string Id)
        {
            try
            {
                var result = veh.GetToLocation(Id);
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
        public IHttpActionResult GetFromLocation()
        {
            try
            {
                var result = veh.GetFromLocation();
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
        public IHttpActionResult GetApprovingAuthList()
        {
            try
            {
                var result = veh.GetApprovingAuthList();
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
        public string SaveData([FromBody] IEnumerable<VehicleRequistionModel> DataToSave)
        {
            try
            {
                string Id = veh.SaveData(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

    }
}
