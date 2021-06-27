#region Using

using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Projects;
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

namespace Library.Service.Projects
{
    /// <summary>
    ///  Class ProductService.
    /// </summary>
    public partial class ProjectPlanningPurchaseOrderDetailService : Service<ProjectPlanningPurchaseOrderDetail>, IProjectPlanningPurchaseOrderDetailService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IProjectPlanningPurchaseOrderMaterialMasterService _projectPlanningPurchaseOrderMaterialService;

        public ProjectPlanningPurchaseOrderDetailService(
            IRepositoryAsync<ProjectPlanningPurchaseOrderDetail> projectPlanningPurchaseOrderDetailRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
             , IProjectPlanningPurchaseOrderMaterialMasterService projectPlanningPurchaseOrderMaterialService
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningPurchaseOrderDetailRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _projectPlanningPurchaseOrderMaterialService = projectPlanningPurchaseOrderMaterialService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<ProjectPlanningPurchaseOrderDetail> entity, string projectPlanningPurchaseOrderId)
        {
            try
            {
                if (entity != null)
                {
                    var pk = GetMaxNumber(nameof(ProjectPlanningPurchaseOrderDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entity)
                    {
                        if (string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(projectPlanningPurchaseOrderId))
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            item.ProjectPlanningPurchaseOrderId = projectPlanningPurchaseOrderId;
                            InsertGraph(item);
                        }
                        else if (!string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(projectPlanningPurchaseOrderId))
                        {
                            UpdateGraph(item);
                        }
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

        public void DeleteWithMaster(string Id)
        {
            try
            {
                var data = Query(r => r.ProjectPlanningPurchaseOrderId == Id).Select().ToList();
                if (data != null)
                {
                    for (int i = 0; i < data.Count(); i++)
                    {
                        _projectPlanningPurchaseOrderMaterialService.DeleteGraph(data[i].Id);
                        DeleteGraph(data[i]);
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

        public void DeleteWithChild(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Query(r => r.Id == Id).Select().FirstOrDefault();
                if (data != null)
                {
                    _projectPlanningPurchaseOrderMaterialService.DeleteGraph(Id);
                    DeleteGraph(data);
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

        public IEnumerable<Object> QueryForProjectPlanningPurchaseOrderDetail(string projectPlanningPurchaseOrderId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT PPPOD.*,FT.TotalQuantity,FT.TotalAmount
	                                   ,PPC.UserName AS ProjectPlanningCategory
	                                   ,PPSC.UserName AS ProjectPlanningSubCategory
	                                   ,PPD.Amount AS PlanningAmount
                                       ,BM.GL
                                       ,BM.BudgetCategory
                                       ,BM.BudgetSubCategory
                                       ,BM.BudgetItem
                                       ,BM.BudgetGroup
                                       FROM [MST].[ProjectPlanningPurchaseOrderDetail] AS PPPOD
                                       LEFT OUTER JOIN [MST].[ProjectPlanningPurchaseOrder] AS PPPO ON PPPOD.ProjectPlanningPurchaseOrderId=PPPO.Id
                                       LEFT OUTER JOIN [MST].[ProjectPlanningDetail] AS PPD ON PPPOD.ProjectPlanningDetailId = PPD.Id
                                       LEFT OUTER JOIN [HKP].[ProjectPlanningCategory] AS PPC ON PPD.ProjectPlanningCategoryId = PPC.Id
                                       LEFT OUTER JOIN [HKP].[ProjectPlanningSubCategory] AS PPSC ON PPD.ProjectPlanningSubCategoryId = PPSC.Id
                                       LEFT OUTER JOIN (SELECT BM.*, BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, BI.UserName AS BudgetItem, BG.UserName AS BudgetGroup, GLGI.AccountCode +' - '+GLGI.UserName AS GL FROM [MST].[BudgetMaster] BM
                                       LEFT OUTER JOIN [HKP].[BudgetCategory]  AS BC on BC.Id = BM.BudgetCategoryId
                                       LEFT OUTER JOIN [HKP].[BudgetSubCategory] AS BSC on BSC.Id = BM.BudgetSubCategoryId
                                       LEFT OUTER JOIN [HKP].[BudgetGroup] AS bg on BM.BudgetGroupId = bg.Id
                                       INNER JOIN [HKP].[Budget] BI ON BI.Id = BM.BudgetId
                                       INNER JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
	                                   ) AS BM ON PPD.BudgetMasterId=BM.Id
	                                   LEFT OUTER JOIN (SELECT PPPOMF.ProjectPlanningPurchaseOrderDetailId,  SUM(PPPOM.Amount) AS TotalAmount,SUM(PPPOM.Quantity) AS TotalQuantity FROM  [MST].[ProjectPlanningPurchaseOrderMaterialMaster] AS PPPOMF
	   		                                LEFT OUTER JOIN (SELECT Id, Quantity,Rate,Quantity*Rate AS Amount FROM [MST].[ProjectPlanningPurchaseOrderMaterialMaster]) AS PPPOM ON PPPOMF.Id = PPPOM.Id group by PPPOMF.ProjectPlanningPurchaseOrderDetailId)  AS FT ON PPPOD.Id = FT.ProjectPlanningPurchaseOrderDetailId
                                WHERE PPPOD.ProjectPlanningPurchaseOrderId='" + projectPlanningPurchaseOrderId + "'";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ProjectPlanningPurchaseOrderDetail Get(string projectPlanningPurchaseOrder)
        {
            return Query(r => r.ProjectPlanningPurchaseOrderId == projectPlanningPurchaseOrder).Select().FirstOrDefault();
        }
    }
}