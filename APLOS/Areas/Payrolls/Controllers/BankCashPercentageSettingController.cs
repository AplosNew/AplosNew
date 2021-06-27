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

namespace Aplos.Areas.Payrolls.Controllers
{

    public class BankCashPercentageSettingController : BaseController
    {
        string TableName = "dbo.CurrencyRuleMaster";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public string PlantId { get; private set; }

        public BankCashPercentageSettingController(ISqlRepository R)
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
        public ActionResult GetList(string PlantId)
        {
            try
            {
                var _bank = _sqlRepository.GetDataCollection("select bcp.Id, bcp.FormulaDes as FormulaDescription, bcp.FormulaDesID as FormulaIDDescription from BankCashPercentageSettinng bcp where HeadLabel='Bank' and PlantId ='" + PlantId + "' ");
                var _cash = _sqlRepository.GetDataCollection("select bcp.Id, bcp.FormulaDes as CashFormulaDescription, bcp.FormulaDesID as FormulaIDDescription from BankCashPercentageSettinng bcp where HeadLabel='Cash' and PlantId ='" + PlantId + "' ");
                return Json(new { bank = _bank, cash = _cash }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public JsonResult Create(BankCashPercentage bp, BankCashPercentage cp)
        {
            string _bpid = string.Empty;
            string _cpid = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsBankCashPercentageSettinng cr = new clsBankCashPercentageSettinng();
                cr.Save(bp, cp,identity, out _bpid,out _cpid);
                return Json(new { Error = false, bpid = _bpid, cpid = _cpid, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public JsonResult GetCurrencyCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsCurrencyRule currencyRule = new clsCurrencyRule();
            return Json(currencyRule.GetCurrency(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
    }
}