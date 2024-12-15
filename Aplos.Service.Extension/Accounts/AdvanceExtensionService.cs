using Library.Core;
using Library.Data.Sql;
using Library.Model.Enums;
using System;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Extension.Accounts
{
    public class AdvanceExtensionService
    {
        SqlRepository _sqlRepository;
        public AdvanceExtensionService()
        {
            _sqlRepository = new SqlRepository();
        }

        //SqlRepository _sqlRepository;
        //public BankExtensionService()
        //{
        //    _sqlRepository = new SqlRepository();
        //}


        #region vendor advance
        public Dictionary<string, object> GetCustomerAdvanceHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , P.UserName AS Customer, PP.UserName AS CustomerPlant, BJ.CurrencyId, C.Code AS CurrencyCode
							--, V.AddedBy, V.PostedBy
							
							,CLD1.Id CheckLotDetailId, CLD1.CheckNumber, CLH.CheckDate
							--, CL.LotNumber,p.UserName Party
                            ,BJ.POId PONo
							, AddedBy =case when u.FullName<>'' then u.FullName else v.AddedBy end
						   ,PostedBy = case when up.FullName<>'' then up.FullName else v.PostedBy end
                           ,E.UserName EntityName
                            FROM [TRN].[Advance] AS BJ
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=BJ.PartyId
							LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId

						    LEFT JOIN TRN.VoucherDetail VD ON VD.VoucherId=V.Id AND VD.BankMasterId<>''
						    left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							--left join HKP.Party p on p.Id=vdd.PartyId

						--	left join trn.CheckLotDetail CLD ON CLD.CheckLotId=CL.Id
						    left join trn.CheckLotDetailHistory CLH on VD.Id= CLH.VoucherDetailId
							LEFT JOIN TRN.CheckLotDetail CLD1 ON CLD1.Id=CLH.CheckLotDetailId
							left join [sec].[User] U on U.UserId=v.AddedBy
							left join sec.[User] up on up.UserId=v.PostedBy
                            LEFT JOIN [ORG].[Entity] E on E.Id=BJ.EntityId
                            WHERE BJ.Archive=0 AND BJ.CompanyGroupId='" + companyGroupId + "' AND BJ.CompanyId='" + companyId + "' AND BJ.PlantId='" + plantId + "' AND BJ.VoucherId='" + voucherId + "' AND BJ.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }

        public DataTable GetCustomerAdvanceDetailData(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget,BM.AccountTitle as AccountTitleName
                            , ACT.UserName AS Activity, CM.UserName AS CashMasterName
                             , ActivityName=case when vd.BankMasterId<>'' then BM.AccountTitle when vd.CashMasterId<>'' then CM.UserName else ACT.UserName end
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                             LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            LEFT JOIN [TRN].[AdvanceDetail] AS IVD ON IVD.Id=VD.AdvanceDetailId
                            LEFT JOIN [TRN].[Advance] AS IV ON IV.VoucherId=V.Id
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion vendor advance

    }
}
