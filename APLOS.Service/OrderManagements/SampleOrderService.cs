#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class SampleOrderService : Service<SampleOrder>, ISampleOrderService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ISampleOrderSubMaterialService _sampleOrderSubMaterialService;
        private readonly ISampleOrderPartnerFunctionService _partnerFunctionService;

        public SampleOrderService(
            IRepositoryAsync<SampleOrder> sampleOrderRepository
            , IPKGeneratorService pkGeneratorService
            , ISampleOrderSubMaterialService sampleOrderSubMaterialService
            , ISampleOrderPartnerFunctionService partnerFunctionService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(sampleOrderRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _sampleOrderSubMaterialService = sampleOrderSubMaterialService;
            _partnerFunctionService = partnerFunctionService;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(SampleOrder), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public GridModel Query(GridParameter parameters, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            parameters.CmdText = @"SELECT SO.Id
                                    , SO.PlantId, PL.UserName AS Plant, SO.EntityId, EN.UserName AS Entity
                                    , SO.SalesOrganisationId, SOG.UserName AS SalesOrganisation, SO.SalesGroupId, SG.UserName AS SalesGroup
                                    , SO.BuyerId, B.UserName AS Buyer, SO.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, SO.CurrencyId, C.Code AS Currency
                                    , SO.PaymentTermId, RequestReferenceDate =REPLACE(CONVERT(CHAR(11), SO.RequestReferenceDate, 106),' ','-')
	                                , SO.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SO.InvoicingByAddress, SO.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SO.DeliveryByAddress
                                    , SO.ReferenceDocNo, DeliveryDate =REPLACE(CONVERT(CHAR(11), SO.DeliveryDate, 106),' ','-')
	                                , CP.IsPaymentTermChangeable
                                FROM [TRN].[SampleOrder] AS SO
                                JOIN [HKP].[Party] AS P ON SO.PartyId=P.Id
                                LEFT JOIN (SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsPaymentTermChangeable
			                                FROM [HKP].[CompanyParty] AS C 
			                                LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Customer'
		                                  ) AS CP ON CP.PartyId=SO.PartyId AND CP.PlantId=SO.PlantId
                                JOIN [ORG].[Plant] AS PL ON SO.PlantId=PL.Id
                                JOIN [ORG].[Entity] AS EN ON SO.EntityId=EN.Id
                                JOIN [ORG].[SalesOrganisation] AS SOG ON SO.SalesOrganisationId=SOG.Id
                                JOIN [ORG].[SalesGroup] AS SG ON SO.SalesGroupId=SG.Id
                                LEFT JOIN [HKP].[Buyer] AS B ON SO.BuyerId=B.Id
                                JOIN [SCS].[Currency] AS C ON SO.CurrencyId=C.Id
                                JOIN [MST].[PaymentTerm] AS PT ON SO.PaymentTermId=PT.Id
                                LEFT JOIN [HKP].[PartyPlant] AS IPP ON SO.InvoicingPartyPlantId=IPP.Id
                                LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                                LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                                LEFT JOIN [HKP].[PartyPlant] AS DPP ON SO.DeliveryPartyPlantId=DPP.Id
                                LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                                LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                                WHERE PL.CompanyId='" + identity.CompanyId + "' AND SO.PlantId='" + plantId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public void InsertGraph(SampleOrder entity, IEnumerable<SampleOrderSubMaterial> details, IEnumerable<SampleOrderPartnerFunction> partnerFunctions)
        {
            var flag = false;
            try
            {
                entity.Id = GetPK();
                _sampleOrderSubMaterialService.InsertGraph(entity.Id, details);
                //_partnerFunctionService.InsertOrUpdateGraph(entity.Id, partnerFunctions);
                base.InsertGraph(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
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

        public void UpdateGraph(SampleOrder entity, IEnumerable<SampleOrderSubMaterial> details, IEnumerable<SampleOrderPartnerFunction> partnerFunctions)
        {
            var flag = false;
            try
            {
                _sampleOrderSubMaterialService.InsertOrUpdateGraph(entity.Id, details);
                _partnerFunctionService.InsertOrUpdateGraph(entity.Id, partnerFunctions);
                base.UpdateGraph(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
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

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                _sampleOrderSubMaterialService.DeleteGraph(id);
                _partnerFunctionService.DeleteGraph(id);
                base.DeleteGraph(id);
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }
}