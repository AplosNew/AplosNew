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
				var sql = @"DECLARE @billingId varchar(10)= '"+ billingId + "',@companyId varchar(10)='"+ companyId + "',@plantId varchar(10)='"+ plantId + @"'

						SELECT  'Vendor' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =MGPGL.GLGeneralInfoId
							,GLGeneralInfoCode =GL.AccountCode
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =MGPGL.BudgetMasterId
							,BudgetCode =B.Code
							,BudgetName =B.UserName
							,ActivityId =MGPGL.ActivityId
							,ActivityCode =A.Code 
							,ActivityName = A.UserName 
							, SUM(JRBD.Amount) AS Dr, NULL Cr
							, SUM(JRBD.Amount) AS Amount
						FROM   dbo.OSReceiveBillingDetail JRBD 
						LEFT JOIN [dbo].[OSReceiveBilling] JRB ON  JRBD.OSReceiveBillingId=JRB.Id
						LEFT JOIN dbo.OSTransformationPODetail JWTCC ON JWTCC.Id=JRBD.OSTransformationPODetailId
						LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.OSTransformationPODetailId=JWTCC.Id
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN HKP.ServiceMaster SM ON SM.Id=JWTCC.ServiceId
						LEFT JOIN HKP.ServiceGroup SVG ON SVG.Id=SM.ServiceGroupId
						LEFT JOIN HKP.ServiceGroupGL SVGL ON SVGL.ServiceGroupId=SVG.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[ServiceGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON SVG.Id = MGGL.ServiceGroupId
								LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[ServiceGroupPartyAccountGroupGL] AS MGPGL ON MGGL.ServiceGroupId = MGPGL.ServiceGroupId AND MGPGL.PartyAccountGroupId= PACG.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
						WHERE JRB.Id=@billingId and IRD.MaterialFor='JWOUTPUTMaterial' AND IR.PlantId=@plantId
						GROUP BY MGPGL.GLGeneralInfoId,MGPGL.BudgetMasterId,MGPGL.ActivityId,GL.AccountCode,GL.UserName
						,B.Code,B.UserName
						,A.Code,A.UserName

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
							, NULL Dr, SUM(JRBD.Amount) AS Cr
							, SUM(JRBD.Amount) AS Amount
						FROM   dbo.OSReceiveBillingDetail JRBD 
						LEFT JOIN [dbo].[OSReceiveBilling] JRB ON  JRBD.OSReceiveBillingId=JRB.Id
						LEFT JOIN dbo.OSTransformationPODetail JWTCC ON JWTCC.Id=JRBD.OSTransformationPODetailId
						LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.OSTransformationPODetailId=JWTCC.Id
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
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
