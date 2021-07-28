using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
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
							, Qty=case when SCH.CharacteristicsValueId<>''  then SCH.Qty
										when FCH.CharacteristicsValueId<>'' then FCH.Qty 
										else SO.Qty end
                            ,0 SalesQty
							, PlanQty	=(	SELECT isnull(case when SCH.CharacteristicsValueId<>''  then SCH.Qty
										when FCH.CharacteristicsValueId<>'' then FCH.Qty 
										else 
										SO.Qty 
										end, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))

                            , ExistSalesQty=ISNULL(case when SCH.CharacteristicsValueId<>''  then SCH.SalesQty
										when FCH.CharacteristicsValueId<>'' then FCH.SalesQty end,
										A.TransactionQty)
							,Balance=ISNULL((case when SCH.CharacteristicsValueId<>''  then SCH.Qty
										when FCH.CharacteristicsValueId<>'' then FCH.Qty else SO.Qty end)
										-(ISNULL(case when SCH.CharacteristicsValueId<>''  then SCH.SalesQty
										when FCH.CharacteristicsValueId<>'' then FCH.SalesQty end,A.TransactionQty)),0)

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

                        LEFT JOIN(
					     Select SUM(SM.TransactionQty) TransactionQty,SM.SalesOrderId from TRN.SalesMaterial SM
                        JOIN TRN.SalesOrderItem SOI ON SOI.SalesId=SM.SalesId
                        Where SOI.MasterOrderItemId " + masterOrderId + @" GROUP BY  SM.SalesOrderId
					   ) A ON A.SalesOrderId=SO.Id

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
           ,0 SalesQty
           ,Balance=ISNULL((case when SC.CharacteristicsValueId<>''  then SC.Qty
										when FC.CharacteristicsValueId<>'' then FC.Qty 
										else SO.Qty  
							end)-(case when SC.CharacteristicsValueId<>''  then SC.SalesQty
										when FC.CharacteristicsValueId<>'' then FC.SalesQty else SO.Qty 
							end),0)
           ,ExistSalesQty=
							ISNULL(case when SC.CharacteristicsValueId<>''  then SC.SalesQty
										when FC.CharacteristicsValueId<>'' then FC.SalesQty else SO.Qty 
							end,0)
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


		public void SaveAdditinalTax(string MasterId,  decimal BooksCurrencyBaseRate, OTSBD.IdentityParameter para, List<Dictionary<string, object>> UserSendData)
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
						dr["BooksCurrencyTaxAmount"] = Math.Round(Convert.ToDecimal(UserSendData[i]["TaxAmount"]) * BooksCurrencyBaseRate,2);
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
							,BaseRate=SO.Rate,TransactionRate=SO.Rate,BaseQty=POLR.Qty,TransactionQty=POLR.Qty,BaseAmount=POLR.Qty*SO.Rate,POLR.Qty SalesQty
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
							Select SUM(BookQty) Qty, SUM(PlanQty) PlanQty,PackingLineItemId from trn.POLotReference 
							GROUP BY PackingLineItemId
							)POLR ON POLR.PackingLineItemId=PLI.PackingLineItemId
							LEFT JOIN(
							Select SUM(SM.TransactionQty) TransactionQty,SM.SalesOrderId from TRN.SalesMaterial SM
							JOIN trn.PackingLineItem PLI ON PLI.SOId=SM.SalesOrderId
							GROUP BY  SM.SalesOrderId
							) A ON A.SalesOrderId=SO.Id
							WHERE  PLI.PackingId " + PackingId+@" ORDER BY SO.DeliveryDate";
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
				parameters.CmdText = @"SELECT S.Id,S.Id AS SalesId, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, S.CurrencyId,CO.BaseCurrencyId, C.Code AS CurrencyCode, S.DocRefNo, ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0) AS Amount,
									 Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,
									Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
                                    , S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId, S.BaseNoOfDays, S.BaseOnDueDate
									, S.InvoiceNo, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
									, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, S.MatureDate, S.ToCurrencyRate
									, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId,S.BLNumber,S.ItemDescription,S.ComercialInvoiceNo,S.EXPFromNo,S.EXPDate,S.BLDate
                                    , CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark,FORMAT(S.AddedDate,'dd-MMM-yyyy')AddedDate,s.AddedBy,S.AddedFromIP,FORMAT(S.UpdatedDate,'dd-MMM-yyyy') UpdatedDate,s.UpdatedBy,S.UpdatedFromIP
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
				var str = @"SELECT SP.Id,SP.PackingId, format(Date,'dd-MMM-yyyy') as AddedDate, format(InactiveDate,'dd-MMM-yyyy') as InActiveDate, DATEDIFF(Day,GETDATE() , InactiveDate) as Active , p.UserName as Customer, ms.UserName as StorageLoc , e.EmployeeName as ByWhom,
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
							Where SP.SalesId='"+ salesId + "'";
				return _sqlRepository.GetDataCollection(str);
			}
			catch (Exception e)
			{
				throw e;
			}
		}



	}


}
