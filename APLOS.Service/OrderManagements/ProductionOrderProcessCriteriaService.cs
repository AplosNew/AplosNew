#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class ProductionOrderProcessCriteriaService : Service<ProductionOrderProcessCriteria>, IProductionOrderProcessCriteriaService
    {
        #region Constructor

        private readonly IRepositoryAsync<ProductionOrderProcessCriteria> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public ProductionOrderProcessCriteriaService(
            IRepositoryAsync<ProductionOrderProcessCriteria> repository
            , IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(repository, unitOfWork, pkGeneratorService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public string GetPK()
        {
            return "BC" + _pkGeneratorService.GetAutoNumber(nameof(ProductionOrderProcessCriteria), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<ProductionOrderProcessCriteria> ProductionOrderProcessCriteriaList(string ProductionOrderMasterID)
        {
            try
            {
                string _sql = "select * from trn.ProductionOrderProcessCriteria where ProductionOrderMasterID='" + ProductionOrderMasterID + "'";
                return _repository.SqlQuery<ProductionOrderProcessCriteria>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<ProductionOrderSubprocessSet> ProductionOrderSubprocessSetList(string masterId)
        {
            try
            {
                string _sql = "select * from trn.ProductionOrderSubprocessSet where ProductionOrderProcessCriteriaId ='" + masterId + "'";
                return _repository.SqlQuery<ProductionOrderSubprocessSet>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetListOrderWise(string ProductionOrderMasterId)
        {
            try
            {
                var _sql = @"
                            SELECT p.UserName Process
                                ,isnull(c.UserName,'')+ isnull(g.UserName,'') + isnull(v.UserName,'') EntityOrVendorName
								,isnull(s.EntityIdWithinCompany,'')+ isnull(s.EntityIdWithinGroup,'') + isnull(s.VendorId,'') EntityOrVendorId
	                            ,c.UserName EntityWICompany
	                            ,g.UserName EntityWIGroup
	                            ,v.UserName Vendor
	                            ,s.EntityId
	                            ,s.ProcessId
	                            ,s.RequiredDays,pt.UserName ProcessType
	                            ,s.LeadDays
	                            ,s.IsJobWorkApplicable
	                            ,s.JobWorkType
	                            ,s.EntityIdWithinCompany
	                            ,s.EntityIdWithinGroup
	                            ,s.VendorId,s.ProcessTypeId,s.Id
                                , case s.IsJobWorkApplicable when 1 then 0 else 1 end setDisable
                            FROM trn.ProductionOrderProcessSet s
                                LEFT JOIN hkp.Process p ON p.Id = s.ProcessId
                                LEFT JOIN org.Entity c ON c.Id = s.EntityIdWithinCompany
                                LEFT JOIN org.Entity g ON g.Id = s.EntityIdWithinGroup
                                LEFT JOIN hkp.Party v ON v.Id = s.VendorId
                                left outer join hkp.processtype pt on pt.Id=s.ProcessTypeId
                            WHERE s.ProductionOrderMasterId='" + ProductionOrderMasterId + @"'
                            ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetList(string ProcessId, string ProcessTypeId, string ProductionOrderMasterId)
        {
            try
            {
                var _sql = @"select
                                    d.Id,
                                    pc.UserName ProcessCriteria,
                                    d.LoadFactor,
                                    d.CycleTime,
                                    d.AvgWeight,
                                    d.Qty,
                                    d.WeightUomId,
                                    d.ProcessId,
                                    d.ProcessCriteriaId,
                                    d.ProcessTypeId,
                                    u.UserName WeightUom

                                ,p1.[Alias] Characteristics1
								,cv1.[Description] Characteristics1Value
	                            ,d.Characteristics1Id
	                            ,p2.[Alias] Characteristics2
								,cv2.[Description] Characteristics2Value
	                            ,d.Characteristics2Id
	                            ,p3.[Alias] Characteristics3
								,cv3.[Description] Characteristics3Value
	                            ,d.Characteristics3Id
	                            ,'xyz' SelectedCharacteristics
	                            ,Characteristics1Selected = CASE isnull(d.Characteristics1Id, '')
		                            WHEN ''
			                            THEN 0
		                            ELSE 1
		                            END
	                            ,Characteristics2Selected = CASE isnull(d.Characteristics2Id, '')
		                            WHEN ''
			                            THEN 0
		                            ELSE 1
		                            END
	                            ,Characteristics3Selected = CASE isnull(d.Characteristics3Id, '')
		                            WHEN ''
			                            THEN 0
		                            ELSE 1
		                            END

                                    from [TRN].[ProductionOrderProcessCriteria] d
                                    left outer join hkp.ProcessCriteria pc on pc.Id=d.ProcessCriteriaId
                                    left outer join scs.UnitOfMeasurement u on u.Id=d.WeightUomId

                                    LEFT JOIN hkp.Characteristics p1 ON p1.Id = d.Characteristics1Id
									LEFT JOIN hkp.Characteristics p2 ON p2.Id = d.Characteristics2Id
									LEFT JOIN hkp.Characteristics p3 ON p3.Id = d.Characteristics3Id

									LEFT JOIN hkp.CharacteristicsValue cv1 ON cv1.Id = d.Characteristics1ValueId
									LEFT JOIN hkp.CharacteristicsValue cv2 ON cv2.Id = d.Characteristics2ValueId
									LEFT JOIN hkp.CharacteristicsValue cv3 ON cv3.Id = d.Characteristics3ValueId

                                Where d.ProcessId='" + ProcessId + @"'
                                  and d.ProcessTypeId='" + ProcessTypeId + @"'
                                  and d.ProductionOrderMasterId='" + ProductionOrderMasterId + @"'
                                    ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ProcessSetOut(ProductionOrderProcessCriteria from_ui, out ProductionOrderProcessCriteria from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = Find(from_ui.Id);

                if (from_db.Id == null || from_db.Id == "")
                {
                    from_db = new ProductionOrderProcessCriteria
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

                from_db.AvgWeight = from_ui.AvgWeight;
                from_db.CycleTime = from_ui.CycleTime;
                from_db.EntityId = from_ui.EntityId;
                from_db.LoadFactor = from_ui.LoadFactor;
                from_db.ProcessCriteriaId = from_ui.ProcessCriteriaId;
                from_db.ProcessId = from_ui.ProcessId;
                from_db.ProcessTypeId = from_ui.ProcessTypeId;
                from_db.ProductionOrderId = from_ui.ProductionOrderId;
                from_db.Qty = from_ui.Qty;
                from_db.WeightUomId = from_ui.WeightUomId;

                from_db.Characteristics1Id = from_ui.Characteristics1Id;
                from_db.Characteristics2Id = from_ui.Characteristics2Id;
                from_db.Characteristics3Id = from_ui.Characteristics3Id;
                from_db.Characteristics1ValueId = from_ui.Characteristics1ValueId;
                from_db.Characteristics2ValueId = from_ui.Characteristics2ValueId;
                from_db.Characteristics3ValueId = from_ui.Characteristics3ValueId;

                #endregion Fields
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveDetail(ProductionOrderProcessCriteria ui_detail)
        {
            ProductionOrderProcessCriteria fromdb_detail = null;
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //Validation
                ProcessSetOut(ui_detail, out fromdb_detail);

                AuditService.Log(fromdb_detail);
                InsertOrUpdateGraph(fromdb_detail);

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
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

        public void DeleteGraph(string id)
        {
            IEnumerable<ProductionOrderSubprocessSet> from_db_ChildList = null;
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "ProductionOrderProcessCriteria Id"));

                ProductionOrderProcessCriteria entity = Find(id);
                from_db_ChildList = ProductionOrderSubprocessSetList(id);
                foreach (var item in from_db_ChildList)
                {
                    item.ModelState = ModelState.Deleted;
                }
                //entity.ProductionOrderSubprocessSet = (ICollection<ProductionOrderSubprocessSet>)from_db_ChildList;

                _unitOfWork.BeginTransaction();
                flag = true;
                base.DeleteGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }
}