using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Library.OrderManagement.OrderControl;
using APLOS;

namespace Aplos.Controllers
{
    [BasicAuthenticationAttribute]
    public class OrderControlTypeController : ApiController
    {
        #region Constructor

        OrderControl _ControlType = new OrderControl();
        public OrderControlTypeController(
            
          )
        {
            
        }

        #endregion Constructor

        [HttpGet]
        public IHttpActionResult GetControlType()
        {
            try
            {
                var result = _ControlType.GetControlType();
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
        public IEnumerable<object> GetForm(string ControlType)
        {
            return _ControlType.GetForm(ControlType);
        }

        [HttpPost]
        public string Create([FromBody] IEnumerable<OrderControlData> DataToSave)
        {
            try
            {
                string Id = _ControlType.Create(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {              
                return ex.ToString();
            }
        }

        [HttpPost]
        [Route("api/Remark/MasterId/{MasterId}")]
        public string SaveRemarks([FromBody]IEnumerable<OrderControlRemarks> DataToSave, [FromUri]string MasterId)
        {
            try
            {
                string Id = _ControlType.SaveRemarks(DataToSave,MasterId);
                return Id;
               
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        [HttpGet]
        public IHttpActionResult GetSOId(string level)
        {
            try
            {
                var result = _ControlType.GetSalesOrderId(level);
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
        public IHttpActionResult GetPRId(string level)
        {
            try
            {
                var result = _ControlType.GetProductionOrderId(level);
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
