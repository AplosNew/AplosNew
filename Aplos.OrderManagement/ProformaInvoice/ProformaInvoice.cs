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
                    dsMaterial.Tables[0].Rows[0]["PIQuantity"] = MaterialData["Quantity"];
                    dsMaterial.Tables[0].Rows[0]["PIMaterialId"] = MaterialData["Id"];
                    dsMaterial.Tables[0].Rows[0]["PIUoMId"] = MaterialData["UoMId"];
                    dsMaterial.Tables[0].Rows[0]["PIPackingListMasterId"] = _Id;
                }
                else
                {
                    _IdM = dsMaterial.Tables[0].Rows[0]["Id"].ToString();
                    EditRow(dsMaterial.Tables[0].Rows[0], MaterialData);
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


        //private string PIMasterSql(string PIMasterId, string PIVersionId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return @"SELECT V.Id AS VoucherId
	       //                                  ,VD.Id AS VoucherDetailId
	       //                                  ,V.VoucherNo
	       //                                  ,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
	       //                                  ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
        //                                     ,VD.DocRefNo, VD.PartyType, VD.Narration
	       //                                  ,GLT.CrAmount AS Amount --[Add : BanK other Credit]
	       //                                  ,'' AS CheckNo
	       //                                  ,'' EncashmentDate
        //                               FROM TRN.VoucherDetail AS VD
        //                               INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
        //                               INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
        //                               WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + BankMasterID + @"' AND (ReconcileId IS NULL))
        //                               AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"'  AND V.IsPark=0
        //                               AND (VD.BankMasterId='" + BankMasterID + @"'  AND V.PostingDate<=CONVERT(DATE,'" + toDate + @"')) --AND V.PostingDate>='" + fromDate + @"'
        //                               AND (VD.CrAmount<>0.0000)";
        //}

        //private string PIMaterialSql(string PIMasterId, string PIVersionId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return @"SELECT V.Id AS VoucherId
	       //                                  ,VD.Id AS VoucherDetailId
	       //                                  ,V.VoucherNo
	       //                                  ,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
	       //                                  ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
        //                                     ,VD.DocRefNo, VD.PartyType, VD.Narration
	       //                                  ,GLT.CrAmount AS Amount --[Add : BanK other Credit]
	       //                                  ,'' AS CheckNo
	       //                                  ,'' EncashmentDate
        //                               FROM TRN.VoucherDetail AS VD
        //                               INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
        //                               INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
        //                               WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + BankMasterID + @"' AND (ReconcileId IS NULL))
        //                               AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"'  AND V.IsPark=0
        //                               AND (VD.BankMasterId='" + BankMasterID + @"'  AND V.PostingDate<=CONVERT(DATE,'" + toDate + @"')) --AND V.PostingDate>='" + fromDate + @"'
        //                               AND (VD.CrAmount<>0.0000)";
        //}
        //public void PoformaInvoiceReport(string PIMasterId, string PIVersionId)
        //{
        //    try
        //    {
        //        string HeaderSql = PIMasterSql(PIMasterId, PIVersionId);
        //        string MaterialSql = PIMaterialSql(PIMasterId, PIVersionId);

        //        //Instantiate the Excel application object
        //        DataTable dtHeader = _sqlRepository.GetDataTable(HeaderSql);
        //        DataTable dtMaterial = _sqlRepository.GetDataTable(MaterialSql);
        //        if (dtHeader.Rows.Count == 0)
        //            throw new Exception("No data found");
        //        ExcelEngine excelEngine = new ExcelEngine();
        //        IApplication application = excelEngine.Excel;

        //        //Set the default application version
        //        application.DefaultVersion = ExcelVersion.Excel2013;
        //        IWorkbook workbook = application.Workbooks.Create(1);
        //        IWorksheet sheet = workbook.Worksheets[0];

        //        sheet.Name = "Cr. Reconcile Pending Report";

        //        int ROW = 6;
        //        int COL = 1;

        //        #region Header

        //        int StartRow = ROW;
        //        sheet[ROW, COL].Text = "Bank :";
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        sheet[ROW, COL].CellStyle.Font.Bold = true;

        //        int colBank = COL;
        //        ROW++;
        //        sheet[ROW, COL].Text = "Branch :";
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        sheet[ROW, COL].CellStyle.Font.Bold = true;
        //        int colBranch = COL;
        //        ROW++;
        //        sheet[ROW, COL].Text = "From Date :";
        //        sheet[ROW, COL].CellStyle.Font.Bold = true;
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colFromDate = COL;
        //        ROW = StartRow;
        //        COL = 4;
        //        sheet[ROW, COL].Text = "Account :";
        //        sheet[ROW, COL].CellStyle.Font.Bold = true;
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colAccount = COL;
        //        ROW++;
        //        sheet[ROW, COL].Text = "Bank GL :";
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colBankGL = COL;
        //        ROW++;
        //        sheet[ROW, COL].Text = "To Date :";
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        sheet[ROW, COL].CellStyle.Font.Bold = true;
        //        int colToDate = COL;
        //        ROW = StartRow;
        //        COL = 7;
        //        sheet[ROW, COL].Text = "Bank Currency :";
        //        sheet[ROW, COL].ColumnWidth = 13;
        //        sheet[ROW, COL].CellStyle.Font.Bold = true;
        //        int colBankCurrency = COL;
        //        // Headerdata
        //        ROW = 6;
        //        sheet[ROW, colBank + 1].Text = dtBank.Rows[0]["BankName"].ToString();
        //        ROW++;
        //        sheet[ROW, colBranch + 1].Text = dtBank.Rows[0]["BankBranchName"].ToString();
        //        ROW++;
        //        sheet[ROW, colFromDate + 1].Text = fromDate;
        //        ROW = StartRow;
        //        sheet[ROW, colAccount + 1].Text = dtBank.Rows[0]["AccountTitle"].ToString();
        //        ROW++;

        //        sheet[ROW, colBankGL + 1].Text = dtBank.Rows[0]["GLGeneralInfoId"].ToString() + "-" + dtBank.Rows[0]["GLGeneralInfoName"].ToString();

        //        ROW++;
        //        sheet[ROW, colToDate + 1].Text = toDate;
        //        ROW = StartRow;
        //        sheet[ROW, colBankCurrency + 1].Text = dtBank.Rows[0]["CurrencyCode"].ToString();

        //        sheet.Range[StartRow, colBank + 1, StartRow, colBank + 2].Merge();
        //        sheet.Range[StartRow + 1, colBranch + 1, StartRow + 1, colBranch + 2].Merge();
        //        sheet.Range[StartRow + 2, colFromDate + 1, StartRow + 2, colFromDate + 2].Merge();
        //        sheet.Range[StartRow, colAccount + 1, StartRow, colAccount + 2].Merge();
        //        sheet.Range[StartRow + 1, colBankGL + 1, StartRow + 1, colBankGL + 2].Merge();
        //        sheet.Range[StartRow + 2, colToDate + 1, StartRow + 2, colToDate + 2].Merge();
        //        sheet.Range[StartRow, colBankCurrency + 1, StartRow, colBankCurrency + 2].Merge();
        //        sheet.Range[StartRow, colBank, StartRow + 3, colBankCurrency + 2].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(232, 244, 248);

        //        ROW = 10;
        //        COL = 1;
        //        #endregion
        //        sheet[ROW, COL].Text = "Id";
        //        sheet[ROW, COL].ColumnWidth = 12;
        //        int colId = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Voucher No";
        //        sheet[ROW, COL].ColumnWidth = 18;
        //        int colVoucherNo = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Voucher Date";
        //        sheet[ROW, COL].ColumnWidth = 15;
        //        int colVoucherDate = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Posting Date";
        //        sheet[ROW, COL].ColumnWidth = 15;
        //        int colPostingDate = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Doc Ref No.";
        //        sheet[ROW, COL].ColumnWidth = 15;
        //        int colDocRefNo = COL;
        //        COL++;

        //        sheet[ROW, COL].Text = "Party Type";
        //        sheet[ROW, COL].ColumnWidth = 20;
        //        int colPartyType = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Narration";
        //        sheet[ROW, COL].ColumnWidth = 15;
        //        int colNarration = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Amount";
        //        sheet[ROW, COL].ColumnWidth = 12;
        //        int colAmount = COL;

        //        COL++;
        //        sheet[ROW, COL].Text = "Check No.";
        //        sheet[ROW, COL].ColumnWidth = 15;
        //        int colCheckNo = COL;


        //        int endCol = COL;
        //        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
        //        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
        //        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
        //        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
        //        ROW++;

        //        StartRow = ROW; //row 20
        //        for (int i = 0; i < dtCRBR.Rows.Count; i++)
        //        {

        //            sheet[ROW, colId].Text = dtCRBR.Rows[i]["VoucherDetailId"].ToString();
        //            sheet[ROW, colVoucherNo].Text = dtCRBR.Rows[i]["VoucherNo"].ToString();
        //            sheet[ROW, colVoucherDate].Text = dtCRBR.Rows[i]["VoucherDate"].ToString();

        //            sheet[ROW, colPostingDate].Text = dtCRBR.Rows[i]["PostingDate"].ToString();
        //            sheet[ROW, colDocRefNo].Text = dtCRBR.Rows[i]["DocRefNo"].ToString();
        //            sheet[ROW, colNarration].Text = dtCRBR.Rows[i]["PartyType"].ToString();
        //            sheet[ROW, colPartyType].Text = dtCRBR.Rows[i]["Narration"].ToString();
        //            sheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtCRBR.Rows[i]["Amount"].ToString());
        //            sheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
        //            sheet[ROW, colCheckNo].Text = dtCRBR.Rows[i]["CheckNo"].ToString();


        //            sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
        //            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

        //            ROW++;

        //        }
        //        sheet[ROW, 1].Text = "Total:";
        //        sheet[ROW, 1].CellStyle.Font.Bold = true;
        //        int colTotal = COL;
        //        var reportUtility = new ReportUtility();
        //        sheet.Range[ROW, colAmount].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colAmount) + StartRow + ":" + reportUtility.GetColumnNameForXls(colAmount) + (ROW - 1) + ")";
        //        sheet.Range[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
        //        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
        //        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
        //        sheet.IsGridLinesVisible = false;
        //        sheet.UsedRange.WrapText = true;
        //        sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
        //        sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
        //        sheet[ROW, 1].CellStyle.Font.Size = 9;

        //        sheet["A" + StartRow.ToString()].FreezePanes();


        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        reportUtility.PlantHeader(ref sheet, endCol, "Cr. Reconcile Pending Report", identity.PlantId);
        //        reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
        //        sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //        sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

        //        string strFileName = "CRReconcilePendingReport.xlsx";
        //        workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
        //        workbook.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}

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
