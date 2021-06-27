using Aplos.Controllers;
using Library.Service.OrderManagements;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System.Data;
using Syncfusion.XlsIO;
using OTSBD;
using System.Linq;
using Library.Service.Enums;
using Library.Service.Helpers;
using bplib;



using System.Web.Hosting;
using Library.Service.Productions.ProductionBooking;
using System.Text.RegularExpressions;


namespace Aplos.Areas.OrderManagements.Controllers
{
    public class OrderReportController : BaseController
    {
        public enum PlanningStatus { TOSTART, FREEZE, RUNNING };
        private EnumPlanningTypes ScreenPlanningType = EnumPlanningTypes.PlanningType1;
        
        #region Constructor

        private readonly IProductionOrderService _productionOrderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ProductionOrderReports ProductionOrderReports = null;

        public OrderReportController(IProductionOrderService productionOrderService, IUnitOfWork U, ISqlRepository R)
        {

            _unitOfWork = U;
            _sqlRepository = R;
            _productionOrderService = productionOrderService;
            ProductionOrderReports = new ProductionOrderReports(_sqlRepository);
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
        public ActionResult GetControlChartReportXls()
        {

            try
            {

                Library.Service.Extension.OrderControl.MailSenderService OCP = new Library.Service.Extension.OrderControl.MailSenderService();
               

                IWorkbook workbook = OCP.ControlChartReportXls(); 

                string strFileName = "Control 1.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
            return null;

        }



        #endregion


    }

}