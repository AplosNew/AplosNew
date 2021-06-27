#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Biometrics;
using System.Collections.Generic;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Model.Attendances;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Web.Script.Serialization;
using System;
using clsAttendance;
using Library.Data.Sql;
using Library.HumanResource.OT;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class OTPolicyController : BaseController
    {
        string TableName = "dbo.OverTimePmtPolicyDetails";
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public OTPolicyController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion Constructor
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from " + TableName + " where Id = '" + Id + "' ");
                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetList(string PlantID)
        {
            try
            {
                var _sql = @"select otm.*,p.CompanyId from OverTimePmtPolicyMaster otm
                            left join ORG.Plant p on p.Id = otm.PlantID
                            where PlantID ='" + PlantID + "'  ";
            return Json(_sqlRepository.GetDataCollection(_sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetDetail(string masterId)
        {
            string sql = @"select otpd.ID, otpd.OverTimePmtPolicyID, otpd.OverTimeDayType, otpd.IsFixed,otpd.FixedValue,otpd.IsFormula,otpd.FormulaDes as FormulaDescription, otpd.FormulaDesID as FormulaIDDescription,otpd.IsDependOnEarning , otpd.SalaryHeadID as SalaryHeadIdFormula
                            from OverTimePmtPolicyDetails otpd where OverTimePmtPolicyID = '" + masterId + "' ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(OTPolicyMaster data, List<OTPolicyDetails> detail)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsOverTimePolicy ot = new clsOverTimePolicy();
                ot.SaveData(data, detail, identity);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult Delete(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OverTimePmtPolicyDetails where id='" + Id + "'");

                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteMaster(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OverTimePmtPolicyMaster where id='" + Id + "'");

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

