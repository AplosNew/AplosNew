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
using System.Drawing;

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using static Library.Service.Helpers.ReportUtility;
using Library.Model.Parties;
using System.Collections.Specialized;

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


        public IEnumerable<object> GetContractList(string column, string value, string PlantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                string sql = @"select * from (SELECT C.Id, C.CustomerId,C.BankId, C.IsLC, C.AddedBy, C.AddedDate, C.AddedFromIP, C.UpdatedBy, C.UpdatedDate, C.UpdatedFromIP, C.MasterLCId, 
isnull(C.ContractNo,'')ContractNo, C.TotalQty, C.SOQty, C.Amount, C.Description, isnull(C.UDNo,'')UDNo,C.FileNo, C.UDDate, C.ContractDate, C.IsPrint,C. IsMarketingCommisssionApplicable, 
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
where So.ContractId=C.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),''),B.AccountTitle Bank,C.LCRequiredDaysfromShipment
                            FROM [dbo].[Contract] C
                            JOIN [HKP].[Party] AS P ON C.CustomerId=P.Id 
							LEFT JOIN dbo.MasterLC LC ON LC.Id = C.MasterLCId
							LEFT JOIN [HKP].[Party] AS PM ON C.MarketingCommisssionId=PM.Id
                            LEFT JOIN MST.BankMaster B ON B.Id=C.BankId
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
,PaymentTerm=isnull(STUFF((select distinct ','+PT.UserName from MST.PaymentTerm AS PT
Left join TRN.MasterOrder XMOI on PT.Id = XMOI.PaymentTermId
INNER JOIN TRN.MasterOrderItem I ON I.MasterOrderId=XMOI.Id
INNER JOIN TRN.SalesOrder SO ON SO.MasterOrderItemId=I.Id	  
where so.ContractId=C.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
,PaymentTermId=isnull(STUFF((select distinct ','+PT.Id from MST.PaymentTerm AS PT
Left join TRN.MasterOrder XMOI on PT.Id = XMOI.PaymentTermId
INNER JOIN TRN.MasterOrderItem I ON I.MasterOrderId=XMOI.Id
INNER JOIN TRN.SalesOrder SO ON SO.MasterOrderItemId=I.Id	  
where so.ContractId=C.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
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
,PaymentTerm=isnull(STUFF((select distinct ','+PT.UserName from MST.PaymentTerm AS PT
Left join TRN.MasterOrder XMOI on PT.Id = XMOI.PaymentTermId
INNER JOIN TRN.MasterOrderItem I ON I.MasterOrderId=XMOI.Id
INNER JOIN TRN.SalesOrder SO ON SO.MasterOrderItemId=I.Id	  
where so.ContractId=C.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
,PaymentTermId=isnull(STUFF((select distinct ','+PT.Id from MST.PaymentTerm AS PT
Left join TRN.MasterOrder XMOI on PT.Id = XMOI.PaymentTermId
INNER JOIN TRN.MasterOrderItem I ON I.MasterOrderId=XMOI.Id
INNER JOIN TRN.SalesOrder SO ON SO.MasterOrderItemId=I.Id	  
where so.ContractId=C.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
,[Buyer]=isnull(STUFF((select distinct ','+B.UserName from 
TRN.MasterOrder XMOI
INNER JOIN TRN.MasterOrderItem I ON I.MasterOrderId=XMOI.Id
INNER JOIN TRN.SalesOrder SO ON SO.MasterOrderItemId=I.Id
LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId	  
where so.ContractId=C.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
,ItemNo=isnull(STUFF((select distinct ','+I.Id from TRN.MasterOrderItem I 
INNER JOIN TRN.SalesOrder SO ON SO.MasterOrderItemId=I.Id
where So.ContractId=C.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),''),LC.LCRef MasterLCNo,B.AccountTitle Bank
                            FROM [dbo].[Contract] C
                            LEFT JOIN [HKP].[Party] AS P ON C.CustomerId=P.Id 
                            LEFT JOIN dbo.MasterLC LC ON LC.Id=C.MasterLCId
                            LEFT JOIN MST.BankMaster B ON B.Id=C.BankId
                            Where C.MasterLCId='" + masterLCId + "' ORDER BY C.CustomerId";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetMasterOrderList(string CompanyId, string PlantId)
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
                return _sqlRepository.GetDataCollection(sql);
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
            string sql = @"SELECT MLC.Id, MLC.BenificiaryBankId, NB.BankName OpeningBank, MLC.OpeningDescription, MLC.LeinBank, MLC.LeinDescription, MLC.LCRef, FORMAT(MLC.LCDate,'dd-MMM-yyyy') LCDate, FORMAT(MLC.ExpiryDate,'dd-MMM-yyyy') ExpiryDate,
							FORMAT(MLC.ExpiryDate,'dd-MMM-yyyy') ExpiryDate, MLC.Amount, MLC.Type, MLC.Tenure, MLC.FinalDestinationId,MLC.PortOfLandingId,
							PR.UserName PortOfLanding, MLC.AddedBy,FORMAT(MLC.AddedDate,'dd-MMM-yyyy') AddedDate, MLC.AddedFromIP, MLC.UpdatedBy, 
							FORMAT(MLC.UpdatedDate,'dd-MMM-yyyy') UpdatedDate, MLC.UpdatedFromIP, MLC.CurrencyId,LB.UserName BenificiaryBank,CN.Code Currency, 
							MLC.CustomerId, P.UserName PartyName,MLC.Version,FORMAT(mlc.LCShipmentDate,'dd-MMM-yyyy')LCShipmentDate,mlc.ShipmentModeId,sm.UserName ShipmentMode,FORMAT(mlc.AmendmentDate,'dd-MMM-yyyy')AmendmentDate
							,mlc.PortOfLoadingId,prl.UserName PortOfLoading,MLC.Remarks,MLC.DescriptionOfGoodsAndOrServices, MLC.OpeningBankId
							,LCC.Clause1 , LCC.Clause2 , LCC.Clause3 , LCC.Clause4 , LCC.Clause5 , LCC.Clause6, LCC.Clause7 , LCC.Clause8 , LCC.Clause9, LCC.Clause10,MLC.NegotiatingBankId 
							,MLC.InsuranceCompany , MLC.InsuranceCoverNote , MLC.InsuranceCompanyDescription
                         FROM [dbo].[MasterLC] MLC
                         LEFT JOIN MST.BankMaster OB  ON OB.Id=MLC.BenificiaryBankId
                         LEFT JOIN HKP.Bank LB ON LB.Id=OB.BankId
                         LEFT JOIN SCS.Currency CN ON CN.Id=MLC.CurrencyId
                         LEFT JOIN HKP.Party P ON P.Id=MLC.CustomerId
                         LEFT JOIN mst.ShipMode AS sm ON sm.Id=mlc.ShipmentModeId
                         LEFT JOIN [MST].[Port] PR ON PR.Id=mlc.PortOfLandingId
                         LEFT JOIN [MST].[Port] PRL ON PRL.Id=mlc.PortOfLoadingId
                         LEFT JOIN [dbo].[NegotiatingBank] NB ON NB.Id=mlc.OpeningBankId
						 left join dbo.lcclauses  LCC on LCC.MasterLCId = MLC.Id
						 order By MLC.AddedDate Desc";
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
LEFT JOIN TRN.SalesOrder SO ON SO.ContractId='" + contractId + @"'
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
                                ,mm.UserName MaterialDescription,Article=CASE WHEN ISNULL(moi.LCArticle,'')<>'' THEN moi.LCArticle WHEN ISNULL(AA.ArticlePartyName,'')<>'' THEN AA.ArticlePartyName ELSE MMA.StandardName END,h.Code as HSNCode
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
                                ,BM.AccountTitle Bank,BM.AccountNumber,BB.UserName AS BankBranch,BB.IFSCCode , BB.SWIFTCode , AM.Address1 Addresss
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
                                LEFT JOIN dbo.ArticleAlias AA ON AA.ArticleId=MMA.Id AND AA.MasterOrderItemID=MOI.Id
                                left join HKP.HSNCode as h on h.Id=mma.HSNCodeId
                                left join TRN.MasterOrder as mo on mo.id=moi.MasterOrderId
                                left join SCS.UnitOfMeasurement as u on u.Id=mo.TotalQtyUOMId
                                left join scs.Currency as cu on cu.Id=mo.CurrencyId
                                left join MSt.PaymentTerm PT ON PT.Id=MO.PaymentTermId
								LEFT JOIN MST.BankMaster BM ON BM.Id=C.BankId
                                LEFT JOIN HKP.BankBranch BB ON BB.Id = BM.BankBranchId
								left join mst.AddressMaster AM on AM.Id = BB.AddressMasterId 
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
                            ,A.TotalQtyUOMId,PL.UserName,C.Code Currency,B.UserName Buyer, (SO.Qty*SO.Rate)Amount,SO.Qty,ISNULL(A.BuyerReferenceNo,'') BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'') OwnReferenceNo,ISNULL(I.BuyerReferenceNo,'') BuyerItem,ISNULL(I.OwnReferenceNo,'') OwnItem , PT.UserName PaymentTerm,A.PaymentTermId
                            ,MM.UserName MaterialMaster,MMA.ShortName Article,po.PONumber,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy')DeliveryDate,CNT.ContractNo
                            FROM TRN.SalesOrder SO
                            Left JOIN dbo.Contract CNT ON CNT.Id=SO.ContractId
							INNER JOIN [TRN].[MasterOrderItem] AS I ON I.Id=SO.MasterOrderItemId
							INNER JOIN [TRN].[MasterOrder] AS A ON A.Id=I.MasterOrderId
                            INNER JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
							Left join MST.PaymentTerm AS PT on PT.Id = A.PaymentTermId
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN TRN.CustomerPO PO ON PO.Id=SO.CustomerPOId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN [HKP].[Buyer] AS B ON B.Id=A.BuyerId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=I.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=I.ArticleId
                            WHERE A.CompanyId='" + CompanyId + "' AND A.PlantId='" + PlantId + "' AND A.PartyId='" + customerId + "' AND SO.ContractId IS NULL ORDER BY P.Id";
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
                            ,MM.UserName MaterialMaster,MMA.ShortName Article,po.PONumber,PT.UserName PaymentTerm,A.PaymentTermId,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy')DeliveryDate,CNT.ContractNo
                            FROM TRN.SalesOrder SO
                            LEFT JOIN dbo.Contract CNT ON CNT.Id=SO.ContractId
							INNER JOIN [TRN].[MasterOrderItem] AS I ON I.Id=SO.MasterOrderItemId
							INNER JOIN [TRN].[MasterOrder] AS A ON A.Id=I.MasterOrderId
                            INNER JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            Left join MST.PaymentTerm AS PT on PT.Id = A.PaymentTermId
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN TRN.CustomerPO PO ON PO.Id=SO.CustomerPOId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN [HKP].[Buyer] AS B ON B.Id=A.BuyerId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=I.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=I.ArticleId
                            WHERE A.CompanyId='" + CompanyId + @"'  AND A.PlantId='" + PlantId + @"' AND A.PartyId='" + customerId + @"' AND SO.ContractId='" + contractId + "')A Order BY A.Flags desc";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSalesOrderListByContract(string customerId, string contractId)
        {
            try
            {

                var sql = @"SELECT I.MasterOrderId,I.Id MasterOrderItemId,MMA.ShortName Article,AAP.UserName CustomerArticle,I.LCArticle
						   ,SalesOrderId=  REPLACE(REPLACE(
										            STUFF((SELECT DISTINCT ','+XSO.Id from 
	                                                    TRN.SalesOrder XSO 
		                                                LEFT JOIN dbo.[Contract] C ON C.Id=XSO.ContractId
			                                                WHERE XSO.MasterOrderItemId=I.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                    ,'&amp;','&'), 'amp;', '')	
							 ,DeliveryDate=  REPLACE(REPLACE(
										            STUFF((SELECT DISTINCT ','+FORMAT(XSO.DeliveryDate,'dd-MMM-yyyy') from 
	                                                    TRN.SalesOrder XSO 
		                                                LEFT JOIN dbo.[Contract] C ON C.Id=XSO.ContractId
			                                                WHERE XSO.MasterOrderItemId=I.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                    ,'&amp;','&'), 'amp;', '')	
                            FROM [TRN].[MasterOrderItem] AS I
							INNER JOIN [TRN].[MasterOrder] AS A ON A.Id=I.MasterOrderId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=I.ArticleId
							LEFT JOIN dbo.ArticleAlias AA ON AA.ArticleId=MMA.Id AND AA.MasterOrderItemId=I.Id
                            LEFT JOIN HKP.Party AAP ON AAP.Id=AA.Partyid
                            WHERE  A.PartyId='"+customerId+@"' AND I.Id IN(Select MasterOrderItemId From TRN.SalesOrder Where ContractId "+contractId+")";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetContarctData()
        {
            try
            {
                string sql = "";

                sql = @"SELECT C.Id,C.FileNo,B.AccountTitle BankName,FORMAT(SO.ShipmentStartDate,'dd-MMM-yyyy')ShipmentStartDate,FORMAT(SO.ShipmentEndDate,'dd-MMM-yyyy') ShipmentEndDate
,C.ContractNo,C.TotalQty,C.Amount,ISNULL(SM.ShipmentQty,0) ShipmentQty,ISNULL(SM.ShippedValue,0) ShippedValue,BalanceQty=C.TotalQty-ISNULL(SM.ShipmentQty,0),BalanceLienValue=C.Amount-ISNULL(SM.ShippedValue,0),C.Remarks,Buyer=REPLACE(REPLACE(STUFF((select distinct ', '+B.UserName FROM 
                                        HKP.Buyer B 
										LEFT JOIN TRN.MasterOrder M ON B.Id=M.BuyerId
										LEFT JOIN TRN.MasterOrderItem MI ON MI.MasterOrderId=M.Id
										LEFT JOIN TRN.SalesOrder S ON S.MasterOrderItemId=MI.Id
                                        WHERE S.ContractId=C.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
FROM dbo.Contract C
LEFT JOIN MST.BankMaster B ON B.Id=C.BankId
LEFT JOIN(Select MIN(DeliveryDate)ShipmentStartDate,MAX(DeliveryDate)ShipmentEndDate,ContractId
FROM TRN.SalesOrder Group By ContractId) SO ON SO.ContractId=C.Id
LEFT JOIN (
Select SUM(S.TransactionQty)ShipmentQty,SUM(S.TransactionAmount)ShippedValue,SO.ContractId from TRN.SalesMaterial S
LEFT JOIN TRN.SalesOrder SO ON SO.Id=S.SalesOrderId
Group By SO.ContractId
) SM ON SM.ContractId=C.Id
Where C.ContractNo<>''
Order By B.AccountTitle";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IWorkbook GetContarctWorkbook(string CompanyId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;


            DataTable data = GetContarctData();

            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Data";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, " Bank Name", 14, ExcelHAlign.HAlignLeft);
            int ColBN = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, " File", 5, ExcelHAlign.HAlignLeft);
            int ColF = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shipment Start Date", 20, ExcelHAlign.HAlignLeft);
            int ColSSD = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Shipment End Date", 20, ExcelHAlign.HAlignLeft);
            int ColSED = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Contract No", 25, ExcelHAlign.HAlignLeft);
            int ColCN = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Qty", 8, ExcelHAlign.HAlignLeft);
            int ColQ = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Value", 12, ExcelHAlign.HAlignLeft);
            int ColV = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shipment Qty", 14, ExcelHAlign.HAlignLeft);
            int ColSQ = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shipped Value", 14, ExcelHAlign.HAlignLeft);
            int ColSV = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Balance Qty", 12, ExcelHAlign.HAlignLeft);
            int ColBQ = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Balance Lien Value", 12, ExcelHAlign.HAlignLeft);
            int ColBLV = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 12, ExcelHAlign.HAlignLeft);
            int ColR = COL;



            endCol = COL;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

            ROW++;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;

            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColBN].Text = data.Rows[i]["BankName"].ToString();
                sheet[ROW, ColF].Text = data.Rows[i]["FileNo"].ToString();
                sheet[ROW, ColSSD].Text = data.Rows[i]["ShipmentStartDate"].ToString();
                //sheet[ROW, ColSED].Number = clsStaticInfo.dbl(data.Rows[i]["ShipmentEndDate"].ToString());
                sheet[ROW, ColSED].Text = data.Rows[i]["ShipmentEndDate"].ToString();
                sheet[ROW, ColCN].Text = data.Rows[i]["ContractNo"].ToString();
                sheet[ROW, ColQ].Text = data.Rows[i]["TotalQty"].ToString();
                sheet[ROW, ColV].Text = data.Rows[i]["Amount"].ToString();
                sheet[ROW, ColSQ].Text = data.Rows[i]["ShipmentQty"].ToString();
                sheet[ROW, ColSV].Text = data.Rows[i]["ShippedValue"].ToString();
                sheet[ROW, ColBQ].Text = data.Rows[i]["BalanceQty"].ToString();
                sheet[ROW, ColBLV].Text = data.Rows[i]["BalanceLienValue"].ToString();
                sheet[ROW, ColR].Text = data.Rows[i]["Remarks"].ToString();

                ROW++;

            }
            sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1


            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "BANK LIEN REPORT", CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private class Combination
        {
            public string GroupKey { get; set; } = "";
            public int Row { get; set; } = 0;
        }
        public IWorkbook GetContarctWorkbookExcel(string companyGroupId, string companyId, string PlantId)
        {
            try
            {
                #region Variable
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataView dvDaily = null;
                DataSet dsCmp = null;
                //clsReport objRpt = null;
                var objRpt = new clsReport();


                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //Create dataset
                DataTable dtManPBSummary = GetContarctData();

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 5;

                #region ColumnHeaderVariables              
                int cFileNo,cBuyer = 0; int cTQ = 0; int cA = 0; int cSV; int cSQ; int cBQ = 0; int cBLV = 0; int cR = 0; int cBN = 0; int cSSD = 0; int cCN = 0; int cSED = 0;
                #endregion
                #region ColumnHeaders
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Bank Name", 14, ExcelHAlign.HAlignCenter); cBN = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "File", 8, ExcelHAlign.HAlignCenter); cFileNo = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Buyer", 25, ExcelHAlign.HAlignCenter); cBuyer = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shipment Start Date", 20, ExcelHAlign.HAlignCenter); cSSD = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shipment End Date", 20, ExcelHAlign.HAlignCenter); cSED = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Contract No", 25, ExcelHAlign.HAlignCenter); cCN = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Total Qty", 8, ExcelHAlign.HAlignCenter); cTQ = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Amount", 12, ExcelHAlign.HAlignCenter); cA = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shipment Qty", 14, ExcelHAlign.HAlignCenter); cSQ = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shipped Value", 14, ExcelHAlign.HAlignCenter); cSV = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BalanceQty", 12, ExcelHAlign.HAlignCenter); cBQ = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Balance Lien Value", 14, ExcelHAlign.HAlignCenter); cBLV = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarks", 14, ExcelHAlign.HAlignCenter); cR = xlsCol; xlsCol++;
                sheet1.Range[xlsRow, cBN, xlsRow, cR].CellStyle.FillBackground = ExcelKnownColors.Light_yellow;
                sheet1.Range[xlsRow, cBN, xlsRow, cR].RowHeight = 15;
                sheet1.Range[xlsRow, cBN, xlsRow, cR].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, cBN, xlsRow, cR].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                var orgCollist = xlsCol;
                xlsRow++;
                endXlsCol = xlsCol;

                #endregion

                if (dtManPBSummary.Rows.Count > 0)
                {


                    #region New

                    string _cgrp1 = string.Empty;
                    string _grp1 = string.Empty;
                    string _grp2 = string.Empty;
                    string _grp3 = string.Empty;
                    string _sgrp3 = string.Empty;
                    string _grp4 = string.Empty;
                    string _grp5 = string.Empty;



                    var catFRow = xlsRow;
                    var catcGrp2FRow = xlsRow;
                    var catGrp2FRow = xlsRow;
                    var catsGrp3FRow = xlsRow;
                    var catGrp3FRow = xlsRow;
                    var catGrp4FRow = xlsRow;
                    var catGrp5FRow = xlsRow;

                    ArrayList rowList = new ArrayList();
                    var lastMPGroup = string.Empty;

                    Dictionary<string, Combination> dicGroup = new Dictionary<string, Combination>();

                    string strGroupBankName = dtManPBSummary.Rows[0]["BankName"].ToString();

                    dicGroup.Add("BankName", new Combination { GroupKey = strGroupBankName, Row = xlsRow });

                    DataRow dr = dtManPBSummary.NewRow();
                    dtManPBSummary.Rows.Add(dr);
                    for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                    {
                        var catLRow = xlsRow;

                        strGroupBankName = dtManPBSummary.Rows[i]["BankName"].ToString();


                        if (dicGroup["BankName"].GroupKey != strGroupBankName)
                        {
                            rowList.Add(xlsRow);
                            oRU.SetHeadText(sheet1, xlsRow, 6, " Subtotal:");

                            sheet1.Range[xlsRow, cTQ].Formula = "=SUM(" + oRU.GetColumnNameForXls(cTQ) + catFRow + ":" + oRU.GetColumnNameForXls(cTQ) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cTQ].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cA].Formula = "=SUM(" + oRU.GetColumnNameForXls(cA) + catFRow + ":" + oRU.GetColumnNameForXls(cA) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cA].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cSQ].Formula = "=SUM(" + oRU.GetColumnNameForXls(cSQ) + catFRow + ":" + oRU.GetColumnNameForXls(cSQ) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cSQ].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cSV].Formula = "=SUM(" + oRU.GetColumnNameForXls(cSV) + catFRow + ":" + oRU.GetColumnNameForXls(cSV) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cSV].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cBQ].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBQ) + catFRow + ":" + oRU.GetColumnNameForXls(cBQ) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cBQ].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cBLV].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBLV) + catFRow + ":" + oRU.GetColumnNameForXls(cBLV) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cBLV].BorderAround(ExcelLineStyle.Hair);


                            sheet1.Range[xlsRow, cTQ, xlsRow, cBLV].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, cBN, xlsRow, cR].CellStyle.FillBackground = ExcelKnownColors.Light_yellow;

                            xlsRow++;


                            sheet1.Range[dicGroup["BankName"].Row, cBN, xlsRow - 1, cBN].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["BankName"].Row, cBN].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["BankName"].Row, cBN].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["BankName"].Row, cBN, xlsRow - 1, cBN].Merge();


                            dicGroup["BankName"].Row = xlsRow;
                            dicGroup["BankName"].GroupKey = strGroupBankName;
                        }


                        #endregion

                        sheet1.Range[xlsRow, cBN].Text = dtManPBSummary.Rows[i]["BankName"].ToString();
                        oRU.SetTextBdr(ref sheet1, xlsRow, cFileNo, dtManPBSummary.Rows[i]["FileNo"].ToString());
                        oRU.SetTextBdr(ref sheet1, xlsRow, cBuyer, dtManPBSummary.Rows[i]["Buyer"].ToString());
                        oRU.SetTextBdr(ref sheet1, xlsRow, cSSD, dtManPBSummary.Rows[i]["ShipmentStartDate"].ToString());
                        oRU.SetTextBdr(ref sheet1, xlsRow, cSED, dtManPBSummary.Rows[i]["ShipmentEndDate"].ToString());
                        oRU.SetTextBdr(ref sheet1, xlsRow, cCN, dtManPBSummary.Rows[i]["ContractNo"].ToString());
                        oRU.SetTextBorder(ref sheet1, xlsRow, cTQ, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["TotalQty"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cA, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["Amount"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cSQ, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["ShipmentQty"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cSV, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["ShippedValue"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cBQ, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["BalanceQty"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cBLV, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["BalanceLienValue"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cR, dtManPBSummary.Rows[i]["Remarks"].ToString());//
                        xlsRow++;
                    }
                    //  xlsRow += 1;
                   // sheet1.AutoFilters.FilterRange = sheet1.Range[catFRow - 1, 1, xlsRow, endXlsCol];


                    oRU.SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (cTQ - 1)].Merge();
                    sheet1.Range[xlsRow, cTQ].Formula = oRU.GetFormulaGrandTotal(rowList, cTQ);
                    sheet1.Range[xlsRow, cA].Formula = oRU.GetFormulaGrandTotal(rowList, cA);
                    sheet1.Range[xlsRow, cSQ].Formula = oRU.GetFormulaGrandTotal(rowList, cSQ);
                    sheet1.Range[xlsRow, cSV].Formula = oRU.GetFormulaGrandTotal(rowList, cSV);
                    sheet1.Range[xlsRow, cBQ].Formula = oRU.GetFormulaGrandTotal(rowList, cBQ);
                    sheet1.Range[xlsRow, cBLV].Formula = oRU.GetFormulaGrandTotal(rowList, cBLV);

                    sheet1.Range[xlsRow, 1, xlsRow, (cTQ - 1)].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cA].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cSQ].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cSV].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cBQ].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cBLV].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cTQ, xlsRow, cBLV].CellStyle.Font.Bold = true;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion
                    
                }
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet1, 2, "BANK LIEN REPORT", identity.CompanyId, identity.CompanyName, "");
                sheet1.Range[1, 2, 5, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                reportUtility.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);


                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public double GetSequence(string masterLcId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT ISNULL(Max(Sequence),0) AS Sequence FROM dbo.MasterLCAddInfo WHERE MasterLcId='" + masterLcId + "'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        public IEnumerable<object> GetMasterLCAddInfoData(string masterLcId)
        {
            try
            {

                var sql = @"SELECT * FROM dbo.MasterLCAddInfo WHERE MasterLcId='" + masterLcId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMasterLCTermsAndConditionsList(string masTerLCId)
        {
            try
            {
                string sql = @"SELECT CT.*,TC.Sequence,TC.Code,TC.ShortName,TC.StandardName,TC.UserName OriginUserName,TC.Description
,CAST((CASE WHEN TC.Type='Contract' THEN 1 ELSE 0 END) AS bit) AS IsContract
,CAST((CASE WHEN TC.Type='LetterOfCredit' THEN 1 ELSE 0 END) AS bit) AS IsMasterLC,TG.GroupName [Group]
FROM [dbo].[MasterLCTermsAndConditions] CT
LEFT JOIN HKP.TermsAndConditions TC ON TC.Id=CT.TermsAndConditionsId
LEFT JOIN dbo.TermsandConditionGroup TG ON TG.Id=TC.GroupId
WHERE CT.MasTerLCId='" + masTerLCId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMasterLCLCClausesList(string masTerLCId)
        {
            try
            {
                string sql = @"SELECT CT.*
FROM [dbo].[LCClauses] CT
                            WHERE CT.MasTerLCId='" + masTerLCId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetGoodWorkEntitySetupData(string goodWorkSetupId)
        {
            try
            {

                var sql = @"SELECT ES.*,E.Code,E.UserName,P.UserName Plant,D.UserName Division,SD.UserName SubDivision,U.UserName Unit,FORMAT(E.EffectiveDate,'dd-MMM-yyyy')EffectiveDate FROM dbo.GoodWorkEntitySetup ES
                    LEFT JOIN ORG.Entity E  ON E.Id=ES.EntityId
                    LEFT JOIN ORG.Division D ON D.Id=E.DivisionId
                    LEFT JOIN ORG.SubDivision SD ON SD.Id=E.SubDivisionId
                    LEFT JOIN ORG.Unit U ON U.Id=E.UnitId
                    LEFT JOIN ORG.Plant P ON P.Id=E.PlantId where  GoodWorkSetupId='" + goodWorkSetupId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetGoodWorkBudgetCodeSetupData(string goodWorkSetupId)
        {
            try
            { 
                var sql = @"SELECT GWB.*,E.Id EntityId,E.UserName EntityName,MB.IsOTEntitled,D.Id DivisionId,D.UserName Division,DP.Id DepartmentId
					,DP.UserName Department,S.Id SectionId,S.UserName Section,SS.Id SubSectionId,SS.UserName SubSection
					,DE.Id DesignationId,DE.UserName Designation,P.Activity,P.UserReportGroup UserGroup,PR.Id ProcessId
					,PR.UserName Process,EC.UserName EmployeeType,MB.Id BudgetId,MB.Code 
					FROM dbo.GoodWorkSetup GWS
					left join dbo.GoodWorkBudgetSetup GWB on GWB.GoodWorkSetUpId=GWS.Id
					left join mst.ManpowerBudget MB on MB.Id=GWB.BudgetId
                    LEFT JOIN ORG.Position P ON P.Id=MB.PositionId
					LEFT JOIN ORG.Entity E  ON E.Id=MB.EntityId
                    LEFT JOIN ORG.Division D ON D.Id=P.DivisionId
                    LEFT JOIN ORG.Department DP ON DP.Id=P.DepartmentId
                    LEFT JOIN ORG.Section S ON S.Id=P.SectionId                    
                    LEFT JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId                    
					LEFT JOIN hkp.Designation DE ON DE.Id=P.DesignationId					
					LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=DE.Id					
					LEFT JOIN hkp.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId				
					LEFT JOIN hkp.Process PR ON PR.Id=P.ProcessId 
                    where  GoodWorkSetupId='" + goodWorkSetupId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetGoodWorkAuthorityData(string goodWorkSetupId)
        {
            try
            {
                var sql = @"SELECT GWA.*,ei.SystemId EmployeeId,ei.EmployeeCode,ei.EmployeeName,C.Id CompanyId,C.UserName Company
                    ,P.Id PlantId,P.UserName Plant,LDEG.Id DesignationId,LDEG.UserName Designation,DP.Id DepartmentId
                    ,DP.UserName Department,S.Id SectionId,S.UserName Section,SS.Id SubSectionId,SS.UserName SubSection
                    ,L.Id LineId,L.UserName Line
                    FROM dbo.GoodWorkAuthoritySetUp GWA
                    left join EmployeeInformation ei on ei.SystemId=GWA.AuthorityId
                    left join org.Company C on C.Id=ei.CompanyId
                    left join org.Plant P on P.Id=ei.PlantId
                    LEFT JOIN HKP.LegalDesignation LDEG ON ei.LegalDesignationId=LDEG.Id
                    LEFT JOIN ORG.Department DP ON DP.Id=ei.DepartmentId
                    LEFT JOIN ORG.Section S ON S.Id=ei.SectionId                    
                    LEFT JOIN ORG.SubSection SS ON SS.Id=ei.SubSectionId 
                    LEFT JOIN ORG.Line L on L.Id=ei.LineId
                    where  GoodWorkSetupId='" + goodWorkSetupId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetGoodWorkCheckByData(string goodWorkSetupId)
        {
            try
            {
                var sql = @"SELECT GWC.*,ei.SystemId EmployeeId,ei.EmployeeCode,ei.EmployeeName,C.Id CompanyId,C.UserName Company
                    ,P.Id PlantId,P.UserName Plant,LDEG.Id DesignationId,LDEG.UserName Designation,DP.Id DepartmentId
                    ,DP.UserName Department,S.Id SectionId,S.UserName Section,SS.Id SubSectionId,SS.UserName SubSection
                    ,L.Id LineId,L.UserName Line
                    FROM dbo.GoodWorkCheckBySetUp GWC
                    left join EmployeeInformation ei on ei.SystemId=GWC.CheckById
                    left join org.Company C on C.Id=ei.CompanyId
                    left join org.Plant P on P.Id=ei.PlantId
                    LEFT JOIN HKP.LegalDesignation LDEG ON ei.LegalDesignationId=LDEG.Id
                    LEFT JOIN ORG.Department DP ON DP.Id=ei.DepartmentId
                    LEFT JOIN ORG.Section S ON S.Id=ei.SectionId                    
                    LEFT JOIN ORG.SubSection SS ON SS.Id=ei.SubSectionId 
                    LEFT JOIN ORG.Line L on L.Id=ei.LineId
                    where  GoodWorkSetupId='" + goodWorkSetupId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetNegotiatingBankList()
        {
            try
            {

                var sql = @"SELECT NB.*,C.userName Country FROM dbo.NegotiatingBank NB
LEFT JOIN SCS.Country C ON C.Id=NB.CountryId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetNegotiatingContractBankList()
        {
            try
            {

                var sql = @"Select BM.AccountTitle Text,B.Id Value  from MST.BankMaster BM
LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
Where BM.IsNegotiatingBank=1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetNegotiatingBankDataList(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"SELECT * FROM (SELECT NB.*,C.UserName Country FROM dbo.NegotiatingBank NB
                            LEFT JOIN SCS.Country C ON C.Id=NB.CountryId) AS TEMP WHERE " + strkey + " ORDER BY TEMP.AddedDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetRemarksControlList(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"SELECT * FROM (SELECT RC.*,AEI.EmployeeName ApprovedBy,IEI.EmployeeName InformTo FROM HKP.RemarksControl RC
LEFT JOIN dbo.EmployeeInformation AEI ON AEI.SystemId=RC.ApprovedById
LEFT JOIN dbo.EmployeeInformation IEI ON IEI.SystemId=RC.InformToId) AS TEMP WHERE " + strkey + " ORDER BY TEMP.AddedDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetRemarksControlCboList()
        {
            try
            {
                return _sqlRepository.GetDataCollection("SELECT Id as Value,Remarks AS Text FROM HKP.RemarksControl");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public double GetRemarksControlSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM HKP.RemarksControl");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        public IEnumerable<object> GetPaymentTermEnum()
        {
            try
            {
                var str = @"Select EnumName Value, UserName Text  from dbo.DefineEnum where Category='PaymentTerm'";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> GetCompanyPartyListNew(string companyGroupId, string companyId, string plantId, string column, string value, string customerVendor)
        {
            try
            {
                string temp = null;
                temp = customerVendor;

                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"select  * from (SELECT CheckState=CAST(0 AS bit),P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, CO.Code AS CountryCode, CO.UserName AS CountryName, S.Code AS StateCode, S.UserName AS StateName
                                    , RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
                                    , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
                                    , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
                                    , DGL.DownPaymentGLId, DGL.DownPaymentGLCode, DGL.DownPaymentGLName
                                    , DGL.DownPaymentBudgetId, DGL.DownPaymentBudgetCode, DGL.DownPaymentBudgetName
                                    , DGL.DownPaymentActivityId, DGL.DownPaymentActivityCode, DGL.DownPaymentActivityName
                                    , SGL.SuspenseGLId, SGL.SuspenseGLCode, SGL.SuspenseGLName
                                    , SGL.SuspenseBudgetId, SGL.SuspenseBudgetCode, SGL.SuspenseBudgetName
                                    , SGL.SuspenseActivityId, SGL.SuspenseActivityCode, SGL.SuspenseActivityName
                                    , CP.TaxApplicable, CP.IsTaxApplicableChangeable
									, (SELECT COUNT(Id) FROM [HKP].[PartyPlant] WHERE PartyId=P.Id) AS TotalPartyPlant
                                    FROM [HKP].[Party] AS P
                                    LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                                    LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                                    LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS DownPaymentGLId, GL.AccountCode AS DownPaymentGLCode, GL.UserName AS DownPaymentGLName
                                    , CPGL.BudgetMasterId AS DownPaymentBudgetId, B.Code AS DownPaymentBudgetCode, B.UserName AS DownPaymentBudgetName
                                    , CPGL.ActivityId AS DownPaymentActivityId, A.Code AS DownPaymentActivityCode, A.UserName AS DownPaymentActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.DownPaymentGL + @"'
                                    ) AS DGL ON DGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
										SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS SuspenseGLId, GL.AccountCode AS SuspenseGLCode, GL.UserName AS SuspenseGLName
										, CPGL.BudgetMasterId AS SuspenseBudgetId, B.Code AS SuspenseBudgetCode, B.UserName AS SuspenseBudgetName
										, CPGL.ActivityId AS SuspenseActivityId, A.Code AS SuspenseActivityCode, A.UserName AS SuspenseActivityName
										FROM [HKP].[CompanyPartyGL] AS CPGL
										LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
										WHERE CPGL.PartyGLType='" + PartyGLType.SuspenseGL + @"'
                                    ) AS SGL ON SGL.CompanyPartyId=CP.Id
                                    WHERE P.Archive=0 AND P.Active=1 AND P.CompanyGroupId='" + companyGroupId + "' AND CP.PartyType IN ('" + temp + "') AND CP.CompanyId='" + companyId + "' AND CP.PlantId='" + plantId + @"'
                                    AND P.Id IN(Select DISTINCT M.PartyId from TRN.MasterOrder M
                                    LEFT JOIN TRN.MasterOrderItem I ON I.MasterOrderId=M.Id
                                    LEFT JOIN TRN.SalesOrder S ON S.MasterOrderItemId=I.Id where S.OrderStatusId NOT IN('Closed','Cancelled'))
                                    ) AS TEMP WHERE " + strkey + " order by Code ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook GetLCPendingReportWorkbookExcel(string companyGroupId, string companyId, string PlantId)
        {
            try
            {
                #region Variable
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataSet dsCmp = null;
                var objRpt = new clsReport();

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                DataTable dtManPBSummary = GetLCPendingReportData();

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 5;

                #region ColumnHeaderVariables              
                int cDD = 0; int cDT = 0; int cC = 0; int cMO; int cSO; int cBF = 0; int cA = 0; int cPC = 0; int cP = 0; int cOR = 0; int cPM = 0; int cPT = 0; int cLC = 0; int cR = 0;
                #endregion
                #region ColumnHeaders
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Delivery Days", 14, ExcelHAlign.HAlignCenter); cDD = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DeliveryDate", 14, ExcelHAlign.HAlignCenter); cDT = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Customer", 20, ExcelHAlign.HAlignCenter); cC = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "MasterOrderNo", 20, ExcelHAlign.HAlignCenter); cMO = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SONo", 25, ExcelHAlign.HAlignCenter); cSO = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Buyer Ref#", 8, ExcelHAlign.HAlignCenter); cBF = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Article", 12, ExcelHAlign.HAlignCenter); cA = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Product Code", 14, ExcelHAlign.HAlignCenter); cPC = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Product", 14, ExcelHAlign.HAlignCenter); cP = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Own Ref#", 12, ExcelHAlign.HAlignCenter); cOR = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Payment Mode", 14, ExcelHAlign.HAlignCenter); cPM = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Payment Term", 14, ExcelHAlign.HAlignCenter); cPT = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LC Status", 14, ExcelHAlign.HAlignCenter); cLC = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarks", 14, ExcelHAlign.HAlignCenter); cR = xlsCol;


                var orgCollist = xlsCol;
                xlsRow++;
                endXlsCol = xlsCol;

                #endregion

                if (dtManPBSummary.Rows.Count > 0)
                {

                    DataRow dr = dtManPBSummary.NewRow();
                    dtManPBSummary.Rows.Add(dr);
                    for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                    {
                        oRU.SetTextBorder(ref sheet1, xlsRow, cDD, dtManPBSummary.Rows[i]["DeliveryDays"].ToString());
                        sheet1.Range[xlsRow, cDT].Text = dtManPBSummary.Rows[i]["DeliveryDate"].ToString();
                        sheet1.Range[xlsRow, cC].Text = dtManPBSummary.Rows[i]["Customer"].ToString();
                        sheet1.Range[xlsRow, cC].ColumnWidth = 40;
                        sheet1.Range[xlsRow, cDD].Text = dtManPBSummary.Rows[i]["DeliveryDays"].ToString();
                        sheet1.Range[xlsRow, cMO].Text = dtManPBSummary.Rows[i]["MasterOrderId"].ToString();
                        sheet1.Range[xlsRow, cMO].ColumnWidth = 13;
                        sheet1.Range[xlsRow, cSO].Text = dtManPBSummary.Rows[i]["SalesOrderId"].ToString();
                        sheet1.Range[xlsRow, cSO].ColumnWidth = 10;
                        sheet1.Range[xlsRow, cBF].Text = dtManPBSummary.Rows[i]["BuyerReferenceNo"].ToString();
                        sheet1.Range[xlsRow, cBF].ColumnWidth = 17;
                        sheet1.Range[xlsRow, cA].Text = dtManPBSummary.Rows[i]["Article"].ToString();
                        sheet1.Range[xlsRow, cA].ColumnWidth = 50;
                        sheet1.Range[xlsRow, cPC].Text = dtManPBSummary.Rows[i]["ProductCode"].ToString();
                        sheet1.Range[xlsRow, cPC].ColumnWidth = 10;
                        sheet1.Range[xlsRow, cP].Text = dtManPBSummary.Rows[i]["Product"].ToString();
                        sheet1.Range[xlsRow, cP].ColumnWidth = 30;
                        sheet1.Range[xlsRow, cOR].Text = dtManPBSummary.Rows[i]["OwnReferenceNo"].ToString();
                        sheet1.Range[xlsRow, cPM].Text = dtManPBSummary.Rows[i]["PaymentMode"].ToString();
                        sheet1.Range[xlsRow, cPM].ColumnWidth = 10;
                        sheet1.Range[xlsRow, cPT].Text = dtManPBSummary.Rows[i]["PaymentTerm"].ToString();
                        sheet1.Range[xlsRow, cPT].ColumnWidth = 25;
                        sheet1.Range[xlsRow, cLC].Text = dtManPBSummary.Rows[i]["LCStatus"].ToString();
                        sheet1.Range[xlsRow, cLC].ColumnWidth = 10;
                        sheet1.Range[xlsRow, cR].Text = dtManPBSummary.Rows[i]["Remarks"].ToString();
                        sheet1.Range[xlsRow, cR].ColumnWidth = 20;
                        xlsRow++;
                    }
                    sheet1.AutoFilters.FilterRange = sheet1.Range[cDD, 1, xlsRow, endXlsCol];


                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = false;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion


                    objRpt.SelectedPlantWiseCompany(identity.PlantId, "", out dsCmp);
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    var FactoryAddress = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 1].Text = FactoryName;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 20;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, 1, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 1].RowHeight = 30;

                    #region Plant Address


                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }

                    #endregion
                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "LC Pending Report";
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 15;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 24;


                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet1.UsedRange["A6"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                }



                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable GetLCPendingReportData()
        {
            try
            {
                string sql = "";

                sql = @"Select DeliveryDays=DATEDIFF(day, GetDate(), SO.DeliveryDate),FORMAT(SO.DeliveryDate,'dd-MMM-yyyy')DeliveryDate
,P.UserName Customer,MO.Id MasterOrderId,SO.Id SalesOrderId,MO.BuyerReferenceNo,A.StandardName Article,PL.Code ProductCode,PL.UserName Product
,MO.OwnReferenceNo,PT.PaymentMode,PT.UserName PaymentTerm,LCStatus =CASE WHEN LC.IsClose=1 THEN 'Close' ELSE '' END,MO.Remarks
From TRN.SalesOrder SO
LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
LEFT JOIN TRN.MasterOrder MO ON MO.Id=MOI.MasterOrderId
LEFT JOIN HKP.Party P ON P.Id=MO.PartyId
LEFT JOIN MST.MaterialMasterArticle A ON A.Id=MOI.ArticleId
LEFT JOIN [dbo].[ProductLibrary] PL ON PL.Id=MOI.ProductLibraryId
LEFT JOIN MST.PaymentTerm PT ON PT.Id=MO.PaymentTermId
LEFT JOIN dbo.Contract C ON C.Id=SO.ContractId
LEFT JOIN dbo.MasterLC LC ON LC.Id=C.MasterLCId
Where SO.OrderStatusId NOT IN('Closed','Cancelled')
Order By SO.DeliveryDate";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IWorkbook GetContractSummaryReportXlx(string ContractId)
        {
            ExcelEngine excelEngine = new ExcelEngine(); 
            IApplication application = excelEngine.Excel; 
            application.DefaultVersion = ExcelVersion.Excel2013; 
            IWorkbook workbook = application.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];

            try
            {
                DataTable dtOrderMaster = ContractSummaryHeaderSQL(ContractId);

                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                DataTable dataDetails = ContractSummaryDetailSQL(ContractId);
                 
                worksheet.Name = "ContractSummaryReport";

                int ROW = 5; int COL = 1;
                int MasterOrderDetailsStartRow = ROW;
                worksheet[ROW, COL].Text = "Customer";
                worksheet[ROW, COL].Text = dtOrderMaster.Rows[0]["Customer"].ToString();
                worksheet[ROW, COL].CellStyle.Font.Bold = true;

                COL = COL+8; 
                worksheet[ROW, COL].Text = "Date: ";
                worksheet[ROW, COL+1].Text = dtOrderMaster.Rows[0]["ContractDate"].ToString();
                worksheet[ROW, COL+1].CellStyle.Font.Bold = true;
                ROW++;

                //COL = COL + 8;
                //worksheet[ROW, COL].Text = "Date";
                COL = 1;
                worksheet[ROW, COL].Text = dtOrderMaster.Rows[0]["ContractNo"].ToString();
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                ROW++;

                int leftColumnCaption = COL;
                int leftColumnValue = leftColumnCaption + 1;

                //int MiddleColumnCaption = leftColumnValue + 2;
                int MiddleColumnCaption = leftColumnValue + 1;
                int MiddleColumnValue = MiddleColumnCaption + 1;

                int RightColumnCaption = MiddleColumnValue + 1;
                int RightColumnValue = RightColumnCaption + 1;

                //Contract Summary header.............................................................
                worksheet[ROW, COL].Text = "PO#";
                worksheet[ROW, COL].ColumnWidth = 10;
                int ColPO = COL;
                COL++;

                worksheet[ROW, COL].Text = "Style No";
                worksheet[ROW, COL].ColumnWidth = 10;
                int ColStyleNo = COL;
                COL++;

                worksheet[ROW, COL].Text = "Order Quantity";
                worksheet[ROW, COL].ColumnWidth = 12;
                int ColOrderQuantity = COL;
                COL++;

                worksheet[ROW, COL].Text = "Shipped Quantity";
                worksheet[ROW, COL].ColumnWidth = 12;
                int ColShippedQuantity = COL;
                COL++;

                worksheet[ROW, COL].Text = "Description";
                worksheet[ROW, COL].ColumnWidth = 12;
                int ColDescription = COL;
                COL++;

                worksheet[ROW, COL].Text = "Unit Price";
                worksheet[ROW, COL].ColumnWidth = 20;
                int ColUnitPrice = COL;
                COL++;

                worksheet[ROW, COL].Text = "Amount";
                worksheet[ROW, COL].ColumnWidth = 20;
                int ColAmount = COL;
                COL++;

                worksheet[ROW, COL].Text = "Export Values";
                worksheet[ROW, COL].ColumnWidth = 20;
                int ColExportValues = COL;
                COL++;

                worksheet[ROW, COL].Text = "Ship Date";
                worksheet[ROW, COL].ColumnWidth = 20;
                int ColShipDate = COL;
                COL++;

                worksheet[ROW, COL].Text = "Remarks";
                worksheet[ROW, COL].ColumnWidth = 20;
                int ColRemarks = COL;

                int endCols = COL;
                worksheet.Range[ROW, 1, ROW, endCols].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                worksheet.Range[ROW, 1, ROW, endCols].CellStyle.Font.Color = ExcelKnownColors.White;
                worksheet.Range[ROW, 1, ROW, endCols].CellStyle.Font.Bold = true;
                worksheet.Range[ROW, 1, ROW, endCols].CellStyle.Font.Size = 9f;
                worksheet.Range[ROW, 1, ROW, endCols].BorderInside(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCols].BorderAround(ExcelLineStyle.Hair);
                ROW++;

                //worksheet.Range[MasterOrderDetailsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Blue_grey;
                int CostingComponentStartRow = ROW;

                for (int i = 0; i < dtOrderMaster.Rows.Count; i++)
                {
                    worksheet[ROW, ColPO].Text = dtOrderMaster.Rows[i]["PO"].ToString();
                    worksheet[ROW, ColStyleNo].Text = dtOrderMaster.Rows[i]["StyleNo"].ToString();
                    worksheet[ROW, ColOrderQuantity].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["OrderQty"].ToString());
                    worksheet[ROW, ColOrderQuantity].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    worksheet[ROW, ColShippedQuantity].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["ShippedQty"].ToString());
                    worksheet[ROW, ColShippedQuantity].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    worksheet[ROW, ColDescription].Text = dtOrderMaster.Rows[i]["Description"].ToString();
                    worksheet[ROW, ColUnitPrice].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["UnitPrice"].ToString());
                    worksheet[ROW, ColUnitPrice].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    worksheet[ROW, ColAmount].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["Amount"].ToString());
                    worksheet[ROW, ColAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    worksheet[ROW, ColExportValues].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["ExportValue"].ToString());
                    worksheet[ROW, ColExportValues].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    worksheet[ROW, ColShipDate].Text = dtOrderMaster.Rows[i]["ShipDate"].ToString();
                    worksheet[ROW, ColRemarks].Text = "";
                
                    worksheet.Range[ROW, 1, ROW, endCols].BorderAround(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCols].BorderInside(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCols].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                int CostingComponentEndRow = ROW - 1;
                ReportUtility reportUtility = new ReportUtility();

                worksheet[ROW, 1].Text = "Total:";
                worksheet.Range[ROW, 1].CellStyle.Font.Bold = true;

                worksheet.Range[ROW, ColPO, ROW, ColStyleNo].Merge();
                worksheet.Range[ROW, ColOrderQuantity, ROW, ColOrderQuantity].Formula = "SUM(" + reportUtility.GetColumnNameForXls(ColOrderQuantity) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(ColOrderQuantity) + CostingComponentEndRow + ")";
                worksheet.Range[ROW, ColOrderQuantity].CellStyle.Font.Bold = true;

                worksheet.Range[ROW, ColShippedQuantity, ROW, ColShippedQuantity].Formula = "SUM(" + reportUtility.GetColumnNameForXls(ColShippedQuantity) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(ColShippedQuantity) + CostingComponentEndRow + ")";
                worksheet.Range[ROW, ColShippedQuantity].CellStyle.Font.Bold = true;

                worksheet.Range[ROW, ColAmount, ROW, ColAmount].Formula = "SUM(" + reportUtility.GetColumnNameForXls(ColAmount) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(ColAmount) + CostingComponentEndRow + ")";
                worksheet.Range[ROW, ColAmount].CellStyle.Font.Bold = true;
                double totalAmount= clsStaticInfo.dbl(dtOrderMaster.Compute("SUM(Amount)","")); 

                worksheet.Range[ROW, ColExportValues, ROW, ColExportValues].Formula = "SUM(" + reportUtility.GetColumnNameForXls(ColExportValues) + CostingComponentStartRow + ":" + reportUtility.GetColumnNameForXls(ColExportValues) + CostingComponentEndRow + ")";
                worksheet.Range[ROW, ColExportValues].CellStyle.Font.Bold = true;

                worksheet.Range[CostingComponentStartRow, 1, CostingComponentEndRow + 1, endCols].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet.Range[ROW, 1, ROW, endCols].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCols].BorderInside(ExcelLineStyle.Hair);
                
                ROW += 2;
                COL = 1;
                #region columns
                worksheet[ROW, COL].Text = "Item";
                worksheet[ROW, COL].ColumnWidth = 10;
                int ColItem = COL;
                COL++;

                worksheet[ROW, COL].Text = "Supplier";
                worksheet[ROW, COL].ColumnWidth = 10;
                int ColSupplier = COL;
                COL++;

                worksheet[ROW, COL].Text = "LC Status";
                worksheet[ROW, COL].ColumnWidth = 12;
                int ColLCStatus = COL;
                COL++;

                worksheet[ROW, COL].Text = "LC Date";
                worksheet[ROW, COL].ColumnWidth = 12;
                int ColLCDate = COL;
                COL++;

                worksheet[ROW, COL].Text = "TTL LC Value";
                worksheet[ROW, COL].ColumnWidth = 12;
                int ColTTLLCValue = COL;
                COL++;

                worksheet[ROW, COL].Text = "Budget Value $";
                worksheet[ROW, COL].ColumnWidth = 20;
                int ColBudgetValue = COL;
                COL++;

                worksheet[ROW, COL].Text = "Actual Cost";
                worksheet[ROW, COL].ColumnWidth = 12;
                int ColActualCost = COL;
                COL++;

                worksheet[ROW, COL].Text = "Short / Excess";
                worksheet[ROW, COL].ColumnWidth = 15;
                int ColShortExcess = COL;
                COL++;

                worksheet[ROW, COL].Text = "BTB Pending";
                worksheet[ROW, COL].ColumnWidth = 15;
                int ColBTBPending = COL;
                COL++;
                 
                worksheet[ROW, COL].Text = "Percentage";
                worksheet[ROW, COL].ColumnWidth = 15;
                int ColPercentage = COL;

                #endregion columns
                int endCol = COL;
                worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dataDetails.Rows.Count; i++)
                {
                    worksheet[ROW, ColItem].Text = dataDetails.Rows[i]["Item"].ToString();
                    worksheet[ROW, ColSupplier].Text = dataDetails.Rows[i]["Supplier"].ToString();
                    worksheet[ROW, ColLCStatus].Text = dataDetails.Rows[i]["LCStatus"].ToString();
                    worksheet[ROW, ColLCDate].Text = dataDetails.Rows[i]["LCDate"].ToString();
                    worksheet[ROW, ColTTLLCValue].Number = clsStaticInfo.dbl(dataDetails.Rows[i]["TotalLCValue"].ToString());
                    worksheet[ROW, ColTTLLCValue].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    worksheet[ROW, ColBudgetValue].Number = clsStaticInfo.dbl(dataDetails.Rows[i]["BudgetValue"].ToString());
                    worksheet[ROW, ColBudgetValue].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    worksheet[ROW, ColActualCost].Number = clsStaticInfo.dbl(dataDetails.Rows[i]["ActualCost"].ToString());
                    worksheet[ROW, ColActualCost].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    worksheet[ROW, ColShortExcess].Number = clsStaticInfo.dbl(dataDetails.Rows[i]["ShortExcess"].ToString());
                    worksheet[ROW, ColShortExcess].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    worksheet[ROW, ColBTBPending].Number = clsStaticInfo.dbl(dataDetails.Rows[i]["BTBPending"].ToString());
                    worksheet[ROW, ColBTBPending].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    double totalPer = clsStaticInfo.dbl(dataDetails.Rows[i]["BudgetValue"].ToString()) / totalAmount * 100;
                    worksheet[ROW, ColPercentage].Number = totalPer;
                    worksheet[ROW, ColPercentage].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                    worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.CellStyle.Font.Size = 8f; 

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility.PlantHeader(ref worksheet, endCol, "Contract Summary" /*+ dtOrderMaster.Rows[0]["ContractNo"].ToString()*/, identity.PlantId);
                // reportUtility.PlantHeader(ref worksheet, endCol, "Contract NO#" + ContractId, identity.PlantId);
                reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;
                return workbook; 
            }
            catch (Exception ex)
            {
                throw (ex); 
            } 
        }

        public DataTable ContractSummaryHeaderSQL(string ContractId)
        {
            string strSQL;
            try
            {
                strSQL = @"select P.UserName Customer,ContractNo= c.ContractNo+' '+ISNULL(format(c.ContractDate,'dd-MMM-yyyy'),'')
                    ,PO=isnull(STUFF((select distinct ','+XMOI.PONumber from 
                    TRN.CustomerPO XMOI 
                    where so.CustomerPOId=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
                    ,so.CustomerPOId
                    ,moi.BuyerReferenceNo StyleNo,sm.TransactionQty ShippedQty,so.Qty OrderQty,so.Description,so.Rate UnitPrice
                    ,Amount= so.Qty*so.Rate,sm.NetAmount ExportValue,format(so.DeliveryDate,'dd-MMM-yyyy') ShipDate,format(c.ContractDate,'dd-MMM-yyyy')ContractDate
                    from Contract c
                    left join trn.SalesOrder so on so.ContractId=c.Id
                    left join trn.MasterOrderItem moi on moi.Id=so.MasterOrderItemId
                    left join trn.SalesMaterial sm on sm.SalesOrderId=so.Id
                    left join hkp.Party P on P.Id=c.CustomerId
                    where c.Id='" + ContractId + "'";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {}
        }
        public DataTable ContractSummaryDetailSQL(string ContractId)
        {
            string strSQL;
            try
            {
                strSQL = @"select x.Item,x.Supplier,x.LCStatus,x.LCDate,sum(x.TotalLCValue) TotalLCValue,sum(x.BudgetValue)BudgetValue,x.ActualCost,x.ShortExcess,x.BTBPending from
                (select ci.UserName Item,P.UserName Supplier
                ,LCStatus=isnull(STUFF((select distinct ','+XMOI.LCRef from 
                PurchaseLC XMOI 
                where c.Id=XMOI.ContractId and XMOI.VendorId=opc.VendorId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
                ,LCDate=STUFF((SELECT DISTINCT ','+FORMAT(XSO.LCDate,'dd-MMM-yyyy') from 
	                                                                    PurchaseLC XSO 
			                                                                WHERE c.Id=XSO.ContractId and XSO.VendorId=opc.VendorId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                ,TotalLCValue=(select Sum(XMOI.Amount) from 
                PurchaseLC XMOI 
                where c.Id=XMOI.ContractId and XMOI.VendorId=opc.VendorId
                group by XMOI.ContractId)										                  	
                ,BudgetValue=ISNULL(opc.GrossAmount,0)*sum(moi.TotalQty),0 as ActualCost
                ,ShortExcess=ISNULL(opc.GrossAmount,0)*sum(moi.TotalQty)- 0
                ,0 as BTBPending
                 from Contract c
                    left join trn.SalesOrder so on so.ContractId=c.Id
                    left join trn.MasterOrderItem moi on moi.Id=so.MasterOrderItemId
					left join dbo.OrderProcurementCostingDirectMaterial opc on opc.OrderCostingMasterTemplateId=moi.OrderCostingMasterTemplateId
					left join HKP.CostingItem ci on ci.Id=opc.CostingItemId
					left join HKP.party p on p.Id=opc.VendorId
                    where c.Id='" + ContractId +@"'
                    group by ci.UserName,P.UserName,c.Id,opc.VendorId,opc.GrossAmount
					)x
                    group by x.Item,x.Supplier,x.LCStatus,x.LCDate,x.ActualCost,x.ShortExcess,x.BTBPending
                    order by x.Item";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            { }
        }

    }
}


