using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Library.Service.Materials
{
    public partial class FabricRollManagementSettingsService : Service<FabricRollManagementSettings>, IFabricRollManagementSettingsService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public FabricRollManagementSettingsService(
            IRepositoryAsync<FabricRollManagementSettings> managementSettingsRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork)
            : base(managementSettingsRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string[] tempParam)
        {
            try
            {
                string str = "";
                str += CreateWhereCluse(tempParam[0], "MM.Code", str);
                str += CreateWhereCluse(tempParam[1], "MM.ShortName", str);
                str += CreateWhereCluse(tempParam[2], "MM.StandardName", str);
                str += CreateWhereCluse(tempParam[3], "MM.UserName", str);
                str += CreateWhereCluse(tempParam[4], "UoM.UserName", str);
                str += CreateWhereCluse(tempParam[5], "C1.UserName", str);
                str += CreateWhereCluse(tempParam[6], "C2.UserName", str);
                str += CreateWhereCluse(tempParam[7], "C3.UserName", str);
                if (!string.IsNullOrEmpty(str))
                    str = str.Insert(0, " WHERE ");
                parameters.CmdText = @"SELECT A.Id
	                                 , A.MaterialMasterId , MM.Code, MM.ShortName, MM.StandardName, MM.UserName, UoM.UserName AS BaseUoM
	                                 , A.BlanketLengthBeforeWash
	                                 , A.BlanketWidthBeforeWash
	                                 , A.Characteristics1Id, C1.UserName AS Characteristics1Name
	                                 , A.Characteristics2Id, C2.UserName AS Characteristics2Name
	                                 , A.Characteristics3Id, C3.UserName AS Characteristics3Name
	                                 , IsDimension1=CASE WHEN A.Characteristics1Id<>'' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
	                                 , IsDimension2=CASE WHEN A.Characteristics2Id<>'' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
	                                 , IsDimension3=CASE WHEN A.Characteristics3Id<>'' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
                                FROM MST.FabricRollManagementSettings AS A
                                INNER JOIN MST.MaterialMaster AS MM ON A.MaterialMasterId=MM.Id
                                INNER JOIN SCS.UnitOfMeasurement AS UoM ON MM.BaseUOMId=UoM.Id
                                LEFT OUTER JOIN HKP.Characteristics AS C1 ON A.Characteristics1Id=C1.Id
                                LEFT OUTER JOIN HKP.Characteristics AS C2 ON A.Characteristics2Id=C2.Id
                                LEFT OUTER JOIN HKP.Characteristics AS C3 ON A.Characteristics3Id=C3.Id" + str;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private static string CreateWhereCluse(string fieldValue, string sqlField, string conditionVariable)
        {
            if (!string.IsNullOrEmpty(fieldValue))
            {
                var str = "";
                if (!string.IsNullOrEmpty(conditionVariable)) str = " AND ";
                return str + sqlField + @" LIKE ('%" + fieldValue + "%')";
            }
            return string.Empty;
        }

        public GridModel GetMaterialMasterList(GridParameter parameters, string companyGroupId, string[] searchParam)
        {
            try
            {
                string sqlParam = "";
                sqlParam += CreateWhereCluse(searchParam[0], "MT.Description", sqlParam);
                sqlParam += CreateWhereCluse(searchParam[1], "MGP.UserName", sqlParam);
                sqlParam += CreateWhereCluse(searchParam[2], "MC.UserName", sqlParam);
                sqlParam += CreateWhereCluse(searchParam[3], "MSC.UserName", sqlParam);
                if (!string.IsNullOrEmpty(sqlParam))
                    sqlParam = sqlParam.Insert(0, " AND ");

                parameters.CmdText = @"SELECT MT.Description AS MaterialType
                                      , MGP.UserName AS MaterialGroupMaster
                                      , PM.UserName AS ProductMaster
                                      , UOMB.UserName AS BaseUom
                                      , MC.UserName MaterialCategory
	                                  , MSC.UserName MaterialSubCategory
                                      , MM.Id AS MaterialMasterId
                                      , MM.Sequence,MM.Code,MM.ShortName,MM.StandardName,MM.UserName, SKU=CASE WHEN MM.WithSKU=1 THEN 'Yes' ELSE 'No' END
                                      , Active=CASE WHEN MM.Active=1 THEN 'Yes' ELSE 'No' END
                                      , FAM.UserName AS AssetMaster, FAM.AssetType
                                      , B.Code AS AssetBudgetCode
                                      , Revenue=CASE WHEN (MM.IsInventory=1 OR MM.IsExpenseOut=1) THEN 'Yes' ELSE 'No' END
                FROM [MST].[MaterialMaster] AS MM
                LEFT JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                LEFT JOIN [HKP].[MaterialCategory] AS MC ON MM.MaterialCategoryId = MC.Id
                LEFT JOIN [HKP].[MaterialSubCategory] AS MSC ON MM.MaterialSubCategoryId = MSC.Id
                LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON MM.AssetMasterId = FAM.Id
                LEFT JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
                INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                LEFT JOIN MST.BudgetMaster AS BM ON MM.BudgetMasterId=BM.Id
                LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                WHERE MM.CompanyGroupId = '" + companyGroupId + @"' AND MM.Archive = 0 AND MM.Active = 1
                AND MM.Id IN(SELECT MaterialMasterId FROM MST.MaterialMasterBusinessProcess AS A INNER JOIN SCS.BusinessProcess AS B ON A.BusinessProcessId=B.Id
							WHERE B.BusinessProcessName='" + BusinessProcessEnum.FabricRollManagement+ "')" + sqlParam;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                  Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                  ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetCharacteristicsList(string materialMasterId)
        {
            try
            {
                var _sql = @"SELECT A.CharacteristicsId , B.UserName AS Characteristics, A.[Sequence] FROM MST.MaterialMasterCharacteristics AS A
                            INNER JOIN HKP.Characteristics AS B ON A.CharacteristicsId=B.Id WHERE A.MaterialMasterId='" + materialMasterId + "' ORDER BY A.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                  Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                  ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(FabricRollManagementSettings), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(FabricRollManagementSettings entity)
        {
            try
            {
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public override void Update(FabricRollManagementSettings entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void Delete(string id)
        {
            try
            {
                var entity = Find(id);
                base.Delete(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }
    }
}