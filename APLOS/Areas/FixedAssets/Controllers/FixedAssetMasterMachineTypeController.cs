#region Using
using Aplos.Controllers;
using Aplos.Model.FixedAssets;
using Aplos.Properties;
using Aplos.Service.FixedAssets;
using Library.Core;
using Library.Crosscutting.Security;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
#endregion

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class FixedAssetMasterMachineTypeController : BaseController
    {
        #region Constractor
        private readonly IFixedAssetMasterMachineTypeService _fixedAssetMasterMachineTypeService;
        public FixedAssetMasterMachineTypeController(IFixedAssetMasterMachineTypeService fixedAssetMasterMachineTypeService)
        {
            _fixedAssetMasterMachineTypeService = fixedAssetMasterMachineTypeService;
        }
        #endregion

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters,string fixedAssetMasterId)
        {
            return Json(_fixedAssetMasterMachineTypeService.Query(parameters,fixedAssetMasterId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete(string id)
        {
            _fixedAssetMasterMachineTypeService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<FixedAssetMasterMachineType> fixedAssetMasterMachineType,string fixedAssetMasterId)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _fixedAssetMasterMachineTypeService.SaveMaster(fixedAssetMasterId,fixedAssetMasterMachineType);
            return Json(new { Message = AplosMessage.Insert });
        }
    }
}