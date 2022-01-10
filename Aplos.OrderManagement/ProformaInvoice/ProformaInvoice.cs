using Library.Core;
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
namespace Library.OrderManagement.ProformaInvoice
{
    public class ProformaInvoice
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        Dictionary<string, List<Factors>> MaterialGroupMasterUOMList = new Dictionary<string, List<Factors>>();

        public ProformaInvoice()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }
        DataSet dsConversion;
        public string Save(Dictionary<string, object> PIPackingListMasterData, Dictionary<string, object> MaterialData, List<Dictionary<string, object>> DataList)
        {
            try
            {
                if (DataList != null)
                {
                    for (int i = 0; i < DataList.Count; i++)
                    {
                        if (clsStaticInfo.dbl(DataList[i]["DistributeQTY"]) <= 0)
                        {
                            throw new Exception("Quantity is missing");
                        }
                    }
                }

                ConnectionManager.DAL.ConManager conPIMaster = new ConnectionManager.DAL.ConManager("1");
                conPIMaster.OpenDataSetThroughAdapter("SELECT * FROM PIPackingListMaster where Id='" + PIPackingListMasterData["Id"] + "'", out DataSet dsMaster, false, "1");
                string _Id = "";
                string PIPackingListID = "";
                string PIPackingListMaterialID = "";

                ConnectionManager.DAL.ConManager conPIVersion = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("PIPackingListMaster", out _Id);
                    _Id = "PPL" + "-" + _Id;
                    PIPackingListMasterData["Id"] = _Id;
                    //PIPackingListMasterData["Id"] = _Id;
                    PIPackingListID = PIPackingListMasterData["Id"].ToString();
                    AddNewRow(dsMaster.Tables[0], PIPackingListMasterData);
                    dsMaster.Tables[0].Rows[0]["PImasterId"] = MaterialData["PIMasterId"];
                   
                }
                else
                {
                    //PIMasterId = PIPackingListMasterData["Id"].ToString();
                    _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], PIPackingListMasterData);
                    dsMaster.Tables[0].Rows[0]["Id"] = _Id;
                }

                ConnectionManager.DAL.ConManager conPIMaterial = new ConnectionManager.DAL.ConManager("1");
                conPIMaterial.OpenDataSetThroughAdapter("SELECT * FROM PIPackingListMaterial where PIPackingListMasterId='" + _Id + "' AND PIMaterialId='" + MaterialData["Id"] + "' ", out DataSet dsMaterial, false, "1");
                string _IdM = "";
                #region data update
                if (dsMaterial.Tables[0].Rows.Count == 0)
                {

                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("PIPackingListMaterial", out _IdM);
                    _IdM = "PLM" + "-" + _IdM;


                    AddNewRow(dsMaterial.Tables[0], MaterialData);
                    dsMaterial.Tables[0].Rows[0]["Id"] = _IdM;
                    dsMaterial.Tables[0].Rows[0]["PIQuantity"] = MaterialData["AllocatedQty"];
                    dsMaterial.Tables[0].Rows[0]["PIMaterialId"] = MaterialData["Id"];
                    dsMaterial.Tables[0].Rows[0]["PIUoMId"] = MaterialData["UoMId"];
                    dsMaterial.Tables[0].Rows[0]["PIPackingListMasterId"] = _Id;
                }
                else
                {
                    _IdM = dsMaterial.Tables[0].Rows[0]["Id"].ToString();
                    EditRow(dsMaterial.Tables[0].Rows[0], MaterialData);
                    dsMaterial.Tables[0].Rows[0]["PIQuantity"] = MaterialData["AllocatedQty"];
                    dsMaterial.Tables[0].Rows[0]["PIMaterialId"] = MaterialData["Id"];
                    dsMaterial.Tables[0].Rows[0]["PIUoMId"] = MaterialData["UoMId"];
                    dsMaterial.Tables[0].Rows[0]["Id"] = _IdM;
                    dsMaterial.Tables[0].Rows[0]["PIPackingListMasterId"] = _Id;
                }

                ConnectionManager.DAL.ConManager conPIDetail = new ConnectionManager.DAL.ConManager("1");
                conPIDetail.OpenDataSetThroughAdapter("select * from PIPackingListDetail where PIPackingListMasterId='" + _Id + "' AND PIMaterialId='" + MaterialData["Id"] + @"'", out DataSet dsPIDetail, false, "1");

                if (DataList == null || DataList.Count == 0)
                {
                    while (dsPIDetail.Tables[0].DefaultView.Count > 0)
                        dsPIDetail.Tables[0].DefaultView[0].Delete();
                }

                if (DataList != null)
                {
                    GetAllUOMConversionData();
                    for (int i = 0; i < dsPIDetail.Tables[0].Rows.Count; i++)
                    {
                        var item = DataList.Where(x => x["PODetailId"].ToString() == dsPIDetail.Tables[0].Rows[i]["PODetailId"].ToString()).FirstOrDefault();
                        if (item == null || item.Count == 0)
                        {
                            dsPIDetail.Tables[0].Rows[i].Delete();
                        }
                    }
                    foreach (var item in DataList)
                    {
                        GetUOMConversionAtMaterialGroupMasterData(item["MaterialGroupMasterId"].ToString(), out dsConversion);

                        double conversiongroupListData = ConvertUoM(item["MaterialGroupMasterId"].ToString(), item["POUoMId"].ToString(), item["PIUoMId"].ToString(), Convert.ToDouble(item["DistributeQTY"]));
                        decimal BaseQty = Convert.ToDecimal(conversiongroupListData);
                        dsPIDetail.Tables[0].DefaultView.RowFilter = "PODetailId='" + clsStaticInfo.nullrecorder(item["PODetailId"]) + "'";

                        DataView dv = new DataView(dsPIDetail.Tables[0]);
                        dv.RowFilter = "PODetailId='" + clsStaticInfo.nullrecorder(item["PODetailId"]) + "'";
                        if (dv.Count > 0)
                        {
                            //edit

                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            drmo["Quantity"] = clsStaticInfo.dbl(item["DistributeQTY"]);
                            drmo["QuantityAtPIUoM"] = BaseQty;
                            drmo["UpdatedBy"] = identity.Name;
                            drmo["UpdatedDate"] = System.DateTime.Now.ToString();
                            drmo["UpdatedFromIP"] = identity.IPAddress;
                            drmo["PIPackingListMasterId"] = _Id;
                            drmo.EndEdit();

                        }
                        else
                        {
                            string PLDetailId = "";
                            //add new
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("PIPackingListDetail", out PLDetailId);
                            PLDetailId = "PLD" + "-" + PLDetailId;
                            item["Id"] = PLDetailId;
                            AddNewRow(dsPIDetail.Tables[0], item);

                            DataRow drmo = dsPIDetail.Tables[0].Rows[dsPIDetail.Tables[0].Rows.Count - 1];

                            drmo["PIMaterialId"] = MaterialData["Id"];
                            drmo["Quantity"] = clsStaticInfo.dbl(item["DistributeQTY"]);
                            drmo["QuantityAtPIUoM"] = BaseQty;
                            drmo["PIPackingListMasterId"] =_Id;
                            drmo["PIPackingListMaterialId"] = _IdM;

                        }
                    }
                }

                #endregion data update
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsMaterial, dsPIDetail);

                return _Id;
            }
            catch (Exception ex)
            {
                throw ex;
                //return Json(new { Error = true, Message = ex.Message });
            }
            return null;
        }

        public double ConvertUoM(string MaterialGroupMasterId, string FromUOM, string ToUOM, double Value)
        {

            //If source and target uom are same, no need conversion
            if (FromUOM == ToUOM)
                return Value;

            List<Factors> AltUOM = MaterialGroupMasterUOMList[MaterialGroupMasterId].Where(ee => ee.AlternativeUOMId == FromUOM).ToList();

            //means, need to convert the source UOM to target UOM
            if (AltUOM.Count > 0)
            {
                //converting to base
                Value = Value * AltUOM[0].AltToBaseUOMFactor;
                //and if target uom is also base;no need to further conversion
                if (AltUOM[0].BaseUOMId == ToUOM)
                    return Value;//because we have already converted the source value to base UOM. no need to further conversion

                //second step conversion from base value to alternative target value
                AltUOM = MaterialGroupMasterUOMList[MaterialGroupMasterId].Where(ee => ee.AlternativeUOMId == ToUOM).ToList();
                if (AltUOM.Count > 0)
                {
                    //convert base value to alternative uom using basetoaltuomfactor
                    return Value = Value * AltUOM[0].BaseToAltUOMFactor;
                }
                else
                {
                    return 0;
                }
            }

            return 0;
        }
        public void GetUOMConversionAtMaterialGroupMasterData(string MaterialGroupMasterId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT UOM.MaterialGroupMasterId, UOM.AlternativeUOMId, UOM.BaseUOMId,
       CONVERT(decimal(18,8),UOM.AltToBaseUOMFactor) AS AltToBaseUOMFactor,  convert(decimal(18,8),UOM.BaseToAltUOMFactor) AS BaseToAltUOMFactor, 
       UOM.UOMType FROM (
                    SELECT mm.Id AS MaterialGroupMasterId, mm.BaseUOMId AS AlternativeUOMId,mm.BaseUOMId,
                    1 AS AltToBaseUOMFactor,1 AS BaseToAltUOMFactor,
                    'BASE' AS UOMType FROM mst.MaterialGroupMaster AS mm
					WHERE mm.Id='" + MaterialGroupMasterId + @"'
                    UNION ALL
                    SELECT mmau.MaterialGroupMasterId, mmau.AlternativeUOMId, mmau.BaseUOMId,
                    mmau.BaseUOMFactor/mmau.AlternativeUOMFactor AS AltToBaseUOMFactor,mmau.AlternativeUOMFactor/mmau.BaseUOMFactor AS AltToBaseUOMFactor,
                    'ALT' AS UOMType FROM  mst.MaterialGroupAlternativeUoM AS mmau
					WHERE mmau.MaterialGroupMasterId='" + MaterialGroupMasterId + @"'
                    ) AS UOM
                    ORDER BY UOM.MaterialGroupMasterId";

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
        public void GetAllUOMConversionData()
        {
            // _sqlRepository = new SqlRepository();

            MakeMaterialCluster(_sqlRepository.GetModelCollection<Factors>(@"SELECT UOM.MaterialGroupMasterId, UOM.AlternativeUOMId, UOM.BaseUOMId,
       CONVERT(decimal(18,8),UOM.AltToBaseUOMFactor) AS AltToBaseUOMFactor,  convert(decimal(18,8),UOM.BaseToAltUOMFactor) AS BaseToAltUOMFactor, 
       UOM.UOMType FROM (
                    SELECT mm.Id AS MaterialGroupMasterId, mm.BaseUOMId AS AlternativeUOMId,mm.BaseUOMId,
                    1 AS AltToBaseUOMFactor,1 AS BaseToAltUOMFactor,
                    'BASE' AS UOMType FROM mst.MaterialGroupMaster AS mm
                    UNION ALL
                    SELECT mmau.MaterialGroupMasterId, mmau.AlternativeUOMId, mmau.BaseUOMId,
                    mmau.BaseUOMFactor/mmau.AlternativeUOMFactor AS AltToBaseUOMFactor,mmau.AlternativeUOMFactor/mmau.BaseUOMFactor AS AltToBaseUOMFactor,
                    'ALT' AS UOMType FROM  mst.MaterialGroupAlternativeUoM AS mmau
                    ) AS UOM
                    ORDER BY UOM.MaterialGroupMasterId"));
        }
        private void MakeMaterialCluster(List<Factors> UOMData)
        {
            MaterialGroupMasterUOMList = new Dictionary<string, List<Factors>>();
            List<Factors> _list = new List<Factors>();
            string MaterialGroupMasterId = "";
            foreach (Factors item in UOMData)
            {
                if (MaterialGroupMasterId != item.MaterialGroupMasterId)
                {
                    _list = new List<Factors>();
                    MaterialGroupMasterUOMList.Add(item.MaterialGroupMasterId, _list);
                }

                _list.Add(item);

                MaterialGroupMasterId = item.MaterialGroupMasterId;
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


        public IEnumerable<object> GetTermsAndConditionPOPopUp(string TermsAndConditionsPIDetailId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select * from TermsAndConditionsPIDetails where TermsAndConditionsPIChildId='" + TermsAndConditionsPIDetailId + "' order by sequence";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public string DeletePIDetailPopUp(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TermsAndConditionsPIDetails where Id='" + id + "'");

                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }
        public string DeletePITitle(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TermsAndConditionsPIDetails where TermsAndConditionsPIChildid='" + id + "'");
                con.executeQuery("delete from TermsAndConditionsPIChild where id='" + id + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }


        private string PIMasterSql(string PIMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT PM.Id,PM.PINo,PM.RefNo,FORMAT(PM.PIDate,'dd-MMM-yyyy') PIDate,PM.CurrencyId,PM.BuyerId
							,PM.CustomerId,PM.InvoicingByAddress,PM.DeliveryByAddress
							,C.Code Currency,B.UserName Buyer,P.UserName Customer,PM.TermsAndConditionsId,PM.ShippingMark,PV.VersionNo LastVersion
     FROM PIMaster PM 
							LEFT OUTER JOIN SCS.Currency AS c ON C.Id=PM.CurrencyId
							LEFT OUTER JOIN hkp.Buyer AS b ON B.Id=PM.BuyerId
							LEFT OUTER JOIN HKP.Party AS p ON p.Id=PM.CustomerId
							LEFT OUTER JOIN PIVersion AS pv ON PM.Id=pv.PIMasterId and PV.Id=(select top 1 Id from PIVersion where PIMasterId=PM.Id ORDER BY VersionNo DESC)
						WHERE PM.Id='" + PIMasterId + @"'";
        }

        private string PIMaterialSql(string PIMasterId, string PIVersionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT p.Id, p.PIMasterId, p.PIVersionId,
CAST(ROUND(p.Rate, 4) AS DECIMAL(10,4)) Rate,
CAST(ROUND(p.Quantity, 2) AS DECIMAL(10,2)) Quantity,
ROUND(p.Amount, 2) Amount,uom.UserName UoM,
 CONCAT(mgm.UserName,' - ',p.[Description]) [Description],FORMAT(p.DeliveryDate,'dd-MMM-yyyy') DeliveryDate, p.MaterialGroupMasterId,mgm.UserName AS MaterialGroup
							   ,h.Code HSNCode,p.UoMId
						  FROM PIMaterial AS p
						  LEFT JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=p.MaterialGroupMasterId
						  LEFT JOIN hkp.HSNCode AS h ON h.Id=p.HSNCodeId
						  LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=p.UoMId
						WHERE p.PIMasterId='" + PIMasterId + @"' AND p.PIVersionId='" + PIVersionId + @"'";
        }

        private string TCSql(string PIMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT  ROW_NUMBER() OVER(ORDER BY tac.Sequence) RoWNo, P.Id PIMasterId
,tac.Id TermsAndConditionMasterId,tacc.Id TermsAndConditionPIChildId,tacd.id TermsAndConditionPIDetailId,
tacc.Title,tacd.HeaderCaption,tacd.DESCRIPTION
FROM PIMaster AS p
LEFT OUTER JOIN HKP.TermsAndConditions AS tac ON P.TermsAndConditionsId=tac.Id
LEFT OUTER JOIN TermsAndConditionsPIChild AS tacc ON tacc.PIMasterId=p.Id
LEFT OUTER JOIN TermsAndConditionsPIDetails AS tacd ON tacd.TermsAndConditionsPIChildId=tacc.Id
WHERE P.id='" + PIMasterId + @"' Order By tac.Sequence,tacc.Id";
        }
        public void PoformaInvoiceReport(string PIMasterId, string PIVersionId)
        {
            try
            {
                var reportUtility = new ReportUtility();

                string HeaderSql = PIMasterSql(PIMasterId);
                string MaterialSql = PIMaterialSql(PIMasterId, PIVersionId);
                string TermsAndConditionSql = TCSql(PIMasterId);

                //Instantiate the Excel application object
                DataTable dtHeader = _sqlRepository.GetDataTable(HeaderSql);
                DataTable dtMaterial = _sqlRepository.GetDataTable(MaterialSql);
                DataTable dtTermsAndConditions = _sqlRepository.GetDataTable(TermsAndConditionSql);
                if (dtHeader.Rows.Count == 0)
                    throw new Exception("No data found");
                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Proforma Invoice Report";

                int ROW = 6;
                int COL = 1;

                #region Header

                int StartRow = ROW;
                sheet[ROW, COL].Text = "PI No.:";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colPINo = COL;
                ROW++;
                sheet[ROW, COL].Text = "Date :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colDate = COL;
                ROW++;
                sheet[ROW, COL].Text = "PI REF# :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colPIRef = COL;
                ROW++;
                sheet[ROW, COL].Text = "Customer :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colCustomer = COL;
                ROW++;
                sheet[ROW, COL].Text = "Address :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colDeliveryByAddress = COL;
                ROW = StartRow;
                COL = 5;
                sheet[ROW, COL].Text = "Version No.:";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colLastVersion = COL;
                ROW++;
                sheet[ROW, COL].Text = "Currency :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colCurrency = COL;
                ROW++;
                sheet[ROW, COL].Text = "Buyer :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colBuyer = COL;
                ROW++;
                sheet[ROW, COL].Text = "Shipping Mark:";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colShippingMark = COL;
                ROW = StartRow;

                // Headerdata
                ROW = 6;
                sheet[ROW, colPINo + 1].Text = dtHeader.Rows[0]["PINo"].ToString();
                ROW++;

                sheet[ROW, colDate + 1].Text = dtHeader.Rows[0]["PIDate"].ToString();
                ROW++;

                sheet[ROW, colPIRef + 1].Text = dtHeader.Rows[0]["RefNo"].ToString();
                ROW++;

                sheet[ROW, colCustomer + 1].Text = dtHeader.Rows[0]["Customer"].ToString();
                ROW++;

                sheet[ROW, colDeliveryByAddress + 1].Text = dtHeader.Rows[0]["DeliveryByAddress"].ToString(); ;
                ROW = StartRow;

                sheet[ROW, colLastVersion + 1].Text = dtHeader.Rows[0]["LastVersion"].ToString();
                ROW++;

                sheet[ROW, colCurrency + 1].Text = dtHeader.Rows[0]["Currency"].ToString();
                ROW++;
                sheet[ROW, colBuyer + 1].Text = dtHeader.Rows[0]["Buyer"].ToString();

                ROW++;
                sheet[ROW, colShippingMark + 1].Text = dtHeader.Rows[0]["ShippingMark"].ToString();
                ROW = StartRow;
                // sheet[ROW, colBankCurrency + 1].Text = dtBank.Rows[0]["CurrencyCode"].ToString();

                sheet.Range[StartRow, colPINo + 1, StartRow, colPINo + 3].Merge();
                sheet.Range[StartRow + 1, colDate + 1, StartRow + 1, colDate + 3].Merge();
                sheet.Range[StartRow + 2, colPIRef + 1, StartRow + 2, colPIRef + 3].Merge();
                sheet.Range[StartRow, colLastVersion + 1, StartRow, colLastVersion + 2].Merge();
                sheet.Range[StartRow + 1, colCurrency + 1, StartRow + 1, colCurrency + 2].Merge();
                sheet.Range[StartRow + 2, colBuyer + 1, StartRow + 2, colBuyer + 2].Merge();
                sheet.Range[StartRow+3, colCustomer + 1, StartRow+3, colCustomer + 3].Merge();
                sheet.Range[StartRow + 3, colShippingMark + 1, StartRow+3, colShippingMark + 2].Merge();
                sheet.Range[StartRow+4, colDeliveryByAddress + 1, StartRow+4, colDeliveryByAddress + 6].Merge();
                sheet.Range[StartRow, colPINo, 11, colPINo + 6].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(232, 244, 248);

                ROW = 12;
                COL = 1;
                #endregion
                sheet[ROW, COL].Text = "Description";
                sheet[ROW, COL].ColumnWidth = 30;
                int colDescription = COL;
                COL++;
                sheet[ROW, COL].Text = "HSN Code";
                sheet[ROW, COL].ColumnWidth = 12;
                int colHSNCode = COL;
                COL++;
                sheet[ROW, COL].Text = "Qty";
                sheet[ROW, COL].ColumnWidth = 18;
                int colQty = COL;
                COL++;
                sheet[ROW, COL].Text = "UoM";
                sheet[ROW, COL].ColumnWidth = 15;
                int colUoM = COL;
                COL++;
                sheet[ROW, COL].Text = "Delivery Date";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Rate.";
                sheet[ROW, COL].ColumnWidth = 15;
                int colRate = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Amount";
                sheet[ROW, COL].ColumnWidth = 20;
                int colTotalAmount = COL;
                
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                StartRow = ROW; //row 20
                for (int i = 0; i < dtMaterial.Rows.Count; i++)
                {
                    sheet[ROW, colDescription].Text = dtMaterial.Rows[i]["Description"].ToString();
                    sheet[ROW, colHSNCode].Text = dtMaterial.Rows[i]["HSNCode"].ToString();

                    sheet[ROW, colQty].Number =clsStaticInfo.dbl( dtMaterial.Rows[i]["Quantity"].ToString());
                    sheet[ROW, colQty].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet[ROW, colUoM].Text = dtMaterial.Rows[i]["UoM"].ToString();
                    sheet[ROW, colDeliveryDate].Text = dtMaterial.Rows[i]["DeliveryDate"].ToString();

                    sheet[ROW, colRate].Number = clsStaticInfo.dbl(dtMaterial.Rows[i]["Rate"].ToString());
                    sheet[ROW, colRate].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet[ROW, colTotalAmount].Number = clsStaticInfo.dbl(dtMaterial.Rows[i]["Amount"].ToString());
                    sheet[ROW, colTotalAmount].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colQty) + ROW + "*" + reportUtility.GetColumnNameForXls(colRate) + (ROW) + ")";

                    sheet[ROW, colTotalAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }
                sheet[ROW, 1].Text = "Total :";
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                int colTotal = COL;
           
                sheet.Range[ROW, colTotal].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTotalAmount) + StartRow + ":" + reportUtility.GetColumnNameForXls(colTotalAmount) + (ROW - 1) + ")";
                sheet.Range[ROW, colTotal].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet[ROW, colTotal].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[ROW , 1, ROW , colTotal- 1].Merge();

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                int TCStartROW = ROW+2;
                ROW = TCStartROW;
                COL = 1;
                int  sl = 1;
                string CmpTitile ="";
                sheet[ROW, COL].Text = "Terms & Conditions :";
                sheet[ROW, COL].CellStyle.Font.Italic = true;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].CellStyle.Font.Underline = ExcelUnderline.Single;
                ROW++;
                ROW++;
                int TitleStartROW = ROW;
                int colHeaderCaption = 1;
                int colDes = 3;
                for (int i = 0; i < dtTermsAndConditions.Rows.Count; i++)
                {
                    if (dtTermsAndConditions.Rows[i]["TermsAndConditionPIChildId"].ToString() != CmpTitile)
                    {
                        sheet[ROW, COL].Text = dtTermsAndConditions.Rows[i]["Title"].ToString();
                        sheet.Range[ROW, COL, ROW, COL + 3].Merge();
                        sheet[ROW, COL].CellStyle.Font.Bold = true;
                        ROW++;
                        sl = 1;
                    }
                    sheet[ROW, colHeaderCaption].Text = sl + "." + dtTermsAndConditions.Rows[i]["HeaderCaption"].ToString();
                    sheet.Range[ROW, colHeaderCaption, ROW, colHeaderCaption+1].Merge();

                    sheet[ROW, colDes].Text = dtTermsAndConditions.Rows[i]["DESCRIPTION"].ToString();
                    sheet.Range[ROW, colDes, ROW, colDes+1].Merge();
                    sl++;
                    sheet.Range[ROW, colHeaderCaption, ROW, colDes + 1].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, colHeaderCaption, ROW, colDes + 1].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                    CmpTitile = dtTermsAndConditions.Rows[i]["TermsAndConditionPIChildId"].ToString();
                }
                sheet.Range[TitleStartROW, 1, ROW, 4].BorderAround(ExcelLineStyle.Thin);

                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet[ROW, 1].CellStyle.Font.Size = 9;
                sheet[TCStartROW, 1].CellStyle.Font.Size = 12;

                // sheet["A" + StartRow.ToString()].FreezePanes();


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility.PlantHeader(ref sheet, endCol, "Proforma Invoice Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet[ROW, colTotal].HorizontalAlignment = ExcelHAlign.HAlignRight;


                string strFileName = "ProformaInvoiceReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        class Factors : BaseModel
        {

            public string MaterialGroupMasterId { get; set; }
            public string AlternativeUOMId { get; set; }
            public string BaseUOMId { get; set; }
            public double AltToBaseUOMFactor { get; set; }
            public double BaseToAltUOMFactor { get; set; }
            public string UOMType { get; set; }

           
        }
    }
}
