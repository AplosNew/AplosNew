#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class DMMController : BaseController
    {
        #region Constructor
        /// <summary>   The DMMService service. </summary>
        private readonly IDMMService _dMMService;
        private readonly ICompanyGroupDMMService _companyGroupDMMService;

        public DMMController(IDMMService dMMService, ICompanyGroupDMMService companyGroupDMMService)
        {
            _dMMService = dMMService;
            _companyGroupDMMService = companyGroupDMMService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_companyGroupDMMService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupDMMService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_dMMService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(DMM dMM)
        {
            _dMMService.Insert(dMM);
            return Json(new { DMM= dMM, Sequence=_dMMService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(DMM dMM)
        {
            _dMMService.Update(dMM);
            return Json(new { Sequence = _dMMService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _dMMService.DeleteGraph(id);
            return Json(new { Sequence = _dMMService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}