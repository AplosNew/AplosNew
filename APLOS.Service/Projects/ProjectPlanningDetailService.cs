#region Using

using Library.Core;
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
    public partial class ProjectPlanningDetailService : Service<ProjectPlanningDetail>, IProjectPlanningDetailService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IProjectPlanningMachineTypeService _projectPlanningFixedAssetService;
        private readonly IProjectPlanningMaterialMasterService _projectPlanningMaterialService;
        private readonly IRepositoryAsync<ProjectPlanningDetail> _projectPlanningDetailRepository;

        public ProjectPlanningDetailService(
            IRepositoryAsync<ProjectPlanningDetail> projectPlanningDetailRepository
                        , IProjectPlanningMachineTypeService projectPlanningFixedAssetService
            , IProjectPlanningMaterialMasterService projectPlanningMaterialService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningDetailRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
            _projectPlanningFixedAssetService = projectPlanningFixedAssetService;
            _projectPlanningMaterialService = projectPlanningMaterialService;
            _projectPlanningDetailRepository = projectPlanningDetailRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<ProjectPlanningDetail> entity, string projectPlanningId)
        {
            try
            {
                if (entity != null)
                {
                    string _pk = GetPK();
                    var count = 0;
                    foreach (var item in entity)
                    {
                        count++;
                        if (string.IsNullOrEmpty(item.Id) && string.IsNullOrEmpty(item.ProjectPlanningId))
                        {
                            var isAvailable = Query(r => r.BudgetMasterId == item.BudgetMasterId && r.ProjectPlanningId == projectPlanningId).Select().FirstOrDefault();
                            if (isAvailable != null)
                            {
                                var bName = getMatchName(item.BudgetMasterId);

                                throw new CustomException(bName + " Is already saved");
                            }
                            item.Id = _pk + "-" + count;
                            item.ProjectPlanningId = projectPlanningId;
                            InsertGraph(item);
                        }
                        else if (!string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.ProjectPlanningId))
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

        //private string getMatchName(string budgetMasterId)
        //{
        //    return _projectPlanningDetailRepository.ExecuteSqlCommand(@"SELECT B.UserName FROM MST.BudgetMaster BM LEFT JOIN HKP.Budget B ON BM.BudgetId=B.Id  WHERE BM.Id='"+budgetMasterId+@"'");
        //}
        private string getMatchName(string budgetMasterId)
        {
            try
            {
                string _sql = @"SELECT B.UserName FROM MST.BudgetMaster BM LEFT JOIN HKP.Budget B ON BM.BudgetId=B.Id  WHERE BM.Id='" + budgetMasterId + @"'";
                var s = _projectPlanningDetailRepository.SqlQuery<string>(_sql).FirstOrDefault();
                return s;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            return "PPD-" + GetAutoNumber(nameof(ProjectPlanningDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void DeleteWithMaster(string Id)
        {
            try
            {
                var data = Query(r => r.ProjectPlanningId == Id).Select().ToList();
                if (data != null)
                {
                    for (int i = 0; i < data.Count(); i++)
                    {
                        _projectPlanningMaterialService.DeleteGraph(data[i].Id);
                        base.DeleteGraph(data[i]);
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
                    //_projectPlanningFixedAssetService.DeleteGraph(Id);
                    _projectPlanningMaterialService.DeleteGraph(Id);
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

        public IEnumerable<Object> QueryForProjectPlanningDetail(string plantId, string projectPlanningId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT PPD.*,PPC.UserName AS ProjectPlanningCategory,PPSC.UserName AS ProjectPlanningSubCategory ,BM.GLGeneralInfoCode,BM.GLGeneralInfoName,BM.BudgetCategory,BM.BudgetSubCategory,BM.BudgetId,BM.BudgetName,BM.BudgetGroup   FROM [MST].[ProjectPlanning] AS PP
LEFT OUTER JOIN [MST].[ProjectPlanningDetail] AS PPD ON PP.Id=PPD.ProjectPlanningId
LEFT OUTER JOIN [HKP].[ProjectPlanningCategory] PPC ON PPD.ProjectPlanningCategoryId = PPC.Id
LEFT OUTER JOIN [HKP].[ProjectPlanningSubCategory] PPSC ON PPD.ProjectPlanningSubCategoryId = PPSC.Id
LEFT OUTER JOIN (SELECT BM.*, BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, BI.UserName AS BudgetName,GLGI.AccountCode, BG.UserName AS BudgetGroup, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName FROM [MST].[BudgetMaster] BM
                                        LEFT OUTER JOIN [HKP].[BudgetCategory]  AS BC on BC.Id = BM.BudgetCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetSubCategory] AS BSC on BSC.Id = BM.BudgetSubCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetGroup] AS bg on BM.BudgetGroupId = bg.Id
                                        INNER JOIN [HKP].[Budget] BI ON BI.Id = BM.BudgetId
                                        INNER JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
										) AS BM ON PPD.BudgetMasterId=BM.Id
WHERE PP.Id='" + projectPlanningId + @"' AND PP.PlantId='" + plantId + @"' AND PP.CompanyId='" + identity.CompanyId + "'  ORDER BY  PPC.UserName,PPSC.UserName,BM.BudgetName,BM.AccountCode";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel QueryForProjectPlanningDetailWithPPId(GridParameter parameters, string projectPlanningId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT PPD.*,PPC.UserName AS ProjectPlanningCategory,PPSC.UserName AS ProjectPlanningSubCategory ,BM.GL,BM.BudgetCategory,BM.BudgetSubCategory,BM.BudgetItem, BM.BudgetGroup    FROM [MST].[ProjectPlanning] AS PP
LEFT OUTER JOIN [MST].[ProjectPlanningDetail] AS PPD ON PP.Id=PPD.ProjectPlanningId
LEFT OUTER JOIN [HKP].[ProjectPlanningCategory] PPC ON PPD.ProjectPlanningCategoryId = PPC.Id
LEFT OUTER JOIN [HKP].[ProjectPlanningSubCategory] PPSC ON PPD.ProjectPlanningSubCategoryId = PPSC.Id
RIGHT OUTER JOIN (SELECT BM.*, BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, BI.UserName AS BudgetItem, BG.UserName AS BudgetGroup,GLGI.AccountCode, GLGI.AccountCode +' - '+GLGI.UserName AS GL FROM [MST].[BudgetMaster] BM
                                        LEFT OUTER JOIN [HKP].[BudgetCategory]  AS BC on BC.Id = BM.BudgetCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetSubCategory] AS BSC on BSC.Id = BM.BudgetSubCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetGroup] AS bg on BM.BudgetGroupId = bg.Id
                                        INNER JOIN [HKP].[Budget] BI ON BI.Id = BM.BudgetId
                                        INNER JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
										) AS BM ON PPD.BudgetMasterId=BM.Id
WHERE PP.Id='" + projectPlanningId + @"'  AND PP.CompanyId='" + identity.CompanyId + "'  ORDER BY BM.AccountCode,BM.BudgetItem";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel QueryForProjectPlanningDetailWithPPIdAndCat(GridParameter parameters, string projectPlanningId, string projectPlanningCategory, string projectPlanningSubCategory)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT PPD.*,PPC.UserName AS ProjectPlanningCategory,PPSC.UserName AS ProjectPlanningSubCategory ,BM.GL,BM.BudgetCategory,BM.BudgetSubCategory,BM.BudgetItem, BM.BudgetGroup    FROM [MST].[ProjectPlanning] AS PP
                                        LEFT OUTER JOIN [MST].[ProjectPlanningDetail] AS PPD ON PP.Id=PPD.ProjectPlanningId
                                        LEFT OUTER JOIN [HKP].[ProjectPlanningCategory] PPC ON PPD.ProjectPlanningCategoryId = PPC.Id
                                        LEFT OUTER JOIN [HKP].[ProjectPlanningSubCategory] PPSC ON PPD.ProjectPlanningSubCategoryId = PPSC.Id
                                        RIGHT OUTER JOIN (SELECT BM.*, BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, BI.UserName AS BudgetItem, BG.UserName AS BudgetGroup, GLGI.AccountCode +' - '+GLGI.UserName AS GL FROM [MST].[BudgetMaster] BM
                                        LEFT OUTER JOIN [HKP].[BudgetCategory]  AS BC on BC.Id = BM.BudgetCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetSubCategory] AS BSC on BSC.Id = BM.BudgetSubCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetGroup] AS bg on BM.BudgetGroupId = bg.Id
                                        INNER JOIN [HKP].[Budget] BI ON BI.Id = BM.BudgetId
                                        INNER JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
										) AS BM ON PPD.BudgetMasterId=BM.Id
WHERE PP.Id='" + projectPlanningId + @"'  AND PPD.ProjectPlanningCategoryId='" + projectPlanningCategory + @"'AND PPD.ProjectPlanningSubCategoryId='" + projectPlanningSubCategory + @"'AND PP.CompanyId='" + identity.CompanyId + "' ";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ProjectPlanningDetail Get(string projectPlanningId)
        {
            return Query(r => r.ProjectPlanningId == projectPlanningId).Select().FirstOrDefault();
        }
    }
}