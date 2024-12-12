using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Taxations;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Taxations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class TaxCodeController : BaseController
    {
        private readonly ITaxCodeService _taxCodeService;
        private readonly ITaxCodeDetailService _taxCodeDetailService;
        private readonly ITaxCodeGLService _taxCodeGLService;
        private readonly ITaxCodeYearService _taxCodeYearService;
        private readonly ISqlRepository _sqlRepository;

        public TaxCodeController(ITaxCodeService taxCodeService,
            ITaxCodeDetailService taxCodeDetailService,
            ITaxCodeGLService taxCodeGLService,
            ITaxCodeYearService taxCodeYearService
             , ISqlRepository sqlRepository)
        {
            _taxCodeService = taxCodeService;
            _taxCodeDetailService = taxCodeDetailService;
            _taxCodeGLService = taxCodeGLService;
            _taxCodeYearService = taxCodeYearService;
            _sqlRepository = sqlRepository;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/TaxCode.cshtml");
        }

        [Authorize]
        public ActionResult TaxCodeYear()
        {
            return View("~/Areas/Accounts/Views/TaxCodeYear.cshtml");
        }

        [Authorize]
        public ActionResult TaxCodeGL()
        {
            return View("~/Areas/Accounts/Views/TaxCodeGL.cshtml");
        }

        //[Authorize]
        //public JsonResult GetCboInput(DateTime postingDate)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(new SelectList(_taxCodeService.GetCboInput(postingDate, identity.CompanyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        //}

        [Authorize, HttpGet]
        public ActionResult GetCboInput(DateTime postingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_taxCodeService.GetCboInput(postingDate,identity.CompanyId, TaxCodeInputOutput.Input.ToString()), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetTaxCodeInputVATGST(DateTime postingDate)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetTaxCodeInputVATGST(postingDate, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult GetTaxCodeInvoiceTriggeringInstanceOthers()
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetTaxCodeInvoiceTriggeringInstanceOthers(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetTaxCodeOutputVATGST(DateTime postingDate)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetTaxCodeOutputVATGST(postingDate, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetAdditionalTaxCbo(DateTime postingDate)
        {
            
           AccountsGLService _accountsGLService =new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetAdditionalTaxCbo(postingDate, identity.CompanyId), JsonRequestBehavior.AllowGet);
            //return Json(new SelectList(_taxCodeService.GetWithholdCboInput(postingDate, identity.CompanyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetAdditionalTaxOutputCbo(DateTime postingDate)
        {

            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetAdditionalTaxOutputCbo(postingDate, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Authorize]
        public JsonResult GetTDSCbo(DateTime postingDate)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetTDSCbo(postingDate, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetTDSCboByServiceMasterId(DateTime postingDate, string serviceMasterIds)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetTDSCboByServiceMasterId(postingDate, identity.CompanyId, serviceMasterIds), JsonRequestBehavior.AllowGet);
        }
        [HttpGet,Authorize]
        public JsonResult GetTDSOutPutCbo(DateTime postingDate)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsGLService.GetTDSOutPutCbo(postingDate, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetWithholdInputTaxCodeCbo(DateTime postingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT DISTINCT TC.Id, TC.UserName AS Text,TC.TaxCategoryId,TCGL.WithholdCreditableGLId GLGeneralInfoId,GLGI.AccountCode GLGeneralInfoCode,GLGI.UserName GLGeneralInfoName
                        ,TCGL.WithholdCreditableBudgetMasterId BudgetMasterId,B.UserName BudgetName,A.UserName ActivityName,TCGL.WithholdCreditableActivityId ActivityId
                        FROM [MST].[TaxCodeYear] AS TCY
                        LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id = TCY.TaxCodeId
                        LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TC.Id = TCGL.TaxCodeId
					    LEFT JOIN [SCS].[TaxYear] AS TY ON TY.Id=TCY.TaxYearId
						LEFT JOIN [SCS].[TaxYearPeriod] AS TYP ON TYP.TaxYearId=TY.Id
                        LEFT JOIN [ORG].Company AS CO ON CO.COAId=TCGL.COAId
                        LEFT JOIN [HKP].GLGeneralInfo AS GLGI ON GLGI.Id=TCGL.WithholdCreditableGLId
                        LEFT JOIN [MST].BudgetMaster AS BM ON BM.Id=TCGL.WithholdCreditableBudgetMasterId
						LEFT JOIN [HKP].Budget B ON B.Id=BM.BudgetId
						LEFT JOIN [HKP].Activity A ON A.Id=TCGL.WithholdCreditableActivityId
                        WHERE TC.InputOrOutput='" + TaxCodeInputOutput.Input + @"'
						AND TYP.StartDate <='" + postingDate.ToDbDate() + "' AND TYP.EndDate >='" + postingDate.ToDbDate() + "' AND CO.Id='" + identity.CompanyId + @"' 
						AND TC.IsWithhold=1";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetWithholdOutputTaxCodeCbo(DateTime postingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT DISTINCT TC.Id, TC.UserName AS Text,TC.TaxCategoryId,TCGL.WithholdCreditableGLId GLGeneralInfoId,GLGI.AccountCode GLGeneralInfoCode,GLGI.UserName GLGeneralInfoName
                        ,TCGL.WithholdCreditableBudgetMasterId BudgetMasterId,B.UserName BudgetName,A.UserName ActivityName,TCGL.WithholdCreditableActivityId ActivityId
                        FROM [MST].[TaxCodeYear] AS TCY
                        LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id = TCY.TaxCodeId
                        LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TC.Id = TCGL.TaxCodeId
					    LEFT JOIN [SCS].[TaxYear] AS TY ON TY.Id=TCY.TaxYearId
						LEFT JOIN [SCS].[TaxYearPeriod] AS TYP ON TYP.TaxYearId=TY.Id
                        LEFT JOIN [ORG].Company AS CO ON CO.COAId=TCGL.COAId
                        LEFT JOIN [HKP].GLGeneralInfo AS GLGI ON GLGI.Id=TCGL.WithholdCreditableGLId
                        LEFT JOIN [MST].BudgetMaster AS BM ON BM.Id=TCGL.WithholdCreditableBudgetMasterId
						LEFT JOIN [HKP].Budget B ON B.Id=BM.BudgetId
						LEFT JOIN [HKP].Activity A ON A.Id=TCGL.WithholdCreditableActivityId
                        WHERE TC.InputOrOutput='" + TaxCodeInputOutput.Output + @"'
						AND TYP.StartDate <='" + postingDate.ToDbDate() + "' AND TYP.EndDate >='" + postingDate.ToDbDate() + "' AND CO.Id='" + identity.CompanyId + @"' 
						AND TC.IsWithhold=1";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetOutputTDSTaxCodeCbo(DateTime postingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT DISTINCT TC.Id, TC.UserName AS Text,TC.IsWithhold,TC.TaxCategoryId,TCGL.WithholdCreditableGLId GLGeneralInfoId,GLGI.AccountCode GLGeneralInfoCode,GLGI.UserName GLGeneralInfoName
                        ,TCGL.WithholdCreditableBudgetMasterId BudgetMasterId,B.UserName BudgetName,A.UserName ActivityName,TCGL.WithholdCreditableActivityId ActivityId
                        FROM [MST].[TaxCodeYear] AS TCY
                        LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id = TCY.TaxCodeId
                        LEFT JOIN [MST].[TaxCategory] AS TXC ON TXC.Id=TC.TaxCategoryId
                        LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TC.Id = TCGL.TaxCodeId
					    LEFT JOIN [SCS].[TaxYear] AS TY ON TY.Id=TCY.TaxYearId
						LEFT JOIN [SCS].[TaxYearPeriod] AS TYP ON TYP.TaxYearId=TY.Id
                        LEFT JOIN [ORG].Company AS CO ON CO.COAId=TCGL.COAId
                        LEFT JOIN [HKP].GLGeneralInfo AS GLGI ON GLGI.Id=TCGL.WithholdCreditableGLId
                        LEFT JOIN [MST].BudgetMaster AS BM ON BM.Id=TCGL.WithholdCreditableBudgetMasterId
						LEFT JOIN [HKP].Budget B ON B.Id=BM.BudgetId
						LEFT JOIN [HKP].Activity A ON A.Id=TCGL.WithholdCreditableActivityId
                        WHERE TC.InputOrOutput='" + TaxCodeInputOutput.Output + @"'
						AND TYP.StartDate <='" + postingDate.ToDbDate() + "' AND TYP.EndDate >='" + postingDate.ToDbDate() + "' AND CO.Id='" + identity.CompanyId + @"' 
						AND TXC.TaxCategoryType='TDS' AND TC.IsWithhold=1";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetOutputTDSCreditableTaxCodeCbo(DateTime postingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT DISTINCT TC.Id, TC.UserName AS Text,TC.IsWithhold,TC.TaxCategoryId,TCGL.CreditableGLId GLGeneralInfoId,GLGI.AccountCode GLGeneralInfoCode,GLGI.UserName GLGeneralInfoName
                        ,TCGL.CreditableGLBudgetMasterId BudgetMasterId,B.UserName BudgetName,A.UserName ActivityName,TCGL.CreditableGLActivityId ActivityId
						,TCY.[Type],TCD.ValueOfFixed
                        FROM [MST].[TaxCodeYear] AS TCY
                        LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id = TCY.TaxCodeId
                        LEFT JOIN [MST].[TaxCategory] AS TXC ON TXC.Id=TC.TaxCategoryId
						LEFT JOIN [MST].[TaxCodeDetail] TCD ON TCD.TaxCodeId=TC.Id AND TCD.TaxCodeYearId=TCY.Id
                        LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TC.Id = TCGL.TaxCodeId
					    LEFT JOIN [SCS].[TaxYear] AS TY ON TY.Id=TCY.TaxYearId
						LEFT JOIN [SCS].[TaxYearPeriod] AS TYP ON TYP.TaxYearId=TY.Id
                        LEFT JOIN [ORG].Company AS CO ON CO.COAId=TCGL.COAId
                        LEFT JOIN [HKP].GLGeneralInfo AS GLGI ON GLGI.Id=TCGL.CreditableGLId
                        LEFT JOIN [MST].BudgetMaster AS BM ON BM.Id=TCGL.CreditableGLBudgetMasterId
						LEFT JOIN [HKP].Budget B ON B.Id=BM.BudgetId
						LEFT JOIN [HKP].Activity A ON A.Id=TCGL.CreditableGLActivityId
                        WHERE TC.InputOrOutput='" + TaxCodeInputOutput.Output + @"'
						AND TYP.StartDate <='" + postingDate.ToDbDate() + "' AND TYP.EndDate >='" + postingDate.ToDbDate() + "' AND CO.Id='" + identity.CompanyId + @"' 
						AND TXC.TaxCategoryType='TDS' AND TC.IsCreditable=1";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        public JsonResult GetCboOutput(DateTime postingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_taxCodeService.GetCboOutput(postingDate, identity.CompanyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetTaxCodeList(GridParameter parameters, string countryId)
        {
            return Json(_taxCodeService.Query(parameters, countryId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetTaxCodeById(string id, string vendorinvoicetaxid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_taxCodeService.GetTaxCodeById(identity.CompanyId, id, vendorinvoicetaxid), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetTaxCodewithPersentageById(string id, string vendorinvoicetaxid, DateTime postingDate)
        {
            AccountsGLService _accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_accountsGLService.GetTaxCodeById(identity.CompanyId, id, vendorinvoicetaxid, postingDate), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult GetTaxCodeYearIdByTaxYearId(string taxyearid, string taxcodeid)
        {
            return Json(_taxCodeYearService.GetIdByTaxYearId(taxyearid, taxcodeid), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetList(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_taxCodeDetailService.GetList(id, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetTaxCodeCondition(GridParameter parameters, string taxCodeid)
        {
            return Json(_taxCodeDetailService.GetTaxCodeCondition(parameters, taxCodeid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTaxCode(string id)
        {
            return Json(_taxCodeService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(TaxCode taxcode)
        {
            _taxCodeService.Insert(taxcode);
            return Json(new { TaxCode = taxcode, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(TaxCode taxcode)
        {
            _taxCodeService.Update(taxcode);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _taxCodeService.Archive(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [HttpGet, Authorize]
        public JsonResult GetTaxCodeCbo(string countryId)
        {
            return Json(_taxCodeService.GetCbo(countryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxCodeYearList(GridParameter parameters, string taxCodeId, string countryId)
        {
            return Json(_taxCodeYearService.Query(parameters, taxCodeId, countryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult TaxCodeYearInsert(TaxCodeYear taxcodeyear, IEnumerable<TaxCodeDetail> taxCodeDetail, TaxCodeDetail taxCodeDerailFixedValue)
        {
            _taxCodeYearService.InsertUpdate(taxcodeyear, taxCodeDetail, taxCodeDerailFixedValue);
            return Json(new { TaxCodeYear = taxcodeyear, Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_taxCodeService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetTaxCodeByPKId(string taxCodeId)
        {
            return Json(_taxCodeService.GetTaxCodeByPKId(taxCodeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult TaxCodeListIncludeWithholdGl(GridParameter parameters)
        {
            return Json(_taxCodeService.TaxCodeListIncludeWithholdGl(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult TaxCodeListIncludeExpensesGl(GridParameter parameters)
        {
            return Json(_taxCodeService.TaxCodeListIncludeExpensesGl(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxCodeDetailList(GridParameter parameters, string id, string type, string taxCodeYearId)
        {
            return Json(_taxCodeDetailService.Query(parameters, id, type, taxCodeYearId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxCodeGLList(GridParameter parameters, string taxcodeid, string coaId)
        {
            return Json(_taxCodeGLService.GetTaxCodeGLList(parameters, taxcodeid, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult TaxCodeGLCreateUpdate(TaxCodeGL taxcodegl)
        {
            _taxCodeGLService.Insert(taxcodegl);
            return Json(new { TaxCodeGL = taxcodegl, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public ActionResult TaxCodeGLDelete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _taxCodeGLService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost, Authorize]
        public JsonResult GetTaxCodeDataPopUpList(string column, string value, string countryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetTaxCodeDataList(column, value, countryId), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetTaxCodeDataList(string column, string value, string countryId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"DECLARE @countryId VARCHAR(100)='" + countryId + @"';
                        SELECT TOP 400 * FROM (SELECT * FROM [MST].[TaxCode]  WHERE Active=1 and Archive=0 and CountryId=@countryId ) AS TEMP WHERE " + strkey + " order by UserName ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
    }
}