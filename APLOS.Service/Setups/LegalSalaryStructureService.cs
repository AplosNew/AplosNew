using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Service.Accounts;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Library.Service.Setups
{
    public partial class LegalSalaryStructureService : Service<LegalSalaryStructure>, ILegalSalaryStructureService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<LegalSalaryStructureValue> _valueRepository;

        public LegalSalaryStructureService(
            IRepositoryAsync<LegalSalaryStructure> legalSalaryGradeRepository,
            IRepositoryAsync<LegalSalaryStructureValue> valueRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(legalSalaryGradeRepository, unitOfWork, pkGeneratorService)
        {
            _valueRepository = valueRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string legalSalaryGradeId)
        {
            try
            {
                parameters.CmdText = @"SELECT LS.Id,LS.LegalSalaryGradeId,LS.EmployeeLocationId,EL.UserName EmployeeLocationName,REPLACE(CONVERT(CHAR(11), LS.EffectiveDate, 106),' ','-') AS EffectiveDate
                                       FROM MST.LegalSalaryStructure LS
									   LEFT JOIN HKP.EmployeeLocation EL ON LS.EmployeeLocationId=EL.Id WHERE LegalSalaryGradeId='" + legalSalaryGradeId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public IEnumerable<object> GetHeadList(string legalSalaryGradeId)
        {
            try
            {
                var _sql = @"SELECT '' AS Id,'' AS LegalSalaryStructureId,LSGH.SalaryHeadId
	                                , SH.SalaryHead, SH.HeadCategory
	                                , CU1.Code AS EntryCurrency , CU2.Code AS DefinitionCurrency, CU3.Code AS DisbusmentCurrency
	                                , CONVERT(DECIMAL(18,4),0)  AS SalaryHeadValue
                            FROM SCS.LegalSalaryGradeHead AS LSGH
                            INNER JOIN SCS.LegalSalaryGrade AS LSG ON LSGH.LegalSalaryGradeId = LSG.Id
                            INNER JOIN SalaryHead AS SH ON LSGH.SalaryHeadId=SH.SalaryHeadID
                            INNER JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = LSG.CurrencyRuleMasterId AND LSGH.SalaryHeadId=CRC.SalaryHeadID
                            INNER JOIN SCS.Currency CU1 ON CRC.AmtEntryCurrency = CU1.Id
                            INNER JOIN SCS.Currency CU2 ON CRC.AmtDefinitionCurrency = CU2.Id
                            INNER JOIN SCS.Currency CU3 ON CRC.AmtDisbusmentCurrency = CU3.Id
                            WHERE LSG.Id='" + legalSalaryGradeId + "' ORDER BY LSGH.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public IEnumerable<object> GetHeadEdit(string id)
        {
            try
            {
                var _sql = @"SELECT LSSV.Id,LSSV.LegalSalaryStructureId,LSSV.SalaryHeadId
	                               , SH.SalaryHead, SH.HeadCategory
	                               , CU1.Code AS EntryCurrency , CU2.Code AS DefinitionCurrency, CU3.Code AS DisbusmentCurrency
	                               , SalaryHeadValue=CONVERT(DECIMAL(18,4),COALESCE((LSSV.SalaryHeadValue),0))
                            FROM [MST].[LegalSalaryStructure] AS LSS
                            LEFT OUTER JOIN [MST].[LegalSalaryStructureValue] AS LSSV ON LSS.Id=LSSV.LegalSalaryStructureId
                            LEFT OUTER JOIN SCS.LegalSalaryGrade AS LSG ON LSS.LegalSalaryGradeId = LSG.Id
                            LEFT OUTER JOIN SalaryHead AS SH ON LSSV.SalaryHeadId=SH.SalaryHeadID
                            LEFT OUTER JOIN SCS.LegalSalaryGradeHead AS LSGH ON LSGH.LegalSalaryGradeId=LSG.Id AND LSGH.SalaryHeadId=SH.SalaryHeadID
                            LEFT OUTER JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = LSG.CurrencyRuleMasterId AND LSSV.SalaryHeadId=CRC.SalaryHeadID
                            LEFT OUTER JOIN SCS.Currency CU1 ON CRC.AmtEntryCurrency = CU1.Id
                            LEFT OUTER JOIN SCS.Currency CU2 ON CRC.AmtDefinitionCurrency = CU2.Id
                            LEFT OUTER JOIN SCS.Currency CU3 ON CRC.AmtDisbusmentCurrency = CU3.Id
                            WHERE LSS.Id='" + id + "' ORDER BY LSGH.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public void InsertOrUpdateGraph(LegalSalaryStructure entity, IEnumerable<LegalSalaryStructureValue> valueList)
        {
            var flag = false;
            try
            {
                if (CheckEffectiveDate(entity))
                    throw new CustomException("This location effective date allready exist for this Legal Salary Grade.");
                _unitOfWork.BeginTransaction();
                flag = true;
                if (string.IsNullOrEmpty(entity.Id))
                {
                    var pk = GetAutoNumber(nameof(LegalSalaryStructure), PKGeneratorEnum.Auto, null, DateTime.Now);
                    entity.Id = pk;
                    InsertOrUpdateChild(valueList, entity.Id);
                    InsertGraph(entity);
                }
                else
                {
                    InsertOrUpdateChild(valueList, entity.Id);
                    UpdateGraph(entity);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
            
        }

        private void InsertOrUpdateChild(IEnumerable<LegalSalaryStructureValue> entities, string fk)
        {
            if (entities == null)
                throw new CustomException("You can not save without legal salary head value.");
            foreach (var item in entities)
            {
                if (string.IsNullOrEmpty(item.Id))
                {
                    var pk = GetAutoNumber(nameof(LegalSalaryStructureValue), PKGeneratorEnum.Auto, null, DateTime.Now);
                    //pk.MaxNumber++;
                    item.Id = pk;
                    item.LegalSalaryStructureId = fk;
                    AuditService.AddedLog(item);
                    _valueRepository.Insert(item);
                }
                else
                {
                    AuditService.UpdatedLog(item);
                    _valueRepository.Update(item);
                }
            }
        }

        public void DeleteGraph(string key)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                LegalSalaryStructure entity = Find(key);
                var childList = _valueRepository.Query(t => t.LegalSalaryStructureId == key).Select().ToList();
                foreach (var item in childList)
                {
                    _valueRepository.Delete(item);
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

        private bool CheckEffectiveDate(LegalSalaryStructure entity)
        {
            return Any(t => t.Id != entity.Id && t.LegalSalaryGradeId == entity.LegalSalaryGradeId && t.EffectiveDate == entity.EffectiveDate && t.EmployeeLocationId == entity.EmployeeLocationId);
        }

        #region Legal Salary Report

        public IWorkbook GetLegalSalaryReport(string effectiveDate, string plantId)
        {
            try
            {
                ReportGeneralVoucher obj = new ReportGeneralVoucher();
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IWorkbook workbook = obj.LegalSalary_Report(excelEngine, effectiveDate, plantId);
                    return workbook;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Legal Salary Report
    }
}