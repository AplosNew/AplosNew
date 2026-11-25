#region Using

using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;
using Aplos.HumanResource;
using System;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeDocumentAssignmentController : BaseController
    {
        #region Constructor

        private readonly IEmployeeDocumentAssignmentService _employeeDocumentAssignmentService;

        public EmployeeDocumentAssignmentController(
              IEmployeeDocumentAssignmentService employeeDocumentAssignmentService
            )
        {
            _employeeDocumentAssignmentService = employeeDocumentAssignmentService;
        }
        EmployeeProfile employeeProfile = new EmployeeProfile();

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult DocStatus()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string assign, string plantId)
        {
            return Json(_employeeDocumentAssignmentService.GetEmployeeData(parameters, assign, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDocumentDataList(string empId)
        {
            return Json(_employeeDocumentAssignmentService.GetDocumentDataList(empId), JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult Create(IEnumerable<EmployeeInformation> employeeInformation)
        //{
        //	_employeeDocumentAssignmentService.InsertORUpdateMaster(employeeInformation);
        //	return Json(new { Message = AplosMessage.Insert });
        //}
        [HttpPost]
        public JsonResult Create(string employeeInformation)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<EmployeeInformation> employee = JsonConvert.DeserializeObject<List<EmployeeInformation>>(employeeInformation, settings);

            _employeeDocumentAssignmentService.InsertORUpdateMaster(employee);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion -- Operations

        #region DocStatus
        [HttpGet, Authorize]
        public ActionResult GetDOJFromToDate()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(employeeProfile.GetDOJFromToDate(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetEmpPendingDocumentDataList(string companyGroupId, string pId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(employeeProfile.GetEmpPendingDocumentDataList(identity.CompanyGroupId, pId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDocumentEmployeeData(List<string> EmpCategory, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(employeeProfile.GetDocumentEmployeeData(identity.CompanyId, EmpCategory, fromDate, toDate), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        public ActionResult DeleteUpdateMultiDocument(string id, List<Dictionary<string, object>> data, string EmpId)
        {
            DeleteUpdateMultiEmpDocument(id, data, EmpId);
            return Json(new { Message = AplosMessage.Success });
        }

        public void DeleteUpdateMultiEmpDocument(string id, List<Dictionary<string, object>> data, string EmpId)
        {
            string strSQL;
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.EmployeeDocument Where EmpSystemID='" + EmpId + "'", out dsMaster, false, "1");
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                if (id != null)
                {

                    strSQL = "DELETE FROM dbo.EmployeeDocument WHERE Id " + id + "";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenConnection("1");
                    objCon.BeginTransaction();
                    objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                    objCon.CommitTransaction();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr.EndEdit();
        }

        #endregion
    }
}