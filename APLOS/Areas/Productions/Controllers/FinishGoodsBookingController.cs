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
        public ActionResult ConsumptionBookPost()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetList()
        {
            return Json(clsFinishGoodsBooking.GetList(), JsonRequestBehavior.AllowGet);
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
        public JsonResult GetCostingItemData(string productionOrderId)
        {
            return Json(clsFinishGoodsBooking.GetCostingItemData(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> WorkDayList, List<Dictionary<string, object>> FinishGoodsBookingDetailList)
        {
            clsFinishGoodsBooking.SaveData(data, WorkDayList, FinishGoodsBookingDetailList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult CreateConsumtionBook(Dictionary<string, object> data, List<Dictionary<string, object>> WorkDayList, List<Dictionary<string, object>> FinishGoodsBookingDetailList)
        {
            clsFinishGoodsBooking.ConsumtionBookData(data, WorkDayList, FinishGoodsBookingDetailList);
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
        public JsonResult GetItemScanChildData(string entityId,string fromDate, string toDate)
        {
            var jsondata = Json(clsFinishGoodsBooking.GetItemScanChildData(entityId,fromDate, toDate), JsonRequestBehavior.AllowGet);
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
        public JsonResult GetFGMaterialDetail(GridParameter parameters, string dateWiseConsumptionId)
        {
            return Json(clsFinishGoodsBooking.GetFGMaterialDetail(parameters, dateWiseConsumptionId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFGJournal(string dateWiseConsumptionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsFinishGoodsBooking.GetFGJournal(identity.CompanyId, dateWiseConsumptionId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetVendorPayableGLBudgetActivity(string inveReveiveId, string companypartyAccountGroupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsFinishGoodsBooking.GetVendorPayableGLBudgetActivity(inveReveiveId, identity.CompanyId, identity.PlantId, companypartyAccountGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult FinishGoodsBookingPost( VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList )
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
                Message = string.Format(AplosMessage.VoucherSave, accountingFinishGoodsService.InsertFinishGoodsBookingPosting(voucherVM, voucherDetailVMList))
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

    }
}