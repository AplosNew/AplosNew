#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.FixedAssets;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.OrderManagement.Packing;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class FinishGoodsBookingController : BaseController
    {


        #region Constructor
        clsFinishGoodsBooking clsFinishGoodsBooking = new clsFinishGoodsBooking();
        private readonly ISqlRepository _sqlRepository;
        public FinishGoodsBookingController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult ConsumptionBook()
        {
            return View();
        }
        public ActionResult FGInventoryPost()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetListByPacking()
        {
            return Json(clsFinishGoodsBooking.GetListByPacking(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetListByProductionBooking()
        {
            return Json(clsFinishGoodsBooking.GetListByProductionBooking(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult FinishGoodsInventoryRegister()
        {
            //return View("~/Areas/Accounts/Views/");
            return View("~/Areas/Productions/Views/FinishGoodsInventoryRegister.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetDetailList(string masterId, string entityId, string processId, string productionOrderId)
        {
            return Json(clsFinishGoodsBooking.GetDetailList(masterId, entityId, processId, productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCostingItemDetailData(string costingId)
        {
            return Json(clsFinishGoodsBooking.GetCostingItemDetailData(costingId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetItemDetailListData(string productionOrderId)
        {
            return Json(clsFinishGoodsBooking.GetItemDetailListData(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCurrentQtyBreakDownData(string productionOrderId, string productCode, string entityId, string fromDate, string toDate)
        {
            return Json(clsFinishGoodsBooking.GetCurrentQtyBreakDownData(productionOrderId, productCode,entityId,fromDate,toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetUnBookedQtyBreakDownData(string productionOrderId, string productCode, string entityId, string fromDate, string toDate)
        {
            return Json(clsFinishGoodsBooking.GetUnBookedQtyBreakDownData(productionOrderId, productCode, entityId, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBookedQtyBreakDownData(string productionOrderId, string productCode, string entityId)
        {
            return Json(clsFinishGoodsBooking.GetBookedQtyBreakDownData(productionOrderId, productCode, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCostingItemData(string productionOrderId)
        {
            return Json(clsFinishGoodsBooking.GetCostingItemData(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> FGList, string ToDate)
        {
            clsFinishGoodsBooking.SaveData(data,FGList, ToDate);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult CreateConsumtionBook(Dictionary<string, object> data, List<Dictionary<string, object>> WorkDayList, List<Dictionary<string, object>> FinishGoodsBookingDetailList)
        {
            clsFinishGoodsBooking.SaveFinishGoodsBookData(data, WorkDayList, FinishGoodsBookingDetailList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public JsonResult GetProcessCbo(string entityId)
        {
            return Json(clsFinishGoodsBooking.GetProcessCbo(entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionOrderDataList(string entityId, string processId)
        {
            return Json(clsFinishGoodsBooking.GetProductionOrderDataList(entityId, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetLineItemData(string entityId, string processId, string productionOrderId, string masterId)
        {
            return Json(clsFinishGoodsBooking.GetLineItemData(entityId, processId, productionOrderId, masterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetBookedAndBalancedData(string productionOrderId)
        {
            return Json(clsFinishGoodsBooking.GetBookedAndBalancedData(productionOrderId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetScanPackingData(string fromDate, string toDate)
        {

            return Json(clsFinishGoodsBooking.GetScanPackingData(fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSavedBookedAndBalancedData(string productionOrderId)
        {
            return Json(clsFinishGoodsBooking.GetSavedBookedAndBalancedData(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetItemDetailData(string masterId)
        {
            return Json(clsFinishGoodsBooking.GetItemDetailData(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFromDate()
        {
            return Json(clsFinishGoodsBooking.GetFromDate(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionBookFromToDate()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsFinishGoodsBooking.GetProductionBookFromToDate(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetItemScanChildData(string entityId,string fromDate, string toDate,string level)
        {
            var jsondata = Json(clsFinishGoodsBooking.GetItemScanChildData(entityId,fromDate, toDate, level), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet, Authorize]
        public JsonResult GetDateWiseDetailDataData(string entityId, string fromDate, string toDate, string POId, string ProductCode)
        {
            var jsondata = Json(clsFinishGoodsBooking.GetDateWiseDetailDataData(entityId, fromDate, toDate, POId, ProductCode), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet, Authorize]
        public JsonResult GetNonPostedProductionSummeryData(string entityId, string processId, string fromDate, string toDate)
        {
            var jsondata = Json(clsFinishGoodsBooking.GetNonPostedProductionSummeryData(entityId, processId, fromDate, toDate), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpGet, Authorize]
        public JsonResult GetDatewiseNonPostedProductionSummeryData(string entityId, string processId, string fromDate, string toDate)
        {
            var jsondata = Json(clsFinishGoodsBooking.GetDatewiseNonPostedProductionSummeryData(entityId, processId, fromDate, toDate), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [Authorize, HttpGet]
        public JsonResult GetListForFinishGoodsBookingPost()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsFinishGoodsBooking.GetListForFinishGoodsBookingPost(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetPostedFinishGoodsBookingData()
        {
            AccountingFinishGoodsService accountingFinishGoodsService = new AccountingFinishGoodsService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountingFinishGoodsService.GetPostedFinishGoodsBookingData(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetFGMaterialDetail(GridParameter parameters, string inventoryReceiveId)
        {
            return Json(clsFinishGoodsBooking.GetFGMaterialDetail(parameters, inventoryReceiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFGJournal(string inventoryReceiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsFinishGoodsBooking.GetFGJournal(identity.CompanyId, inventoryReceiveId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetFGInventoryGLBudgetActivity(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsFinishGoodsBooking.GetFGInventoryGLBudgetActivity(inveReveiveId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult FinishGoodsBookingPost( VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<VoucherDetailViewModel> fGInventoryGLBudgetActivityVMList)
        {
            AccountingFinishGoodsService accountingFinishGoodsService = new AccountingFinishGoodsService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (voucherDetailVMList != null)
            {
                foreach (var item in voucherDetailVMList)
                {
                    if (item.GLGeneralInfoId == null)
                        throw new CustomException("GL is Not Mapped !");
                    if (item.BudgetMasterId == null)
                        throw new CustomException("Budget is Not Mapped !");
                    if (item.ActivityId == null)
                        throw new CustomException("Activity is Not Mapped!");
                }

                if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Dr Cr Amount not equal");
            }
            else
                throw new CustomException("No Journal");

            return Json(new
            {
                Message = string.Format(AplosMessage.VoucherSave, accountingFinishGoodsService.InsertFinishGoodsBookingPosting(voucherVM, voucherDetailVMList, fGInventoryGLBudgetActivityVMList))
            });

        }

        [HttpGet, Authorize]
        public ActionResult FinishGoodsBookingPostReport(ReportFormat reportFormat, string voucherId)
        {
            AccountingFinishGoodsService accountingFinishGoodsService = new AccountingFinishGoodsService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var workbook = accountingFinishGoodsService.FinishGoodsBookingPostReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName, false);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }


        #region FG Inventory Register Report
        [Authorize, HttpPost]
        public JsonResult GetPurchaseRegister(string fromDate, string toDate, string Type)
        {

            DateTime fDate = DateTime.Parse(fromDate);
            DateTime tDate = DateTime.Parse(toDate);
            if (fromDate == null || fromDate == "")
            {
                throw new CustomException("Select From Date");
            }
            else if (toDate == null || toDate == "")
            {
                throw new CustomException("Select To Date");
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(clsFinishGoodsBooking.GetPurchaseRegister(fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }




        [Authorize, HttpGet]
        public ActionResult PurchaseRegisterReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "FG Inventory Register Report" + fromDate + "To" + toDate + "";
            
            var workbook = clsFinishGoodsBooking.CreatePurchaseRegisterReportSheet(identity.CompanyId, plantId, fromDate, toDate, Type);
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



        [Authorize, HttpGet]
        public ActionResult PurchaseRegisterReportExcel(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "FG Inventory Register Report" + fromDate + "To" + toDate + "";
           // Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
            // return Json(obj.CreatePurchaseRegisterReportSheet(identity.CompanyId, plantId, fromDate, toDate, Type), JsonRequestBehavior.AllowGet);
            var workbook = clsFinishGoodsBooking.CreatePurchaseRegisterReportSheetExcel(identity.CompanyId, plantId, fromDate, toDate, Type);
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
        public ActionResult GetFGInventoryRegisterPoPUpListData(string finishGoodsBookingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           // AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);

            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(clsFinishGoodsBooking.GetFGInventoryRegisterPoPUpListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, finishGoodsBookingId), JsonRequestBehavior.AllowGet);

        }

        //[HttpGet, Authorize]
        //public ActionResult PabyableJournal(ReportFormat reportFormat, string inventoryReceiveId, string employeeId, bool isReversCharge, bool isFoc)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var reportFileName = "GRN";
        //    //AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
        //    var workbook = clsFinishGoodsBooking.PabyableJournal(identity.CompanyId, identity.PlantId, inventoryReceiveId, employeeId, isReversCharge, isFoc, reportFileName);
        //    switch (reportFormat)
        //    {
        //        case ReportFormat.Pdf:
        //            return RenderReportAsPdf(workbook, reportFileName);

        //        case ReportFormat.Excel:
        //            return RenderReportAsExcel(workbook, reportFileName);

        //        default:
        //            return View();
        //    }
        //}


        #endregion
    }
}