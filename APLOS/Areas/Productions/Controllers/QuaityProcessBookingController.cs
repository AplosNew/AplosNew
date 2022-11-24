#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;
using Library.Model.Productions.ProductionBooking;
using Library.Data.Sql;
using Library.OrderManagement.Production;
using System;
using Library.ViewModel.OrderManagements;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class QuaityProcessBookingController : BaseController
    {
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly ISqlRepository _sqlRepository;
        #region Constructor
        /// <summary>   The ProductionSummaryService service. </summary>
        private readonly IProductionSummaryService _ProductionSummaryService;

        public QuaityProcessBookingController(IProductionSummaryService ProductionSummaryService, ISqlRepository sqlRepository)
        {
            _ProductionSummaryService = ProductionSummaryService;
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

       
        #endregion

        #region -- Operations
        
        [HttpGet, Authorize]
        public JsonResult GetQualityProcessCbo(string ProcessId)
        {
            return Json(_productionSummaryData.GetQualityProcessCbo(ProcessId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetQualityProcessParameterList(string ProcessId, string masterId)
        {
            return Json(_productionSummaryData.GetQualityProcessParameterList(ProcessId, masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetList()
        {
            return Json(_productionSummaryData.GetQualityList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetShiftGroupCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetShiftGroupCbo(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionBookingData(string processId, string productionDate, string ProductionShiftId)
        {
            return Json(_productionSummaryData.GetProductionBookingData(processId, productionDate, ProductionShiftId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> ProdBookedSaveList, IEnumerable<QuaityProcessBookingParameterValue> ParameterList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _productionSummaryData.SaveData(data, ProdBookedSaveList, ParameterList);
            return Json(new {Message = AplosMessage.Success });
        }
        
        //[HttpPost]
        //public ActionResult Delete(string id)
        //{
        //    _ProductionSummaryService.DeleteDetail(id);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}
        

        #endregion

        

    }
}