using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Addresses;
using Library.Model.Organizations;
using Library.Model.Setups;
using Library.Service.Helpers;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Organizations.Controllers
{
    public class CompanyGroupController : BaseController
    {
        private readonly ICompanyGroupService _companyGroupService;

        public CompanyGroupController(ICompanyGroupService companyGroupService)
        {
            _companyGroupService = companyGroupService;
        }

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult UpdateCompanyGroup()
        {
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetName(string id)
        {
            return Json(_companyGroupService.GetCompanyGroupDisplayName(id), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult GetNameAndLogoDefault()
        {
            return Json(_companyGroupService.GetNameAndLogo(), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult GetNameAndLogo(string id)
        {
             return Json(_companyGroupService.GetNameAndLogo(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_companyGroupService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCompanyGroupCurrency()
        {
            return Json(new SelectList(_companyGroupService.GetCompanyGroupCurrency(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCompanyGrpList()
        {
            return Json(_companyGroupService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetcompanyGroup()
        {
            return Json(_companyGroupService.Query().Select().OrderBy(r => r.Sequence), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCompanyGroupList(GridParameter parameters)
        {
            return Json(_companyGroupService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCompanyGroupById(string id)
        {
            return Json(_companyGroupService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_companyGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FormCollection form)
        {
            var companyGroup = new JavaScriptSerializer().Deserialize<CompanyGroup>(form["companyGroup"]);
            var file = Request.Files["file"];
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png")
                    companyGroup.Image = Path.GetExtension(file.FileName);
                else
                    throw new CustomException(Resources.ImageUploadError);
            }
            _companyGroupService.Insert(companyGroup,
                    new JavaScriptSerializer().Deserialize<AddressMaster>(form["addressMaster"]),
                    new JavaScriptSerializer().Deserialize<ContactMaster>(form["contactMaster"]),
                    new JavaScriptSerializer().Deserialize<List<LocalLanguage>>(form["localLanguages"]));
            if (file != null && !string.IsNullOrEmpty(companyGroup.Image))
            {
                var path = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyGroup.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                    file.SaveAs(path);
            }
            return Json(new { CompanyGroup = companyGroup, Sequence = _companyGroupService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(FormCollection form)
        {
            var companyGroup = new JavaScriptSerializer().Deserialize<CompanyGroup>(form["companyGroup"]);
            var file = Request.Files["file"];
            if (file != null)
            {
                var directory = ResourcesPathReader.GetLogoOrImagePath();
                var replacepath = Path.Combine(directory);

                var fileId = "";
                var fileName = "";

                var data = _companyGroupService.GetDocFile(companyGroup.Id);
                if (data.Count > 0)
                {
                    if (!string.IsNullOrEmpty(data["Id"].ToString()) &&
                    !string.IsNullOrEmpty(data["Image"].ToString()))
                        fileId = data["Id"].ToString();
                    fileName = data["Image"].ToString();

                    if (System.IO.File.Exists(replacepath + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(replacepath + fileId + Path.GetExtension(fileName));
                }

                var extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png")
                {
                    companyGroup.Image = Path.GetExtension(file.FileName);
                    if (!string.IsNullOrEmpty(companyGroup.Image))
                        companyGroup.Image = companyGroup.Id + companyGroup.Image;
                }
                else
                    throw new CustomException(Resources.ImageUploadError);
            }
            _companyGroupService.Update(companyGroup,
                new JavaScriptSerializer().Deserialize<AddressMaster>(form["addressMaster"]),
                new JavaScriptSerializer().Deserialize<ContactMaster>(form["contactMaster"]),
                new JavaScriptSerializer().Deserialize<List<LocalLanguage>>(form["localLanguages"]));
            if (file != null && !string.IsNullOrEmpty(companyGroup.Image))
            {
                var path = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyGroup.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                    file.SaveAs(path);
            }
            return Json(new { CompanyGroup = companyGroup, Sequence = _companyGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _companyGroupService.Archive(id);
            return Json(new { Sequence = _companyGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public JsonResult DeleteLogo(string id)
        {
            var directory = ResourcesPathReader.GetLogoOrImagePath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _companyGroupService.GetDocFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["Id"].ToString()) &&
                !string.IsNullOrEmpty(data["Image"].ToString()))
                    fileId = data["Id"].ToString();
                fileName = data["Image"].ToString();

                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _companyGroupService.UpdateLogo(id);

            return Json(new { Message = "Logo removed successfully." });
        }

    }
}