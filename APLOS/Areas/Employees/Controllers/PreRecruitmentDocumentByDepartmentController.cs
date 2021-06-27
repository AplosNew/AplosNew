#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.Properties;
using Library.Service.Securites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class PreRecruitmentDocumentByDepartmentController : BaseController
    {
        #region Constructor
        private readonly IUserService _userService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IPreRecruitmentDocumentService _preRecruitmentDocumentService;
        public PreRecruitmentDocumentByDepartmentController(
              IUserService userService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IPreRecruitmentDocumentService preRecruitmentDocumentService
            )
        {
            _userService = userService;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _preRecruitmentDocumentService = preRecruitmentDocumentService;

        }
        #endregion

        #region -- Pages
      
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
			if ((!identity.IsControlAdmin && !identity.IsSysAdmin))
			{
				if (string.IsNullOrEmpty(identity.EmployeeId))
					throw new CustomException(string.Format(ServiceResources.EmployeeNotMap));
				var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "OrgDocRP", identity.EmployeeId);
				if (entity == null || !entity.Any())
					throw new CustomException(string.Format(ServiceResources.EmployeeNotMapWithEntity));
			}
			string message = "";
			if (identity.IsSysAdmin)
				message = ServiceResources.PreRecruitmentSysAdmin.ToString();
			return Json(new
			{
				Message = message,
				Data = _preRecruitmentDocumentService.GetSubmittedEmployee(parameters, identity.IsControlAdmin, identity.IsSysAdmin, identity.CompanyGroupId, identity.CompanyId, identity.EmployeeId)
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
        public ActionResult GetEntityByEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "OrgDocRP", identity.EmployeeId);
            return Json(entity, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetDocumentData(string companyGroupId, string budgetId, string plantId, string empType, string pId)
        {
            return Json(_preRecruitmentDocumentService.GetDocumentData(companyGroupId, budgetId, plantId, empType, pId), JsonRequestBehavior.AllowGet);
        }

		[HttpGet, Authorize]
		public ActionResult GetDocumentDataList(string companyGroupId, string budgetId, string pId, string plantId)
		{
			return Json(_preRecruitmentDocumentService.GetDocumentDataList(companyGroupId, budgetId, pId, plantId), JsonRequestBehavior.AllowGet);
		}
        [Authorize]
		public JsonResult DeleteDocument(string id)
        {
            var directory = ResourcesPathReader.GetDocumentSourcePath();
            string path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _preRecruitmentDocumentService.GetDocFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
                !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["FileId"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
			_preRecruitmentDocumentService.UpdatePreRecruitmentDocument(id);
			return Json(new { Message = "File detach successfully." });
        }

        private bool DeleteDoc(List<PreRecruitmentDocument> detailList)
        {
            try
            {
                var directory = ResourcesPathReader.GetDocumentSourcePath();
                string path = Path.Combine(directory);

                var data = _preRecruitmentDocumentService.GetDocumentFile(detailList[0].PreRecruitmentEmployeeId);
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

        [HttpPost, Authorize]
        public JsonResult CreateDocument(FormCollection form, HttpPostedFileBase[] file)
        {
            List<PreRecruitmentDocument> detailList = System.Web.Helpers.Json.Decode<List<PreRecruitmentDocument>>(form["preRecruitmentDocument"]);
            List<PreRecruitmentDocument> preRecruitmentDocument = new List<PreRecruitmentDocument>();

            var directory = ResourcesPathReader.GetDocumentSourcePath();
            string path = Path.Combine(directory);
            if (detailList.Count() > 0)
            {
                DeleteDoc(detailList);
                _preRecruitmentDocumentService.InsertGraph(detailList, detailList[0].PreRecruitmentEmployeeId);
            }
            else
            {
                throw new CustomException("No data found.");
            }
            if (file.IsNotNull())
            {
                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }
            }
            string fileId = null;
            if (file.IsNotNull())
            {
                foreach (var item in file)
                {
                    fileId = GetFileId(detailList, item.FileName);
                    System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                    item.SaveAs(path + fileId + Path.GetExtension(item.FileName));
                }
            }
            return Json(new { PreRecruitmentDocument = preRecruitmentDocument, Message = AplosMessage.Success });
        }
        [HttpPost,Authorize]
        public JsonResult CreateDeptDocument(FormCollection form, HttpPostedFileBase[] file, string empId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            List<PreRecruitmentDocument> detailList = System.Web.Helpers.Json.Decode<List<PreRecruitmentDocument>>(form["preRecruitmentDocument"]);
            List<PreRecruitmentDocument> preRecruitmentDocument = new List<PreRecruitmentDocument>();

            var directory = ResourcesPathReader.GetDocumentSourcePath();
            string path = Path.Combine(directory);

            if (file.IsNotNull())
            {
                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }
            }
            string fileId = null;
            if (file.IsNotNull())
            {
                foreach (var item in file)
                {
                    fileId = GetFileId(detailList, item.FileName);
                    System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                    item.SaveAs(path + fileId + Path.GetExtension(item.FileName));
                }
            }
            if (detailList.Count() > 0)
            {
                DeleteDoc(detailList);
                _preRecruitmentDocumentService.InsertGraph(detailList, detailList[0].PreRecruitmentEmployeeId);
            }
            var empData = _preRecruitmentEmployeeService.Find(empId);
            empData.IsDepartmentSubmit = true;
            empData.DeptDocumentBy = identity.FullName;
            empData.DeptDocumentDateTime = DateTime.Now;
            _preRecruitmentEmployeeService.Update(empData);
            return Json(new { PreRecruitmentDocument = preRecruitmentDocument, Message = AplosMessage.Success });
        }

        private string GetDoc(List<PreRecruitmentDocument> doc, string fileName)
        {
            return doc.Find(r => r.FileName == fileName).ComplianceDocumentId;
        }

        private string GetFileId(IEnumerable<PreRecruitmentDocument> list, string fileName)
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

        private string GetFileName(IEnumerable<PreRecruitmentDocument> list, string fileid)
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

		[HttpPost]
		public JsonResult CreateDepartmentDocument(FormCollection form, HttpPostedFileBase[] file)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			PreRecruitmentDocument preRecruitmentDocument = new JavaScriptSerializer().Deserialize<PreRecruitmentDocument>(form["preRecruitmentDocument"]);

			var directory = ResourcesPathReader.GetDocumentSourcePath();
			string path = Path.Combine(directory);
			if (file.IsNotNull())
			{
				for (int i = 0; i < file.Length; i++)
				{
					ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
				}
			}
			var fileId = "";
			var fileName = "";
			var filedata = _preRecruitmentDocumentService.GetDocFile(preRecruitmentDocument.Id);
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

			_preRecruitmentDocumentService.InsertORUpdate(preRecruitmentDocument);
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
			var empData = _preRecruitmentEmployeeService.Find(preRecruitmentDocument.PreRecruitmentEmployeeId);
			if (empData.IsDepartmentSubmit == false)
			{
				empData.IsDepartmentSubmit = true;
				empData.DeptDocumentBy = identity.FullName;
				empData.DeptDocumentDateTime = DateTime.Now;
				_preRecruitmentEmployeeService.Update(empData);
			}
			return Json(new { PreRecruitmentDocument = preRecruitmentDocument, Message = AplosMessage.Success });
		}
		#endregion
	}
}