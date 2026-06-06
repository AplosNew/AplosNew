#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.OrderManagements;
using Library.Model.Parties;
using Library.Model.Productions;
using Library.Model.Taxations;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Systems;
using Library.ViewModel.OrderManagements;
using OTSBD;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.OfficeChartToImageConverter;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class MasterOrderService : Service<MasterOrder>, IMasterOrderService
    {
        #region Constructor

        private readonly IEntityService _entityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IRepositoryAsync<MasterOrderResPerson> _personRepository;
        private readonly IRepositoryAsync<UserRemarksControl> _UserRemarksControlRepository;
        private readonly IRepositoryAsync<MasterOrderItem> _itemRepository;
        private readonly IRepositoryAsync<MasterOrderAttributeValue> _itemAttributeValueRepository;
        // private readonly IRepositoryAsync<CustomerDivision> _customerDivisionRepository;
        private readonly IRepositoryAsync<SalesOrderMaster> _salesOrderRepository;
        private readonly IRepositoryAsync<SalesOrderTax> _salesOrderTaxRepository;
        private readonly IRepositoryAsync<CustomerPO> _customerPORepository;

        private readonly IRepositoryAsync<FirstCharacteristics> _firstCharacteristicsRepository;
        private readonly IRepositoryAsync<SecondCharacteristics> _secondCharacteristicsRepository;
        private readonly IRepositoryAsync<ThirdCharacteristics> _thirdCharacteristicsRepository;
        private readonly IRepositoryAsync<SOCostingConfirmation> _SOCostingConfirmationRepository;
        private readonly IRepositoryAsync<MasterOrderItemCostingRate> _MasterOrderItemCostingRateRepository;


        private readonly ISqlRepository _sqlRepository;

        public MasterOrderService(
            IRepositoryAsync<MasterOrder> baseRepository
            , IRepositoryAsync<MasterOrderResPerson> personRepository
            , IRepositoryAsync<UserRemarksControl> UserRemarksControlRepository
            , IRepositoryAsync<MasterOrderItem> itemRepository
            , IRepositoryAsync<MasterOrderAttributeValue> itemAttributeValueRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IEntityService entityService
            // , IRepositoryAsync<CustomerDivision> customerDivisionRepository
            , IRepositoryAsync<SalesOrderMaster> salesOrderRepository
            , IRepositoryAsync<SalesOrderTax> salesOrderTaxRepository
            , IRepositoryAsync<CustomerPO> customerPORepository
            , IRepositoryAsync<FirstCharacteristics> firstCharacteristicsRepository
            , IRepositoryAsync<SecondCharacteristics> secondCharacteristicsRepository
            , IRepositoryAsync<ThirdCharacteristics> thirdCharacteristicsRepository
            , IRepositoryAsync<SOCostingConfirmation> SOCostingConfirmationRepository
            , IRepositoryAsync<MasterOrderItemCostingRate> MasterOrderItemCostingRateRepository

            , IUnitOfWork unitOfWork) :
            base(baseRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
            _entityService = entityService;
            //  _customerDivisionRepository = customerDivisionRepository;
            _salesOrderRepository = salesOrderRepository;
            _salesOrderTaxRepository = salesOrderTaxRepository;
            _customerPORepository = customerPORepository;
            _personRepository = personRepository;
            _UserRemarksControlRepository = UserRemarksControlRepository;
            _itemRepository = itemRepository;
            _itemAttributeValueRepository = itemAttributeValueRepository;

            _firstCharacteristicsRepository = firstCharacteristicsRepository;
            _secondCharacteristicsRepository = secondCharacteristicsRepository;
            _thirdCharacteristicsRepository = thirdCharacteristicsRepository;
            _SOCostingConfirmationRepository = SOCostingConfirmationRepository;
            _MasterOrderItemCostingRateRepository = MasterOrderItemCostingRateRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetSpecialTaxList(string plantId)
        {
            try
            {
                var sql = @"SELECT * FROM HKP.SpecialTax WHERE CountryId=(SELECT AM.CountryId FROM ORG.Plant P LEFT JOIN MST.AddressMaster AM ON AM.Id=P.AddressMasterId WHERE P.Id='" + plantId + "')";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetTaskList(string buyerId, string buyerDepartmentId, string buyerDivisionId, string moId)
        {
            try
            {

                var sql = @"SELECT 
                                    tt.TaskMasterId,tt.TaskDescription,tt.IsMandatory
                                    ,CASE WHEN ISNULL(mot.Id,'')='' THEN 0 ELSE 1 END AS Active
                                     FROM mst.BuyerMaster AS  BM
                                    LEFT OUTER JOIN TaskTemplateMaster AS TTM ON TTM.Id=bm.TaskTemplateMasterId
                                    INNER JOIN TaskTemplate AS tt ON tt.TaskTemplateMasterId=bm.TaskTemplateMasterId 
                                    LEFT OUTER JOIN MasterOrderTNA AS mot ON mot.TaskMasterId=tt.TaskMasterId AND MOT.MasterOrderId='" + moId + @"'
                                    WHERE
                                    ISNULL(bm.BuyerId,'')='" + buyerId + @"'
                                    AND ISNULL(bm.BuyerDepartmentId,'" + buyerDepartmentId + @"')='" + buyerDepartmentId + @"'
                                    AND ISNULL(bm.BuyerDivisionId  ,'" + buyerDivisionId + "')='" + buyerDivisionId + "'";
                return _sqlRepository.GetDataCollection(sql);


            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        private string GetEntityPrefix(string entityId)
        {
            var epx = _entityService.GetEntityPrefix(entityId);
            if (string.IsNullOrEmpty(epx)) throw new Exception("Selected entity has no prefix...");
            return epx.Trim() + DateTime.Now.ToString("yy") + _pkGeneratorService.GetAutoNumber(entityId, PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        //public IEnumerable<object> GetDepartmentPersonCbo(string plantId, string partyAccountGroupId, string partyId)
        //{
        //    try
        //    {
        //        var parameter = "";
        //        var dataList = _customerDivisionRepository.Query(t => t.PlantId == plantId && t.PartyAccountGroupId == partyAccountGroupId).Select().ToList();
        //        if (dataList.IsNotNull() && dataList.Count() == 1)
        //        {
        //            if (dataList[0].PartyId == "-1")
        //                parameter += " AND CD.PartyId='-1'";
        //            else
        //                parameter += " AND CD.PartyId='" + partyId + "'";
        //        }
        //        else
        //            parameter += " AND CD.PartyId='" + partyId + "'";
        //        var sql = @"SELECT  CDP.PartyRespnsiblePersonId AS [Value], CM.ContactPerson AS [Text]
        //                FROM [MST].[CustomerDivisionResPerson] AS CDP
        //                JOIN [MST].[CustomerDivision] AS CD ON CDP.CustomerDivisionId=CD.Id
        //                JOIN [MST].[ContactMaster] AS CM ON CDP.PartyRespnsiblePersonId=CM.Id
        //                WHERE CD.PlantId='" + plantId + "' AND CD.PartyAccountGroupId='" + partyAccountGroupId + "'" + parameter;
        //        return _sqlRepository.GetDataCollection(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
        //    }
        //}

        public GridModel Query(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT A.Id, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId,A.AddedDate AS CreationDate,a.AddedBy AS CreatedBy
                                    , A.OrderType, A.PartyId, P.Code CustomerCode,P.UserName AS CustomerName, A.BuyerId,B.UserName Buyer
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId,OC.UserName AS OrderCategory, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' AND PlantId=A.PlantId)
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage,A.BuyerDepartmentId
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.IsExtraOrderPercentage,PM.UserName ProductMaster,OS.UserName OrderStatus,A.AddedDate,A.AddedBy
                                       ,A.OwnReferenceNo,A.BuyerReferenceNo
									   ,[BuyerReferenceNoItem]=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     [OwnItem]=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									  ,A.PaymentTermId,A.PaymentTermDays,A.ExceptionalProcessId,A.ExceptionalSubProcessId
                                   
                                   ,ContractNo=STUFF((select distinct ','+CNT.ContractNo from dbo.Contract CNT
															INNER JOIN trn.SalesOrder XSO  ON XSO.ContractId=CNT.Id	  
															INNER JOIN trn.MasterOrderItem XMOI  ON XMOI.Id=XSO.MasterOrderItemId	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
									MasterLCNo=STUFF((select distinct ','+MLC.LCRef from dbo.Contract CNT
															INNER JOIN trn.SalesOrder XSO  ON XSO.ContractId=CNT.Id	  
															INNER JOIN trn.MasterOrderItem XMOI  ON XMOI.Id=XSO.MasterOrderItemId
															LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            FROM [TRN].[MasterOrder] AS A
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN TRN.Commitment COM ON COM.Id=A.CommitmentId
							LEFT JOIN [MST].[ProductMaster] PM ON COM.ProductMasterId=PM.Id
                            LEFT JOIN HKP.OrderStatus OS ON OS.Id=A.OrderStatusId
                            LEFT JOIN hkp.OrderCategory AS oc ON oc.Id=a.OrderCategoryId
                            LEFT JOIN HKP.Buyer B ON B.Id=A.BuyerId
                            WHERE A.CompanyId='" + companyId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetList(string companyId, string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";


            string sql = @"select top(1000)* from (SELECT A.Id,A.TaskTemplateMasterId, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId, EN.UserName Entity,FORMAT(A.AddedDate,'dd-MMM-yyyy') AS CreationDate,a.AddedBy AS CreatedBy
                                    , A.OrderType, A.PartyId, P.Code CustomerCode, P.UserName AS CustomerName, A.BuyerId,B.UserName Buyer
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId,OC.UserName AS OrderCategory, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
									,MS.TotalAmount
                                    , A.NoOfLineItem, A.ResponsiblePersonId,(EI.EmployeeCode+'-'+ EI.EmployeeName) AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' AND PlantId=A.PlantId)
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage,A.BuyerDepartmentId
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.IsExtraOrderPercentage,PM.UserName ProductMaster,OS.UserName OrderStatus,A.AddedDate,A.AddedBy
                                       ,A.OwnReferenceNo,A.BuyerReferenceNo
									   ,[BuyerReferenceNoItem]=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     [OwnItem]=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									  ,A.PaymentTermId,A.PaymentTermDays,A.ExceptionalProcessId,A.ExceptionalSubProcessId
                                   
                                   ,ContractNo=STUFF((select distinct ','+CNT.ContractNo from dbo.Contract CNT
															INNER JOIN trn.SalesOrder XSO  ON XSO.ContractId=CNT.Id	  
															INNER JOIN trn.MasterOrderItem XMOI  ON XMOI.Id=XSO.MasterOrderItemId	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
									MasterLCNo=STUFF((select distinct ','+MLC.LCRef from dbo.Contract CNT
															INNER JOIN trn.SalesOrder XSO  ON XSO.ContractId=CNT.Id	  
															INNER JOIN trn.MasterOrderItem XMOI  ON XMOI.Id=XSO.MasterOrderItemId
															LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),CP.PaymentTermId DefaultPaymentTermId,RC.Process RemarksControl,URC.RemarkControlId,FORMAT(A.BaseOnDueDate,'dd-MMM-yyyy')BaseOnDueDate,FORMAT(A.MatureDate,'dd-MMM-yyyy')MatureDate,ISNULL(CP.IsBillDiscountingDays,0)IsBillDiscountingDays
                            FROM [TRN].[MasterOrder] AS A
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=A.PartyId AND CP.PartyType='Customer' AND CP.PlantId=A.PlantId
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN TRN.Commitment COM ON COM.Id=A.CommitmentId
							LEFT JOIN [MST].[ProductMaster] PM ON COM.ProductMasterId=PM.Id
                            LEFT JOIN HKP.OrderStatus OS ON OS.Id=A.OrderStatusId
                            LEFT JOIN hkp.OrderCategory AS oc ON oc.Id=a.OrderCategoryId
                            LEFT JOIN HKP.Buyer B ON B.Id=A.BuyerId
                            LEFT JOIN ORG.Entity EN ON EN.Id=A.EntityId
							LEFT JOIN(select moi.MasterOrderId,TotalAmount=SUM(SO.Qty*SO.Rate) 
									from TRN.MasterOrderItem moi
									LEFT JOIN TRN.SalesOrder so on so.MasterOrderItemId=moi.Id
									Group By moi.MasterOrderId) MS ON MS.MasterOrderId=A.Id
                            LEFT JOIN TRN.UserRemarksControl URC ON URC.MasterOrderId=A.Id
							LEFT JOIN [HKP].[RemarksControl] RC ON RC.Id=URC.RemarkControlId
                            WHERE A.CompanyId='" + companyId + "') AS TEMP WHERE " + strkey + " ORDER BY AddedDate Desc";

            return _sqlRepository.GetDataCollection(sql, null);
        }

        public GridModel QueryIdependent(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT A.Id, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId
                                    , A.OrderType, A.PartyId, P.UserName AS CustomerName, A.BuyerId	
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' )
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage,A.BuyerDepartmentId
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.IsExtraOrderPercentage,PM.UserName ProductMaster,OS.UserName OrderStatus
                                    ,A.OwnReferenceNo,A.BuyerReferenceNo
                            FROM [TRN].[MasterOrder] AS A
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN TRN.Commitment COM ON COM.Id=A.CommitmentId
							LEFT JOIN [MST].[ProductMaster] PM ON COM.ProductMasterId=PM.Id
                            LEFT JOIN HKP.OrderStatus OS ON OS.Id=A.OrderStatusId
                            WHERE A.CompanyId='" + companyId + "' AND OrderType='Independent'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetMasterOrderList(string companyId, string plantId)
        {
            try
            {
                var sql = @"SELECT  A.Id AS  MasterOrderId,MOI.Id MasterOrderItemId, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId
                                    , A.PartyId, P.UserName AS CustomerName, A.CurrencyId,CO.BaseCurrencyId, A.TotalQty	
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress								    
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,0 Active
                                    --,ISNULL(CNT.ContractNo,'')ContractNo,ISNULL(MLC.LCRef,'')LCRef
									,B.UserName Buyer,ISNULL(A.BuyerReferenceNo,'')BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'')OwnReferenceNo,ISNULL(MOI.BuyerReferenceNo,'') StyleNo
									,ISNULL(MOI.OwnReferenceNo,'') OwnStyleNo
                                    ,MM.UserName MaterialMaster,MMA.StandardName Article,ISNULL(AA.ArticlePartyName,P.UserName) CustomerArticle,MOI.TotalQty ItemQty--,SO.ContractId
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    ,PONumber=  REPLACE(REPLACE(
										            STUFF((SELECT DISTINCT ','+CPO.PONumber from 
	                                                    TRN.SalesOrder XSO 
		                                                    JOIN [TRN].[CustomerPO] CPO ON CPO.Id=XSO.CustomerPOId
		                                                      JOIN trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    LEFT OUTER JOIN TRN.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                                WHERE MOI.Id=Xmoi.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                    ,'&amp;','&'), 'amp;', '')	
									,ContractNo=Stuff((
                    SELECT distinct',' + C.ContractNo
                    FROM  dbo.[Contract] C 
					LEFT JOIN TRN.SalesOrder SO on SO.ContractId = C.Id
                    WHERE SO.MasterOrderItemId=MOI.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
					,ContractId=Stuff((
                    SELECT distinct',' + C.Id
                    FROM  dbo.[Contract] C 
					LEFT JOIN TRN.SalesOrder SO on SO.ContractId = C.Id
                    WHERE SO.MasterOrderItemId=MOI.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
					,LCRef=Stuff((
                    SELECT distinct',' + MLC.LCRef
                    FROM dbo.MasterLC MLC 
					left join dbo.[Contract] C on C.MasterLCId=MLC.Id 
					LEFT JOIN TRN.SalesOrder SO on SO.ContractId = C.Id
                    WHERE SO.MasterOrderItemId=MOI.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
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
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=MOI.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=MOI.ArticleId
                            LEFT JOIN dbo.ArticleAlias AA ON AA.ArticleId=MMA.Id AND AA.MasterOrderItemId=MOI.Id
                            WHERE A.CompanyId='" + companyId + "' AND A.PlantId='" + plantId + "' ORDER BY P.Id";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        //public IEnumerable<object> GetDepartmentPersonList(string plantId, string partyAccountGroupId, string partyId, bool flag)
        //{
        //    try
        //    {
        //        var parameter = "";
        //        var dataList = _customerDivisionRepository.Query(t => t.PlantId == plantId && t.PartyAccountGroupId == partyAccountGroupId).Select().ToList();
        //        if (dataList.IsNotNull() && dataList.Count() == 1)
        //        {
        //            if (dataList[0].PartyId == "-1")
        //                parameter += " AND CD.PartyId='-1'";
        //            else
        //                parameter += " AND CD.PartyId='" + partyId + "'";
        //        }
        //        else
        //            parameter += " AND CD.PartyId='" + partyId + "'";
        //        if (flag)
        //            parameter += " AND ORD.IsDefault=1";
        //        var sql = @"SELECT NULL AS Id, NULL MasterOrderId, CustomerDivisionId, CDP.OrderResponsibleDepartmentId, ORD.[Name] AS Department
        //                        , CDP.OurRespnsiblePersonId, EI.EmployeeCode, EI.EmployeeName
        //                        , CDP.PartyRespnsiblePersonId, CM.ContactPerson AS PartyRespnsiblePerson
        //                FROM [MST].[CustomerDivisionResPerson] AS CDP
        //                JOIN [MST].[CustomerDivision] AS CD ON CDP.CustomerDivisionId=CD.Id
        //                JOIN [MST].[OrderResponsibleDepartment] AS ORD ON CDP.OrderResponsibleDepartmentId=ORD.Id
        //                LEFT JOIN [EmployeeInformation] AS EI ON CDP.OurRespnsiblePersonId=EI.SystemId
        //                LEFT JOIN [MST].[ContactMaster] AS CM ON CDP.PartyRespnsiblePersonId=CM.Id
        //                WHERE CD.PlantId='" + plantId + "' AND CD.PartyAccountGroupId='" + partyAccountGroupId + "'" + parameter;
        //        return _sqlRepository.GetDataCollection(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
        //    }
        //}

        public IEnumerable<object> GetResponsiblePersonList(string masterId)
        {
            try
            {
                var sql = @"SELECT MOP.Id, MOP.MasterOrderId	
                                , MOP.OrderResponsibleDepartmentId, ORD.[Name] AS Department
                                , MOP.OurRespnsiblePersonId, EI.EmployeeCode, EI.EmployeeName	
                                , MOP.PartyRespnsiblePersonId, CM.ContactPerson AS PartyRespnsiblePerson
                        FROM [TRN].[MasterOrderResPerson] AS MOP
                        JOIN [MST].[OrderResponsibleDepartment] AS ORD ON MOP.OrderResponsibleDepartmentId=ORD.Id
                        LEFT JOIN [EmployeeInformation] AS EI ON MOP.OurRespnsiblePersonId=EI.SystemId
                        LEFT JOIN [MST].[ContactMaster] AS CM ON MOP.PartyRespnsiblePersonId=CM.Id
                        WHERE MOP.MasterOrderId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetArticleCodeList(string materialMasterId, string articleCode)
        {
            try
            {
                var parameters = "";
                if (string.IsNullOrEmpty(materialMasterId) && materialMasterId == "null")
                    parameters += " AND ART.MaterialMasterId='materialMasterId'";
                var sql = @"SELECT ART.Id, ART.Code AS ArticleCode, ART.ShortName, ART.StandardName AS ArticleName
	                     , ART.MaterialMasterId, MM.Code AS MaterialMasterCode, MM.UserName AS MaterialMasterName
                    FROM MST.MaterialMasterArticle AS ART
                    LEFT JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId=MM.Id
                    WHERE ART.Code LIKE '%" + articleCode + "%' " + parameters + " ORDER BY ART.Code";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel GetCompanyPartyList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string customerVendor)
        {
            try
            {
                var sql = "";
                if (!string.IsNullOrEmpty(plantId))
                    sql += " AND CP.PlantId='" + plantId + "'";

                parameters.CmdText = @"SELECT P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
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
                                    , CP.TaxApplicable, CP.IsTaxApplicableChangeable, CP.PlantId
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
                                    WHERE P.Archive=0 AND P.Active=1 AND P.CompanyGroupId='" + companyGroupId + "' AND P.PartyType IN ('" + PartyType.Party + "', '" + PartyType.Company + "') AND CP.CompanyId='" + companyId + "'" + sql;
                // If this params null will return all customer and vendor list either specific.
                if (!string.IsNullOrEmpty(customerVendor))
                    parameters.CmdText += " AND CP.PartyType='" + customerVendor + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetMasterItemList(string masterOrderId)
        {
            try
            {
                var sql = @"SELECT MOI.Id, MOI.MasterOrderId, MOI.InquiryItemId, MOI.SampleItemId, MOI.TestingStandardId
                           ,Status=STUFF((select distinct ','+case when CheckByStatus = 'Checked' then 'Checked' else 'Pending' end from trn.SalesOrder XSO 
							                                where XSO.MasterOrderItemId=MOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,CheckStatus=STUFF((select distinct ','+XSO.CheckByStatus from trn.SalesOrder XSO 
							                                where XSO.MasterOrderItemId=MOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						   ,ApproveStatus=STUFF((select distinct ','+XSO.ApprovedStatus from trn.SalesOrder XSO 
							                                where XSO.MasterOrderItemId=MOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
	                         , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	                         , MOI.ArticleId, ART.StandardName AS ArticleName
	                         , MOI.BuyerReferenceNo, MOI.OwnReferenceNo, MOI.TotalQty
	                         , MOI.OrderWastagePercentage, MOI.ExtraOrderPercentage, MOI.ProductionGrouping, MM.HSNCodeId
							 , ISNULL(HART.HasAttribute,CAST(0 AS BIT)) AS HasAttribute
                             , ISNULL((select sum(SO.Qty) from TRN.SalesOrder SO where So.MasterOrderItemId = MOI.Id),0) as SOQty
                             , ISNULL((select sum(so.Rate*so.Qty) from TRN.SalesOrder SO where So.MasterOrderItemId = MOI.Id),0) as TotalAmount
                             ,MOI.Type,MOI.IsRepeat, PM.UserName AS ProductMaster
                            --a ,MOI.ContractId,CNT.ContractNo,MLC.LCRef
							 ,MOI.BuyerItemDescription,MOI.MainRawMaterialDescription,MOI.PartyId,MOI.EntityIdWithinGroup,MOI.EntityIdWithinCompany,MOI.JobWorkType
                             , EntityOrVendorName= CASE WHEN MOI.EntityIdWithinCompany<>'' THEN EWCC.UserName +' - '+EWC.UserName 
					                        WHEN MOI.EntityIdWithinGroup<>'' THEN EWGC.UserName+' - '+EWG.UserName
					                        WHEN MOI.PartyId<>'' THEN PRT.UserName
					                        ELSE PRT.UserName END
                            ,enableJobOrOutSource=CASE WHEN MOI.[Type]='JobWork' OR MOI.[Type]='OutSource' THEN 'false' ELSE 'true' END
                            ,MOI.ProductLibraryId,MOI.FileName,MOI.Remark,MOI.OrderStatusId,MOI.UOMId
                            ,BOQNo=(Select COUNT(Id) from [dbo].[QuickBOQ] Where MasterOrderItemId=MOI.Id)
                            ,SONo=(Select COUNT(Id) from TRN.SalesOrder Where MasterOrderItemId=MOI.Id)
                            ,MOI.Consignment,MOI.OrderCostingMasterTemplateId,'' TempList,PM.Id ProductMasterId,CAST(1 as bit) ByDefault,PL.UserName ProductLibrary,OCT.UserName OrderCostingMasterTemplate,MOI.Rate,ISNULL(AA.ArticlePartyName,P.UserName) CustomerArticle,ISNULL(ART.IsDefaultProductionGrouping,0)IsDefault,MOI.ItemCategory
                        ,MOI.CSPT                        
                        FROM TRN.MasterOrderItem AS MOI
                        JOIN MST.MaterialMaster AS MM ON MOI.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON MOI.ArticleId=ART.Id
                        LEFT JOIN HKP.ProductionGrouping PG ON PG.Id=ART.ProductionGroupingId
						LEFT JOIN (SELECT AttributeSetLength=CASE WHEN COUNT(MaterialMasterId)>0 THEN COUNT(MaterialMasterId) ELSE 0 END
                                                , HasAttribute=CASE WHEN COUNT(MaterialMasterId)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MaterialMasterId
                                            FROM MST.MaterialMasterAttribute GROUP BY MaterialMasterId) AS HART ON HART.MaterialMasterId=MM.Id
                        LEFT JOIN dbo.ArticleAlias AA ON AA.ArticleId=ART.Id AND AA.MasterOrderItemId=MOI.Id
						LEFT JOIN HKP.Party P ON P.Id=AA.Partyid
                        LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId= MM.Id
						LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                        --LEFT JOIN dbo.Contract CNT ON CNT.Id=MOI.ContractId
						--LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId
						LEFT JOIN ORG.Entity AS EWC ON MOI.EntityIdWithinCompany=EWC.Id
						LEFT JOIN ORG.Company AS EWCC ON EWC.CompanyId=EWCC.Id
                        LEFT JOIN ORG.Entity AS EWG ON MOI.EntityIdWithinGroup=EWG.Id
						LEFT JOIN ORG.CompanyGroup AS EWGC ON EWG.CompanyGroupId=EWGC.Id
                        LEFT JOIN HKP.Party AS PRT ON MOI.PartyId=PRT.Id
                        LEFT JOIN dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId
                        LEFT JOIN dbo.OrderCostingMasterTemplate OCT ON OCT.Id=MOI.OrderCostingMasterTemplateId
                        WHERE MOI.MasterOrderId='" + masterOrderId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetMasterItemForApproveList(string masterOrderId, string empId)
        {
            try
            {
                var sql = @"SELECT MOI.Id, MOI.MasterOrderId, MOI.InquiryItemId, MOI.SampleItemId, MOI.TestingStandardId
                           ,Status=STUFF((select distinct ','+case when CheckByStatus = 'Checked' then 'Checked' else 'Pending' end from trn.SalesOrder XSO 
							                                where XSO.MasterOrderItemId=MOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,CheckStatus=STUFF((select distinct ','+XSO.CheckByStatus from trn.SalesOrder XSO 
							                                where XSO.MasterOrderItemId=MOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						   ,ApproveStatus=STUFF((select distinct ','+XSO.ApprovedStatus from trn.SalesOrder XSO 
							                                where XSO.MasterOrderItemId=MOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
	                         , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	                         , MOI.ArticleId, ART.StandardName AS ArticleName
	                         , MOI.BuyerReferenceNo, MOI.OwnReferenceNo, MOI.TotalQty
	                         , MOI.OrderWastagePercentage, MOI.ExtraOrderPercentage, MOI.ProductionGrouping, MM.HSNCodeId
							 , ISNULL(HART.HasAttribute,CAST(0 AS BIT)) AS HasAttribute
                             , ISNULL((select sum(SO.Qty) from TRN.SalesOrder SO where So.MasterOrderItemId = MOI.Id),0) as SOQty
                             , ISNULL((select sum(so.Rate*so.Qty) from TRN.SalesOrder SO where So.MasterOrderItemId = MOI.Id),0) as TotalAmount
                             ,MOI.Type,MOI.IsRepeat, PM.UserName AS ProductMaster
                            --a ,MOI.ContractId,CNT.ContractNo,MLC.LCRef
							 ,MOI.BuyerItemDescription,MOI.MainRawMaterialDescription,MOI.PartyId,MOI.EntityIdWithinGroup,MOI.EntityIdWithinCompany,MOI.JobWorkType
                             , EntityOrVendorName= CASE WHEN MOI.EntityIdWithinCompany<>'' THEN EWCC.UserName +' - '+EWC.UserName 
					                        WHEN MOI.EntityIdWithinGroup<>'' THEN EWGC.UserName+' - '+EWG.UserName
					                        WHEN MOI.PartyId<>'' THEN PRT.UserName
					                        ELSE PRT.UserName END
                            ,enableJobOrOutSource=CASE WHEN MOI.[Type]='JobWork' OR MOI.[Type]='OutSource' THEN 'false' ELSE 'true' END
                            ,MOI.ProductLibraryId,MOI.FileName,MOI.Remark,MOI.OrderStatusId,MOI.UOMId
                            ,BOQNo=(Select COUNT(Id) from [dbo].[QuickBOQ] Where MasterOrderItemId=MOI.Id)
                            ,SONo=(Select COUNT(Id) from TRN.SalesOrder Where MasterOrderItemId=MOI.Id)
                            ,MOI.Consignment,MOI.OrderCostingMasterTemplateId,'' TempList,PM.Id ProductMasterId,CAST(1 as bit) ByDefault,PL.UserName ProductLibrary,OCT.UserName OrderCostingMasterTemplate,MOI.Rate,ISNULL(AA.ArticlePartyName,P.UserName) CustomerArticle
                        FROM TRN.MasterOrderItem AS MOI
                        JOIN MST.MaterialMaster AS MM ON MOI.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON MOI.ArticleId=ART.Id
						LEFT JOIN (SELECT AttributeSetLength=CASE WHEN COUNT(MaterialMasterId)>0 THEN COUNT(MaterialMasterId) ELSE 0 END
                                                , HasAttribute=CASE WHEN COUNT(MaterialMasterId)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MaterialMasterId
                                            FROM MST.MaterialMasterAttribute GROUP BY MaterialMasterId) AS HART ON HART.MaterialMasterId=MM.Id
                        LEFT JOIN dbo.ArticleAlias AA ON AA.ArticleId=ART.Id AND AA.MasterOrderItemId=MOI.Id
						LEFT JOIN HKP.Party P ON P.Id=AA.Partyid
                        LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId= MM.Id
						LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                        --LEFT JOIN dbo.Contract CNT ON CNT.Id=MOI.ContractId
						--LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId
						LEFT JOIN ORG.Entity AS EWC ON MOI.EntityIdWithinCompany=EWC.Id
						LEFT JOIN ORG.Company AS EWCC ON EWC.CompanyId=EWCC.Id
                        LEFT JOIN ORG.Entity AS EWG ON MOI.EntityIdWithinGroup=EWG.Id
						LEFT JOIN ORG.CompanyGroup AS EWGC ON EWG.CompanyGroupId=EWGC.Id
                        LEFT JOIN HKP.Party AS PRT ON MOI.PartyId=PRT.Id
                        LEFT JOIN dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId
                        LEFT JOIN dbo.OrderCostingMasterTemplate OCT ON OCT.Id=MOI.OrderCostingMasterTemplateId
                        WHERE MOI.MasterOrderId='" + masterOrderId + @"' AND MOI.Id IN(Select distinct SO.MasterOrderItemId from  TRN.SalesOrder SO
Where SO.CheckByStatus = 'Checked' AND ApprovedStatus = 'To Be Approve' AND SO.ApproveBy = '" + empId + "')";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetItemsData(string masterOrderId)
        {
            try
            {

                var sql = @"SELECT  mo.MasterOrderNo,moi.Id MasterOrderItemId
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                             
                                    ,SO.Description
									,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM trn.MasterOrderItem moi
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON moi.Id = SO.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                  WHERE moi.MasterOrderId ='" + masterOrderId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetAttributeListByMaterialMasterId(string materialMasterId)
        {
            try
            {
                var sql = @"SELECT NULL AS Id
                            , MMA.MaterialMasterId
                            , MMA.MaterialAttributeId AS AttributeId
                            , MA.UserName AS AttributeName
                            , MMA.IsFreeField
                            , MMA.IsPreDefinedField
                            , MMA.IsMandatory
		                    , MAV.Id AS AttributeValueId
		                    , ValueFreeText=MAV.UserName
                            , MA.ValueAssignmentLevel
		                    , MAV.SourceType
                    FROM MST.MaterialMasterAttribute AS MMA
                    LEFT JOIN HKP.MaterialAttribute AS MA ON MMA.MaterialAttributeId = MA.Id
                    LEFT JOIN (SELECT * FROM HKP.MaterialAttributeValue WHERE Active=1 AND IsDefault=1) AS MAV 
		                    ON MAV.MaterialAttributeId=MMA.MaterialAttributeId AND MAV.SourceType=MA.ValueAssignmentLevel
                    WHERE MMA.MaterialMasterId='" + materialMasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetOrderAttributeListByMasterId(string masterItemId, string materialMasterId)
        {
            try
            {
                var sql = @"SELECT MOV.Id
                                , MMA.MaterialMasterId
                                , MOV.MasterOrderItemId
                                , MMA.MaterialAttributeId AS AttributeId
                                , MA.UserName AS AttributeName
                                , MMA.IsFreeField
                                , MMA.IsPreDefinedField
                                , MMA.IsMandatory
		                        , AttributeValueId=CASE WHEN (MOV.Id IS NULL AND MAV.IsDefault=1) THEN MAV.Id 
								                        WHEN MOV.Id<>'' THEN MOV.AttributeValueId END
		                        , ValueFreeText =CASE WHEN (MOV.Id IS NULL AND MAV.IsDefault=1) THEN MAV.UserName
							                          WHEN MOV.AttributeValueId<>'' THEN MAV.UserName ELSE MOV.ValueFreeText END
                                , MOV.ValueRemarks, MA.ValueAssignmentLevel, MAV.SourceType, MOV.ReferenceSampleandRemarks
                        FROM MST.MaterialMasterAttribute AS MMA
                        LEFT JOIN HKP.MaterialAttribute AS MA ON MMA.MaterialAttributeId = MA.Id
                        LEFT JOIN (SELECT A.*, B.MaterialMasterId FROM TRN.MasterOrderAttributeValue AS A
			                        JOIN TRN.MasterOrderItem AS B ON A.MasterOrderItemId=B.Id WHERE B.Id='" + masterItemId + @"') AS MOV 
			                        ON MOV.AttributeId=MMA.MaterialAttributeId AND MMA.MaterialMasterId=MOV.MaterialMasterId
                        LEFT JOIN (SELECT * FROM HKP.MaterialAttributeValue WHERE Active=1) AS MAV 
		                        ON MAV.MaterialAttributeId=MMA.MaterialAttributeId AND MOV.AttributeValueId=MAV.Id AND MAV.SourceType=MA.ValueAssignmentLevel
                        WHERE MMA.MaterialMasterId='" + materialMasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetSOList(string masterItemId)
        {
            try
            {
                var sql = @"SELECT  SO.Id,SO.ParentId
                            , SO.MasterOrderItemId
                            , MOI.MaterialMasterId
                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), SO.DeliveryDate, 106),' ','-')
                            , SO.DestinationId, D.UserName Destination
                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), SO.CommitmentDate, 106),' ','-')
                            , SO.ShipmentModeId
                            , SO.CustomerPOId
		                    , po.PONumber
                            ,SO.DestinationDescription
                            , SO.OrderStatusId, SO.OrderCategoryId
                            , SO.SOType, SO.ResponsiblePersonId
                            , SO.UpCharge, SO.Qty, SO.Rate, SO.IsFirstEntry,SO.Discount,(EMP.EmployeeCode+'-'+EMP.EmployeeName) ResponsiblePersonName
                            ,FORMAT (SO.LSD, 'dd-MMM-yyyy') as LSD ,FORMAT (SO.MainRawMaterialInhouseDate, 'dd-MMM-yyyy') as MainRawMaterialInhouseDate
                            ,FORMAT (SO.OtherRawMaterialInhouseDate, 'dd-MMM-yyyy') as OtherRawMaterialInhouseDate
                            ,FORMAT (SO.PlanExFactoryDate, 'dd-MMM-yyyy') as PlanExFactoryDate
                            , hasFirst=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[FirstCharacteristics] WHERE SalesOrderId=SO.Id)
                            --, (SELECT ISNULL(sum(Qty),0) total FROM(
							--		select Qty FROM  TRN.FirstCharacteristics AS FCS WHERE SO.Id= FCS.SalesOrderId
							--		union
							--		select Qty FROM TRN.SecondCharacteristics AS SCS WHERE SO.Id= SCS.SalesOrderId
							--		union
							--		select Qty FROM TRN.ThirdCharacteristics AS TCS WHERE SO.Id= TCS.SalesOrderId
							--	) SoT ) as SKUQty
                            ,(SELECT ISNULL(sum(Qty),0) FROM TRN.FirstCharacteristics AS FCS WHERE SO.Id= FCS.SalesOrderId) SKUQty
                            , isTax=(SELECT ISNULL(COUNT(DISTINCT SalesOrderId),0) FROM [TRN].[SalesOrderTax] WHERE SalesOrderId=SO.Id)
                            ,ISNULL(POD.ProductionOrderId,'') ProductionOrderId,SO.Reason,SO.Description,SO.CM,SO.SalesOrderYear,SO.WeekNo
                            ,SO.ProductionBookedQty,SO.ProductionBookingLevel,SO.SalesExpense,SO.CM,SO.DirectMaterialCost,SO.DirectProcessCost,SO.Commission,SO.ValueLoss,SO.Other,SO.StockResponsiblePersonId,SO.ShipmentFromStock,SO.ProductionType,SEMP.EmployeeName StockResponsiblePerson,SO.PackingTypeId,PT.UserName PackingType,SO.ContractId,C.ContractNo,SO.CheckByDate,SO.CheckByStatus,SO.ApproveBy,SO.ApproveByDate,SO.ApprovedStatus,SO.DeliveryGroup,FORMAT(PO.PODate,'dd-MMM-yyyy')PODate,SO.LineItemReference
                    FROM [TRN].[SalesOrder] AS SO
                   -- LEFT JOIN TRN.FirstCharacteristics SKU ON SKU.SalesOrderId=SO.Id
                    JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
                    LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                    LEFT JOIN dbo.EmployeeInformation AS EMP ON EMP.SystemId = SO.ResponsiblePersonId
                    LEFT JOIN dbo.EmployeeInformation AS SEMP ON SEMP.SystemId = SO.StockResponsiblePersonId
                    LEFT JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
                    LEFT JOIN [MST].[Destination] D ON D.Id=SO.DestinationId
                    LEFT JOIN HKP.PackingType PT ON PT.Id=SO.PackingTypeId
                    LEFT JOIN dbo.Contract C ON C.Id=SO.ContractId
                    WHERE SO.MasterOrderItemId='" + masterItemId + "' ORDER BY SO.DeliveryDate";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }



        public IEnumerable<object> GetpackingTypeList(string SOId, string PackingType)
        {
            try
            {
                string sql = "";
                if (PackingType == "AssortedAssorted")
                {
                    sql = @"select NULL Id,CV1.Id FGFirstCharacteristicsId,CV1.UserName Color,CV2.Id FGSecondCharacteristicsId,CV2.UserName Size,sum(sku2.Qty) Quantity,0 ToPlanQuantity,0 [Plan]
                                                from TRN.SalesOrder SO
                                                left join TRN.FirstCharacteristics sku1 on sku1.SalesOrderId=SO.Id
                                                left join [HKP].[CharacteristicsValue] CV1 on CV1.Id=sku1.CharacteristicsValueId
                                                left join TRN.SecondCharacteristics sku2 on sku2.SalesOrderId=SO.Id
                                                left join [HKP].[CharacteristicsValue] CV2 on CV2.Id=sku2.CharacteristicsValueId
                                                where SO.Id " + SOId + @" and CV1.UserName is not null
                                                group by CV1.Id,CV1.UserName ,CV2.Id ,CV2.UserName";
                }
                else if (PackingType == "AssortedSolid")
                {
                    sql = @"select NULL Id,CV1.Id FGFirstCharacteristicsId,CV1.UserName Color,sum(sku1.Qty) Quantity,0 ToPlanQuantity,0 [Plan]
                                                from TRN.SalesOrder SO
                                                left join TRN.FirstCharacteristics sku1 on sku1.SalesOrderId=SO.Id
                                                left join [HKP].[CharacteristicsValue] CV1 on CV1.Id=sku1.CharacteristicsValueId

                                                where SO.Id " + SOId + @" and CV1.UserName is not null
                                                group by CV1.UserName,CV1.Id";
                }
                else if (PackingType == "SolidSolid")
                {
                    sql = @"select NULL Id,sum(SO.Qty) Quantity,0 ToPlanQuantity,0 [Plan]
                                                from TRN.SalesOrder SO
                                                where SO.Id " + SOId + @"";
                }
                else
                {
                    sql = @"select NULL Id,CV2.Id FGSecondCharacteristicsId,CV2.UserName Size,sum(sku2.Qty) Quantity,0 ToPlanQuantity,0 [Plan]
                                                from TRN.SalesOrder SO
                                                left join TRN.SecondCharacteristics sku2 on sku2.SalesOrderId=SO.Id
                                                left join [HKP].[CharacteristicsValue] CV2 on CV2.Id=sku2.CharacteristicsValueId
                                                where SO.Id " + SOId + @" and CV2.UserName is not null
                                                group by CV2.Id,CV2.UserName";
                }
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }


        public IEnumerable<object> GetFirstSkuSalesOrderId(string salesOrderId)
        {
            try
            {
                var sql = @"SELECT FCH.Id	
	                        , FCH.[Sequence]	
	                        , FCH.SalesOrderId
                            , NULL FirstCharacteristicsId
                            , NULL SecondCharacteristicsId
	                        , FCH.CharacteristicsId	
	                        , CH.UserName AS CharacteristicsName
	                        , FCH.CharacteristicsValueId
	                        , CHV.UserName AS CharacteristicsValueName
	                        , FCH.ValueFreeText	
	                        , MOI.MaterialMasterId
	                        , CH.ValueAssignmentLevel
	                        , FCH.Qty
                        FROM [TRN].[FirstCharacteristics] AS FCH
                        JOIN [HKP].[Characteristics] AS CH ON FCH.CharacteristicsId=CH.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV ON FCH.CharacteristicsValueId=CHV.Id
                        JOIN [TRN].[SalesOrder] AS SO ON FCH.SalesOrderId=SO.Id
                        JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                        WHERE FCH.SalesOrderId='" + salesOrderId + "' ORDER BY FCH.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetSecondSkuSalesOrderId(string salesOrderId)
        {
            try
            {
                var sql = @"SELECT FCH.Id	
	                        , FCH.[Sequence]	
	                        , FCH.SalesOrderId	
                            , FCH.FirstCharacteristicsId
	                        , NULL SecondCharacteristicsId
	                        , FCH.CharacteristicsId	
	                        , CH.UserName AS CharacteristicsName
	                        , FCH.CharacteristicsValueId	
	                        , FCH.ValueFreeText	
	                        , MOI.MaterialMasterId
	                        , CH.ValueAssignmentLevel
	                        , FCH.Qty
                        FROM [TRN].[SecondCharacteristics] AS FCH
                        JOIN [HKP].[Characteristics] AS CH ON FCH.CharacteristicsId=CH.Id
                        JOIN [TRN].[SalesOrder] AS SO ON FCH.SalesOrderId=SO.Id
                        JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                        WHERE FCH.SalesOrderId='" + salesOrderId + "' ORDER BY FCH.FirstCharacteristicsId, FCH.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetThirdSkuSalesOrderId(string salesOrderId)
        {
            try
            {
                var sql = @"SELECT FCH.Id	
	                        , FCH.[Sequence]	
	                        , FCH.SalesOrderId	
                            , NULL FirstCharacteristicsId
                            , FCH.SecondCharacteristicsId
	                        , FCH.CharacteristicsId	
	                        , CH.UserName AS CharacteristicsName
	                        , FCH.CharacteristicsValueId	
	                        , FCH.ValueFreeText	
	                        , MOI.MaterialMasterId
	                        , CH.ValueAssignmentLevel
	                        , FCH.Qty
                        FROM [TRN].[ThirdCharacteristics] AS FCH
                        JOIN [HKP].[Characteristics] AS CH ON FCH.CharacteristicsId=CH.Id
                        JOIN [TRN].[SalesOrder] AS SO ON FCH.SalesOrderId=SO.Id
                        JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                        WHERE FCH.SalesOrderId='" + salesOrderId + "' ORDER BY FCH.SecondCharacteristicsId, FCH.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetCharacteristicsByMaterialMasterId(string materialMasterId)
        {
            //Query by Sir for Characteristics with Value
            //            string _sql = @"SELECT MM.UserName MaterialMaster,MMC.MaterialMasterId ,C.ValueAssignmentLevel,MMC.CharacteristicsId,C.UserName Characteristics,cv.Id CVId,CV.UserName ValueName,MMC.Sequence,MMC.Id
            //FROM MST.MaterialMasterCharacteristics MMC
            //JOIN MST.MaterialMaster MM ON MM.Id=MMC.MaterialMasterId
            //JOIN HKP.Characteristics C ON C.Id=MMC.CharacteristicsId AND ISNULL(C.ValueAssignmentLevel,'')='General'
            //JOIN HKP.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id AND ISNULL(CV.MaterialMasterId,'')=''
            //WHERE MM.Id='2528'
            //UNION ALL
            //SELECT MM.UserName MaterialMaster,MMC.MaterialMasterId ,C.ValueAssignmentLevel,MMC.CharacteristicsId,C.UserName Characteristics,cv.Id CVId,CV.UserName ValueName,MMC.Sequence,MMC.Id
            //FROM MST.MaterialMasterCharacteristics MMC
            //JOIN MST.MaterialMaster MM ON MM.Id=MMC.MaterialMasterId
            //JOIN HKP.Characteristics C ON C.Id=MMC.CharacteristicsId AND ISNULL(C.ValueAssignmentLevel,'')='Specific'
            //JOIN HKP.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id AND ISNULL(CV.MaterialMasterId,'')<>'' AND MM.Id=CV.MaterialMasterId
            //WHERE MM.Id='2528'";

            string _sql = @"SELECT  MMC.CharacteristicsId AS [Value],NULL AS Id, MMC.Id AS MaterialMasterCharacteristicsId, c.UserName AS [Text], MMC.IsFreeField
	                            , MMC.IsPreDefinedField, MMC.IsMandatory, C.ValueAssignmentLevel, MMC.[Sequence]
	                            , CharacteristicsValueId = CASE WHEN (C.ValueAssignmentLevel='General' AND CV.IsDefault=1) THEN CV.Id ELSE NULL END

	                            , MaterialMasterCharacteristicsValueId = CASE WHEN (C.ValueAssignmentLevel='Specific' AND MMCV.IsDefault=1) THEN MMCV.Id ELSE NULL END
	                            , ValueFreeText =CASE WHEN (C.ValueAssignmentLevel='General' AND CV.IsDefault=1) THEN CV.UserName
					                               WHEN (C.ValueAssignmentLevel='Specific' AND MMCV.IsDefault=1) THEN MMCV.UserName ELSE NULL END
						        , MMC.MaterialMasterId, 0 AS FlagDisable
                            FROM MST.MaterialMasterCharacteristics AS MMC
                            JOIN HKP.Characteristics C ON MMC.CharacteristicsId=C.Id
                            LEFT JOIN (SELECT * FROM HKP.CharacteristicsValue WHERE Active=1 AND IsDefault=1 AND MaterialMasterId='" + materialMasterId + @"') AS CV ON CV.CharacteristicsId=MMC.CharacteristicsId AND CV.CharacteristicsId=C.Id
                            LEFT JOIN (SELECT * FROM HKP.CharacteristicsValue WHERE Active=1 AND IsDefault=1 AND MaterialMasterId='" + materialMasterId + @"') AS MMCV ON MMCV.MaterialMasterId=MMC.MaterialMasterId AND MMCV.CharacteristicsId=MMC.Id
                            WHERE MMC.MaterialMasterId='" + materialMasterId + "' ORDER BY MMC.[Sequence]";
            return _sqlRepository.GetDataCollection(_sql, null);
        }

        public IEnumerable<object> GetChValueCbo(string materialId)
        {
            string _sql = @"SELECT CV.Id AS [Value], CV.UserName AS [Text], CV.CharacteristicsId
                    FROM [MST].[MaterialMasterCharacteristics] AS MMC
                    JOIN [HKP].[Characteristics] C ON MMC.CharacteristicsId=C.Id
                    JOIN [HKP].[CharacteristicsValue] AS CV ON CV.CharacteristicsId = MMC.CharacteristicsId AND CV.CharacteristicsId = C.Id
                    WHERE MMC.MaterialMasterId = '" + materialId + "' AND CV.SourceType=C.ValueAssignmentLevel";
            return _sqlRepository.GetDataCollection(_sql, null);
        }

        public DataTable GetValueAssignmentLevel(string materialId)
        {
            try
            {
                string sql = @"SELECT DISTINCT C.ValueAssignmentLevel  FROM [MST].[MaterialMasterCharacteristics] MMC
                              JOIN [HKP].[Characteristics] C ON MMC.CharacteristicsId=C.Id
                              WHERE MMC.MaterialMasterId='" + materialId + @"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetChValueCboByMaterialId(string materialId)
        {
            string assignmentLevel = string.Empty;
            try
            {

                DataTable valueAssignmentLevel = GetValueAssignmentLevel(materialId);
                if (valueAssignmentLevel.Rows.Count > 0)
                {
                    assignmentLevel = valueAssignmentLevel.Rows[0]["ValueAssignmentLevel"].ToString();
                }
                string _sql = string.Empty;
                if (assignmentLevel == ValueAssignmentEnum.Specific.ToString())

                    _sql = @"SELECT CV.Id AS [Value], CV.UserName AS [Text], CV.CharacteristicsId FROM [HKP].[Characteristics] C
                             LEFT JOIN hkp.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id
                             Where CV.MaterialMasterId='" + materialId + @"' AND CV.CharacteristicsId IN (SELECT MMC.CharacteristicsId  FROM [MST].[MaterialMasterCharacteristics] MMC  Where MaterialMasterId='" + materialId + @"') 
                             AND  C.ValueAssignmentLevel='" + assignmentLevel + @"' Order by CV.UserName";
                else
                    _sql = @"SELECT CV.Id AS [Value], CV.UserName AS [Text], CV.CharacteristicsId
                    FROM [MST].[MaterialMasterCharacteristics] AS MMC
                    JOIN [HKP].[Characteristics] C ON MMC.CharacteristicsId=C.Id
                    JOIN [HKP].[CharacteristicsValue] AS CV ON CV.CharacteristicsId = MMC.CharacteristicsId AND CV.CharacteristicsId = C.Id
                    WHERE MMC.MaterialMasterId = '" + materialId + "' AND CV.SourceType=C.ValueAssignmentLevel";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetTaxCategoryList(string companyGroupId, string masterOrderId, string plantId, string hsnCodeId, string specialTaxId, string PODate)
        {
            try
            {
                if (string.IsNullOrEmpty(specialTaxId) || string.IsNullOrWhiteSpace(specialTaxId) || specialTaxId == "null")
                {
                    var sql = @"DECLARE @masterOrderId varchar(10)='" + masterOrderId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                        SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                                JOIN [TRN].[MasterOrder] AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@masterOrderId)-- AND AD.Active=1 AND AD.Archive=0)
                        SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
						                        JOIN [TRN].[MasterOrder] AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@masterOrderId)-- AND AD.Active=1 AND AD.Archive=0)

                        SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                        SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                        SELECT TVD.Id AS TaxVariantDetailId, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage], 0) AS [Percentage], NULL TotalAmount
                        FROM [MST].[TaxVariantDetail] AS TVD
                        JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                        JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                        --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
                        LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
			                        FROM [MST].[HSNTaxPercentage] WHERE CountryId=@partyCountry AND HSNCodeId=@hsnCodeId AND convert(DATE, EffectiveDate)<='" + PODate + @"') AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                        LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                        WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                        AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticSales + @"'
				                           WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasSales + @"' END
                        AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                           WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                        OR TV.Different IS NULL)
                        ORDER BY TC.[Sequence]";
                    return _sqlRepository.GetDataCollection(sql);
                }
                else
                {
                    var sql = @"DECLARE @masterOrderId varchar(10)='" + masterOrderId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + specialTaxId + @"'
                        SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                                JOIN [TRN].[MasterOrder] AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@masterOrderId)-- AND AD.Active=1 AND AD.Archive=0)
                        SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
						                        JOIN [TRN].[MasterOrder] AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@masterOrderId)-- AND AD.Active=1 AND AD.Archive=0)

                        SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                        SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                        SELECT TVD.Id AS TaxVariantDetailId, TVD.TaxCategoryId, HP.SpecialTaxId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage], 0) AS [Percentage], NULL TotalAmount
                        FROM [MST].[TaxVariantDetail] AS TVD
                        JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                        JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                        --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE SpecialTaxId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
                        LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
			                        FROM [MST].[HSNTaxPercentage] WHERE CountryId=@partyCountry AND SpecialTaxId=@hsnCodeId) AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                        --LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                        LEFT JOIN HKP.SpecialTax AS HN ON HP.SpecialTaxId=HN.Id
                        WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                        AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticSales + @"'
				                           WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasSales + @"' END
                        AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                           WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                        OR TV.Different IS NULL)
                        ORDER BY TC.[Sequence]";
                    return _sqlRepository.GetDataCollection(sql);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetSalesOrderTaxCategoryList(string salesOrderId)
        {
            try
            {
                var sql = @"SELECT SOT.Id, SOT.SalesOrderId, SOT.TaxCategoryId, TC.UserName, SOT.HSNCodeId, HSN.Code AS HSNCode, SOT.[Percentage], SOT.TaxAmount
                            FROM [TRN].[SalesOrderTax] SOT
                            LEFT JOIN [MST].[TaxCategory] AS TC ON  SOT.TaxCategoryId = TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HSN ON SOT.HSNCodeId = HSN.Id
                            WHERE SOT.SalesOrderId='" + salesOrderId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel GetEmployeeListResponsible(GridParameter parameters, string companyId, string plantId, string partyAccountGroupId, string partyId)
        {
            try
            {
                //var sql = "";
                //var dataList = _customerDivisionRepository.Query(t => t.PlantId == plantId && t.PartyAccountGroupId == partyAccountGroupId).Select().ToList();
                //if (dataList.IsNotNull() && dataList.Count() == 1)
                //{
                //    if (dataList[0].PartyId == "-1")
                //        sql += " AND CD.PartyId='-1'";
                //    else
                //        sql += " AND CD.PartyId='" + partyId + "'";
                //}
                //else
                //    sql += " AND CD.PartyId='" + partyId + "'";

                parameters.CmdText = @"SELECT EI.SystemId, PR.Id AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [Designation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,PR.Code PCode
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            LEFT JOIN HKP.Designation AS DEG ON DEG.Id=EI.GivenDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=PR.DepartmentId
                            WHERE EI.CompanyId='" + companyId + "' AND EI.PlantId='" + plantId + "' AND EI.EmployeeStatus='Active'";
                //WHERE EI.SystemId IN(SELECT CDP.OurRespnsiblePersonId FROM [MST].[CustomerDivisionResPerson] AS CDP
                //     JOIN [MST].[CustomerDivision] AS CD ON CDP.CustomerDivisionId=CD.Id
                //     WHERE CD.PlantId='" + plantId + "' AND CD.PartyAccountGroupId='" + partyAccountGroupId + "'" + sql + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetPreparedEmployeeList(GridParameter parameters, string plantId, string employeeId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.SystemId, P.Id AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [Designation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionId
                            LEFT JOIN HKP.Designation AS DEG ON DEG.Id=EI.GivenDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            WHERE EI.SystemId<>'" + employeeId + "' AND EI.PlantId='" + plantId + "' AND EI.EmployeeStatus='Active'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public Dictionary<string, object> GetTaskTemplateMasterId(string buyerId, string buyerDivisionId, string buyerDepartmentId,string entityId)
        {
            try
            {
                var sql = "SELECT BM.TaskTemplateMasterId FROM mst.BuyerMaster AS  BM WHERE bm.BuyerId='" + buyerId + "' AND isnull(bm.BuyerDepartmentId,'" + buyerDepartmentId + "')='" + buyerDepartmentId + "' AND isnull(bm.BuyerDivisionId,'" + buyerDivisionId + "')='" + buyerDivisionId + "' AND Id IN(select distinct BuyerMasterId from [dbo].[BuyerMasterEntity] Where EntityId='"+ entityId + "')";
                return _sqlRepository.GetData(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(MasterOrder), out sID);
            return sID;
        }

        public void InsertOrUpdate(MasterOrder entity)
        {
            try
            {

                CheckUnique(entity);

                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPK();

                    entity.MasterOrderNo = entity.Id;
                    if (entity.BuyerDepartmentId == "ALL")
                    {
                        entity.BuyerDepartmentId = null;
                    }
                    if (entity.BuyerDivisionId == "ALL")
                    {
                        entity.BuyerDivisionId = null;
                    }
                    base.Insert(entity);
                }
                else
                {
                    if (entity.BuyerDepartmentId == "ALL")
                    {
                        entity.BuyerDepartmentId = null;
                    }
                    if (entity.BuyerDivisionId == "ALL")
                    {
                        entity.BuyerDivisionId = null;
                    }
                    base.Update(entity);
                }


            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }



        public void Insert(MasterOrder entity, List<MasterOrderTNA> taskList, UserRemarksControl userRemarksControl)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                CheckUnique(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (!string.IsNullOrEmpty(entity.BuyerId))
                {
                    var taskTemplateMasterId = GetTaskTemplateMasterId(entity.BuyerId, entity.BuyerDivisionId, entity.BuyerDepartmentId,entity.EntityId);
                    if (taskTemplateMasterId.Count > 0)
                    {
                        entity.TaskTemplateMasterId = taskTemplateMasterId["TaskTemplateMasterId"].ToString();
                    }

                }
                entity.Id = GetPK();
                //if (!string.IsNullOrEmpty(entity.EntityId))
                //{
                //    entity.MasterOrderNo = GetEntityPrefix(entity.EntityId);
                //}
                //else
                entity.MasterOrderNo = entity.Id;
                if (entity.BuyerDepartmentId == "ALL")
                {
                    entity.BuyerDepartmentId = null;
                }
                if (entity.BuyerDivisionId == "ALL")
                {
                    entity.BuyerDivisionId = null;
                }
                base.Insert(entity);


                if (userRemarksControl.RemarkControlId != null)
                {
                    userRemarksControl.Id = entity.Id;
                    userRemarksControl.MasterOrderId = entity.Id;
                    userRemarksControl.AddedBy = entity.AddedBy;
                    userRemarksControl.AddedFromIP = entity.AddedFromIP;
                    userRemarksControl.AddedDate = entity.AddedDate;
                    AuditService.AddedLog(userRemarksControl);
                    userRemarksControl.ModelState = ModelState.Added;
                    _UserRemarksControlRepository.Insert(userRemarksControl);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                if (taskList != null)
                {
                    SaveData(taskList, entity.Id);
                }

                TaskScheduler.TaskScheduler schedule = new TaskScheduler.TaskScheduler(_sqlRepository);
                schedule.CopyTaskTemplate(entity.Id);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetTLPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "MasterOrderTNA", out idFromDB);
            systemID = "MOT" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        private void SaveData(List<MasterOrderTNA> data, string moId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                foreach (var item in data)
                {
                    string sql = "SELECT * FROM [dbo].[MasterOrderTNA] WHERE Id='" + item.Id + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        dr["Id"] = GetTLPK();
                        dr["MasterOrderId"] = moId;
                        dr["TaskMasterId"] = item.TaskMasterId;
                        dr["IsRequired"] = item.IsRequired;
                        // dr["SequentialDate"] = item.SequentialDate;
                        // dr["TaskDependentDate"] = item.TaskDependentDate;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        //edit
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["MasterOrderId"] = moId;
                        dr["TaskMasterId"] = item.TaskMasterId;
                        dr["IsRequired"] = item.IsRequired;
                        // dr["SequentialDate"] = item.SequentialDate;
                        // dr["TaskDependentDate"] = item.TaskDependentDate;

                        dr["UpdatedBy"] = identity.Name;
                        dr["DateUpdated"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
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

        private void SaveMasterOrderItemData(List<MasterOrderItem> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var count = _itemRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[MasterOrderItem] WHERE MasterOrderId='{masterId}'").First();
                foreach (var item in data)
                {
                    string sql = "SELECT * FROM TRN.MasterOrderItem WHERE Id='" + item.Id + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        dr["Id"] = MakePK(item.MasterOrderId, count, 2);
                        dr["MasterOrderId"] = item.MasterOrderId;
                        dr["MaterialMasterId"] = item.MaterialMasterId;
                        dr["ArticleId"] = item.ArticleId;
                        dr["InquiryItemId"] = item.InquiryItemId;
                        dr["SampleItemId"] = item.SampleItemId;
                        dr["BuyerReferenceNo"] = item.BuyerReferenceNo;
                        dr["OwnReferenceNo"] = item.OwnReferenceNo;
                        dr["TotalQty"] = item.TotalQty;
                        dr["OrderWastagePercentage"] = item.OrderWastagePercentage;
                        dr["ExtraOrderPercentage"] = item.ExtraOrderPercentage;
                        dr["TestingStandardId"] = item.TestingStandardId;
                        dr["Type"] = item.Type;
                        dr["ProductionGrouping"] = item.ProductionGrouping;
                        dr["CSPt"] = item.CSPT;
                        dr["IsRepeat"] = item.IsRepeat;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = item.AddedDate;
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dsMaster.Tables[0].Rows.Add(dr);
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

        private void CheckUnique(MasterOrder entity)
        {
            var data = base.Query(t => t.Id != entity.Id && t.EntityId == entity.EntityId && t.MasterOrderNo == entity.MasterOrderNo).Select().FirstOrDefault();
            if (data != null) throw new CustomException("This masterorderno already exist.");
        }

        public string RemoveSpace(string oldStr)
        {
            string newStr = string.Empty;
            if (!string.IsNullOrEmpty(oldStr) || !string.IsNullOrWhiteSpace(oldStr))
            {
                newStr = Regex.Replace(oldStr, " {2,}", " ");
            }
            return newStr;
        }

        public DataTable GetUsedData(string id)
        {
            try
            {
                string sql = @"SELECT SM.Id FROM  TRN.SalesMaterial SM 
JOIN TRN.SalesOrder SO ON SO.Id=SM.SalesOrderId
JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
WHERE MOI.MasterOrderId='" + id + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveUserRemarksControl(MasterOrder entity, Dictionary<string, object> data)
        {
            DataSet dsMaster;
            string TableName = "TRN.UserRemarksControl";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where MasterOrderId='" + entity.Id + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("UserRemarksControl", out _Id);

                    data["Id"] = _Id;
                    data["MasterOrderId"] = entity.Id;
                    AddNewRow(dsMaster.Tables[0], data, entity);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data, entity);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData, MasterOrder entity)
        {

            DataRow dr = dt.NewRow();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
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
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData, MasterOrder entity)
        {
            dr.BeginEdit();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
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


        public void Update(MasterOrder entity, string masterId, IEnumerable<MasterOrderResPerson> personList, IEnumerable<MasterOrderItem> itemList, UserRemarksControl userRemarksControl)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            var flag = false;
            try
            {
                var dbmo = Find(entity.Id);

                string _sql = @"select PO.Id from trn.ProductionOrder PO
LEFT JOIn trn.ProductionOrderDetail D ON D.ProductionOrderId=PO.Id
LEFT JOIN TRN.SalesOrder SO ON SO.id=D.SalesOrderId
LEFT JOIN TRN.MasterOrderItem I ON I.Id=SO.MasterOrderItemId
LEFT JOIN TRN.MasterOrder MO ON MO.Id=I.MasterOrderId
Where MO.Id='"+entity.Id+"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(_sql, out dsMaster, false, "1");

                if (dbmo.CompanyId!=entity.CompanyId && dbmo.PlantId!=entity.PlantId && dsMaster.Tables[0].Rows.Count>0)
                {
                    throw new Exception("Company and Plant change not possible as Production Order is created.");
                }

                if (dbmo.PlantId != entity.PlantId && dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Plant change not possible as Production Order is created.");
                }

                var personDbDataList = _personRepository.Query(t => t.MasterOrderId == masterId).Select().ToList();
                var itemDbDataList = _itemRepository.Query(t => t.MasterOrderId == masterId).Select().ToList();

                if (!string.IsNullOrEmpty(entity.BuyerId))
                {
                    var taskTemplateMasterId = GetTaskTemplateMasterId(entity.BuyerId, entity.BuyerDivisionId, entity.BuyerDepartmentId,entity.EntityId);
                    if (taskTemplateMasterId.Count > 0)
                    {
                        entity.TaskTemplateMasterId = taskTemplateMasterId["TaskTemplateMasterId"].ToString();
                    }

                }

                _unitOfWork.BeginTransaction();
                flag = true;

                if (itemList.IsNotNull() && entity.TotalQty < itemList.Select(t => t.TotalQty).Sum())
                    throw new CustomException("Sum of item quantity can't be greater than " + entity.TotalQty);
                if (entity.BuyerDepartmentId == "ALL")
                {
                    entity.BuyerDepartmentId = null;
                }
                if (entity.BuyerDivisionId == "ALL")
                {
                    entity.BuyerDivisionId = null;
                }
                base.Update(entity);

                if (personList.IsNotNull() && personList.Count() > 0)
                {
                    var count = _personRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[MasterOrderResPerson] WHERE MasterOrderId='{masterId}'").First();
                    foreach (var item in personList)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            count++;
                            item.Id = MakePK(masterId, count, 2);
                            item.MasterOrderId = masterId;
                            AuditService.AddedLog(item);
                            _personRepository.Insert(item);
                        }
                        else
                        {
                            AuditService.UpdatedLog(item);
                            _personRepository.Update(item);
                        }
                    }
                }
                if (itemList.IsNotNull())
                {
                    var itemIds = itemList.Select(t => t.Id).ToArray();
                    var salesOrderDbList = _salesOrderRepository.Query(t => itemIds.Contains(t.MasterOrderItemId)).Select().ToList();
                    var salesOrderIds = salesOrderDbList.Select(a => a.Id).ToArray();

                    var firstCharDbList = _firstCharacteristicsRepository.Query(t => salesOrderIds.Contains(t.SalesOrderId)).Select().ToList();
                    var secondCharDbList = _secondCharacteristicsRepository.Query(t => salesOrderIds.Contains(t.SalesOrderId)).Select().ToList();
                    var thirdCharDbList = _thirdCharacteristicsRepository.Query(t => salesOrderIds.Contains(t.SalesOrderId)).Select().ToList();
                    var SOCostingConfirmationDbList = _SOCostingConfirmationRepository.Query(t => salesOrderIds.Contains(t.SalesOrderId)).Select().ToList();

                    //var count = _itemRepository.SqlQuery<int>($"SELECT count(Id)Id FROM [TRN].[MasterOrderItem] WHERE MasterOrderId='{masterId}'").First();
                    var count = _itemRepository.SqlQuery<int>($"SELECT CAST((RIGHT(ISNULL(MAX(CAST(Id AS INT)), 0),2)) AS INT) Id FROM [TRN].[MasterOrderItem] WHERE MasterOrderId='{masterId}'").First();
                    foreach (var item in itemList)
                    {
                        if (item.TotalQty == 0) throw new CustomException("Add Qty");
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            count++;
                            item.ProductionGrouping = RemoveSpace(item.ProductionGrouping);
                            item.Id = MakePK(masterId, count, 2);
                            item.MasterOrderId = masterId;
                            AuditService.AddedLog(item);
                            _itemRepository.Insert(item);
                        }
                        else
                        {
                            if (!_itemRepository.Any(t => t.Id == item.Id && t.MaterialMasterId == item.MaterialMasterId))
                            {
                                var attributeList = _itemAttributeValueRepository.Query(t => t.MasterOrderItemId == item.Id).Select().ToList();
                                foreach (var attr in attributeList)
                                {
                                    _itemAttributeValueRepository.Delete(attr);
                                }

                                var soList = salesOrderDbList.Where(t => t.MasterOrderItemId == item.Id).ToList();
                                foreach (var so in soList)
                                {
                                    var firstList = firstCharDbList.Where(t => t.SalesOrderId == so.Id).ToList();
                                    var secondList = secondCharDbList.Where(t => t.SalesOrderId == so.Id).ToList();
                                    var thirdList = thirdCharDbList.Where(t => t.SalesOrderId == so.Id).ToList();
                                    var SOCostingList = SOCostingConfirmationDbList.Where(t => t.SalesOrderId == so.Id).ToList();
                                    foreach (var third in thirdList)
                                    {
                                        _thirdCharacteristicsRepository.Delete(third);
                                    }
                                    foreach (var second in secondList)
                                    {
                                        _secondCharacteristicsRepository.Delete(second);
                                    }
                                    foreach (var first in firstList)
                                    {
                                        _firstCharacteristicsRepository.Delete(first);
                                    }
                                    foreach (var costing in SOCostingList)
                                    {
                                        _SOCostingConfirmationRepository.Delete(costing);
                                    }
                                    _salesOrderRepository.Delete(so);
                                }

                            }
                            item.ProductionGrouping = RemoveSpace(item.ProductionGrouping);
                            AuditService.UpdatedLog(item);
                            _itemRepository.Update(item);
                        }
                    }
                }

                if (personDbDataList.IsNotNull() && personDbDataList.Count > 0)
                {
                    if (personList == null)
                    {
                        foreach (var item in personDbDataList)
                        {
                            _personRepository.Delete(item);
                        }
                    }
                    else
                    {
                        foreach (var item in personDbDataList)
                        {
                            if (!personList.Any(t => t.Id == item.Id))
                            {
                                _personRepository.Delete(item);
                            }
                        }
                    }
                }

                if (itemDbDataList.IsNotNull() && itemDbDataList.Count > 0)
                {
                    if (itemList == null)
                    {
                        foreach (var item in itemDbDataList)
                        {
                            var attributeList = _itemAttributeValueRepository.Query(t => t.MasterOrderItemId == item.Id).Select().ToList();
                            foreach (var attr in attributeList)
                            {
                                _itemAttributeValueRepository.Delete(attr);
                            }

                            var MOICostingRateDbDataList = _MasterOrderItemCostingRateRepository.Query(t => t.MasterOrderItemId == item.Id).Select().ToList();
                            foreach (var itemrate in MOICostingRateDbDataList)
                            {
                                _MasterOrderItemCostingRateRepository.Delete(itemrate);
                            }
                            _itemRepository.Delete(item);
                            DeleteMOIDocumntFromFolder(item.Id);
                            DeleteArticleAlias(item.Id);
                        }
                    }
                    else
                    {
                        var itemIds = itemList.Select(t => t.Id).ToArray();
                        var salesOrderDbList = _salesOrderRepository.Query(t => itemIds.Contains(t.MasterOrderItemId)).Select().ToList();
                        var salesOrderIds = salesOrderDbList.Select(a => a.Id).ToArray();

                        var firstCharDbList = _firstCharacteristicsRepository.Query(t => salesOrderIds.Contains(t.SalesOrderId)).Select().ToList();
                        var secondCharDbList = _secondCharacteristicsRepository.Query(t => salesOrderIds.Contains(t.SalesOrderId)).Select().ToList();
                        var thirdCharDbList = _thirdCharacteristicsRepository.Query(t => salesOrderIds.Contains(t.SalesOrderId)).Select().ToList();
                        var SOCostingConfirmationDbList = _SOCostingConfirmationRepository.Query(t => salesOrderIds.Contains(t.SalesOrderId)).Select().ToList();
                        foreach (var item in itemDbDataList)
                        {
                            if (!itemList.Any(t => t.Id == item.Id))
                            {
                                var soList = _salesOrderRepository.Query(t => t.MasterOrderItemId == item.Id).Select().ToList();
                                foreach (var so in soList)
                                {
                                    var firstList = firstCharDbList.Where(t => t.SalesOrderId == so.Id).ToList();
                                    var secondList = secondCharDbList.Where(t => t.SalesOrderId == so.Id).ToList();
                                    var thirdList = thirdCharDbList.Where(t => t.SalesOrderId == so.Id).ToList();
                                    var SOCostingList = SOCostingConfirmationDbList.Where(t => t.SalesOrderId == so.Id).ToList();
                                    foreach (var third in thirdList)
                                    {
                                        _thirdCharacteristicsRepository.Delete(third);
                                    }
                                    foreach (var second in secondList)
                                    {
                                        _secondCharacteristicsRepository.Delete(second);
                                    }
                                    foreach (var first in firstList)
                                    {
                                        _firstCharacteristicsRepository.Delete(first);
                                    }
                                    foreach (var costing in SOCostingList)
                                    {
                                        _SOCostingConfirmationRepository.Delete(costing);
                                    }
                                    _salesOrderRepository.Delete(so);
                                }

                                //var childCustomerPo = _customerPORepository.Query(r => r.MasterOrderId == item.Id).Select().FirstOrDefault();

                                var attributeList = _itemAttributeValueRepository.Query(t => t.MasterOrderItemId == item.Id).Select().ToList();
                                foreach (var attr in attributeList)
                                {
                                    _itemAttributeValueRepository.Delete(attr);
                                }

                                var MOICostingRateDbDataList = _MasterOrderItemCostingRateRepository.Query(t => t.MasterOrderItemId == item.Id).Select().ToList();
                                foreach (var itemrate in MOICostingRateDbDataList)
                                {
                                    _MasterOrderItemCostingRateRepository.Delete(itemrate);
                                }

                                _itemRepository.Delete(item);
                                DeleteMOIDocumntFromFolder(item.Id);
                                DeleteArticleAlias(item.Id);
                            }
                        }
                    }
                }

                //if (UserRemarksControl["RemarkControlId"] != null)
                //{
                //   SaveUserRemarksControl(entity, UserRemarksControl);
                //}

                if (userRemarksControl.RemarkControlId != null)
                {
                    var data = _UserRemarksControlRepository.Find(userRemarksControl.MasterOrderId);
                    if (data == null)
                    {
                        userRemarksControl.Id = entity.Id;
                        userRemarksControl.MasterOrderId = entity.Id;
                        userRemarksControl.AddedBy = entity.AddedBy;
                        userRemarksControl.AddedFromIP = entity.AddedFromIP;
                        userRemarksControl.AddedDate = entity.AddedDate;
                        userRemarksControl.ModelState = ModelState.Added;
                        AuditService.AddedLog(userRemarksControl);
                        _UserRemarksControlRepository.Insert(userRemarksControl);
                    }
                    else
                    {

                        data.RemarkControlId = userRemarksControl.RemarkControlId;
                        data.UserRemarks = userRemarksControl.UserRemarks;
                        data.UpdatedBy = entity.UpdatedBy;
                        data.UpdatedFromIP = entity.UpdatedFromIP;
                        data.UpdatedDate = entity.UpdatedDate;
                        data.ModelState = ModelState.Modified;
                        AuditService.UpdatedLog(data);
                        _UserRemarksControlRepository.Update(data);
                    }

                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                TaskScheduler.TaskScheduler schedule = new TaskScheduler.TaskScheduler(_sqlRepository);
                schedule.CopyTaskTemplate(entity.Id);

                DataTable dtm = _sqlRepository.GetDataTable("SELECT * FROM trn.MasterOrder AS mo WHERE mo.Id='" + entity.Id + "'");

                //line item related tasks
                string sql = @"SELECT MOI.* FROM trn.MasterOrder AS mo 
                                INNER JOIN hkp.OrderStatus AS os ON os.Id=mo.OrderStatusId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                WHERE mo.id='" + entity.Id + "' and os.Id<>'" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"' AND ISNULL(mo.TaskTemplateMasterId,'')='" + dtm.Rows[0]["TaskTemplateMasterId"].ToString() + "'";

                DataTable dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {
                    try
                    {
                        DataTable dt = schedule.GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.Style);
                        if (dt.Rows.Count > 0)
                            schedule.MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.Style);
                    }
                    catch (Exception ex)
                    {

                    }
                }

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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteCostingRate(string itemId)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM dbo.MasterOrderItemCostingRate WHERE MasterOrderItemId='" + itemId + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteMOIDocumntFromFolder(string id)
        {
            var directory = ResourcesPathReader.GetMOIDocumentPath();
            var path = Path.Combine(directory);
            ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
            string sql = "SELECT * FROM TRN.MasterOrderItem WHERE Id='" + id + "'";
            DataSet dsLocal = null;
            connection.BeginTransaction();
            connection.getDataSet(sql, out dsLocal);
            connection.CommitTransaction();
            var FN = dsLocal.Tables[0].Rows[0]["FileName"].ToString();

            if (System.IO.File.Exists(path + id + Path.GetExtension(FN)))
                System.IO.File.Delete(path + id + Path.GetExtension(FN));

        }

        public void DeleteArticleAlias(string id)
        {
            ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
            string sql = " delete FROM ArticleAlias WHERE MasterOrderItemId='" + id + "'";
            connection.BeginTransaction();
            connection.executeQuery(sql);
            connection.CommitTransaction();

        }

        public void InsertOrUpdateGraph(string masterItemId, IEnumerable<MasterOrderAttributeValue> attributeValueList)
        {
            var flag = false;
            try
            {
                if (attributeValueList.IsNull()) return;
                _unitOfWork.BeginTransaction();
                flag = true;

                var dbList = _itemAttributeValueRepository.Query(t => t.MasterOrderItemId == masterItemId).Select().AsEnumerable();

                var localList = attributeValueList.ToList();
                var count = _personRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[MasterOrderAttributeValue] WHERE MasterOrderItemId='{masterItemId}'").First();

                //foreach (var del in localList)
                //{
                //    if (string.IsNullOrEmpty(del.Id) && string.IsNullOrEmpty(del.AttributeValueId) && string.IsNullOrEmpty(del.ValueFreeText)
                //        && string.IsNullOrEmpty(del.ValueRemarks))
                //        attributeValueList.ToList().Remove(del);
                //}

                foreach (var item in attributeValueList)
                {
                    if (string.IsNullOrEmpty(item.Id))//Insert
                    {
                        if (string.IsNullOrEmpty(item.AttributeValueId) && string.IsNullOrEmpty(item.ValueFreeText) && string.IsNullOrEmpty(item.ValueRemarks))
                        {
                            //Do Nothing.
                        }
                        else
                        {
                            count++;
                            item.Id = MakePK(masterItemId, count, 2);
                            SetAttributeValueId(item);
                            item.MasterOrderItemId = masterItemId;
                            AuditService.AddedLog(item);
                            _itemAttributeValueRepository.Insert(item);
                        }
                    }
                    else
                    {
                        //Edit
                        if (string.IsNullOrEmpty(item.AttributeValueId) && string.IsNullOrEmpty(item.ValueFreeText) && string.IsNullOrEmpty(item.ValueRemarks))
                        {
                            _itemAttributeValueRepository.Delete(item);
                        }
                        else
                        {
                            SetAttributeValueId(item);
                            AuditService.UpdatedLog(item);
                            _itemAttributeValueRepository.Update(item);
                        }
                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private static void SetAttributeValueId(MasterOrderAttributeValue item)
        {
            if (item.AttributeValueId != null)//
                item.ValueFreeText = null;
            else
            {
                //if (item.ValueFreeText == null)
                //    throw new CustomException("Free Text can not be null");
            }
        }


        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                var useddata = GetUsedData(id);
                if (useddata.Rows.Count > 0)
                {
                    throw new Exception("Delete is not allowed after creation of Invoice.");
                }
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                objCon.ExecuteNonQueryWrapper(@"DELETE FROM MasterOrderExchangeRates WHERE TransactionId='" + id + @"'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM TnALog WHERE MasterOrderId='" + id + @"'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM trn.ThirdCharacteristics WHERE SalesOrderId IN (SELECT Id FROM trn.SalesOrder WHERE MasterOrderItemId IN (SELECT Id FROM trn.MasterOrderItem WHERE MasterOrderId='" + id + @"'))", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM trn.SecondCharacteristics  WHERE SalesOrderId IN (SELECT Id FROM trn.SalesOrder WHERE MasterOrderItemId IN (SELECT Id FROM trn.MasterOrderItem WHERE MasterOrderId='" + id + @"'))", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM trn.FirstCharacteristics WHERE SalesOrderId IN (SELECT Id FROM trn.SalesOrder WHERE MasterOrderItemId IN (SELECT Id FROM trn.MasterOrderItem WHERE MasterOrderId='" + id + @"'))", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM trn.SalesOrder WHERE MasterOrderItemId IN (SELECT Id FROM trn.MasterOrderItem WHERE MasterOrderId='" + id + @"')", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM dbo.MasterOrderItemCostingRate WHERE MasterOrderItemId IN (SELECT Id FROM trn.MasterOrderItem WHERE MasterOrderId='" + id + @"')", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM trn.MasterOrderItem WHERE MasterOrderId='" + id + @"'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM MasterOrderTaskTemplateDependency Where PreTaskTemplateId IN(select Id from dbo.MasterOrderTaskTemplate Where MasterOrderId='" + id + @"')", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM MasterOrderTaskTemplateSubTasks Where MasterOrderTaskTemplateId IN(select Id from dbo.MasterOrderTaskTemplate Where MasterOrderId='" + id + @"')", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM dbo.TaskAudit WHERE TaskManagerMasterId IN(select Id from dbo.TaskManagerMaster Where TNATasksId IN (select Id from dbo.TNATasks where TaskTemplateId IN(select Id from dbo.MasterOrderTaskTemplate Where MasterOrderId='" + id + @"')))", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM dbo.TaskManagerSubTasks WHERE TaskManagerMasterId IN(select Id from dbo.TaskManagerMaster Where TNATasksId IN (select Id from dbo.TNATasks where TaskTemplateId IN(select Id from dbo.MasterOrderTaskTemplate Where MasterOrderId='" + id + @"')))", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM dbo.TaskManagerMaster Where TNATasksId IN (select Id from dbo.TNATasks where TaskTemplateId IN(select Id from dbo.MasterOrderTaskTemplate Where MasterOrderId='" + id + @"'))", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM TNATasks Where TaskTemplateId IN(select Id from dbo.MasterOrderTaskTemplate Where MasterOrderId='" + id + @"')", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM dbo.MasterOrderTaskTemplate WHERE MasterOrderId='" + id + @"'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM dbo.TNAMaster WHERE MasterOrderId='" + id + @"'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM TRN.UserRemarksControl  WHERE MasterOrderId='" + id + @"'", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM trn.MasterOrder WHERE Id='" + id + @"'", true, "1");

                objCon.CommitTransaction();

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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private bool CheckUnique(string CommitmentDate, string deliveryDate, string destinationId, string shipmentModeId)
        {
            try
            {
                var _sql = @"SELECT * FROM [TRN].[SalesOrder] Where CommitmentDate='" + CommitmentDate + @"' AND DeliveryDate='" + deliveryDate + @"' AND DestinationId='" + destinationId + @"' AND ShipmentModeId='" + shipmentModeId + "'";
                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool CheckUnique(string Id, string CommitmentDate, string deliveryDate, string destinationId, string shipmentModeId, string destinationDescription)
        {
            try
            {
                var _sql = @"SELECT * FROM [TRN].[SalesOrder] Where CommitmentDate='" + CommitmentDate + @"' AND DeliveryDate='" + deliveryDate + @"' AND DestinationId='" + destinationId + @"' AND DestinationDescription='" + destinationDescription + @"' AND ShipmentModeId='" + shipmentModeId + @"' AND Id <>'" + Id + "'";
                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertOrUpdateSOGraph(string masterItemId, SalesOrderMaster salesOrderMaster, MasterOrder masterorder)
        {
            try
            {
                CheckUnique(salesOrderMaster);
                var itemQty = _itemRepository.Query(t => t.Id == salesOrderMaster.MasterOrderItemId).Select(t => t.TotalQty).FirstOrDefault();
                var soTotalQty = _salesOrderRepository.Query(t => t.Id != salesOrderMaster.Id && t.MasterOrderItemId == salesOrderMaster.MasterOrderItemId).Select(t => t.Qty).Sum() + salesOrderMaster.Qty;

                if (soTotalQty > itemQty)
                    throw new CustomException("Sum of sales order quantity can't be greater than " + itemQty);

                if (string.IsNullOrEmpty(salesOrderMaster.Id))
                {

                    var count = _salesOrderRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[SalesOrder] WHERE MasterOrderItemId='{salesOrderMaster.MasterOrderItemId}'").First();
                    salesOrderMaster.IsFirstEntry = count == 0 ? true : false;
                    count++;
                    salesOrderMaster.Id = MakePK(salesOrderMaster.MasterOrderItemId, count, 2);
                    salesOrderMaster.MasterOrderItemId = masterItemId;
                    salesOrderMaster.OrderStatusId = null;
                    salesOrderMaster.CheckByStatus = "To Be Check";
                    AuditService.AddedLog(salesOrderMaster);
                    _salesOrderRepository.Insert(salesOrderMaster);

                }
                else
                {

                    AuditService.UpdatedLog(salesOrderMaster);

                    if (salesOrderMaster.OrderStatusId != OrderStatusEnum.Active.ToString())
                    {
                        salesOrderMaster.OrderStatusChangedBy = salesOrderMaster.UpdatedBy;
                        salesOrderMaster.OrderStatusChangedDate = salesOrderMaster.UpdatedDate;
                        salesOrderMaster.OrderStatusChangedFromIP = salesOrderMaster.UpdatedFromIP;
                    }
                    _salesOrderRepository.Update(salesOrderMaster);

                    var dbList = _salesOrderTaxRepository.Query(t => t.SalesOrderId == salesOrderMaster.Id).Select().AsEnumerable();
                    var SoTotalAmount = salesOrderMaster.Qty * salesOrderMaster.Rate;
                    //var TaxAmount= SoTotalAmount 

                }
                _unitOfWork.SaveChanges();

                TaskScheduler.TaskScheduler schedule = new TaskScheduler.TaskScheduler(_sqlRepository);
                schedule.UpdateTaskStatus();
                //Sales Order Related Tasks
                string sql = @"SELECT SO.* FROM trn.MasterOrder AS mo 
                                INNER JOIN hkp.OrderStatus AS os ON os.Id=mo.OrderStatusId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                           WHERE so.id='" + salesOrderMaster.Id + "' and os.Id<>'" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"' AND ISNULL(mo.TaskTemplateMasterId,'')='" + masterorder.TaskTemplateMasterId + "'";

                DataTable dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {

                    try
                    {
                        DataTable dt = schedule.GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.SalesOrder);
                        if (dt.Rows.Count > 0)
                            schedule.MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.SalesOrder);
                    }
                    catch (Exception ex)
                    {

                    }
                }

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

        private void CheckSplitSalesOrderUnique(SalesOrderMaster entity)
        {
            var data = _salesOrderRepository.Query(t => t.Id != entity.Id && t.CustomerPOId == entity.CustomerPOId && t.DeliveryDate == entity.DeliveryDate && t.DestinationId == entity.DestinationId
            && t.CommitmentDate == entity.CommitmentDate && t.ShipmentModeId == entity.ShipmentModeId).Select().FirstOrDefault();
            if (data != null) throw new CustomException("Same combination already exists.");
        }

        public void InsertOrUpdateSplitSOGraph(string masterItemId, SalesOrderMaster salesOrderMaster)
        {
            try
            {
                //CheckSplitSalesOrderUnique(salesOrderMaster);
                //var itemQty = _itemRepository.Query(t => t.Id == salesOrderMaster.MasterOrderItemId).Select(t => t.TotalQty).FirstOrDefault();
                //var soTotalQty = _salesOrderRepository.Query(t => t.Id != salesOrderMaster.Id && t.MasterOrderItemId == salesOrderMaster.MasterOrderItemId).Select(t => t.Qty).Sum() + salesOrderMaster.Qty;


                //if (soTotalQty > itemQty)
                //    throw new CustomException("Sum of sales order quantity can't be greater than " + itemQty);
                var so = _salesOrderRepository.Find(salesOrderMaster.ParentId);
                if (so != null)
                {
                    var soQty = so.Qty - salesOrderMaster.Qty;
                    so.Qty = soQty;
                    AuditService.UpdatedLog(so);
                    _salesOrderRepository.Update(so);
                }

                if (string.IsNullOrEmpty(salesOrderMaster.Id))
                {

                    var count = _salesOrderRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[SalesOrder] WHERE MasterOrderItemId='{salesOrderMaster.MasterOrderItemId}'").First();
                    salesOrderMaster.IsFirstEntry = count == 0 ? true : false;
                    count++;
                    salesOrderMaster.Id = MakePK(salesOrderMaster.MasterOrderItemId, count, 2);
                    salesOrderMaster.MasterOrderItemId = masterItemId;
                    AuditService.AddedLog(salesOrderMaster);
                    _salesOrderRepository.Insert(salesOrderMaster);

                }
                else
                {

                    AuditService.UpdatedLog(salesOrderMaster);
                    _salesOrderRepository.Update(salesOrderMaster);

                    var dbList = _salesOrderTaxRepository.Query(t => t.SalesOrderId == salesOrderMaster.Id).Select().AsEnumerable();
                    var SoTotalAmount = salesOrderMaster.Qty * salesOrderMaster.Rate;
                    //var TaxAmount= SoTotalAmount 

                }
                _unitOfWork.SaveChanges();


            }

            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void CheckUnique(SalesOrderMaster entity)
        {
            var data = _salesOrderRepository.Query(t => t.Id != entity.Id && t.CustomerPOId == entity.CustomerPOId && t.MasterOrderItemId == entity.MasterOrderItemId && t.DeliveryDate == entity.DeliveryDate && t.DestinationId == entity.DestinationId
            && t.CommitmentDate == entity.CommitmentDate && t.ShipmentModeId == entity.ShipmentModeId && t.Description == entity.Description && t.DestinationDescription == entity.DestinationDescription && t.ParentId == null).Select().FirstOrDefault();
            if (data != null) throw new CustomException("Same combination already exists.");

            //if (_salesOrderRepository.Any(t => t.Id != entity.Id && t.DeliveryDate == entity.DeliveryDate && t.DestinationId== entity.DestinationId
            //&& t.CommitmentDate == entity.CommitmentDate && t.ShipmentModeId == entity.ShipmentModeId))
            //    throw new CustomException("Same combination already exists.");

        }

        public void UpdateSOGraph(string masterItemId, SalesOrderMaster salesOrderMaster, IEnumerable<SalesOrderTax> taxCategoryList, MasterOrder masterorder)
        {
            try
            {
                if (salesOrderMaster.ParentId == null || string.IsNullOrEmpty(salesOrderMaster.ParentId))
                {
                    CheckUnique(salesOrderMaster);
                }
                var itemQty = _itemRepository.Query(t => t.Id == salesOrderMaster.MasterOrderItemId).Select(t => t.TotalQty).FirstOrDefault();
                var soTotalQty = _salesOrderRepository.Query(t => t.Id != salesOrderMaster.Id && t.MasterOrderItemId == salesOrderMaster.MasterOrderItemId).Select(t => t.Qty).Sum() + salesOrderMaster.Qty;



                if (soTotalQty > itemQty)
                    throw new CustomException("Sum of sales order quantity can't be greater than " + itemQty);


                if (string.IsNullOrEmpty(salesOrderMaster.Id))
                {

                    var count = _salesOrderRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[SalesOrder] WHERE MasterOrderItemId='{salesOrderMaster.MasterOrderItemId}'").First();
                    salesOrderMaster.IsFirstEntry = count == 0 ? true : false;
                    count++;
                    salesOrderMaster.Id = MakePK(salesOrderMaster.MasterOrderItemId, count, 2);
                    salesOrderMaster.MasterOrderItemId = masterItemId;
                    AuditService.AddedLog(salesOrderMaster);
                    _salesOrderRepository.Insert(salesOrderMaster);

                }
                else
                {

                    AuditService.UpdatedLog(salesOrderMaster);
                    if (salesOrderMaster.OrderStatusId != OrderStatusEnum.Active.ToString())
                    {
                        salesOrderMaster.OrderStatusChangedBy = salesOrderMaster.UpdatedBy;
                        salesOrderMaster.OrderStatusChangedDate = salesOrderMaster.UpdatedDate;
                        salesOrderMaster.OrderStatusChangedFromIP = salesOrderMaster.UpdatedFromIP;
                    }
                    _salesOrderRepository.Update(salesOrderMaster);

                    var dbList = _salesOrderTaxRepository.Query(t => t.SalesOrderId == salesOrderMaster.Id).Select().AsEnumerable();
                    var SoTotalAmount = salesOrderMaster.Qty * salesOrderMaster.Rate;
                    //var TaxAmount= SoTotalAmount 

                }
                if (taxCategoryList != null)
                {
                    InsertOrUpdateSalesOrderTax(salesOrderMaster.Id, taxCategoryList);
                }
                _unitOfWork.SaveChanges();

                TaskScheduler.TaskScheduler schedule = new TaskScheduler.TaskScheduler(_sqlRepository);
                schedule.UpdateTaskStatus();
                //Sales Order Related Tasks
                string sql = @"SELECT SO.* FROM trn.MasterOrder AS mo 
                                INNER JOIN hkp.OrderStatus AS os ON os.Id=mo.OrderStatusId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                           WHERE so.id='" + salesOrderMaster.Id + "' and os.Id<>'" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"' AND ISNULL(mo.TaskTemplateMasterId,'')='" + masterorder.TaskTemplateMasterId + "'";

                DataTable dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {

                    try
                    {
                        DataTable dt = schedule.GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.SalesOrder);
                        if (dt.Rows.Count > 0)
                            schedule.MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.SalesOrder);
                    }
                    catch (Exception ex)
                    {

                    }
                }

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

        public void CheckSOGraph(SalesOrderMaster salesOrderMaster)
        {
            try
            {

                if (!string.IsNullOrEmpty(salesOrderMaster.Id))
                {
                    var sodata = _salesOrderRepository.Find(salesOrderMaster.Id);
                    AuditService.UpdatedLog(sodata);
                    sodata.CheckByStatus = salesOrderMaster.CheckByStatus;
                    sodata.ApproveBy = salesOrderMaster.ApproveBy;
                    if (sodata.CheckByStatus == "Checked")
                    {
                        sodata.ApprovedStatus = "To Be Approve";
                    }
                    if (sodata.CheckByStatus == "Reject")
                    {
                        sodata.OrderStatusId = "Cancelled";
                        sodata.ApproveBy = null;
                    }
                    sodata.CheckByRemark = salesOrderMaster.CheckByRemark;
                    sodata.CheckByDate = DateTime.Now;

                    _salesOrderRepository.Update(sodata);
                }

                _unitOfWork.SaveChanges();

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

        public DataSet GetMasterOrderStatus(string moItemId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"Select distinct MA.OrderStatusId from  TRN.SalesOrder S 
left  join TRN.MasterOrderItem M ON M.Id=S.MasterOrderItemId
left  join TRN.MasterOrder MA ON MA.Id=M.MasterOrderId
Where S.MasterOrderItemId='" + moItemId + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        public void ApproveSOGraph(MasterOrder entity, SalesOrderMaster salesOrderMaster)
        {
            try
            {
                var masterOrderStatus = "";
                var dsOrderStatus = GetMasterOrderStatus(salesOrderMaster.MasterOrderItemId);
                if (dsOrderStatus.Tables[0].Rows.Count > 0)
                {
                    masterOrderStatus = dsOrderStatus.Tables[0].Rows[0]["OrderStatusId"].ToString();
                }
                if (!string.IsNullOrEmpty(salesOrderMaster.Id))
                {
                    var sodata = _salesOrderRepository.Find(salesOrderMaster.Id);

                    AuditService.UpdatedLog(salesOrderMaster);
                    sodata.ApproveByDate = DateTime.Now;
                    sodata.OrderStatusId = masterOrderStatus;
                    sodata.ApprovedStatus = salesOrderMaster.ApprovedStatus;
                    if (salesOrderMaster.ApprovedStatus== "UnApprove")
                    {
                        sodata.CheckByStatus = "To Be Check";
                    }

                    sodata.ApproveByRemark = salesOrderMaster.ApproveByRemark;
                    _salesOrderRepository.Update(sodata);
                }

                _unitOfWork.SaveChanges();

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

        public void DeleteSOGraph(string masterItemId, SalesOrderMaster salesOrderMaster)
        {
            var flag = false;
            try
            {

                if (!string.IsNullOrEmpty(salesOrderMaster.Id))
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;

                    AuditService.UpdatedLog(salesOrderMaster);
                    _thirdCharacteristicsRepository.Delete(_thirdCharacteristicsRepository.Query(t => t.SalesOrderId == salesOrderMaster.Id).Select().AsEnumerable());
                    _secondCharacteristicsRepository.Delete(_secondCharacteristicsRepository.Query(t => t.SalesOrderId == salesOrderMaster.Id).Select().AsEnumerable());
                    _firstCharacteristicsRepository.Delete(_firstCharacteristicsRepository.Query(t => t.SalesOrderId == salesOrderMaster.Id).Select().AsEnumerable());
                    _SOCostingConfirmationRepository.Delete(_SOCostingConfirmationRepository.Query(t => t.SalesOrderId == salesOrderMaster.Id).Select().AsEnumerable());

                    _salesOrderTaxRepository.Delete(_salesOrderTaxRepository.Query(t => t.SalesOrderId == salesOrderMaster.Id).Select().AsEnumerable());

                    _unitOfWork.SaveChanges();

                    _salesOrderRepository.Delete(_salesOrderRepository.Query(t => t.Id == salesOrderMaster.Id).Select().AsEnumerable());

                    _unitOfWork.SaveChanges();

                    flag = false;
                    _unitOfWork.Commit();
                }
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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertOrUpdateCharacteristics(IEnumerable<SalesOrderCharacteristicsViewModel> entities, int listLength, string soId)
        {
            var flag = false;
            try
            {
                if (entities != null && !string.IsNullOrEmpty(soId))
                {
                    var viewModel = entities.ToList();
                    if (viewModel.IsNull() && viewModel.Count() == 0) throw new CustomException("can not null");

                    _unitOfWork.BeginTransaction();
                    flag = true;

                    var firstData = viewModel.Where(t => t.Flag == "1st").FirstOrDefault();
                    var firstChar = new FirstCharacteristics();
                    var salesOrderId = viewModel.FirstOrDefault().SalesOrderId;
                    var soTotalQty = _salesOrderRepository.Query(t => t.Id == salesOrderId).Select(t => t.Qty).FirstOrDefault();
                    var firstCharId = _firstCharacteristicsRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[FirstCharacteristics] WHERE SalesOrderId='" + salesOrderId + "'").First();


                    if (viewModel.IsNotNull() && viewModel.Count() > 0)
                    {
                        if (listLength == 1)
                        {
                            var firstTotalQtyEntered = viewModel.Select(t => t.Qty).Sum();

                            var firstSkuDBlist = _firstCharacteristicsRepository.Query(t => t.SalesOrderId == salesOrderId).Select().AsEnumerable();
                            foreach (var child in firstSkuDBlist)
                            {
                                if (viewModel.FirstOrDefault(a => a.Id == child.Id) == null)
                                {
                                    _firstCharacteristicsRepository.Delete(child);
                                }
                            }

                            foreach (var item in viewModel)
                            {
                                if (string.IsNullOrEmpty(item.Id))
                                {
                                    //var firstTotalQtyDB = _firstCharacteristicsRepository.Query(t => t.SalesOrderId == salesOrderId).Select(t => t.Qty).Sum();
                                    //if ((firstTotalQtyEntered + firstTotalQtyDB) > soTotalQty)
                                    //    throw new CustomException("Sum of SKU quantity can't be greater than " + soTotalQty);

                                    firstCharId++;
                                    var firstCharData = new FirstCharacteristics
                                    {
                                        Id = MakePK(viewModel.FirstOrDefault().SalesOrderId, firstCharId, 2),
                                        Sequence = item.Sequence,
                                        SalesOrderId = item.SalesOrderId,
                                        CharacteristicsId = item.CharacteristicsId,
                                        CharacteristicsValueId = item.CharacteristicsValueId,
                                        ValueFreeText = item.ValueFreeText,
                                        Qty = item.Qty
                                    };
                                    AuditService.AddedLog(firstCharData);
                                    _firstCharacteristicsRepository.Insert(firstCharData);
                                }
                                else
                                {
                                    //var firstTotalQtyDB = _firstCharacteristicsRepository.Query(t => t.SalesOrderId == salesOrderId && t.Id != item.Id).Select(t => t.Qty).Sum();
                                    //if ((firstTotalQtyEntered + firstTotalQtyDB) > soTotalQty)
                                    //    throw new CustomException("Sum of SKU quantity can't be greater than " + soTotalQty);

                                    var firstCharData = new FirstCharacteristics
                                    {
                                        Id = item.Id,
                                        Sequence = item.Sequence,
                                        SalesOrderId = item.SalesOrderId,
                                        CharacteristicsId = item.CharacteristicsId,
                                        CharacteristicsValueId = item.CharacteristicsValueId,
                                        ValueFreeText = item.ValueFreeText,
                                        Qty = item.Qty
                                    };
                                    AuditService.UpdatedLog(firstCharData);
                                    _firstCharacteristicsRepository.Update(firstCharData);
                                    //var count = _secondCharacteristicsRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[SecondCharacteristics] WHERE FirstCharacteristicsid='{firstCharData.Id}'").First();
                                    //var Sequence = _secondCharacteristicsRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Sequence, 2) AS INT)), 0) Id FROM [TRN].[SecondCharacteristics] WHERE FirstCharacteristicsid='{firstCharData.Id}'").First();

                                    //var skuDBlist = _secondCharacteristicsRepository.Query(t => t.FirstCharacteristicsId == firstCharData.Id).Select().AsEnumerable();
                                    //foreach (var child in skuDBlist)
                                    //{
                                    //    if (item.ChildList.FirstOrDefault(a => a.Id == child.Id) == null)
                                    //    {
                                    //        _secondCharacteristicsRepository.Delete(child);
                                    //    }
                                    //}


                                }
                            }
                        }
                        else
                        if (listLength == 2)
                        {
                            var firstTotalQty = viewModel.Select(t => t.Qty).Sum();
                            if (firstTotalQty > soTotalQty)
                                throw new CustomException("Sum of SKU quantity can't be greater than " + soTotalQty);

                            var skuChar1DBlist = _firstCharacteristicsRepository.Query(t => t.SalesOrderId == salesOrderId).Select().AsEnumerable();
                            foreach (var child in skuChar1DBlist)
                            {
                                if (viewModel.FirstOrDefault(a => a.Id == child.Id) == null)
                                {
                                    var firstCharData = new FirstCharacteristics
                                    {
                                        Id = child.Id
                                    };
                                    var skuChar2DBlist = _secondCharacteristicsRepository.Query(t => t.FirstCharacteristicsId == child.Id).Select().AsEnumerable();
                                    foreach (var third in skuChar2DBlist)
                                    {
                                        _secondCharacteristicsRepository.Delete(third);
                                    }
                                    _firstCharacteristicsRepository.Delete(_firstCharacteristicsRepository.Query(t => t.Id == child.Id).Select().AsEnumerable());
                                }
                            }

                            foreach (var item in viewModel)
                            {
                                if (item.Id.StartsWith("-"))
                                {
                                    firstCharId++;
                                    var firstCharData = new FirstCharacteristics
                                    {
                                        Id = MakePK(viewModel.FirstOrDefault().SalesOrderId, firstCharId, 2),
                                        Sequence = item.Sequence,
                                        SalesOrderId = item.SalesOrderId,
                                        CharacteristicsId = item.CharacteristicsId,
                                        CharacteristicsValueId = item.CharacteristicsValueId,
                                        ValueFreeText = item.ValueFreeText,
                                        Qty = item.Qty
                                    };
                                    AuditService.AddedLog(firstCharData);

                                    int count = 0;
                                    foreach (var child in item.ChildList)
                                    {
                                        count++;
                                        var secondCharData = new SecondCharacteristics
                                        {
                                            Id = MakePK(firstCharData.Id, count, 2),
                                            FirstCharacteristicsId = firstCharData.Id,
                                            Sequence = child.Sequence,
                                            SalesOrderId = child.SalesOrderId,
                                            CharacteristicsId = child.CharacteristicsId,
                                            CharacteristicsValueId = child.CharacteristicsValueId,
                                            ValueFreeText = child.ValueFreeText,
                                            Qty = child.Qty
                                        };
                                        AuditService.AddedLog(secondCharData);
                                        _secondCharacteristicsRepository.Insert(secondCharData);
                                    }
                                    _firstCharacteristicsRepository.Insert(firstCharData);
                                }
                                else
                                {
                                    var firstCharData = new FirstCharacteristics
                                    {
                                        Id = item.Id,
                                        Sequence = item.Sequence,
                                        SalesOrderId = item.SalesOrderId,
                                        CharacteristicsId = item.CharacteristicsId,
                                        CharacteristicsValueId = item.CharacteristicsValueId,
                                        ValueFreeText = item.ValueFreeText,
                                        Qty = item.Qty
                                    };
                                    AuditService.UpdatedLog(firstCharData);
                                    var count = _secondCharacteristicsRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[SecondCharacteristics] WHERE FirstCharacteristicsid='{firstCharData.Id}'").First();
                                    var Sequence = _secondCharacteristicsRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Sequence, 2) AS INT)), 0) Id FROM [TRN].[SecondCharacteristics] WHERE FirstCharacteristicsid='{firstCharData.Id}'").First();

                                    var skuDBlist = _secondCharacteristicsRepository.Query(t => t.FirstCharacteristicsId == firstCharData.Id).Select().AsEnumerable();
                                    foreach (var child in skuDBlist)
                                    {
                                        if (item.ChildList.FirstOrDefault(a => a.Id == child.Id) == null)
                                        {
                                            _secondCharacteristicsRepository.Delete(child);
                                        }
                                    }
                                    foreach (var child in item.ChildList)
                                    {
                                        if (string.IsNullOrEmpty(child.Id))
                                        {
                                            count++;
                                            Sequence++;
                                            var secondCharData = new SecondCharacteristics
                                            {
                                                Id = MakePK(firstCharData.Id, count, 2),
                                                FirstCharacteristicsId = firstCharData.Id,
                                                Sequence = count,
                                                SalesOrderId = child.SalesOrderId,
                                                CharacteristicsId = child.CharacteristicsId,
                                                CharacteristicsValueId = child.CharacteristicsValueId,
                                                ValueFreeText = child.ValueFreeText,
                                                Qty = child.Qty

                                            };
                                            AuditService.AddedLog(secondCharData);
                                            _secondCharacteristicsRepository.Insert(secondCharData);
                                        }
                                        else
                                        {
                                            var secondCharData = new SecondCharacteristics
                                            {
                                                Id = child.Id,
                                                FirstCharacteristicsId = firstCharData.Id,
                                                Sequence = child.Sequence,
                                                SalesOrderId = child.SalesOrderId,
                                                CharacteristicsId = child.CharacteristicsId,
                                                CharacteristicsValueId = child.CharacteristicsValueId,
                                                ValueFreeText = child.ValueFreeText,
                                                Qty = child.Qty
                                            };
                                            AuditService.UpdatedLog(secondCharData);
                                            _secondCharacteristicsRepository.Update(secondCharData);
                                        }
                                    }
                                    _firstCharacteristicsRepository.Update(firstCharData);
                                }
                            }
                        }
                        else if (listLength == 3)
                        {
                            int count = 0;
                            foreach (var item in viewModel)
                            {
                                if (string.IsNullOrEmpty(item.Id))
                                {
                                    count++;
                                    var secondCharData = new SecondCharacteristics
                                    {
                                        Id = MakePK(firstChar.Id, count, 2),
                                        FirstCharacteristicsId = firstChar.Id,
                                        Sequence = item.Sequence,
                                        SalesOrderId = item.SalesOrderId,
                                        CharacteristicsId = item.CharacteristicsId,
                                        CharacteristicsValueId = item.CharacteristicsValueId,
                                        ValueFreeText = item.ValueFreeText,
                                        Qty = item.Qty
                                    };
                                    AuditService.AddedLog(secondCharData);

                                    int thirdCount = 0;
                                    foreach (var child in item.ChildList)
                                    {
                                        thirdCount++;
                                        var thirdCharData = new ThirdCharacteristics
                                        {
                                            Id = MakePK(secondCharData.Id, thirdCount, 2),
                                            SecondCharacteristicsId = secondCharData.Id,
                                            Sequence = child.Sequence,
                                            SalesOrderId = child.SalesOrderId,
                                            CharacteristicsId = child.CharacteristicsId,
                                            CharacteristicsValueId = child.CharacteristicsValueId,
                                            ValueFreeText = child.ValueFreeText,
                                            Qty = child.Qty
                                        };
                                        AuditService.AddedLog(thirdCharData);
                                        _thirdCharacteristicsRepository.Insert(thirdCharData);
                                    }
                                    _secondCharacteristicsRepository.Insert(secondCharData);
                                }
                                else
                                {
                                    var secondCharData = new SecondCharacteristics
                                    {
                                        Id = item.Id,
                                        FirstCharacteristicsId = item.FirstCharacteristicsId,
                                        Sequence = item.Sequence,
                                        SalesOrderId = item.SalesOrderId,
                                        CharacteristicsId = item.CharacteristicsId,
                                        CharacteristicsValueId = item.CharacteristicsValueId,
                                        ValueFreeText = item.ValueFreeText,
                                        Qty = item.Qty
                                    };
                                    AuditService.UpdatedLog(secondCharData);
                                    _secondCharacteristicsRepository.Update(secondCharData);

                                    foreach (var child in item.ChildList)
                                    {
                                        var thirdCharData = new ThirdCharacteristics
                                        {
                                            Id = child.Id,
                                            SecondCharacteristicsId = child.SecondCharacteristicsId,
                                            Sequence = child.Sequence,
                                            SalesOrderId = child.SalesOrderId,
                                            CharacteristicsId = child.CharacteristicsId,
                                            CharacteristicsValueId = child.CharacteristicsValueId,
                                            ValueFreeText = child.ValueFreeText,
                                            Qty = child.Qty
                                        };
                                        AuditService.UpdatedLog(secondCharData);
                                        _thirdCharacteristicsRepository.Update(thirdCharData);
                                    }
                                }
                            }
                        }
                    }
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                else
                {
                    _thirdCharacteristicsRepository.Delete(_thirdCharacteristicsRepository.Query(t => t.SalesOrderId == soId).Select().AsEnumerable());
                    _secondCharacteristicsRepository.Delete(_secondCharacteristicsRepository.Query(t => t.SalesOrderId == soId).Select().AsEnumerable());
                    _firstCharacteristicsRepository.Delete(_firstCharacteristicsRepository.Query(t => t.SalesOrderId == soId).Select().AsEnumerable());
                    _unitOfWork.SaveChanges();

                }


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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertOrUpdateSalesOrderTax(string salesOrderId, IEnumerable<SalesOrderTax> salesOrderTaxList)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                var dbList = _salesOrderTaxRepository.Query(t => t.SalesOrderId == salesOrderId).Select().AsEnumerable();

                var count = _salesOrderTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[SalesOrderTax] WHERE SalesOrderId='{salesOrderId}'").First();

                foreach (var item in salesOrderTaxList)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        count++;
                        item.Id = MakePK(salesOrderId, count, 2);
                        item.SalesOrderId = salesOrderId;
                        AuditService.AddedLog(item);
                        _salesOrderTaxRepository.Insert(item);
                    }
                    else
                    {
                        AuditService.UpdatedLog(item);
                        _salesOrderTaxRepository.Update(item);
                    }
                }
                if (dbList != null)
                {
                    var deleteList = dbList.Where(t => t.SalesOrderId == salesOrderId).ToList();
                    foreach (var item in deleteList)
                    {
                        if (!salesOrderTaxList.Any(t => t.Id == item.Id))
                            _salesOrderTaxRepository.Delete(item);
                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteItem(string id)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;

                var itemData = _itemRepository.Query(t => t.Id == id).Select().FirstOrDefault();

                var salesList = _salesOrderRepository.Query(t => t.MasterOrderItemId == id).Select().ToList();
                var salesIds = salesList.Select(t => t.Id).ToArray();

                var firstList = _firstCharacteristicsRepository.Query(t => salesIds.Contains(t.SalesOrderId)).Select().ToList();
                var secondList = _secondCharacteristicsRepository.Query(t => salesIds.Contains(t.SalesOrderId)).Select().ToList();
                var thirdList = _thirdCharacteristicsRepository.Query(t => salesIds.Contains(t.SalesOrderId)).Select().ToList();

                DeleteItemAttributeValue(id);
                foreach (var item in thirdList)
                {
                    _thirdCharacteristicsRepository.Delete(item);
                }
                foreach (var item in secondList)
                {
                    _secondCharacteristicsRepository.Delete(item);
                }
                foreach (var item in firstList)
                {
                    _firstCharacteristicsRepository.Delete(item);
                }
                foreach (var item in salesList)
                {
                    DeleteTax(item.Id);
                    _salesOrderRepository.Delete(item);
                }
                _itemRepository.Delete(itemData);

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteSO(string id)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;

                var salesData = _salesOrderRepository.Query(t => t.Id == id).Select().FirstOrDefault();

                DeleteTax(id);

                var firstList = _firstCharacteristicsRepository.Query(t => t.SalesOrderId == id).Select().ToList();
                var secondList = _secondCharacteristicsRepository.Query(t => t.SalesOrderId == id).Select().ToList();
                var thirdList = _thirdCharacteristicsRepository.Query(t => t.SalesOrderId == id).Select().ToList();

                foreach (var item in thirdList)
                {
                    _thirdCharacteristicsRepository.Delete(item);
                }
                foreach (var item in secondList)
                {
                    _secondCharacteristicsRepository.Delete(item);
                }
                foreach (var item in firstList)
                {
                    _firstCharacteristicsRepository.Delete(item);
                }

                _salesOrderRepository.Delete(salesData);

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteFirstSku(string id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                var firstData = _firstCharacteristicsRepository.Query(t => t.Id == id).Select().FirstOrDefault();
                _firstCharacteristicsRepository.Delete(firstData);

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void DeleteTax(string salesOrderId)
        {
            try
            {
                var taxList = _salesOrderTaxRepository.Query(t => t.SalesOrderId == salesOrderId).Select().ToList();

                foreach (var item in taxList)
                {
                    _salesOrderTaxRepository.Delete(item);
                }

            }
            catch (Exception)
            {
                throw;
            }

        }

        private void DeleteItemAttributeValue(string itemId)
        {
            try
            {
                var attributeValueList = _itemAttributeValueRepository.Query(t => t.MasterOrderItemId == itemId).Select().ToList();
                foreach (var item in attributeValueList)
                {
                    _itemAttributeValueRepository.Delete(item);
                }
            }
            catch (Exception)
            {
                throw;
            }

        }



        #region report GetMasterOrderReport
        public void GetMasterOrderReport(string companyId, string plantId, string masterOrderId)
        {

            var fileName = "";
            var strPath = "";

            var File = "";


            fileName = "PI No F.docx";
            strPath = Path.Combine(ResourcesPathReader.GetMasterOrderFilePath(), fileName);
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);
            //Gets the paragraph at index 1
            try
            {
                WSection section = document.Sections[0];

                //DataTable dsOrderMaster;

                //dsOrderMaster = LoadOrderMaster(companyId, masterOrderId);//sql
                Dictionary<string, string> columns = new Dictionary<string, string>();

                //foreach (DataColumn item in dsOrderMaster.Columns)
                //    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);


                //MakeOrderDetailsTable(document, dsOrderMaster, masterOrderId);//Material Details 



                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    //if (columns.ContainsKey(text.ToUpper()))
                    //{
                    //    ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    //}
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);


                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);

                }



                ////Creates an instance of the DocToPDFConverter
                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);

                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects
                document.Close();

                //Saves the PDF file 
                pdfDocument.Save("file.pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);

                document.Close();

            }
            catch (Exception ex)
            {
                throw ex;

            }

            document.Close();

        }

        class ClsStdLib
        {
            public static string passWord = "prodDisplay";
            public ClsStdLib()
            {

            }
            public enum MType
            {
                Error,
                Success,
                Information
            }
            public static bool passwordGet = true;
            public static string[] sMonth = new string[] { "<Unselect>", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            public static string DataRankNames(int dayNo)
            {

                if (dayNo <= 0)
                    return "";

                if (dayNo.ToString().Length > 1)
                {
                    string Right = dayNo.ToString().Substring(dayNo.ToString().Length - 2, 2);
                    if (ClsStdLib.Dbl(Right) >= 10 && ClsStdLib.Dbl(Right) <= 20)
                        return dayNo + "th";
                }

                string RightString = dayNo.ToString().Substring(dayNo.ToString().Length - 1, 1);
                switch (RightString)
                {
                    case "1":
                        return dayNo + "st";
                    case "2":
                        return dayNo + "nd";
                    case "3":
                        return dayNo + "rd";
                    default:
                        return dayNo + "th";

                }




            }

            #region date related
            public static readonly string dateFormat = "dd-MMM-yyyy";
            public static readonly string sqliteDateFormat = "yyyy-MM-dd";
            public static readonly string AppToDBdateFormat = "yyyy-MM-dd hh:mm:ss";
            public static bool IsDateOK(string strdate)
            {
                try
                {
                    if (strdate.Length != 11)
                    {
                        return false;
                    }
                    if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
                    {
                        return false;
                    }
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            private static bool DateOkCheck(string strdate)
            {
                try
                {
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            public static object Chk_NullDateData(object dateValue)
            {
                if (DateOkCheck("" + dateValue.ToString()) == false)
                {
                    dateValue = "";
                }

                if (("" + dateValue.ToString()) == "")
                {
                    System.DateTime dt = new System.DateTime(1901, 1, 1);
                    dateValue = (object)dt;
                }
                return (object)dateValue;
            }
            public static System.DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
            {
                string strDate = null;
                dateValue = Chk_NullDateData(dateValue);
                strDate = dateValue.ToString();
                if (strDate != "")
                {
                    if (input_date_format.Trim() != "")
                    {
                        if (output_date_format.Trim() != "")
                        {
                            System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo
                            {
                                ShortDatePattern = input_date_format
                            };
                            System.DateTime myDt = System.Convert.ToDateTime(strDate, InputFormat);
                            strDate = myDt.ToString(output_date_format);
                        }
                    }
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static Object DateData_AppToDB(object dateValue, string DB_Level_date_format)
            {
                if (string.IsNullOrEmpty((string)dateValue))
                    return DBNull.Value;

                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString(), DB_Level_date_format).ToString();
                }

                string m = System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);
                return System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);


            }// End of function
            public static System.DateTime DateData_DBToApp(object dateValue)
            {
                string strDate = null;
                strDate = dateValue.ToString();

                System.Globalization.DateTimeFormatInfo myDBDateFormat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
                strDate = DateData_DBToApp(dateValue, myDBDateFormat.ShortDatePattern.ToString()).ToString();
                return System.Convert.ToDateTime(strDate);
            }// End function
            public static System.DateTime DateData_DBToApp(object dateValue, string DB_Level_date_format)
            {
                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, DB_Level_date_format, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString()).ToString();
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static String MakeBaseBlank(object dateValue)
            {
                System.DateTime dt;
                dt = System.Convert.ToDateTime(dateValue.ToString());
                if (dt.Year == 1901)
                {
                    return "";
                }
                else
                {
                    return dateValue.ToString();
                }
            }// End of function
            ///<summary>
            ///return day difference in integer. 
            ///    Example 1: firstDate[Less Than]lastDate returns positive value
            ///    Example 2: firstDate>lastDate returns negative value
            ///    Example 3: firstDate=lastDate returns 0 [zero]**/
            /// </summary>
            public static int DateDiff(string firstDate, string lastDate)
            {

                int difference = 0;
                try
                {
                    firstDate = Convert.ToDateTime(firstDate).ToString("dd-MMM-yyyy");
                    lastDate = Convert.ToDateTime(lastDate).ToString("dd-MMM-yyyy");

                    if (IsDateOK(firstDate) == false)
                    {
                        Exception ex = new Exception("Invalid [First Date]");
                        throw (ex);
                    }
                    if (IsDateOK(lastDate) == false)
                    {
                        Exception ex = new Exception("Invalid [Last Date]");
                        throw (ex);
                    }
                    DateTime dateFirstDate = Convert.ToDateTime(firstDate);
                    DateTime dateLastDate = Convert.ToDateTime(lastDate);
                    TimeSpan TimeSpan = dateLastDate.Subtract(dateFirstDate);


                    difference = TimeSpan.Days;
                }
                catch (Exception ex)
                {
                    throw (ex);
                }

                return difference;
            }



            public static string GetSqliteDate(string standardDate)
            {
                return (Convert.ToDateTime(standardDate).ToString(sqliteDateFormat));
            }
            public static string GetStandardDateFromSqliteDate(string SqliteDate)
            {
                if (SqliteDate.Length != 10)
                    return "";
                if (SqliteDate.Split('-').Length != 3)
                    return "";
                //many things to validate 
                //but i have less time :)
                string month = ValidLength(sMonth[Convert.ToInt32(SqliteDate.Split('-')[1])], 3).ToString();


                return SqliteDate.Split('-')[2] + "-" + month + "-" + SqliteDate.Split('-')[0];
            }
            #endregion date related

            #region numeric
            public static bool IsNumeric(string strNumber)
            {
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Length == 0)
                {
                    return false;
                }
                return Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out double d);
            } // End Function
            public static string GetNumericData(string strNumber)
            {
                strNumber = strNumber.Replace(",", "");
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0"; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out double d) == true)
                {
                    return strNumber;
                }
                else
                {
                    return "0";
                }
            }// end function
            public static string GetNumericDataInDecimalFormat(string strNumber, int precision)
            {
                if (precision < 1)
                    return strNumber;

                string s_precision = new String('0', precision);

                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0." + s_precision; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out double d) == true)
                {
                    return string.Format("{0:0." + s_precision + "}", d);
                }
                else
                {
                    return "0." + s_precision;
                }
            }// end function
            public static double Dbl(string d)
            {
                return Convert.ToDouble(GetNumericData(d));

            }
            public static int Percentage(int total, double percentage)
            {
                return (int)(total * (percentage / 100));

            }
            //validation
            public static void NumericValidation(string value, bool isMandatory, bool isInteger, bool negativeAllowed, string fieldName)
            {

                try
                {



                    if (isMandatory == true)
                    {
                        if (value.Trim() == "")
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }
                        if (Convert.ToDouble(GetNumericData(value.Trim())) == 0)
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }

                        if (value.Trim() != "")
                        {
                            if (IsNumeric(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }

                    if (value.Trim() != "")
                    {
                        if (IsNumeric(value.Trim()) == false)
                        {
                            Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                            throw (ex);
                        }
                        if (isInteger == true)
                        {

                            if (IsInt(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Number must be integer for the field [" + fieldName + "]");
                                throw (ex);
                            }

                        }
                        if (negativeAllowed == false)
                        {
                            if (Convert.ToDouble(GetNumericData(value.Trim())) < 0)
                            {
                                Exception ex = new Exception("Negative values are not allowed for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }



                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }


            }

            ///<summary>
            ///check whether a value is integer or not returns true if integer, 
            ///false if floating or string containing alpahnumeric
            ///</summary>
            public static bool IsInt(string num)
            {

                bool isInt;
                try
                {
                    isInt = System.Int32.TryParse(num, out int number);
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
                return isInt;
            }


            #endregion numeric

            #region string

            public static readonly string excelNegativePOsitiveSign = @"+#,##0.00;-#,##0.00;* ??;@";
            public static readonly string NegativePOsitiveSign = @"+#,##0.00;-#,##0.00;0";
            public static readonly string NumberFormatString = "#,##0.000;(#,##0.000);* ??;@";
            public static readonly string NumberFormatStringFourDecimal = "#,##0.0000;(#,##0.0000);* ??;@";
            public static readonly string NumberFormatStringFiveDecimal = "#,##0.00000;(#,##0.00000);* ??;@";
            public static readonly string NumberFormatStringTwoDecimal = "#,##0.00;(#,##0.00);* ??;@";
            public static readonly string NumberFormatStringTwoDecimalWithZero = "#,##0.00;(#,##0.00)";
            public static readonly string NumberFormatStringInteger = "#,##0;(#,##0);* ??;@";
            public static readonly string NumberFormatStringIntegerWithZero = "#,##0;(#,##0)";
            public static readonly string NumberFormatStringText = "@"; //format cell data as text


            public static object ValidLength(string str)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");

                return (object)removechar.Trim();

            }
            public static object ValidLength(string str, int length)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");


                int strLen = removechar.Length;
                if (strLen > length)
                    removechar = removechar.Substring(0, length);

                return (object)removechar.Trim();

            }
            public static string FileNameLegalChar(string fileName)
            {
                string illegalChar = @"~`!@#$%^&*=/\|>,<";
                foreach (char c in illegalChar)
                {
                    fileName = fileName.Replace(c.ToString(), " ");
                }

                return fileName;
            }
            private StringCollection GetTableColumns(ref DataSet dsLocal)
            {
                StringCollection strcol = new StringCollection();
                for (int COL = 0; COL < dsLocal.Tables[0].Columns.Count; COL++)
                {
                    strcol.Add(dsLocal.Tables[0].Columns[COL].ColumnName.ToUpper());
                }

                return strcol;

            }
            public static string EmptyString(string str)
            {
                //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
                if (str == "&nbsp;")
                    str = "";
                if (string.IsNullOrEmpty(str) == true)
                    str = "";


                return str;
            }//this function returns an empty string(not a null) from null or empty '&nbsp;' from the page
            #endregion string


            #region others

            public static string GetxlsCol(int intCol)
            {
                //returns excel columns based on column number. tested 1 to 256 column numbers
                try
                {
                    if (intCol < 1 || intCol > 256)
                    {
                        System.Exception ex = new Exception("Invalid Column Value");
                        throw (ex);
                    }
                    intCol = intCol - 1;
                    int intFirstLetter = ((intCol) / 512) + 64;
                    int intSecondLetter = ((intCol % 512) / 26) + 64;
                    int intThirdLetter = (intCol % 26) + 65;
                    char FirstLetter;
                    char SecondLetter;
                    if (intFirstLetter > 64)
                        FirstLetter = (char)intFirstLetter;
                    else
                        FirstLetter = ' ';

                    if (intSecondLetter > 64)
                        SecondLetter = (char)intSecondLetter;
                    else
                        SecondLetter = ' ';

                    char ThirdLetter = (char)intThirdLetter;
                    return string.Concat(FirstLetter, SecondLetter, ThirdLetter).Trim();
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
            }//returns excel columns based on column number. tested 1 to 256 column numbers
            #endregion others

            public static object RetValidLen(string Data)
            {
                if (string.IsNullOrEmpty(Data))
                    return DBNull.Value;

                return Data;
            }
            public static double Sum(string columnName, DataTable dtLocal, string criteria)
            {
                double total = 0;
                DataRow[] dr = dtLocal.Select(criteria);
                foreach (DataRow d in dr)
                {
                    total += Dbl(d[columnName].ToString());
                }


                return total;
            }
        }

        public DataTable LoadOrderMasterItems(string OrderMasterID)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT so.MasterOrderItemId,so.Id AS SOID,CONCAT( mm.[Description],' ',a.StandardName) AS MaterialDesc,
                                so.Qty,uom.UserName AS UOM,SO.Rate,so.Qty*so.Rate AS Amount,isnull(SO.Discount,0) AS Discount
                                FROM [TRN].[MasterOrderItem] T
                                INNER JOIN [TRN].[MasterOrder] O ON o.Id=t.MasterOrderId
                                INNER JOIN [TRN].[SalesOrder]  SO ON so.MasterOrderItemId=t.Id
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=t.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=mm.MaterialGroupMasterId
                                LEFT OUTER JOIN [MST].[MaterialMasterArticle] A ON a.Id=t.ArticleId
                                LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=o.TotalQtyUOMId
                                WHERE MasterOrderId='" + OrderMasterID + "'";
                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        //public DataTable LoadOrderMaster(string companyId, string OrderMasterID)
        //{
        //    string strSQL;
        //    try
        //    {
        //        strSQL = @"SELECT P.UserName AS CustomerName,a.Id AS FileNo,s.Code AS CustomerStateCode,a.[Type], B.UserName Buyer,pl.GSTIN,InvPP.GSTIN PartyGSTIN, A.MasterOrderNo, A.OrderYear, A.CurrencyId, A.TotalQty	, A.NoOfLineItem, EI.EmployeeName AS ResponsiblePersonName
        //                    , InvPP.UserName AS InvoicingPartyPlant,UOm.UserName AS UOM, A.InvoicingByAddress, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
        //                    , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
        //                    AND PartyId=A.PartyId AND PartyType='Customer' ),A.OrderWastagePercentage,A.ExtraOrderPercentage,C.Code Currency
        //                    FROM [TRN].[MasterOrder] AS A
        //                    JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
        //                    LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
        //                    LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
        //                    LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
        //                    LEFT OUTER JOIN  [MST].[AddressMaster] AM ON am.Id=invpp.AddressMasterId
        //                    LEFT OUTER JOIN scs.[State] AS s ON s.Id=am.StateId
        //                    LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
        //                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
        //                    LEFT JOIN [HKP].[Buyer] AS B ON B.Id=A.BuyerId
        //                    LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=A.TotalQtyUOMId
        //                    WHERE A.CompanyId='" + companyId + @"' AND A.Id='" + OrderMasterID + "'";

        //        return _sqlRepository.GetDataTable(strSQL);

        //    }
        //    catch (System.Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {

        //    }
        //}

        //public DataTable LoadOrderMasterTax(string OrderMasterID)
        //{
        //    string strSQL;
        //    try
        //    {
        //        strSQL = @"SELECT so.MasterOrderItemId,tg.Code AS TaxCode,st.Percentage, st.TaxAmount
        //                   FROM [TRN].[MasterOrderItem] T
        //                   INNER JOIN [TRN].[MasterOrder] O ON o.Id=t.MasterOrderId
        //                   INNER JOIN [TRN].[SalesOrder]  SO ON so.MasterOrderItemId=t.Id
        //                   INNER JOIN [TRN].[SalesOrderTax] ST ON st.SalesOrderId=so.Id
        //                   LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=st.TaxCategoryId
        //                   WHERE MasterOrderId='" + OrderMasterID + @"' ORDER BY tg.[Sequence]";

        //        return _sqlRepository.GetDataTable(strSQL);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {

        //    }
        //}


        #endregion

        #region Proforma Invoice Report Word Report

        public void GetProformaInvoiceReportService(string companyId, string plantId, string masterOrderId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "ProformaInvoice" + plantId + ".docx";

            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            WordDocument document = new WordDocument(File, FormatType.Docx);

            try
            {
                WSection section = document.Sections[0];

                DataTable dsOrderMaster;

                dsOrderMaster = loadProformaInvoiceMaster(masterOrderId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeProformaInvoiceDetailTable(companyId, companyId, plantId, masterOrderId, document, dsOrderMaster);   // {materialItems}

                //var SalesTotal = makeProformaInvoiceServiceTable(companyId, companyId, plantId, masterOrderId, document, dsOrderMaster);   // {{ServiceItems}}

                document.Replace("{GrandTotal}", (MaterialTotal).ToString("F2") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);


                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        //ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Tables[0].Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                        document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);


                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }
                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = fileName;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();

            }
            catch (Exception ex)
            {
                throw ex;

            }

            document.Close();
        }

        public double makeProformaInvoiceDetailTable(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{materialItems}";

            DataTable sales, materialTax;
            //Sales== Master Query
            sales = loadSalesOrderMaster(salesId);
            materialTax = ProformaInvoiceMasterTax(salesId);

            int LasColumnIndex = 9;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(materialTax.DefaultView.ToTable(true, "TaxCode"));

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 100;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 100;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar2].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar3].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UoM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL++;
            wTable.Rows[ROW].Cells[colUoM].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL;
            wTable.Rows[ROW].Cells[colRate].Width = 80;

            int colTotalTaxableAmount = COL;
            int colpersentage = COL;
            int colTaxAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 100;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
                COL = colTotalTaxableAmount;
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
            }


            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        COL++;
                        colpersentage = COL; COL++;

                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                        colTaxAmount = COL;
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }

            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);

                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);
                }
                ROW++;
            }
            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                sl++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colHSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(ClsStdLib.Dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("F2"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString());
                TROW.Cells[colRate].AddParagraph().AppendText(ClsStdLib.Dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("F2"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(ClsStdLib.Dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("F2"));
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Percentage"].ToString());
                TROW.Cells[colpersentage].AddParagraph().AppendText(Convert.ToDouble(dsOrderMaster.Rows[i]["Percentage"].ToString()).ToString("F2"));
                TROW.Cells[colTaxAmount].AddParagraph().AppendText(Convert.ToDouble(dsOrderMaster.Rows[i]["TaxAmount"].ToString()).ToString("F2"));

                //if (dv.Count > 0)
                //{
                //    DataView dvtax = new DataView(materialTax.DefaultView.ToTable());

                //    for (int T = 0; T < dv.Count; T++)
                //    {
                //        //dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "'";
                //        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND SalesOrderId='" + materialTax.Rows[i]["SalesOrderId"].ToString() + "'";

                //        if (dvtax.Count > 0)
                //        {
                //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));
                //        }
                //    }
                //}
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            range.ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colArticle || C == colHSN || C == colUoM || C == colRate || C == colQty || C == colChar1 || C == colChar2 || C == colChar3 || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += ClsStdLib.Dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2")).ApplyCharacterFormat(FontBold);
            }
            #endregion Total

            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = ClsStdLib.Dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString())
                    //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                    + ClsStdLib.Dbl(materialTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 30;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 30 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return total;
        }

        public DataTable loadProformaInvoiceMaster(string masterOrderId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IR.Id CustomerNo
                                --,IR.CompanyGroupId
                                ,IR.CompanyId
								,p.UserName Customer
								,Addres.Address1 VendorAddress
								,ISNULL(HSNC.Code,MHSN.Code) HSNCode
                                ,Plant.GSTIN 
                                ,DPARTYPL.GSTIN ShipGSTIN
                                ,INVPARTYPL.GSTIN BillGSTIN
								--,IR.DocRefNo   
	                            --,IR.InvoiceNo
                                ,REPLACE(Convert(VARCHAR(11), SO.DeliveryDate, 106), ' ', '-') AS DeliveryDate
                                --,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                --,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
		                        ,IR.InvoicingPartyPlantId
		                        ,INVPARTYPL.UserName InvoiceParty
                                ,INVPARTYPL.UserName InvoiceParty2
		                        ,IR.InvoicingByAddress
		                        ,IR.DeliveryByAddress
		                        ,DPARTYPL.UserName DeliveryParty
		                        ,IR.DeliveryPartyPlantId		
		                        ,MOI.MaterialMasterId
		                        
		                        ,IR.AddedBy
		                        ,IR.AddedDate
		                        ,IR.UpdatedBy
		                        ,IR.UpdatedDate
		                        ,0 IsApproved
		                     --   ,IR.PartyType
		                        ,0 IsNonCreditable
		                        ,IR.CurrencyId
	                            ,CRNC.Code AS CurrencyName
	                           -- ,IR.ToCurrencyRate
		                        ,BASECRNC.Code AS BaseCurrencyName
		                       -- ,PayTerm.UserName PaymentTerm
	                          ,MM.UserName MaterialMaster
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
	                          ,MOI.ArticleId
	                          ,MMA.StandardName Article
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                             
	                          ,FCV.UserName AS FirstCharacteristicsValue
                             
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                       
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                          ,ROUND(SO.Qty, 2) POTransactionQty
	                          ,ROUND(SO.Rate, 2) TransactionRate
	                          ,ROUND((SO.Qty*SO.Rate), 2) AS TrnAmount
	                          ,BaseAmount=ROUND((SO.Qty*SO.Rate), 2)
	                          ,BaseTaxAmount=(SO.Qty*SO.Rate)-SO.Discount	                         
	                          ,TUoM.UserName AS TransactionUoM
							
							  ,OurOrderRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+MO.OwnReferenceNo FROM 
                                        TRN.SalesOrderItem SOI
										JOIN TRN.MasterOrder MO ON MO.Id=SOI.MasterOrderId
                                        WHERE IR.Id=SOI.SalesId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
							,YourOrderRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+MO.BuyerReferenceNo FROM 
                                        TRN.SalesOrderItem SOI
										JOIN TRN.MasterOrder MO ON MO.Id=SOI.MasterOrderId
                                        WHERE IR.Id=SOI.SalesId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
										,tg.Code AS TaxCode,ISNULL(IRT.Percentage,0) Percentage, ISNULL(IRT.TaxAmount,0)TaxAmount
                              FROM [TRN].SalesOrder SO
							  LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id = SO.MasterOrderItemId
							  LEFT JOIN TRN.MasterOrder IR ON IR.Id = MOI.MasterOrderId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId

                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = IR.CurrencyId
                         LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                         LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId 
						 LEFT JOIN HKP.Party P ON P.Id=IR.PartyId
						 LEFT JOIN [MST].[AddressMaster] Addres ON Addres.Id= P.AddressMasterId
                         LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = MOI.MaterialMasterId
						 	LEFT JOIN [HKP].[HSNCode] AS MHSN ON MHSN.ID=MM.HSNCodeId
                         LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = MOI.ArticleId
						 	LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MMA.HSNCodeId

                         LEFT JOIN TRN.FirstCharacteristics AS SFC ON SO.Id = SFC.SalesOrderId
						  LEFT JOIN HKP.Characteristics AS FC ON FC.Id = SFC.CharacteristicsId

                         LEFT JOIN TRN.SecondCharacteristics AS SSC ON SO.Id = SSC.SalesOrderId
						 LEFT JOIN HKP.Characteristics AS SC ON SC.Id = SSC.CharacteristicsId

                         LEFT JOIN TRN.ThirdCharacteristics AS TSC ON SO.Id = TSC.SalesOrderId
						 LEFT JOIN HKP.Characteristics AS TC ON SC.Id = TSC.CharacteristicsId

                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON FCV.Id = SFC.CharacteristicsValueId
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON SCV.Id = SSC.CharacteristicsValueId
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON TCV.Id = TSC.CharacteristicsValueId
                         JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IR.TotalQtyUOMId = TUoM.Id
						  LEFT join [TRN].[SalesOrderTax] IRT ON IRT.SalesOrderId = SO.Id 
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                            WHERE  IR.Id='" + masterOrderId + "'";

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
        public DataTable loadSalesOrderItems(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IOS.Id ServiceId, SM.UserName  Service ,IOS.Amount,IOS.TaxAmount,IOS.AddedBy,IOS.AddedDate,IOS.UpdatedBy,IOS.UpdatedDate 
                               FROM TRN.Sales   IR
                            INNER join trn.SalesService IOS ON IOS.SalesId = IR.Id
                            INNER JOIN HKP.ServiceMaster SM ON IOS.ServiceMasterId = SM.Id 
                             where IR.Id = '" + SalesId + "'";

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
        public DataTable ProformaInvoiceMasterTax(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"select IR.Id SalesOrderId,PO.MasterOrderItemId,PO.Id SalesMaterialId,IRT.Id AS SalesTax,tg.Code AS TaxCode,IRT.Percentage, IRT.TaxAmount
								from [TRN].SalesOrder PO
                               Inner join [TRN].[SalesOrderTax] IRT ON IRT.SalesOrderId = PO.Id 
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
							   LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id = PO.MasterOrderItemId
							  LEFT JOIN TRN.MasterOrder IR ON IR.Id = MOI.MasterOrderId
                                 WHERE IR.Id='" + SalesId + @"'";

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
        public DataTable loadSalesOrderMaster(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IOS.Id ServiceId, SM.UserName  Service ,IOS.Amount,IOS.TaxAmount,IOS.AddedBy,IOS.AddedDate,IOS.UpdatedBy,IOS.UpdatedDate 
                               FROM TRN.Sales   IR
                            INNER join trn.SalesService IOS ON IOS.SalesId = IR.Id
                            INNER JOIN HKP.ServiceMaster SM ON IOS.ServiceMasterId = SM.Id 
                            where IR.Id = '" + SalesId + @"'";

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
        public DataTable loadGRNServiceMasterTex(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"select PO.SalesId,PO.Id SalesMaterialId,IRT.Id AS SalesTax,tg.Code AS TaxCode,IRT.Percentage, IRT.Amount TaxAmount
								from TRN.[SalesService] PO
                               Inner join trn.SalesTax IRT ON IRT.SalesServiceId = PO.Id 
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                 WHERE PO.SalesId='" + SalesId + @"'
								 and IRT.SalesServiceId  IS NOT NULL AND  IRT.SalesMaterialId IS NULL 
								 ORDER BY tg.[Sequence] ";

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



        #endregion

        public Dictionary<string, object> GetWeekendbyBuyer(string buyerId)
        {
            try
            {
                var sql = "SELECT Weekend FROM [MST].[LSD] Where BuyerId='" + buyerId + "'";
                return _sqlRepository.GetData(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetDay(string customWeekOff)
        {
            try
            {
                Dictionary<string, int> dayList = new Dictionary<string, int>();
                dayList.Add("Friday", 0);
                dayList.Add("Saturday", 1);
                dayList.Add("Sunday", 2);
                dayList.Add("Monday", 3);
                dayList.Add("Tuesday", 4);
                dayList.Add("Wednesday", 5);
                dayList.Add("Thursday", 6);

                var dv = 0;
                if (dayList.ContainsKey(customWeekOff))
                {
                    dv = dayList[customWeekOff];
                }

                int _count = 0;
                foreach (var item in dayList)
                {
                    int a = dv + _count;
                    if (a > 0)
                        a = 7 - a;

                    if (a < 0)
                        a = Math.Abs(a);
                    dayList[item.Key] = a;
                    _count++;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public string GetDelivaryDate(string year, int weekNo, string buyerId)
        {
            try
            {
                var weekendDay = GetWeekendbyBuyer(buyerId);
                if (weekendDay.Count == 0)
                {
                    throw new Exception("Please set Weekend from Order Date Setting.");
                }
                //GetDay(weekendDay["WeekendforProductionOrder"].ToString());
                int delta = 0;
                DateTime inputDate = Convert.ToDateTime("01-Jan" + year);
                if (weekendDay["Weekend"].ToString().ToUpper() == DayOfWeek.Friday.ToString().ToUpper())
                {
                    delta = DayOfWeek.Friday - inputDate.DayOfWeek;
                }
                else if (weekendDay["Weekend"].ToString().ToUpper() == DayOfWeek.Saturday.ToString().ToUpper())
                {
                    delta = DayOfWeek.Saturday - inputDate.DayOfWeek;
                }
                else if (weekendDay["Weekend"].ToString().ToUpper() == DayOfWeek.Sunday.ToString().ToUpper())
                {
                    delta = DayOfWeek.Sunday - inputDate.DayOfWeek;
                }
                else if (weekendDay["Weekend"].ToString().ToUpper() == DayOfWeek.Monday.ToString().ToUpper())
                {
                    delta = DayOfWeek.Monday - inputDate.DayOfWeek;
                }
                else if (weekendDay["Weekend"].ToString().ToUpper() == DayOfWeek.Tuesday.ToString().ToUpper())
                {
                    delta = DayOfWeek.Tuesday - inputDate.DayOfWeek;
                }
                else if (weekendDay["Weekend"].ToString().ToUpper() == DayOfWeek.Wednesday.ToString().ToUpper())
                {
                    delta = DayOfWeek.Wednesday - inputDate.DayOfWeek;
                }
                else if (weekendDay["Weekend"].ToString().ToUpper() == DayOfWeek.Thursday.ToString().ToUpper())
                {
                    delta = DayOfWeek.Thursday - inputDate.DayOfWeek;
                }


                DateTime WeekDate = inputDate;

                if (delta < 0)
                    WeekDate = inputDate.AddDays(delta);
                else
                    WeekDate = inputDate.AddDays(delta - 7);

                WeekDate = WeekDate.AddDays(weekNo * 7);
                string date = WeekDate.ToString("dd-MMM-yyyy");
                return date;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public object GetOrderDateSetting(string shipmentModeId, string buyerId)
        {
            try
            {
                var sql = "SELECT * FROM [MST].[LSD] WHERE ShipModeId='" + shipmentModeId + "' AND BuyerId='" + buyerId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public object GetSOBookedQtyAndLevel(string salesOrderId)
        {
            try
            {
                string sql = string.Empty;
                sql = @"SELECT ISNULL(SUM(Quantity),0) Quantity,'SalesOrder' BookingLevel FROM TRN.ProductionSummary PS
                        WHERE PS.SalesOrderId='" + salesOrderId + @"' AND PS.ProcessId=(
                        SELECT TOP(1) POPS.ProcessId FROM [TRN].[ProductionOrderDetail] POD 
                        JOIN TRN.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
                        JOIN [TRN].[ProductionOrderProcessSet] POPS ON POPS.ProductionOrderId=POD.ProductionOrderId
                        WHERE POPS.IsBaseProcess=1 AND POD.SalesOrderId='" + salesOrderId + "')";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public object GetPOBookedQtyAndLevel(string salesOrderId)
        {
            try
            {
                string sql = string.Empty;
                sql = @"SELECT ISNULL(SUM(Quantity),0) Quantity,'ProductionOrder' BookingLevel FROM TRN.ProductionSummary PS
                            WHERE PS.ProductionOrderId=(SELECT ProductionOrderId FROM [TRN].[ProductionOrderDetail] WHERE SalesOrderId='" + salesOrderId + @"') AND PS.ProcessId=(
                            SELECT TOP(1) POPS.ProcessId FROM [TRN].[ProductionOrderDetail] POD 
                            JOIN TRN.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
                            JOIN [TRN].[ProductionOrderProcessSet] POPS ON POPS.ProductionOrderId=POD.ProductionOrderId
                            WHERE POPS.IsBaseProcess=1 AND POD.SalesOrderId='" + salesOrderId + "')";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void UpdateSODateGraph(SalesOrderMaster salesOrderMaster)
        {
            try
            {

                if (!string.IsNullOrEmpty(salesOrderMaster.Id))
                {
                    var sodata = _salesOrderRepository.Find(salesOrderMaster.Id);
                    AuditService.UpdatedLog(salesOrderMaster);
                    sodata.DeliveryDate = salesOrderMaster.DeliveryDate;
                    sodata.DeliveryDate = salesOrderMaster.DeliveryDate;
                    sodata.CheckByStatus = salesOrderMaster.CheckByStatus;
                    sodata.CheckByDate = salesOrderMaster.CheckByDate;
                    sodata.ApproveBy = salesOrderMaster.ApproveBy;
                    sodata.ApprovedStatus = salesOrderMaster.ApprovedStatus;
                    sodata.ApproveByDate = salesOrderMaster.ApproveByDate;


                    _salesOrderRepository.Update(sodata);

                }

                _unitOfWork.SaveChanges();


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

        public void UpdateSODate(SalesOrderMaster salesOrderMaster)
        {
            try
            {

                if (!string.IsNullOrEmpty(salesOrderMaster.Id))
                {
                    var sodata = _salesOrderRepository.Find(salesOrderMaster.Id);
                    AuditService.UpdatedLog(salesOrderMaster);
                    sodata.DeliveryDate = salesOrderMaster.DeliveryDate;
                    sodata.CommitmentDate = salesOrderMaster.CommitmentDate;
                    sodata.MainRawMaterialInhouseDate = salesOrderMaster.MainRawMaterialInhouseDate;
                    sodata.PlanExFactoryDate = salesOrderMaster.PlanExFactoryDate;
                    sodata.OtherRawMaterialInhouseDate = salesOrderMaster.OtherRawMaterialInhouseDate;
                    sodata.LSD = salesOrderMaster.LSD;
                    sodata.CheckByStatus = salesOrderMaster.CheckByStatus;
                    sodata.CheckByDate = salesOrderMaster.CheckByDate;
                    sodata.ApproveBy = salesOrderMaster.ApproveBy;
                    sodata.ApprovedStatus = salesOrderMaster.ApprovedStatus;
                    sodata.ApproveByDate = salesOrderMaster.ApproveByDate;
                    _salesOrderRepository.Update(sodata);

                }

                TaskScheduler.TaskScheduler schedule = new TaskScheduler.TaskScheduler(_sqlRepository);
                schedule.UpdateTaskStatus();

                DataTable dtm = _sqlRepository.GetDataTable(@"SELECT M.TaskTemplateMasterId FROM TRN.SalesOrder S
LEFT JOIn TRN.MasterOrderItem I ON I.id = S.MasterOrderItemId
LEFT JOIN trn.MasterOrder M ON M.Id = I.MasterOrderId Where S.Id='" + salesOrderMaster.Id + "'");

                //Sales Order Related Tasks
                string sql = @"SELECT SO.* FROM trn.MasterOrder AS mo 
                                INNER JOIN hkp.OrderStatus AS os ON os.Id=mo.OrderStatusId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                           WHERE so.id='" + salesOrderMaster.Id + "' and os.Id<>'" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"' AND ISNULL(mo.TaskTemplateMasterId,'')='" + dtm.Rows[0]["TaskTemplateMasterId"].ToString() + "'";

                DataTable dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {

                    try
                    {
                        DataTable dt = schedule.GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.SalesOrder);
                        if (dt.Rows.Count > 0)
                            schedule.MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.SalesOrder);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                _unitOfWork.SaveChanges();
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

        public void UpdateSORate(SalesOrderMaster salesOrderMaster)
        {
            try
            {
                if (!string.IsNullOrEmpty(salesOrderMaster.Id))
                {
                    var sodata = _salesOrderRepository.Find(salesOrderMaster.Id);
                    AuditService.UpdatedLog(salesOrderMaster);
                    sodata.Rate = salesOrderMaster.Rate;
                    sodata.CM = salesOrderMaster.CM;
                    sodata.UpCharge = salesOrderMaster.UpCharge;
                    sodata.Discount = salesOrderMaster.Discount;
                    sodata.SalesExpense = salesOrderMaster.SalesExpense;
                    sodata.CheckByStatus = salesOrderMaster.CheckByStatus;
                    sodata.CheckByDate = salesOrderMaster.CheckByDate;
                    sodata.ApproveBy = salesOrderMaster.ApproveBy;
                    sodata.ApprovedStatus = salesOrderMaster.ApprovedStatus;
                    sodata.ApproveByDate = salesOrderMaster.ApproveByDate;
                    _salesOrderRepository.Update(sodata);

                }

                _unitOfWork.SaveChanges();

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

        public void UpdateSOQTY(SalesOrderMaster salesOrderMaster)
        {
            try
            {

                if (!string.IsNullOrEmpty(salesOrderMaster.Id))
                {
                    var sodata = _salesOrderRepository.Find(salesOrderMaster.Id);
                    AuditService.UpdatedLog(salesOrderMaster);
                    sodata.Qty = salesOrderMaster.Qty;
                    sodata.ProductionBookingLevel = salesOrderMaster.ProductionBookingLevel;
                    sodata.CheckByStatus = salesOrderMaster.CheckByStatus;
                    sodata.CheckByDate = salesOrderMaster.CheckByDate;
                    sodata.ApproveBy = salesOrderMaster.ApproveBy;
                    sodata.ApprovedStatus = salesOrderMaster.ApprovedStatus;
                    sodata.ApproveByDate = salesOrderMaster.ApproveByDate;
                    _salesOrderRepository.Update(sodata);

                }

                _unitOfWork.SaveChanges();

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

        public void UpdateSOStatus(SalesOrderMaster salesOrderMaster)
        {
            try
            {

                if (!string.IsNullOrEmpty(salesOrderMaster.Id))
                {
                    var sodata = _salesOrderRepository.Find(salesOrderMaster.Id);
                    AuditService.UpdatedLog(salesOrderMaster);
                    sodata.OrderStatusId = salesOrderMaster.OrderStatusId;

                    if (sodata.OrderStatusId != OrderStatusEnum.Active.ToString())
                    {
                        sodata.OrderStatusChangedBy = salesOrderMaster.UpdatedBy;
                        sodata.OrderStatusChangedDate = salesOrderMaster.UpdatedDate;
                        sodata.OrderStatusChangedFromIP = salesOrderMaster.UpdatedFromIP;
                        sodata.ProductionBookedQty = salesOrderMaster.ProductionBookedQty;
                        sodata.CheckByStatus = salesOrderMaster.CheckByStatus;
                        sodata.CheckByDate = salesOrderMaster.CheckByDate;
                        sodata.ApproveBy = salesOrderMaster.ApproveBy;
                        sodata.ApprovedStatus = salesOrderMaster.ApprovedStatus;
                        sodata.ApproveByDate = salesOrderMaster.ApproveByDate;

                    }

                    _salesOrderRepository.Update(sodata);

                }
                TaskScheduler.TaskScheduler schedule = new TaskScheduler.TaskScheduler(_sqlRepository);
                schedule.UpdateTaskStatus();

                DataTable dtm = _sqlRepository.GetDataTable(@"SELECT M.TaskTemplateMasterId FROM TRN.SalesOrder S
LEFT JOIn TRN.MasterOrderItem I ON I.id = S.MasterOrderItemId
LEFT JOIN trn.MasterOrder M ON M.Id = I.MasterOrderId Where S.Id='" + salesOrderMaster.Id + "'");

                //Sales Order Related Tasks
                string sql = @"SELECT SO.* FROM trn.MasterOrder AS mo 
                                INNER JOIN hkp.OrderStatus AS os ON os.Id=mo.OrderStatusId
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                           WHERE so.id='" + salesOrderMaster.Id + "' and os.Id<>'" + Library.Model.Enums.OrderStatusEnum.Closed.ToString() + @"' AND ISNULL(mo.TaskTemplateMasterId,'')='" + dtm.Rows[0]["TaskTemplateMasterId"].ToString() + "'";

                DataTable dtMasterReferenceData = _sqlRepository.GetDataTable(sql);
                for (int i = 0; i < dtMasterReferenceData.Rows.Count; i++)
                {

                    try
                    {
                        DataTable dt = schedule.GetDataSourceMasterOrderNew(dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.SalesOrder);
                        if (dt.Rows.Count > 0)
                            schedule.MakeTNAMaster(dt, dtMasterReferenceData.Rows[i]["Id"].ToString(), TaskAppliedOnEnum.SalesOrder);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                _unitOfWork.SaveChanges();


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

        public void SaveMOIData(IEnumerable<MasterOrderItem> dataList, string masterId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var count = _itemRepository.SqlQuery<int>($"SELECT CAST((RIGHT(ISNULL(MAX(CAST(Id AS INT)), 0),2)) AS INT) Id FROM [TRN].[MasterOrderItem] WHERE MasterOrderId='{masterId}'").First();
                foreach (var item in dataList)
                {
                    if (item.TotalQty == 0) throw new CustomException("Add Qty");
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        count++;
                        item.ProductionGrouping = RemoveSpace(item.ProductionGrouping);
                        item.Id = MakePK(masterId, count, 2);
                        item.MasterOrderId = masterId;
                        AuditService.AddedLog(item);
                        _itemRepository.Insert(item);
                    }

                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveSOData(IEnumerable<SalesOrderMaster> dataList, string masterId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var count = _salesOrderRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[SalesOrder] WHERE MasterOrderItemId='{masterId}'").First();
                foreach (var item in dataList)
                {
                    if (item.DestinationId=="NULL")
                    {
                        item.DestinationId = null;
                    }
                    count++;
                    item.Id = MakePK(masterId, count, 2);
                    item.MasterOrderItemId = masterId;
                    item.OrderStatusId = null;
                    item.CheckByStatus = "To Be Check";
                    AuditService.AddedLog(item);
                    _salesOrderRepository.Insert(item);

                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }



}