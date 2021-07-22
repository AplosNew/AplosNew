using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APLOS;
using Library.HumanResource.NewAttendanceProcess;


namespace Aplos.Controllers
{
    [BasicAuthentication]
    public class ManualAttndFromAppController : ApiController
    {

        ShiftChangeService app = new ShiftChangeService();

        public ManualAttndFromAppController()
        {

        }
       
       
        [HttpGet]
        
        public IHttpActionResult GetShiftData(string ShiftId,string Date)
        {
            try
            {
                var result = app.GetShiftData(ShiftId,Date);
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
