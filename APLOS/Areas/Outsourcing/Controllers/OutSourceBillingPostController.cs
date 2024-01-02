using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using OTSBD;
using Library.MaterialManagement.JobWork;
using Library.Accounting.Accounts;
using Library.ViewModel.Vouchers;
using Library.Model.Vouchers;
using Library.Model.Enums;
using Library.Data;
using Library.Service.Logs;
using System.Reflection;
using Library.Service.Enums;

namespace Aplos.Areas.Outsourcing.Controllers
{
    public class OutSourceBillingPostController : BaseController
    {
        JobWorkReceiptValueAdded R = new JobWorkReceiptValueAdded();

        #region Constructor
        private readonly SqlRepository _sqlRepository;
        public OutSourceBillingPostController(SqlRepository Repository)
        {
            _sqlRepository = Repository;
            R = new JobWorkReceiptValueAdded();
        }
        #endregion

        #region Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region Operations

        [HttpGet, Authorize]
        public ActionResult GetOutsourcingBillingNonPostData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT RB.Id,RB.PlantId,RB.OSTransformationPOId,RB.InvoiceNo,RB.InvoiceNo DocRefNo
			, FORMAT(RB.InvoiceDate,'dd-MMM-yyyy') InvoiceDate
			, FORMAT(RB.InvoiceDate,'dd-MMM-yyyy') PostingDate,FORMAT(RB.InvoiceDate,'dd-MMM-yyyy') DocDate
			, RB.VoucherId,RB.BillingRate,RB.BillingRate CompanyCurrencyRate
			, TabType='Transformation', tc.EntityId,tc.PartyId,tc.Remarks,FORMAT(tc.PODate,'dd-MMM-yyyy') as ValueAddedDate
			                --,FORMAT(tc.[Time],'hh:mm tt')[VACTime]
                            ,FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
			                FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
			                e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName,tc.CurrencyId,CU.Code Currency
                            ,TC.PurchaseLCId,ISNULL(LC.LCRef,'')LCRef,ISNULL(CN.ContractNo,'')ContractNo
			                from [dbo].[OSReceiveBilling] RB
			                LEFT JOIN [dbo].[OSTransformationPO] tc ON tc.Id=RB.OSTransformationPOId
			                LEFT JOIN ORG.Entity e on e.Id=tc.EntityId
			                LEFT JOIN HKP.Party p on p.Id=tc.PartyId
                            LEFT JOIN [SCS].[Currency] AS CU ON tc.CurrencyId=CU.Id
							LEFT JOIN dbo.PurchaseLC LC ON LC.Id=TC.PurchaseLCId
							LEFT JOIN dbo.[Contract] CN ON CN.Id=TC.ContractId 
                            Where RB.PlantId='" + identity.PlantId + "' AND RB.VoucherId IS NULL";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetJWReceiveBillingDetailData(string masterId)
        {
            try
            {
                string sql = @"SELECT RBD.*,CTC.Id OSTransformationPODetailId,CTC.MaterialMasterId,MM.UserName MaterialName
                            ,ART.Id ArticleId,ART.StandardName Article,CTC.FirstCharacteristicsId,FC.UserName AS SKU1 ,CTC.FirstCharacteristicsValueId,FCV.UserName AS FirstCharacteristicsValue
                            ,CTC.SecondCharacteristicsId,SC.UserName AS SKU2,CTC.SecondCharacteristicsValueId,SCV.UserName AS SecondCharacteristicsValue
                            ,CTC.ThirdCharacteristicsId,TC.UserName AS SKU3,CTC.ThirdCharacteristicsValueId,TCV.UserName AS ThirdCharacteristicsValue
                            ,CTC.Quantity OrderQty,IRD.TransactionQty ReceiveQty,ISNULL(B.BillingQty,0) OtherBillingQty,(IRD.TransactionQty-ISNULL(B.BillingQty,0)) BalanceQty
                            from dbo.OSReceiveBillingDetail RBD 
                            LEFT JOIN [dbo].[OSTransformationPODetail] CTC ON CTC.Id=RBD.OSTransformationPODetailId
                            --LEFT JOIN dbo.JobWorkTransformationContract JWTC ON JWTC.Id=CTC.OSTransformationPOId
                            --LEFT JOIN [dbo].[OSTransformationPO] JWPO ON JWPO.Id=CTC.OSTransformationPOId
                            LEFT JOIN MST.MaterialMaster AS MM ON CTC.MaterialMasterId = MM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON CTC.ArticleId = ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON CTC.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON CTC.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON CTC.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON CTC.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON CTC.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON CTC.ThirdCharacteristicsValueId = TCV.Id
                            LEFT JOIN (select SUM(TransactionQty) TransactionQty,OSTransformationPODetailId,MaterialTranRate from TRN.InventoryReceiveDetail GROUP BY OSTransformationPODetailId,MaterialTranRate) IRD ON IRD.OSTransformationPODetailId=CTC.Id
                            LEFT JOIN (Select OSTransformationPODetailId,SUM(BillingQty) BillingQty from dbo.OSReceiveBillingDetail WHERE OSReceiveBillingId<>'" + masterId + @"' GROUP BY OSTransformationPODetailId) B ON B.OSTransformationPODetailId=CTC.Id
                            WHERE RBD.OSReceiveBillingId='" + masterId + "'";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetOutsourcingBillingJV(string billingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsOutsourceBillingService _accountsOutsourceBillingService = new AccountsOutsourceBillingService(_sqlRepository);
            return Json(_accountsOutsourceBillingService.GetOutsourceBillingJV(identity.CompanyId, identity.PlantId, billingId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetOutsourcingBillingPostedList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(OutsourcingBillingPostedList(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> OutsourcingBillingPostedList(string column, string value, string plantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        select top 100 * from (SELECT RB.Id,RB.PlantId,RB.OSTransformationPOId,RB.InvoiceNo,RB.InvoiceNo DocRefNo
			, FORMAT(RB.InvoiceDate,'dd-MMM-yyyy') InvoiceDate
			, FORMAT(RB.InvoiceDate,'dd-MMM-yyyy') PostingDate,FORMAT(RB.InvoiceDate,'dd-MMM-yyyy') DocDate
			, RB.VoucherId,RB.BillingRate,RB.BillingRate CompanyCurrencyRate
			, TabType='Transformation', tc.EntityId,tc.PartyId,tc.Remarks,FORMAT(tc.PODate,'dd-MMM-yyyy') as ValueAddedDate
			                ,FORMAT(tc.[Time],'hh:mm tt')[VACTime],FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
			                FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
			                e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName,tc.CurrencyId,CU.Code Currency
                            ,TC.PurchaseLCId,ISNULL(LC.LCRef,'')LCRef,ISNULL(CN.ContractNo,'')ContractNo
			                from [dbo].[OSReceiveBilling] RB
			                LEFT JOIN [dbo].[OSTransformationPO] tc ON tc.Id=RB.OSTransformationPOId
			                LEFT JOIN ORG.Entity e on e.Id=tc.EntityId
			                LEFT JOIN HKP.Party p on p.Id=tc.PartyId
                            LEFT JOIN [SCS].[Currency] AS CU ON tc.CurrencyId=CU.Id
							LEFT JOIN dbo.PurchaseLC LC ON LC.Id=TC.PurchaseLCId
							LEFT JOIN dbo.[Contract] CN ON CN.Id=TC.ContractId
                            
                            Where RB.PlantId='" + plantId + "' AND RB.VoucherId<>'') AS TEMP WHERE " + strkey + " order by PostingDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            string strSQL, strCSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strCSQL = "DELETE FROM [dbo].[OSReceiveBillingDetail] WHERE OSReceiveBillingId='" + Id + "'";
                strSQL = "DELETE FROM [dbo].[OSReceiveBilling] WHERE Id = '" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strCSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function


        [HttpPost]
        public JsonResult OutSourceBillingPost(string outsourceBillingId,VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
           
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            InsertOutSourceBillingPost(outsourceBillingId,voucherVM, voucherDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        public string InsertOutSourceBillingPost(string outsourceBillingId,VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList )
        {
            try
            {
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);


                DataSet _drvDetailData = null;
                DataSet _drvDetailCurrencyData = null;
                DataSet _crvDetailData = null;
                DataSet _crvDetailCurrencyData = null;
                //DataSet _frDisposeData = null;
                //DataSet _fixedAssetRegisterData = null;
                //DataSet _advanceReqScheData = null;

                var voucher = new Voucher
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    CurrencyId = voucherVM.CurrencyId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    VoucherDate = DateTime.Now,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = "Posted",//voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.OutSourceBilling.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId
                };
                _accountsCommonService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);

                var currentVoucherDetaiRecord = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {

                    if (voucherDetailVM.TrnType == "Dr" && voucherDetailVM.Amount > 0)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // in libility side Dr.
                        var voucherDr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            DrAmount = voucherDetailVM.Amount,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord, ref _drvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucher.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _accountsCommonService.GetCompanyCurrencyExchange(voucher.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            DrAmount = voucherDr.DrAmount * voucherVM.CompanyCurrencyRate
                        }, ref _drvDetailCurrencyData);
                    }
                    else if (voucherDetailVM.TrnType == "Cr" && voucherDetailVM.Amount > 0)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        // INSERT INTO VoucherDetail
                        var voucherCr = new VoucherDetail
                        {
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            CurrencyId = voucher.CurrencyId,
                            DrAmount = 0,
                            CrAmount = voucherDetailVM.Amount,
                        };
                        currentVoucherDetaiRecord++;
                        _accountsCommonService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord, ref _crvDetailData);

                        _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                        {
                            ParallelCurrencyId = companyCurrencyId,
                            FromCurrencyId = voucher.CurrencyId,
                            ToCurrencyId = companyCurrencyId,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                            ToCurrencyConversion = _accountsCommonService.GetCompanyCurrencyExchange(voucher.CurrencyId, companyCurrencyId, voucherVM.CompanyCurrencyRate),
                            CrAmount = voucherCr.CrAmount * voucherVM.CompanyCurrencyRate
                        }, ref _crvDetailCurrencyData);
                    }
                }

                ConnectionManager.DAL.ConManager objCon;
                objCon = new ConnectionManager.DAL.ConManager("1");
                DataSet dsBillMaster;
                objCon.OpenDataSetThroughAdapter("select * from dbo.OSReceiveBilling Where Id='" + outsourceBillingId + "'", out dsBillMaster, false, "1");

                DataView dv = new DataView(dsBillMaster.Tables[0]);
                dv.RowFilter = "Id='" + outsourceBillingId + "'";
               
                if (dv.Count > 0)
                {
                    DataRow drmo = dv[0].Row;
                    drmo.BeginEdit();

                    drmo["VoucherId"] = voucher.Id;
                    drmo["UpdatedBy"] = voucher.AddedBy;
                    drmo["UpdatedDate"] = DateTime.Now.ToString();
                    drmo["UpdatedFromIP"] = voucher.AddedFromIP;
                    drmo.EndEdit();
                }

                clsStaticInfo objApp = new clsStaticInfo();
                objApp.SaveDataSets(_vdataset, _drvDetailData, _drvDetailCurrencyData, _crvDetailData, _crvDetailCurrencyData, dsBillMaster);
               

                
                return voucher.VoucherNo;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        #endregion





    }
}