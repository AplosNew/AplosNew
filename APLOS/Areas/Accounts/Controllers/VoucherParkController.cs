using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class VoucherParkController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public VoucherParkController(
             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/VoucherPark/Aplos.cshtml");
        }


        [HttpPost, Authorize]
        public ActionResult GetVoucherDataList(string voucherNo)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = accountsCommonService.getVoucherDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherNo), Error = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ParkModeVoucher(string voucherId,string sourceType)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var rdBuilder = new System.Text.StringBuilder();
                if(sourceType== SourceType.JournalVoucher.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                }
                if (sourceType == SourceType.BankJournal.ToString() || sourceType == SourceType.CashJournal.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].BankJournal SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == SourceType.VendorInvoice.ToString()|| sourceType == SourceType.InventoryPayable.ToString()||sourceType == SourceType.CustomerInvoice.ToString() )
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var InvoiceSql = @"UPDATE [TRN].Invoice SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(InvoiceSql);
                }
                if (sourceType == SourceType.VendorPayment.ToString() || sourceType == SourceType.CustomerReceipt.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].InvoiceWriteOff SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == SourceType.CustomerAdvance.ToString() || sourceType == SourceType.VendorAdvance.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].Advance SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == SourceType.VendorAdvanceWriteOff.ToString() || sourceType == SourceType.CustomerAdvanceWriteOff.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].AdvanceWriteOff SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == SourceType.Loan.ToString()|| sourceType == SourceType.Investment.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].Financing SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == "AdditionalLoanPayable")
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].FinancingSubsequentTransaction SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == SourceType.LoanPayment.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].FinancingWriteOff SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == SourceType.DebitNote.ToString() || sourceType == SourceType.CreditNote.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].AdjustmentNote SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == SourceType.EmployeePayable.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].EmployeePayable SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == SourceType.EmployeePayment.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].EmployeePayableWriteOff SET RowState='Parked' WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == SourceType.SalaryPayable.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                }
                if (sourceType == SourceType.SalaryDisbursement.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                }
                if (sourceType == SourceType.FixedAssetCapitalizeJournal.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                }
                if (sourceType == SourceType.ServicePayable.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                }
                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return Json(new { Message = " Successfully Parked" });
            }
            catch (CustomException)
            {
                throw;
            }

            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }










    }
}