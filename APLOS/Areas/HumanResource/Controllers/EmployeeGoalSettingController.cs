using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Properties;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Service.Helpers;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class EmployeeGoalSettingController : Controller
    {
        EmployeeGoalSetting egs = new EmployeeGoalSetting();

        private readonly ISqlRepository _sqlRepository;
        #region Constructor
        public EmployeeGoalSettingController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion Constructor

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page

        #region All get Operations
        [HttpPost]
        public ActionResult GetEGList()
        {
            try
            {
                return Json(egs.GetEGList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult getSelectedEmployee(string SelectedEmployeeId)
        {
            return Json(egs.getSelectedEmployee(SelectedEmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getPerformancePeriod()
        {
            return Json(egs.getPerformancePeriod(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getEmployee()
        {
            try
            {
                var jsondata = Json(egs.getEmployee(), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getPMSMaster(string SystemId)
        {
            try
            {
                var jsondata = Json(egs.getPMSMaster(SystemId), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion All get Operations

        #region Save Operations
        [HttpPost]
        public JsonResult CreateEGSParent(Dictionary<string, object> datas, string SelectedEmployeeId)
        {
           
            try
            {
                return Json(new { Error = "No", Data = egs.CreateEGSParent(datas, SelectedEmployeeId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Save Operations

        public ActionResult Delete(string id)
        {
            try
            {
                egs.Delete(id);

                return Json(new { Error = false,  Message = Properties.AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        #region EMPLOYEE GOAL SETTING CHILD
       
        public JsonResult CreateEGChild(Dictionary<string, object> data)
        {

            try
            {
                return Json(new { Error = "No", Data = egs.CreateEGChild(data), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public void SaveFile(out string path)
        {
            path = "";
            try
            {
                var file = Request.Files["file"];
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {
                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetOTManualFile(), file.FileName);
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion EMPLOYEE GOAL SETTING CHILD
    }
}