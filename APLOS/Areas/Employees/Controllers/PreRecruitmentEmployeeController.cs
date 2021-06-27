#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using Library.Data;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Service.Helpers;
#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class PreRecruitmentEmployeeController : BaseController
    {
        #region Constructor
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployee;
        private readonly IPreRecruitmentEmpReferenceService _preRecruitmentEmpReference;
        private readonly IPreRecruitmentEmpQualificationService _preRecruitmentEmpQualification;
        private readonly IPreRecruitmentEmpExperienceService _preRecruitmentEmpExperience;
        private readonly IPreRecruitmentEmpTrainingService _preRecruitmentEmpTraining;
        public PreRecruitmentEmployeeController(
              IPreRecruitmentEmployeeService preRecruitmentEmployee
            , IPreRecruitmentEmpReferenceService preRecruitmentEmpReference
            , IPreRecruitmentEmpQualificationService preRecruitmentEmpQualification
            , IPreRecruitmentEmpExperienceService preRecruitmentEmpExperience
            , IPreRecruitmentEmpTrainingService preRecruitmentEmpTraining
            )
        {
            _preRecruitmentEmployee = preRecruitmentEmployee;
            _preRecruitmentEmpReference = preRecruitmentEmpReference;
            _preRecruitmentEmpQualification = preRecruitmentEmpQualification;
            _preRecruitmentEmpExperience = preRecruitmentEmpExperience;
            _preRecruitmentEmpTraining = preRecruitmentEmpTraining;
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
        public ActionResult GetList(string empid)
        {
            return Json(_preRecruitmentEmployee.GetData(empid), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetReferenceData(string id)
        {
            return Json(_preRecruitmentEmpReference.GetData(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetQualificationData(string empSystemID)
        {
            return Json(_preRecruitmentEmpQualification.GetData(empSystemID), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetExperienceData(string id)
        {
            return Json(_preRecruitmentEmpExperience.GetData(id), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetTrainingData(string id)
        {
            return Json(_preRecruitmentEmpTraining.GetData(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FormCollection form)
        {
            PreRecruitmentEmployee preRecruitmentEmployees = new JavaScriptSerializer().Deserialize<PreRecruitmentEmployee>(form["preRecruitmentEmployee"]);
            string empId = string.Empty;
            HttpPostedFileBase file = Request.Files["file"];
            if (file != null)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png")
                {
                    preRecruitmentEmployees.Image = Path.GetExtension(file.FileName);
                    if (!string.IsNullOrEmpty(preRecruitmentEmployees.Image.ToString()))
                        preRecruitmentEmployees.Image = preRecruitmentEmployees.Id + preRecruitmentEmployees.Image;
                }
                else
                    throw new CustomException(Resources.ImageUploadError);
            }
            _preRecruitmentEmployee.UpdateMaster(preRecruitmentEmployees);

            if (file != null)
            {
                string path = Path.Combine(ResourcesPathReader.GetEmployeeJobDescriptionPath()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeImage, typeof(string)).ToString())*/, preRecruitmentEmployees.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                    file.SaveAs(path);
            }
            return Json(new { empId, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult CreateAddress(PreRecruitmentEmployee preRecruitmentEmployee)
        {
            string empId = string.Empty;
            _preRecruitmentEmployee.UpdateMaster(preRecruitmentEmployee);
            return Json(new { PreRecruitmentEmployee = preRecruitmentEmployee, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult CreateReference(PreRecruitmentEmpReference preRecruitmentEmpReference)
        {
            string empId = string.Empty;
            _preRecruitmentEmpReference.InsertOrUpdate(preRecruitmentEmpReference);
            return Json(new { empId, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult CreateQualification(FormCollection form, HttpPostedFileBase[] file)
        {
            PreRecruitmentEmpQualification preRecruitmentEmpQualification = new JavaScriptSerializer().Deserialize<PreRecruitmentEmpQualification>(form["preRecruitmentEmpQualification"]);
            string empId = string.Empty;
            _preRecruitmentEmpQualification.InsertORUpdateMaster(preRecruitmentEmpQualification);
            string fileId = null;
            if (file.IsNotNull())
            {
                var directory = new AppSettingsReader().GetValue("QUALIFICATIONDOC", typeof(string)).ToString() + "/"; //get pic url from web config
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string path = System.IO.Path.Combine((Server.MapPath(directory)));

                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + System.IO.Path.GetExtension(item.FileName));
                        item.SaveAs(path + preRecruitmentEmpQualification.SystemID + System.IO.Path.GetExtension(item.FileName));
                    }
                }
            }
            return Json(new { empId, Message = AplosMessage.Success });
        }
        bool IsValidFile(string ext)
        {
            string[] validFileFormate = new string[] { "xlsx", "xlx", "doc", "docx", "jpg", "png", "gif", "pdf" };
            for (int i = 0; i < validFileFormate.Length; i++)
            {
                string vF = "." + validFileFormate[i];
                if (vF == ext)
                {
                    return true;
                }
            }
            return false;
        }

        //[HttpPost]
        //public JsonResult CreateQualification(PreRecruitmentEmpQualification preRecruitmentEmpQualification)
        //{
        //    string empId = string.Empty;
        //    _preRecruitmentEmpQualification.InsertORUpdateMaster(preRecruitmentEmpQualification, out empId);
        //    return Json(new { empId = empId, Message = AplosMessage.Success });
        //}

        //[HttpPost]
        //public JsonResult CreateExperience(PreRecruitmentEmpExperience preRecruitmentEmpExperience)
        //{
        //    string empId = string.Empty;
        //    _preRecruitmentEmpExperience.InsertORUpdateMaster(preRecruitmentEmpExperience, out empId);
        //    return Json(new { empId = empId, Message = AplosMessage.Success });
        //}
        [HttpPost]
        public JsonResult CreateExperience(FormCollection form, HttpPostedFileBase[] file)
        {
            PreRecruitmentEmpExperience preRecruitmentEmpExperience = new JavaScriptSerializer().Deserialize<PreRecruitmentEmpExperience>(form["preRecruitmentEmpExperience"]);
            string empId = string.Empty;
            _preRecruitmentEmpExperience.InsertORUpdateMaster(preRecruitmentEmpExperience);
            string fileId = null;
            if (file.IsNotNull())
            {
                var directory = new AppSettingsReader().GetValue("EXPERIENCEDOC", typeof(string)).ToString() + "/"; //get pic url from web config
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string path = System.IO.Path.Combine((Server.MapPath(directory)));

                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + System.IO.Path.GetExtension(item.FileName));
                        item.SaveAs(path + preRecruitmentEmpExperience.SystemID + System.IO.Path.GetExtension(item.FileName));
                    }
                }
            }
            return Json(new { empId, Message = AplosMessage.Success });
        }
        //[HttpPost]
        //public JsonResult CreateTraining(PreRecruitmentEmpTraining preRecruitmentEmpTraining)
        //{
        //    string empId = string.Empty;
        //    _preRecruitmentEmpTraining.InsertORUpdateMaster(preRecruitmentEmpTraining, out empId);
        //    return Json(new { empId = empId, Message = AplosMessage.Success });
        //}
        [HttpPost]
        public JsonResult CreateTraining(FormCollection form, HttpPostedFileBase[] file)
        {
            PreRecruitmentEmpTraining preRecruitmentEmpTraining = new JavaScriptSerializer().Deserialize<PreRecruitmentEmpTraining>(form["preRecruitmentEmpTraining"]);
            string empId = string.Empty;
            _preRecruitmentEmpTraining.InsertORUpdateMaster(preRecruitmentEmpTraining);
            string fileId = null;
            if (file.IsNotNull())
            {
                var directory = new AppSettingsReader().GetValue("EXPERIENCEDOC", typeof(string)).ToString() + "/"; //get pic url from web config
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string path = System.IO.Path.Combine((Server.MapPath(directory)));

                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + System.IO.Path.GetExtension(item.FileName));
                        item.SaveAs(path + preRecruitmentEmpTraining.SystemID + System.IO.Path.GetExtension(item.FileName));
                    }
                }
            }
            return Json(new { empId, Message = AplosMessage.Success });
        }

        public JsonResult DeleteQualification(string id)
        {
            _preRecruitmentEmpQualification.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public JsonResult DeleteExperience(string id)
        {
            _preRecruitmentEmpExperience.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public JsonResult DeleteTraining(string id)
        {
            _preRecruitmentEmpTraining.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}