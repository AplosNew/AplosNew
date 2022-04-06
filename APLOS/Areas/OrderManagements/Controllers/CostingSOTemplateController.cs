#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Aplos.Controllers;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class CostingSOTemplateController : BaseController
    {
        #region Constructor

        public CostingSOTemplateController()
        {
            
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