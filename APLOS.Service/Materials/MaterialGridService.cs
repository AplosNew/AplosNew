#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Materials;
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

namespace Library.Service.Materials
{
    public class MaterialGridService : Service<MaterialGrid>, IMaterialGridService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IMaterialGridCharacteristicsService _materialGridCharacteristicsService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<MaterialGrid> _materialGridRepository;

        public MaterialGridService(
            IRepositoryAsync<MaterialGrid> materialGridRepository,
            IPKGeneratorService pkGeneratorService,
            IMaterialGridCharacteristicsService materialGridCharacteristicsService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(materialGridRepository, unitOfWork)
        {
            _materialGridRepository = materialGridRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _materialGridCharacteristicsService = materialGridCharacteristicsService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00m;
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = $"SELECT Id, CompanyGroupId,	Sequence, Code, ShortName, StandardName, UserName, Description, Remarks, Active, Archive FROM {DbSchema.HKP}.[{DbTable.MaterialGrid}] WHERE CompanyGroupId='{identity.CompanyGroupId}' AND Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="companyGroupId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public GridModel GetMaterialGridListWithoutExisting(GridParameter parameters, string companyGroupId, string[] ids)
        {
            try
            {
                var id = "";
                if (ids.Length > 0)
                    id = string.Join(",", ids.Select(item => "'" + item + "'"));
                else
                    id = "' '";
                parameters.order = "asc";
                parameters.sort = "UserName";
                parameters.CmdText = @"SELECT Id
	                                  ,UserName
	                                  ,[Description]
	                                  ,Active
                                FROM HKP.MaterialGrid
                                WHERE CompanyGroupId='" + companyGroupId + "' AND Archive=0 AND Id NOT IN (" + id + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void Insert(MaterialGrid entity, IEnumerable<MaterialGridCharacteristics> materialGridCharacteristics, string[] deletedItems)
        {
            var flag = false;
            try
            {
                CheckUnique(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPK();
                    entity.CompanyGroupId = identity.CompanyGroupId;
                    InsertGraph(entity);
                }
                else
                {
                    entity.CompanyGroupId = identity.CompanyGroupId;
                    UpdateGraph(entity);
                }
                _materialGridCharacteristicsService.Insert(materialGridCharacteristics, entity.Id, deletedItems);

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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                entity.AddedBy, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void CheckUnique(MaterialGrid entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.Description, entity.Description, r => r.Description == entity.Description && r.Id != entity.Id && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
        }

        private string GetPK()
        {
            return "MG" + _pkGeneratorService.GetAutoNumber(nameof(MaterialGrid), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(MaterialGrid entity)
        {
            try
            {
                CheckUnique(entity);
                base.Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                entity.AddedBy, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets all items in this collection. </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to process all items in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override IQueryFluent<MaterialGrid> Query()
        {
            return base.Query(r => !r.Archive);
        }

        public IEnumerable<object> GetMaterialGridList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return from m in base.Query(m => m.CompanyGroupId == identity.CompanyGroupId && m.Active && !m.Archive).Select()
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteGraph(string key)
        {
            var flag = false;
            try
            {
                CheckIdUse(key);
                _unitOfWork.BeginTransaction();
                flag = true;
                MaterialGrid entity = Find(key);
                base.DeleteGraph(entity);
                _materialGridCharacteristicsService.DeleteGraph(entity.Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void CheckIdUse(string id)
        {
            string sql = $"IF EXISTS(SELECT 1 FROM( " +
                            $"SELECT MaterialGridId AS CheckingColumn FROM {DbSchema.Masters}.[{DbTable.MaterialMaster}] WHERE Archive=0 " +
                            $") A WHERE CheckingColumn = '{id}') SELECT 1 ELSE SELECT 0 RETURN ";
            var data = Convert.ToBoolean(_materialGridRepository.SqlQuery<int>(sql).Single());
            if (data)
                throw new CustomException("Already grid exist in material master, you can't delete....!");
        }
    }
}