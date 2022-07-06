using Aplos.Controllers;
using Library.Data.Sql;
using Library.MaterialManagement.Inventory;
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

       
    }


}