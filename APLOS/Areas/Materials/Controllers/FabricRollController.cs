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
using System.Web;
using Aplos.Helpers;
using Library.OrderManagement.Production;

#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class FabricRollController : BaseController
    {
        #region -- Constructor

        private readonly IFabricRollMasterService _fabricRollMasterService;
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
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

        public ActionResult FabricGrouping()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetFromToDate()
        {
            string sql = @"SELECT FORMAT(MIN(A.AddedDate),'dd-MMM-yyyy') FromDate,FORMAT(MAX(A.AddedDate),'dd-MMM-yyyy') ToDate FROM TRN.InventoryReceive A WHERE A.GRNType in('GRNBYPO','GRN' ,'EMPGRN','GRNBYBOQ') AND A.Id Not IN(SELECT GRNId FROM [BPDT].[FabricRollManagementMaster])";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string paidHours)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fabricRollMasterService.Query(parameters, identity.CompanyGroupId, paidHours, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetSummaryList(string GRNId, string parameters)
        {
            return Json(_productionSummaryData.GetSummaryList(GRNId, parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetCustomerDataList(string HeaderId)
        {
            return Json(_productionSummaryData.GetCustomerDataList(HeaderId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetFilterList(string GRNId)
        {
            return Json(_productionSummaryData.GetFilterList(GRNId), JsonRequestBehavior.AllowGet);
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
            JsonResult json = Json(clsFabric.GRNList(column, value, fromDate, toDate, identity.PlantId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
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
                    //clsStaticInfo.SetDate(sheet1[xlsRow, xlsCol], Convert.ToDateTime(fabricRollMaster["PODate"]).ToString("dd-MMM-yyyy"));
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["PODate"].ToString();
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

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Invoice No"); xlsCol++;
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["InvoiceNo"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["InvoiceNo"].ToString();
                }
                //xlsCol += 1;

               

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
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PI No");
                if (!string.IsNullOrEmpty(Convert.ToString(fabricRollMaster["PINo"])))
                {
                    sheet1[xlsRow, xlsCol].Text = fabricRollMaster["PINo"].ToString();
                }
                xlsCol += 1;
                xlsRow = 6; xlsCol = 1;
                int endXlsCol = 1;

                #region ------------------Column Header------------------

                int colSeq = 0; int colGRNRowId = 0; int colLotNo, colColor, colFabricType, colFabricQuality = 0; int colShade = 0; int colMarkarCode = 0; int colShadeGroup = 0; int colLength, colOwnGSM, colStdGSM, colGSMVariation,colGSMVariationPer, colShrinkageGroup, colDia, colQualityStatus, colFTPReportNo, colFTPReceiveDate,colFTPStatus = 0;
                int colCutableWidth, colDimensionalChange3rdWash = 0; int colShrinkagewidth, colShrinkageLength = 0; int colQty = 0; int colQtyUoM = 0; int colActualQty = 0; int colSupplier = 0;
                int colSupplierRollNo, colSupplierQualityGrade = 0; int colOwnRollNo = 0; int colBuyerRollNo = 0; int colGrouping = 0; int colRemarks = 0;
                int colSpirality3rdWash, colPillingResistance, colBurstingStrength, colAbsorbency, colpHValue, colSewablity, colHandfeel = 0;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sequence");colSeq = xlsCol;xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GRNRowId");colGRNRowId = xlsCol;xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Color"); colColor = xlsCol;xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LotNo");colLotNo = xlsCol;xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ShadeGroup"); colShadeGroup = xlsCol;xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FabricType"); colFabricType = xlsCol;xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FabricQuality"); colFabricQuality = xlsCol;xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SupplierRollNo"); colSupplierRollNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OwnRollNo"); colOwnRollNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "QtyUoM"); colQtyUoM = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SupplierQty"); colSupplier = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ActualQty"); colActualQty = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "CutableWidth"); colCutableWidth = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OwnGSM"); colOwnGSM = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "StdGSM"); colStdGSM = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GSMVariation"); colGSMVariation = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GSMVariationPer",16); colGSMVariationPer = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shade"); colShade = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ShrinkageLengthWise",20); colShrinkageLength = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ShrinkageWidthWise",20); colShrinkagewidth = xlsCol; xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ShrinkageGroup"); colShrinkageGroup = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Dia",10); colDia = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SupplierQualityGrade",20); colSupplierQualityGrade = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "QualityStatus"); colQualityStatus = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FTPReportNo"); colFTPReportNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FTPReceiveDate"); colFTPReceiveDate = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FTPStatus"); colFTPStatus = xlsCol;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DimensionalChange3rdWash",11); colDimensionalChange3rdWash = xlsCol; xlsCol += 1;                
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Spirality3rdWash",16); colSpirality3rdWash = xlsCol; xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PillingResistance",16); colPillingResistance = xlsCol; xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BurstingStrength",16); colBurstingStrength = xlsCol; xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Absorbency"); colAbsorbency = xlsCol; xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "pHValue"); colpHValue = xlsCol; xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sewablity"); colSewablity = xlsCol; xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Handfeel"); colHandfeel = xlsCol; xlsCol += 1;

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

                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Supplier Qty";
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Actual Qty";
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorBoxTitle = "Number Error";
                                                     
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Cutable Weight";
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Own GSM";
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Std.GSM";
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colColor, xlsRow, colColor].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colLotNo, xlsRow, colLotNo].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colShadeGroup, xlsRow, colShadeGroup].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFabricType, xlsRow, colFabricType].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFabricQuality, xlsRow, colFabricQuality].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colSupplierRollNo, xlsRow, colSupplierRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colOwnRollNo, xlsRow, colOwnRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colQtyUoM, xlsRow, colQtyUoM].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colGSMVariation, xlsRow, colGSMVariation].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colGSMVariationPer, xlsRow, colGSMVariationPer].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShade, xlsRow, colShade].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShrinkageLength, xlsRow, colShrinkageLength].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShrinkagewidth, xlsRow, colShrinkagewidth].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colShrinkageGroup, xlsRow, colShrinkageGroup].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colDia, xlsRow, colDia].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colSupplierQualityGrade, xlsRow, colSupplierQualityGrade].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colQualityStatus, xlsRow, colQualityStatus].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFTPReportNo, xlsRow, colFTPReportNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFTPReceiveDate, xlsRow, colFTPReceiveDate].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFTPStatus, xlsRow, colFTPStatus].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colDimensionalChange3rdWash, xlsRow, colDimensionalChange3rdWash].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colSpirality3rdWash, xlsRow, colSpirality3rdWash].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colPillingResistance, xlsRow, colPillingResistance].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colBurstingStrength, xlsRow, colBurstingStrength].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colAbsorbency, xlsRow, colAbsorbency].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colpHValue, xlsRow, colpHValue].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colSewablity, xlsRow, colSewablity].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colHandfeel, xlsRow, colHandfeel].CellStyle.Locked = false;

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

                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Supplier Qty";
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Actual Qty";
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Cutable Weight";
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Own GSM";
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.IsEmptyCellAllowed = true;
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.AllowType = ExcelDataType.Decimal;
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.FirstFormula = "0";
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Std.GSM";
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].DataValidation.ErrorBoxTitle = "Number Error";

                            sheet1.Range[xlsRow, colColor, xlsRow, colColor].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colLotNo, xlsRow, colLotNo].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colShadeGroup, xlsRow, colShadeGroup].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFabricType, xlsRow, colFabricType].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFabricQuality, xlsRow, colFabricQuality].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colSupplierRollNo, xlsRow, colSupplierRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colOwnRollNo, xlsRow, colOwnRollNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colQtyUoM, xlsRow, colQtyUoM].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colSupplier, xlsRow, colSupplier].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colActualQty, xlsRow, colActualQty].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colCutableWidth, xlsRow, colCutableWidth].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colOwnGSM, xlsRow, colOwnGSM].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colStdGSM, xlsRow, colStdGSM].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colGSMVariation, xlsRow, colGSMVariation].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colGSMVariationPer, xlsRow, colGSMVariationPer].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShade, xlsRow, colShade].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShrinkageLength, xlsRow, colShrinkageLength].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colShrinkagewidth, xlsRow, colShrinkagewidth].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colShrinkageGroup, xlsRow, colShrinkageGroup].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colDia, xlsRow, colDia].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colSupplierQualityGrade, xlsRow, colSupplierQualityGrade].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colQualityStatus, xlsRow, colQualityStatus].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFTPReportNo, xlsRow, colFTPReportNo].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFTPReceiveDate, xlsRow, colFTPReceiveDate].CellStyle.Locked = false;
                            sheet1.Range[xlsRow, colFTPStatus, xlsRow, colFTPStatus].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colDimensionalChange3rdWash, xlsRow, colDimensionalChange3rdWash].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colSpirality3rdWash, xlsRow, colSpirality3rdWash].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colPillingResistance, xlsRow, colPillingResistance].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colBurstingStrength, xlsRow, colBurstingStrength].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colAbsorbency, xlsRow, colAbsorbency].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colpHValue, xlsRow, colpHValue].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colSewablity, xlsRow, colSewablity].CellStyle.Locked = false;
                            //sheet1.Range[xlsRow, colHandfeel, xlsRow, colHandfeel].CellStyle.Locked = false;

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
        [HttpGet, Authorize]
        public ActionResult GetFabricRollChildPendingList(string FabricRollManagementMasterId)
        {
            return Json(clsFabric.GetFabricRollChildPendingList(FabricRollManagementMasterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetFabricRollChildConfirmList(string FabricRollManagementMasterId)
        {
            return Json(clsFabric.GetFabricRollChildConfirmList(FabricRollManagementMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetFabricRollMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(clsFabric.GetFabricRollChildPendingDataList(identity.PlantId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetGroupingData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(clsFabric.GetGroupingData(identity.PlantId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetFabricRollMasterConfirmDataList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(clsFabric.GetFabricRollMasterConfirmDataList(identity.PlantId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
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
                DataTable dt = workbook.Worksheets[0].ExportDataTable(6, 1, 5000, 25, ExcelExportDataTableOptions.ColumnNames);
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

        [HttpPost]
        public ActionResult UpdateFabricDetails(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("select * from BPDT.FabricRollManagementChild where CutableWidth = '" + item["CutableWidth"] + "' and ShrinkageWidthWise = '" + item["ShrinkageWidthWise"] + "' and ShrinkageLengthWise = '" + item["ShrinkageLengthWise"] + "' and Shade = '" + item["Shade"] + "'", out dsProdBooked, false, "1");
                        //DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dsProdBooked.Tables[0].Rows.Count > 0)
                        {
                            for (int j = 0; j < dsProdBooked.Tables[0].Rows.Count; j++)
                            {
                                dsProdBooked.Tables[0].DefaultView.RowFilter = "Id='" + dsProdBooked.Tables[0].Rows[j]["Id"].ToString() + "'";

                                if (dsProdBooked.Tables[0].DefaultView.Count > 0)
                                {
                                    //edit
                                    DataRow dr = dsProdBooked.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();

                                    dr["Status"] = 1;
                                    if(item["CutableWidthGroup"] == null)
                                    {
                                        dr["CutableWidthGroup"] = DBNull.Value;
                                    }
                                    else
                                    {
                                        dr["CutableWidthGroup"] = item["CutableWidthGroup"];
                                    }
                                    dr["MarkerGroup"] = item["MarkerGroup"];
                                    dr["FabricGroup"] = item["FabricGroup"];
                                    if (item["ShrinkageGroup"] == null)
                                    {
                                        dr["ShrinkageGroup"] = DBNull.Value;
                                    }
                                    else
                                    {
                                        dr["ShrinkageGroup"] = item["ShrinkageGroup"];
                                    }
                                    if (item["ShadeGroup"] == null)
                                    {
                                        dr["ShadeGroup"] = DBNull.Value;
                                    }
                                    else
                                    {
                                        dr["ShadeGroup"] = item["ShadeGroup"];
                                    }
                                    dr["Remarks"] = item["Remarks"];
                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                    dr.EndEdit();
                                }
                            }
                        }

                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
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

        #region upload Production Bulletin picture
        [HttpPost, Authorize]

        public ActionResult SaveFabricRollFileDefault(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the Fabric Roll.");

                foreach (var file in UploadDefault)
                {

                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    //var fileName = Path.GetFileName(AdditionalData.Rows[0]["Id"].ToString() + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.GetFabricRollsFilePath(), fileName);

                    var directory = ResourcesPathReader.GetFabricRollsFilePath();
                    var path = Path.Combine(directory);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetFabricRollsFilePath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetFabricRollsFilePath());
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select * from [BPDT].[FabricRollManagementMaster] where id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();
                    var FN = dsLocal.Tables[0].Rows[0]["Attachment"].ToString();
                    if (fileN != FN)
                        if (System.IO.File.Exists(path + UploadDefault_data + Path.GetExtension(FN)))
                            System.IO.File.Delete(path + UploadDefault_data + Path.GetExtension(FN));
                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["Attachment"] = fileN;

                        dsLocal.Tables[0].Rows[0].EndEdit();

                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }
        [HttpPost, Authorize]
        public ActionResult GetFileInfo(string Id)
        {

            try
            {
                return Json(_sqlRepository.GetDataCollection("SELECT Attachment FROM [BPDT].[FabricRollManagementMaster] WHERE Id ='" + Id + "' "), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult createCustomer(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "TRN.CustomerMaterial";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                //ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                //conC.BeginTransaction();
                //conC.executeQuery("delete from " + TableName + " where  HeaderId ='" + Pid + "'");
                //conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and HeaderId='" + item["HeaderId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "CD" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion upload product picture



        [HttpPost, Authorize]
        public ActionResult GetFabricRollReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("EJVALUE"))
                        continue;

                    dt.Columns.Add(item);
                }


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("EJVALUE"))
                            continue;

                        dr[item] = data[i][item];
                    }

                    dt.Rows.Add(dr);
                }

                string fileName = "";
                fileName = GetFabricRollReportXL(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string GetFabricRollReportXL(DataTable data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            var filePath = "";
            try
            {

                ReportUtility ru = new ReportUtility();

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "FabricRollData";
                sheet1 = workbook.Worksheets[0];

                int xlsRow = 6; int xlsCol = 1;
                int endXlsCol = 1;
                #region ------------------Column Header------------------

                int colSeq = 0; int colGRNRowId = 0; int colLotNo, colColor, colFabricType, colFabricQuality = 0; int colShade = 0; int  colOwnGSM, colStdGSM, colGSMVariation, colGSMVariationPer,  colDia, colQualityStatus, colFTPReportNo, colFTPReceiveDate, colFTPStatus = 0;
                int colCutableWidth = 0; int colShrinkagewidth, colShrinkageLength = 0; int colQtyUoM = 0; int colActualQty = 0; int colSupplier = 0;
                int colSupplierRollNo, colSupplierQualityGrade = 0; int colOwnRollNo = 0;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Id");int colId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FabricRollManagementMasterId");int colFabricRollManagementMasterId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sequence"); colSeq = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GRNRowId"); colGRNRowId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Color"); colColor = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LotNo"); colLotNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FabricType"); colFabricType = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FabricQuality"); colFabricQuality = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SupplierRollNo"); colSupplierRollNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OwnRollNo"); colOwnRollNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "QtyUoM"); colQtyUoM = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SupplierQty"); colSupplier = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ActualQty"); colActualQty = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "CutableWidth"); colCutableWidth = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OwnGSM"); colOwnGSM = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "StdGSM"); colStdGSM = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GSMVariation"); colGSMVariation = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GSMVariationPer", 16); colGSMVariationPer = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shade"); colShade = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ShrinkageLengthWise", 20); colShrinkageLength = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ShrinkageWidthWise", 20); colShrinkagewidth = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Dia", 10); colDia = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SupplierQualityGrade", 20); colSupplierQualityGrade = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "QualityStatus"); colQualityStatus = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FTPReportNo"); colFTPReportNo = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FTPReceiveDate"); colFTPReceiveDate = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FTPStatus"); colFTPStatus = xlsCol;
                
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------

                int endCol = xlsCol;
                sheet1.Range[xlsRow, 1, xlsRow, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet1.Range[xlsRow, 1, xlsRow, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet1.Range[xlsRow, 1, xlsRow, endCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endCol].CellStyle.Font.Size = 9f;
                sheet1.Range[xlsRow, 1, xlsRow, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endCol].BorderAround(ExcelLineStyle.Hair);

                xlsRow++;

                int startRow = xlsRow;
                int LastRow = xlsRow + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet1[xlsRow, colId].Text = data.Rows[i]["Id"].ToString();
                    sheet1[xlsRow, colFabricRollManagementMasterId].Text = data.Rows[i]["FabricRollManagementMasterId"].ToString();
                    sheet1[xlsRow, colSeq].Text = data.Rows[i]["Sequence"].ToString();
                    sheet1[xlsRow, colColor].Text = data.Rows[i]["Color"].ToString();
                    sheet1[xlsRow, colLotNo].Text = data.Rows[i]["LotNo"].ToString();
                    sheet1[xlsRow, colFabricType].Text = data.Rows[i]["FabricType"].ToString();
                    sheet1[xlsRow, colFabricQuality].Text = data.Rows[i]["FabricQuality"].ToString();
                    sheet1[xlsRow, colSupplierRollNo].Text = data.Rows[i]["SupplierRollNo"].ToString();
                    sheet1[xlsRow, colOwnRollNo].Text = data.Rows[i]["OwnRollNo"].ToString();
                    sheet1[xlsRow, colQtyUoM].Text = data.Rows[i]["QtyUoM"].ToString();
                    sheet1[xlsRow, colSupplier].Number = clsStaticInfo.dbl(data.Rows[i]["SupplierQty"].ToString());

                    sheet1[xlsRow, colActualQty].Number = clsStaticInfo.dbl(data.Rows[i]["ActualQty"].ToString());
                    sheet1[xlsRow, colCutableWidth].Number = clsStaticInfo.dbl(data.Rows[i]["CutableWidth"].ToString());
                    sheet1[xlsRow, colOwnGSM].Text = data.Rows[i]["OwnGSM"].ToString();
                    sheet1[xlsRow, colStdGSM].Text = data.Rows[i]["StdGSM"].ToString();
                    sheet1[xlsRow, colGSMVariation].Text = data.Rows[i]["GSMVariation"].ToString();
                    sheet1[xlsRow, colGSMVariationPer].Text = data.Rows[i]["GSMVariationPer"].ToString();
                    sheet1[xlsRow, colShade].Text = data.Rows[i]["Shade"].ToString();
                    sheet1[xlsRow, colShrinkageLength].Number = clsStaticInfo.dbl(data.Rows[i]["ShrinkageLengthWise"].ToString());
                    sheet1[xlsRow, colShrinkagewidth].Number = clsStaticInfo.dbl(data.Rows[i]["ShrinkageWidthWise"].ToString());
                    sheet1[xlsRow, colActualQty].Number = clsStaticInfo.dbl(data.Rows[i]["ActualQty"].ToString());
                    sheet1[xlsRow, colDia].Text = data.Rows[i]["Dia"].ToString();
                    sheet1[xlsRow, colSupplierQualityGrade].Text = data.Rows[i]["SupplierQualityGrade"].ToString();
                    sheet1[xlsRow, colQualityStatus].Text = data.Rows[i]["QualityStatus"].ToString();
                    sheet1[xlsRow, colFTPReportNo].Text = data.Rows[i]["FTPReportNo"].ToString();
                    sheet1[xlsRow, colFTPReceiveDate].Text = data.Rows[i]["FTPReceiveDate"].ToString();
                    sheet1[xlsRow, colFTPStatus].Text = data.Rows[i]["FTPStatus"].ToString();

                    sheet1.Range[xlsRow, 1, xlsRow, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endCol].CellStyle.Font.Size = 8f;
                    xlsRow++;

                }

               // sheet1.AutoFilters.FilterRange = sheet1.Range[startRow - 1, 1, xlsRow, endCol];
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet1.Range[startRow, 1, xlsRow, endCol].CellStyle.Font.Size = 8f;
                sheet1["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet1, endCol, "Fabric Roll Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet1, 6, ExcelPageOrientation.Landscape);
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet1.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet1.PageSetup.TopMargin = 0.2;
                sheet1.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet1.PageSetup.LeftMargin = 0.2;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
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

        [HttpPost, Authorize]
        public JsonResult ImportFabricData()
        {
            string path;
            clsTemplateReadProfile objR = null;
            try
            {
                objR = new clsTemplateReadProfile();
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFiles(out path);
                var data = ReadFabricData(identity.PlantId, path);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public List<FabricRollGroupTemplate> ReadFabricData(string plantid, string path)
        {
            List<FabricRollGroupTemplate> data = null;
            //string path = "";
            DataSet dsExcel = null;
            try
            {
                data = new List<FabricRollGroupTemplate>();
                //SaveFile(out path);
                ReadFabricFile(path, out dsExcel);
                Validation(dsExcel, plantid);
                data = dsExcel.Tables[0].ToList<FabricRollGroupTemplate>();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ReadFabricFile(string path, out DataSet dsExcel)
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
                DataTable dt = workbook.Worksheets[0].ExportDataTable(5, 1, 5000, 27, ExcelExportDataTableOptions.ColumnNames);
                dt.DefaultView.RowFilter = "isnull(Sequence,'')<>''";
                dt = dt.DefaultView.ToTable();
               
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


        [HttpPost]
        public JsonResult CreateFabricGrouping(List<Dictionary<string, object>> grnDetailList)
        {
            clsFabric.SaveFabricGrouping(grnDetailList);
            return Json(new { Message = AplosMessage.Insert });
        }


    }


    public class FabricRollTemplate
    {
        public string Sequence { get; set; }
        public string GRNRowId { get; set; }
        public string LotNo { get; set; }
        public string Color { get; set; }
        //public string ShadeGroup { get; set; }
        public string FabricType { get; set; }
        public string FabricQuality { get; set; }
        public string SupplierRollNo { get; set; }
        public string OwnRollNo { get; set; }
        public string QtyUoM { get; set; }
        public string SupplierQty { get; set; }
        public string ActualQty { get; set; }
        public string CutableWidth { get; set; }
        public string OwnGSM { get; set; }
        public string StdGSM { get; set; }
        public string GSMVariation { get; set; }
        public string GSMVariationPer { get; set; }
        public string Shade { get; set; }
        public string ShrinkageLengthWise { get; set; }
        public string ShrinkageWidthWise { get; set; }
        //public string ShrinkageGroup { get; set; }
        public string Dia { get; set; }
        public string SupplierQualityGrade { get; set; }
        public string QualityStatus { get; set; }
        public string FTPReportNo { get; set; }
        public string FTPReceiveDate { get; set; }
        public string FTPStatus { get; set; }
        //public string DimensionalChange3rdWash { get; set; }
        //public string Spirality3rdWash { get; set; }
        //public string PillingResistance { get; set; }
        //public string BurstingStrength { get; set; }
        //public string Absorbency { get; set; }
        //public string pHValue { get; set; }
        //public string Sewablity { get; set; }
        //public string Handfeel { get; set; }

    }

    public class FabricRollGroupTemplate
    {

        public string Id { get; set; }
        public string FabricRollManagementMasterId { get; set; }
        public string Sequence { get; set; }
        public string GRNRowId { get; set; }
        public string LotNo { get; set; }
        public string Color { get; set; }
        public string FabricType { get; set; }
        public string FabricQuality { get; set; }
        public string SupplierRollNo { get; set; }
        public string OwnRollNo { get; set; }
        public string QtyUoM { get; set; }
        public string SupplierQty { get; set; }
        public string ActualQty { get; set; }
        public string CutableWidth { get; set; }
        public string OwnGSM { get; set; }
        public string StdGSM { get; set; }
        public string GSMVariation { get; set; }
        public string GSMVariationPer { get; set; }
        public string Shade { get; set; }
        public string ShrinkageLengthWise { get; set; }
        public string ShrinkageWidthWise { get; set; }
        public string Dia { get; set; }
        public string SupplierQualityGrade { get; set; }
        public string QualityStatus { get; set; }
        public string FTPReportNo { get; set; }
        public string FTPReceiveDate { get; set; }
        public string FTPStatus { get; set; }
        //public string DimensionalChange3rdWash { get; set; }
        //public string Spirality3rdWash { get; set; }
        //public string PillingResistance { get; set; }
        //public string BurstingStrength { get; set; }
        //public string Absorbency { get; set; }
        //public string pHValue { get; set; }
        //public string Sewablity { get; set; }
        //public string Handfeel { get; set; }

    }
}