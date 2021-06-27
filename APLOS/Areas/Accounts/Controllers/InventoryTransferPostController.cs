using Aplos.Controllers;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Mvc;
using Library.Accounting.Accounts;
using Library.Data.Sql;
using Library.Core;
using Library.Model.Enums;

namespace Aplos.Areas.Accounts.Controllers
{
    public class InventoryTransferPostController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;

        public InventoryTransferPostController( ISqlRepository sqlRepository )
        {
            _sqlRepository = sqlRepository;
        }


        #region Inventory Transfer Posting
        public ActionResult InventoryTransferJournal()
        {
            return View("~/Areas/Accounts/Views/InventoryPayable/InventoryTransferJournal.cshtml");

        }

        [Authorize, HttpGet]
        public JsonResult GetListForTransferJournal()
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetGRNListForTransferJournal(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetPostedInventoryTransferList(string column, string value)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetPostedInventoryTransferList(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFromPlantInventoryTransferPayable(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            
            return Json(_accountsInventoryPayableService.GetFromPlantInventoryTransferPayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
           
        }

        [Authorize, HttpGet]
        public JsonResult GetToPlantInventoryTransferPayable(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            return Json(_accountsInventoryPayableService.GetToPlantInventoryTransferPayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetTransferVendorPayableGLBudgetActivity(string inveReveiveId,string partyId)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetTransferVendorPayableGLBudgetActivity(inveReveiveId, identity.CompanyId, identity.PlantId, partyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialPayableList(GridParameter parameters, string inveReveiveId)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetPayableMaterial(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetInvetoryTransferVoucher(ReportFormat reportFormat,string plantId,string plantName, string voucherId)
        {
            AccountsInvoiceReportService _accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountsInvoiceReportService.GetToPlantInvetoryTransferVoucher(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, plantId, plantName, voucherId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }
        #endregion
    }
}