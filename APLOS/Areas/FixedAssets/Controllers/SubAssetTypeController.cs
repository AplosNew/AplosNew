#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.FixedAssets;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class SubAssetTypeController : BaseController
    {
        #region Constructor

        private readonly ISubAssetTypeService _subAssetTypeService;

        public SubAssetTypeController(
              ISubAssetTypeService SubAssetTypeService
            )
        {
            _subAssetTypeService = SubAssetTypeService;
        }

        #endregion Constructor
        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_subAssetTypeService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/FixedAssets/Views/SubAssetType.cshtml");

        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_subAssetTypeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_subAssetTypeService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SubAssetType model)
        {
            _subAssetTypeService.Insert(model);
            return Json(new { SubAssetType = model, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(SubAssetType model)
        {
            _subAssetTypeService.Update(model);
            return Json(new { SubAssetType = model, Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _subAssetTypeService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}