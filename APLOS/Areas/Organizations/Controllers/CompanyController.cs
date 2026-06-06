using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Addresses;
using Library.Model.Organizations;
using Library.Model.Setups;
using Library.Service.Helpers;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Organizations.Controllers
{
    public class CompanyController : BaseController
    {
        #region Constructor

        private readonly ICompanyService _companyService;
        private readonly ISqlRepository _sqlRepository;

        public CompanyController(ICompanyService companyService, ISqlRepository sqlRepository)
        {
            _companyService = companyService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetGroupAndCompanyPKPrefix()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyService.GetGroupAndCompanyPKPrefix(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            if (string.IsNullOrEmpty(companyGroupId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                companyGroupId = identity.CompanyGroupId;
            }
            return Json(_companyService.GetCboCompanyByCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCboInterCompany(string companyGroupId)
        {
            return Json(_companyService.GetCboInterCompany(companyGroupId), JsonRequestBehavior.AllowGet);
        }

       
        [HttpGet, AllowAnonymous]
        public JsonResult GetCboByCOA(string coaId)
        {
            return Json(GetCboDataByCOA(coaId), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetCboDataByCOA(string coaId)
        {
            return _sqlRepository.GetDataCollection("SELECT UserName AS [Text],Id AS Value FROM ORG.Company  AS c WHERE COAId='" + coaId + "' And Active=1 Order By Sequence");
        }

        [HttpGet, Authorize]
        public JsonResult GetCompanyCurrencyList()
        {
            return Json(new SelectList(_companyService.GetCboCompanyCurrency(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCompanyCurrency(string param1)
        {
            return Json(new SelectList(_companyService.GetCboCompanyCurrencyByCompany(param1), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllCompanyList()
        {
            return Json(new SelectList(_companyService.Query().Select(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCompanyConfiguration()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyService.GetCompanyConfiguration(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

  
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult OrganizationCategory()
        {
            return View();
        }

        [Authorize]
        public ActionResult OrganizationClass()
        {
            return View();
        }

        [Authorize]
        public ActionResult UpdateCompany()
        {
            return View();
        }

        #region ---Operation

        [HttpGet]
        public ActionResult GetList(GridParameter parameters, string companyGroupId)
        {
            return Json(_companyService.Query(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCompanyById(string id)
        {
            return Json(_companyService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_companyService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FormCollection form)
        {
            var company = new JavaScriptSerializer().Deserialize<Company>(form["company"]);
            var file = Request.Files["file"];
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png")
                    company.Image = Path.GetExtension(file.FileName);
                else
                    throw new CustomException(Resources.ImageUploadError);
            }
            _companyService.Insert(company,
                    new JavaScriptSerializer().Deserialize<AddressMaster>(form["addressMaster"]),
                    new JavaScriptSerializer().Deserialize<ContactMaster>(form["contactMaster"]),
                    new JavaScriptSerializer().Deserialize<List<LocalLanguage>>(form["localLanguages"]));
            if (file != null && !string.IsNullOrEmpty(company.Image))
            {
                //TO DO Have to change Path
                var path = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), company.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                    file.SaveAs(path);
            }
            return Json(new { Company = company, Sequence = _companyService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(FormCollection form)
        {
            var company = new JavaScriptSerializer().Deserialize<Company>(form["company"]);
            var file = Request.Files["file"];
            if (file != null)
            {
                var directory = ResourcesPathReader.GetLogoOrImagePath();
                var replacepath = Path.Combine(directory);

                var fileId = "";
                var fileName = "";

                var data = _companyService.GetDocFile(company.Id);
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
                    company.Image = Path.GetExtension(file.FileName);
                    if (!string.IsNullOrEmpty(company.Image))
                        company.Image = company.Id + company.Image;
                }
                else
                    throw new CustomException(Resources.ImageUploadError);
            }
            _companyService.Update(company,
                new JavaScriptSerializer().Deserialize<AddressMaster>(form["addressMaster"]),
                new JavaScriptSerializer().Deserialize<ContactMaster>(form["contactMaster"]),
                new JavaScriptSerializer().Deserialize<List<LocalLanguage>>(form["localLanguages"]));
            if (file == null || string.IsNullOrEmpty(company.Image))
                return Json(new
                {
                    Company = company,
                    Sequence = _companyService.GetAutoSequence(),
                    Message = AplosMessage.Updated
                });
            // TODO: plz change path
            var path = Path.Combine(ResourcesPathReader.GetLogoOrImagePath()/*Server.MapPath(UrlResources.OrganizationLogoOrImage)*/, company.Image);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                file.SaveAs(path);
            }
            else
                file.SaveAs(path);
            return Json(new { Company = company, Sequence = _companyService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _companyService.Archive(id);
            return Json(new { Sequence = _companyService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public JsonResult DeleteLogo(string id)
        {
            var directory = ResourcesPathReader.GetLogoOrImagePath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _companyService.GetDocFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["Id"].ToString()) &&
                !string.IsNullOrEmpty(data["Image"].ToString()))
                    fileId = data["Id"].ToString();
                fileName = data["Image"].ToString();

                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _companyService.UpdateLogo(id);

            return Json(new { Message = "Logo removed successfully." });
        }

        
        #endregion ---Operation

        #region Company COA

        public ActionResult CompanyCoa()
        {
            return View();
        }

        public ActionResult GetCompanyCOAList(GridParameter parameters)
        {
            return Json(_companyService.GetCompanyCoaList(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CompanyCOAEdit(Company company)
        {
            _companyService.CompanyConfigUpdate(company);
            return Json(new { Message = AplosMessage.Updated });
        }

        #endregion Company COA
    }
}