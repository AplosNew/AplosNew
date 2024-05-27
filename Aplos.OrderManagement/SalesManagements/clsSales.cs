using ConnectionManager;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
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
                            , isTax=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[SalesOrderTax] WHERE SalesOrderId=SO.Id),ISNULL(mma.HSNCodeId,mm.HSNCodeId)HSNCodeId,MO.InvoicingPartyPlantId
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
					JOIN [HKP].[Party] P ON P.Id = MO.PartyId
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
                  ,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesService] WHERE SalesId=SA.Id)/NULLIF((Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id),0))*SM.TransactionAmount
	           ,ServiceTax=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesTax] WHERE SalesId=SA.Id  AND SalesServiceId<>'')/NULLIF((Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id),0))*SM.TransactionAmount
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
            , SCV.UserName AS SecondCharacteristicsValue,TCV.UserName AS ThirdCharacteristicsValue,SM.FirstCharacteristicsValueId,SM.SecondCharacteristicsValueId,SM.ThirdCharacteristicsValueId 
			, SM.IsCanceled,SM.CanceledBy,SM.Remark,SM.FirstCharacteristicsId,SM.SecondCharacteristicsId,SM.ThirdCharacteristicsId , HSN.Code HSNCode
            FROM TRN.SalesMaterial AS SM 
            LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId
            LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
            LEFT JOIN MST.MaterialMasterArticle AS ART ON SM.ArticleId=ART.Id
			left join HKP.HSNCode as HSN on HSN.Id = ART.HSNCodeId
            LEFT JOIN TRN.FirstCharacteristics AS FC ON FC.Id=SM.FirstCharacteristicsId
            LEFT JOIN HKP.CharacteristicsValue AS FCV ON FCV.Id=SM.FirstCharacteristicsValueId
            LEFT JOIN TRN.SecondCharacteristics AS SC ON SC.Id=SM.SecondCharacteristicsId
            LEFT JOIN HKP.CharacteristicsValue AS SCV ON SCV.Id=SM.SecondCharacteristicsValueId
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

        public IEnumerable<object> GetSalesAdditionalInfoData(string salesId)
        {
            try
            {
                string sql = @"SELECT Flag=CAST(CASE WHEN SA.Id IS NULL THEN 0 ELSE 1 END AS bit),A.UserName,SA.Id,SA.SalesId
,A.Id AdditionalInfoId,SA.Value,SA.Remarks,A.CharecterType,'' CharType,''datepic
FROM [HKP].[AdditionalInfo] A
OUTER APPLY(Select * from [dbo].[SalesAdditionalInfo] Where AdditionalInfoId=A.Id AND SalesId='" + salesId + @"') SA  Where A.Category='SalesInvoice' Order By A.sequence";
                return _sqlRepository.GetDataCollection(sql);
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
                var _sql = @"SELECT  MOI.Id MasterOrderItemId,MOI.MasterOrderId,SO.Id SONo,SO.Id SalesOrderId,PLI.PackingId, po.PONumber,PODate=REPLACE(CONVERT(CHAR(11), po.PODate, 106),' ','-'), DeliveryDate = REPLACE(CONVERT(CHAR(11), SO.DeliveryDate, 106),' ','-'),SO.ParentId
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
							, isTax=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[SalesOrderTax] WHERE SalesOrderId=SO.Id),ISNULL(MMA.HSNCodeId,MM.HSNCodeId)HSNCodeId,ISNULL(HA.Code,HM.Code)HSNCode,MO.InvoicingPartyPlantId,MO.DeliveryPartyPlantId
							,POLR.Qty,POLR.PlanQty,Balance=POLR.PlanQty-POLR.Qty,TransactionQty=POLR.Qty,TransactionAmount=POLR.Qty*SO.Rate
							,BaseRate=SO.Rate,TransactionRate=SO.Rate,BaseQty=POLR.Qty,TransactionQty=POLR.Qty,BaseAmount=POLR.Qty*SO.Rate,POLR.Qty SalesQty,'' GoodsDescription
							FROM [TRN].[SalesOrder] AS SO
							JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
							JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id
							JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
							JOIN [HKP].[Party] P ON P.Id = MO.PartyId
							LEFT JOIN [MST].[MaterialMasterArticle] AS MMA ON MOI.ArticleId = MMA.Id
							LEFT JOIN HKP.HSNCode HM ON HM.Id=MM.HSNCodeId
							LEFT JOIN HKP.HSNCode HA ON HA.Id=MMA.HSNCodeId
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
							left join dbo.ItemScanChild sc on sc.PackingId = po.Id AND Booked = 1 and SalesReturnId is null
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
                                    , CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark,S.AddedDate,s.AddedBy,S.AddedFromIP,FORMAT(S.UpdatedDate,'dd-MMM-yyyy') UpdatedDate,s.UpdatedBy,S.UpdatedFromIP,S.PaymentToReceiveBankId , NEGBNKMT.AccountTitle BankName
									,IsMail=CAST((CASE WHEN FORMAT(S.AddedDate,'dd-MMM-yyyy') = FORMAT(GETDATE(),'dd-MMM-yyyy') THEN 0 ELSE 1 END) AS BIT) 
									,MLCRef=Stuff((
										SELECT distinct',' + LC.LCRef
										FROM dbo.MasterLC LC 
										LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
										LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
										LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
										WHERE SM.SalesId=S.Id
										FOR XML PATH('')
										), 1, 1, '')
									,ContractNo=Stuff((
											SELECT distinct',' + C.Id
											FROM  dbo.[Contract] C 
											LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
											LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
											WHERE SM.SalesId=S.Id
											FOR XML PATH('')
											), 1, 1, '')
									,S.IsAdditionalInfoApplicable , S.AdditionalFrieght , S.AdditionalFrieghtValue , S.Incoterms , S.IncotermsValue
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
									LEFT JOIN dbo.PostSalesInvoice PSI ON PSI.SalesId = s.Id
									AND PSI.Id=(SELECT TOP 1 Id FROM dbo.PostSalesInvoice MR WHERE MR.SalesId=PSI.SalesId ORDER BY MR.UpdatedDate DESC)
									left join mst.BankMaster NEGBNKMT on NEGBNKMT.Id = PSI.BankMasterId
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

            try
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
                ,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesService] WHERE SalesId=SA.Id)/NULLIF((Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id),0))*SM.TransactionAmount
	           ,ServiceTax=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesTax] WHERE SalesId=SA.Id  AND SalesServiceId<>'')/NULLIF((Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id),0))*SM.TransactionAmount,ISNULL(ART.HSNCodeId,MM.HSNCodeId)HSNCodeId,ISNULL(HA.Code,HM.Code)HSNCode
			,A.PaymentTermId,A.Code PaymentTermCode,A.UserName PaymentTermName,A.BaseLineDate, A.NoOfDay,A.PaymentMode 
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
			LEFT JOIN HKP.HSNCode HM ON HM.Id=MM.HSNCodeId
			LEFT JOIN HKP.HSNCode HA ON HA.Id=ART.HSNCodeId
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
			LEFT JOIN (SELECT PT.Id PaymentTermId, PT.UserName , PT.BaseLineDate, PTD.NoOfDay, PT.Code,PT.PaymentMode
                            FROM [MST].[PaymentTerm] PT
                            LEFT JOIN [MST].[PaymentTermDetail] PTD ON PTD.PaymentTermId=PT.Id
                            WHERE PTD.[Sequence]='3' AND PT.Active=1 AND PT.Archive=0 AND PT.IsCustomer=1) AS A ON A.PaymentTermId=MO.PaymentTermId

            WHERE SA.CompanyGroupId='" + companyGroupId + "' AND SA.CompanyId='" + companyId + "' AND SA.PlantId='" + plantId + "' AND SA.Id='" + salesId + "'";

                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> GetSalesPackingData(string salesId)
        {
            try
            {
                string str = @"SELECT SP.PackingId, SP.Id, format(Date,'dd-MMM-yyyy') as AddedDate, format(InactiveDate,'dd-MMM-yyyy') as InActiveDate, p.UserName as Customer, ms.UserName as StorageLoc , e.EmployeeName as ByWhom,
                            ei.Employeename as DRespPerson, en.UserName as Entity, pk.Remarks,pk.CustomerId,pk.EntityId,CP.CurrencyId,C.Code AS Currency
                            FROM dbo.SalesPacking SP
							LEFT JOIN TRN.Packing pk ON pk.PackingId=SP.PackingId
                            LEFT JOIN hkp.Party p on p.Id = pk.CustomerId
                            LEFT JOIN dbo.EmployeeInformation e on e.SystemId = pk.ByWhom
                            LEFT JOIN dbo.EmployeeInformation ei on ei.SystemId = pk.DispatchResponsiblePersonId
                            LEFT JOIN hkp.MaterialStorage ms on ms.Id = pk.StorageLocId
                            LEFT JOIN org.Entity en on en.Id = pk.EntityId
                            LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'
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
                                    WHERE S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + "'";
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
,''ShippingAddr1,''ShippingAddr2,''ShippingLocation,''ShippingPinCode,''ShippingState,''SlNo,mma.StandardName ProductDescription,''IsService,ISNULL(ha.Code,h.Code) HSNcode   
,''Barcode, sm.TransactionQty Quantity,''FreeQuantity,uom.Code Unit,FORMAT(sm.TransactionRate,'N4') UnitPrice,FORMAT(sm.TransactionAmount,'N2') GrossAmount,'' Discount,''PreTaxValue
,FORMAT(sm.TransactionAmount,'N2') Taxablevalue,FORMAT(TAxInfo1.Percentage,'N2') GSTRate,FORMAT(TAxInfo1.Amount,'N4') IgstAmt,FORMAT(TAxInfo2.Amount,'N2') SgstAmt,FORMAT(TAxInfo3.Amount,'N2') CgstAmt,'' CessRate,''CessAmtAdval
,''CessNonAdvalAmt,''StateCessRate,''StateCessAdvalAmt,''StateCessNonAdvalAmt,TAxInfo4.TaxAmount OtherCharges,FORMAT(sm.NetAmount,'N2') ItemTotal,''BatchName,''BatchExpiryDt,''WarrantyDt
,FORMAT(sm.NetAmount,'N2') TotalInvoicevalue,''ShippingBillNo,''ShippingBillDt,''[Port],''Refundclaim,''ForeignCurrency,''CountryCode,''ExportDutyAmount,''TransID,''TransName 
,''TransMode,''Distance,''TransDocNo,''TransDocDate,''VehicleNo,''VehicleType,''ErrorList
  FROM TRN.Sales S
LEFT JOIN TRN.SalesMaterial AS sm ON sm.SalesId=s.Id 
LEFT JOIN MST.MaterialMaster AS mm ON sm.MaterialMasterId=mm.Id
LEFT JOIN MST.MaterialMasterArticle AS mma ON sm.ArticleId=mma.Id
LEFT JOIN HKP.HSNCode AS h ON h.Id = mm.HSNCodeId
LEFT JOIN HKP.HSNCode AS ha ON ha.Id = mma.HSNCodeId
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
LEFT JOIN (
SELECT A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage,A.TaxAmount 
FROM TRN.[SalesAdditionalTax] A
LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
WHERE B.Code='TCS'
) TAxInfo4	ON TAxInfo4.SalesId=s.Id
WHERE s.Id " + Ids + "";
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
,''ShippingAddr1,''ShippingAddr2,''ShippingLocation,''ShippingPinCode,''ShippingState,''SlNo,mma.StandardName ProductDescription,''IsService,ISNULL(ha.Code,h.Code) HSNcode   
,''Barcode, sm.TransactionQty Quantity,''FreeQuantity,uom.Code Unit,CONVERT(numeric(10,4),sm.TransactionRate*s.ToCurrencyRate) UnitPrice,CONVERT(numeric(10,2),sm.TransactionQty*CONVERT(numeric(10,4),sm.TransactionRate*s.ToCurrencyRate)) GrossAmount,'' Discount,''PreTaxValue
,CONVERT(numeric(10,2),sm.TransactionQty*CONVERT(numeric(10,4),sm.TransactionRate*s.ToCurrencyRate))Taxablevalue,CONVERT(numeric(10,2),ISNULL(TAxInfo1.Percentage,0)+ISNULL(TAxInfo2.Percentage,0)+ISNULL(TAxInfo3.Percentage,0)) GSTRate,CONVERT(numeric(10,2),TAxInfo1.Amount*s.ToCurrencyRate) IgstAmt,CONVERT(numeric(10,2),TAxInfo2.Amount*s.ToCurrencyRate) SgstAmt,CONVERT(numeric(10,2),TAxInfo3.Amount*s.ToCurrencyRate) CgstAmt,'' CessRate,''CessAmtAdval
,''CessNonAdvalAmt,''StateCessRate,''StateCessAdvalAmt,''StateCessNonAdvalAmt,CONVERT(numeric(10,2),TAxInfo4.TaxAmount) OtherCharges,CONVERT(numeric(10,2),(sm.NetAmount*s.ToCurrencyRate)+ISNULL(TAxInfo4.TaxAmount,0)) ItemTotal,''BatchName,''BatchExpiryDt,''WarrantyDt
,CONVERT(numeric(10,2),sm.NetAmount*s.ToCurrencyRate)TotalInvoicevalue,''ShippingBillNo,''ShippingBillDt,''[Port],''Refundclaim,''ForeignCurrency,''CountryCode,''ExportDutyAmount,''TransID,''TransName 
,''TransMode,''Distance,''TransDocNo,''TransDocDate,''VehicleNo,''VehicleType,''ErrorListst
  FROM TRN.Sales S
LEFT JOIN TRN.SalesMaterial AS sm ON sm.SalesId=s.Id 
LEFT JOIN MST.MaterialMaster AS mm ON sm.MaterialMasterId=mm.Id
LEFT JOIN MST.MaterialMasterArticle AS mma ON sm.ArticleId=mma.Id
LEFT JOIN HKP.HSNCode AS h ON h.Id = mm.HSNCodeId
LEFT JOIN HKP.HSNCode AS ha ON ha.Id = mma.HSNCodeId
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
LEFT JOIN (
SELECT A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage,A.TaxAmount 
FROM TRN.[SalesAdditionalTax] A
LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
WHERE B.Code='TCS'
) TAxInfo4	ON TAxInfo4.SalesId=s.Id
WHERE sm.Id IN(" + Ids + ")";
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
                    if (IgstAmt != 0)
                    {
                        sheet1.Range[xlsRow, colIgstAmts].Text = Convert.ToString(IgstAmt);
                    }
                    else
                    {
                        sheet1.Range[xlsRow, colIgstAmts].Text = null;
                    }
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
                    //sheet1.Range[xlsRow, colOtherChargess].Text = dtdata.Rows[i]["OtherCharges"].ToString();
                    sheet1.Range[xlsRow, colOtherChargess].Text = null;
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
                                    ,C.UserName Company, '' CalculatedTime
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
                                WHERE E.EmployeeStatus='Active' AND E.EmpType<>'Guest'   
								Order by EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetAllGoodWorkEmployeeData()
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
                                    ,C.UserName Company, '' CalculatedTime
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
                                WHERE E.EmployeeStatus='Active' AND E.EmpType<>'Guest'  
								AND PR.Id IN(select GoodWorkPositionCodeId from org.position WHERE GoodWorkPositionCodeId<>'')
								Order by EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPayableCreationEmployeeData()
        {
            try
            {
                string CmdText = @"SELECT E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeCode,E.EmployeeName
							    	,PMB.Code BudgetCode
							    	,PR.UserName PositionName 
                                    ,E.DepartmentId
                                    ,E.DivisionId
									,E.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId 
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
                                    ,E.EmployeeCode 
                                    ,P.UserName Plant
									,SS.UserName SubSection 
                                     ,isnull( L.UserName,'') Line,EC.UserName EmployeeCategory
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
								LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
								left join [MST].[DesignationMaster] DM on DM.DesignationId=E.GivenDesignationId
								left join [HKP].[EmployeeCategory] EC on EC.Id=DM.EmployeeCategoryId
                                WHERE E.EmployeeStatus='Active' AND E.EmpType<>'Guest'  
								AND PR.Id IN(select GoodWorkPositionCodeId from org.position WHERE GoodWorkPositionCodeId<>'')
								Order by EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetAllActiveEmployeeData()
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
                                    ,C.UserName Company, '' CalculatedTime
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
                                WHERE E.EmployeeStatus='Active' AND E.EmpType<>'Guest'  
								Order by EmployeeCodeNumeric";
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

        public List<Dictionary<string, object>> GetMasterOrderSalesPostedList(string companyGroupId, string companyId, string plantId, string column, string value, string FromDate, string ToDate)
        {
            try
            {
                string fe = "";
                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                {
                    fe = "AND convert(date,S.AddedDate) between '" + FromDate + "' AND '" + ToDate + "'";
                }
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        select * from (SELECT S.Id,S.Id AS SalesId,S.InvoiceStatus, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, S.CurrencyId, C.Code AS CurrencyCode, S.DocRefNo, ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0) AS Amount,
									Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,
									Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
                                    , S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId, S.BaseNoOfDays, S.BaseOnDueDate
									, S.InvoiceNo, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
									, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, S.MatureDate, S.ToCurrencyRate
									, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId
                                    , CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark
									,V.VoucherNo,PAG.UserName PartyAccountGroup,P.PartyNature , PSI.BankDocRef
,ContractNo=Stuff((
                    SELECT distinct',' + C.ContractNo
                    FROM  dbo.[Contract] C 
					LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN [TRN].[SalesMaterial] SM ON SM.SalesOrderId=SO.Id
                    WHERE S.Id = SM.SalesId
                    FOR XML PATH('')
                    ), 1, 1, '')
					,LCRef=Stuff((
                    SELECT distinct',' + MLC.LCRef
                    FROM  dbo.[Contract] C 
					LEFT JOIN dbo.[MasterLC] MLC ON MLC.Id=C.MasterLCId
					LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN [TRN].[SalesMaterial] SM ON SM.SalesOrderId=SO.Id
                    WHERE S.Id = SM.SalesId
                    FOR XML PATH('')
                    ), 1, 1, '')
					,BenificiaryBankId=Stuff((
                    SELECT distinct',' + MLC.BenificiaryBankId
                    FROM  dbo.[Contract] C 
					LEFT JOIN dbo.[MasterLC] MLC ON MLC.Id=C.MasterLCId
					LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN [TRN].[SalesMaterial] SM ON SM.SalesOrderId=SO.Id
                    WHERE S.Id = SM.SalesId
                    FOR XML PATH('')
                    ), 1, 1, '')
					,MLCRef=Stuff((
										SELECT distinct',' + LC.LCRef
										FROM dbo.MasterLC LC 
										LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
										LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
										LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
										WHERE SM.SalesId=S.Id
										FOR XML PATH('')
										), 1, 1, '')
									,PSI.DocDeliveryDate
									FROM [TRN].[Sales] AS S
									left join PostSalesInvoice PSI on PSI.SalesId = S.Id
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
									LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'
									LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PT.AddressMasterId
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId=S.Id
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId=S.Id
                                    WHERE S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + "' AND S.PlantId='" + plantId + "' AND S.VoucherId<>'' AND S.SourceType IN('MasterOrderSales','Packing') " + fe + " AND S.IsAdditionalInfoApplicable=1" +
                                    ") AS TEMP WHERE " + strkey + " order by PostingDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetMasterOrderSalesPostedDataList(string plantId, string FromDate, string ToDate, string Ids)
        {
            try
            {
                string sql = @"Select A.InvoiceNo,A.VoucherNo,A.PartyCode,A.PartyName,A.PartyAccountGroup,A.BillTo,A.DocRefNo,A.CurrencyCode,A.Amount,A.Value,A.UserName 
into #tempOT from
(

SELECT S.Id,S.Id AS SalesId, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, S.CurrencyId, C.Code AS CurrencyCode, S.DocRefNo, ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0) AS Amount,
Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,
Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
, S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId, S.BaseNoOfDays, S.BaseOnDueDate
, S.InvoiceNo, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, S.MatureDate, S.ToCurrencyRate
, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId
, CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark,SAI.Value,AI.UserName
,V.VoucherNo,PAG.UserName PartyAccountGroup
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
LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'
LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId
LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PT.AddressMasterId
LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId=S.Id
LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId=S.Id
LEFT JOIN dbo.SalesAdditionalInfo SAI ON SAI.SalesId=S.Id
LEFT JOIN hkp.AdditionalInfo AI ON AI.Id=SAI.AdditionalInfoId
WHERE  S.PlantId='" + plantId + @"' AND S.VoucherId<>'' AND S.SourceType IN('MasterOrderSales','Packing') 
AND convert(date,S.AddedDate) between '" + FromDate + @"' AND '" + ToDate + @"' AND S.IsAdditionalInfoApplicable=1
AND S.Id " + Ids + @"
)A
DECLARE @sql nvarchar(max), @col nvarchar(max)
                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(REPLACE(CONVERT(VARCHAR(40), UserName, 113), ' ', ' '))    
                                FROM #tempOT 
                                FOR XML PATH ('')
                            ) SELECT @sql = N'
                            (SELECT * FROM #tempOT PIVOT (MAX([Value]) FOR [UserName] IN ('+STUFF(@col,1,1,'')+')) as pvt)' 
                            EXEC sp_executesql @sql
                            drop table #tempOT";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public DataTable GetInventorySalesReportData(string CompanyGroupId, string CompanyId, string PlantId, string fromDate, string toDate, string Qty, string Amount, string Summary, string Type, string partyId)
        {

            var CusAll = "";
            if (partyId != "null")
            {
                CusAll = "where x.PartyId = '" + partyId + @"'";
            }

            var sql = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (Summary == "Details")
                {

                    if (Type == "ForThePeriod")
                    {
                        sql = @"Select * from(
								SELECT 
								ROW_NUMBER() Over(Order by   SM.Id) As[S.N]
								,CASE WHEN SA.SourceType='Sales' THEN 'MaterialSales'
									WHEN SA.SourceType='Packing' THEN 'PackingwiseSales'
									ELSE  SA.SourceType END SourceType
								,SM.Id
								,SM.SalesId
								,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
								,SM.SalesOrderId
								,MO.Id MasterOrderId
								,SO.Id SONo
								,po.PONumber
								,PPI.UserName AS BillTo
								,AM.Address1 as  BillToAddress
								,ST.UserName as  BillToState
								,PPI.GSTIN as BillToGSTNo
								,PPD.UserName AS ShipTo
								,AMD.Address1 as ShipToAddress
								,STD.UserName as ShipToState
		                        ,PPD.GSTIN as ShipToGSTNo
								, SA.ToCurrencyRate
								, SA.DocRefNo
								,FORMAT(SA.InvoiceDate,'dd-MMM-yyyy') DocDate
								,'' PartyType,SA.PartyId,P.UserName AS PartyName,p.Code
								,MGM.UserName AS MaterialGroupMasterName
								,MM.UserName MaterialMasterName
								,ART.StandardName AS MaterialMasterArticleName
								,FCV.UserName FirstCharacteristicsValue
								,SCV.UserName SecondCharacteristicsValue
								,TCV.UserName ThirdCharacteristicsValue
								--,'' HSNCode
								,SM.TransactionRate
								,SM.TransactionQty
								,CU.Code AS Currency
								,SM.TransactionAmount
								,SM.BooksCurrencyBaseRate
								,SM.TransactionAmount*ISNULL(SA.ToCurrencyRate,1) BooksAmount
								,SM.TaxAmount
								,SM.NetAmount
								,SM.NetAmount * SA.ToCurrencyRate NetBookValue
								,v.VoucherNo VoucherDetailId
								,BUoM.UserName AS BaseUoM
								,TUoM.UserName AS TransactionUoM
								
								,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate
								,DT.UserName DestinationName
								,SO.SOType
								,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesService] WHERE SalesId=SA.Id)/nullif((Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id),0))*SM.TransactionAmount
								,ServiceTax=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesTax] WHERE SalesId=SA.Id  AND SalesServiceId<>'')/nullif((Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id),0))*SM.TransactionAmount
								,E.UserName Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,Posted=CASE WHEN SA.VoucherId IS NULL THEN 'No' ELSE 'YES'  END
								,ISNULL(SA.Narration,'') NoteForAccounts
								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage

		                        ,PSI.CNFContainerNo ContainerNo,PSI.TransportDriverName as TransporterName,PSI.TransportDocRefNo 
								,FORMAT( PSI.TransportDocDate, 'dd-MMM-yyyy')TransportDocDate,Agent.UserName as AgentName
								,''AgentCommission
								,'' Insurance
								,PSI.CargoGrossWt GrossWeight,''LoTNo
								,CON.ContractNo
								,ML.LCRef MasterLcNo
								,SA.ComercialInvoiceNo
								,FORMAT(PSI.ExpDate,'dd-MMM-yyyy') ExpiryDate
								,PSI.CNFBLAWB BLAWBNo,FORMAT(PSI.CNFBLAWBDate,'dd-MMM-yyyy') BLAWBDate
								,PTM.UserName PaymentTerm,FORMAT(SA.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate
								,SA.BaseNoOfDays NoOfDays
							    ,FORMAT(SA.MatureDate,'dd-MMM-yyyy') MatureDate
								,PL.Amount LCAmount
								,FORMAT(PSI.ExFactoryDate,'dd-MMM-yyyy') ExFactoryDate
								,TA.UserName TransportAgent	

								,CNfA.UserName CNFAgent
								,PSI.CNFContainerNo
								,PSI.CNFVesselTrackingNo
								, OwnReferenceNo=STUFF((select distinct ','+MO.OwnReferenceNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													, RealizeAmount=isnull(I.WrittenOffAmount,0)

									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
								                where XI.VoucherId=SA.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									--, BalanceAmount=isnull(ISNULL(SM.TransactionAmount,0) - ISNULL(I.WrittenOffAmount,0),0)
									,(Select Stuff((
									Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
									from dbo.ProductLibraryAttribute pla
									where pla.ProductLibraryId = pll.Id
									for XML PATH('')
									) , 1, 2, '')) as PordDertails , 
 
									(Select Stuff((
									Select ', ' + sc.LotNo
									from (Select distinct sc.LotNo
									from dbo.SalesPacking spss
									left join trn.Packing p on p.PackingId = spss.PackingId
									left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
									left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
									left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
									where spss.SalesId = SM.SalesId) as sc
									for XML PATH('')
									),1,2,''))  as LOT
									, BuyerRefNo=STUFF((select distinct ','+MO.BuyerReferenceNo
									from trn.SalesMaterial SMX									 
									join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
									LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
									LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
									(Select Count(sc.RefNo)  as Bags
									from dbo.SalesPacking sp
									left join trn.Packing p on p.PackingId = sp.PackingId
									left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
									left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
									left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
									where sp.SalesId = SM.SalesId) as Bags,
									Convert(varchar , (Select SUM(sc.GWeight)  as Bags
									from dbo.SalesPacking sp
									left join trn.Packing p on p.PackingId = sp.PackingId
									left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
									left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
									left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
									where sp.SalesId = SM.SalesId) ) as GrossWeights,
									PSI.TransportVehicleNo , PSI.TransportDriverNo

								FROM TRN.SalesMaterial AS SM 
								LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId

									left outer join TRN.SalesOrder So on SO.Id=SM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join dbo.ProductLibrary pll on pll.Id = moi.ProductLibraryId
									left outer join [Contract] CON on CON.Id=so.ContractId
									left outer join PurchaseLC PL on PL.ContractId=CON.Id
									Left outer join MasterLC ML on ML.Id=CON.MasterLCId
									left outer join PostSalesInvoice PSI on PSI.SalesId=SA.Id
									left outer join MST.PaymentTerm PTM on PTM.Id=SA.PaymentTermId

									left outer join HKP.Party CNfA on CNfA.Id=SA.PartyId
									left outer join HKP.Party TA on TA.Id=SA.PartyId


						--LEFT JOIN [TRN].[SalesOrder] AS SO ON SM.SalesOrderId=SO.Id
						--LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
						LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
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
						LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
						LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
						LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
						LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
						LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
						LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
						LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
						LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
						LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
						LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
						Left JOIN [ORG].[Entity] E On E.id= SA.EntityId
						LEFT JOIN (SELECT SUM(Amount) Amount,SUM(WrittenOffAmount) WrittenOffAmount,VoucherId 
										FROM TRN.Invoice GROUP BY VoucherId) I ON I.VoucherId=SA.VoucherId
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST' and A.SalesServiceId IS NULL
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								   ) TAxInfo	ON TAxInfo.SalesMaterialId=SM.Id 
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST' and A.SalesServiceId IS NULL	
									) TAxInfo1	ON TAxInfo1.SalesMaterialId=SM.Id 
							  		 
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST' and A.SalesServiceId IS NULL	
									) TAxInfo2	ON TAxInfo2.SalesMaterialId=SM.Id 

						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS' and A.SalesServiceId IS NULL									
									) TAxInfo3	ON TAxInfo3.SalesMaterialId=SM.Id 


							
					
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS' and A.SalesServiceId IS NULL		 
								
						) TAxInfo6 ON TAxInfo6.SalesMaterialId=SM.Id 
						LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
                        --LEFT JOIN PostSalesInvoice PSI On PSI.SalesId=SA.Id
						LEFT JOIN HKP.Party as Agent on Agent.Id=PSI.TransportAgentId

								WHERE SA.PlantId='" + identity.PlantId + @"' 
								AND convert(Date,SA.InvoiceDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' 

									UNION ALL

														Select                  
								ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
								,IR.SourceType
								,ISs.Id
								,IR.Id SalesId
								,FORMAT(IR.EntryDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,'' SalesOrderId
								,'' MasterOrderId
								,'' SONo
								,'' PONumber
								,'' AS BillTo
								,'' as BillToAddress
								,'' as BillToState
								,'' as BillToGSTNo
								,'' AS ShipTo
								,'' AS ShipToAddress
								,'' AS ShipToState
								,'' as ShipToGSTNo
								, 0 ToCurrencyRate
								, '' DocRefNo
								,FORMAT(IR.InvoiceDate,'dd-MMM-yyyy') DocDate
								,'' PartyType,IR.PartyId, P.UserName AS PartyName,p.Code
								,'' AS MaterialGroupMasterName
								,SM.UserName MaterialMasterName
								,'' AS MaterialMasterArticleName
								,''FirstCharacteristicsValue
								,'' SecondCharacteristicsValue
								,'' ThirdCharacteristicsValue
								--, '' HSNCode
								,0 TransactionRate
								,0 TransactionQty
								,''  Currency
								,ISs.Amount TransactionAmount
								,IRM.BooksCurrencyBaseRate
								,ISs.Amount*ISNULL(ToCurrencyRate,1) BooksAmount
								,ISs.TaxAmount
								,0 NetAmount
								,0 NetBookValue
								,'' VoucherDetailId
								,''  BaseUoM
								,''  TransactionUoM
								
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,0 ServiceCharge
								, 0 ServiceTax
								,E.UserName Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,'' Posted
								,'' 'NoteForAccounts'

								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
								,''ContainerNo ,''TransporterName,''TransportDocRefNo 
								,FORMAT(PSI.TransportDocDate,'dd-MMM-yyyy') TransportDocDate,''AgentName
								,''AgentCommission
								,'' Insurance
								,''GrossWeight,''LoTNo
								,CON.ContractNo
								,ML.LCRef MasterLcNo
								,IR.ComercialInvoiceNo
								,FORMAT(PSI.ExpDate,'dd-MMM-yyyy') ExpiryDate
								,PSI.CNFBLAWB BLAWBNo,FORMAT(PSI.CNFBLAWBDate,'dd-MMM-yyyy') BLAWBDate
								,PTM.UserName PaymentTerm,FORMAT(IR.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate
								,IR.BaseNoOfDays NoOfDays
							    ,FORMAT(IR.MatureDate,'dd-MMM-yyyy') MatureDate
								,PL.Amount LCAmount
								,FORMAT(PSI.ExFactoryDate,'dd-MMM-yyyy') ExFactoryDate
								,TA.UserName TransportAgent	

								,CNfA.UserName CNFAgent
								,PSI.CNFContainerNo
								,PSI.CNFVesselTrackingNo
								, OwnReferenceNo=STUFF((select distinct ','+MO.OwnReferenceNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                where smx.SalesId=IR.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													, RealizeAmount=isnull(I.WrittenOffAmount,0)

									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
								                where XI.VoucherId=IR.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,(Select Stuff((
								Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
								from dbo.ProductLibraryAttribute pla
								where pla.ProductLibraryId = pll.Id
								for XML PATH('')
								) , 1, 2, '')) as PordDertails  , 
								(Select Stuff((
								Select ', ' + sc.LotNo
								from (Select distinct sc.LotNo
								from dbo.SalesPacking spss
								left join trn.Packing p on p.PackingId = spss.PackingId
								left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
								left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
								left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
								where spss.SalesId = IR.Id) as sc
								for XML PATH('')
								),1,2,''))  as LOT
								, BuyerRefNo=STUFF((select distinct ','+MO.BuyerReferenceNo
								from trn.SalesMaterial SMX									 
								join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
								LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
								LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
								where smx.SalesId=IR.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
								(Select Count(sc.RefNo)  as Bags
								from dbo.SalesPacking sp
								left join trn.Packing p on p.PackingId = sp.PackingId
								left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
								left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
								left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
								where sp.SalesId = IR.Id) as Bags,
								Convert(varchar , (Select SUM(sc.GWeight)  as Bags
								from dbo.SalesPacking sp
								left join trn.Packing p on p.PackingId = sp.PackingId
								left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
								left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
								left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
								where sp.SalesId = IR.Id) ) as GrossWeights,
								PSI.TransportVehicleNo , PSI.TransportDriverNo

									--, BalanceAmount=isnull(ISNULL(ISs.Amount,0)- ISNULL(I.WrittenOffAmount,0),0)

								from trn.SalesService AS ISs
								LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
								left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
								left outer join trn.SalesMaterial IRM on IRM.SalesId=IR.Id
									left outer join TRN.SalesOrder So on SO.Id=IRM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join dbo.ProductLibrary pll on pll.ID = moi.ProductLibraryId
									left outer join [Contract] CON on CON.Id=SO.ContractId
									left outer join PurchaseLC PL on PL.ContractId=CON.Id
									Left outer join MasterLC ML on ML.Id=CON.MasterLCId
									left outer join PostSalesInvoice PSI on PSI.SalesId=IR.Id
									left outer join MST.PaymentTerm PTM on PTM.Id=IR.PaymentTermId

									left outer join HKP.Party CNfA on CNfA.Id=IR.PartyId
									left outer join HKP.Party TA on TA.Id=IR.PartyId

						LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
						left join trn.Voucher V on V.Id=I.VoucherId
						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
						left join trn.Voucher V1 on V1.Id=ep.VoucherId
						Left JOIN [ORG].[Entity] E On E.id= IR.EntityId
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST'  
									
									) TAxInfo	ON TAxInfo.SalesServiceId=ISs.Id AND TAxInfo.SalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST'  
									) TAxInfo1	ON TAxInfo1.SalesServiceId=ISs.Id AND TAxInfo1.SalesServiceId IS NOT NULL 
							  		 
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST'  

											) TAxInfo2	ON TAxInfo2.SalesServiceId=ISs.Id AND TAxInfo2.SalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS'  
									) TAxInfo3	ON TAxInfo3.SalesServiceId=ISs.Id AND TAxInfo3.SalesServiceId IS NOT NULL


						
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS'
						) TAxInfo6 ON TAxInfo6.SalesServiceId=ISs.Id AND TAxInfo6.SalesServiceId IS NOT NULL

								WHERE IR.PlantId='" + identity.PlantId + @"' 
								AND convert(Date,IR.InvoiceDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' 
								union ALL

								SELECT 
								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,'InventorySales' SourceType
								,IID.Id
								,II.Id SalesId
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,FORMAT(II.SalesDate, 'dd-MMM-yyyy') InvoiceDate
								,'' SalesOrderId
								,'' MasterOrderId
								,'' SONo
								,'' PONumber
								,PPI.UserName AS BillTo
								,AM.Address1 as BillToAddress
								,ST.UserName as BillToState				
								,PPI.GSTIN as BillToGSTNo
								,PPI1.UserName ShipTo
								,AM1.Address1 ShipToAddress
								,ST1.UserName ShipToState
								,PPI1.GSTIN ShipToGSTNo
								,II.ToCurrencyRate
								, II.DocRefNo
								,II.CustomerId PartyId
								,FORMAT(II.DocDate, 'dd-MMM-yyyy') DocDate
								,'' PartyType, P.UserName AS PartyName,p.Code
								,MGM.UserName AS MaterialGroupMasterName
								,MM.UserName MaterialMasterName
								,ART.StandardName AS MaterialMasterArticleName
								, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue							
								, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue						
								, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
								--, ISNULL(TAxInfo.HSCode,'') HSNCode
								,IID.SalesRate TransactionRate
								,IID.TransactionQty 
								,CU.Code AS Currency
								,IID.TransactionQty *IID.SalesRate TransactionAmount
								,0 BooksCurrencyBaseRate
								,(IID.TransactionQty *IID.SalesRate)*ISNULL(II.ToCurrencyRate,1) BooksAmount
								,SCr1.TaxAmount TaxAmount
								,IID.[TotalSalesAmount] NetAmount
								,IID.[BooksCurrencyTransactionAmount] NetBookValue
								,II.VoucherId VoucherDetailId
								,TUoM.UserName AS BaseUoM
								,TUoM.UserName AS TransactionUoM
								
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,SCr.Amount ServiceCharge
								,SCr.TotalTaxAmount ServiceTax

								,E.UserName AS Entity 
								,EI2.EmployeeName CheckedByName
								,II.CheckedBy
								,EI1.EmployeeName ApprovedByName
								,II.ApprovedBy
								,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
								,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'

								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
								,''ContainerNo ,''TransporterName,''TransportDocRefNo 
								,''TransportDocDate,''AgentName
								,''AgentCommission
								,'' Insurance
								,''GrossWeight,''LoTNo
								,''ContractNo
								,''MasterLcNo
								,''ComercialInvoiceNo
								,''ExpiryDate
								,''BLAWBNo,''BLAWBDate
								,''PaymentTerm,''BaseOnDueDate
								,0NoOfDays
							    ,''MatureDate
								,0LCAmount
								,''ExFactoryDate
								,''TransportAgent	

								,''CNFAgent
								,''CNFContainerNo
								,''CNFVesselTrackingNo
								,''OwnReferenceNo
								,0 RealizeAmount
								,''RealizeDate,'' PordDertails  ,'' LOT ,'' BuyerRefNo,'' Bags,'' GrossWeights,
								'' TransportVehicleNo , '' TransportDriverNo

								--,0BalanceAmount

								FROM[TRN].[InventorySalesDetail] AS IID
								left outer join [TRN].[InventorySales] AS II on II.Id=IID.InventorySalesId
								left JOIN [TRN].[InventorySalesHistory] AS ISH on ISH.InventorySalesDetailId=IID.ID
								left JOIN [TRN].[InventoryReceiveDetail] AS IRD on ISH.InventoryReceiveDetailId=IRD.ID
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN SCS.Currency AS CU ON CU.Id=II.CurrencyId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
								LEFT JOIN [SCS].[State] as ST on ST.Id=AM.StateId

						LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
						LEFT JOIN [MST].[AddressMaster] AS AM1 ON AM1.Id=PPI1.AddressMasterId
						LEFT JOIN [SCS].[State] as ST1 on ST1.Id=AM1.StateId
						Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
						Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
						Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
						Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
						Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId
						--Left Join [HKP].[Party] Par As Par.Id=II.P
						LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IID.InventoryMaterialId
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					--	LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id			
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						LEFT JOIN(Select sum(Amount) Amount, sum(TotalTaxAmount) TotalTaxAmount, InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
						LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id
			LEFT JOIN (SELECT A.InventorySalesDetailId, A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId

									WHERE B.Code='CGST' and A.InventorySalesServiceId IS NULL	
									group by A.InventorySalesId, B.UserName ,B.Code,A.InventorySalesDetailId								
								   ) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId and TAxInfo.InventorySalesDetailId=IID.Id
						LEFT JOIN (SELECT A.InventorySalesDetailId, A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId

									WHERE B.Code='IGST' and A.InventorySalesServiceId IS NULL	
									group by A.InventorySalesId, B.UserName ,B.Code,A.InventorySalesDetailId									
									) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId and TAxInfo1.InventorySalesDetailId=IID.Id 
							  		 
						LEFT JOIN (SELECT A.InventorySalesDetailId, A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId

									WHERE B.Code='SGST' and A.InventorySalesServiceId IS NULL	
									group by A.InventorySalesId, B.UserName ,B.Code,A.InventorySalesDetailId										
									) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId and TAxInfo2.InventorySalesDetailId=IID.Id 

						LEFT JOIN (SELECT A.InventorySalesDetailId, A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId

									WHERE B.Code='TDS' and A.InventorySalesServiceId IS NULL	
									group by A.InventorySalesId, B.UserName ,B.Code,A.InventorySalesDetailId						
									) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId and TAxInfo3.InventorySalesDetailId=IID.Id 							
					
						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount FROM 
									[TRN].InventorySalesAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' 	
									Group by A.InventorySalesId, B.UserName ,B.Code 
						) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
						
						WHERE II.PlantId='" + identity.PlantId + @"' 
						AND convert(Date,II.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' 
					

								UNION ALL

								Select                  
								ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
								,'InventorySales' SourceType
								,ISs.Id
								,IR.Id SalesId
								,FORMAT(IR.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,'' SalesOrderId
								,'' MasterOrderId
								,'' SONo
								,'' PONumber
								,'' AS BillTo
								,'' AS BillToAddress
								,'' AS BillToState
								,'' as BillToGSTNo
								,'' AS ShipTo
								,'' AS ShipToAddress
								,'' AS ShipToState	
								,'' as ShipToGSTNo
								, 0 ToCurrencyRate
								, '' DocRefNo
								,FORMAT(IR.DocDate,'') DocDate
								,'' PartyType, P.UserName AS PartyName,p.Code
								,'' AS MaterialGroupMasterName
								,SM.UserName MaterialMasterName
								,'' AS MaterialMasterArticleName
								,''FirstCharacteristicsValue
								,'' SecondCharacteristicsValue
								,'' ThirdCharacteristicsValue
								--, '' HSNCode
								,IR.CustomerId PartyId
								,0 TransactionRate
								,0 TransactionQty
								,'' AS Currency
								,ISs.Amount TransactionAmount
								,0 BooksCurrencyBaseRate
								,ISs.Amount*ISNULL(ToCurrencyRate,1) BooksAmount
								,0 TaxAmount
								,ISs.Amount NetAmount
								,ISs.Amount NetBookValue
								,'' VoucherDetailId
								,'' AS BaseUoM
								,'' AS TransactionUoM
								
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,0 ServiceCharge
								,0 ServiceTax
								,E.UserName Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,'' Posted
								,'' 'NoteForAccounts'

						,round(isnull(TAxInfo.TaxAmount,0),2)  CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
						,''ContainerNo ,''TransporterName,''TransportDocRefNo 
						,''TransportDocDate,''AgentName
						,''AgentCommission
						,'' Insurance
						,''GrossWeight,''LoTNo
						,''ContractNo
						,''MasterLcNo
						,''ComercialInvoiceNo
						,''ExpiryDate
						,''BLAWBNo,''BLAWBDate
						,''PaymentTerm,''BaseOnDueDate
						,0 NoOfDays
					    ,''MatureDate
						,0 LCAmount
						,''ExFactoryDate
						,''TransportAgent	
						
						,''CNFAgent
						,''CNFContainerNo
						,''CNFVesselTrackingNo
						,''OwnReferenceNo
						,0 RealizeAmount
					    ,''RealizeDate,'' PordDertails  ,'' LOT ,'' BuyerRefNo,'' Bags,'' GrossWeights,
						'' TransportVehicleNo , '' TransportDriverNo

							--,0BalanceAmount
						from trn.InventoryService AS ISS
						LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
						left jOIN [TRN].[InventorySales] AS IR ON IR.Id=ISs.InventoryReceiveId
						LEFT JOIN HKP.Party AS P ON P.Id=IR.CustomerId
						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
						left join trn.Voucher V on V.Id=I.VoucherId
						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
						left join trn.Voucher V1 on V1.Id=ep.VoucherId
						Left JOIN [ORG].[Entity] E On E.id= IR.EntityId
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST'  
									Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
									--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
									) TAxInfo	ON TAxInfo.InventorySalesServiceId=ISs.Id AND TAxInfo.InventorySalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST'  
									Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
									) TAxInfo1	ON TAxInfo1.InventorySalesServiceId=ISs.Id AND TAxInfo1.InventorySalesServiceId IS NOT NULL 
							  		 
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST'  
                                    Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
											) TAxInfo2	ON TAxInfo2.InventorySalesServiceId=ISs.Id AND TAxInfo2.InventorySalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS' 
                                    Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
									) TAxInfo3	ON TAxInfo3.InventorySalesServiceId=ISs.Id AND TAxInfo3.InventorySalesServiceId IS NOT NULL
							
					
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,Sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS' 
                                    Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
						) TAxInfo6 ON TAxInfo6.InventorySalesServiceId=ISs.Id AND TAxInfo6.InventorySalesServiceId IS NOT NULL

								WHERE  IR.PlantId='" + identity.PlantId + @"' 
								AND convert(Date,IR.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"')x
								 " + CusAll + "";
                        return _sqlRepository.GetDataTable(sql);
                    }
                    else
                    {
                        sql = @"Select * from( 
								SELECT 
								ROW_NUMBER() Over(Order by   SM.Id) As[S.N]
								,CASE WHEN SA.SourceType='Sales' THEN 'MaterialSales'
									WHEN SA.SourceType='Packing' THEN 'PackingwiseSales'
									ELSE  SA.SourceType END SourceType
								,SM.Id
								,SM.SalesId,SM.BooksCurrencyBaseRate
								,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
								,SM.SalesOrderId
								,MO.Id MasterOrderId
								,SO.Id SONo
								,po.PONumber
								,PPI.UserName AS BillTo
								,AM.Address1 as  BillToAddress
								,ST.UserName as  BillToState
								,PPI.GSTIN as BillToGSTNo
								,PPD.UserName AS ShipTo
								,AMD.Address1 as ShipToAddress
								,STD.UserName as ShipToState
		                        ,PPD.GSTIN as ShipToGSTNo
								, SA.ToCurrencyRate
								, SA.DocRefNo
								,FORMAT(SA.InvoiceDate,'dd-MMM-yyyy') DocDate
								,'' PartyType,SA.PartyId, P.UserName AS PartyName,p.Code
								,MGM.UserName AS MaterialGroupMasterName
								,MM.UserName MaterialMasterName
								,ART.StandardName AS MaterialMasterArticleName
								,FCV.UserName FirstCharacteristicsValue
								,SCV.UserName SecondCharacteristicsValue
								,TCV.UserName ThirdCharacteristicsValue
								--,'' HSNCode
								,SM.TransactionRate
								,SM.TransactionQty
								,SM.TransactionAmount
								,SM.TransactionAmount*ISNULL(SA.ToCurrencyRate,1) BooksAmount
								,SM.TaxAmount
								,SM.NetAmount
								,SM.NetAmount * SA.ToCurrencyRate NetBookValue
								,v.VoucherNo VoucherDetailId
								,BUoM.UserName AS BaseUoM
								,TUoM.UserName AS TransactionUoM
								,CU.Code AS Currency
								,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate
								,DT.UserName DestinationName
								,SO.SOType
								,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesService] WHERE SalesId=SA.Id)/nullif((Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id),0))*SM.TransactionAmount
								,ServiceTax=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesTax] WHERE SalesId=SA.Id  AND SalesServiceId<>'')/nullif((Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id),0))*SM.TransactionAmount
								,E.UserName Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,Posted=CASE WHEN SA.VoucherId IS NULL THEN 'No' ELSE 'YES'  END
								,ISNULL(SA.Narration,'') NoteForAccounts
								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage

		                        ,PSI.CNFContainerNo ContainerNo,PSI.TransportDriverName as TransporterName,PSI.TransportDocRefNo 
								,FORMAT( PSI.TransportDocDate, 'dd-MMM-yyyy')TransportDocDate,Agent.UserName as AgentName
								,''AgentCommission
								,'' Insurance
								,PSI.CargoGrossWt GrossWeight,''LoTNo
								,CON.ContractNo
								,ML.LCRef MasterLcNo
								,SA.ComercialInvoiceNo
								,FORMAT(PSI.ExpDate,'dd-MMM-yyyy') ExpiryDate
								,PSI.CNFBLAWB BLAWBNo,FORMAT(PSI.CNFBLAWBDate,'dd-MMM-yyyy') BLAWBDate
								,PTM.UserName PaymentTerm,FORMAT(SA.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate
								,SA.BaseNoOfDays NoOfDays
							    ,FORMAT(SA.MatureDate,'dd-MMM-yyyy') MatureDate
								,PL.Amount LCAmount
								,FORMAT(PSI.ExFactoryDate,'dd-MMM-yyyy') ExFactoryDate
								,TA.UserName TransportAgent	

								,CNfA.UserName CNFAgent
								,PSI.CNFContainerNo
								,PSI.CNFVesselTrackingNo
								, OwnReferenceNo=STUFF((select distinct ','+MO.OwnReferenceNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													, RealizeAmount=isnull(I.WrittenOffAmount,0)

									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
								                where XI.VoucherId=SA.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),(Select Stuff((
												Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
												from dbo.ProductLibraryAttribute pla
												where pla.ProductLibraryId = pll.Id
												for XML PATH('')
												) , 1, 2, '')) as PordDertails , 
 
												(Select Stuff((
												Select ', ' + sc.LotNo
												from (Select distinct sc.LotNo
												from dbo.SalesPacking spss
												left join trn.Packing p on p.PackingId = spss.PackingId
												left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
												left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
												left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
												where spss.SalesId = SM.SalesId) as sc
												for XML PATH('')
												),1,2,''))  as LOT
												, BuyerRefNo=STUFF((select distinct ','+MO.BuyerReferenceNo
												from trn.SalesMaterial SMX									 
												join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
												where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
												(Select Count(sc.RefNo)  as Bags
												from dbo.SalesPacking sp
												left join trn.Packing p on p.PackingId = sp.PackingId
												left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
												left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
												left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
												where sp.SalesId = SM.SalesId) as Bags,
												Convert(varchar , (Select SUM(sc.GWeight)  as Bags
												from dbo.SalesPacking sp
												left join trn.Packing p on p.PackingId = sp.PackingId
												left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
												left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
												left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
												where sp.SalesId = SM.SalesId) ) as GrossWeights,
												PSI.TransportVehicleNo , PSI.TransportDriverNo

								FROM TRN.SalesMaterial AS SM 
								LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId

									left outer join TRN.SalesOrder So on SO.Id=SM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join dbo.ProductLibrary pll on pll.Id = moi.ProductLibraryId
									left outer join [Contract] CON on CON.Id=so.ContractId
									left outer join PurchaseLC PL on PL.ContractId=CON.Id
									Left outer join MasterLC ML on ML.Id=CON.MasterLCId
									left outer join PostSalesInvoice PSI on PSI.SalesId=SA.Id
									left outer join MST.PaymentTerm PTM on PTM.Id=SA.PaymentTermId

									left outer join HKP.Party CNfA on CNfA.Id=SA.PartyId
									left outer join HKP.Party TA on TA.Id=SA.PartyId


						--LEFT JOIN [TRN].[SalesOrder] AS SO ON SM.SalesOrderId=SO.Id
						--LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
						LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
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
						LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
						LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
						LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
						LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
						LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
						LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
						LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
						LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
						LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
						LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
						Left JOIN [ORG].[Entity] E On E.id= SA.EntityId
						LEFT JOIN (SELECT SUM(Amount) Amount,SUM(WrittenOffAmount) WrittenOffAmount,VoucherId 
										FROM TRN.Invoice GROUP BY VoucherId) I ON I.VoucherId=SA.VoucherId
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST' and A.SalesServiceId IS NULL
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								   ) TAxInfo	ON TAxInfo.SalesMaterialId=SM.Id 
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST' and A.SalesServiceId IS NULL	
									) TAxInfo1	ON TAxInfo1.SalesMaterialId=SM.Id 
							  		 
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST' and A.SalesServiceId IS NULL	
									) TAxInfo2	ON TAxInfo2.SalesMaterialId=SM.Id 

						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS' and A.SalesServiceId IS NULL									
									) TAxInfo3	ON TAxInfo3.SalesMaterialId=SM.Id 


							
					
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS' and A.SalesServiceId IS NULL		 
								
						) TAxInfo6 ON TAxInfo6.SalesMaterialId=SM.Id 
						LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
                        --LEFT JOIN PostSalesInvoice PSI On PSI.SalesId=SA.Id
						LEFT JOIN HKP.Party as Agent on Agent.Id=PSI.TransportAgentId
						WHERE SA.PlantId='" + identity.PlantId + "' " +
                        "AND convert(Date,SA.InvoiceDate) <= '" + toDate + @"'
						UNION ALL
						
						Select                  
								ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
								,IR.SourceType
								,ISs.Id
								,IR.Id SalesId,IRM.BooksCurrencyBaseRate
								,FORMAT(IR.EntryDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,'' SalesOrderId
								,'' MasterOrderId
								,'' SONo
								,'' PONumber
								,'' AS BillTo
								,'' as BillToAddress
								,'' as BillToState
								,'' as BillToGSTNo
								,'' AS ShipTo
								,'' AS ShipToAddress
								,'' AS ShipToState
								,'' as ShipToGSTNo
								, 0 ToCurrencyRate
								, '' DocRefNo
								,FORMAT(IR.InvoiceDate,'dd-MMM-yyyy') DocDate
								,'' PartyType,IR.PartyId, P.UserName AS PartyName,p.Code
								,'' AS MaterialGroupMasterName
								,SM.UserName MaterialMasterName
								,'' AS MaterialMasterArticleName
								,''FirstCharacteristicsValue
								,'' SecondCharacteristicsValue
								,'' ThirdCharacteristicsValue
								--, '' HSNCode
								,0 TransactionRate
								,0 TransactionQty
								,ISs.Amount TransactionAmount
								,ISs.Amount*ISNULL(ToCurrencyRate,1) BooksAmount
								,ISs.TaxAmount
								,0 NetAmount
								,0 NetBookValue
								,'' VoucherDetailId
								,''  BaseUoM
								,''  TransactionUoM
								,''  Currency
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,0 ServiceCharge
								, 0 ServiceTax
								,E.UserName Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,'' Posted
								,'' 'NoteForAccounts'

								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
								,''ContainerNo ,''TransporterName,''TransportDocRefNo 
								,FORMAT(PSI.TransportDocDate,'dd-MMM-yyyy') TransportDocDate,''AgentName
								,''AgentCommission
								,'' Insurance
								,''GrossWeight,''LoTNo
								,CON.ContractNo
								,ML.LCRef MasterLcNo
								,IR.ComercialInvoiceNo
								,FORMAT(PSI.ExpDate,'dd-MMM-yyyy') ExpiryDate
								,PSI.CNFBLAWB BLAWBNo,FORMAT(PSI.CNFBLAWBDate,'dd-MMM-yyyy') BLAWBDate
								,PTM.UserName PaymentTerm,FORMAT(IR.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate
								,IR.BaseNoOfDays NoOfDays
							    ,FORMAT(IR.MatureDate,'dd-MMM-yyyy') MatureDate
								,PL.Amount LCAmount
								,FORMAT(PSI.ExFactoryDate,'dd-MMM-yyyy') ExFactoryDate
								,TA.UserName TransportAgent	

								,CNfA.UserName CNFAgent
								,PSI.CNFContainerNo
								,PSI.CNFVesselTrackingNo
								, OwnReferenceNo=STUFF((select distinct ','+MO.OwnReferenceNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                where smx.SalesId=IR.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													, RealizeAmount=isnull(I.WrittenOffAmount,0)

									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
								                where XI.VoucherId=IR.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),(Select Stuff((
											Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
											from dbo.ProductLibraryAttribute pla
											where pla.ProductLibraryId = pll.Id
											for XML PATH('')
											) , 1, 2, '')) as PordDertails  , 
											(Select Stuff((
											Select ', ' + sc.LotNo
											from (Select distinct sc.LotNo
											from dbo.SalesPacking spss
											left join trn.Packing p on p.PackingId = spss.PackingId
											left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
											left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
											left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
											where spss.SalesId = IR.Id) as sc
											for XML PATH('')
											),1,2,''))  as LOT
											, BuyerRefNo=STUFF((select distinct ','+MO.BuyerReferenceNo
											from trn.SalesMaterial SMX									 
											join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
											LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
											LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
											where smx.SalesId=IR.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											(Select Count(sc.RefNo)  as Bags
											from dbo.SalesPacking sp
											left join trn.Packing p on p.PackingId = sp.PackingId
											left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
											left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
											left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
											where sp.SalesId = IR.Id) as Bags,
											Convert(varchar , (Select SUM(sc.GWeight)  as Bags
											from dbo.SalesPacking sp
											left join trn.Packing p on p.PackingId = sp.PackingId
											left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
											left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
											left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
											where sp.SalesId = IR.Id) ) as GrossWeights,
											PSI.TransportVehicleNo , PSI.TransportDriverNo

									--, BalanceAmount=isnull(ISNULL(ISs.Amount,0)- ISNULL(I.WrittenOffAmount,0),0)

								from trn.SalesService AS ISs
								LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
								left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
								left outer join trn.SalesMaterial IRM on IRM.SalesId=IR.Id
									left outer join TRN.SalesOrder So on SO.Id=IRM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join dbo.ProductLibrary pll on pll.ID = moi.ProductLibraryId
									left outer join [Contract] CON on CON.Id=so.ContractId
									left outer join PurchaseLC PL on PL.ContractId=CON.Id
									Left outer join MasterLC ML on ML.Id=CON.MasterLCId
									left outer join PostSalesInvoice PSI on PSI.SalesId=IR.Id
									left outer join MST.PaymentTerm PTM on PTM.Id=IR.PaymentTermId

									left outer join HKP.Party CNfA on CNfA.Id=IR.PartyId
									left outer join HKP.Party TA on TA.Id=IR.PartyId

						LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
						left join trn.Voucher V on V.Id=I.VoucherId
						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
						left join trn.Voucher V1 on V1.Id=ep.VoucherId
						Left JOIN [ORG].[Entity] E On E.id= IR.EntityId
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST'  
									
									) TAxInfo	ON TAxInfo.SalesServiceId=ISs.Id AND TAxInfo.SalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST'  
									) TAxInfo1	ON TAxInfo1.SalesServiceId=ISs.Id AND TAxInfo1.SalesServiceId IS NOT NULL 
							  		 
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST'  

											) TAxInfo2	ON TAxInfo2.SalesServiceId=ISs.Id AND TAxInfo2.SalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS'  
									) TAxInfo3	ON TAxInfo3.SalesServiceId=ISs.Id AND TAxInfo3.SalesServiceId IS NOT NULL


						
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS'
						) TAxInfo6 ON TAxInfo6.SalesServiceId=ISs.Id AND TAxInfo6.SalesServiceId IS NOT NULL

								WHERE IR.PlantId='" + identity.PlantId + "' " +
                                "AND convert(Date,IR.InvoiceDate) <= '" + toDate + @"'
								UNION ALL

								SELECT 
								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,'InventorySales' SourceType
								,IID.Id
								,II.Id SalesId,0 BooksCurrencyBaseRate
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,FORMAT(II.SalesDate, 'dd-MMM-yyyy') InvoiceDate
								,'' SalesOrderId
								,'' MasterOrderId
								,'' SONo
								,'' PONumber
								,PPI.UserName AS BillTo
								,AM.Address1 as BillToAddress
								,ST.UserName as BillToState				
								,PPI.GSTIN as BillToGSTNo
								,PPI1.UserName ShipTo
								,AM1.Address1 ShipToAddress
								,ST1.UserName ShipToState
								,PPI1.GSTIN ShipToGSTNo
								,II.ToCurrencyRate
								, II.DocRefNo
								,FORMAT(II.DocDate, 'dd-MMM-yyyy') DocDate
								,'' PartyType,II.CustomerId PartyId, P.UserName AS PartyName,p.Code
								,MGM.UserName AS MaterialGroupMasterName
								,MM.UserName MaterialMasterName
								,ART.StandardName AS MaterialMasterArticleName
								, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue							
								, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue						
								, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
								--, ISNULL(TAxInfo.HSCode,'') HSNCode
								,IID.SalesRate TransactionRate
								,IID.TransactionQty 
								,IID.TransactionQty *IID.SalesRate TransactionAmount
								,(IID.TransactionQty *IID.SalesRate)*ISNULL(II.ToCurrencyRate,1) BooksAmount
								,SCr1.TaxAmount TaxAmount
								,IID.[TotalSalesAmount] NetAmount
								,IID.[BooksCurrencyTransactionAmount] NetBookValue
								,II.VoucherId VoucherDetailId
								,TUoM.UserName AS BaseUoM
								,TUoM.UserName AS TransactionUoM
								,CU.Code AS Currency
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,SCr.Amount ServiceCharge
								,SCr.TotalTaxAmount ServiceTax

								,E.UserName AS Entity 
								,EI2.EmployeeName CheckedByName
								,II.CheckedBy
								,EI1.EmployeeName ApprovedByName
								,II.ApprovedBy
								,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
								,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'

								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
								,''ContainerNo ,''TransporterName,''TransportDocRefNo 
								,''TransportDocDate,''AgentName
								,''AgentCommission
								,'' Insurance
								,''GrossWeight,''LoTNo
								,''ContractNo
								,''MasterLcNo
								,''ComercialInvoiceNo
								,''ExpiryDate
								,''BLAWBNo,''BLAWBDate
								,''PaymentTerm,''BaseOnDueDate
								,0NoOfDays
							    ,''MatureDate
								,0LCAmount
								,''ExFactoryDate
								,''TransportAgent	

								,''CNFAgent
								,''CNFContainerNo
								,''CNFVesselTrackingNo
								,''OwnReferenceNo
													,0 RealizeAmount

									,''RealizeDate,'' PordDertails  ,'' LOT ,'' BuyerRefNo,'' Bags,'' GrossWeights,
								'' TransportVehicleNo , '' TransportDriverNo

									--,0BalanceAmount

								FROM[TRN].[InventorySalesDetail] AS IID
								left outer join [TRN].[InventorySales] AS II on II.Id=IID.InventorySalesId
								left JOIN [TRN].[InventorySalesHistory] AS ISH on ISH.InventorySalesDetailId=IID.ID
								left JOIN [TRN].[InventoryReceiveDetail] AS IRD on ISH.InventoryReceiveDetailId=IRD.ID
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN SCS.Currency AS CU ON CU.Id=II.CurrencyId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
								LEFT JOIN [SCS].[State] as ST on ST.Id=AM.StateId

						LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
						LEFT JOIN [MST].[AddressMaster] AS AM1 ON AM1.Id=PPI1.AddressMasterId
						LEFT JOIN [SCS].[State] as ST1 on ST1.Id=AM1.StateId
						Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
						Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
						Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
						Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
						Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId
						--Left Join [HKP].[Party] Par As Par.Id=II.P
						LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IID.InventoryMaterialId
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					--	LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id			
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						LEFT JOIN(Select sum(Amount) Amount, sum(TotalTaxAmount) TotalTaxAmount, InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
						LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id
			LEFT JOIN (SELECT A.InventorySalesDetailId, A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId

									WHERE B.Code='CGST' and A.InventorySalesServiceId IS NULL	
									group by A.InventorySalesId, B.UserName ,B.Code,A.InventorySalesDetailId								
								   ) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId and TAxInfo.InventorySalesDetailId=IID.Id
						LEFT JOIN (SELECT A.InventorySalesDetailId, A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId

									WHERE B.Code='IGST' and A.InventorySalesServiceId IS NULL	
									group by A.InventorySalesId, B.UserName ,B.Code,A.InventorySalesDetailId									
									) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId and TAxInfo1.InventorySalesDetailId=IID.Id 
							  		 
						LEFT JOIN (SELECT A.InventorySalesDetailId, A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId

									WHERE B.Code='SGST' and A.InventorySalesServiceId IS NULL	
									group by A.InventorySalesId, B.UserName ,B.Code,A.InventorySalesDetailId										
									) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId and TAxInfo2.InventorySalesDetailId=IID.Id 

						LEFT JOIN (SELECT A.InventorySalesDetailId, A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId

									WHERE B.Code='TDS' and A.InventorySalesServiceId IS NULL	
									group by A.InventorySalesId, B.UserName ,B.Code,A.InventorySalesDetailId						
									) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId and TAxInfo3.InventorySalesDetailId=IID.Id 							
					
						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount FROM 
									[TRN].InventorySalesAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' 	
									Group by A.InventorySalesId, B.UserName ,B.Code 
						) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
						
						WHERE II.PlantId='" + identity.PlantId + "' " +
                        "AND convert(Date,II.SalesDate) <= '" + toDate + @"'
						
						UNION ALL
						Select                  
								ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
								,'InventorySales' SourceType
								,ISs.Id
								,IR.Id SalesId,0 BooksCurrencyBaseRate
								,FORMAT(IR.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,'' SalesOrderId
								,'' MasterOrderId
								,'' SONo
								,'' PONumber
								,'' AS BillTo
								,'' AS BillToAddress
								,'' AS BillToState
								,'' as BillToGSTNo
								,'' AS ShipTo
								,'' AS ShipToAddress
								,'' AS ShipToState	
								,'' as ShipToGSTNo
								, 0 ToCurrencyRate
								, '' DocRefNo
								,FORMAT(IR.DocDate,'') DocDate
								,'' PartyType, P.UserName AS PartyName,p.Code
								,'' AS MaterialGroupMasterName
								,SM.UserName MaterialMasterName
								,'' AS MaterialMasterArticleName
								,''FirstCharacteristicsValue
								,'' SecondCharacteristicsValue
								,'' ThirdCharacteristicsValue
								--, '' HSNCode
								,IR.CustomerId PartyId
								,0 TransactionRate
								,0 TransactionQty
								,ISs.Amount TransactionAmount
								,ISs.Amount*ISNULL(ToCurrencyRate,1) BooksAmount
								,0 TaxAmount
								,ISs.Amount NetAmount
								,ISs.Amount NetBookValue
								,'' VoucherDetailId
								,'' AS BaseUoM
								,'' AS TransactionUoM
								,'' AS Currency
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,0 ServiceCharge
								,0 ServiceTax
								,E.UserName Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,'' Posted
								,'' 'NoteForAccounts'

						,round(isnull(TAxInfo.TaxAmount,0),2)  CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
						,''ContainerNo ,''TransporterName,''TransportDocRefNo 
						,''TransportDocDate,''AgentName
						,''AgentCommission
						,'' Insurance
						,''GrossWeight,''LoTNo
						,''ContractNo
						,''MasterLcNo
						,''ComercialInvoiceNo
						,''ExpiryDate
						,''BLAWBNo,''BLAWBDate
						,''PaymentTerm,''BaseOnDueDate
						,0 NoOfDays
					    ,''MatureDate
						,0 LCAmount
						,''ExFactoryDate
						,''TransportAgent	
						
						,''CNFAgent
						,''CNFContainerNo
						,''CNFVesselTrackingNo
						,''OwnReferenceNo
						,0 RealizeAmount
					    ,''RealizeDate,'' PordDertails  ,'' LOT ,'' BuyerRefNo,'' Bags,'' GrossWeights,
						'' TransportVehicleNo , '' TransportDriverNo

							--,0BalanceAmount
						from trn.InventoryService AS ISS
						LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
						left jOIN [TRN].[InventorySales] AS IR ON IR.Id=ISs.InventoryReceiveId
						LEFT JOIN HKP.Party AS P ON P.Id=IR.CustomerId
						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
						left join trn.Voucher V on V.Id=I.VoucherId
						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
						left join trn.Voucher V1 on V1.Id=ep.VoucherId
						Left JOIN [ORG].[Entity] E On E.id= IR.EntityId
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST'  
									Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
									--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
									) TAxInfo	ON TAxInfo.InventorySalesServiceId=ISs.Id AND TAxInfo.InventorySalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST'  
									Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
									) TAxInfo1	ON TAxInfo1.InventorySalesServiceId=ISs.Id AND TAxInfo1.InventorySalesServiceId IS NOT NULL 
							  		 
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST'  
                                    Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
											) TAxInfo2	ON TAxInfo2.InventorySalesServiceId=ISs.Id AND TAxInfo2.InventorySalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS' 
                                    Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
									) TAxInfo3	ON TAxInfo3.InventorySalesServiceId=ISs.Id AND TAxInfo3.InventorySalesServiceId IS NOT NULL
							
					
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,Sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS' 
                                    Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
						) TAxInfo6 ON TAxInfo6.InventorySalesServiceId=ISs.Id AND TAxInfo6.InventorySalesServiceId IS NOT NULL
						WHERE IR.PlantId='" + identity.PlantId + "' " +
                        "AND convert(Date,IR.SalesDate) <= '" + toDate + @"')x
						 " + CusAll + "";
                        return _sqlRepository.GetDataTable(sql);
                    }
                }
                else
                {
                    if (Type == "ForThePeriod")
                    {
                        sql = @"Select * from(
									SELECT 
									ROW_NUMBER() Over(Order by SA.Id) As[S.N]
									,SA.Id SalesId
									,SA.SourceType,SMD.BooksCurrencyBaseRate
									--SM.Id	
									,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate						
									,PPI.UserName AS BillTo
									,PPD.UserName AS ShipTo
									, SA.ToCurrencyRate
									, SA.DocRefNo
									,FORMAT(SA.InvoiceDate,'dd-MMM-yyyy') DocDate
									,SA.PartyId, P.UserName AS PartyName,p.Code	
									,SMD.TransactionAmount
									,SMD.TransactionAmount*ISNULL(SA.ToCurrencyRate,1) BooksAmount
									,v.VoucherNo VoucherId
									,CU.Code AS Currency
									,''SOType
									,sum(round(isnull(ServiceData.ServiceAmount,0),2)) ServiceCharge
									,sum(round(isnull(ServiceData.ServiceTax,0),2)) ServiceTax
									,E.UserName Entity
									,'' CheckedByName
									,'' CheckedBy
									,'' ApprovedByName
									,'' ApprovedBy
									,Posted=CASE WHEN v.VoucherNo IS NULL THEN 'No' ELSE 'YES'  END
									,iSNUll( SA.Narration,'') NoteForAccounts	
									--,sum(round(isnull(SMD.TaxAmount,0),2)) CGST			
									--,sum(round(isnull(SMD.TaxAmount,0),2)) SGST
									--,sum(round(isnull(SMD.TaxAmount,0),2)) IGST
									--,sum(round(isnull(SMD.TaxAmount,0),2)) TDS
									,SMD.CGST
									,SMD.SGST
									,SMD.IGST
									,SMD.TDS
									,round(isnull(TAxInfo6.TaxAmount,0),2) TCS

									--,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))  BooksCGST		
									--,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
									--,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
									,SMD.BooksCGST
									,SMD.BooksSGST
									,SMD.BooksIGST
									,round(isnull(TAxInfo6.BooksTaxAmount,0),2) BooksTCS

									,sum(round(isnull(ServiceData.ServiceAmount,0),2))+Sum(SMD.TransactionAmount ) TotalTaxableAmt
									,Sum(SMD.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
									,sum(ServiceData.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt

									,sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
									,(Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt

									,SONumber=STUFF((select distinct ','+XSO.Id 
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId                                     
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, PONumber=STUFF((select distinct ','+CPO.PONumber
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, MasterOrder=STUFF((select distinct ','+MO.MasterOrderNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                 where smx.SalesId=SA.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, InvoiceAmount=isnull(I.Amount,0)
									, RealizeAmount=isnull(I.WrittenOffAmount,0)						
		                            , BalanceAmount=isnull(isnull(SMD.NetAmount,0) -isnull(I.WrittenOffAmount,0),0)
									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
								                where XI.VoucherId=SA.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, OwnReferenceNo=STUFF((select distinct ','+MO.OwnReferenceNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,FORMAT(PSI.ExpDate,'dd-MMM-yyyy') ExpDate,PSI.CNFBLAWB BLAWBNo,FORMAT(PSI.CNFBLAWBDate,'dd-MMM-yyyy') BLAWBDate,FORMAT(PSI.TransportDocDate,'dd-MMM-yyyy') TransportDocDate
									,CNfA.UserName CNFAgent
									,TA.UserName TransportAgent							
									,FORMAT(PSI.ExFactoryDate,'dd-MMM-yyyy') ExFactoryDate
									,PSI.CNFContainerNo,PSI.CNFVesselTrackingNo
									,PTM.UserName PaymentTerm,FORMAT(SA.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate
									,SA.BaseNoOfDays NoOfDays
									,FORMAT(SA.MatureDate,'dd-MMM-yyyy') MatureDate
									,SA.EXPFromNo,SA.ComercialInvoiceNo
									,SMD.LCAmount,SMD.ContractNo
									,SMD.MasterLcNo

									FROM TRN.Sales AS SA
									--left outer join TRN.SalesMaterial SM on SM.SalesId=SA.Id
									-----------------------------------------------------------
									LEFT JOIN (

									select SM.SalesId, Sum(SM.TransactionAmount) TransactionAmount,Sum(SM.NetAmount) NetAmount
									,Sum(SM.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount 
									,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST			
									,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
									,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
									,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
									,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
									,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
									,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
									,PL.Amount LCAmount,CON.ContractNo
									,ML.LCRef MasterLcNo,SM.BooksCurrencyBaseRate

									from TRN.SalesMaterial SM 
									left outer join TRN.SalesOrder So on SO.Id=SM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join [Contract] CON on CON.Id=so.ContractId
									left outer join PurchaseLC PL on PL.ContractId=CON.Id
									Left outer join MasterLC ML on ML.Id=CON.MasterLCId


									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount ,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='CGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo	ON TAxInfo.salesMaterialId=SM.Id
									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='IGST' --and A.SalesServiceId IS NULL	
												Group by A.salesMaterialId
												) TAxInfo1	ON TAxInfo1.salesMaterialId=SM.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.TaxCategoryType='SGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo2	ON TAxInfo2.salesMaterialId=SM.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='TDS' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo3	ON TAxInfo3.salesMaterialId=SM.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='VAT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo4 ON TAxInfo4.salesMaterialId=SM.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='AIT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo5 ON TAxInfo5.salesMaterialId=SM.Id 


									--where SM.SalesId='MS2021596'
									Group BY SM.SalesId,PL.Amount ,CON.ContractNo
									,ML.LCRef ,SM.BooksCurrencyBaseRate

									)SMD  ON SA.Id=SMD.SalesId

									left outer join PostSalesInvoice PSI on PSI.SalesId=SA.Id
									left outer join MST.PaymentTerm PTM on PTM.Id=SA.PaymentTermId

									left outer join HKP.Party CNfA on CNfA.Id=SA.PartyId
									left outer join HKP.Party TA on TA.Id=SA.PartyId

									LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
									--LEFT JOIN TRN.Invoice I ON I.VoucherId=SA.VoucherId
									LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
									--LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
									--LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
									LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
									Left JOIN [ORG].[Entity] E On E.id= SA.EntityId
									LEFT JOIN (SELECT SUM(Amount) Amount,SUM(WrittenOffAmount) WrittenOffAmount,VoucherId 
												FROM TRN.Invoice GROUP BY VoucherId) I ON I.VoucherId=SA.VoucherId

									LEFT JOIN (SELECT A.SalesId,A.BooksCurrencyTaxAmount BooksTaxAmount,TaxAmount TaxAmount
												FROM trn.SalesAdditionalTax A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 		
												WHERE B.Code='TCS'  
												--Group BY A.SalesId				
									) TAxInfo6 ON TAxInfo6.SalesId=SA.Id 
									LEFT JOIN(Select ISS.SalesId, Sum(ISS.Amount) ServiceAmount,Sum(ISS.TaxAmount) ServiceTax,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
											from trn.SalesService AS ISS
											LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
											left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
											group by ISS.SalesId
											)ServiceData on ServiceData.SalesId=SA.Id

									WHERE SA.PlantId='" + identity.PlantId + @"' 
									AND convert(Date,SA.InvoiceDate) BETWEEN '" + fromDate + @"' AND '" + toDate + @"'-- and sm.SalesId='202110'
									Group By SA.PartyId,p.Code	,TAxInfo6.BooksTaxAmount,TAxInfo6.TaxAmount,SA.InvoiceDate,SA.SourceType,SA.Id,SA.DocRefNo,SA.EntryDate,PPI.UserName,PPD.UserName
									,SA.ToCurrencyRate, P.UserName,v.VoucherNo,CU.Code,E.UserName,SA.VoucherId,I.Amount,I.WrittenOffAmount,PSI.ExpDate,PSI.CNFBLAWB,PSI.CNFBLAWBDate 
									,PSI.ExFactoryDate,PSI.TransportDocRefNo
									,PSI.CNFContainerNo,PSI.CNFVesselTrackingNo,SMD.TransactionAmount

									,PTM.UserName ,SA.BaseOnDueDate,SA.BaseNoOfDays,SA.MatureDate,SA.EXPFromNo,SA.ComercialInvoiceNo
									,CNfA.UserName,TA.UserName 
									,SMD.BooksCurrencyBaseRate
									,SMD.LCAmount,SMD.ContractNo
									,SMD.MasterLcNo,PSI.TransportDocDate,SA.Narration
									,SMD.CGST
									,SMD.SGST
									,SMD.IGST
									,SMD.TDS
									,SMD.BooksCGST
									,SMD.BooksSGST
									,SMD.BooksIGST
									,SMD.NetAmount

									UNION ALL
									SELECT 

								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,II.Id SalesId
								,'InventorySales' SourceType,0 BooksCurrencyBaseRate
								--,IID.Id						
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,PPI.UserName AS BillTo
								,PPI1.UserName ShipTo
								,II.ToCurrencyRate
								, II.DocRefNo
								,FORMAT(II.DocDate,'dd-MMM-yyyy') DocDate
								,II.CustomerId PartyId, P.UserName AS PartyName,p.Code
								,Sum(IID.Qty *IID.SalesRate) TransactionAmount
								,Sum(IID.Qty *IID.SalesRate)*ISNULL(II.ToCurrencyRate,1) BooksAmount
								--,sum(SCr1.TaxAmount) TaxAmount
								--,0 NetAmount
								,v.VoucherNo VoucherId
								,'' AS Currency
								,'' SOType
								,sum(SCr.ServiceAmount) ServiceCharge
								,sum(SCr.TotalTaxAmount) ServiceTax
								,E.UserName AS Entity 
								,EI2.EmployeeName CheckedByName
								,II.CheckedBy
								,EI1.EmployeeName ApprovedByName
								,II.ApprovedBy
								,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
								,'' 'NoteForAccounts'
								,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST				
								,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
								,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
								,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
								,sum(round(isnull(TAxInfo6.TaxAmount,0),2)) TCS
								,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
								,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
								,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
								,sum(round(isnull(TAxInfo6.BooksTaxAmount,0),2)) BooksTCS			
								,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmt
								,Sum(IID.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
								,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt
								,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
								,(Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
								,'' SONumber
								,'' PONumber
								,'' MasterOrder
								, InvoiceAmount=isnull(I.Amount,0)
								, RealizeAmount=isnull(I.WrittenOffAmount,0)
								, BalanceAmount=isnull(isnull(IID.TransactionAmount,0) -isnull(I.WrittenOffAmount,0),0)
									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
									                                where XI.VoucherId=II.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,'' OwnReferenceNo
									,''ExpDate,''BLAWBNo,''BLAWBDate,''TransportDocDate
									,''CNFAgent
									,''TransportAgent

									,''ExFactoryDate
									,''CNFContainerNo,''CNFVesselTrackingNo

									,''PaymentTerm,''BaseOnDueDate
									,0 NoOfDays
									,''MatureDate
									,''EXPFromNo,''ComercialInvoiceNo		

									,0 LCAmount,''ContractNo
									,''MasterLcNo
								FROM [TRN].[InventorySales] AS II
								left JOIN (select InventoryMaterialId,Id,InventorySalesId
								,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate
								,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId
								,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0

								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
								Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
								Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
								Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
								Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
								Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId
									LEFT JOIN (SELECT SUM(Amount) Amount,SUM(WrittenOffAmount) WrittenOffAmount,InventorySalesId 
												FROM TRN.Invoice GROUP BY InventorySalesId) I ON I.InventorySalesId=II.Id
								LEFT JOIN(Select sum(Amount) ServiceAmount, sum(TotalTaxAmount) TotalTaxAmount,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,Sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount,InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
								LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId,sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='CGST' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId 
								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='IGST' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='SGST' --and A.InventorySalesServiceId IS NULL 
											GROUP BY A.InventorySalesId
											) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='TDS' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='VAT' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId

								) TAxInfo4 ON TAxInfo4.InventorySalesId=IID.Id 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
										FROM [TRN].[InventorySalesTax] A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='AIT' --and A.InventorySalesServiceId IS NULL 
											GROUP BY A.InventorySalesId

								) TAxInfo5 ON TAxInfo5.InventorySalesId=IID.InventorySalesId 
								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksTaxAmount
											FROM [TRN].InventorySalesAdditionalTax A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='TCS'
											GROUP BY A.InventorySalesId
								) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
								LEFT JOIN trn.Voucher V On V.Id=II.VoucherId
								WHERE II.PlantId='" + identity.PlantId + @"' 
								AND convert(Date,II.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
								GROUP BY II.CustomerId,p.Code,II.Id,II.SalesDate,PPI.UserName ,PPI1.UserName ,IID.TransactionAmount
								,II.ToCurrencyRate, II.DocRefNo,II.DocDate, P.UserName ,II.[Status],v.VoucherNo,E.UserName 
								,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName,II.ApprovedBy,II.VoucherId,I.Amount,I.WrittenOffAmount)x
								 " + CusAll + "";
                        return _sqlRepository.GetDataTable(sql);
                    }
                    else
                    {
                        sql = @"SELECT * FROM(
									SELECT ROW_NUMBER() Over(Order by SA.Id) As[S.N]
									,SA.Id SalesId
									,SA.SourceType,SMD.BooksCurrencyBaseRate
									--SM.Id	
									,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate ,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
									--,SMD.SalesOrderId
									--,MO.Id MasterOrderId
									--,SO.Id SONo
									--,po.PONumber
									,PPI.UserName AS BillTo
									,PPD.UserName AS ShipTo
									, SA.ToCurrencyRate
									, SA.DocRefNo
									,'' DocDate
									,SA.PartyId, P.UserName AS PartyName,p.Code	
									--, '' HSNCode
									--,SM.BaseRate
									--,SM.BaseUoMFactor
									--,SM.TransactionRate
									--,SM.TransactionQty
									,Sum(SMD.TransactionAmount) TransactionAmount
									,Sum(SMD.TransactionAmount)*ISNULL(SA.ToCurrencyRate,1) BooksAmount
									--,SM.TaxAmount
									--,SM.NetAmount
									,v.VoucherNo VoucherId
									--,BUoM.UserName AS BaseUoM
									--,TUoM.UserName AS TransactionUoM
									,CU.Code AS Currency
									--,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate
									--,DT.UserName DestinationName
									,''SOType
									,sum(round(isnull(ServiceData.ServiceAmount,0),2)) ServiceCharge
									,sum(round(isnull(ServiceData.ServiceTax,0),2)) ServiceTax
									--TransactionAmount
									,'' Entity
									,'' CheckedByName
									,'' CheckedBy
									,'' ApprovedByName
									,'' ApprovedBy
									,Posted=CASE WHEN v.VoucherNo IS NULL THEN 'No' ELSE 'YES'  END
									,'' 'NoteForAccounts'

									,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST			
									,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
									,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
									,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
									,round(isnull(TAxInfo6.TaxAmount,0),2) TCS

									,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
									,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
									,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
									,round(isnull(TAxInfo6.BooksTaxAmount,0),2) BooksTCS

									,sum(round(isnull(ServiceData.ServiceAmount,0),2))+Sum(SMD.TransactionAmount ) TotalTaxableAmt
									,Sum(SMD.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
									,sum(ServiceData.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt

									,sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
									,(Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt

									,SONumber=STUFF((select distinct ','+XSO.Id 
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId                                     
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, PONumber=STUFF((select distinct ','+CPO.PONumber
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, MasterOrder=STUFF((select distinct ','+MO.MasterOrderNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									FROM TRN.Sales AS SA
									LEFT JOIN (select Id, SalesId,SalesOrderId,BooksCurrencyBaseRate, Sum(TransactionAmount) TransactionAmount,Sum(NetAmount) NetAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from TRN.SalesMaterial Group BY SalesId,SalesOrderId,Id,BooksCurrencyBaseRate)SMD  ON SA.Id=SMD.SalesId
									--LEFT JOIN [TRN].[SalesOrder] AS SO ON SMD.SalesOrderId=SO.Id
									--LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
									--LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
									--LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
									--LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId	
									LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
									--LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
									--LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
									LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount ,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='CGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo	ON TAxInfo.salesMaterialId=SMD.Id
									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='IGST' --and A.SalesServiceId IS NULL	
												Group by A.salesMaterialId
												) TAxInfo1	ON TAxInfo1.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='SGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo2	ON TAxInfo2.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='TDS' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo3	ON TAxInfo3.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='VAT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo4 ON TAxInfo4.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='AIT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo5 ON TAxInfo5.salesMaterialId=SMD.Id 
									LEFT JOIN (SELECT A.SalesId,A.BooksCurrencyTaxAmount BooksTaxAmount,TaxAmount TaxAmount
												FROM trn.SalesAdditionalTax A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 		
												WHERE B.Code='TCS'  
												--Group BY A.SalesId				
									) TAxInfo6 ON TAxInfo6.SalesId=SA.Id 
									LEFT JOIN(Select ISS.SalesId, Sum(ISS.Amount) ServiceAmount,Sum(ISS.TaxAmount) ServiceTax,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
											from trn.SalesService AS ISS
											LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
											left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
											group by ISS.SalesId
											)ServiceData on ServiceData.SalesId=SA.Id
									LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
									WHERE SA.PlantId='" + identity.PlantId + "' " +
                                    "AND convert(Date,SA.InvoiceDate) <= '" + toDate + @"'-- and sm.SalesId='202110'
									Group By SA.PartyId,p.Code	,TAxInfo6.BooksTaxAmount,TAxInfo6.TaxAmount,SA.InvoiceDate,SA.SourceType,SA.Id,SA.DocRefNo,SA.EntryDate,PPI.UserName,PPD.UserName,SA.ToCurrencyRate, P.UserName,v.VoucherNo,CU.Code,SMD.BooksCurrencyBaseRate
								UNION ALL
								SELECT 

								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,II.Id SalesId
								,'InventorySales' SourceType,0 BooksCurrencyBaseRate
								--,IID.Id						
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								--,'' SalesOrderId
								--,'' MasterOrderId
								--,'' SONo
								--,'' PONumber
								,PPI.UserName AS BillTo
								,PPI1.UserName ShipTo
								,II.ToCurrencyRate
								, II.DocRefNo
								,II.DocDate
								, P.UserName AS PartyName,p.Code
								--,MGM.UserName AS MaterialGroupMasterName
								--,MM.UserName MaterialMasterName
								--,ART.StandardName AS MaterialMasterArticleName
								--, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue							
								--, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue						
								--, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
								--, '' HSNCode
								,II.CustomerId PartyId
								--,Sum(IID.PolicyRate) BaseRate
								--,0 BaseUoMFactor
								--,sum(IID.PolicyRate) TransactionRate
								--,Sum(IID.Qty) TransactionQty
								,Sum(IID.Qty *IID.SalesRate) TransactionAmount
								,Sum(IID.Qty *IID.SalesRate)*ISNULL(II.ToCurrencyRate,1) BooksAmount
								--,sum(SCr1.TaxAmount) TaxAmount
								--,0 NetAmount
								,v.VoucherNo VoucherId
								--,TUoM.UserName AS BaseUoM
								--,TUoM.UserName AS TransactionUoM
								,'' AS Currency
								--,'' DeliveryDate
								--,'' DestinationName
								,'' SOType
								,sum(SCr.ServiceAmount) ServiceCharge
								,sum(SCr.TotalTaxAmount) ServiceTax

								,E.UserName AS Entity 
								,EI2.EmployeeName CheckedByName
								,II.CheckedBy
								,EI1.EmployeeName ApprovedByName
								,II.ApprovedBy
								,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
								,'' 'NoteForAccounts'

								,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST				
								,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
								,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
								,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
								,sum(round(isnull(TAxInfo6.TaxAmount,0),2)) TCS
								,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
								,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
								,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
								,sum(round(isnull(TAxInfo6.BooksTaxAmount,0),2)) BooksTCS			
								,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmt
								,Sum(IID.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
								,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt
								,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
								,(Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt


								,'' SONumber
								,'' PONumber
								,'' MasterOrder
								FROM[TRN].[InventorySales] AS II
								left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
								Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
								Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
								Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
								Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
								Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId

								LEFT JOIN(Select sum(Amount) ServiceAmount, sum(TotalTaxAmount) TotalTaxAmount,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,Sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount,InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
								LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId,sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='CGST' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId 
								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='IGST' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='SGST' --and A.InventorySalesServiceId IS NULL 
											GROUP BY A.InventorySalesId
											) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='TDS' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='VAT' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId

								) TAxInfo4 ON TAxInfo4.InventorySalesId=IID.Id 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
										FROM [TRN].[InventorySalesTax] A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.TaxCategoryType='AIT' --and A.InventorySalesServiceId IS NULL 
											GROUP BY A.InventorySalesId

								) TAxInfo5 ON TAxInfo5.InventorySalesId=IID.InventorySalesId 
								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksTaxAmount
											FROM [TRN].InventorySalesAdditionalTax A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='TCS'
											GROUP BY A.InventorySalesId
								) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
								LEFT JOIN trn.Voucher V On V.Id=II.VoucherId
								WHERE II.PlantId='" + identity.PlantId + @"' 
								AND convert(Date,II.SalesDate) <= '" + toDate + @"'
								GROUP BY II.CustomerId,p.Code	,II.Id,II.SalesDate,PPI.UserName ,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo,II.DocDate, P.UserName ,II.[Status],v.VoucherNo,E.UserName ,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName,II.ApprovedBy)x
							 " + CusAll + "";
                        return _sqlRepository.GetDataTable(sql);
                    }
                }

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void GetMasterData(string Ids, out DataTable dtOrder)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {

                strSql = @"SELECT S.Id,S.Id AS SalesId, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, S.CurrencyId, C.Code AS CurrencyCode, S.DocRefNo, ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0) AS Amount,
Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,
Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
, S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId, S.BaseNoOfDays, S.BaseOnDueDate
, S.InvoiceNo, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, S.MatureDate, S.ToCurrencyRate
, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId
, CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark--,SAI.Value,AI.UserName
,V.VoucherNo,PAG.UserName PartyAccountGroup
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
LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'
LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId
LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PT.AddressMasterId
LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId=S.Id
LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId=S.Id
WHERE  S.VoucherId<>'' AND S.SourceType IN('MasterOrderSales','Packing') AND S.IsAdditionalInfoApplicable=1
AND S.Id " + Ids + "";

                dtOrder = _sqlRepository.GetDataTable(strSql);
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

        public Dictionary<string, List<DataRow>> GetParameterData(string Ids, out DataTable dtParameter)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicParameter = new Dictionary<string, List<DataRow>>();
            dtParameter = new DataTable("Tmp");
            try
            {
                strSql = @"select SAI.*,AI.UserName,AI.CharecterType from dbo.SalesAdditionalInfo SAI
LEFT JOIN hkp.AdditionalInfo AI ON AI.Id=SAI.AdditionalInfoId
Where SAI.SalesId " + Ids + @"
order by SAI.SalesId";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(strSql, out dsRef);

                dtParameter = dsRef.Tables[0].DefaultView.ToTable(true, "AdditionalInfoId", "UserName", "CharecterType");
                dtParameter = dtParameter.DefaultView.ToTable();

                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["SalesId"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicParameter.Add(dt.Rows[i]["SalesId"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["SalesId"].ToString();
                }

                return dicParameter;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }


        public void GetSalesChalanReportData(string masterId, out DataTable dtOrder)
        {
            string strSql = string.Empty;
            try
            {
                strSql = @"Select SCD.*,P.UserName Customer,BKD.NoOfPackage,BKD.NetWeight,BKD.GrossWeight,DT.UserName Destination,FORMAT(S.InvoiceDate,'dd-MMM-yyyy')InvoiceDate
                                    ,SC.VechileNo,SC.UserRef,FORMAT(SC.AddedDate,'dd-MMM-yyyy')GatePassDate
									,EI.EmployeeName CheckedBy,EIM.EmployeeName ApprovedBy,SC.CheckedStatus,SC.ApprovedStatus,ISNULL(SC.IsDispatchConfirmation,0)IsDispatchConfirmation,SC.DispatchConfirmationBy
                                    from dbo.SalesChalanDetail SCD
                                    LEFT JOIN dbo.SalesChalan SC ON SC.Id=SCD.SalesChalanId
                                    LEFT JOIN TRN.Sales S ON S.Id=SCD.InvoiceId
                                    LEFT JOIN HKP.Party P ON P.Id=S.PartyId
                                    left join (select  sum(isc.NetWeight) NetWeight ,sum(isc.Gweight) GrossWeight , Count(isc.RefNo) NoOfPackage,isc.SalesId 
                                                    from itemscanchild isc
                                                    left join trn.POLotReference PLR on PLR.Id = isc.PackingId
                                                    left join trn.PackingLineItem pli on pli.PackingLineItemId = PLR.PackingLineItemId
				                                    group by  isc.salesId) BKD on BKD.salesId = s.Id
                                    left join PostSalesInvoice PSI on PSI.SalesId = BKD.SalesId
                                    left join MST.Addressmaster AM on Am.Id = P.AddressmasterId
                                    left join scs.District DT on DT.Id = AM.DistrictId
									left join EmployeeInformation EI on EI.SystemId = SC.CheckById
									left join EmployeeInformation EIM on EIM.SystemId = SC.ApproveById
                                    Where SCD.SalesChalanId='" + masterId + "'";

                dtOrder = _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//End Function

        public IEnumerable<object> GetVehicleNoCbo(string fromDate, string toDate)
        {
            try
            {
                string sql = @"SELECT DISTINCT TransportVehicleNo AS Value,TransportVehicleNo AS Text
                                FROM [dbo].[PostSalesInvoice] PO
                                LEFT JOIN TRN.Sales S ON S.Id=PO.SalesId
                                Where TransportVehicleNo IS NOT NULL AND FORMAT(S.AddedDate,'dd-MMM-yyyy') between '" + fromDate + @"' AND '" + toDate + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> GetTransportDriverNo(string TransportVehicleNo)
        {
            try
            {
                string sql = @"SELECT TransportDriverNo FROM [dbo].[PostSalesInvoice] Where TransportVehicleNo='" + TransportVehicleNo + "' AND TransportDriverNo IS NOT NULL";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IEnumerable<object> GetSalesChalan(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select * from (Select SC.*,WE.EmployeeName ByWhom,SE.EmployeeName SecurityInCharge,RE.EmployeeName ResponsiblePerson,CE.EmployeeName CheckBy,AE.EmployeeName ApproveBy
                                                    from [dbo].[SalesChalan] SC
                                                    LEFT JOIN dbo.EmployeeInformation WE ON WE.SystemId=SC.ByWhomId
                                                    LEFT JOIN dbo.EmployeeInformation SE ON SE.SystemId=SC.SecurityInChargeId
                                                    LEFT JOIN dbo.EmployeeInformation RE ON RE.SystemId=SC.ResponsiblePersonId
                                                    LEFT JOIN dbo.EmployeeInformation CE ON CE.SystemId=SC.CheckById
                                                    LEFT JOIN dbo.EmployeeInformation AE ON AE.SystemId=SC.ApproveById) AS TEMP WHERE " + strkey + " Order By TEMP.AddedDate DESC";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> GetInvoiceData(string fromDate, string toDate, string vehicleno)
        {
            try
            {
                string sql = @"SELECT Checked=CAST(0 AS bit),FORMAT(S.InvoiceDate,'dd-MMM-yyyy')InvoiceDate, S.Id InvoiceId,P.UserName Customer,DT.UserName Destination,BKD.NoOfPackage,BKD.NetWeight,BKD.GrossWeight
                                FROM TRN.Sales S
                                LEFT JOIN HKP.Party P ON P.Id=S.PartyId
                                LEFT JOIN (select  sum(isc.NetWeight) NetWeight ,sum(isc.Gweight) GrossWeight , Count(isc.RefNo) NoOfPackage , isc.SalesId 
                                                from itemscanchild isc
                                                left join trn.POLotReference PLR on PLR.Id = isc.PackingId
                                                left join trn.PackingLineItem pli on pli.PackingLineItemId = PLR.PackingLineItemId
				                                group by  isc.salesId) BKD on BKD.salesId = s.Id
                                left join PostSalesInvoice PSI on PSI.SalesId = BKD.SalesId
                                left join MST.Addressmaster AM on Am.Id = P.AddressmasterId
                                left join scs.District Dt on DT.Id = AM.DistrictId
                                Where FORMAT(S.AddedDate,'dd-MMM-yyyy') between '" + fromDate + @"' AND '" + toDate + @"' AND PSI.Transportvehicleno='" + vehicleno + @"'
                                AND S.Id NOT IN(Select  InvoiceId from dbo.SalesChalanDetail)
                                ORDER BY S.Id";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetInvoiceDataByChalan(string masterId)
        {
            try
            {
                string sql = @"Select SCD.*,P.UserName Customer,BKD.NoOfPackage,BKD.NetWeight,BKD.GrossWeight,DT.UserName Destination,FORMAT(S.InvoiceDate,'dd-MMM-yyyy')InvoiceDate 
                                    from dbo.SalesChalanDetail SCD
                                    LEFT JOIN TRN.Sales S ON S.Id=SCD.InvoiceId
                                    LEFT JOIN HKP.Party P ON P.Id=S.PartyId
                                    left join (select  sum(isc.NetWeight) NetWeight ,sum(isc.Gweight) GrossWeight , Count(isc.RefNo) NoOfPackage,isc.SalesId 
                                                    from itemscanchild isc
                                                    left join trn.POLotReference PLR on PLR.Id = isc.PackingId
                                                    left join trn.PackingLineItem pli on pli.PackingLineItemId = PLR.PackingLineItemId
				                                    group by  isc.salesId) BKD on BKD.salesId = s.Id
                                    left join PostSalesInvoice PSI on PSI.SalesId = BKD.SalesId
                                    left join MST.Addressmaster AM on Am.Id = P.AddressmasterId
                                    left join scs.District DT on DT.Id = AM.DistrictId
                                    Where SCD.SalesChalanId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetUncheckedSalesChalanData(string EmployeeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"Select SC.*,WE.EmployeeName ByWhom,SE.EmployeeName SecurityInCharge,RE.EmployeeName ResponsiblePerson,CE.EmployeeName CheckBy,AE.EmployeeName ApproveBy
								from [dbo].[SalesChalan] SC
								LEFT JOIN dbo.EmployeeInformation WE ON WE.SystemId=SC.ByWhomId
								LEFT JOIN dbo.EmployeeInformation SE ON SE.SystemId=SC.SecurityInChargeId
								LEFT JOIN dbo.EmployeeInformation RE ON RE.SystemId=SC.ResponsiblePersonId
								LEFT JOIN dbo.EmployeeInformation CE ON CE.SystemId=SC.CheckById
								LEFT JOIN dbo.EmployeeInformation AE ON AE.SystemId=SC.ApproveById 
								where SC.CheckedStatus='To Be Check' AND SC.CheckById='" + EmployeeId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetcheckedSalesChalanData(string EmployeeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"Select SC.*,WE.EmployeeName ByWhom,SE.EmployeeName SecurityInCharge,RE.EmployeeName ResponsiblePerson,CE.EmployeeName CheckBy,AE.EmployeeName ApproveBy
								from [dbo].[SalesChalan] SC
								LEFT JOIN dbo.EmployeeInformation WE ON WE.SystemId=SC.ByWhomId
								LEFT JOIN dbo.EmployeeInformation SE ON SE.SystemId=SC.SecurityInChargeId
								LEFT JOIN dbo.EmployeeInformation RE ON RE.SystemId=SC.ResponsiblePersonId
								LEFT JOIN dbo.EmployeeInformation CE ON CE.SystemId=SC.CheckById
								LEFT JOIN dbo.EmployeeInformation AE ON AE.SystemId=SC.ApproveById 
								where SC.CheckedStatus='Checked' AND SC.ApprovedStatus='To Be Approve' AND SC.ApproveById='" + EmployeeId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetApproveBycheckedSalesChalanData(string EmployeeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"Select SC.*,WE.EmployeeName ByWhom,SE.EmployeeName SecurityInCharge,RE.EmployeeName ResponsiblePerson,CE.EmployeeName CheckBy,AE.EmployeeName ApproveBy,SC.CheckedStatus,SC.ApprovedStatus
								from [dbo].[SalesChalan] SC
								LEFT JOIN dbo.EmployeeInformation WE ON WE.SystemId=SC.ByWhomId
								LEFT JOIN dbo.EmployeeInformation SE ON SE.SystemId=SC.SecurityInChargeId
								LEFT JOIN dbo.EmployeeInformation RE ON RE.SystemId=SC.ResponsiblePersonId
								LEFT JOIN dbo.EmployeeInformation CE ON CE.SystemId=SC.CheckById
								LEFT JOIN dbo.EmployeeInformation AE ON AE.SystemId=SC.ApproveById 
								where SC.ApprovedStatus='Approved' AND SC.ApproveById='" + EmployeeId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetcheckedSalesChalanDataList(string EmployeeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"Select SC.*,WE.EmployeeName ByWhom,SE.EmployeeName SecurityInCharge,RE.EmployeeName ResponsiblePerson,CE.EmployeeName CheckBy,AE.EmployeeName ApproveBy,SC.CheckedStatus,SC.ApprovedStatus
								from [dbo].[SalesChalan] SC
								LEFT JOIN dbo.EmployeeInformation WE ON WE.SystemId=SC.ByWhomId
								LEFT JOIN dbo.EmployeeInformation SE ON SE.SystemId=SC.SecurityInChargeId
								LEFT JOIN dbo.EmployeeInformation RE ON RE.SystemId=SC.ResponsiblePersonId
								LEFT JOIN dbo.EmployeeInformation CE ON CE.SystemId=SC.CheckById
								LEFT JOIN dbo.EmployeeInformation AE ON AE.SystemId=SC.ApproveById 
								where SC.CheckedStatus='Checked' AND SC.CheckById='" + EmployeeId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSalesChalanCheckedByCboList()
        {
            var sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='SalesChalanCheckBy' AND E.EmployeeStatus='Active'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetSalesChalanApproveByCboList()
        {
            var sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='SalesChalanApproveBy' AND E.EmployeeStatus='Active'";
            return _sqlRepository.GetDataCollection(sql, null);
        }
        public IEnumerable<object> GetMultipleVendorPaymentApproveByCboList()
        {
            var sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='MultipleVendorPayment' AND E.EmployeeStatus='Active'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetApproveByDataForDispatchConfirmation()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"Select SC.*,WE.EmployeeName ByWhom,SE.EmployeeName SecurityInCharge,RE.EmployeeName ResponsiblePerson,CE.EmployeeName CheckBy,AE.EmployeeName ApproveBy,SC.CheckedStatus,SC.ApprovedStatus
								from [dbo].[SalesChalan] SC
								LEFT JOIN dbo.EmployeeInformation WE ON WE.SystemId=SC.ByWhomId
								LEFT JOIN dbo.EmployeeInformation SE ON SE.SystemId=SC.SecurityInChargeId
								LEFT JOIN dbo.EmployeeInformation RE ON RE.SystemId=SC.ResponsiblePersonId
								LEFT JOIN dbo.EmployeeInformation CE ON CE.SystemId=SC.CheckById
								LEFT JOIN dbo.EmployeeInformation AE ON AE.SystemId=SC.ApproveById 
								where SC.ApprovedStatus='Approved' AND SC.IsDispatchConfirmation=0";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetApproveByDataForDispatchConfirmed()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"Select SC.*,WE.EmployeeName ByWhom,SE.EmployeeName SecurityInCharge,RE.EmployeeName ResponsiblePerson,CE.EmployeeName CheckBy,AE.EmployeeName ApproveBy,SC.CheckedStatus,SC.ApprovedStatus
								from [dbo].[SalesChalan] SC
								LEFT JOIN dbo.EmployeeInformation WE ON WE.SystemId=SC.ByWhomId
								LEFT JOIN dbo.EmployeeInformation SE ON SE.SystemId=SC.SecurityInChargeId
								LEFT JOIN dbo.EmployeeInformation RE ON RE.SystemId=SC.ResponsiblePersonId
								LEFT JOIN dbo.EmployeeInformation CE ON CE.SystemId=SC.CheckById
								LEFT JOIN dbo.EmployeeInformation AE ON AE.SystemId=SC.ApproveById 
								where SC.IsDispatchConfirmation=1";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook MultipleVendorPaymentReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string mvpId)
        {
            var excelEngine = new ExcelEngine();

            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            DataTable dsLocal = GetMultipleVendorPaymentReportData(mvpId);
            if (dsLocal.Rows.Count == 0)
                throw new Exception("No data found");
            DataTable data = GetMultipleVendorPaymentDetailReportData(mvpId);

            reportFileName = "Multiple Vendor Payment" + dsLocal.Rows[0]["MultiplePaymentNo"];

            var row = 7;
            var col = 1;
            var colRow = row;

            reportUtility.SetMasterHeaderText(ref sheet, row, col, "Multiple Payment No");
            reportUtility.SetText(ref sheet, row, col + 1, dsLocal.Rows[0]["MultiplePaymentNo"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();


            reportUtility.SetMasterHeaderText(ref sheet, row, col + 3, "Tentative Date");
            reportUtility.SetText(ref sheet, row, col + 4, dsLocal.Rows[0]["TentativeDate"].ToString());
            sheet[reportUtility.GetColumnNameForXls(5) + row + ":" + reportUtility.GetColumnNameForXls(6) + row].Merge();


            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, col, "Due Up To Date");
            reportUtility.SetText(ref sheet, row, col + 1, dsLocal.Rows[0]["DueUpToDate"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, row, col + 3, "Bank");
            reportUtility.SetText(ref sheet, row, col + 4, dsLocal.Rows[0]["Bank"].ToString());
            sheet[reportUtility.GetColumnNameForXls(5) + row + ":" + reportUtility.GetColumnNameForXls(6) + row].Merge();

            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, col, "Account No");
            reportUtility.SetText(ref sheet, row, col + 1, dsLocal.Rows[0]["AccountNo"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, row, col + 3, "Approved By");
            reportUtility.SetText(ref sheet, row, col + 4, dsLocal.Rows[0]["ApprovedBy"].ToString());
            sheet[reportUtility.GetColumnNameForXls(5) + row + ":" + reportUtility.GetColumnNameForXls(6) + row].Merge();

            row++; //row8
            reportUtility.SetMasterHeaderText(ref sheet, row, col, "Approval Status");
            reportUtility.SetText(ref sheet, row, col + 1, dsLocal.Rows[0]["ApprovalStatus"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            sheet[reportUtility.GetColumnNameForXls(5) + row + ":" + reportUtility.GetColumnNameForXls(6) + row].Merge();

            sheet.Range[colRow, col, row, col + 5].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[colRow, col, row, col + 5].BorderInside(ExcelLineStyle.Hair);
            row++;

            int ROW = 12; int COL = 1;

            #region columns
            sheet[ROW, COL].Text = "Multiple Payment Detail No";
            sheet[ROW, COL].ColumnWidth = 20;
            int ColMultiplePaymentDetailId = COL;
            COL++;

            sheet[ROW, COL].Text = "Party";
            sheet[ROW, COL].ColumnWidth = 20;
            int ColPartyName = COL;
            COL++;

            sheet[ROW, COL].Text = "Doc RefNo";
            sheet[ROW, COL].ColumnWidth = 12;
            int ColDocRefNo = COL;
            COL++;

            sheet[ROW, COL].Text = "Currency";
            sheet[ROW, COL].ColumnWidth = 12;
            int ColCurrency = COL;
            COL++;

            sheet[ROW, COL].Text = "Amount";
            sheet[ROW, COL].ColumnWidth = 20;
            int ColAmount = COL;
            COL++;

            sheet[ROW, COL].Text = "Status";
            sheet[ROW, COL].ColumnWidth = 12;
            int ColStatus = COL;

            #endregion columns
            int endCol = COL;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            ROW++;

            int startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColMultiplePaymentDetailId].Text = data.Rows[i]["MultiplePaymentDetailId"].ToString();
                sheet[ROW, ColPartyName].Text = data.Rows[i]["PartyName"].ToString();
                sheet[ROW, ColDocRefNo].Text = data.Rows[i]["DocRefNo"].ToString();
                sheet[ROW, ColCurrency].Text = data.Rows[i]["Currency"].ToString();
                sheet[ROW, ColAmount].Text = data.Rows[i]["Amount"].ToString();
                sheet[ROW, ColStatus].Text = data.Rows[i]["Status"].ToString();

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
            //reportUtility.PlantHeader(ref sheet, endCol, "Utility Transaction Report", identity.PlantId);
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

            reportUtility.CompanyPlantHeader(ref sheet, endCol, "Journal Voucher", companyId, plantId, plantName, null);
            reportUtility.FreezePage(ref sheet, 1, endCol);
            reportUtility.PageAdjustableSetup(ref sheet, 1, row + 3, ExcelPageOrientation.Portrait);

            return workbook;
        }
        public DataTable GetMultipleVendorPaymentReportData(string mvpId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select MP.Id MultiplePaymentNo,format(MP.TentativeDate,'dd-MMM-yyyy') TentativeDate,format(MP.DueUpToDate,'dd-MMM-yyyy') DueUpToDate
							,BM.AccountTitle Bank,BM.AccountNumber AccountNo,EI.EmployeeName ApprovedBy,MP.ApprovalStatus
							from TRN.MultiplePayment MP 
							left join MST.BankMaster BM on BM.Id=MP.BankMasterId
							left join EmployeeInformation EI on EI.SystemId=MP.ApprovedBy
							where MP.Id='" + mvpId + @"'";
            return _sqlRepository.GetDataTable(sql);
        }
        public DataTable GetMultipleVendorPaymentDetailReportData(string mvpId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select MPD.Id MultiplePaymentDetailId,I.DocRefNo,C.Code Currency,MPD.Amount
									,Status= case when MPD.IsPark=0 then 'Posted' else 'Parked' end,P.UserName PartyName
									from TRN.MultiplePaymentDetail MPD
									left join EmployeeInformation EI on EI.SystemId=MPD.PartyId
									left join TRN.Invoice I on I.Id=MPD.InvoiceId
									left join SCS.Currency C on C.Id=I.CurrencyId
									LEFT JOIN HKP.Party P ON P.Id=I.PartyId
							where MPD.MultiplePaymentId='" + mvpId + @"'";
            return _sqlRepository.GetDataTable(sql);
        }

        #region Good Work Check

        public IEnumerable<object> GetGoodWorkPaymentApproveByCboList()
        {
            var sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner Join dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='GoodWorkPaymentApproveBy' AND E.EmployeeStatus='Active'";
            return _sqlRepository.GetDataCollection(sql, null);
        }
        public IEnumerable<object> GetEmployeeMultipleAdvanceApproveByCboList()
        {
            var sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner Join dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='EmployeeMultipleAdvanceApproveBy' AND E.EmployeeStatus='Active'";
            return _sqlRepository.GetDataCollection(sql, null);
        }
        public IEnumerable<object> GetUncheckedGoodWorkData(string EmployeeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select gw.Id,format(gw.WorkDate,'dd-MMM-yyyy')WorkDate,format(gw.FromTime,'hh:mm') FromTime
									,format(gw.ToTime,'hh:mm') ToTime,gw.Minute,gw.Remarks,SD.UserName Shift 
									from goodwork gw  
									left join ShiftDefination SD on SD.SystemID=gw.ShiftId
						  where gw.CheckedStatus='To Be Checked' and gw.ApprovedStatus is null 
							AND gw.CheckedBy='" + EmployeeId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetcheckedGoodWorkData(string EmployeeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select gw.Id,format(gw.WorkDate,'dd-MMM-yyyy')WorkDate,format(gw.FromTime,'hh:mm') FromTime
									,format(gw.ToTime,'hh:mm') ToTime,gw.Minute,gw.Remarks,SD.UserName Shift 
									from goodwork gw  
									left join ShiftDefination SD on SD.SystemID=gw.ShiftId 
									where gw.CheckedStatus ='Checked' AND gw.ApprovedStatus ='To Be Approved' AND gw.CheckedBy='" + EmployeeId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetApproveBycheckedGoodWorkData(string EmployeeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select gw.Id,format(gw.WorkDate,'dd-MMM-yyyy')WorkDate,format(gw.FromTime,'hh:mm') FromTime
										,format(gw.ToTime,'hh:mm') ToTime,gw.Minute,gw.Remarks,SD.UserName Shift 
										,ei.EmployeeName
										from goodwork gw  
										left join ShiftDefination SD on SD.SystemID=gw.ShiftId	
										left join EmployeeInformation ei on ei.SystemId=gw.ApprovedBy								
						where gw.ApprovedStatus='To Be Approved' AND gw.ApprovedBy='" + EmployeeId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetcheckedGoodWorkDataList(string EmployeeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select gw.Id,format(gw.WorkDate,'dd-MMM-yyyy')WorkDate,format(gw.FromTime,'hh:mm') FromTime
										,format(gw.ToTime,'hh:mm') ToTime,gw.Minute,gw.Remarks,SD.UserName Shift 
										,ei.EmployeeName
										from goodwork gw  
										left join ShiftDefination SD on SD.SystemID=gw.ShiftId	
										left join EmployeeInformation ei on ei.SystemId=gw.ApprovedBy								
						where gw.CheckedStatus ='Checked' AND gw.ApprovedStatus='Approved' AND gw.ApprovedBy='" + EmployeeId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Good Work Check

        #region Production Inventory Sales
        public IEnumerable<object> GetProductionOrderSOList(string productionOrderId)
        {
            try
            {

                var _sql = @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN
                                ,MO.Type,isnull(moi.Consignment,0) AS Consignment
                                ,CASE WHEN ISNULL(eout.Id,'')<>'' OR ISNULL(TOUT.Id,'')<>'' THEN CONCAT(POWN.UserName,'(',EOWN.UserName,')') ELSE '' END AS OrderOwner

	                            ,POD.Id ProductionOrderDetailId, POD.ProductionOrderId, MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId
	                            , SO.Id AS SalesOrderId,SO.Id SONo, P.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName,PM.UserName AS ProductName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
                                , LSD = REPLACE(CONVERT(CHAR(11), SO.LSD, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , isnull(PO.PONumber,'') AS PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            ,ISNULL(SO.Description,'')Description
	                            , Flag = CAST(0 AS BIT),ISNULL(SO.DestinationDescription,'')DestinationDescription
								, SO.Qty,0 SoldQty,BalanceQty=SO.Qty-0, SO.CM, SO.Rate,0 TransactionQty
							
                       FROM  [TRN].[SalesOrder] AS SO 
                       JOIN [TRN].[ProductionOrderDetail] AS POD ON pod.SalesOrderId=so.Id
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
					   LEFT JOIN ORG.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
					   LEFT JOIN ORG.Plant AS POUT ON POUT.Id=EOUT.PlantId
					   LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
					   LEFT JOIN ORG.Plant AS POWN ON POWN.Id=MO.PlantId
					   LEFT JOIN ORG.Entity AS EOWN ON EOWN.Id=MO.EntityId

                      WHERE POD.ProductionOrderId = '" + productionOrderId + "'ORDER BY MOI.MATERIALMASTERID,MOI.ArticleID";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)

            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }
        #endregion

        public IEnumerable<object> GetPartyAdditionalInfoDataList(string partyId)
        {
            try
            {
                string sql = @"SELECT Flag=CAST(CASE WHEN SA.Id IS NULL THEN 0 ELSE 1 END AS bit),A.UserName,SA.Id,SA.LineItemId
,A.Id AdditionalInfoId,SA.Value,SA.Remarks,A.CharecterType,'' CharType,''datepic
FROM [HKP].[AdditionalInfo] A
OUTER APPLY(Select * from [dbo].[SalesAdditionalInfo] Where AdditionalInfoId=A.Id AND PartyId='" + partyId + @"') SA
Where A.Category='Party'
Order By A.sequence";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> GetSalesMaterialDataList(string plantId, string fromDate, string toDate, string inputCreditId)
        {
            var cmdText = @"SELECT Flag=CAST(CASE WHEN SM.InputCreditId IS NULL THEN 0 ELSE 1 END AS bit),SM.Id,MO.Id MasterOrderId,SO.Id SONo,po.PONumber, FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate,DT.UserName DestinationName,MM.UserName MaterialMasterName,ART.StandardName AS MaterialMasterArticleName
,ISNULL(AHSN.Code,HSN.Code) HSNCode,FCV.UserName SKU1,SCV.UserName SKU2,SM.TransactionRate,SM.TransactionQty,SM.TransactionAmount,SM.TaxAmount
,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesService] WHERE SalesId=SA.Id)/NULLIF((Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id),0))*SM.TransactionAmount
	           ,ServiceTax=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesTax] WHERE SalesId=SA.Id  AND SalesServiceId<>'')/NULLIF((Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id),0))*SM.TransactionAmount
           ,SM.InputCreditId,'SalesMaterial' SourceType
			FROM TRN.SalesMaterial AS SM 
            LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId
            LEFT JOIN [TRN].[SalesOrder] AS SO ON SM.SalesOrderId=SO.Id
            JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
			JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
			LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
			LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId

            LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
			left join HKP.HSNCode as HSN on HSN.Id = MM.HSNCodeId
            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
            LEFT JOIN MST.MaterialMasterArticle AS ART ON SM.ArticleId=ART.Id
			left join HKP.HSNCode as AHSN on AHSN.Id = ART.HSNCodeId
            LEFT JOIN TRN.FirstCharacteristics AS FC ON FC.Id=SM.FirstCharacteristicsId AND SM.SalesOrderId=FC.SalesOrderId
            LEFT JOIN HKP.CharacteristicsValue AS FCV ON FCV.Id=SM.FirstCharacteristicsValueId

            LEFT JOIN TRN.SecondCharacteristics AS SC ON SC.Id=SM.SecondCharacteristicsId AND SM.SalesOrderId=SC.SalesOrderId
            LEFT JOIN HKP.CharacteristicsValue AS SCV ON SCV.Id=SM.SecondCharacteristicsValueId
            WHERE SA.PlantId='" + plantId + @"' AND SM.AddedDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(SM.InputCreditId,'" + inputCreditId + @"')='" + inputCreditId + @"'
UNION
SELECT Flag=CAST(CASE WHEN SM.InputCreditId IS NULL THEN 0 ELSE 1 END AS bit),SM.Id,'' MasterOrderId,'' SONo,''PONumber, '' DeliveryDate,'' DestinationName,MM.UserName MaterialMasterName,ART.StandardName AS MaterialMasterArticleName
,ISNULL(AHSN.Code,HSN.Code) HSNCode,FCV.UserName SKU1,SCV.UserName SKU2,SM.SalesRate TransactionRate,SM.TransactionQty,SM.TotalSalesAmount TransactionAmount,0 TaxAmount
,ServiceCharge=0,ServiceTax=0,SM.InputCreditId,'InventorySales' SourceType
			FROM TRN.InventorySalesDetail AS SM 
            LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=SM.InventoryMaterialId
            LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=IM.MaterialMasterId
			left join HKP.HSNCode as HSN on HSN.Id = MM.HSNCodeId
            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
            LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
			left join HKP.HSNCode as AHSN on AHSN.Id = ART.HSNCodeId
            LEFT JOIN TRN.FirstCharacteristics AS FC ON FC.Id=IM.FirstCharacteristicsId
            LEFT JOIN HKP.CharacteristicsValue AS FCV ON FCV.Id=IM.FirstCharacteristicsValueId

            LEFT JOIN TRN.SecondCharacteristics AS SC ON SC.Id=IM.SecondCharacteristicsId 
            LEFT JOIN HKP.CharacteristicsValue AS SCV ON SCV.Id=IM.SecondCharacteristicsValueId
            WHERE IM.PlantId='" + plantId + @"' AND SM.AddedDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(SM.InputCreditId,'" + inputCreditId + @"')='" + inputCreditId + @"'";

            return _sqlRepository.GetDataCollection(cmdText);
        }

        public List<Dictionary<string, object>> GetTaggedSalesMaterialDataList(string inputCreditId)
        {
            var cmdText = @"SELECT Flag=CAST(CASE WHEN SM.InputCreditId IS NULL THEN 0 ELSE 1 END AS bit),SM.Id,MO.Id MasterOrderId,SO.Id SONo,po.PONumber, FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate,DT.UserName DestinationName,MM.UserName MaterialMasterName,ART.StandardName AS MaterialMasterArticleName
,ISNULL(AHSN.Code,HSN.Code) HSNCode,FCV.UserName SKU1,SCV.UserName SKU2,SM.TransactionRate,SM.TransactionQty,SM.TransactionAmount,SM.TaxAmount
,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesService] WHERE SalesId=SA.Id)/NULLIF((Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id),0))*SM.TransactionAmount
	           ,ServiceTax=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesTax] WHERE SalesId=SA.Id  AND SalesServiceId<>'')/NULLIF((Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id),0))*SM.TransactionAmount
           ,SM.InputCreditId,'SalesMaterial' SourceType
			FROM TRN.SalesMaterial AS SM 
            LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId
            LEFT JOIN [TRN].[SalesOrder] AS SO ON SM.SalesOrderId=SO.Id
            JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
			JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
			LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
			LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId

            LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
			left join HKP.HSNCode as HSN on HSN.Id = MM.HSNCodeId
            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
            LEFT JOIN MST.MaterialMasterArticle AS ART ON SM.ArticleId=ART.Id
			left join HKP.HSNCode as AHSN on AHSN.Id = ART.HSNCodeId
            LEFT JOIN TRN.FirstCharacteristics AS FC ON FC.Id=SM.FirstCharacteristicsId AND SM.SalesOrderId=FC.SalesOrderId
            LEFT JOIN HKP.CharacteristicsValue AS FCV ON FCV.Id=SM.FirstCharacteristicsValueId

            LEFT JOIN TRN.SecondCharacteristics AS SC ON SC.Id=SM.SecondCharacteristicsId AND SM.SalesOrderId=SC.SalesOrderId
            LEFT JOIN HKP.CharacteristicsValue AS SCV ON SCV.Id=SM.SecondCharacteristicsValueId
            WHERE SM.InputCreditId='" + inputCreditId + @"'";

            return _sqlRepository.GetDataCollection(cmdText);
        }

        public IEnumerable<object> GetBankMaster()
        {
            var sql = @"SELECT BM.Id, BM.AccountTitle, BM.AccountNumber, BM.CurrencyId, C.Code AS CurrencyCode, B.UserName AS BankName, BB.UserName AS BankBranchName, GLGI.AccountCode AS GLGeneralInfoCode
,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName, A.UserName AS ActivityName
FROM [MST].[BankMaster] AS BM
LEFT JOIN [SCS].[Currency] AS C ON C.Id=BM.CurrencyId
LEFT JOIN [HKP].[Bank] AS B ON B.Id=BM.BankId
LEFT JOIN [HKP].[BankBranch] AS BB ON BB.Id=BM.BankBranchId
LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=BM.BudgetMasterId
LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
LEFT JOIN [HKP].[Activity] AS A ON A.Id=BM.ActivityId
Where BM.AccountType='HouseBank'";
            return _sqlRepository.GetDataCollection(sql);
        }
        public IEnumerable<object> GetSalesProcessTransactionList()
        {
            string sql = @"SELECT SPM.Id SalesProcessId,SPM.SalesProcess,SPM.Sequence,T.Id,T.SalesTypeId,T.PaymentModeId,T.StandardDaysFromInvoice,T.StandardDaysFromPreviousProcess,T.IsBankApplicable
,T.BankId,T.DepartmentId,T.ResponsiblePersonId,T.PaymentProcess,T.Remark,EI.EmployeeName ResponsiblePerson,DP.UserName Department,BN.UserName Bank FROM HKP.SalesProcessMaster SPM
OUTER APPLY (Select T.Id,T.SalesTypeId,T.PaymentModeId,T.StandardDaysFromInvoice,T.StandardDaysFromPreviousProcess,T.IsBankApplicable
,T.BankId,T.DepartmentId,T.ResponsiblePersonId,T.PaymentProcess,T.Remark
from TRN.SalesProcessTransaction T Where SalesProcessId=SPM.Id
) T
LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=T.ResponsiblePersonId
LEFT JOIN ORG.Department DP ON DP.Id= T.DepartmentId
LEFT JOIN HKP.Bank BN ON BN.Id= T.BankId";
            return _sqlRepository.GetDataCollection(sql);
        }



    }
}
