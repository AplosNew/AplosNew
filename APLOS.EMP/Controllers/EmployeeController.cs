#region Using
using Library.Core;
using Library.Data;
using Library.Model.External;
using Library.Service.External;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion

namespace Aplos.Controllers
{
    public class EmployeeController : BaseController
    {
        #region Constructor
        private readonly IEmployeeService _employeeService;
        private readonly IActivityService _activityService;
        private readonly IDocumentActivityService _documentActivityService;
        private readonly IKPIService _kpiService;

        public EmployeeController(IEmployeeService employeeService
            , IActivityService activityService
            , IDocumentActivityService documentActivityService
            , IKPIService kpiService)
        {
            _employeeService = employeeService;
            _activityService = activityService;
            _documentActivityService = documentActivityService;
            _kpiService = kpiService;
        }
        #endregion

        #region Pages
        [AllowAnonymous]
        public ActionResult Login(string id)
        {
            ViewBag.Id = id;
            ViewBag.ControllerName = "employeeLoginController";
            return View();
        }
        [AllowAnonymous]
        public ActionResult PinChange()
        {
            ViewBag.ControllerName = "employeePinChangeController";
            return View();
        }
        [AllowAnonymous]
        public ActionResult Aplos(string id)
        {
            ViewBag.ControllerName = "employeeController";
            ViewBag.Id = id;
            return View();
        }
        #endregion

        #region Operation

