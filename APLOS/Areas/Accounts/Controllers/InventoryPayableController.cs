using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using System.Threading;
using System.Web.Mvc;
using Library.Core;
using Library.Accounting.Accounts;
using Library.Data.Sql;
using System;
using Library.Data;
using Library.Service.Logs;
using Library.Service.Enums;
using System.Reflection;
using System.Collections.Generic;
using Aplos.Properties;
using Library.ViewModel.Vouchers;

namespace Aplos.Areas.Accounts.Controllers
{
    public class InventoryPayableController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
       // private readonly IInventoryPayableService _inventoryPayableService;

        public InventoryPayableController(
         //   IInventoryPayableService inventoryPayableService
             ISqlRepository sqlRepository
            )
        {
         //   _inventoryPayableService = inventoryPayableService;
            _sqlRepository = sqlRepository;
        }


        public ActionResult InventoryPayable()
        {
            return View();
        }

        public ActionResult InventoryOutSourceReceivePost()
        {
            return View();
        }

        public ActionResult ServicePayable()
        {
            return View();
        }


        public ActionResult InventoryIssueJournal()
        {
            return View();
        }

        public ActionResult InventoryIssueReturnJournal()
        {
            return View();
        }

        #region GRN Payable


        [HttpPost, Authorize]
        public JsonResult GetPostingList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetPostingList(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetPostingList(string column, string value, string plantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        select top 100 * from (SELECT IR.Id,IR.Id GRNNo, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
			                        , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                    , Particular=CASE WHEN IR.EmployeeId<>'' THEN EI.EmployeeName WHEN IR.PartyId<>'' THEN P.UserName  ELSE P.UserName END
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice
                                    , InvoiceId=CASE WHEN IR.EmployeeId<> '' THEN EP.Id ELSE IV.Id END
                                    , IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IR.InvoicingPartyPlantId PartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty,IRD.ShortageQty,IRD.ShortageValue, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, IR.IsTaxApplicable
                                    , COUNT(*) OVER () AS TotalRows
									,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
									,IR.GateEntryNo,IR.POId,IR.ToCurrencyRate,IR.NoteForAccounts Narration
									,VoucherNo = CASE WHEN IR.EmployeeId <>'' THEN VE.VoucherNo ELSE V.VoucherNo END
									,VoucherId = CASE WHEN IR.EmployeeId <>'' THEN VE.Id ELSE V.Id END
									,VoucherTypeId = CASE WHEN IR.EmployeeId <>'' THEN VE.VoucherTypeId ELSE V.VoucherTypeId END
									,PostingDate= CASE WHEN IR.EmployeeId <>'' THEN REPLACE(CONVERT(CHAR(11), VE.PostingDate, 106),' ','-') ELSE REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') END
                                    ,MS.UserName MaterialStorageName, IR.IsFOC, ISNULL(ADT.TaxAmount,0) TDSTax, ADT.VoucherId TDSTaxVoucherId, ADT.Id AdditionalTaxId
                                    ,IsTDSTaxPost=CASE WHEN ADT.VoucherId<>'' THEN 'TDSPosted' WHEN  ADT.InventoryReceiveId IS NULL THEN '' ELSE 'TDSParked' end
                                    ,IsShortagePost=CASE WHEN AN.VoucherId<>'' THEN 'ShortagePosted' WHEN  AN.InventoryReceiveId IS NULL THEN '' ELSE 'ShortageParked' end
                                    ,AN.VoucherId DebitNoteVoucherId
									,VT.VoucherNo TDSVoucherNo,V.IsPark,IV.WrittenOffAmount
                                    ,IR.OtherPartyId,IR.OtherPartyPlantId,V.EntityId,EN.UserName Entity
						FROM [TRN].[InventoryReceive] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty,SUM(ISNULL(A.ShortageQty,0)) AS ShortageQty,SUM(ISNULL(A.ShortageValue,0)) AS ShortageValue, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId=@plantId GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId=@plantId GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        --LEFT JOIN TRN.GRNAcceptanceMap IGD ON IGD.GRNId=IR.Id
                        LEFT JOIN TRN.Invoice IV ON IV.inventoryReceiveId=IR.Id  and IR.PartyId=IV.PartyId
                        LEFT JOIN TRN.Adjustmentnote AN ON AN.inventoryReceiveId=IR.Id
						LEFT JOIN TRN.Voucher V ON V.Id=IR.VoucherId
						LEFT JOIN TRN.EmployeePayable EP ON EP.InventoryReceiveId=IR.Id
						LEFT JOIN TRN.Voucher VE ON VE.Id=EP.VoucherId
                        LEFT JOIN HKP.MaterialStorage MS ON MS.Id=IR.MaterialStorageId
                        LEFT JOIN TRN.AdditionalTax ADT ON ADT.InventoryReceiveId=IR.Id
						LEFT JOIN TRN.Voucher VT ON VT.Id=ADT.VoucherId
                        LEFT JOIN ORG.Entity EN ON EN.Id=V.EntityId
                        WHERE IR.PlantId=@plantId AND V.Archive=0 AND IR.[Status]='Posting' AND IR.IsPaymentHold=0 AND IR.PlantId=@plantId AND IR.FixedAssetOrInventory='Inventory' AND IR.OpeningBalanceId IS NULL
                        ) AS TEMP WHERE " + strkey + " order by PostingDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        /// <summary>
        /// using inventory payable
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Authorize, HttpGet]
        public JsonResult GetListForInvPayable()
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetGRNListForInvPayable(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        

        [HttpPost, Authorize]
        public JsonResult GetAdditionalTaxDetail(string additionalTaxId)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetAdditionalTaxDetail(additionalTaxId));
        }

