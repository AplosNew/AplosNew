#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Model.Costings;
using Library.Service.Costings;
using Library.Data.Sql;

#endregion

namespace Aplos.Areas.Costings.Controllers
{
    public class BOQCostingApprovalSettingController : BaseController
    {
        #region Constructor
        
        private readonly ISqlRepository _sqlRepository;

        public BOQCostingApprovalSettingController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region -- Pages
    
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        

        #endregion
    }
}