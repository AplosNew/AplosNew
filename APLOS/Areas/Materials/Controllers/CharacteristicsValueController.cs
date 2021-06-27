using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Materials;
using Library.Service.Materials;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Materials.Controllers
{
    public class CharacteristicsValueController : Controller
    {
        #region Constructor

        private readonly ICharacteristicsValueService _characteristicsValueService;

        public CharacteristicsValueController(ICharacteristicsValueService characteristicsValueService)
        {
            this._characteristicsValueService = characteristicsValueService;
        }

        #endregion Constructor

        #region -- Pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetCbo(string characteristicsId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_characteristicsValueService.GetCbo(identity.CompanyGroupId, characteristicsId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCharacteristicsValueCboByCharacteristicsId(string materialMasterId, string characteristicsId, string valueAssignmentLevel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_characteristicsValueService.GetCharacteristicsValueCboByCharacteristicsId(materialMasterId,characteristicsId, valueAssignmentLevel), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCharacteristicsValueCbo()
        {
            return Json(new SelectList(_characteristicsValueService.GetCharacteristicsValueList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string characteristicsId, string materialId)
        {
            return Json(_characteristicsValueService.GetAutoSequence(characteristicsId, materialId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string characteristicsId, string ids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_characteristicsValueService.Query(parameters, identity.CompanyGroupId, characteristicsId, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCharacteristicsValueSearchData(GridParameter parameters, string assignment, string materialMasterId, string charId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_characteristicsValueService.GetCharacteristicsValueSearchData(parameters, identity.CompanyGroupId, assignment, materialMasterId, charId), JsonRequestBehavior.AllowGet);
        }
		[HttpGet, Authorize]
		public JsonResult GetCharacteristicsValueSearchData1(string assignment, string materialMasterId, string charId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var jsondata = Json(_characteristicsValueService.GetCharacteristicsValueSearchData1(identity.CompanyGroupId, assignment, materialMasterId, charId), JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;
		}

		[HttpGet, Authorize]
        public JsonResult GetCharacteristicsValueListByMaterialMaster(string materialMasterId)
        {
            return Json(_characteristicsValueService.GetCharacteristicsValueListByMaterialMaster(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCharacteristicsValue(string id)
        {
            return Json(_characteristicsValueService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CharacteristicsValue entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            _characteristicsValueService.Insert(entity);
            return Json(new { CharacteristicsValue = entity, Sequence = _characteristicsValueService.GetAutoSequence(entity.CharacteristicsId, null), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(CharacteristicsValue characteristicsvalue)
        {
            _characteristicsValueService.Update(characteristicsvalue);
            return Json(new { Sequence = _characteristicsValueService.GetAutoSequence(characteristicsvalue.CharacteristicsId, null), Message = AplosMessage.Success });
        }

        public ActionResult Delete(string id)
        {
            var data = _characteristicsValueService.Find(id);
            _characteristicsValueService.DeleteGraph(id);
            return Json(new { Sequence = _characteristicsValueService.GetAutoSequence(data.CharacteristicsId, null), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}