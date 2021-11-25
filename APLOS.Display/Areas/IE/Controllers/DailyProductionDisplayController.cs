#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.IE.Controllers
{
    public class DailyProductionDisplayController : BaseController
    {
        #region Constructor
        private readonly SqlRepository _sqlRepository;
  
        public DailyProductionDisplayController(
            )
        {
            _sqlRepository = new SqlRepository();

        }

        #endregion Constructor


        [AllowAnonymous]
        public ActionResult Aplos()
        {
            return View();
        }

     
    }
}