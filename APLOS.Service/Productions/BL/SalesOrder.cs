using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System;
using System.Data;
using System.Threading;

namespace Library.Service.Productions.BL
{
    internal class SalesOrder
    {
        private readonly ISqlRepository _sqlRepository;

        public SalesOrder()
        {
            _sqlRepository = new SqlRepository();
        }

        public IWorkbook SalesOrder_Report(ref ExcelEngine excelEngine, string masterid, string salesOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataView dvVoucher = null;
            DataTable dtVoucher = null;
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];

                #region data

                DataSet dslocal = GetSalesOrder(salesOrderId);
                dvVoucher = new DataView(dslocal.Tables[0]);
                dtVoucher = dvVoucher.ToTable(true, "Id", "Description", "ResponsiblePerson", "Customer", "FileNo", "Cm", "Plant", "Unit", "SalesGroup", "OrderGrade", "OrderStatus", "Sam", "Currency", "OrderCategory");
                if (dtVoucher.Rows.Count == 0)
                {
                    throw (new Exception("No Sales Order Found !!!"));
                }

                #endregion data

                sheet1.Name = nameof(SalesOrder);
                var _row = 6;
                var _col = 1;
                var _col3 = 3;

                #region Left Header

                oRU.SetMasterHeaderText(ref sheet1, _row, _col, "Id");
                sheet1[oRU.GetColumnNameForXls(_col) + _row + ":" + oRU.GetColumnNameForXls(_col + 1) + _row].Merge();
                oRU.SetText(ref sheet1, _row, _col + 2, dtVoucher.Rows[0]["Id"].ToString()); _row++;
                sheet1[oRU.GetColumnNameForXls(_col3) + _row + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _row].Merge();

                oRU.SetMasterHeaderText(ref sheet1, _row, _col, "Customer");
                sheet1[oRU.GetColumnNameForXls(_col) + _row + ":" + oRU.GetColumnNameForXls(_col + 1) + _row].Merge();
                oRU.SetText(ref sheet1, _row, _col + 2, dtVoucher.Rows[0]["Customer"].ToString()); _row++;
                sheet1[oRU.GetColumnNameForXls(_col3) + _row + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _row].Merge();

                oRU.SetMasterHeaderText(ref sheet1, _row, _col, "Sales Order");
                sheet1[oRU.GetColumnNameForXls(_col) + _row + ":" + oRU.GetColumnNameForXls(_col + 1) + _row].Merge();
                oRU.SetText(ref sheet1, _row, _col + 2, dtVoucher.Rows[0]["Description"].ToString()); _row++;
                sheet1[oRU.GetColumnNameForXls(_col3) + _row + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _row].Merge();

                oRU.SetMasterHeaderText(ref sheet1, _row, _col, "Fill No");
                sheet1[oRU.GetColumnNameForXls(_col) + _row + ":" + oRU.GetColumnNameForXls(_col + 1) + _row].Merge();
                oRU.SetText(ref sheet1, _row, _col + 2, dtVoucher.Rows[0]["FileNo"].ToString()); _row++;
                sheet1[oRU.GetColumnNameForXls(_col3) + _row + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _row].Merge();

                oRU.SetMasterHeaderText(ref sheet1, _row, _col, "Responsible Person");
                sheet1[oRU.GetColumnNameForXls(_col) + _row + ":" + oRU.GetColumnNameForXls(_col + 1) + _row].Merge();
                oRU.SetText(ref sheet1, _row, _col + 2, dtVoucher.Rows[0]["ResponsiblePerson"].ToString()); _row++;
                sheet1[oRU.GetColumnNameForXls(_col3) + _row + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _row].Merge();

                oRU.SetMasterHeaderText(ref sheet1, _row, _col, "CM");
                sheet1[oRU.GetColumnNameForXls(_col) + _row + ":" + oRU.GetColumnNameForXls(_col + 1) + _row].Merge();
                oRU.SetText(ref sheet1, _row, _col + 2, dtVoucher.Rows[0]["Cm"].ToString()); _row++;
                sheet1[oRU.GetColumnNameForXls(_col3) + _row + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _row].Merge();

                oRU.SetMasterHeaderText(ref sheet1, _row, _col, "Currency");
                sheet1[oRU.GetColumnNameForXls(_col) + _row + ":" + oRU.GetColumnNameForXls(_col + 1) + _row].Merge();
                oRU.SetText(ref sheet1, _row, _col + 2, dtVoucher.Rows[0]["Currency"].ToString()); _row++;

                #endregion Left Header

                var _rowR = 6;
                var _colR = 6;
                var _col8 = 8;

                #region Right Header

