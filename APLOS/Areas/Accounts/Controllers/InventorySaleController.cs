using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using Library.Service.Invoices;
using Library.MaterialManagement.Reports;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.Data;
using System.Linq;
using Library.Data.Sql;
using Library.Accounting.Accounts;
using Library.Core;

namespace Aplos.Areas.Accounts.Controllers
{
    public class InventorySaleController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly AccountsSalesService _accountsSalesService;

        public InventorySaleController(
             ISqlRepository sqlRepository
            , AccountsSalesService accountsSalesService
            )
        {
            _sqlRepository = sqlRepository;
            _accountsSalesService = accountsSalesService;
        }

        

        #region Inventory Sales Posting
        
        public ActionResult InventoryReceivable()
        {
            return View();
        }

        [Authorize, HttpPost]
        public JsonResult GetListForInvReceivable(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsSalesService.GetSalesListForInvReveivable(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialReceivableList(GridParameter parameters, string inveReveiveId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsSalesService.GetReceivableMaterial(parameters, identity.CompanyId,identity.PlantId,inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetBudgetActivityInSalesMaterial(string inventorysalesId, string customerId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsSalesService.GetBudgetActivityInSalesMaterial(identity.CompanyId, identity.PlantId, inventorysalesId, customerId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventorySalesBudgetActivityInSalesMaterial(string inventorysalesId, string customerId,string taxapplicable)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_accountsSalesService.GetInventorySalesBudgetActivityInSalesMaterial(identity.CompanyId, identity.PlantId, inventorysalesId, customerId, taxapplicable), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetPostingInvReceivableList(string column, string value)
        {
            AccountsInventorySalesService _accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventorySalesService.GetPostingInvReceivableData(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetPostingInventorySalesList(string column, string value)
        {
            AccountsInventorySalesService _accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventorySalesService.GetPostingInventorySalesData(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialReceivable(string inveReveiveId, string employeeId, string partyId,string taxapplicable)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetInventoryMaterialReceivableData(identity.CompanyId, identity.PlantId, inveReveiveId, partyId, taxapplicable), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetInventorySaleDetailGLList(string inventorySalesId, string customerId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventorySalesService.GetInventorySaleDetailGLListData(identity.CompanyId, identity.PlantId, inventorySalesId, customerId), JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public ActionResult ReceivableJournal(ReportFormat reportFormat, string inventoryReceiveId, string employeeId, bool isReversCharge,bool isFoc)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            var reportFileName = "GRN";
            var workbook = accountsInventoryPayableReportService.PabyableJournal(identity.CompanyId, identity.PlantId, inventoryReceiveId, employeeId, isReversCharge, isFoc, reportFileName);
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



        [HttpPost]
        public ActionResult DeleteInventorySales(string salesId, string voucherId)
        {
            //if (tDSTaxVoucherId != null)
            //    throw new CustomException("TDS voucher no  " + tDSVoucherNo + "need to delete first!");

            //if (type == NewBeneficiaryType.Vendor.ToString())
            //    _invoiceService.DeleteInventoryPayable(grnId, invoiceId, voucherId);
            //if (type == NewBeneficiaryType.Employee.ToString())
            //    _employeePayableService.DeleteGRNBeneficiaryEmployee(grnId, invoiceId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}