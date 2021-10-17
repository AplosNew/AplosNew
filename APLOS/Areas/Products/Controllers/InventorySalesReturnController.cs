using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.Data.Sql;
using Library.Accounting.Accounts;
using System;
using Library.MaterialManagement.Inventory;
using Library.Model.Inventory;
using Library.ViewModel.SalesManagements;

namespace Aplos.Areas.Products.Controllers
{
    public class InventorySalesReturnController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly IInventoryIssueService _inventoryIssueService;
        public InventorySalesReturnController(ISqlRepository sqlRepository,IInventoryIssueService inventoryIssueService)
        {
            _sqlRepository = sqlRepository;
            _inventoryIssueService = inventoryIssueService;
        }

        #region Inventory Sales Posting
        
        public ActionResult Aplos()
        {
            return View();
        }
      
        [Authorize, HttpGet]
        public JsonResult GetInventorySaleDetailGLList(string inventorySalesId, string customerId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventorySalesService.GetInventorySaleDetailGLListData(identity.CompanyId, identity.PlantId, inventorySalesId, customerId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalesDetailDataBySales(string inventorySalesId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            return Json(accountsInventorySalesService.GetSalesDetailDataBySales(inventorySalesId), JsonRequestBehavior.AllowGet);

        }
        [Authorize, HttpGet]
        public JsonResult GetTaxInfoRowWise(string inventorySalesId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            return Json(accountsInventorySalesService.GetTaxInfoRowWise(inventorySalesId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetSalesDetailDataForUpdateReturn(string salesReturnId, string inventorySalesId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            return Json(accountsInventorySalesService.GetSalesDetailDataForUpdateReturn(salesReturnId,inventorySalesId), JsonRequestBehavior.AllowGet);

        }
        [Authorize, HttpGet]
        public JsonResult GetTaxForUpdateSalesReturn(string salesReturnId, string inventorySalesId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            return Json(accountsInventorySalesService.GetTaxForUpdateSalesReturn(salesReturnId,inventorySalesId), JsonRequestBehavior.AllowGet);

        }
        [Authorize, HttpGet]
        public JsonResult GetTaxInfo(string inventorySalesId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            return Json(accountsInventorySalesService.GetTaxInfo(inventorySalesId), JsonRequestBehavior.AllowGet);

        }
        [Authorize, HttpGet]
        public JsonResult GetServiceChargeList(string inventorySalesId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            return Json(accountsInventorySalesService.GetServiceChargeList(inventorySalesId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetServiceChargeForUpdateList(string salesReturnId, string inventorySalesId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            return Json(accountsInventorySalesService.GetServiceChargeForUpdateList(salesReturnId,inventorySalesId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetServiceTaxList(string inventorySalesId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            return Json(accountsInventorySalesService.GetServiceTaxList(inventorySalesId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetServiceTaxForUpdate(string salesReturnId, string inventorySalesId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            return Json(accountsInventorySalesService.GetServiceTaxForUpdate(salesReturnId,inventorySalesId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            return Json(accountsInventorySalesService.GetInventorySalesReturnData(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(InventorySalesReturn inventoryIssue, IEnumerable<InventorySalesReturnDetailViewModel> entities,IEnumerable<SalesReturnTaxViewModel> salesReturnTaxList, IEnumerable<InventorySalesReturnServiceViewModel> salesServiceVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
            inventoryIssue.CompanyId = identity.CompanyId;
            inventoryIssue.PlantId = identity.PlantId;
            _inventoryIssueService.SalesReturnInsert(inventoryIssue, entities, salesReturnTaxList, salesServiceVMList);
            return Json(new { inventoryIssue, Message = AplosMessage.Success + "Sales No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(InventorySalesReturn inventoryIssue, IEnumerable<InventorySalesReturnDetailViewModel> entities, IEnumerable<SalesReturnTaxViewModel> salesReturnTaxList, IEnumerable<InventorySalesReturnServiceViewModel> salesServiceVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
            inventoryIssue.CompanyId = identity.CompanyId;
            inventoryIssue.PlantId = identity.PlantId;
            _inventoryIssueService.SalesReturnInsert(inventoryIssue, entities, salesReturnTaxList, salesServiceVMList);
            return Json(new { inventoryIssue, Message = AplosMessage.Success + "Sales No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}