                oRU.SetMasterHeaderText(ref sheet1, _rowR, _colR, "Plant");
                sheet1[oRU.GetColumnNameForXls(_colR) + _rowR + ":" + oRU.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                oRU.SetText(ref sheet1, _rowR, _colR + 2, dtVoucher.Rows[0]["Plant"].ToString()); _rowR++;
                sheet1[oRU.GetColumnNameForXls(_col8) + _rowR + ":" + oRU.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

                oRU.SetMasterHeaderText(ref sheet1, _rowR, _colR, "Unit");
                sheet1[oRU.GetColumnNameForXls(_colR) + _rowR + ":" + oRU.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                oRU.SetText(ref sheet1, _rowR, _colR + 2, dtVoucher.Rows[0]["Unit"].ToString()); _rowR++;
                sheet1[oRU.GetColumnNameForXls(_col8) + _rowR + ":" + oRU.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

                oRU.SetMasterHeaderText(ref sheet1, _rowR, _colR, "Sales Group");
                sheet1[oRU.GetColumnNameForXls(_colR) + _rowR + ":" + oRU.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                oRU.SetText(ref sheet1, _rowR, _colR + 2, dtVoucher.Rows[0]["SalesGroup"].ToString()); _rowR++;
                sheet1[oRU.GetColumnNameForXls(_col8) + _rowR + ":" + oRU.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

                oRU.SetMasterHeaderText(ref sheet1, _rowR, _colR, "Order Grade");
                sheet1[oRU.GetColumnNameForXls(_colR) + _rowR + ":" + oRU.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                oRU.SetText(ref sheet1, _rowR, _colR + 2, dtVoucher.Rows[0]["OrderGrade"].ToString()); _rowR++;
                sheet1[oRU.GetColumnNameForXls(_col8) + _rowR + ":" + oRU.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

                oRU.SetMasterHeaderText(ref sheet1, _rowR, _colR, "Order Status");
                sheet1[oRU.GetColumnNameForXls(_colR) + _rowR + ":" + oRU.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                oRU.SetText(ref sheet1, _rowR, _colR + 2, dtVoucher.Rows[0]["OrderStatus"].ToString()); _rowR++;
                sheet1[oRU.GetColumnNameForXls(_col8) + _rowR + ":" + oRU.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

                oRU.SetMasterHeaderText(ref sheet1, _rowR, _colR, "Order Category");
                sheet1[oRU.GetColumnNameForXls(_colR) + _rowR + ":" + oRU.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                oRU.SetText(ref sheet1, _rowR, _colR + 2, dtVoucher.Rows[0]["OrderCategory"].ToString()); _rowR++;
                sheet1[oRU.GetColumnNameForXls(_col8) + _rowR + ":" + oRU.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

                oRU.SetMasterHeaderText(ref sheet1, _rowR, _colR, "SAM");
                sheet1[oRU.GetColumnNameForXls(_colR) + _rowR + ":" + oRU.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                oRU.SetText(ref sheet1, _rowR, _colR + 2, dtVoucher.Rows[0]["Sam"].ToString()); _rowR++;

                #endregion Right Header

                #region List data

                DataSet salesOrderList = GetSalesOrderMaterial(salesOrderId);
                dvVoucher = new DataView(salesOrderList.Tables[0]);
                dtVoucher = dvVoucher.ToTable(true, "Id", "Material", "Port", "Destination", "DeliveryDate", "PONumber", "ShipmentMode", "ExcessPercentage", "OwnShipmentDate", "BuyerStyle", "UOM", "Qty");
                if (dtVoucher.Rows.Count == 0)
                {
                    throw (new Exception("No Sales Order Details Found !!!"));
                }

                #endregion List data

                var _rowL = 15;
                var _colIndex = 1;
                var shet2EndxlsCol = _col;

                #region Body header

                oRU.SetHeaderText(ref sheet1, _rowL, _colIndex, "Id"); _colIndex++;
                oRU.SetHeaderText(ref sheet1, _rowL, _colIndex, "Material", 30); _colIndex++;
                oRU.SetHeaderText(ref sheet1, _rowL, _colIndex, "Port"); _colIndex++;
                oRU.SetHeaderText(ref sheet1, _rowL, _colIndex, "Destination"); _colIndex++;
                oRU.SetHeaderText(ref sheet1, _rowL, _colIndex, "Dlv Date"); _colIndex++;
                oRU.SetHeaderText(ref sheet1, _rowL, _colIndex, "PO Number"); _colIndex++;
                oRU.SetHeaderText(ref sheet1, _rowL, _colIndex, "Ship Mode", 9); _colIndex++;
                oRU.SetHeaderText(ref sheet1, _rowL, _colIndex, "Excess(%)", 10, ExcelHAlign.HAlignRight); _colIndex++;
                oRU.SetHeaderText(ref sheet1, _rowL, _colIndex, "Own Ship Date", 12); _colIndex++;
                oRU.SetHeaderText(ref sheet1, _rowL, _colIndex, "Buyer Style", 10); _colIndex++;
                oRU.SetHeaderText(ref sheet1, _rowL, _colIndex, "UOM"); _colIndex++;
                oRU.SetHeaderText(ref sheet1, _rowL, _colIndex, "Qty", 9, ExcelHAlign.HAlignRight);
                shet2EndxlsCol = 12;

                #endregion Body header

                for (int i = 0; i < dtVoucher.Rows.Count; i++)
                {
                    _rowL++;

                    oRU.SetText(ref sheet1, _rowL, 1, dtVoucher.Rows[i]["Id"].ToString());
                    oRU.SetText(ref sheet1, _rowL, 2, dtVoucher.Rows[i]["Material"].ToString());
                    oRU.SetText(ref sheet1, _rowL, 3, dtVoucher.Rows[i]["Port"].ToString());
                    oRU.SetText(ref sheet1, _rowL, 4, dtVoucher.Rows[i]["Destination"].ToString());
                    oRU.SetText(ref sheet1, _rowL, 5, dtVoucher.Rows[i]["DeliveryDate"].ToString());
                    oRU.SetText(ref sheet1, _rowL, 6, dtVoucher.Rows[i]["PONumber"].ToString());
                    oRU.SetText(ref sheet1, _rowL, 7, dtVoucher.Rows[i]["ShipmentMode"].ToString());
                    oRU.SetText(ref sheet1, _rowL, 8, Convert.ToDouble(dtVoucher.Rows[i]["ExcessPercentage"].ToString()));
                    oRU.SetText(ref sheet1, _rowL, 9, dtVoucher.Rows[i]["OwnShipmentDate"].ToString());
                    oRU.SetText(ref sheet1, _rowL, 10, dtVoucher.Rows[i]["BuyerStyle"].ToString());
                    oRU.SetText(ref sheet1, _rowL, 11, dtVoucher.Rows[i]["UOM"].ToString());
                    oRU.SetText(ref sheet1, _rowL, 12, Convert.ToInt32(dtVoucher.Rows[i]["Qty"].ToString()));
                }

                sheet1.Range[(15), 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyHeader(ref sheet1, shet2EndxlsCol, "Sales Order [" + salesOrderId + "]", identity.CompanyId);
                //oRU.FreezePage(ref sheet, 1, 5);
                oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetSalesOrder(string salesOrderId)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @" SELECT s.Id, S.Description,ISNULL (E.EmployeeName,'') +isnull(PM.Code,'') + isnull(PR.UserName,'') AS ResponsiblePerson, PA.UserName AS Customer,S.FileNo,S.Cm, P.UserName AS Plant,U.UserName AS Unit,SA.UserName AS SalesGroup,S.OrderGrade,S.OrderStatus,S.Sam,C.Name AS Currency,O.UserName AS OrderCategory
                                FROM TRN.SalesOrderMaster AS S
                                LEFT JOIN dbo.EmployeeInformation AS E ON E.SystemId = S.EmployeeId
                                LEFT JOIN [MST].[ManpowerBudget] AS PM ON PM.Id = S.ManpowerBudgetId
                                LEFT JOIN ORG.Position AS PR ON PR.Id = S.PositionId
                                LEFT JOIN HKP.Party AS PA ON PA.Id = S.CustomerId
                                LEFT JOIN ORG.Plant AS P ON P.Id = S.PlantId
                                LEFT JOIN ORG.Unit AS U ON U.Id = S.UnitId
                                LEFT JOIN ORG.SalesGroup AS SA ON SA.Id = S.SalesGroupId
                                LEFT JOIN SCS.Currency AS C ON C.Id = S.CmCurrencyId
                                LEFT JOIN HKP.OrderCategory AS O ON O.Id = S.OrderCategoryId
                                    where S.Id='" + salesOrderId + @"'";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetSalesOrderMaterial(string SalesOrderMasterId)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @" SELECT SM.Id, M.UserName AS Material, P.UserName AS Port
                                        , D.UserName AS Destination, C.PONumber AS PONumber, S.UserName AS ShipmentMode
                                        ,ISNULL(SM.ExcessPercentage,0) as ExcessPercentage, B.UserName AS BuyerStyle, U.UserName AS UOM
                                        ,ISNULL (SM.Qty,0) as Qty
                                        ,Replace(CONVERT(VARCHAR(11), SM.DeliveryDate, 106), ' ', '-') DeliveryDate
                                        ,Replace(CONVERT(VARCHAR(11), SM.OwnShipmentDate, 106), ' ', '-') OwnShipmentDate
                                        FROM TRN.SalesOrderMaterialMaster AS SM
                                        LEFT JOIN MST.MaterialMaster as M ON M.Id=SM.MaterialMasterId
                                        LEFT JOIN MST.Port as P ON P.Id=SM.PortId
                                        LEFT JOIN MST.Destination as D ON D.Id=SM.DestinationId

                                        LEFT JOIN TRN.CustomerPO as C ON C.Id=SM.CustomerPOId
                                        LEFT JOIN MST.ShipMode as S ON S.Id=SM.ShipmentModeId
                                        LEFT JOIN HKP.BuyerStyle as B ON B.Id=SM.BuyerStyleId
                                        LEFT JOIN SCS.UnitOfMeasurement as U ON U.Id=SM.UomId
                                        where SM.SalesOrderMasterId='" + SalesOrderMasterId + @"'";
                //order by sm.DeliveryDate";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}