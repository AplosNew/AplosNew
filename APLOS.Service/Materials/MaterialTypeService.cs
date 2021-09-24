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
    public partial class MaterialTypeService : Service<MaterialType>, IMaterialTypeService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        //private readonly IRepositoryAsync<MaterialTypeNature> _materialTypeNatureRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;

        public MaterialTypeService(
            IRepositoryAsync<MaterialType> materialRepository,
            //IRepositoryAsync<MaterialTypeNature> materialTypeNatureRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(materialRepository, unitOfWork, pkGeneratorService)
        {
           // _materialTypeNatureRepository = materialTypeNatureRepository;
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public override void Insert(MaterialType entity)
        {
          
            try
            {
                CheckUnique(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.Id = GetPK();
                entity.CompanyGroupId = identity.CompanyGroupId;
               
                base.Insert(entity);
                
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
              
            }
        }

        //public void InsertGraph(MaterialType entity, IEnumerable<MaterialTypeNature> materialTypeNatureList)
        //{
        //    var flag = false;
        //    try
        //    {
        //        _unitOfWork.BeginTransaction();
        //        flag = true;
        //        CheckUnique(entity);
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        entity.Id = GetPK();
        //        entity.CompanyGroupId = identity.CompanyGroupId;
        //        foreach (var item in materialTypeNatureList)
        //        {
        //            if (item.Flag)
        //                MaterialTypeNatureInsertGraph(entity, item);
        //        }

        //        base.InsertGraph(entity);
        //        _unitOfWork.SaveChanges();
        //        flag = false;
        //        _unitOfWork.Commit();
        //    }
        //    catch (CustomException)
        //    {
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
        //        entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
        //    }
        //    finally
        //    {
        //        if (flag)
        //            _unitOfWork.Rollback();
        //    }
        //}

        //public void UpdateGraph(MaterialType entity, IEnumerable<MaterialTypeNature> materialTypeNatureList)
        //{
        //    var flag = false;
        //    try
        //    {
        //        CheckUnique(entity);

        //        if (materialTypeNatureList.Any(t => t.Flag))
        //        {
        //            _unitOfWork.BeginTransaction();
        //            flag = true;
        //            foreach (var item in materialTypeNatureList)
        //            {
        //                if (item.Flag && string.IsNullOrEmpty(item.Id))
        //                    MaterialTypeNatureInsertGraph(entity, item);
        //                else if (!item.Flag && !string.IsNullOrEmpty(item.Id))
        //                    _materialTypeNatureRepository.Delete(item);
        //            }
        //            base.UpdateGraph(entity);
        //            _unitOfWork.SaveChanges();
        //            flag = false;
        //            _unitOfWork.Commit();
        //        }
        //        else throw new CustomException("Please check at least one nature.");
        //    }
        //    catch (CustomException)
        //    {
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
        //        entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
        //    }
        //    finally
        //    {
        //        if (flag)
        //            _unitOfWork.Rollback();
        //    }
        //}
        public override void Update(MaterialType entity)
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
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
              
            }
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                var masterData = Find(id);
                //var childData = _materialTypeNatureRepository.Query(t => t.MaterialTypeId == id).Select().ToList();
                //foreach (var item in childData)
                //{
                //    _materialTypeNatureRepository.Delete(item);
                //}
                base.DeleteGraph(masterData);
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(MaterialType), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private string GetMaterialTypeNaturePK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(MaterialTypeNature), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void CheckUnique(MaterialType entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
            //CheckUniqueColumn(UniqueColumnName.Prefix, entity.Prefix, r => r.Prefix == entity.Prefix && r.Id != entity.Id && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
            //CheckUniqueColumn(UniqueColumnName.Description, entity.Description, r => r.Description == entity.Description && r.Id != entity.Id && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
        }
        
        //private void MaterialTypeNatureInsertGraph(MaterialType entity, MaterialTypeNature item)
        //{
        //    item.Id = GetMaterialTypeNaturePK();
        //    item.MaterialTypeId = entity.Id;
        //    AuditService.AddedLog(item);
        //    _materialTypeNatureRepository.Insert(item);
        //}

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

        public override IQueryFluent<MaterialType> Query()
        {
            return base.Query(r => !r.Archive);
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return from m in base.Query(m => !m.Archive && m.Active && m.CompanyGroupId == identity.CompanyGroupId).Select().OrderBy(m => m.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<object> GetCboFilterBySFG()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return from m in base.Query(m => !m.Archive && m.Active && m.CompanyGroupId == identity.CompanyGroupId).Select().OrderBy(m => m.UserName)
                       //join c in _materialTypeNatureRepository.Query(t => t.Nature == EnumMaterialTypeNatureList.SemiFinishedGoods.ToString()).Select() on m.Id equals c.MaterialTypeId
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }
        //TODO:Shift to enum
        public IEnumerable<object> GetMaterialTypeNatureListCbo()
        {
            return Enum.GetValues(typeof(EnumMaterialTypeNatureList)).Cast<EnumMaterialTypeNatureList>().Select(v => new
            {
                Id = "",
                Text = v.ToString(),
                Nature = v.ToString(),
                Flag = false
            });
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = $"SELECT * " +
                          $"FROM  {DbSchema.HKP}.[{DbTable.MaterialType}] WHERE CompanyGroupId='{identity.CompanyGroupId}' AND Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        /// <summary>
        /// This cbo go to fabric roll management.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ComboModel> GetCboByMaterialMaster(string companyGroupId)
        {
            var _sql = @"SELECT Id,[Description] FROM HKP.MaterialType WHERE Id IN (SELECT DISTINCT MaterialTypeId FROM MST.MaterialMaster WHERE CompanyGroupId='" + companyGroupId + "') ORDER BY [Description]";
            return _sqlRepository.GetCombo(_sql, "Id", "Description");
        }

        //public IEnumerable<object> GetMaterialTypeNatureList(string masterId)
        //{
        //    try
        //    {
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        var sql = @"SELECT Id, Nature, Nature AS Text,CAST(1 AS BIT) Flag FROM HKP.MaterialTypeNature WHERE MaterialTypeId='" + masterId + "'";
        //        return _sqlRepository.GetDataCollection(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
        //    }
        //}
    }
}