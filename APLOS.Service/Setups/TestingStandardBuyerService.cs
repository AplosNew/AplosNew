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
    public class TestingStandardBuyerService : Service<TestingStandardBuyer>, ITestingStandardBuyerService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public TestingStandardBuyerService(
            IRepositoryAsync<TestingStandardBuyer> projectPlanningDetailRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(projectPlanningDetailRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<TestingStandardBuyer> entity, string testingStandardId)
        {
            try
            {
                if (entity != null)
                {
                    string _pk = GetPK();
                    var count = 0;
                    var from_db = QueryForTestingStandardBuyer(testingStandardId).ToList();
                    foreach (var item in entity)
                    {
                        var existItem = QueryForTestingStandardBuyer(testingStandardId, item.BuyerId);
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

        public IEnumerable<TestingStandardBuyer> GetDetailList(string SalesOrderInvoicePackingListId)
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
            return "PPD-" + GetAutoNumber(nameof(TestingStandardBuyer), PKGeneratorEnum.Auto, null, DateTime.Now);
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

        public IEnumerable<Object> QueryForTestingStandardBuyer(string testingStandardId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT TSB.*,B.Code,B.ShortName,B.StandardName,B.UserName FROM [SCS].[TestingStandardBuyer] AS TSB
                                LEFT OUTER JOIN [HKP].[Buyer] AS B ON TSB.BuyerId = B.Id
                                WHERE TSB.TestingStandardId='" + testingStandardId + "' ";
                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<Object> QueryForTestingStandardBuyer(string testingStandardId, string buyerid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT TSB.*,B.Code,B.ShortName,B.StandardName,B.UserName FROM [SCS].[TestingStandardBuyer] AS TSB
                                LEFT OUTER JOIN [HKP].[Buyer] AS B ON TSB.BuyerId = B.Id
                                WHERE TSB.TestingStandardId='" + testingStandardId + "' And TSB.BuyerId='" + buyerid + "' ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel QueryForTestingStandardBuyerWithTSId(GridParameter parameters, string testingStandardId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT PPD.*,PPC.UserName AS TestingStandardCategory,PPSC.UserName AS TestingStandardSubCategory ,BM.GL,BM.BudgetCategory,BM.BudgetSubCategory,BM.BudgetItem, BM.BudgetGroup    FROM [MST].[TestingStandard] AS PP
                                        LEFT OUTER JOIN [MST].[TestingStandardBuyer] AS PPD ON PP.Id=PPD.TestingStandardId
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
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel QueryForTestingStandardBuyerWithPPIdAndCat(GridParameter parameters, string testingStandardId, string projectPlanningCategory, string projectPlanningSubCategory)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT PPD.*,PPC.UserName AS TestingStandardCategory,PPSC.UserName AS TestingStandardSubCategory ,BM.GL,BM.BudgetCategory,BM.BudgetSubCategory,BM.BudgetItem, BM.BudgetGroup    FROM [MST].[TestingStandard] AS PP
                                        LEFT OUTER JOIN [MST].[TestingStandardBuyer] AS PPD ON PP.Id=PPD.TestingStandardId
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
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public TestingStandardBuyer Get(string testingStandardId)
        {
            return Query(r => r.TestingStandardId == testingStandardId).Select().FirstOrDefault();
        }
    }
}