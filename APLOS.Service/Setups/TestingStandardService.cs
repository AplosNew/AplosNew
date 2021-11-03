#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Setups;
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
using System.Threading;

#endregion Using

namespace Library.Service.Setups
{
    public class TestingStandardService : Service<TestingStandard>, ITestingStandardService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ITestingStandardDetailService _testingStandardDetailService;
        private readonly ITestingStandardBuyerService _testingStandardDetailBuyerService;

        public TestingStandardService(
            IRepositoryAsync<TestingStandard> testingStandardRepository
            , ITestingStandardDetailService testingStandardDetailService
            , ITestingStandardBuyerService testingStandardDetailBuyerService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(testingStandardRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _testingStandardDetailService = testingStandardDetailService;
            _testingStandardDetailBuyerService = testingStandardDetailBuyerService;
        }

        #endregion Constructor

        public string InsertAndUpdate(TestingStandard entity, IEnumerable<TestingStandardDetail> testingStandardDetail, IEnumerable<TestingStandardBuyer> testingStandardMaterial)
        {
            var flag = false;
            string pkId = GetPK();
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = "PPC-" + pkId;
                    entity.CompanyGroupId = identity.CompanyGroupId;
                    entity.ModelState = ModelState.Added;
                    AuditService.Log(entity);
                }
                else
                {
                    entity.ModelState = ModelState.Modified;
                    AuditService.Log(entity);
                }
                InsertOrUpdateGraph(entity);
                //******TestingStandardDetail******//
                if (testingStandardDetail != null)
                {
                    _testingStandardDetailService.InsertOrUpdate(testingStandardDetail, entity.Id);
                }
                //******TestingStandardDetailBuyer******//
                if (testingStandardMaterial != null)
                {
                    _testingStandardDetailBuyerService.InsertOrUpdate(testingStandardMaterial, entity.Id);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = base.Query(r => r.Id == Id).Select().FirstOrDefault();
                if (data != null)
                {
                    _testingStandardDetailService.DeleteWithMaster(Id);
                    _testingStandardDetailBuyerService.DeleteWithMaster(Id);
                    base.DeleteGraph(data);
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

        private string GetPK()
        {
            return GetAutoNumber(nameof(TestingStandard), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void CheckUnique(TestingStandard entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id);
            CheckUniqueColumn(UniqueColumnName.Code, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id);
        }

        public override void Update(TestingStandard entity)
        {
            try
            {
                CheckUnique(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM  [SCS].[TestingStandard] AS TS WHERE TS.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetCbo(string companyGroupId)
        {
            string _sql = @"SELECT T.Id,T.UserName
                            FROM SCS.TestingStandard AS T WHERE CompanyGroupId='" + companyGroupId + "'";
            return _sqlRepository.GetCombo(_sql, "Id", "UserName");
        }
        public IEnumerable<object> GetCboWithBuyer(string companyGroupId)
        {
            string sql = @"SELECT TS.Id AS [Value], TS.UserName AS [Text], TSB.BuyerId FROM [SCS].[TestingStandardBuyer] AS TSB
                        JOIN  [SCS].[TestingStandard] AS TS ON TSB.TestingStandardId = TS.Id
                        WHERE TS.CompanyGroupId='"+ companyGroupId + "' ORDER BY TS.UserName";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public GridModel FindById(GridParameter parameters, string id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT TS.* FROM [SCS].[TestingStandard] AS TS
				                       WHERE TS.Id='" + id + "' AND TS.CompanyGroupId='" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetCompanyCurrencyCountryWise()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string _sql = @"SELECT CU.Code, C.BaseCurrencyId, CO.CurrencyId FROM ORG.Company AS C
                         INNER JOIN MST.AddressMaster AS AM ON AM.Id=C.AddressMasterId
                         INNER JOIN SCS.Country AS CO ON CO.Id=AM.CountryId
                         INNER JOIN SCS.Currency CU ON CU.Id=C.BaseCurrencyId
                        WHERE C.Id='" + identity.CompanyId + "' ";
            return _sqlRepository.GetDataCollection(_sql, null);
        }

        public IEnumerable<object> GetCoaIdByCompany()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string _sql = @"SELECT COM.COAId,C.UserName AS CoaName FROM [ORG].[Company] AS COM
                            LEFT OUTER JOIN [HKP].[COA] AS C ON COM.COAId = C.Id
                            WHERE COM.Id='" + identity.CompanyId + "' ";
            return _sqlRepository.GetDataCollection(_sql, null);
        }

        #region Testing Standard Report

        public IWorkbook GetTestingStandardReport(string testing)
        {
            ReportGeneralVoucher obj = new ReportGeneralVoucher();
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IWorkbook workbook = obj.TestingStandard_Report(excelEngine, testing);
                return workbook;
            }
        }

        #endregion Testing Standard Report
    }
}