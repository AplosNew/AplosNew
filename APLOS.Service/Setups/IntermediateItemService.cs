#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Setups;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Setups
{
    public class IntermediateItemService : Service<IntermediateItem>, IIntermediateItemService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<CompanyGroupIntermediateItem> _companyGroupIntermediateItemRepository;

        public IntermediateItemService(
            IRepositoryAsync<IntermediateItem> IntermediateItemRepository,
            IRepositoryAsync<CompanyGroupIntermediateItem> companyGroupIntermediateItemRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(IntermediateItemRepository, unitOfWork)
        {
            _companyGroupIntermediateItemRepository = companyGroupIntermediateItemRepository;
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region GetAutoSequence

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Query auto sequence number. </summary>
        /// <returns>   decimal. </returns>
        ///-------------------------------------------------------------------------------------------------
        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        #endregion GetAutoSequence

        #region GetDayList

        public GridModel GetCbo(string companyGroupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                //var sql = $"SELECT um.Id AS Value, um.UserName as Text FROM {DbSchema.Setup}.[{DbTable.IntermediateItem}] AS um " +
                //          $"left outer join(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupWiseUOM}] WHERE CompanyGroupId = '{identity.CompanyGroupId}') cgu " +
                //          $"ON um.Id = cgu.UOMId  WHERE ISNULL(cgu.Id, '')<> '' AND  um.Archive=0 ";

                var sql = @"SELECT B.Id Value,B.UserName Text FROM [HKP].[IntermediateItem] B
                            LEFT JOIN HKP.CompanyGroupIntermediateItem CB ON B.Id=CB.IntermediateItemId
                            WHERE  B.Archive =0 AND B.Active=1 AND CB.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
        }

        #endregion GetDayList

        #region InsertUpdate

        /// <summary>
        /// CompanyFYPeriod Insert.
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(IntermediateItem entity)
        {
            var flag = false;
            try
            {
                CheckUnique(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetAutoId();
                InsertGraph(entity);

                CompanyGroupIntermediateItem comgroupuom = new CompanyGroupIntermediateItem
                {
                    Id = GetAutoComGBDepartmentId(),
                    IntermediateItemId = entity.Id,
                    CompanyGroupId = identity.CompanyGroupId
                };
                AuditService.AddedLog(comgroupuom);
                _companyGroupIntermediateItemRepository.Insert(comgroupuom);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetAutoId()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(IntermediateItem), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private string GetAutoComGBDepartmentId()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(CompanyGroupIntermediateItem), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(IntermediateItem entity)
        {
            try
            {
                CheckUnique(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
        }

        #endregion InsertUpdate

        private void CheckUnique(IntermediateItem entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && !r.Archive);
        }

        public void DeleteGraph(string key)
        {
            var flag = false;

            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                IntermediateItem IntermediateItem = Find(key);
                _companyGroupIntermediateItemRepository.Delete(key);
                base.DeleteGraph(IntermediateItem);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                parameters.CmdText = @"SELECT B.* FROM [HKP].[IntermediateItem] B
                                        LEFT JOIN HKP.CompanyGroupIntermediateItem CB ON B.Id=CB.IntermediateItemId
                                        WHERE B.Archive =0 AND B.Active=1 AND CB.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }
    }
}