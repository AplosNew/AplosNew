using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Library.OrderManagement.Production;
using APLOS;
using System.Collections.Generic;

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

        #region Functions

        [HttpGet]
        public IHttpActionResult GetListAPIforProduction(string ProdnDate, string ProcessId, string EntityId, string ShiftId, string WkId)
        {
            try
            {
                var result = _empOpt.GetListAPIforProduction(ProdnDate, ProcessId, EntityId, ShiftId, WkId);
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
        public string Create([FromBody] IEnumerable<DailyProduction> DataToSave)
        {
            try
            {
                string Id = _empOpt.Create(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

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
        public IHttpActionResult GetOperation(string ProdOrderId,string ProcessId)
        {
            try
            {
                var result = _empOpt.GetOperation(ProdOrderId,ProcessId);
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
        public IHttpActionResult GetEmp(string AddedBy, string WkId, string OPId)
        {
            try
            {
                var result = _empOpt.GetEmp(AddedBy, WkId, OPId);
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
                var result = _empOpt.GetDetailProductionList(ProdnDate, EntityId, ProcessId, ShiftId, WkId, PoId, OPId);
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
