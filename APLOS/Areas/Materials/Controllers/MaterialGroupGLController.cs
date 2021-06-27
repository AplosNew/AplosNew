#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Materials;
using Library.Model.Parties;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialGroupGLController : BaseController
    {
        #region Constructor

        /// <summary>   The MaterialGroupGLService service. </summary>
        private readonly IMaterialGroupGLService _materialGroupGLService;

        public MaterialGroupGLController(
              IMaterialGroupGLService materialGroupGLService
            )
        {
            _materialGroupGLService = materialGroupGLService;
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

        [HttpPost]
        public JsonResult UpdateMaterialGroupDeterminate(IEnumerable<MaterialGroupGL> materialGroupGL, IEnumerable<MaterialGroupPartyAccountGroupGL> materialGroupVendorReconGL)
        {
            _materialGroupGLService.InsertUpdateMaterialGroupDeterminate(materialGroupGL, materialGroupVendorReconGL);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet]
        public ActionResult GetDataByMaterialGroupMasterId(GridParameter parameters, string materialGroupMasterId, string coaId)
        {
            return Json(_materialGroupGLService.GetDataByMaterialGroupMasterId(parameters, materialGroupMasterId, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetListWithCombine(string coaId)
        {
            return Json(_materialGroupGLService.GetSearchWithCombine(coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetListWithCombineAssign(string coaId)
        {
            return Json(_materialGroupGLService.GetSearchWithCombineWithAssign(coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetListWithCombineNotAssign(string coaId)
        {
            return Json(_materialGroupGLService.GetSearchWithCombineWithNotAssign(coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCombineCoa(GridParameter parameters)
        {
            return Json(_materialGroupGLService.GetSearchWithCombineCoa(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListAccountGroupVendor(GridParameter parameters)
        {
            return Json(_materialGroupGLService.GetPartyAccountGroup(parameters, PartyType.Vendor.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListAccountGroupAll(GridParameter parameters)
        {
            return Json(_materialGroupGLService.GetPartyAccountGroup(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPartyAccountVD(GridParameter parameters)
        {
            return Json(_materialGroupGLService.GetPartyAccountVD(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit(MaterialGroupGL fixedAssetClass)
        {
            _materialGroupGLService.Update(fixedAssetClass);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _materialGroupGLService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        [HttpGet, Authorize]
        public ActionResult GetReport()
        {

            try
            {
                Library.Accounting.Accounts.AccountsMaterialGroupGlService Report = new Library.Accounting.Accounts.AccountsMaterialGroupGlService();
                Report.MaterialGrouprRport();
                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }


        #endregion -- Operations
    }
}