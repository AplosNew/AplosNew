#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class UOMConversionController : BaseController
    {
        #region Constructor

        private readonly IUOMConversionService _uOMConversionService;

        public UOMConversionController(IUOMConversionService uOMConversionService)
        {
            _uOMConversionService = uOMConversionService;
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

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_uOMConversionService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetToUOMFactor(string fromUOMId, string toUOMId)
        {
            return Json(_uOMConversionService.GetToUOMFactor(fromUOMId, toUOMId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetUOMValueConvert(string fromUOMId, string toUOMId, int quantity)
        {
            return Json(_uOMConversionService.GetUOMValueConversation(fromUOMId, toUOMId, quantity), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(UOMConversion uOMConversion)
        {
            _uOMConversionService.Insert(uOMConversion);
            return Json(new { UOMConversion = uOMConversion, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _uOMConversionService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}