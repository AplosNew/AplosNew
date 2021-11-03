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
    public partial class MaterialGroup3Service : Service<MaterialGroup3>, IMaterialGroup3Service
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ICompanyGroupWiseMaterialGroup3Service _companyGroupWiseMaterialGroup3Service;
        private readonly ISqlRepository _sqlRepository;

        public MaterialGroup3Service(
            IRepositoryAsync<MaterialGroup3> materialGroup3Repository,
            IPKGeneratorService pkGeneratorService,
            ICompanyGroupWiseMaterialGroup3Service companyGroupWiseMaterialGroup3Service,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(materialGroup3Repository, unitOfWork, pkGeneratorService)
        {
            _pkGeneratorService = pkGeneratorService;
            _companyGroupWiseMaterialGroup3Service = companyGroupWiseMaterialGroup3Service;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public override void Insert(MaterialGroup3 entity)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                InsertGraph(entity);
                _companyGroupWiseMaterialGroup3Service.Insert(entity.Id);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(MaterialGroup3), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void Check(MaterialGroup3 entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && !r.Archive);
        }

        public override void Update(MaterialGroup3 entity)
        {
            try
            {
                Check(entity);
                UpdateGraph(entity);
                _unitOfWork.SaveChanges();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public override void Archive(string key)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                _companyGroupWiseMaterialGroup3Service.DeleteGraph(key);
                DeleteGraph(key);
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

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        public IEnumerable<object> GetCboList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = $"SELECT mg.Id AS Value, mg.UserName as Text FROM {DbSchema.HKP}.[{DbTable.MaterialGroup3}] AS mg " +
                          $"left outer join(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupWiseMaterialGroup3}] WHERE CompanyGroupId = '{identity.CompanyGroupId}') AS cgmg3 " +
                          $"ON mg.Id = cgmg3.MaterialGroup3Id  WHERE ISNULL(cgmg3.Id, '')<> '' AND  mg.Archive=0 AND mg.Active=1 ORDER BY mg.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = $"SELECT mg.* FROM {DbSchema.HKP}.[{DbTable.MaterialGroup3}] AS mg " +
                           $"left outer join(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupWiseMaterialGroup3}] WHERE CompanyGroupId = '{identity.CompanyGroupId}') AS cgmg3 " +
                           $"ON mg.Id = cgmg3.MaterialGroup3Id  WHERE ISNULL(cgmg3.Id, '')<> '' AND  mg.Archive=0 ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }
    }
}