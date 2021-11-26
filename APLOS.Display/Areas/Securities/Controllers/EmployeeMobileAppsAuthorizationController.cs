#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Model.Employees;
using Library.Service.Employees;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.Securities.Controllers
{
    public class EmployeeMobileAppsAuthorizationController : BaseController
    {
        #region Constructor

        private readonly IEmployeeMobileAppsAuthorizationService _empMobileAccessService;

        public EmployeeMobileAppsAuthorizationController(IEmployeeMobileAppsAuthorizationService empMobileAccessService)
        {
            _empMobileAccessService = empMobileAccessService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        [Authorize]
        public ActionResult MobileApp()
        {
            return View();
        }


        [Authorize, HttpGet]
        public ActionResult GetList(string plantId, string unitId, string divisionId, string departmentId,
            string sectionId, string subSectionId, string designationGroupId, string designationId, string employeeId, string flag)
        {
            var data = _empMobileAccessService.Query(plantId, unitId, divisionId, departmentId, sectionId, subSectionId
                , designationGroupId, designationId, new JavaScriptSerializer().Deserialize<string[]>(employeeId), flag);
            return new JsonResult
            {
                ContentEncoding = Encoding.Default,
                ContentType = "application/json",
                Data = data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue
            };
            //return Json(_empMobileAccessService.Query(plantId, unitId, divisionId, departmentId, sectionId, subSectionId,
            //    designationGroupId, designationId, new JavaScriptSerializer().Deserialize<string[]>(employeeId), flag), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<EmployeeMobileAppsAuthorization> entities)
        {
            _empMobileAccessService.InsertUpdateOrDeleteGraph(entities);
            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpPost]
        public JsonResult ResetPin(EmployeeMobileAppsAuthorization entity)
        {
            _empMobileAccessService.Update(entity);
            return Json(new { Message = "Pin Changed Successfully" });
        }

        [HttpPost]
        public JsonResult ResetGuestPin(EmployeeMobileAppsAuthorization entity)
        {

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsRef;
            string strSql = @"select * from HKP.EmployeeMobileAppsAuthorization where Employeeid='" + entity.EmployeeId + "'";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

            if (dsRef.Tables[0].Rows.Count > 0)
            {

                dsRef.Tables[0].Rows[0].BeginEdit();

                dsRef.Tables[0].Rows[0]["PIN"] = entity.PIN;
                dsRef.Tables[0].Rows[0].EndEdit();
            }

            clsStaticInfo objStatic = new clsStaticInfo();
            objStatic.SaveDataSets(dsRef);
            return Json(new { Message = "Pin Changed Successfully" });
        }

        [HttpPost]
        public JsonResult UpdateEmployeePIN(List<Dictionary<string,string>> entities)
        {
            try
            {
                if (entities == null)
                    throw new Exception("No data found");

                ConnectionManager.DAL.ConManager objCon;
                DataSet dsRef;
                  string    strSql = @"select * from HKP.EmployeeMobileAppsAuthorization where employeeid='"+entities[0]["EmployeeId"] + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count > 0)
                {
                    
                    dsRef.Tables[0].Rows[0].BeginEdit();

                    dsRef.Tables[0].Rows[0]["PIN"] = entities[0]["PIN"];
                    dsRef.Tables[0].Rows[0].EndEdit();
                }

                clsStaticInfo objStatic = new clsStaticInfo();
                objStatic.SaveDataSets(dsRef);

                return Json(new { Message = "PIN Updated Successfully", Error = false });
            }
            catch (System.Exception ex)
            {

                return Json(new { Message = ex.Message,Error=true });
            }
          
        }

        [Authorize, HttpGet]
        public JsonResult GetAssignedEmployee(string plantId)
        {
            return Json(_empMobileAccessService.GetAssignedEmployee(plantId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetUnAssignedEmployee(string plantId)
        {
            return Json(_empMobileAccessService.GetUnAssignEmployee(plantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult Delete(string id)
        {
            _empMobileAccessService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


    }
}