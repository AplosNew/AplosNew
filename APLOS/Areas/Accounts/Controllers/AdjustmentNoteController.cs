using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Service.Advances;
using Library.Service.Invoices;
using Library.ViewModel.Banks;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class AdjustmentNoteController : BaseController
    {
        private readonly IAdjustmentNoteService _adjustmentNoteService;
        private readonly IAdjustmentNoteReportService _adjustmentNoteReportService;
        private readonly IInvoiceWriteOffService _invoiceWriteOffService;
        private readonly ISqlRepository _sqlRepository;

        public AdjustmentNoteController(
            IAdjustmentNoteService adjustmentNoteService
            , IAdjustmentNoteReportService adjustmentNoteReportService
            , IInvoiceWriteOffService invoiceWriteOffService
            , ISqlRepository sqlRepository)
        {
            _adjustmentNoteService = adjustmentNoteService;
            _adjustmentNoteReportService = adjustmentNoteReportService;
            _invoiceWriteOffService = invoiceWriteOffService;
            _sqlRepository = sqlRepository;
        }

        [HttpGet, Authorize]
        public ActionResult CreditNote()
        {
            return View("~/Areas/Accounts/Views/CreditNote.cshtml");
        }

        [HttpGet]
        public JsonResult GetCreditNoteList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_adjustmentNoteService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CreditNote), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertCreditNote(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList, IEnumerable<InvoiceTaxViewModel> tdsTaxList, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList, IEnumerable<VoucherDetailViewModel> voucherDetailInvoiceList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.CreditNote.ToString();
            if (voucherVM.IsInvoiceSetOff == true)
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _adjustmentNoteService.InsertCreditNote_InvoiceSetOff(voucherVM, voucherDetailVMList, invoiceTaxVMList, tdsTaxList, invoiceDetailChargesList, voucherDetailInvoiceList)) });
            }
            else
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _adjustmentNoteService.InsertCreditNote(voucherVM, voucherDetailVMList, invoiceTaxVMList, tdsTaxList, invoiceDetailChargesList)) });
            }
        }

        public JsonResult UpdateCreditNote(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.CreditNote.ToString();
            //_adjustmentNoteService.InsertCreditNote(voucherVM, voucherDetailVMList, invoiceTaxVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult PostCreditNote(string adjustmentNoteId, string entityId, string voucherId)
        {
            _adjustmentNoteService.Post(adjustmentNoteId, entityId, voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }

        #region GetCreditNoteReport


        //new format

        [HttpGet, Authorize]
        public ActionResult GetCreditNoteReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _adjustmentNoteReportService.GetCreditNoteReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CreditNote);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        #endregion GetCreditNoteReport

        [HttpGet, Authorize]
        public ActionResult DebitNote()
        {
            return View("~/Areas/Accounts/Views/DebitNote.cshtml");
        }

        [HttpGet]
        public JsonResult GetDebitNoteList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_adjustmentNoteService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.DebitNote), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertDebitNote(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList, IEnumerable<InvoiceTaxViewModel> tdsTaxList, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList, IEnumerable<VoucherDetailViewModel> voucherDetailInvoiceList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.DebitNote.ToString();
            if (voucherVM.IsInvoiceSetOff == true)
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _adjustmentNoteService.InsertDebitNote_InvoiceSetOff(voucherVM, voucherDetailVMList, invoiceTaxVMList, tdsTaxList, invoiceDetailChargesList, voucherDetailInvoiceList)) });
            }
            else
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _adjustmentNoteService.InsertDebitNote(voucherVM, voucherDetailVMList, invoiceTaxVMList, tdsTaxList, invoiceDetailChargesList)) });
            }
            
        }

        public JsonResult UpdateDebitNote(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.DebitNote.ToString();
            //_adjustmentNoteService.InsertCreditNote(voucherVM, voucherDetailVMList, invoiceTaxVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult PostDebitNote(string adjustmentNoteId, string entityId, string voucherId)
        {
            _adjustmentNoteService.Post(adjustmentNoteId, entityId, voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public JsonResult DeleteDebitNote(string adjustmentNoteId, string voucherId)
        {
            _adjustmentNoteService.DeleteAdjustmentNote(adjustmentNoteId, voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet, Authorize]
        public ActionResult GetDebitNoteReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _adjustmentNoteReportService.GetDebitNoteReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.DebitNote);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);
                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);
                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        #region Debit Note SetOff

        public ActionResult DebitNoteSetOff()
        {
            return View("~/Areas/Accounts/Views/DebitNoteSetOff.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetDebitNoteSetOffList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_invoiceWriteOffService.GetNoteSetOff(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.DebitNoteSetOff), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDebitNoteAvailableList(GridParameter parameters, string partyId, string partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_adjustmentNoteService.GetDebitNoteList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, partyType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertDebitNoteSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<VoucherDetailViewModel> voucherDetailInvoiceList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.DebitNoteSetOff.ToString();
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate <= 0)
                throw new CustomException("Please Input Rate.");
            if ((voucherVM.PaymentSource == "Bank") && (voucherVM.BankMasterId == null))
                throw new CustomException(Resources.SelectBank);
            if ((voucherVM.PaymentSource == "Cash") && (voucherVM.CashMasterId == null))
                throw new CustomException(Resources.SelectCash);
            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
                voucherVM.EntityId = advanceDetailVM.EntityId;
            }
            if (voucherVM.PaymentSource == "SetOff")
            {
                voucherVM.Amount = voucherDetailInvoiceList.Sum(r => r.Amount);
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.InsertDebitNoteInvoiceSetOff(voucherVM, voucherDetailVMList, voucherDetailInvoiceList)) });
            }
            else
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.InsertDebitNoteSetOff(voucherVM, voucherDetailVMList, bankChargeDetailVMList)) });

        }

        [HttpPost]
        public ActionResult UpdateDebitNoteSetOff()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PostDebitNoteSetOff(string invoiceWriteOffId)
        {
            _invoiceWriteOffService.Post(invoiceWriteOffId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet, Authorize]
        public ActionResult DebitNoteSetOffReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _adjustmentNoteReportService.DebitNoteSetOffReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.DebitNoteSetOff);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }




        #endregion

        #region Credit Note SetOff

       
        public ActionResult CreditNoteSetOff()
        {
            return View("~/Areas/Accounts/Views/CreditNoteSetOff.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetCreditNoteSetOffList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_invoiceWriteOffService.GetNoteSetOff(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CreditNoteSetOff), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetVendorAvailableInvoiceListForCreditNotes(GridParameter parameters, string partyId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetVendorAvailableInvoiceListForCreditNotes(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCreditNoteAvailableList(GridParameter parameters, string partyId, string partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_adjustmentNoteService.GetCreditNoteList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyId, partyType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertCreditNoteSetOff(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<VoucherDetailViewModel> voucherDetailInvoiceList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.CreditNoteSetOff.ToString();
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate <= 0)
                throw new CustomException("Please Input Rate.");
            if ((voucherVM.PaymentSource == "Bank") && (voucherVM.BankMasterId == null))
                throw new CustomException(Resources.SelectBank);
            if ((voucherVM.PaymentSource == "Cash") && (voucherVM.CashMasterId == null))
                throw new CustomException(Resources.SelectCash);
                
            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
                voucherVM.EntityId = advanceDetailVM.EntityId;
            }
            if (voucherVM.PaymentSource == "SetOff")
            {
                voucherVM.Amount = voucherDetailInvoiceList.Sum(r => r.Amount);
                if (voucherVM.PartyType == PartyType.Vendor.ToString())
                {
                    return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.InsertVendorCreditNoteSetOff(voucherVM, voucherDetailVMList, voucherDetailInvoiceList)) });
                }
                else
                {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.InsertCreditNoteInvoiceSetOff(voucherVM, voucherDetailVMList, voucherDetailInvoiceList)) });
                }
            }
            else
            {
                if (voucherVM.CurrencyId == voucherDetailVMList.FirstOrDefault().CurrencyId)
                {
                    return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.InsertCreditNoteSetOff(voucherVM, voucherDetailVMList, taxDetailVMList)) });
                }
                else
                {
                    if (voucherVM.BankAmount == 0 || voucherVM.BankAmount.ToString() == null)
                        throw new CustomException("Please Input Bank Amount.");
                    foreach (var voucherDetailVM in voucherDetailVMList)
                    {
                        if (voucherDetailVM.ConvertedAmount == 0 || voucherDetailVM.ConvertedAmount.ToString() == null)
                            throw new CustomException("Tr. Amount should more than 0");
                    }
                    return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.InsertCreditNoteSetOffDifferentCurrency(voucherVM, voucherDetailVMList, taxDetailVMList)) });
                }
            }

        }

        [HttpPost]
        public ActionResult UpdateCreditNoteSetOff()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PostCreditNoteSetOff(string invoiceWriteOffId)
        {
            _invoiceWriteOffService.Post(invoiceWriteOffId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public JsonResult DeleteDebitNoteSetOff(string invoiceWriteOffId, string voucherId)
        {
            _invoiceWriteOffService.DeleteAdjustmentNoteWriteOff(invoiceWriteOffId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #region CreditNoteSetOffReport

        [HttpGet, Authorize]
        public ActionResult CreditNoteSetOffReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _adjustmentNoteReportService.CreditNoteSetOffReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CreditNoteSetOff);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }
        #endregion CreditNoteSetOffReport

        #endregion

        #region DebitCreditNoteProcessControl

        [Authorize]
        public ActionResult DebitCreditNoteProcessControl()
        {
            return View("~/Areas/Accounts/Views/DebitCreditNoteProcessControl.cshtml");

        }

        //[HttpGet, Authorize]
        //public JsonResult GetCbo()
        //{
        //    return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        //}
        string TableName = "hkp.DebitCreditNoteProcessControl";
        [HttpPost, Authorize]
        public ActionResult GetDebitCreditNoteProcessControlList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT TOP 100 * FROM (SELECT DNCN.*,AD.UserName DrControl,AC.UserName CrControl
                           FROM [HKP].DebitCreditNoteProcessControl DNCN 
                           LEFT JOIN [MST].BudgetMasterActivity BMAD ON BMAD.Id=DNCN.DrControlId
                           LEFT JOIN [MST].BudgetMasterActivity BMAC ON BMAC.Id=DNCN.CrControlId
                           LEFT JOIN [HKP].Activity AD ON AD.Id=BMAD.ActivityId
                           LEFT JOIN [HKP].Activity AC ON AC.Id=BMAC.ActivityId
                ) AS TEMP WHERE " + strkey + " ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        

        [HttpPost]
        public JsonResult DebitCreditNoteProcessControlCreate(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("UserName already exists!!!");

                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where   Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";




                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(TableName), out _Id);

                    data["Id"] = "LC" + _Id;
                    _accountsCommonService.AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    _accountsCommonService.EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult DebitCreditNoteProcessControlDelete(string id)
        {
            string sql = @"select * from [HKP].[DebitCreditNoteProcessControl] where Id = '" + id + "'";
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false,  Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }


        #endregion
    }
}