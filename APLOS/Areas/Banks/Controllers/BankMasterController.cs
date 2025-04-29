using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Addresses;
using Library.Model.Banks;
using Library.Service.Banks;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class BankMasterController : BaseController
    {
        private readonly IBankMasterService _bankMasterService;
        private readonly ISqlRepository _sqlRepository;
        public BankMasterController(IBankMasterService bankMasterService, ISqlRepository sqlRepository)
        {
            _bankMasterService = bankMasterService;
            _sqlRepository = sqlRepository;
        }

        [HttpGet]
        public ActionResult BankMaster()
        {
            return View("~/Areas/Banks/Views/BankMaster.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetBankMasterGL(string bankMasterId)
        {
            return Json(_bankMasterService.GetBankMasterGL(bankMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankMasterByMasterId(string bankMasterId)
        {
            return Json(_bankMasterService.GetBankMasterById(bankMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_bankMasterService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankMasterHouseBankCboList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankMasterService.GetBankMasterCboList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, BankACType.HouseBank), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetBankMasterLoanBankCboList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankMasterService.GetBankMasterCboList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, BankACType.Loan), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankMasterCboListByEntity(string entityId)
        {
            AccountsBankService _accountsBankService = new AccountsBankService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsBankService.GetBankMasterCboListByEntity(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, entityId, BankACType.HouseBank), JsonRequestBehavior.AllowGet);
            //Need to delete
            //   return Json(_bankMasterService.GetBankMasterCboListByEntity(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, entityId, BankACType.HouseBank), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankMasterCboListByPlant()
        {
            AccountsBankService _accountsBankService = new AccountsBankService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsBankService.GetBankMasterCboListByPlant(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, BankACType.HouseBank), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetNegotiatingBankMasterCboListByPlant()
        {
            AccountsBankService _accountsBankService = new AccountsBankService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsBankService.GetNegotiatingBankMasterCboListByPlant(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPartyBankCboListByParty(string partyId)
        {
            AccountsBankService _accountsBankService = new AccountsBankService(_sqlRepository);
            
            return Json(_accountsBankService.GetPartyBankCboListByParty(partyId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetInvestmentBankMasterCbo(string entityId)
        {
            AccountsBankService _accountsBankService = new AccountsBankService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsBankService.GetInvestmentBankMasterCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankMasterVoucher(GridParameter parameters, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankMasterService.GetBankMasterList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, entityId, BankACType.HouseBank), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetHouseBankBankMasterList(GridParameter parameters, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankMasterService.GetBankMasterList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, entityId, BankACType.HouseBank), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankMasterList(GridParameter parameters, BankACType bankACType, string entityId)
        {
            AccountsBankService _accountsBankService = new AccountsBankService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsBankService.GetBankMasterList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, entityId, bankACType), JsonRequestBehavior.AllowGet);
            //Need To delete
            //return Json(_bankMasterService.GetBankMasterList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, entityId, bankACType), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAllBankMasterLists(GridParameter parameters)
        {
            AccountsBankService _accountsBankService = new AccountsBankService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsBankService.GetAllBankMasterLists(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllBankMasterList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankMasterService.GetBankMasterList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankMasterParty(GridParameter parameters, string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankMasterService.GetBankMasterList(parameters, identity.CompanyGroupId, identity.CompanyId, id, BankACType.Party), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankMasterVoucherPayment(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankMasterService.GetBankMasterVoucherPayment(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankMasterByGL(string glGeneralInfoId)
        {
            return Json(_bankMasterService.GetBankMasterByGL(glGeneralInfoId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBankMasterQuery(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankMasterService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult Query(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankMasterService.GetCompanyBankList(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetBankMasterById(string id)
        {
            return Json(_bankMasterService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BankMaster bankMaster, IEnumerable<ContactMaster> contactMaster)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            bankMaster.CompanyGroupId = identity.CompanyGroupId;
            _bankMasterService.Insert(bankMaster, contactMaster);
            return Json(new { BankMaster = bankMaster, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BankMaster bankMaster, IEnumerable<ContactMaster> contactMaster)
        {
            _bankMasterService.Update(bankMaster, contactMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _bankMasterService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}