#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Parties.Controllers
{
    public class IntermediateItemEntityController : BaseController
    {
        #region Constructor

        private readonly IIntermediateItemEntityService _intermediateItemService;

        public IntermediateItemEntityController(IIntermediateItemEntityService intermediateItemService)
        {
            _intermediateItemService = intermediateItemService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<IntermediateItemEntity> intermediateItemEntity)
        {
            _intermediateItemService.InsertORUpdate(intermediateItemEntity);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _intermediateItemService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithEntity(GridParameter parameters, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_intermediateItemService.Query(parameters, entityId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}