using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.HumanResource.Employee;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class IndividualGratuityPolicyController : BaseController
    {
        
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public IndividualGratuityPolicyController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
                
        public ActionResult Aplos()
        {
            return View();
        }


        #region Get
        [HttpGet, Authorize]
        public ActionResult GetGratuityIns()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IndividualGratuityPolicy ep = new IndividualGratuityPolicy();
                return Json(ep.GetGratuityIns(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IndividualGratuityPolicy ep = new IndividualGratuityPolicy();
            JsonResult json = Json(ep.GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost, Authorize]
        public ActionResult GetList(string EmpSytemIDList)
        {
            try
            {
                //string EmpList = "'" + EmpSytemIDList.Replace(",", "','") + "'";//replaced with ""
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IndividualGratuityPolicy ep = new IndividualGratuityPolicy();
                return Json(ep.GetList(EmpSytemIDList,identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetGPDetails()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IndividualGratuityPolicy ep = new IndividualGratuityPolicy();
                return Json(ep.GetGPDetails(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion
        [HttpPost]
        public JsonResult Create(List<EmpList> EmpList)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //EmpList.AddedBy = identity.Name;
                IndividualGratuityPolicy ep = new IndividualGratuityPolicy();
                ep.SaveMaster(EmpList);
                return Json(new { Error = false, Data = EmpList, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[IndividualGratuityPolicy] where id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}