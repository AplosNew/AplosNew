using Library.Model.IE;
using Aplos.Properties;
using System.Web.Mvc;
using System.Threading;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Aplos.Controllers;
using Library.Service.IEnumerable;
using System.Collections.Generic;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.Data;
using System;
using OTSBD;
using Library.Service.Machines;
using System.Web;
using System.IO;
using Library.Data;
using Library.Service.Enums;
using Library.Service.Logs;
using System.Reflection;
using Library.Core;
using System.Linq;
using System.Drawing;
using Library.General.IE;

namespace Aplos.Areas.IE.Controllers
{
    public class BulletinTemplateController : BaseController
    {
        #region Constructor

        private readonly IBulletinTemplateService _bulletinTemplateService;
        private readonly IOperationVariationService _operationVariationService;
        clsBulletin clsb = new clsBulletin();
        private readonly ISqlRepository _sqlRepository;


        public BulletinTemplateController(
            IBulletinTemplateService bulletinTemplateService
          , ISqlRepository sqlRepository
            , IOperationVariationService operationVariationService

            )
        {
            _bulletinTemplateService = bulletinTemplateService;
            _operationVariationService = operationVariationService;
            _sqlRepository = sqlRepository;

        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region Operation

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bulletinTemplateService.Query(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetFileInfo(string Id)
        {

            try
            {
                return Json(_sqlRepository.GetDataCollection("SELECT PicFileName FROM MST.BulletinTemplate WHERE Id ='" + Id + "' "), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetOperationData(string processId, string bulletinTemplateId, string productMasterId)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bulletinTemplateService.GetOperationData(identity.CompanyGroupId, processId, bulletinTemplateId, productMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProcessQtyAndNoWSData(string processId, string productMasterId)
        {
            return Json(_bulletinTemplateService.GetProcessQtyAndNoWSData(processId, productMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBulletinOperation(string bulletinTemplateMasterId)
        {
            return Json(_bulletinTemplateService.GetBulletinOperation(bulletinTemplateMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMachineChangeInfo(string plantId)
        {
            try
            {
                return Json(clsb.GetMachineChangeInfo(plantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetBulletinMachineOperation(string bulletinTemplateMasterId)
        {
            try
            {
                return Json(clsb.GetBulletinMachineOperation(bulletinTemplateMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult Create(BulletinTemplate bulletinTemplate)
        {
            try
            {
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bulletinTemplate.CompanyGroupId = identity.CompanyGroupId;
                _bulletinTemplateService.Insert(bulletinTemplate);
                return Json(new { BulletinTemplate = bulletinTemplate, Message = AplosMessage.Success });
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult Edit(BulletinTemplate bulletinTemplate)
        {
            _bulletinTemplateService.Update(bulletinTemplate);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult CreateProcess(BulletinTemplateMaster bulletinTemplateMaster)
        {
            _bulletinTemplateService.InsertOrUpdateProcess(bulletinTemplateMaster);
            return Json(new { BulletinTemplateMaster = bulletinTemplateMaster, Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetProcessData(string bulletinTemplateId)
        {
            return Json(_bulletinTemplateService.GetBulletinProcess(bulletinTemplateId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateBuyer(BulletinTemplateBuyerInfo bulletinTemplateBuyer)
        {
            _bulletinTemplateService.InsertOrUpdateBuyer(bulletinTemplateBuyer);
            return Json(new { BulletinTemplateBuyerInfo = bulletinTemplateBuyer, Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo(string processId)
        {
            return Json(_bulletinTemplateService.GetCbo(processId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateOperation(IEnumerable<BulletinTemplateDetail> bulletinTemplateDetails, string bulletinTemplateMasterId)
        {
            _bulletinTemplateService.InsertOrUpdateOperation(bulletinTemplateDetails, bulletinTemplateMasterId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult InsertOperation(string Code, string processId, string bulletinTemplateMasterId)
        {

            //string str = Code.Replace(" ", ",");
            string codes = "'" + Code.Replace(" ", "','") + "'";//replaced with ""
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IdentityParameter para = new IdentityParameter
            {
                CompanyGroupId = identity.CompanyGroupId,
                CompanyId = identity.CompanyId,
                PlantId = identity.PlantId,
                AddedBy = identity.Name,
                AddedDate = DateTime.Now,
                AddedFromIP = identity.IPAddress,
                UpdatedBy = identity.Name,
                UpdatedDate = DateTime.Now,
                UpdatedFromIP = identity.IPAddress
            };
            _bulletinTemplateService.InsertOperation(para, codes, processId, bulletinTemplateMasterId);
            return Json(new { Message = AplosMessage.Success });
        }

        [Authorize, HttpPost]
        public JsonResult UpdateMachine(BulletinTemplateDetail machine)
        {
            _bulletinTemplateService.UpdateMachine(machine);
            return Json(new { Message = AplosMessage.Updated });
        }

        [Authorize, HttpPost]
        public JsonResult UpdateSequence(BulletinTemplateDetail bulletinTemplateDetail)
        {
            _bulletinTemplateService.UpdateSequence(bulletinTemplateDetail);
            return Json(new { Message = AplosMessage.Updated });
        }

        [Authorize, HttpPost]
        public JsonResult UpdateOperationVaiationCode(BulletinTemplateDetail bulletinTemplateDetail, string processId, string bulletinTemplateMasterId)
        {

            string Code = "'" + bulletinTemplateDetail.OperationCode.Replace(" ", "','") + "'";//replaced with ""
            //string Code = bulletinTemplateDetail.OperationCode;
            decimal Sequence = bulletinTemplateDetail.Sequence;
            _bulletinTemplateService.DeleteOperation(bulletinTemplateDetail.Id);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IdentityParameter para = new IdentityParameter
            {
                CompanyGroupId = identity.CompanyGroupId,
                CompanyId = identity.CompanyId,
                PlantId = identity.PlantId,
                AddedBy = identity.Name,
                AddedDate = DateTime.Now,
                AddedFromIP = identity.IPAddress,
                UpdatedBy = identity.Name,
                UpdatedDate = DateTime.Now,
                UpdatedFromIP = identity.IPAddress
            };

            _bulletinTemplateService.ReplaceOperation(para, Code, Sequence, processId, bulletinTemplateMasterId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult GetBuyerData(string bulletinTemplateId)
        {
            return Json(_bulletinTemplateService.GetBulletinBuyer(bulletinTemplateId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetThreadMatrixData(string bulletinTemplateMasterId)
        {
            try
            {
                return Json(clsb.GetThreadMatrixData(bulletinTemplateMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _bulletinTemplateService.DeleteBulletin(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public ActionResult DeleteProcess(string id)
        {
            _bulletinTemplateService.DeleteProcess(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public ActionResult DeleteBuyer(string id)
        {
            _bulletinTemplateService.DeleteBuyer(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult DeleteOperation(string id)
        {
            _bulletinTemplateService.DeleteOperation(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        [Authorize, HttpGet]
        public JsonResult GetAutoSequence(string id)
        {
            return Json(_bulletinTemplateService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Copy(BulletinTemplate bulletinTemplate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bulletinTemplate.AddedBy = identity.Name;
                bulletinTemplate.AddedFromIP = identity.IPAddress;
                _bulletinTemplateService.Copy(bulletinTemplate);
                return Json(new { Message = "Data copied successfully." });
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        
        private string GetGeneralPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "B", out idFromDB);
            systemID = idFromDB;
            sID = systemID.Trim();
            return sID;

        }
        

        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = drSource[drSource.Table.Columns[COL].ColumnName];

                }
                catch (Exception)
                {
                }
                try
                {
                    drDestination["AddedBy"] = identity.Name;
                    drDestination["AddedDate"] = DateTime.Now;
                    drDestination["AddedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedBy"] = identity.Name;
                    drDestination["UpdatedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedDate"] = DateTime.Now;

                }
                catch (Exception)
                {
                }
            }

        }

        public ActionResult DeleteMultiOperation(string id)
        {
            clsb.DeleteMultiBulletinOperation(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        

        #region upload Production Bulletin picture
        [HttpPost, Authorize]
        public ActionResult SaveBulletinDefault(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the Bulletin first");

                foreach (var file in UploadDefault)
                {

                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.GetBulletinImagePath(), fileName);

                    var directory = ResourcesPathReader.GetBulletinImagePath();
                    var path = Path.Combine(directory);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetBulletinImagePath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetBulletinImagePath());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select* from [MST].[BulletinTemplate] where id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();
                    var FN = dsLocal.Tables[0].Rows[0]["PicFileName"].ToString();
                    if (fileN != FN)
                        if (System.IO.File.Exists(path + UploadDefault_data + Path.GetExtension(FN)))
                            System.IO.File.Delete(path + UploadDefault_data + Path.GetExtension(FN));
                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["PicFileName"] = fileN;

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


        #endregion upload product picture

        #region Multi Operation Add
        [HttpPost, Authorize]
        public JsonResult InsertMultiOperation(string Code, string processId, string bulletinTemplateMasterId, IEnumerable<MultiCode> MultiCodeList)
        {

            //string str = Code.Replace(" ", ",");
            string codes = "'" + Code.Replace(" ", "','") + "'";//replaced with ""
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IdentityParameter para = new IdentityParameter
            {
                CompanyGroupId = identity.CompanyGroupId,
                CompanyId = identity.CompanyId,
                PlantId = identity.PlantId,
                AddedBy = identity.Name,
                AddedDate = DateTime.Now,
                AddedFromIP = identity.IPAddress,
                UpdatedBy = identity.Name,
                UpdatedDate = DateTime.Now,
                UpdatedFromIP = identity.IPAddress
            };
            SaveMultiOperation(para, codes, processId, bulletinTemplateMasterId, MultiCodeList);
            return Json(new { Message = AplosMessage.Success });
        }

       
        private string GetOperationPK()
        {
            //return GetAutoNumber(nameof(BulletinTemplateDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(BulletinTemplateDetail), out sID);
            return sID;
        }
        public void SaveMultiOperation(IdentityParameter para, string Code, string processId, string bulletinTemplateMasterId, IEnumerable<MultiCode> MultiCodeList)
        {
            try
            {
                DataSet dataSet = clsb.GetOperationDataByCode(para.CompanyGroupId, Code, processId, bulletinTemplateMasterId);
                ConnectionManager.DAL.ConManager objCon;
                var id = GetOperationPK();
                string sql = "SELECT * FROM [MST].[BulletinTemplateDetail] WHERE Id=''";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsOperation, false, "1");
                int count = 0;


                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                    {
                        //var filteredSeq = MultiCodeList.Where(p => dataSet.Tables[0].Rows[i]["OperationCode"] = p.OperationCode.Contains(p.Sequenc.ToString()));
                        var filteredSeq = MultiCodeList.Where(p => p.OperationCode == dataSet.Tables[0].Rows[i]["OperationCode"].ToString()).Select(p => p.Sequenc).FirstOrDefault();

                        count++;

                        DataRow dr = dsOperation.Tables[0].NewRow();

                        dr["Id"] = id + "-" + count;
                        dr["BulletinTemplateMasterId"] = bulletinTemplateMasterId;
                        dr["Sequence"] = filteredSeq;
                        dr["OperationVariationId"] = dataSet.Tables[0].Rows[i]["OperationVariationId"];
                        dr["OperationGroup"] = null;
                        dr["SkillId"] = dataSet.Tables[0].Rows[i]["SkillId"];
                        dr["MachineVarientId"] = dataSet.Tables[0].Rows[i]["MachineVarientId"];
                        dr["FGZoneId"] = null;
                        dr["FGComponentId"] = null;
                        dr["AdditionalSPT"] = dataSet.Tables[0].Rows[i]["AdditionalSAM"];
                        dr["TotalSPT"] = dataSet.Tables[0].Rows[i]["TotalSAM"];
                        dr["AllotedWorkstation"] = 0;
                        dr["AllotedManpower"] = 0;
                        dr["AvgAllotedTime"] = 0;
                        dr["AttachmentId"] = null;
                        dr["GaugeFolderId"] = null;
                        dr["OperationConsumptionId"] = null;
                        dr["OperationTypeId"] = dataSet.Tables[0].Rows[i]["OperationTypeId"];
                        dr["Frequency"] = dataSet.Tables[0].Rows[i]["Frequency"];
                        dr["Remark"] = null;
                        dr["OperationCategoryId"] = dataSet.Tables[0].Rows[i]["OperationCategoryId"];
                        dr["QualityLevel"] = null;
                        dr["OperationTargetPerHr"] = 0;
                        dr["RequiredManPower"] = 0;

                        dr["SPI"] = dataSet.Tables[0].Rows[i]["SPI"];
                        dr["NoOfStitch"] = 0;
                        dr["OperationLength"] = dataSet.Tables[0].Rows[i]["OperationLength"];
                        dr["StitchCodeId"] = dataSet.Tables[0].Rows[i]["StitchCodeId"];
                        dr["FabricWidth"] = 0;
                        dr["NeedleDescription"] = null;
                        dr["NeedleMaterialMasterId"] = null;
                        dr["NeedleArticleId"] = null;
                        dr["BobbinMaterialMasterId"] = null;
                        dr["BobbinArticleId"] = null;
                        dr["LooperDescription"] = null;
                        dr["LooperMaterialMasterId"] = null;
                        dr["LooperArticleId"] = null;
                        dr["SPIConsumption"] = 0;
                        dr["NeedleConsumption"] = 0;
                        dr["BobbinConsumption"] = 0;
                        dr["LooperConsumption"] = 0;
                        dr["Consumption"] = 0;
                        dr["WastagePercentage"] = 0;
                        dr["ExtraOrderPercentage"] = 0;

                        dr["AddedBy"] = para.AddedBy;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = para.AddedFromIP;


                        dsOperation.Tables[0].Rows.Add(dr);
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsOperation);
                }
                else
                {
                    throw new Exception("Wrong Operation Code !!!.");
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        #endregion Multi Operation Add

        [HttpGet, Authorize]
        public ActionResult GetProductionBulletinInfo(string Id)
        {

            try
            {
                return Json(clsb.GetProductionBulletinInfo(Id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }


        #endregion

        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;

        }
        private void SetHeaderTextUnder(ref IWorksheet sheet, int row, int col, int endRow, int endCol, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col, endRow, endCol].Text = txt;
            sheet.Range[row, col, endRow, endCol].ColumnWidth = width;
            sheet.Range[row, col, endRow, endCol].CellStyle.Font.Bold = true;
            sheet.Range[row, col, endRow, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col, endRow, endCol].Merge();
            //sheet.Range[row, col, endRow, endCol].HorizontalAlignment = ExcelVAlign.;

        }

        #region Reports for Bullatin Template 
        [HttpGet, Authorize]
        public ActionResult GetBulletinTamplateIndexReport(ReportFormat reportFormat)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Bulletin Template";
            var workbook = GetBulletinTamplateIndexReportWorkSheet();
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

        private IWorkbook GetBulletinTamplateIndexReportWorkSheet()
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "BulletinTemplate";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = clsb.GetBulletinTemplateData();

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Bulletin ID", 12, ExcelHAlign.HAlignLeft);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Bulletin Name", 25, ExcelHAlign.HAlignLeft);
            int ColBulletinName = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Alternative Name", 25, ExcelHAlign.HAlignLeft);
            int ColAlternativeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Product Master", 25, ExcelHAlign.HAlignLeft);
            int ColProductMaster = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Size Group", 11, ExcelHAlign.HAlignLeft);
            int ColSizeGroup = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Buyer", 25, ExcelHAlign.HAlignLeft);
            int ColBuyer = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Buyer Item RefNo.", 25, ExcelHAlign.HAlignLeft);
            int ColBuyerItemRefNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Own Style RefNo.", 25, ExcelHAlign.HAlignLeft);
            int ColOwnStyleRefNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process", 30, ExcelHAlign.HAlignLeft);
            int ColProcess = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "By Whom", 15, ExcelHAlign.HAlignLeft);
            int ColByWhom = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "SPT", 15, ExcelHAlign.HAlignCenter);
            int ColSPT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ReqMP", 15, ExcelHAlign.HAlignCenter);
            int ColReqMP = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Allocated Mp", 15, ExcelHAlign.HAlignCenter);
            int ColAllocatedMP = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "No of W/S", 15, ExcelHAlign.HAlignCenter);
            int ColNoofWS = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Target", 15, ExcelHAlign.HAlignCenter);
            int ColTarget = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Added Date", 15, ExcelHAlign.HAlignLeft);
            int ColAddedDate = COL;
            COL++;

            endCol = COL;
            #endregion Headers

            var startRow = 0;

            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColId].Text = data.Rows[i]["Id"].ToString();
                sheet[ROW, ColBulletinName].Text = data.Rows[i]["BulletinName"].ToString();
                sheet[ROW, ColAlternativeName].Text = data.Rows[i]["AlternativeName"].ToString();
                sheet[ROW, ColProductMaster].Text = data.Rows[i]["ProductMaster"].ToString();
                sheet[ROW, ColSizeGroup].Text = data.Rows[i]["SizeGroup"].ToString();

                sheet[ROW, ColBuyer].Text = data.Rows[i]["Buyer"].ToString();
                sheet[ROW, ColBuyerItemRefNo].Text = data.Rows[i]["BuyerItemRefNo"].ToString();

                sheet[ROW, ColOwnStyleRefNo].Text = data.Rows[i]["OwnStyleRefNo"].ToString();
                sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();

                sheet[ROW, ColByWhom].Text = data.Rows[i]["ByWhom"].ToString();

                sheet[ROW, ColSPT].Number = Convert.ToDouble(data.Rows[i]["TotalSPT"].ToString());
                sheet[ROW, ColSPT].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColSPT].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColSPT].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                sheet[ROW, ColReqMP].Number = Convert.ToDouble(data.Rows[i]["RequiredManPower"].ToString());
                sheet[ROW, ColReqMP].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColReqMP].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColReqMP].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColAllocatedMP].Number = Convert.ToDouble(data.Rows[i]["AllotedManpower"].ToString());
                sheet[ROW, ColAllocatedMP].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColAllocatedMP].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColAllocatedMP].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColNoofWS].Number = Convert.ToDouble(data.Rows[i]["AllotedWorkstation"].ToString());
                sheet[ROW, ColNoofWS].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet[ROW, ColNoofWS].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColNoofWS].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColTarget].Text = data.Rows[i]["LineTargetPerHour"].ToString();
                sheet[ROW, ColTarget].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColTarget].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColAddedDate].Text = data.Rows[i]["CreationDate"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.00";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyHeader(ref sheet, endCol, "Bulletin Tamplate", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        

        [HttpGet, Authorize]
        public ActionResult GetBulletinTamplateReport(ReportFormat reportFormat, string bulletinTemplateId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Bulletin Template " + bulletinTemplateId + "";
            var workbook = GetBulletinTamplateReportWorkSheet(bulletinTemplateId);
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

        private void GetWorkSheet2(ref IWorksheet sheet, ref ReportUtility report, DataTable data)
        {

            sheet.Name = "Bulletin Template Formula";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            if (data.Rows.Count > 0)
            {
                int ColBulletinNameHeader = 1;
                int ColBulletinNameEnd;
                int ColByWhomHeader;
                int ColByWhomEnd;
                int ColByWhom;
                int ColProductMasterHeader = 1;
                int ColProductEnd;


                SetHeaderTextTop(ref sheet, ROW, ColBulletinNameHeader, "Bulletin Name :", 12, ExcelHAlign.HAlignLeft);
                ColBulletinNameHeader++;
                ColBulletinNameEnd = ColBulletinNameHeader + 1;
                sheet.Range[ROW, ColBulletinNameHeader, ROW, ColBulletinNameEnd].Text = data.Rows[0]["BulletinName"].ToString();
                sheet.Range[ROW, ColBulletinNameHeader, ROW, ColBulletinNameEnd].Merge();
                sheet.Range[ROW, ColBulletinNameHeader, ROW, ColBulletinNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColBulletinNameHeader, ROW, ColBulletinNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColBulletinNameEnd++;

                ColByWhomHeader = ColBulletinNameEnd;
                SetHeaderTextTop(ref sheet, ROW, ColByWhomHeader, "ByWhom :", 20, ExcelHAlign.HAlignLeft);
                ColByWhomHeader++;
                ColByWhomEnd = ColByWhomHeader + 1;
                ColByWhom = ColByWhomHeader;
                sheet.Range[ROW, ColByWhom, ROW, ColByWhomEnd].Text = data.Rows[0]["ByWhom"].ToString();
                sheet.Range[ROW, ColByWhom, ROW, ColByWhomEnd].Merge();
                sheet.Range[ROW, ColByWhom, ROW, ColByWhomEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColByWhom, ROW, ColByWhomEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;


                SetHeaderTextTop(ref sheet, ROW, ColProductMasterHeader, "Product Master :", 12, ExcelHAlign.HAlignLeft);
                ColProductMasterHeader++;
                ColProductEnd = ColProductMasterHeader + 1;
                int ColProductMaster = ColProductMasterHeader;
                sheet.Range[ROW, ColProductMasterHeader, ROW, ColProductEnd].Text = data.Rows[0]["ProductMaster"].ToString();
                sheet.Range[ROW, ColProductMasterHeader, ROW, ColProductEnd].Merge();
                sheet.Range[ROW, ColProductMasterHeader, ROW, ColProductEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColProductMasterHeader, ROW, ColProductEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColProductEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColProductEnd, "Size Group :", 20, ExcelHAlign.HAlignLeft);
                ColProductEnd++;
                int ColSizeGroup = ColProductEnd;
                int ColSizeGroupEnd = ColProductEnd + 1;
                sheet.Range[ROW, ColSizeGroup, ROW, ColSizeGroupEnd].Text = data.Rows[0]["SizeGroup"].ToString();
                sheet.Range[ROW, ColSizeGroup, ROW, ColSizeGroupEnd].Merge();
                sheet.Range[ROW, ColSizeGroup, ROW, ColSizeGroupEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColSizeGroup, ROW, ColSizeGroupEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

            }


            DataView dvOperationGrup = new DataView(data);

            Dictionary<string, double> dist = new Dictionary<string, double>();

            DataTable dtOperationGroup = dvOperationGrup.ToTable(true, "Process");

            string ProcessName = "";
            int ColProcess = COL;
            COL++;
            int ColSequence = COL;
            COL++;
            int ColOperationVariation = COL;
            COL++;
            int ColMachineMaster = COL;
            COL++;
            int ColMachineVarient = COL;
            COL++;
            int ColSkill = COL;
            COL++;
            int ColTotalSPT = COL;
            COL++;
            int ColOperationGroup = COL;
            COL++;
            int ColAVGTotalTime = COL;
            COL++;
            int ColAllotedWorkstation = COL;
            COL++;
            int ColAllotedManpower = COL;
            COL++;
            int ColFrequency = COL;
            COL++;
            int ColFGZone = COL;
            COL++;
            int ColFGComponent = COL;
            COL++;
            int ColOperationType = COL;
            COL++;
            int ColOperationConsumption = COL;
            COL++;
            int ColGaugeFolder = COL;
            COL++;
            int ColOperationCategory = COL;
            COL++;
            int ColQualityLevel = COL;

            int RowIndex = ROW;
            int endRow;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                string name = data.Rows[i]["Process"].ToString();

                if (ProcessName != data.Rows[i]["Process"].ToString())
                {


                    int startRow = ROW;
                    endRow = ROW;
                    RowIndex = ROW;
                    startRow = ROW;


                    int nextProcessRow;
                    int endCurrentProcessRow;
                    int rowIndexCurrentProcessingRep = ROW;

                    nextProcessRow = ROW;
                    endCurrentProcessRow = nextProcessRow - 1;


                    RowIndex = ROW;

                    #region Headers
                    report.SetHeaderText(ref sheet, ROW, ColProcess, "Process", 12, ExcelHAlign.HAlignLeft);
                    report.SetHeaderText(ref sheet, ROW, ColSequence, "Sequence", 8, ExcelHAlign.HAlignRight);
                    report.SetHeaderText(ref sheet, ROW, ColOperationVariation, "Operation Variation", 15, ExcelHAlign.HAlignLeft);
                    report.SetHeaderText(ref sheet, ROW, ColMachineMaster, "Machine Master", 15, ExcelHAlign.HAlignLeft);
                    report.SetHeaderText(ref sheet, ROW, ColMachineVarient, "Machine Varient", 20, ExcelHAlign.HAlignLeft);
                    report.SetHeaderText(ref sheet, ROW, ColSkill, "Skill", 11, ExcelHAlign.HAlignLeft);
                    report.SetHeaderText(ref sheet, ROW, ColTotalSPT, "SPT", 10, ExcelHAlign.HAlignRight);
                    report.SetHeaderText(ref sheet, ROW, ColOperationGroup, "Operation Group", 8, ExcelHAlign.HAlignLeft);
                    report.SetHeaderText(ref sheet, ROW, ColAVGTotalTime, "AVG Total Time", 10, ExcelHAlign.HAlignRight);
                    report.SetHeaderText(ref sheet, ROW, ColAllotedWorkstation, "Alloted Workstation", 10, ExcelHAlign.HAlignRight);
                    report.SetHeaderText(ref sheet, ROW, ColAllotedManpower, "Alloted Manpower", 10, ExcelHAlign.HAlignRight);
                    report.SetHeaderText(ref sheet, ROW, ColFrequency, "Frequency", 10, ExcelHAlign.HAlignRight);
                    report.SetHeaderText(ref sheet, ROW, ColFGZone, "FGZone", 10, ExcelHAlign.HAlignLeft);
                    report.SetHeaderText(ref sheet, ROW, ColFGComponent, "FG Component", 12, ExcelHAlign.HAlignLeft);
                    report.SetHeaderText(ref sheet, ROW, ColOperationType, "Operation Type", 8, ExcelHAlign.HAlignLeft);
                    report.SetHeaderText(ref sheet, ROW, ColOperationConsumption, "Operation Consumption", 11, ExcelHAlign.HAlignLeft);
                    report.SetHeaderText(ref sheet, ROW, ColGaugeFolder, "Gauge Folder", 10, ExcelHAlign.HAlignLeft);
                    report.SetHeaderText(ref sheet, ROW, ColOperationCategory, "Operation Category", 8, ExcelHAlign.HAlignLeft);
                    report.SetHeaderText(ref sheet, ROW, ColQualityLevel, "Quality Level", 8, ExcelHAlign.HAlignLeft);

                    ROW++;
                    endCol = COL;
                    #endregion Headers



                    //if (RowIndex < ROW)
                    //{
                    //    sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].Merge();
                    //    sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //    sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //}
                }


                #region Rows
                sheet[ROW, ColAVGTotalTime].Number = clsStaticInfo.dbl(data.Rows[i]["AvgAllotedTime"].ToString());
                sheet[ROW, ColAVGTotalTime].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColSequence].Number = clsStaticInfo.dbl(data.Rows[i]["Sequence"].ToString());
                sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                sheet[ROW, ColOperationVariation].Text = data.Rows[i]["OperationVariation"].ToString();
                sheet[ROW, ColMachineMaster].Text = data.Rows[i]["MachineMaster"].ToString();
                sheet[ROW, ColMachineVarient].Text = data.Rows[i]["MachineVarient"].ToString();

                sheet[ROW, ColSkill].Text = data.Rows[i]["Skill"].ToString();
                sheet[ROW, ColOperationGroup].Text = data.Rows[i]["OperationGroup"].ToString();

                sheet.Range[ROW, ColTotalSPT].Number = Convert.ToDouble(data.Rows[i]["TotalSPT"].ToString());
                sheet.Range[ROW, ColTotalSPT].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[ROW, ColTotalSPT, ROW, ColTotalSPT].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, ColAllotedWorkstation].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedWorkstation"].ToString());

                sheet[ROW, ColAllotedManpower].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedManpower"].ToString());
                sheet[ROW, ColFrequency].Number = clsStaticInfo.dbl(data.Rows[i]["Frequency"].ToString());
                sheet[ROW, ColFGZone].Text = data.Rows[i]["FGZone"].ToString();
                sheet[ROW, ColFGComponent].Text = data.Rows[i]["FGComponent"].ToString();

                sheet[ROW, ColOperationType].Text = data.Rows[i]["OperationType"].ToString();
                sheet[ROW, ColOperationConsumption].Text = data.Rows[i]["OperationConsumption"].ToString();
                sheet[ROW, ColGaugeFolder].Text = data.Rows[i]["GaugeFolder"].ToString();
                sheet[ROW, ColOperationCategory].Text = data.Rows[i]["OperationCategory"].ToString();
                sheet[ROW, ColQualityLevel].Text = data.Rows[i]["QualityLevel"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                ProcessName = data.Rows[i]["Process"].ToString();

                ROW++;
                //if (ProcessName != "")
                //    ROW += 5;
                if (i < data.Rows.Count - 1)
                {
                    if (ProcessName != data.Rows[i + 1]["Process"].ToString())
                    {
                        sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].Merge();
                        sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                        int ColFormulaValue = ColProcess + 2;
                        int RowSummary = ROW;

                        sheet[ROW, ColProcess].Text = "Total";
                        sheet[ROW, ColTotalSPT].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTotalSPT) + RowIndex.ToString() + ":" + clsStaticInfo.GetxlsCol(ColTotalSPT) + (ROW - 1).ToString() + ")";
                        sheet[ROW, ColAllotedManpower].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColAllotedManpower) + RowIndex.ToString() + ":" + clsStaticInfo.GetxlsCol(ColAllotedManpower) + (ROW - 1).ToString() + ")";
                        sheet[ROW, ColAVGTotalTime].Formula = "Max(" + clsStaticInfo.GetxlsCol(ColAVGTotalTime) + RowIndex.ToString() + ":" + clsStaticInfo.GetxlsCol(ColAVGTotalTime) + (ROW - 1).ToString() + ")";
                        ROW++;

                        int RowPitchTime = ROW;
                        int RowProductionEffPHoure = ROW;
                        sheet[RowPitchTime, ColProcess].Text = "Pitch Time";
                        sheet[RowPitchTime, ColFormulaValue].Formula = clsStaticInfo.GetxlsCol(ColTotalSPT) + RowSummary + "/" + clsStaticInfo.GetxlsCol(ColAllotedManpower) + RowSummary;
                        ROW++;

                        int RowMaxAllotedTime = ROW;
                        int RowProductionEffPDay = ROW;
                        sheet[RowMaxAllotedTime, ColProcess].Text = "Max Of Alloted Time";
                        sheet[RowMaxAllotedTime, ColFormulaValue].Formula = clsStaticInfo.GetxlsCol(ColAVGTotalTime) + RowSummary;
                        ROW++;

                        int RowOrgEff = ROW;
                        int LinePHour = ROW;
                        sheet[RowOrgEff, ColProcess].Text = "Organization Efficiency";
                        sheet[RowOrgEff, ColFormulaValue].Formula = clsStaticInfo.GetxlsCol(ColFormulaValue) + RowPitchTime + "/" + clsStaticInfo.GetxlsCol(ColFormulaValue) + RowMaxAllotedTime;

                        int ColFormulaValue1 = ColMachineVarient + 2;
                        sheet.Range[RowProductionEffPHoure, ColMachineVarient, RowProductionEffPHoure, ColMachineVarient + 1].Text = "Production 100% Efficiency per Hour ";
                        sheet.Range[RowProductionEffPHoure, ColMachineVarient, RowProductionEffPHoure, ColMachineVarient + 1].Merge();
                        sheet[RowProductionEffPHoure, ColFormulaValue1].Formula = "(" + clsStaticInfo.GetxlsCol(ColAllotedManpower) + RowSummary + "*" + 60 + ")" + "/" + clsStaticInfo.GetxlsCol(ColTotalSPT) + RowSummary;

                        sheet.Range[RowProductionEffPDay, ColMachineVarient, RowProductionEffPDay, ColMachineVarient + 1].Text = "Production 100% Efficiency per Day ";
                        sheet.Range[RowProductionEffPDay, ColMachineVarient, RowProductionEffPDay, ColMachineVarient + 1].Merge();
                        sheet[RowProductionEffPDay, ColFormulaValue1].Formula = clsStaticInfo.GetxlsCol(ColFormulaValue1) + RowProductionEffPHoure + "*" + data.Rows[i]["PlannedHoursPerDay"].ToString();

                        sheet.Range[LinePHour, ColMachineVarient, LinePHour, ColMachineVarient + 1].Text = "Line Target per Hour ";
                        sheet.Range[LinePHour, ColMachineVarient, LinePHour, ColMachineVarient + 1].Merge();
                        sheet[LinePHour, ColFormulaValue1].Formula = clsStaticInfo.GetxlsCol(ColFormulaValue1) + RowProductionEffPHoure + "*" + clsStaticInfo.GetxlsCol(ColFormulaValue) + RowOrgEff;


                        ROW += 3;
                    }

                }
                else if (i == data.Rows.Count - 1)
                {
                    sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].Merge();
                    sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    int ColFormulaValue = ColProcess + 2;
                    int RowSummary = ROW;

                    sheet[ROW, ColProcess].Text = "Total";
                    sheet[ROW, ColTotalSPT].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTotalSPT) + RowIndex.ToString() + ":" + clsStaticInfo.GetxlsCol(ColTotalSPT) + (ROW - 1).ToString() + ")";
                    sheet[ROW, ColAllotedManpower].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColAllotedManpower) + RowIndex.ToString() + ":" + clsStaticInfo.GetxlsCol(ColAllotedManpower) + (ROW - 1).ToString() + ")";
                    sheet[ROW, ColAVGTotalTime].Formula = "Max(" + clsStaticInfo.GetxlsCol(ColAVGTotalTime) + RowIndex.ToString() + ":" + clsStaticInfo.GetxlsCol(ColAVGTotalTime) + (ROW - 1).ToString() + ")";
                    ROW++;

                    int RowPitchTime = ROW;
                    int RowProductionEffPHoure = ROW;
                    sheet[RowPitchTime, ColProcess].Text = "Pitch Time";
                    sheet[RowPitchTime, ColFormulaValue].Formula = clsStaticInfo.GetxlsCol(ColTotalSPT) + RowSummary + "/" + clsStaticInfo.GetxlsCol(ColAllotedManpower) + RowSummary;
                    ROW++;

                    int RowMaxAllotedTime = ROW;
                    int RowProductionEffPDay = ROW;
                    sheet[RowMaxAllotedTime, ColProcess].Text = "Max Of Alloted Time";
                    sheet[RowMaxAllotedTime, ColFormulaValue].Formula = clsStaticInfo.GetxlsCol(ColAVGTotalTime) + RowSummary;
                    ROW++;

                    int RowOrgEff = ROW;
                    int LinePHour = ROW;
                    sheet[RowOrgEff, ColProcess].Text = "Organization Efficiency";
                    sheet[RowOrgEff, ColFormulaValue].Formula = clsStaticInfo.GetxlsCol(ColFormulaValue) + RowPitchTime + "/" + clsStaticInfo.GetxlsCol(ColFormulaValue) + RowMaxAllotedTime;

                    int ColFormulaValue1 = ColMachineVarient + 2;
                    sheet.Range[RowProductionEffPHoure, ColMachineVarient, RowProductionEffPHoure, ColMachineVarient + 1].Text = "Production 100% Efficiency per Hour ";
                    sheet.Range[RowProductionEffPHoure, ColMachineVarient, RowProductionEffPHoure, ColMachineVarient + 1].Merge();
                    sheet[RowProductionEffPHoure, ColFormulaValue1].Formula = "(" + clsStaticInfo.GetxlsCol(ColAllotedManpower) + RowSummary + "*" + 60 + ")" + "/" + clsStaticInfo.GetxlsCol(ColTotalSPT) + RowSummary;

                    sheet.Range[RowProductionEffPDay, ColMachineVarient, RowProductionEffPDay, ColMachineVarient + 1].Text = "Production 100% Efficiency per Day ";
                    sheet.Range[RowProductionEffPDay, ColMachineVarient, RowProductionEffPDay, ColMachineVarient + 1].Merge();
                    sheet[RowProductionEffPDay, ColFormulaValue1].Formula = clsStaticInfo.GetxlsCol(ColFormulaValue1) + RowProductionEffPHoure + "*" + data.Rows[i]["PlannedHoursPerDay"].ToString();

                    sheet.Range[LinePHour, ColMachineVarient, LinePHour, ColMachineVarient + 1].Text = "Line Target per Hour ";
                    sheet.Range[LinePHour, ColMachineVarient, LinePHour, ColMachineVarient + 1].Merge();
                    sheet[LinePHour, ColFormulaValue1].Formula = clsStaticInfo.GetxlsCol(ColFormulaValue1) + RowProductionEffPHoure + "*" + clsStaticInfo.GetxlsCol(ColFormulaValue) + RowOrgEff;


                    ROW += 3;
                }
                #endregion Rows
            }

            endRow = ROW - 1;


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, endCol, "Bulletin Tamplate", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
        }
        private IWorkbook GetBulletinTamplateReportWorkSheet(string bulletinTemplateId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            var sheet1 = workbook.Worksheets[1];
            var sheet2 = workbook.Worksheets[2];

            sheet.Name = "BulletinTemplate";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetBullatinTamplateReportDataByBullatinTamplateId(bulletinTemplateId);
            if (data.Rows.Count > 0)
            {
                int ColBulletinNameHeader = 1;
                int ColBulletinNameEnd;
                int ColByWhomHeader;
                int ColByWhomEnd;
                int ColByWhom;
                int ColProductMasterHeader = 1;
                int ColProductEnd;


                SetHeaderTextTop(ref sheet, ROW, ColBulletinNameHeader, "Bulletin Name", 12, ExcelHAlign.HAlignLeft);
                ColBulletinNameHeader++;
                ColBulletinNameEnd = ColBulletinNameHeader + 1;
                sheet.Range[ROW, ColBulletinNameHeader, ROW, ColBulletinNameEnd].Text = data.Rows[0]["BulletinName"].ToString();
                sheet.Range[ROW, ColBulletinNameHeader, ROW, ColBulletinNameEnd].Merge();
                sheet.Range[ROW, ColBulletinNameHeader, ROW, ColBulletinNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColBulletinNameHeader, ROW, ColBulletinNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColBulletinNameEnd++;

                ColByWhomHeader = ColBulletinNameEnd;
                SetHeaderTextTop(ref sheet, ROW, ColByWhomHeader, "ByWhom", 20, ExcelHAlign.HAlignLeft);
                ColByWhomHeader++;
                ColByWhomEnd = ColByWhomHeader + 1;
                ColByWhom = ColByWhomHeader;
                sheet.Range[ROW, ColByWhom, ROW, ColByWhomEnd].Text = data.Rows[0]["ByWhom"].ToString();
                sheet.Range[ROW, ColByWhom, ROW, ColByWhomEnd].Merge();
                sheet.Range[ROW, ColByWhom, ROW, ColByWhomEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColByWhom, ROW, ColByWhomEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;


                SetHeaderTextTop(ref sheet, ROW, ColProductMasterHeader, "Product Master", 12, ExcelHAlign.HAlignLeft);
                ColProductMasterHeader++;
                ColProductEnd = ColProductMasterHeader + 1;
                int ColProductMaster = ColProductMasterHeader;
                sheet.Range[ROW, ColProductMasterHeader, ROW, ColProductEnd].Text = data.Rows[0]["ProductMaster"].ToString();
                sheet.Range[ROW, ColProductMasterHeader, ROW, ColProductEnd].Merge();
                sheet.Range[ROW, ColProductMasterHeader, ROW, ColProductEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColProductMasterHeader, ROW, ColProductEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColProductEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColProductEnd, "Size Group", 20, ExcelHAlign.HAlignLeft);
                ColProductEnd++;
                int ColSizeGroup = ColProductEnd;
                int ColSizeGroupEnd = ColProductEnd + 1;
                sheet.Range[ROW, ColSizeGroup, ROW, ColSizeGroupEnd].Text = data.Rows[0]["SizeGroup"].ToString();
                sheet.Range[ROW, ColSizeGroup, ROW, ColSizeGroupEnd].Merge();
                sheet.Range[ROW, ColSizeGroup, ROW, ColSizeGroupEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColSizeGroup, ROW, ColSizeGroupEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

            }

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Process", 12, ExcelHAlign.HAlignLeft);
            int ColProcess = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sequence", 8, ExcelHAlign.HAlignRight);
            int ColSequence = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Operation Variation", 15, ExcelHAlign.HAlignLeft);
            int ColOperationVariation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Machine Master", 15, ExcelHAlign.HAlignLeft);
            int ColMachineMaster = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Machine Varient", 26, ExcelHAlign.HAlignLeft);
            int ColMachineVarient = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Skill", 11, ExcelHAlign.HAlignLeft);
            int ColSkill = COL;
            COL++;


            //report.SetHeaderText(ref sheet, ROW, COL, "Additional SPT", 11, ExcelHAlign.HAlignRight);
            //int ColAdditionalSPT = COL;
            //COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "SPT", 10, ExcelHAlign.HAlignRight);
            int ColTotalSPT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Operation Group", 8, ExcelHAlign.HAlignLeft);
            int ColOperationGroup = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "AVG Total Time", 10, ExcelHAlign.HAlignRight);

            int ColAVGTotalTime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Alloted Workstation", 10, ExcelHAlign.HAlignRight);
            int ColAllotedWorkstation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Alloted Manpower", 10, ExcelHAlign.HAlignRight);
            int ColAllotedManpower = COL;
            COL++;



            report.SetHeaderText(ref sheet, ROW, COL, "Frequency", 10, ExcelHAlign.HAlignRight);
            int ColFrequency = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "FGZone", 10, ExcelHAlign.HAlignLeft);
            int ColFGZone = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "FG Component", 12, ExcelHAlign.HAlignLeft);
            int ColFGComponent = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Operation Type", 8, ExcelHAlign.HAlignLeft);
            int ColOperationType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Operation Consumption", 11, ExcelHAlign.HAlignLeft);
            int ColOperationConsumption = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Gauge Folder", 10, ExcelHAlign.HAlignLeft);
            int ColGaugeFolder = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Operation Category", 8, ExcelHAlign.HAlignLeft);
            int ColOperationCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Quality Level", 8, ExcelHAlign.HAlignLeft);
            int ColQualityLevel = COL;
            ROW++;
            endCol = COL;
            #endregion Headers


            DataView dvOperationGrup = new DataView(data);

            Dictionary<string, double> dist = new Dictionary<string, double>();

            DataTable dtOperationGroup = dvOperationGrup.ToTable(true, "OperationGroup");

            string ProcessName = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                if (ProcessName != data.Rows[i]["Process"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                        sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].Merge();
                        sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    RowIndex = ROW;
                }

                sheet[ROW, ColAVGTotalTime].Number = clsStaticInfo.dbl(data.Rows[i]["AvgAllotedTime"].ToString());
                sheet[ROW, ColAVGTotalTime].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColSequence].Number = clsStaticInfo.dbl(data.Rows[i]["Sequence"].ToString());
                sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                sheet[ROW, ColOperationVariation].Text = data.Rows[i]["OperationVariation"].ToString();
                sheet[ROW, ColMachineMaster].Text = data.Rows[i]["MachineMaster"].ToString();
                sheet[ROW, ColMachineVarient].Text = data.Rows[i]["MachineVarient"].ToString();

                sheet[ROW, ColSkill].Text = data.Rows[i]["Skill"].ToString();
                sheet[ROW, ColOperationGroup].Text = data.Rows[i]["OperationGroup"].ToString();

                sheet.Range[ROW, ColTotalSPT].Number = Convert.ToDouble(data.Rows[i]["TotalSPT"].ToString());
                sheet.Range[ROW, ColTotalSPT].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[ROW, ColTotalSPT, ROW, ColTotalSPT].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, ColAllotedWorkstation].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedWorkstation"].ToString());

                sheet[ROW, ColAllotedManpower].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedManpower"].ToString());
                sheet[ROW, ColFrequency].Number = clsStaticInfo.dbl(data.Rows[i]["Frequency"].ToString());
                sheet[ROW, ColFGZone].Text = data.Rows[i]["FGZone"].ToString();
                sheet[ROW, ColFGComponent].Text = data.Rows[i]["FGComponent"].ToString();

                sheet[ROW, ColOperationType].Text = data.Rows[i]["OperationType"].ToString();
                sheet[ROW, ColOperationConsumption].Text = data.Rows[i]["OperationConsumption"].ToString();
                sheet[ROW, ColGaugeFolder].Text = data.Rows[i]["GaugeFolder"].ToString();
                sheet[ROW, ColOperationCategory].Text = data.Rows[i]["OperationCategory"].ToString();
                sheet[ROW, ColQualityLevel].Text = data.Rows[i]["QualityLevel"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                ProcessName = data.Rows[i]["Process"].ToString();

                ROW++;
            }

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
                sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].Merge();
                sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }

            GetWorkSheetBulletinTamplateCalculation(ref sheet1, ref report, data, "Bulletin Tamplate Calculation");
            GetWorkSheetTamplateFormula(ref sheet2, ref report, data, "Bulletin Tamplate Calculation Formula");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyHeader(ref sheet, endCol, "Bulletin Tamplate", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private DataTable GetBullatinTamplateReportDataByBullatinTamplateId(string bulletinTemplateId)
        {
            var sql = @"SELECT BT.Id,BT.ByWhom,PM.UserName AS ProductMaster,SG.UserName AS SizeGroup, BT.BulletinName,BTD.Sequence, p.UserName As Process,OPV.Code OperationCode,OPV.UserName AS OperationVariation,ISNULL(MM.UserName,'Manual') AS MachineMaster, MMA.StandardName AS MachineVarient, S.UserName AS Skill
                ,BTD.OperationGroup,BTD.AdditionalSPT,ISNULL(BTD.TotalSPT,0) as TotalSPT,ISNULL(BTD.AllotedWorkstation,0) as AllotedWorkstation,ISNULL(BTD.AllotedManpower,0) as AllotedManpower,BTD.Frequency
                ,FZ.UserName AS FGZone, fgc.UserName AS FGComponent,isnull(BTD.AvgAllotedTime,0) AS AvgAllotedTime
                ,OT.UserName AS OperationType, OC.UserName AS OperationConsumption, GF.UserName AS GaugeFolder, OCategory.UserName AS OperationCategory,BTD.QualityLevel,BM.PlannedHoursPerDay,BM.RequiredStdTarget, TotalBT=BM.PlannedHoursPerDay*BM.RequiredStdTarget
                ,MMA.Code MachineCode,BTD.OperationTargetPerHr,BTD.RequiredManPower,BM.ProcessId,BT.PicFileName 
                ,Buyer=REPLACE(REPLACE(
							STUFF((select distinct ', '+B.UserName FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
										JOIN HKP.Buyer B ON B.Id=BTB.BuyerId
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	

                ,BuyerStyleRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+BTB.BuyerStyleRefNo FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')


                ,OwnStyleRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+BTB.OwnStyleRefNo FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                ,OperationSPT=BTD.TotalSPT-BTD.AdditionalSPT,MMA.Id MachineVarientId,ShortName=CASE WHEN MMA.ShortName IS NULL THEN 'Manual' ELSE MMA.ShortName END, Machine=CASE WHEN MMA.ShortName IS NULL THEN 'No' ELSE 'Yes' END
                ,ATH.UserName Attachment,BTD.Remark,BT.AddedBy,FORMAT(BT.AddedDate,'dd-MMM-yyyy') AddedDate
                FROM [MST].[BulletinTemplate] BT 
                LEFT JOIN [MST].[BulletinTemplateMaster] BM ON BT.Id = BM.BulletinTemplateId
                LEFT JOIN [MST].[BulletinTemplateDetail] BTD ON BM.Id = BTD.BulletinTemplateMasterId
                LEFT JOIN HKP.Process p ON p.Id = BM.ProcessId 
                LEFT JOIN MST.OperationVariation OPV ON OPV.Id = BTD.OperationVariationId 
                LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id = BTD.MachineVarientId
                LEFT JOIN HKP.FGZone FZ ON FZ.Id = BTD.FGZoneId 
                LEFT JOIN HKP.FGComponent FGC ON FGC.Id = BTD.FGComponentId
                LEFT JOIN HKP.OperationType OT ON OT.Id = BTD.OperationTypeId
                LEFT JOIN HKP.OperationConsumption OC ON OC.Id = BTD.OperationConsumptionId
                LEFT JOIN HKP.GaugeFolder GF ON GF.Id = BTD.GaugeFolderId
                LEFT JOIN HKP.OperationCategory OCategory ON OCategory.Id = BTD.OperationCategoryId
                LEFT JOIN MST.ProductMaster PM ON PM.Id = BT.ProductMasterId
                LEFT JOIN HKP.SizeGroup SG ON SG.Id = BT.SizeGroupId
                LEFT JOIN mst.OperationMaster AS om ON om.Id=BTD.SkillMasterId 
                LEFT JOIN HKP.Skill S ON S.Id = OM.SkillId
                LEFT JOIN MST.MaterialMaster MM ON MM.Id = MMA.MaterialMasterId
                LEFT JOIN HKP.Attachment ATH ON ATH.Id = BTD.AttachmentId
                WHERE BT.Id = '" + bulletinTemplateId + "' ORDER BY P.UserName,BTD.Sequence";

            return _sqlRepository.GetDataTable(sql);
        }
        private void GetWorkSheetBulletinTamplateCalculation(ref IWorksheet sheet, ref ReportUtility report, DataTable data, string sheetTitle)
        {

            sheet.Name = sheetTitle;


            int ROW = 6;
            int LeftColCaption = 1;
            int LeftColValue = 2;

            double processWith = 21;
            int RighColCaption = 4;
            int RightColValue = 5;
            double rowHeight = 16;
            double colLeftWidth = 20;
            double colRightWidth = 32;
            double colLeftValueWidth = 17;
            double colRightValueWidth = 17;


            DataTable dtDistinct = data.DefaultView.ToTable(true, "Process");
            int rowStartForBorderProcessWise = ROW;
            for (int i = 0; i < dtDistinct.Rows.Count; i++)
            {
                rowStartForBorderProcessWise = ROW;
                sheet.Range[ROW, 1, ROW, 5].Text = dtDistinct.Rows[i]["Process"].ToString();
                sheet.Range[ROW, 1, ROW, 5].Merge();
                sheet.Range[ROW, 1, ROW, 5].RowHeight = processWith;
                sheet.Range[ROW, 1, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[ROW, 1, ROW, 5].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, 1, ROW, 5].CellStyle.Interior.Color = System.Drawing.Color.LightBlue;
                sheet.Range[ROW, 1, ROW, 5].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, 5].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, 5].BorderAround(ExcelLineStyle.Hair);
                //sheet.Range[ROW, 1, ROW, 5].CellStyle.Font.Color = ExcelKnownColors.White;

                ROW++;
                double plannedHourPerDay = Convert.ToDouble(data.Rows[i]["PlannedHoursPerDay"]);
                double TotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Process='" + dtDistinct.Rows[i]["PRocess"].ToString() + "'").ToString());
                double TotalManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Process='" + dtDistinct.Rows[i]["PRocess"].ToString() + "'").ToString());
                double MaxAllotedTime = clsStaticInfo.dbl(data.Compute("Max(AvgAllotedTime)", "Process='" + dtDistinct.Rows[i]["PRocess"].ToString() + "'").ToString());

                double PitchTime = 0;
                if (TotalManpower != 0)
                    PitchTime = TotalSPT / TotalManpower;

                double OrgEfficiency = 0;
                if (MaxAllotedTime != 0)
                    OrgEfficiency = PitchTime / MaxAllotedTime;

                double ProdEffPerHour = 0;
                if (TotalSPT != 0)
                    ProdEffPerHour = TotalManpower * 60 / TotalSPT;

                double ProdEffPerDay = ProdEffPerHour * plannedHourPerDay;
                double LineTargetPerHour = ProdEffPerHour * OrgEfficiency;

                int StartRow = ROW;

                sheet[ROW, LeftColCaption].Text = "Pitch Time";
                sheet[ROW, LeftColCaption].RowHeight = rowHeight;
                sheet[ROW, LeftColCaption].ColumnWidth = colLeftWidth;
                sheet[ROW, LeftColValue].Number = PitchTime;
                sheet[ROW, LeftColValue].ColumnWidth = colLeftValueWidth;
                ROW++;

                sheet[ROW, LeftColCaption].Text = "Max Allotted Time";
                sheet[ROW, LeftColCaption].RowHeight = rowHeight;
                sheet[ROW, LeftColCaption].ColumnWidth = colLeftWidth;
                sheet[ROW, LeftColValue].Number = MaxAllotedTime;
                sheet[ROW, LeftColValue].ColumnWidth = colLeftValueWidth;
                ROW++;

                sheet[ROW, LeftColCaption].Text = "Organization Efficiency";
                sheet[ROW, LeftColCaption].RowHeight = rowHeight;
                sheet[ROW, LeftColCaption].ColumnWidth = colLeftWidth;
                sheet[ROW, LeftColValue].Number = OrgEfficiency;
                sheet[ROW, LeftColValue].ColumnWidth = colLeftValueWidth;
                ROW++;

                ROW = StartRow;
                //right side starts from here
                sheet[ROW, RighColCaption].Text = "Production 100% Efficiency Per Hour";
                sheet[ROW, RighColCaption].RowHeight = rowHeight;
                sheet[ROW, RighColCaption].ColumnWidth = colRightWidth;
                sheet[ROW, RightColValue].Number = ProdEffPerHour;
                sheet[ROW, RighColCaption].ColumnWidth = 30;
                sheet[ROW, RightColValue].ColumnWidth = colRightValueWidth;
                ROW++;

                sheet[ROW, RighColCaption].Text = "Production 100% Efficiency Per Hour";
                sheet[ROW, RighColCaption].RowHeight = rowHeight;
                sheet[ROW, RighColCaption].ColumnWidth = colRightWidth;
                sheet[ROW, RightColValue].Number = ProdEffPerHour;
                sheet[ROW, RighColCaption].ColumnWidth = 30;
                sheet[ROW, RightColValue].ColumnWidth = colRightValueWidth;
                ROW++;

                sheet[ROW, RighColCaption].Text = "Production 100% Efficiency Per Day";
                sheet[ROW, RighColCaption].RowHeight = rowHeight;
                sheet[ROW, RighColCaption].ColumnWidth = colRightWidth;
                sheet[ROW, RightColValue].Number = ProdEffPerDay;
                sheet[ROW, RighColCaption].ColumnWidth = 30;
                sheet[ROW, RightColValue].ColumnWidth = colRightValueWidth;
                ROW++;

                int endRowForBorderProcessWise = ROW - 1;
                sheet.Range[rowStartForBorderProcessWise, 1, endRowForBorderProcessWise, RightColValue].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[rowStartForBorderProcessWise, 1, endRowForBorderProcessWise, RightColValue].BorderInside(ExcelLineStyle.Hair);
            }


            int endCol = RightColValue;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, endCol, sheetTitle, identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
        }
        private void GetWorkSheetTamplateFormula(ref IWorksheet sheet, ref ReportUtility report, DataTable data, string sheetTitle)
        {

            sheet.Name = sheetTitle;


            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            int ColLeftCaption = COL;
            int ColLeftValue = 2;

            int ColRightCaption = 4;
            int ColRightValue = 5;

            double ColLeftCaptionWidth = 20;
            double ColLeftValueWidth = 30;
            double ColRightCaptionWidth = 33;
            double ColRightValueWidth = 46;
            double rowHeight = 15;

            int RowRight = ROW;
            sheet[ROW, ColLeftCaption].Text = "Pitch Time";
            sheet[ROW, ColLeftCaption].RowHeight = rowHeight;
            sheet[ROW, ColLeftCaption].ColumnWidth = ColLeftCaptionWidth;
            sheet[ROW, ColLeftCaption].CellStyle.Font.Bold = true;
            sheet[ROW, ColLeftValue].Text = "Total SPT / Total Manpower";
            sheet[ROW, ColLeftValue].ColumnWidth = ColLeftValueWidth;
            ROW++;

            sheet[ROW, ColLeftCaption].Text = "Max Allotted Time";
            sheet[ROW, ColLeftCaption].RowHeight = rowHeight;
            sheet[ROW, ColLeftCaption].ColumnWidth = ColLeftCaptionWidth;
            sheet[ROW, ColLeftCaption].CellStyle.Font.Bold = true;
            sheet[ROW, ColLeftValue].Text = "Max of Avg Allotted Time (based on group)";
            sheet[ROW, ColLeftValue].ColumnWidth = ColLeftValueWidth;
            ROW++;

            sheet[ROW, ColLeftCaption].Text = "Organization Efficiency";
            sheet[ROW, ColLeftCaption].RowHeight = rowHeight;
            sheet[ROW, ColLeftCaption].ColumnWidth = ColLeftCaptionWidth;
            sheet[ROW, ColLeftCaption].CellStyle.Font.Bold = true;
            sheet[ROW, ColLeftValue].Text = "Pith Time / Max Allotted Time";
            sheet[ROW, ColLeftValue].ColumnWidth = ColLeftValueWidth;

            //right data here 
            sheet[RowRight, ColRightCaption].Text = "Production 100% Efficiency Per Hour";
            sheet[RowRight, ColRightCaption].ColumnWidth = ColRightCaptionWidth;
            sheet[RowRight, ColRightCaption].CellStyle.Font.Bold = true;
            sheet[RowRight, ColRightValue].Text = "(Total Manpower * 60) / Total SPT";
            sheet[RowRight, ColRightValue].ColumnWidth = ColRightValueWidth;
            sheet.Range[RowRight, ColLeftCaption, RowRight, ColRightValue].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[RowRight, ColLeftCaption, RowRight, ColRightValue].BorderInside(ExcelLineStyle.Hair);
            RowRight++;

            sheet[RowRight, ColRightCaption].Text = "Production 100% Efficiency Per Day";
            sheet[RowRight, ColRightCaption].ColumnWidth = ColRightCaptionWidth;
            sheet[RowRight, ColRightCaption].CellStyle.Font.Bold = true;
            sheet[RowRight, ColRightValue].Text = "Production at 100% Efficiency Per Hour * Planned Hours per Day";
            sheet[RowRight, ColRightValue].ColumnWidth = ColRightValueWidth;
            sheet.Range[RowRight, ColLeftCaption, RowRight, ColRightValue].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[RowRight, ColLeftCaption, RowRight, ColRightValue].BorderInside(ExcelLineStyle.Hair);
            RowRight++;

            sheet[RowRight, ColRightCaption].Text = "Line Target Per Hour";
            sheet[RowRight, ColRightCaption].ColumnWidth = ColRightCaptionWidth;
            sheet[RowRight, ColRightCaption].CellStyle.Font.Bold = true;
            sheet[RowRight, ColRightValue].Text = "Production at 100% Efficiency Per Hour * Organization Efficiency";
            sheet[RowRight, ColRightValue].ColumnWidth = ColRightValueWidth;
            sheet.Range[RowRight, ColLeftCaption, RowRight, ColRightValue].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[RowRight, ColLeftCaption, RowRight, ColRightValue].BorderInside(ExcelLineStyle.Hair);
            RowRight++;

            endCol = ColRightValue;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, endCol, sheetTitle, identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
        }

        [HttpGet, Authorize]
        public ActionResult GetBulletinTamplateSummaryReport(ReportFormat reportFormat, string bulletinTemplateId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Bulletin Template Summary - " + bulletinTemplateId + "";
            var workbook = GetBulletinTamplateSummaryReportWorkSheet(bulletinTemplateId);
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

        private IWorkbook GetBulletinTamplateSummaryReportWorkSheet(string bulletinTemplateId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();

            DataTable data = GetBullatinTamplateReportDataByBullatinTamplateId(bulletinTemplateId);
            DataTable dtProcess = new DataView(data).ToTable(true, "Process", "ProcessId");
            var workbook = report.GetWorkbook(ref excelEngine, dtProcess.Rows.Count);
            workbook.Version = ExcelVersion.Excel2016;

            for (int i = 0; i < dtProcess.Rows.Count; i++)
            {
                DataView dv = new DataView(data);
                dv.RowFilter = "ProcessId='" + dtProcess.Rows[i]["ProcessId"].ToString() + "'";

                var sheet = workbook.Worksheets[i];

                CreateSheet(dtProcess.Rows[i]["Process"].ToString(), dv.ToTable(), ref sheet, identity.CompanyId);
            }

            return workbook;
        }


        [HttpGet, Authorize]
        public ActionResult GetBulletinTamplateDetailReport(ReportFormat reportFormat, string bulletinTemplateId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Bulletin Template Detail - " + bulletinTemplateId + "";
            var workbook = GetBulletinTamplateDetailReportWorkSheet(bulletinTemplateId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);

                default:
                    return RenderReportAsExcelx(workbook, reportFileName);
            }
        }

        private IWorkbook GetBulletinTamplateDetailReportWorkSheet(string bulletinTemplateId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();

            DataTable data = GetBullatinTamplateReportDataByBullatinTamplateId(bulletinTemplateId);
            DataTable dtProcess = new DataView(data).ToTable(true, "Process", "ProcessId");
            var workbook = report.GetWorkbook(ref excelEngine, dtProcess.Rows.Count);
            workbook.Version = ExcelVersion.Excel2016;



            for (int i = 0; i < dtProcess.Rows.Count; i++)
            {
                DataView dv = new DataView(data);
                dv.RowFilter = "ProcessId='" + dtProcess.Rows[i]["ProcessId"].ToString() + "'";

                var sheet = workbook.Worksheets[i];

                CreateDetailSheet(dtProcess.Rows[i]["Process"].ToString(), dv.ToTable(), ref sheet, identity.CompanyId);
            }

            return workbook;
        }

        void CreateSheet(string SheetName, DataTable data, ref IWorksheet sheet, string companyId)
        {
            try
            {
                var report = new ReportUtility();
                sheet.Name = SheetName;
                int colPitchTime = 0;
                int rowPitchTime = 0;
                int ROW = 6;
                int endCol = 1;
                int COL = 5;
                string ImageExt = Path.GetExtension(data.Rows[0]["PicFileName"].ToString());
                string IdImage = data.Rows[0]["Id"].ToString();
                #region Image
                try
                {
                    string strPath = Path.Combine(ResourcesPathReader.GetBulletinImagePath(), IdImage + ImageExt);
                    Image companyLogo = Image.FromFile(strPath);
                    if (companyLogo != null)
                    {
                        double totalWidth = sheet.GetColumnWidth(1) + sheet.GetColumnWidth(28);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet.GetRowHeight(1) + sheet.GetRowHeight(2) + sheet.GetRowHeight(36) + sheet.GetRowHeight(36)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet.Pictures.AddPicture(6, 2, companyLogo);


                    }


                }
                catch (Exception)
                {


                }
                #endregion

                sheet.Range[6, 2, 12, 4].BorderAround(ExcelLineStyle.Double);

                #region Headers
                int rws = ROW;
                sheet.Range[ROW, COL].Text = "Buyer Name";
                sheet.Range[ROW, COL + 1].Text = " " + data.Rows[0]["Buyer"].ToString().Trim();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;
                sheet.Range[ROW, COL].Text = "Buyer Style Ref No";
                sheet.Range[ROW, COL + 1].Text = " " + data.Rows[0]["BuyerStyleRefNo"].ToString().Trim();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;
                sheet.Range[ROW, COL].Text = "Own Style Ref No";
                sheet.Range[ROW, COL + 1].Text = " " + data.Rows[0]["OwnStyleRefNo"].ToString().Trim();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;
                sheet.Range[ROW, COL].Text = "Product Master";
                sheet.Range[ROW, COL + 1].Text = " " + data.Rows[0]["ProductMaster"].ToString().Trim();
                sheet.Range[ROW, COL, ROW, COL + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();

                double plannedHourPerDay = Convert.ToDouble(data.Rows[0]["PlannedHoursPerDay"]);
                double TotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", null));
                double TotalManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", null));
                double TotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", null));
                double MaxAllotedTime = clsStaticInfo.dbl(data.Compute("Max(AvgAllotedTime)", null));
                double TotalRMP = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", null));

                double PitchTime = 0;
                if (TotalManpower != 0)
                    PitchTime = TotalSPT / TotalManpower;

                double OrgEfficiency = 0;
                if (MaxAllotedTime != 0)
                    OrgEfficiency = PitchTime / MaxAllotedTime;

                double ProdEffPerHour = 0;
                if (TotalSPT != 0)
                    ProdEffPerHour = TotalManpower * 60 / TotalSPT;

                double ProdEffPerDay = ProdEffPerHour * plannedHourPerDay;
                double LineTargetPerHour = ProdEffPerHour * OrgEfficiency;


                ROW++;
                sheet.Range[ROW, COL].Text = "Pitch Time(Minutes)";
                //sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 1].Number = PitchTime;
                colPitchTime = COL + 1;
                rowPitchTime = ROW;

                sheet.Range[ROW, COL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[ROW, COL + 1, ROW, COL + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL + 1, ROW, COL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();

                ROW++;
                sheet.Range[ROW, COL].Text = "Planned Working Hour PerDay";
                //sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 1].Number = plannedHourPerDay;
                sheet.Range[ROW, COL + 1].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[ROW, COL, ROW, COL + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL, ROW, COL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                sheet.Range[6, 5, 12, 7].BorderAround(ExcelLineStyle.Thick);


                int rwe = 6;
                int PCOL = 8;
                sheet.Range[rws, PCOL].Text = "Particulars";
                sheet.Range[rws, PCOL + 1].Text = "SPT(Minutes)";
                sheet.Range[rws, PCOL + 2].Text = "MP";
                sheet.Range[rws, PCOL + 3].Text = "Work Station";
                sheet.Range[rws, PCOL + 4].Text = "Bullitin Target(Pcs)";
                sheet.Range[rws, PCOL + 5].Text = "Planned Efficency(%)";
                sheet.Range[rws, PCOL + 6].Text = "Planned Per Man productivity(Pcs)";

                sheet.Range[6, 12, 12, 14].BorderAround(ExcelLineStyle.Thick);
                sheet.Range[rws, PCOL, rws, PCOL + 6].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL + 6].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 7].Text = "Target(%)";
                sheet.Range[rws, PCOL + 7].ColumnWidth = 9;
                sheet.Range[rws, PCOL + 8].Text = "Per Hr";
                sheet.Range[rws, PCOL + 9].Text = "Per Day";

                sheet.Range[rws, PCOL + 7, rws, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 7, rws, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[6, 15, 12, 17].BorderAround(ExcelLineStyle.Thick);

                sheet.Range[rws, PCOL + 10].Text = "Created By";
                sheet.Range[rws, PCOL + 10].ColumnWidth = 9;
                sheet.Range[rws, PCOL + 10, rws, PCOL + 10].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 11].Text = "Creation Date";
                sheet.Range[rws, PCOL + 11].ColumnWidth = 9;
                sheet.Range[rws, PCOL + 11, rws, PCOL + 11].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 12].Text = "Revision";
                sheet.Range[rws, PCOL + 12, rws, PCOL + 12].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 13].Text = "Approved By";
                sheet.Range[rws, PCOL + 13].ColumnWidth = 12;
                sheet.Range[rws, PCOL + 13, rws, PCOL + 13].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[6, 18, 12, 21].BorderAround(ExcelLineStyle.Thick);

                rws++;
                sheet.Range[rws, PCOL].Text = "Non MC";
                sheet.Range[rws, PCOL, rws, PCOL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                double NMCTotalSPT = 0;
                double NMCTotalWS = 0;
                double NMCTotalMP = 0;

                NMCTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='No'"));
                NMCTotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='No'"));
                NMCTotalMP = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='No'"));

                sheet.Range[rws, PCOL + 1].Number = NMCTotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 2].Number = NMCTotalMP;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 3].Number = NMCTotalWS;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                rws++;
                sheet.Range[rws, PCOL].Text = "MC";
                sheet.Range[rws, PCOL, rws, PCOL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                double MCTotalSPT = 0;
                double MCTotalWS = 0;
                double MCTotalMP = 0;

                MCTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='Yes'"));
                MCTotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='Yes'"));
                MCTotalMP = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='Yes'"));

                sheet.Range[rws, PCOL + 1].Number = MCTotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 2].Number = MCTotalMP;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 3].Number = MCTotalWS;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                rws++;
                sheet.Range[rws, PCOL].Text = "Total";
                sheet.Range[rws, PCOL, rws, PCOL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 1].Number = TotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 2].Number = TotalManpower;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 3].Number = TotalWS;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, 8, rws, 11].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[rws, 8, rws, 11].CellStyle.Font.Bold = true;
                sheet.Range[rws, 8, rws, 11].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[6, 8, 12, 11].BorderAround(ExcelLineStyle.Thick);

                //sheet.Range[7, PCOL + 4].Text = "100";
                sheet.Range[7, PCOL + 7].Number = Convert.ToInt32("100");
                sheet.Range[7, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 7, 7, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[7, PCOL + 7, 7, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[7, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour);
                sheet.Range[7, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 8, 7, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[7, PCOL + 8, 7, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[7, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay);
                sheet.Range[7, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 9, 7, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[7, PCOL + 9, 7, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;



                //sheet.Range[8, PCOL + 4].Text = "85";

                sheet.Range[8, PCOL + 7].Number = Convert.ToInt32("85");
                sheet.Range[8, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 7, 8, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[8, PCOL + 7, 8, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[8, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .85);
                sheet.Range[8, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 8, 8, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[8, PCOL + 8, 8, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[8, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .85);
                sheet.Range[8, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 9, 8, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[8, PCOL + 9, 8, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;


                //sheet.Range[9, PCOL + 4].Text = "75";

                sheet.Range[9, PCOL + 7].Number = Convert.ToInt32("75");
                sheet.Range[9, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 7, 9, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[9, PCOL + 7, 9, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[9, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .75);
                sheet.Range[9, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 8, 9, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[9, PCOL + 8, 9, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[9, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .75);
                sheet.Range[9, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 9, 9, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[9, PCOL + 9, 9, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;

                //sheet.Range[10, PCOL + 4].Text = "65";

                sheet.Range[10, PCOL + 7].Number = Convert.ToInt32("65");
                sheet.Range[10, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 7, 10, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[10, PCOL + 7, 10, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[10, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .65);
                sheet.Range[10, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 8, 10, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[10, PCOL + 8, 10, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;


                sheet.Range[10, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .65);
                sheet.Range[10, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 9, 10, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[10, PCOL + 9, 10, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;


                //sheet.Range[11, PCOL + 4].Text = "55";
                sheet.Range[11, PCOL + 7].Number = Convert.ToInt32("55");
                sheet.Range[11, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 7, 11, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[11, PCOL + 7, 11, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[11, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .55);
                sheet.Range[11, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 8, 11, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[11, PCOL + 8, 11, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[11, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .55);
                sheet.Range[11, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 9, 11, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[11, PCOL + 9, 11, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;



                // sheet.Range[12, PCOL + 4].Text = "50";

                sheet.Range[12, PCOL + 7].Number = Convert.ToInt32("50");
                sheet.Range[12, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 7, 12, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[12, PCOL + 7, 12, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[12, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .50);
                sheet.Range[12, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 8, 12, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[12, PCOL + 8, 12, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[12, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .50);
                sheet.Range[12, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 9, 12, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[12, PCOL + 9, 12, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;

                // Created By	Creation Date Revision

                sheet.Range[7, PCOL + 10].Text = data.Rows[0]["AddedBy"].ToString();
                sheet.Range[7, PCOL + 11].Text = data.Rows[0]["AddedDate"].ToString();
                sheet.Range[7, PCOL + 12].Text = "1";


                //sheet.Range[rws, COL, rwe, COL].BorderInside(ExcelLineStyle.Thin);
                //sheet.Range[rws, COL, rwe, COL].BorderAround(ExcelLineStyle.Thin);

                sheet.Range[6, 5, 6, 21].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[6, 5, 6, 21].CellStyle.Font.Bold = true;
                sheet.Range[6, 5, 6, 21].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[6, 5, 6, 21].CellStyle.Font.Size = 9f;
                sheet.Range[6, 5, 6, 21].RowHeight = 30;
                sheet.Range[6, 5, 6, 21].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[6, 5, 6, 21].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[6, 5, 6, 21].BorderInside(ExcelLineStyle.Hair);


                ROW++;
                ROW++;
                ROW++;
                COL = 2;
                int sCol = COL;

                #region SetHeaderText


                report.SetHeaderText(ref sheet, ROW, COL, "Sr.No.", 8, ExcelHAlign.HAlignCenter);
                int ColSequence = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Code", 10, ExcelHAlign.HAlignCenter);
                int ColOperationCode = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Description", 28, ExcelHAlign.HAlignCenter);
                int ColMachineVarient = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Machine", 52, ExcelHAlign.HAlignCenter);
                int ColMachineCode = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "FG Zone", 15, ExcelHAlign.HAlignCenter);
                int ColFGZone = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Category", 15, ExcelHAlign.HAlignCenter);
                int ColOperationCategory = COL;
                COL++;



                report.SetHeaderText(ref sheet, ROW, COL, "SPT(Minutes)", 11, ExcelHAlign.HAlignCenter);
                int ColTotalSPT = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Target/Hr(Pcs)", 15, ExcelHAlign.HAlignCenter);
                int ColOperationTargetPerHr = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Required Man Power", 11, ExcelHAlign.HAlignCenter);
                int ColRequiredManPower = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Alloted Manpower", 11, ExcelHAlign.HAlignCenter);
                int ColAllotedManpower = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Alloted Workstation", 13, ExcelHAlign.HAlignCenter);
                int ColAllotedWorkstation = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Group", 18, ExcelHAlign.HAlignCenter);
                int ColOperationGroup = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Avg.Alloted Time", 15, ExcelHAlign.HAlignCenter);
                int ColAvgAllotedTime = COL;


                #endregion

                ROW++;
                endCol = COL;
                #endregion Headers

                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].RowHeight = 30;
                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);

                string ProcessName = "";
                var startRow = 0;
                var endRow = 0;
                int RowIndex = ROW;
                startRow = ROW;
                int srNo = 0;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    srNo++;
                    //sheet[ROW, ColSequence].Number = clsStaticInfo.dbl(data.Rows[i]["Sequence"].ToString());
                    sheet[ROW, ColSequence].Number = srNo;
                    sheet[ROW, ColOperationCode].Text = data.Rows[i]["OperationCode"].ToString();
                    sheet[ROW, ColMachineVarient].Text = data.Rows[i]["OperationVariation"].ToString();
                    sheet[ROW, ColMachineCode].Text = data.Rows[i]["ShortName"].ToString();
                    sheet.Range[ROW, ColTotalSPT].Number = Convert.ToDouble(data.Rows[i]["TotalSPT"].ToString());
                    sheet.Range[ROW, ColTotalSPT].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[ROW, ColTotalSPT, ROW, ColTotalSPT].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColTotalSPT, ROW, ColTotalSPT].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColOperationTargetPerHr].Number = clsStaticInfo.dbl(data.Rows[i]["OperationTargetPerHr"].ToString());
                    sheet.Range[ROW, ColOperationTargetPerHr, ROW, ColOperationTargetPerHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationTargetPerHr, ROW, ColOperationTargetPerHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColRequiredManPower].Number = clsStaticInfo.dbl(data.Rows[i]["RequiredManPower"].ToString());
                    sheet.Range[ROW, ColRequiredManPower, ROW, ColRequiredManPower].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColRequiredManPower, ROW, ColRequiredManPower].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColAllotedManpower].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedManpower"].ToString());
                    sheet.Range[ROW, ColAllotedManpower, ROW, ColAllotedManpower].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAllotedManpower, ROW, ColAllotedManpower].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColAllotedWorkstation].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedWorkstation"].ToString());
                    sheet.Range[ROW, ColAllotedWorkstation, ROW, ColAllotedWorkstation].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAllotedWorkstation, ROW, ColAllotedWorkstation].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    sheet[ROW, ColOperationGroup].Text = data.Rows[i]["OperationGroup"].ToString();
                    sheet.Range[ROW, ColAvgAllotedTime].Number = Convert.ToDouble(data.Rows[i]["AvgAllotedTime"].ToString());
                    sheet.Range[ROW, ColAvgAllotedTime].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[ROW, ColAvgAllotedTime, ROW, ColAvgAllotedTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAvgAllotedTime, ROW, ColAvgAllotedTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColFGZone].Text = data.Rows[i]["FGZone"].ToString();

                    sheet[ROW, ColOperationCategory].Text = data.Rows[i]["OperationCategory"].ToString();
                    sheet.Range[ROW, ColOperationCategory, ROW, ColOperationCategory].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationCategory, ROW, ColOperationCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    ProcessName = data.Rows[i]["Process"].ToString();

                    ROW++;
                }
                sheet.Range[startRow, ColSequence, ROW, ColAvgAllotedTime].BorderAround(ExcelLineStyle.Thick);
                endRow = ROW++;
                #region UH


                sheet.Range[endRow, 2].Text = ProcessName + " SPT";
                sheet.Range[endRow, 2, endRow, 3].Merge();
                sheet.Range[endRow, 3].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 8].Number = TotalSPT;
                sheet.Range[endRow, 8].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[endRow, 8].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 8, endRow, 8].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 8, endRow, 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[endRow, 9].Text = "TOTAL MP";
                sheet.Range[endRow, 9].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 11].Number = TotalManpower;
                sheet.Range[endRow, 11, endRow, 11].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 11, endRow, 11].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[endRow, 11].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 12].Number = TotalWS;
                sheet.Range[endRow, 12, endRow, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 12, endRow, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[endRow, 12].CellStyle.Font.Bold = true;

                endRow++;
                endRow++;
                int edRow = endRow++;
                int edCRow = edRow;

                sheet.Range[endRow, 4].Text = "MACHINE & MANPOWER REQUIREMENT SUMMARY";
                sheet.Range[endRow, 4].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 4, endRow, 5].Merge();

                int col = 4; edRow++; edRow++;
                sheet.Range[edRow, col].Text = "Machine";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                col++;
                sheet.Range[edRow, col].Text = "Machine Variation";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "SPT(Min)";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Req MP";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Allotted MP";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Allotted WS";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 4, edRow, 9].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[edRow, 4, edRow, 9].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 4, edRow, 9].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[edRow, 4, edRow, 9].CellStyle.Font.Size = 9f;
                sheet.Range[edRow, 4, edRow, 9].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[edRow, 4, edRow, 9].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[edRow, 4, edRow, 9].BorderAround(ExcelLineStyle.Thick);
                #endregion

                //DataTable dtM = new DataView(data).ToTable(true, "ShortName", "MachineVarientId", "MachineVarient", "AllotedWorkstation", "AllotedManpower", "RequiredManPower", "TotalSPT");
                DataTable dtM = new DataView(data).ToTable(true, "MachineMaster", "MachineVarientId", "ShortName");
                DataView dvM = new DataView(dtM);
                dvM.RowFilter = "MachineVarientId='" + data.Rows[0]["MachineVarientId"].ToString() + "'";
                edRow++;
                int msr = edRow;
                int sc = 4;
                int ec = 0;
                for (int i = 0; i < dtM.Rows.Count; i++)
                {

                    col = 4;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["MachineMaster"].ToString(); col++;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["ShortName"].ToString(); col++;

                    if (!string.IsNullOrEmpty(dtM.Rows[i]["MachineVarientId"].ToString()))
                    {
                        double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MTotalSPT;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MRequiredManPower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MAllotedManpower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedWorkstation = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MAllotedWorkstation;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    else
                    {
                        double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MTotalSPT;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MRequiredManPower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MAllotedManpower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedWorkstation = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MAllotedWorkstation;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    edRow++;
                }
                ec = col;
                int mer = edRow;


                sheet.Range[edRow, 4].Text = "TOTAL";
                sheet.Range[edRow, 4].CellStyle.Font.Bold = true;

                sheet.Range[edRow, 6].Number = TotalSPT;
                sheet.Range[edRow, 6].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[edRow, 6].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 6, edRow, 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 6, edRow, 6].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 7].Number = TotalRMP;
                sheet.Range[edRow, 7].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 7, edRow, 7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 7, edRow, 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 8].Number = TotalManpower;
                sheet.Range[edRow, 8].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 8, edRow, 8].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 8, edRow, 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 9].Number = TotalWS;
                sheet.Range[edRow, 9].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 9, edRow, 9].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 9, edRow, 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                sheet.Range[8, 12].Number = clsStaticInfo.dbl(data.Rows[0]["RequiredStdTarget"].ToString());
                sheet.Range[8, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[8, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[9, 12].Number = clsStaticInfo.dbl(data.Rows[0]["TotalBT"].ToString());
                sheet.Range[9, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[9, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                double rst = Convert.ToDouble(data.Rows[0]["RequiredStdTarget"]);
                double tbt = Convert.ToDouble(data.Rows[0]["TotalBT"]);
                double peHr = (rst / ProdEffPerHour) * 100;
                double peday = (tbt / ProdEffPerDay) * 100;
                sheet.Range[8, 13].Number = clsStaticInfo.dbl(peHr);
                sheet.Range[8, 13].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[8, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[8, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[9, 13].Number = clsStaticInfo.dbl(peday);
                sheet.Range[9, 13].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[9, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[9, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[8, 14].Number = clsStaticInfo.dbl(rst / TotalManpower);
                sheet.Range[8, 14].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[8, 14].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[8, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[9, 14].Number = clsStaticInfo.dbl((rst / TotalManpower) * plannedHourPerDay);
                sheet.Range[9, 14].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[9, 14].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[9, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[msr, sc, mer, ec].BorderAround(ExcelLineStyle.Thick);


                edCRow++; edCRow++;
                int Ccol = 11;
                int Cmsr = edCRow;
                int Csc = 11;//edCRow++;
                sheet.Range[edCRow, Ccol].Text = "Operation Category";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "SAM";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "Req MP";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "Allotted MP";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "Skill(%)";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                DataTable dtOC = new DataView(data).ToTable(true, "OperationCategory");
                dtOC.DefaultView.Sort = "OperationCategory ASC";
                dtOC = dtOC.DefaultView.ToTable();
                edCRow++;
                double tpercent = 0;
                for (int i = 0; i < dtOC.Rows.Count; i++)
                {

                    Ccol = 11;
                    sheet.Range[edCRow, Ccol].Text = dtOC.Rows[i]["OperationCategory"].ToString();
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "OperationCategory='" + dtOC.Rows[i]["OperationCategory"].ToString() + "'"));
                    sheet.Range[edCRow, Ccol].Number = MTotalSPT;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "OperationCategory='" + dtOC.Rows[i]["OperationCategory"].ToString() + "'"));
                    sheet.Range[edCRow, Ccol].Number = MRequiredManPower;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "OperationCategory='" + dtOC.Rows[i]["OperationCategory"].ToString() + "'"));
                    sheet.Range[edCRow, Ccol].Number = MAllotedManpower;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double calPer = (MAllotedManpower / TotalManpower) * 100;
                    tpercent += calPer;

                    sheet.Range[edCRow, Ccol].Number = calPer;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    edCRow++;
                }

                int Cec = Ccol;
                int Cmer = edCRow;

                sheet.Range[edCRow, 11].Text = "TOTAL";
                sheet.Range[edCRow, 11].CellStyle.Font.Bold = true;

                sheet.Range[edCRow, 12].Number = TotalSPT;
                sheet.Range[edCRow, 12].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 12].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 12, edCRow, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 12, edCRow, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edCRow, 13].Number = TotalRMP;
                sheet.Range[edCRow, 13].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 13, edCRow, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 13, edCRow, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edCRow, 14].Number = TotalManpower;
                sheet.Range[edCRow, 14].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 14, edCRow, 14].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 14, edCRow, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edCRow, 15].Number = tpercent;
                sheet.Range[edCRow, 15].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 15, edCRow, 15].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 15, edCRow, 15].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Font.Bold = true;
                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Font.Size = 9f;
                sheet.Range[Cmsr, 11, Cmsr, 15].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[Cmsr, 11, Cmsr, 15].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[Cmsr, Csc, Cmer, Cec].BorderAround(ExcelLineStyle.Thick);


                //sheet.UsedRange.NumberFormat = "#,##0.000";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                sheet.IsGridLinesVisible = false;


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet, 2, "Bulletin Template Summary - " + SheetName + "", identity.CompanyId, identity.CompanyName, "");
                sheet.Range[1, 2, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //report.CompanyHeader(ref sheet, endCol, "Bulletin Template - " + SheetName + "", companyId);
                report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        void CreateDetailSheet(string SheetName, DataTable data, ref IWorksheet sheet, string companyId)
        {
            try
            {
                var report = new ReportUtility();
                sheet.Name = SheetName;

                int ROW = 6;
                int endCol = 1;
                int COL = 5;

                sheet.Range[6, 2, 12, 4].BorderAround(ExcelLineStyle.Double);
                string ImageExt = Path.GetExtension(data.Rows[0]["PicFileName"].ToString());
                string IdImage = data.Rows[0]["Id"].ToString();
                #region Image
                try
                {
                    string strPath = Path.Combine(ResourcesPathReader.GetBulletinImagePath(), IdImage + ImageExt);
                    Image companyLogo = Image.FromFile(strPath);
                    if (companyLogo != null)
                    {
                        double totalWidth = sheet.GetColumnWidth(1) + sheet.GetColumnWidth(28);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet.GetRowHeight(1) + sheet.GetRowHeight(2) + sheet.GetRowHeight(36) + sheet.GetRowHeight(36)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet.Pictures.AddPicture(6, 2, companyLogo);


                    }


                }
                catch (Exception)
                {


                }
                #endregion

                #region Headers
                int rws = ROW;
                sheet.Range[ROW, COL].Text = "Buyer Name";
                sheet.Range[ROW, COL, ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL + 1].Text = " " + data.Rows[0]["Buyer"].ToString().Trim();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;
                sheet.Range[ROW, COL].Text = "Buyer Style Ref No";
                sheet.Range[ROW, COL + 1].Text = " " + data.Rows[0]["BuyerStyleRefNo"].ToString().Trim();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;
                sheet.Range[ROW, COL].Text = "Own Style Ref No";
                int rowBulletinTarget = ROW;
                sheet.Range[ROW, COL + 1].Text = " " + data.Rows[0]["OwnStyleRefNo"].ToString().Trim();
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();
                ROW++;
                sheet.Range[ROW, COL].Text = "Product Master";
                sheet.Range[ROW, COL + 1].Text = " " + data.Rows[0]["ProductMaster"].ToString().Trim();
                sheet.Range[ROW, COL, ROW, COL + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();

                double plannedHourPerDay = Convert.ToDouble(data.Rows[0]["PlannedHoursPerDay"]);
                double TotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", null));
                double TotalManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", null));
                double TotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", null));
                double MaxAllotedTime = clsStaticInfo.dbl(data.Compute("Max(AvgAllotedTime)", null));
                double TotalRMP = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", null));

                double PitchTime = 0;
                if (TotalManpower != 0)
                    PitchTime = TotalSPT / TotalManpower;

                double OrgEfficiency = 0;
                if (MaxAllotedTime != 0)
                    OrgEfficiency = PitchTime / MaxAllotedTime;

                double ProdEffPerHour = 0;
                if (TotalSPT != 0)
                    ProdEffPerHour = TotalManpower * 60 / TotalSPT;

                double ProdEffPerDay = ProdEffPerHour * plannedHourPerDay;
                double LineTargetPerHour = ProdEffPerHour * OrgEfficiency;


                ROW++;
                sheet.Range[ROW, COL].Text = "Pitch Time(Minutes)";
                //sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 1].Number = PitchTime;
                int colPitchTime = COL + 1;
                int rowPitchTime = ROW;
                sheet.Range[ROW, COL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[ROW, COL + 1, ROW, COL + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL + 1, ROW, COL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();

                ROW++;
                sheet.Range[ROW, COL].Text = "Planned Working Hour PerDay";
                //sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 1].Number = plannedHourPerDay;
                sheet.Range[ROW, COL + 1].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[ROW, COL, ROW, COL + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL, ROW, COL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, COL + 1, ROW, COL + 2].Merge();

                sheet.Range[6, 5, 12, 7].BorderAround(ExcelLineStyle.Thick);


                int rwe = 6;
                int PCOL = 8;
                sheet.Range[rws, PCOL].Text = "Particulars";
                sheet.Range[rws, PCOL + 1].Text = "SPT(Minutes)";
                sheet.Range[rws, PCOL + 2].Text = "MP";
                sheet.Range[rws, PCOL + 3].Text = "Work Station";
                sheet.Range[rws, PCOL + 4].Text = "Bullitin Target(Pcs)";
                int colBulletinTarget = PCOL + 4;
                sheet.Range[rws, PCOL + 5].Text = "Planned Efficency(%)";
                sheet.Range[rws, PCOL + 6].Text = "Planned Per Man productivity(Pcs)";

                sheet.Range[6, 12, 12, 14].BorderAround(ExcelLineStyle.Thick);
                sheet.Range[rws, PCOL, rws, PCOL + 6].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL + 6].VerticalAlignment = ExcelVAlign.VAlignCenter;


                sheet.Range[rws, PCOL + 7].Text = "Target(%)";
                sheet.Range[rws, PCOL + 8].Text = "Per Hr";
                sheet.Range[rws, PCOL + 9].Text = "Per Day";

                sheet.Range[rws, PCOL + 7, rws, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 7, rws, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[6, 15, 12, 17].BorderAround(ExcelLineStyle.Thick);

                sheet.Range[rws, PCOL + 10].Text = "Created By";
                sheet.Range[rws, PCOL + 10, rws, PCOL + 10].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 11].Text = "Creation Date";
                sheet.Range[rws, PCOL + 11, rws, PCOL + 11].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 12].Text = "Revision";
                sheet.Range[rws, PCOL + 12, rws, PCOL + 12].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 13].Text = "Approved By";
                sheet.Range[rws, PCOL + 13, rws, PCOL + 13].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[6, 18, 12, 21].BorderAround(ExcelLineStyle.Thick);

                rws++;
                sheet.Range[rws, PCOL].Text = "Non MC";
                sheet.Range[rws, PCOL, rws, PCOL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                double NMCTotalSPT = 0;
                double NMCTotalWS = 0;
                double NMCTotalMP = 0;

                NMCTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='No'"));
                NMCTotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='No'"));
                NMCTotalMP = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='No'"));

                sheet.Range[rws, PCOL + 1].Number = NMCTotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 2].Number = NMCTotalMP;
                sheet.Range[rws, PCOL + 2].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 3].Number = NMCTotalWS;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                rws++;
                sheet.Range[rws, PCOL].Text = "MC";
                sheet.Range[rws, PCOL, rws, PCOL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                double MCTotalSPT = 0;
                double MCTotalWS = 0;
                double MCTotalMP = 0;

                MCTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='Yes'"));
                MCTotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='Yes'"));
                MCTotalMP = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='Yes'"));

                sheet.Range[rws, PCOL + 1].Number = MCTotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 2].Number = MCTotalMP;
                sheet.Range[rws, PCOL + 2].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 3].Number = MCTotalWS;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                rws++;
                sheet.Range[rws, PCOL].Text = "Total";
                sheet.Range[rws, PCOL, rws, PCOL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 1].Number = TotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 2].Number = TotalManpower; string ToTalManpowerCellAddr = clsStaticInfo.GetxlsCol(PCOL + 2) + (rws.ToString());
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 3].Number = TotalWS;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, 8, rws, 11].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[rws, 8, rws, 11].CellStyle.Font.Bold = true;
                sheet.Range[rws, 8, rws, 11].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[6, 8, 12, 11].BorderAround(ExcelLineStyle.Thick);

                //sheet.Range[7, PCOL + 4].Text = "100";
                sheet.Range[7, PCOL + 7].Number = Convert.ToInt32("100");
                sheet.Range[7, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 7, 7, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[7, PCOL + 7, 7, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;


                string CellTargetAt100PercentEfficiency = clsStaticInfo.GetxlsCol(PCOL + 8) + "7";
                sheet.Range[7, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour);
                sheet.Range[7, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 8, 7, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[7, PCOL + 8, 7, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[7, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay);
                sheet.Range[7, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 9, 7, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[7, PCOL + 9, 7, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;



                //sheet.Range[8, PCOL + 4].Text = "85";

                sheet.Range[8, PCOL + 7].Number = Convert.ToInt32("85");
                sheet.Range[8, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 7, 8, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[8, PCOL + 7, 8, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[8, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .85);
                sheet.Range[8, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 8, 8, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[8, PCOL + 8, 8, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[8, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .85);
                sheet.Range[8, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 9, 8, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[8, PCOL + 9, 8, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;


                //sheet.Range[9, PCOL + 4].Text = "75";

                sheet.Range[9, PCOL + 7].Number = Convert.ToInt32("75");
                sheet.Range[9, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 7, 9, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[9, PCOL + 7, 9, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[9, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .75);
                sheet.Range[9, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 8, 9, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[9, PCOL + 8, 9, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[9, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .75);
                sheet.Range[9, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 9, 9, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[9, PCOL + 9, 9, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;

                //sheet.Range[10, PCOL + 4].Text = "65";

                sheet.Range[10, PCOL + 7].Number = Convert.ToInt32("65");
                sheet.Range[10, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 7, 10, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[10, PCOL + 7, 10, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[10, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .65);
                sheet.Range[10, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 8, 10, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[10, PCOL + 8, 10, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;


                sheet.Range[10, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .65);
                sheet.Range[10, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 9, 10, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[10, PCOL + 9, 10, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;


                //sheet.Range[11, PCOL + 4].Text = "55";
                sheet.Range[11, PCOL + 7].Number = Convert.ToInt32("55");
                sheet.Range[11, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 7, 11, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[11, PCOL + 7, 11, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[11, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .55);
                sheet.Range[11, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 8, 11, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[11, PCOL + 8, 11, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[11, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .55);
                sheet.Range[11, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 9, 11, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[11, PCOL + 9, 11, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;



                // sheet.Range[12, PCOL + 4].Text = "50";

                sheet.Range[12, PCOL + 7].Number = Convert.ToInt32("50");
                sheet.Range[12, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 7, 12, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[12, PCOL + 7, 12, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[12, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .50);
                sheet.Range[12, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 8, 12, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[12, PCOL + 8, 12, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[12, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .50);
                sheet.Range[12, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 9, 12, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[12, PCOL + 9, 12, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;

                // Created By	Creation Date Revision

                sheet.Range[7, PCOL + 10].Text = data.Rows[0]["AddedBy"].ToString();
                sheet.Range[7, PCOL + 11].Text = data.Rows[0]["AddedDate"].ToString();
                sheet.Range[7, PCOL + 12].Text = "1";


                //sheet.Range[rws, COL, rwe, COL].BorderInside(ExcelLineStyle.Thin);
                //sheet.Range[rws, COL, rwe, COL].BorderAround(ExcelLineStyle.Thin);

                sheet.Range[6, 5, 6, 21].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[6, 5, 6, 21].CellStyle.Font.Bold = true;
                sheet.Range[6, 5, 6, 21].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[6, 5, 6, 21].CellStyle.Font.Size = 9f;
                sheet.Range[6, 5, 6, 21].RowHeight = 30;
                sheet.Range[6, 5, 6, 21].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[6, 5, 6, 21].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[6, 5, 6, 21].BorderInside(ExcelLineStyle.Hair);


                ROW++;
                ROW++;
                ROW++;
                COL = 2;
                int sCol = COL;

                #region SetHeaderText


                report.SetHeaderText(ref sheet, ROW, COL, "Sr.No.", 8, ExcelHAlign.HAlignCenter);
                int ColSequence = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Code", 10, ExcelHAlign.HAlignCenter);
                int ColOperationCode = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Description", 28, ExcelHAlign.HAlignLeft);
                int ColMachineVarient = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Machine", 52, ExcelHAlign.HAlignLeft);
                int ColMachineCode = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "FG Zone", 15, ExcelHAlign.HAlignLeft);
                int ColFGZone = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "FG Component", 15, ExcelHAlign.HAlignLeft);
                int ColFGComponent = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Category", 15, ExcelHAlign.HAlignCenter);
                int ColOperationCategory = COL;
                COL++;



                report.SetHeaderText(ref sheet, ROW, COL, "SPT(Minutes)", 11, ExcelHAlign.HAlignCenter);
                int ColTotalSPT = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Target/Hr(Pcs)", 15, ExcelHAlign.HAlignCenter);
                int ColOperationTargetPerHr = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Required Man Power", 11, ExcelHAlign.HAlignCenter);
                int ColRequiredManPower = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Alloted Manpower", 11, ExcelHAlign.HAlignCenter);
                int ColAllotedManpower = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Alloted Workstation", 13, ExcelHAlign.HAlignCenter);
                int ColAllotedWorkstation = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Group", 18, ExcelHAlign.HAlignCenter);
                int ColOperationGroup = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Avg.Alloted Time", 13, ExcelHAlign.HAlignCenter);
                int ColAvgAllotedTime = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation wise O/p per Hr.", 13, ExcelHAlign.HAlignCenter);
                int ColOperationWiseOutputPerHour = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Attachment", 15, ExcelHAlign.HAlignLeft);
                int ColAttachment = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Gauge Folder", 15, ExcelHAlign.HAlignLeft);
                int ColGaugeFolder = COL;
                COL++;


                report.SetHeaderText(ref sheet, ROW, COL, "Operation Type", 15, ExcelHAlign.HAlignLeft);
                int ColOperationType = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Quality Level", 15, ExcelHAlign.HAlignLeft);
                int ColQualityLevel = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Frequency", 12, ExcelHAlign.HAlignCenter);
                int ColFrequency = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Remark", 15, ExcelHAlign.HAlignLeft);
                int ColRemark = COL;
                endCol = COL;
                sheet.Range[ROW, sCol, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[ROW, sCol, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, sCol, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, sCol, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, sCol, ROW, endCol].RowHeight = 30;
                sheet.Range[ROW, sCol, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, sCol, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                COL++;
                COL++;

                sheet[ROW, COL].Text = "Target On Org. Efficiency";
                sheet[ROW, COL].ColumnWidth = 0.05f;
                int ColTargetOnOrgEff = COL;
                COL++;

                sheet[ROW, COL].Text = "Production @ 100% Eff";
                sheet[ROW, COL].ColumnWidth = 0.05f;

                int colProductionAt100PercentEfficiency = COL;
                COL++;
                sheet[ROW, COL].Text = "Line Req Tgt";
                sheet[ROW, COL].ColumnWidth = 0.05f;

                int colLineReqTgt = COL;
                endCol = COL;


                #endregion

                ROW++;
                endCol = COL;
                #endregion Headers


                string ProcessName = "";
                var startRow = 0;
                var endRow = 0;
                int RowIndex = ROW;
                startRow = ROW;
                int srNo = 0;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    srNo++;
                    //sheet[ROW, ColSequence].Number = clsStaticInfo.dbl(data.Rows[i]["Sequence"].ToString());
                    sheet[ROW, ColSequence].Number = srNo;
                    sheet.Range[ROW, ColSequence, ROW, ColSequence].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColSequence, ROW, ColSequence].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColOperationCode].Text = data.Rows[i]["OperationCode"].ToString();
                    sheet.Range[ROW, ColOperationCode, ROW, ColOperationCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationCode, ROW, ColOperationCode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColMachineVarient].Text = data.Rows[i]["OperationVariation"].ToString();
                    sheet[ROW, ColMachineCode].Text = data.Rows[i]["ShortName"].ToString();
                    sheet.Range[ROW, ColTotalSPT].Number = Convert.ToDouble(data.Rows[i]["TotalSPT"].ToString());
                    sheet.Range[ROW, ColTotalSPT].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[ROW, ColTotalSPT, ROW, ColTotalSPT].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColTotalSPT, ROW, ColTotalSPT].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColOperationTargetPerHr].Number = clsStaticInfo.dbl(data.Rows[i]["OperationTargetPerHr"].ToString());
                    sheet.Range[ROW, ColOperationTargetPerHr, ROW, ColOperationTargetPerHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationTargetPerHr, ROW, ColOperationTargetPerHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColRequiredManPower].Number = clsStaticInfo.dbl(data.Rows[i]["RequiredManPower"].ToString());
                    sheet.Range[ROW, ColRequiredManPower, ROW, ColRequiredManPower].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColRequiredManPower, ROW, ColRequiredManPower].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColAllotedManpower].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedManpower"].ToString());
                    sheet.Range[ROW, ColAllotedManpower, ROW, ColAllotedManpower].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAllotedManpower, ROW, ColAllotedManpower].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColAllotedWorkstation].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedWorkstation"].ToString());
                    sheet.Range[ROW, ColAllotedWorkstation, ROW, ColAllotedWorkstation].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAllotedWorkstation, ROW, ColAllotedWorkstation].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    sheet[ROW, ColOperationGroup].Text = data.Rows[i]["OperationGroup"].ToString();
                    sheet.Range[ROW, ColOperationGroup, ROW, ColOperationGroup].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationGroup, ROW, ColOperationGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[ROW, ColAvgAllotedTime].Number = Convert.ToDouble(data.Rows[i]["AvgAllotedTime"].ToString());
                    sheet.Range[ROW, ColAvgAllotedTime].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[ROW, ColAvgAllotedTime, ROW, ColAvgAllotedTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAvgAllotedTime, ROW, ColAvgAllotedTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet.Range[ROW, ColOperationWiseOutputPerHour].Formula = "60/" + clsStaticInfo.GetxlsCol(ColAvgAllotedTime) + ROW;
                    sheet.Range[ROW, ColOperationWiseOutputPerHour].NumberFormat = clsStaticInfo.NumberFormat(0);


                    sheet[ROW, ColFGZone].Text = data.Rows[i]["FGZone"].ToString();
                    sheet[ROW, ColFGComponent].Text = data.Rows[i]["FGComponent"].ToString();
                    sheet[ROW, ColOperationCategory].Text = data.Rows[i]["OperationCategory"].ToString();
                    sheet.Range[ROW, ColOperationCategory, ROW, ColOperationCategory].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationCategory, ROW, ColOperationCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColAttachment].Text = data.Rows[i]["Attachment"].ToString();
                    sheet[ROW, ColGaugeFolder].Text = data.Rows[i]["GaugeFolder"].ToString();
                    //sheet[ROW, ColOperationConsumption].Text = data.Rows[i]["OperationConsumption"].ToString();
                    sheet[ROW, ColOperationType].Text = data.Rows[i]["OperationType"].ToString();
                    sheet[ROW, ColQualityLevel].Text = data.Rows[i]["QualityLevel"].ToString();


                    sheet[ROW, ColFrequency].Number = clsStaticInfo.dbl(data.Rows[i]["Frequency"].ToString());
                    sheet.Range[ROW, ColFrequency].NumberFormat = clsStaticInfo.NumberFormat(0);
                    sheet.Range[ROW, ColFrequency, ROW, ColFrequency].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    sheet[ROW, ColRemark].Text = data.Rows[i]["Remark"].ToString();


                    sheet.Range[ROW, 1, ROW, ColRemark].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, ColRemark].BorderAround(ExcelLineStyle.Hair);



                    sheet[ROW, colLineReqTgt].Formula = clsStaticInfo.GetxlsCol(colBulletinTarget) + rowBulletinTarget;
                    sheet[ROW, colLineReqTgt].NumberFormat = clsStaticInfo.NumberFormat(0);

                    sheet[ROW, colProductionAt100PercentEfficiency].Formula = CellTargetAt100PercentEfficiency;
                    sheet[ROW, colProductionAt100PercentEfficiency].NumberFormat = clsStaticInfo.NumberFormat(0);


                    sheet[ROW, ColTargetOnOrgEff].Formula = "(" + CellTargetAt100PercentEfficiency + ")*" + OrgEfficiency.ToString();
                    sheet[ROW, ColTargetOnOrgEff].NumberFormat = clsStaticInfo.NumberFormat(0);


                    //sheet.Range[ROW, ColRemark + 2, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    //sheet.Range[ROW, ColRemark + 2, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    ProcessName = data.Rows[i]["Process"].ToString();

                    ROW++;
                }

                #region graph

                IChartShape chart = sheet.Charts.Add();
                //Set chart type
                chart.ChartType = ExcelChartType.Column_Clustered;
                //Set Chart Title
                chart.ChartTitle = "Per Hour Production Comparison";

                //Output Base on BPT
                IChartSerie ChartOperationOpt = chart.Series.Add("Operation Wise Output Per Hour");
                ChartOperationOpt.SerieType = ExcelChartType.Column_Clustered;
                ChartOperationOpt.Values = sheet.Range[startRow, ColOperationWiseOutputPerHour, ROW - 1, ColOperationWiseOutputPerHour];
                // productA.CategoryLabels = sheet1.Range["A2:A6"];

                //colLineReqTg
                IChartSerie ChartLineReq = chart.Series.Add("Plan Target");
                ChartLineReq.SerieType = ExcelChartType.Line;
                ChartLineReq.Values = sheet.Range[startRow, colLineReqTgt, ROW - 1, colLineReqTgt];

                //colProductionAt100PercentEfficiency
                IChartSerie ChartProductionAt100PercentEfficiency = chart.Series.Add("Std. Target at 100%");
                ChartProductionAt100PercentEfficiency.SerieType = ExcelChartType.Line;
                ChartProductionAt100PercentEfficiency.Values = sheet.Range[startRow, colProductionAt100PercentEfficiency, ROW - 1, colProductionAt100PercentEfficiency];


                //colProductionAt100PercentEfficiency
                IChartSerie ChartTargetOnOrgEff = chart.Series.Add("Target at Org. Eff.");
                ChartTargetOnOrgEff.SerieType = ExcelChartType.Line;
                ChartTargetOnOrgEff.Values = sheet.Range[startRow, ColTargetOnOrgEff, ROW - 1, ColTargetOnOrgEff];


                for (int i = 1; i <= endCol; i++)
                    chart.XPos += sheet[1, i].ColumnWidth * 7.5;

                chart.YPos = 240;

                chart.Legend.Position = ExcelLegendPosition.Bottom;
                chart.Scale(50, 100);
                #endregion graph

                sheet.Range[startRow, ColSequence, ROW, ColRemark].BorderAround(ExcelLineStyle.Thick);
                endRow = ROW++;
                #region UH


                sheet.Range[endRow, 2].Text = "Total SPT";
                sheet.Range[endRow, 2, endRow, 3].Merge();
                sheet.Range[endRow, 3].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 9].Number = TotalSPT;
                sheet.Range[endRow, 9].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[endRow, 9].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 9, endRow, 9].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 9, endRow, 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[endRow, 10].Text = "TOTAL MP";
                sheet.Range[endRow, 10].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 12].Number = TotalManpower;
                sheet.Range[endRow, 12, endRow, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 12, endRow, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[endRow, 12].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 13].Number = TotalWS;
                sheet.Range[endRow, 13, endRow, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 13, endRow, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[endRow, 13].CellStyle.Font.Bold = true;

                endRow++;
                endRow++;
                int edRow = endRow++;
                int edCRow = edRow;

                sheet.Range[endRow, 4].Text = "MACHINE & MANPOWER REQUIREMENT SUMMARY";
                sheet.Range[endRow, 4].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 4, endRow, 5].Merge();

                int col = 4; edRow++; edRow++;
                sheet.Range[edRow, col].Text = "Machine";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                col++;
                sheet.Range[edRow, col].Text = "Machine Variation";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;

                col++;
                sheet.Range[edRow, col].Text = "SPT(Min)";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Req MP";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Allotted MP";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Allotted WS";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 4, edRow, 9].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[edRow, 4, edRow, 9].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 4, edRow, 9].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[edRow, 4, edRow, 9].CellStyle.Font.Size = 9f;
                sheet.Range[edRow, 4, edRow, 9].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[edRow, 4, edRow, 9].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[edRow, 4, edRow, 9].BorderAround(ExcelLineStyle.Thick);
                #endregion

                //DataTable dtM = new DataView(data).ToTable(true, "ShortName", "MachineVarientId", "MachineVarient", "AllotedWorkstation", "AllotedManpower", "RequiredManPower", "TotalSPT");
                DataTable dtM = new DataView(data).ToTable(true, "MachineMaster", "MachineVarientId", "ShortName");
                DataView dvM = new DataView(dtM);
                dvM.RowFilter = "MachineVarientId='" + data.Rows[0]["MachineVarientId"].ToString() + "'";
                edRow++;
                int msr = edRow;
                int sc = 4;
                int ec = 0;
                for (int i = 0; i < dtM.Rows.Count; i++)
                {

                    col = 4;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["MachineMaster"].ToString(); col++;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["ShortName"].ToString(); col++;

                    if (!string.IsNullOrEmpty(dtM.Rows[i]["MachineVarientId"].ToString()))
                    {
                        double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MTotalSPT;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MRequiredManPower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MAllotedManpower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedWorkstation = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MAllotedWorkstation;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    else
                    {
                        double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MTotalSPT;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MRequiredManPower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MAllotedManpower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedWorkstation = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MAllotedWorkstation;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    edRow++;
                }
                ec = col;
                int mer = edRow;


                sheet.Range[edRow, 4].Text = "TOTAL";
                sheet.Range[edRow, 4].CellStyle.Font.Bold = true;

                sheet.Range[edRow, 6].Number = TotalSPT;
                sheet.Range[edRow, 6].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[edRow, 6].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 6, edRow, 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 6, edRow, 6].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 7].Number = TotalRMP;
                sheet.Range[edRow, 7].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 7, edRow, 7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 7, edRow, 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 8].Number = TotalManpower;
                sheet.Range[edRow, 8].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 8, edRow, 8].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 8, edRow, 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 9].Number = TotalWS;
                sheet.Range[edRow, 9].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 9, edRow, 9].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 9, edRow, 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                sheet.Range[8, 12].Number = clsStaticInfo.dbl(data.Rows[0]["RequiredStdTarget"].ToString());
                sheet.Range[8, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[8, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[9, 12].Number = clsStaticInfo.dbl(data.Rows[0]["TotalBT"].ToString());
                sheet.Range[9, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[9, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                double rst = Convert.ToDouble(data.Rows[0]["RequiredStdTarget"]);
                double tbt = Convert.ToDouble(data.Rows[0]["TotalBT"]);
                double peHr = (rst / ProdEffPerHour) * 100;
                double peday = (tbt / ProdEffPerDay) * 100;
                sheet.Range[8, 13].Number = clsStaticInfo.dbl(peHr);
                sheet.Range[8, 13].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[8, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[8, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[9, 13].Number = clsStaticInfo.dbl(peday);
                sheet.Range[9, 13].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[9, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[9, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[8, 14].Number = clsStaticInfo.dbl(rst / TotalManpower);
                sheet.Range[8, 14].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[8, 14].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[8, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[9, 14].Number = clsStaticInfo.dbl((rst / TotalManpower) * plannedHourPerDay);
                sheet.Range[9, 14].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[9, 14].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[9, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[msr, sc, mer, ec].BorderAround(ExcelLineStyle.Thick);


                edCRow++; edCRow++;
                int Ccol = 11;
                int Cmsr = edCRow;
                int Csc = 11;//edCRow++;
                sheet.Range[edCRow, Ccol].Text = "Operation Category";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "SAM";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "Req MP";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "Allotted MP";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "Skill(%)";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                DataTable dtOC = new DataView(data).ToTable(true, "OperationCategory");
                dtOC.DefaultView.Sort = "OperationCategory ASC";
                dtOC = dtOC.DefaultView.ToTable();
                edCRow++;
                double tpercent = 0;
                for (int i = 0; i < dtOC.Rows.Count; i++)
                {

                    Ccol = 11;
                    sheet.Range[edCRow, Ccol].Text = dtOC.Rows[i]["OperationCategory"].ToString();
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "OperationCategory='" + dtOC.Rows[i]["OperationCategory"].ToString() + "'"));
                    sheet.Range[edCRow, Ccol].Number = MTotalSPT;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "OperationCategory='" + dtOC.Rows[i]["OperationCategory"].ToString() + "'"));
                    sheet.Range[edCRow, Ccol].Number = MRequiredManPower;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "OperationCategory='" + dtOC.Rows[i]["OperationCategory"].ToString() + "'"));
                    sheet.Range[edCRow, Ccol].Number = MAllotedManpower;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double calPer = (MAllotedManpower / TotalManpower) * 100;
                    tpercent += calPer;

                    sheet.Range[edCRow, Ccol].Number = Math.Round(calPer);
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(0);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    edCRow++;
                }

                int Cec = Ccol;
                int Cmer = edCRow;

                sheet.Range[edCRow, 11].Text = "TOTAL";
                sheet.Range[edCRow, 11].CellStyle.Font.Bold = true;

                sheet.Range[edCRow, 12].Number = TotalSPT;
                sheet.Range[edCRow, 12].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 12].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 12, edCRow, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 12, edCRow, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edCRow, 13].Number = TotalRMP;
                sheet.Range[edCRow, 13].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 13, edCRow, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 13, edCRow, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edCRow, 14].Number = TotalManpower;
                sheet.Range[edCRow, 14].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 14, edCRow, 14].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 14, edCRow, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edCRow, 15].Number = tpercent;
                sheet.Range[edCRow, 15].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 15, edCRow, 15].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 15, edCRow, 15].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Font.Bold = true;
                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Font.Size = 9f;
                sheet.Range[Cmsr, 11, Cmsr, 15].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[Cmsr, 11, Cmsr, 15].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[Cmsr, Csc, Cmer, Cec].BorderAround(ExcelLineStyle.Thick);


                //sheet.UsedRange.NumberFormat = "#,##0.000";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                sheet.IsGridLinesVisible = false;


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet, 2, "Bulletin Template Detail - " + SheetName + "", identity.CompanyId, identity.CompanyName, "");
                sheet.Range[1, 2, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //report.CompanyHeader(ref sheet, endCol, "Bulletin Template - " + SheetName + "", companyId);
                report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }




        #endregion end Reports for Bullatin Template

        #region Thread Consumption
        [HttpGet, Authorize]
        public ActionResult GetThreadConsumptionReport(ReportFormat reportFormat, string bulletinTemplateMasterId,string bulletinId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Thread Consumption";
            var workbook = GetThreadConsumptionReportWorkSheet(bulletinTemplateMasterId, bulletinId);
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

        private IWorkbook GetThreadConsumptionReportWorkSheet(string bulletinTemplateMasterId, string bulletinId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "Thread Consumption";

            int ROW = 5;
            int endCol = 1;
            int COL = 1;

            DataTable dataHeader = clsb.GetBulletinTemplateDatabyId(bulletinId);
            DataTable data = clsb.GetThreadConsumptionData(bulletinTemplateMasterId);
            DataTable summaryData = clsb.GetThreadConsumptionSummaryData(bulletinTemplateMasterId);

            #region Headers

            sheet.Range[ROW, COL + 1].Text = "Bulletin Id";
            sheet.Range[ROW, COL, ROW, COL + 1].Merge();
            sheet.Range[ROW, COL + 2].Text = " " + dataHeader.Rows[0]["Id"].ToString().Trim();
            sheet.Range[ROW, COL + 2, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;

            sheet.Range[ROW, COL + 1].Text = "Bulletin Name";
            sheet.Range[ROW, COL, ROW, COL + 1].Merge();
            sheet.Range[ROW, COL + 2].Text = " " + dataHeader.Rows[0]["BulletinName"].ToString().Trim();
            sheet.Range[ROW, COL + 2, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;

            sheet.Range[ROW, COL + 1].Text = "Product";
            sheet.Range[ROW, COL, ROW, COL + 1].Merge();
            sheet.Range[ROW, COL + 2].Text = " " + dataHeader.Rows[0]["ProductMaster"].ToString().Trim();
            sheet.Range[ROW, COL, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            ROW++;
            ROW++;


            report.SetHeaderText(ref sheet, ROW, COL, "Operation", 25, ExcelHAlign.HAlignLeft);
            int ColOperation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Operation Code", 12, ExcelHAlign.HAlignLeft);
            int ColOperationCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Operation Variation", 25, ExcelHAlign.HAlignLeft);
            int ColOperationVariation = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Machine Name", 25, ExcelHAlign.HAlignLeft);
            int ColMachineName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Stitch Code", 17, ExcelHAlign.HAlignLeft);
            int ColStitchCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SPI", 10, ExcelHAlign.HAlignCenter);
            int ColSPI = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "No of Rows", 10, ExcelHAlign.HAlignCenter);
            int ColNoOfStitch = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Operation Length(cm)", 10, ExcelHAlign.HAlignCenter);
            int ColOperationLength = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Fabric Width(inch)", 10, ExcelHAlign.HAlignCenter);
            int ColFabricWidth = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SPI Consumption", 12, ExcelHAlign.HAlignCenter);
            int ColSPIConsumption = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Consumption(mt)", 11, ExcelHAlign.HAlignCenter);
            int ColConsumption = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Needle Material Master", 15, ExcelHAlign.HAlignLeft);
            int ColNeedleMaterialMaster = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Needle Article", 15, ExcelHAlign.HAlignLeft);
            int ColNeedleArticle = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Needle Description", 30, ExcelHAlign.HAlignLeft);
            int ColNeedleDescription = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Needle Consumption", 15, ExcelHAlign.HAlignCenter);
            int ColNeedleConsumption = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Bobbin Material Master", 15, ExcelHAlign.HAlignLeft);
            int ColBobbinMaterialMaster = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Bobbin Article", 15, ExcelHAlign.HAlignLeft);
            int ColBobbinArticle = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Bobbin Description", 30, ExcelHAlign.HAlignLeft);
            int ColBobbinDescription = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Bobbin Consumption", 15, ExcelHAlign.HAlignCenter);
            int ColBobbinConsumption = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Looper Material Master", 15, ExcelHAlign.HAlignLeft);
            int ColLooperMaterialMaster = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Looper Article", 15, ExcelHAlign.HAlignLeft);
            int ColLooperArticle = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Looper Description", 30, ExcelHAlign.HAlignLeft);
            int ColLooperDescription = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Looper Consumption", 15, ExcelHAlign.HAlignCenter);
            int ColLooperConsumption = COL;

            endCol = COL;
            #endregion Headers

            var startRow = 0;
            var endRow = 0;

            int RowIndex = ROW;

            decimal totalWastagePercentage = 0;
            decimal WastagePercentage = 0;
            decimal ExtraOrderPercentage = 0;
            ROW++;
            startRow = ROW;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                totalWastagePercentage = Convert.ToDecimal(data.Rows[i]["TotalWastagePercentage"].ToString());
                WastagePercentage = Convert.ToDecimal(data.Rows[i]["WastagePercentage"].ToString());
                ExtraOrderPercentage = Convert.ToDecimal(data.Rows[i]["ExtraOrderPercentage"].ToString());
                sheet[ROW, ColOperation].Text = data.Rows[i]["Operation"].ToString();
                sheet[ROW, ColOperationCode].Text = data.Rows[i]["OperationCode"].ToString();
                sheet[ROW, ColOperationVariation].Text = data.Rows[i]["OperationVariation"].ToString();
                sheet[ROW, ColMachineName].Text = data.Rows[i]["MachineName"].ToString();
                sheet[ROW, ColStitchCode].Text = data.Rows[i]["StitchCode"].ToString();

                sheet.Range[ROW, ColSPI].Number = Convert.ToInt32(data.Rows[i]["SPI"].ToString());
                sheet.Range[ROW, ColSPI].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColSPI].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                sheet.Range[ROW, ColNoOfStitch].Number = Convert.ToInt32(data.Rows[i]["NoOfStitch"].ToString());
                sheet.Range[ROW, ColNoOfStitch].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColNoOfStitch].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColOperationLength].Number = Convert.ToDouble(data.Rows[i]["OperationLength"].ToString());
                sheet.Range[ROW, ColOperationLength].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColOperationLength].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColFabricWidth].Number = Convert.ToDouble(data.Rows[i]["FabricWidth"].ToString());
                sheet.Range[ROW, ColFabricWidth].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColFabricWidth].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColSPIConsumption].Number = Convert.ToDouble(data.Rows[i]["SPIConsumption"].ToString());
                sheet.Range[ROW, ColSPIConsumption].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColSPIConsumption].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColConsumption].Number = Convert.ToDouble(data.Rows[i]["Consumption"].ToString());
                sheet.Range[ROW, ColConsumption].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColConsumption].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColNeedleMaterialMaster].Text = data.Rows[i]["NeedleMaterialMaster"].ToString();
                sheet[ROW, ColNeedleArticle].Text = data.Rows[i]["NeedleArticle"].ToString();
                sheet[ROW, ColNeedleDescription].Text = data.Rows[i]["NeedleDescription"].ToString();
                sheet[ROW, ColNeedleConsumption].Number = Convert.ToDouble(data.Rows[i]["NeedleConsumption"].ToString());
                sheet.Range[ROW, ColNeedleConsumption].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColNeedleConsumption].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColBobbinMaterialMaster].Text = data.Rows[i]["BobbinMaterialMaster"].ToString();
                sheet[ROW, ColBobbinArticle].Text = data.Rows[i]["BobbinArticle"].ToString();
                sheet[ROW, ColBobbinDescription].Text = data.Rows[i]["BobbinDescription"].ToString();
                sheet[ROW, ColBobbinConsumption].Number = Convert.ToDouble(data.Rows[i]["BobbinConsumption"].ToString());
                sheet.Range[ROW, ColBobbinConsumption].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColBobbinConsumption].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColLooperMaterialMaster].Text = data.Rows[i]["LooperMaterialMaster"].ToString();
                sheet[ROW, ColLooperArticle].Text = data.Rows[i]["LooperArticle"].ToString();
                sheet[ROW, ColLooperDescription].Text = data.Rows[i]["LooperDescription"].ToString();
                sheet[ROW, ColLooperConsumption].Number = Convert.ToDouble(data.Rows[i]["LooperConsumption"].ToString());
                sheet.Range[ROW, ColLooperConsumption].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColLooperConsumption].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                ROW++;
            }

            endRow = ROW;
            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[startRow, ColOperation, endRow, ColLooperConsumption].BorderAround(ExcelLineStyle.Thin);

            #region Material Summary Header

            ROW++;
            ROW++;
            sheet.Range[ROW, 2].Text = "MATERIAL SUMMARY";
            sheet.Range[ROW, 2].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 2, ROW, 3].Merge();
            //sheet.Range[ROW, 2, ROW, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[ROW, 2, ROW, 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            sheet.Range[ROW, 4].Text = "Wastage(%): " + WastagePercentage;
            sheet.Range[ROW, 5].Text = "Extra Order(%): " + ExtraOrderPercentage;

            sheet.Range[ROW, 2, ROW, 5].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, 2, ROW, 5].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            ROW++;

            int col = 1;
            report.SetHeaderText(ref sheet, ROW, col, "Thread", 15, ExcelHAlign.HAlignLeft);
            int ColThread = col;
            col++;

            report.SetHeaderText(ref sheet, ROW, col, "Needle Consumption", 25, ExcelHAlign.HAlignCenter);
            int ColNC = col;
            col++;

            report.SetHeaderText(ref sheet, ROW, col, "Bobbin Consumption", 25, ExcelHAlign.HAlignCenter);
            int ColBC = col;
            col++;

            report.SetHeaderText(ref sheet, ROW, col, "Looper Consumption", 17, ExcelHAlign.HAlignCenter);
            int ColLC = col; col++;
            report.SetHeaderText(ref sheet, ROW, col, "Total", 17, ExcelHAlign.HAlignCenter);
            int ColTotal = col++;
            report.SetHeaderText(ref sheet, ROW, col, "Including Wastage(%)", 15, ExcelHAlign.HAlignCenter);
            int ColTotalWastage = col;
            ROW++;
            int strRow = ROW;
            for (int i = 0; i < summaryData.Rows.Count; i++)
            {

                sheet[ROW, ColThread].Text = summaryData.Rows[i]["Thread"].ToString();

                sheet[ROW, ColNC].Number = Convert.ToDouble(summaryData.Rows[i]["NeedleConsumption"].ToString());
                sheet.Range[ROW, ColNC].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColNC].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColBC].Number = Convert.ToDouble(summaryData.Rows[i]["BobbinConsumption"].ToString());
                sheet.Range[ROW, ColBC].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColBC].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColLC].Number = Convert.ToDouble(summaryData.Rows[i]["LooperConsumption"].ToString());
                sheet.Range[ROW, ColLC].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColLC].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColTotal].Formula = clsStaticInfo.GetxlsCol(ColNC) + ROW + "+" + clsStaticInfo.GetxlsCol(ColBC) + ROW + "+" + clsStaticInfo.GetxlsCol(ColLC) + ROW;
                sheet.Range[ROW, ColTotal].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColTotal].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet[ROW, ColTotalWastage].Formula = clsStaticInfo.GetxlsCol(ColTotal) + ROW + "+" + clsStaticInfo.GetxlsCol(ColTotal) + ROW + "*" + totalWastagePercentage / 100;
                sheet.Range[ROW, ColTotalWastage].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColTotalWastage].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                ROW++;
            }

            int edRow = ROW;
            sheet[edRow, 1].Text = "Total";
            sheet.Range[edRow, ColTotal, edRow, ColTotal].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTotal) + strRow + ":" + clsStaticInfo.GetxlsCol(ColTotal) + (edRow - 1) + ")";
            sheet.Range[edRow, ColNC, edRow, ColNC].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColNC) + strRow + ":" + clsStaticInfo.GetxlsCol(ColNC) + (edRow - 1) + ")";
            sheet.Range[edRow, ColBC, edRow, ColBC].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColBC) + strRow + ":" + clsStaticInfo.GetxlsCol(ColBC) + (edRow - 1) + ")";
            sheet.Range[edRow, ColLC, edRow, ColLC].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColLC) + strRow + ":" + clsStaticInfo.GetxlsCol(ColLC) + (edRow - 1) + ")";

            sheet.Range[edRow, ColTotalWastage, edRow, ColTotalWastage].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTotalWastage) + strRow + ":" + clsStaticInfo.GetxlsCol(ColTotalWastage) + (edRow - 1) + ")";

            sheet.Range[edRow, ColNC, edRow, ColTotalWastage].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[edRow, ColNC, edRow, ColTotalWastage].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[strRow, ColThread, edRow, ColTotalWastage].BorderAround(ExcelLineStyle.Thin);

            #endregion

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyHeader(ref sheet, endCol, "Thread Consumption", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

       

        

        #endregion

        #region BulTamplateSummaryReport old
        [HttpGet, Authorize]
        public ActionResult GetBulTamplateSummaryReport(ReportFormat reportFormat, string bulletinTemplateId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Bulletin Template Summary - " + bulletinTemplateId + "";
            var workbook = GetBulTamplateSummaryReportWorkSheet(bulletinTemplateId);
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

        private IWorkbook GetBulTamplateSummaryReportWorkSheet(string bulletinTemplateId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();

            DataTable data = GetBullatinTamplateReportDataByBullatinTamplateId(bulletinTemplateId);
            DataTable dtProcess = new DataView(data).ToTable(true, "Process", "ProcessId");
            var workbook = report.GetWorkbook(ref excelEngine, dtProcess.Rows.Count);
            workbook.Version = ExcelVersion.Excel2016;

            for (int i = 0; i < dtProcess.Rows.Count; i++)
            {
                DataView dv = new DataView(data);
                dv.RowFilter = "ProcessId='" + dtProcess.Rows[i]["ProcessId"].ToString() + "'";

                var sheet = workbook.Worksheets[i];

                CreateSummarySheet(dtProcess.Rows[i]["Process"].ToString(), dv.ToTable(), ref sheet, identity.CompanyId);
            }

            return workbook;
        }

        void CreateSummarySheet(string SheetName, DataTable data, ref IWorksheet sheet, string companyId)
        {
            try
            {
                var report = new ReportUtility();
                sheet.Name = SheetName;

                int ROW = 6;
                int endCol = 1;
                int COL = 1;


                #region Headers
                int rws = ROW;
                sheet.Range[ROW, COL + 1].Text = "Buyer Name";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["Buyer"].ToString().Trim();
                sheet.Range[ROW, COL + 2, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Buyer Style Ref No";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                //sheet.Range[ROW, COL + 3].Text = " " + data.Rows[0]["BuyerStyleRefNo"].ToString().Trim();
                //sheet.Range[ROW, COL, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["BuyerStyleRefNo"].ToString().Trim();
                //sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Own Style Ref No";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["OwnStyleRefNo"].ToString().Trim();
                //sheet.Range[ROW, COL + 3].Number = Convert.ToInt32(data.Rows[0]["OwnStyleRefNo"].ToString().Trim());
                //sheet.Range[ROW, COL + 3].NumberFormat = clsStaticInfo.NumberFormat(0);

                sheet.Range[ROW, COL, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Product Master";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["ProductMaster"].ToString().Trim();
                sheet.Range[ROW, COL, ROW, COL + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();

                double plannedHourPerDay = Convert.ToDouble(data.Rows[0]["PlannedHoursPerDay"]);
                double TotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", null));
                double TotalManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", null));
                double TotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", null));
                double MaxAllotedTime = clsStaticInfo.dbl(data.Compute("Max(AvgAllotedTime)", null));
                double TotalRMP = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", null));

                double PitchTime = 0;
                if (TotalManpower != 0)
                    PitchTime = TotalSPT / TotalManpower;

                double OrgEfficiency = 0;
                if (MaxAllotedTime != 0)
                    OrgEfficiency = PitchTime / MaxAllotedTime;

                double ProdEffPerHour = 0;
                if (TotalSPT != 0)
                    ProdEffPerHour = TotalManpower * 60 / TotalSPT;

                double ProdEffPerDay = ProdEffPerHour * plannedHourPerDay;
                double LineTargetPerHour = ProdEffPerHour * OrgEfficiency;


                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Pitch Time";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Number = PitchTime;
                sheet.Range[ROW, COL + 2].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[ROW, COL, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL, ROW, COL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();

                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Planned Hour PerDay";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Number = plannedHourPerDay;
                sheet.Range[ROW, COL + 2].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[ROW, COL, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL, ROW, COL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();

                int rwe = ROW;
                int PCOL = 4;
                sheet.Range[rws, PCOL].Text = "Particulars";
                sheet.Range[rws, PCOL + 1].Text = "SPT";
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 2].Text = "MP";
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 3].Text = "Work Station";
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 5].Text = "Target(%)";
                sheet.Range[rws, PCOL + 5, rws, PCOL + 5].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 6].Text = "Per Hr";
                sheet.Range[rws, PCOL + 6, rws, PCOL + 6].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 7].Text = "Per Day";
                sheet.Range[rws, PCOL + 7, rws, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignRight;
                rws++;
                sheet.Range[rws, PCOL].Text = "Non MC";
                double NMCTotalSPT = 0;
                double NMCTotalWS = 0;
                double NMCTotalMP = 0;

                NMCTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='No'"));
                NMCTotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='No'"));
                NMCTotalMP = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='No'"));

                sheet.Range[rws, PCOL + 1].Number = NMCTotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 2].Number = NMCTotalMP;
                sheet.Range[rws, PCOL + 2].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 3].Number = NMCTotalWS;

                rws++;
                sheet.Range[rws, PCOL].Text = "MC";
                double MCTotalSPT = 0;
                double MCTotalWS = 0;
                double MCTotalMP = 0;

                MCTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='Yes'"));
                MCTotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='Yes'"));
                MCTotalMP = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='Yes'"));

                sheet.Range[rws, PCOL + 1].Number = MCTotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 2].Number = MCTotalMP;
                sheet.Range[rws, PCOL + 2].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 3].Number = MCTotalWS;


                //sheet.Range[7, PCOL + 4].Text = "100";
                sheet.Range[7, PCOL + 5].Number = Convert.ToInt32("100");
                sheet.Range[7, PCOL + 5].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 5, 7, PCOL + 5].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[7, PCOL + 6].Number = Convert.ToInt32(ProdEffPerHour);
                sheet.Range[7, PCOL + 6].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 6, 7, PCOL + 6].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[7, PCOL + 7].Number = Convert.ToInt32(ProdEffPerDay);
                sheet.Range[7, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 7, 7, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignRight;

                //sheet.Range[8, PCOL + 4].Text = "85";

                sheet.Range[8, PCOL + 5].Number = Convert.ToInt32("85");
                sheet.Range[8, PCOL + 5].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 5, 8, PCOL + 5].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[8, PCOL + 6].Number = Convert.ToInt32(ProdEffPerHour * .85);
                sheet.Range[8, PCOL + 6].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 6, 8, PCOL + 6].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[8, PCOL + 7].Number = Convert.ToInt32(ProdEffPerDay * .85);
                sheet.Range[8, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 7, 8, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignRight;


                //sheet.Range[9, PCOL + 4].Text = "75";

                sheet.Range[9, PCOL + 5].Number = Convert.ToInt32("75");
                sheet.Range[9, PCOL + 5].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 5, 9, PCOL + 5].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[9, PCOL + 6].Number = Convert.ToInt32(ProdEffPerHour * .75);
                sheet.Range[9, PCOL + 6].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 6, 9, PCOL + 6].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[9, PCOL + 7].Number = Convert.ToInt32(ProdEffPerDay * .75);
                sheet.Range[9, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 7, 9, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignRight;

                //sheet.Range[10, PCOL + 4].Text = "65";

                sheet.Range[10, PCOL + 5].Number = Convert.ToInt32("65");
                sheet.Range[10, PCOL + 5].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 5, 10, PCOL + 5].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[10, PCOL + 6].Number = Convert.ToInt32(ProdEffPerHour * .65);
                sheet.Range[10, PCOL + 6].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 6, 10, PCOL + 6].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[10, PCOL + 7].Number = Convert.ToInt32(ProdEffPerDay * .65);
                sheet.Range[10, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 7, 10, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignRight;

                //sheet.Range[11, PCOL + 4].Text = "55";
                sheet.Range[11, PCOL + 5].Number = Convert.ToInt32("55");
                sheet.Range[11, PCOL + 5].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 5, 11, PCOL + 5].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[11, PCOL + 6].Number = Convert.ToInt32(ProdEffPerHour * .55);
                sheet.Range[11, PCOL + 6].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 6, 11, PCOL + 6].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[11, PCOL + 7].Number = Convert.ToInt32(ProdEffPerDay * .55);
                sheet.Range[11, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 7, 11, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignRight;



                // sheet.Range[12, PCOL + 4].Text = "50";

                sheet.Range[12, PCOL + 5].Number = Convert.ToInt32("50");
                sheet.Range[12, PCOL + 5].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 5, 12, PCOL + 5].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[12, PCOL + 6].Number = Convert.ToInt32(ProdEffPerHour * .50);
                sheet.Range[12, PCOL + 6].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 6, 12, PCOL + 6].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[12, PCOL + 7].Number = Convert.ToInt32(ProdEffPerDay * .50);
                sheet.Range[12, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 7, 12, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignRight;



                rws++;
                sheet.Range[rws, PCOL].Text = "Total";
                sheet.Range[rws, PCOL + 1].Number = TotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 2].Number = TotalManpower;
                sheet.Range[rws, PCOL + 2].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 3].Number = TotalWS;

                sheet.Range[6, 4, 12, 11].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[6, 4, 12, 11].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[6, 1, 12, 11].BorderAround(ExcelLineStyle.Thin);

                //sheet.Range[rws, COL, rwe, COL].BorderInside(ExcelLineStyle.Thin);
                //sheet.Range[rws, COL, rwe, COL].BorderAround(ExcelLineStyle.Thin);

                ROW++;
                ROW++;
                ROW++;

                report.SetHeaderText(ref sheet, ROW, COL, "Sr.No.", 8, ExcelHAlign.HAlignCenter);
                int ColSequence = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Description", 30, ExcelHAlign.HAlignLeft);
                int ColMachineVarient = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Machine", 52, ExcelHAlign.HAlignLeft);
                int ColMachineCode = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "SPT(Minutes)", 11, ExcelHAlign.HAlignCenter);
                int ColTotalSPT = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Target/Hr(Pcs)", 13, ExcelHAlign.HAlignCenter);
                int ColOperationTargetPerHr = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Required Man Power", 10, ExcelHAlign.HAlignCenter);
                int ColRequiredManPower = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Alloted Manpower", 10, ExcelHAlign.HAlignCenter);
                int ColAllotedManpower = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Alloted Workstation", 10, ExcelHAlign.HAlignCenter);
                int ColAllotedWorkstation = COL;
                COL++;


                report.SetHeaderText(ref sheet, ROW, COL, "Operation Group", 8, ExcelHAlign.HAlignCenter);
                int ColOperationGroup = COL;
                COL++;


                report.SetHeaderText(ref sheet, ROW, COL, "Avg. Alloted Time", 10, ExcelHAlign.HAlignCenter);
                int ColAvgAllotedTime = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "FG Zone", 12, ExcelHAlign.HAlignLeft);
                int ColFGZone = COL;
                //COL++;


                ROW++;
                endCol = COL;
                #endregion Headers


                string ProcessName = "";
                var startRow = 0;
                var endRow = 0;
                int RowIndex = ROW;
                startRow = ROW;
                int srNo = 0;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    srNo++;

                    sheet[ROW, ColSequence].Number = srNo;
                    //sheet[ROW, ColSequence].Number = clsStaticInfo.dbl(data.Rows[i]["Sequence"].ToString());
                    sheet.Range[ROW, ColSequence, ROW, ColSequence].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColSequence, ROW, ColSequence].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColMachineVarient].Text = data.Rows[i]["OperationVariation"].ToString();
                    sheet[ROW, ColMachineCode].Text = data.Rows[i]["ShortName"].ToString();
                    sheet.Range[ROW, ColTotalSPT].Number = Convert.ToDouble(data.Rows[i]["TotalSPT"].ToString());
                    sheet.Range[ROW, ColTotalSPT].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[ROW, ColTotalSPT, ROW, ColTotalSPT].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColTotalSPT, ROW, ColTotalSPT].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColOperationTargetPerHr].Number = clsStaticInfo.dbl(data.Rows[i]["OperationTargetPerHr"].ToString());
                    sheet.Range[ROW, ColOperationTargetPerHr, ROW, ColOperationTargetPerHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationTargetPerHr, ROW, ColOperationTargetPerHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColRequiredManPower].Number = clsStaticInfo.dbl(data.Rows[i]["RequiredManPower"].ToString());
                    sheet.Range[ROW, ColRequiredManPower, ROW, ColRequiredManPower].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColRequiredManPower, ROW, ColRequiredManPower].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColAllotedManpower].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedManpower"].ToString());
                    sheet.Range[ROW, ColAllotedManpower, ROW, ColAllotedManpower].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAllotedManpower, ROW, ColAllotedManpower].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColAllotedWorkstation].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedWorkstation"].ToString());
                    sheet.Range[ROW, ColAllotedWorkstation, ROW, ColAllotedWorkstation].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAllotedWorkstation, ROW, ColAllotedWorkstation].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    sheet[ROW, ColOperationGroup].Text = data.Rows[i]["OperationGroup"].ToString();
                    sheet.Range[ROW, ColOperationGroup, ROW, ColOperationGroup].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationGroup, ROW, ColOperationGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[ROW, ColAvgAllotedTime].Number = Convert.ToDouble(data.Rows[i]["AvgAllotedTime"].ToString());
                    sheet.Range[ROW, ColAvgAllotedTime].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[ROW, ColAvgAllotedTime, ROW, ColAvgAllotedTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAvgAllotedTime, ROW, ColAvgAllotedTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet[ROW, ColFGZone].Text = data.Rows[i]["FGZone"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    ProcessName = data.Rows[i]["Process"].ToString();

                    ROW++;
                }


                sheet.Range[startRow, ColSequence, ROW, ColFGZone].BorderAround(ExcelLineStyle.Thin);

                endRow = ROW++;

                #region UH

                sheet.Range[endRow, 1].Text = ProcessName + " SPT";
                sheet.Range[endRow, 1].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 4].Number = TotalSPT;
                sheet.Range[endRow, 4].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[endRow, 4].CellStyle.Font.Bold = true;

                //sheet.Range[endRow, 1, endRow, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[endRow, 1, endRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 1, endRow, 3].Merge();

                sheet.Range[endRow, 5].Text = "TOTAL MP";
                sheet.Range[endRow, 5].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 7].Number = TotalManpower;
                sheet.Range[endRow, 7].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet.Range[endRow, 7, endRow, 7].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[endRow, 7].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 8].Number = TotalWS;
                //                sheet.Range[endRow, 8, endRow, 8].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[endRow, 8].CellStyle.Font.Bold = true;

                sheet.Range[endRow, 1, endRow, 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[endRow, 1, endRow, 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                endRow++;
                endRow++;
                int edRow = endRow++;

                sheet.Range[endRow, 1].Text = "MACHINE REQUIREMENT";
                sheet.Range[endRow, 1].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 1, endRow, 3].Merge();
                sheet.Range[endRow, 1, endRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 1, endRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                int col = 2; edRow++; edRow++;
                sheet.Range[edRow, col].Text = "Machine";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                col++;
                sheet.Range[edRow, col].Text = "Machine Variation";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "SAM";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Req MP";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Allotted MP";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Allotted WS";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                #endregion

                //double NMTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='No'"));
                //sheet.Range[edRow, 5].Number = NMTotalSPT;
                //sheet.Range[edRow, 5].NumberFormat = clsStaticInfo.NumberFormat(2); //col++;


                //DataTable dtM = new DataView(data).ToTable(true, "ShortName", "MachineVarientId", "MachineVarient", "AllotedWorkstation", "AllotedManpower", "RequiredManPower", "TotalSPT");
                // DataTable dtM = new DataView(data).ToTable(true, "ShortName", "MachineVarientId", "MachineVarient");
                DataTable dtM = new DataView(data).ToTable(true, "MachineMaster", "MachineVarientId", "ShortName");
                DataView dvM = new DataView(dtM);
                dvM.RowFilter = "MachineVarientId='" + data.Rows[0]["MachineVarientId"].ToString() + "'";
                edRow++;
                int msr = edRow;
                int sc = 2;
                for (int i = 0; i < dtM.Rows.Count; i++)
                {
                    col = 2;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["MachineMaster"].ToString(); col++;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["ShortName"].ToString(); col++;

                    if (!string.IsNullOrEmpty(dtM.Rows[i]["MachineVarientId"].ToString()))
                    {
                        double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MTotalSPT;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);

                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MRequiredManPower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MAllotedManpower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);

                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedWorkstation = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MAllotedWorkstation;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);

                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    else
                    {
                        double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MTotalSPT;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);

                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MRequiredManPower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);

                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MAllotedManpower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);

                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedWorkstation = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MAllotedWorkstation;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);

                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    edRow++;
                }
                int ec = col;
                int mer = edRow;

                sheet.Range[msr - 1, sc, mer, ec].BorderAround(ExcelLineStyle.Thin);

                sheet.Range[edRow, 3].Text = "TOTAL";
                sheet.Range[edRow, 3].CellStyle.Font.Bold = true;

                sheet.Range[edRow, 4].Number = TotalSPT;
                sheet.Range[edRow, 4].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[edRow, 4].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 5].Number = TotalRMP;
                sheet.Range[edRow, 5].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[edRow, 5].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 6].Number = TotalManpower;
                sheet.Range[edRow, 6].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 6].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[edRow, 7].Number = TotalWS;
                sheet.Range[edRow, 7].CellStyle.Font.Bold = true;

                sheet.Range[edRow, 3, edRow, 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[edRow, 3, edRow, 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                //sheet.UsedRange.NumberFormat = "#,##0.000";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                sheet.IsDisplayZeros = true;
                report.CompanyHeader(ref sheet, endCol, "Bulletin Template - " + SheetName + "", companyId);
                report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion  BulTamplateSummaryReport old

        #region Repotrs for ProductionBulletinTemplate 
        [HttpGet, Authorize]
        public ActionResult GetProductionBulletinTemplateReport(ReportFormat reportFormat, string ProductionOrderId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Bulletin Template " + ProductionOrderId + "";
            IWorkbook workbook;
            try
            {
                workbook = GetProductionBulletinTamplateReportWorkSheet(ProductionOrderId);
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }

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
        private IWorkbook GetProductionBulletinTamplateReportWorkSheet(string ProductionOrderId)
        {

            try
            {
                var excelEngine = new ExcelEngine();
                var report = new ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 3);
                workbook.Version = ExcelVersion.Excel2016;

                var sheet = workbook.Worksheets[0];
                var sheet1 = workbook.Worksheets[1];
                var sheet2 = workbook.Worksheets[2];

                sheet.Name = "BulletinTemplate";


                int ROW = 6;
                int endCol = 1;
                int COL = 1;


                DataTable data = clsb.GetProductionBulletinTemplateReportDataByProductionBulletinTemplateId(ProductionOrderId);
                if (data.Rows.Count > 0)
                {
                    int ColBulletinNameHeader = 1;
                    int ColBulletinNameEnd;
                    int ColByWhomHeader;
                    int ColByWhomEnd;
                    int ColByWhom;
                    int ColProductMasterHeader = 1;
                    int ColProductEnd;


                    SetHeaderTextTop(ref sheet, ROW, ColBulletinNameHeader, "Bulletin Name", 12, ExcelHAlign.HAlignLeft);
                    ColBulletinNameHeader++;
                    ColBulletinNameEnd = ColBulletinNameHeader + 1;
                    sheet.Range[ROW, ColBulletinNameHeader, ROW, ColBulletinNameEnd].Text = data.Rows[0]["BulletinName"].ToString();
                    sheet.Range[ROW, ColBulletinNameHeader, ROW, ColBulletinNameEnd].Merge();
                    sheet.Range[ROW, ColBulletinNameHeader, ROW, ColBulletinNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[ROW, ColBulletinNameHeader, ROW, ColBulletinNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    ColBulletinNameEnd++;

                    ColByWhomHeader = ColBulletinNameEnd;
                    SetHeaderTextTop(ref sheet, ROW, ColByWhomHeader, "ByWhom", 20, ExcelHAlign.HAlignLeft);
                    ColByWhomHeader++;
                    ColByWhomEnd = ColByWhomHeader + 1;
                    ColByWhom = ColByWhomHeader;
                    sheet.Range[ROW, ColByWhom, ROW, ColByWhomEnd].Text = data.Rows[0]["ByWhom"].ToString();
                    sheet.Range[ROW, ColByWhom, ROW, ColByWhomEnd].Merge();
                    sheet.Range[ROW, ColByWhom, ROW, ColByWhomEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[ROW, ColByWhom, ROW, ColByWhomEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    ROW++;


                    SetHeaderTextTop(ref sheet, ROW, ColProductMasterHeader, "Product Master", 12, ExcelHAlign.HAlignLeft);
                    ColProductMasterHeader++;
                    ColProductEnd = ColProductMasterHeader + 1;
                    int ColProductMaster = ColProductMasterHeader;
                    sheet.Range[ROW, ColProductMasterHeader, ROW, ColProductEnd].Text = data.Rows[0]["ProductMaster"].ToString();
                    sheet.Range[ROW, ColProductMasterHeader, ROW, ColProductEnd].Merge();
                    sheet.Range[ROW, ColProductMasterHeader, ROW, ColProductEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[ROW, ColProductMasterHeader, ROW, ColProductEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    ColProductEnd++;

                    SetHeaderTextTop(ref sheet, ROW, ColProductEnd, "Size Group", 20, ExcelHAlign.HAlignLeft);
                    ColProductEnd++;
                    int ColSizeGroup = ColProductEnd;
                    int ColSizeGroupEnd = ColProductEnd + 1;
                    sheet.Range[ROW, ColSizeGroup, ROW, ColSizeGroupEnd].Text = data.Rows[0]["SizeGroup"].ToString();
                    sheet.Range[ROW, ColSizeGroup, ROW, ColSizeGroupEnd].Merge();
                    sheet.Range[ROW, ColSizeGroup, ROW, ColSizeGroupEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[ROW, ColSizeGroup, ROW, ColSizeGroupEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    ROW++;

                }
                else
                {
                    throw new Exception("No Data found.");
                }

                #region Headers
                report.SetHeaderText(ref sheet, ROW, COL, "Process", 12, ExcelHAlign.HAlignLeft);
                int ColProcess = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Sequence", 8, ExcelHAlign.HAlignRight);
                int ColSequence = COL;
                COL++;


                report.SetHeaderText(ref sheet, ROW, COL, "Operation Variation", 15, ExcelHAlign.HAlignLeft);
                int ColOperationVariation = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Machine Master", 15, ExcelHAlign.HAlignLeft);
                int ColMachineMaster = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Machine Varient", 26, ExcelHAlign.HAlignLeft);
                int ColMachineVarient = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Skill", 11, ExcelHAlign.HAlignLeft);
                int ColSkill = COL;
                COL++;


                //report.SetHeaderText(ref sheet, ROW, COL, "Additional SPT", 11, ExcelHAlign.HAlignRight);
                //int ColAdditionalSPT = COL;
                //COL++;


                report.SetHeaderText(ref sheet, ROW, COL, "SPT", 10, ExcelHAlign.HAlignRight);
                int ColTotalSPT = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Group", 8, ExcelHAlign.HAlignLeft);
                int ColOperationGroup = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "AVG Total Time", 10, ExcelHAlign.HAlignRight);

                int ColAVGTotalTime = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Alloted Workstation", 10, ExcelHAlign.HAlignRight);
                int ColAllotedWorkstation = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Alloted Manpower", 10, ExcelHAlign.HAlignRight);
                int ColAllotedManpower = COL;
                COL++;



                report.SetHeaderText(ref sheet, ROW, COL, "Frequency", 10, ExcelHAlign.HAlignRight);
                int ColFrequency = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "FGZone", 10, ExcelHAlign.HAlignLeft);
                int ColFGZone = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "FG Component", 12, ExcelHAlign.HAlignLeft);
                int ColFGComponent = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Type", 8, ExcelHAlign.HAlignLeft);
                int ColOperationType = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Consumption", 11, ExcelHAlign.HAlignLeft);
                int ColOperationConsumption = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Gauge Folder", 10, ExcelHAlign.HAlignLeft);
                int ColGaugeFolder = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Category", 8, ExcelHAlign.HAlignLeft);
                int ColOperationCategory = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Quality Level", 8, ExcelHAlign.HAlignLeft);
                int ColQualityLevel = COL;
                ROW++;
                endCol = COL;
                #endregion Headers
                DataView dvOperationGrup = new DataView(data);

                Dictionary<string, double> dist = new Dictionary<string, double>();

                DataTable dtOperationGroup = dvOperationGrup.ToTable(true, "OperationGroup");

                string ProcessName = "";
                var startRow = 0;
                var endRow = 0;
                int RowIndex = ROW;
                startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {

                    if (ProcessName != data.Rows[i]["Process"].ToString())
                    {

                        if (RowIndex < ROW)
                        {
                            sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].Merge();
                            sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        }
                        RowIndex = ROW;
                    }

                    sheet[ROW, ColAVGTotalTime].Number = clsStaticInfo.dbl(data.Rows[i]["AvgAllotedTime"].ToString());
                    sheet[ROW, ColAVGTotalTime].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet[ROW, ColSequence].Number = clsStaticInfo.dbl(data.Rows[i]["Sequence"].ToString());
                    sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                    sheet[ROW, ColOperationVariation].Text = data.Rows[i]["OperationVariation"].ToString();
                    sheet[ROW, ColMachineMaster].Text = data.Rows[i]["MachineMaster"].ToString();
                    sheet[ROW, ColMachineVarient].Text = data.Rows[i]["MachineVarient"].ToString();

                    sheet[ROW, ColSkill].Text = data.Rows[i]["Skill"].ToString();
                    sheet[ROW, ColOperationGroup].Text = data.Rows[i]["OperationGroup"].ToString();

                    sheet.Range[ROW, ColTotalSPT].Number = Convert.ToDouble(data.Rows[i]["TotalSPT"].ToString());
                    sheet.Range[ROW, ColTotalSPT].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[ROW, ColTotalSPT, ROW, ColTotalSPT].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet[ROW, ColAllotedWorkstation].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedWorkstation"].ToString());

                    sheet[ROW, ColAllotedManpower].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedManpower"].ToString());
                    sheet[ROW, ColFrequency].Number = clsStaticInfo.dbl(data.Rows[i]["Frequency"].ToString());
                    sheet[ROW, ColFGZone].Text = data.Rows[i]["FGZone"].ToString();
                    sheet[ROW, ColFGComponent].Text = data.Rows[i]["FGComponent"].ToString();

                    sheet[ROW, ColOperationType].Text = data.Rows[i]["OperationType"].ToString();
                    sheet[ROW, ColOperationConsumption].Text = data.Rows[i]["OperationConsumption"].ToString();
                    sheet[ROW, ColGaugeFolder].Text = data.Rows[i]["GaugeFolder"].ToString();
                    sheet[ROW, ColOperationCategory].Text = data.Rows[i]["OperationCategory"].ToString();
                    sheet[ROW, ColQualityLevel].Text = data.Rows[i]["QualityLevel"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    ProcessName = data.Rows[i]["Process"].ToString();

                    ROW++;
                }

                endRow = ROW - 1;

                if (RowIndex < ROW - 1)
                {
                    sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].Merge();
                    sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[RowIndex, ColProcess, ROW - 1, ColProcess].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                }

                GetWorkSheetBulletinTamplateCalculation(ref sheet1, ref report, data, "Production Bulletin Tamplate Calculation");
                GetWorkSheetTamplateFormula(ref sheet2, ref report, data, "Production Bulletin Tamplate Calculation");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                sheet.UsedRange.NumberFormat = "#,##0.000";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                report.CompanyPlantHeader(ref sheet, endCol, "Production Bulletin Tamplate", identity.CompanyId, identity.PlantName, null);
                report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        



        [HttpGet, Authorize]
        public ActionResult GetBulletinTamplateProductionDetailReport(ReportFormat reportFormat, string ProductionOrderId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Production Bulletin Template Detail - " + ProductionOrderId + "";
            var workbook = GetBulletinTamplateProductionDetailReportWorkSheet(ProductionOrderId);
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

        private IWorkbook GetBulletinTamplateProductionDetailReportWorkSheet(string ProductionOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();

            DataTable data = clsb.GetProductionBulletinTemplateReportDataByProductionBulletinTemplateId(ProductionOrderId);
            DataTable dtProcess = new DataView(data).ToTable(true, "Process", "ProcessId");
            var workbook = report.GetWorkbook(ref excelEngine, dtProcess.Rows.Count);
            workbook.Version = ExcelVersion.Excel2016;



            for (int i = 0; i < dtProcess.Rows.Count; i++)
            {
                DataView dv = new DataView(data);
                dv.RowFilter = "ProcessId='" + dtProcess.Rows[i]["ProcessId"].ToString() + "'";

                var sheet = workbook.Worksheets[i];

                CreateProductionDetailSheet(dtProcess.Rows[i]["Process"].ToString(), dv.ToTable(), ref sheet, identity.CompanyId);
            }

            return workbook;
        }

        
        void CreateProductionDetailSheet(string SheetName, DataTable data, ref IWorksheet sheet, string companyId)
        {
            try
            {
                var report = new ReportUtility();
                sheet.Name = SheetName;

                int ROW = 6;
                int endCol = 1;
                int COL = 5;

                sheet.Range[6, 2, 14, 4].BorderAround(ExcelLineStyle.Double);
                string ImageExt = Path.GetExtension(data.Rows[0]["PicFileName"].ToString());
                string IdImage = data.Rows[0]["Id"].ToString();
                #region Image
                try
                {
                    string strPath = Path.Combine(ResourcesPathReader.GetProductionBulletinImagePath(), IdImage + ImageExt);
                    Image companyLogo = Image.FromFile(strPath);
                    if (companyLogo != null)
                    {
                        double totalWidth = sheet.GetColumnWidth(1) + sheet.GetColumnWidth(28);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet.GetRowHeight(1) + sheet.GetRowHeight(2) + sheet.GetRowHeight(36) + sheet.GetRowHeight(36)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet.Pictures.AddPicture(6, 2, companyLogo);


                    }


                }
                catch (Exception)
                {


                }
                #endregion

                #region Headers
                int rws = ROW;
                sheet.Range[ROW, COL + 1].Text = "Production OrderId";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["ProductionOrderId"].ToString().Trim();
                sheet.Range[ROW, COL + 2, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Bulletin Buyer Style Ref No";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["BulletinBuyerStyleRefNo"].ToString().Trim();
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "SO Description";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["Description"].ToString().Trim();
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Buyer Name";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["Buyer"].ToString().Trim();
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Buyer Style Ref No";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["BuyerOrder"].ToString().Trim();
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Own Style Ref No";
                int rowBulletinTarget = ROW;
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["OwnOrder"].ToString().Trim();

                sheet.Range[ROW, COL, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Product Master";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 3].Text = " " + data.Rows[0]["ProductMaster"].ToString().Trim();
                sheet.Range[ROW, COL, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();

                double plannedHourPerDay = Convert.ToDouble(data.Rows[0]["PlannedHoursPerDay"]);
                double TotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", null));
                double TotalManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", null));
                double TotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", null));
                double TotalRMP = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", null));
                double MaxAllotedTime = clsStaticInfo.dbl(data.Compute("Max(AvgAllotedTime)", null));

                double PitchTime = 0;
                if (TotalManpower != 0)
                    PitchTime = TotalSPT / TotalManpower;

                double OrgEfficiency = 0;
                if (MaxAllotedTime != 0)
                    OrgEfficiency = PitchTime / MaxAllotedTime;

                double ProdEffPerHour = 0;
                if (TotalSPT != 0)
                    ProdEffPerHour = TotalManpower * 60 / TotalSPT;

                double ProdEffPerDay = ProdEffPerHour * plannedHourPerDay;
                double LineTargetPerHour = ProdEffPerHour * OrgEfficiency;


                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Pitch Time";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Number = PitchTime;
                sheet.Range[ROW, COL + 2].NumberFormat = clsStaticInfo.NumberFormat(4);
                sheet.Range[ROW, COL, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL, ROW, COL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();

                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Planned Hour PerDay";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Number = plannedHourPerDay;
                sheet.Range[ROW, COL + 2].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[ROW, COL, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL, ROW, COL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();

                sheet.Range[6, 5, 14, 8].BorderAround(ExcelLineStyle.Thick);


                int rwe = 6;
                int PCOL = 8;
                sheet.Range[rws, PCOL].Text = "Particulars";
                sheet.Range[rws, PCOL + 1].Text = "SPT(Minutes)";
                sheet.Range[rws, PCOL + 2].Text = "MP";
                sheet.Range[rws, PCOL + 3].Text = "Work Station";
                sheet.Range[rws, PCOL + 4].Text = "Bullitin Target(Pcs)";
                int colBulletinTarget = PCOL + 4;
                sheet.Range[rws, PCOL + 5].Text = "Planned Efficency(%)";
                sheet.Range[rws, PCOL + 6].Text = "Planned Per Man productivity(Pcs)";

                sheet.Range[6, 12, 14, 14].BorderAround(ExcelLineStyle.Thick);
                sheet.Range[rws, PCOL, rws, PCOL + 6].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL + 6].VerticalAlignment = ExcelVAlign.VAlignCenter;


                sheet.Range[rws, PCOL + 7].Text = "Target(%)";
                sheet.Range[rws, PCOL + 8].Text = "Per Hr";
                sheet.Range[rws, PCOL + 9].Text = "Per Day";

                sheet.Range[rws, PCOL + 7, rws, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 7, rws, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[6, 15, 14, 17].BorderAround(ExcelLineStyle.Thick);

                sheet.Range[rws, PCOL + 10].Text = "Created By";
                sheet.Range[rws, PCOL + 10, rws, PCOL + 10].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 11].Text = "Creation Date";
                sheet.Range[rws, PCOL + 11, rws, PCOL + 11].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 12].Text = "Revision";
                sheet.Range[rws, PCOL + 12, rws, PCOL + 12].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 13].Text = "Approved By";
                sheet.Range[rws, PCOL + 13, rws, PCOL + 13].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[6, 18, 14, 21].BorderAround(ExcelLineStyle.Thick);

                rws++;
                sheet.Range[rws, PCOL].Text = "Non MC";
                sheet.Range[rws, PCOL, rws, PCOL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                double NMCTotalSPT = 0;
                double NMCTotalWS = 0;
                double NMCTotalMP = 0;

                NMCTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='No'"));
                NMCTotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='No'"));
                NMCTotalMP = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='No'"));

                sheet.Range[rws, PCOL + 1].Number = NMCTotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 2].Number = NMCTotalMP;
                sheet.Range[rws, PCOL + 2].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 3].Number = NMCTotalWS;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                rws++;
                sheet.Range[rws, PCOL].Text = "MC";
                sheet.Range[rws, PCOL, rws, PCOL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                double MCTotalSPT = 0;
                double MCTotalWS = 0;
                double MCTotalMP = 0;

                MCTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='Yes'"));
                MCTotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='Yes'"));
                MCTotalMP = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='Yes'"));

                sheet.Range[rws, PCOL + 1].Number = MCTotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 2].Number = MCTotalMP;
                sheet.Range[rws, PCOL + 2].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 3].Number = MCTotalWS;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                rws++;
                sheet.Range[rws, PCOL].Text = "Total";
                sheet.Range[rws, PCOL, rws, PCOL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 1].Number = TotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 2].Number = TotalManpower; string ToTalManpowerCellAddr = clsStaticInfo.GetxlsCol(PCOL + 2) + (rws.ToString());
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 3].Number = TotalWS;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, 8, rws, 11].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[rws, 8, rws, 11].CellStyle.Font.Bold = true;
                sheet.Range[rws, 8, rws, 11].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[6, 8, 14, 11].BorderAround(ExcelLineStyle.Thick);

                //sheet.Range[7, PCOL + 4].Text = "100";
                sheet.Range[7, PCOL + 7].Number = Convert.ToInt32("100");
                sheet.Range[7, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 7, 7, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[7, PCOL + 7, 7, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;


                string CellTargetAt100PercentEfficiency = clsStaticInfo.GetxlsCol(PCOL + 8) + "7";
                sheet.Range[7, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour);
                sheet.Range[7, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 8, 7, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[7, PCOL + 8, 7, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[7, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay);
                sheet.Range[7, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 9, 7, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[7, PCOL + 9, 7, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;



                //sheet.Range[8, PCOL + 4].Text = "85";

                sheet.Range[8, PCOL + 7].Number = Convert.ToInt32("85");
                sheet.Range[8, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 7, 8, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[8, PCOL + 7, 8, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[8, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .85);
                sheet.Range[8, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 8, 8, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[8, PCOL + 8, 8, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[8, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .85);
                sheet.Range[8, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 9, 8, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[8, PCOL + 9, 8, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;


                //sheet.Range[9, PCOL + 4].Text = "75";

                sheet.Range[9, PCOL + 7].Number = Convert.ToInt32("75");
                sheet.Range[9, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 7, 9, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[9, PCOL + 7, 9, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[9, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .75);
                sheet.Range[9, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 8, 9, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[9, PCOL + 8, 9, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[9, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .75);
                sheet.Range[9, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 9, 9, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[9, PCOL + 9, 9, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;

                //sheet.Range[10, PCOL + 4].Text = "65";

                sheet.Range[10, PCOL + 7].Number = Convert.ToInt32("65");
                sheet.Range[10, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 7, 10, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[10, PCOL + 7, 10, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[10, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .65);
                sheet.Range[10, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 8, 10, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[10, PCOL + 8, 10, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;


                sheet.Range[10, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .65);
                sheet.Range[10, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 9, 10, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[10, PCOL + 9, 10, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;


                //sheet.Range[11, PCOL + 4].Text = "55";
                sheet.Range[11, PCOL + 7].Number = Convert.ToInt32("55");
                sheet.Range[11, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 7, 11, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[11, PCOL + 7, 11, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[11, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .55);
                sheet.Range[11, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 8, 11, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[11, PCOL + 8, 11, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[11, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .55);
                sheet.Range[11, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 9, 11, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[11, PCOL + 9, 11, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;



                // sheet.Range[12, PCOL + 4].Text = "50";

                sheet.Range[12, PCOL + 7].Number = Convert.ToInt32("50");
                sheet.Range[12, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 7, 12, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[12, PCOL + 7, 12, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[12, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .50);
                sheet.Range[12, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 8, 12, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[12, PCOL + 8, 12, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[12, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .50);
                sheet.Range[12, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 9, 12, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[12, PCOL + 9, 12, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;

                // Created By	Creation Date Revision

                sheet.Range[7, PCOL + 10].Text = data.Rows[0]["AddedBy"].ToString();
                sheet.Range[7, PCOL + 11].Text = data.Rows[0]["AddedDate"].ToString();
                sheet.Range[7, PCOL + 12].Text = "1";


                //sheet.Range[rws, COL, rwe, COL].BorderInside(ExcelLineStyle.Thin);
                //sheet.Range[rws, COL, rwe, COL].BorderAround(ExcelLineStyle.Thin);

                sheet.Range[6, 5, 6, 21].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[6, 5, 6, 21].CellStyle.Font.Bold = true;
                sheet.Range[6, 5, 6, 21].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[6, 5, 6, 21].CellStyle.Font.Size = 9f;
                sheet.Range[6, 5, 6, 21].RowHeight = 30;
                sheet.Range[6, 5, 6, 21].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[6, 5, 6, 21].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[6, 5, 6, 21].BorderInside(ExcelLineStyle.Hair);


                ROW++;
                ROW++;
                ROW++;
                COL = 2;
                int sCol = COL;

                #region SetHeaderText


                report.SetHeaderText(ref sheet, ROW, COL, "Sr.No.", 8, ExcelHAlign.HAlignCenter);
                int ColSequence = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Code", 10, ExcelHAlign.HAlignCenter);
                int ColOperationCode = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Description", 28, ExcelHAlign.HAlignLeft);
                int ColMachineVarient = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Machine", 52, ExcelHAlign.HAlignLeft);
                int ColMachineCode = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "FG Zone", 15, ExcelHAlign.HAlignLeft);
                int ColFGZone = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "FG Component", 15, ExcelHAlign.HAlignLeft);
                int ColFGComponent = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Category", 15, ExcelHAlign.HAlignCenter);
                int ColOperationCategory = COL;
                COL++;



                report.SetHeaderText(ref sheet, ROW, COL, "SPT(Minutes)", 11, ExcelHAlign.HAlignCenter);
                int ColTotalSPT = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Target/Hr(Pcs)", 15, ExcelHAlign.HAlignCenter);
                int ColOperationTargetPerHr = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Required Man Power", 11, ExcelHAlign.HAlignCenter);
                int ColRequiredManPower = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Alloted Manpower", 11, ExcelHAlign.HAlignCenter);
                int ColAllotedManpower = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Alloted Workstation", 13, ExcelHAlign.HAlignCenter);
                int ColAllotedWorkstation = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Group", 18, ExcelHAlign.HAlignCenter);
                int ColOperationGroup = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Avg.Alloted Time", 13, ExcelHAlign.HAlignCenter);
                int ColAvgAllotedTime = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation wise O/p per Hr.", 13, ExcelHAlign.HAlignCenter);
                int ColOperationWiseOutputPerHour = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Attachment", 15, ExcelHAlign.HAlignLeft);
                int ColAttachment = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Gauge Folder", 15, ExcelHAlign.HAlignLeft);
                int ColGaugeFolder = COL;
                COL++;


                report.SetHeaderText(ref sheet, ROW, COL, "Operation Type", 15, ExcelHAlign.HAlignLeft);
                int ColOperationType = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Quality Level", 15, ExcelHAlign.HAlignLeft);
                int ColQualityLevel = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Frequency", 12, ExcelHAlign.HAlignCenter);
                int ColFrequency = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Remark", 15, ExcelHAlign.HAlignLeft);
                int ColRemark = COL;
                endCol = COL;
                sheet.Range[ROW, sCol, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[ROW, sCol, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, sCol, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, sCol, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, sCol, ROW, endCol].RowHeight = 30;
                sheet.Range[ROW, sCol, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, sCol, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                COL++;
                COL++;

                sheet[ROW, COL].Text = "Target On Org. Efficiency";
                sheet[ROW, COL].ColumnWidth = 0.05f;
                int ColTargetOnOrgEff = COL;
                COL++;

                sheet[ROW, COL].Text = "Production @ 100% Eff";
                sheet[ROW, COL].ColumnWidth = 0.05f;

                int colProductionAt100PercentEfficiency = COL;
                COL++;
                sheet[ROW, COL].Text = "Line Req Tgt";
                sheet[ROW, COL].ColumnWidth = 0.05f;

                int colLineReqTgt = COL;
                endCol = COL;


                #endregion

                ROW++;
                endCol = COL;
                #endregion Headers


                string ProcessName = "";
                var startRow = 0;
                var endRow = 0;
                int RowIndex = ROW;
                startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {

                    sheet[ROW, ColSequence].Number = clsStaticInfo.dbl(data.Rows[i]["Sequence"].ToString());
                    sheet.Range[ROW, ColSequence, ROW, ColSequence].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColSequence, ROW, ColSequence].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColOperationCode].Text = data.Rows[i]["OperationCode"].ToString();
                    sheet.Range[ROW, ColOperationCode, ROW, ColOperationCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationCode, ROW, ColOperationCode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColMachineVarient].Text = data.Rows[i]["OperationVariation"].ToString();
                    sheet[ROW, ColMachineCode].Text = data.Rows[i]["ShortName"].ToString();
                    sheet.Range[ROW, ColTotalSPT].Number = Convert.ToDouble(data.Rows[i]["TotalSPT"].ToString());
                    sheet.Range[ROW, ColTotalSPT].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[ROW, ColTotalSPT, ROW, ColTotalSPT].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColTotalSPT, ROW, ColTotalSPT].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColOperationTargetPerHr].Number = clsStaticInfo.dbl(data.Rows[i]["OperationTargetPerHr"].ToString());
                    sheet.Range[ROW, ColOperationTargetPerHr, ROW, ColOperationTargetPerHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationTargetPerHr, ROW, ColOperationTargetPerHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColRequiredManPower].Number = clsStaticInfo.dbl(data.Rows[i]["RequiredManPower"].ToString());
                    sheet.Range[ROW, ColRequiredManPower, ROW, ColRequiredManPower].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColRequiredManPower, ROW, ColRequiredManPower].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColAllotedManpower].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedManpower"].ToString());
                    sheet.Range[ROW, ColAllotedManpower, ROW, ColAllotedManpower].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAllotedManpower, ROW, ColAllotedManpower].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColAllotedWorkstation].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedWorkstation"].ToString());
                    sheet.Range[ROW, ColAllotedWorkstation, ROW, ColAllotedWorkstation].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAllotedWorkstation, ROW, ColAllotedWorkstation].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    sheet[ROW, ColOperationGroup].Text = data.Rows[i]["OperationGroup"].ToString();
                    sheet.Range[ROW, ColOperationGroup, ROW, ColOperationGroup].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationGroup, ROW, ColOperationGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[ROW, ColAvgAllotedTime].Number = Convert.ToDouble(data.Rows[i]["AvgAllotedTime"].ToString());
                    sheet.Range[ROW, ColAvgAllotedTime].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[ROW, ColAvgAllotedTime, ROW, ColAvgAllotedTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAvgAllotedTime, ROW, ColAvgAllotedTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet.Range[ROW, ColOperationWiseOutputPerHour].Formula = "60/" + clsStaticInfo.GetxlsCol(ColAvgAllotedTime) + ROW;
                    sheet.Range[ROW, ColOperationWiseOutputPerHour].NumberFormat = clsStaticInfo.NumberFormat(0);


                    sheet[ROW, ColFGZone].Text = data.Rows[i]["FGZone"].ToString();
                    sheet[ROW, ColFGComponent].Text = data.Rows[i]["FGComponent"].ToString();
                    sheet[ROW, ColOperationCategory].Text = data.Rows[i]["OperationCategory"].ToString();
                    sheet.Range[ROW, ColOperationCategory, ROW, ColOperationCategory].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationCategory, ROW, ColOperationCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColAttachment].Text = data.Rows[i]["Attachment"].ToString();
                    sheet[ROW, ColGaugeFolder].Text = data.Rows[i]["GaugeFolder"].ToString();
                    //sheet[ROW, ColOperationConsumption].Text = data.Rows[i]["OperationConsumption"].ToString();
                    sheet[ROW, ColOperationType].Text = data.Rows[i]["OperationType"].ToString();
                    sheet[ROW, ColQualityLevel].Text = data.Rows[i]["QualityLevel"].ToString();


                    sheet[ROW, ColFrequency].Number = clsStaticInfo.dbl(data.Rows[i]["Frequency"].ToString());
                    sheet.Range[ROW, ColFrequency].NumberFormat = clsStaticInfo.NumberFormat(0);
                    sheet.Range[ROW, ColFrequency, ROW, ColFrequency].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    sheet[ROW, ColRemark].Text = data.Rows[i]["Remark"].ToString();


                    sheet.Range[ROW, 1, ROW, ColRemark].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, ColRemark].BorderAround(ExcelLineStyle.Hair);



                    sheet[ROW, colLineReqTgt].Formula = clsStaticInfo.GetxlsCol(colBulletinTarget) + rowBulletinTarget;
                    sheet[ROW, colLineReqTgt].NumberFormat = clsStaticInfo.NumberFormat(0);

                    sheet[ROW, colProductionAt100PercentEfficiency].Formula = CellTargetAt100PercentEfficiency;
                    sheet[ROW, colProductionAt100PercentEfficiency].NumberFormat = clsStaticInfo.NumberFormat(0);


                    sheet[ROW, ColTargetOnOrgEff].Formula = "(" + CellTargetAt100PercentEfficiency + ")*" + OrgEfficiency.ToString();
                    sheet[ROW, ColTargetOnOrgEff].NumberFormat = clsStaticInfo.NumberFormat(0);


                    //sheet.Range[ROW, ColRemark + 2, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    //sheet.Range[ROW, ColRemark + 2, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    ProcessName = data.Rows[i]["Process"].ToString();

                    ROW++;
                }

                #region graph

                IChartShape chart = sheet.Charts.Add();
                //Set chart type
                chart.ChartType = ExcelChartType.Column_Clustered;
                //Set Chart Title
                chart.ChartTitle = "Per Hour Production Comparison";

                //Output Base on BPT
                IChartSerie ChartOperationOpt = chart.Series.Add("Operation Wise Output Per Hour");
                ChartOperationOpt.SerieType = ExcelChartType.Column_Clustered;
                ChartOperationOpt.Values = sheet.Range[startRow, ColOperationWiseOutputPerHour, ROW - 1, ColOperationWiseOutputPerHour];
                // productA.CategoryLabels = sheet1.Range["A2:A6"];

                //colLineReqTg
                IChartSerie ChartLineReq = chart.Series.Add("Plan Target");
                ChartLineReq.SerieType = ExcelChartType.Line;
                ChartLineReq.Values = sheet.Range[startRow, colLineReqTgt, ROW - 1, colLineReqTgt];

                //colProductionAt100PercentEfficiency
                IChartSerie ChartProductionAt100PercentEfficiency = chart.Series.Add("Std. Target at 100%");
                ChartProductionAt100PercentEfficiency.SerieType = ExcelChartType.Line;
                ChartProductionAt100PercentEfficiency.Values = sheet.Range[startRow, colProductionAt100PercentEfficiency, ROW - 1, colProductionAt100PercentEfficiency];


                //colProductionAt100PercentEfficiency
                IChartSerie ChartTargetOnOrgEff = chart.Series.Add("Target at Org. Eff.");
                ChartTargetOnOrgEff.SerieType = ExcelChartType.Line;
                ChartTargetOnOrgEff.Values = sheet.Range[startRow, ColTargetOnOrgEff, ROW - 1, ColTargetOnOrgEff];


                for (int i = 1; i <= endCol; i++)
                    chart.XPos += sheet[1, i].ColumnWidth * 7.5;

                chart.YPos = 240;

                chart.Legend.Position = ExcelLegendPosition.Bottom;
                chart.Scale(50, 100);
                #endregion graph

                sheet.Range[startRow, ColSequence, ROW, ColRemark].BorderAround(ExcelLineStyle.Thick);
                endRow = ROW++;
                #region UH


                sheet.Range[endRow, 2].Text = "Total SPT";
                sheet.Range[endRow, 2, endRow, 3].Merge();
                sheet.Range[endRow, 3].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 9].Number = TotalSPT;
                sheet.Range[endRow, 9].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[endRow, 9].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 9, endRow, 9].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 9, endRow, 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[endRow, 10].Text = "TOTAL MP";
                sheet.Range[endRow, 10].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 12].Number = TotalManpower;
                sheet.Range[endRow, 12, endRow, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 12, endRow, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[endRow, 12].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 13].Number = TotalWS;
                sheet.Range[endRow, 13, endRow, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 13, endRow, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[endRow, 13].CellStyle.Font.Bold = true;

                endRow++;
                endRow++;
                int edRow = endRow++;
                int edCRow = edRow;

                sheet.Range[endRow, 4].Text = "MACHINE & MANPOWER REQUIREMENT SUMMARY";
                sheet.Range[endRow, 4].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 4, endRow, 5].Merge();

                int col = 4; edRow++; edRow++;
                sheet.Range[edRow, col].Text = "Machine";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                col++;
                sheet.Range[edRow, col].Text = "Machine Variation";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;

                col++;
                sheet.Range[edRow, col].Text = "SPT(Min)";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Req MP";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Allotted MP";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Allotted WS";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 4, edRow, 9].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[edRow, 4, edRow, 9].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 4, edRow, 9].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[edRow, 4, edRow, 9].CellStyle.Font.Size = 9f;
                sheet.Range[edRow, 4, edRow, 9].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[edRow, 4, edRow, 9].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[edRow, 4, edRow, 9].BorderAround(ExcelLineStyle.Thick);
                #endregion

                //DataTable dtM = new DataView(data).ToTable(true, "ShortName", "MachineVarientId", "MachineVarient", "AllotedWorkstation", "AllotedManpower", "RequiredManPower", "TotalSPT");
                DataTable dtM = new DataView(data).ToTable(true, "MachineMaster", "MachineVarientId", "ShortName");
                DataView dvM = new DataView(dtM);
                dvM.RowFilter = "MachineVarientId='" + data.Rows[0]["MachineVarientId"].ToString() + "'";
                edRow++;
                int msr = edRow;
                int sc = 4;
                int ec = 0;
                for (int i = 0; i < dtM.Rows.Count; i++)
                {

                    col = 4;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["MachineMaster"].ToString(); col++;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["ShortName"].ToString(); col++;

                    if (!string.IsNullOrEmpty(dtM.Rows[i]["MachineVarientId"].ToString()))
                    {
                        double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MTotalSPT;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MRequiredManPower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MAllotedManpower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedWorkstation = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MAllotedWorkstation;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    else
                    {
                        double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MTotalSPT;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MRequiredManPower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MAllotedManpower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedWorkstation = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MAllotedWorkstation;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    edRow++;
                }
                ec = col;
                int mer = edRow;


                sheet.Range[edRow, 4].Text = "TOTAL";
                sheet.Range[edRow, 4].CellStyle.Font.Bold = true;

                sheet.Range[edRow, 6].Number = TotalSPT;
                sheet.Range[edRow, 6].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[edRow, 6].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 6, edRow, 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 6, edRow, 6].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 7].Number = TotalRMP;
                sheet.Range[edRow, 7].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 7, edRow, 7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 7, edRow, 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 8].Number = TotalManpower;
                sheet.Range[edRow, 8].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 8, edRow, 8].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 8, edRow, 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 9].Number = TotalWS;
                sheet.Range[edRow, 9].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 9, edRow, 9].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 9, edRow, 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                sheet.Range[8, 12].Number = clsStaticInfo.dbl(data.Rows[0]["RequiredStdTarget"].ToString());
                sheet.Range[8, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[8, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[9, 12].Number = clsStaticInfo.dbl(data.Rows[0]["TotalBT"].ToString());
                sheet.Range[9, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[9, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                double rst = Convert.ToDouble(data.Rows[0]["RequiredStdTarget"]);
                double tbt = Convert.ToDouble(data.Rows[0]["TotalBT"]);
                double peHr = (rst / ProdEffPerHour) * 100;
                double peday = (tbt / ProdEffPerDay) * 100;
                sheet.Range[8, 13].Number = clsStaticInfo.dbl(peHr);
                sheet.Range[8, 13].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[8, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[8, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[9, 13].Number = clsStaticInfo.dbl(peday);
                sheet.Range[9, 13].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[9, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[9, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[8, 14].Number = clsStaticInfo.dbl(rst / TotalManpower);
                sheet.Range[8, 14].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[8, 14].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[8, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[9, 14].Number = clsStaticInfo.dbl((rst / TotalManpower) * plannedHourPerDay);
                sheet.Range[9, 14].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[9, 14].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[9, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[msr, sc, mer, ec].BorderAround(ExcelLineStyle.Thick);


                edCRow++; edCRow++;
                int Ccol = 11;
                int Cmsr = edCRow;
                int Csc = 11;//edCRow++;
                sheet.Range[edCRow, Ccol].Text = "Operation Category";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "SAM";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "Req MP";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "Allotted MP";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "Skill(%)";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                DataTable dtOC = new DataView(data).ToTable(true, "OperationCategory");
                dtOC.DefaultView.Sort = "OperationCategory ASC";
                dtOC = dtOC.DefaultView.ToTable();
                edCRow++;
                double tpercent = 0;
                for (int i = 0; i < dtOC.Rows.Count; i++)
                {

                    Ccol = 11;
                    sheet.Range[edCRow, Ccol].Text = dtOC.Rows[i]["OperationCategory"].ToString();
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "OperationCategory='" + dtOC.Rows[i]["OperationCategory"].ToString() + "'"));
                    sheet.Range[edCRow, Ccol].Number = MTotalSPT;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "OperationCategory='" + dtOC.Rows[i]["OperationCategory"].ToString() + "'"));
                    sheet.Range[edCRow, Ccol].Number = MRequiredManPower;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "OperationCategory='" + dtOC.Rows[i]["OperationCategory"].ToString() + "'"));
                    sheet.Range[edCRow, Ccol].Number = MAllotedManpower;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double calPer = (MAllotedManpower / TotalManpower) * 100;
                    tpercent += calPer;

                    sheet.Range[edCRow, Ccol].Number = Math.Round(calPer);
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(0);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    edCRow++;
                }

                int Cec = Ccol;
                int Cmer = edCRow;

                sheet.Range[edCRow, 11].Text = "TOTAL";
                sheet.Range[edCRow, 11].CellStyle.Font.Bold = true;

                sheet.Range[edCRow, 12].Number = TotalSPT;
                sheet.Range[edCRow, 12].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 12].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 12, edCRow, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 12, edCRow, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edCRow, 13].Number = TotalRMP;
                sheet.Range[edCRow, 13].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 13, edCRow, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 13, edCRow, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edCRow, 14].Number = TotalManpower;
                sheet.Range[edCRow, 14].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 14, edCRow, 14].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 14, edCRow, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edCRow, 15].Number = tpercent;
                sheet.Range[edCRow, 15].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 15, edCRow, 15].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 15, edCRow, 15].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Font.Bold = true;
                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Font.Size = 9f;
                sheet.Range[Cmsr, 11, Cmsr, 15].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[Cmsr, 11, Cmsr, 15].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[Cmsr, Csc, Cmer, Cec].BorderAround(ExcelLineStyle.Thick);


                //sheet.UsedRange.NumberFormat = "#,##0.000";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                sheet.IsGridLinesVisible = false;


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet, 2, "Bulletin Template Detail - " + SheetName + "", identity.CompanyId, identity.CompanyName, "");
                sheet.Range[1, 2, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //report.CompanyHeader(ref sheet, endCol, "Bulletin Template - " + SheetName + "", companyId);
                report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetProductionBulletinTamplateSummaryReport(ReportFormat reportFormat, string ProductionOrderId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Production Bulletin Template Summary - " + ProductionOrderId + "";
            var workbook = GetProductionBulletinTamplateSummaryReportWorkSheet(ProductionOrderId);
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

        private IWorkbook GetProductionBulletinTamplateSummaryReportWorkSheet(string ProductionOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();

            DataTable data = clsb.GetProductionBulletinTemplateReportDataByProductionBulletinTemplateId(ProductionOrderId);
            DataTable dtProcess = new DataView(data).ToTable(true, "Process", "ProcessId");
            var workbook = report.GetWorkbook(ref excelEngine, dtProcess.Rows.Count);
            workbook.Version = ExcelVersion.Excel2016;



            for (int i = 0; i < dtProcess.Rows.Count; i++)
            {
                DataView dv = new DataView(data);
                dv.RowFilter = "ProcessId='" + dtProcess.Rows[i]["ProcessId"].ToString() + "'";

                var sheet = workbook.Worksheets[i];

                CreateProductionSheet(dtProcess.Rows[i]["Process"].ToString(), dv.ToTable(), ref sheet, identity.CompanyId);
            }

            return workbook;
        }

        void CreateProductionSheet(string SheetName, DataTable data, ref IWorksheet sheet, string companyId)
        {
            try
            {
                var report = new ReportUtility();
                sheet.Name = SheetName;
                int colPitchTime = 0;
                int rowPitchTime = 0;
                int ROW = 6;
                int endCol = 1;
                int COL = 5;
                string ImageExt = Path.GetExtension(data.Rows[0]["PicFileName"].ToString());
                string IdImage = data.Rows[0]["Id"].ToString();
                #region Image
                try
                {
                    string strPath = Path.Combine(ResourcesPathReader.GetProductionBulletinImagePath(), IdImage + ImageExt);
                    Image companyLogo = Image.FromFile(strPath);
                    if (companyLogo != null)
                    {
                        double totalWidth = sheet.GetColumnWidth(1) + sheet.GetColumnWidth(28);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet.GetRowHeight(1) + sheet.GetRowHeight(2) + sheet.GetRowHeight(36) + sheet.GetRowHeight(36)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet.Pictures.AddPicture(6, 2, companyLogo);


                    }


                }
                catch (Exception)
                {


                }
                #endregion

                sheet.Range[6, 2, 14, 4].BorderAround(ExcelLineStyle.Double);

                #region Headers
                int rws = ROW;
                sheet.Range[ROW, COL + 1].Text = "Production OrderId";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["ProductionOrderId"].ToString().Trim();
                sheet.Range[ROW, COL + 2, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Bulletin Buyer Style Ref No";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["BulletinBuyerStyleRefNo"].ToString().Trim();
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "SO Description";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["Description"].ToString().Trim();
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Buyer Name";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["Buyer"].ToString().Trim();
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Buyer Style Ref No";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["BuyerOrder"].ToString().Trim();
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Own Style Ref No";
                int rowBulletinTarget = ROW;
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Text = " " + data.Rows[0]["OwnOrder"].ToString().Trim();

                sheet.Range[ROW, COL, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();
                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Product Master";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 3].Text = " " + data.Rows[0]["ProductMaster"].ToString().Trim();
                sheet.Range[ROW, COL, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();

                double plannedHourPerDay = Convert.ToDouble(data.Rows[0]["PlannedHoursPerDay"]);
                double TotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", null));
                double TotalManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", null));
                double TotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", null));
                double TotalRMP = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", null));
                double MaxAllotedTime = clsStaticInfo.dbl(data.Compute("Max(AvgAllotedTime)", null));

                double PitchTime = 0;
                if (TotalManpower != 0)
                    PitchTime = TotalSPT / TotalManpower;

                double OrgEfficiency = 0;
                if (MaxAllotedTime != 0)
                    OrgEfficiency = PitchTime / MaxAllotedTime;

                double ProdEffPerHour = 0;
                if (TotalSPT != 0)
                    ProdEffPerHour = TotalManpower * 60 / TotalSPT;

                double ProdEffPerDay = ProdEffPerHour * plannedHourPerDay;
                double LineTargetPerHour = ProdEffPerHour * OrgEfficiency;


                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Pitch Time";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Number = PitchTime;
                sheet.Range[ROW, COL + 2].NumberFormat = clsStaticInfo.NumberFormat(4);
                sheet.Range[ROW, COL, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL, ROW, COL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();

                ROW++;
                sheet.Range[ROW, COL + 1].Text = "Planned Hour PerDay";
                sheet.Range[ROW, COL, ROW, COL + 1].Merge();
                sheet.Range[ROW, COL + 2].Number = plannedHourPerDay;
                sheet.Range[ROW, COL + 2].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[ROW, COL, ROW, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, COL, ROW, COL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, COL + 2, ROW, COL + 3].Merge();

                sheet.Range[6, 5, 14, 8].BorderAround(ExcelLineStyle.Thick);


                int rwe = 6;
                int PCOL = 8;
                sheet.Range[rws, PCOL].Text = "Particulars";
                sheet.Range[rws, PCOL + 1].Text = "SPT(Minutes)";
                sheet.Range[rws, PCOL + 2].Text = "MP";
                sheet.Range[rws, PCOL + 3].Text = "Work Station";
                sheet.Range[rws, PCOL + 4].Text = "Bullitin Target(Pcs)";
                int colBulletinTarget = PCOL + 4;
                sheet.Range[rws, PCOL + 5].Text = "Planned Efficency(%)";
                sheet.Range[rws, PCOL + 6].Text = "Planned Per Man productivity(Pcs)";

                sheet.Range[6, 12, 14, 14].BorderAround(ExcelLineStyle.Thick);
                sheet.Range[rws, PCOL, rws, PCOL + 6].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL + 6].VerticalAlignment = ExcelVAlign.VAlignCenter;


                sheet.Range[rws, PCOL + 7].Text = "Target(%)";
                sheet.Range[rws, PCOL + 8].Text = "Per Hr";
                sheet.Range[rws, PCOL + 9].Text = "Per Day";

                sheet.Range[rws, PCOL + 7, rws, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 7, rws, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[6, 15, 14, 17].BorderAround(ExcelLineStyle.Thick);

                sheet.Range[rws, PCOL + 10].Text = "Created By";
                sheet.Range[rws, PCOL + 10, rws, PCOL + 10].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 11].Text = "Creation Date";
                sheet.Range[rws, PCOL + 11, rws, PCOL + 11].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 12].Text = "Revision";
                sheet.Range[rws, PCOL + 12, rws, PCOL + 12].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[rws, PCOL + 13].Text = "Approved By";
                sheet.Range[rws, PCOL + 13, rws, PCOL + 13].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[6, 18, 14, 21].BorderAround(ExcelLineStyle.Thick);

                rws++;
                sheet.Range[rws, PCOL].Text = "Non MC";
                sheet.Range[rws, PCOL, rws, PCOL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                double NMCTotalSPT = 0;
                double NMCTotalWS = 0;
                double NMCTotalMP = 0;

                NMCTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='No'"));
                NMCTotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='No'"));
                NMCTotalMP = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='No'"));

                sheet.Range[rws, PCOL + 1].Number = NMCTotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 2].Number = NMCTotalMP;
                sheet.Range[rws, PCOL + 2].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 3].Number = NMCTotalWS;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                rws++;
                sheet.Range[rws, PCOL].Text = "MC";
                sheet.Range[rws, PCOL, rws, PCOL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                double MCTotalSPT = 0;
                double MCTotalWS = 0;
                double MCTotalMP = 0;

                MCTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='Yes'"));
                MCTotalWS = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='Yes'"));
                MCTotalMP = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='Yes'"));

                sheet.Range[rws, PCOL + 1].Number = MCTotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 2].Number = MCTotalMP;
                sheet.Range[rws, PCOL + 2].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 3].Number = MCTotalWS;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                rws++;
                sheet.Range[rws, PCOL].Text = "Total";
                sheet.Range[rws, PCOL, rws, PCOL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL, rws, PCOL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 1].Number = TotalSPT;
                sheet.Range[rws, PCOL + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 1, rws, PCOL + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 2].Number = TotalManpower; string ToTalManpowerCellAddr = clsStaticInfo.GetxlsCol(PCOL + 2) + (rws.ToString());
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 2, rws, PCOL + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, PCOL + 3].Number = TotalWS;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rws, PCOL + 3, rws, PCOL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[rws, 8, rws, 11].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[rws, 8, rws, 11].CellStyle.Font.Bold = true;
                sheet.Range[rws, 8, rws, 11].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[6, 8, 14, 11].BorderAround(ExcelLineStyle.Thick);

                //sheet.Range[7, PCOL + 4].Text = "100";
                sheet.Range[7, PCOL + 7].Number = Convert.ToInt32("100");
                sheet.Range[7, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 7, 7, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[7, PCOL + 7, 7, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;


                string CellTargetAt100PercentEfficiency = clsStaticInfo.GetxlsCol(PCOL + 8) + "7";
                sheet.Range[7, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour);
                sheet.Range[7, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 8, 7, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[7, PCOL + 8, 7, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[7, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay);
                sheet.Range[7, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[7, PCOL + 9, 7, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[7, PCOL + 9, 7, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;



                //sheet.Range[8, PCOL + 4].Text = "85";

                sheet.Range[8, PCOL + 7].Number = Convert.ToInt32("85");
                sheet.Range[8, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 7, 8, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[8, PCOL + 7, 8, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[8, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .85);
                sheet.Range[8, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 8, 8, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[8, PCOL + 8, 8, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[8, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .85);
                sheet.Range[8, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[8, PCOL + 9, 8, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[8, PCOL + 9, 8, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;


                //sheet.Range[9, PCOL + 4].Text = "75";

                sheet.Range[9, PCOL + 7].Number = Convert.ToInt32("75");
                sheet.Range[9, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 7, 9, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[9, PCOL + 7, 9, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[9, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .75);
                sheet.Range[9, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 8, 9, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[9, PCOL + 8, 9, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[9, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .75);
                sheet.Range[9, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[9, PCOL + 9, 9, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[9, PCOL + 9, 9, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;

                //sheet.Range[10, PCOL + 4].Text = "65";

                sheet.Range[10, PCOL + 7].Number = Convert.ToInt32("65");
                sheet.Range[10, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 7, 10, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[10, PCOL + 7, 10, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[10, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .65);
                sheet.Range[10, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 8, 10, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[10, PCOL + 8, 10, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;


                sheet.Range[10, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .65);
                sheet.Range[10, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[10, PCOL + 9, 10, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[10, PCOL + 9, 10, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;


                //sheet.Range[11, PCOL + 4].Text = "55";
                sheet.Range[11, PCOL + 7].Number = Convert.ToInt32("55");
                sheet.Range[11, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 7, 11, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[11, PCOL + 7, 11, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[11, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .55);
                sheet.Range[11, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 8, 11, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[11, PCOL + 8, 11, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[11, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .55);
                sheet.Range[11, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[11, PCOL + 9, 11, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[11, PCOL + 9, 11, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;



                // sheet.Range[12, PCOL + 4].Text = "50";

                sheet.Range[12, PCOL + 7].Number = Convert.ToInt32("50");
                sheet.Range[12, PCOL + 7].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 7, 12, PCOL + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[12, PCOL + 7, 12, PCOL + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[12, PCOL + 8].Number = Convert.ToInt32(ProdEffPerHour * .50);
                sheet.Range[12, PCOL + 8].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 8, 12, PCOL + 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[12, PCOL + 8, 12, PCOL + 8].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet.Range[12, PCOL + 9].Number = Convert.ToInt32(ProdEffPerDay * .50);
                sheet.Range[12, PCOL + 9].NumberFormat = clsStaticInfo.NumberFormat(0);
                sheet.Range[12, PCOL + 9, 12, PCOL + 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[12, PCOL + 9, 12, PCOL + 9].VerticalAlignment = ExcelVAlign.VAlignCenter;

                // Created By	Creation Date Revision

                sheet.Range[7, PCOL + 10].Text = data.Rows[0]["AddedBy"].ToString();
                sheet.Range[7, PCOL + 11].Text = data.Rows[0]["AddedDate"].ToString();
                sheet.Range[7, PCOL + 12].Text = "1";


                //sheet.Range[rws, COL, rwe, COL].BorderInside(ExcelLineStyle.Thin);
                //sheet.Range[rws, COL, rwe, COL].BorderAround(ExcelLineStyle.Thin);

                sheet.Range[6, 5, 6, 21].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[6, 5, 6, 21].CellStyle.Font.Bold = true;
                sheet.Range[6, 5, 6, 21].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[6, 5, 6, 21].CellStyle.Font.Size = 9f;
                sheet.Range[6, 5, 6, 21].RowHeight = 30;
                sheet.Range[6, 5, 6, 21].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[6, 5, 6, 21].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[6, 5, 6, 21].BorderInside(ExcelLineStyle.Hair);


                ROW++;
                ROW++;
                ROW++;
                COL = 2;
                int sCol = COL;

                #region SetHeaderText


                report.SetHeaderText(ref sheet, ROW, COL, "Sr.No.", 8, ExcelHAlign.HAlignCenter);
                int ColSequence = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Code", 10, ExcelHAlign.HAlignCenter);
                int ColOperationCode = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Description", 28, ExcelHAlign.HAlignCenter);
                int ColMachineVarient = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Machine", 52, ExcelHAlign.HAlignCenter);
                int ColMachineCode = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "FG Zone", 15, ExcelHAlign.HAlignCenter);
                int ColFGZone = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Category", 15, ExcelHAlign.HAlignCenter);
                int ColOperationCategory = COL;
                COL++;



                report.SetHeaderText(ref sheet, ROW, COL, "SPT(Minutes)", 11, ExcelHAlign.HAlignCenter);
                int ColTotalSPT = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Target/Hr(Pcs)", 15, ExcelHAlign.HAlignCenter);
                int ColOperationTargetPerHr = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Required Man Power", 11, ExcelHAlign.HAlignCenter);
                int ColRequiredManPower = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Alloted Manpower", 11, ExcelHAlign.HAlignCenter);
                int ColAllotedManpower = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Alloted Workstation", 13, ExcelHAlign.HAlignCenter);
                int ColAllotedWorkstation = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Operation Group", 18, ExcelHAlign.HAlignCenter);
                int ColOperationGroup = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "Avg.Alloted Time", 15, ExcelHAlign.HAlignCenter);
                int ColAvgAllotedTime = COL;


                #endregion

                ROW++;
                endCol = COL;
                #endregion Headers

                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].RowHeight = 30;
                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW - 1, sCol, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);

                string ProcessName = "";
                var startRow = 0;
                var endRow = 0;
                int RowIndex = ROW;
                startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {

                    sheet[ROW, ColSequence].Number = clsStaticInfo.dbl(data.Rows[i]["Sequence"].ToString());
                    sheet[ROW, ColOperationCode].Text = data.Rows[i]["OperationCode"].ToString();
                    sheet[ROW, ColMachineVarient].Text = data.Rows[i]["OperationVariation"].ToString();
                    sheet[ROW, ColMachineCode].Text = data.Rows[i]["ShortName"].ToString();
                    sheet.Range[ROW, ColTotalSPT].Number = Convert.ToDouble(data.Rows[i]["TotalSPT"].ToString());
                    sheet.Range[ROW, ColTotalSPT].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[ROW, ColTotalSPT, ROW, ColTotalSPT].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColTotalSPT, ROW, ColTotalSPT].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColOperationTargetPerHr].Number = clsStaticInfo.dbl(data.Rows[i]["OperationTargetPerHr"].ToString());
                    sheet.Range[ROW, ColOperationTargetPerHr, ROW, ColOperationTargetPerHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationTargetPerHr, ROW, ColOperationTargetPerHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColRequiredManPower].Number = clsStaticInfo.dbl(data.Rows[i]["RequiredManPower"].ToString());
                    sheet.Range[ROW, ColRequiredManPower, ROW, ColRequiredManPower].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColRequiredManPower, ROW, ColRequiredManPower].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColAllotedManpower].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedManpower"].ToString());
                    sheet.Range[ROW, ColAllotedManpower, ROW, ColAllotedManpower].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAllotedManpower, ROW, ColAllotedManpower].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColAllotedWorkstation].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedWorkstation"].ToString());
                    sheet.Range[ROW, ColAllotedWorkstation, ROW, ColAllotedWorkstation].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAllotedWorkstation, ROW, ColAllotedWorkstation].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    sheet[ROW, ColOperationGroup].Text = data.Rows[i]["OperationGroup"].ToString();
                    sheet.Range[ROW, ColAvgAllotedTime].Number = Convert.ToDouble(data.Rows[i]["AvgAllotedTime"].ToString());
                    sheet.Range[ROW, ColAvgAllotedTime].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[ROW, ColAvgAllotedTime, ROW, ColAvgAllotedTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColAvgAllotedTime, ROW, ColAvgAllotedTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet[ROW, ColFGZone].Text = data.Rows[i]["FGZone"].ToString();

                    sheet[ROW, ColOperationCategory].Text = data.Rows[i]["OperationCategory"].ToString();
                    sheet.Range[ROW, ColOperationCategory, ROW, ColOperationCategory].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, ColOperationCategory, ROW, ColOperationCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    ProcessName = data.Rows[i]["Process"].ToString();

                    ROW++;
                }
                sheet.Range[startRow, ColSequence, ROW, ColAvgAllotedTime].BorderAround(ExcelLineStyle.Thick);
                endRow = ROW++;
                #region UH


                sheet.Range[endRow, 2].Text = ProcessName + " SPT";
                sheet.Range[endRow, 2, endRow, 3].Merge();
                sheet.Range[endRow, 3].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 8].Number = TotalSPT;
                sheet.Range[endRow, 8].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[endRow, 8].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 8, endRow, 8].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 8, endRow, 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[endRow, 9].Text = "TOTAL MP";
                sheet.Range[endRow, 9].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 11].Number = TotalManpower;
                sheet.Range[endRow, 11, endRow, 11].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 11, endRow, 11].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[endRow, 11].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 12].Number = TotalWS;
                sheet.Range[endRow, 12, endRow, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[endRow, 12, endRow, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[endRow, 12].CellStyle.Font.Bold = true;

                endRow++;
                endRow++;
                int edRow = endRow++;
                int edCRow = edRow;

                sheet.Range[endRow, 4].Text = "MACHINE & MANPOWER REQUIREMENT SUMMARY";
                sheet.Range[endRow, 4].CellStyle.Font.Bold = true;
                sheet.Range[endRow, 4, endRow, 5].Merge();

                int col = 4; edRow++; edRow++;
                sheet.Range[edRow, col].Text = "Machine";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                col++;
                sheet.Range[edRow, col].Text = "Machine Variation";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "SPT(Min)";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Req MP";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Allotted MP";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                col++;
                sheet.Range[edRow, col].Text = "Allotted WS";
                sheet.Range[edRow, col].CellStyle.Font.Bold = true;
                sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 4, edRow, 9].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[edRow, 4, edRow, 9].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 4, edRow, 9].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[edRow, 4, edRow, 9].CellStyle.Font.Size = 9f;
                sheet.Range[edRow, 4, edRow, 9].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[edRow, 4, edRow, 9].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[edRow, 4, edRow, 9].BorderAround(ExcelLineStyle.Thick);
                #endregion

                //DataTable dtM = new DataView(data).ToTable(true, "ShortName", "MachineVarientId", "MachineVarient", "AllotedWorkstation", "AllotedManpower", "RequiredManPower", "TotalSPT");
                DataTable dtM = new DataView(data).ToTable(true, "MachineMaster", "MachineVarientId", "ShortName");
                DataView dvM = new DataView(dtM);
                dvM.RowFilter = "MachineVarientId='" + data.Rows[0]["MachineVarientId"].ToString() + "'";
                edRow++;
                int msr = edRow;
                int sc = 4;
                int ec = 0;
                for (int i = 0; i < dtM.Rows.Count; i++)
                {

                    col = 4;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["MachineMaster"].ToString(); col++;
                    sheet.Range[edRow, col].Text = dtM.Rows[i]["ShortName"].ToString(); col++;

                    if (!string.IsNullOrEmpty(dtM.Rows[i]["MachineVarientId"].ToString()))
                    {
                        double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MTotalSPT;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MRequiredManPower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MAllotedManpower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedWorkstation = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "MachineVarientId='" + dtM.Rows[i]["MachineVarientId"].ToString() + "'"));
                        sheet.Range[edRow, col].Number = MAllotedWorkstation;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    else
                    {
                        double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MTotalSPT;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MRequiredManPower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MAllotedManpower;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        col++;

                        double MAllotedWorkstation = clsStaticInfo.dbl(data.Compute("SUM(AllotedWorkstation)", "Machine='No'"));
                        sheet.Range[edRow, col].Number = MAllotedWorkstation;
                        sheet.Range[edRow, col].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet.Range[edRow, col, edRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[edRow, col, edRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    edRow++;
                }
                ec = col;
                int mer = edRow;


                sheet.Range[edRow, 4].Text = "TOTAL";
                sheet.Range[edRow, 4].CellStyle.Font.Bold = true;

                sheet.Range[edRow, 6].Number = TotalSPT;
                sheet.Range[edRow, 6].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[edRow, 6].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 6, edRow, 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 6, edRow, 6].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 7].Number = TotalRMP;
                sheet.Range[edRow, 7].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 7, edRow, 7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 7, edRow, 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 8].Number = TotalManpower;
                sheet.Range[edRow, 8].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 8, edRow, 8].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 8, edRow, 8].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edRow, 9].Number = TotalWS;
                sheet.Range[edRow, 9].CellStyle.Font.Bold = true;
                sheet.Range[edRow, 9, edRow, 9].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edRow, 9, edRow, 9].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                sheet.Range[8, 12].Number = clsStaticInfo.dbl(data.Rows[0]["RequiredStdTarget"].ToString());
                sheet.Range[8, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[8, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[9, 12].Number = clsStaticInfo.dbl(data.Rows[0]["TotalBT"].ToString());
                sheet.Range[9, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[9, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                double rst = Convert.ToDouble(data.Rows[0]["RequiredStdTarget"]);
                double tbt = Convert.ToDouble(data.Rows[0]["TotalBT"]);
                double peHr = (rst / ProdEffPerHour) * 100;
                double peday = (tbt / ProdEffPerDay) * 100;
                sheet.Range[8, 13].Number = clsStaticInfo.dbl(peHr);
                sheet.Range[8, 13].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[8, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[8, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[9, 13].Number = clsStaticInfo.dbl(peday);
                sheet.Range[9, 13].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[9, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[9, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[8, 14].Number = clsStaticInfo.dbl(rst / TotalManpower);
                sheet.Range[8, 14].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[8, 14].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[8, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[9, 14].Number = clsStaticInfo.dbl((rst / TotalManpower) * plannedHourPerDay);
                sheet.Range[9, 14].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[9, 14].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[9, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[msr, sc, mer, ec].BorderAround(ExcelLineStyle.Thick);


                edCRow++; edCRow++;
                int Ccol = 11;
                int Cmsr = edCRow;
                int Csc = 11;//edCRow++;
                sheet.Range[edCRow, Ccol].Text = "Operation Category";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "SAM";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "Req MP";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "Allotted MP";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                Ccol++;
                sheet.Range[edCRow, Ccol].Text = "Skill(%)";
                sheet.Range[edCRow, Ccol].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                DataTable dtOC = new DataView(data).ToTable(true, "OperationCategory");
                dtOC.DefaultView.Sort = "OperationCategory ASC";
                dtOC = dtOC.DefaultView.ToTable();
                edCRow++;
                double tpercent = 0;
                for (int i = 0; i < dtOC.Rows.Count; i++)
                {

                    Ccol = 11;
                    sheet.Range[edCRow, Ccol].Text = dtOC.Rows[i]["OperationCategory"].ToString();
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double MTotalSPT = clsStaticInfo.dbl(data.Compute("SUM(TotalSPT)", "OperationCategory='" + dtOC.Rows[i]["OperationCategory"].ToString() + "'"));
                    sheet.Range[edCRow, Ccol].Number = MTotalSPT;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double MRequiredManPower = clsStaticInfo.dbl(data.Compute("SUM(RequiredManPower)", "OperationCategory='" + dtOC.Rows[i]["OperationCategory"].ToString() + "'"));
                    sheet.Range[edCRow, Ccol].Number = MRequiredManPower;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double MAllotedManpower = clsStaticInfo.dbl(data.Compute("SUM(AllotedManpower)", "OperationCategory='" + dtOC.Rows[i]["OperationCategory"].ToString() + "'"));
                    sheet.Range[edCRow, Ccol].Number = MAllotedManpower;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    Ccol++;

                    double calPer = (MAllotedManpower / TotalManpower) * 100;
                    tpercent += calPer;

                    sheet.Range[edCRow, Ccol].Number = calPer;
                    sheet.Range[edCRow, Ccol].NumberFormat = clsStaticInfo.NumberFormat(2);
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[edCRow, Ccol, edCRow, Ccol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    edCRow++;
                }

                int Cec = Ccol;
                int Cmer = edCRow;

                sheet.Range[edCRow, 11].Text = "TOTAL";
                sheet.Range[edCRow, 11].CellStyle.Font.Bold = true;

                sheet.Range[edCRow, 12].Number = TotalSPT;
                sheet.Range[edCRow, 12].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 12].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 12, edCRow, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 12, edCRow, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edCRow, 13].Number = TotalRMP;
                sheet.Range[edCRow, 13].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 13, edCRow, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 13, edCRow, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edCRow, 14].Number = TotalManpower;
                sheet.Range[edCRow, 14].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 14, edCRow, 14].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 14, edCRow, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[edCRow, 15].Number = tpercent;
                sheet.Range[edCRow, 15].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 15, edCRow, 15].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[edCRow, 15, edCRow, 15].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Font.Bold = true;
                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[Cmsr, 11, Cmsr, 15].CellStyle.Font.Size = 9f;
                sheet.Range[Cmsr, 11, Cmsr, 15].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[Cmsr, 11, Cmsr, 15].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[Cmsr, Csc, Cmer, Cec].BorderAround(ExcelLineStyle.Thick);


                //sheet.UsedRange.NumberFormat = "#,##0.000";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                sheet.IsGridLinesVisible = false;


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet, 2, "Bulletin Template Summary - " + SheetName + "", identity.CompanyId, identity.CompanyName, "");
                sheet.Range[1, 2, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //report.CompanyHeader(ref sheet, endCol, "Bulletin Template - " + SheetName + "", companyId);
                report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        #endregion end Repotrs for ProductionBulletinTemplate 

        #region --2nd Bullatin Report --
        [HttpGet, Authorize]
        public ActionResult GetBulletinTamplate2ndIndexReport(ReportFormat reportFormat, string ProductionId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Confirm Order Bullitin Summary";
            var workbook = GetBulletinTamplate2ndIndexReportWorkSheet(ProductionId);
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

        private IWorkbook GetBulletinTamplate2ndIndexReportWorkSheet(string ProductionId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "ConfirmOrderBullitinSummary";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = clsb.GetBulletin2ndTemplateData(ProductionId);

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Material MasterId", 12, ExcelHAlign.HAlignLeft);
            int ColMaterialMasterId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ArticleId", 25, ExcelHAlign.HAlignLeft);
            int ColArticleId = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "SO1", 25, ExcelHAlign.HAlignLeft);
            int ColSO1 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO2", 25, ExcelHAlign.HAlignLeft);
            int ColSO2 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO3", 11, ExcelHAlign.HAlignLeft);
            int ColSO3 = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "SOC1", 25, ExcelHAlign.HAlignLeft);
            int ColSOC1 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SOCV1", 25, ExcelHAlign.HAlignLeft);
            int ColSOCV1 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SOC2", 25, ExcelHAlign.HAlignLeft);
            int ColSOC2 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SOCV2", 30, ExcelHAlign.HAlignLeft);
            int ColSOCV2 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SOC3", 15, ExcelHAlign.HAlignLeft);
            int ColSOC3 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SOCV3", 15, ExcelHAlign.HAlignLeft);
            int ColSOCV3 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Order Breakdown Qty", 15, ExcelHAlign.HAlignLeft);
            int ColOrderBreakdownQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "BPT", 15, ExcelHAlign.HAlignLeft);
            int ColBPT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "AddedDate", 15, ExcelHAlign.HAlignLeft);
            int ColAddedDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "AddedBy", 15, ExcelHAlign.HAlignLeft);
            int ColAddedBy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "RequiredStdTarget", 15, ExcelHAlign.HAlignLeft);
            int ColRequiredStdTarget = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "TotalSPT", 15, ExcelHAlign.HAlignLeft);
            int ColTotalSPT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "AllotedManpower", 15, ExcelHAlign.HAlignLeft);
            int ColAllotedManpower = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PlanEfficency", 15, ExcelHAlign.HAlignLeft);
            int ColPlanEfficency = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PerManProductivity", 15, ExcelHAlign.HAlignLeft);
            int ColPerManProductivity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Target", 15, ExcelHAlign.HAlignLeft);
            int ColTarget = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PlannedHoursPerDay", 15, ExcelHAlign.HAlignLeft);
            int ColPlannedHoursPerDay = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "MCTotalSPT", 15, ExcelHAlign.HAlignLeft);
            int ColMCTotalSPT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "NMCTotalSPT", 15, ExcelHAlign.HAlignLeft);
            int ColNMCTotalSPT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "MCTotalMP", 15, ExcelHAlign.HAlignLeft);
            int ColMCTotalMP = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "NMCTotalMP", 15, ExcelHAlign.HAlignLeft);
            int ColNMCTotalMP = COL;
            COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "SOCV3", 15, ExcelHAlign.HAlignLeft);
            //int ColSOCV3 = COL;
            //COL++;

            endCol = COL;
            #endregion Headers

            var startRow = 0;

            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColMaterialMasterId].Text = data.Rows[i]["MaterialMasterId"].ToString();
                sheet[ROW, ColArticleId].Text = data.Rows[i]["ArticleId"].ToString();
                sheet[ROW, ColSO1].Text = data.Rows[i]["SO1"].ToString();
                sheet[ROW, ColSO2].Text = data.Rows[i]["SO2"].ToString();
                sheet[ROW, ColSO3].Text = data.Rows[i]["SO3"].ToString();
                sheet[ROW, ColSOC1].Text = data.Rows[i]["SOC1"].ToString();
                sheet[ROW, ColSOCV1].Text = data.Rows[i]["SOCV1"].ToString();
                sheet[ROW, ColSOC2].Text = data.Rows[i]["SOC2"].ToString();
                sheet[ROW, ColSOCV2].Text = data.Rows[i]["SOCV2"].ToString();
                sheet[ROW, ColSOC3].Text = data.Rows[i]["SOC3"].ToString();
                sheet[ROW, ColSOCV3].Text = data.Rows[i]["SOCV3"].ToString();
                sheet[ROW, ColOrderBreakdownQty].Text = data.Rows[i]["OrderBreakdownQty"].ToString();
                sheet[ROW, ColBPT].Text = data.Rows[i]["BPT"].ToString();
                sheet[ROW, ColAddedDate].Text = data.Rows[i]["AddedDate"].ToString();
                sheet[ROW, ColAddedBy].Text = data.Rows[i]["AddedBy"].ToString();
                sheet[ROW, ColRequiredStdTarget].Text = data.Rows[i]["RequiredStdTarget"].ToString();
                sheet[ROW, ColTotalSPT].Text = data.Rows[i]["TotalSPT"].ToString();
                sheet[ROW, ColAllotedManpower].Text = data.Rows[i]["AllotedManpower"].ToString();
                sheet[ROW, ColPlanEfficency].Text = data.Rows[i]["PlanEfficency"].ToString();
                sheet[ROW, ColPerManProductivity].Text = data.Rows[i]["PerManProductivity"].ToString();
                sheet[ROW, ColTarget].Text = data.Rows[i]["Target"].ToString();
                sheet[ROW, ColPlannedHoursPerDay].Text = data.Rows[i]["PlannedHoursPerDay"].ToString();
                sheet[ROW, ColMCTotalSPT].Text = data.Rows[i]["MCTotalSPT"].ToString();
                sheet[ROW, ColNMCTotalSPT].Text = data.Rows[i]["NMCTotalSPT"].ToString();
                sheet[ROW, ColMCTotalMP].Text = data.Rows[i]["MCTotalMP"].ToString();
                sheet[ROW, ColNMCTotalMP].Text = data.Rows[i]["NMCTotalMP"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyHeader(ref sheet, endCol, "Confirm Order Bullitin Summary", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        
        #endregion

    }
    public class MultiCode
    {
        public string Sequenc { get; set; }
        public string OperationCode { get; set; }
    }
}