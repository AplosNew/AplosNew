using Library.Data.Sql;
using System.Text;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class QueryController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;

        public QueryController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        [HttpPost]
        public JsonResult GetQueryResult(string sql)
        {
            return new JsonResult
            {
                ContentEncoding = Encoding.UTF8,
                ContentType = "application/json;",
                Data = _sqlRepository.GetDataCollection(sql),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue
            };
        }
    }
}