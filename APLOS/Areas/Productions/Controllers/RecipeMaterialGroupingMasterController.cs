using Aplos.Controllers;
using Aplos.Properties;
using System.Web.Mvc;
using Library.Model.Productions.Recipe;
using Library.Core;
using Library.Service.Productions;
using Library.Data;

namespace Aplos.Areas.Productions.Controllers
{
    public class RecipeMaterialGroupingMasterController : BaseController
    {
        #region Constructor

        private readonly IRecipeMaterialGroupingMasterService _recipeMaterialGroupingMasterService;
       

        public RecipeMaterialGroupingMasterController(
              IRecipeMaterialGroupingMasterService recipeMaterialGroupingMasterService
            )
        {
            _recipeMaterialGroupingMasterService = recipeMaterialGroupingMasterService;
        }

        #endregion Constructor


       
        public ActionResult Aplos()
        {
            return View();
        }


        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_recipeMaterialGroupingMasterService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_recipeMaterialGroupingMasterService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetRecipeMaterialGroupingDetailList(string masterid)
        {
            return Json(_recipeMaterialGroupingMasterService.GetRecipeMaterialGroupingDetailList(masterid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_recipeMaterialGroupingMasterService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(RecipeMaterialGroupingMaster model)
        {
            _recipeMaterialGroupingMasterService.InsertOrUpdate(model);
            return Json(new { RecipeMaterialGroupingMaster = model, Sequence = _recipeMaterialGroupingMasterService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult CreateRecipeMaterialGroupingDetail(RecipeMaterialGroupingDetail recipeMaterialGroupingDetail)
        {
            var IsDuplicateEntryAllowed = _recipeMaterialGroupingMasterService.RecipeMaterialGroupingValidation(recipeMaterialGroupingDetail.RecipeMaterialGroupingMasterId, recipeMaterialGroupingDetail.ArticleId, recipeMaterialGroupingDetail.MaterialMasterId);
            if (IsDuplicateEntryAllowed)
            {
                _recipeMaterialGroupingMasterService.CreateRecipeMaterialGroupingDetail(recipeMaterialGroupingDetail);
            }
            else
            {
                throw new CustomException("Selected Material/Article already exists...");
            }
           
            return Json(new { RecipeMaterialGroupingMaster = recipeMaterialGroupingDetail, Message = AplosMessage.Success });
        }

        public ActionResult Delete(string id)
        {
            _recipeMaterialGroupingMasterService.Delete(id);
            return Json(new { Sequence = _recipeMaterialGroupingMasterService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        [Authorize]
        public JsonResult DeleteRawMaterial(string rawmaterialid)
        {
            _recipeMaterialGroupingMasterService.DeleteRawMaterial(rawmaterialid);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}