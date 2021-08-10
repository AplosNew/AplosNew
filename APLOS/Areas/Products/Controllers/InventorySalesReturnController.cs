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
        public JsonResult GetSalesDetailByIssueId(string issueId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            return Json(accountsInventorySalesService.GetSalesDetailDataByIssueId(issueId), JsonRequestBehavior.AllowGet);

        }
        [Authorize, HttpGet]
        public JsonResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            return Json(accountsInventorySalesService.GetInventorySalesReturnData(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(InventorySalesReturn inventoryIssue, IEnumerable<InventorySalesReturnDetailViewModel> entities, IEnumerable<InventorySalesReturnServiceViewModel> salesServiceVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
            inventoryIssue.CompanyId = identity.CompanyId;
            inventoryIssue.PlantId = identity.PlantId;
            _inventoryIssueService.SalesReturnInsert(inventoryIssue, entities, salesServiceVMList);
            return Json(new { inventoryIssue, Message = AplosMessage.Success + "Sales No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
        }


        #endregion
    }
}