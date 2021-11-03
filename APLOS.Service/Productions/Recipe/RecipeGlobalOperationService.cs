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
    public class RecipeGlobalOperationService : Service<RecipeGlobalOperation>, IRecipeGlobalOperationService
    {
        #region Constructor

        /// <summary>   The unit of work. </summary>
        private readonly IUnitOfWork _unitOfWork;

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRecipeGlobalUtilityService _recipeGlobalutilityservice;//IRecipeSubprocessService
        private readonly IRecipeGlobalRawMaterialService _reciperawmaterialservice;//IRecipeSubprocessService

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        /// <param name="SalesOrderMasterRepository">    The repArea. </param>
        /// <param name="unitOfWork">   The unit of work. </param>
        ///-------------------------------------------------------------------------------------------------
        public RecipeGlobalOperationService(
            IRepositoryAsync<RecipeGlobalOperation> RecipeSubprocessRepository, IPKGeneratorService pkGeneratorService,
            IRecipeGlobalUtilityService recipeGlobalutilityservice,
            IRecipeGlobalRawMaterialService reciperawmaterialservice,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(RecipeSubprocessRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _reciperawmaterialservice = reciperawmaterialservice;
            _recipeGlobalutilityservice = recipeGlobalutilityservice;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<ComboModel> GetGlobalOperationCbo(string RecipeGlobalSubprocessId)
        {
            try
            {
                string _sql = @"select
                                o.Id ,p.UserName
                                from
                                trn.RecipeGlobalOperation o
                                left outer join mst.Operation p on p.Id=o.OperationId
                                where o.RecipeGlobalSubprocessId='" + RecipeGlobalSubprocessId + "'";
                return _sqlRepository.GetCombo(_sql, "Id", "UserName");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetPK()
        {
            return "RWO" + _pkGeneratorService.GetAutoNumber(nameof(RecipeGlobalOperation), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<RecipeGlobalOperation> GetDetailList(string MasterId)
        {
            //getbulletindetaillist
            try
            {
                string _sql = "select * from trn.RecipeGlobalOperation where RecipeGlobalSubprocessId='" + MasterId + "'";
                return _sqlRepository.GetModelCollection<RecipeGlobalOperation>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetList(string subprocessid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"select s.Id,sp.UserName Operation,s.Sequence ,s.RecipeGlobalMasterId,s.RecipeGlobalSubprocessId,s.SubprocessId
                                 from trn.RecipeGlobalOperation s
                                left outer join mst.Operation sp on sp.Id=s.OperationId
                                where s.SubprocessId='" + subprocessid + "' order by sp.UserName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetOperation(string pk)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"select s.*
                                 from trn.RecipeGlobalOperation s
                                where s.Id='" + pk + "'";
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
                var _sql = @"select s.Id,sp.UserName Subprocess,s.StartPressure,s.StartTemperature,s.EndTemperature,
                                s.EndPressure,s.Duration,s.GradientPressure,s.GradientTemperature,s.Remark,s.SubprocessId
                                 from trn.RecipeGlobalOperation s
                                left outer join hkp.SubProcess sp on sp.Id=s.SubprocessId
                                where s.Id='" + id + "' order by sp.UserName,s.Duration";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CreateRecipeOperation(RecipeGlobalOperation ui_ob)
        {
            RecipeGlobalOperation db_ob = null;
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

        public void OutDetail(RecipeGlobalOperation from_ui, out RecipeGlobalOperation from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = Find(from_ui.Id);
                ///Check Duplicate
                //detailList = _recipesubprocessservice.GetList(from_ui.RecipeMasterID);//get all child for this master
                //CheckDuplicateSubprocess(from_ui, detailList);

                if (from_db==null || from_db.Id == null || from_db.Id == "")
                {
                    from_db = new RecipeGlobalOperation
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(from_db);

                    #region Add

                    from_db.Id = GetPK();//set pk

                    from_db.OperationId = from_ui.OperationId;
                    from_db.RecipeGlobalMasterId = from_ui.RecipeGlobalMasterId;
                    from_db.RecipeGlobalSubprocessId = from_ui.RecipeGlobalSubprocessId;
                    from_db.Sequence = from_ui.Sequence;
                    from_db.SubprocessId = from_ui.SubprocessId;

                    #endregion Add
                }
                else
                {
                    #region Edit

                    from_db.ModelState = ModelState.Modified;
                    AuditService.Log(from_db);

                    from_db.OperationId = from_ui.OperationId;
                    from_db.RecipeGlobalMasterId = from_ui.RecipeGlobalMasterId;
                    from_db.RecipeGlobalSubprocessId = from_ui.RecipeGlobalSubprocessId;
                    from_db.Sequence = from_ui.Sequence;
                    from_db.SubprocessId = from_ui.SubprocessId;

                    #endregion Edit
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CheckDuplicate(RecipeGlobalOperation detail_ui, IEnumerable<object> from_db_detailList)
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

        public void DelOperationlList(string subprocessid, out IEnumerable<RecipeGlobalOperation> from_db)
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

        public void DeleteOperation(string OperationId)
        {
            RecipeGlobalOperation from_db = null;
            IEnumerable<RecipeGlobalUtility> u_fromdb = null;
            IEnumerable<RecipeGlobalRawMaterial> rm_fromdb = null;
            var flag = false;
            try
            {
                from_db = Find(OperationId);
                //_recipeGlobaloperationservice.DelOperationlList(detailid, out o_fromdb);
                _recipeGlobalutilityservice.DelUtilitylListByOperationId(OperationId, out u_fromdb);
                _reciperawmaterialservice.DelRawMaterialListByOperationId(OperationId, out rm_fromdb);

                // from_db.RecipeGlobalOperation = (ICollection<RecipeGlobalOperation>)o_fromdb;
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
    }
}