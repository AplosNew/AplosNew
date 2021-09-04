using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.Accounts
{
    public class AccountsOutsourceBillingService
	{
        private readonly ISqlRepository _sqlRepository;
        public AccountsOutsourceBillingService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
		public IEnumerable<object> GetOutsourceBillingJV(string companyId, string plantId, string billingId)
		{
			try
			{
				var sql = @"DECLARE @billingId varchar(10)= '"+ billingId + "',@plantId varchar(10)='" + plantId + @"'

						SELECT  'JobWork' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =SVGL.ServiceGLId
							,GLGeneralInfoCode =GLF.AccountCode
							,GLGeneralInfoName =GLF.UserName
							,BudgetMasterId =SVGL.ServiceBudgetMasterId
							,BudgetCode =BF.Code
							,BudgetName =BF.UserName
							,ActivityId =SVGL.ServiceActivityId
							,ActivityCode =AF.Code 
							,ActivityName = AF.UserName 
							, SUM(IRD.TransactionQty*JWTCC.RatePerUnit) AS Dr, NULL Cr
							, SUM(IRD.TransactionQty*JWTCC.RatePerUnit) AS Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [MST].[JobWorkTransformationMaster] JWTM ON JWTM.Id=IRD.JWTCMId
						LEFT JOIN [dbo].[JWReceiveBilling] JRB ON JRB.JWTransformationPurchaseOrderId=IRD.JWTCMId
						LEFT JOIN dbo.JobWorkTransformationContractChild JWTCC ON JWTCC.Id=IRD.JWTCMDId
						LEFT JOIN HKP.ServiceMaster SM ON SM.Id=JWTCC.ServiceId
						LEFT JOIN HKP.ServiceGroup SVG ON SVG.Id=SM.ServiceGroupId
						LEFT JOIN HKP.ServiceGroupGL SVGL ON SVGL.ServiceGroupId=SVG.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON SVGL.ServiceGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON SVGL.ServiceBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON SVGL.ServiceActivityId= AF.Id
						WHERE JRB.Id=@billingId and IRD.MaterialFor='JWOUTPUTMaterial' AND IR.PlantId=@plantId
						GROUP BY SVGL.ServiceGLId,SVGL.ServiceBudgetMasterId,SVGL.ServiceActivityId,GLF.AccountCode,GLF.UserName
						,BF.Code,BF.UserName
						,AF.Code,AF.UserName

						UNION

						SELECT  'GRIR' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =SVGL.ClearingAccountGLId
							,GLGeneralInfoCode =GLF.AccountCode
							,GLGeneralInfoName =GLF.UserName
							,BudgetMasterId =SVGL.ClearingAccountBudgetMasterId
							,BudgetCode =BF.Code
							,BudgetName =BF.UserName
							,ActivityId =SVGL.ClearingAccountActivityId
							,ActivityCode =AF.Code 
							,ActivityName = AF.UserName 
							, NULL Dr, SUM(IRD.TransactionQty*JWTCC.RatePerUnit) AS Cr
							, SUM(IRD.TransactionQty*JWTCC.RatePerUnit) AS Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [MST].[JobWorkTransformationMaster] JWTM ON JWTM.Id=IRD.JWTCMId
						LEFT JOIN [dbo].[JWReceiveBilling] JRB ON JRB.JWTransformationPurchaseOrderId=IRD.JWTCMId
						LEFT JOIN dbo.JobWorkTransformationContractChild JWTCC ON JWTCC.Id=IRD.JWTCMDId
						LEFT JOIN HKP.ServiceMaster SM ON SM.Id=JWTCC.ServiceId
						LEFT JOIN HKP.ServiceGroup SVG ON SVG.Id=SM.ServiceGroupId
						LEFT JOIN HKP.ServiceGroupGL SVGL ON SVGL.ServiceGroupId=SVG.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON SVGL.ClearingAccountGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON SVGL.ClearingAccountBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON SVGL.ClearingAccountActivityId= AF.Id
						WHERE JRB.Id=@billingId and IRD.MaterialFor='JWOUTPUTMaterial' AND IR.PlantId=@plantId
						GROUP BY SVGL.ClearingAccountGLId,SVGL.ClearingAccountBudgetMasterId,SVGL.ClearingAccountActivityId,GLF.AccountCode,GLF.UserName
						,BF.Code,BF.UserName
						,AF.Code,AF.UserName";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

	}
}