        [HttpPost, Authorize]
        public JsonResult GetShortageQtyDetail(string grnId,string adjustmentNoteTypeId)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetShortageQtyDetail(grnId, adjustmentNoteTypeId));
        }

        [HttpPost, Authorize]
        public JsonResult GetPurchaseDiscountGL()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetDiscountGL(identity.CompanyId, GeneralAccountDeterminateEnum.PurchaseDiscount));
        }

        public IEnumerable<object> GetDiscountGL(string companyId, GeneralAccountDeterminateEnum type)
        {
            try
            {
                var sql = @"SELECT '" + type + @"' OtherName,0 Amount,0 Dr,0 Cr ,GL.AccountCode GLGeneralInfoCode,GAD.GLGeneralInfoId,GL.UserName GLGeneralInfoName,
				B.Code BudgetCode,GAD.BudgetMasterId,B.UserName BudgetName,A.Code AcitvityCode,GAD.ActivityId,A.UserName ActivityName,0 IsAsset
				, NULL TaxCategoryId,'Dr' TranType, NULL MaterialGroupId, NULL InventoryReceiveDetailId
				FROM HKP.GeneralAccountDeterminate GAD 
				LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=GAD.GLGeneralInfoId
				LEFT JOIN MST.BudgetMaster BM ON BM.Id=GAD.BudgetMasterId
				LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId 
				LEFT JOIN HKP.Activity A ON A.Id=GAD.ActivityId
				LEFT JOIN ORG.Company C ON C.COAId=GAD.COAId
				WHERE GAD.Id='" + type + "' AND C.Id='" + companyId + @"' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        [HttpPost, Authorize]
        public JsonResult GetPurchaseOrderDiscount(string grnId)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetPurchaseOrderDiscount(identity.PlantId, grnId));
        }

        [HttpPost, Authorize]
        public JsonResult GetPurchaseOrderDiscountWithAcceptance(string purchaseDocAcceptanceId)
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetPurchaseOrderDiscountWithAcceptance(identity.PlantId, purchaseDocAcceptanceId));
        }

       

        [HttpGet, Authorize]
        public ActionResult PabyableJournal(ReportFormat reportFormat, string inventoryReceiveId, string employeeId, bool isReversCharge, bool isFoc, string otherVendorId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "GRN";
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            var workbook = accountsInventoryPayableReportService.PabyableJournal(identity.CompanyId, identity.PlantId, inventoryReceiveId, employeeId, isReversCharge, isFoc, reportFileName, otherVendorId);
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

        [HttpGet, Authorize]
        public ActionResult FGInventoryJournal(ReportFormat reportFormat, string inventoryReceiveId, string employeeId, bool isReversCharge, bool isFoc)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "FG Inventory GRN";
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            var workbook = accountsInventoryPayableReportService.FGInventoryJournal(identity.CompanyId, identity.PlantId, inventoryReceiveId, employeeId, isReversCharge, isFoc, reportFileName);
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
        public JsonResult UpdateGRNPaymentTerm(VoucherViewModel voucherVM)
        {
            var inDirect = new System.Text.StringBuilder();
            var inDirectsql = "";

            inDirectsql = @"DECLARE @InventoryReceiveId varchar(50)='" + voucherVM.Id + @"',@PaymentTermId varchar(50)='" + voucherVM.PaymentTermId + @"',@BaseNoOfDays int=" + voucherVM.BaseNoOfDays + @",@POId varchar(50)=''
	                        select @POId=POId from [TRN].[InventoryReceiveDetail]  where InventoryReceiveId=@InventoryReceiveId
	                        update [TRN].[InventoryReceive] set  PaymentTermId=@PaymentTermId ,BaseNoOfDays=@BaseNoOfDays,MatureDate=DATEADD(DAY,@BaseNoOfDays,BaseOnDueDate) where Id=@InventoryReceiveId
	                        update TRN.Invoice set  PaymentTermId=@PaymentTermId,BaseNoOfDays=@BaseNoOfDays,ActualDueDate=DATEADD(DAY,@BaseNoOfDays,BaseOnDueDate),RevisedDueDate=DATEADD(DAY,@BaseNoOfDays,BaseOnDueDate) where InventoryReceiveId=@InventoryReceiveId
	                        update [TRN].[PurchaseOrder] set  PaymentTermId=@PaymentTermId ,BaseNoOfDays=@BaseNoOfDays,MatureDate=DATEADD(DAY,@BaseNoOfDays,BaseOnDueDate) where Id=@POId 

                            update [TRN].[Voucher] set EntityId='" + voucherVM.EntityId + @"' where Id='" + voucherVM.VoucherId + @"'
                            update [TRN].[VoucherDetail]  set EntityId='" + voucherVM.EntityId + @"' where VoucherId='" + voucherVM.VoucherId + @"' 
                            update [TRN].[Invoice]  set EntityId='" + voucherVM.EntityId + @"' where VoucherId='" + voucherVM.VoucherId + @"' ";
            inDirect.Append(inDirectsql);
            _sqlRepository.ExecuteSqlCommand(inDirect.ToString());
            return Json(new { Message = AplosMessage.Updated });
        }

        #endregion
        #region InventoryJobWorkReceived

        [Authorize, HttpGet]
        public JsonResult GetListForInvOutSourceReceived()
        {
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInventoryPayableService.GetOutSourceReceivedList(identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetInventoryOutSourceReceivedJV(string inveReveiveId, string employeeId, bool isReversCharge, string foc)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            if (foc == "NO")
            {
                
                  return Json(_accountsInventoryPayableService.GetInventoryOutSourceReceivedJV(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
            }
            else
            {
                AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
                return Json(_accountsInvoiceService.GetInventoryPayableFOC(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);

            }
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryOSServiceMasterData(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
                return Json(_accountsInventoryPayableService.GetInventoryOSServiceMasterData(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetInventoryOutSourceGIRI(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetInventoryOutSourceGIRI(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryOutSourceWIP(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetInventoryOutSourceWIP(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetOutSourceInventoryReceivePostedList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetOutSourcePostedList(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOutSourcingVoucherReport(ReportFormat reportFormat, string voucherId, string sourceType)
        {
            AccountsInventoryPayableReportService _accountsInventoryPayableService = new AccountsInventoryPayableReportService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountsInventoryPayableService.GetOutSourcingVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, sourceType);
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


        #region Service Payable
        [Authorize, HttpGet]
        public JsonResult GetListForSvcPayable()
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventoryPayableService.GetListForSvcPayable(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetServicePayable(string serviceAcknowledgementMasterId)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventoryPayableService.GetServicePayable(identity.CompanyId, identity.PlantId, serviceAcknowledgementMasterId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceDetailGL(string serviceAcknowledgementMasterId)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventoryPayableService.GetServiceDetailGL(identity.CompanyId, identity.PlantId, serviceAcknowledgementMasterId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceData(GridParameter parameters, string serviceAcknowledgementMasterId)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            return Json(accountsInventoryPayableService.GetServiceData(parameters, serviceAcknowledgementMasterId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetServiceAdditionalTax(GridParameter parameters, string serviceAcknowledgementMasterId)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            return Json(accountsInventoryPayableService.GetServiceAdditionalTax(parameters, serviceAcknowledgementMasterId), JsonRequestBehavior.AllowGet);

        }

        [HttpPost, Authorize]
        public JsonResult GetServicePostingList(string column, string value)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventoryPayableService.GetServicePostingList(column, value,identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult ServicePabyableJournal(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            var workbook = accountsInventoryPayableReportService.GetServicePayableReportSheet(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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

        #region Issue Journal

        [Authorize, HttpGet]
        public JsonResult GetIssueJournalList(GridParameter parameters)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventoryPayableService.GetIssueJournalList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetIssueReturnJournalList(GridParameter parameters)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventoryPayableService.GetIssueReturnJournalList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialIssueGLList(GridParameter parameters, string issueId)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventoryPayableService.GetIssueMaterialGL(parameters, issueId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialIssueReturnGLList(GridParameter parameters, string issueId)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventoryPayableService.GetIssueReturnMaterialGL(parameters, issueId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult IssueJournalReport(ReportFormat reportFormat, string inventoryIssueId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            var reportFileName = "Inventory Issue Journal";
            var workbook = accountsInventoryPayableReportService.IssueJournal(reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, inventoryIssueId);
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
        [HttpGet, Authorize]
        public ActionResult IssueReturnJournalReport(ReportFormat reportFormat, string inventoryIssueReturnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            var reportFileName = "Inventory Issue Journal";
            var workbook = accountsInventoryPayableReportService.IssueReturnJournalReport(reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, inventoryIssueReturnId);
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
        #region Shortage
        [Authorize]
        public ActionResult InventoryShortagePayable()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryShortage()
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventoryPayableService.GetInventoryShortage(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.InventoryShortagePayable), JsonRequestBehavior.AllowGet);
        }

       

        #endregion

        #region Reject
        [Authorize]
        public ActionResult InventoryRejectPayable()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryReject()
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventoryPayableService.GetInventoryShortage(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.InventoryRejectPayable), JsonRequestBehavior.AllowGet);
        }

        

        #endregion

        #region Inventory Sales Posting

        public ActionResult InventoryReceivable()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult ReceivableJournal(ReportFormat reportFormat, string inventoryReceiveId, string employeeId, bool isReversCharge, bool isFoc,string otherVendorId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            var reportFileName = "GRN";
            var workbook = accountsInventoryPayableReportService.PabyableJournal(identity.CompanyId, identity.PlantId, inventoryReceiveId, employeeId, isReversCharge, isFoc, reportFileName, otherVendorId);
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