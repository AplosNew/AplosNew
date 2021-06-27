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
using Library.Service.Payrolls.Setting;
using static Library.Service.Payrolls.Setting.clsCurrencyRule;
using Library.HumanResource.Payroll.Setting;
using Library.HumanResource.Payroll.Tax;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class BonusProcessController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public BonusProcessController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetHead()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                BonusProcess ep = new BonusProcess();
                return Json(ep.GetHead("Festival Bonus"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetBonus()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                BonusProcess ep = new BonusProcess();
                return Json(ep.BonusPolicyMasterInfo(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCurrency()
        {
            try
            {
                DataSet dsCurrency = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                BonusProcess ep = new BonusProcess();
                ep.GetLocalCurrency(identity.PlantId, out dsCurrency);
                var _EmpList = new List<CurrencyModel>();
                if (dsCurrency.Tables[0].Rows.Count > 0)
                {
                    _EmpList = dsCurrency.Tables[0].ToList<CurrencyModel>();
                }
                //return Json(new { Error = false, data = _EmpList });
                return Json(new { Error = false, data = _EmpList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDetails(String MasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                BonusProcess ep = new BonusProcess();
                return Json(ep.GetMasterDetails(MasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetEmpBonus(String MasterId, string CutOffDate)
        {
            try
            {
                DataSet dsEmpLocal = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                BonusProcess ep = new BonusProcess();
                ep.GetEmp(MasterId, identity.PlantId, CutOffDate, out dsEmpLocal);
                var _EmpList = new List<BonusProcessLocal>();
                if (dsEmpLocal.Tables[0].Rows.Count > 0)
                {
                    _EmpList = dsEmpLocal.Tables[0].ToList<BonusProcessLocal>();
                }
                return Json(new { Error = false, data = _EmpList });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Create(List<BonusProcessLocal> process, string MasterID, BonusProcessModel Bonus)
        {
            string _id = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                BonusProcess p = new BonusProcess();
                p.SetBonusProcess(process, MasterID, Bonus);
                return Json(new { Error = false, Data = Bonus, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public JsonResult Report(string workDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                BonusProcess p = new BonusProcess();
                var workbook = p.Report(identity.PlantId, workDate);
                return Json(new { FileName = workbook, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}