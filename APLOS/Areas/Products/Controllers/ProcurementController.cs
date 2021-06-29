
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.Model.Parties;
using Library.MaterialManagement.Inventory;
using Library.MaterialManagement.Reports;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
namespace Aplos.Areas.Products.Controllers
{
    public class ProcurementController : BaseController
    {
        #region Constructor

        private readonly IPurchaseOrderService _inventoryReveiveService;
        private readonly IProcurementMasterService _procurementMasterService;
        private readonly IProcurementMasterDetailService _procurementMasterDetailService;
        private readonly IMaterialRequsitionDetailsServiceService _materialRequsitionDetailsServiceService;
        private readonly IMaterialRequsitionMasterServiceService _materialRequsitionMasterServiceService;
        private readonly IPurchaseOrderDetailService _inventoryDetailService;
        private readonly IPOMaterialService _inventoryMaterialService;
        private readonly IPurchaseOrderServiceService _inventoryService;
        private readonly IInventoryReceiveReportService _inventoryReportService;
        private readonly ISqlRepository _sqlRepository;

        public ProcurementController(
             IPurchaseOrderService inventoryReveiveService
            , IProcurementMasterService procurementMasterService
            , IProcurementMasterDetailService procurementMasterMasterService
            , IMaterialRequsitionDetailsServiceService materialRequsitionDetailsServiceService
            , IPurchaseOrderDetailService inventoryDetailService
            , IPOMaterialService inventoryMaterialService
            , IInventoryReceiveReportService inventoryReportService
            , IPurchaseOrderServiceService inventoryService
            , IMaterialRequsitionMasterServiceService materialRequsitionMasterServiceService
            , ISqlRepository sqlRepository)
        {
            _procurementMasterDetailService = procurementMasterMasterService;
            _inventoryReveiveService = inventoryReveiveService;
            _inventoryDetailService = inventoryDetailService;
            _inventoryMaterialService = inventoryMaterialService;
            _inventoryService = inventoryService;
            _inventoryReportService = inventoryReportService;
            _sqlRepository = sqlRepository;
            _materialRequsitionDetailsServiceService = materialRequsitionDetailsServiceService;
            _materialRequsitionMasterServiceService = materialRequsitionMasterServiceService;
            _procurementMasterService = procurementMasterService;
        
        }

        #endregion Constructor

        #region Aplos
        
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Aplos

        [HttpPost]
        public JsonResult Create(ProcurementMaster entity)
        {
            //try
            //{
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.CompanyGroupId = identity.CompanyGroupId;
                entity.CompanyId = identity.CompanyId;
                entity.PlantId = identity.PlantId;


                if (entity.ProcurementFrequency == "Dalily")
                {
                    if (entity.ProcurementDays > 1)
                    {
                        throw new CustomException("Enter Procurement days 1");
                    }
                    else if (entity.ProcurementDays == 0)
                    {
                        throw new CustomException("Enter Procurement days 1");
                    }
                }
                else if (entity.ProcurementFrequency == "Weekly")
                {
                    if (entity.ProcurementDays > 7)
                    {
                        throw new CustomException("Enter Procurement Weekly 7");
                    }
                    else if (entity.ProcurementDays == 0)
                    {
                        throw new CustomException("Enter Procurement Weekly 7");
                    }
                }


                else if (entity.ProcurementFrequency == "Bi-Weekly")
                {
                    if (entity.ProcurementDays > 14)
                    {
                        throw new CustomException("Enter Procurement Bi-Weekly 14");
                    }
                    else if (entity.ProcurementDays == 0)
                    {
                        throw new CustomException("Enter Procurement Bi-Weekly 14");
                    }
                }

                else if (entity.ProcurementFrequency == "Monthly")
                {
                    if (entity.ProcurementDays > 30)
                    {
                        throw new CustomException("Enter Procurement Monthly 30");
                    }
                    else if (entity.ProcurementDays == 0)
                    {
                        throw new CustomException("Enter Procurement Bi-Weekly 30");
                    }
                }
                else if (entity.ProcurementFrequency == "Quartely")
                {
                    if (entity.ProcurementDays > 90)
                    {
                        throw new CustomException("Enter Procurement Quartely 90");
                    }
                    else if (entity.ProcurementDays == 0)
                    {
                        throw new CustomException("Enter Procurement Quartely 90");
                    }
                }

                else if (entity.ProcurementFrequency == "Bi-Annualy")
                {
                    if (entity.ProcurementDays > 180)
                    {
                        throw new CustomException("Enter Procurement Bi-Annualy 180");
                    }
                    else if (entity.ProcurementDays == 0)
                    {
                        throw new CustomException("Enter Procurement Bi-Annualy 180");
                    }
                }

                else if (entity.ProcurementFrequency == "Annualy")
                {
                    if (entity.ProcurementDays > 180)
                    {
                        throw new CustomException("Enter Procurement Annualy 180");
                    }
                    else if (entity.ProcurementDays == 0)
                    {
                        throw new CustomException("Enter Procurement Bi-Annualy 180");
                    }
                }

                else if (entity.ProcurementFrequency == "Annualy")
                {
                    if (entity.ProcurementDays > 365)
                    {
                        throw new CustomException("Enter Procurement Annualy 365");
                    }
                    else if (entity.ProcurementDays == 0)
                    {
                        throw new CustomException("Enter Procurement Bi-Annualy 365");
                    }
                }

                _procurementMasterService.Insert(entity);
            //}
            //catch (Exception ex)
            //{
            //    //throw new CustomException("This Material Already Exist");
            //}
            return Json(new { ProcurementMaster = entity, entity.Id, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult DetailCreate(ProcurementMasterDetail entity)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (entity.PartyBaseRate == 0)
            {
                throw new CustomException("Enter Party Base RAte");

            }
            //entity.CompanyGroupId = identity.CompanyGroupId;
            //entity.CompanyId = identity.CompanyId;
            //entity.PlantId = identity.PlantId;
                _procurementMasterDetailService.Insert(entity);
            return Json(new { ProcurementDetail = entity, entity.Id, Message = AplosMessage.Insert });
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialList(string materialId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_procurementMasterDetailService.GetProcurementMasterDetailsByMasterId(materialId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Edit(ProcurementMaster entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
         _procurementMasterService.Update(entity);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _procurementMasterService.DeleteReq(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }



        [Authorize, HttpPost]


        public ActionResult DetailDelete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _procurementMasterDetailService.DetailDeleteReq(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }


        [HttpGet]
        public JsonResult GetDataByProcurementMasterId()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_procurementMasterService.GetDataByProcurementMasterId(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMaterialTypeCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_procurementMasterService.GetMaterialTypeCbo(), JsonRequestBehavior.AllowGet);
        }


        //QualityStdSet


        public JsonResult GetQualityStdCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_procurementMasterService.GetQualityStdCbo(), JsonRequestBehavior.AllowGet);
        }




        #region ProcurementMaster Reports


        [HttpGet, Authorize]
        public ActionResult ProcurementMasterReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Procurement Master "+DateTime.Now.ToString("MM/dd/yyyy");
            var workbook = _procurementMasterService.CreateProcurementMasterReportSheet(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount);
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


















