using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.Accounts
{
    public class CheckQueryService
    {
        private readonly ISqlRepository _sqlRepository;
        public CheckQueryService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }


		public IEnumerable<object> GetUNApprovalList(string plantId, string POTypeApprovalStatus)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var Sql = "";
			try
			{
					Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
					SELECT * FROM(SELECT ROW_NUMBER() OVER (ORDER BY  IR.Id) AS SiNo,'PO' SourceType ,'' VoucherId, IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106), ' ', '-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
											, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
											, IR.CurrencyId, CU.Code AS CurrencyCode
											, IRD.TransactionQty,IRD.UoM TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount,IR.ToCurrencyRate
											, IR.AddedBy, PLC.LCANo PurchaseLC,Ctc.ContractNo ContructNumber,Par.UserName Customer
											, eI.EmployeeName CheckedBy , IR.CheckedByStatus AS CheckedByStatus,IR.AuthorizedByStatus AS AuthorizedByStatus
											, eI1.EmployeeName AuthorizedBy
									FROM[TRN].[PurchaseOrder] AS IR 
									left JOIN[HKP].[Party] AS P ON IR.PartyId = P.Id
									LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						            LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						            LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                                    LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId = IR.CheckedBy
									LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId = IR.AuthorizedBy
									left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
									LEFT JOIN[HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
									LEFT JOIN [ORG].Plant PL ON PL.Id= IR.PlantId
									LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount 
									,UoM.UserName UoM
									FROM [TRN].[PurchaseOrderDetail] AS A
												  LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON UoM.Id=A.TransactionUoMId
												  GROUP BY A.InventoryReceiveId,UoM.UserName) AS IRD ON IRD.InventoryReceiveId=IR.Id
									 WHERE IR.AuthorizedBy ='" + identity.EmployeeId+ @"'
									 AND IR.CheckedByStatus = 'Checked'
									 AND IR.AuthorizedByStatus = 'For Approval'

									 UNION ALL
									 SELECT ROW_NUMBER() OVER (ORDER BY  IR.Id) AS SiNo,'PO' SourceType ,'' VoucherId, IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106), ' ', '-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
											, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
											, IR.CurrencyId, CU.Code AS CurrencyCode
											, IRD.TransactionQty,IRD.UoM TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount,IR.ToCurrencyRate
											, IR.AddedBy, PLC.LCANo PurchaseLC,Ctc.ContractNo ContructNumber,Par.UserName Customer
											, eI.EmployeeName CheckedBy , IR.CheckedByStatus AS CheckedByStatus,IR.AuthorizedByStatus AS AuthorizedByStatus
											, eI1.EmployeeName AuthorizedBy
									FROM[TRN].[PurchaseOrder] AS IR 
									left JOIN[HKP].[Party] AS P ON IR.PartyId = P.Id
									LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						            LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						            LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                                    LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId = IR.CheckedBy
									LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId = IR.AuthorizedBy
									left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
									LEFT JOIN[HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
									LEFT JOIN [ORG].Plant PL ON PL.Id= IR.PlantId
									LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount 
									,UoM.UserName UoM
									FROM [TRN].[PurchaseOrderDetail] AS A
												  LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON UoM.Id=A.TransactionUoMId
												  GROUP BY A.InventoryReceiveId,UoM.UserName) AS IRD ON IRD.InventoryReceiveId=IR.Id
									 WHERE IR.AuthorizedBy ='" + identity.EmployeeId + @"'
									  AND IR.CheckedByStatus Is Null 
									AND IR.AuthorizedByStatus = 'For Approval'

									UNION ALL
									SELECT ROW_NUMBER() OVER (ORDER BY  V.Id) AS SiNo,V.SourceType , V.Id VoucherId,V.VoucherNo Id,  REPLACE(CONVERT(CHAR(11), V.DocDate, 106), ' ', '-') PODate, V.CompanyGroupId, V.CompanyId, V.PlantId, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
											, V.DocRefNo, REPLACE(CONVERT(CHAR(11), V.DocDate, 106), ' ', '-') AS DocDate
											, V.CurrencyId, CU.Code AS CurrencyCode
											, 0 TransactionQty,NULL TransactionUoM,VD.DrAmount TransactionAmount,ROUND(VD.DrAmount, 2) BaseAmount,I.CompanyCurrencyRate ToCurrencyRate
											, V.AddedBy, NULL PurchaseLC,NULL ContructNumber,NULL Customer
											, NULL CheckedBy , NULL CheckedByStatus,V.ApprovedByStatus AuthorizedByStatus
											, eI1.EmployeeName AuthorizedBy
									FROM[TRN].[Voucher] AS V 
									LEFT JOIN [TRN].[Invoice] I ON I.VoucherId=V.Id
									LEFT JOIN[HKP].[Party] AS P ON I.PartyId = P.Id
									LEFT JOIN[SCS].[Currency] AS CU ON V.CurrencyId = CU.Id
									LEFT JOIN (SELECT VoucherId,Sum(DrAmount) DrAmount,Sum(CrAmount) CrAmount FROM  [TRN].[VoucherDetail] GROUP BY VoucherId) VD ON VD.VoucherId=V.Id
									LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId = V.ApprovedById
									WHERE V.ApprovedById ='" + identity.EmployeeId + @"' AND V.ApprovedByStatus = 'ToBeApproved' AND V.IsPark=1
									  )x
									  --Order by PODate ASC
