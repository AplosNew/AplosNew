using Aplos.Controllers;
using Aplos.Properties;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Setups.Controllers
{
    public class PrdOrdSettingController : BaseController
    {
        #region Constructor

        private readonly IPrdOrdSettingService _prdOrdSettingService;

        public PrdOrdSettingController(IPrdOrdSettingService prdOrdSettingService)
        {
            _prdOrdSettingService = prdOrdSettingService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [Authorize, HttpGet]
        public ActionResult GetList(string groupId, string companyId, string plantId)
        {
            return Json(_prdOrdSettingService.GetList(groupId, companyId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<PrdOrdSetting> prdOrdSetting)
        {
            _prdOrdSettingService.InsertOrUpdateGraph(prdOrdSetting);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion -- Operations
    }
}