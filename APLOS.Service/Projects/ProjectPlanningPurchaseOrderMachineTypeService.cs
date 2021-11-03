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
    public partial class ProjectPlanningPurchaseOrderMachineTypeService : Service<ProjectPlanningPurchaseOrderMachineType>, IProjectPlanningPurchaseOrderMachineTypeService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public ProjectPlanningPurchaseOrderMachineTypeService(
            IRepositoryAsync<ProjectPlanningPurchaseOrderMachineType> projectPlanningPurchaseOrderMachineTypeRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningPurchaseOrderMachineTypeRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<ProjectPlanningPurchaseOrderMachineType> entity, string ProjectPlanningPurchaseOrderId)
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
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.Id = _pk + "-" + count;
                            item.ProjectPlanningPurchaseOrderId = ProjectPlanningPurchaseOrderId;
                            InsertGraph(item);
                        }
                        else if (!string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.ProjectPlanningPurchaseOrderId))
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

        private string GetPK()
        {
            return "PPFM-" + GetAutoNumber(nameof(ProjectPlanningPurchaseOrderMachineType), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void DeleteGraph(string projectPlanningPurchaseOrderDetailId)
        {
            try
            {
                var data = Query(r => r.ProjectPlanningPurchaseOrderDetailId == projectPlanningPurchaseOrderDetailId).Select().ToList();
                if (data != null)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
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

        public IEnumerable<Object> QueryForProjectPlanningPurchaseOrderMachineType(string plantId, string projectPlanningPurchaseOrderId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT PPD.*,BM.GL,BM.BudgetCategory,BM.BudgetSubCategory,BM.BudgetItem   FROM [MST].[ProjectPlanningPurchaseOrder] AS PP
LEFT OUTER JOIN [MST].[ProjectPlanningPurchaseOrderMachineType] AS PPD ON PP.Id=PPD.ProjectPlanningId
LEFT OUTER JOIN (SELECT BM.*, BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, BI.UserName AS BudgetItem, BG.UserName AS BudgetGroup, GLGI.AccountCode +' - '+GLGI.UserName AS GL FROM [MST].[BudgetMaster] BM
                                        LEFT OUTER JOIN [HKP].[BudgetCategory]  AS BC on BC.Id = BM.BudgetCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetSubCategory] AS BSC on BSC.Id = BM.BudgetSubCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetGroup] AS bg on BM.BudgetGroupId = bg.Id
                                        INNER JOIN [HKP].[Budget] BI ON BI.Id = BM.BudgetId
                                        INNER JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
										) AS BM ON PPD.BudgetMasterId=BM.Id
                                    WHERE PP.Id='" + projectPlanningPurchaseOrderId + @"' AND PP.PlantId='" + plantId + @"' AND PP.CompanyId='" + identity.CompanyId + "' ";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ProjectPlanningPurchaseOrderMachineType Get(string projectPlanningPurchaseOrderId)
        {
            return Query(r => r.ProjectPlanningPurchaseOrderId == projectPlanningPurchaseOrderId).Select().FirstOrDefault();
        }

        public GridModel ProjectplanninPurchaseOrderMachineTypeMasterList(GridParameter parameters, string projectPlanningPurchaseOrderDetailId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT   POMT.*,POUOM.UserName AS POMachineTypeUomName
                                ,PPMT.Quantity AS ProjectPlanningMachineTypeQuantity
                                ,pomt.Quantity as kk
                                ,UOM.UserName AS PlanningUOM,PPMT.PlanningUOMId
                                ,MT.UserName AS AssetItemName
                                ,sss.ReverseTotalQuantity - pomt.ReverseQuantity as ReverseTotalQuantity
                                FROM
                                (

								SELECT   pppo.ProjectPlanningMachineTypeId,SUM(PPPO.ReverseQuantity) AS ReverseTotalQuantity
								FROM [MST].[ProjectPlanningMachineType] AS PPMT
										right OUTER JOIN [MST].[ProjectPlanningPurchaseOrderMachineType] AS PPPO ON PPMT.Id=PPPO.ProjectPlanningMachineTypeId
										--where ppmt.ProjectPlanningId = 'PPC-120'
										group by  pppo.ProjectPlanningMachineTypeId
										) as sss
                            left outer join [MST].[ProjectPlanningPurchaseOrderMachineType] AS POMT on pomt.ProjectPlanningMachineTypeId=sss.ProjectPlanningMachineTypeId
							LEFT OUTER JOIN [MST].[ProjectPlanningMachineType] AS PPMT ON POMT.ProjectPlanningMachineTypeId = PPMT.Id
							LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOM ON PPMT.PlanningUOMId=UOM.Id
							LEFT OUTER JOIN SCS.UnitOfMeasurement AS POUOM ON POMT.POMachineTypeUomId=POUOM.Id
                            LEFT OUTER JOIN MST.[AssetItem] AS MT ON PPMT.AssetItemId = MT.Id
                            LEFT OUTER JOIN (SELECT * FROM HKP.CompanyGroupAssetItem WHERE CompanyGroupId = '" + identity.CompanyGroupId + @"') cgmt ON mt.Id = cgmt.AssetItemId
                            WHERE POMT.ProjectPlanningPurchaseOrderDetailId ='" + projectPlanningPurchaseOrderDetailId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}