";
				
				return _sqlRepository.GetDataCollection(Sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetApprovedList()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var Sql = "";
			try
			{
				Sql = @" SELECT * FROM(
									 SELECT ROW_NUMBER() OVER (ORDER BY  IR.Id) AS SiNo,'PO' SourceType ,'' VoucherId, IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106), ' ', '-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
											, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
											, IR.CurrencyId, CU.Code AS CurrencyCode
											, IRD.TransactionQty,IRD.UoM TransactionUoM, IRD.TransactionAmount,ROUND(IRD.BaseAmount, 2) BaseAmount,IR.ToCurrencyRate
											, IR.AddedBy, PLC.LCANo PurchaseLC,Ctc.ContractNo ContructNumber,Par.UserName Customer
											, eI.EmployeeName CheckedBy , IR.CheckedByStatus AS CheckedByStatus,IR.AuthorizedByStatus AS AuthorizedByStatus
											, eI1.EmployeeName AuthorizedBy
									FROM[TRN].[PurchaseOrder] AS IR 
									left JOIN[HKP].[Party] AS P ON IR.PartyId = P.Id
									LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=IR.PurchaseLCId 
						            LEFT JOIN [dbo].[Contract] Ctc ON Ctc.Id = PLC.ContractId
						            LEFT JOIN [HKP].[Party] Par ON Par.Id= Ctc.CustomerId 
                                    LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId = IR.CheckedBy
									LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId = IR.AuthorizedBy
									left JOIN[SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
									LEFT JOIN[HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
									LEFT JOIN [ORG].Plant PL ON PL.Id= IR.PlantId
									LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount 
									,UoM.UserName UoM
									FROM [TRN].[PurchaseOrderDetail] AS A
												  LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON UoM.Id=A.TransactionUoMId
												  GROUP BY A.InventoryReceiveId,UoM.UserName) AS IRD ON IRD.InventoryReceiveId=IR.Id
									 WHERE IR.AuthorizedBy ='" + identity.EmployeeId + @"'
									AND IR.AuthorizedByStatus = 'Approved'

									UNION ALL
									SELECT ROW_NUMBER() OVER (ORDER BY  V.Id) AS SiNo,V.SourceType , V.Id VoucherId,V.VoucherNo Id,  REPLACE(CONVERT(CHAR(11), V.DocDate, 106), ' ', '-') PODate, V.CompanyGroupId, V.CompanyId, V.PlantId, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
											, V.DocRefNo, REPLACE(CONVERT(CHAR(11), V.DocDate, 106), ' ', '-') AS DocDate
											, V.CurrencyId, CU.Code AS CurrencyCode
											, 0 TransactionQty,NULL TransactionUoM,VD.DrAmount TransactionAmount,ROUND(VD.DrAmount, 2) BaseAmount,I.CompanyCurrencyRate ToCurrencyRate
											, V.AddedBy, NULL PurchaseLC,NULL ContructNumber,NULL Customer
											, NULL CheckedBy , NULL CheckedByStatus,V.ApprovedByStatus AuthorizedByStatus
											, eI1.EmployeeName AuthorizedBy
									FROM[TRN].[Voucher] AS V 
									LEFT JOIN [TRN].[Invoice] I ON I.VoucherId=V.Id
									LEFT JOIN[HKP].[Party] AS P ON I.PartyId = P.Id
									LEFT JOIN[SCS].[Currency] AS CU ON V.CurrencyId = CU.Id
									LEFT JOIN (SELECT VoucherId,Sum(DrAmount) DrAmount,Sum(CrAmount) CrAmount FROM  [TRN].[VoucherDetail] GROUP BY VoucherId) VD ON VD.VoucherId=V.Id
									LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId = V.ApprovedById
									WHERE V.ApprovedById ='" + identity.EmployeeId + @"' AND V.ApprovedByStatus = 'Approved'
									  )x
									  --Order by PODate ASC
";

				return _sqlRepository.GetDataCollection(Sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public void UpdateApprovalStatus(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy, string ApproveRejectReason)
		{
			try
			{
				ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
				var Status = CheckedStataus;
				var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
				con.BeginTransaction();
				con.executeQuery("Update TRN.Voucher set ApprovedByStatus='" + Status + "',ApprovedDate='"+ AddedDate + "' where id='" + PoId + "'");
				
				con.CommitTransaction();
				//_sqlRepository.ExecuteSqlCommand(_sql1);
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
