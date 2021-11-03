#region Using

using Aplos.Controllers;
using System.Web.Mvc;
using Library.HumanResource.Dashboard;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Crosscutting.Security;
using System.Threading;
using OTSBD;
using System.Data;
using Library.Service.Helpers;
using Library.Data.Sql;

#endregion Using

namespace Aplos.Areas.Farming.Controllers
{
    public class FarmingDashboardController : BaseController
    {
        #region Constructor

        FarmingData _farmingData = new FarmingData();
        public FarmingDashboardController(
            )
        {
        }

        #endregion Constructor

        
        public ActionResult Aplos()
        {
            return View();
        }


        [HttpGet, Authorize]
        //The Initial Table with ICS Contents
        public ActionResult getInitialData()
        {
            return Json(_farmingData.getInitialData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getDrillData(string icsGroup)
        {
            return Json(_farmingData.getDrillData(icsGroup), JsonRequestBehavior.AllowGet);
        }
        /*[HttpGet, Authorize]
        public ActionResult getInitialPlannedArea()
        {
            return Json(_farmingData.getInitialPlannedArea(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getInitialTotalArea()
        {
            return Json(_farmingData.getInitialTotalArea(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getGroupFarmers(string icsGroup)
        {
            return Json(_farmingData.getGroupFarmers(icsGroup), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult getGroupPlannedArea(string icsGroup)
        {
            return Json(_farmingData.getGroupPlannedArea(icsGroup), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult getGroupTotalArea(string icsGroup)
        {
            return Json(_farmingData.getGroupTotalArea(icsGroup), JsonRequestBehavior.AllowGet);
        }
        */
        /* [HttpGet, Authorize]
       public ActionResult getFarmersGroupWise(string icsGroup)
        {
            return Json(_farmingData.getFarmersGroupWise(icsGroup), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getFarmersIcsWise(string ics)
        {
            return Json(_farmingData.getFarmersIcsWise(ics), JsonRequestBehavior.AllowGet);
        }*/

        [HttpGet, Authorize]
        public ActionResult getActiveFarmers(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {
            return Json(_farmingData.getActiveFarmers( landId,  cropId,  cropTypeId,  cropCategoryId,  cropSubCategoryId,  column,  groups), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult getInactiveFarmers(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {
            return Json(_farmingData.getInactiveFarmers( landId,  cropId,  cropTypeId,  cropCategoryId,  cropSubCategoryId,  column,  groups), JsonRequestBehavior.AllowGet);
        }

        [HttpGet , Authorize]
        public ActionResult getTotalArea(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {
            return Json(_farmingData.getTotalArea( landId,  cropId,  cropTypeId,  cropCategoryId,  cropSubCategoryId,  column,  groups), JsonRequestBehavior.AllowGet);
        }
        [HttpGet , Authorize]
        public ActionResult getPlannedArea(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {
            return Json(_farmingData.getPlannedArea( landId,  cropId,  cropTypeId,  cropCategoryId,  cropSubCategoryId,  column,  groups), JsonRequestBehavior.AllowGet);
        }


        [HttpGet , Authorize]
        public ActionResult GetFarmerPrintReport(ReportFormat reportFormat, string FarmerMasterPrintId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " Farmer Master " + FarmerMasterPrintId + "";
            var workbook = _farmingData.GetFarmerMasterReportWorkSheet(FarmerMasterPrintId);
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

        [HttpGet, Authorize]
        public ActionResult GetActiveFarmersPrintReport(ReportFormat reportFormat, string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " Active Farmers  " + groups + "";
            var workbook = _farmingData.GetActiveFarmersReport( landId,  cropId,  cropTypeId,  cropCategoryId,  cropSubCategoryId,  column,  groups);
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

        [HttpGet, Authorize]
        public ActionResult GetInactiveFarmersPrintReport(ReportFormat reportFormat, string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " Inactive Farmers  " + groups + "";
            var workbook = _farmingData.GetInactiveFarmersReport(landId, cropId, cropTypeId, cropCategoryId, cropSubCategoryId, column, groups);
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


        [HttpGet, Authorize]
        public ActionResult GetTotalAreaReport(ReportFormat reportFormat, string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " Total Area  " + groups + "";
            var workbook = _farmingData.GetTotalAreaReport(landId, cropId, cropTypeId, cropCategoryId, cropSubCategoryId, column, groups);
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

        [HttpGet, Authorize]
        public ActionResult GetPlannedAreaReport(ReportFormat reportFormat, string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string column, string groups)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " Planned Area  " + groups + "";
            var workbook = _farmingData.GetPlannedAreaReport(landId, cropId, cropTypeId, cropCategoryId, cropSubCategoryId, column, groups);
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

        // -------------------------------- THE DROP DOWN APIs ------------------------------------\\
        [HttpGet, Authorize]
        //The Initial Table with ICS Contents
        public ActionResult getCropType()
        {
            return Json(_farmingData.getCropType(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        //The Initial Table with ICS Contents
        public ActionResult getCropCategory()
        {
            return Json(_farmingData.getCropCategory(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        //The Initial Table with ICS Contents
        public ActionResult getCropSubCategory()
        {
            return Json(_farmingData.getCropSubCategory(), JsonRequestBehavior.AllowGet);
           
        }

        [HttpGet, Authorize]
        //The Initial Table with ICS Contents
        public ActionResult getLand()
        {
            return Json(_farmingData.getLand(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        //The Initial Table with ICS Contents
        public ActionResult getCrop()
        {
            return Json(_farmingData.getCrop(), JsonRequestBehavior.AllowGet);
        }
        // -------------------------------- THE DROP DOWN APIs ENds------------------------------------\\
        
        [HttpGet , Authorize]
        public ActionResult getFilterData(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId)
        {
            return Json(_farmingData.getFilterData(landId, cropId, cropTypeId, cropCategoryId, cropSubCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet , Authorize]
        public ActionResult getFilterDrillData(string landId, string cropId, string cropTypeId, string cropCategoryId, string cropSubCategoryId, string icsGroup)
        {
            return Json(_farmingData.getFilterDrillData( landId,  cropId,  cropTypeId,  cropCategoryId,  cropSubCategoryId,  icsGroup) , JsonRequestBehavior.AllowGet);
        }

        [HttpGet , Authorize]
        public ActionResult IcsPie()
        {
            return Json(_farmingData.IcsPie() , JsonRequestBehavior.AllowGet);
        }
    }
}