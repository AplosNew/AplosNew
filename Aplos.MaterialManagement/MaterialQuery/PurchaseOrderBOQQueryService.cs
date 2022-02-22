using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Parties;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Aplos.MaterialManagement.MaterialQuery
{
    public class PurchaseOrderBOQQueryService
    {
        private readonly ISqlRepository _sqlRepository;
        public PurchaseOrderBOQQueryService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        public List<Dictionary<string, object>> GetCompanyBOQPartyListNew(string companyGroupId, string companyId, string plantId, string column, string value, string customerVendor)
        {
            try
            {
                string temp = null;
                if (customerVendor == "Vendor" || customerVendor == "Customer")
                {
                    temp = customerVendor;
                }
                if (customerVendor == null)
                {
                    temp = "Vendor" + "','" + "Customer";
                }
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"select top 100 * from (SELECT  P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
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
                                    ) AS TEMP WHERE " + strkey + " order by Code ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

    }
}
