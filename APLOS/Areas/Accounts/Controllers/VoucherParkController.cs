using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Service.Extension.Accounts;
using Library.Service.Vouchers;
using Library.ViewModel.Vouchers;
using System;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class VoucherParkController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IVoucherService _voucherService;
        public VoucherParkController(
             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IVoucherService voucherService
            )
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _voucherService = voucherService;
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
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var voucher = _voucherService.FindVoucher(voucherId);
                var voucherVM = new VoucherViewModel
                {
                    CompanyId = voucher.CompanyId,
                    PostingDate = voucher.PostingDate
                };
               
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _accountsCommonService.CheckingFiscalYearClose(voucher);

                ConnectionManager.DAL.ConManager objCon2;
                DataSet dsMasterbankReconciled = null;
                string bankReconciledsql = @"SELECT BRM.*  from TRN.BankReconciliationMap AS BRM
                                            INNER JOIN TRN.VoucherDetail AS VD ON VD.Id=BRM.VoucherDetailId
                                            where VD.VoucherId = '" + voucherId + "' ";
                objCon2 = new ConnectionManager.DAL.ConManager("1");
                objCon2.OpenDataSetThroughAdapter(bankReconciledsql, out dsMasterbankReconciled, false, "1");

                if (dsMasterbankReconciled.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("Voucher Park Mode not allowed, Bank Reconciled have to delete first!");
                }


                if (sourceType== SourceType.JournalVoucher.ToString())
                {
                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    string setOffsql = @"SELECT V.VoucherNo	
								from	[TRN].[CapitalizationMasterDetail] CD
								INNER JOIN trn.VoucherDetail VD ON VD.Id=CD.VoucherDetailId
								INNER JOIN trn.CapitalizationMaster CM ON CM.Id=CD.CapitalizationMasterId
								INNER JOIN trn.Voucher V ON V.Id=CM.VoucherId
								WHERE VD.VoucherId= '" + voucherId + @"' ";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }

                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                }
                if (sourceType == SourceType.BankJournal.ToString() || sourceType == SourceType.CashJournal.ToString())
                {
                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    string setOffsql = @"SELECT V.VoucherNo	
								from	[TRN].[CapitalizationMasterDetail] CD
								INNER JOIN trn.VoucherDetail VD ON VD.Id=CD.VoucherDetailId
								INNER JOIN trn.CapitalizationMaster CM ON CM.Id=CD.CapitalizationMasterId
								INNER JOIN trn.Voucher V ON V.Id=CM.VoucherId
								WHERE VD.VoucherId= '" + voucherId + @"' ";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }

                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].BankJournal SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                  

                }
                if (sourceType == SourceType.VendorInvoice.ToString()|| sourceType == SourceType.InventoryPayable.ToString()||sourceType == SourceType.CustomerInvoice.ToString())
                {
                    if (sourceType == SourceType.InventoryPayable.ToString())
                    {
                        ConnectionManager.DAL.ConManager objCon;
                        DataSet dsMaster = null;
                        string sql = "SELECT * FROM TRN.InventoryIssueHistory WHERE InventoryReceiveDetailId in(SELECT Id FROM TRN.InventoryReceiveDetail WHERE InventoryReceiveId in(SELECT Id FROM TRN.InventoryReceive WHERE VoucherId='" + voucherId + "'))";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        if (dsMaster.Tables[0].Rows.Count>0)
                        {
                            throw new CustomException("Voucher Park Mode not allowed, GRN already issued !");
                        }
                    }
                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    string setOffsql = @"SELECT VoucherNo from trn.InvoiceWriteOffDetail iwd JOIN trn.InvoiceWriteOff iw on iw.Id=iwd.InvoiceWriteOffId LEFT JOIN trn.Voucher v on v.Id = iw.VoucherId
                                            WHERE InvoiceId in (select Id from trn.invoice where VoucherId = '" + voucherId + "')";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed, Payment Voucher No '"+ dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }

                    DataSet dsMaster2 = null;
                    string setOffsq2 = @"SELECT V.VoucherNo	
								from	[TRN].[CapitalizationMasterDetail] CD
								INNER JOIN trn.VoucherDetail VD ON VD.Id=CD.VoucherDetailId
								INNER JOIN trn.CapitalizationMaster CM ON CM.Id=CD.CapitalizationMasterId
								INNER JOIN trn.Voucher V ON V.Id=CM.VoucherId
								WHERE VD.VoucherId= '" + voucherId + @"' ";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsq2, out dsMaster2, false, "1");

                    if (dsMaster2.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster2.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }

                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var InvoiceSql = @"UPDATE [TRN].Invoice SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(InvoiceSql);

                }
                if (sourceType == SourceType.InvoiceToAcceptance.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var InvoiceSql = @"UPDATE [TRN].Invoice SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].InvoiceWriteOff SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(bankJournalSql);
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
                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    string setOffsql = @"SELECT VoucherNo from trn.AdvanceWriteOffDetail iwd JOIN trn.AdvanceWriteOff iw on iw.Id=iwd.AdvanceWriteOffId 
                    LEFT JOIN    trn.Voucher v on v.Id = iw.VoucherId WHERE AdvanceId in (select Id from trn.Advance where VoucherId = '"+voucherId+@"')";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed, SetOf Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].Advance SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == SourceType.VendorAdvanceWriteOff.ToString() || sourceType == SourceType.CustomerAdvanceWriteOff.ToString() || sourceType == SourceType.EmployeeAdvanceWriteOff.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].AdvanceWriteOff SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == SourceType.Loan.ToString()|| sourceType == SourceType.Investment.ToString() || sourceType == SourceType.AutoLoan.ToString())
                {
                    ConnectionManager.DAL.ConManager objCon1;
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    DataSet dsMaster1 = null;
                    
                    string setOffsql = @"SELECT VoucherNo from trn.FinancingDetailWriteOff FDW JOIN trn.FinancingWriteOff FW on FW.Id=FDW.FinancingWriteOffId 
                    LEFT JOIN    trn.Voucher v on v.Id = FW.VoucherId WHERE FDW.FinancingId in (select Id from [TRN].[Financing] where VoucherId = '" + voucherId + @"')";
                   
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");
                   

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed, SetOf Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }
                    DataSet dsLoanInterest = null;
                    string loanInterestsql = @"SELECT VoucherNo,FST.SourceType from TRN.FinancingSubsequentTransaction FST INNER JOIN    
                    trn.Voucher v on v.Id = FST.VoucherId WHERE FST.SourceType NOT IN('Loan','AutoLoan','Investment') AND FST.FinancingId in (select Id from [TRN].[Financing] where VoucherId = '" + voucherId + @"')";
                    objCon1.OpenDataSetThroughAdapter(loanInterestsql, out dsLoanInterest, false, "1");
                    if (dsLoanInterest.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed, " + dsLoanInterest.Tables[0].Rows[0]["SourceType"].ToString() + "  Voucher No: '" + dsLoanInterest.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }

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
                if (sourceType == SourceType.LoanPayment.ToString()|| sourceType == SourceType.InvestmentSetOff.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].FinancingWriteOff SET ISPark=1,IsPosted=0 WHERE VoucherId='" + voucherId + "'";
                    var subTrn = @"UPDATE [TRN].FinancingSubsequentTransaction SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                    rdBuilder.Append(subTrn);
                }
                if (sourceType == SourceType.DebitNote.ToString() || sourceType == SourceType.CreditNote.ToString())
                {
                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    string setOffsql = @"SELECT VoucherNo from trn.InvoiceWriteOffDetail iwd JOIN trn.InvoiceWriteOff iw on iw.Id=iwd.InvoiceWriteOffId LEFT JOIN trn.Voucher v on v.Id = iw.VoucherId
                                            WHERE iwd.AdjustmentNoteId in (select Id from trn.AdjustmentNote where VoucherId = '" + voucherId + "')";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].AdjustmentNote SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == SourceType.DebitNoteSetOff.ToString() || sourceType == SourceType.CreditNoteSetOff.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].InvoiceWriteOff SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(bankJournalSql);
                }
                if (sourceType == SourceType.EmployeePayable.ToString())
                {
                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    string setOffsql = @"SELECT VoucherNo from trn.EmployeePayableWriteOffDetail iwd JOIN trn.EmployeePayableWriteOff iw on iw.Id=iwd.EmployeePayableWriteOffId LEFT JOIN trn.Voucher v on v.Id = iw.VoucherId
                                            WHERE EmployeePayableId in (select Id from trn.EmployeePayable  where VoucherId = '" + voucherId + "')";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed, Payment Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }

                    DataSet dsMaster2 = null;
                    string setOffsq2 = @"SELECT V.VoucherNo	
								from	[TRN].[CapitalizationMasterDetail] CD
								INNER JOIN trn.VoucherDetail VD ON VD.Id=CD.VoucherDetailId
								INNER JOIN trn.CapitalizationMaster CM ON CM.Id=CD.CapitalizationMasterId
								INNER JOIN trn.Voucher V ON V.Id=CM.VoucherId
								WHERE VD.VoucherId= '" + voucherId + @"' ";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsq2, out dsMaster2, false, "1");

                    if (dsMaster2.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster2.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }

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
                if (sourceType == SourceType.FinalSettlementJournal.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + @"'
                                       UPDATE [TRN].EmployeePayableWriteOff SET RowState='Parked' WHERE VoucherId='" + voucherId + @"' 
                                       UPDATE trn.AdvanceWriteoff SET IsPark=1 where VoucherId='" + voucherId + @"'
                                       UPDATE trn.EmployeeSubsequentTransaction SET IsPark=1 where VoucherId='" + voucherId + @"' ";
                    
                    rdBuilder.Append(voucherSql);
                }
                if (sourceType == SourceType.SalaryPayable.ToString() || sourceType == SourceType.SalaryDisbursement.ToString() || sourceType == SourceType.BonusDisbursement.ToString() || sourceType == SourceType.PFESICDisbursement.ToString() )
                {
                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    string setOffsql = @"SELECT V.VoucherNo
								from	 [dbo].[SalaryLock] SL
								INNER JOIN trn.Voucher V ON V.Id=SL.DisbursementVoucherId
								WHERE SL.PayableVoucherId= '" + voucherId + @"'
                                GROUP BY V.VoucherNo ";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                }
                if (sourceType == SourceType.GoodWorkDisbursement.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + @"'
                                       UPDATE trn.EmployeeSubsequentTransaction SET IsPark=1 where VoucherId='" + voucherId + @"' ";
                    rdBuilder.Append(voucherSql);
                }
                if (sourceType == SourceType.FixedAssetCapitalizeJournal.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                }
                if (sourceType == SourceType.ServicePayable.ToString())
                {
                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    string setOffsql = @"SELECT V.VoucherNo	
								from	[TRN].[CapitalizationMasterDetail] CD
								INNER JOIN trn.VoucherDetail VD ON VD.Id=CD.VoucherDetailId
								INNER JOIN trn.CapitalizationMaster CM ON CM.Id=CD.CapitalizationMasterId
								INNER JOIN trn.Voucher V ON V.Id=CM.VoucherId
								WHERE VD.VoucherId= '" + voucherId + @"' ";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }

                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                }
                if (sourceType == SourceType.EmployeeAdvance.ToString())
                {
                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    DataSet dsMaster2 = null;
                    string setOffsql = @"SELECT VoucherNo from trn.AdvanceWriteOffDetail iwd JOIN trn.AdvanceWriteOff iw on iw.Id=iwd.AdvanceWriteOffId LEFT JOIN trn.Voucher v on v.Id = iw.VoucherId
                                            WHERE iwd.AdvanceId in (select Id from trn.Advance where VoucherId = '" + voucherId + "')";
                    string setOffsql2 = @"SELECT VoucherNo from trn.AdvanceWriteOffDetail iwd JOIN trn.AdvanceWriteOff iw on iw.Id=iwd.AdvanceWriteOffId LEFT JOIN trn.Voucher v on v.Id = iw.VoucherId
                                            WHERE iwd.EmployeeAdvanceDetailId in (select Id from [TRN].[EmployeeAdvanceDetail] where VoucherId = '" + voucherId + "')";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql2, out dsMaster2, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }
                    if (dsMaster2.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster2.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }

                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var advanceSql = @"UPDATE [TRN].Advance SET ISPark=1,IsPosted=0 WHERE VoucherId='" + voucherId + @"'
                                       UPDATE trn.EmployeeSubsequentTransaction SET IsPark=1 where VoucherId='" + voucherId + @"' ";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(advanceSql);
                }
                if (sourceType == SourceType.SalesInvoice.ToString() || sourceType == SourceType.PostInvoice.ToString())
                {

                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    string setOffsql = @"SELECT VoucherNo from trn.InvoiceWriteOffDetail iwd JOIN trn.InvoiceWriteOff iw on iw.Id=iwd.InvoiceWriteOffId LEFT JOIN trn.Voucher v on v.Id = iw.VoucherId
                                            WHERE InvoiceId in (select Id from trn.invoice where VoucherId = '" + voucherId + "')";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var invoiceSql = @"UPDATE [TRN].Invoice SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    var salesSql = @"UPDATE [TRN].Sales SET RowState='Parked' WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(invoiceSql);
                    rdBuilder.Append(salesSql);
                }
                if (sourceType == SourceType.LoanInterestPayable.ToString() || sourceType == SourceType.LoanInterestPayableReverse.ToString() || sourceType == "OtherExpensesPayable" || sourceType == "LoanTax")
                {
                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    string setOffsql = @"SELECT VoucherNo from TRN.FinancingWriteOff FW  LEFT JOIN trn.Voucher v on v.Id = FW.VoucherId
                                            WHERE FW.FinancingId in (select FinancingId from TRN.FinancingSubsequentTransaction where VoucherId = '" + voucherId + "')";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }

                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var invoiceSql = @"UPDATE TRN.FinancingSubsequentTransaction SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(invoiceSql);
                }
                if (sourceType == SourceType.VendorInvoiceCharge.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var invoiceSql = @"UPDATE TRN.InvoiceWriteOff SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(invoiceSql);
                }
                if (sourceType == SourceType.IssueJournal.ToString())
                {
                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    string setOffsql = @"SELECT V.VoucherNo	
								from	[TRN].[CapitalizationMasterDetail] CD
								INNER JOIN trn.VoucherDetail VD ON VD.Id=CD.VoucherDetailId
								INNER JOIN trn.CapitalizationMaster CM ON CM.Id=CD.CapitalizationMasterId
								INNER JOIN trn.Voucher V ON V.Id=CM.VoucherId
								WHERE VD.VoucherId= '" + voucherId + @"' ";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Voucher Park Mode not allowed,  Voucher No '" + dsMaster1.Tables[0].Rows[0]["VoucherNo"].ToString() + "' have to delete first!");
                    }

                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                }
                if (sourceType == SourceType.SalaryJournal.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                }
                if (sourceType == SourceType.CustomerBanksReceipt.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    var invoiceWriteOffSql = @"UPDATE [TRN].InvoiceWriteOff SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    var bankJournalSql = @"UPDATE [TRN].FinancingWriteOff SET ISPark=1,IsPosted=0 WHERE VoucherId='" + voucherId + "'";
                    var subTrn = @"UPDATE [TRN].FinancingSubsequentTransaction SET ISPark=1 WHERE VoucherId='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                    rdBuilder.Append(invoiceWriteOffSql);
                    rdBuilder.Append(bankJournalSql);
                    rdBuilder.Append(subTrn);
                }
                if (sourceType == SourceType.PurchaseDocAcceptance.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);   
                }
                if (sourceType == SourceType.InvoiceCharge.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                }
                if (sourceType == SourceType.PurchaseLCOpeningCharges.ToString())
                {
                    var voucherSql = @"UPDATE [TRN].Voucher SET ISPark=1 WHERE Id='" + voucherId + "'";
                    rdBuilder.Append(voucherSql);
                }

                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                _unitOfWork.SaveChanges();
                _accountsCommonService.InsertVoucherLogParked(voucherId, voucher.VoucherNo, "", "", "", "", "", "", "", "", "", "", "", "");
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