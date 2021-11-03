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
using System.Linq;
using System.Threading;

#endregion Using

namespace Library.Service.Productions.Recipe
{
    public class RecipeGlobalUtilityService : Service<RecipeGlobalUtility>, IRecipeGlobalUtilityService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRecipeGlobalRawMaterialService _irecipeGlobalrawmaterialservice;
        private readonly IRepositoryAsync<RecipeGlobalUtility> _recipeSubprocessRepository;

        public RecipeGlobalUtilityService(
            IRepositoryAsync<RecipeGlobalUtility> RecipeSubprocessRepository
            , IPKGeneratorService pkGeneratorService,
             IRecipeGlobalRawMaterialService reciperawmaterialservice,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(RecipeSubprocessRepository, unitOfWork, pkGeneratorService)
        {
            _recipeSubprocessRepository = RecipeSubprocessRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _irecipeGlobalrawmaterialservice = reciperawmaterialservice;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public string GetPK()
        {
            return "RWU" + _pkGeneratorService.GetAutoNumber(nameof(RecipeGlobalUtility), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public RecipeGlobalUtility GetDetail(string PK)
        {
            try
            {
                string _sql = "select * from trn.RecipeGlobalUtility where Id='" + PK + "' ";
                return _recipeSubprocessRepository.SelectQuery(_sql, null).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetUtility(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string _sql = "select * from trn.RecipeGlobalUtility where Id='" + id + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<RecipeGlobalUtility> GetDetailList(string SubprocessId)
        {
            //getbulletindetaillist
            try
            {
                string _sql = "select * from trn.RecipeGlobalUtility where RecipeGlobalSubprocessId='" + SubprocessId + "'";
                return _sqlRepository.GetModelCollection<RecipeGlobalUtility>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<RecipeGlobalUtility> GetListByOperationId(string OperationId)
        {
            //getbulletindetaillist
            try
            {
                string _sql = "select * from trn.RecipeGlobalUtility where RecipeGlobalOperationId='" + OperationId + "'";
                return _sqlRepository.GetModelCollection<RecipeGlobalUtility>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetList(string RecipeGlobalSubprocessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"select s.Id
                            ,s.RecipeGlobalMasterId
                            ,s.RecipeGlobalSubprocessId
                            ,s.RecipeGlobalOperationId
                            ,s.SubprocessId
							,s.UtilityId
                            ,o.UserName Operation
                            ,u.UserName Utility
                            ,s.QtyValue
                            ,m.UserName Uom
                            ,s.Uom UomId
                            ,s.Duration
                            ,s.Temperature
                            ,s.IsFixed
                            ,s.Ph
                            ,s.Sequence
                            ,s.Remark
                            from trn.RecipeGlobalUtility s
                            left outer join hkp.Utility u on u.Id=s.UtilityId
                            left outer join scs.UnitOfMeasurement m on m.Id=s.Uom
                            left outer join [HKP].[RecipeOperation] o on o.Id=s.RecipeGlobalOperationId
                            where s.RecipeGlobalSubprocessId='" + RecipeGlobalSubprocessId + @"'
                            order by o.UserName,u.UserName,s.Sequence";
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
                var _sql = @"select s.Id
                            ,s.RecipeGlobalMasterId
                            ,s.RecipeGlobalSubprocessId
                            ,s.RecipeGlobalOperationId
                            ,s.SubprocessId
                            ,s.OperationId
							,s.UtilityId
                            ,o.UserName Operation
                            ,u.UserName Utility
                            ,s.QtyValue
                            ,m.UserName Uom
                            ,s.Uom UomId
                            ,s.Duration
                            ,s.Temperature
                            ,s.IsFixed
                            ,s.Ph
                            ,s.Sequence
                            ,s.Remark
                            from trn.RecipeGlobalUtility s
                            left outer join hkp.Utility u on u.Id=s.UtilityId
                            left outer join scs.UnitOfMeasurement m on m.Id=s.Uom
                            left outer join mst.Operation o on o.Id=s.OperationId
                            where s.Id='" + id + @"'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void OutDetail(RecipeGlobalUtility from_ui, out RecipeGlobalUtility from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (from_db.Id != null || from_db.Id != "")
                from_db = GetDetail(from_ui.Id);
                ///Check Duplicate
                //detailList = _recipesubprocessservice.GetList(from_ui.RecipeMasterID);//get all child for this master
                //CheckDuplicateSubprocess(from_ui, detailList);

                if (from_db.Id == null || from_db.Id == "")
                {
                    from_db = new RecipeGlobalUtility
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(from_db);

                    from_db.Id = GetPK();//set pk
                }
                else
                {
                    from_db.ModelState = ModelState.Modified;
                    AuditService.Log(from_db);
                }

                #region Fields

                from_db.Duration = from_ui.Duration;
                from_db.IsFixed = from_ui.IsFixed;
                from_db.Ph = from_ui.Ph;
                from_db.QtyValue = from_ui.QtyValue;
                from_db.RecipeGlobalMasterId = from_ui.RecipeGlobalMasterId;
                from_db.RecipeGlobalOperationId = from_ui.RecipeGlobalOperationId;
                from_db.RecipeGlobalSubprocessId = from_ui.RecipeGlobalSubprocessId;
                from_db.Remark = from_ui.Remark;
                from_db.Sequence = from_ui.Sequence;
                from_db.SubprocessId = from_ui.SubprocessId;
                from_db.Temperature = from_ui.Temperature;
                from_db.Uom = from_ui.Uom;
                from_db.UtilityId = from_ui.UtilityId;

                #endregion Fields
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteUtility(string UtilityId)
        {
            RecipeGlobalUtility from_db = null;
            IEnumerable<RecipeGlobalRawMaterial> rm_fromdb = null;
            var flag = false;
            try
            {
                from_db = GetDetail(UtilityId);
                //_recipeGlobaloperationservice.DelOperationlList(detailid, out o_fromdb);
                //_recipeGlobalutilityservice.DelUtilitylListByOperationId(OperationId, out u_fromdb);
                _irecipeGlobalrawmaterialservice.DelRawMaterialListByUtilityId(UtilityId, out rm_fromdb);

                // from_db.RecipeGlobalOperation = (ICollection<RecipeGlobalOperation>)o_fromdb;
                //from_db.RecipeGlobalUtility = (ICollection<RecipeGlobalUtility>)u_fromdb;
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

        public void CheckDuplicate(RecipeGlobalUtility detail_ui, IEnumerable<object> from_db_detailList)
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

        public void DelUtilitylList(string subprocessid, out IEnumerable<RecipeGlobalUtility> from_db)
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

        public void DelUtilitylListByOperationId(string operationid, out IEnumerable<RecipeGlobalUtility> from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetListByOperationId(operationid);
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

        
    }
}