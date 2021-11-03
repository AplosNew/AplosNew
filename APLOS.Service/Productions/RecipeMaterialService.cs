#region Using

using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Productions;
using Library.Service.Core;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Threading;
using Library.Core;
using Library.Data;
#endregion Using

namespace Library.Service.Productions
{
    public class RecipeMaterialService : Service<RecipeMaterial>, IRecipeMaterialService
    {
        #region Table Name

        private readonly string tRecipeRawMaterial = " " + DbSchema.Transaction + ".[" + DbTable.RecipeRawMaterial + "] ";
        private readonly string tUnitOfMeasurement = " " + DbSchema.SystemConfigurationAndSetup + ".[UnitOfMeasurement] ";
        private readonly string tMaterialMaster = " " + DbSchema.Masters + ".[" + DbTable.MaterialMaster + "] ";

        #endregion Table Name

        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        private readonly IRepositoryAsync<RecipeMaterial> _recipeMaterialRepository;

        public RecipeMaterialService(
            IRepositoryAsync<RecipeMaterial> RecipeRawMaterialRepository,
            IPKGeneratorService pkGeneratorService,
            IRepositoryAsync<RecipeMaterial> recipeMaterialRepository,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(RecipeRawMaterialRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;

            _recipeMaterialRepository = recipeMaterialRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetRecipeMaterialListNew(string masterId)
        {
            try
            {
                var sql = @"SELECT BM.Id, BM.RecipeGlobalMasterId, BM.MaterialMasterId, MM.UserName AS MaterialMasterName
                         , MT.[Description] AS MaterialTypeName, MGP.UserName AS MaterialGroupMasterName, 
						 PM.UserName AS ProductMasterName, MM.Code, MM.UserName AS MaterialMasterName, BM.ArticleId, MMA.StandardName as 'ArticleName', MMA.Code as 'ArticleCode'
                        FROM [TRN].[RecipeMaterial] AS BM
                        LEFT JOIN [MST].[MaterialMaster] AS MM ON BM.MaterialMasterId=MM.Id
                        LEFT JOIN [TRN].[ProductDefinition] AS PD ON PD.MaterialMasterId= MM.Id
                        LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                        LEFT JOIN[HKP].[MaterialType] AS MT ON MGP.MaterialTypeId = MT.Id
                        LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS MMA ON MMA.Id = BM.ArticleId
						WHERE BM.RecipeGlobalMasterId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                throw;
                //throw new CustomException(ex.Message, ex,
                //    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                //    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetRecipeCbo(string entityId)
        {
            var sql = @"select Id, UserName from TRN.RecipeGlobalMaster where EntityId=" + entityId + " ORDER BY UserName";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public void DeleteRecipeMaterial(string id)
        {
            try
            {
                var data = _recipeMaterialRepository.Find(id);

                if (data.IsNotNull())
                {
                    _recipeMaterialRepository.Delete(data);
                    _unitOfWork.SaveChanges();
                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                //throw new CustomException(ex.Message, ex,
                //    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                //    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public string GetPK()
        {
            return "RR" + _pkGeneratorService.GetAutoNumber(nameof(RecipeMaterial), PKGeneratorEnum.Auto, null, DateTime.Now);
        }


        public override void Insert(RecipeMaterial entity)
        {
            try
            {
                if (entity != null)
                {
                    entity.Id = GetPK();
                    base.Insert(entity);
                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                //throw new CustomException(ex.Message, ex,
                //Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                //ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public bool ShouldValidation(string RecipeGlobalMasterId,string MaterialMasterId, string articleId)
        {
            try
            {
                var wc = "";
                if (string.IsNullOrEmpty(articleId))
                {
                    wc = @" where MaterialMasterId='" + MaterialMasterId + "'";
                }
                else
                {
                    wc = @" WHERE MaterialMasterId='" + MaterialMasterId + @"' AND ArticleId = '"+ articleId + "'";
                }
               var _sql= @" SELECT ArticleId FROM TRN.RecipeMaterial "+wc+"";
                    //mm has no attri
              var list=  _sqlRepository.GetDataCollection(_sql, null);

                if(list.Count>0)
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

        public string GetMaterialAtricleName(string RecipeGlobalMasterId, string MaterialMasterId, string articleId)
        {
            try
            {
                var wc = "";
                if (string.IsNullOrEmpty(articleId))
                {
                    wc = @" where RM.MaterialMasterId='" + MaterialMasterId + "'";
                }
                else
                {
                    wc = @" WHERE RM.MaterialMasterId='" + MaterialMasterId + @"' AND RM.ArticleId = '" + articleId + "'";
                }
                var _sql = @"SELECT MGP.UserName FROM TRN.RecipeMaterial RM
                         LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id=RM.ArticleId   
                        LEFT JOIN MST.MaterialMaster as MM on MM.Id= RM.MaterialMasterId 
                        LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                        " + wc + "";
                //mm has no attri
                var list = _sqlRepository.GetDataCollection(_sql, null);
                var name= list[0]["UserName"].ToString();
                if (list.Count > 0)
                {
                    return name;
                }
                return name;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<RecipeMaterial> GetDetailList(string RecipeSubprocessId)
        {
            try
            {
                var _sql = "select * from " + tRecipeRawMaterial + " where RecipeSubprocessId='" + RecipeSubprocessId + "'";
                return _sqlRepository.GetModelCollection<RecipeMaterial>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<RecipeMaterial> GetDetailListByMasterId(string RecipeMasterId)
        {
            try
            {
                var _sql = "select * from " + tRecipeRawMaterial + " where RecipeMasterId='" + RecipeMasterId + "'";
                return _sqlRepository.GetModelCollection<RecipeMaterial>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetList(string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"select r.Id,mm.Description MaterialMaster,r.Qty,u.UserName Uom,r.Ispercentage,r.Remark,r.UomId,r.RawMaterialId
                                    ,r.RecipeMasterId,r.RecipeSubprocessId from " + tRecipeRawMaterial + @" r
                                    left outer join " + tUnitOfMeasurement + @" u on r.UomId=u.Id
                                    left outer join " + tMaterialMaster + @" mm on r.RawMaterialId=mm.Id
                                        where  r.RecipeSubprocessId='" + MasterId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetDetailById(string pk)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"select r.Id,mm.Description MaterialMasterDescription,mm.Code MaterialMasterCode,r.RawMaterialId MaterialMasterId,r.Qty,u.UserName Uom,r.Ispercentage,r.Remark,r.UomId,r.RawMaterialId
                                    ,r.RecipeMasterId,r.RecipeSubprocessId from " + tRecipeRawMaterial + @" r
                                    left outer join " + tUnitOfMeasurement + @" u on r.UomId=u.Id
                                    left outer join " + tMaterialMaster + @" mm on r.RawMaterialId=mm.Id
                                        where  r.Id='" + pk + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}