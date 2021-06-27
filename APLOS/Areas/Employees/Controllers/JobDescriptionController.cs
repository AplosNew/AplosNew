#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class JobDescriptionController : BaseController
    {
        #region Constructor
        private readonly IJobDescriptionService _jobDescriptionService;
        private readonly IJobDescriptionDetailService _jobDescriptionDetailService;
        public JobDescriptionController(
              IJobDescriptionService jobDescriptionService
              , IJobDescriptionDetailService jobDescriptionDetail
            )
        {
            _jobDescriptionService = jobDescriptionService;
            _jobDescriptionDetailService = jobDescriptionDetail;
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
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_jobDescriptionService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetJobDescriptionList(GridParameter parameters, string jobDescriptionIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_jobDescriptionService.Query(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(jobDescriptionIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailList(GridParameter parameters, string jobDescriptionId)
        {
            return Json(_jobDescriptionDetailService.Query(parameters, jobDescriptionId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FormCollection form, HttpPostedFileBase[] file)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jobDescriptionDetail = new List<JobDescriptionDetail>();
            var jobDescription = new JavaScriptSerializer().Deserialize<JobDescription>(form["jobDescription"]);
            jobDescription.CompanyGroupId = identity.CompanyGroupId;
            string extension = null;
            string fileId = null;
            if (file.IsNotNull())
            {
                for (int i = 0; i < file.Length; i++)
                {
                    extension = Path.GetExtension(file[i].FileName);
                    if (!IsValidFile(extension))
                        throw new CustomException("File Formate is not valid");
                    var ob = new JobDescriptionDetail
                    {
                        FileName = file[i].FileName
                    };
                    jobDescriptionDetail.Add(ob);
                }
            }
            _jobDescriptionService.InsertGraph(jobDescription, jobDescriptionDetail);
            if (file.IsNotNull())
            {
                string path = Path.Combine(ResourcesPathReader.GetEmployeeJobDescriptionPath()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeJobDescription, typeof(string)).ToString())*/) +"/";
                foreach (var item in file)
                {
                    System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                    fileId = GetFileId(jobDescriptionDetail, item.FileName);
                    item.SaveAs(path + fileId + Path.GetExtension(item.FileName));
                }
            }
            return Json(new { JobDescription = jobDescription, Message = AplosMessage.Success });
        }

        bool IsValidFile(string ext)
        {
            string[] validFileFormate = { "xlsx", "xlx", "doc", "docx", "jpg", "png", "gif", "pdf" };
            for (var i = 0; i < validFileFormate.Length; i++)
            {
                string vF = "." + validFileFormate[i];
                if (vF == ext)
                {
                    return true;
                }
            }
            return false;
        }

        private string GetFileId(List<JobDescriptionDetail> list, string fileName)
        {
            foreach (var ob in list)
            {
                if (ob.FileName == fileName)
                {
                    return ob.FileId;
                }
            }

            return "";
        }

        [HttpPost]
        public JsonResult Edit(FormCollection form, HttpPostedFileBase[] file)
        {
            var jobDescriptionDetail = new List<JobDescriptionDetail>();
            var jobDescription = new JavaScriptSerializer().Deserialize<JobDescription>(form["jobDescription"]);
            string fileId = null;
            if (file.IsNotNull())
            {
                for (var i = 0; i < file.Length; i++)
                {
                    if (file[i].IsNotNull())
                    {
                        Path.GetExtension(file[i].FileName);
                        JobDescriptionDetail ob = new JobDescriptionDetail
                        {
                            FileName = file[i].FileName
                        };
                        jobDescriptionDetail.Add(ob);
                    }
                }
            }
            _jobDescriptionService.UpdateGraph(jobDescription, jobDescriptionDetail);
            if (file.IsNotNull())
            {
				//TO Do path change
                string path = Path.Combine(ResourcesPathReader.GetEmployeeJobDescriptionPath()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeJobDescription, typeof(string)).ToString())*/) + "/";
                foreach (var item in file)
                {
                    if (item.IsNotNull())
                    {
                        System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        fileId = GetFileId(jobDescriptionDetail, item.FileName);
                        item.SaveAs(path + fileId + Path.GetExtension(item.FileName));
                    }
                }
            }
            return Json(new { JobDescription = jobDescription, Message = AplosMessage.Success });
        }
        public ActionResult Delete(string id)
        {
            _jobDescriptionService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        public ActionResult JobDescriptionDetailDelete(string id, string fileId, string fileName)
        {
            _jobDescriptionDetailService.Delete(id);
            var fullPath = Path.Combine(ResourcesPathReader.GetEmployeeJobDescriptionPath()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeJobDescription, typeof(string)).ToString())*/) + "/";
            if (System.IO.File.Exists(fullPath + fileId + Path.GetExtension(fileName)))
                System.IO.File.Delete(fullPath + fileId + Path.GetExtension(fileName));
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}