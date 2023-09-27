#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionOrderEntitySetupController : BaseController
    {
        #region Constructor
      
        public ProductionOrderEntitySetupController()
        {
           
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        
    }
}