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
    public class PostRecruitmentDocumentByDepartmentController : BaseController
	{
		#region Constructor

	    private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
		private readonly IPostRecruitmentDocumentService _postRecruitmentDocumentService;
		public PostRecruitmentDocumentByDepartmentController(
                IPreRecruitmentEmployeeService preRecruitmentEmployeeService
			, IPostRecruitmentDocumentService postRecruitmentDocumentService
			)
		{
		    _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
			_postRecruitmentDocumentService = postRecruitmentDocumentService;

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
				var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "PostRecruitmentOrgDocRP", identity.EmployeeId);
				if (entity == null || !entity.Any())
					throw new CustomException(string.Format(ServiceResources.EmployeeNotMapWithEntity));
			}
			string message = "";
			if (identity.IsSysAdmin)
				message = ServiceResources.PreRecruitmentSysAdmin;
			return Json(new
			{
				Message = message,
				Data = _postRecruitmentDocumentService.GetAllEmployee(parameters, identity.IsControlAdmin, identity.IsSysAdmin, identity.CompanyGroupId, identity.CompanyId, identity.EmployeeId)
			}, JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetEntityByEmployee()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "PostRecruitmentOrgDocRP", identity.EmployeeId);
			return Json(entity, JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetDocumentData(string companyGroupId, string budgetId, string plantId, string pId)
		{
			return Json(_postRecruitmentDocumentService.GetDocumentData(companyGroupId, budgetId, plantId, pId), JsonRequestBehavior.AllowGet);
		}

		public JsonResult DeleteDocument(string id)
		{
			var directory = ResourcesPathReader.GetDocumentDestinationPath();
			string path = Path.Combine(directory);
			var fileId = "";
			var fileName = "";
			var data = _postRecruitmentDocumentService.GetDocFile(id);
			if (data.Count > 0)
			{
				if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
				!string.IsNullOrEmpty(data["FileName"].ToString()))
					fileId = data["FileId"].ToString();
				fileName = data["FileName"].ToString();
				if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
					System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
			}
			_postRecruitmentDocumentService.UpdatePostRecruitmentDocument(id);
			return Json(new { Message = "File detach successfully." });
		}


		private bool DeleteDoc(List<EmployeeDocument> detailList)
		{
			try
			{
				var directory = ResourcesPathReader.GetDocumentDestinationPath();
				string path = Path.Combine(directory);

				var data = _postRecruitmentDocumentService.GetDocumentFile(detailList[0].EmpSystemID);
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

		[HttpPost]
		public JsonResult CreateDocument(FormCollection form, HttpPostedFileBase[] file)
		{
			EmployeeDocument employeeDocument = new JavaScriptSerializer().Deserialize<EmployeeDocument>(form["employeeDocument"]);

			var directory = ResourcesPathReader.GetDocumentDestinationPath();
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
			var filedata = _postRecruitmentDocumentService.GetDocFile(employeeDocument.Id);
			if (filedata.Count > 0)
			{
				if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
					!string.IsNullOrEmpty(filedata["FileName"].ToString()))
					fileId = filedata["FileId"].ToString();
				fileName = filedata["FileName"].ToString();

				if (fileName != employeeDocument.FileName)
					if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
						System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
			}

			_postRecruitmentDocumentService.InsertORUpdate(employeeDocument);
			if (file.IsNotNull())
			{
				foreach (var item in file)
				{
					if (item != null)
					{
						if (System.IO.File.Exists(path + item.FileName))
							System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
						item.SaveAs(path + employeeDocument.Id + Path.GetExtension(item.FileName));
					}
				}
			}

			return Json(new { EmployeeDocument = employeeDocument, Message = AplosMessage.Success });
		}

		[HttpPost, ChaildAction(ParentActionName = "Create")]
		public JsonResult CreateDeptDocument(FormCollection form, HttpPostedFileBase[] file, string empId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			List<EmployeeDocument> detailList = System.Web.Helpers.Json.Decode<List<EmployeeDocument>>(form["employeeDocument"]);
			List<EmployeeDocument> preRecruitmentDocument = new List<EmployeeDocument>();

			var directory = ResourcesPathReader.GetDocumentDestinationPath();
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
				_postRecruitmentDocumentService.InsertGraph(detailList, detailList[0].EmpSystemID);
			}
			var empData = _preRecruitmentEmployeeService.Find(empId);
			empData.IsDepartmentSubmit = true;
			empData.DeptDocumentBy = identity.FullName;
			empData.DeptDocumentDateTime = DateTime.Now;
			_preRecruitmentEmployeeService.Update(empData);
			return Json(new { PreRecruitmentDocument = preRecruitmentDocument, Message = AplosMessage.Success });
		}

		private string GetDoc(List<EmployeeDocument> doc, string fileName)
		{
			return doc.Find(r => r.FileName == fileName).ComplianceDocumentId;
		}

		private string GetFileId(IEnumerable<EmployeeDocument> list, string fileName)
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

		private string GetFileName(IEnumerable<EmployeeDocument> list, string fileid)
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

		#endregion
	}
}