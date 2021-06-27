#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class CandidateAdministrationController : BaseController
    {
        #region Constructor

        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IPreRecruitmentDocumentService _preRecruitmentDocumentService;

        public CandidateAdministrationController(IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IPreRecruitmentDocumentService preRecruitmentDocumentService)
        {
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _preRecruitmentDocumentService = preRecruitmentDocumentService;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult CandidateDocument()
        {
            return View();
        }

        [Authorize]
        public ActionResult CandidateDocumentAddRemove()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string plantId)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_preRecruitmentEmployeeService.GetAllCandidate(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(PreRecruitmentEmployee preRecruitmentEmployee)
        {
            _preRecruitmentEmployeeService.UpdateCandidate(preRecruitmentEmployee);
            return Json(new { PreRecruitmentEmployee = preRecruitmentEmployee, Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetCandidateDataWithAssignNonAssignDoc(GridParameter parameters, string assign, string plantId)
        {
            return Json(_preRecruitmentEmployeeService.GetCandidateDataWithAssignNonAssignDoc(parameters, assign, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateCandidateDocument(string candidateInfo)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<PreRecruitmentEmployee> candidate = JsonConvert.DeserializeObject<List<PreRecruitmentEmployee>>(candidateInfo, settings);

            _preRecruitmentDocumentService.CreateCandidateDocument(candidate);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet]
        public ActionResult GetDocumentDataList(string empId)
        {
            return Json(_preRecruitmentDocumentService.GetDocumentDataList(empId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateNewDOcument(IEnumerable<PreRecruitmentDocument> candidateDocument, string empId)
        {
            _preRecruitmentDocumentService.CreateNewDOcument(candidateDocument, empId);
            return Json(new { Message = AplosMessage.Success });
        }

        public JsonResult DeleteSingleDocument(string id)
        {
            var directory = ResourcesPathReader.GetDocumentSourcePath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _preRecruitmentDocumentService.GetDocFile(id);
            var fName = data["FileName"].ToString();
            if (!string.IsNullOrEmpty(fName))
            {
                throw new CustomException("This document cannot be deleted.");
            }
            else
            {
                if (data.Count > 0)
                {
                    if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
                    !string.IsNullOrEmpty(data["FileName"].ToString()))
                        fileId = data["FileId"].ToString();
                    fileName = data["FileName"].ToString();

                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }
                _preRecruitmentDocumentService.DeleteCandidateDocument(id);
            }
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet]
        public ActionResult GetEmpDocumentDataList(string companyGroupId, string pId, string plantId)
        {
            return Json(_preRecruitmentDocumentService.GetEmpDocumentDataList(companyGroupId, pId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDocumentList(string plantId, string empType, string budgetCode, string givenDesignationId)
        {
            return Json(_preRecruitmentDocumentService.GetDocumentList(plantId, empType, budgetCode, givenDesignationId), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}