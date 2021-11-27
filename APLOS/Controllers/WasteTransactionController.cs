using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APLOS;
using Library.OrderManagement.Production;

namespace Aplos.Controllers
{
    [BasicAuthentication]
    public class WasteTransactionController : ApiController
    {
        WasteTransactionService _data = new WasteTransactionService();
        public WasteTransactionController()
        {
            _data = new WasteTransactionService();
        }


        [HttpGet]
        public IHttpActionResult GetBudgetInfo(string UserId)
        {
            try
            {
                var result = _data.GetBudgetInfo(UserId);
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
        public IHttpActionResult GetItemName(string Entity,string BudgetId)
        {
            try
            {
                var result = _data.GetItemName(Entity,BudgetId);
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
        public string Create([FromBody] IEnumerable<WasteTransactionModel> DataToSave)
        {
            try
            {
                string Id = _data.SaveData(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

    }
}
