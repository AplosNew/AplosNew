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

namespace Aplos.Areas.Payrolls.Controllers
{

    public class CurrencyRuleController : BaseController
    {
        string TableName = "dbo.CurrencyRuleMaster";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public CurrencyRuleController(ISqlRepository R)
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
            DataSet ds = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsCurrencyRule cr = new clsCurrencyRule();
                cr.GetCurrencyRuleMaster(PlantID, out ds);
                var xx = ds.Tables[0].ToList<CurrencyRuleMaster>();//tbd
                return Json(new { Error = false, data = xx});
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpPost]
        public ActionResult GetDetail(string SystemID)
        {
            string sql = @"select c.* ,s.SalaryHead,
							cu.Code AmtEntryCurrencyName, cur.Code AmtDefinitionCurrencyName,curr.Code AmtDisbusmentCurrencyName
							from  CurrencyRuleChild c
							left join SalaryHead s on s.SalaryHeadID = c.SalaryHeadID
							left join SCS.Currency cu on cu.Id=c.AmtEntryCurrency
							left join SCS.Currency cur on cur.Id=c.AmtDefinitionCurrency
							left join SCS.Currency curr on curr.Id=c.AmtDisbusmentCurrency
                            where c.MstSystemID =  '" + SystemID + "' ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(clsCurrencyRule.CurrencyRuleMaster data, List<clsCurrencyRule.CurrencyRulDetails> detail)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsCurrencyRule cr = new clsCurrencyRule();
                cr.SaveData(data, detail, identity);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost,Authorize]
        public ActionResult Delete(string SystemID)
        {
            try
            {
                if (string.IsNullOrEmpty(SystemID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from CurrencyRuleChild where SystemID='" + SystemID + "'");

                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteMaster(string SystemID)
        {
            try
            {
                if (string.IsNullOrEmpty(SystemID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from CurrencyRuleMaster where SystemID='" + SystemID + "'");

                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        
        public JsonResult GetCurrencyCbo(string plantId)
        {
            var identity= (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsCurrencyRule currencyRule = new clsCurrencyRule();
            return Json(currencyRule.GetCurrency(plantId), JsonRequestBehavior.AllowGet);
        }
    }
}