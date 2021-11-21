#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using System;
using System.Data;
using OTSBD;
using clsAttendance;
using System.Collections.Generic;
using Library.HumanResource.Payroll.Setting;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class BonusPolicyMonthlyRetainController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        public BonusPolicyMonthlyRetainController(
              IStoppageService stoppageService,
              ISqlRepository sqlRepository
            )
        {
            _stoppageService = stoppageService;
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
        [HttpPost, Authorize]
        public ActionResult GetMaster(string PlantID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select m.*,p.CompanyId
                        from BonusPolicyMonthlyRetainMaster m
                        left join ORG.Plant p on p.Id = m.PlantID
                        where m.PlantID = '" + PlantID + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetMonths(string MasterID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select *  from BonusPolicyMonthlyRetainMonthNo
                            where BnsPlcMthRetainMstID ='" + MasterID + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetDetails(string BnsPlcMthRetainID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select bpmrd.ID, bpmrd.BnsPlcMthRetainID, bpmrd.FormulaDesEarning, bpmrd.FormulaDesIDEarning, bpmrd.SalaryHeadIDEarning,
                        bpmrd.EarningValueRangeFrom, bpmrd.EarningValueRangeTo, bpmrd.IsMandatory, bpmrd.IsFixed, bpmrd.FixedValue, bpmrd.IsFormula,
                        bpmrd.IsDependOnEarning, bpmrd.IsMinWages, bpmrd.CompMinWagesAndOrginal, bpmrd.FormulaDes as FormulaDescription, 
                        bpmrd.FormulaDesID as FormulaIDDescription, bpmrd.SalaryHeadID as SalaryHeadIdFormula, bpmrd.GroupID,bpmrd.PlantID, 
                        bpmrd.AddedBy, bpmrd.AddedDate, bpmrd.AddedFromIP, bpmrd.UpdatedBy, bpmrd.UpdatedDate,bpmrd.UpdatedFromIP
                        from BonusPolicyMonthlyRetainDetails bpmrd
                            where BnsPlcMthRetainID = '" + BnsPlcMthRetainID + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetDistribution(string detailsID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from BonusPolicyMonthlyRetainDistribution 
                            where BonusPolicyDetailsID = '" + detailsID + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BnsPlcMthRetain master, List<BnsPlcMthRetainMthNo> months,List<BonusPolicyMonthlyRetainMasterSalaryHead> HeadList)
        {
            string _id = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                master.GroupID = identity.CompanyGroupId;
                master.AddedBy = identity.Name;
                master.AddedFromIP = identity.IPAddress;
                clsBonusPolicyMonthlyRetain cr = new clsBonusPolicyMonthlyRetain();
                cr.Save(master, months, HeadList);
                return Json(new { Error = false, Data = master.ID, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateDetails(BnsPlcMthRetainDetail details)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                details.GroupID = identity.CompanyGroupId;
                details.AddedBy = identity.Name;
                details.AddedFromIP = identity.IPAddress;
                clsBonusPolicyMonthlyRetain cr = new clsBonusPolicyMonthlyRetain();
                cr.SaveDetails(details);
                return Json(new { Error = false, data = details, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateDistribution(BnsPlcMthRetainDistribution distribution)
        {
            string _id = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsBonusPolicyMonthlyRetain cr = new clsBonusPolicyMonthlyRetain();
                cr.SaveDistribution(distribution);
                return Json(new { Error = false, Data = _id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteMaster(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsBonusPolicyMonthlyRetain cr = new clsBonusPolicyMonthlyRetain();
                cr.DeleteMaster(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult DeleteMonth(string ID, string monthno)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsBonusPolicyMonthlyRetain cr = new clsBonusPolicyMonthlyRetain();
                cr.DeleteMonth(ID, monthno);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult DeleteDetails(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsBonusPolicyMonthlyRetain cr = new clsBonusPolicyMonthlyRetain();
                cr.DeleteDetails(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult DeleteDistribution(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsBonusPolicyMonthlyRetain cr = new clsBonusPolicyMonthlyRetain();
                cr.DeleteDistribution(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetHeads(string masterID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select p.*,s.SalaryHead SalaryHeadName from BonusPolicyMonthlyRetainMasterSalaryHead p
                                left join SalaryHead s on s.SalaryHeadID=p.SalaryHeadID
                            where BonusPolicyMonthlyRetainMasterId ='" + masterID + "' Order By Sequence";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult DeleteHeadMaster(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from BonusPolicyMonthlyRetainMasterSalaryHead where Id='" + ID + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}