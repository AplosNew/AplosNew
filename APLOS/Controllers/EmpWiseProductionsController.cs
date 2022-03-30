using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Library.OrderManagement.Production;
using APLOS;

namespace Aplos.Controllers
{
    [BasicAuthentication]
    public class EmpWiseProductionsController : ApiController
    {
        #region Constructor
        EmployeeOperationsAPIService _empOpt = new EmployeeOperationsAPIService();

        public EmpWiseProductionsController()
        {
            _empOpt = new EmployeeOperationsAPIService();
        }

        #endregion Constructor

        #region Get Functions

        [HttpGet]
        public IHttpActionResult GetPeriod()
        {
            try
            {
                var result = _empOpt.GetPeriod();
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
        public IHttpActionResult GetOperation(string ProdOrderId)
        {
            try
            {
                var result = _empOpt.GetOperation(ProdOrderId);
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
      
        #endregion
    }
}
