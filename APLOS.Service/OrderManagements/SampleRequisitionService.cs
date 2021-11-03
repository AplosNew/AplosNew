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
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class SampleRequisitionService : Service<SampleRequisition>, ISampleRequisitionService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public SampleRequisitionService(
            IRepositoryAsync<SampleRequisition> sampleRequisitionRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(sampleRequisitionRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(SampleRequisition), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public GridModel Query(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            parameters.CmdText = @"SELECT SO.Id
                                        ,SO.PlantId
                                        ,PL.UserName AS Plant
                                        ,SO.EntityId
                                        ,EN.UserName AS Entity
                                        ,SO.BuyerId
                                        ,B.UserName AS Buyer
                                        ,SO.CustomerId
                                        ,P.UserName AS CustomerName
                                        ,SO.CurrencyId
                                        ,C.Code AS Currency
                                        ,SO.PaymentTermId
                                        ,RequestReferenceDate =REPLACE(CONVERT(CHAR(11), SO.RequestReferenceDate, 106),' ','-')
                                        ,SO.ReferenceDocNo
                                        ,BuyerRequirementDate =REPLACE(CONVERT(CHAR(11), SO.BuyerRequirementDate, 106),' ','-')
	                                    ,CSD.IsChangeable
                                        ,SO.PaidStatus
                                FROM TRN.SampleRequisition AS SO
                                INNER JOIN HKP.Party AS P ON SO.CustomerId=P.Id
                                INNER JOIN ORG.Plant AS PL ON SO.PlantId=PL.Id
                                INNER JOIN ORG.Entity AS EN ON SO.EntityId=EN.Id
                                INNER JOIN HKP.CustomerSalesData AS CSD ON CSD.PartyId=P.Id
                                INNER JOIN HKP.Buyer AS B ON SO.BuyerId=B.Id
                                INNER JOIN SCS.Currency AS C ON SO.CurrencyId=C.Id
                                WHERE PL.CompanyId='" + identity.CompanyId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public override void InsertGraph(SampleRequisition entity)
        {
            var flag = false;
            try
            {
                entity.Id = GetPK();
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

        public override void UpdateGraph(SampleRequisition entity)
        {
            var flag = false;
            try
            {
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