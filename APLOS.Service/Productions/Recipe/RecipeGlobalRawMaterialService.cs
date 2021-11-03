#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Productions.Recipe;
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

namespace Library.Service.Productions.Recipe
{
    public class RecipeGlobalRawMaterialService : Service<RecipeGlobalRawMaterial>, IRecipeGlobalRawMaterialService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<RecipeGlobalRawMaterial> _recipeSubprocessRepository;
        private readonly IRepositoryAsync<RecipeGlobalMaterialGroup> _recipeGlobalMaterialGroupRepository;

        public ModelState ModelState { get; private set; }

        public RecipeGlobalRawMaterialService(
            IRepositoryAsync<RecipeGlobalRawMaterial> RecipeSubprocessRepository
            , IRepositoryAsync<RecipeGlobalMaterialGroup> recipeGlobalMaterialGroupRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(RecipeSubprocessRepository, unitOfWork, pkGeneratorService)
        {
            _recipeSubprocessRepository = RecipeSubprocessRepository;
            _recipeGlobalMaterialGroupRepository = recipeGlobalMaterialGroupRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public bool ShouldValidation(string RecipeGlobalMasterId, string MaterialMasterId, string articleId, string subpProcessId)
        {
            try
            {
                var wc = "";
                if (string.IsNullOrEmpty(articleId))
                {
                    wc = @" AND MaterialMasterId='" + MaterialMasterId + "'";
                }
                else
                {
                    wc = @" AND ArticleId = '" + articleId + "'";
                }
                var _sql = @" SELECT ArticleId FROM TRN.RecipeGlobalRawMaterial WHERE RecipeGlobalSubprocessId='" + subpProcessId + @"' AND RecipeGlobalMasterId='" + RecipeGlobalMasterId + @"' " + wc + "";
                //mm has no attri
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

        public bool RecipeGlobalMaterialGroupValidation(string subpProcessId, string recipeMaterialGroupingMasterId)
        {
            try
            {
                var _sql = @"SELECT Id FROM TRN.RecipeGlobalMaterialGroup WHERE RecipeGlobalSubprocessId='" + subpProcessId + @"' AND RecipeMaterialGroupingMasterId='"+ recipeMaterialGroupingMasterId + "'";
                //mm has no attri
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

        public string GetPK()
        {
            return "RWR" + _pkGeneratorService.GetAutoNumber(nameof(RecipeGlobalRawMaterial), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public RecipeGlobalRawMaterial GetDetail(string PK)
        {
            try
            {
                string _sql = "select * from trn.RecipeGlobalRawMaterial where Id='" + PK + "' ";
                return _recipeSubprocessRepository.SelectQuery(_sql, null).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<RecipeGlobalRawMaterial> GetDetailList(string SubprocessId)
        {
            //getbulletindetaillist
            try
            {
                string _sql = "select * from trn.RecipeGlobalRawMaterial where RecipeGlobalSubprocessId='" + SubprocessId + "'";
                return _sqlRepository.GetModelCollection<RecipeGlobalRawMaterial>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<RecipeGlobalRawMaterial> GetListOperationId(string OperationId)
        {
            //getbulletindetaillist
            try
            {
                string _sql = "select * from trn.RecipeGlobalRawMaterial where RecipeGlobalSubprocessId='" + OperationId + "'";
                return _sqlRepository.GetModelCollection<RecipeGlobalRawMaterial>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<RecipeGlobalRawMaterial> GetListUtilityId(string UtilityId)
        {
            //getbulletindetaillist
            try
            {
                string _sql = "select * from trn.RecipeGlobalRawMaterial where RecipeGlobalUtilityId='" + UtilityId + "'";
                return _sqlRepository.GetModelCollection<RecipeGlobalRawMaterial>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetList(string UtilityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"select
                                    r.Id
                                    ,o.UserName Operation
                                    ,u.UserName Utility
                                    ,mm.UserName MaterialMaster
                                    ,r.QtyValue
                                    ,uom.UserName Uom
                                    ,r.IsFixed
                                    ,r.Remark
                                    ,r.MaterialMasterId
                                    ,r.RecipeGlobalMasterId
                                    ,r.OperationId
                                    ,r.RecipeGlobalOperationId
                                    ,r.RecipeGlobalSubprocessId
                                    ,r.RecipeGlobalUtilityId
                                    ,r.SubprocessId
                                    ,r.UomId
                                    ,r.UtilityId
                                    from [TRN].[RecipeGlobalRawMaterial] r
                                    left outer join mst.Operation o on o.Id=r.OperationId
                                    left outer join hkp.Utility u on u.Id=r.UtilityId
                                    left outer join mst.MaterialMaster mm on mm.Id=r.MaterialMasterId
                                    left outer join scs.UnitOfMeasurement uom on uom.Id=r.UomId
                                     Where r.UtilityId='" + UtilityId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetMaterialMaster(string mmid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"SELECT MT.Description AS MaterialType
	                                                ,MGP.UserName AS MaterialGroupMaster
	                                                ,MM.Code
	                                                ,MM.UserName
	                                                ,MG.[Description] AS GridName
	                                                ,PM.UserName AS ProductMaster
	                                                ,UOMB.UserName AS BaseUom
	                                                ,MM.StandardName
	                                                ,MM.ShortName
	                                                ,MM.[Description]
	                                                ,MM.Id
	                                                ,MM.MaterialGridId
	                                                ,MM.BaseUOMId
                                                FROM [MST].[MaterialMaster] AS MM
                                                LEFT JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                                                LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                                                LEFT JOIN [HKP].[MaterialGrid] AS MG ON MM.MaterialGridId = MG.Id
                                                LEFT JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
                                                INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                                                WHERE MM.CompanyGroupId = '" + identity.CompanyGroupId + @"'
	                                                AND MM.Archive = 0
	                                                AND MM.Active = 1
	                                                AND MM.Id = '" + mmid + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetDetailById(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"select
                                    r.Id
                                    ,o.UserName Operation
                                    ,u.UserName Utility
                                    ,mm.UserName MaterialMaster
                                    ,r.QtyValue
                                    ,uom.UserName Uom
                                    ,r.IsFixed
                                    ,r.Remark
                                    ,r.MaterialMasterId
                                    ,r.RecipeGlobalMasterId
                                    ,r.OperationId
                                    ,r.RecipeGlobalOperationId
                                    ,r.RecipeGlobalSubprocessId
                                    ,r.RecipeGlobalUtilityId
                                    ,r.SubprocessId
                                    ,r.UomId
                                    ,r.UtilityId
                                    from [TRN].[RecipeGlobalRawMaterial] r
                                    left outer join mst.Operation o on o.Id=r.OperationId
                                    left outer join hkp.Utility u on u.Id=r.UtilityId
                                    left outer join mst.MaterialMaster mm on mm.Id=r.MaterialMasterId
                                    left outer join scs.UnitOfMeasurement uom on uom.Id=r.UomId
                                     Where r.Id='" + id + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CreateRecipeRawMaterial(RecipeGlobalRawMaterial ui_ob)
        {
            RecipeGlobalRawMaterial db_ob = null;
            var flag = false;
            try
            {
                ///subprocess
                OutDetail(ui_ob, out db_ob);
                InsertOrUpdateGraph(db_ob);
                //validation
                //detailList = _recipesubprocessservice.GetList(localSubprocess.RecipeGlobalMasterID);//get all child for this master
                _unitOfWork.BeginTransaction();
                flag = true;
                // _recipesubprocessservice.CheckDuplicate(localSubprocess, detailList);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void CreateRecipeGlobalMaterialGroup(RecipeGlobalMaterialGroup entity)
        {
            try
            {
                if (entity.Id.IsNullOrEmpty())
                {
                    entity.Id = GetAutoNumber(nameof(RecipeGlobalMaterialGroup), PKGeneratorEnum.Auto, null, DateTime.Now);
                    ModelState = ModelState.Added;
                    AuditService.AddedLog(entity);
                    _recipeGlobalMaterialGroupRepository.Insert(entity);
                }
                else
                {
                    ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(entity);
                    _recipeGlobalMaterialGroupRepository.Update(entity);
                }
                _unitOfWork.SaveChanges();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void DeleteRecipeGlobalMaterialGroup(string id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = _recipeGlobalMaterialGroupRepository.Find(id);
                if (data != null)
                {
                    _recipeGlobalMaterialGroupRepository.Delete(data);
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

        public void OutDetail(RecipeGlobalRawMaterial from_ui, out RecipeGlobalRawMaterial from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //from_db = GetDetail(from_ui.Id);
                ///Check Duplicate
                //detailList = _recipesubprocessservice.GetList(from_ui.RecipeMasterID);//get all child for this master
                //CheckDuplicateSubprocess(from_ui, detailList);

                if (from_db == null || from_db.Id == null || from_db.Id == "")
                {
                    from_db = new RecipeGlobalRawMaterial
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(from_db);

                    #region Add

                    from_db.Id = GetPK();//set pk

                    from_db.RecipeGlobalMasterId = from_ui.RecipeGlobalMasterId;
                    from_db.RecipeGlobalOperationId = from_ui.RecipeGlobalOperationId;
                    from_db.RecipeGlobalSubprocessId = from_ui.RecipeGlobalSubprocessId;
                    from_db.RecipeGlobalUtilityId = from_ui.RecipeGlobalUtilityId;
                    from_db.SubprocessId = from_ui.SubprocessId;
                    from_db.UtilityId = from_ui.UtilityId;

                    #endregion Add
                }
                else
                {
                    #region Edit

                    from_db.ModelState = ModelState.Modified;
                    AuditService.Log(from_db);

                    #endregion Edit
                }

                from_db.IsOperationLevel = from_ui.IsOperationLevel;
                from_db.IsFixed = from_ui.IsFixed;
                from_db.MaterialMasterId = from_ui.MaterialMasterId;
                from_db.ArticleId = from_ui.ArticleId;
                from_db.QtyValue = from_ui.QtyValue;
                from_db.UomId = from_ui.UomId;
                from_db.Remark = from_ui.Remark;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CheckDuplicate(RecipeGlobalRawMaterial detail_ui, IEnumerable<object> from_db_detailList)
        {
            try
            {
                foreach (var item in from_db_detailList)
                {
                    var dic = (Dictionary<string, object>)item;
                    if (dic["Id"].ToString() != detail_ui.Id)
                    {
                        if (dic["SubprocessId"].ToString() == detail_ui.MaterialMasterId)
                        {
                            throw new Exception("Subprocess: [" + dic["Subprocess"] + "] has already been taken...");
                        }
                    }//id
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DelRawMaterialList(string subprocessid, out IEnumerable<RecipeGlobalRawMaterial> from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetDetailList(subprocessid);
                foreach (var ui in from_db)
                {
                    ui.ModelState = ModelState.Deleted;
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DelRawMaterialListByOperationId(string OperationId, out IEnumerable<RecipeGlobalRawMaterial> from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetListOperationId(OperationId);
                foreach (var ui in from_db)
                {
                    ui.ModelState = ModelState.Deleted;
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DelRawMaterialListByUtilityId(string UtilityId, out IEnumerable<RecipeGlobalRawMaterial> from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetListUtilityId(UtilityId);
                foreach (var ui in from_db)
                {
                    ui.ModelState = ModelState.Deleted;
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteRawMaterial(string rawMaterialId)
        {
            RecipeGlobalRawMaterial from_db = null;
            var flag = false;
            try
            {
                //master
                DelRawMaterial(rawMaterialId, out from_db);
                Delete(from_db);

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void DelRawMaterial(string id, out RecipeGlobalRawMaterial from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = Find(id);

                if (from_db.Id == null || from_db.Id == "")
                {
                    throw new Exception("No Row found against Id: [" + id + "]");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}