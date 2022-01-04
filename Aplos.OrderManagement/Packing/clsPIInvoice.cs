using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.OrderManagement.Packing
{
    public class clsPIInvoice
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        public clsPIInvoice()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> GetPackingData()
        {
            try
            {
                var str = @"SELECT *
                                FROM (
                                	SELECT IPL.CommercialInvoiceMasterId Id
                                		,plm.Id PackingId
                                		,p.UserName Customer
                                		,p.id CustomerId
                                		,c.Code Currency
                                		,c.Id CurrencyId
                                		,FORMAT(plm.AddedDate, 'dd-MMM-yyyy') AddedDate
                                        ,e.UserName Entity
                                	FROM PIPackingListMaster AS plm
                                	LEFT JOIN PIMaster AS pm ON pm.Id = plm.PImasterId
                                	LEFT JOIN hkp.Party p ON p.Id = pm.CustomerId
                                	LEFT JOIN [SCS].[Currency] AS C ON C.Id = pm.CurrencyId
                                	LEFT JOIN CommercialInvoicePackingList IPL ON IPL.PIPackingListMasterId = PLM.Id
                                    LEFT JOIN ORG.Entity AS e ON e.Id=plm.EntityId
                                	) d
                                WHERE d.Id IS NULL";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> GetSelectedPackingData(string CommercialInvoiceMasterId)
        {
            try
            {
                var str = @"SELECT *
                                FROM (
                                	SELECT IPL.CommercialInvoiceMasterId Id
                                		,plm.Id PackingId
                                		,p.UserName Customer
                                		,p.id CustomerId
                                		,c.Code Currency
                                		,c.Id CurrencyId
                                		,FORMAT(plm.AddedDate, 'dd-MMM-yyyy') AddedDate
                                	FROM PIPackingListMaster AS plm
                                	LEFT JOIN PIMaster AS pm ON pm.Id = plm.PImasterId
                                	LEFT JOIN hkp.Party p ON p.Id = pm.CustomerId
                                	LEFT JOIN [SCS].[Currency] AS C ON C.Id = pm.CurrencyId
                                	LEFT JOIN CommercialInvoicePackingList IPL ON IPL.PIPackingListMasterId = PLM.Id
                                	) d
                                WHERE d.Id = '" + CommercialInvoiceMasterId + @"' ";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> GetMasterData(string companyGroupId, string companyId, string PlantId)
        {
            try
            {
                var str = @"SELECT S.Id
                                	,S.PartyId
                                	,P.Code AS PartyCode
                                	,P.UserName AS PartyName
                                	,S.CurrencyId
                                	,CO.BaseCurrencyId
                                	,C.Code AS CurrencyCode
                                	,S.DocRefNo
                                	,Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') EntryDate
                                	,Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate
                                	,Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate
                                	,Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
                                	,Replace(CONVERT(VARCHAR(11), S.BaseOnDueDate, 106), ' ', '-') BaseOnDueDate
                                	,S.DeliveryPartyPlantId
                                	,S.InvoicingPartyPlantId AS PartyPlantId
                                	,S.InvoicingPartyPlantId
                                	,S.PaymentTermId
                                	,S.BaseNoOfDays
                                	--,S.BaseOnDueDate
                                	,S.InvoiceNo
                                	,PPI.UserName AS BillTo
                                	,AM.StateId AS InvoicingStateId
                                	,ST.UserName AS InvoicingState
                                	,PPI.GSTIN AS InvoicingGSTIN
                                	,PPD.UserName AS ShipTo
                                	,STD.UserName AS DeliveryState
                                	,PPD.GSTIN AS DeliveryGSTIN
                                	,S.InvoicingByAddress
                                	,S.DeliveryByAddress
                                	,S.MatureDate
                                	,AMP.StateId AS PlantStateId
                                	,S.ComercialInvoiceNo
                                	,S.EXPDate
                                	,S.BLDate
                                	,FORMAT(S.AddedDate, 'dd-MMM-yyyy') AddedDate
                                	,s.AddedBy
                                	,S.AddedFromIP
                                	,FORMAT(S.UpdatedDate, 'dd-MMM-yyyy') UpdatedDate
                                	,s.UpdatedBy
                                	,S.UpdatedFromIP
                                FROM CommercialInvoiceMaster s
                                LEFT JOIN [ORG].[Company] AS CO ON CO.Id = S.CompanyId
                                JOIN [HKP].[Party] AS P ON P.Id = S.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id = S.InvoicingPartyPlantId
                                LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id = PPI.AddressMasterId
                                LEFT JOIN [SCS].[State] AS ST ON ST.Id = AM.StateId
                                LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id = S.DeliveryPartyPlantId
                                LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id = PPD.AddressMasterId
                                LEFT JOIN [SCS].[State] AS STD ON STD.Id = AMD.StateId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id = S.CurrencyId
                                LEFT JOIN [ORG].[Plant] AS PT ON PT.Id = S.PlantId
                                LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id = PT.AddressMasterId
                                WHERE S.CompanyGroupId = '" + companyGroupId + @"'                                
                                    AND S.CompanyId = '" + companyId + @"'                                
                                    AND S.PlantId = '" + PlantId + @"'";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetPackingSOData(string PackingId)
        {
            try
            {
                var _sql = @"SELECT M.Id PIPackingListMaterialId
                                        	,p.Id AS PIMaterialId
                                        	,MGM.UserName AS MaterialGroup
                                        	,p.[Description]
                                        	,FORMAT(p.DeliveryDate, 'dd-MMM-yyyy') DeliveryDate
                                        	,p.Quantity
                                        	,uom.UserName AS UOM
                                        	,p.Rate
                                        	,p.Amount
                                        	,p.Amount NetAmount
                                            ,p.HSNCodeId
                                        FROM PIPackingListMaster AS PM
                                        INNER JOIN PIPackingListMaterial AS M ON pm.Id = m.PIPackingListMasterId
                                        INNER JOIN PIMaterial AS p ON p.Id = m.PIMaterialId
                                        INNER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id = p.MaterialGroupMasterId
                                        INNER JOIN scs.UnitOfMeasurement AS uom ON uom.Id = p.UoMId
                                        WHERE PM.Id " + PackingId + "";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void save(Dictionary<string, object> MasterData, List<Dictionary<string, object>> CommercialInvoicePackingList, List<Dictionary<string, object>> CommercialInvoicePIMaterial, List<Dictionary<string, object>> MaterialtaxList, List<Dictionary<string, object>> Charge, List<Dictionary<string, object>> ChargeTax)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster, dsDetails, dsMaterial, dsTaxes, dsCharges; DataRow dr;
                string MasterID = "";
                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "CommercialInvoiceMaster", out string TempId);
                int count = 0;

                #region Master save

                string sql = "SELECT * FROM [dbo].[CommercialInvoiceMaster] WHERE Id ='" + MasterData["Id"] + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                dsMaster.Tables[0].DefaultView.RowFilter = "Id = '" + MasterData["Id"] + "'  ";
                if (dsMaster.Tables[0].DefaultView.Count == 0)
                {
                    dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "M" + TempId;
                    MasterID = dr["Id"].ToString();
                    MasterData["Id"] = MasterID;
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = identity.PlantId;
                    dr["PartyId"] = MasterData["PartyId"];
                    dr["InvoicingPartyPlantId"] = MasterData["InvoicingPartyPlantId"];
                    dr["CurrencyId"] = MasterData["CurrencyId"];
                    dr["DeliveryPartyPlantId"] = MasterData["DeliveryPartyPlantId"];
                    dr["DocRefNo"] = MasterData["DocRefNo"];
                    dr["EntryDate"] = MasterData["EntryDate"];
                    // dr["InvoiceNo"] = MasterData["InvoiceNo"];//discuss (not present in UI)
                    dr["InvoiceDate"] = MasterData["InvoiceDate"];
                    dr["PaymentTermId"] = MasterData["PaymentTermId"];
                    dr["BaseOnDueDate"] = MasterData["BaseOnDueDate"];
                    dr["BaseNoOfDays"] = MasterData["BaseNoOfDays"];
                    dr["MatureDate"] = MasterData["MatureDate"];
                    dr["InvoicingByAddress"] = MasterData["InvoicingByAddress"];
                    dr["DeliveryByAddress"] = MasterData["DeliveryByAddress"];
                    dr["ComercialInvoiceNo"] = MasterData["ComercialInvoiceNo"];
                    dr["BLDate"] = MasterData["BLDate"];
                    dr["EXPDate"] = MasterData["EXPDate"];
                    dr["SourceType"] = MasterData["SourceType"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);

                }
                else
                {
                    dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["PartyId"] = MasterData["PartyId"];
                    dr["InvoicingPartyPlantId"] = MasterData["InvoicingPartyPlantId"];
                    dr["CurrencyId"] = MasterData["CurrencyId"];
                    dr["DeliveryPartyPlantId"] = MasterData["DeliveryPartyPlantId"];
                    dr["DeliveryPartyPlantId"] = MasterData["DeliveryPartyPlantId"];
                    dr["DocRefNo"] = MasterData["DocRefNo"];
                    dr["EntryDate"] = MasterData["EntryDate"];
                    // dr["InvoiceNo"] = MasterData["InvoiceNo"];//discuss (not present in UI)
                    dr["InvoiceDate"] = MasterData["InvoiceDate"];
                    dr["PaymentTermId"] = MasterData["PaymentTermId"];
                    dr["BaseOnDueDate"] = MasterData["BaseOnDueDate"];
                    dr["BaseNoOfDays"] = MasterData["BaseNoOfDays"];
                    dr["MatureDate"] = MasterData["MatureDate"];
                    dr["InvoicingByAddress"] = MasterData["InvoicingByAddress"];
                    dr["DeliveryByAddress"] = MasterData["DeliveryByAddress"];
                    dr["ComercialInvoiceNo"] = MasterData["ComercialInvoiceNo"];
                    dr["BLDate"] = MasterData["BLDate"];
                    dr["EXPDate"] = MasterData["EXPDate"];
                    dr["SourceType"] = MasterData["SourceType"];

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                }

                #endregion

                #region Commercial Invoice Packing List Save

                dr = null;
                string sql1 = "SELECT * FROM [dbo].[CommercialInvoicePackingList] WHERE CommercialInvoiceMasterId ='" + MasterData["Id"] + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsDetails, false, "1");

                for (int i = 0; i < CommercialInvoicePackingList.Count; i++)
                {
                    dsDetails.Tables[0].DefaultView.RowFilter = "Id = '" + CommercialInvoicePackingList[i]["Id"] + "'  ";
                    if (dsDetails.Tables[0].DefaultView.Count == 0)
                    {
                        dr = dsDetails.Tables[0].NewRow();

                        dr["Id"] = "C" + TempId + count++;
                        dr["CommercialInvoiceMasterId"] = MasterID;
                        dr["PIPackingListMasterId"] = CommercialInvoicePackingList[i]["PackingId"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsDetails.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        dr = dsDetails.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["PIPackingListMasterId"] = CommercialInvoicePackingList[i]["PackingId"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                }

                #endregion

                #region Commercial Invoice PI Material Save

                dr = null;
                string PIMaterial = "";
                string sql2 = "SELECT * FROM [dbo].[CommercialInvoicePIMaterial] WHERE CommercialInvoiceMasterId ='" + MasterData["Id"] + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql2, out dsMaterial, false, "1");

                for (int i = 0; i < CommercialInvoicePIMaterial.Count; i++)
                {
                    dsMaterial.Tables[0].DefaultView.RowFilter = "Id = '" + CommercialInvoicePIMaterial[i]["PIMaterialId"] + "'  ";
                    if (dsMaterial.Tables[0].DefaultView.Count == 0)
                    {
                        dr = dsMaterial.Tables[0].NewRow();

                        dr["Id"] = "C" + TempId + count++;
                        PIMaterial = dr["Id"].ToString();
                        dr["CommercialInvoiceMasterId"] = MasterID;
                        dr["PIPackingListMaterialId"] = CommercialInvoicePIMaterial[i]["PIPackingListMaterialId"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsMaterial.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        dr = dsMaterial.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["PIPackingListMaterialId"] = CommercialInvoicePIMaterial[i]["PIPackingListMaterialId"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                }

                #endregion

                #region Charges Save Part
                dr = null;
                string sql4 = "SELECT * FROM [dbo].[CommercialInvoiceCharges] WHERE CommercialInvoiceMasterId ='" + MasterData["Id"] + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql4, out dsCharges, false, "1");
                count = 0;

                for (int i = 0; i < Charge.Count; i++)
                {
                    dsCharges.Tables[0].DefaultView.RowFilter = "Id = '" + Charge[i]["Id"] + "'  ";
                    if (dsCharges.Tables[0].DefaultView.Count == 0)
                    {
                        dr = dsCharges.Tables[0].NewRow();

                        dr["Id"] = "Ch" + TempId + count++;
                        dr["CommercialInvoiceMasterId"] = MasterID;
                        dr["ServiceMasterId"] = Charge[i]["ServiceMasterId"];
                        dr["VoucherDetailId"] = DBNull.Value;
                        dr["Amount"] = Charge[i]["Amount"];
                        dr["TaxAmount"] = Charge[i]["TaxAmount"];
                        dr["NetAmount"] = Charge[i]["NetAmount"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsCharges.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        dr = dsCharges.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr.EndEdit();
                    }
                }

                #endregion

                #region Tax Save Part

                dr = null;
                string sql3 = "SELECT * FROM [dbo].[CommercialInvoiceTaxes] WHERE CommercialInvoiceMasterId ='" + MasterData["Id"] + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql3, out dsTaxes, false, "1");
                count = 0;

                #region Material Tax Save
                if ( MaterialtaxList != null)
                {
                    for (int i = 0; i < MaterialtaxList.Count; i++)
                    {
                        dsTaxes.Tables[0].DefaultView.RowFilter = "Id = '" + MaterialtaxList[i]["Id"] + "'  ";
                        if (dsTaxes.Tables[0].DefaultView.Count == 0)
                        {
                            if (CommercialInvoicePIMaterial.Count != 0)
                            {
                                for (int j = 0; j < dsMaterial.Tables[0].Rows.Count; j++)
                                {
                                    dr = dsTaxes.Tables[0].NewRow();

                                    dr["Id"] = "T" + TempId + count++;
                                    dr["CommercialInvoiceMasterId"] = MasterID;
                                    dr["CommercialInvoicePIMaterialId"] = dsMaterial.Tables[0].Rows[i]["Id"];
                                    dr["CommercialInvoiceChargesId"] = DBNull.Value;
                                    dr["TaxCategoryId"] = MaterialtaxList[i]["TaxCategoryId"];
                                    dr["HSNCodeId"] = MaterialtaxList[i]["HSNCodeId"];
                                    dr["Percentage"] = MaterialtaxList[i]["Percentage"];
                                    dr["Amount"] = MaterialtaxList[i]["TotalAmount"];

                                    dr["AddedBy"] = identity.Name;
                                    dr["AddedDate"] = DateTime.Now;
                                    dr["AddedFromIP"] = identity.IPAddress;
                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = DateTime.Now;
                                    dr["UpdatedFromIP"] = identity.IPAddress;
                                    dsTaxes.Tables[0].Rows.Add(dr);
                                }
                            }

                        }
                        else
                        {
                            dr = dsTaxes.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            dr.EndEdit();
                        }
                    }
                }
                #endregion

                #region Charge Tax
                if (ChargeTax != null)
                {
                    for (int i = 0; i < ChargeTax.Count; i++)
                    {
                        dsTaxes.Tables[0].DefaultView.RowFilter = "Id = '" + ChargeTax[i]["Id"] + "'  ";
                        if (dsTaxes.Tables[0].DefaultView.Count == 0)
                        {

                            for (int j = 0; j < dsCharges.Tables[0].Rows.Count; j++)
                            {
                                dr = dsTaxes.Tables[0].NewRow();
                                dr["Id"] = "T" + TempId + count++;
                                dr["CommercialInvoiceMasterId"] = MasterID;
                                dr["CommercialInvoicePIMaterialId"] = DBNull.Value;
                                dr["CommercialInvoiceChargesId"] = dsCharges.Tables[0].Rows[j]["Id"];
                                dr["TaxCategoryId"] = ChargeTax[i]["TaxCategoryId"];
                                dr["HSNCodeId"] = ChargeTax[i]["HSNCodeId"];
                                dr["Percentage"] = ChargeTax[i]["Percentage"];
                                dr["Amount"] = ChargeTax[i]["Amount"];

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = DateTime.Now;
                                dr["AddedFromIP"] = identity.IPAddress;
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = DateTime.Now;
                                dr["UpdatedFromIP"] = identity.IPAddress;
                                dsTaxes.Tables[0].Rows.Add(dr);
                            }
                        }

                        else
                        {
                            dr = dsTaxes.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            dr.EndEdit();
                        }
                    }
                }

                #endregion

                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetails, dsMaterial, dsCharges, dsTaxes);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}