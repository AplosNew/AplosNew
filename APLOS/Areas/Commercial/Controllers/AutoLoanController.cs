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
using System.Linq;
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
        [Authorize, HttpGet]
        public JsonResult GetAutoLoanPostableDetailList(string LoanAgainstAcceptanceMasterId, string SourceType)
        {
            AccountsAutoLoanService accountsAutoLoanService = new AccountsAutoLoanService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(accountsAutoLoanService.GetAutoLoanPostableDetailList(identity.PlantId, LoanAgainstAcceptanceMasterId, SourceType), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [HttpPost]
        public JsonResult SaveAutoLoan(List<Dictionary<string, object>> autoLoanData,Dictionary<string,object> LCModel)
        {
            try
            {
                #region Validation
                string LC = "";
                for (int i = 0; i < autoLoanData.Count; i++)
                {
                    if (i == 0 ||LC == autoLoanData[i]["PurchaseLCNo"].ToString())
                    {
                        LC = autoLoanData[i]["PurchaseLCNo"].ToString();
                    }
                    else
                    {
                        throw new Exception("LC should be matched with " + LC + " ");
                    }
                }
                if (string.IsNullOrEmpty(LCModel["LoanDate"].ToString()))
                {
                    throw new Exception("Insert Loan date");
                }
                if (string.IsNullOrEmpty(LCModel["LoanNo"].ToString()))
                {
                    throw new Exception("Insert Loan no");
                }
                #endregion
                SaveLoanAgainstAcceptance(autoLoanData, LCModel);
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
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "LoanAgainstAcceptanceMaster", out sID);
            return sID;
        }
        private void SaveLoanAgainstAcceptance(List<Dictionary<string, object>> data, Dictionary<string, object> LCModel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster,dsDetails; DataRow drSave, drMSave;
                    string MasterId = string.Empty; int count = 0;
                    
                    string sql = "SELECT * FROM [LoanAgainstAcceptanceMaster] WHERE 1=2";
                    string sql2 = "SELECT * FROM [LoanAgainstAcceptanceDetail] WHERE 1=2";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                    objCon.OpenDataSetThroughAdapter(sql2, out dsDetails, false, "1");

                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        drMSave = dsMaster.Tables[0].NewRow();
                        drMSave["Id"] = GetPK();
                        MasterId = drMSave["Id"].ToString();

                        drMSave["VoucherId"] = null;
                        drMSave["CompanyGroupId"] = identity.CompanyGroupId;
                        drMSave["CompanyId"] = identity.CompanyId;
                        drMSave["PlantId"] = identity.PlantId;
                        //drMSave["EntityId"] = data[0]["EntityId"];
                        drMSave["CurrencyId"] = data[0]["CurrencyId"];
                        drMSave["PartyType"] = "Vendor";
                        drMSave["PartyId"] = data[0]["PartyId"];
                        drMSave["PartyPlantId"] = data[0]["PartyPlantId"];
                        drMSave["PaymentSource"] = "Bank";
                        drMSave["TransactionType"] = "LoanTaken";
                        drMSave["Amount"] = LCModel["Amount"];
                        drMSave["LoanDate"] = LCModel["LoanDate"];
                        drMSave["LoanNo"] = LCModel["LoanNo"];
                        drMSave["IsPark"] = true;

                        drMSave["AddedBy"] = identity.Name;
                        drMSave["AddedDate"] = DateTime.Now;
                        drMSave["AddedFromIP"] = identity.IPAddress;

                        drMSave["UpdatedBy"] = identity.Name;
                        drMSave["UpdatedDate"] = DateTime.Now;
                        drMSave["UpdatedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(drMSave);

                    }
                    foreach (var item in data)
                    {
                        dsDetails.Tables[0].DefaultView.RowFilter = "Id = '" + item["PurchaseDocAcceptanceId"] + "'";
                        if (dsDetails.Tables[0].DefaultView.Count == 0)
                        {
                            count++;
                            drSave = dsDetails.Tables[0].NewRow();
                            drSave["Id"] =  MasterId + count;
                            drSave["LoanAgainstAcceptanceMasterId"] = MasterId;
                            if(item["SourceType"].ToString() == "Acceptance")
                            {
                                drSave["PurchaseDocAcceptanceId"] = item["PurchaseDocAcceptanceId"];
                            }
                            else
                            {
                                drSave["InvoiceId"] = item["PurchaseDocAcceptanceId"];
                            }
                            
                            drSave["BankMasterId"] = item["BankMasterId"];

                            drSave["AddedBy"] = identity.Name;
                            drSave["AddedDate"] = DateTime.Now;
                            drSave["AddedFromIP"] = identity.IPAddress;

                            drSave["UpdatedBy"] = identity.Name;
                            drSave["UpdatedDate"] = DateTime.Now;
                            drSave["UpdatedFromIP"] = identity.IPAddress;
                            dsDetails.Tables[0].Rows.Add(drSave);

                        }
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsDetails);
                    //foreach (var item in data)
                    //{
                    //    string sql = "SELECT * FROM [trn].[LoanAgainstAcceptance] WHERE Id='" + item["Id"] + "'";
                    //    objCon = new ConnectionManager.DAL.ConManager("1");
                    //    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                    //    if (dsMaster.Tables[0].Rows.Count == 0)
                    //    {
                    //        DataRow dr = dsMaster.Tables[0].NewRow();
                    //        dr["Id"] = GetPK();
                    //        dr["PurchaseDocAcceptanceId"] = item["PurchaseDocAcceptanceId"];
                    //        dr["VoucherId"] = null;
                    //        dr["BankMasterId"] = item["BankMasterId"];
                    //        dr["CompanyGroupId"] = identity.CompanyGroupId;
                    //        dr["CompanyId"] = identity.CompanyId;
                    //        dr["PlantId"] = identity.PlantId;
                    //        //dr["EntityId"] = identity.EntityId;
                    //        dr["CurrencyId"] = item["CurrencyId"];
                    //        dr["PartyType"] = "Vendor";
                    //        dr["PartyId"] = item["PartyId"];
                    //        dr["PartyPlantId"] = item["PartyPlantId"];
                    //        dr["Amount"] = item["Amount"];
                    //        dr["PaymentSource"] = "Bank";
                    //        dr["TransactionType"] = "LoanTaken";
                    //        dr["LoanDate"] = item["LoanDate"];
                    //        dr["LoanNo"] = item["LoanNo"];
                    //        dr["IsPark"] = true;

                    //        dr["AddedBy"] = identity.Name;
                    //        dr["AddedDate"] = DateTime.Now;
                    //        dr["AddedFromIP"] = identity.IPAddress;

                    //        dsMaster.Tables[0].Rows.Add(dr);
                    //    }
                    //    else
                    //    {
                    //        //edit
                    //        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    //        dr.BeginEdit();

                    //        dr["PurchaseDocAcceptanceId"] = item["PurchaseDocAcceptanceId"];
                    //        dr["VoucherId"] = null;
                    //        dr["BankMasterId"] = item["BankMasterId"];
                    //        dr["CompanyGroupId"] = identity.CompanyGroupId;
                    //        dr["CompanyId"] = identity.CompanyId;
                    //        dr["PlantId"] = identity.PlantId;
                    //        //dr["EntityId"] = identity.EntityId;
                    //        dr["CurrencyId"] = item["CurrencyId"];
                    //        dr["PartyType"] = "Vendor";
                    //        dr["PartyId"] = item["PartyId"];
                    //        dr["PartyPlantId"] = item["PartyPlantId"];
                    //        dr["Amount"] = item["Amount"];
                    //        dr["PaymentSource"] = "Bank";
                    //        dr["TransactionType"] = "LoanTaken";
                    //        dr["LoanDate"] = item["LoanDate"];
                    //        dr["LoanNo"] = item["LoanNo"];
                    //        dr["IsPark"] = true;

                    //        dr["AddedBy"] = identity.Name;
                    //        dr["AddedDate"] = DateTime.Now;
                    //        dr["AddedFromIP"] = identity.IPAddress;

                    //        dr.EndEdit();
                    //    }
                    //    clsStaticInfo obj = new clsStaticInfo();
                    //    obj.SaveDataSets(dsMaster);
                    //}
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        [HttpGet, Authorize]
        public JsonResult GetSaveData()
        {
            try
            {
                AccountsAutoLoanService _accountsLoanService = new AccountsAutoLoanService(_sqlRepository);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(_accountsLoanService.GetMaster(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
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
        public JsonResult AutoLoanPost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<FinancingScheduleViewModel> loanRepaymentSchedulelist)
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
            if (voucherDetailVMList.FirstOrDefault().CompanyCurrencyRate < 0 || voucherDetailVMList.FirstOrDefault().CompanyCurrencyRate == 0)
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
            if (voucherVM.SettlementType == "Acceptance")
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _autoLoanService.ParkAutoLoan(voucherVM, voucherDetailVMList, existingLoanList, loanRepaymentSchedulelist)) });
            }
            else
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _autoLoanService.ParkAutoLoanInvoice(voucherVM, voucherDetailVMList, existingLoanList, loanRepaymentSchedulelist)) });
            }
        }

    }


}