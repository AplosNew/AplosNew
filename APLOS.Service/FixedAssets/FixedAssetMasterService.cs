using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.FixedAssets;
using Library.Model.Systems;
using Library.Service.Accounts;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.FixedAssets
{
    public class FixedAssetMasterService : Service<FixedAssetMaster>, IFixedAssetMasterService
    {
        #region Constructor

        private readonly IRepositoryAsync<FixedAssetMasterGL> _materialMasterGLRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IFixedAssetMasterGLService _fixedAssetGL;

        public FixedAssetMasterService(
            IRepositoryAsync<FixedAssetMaster> fixedAssetMasterRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IFixedAssetMasterGLService fixedAssetGL
            , IRepositoryAsync<FixedAssetMasterGL> fixedAssetGLRepository
            ) : base(fixedAssetMasterRepository, unitOfWork, pkGeneratorService)
        {
            _materialMasterGLRepository = fixedAssetGLRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _fixedAssetGL = fixedAssetGL;
        }

        #endregion Constructor

        private readonly string tbl_FixedAssetMaster = " " + DbSchema.Masters + ".[FixedAssetMaster] ";
        private readonly string tbl_FixedAssetCategory = " " + DbSchema.HKP + ".[FixedAssetCategory] ";
        private readonly string tbl_FixedAssetSubCategory = " " + DbSchema.HKP + ".[FixedAssetSubCategory] ";

        private string GetPK()
        {
            return GetAutoNumber(nameof(FixedAssetMaster), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private PKGenerator GetMaxNumber()
        {
            return _fixedAssetGL.GetMaxNumber("FixedAssetGL", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InsertUpdateFixAssetMaster(FixedAssetMaster entity)
        {
            var flag = false;
            try
            {
                CheckUnique(entity);
                //CheckUniqueCombine(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                const int i = 1;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPK() + "-" + i;
                    entity.CompanyGroupId = identity.CompanyGroupId;
                    InsertGraph(entity);
                }
                else
                {
                    //AssetTypeChangeValidation(entity.Id, entity.AssetType);
                    UpdateGraph(entity);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public override void Update(FixedAssetMaster entity)
        {
            var flag = false;
            try
            {
                if (entity.Id != null || entity.Id != "")
                {
                    _unitOfWork.BeginTransaction();
                    CheckUnique(entity);
                    CheckUniqueCombine(entity);
                    flag = true;
                    // If department row inacitve
                    base.Update(entity);

                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                else
                {
                    throw new CustomException("Please select FixedAsset Master to update");
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void CheckUnique(FixedAssetMaster entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id);
        }

        private void CheckUniqueCombine(FixedAssetMaster entity)
        {
            var db_Data = base.Query(t => t.Id != entity.Id && t.FixedAssetCategoryId == entity.FixedAssetCategoryId && t.FixedAssetSubCategoryId == entity.FixedAssetSubCategoryId).Select().FirstOrDefault();
            if (db_Data != null)
                throw new CustomException("This combination already exist on " + db_Data.UserName);
        }

        public GridModel GetSearch(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAM.*,
                        FAC.UserName 'FixedAssetCategory',
                        FASC.UserName 'FixedAssetSubCategory'
                        FROM  MST.[FixedAssetMaster]  FAM
                        LEFT OUTER JOIN  HKP.[FixedAssetCategory]  FAC ON FAM.FixedAssetCategoryId=FAC.Id
                        LEFT OUTER JOIN  HKP.[FixedAssetSubCategory]  FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                     WHERE FAM.CompanyGroupId='" + identity.CompanyGroupId + @"' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId, string[] ids)
        {
            try
            {
                parameters.CmdText = $"SELECT FAM.Id FixedAssetMasterId,FAM.UserName AS FixedAssetMasterName, FAC.UserName 'FixedAssetCategoryName',FASC.UserName 'FixedAssetSubCategoryName',FAM.AssetType FROM {tbl_FixedAssetMaster} FAM " +
                    $"LEFT OUTER JOIN {tbl_FixedAssetCategory} FAC ON FAM.FixedAssetCategoryId=FAC.Id " +
                    $"LEFT OUTER JOIN {tbl_FixedAssetSubCategory} FASC ON FAM.FixedAssetSubCategoryId=FASC.Id " +
                    $"WHERE CompanyGroupId='{companyGroupId}' AND FAM.Id NOT IN({ReturnStringArray(ids)})";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel QueryAsMaterialMaster(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT MM.Id, MM.Code, MM.UserName MaterialMaster, MGP.UserName AS MaterialGroupMaster, FAM.UserName AS AssetMaster, FAM.AssetType
                                        FROM [MST].[MaterialMaster] AS MM
                                        LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId=MGP.Id
                                        LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON MM.AssetMasterId=FAM.Id
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON MM.BudgetMasterId=BM.Id
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        WHERE MM.IsAsset=1 AND MM.Archive=0 AND MM.Active=1 AND FAM.CompanyGroupId='" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        /// <summary>
        /// For Tagging in BudgetMaster > activity > FA Linkage
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="companyGroupId"></param>
        /// <param name="budgetMasterId"></param>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public GridModel GetMaterialMasterAssetTypeList(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT NULL AS Id, MM.Id AS MaterialMasterId, FAM.Id FixedAssetMasterId, MT.Description AS MaterialType, MGP.UserName AS MaterialGroupMaster, MM.Code, MM.UserName AS AssetItem
                                        , FAC.UserName AS FixedAssetCategory, FASC.UserName AS FixedAssetSubCategory, FAM.UserName AS FixedAssetMaster, FAM.AssetType
                                        FROM [MST].[MaterialMaster] AS MM
										LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MGP.Id=MM.MaterialGroupMasterId
										LEFT JOIN [HKP].[MaterialType] AS MT ON MT.Id=MGP.MaterialTypeId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=MM.BudgetMasterId
										LEFT JOIN [HKP].[FixedAssetMasterBudgetTag] AS FAMT ON FAMT.BudgetMasterId=BM.Id AND FAMT.BudgetMasterId=MM.BudgetMasterId
										LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=FAMT.FixedAssetMasterId
										LEFT JOIN [HKP].[FixedAssetCategory] FAC ON FAM.FixedAssetCategoryId=FAC.Id
										LEFT JOIN [HKP].[FixedAssetSubCategory] FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        WHERE MM.IsAsset=1 AND MM.Archive=0 AND MM.Active=1 AND FAM.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel QueryWithType(GridParameter parameters, string type)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAM.Id, FAM.UserName,FAC.UserName 'FixedAssetCategory', FASC.UserName 'FixedAssetSubCategory', FAM.AssetType FROM  MST.[FixedAssetMaster]  FAM
                                        LEFT OUTER JOIN HKP.[FixedAssetCategory] FAC ON FAM.FixedAssetCategoryId=FAC.Id
                                        LEFT OUTER JOIN HKP.[FixedAssetSubCategory] FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                                        WHERE FAM.CompanyGroupId= '" + identity.CompanyGroupId + "' AND FAM.AssetType= '" + type + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel QueryWithTypeGl(GridParameter parameters, string type)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAM.Id, FAM.UserName, FAC.UserName 'FixedAssetCategory', FASC.UserName 'FixedAssetSubCategory', FG.FixedAssetGLId,FG.AssetBudgetId,FG.AssetActivityId,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS GL FROM  MST.[FixedAssetMaster]  FAM
                                        LEFT OUTER JOIN HKP.[FixedAssetCategory] FAC ON FAM.FixedAssetCategoryId=FAC.Id
                                        LEFT OUTER JOIN  HKP.[FixedAssetSubCategory] FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
										LEFT OUTER JOIN [HKP].[FixedAssetGL] FG ON FAM.Id=FG.FixedAssetMasterId
										LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=FG.FixedAssetGLId
                                        WHERE FAM.CompanyGroupId= '" + identity.CompanyGroupId + "' AND FAM.AssetType= '" + type + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetFixedAssetMasterDeterminateGL(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT FAM.Id AS FixedAssetMasterId, FAM.UserName AS FixedAssetMasterName
                                	, FAM.Code AS FACode, FAD.COAId
                                	, C.UserName AS COAName
                                	, FAD.FixedAssetGLId
                                	, FAD.AccumulatedDepreciationGLId
                                	, GL.UserName AS AssetGLName
                                	, GL1.UserName AS AccDepreciation
                                	, FOB.Id
                                    , FOB.OpeningBalanceId
                                	, FOB.CompanyId
                                	, FOB.Quantity
                                	, FOB.AddedBy
                                	, FOB.AddedDate
                                	, FOB.AddedFromIP
                                FROM MST.FixedAssetMaster AS FAM
                                LEFT JOIN HKP.FixedAssetGL AS FAD ON FAD.FixedAssetMasterId = FAM.Id
                                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = FAD.FixedAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GL1 ON GL1.Id = FAD.AccumulatedDepreciationGLId
                                LEFT JOIN ORG.Company AS COM ON COM.COAId = FAD.COAId
                                LEFT JOIN HKP.COA AS C ON C.Id = FAD.COAId
                                LEFT JOIN (
	                            SELECT F.*,FO.CompanyId
	                            FROM TRN.MaterialMasterOpeningBalanceDetail AS F
								LEFT JOIN TRN.OpeningBalance AS FO ON FO.Id=F.OpeningBalanceId
	                            ) AS FOB ON FOB.FixedAssetMasterId = FAM.Id";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in base.Query(r => r.Active).Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        [Obsolete("Have to shift in controller")]
        public static IEnumerable<object> GetFixedAssetItemCbo()
        {
            return Enum.GetValues(typeof(FixedAssetItemEnum)).Cast<FixedAssetItemEnum>().Select(v => new
            {
                Text = v.ToString(),
                Value = v.ToString()
            });
        }

        public void DeleteItem(string masterId)
        {
            var child = GetItem(masterId);
            if (child != null)
            {
                throw new CustomException("This item is used on transaction");
            }
            Delete(masterId);
        }

        public FixedAssetMasterGL GetItem(string masterId)
        {
            var _sql = @"select * from [HKP].[FixedAssetMasterGL] WHERE FixedAssetMasterId = '" + masterId + @"' ";
            return _materialMasterGLRepository.SelectQuery(_sql).FirstOrDefault();
        }

        public GridModel GetFixedAssetDetermineByMasterId(GridParameter parameters, string assetMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAD.FixedAssetGLId,FAD.AccumulatedDepreciationGLId
						  FROM MST.FixedAssetMaster AS FAM
						  LEFT JOIN [HKP].[FixedAssetGL] FAD ON FAD.FixedAssetMasterId=FAM.Id
						  LEFT JOIN ORG.Company AS COM ON COM.COAId=FAD.COAId
						  WHERE FAD.FixedAssetMasterId='" + assetMasterId + @"' AND COM.Id='" + identity.CompanyId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public IEnumerable<object> CheckMasterIsRegisterApplyByMasterId(string fxmasterId)
        {
            var _sql = @"select fod.IsRegisterApply,faob.IsPark from [MST].FixedAssetMaster as fm
                                left outer join [TRN].[MaterialMasterOpeningBalanceDetail] as fod on fm.Id = fod.FixedAssetMasterId
								left outer join [TRN].OpeningBalance as faob on fod.OpeningBalanceId=faob.Id
                               WHERE fod.FixedAssetMasterId = '" + fxmasterId + @"' ";
            return _sqlRepository.GetDataCollection(_sql, null);
        }

        #region Report

        public IWorkbook GetFixedAssetMaster()
        {
            var obj = new ReportGeneralVoucher();
            using (var excelEngine = new ExcelEngine())
            {
                var workbook = obj.FixedAssetMaster_Report(excelEngine);
                return workbook;
            }
        }

        #endregion Report

        private void AssetTypeChangeValidation(string id, string assetType)
        {
            try
            {
                var sql = @"SELECT A.Id, A.UserName,A.AssetType AI,F.AssetType FAM

                            FROM MST.FixedAssetMaster AS F
                            LEFT OUTER JOIN(SELECT Id,UserName,AssetType,FixedAssetMasterId FROM MST.AssetItem WHERE FixedAssetMasterId='" + id + @"') AS A ON F.Id=A.FixedAssetMasterId
                            WHERE F.Id='" + id + "'";
                var data = _sqlRepository.GetData(sql, null);
                if (!string.IsNullOrEmpty(data["Id"].ToString()) && data["FAM"].ToString() != assetType)
                    throw new CustomException("Can not change asset type [" + data["FAM"] + "]. This is using on asset item [" + data["UserName"] + "]");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetFixedAssetMasterData(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT    FAC.UserName FixedAssetCategory, FASC.UserName FixedAssetSubCategory
									, FAM.UserName FixedAssetMasterName, FAM.FixedAssetCategoryId
									,FAM.Id FixedAssetMasterId
                                    , FAM.FixedAssetSubCategoryId, FAM.AssetType
                                    FROM  [MST].[FixedAssetMaster] FAM 
                                    LEFT JOIN HKP.FixedAssetCategory FAC ON FAM.FixedAssetCategoryId=FAC.Id
                                    LEFT JOIN HKP.FixedAssetSubCategory FASC ON FAM.FixedAssetSubCategoryId=FASC.Id";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetFixedAssetMasterPoPUpData()
        { 
            string str = @"SELECT FAM.*,
                        FAC.UserName 'FixedAssetCategory',
                        FASC.UserName 'FixedAssetSubCategory'
                        FROM  MST.[FixedAssetMaster]  FAM
                        LEFT OUTER JOIN  HKP.[FixedAssetCategory]  FAC ON FAM.FixedAssetCategoryId=FAC.Id
                        LEFT OUTER JOIN  HKP.[FixedAssetSubCategory]  FASC ON FAM.FixedAssetSubCategoryId=FASC.Id";  
            return _sqlRepository.GetDataCollection(str,null);
        }
        public GridModel GetFAMISearch(GridParameter parameters)
        {
            try
            { 
                parameters.CmdText = @"SELECT fami.Id,fam.Id FixedAssetMasterId,fam.UserName FixedAssetMaster,fami.Code,fami.ShortName,fami.StandardName,fami.UserName
									                ,uom.Id CapacityUoMId,uom.UserName CapacityUoM,fami.CapacityValue,isnull(fami.Description,'') Description
									                ,isnull(fami.Remarks,'') Remarks

                                                    FROM mst.FixedAssetItem AS fami
                                                    LEFT JOIN mst.FixedAssetMaster AS fam ON fam.Id=fami.FixedAssetMasterId
                                                    LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=fami.CapacityUoMId";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public string GetFixedAssetMasterReport(string ReportHeader, string reportFileName,string CompanyGroupId) 
        {
            var filePath = "";
            ExcelEngine excelEngine = null;
            excelEngine = new ExcelEngine(); 
            IApplication application = excelEngine.Excel; 
            application.DefaultVersion = ExcelVersion.Excel2013; 
            IWorkbook workbook = application.Workbooks.Create(1); 
            IWorksheet worksheet = workbook.Worksheets[0];

            var data = getFAMIDataList();
            try
            {
                worksheet.Name = "Fixed Asset Master";
                int COL = 1; int ROW = 6;

                int startCol = COL;
                worksheet[ROW, COL].Text = "Code";
                int colCode = COL;
                worksheet[ROW, COL].ColumnWidth = 12;
                COL++;

                worksheet[ROW, COL].Text = "Fixed Asset Master Name";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colFixedAssetMasterName = COL;
                worksheet[ROW, COL].ColumnWidth = 25;
                COL++;

                worksheet[ROW, COL].Text = "Fixed Asset Category";
                int colFixedAssetCategory = COL;
                worksheet[ROW, COL].ColumnWidth = 20;
                COL++;

                worksheet[ROW, COL].Text = "Fixed Asset SubCategory";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colFixedAssetSubCategory = COL;
                worksheet[ROW, COL].ColumnWidth = 22;
                COL++;

                worksheet[ROW, COL].Text = "AssetType";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colAssetType = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                //COL++;

                //worksheet[ROW, COL].Text = "GL";
                //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //int colGL = COL;
                //worksheet[ROW, COL].ColumnWidth = 15;
                //COL++;

                //worksheet[ROW, COL].Text = "Budget";
                //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //int colBudget = COL;
                //worksheet[ROW, COL].ColumnWidth = 15;

                int endCol = COL;
                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Size = 12;
                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Bold = true;

                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Yellow;
                worksheet.Range[ROW, startCol, ROW, COL].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, startCol, ROW, COL].BorderInside(ExcelLineStyle.Hair);
 
                ROW++;
                int startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    worksheet[ROW, colCode].Text = data.Rows[i]["Code"].ToString();
                    worksheet[ROW, colFixedAssetMasterName].Text = data.Rows[i]["UserName"].ToString();
                    worksheet[ROW, colFixedAssetCategory].Text = data.Rows[i]["FixedAssetCategory"].ToString();
                    worksheet[ROW, colFixedAssetSubCategory].Text = data.Rows[i]["FixedAssetSubCategory"].ToString();
                    worksheet[ROW, colAssetType].Text = data.Rows[i]["AssetType"].ToString(); 
                    //worksheet[ROW, colGL].Text = data.Rows[i]["GL"].ToString(); 
                    //worksheet[ROW, colBudget].Text = data.Rows[i]["Budget"].ToString(); 

                    worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                } 

                worksheet.UsedRange.WrapText = true;
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                worksheet["A" + startRow.ToString()].FreezePanes();
 
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.MainCompanyGroupHeader(ref worksheet, endCol, "Fixed Asset Master Report", CompanyGroupId);
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
                
                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.WrapText = true;
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;
                 
                //#endregion ******************Report Header******************
                worksheet.PageSetup.TopMargin = 0.2;
                worksheet.PageSetup.BottomMargin = 0.8; 
                worksheet.PageSetup.LeftMargin = 0.2;
                worksheet.PageSetup.RightMargin = 0.2;
                worksheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                worksheet.PageSetup.FitToPagesTall = 0;
                worksheet.PageSetup.FitToPagesWide = 1;
                worksheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                worksheet.PageSetup.CenterHorizontally = true;
                 
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public string GetFixedAssetMasterIndividualReport(string FAMId, string ReportHeader, string reportFileName, string CompanyGroupId)
        {
            var filePath = "";
            var reportUtility = new ReportUtility();
            ExcelEngine excelEngine = null;
            excelEngine = new ExcelEngine();
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2013;
            IWorkbook workbook = application.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];

            var HeaderData = getFAMIHeaderData(FAMId);
            var data = getFAMIlist(FAMId);

            var row = 5;
            reportUtility.SetMasterHeaderText(ref worksheet, row, 1, "Code");
            reportUtility.SetText(ref worksheet, row, 2, HeaderData["Code"].ToString());
            worksheet[row, 2, row, 3].Merge();

            reportUtility.SetMasterHeaderText(ref worksheet, row, 4, "Fixed Asset Master");
            reportUtility.SetText(ref worksheet, row, 5, HeaderData["UserName"].ToString());
            worksheet[row, 5, row, 6].Merge();
            row++;

            reportUtility.SetMasterHeaderText(ref worksheet, row, 1, "Category");
            reportUtility.SetText(ref worksheet, row, 2, HeaderData["FixedAssetCategory"].ToString());
            worksheet[row, 2, row, 3].Merge();

            reportUtility.SetMasterHeaderText(ref worksheet, row, 4, "Sub Category");
            reportUtility.SetText(ref worksheet, row, 5, HeaderData["FixedAssetSubCategory"].ToString());
            worksheet[row, 5, row, 6].Merge();
            row++;

            reportUtility.SetMasterHeaderText(ref worksheet, row, 1, "Asset Type");
            reportUtility.SetText(ref worksheet, row, 2, HeaderData["AssetType"].ToString());
            worksheet[row, 2, row, 3].Merge();
            worksheet[row, 2].ColumnWidth = 30;
            row++;

            worksheet[row, 4].ColumnWidth = 20;
            //worksheet[row, 5].ColumnWidth = 20;
            //worksheet[row, 6, row, 7].Merge();

            //worksheet[row, 6].ColumnWidth = 20;
            //worksheet[row, 7].ColumnWidth = 20; 
            row++;

            try
            {
                worksheet.Name = "Fixed Asset Master";
                int COL = 1; int ROW = row;

                int startCol = COL;
                worksheet[ROW, COL].Text = "Code";
                int colCode = COL;
                worksheet[ROW, COL].ColumnWidth = 12;
                COL++;
 
                worksheet[ROW, COL].Text = "Fixed Asset Master Item";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colUserName = COL;
                worksheet[ROW, COL].ColumnWidth = 22;
                COL++;

                worksheet[ROW, COL].Text = "Fixed Asset Master";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colFixedAssetMaster = COL;
                worksheet[ROW, COL].ColumnWidth = 17;
                COL++;

                worksheet[ROW, COL].Text = "Capacity Value";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colCapacityValue = COL;
                //worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Description";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colDescription = COL;
                worksheet[ROW, COL].ColumnWidth = 20;
                 
                int endCol = COL;
                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Size = 12;
                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Bold = true;

                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Yellow;
                worksheet.Range[ROW, startCol, ROW, COL].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, startCol, ROW, COL].BorderInside(ExcelLineStyle.Hair);

                ROW++;
                int startRow = ROW;

               
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    worksheet[ROW, colCode].Text = data.Rows[i]["Code"].ToString(); 
                    worksheet[ROW, colUserName].Text = data.Rows[i]["UserName"].ToString();
                    worksheet[ROW, colFixedAssetMaster].Text = data.Rows[i]["FixedAssetMaster"].ToString();
                    worksheet[ROW, colCapacityValue].Text = data.Rows[i]["CapacityValue"].ToString()+" " + data.Rows[i]["CapacityUoM"].ToString();
                    worksheet[ROW, colDescription].Text = data.Rows[i]["Description"].ToString(); 

                    worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                worksheet.UsedRange.WrapText = true;
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                worksheet["A" + startRow.ToString()].FreezePanes();

                //ReportUtility reportUtility = new ReportUtility();
                reportUtility.MainCompanyGroupHeader(ref worksheet, endCol, "Fixed Asset Master Report", CompanyGroupId);
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.WrapText = true;
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                worksheet.PageSetup.TopMargin = 0.2;
                worksheet.PageSetup.BottomMargin = 0.8;
                worksheet.PageSetup.LeftMargin = 0.2;
                worksheet.PageSetup.RightMargin = 0.2;
                worksheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                worksheet.PageSetup.FitToPagesTall = 0;
                worksheet.PageSetup.FitToPagesWide = 1;
                worksheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                worksheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public string GetFixedAssetMasterItemReport(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName, string PlantId)
        {
            var filePath = "";
            ExcelEngine excelEngine = null;
            excelEngine = new ExcelEngine();
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2013;
            IWorkbook workbook = application.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];

            try
            {
                worksheet.Name = "Fixed Asset Master Item";
                int COL = 1; int ROW = 6;

                int startCol = COL;
                worksheet[ROW, COL].Text = "Code";
                int colCode = COL;
                worksheet[ROW, COL].ColumnWidth = 12;
                COL++;

                worksheet[ROW, COL].Text = "Fixed Asset Master Item";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colUserName = COL;
                worksheet[ROW, COL].ColumnWidth = 22;
                COL++;

                worksheet[ROW, COL].Text = "Fixed Asset Master";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colFixedAssetMaster = COL;
                worksheet[ROW, COL].ColumnWidth = 17;
                COL++;

                worksheet[ROW, COL].Text = "Capacity Value";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colCapacityValue = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Description";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colDescription = COL;
                worksheet[ROW, COL].ColumnWidth = 20;

                int endCol = COL;
                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Size = 12;
                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Bold = true;

                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Yellow;
                worksheet.Range[ROW, startCol, ROW, COL].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, startCol, ROW, COL].BorderInside(ExcelLineStyle.Hair);

                ROW++;
                int startRow = ROW;

                for (int i = 0; i < data.Count; i++)
                {
                    worksheet[ROW, colCode].Text = data[i]["Code"].ToString();
                    worksheet[ROW, colUserName].Text = data[i]["UserName"].ToString();
                    worksheet[ROW, colFixedAssetMaster].Text = data[i]["FixedAssetMaster"].ToString();
                    worksheet[ROW, colCapacityValue].Text = data[i]["CapacityValue"].ToString() + " " + data[i]["CapacityUoM"].ToString();
                    worksheet[ROW, colDescription].Text = data[i]["Description"].ToString();

                    worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                worksheet.UsedRange.WrapText = true;
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                worksheet["A" + startRow.ToString()].FreezePanes();

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.MainCompanyGroupHeader(ref worksheet, endCol, "Fixed Asset Master Item Report", PlantId);
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.WrapText = true;
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                worksheet.PageSetup.TopMargin = 0.2;
                worksheet.PageSetup.BottomMargin = 0.8;
                worksheet.PageSetup.LeftMargin = 0.2;
                worksheet.PageSetup.RightMargin = 0.2;
                worksheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                worksheet.PageSetup.FitToPagesTall = 0;
                worksheet.PageSetup.FitToPagesWide = 1;
                worksheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                worksheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public DataTable getFAMIDataList()
        {
            string str = @"SELECT FAM.*,FAC.UserName FixedAssetCategory,FASC.UserName FixedAssetSubCategory--,GL.UserName GL,B.UserName Budget   
					                                FROM mst.FixedAssetMaster FAM
					                                left join [HKP].[FixedAssetCategory] FAC on FAC.Id=FAM.FixedAssetCategoryId
					                                left join [HKP].[FixedAssetSubCategory] FASC on FASC.Id=FAM.FixedAssetSubCategoryId
					                                --left join [HKP].[FixedAssetMasterBudgetTag] FMBT on FMBT.FixedAssetMasterId=FAM.Id
					                                --left join [MST].[BudgetMaster] BM on BM.Id=FMBT.BudgetMasterId
					                                --left join [HKP].[GLGeneralInfo] GL on GL.Id=BM.GLGeneralInfoId
					                                --left join [HKP].[Budget] B on B.Id=BM.BudgetId";
            return _sqlRepository.GetDataTable(str);
        }

        public Dictionary<string, object> getFAMIHeaderData(string FAMId)
        {
            string str = @"SELECT *,FAC.UserName FixedAssetCategory,FASC.UserName FixedAssetSubCategory   
					                    FROM mst.FixedAssetMaster FAM
					                    left join [HKP].[FixedAssetCategory] FAC on FAC.Id=FAM.FixedAssetCategoryId
					                    left join [HKP].[FixedAssetSubCategory] FASC on FASC.Id=FAM.FixedAssetSubCategoryId
                                        where FAM.Id ='" + FAMId + "'";
            return _sqlRepository.GetData(str); 
        }
        public DataTable getFAMIlist(string FAMId)
        {
            string str = @"SELECT fam.Id FixedAssetMasterId,fami.Id,fam.UserName FixedAssetMaster,fami.Code,fami.ShortName,fami.StandardName,fami.UserName
									                ,uom.UserName CapacityUoM,fami.CapacityValue,isnull(fami.Description,'') Description
									                ,isnull(fami.Remarks,'') Remarks

                                                    FROM mst.FixedAssetMaster AS fam 
													left join mst.FixedAssetItem AS fami ON fam.Id=fami.FixedAssetMasterId
                                                    LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=fami.CapacityUoMId
													where fam.Id ='" + FAMId + "'";
            return _sqlRepository.GetDataTable(str);
        }

    }
}