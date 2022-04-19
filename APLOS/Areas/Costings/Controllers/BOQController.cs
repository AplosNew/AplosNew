#region Using

using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Costings.Controllers
{
    public class BOQController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public BOQController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult Approval()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetEditList(string column, string value)
        {
            List<Dictionary<string, object>> data = new Library.OrderManagement.Costing.CostingBOQ().GetEditList(column, value);
            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetItemList(string CostingBOQMasterId)
        {
            List<Dictionary<string, object>> data = new Library.OrderManagement.Costing.CostingBOQ().GetAllCostingDirectMaterial(CostingBOQMasterId);
            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetItemListForQtyEdit(string CostingBOQMasterId)
        {
            List<Dictionary<string, object>> data = new Library.OrderManagement.Costing.CostingBOQ().GetAllCostingDirectMaterialForQuantityEdit(CostingBOQMasterId);
            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetAllBOQCosting(string CostingBOQMasterId)
        {
            List<Dictionary<string, object>> data = new Library.OrderManagement.Costing.CostingBOQ().GetAllBOQCosting(CostingBOQMasterId);
            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(string Id, List<Dictionary<string, object>> MaterialAttachmentData, List<Dictionary<string, object>> QuantityData)
        {
            try
            {
                new Library.OrderManagement.Costing.CostingBOQ().UpdateBOQGeneration(Id, MaterialAttachmentData, QuantityData);
                return Json(new { Error = false, Message = "BOM Updated Successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public ActionResult Update(List<Dictionary<string, object>> QuantityData)
        {
            try
            {
                new Library.OrderManagement.Costing.CostingBOQ().UpdateBOQ(QuantityData);
                return Json(new { Error = false, Message = "BOM Updated Successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public ActionResult Delete(string Id)
        {
            try
            {
                new Library.OrderManagement.Costing.CostingBOQ().Delete(Id);
                return Json(new { Error = false, Message = "Data deleted successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost, Authorize]
        public ActionResult XUploadAttachment(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\\", "");
                DataTable AdditionalData = CustomJsonResult.ToDataTable(UploadDefault_data);

                //var settings = new JsonSerializerSettings
                //{
                //    NullValueHandling = NullValueHandling.Ignore,
                //    MissingMemberHandling = MissingMemberHandling.Ignore
                //};
                //List<Dictionary<string, string>> AdditionalData.Rows[0]1 = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(UploadDefault_data, settings);

                //Dictionary<string, string> AdditionalData.Rows[0] = JsonConvert.DeserializeObject<Dictionary<string, string>>(UploadDefault_data, settings);


                AdditionalData.Rows[0]["Id"] = AdditionalData.Rows[0]["Id"].ToString().Replace("\"", "");
                if (string.IsNullOrEmpty(AdditionalData.Rows[0]["Id"].ToString()))
                    throw new Exception("Save the item first");



                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                foreach (var file in UploadDefault)
                {

                    string _Id = AdditionalData.Rows[0]["TableName"].ToString() + AdditionalData.Rows[0]["Id"].ToString();

                    var fileName = Path.GetFileName(_Id + new FileInfo(file.FileName).Extension);
                    var destinationPath = Path.Combine(ResourcesPathReader.CostingBoqPath(), _Id + new FileInfo(file.FileName).Extension);

                    if (System.IO.Directory.Exists(ResourcesPathReader.CostingBoqPath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.CostingBoqPath());
                        }
                        catch (Exception ex)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select* from " + AdditionalData.Rows[0]["TableName"] + " where Id='" + AdditionalData.Rows[0]["Id"].ToString() + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();




                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        #region Task data update
                        if (dsLocal.Tables[0].Rows[0]["FileName"].ToString() != "")
                        {
                            //try to delete the existing file
                            try
                            {
                                var _Path = Path.Combine(ResourcesPathReader.GetToDoPath(), dsLocal.Tables[0].Rows[0]["FileName"].ToString());
                                if (System.IO.File.Exists(_Path))
                                    System.IO.File.Delete(_Path);
                            }
                            catch (Exception)
                            {

                            }

                        }

                        DataRow dr = dsLocal.Tables[0].Rows[0];

                        dr.BeginEdit();

                        dr["FileName"] = fileName;
                        dr["FileOriginalName"] = file.FileName;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();


                        #endregion data update





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

        #region upload product picture
        [HttpPost, Authorize]
        public ActionResult UploadAttachment(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the production order first");

                foreach (var file in UploadDefault)
                {
                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var destinationPath = Path.Combine(ResourcesPathReader.CostingBoqPath(), fileName);

                    if (System.IO.Directory.Exists(ResourcesPathReader.CostingBoqPath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.CostingBoqPath());
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select* from dbo.BOQ where id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["FileName"] = fileName;
                        dsLocal.Tables[0].Rows[0]["FileOriginalName"] = file.FileName;
                        dsLocal.Tables[0].Rows[0]["Extension"] = Path.GetExtension(file.FileName);

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
        [Authorize]
        public ActionResult RemoveDefault(string[] fileNames)
        {
            foreach (var fullName in fileNames)
            {
                var fileName = Path.GetFileName(fullName);
                var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            return Content("");
        }

        #endregion upload product picture



        [HttpPost, Authorize]
        public ActionResult GetCostingBOQReport(string boqId)
        {
            try
            {
                string fileName = "";
                fileName = CostingBOQReport(boqId, "CostingBOQReport");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string CostingBOQReport(string boqId, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "CostingBOQReports";
                sheet = workbook.Worksheets[0];
                DataTable data;
                CostingBOQSQL(boqId, out data);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Sequence";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSequence = COL;
                COL++;

                sheet[ROW, COL].Text = "Item Ref No";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColItemRefNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Consumption";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColConsumption = COL;
                COL++;

                sheet[ROW, COL].Text = "BOM Qty";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBOMQty = COL;
                COL++;
                sheet[ROW, COL].Text = "BOQ UOM";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBOQUOM = COL;
                COL++;
                sheet[ROW, COL].Text = "BOM Qty Base";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBOMQtyBase = COL;
                COL++;
                sheet[ROW, COL].Text = "Required Qty";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRequiredQty = COL;
                COL++;
                sheet[ROW, COL].Text = "PO BOQ Qty";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPOBOQQty = COL;
                COL++;
                sheet[ROW, COL].Text = "PO UOM";
                sheet[ROW, COL].ColumnWidth = 22;
                int ColPOUOM = COL;
                COL++;
                sheet[ROW, COL].Text = "PO Trn BO QQty";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColPOTrnBOQQty = COL;
                COL++;
                sheet[ROW, COL].Text = "PO Amount";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColPOAmount = COL;
                COL++;
                sheet[ROW, COL].Text = "Balance BOQ";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBalanceBOQ = COL;
                COL++;
                sheet[ROW, COL].Text = "GRN Base Qty";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColGRNBaseQty = COL;
                COL++;

                sheet[ROW, COL].Text = "GRN Amount";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColGRNAmount = COL;
                COL++;
                sheet[ROW, COL].Text = "GRN UOM";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColGRNUOM = COL;
                COL++;
                sheet[ROW, COL].Text = "Balance PO Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColBalancePOQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Issue Base Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColIssueBaseQty = COL;
                COL++;
                sheet[ROW, COL].Text = "IssueAmount";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColIssueAmount = COL;
                COL++;
                sheet[ROW, COL].Text = "BalanceGRNQty";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColBalanceGRNQty = COL;


                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                //sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColSequence].Text = data.Rows[i]["Sequence"].ToString();
                    sheet[ROW, ColItemRefNo].Text = clsStaticInfo.GetDate(data.Rows[i]["ItemRefNo"].ToString());
                    sheet[ROW, ColConsumption].Text = data.Rows[i]["Consumption"].ToString();
                    sheet[ROW, ColBOMQty].Number = clsStaticInfo.dbl(data.Rows[i]["BOMQty"].ToString());
                    sheet[ROW, ColBOQUOM].Text = data.Rows[i]["BOQUOM"].ToString();
                    sheet[ROW, ColBOMQtyBase].Text = data.Rows[i]["BOMQtyBase"].ToString();
                    sheet[ROW, ColRequiredQty].Number = clsStaticInfo.dbl(data.Rows[i]["RequiredQty"].ToString());
                    sheet[ROW, ColPOBOQQty].Number = clsStaticInfo.dbl(data.Rows[i]["POBOQQty"].ToString());
                    sheet[ROW, ColPOUOM].Text = data.Rows[i]["POUOM"].ToString();
                    sheet[ROW, ColPOTrnBOQQty].Number = clsStaticInfo.dbl(data.Rows[i]["POTrnBOQQty"].ToString());
                    sheet[ROW, ColPOAmount].Number = clsStaticInfo.dbl(data.Rows[i]["POAmount"].ToString());
                    sheet[ROW, ColBalanceBOQ].Number = clsStaticInfo.dbl(data.Rows[i]["BalanceBOQ"].ToString());
                    sheet[ROW, ColGRNBaseQty].Number = clsStaticInfo.dbl(data.Rows[i]["GRNBaseQty"].ToString());
                    sheet[ROW, ColGRNAmount].Number = clsStaticInfo.dbl(data.Rows[i]["GRNAmount"].ToString());
                    sheet[ROW, ColGRNUOM].Text = data.Rows[i]["GRNUOM"].ToString();
                    sheet[ROW, ColBalancePOQty].Number = clsStaticInfo.dbl(data.Rows[i]["BalancePOQty"].ToString());
                    sheet[ROW, ColIssueBaseQty].Number = clsStaticInfo.dbl(data.Rows[i]["IssueBaseQty"].ToString());
                    sheet[ROW, ColIssueAmount].Number = clsStaticInfo.dbl(data.Rows[i]["IssueAmount"].ToString());
                    sheet[ROW, ColBalanceGRNQty].Number = clsStaticInfo.dbl(data.Rows[i]["BalanceGRNQty"].ToString());

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Costing BOQ Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;




                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
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


        public void CostingBOQSQL(string boqId, out DataTable data)
        {
            try
            {


                string strSQL = @"SELECT boq.[Sequence],boq.ItemRefNo,boq.Consumption,boq.BOMQty,UOM.UserName BOQUOM,boq.BOMQtyBase,boq.RequiredQty
										, poboq.POBOQQty,poboq.POUOM,poboq.POTrnBOQQty,poboq.POAmount,BalanceBOQ=boq.BOMQtyBase-poboq.POBOQQty 
										, grnboq.GRNBaseQty
										, grnboq.GRNAmount
										, grnboq.GRNUOM
										, BalancePOQty=poboq.POBOQQty-grnboq.GRNBaseQty
										, issueboq.IssueBaseQty
										, issueboq.IssueAmount
										, BalanceGRNQty=grnboq.GRNBaseQty-issueboq.IssueBaseQty
										FROM BOQ  boq
										LEFT JOIN SCS.UnitOfMeasurement UOM ON UOM.Id=boq.UoMId
										--left join dbo.CostingMasterTemplate cmt on cmt.Id=boq.CostingItemId
										left join(SELECT pomap.BOQDetailId,sum(pomap.POBOQQty) POBOQQty,sum(pomap.TransactionQty) POTrnBOQQty,UOM.UserName POUOM,SUM(pod.BaseAmount) POAmount 
													FROM  trn.POBOQMAP pomap 
													JOIN trn.PurchaseOrderDetail pod on pod.Id=pomap.PODetailId
													LEFT JOIN SCS.UnitOfMeasurement UOM ON UOM.Id=pod.TransactionUoMId
													GROUP BY pomap.BOQDetailId,UOM.UserName
													) poboq ON poboq.BOQDetailId=boq.Id

										left join (SELECT gpa.BOQDetailId,sum(gpa.TransactionQty) GRNBaseQty,UOM.UserName GRNUOM,sum(IRD.TotalMaterialTranAmount ) GRNAmount
														FROM trn.GRNPORequisitionAllocation gpa 
														JOIN trn.InventoryReceiveDetail IRD ON gpa.InventoryReceiveDetailId=IRD.Id
														LEFT JOIN SCS.UnitOfMeasurement UOM ON UOM.Id=IRD.TransactionUoMId
														GROUP BY gpa.BOQDetailId,UOM.UserName
													) grnboq ON grnboq.BOQDetailId=poboq.BOQDetailId

										left join (SELECT iihb.BOQDetailId,sum(iihb.Qty) IssueBaseQty ,sum(iihb.Qty*iih.Rate) IssueAmount
													FROM trn.InventoryIssueHistoryBOQ iihb 
													join TRN.InventoryIssueHistory iih on iihb.InventoryIssueHistoryId=iih.Id
													GROUP BY iihb.BOQDetailId

										) issueboq ON issueboq.BOQDetailId=poboq.BOQDetailId

										WHERE boq.Id= '" + boqId + @"'";

                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }


        
    }
}
