using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
            Remarks
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



                sheet[ROW, COL].Text = "Supplier Roll No";
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
                    sheet.Range[ROW, colRollControlNo, ROW, colCurrentReceivedQty].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;


                    //sheet[ROW, colStorageLocation].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ROW, leftCOL, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, leftCOL, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, leftCOL, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                sheet.Range[1, leftCOL, ROW, endCol].WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.UsedRange.NumberFormat = clsGRNReports.NumberFormatStringFormulaTwoDecimal;

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

        //private void makeDataTableFromExcel(string GRNSystemID, out DataSet dsHeader, out DataSet dsData)
        //{
        //    dsHeader = new DataSet();
        //    dsData = new DataSet();

        //    try
        //    {
        //        if (ctrlFileUpload.HasFile)
        //        {
        //            string fileName =
        //                Path.GetFileName(ctrlFileUpload.PostedFile.FileName);



        //            string fileExtension =
        //                Path.GetExtension(ctrlFileUpload.PostedFile.FileName);

        //            string[] allowedExtenstions = new string[] { ".xls", ".xlsx" };
        //            if (allowedExtenstions.Contains(fileExtension) == false)
        //            {
        //                Exception ex = new Exception("Only excel files are allowed");
        //                throw (ex);
        //            }


        //            string fileLocation = Server.MapPath("~/DOC/" + fileName + Session.SessionID);
        //            ctrlFileUpload.SaveAs(fileLocation);


        //            string error_user = "";
        //            try
        //            {
        //                ExcelEngine excelEngine = null;
        //                IApplication application = null;
        //                IWorkbook workbook = null;
        //                IWorksheet sheet = null;


        //                excelEngine = new ExcelEngine();
        //                application = excelEngine.Excel;
        //                workbook = excelEngine.Excel.Workbooks.Open(fileLocation, ExcelOpenType.Automatic);
        //                sheet = workbook.Worksheets[0];

        //                DataTable dtR = sheet.ExportDataTable(sheet.UsedRange, ExcelExportDataTableOptions.ColumnNames);

        //                DataView dvSO = new DataView(dtR);

        //                //checking user and other validations
        //                dvSO.RowFilter = colIndex.Type.ToString() + "='" + dataType.MAINHEADER.ToString() + "'";
        //                if (dvSO.Count > 0)
        //                {
        //                    if (reportScreenName.ToUpper() != dvSO[0][colIndex.RerpotName.ToString()].ToString().ToUpper())
        //                    {
        //                        error_user = "This file does not contain valid package data";
        //                        Exception ex = new Exception(error_user);
        //                        throw (ex);
        //                    }
        //                    if (identit.ToString().ToUpper() != dvSO[0][colIndex.USER.ToString()].ToString().ToUpper())
        //                    {
        //                        error_user = "login user does not match with file user [" + dvSO[0][1].ToString().ToUpper() + "]";
        //                        Exception ex = new Exception(error_user);
        //                        throw (ex);
        //                    }
        //                    if (GRNSystemID.ToUpper() != dvSO[0][colIndex.GRNMasterSystemID.ToString()].ToString().ToUpper())
        //                    {
        //                        error_user = "Selected GRN No does not match with uploaded GRN No.";
        //                        Exception ex = new Exception(error_user);
        //                        throw (ex);
        //                    }

        //                }
        //                else
        //                {
        //                    error_user = "No valid data found in the file!!!";
        //                    Exception ex = new Exception(error_user);
        //                    throw (ex);
        //                }

        //                //header
        //                dvSO.RowFilter = null;
        //                dvSO.RowFilter = colIndex.Type.ToString() + "='" + dataType.MAINHEADER.ToString() + "'";
        //                dsHeader.Tables.Add(dvSO.ToTable());

        //                //data
        //                dvSO.RowFilter = null;
        //                dvSO.RowFilter = colIndex.Type.ToString() + "='" + dataType.DATA.ToString() + "'";
        //                dsData.Tables.Add(dvSO.ToTable());

        //                excelEngine.Dispose();
        //                workbook.Close();
        //                if (File.Exists(fileLocation))
        //                {
        //                    File.Delete(fileLocation);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                if (ex.Message.ToUpper() != error_user.ToUpper())
        //                    ex = new Exception("File does not contain valid upload data");
        //                throw (ex);
        //            }
        //            finally
        //            {

        //                if (File.Exists(fileLocation))
        //                    File.Delete(fileLocation);
        //            }


        //        }
        //        else
        //        {
        //            Exception ex = new Exception("Please select a local file to upload");
        //            throw (ex);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }

        //}
        //private void updatePackingListFromUploadedFile()
        //{

        //    DataRow dr = null;
        //    DataRow[] drBIN = null;
        //    DataRow[] drStorageLocation = null;

        //    FabricRollClass objStatic = null;

        //    DataSet dsServerDataHeader = null;
        //    DataSet dsHeader = null;
        //    DataSet dsServerData = null;
        //    DataSet dsAddItem = null;
        //    DataSet dsGRNSKU = null;
        //    DataSet dsStorageLocation = null;
        //    System.Data.DataSet dsBinNo = null;

        //    try
        //    {

        //        objStatic = new FabricRollClass();

        //        FabricRollClass.validateGRNTransfer(lblGRNSystemID.Text);

        //        makeDataTableFromExcel(lblGRNSystemID.Text, out dsServerDataHeader, out dsServerData);
        //        if (dsServerData.Tables[0].Rows.Count == 0)
        //        {
        //            Exception ex = new Exception("No data found in uploaded file");
        //            throw (ex);
        //        }

        //        objStatic.grnInternalControls(dsServerDataHeader.Tables[0].Rows[0][colIndex.BOMandSOWiseRMSystemID.ToString()].ToString(), lblPOSystemID.Text);


        //        objStatic.GetGRNHeaderInfo_Report(dsServerDataHeader.Tables[0].Rows[0][colIndex.GRNMasterSystemID.ToString()].ToString(), out dsHeader);
        //        if (dsHeader.Tables[0].Rows.Count == 0)
        //        {
        //            Exception ex = new Exception("No data found in the system according to uploaded file");
        //            throw (ex);
        //        }
        //        objStatic.getStorageLocationForDdlByLocalPO(dsHeader.Tables[0].Rows[0]["POSystemID"].ToString(), dsHeader.Tables[0].Rows[0]["GRNNO"].ToString(), out dsStorageLocation);
        //        objStatic.GetBinFromCompanyID(dsHeader.Tables[0].Rows[0]["POSystemID"].ToString(), out dsBinNo);


        //        for (int i = 0; i < dsServerData.Tables[0].Rows.Count; i++)
        //        {
        //            clsStaticInfo.numericValidation(dsServerData.Tables[0].Rows[i][colIndex.PackingListQuantity.ToString()].ToString(), false, false, false, "Quantity");


        //            //storage location
        //            if (dsServerData.Tables[0].Rows[i][colIndex.StorageLocationName.ToString()].ToString() != "")
        //            {
        //                if (dsStorageLocation.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.StorageLocationName.ToString()].ToString() + "'").Length != 1)
        //                {
        //                    Exception ex = new Exception("storage location [" + dsServerData.Tables[0].Rows[i][colIndex.StorageLocationName.ToString()].ToString() + "] not found in database!!!");
        //                    throw (ex);
        //                }
        //            }
        //            else
        //            {
        //                Exception ex = new Exception("Plase insert storage location for package no-[" + dsServerData.Tables[0].Rows[i][colIndex.RollControlNo.ToString()].ToString() + "]");
        //                throw (ex);
        //            }

        //            ////BIN No
        //            //if (dsServerData.Tables[0].Rows[i][colIndex.BinSystemID.ToString()].ToString() != "")
        //            //{
        //            //    if (dsBinNo.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.BinSystemID.ToString()].ToString() + "'").Length != 1)
        //            //    {
        //            //        Exception ex = new Exception("BIN [" + dsServerData.Tables[0].Rows[i][colIndex.BinSystemID.ToString()].ToString() + "] not found in database!!!");
        //            //        throw (ex);
        //            //    }
        //            //}

        //            #region bin and storage location validation

        //            if (dsServerData.Tables[0].Rows[i][colIndex.BinSystemID.ToString()].ToString() != "")
        //            {
        //                //if (dsServerData.Tables[0].Rows[i][colIndex.StorageLocationName.ToString()].ToString()=="")
        //                drBIN = dsBinNo.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.BinSystemID.ToString()].ToString() + "'");
        //                drStorageLocation = dsStorageLocation.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.StorageLocationName.ToString()].ToString() + "'");

        //                if (drBIN.Length > 0)
        //                {
        //                    if (drStorageLocation.Length > 0)
        //                    {

        //                        drBIN = dsBinNo.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.BinSystemID.ToString()].ToString() + "' AND PlantID='" + drStorageLocation[0]["PlantID"].ToString() + "'");
        //                        if (drBIN.Length == 0)
        //                        {
        //                            Exception ex = new Exception("BIN [" + dsServerData.Tables[0].Rows[i][colIndex.BinSystemID.ToString()].ToString() + "] not found in database!!!");
        //                            throw (ex);
        //                        }
        //                        else
        //                        {
        //                            if (dsBinNo.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.BinSystemID.ToString()].ToString() + "' AND PlantID='" + drStorageLocation[0]["PlantID"].ToString() + "'").Length != 1)
        //                            {
        //                                Exception ex = new Exception("Multiple BIN [" + dsServerData.Tables[0].Rows[i][colIndex.BinSystemID.ToString()].ToString() + "] found for same plant!!!");
        //                                throw (ex);
        //                            }


        //                        }


        //                    }
        //                    else
        //                    {
        //                        Exception ex = new Exception("BIN [" + dsServerData.Tables[0].Rows[i][colIndex.BinSystemID.ToString()].ToString() + "] found but storage location is missing!!!");
        //                        throw (ex);
        //                    }
        //                }
        //                else
        //                {
        //                    Exception ex = new Exception("BIN [" + dsServerData.Tables[0].Rows[i][colIndex.BinSystemID.ToString()].ToString() + "] not found in database!!!");
        //                    throw (ex);
        //                }

        //            }

        //            #endregion bin and storage location validation

        //        }



        //        objStatic.getPOItemsSKUGRNPackingList_ByGRNMaterial(dsServerData.Tables[0].Rows[0][colIndex.GRNMaterialSystemID.ToString()].ToString(), out dsAddItem);
        //        if (dsServerData.Tables[0].Rows.Count == 0)
        //        {
        //            Exception ex = new Exception("No data found in the system according to uploaded file");
        //            throw (ex);
        //        }


        //        DataView dvLocal = new DataView();
        //        dvLocal.Table = dsAddItem.Tables[0];
        //        DataView dvLocalCopyForDelete = new DataView(dvLocal.ToTable());


        //        objStatic.GetGRNSKU_ByGRNMaterialSystemID(dsServerData.Tables[0].Rows[0][colIndex.GRNMaterialSystemID.ToString()].ToString(), out dsGRNSKU);


        //        clsUnitConversion objUnit = new clsUnitConversion();
        //        DataSet dsUnits = null;
        //        objUnit.GetConversionFactorByRMCode(dsServerData.Tables[0].Rows[0][colIndex.BOMandSOWiseRMSystemID.ToString()].ToString(), out dsUnits);
        //        DataView dvUnits = new DataView();
        //        dvUnits.Table = dsUnits.Tables[0];

        //        double baseQuantityPL = 0;
        //        double baseQuantityREC = 0;
        //        double Quantity = 0;

        //        int updatedRollCount = 0;

        //        string sourceUOM = "";
        //        string BaseUOM = "";

        //        for (int i = 0; i < dsServerData.Tables[0].Rows.Count; i++)
        //        {
        //            dvLocal.RowFilter = "SystemID='" + dsServerData.Tables[0].Rows[i][colIndex.SystemID.ToString()].ToString() + "'";
        //            if (dvLocal.Count > 0)
        //            {
        //                if (dvLocal[0]["BOMandSOwiseRMSystemID"].ToString().ToUpper() != dvLocal[0]["BOMandSOwiseRMSystemIDtransferred"].ToString().ToUpper()
        //                    ||
        //                    dvLocal[0]["MaterialMasterAttributeSystemID"].ToString().ToUpper() != dvLocal[0]["MaterialMasterAttributeSystemIDtransferred"].ToString().ToUpper()
        //                     || dvLocal[0]["isLocationTransferred"].ToString().ToUpper() == "YES"
        //                    )
        //                {
        //                    //that means, material has been transferred to another file or storage location
        //                    continue;
        //                }
        //                updatedRollCount++;
        //                //height = qualified quantity
        //                sourceUOM = dsServerData.Tables[0].Rows[i][colIndex.UOMSystemID.ToString()].ToString();
        //                BaseUOM = dsServerData.Tables[0].Rows[i][colIndex.UOMSystemIDBase.ToString()].ToString();
        //                Quantity = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsServerData.Tables[0].Rows[i][colIndex.PackingListQuantity.ToString()].ToString()));
        //                baseQuantityPL = convertedUOM(sourceUOM, Quantity, 0, BaseUOM, 0, dvUnits);



        //                dr = dvLocal[0].Row;
        //                dr.BeginEdit();

        //                dr["VendorPackingFormNo"] = bplib.clsWebLib.RetValidLen(dsServerData.Tables[0].Rows[i][colIndex.VendorPackingFormNo.ToString()].ToString());
        //                dr["VendorLotNo"] = bplib.clsWebLib.RetValidLen(dsServerData.Tables[0].Rows[i][colIndex.VendorLotNo.ToString()].ToString());

        //                //dr["StorageLocationID"] = DBNull.Value;
        //                //if (dsStorageLocation.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.StorageLocationName.ToString()].ToString() + "'").Length == 1)
        //                //    dr["StorageLocationID"] = dsStorageLocation.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.StorageLocationName.ToString()].ToString() + "'")[0]["StorageLocationID"].ToString();
        //                string storageLocationPlantID = "";
        //                drStorageLocation = dsStorageLocation.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.StorageLocationName.ToString()].ToString() + "'");
        //                if (drStorageLocation.Length > 0)
        //                    storageLocationPlantID = drStorageLocation[0]["PlantID"].ToString();

        //                dr["BINSystemID"] = DBNull.Value;
        //                DataRow[] binNo = dsBinNo.Tables[0].Select("Code='" + dsServerData.Tables[0].Rows[i][colIndex.BinSystemID.ToString()].ToString() + "' AND PlantID='" + storageLocationPlantID + "'");
        //                if (binNo.Length == 1)
        //                    dr["BINSystemID"] = binNo[0]["SystemID"].ToString();

        //                dr["PackingListQuantity"] = bplib.clsWebLib.GetNumData(dsServerData.Tables[0].Rows[i][colIndex.PackingListQuantity.ToString()].ToString());
        //                dr["PackingListQuantityBase"] = baseQuantityPL;


        //                if (dr["FLAGReceivedQty"].ToString() != "YES" && dr["IsIssued"].ToString().ToUpper() != "YES"
        //                    && dr["IsLeftOverStock"].ToString() != "YES" && dr["isDisposed"].ToString().ToUpper() != "YES")
        //                {
        //                    dr["ReceivedQuantity"] = bplib.clsWebLib.GetNumData(dsServerData.Tables[0].Rows[i][colIndex.PackingListQuantity.ToString()].ToString());
        //                    dr["ReceivedQuantityBase"] = baseQuantityPL;

        //                    dr["BalanceQuantityReceived"] = bplib.clsWebLib.GetNumData(dr["ReceivedQuantity"].ToString());
        //                    dr["BalanceQuantityReceivedBase"] = bplib.clsWebLib.GetNumData(dr["ReceivedQuantityBase"].ToString());

        //                }
        //                dr["Remarks"] = bplib.clsWebLib.RetValidLen(dsServerData.Tables[0].Rows[i][colIndex.Remarks.ToString()].ToString());

        //                dr.EndEdit();


        //            }
        //        }


        //        dvLocal = new DataView(dsAddItem.Tables[0].DefaultView.Table);

        //        for (int i = 0; i < dsGRNSKU.Tables[0].Rows.Count; i++)
        //        {
        //            dvLocal.RowFilter = "GRNSKUSystemID='" + dsGRNSKU.Tables[0].Rows[i]["SystemID"].ToString() + "'";

        //            dr = dsGRNSKU.Tables[0].Rows[i];
        //            dr.BeginEdit();

        //            dr["numberOfPackages"] = DBNull.Value;

        //            if (dvLocal.Count > 0)
        //            {
        //                dr["numberOfPackages"] = bplib.clsWebLib.GetNumData(dvLocal.Count.ToString());

        //            }
        //            dr.EndEdit();
        //        }

        //        ////update quantity to SKU table
        //        //for (int i = 0; i < dsGRNSKU.Tables[0].Rows.Count; i++)
        //        //{

        //        //    dr = dsGRNSKU.Tables[0].Rows[i];
        //        //    dr.BeginEdit();
        //        //    dr["PackingListQuantity"] = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsAddItem.Tables[0].Compute("SUM(PackingListQuantity)", "GRNSKUSystemID='" + dsGRNSKU.Tables[0].Rows[i]["SystemID"].ToString() + "'").ToString()));
        //        //    dr["PackingListQuantityBase"] = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsAddItem.Tables[0].Compute("SUM(PackingListQuantityBase)", "GRNSKUSystemID='" + dsGRNSKU.Tables[0].Rows[i]["SystemID"].ToString() + "'").ToString()));

        //        //    dr["ReceivedQuantity"] = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsAddItem.Tables[0].Compute("SUM(PackingListQuantity)", "GRNSKUSystemID='" + dsGRNSKU.Tables[0].Rows[i]["SystemID"].ToString() + "'").ToString()));
        //        //    dr["ReceivedQuantityBase"] = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsAddItem.Tables[0].Compute("SUM(ReceivedQuantityBase)", "GRNSKUSystemID='" + dsGRNSKU.Tables[0].Rows[i]["SystemID"].ToString() + "'").ToString()));
        //        //    dr.EndEdit();

        //        //}

        //        ////update quantity to material level
        //        //DataSet dsMaterial = null;
        //        //objStatic.GetGRNMaterialBySystemID(dsServerData.Tables[0].Rows[0][colIndex.GRNMaterialSystemID.ToString()].ToString(), out dsMaterial);
        //        //for (int i = 0; i < dsMaterial.Tables[0].Rows.Count; i++)
        //        //{
        //        //    dr = dsMaterial.Tables[0].Rows[i];
        //        //    dr.BeginEdit();
        //        //    dr["PackingListQuantity"] = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsGRNSKU.Tables[0].Compute("SUM(PackingListQuantity)", "GRNMaterialSystemID='" + dsMaterial.Tables[0].Rows[i]["SystemID"].ToString() + "'").ToString()));
        //        //    dr["PackingListQuantityBase"] = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsGRNSKU.Tables[0].Compute("SUM(PackingListQuantityBase)", "GRNMaterialSystemID='" + dsMaterial.Tables[0].Rows[i]["SystemID"].ToString() + "'").ToString()));

        //        //    dr["ReceivedQuantity"] = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsGRNSKU.Tables[0].Compute("SUM(ReceivedQuantity)", "GRNMaterialSystemID='" + dsMaterial.Tables[0].Rows[i]["SystemID"].ToString() + "'").ToString()));
        //        //    dr["ReceivedQuantityBase"] = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsGRNSKU.Tables[0].Compute("SUM(ReceivedQuantityBase)", "GRNMaterialSystemID='" + dsMaterial.Tables[0].Rows[i]["SystemID"].ToString() + "'").ToString()));
        //        //    dr.EndEdit();

        //        //}





        //        //objStatic.SaveDataSets(dsAddItem, dsMaterial, dsGRNSKU);

        //        objStatic.SaveDataSets(dsAddItem, dsGRNSKU);



        //        string GRNMasterSystemID = lblGRNSystemID.Text;
        //        clearFormMain("EDIT");
        //        loadGRN(GRNMasterSystemID);

        //        string successLog = updatedRollCount.ToString("F0") + "/" + dsServerData.Tables[0].Rows.Count.ToString() + " Updated successfully.\r\nMaterial Desc: " + dsServerDataHeader.Tables[0].Rows[0][colIndex.MaterialDesc.ToString()].ToString();
        //        ShowLog("Data uploaded successfully!!!");
        //        displayMsgs(successLog, "OK", "Save");




        //    }
        //    catch (Exception ex)
        //    {
        //        displayMsgs(ex.Message, "ERROR", "Save");
        //    }

        //}
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
