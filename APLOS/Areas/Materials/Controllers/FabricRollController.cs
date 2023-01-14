#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Materials;
using Library.Service.Enums;
using Library.Service.Materials;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.Data.Sql;
using System.Web.Script.Serialization;
using System;
using Newtonsoft.Json;
using Library.Data;
using System.IO;
using Library.HumanResource.Attendance.Manual;
using Library.Service.Helpers;
using System.Data;
using Library.OrderManagement.FabricRollClass;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using OTSBD;
using Library.Service.HumanResources.Profile;
using Library.MaterialManagement.Material;
using Library.Service.Systems;

#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class FabricRollController : BaseController
    {
        #region -- Constructor

        private readonly IFabricRollMasterService _fabricRollMasterService;
        private SqlRepository _sqlRepository = new SqlRepository();
        FabricRollClass clsFabric = new FabricRollClass();
        public FabricRollController(IFabricRollMasterService fabricRollMasterService)
        {
            _fabricRollMasterService = fabricRollMasterService;
        }

        #endregion -- Constructor

        #region Pages


        public ActionResult Aplos_()
        {
            return View();
        }

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetFromToDate()
        {
            string sql = @"SELECT FORMAT(MIN(A.AddedDate),'dd-MMM-yyyy') FromDate,FORMAT(MAX(A.AddedDate),'dd-MMM-yyyy') ToDate FROM TRN.InventoryReceive A WHERE A.GRNType in('GRNBYPO','GRN' ,'EMPGRN')";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string paidHours)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fabricRollMasterService.Query(parameters, identity.CompanyGroupId, paidHours, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFabricIncrementValue()
        {
            return Json(_fabricRollMasterService.InsertOrUpdateGraphIncrement(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult Create(IEnumerable<FabricRollMaster> entities)
        {
            _fabricRollMasterService.InsertOrUpdateGraph(entities);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Update(List<Dictionary<string, object>> FabricRollData, string PackingForm)
        {
            _fabricRollMasterService.UpdateFabricRoll(FabricRollData, PackingForm);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult GetRoll(int NoofRolls, Dictionary<string, object> SelectedRow, double Width, string PackingForm)
        {
            _fabricRollMasterService.CreateRoll(NoofRolls, SelectedRow, Width, PackingForm);
            return Json(new { Message = AplosMessage.Insert });
        }

        public ActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TRN.FabricRollMaster where id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false,/* Sequence = GetSequence(),*/ Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public JsonResult GetGRNList(GridParameter parameters)
        {
            return Json(_fabricRollMasterService.GetGRNList(parameters, BusinessProcessEnum.FabricRollManagement.ToString()), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetGRNDetailList(GridParameter parameters, string inventoryReceiveId)
        {
            return Json(_fabricRollMasterService.GetGRNDetailList(parameters, inventoryReceiveId, BusinessProcessEnum.FabricRollManagement.ToString()), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetFABRollList(GridParameter parameters, string inventoryReceiveDetailId)
        {
            return Json(_fabricRollMasterService.GetFABRollList(parameters, inventoryReceiveDetailId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetBarCideList(string inventoryReceiveDetailId)
        {
            return Json(_fabricRollMasterService.GetBarCideList(inventoryReceiveDetailId), JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations

        [HttpPost, Authorize]
        public ActionResult GRNList(string column, string value, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsFabric.GRNList(column, value, fromDate, toDate,identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult MaterialList(string inventoryReceiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsFabric.MaterialList(inventoryReceiveId), JsonRequestBehavior.AllowGet);

        }

        [HttpPost, Authorize]
        public ActionResult GetMaterialListData(string inventoryReceiveId)
        {
            return Json(clsFabric.GetMaterialListData(inventoryReceiveId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult FabricRollList(string inventoryReceiveDetailId)
        {
            return Json(clsFabric.FabricRollList(inventoryReceiveDetailId), JsonRequestBehavior.AllowGet);
        }

       

        [HttpGet, Authorize]
        public ActionResult DownloadRollReport(string inventoryReceiveDetailId)
        {
            try
            {
                clsFabric.DownloadReport(inventoryReceiveDetailId);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        #region Upload Roll Data

        [HttpPost, Authorize]
        public JsonResult CreateRollFile(FormCollection form)
        {
            var pre = form["FabricRollFile"];
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var FabricRollFile = JsonConvert.DeserializeObject<FabricRollFile>(pre, settings);
            var file = Request.Files["file"];
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() != ".xls" && extension.ToLower() != ".xlsx")
                {
                    throw new CustomException(Resources.ImageUploadError);
                }


                FabricRollClass Clsss = new FabricRollClass();
                //clsManualAttendanceFileUpload p = new clsManualAttendanceFileUpload();
                Clsss.Save(file.FileName, extension, FabricRollFile, out DataSet dsMaster);
                var path = Path.Combine(ResourcesPathReader.GetFabricRollFilePath(), dsMaster.Tables[0].Rows[0]["FileId"].ToString());

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                {
                    file.SaveAs(path);
                }
            }
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetMaster()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsManualAttendanceFileUpload ep = new clsManualAttendanceFileUpload();
                return Json(ep.GetMaster(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        #endregion
        public void SaveFile(out string path)
        {
            path = "";
            try
            {
                var file = Request.Files["file"];
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {
                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetFabricRollData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region SampleFile
        [HttpPost, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat, List<Dictionary<string, object>> GridTempList, Dictionary<string, object> fabricRollMaster)
        {
            string fileName = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            fileName = GetSampleFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, GridTempList, fabricRollMaster);
            var reportFileName = "Fabric Roll Management Template";
            return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            //switch (reportFormat)
            //{
            //    case ReportFormat.Pdf:
            //        return RenderReportAsPdf(workbook, reportFileName);

            //    case ReportFormat.Excel:
            //        return RenderReportAsExcel(workbook, reportFileName);

            //    default:
            //        return RenderReportAsExcel(workbook, reportFileName);
            //}

        }

        public string GetSampleFile(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, List<Dictionary<string, object>> GridTempList, Dictionary<string, object> fabricRollMaster)
        {
            #region declare
            clsReport objRpt = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;

            int maxRow = 5001;

            #endregion

            try
            {
                //sorting
                //lock               
                var filePath = "";
                ReportUtility ru = new ReportUtility();

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = ru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);


                #region Lunch Out
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                IWorksheet sheetSource = null;
                sheetSource = workbook.Worksheets[1];

                int xlsRow = 1, xlsCol = 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GRNNo"); xlsCol += 1;
                sheet1[xlsRow, xlsCol].Text = fabricRollMaster["GRNNo"].ToString();
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GRN Date"); xlsCol += 1;
                if (fabricRollMaster["GRNDate"] != null)
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["GRNDate"].ToString();
                }
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Amount"); xlsCol++;
                sheet1[xlsRow, xlsCol].Text = fabricRollMaster["TransactionAmount"].ToString() + " " + fabricRollMaster["CurrencyCode"].ToString();
                xlsCol += 1;

                xlsRow++; xlsCol = 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PO No"); xlsCol++;
                //sheet1[xlsRow, xlsCol].Text = fabricRollMaster["POId"].ToString();
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["POId"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["POId"].ToString();
                }
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PO Date"); xlsCol++;
                //sheet1[xlsRow, xlsCol].Text = clsStaticInfo.SetDate(fabricRollMaster["PODate"].ToString());
                if (fabricRollMaster["PODate"] != null)
                {
                    clsStaticInfo.SetDate(sheet1[xlsRow, xlsCol], Convert.ToDateTime(fabricRollMaster["PODate"]).ToString("dd-MMM-yyyy"));
                }
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Vendor Ref No"); xlsCol++;
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["VendorRefNo"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["VendorRefNo"].ToString();
                }
                xlsCol += 1;

                xlsRow++; xlsCol = 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LC No"); xlsCol++;
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["PurchaseLCNo"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["PurchaseLCNo"].ToString();
                }
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LC Date"); xlsCol++;
                if (fabricRollMaster["LCDate"] != null)
                {
                    clsStaticInfo.SetDate(sheet1[xlsRow, xlsCol], Convert.ToDateTime(fabricRollMaster["LCDate"]).ToString("dd-MMM-yyyy"));
                }

                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PI No");
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["PINo"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["PINo"].ToString();
                }
                xlsCol += 1;

                xlsRow++; xlsCol = 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Vendor"); xlsCol++;
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["PartyName"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["PartyName"].ToString();
                }
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Opening Bank"); xlsCol++;
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["OpeningBank"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["OpeningBank"].ToString();
                }
                xlsCol += 1;

                xlsRow = 6; xlsCol = 1;
                int endXlsCol = 1;

                #region ------------------Column Header------------------

                int colSeq = 0; int colGRNRowId = 0; int colLotNo = 0; int colShade = 0; int colMarkarCode = 0; int colFabricGroup = 0; int colLength = 0;
                int colWeight = 0; int colShrinkage = 0; int colQty = 0; int colQtyUoM = 0; int colActualQty = 0; int colInvoiceQty = 0;
                int colSupplierRollNo = 0; int colOwnRollNo = 0; int colBuyerRollNo = 0; int colGrouping = 0; int colRemarks = 0;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sequence");
                colSeq = xlsCol;
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@";
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GRNRowId");
                colGRNRowId = xlsCol;
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@";
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LotNo");
                colLotNo = xlsCol;
                //sheet1.Range[xlsRow + 1, xlsCol, maxRow, xlsCol].NumberFormat = "@";
                xlsCol += 1;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shade"); colShade = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "MarkarCode"); colMarkarCode = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FabricGroup"); colFabricGroup = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Length"); colLength = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Weight"); colWeight = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shrinkage"); colShrinkage = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Qty"); colQty = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "QtyUoM"); colQtyUoM = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ActualQty"); colActualQty = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "InvoiceQty"); colInvoiceQty = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SupplierRollNo"); colSupplierRollNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OwnRollNo"); colOwnRollNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BuyerRollNo"); colBuyerRollNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Grouping"); colGrouping = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarks"); colRemarks = xlsCol;


                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------
                int count = 0;
                #region DataPlot
                string grnId = string.Empty;

                foreach (var item in GridTempList)
                {
                    if (grnId == item["Id"].ToString())
                    {
                        for (int i = 0; i < Convert.ToInt32(item["RollNo"].ToString()); i++)
                        {
                            count++;
                            sheet1[xlsRow, 1].Number = count;
                            sheet1[xlsRow, 2].Text = item["Id"].ToString();

                            grnId = item["Id"].ToString();

                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Length";
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Weight";
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Qty";
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for ActualQty";
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for InvoiceQty";
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colLotNo, xlsRow, colLotNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShade, xlsRow, colShade].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colMarkarCode, xlsRow, colMarkarCode].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFabricGroup, xlsRow, colFabricGroup].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShrinkage, xlsRow, colShrinkage].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colQtyUoM, xlsRow, colQtyUoM].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colSupplierRollNo, xlsRow, colSupplierRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colOwnRollNo, xlsRow, colOwnRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colBuyerRollNo, xlsRow, colBuyerRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colGrouping, xlsRow, colGrouping].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colRemarks, xlsRow, colRemarks].CellStyle.Locked = false;

                            xlsRow++;




                        }

                    }
                    else
                    {

                        for (int i = 0; i < Convert.ToInt32(item["RollNo"].ToString()); i++)
                        {
                            count++;
                            sheet1[xlsRow, 1].Number = count;
                            sheet1[xlsRow, 2].Text = item["Id"].ToString();

                            grnId = item["Id"].ToString();

                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Length";
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Weight";
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Qty";
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for ActualQty";
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for InvoiceQty";
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colLotNo, xlsRow, colLotNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShade, xlsRow, colShade].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colMarkarCode, xlsRow, colMarkarCode].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFabricGroup, xlsRow, colFabricGroup].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colLength, xlsRow, colLength].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colWeight, xlsRow, colWeight].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShrinkage, xlsRow, colShrinkage].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colQty, xlsRow, colQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colQtyUoM, xlsRow, colQtyUoM].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colInvoiceQty, xlsRow, colInvoiceQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colSupplierRollNo, xlsRow, colSupplierRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colOwnRollNo, xlsRow, colOwnRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colBuyerRollNo, xlsRow, colBuyerRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colGrouping, xlsRow, colGrouping].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colRemarks, xlsRow, colRemarks].CellStyle.Locked = false;

                            xlsRow++;
                        }

                    }


                }


                xlsRow++;

                #endregion

                #region UsedRange Alignment

                sheet1.Protect(bplib.clsWebLib.REPORT_LOCK_PASSWORD, ExcelSheetProtection.Filtering | ExcelSheetProtection.All);
                workbook.Worksheets[1].Protect(bplib.clsWebLib.REPORT_LOCK_PASSWORD);
                workbook.Protect(false, true, bplib.clsWebLib.REPORT_LOCK_PASSWORD);

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 10;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Sheet1";
                #endregion Page Setup

                //sheetSource.Protect("2020", ExcelSheetProtection.Content);


                #endregion  Lunch Out

                //return workbook;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FabricRollManage" + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region MyRegion
        [HttpGet, Authorize]
        public JsonResult GetSavedList(string GRNId)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(clsFabric.GetSavedList(GRNId,identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetFabricRollChildList(string FabricRollManagementMasterId)
        {
            return Json(clsFabric.GetFabricRollChildList(FabricRollManagementMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult ImportData()
        {
            string path;
            clsTemplateReadProfile objR = null;
            try
            {
                objR = new clsTemplateReadProfile();
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFiles(out path);
                var data = ReadData(identity.PlantId, path);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public void SaveFiles(out string path)
        {
            path = "";
            try
            {
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //GetPlantwiseData(identity.PlantId);
                var file = Request.Files["file"];
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {
                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<FabricRollTemplate> ReadData(string plantid, string path)
        {
            List<FabricRollTemplate> data = null;
            //string path = "";
            DataSet dsExcel = null;
            try
            {
                data = new List<FabricRollTemplate>();
                //SaveFile(out path);
                ReadFile(path, out dsExcel);
                Validation(dsExcel, plantid);
                data = dsExcel.Tables[0].ToList<FabricRollTemplate>();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ReadFile(string path, out DataSet dsExcel)
        {
            FileInfo docFile;
            dsExcel = null;
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = excelEngine.Excel.Workbooks.Open(path);
                //DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                DataTable dt = workbook.Worksheets[0].ExportDataTable(6, 1, 5000, 18, ExcelExportDataTableOptions.ColumnNames);
                dt.DefaultView.RowFilter = "isnull(Sequence,'')<>''";
                dt = dt.DefaultView.ToTable();
                //var pquom = "";
                //var quomid = "";
                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    if (pquom != dt.Rows[i]["QtyUoM"].ToString())
                //    {
                //        pquom = dt.Rows[i]["QtyUoM"].ToString();
                //        var sqlProcess = @"select Id QtyUoMId from SCS.UnitOfMeasurement Where UserName='"+ pquom + "'";
                //        DataTable dtProcess = _sqlRepository.GetDataTable(sqlProcess);
                //        quomid = dtProcess.Rows[0]["QtyUoMId"].ToString();
                //    }
                //        Rows.Add(
                //}
                dsExcel = new DataSet();
                dsExcel.Tables.Add(dt);
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    //exception += "\r\nTrying to delete";
                    docFile.Delete();
                }
            }
            catch (Exception ex)
            {
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
                throw (ex);
            }
        }

        public void Validation(DataSet dsExcel, string plantid)
        {

            try
            {

                if (dsExcel.Tables[0].Rows.Count > 0)
                {
                    if (false)
                    {
                        for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                        {
                            string strTempPDate = "";
                            string strTempPTimee = "";
                            string strTempPType = "";

                            strTempPDate = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                            strTempPTimee = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                            strTempPType = dsExcel.Tables[0].Rows[i][3].ToString().Trim().ToUpper();

                        }//for

                    }

                }
                else
                {
                    throw new Exception("Please Select File");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult CreateFabricRollManage(Dictionary<string, object> data, List<Dictionary<string, object>> grnDetailList)
        {
            clsFabric.SaveFabricRollManageData(data, grnDetailList, out string masterId);
            data["Id"] = masterId;
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

       

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        #endregion

    }


    public class FabricRollTemplate
    {

        public string Sequence { get; set; }
        public string GRNRowId { get; set; }
        public string LotNo { get; set; }
        public string Shade { get; set; }
        public string MarkarCode { get; set; }
        public string FabricGroup { get; set; }
        public string Length { get; set; }

        public string Weight { get; set; }
        public string Shrinkage { get; set; }
        public string Qty { get; set; }
        public string QtyUoM { get; set; }
        public string ActualQty { get; set; }
        public string InvoiceQty { get; set; }

        public string SupplierRollNo { get; set; }
        public string OwnRollNo { get; set; }
        public string BuyerRollNo { get; set; }
        public string Grouping { get; set; }
        public string Remarks { get; set; }

    }
}