using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using Library.Service.Systems;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.UI.WebControls;

namespace Library.OrderManagement.FabricRollClass
{
    public class FabricRollClass
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        private object clsGRNReports;
        public FabricRollClass()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        enum colIndex
        {
            Type = 1,
            USER,
            RerpotName,
            InventoryReceiveId,
            SystemID,

            RollControlNo = 6,
            VendorRollNo,
            VendorLotNo,
            Quantity,
            PackingListQuantity,
            VendorPackingFormNo,
            StorageLocationName,
            BinSystemID,
            Remarks,
            GRNMasterSystemID
        }
        enum dataType
        {
            DATA,
            MAINHEADER
        }

        private static readonly string reportScreenName = "GRN";
        public void DownloadReport(string inventoryReceiveDetailId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            // initialize the Class of Query 
            System.Data.DataSet dsCompany = null;
            System.Data.DataSet dsHeader = null;
            System.Data.DataSet dsItems = null;
            System.Data.DataSet dsStorageLocation = null;
            System.Data.DataSet dsBinNo = null;

            //clsPurchaseOrder objPur = null;
            FabricRollClass objPur = null;
            clsStaticInfo objStatic = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;


            string address = "";
            try
            {
                objStatic = new clsStaticInfo();
                objPur = new FabricRollClass();
                objPur.GetFabricRollHeaderInfo_Report(inventoryReceiveDetailId, out dsHeader);
                if (dsHeader.Tables[0].Rows.Count == 0)
                {
                    Exception ex = new Exception("Please select GRN");
                    throw (ex);
                }

                //objPur.GetGRNDownload(inventoryReceiveDetailId, GRNMaterialSystemID, "ROLL", out dsItems);
                objPur.GetRollDownload(inventoryReceiveDetailId, out dsItems);
                if (dsItems.Tables[0].Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data Found");
                    throw (ex);
                }

                // objPur.getPlantForReportTitle(dsHeader.Tables[0].Rows[0]["PlantID"].ToString(), out dsCompany);
                objPur.getPlantForReportTitle(identity.PlantId, out dsCompany);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(3);
                sheet = workbook.Worksheets[0];


                double RowHeight = 0;
                int ROW = 7;
                int leftCOL = (int)colIndex.RollControlNo; int leftCOLData = leftCOL + 1;
                int rightCOL = leftCOLData + 3; int rightCOLData = rightCOL + 2;
                int lastCOL = rightCOLData + 2;
                int startRow = ROW;
                int COL = leftCOL;

                ROW = 5;

                #region Left Data

                sheet[ROW, leftCOL].Text = "GRN No";
                sheet[ROW, leftCOL].CellStyle.Font.Bold = true;

                //left data
                sheet[ROW, leftCOLData].Text = dsHeader.Tables[0].Rows[0]["GRNNo"].ToString();
                sheet.Range[ROW, leftCOLData, ROW, rightCOL - 1].Merge();
                ROW++;

                sheet[ROW, leftCOL].Text = "GRN Date";
                sheet[ROW, leftCOL].CellStyle.Font.Bold = true;

                //left data
                sheet[ROW, leftCOLData].Text = dsHeader.Tables[0].Rows[0]["GRNDate"].ToString();
                sheet.Range[ROW, leftCOLData, ROW, rightCOL - 1].Merge();
                ROW++;

                sheet[ROW, leftCOL].Text = "Supplier Code";
                sheet[ROW, leftCOL].CellStyle.Font.Bold = true;
                //left data
                sheet[ROW, leftCOLData].Text = dsHeader.Tables[0].Rows[0]["PartyCode"].ToString();
                sheet.Range[ROW, leftCOLData, ROW, rightCOL - 1].Merge();
                ROW++;

                sheet[ROW, leftCOL].Text = "Supplier";
                sheet[ROW, leftCOL].CellStyle.Font.Bold = true;

                //left data
                sheet[ROW, leftCOLData].Text = dsHeader.Tables[0].Rows[0]["PartyName"].ToString();
                sheet.Range[ROW, leftCOLData, ROW, rightCOL - 1].Merge();

                ROW++;

                sheet[ROW, leftCOL].Text = dsHeader.Tables[0].Rows[0]["SKU1"].ToString();
                sheet[ROW, leftCOL].CellStyle.Font.Bold = true;

                //left data
                sheet[ROW, leftCOLData].Text = dsHeader.Tables[0].Rows[0]["SKUValue"].ToString();
                sheet.Range[ROW, leftCOLData, ROW, rightCOL - 1].Merge();
                ROW++;

                sheet[ROW, leftCOL].Text = "Material";
                sheet[ROW, leftCOL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, leftCOLData, ROW, rightCOL - 1].Merge();
                //left data
                sheet[ROW, leftCOLData].Text = dsHeader.Tables[0].Rows[0]["MaterialMasterName"].ToString();
                sheet.Range[ROW, leftCOLData, ROW, lastCOL - 1].Merge();
                sheet[ROW, leftCOLData].RowHeight = sheet[ROW, leftCOLData].RowHeight * 3;
                #endregion Left Data

                ROW = 5;
                #region right Data

                sheet[ROW, rightCOL].Text = "File No";
                sheet[ROW, rightCOL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, rightCOL, ROW, rightCOLData - 1].Merge();
                //right data
                sheet[ROW, rightCOLData].Text = dsHeader.Tables[0].Rows[0]["MasterOrderId"].ToString();
                sheet.Range[ROW, rightCOLData, ROW, lastCOL - 1].Merge();

                ROW++;

                sheet[ROW, rightCOL].Text = "Purchase Order No";
                sheet[ROW, rightCOL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, rightCOL, ROW, rightCOLData - 1].Merge();
                //right data
                sheet[ROW, rightCOLData].Text = dsHeader.Tables[0].Rows[0]["POId"].ToString();
                sheet.Range[ROW, rightCOLData, ROW, lastCOL - 1].Merge();

                ROW++;
                sheet[ROW, rightCOL].Text = "PO Vendor Ref/PI";
                sheet[ROW, rightCOL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, rightCOL, ROW, rightCOLData - 1].Merge();
                //right data
                //sheet[ROW, rightCOLData].Text = dsHeader.Tables[0].Rows[0]["PurchaseRef"].ToString();
                sheet.Range[ROW, rightCOLData, ROW, lastCOL - 1].Merge();

                ROW++;

                sheet[ROW, rightCOL].Text = "Invoice No";
                sheet[ROW, rightCOL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, rightCOL, ROW, rightCOLData - 1].Merge();
                //right data
                sheet[ROW, rightCOLData].Text = dsHeader.Tables[0].Rows[0]["InvoiceNo"].ToString();
                sheet.Range[ROW, rightCOLData, ROW, lastCOL - 1].Merge();

                ROW++;


                sheet[ROW, rightCOL].Text = "Invoice Date";
                sheet[ROW, rightCOL].CellStyle.Font.Bold = true;
                sheet.Range[ROW, rightCOL, ROW, rightCOLData - 1].Merge();
                //right data
                if (dsHeader.Tables[0].Rows[0]["InvoiceDate"].ToString() != "")
                    sheet[ROW, rightCOLData].Text = bplib.clsWebLib.makeBaseBlank(Convert.ToDateTime(dsHeader.Tables[0].Rows[0]["InvoiceDate"].ToString()));
                sheet.Range[ROW, rightCOLData, ROW, lastCOL - 1].Merge();

                ROW++;
                #endregion right Data


                ROW = 12;

                //sheet[ROW, (int)colIndex.Type].Text = dataType.MAINHEADER.ToString();
                //sheet[ROW, (int)colIndex.USER].Text = identity.UserId;
                //sheet[ROW, (int)colIndex.RerpotName].Text = reportScreenName;
                //sheet[ROW, (int)colIndex.MaterialDesc].Text = dsItems.Tables[0].Rows[0]["MaterialDesc"].ToString();
                //sheet[ROW, (int)colIndex.BOMandSOWiseRMSystemID].Text = dsItems.Tables[0].Rows[0]["BOMandSOWiseRMSystemID"].ToString();
                //sheet[ROW, (int)colIndex.GRNMasterSystemID].Text = dsHeader.Tables[0].Rows[0]["GRNNO"].ToString();
                //sheet[ROW, 1].RowHeight = 0;
                //ROW++;

                sheet[ROW, (int)colIndex.Type].Text = dataType.MAINHEADER.ToString();
                sheet[ROW, (int)colIndex.USER].Text = identity.UserId;
                sheet[ROW, (int)colIndex.RerpotName].Text = reportScreenName;
                sheet[ROW, (int)colIndex.InventoryReceiveId].Text = dsHeader.Tables[0].Rows[0]["GRNNO"].ToString();
                sheet[ROW, 1].RowHeight = 0;
                ROW++;
                int endCol = 8;

                sheet[ROW, COL].Text = "Roll No.";
                sheet[ROW, COL].ColumnWidth = 18;
                int colRollControlNo = (int)colIndex.RollControlNo;
                COL++;

                sheet[ROW, COL].Text = "Supplier Roll No.";
                sheet[ROW, COL].ColumnWidth = 18;
                int colSupplierRollNo = (int)colIndex.VendorRollNo;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                COL++;

                sheet[ROW, COL].Text = "Lot No";
                sheet[ROW, COL].ColumnWidth = 12;
                int colLotNo = (int)colIndex.VendorLotNo;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                COL++;

                sheet[ROW, COL].Text = "Roll Wise Packing List Qty";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colCurrentReceivedQty = (int)colIndex.Quantity;

                endCol = colCurrentReceivedQty;

                sheet.Range[ROW, leftCOL, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(150, 250, 150);
                sheet.Range[ROW, leftCOL, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, leftCOL, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, leftCOL, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, leftCOL, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                RowHeight += sheet.Range[ROW, 1].RowHeight;

                int colAutoHeight = endCol + 3;
                startRow = ROW;

                for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                {
                    sheet[ROW, (int)colIndex.Type].Text = dataType.DATA.ToString();
                    //sheet[ROW, (int)colIndex.BOMandSOWiseRMSystemID].Text = dsItems.Tables[0].Rows[i]["BOMandSOWiseRMSystemID"].ToString();
                    //sheet[ROW, (int)colIndex.GRNMaterialSystemID].Text = dsItems.Tables[0].Rows[i]["GRNMaterialSystemID"].ToString();
                    //sheet[ROW, (int)colIndex.GRNSKUSystemID].Text = dsItems.Tables[0].Rows[i]["GRNSKUSystemID"].ToString();
                    //sheet[ROW, (int)colIndex.SystemID].Text = dsItems.Tables[0].Rows[i]["SystemID"].ToString();
                    //sheet[ROW, (int)colIndex.UOMSystemID].Text = dsItems.Tables[0].Rows[i]["UOMSystemID"].ToString();
                    //sheet[ROW, (int)colIndex.UOMSystemIDBase].Text = dsItems.Tables[0].Rows[i]["UOMSystemIDBase"].ToString();

                    sheet[ROW, colRollControlNo].Text = dsItems.Tables[0].Rows[i]["RollNo"].ToString();
                    //sheet[ROW, colColor].Text = dsItems.Tables[0].Rows[i]["SKUValue"].ToString();
                    sheet[ROW, colSupplierRollNo].Text = dsItems.Tables[0].Rows[i]["VendorRollNo"].ToString();
                    sheet[ROW, colLotNo].Text = dsItems.Tables[0].Rows[i]["VendorLotNo"].ToString();
                    sheet[ROW, colCurrentReceivedQty].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[i]["VendorQty"].ToString()));

                    //if (dsItems.Tables[0].Rows[i]["BOMandSOwiseRMSystemID"].ToString().ToUpper() != dsItems.Tables[0].Rows[i]["BOMandSOwiseRMSystemIDtransferred"].ToString().ToUpper()
                    //     || dsItems.Tables[0].Rows[i]["MaterialMasterAttributeSystemID"].ToString().ToUpper() != dsItems.Tables[0].Rows[i]["MaterialMasterAttributeSystemIDtransferred"].ToString().ToUpper()
                    //     || dsItems.Tables[0].Rows[i]["isLocationTransferred"].ToString().ToUpper() == "YES")
                    //{
                    //    sheet[ROW, colRollControlNo].CellStyle.Font.Color = ExcelKnownColors.Red;
                    //}

                    sheet.Range[ROW, colSupplierRollNo, ROW, endCol].CellStyle.Locked = false;
                    sheet[ROW, colSupplierRollNo].CellStyle.Locked = false;
                    sheet.Range[ROW, colRollControlNo].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                    //sheet.Range[ROW, colRollControlNo, ROW, colCurrentReceivedQty].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;


                    sheet.Range[ROW, leftCOL, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, leftCOL, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, leftCOL, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                sheet.Range[1, leftCOL, ROW, endCol].WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange.NumberFormat = clsStaticInfo.NumberFormat(2);

                for (int i = 1; i < leftCOL; i++)
                {
                    sheet[ROW, i].ColumnWidth = 0;
                }



                for (int i = leftCOL; i <= endCol; i++)
                {
                    if (i != colCurrentReceivedQty)
                    {
                        sheet.Range[startRow, i, ROW, i].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet.Range[startRow, i, ROW, i].NumberFormat = clsStaticInfo.NumberFormat(2);
                    }
                }
                //sheet.Range[startRow, colStorageLocation, ROW, colStorageLocation].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[startRow, colCurrentReceivedQty, ROW, colCurrentReceivedQty].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[startRow, colCurrentReceivedQty, ROW - 1, colCurrentReceivedQty].DataValidation.IsEmptyCellAllowed = true;
                sheet.Range[startRow, colCurrentReceivedQty, ROW - 1, colCurrentReceivedQty].DataValidation.AllowType = ExcelDataType.Decimal;
                sheet.Range[startRow, colCurrentReceivedQty, ROW - 1, colCurrentReceivedQty].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                sheet.Range[startRow, colCurrentReceivedQty, ROW - 1, colCurrentReceivedQty].DataValidation.FirstFormula = "0";
                sheet.Range[startRow, colCurrentReceivedQty, ROW - 1, colCurrentReceivedQty].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                sheet.Range[startRow, colCurrentReceivedQty, ROW - 1, colCurrentReceivedQty].DataValidation.ErrorBoxText = "Only positive numbers are allowed";
                sheet.Range[startRow, colCurrentReceivedQty, ROW - 1, colCurrentReceivedQty].DataValidation.ErrorBoxTitle = "Number Error";

                //sheet.Range[startRow, colCurrentReceivedQty, ROW - 1, colCurrentReceivedQty].Numb


                sheet.Protect(bplib.clsWebLib.REPORT_LOCK_PASSWORD);
                workbook.Worksheets[1].Protect(bplib.clsWebLib.REPORT_LOCK_PASSWORD);
                workbook.Protect(false, true, bplib.clsWebLib.REPORT_LOCK_PASSWORD);
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;
                //workbook.Version = ExcelVersion.Excel97to2003;
                workbook.Version = ExcelVersion.Excel2016;

                string strFileName = "GRN " + (inventoryReceiveDetailId) + " " + System.DateTime.Today.ToString("dd-MMM-yyyy") + ".xls";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
                excelEngine.Dispose();


            }
            catch (Exception ex)
            {
                //displayMsgs(ex.Message, "ERROR", "Save");
                //ShowLog(ex.Message);

                throw ex;
            }
            finally
            {


                excelEngine = null;
                application = null;
                workbook = null;
                sheet = null;

            }

        }
        private string listFiles(DataGrid dg)
        {
            if (dg == null)
                return "''";

            string salesOrderMasterSystemID = "''";


            CheckBox chk;
            for (int i = 0; i < dg.Items.Count; i++)
            {
                chk = (CheckBox)dg.Items[i].FindControl("chkSelect");
                if (chk != null)
                {
                    if (chk.Checked == true)
                    {
                        salesOrderMasterSystemID += ",'" + clsStaticInfo.valueFromGrid(i, "SalesOrderMasterSystemID", ref dg) + "'";
                    }
                }
                else
                {

                    //for quality, no checkbox
                    salesOrderMasterSystemID += ",'" + clsStaticInfo.valueFromGrid(i, "SalesOrderMasterSystemID", ref dg) + "'";
                }

            }



            return salesOrderMasterSystemID;
        }
        private void getOpenFiles(string listSalesOrderMasterSystemID, string GRNMasterSystemID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT SOM.fileNos,pidf.physicalInventoryDocumentMasterSystemID FROM physicalInventoryDocumentFiles pidf 
INNER JOIN SalesOrderMaster som ON som.systemID=pidf.SalesOrderMasterSystemID
WHERE (pidf.SalesOrderMasterSystemID IN (" + listSalesOrderMasterSystemID + ") " + @" 
OR pidf.SalesOrderMasterSystemID IN (SELECT distinct g.SalesOrderMasterSystemID
                                       FROM GRNMaterial g WHERE g.GRNMasterSystemID='" + GRNMasterSystemID + "')) " + @" 

AND pidf.isDocumentOpen='YES'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function
        private void getInventoryClosedFiles(string listSalesOrderMasterSystemID, string GRNMasterSystemID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT distinct g.SalesOrderMasterSystemID
                                       FROM GRNMaterial g WHERE g.GRNMasterSystemID='" + GRNMasterSystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");


                string list = "''";
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    list += ",'" + dsRef.Tables[0].Rows[i]["SalesOrderMasterSystemID"].ToString() + "'";
                }


                strSql = @"SELECT * from  SalesOrderMaster som 
                                    WHERE (som.SystemID IN (" + listSalesOrderMasterSystemID + ") " + @" 
                                    OR som.SystemID IN (" + list + ") )" + @" 

                                    AND som.isInventoryClosed='YES'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function

        public void GetFabricRollHeaderInfo_Report(string InventoryReceiveDetailId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT 
DISTINCT IRD.Id,IRD.InventoryReceiveId,IRD.TransactionQty,IRD.TransactionUoMId,Isnull(FRM.SplitCount,0)SplitCount
,ISNULL(FRM.TotalDistributeQty,0)TotalDistributeQty,UOM.UserName UOM,BUoM.UserName BaseUoM,IR.Id GRNNo,FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate
,P.UserName PartyName,P.code PartyCode,PL.FabRollPrefix,IM.PlantId,IM.MaterialMasterId,IM.ArticleId
,IM.FirstCharacteristicsId SKUId,MM.UserName MaterialMasterName,MMA.StandardName ArticleName
,C.UserName SKU1,C2.UserName SKU2,C3.UserName SKU3,CV.UserName SKUValue, C.UserName +':'+CV.UserName SKUInfo,CU.Code
,MGM.UserName MaterialGroup,MOI.MasterOrderId,PO.Id POId,IR.InvoiceNo,FORMAT(IR.InvoiceDate,'dd-MMM-yyyy') InvoiceDate
FROM [TRN].[InventoryReceiveDetail] IRD
                                        LEFT JOIN TRN.InventoryReceive IR ON IRD.InventoryReceiveId=IR.Id
                                        LEFT JOIN HKP.Party P ON IR.PartyId=P.Id
                                        LEFT JOIN TRN.InventoryMaterial IM ON IRD.InventoryMaterialId=IM.Id
										left outer join trn.MasterOrderItem MOI on MOI.Id=IRD.MasterOrderItemId
										left outer join TRN.PurchaseOrder PO ON PO.id=IRD.POId
										left outer join TRN.PurchaseOrderDetail POD ON POD.Id=IRD.PODetailsId
                                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
										LEFT JOIN scs.PlantConfig PL ON  PL.PlantId=IM.PlantId
                                        LEFT JOIN SCS.UnitOfMeasurement UOM ON IRD.TransactionUoMId=UOM.Id
                                        LEFT JOIN SCS.UnitOfMeasurement BUoM ON IRD.BaseUOMId=BUoM.Id
                                        LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id

                                        LEFT JOIN MST.MaterialGroupMaster MGM ON MM.MaterialGroupMasterId=MGM.Id
                                        LEFT JOIN MST.MaterialMasterArticle MMA ON IM.ArticleId=MMA.Id

                                        LEFT JOIN HKP.Characteristics C ON IM.FirstCharacteristicsId=C.Id
                                        LEFT JOIN HKP.Characteristics C2 ON IM.SecondCharacteristicsId=C2.Id
                                        LEFT JOIN HKP.Characteristics C3 ON IM.ThirdCharacteristicsId=C3.Id

                                        LEFT JOIN [HKP].[CharacteristicsValue] CV ON IM.FirstCharacteristicsValueId=CV.Id
                                        LEFT JOIN MST.MaterialMasterBusinessProcess MMBP ON MM.Id=MMBP.MaterialMasterId
                                        LEFT JOIN SCS.BusinessProcess BP ON MMBP.BusinessProcessId=BP.Id
										LEFT JOIN (SELECT COUNT(Id) SplitCount,Sum(VendorQty) TotalDistributeQty
										,InventoryReceiveDetailId FROM TRN.FabricRollMaster 
										GROUP BY InventoryReceiveDetailId) FRM ON IRD.Id=FRM.InventoryReceiveDetailId
WHERE BP.BusinessProcessName='FabricRollManagement' AND ird.Id='" + InventoryReceiveDetailId + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function
        public void GetGRNHeaderInfo_Report(string SystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select 
--grn master data
GMAT.GMCount,pmat.POMCount,
g.SystemID AS GRNNO,REPLACE(CONVERT(CHAR(11), g.GRNDate, 106), ' ', '-') AS GRNDate,
g.SystemID AS GRNNO,REPLACE(CONVERT(CHAR(11), g.InvoiceDate, 106), ' ', '-') AS InvoiceDate,
g.GateEntryNo,REPLACE(CONVERT(CHAR(11), g.GateEntryDate, 106), ' ', '-') AS GateEntryDate,isnull(g.TotalGRNAmount,0) AS Amount,
g.GRNChallanNo,REPLACE(CONVERT(CHAR(11), g.DeliveryNoteDate, 106), ' ', '-') AS DeliveryNoteDate,
g.VehicleNo, g.DriverName, g.Remarks,g.CompanyID,g.InvoiceNumber,c2.CurrencyDesc as Currency,b.[Description] AS Buyer,

--po and other data
po.PoNo,po.SystemID AS POSystemID,po.yourRef AS PurchaseRef,REPLACE(CONVERT(CHAR(11),po.OrderDate, 106), ' ', '-') AS PODate,po.totalPOAmount,c2.CurrencyDesc,
c.Vendor, c.CODE AS VendorCode,PO.PlantID,PO.PurchasingOrganizationID,
cm.CommercialInvoiceNo,REPLACE(CONVERT(CHAR(11), cm.CommercialInvoiceDate, 106), ' ', '-') AS CommercialInvoiceDate,
som.fileNos


from GRNMaster g
left outer join PurchaseOrderMasterOnRMRequisition po on po.systemID=g.PurchaseOrderMasterOnRMRequisitionSystemID

LEFT OUTER JOIN (SELECT g.GRNMasterSystemID,COUNT(*) AS GMCount
                   FROM GRNMaterial g GROUP BY g.GRNMasterSystemID) AS GMAT ON GMAT.GRNMasterSystemID=g.SystemID

LEFT OUTER JOIN (SELECT g.PurchaseOrderMasterOnRMRequisitionSystemID,COUNT(*) AS POMCount
                   FROM PurchaseOrderOnRMRequisitionDetail g GROUP BY g.PurchaseOrderMasterOnRMRequisitionSystemID) AS PMAT ON PMAT.PurchaseOrderMasterOnRMRequisitionSystemID=PO.SystemID

LEFT OUTER JOIN (SELECT c.CompanyID AS CustomerID,c.CODE,c.CompanyName as Vendor,'COMPANY' AS CTYPE
				FROM company C
				UNION
				SELECT c.ContactID,c.CODE, c.ContactName,'CONTACT' AS CTYPE
				FROM Contacts C                                        
				) AS  c ON c.CustomerID=isnull(po.ContactIDAsVendor,'')+isnull(po.CompanyIDAsVendor,'')
left outer join SalesOrderMaster som on som.systemID=po.SalesOrderMasterSystemID
LEFT OUTER JOIN Buyer b ON b.BuyerID=som.buyerID
left outer join Currency c2 on c2.CurrencyCode=po.currencyCode
left outer join CIDetailMaster cm on cm.CommercialInvoiceMasterId=g.CommercialInvoiceMasterId
where g.SystemID='" + SystemID + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function
        public void GetGRNDownload(string GRNMasterSystemID, string GRNMaterialSystemID, string packingForm, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            string Filter = "";
            //if (SalesOrderMasterSystemID != "")
            //    Filter += " AND gl.SalesOrderMasterSystemID='" + SalesOrderMasterSystemID + "' ";

            //if (CommercialInvoiceMasterId != "")
            //    Filter += " AND g3.CommercialInvoiceMasterId='" + CommercialInvoiceMasterId + "' ";

            //if (MaterialMasterAttributeSystemID != "")
            //    Filter += " AND GL.MaterialMasterAttributeSystemID='" + MaterialMasterAttributeSystemID + "' ";

            //if (RMID != "")
            //    Filter += " AND GL.BOMandSOwiseRMSystemID='" + RMID + "' ";

            if (GRNMasterSystemID != "")
                Filter += " AND GL.GRNMasterSystemID='" + GRNMasterSystemID + "' ";

            if (GRNMaterialSystemID != "")
                Filter += " AND GL.GRNMaterialSystemID='" + GRNMaterialSystemID + "' ";

            if (packingForm.ToUpper() == "ROLL")
                packingForm = " AND gl.PackingForm in ('ROLL','BALE') ";

            try
            {
                strSQL = @"select 
---------------GRN HEADER LEVEL DATA-----------------
gl.GRNMasterSystemID,
REPLACE(CONVERT(CHAR(11), g3.GRNDate, 106), ' ', '-') AS GRNDate,GL.FLAGReceivedQty,po.PurchasingOrganizationID,PO.PlantID,SLA.PlantID AS SLPlantID,
g3.GateEntryNo,REPLACE(CONVERT(CHAR(11), g3.GateEntryDate, 106), ' ', '-') AS GateEntryDate,isnull(gl.isLocationTransferred,'') AS isLocationTransferred,
g3.GRNChallanNo,REPLACE(CONVERT(CHAR(11), g3.DeliveryNoteDate, 106), ' ', '-') AS DeliveryNoteDate,
g3.VehicleNo, g3.DriverName,g3.isQualityDone,gl.GSM,

--po and other data
po.PoNo,po.yourRef AS PurchaseRef,REPLACE(CONVERT(CHAR(11),po.OrderDate, 106), ' ', '-') AS PODate,po.totalPOAmount,
c.Vendor, c.CODE AS VendorCode,som.FileNos,g3.InvoiceNumber,g2.MaterialMasterAttributeSystemID,
cm.CommercialInvoiceNo,REPLACE(CONVERT(CHAR(11), cm.CommercialInvoiceDate, 106), ' ', '-') AS CommercialInvoiceDate,
-------------------Main Data---------------
gl.UOMSystemID,gl.UOMSystemIDBase,u.Name AS UOM,gl.PackingForm,
 gl.SystemID,gl.GRNMaterialSystemID,gl.GRNSKUSystemID, gl.PackingFormNo, gl.VendorPackingFormNo,gl.QualifiedQuantity AS ActualQuantity,gl.ReceivedQuantity,
       gl.VendorLotNo, gl.PackingListQuantity, BIN.Code AS BinNo, gl.Remarks,gl.BlanketLengthAfterWash,gl.BlanketWidthAfterWash,
gl.Width,gl.Inspected,isnull( gl.IsIssued,'NO') AS IsIssued, gl.BOMandSOwiseRMSystemIDtransferred,	gl.MaterialMasterAttributeSystemIDtransferred,
case when isnull(gl.BlanketLength,0)=0 then isnull(g.BlanketLength,0) else  isnull(gl.BlanketLength,0) end as BlanketLength,
case when isnull(gl.BlanketWidth,0)=0 then isnull(g.BlanketWidth,0) else  isnull(gl.BlanketWidth,0) end as BlanketWidth,
isnull(gl.IsLeftOverStock,'NO') AS IsLeftOverStock,	isnull(gl.isDisposed,'NO') AS isDisposed,gl.BlanketWeightBeforeWash,gl.BlanketWeightAfterWash,
--isnull(gl.BlanketLength,g.BlanketLength) AS BlanketLength,
--isnull(gl.BlanketWidth,g.BlanketWidth) AS BlanketWidth,
        sl.Code AS StorageLocationName,g2.POVendorSpec,
	gl.CW,	gl.A_Test,	gl.B_Test,	sdn.Code AS Shading,FFT.Code AS FabricFinishType,	gl.QCStatus,	gl.QCRemarks,
       gl.ShadeSystemID, s.Code AS QCShade, ss.Code AS QCSubShade,gl.ShrinkageGroupSystemID,sg.Code AS QCShrinkageGroup,
        gl.MerchShade AS MerchShadeSystemID, Ms.Code AS MerchShade,sMs.Code AS MerchSubShade,gl.LengthShrinkagePercentage,gl.WidthShrinkagePercentage,
----------------------START OF SKU AND CHARACTERISTICS-----------------------
pod.BOMandSOWiseRMSystemID,pod.MaterialDesc,pod.pidesc,somMat.FileNos AS MaterialFileNos,mm.MaterialCode,
ISNULL(isnull(mcvDIM1.Description,mcvdtmDIM1.Description),'')+ISNULL(isnull(mcvDIM2.Description,mcvdtmDIM2.Description),'')+ISNULL(isnull(mcvDIM3.Description,mcvdtmDIM3.Description),'') AS SKU,
--DIM1--
mcDIM1.Alias AS DIM1Characteristics,
isnull(mcvDIM1.[Description],mcvdtmDIM1.[Description]) AS DIM1CharValue,
mma.DIM1BuyerLevelSpecification,mma.DIM1VendorLevelSpecification,
--Y--
mcDIM2.Alias AS DIM2Characteristics,
isnull(mcvDIM2.[Description],mcvdtmDIM2.[Description]) AS DIM2CharValue,
mma.DIM2BuyerLevelSpecification,mma.DIM2VendorLevelSpecification,
--DIM3--
mcDIM3.Alias AS DIM3Characteristics,
isnull(mcvDIM3.[Description],mcvdtmDIM3.[Description]) AS DIM3CharValue,
mma.DIM3BuyerLevelSpecification,mma.DIM3VendorLevelSpecification
----------------------END OF SKU AND CHARACTERISTICS-----------------------
  from GRNPackingList gl
  left outer join GRNMaterial g on g.SystemID=gl.GRNMaterialSystemID
  left outer join GRNSKU g2 on g2.SystemID=gl.GRNSKUSystemID
  left outer join PurchaseOrderOnRMRequisitionDetail pod on pod.systemID=g.PurchaseOrderOnRMRequisitionDetailSystemID
    LEFT OUTER JOIN SalesOrderMaster somMat ON somMat.systemID=pod.SalesOrderMasterSystemID
    left outer join MaterialMaster mm on mm.systemid=pod.MaterialMasterSystemID
  left outer join StorageLocation sl on sl.StorageLocationID=isnull(gl.StorageLocationID,g.StorageLocationID)
  left outer join StorageLocationAndPlantAssignment SLA on sla.StorageLocationID=sl.StorageLocationID
  left outer join MaterialMasterAttribute mma on mma.SystemID=g2.MaterialMasterAttributeSystemID
  left outer join GRNMaster g3 on g3.SystemID=gl.GRNMasterSystemID
  left outer join PurchaseOrderMasterOnRMRequisition po on po.systemID=g3.PurchaseOrderMasterOnRMRequisitionSystemID
  left outer join Shade s on s.SystemID=gl.ShadeSystemID
  left outer join Shade Ms on Ms.SystemID=gl.MerchShade
  left outer join SubShade ss on ss.SystemID=gl.SubShadeSystemID
  left outer join SubShade sMs on sMs.SystemID=gl.MerchSubShade
  left outer join Shading sdn on sdn.SystemID=gl.ShadingSystemID
  left outer join FabricFinishType FFT on FFT.SystemID=gl.FabricFinishTypeSystemID
  left outer join ShrinkageGroup sg on sg.SystemID=gl.ShrinkageGroupSystemID
  left outer join uom u on u.systemid=gl.UOMSystemID
  left outer join BIN ON BIN.SystemID=gl.BINSystemID
  LEFT OUTER JOIN (SELECT c.CompanyID AS CustomerID,c.CODE,c.CompanyName as Vendor,'COMPANY' AS CTYPE
				FROM company C
				UNION
				SELECT c.ContactID,c.CODE, c.ContactName,'CONTACT' AS CTYPE
				FROM Contacts C                                        
				) AS  c ON c.CustomerID=isnull(po.ContactIDAsVendor,'')+isnull(po.CompanyIDAsVendor,'')
  left outer join SalesOrderMaster som on som.systemID=po.SalesOrderMasterSystemID
  left outer join CIDetailMaster cm on cm.CommercialInvoiceMasterId=g3.CommercialInvoiceMasterId
  ----------------------START OF SKU AND CHARACTERISTICS-----------------------
LEFT OUTER JOIN MaterialCharacteristics mcDIM1 ON mcDIM1.SystemID=mma.DIM1ComponentCharacteristicsSystemID
LEFT OUTER JOIN MaterialCharacteristicsValue mcvDIM1 ON mcvDIM1.SystemID=mma.DIM1ComponentCharacteristicsValueSystemID  
LEFT OUTER JOIN MaterialCharacteristicsValue mcvdtmDIM1 ON mcvdtmDIM1.SystemID=mma.DIM1DTMCharacteristicsValueSystemID

LEFT OUTER JOIN MaterialCharacteristics mcDIM2 ON mcDIM2.SystemID=mma.DIM2ComponentCharacteristicsSystemID
LEFT OUTER JOIN MaterialCharacteristicsValue mcvDIM2 ON mcvDIM2.SystemID=mma.DIM2ComponentCharacteristicsValueSystemID  
LEFT OUTER JOIN MaterialCharacteristicsValue mcvdtmDIM2 ON mcvdtmDIM2.SystemID=mma.DIM2DTMCharacteristicsValueSystemID


LEFT OUTER JOIN MaterialCharacteristics mcDIM3 ON mcDIM3.SystemID=mma.DIM3ComponentCharacteristicsSystemID
LEFT OUTER JOIN MaterialCharacteristicsValue mcvDIM3 ON mcvDIM3.SystemID=mma.DIM3ComponentCharacteristicsValueSystemID
LEFT OUTER JOIN MaterialCharacteristicsValue mcvdtmDIM3 ON mcvdtmDIM3.SystemID=mma.DIM3DTMCharacteristicsValueSystemID

  
where 1=1 " + packingForm + Filter + @"


ORDER BY pod.BOMandSOWiseRMSystemID,ISNULL(isnull(mcvDIM1.Code,mcvdtmDIM1.Code),'')+ISNULL(isnull(mcvDIM2.Code,mcvdtmDIM2.Code),'')+ISNULL(isnull(mcvDIM3.Code,mcvdtmDIM3.Code),'')
  , gl.PackingFormNo";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function

        public void GetRollDownload(string inventoryReceiveDetailId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            string Filter = "";


            try
            {
                strSQL = @"select * from TRN.FabricRollMaster where InventoryReceiveDetailId='" + inventoryReceiveDetailId + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function

        public void getPlantForReportTitle(string PlantID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string strSql = @"SELECT P.*,'' AS Address3,'' AS Address4,c2.UserName AS CountryName,AM.Postcode,c3.UserName Company
 FROM org.Plant p
Left outer join MST.AddressMaster AM on AM.Id=P.AddressMasterId
LEFT OUTER JOIN  SCS.Country c2 ON AM.CountryId=c2.Id
LEFT OUTER JOIN ORG.Company c3 ON c3.Id=p.CompanyID
WHERE P.Id='" + PlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function
        public void getStorageLocationForDdlByLocalPO(string POSystemID, string grnSystemID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                string strSql = @"SELECT p.* FROM PurchaseOrderMasterOnRMRequisition po
INNER JOIN plant p ON po.plantID=p.PlantID

WHERE po.systemID='" + POSystemID + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");


                if (dsRef.Tables[0].Rows.Count > 0)
                {
                    if (dsRef.Tables[0].Rows[0]["IsPopulateStorageLocationByPlant"].ToString().ToUpper() == "YES")
                    {
                        //load storage location by plant

                        strSql = @"SELECT * FROM (SELECT S.*,sl.PlantID, case when ISNULL(S.Code,'')='' then S.StorageLocationName 
ELSE ISNULL(S.Code,'')+'-'+ISNULL(S.StorageLocationName,'') END AS DataText FROM StorageLocation  s
inner join StorageLocationAndPlantAssignment sl ON s.StorageLocationID=sl.StorageLocationID
inner join storagelocation sll on sll.StorageLocationID=sl.StorageLocationID and sll.StorageLocationType='Main Store'
WHERE sl.PlantID IN (select PO.plantID from PurchaseOrderMasterOnRMRequisition po where po.systemID='" + POSystemID + "' " + @") 

and sll.[Status]='Active'

UNION

SELECT S.*,sl.PlantID, case when ISNULL(S.Code,'')='' then S.StorageLocationName 
ELSE ISNULL(S.Code,'')+'-'+ISNULL(S.StorageLocationName,'') END AS DataText FROM StorageLocation  s
inner join StorageLocationAndPlantAssignment sl ON s.StorageLocationID=sl.StorageLocationID
WHERE 
 isnull(s.StorageLocationID,'') IN (
	
select isnull(g.StorageLocationID,'')
from GRNMaster g where g.SystemID='" + grnSystemID + "' " + @"

union 

select isnull(g.StorageLocationID,'')
from GRNMaterial g where g.GRNMasterSystemID='" + grnSystemID + "' " + @" 
                                                       
union 
select isnull(g.StorageLocationID,'')
from GRNPackingList g where g.GRNMasterSystemID='" + grnSystemID + "' " + @"
)) AS S ORDER BY S.CODE ";
                    }
                    else
                    {
                        //load all storage location of the company
                        strSql = @"SELECT * FROM (SELECT S.*,sl.PlantID,  case when ISNULL(S.Code,'')='' then S.StorageLocationName 
ELSE ISNULL(S.Code,'')+'-'+ISNULL(S.StorageLocationName,'') END AS DataText FROM StorageLocation  s
inner join StorageLocationAndPlantAssignment sl ON s.StorageLocationID=sl.StorageLocationID
inner join PlantAndCompanyAssignment paca ON paca.PlantID=sl.PlantID
inner join storagelocation sll on sll.StorageLocationID=sl.StorageLocationID and sll.StorageLocationType='Main Store'
WHERE paca.CompanyID IN (select paca.CompanyID from PurchaseOrderMasterOnRMRequisition po 
                         inner join PlantAndCompanyAssignment paca ON paca.PlantID=PO.PlantID
                         where po.systemID='" + POSystemID + "' " + @") 

and sll.[Status]='Active' 


UNION
SELECT S.*,sl.PlantID, case when ISNULL(S.Code,'')='' then S.StorageLocationName 
ELSE ISNULL(S.Code,'')+'-'+ISNULL(S.StorageLocationName,'') END AS DataText FROM StorageLocation  s
inner join StorageLocationAndPlantAssignment sl ON s.StorageLocationID=sl.StorageLocationID
WHERE 
 isnull(s.StorageLocationID,'') IN (
	
select isnull(g.StorageLocationID,'')
from GRNMaster g where g.SystemID='" + grnSystemID + "' " + @"

union 

select isnull(g.StorageLocationID,'')
from GRNMaterial g where g.GRNMasterSystemID='" + grnSystemID + "' " + @"
                                                       
union 
select isnull(g.StorageLocationID,'')
from GRNPackingList g where g.GRNMasterSystemID='" + grnSystemID + "' " + @"
)) AS S ORDER BY S.CODE ";
                    }

                }
                else
                {


                    strSql = @"SELECT * FROM (SELECT S.*,sl.PlantID,  case when ISNULL(S.Code,'')='' then S.StorageLocationName 
ELSE ISNULL(S.Code,'')+'-'+ISNULL(S.StorageLocationName,'') END AS DataText FROM StorageLocation  s
inner join StorageLocationAndPlantAssignment sl ON s.StorageLocationID=sl.StorageLocationID
inner join storagelocation sll on sll.StorageLocationID=sl.StorageLocationID and sll.StorageLocationType='Main Store'
WHERE sl.PlantID IN (select PO.plantID from PurchaseOrderMasterOnRMRequisition po where po.systemID='" + POSystemID + "' " + @") 

and sll.[Status]='Active'

UNION

SELECT S.*,sl.PlantID, case when ISNULL(S.Code,'')='' then S.StorageLocationName 
ELSE ISNULL(S.Code,'')+'-'+ISNULL(S.StorageLocationName,'') END AS DataText FROM StorageLocation  s
WHERE 
 isnull(s.StorageLocationID,'') IN (
	
select isnull(g.StorageLocationID,'')
from GRNMaster g where g.SystemID='" + grnSystemID + "' " + @"

union 

select isnull(g.StorageLocationID,'')
from GRNMaterial g where g.GRNMasterSystemID='" + grnSystemID + "' " + @" 
                                                       
union 
select isnull(g.StorageLocationID,'')
from GRNPackingList g where g.GRNMasterSystemID='" + grnSystemID + "' " + @"
)) AS S ORDER BY S.CODE ";

                }


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function
        public void GetBinFromPOPlantAndStorageLocation(string PlantIDCollection, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select distinct * from BIN b where plantID IN (" + PlantIDCollection + ")";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");



            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function
        public void GetBinFromPOPlant(string PlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from plant p where p.plantID IN (" + PlantID + ")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count > 0)
                {
                    if (dsRef.Tables[0].Rows[0]["IsPopulateStorageLocationByPlant"].ToString().ToUpper() == "NO")
                    {
                        strSQL = @"                                    select distinct * from BIN b
                                    INNER JOIN PlantAndCompanyAssignment p ON p.PlantID=b.plantID
                                    WHERE p.CompanyID IN (
                                        select p.CompanyID from PlantAndCompanyAssignment p
                                        where p.plantID IN(" + PlantID + "))";
                    }
                    else
                    {
                        strSQL = @"select * from BIN b
                                    where b.plantID IN (" + PlantID + ")";
                    }



                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
                }
                else
                {
                    Exception ex = new Exception("No plant found in the PO");
                    throw (ex);

                }


            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function

        private void makeDataTableFromExcel(string filePath, out DataSet dsHeader, out DataSet dsData)
        {
            dsHeader = new DataSet();
            dsData = new DataSet();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (string.IsNullOrEmpty(filePath) == false)
                {
                    string fileName =
                        Path.GetFileName(filePath);



                    string fileExtension =
                        Path.GetExtension(filePath);

                    string[] allowedExtenstions = new string[] { ".xls", ".xlsx" };
                    if (allowedExtenstions.Contains(fileExtension) == false)
                    {
                        Exception ex = new Exception("Only excel files are allowed");
                        throw (ex);
                    }


                    string fileLocation = filePath;


                    string error_user = "";
                    try
                    {
                        ExcelEngine excelEngine = null;
                        IApplication application = null;
                        IWorkbook workbook = null;
                        IWorksheet sheet = null;


                        excelEngine = new ExcelEngine();
                        application = excelEngine.Excel;
                        workbook = excelEngine.Excel.Workbooks.Open(fileLocation, ExcelOpenType.Automatic);
                        sheet = workbook.Worksheets[0];

                        DataTable dtR = sheet.ExportDataTable(sheet.UsedRange, ExcelExportDataTableOptions.ColumnNames);

                        DataView dvSO = new DataView(dtR);

                        //checking user and other validations
                        dvSO.RowFilter = colIndex.Type.ToString() + "='" + dataType.MAINHEADER.ToString() + "'";
                        if (dvSO.Count > 0)
                        {
                            if (reportScreenName.ToUpper() != dvSO[0][colIndex.RerpotName.ToString()].ToString().ToUpper())
                            {
                                error_user = "This file does not contain valid package data";
                                Exception ex = new Exception(error_user);
                                throw (ex);
                            }
                            if (identity.UserId.ToUpper() != dvSO[0][colIndex.USER.ToString()].ToString().ToUpper())
                            {
                                error_user = "login user does not match with file user [" + dvSO[0][1].ToString().ToUpper() + "]";
                                Exception ex = new Exception(error_user);
                                throw (ex);
                            }


                        }
                        else
                        {
                            error_user = "No valid data found in the file!!!";
                            Exception ex = new Exception(error_user);
                            throw (ex);
                        }

                        //header
                        dvSO.RowFilter = null;
                        dvSO.RowFilter = colIndex.Type.ToString() + "='" + dataType.MAINHEADER.ToString() + "'";
                        dsHeader.Tables.Add(dvSO.ToTable());

                        //data
                        dvSO.RowFilter = null;
                        dvSO.RowFilter = colIndex.Type.ToString() + "='" + dataType.DATA.ToString() + "'";
                        dsData.Tables.Add(dvSO.ToTable());

                        excelEngine.Dispose();
                        workbook.Close();
                        if (File.Exists(fileLocation))
                        {
                            File.Delete(fileLocation);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.ToUpper() != error_user.ToUpper())
                            ex = new Exception("File does not contain valid upload data");
                        throw (ex);
                    }
                    finally
                    {

                        if (File.Exists(fileLocation))
                            File.Delete(fileLocation);
                    }


                }
                else
                {
                    Exception ex = new Exception("Please select a local file to upload");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        private void updatePackingListFromUploadedFile(string FilePath)
        {

            DataRow dr = null;
            DataRow[] drBIN = null;
            DataRow[] drStorageLocation = null;

            FabricRollClass objStatic = null;

            DataSet dsServerDataHeader = null;
            DataSet dsHeader = null;
            DataSet dsServerData = null;
            DataSet dsAddItem = null;
            DataSet dsGRNSKU = null;
            DataSet dsStorageLocation = null;
            System.Data.DataSet dsBinNo = null;

            try
            {

                objStatic = new FabricRollClass();

                // makeDataTableFromExcel(lblGRNSystemID.Text, out dsServerDataHeader, out dsServerData);
                makeDataTableFromExcel(FilePath, out dsServerDataHeader, out dsServerData);
                if (dsServerData.Tables[0].Rows.Count == 0)
                {
                    Exception ex = new Exception("No data found in uploaded file");
                    throw (ex);
                }



                objStatic.GetFabricRollHeaderInfo_Report(dsServerDataHeader.Tables[0].Rows[0][colIndex.GRNMasterSystemID.ToString()].ToString(), out dsHeader);
                if (dsHeader.Tables[0].Rows.Count == 0)
                {
                    Exception ex = new Exception("No data found in the system according to uploaded file");
                    throw (ex);
                }


                for (int i = 0; i < dsServerData.Tables[0].Rows.Count; i++)
                {
                    clsStaticInfo.numericValidation(dsServerData.Tables[0].Rows[i][colIndex.PackingListQuantity.ToString()].ToString(), false, false, false, "Quantity");




                }



                //objStatic.getPOItemsSKUGRNPackingList_ByGRNMaterial(dsServerData.Tables[0].Rows[0][colIndex.GRNMaterialSystemID.ToString()].ToString(), out dsAddItem);
                if (dsServerData.Tables[0].Rows.Count == 0)
                {
                    Exception ex = new Exception("No data found in the system according to uploaded file");
                    throw (ex);
                }


                DataView dvLocal = new DataView();
                dvLocal.Table = dsAddItem.Tables[0];
                DataView dvLocalCopyForDelete = new DataView(dvLocal.ToTable());


                //objStatic.GetGRNSKU_ByGRNMaterialSystemID(dsServerData.Tables[0].Rows[0][colIndex.GRNMaterialSystemID.ToString()].ToString(), out dsGRNSKU);


                //clsUnitConversion objUnit = new clsUnitConversion();
                DataSet dsUnits = null;
                //objUnit.GetConversionFactorByRMCode(dsServerData.Tables[0].Rows[0][colIndex.BOMandSOWiseRMSystemID.ToString()].ToString(), out dsUnits);
                DataView dvUnits = new DataView();
                dvUnits.Table = dsUnits.Tables[0];

                double baseQuantityPL = 0;
                double baseQuantityREC = 0;
                double Quantity = 0;

                int updatedRollCount = 0;

                string sourceUOM = "";
                string BaseUOM = "";

                for (int i = 0; i < dsServerData.Tables[0].Rows.Count; i++)
                {
                    dvLocal.RowFilter = "SystemID='" + dsServerData.Tables[0].Rows[i][colIndex.SystemID.ToString()].ToString() + "'";
                    if (dvLocal.Count > 0)
                    {
                        if (dvLocal[0]["BOMandSOwiseRMSystemID"].ToString().ToUpper() != dvLocal[0]["BOMandSOwiseRMSystemIDtransferred"].ToString().ToUpper()
                            ||
                            dvLocal[0]["MaterialMasterAttributeSystemID"].ToString().ToUpper() != dvLocal[0]["MaterialMasterAttributeSystemIDtransferred"].ToString().ToUpper()
                             || dvLocal[0]["isLocationTransferred"].ToString().ToUpper() == "YES"
                            )
                        {
                            //that means, material has been transferred to another file or storage location
                            continue;
                        }
                        updatedRollCount++;
                        //height = qualified quantity
                        //sourceUOM = dsServerData.Tables[0].Rows[i][colIndex.UOMSystemID.ToString()].ToString();
                        //BaseUOM = dsServerData.Tables[0].Rows[i][colIndex.UOMSystemIDBase.ToString()].ToString();
                        Quantity = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsServerData.Tables[0].Rows[i][colIndex.PackingListQuantity.ToString()].ToString()));
                        //baseQuantityPL = convertedUOM(sourceUOM, Quantity, 0, BaseUOM, 0, dvUnits);



                        dr = dvLocal[0].Row;
                        dr.BeginEdit();

                        dr["VendorPackingFormNo"] = bplib.clsWebLib.RetValidLen(dsServerData.Tables[0].Rows[i][colIndex.VendorPackingFormNo.ToString()].ToString());
                        dr["VendorLotNo"] = bplib.clsWebLib.RetValidLen(dsServerData.Tables[0].Rows[i][colIndex.VendorLotNo.ToString()].ToString());

                        //dr["StorageLocationID"] = DBNull.Value;
                        //if (dsStorageLocation.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.StorageLocationName.ToString()].ToString() + "'").Length == 1)
                        //    dr["StorageLocationID"] = dsStorageLocation.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.StorageLocationName.ToString()].ToString() + "'")[0]["StorageLocationID"].ToString();
                        string storageLocationPlantID = "";
                        drStorageLocation = dsStorageLocation.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.StorageLocationName.ToString()].ToString() + "'");
                        if (drStorageLocation.Length > 0)
                            storageLocationPlantID = drStorageLocation[0]["PlantID"].ToString();

                        dr["BINSystemID"] = DBNull.Value;
                        DataRow[] binNo = dsBinNo.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.BinSystemID.ToString()].ToString() + "' AND PlantID='" + storageLocationPlantID + "'");
                        if (binNo.Length == 1)
                            dr["BINSystemID"] = binNo[0]["SystemID"].ToString();

                        dr["PackingListQuantity"] = bplib.clsWebLib.GetNumData(dsServerData.Tables[0].Rows[i][colIndex.PackingListQuantity.ToString()].ToString());
                        dr["PackingListQuantityBase"] = baseQuantityPL;


                        if (dr["FLAGReceivedQty"].ToString() != "YES" && dr["IsIssued"].ToString().ToUpper() != "YES"
                            && dr["IsLeftOverStock"].ToString() != "YES" && dr["isDisposed"].ToString().ToUpper() != "YES")
                        {
                            dr["ReceivedQuantity"] = bplib.clsWebLib.GetNumData(dsServerData.Tables[0].Rows[i][colIndex.PackingListQuantity.ToString()].ToString());
                            dr["ReceivedQuantityBase"] = baseQuantityPL;

                            dr["BalanceQuantityReceived"] = bplib.clsWebLib.GetNumData(dr["ReceivedQuantity"].ToString());
                            dr["BalanceQuantityReceivedBase"] = bplib.clsWebLib.GetNumData(dr["ReceivedQuantityBase"].ToString());

                        }
                        dr["Remarks"] = bplib.clsWebLib.RetValidLen(dsServerData.Tables[0].Rows[i][colIndex.Remarks.ToString()].ToString());

                        dr.EndEdit();


                    }
                }


                dvLocal = new DataView(dsAddItem.Tables[0].DefaultView.Table);

                for (int i = 0; i < dsGRNSKU.Tables[0].Rows.Count; i++)
                {
                    dvLocal.RowFilter = "GRNSKUSystemID='" + dsGRNSKU.Tables[0].Rows[i]["SystemID"].ToString() + "'";

                    dr = dsGRNSKU.Tables[0].Rows[i];
                    dr.BeginEdit();

                    dr["numberOfPackages"] = DBNull.Value;

                    if (dvLocal.Count > 0)
                    {
                        dr["numberOfPackages"] = bplib.clsWebLib.GetNumData(dvLocal.Count.ToString());

                    }
                    dr.EndEdit();
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsAddItem, dsGRNSKU);
            }

            catch (Exception ex)
            {

            }

        }
        private static void validateGRNTransfer(object text)
        {
            throw new NotImplementedException();
        }

        public static void validateGRNTransfer(string GrnMasterSystemID)
        {
            ConnectionManager.DAL.ConManager objCon;
            System.Data.DataSet dsRef;
            try
            {
                string strSql = @"SELECT * FROM GRNMaster g WHERE isnull(g.MaterialTransferSOWiseMasterSystemID,'')<>'' AND g.SystemID='" + GrnMasterSystemID + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("Material transferred to bank file. Cannot change GRN");
                    throw (ex);
                }
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function
        private string fileList = "";
        public void grnInternalControls(DataGrid dg, string GRNMasterSystemID, string POSystemID)
        {
            DataSet dsTemp = null;
            try
            {
                if (fileList == "")
                    fileList = listFiles(dg);

                //inventory file closed
                getInventoryClosedFiles(fileList, GRNMasterSystemID, out dsTemp);
                if (dsTemp.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("File# " + dsTemp.Tables[0].Rows[0]["fileNos"].ToString() + " has been closed for inventory therefore cannot update/save/delete current GRN");
                }

                //file opened for physical inventory
                getOpenFiles(fileList, GRNMasterSystemID, out dsTemp);
                if (dsTemp.Tables[0].Rows.Count > 0)
                    throw new Exception("File# " + dsTemp.Tables[0].Rows[0]["fileNos"].ToString() + " has been opened for physical inventory, Document# " + dsTemp.Tables[0].Rows[0]["physicalInventoryDocumentMasterSystemID"].ToString() + " therefore cannot update/save/delete current GRN");


                //po approval related
                //getPurchaseOrderMasterOnRMRequisition(POSystemID, out dsTemp);
                //if (dsTemp.Tables[0].Rows.Count > 0)
                {
                    if (bplib.clsWebLib.GetBoolData(dsTemp.Tables[0].Rows[0]["isOutOfBudget"].ToString()) == true && bplib.clsWebLib.GetBoolData(dsTemp.Tables[0].Rows[0]["isPOApproved"].ToString()) == false)
                        throw new Exception("Unapproved PO (Out of budget), cannot update/save/delete current GRN");

                    if (bplib.clsWebLib.GetBoolData(dsTemp.Tables[0].Rows[0]["isPOApproved"].ToString()) == false && dsTemp.Tables[0].Rows[0]["POType"].ToString().ToUpper() == "LOCAL PO")
                        throw new Exception("Unapproved PO (LOCAL PO), cannot update/save/delete current GRN");

                    //if (bplib.clsWebLib.GetBoolData(dsTemp.Tables[0].Rows[0]["isPOApproved"].ToString()) == false && dsTemp.Tables[0].Rows[0]["POType"].ToString().ToUpper() == "DIRECT LC")
                    //    throw new Exception("Unapproved PO (DIRECT LC), cannot update/save/delete current GRN");
                }



            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void grnInternalControls(string BOMAndSoWiseRMSystemID, string POSystemID)
        {
            try
            {
                DataSet dsLocal = null;
                GetBOMandSOWiseRM(BOMAndSoWiseRMSystemID, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                    fileList = "'" + dsLocal.Tables[0].Rows[0]["SalesOrderMasterSystemID"].ToString() + "'";


                grnInternalControls(null, "", POSystemID);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public void GetBOMandSOWiseRM(string systemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
SELECT bsr.*,mgm.GridNo, mm.MaterialCode, mm.MaterialDesc, mm.MaterialDetailDesc,mm.PackingForm,
mg.ItemDescription AS MaterialGroup,mt.MaterialTypeDesc
  FROM BOMandSOWiseRM bsr
LEFT OUTER JOIN MaterialMaster mm ON mm.SystemID=bsr.MaterialMasterSystemID
LEFT OUTER JOIN MaterialGroup mg ON mg.SystemID=mm.materialGroupID
LEFT OUTER JOIN MaterialType mt ON mt.MaterialTypeID=mm.MaterialTypeID
LEFT OUTER JOIN MaterialGridMaster mgm ON mgm.SystemID=mm.materialGridMasterSystemID
                        WHERE bsr.systemID='" + systemID + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function
        public void Save(string filename, string extension, FabricRollFile file, out DataSet dsMaster)
        {
            try
            {

                GetData(file.Id, out dsMaster);
                _Save(ref dsMaster, filename, extension, file);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetData(string FileId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from FabricRollFile where Id='" + FileId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        void _Save(ref DataSet dsSaveBonusMaster, string filename, string extension, FabricRollFile ui_master)
        {
            DataView _dvSave = null;
            //_masterpk = string.Empty;
            try
            {
                _dvSave = new DataView(dsSaveBonusMaster.Tables[0]);
                _dvSave.RowFilter = "Id ='" + ui_master.Id + "'";
                if (_dvSave.Count == 0)
                {
                    DataRow dr = dsSaveBonusMaster.Tables[0].NewRow();
                    _SaveCol("ADDNEW", filename, extension, ui_master, ref dr);
                    dsSaveBonusMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _SaveCol("Edit", filename, extension, ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void _SaveCol(string OPN_FLAG, string filename, string extension, FabricRollFile ui_master, ref DataRow drLocal)
        {
            bplib.clsGenID objGenID = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string idFromDB = "";
            string systemID = "";

            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "File", out idFromDB);
                    //systemID =  idFromDB;
                    //ui_master.Id = systemID.Trim();
                    drLocal["Id"] = bplib.clsWebLib.RetValidLen(idFromDB);
                    drLocal["FileId"] = idFromDB + extension;
                    drLocal["FileName"] = filename;
                    drLocal["FileStatus"] = "Uploaded";
                    drLocal["PlantId"] = ui_master.PlantId;
                    drLocal["AddedBy"] = identity.Name;
                    drLocal["AddedFromIP"] = identity.IPAddress;
                    drLocal["AddedDate"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);

                }
                else
                {
                    drLocal["UpdatedBy"] = ui_master.AddedBy;
                    drLocal["UpdatedFromIP"] = identity.IPAddress;
                    drLocal["UpdatedDate"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function


        public IEnumerable<object> GRNList(string column, string value, string fromDate, string toDate, string PlantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false)
                    strkey = column + " like '%" + value + "%'";

                string _sql = @"SELECT * FROM (SELECT IR.Id GRNNo
                                    ,IR.Status GRNStatus
                                    ,FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, INV.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,NULL) POId,PO.PODate
									,isnull(PO.PurchaseLCId,NULL) PurchaseLCId
									,isnull(PO.ContractId,NULL) ContractId
                                    ,ISNull(po.ContractNo,NULL) ContractNo,isnull(PO.LCANo,NULL) LCANo,isnull(PO.LCDate,NULL) LCDate
									,PO.VendorRefNo,PO.PINo,PO.PurchaseLCNo
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,NULL) GateName,IR.NoteForAccounts--,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy
                                    
									,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.GRNType
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
							,PO.UDNo,ISNULL(MLC.OpeningBank,NULL) OpeningBank,ISNULL(Pr.UserName ,NULL) CustomerName
,BuyerPONumber=STUFF((SELECT DISTINCT ','+PO.PONumber from
                            			BOQ boq
                            			INNER JOin trn.POBOQMAP xboqMap on boq.Id=xboqMap.BOQDetailId
										INNER JOIN trn.PurchaseOrderDetail xpod on xpod.Id=xboqMap.PODetailId
										LEFT OUTER JOIN [TRN].[SalesOrder] AS so ON so.MasterOrderItemId=boq.MasterOrderItemId
										LEFT OUTER JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
										left join trn.GRNPORequisitionAllocation pogrnmap on pogrnmap.BOQDetailId=boq.Id
										LEFT JOIN (select * from TRN.InventoryReceiveDetail) IRD on IRD.InventoryReceiveId=IR.Id
                            			WHERE pogrnmap.InventoryReceiveDetailId=IRD.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
,BuyerReferenceNo=STUFF((select distinct ','+MO.BuyerReferenceNo from
									trn.PurchaseOrderDetail POD
									LEFT JOIN TRN.PurchaseOrder xpo on xpo.Id=POD.InventoryReceiveId
									LEFT JOIN (select * from TRN.InventoryReceiveDetail) IRD on IRD.InventoryReceiveId=IR.Id
									LEFT JOIN DBO.[Contract] C on C.Id=xpo.ContractId
									LEFT JOIN trn.SalesOrder SO ON SO.ContractId=C.Id
									LEFT JOIN trn.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
									LEFT JOIN trn.MasterOrder MO on MO.Id=MOI.MasterOrderId
									where POD.Id=IRD.PODetailsId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN TRN.Invoice INV ON INV.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                         LEFT JOIN(SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,VendorRefNo=STUFF((select distinct ','+xpo.DocRefNo  from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,PurchaseLCNo=STUFF((select distinct ','+PLC.LCRef from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,PINo=STUFF((select distinct ','+PLC.PINo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,PODate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), xpo.PODate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate,PODate
							)PO ON PO.GRNId = IR.Id
							LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
							LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
							LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CON.MasterLCId

							JOIN 
							(
							SELECT DISTINCT IRD1.InventoryReceiveId FROM TRN.InventoryReceiveDetail IRD1
							LEFT JOIN TRN.PurchaseOrder po1 on po1.id=IRD1.POId
							LEFT JOIN TRN.InventoryMaterial IM ON IRD1.InventoryMaterialId=IM.Id
							LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialMasterBusinessProcess MMBP ON MM.Id=MMBP.MaterialMasterId
							LEFT JOIN SCS.BusinessProcess BP ON MMBP.BusinessProcessId=BP.Id
                        WHERE BP.BusinessProcessName='FabricRollManagement'
						) D ON D.InventoryReceiveId=IR.Id --and IR.GRNType in('GRNBYPO','GRN' ,'EMPGRN','GRNBYBOQ')
					  and IR.GRNType in('GRNBYPO','GRN' ,'EMPGRN','GRNBYBOQ') AND IR.AddedDate between '" + fromDate + @"' AND '" + toDate + @"') AS TEMP WHERE " + strkey;
                return _sqlRepository.GetDataCollection(_sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> MaterialList(string inventoryReceiveId)
        {
            try
            {
                string _sql = @"SELECT 
DISTINCT IRD.Id,IRD.InventoryReceiveId,IRD.TransactionQty,IRD.TransactionUoMId,Isnull(FRM.SplitCount,0)SplitCount
,ISNULL(FRM.TotalDistributeQty,0)TotalDistributeQty,UOM.UserName UOM,BUoM.UserName BaseUoM,IR.Id GRNNo,IR.GRNDate
,P.UserName PartyName,PL.FabRollPrefix,IM.PlantId,IM.MaterialMasterId,IM.ArticleId
,IM.FirstCharacteristicsId SKUId,MM.UserName MaterialMasterName,MMA.StandardName ArticleName
,C.UserName SKU1,C2.UserName SKU2,C3.UserName SKU3,CV.UserName SKUValue,CV2.UserName SKUValue2,CV3.UserName SKUValue3, C.UserName +':'+CV.UserName SKUInfo,CU.Code
,MGM.UserName MaterialGroup
FROM [TRN].[InventoryReceiveDetail] IRD
                                        LEFT JOIN TRN.InventoryReceive IR ON IRD.InventoryReceiveId=IR.Id
                                        LEFT JOIN HKP.Party P ON IR.PartyId=P.Id
                                        LEFT JOIN TRN.InventoryMaterial IM ON IRD.InventoryMaterialId=IM.Id
										--LEFT JOIN ORG.Plant PL ON IM.PlantId= PL.Id
                                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
										LEFT JOIN scs.PlantConfig PL ON  PL.PlantId=IM.PlantId
                                        LEFT JOIN SCS.UnitOfMeasurement UOM ON IRD.TransactionUoMId=UOM.Id
                                        LEFT JOIN SCS.UnitOfMeasurement BUoM ON IRD.BaseUOMId=BUoM.Id
                                        LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id

                                        LEFT JOIN MST.MaterialGroupMaster MGM ON MM.MaterialGroupMasterId=MGM.Id
                                        LEFT JOIN MST.MaterialMasterArticle MMA ON IM.ArticleId=MMA.Id

                                        LEFT JOIN HKP.Characteristics C ON IM.FirstCharacteristicsId=C.Id
                                        LEFT JOIN HKP.Characteristics C2 ON IM.SecondCharacteristicsId=C2.Id
                                        LEFT JOIN HKP.Characteristics C3 ON IM.ThirdCharacteristicsId=C3.Id

                                        LEFT JOIN [HKP].[CharacteristicsValue] CV ON IM.FirstCharacteristicsValueId=CV.Id
                                        LEFT JOIN [HKP].[CharacteristicsValue] CV2 ON IM.SecondCharacteristicsValueId=CV2.Id
                                        LEFT JOIN [HKP].[CharacteristicsValue] CV3 ON IM.ThirdCharacteristicsValueId=CV3.Id
                                        LEFT JOIN MST.MaterialMasterBusinessProcess MMBP ON MM.Id=MMBP.MaterialMasterId
                                        LEFT JOIN SCS.BusinessProcess BP ON MMBP.BusinessProcessId=BP.Id
										LEFT JOIN (SELECT COUNT(Id) SplitCount,Sum(VendorQty) TotalDistributeQty
										,InventoryReceiveDetailId FROM TRN.FabricRollMaster 
										GROUP BY InventoryReceiveDetailId) FRM ON IRD.Id=FRM.InventoryReceiveDetailId
WHERE BP.BusinessProcessName='FabricRollManagement' AND IRD.InventoryReceiveId='" + inventoryReceiveId + @"'";
                return _sqlRepository.GetDataCollection(_sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMaterialListData(string inventoryReceiveId)
        {
            try
            {
                string _sql = @"SELECT 
DISTINCT IRD.Id,IRD.InventoryReceiveId,IRD.TransactionQty,IRD.TransactionUoMId,Isnull(FRM.SplitCount,0)SplitCount
,ISNULL(FRM.TotalDistributeQty,0)TotalDistributeQty,UOM.UserName UOM,BUoM.UserName BaseUoM,IR.Id GRNNo,IR.GRNDate
,P.UserName PartyName,PL.FabRollPrefix,IM.PlantId,IM.MaterialMasterId,IM.ArticleId
,IM.FirstCharacteristicsId SKUId,MM.UserName MaterialMasterName,MMA.StandardName ArticleName
,C.UserName SKU1,C2.UserName SKU2,C3.UserName SKU3,CV.UserName SKUValue,CV2.UserName SKUValue2,CV3.UserName SKUValue3, C.UserName +':'+CV.UserName SKUInfo,CU.Code
,MGM.UserName MaterialGroup,CAST(0 AS bit) Flag
FROM [TRN].[InventoryReceiveDetail] IRD
                                        LEFT JOIN TRN.InventoryReceive IR ON IRD.InventoryReceiveId=IR.Id
                                        LEFT JOIN HKP.Party P ON IR.PartyId=P.Id
                                        LEFT JOIN TRN.InventoryMaterial IM ON IRD.InventoryMaterialId=IM.Id
										--LEFT JOIN ORG.Plant PL ON IM.PlantId= PL.Id
                                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
										LEFT JOIN scs.PlantConfig PL ON  PL.PlantId=IM.PlantId
                                        LEFT JOIN SCS.UnitOfMeasurement UOM ON IRD.TransactionUoMId=UOM.Id
                                        LEFT JOIN SCS.UnitOfMeasurement BUoM ON IRD.BaseUOMId=BUoM.Id
                                        LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id

                                        LEFT JOIN MST.MaterialGroupMaster MGM ON MM.MaterialGroupMasterId=MGM.Id
                                        LEFT JOIN MST.MaterialMasterArticle MMA ON IM.ArticleId=MMA.Id

                                        LEFT JOIN HKP.Characteristics C ON IM.FirstCharacteristicsId=C.Id
                                        LEFT JOIN HKP.Characteristics C2 ON IM.SecondCharacteristicsId=C2.Id
                                        LEFT JOIN HKP.Characteristics C3 ON IM.ThirdCharacteristicsId=C3.Id

                                        LEFT JOIN [HKP].[CharacteristicsValue] CV ON IM.FirstCharacteristicsValueId=CV.Id
                                        LEFT JOIN [HKP].[CharacteristicsValue] CV2 ON IM.SecondCharacteristicsValueId=CV2.Id
                                        LEFT JOIN [HKP].[CharacteristicsValue] CV3 ON IM.ThirdCharacteristicsValueId=CV3.Id
                                        LEFT JOIN MST.MaterialMasterBusinessProcess MMBP ON MM.Id=MMBP.MaterialMasterId
                                        LEFT JOIN SCS.BusinessProcess BP ON MMBP.BusinessProcessId=BP.Id
										LEFT JOIN (SELECT COUNT(Id) SplitCount,Sum(VendorQty) TotalDistributeQty
										,InventoryReceiveDetailId FROM TRN.FabricRollMaster 
										GROUP BY InventoryReceiveDetailId) FRM ON IRD.Id=FRM.InventoryReceiveDetailId
WHERE BP.BusinessProcessName='FabricRollManagement' AND IRD.InventoryReceiveId='" + inventoryReceiveId + @"'";
                return _sqlRepository.GetDataCollection(_sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> FabricRollList(string inventoryReceiveDetailId)
        {
            try
            {
                string _sql = @"select * from TRN.FabricRollMaster where InventoryReceiveDetailId='" + inventoryReceiveDetailId + @"'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetFabricRollChildList(string FabricRollManagementMasterId)
        {
            try
            {
                string _sql = @"Select * from [BPDT].[FabricRollManagementChild] Where FabricRollManagementMasterId='" + FabricRollManagementMasterId + "' Order By Sequence";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetFabricRollChildPendingDataList(string PlantId)
        {
            try
            {
                string _sql = @"SELECT M.*,PE.EmployeeName PreparedBy,CE.EmployeeName CheckedBy FROM [BPDT].[FabricRollManagementMaster] M
LEFT JOIN dbo.EmployeeInformation PE on PE.SystemId=M.PreparedById
LEFT JOIN dbo.EmployeeInformation CE on CE.SystemId=M.CheckedById
 Where M.PlantId='" + PlantId + "' AND ISNULL(M.IsChecked,0)=0 Order By M.GRNId";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSavedList(string GRNId, string PlantId)
        {
            try
            {
                string sql = @"
SELECT F.Id,IR.Id GRNNo,F.PreparedById,F.CheckedById,F.Remarks,F.Comment,F.UserName
,PE.EmployeeCode PreparedByCode,CE.EmployeeCode CheckedByCode,PE.EmployeeName PreparedByName,CE.EmployeeName CheckedByName
                                    ,IR.Status GRNStatus
                                    ,FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, INV.InvoiceNo
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,NULL) POId,PO.PODate
									,isnull(PO.PurchaseLCId,NULL) PurchaseLCId
									,isnull(PO.ContractId,NULL) ContractId
                                    ,ISNull(po.ContractNo,NULL) ContractNo,isnull(PO.LCANo,NULL) LCANo,isnull(PO.LCDate,NULL) LCDate
									,PO.VendorRefNo,PO.PINo,PO.PurchaseLCNo
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,NULL) GateName,IR.NoteForAccounts--,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy                                    
									,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.GRNType
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
							,PO.UDNo,ISNULL(MLC.OpeningBank,NULL) OpeningBank,ISNULL(Pr.UserName ,NULL) CustomerName
							,BuyerPONumber=STUFF((SELECT DISTINCT ','+PO.PONumber from
                            			BOQ boq
                            			INNER JOin trn.POBOQMAP xboqMap on boq.Id=xboqMap.BOQDetailId
										INNER JOIN trn.PurchaseOrderDetail xpod on xpod.Id=xboqMap.PODetailId
										LEFT OUTER JOIN [TRN].[SalesOrder] AS so ON so.MasterOrderItemId=boq.MasterOrderItemId
										LEFT OUTER JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
										left join trn.GRNPORequisitionAllocation pogrnmap on pogrnmap.BOQDetailId=boq.Id
										LEFT JOIN (select * from TRN.InventoryReceiveDetail) IRD on IRD.InventoryReceiveId=IR.Id
                            			WHERE pogrnmap.InventoryReceiveDetailId=IRD.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
,BuyerReferenceNo=STUFF((select distinct ','+MOI.BuyerReferenceNo from
									trn.PurchaseOrderDetail POD
									LEFT JOIN TRN.PurchaseOrder xpo on xpo.Id=POD.InventoryReceiveId
									LEFT JOIN (select * from TRN.InventoryReceiveDetail) IRD on IRD.InventoryReceiveId=IR.Id
									LEFT JOIN DBO.[Contract] C on C.Id=xpo.ContractId
									LEFT JOIN trn.SalesOrder SO ON SO.ContractId=C.Id
									LEFT JOIN trn.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
									LEFT JOIN trn.MasterOrder MO on MO.Id=MOI.MasterOrderId
									where POD.Id=IRD.PODetailsId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							from [BPDT].[FabricRollManagementMaster] F 
							LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=F.GRNId 
							LEFT JOIN dbo.EmployeeInformation PE ON PE.SystemId=F.PreparedById
LEFT JOIN dbo.EmployeeInformation CE ON CE.SystemId=F.CheckedById
							JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
							LEFT JOIN TRN.Invoice INV ON INV.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId 
						FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId	
                         LEFT JOIN(SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,VendorRefNo=STUFF((select distinct ','+xpo.DocRefNo  from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,PurchaseLCNo=STUFF((select distinct ','+PLC.LCRef from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,PINo=STUFF((select distinct ','+PLC.PINo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,PODate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), xpo.PODate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate,PODate
							)PO ON PO.GRNId = IR.Id
							LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
							LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
							LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CON.MasterLCId

							JOIN 
							(
							SELECT DISTINCT IRD1.InventoryReceiveId FROM TRN.InventoryReceiveDetail IRD1
							LEFT JOIN TRN.PurchaseOrder po1 on po1.id=IRD1.POId
							LEFT JOIN TRN.InventoryMaterial IM ON IRD1.InventoryMaterialId=IM.Id
							LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialMasterBusinessProcess MMBP ON MM.Id=MMBP.MaterialMasterId
							LEFT JOIN SCS.BusinessProcess BP ON MMBP.BusinessProcessId=BP.Id
                        WHERE BP.BusinessProcessName='FabricRollManagement'
						) D ON D.InventoryReceiveId=IR.Id
Where F.GRNId='" + GRNId + "'"; ;
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveFabricRollManageData(Dictionary<string, object> data, List<Dictionary<string, object>> grnDetailList, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsMaster, dsDetail, dsGRNDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [BPDT].[FabricRollManagementMaster] where UserName='" + data["UserName"] + "'", out DataSet dsFabricRollManagementMasterUserNameValidation, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [BPDT].[FabricRollManagementMaster] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id, _detailId = "";


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    if (dsFabricRollManagementMasterUserNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "FabricRollManagementMaster", out _Id);

                        data["Id"] = _Id;
                        data["PlantId"] = identity.PlantId;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["PlantId"] = identity.PlantId;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                con.OpenDataSetThroughAdapter("SELECT * FROM BPDT.FabricRollManagementChild WHERE FabricRollManagementMasterId ='" + masterId + "'", out dsDetail, false, "1");
                con.OpenDataSetThroughAdapter("SELECT COUNT(Id)Id FROM [BPDT].[FabricRollManagementChild] WHERE FabricRollManagementMasterId ='" + masterId + "'", out dsId, false, "1");

                int count = Convert.ToInt32(dsId.Tables[0].Rows[0]["Id"].ToString());


                foreach (var item in grnDetailList)
                {

                    DataView dv = new DataView(dsDetail.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (item["CutableWidth"]==null)
                    {
                        throw new Exception("Cutable Width is required.");
                    }
                    if (item["Shade"]==null)
                    {
                        throw new Exception("Shade is required.");
                    }
                    if (item["ShrinkageLengthWise"]==null)
                    {
                        throw new Exception("Shrinkage Length Wise is required.");
                    }
                    if (item["ShrinkageWidthWise"] == null)
                    {
                        throw new Exception("Shrinkage Width Wise is required.");
                    }

                    if (dv.Count == 0)
                    {
                        count++;

                        item["Id"] = masterId + "-" + count;
                        item["Sequence"] = count;
                        item["FabricRollManagementMasterId"] = masterId;

                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                }


                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail);

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
    }
}
public class FabricRollFile
{
    public string Id { get; set; }
    public string FileId { get; set; }
    public string FileName { get; set; }
    public string FileStatus { get; set; }
    public string PlantId { get; set; }
    public string AddedBy { get; set; }
    public string AddedDate { get; set; }
    public string AddedFromIP { get; set; }
    public string UpdatedBy { get; set; }
    public string UpdatedDate { get; set; }
    public string UpdatedFromIP { get; set; }

}

