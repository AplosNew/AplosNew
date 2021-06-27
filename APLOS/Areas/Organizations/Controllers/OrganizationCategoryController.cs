#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class OrganizationCategoryController : BaseController
    {
        #region -- Constructor

        private readonly IOrganizationCategoryService _organizationCategoryService;

        public OrganizationCategoryController(IOrganizationCategoryService organizationCategoryService)
        {
            _organizationCategoryService = organizationCategoryService;
        }

        #endregion -- Constructor

        [HttpGet]
        public ActionResult GetList()
        {
            return Json(_organizationCategoryService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetOrganizationCategoryList(GridParameter parameters)
        {
            return Json(_organizationCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(OrganizationCategory organizationCategory)
        {
            if (ModelState.IsValid)
            {
                _organizationCategoryService.Insert(organizationCategory);
                return Json(new { OrganizationCategory = organizationCategory, Sequence = _organizationCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(OrganizationCategory organizationCategory)
        {
            if (ModelState.IsValid)
            {
                _organizationCategoryService.Update(organizationCategory);
                return Json(new { Sequence = _organizationCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _organizationCategoryService.Archive(id);
                return Json(new { Sequence = _organizationCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            return Json(_organizationCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_organizationCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the OrganizationCategory List.
        /// </summary>
        /// <returns>JsonResult.</returns>
        [HttpGet]
        public JsonResult GetOrganizationCatList()
        {
            return Json(new SelectList(_organizationCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
    }
}