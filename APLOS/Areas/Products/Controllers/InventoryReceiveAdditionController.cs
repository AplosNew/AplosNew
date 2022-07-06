using Aplos.Controllers;
using Aplos.MaterialManagement;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.MaterialManagement.Inventory;
using System;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Products.Controllers
{
    public class InventoryReceiveAdditionController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        public InventoryReceiveAdditionController( ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor



        public ActionResult Purchaseconfirmation()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetFiltersPurchaseconfirmationData(string fromDate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                InventoryReceiveQueryService obj = new InventoryReceiveQueryService(_sqlRepository);
                return Json(obj.GetFiltersPurchaseconfirmationData(identity.PlantId, fromDate, todate), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }


}