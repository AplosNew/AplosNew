#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.Properties;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class ResignationController : BaseController
    {
        #region Constructor
        private readonly IResignationService _ResignationService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;

        private readonly IEmployeeLeaveSummaryService _employeeLeaveSummary;

        private readonly ISqlRepository _sqlRepository;

        public ResignationController(
              IResignationService ResignationService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IEmployeeLeaveSummaryService employeeLeaveSummary
            , ISqlRepository sqlRepository
            )
        {
            _ResignationService = ResignationService;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _employeeLeaveSummary = employeeLeaveSummary;
            _sqlRepository = sqlRepository;
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
        public ActionResult NewList(GridParameter parameters, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ResignationService.ActiveEmpListByPlantId(parameters, plantId, identity.IsControlAdmin, identity.IsSysAdmin, identity.CompanyGroupId, identity.CompanyId, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult PendingList(GridParameter parameters, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ResignationService.PendingResignationQueryByPlantId(parameters, plantId, identity.IsControlAdmin, identity.IsSysAdmin, identity.CompanyGroupId, identity.CompanyId, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEntityByEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "ResignationApply", identity.EmployeeId);
            return Json(entity, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetResignationHistoryById(string EmployeeId)
        {
            return Json(_ResignationService.ResignationHistoryByID(EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetExperience(string EmpId)
        {
            int tYear = 0;
            int tMonth = 0;
            _ResignationService.GetExperience(EmpId, out tYear, out tMonth);
            return Json(new { DurationY = tYear, DurationM = tMonth, JsonRequestBehavior.AllowGet });
        }


        [HttpPost]
        public JsonResult Create(FormCollection form, HttpPostedFileBase[] file)
        {

            var pre = form["Resignation"];
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var reg = JsonConvert.DeserializeObject<Resignation>(pre, settings);
            var directory = ResourcesPathReader.GetEmployeeResignationLetterPath(); //new AppSettingsReader().GetValue("RESIGNATION_LETTER", typeof(string)).ToString(); //get pic url from web config
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string path = Path.Combine(directory);
            string _id = "";
            var fileName = "";
            var filedata = _ResignationService.GetFile(reg.Id);
            if (file.IsNotNull())
            {
                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }
            }

            _ResignationService.Save(reg, out _id);

           
            if (filedata.Count > 0)
            {
                if (
                    !string.IsNullOrEmpty(filedata["AttachLetter"].ToString()))
                    fileName = filedata["AttachLetter"].ToString();

                if (fileName != reg.AttachLetter)
                    if (System.IO.File.Exists(path + _id + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + _id + Path.GetExtension(fileName));
            }

            if (file.IsNotNull())
            {
                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + _id + Path.GetExtension(item.FileName));
                        item.SaveAs(path + _id + Path.GetExtension(item.FileName));
                    }
                }
            }
            return Json(new { Resignation = reg, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(FormCollection form, HttpPostedFileBase[] file)
        {

            var pre = form["Resignation"];
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var reg = JsonConvert.DeserializeObject<Resignation>(pre, settings);
            var directory = ResourcesPathReader.GetEmployeeResignationLetterPath(); //new AppSettingsReader().GetValue("RESIGNATION_LETTER", typeof(string)).ToString(); //get pic url from web config
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string path = Path.Combine(directory);
            string _id = "";
            var fileName = "";
            var filedata = _ResignationService.GetFile(reg.Id);
            if (file.IsNotNull())
            {
                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }
            }

            _ResignationService.Update(reg);
            _id = reg.Id;

            if (filedata.Count > 0)
            {
                if (
                    !string.IsNullOrEmpty(filedata["AttachLetter"].ToString()))
                    fileName = filedata["AttachLetter"].ToString();

                if (fileName != reg.AttachLetter)
                    if (System.IO.File.Exists(path + _id + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + _id + Path.GetExtension(fileName));
            }

            if (file.IsNotNull())
            {
                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + _id + Path.GetExtension(item.FileName));
                        item.SaveAs(path + _id + Path.GetExtension(item.FileName));
                    }
                }
            }
            return Json(new { Resignation = reg, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult LeaveSummary(string CompanyGroupId)
        {
            _employeeLeaveSummary.Save(CompanyGroupId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public JsonResult GetResignationType()
        {
            try
            {
                var sql = "select Id Value, UserName Text from HKP.ResignationType order by UserName";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion
    }
}