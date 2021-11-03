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
    public class OrganizationClassController : BaseController
    {
        #region -- Constructor

        private readonly IOrganizationClassService _organizationClassService;

        public OrganizationClassController(IOrganizationClassService organizationClassService)
        {
            _organizationClassService = organizationClassService;
        }

        #endregion -- Constructor

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetList()
        {
            return Json(_organizationClassService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetOrganizationClassList(GridParameter parameters)
        {
            return Json(_organizationClassService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(OrganizationClass organizationClass)
        {
            if (ModelState.IsValid)
            {
                _organizationClassService.Insert(organizationClass);
                return Json(new { OrganizationClass = organizationClass, Sequence = _organizationClassService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(OrganizationClass organizationClass)
        {
            if (ModelState.IsValid)
            {
                _organizationClassService.Update(organizationClass);
                return Json(new { Sequence = _organizationClassService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _organizationClassService.Archive(id);
                return Json(new { Sequence = _organizationClassService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            return Json(_organizationClassService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_organizationClassService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the OrganizationCategory List.
        /// </summary>
        /// <returns>JsonResult.</returns>
        [HttpGet]
        public JsonResult GetOrganizationClsList()
        {
            return Json(new SelectList(_organizationClassService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
    }
}