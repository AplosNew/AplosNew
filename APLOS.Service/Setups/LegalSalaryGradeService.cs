using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
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

namespace Library.Service.Setups
{
    public partial class LegalSalaryGradeService : Service<LegalSalaryGrade>, ILegalSalaryGradeService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<LegalSalaryGradeHead> _legalSalaryGradeHeadRepository;

        public LegalSalaryGradeService(
            IRepositoryAsync<LegalSalaryGrade> legalSalaryGradeRepository,
            IRepositoryAsync<LegalSalaryGradeHead> legalSalaryGradeHeadRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(legalSalaryGradeRepository, unitOfWork, pkGeneratorService)
        {
            _legalSalaryGradeHeadRepository = legalSalaryGradeHeadRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence(string plantId)
        {
            try
            {
                return base.Query(r => !r.Archive && r.PlantId == plantId).Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        private string GetPK(string companyGroupId)
        {
            return GetAutoNumber(nameof(LegalSalaryGrade), PKGeneratorEnum.Yearly, companyGroupId, DateTime.Now);
        }

        private void CheckUnique(LegalSalaryGrade entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, t => t.Code == entity.Code && t.Id != entity.Id && !t.Archive && t.CompanyGroupId == entity.CompanyGroupId && t.PlantId==entity.PlantId);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, t => t.UserName == entity.UserName && t.Id != entity.Id && !t.Archive && t.CompanyGroupId == entity.CompanyGroupId && t.PlantId == entity.PlantId);
        }

        public void InsertGraph(LegalSalaryGrade entity, IEnumerable<LegalSalaryGradeHead> legalSalaryGradeHead)
        {
            var flag = false;
            string pk = GetPK(entity.CompanyGroupId);
            try
            {
                if (legalSalaryGradeHead == null)
                    throw new CustomException("Please insert legal salary grade head");
                CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                InsertOrUpdateChild(legalSalaryGradeHead, pk, false);
                entity.Id = pk;
                base.InsertGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void InsertOrUpdateChild(IEnumerable<LegalSalaryGradeHead> entities, string pk, bool flag)
        {
            var dbList = _legalSalaryGradeHeadRepository.Query(t => t.LegalSalaryGradeId == pk).Select().AsEnumerable();
            if (entities != null)
            {
                var count = _legalSalaryGradeHeadRepository.CreateChildPk(t => t.LegalSalaryGradeId == pk, x => x.Id, pk).ToInt();
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = pk + "-" + count;
                        item.LegalSalaryGradeId = pk;
                        AuditService.AddedLog(item);
                        _legalSalaryGradeHeadRepository.Insert(item);
                        count++;
                    }
                    else
                    {
                        if (dbList.Any(t => t.Id == item.Id))
                        {
                            AuditService.UpdatedLog(item);
                            _legalSalaryGradeHeadRepository.Update(item);
                        }
                        else
                            throw new CustomException(ServiceResources.RecordNoLonger.ToString());
                    }
                }
            }
            /// comment for permanently delete from screen
            //if (flag)
            //{
            //    if (dbList != null)
            //    {
            //        if (entities == null)
            //        {
            //            foreach (var item in dbList)
            //            {
            //                _legalSalaryGradeHeadRepository.Delete(item);
            //            }
            //        }
            //        else
            //        {
            //            foreach (var item in dbList)
            //            {
            //                if (!entities.Any(t => t.Id == item.Id))
            //                {
            //                    _legalSalaryGradeHeadRepository.Delete(item);
            //                }
            //            }
            //        }
            //    }
            //}
        }

        public void UpdateGraph(LegalSalaryGrade entity, IEnumerable<LegalSalaryGradeHead> legalSalaryGradeHead)
        {
            var flag = false;
            try
            {
                if (legalSalaryGradeHead == null)
                    throw new CustomException("Please insert legal salary grade head");
                CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                InsertOrUpdateChild(legalSalaryGradeHead, entity.Id, true);
                base.UpdateGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string key)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                LegalSalaryGrade entity = Find(key);

                var childList = _legalSalaryGradeHeadRepository.Query(t => t.LegalSalaryGradeId == key).Select().AsEnumerable();
                foreach (var item in childList)
                {
                    _legalSalaryGradeHeadRepository.Delete(item);
                }
                base.DeleteGraph(entity.Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT LG.Id, LG.CompanyGroupId, LG.CurrencyRuleMasterId, LG.Sequence, LG.Code, LG.ShortName, LG.StandardName, LG.UserName, LG.PlantId
	                                        , LG.[Description], LG.Remarks, LG.Active,CRM.CurrencyRuleName
                                       FROM SCS.LegalSalaryGrade AS LG
                                       INNER JOIN dbo.CurrencyRuleMaster AS CRM ON LG.CurrencyRuleMasterId=CRM.SystemID
                                       WHERE LG.CompanyGroupId='" + companyGroupId + "' AND LG.PlantId='"+ plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetCurrencyRuleCbo(string companyGroupId,string plantId)
        {
            string _sql = @"SELECT SystemID,CurrencyRuleName FROM CurrencyRuleMaster
                            WHERE GroupID = '" + companyGroupId + "' AND PlantID='"+plantId+"'";
            return _sqlRepository.GetCombo(_sql, "SystemID", "CurrencyRuleName");
        }

        public IEnumerable<ComboModel> GetCbo(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string _sql = @"SELECT Id,UserName FROM SCS.LegalSalaryGrade WHERE Active=1 AND CompanyGroupId='" + identity.CompanyGroupId + "' AND PlantId='"+plantId+"' ORDER BY Sequence,UserName";
            return _sqlRepository.GetCombo(_sql, "Id", "UserName");
        }

        public List<Dictionary<string, object>> GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string _sql = @"SELECT Id LegalSalaryGradeId,UserName,PlantId FROM SCS.LegalSalaryGrade WHERE Active=1 AND CompanyGroupId='" + identity.CompanyGroupId + "'  AND PlantId<>'' ORDER BY Sequence,UserName";
            //return _sqlRepository.GetCombo(_sql, "Id", "UserName");
            return _sqlRepository.GetDataCollection(_sql);
        }

        public GridModel SalaryHeadList(GridParameter parameters, string companyGroupId, string currencyRuleId, string[] salaryHeadIds)
        {
            try
            {
                parameters.CmdText = @"SELECT CAST(0 as BIT) AS Flag,SH.SalaryHeadID AS SalaryHeadId, SH.SalaryHead, SH.HeadCategory
	                                        , CU1.Code AS EntryCurrency , CU2.Code AS DefinitionCurrency, CU3.Code AS DisbusmentCurrency
                                       FROM SalaryHead SH
                                       INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                                       INNER JOIN SCS.Currency CU1 ON CRC.AmtEntryCurrency = CU1.Id
                                       INNER JOIN SCS.Currency CU2 ON CRC.AmtDefinitionCurrency = CU2.Id
                                       INNER JOIN SCS.Currency CU3 ON CRC.AmtDisbusmentCurrency = CU3.Id
                                       AND CRC.MstSystemID ='" + currencyRuleId + "' AND SH.GroupID='" + companyGroupId + @"' AND SH.HeadType='E'
                                       AND SH.SalaryHeadID NOT IN(" + ReturnStringArray(salaryHeadIds) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public IEnumerable<object> LegalSalaryGradeHeadList(string legalSalaryGradeId)
        {
            try
            {
                var _sql = @"SELECT LSH.Id,LSH.LegalSalaryGradeId,LSH.[Sequence],LSH.SalaryHeadId, SH.SalaryHead,  SH.Description, SH.HeadCategory
	                                            , CU1.Code AS EntryCurrency , CU2.Code AS DefinitionCurrency, CU3.Code AS DisbusmentCurrency
                                        FROM SCS.LegalSalaryGradeHead AS LSH
                                        INNER JOIN SalaryHead SH ON LSH.SalaryHeadId=SH.SalaryHeadId
                                        INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                                        INNER JOIN SCS.Currency CU1 ON CRC.AmtEntryCurrency = CU1.Id
                                        INNER JOIN SCS.Currency CU2 ON CRC.AmtDefinitionCurrency = CU2.Id
                                        INNER JOIN SCS.Currency CU3 ON CRC.AmtDisbusmentCurrency = CU3.Id
                                        AND LSH.LegalSalaryGradeId='" + legalSalaryGradeId + "' AND CRC.MstSystemID IN(SELECT CurrencyRuleMasterId FROM SCS.LegalSalaryGrade WHERE Id='" + legalSalaryGradeId + @"') ORDER BY LSH.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }
        public void LegalSalaryGradeDelete(string key)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                LegalSalaryGradeHead entity = _legalSalaryGradeHeadRepository.Find(key);
                _legalSalaryGradeHeadRepository.Delete(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }
    }
}