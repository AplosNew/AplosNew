using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Library.OrderManagement.Sales
{
    public class clsSales
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        #region Constructor
        public clsSales()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();


        }
        #endregion Constructor

        public IEnumerable<object> GetItemSOSKUList(string masterOrderId)
        {
            try
            {
                var sql = @"SELECT  MOI.Id MasterOrderItemId,MOI.MasterOrderId,SO.Id SONo, po.PONumber,PODate=REPLACE(CONVERT(CHAR(11), po.PODate, 106),' ','-'), DeliveryDate = REPLACE(CONVERT(CHAR(11), SO.DeliveryDate, 106),' ','-'),SO.ParentId
                            , SO.DestinationId
							,DT.UserName DestinationName
							,PM.UserName ProductName
                            , SO.ShipmentModeId
							,MOI.MaterialMasterId
							,MM.UserName MaterialMasterName
							,MOI.ArticleId,MOI.BuyerReferenceNo
							,WithSKU=CASE WHEN MM.WithSKU=1 THEN 'Yes' WHEN MM.WithSKU=0 THEN 'No' END
							,MMA.StandardName MaterialMasterArticleName
							,FCH.Id FirstCharacteristicsId
							,FCH.CharacteristicsValueId FirstCharacteristicsValueId
							,CHV.UserName SKU1
							,CHV2.UserName SKU2
                            ,SCH.Id SecondCharacteristicsId
                            ,SCH.CharacteristicsValueId SecondCharacteristicsValueId
							,CHV3.UserName SKU3
                            ,TCH.Id ThirdCharacteristicsId
							,TCH.CharacteristicsValueId ThirdCharacteristicsValueId
							--,FCH.Qty SKU1Qty,SCH.Qty SKU2Qty
							, SO.MasterOrderItemId
                            , MOI.MaterialMasterId
                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), SO.CommitmentDate, 106),' ','-')
                            , SO.CustomerPOId
                            , SO.OrderStatusId, SO.OrderCategoryId
                            , SO.SOType, SO.ResponsiblePersonId
                            ,MO.TotalQtyUOMId BaseUOMId
                            , SO.UpCharge,  SO.Rate, SO.IsFirstEntry,SO.Discount,EMP.EmployeeName ResponsiblePersonName
                            ,FORMAT (SO.LSD, 'dd-MMM-yyyy') as LSD ,FORMAT (SO.MainRawMaterialInhouseDate, 'dd-MMM-yyyy') as MainRawMaterialInhouseDate
                            ,FORMAT (SO.OtherRawMaterialInhouseDate, 'dd-MMM-yyyy') as OtherRawMaterialInhouseDate
                            , hasFirst=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[FirstCharacteristics] WHERE SalesOrderId=SO.Id)
                            
                            ,(SELECT ISNULL(sum(Qty),0) FROM TRN.FirstCharacteristics AS FCS WHERE SO.Id= FCS.SalesOrderId) SKUQty
                            , isTax=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[SalesOrderTax] WHERE SalesOrderId=SO.Id),mm.HSNCodeId,mo.InvoicingPartyPlantId
							, Qty=case when SCH.CharacteristicsValueId<>''  then SCH.Qty
										when FCH.CharacteristicsValueId<>'' then FCH.Qty 
										else SO.Qty end
                            ,0 SalesQty
							, PlanQty	=(SELECT isnull(case when SCH.CharacteristicsValueId<>''  then SCH.Qty
										when FCH.CharacteristicsValueId<>'' then FCH.Qty 
										else 
										SO.Qty 
										end, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))

                            , ExistSalesQty=ISNULL(case when SCH.CharacteristicsValueId<>''  then SCH.SalesQty
										when FCH.CharacteristicsValueId<>'' then FCH.SalesQty end,
										SM.TransactionQty)
							,Balance=(SELECT isnull(case when SCH.CharacteristicsValueId<>'' then SCH.Qty
										when FCH.CharacteristicsValueId<>'' then FCH.Qty 
										else SO.Qty end, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))-ISNULL(case when SCH.CharacteristicsValueId<>'' then SCH.SalesQty
										when FCH.CharacteristicsValueId<>'' then FCH.SalesQty end,SM.TransactionQty)

                    FROM [TRN].[SalesOrder] AS SO
                    JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
                    JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id
                    JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
                    LEFT JOIN [MST].[MaterialMasterArticle] AS MMA ON MOI.ArticleId = MMA.Id
                    LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                    LEFT JOIN dbo.EmployeeInformation AS EMP ON EMP.SystemId = SO.ResponsiblePersonId
					LEFT JOIN [TRN].[FirstCharacteristics] AS FCH ON FCH.SalesOrderId=SO.Id
                      LEFT  JOIN [HKP].[Characteristics] AS CH ON FCH.CharacteristicsId=CH.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV ON FCH.CharacteristicsValueId=CHV.Id
					LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId

						LEFT JOIN [TRN].[SecondCharacteristics] AS SCH ON SO.Id=SCH.SalesOrderId 
						AND FCH.Id=SCH.FirstCharacteristicsId AND ISNULL(SCH.Qty,0) >0
                        LEFT JOIN [HKP].[Characteristics] AS CH2 ON SCH.CharacteristicsId=CH2.Id
                       LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON SCH.CharacteristicsValueId=CHV2.Id

					   LEFT JOIN [TRN].[ThirdCharacteristics] AS TCH ON    SO.Id=TCH.SalesOrderId 
					   LEFT JOIN [HKP].[Characteristics] AS CH3 ON TCH.CharacteristicsId=CH3.Id
                       LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON TCH.CharacteristicsValueId=CHV3.Id

					   LEFT JOIN TRN.ProductDefinition AS PD ON PD.MaterialMasterId=MOI.MaterialMasterId
					   LEFT JOIN MST.ProductMaster AS PM ON PM.Id=PD.ProductMasterId
                     LEFT JOIN (SELECT SUM(TransactionQty) TransactionQty,SalesOrderId,FirstCharacteristicsValueId,SecondCharacteristicsValueId FROM TRN.SalesMaterial GROUP BY  SalesOrderId,FirstCharacteristicsValueId,SecondCharacteristicsValueId) SM ON SM.SalesOrderId=SO.Id
					 AND SM.FirstCharacteristicsValueId=FCH.CharacteristicsValueId AND SM.SecondCharacteristicsValueId=SCH.CharacteristicsValueId
                    WHERE MOI.Id " + masterOrderId + " ORDER BY SO.DeliveryDate";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> GetMasterOrderSalesMaterialData(string companyGroupId, string companyId, string plantId, string salesId)
        {

            try
            {
                var cmdText = @"SELECT SM.*,  MGM.UserName AS MaterialGroupMasterName,MM.UserName MaterialMasterName,ART.StandardName AS MaterialMasterArticleName,MOI.BuyerReferenceNo,PO.PONumber
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
           ,0 SalesQty
          --   ,Balance=SM.TransactionQty-ISNULL(case when SC.CharacteristicsValueId<>''  then SC.SalesQty
							--			when FC.CharacteristicsValueId<>'' then FC.SalesQty else SO.Qty 
							--end,0)
       --    ,ExistSalesQty=ISNULL(case when SC.CharacteristicsValueId<>''  then SC.SalesQty
							--			when FC.CharacteristicsValueId<>'' then FC.SalesQty else SO.Qty 
							--end,0)
							, ExistSalesQty=ISNULL(case when SC.CharacteristicsValueId<>''  then SC.SalesQty
										when FC.CharacteristicsValueId<>'' then FC.SalesQty end,
										SM.TransactionQty)
							,Balance=(SELECT isnull(case when SC.CharacteristicsValueId<>'' then SC.Qty
										when FC.CharacteristicsValueId<>'' then FC.Qty 
										else SO.Qty end, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))-ISNULL(case when SC.CharacteristicsValueId<>'' then SC.SalesQty
										when FC.CharacteristicsValueId<>'' then FC.SalesQty end,SM.TransactionQty)
                ,SM.TransactionQty TempSalesQty
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
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public List<Dictionary<string, object>> GetSalesMaterialData(string companyGroupId, string companyId, string plantId, string salesId)
        {
            var cmdText = @"SELECT SM.Id, SM.SalesId, MGM.UserName AS MaterialGroupMasterName, SM.MaterialMasterId, MM.UserName MaterialMasterName, SM.ArticleId, ART.StandardName AS ArticleName
            , SM.TransactionQty,BUoM.UserName AS BaseUoM, SM.BaseUOMId, SM.TransactionUoMId, TUoM.UserName AS TransactionUoM, SM.TransactionRate
            , CU.Code AS Currency, SM.TransactionAmount, SM.TaxAmount, SM.NetAmount, NULL TaxList ,FC.ValueFreeText,FCV.UserName AS [FreeText] 
            , SCV.UserName AS SecondCharacteristicsValue,TCV.UserName AS ThirdCharacteristicsValue 
            FROM TRN.SalesMaterial AS SM 
            LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId
            LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
            LEFT JOIN MST.MaterialMasterArticle AS ART ON SM.ArticleId=ART.Id
            LEFT JOIN TRN.FirstCharacteristics AS FC ON FC.Id=SM.FirstCharacteristicsId
            LEFT JOIN HKP.CharacteristicsValue AS FCV ON FCV.Id=SM.FirstCharacteristicsValueId
            LEFT JOIN TRN.SecondCharacteristics AS SC ON SC.Id=SM.SecondCharacteristicsId
            LEFT JOIN HKP.CharacteristicsValue AS SCV ON SCV.Id=SM.SecondCharacteristicsId
            LEFT JOIN TRN.ThirdCharacteristics AS TC ON TC.Id=SM.ThirdCharacteristicsId
            LEFT JOIN HKP.CharacteristicsValue AS TCV ON TCV.Id=SM.ThirdCharacteristicsValueId
            LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
            JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
            WHERE SA.CompanyGroupId='" + companyGroupId + "' AND SA.CompanyId='" + companyId + "' AND SA.PlantId='" + plantId + "' AND SA.Id='" + salesId + @"'";

            return _sqlRepository.GetDataCollection(cmdText);
        }

        private string GetAddiTaxId()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SalesAdditionalTax", out sID);
            return sID;
        }


        public void SaveAdditinalTax(string MasterId, decimal BooksCurrencyBaseRate, OTSBD.IdentityParameter para, List<Dictionary<string, object>> UserSendData)
        {

            try
            {
                string sql = "select * from TRN.SalesAdditionalTax where SalesId='" + MasterId + "'";
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
                        dr["SalesId"] = MasterId.ToString();
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

        public IEnumerable<object> GetAdvanceTaxInfo(string SalesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                sql = @"Select a.Id,a.TaxCodeId,a.Percentage ValueOfFixed,a.TaxAmount,a.AddedBy,a.AddedDate,a.AddedFromIP,b.UserName TaxName,SalesId
						from [TRN].[SalesAdditionalTax] a
						left join [mst].[TAXCode] b ON b.Id=a.TaxCodeId where a.SalesId='" + SalesId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> AdditionalTaxDelete(string Id)
        {
            try
            {
                var _sql = @" Delete from [TRN].[SalesAdditionalTax] where Id='" + Id + @"'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPackingSOData(string PackingId)
        {
            try
            {
                var _sql = @"SELECT  MOI.Id MasterOrderItemId,MOI.MasterOrderId,SO.Id SONo,SO.Id SalesOrderId, po.PONumber,PODate=REPLACE(CONVERT(CHAR(11), po.PODate, 106),' ','-'), DeliveryDate = REPLACE(CONVERT(CHAR(11), SO.DeliveryDate, 106),' ','-'),SO.ParentId
							, SO.DestinationId
							,DT.UserName DestinationName
							,PM.UserName ProductName
							, SO.ShipmentModeId
							,MOI.MaterialMasterId
							,MM.UserName MaterialMasterName
							,MOI.ArticleId
							,WithSKU=CASE WHEN MM.WithSKU=1 THEN 'Yes' WHEN MM.WithSKU=0 THEN 'No' END
							,MMA.StandardName MaterialMasterArticleName
							,FCH.Id FirstCharacteristicsId
							,FCH.CharacteristicsValueId FirstCharacteristicsValueId
							,CHV.UserName SKU1
							,CHV2.UserName SKU2
							,SCH.Id SecondCharacteristicsId
							,SCH.CharacteristicsValueId SecondCharacteristicsValueId
							,CHV3.UserName SKU3
							,TCH.Id ThirdCharacteristicsId
							,TCH.CharacteristicsValueId ThirdCharacteristicsValueId
							--,FCH.Qty SKU1Qty,SCH.Qty SKU2Qty
							, SO.MasterOrderItemId
							, MOI.MaterialMasterId
							, CommitmentDate = REPLACE(CONVERT(CHAR(11), SO.CommitmentDate, 106),' ','-')
							, SO.CustomerPOId
							, SO.OrderStatusId, SO.OrderCategoryId
							, SO.SOType, SO.ResponsiblePersonId
							,MO.TotalQtyUOMId BaseUOMId
							, SO.UpCharge,  SO.Rate, SO.IsFirstEntry,SO.Discount,EMP.EmployeeName ResponsiblePersonName
							,FORMAT (SO.LSD, 'dd-MMM-yyyy') as LSD ,FORMAT (SO.MainRawMaterialInhouseDate, 'dd-MMM-yyyy') as MainRawMaterialInhouseDate
							,FORMAT (SO.OtherRawMaterialInhouseDate, 'dd-MMM-yyyy') as OtherRawMaterialInhouseDate
							, hasFirst=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[FirstCharacteristics] WHERE SalesOrderId=SO.Id)
                            
							,(SELECT ISNULL(sum(Qty),0) FROM TRN.FirstCharacteristics AS FCS WHERE SO.Id= FCS.SalesOrderId) SKUQty
							, isTax=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[SalesOrderTax] WHERE SalesOrderId=SO.Id),mm.HSNCodeId,mo.InvoicingPartyPlantId
							,POLR.Qty,POLR.PlanQty,Balance=POLR.PlanQty-POLR.Qty,TransactionQty=POLR.Qty,TransactionAmount=POLR.Qty*SO.Rate
							,BaseRate=SO.Rate,TransactionRate=SO.Rate,BaseQty=POLR.Qty,TransactionQty=POLR.Qty,BaseAmount=POLR.Qty*SO.Rate,POLR.Qty SalesQty,'' GoodsDescription
							FROM [TRN].[SalesOrder] AS SO
							JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
							JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id
							JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
							LEFT JOIN [MST].[MaterialMasterArticle] AS MMA ON MOI.ArticleId = MMA.Id
							LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
							LEFT JOIN dbo.EmployeeInformation AS EMP ON EMP.SystemId = SO.ResponsiblePersonId
							LEFT JOIN [TRN].[FirstCharacteristics] AS FCH ON FCH.SalesOrderId=SO.Id
							LEFT  JOIN [HKP].[Characteristics] AS CH ON FCH.CharacteristicsId=CH.Id
							LEFT JOIN [HKP].[CharacteristicsValue] AS CHV ON FCH.CharacteristicsValueId=CHV.Id
							LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId

							LEFT JOIN [TRN].[SecondCharacteristics] AS SCH ON    SO.Id=SCH.SalesOrderId 
							AND FCH.Id=SCH.FirstCharacteristicsId AND SCH.Qty >0
							LEFT JOIN [HKP].[Characteristics] AS CH2 ON SCH.CharacteristicsId=CH2.Id
							LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON SCH.CharacteristicsValueId=CHV2.Id

							LEFT JOIN [TRN].[ThirdCharacteristics] AS TCH ON    SO.Id=TCH.SalesOrderId 
							LEFT JOIN [HKP].[Characteristics] AS CH3 ON TCH.CharacteristicsId=CH3.Id
							LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON TCH.CharacteristicsValueId=CHV3.Id

							LEFT JOIN TRN.ProductDefinition AS PD ON PD.MaterialMasterId=MOI.MaterialMasterId
							LEFT JOIN MST.ProductMaster AS PM ON PM.Id=PD.ProductMasterId
							LEFT JOIN trn.PackingLineItem PLI ON PLI.SOId=SO.Id
							LEFT JOIN 
							(
							--Select SUM(BookQty) Qty, SUM(PlanQty) PlanQty,PackingLineItemId from trn.POLotReference 
							--GROUP BY PackingLineItemId
							Select ISNULL(SUM(sc.NetWeight),0) Qty, ISNULL(SUM(PlanQty),0) PlanQty,PackingLineItemId from trn.POLotReference po
							left join dbo.ItemScanChild sc on sc.PackingId = po.Id AND Booked = 1 
							 GROUP BY PackingLineItemId
							)POLR ON POLR.PackingLineItemId=PLI.PackingLineItemId
							LEFT JOIN(
							Select ISNULL(SUM(SM.TransactionQty),0) TransactionQty,SM.SalesOrderId from TRN.SalesMaterial SM
							JOIN trn.PackingLineItem PLI ON PLI.SOId=SM.SalesOrderId
							GROUP BY  SM.SalesOrderId
							) A ON A.SalesOrderId=SO.Id
							WHERE  PLI.PackingId " + PackingId + @" ORDER BY SO.DeliveryDate";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public GridModel GetPackingSalesList(GridParameter parameters, string companyGroupId, string companyId)
        {
            try
            {
                //parameters.sort = "CAST(AddedDate AS datetime)";
                //parameters.sort = "TAB.AddedDate,TAB.InvoiceNo";
                parameters.CmdText = @"SELECT S.Id,S.Id AS SalesId, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, S.CurrencyId,CO.BaseCurrencyId, C.Code AS CurrencyCode, S.DocRefNo, ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0) AS Amount,
									 Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,
									Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
                                    , S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId, S.BaseNoOfDays, S.BaseOnDueDate
									, S.InvoiceNo, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
									, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, S.MatureDate, S.ToCurrencyRate
									, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId,S.BLNumber,S.ItemDescription,S.ComercialInvoiceNo,S.EXPFromNo,S.EXPDate,S.BLDate
                                    , CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark,S.AddedDate,s.AddedBy,S.AddedFromIP,FORMAT(S.UpdatedDate,'dd-MMM-yyyy') UpdatedDate,s.UpdatedBy,S.UpdatedFromIP
									FROM [TRN].[Sales] AS S
                                    LEFT JOIN [ORG].[Company] AS CO ON CO.Id=S.CompanyId
                                    JOIN [HKP].[Party] AS P ON P.Id=S.PartyId
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=S.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=S.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=S.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=S.PlantId
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PT.AddressMasterId
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId=S.Id
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId=S.Id
                                    WHERE S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + "'  AND SourceType='Packing'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

        public IEnumerable<object> GetSalesPackingData(string salesId)
        {
            try
            {
                string str = @"SELECT SP.Id,SP.PackingId, format(Date,'dd-MMM-yyyy') as AddedDate, format(InactiveDate,'dd-MMM-yyyy') as InActiveDate, p.UserName as Customer, ms.UserName as StorageLoc , e.EmployeeName as ByWhom,
                            ei.Employeename as DRespPerson, en.UserName as Entity, pk.Remarks,pk.CustomerId,pk.EntityId,CP.CurrencyId,C.Code AS Currency 
                            FROM dbo.SalesPacking SP
							LEFT JOIN TRN.Packing pk ON pk.PackingId=SP.PackingId
                            LEFT JOIN hkp.Party p on p.Id = pk.CustomerId
                            LEFT JOIN dbo.EmployeeInformation e on e.SystemId = pk.ByWhom
                            LEFT JOIN dbo.EmployeeInformation ei on ei.SystemId = pk.DispatchResponsiblePersonId
                            LEFT JOIN hkp.MaterialStorage ms on ms.Id = pk.StorageLocId
                            LEFT JOIN org.Entity en on en.Id = pk.EntityId
                            LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
							Where SP.SalesId='" + salesId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Dictionary<string, object> GetQtyAmountByPackingId(string packingid)
        {
            try
            {
                string sql = @"Select PackingID,Sum(NetWeight) Qty,Sum(Amount) Amount,TembTbl.ProductLibraryId from
							(
							SELECT PK.PackingId,IsNull(ISC.NetWeight,0) NetWeight,IsNull(RD.MaterialTranRate,0) Rate,IsNull(ISC.NetWeight,0) * IsNull(RD.MaterialTranRate,0) Amount,PL.Id ProductLibraryId FROM
							dbo.ItemScanChild ISC
							LEFT JOIN TRN.POLotReference POR ON ISC.PackingId=POR.Id
							LEFT JOIN TRN.PackingLineItem PLI ON POR.PackingLineItemId=PLI.PackingLineItemId
							LEFT JOIN TRN.Packing PK ON PLI.PackingId=PK.PackingId
							LEFT JOIN TRN.InventoryReceiveDetail RD ON ISC.InventoryReceiveDetailId=RD.Id
							LEFT JOIN dbo.ProductLibrary PL ON PL.Code=ISC.ProductCode
							Where PK.PackingId='" + packingid + @"'
							) TembTbl group by PackingId,ProductLibraryId";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<Dictionary<string, object>> GetParkedSalesList(string companyGroupId, string companyId)
        {
            try
            {
                var cmdText = @"SELECT CAST(0 AS BIT) Flag,S.Id AS SalesId, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName,P.Code Tracenent, S.CurrencyId, C.Code AS CurrencyCode
									, DocRefNo=case when S.SourceType='MasterOrderSales' then 'MS-'+ S.DocRefNo WHEN S.SourceType='Packing' THEN 'PS-'+S.DocRefNo else 'S-'+ S.DocRefNo end
									, ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0) AS Amount,
									Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') DocDate,
									Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
                                    , S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId
									, Replace(CONVERT(VARCHAR(11), S.MatureDate, 106), ' ', '-')  MatureDate, Replace(CONVERT(VARCHAR(11), S.BaseOnDueDate, 106), ' ', '-') BaseOnDueDate,S.BaseNoOfDays
									, InvoiceNo=case when S.SourceType='MasterOrderSales' then 'MS-'+ S.InvoiceNo WHEN S.SourceType='Packing' THEN 'PS-'+S.InvoiceNo else 'S-'+ S.InvoiceNo end
									, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
									, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, S.ToCurrencyRate
									, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId
                                    , CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark, CP.TaxApplicable,CP.PartyAccountGroupId,CP.IsPaymentTermChangeable
									, S.SourceType--,SP.Id SalesPackingId
									FROM [TRN].[Sales] AS S
                                    JOIN [HKP].[Party] AS P ON P.Id=S.PartyId
                                    LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND S.PlantId=CP.PlantId AND CP.PartyType='Customer'
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=S.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=S.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=S.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=S.PlantId
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PT.AddressMasterId
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId=S.Id
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId=S.Id
                                    WHERE S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + "'  AND S.RowState='Parked'";
                return _sqlRepository.GetDataCollection(cmdText);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSalesMaterialList(string Ids)
        {
            try
            {
                var sql = @"SELECT CAST(0 AS BIT) Active,sm.Id SalesMaterialId,'B2B'SupplyTypeCode,'No'ReverseCharge,''eCommGSTIN,''IgstOnIntra,''DocumentType,S.Id DocumentNumber,FORMAT(s.InvoiceDate,'dd-MMM-yyyy')DocumentDate
,p.TINNO BuyerGSTIN,P.UserName BuyerLegalName,''BuyerTradeName,ST.UserName BuyerPOS,am.Address1 BuyerAddr1
,am.Address2 BuyerAddr2,ST.UserName BuyerLocation,P.PINCode BuyerPinCode,ST.UserName BuyerState, am.Phone BuyerPhoneNumber,am.Email BuyerEmailId
,'' DispatchName,'' DispatchAddr1,''DispatchAddr2,''DispatchLocation,''DispatchPinCode,''DispatchState,''ShippingGSTIN,''ShippingLegalName,''ShippingTradeName
,''ShippingAddr1,''ShippingAddr2,''ShippingLocation,''ShippingPinCode,''ShippingState,''SlNo,mma.StandardName ProductDescription,''IsService,h.Code HSNcode   
,''Barcode, sm.TransactionQty Quantity,''FreeQuantity,uom.Code Unit,FORMAT(sm.TransactionRate,'N4') UnitPrice,FORMAT(sm.TransactionAmount,'N2') GrossAmount,'' Discount,''PreTaxValue
,FORMAT(sm.TransactionAmount,'N2') Taxablevalue,FORMAT(TAxInfo1.Percentage,'N2') GSTRate,FORMAT(TAxInfo1.Amount,'N4') IgstAmt,FORMAT(TAxInfo2.Amount,'N2') SgstAmt,FORMAT(TAxInfo3.Amount,'N2') CgstAmt,'' CessRate,''CessAmtAdval
,''CessNonAdvalAmt,''StateCessRate,''StateCessAdvalAmt,''StateCessNonAdvalAmt,''OtherCharges,FORMAT(sm.NetAmount,'N2') ItemTotal,''BatchName,''BatchExpiryDt,''WarrantyDt
,FORMAT(sm.NetAmount,'N2') TotalInvoicevalue,''ShippingBillNo,''ShippingBillDt,''[Port],''Refundclaim,''ForeignCurrency,''CountryCode,''ExportDutyAmount,''TransID,''TransName 
,''TransMode,''Distance,''TransDocNo,''TransDocDate,''VehicleNo,''VehicleType,''ErrorList
  FROM TRN.Sales S
LEFT JOIN TRN.SalesMaterial AS sm ON sm.SalesId=s.Id 
LEFT JOIN MST.MaterialMaster AS mm ON sm.MaterialMasterId=mm.Id
LEFT JOIN HKP.HSNCode AS h ON h.Id = mm.HSNCodeId
LEFT JOIN MST.MaterialMasterArticle AS mma ON sm.ArticleId=mma.Id
LEFT JOIN SCS.UnitOfMeasurement AS uom ON uom.Id = sm.BaseUOMId
LEFT JOIN hkp.Party P ON P.Id = S.PartyId
LEFT JOIN MST.AddressMaster AS am ON am.Id = P.AddressMasterId
LEFT JOIN SCS.[State] ST ON ST.Id = am.StateId 
LEFT JOIN (
SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage,A.Amount,hs.Code HSCode 
FROM TRN.SalesTax A
LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
WHERE B.Code='IGST' AND SalesServiceId IS NULL
) TAxInfo1	ON TAxInfo1.SalesMaterialId=sm.Id 
LEFT JOIN (
SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage,A.Amount,hs.Code HSCode 
FROM TRN.SalesTax A
LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
WHERE B.Code='SGST' AND SalesServiceId IS NULL
) TAxInfo2	ON TAxInfo2.SalesMaterialId=sm.Id
LEFT JOIN (
SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage,A.Amount,hs.Code HSCode 
FROM TRN.SalesTax A
LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
WHERE B.Code='CGST' AND SalesServiceId IS NULL
) TAxInfo3	ON TAxInfo3.SalesMaterialId=sm.Id
WHERE s.RowState='Parked' AND s.Id " + Ids + "";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetSalesMaterialDataList(string Ids)
        {
            try
            {
                var sql = @"SELECT CAST(0 AS BIT) Active,sm.Id SalesMaterialId,'B2B'SupplyTypeCode,'No'ReverseCharge,''eCommGSTIN,''IgstOnIntra,'Tax Invoice' DocumentType,S.Id DocumentNumber,FORMAT(s.InvoiceDate,'dd/MM/yyyy')DocumentDate
,p.TINNO BuyerGSTIN,P.UserName BuyerLegalName,''BuyerTradeName,ST.UserName BuyerPOS,am.Address1 BuyerAddr1
,am.Address2 BuyerAddr2,ST.UserName BuyerLocation,P.PINCode BuyerPinCode,ST.UserName BuyerState, am.Phone BuyerPhoneNumber,am.Email BuyerEmailId
,'' DispatchName,'' DispatchAddr1,''DispatchAddr2,''DispatchLocation,''DispatchPinCode,''DispatchState,''ShippingGSTIN,''ShippingLegalName,''ShippingTradeName
,''ShippingAddr1,''ShippingAddr2,''ShippingLocation,''ShippingPinCode,''ShippingState,''SlNo,mma.StandardName ProductDescription,''IsService,h.Code HSNcode   
,''Barcode, sm.TransactionQty Quantity,''FreeQuantity,uom.Code Unit,CONVERT(numeric(10,2),sm.TransactionRate*s.ToCurrencyRate) UnitPrice,CONVERT(numeric(10,2),sm.TransactionAmount*s.ToCurrencyRate)GrossAmount,'' Discount,''PreTaxValue
,CONVERT(numeric(10,2),sm.TransactionAmount*s.ToCurrencyRate)Taxablevalue,CONVERT(numeric(10,2),TAxInfo1.Percentage) GSTRate,CONVERT(numeric(10,2),TAxInfo1.Amount*s.ToCurrencyRate) IgstAmt,CONVERT(numeric(10,2),TAxInfo2.Amount*s.ToCurrencyRate) SgstAmt,CONVERT(numeric(10,2),TAxInfo3.Amount*s.ToCurrencyRate) CgstAmt,'' CessRate,''CessAmtAdval
,''CessNonAdvalAmt,''StateCessRate,''StateCessAdvalAmt,''StateCessNonAdvalAmt,''OtherCharges,CONVERT(numeric(10,2),sm.NetAmount*s.ToCurrencyRate) ItemTotal,''BatchName,''BatchExpiryDt,''WarrantyDt
,CONVERT(numeric(10,2),sm.NetAmount*s.ToCurrencyRate)TotalInvoicevalue,''ShippingBillNo,''ShippingBillDt,''[Port],''Refundclaim,''ForeignCurrency,''CountryCode,''ExportDutyAmount,''TransID,''TransName 
,''TransMode,''Distance,''TransDocNo,''TransDocDate,''VehicleNo,''VehicleType,''ErrorListst
  FROM TRN.Sales S
LEFT JOIN TRN.SalesMaterial AS sm ON sm.SalesId=s.Id 
LEFT JOIN MST.MaterialMaster AS mm ON sm.MaterialMasterId=mm.Id
LEFT JOIN HKP.HSNCode AS h ON h.Id = mm.HSNCodeId
LEFT JOIN MST.MaterialMasterArticle AS mma ON sm.ArticleId=mma.Id
LEFT JOIN SCS.UnitOfMeasurement AS uom ON uom.Id = sm.BaseUOMId
LEFT JOIN hkp.Party P ON P.Id = S.PartyId
LEFT JOIN MST.AddressMaster AS am ON am.Id = P.AddressMasterId
LEFT JOIN SCS.[State] ST ON ST.Id = am.StateId 
LEFT JOIN (
SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage,A.Amount,hs.Code HSCode 
FROM TRN.SalesTax A
LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
WHERE B.Code='IGST' AND SalesServiceId IS NULL
) TAxInfo1	ON TAxInfo1.SalesMaterialId=sm.Id 
LEFT JOIN (
SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage,A.Amount,hs.Code HSCode 
FROM TRN.SalesTax A
LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
WHERE B.Code='SGST' AND SalesServiceId IS NULL
) TAxInfo2	ON TAxInfo2.SalesMaterialId=sm.Id
LEFT JOIN (
SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage,A.Amount,hs.Code HSCode 
FROM TRN.SalesTax A
LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
WHERE B.Code='CGST' AND SalesServiceId IS NULL
) TAxInfo3	ON TAxInfo3.SalesMaterialId=sm.Id
WHERE s.RowState='Parked' AND sm.Id IN(" + Ids + ")";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook GetEInvoiceReports(string CompanyGroupId, string CompanyId, string PlantId, string PlantName, string UserName, string issueIds)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string FactoryAddress = string.Empty;
            string OTConsiderOn = string.Empty;
            #endregion

            try
            {
                objRpt = new clsReport();


                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);

                #region Get Data Query
                DataTable dtdata = GetSalesMaterialDataList(issueIds);
                if (dtdata.Rows.Count == 0)
                    throw new Exception("No data found");


                #endregion

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";

                var colSupplyTypeCode = 0; var colReverseCharge = 0; int coleCommGSTIN = 0; int colIgstOnIntra = 0; int colDocumentType = 0; int colDocumentNumber = 0; int colDocumentDate = 0; int colBuyerGSTIN = 0; int colBuyerLegalName = 0; int colBuyerTradeName = 0; int colBuyerPOS = 0; int colBuyerAddr1 = 0; int colBuyerAddr2 = 0; int colBuyerLocation = 0; int colBuyerPinCode = 0; int colBuyerState = 0; int colBuyerPhoneNumber = 0; int colBuyerEmailId = 0; int colDispatchName = 0; int colDispatchAddr1 = 0; int colDispatchAddr2 = 0; int colDispatchLocation = 0; int colDispatchPinCode = 0; int colDispatchState = 0; int colShippingGSTIN = 0; int colShippingLegalName = 0; int colShippingTradeName = 0; int colShippingAddr1 = 0; int colShippingAddr2 = 0; int colShippingLocation = 0; int colShippingPinCode = 0; int colShippingState = 0; int colSlNo = 0; int colProductDescription = 0; int colIsService = 0; int colHSNcode = 0;
                int colBarcode = 0; int colQuantity = 0; int colFreeQuantity = 0; int colUnit = 0; int colUnitPrice = 0; int colGrossAmount = 0; int colDiscount = 0;
                int colPreTaxValue = 0; int colTaxablevalue = 0; int colGSTRate = 0; int colIgstAmt = 0; int colSgstAmt = 0; int colCgstAmt = 0; int colCessRate = 0;
                int colCessAmtAdval = 0; int colCessNonAdvalAmt = 0; int colStateCessRate = 0; int colStateCessAdvalAmt = 0; int colStateCessNonAdvalAmt = 0;
                int colOtherCharges = 0; int colItemTotal = 0; int colBatchName = 0; int colBatchExpiryDt = 0; int colWarrantyDt = 0; int colTotalInvoicevalue = 0;
                int colShippingBillNo = 0; int colShippingBillDt = 0; int colPort = 0; int colRefundclaim = 0; int colForeignCurrency = 0; int colCountryCode = 0;
                int colExportDutyAmount = 0; int colTransID = 0; int colTransName = 0; int colTransMode = 0; int colDistance = 0; int colTransDocNo = 0; int colTransDocDate = 0; int colVehicleNo = 0; int colVehicleType = 0; int colErrorList = 0;

                objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);

                objRpt.SelectedPlant(PlantId, out dsFactory);

                workbook = application.Workbooks.Create(1);

                #region Task List

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 6;

                #region ------------------Column Header------------------
                colSupplyTypeCode = xlsCol;
                sheet1.Range[xlsRow, colSupplyTypeCode].Text = "Supply Type Code";
                sheet1.Range[xlsRow, colSupplyTypeCode].ColumnWidth = 14;
                xlsCol += 1;
                colReverseCharge = xlsCol;
                sheet1.Range[xlsRow, colReverseCharge].Text = "Reverse Charge";
                sheet1.Range[xlsRow, colReverseCharge].ColumnWidth = 11;
                xlsCol += 1;
                coleCommGSTIN = xlsCol;
                sheet1.Range[xlsRow, coleCommGSTIN].Text = "e-Comm GSTIN";
                sheet1.Range[xlsRow, coleCommGSTIN].ColumnWidth = 11;


                xlsCol += 1;
                colIgstOnIntra = xlsCol;
                sheet1.Range[xlsRow, colIgstOnIntra].Text = "Igst On Intra";
                sheet1.Range[xlsRow, colIgstOnIntra].ColumnWidth = 11;

                xlsCol += 1;
                colDocumentType = xlsCol;
                sheet1.Range[xlsRow, colDocumentType].Text = "Document Type";
                sheet1.Range[xlsRow, colDocumentType].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colDocumentType].ColumnWidth = 11;

                xlsCol += 1;
                colDocumentNumber = xlsCol;
                sheet1.Range[xlsRow, colDocumentNumber].Text = "Document Number";
                sheet1.Range[xlsRow, colDocumentNumber].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colDocumentNumber].ColumnWidth = 12;
                xlsCol += 1;
                colDocumentDate = xlsCol;
                sheet1.Range[xlsRow, colDocumentDate].Text = "Document Date";
                sheet1.Range[xlsRow, colDocumentDate].ColumnWidth = 12;

                xlsCol += 1;
                colBuyerGSTIN = xlsCol;
                sheet1.Range[xlsRow, colBuyerGSTIN].Text = "Buyer GSTIN";
                sheet1.Range[xlsRow, colBuyerGSTIN].ColumnWidth = 14;

                xlsCol += 1;
                colBuyerLegalName = xlsCol;
                sheet1.Range[xlsRow, colBuyerLegalName].Text = "Buyer Legal Name";
                sheet1.Range[xlsRow, colBuyerLegalName].ColumnWidth = 34;

                xlsCol += 1;
                colBuyerTradeName = xlsCol;
                sheet1.Range[xlsRow, colBuyerTradeName].Text = "Buyer Trade Name";
                sheet1.Range[xlsRow, colBuyerTradeName].ColumnWidth = 25;

                xlsCol += 1;
                colBuyerPOS = xlsCol;
                sheet1.Range[xlsRow, colBuyerPOS].Text = "Buyer POS";
                sheet1.Range[xlsRow, colBuyerPOS].ColumnWidth = 14;

                xlsCol += 1;
                colBuyerAddr1 = xlsCol;
                sheet1.Range[xlsRow, colBuyerAddr1].Text = "Buyer Addr1";
                sheet1.Range[xlsRow, colBuyerAddr1].ColumnWidth = 53;

                xlsCol += 1;
                colBuyerAddr2 = xlsCol;
                sheet1.Range[xlsRow, colBuyerAddr2].Text = "Buyer Addr2";
                sheet1.Range[xlsRow, colBuyerAddr2].ColumnWidth = 25;

                xlsCol += 1;
                colBuyerLocation = xlsCol;
                sheet1.Range[xlsRow, colBuyerLocation].Text = "Buyer Location";
                sheet1.Range[xlsRow, colBuyerLocation].ColumnWidth = 11;

                xlsCol += 1;
                colBuyerPinCode = xlsCol;
                sheet1.Range[xlsRow, colBuyerPinCode].Text = "Buyer Pin Code";
                sheet1.Range[xlsRow, colBuyerPinCode].ColumnWidth = 10;

                xlsCol += 1;
                colBuyerState = xlsCol;
                sheet1.Range[xlsRow, colBuyerState].Text = "Buyer State";
                sheet1.Range[xlsRow, colBuyerState].ColumnWidth = 9;

                xlsCol += 1;
                colBuyerPhoneNumber = xlsCol;
                sheet1.Range[xlsRow, colBuyerPhoneNumber].Text = "Buyer Phone Number";
                sheet1.Range[xlsRow, colBuyerPhoneNumber].ColumnWidth = 14;

                xlsCol += 1;
                colBuyerEmailId = xlsCol;
                sheet1.Range[xlsRow, colBuyerEmailId].Text = "Buyer EmailId";
                sheet1.Range[xlsRow, colBuyerEmailId].ColumnWidth = 25;

                xlsCol += 1;
                colDispatchName = xlsCol;
                sheet1.Range[xlsRow, colDispatchName].Text = "Dispatch Name";
                sheet1.Range[xlsRow, colDispatchName].ColumnWidth = 12;

                xlsCol += 1;
                colDispatchAddr1 = xlsCol;
                sheet1.Range[xlsRow, colDispatchAddr1].Text = "Dispatch Addr1";
                sheet1.Range[xlsRow, colDispatchAddr1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colDispatchAddr1].ColumnWidth = 12;

                xlsCol += 1;
                colDispatchAddr2 = xlsCol;
                sheet1.Range[xlsRow, colDispatchAddr2].Text = "Dispatch Addr2";
                sheet1.Range[xlsRow, colDispatchAddr2].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colDispatchAddr2].ColumnWidth = 12;
                xlsCol += 1;
                colDispatchLocation = xlsCol;
                sheet1.Range[xlsRow, colDispatchLocation].Text = "Dispatch Location";
                sheet1.Range[xlsRow, colDispatchLocation].ColumnWidth = 12;

                xlsCol += 1;
                colDispatchPinCode = xlsCol;
                sheet1.Range[xlsRow, colDispatchPinCode].Text = "Dispatch Pin Code";
                sheet1.Range[xlsRow, colDispatchPinCode].ColumnWidth = 12;

                xlsCol += 1;
                colDispatchState = xlsCol;
                sheet1.Range[xlsRow, colDispatchState].Text = "Dispatch State";
                sheet1.Range[xlsRow, colDispatchState].ColumnWidth = 12;

                xlsCol += 1;
                colShippingGSTIN = xlsCol;
                sheet1.Range[xlsRow, colShippingGSTIN].Text = "Shipping GSTIN";
                sheet1.Range[xlsRow, colShippingGSTIN].ColumnWidth = 25;
                xlsCol += 1;
                colShippingLegalName = xlsCol;
                sheet1.Range[xlsRow, colShippingLegalName].Text = "Shipping Legal Name";
                sheet1.Range[xlsRow, colShippingLegalName].ColumnWidth = 14;

                xlsCol += 1;
                colShippingTradeName = xlsCol;
                sheet1.Range[xlsRow, colShippingTradeName].Text = "Shipping Trade Name";
                sheet1.Range[xlsRow, colShippingTradeName].ColumnWidth = 14;

                xlsCol += 1;
                colShippingAddr1 = xlsCol;
                sheet1.Range[xlsRow, colShippingAddr1].Text = "Shipping Addr1";
                sheet1.Range[xlsRow, colShippingAddr1].ColumnWidth = 14;

                xlsCol += 1;
                colShippingAddr2 = xlsCol;
                sheet1.Range[xlsRow, colShippingAddr2].Text = "Shipping Addr2";
                sheet1.Range[xlsRow, colShippingAddr2].ColumnWidth = 14;

                xlsCol += 1;
                colShippingLocation = xlsCol;
                sheet1.Range[xlsRow, colShippingLocation].Text = "Shipping Location";
                sheet1.Range[xlsRow, colShippingLocation].ColumnWidth = 14;

                xlsCol += 1;
                colShippingPinCode = xlsCol;
                sheet1.Range[xlsRow, colShippingPinCode].Text = "Shipping Pin Code";
                sheet1.Range[xlsRow, colShippingPinCode].ColumnWidth = 14;

                xlsCol += 1;
                colShippingState = xlsCol;
                sheet1.Range[xlsRow, colShippingState].Text = "Shipping State";
                sheet1.Range[xlsRow, colShippingState].ColumnWidth = 14;

                xlsCol += 1;
                colSlNo = xlsCol;
                sheet1.Range[xlsRow, colSlNo].Text = "SlNo";
                sheet1.Range[xlsRow, colSlNo].ColumnWidth = 14;

                xlsCol += 1;
                colProductDescription = xlsCol;
                sheet1.Range[xlsRow, colProductDescription].Text = "Product Description";
                sheet1.Range[xlsRow, colProductDescription].ColumnWidth = 41;

                xlsCol += 1;
                colIsService = xlsCol;
                sheet1.Range[xlsRow, colIsService].Text = "Is_Service";
                sheet1.Range[xlsRow, colIsService].ColumnWidth = 14;

                xlsCol += 1;
                colHSNcode = xlsCol;
                sheet1.Range[xlsRow, colHSNcode].Text = "HSN code";
                sheet1.Range[xlsRow, colHSNcode].ColumnWidth = 14;
                xlsCol += 1;
                colBarcode = xlsCol;
                sheet1.Range[xlsRow, colBarcode].Text = "Barcode";
                sheet1.Range[xlsRow, colBarcode].ColumnWidth = 14;

                xlsCol += 1;
                colQuantity = xlsCol;
                sheet1.Range[xlsRow, colQuantity].Text = "Quantity";
                sheet1.Range[xlsRow, colQuantity].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colQuantity].ColumnWidth = 14;

                xlsCol += 1;
                colFreeQuantity = xlsCol;
                sheet1.Range[xlsRow, colFreeQuantity].Text = "Free Quantity";
                sheet1.Range[xlsRow, colFreeQuantity].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colFreeQuantity].ColumnWidth = 14;
                xlsCol += 1;
                colUnit = xlsCol;
                sheet1.Range[xlsRow, colUnit].Text = "Unit";
                sheet1.Range[xlsRow, colUnit].ColumnWidth = 14;

                xlsCol += 1;
                colUnitPrice = xlsCol;
                sheet1.Range[xlsRow, colUnitPrice].Text = "Unit Price";
                sheet1.Range[xlsRow, colUnitPrice].ColumnWidth = 14;

                xlsCol += 1;
                colGrossAmount = xlsCol;
                sheet1.Range[xlsRow, colGrossAmount].Text = "Gross Amount";
                sheet1.Range[xlsRow, colGrossAmount].ColumnWidth = 14;

                xlsCol += 1;
                colDiscount = xlsCol;
                sheet1.Range[xlsRow, colDiscount].Text = "Discount";
                sheet1.Range[xlsRow, colDiscount].ColumnWidth = 14;

                xlsCol += 1;
                colPreTaxValue = xlsCol;
                sheet1.Range[xlsRow, colPreTaxValue].Text = "Pre Tax Value";
                sheet1.Range[xlsRow, colPreTaxValue].ColumnWidth = 14;

                xlsCol += 1;
                colTaxablevalue = xlsCol;
                sheet1.Range[xlsRow, colTaxablevalue].Text = "Taxable value";
                sheet1.Range[xlsRow, colTaxablevalue].ColumnWidth = 14;

                xlsCol += 1;
                colGSTRate = xlsCol;
                sheet1.Range[xlsRow, colGSTRate].Text = "GST Rate(%)";
                sheet1.Range[xlsRow, colGSTRate].ColumnWidth = 14;

                xlsCol += 1;
                colSgstAmt = xlsCol;
                sheet1.Range[xlsRow, colSgstAmt].Text = "Sgst Amt(Rs)";
                sheet1.Range[xlsRow, colSgstAmt].ColumnWidth = 14;

                xlsCol += 1;
                colCgstAmt = xlsCol;
                sheet1.Range[xlsRow, colCgstAmt].Text = "Cgst Amt(Rs)";
                sheet1.Range[xlsRow, colCgstAmt].ColumnWidth = 14;

                xlsCol += 1;
                colIgstAmt = xlsCol;
                sheet1.Range[xlsRow, colIgstAmt].Text = "Igst Amt(Rs)";
                sheet1.Range[xlsRow, colIgstAmt].ColumnWidth = 14;


                xlsCol += 1;
                colCessRate = xlsCol;
                sheet1.Range[xlsRow, colCessRate].Text = "Cess Rate(%)";
                sheet1.Range[xlsRow, colCessRate].ColumnWidth = 14;

                xlsCol += 1;
                colCessAmtAdval = xlsCol;
                sheet1.Range[xlsRow, colCessAmtAdval].Text = "Cess Amt Adval(Rs)";
                sheet1.Range[xlsRow, colCessAmtAdval].ColumnWidth = 14;

                xlsCol += 1;
                colCessNonAdvalAmt = xlsCol;
                sheet1.Range[xlsRow, colCessNonAdvalAmt].Text = "Cess Non Adval Amt(Rs)";
                sheet1.Range[xlsRow, colCessNonAdvalAmt].ColumnWidth = 14;

                xlsCol += 1;
                colStateCessRate = xlsCol;
                sheet1.Range[xlsRow, colStateCessRate].Text = "State Cess Rate(%)";
                sheet1.Range[xlsRow, colStateCessRate].ColumnWidth = 14;

                xlsCol += 1;
                colStateCessAdvalAmt = xlsCol;
                sheet1.Range[xlsRow, colStateCessAdvalAmt].Text = "State Cess Adval Amt(Rs)";
                sheet1.Range[xlsRow, colStateCessAdvalAmt].ColumnWidth = 14;

                xlsCol += 1;
                colStateCessNonAdvalAmt = xlsCol;
                sheet1.Range[xlsRow, colStateCessNonAdvalAmt].Text = "State Cess Non Adval Amt(Rs)";
                sheet1.Range[xlsRow, colStateCessNonAdvalAmt].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colStateCessNonAdvalAmt].ColumnWidth = 14;

                xlsCol += 1;
                colOtherCharges = xlsCol;
                sheet1.Range[xlsRow, colOtherCharges].Text = "Other Charges";
                sheet1.Range[xlsRow, colOtherCharges].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colOtherCharges].ColumnWidth = 14;
                xlsCol += 1;
                colItemTotal = xlsCol;
                sheet1.Range[xlsRow, colItemTotal].Text = "Item Total";
                sheet1.Range[xlsRow, colItemTotal].ColumnWidth = 14;

                xlsCol += 1;
                colBatchName = xlsCol;
                sheet1.Range[xlsRow, colBatchName].Text = "Batch Name";
                sheet1.Range[xlsRow, colBatchName].ColumnWidth = 14;

                xlsCol += 1;
                colBatchExpiryDt = xlsCol;
                sheet1.Range[xlsRow, colBatchExpiryDt].Text = "Batch Expiry Dt";
                sheet1.Range[xlsRow, colBatchExpiryDt].ColumnWidth = 14;

                xlsCol += 1;
                colWarrantyDt = xlsCol;
                sheet1.Range[xlsRow, colWarrantyDt].Text = "Warranty Dt";
                sheet1.Range[xlsRow, colWarrantyDt].ColumnWidth = 14;
                xlsCol += 1;
                colTotalInvoicevalue = xlsCol;
                sheet1.Range[xlsRow, colTotalInvoicevalue].Text = "Total Taxable value";
                sheet1.Range[xlsRow, colTotalInvoicevalue].ColumnWidth = 14;

                xlsCol += 1;
                int colSgstAmts = xlsCol;
                sheet1.Range[xlsRow, colSgstAmts].Text = "Sgst Amt";
                sheet1.Range[xlsRow, colSgstAmts].ColumnWidth = 14;

                xlsCol += 1;
                int colCgstAmts = xlsCol;
                sheet1.Range[xlsRow, colCgstAmts].Text = "Cgst Amt";
                sheet1.Range[xlsRow, colCgstAmts].ColumnWidth = 14;

                xlsCol += 1;
                int colIgstAmts = xlsCol;
                sheet1.Range[xlsRow, colIgstAmts].Text = "Igst Amt";
                sheet1.Range[xlsRow, colIgstAmts].ColumnWidth = 14;

                xlsCol += 1;
                int colCessAmt = xlsCol;
                sheet1.Range[xlsRow, colCessAmt].Text = "Cess Amt";
                sheet1.Range[xlsRow, colCessAmt].ColumnWidth = 14;

                xlsCol += 1;
                int colStateCessAmt = xlsCol;
                sheet1.Range[xlsRow, colStateCessAmt].Text = "State Cess Amt";
                sheet1.Range[xlsRow, colStateCessAmt].ColumnWidth = 14;

                xlsCol += 1;
                int colDiscounts = xlsCol;
                sheet1.Range[xlsRow, colDiscounts].Text = "Discount";
                sheet1.Range[xlsRow, colDiscounts].ColumnWidth = 14;

                xlsCol += 1;
                int colOtherChargess = xlsCol;
                sheet1.Range[xlsRow, colOtherChargess].Text = "Other chargess";
                sheet1.Range[xlsRow, colOtherChargess].ColumnWidth = 14;

                xlsCol += 1;
                int colRoundoff = xlsCol;
                sheet1.Range[xlsRow, colRoundoff].Text = "Round off";
                sheet1.Range[xlsRow, colRoundoff].ColumnWidth = 14;

                xlsCol += 1;
                int colTotalInvoicevalues = xlsCol;
                sheet1.Range[xlsRow, colTotalInvoicevalues].Text = "Total Invoice value";
                sheet1.Range[xlsRow, colTotalInvoicevalues].ColumnWidth = 14;


                xlsCol += 1;
                colShippingBillNo = xlsCol;
                sheet1.Range[xlsRow, colShippingBillNo].Text = "Shipping BillNo";
                sheet1.Range[xlsRow, colShippingBillNo].ColumnWidth = 14;

                xlsCol += 1;
                colShippingBillDt = xlsCol;
                sheet1.Range[xlsRow, colShippingBillDt].Text = "Shipping BillDt";
                sheet1.Range[xlsRow, colShippingBillDt].ColumnWidth = 14;

                xlsCol += 1;
                colPort = xlsCol;
                sheet1.Range[xlsRow, colPort].Text = "Port";
                sheet1.Range[xlsRow, colPort].ColumnWidth = 14;

                xlsCol += 1;
                colRefundclaim = xlsCol;
                sheet1.Range[xlsRow, colRefundclaim].Text = "Refund claim";
                sheet1.Range[xlsRow, colRefundclaim].ColumnWidth = 14;

                xlsCol += 1;
                colForeignCurrency = xlsCol;
                sheet1.Range[xlsRow, colForeignCurrency].Text = "Foreign Currency";
                sheet1.Range[xlsRow, colForeignCurrency].ColumnWidth = 14;

                xlsCol += 1;
                colCountryCode = xlsCol;
                sheet1.Range[xlsRow, colCountryCode].Text = "Country Code";
                sheet1.Range[xlsRow, colCountryCode].ColumnWidth = 14;

                xlsCol += 1;
                colExportDutyAmount = xlsCol;
                sheet1.Range[xlsRow, colExportDutyAmount].Text = "Export Duty Amount";
                sheet1.Range[xlsRow, colExportDutyAmount].ColumnWidth = 14;

                xlsCol += 1;
                colTransID = xlsCol;
                sheet1.Range[xlsRow, colTransID].Text = "TransID";
                sheet1.Range[xlsRow, colTransID].ColumnWidth = 14;

                xlsCol += 1;
                colTransName = xlsCol;
                sheet1.Range[xlsRow, colTransName].Text = "TransName";
                sheet1.Range[xlsRow, colTransName].ColumnWidth = 14;

                xlsCol += 1;
                colTransMode = xlsCol;
                sheet1.Range[xlsRow, colTransMode].Text = "Trans Mode";
                sheet1.Range[xlsRow, colTransMode].ColumnWidth = 14;

                xlsCol += 1;
                colDistance = xlsCol;
                sheet1.Range[xlsRow, colDistance].Text = "Distance";
                sheet1.Range[xlsRow, colDistance].ColumnWidth = 14;

                xlsCol += 1;
                colTransDocNo = xlsCol;
                sheet1.Range[xlsRow, colTransDocNo].Text = "Trans Doc No";
                sheet1.Range[xlsRow, colTransDocNo].ColumnWidth = 14;

                xlsCol += 1;
                colTransDocDate = xlsCol;
                sheet1.Range[xlsRow, colTransDocDate].Text = "Trans Doc Date";
                sheet1.Range[xlsRow, colTransDocDate].ColumnWidth = 14;

                xlsCol += 1;
                colVehicleNo = xlsCol;
                sheet1.Range[xlsRow, colVehicleNo].Text = "Vehicle No";
                sheet1.Range[xlsRow, colVehicleNo].ColumnWidth = 14;

                xlsCol += 1;
                colVehicleType = xlsCol;
                sheet1.Range[xlsRow, colVehicleType].Text = "Vehicle Type";
                sheet1.Range[xlsRow, colVehicleType].ColumnWidth = 9;

                xlsCol += 1;
                colErrorList = xlsCol;
                sheet1.Range[xlsRow, colErrorList].Text = "Error List";
                sheet1.Range[xlsRow, colErrorList].ColumnWidth = 35;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 40;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsRow++;

                #endregion ------------------Column Header------------------

                //Add rich-text Excel comment
                IFont fontCaption = workbook.CreateFont();
                fontCaption.Size = 8f;
                IFont fontRegular = workbook.CreateFont();
                fontRegular.Italic = true;
                fontRegular.Size = 6f;

                int StartRow = xlsRow;
                int slCount = 0;
                double TotalInvoicevalue = 0;
                double Taxablevalue = 0;
                double IgstAmt = 0;
                string pd = "";
                #region ----------------------Data-----------------------
                for (int i = 0; i < dtdata.Rows.Count; i++)
                {

                    if (pd == dtdata.Rows[i]["DocumentNumber"].ToString())
                    {
                        slCount++;
                    }
                    else
                    {
                        slCount = 1;
                    }
                    pd = dtdata.Rows[i]["DocumentNumber"].ToString();


                    sheet1.Range[xlsRow, colSupplyTypeCode].Text = dtdata.Rows[i]["SupplyTypeCode"].ToString();
                    sheet1.Range[xlsRow, colReverseCharge].Text = dtdata.Rows[i]["ReverseCharge"].ToString();
                    sheet1.Range[xlsRow, coleCommGSTIN].Text = dtdata.Rows[i]["eCommGSTIN"].ToString();
                    sheet1.Range[xlsRow, colIgstOnIntra].Text = dtdata.Rows[i]["IgstOnIntra"].ToString();
                    sheet1.Range[xlsRow, colDocumentType].Text = dtdata.Rows[i]["DocumentType"].ToString();
                    sheet1.Range[xlsRow, colDocumentNumber].Text = dtdata.Rows[i]["DocumentNumber"].ToString();
                    sheet1.Range[xlsRow, colDocumentDate].Text = dtdata.Rows[i]["DocumentDate"].ToString();
                    sheet1.Range[xlsRow, colBuyerGSTIN].Text = dtdata.Rows[i]["BuyerGSTIN"].ToString();
                    sheet1.Range[xlsRow, colBuyerLegalName].Text = dtdata.Rows[i]["BuyerLegalName"].ToString();
                    sheet1.Range[xlsRow, colBuyerTradeName].Text = dtdata.Rows[i]["BuyerTradeName"].ToString();
                    sheet1.Range[xlsRow, colBuyerPOS].Text = dtdata.Rows[i]["BuyerPOS"].ToString();
                    sheet1.Range[xlsRow, colBuyerAddr1].Text = dtdata.Rows[i]["BuyerAddr1"].ToString();
                    sheet1.Range[xlsRow, colBuyerAddr2].Text = dtdata.Rows[i]["BuyerAddr2"].ToString();
                    sheet1.Range[xlsRow, colBuyerLocation].Text = dtdata.Rows[i]["BuyerLocation"].ToString();
                    sheet1.Range[xlsRow, colBuyerPinCode].Text = dtdata.Rows[i]["BuyerPinCode"].ToString();
                    sheet1.Range[xlsRow, colBuyerState].Text = dtdata.Rows[i]["BuyerState"].ToString();
                    sheet1.Range[xlsRow, colBuyerPhoneNumber].Text = dtdata.Rows[i]["BuyerPhoneNumber"].ToString();
                    sheet1.Range[xlsRow, colBuyerEmailId].Text = dtdata.Rows[i]["BuyerEmailId"].ToString();
                    sheet1.Range[xlsRow, colDispatchName].Text = dtdata.Rows[i]["DispatchName"].ToString();

                    sheet1.Range[xlsRow, colDispatchAddr1].Text = dtdata.Rows[i]["DispatchAddr1"].ToString();
                    sheet1.Range[xlsRow, colDispatchAddr2].Text = dtdata.Rows[i]["DispatchAddr2"].ToString();
                    sheet1.Range[xlsRow, colDispatchLocation].Text = dtdata.Rows[i]["DispatchLocation"].ToString();
                    sheet1.Range[xlsRow, colDispatchPinCode].Text = dtdata.Rows[i]["DispatchPinCode"].ToString();
                    sheet1.Range[xlsRow, colDispatchState].Text = dtdata.Rows[i]["DispatchState"].ToString();
                    sheet1.Range[xlsRow, colShippingGSTIN].Text = dtdata.Rows[i]["ShippingGSTIN"].ToString();
                    sheet1.Range[xlsRow, colShippingLegalName].Text = dtdata.Rows[i]["ShippingLegalName"].ToString();
                    sheet1.Range[xlsRow, colShippingTradeName].Text = dtdata.Rows[i]["ShippingTradeName"].ToString();
                    sheet1.Range[xlsRow, colShippingAddr1].Text = dtdata.Rows[i]["ShippingAddr1"].ToString();
                    sheet1.Range[xlsRow, colShippingAddr2].Text = dtdata.Rows[i]["ShippingAddr2"].ToString();
                    sheet1.Range[xlsRow, colShippingLocation].Text = dtdata.Rows[i]["ShippingLocation"].ToString();
                    sheet1.Range[xlsRow, colShippingPinCode].Text = dtdata.Rows[i]["ShippingPinCode"].ToString();
                    sheet1.Range[xlsRow, colShippingState].Text = dtdata.Rows[i]["ShippingState"].ToString();
                    sheet1.Range[xlsRow, colSlNo].Text = slCount.ToString();
                    sheet1.Range[xlsRow, colProductDescription].Text = dtdata.Rows[i]["ProductDescription"].ToString();
                    sheet1.Range[xlsRow, colIsService].Text = "NO";
                    sheet1.Range[xlsRow, colHSNcode].Text = dtdata.Rows[i]["HSNcode"].ToString();
                    sheet1.Range[xlsRow, colBarcode].Text = dtdata.Rows[i]["Barcode"].ToString();
                    sheet1.Range[xlsRow, colQuantity].Text = dtdata.Rows[i]["Quantity"].ToString();
                    //sheet1.Range[xlsRow, colQuantity].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet1.Range[xlsRow, colFreeQuantity].Text = dtdata.Rows[i]["FreeQuantity"].ToString();
                    //sheet1.Range[xlsRow, colUnit].Text = dtdata.Rows[i]["Unit"].ToString();
                    sheet1.Range[xlsRow, colUnit].Text = "KILOGRAMS";
                    //sheet1.Range[xlsRow, colUnit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(4);
                    sheet1.Range[xlsRow, colUnitPrice].Text = dtdata.Rows[i]["UnitPrice"].ToString();
                    //sheet1.Range[xlsRow, colUnitPrice].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet1.Range[xlsRow, colGrossAmount].Text = dtdata.Rows[i]["GrossAmount"].ToString();
                    //sheet1.Range[xlsRow, colGrossAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet1.Range[xlsRow, colDiscount].Text = dtdata.Rows[i]["Discount"].ToString();
                    sheet1.Range[xlsRow, colDiscounts].Text = dtdata.Rows[i]["Discount"].ToString();
                    sheet1.Range[xlsRow, colPreTaxValue].Text = dtdata.Rows[i]["PreTaxValue"].ToString();
                    sheet1.Range[xlsRow, colTaxablevalue].Text = dtdata.Rows[i]["Taxablevalue"].ToString();
                    //sheet1.Range[xlsRow, colTaxablevalue].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet1.Range[xlsRow, colGSTRate].Text = dtdata.Rows[i]["GSTRate"].ToString();
                    //sheet1.Range[xlsRow, colGSTRate].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet1.Range[xlsRow, colIgstAmt].Text = dtdata.Rows[i]["IgstAmt"].ToString();
                    IgstAmt = clsStaticInfo.dbl(dtdata.Compute("SUM(IgstAmt)", "DocumentNumber='" + dtdata.Rows[i]["DocumentNumber"].ToString() + "'"));
                    sheet1.Range[xlsRow, colIgstAmts].Text = Convert.ToString(IgstAmt);
                    sheet1.Range[xlsRow, colSgstAmt].Text = dtdata.Rows[i]["SgstAmt"].ToString();
                    sheet1.Range[xlsRow, colSgstAmts].Text = Convert.ToString(clsStaticInfo.dbl(dtdata.Compute("SUM(SgstAmt)", "DocumentNumber='" + dtdata.Rows[i]["DocumentNumber"].ToString() + "'")));
                    sheet1.Range[xlsRow, colCgstAmt].Text = dtdata.Rows[i]["CgstAmt"].ToString();
                    sheet1.Range[xlsRow, colCgstAmts].Text = Convert.ToString(clsStaticInfo.dbl(dtdata.Compute("SUM(CgstAmt)", "DocumentNumber='" + dtdata.Rows[i]["DocumentNumber"].ToString() + "'")));
                    sheet1.Range[xlsRow, colCessRate].Text = dtdata.Rows[i]["CessRate"].ToString();
                    sheet1.Range[xlsRow, colCessAmtAdval].Text = dtdata.Rows[i]["CessAmtAdval"].ToString();
                    sheet1.Range[xlsRow, colCessAmt].Text = dtdata.Rows[i]["CessAmtAdval"].ToString();
                    sheet1.Range[xlsRow, colStateCessAmt].Text = "";
                    sheet1.Range[xlsRow, colCessNonAdvalAmt].Text = dtdata.Rows[i]["CessNonAdvalAmt"].ToString();
                    sheet1.Range[xlsRow, colStateCessRate].Text = dtdata.Rows[i]["StateCessRate"].ToString();
                    sheet1.Range[xlsRow, colStateCessAdvalAmt].Text = dtdata.Rows[i]["StateCessAdvalAmt"].ToString();
                    sheet1.Range[xlsRow, colStateCessNonAdvalAmt].Text = dtdata.Rows[i]["StateCessNonAdvalAmt"].ToString();
                    sheet1.Range[xlsRow, colOtherCharges].Text = dtdata.Rows[i]["OtherCharges"].ToString();
                    sheet1.Range[xlsRow, colOtherChargess].Text = dtdata.Rows[i]["OtherCharges"].ToString();
                    sheet1.Range[xlsRow, colItemTotal].Text = dtdata.Rows[i]["ItemTotal"].ToString();
                    sheet1.Range[xlsRow, colTotalInvoicevalues].Text = Convert.ToString(clsStaticInfo.dbl(dtdata.Compute("SUM(ItemTotal)", "DocumentNumber='" + dtdata.Rows[i]["DocumentNumber"].ToString() + "'")));
                    sheet1.Range[xlsRow, colBatchName].Text = dtdata.Rows[i]["BatchName"].ToString();
                    sheet1.Range[xlsRow, colBatchExpiryDt].Text = dtdata.Rows[i]["BatchExpiryDt"].ToString();
                    sheet1.Range[xlsRow, colWarrantyDt].Text = dtdata.Rows[i]["WarrantyDt"].ToString();
                    sheet1.Range[xlsRow, colTotalInvoicevalue].Text = Convert.ToString(clsStaticInfo.dbl(dtdata.Compute("SUM(Taxablevalue)", "DocumentNumber='" + dtdata.Rows[i]["DocumentNumber"].ToString() + "'"))); 
                    //sheet1.Range[xlsRow, colTotalInvoicevalue].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet1.Range[xlsRow, colShippingBillNo].Text = dtdata.Rows[i]["ShippingBillNo"].ToString();
                    sheet1.Range[xlsRow, colShippingBillDt].Text = dtdata.Rows[i]["ShippingBillDt"].ToString();
                    sheet1.Range[xlsRow, colPort].Text = dtdata.Rows[i]["Port"].ToString();
                    sheet1.Range[xlsRow, colRefundclaim].Text = dtdata.Rows[i]["Refundclaim"].ToString();
                    sheet1.Range[xlsRow, colForeignCurrency].Text = dtdata.Rows[i]["ForeignCurrency"].ToString();
                    sheet1.Range[xlsRow, colCountryCode].Text = dtdata.Rows[i]["CountryCode"].ToString();
                    sheet1.Range[xlsRow, colExportDutyAmount].Text = dtdata.Rows[i]["ExportDutyAmount"].ToString();
                    sheet1.Range[xlsRow, colTransID].Text = dtdata.Rows[i]["TransID"].ToString();
                    sheet1.Range[xlsRow, colTransName].Text = dtdata.Rows[i]["TransName"].ToString();
                    sheet1.Range[xlsRow, colTransMode].Text = dtdata.Rows[i]["TransMode"].ToString();
                    sheet1.Range[xlsRow, colDistance].Text = dtdata.Rows[i]["Distance"].ToString();
                    sheet1.Range[xlsRow, colTransDocNo].Text = dtdata.Rows[i]["TransDocNo"].ToString();
                    sheet1.Range[xlsRow, colTransDocDate].Text = dtdata.Rows[i]["TransDocDate"].ToString();
                    sheet1.Range[xlsRow, colVehicleNo].Text = dtdata.Rows[i]["VehicleNo"].ToString();
                    sheet1.Range[xlsRow, colVehicleType].Text = dtdata.Rows[i]["VehicleType"].ToString();
                    sheet1.Range[xlsRow, colErrorList].Text = null;


                    xlsRow++;
                }
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;
                sheet1.Range[StartRow, 1, xlsRow - 1, endXlsCol].CellStyle.Font.Size = 8f;
                // sheet1.AutoFilters.FilterRange = sheet1.Range[StartRow - 1, 1, xlsRow, endXlsCol];
                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************
                xlsRow = 1;
                FactoryName = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "E-Invoice System";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment
                sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange.WrapText = true;
                //sheet1.UsedRange.NumberFormat = "#,##0.00";
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + UserName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "EInvoice";
                #endregion Page Setup

                #endregion  ManualOutTime



                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetplantByCompanyId(string companyId)
        {
            try
            {
                string CmdText = @"Select CAST(0 as bit) Flag,P.* from ORG.Plant P Where CompanyId='" + companyId + "' Order By P.Sequence";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetAllEmployeeData()
        {
            try
            {
                string CmdText = @"SELECT CAST (0 AS bit) Flag,E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,PMB.Code BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,E.DepartmentId
                                    ,E.DivisionId
									,E.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,E.EmployeeCategorySystemID EmployeeCategoryId
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
                                    ,E.EmployeeCode
									,E.EmpPicPath
                                    ,E.DOJ
                                    ,P.UserName Plant
									,SS.UserName SubSection
                                    ,E.EmployeeCodeNumeric
                                    ,C.UserName Company
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON E.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON E.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
                                LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
                                WHERE E.EmployeeStatus='Active' AND E.EmpType<>'Guest'  Order by EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SavePlantData(List<Dictionary<string, object>> data)
        {
            try
            {
                if (data != null)
                {
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.SalesOrderApprovalPlant", out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["PlantId"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = item["PlantId"];
                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }


                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }



            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        public IEnumerable<object> GetSalesOrderApprovalPlantData(string masterid)
        {
            try
            {
                string CmdText = @"Select SP.*,P.Sequence,P.ShortName,P.StandardName,P.UserName
from SalesOrderApprovalPlant SP
LEFT JOIN ORG.Plant P ON P.Id=SP.PlantId
Where SP.SalesOrderApprovalMasterId='" + masterid + @"'
Order by P.Sequence";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveCheckByData(List<Dictionary<string, object>> data)
        {
            try
            {
                if (data != null)
                {
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.SalesOrderCheckBy", out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "EmpSystemId='" + item["EmpSystemId"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = item["EmpSystemId"];
                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }


                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }



            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void SaveApproveByData(List<Dictionary<string, object>> data)
        {
            try
            {
                if (data != null)
                {
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.SalesOrderApproveBy", out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "EmpSystemId='" + item["EmpSystemId"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = item["EmpSystemId"];
                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }


                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }



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

        public IEnumerable<object> GetCheckByData(string masterId)
        {
            try
            {
                string CmdText = @"SELECT PE.Id,PE.EmpSystemId
							    	,E.EmployeeName
							    	,PMB.Code BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,E.DepartmentId
                                    ,E.DivisionId
									,E.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,E.EmployeeCategorySystemID EmployeeCategoryId
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
                                    ,E.EmployeeCode
									,E.EmpPicPath
                                    ,E.DOJ
                                    ,P.UserName Plant
									,SS.UserName SubSection
                                    ,E.EmployeeCodeNumeric
                                    ,C.UserName Company
							    FROM [dbo].[SalesOrderCheckBy] PE
							    LEFT JOIN  EmployeeInformation E ON E.SystemId=PE.EmpSystemId
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON E.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON E.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
                                LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
                                WHERE PE.SalesOrderApprovalMasterId='" + masterId + "' Order by EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetApproveByData(string masterId)
        {
            try
            {
                string CmdText = @"SELECT PE.Id,PE.EmpSystemId
							    	,E.EmployeeName
							    	,PMB.Code BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,E.DepartmentId
                                    ,E.DivisionId
									,E.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,E.EmployeeCategorySystemID EmployeeCategoryId
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
                                    ,E.EmployeeCode
									,E.EmpPicPath
                                    ,E.DOJ
                                    ,P.UserName Plant
									,SS.UserName SubSection
                                    ,E.EmployeeCodeNumeric
                                    ,C.UserName Company
							    FROM [dbo].[SalesOrderApproveBy] PE
							    LEFT JOIN  EmployeeInformation E ON E.SystemId=PE.EmpSystemId
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON E.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON E.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
                                LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
                                WHERE PE.SalesOrderApprovalMasterId='" + masterId + "' Order by EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> GetMasterOrderSalesPostedList(string companyGroupId, string companyId, string plantId, string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        select * from ( SELECT S.Id,S.Id AS SalesId, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, S.CurrencyId, C.Code AS CurrencyCode, S.DocRefNo, ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0) AS Amount,
									Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,
									Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
                                    , S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId, S.BaseNoOfDays, S.BaseOnDueDate
									, S.InvoiceNo, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
									, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, S.MatureDate, S.ToCurrencyRate
									, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId
                                    , CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark
									,V.VoucherNo,SP.VoucherId SalesPackingVoucherId
									FROM [TRN].[Sales] AS S
                                    JOIN [HKP].[Party] AS P ON P.Id=S.PartyId
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=S.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=S.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=S.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=S.PlantId
									LEFT JOIN [TRN].Voucher V ON V.Id=S.VoucherId
									LEFT JOIN dbo.SalesPacking SP ON SP.SalesId=S.Id
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PT.AddressMasterId
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId=S.Id
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId=S.Id
                                    WHERE S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + "' AND S.PlantId='" + plantId + "' AND S.VoucherId<>'' AND S.SourceType='MasterOrderSales'" +
                                    ") AS TEMP WHERE " + strkey + " order by PostingDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }


}
