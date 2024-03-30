using Aplos.Controllers;
using Library.Data.Sql;
using System.Web.Mvc;
using Library.Service.Administration.Contract;

namespace Aplos.Areas.Administration.Controllers
{
    public class GeneralCheckedApprovedController : BaseController
    {
        GeneralContractCheckService gc = new GeneralContractCheckService();
        private readonly SqlRepository _sqlRepository;
        public GeneralCheckedApprovedController()
        {
            _sqlRepository = new SqlRepository();
        }

        public ActionResult Aplos()
        {
            return View();
        }

    }
}