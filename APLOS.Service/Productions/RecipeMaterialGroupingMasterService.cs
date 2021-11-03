#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Biometrics;
using Library.Model.Productions.Recipe;
using Library.Service.Attendances;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Productions
{
    public class RecipeMaterialGroupingMasterService : Service<RecipeMaterialGroupingMaster>, IRecipeMaterialGroupingMasterService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<RecipeMaterialGroupingDetail> _recipeMaterialGroupingDetailRepository;

        public ModelState ModelState { get; private set; }

        public RecipeMaterialGroupingMasterService(
            IRepositoryAsync<RecipeMaterialGroupingMaster> RecipeMaterialGroupingMasterRepository
            , IRepositoryAsync<RecipeMaterialGroupingDetail> recipeMaterialGroupingDetailRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(RecipeMaterialGroupingMasterRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _recipeMaterialGroupingDetailRepository = recipeMaterialGroupingDetailRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(RecipeMaterialGroupingMaster entity)
        {
            try
            {
                Check(entity);
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPK();
                    base.Insert(entity);
                }
                else
                {
                    Check(entity);
                    base.Update(entity);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
     
        public void CreateRecipeMaterialGroupingDetail(RecipeMaterialGroupingDetail entity)
        {
            try
            {
                if (entity.Id.IsNullOrEmpty())
                {
                    
                    entity.Id = GetAutoNumber(nameof(RecipeMaterialGroupingDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
                    ModelState = ModelState.Added;
                    AuditService.AddedLog(entity);
                    _recipeMaterialGroupingDetailRepository.Insert(entity);
                }
                else
                {
                    
                    ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(entity);
                    _recipeMaterialGroupingDetailRepository.Update(entity);
                }
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(RecipeMaterialGroupingMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void Check(RecipeMaterialGroupingMaster entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        }

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

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in base.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT U.UserName Uom, RM.*
                                        FROM Mst.RecipeMaterialGroupingMaster RM
                                        LEFT JOIN SCS.UnitOfMeasurement U ON U.Id = RM.UomId";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetRecipeMaterialGroupingDetailList(string masterId)
        {
            try
            {
                var _sql = @"SELECT D.*,M.UserName RecipeMaterialGroupingMaster,MM.UserName MaterialMasterName,MMA.StandardName ArticleName,U.UserName Description 
                           FROM [MST].[RecipeMaterialGroupingDetail] D
                           LEFT JOIN [MST].[RecipeMaterialGroupingMaster] M ON M.Id=D.RecipeMaterialGroupingMasterId
                           LEFT JOIN MST.MaterialMaster MM ON MM.Id=D.MaterialMasterId
                           LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id = D.ArticleId
                           LEFT JOIN SCS.UnitOfMeasurement U ON U.Id = D.UomId
                           WHERE M.Id='" + masterId + @"' ORDER BY MM.UserName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region    Delete
        public void Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new CustomException("Recipe is not found...");

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Find(id);
                if (data != null)
                {
                    _recipeMaterialGroupingDetailRepository.ExecuteSqlCommand("DELETE FROM [MST].[RecipeMaterialGroupingDetail] Where RecipeMaterialGroupingMasterId='" + id + "'");
                    base.Delete(data);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
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
        public void DeleteRawMaterial(string rawmaterialid)
        {
            //if (string.IsNullOrEmpty(rawmaterialid))
            //    throw new CustomException("Recipe is not found...");

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = _recipeMaterialGroupingDetailRepository.Find(rawmaterialid);
                if (data != null)
                {

                    _recipeMaterialGroupingDetailRepository.Delete(data);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
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

        public bool RecipeMaterialGroupingValidation(string RecipeMaterialGroupingMasterId,string articleId ,string MaterialMasterId)
        {
            try
            {
                var wc = "";
                if (string.IsNullOrEmpty(articleId))
                {
                    wc = @" RecipeMaterialGroupingMasterId='" + RecipeMaterialGroupingMasterId + @"' and MaterialMasterId='"+ MaterialMasterId + "'";
                }
                else
                {
                    wc = @" RecipeMaterialGroupingMasterId='" + RecipeMaterialGroupingMasterId + @"' and MaterialMasterId='" + MaterialMasterId + @"' AND ArticleId = '" + articleId + "'";
                }
                var _sql = @" SELECT Id FROM Mst.RecipeMaterialGroupingDetail  where " + wc + "";
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
        # endregion

    }
}