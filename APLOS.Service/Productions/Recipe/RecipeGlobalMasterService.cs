#region Using
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Productions.Recipe;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
#endregion Using

namespace Library.Service.Productions.Recipe
{
    public class RecipeGlobalMasterService : Service<RecipeGlobalMaster>, IRecipeGlobalMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;//
        private readonly IRecipeGlobalSubprocessService _recipesubprocessservice;//IRecipeSubprocessService
        public readonly IRecipeGlobalOperationService _recipeGlobaloperationservice;//IRecipeSubprocessService
        private readonly IRecipeGlobalUtilityService _recipeGlobalutilityservice;//IRecipeSubprocessService
        private readonly IRecipeGlobalRawMaterialService _reciperawmaterialservice;//IRecipeSubprocessService

        public RecipeGlobalMasterService(
            IRepositoryAsync<RecipeGlobalMaster> RecipeMasterRepository,
            IPKGeneratorService pkGeneratorService,
            IRecipeGlobalSubprocessService recipesubprocessservice,
            IRecipeGlobalOperationService recipeGlobaloperationservice,
            IRecipeGlobalUtilityService recipeGlobalutilityservice,
            IRecipeGlobalRawMaterialService reciperawmaterialservice,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(RecipeMasterRepository, unitOfWork, pkGeneratorService)
        {
            _reciperawmaterialservice = reciperawmaterialservice;
            _recipesubprocessservice = recipesubprocessservice;
            _unitOfWork = unitOfWork;
            _recipeGlobalutilityservice = recipeGlobalutilityservice;
            _recipeGlobaloperationservice = recipeGlobaloperationservice;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IWorkbook GetRecipeReport(out string reportFileName, string mmId, string companyGroupId, string companyId, string plantId)
        {
            ExcelEngine excelEngine = null;
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();

                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet(ref sheet1, oRU, "Recipe Information ", "Recipe Information", mmId, companyGroupId, companyId, plantId);
                reportFileName= DateTime.Now.ToString("yyMMdd") + "-" + "Recipe Report";
                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private void CreateSheet(ref IWorksheet sheet1, ReportUtility oRU, string SheetHeader, string SheetName, string mmId, string companyGroupId, string companyId, string plantId)
        {
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            try
            {
                var dtRecipe = GetRecipeInfo(mmId);

                xlsRow = 4;

                #region ------------------Column Header------------------
                xlsCol = 1;
                xlsRow += 1;

                sheet1.Range[5, xlsCol].RowHeight = 20;
                xlsCol = 1;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Process", 20);
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol + 2, "Process Criteria", 20);
                sheet1.Range[xlsRow, xlsCol + 1].ColumnWidth = 20;
                oRU.SetCellText(sheet1, xlsRow, xlsCol + 1, dtRecipe.Rows[0]["Process"].ToString());
                sheet1.Range[xlsRow, xlsCol + 3].ColumnWidth = 20;
                oRU.SetCellText(sheet1, xlsRow, xlsCol + 3, dtRecipe.Rows[0]["ProcessCriteria"].ToString());


                xlsRow += 1;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Code");
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol + 2, "Name");
                oRU.SetCellText(sheet1, xlsRow, xlsCol + 1, dtRecipe.Rows[0]["Code"].ToString());
                oRU.SetCellText(sheet1, xlsRow, xlsCol + 3, dtRecipe.Rows[0]["UserName"].ToString());

                xlsRow += 1;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, dtRecipe.Rows[0]["Specification"].ToString());
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol + 2, dtRecipe.Rows[0]["Specification2"].ToString());
                //oRU.SetCellText(sheet1, xlsRow, xlsCol + 1, dtRecipe.Rows[0]["Thickness"].ToString());
                oRU.SetCellText(sheet1, xlsRow, xlsCol + 1, dtRecipe.Rows[0]["SpecificationValue"].ToString());
                
                oRU.SetCellText(sheet1, xlsRow, xlsCol + 3, dtRecipe.Rows[0]["SpecificationValue2"].ToString());


                xlsRow += 1;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Weight");
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol + 2, dtRecipe.Rows[0]["SpecificationZero"].ToString());
                var aw = dtRecipe.Rows[0]["MaterialAvgWeight"].ToString();
                oRU.SetCellText(sheet1, xlsRow, xlsCol + 1, aw + " " + dtRecipe.Rows[0]["SizeUom"].ToString());
                oRU.SetCellText(sheet1, xlsRow, xlsCol + 3, dtRecipe.Rows[0]["SpecificationZeroValue"].ToString());
                //xlsRow += 1;
                List<string> subprocess = new List<string>();
                int count = 0;
                for (int i = 0; i < dtRecipe.Rows.Count; i++)
                {

                    if (subprocess.Contains(dtRecipe.Rows[i]["SubProcessId"].ToString()))
                    {
                        xlsRow += 1;
                        oRU.SetText(ref sheet1, xlsRow, xlsCol, dtRecipe.Rows[i]["MaterialMaster"].ToString(),false, ExcelLineStyle.Hair);
                        oRU.SetText(ref sheet1, xlsRow, xlsCol + 1, dtRecipe.Rows[i]["Article"].ToString(), false, ExcelLineStyle.Hair);
                        oRU.SetText(ref sheet1, xlsRow, xlsCol + 2, dtRecipe.Rows[i]["RawValue"].ToString(), false, ExcelLineStyle.Hair);
                        oRU.SetText(ref sheet1, xlsRow, xlsCol + 3, dtRecipe.Rows[i]["RawUom"].ToString(), false, ExcelLineStyle.Hair);
                    }
                    else
                    {
                        count++;
                        xlsRow += 2;
                        subprocess.Add(dtRecipe.Rows[i]["SubProcessId"].ToString());
                        oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, dtRecipe.Rows[i]["Sequence"].ToString(), ExcelHAlign.HAlignLeft);
                        oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol + 1, dtRecipe.Rows[i]["Description"].ToString());
                        oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol + 2, dtRecipe.Rows[i]["LineItemValue"].ToString());
                        oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol + 3, dtRecipe.Rows[i]["SizeUom"].ToString());
                        xlsRow += 1;
                        oRU.SetText(ref sheet1, xlsRow, xlsCol, dtRecipe.Rows[i]["MaterialMaster"].ToString(), false, ExcelLineStyle.Hair);
                        oRU.SetText(ref sheet1, xlsRow, xlsCol + 1, dtRecipe.Rows[i]["Article"].ToString(), false, ExcelLineStyle.Hair);
                        oRU.SetText(ref sheet1, xlsRow, xlsCol + 2, dtRecipe.Rows[i]["RawValue"].ToString(), false, ExcelLineStyle.Hair);
                        oRU.SetText(ref sheet1, xlsRow, xlsCol + 3, dtRecipe.Rows[i]["RawUom"].ToString(), false, ExcelLineStyle.Hair);
                    }
                    //xlsRow += 1;
                    xlsCol = 1;

                }; //loop
                xlsRow += 2;
                var row = xlsRow + 1;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remark");
                oRU.SetCellText(sheet1, xlsRow, xlsCol+1, dtRecipe.Rows[0]["Remark"].ToString());
                sheet1.Range["B" + xlsRow + "" + ":D" + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet1.Range["B"+xlsRow+"" + ":D" + row].Merge();
                
                endXlsCol = 4;
                #endregion ------------------Column Header-----------------


                xlsCol = 2;
                xlsRow += 5;
                sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet1.UsedRange.WrapText = true;
                sheet1.Name = SheetName;

                sheet1.Range[xlsRow, xlsCol].NumberFormat = oRU.NumberFormatDecimalTwo();

                sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].Merge();
                sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                oRU.CompanyGroupHeader(ref sheet1, endXlsCol, "Recipe Information", companyGroupId);
                oRU.PageSetup(ref sheet1, 4, ExcelPageOrientation.Portrait);
                sheet1.PageSetup.CenterHorizontally = true;

                #region UsedRange Alignment
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable GetRecipeInfo(string mmId)
        {
            try
            {
                var sql = @"SELECT m.Id
                       	,m.Code
                       	,m.UserName 
                       	,p.UserName Process
                       	,pc.UserName ProcessCriteria
                        ,CONVERT(NUMERIC(10,2), m.MaterialAvgWeight) MaterialAvgWeight
                       	,u.UserName SizeUom
                       	,m.[BatchSize]
                       	,m.[Description] Remark
						--attr-----------
                       	--,ma.UserName SpecificationZero
                       	--,mav.UserName SpecificationZeroValue                       	
                       -- ,mas.UserName Specification
                      --  ,masv.UserName SpecificationValue
                       -- ,mas1.UserName Specification2
                       -- ,masv1.UserName SpecificationValue2
						--char-------
						,mac1.UserName char1
                       	,macv1.UserName char1Value                       	
                        ,mac2.UserName char2
                       	,macv2.UserName char2Value
                        ,mac3.UserName char3
                       	,macv3.UserName char3Value
						--------------
						,SpecificationZero=case when ma.UserName is null then mac1.UserName else  ma.UserName end 
						,SpecificationZeroValue=case when mav.UserName is null then macv1.UserName else mav.UserName end

						,Specification=case when mas.UserName is null then mac2.UserName else  mas.UserName end 
						,SpecificationValue=case when masv.UserName is null then macv2.UserName else masv.UserName end

						,Specification2=case when mas1.UserName is null then mac3.UserName else  mas1.UserName end 
						,SpecificationValue2=case when masv1.UserName is null then macv3.UserName else masv1.UserName end

                       	,s.[Description] 
                       	-- ,s.[Sequence]
                       , cast (ROUND( s.[Sequence],0) as int) Sequence
                       	,s.LineItemValue
                       	,s.Id SubProcessId
                       	--raw material
                       	,c.MaterialMaster
                       	,c.StandardName Article
                       	,c.QtyValue RawValue
                       	,c.Uom RawUom
                       	,c.sort
                       FROM TRN.RecipeGlobalMaster m
                       LEFT JOIN trn.RecipeGlobalSubprocess s ON m.id = s.RecipeGlobalMasterID
                       LEFT JOIN (
                       SELECT 1 sort,mm.UserName MaterialMaster,mma.StandardName,ur.UserName UoM,r.QtyValue,RecipeGlobalMasterId,RecipeGlobalSubprocessId,r.MaterialMasterId,r.ArticleId FROM trn.RecipeGlobalRawMaterial r
                       LEFT JOIN mst.MaterialMaster mm ON mm.id = r.MaterialMasterId
                       LEFT JOIN [MST].[MaterialMasterArticle] mma ON mma.id = r.ArticleId
                       LEFT JOIN scs.UnitOfMeasurement ur ON ur.id = r.UomId
                       UNION
                       Select 2 sort, b.UserName MaterialMaster,''StandardName,uom.UserName UoM,A.[Value] QtyValue,RecipeGlobalMasterId,RecipeGlobalSubprocessId,'' MaterialMasterId,'' ArticleId from  [TRN].[RecipeGlobalMaterialGroup] A
                       LEFT JOIN [MST].[RecipeMaterialGroupingMaster] B ON A.RecipeMaterialGroupingMasterId=B.Id
                       LEFT JOIN SCS.UnitOfMeasurement uom ON uom.Id = A.UomId
                       )
                       c ON c.RecipeGlobalMasterId = m.id AND c.RecipeGlobalSubprocessId = s.id
                       LEFT JOIN hkp.Process p ON p.id = m.ProcessId
                       LEFT JOIN hkp.ProcessCriteria pc ON pc.id = m.ProcessCriteriaId
                       LEFT JOIN scs.UnitOfMeasurement u ON u.id = m.AvgUom
					   --************
                       LEFT JOIN hkp.MaterialAttribute ma ON ma.id = m.MaterialAttributeId
                       LEFT JOIN hkp.MaterialAttributeValue mav ON mav.id = m.AttributeValueId
                       LEFT JOIN hkp.MaterialAttribute mas ON mas.id = m.Specification1Id
                       LEFT JOIN hkp.MaterialAttributeValue masv ON masv.id = m.Specification1ValueId
                       LEFT JOIN hkp.MaterialAttribute mas1 ON mas1.id = m.Specification2Id
                       LEFT JOIN hkp.MaterialAttributeValue masv1 ON masv1.id = m.Specification2ValueId
					   --**********
					    LEFT JOIN hkp.Characteristics mac1 ON mac1.id = m.Characteristics1Id
						LEFT JOIN hkp.CharacteristicsValue macv1 ON macv1.id = m.Characteristics1ValueId
						LEFT JOIN hkp.Characteristics mac2 ON mac2.id = m.Characteristics2Id
						LEFT JOIN hkp.CharacteristicsValue macv2 ON macv2.id = m.Characteristics2ValueId
						LEFT JOIN hkp.Characteristics mac3 ON mac3.id = m.Characteristics3Id
						LEFT JOIN hkp.CharacteristicsValue macv3 ON macv3.id = m.Characteristics3ValueId
                       --subprocess-------------
                       WHERE m.id = '"+mmId+@"'
                       ORDER BY S.Sequence,S.[Description],c.sort,c.MaterialMasterId,c.ArticleId";
                var list = _sqlRepository.GetDataTable(sql);

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetRecipeRawMaterialList(string masterId)
        {
            try
            {
                var sql = @"SELECT BM.Id, BM.RecipeGlobalMasterId, BM.RecipeGlobalSubprocessId, BM.MaterialMasterId, 
                        MM.UserName AS MaterialMasterName, BM.ArticleId, ART.StandardName AS ArticleName, BM.QtyValue as 'RmValue',UOM.UserName
						FROM [TRN].[RecipeGlobalRawMaterial] AS BM
						LEFT JOIN [MST].[MaterialMaster] AS MM ON BM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON BM.ArticleId=ART.Id
						LEFT JOIN SCS.UnitOfMeasurement AS UOM ON UOM.Id=BM.UomId	
						WHERE BM.RecipeGlobalSubprocessId='" + masterId + @"'  ORDER BY MM.UserName ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
                //throw new CustomException(ex.Message, ex,
                //    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                //    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel GetEntityProductionProcessCbo(bool cadmin, bool sadmin, string userId, string entityId)
        {
            if (cadmin || sadmin)
            {
                string _sql = @"SELECT DISTINCT P.Id AS [Value], P.UserName AS [Text] FROM HKP.EntityProcessTag AS EP
                            JOIN HKP.Process AS P ON EP.ProcessId=P.Id WHERE EP.EntityId='" + entityId + "'  AND P.IsProductionProcess=1";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = _sql });
            }
            else
            {
                string _sql = @"SELECT P.Id AS [Value], P.UserName AS [Text] FROM HKP.EntityProcessTag EPT
						        INNER JOIN HKP.Process AS P ON P.Id=EPT.ProcessId
						        INNER JOIN [SEC].[UserProcess] UP ON UP.ProcessId=P.Id
						        WHERE EPT.EntityId='" + entityId + @"' AND UP.UserId='" + userId + "' AND P.IsProductionProcess=1";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = _sql });
            }
        }


        public IEnumerable<ComboModel> GetMaterialAttributeCbo()
        {
            var sql = @"select Id, StandardName from HKP.MaterialAttribute";
            return _sqlRepository.GetCombo(sql, "Id", "StandardName");
        }

        public IEnumerable<ComboModel> GetCharacteristicsCbo()
        {
            var sql = @"select Id, StandardName from HKP.Characteristics";
            return _sqlRepository.GetCombo(sql, "Id", "StandardName");
        }

        public IEnumerable<ComboModel> GetMaterialMasterCbo()
        {
            var sql = @"select mm.Id, mm.StandardName from MST.MaterialMaster as MM
                        --LEFT JOIN [HKP].[MaterialType] AS MT ON MT.Id= MM.MaterialTypeId 
                        --LEFT JOIN [HKP].[MaterialTypeNature] AS MTN ON MTN.MaterialTypeId= MT.Id where MTN.Nature='RawMaterial'
                         ";
            return _sqlRepository.GetCombo(sql, "Id", "StandardName");
        }

        public IEnumerable<ComboModel> GetRecipeOperationCbo(string processId)
        {
            var sql = @"select Id, UserName as 'StandardName' from [HKP].[RecipeOperation] ";
            return _sqlRepository.GetCombo(sql, "Id", "StandardName");
        }

        public IEnumerable<ComboModel> GetUnitOfMeasurementCbo()
        {
            var sql = @"select Id, UserName from SCS.UnitOfMeasurement order by  Username";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public IEnumerable<ComboModel> GetMeasurementCbo(string materialMasterId)
        {
            var sql = @"SELECT U.Id, UserName FROM SCS.UnitOfMeasurement M
                      INNER JOIN 
                      (SELECT BaseUOMId AS Id FROM [MST].[MaterialMaster] WHERE Id='" + materialMasterId + @"'
                      UNION
                      SELECT AlternativeUOMId AS Id FROM [MST].[MaterialMasterAlternativeUOM] WHERE MaterialMasterId='" + materialMasterId + @"'
                      ) U 
                      ON U.Id=M.Id";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public IEnumerable<ComboModel> GetRecipeMaterialGroupingMasterMeasurementCbo(string recipeMaterialGroupingMasterId)
        {
            var sql = @"Select U.Id,U.UserName FROM [MST].[RecipeMaterialGroupingMaster] RMGM
                        LEFT JOIN [SCS].[UnitOfMeasurement] U ON U.Id=RMGM.UomId
                        Where RMGM.Id='"+ recipeMaterialGroupingMasterId + "'";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public GridModel RecipeGlobalMasterList(GridParameter parameters, string entityId, string processId)
        {
            try
            {
                parameters.CmdText = @"SELECT BRM.Id, BRM.CompanyGroupId, BRM.CompanyId, BRM.EntityId, BRM.ProcessId, PR.UserName AS ProcessName,
                                     BRM.Code, BRM.UserName as 'Name',BRM.Description FROM [TRN].[RecipeGlobalMaster] AS BRM
						             LEFT JOIN [HKP].[Process] AS PR ON BRM.ProcessId=PR.Id
						             WHERE BRM.EntityId='" + entityId + @"' AND BRM.ProcessId='" + processId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw ex;
                //throw new CustomException(ex.Message, ex,
                //    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                //    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> RecipeDetailsUsedListList(string recipemasterId)
        {
            try
            {
                var sql = @"SELECT RM.Id, MM.StandardName , MT.[Description] AS MaterialTypeName, MGP.UserName AS MaterialGroupMasterName
					,PM.UserName AS ProductMasterName, MM.Code, MM.UserName AS MaterialMasterName, RM.ArticleId, MMA.StandardName as 'ArticleName', MMA.Code as 'ArticleCode'
					 FROM TRN.RecipeGlobalMaster AS RG
                           JOIN TRN.RecipeMaterial as RM on RG.Id= RM.RecipeGlobalMasterId 
                           JOIN MST.MaterialMaster as MM on MM.Id= RM.MaterialMasterId 
						   LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
						   LEFT JOIN[HKP].[MaterialType] AS MT ON MGP.MaterialTypeId = MT.Id
						   LEFT JOIN [TRN].[ProductDefinition] AS PD ON PD.MaterialMasterId= MM.Id
						   LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
						   LEFT JOIN [MST].[MaterialMasterArticle] AS MMA ON MMA.Id = RM.ArticleId
                           WHERE RG.Id= '" + recipemasterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<ComboModel> GetRecipeCbo(string entityId)
        {
            var sql = @"select Id, UserName from TRN.RecipeGlobalMaster where EntityId=" + entityId + " ORDER BY UserName";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        private string GetPK()
        {
            return "RWM" + _pkGeneratorService.GetAutoNumber(nameof(RecipeGlobalMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<object> GetList(string masterid, string companyGroupId, string companyId)
        {
            try
            {
                
                var sql = @"
                                SELECT m.Id,  p.Id as 'ProcessId',m.EntityId
	                                ,m.[Description]
	                                ,m.Code
	                                ,m.UserName
	                                ,m.[BatchSize]
	                                ,uombs.UserName BatchUom
	                                ,m.MaterialAvgWeight
	                                ,uomavg.UserName AvgUomWeight
	                                ,m.AvgUom
	                                ,m.Uom
	                                ,m.StartPressure
	                                ,m.EndPressure
	                                ,m.StartTemperature
	                                ,m.EndTemperature
	                                ,m.GradientPressure
	                                ,m.GradientTemperature
	                                ,mm.Code MaterialMasterCode
	                                ,mm.[UserName] MaterialMasterDescription
	                                ,mm.Id MaterialMasterId,'' GridNO
	                                --,mg.GridNO
	                                ,mt.[Description] MaterialType
	                                ,mgr.UserName MaterialGroup
	                                ,mm.UserName
	                                ,uom.UserName BaseUOM
	                                ,m.Characteristics1Id
	                                ,m.Characteristics2Id
	                                ,m.Characteristics3Id
	                                ,m.Characteristics1ValueId
	                                ,m.Characteristics2ValueId
	                                ,m.Characteristics3ValueId
	                                ,p.UserName Process
	                                ,m.ProcessId,m.ProcessCriteriaId,pct.UserName ProcessCriteria
	                                ,c1.StandardName Characteristics1
	                                ,c2.StandardName Characteristics2
	                                ,c3.StandardName Characteristics3
	                                ,'xyz' SelectedCharacteristics
	                                ,Characteristics1Selected = CASE isnull(m.Characteristics1Id, '')
		                                WHEN ''
			                                THEN 0
		                                ELSE 1
		                                END
	                                ,Characteristics2Selected = CASE isnull(m.Characteristics2Id, '')
		                                WHEN ''
			                                THEN 0
		                                ELSE 1
		                                END
	                                ,Characteristics3Selected = CASE isnull(m.Characteristics3Id, '')
		                                WHEN ''
			                                THEN 0
		                                ELSE 1
		                                END
	                                ,cv1.[Description] Characteristics1Value
	                                ,cv2.[Description] Characteristics2Value
	                                ,cv3.[Description] Characteristics3Value
                                    ,rc.RecipeLevel
									,rc.RecipeDependAttributeId MaterialAttributeId
									,rc.RecipeDependCharacteristicsId
									,mat.UserName DependantAttribute
                                    ,m.AttributeValueId
                                    ,matv.UserName AttributeValueName
                                    --,m.Thickness,m.Weight
                                    ,m.Specification1Id
									,m.Specification2Id
									,m.Specification1ValueId
									,m.Specification2ValueId
									,sp1.UserName Specification1
									,sp2.UserName Specification2
									,spv1.UserName Specification1ValueName
									,spv2.UserName Specification2ValueName
                                    ,cv1.UserName Characteristics1ValueName
									,cv2.UserName Characteristics2ValueName
									,cv3.UserName Characteristics3ValueName
                                FROM trn.RecipeGlobalMaster m
                                LEFT JOIN mst.MaterialMaster mm ON mm.Id = m.MaterialMasterId
                               -- LEFT JOIN hkp.MaterialGrid mg ON mg.Id = mm.MaterialGridId
                                LEFT JOIN MST.MaterialGroupMaster mgr ON mgr.Id = mm.MaterialGroupMasterId
                                LEFT JOIN hkp.MaterialType mt ON mt.Id = mgr.materialTypeId
                                LEFT JOIN SCS.UnitOfMeasurement uom ON uom.Id = mm.BaseUOMId
                                LEFT JOIN SCS.UnitOfMeasurement uombs ON uombs.Id = m.Uom
                                LEFT JOIN SCS.UnitOfMeasurement uomavg ON uomavg.Id = m.AvgUom
                                LEFT JOIN hkp.Process p ON p.Id = m.ProcessId
                                LEFT JOIN hkp.ProcessCriteria pct ON pct.Id = m.ProcessCriteriaId
                                LEFT JOIN hkp.Characteristics c1 ON c1.Id = m.Characteristics1Id
                                LEFT JOIN hkp.Characteristics c2 ON c2.Id = m.Characteristics2Id
                                LEFT JOIN hkp.Characteristics c3 ON c3.Id = m.Characteristics3Id
                                LEFT JOIN hkp.CharacteristicsValue cv1 ON cv1.Id = m.Characteristics1ValueId
                                LEFT JOIN hkp.CharacteristicsValue cv2 ON cv2.Id = m.Characteristics2ValueId
                                LEFT JOIN hkp.CharacteristicsValue cv3 ON cv3.Id = m.Characteristics3ValueId
                                left join [SCS].[RecipeConfig] rc on rc.ProcessId=m.ProcessId and rc.CompanyId=m.CompanyId
								left join hkp.MaterialAttribute mat on mat.id=m.MaterialAttributeId
                                left join hkp.MaterialAttributeValue matv on matv.id=m.AttributeValueId

								left join hkp.MaterialAttribute sp1 on sp1.id=m.Specification1Id								
								left join hkp.MaterialAttribute sp2 on sp2.id=m.Specification2Id

								left join hkp.MaterialAttributeValue spv1 on spv1.id=m.Specification1ValueId								
								left join hkp.MaterialAttributeValue spv2 on spv2.id=m.Specification2ValueId

                                WHERE   m.Companygroupid='" + companyGroupId + @"' and m.CompanyId='" + companyId + @"'
                                        and m.Id='" + masterid + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<object> GetList()
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                sql = @"SELECT m.Id
	                                ,m.[Description]
	                                ,m.Code
	                                ,m.UserName
	                                ,m.[BatchSize]
	                                ,uombs.UserName UomBatchSize
	                                ,m.MaterialAvgWeight
	                                ,uomavg.UserName UomAvgWeight
	                                ,m.AvgUom
	                                ,m.Uom
	                                ,m.StartPressure
	                                ,m.EndPressure
	                                ,m.StartTemperature
	                                ,m.EndTemperature
	                                ,m.GradientPressure
	                                ,m.GradientTemperature
	                                ,mm.Code MaterialMasterCode
	                                ,mm.[UserName] MaterialMasterDescription
	                                ,mm.Id MaterialMasterId
	                                ,mg.GridNO
	                                ,mt.[Description] MaterialType
	                                ,mgr.UserName MaterialGroup
	                                ,mm.UserName
	                                ,uom.UserName BaseUOM
	                                ,m.Characteristics1Id
	                                ,m.Characteristics2Id
	                                ,m.Characteristics3Id
	                                ,m.Characteristics1ValueId
	                                ,m.Characteristics2ValueId
	                                ,m.Characteristics3ValueId
	                                ,p.UserName Process
	                                ,m.ProcessId,m.ProcessCriteriaId,pct.UserName ProcessCriteria
	                                ,c1.StandardName Characteristics1
	                                ,c2.StandardName Characteristics2
	                                ,c3.StandardName Characteristics3
	                                ,'xyz' SelectedCharacteristics
	                                ,Characteristics1Selected = CASE isnull(m.Characteristics1Id, '')
		                                WHEN ''
			                                THEN 0
		                                ELSE 1
		                                END
	                                ,Characteristics2Selected = CASE isnull(m.Characteristics2Id, '')
		                                WHEN ''
			                                THEN 0
		                                ELSE 1
		                                END
	                                ,Characteristics3Selected = CASE isnull(m.Characteristics3Id, '')
		                                WHEN ''
			                                THEN 0
		                                ELSE 1
		                                END
	                                ,cv1.[Description] Characteristics1Value
	                                ,cv2.[Description] Characteristics2Value
	                                ,cv3.[Description] Characteristics3Value
                                FROM trn.RecipeGlobalMaster m
                                LEFT JOIN mst.MaterialMaster mm ON mm.Id = m.MaterialMasterId
                                LEFT JOIN hkp.MaterialGrid mg ON mg.Id = mm.MaterialGridId
                                LEFT JOIN hkp.MaterialType mt ON mt.Id = mm.materialTypeId
                                LEFT JOIN MST.MaterialGroupMaster mgr ON mgr.Id = mm.MaterialGroupMasterId
                                LEFT JOIN SCS.UnitOfMeasurement uom ON uom.Id = mm.BaseUOMId
                                LEFT JOIN SCS.UnitOfMeasurement uombs ON uombs.Id = m.Uom
                                LEFT JOIN SCS.UnitOfMeasurement uomavg ON uomavg.Id = m.AvgUom
                                LEFT JOIN hkp.Process p ON p.Id = m.ProcessId
                                LEFT JOIN hkp.ProcessCriteria pct ON pct.Id = m.ProcessCriteriaId
                                LEFT JOIN hkp.Characteristics c1 ON c1.Id = m.Characteristics1Id
                                LEFT JOIN hkp.Characteristics c2 ON c2.Id = m.Characteristics2Id
                                LEFT JOIN hkp.Characteristics c3 ON c3.Id = m.Characteristics3Id
                                LEFT JOIN hkp.CharacteristicsValue cv1 ON cv1.Id = m.Characteristics1ValueId
                                LEFT JOIN hkp.CharacteristicsValue cv2 ON cv2.Id = m.Characteristics2ValueId
                                LEFT JOIN hkp.CharacteristicsValue cv3 ON cv3.Id = m.Characteristics3ValueId
                                    WHERE  m.Companygroupid='" + identity.CompanyGroupId + @"'
                                            and m.CompanyId='" + identity.CompanyId + @"'
                                        Order by m.[Description]   ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetRecipeConfigData(string plantId, string processId)
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                sql = @"SELECT rc.*,UserName=CASE WHEN RC.RecipeLevel='AR' THEN mat.UserName ELSE matc.UserName END,
			                        s1=CASE WHEN  RC.SpecificationLevel1='AR' THEN s1.UserName ELSE s1c.UserName END,
                        			s2=CASE WHEN  RC.SpecificationLevel2='AR' THEN s2.UserName ELSE s2c.UserName END
                        FROM  SCS.RecipeConfig RC
                        LEFT JOIN   hkp.MaterialAttribute mat on mat.id=RC.RecipeDependAttributeId
                        LEFT JOIN   hkp.MaterialAttribute s1 on s1.id=RC.SpecificationAttributeId1
                        LEFT JOIN   hkp.MaterialAttribute s2 on s2.id=RC.SpecificationAttributeId2
                        
                        LEFT JOIN   hkp.Characteristics matc on matc.id=RC.RecipeDependCharacteristicsId
                        LEFT JOIN   hkp.Characteristics s1c on s1c.id=RC.SpecificationCharacteristicId1
                        LEFT JOIN   hkp.Characteristics s2c on s2c.id=RC.SpecificationCharacteristicId2
                        WHERE rc.PlantId='" + plantId + "' AND rc.ProcessId='" + processId + "'  ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetListByMMId(GridParameter parameters, string materialmasterid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT m.Id
	                                ,m.[Description]
	                                ,m.Code
	                                ,m.UserName
	                                ,m.[BatchSize]
	                                ,uombs.UserName UomBatchSize
	                                ,m.MaterialAvgWeight
	                                ,uomavg.UserName UomAvgWeight
	                                ,m.AvgUom
	                                ,m.Uom
	                                ,m.StartPressure
	                                ,m.EndPressure
	                                ,m.StartTemperature
	                                ,m.EndTemperature
	                                ,m.GradientPressure
	                                ,m.GradientTemperature
	                                ,mm.Code MaterialMasterCode
	                                ,mm.[UserName] MaterialMasterDescription
	                                ,mm.Id MaterialMasterId,'' GridNO
	                               -- ,mg.GridNO
	                                ,mt.[Description] MaterialType
	                                ,mgr.UserName MaterialGroup
	                                ,mm.UserName MaterialMasterUserName
	                                ,uom.UserName BaseUOM
	                                ,m.Characteristics1Id
	                                ,m.Characteristics2Id
	                                ,m.Characteristics3Id
	                                ,m.Characteristics1ValueId
	                                ,m.Characteristics2ValueId
	                                ,m.Characteristics3ValueId
	                                ,p.UserName Process,m.ProcessCriteriaId,pct.UserName ProcessCriteria
	                                ,m.ProcessId
	                                ,c1.StandardName Characteristics1
	                                ,c2.StandardName Characteristics2
	                                ,c3.StandardName Characteristics3
	                                ,'xyz' SelectedCharacteristics
	                                ,Characteristics1Selected = CASE isnull(m.Characteristics1Id, '')
		                                WHEN ''
			                                THEN 0
		                                ELSE 1
		                                END
	                                ,Characteristics2Selected = CASE isnull(m.Characteristics2Id, '')
		                                WHEN ''
			                                THEN 0
		                                ELSE 1
		                                END
	                                ,Characteristics3Selected = CASE isnull(m.Characteristics3Id, '')
		                                WHEN ''
			                                THEN 0
		                                ELSE 1
		                                END
	                                ,cv1.[Description] Characteristics1Value
	                                ,cv2.[Description] Characteristics2Value
	                                ,cv3.[Description] Characteristics3Value
                                FROM trn.RecipeGlobalMaster m
                                LEFT JOIN mst.MaterialMaster mm ON mm.Id = m.MaterialMasterId
                          --      LEFT JOIN hkp.MaterialGrid mg ON mg.Id = mm.MaterialGridId
                                LEFT JOIN hkp.MaterialType mt ON mt.Id = mm.materialTypeId
                                LEFT JOIN MST.MaterialGroupMaster mgr ON mgr.Id = mm.MaterialGroupMasterId
                                LEFT JOIN SCS.UnitOfMeasurement uom ON uom.Id = mm.BaseUOMId
                                LEFT JOIN SCS.UnitOfMeasurement uombs ON uombs.Id = m.Uom
                                LEFT JOIN SCS.UnitOfMeasurement uomavg ON uomavg.Id = m.AvgUom
                                LEFT JOIN hkp.Process p ON p.Id = m.ProcessId
                                    LEFT JOIN hkp.ProcessCriteria pct ON pct.Id = m.ProcessCriteriaId
                                LEFT JOIN hkp.Characteristics c1 ON c1.Id = m.Characteristics1Id
                                LEFT JOIN hkp.Characteristics c2 ON c2.Id = m.Characteristics2Id
                                LEFT JOIN hkp.Characteristics c3 ON c3.Id = m.Characteristics3Id
                                LEFT JOIN hkp.CharacteristicsValue cv1 ON cv1.Id = m.Characteristics1ValueId
                                LEFT JOIN hkp.CharacteristicsValue cv2 ON cv2.Id = m.Characteristics2ValueId
                                LEFT JOIN hkp.CharacteristicsValue cv3 ON cv3.Id = m.Characteristics3ValueId
                                    WHERE m.MaterialMasterId='" + materialmasterid + @"'
                                            and  m.Companygroupid='" + identity.CompanyGroupId + @"'
                                            and m.CompanyId='" + identity.CompanyId + @"'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetMasterId(string materialmasterid)
        {
            try
            {
                string _sql = "select Id from trn.RecipeGlobalMaster where MaterialmasterId='" + materialmasterid + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetRecipeTag(string RecipeId)
        {
            try
            {
                string _sql = @"select distinct p.Description ProductionOrder,p.Id from
                                [TRN].[ProductionOrderRecipeTag] t
                                left outer join TRN.SalesOrderMaster p on t.ProductionOrderMasterId=p.Id
                                Where t.RecipeMasterId='" + RecipeId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetRecipeByPOCbo(string pomid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"select distinct r.Id [Value] ,r.UserName [Text] from trn.RecipeMaster r
                                    left outer join trn.ProductionOrderRecipeTag  t  on r.Id=t.RecipeMasterId
                                    where t.ProductionOrderMasterId='" + pomid + @"' and t.CompanyId='" + identity.CompanyId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetSkuAsperConfig(string entityid, string MaterialMasterId)
        {
            try
            {
                string _sql = @"SELECT c.Id
	                            , p1.[UserName] Characteristics1
	                            , c.Characteristics1Id
	                            , p2.[UserName] Characteristics2
	                            , c.Characteristics2Id
	                            , p3.[UserName] Characteristics3
	                            , c.Characteristics3Id
	                            , 'xyz' SelectedCharacteristics
	                            , Characteristics1Selected = CASE isnull(c.Characteristics1Id, '') WHEN '' THEN 0 ELSE 1 END
	                            , Characteristics2Selected = CASE isnull(c.Characteristics2Id, '') WHEN '' THEN 0 ELSE 1 END
	                            , Characteristics3Selected = CASE isnull(c.Characteristics3Id, '') WHEN '' THEN 0 ELSE 1 END
                            FROM [TRN].[RecipeConfig] c
                            LEFT JOIN hkp.Characteristics p1 ON p1.Id = c.Characteristics1Id
                            LEFT JOIN hkp.Characteristics p2 ON p2.Id = c.Characteristics2Id
                            LEFT JOIN hkp.Characteristics p3 ON p3.Id = c.Characteristics3Id
                            where c.EntityId='"+ entityid + @"' and c.MaterialGridId=(select MaterialGridId from mst.MaterialMaster where Id='" + MaterialMasterId + @"' )";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<ComboModel> GetProcessCriteriaCbo(string companyGrupId)
        {
            var sql = @"SELECT PC.Id,PC.UserName FROM [HKP].[ProcessCriteria]	PC
					  LEFT JOIN [HKP].[CompanyGroupProcessCriteria] CGPC ON  CGPC.ProcessCriteriaId=PC.Id
					  WHERE CGPC.CompanyGroupId='" + companyGrupId + @"' ORDER BY UserName";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public IEnumerable<ComboModel> GetSubProcessCbo(string companyGrupId, string ProcessId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @" SELECT S.Id,S.UserName FROM HKP.SubProcess S
                              LEFT JOIN (SELECT * FROM MST.CompanySubProcess WHERE CompanyId='" + companyGrupId + @"') cs ON cs.SubProcessId= s.Id
                              WHERE S.Id = (SELECT RecipeDependonSubprocessId FROM [SCS].[RecipeConfig] WHERE ProcessId='" + ProcessId + @"')
                              ORDER BY S.UserName";
                return _sqlRepository.GetCombo(_sql, "Id", "UserName");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetMaterialArticle(GridParameter parameters, string materialMasterId)
        {
            try
            {
                var sql = "";
                if (!string.IsNullOrEmpty(materialMasterId))
                    sql = " WHERE MaterialMasterId='" + materialMasterId + "'";

                parameters.CmdText = @"SELECT ART.Id, ART.MaterialMasterId, MM.UserName AS MaterialMasterName
                                        , MM.Code AS MaterialCode, MG.UserName AS MaterialGroup
                                        , ART.Code, ART.ShortName, ART.StandardName
                                    FROM MST.MaterialMasterArticle AS ART
                                    LEFT JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId=MM.Id 
                                    LEFT JOIN MST.MaterialGroupMaster AS MG ON MM.MaterialGroupMasterId=MG.Id 
                                    LEFT JOIN HKP.MaterialType AS MT ON MG.MaterialTypeId=MT.Id" + sql;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetRecipeMaterialGroup()
        {
            try
            {
                var sql = @"SELECT RMGM.Id RecipeMaterialGroupingMasterId,RMGM.Sequence,RMGM.Code,RMGM.UserName,RMGM.StandardName, RMGM.QtyValue,RMGM.UomId, UOM.UserName UnitOfMeasurement
                            FROM [MST].[RecipeMaterialGroupingMaster] RMGM
                            LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=RMGM.UomId";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetRecipeGlobalMaterialGroup(string recipeGlobalSubprocessId)
        {
            try
            {
                var sql = @"SELECT RGMG.Id, RMGM.Id RecipeGlobalMaterialGroupId,RMGM.Sequence,RMGM.Code,RMGM.UserName,RMGM.StandardName, RMGM.QtyValue,RMGM.UomId, UOM.UserName UnitOfMeasurement
                            ,RGMG.RecipeGlobalMasterId,RGMG.RecipeGlobalSubprocessId,RGMG.Value
                            FROM [MST].[RecipeMaterialGroupingMaster] RMGM
                            LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=RMGM.UomId
                            LEFT JOIN [TRN].[RecipeGlobalMaterialGroup] RGMG ON RGMG.RecipeMaterialGroupingMasterId=RMGM.Id
                            WHERE RGMG.RecipeGlobalSubprocessId='" + recipeGlobalSubprocessId + @"' ORDER BY RMGM.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Insert n Update

        private void Validation(string masterid, string Recipe)
        {
            //get ProdOrderRecipeTag
            try
            {
                string polist = "";
                var list = GetRecipeTag(masterid);
                if (list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        var dic = (Dictionary<string, object>)item;
                        if (string.IsNullOrEmpty(polist))
                        {
                            polist = dic["ProductionOrder"].ToString();
                        }
                        else
                        {
                            polist += ", " + dic["ProductionOrder"];
                        }
                    }//for
                    throw new Exception("Recipe: [" + Recipe + "] has already been tagged with Order" + (list.Count() > 1 ? "s" : "") + ": [" + polist + "]");
                }//count
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OutMaster(RecipeGlobalMaster from_ui, out RecipeGlobalMaster from_db)
        {
            from_db = null;
            try
            {
                #region init Object

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //from_db = GetMaster(from_ui.Id);
                from_db = Find(from_ui.Id);
                //Validation
                //Validation(from_ui.Id, from_ui.Description);

                if (from_db == null || from_db.Id == null || from_db.Id == "")
                {
                    from_db = new RecipeGlobalMaster
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(from_db);

                    from_db.Id = GetPK();//set pk
                    from_db.CompanyGroupId = identity.CompanyGroupId;
                    from_db.CompanyId = identity.CompanyId;
                }
                else
                {
                    from_db.ModelState = ModelState.Modified;
                    AuditService.Log(from_db);
                }

                #endregion init Object

                #region Set Fields

                from_db.Characteristics1Id = (string.IsNullOrEmpty(from_ui.Characteristics1ValueId) ? null : from_ui.Characteristics1Id);
                from_db.Characteristics3Id = (string.IsNullOrEmpty(from_ui.Characteristics3ValueId) ? null : from_ui.Characteristics3Id);
                from_db.Characteristics2Id = (string.IsNullOrEmpty(from_ui.Characteristics2ValueId) ? null : from_ui.Characteristics2Id);
                from_db.Characteristics1ValueId = from_ui.Characteristics1ValueId;
                from_db.Characteristics2ValueId = from_ui.Characteristics2ValueId;
                from_db.Characteristics3ValueId = from_ui.Characteristics3ValueId;

                from_db.BatchSize = from_ui.BatchSize;
                from_db.MaterialAvgWeight = from_ui.MaterialAvgWeight;
                from_db.AvgUom = from_ui.AvgUom;
                from_db.Uom = from_ui.Uom;
                from_db.Code = from_ui.Code;
                from_db.Description = from_ui.Description;
                from_db.MaterialMasterId = from_ui.MaterialMasterId;
                from_db.ProcessId = from_ui.ProcessId;
                from_db.ProcessCriteriaId = from_ui.ProcessCriteriaId;
                from_db.UserName = from_ui.UserName;
                from_db.StartTemperature = from_ui.StartTemperature;
                from_db.StartPressure = from_ui.StartPressure;
                from_db.EndPressure = from_ui.EndPressure;
                from_db.GradientTemperature = from_ui.GradientTemperature;
                from_db.GradientPressure = from_ui.GradientPressure;
                from_db.EndTemperature = from_ui.EndTemperature;
                from_db.EntityId = from_ui.EntityId;
                from_db.MaterialAttributeId = from_ui.MaterialAttributeId;
                from_db.AttributeValueId = from_ui.AttributeValueId;
                from_db.Specification1Id = from_ui.Specification1Id;
                from_db.Specification1ValueId = from_ui.Specification1ValueId;
                from_db.Specification2Id = from_ui.Specification2Id;
                from_db.Specification2ValueId = from_ui.Specification2ValueId;
                //from_db.Thickness = from_ui.Thickness;
                //from_db.Weight = from_ui.Weight;

                #endregion Set Fields
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertORUpdateMaster(RecipeGlobalMaster master, out string masterid)
        {
            RecipeGlobalMaster localMaster = null;
            masterid = string.Empty;
            var flag = false;
            try
            {
                OutMaster(master, out localMaster);
                InsertOrUpdateGraph(localMaster);
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                masterid = localMaster.Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public string GetUtilityPK()
        {
            return "RWU" + _pkGeneratorService.GetAutoNumber(nameof(RecipeGlobalUtility), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InsertORUpdateDetail(RecipeGlobalUtility recipeutility)
        {

            try
            {

                if (string.IsNullOrEmpty(recipeutility.Id))
                {
                    recipeutility.Id = GetUtilityPK();
                    _recipeGlobalutilityservice.Insert(recipeutility);
                }
                else
                {
                    _recipeGlobalutilityservice.Update(recipeutility);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, recipeutility.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void InsertORUpdateDetailChild(RecipeGlobalRawMaterial detailchild)
        {
            RecipeGlobalRawMaterial localDetail = null;
            IEnumerable<object> detailList = null;
            var flag = false;
            try
            {
                _reciperawmaterialservice.OutDetail(detailchild, out localDetail);
                //AuditService.Log(localDetail, localDetail.Archive);
                _reciperawmaterialservice.InsertOrUpdateGraph(localDetail);

                //validation
                detailList = _reciperawmaterialservice.GetList(localDetail.MaterialMasterId);//get all child for this master

                _unitOfWork.BeginTransaction();
                flag = true;
                _reciperawmaterialservice.CheckDuplicate(localDetail, detailList);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #endregion Insert n Update

        #region Delete
        public void DeleteRecipe(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new CustomException("Recipe is not found...");

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Find(id);
                if (data != null)
                {

                    _recipeGlobalutilityservice.ExecuteSqlCommand("DELETE FROM [TRN].[RecipeGlobalUtility] Where RecipeGlobalMasterId='" + id + "'");
                    _reciperawmaterialservice.ExecuteSqlCommand("DELETE FROM [TRN].[RecipeGlobalRawMaterial] Where RecipeGlobalMasterId='" + id + "'");
                    _recipeGlobaloperationservice.ExecuteSqlCommand("DELETE FROM [TRN].[RecipeGlobalOperation] Where RecipeGlobalMasterId='" + id + "'");
                    _recipesubprocessservice.ExecuteSqlCommand("DELETE FROM [TRN].[RecipeGlobalMaterialGroup] Where RecipeGlobalMasterId='" + id + "'");
                    _recipesubprocessservice.ExecuteSqlCommand("DELETE FROM [TRN].[RecipeGlobalSubprocess] Where RecipeGlobalMasterId='" + id + "'");
                    _recipesubprocessservice.ExecuteSqlCommand("DELETE FROM [TRN].[RecipeMaterial] Where RecipeGlobalMasterId='" + id + "'");
                    base.Delete(data);
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

        private void DelDetailList(string id, IEnumerable<RecipeGlobalRawMaterial> from_db_child, out IEnumerable<RecipeGlobalSubprocess> from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = _recipesubprocessservice.GetDetailList(id);
                foreach (var ui in from_db)
                {
                    var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                    if (db != null)
                    {
                        db.ModelState = ModelState.Deleted;
                        //get child as its properties
                        ///var dbc = from_db_child.Where(a => a.RecipeGlobalSubprocessId == db.Id).ToList();
                        ///if (dbc.Count() > 0)
                        ///{
                        ///    db.RecipeRawMaterialList = dbc;
                        ///}
                    }
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DelDetailChildListByMaster(string masterid, out IEnumerable<RecipeGlobalRawMaterial> from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ///from_db = _reciperawmaterialservice.GetDetailListByMasterId(masterid);
                foreach (var ui in from_db)
                {
                    var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                    if (db != null)
                    {
                        db.ModelState = ModelState.Deleted;
                    }
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Delete
    }
}