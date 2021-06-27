using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Setups.Controllers
{
    public class SalesTypeController : BaseController
    {
        #region -- Constructor

        private readonly ISalesTypeService _salesTypeService;

        public SalesTypeController(ISalesTypeService salesTypeService)
        {
            _salesTypeService = salesTypeService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_salesTypeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salesTypeService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_salesTypeService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalesType()
        {
            return Json(_salesTypeService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalesTypeById(string id)
        {
            return Json(_salesTypeService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SalesType salesType)
        {
            _salesTypeService.Insert(salesType);
            return Json(new { SalesType = salesType, Sequence = _salesTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(SalesType salesType)
        {
            _salesTypeService.Update(salesType);
            return Json(new { Sequence = _salesTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _salesTypeService.Delete(id);
                return Json(new { Sequence = _salesTypeService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations
    }
}