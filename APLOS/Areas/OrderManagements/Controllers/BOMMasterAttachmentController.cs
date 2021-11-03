#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Aplos.Controllers;
using System;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using Library.Data.Sql;
using System.Collections.Generic;
using Syncfusion.XlsIO;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class BOMMasterAttachmentController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public BOMMasterAttachmentController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion


        #region Operations
        [HttpPost, Authorize]
        public ActionResult getlist(string column, string value, string Assigned)
        {
            Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
            var jsondata = Json(attchment.LoadAllTemplate(bplib.clsWebLib.GetBoolData(Assigned), column, value), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult searchBOM(string column, string value, string ArticleId, bool loadAll)
        {
            Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();

            var jsondata = Json(attchment.GetBOMList(column, value, ArticleId, loadAll), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost]
        public ActionResult saveAttachment(Dictionary<string, object> Data)
        {
            try
            {
                Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
                attchment.saveAttachment(Data);

                return Json(new { Error = false, Message = "BOM tagged successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {


                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost]
        public ActionResult UnTagAttachment(string ItemId)
        {
            try
            {
                Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
                attchment.UntagAttachment(ItemId);

                return Json(new { Error = false, Message = "BOM un-tagged successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {


                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost]
        public ActionResult ApprovalRequiredQty(string Id, bool Approve)
        {
            try
            {
                Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
                attchment.ApprovalRequireQty(Id, Approve);

                return Json(new { Error = false, Message = "Item approved" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {


                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost]
        public ActionResult ApprovalRequiredQtyMaterial(string MasterOrderItemId, string VendorId, string MaterialMasterId, string ArticleId, bool Approve)
        {
            try
            {
                Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
                attchment.ApprovalRequireQtyMaterial(MasterOrderItemId, VendorId, MaterialMasterId, ArticleId, Approve);

                return Json(new { Error = false, Message = "Item approved" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {


                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost, Authorize]
        public ActionResult UpdateMainMaterialFlag(string MasterOrderItemId, string VendorId, string MaterialMasterId, string ArticleId, bool isMainMaterial)
        {
            try
            {
                Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
                attchment.UpdateMaterialFlag(MasterOrderItemId, VendorId, MaterialMasterId, ArticleId, isMainMaterial);

                return Json(new { Error = false, Message = "Item updated" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {


                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpGet]
        public ActionResult BOMProcess(string MasterOrderItemId)
        {
            try
            {
                //will delete this code after implementing on screen entry facility of BOM
                List<Dictionary<string, object>> OriginalTaggedData = _sqlRepository.GetDataCollection("select * from BOMMasterAttachmentWithItem where MasterOrderItemId='" + MasterOrderItemId + "'");
                Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
                attchment.UntagAttachment(MasterOrderItemId);
                attchment.saveAttachment(OriginalTaggedData[0]);

                Library.OrderManagement.BOM.BOMGeneration BOQ = new Library.OrderManagement.BOM.BOMGeneration();
                BOQ.BOM(MasterOrderItemId);

                return Json(new { Error = false, Message = "BOM processed successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                if (ex.Message.Contains(".xlsx"))
                    return Json(new { Error = true, FileName = ex.Message, Message = "Error in BOM Process, please review the error file" }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { Error = true, FileName = "", Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpGet, Authorize]
        public ActionResult MasterOrderBOMReport(string MasterOrderItemId)// MasterOrderReport
        {

            try
            {
                // if (string.IsNullOrEmpty(MasterLCList))
                //   throw new Exception("Please select at least one master Order");

                Library.OrderManagement.BOM.BOMReports attchment = new Library.OrderManagement.BOM.BOMReports();


                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = attchment.GetMasterOrderReport(MasterOrderItemId, Library.OrderManagement.BOM.BOMReports.BOMLevel.SO);
                IWorkbook workbookItem = attchment.GetMasterOrderReport(MasterOrderItemId, Library.OrderManagement.BOM.BOMReports.BOMLevel.Item);
                attchment.DrawBOMTemplateData(workbookItem.Worksheets[1], MasterOrderItemId);
                attchment.DrawBOMTemplateDataSubMaterial(workbookItem.Worksheets[2], MasterOrderItemId);


                workbook.Worksheets.AddCopy(workbookItem.Worksheets[0]);
                workbook.Worksheets.AddCopy(workbookItem.Worksheets[1]);
                workbook.Worksheets.AddCopy(workbookItem.Worksheets[2]);


                string strFileName = "BOM-" + MasterOrderItemId + ".xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
                workbookItem.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }
        [HttpGet, Authorize]
        public ActionResult LoadBomRequiredQty(string MasterOrderItemId)
        {
            try
            {
                Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();

                var jsondata = Json(new
                {
                    ChildData = attchment.GetBOMItemList(MasterOrderItemId, false),
                    ParentData = attchment.GetBOMItemList(MasterOrderItemId, true),
                    RateData = attchment.GetBOMItemListWithRate(MasterOrderItemId)
                }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {


                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpGet, Authorize]
        public ActionResult LoadBomRequiredQtyChild(string MasterOrderItemId, string ParentId)
        {
            try
            {
                Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();

                var jsondata = Json(attchment.GetBOMItemListChild(MasterOrderItemId, ParentId), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {


                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost]
        public ActionResult UpdateBomRequiredQty(List<Dictionary<string, object>> data)
        {
            try
            {
                Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
                attchment.UpdateBomRequiredQty(data);

                return Json(new { Message = "BOM items updated successfully", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {


                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost]
        public ActionResult UpdateBomRequiredQtyRate(List<Dictionary<string, object>> data)
        {
            try
            {
                Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
                attchment.UpdateBomRequiredQtyRate(data);

                return Json(new { Message = "BOM Rate Updated successfully", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {


                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost, Authorize]
        public ActionResult GetBOMItemListForReport(string MasterOrderItemId)
        {
            Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
            var jsondata = Json(attchment.GetBOMItemListForReportByMasterOrderItemId(MasterOrderItemId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpGet, Authorize]
        public ActionResult GetBOMReport(string ItemIds, string MasterOrderItemId)
        {

            try
            {

                Library.OrderManagement.BOM.TemplateAttchment GetBoMReport = new Library.OrderManagement.BOM.TemplateAttchment();

                GetBoMReport.BOMReport(ItemIds, MasterOrderItemId);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }
        #endregion Operations
    }

}