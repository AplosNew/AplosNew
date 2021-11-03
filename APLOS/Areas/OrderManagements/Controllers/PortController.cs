#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Aplos.Controllers;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class PortController : BaseController
    {
        #region Constructor
        /// <summary>   The skillCategoryService service. </summary>
        private readonly IPortService _skillCategoryService;
        private readonly ICompanyGroupPortService _companyGroupPortService;

        public PortController(IPortService skillCategoryService, ICompanyGroupPortService companyGroupPortService)
        {
            _skillCategoryService = skillCategoryService;
            _companyGroupPortService = companyGroupPortService;
        }
        #endregion

        #region -- Pages
       
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_companyGroupPortService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupPortService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_skillCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Port skillCategory)
        {
            _skillCategoryService.Insert(skillCategory);
            return Json(new { Port= skillCategory, Sequence=_skillCategoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(Port skillCategory)
        {
            _skillCategoryService.Update(skillCategory);
            return Json(new { Sequence = _skillCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _skillCategoryService.DeleteGraph(id);
            return Json(new { Sequence = _skillCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}