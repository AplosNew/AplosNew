using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Security.Core;
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
        
        [Authorize]
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

        [HttpGet, Authorize]
        public ActionResult getSelectedEmployeeName(string SelectedEmployeeId)
        {
            return Json(egs.getSelectedEmployeeName(SelectedEmployeeId), JsonRequestBehavior.AllowGet);
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
        public JsonResult Create(Dictionary<string, object> datas, string SelectedEmployeeId, string EGSetting, string PMSId)
        {
           
            try
            {
                return Json(new { Error = "No", Data = egs.Create(datas, SelectedEmployeeId, EGSetting, PMSId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Save Operations
        #region File Upload
        [HttpPost, Authorize]
        public ActionResult UploadAttachment(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\\", "");
                DataTable AdditionalData = CustomJsonResult.ToDataTable(UploadDefault_data);


                AdditionalData.Rows[0]["Id"] = AdditionalData.Rows[0]["Id"].ToString().Replace("\"", "");
                if (string.IsNullOrEmpty(AdditionalData.Rows[0]["Id"].ToString()))
                    throw new Exception("Save the item first");


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                foreach (var file in UploadDefault)
                {

                    string _Id = "IncomeTax_" + AdditionalData.Rows[0]["Id"].ToString();

                    var fileName = Path.GetFileName(_Id + new FileInfo(file.FileName).Extension);
                    var destinationPath = Path.Combine(ResourcesPathReader.EmployeeIncomeTax(), _Id + new FileInfo(file.FileName).Extension);

                    if (Directory.Exists(ResourcesPathReader.EmployeeIncomeTax()) == false)
                    {
                        try
                        {
                            Directory.CreateDirectory(ResourcesPathReader.EmployeeIncomeTax());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select* from " + AdditionalData.Rows[0]["TableName"] + " where Id='" + AdditionalData.Rows[0]["Id"].ToString() + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();


                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        #region Task data update
                        if (dsLocal.Tables[0].Rows[0]["FileName"].ToString() != "")
                        {
                            //try to delete the existing file
                            try
                            {
                                var _Path = Path.Combine(ResourcesPathReader.GetToDoPath(), dsLocal.Tables[0].Rows[0]["FileName"].ToString());
                                if (System.IO.File.Exists(_Path))
                                    System.IO.File.Delete(_Path);
                            }
                            catch (Exception)
                            {

                            }

                        }

                        DataRow dr = dsLocal.Tables[0].Rows[0];

                        dr.BeginEdit();

                        dr["FileName"] = fileName;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();


                        #endregion data update

                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);

                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }

        #endregion File Upload
        public ActionResult Delete(string id)
        {
            try
            {
                egs.Delete(id);

                return Json(new { Error = false,  Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        #region EMPLOYEE GOAL SETTING CHILD
        [HttpPost]
        public ActionResult GetEGChild(string SelectedEmployeeId, string PerformanceYearId)
        {
            try
            {
                return Json(egs.GetEGChild(SelectedEmployeeId, PerformanceYearId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteChild(string id)
        {
            try
            {
                egs.DeleteChild(id);

                return Json(new { Error = false, Message = Properties.AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        public void SaveFile()
        {
           
            try
            {
                string path = Server.MapPath("");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                foreach (string key in Request.Files)
                {
                    HttpPostedFileBase postedFile = Request.Files[key];
                    postedFile.SaveAs(path + postedFile.FileName);
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