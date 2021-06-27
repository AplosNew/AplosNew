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
        public IHttpActionResult GetOperation(string ProdOrderId)
        {
            try
            {
                var result = _DailyProduction.GetOperation(ProdOrderId);
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
                string Id = _DailyProduction.Create(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpGet]
        public IHttpActionResult GetListAPIforProduction(string ProdnDate, string ProcessId, string EntityId, string ShiftId, string WkId)
        {
            try
            {
                var result = _DailyProduction.GetListAPIforProduction(ProdnDate, ProcessId, EntityId, ShiftId, WkId);
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
        public IHttpActionResult GetEmp(string AddedBy, string WkId, string OPId)
        {
            try
            {
                var result = _DailyProduction.GetEmp(AddedBy, WkId, OPId);
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

        [HttpPost]
        public string DeleteOp([FromBody] IEnumerable<operationwise> DataToDelete)
        {
            try
            {
                _DailyProduction.DeleteOp(DataToDelete);
            }
            catch (Exception ex)
            {

                return ex.ToString();

            }
            return "";

        }

        [HttpPost]
        public string CreateOp([FromBody] IEnumerable<operationwise> DataToSavex)
        {
            try
            {
                string Id = _DailyProduction.CreateOp(DataToSavex);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }


        [HttpGet]
        public IHttpActionResult GetOPSkill(string Operation)
        {
            try
            {
                var result = _DailyProduction.GetOPSkill(Operation);
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
