using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APLOS;
using Library.HumanResource.Employee;

namespace Aplos.Controllers
{
    [BasicAuthentication]
    public class VehicleRequistionController : ApiController
    {
        VehicleRequistionService veh = new VehicleRequistionService();
        public VehicleRequistionController()
        {
            veh = new VehicleRequistionService();
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
        
      
    }
}
