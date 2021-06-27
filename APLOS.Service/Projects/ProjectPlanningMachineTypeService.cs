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
    public partial class ProjectPlanningMachineTypeService : Service<ProjectPlanningMachineType>, IProjectPlanningMachineTypeService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public ProjectPlanningMachineTypeService(
            IRepositoryAsync<ProjectPlanningMachineType> projectPlanningMachineTypeRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningMachineTypeRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<ProjectPlanningMachineType> entity, string projectPlanningId)
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

        private string GetPK()
        {
            return "PPFM-" + GetAutoNumber(nameof(ProjectPlanningMachineType), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void DeleteGraph(string projectPlanningDetailId)
        {
            try
            {
                var data = Query(r => r.ProjectPlanningDetailId == projectPlanningDetailId).Select().ToList();
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

        public IEnumerable<Object> QueryForProjectPlanningMachineType(string plantId, string projectPlanningId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT PPD.*,BM.GL,BM.BudgetCategory,BM.BudgetSubCategory,BM.BudgetItem   FROM [MST].[ProjectPlanning] AS PP
LEFT OUTER JOIN [MST].[ProjectPlanningMachineType] AS PPD ON PP.Id=PPD.ProjectPlanningId
LEFT OUTER JOIN (SELECT BM.*, BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, BI.UserName AS BudgetItem, BG.UserName AS BudgetGroup, GLGI.AccountCode +' - '+GLGI.UserName AS GL FROM [MST].[BudgetMaster] BM
                                        LEFT OUTER JOIN [HKP].[BudgetCategory]  AS BC on BC.Id = BM.BudgetCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetSubCategory] AS BSC on BSC.Id = BM.BudgetSubCategoryId
                                        LEFT OUTER JOIN [HKP].[BudgetGroup] AS bg on BM.BudgetGroupId = bg.Id
                                        INNER JOIN [HKP].[Budget] BI ON BI.Id = BM.BudgetId
                                        INNER JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
										) AS BM ON PPD.BudgetMasterId=BM.Id
WHERE PP.Id='" + projectPlanningId + @"' AND PP.PlantId='" + plantId + @"' AND PP.CompanyId='" + identity.CompanyId + "' ";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ProjectPlanningMachineType Get(string projectPlanningId)
        {
            return Query(r => r.ProjectPlanningId == projectPlanningId).Select().FirstOrDefault();
        }

        public GridModel ProjectplanninMachineTypeMasterList(GridParameter parameters, string projectPlanningDetailId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT PPF.*,MT.UserName AS AssetItemName,FAC.UserName AS AssetClassName,FASC.UserName AS AssetSubClassName,FAM.UserName AS FixedAssetMasterName, PPPOMT.ReverseTotalQuantity,PUOM.UserName AS PlanningUOM,BUOM.UserName AS BaseUOM  FROM [MST].[ProjectPlanningMachineType] AS PPF
							LEFT OUTER JOIN (SELECT ProjectPlanningMachineTypeId,SUM(ReverseQuantity) ReverseTotalQuantity FROM  [MST].[ProjectPlanningPurchaseOrderMachineType] Group By ProjectPlanningMachineTypeId)AS PPPOMT ON PPF.Id=PPPOMT.ProjectPlanningMachineTypeId
							LEFT OUTER JOIN MST.[AssetItem] AS mt ON PPF.AssetItemId = MT.Id
                            LEFT OUTER JOIN (SELECT * FROM HKP.CompanyGroupAssetItem WHERE CompanyGroupId = '" + identity.CompanyGroupId + @"') cgmt ON mt.Id = cgmt.AssetItemId
							LEFT OUTER JOIN HKP.FixedAssetClass AS FAC ON MT.FixedAssetClassId=FAC.Id
							LEFT OUTER JOIN HKP.FixedAssetSubClass AS FASC ON MT.FixedAssetSubClassId=FASC.Id
							LEFT OUTER JOIN MST.FixedAssetMaster AS FAM ON MT.FixedAssetMasterId=FAM.Id
							LEFT OUTER JOIN SCS.UnitOfMeasurement AS PUOM ON PPF.PlanningUOMId=PUOM.Id
							LEFT OUTER JOIN SCS.UnitOfMeasurement AS BUOM ON MT.BaseUOMId = BUOM.Id
                            WHERE PPF.ProjectPlanningDetailId ='" + projectPlanningDetailId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel ProjectplanninMaterialMasterList(GridParameter parameters, string projectPlanningDetailId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAM.Id,FAM.UserName,FA.UserName AS FixedAsset, FAC.UserName AS FixedAssetCategory,FASC.UserName AS FixedAssetSubCategory,FAC.UserName AS FixedAssetClass,FASC.UserName AS FixedAssetSubClass, PPF.Quantity,PPF.MachineTypeId,MT.UserName As MachineTypeName FROM [MST].[FixedAssetMaster] FAM
                            LEFT OUTER JOIN [HKP].[FixedAsset] FA ON FAM.FixedAssetId = FA.Id
                            LEFT OUTER JOIN [HKP].[FixedAssetCategory] FAC ON FAM.FixedAssetCategoryId = FAC.Id
                            LEFT OUTER JOIN [HKP].[FixedAssetSubCategory] FASC ON FAM.FixedAssetSubCategoryId = FASC.Id
                            LEFT OUTER JOIN [HKP].[FixedAssetClass] FAAC ON FAM.FixedAssetClassId = FAAC.Id
                            LEFT OUTER JOIN [HKP].[FixedAssetSubClass] FAASC ON FAM.FixedAssetSubClassId = FAASC.Id
                            LEFT OUTER JOIN [MST].[ProjectPlanningMachineType] PPF ON FAM.Id=PPF.FixedAssetMasterId
                            LEFT OUTER JOIN [MST].[FixedAssetMasterMachineType] FMMT ON PPF.MachineTypeId = FMMT.Id
                            LEFT OUTER JOIN [MST].[MachineType] MT ON FMMT.MachineTypeId=MT.Id
                            WHERE PPF.ProjectPlanningDetailId ='" + projectPlanningDetailId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<Object> getUomList(string assetItemId)
        {
            string search1 = null;
            string search2 = null;
            if (!string.IsNullOrEmpty(assetItemId))
            {
                search1 = "And AST.Id='" + assetItemId + @"'";
                search2 = "WHERE AUOM.AssetItemId = '" + assetItemId + @"'";
            }
            var sql = @"SELECT DISTINCT UOM1.Id AS UoMID,UOM1.UserName AS UoM,AST.Id FROM MST.AssetItem AS AST
                        LEFT  JOIN SCS.UnitOfMeasurement AS UOM1 ON AST.BaseUOMId = UOM1.Id
                        WHERE UOM1.Id <>'' " + search1 + @"
                        UNION
                        SELECT DISTINCT UOM2.Id AS UoMID, UOM2.UserName AS UoM,AUOM.AssetItemId AS Id FROM MST.AssetItemAlternativeUOM AS AUOM
                        LEFT  JOIN  SCS.UnitOfMeasurement AS UOM2 ON AUOM.AlternativeUOMId = UOM2.Id
                        " + search2 + "";
            return _sqlRepository.GetDataCollection(sql, null);
        }
    }
}