using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Productions.SalesOrderInvoice;
using Library.Service.Invoices;
using Library.Service.Payments;
using Library.Service.Processes;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.OrderManagements.Controllers
{
	public class SalesOrderInvoiceController : BaseController
    {
        #region Constructor

        private readonly IProcessService _processService;
        private readonly ISalesOrderInvoiceMasterService _soim;
        private readonly ISalesOrderInvoiceDetailService _pld;
        private readonly ISalesOrderInvoicePackingListService _ipl;
        private readonly IPaymentTermService _pts;

        public SalesOrderInvoiceController(
            ISalesOrderInvoiceMasterService salesorderpackinglistmasterservice,
            ISalesOrderInvoicePackingListService ipl,
            IProcessService processservice,
            IPaymentTermService pts,
            ISalesOrderInvoiceDetailService pld)
        {
            _soim = salesorderpackinglistmasterservice;
            _processService = processservice;
            _pld = pld;
            _ipl = ipl;
            _pts = pts;
        }

        #endregion Constructor

        #region -- Pages

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        [Authorize]
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize, HttpGet]
        public ActionResult Getplsearch(GridParameter gridparameter, string EntityId, string customerid, string plantid)
        {
            return Json(_pld.GetPLHeadSearch(gridparameter, EntityId, customerid, plantid), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerSearchData(GridParameter gridparameter, string sorgid)
        {
            //ICustomerCompanyDataService
            //var customerSearchData = _ccds.GetCustomerSearchData(gridparameter, sorgid);
            return Json(new { /*customerSearchData,*/ Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult LoadCustomerPaymentTerm()
        {
            return Json(_pts.GetCustomerCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult Getsalestype()
        {
            return Json(_soim.GetSalesType(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult Getmasterinfo(GridParameter gridparameter, string plantid,string entityId)
        {
            return Json(_soim.GetMasterList(gridparameter, plantid, entityId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetBaseLineDateSetting(string PaymentTermId)
        {
            return Json(_soim.GetBaseLineDateSetting(PaymentTermId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult Get_PL_Material_View_SetQty(string packmasterid)
        {
            return Json(JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult Get_Invoiced_Material_Edit_SetQty(string ipackmasterid)
        {
            return Json(_pld.Get_Invoiced_Material_Edit_SetQty(ipackmasterid), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetInvoicePackingListHead(string masterid)
        {
            return Json(_ipl.GetInvoicePackingListHead(masterid), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetInvoiceMaster(string id)
        {
            return Json(_soim.GetInvoiceMaster(id), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public ActionResult getfileinfo(GridParameter gridparameter, string entityid)
        //{
        //    return Json(_salesorderpackinglistmasterservice.GetFileInfo(gridparameter, entityid), JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public JsonResult Create(SalesOrderInvoiceMaster master)
        {
            string masterid = string.Empty;
            _soim.SaveMaster(master, out masterid);
            return Json(new { id = masterid, Message = AplosMessage.Insert });
        }

        [HttpPost, ChaildAction(ParentActionName = "Create")]
        public JsonResult CreateDetail(string masterid, SalesOrderInvoicePackingList packing, SalesOrderInvoiceDetail detail)
        {
            //_ipl.SaveDetailList(masterid, detail, pldlist);
            _soim.SaveDetailList(masterid, packing, detail);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string masterid)
        {
            _soim.DeleteMaster(masterid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, ChaildAction(ParentActionName = "Delete")]
        public JsonResult DeleteDetailSingle(string masterid, string detailid)
        {
            _soim.DeleteDetailSingle(masterid, detailid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}