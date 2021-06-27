#region Using

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
using Library.Security.Core;
using Library.Service.Finances;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class AutoLoanController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IAutoLoanService _autoLoanService;

        public AutoLoanController( ISqlRepository R
           , IAutoLoanService autoLoanService
            )
        {
            _sqlRepository = R;
            _autoLoanService = autoLoanService;
        }
        #endregion

        #region -- Pages
       
        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult AutoLoanPost()
        {
            return View();
        }
        #endregion

        [Authorize, HttpGet]
        public JsonResult GetAutoLoanAvailableList(bool dateRange,string fromDate, string toDate)
        {
            AccountsAutoLoanService accountsAutoLoanService = new AccountsAutoLoanService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(accountsAutoLoanService.GetAutoLoanAvailableList(identity.PlantId,dateRange,fromDate,toDate), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [Authorize, HttpGet]
        public JsonResult GetAutoLoanPostableList()
        {
            AccountsAutoLoanService accountsAutoLoanService = new AccountsAutoLoanService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(accountsAutoLoanService.GetAutoLoanPostableList(identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [HttpPost]
        public JsonResult SaveAutoLoan(IEnumerable<LoanAgainstAcceptance> autoLoanData)
        {
            try
            {
                SaveLoanAgainstAcceptance(autoLoanData);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(LoanAgainstAcceptance), out sID);
            return sID;
        }
        private void SaveLoanAgainstAcceptance(IEnumerable<LoanAgainstAcceptance> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [trn].[LoanAgainstAcceptance] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetPK();
                            dr["PurchaseDocAcceptanceId"] = item.PurchaseDocAcceptanceId;
                            dr["VoucherId"] = null;
                            dr["BankMasterId"] = item.BankMasterId;
                            dr["CompanyGroupId"] = identity.CompanyGroupId;
                            dr["CompanyId"] = identity.CompanyId;
                            dr["PlantId"] = identity.PlantId;
                            //dr["EntityId"] = identity.EntityId;
                            dr["CurrencyId"] = item.CurrencyId;
                            dr["PartyType"] = "Vendor";
                            dr["PartyId"] = item.PartyId;
                            dr["PartyPlantId"] = item.PartyPlantId;
                            dr["Amount"] = item.Amount;
                            dr["PaymentSource"] = "Bank";
                            dr["TransactionType"] = "LoanTaken";
                            dr["LoanDate"] = item.LoanDate;
                            dr["LoanNo"] = item.LoanNo;
                            dr["IsPark"] = true;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["PurchaseDocAcceptanceId"] = item.PurchaseDocAcceptanceId;
                            dr["VoucherId"] = null;
                            dr["BankMasterId"] = item.BankMasterId;
                            dr["CompanyGroupId"] = item.CompanyGroupId;
                            dr["CompanyId"] = item.CompanyId;
                            dr["PlantId"] = item.PlantId;
                            dr["EntityId"] = item.EntityId;
                            dr["CurrencyId"] = item.CurrencyId;
                            dr["PartyType"] = item.PartyType;
                            dr["PartyId"] = item.PartyId;
                            dr["PartyPlantId"] = item.PartyPlantId;
                            dr["Amount"] = item.Amount;
                            dr["PaymentSource"] = "Bank";
                            dr["TransactionType"] = "LoanTaken";
                            dr["LoanDate"] = item.LoanDate;
                            dr["LoanNo"] = item.LoanNo;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoLoanList(GridParameter parameters)
        {
            AccountsAutoLoanService _accountsLoanService = new AccountsAutoLoanService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.LoanQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.AutoLoan), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AutoLoanPost(VoucherViewModel voucherVM, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<FinancingScheduleViewModel> loanRepaymentSchedulelist)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = false;
            voucherVM.SourceType = SourceType.AutoLoan.ToString();
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.Amount < 0 || voucherVM.Amount == 0)
                throw new CustomException("Please Input Amount !");
            if (voucherVM.CompanyCurrencyRate < 0 || voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            if (voucherVM.TransactionType == null)
                throw new CustomException("Please Select Loan Type !");
         
            if (voucherVM.PartyType == PartyType.Vendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            if (voucherVM.PartyType == PartyType.Director.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Director!");
            if (voucherVM.IsSchedule)
            {
                if (voucherVM.RepaymentStartDate == null)
                    throw new CustomException("Please Input  Repayment Date!");
                if (voucherVM.ProfitRate == 0)
                    throw new CustomException("Please Input  Profit Rate!");
                if (voucherVM.LifeOfYear == 0)
                    throw new CustomException("Please Input  Life Of Year!");
                if (voucherVM.NoOfInstallmentPerYear == 0)
                    throw new CustomException("Please Input  No Of Installment!");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _autoLoanService.ParkAutoLoan(voucherVM, existingLoanList, loanRepaymentSchedulelist)) });
        }

    }


}