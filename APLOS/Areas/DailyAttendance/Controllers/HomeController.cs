using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.DailyAttendance.Controllers
{
    [AllowAnonymous]
    public class HomeController : BaseController
    {
        #region Constructor

        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployee;
        private readonly IPreRecruitmentEmpReferenceService _preRecruitmentEmpReference;
        private readonly IPreRecruitmentEmpQualificationService _preRecruitmentEmpQualification;
        private readonly IPreRecruitmentEmpExperienceService _preRecruitmentEmpExperience;
        private readonly IPreRecruitmentEmpTrainingService _preRecruitmentEmpTraining;
        private readonly IPreRecruitmentDocumentService _preRecruitmentDocument;

        public HomeController(
              IPreRecruitmentEmployeeService preRecruitmentEmployee
            , IPreRecruitmentEmpReferenceService preRecruitmentEmpReference
            , IPreRecruitmentEmpQualificationService preRecruitmentEmpQualification
            , IPreRecruitmentEmpExperienceService preRecruitmentEmpExperience
            , IPreRecruitmentEmpTrainingService preRecruitmentEmpTraining
            , IPreRecruitmentDocumentService preRecruitmentDocument
            )
        {
            _preRecruitmentEmployee = preRecruitmentEmployee;
            _preRecruitmentEmpReference = preRecruitmentEmpReference;
            _preRecruitmentEmpQualification = preRecruitmentEmpQualification;
            _preRecruitmentEmpExperience = preRecruitmentEmpExperience;
            _preRecruitmentEmpTraining = preRecruitmentEmpTraining;
            _preRecruitmentDocument = preRecruitmentDocument;
        }

        #endregion Constructor

        #region -- Pages

        [HttpGet]
        public ActionResult Logout(string id)
        {
            var basePath = "";
#if DEBUG
            basePath = "";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (!string.IsNullOrEmpty(appName))
                basePath = "/" + appName + "";
#endif
            return Json(new { Id = id, BasePath = basePath, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Login(string id)
        {
            ViewBag.Id = id;
            return View();
        }

        [HttpPost]
        public ActionResult Login(string id, string pin)
        {
            HttpContext.Response.Cookies.Add(new HttpCookie("ROOT_FOLDRR", ResourcesPathReader.GetROOT_FOLDER()));
            return Json(new { IsFirstLogin = _preRecruitmentEmployee.Login(id, pin) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ChangePin(string id)
        {
            ViewBag.Id = id;
            return View();
        }

        [HttpPost]
        public ActionResult ChangePin(string id, string pin)
        {
            _preRecruitmentEmployee.UpdatePinAndLoginFlag(id, pin);
            return Json(new { Message = "Success" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos(string id)
        {
#if DEBUG
            ViewBag.BasePath = "/dailyAttendance";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (string.IsNullOrEmpty(appName))
                ViewBag.BasePath = "/dailyattendance";
            else
                ViewBag.BasePath = "/" + appName + "/dailyattendance";
#endif
            ViewBag.id = id;
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet]
        public ActionResult GetDocumentData(string companyGroupId, string budgetId, string plantId, string empType, string pId)
        {
            return Json(_preRecruitmentEmployee.GetDocumentData(companyGroupId, budgetId, plantId, empType, pId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDocumentDataList(string companyGroupId, string budgetId, string pId, string plantId)
        {
            return Json(_preRecruitmentEmployee.GetDocumentDataList(companyGroupId, budgetId, pId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(string empid)
        {
            return Json(_preRecruitmentEmployee.GetData(empid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPostOfficeName(GridParameter parameter, string sCountry, string sDistrict)
        {
            return Json(_preRecruitmentEmployee.SearchPostOfficeName(parameter, sCountry, sDistrict), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchCountryName(GridParameter parameter)
        {
            return Json(_preRecruitmentEmployee.SearchCountryName(parameter), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchCityName(GridParameter parameter, string countryId)
        {
            return Json(_preRecruitmentEmployee.SearchCityName(parameter, countryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SearchDistrictName(GridParameter parameter, string countryId)
        {
            return Json(_preRecruitmentEmployee.SearchDistrictName(parameter, countryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPoliceStationName(GridParameter parameter, string districtId)
        {
            return Json(_preRecruitmentEmployee.GetPoliceStationName(parameter, districtId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetJobData(string id)
        {
            return Json(_preRecruitmentEmployee.GetJobData(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetReferenceData(string id)
        {
            return Json(_preRecruitmentEmpReference.GetData(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetQualificationData(string id)
        {
            return Json(_preRecruitmentEmpQualification.GetData(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetExperienceData(string id)
        {
            return Json(_preRecruitmentEmpExperience.GetData(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTrainingData(string id)
        {
            return Json(_preRecruitmentEmpTraining.GetData(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FormCollection form)
        {
            var pre = form["preRecruitmentEmployee"];
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var preRecruitmentEmployees = JsonConvert.DeserializeObject<PreRecruitmentEmployee>(pre, settings);
            var empId = string.Empty;
            var file = Request.Files["file"];
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
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
                var path = Path.Combine(ResourcesPathReader.GetEmployeePicPath(), preRecruitmentEmployees.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                {
                    file.SaveAs(path);
                }
            }
            return Json(new { PreRecruitmentEmployee = preRecruitmentEmployees, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult CreatePersonal(PreRecruitmentEmployee preRecruitmentEmployee)
        {
            _preRecruitmentEmployee.UpdatePersonal(preRecruitmentEmployee);
            return Json(new { PreRecruitmentEmployee = preRecruitmentEmployee, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult CreateAddress(PreRecruitmentEmployee preRecruitmentEmployee)
        {
            _preRecruitmentEmployee.UpdateAddress(preRecruitmentEmployee);
            return Json(new { PreRecruitmentEmployee = preRecruitmentEmployee, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult CreateFinal(PreRecruitmentEmployee preRecruitmentEmployee)
        {
            _preRecruitmentEmployee.UpdateFinal(preRecruitmentEmployee);
            return Json(new { PreRecruitmentEmployee = preRecruitmentEmployee, Message = "Submitted Successfully. You won't able to change anything." });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult CreateReference(PreRecruitmentEmpReference preRecruitmentEmpReference)
        {
            _preRecruitmentEmpReference.InsertOrUpdate(preRecruitmentEmpReference);
            return Json(new { PreRecruitmentEmpReference = preRecruitmentEmpReference, Message = AplosMessage.Success });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult CreateQualification(FormCollection form, HttpPostedFileBase[] file)
        {
            var preRecruitmentEmpQualification = new JavaScriptSerializer().Deserialize<PreRecruitmentEmpQualification>(form["preRecruitmentEmpQualificationNew"]);

            var directory = ResourcesPathReader.GetQualificationSourcePath();
            var path = Path.Combine(directory);

            if (file.IsNotNull())
            {
                for (var i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }
            }

            var fileId = "";
            var fileName = "";
            var filedata = _preRecruitmentEmpQualification.GetQualificationFile(preRecruitmentEmpQualification.SystemID);
            if (filedata.Count > 0)
            {
                if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                    !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                    fileId = filedata["FileId"].ToString();
                fileName = filedata["FileName"].ToString();

                if (fileName != preRecruitmentEmpQualification.FileName)
                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }

            _preRecruitmentEmpQualification.InsertORUpdateMaster(preRecruitmentEmpQualification);
            if (file.IsNotNull())
            {
                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + preRecruitmentEmpQualification.SystemID + Path.GetExtension(item.FileName));
                    }
                }
            }
            return Json(new { PreRecruitmentEmpQualification = preRecruitmentEmpQualification, Message = AplosMessage.Success });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult CreateExperience(FormCollection form, HttpPostedFileBase[] file)
        {
            var preRecruitmentEmpExperience = new JavaScriptSerializer().Deserialize<PreRecruitmentEmpExperience>(form["preRecruitmentEmpExperienceNew"]);

            var directory = ResourcesPathReader.GetExperienceSourcePath();
            var path = Path.Combine(directory);
            if (file.IsNotNull())
            {
                for (var i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }
            }
            var fileId = "";
            var fileName = "";
            var filedata = _preRecruitmentEmpExperience.GetExperienceFile(preRecruitmentEmpExperience.SystemID);
            if (filedata.Count > 0)
            {
                if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                    !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                    fileId = filedata["FileId"].ToString();
                fileName = filedata["FileName"].ToString();

                if (fileName != preRecruitmentEmpExperience.FileName)
                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }

            _preRecruitmentEmpExperience.InsertORUpdateMaster(preRecruitmentEmpExperience);
            if (file.IsNotNull())
            {
                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + preRecruitmentEmpExperience.SystemID + Path.GetExtension(item.FileName));
                    }
                }
            }
            return Json(new { PreRecruitmentEmpExperience = preRecruitmentEmpExperience, Message = AplosMessage.Success });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult CreateTraining(FormCollection form, HttpPostedFileBase[] file)
        {
            var preRecruitmentEmpTraining = new JavaScriptSerializer().Deserialize<PreRecruitmentEmpTraining>(form["preRecruitmentEmpTrainingNew"]);

            var directory = ResourcesPathReader.GetTrainingSourcePath();
            var path = Path.Combine(directory);
            if (file.IsNotNull())
            {
                for (var i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }
            }
            var fileId = "";
            var fileName = "";
            var filedata = _preRecruitmentEmpTraining.GetTrainingFile(preRecruitmentEmpTraining.SystemID);
            if (filedata.Count > 0)
            {
                if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                    !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                    fileId = filedata["FileId"].ToString();
                fileName = filedata["FileName"].ToString();

                if (fileName != preRecruitmentEmpTraining.FileName)
                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }

            _preRecruitmentEmpTraining.InsertORUpdateMaster(preRecruitmentEmpTraining);

            if (file.IsNotNull())
            {
                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + preRecruitmentEmpTraining.SystemID + Path.GetExtension(item.FileName));
                    }
                }
            }
            return Json(new { PreRecruitmentEmpTraining = preRecruitmentEmpTraining, Message = AplosMessage.Success });
        }

        public JsonResult DeleteQualification(string id)
        {
            var directory = ResourcesPathReader.GetQualificationSourcePath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _preRecruitmentEmpQualification.GetQualificationFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
                !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["FileId"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _preRecruitmentEmpQualification.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public JsonResult DeleteExperience(string id)
        {
            var directory = ResourcesPathReader.GetExperienceSourcePath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _preRecruitmentEmpExperience.GetExperienceFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
            !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["FileId"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _preRecruitmentEmpExperience.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public JsonResult DeleteTraining(string id)
        {
            var directory = ResourcesPathReader.GetTrainingSourcePath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _preRecruitmentEmpTraining.GetTrainingFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
            !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["FileId"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _preRecruitmentEmpTraining.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public JsonResult DeleteDocument(string id)
        {
            var directory = ResourcesPathReader.GetDocumentSourcePath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _preRecruitmentDocument.GetDocFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
                !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["FileId"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _preRecruitmentDocument.UpdatePreRecruitmentDocument(id);

            return Json(new { Message = "File detach successfully." });
        }

        private bool DeleteDoc(List<PreRecruitmentDocument> detailList)
        {
            try
            {
                var directory = ResourcesPathReader.GetDocumentSourcePath();
                var path = Path.Combine(directory);

                var data = _preRecruitmentDocument.GetDocumentFile(detailList[0].PreRecruitmentEmployeeId);
                if (data != null)
                {
                    foreach (var item in detailList)
                    {
                        var loList = data.FirstOrDefault(r => r.Id == item.Id);
                        if (loList != null)
                        {
                            var fn = loList.FileName;
                            var id = loList.Id;

                            if (item.FileName != fn)
                            {
                                if (System.IO.File.Exists(path + id + Path.GetExtension(fn)))
                                    System.IO.File.Delete(path + id + Path.GetExtension(fn));
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult CreateDocument(FormCollection form, HttpPostedFileBase[] file)
        {
            var preRecruitmentDocument = new JavaScriptSerializer().Deserialize<PreRecruitmentDocument>(form["preRecruitmentDocument"]);

            var directory = ResourcesPathReader.GetDocumentSourcePath();
            var path = Path.Combine(directory);
            if (file.IsNotNull())
            {
                for (var i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }
            }
            var fileId = "";
            var fileName = "";
            var filedata = _preRecruitmentDocument.GetDocFile(preRecruitmentDocument.Id);
            if (filedata.Count > 0)
            {
                if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                    !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                    fileId = filedata["FileId"].ToString();
                fileName = filedata["FileName"].ToString();

                if (fileName != preRecruitmentDocument.FileName)
                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }

            _preRecruitmentDocument.InsertORUpdateMaster(preRecruitmentDocument);
            if (file.IsNotNull())
            {
                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + preRecruitmentDocument.Id + Path.GetExtension(item.FileName));
                    }
                }
            }
            return Json(new { PreRecruitmentDocument = preRecruitmentDocument, Message = AplosMessage.Success });
        }

        private static string GetDoc(List<PreRecruitmentDocument> doc, string fileName)
        {
            return doc.Find(r => r.FileName == fileName).ComplianceDocumentId;
        }

        private static string GetFileId(IEnumerable<PreRecruitmentDocument> list, string fileName)
        {
            foreach (var item in list)
            {
                if (item.FileName == fileName)
                {
                    return item.Id;
                }
            }
            return "";
        }

        private static string GetFileName(IEnumerable<PreRecruitmentDocument> list, string fileid)
        {
            foreach (var item in list)
            {
                if (item.FileId == fileid)
                {
                    return item.FileName;
                }
            }
            return "";
        }

        #endregion -- Operations
    }
}