        [HttpPost, AllowAnonymous]
        public ActionResult Login(string id, string initialpin)
        {
            return Json(_employeeService.Login(id, initialpin), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public ActionResult GetList(string id)
        {
            return Json(_employeeService.Query(id), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, AllowAnonymous]
        public ActionResult GetEmployeeList(string id, string initialpin)
        {
            return Json(_employeeService.QueryEmployeeAccess(id, initialpin), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, AllowAnonymous]
        public ActionResult GetDataList(string id)
        {
            return Json(_employeeService.QueryList(id), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, AllowAnonymous]
        public ActionResult GetNameList(GridParameter parameters, string companyGroupId, string id)
        {
            return Json(_employeeService.QueryReportingOfficer(parameters, companyGroupId, id), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, AllowAnonymous]
        public ActionResult GetEmployeeListByCompanyGroup(GridParameter parameters, string companyGroupId)
        {
            return Json(_employeeService.GetEmployeeListByCompanyGroup(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Update(Employee employee)
        {
            string empId = string.Empty;
            _employeeService.Update(employee);
            return Json(new { Employee = employee, Message = "Save Successful" });
        }
        [HttpPost]
        public JsonResult UpdatePIN(string id, string newPin)
        {
            string empId = string.Empty;
            _employeeService.UpdatePIN(id, newPin);
            return Json(new { Message = "Pin update successful. Please login with your new pin." });
        }
        [HttpPost]
        public JsonResult UpdateUserAccess(Employee employee)
        {
            string empId = string.Empty;
            _employeeService.UpdateUserAccess(employee);
            return Json(new { Employee = employee, Message = "Save Successful" });
        }
        [HttpPost]
        public JsonResult UpdateEmployeeSubmit(Employee employee)
        {
            string empId = string.Empty;
            _employeeService.UpdateEmployeeSubmit(employee);
            return Json(new { Employee = employee, Message = "Save Successful" });
        }
        [HttpPost]
        public JsonResult UpdateSubmit(Employee employee)
        {
            string empId = string.Empty;
            _employeeService.UpdateSubmit(employee);
            return Json(new { Employee = employee, Message = "Save Successful" });
        }
        [HttpGet, AllowAnonymous]
        public JsonResult GetCboList(string companyGroupId)
        {
            return Json(_employeeService.GetCboList(companyGroupId).Rows, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, AllowAnonymous]
        public JsonResult GetNameCboList()
        {
            return Json(_employeeService.GetNameCboList().Rows, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, AllowAnonymous]
        public JsonResult GetDynamicData(string employeeId)
        {
            return Json(_employeeService.GetDynamicData(employeeId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, AllowAnonymous]
        public ActionResult GetEmployeeByCompanyGroup(GridParameter parameters, string companyGroupId)
        {
            return Json(_employeeService.GetEmployeeByCompanyGroup(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, AllowAnonymous]
        public ActionResult GetEmployeeByCompanyGroupAndSubmit(GridParameter parameters, string companyGroupId)
        {
            return Json(_employeeService.GetEmployeeByCompanyGroupAndSubmit(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Activity
        [HttpPost]
        public JsonResult SaveActivity(ActivityEmp activity)
        {
             _activityService.InsertOrUpdate(activity);
            return Json(new { Activity = activity, Message = "Save Successful" });
        }

        public ActionResult GetActivityList(GridParameter parameters, string employeeId)
        {
            return Json(_activityService.Query(parameters, employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateActivity(string id, string fieldName)
        {
            string empId = string.Empty;
            _activityService.UpdateActivity(id, fieldName);
            return Json(new { Message = "" });
        }

        [HttpPost]
        public ActionResult ActivityDelete(string id)
        {

            if (!string.IsNullOrEmpty(id))
            {
                _activityService.Delete(id);
                return Json(new { Message = "Delete Successful" });
            }
            else
                throw new CustomException("Id not Found");
        }

        public JsonResult GetActivityCboList(string employeeId)
        {
            return Json(_activityService.GetCbo(employeeId).Rows, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetKPICboList(string employeeId)
        {
            return Json(_activityService.GetKPICbo(employeeId).Rows, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region DocumentAcitvity
        [HttpPost]
        public JsonResult Create(FormCollection form, HttpPostedFileBase[] file)
        {
            DocumentActivity documentActivity = new JavaScriptSerializer().Deserialize<DocumentActivity>(form["documentActivityNew"]);
            var data = _employeeService.GetEmployee(documentActivity.EmployeeId);
            var folderName = "";

            if (!string.IsNullOrEmpty(data["DocumentFolderName"].ToString()) &&
                !string.IsNullOrEmpty(data["LogoFileName"].ToString()) &&
                !string.IsNullOrEmpty(data["Name"].ToString()))
                folderName = data["DocumentFolderName"].ToString();

            var directory = new AppSettingsReader().GetValue("DOC", typeof(string)).ToString() + folderName + "/";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string path = System.IO.Path.Combine((Server.MapPath(directory)));

            var fileId = "";
            var fileName = "";
            var filedata = _documentActivityService.GetDocFile(documentActivity.Id);
            if (filedata.Count > 0)
            {
                if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                    !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                    fileId = filedata["FileId"].ToString();
                fileName = filedata["FileName"].ToString();

                if (fileName != documentActivity.FileName)
                    if (System.IO.File.Exists(path + fileId + System.IO.Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + System.IO.Path.GetExtension(fileName));
            }
		 var docPk=	_activityService.GetPk(documentActivity);

			if (file.IsNotNull())
            {
                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + System.IO.Path.GetExtension(item.FileName));
                        item.SaveAs(path + docPk + System.IO.Path.GetExtension(item.FileName));
                    }
                }
            }

			if (!string.IsNullOrEmpty(documentActivity.FileName))
			{
				if (!System.IO.File.Exists(path + docPk + System.IO.Path.GetExtension(documentActivity.FileName)))
					throw new CustomException("File didn't saved."); 
			}

			_activityService.InsertOrUpdateDocument(documentActivity, docPk);

			return Json(new { DocumentActivity = documentActivity, Message = "Data Saved Successfully" });
        }

		public JsonResult DetachDocument(FormCollection form, HttpPostedFileBase[] file)
		{
			DocumentActivity documentActivity = new JavaScriptSerializer().Deserialize<DocumentActivity>(form["documentActivityNew"]);
			var data = _employeeService.GetEmployee(documentActivity.EmployeeId);
			var folderName = "";

			if (!string.IsNullOrEmpty(data["DocumentFolderName"].ToString()) &&
				!string.IsNullOrEmpty(data["LogoFileName"].ToString()) &&
				!string.IsNullOrEmpty(data["Name"].ToString()))
				folderName = data["DocumentFolderName"].ToString();

			var directory = new AppSettingsReader().GetValue("DOC", typeof(string)).ToString() + folderName + "/";
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
			string path = System.IO.Path.Combine((Server.MapPath(directory)));

			var fileId = "";
			var fileName = "";
			var filedata = _documentActivityService.GetDocFile(documentActivity.Id);
			if (filedata.Count > 0)
			{
				if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
					!string.IsNullOrEmpty(filedata["FileName"].ToString()))
					fileId = filedata["FileId"].ToString();
				fileName = filedata["FileName"].ToString();

				if (fileName != documentActivity.FileName)
					if (System.IO.File.Exists(path + fileId + System.IO.Path.GetExtension(fileName)))
						System.IO.File.Delete(path + fileId + System.IO.Path.GetExtension(fileName));
			}
			var docPk = _activityService.GetPk(documentActivity);

			if (file.IsNotNull())
			{
				foreach (var item in file)
				{
					if (item != null)
					{
						if (System.IO.File.Exists(path + item.FileName))
							System.IO.File.Delete(path + fileId + System.IO.Path.GetExtension(item.FileName));
						item.SaveAs(path + documentActivity.Id + System.IO.Path.GetExtension(item.FileName));
					}
				}
			}
			_activityService.InsertOrUpdateDocument(documentActivity, docPk);
			return Json(new { DocumentActivity = documentActivity, Message = "File Detached Successfully" });
		}


        public ActionResult GetDocumentActivityList(string activityId)
        {
            return Json(_documentActivityService.GetDocumentList(activityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteDocument(string id)
        {
            var dbdata = _documentActivityService.Find(id);
            if (dbdata == null || string.IsNullOrEmpty(dbdata.Id))
                throw new CustomException("The record no longer exists.");

            if (!string.IsNullOrEmpty(id))
            {
                var data = _employeeService.GetDocumentFolder(id);
                var folderName = "";

                if (!string.IsNullOrEmpty(data["DocumentFolderName"].ToString()))
                    folderName = data["DocumentFolderName"].ToString();

                var directory = new AppSettingsReader().GetValue("DOC", typeof(string)).ToString() + folderName + "/";
                string path = System.IO.Path.Combine((Server.MapPath(directory)));
                var fileId = "";
                var fileName = "";
                var filedata = _documentActivityService.GetDocFile(id);
                if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                    fileId = filedata["FileId"].ToString();
                fileName = filedata["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + System.IO.Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + System.IO.Path.GetExtension(fileName));
                _documentActivityService.Delete(id);
                return Json(new { Message = "Data Deleted Successfully" });
            }
            else
                throw new CustomException("Id not Found");
        }
        #endregion

        #region DropDown
        [HttpGet, AllowAnonymous]
        public JsonResult GetActivityCategoryCboList()
        {
            return Json(_employeeService.GetActivityCategoryCboList().Rows, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, AllowAnonymous]
        public JsonResult GetActivityImportanceCboList()
        {
            return Json(_employeeService.GetActivityImportanceCboList().Rows, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, AllowAnonymous]
        public JsonResult GetPeriodCboList()
        {
            return Json(_employeeService.GetPeriodCboList().Rows, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, AllowAnonymous]
        public JsonResult GetDocumentFormateCboList()
        {
            return Json(_employeeService.GetDocumentFormateCboList().Rows, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, AllowAnonymous]
        public JsonResult GetDataSourceCategoryCboList()
        {
            return Json(_employeeService.GetDataSourceCategoryCboList().Rows, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region KPI
        [HttpPost]
        public JsonResult SaveKPI(KPI kpi)
        {
            _activityService.InsertOrUpdateKPI(kpi);
            return Json(new { KPI = kpi, Message = "Data Saved Successfully" });
        }

        public ActionResult GetKpiList(string activityId)
        {
            return Json(_kpiService.GetKPIList(activityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteKPI(string id)
        {
            var dbdata = _kpiService.Find(id);
            if (dbdata == null || string.IsNullOrEmpty(dbdata.Id))
                throw new CustomException("The record no longer exists.");

            if (!string.IsNullOrEmpty(id))
            {
                _kpiService.Delete(id);
                return Json(new { Message = "Data Deleted Successfully" });
            }
            else
                throw new CustomException("Id not Found");
        }
        #endregion
    }
}