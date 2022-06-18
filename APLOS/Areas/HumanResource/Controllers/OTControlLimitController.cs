#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data.Sql;
using Library.Model.Payrolls;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class OTControlLimitController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        public OTControlLimitController(ISqlRepository R)
        {
            _sqlRepository=R;
        }
        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations



        #endregion -- Operations
    }
}