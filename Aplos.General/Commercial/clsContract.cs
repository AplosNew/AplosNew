using Library.Core;
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


namespace Library.General.Commercial
{
    public class clsContract
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        public clsContract()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();            
        }

        
        public IEnumerable<object> GetContractList(string column, string value,string PlantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                string sql = @"select top 100 * from (SELECT C.Id, C.CustomerId, C.IsLC, C.AddedBy, C.AddedDate, C.AddedFromIP, C.UpdatedBy, C.UpdatedDate, C.UpdatedFromIP, C.MasterLCId, 
isnull(C.ContractNo,'')ContractNo, C.TotalQty, C.SOQty, C.Amount, C.Description, isnull(C.UDNo,'')UDNo, C.UDDate, C.ContractDate, C.IsPrint,C. IsMarketingCommisssionApplicable, 
C.MarketingCommisssionId, C.IsBusinessDevelopmentChargesApplicable, C.BusinessDevelopmentCharge, C.BusinessDevelopmentChargeValue, 
C.InvoicingPartyPlantId, C.DeliveryPartyPlantId, C.InvoicingByAddress, C.DeliveryByAddress, C.MarketingCommisssionCharge, 
C.MarketingCommisssionValue,  isnull(C.Remarks,'')Remarks, C.PlantId, isnull(P.UserName,'') CustomerName,PM.UserName MarketingCommisssion,LC.LCRef MasterLCNo,FORMAT(C.AddedDate,'dd-MMM-yyyy') CreationDate
,[Buyer]=isnull(STUFF((select distinct ','+B.UserName from 
TRN.MasterOrder XMOI
INNER JOIN TRN.MasterOrderItem I ON I.MasterOrderId=XMOI.Id
INNER JOIN TRN.SalesOrder SO ON SO.MasterOrderItemId=I.Id
LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId	  
where so.ContractId=C.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
,ItemNo=isnull(STUFF((select distinct ','+I.Id from TRN.MasterOrderItem I 
INNER JOIN TRN.SalesOrder SO ON SO.MasterOrderItemId=I.Id
where So.ContractId=C.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
                            FROM [dbo].[Contract] C
                            JOIN [HKP].[Party] AS P ON C.CustomerId=P.Id 
							LEFT JOIN dbo.MasterLC LC ON LC.Id = C.MasterLCId
							LEFT JOIN [HKP].[Party] AS PM ON C.MarketingCommisssionId=PM.Id 
                            WHERE C.PlantId='" + PlantId + "') AS TEMP WHERE " + strkey + " ORDER BY TEMP.AddedDate desc";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetContractTermsAndConditionsList(string ContractId)
        {
            try
            {
                string sql = @"SELECT CT.*,TC.Sequence,TC.Code,TC.ShortName,TC.StandardName,TC.UserName,TC.Description  FROM [dbo].[ContractTermsAndConditions] CT
                            LEFT JOIN HKP.TermsAndConditions TC ON TC.Id=CT.TermsAndConditionsId
                            WHERE CT.ContractId='" + ContractId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetContractDetail(string partyId, string contractId)
        {
            try
            {
                var sql = @"SELECT SUM(A.TotalQty) TotalQty,C.Code 
                    FROM TRN.MasterOrder A
                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                    WHERE PartyId='" + partyId + @"' AND ContractId='" + contractId + @"'
                    GROUP BY A.TotalQty,C.Code";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetContractListByCustomer(string customerId)
        {
            try
            {
                string sql = @"SELECT Active=CAST (0 AS bit),C.*, P.UserName AS CustomerName
                            FROM [dbo].[Contract] C
                            JOIN [HKP].[Party] AS P ON C.CustomerId=P.Id 
                            WHERE C.MasterLCId IS NULL AND C.CustomerId='" + customerId + "' ORDER BY C.CustomerId";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSavedContractList(string masterLCId)
        {
            string sql = @"SELECT C.*, P.UserName AS CustomerName
                            FROM [dbo].[Contract] C
                            JOIN [HKP].[Party] AS P ON C.CustomerId=P.Id 
                            Where C.MasterLCId='" + masterLCId + "' ORDER BY C.CustomerId";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetMasterOrderList(string CompanyId,string PlantId)
        {
            try
            {
               
                var sql = @"SELECT A.Id AS  MasterOrderId,I.Id MasterOrderItemId, A.PartyId, P.UserName AS CustomerName, A.MasterOrderNo, A.CurrencyId, SI.TotalQty	
                            ,A.TotalQtyUOMId,PL.UserName,C.Code Currency, 0 Active,B.UserName Buyer, SO.Amount,SO.Qty,ISNULL(A.BuyerReferenceNo,'') BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'') OwnReferenceNo,ISNULL(I.BuyerReferenceNo,'') BuyerItem,ISNULL(I.OwnReferenceNo,'') OwnItem
                            ,MM.UserName MaterialMaster,MMA.ShortName Article
                            FROM [TRN].[MasterOrderItem] AS I
							inner join [TRN].[MasterOrder] AS A ON A.Id=I.MasterOrderId
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN [HKP].[Buyer] AS B ON B.Id=A.BuyerId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=I.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=I.ArticleId
                            LEFT JOIN (
							Select SUM(TotalQty) TotalQty,MasterOrderId,Id FROM [TRN].[MasterOrderItem] Group By MasterOrderId,Id
							) SI ON SI.Id=I.Id
                            LEFT JOIN (
							SELECT SUM(S.Qty) Qty, SUM(S.Qty*S.Rate) Amount, MOI.Id
							FROM TRN.SalesOrder S
							LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=S.MasterOrderItemId
							GROUP BY MOI.Id
							) SO ON SO.Id=I.Id
                            WHERE A.CompanyId='" + CompanyId + "'  AND A.PlantId='" + PlantId + "' AND I.ContractId IS NULL  ORDER BY P.Id";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMasterOrderListbyContract(string contractId)
        {

            try
            {

                var sql = @"SELECT A.Id AS  MasterOrderId,I.Id MasterOrderItemId, A.PartyId, P.UserName AS CustomerName, A.MasterOrderNo, A.CurrencyId, SI.TotalQty	
                            ,A.TotalQtyUOMId,PL.UserName,C.Code Currency, 0 Active,B.UserName Buyer, SO.Amount,SO.Qty,ISNULL(A.BuyerReferenceNo,'') BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'') OwnReferenceNo,ISNULL(I.BuyerReferenceNo,'') BuyerItem,ISNULL(I.OwnReferenceNo,'') OwnItem
                            ,MM.UserName MaterialMaster,MMA.ShortName Article
                            FROM [TRN].[MasterOrderItem] AS I
							inner join [TRN].[MasterOrder] AS A ON A.Id=I.MasterOrderId
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN [HKP].[Buyer] AS B ON B.Id=A.BuyerId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=I.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=I.ArticleId
                            LEFT JOIN (
							Select SUM(TotalQty) TotalQty,MasterOrderId,Id FROM [TRN].[MasterOrderItem] Group By MasterOrderId,Id
							) SI ON SI.Id=I.Id
                            LEFT JOIN (
							SELECT SUM(S.Qty) Qty, SUM(S.Qty*S.Rate) Amount, MOI.Id
							FROM TRN.SalesOrder S
							LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=S.MasterOrderItemId
							GROUP BY MOI.Id
							) SO ON SO.Id=I.Id
                            WHERE I.ContractId='" + contractId + "' ORDER BY P.Id";
                return _sqlRepository.GetDataCollection(sql) ;
            }
            catch (Exception ex)
            {
                throw ex;
            }



        }

        public IEnumerable<object> GetMasterOrderListbyCustomer(string CompanyId, string PlantId, string customerId)
        {
            try
            {
                var sql = @"SELECT A.Id AS  MasterOrderId,I.Id MasterOrderItemId, A.PartyId, P.UserName AS CustomerName, A.MasterOrderNo, A.CurrencyId, SI.TotalQty	
                            ,A.TotalQtyUOMId,PL.UserName,C.Code Currency, 0 Active,B.UserName Buyer, SO.Amount,SO.Qty,ISNULL(A.BuyerReferenceNo,'') BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'') OwnReferenceNo,ISNULL(I.BuyerReferenceNo,'') BuyerItem,ISNULL(I.OwnReferenceNo,'') OwnItem
                            ,MM.UserName MaterialMaster,MMA.ShortName Article
                             FROM [TRN].[MasterOrderItem] AS I
							inner join [TRN].[MasterOrder] AS A ON A.Id=I.MasterOrderId
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN [HKP].[Buyer] AS B ON B.Id=A.BuyerId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=I.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=I.ArticleId
                           LEFT JOIN (
							Select SUM(TotalQty) TotalQty,MasterOrderId,Id FROM [TRN].[MasterOrderItem] Group By MasterOrderId,Id
							) SI ON SI.Id=I.Id
                           LEFT JOIN (
							SELECT SUM(S.Qty) Qty, SUM(S.Qty*S.Rate) Amount, MOI.Id
							FROM TRN.SalesOrder S
							LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=S.MasterOrderItemId
							GROUP BY MOI.Id
							) SO ON SO.Id=I.Id
                            WHERE A.CompanyId='" + CompanyId + "'  AND A.PlantId='" + PlantId + "' AND A.PartyId='" + customerId + "' AND I.ContractId IS NULL ORDER BY P.Id";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMasterLcData(string contractId)
        {
            try
            {
                var sql = @"SELECT M.* FROM [dbo].[MasterLC] M
                            LEFT JOIN [dbo].[Contract] C ON C.MasterLCId=M.Id
                            WHERE C.Id='" + contractId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMasterLCList(string customerId)
        {
            string sql = @" SELECT MLC.Id, MLC.BenificiaryBankId, MLC.OpeningBank, MLC.OpeningDescription, MLC.LeinBank, MLC.LeinDescription, MLC.LCRef, FORMAT(MLC.LCDate,'dd-MMM-yyyy') LCDate, FORMAT(MLC.ExpiryDate,'dd-MMM-yyyy') ExpiryDate,
                             MLC.Amount, MLC.Type, MLC.Tenure, MLC.FinalDestinationId, MLC.PortOfLandingId, MLC.AddedBy,FORMAT(MLC.AddedDate,'dd-MMM-yyyy') AddedDate, MLC.AddedFromIP, MLC.UpdatedBy, FORMAT(MLC.UpdatedDate,'dd-MMM-yyyy') UpdatedDate, MLC.UpdatedFromIP, MLC.CurrencyId
                            ,OB.AccountTitle OpeningBank,CN.Code Currency, MLC.CustomerId, P.UserName PartyName 
                            FROM [dbo].[MasterLC] MLC                            
                            LEFT JOIN MST.BankMaster OB  ON OB.Id=MLC.BenificiaryBankId
                            LEFT JOIN SCS.Currency CN ON CN.Id=MLC.CurrencyId
                            LEFT JOIN HKP.Party P ON P.Id=MLC.CustomerId
                            WHERE MLC.CustomerId='" + customerId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetMasterLCDataList()
        {
            string sql = @"SELECT MLC.Id, MLC.BenificiaryBankId, MLC.OpeningBank, MLC.OpeningDescription, MLC.LeinBank, MLC.LeinDescription, MLC.LCRef, FORMAT(MLC.LCDate,'dd-MMM-yyyy') LCDate, FORMAT(MLC.ExpiryDate,'dd-MMM-yyyy') ExpiryDate,
                          MLC.Amount, MLC.Type, MLC.Tenure, MLC.FinalDestinationId, MLC.PortOfLandingId, MLC.AddedBy,FORMAT(MLC.AddedDate,'dd-MMM-yyyy') AddedDate, MLC.AddedFromIP, MLC.UpdatedBy, FORMAT(MLC.UpdatedDate,'dd-MMM-yyyy') UpdatedDate, MLC.UpdatedFromIP, MLC.CurrencyId
                         ,LB.UserName BenificiaryBank,CN.Code Currency, MLC.CustomerId, P.UserName PartyName 
                         FROM [dbo].[MasterLC] MLC
                         LEFT JOIN MST.BankMaster OB  ON OB.Id=MLC.BenificiaryBankId
                         LEFT JOIN HKP.Bank LB ON LB.Id=OB.BankId
                         LEFT JOIN SCS.Currency CN ON CN.Id=MLC.CurrencyId
                         LEFT JOIN HKP.Party P ON P.Id=MLC.CustomerId";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetContractFundData(string contractId)
        {
            string sql = @"SELECT A.Id,A.Sequence,A.FundUtilization,A.UserName,CONVERT(decimal(18,2),SUM(A.FundValue)) CostingValue,A.StandardValue [Percentage],A.StandardValue CostingValuePercentage,A.Remarks,A.CurrencyId,A.UserValue 
,CostingPercentage=CONVERT(decimal(18,2),CASE WHEN SUM(A.OrderValue)>0 THEN SUM(A.FundValue)/SUM(A.OrderValue) ELSE 0 END)
FROM
(
SELECT CF.Id,CFU.Id FundUtilization,CFU.UserName ,C.*,MOI.TotalQty,CFU.ValueType,(SO.Rate*SO.Qty)OrderValue
,FundValue=CASE WHEN CFU.ValueType='Percentage' THEN ISNULL(C.TotalGrossAmount,0)* ISNULL(MOI.TotalQty,0)*(1/NULLIF(CFU.StandardValue,0)) ELSE CFU.StandardValue END
,CFU.StandardValue,CFU.Sequence,CF.Remarks,CF.CurrencyId,CF.UserValue
FROM  dbo.ContractFundUtilization CFU 
LEFT JOIN TRN.SalesOrder SO ON SO.ContractId='"+ contractId + @"'
LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
LEFT JOIN (
 SELECT pc.OrderCostingMasterTemplateId,PC.CostingItemId,I.ContractFundId,pc.GrossAmount AS TotalGrossAmount FROM OrderPreCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId 
    UNION ALL SELECT pc.OrderCostingMasterTemplateId,PC.CostingItemId,I.ContractFundId,pc.Amount AS TotalGrossAmount FROM OrderPreCostingDirectProcess AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
    UNION ALL SELECT pc.OrderCostingMasterTemplateId,PC.CostingItemId,I.ContractFundId,pc.[Value] AS TotalGrossAmount FROM OrderPreCostingOperation AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
    UNION ALL SELECT pc.OrderCostingMasterTemplateId,PC.CostingItemId,I.ContractFundId,pc.Amount AS TotalGrossAmount FROM OrderPreCostingSalesExpense AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
    UNION ALL SELECT pc.OrderCostingMasterTemplateId,PC.CostingItemId,I.ContractFundId,pc.Amount AS TotalGrossAmount FROM OrderPreCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
    UNION ALL SELECT pc.OrderCostingMasterTemplateId,PC.CostingItemId,I.ContractFundId,pc.Amount AS TotalGrossAmount FROM OrderPreCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
) C ON C.OrderCostingMasterTemplateId=MOI.OrderCostingMasterTemplateId and C.ContractFundId = CFU.Id AND ISNULL(C.TotalGrossAmount,0)>0  
LEFT JOIN ContractFund CF ON CF.ContractId=SO.ContractId AND CFU.Id=CF.FundUtilization
) A
GROUP BY A.UserName,A.StandardValue,A.Sequence,A.FundUtilization,A.Id,A.Remarks,A.CurrencyId,A.UserValue ORDER BY A.Sequence";

            return _sqlRepository.GetDataCollection(sql);
        }


        public DataTable ProformaInvoiceSQL(string ContractId)
        {
            string strSQL;
            try
            {
                strSQL = @"select  moi.Id MasterOrderItemID,c.Id as ContractId
                                ,so.Rate,So.UpCharge
								,so.Qty
								,(so.Rate*so.Qty) as Amount
,mm.UserName MaterialDescription,mma.StandardName as Article,h.Code as HSNCode
                                ,c.description as Reference,
                                pc.UserName as CustomerName,u.UserName as UoM,
                                pbt.UserName as ConsigneeBilltoName,
                                pst.UserName as ConsigneeShiptoName
                                ,c.MarketingCommisssionCharge,
                                c.Remarks,
                                CONVERT(NUMERIC(10,2),ISNULL(c.MarketingCommisssionValue,0)) MarketingCommisssionValue,
                                c.InvoicingByAddress as ConsigneeBillToAddress,c.DeliveryByAddress as ConsigneeShipToAddress,cu.Code as CurrencyName,cu.Id CurrencyId,
                                p.UserName as MarketingCommissioningAgent,c.ContractNo,FORMAT(c.AddedDate,'dd-MMM-yyyy') AddedDate,PT.UserName PaymentTerm
                                ,SO.Id SONo,CONVERT(varchar,SO.DeliveryDate,5) DeliveryDate,DS.UserName Destination,moi.BuyerReferenceNo,C.AddedBy CreatedBy
                                from dbo.[Contract] C
                                left join  TRN.SalesOrder as so on C.Id=SO.ContractId
                                left join TRN.MasterOrderItem as moi on moi.Id=SO.MasterOrderItemId
                                left join HKP.Party as p on p.Id=c.MarketingCommisssionId
                                left join HKP.Party as pc on pc.Id=c.CustomerId
                                left join HKP.PartyPlant as pbt on pbt.Id=c.InvoicingPartyPlantId
                                left join HKP.PartyPlant as pst on pst.Id=c.DeliveryPartyPlantId
                                LEFT JOIN MST.Destination DS ON DS.Id=SO.DestinationId
                                left join MST.MaterialMaster as mm on mm.Id=moi.MaterialMasterId
                                left join MST.MaterialMasterArticle as mma on mma.MaterialMasterId=mm.Id AND MOI.ArticleId=MMA.Id
                                left join HKP.HSNCode as h on h.Id=mma.HSNCodeId
                                left join TRN.MasterOrder as mo on mo.id=moi.MasterOrderId
                                left join SCS.UnitOfMeasurement as u on u.Id=mo.TotalQtyUOMId
                                left join scs.Currency as cu on cu.Id=mo.CurrencyId
                                left join MSt.PaymentTerm PT ON PT.Id=MO.PaymentTermId
                                where c.Id='" + ContractId + "'";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable TermsAndConditionSQL(string ContractId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT ROW_NUMBER() OVER(ORDER BY TC.Sequence) RoWNo,
                        tc.Description as TermsAndConditions from dbo.ContractTermsAndConditions as ctc
                        left outer join hkp.TermsAndConditions as tc on tc.Id=ctc.TermsAndConditionsId
                        where ctc.ContractId='" + ContractId + "' Order By TC.Sequence ";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public DataTable dtConsigneeNameAddress(string partyId)
        {
            try
            {
                string sql = "";

                sql = @"SELECT P.UserName CustomerName,AM.Address1,CN.UserName CountryName FROM HKP.Party P
                LEFT JOIN MST.AddressMaster AM ON AM.Id=P.AddressMasterId
                LEFT JOIN SCS.District D ON D.Id=AM.DistrictId
                LEFT JOIN SCS.Country CN ON CN.Id=AM.CountryId
                
                WHERE P.Id='" + partyId + @"'";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public DataTable dtNameAddressVendor(string companyId)// Company Address
        {
            try
            {
                string sql = "";

                sql = @"SELECT C.UserName CompanyName,AM.Address1,CM.Phone1 Phone,D.UserName DistrictName,CN.UserName CountryName FROM ORG.Company C
                    LEFT JOIN MST.AddressMaster AM ON AM.Id=C.AddressMasterId
                    LEFT JOIN MST.ContactMaster CM ON CM.Id=C.ContactMasterId
                    LEFT JOIN SCS.District D ON D.Id=AM.DistrictId
                    LEFT JOIN SCS.Country CN ON CN.Id=AM.CountryId
                    WHERE C.Id='" + companyId + @"'
                ";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<object> GetContractItemDataList(string CompanyId, string PlantId, string contractId)
        {
            try
            {
                var sql = @"SELECT CI.Id,CI.BuyerItemRef,CI.OwnItemRef,CI.ContractId,A.Id AS  MasterOrderId,MOI.Id MasterOrderItemId, A.PartyId, P.UserName AS CustomerName, A.CurrencyId,CO.BaseCurrencyId, A.TotalQty,SO.Qty,SO.Rate,Amount=SO.Qty*SO.Rate	
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress								    
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,0 Active
                                    ,ISNULL(CNT.ContractNo,'')ContractNo,ISNULL(MLC.LCRef,'')LCRef
									,B.UserName Buyer,ISNULL(A.BuyerReferenceNo,'')BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'')OwnReferenceNo,ISNULL(MOI.BuyerReferenceNo,'') StyleNo,ISNULL(MOI.OwnReferenceNo,'') OwnStyleNo
                                    ,MM.UserName MaterialMaster,MMA.StandardName Article,MOI.TotalQty ItemQty,MOI.ContractId
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
									,MM.Id MaterialMasterId, MMA.Id ArticleId
                                    ,PONumber=  REPLACE(REPLACE(
										            STUFF((SELECT DISTINCT ','+CPO.PONumber from 
	                                                    TRN.SalesOrder XSO 
		                                                    JOIN [TRN].[CustomerPO] CPO ON CPO.Id=XSO.CustomerPOId
		                                                      JOIN trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    LEFT OUTER JOIN TRN.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                                WHERE MOI.Id=Xmoi.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                    ,'&amp;','&'), 'amp;', '')	
                            FROM [dbo].[ContractItems] CI
							JOIN TRN.MasterOrderItem MOI ON MOI.Id=CI.MasterOrderItemId
							LEFT JOIN [TRN].[MasterOrder] AS A ON A.Id=MOI.MasterOrderId
                            LEFT JOIN [ORG].[Company] AS CO ON CO.Id=A.CompanyId
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=A.PartyId  AND CP.PlantId=A.PlantId AND CP.PartyType='Customer'
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN HKP.Buyer AS B ON B.Id=A.BuyerId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN dbo.Contract CNT ON CNT.Id=MOI.ContractId
							LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=MOI.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=MOI.ArticleId
							LEFT JOIN(Select SUM(Qty) Qty,Rate,MasterOrderItemId From TRN.SalesOrder Group By MasterOrderItemId,Rate) SO ON SO.MasterOrderItemId=MOI.Id
                            WHERE A.CompanyId='" + CompanyId + "' AND A.PlantId='" + PlantId + "' AND CI.ContractId='" + contractId + "' --ORDER BY P.Id";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMasterOrderDataList(string CompanyId, string PlantId)
        {
            try
            {
                var sql = @"SELECT A.Id AS  MasterOrderId,MOI.Id MasterOrderItemId, A.PartyId, P.UserName AS CustomerName, A.CurrencyId,CO.BaseCurrencyId, A.TotalQty,SO.Qty,SO.Rate,Amount=SO.Qty*SO.Rate	
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress								    
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,0 Active
                                    ,ISNULL(CNT.ContractNo,'')ContractNo,ISNULL(MLC.LCRef,'')LCRef
									,B.UserName Buyer,ISNULL(A.BuyerReferenceNo,'')BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'')OwnReferenceNo,ISNULL(MOI.BuyerReferenceNo,'') StyleNo,ISNULL(MOI.OwnReferenceNo,'') OwnStyleNo
                                    ,MM.UserName MaterialMaster,MMA.StandardName Article,MOI.TotalQty ItemQty,MOI.ContractId
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    ,MM.Id MaterialMasterId, MMA.Id ArticleId
                                    ,PONumber=  REPLACE(REPLACE(
										            STUFF((SELECT DISTINCT ','+CPO.PONumber from 
	                                                    TRN.SalesOrder XSO 
		                                                    JOIN [TRN].[CustomerPO] CPO ON CPO.Id=XSO.CustomerPOId
		                                                      JOIN trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    LEFT OUTER JOIN TRN.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                                WHERE MOI.Id=Xmoi.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                    ,'&amp;','&'), 'amp;', '')	
                            FROM [TRN].[MasterOrder] AS A
                            LEFT JOIN [ORG].[Company] AS CO ON CO.Id=A.CompanyId
							JOIN TRN.MasterOrderItem MOI ON MOI.MasterOrderId=A.Id
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=A.PartyId  AND CP.PlantId=A.PlantId AND CP.PartyType='Customer'
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN HKP.Buyer AS B ON B.Id=A.BuyerId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN dbo.Contract CNT ON CNT.Id=MOI.ContractId
							LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=MOI.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=MOI.ArticleId
							LEFT JOIN(Select SUM(Qty) Qty,Rate,MasterOrderItemId From TRN.SalesOrder Group By MasterOrderItemId,Rate) SO ON SO.MasterOrderItemId=MOI.Id
                            WHERE A.CompanyId='" + CompanyId + "' AND A.PlantId='" + PlantId + "' --ORDER BY P.Id";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSalesOrderList(string CompanyId, string PlantId, string customerId)
        {
            try
            {

                var sql = @"SELECT Flags=CAST(CASE WHEN SO.ContractId IS NULL THEN 0 ELSE 1 END AS BIT),SO.Id SalesOrderId,SO.ContractId,A.Id AS  MasterOrderId,I.Id MasterOrderItemId, A.PartyId, P.UserName AS CustomerName, A.MasterOrderNo, A.CurrencyId, SO.Qty TotalQty	
                            ,A.TotalQtyUOMId,PL.UserName,C.Code Currency,B.UserName Buyer, (SO.Qty*SO.Rate)Amount,SO.Qty,ISNULL(A.BuyerReferenceNo,'') BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'') OwnReferenceNo,ISNULL(I.BuyerReferenceNo,'') BuyerItem,ISNULL(I.OwnReferenceNo,'') OwnItem
                            ,MM.UserName MaterialMaster,MMA.ShortName Article,po.PONumber
                            FROM TRN.SalesOrder SO
							INNER JOIN [TRN].[MasterOrderItem] AS I ON I.Id=SO.MasterOrderItemId
							INNER JOIN [TRN].[MasterOrder] AS A ON A.Id=I.MasterOrderId
                            INNER JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN TRN.CustomerPO PO ON PO.Id=SO.CustomerPOId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN [HKP].[Buyer] AS B ON B.Id=A.BuyerId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=I.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=I.ArticleId
                            WHERE A.CompanyId='" + CompanyId + "' AND A.PlantId='"+ PlantId + "' AND A.PartyId='"+ customerId + "' AND SO.ContractId IS NULL ORDER BY P.Id";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEditSalesOrderList(string CompanyId, string PlantId, string customerId, string contractId)
        {
            try
            {

                var sql = @"SELECT * FROM(SELECT Flags=CAST(CASE WHEN SO.ContractId IS NULL THEN 0 ELSE 1 END AS BIT),SO.Id SalesOrderId,SO.ContractId,A.Id AS  MasterOrderId,I.Id MasterOrderItemId, A.PartyId, P.UserName AS CustomerName, A.MasterOrderNo, A.CurrencyId, SO.Qty TotalQty	
                            ,A.TotalQtyUOMId,PL.UserName,C.Code Currency,B.UserName Buyer, (SO.Qty*SO.Rate)Amount,SO.Qty,ISNULL(A.BuyerReferenceNo,'') BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'') OwnReferenceNo,ISNULL(I.BuyerReferenceNo,'') BuyerItem,ISNULL(I.OwnReferenceNo,'') OwnItem
                            ,MM.UserName MaterialMaster,MMA.ShortName Article,po.PONumber
                            FROM TRN.SalesOrder SO
								INNER JOIN [TRN].[MasterOrderItem] AS I ON I.Id=SO.MasterOrderItemId
							INNER JOIN [TRN].[MasterOrder] AS A ON A.Id=I.MasterOrderId
                            INNER JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN TRN.CustomerPO PO ON PO.Id=SO.CustomerPOId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN [HKP].[Buyer] AS B ON B.Id=A.BuyerId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=I.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=I.ArticleId
                            WHERE A.CompanyId='" + CompanyId+ @"'  AND A.PlantId='" + PlantId + @"' AND A.PartyId='" + customerId + @"' AND SO.ContractId IS NULL 
UNION 
							SELECT Flags=CAST(CASE WHEN SO.ContractId IS NULL THEN 0 ELSE 1 END AS BIT),SO.Id SalesOrderId,SO.ContractId,A.Id AS  MasterOrderId,I.Id MasterOrderItemId, A.PartyId, P.UserName AS CustomerName, A.MasterOrderNo, A.CurrencyId, SO.Qty TotalQty	
                            ,A.TotalQtyUOMId,PL.UserName,C.Code Currency,B.UserName Buyer, (SO.Qty*SO.Rate)Amount,SO.Qty,ISNULL(A.BuyerReferenceNo,'') BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'') OwnReferenceNo,ISNULL(I.BuyerReferenceNo,'') BuyerItem,ISNULL(I.OwnReferenceNo,'') OwnItem
                            ,MM.UserName MaterialMaster,MMA.ShortName Article,po.PONumber
                            FROM TRN.SalesOrder SO
								INNER JOIN [TRN].[MasterOrderItem] AS I ON I.Id=SO.MasterOrderItemId
							INNER JOIN [TRN].[MasterOrder] AS A ON A.Id=I.MasterOrderId
                            INNER JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN TRN.CustomerPO PO ON PO.Id=SO.CustomerPOId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN [HKP].[Buyer] AS B ON B.Id=A.BuyerId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=I.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=I.ArticleId
                            WHERE A.CompanyId='" + CompanyId + @"'  AND A.PlantId='" + PlantId + @"' AND A.PartyId='" + customerId + @"' AND SO.ContractId='"+ contractId + "')A Order BY A.Flags desc";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}


