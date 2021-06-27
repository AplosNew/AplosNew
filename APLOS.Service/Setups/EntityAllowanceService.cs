#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Organizations;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Setups
{
    public class EntityAllowanceService : Service<EntityAllowance>, IEntityAllowanceService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public EntityAllowanceService(
            IRepositoryAsync<EntityAllowance> entityAllowanceRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(entityAllowanceRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public override void Insert(EntityAllowance entity)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                entity.CompanyGroupId = identity.CompanyGroupId;
                base.Insert(entity);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetPK()
        {
            return GetAutoNumber("EntityAllowance", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void CheckUniqeCombineRow(EntityAllowance entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var db_Data = base.Query(t => t.Id != entity.Id && t.EntityId == entity.EntityId && t.DesignationGroupId == entity.DesignationGroupId && t.CompanyGroupId == identity.CompanyGroupId).Select().FirstOrDefault();
            if (db_Data != null)
                throw new CustomException("This combination already exist....!");
        }

        public override void Update(EntityAllowance entity)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                UpdateGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void Delete(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                base.Delete(Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT * FROM ORG.EntityAllowance ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT PPC.Id AS Value, PPC.UserName AS Text FROM ORG.EntityAllowance AS PPC
                                 ORDER BY PPC.UserName ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetEffectiveDateList(GridParameter parameters, string companyGroupId, string entityId, string designationId)
        {
            try
            {
                parameters.CmdText = @"SELECT EA.*,C.Code AS CurrencyName,E.UserName AS EntityName FROM [ORG].[EntityAllowance] AS EA
                                LEFT OUTER JOIN SCS.Currency AS C ON EA.CurrencyId=C.Id
								LEFT OUTER JOIN ORG.Entity AS E ON EA.EntityId = E.Id
                                WHERE EA.EntityId='" + entityId + "' AND EA.DesignationGroupId='" + designationId + "' AND EA.CompanyGroupId='" + companyGroupId + @"'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}