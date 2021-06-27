#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class ProductionOrderSubprocessSetService : Service<ProductionOrderSubprocessSet>, IProductionOrderSubprocessSetService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<ProductionOrderSubprocessSet> _repository;

        public ProductionOrderSubprocessSetService(
            IRepositoryAsync<ProductionOrderSubprocessSet> Repository
            , IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(Repository, unitOfWork, pkGeneratorService)
        {
            _repository = Repository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public string GetPK()
        {
            return "BSP" + _pkGeneratorService.GetAutoNumber(nameof(ProductionOrderSubprocessSet), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<ProductionOrderSubprocessSet> ProductionOrderSubprocessSetList(string productionOrderProcessCriteriaId)
        {
            try
            {
                string _sql = "select * from trn.ProductionOrderSubprocessSet as p where p.ProductionOrderProcessCriteriaId='" + productionOrderProcessCriteriaId + "'";
                return _repository.SqlQuery<ProductionOrderSubprocessSet>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetListProcessAndProcessTypeWise(GridParameter parameters, string entityid, string processid, string processtypeid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT
                                sd.Id As SubProcessSetDetailId,
								s.Id As SubProcessSetId
								,0 IsSelectedId
					            ,p.UserName Subprocess
                                ,isnull(c.UserName,'')+ isnull(g.UserName,'') + isnull(v.UserName,'') EntityOrVendorName
	                            ,s.EntityId
	                            ,s.ProcessId
                                ,s.Code
								,s.Description
	                            ,sd.[Days]
								,sd.Symbol
								,sd.JobWorkApplicable
	                            ,sd.ProductionCycleTime
	                            ,sd.[Sequence]
							    ,sd.IsBaseProcess
                                ,s.ProcessTypeId
                            FROM HKP.SubprocessSet s
							LEFT OUTER JOIN HKP.SubProcessSetDetail sd on sd.SubProcessSetId=s.Id
                            LEFT OUTER JOIN HKP.SubProcess p ON p.Id = sd.SubProcessId
                            LEFT OUTER JOIN ORG.Entity c ON c.Id = sd.EntityIdWithinCompany
                            LEFT OUTER JOIN ORG.Entity g ON g.Id = sd.EntityIdWithinGroup
                            LEFT OUTER JOIN HKP.Party v ON v.Id = sd.VendorId
                            LEFT OUTER join HKP.ProcessType pt on pt.Id=s.ProcessTypeId
							WHERE s.ProcessId='" + processid + @"'
								AND s.ProcessTypeId='" + processtypeid + @"'
								AND s.EntityId='" + entityid + @"'";
                return _sqlRepository.GetGridData(parameters);
                //return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetDetailChildList(string productionOrderProcessCriteriaId, string entityid, string processid, string processtypeid)
        {
            try
            {
                var sql = @"SELECT m.Id,
                                   m.SubProcessSetDetailId,
                                   m.SubProcessSetId,
                                   0 IsSelectedId,
                                   p.UserName Subprocess,
                                   Isnull(c.UserName, '') + Isnull(g.UserName, '') + Isnull(v.UserName, '') EntityOrVendorName,
                                   m.EntityId,
                                   m.ProcessId,
                                   s.Code,
                                   s.Description,
                                   sd.[Days],
                                   sd.Symbol,
                                   sd.JobWorkApplicable,
								   case sd.JobWorkApplicable when 1 then 0 else 1 end setDisable,
                                   sd.ProductionCycleTime,
                                   sd.[Sequence],
                                   sd.IsBaseProcess,
                                   m.ProcessTypeId,
                                   m.ProductionOrderProcessCriteriaId
                            FROM   TRN.ProductionOrderSubprocessSet m
                                   LEFT OUTER JOIN HKP.SubProcessSetDetail sd on m.SubProcessSetDetailId = sd.Id
                                   LEFT OUTER JOIN HKP.SubprocessSet s on s.Id = sd.SubProcessSetId
                                   LEFT OUTER JOIN HKP.SubProcess p ON p.Id = sd.SubProcessId
                                   LEFT OUTER JOIN ORG.Entity c ON c.Id = sd.EntityIdWithinCompany
                                   LEFT OUTER JOIN ORG.Entity g ON g.Id = sd.EntityIdWithinGroup
                                   LEFT OUTER JOIN HKP.Party v  ON v.Id = sd.VendorId
                                   LEFT OUTER JOIN HKP.ProcessType pt ON pt.Id = s.ProcessTypeId
                            	   WHERE  m.ProductionOrderProcessCriteriaId='" + productionOrderProcessCriteriaId + @"'";
                //return _sqlRepository.GetGridData(sql,null);
                IEnumerable<object> list = _sqlRepository.GetDataCollection(sql, null);
                if (list.Count() > 0)
                {
                    return list;
                }
                else
                {
                    var sql2 = @"SELECT '' Id,
                                        sd.Id SubProcessSetDetailId,
                                        s.Id SubProcessSetId,
                                        0 IsSelectedId,
                                        p.UserName Subprocess,
                                        Isnull(c.UserName, '') + Isnull(g.UserName, '') + Isnull(v.UserName, '') EntityOrVendorName,
                                        s.EntityId,
                                        s.ProcessId,
                                        s.Code,
                                        s.Description,
                                        sd.[Days],
                                        sd.Symbol,
                                        sd.JobWorkApplicable,
								        case sd.JobWorkApplicable when 1 then 0 else 1 end setDisable,
                                        sd.ProductionCycleTime,
		                                --select * from HKP.SubprocessSet
                                        sd.[Sequence],
                                        sd.IsBaseProcess,
                                        s.ProcessTypeId,
                                        '' ProductionOrderProcessCriteriaId
                                FROM   HKP.SubProcessSetDetail sd
                                        LEFT OUTER JOIN HKP.SubprocessSet s on s.Id = sd.SubProcessSetId
                                        LEFT OUTER JOIN HKP.SubProcess p ON p.Id = sd.SubProcessId
                                        LEFT OUTER JOIN ORG.Entity c ON c.Id = sd.EntityIdWithinCompany
                                        LEFT OUTER JOIN ORG.Entity g ON g.Id = sd.EntityIdWithinGroup
                                        LEFT OUTER JOIN HKP.Party v  ON v.Id = sd.VendorId
                                        LEFT OUTER JOIN HKP.ProcessType pt ON pt.Id = s.ProcessTypeId
		                                where s.EntityId='" + entityid + @"'
		                                and s.ProcessId='" + processid + @"'
		                                and s.ProcessTypeId='" + processtypeid + @"'";
                    return _sqlRepository.GetDataCollection(sql2, null);
                }//else
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<ComboModel> GetProcessCbo(string entityid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT DISTINCT P.Id [Value], P.UserName [Text] FROM [HKP].[ProcessSetDetail] AS PSD
                                JOIN [HKP].[ProcessSet] AS PS ON PSD.ProcessSetId=PS.Id
                                JOIN [HKP].[Process] P ON PSD.ProcessId=P.Id
                                WHERE EntityId='"+ entityid + "'";
                return _sqlRepository.GetCombo(_sql, "Value", "Text");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<ComboModel> GetProcessTypeCbo(string Orderid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"select  distinct
                                        p.Id [Value],p.UserName [Text]
                                from [TRN].[ProductionOrderProcessSet] r
                                left outer join hkp.ProcessType p on p .Id=r.ProcessTypeId
                                where r.ProductionOrderMasterId='" + Orderid + @"'";
                return _sqlRepository.GetCombo(_sql, "Value", "Text");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void XProcessSetOut(string ProductionOrderMasterId, IEnumerable<ProductionOrderSubprocessSet> from_ui, out List<ProductionOrderSubprocessSet> from_db)
        {
            var _count = 0;
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = ProductionOrderSubprocessSetList(ProductionOrderMasterId).ToList<ProductionOrderSubprocessSet>();
                var PK = GetPK();
                if (from_ui != null)
                {
                    foreach (var ui in from_ui)
                    {
                        var db = from_db.Where(a => a.Id == ui.Id).FirstOrDefault();
                        if (db == null)//new
                        {
                            _count += 1;
                            db = new ProductionOrderSubprocessSet
                            {
                                Id = PK + "-" + _count,
                                ModelState = ModelState.Added
                            };
                            AuditService.Log(db);
                            db.EntityId = ui.EntityId;
                            db.ProductionOrderId = ui.ProductionOrderId;
                            db.ProcessId = ui.ProcessId;

                            from_db.Add(db);
                        }
                        else
                        {
                            db.ModelState = ModelState.Modified;
                            AuditService.Log(db);
                            db.EntityId = ui.EntityId;
                            db.ProductionOrderId = ui.ProductionOrderId;
                            db.ProcessId = ui.ProcessId;
                        }
                    }//foreach
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveProcessSet(string ProductionOrderMasterId, IEnumerable<ProductionOrderSubprocessSet> ui_pmaterial)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var flag = false;
            try
            {
                XProcessSetOut(ProductionOrderMasterId, ui_pmaterial, out List<ProductionOrderSubprocessSet> localDetailList);
                foreach (var item in localDetailList)
                {
                    AuditService.Log(item);
                    InsertOrUpdateGraph(item);
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                ui_pmaterial = localDetailList;
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

        public void ProcessSetOut(string ProductionOrderMasterId, ProductionOrderSubprocessSet from_ui, out ProductionOrderSubprocessSet from_db)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetDetailGridData(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"SELECT
                                  PBPC.*,
                                  PC.UserName AS ProcessCriteria,
                                  cv1.[Description] Characteristics1Value,
                                  cv2.[Description] Characteristics2Value,
                                  cv3.[Description] Characteristics3Value

                                FROM [TRN].[ProductionOrderProcessCriteria] PBPC
                                LEFT OUTER JOIN [HKP].[ProcessCriteria] PC  ON PC.Id = PBPC.ProcessCriteriaId

                                LEFT JOIN hkp.CharacteristicsValue cv1  ON cv1.Id = PBPC.Characteristics1ValueId
                                LEFT JOIN hkp.CharacteristicsValue cv2  ON cv2.Id = PBPC.Characteristics2ValueId
                                LEFT JOIN hkp.CharacteristicsValue cv3  ON cv3.Id = PBPC.Characteristics3ValueId
                             Where PBPC.Id='" + Id + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetCharacteristicsSetting(string entityid, string mmid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                IEnumerable<object> eml = GetEntityMMCharacteristicsSetting(entityid, mmid);
                if (eml.Count() > 0)
                {
                    return eml;
                }
                else
                {
                    throw new Exception("No 'Entity and Material Master' wise Characteristics setting found...");
                }//entity
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<object> GetEntityMMCharacteristicsSetting(string entityid, string mmid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"SELECT
                                   [Characteristics1Id]
                                  ,[Characteristics2Id]
                                  ,[Characteristics3Id]
                                  ,[ApplicableatMaterialLevel]

                              FROM [DEVCORE].[TRN].[RecipeConfig]
                              where EntityId='" + entityid + @"' and
                              MaterialGridId =(select MaterialGridId from mst.MaterialMaster where id='" + mmid + @"')";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ProcessSetOutChild(string ProductionOrderSubprocessCritariaId, IEnumerable<ProductionOrderSubprocessSet> from_ui, out List<ProductionOrderSubprocessSet> from_db)
        {
            var _count = 0;
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = ProductionOrderSubprocessSetList(ProductionOrderSubprocessCritariaId).ToList<ProductionOrderSubprocessSet>();

                foreach (var item in from_db)
                {
                    var db = from_ui.Where(a => a.Id == item.Id).FirstOrDefault();
                    if (db == null)
                    {
                        item.ModelState = ModelState.Deleted;
                    }
                }

                var PK = GetPK();
                if (from_ui != null)
                {
                    foreach (var ui in from_ui)
                    {
                        var db = from_db.Where(a => a.Id == ui.Id).FirstOrDefault();
                        if (db == null)//new
                        {
                            _count += 1;
                            db = new ProductionOrderSubprocessSet
                            {
                                Id = PK + "-" + _count,
                                ModelState = ModelState.Added
                            };
                            AuditService.Log(db);
                            db.SubProcessSetDetailId = ui.SubProcessSetDetailId;
                            db.ProductionOrderProcessCriteriaId = ui.ProductionOrderProcessCriteriaId;
                            db.EntityId = ui.EntityId;
                            db.ProductionOrderId = ui.ProductionOrderId;
                            db.ProcessId = ui.ProcessId;
                            db.ProcessTypeId = ui.ProcessTypeId;
                            db.SubProcessSetId = ui.SubProcessSetId;

                            from_db.Add(db);
                        }
                        else
                        {
                            db.ModelState = ModelState.Modified;
                            AuditService.Log(db);
                            db.SubProcessSetDetailId = ui.SubProcessSetDetailId;
                            db.ProductionOrderProcessCriteriaId = ui.ProductionOrderProcessCriteriaId;
                            db.EntityId = ui.EntityId;
                            db.ProductionOrderId = ui.ProductionOrderId;
                            db.ProcessId = ui.ProcessId;
                            db.ProcessTypeId = ui.ProcessTypeId;
                            db.SubProcessSetId = ui.SubProcessSetId;
                        }
                    }//foreach
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveProcessSetChild(string ProductionOrderSubprocessCritariaId, IEnumerable<ProductionOrderSubprocessSet> ui_pmaterial)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var flag = false;
            try
            {
                //Validation
                //ProductionOrderSubprocessSet
                ProcessSetOutChild(ProductionOrderSubprocessCritariaId, ui_pmaterial, out List<ProductionOrderSubprocessSet> localDetailList);
                foreach (var item in localDetailList)
                {
                    AuditService.Log(item);
                    InsertOrUpdateGraph(item);
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                ui_pmaterial = localDetailList;
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
    }
}