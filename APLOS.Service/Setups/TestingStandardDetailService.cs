#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Setups;
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

namespace Library.Service.Setups
{
    public class TestingStandardDetailService : Service<TestingStandardDetail>, ITestingStandardDetailService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly ITestingStandardBuyerService _testingStandardDetailBuyerService;

        public TestingStandardDetailService(
            IRepositoryAsync<TestingStandardDetail> projectPlanningDetailRepository
            , ITestingStandardBuyerService testingStandardDetailBuyerService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(projectPlanningDetailRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
            _testingStandardDetailBuyerService = testingStandardDetailBuyerService;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<TestingStandardDetail> entity, string testingStandardId)
        {
            try
            {
                if (entity != null)
                {
                    string _pk = GetPK();
                    var count = 0;
                    var from_db = QueryForTestingStandardDetail(testingStandardId).ToList();
                    foreach (var item in entity)
                    {
                        var existItem = QueryForTestingStandardWithTesting(testingStandardId, item.TestingId);
                        foreach (var t in existItem)
                        {
                            var dic = (Dictionary<string, object>)t;
                            if (existItem != null)
                            {
                                throw new CustomException(dic["UserName"]+ " is already saved!!");
                            }
                        }

                        count++;
                        if (string.IsNullOrEmpty(item.Id) && string.IsNullOrEmpty(item.TestingStandardId))
                        {
                            item.Id = _pk + "-" + count;
                            item.TestingStandardId = testingStandardId;
                            InsertGraph(item);
                        }
                        else if (!string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.TestingStandardId))
                        {
                            UpdateGraph(item);
                        }
                        //base.InsertOrUpdateGraph(item);
                    }
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<TestingStandardDetail> GetDetailList(string SalesOrderInvoicePackingListId)
        {
            try
            {
                return from m in Query(m => m.TestingStandardId == SalesOrderInvoicePackingListId) select m;
                //select new { Text = m.FileName, Value = m.Id };
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            return "PPD-" + GetAutoNumber(nameof(TestingStandardDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void DeleteWithMaster(string Id)
        {
            try
            {
                var data = Query(r => r.TestingStandardId == Id).Select().ToList();
                if (data != null)
                {
                    for (int i = 0; i < data.Count(); i++)
                    {
                        Delete(data[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteGraph(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Query(r => r.Id == Id).Select().FirstOrDefault();
                if (data != null)
                {
                    _testingStandardDetailBuyerService.DeleteGraph(Id);
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

        public IEnumerable<Object> QueryForTestingStandardDetail(string testingStandardId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT TSD.*,T.UserName,T.Code,T.ShortName,T.StandardName,T.TestingCategoryId,T.UserName,TC.UserName AS TestingCategoryName 
                                , OP.UserName OriginatingProcess, TP.UserName TestingProcess
                                FROM [SCS].[TestingStandard] AS TS
                                LEFT JOIN [SCS].[TestingStandardDetail] AS TSD ON TS.Id=TSD.TestingStandardId
                                LEFT JOIN [SCS].[Testing] AS T ON TSD.TestingId = T.Id
                                LEFT JOIN [HKP].[TestingCategory] AS TC ON T.TestingCategoryId = TC.Id
                                LEFT JOIN [HKP].[Process] AS OP ON TSD.OriginatingProcessId = OP.Id
                                LEFT JOIN [HKP].[Process] AS TP ON TSD.TestingProcessId = TP.Id
                                WHERE TSD.TestingStandardId='" + testingStandardId + "' ";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<Object> QueryForTestingStandardWithTesting(string testingStandardId, string testingId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT TSD.*,T.UserName,T.Code,T.ShortName,T.StandardName,T.TestingCategoryId,T.UserName,TC.UserName AS TestingCategoryName FROM [SCS].[TestingStandard] AS TS
LEFT OUTER JOIN [SCS].[TestingStandardDetail] AS TSD ON TS.Id=TSD.TestingStandardId
LEFT OUTER JOIN [SCS].[Testing] AS T ON TSD.TestingId = T.Id
LEFT OUTER JOIN [HKP].[TestingCategory] AS TC ON T.TestingCategoryId = TC.Id
WHERE TSD.TestingStandardId='" + testingStandardId + "' AND TSD.TestingId='" + testingId + "' ";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel QueryForTestingStandardDetailWithTSId(GridParameter parameters, string testingStandardId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT PPD.*,PPC.UserName AS TestingStandardCategory,PPSC.UserName AS TestingStandardSubCategory ,BM.GL,BM.BudgetCategory,BM.BudgetSubCategory,BM.BudgetItem, BM.BudgetGroup    FROM [MST].[TestingStandard] AS PP
LEFT OUTER JOIN [MST].[TestingStandardDetail] AS PPD ON PP.Id=PPD.TestingStandardId
LEFT OUTER JOIN [HKP].[TestingStandardCategory] PPC ON PPD.TestingStandardCategoryId = PPC.Id
LEFT OUTER JOIN [HKP].[TestingStandardSubCategory] PPSC ON PPD.TestingStandardSubCategoryId = PPSC.Id
RIGHT OUTER JOIN (SELECT BM.*, BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, BI.UserName AS BudgetItem, BG.UserName AS BudgetGroup, GLGI.AccountCode +' - '+GLGI.UserName AS GL FROM [MST].[BudgetMaster] BM
                                        LEFT OUTER JOIN [HKP].[BudgetCategory]  AS BC on BC.Id = BM.BudgetCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetSubCategory] AS BSC on BSC.Id = BM.BudgetSubCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetGroup] AS bg on BM.BudgetGroupId = bg.Id
                                        INNER JOIN [HKP].[Budget] BI ON BI.Id = BM.BudgetId
                                        INNER JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
										) AS BM ON PPD.BudgetMasterId=BM.Id
WHERE PP.Id='" + testingStandardId + @"'  AND PP.CompanyId='" + identity.CompanyId + "' ";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel QueryForTestingStandardDetailWithPPIdAndCat(GridParameter parameters, string testingStandardId, string projectPlanningCategory, string projectPlanningSubCategory)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT PPD.*,PPC.UserName AS TestingStandardCategory,PPSC.UserName AS TestingStandardSubCategory ,BM.GL,BM.BudgetCategory,BM.BudgetSubCategory,BM.BudgetItem, BM.BudgetGroup    FROM [MST].[TestingStandard] AS PP
                                        LEFT OUTER JOIN [MST].[TestingStandardDetail] AS PPD ON PP.Id=PPD.TestingStandardId
                                        LEFT OUTER JOIN [HKP].[TestingStandardCategory] PPC ON PPD.TestingStandardCategoryId = PPC.Id
                                        LEFT OUTER JOIN [HKP].[TestingStandardSubCategory] PPSC ON PPD.TestingStandardSubCategoryId = PPSC.Id
                                        RIGHT OUTER JOIN (SELECT BM.*, BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, BI.UserName AS BudgetItem, BG.UserName AS BudgetGroup, GLGI.AccountCode +' - '+GLGI.UserName AS GL FROM [MST].[BudgetMaster] BM
                                        LEFT OUTER JOIN [HKP].[BudgetCategory]  AS BC on BC.Id = BM.BudgetCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetSubCategory] AS BSC on BSC.Id = BM.BudgetSubCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetGroup] AS bg on BM.BudgetGroupId = bg.Id
                                        INNER JOIN [HKP].[Budget] BI ON BI.Id = BM.BudgetId
                                        INNER JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
										) AS BM ON PPD.BudgetMasterId=BM.Id
WHERE PP.Id='" + testingStandardId + @"'  AND PPD.TestingStandardCategoryId='" + projectPlanningCategory + @"'AND PPD.TestingStandardSubCategoryId='" + projectPlanningSubCategory + @"'AND PP.CompanyId='" + identity.CompanyId + "' ";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public TestingStandardDetail Get(string testingStandardId)
        {
            return Query(r => r.TestingStandardId == testingStandardId).Select().FirstOrDefault();
        }
    }
}