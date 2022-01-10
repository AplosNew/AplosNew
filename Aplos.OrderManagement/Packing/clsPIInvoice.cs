using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Taxations;
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
                                	SELECT IPL.CommercialInvoiceMasterId Id,plm.[Description],plm.Remarks
                                		,plm.Id PackingId
                                		,p.UserName Customer
                                		,p.id CustomerId
                                		,c.Code Currency
                                		,c.Id CurrencyId
                                		,FORMAT(plm.AddedDate, 'dd-MMM-yyyy') AddedDate
                                        ,e.UserName Entity
                                        ,pm.RefNo
                                        ,B.UserName Buyer
                                        ,PM.ShippingMark
                                        ,PM.InvoicingByAddress
                                        ,PM.DeliveryByAddress
                                        ,FORMAT(PM.PIDate,'dd-MMM-yyyy')PIDate,pm.PINo
                                	FROM PIPackingListMaster AS plm
                                	LEFT JOIN PIMaster AS pm ON pm.Id = plm.PImasterId
                                	LEFT JOIN hkp.Party p ON p.Id = pm.CustomerId
                                	LEFT JOIN [SCS].[Currency] AS C ON C.Id = pm.CurrencyId
                                	LEFT JOIN CommercialInvoicePackingList IPL ON IPL.PIPackingListMasterId = PLM.Id
                                    LEFT JOIN ORG.Entity AS e ON e.Id=plm.EntityId
									LEFT OUTER JOIN hkp.Buyer AS b ON B.Id=PM.BuyerId
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
                                	SELECT IPL.CommercialInvoiceMasterId ,IPL.Id,plm.[Description],plm.Remarks
                                		,plm.Id PackingId
                                		,p.UserName Customer
                                		,p.id CustomerId
                                		,c.Code Currency
                                		,c.Id CurrencyId,e.UserName Entity
                                		,FORMAT(plm.AddedDate, 'dd-MMM-yyyy') AddedDate
                                	FROM PIPackingListMaster AS plm
                                	LEFT JOIN PIMaster AS pm ON pm.Id = plm.PImasterId
                                	LEFT JOIN hkp.Party p ON p.Id = pm.CustomerId
                                	LEFT JOIN [SCS].[Currency] AS C ON C.Id = pm.CurrencyId
                                	LEFT JOIN CommercialInvoicePackingList IPL ON IPL.PIPackingListMasterId = PLM.Id
                                    LEFT JOIN ORG.Entity AS e ON e.Id=plm.EntityId
                                	) d
                                WHERE d.CommercialInvoiceMasterId = '" + CommercialInvoiceMasterId + @"' ";

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
                                    ,s.EXPFromNo,s.BLNumber
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
                                        	,M.PIQuantity Quantity
                                        	,uom.UserName AS UOM
                                        	,p.Rate
                                        	,(M.PIQuantity*p.Rate) Amount
                                        	,(M.PIQuantity*p.Rate) NetAmount
                                            ,SUM(cit.Amount)TaxAmount
                                            ,p.HSNCodeId
                                            ,c.Id
                                            ,c.CommercialInvoiceMasterId
                                        FROM PIPackingListMaster AS PM
                                        INNER JOIN PIPackingListMaterial AS M ON pm.Id = m.PIPackingListMasterId
                                        INNER JOIN PIMaterial AS p ON p.Id = m.PIMaterialId
                                        INNER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id = p.MaterialGroupMasterId
                                        INNER JOIN scs.UnitOfMeasurement AS uom ON uom.Id = p.UoMId
                                        LEFT JOIN CommercialInvoicePIMaterial c ON c.PIPackingListMaterialId=m.Id
                                        LEFT JOIN CommercialInvoiceTaxes AS cit ON cit.CommercialInvoicePIMaterialId=c.Id
                                        WHERE PM.Id " + PackingId + @" 
                                        GROUP BY M.Id,p.Id,MGM.UserName,p.[Description],p.DeliveryDate,M.PIQuantity,uom.UserName 
                                        ,p.Rate,p.Amount,p.HSNCodeId,c.Id,c.CommercialInvoiceMasterId";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string PODate, string Id)
        {
            try
            {
                var sql = "";
                if (string.IsNullOrEmpty(Id) || Id == "null")
                {
                    sql = @"DECLARE @receiveId varchar(100)='" + receiveId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id WHERE PP.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id WHERE PP.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT ct.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, HP.[Percentage] AS [Percentage], NULL TotalAmount
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId AND convert(DATE, EffectiveDate)<='" + PODate + @"') AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    LEFT JOIN CommercialInvoiceTaxes ct ON ct.TaxCategoryId = HP.TaxCategoryId AND ct.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticSales + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasSales + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                }
                else
                {
                    sql = @"SELECT HN.Code HSNCode
                                	,t.HSNCodeId
                                	,t.Id
                                	,t.Percentage
                                	,t.TaxCategoryId
                                	,0 TotalAmount
                                	,TC.UserName
                                	,t.CommercialInvoicePIMaterialId
                                FROM CommercialInvoiceTaxes t
                                LEFT JOIN CommercialInvoicePIMaterial AS M ON M.Id = t.CommercialInvoicePIMaterialId
                                LEFT JOIN [HKP].[HSNCode] AS HN ON HN.Id = t.HSNCodeId
                                JOIN [MST].[TaxCategory] AS TC ON TC.Id = T.TaxCategoryId
                                WHERE M.Id = '" + Id + @"'";
                }

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Dictionary<string, object>> GetSalesTaxData(string companyGroupId, string companyId, string plantId, string salesId)
        {
            var cmdText = @"SELECT c.Id,c.CommercialInvoiceMasterId,c.ServiceMasterId,sm.UserName ChargeName ,c.Amount,c.TaxAmount,(c.Amount+c.TaxAmount)TaxAndTotal,c.Amount TransactionAmount,c.Amount NetAmount
                                FROM  CommercialInvoiceCharges c 
                                LEFT JOIN HKP.ServiceMaster AS sm ON sm.Id=c.ServiceMasterId
								WHERE c.CommercialInvoiceMasterId='" + salesId + @"' ";
            return _sqlRepository.GetDataCollection(cmdText);
        }
        public List<Dictionary<string, object>> GetSalesServiceTaxData(string companyGroupId, string companyId, string plantId, string salesId)
        {
            var cmdText = @"SELECT ST.Id, ST.CommercialInvoiceMasterId, ST.CommercialInvoiceChargesId, ST.TaxCategoryId, TC.UserName AS TaxCategory,ST.HSNCodeId, HC.Code HSNCode, ST.[Percentage], ST.Amount
								FROM CommercialInvoiceTaxes AS ST 
								LEFT JOIN CommercialInvoiceCharges AS SM ON SM.Id=ST.CommercialInvoiceChargesId
								LEFT JOIN CommercialInvoiceMaster AS SA ON SA.Id=ST.CommercialInvoiceMasterId
								LEFT JOIN MST.TaxCategory AS TC ON TC.Id=ST.TaxCategoryId
								LEFT JOIN HKP.HSNCode AS HC ON HC.Id=ST.HSNCodeId
								WHERE SA.CompanyGroupId='" + companyGroupId + "' AND SA.CompanyId='" + companyId + "' AND SA.PlantId='" + plantId + "' AND ST.CommercialInvoiceChargesId in (" + salesId + @")";
            return _sqlRepository.GetDataCollection(cmdText);
        }
        public List<Dictionary<string, object>> GetPackingSalesMaterialData(string companyGroupId, string companyId, string plantId, string salesId)
        {

            var cmdText = @"SELECT SM.*,  MGM.UserName AS MaterialGroupMasterName,MM.UserName MaterialMasterName,ART.StandardName AS MaterialMasterArticleName
            , BUoM.UserName AS BaseUoM, TUoM.UserName AS TransactionUoM
            , CU.Code AS Currency,NULL TaxList ,FC.ValueFreeText,FCV.UserName AS [FreeText] 
            , SCV.UserName AS SecondCharacteristicsValue,TCV.UserName AS ThirdCharacteristicsValue

			,FC.Id FirstCharacteristicsId
			,FC.CharacteristicsValueId FirstCharacteristicsValueId
			,CH.UserName FCH,FCV.UserName SKU1

			,CH2.UserName SCH,SCV.UserName SKU2
			,SC.Id SecondCharacteristicsId
            ,SC.CharacteristicsValueId SecondCharacteristicsValueId

			,CH3.UserName TCH,TCV.UserName SKU3
			,TC.Id ThirdCharacteristicsId
			,TC.CharacteristicsValueId ThirdCharacteristicsValueId
            ,MO.Id MasterOrderId,SO.Id SONo,po.PONumber, FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate,DT.UserName DestinationName
			, SO.SOType,SO.Rate
           ,SM.TransactionQty SalesQty
                ,SM.TransactionQty 
                ,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesService] WHERE SalesId=SA.Id)/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount
	           ,ServiceTax=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesTax] WHERE SalesId=SA.Id  AND SalesServiceId<>'')/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount
            FROM TRN.SalesMaterial AS SM 
            LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId
            LEFT JOIN [TRN].[SalesOrder] AS SO ON SM.SalesOrderId=SO.Id
            JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
			JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
			LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
			LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId

            LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
            LEFT JOIN MST.MaterialMasterArticle AS ART ON SM.ArticleId=ART.Id
            LEFT JOIN TRN.FirstCharacteristics AS FC ON FC.Id=SM.FirstCharacteristicsId AND SM.SalesOrderId=FC.SalesOrderId
            LEFT JOIN HKP.CharacteristicsValue AS FCV ON FCV.Id=SM.FirstCharacteristicsValueId
			LEFT JOIN [HKP].[Characteristics] AS CH ON FC.CharacteristicsId=CH.Id

            LEFT JOIN TRN.SecondCharacteristics AS SC ON SC.Id=SM.SecondCharacteristicsId AND SM.SalesOrderId=SC.SalesOrderId
            LEFT JOIN HKP.CharacteristicsValue AS SCV ON SCV.Id=SM.SecondCharacteristicsValueId
			LEFT JOIN [HKP].[Characteristics] AS CH2 ON SC.CharacteristicsId=CH2.Id

            LEFT JOIN TRN.ThirdCharacteristics AS TC ON TC.Id=SM.ThirdCharacteristicsId AND SM.SalesOrderId=TC.SalesOrderId
            LEFT JOIN HKP.CharacteristicsValue AS TCV ON TCV.Id=SM.ThirdCharacteristicsValueId
			LEFT JOIN [HKP].[Characteristics] AS CH3 ON TC.CharacteristicsId=CH3.Id

            LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
            JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
            WHERE SA.CompanyGroupId='" + companyGroupId + "' AND SA.CompanyId='" + companyId + "' AND SA.PlantId='" + plantId + "' AND SA.Id='" + salesId + "'";

            return _sqlRepository.GetDataCollection(cmdText);
        }

        public void save(Dictionary<string, object> MasterData, List<Dictionary<string, object>> CommercialInvoicePackingList, List<CommercialInvoiceModel> CommercialInvoicePIMaterial, List<Dictionary<string, object>> MaterialtaxList, List<ChargeModel> Charge, List<Dictionary<string, object>> ChargeTax)
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
                    dr["BLNumber"] = MasterData["BLNumber"];
                    dr["BLDate"] = MasterData["BLDate"];
                    dr["EXPFromNo"] = MasterData["EXPFromNo"];
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

                    MasterID = dr["Id"].ToString();
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
                    dr["BLNumber"] = MasterData["BLNumber"];
                    dr["BLDate"] = MasterData["BLDate"];
                    dr["EXPFromNo"] = MasterData["EXPFromNo"];
                    dr["EXPDate"] = MasterData["EXPDate"];
                    //dr["SourceType"] = MasterData["SourceType"];

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
                string sql2 = "SELECT * FROM [dbo].[CommercialInvoicePIMaterial] WHERE CommercialInvoiceMasterId ='" + MasterData["Id"] + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql2, out dsMaterial, false, "1");

                for (int i = 0; i < CommercialInvoicePIMaterial.Count; i++)
                {
                    dsMaterial.Tables[0].DefaultView.RowFilter = "Id = '" + CommercialInvoicePIMaterial[i].Id + "'  ";
                    if (dsMaterial.Tables[0].DefaultView.Count == 0)
                    {
                        dr = dsMaterial.Tables[0].NewRow();

                        dr["Id"] = "C" + TempId + count++;
                        CommercialInvoicePIMaterial[i].Id = dr["Id"].ToString();
                        dr["CommercialInvoiceMasterId"] = MasterID;
                        CommercialInvoicePIMaterial[i].CommercialInvoiceMasterId = dr["CommercialInvoiceMasterId"].ToString();
                        dr["PIPackingListMaterialId"] = CommercialInvoicePIMaterial[i].PIPackingListMaterialId;

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
                        dr["PIPackingListMaterialId"] = CommercialInvoicePIMaterial[i].PIPackingListMaterialId;
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
                if (Charge != null)
                {


                    for (int i = 0; i < Charge.Count; i++)
                    {
                        dsCharges.Tables[0].DefaultView.RowFilter = "Id = '" + Charge[i].Id + "'  ";
                        if (dsCharges.Tables[0].DefaultView.Count == 0)
                        {
                            dr = dsCharges.Tables[0].NewRow();

                            dr["Id"] = "Ch" + TempId + count++;
                            Charge[i].Id = dr["Id"].ToString();
                            dr["CommercialInvoiceMasterId"] = MasterID;
                            Charge[i].CommercialInvoiceMasterId = dr["CommercialInvoiceMasterId"].ToString();
                            dr["ServiceMasterId"] = Charge[i].ServiceMasterId;
                            dr["VoucherDetailId"] = DBNull.Value;
                            dr["Amount"] = Charge[i].Amount;
                            dr["TaxAmount"] = Charge[i].TaxAmount;
                            dr["NetAmount"] = Charge[i].NetAmount;

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
                            dr["ServiceMasterId"] = Charge[i].ServiceMasterId;
                            dr["VoucherDetailId"] = DBNull.Value;
                            dr["Amount"] = Charge[i].Amount;
                            dr["TaxAmount"] = Charge[i].TaxAmount;
                            dr["NetAmount"] = Charge[i].NetAmount;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();
                        }
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
                if (CommercialInvoicePIMaterial != null)
                {
                    foreach (var item in CommercialInvoicePIMaterial)
                    {
                        if (item.TaxList != null)
                        {
                            foreach (var y in item.TaxList)
                            {
                                dsTaxes.Tables[0].DefaultView.RowFilter = "Id = '" + y.Id + "'  ";
                                if (dsTaxes.Tables[0].DefaultView.Count == 0)
                                {
                                    dr = dsTaxes.Tables[0].NewRow();

                                    dr["Id"] = "T" + TempId + count++;
                                    dr["CommercialInvoiceMasterId"] = item.CommercialInvoiceMasterId;
                                    dr["CommercialInvoicePIMaterialId"] = item.Id;
                                    dr["CommercialInvoiceChargesId"] = DBNull.Value;
                                    dr["TaxCategoryId"] = y.TaxCategoryId;
                                    dr["HSNCodeId"] = y.HSNCodeId;
                                    dr["Percentage"] = y.Percentage;
                                    dr["Amount"] = y.TotalAmount;

                                    dr["AddedBy"] = identity.Name;
                                    dr["AddedDate"] = DateTime.Now;
                                    dr["AddedFromIP"] = identity.IPAddress;
                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = DateTime.Now;
                                    dr["UpdatedFromIP"] = identity.IPAddress;
                                    dsTaxes.Tables[0].Rows.Add(dr);

                                }
                                else
                                {
                                    dr = dsTaxes.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();
                                    dr["TaxCategoryId"] = y.TaxCategoryId;
                                    dr["HSNCodeId"] = y.HSNCodeId;
                                    dr["Percentage"] = y.Percentage;
                                    dr["Amount"] = y.TotalAmount;
                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = DateTime.Now;
                                    dr["UpdatedFromIP"] = identity.IPAddress;
                                    dr.EndEdit();
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Charge Tax
                if (Charge != null)
                {
                    foreach (var item in Charge)
                    {
                        if (item.ServiceTaxList != null)
                        {


                            foreach (var x in item.ServiceTaxList)
                            {
                                dsTaxes.Tables[0].DefaultView.RowFilter = "Id = '" + x.Id + "'  ";
                                if (dsTaxes.Tables[0].DefaultView.Count == 0)
                                {
                                    dr = dsTaxes.Tables[0].NewRow();
                                    dr["Id"] = "T" + TempId + count++;
                                    dr["CommercialInvoiceMasterId"] = item.CommercialInvoiceMasterId;
                                    dr["CommercialInvoicePIMaterialId"] = DBNull.Value;
                                    dr["CommercialInvoiceChargesId"] = item.Id;
                                    dr["TaxCategoryId"] = x.TaxCategoryId;
                                    dr["HSNCodeId"] = x.HSNCodeId;
                                    dr["Percentage"] = x.Percentage;
                                    dr["Amount"] = x.Amount;

                                    dr["AddedBy"] = identity.Name;
                                    dr["AddedDate"] = DateTime.Now;
                                    dr["AddedFromIP"] = identity.IPAddress;
                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = DateTime.Now;
                                    dr["UpdatedFromIP"] = identity.IPAddress;
                                    dsTaxes.Tables[0].Rows.Add(dr);

                                }

                                else
                                {
                                    dr = dsTaxes.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();
                                    dr["TaxCategoryId"] = x.TaxCategoryId;
                                    dr["HSNCodeId"] = x.HSNCodeId;
                                    dr["Percentage"] = x.Percentage;
                                    dr["Amount"] = x.Amount;

                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = DateTime.Now;
                                    dr["UpdatedFromIP"] = identity.IPAddress;
                                    dr.EndEdit();
                                }
                            }
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

        public void SaveAdditinalTax(string MasterId, decimal BooksCurrencyBaseRate, OTSBD.IdentityParameter para, List<Dictionary<string, object>> UserSendData)
        {

            try
            {
                string sql = "select * from CommercialInvoiceAdditionalTax where CommercialInvoiceMasterId='" + MasterId + "'";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out DataSet dsDetail, false, "1");

                for (int i = 0; i < UserSendData.Count; i++)
                {
                    dsDetail.Tables[0].DefaultView.RowFilter = "TaxCodeId='" + UserSendData[i]["TaxCodeId"].ToString() + "'";
                    if (dsDetail.Tables[0].DefaultView.Count == 0)
                    {

                        DataRow dr = dsDetail.Tables[0].NewRow();
                        dr["Id"] = GetAddiTaxId();
                        dr["TaxCodeId"] = UserSendData[i]["TaxCodeId"];
                        dr["TaxCategoryId"] = UserSendData[i]["TaxCategoryId"];
                        dr["Percentage"] = UserSendData[i]["ValueOfFixed"];
                        dr["TaxAmount"] = UserSendData[i]["TaxAmount"];
                        dr["BooksCurrencyTaxAmount"] = Math.Round(Convert.ToDecimal(UserSendData[i]["TaxAmount"]) * BooksCurrencyBaseRate, 2);
                        dr["AddedBy"] = para.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = para.AddedFromIP;
                        dr["CommercialInvoiceMasterId"] = MasterId.ToString();
                        dsDetail.Tables[0].Rows.Add(dr);
                    }

                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetAddiTaxId()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "CommercialInvoiceAdditionalTax", out sID);
            return sID;
        }
        public IEnumerable<object> GetAdvanceTaxInfo(string SalesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                sql = @"Select a.Id,a.TaxCodeId,a.Percentage ValueOfFixed,a.TaxAmount,a.AddedBy,a.AddedDate,a.AddedFromIP,b.UserName TaxName,CommercialInvoiceMasterId
						from CommercialInvoiceAdditionalTax a
						left join [mst].[TAXCode] b ON b.Id=a.TaxCodeId where a.CommercialInvoiceMasterId='" + SalesId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}


public class ChargeModel
{
    public string Id { get; set; }
    public string CommercialInvoiceMasterId { get; set; }
    public string ServiceMasterId { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }

    public ICollection<ChargeTaxModel> ServiceTaxList { get; set; }
}
public class ChargeTaxModel
{
    public string Id { get; set; }
    public string CommercialInvoiceMasterId { get; set; }
    public string CommercialInvoiceChargesId { get; set; }
    public string TaxCategoryId { get; set; }
    public string HSNCodeId { get; set; }
    public decimal Percentage { get; set; }
    public decimal Amount { get; set; }
}

public class CommercialInvoiceModel
{
    public string Id { get; set; }
    public string CommercialInvoiceMasterId { get; set; }
    public string PIPackingListMaterialId { get; set; }
    public ICollection<MaterialTaxModel> TaxList { get; set; }
}
public class MaterialTaxModel
{
    public string Id { get; set; }
    public string CommercialInvoiceMasterId { get; set; }
    public string CommercialInvoicePIMaterialId { get; set; }
    public string TaxCategoryId { get; set; }
    public string HSNCodeId { get; set; }
    public decimal Percentage { get; set; }
    public decimal TotalAmount { get; set; }
}
