using System;
using System.Web.Mvc;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using static Library.Service.Payrolls.Setting.clsCurrencyRule;

using Library.HumanResource.Payroll.Tax;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class DeductionInvestmentMasterController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        DeductionInvestmentMasterService di = new DeductionInvestmentMasterService();
        public DeductionInvestmentMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion


        //string TableName = "dbo.TaxInvestmentMaster";
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string MasterId)
        {
            return Json(GetSequence(MasterId), JsonRequestBehavior.AllowGet);
        }
        private double GetSequence(string MasterId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM IncomeTaxItemChild where IncomeTaxItemMasterId='"+ MasterId + "'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
            return 1;
        }
        [Authorize, HttpGet]
        public ActionResult GetList(string Company)
        {
            return Json(di.GetList(Company), JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet , Authorize]
        public ActionResult getTaxSavingGroup()
        {
            return Json(di.getTaxSavingGroup(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetTaxYear()
        {
            try
            {
                return Json(di.GetTaxYear(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult getTaxSavingItem()
        {
            return Json(di.getTaxSavingItem(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getChildList(string id)
        {
            return Json(di.getChildList(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            if (data["TaxTypeId"] == null)
            {
                throw new Exception("Select Tax Type");
            }
            if (data["TaxYearId"] == null)
            {
                throw new Exception("Select Tax Year");
            }
            if (data["TaxSavingGroupId"] == null)
            {
                throw new Exception("Select Tax Year");
            }
            var jj = di.Create(data);
            
                return Json(new { Error = false, Data = jj, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult CreateChild(Dictionary<string, object> dataChild , string maxLimit )
        {
            if (Convert.ToBoolean(dataChild["IsDeduction"]) == false && Convert.ToBoolean(dataChild["IsInvestment"]) == false && Convert.ToBoolean(dataChild["IsEarning"]) == false )
            {
                return Json(new { Error = true, Data = dataChild, Message = "Select at least one field from [Deduction], [Earning], [Investment]" });
            }
            if(dataChild["IncomeTaxItemMasterId"] == null)
            {
                return Json(new { Error = true, Data = dataChild, Message = "Please First Save The Master" });
            }
            if((dataChild["isTaxableIncome"] == null & dataChild["isTax"]== null ) || (dataChild["isTaxableIncome"].ToString() == false.ToString() & dataChild["isTax"].ToString() == false.ToString()) || (dataChild["isTaxableIncome"].ToString() == "True" & dataChild["isTax"].ToString() =="True"))
            {
                return Json(new { Error = true, Data = dataChild, Message = "Please Select Taxable Income or Tax" });
            }
            if (dataChild["TaxSavingItemId"] == null)
            {
                return Json(new { Error = true, Data = dataChild, Message = "Please Select Tax Saving Item" });
            }
            if (Convert.ToBoolean(dataChild["isPercentage"]) == true && dataChild["SalaryHeadId"] == null)
            {
                return Json(new { Error = true, Data = dataChild, Message = "Please Select Salary Head" });
            }
            string jj = di.CreateChild(dataChild, maxLimit );
            if (jj == "Success")
            {
                return Json(new { Error = false, Data = dataChild, Sequence = GetSequence(dataChild["IncomeTaxItemMasterId"].ToString()), Message = AplosMessage.Updated });
            }
            else
            {
                return Json(new { Error = true, Data = dataChild, Message = jj });
            }

        }
        [HttpPost]
        public ActionResult Delete(string id)
        {
            string jj = di.Delete(id);
            if (jj == "Success")
            {
                return Json(new { Error = false, Data = id, Message = AplosMessage.Updated });
            }
            else
            {
                return Json(new { Error = true, Data = id, Message = jj });
            }
        }

        [HttpPost]
        public ActionResult DeleteChild(string id)
        {
            string jj = di.DeleteChild(id);
            if (jj == "Success")
            {
                return Json(new { Error = false, Data = id, Message = AplosMessage.Updated });
            }
            else
            {
                return Json(new { Error = true, Data = id, Message = jj });
            }
        }
        
    }
}