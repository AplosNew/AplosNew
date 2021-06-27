#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Productions.Recipe;
using Library.Service.Core;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Threading;

#endregion Using

namespace Library.Service.Productions.Recipe
{
    public class RecipeGlobalSubprocessService : Service<RecipeGlobalSubprocess>, IRecipeGlobalSubprocessService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRecipeGlobalOperationService _recipeGlobaloperationservice;//IRecipeSubprocessService
        private readonly IRecipeGlobalUtilityService _recipeGlobalutilityservice;//IRecipeSubprocessService
        private readonly IRecipeGlobalRawMaterialService _reciperawmaterialservice;//IRecipeSubprocessService

        public RecipeGlobalSubprocessService(
            IRepositoryAsync<RecipeGlobalSubprocess> RecipeSubprocessRepository
            , IPKGeneratorService pkGeneratorService,
           IRecipeGlobalOperationService recipeGlobaloperationservice,
            IRecipeGlobalUtilityService recipeGlobalutilityservice,
            IRecipeGlobalRawMaterialService reciperawmaterialservice,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(RecipeSubprocessRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
            _reciperawmaterialservice = reciperawmaterialservice;
            _recipeGlobalutilityservice = recipeGlobalutilityservice;
            _recipeGlobaloperationservice = recipeGlobaloperationservice;
        }

        #endregion Constructor

        public string GetPK()
        {
            return "RWS" + _pkGeneratorService.GetAutoNumber(nameof(RecipeGlobalSubprocess), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<RecipeGlobalSubprocess> GetDetailList(string MasterId)
        {
            //getbulletindetaillist
            try
            {
                string _sql = "select * from TRN.RecipeGlobalSubprocess where RecipeMasterID='" + MasterId + "'";
                return _sqlRepository.GetModelCollection<RecipeGlobalSubprocess>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetList(string RecipeGlobalMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"SELECT DISTINCT tab1.*
                                   -- , ro.UserName as 'RecipeOperationName'
                                   FROM (SELECT S.Id, S.Description, S.RecipeOperationId
	                              ,M.UserName Subprocess
                                  ,S.RecipeGlobalMasterId
                                  ,S.SubprocessId
                                  ,S.[Sequence]
                                 ,(CONVERT(VARCHAR, CAST(S.LineItemValue as money)) +' '+U.UserName) ItemValue
                                  FROM [TRN].[RecipeGlobalSubprocess] s
                                 LEFT OUTER JOIN HKP.SubProcess m on m.Id=s.SubprocessId
                                 LEFT OUTER JOIN TRN.RecipeGlobalMaster RGM on RGM.Id=S.RecipeGlobalMasterId
                                 LEFT JOIN scs.UnitOfMeasurement u ON u.id = RGM.AvgUom
                                WHERE S.RecipeGlobalMasterId='" + RecipeGlobalMasterId + @"'
                                ) AS tab1  LEFT JOIN HKP.RecipeOperation RO ON RO.Id= tab1.RecipeOperationId order by [Sequence]";
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
                var _sql = @"select s.Id,sp.UserName Subprocess,s.SubprocessId,s.[Sequence],s.RecipeGlobalMasterId, s.RecipeOperationId, s.Description,S.LineItemValue
                                 from trn.RecipeGlobalSubprocess s
                                left outer join hkp.SubProcess sp on sp.Id=s.SubprocessId
                                where s.Id='" + id + "' order by sp.UserName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CreateRecipeSubprocess(RecipeGlobalSubprocess recipeSubprocess)
        {
            RecipeGlobalSubprocess localSubprocess = null;
            var flag = false;
            try
            {
                ///subprocess
                OutDetail(recipeSubprocess, out localSubprocess);
                InsertOrUpdateGraph(localSubprocess);
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

        public void OutDetail(RecipeGlobalSubprocess from_ui, out RecipeGlobalSubprocess from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = Find(from_ui.Id);
                ///Check Duplicate
                //detailList = _recipesubprocessservice.GetList(from_ui.RecipeMasterID);//get all child for this master
                //CheckDuplicateSubprocess(from_ui, detailList);

                if (from_db == null || from_db.Id == null || from_db.Id == "")
                {
                    from_db = new RecipeGlobalSubprocess
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(from_db);
                    from_db.Id = GetPK();//set pk
                    from_db.RecipeGlobalMasterId = from_ui.RecipeGlobalMasterId;
                    from_db.SubprocessId = from_ui.SubprocessId;
                    from_db.Sequence = from_ui.Sequence;

                    from_db.RecipeOperationId = from_ui.RecipeOperationId;
                    from_db.Description = from_ui.Description;
                    from_db.LineItemValue = from_ui.LineItemValue;
                }
                else
                {
                    from_db.ModelState = ModelState.Modified;
                    AuditService.Log(from_db);
                    from_db.SubprocessId = from_ui.SubprocessId;
                    from_db.Sequence = from_ui.Sequence;

                    from_db.RecipeOperationId = from_ui.RecipeOperationId;
                    from_db.Description = from_ui.Description;
                    from_db.LineItemValue = from_ui.LineItemValue;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CheckDuplicate(RecipeGlobalSubprocess detail_ui, IEnumerable<object> from_db_detailList)
        {
            try
            {
                foreach (var item in from_db_detailList)
                {
                    var dic = (Dictionary<string, object>)item;
                    if (dic["Id"].ToString() != detail_ui.Id)
                    {
                        if (dic["SubprocessId"].ToString() == detail_ui.SubprocessId)
                        {
                            throw new Exception("Subprocess: [" + dic["Subprocess"]+ "] has already been taken...");
                        }
                    }//id
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteDetail(string detailid)
        {
            RecipeGlobalSubprocess from_db = null;
            IEnumerable<RecipeGlobalOperation> o_fromdb = null;
            IEnumerable<RecipeGlobalUtility> u_fromdb = null;
            IEnumerable<RecipeGlobalRawMaterial> rm_fromdb = null;
            var flag = false;
            try
            {
                DelSubprocess(detailid, out from_db);
                _recipeGlobaloperationservice.DelOperationlList(detailid, out o_fromdb);
                _recipeGlobalutilityservice.DelUtilitylList(detailid, out u_fromdb);
                _reciperawmaterialservice.DelRawMaterialList(detailid, out rm_fromdb);

                from_db.RecipeGlobalOperation = (ICollection<RecipeGlobalOperation>)o_fromdb;
                from_db.RecipeGlobalUtility = (ICollection<RecipeGlobalUtility>)u_fromdb;
                from_db.RecipeGlobalRawMaterial = (ICollection<RecipeGlobalRawMaterial>)rm_fromdb;

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

        private void DelSubprocess(string id, out RecipeGlobalSubprocess from_db)
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
                else
                {
                    from_db.ModelState = ModelState.Deleted;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //del detail childlist (by subprocessid)
    }
}