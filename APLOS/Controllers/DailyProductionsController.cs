using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Library.OrderManagement.Production;
using APLOS;


namespace Aplos.Controllers
{
    [BasicAuthenticationAttribute]
    public class DailyProductionsController : ApiController
    {
        #region Constructor
        DailyProductionData _DailyProduction = new DailyProductionData();

        public DailyProductionsController(

          )
        {

        }

        #endregion Constructor       

    
       

        [HttpGet]
        public IHttpActionResult GetOP(string AddedBy, string WkId)
        {
            try
            {
                var result = _DailyProduction.GetOP(AddedBy, WkId);
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
        public IHttpActionResult GetWk(string AddedBy)
        {
            try
            {
                var result = _DailyProduction.GetWk(AddedBy);
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
        public IHttpActionResult GetDetailProductionList(string ProdnDate, string EntityId, string ProcessId, string ShiftId, string WkId, string PoId, string OPId)
        {
            try
            {
                var result = _DailyProduction.GetDetailProductionList(ProdnDate, EntityId, ProcessId, ShiftId, WkId, PoId, OPId);
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
        public string Delete([FromBody] IEnumerable<DailyProduction> DataToDelete)
        {
            try
            {
                _DailyProduction.Delete(DataToDelete);
            }
            catch (Exception ex)
            {

                return ex.ToString();

            }
            return "";

        }
              
    }
}
