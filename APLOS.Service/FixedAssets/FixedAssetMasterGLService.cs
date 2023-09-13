#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.FixedAssets;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.FixedAssets
{
    public class FixedAssetMasterGLService : Service<FixedAssetMasterGL>, IFixedAssetMasterGLService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IFixedAssetMasterVendorReconGLService _fixedAssetVendorReconGLService;

        public FixedAssetMasterGLService(
              IRepositoryAsync<FixedAssetMasterGL> FixedAssetClassRepository
            , IPKGeneratorService pkGeneratorService
            , IFixedAssetMasterVendorReconGLService fixedAssetVendorReconGLService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(FixedAssetClassRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _fixedAssetVendorReconGLService = fixedAssetVendorReconGLService;
        }

        #endregion Constructor

        public void InsertOrUpdate(string masterId, FixedAssetMasterGL entity, IEnumerable<FixedAssetMasterVendorReconGL> fixedAssetVendorReconGL)
        {
            try
            {
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPK();
                    entity.FixedAssetMasterId = masterId;
                    InsertGraph(entity);
                }
                else
                {
                    UpdateGraph(entity);
                }
                IEnumerable<FixedAssetMasterGL> list = new List<FixedAssetMasterGL> { entity };
                _fixedAssetVendorReconGLService.InsertOrUpdate(list, fixedAssetVendorReconGL);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(FixedAssetMasterGL), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(FixedAssetMasterGL), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        //Using on FixedAssetMaster
        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "FixedAssetMasterGL Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                FixedAssetMasterGL entity = Find(id);
                // If section row inactive
                _fixedAssetVendorReconGLService.DeleteGraph(entity.Id);
                base.DeleteGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name,
                    MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #region FixedAssetGL

        public void InsertUpdateFixedAssetMasterGL(IEnumerable<FixedAssetMasterGL> entities, IEnumerable<FixedAssetMasterVendorReconGL> fixedAssetVendorReconGL)
        {
            var flag = false;
            try
            {
                var pk = GetMaxNumber();
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.ModelState = ModelState.Added;
                        pk.MaxNumber++;
                        //log
                        AuditService.Log(item);
                        item.Id = pk.MaxNumber.ToString();
                    }
                    else
                    {
                        item.ModelState = ModelState.Modified;
                        AuditService.Log(item);
                        //log
                    }

                    InsertOrUpdateGraph(item);
                }

                _fixedAssetVendorReconGLService.InsertOrUpdate(entities, fixedAssetVendorReconGL);

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        [Obsolete("Have to shift in controller")]
        public IEnumerable<object> GetFixedAssetItemCbo()
        {
            return Enum.GetValues(typeof(FixedAssetItemEnum)).Cast<FixedAssetItemEnum>().Select(v => new
            {
                Text = v.ToString(),
                Value = v.ToString()
            });
        }

        public GridModel GetDataByFixedAssetMasterId(GridParameter parameters, string fixedAssetMasterId, string coaId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (fixedAssetMasterId != null)
                {
                    parameters.CmdText = @"SELECT FAD.*
								,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS AssetGLInfo
								,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS AccDepreciationGLInfo
								,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS DepreciationGLInfo
								,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS AUCGLInfo
								,GLGI5.AccountCode + ' - ' + GLGI5.UserName AS DownPaymentGLInfo
								,GLGI6.AccountCode + ' - ' + GLGI6.UserName AS ClearingAccountGLInfo
								,GLGI7.AccountCode + ' - ' + GLGI7.UserName AS GainOnSaleOfAssetGLInfo
								,GLGI8.AccountCode + ' - ' + GLGI8.UserName AS LossOnSaleOfAssetGLInfo
								,GLGI9.AccountCode + ' - ' + GLGI9.UserName AS LossOnDisposalAssetGLInfo
                                 FROM [HKP].[FixedAssetMasterGL] FAD
                                 LEFT OUTER JOIN [MST].[FixedAssetMaster] FAM ON FAD.FixedAssetMasterId = FAD.Id
							  LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=FAD.FixedAssetMasterGLId
							  LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
							  LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=FAD.DepreciationGLId
						      LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FAD.AssetUnderConstructionGLId
							  LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI5 ON GLGI5.Id=FAD.DownPaymentGLId
        			          LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI6 ON GLGI6.Id=FAD.ClearingAccountGLId
							  LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI7 ON GLGI7.Id=FAD.GainOnSaleOfAssetGLId
        			          LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI8 ON GLGI8.Id=FAD.LossOnSaleOfAssetGLId
        			          LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI9 ON GLGI9.Id=FAD.LossOnDisposalAssetGLId
                            WHERE FAD.FixedAssetMasterId='" + fixedAssetMasterId + @"' AND FAD.COAId='" + coaId + @"'  ";
                }
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public GridModel GetVendorReconDataByFixedAssetMasterId(GridParameter parameters, string fixedAssetMasterId, string coaId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (fixedAssetMasterId != null)
                {
                    parameters.CmdText = @"SELECT FADV.FixedAssetMasterGLId,FADV.PartyAccountGroupId,FADV.VendorReconGLId,GI.AccountCode as VendorReconGLCode,GI.UserName AS VendorRecontGLText FROM [HKP].[FixedAssetMasterVendorReconGL] AS FADV
                                            LEFT OUTER JOIN HKP.GLGeneralInfo AS GI ON FADV.VendorReconGLId = GI.Id
                                            LEFT OUTER JOIN [HKP].[FixedAssetMasterGL] AS FAD ON FADV.FixedAssetMasterGLId =FAD.Id
                            WHERE FADV.FixedAssetMasterId='" + fixedAssetMasterId + @"' AND FAD.COAId='" + coaId + @"'    ";
                }
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

       
        private string GetOp(string search)
        {
            return string.IsNullOrEmpty(search) ? "WHERE" : "AND";
        }

        #endregion FixedAssetGL

        public GridModel GetPartyAccountGroup(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT FADVR.Id
                                                ,FADVR.FixedAssetMasterGLId
                                                ,FADVR.VendorReconGLId
                                                ,FADVR.AddedBy
                                                ,FADVR.AddedDate
                                                ,FADVR.AddedFromIP
                                                ,FADVR.UpdatedBy
                                                ,FADVR.UpdatedDate
                                                ,FADVR.UpdatedFromIP
                                                ,PAG.Id AS PartyAccountGroupId
                                                ,PAG.UserName
                                                ,PAG.Code
                                                ,VR.UserName AS VendorRecontGLText
                                                ,VR.AccountCode AS VendorReconGLCode
                                                FROM HKP.PartyAccountGroup AS PAG
                                                LEFT OUTER JOIN [HKP].[FixedAssetMasterVendorReconGL]AS FADVR ON FADVR.PartyAccountGroupId = PAG.Id
                                                	LEFT OUTER JOIN HKP.GLGeneralInfo AS VR ON FADVR.VendorReconGLId = VR.Id
                                                WHERE PAG.AccountType='" + ReconcileAccountEnum.Vendor + "' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel GetPartyAccountVD(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT FADV.*, GLGI4.AccountCode AS ClearingAccGLCode,GLGI4.UserName AS ClearingAccGLText,B.UserName BudgetName,A.UserName ActivityName FROM  [HKP].[FixedAssetMasterVendorReconGL] AS FADV
				 LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FADV.VendorReconGLId
				 LEFT OUTER JOIN MST.BudgetMaster BM ON FADV.VendorReconBudgetMasterId = BM.Id
				 LEFT OUTER JOIN HKP.Budget B ON BM.BudgetId = B.Id
				 LEFT OUTER JOIN HKP.Activity A ON FADV.VendorReconActivityId = A.Id  ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetPartyAccountWithAssignList(string partyAcId, string FixedAssetMasterGlId)
        {
            try
            {
                string _sql = @"SELECT F.FixedAssetMasterGLId,GL.UserName AS GL FROM HKP.FixedAssetMasterVendorReconGL f
                                LEFT OUTER JOIN HKP.FixedAssetMasterGL G ON F.FixedAssetMasterGLId=G.Id
                                LEFT OUTER JOIN HKP.GLGeneralInfo GL ON F.VendorReconGLId=GL.Id
                                WHERE F.PartyAccountGroupId='20175' AND F.FixedAssetMasterGLId='" + FixedAssetMasterGlId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetBudgetActivityCbo(string budgetId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT A.Id AS Value, A.UserName AS Text FROM HKP.Activity AS A
                                LEFT OUTER JOIN [MST].[BudgetMasterActivity] AS BA ON A.Id= BA.ActivityId
                                LEFT OUTER JOIN [MST].[BudgetMaster] AS BM ON BA.BudgetMasterId=BM.Id
                                LEFT OUTER JOIN HKP.Budget AS B ON BM.BudgetId = B.Id
                                WHERE B.Id='" + budgetId + "' ORDER BY A.UserName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetAccountGroupData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"select b.FixedAssetMasterGLId
                                ,a.UserName
                                ,b.VendorReconGLId
                                ,g.AccountCode +'-'+ g.UserName as VendorReconGL
                                ,b.VendorReconBudgetId
                                ,bu.UserName VendorReconBudget
                                ,b.VendorReconActivityId
                                ,ac.UserName VendorReconActivity
                                from hkp.PartyAccountGroup a
                                left outer join [HKP].[FixedAssetMasterVendorReconGL] b on a.Id=b.PartyAccountGroupId
                                left outer join hkp.GLGeneralInfo g on b.VendorReconGLId = g.Id
                                left outer join hkp.Budget bu on b.VendorReconBudgetId=bu.Id
                                left outer join hkp.Activity ac on b.VendorReconActivityId = ac.Id
                                where AccountType='vendor'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetAccountGroupData2()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"DECLARE @cols AS NVARCHAR(MAX),
                                 @query  AS NVARCHAR(MAX),
                                @accountType varchar(10)  = 'Vendor',
                                @gl varchar(10)='35'
                                select @cols = STUFF((SELECT DISTINCT ',' + QUOTENAME(UserName)
                                     FROM [HKP].[PartyAccountGroup] where AccountType='Vendor'
                                   FOR XML PATH(''), TYPE
                                   ).value('.', 'NVARCHAR(MAX)') ,1,1,'')
                                   --print @cols
                                set @query = N'SELECT ' + @cols + N', FixedAssetMasterGLId,PartyAccountGroupId,FixedAssetMasterId FROM
                                    (
                                 SELECT A.Id,b.FixedAssetMasterGLId,a.AccountType,B.VendorReconGLId,B.FixedAssetMasterId, B.PartyAccountGroupId,A.UserName AS ColumnName, C.UserName
                                 FROM  [HKP].[PartyAccountGroup] AS A
                                    LEFT  JOIN [HKP].[FixedAssetMasterVendorReconGL] AS B ON  A.Id=B.PartyAccountGroupId
                                    LEFT  JOIN [HKP].[GLGeneralInfo] C on B.VendorReconGLId = C.Id
	                                where a.AccountType='''+@accountType+'''
                                   ) x
                                   pivot
                                   (
                                    max(UserName)
                                    for ColumnName in (' + @cols + N')
                                   ) p '

                                exec sp_executesql @query;";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }


        private string FixedAssetMasterGLSql()
        {

            return @"DECLARE @sql_ nvarchar(max)
                         select FixedAssetMasterId
                                ,FixedAssetMasterName
                                ,AccDepreciationGLInfo
                                ,DepreciationGLInfo
                                ,AUCGLInfo
                                ,DownPaymentGLInfo
                                ,ClearingAccountGLInfo
                                , GainOnSaleOfAssetGLInfo
                                , LossOnSaleOfAssetGLInfo
                                , LossOnDisposalAssetGLInfo
                                , LessValueAssetGLInfo
								,COA
							    ,GL,
							    PartyAccountGroup +'GL' PartyAccountGroup
                                    INTO #tempOT
                                    from 
                                    (
                                    SELECT A.* FROM
	                                (
									SELECT FAD.Id
                                ,FAM.Id AS FixedAssetMasterId
                                ,FAM.UserName AS FixedAssetMasterName
                                ,FAD.AccumulatedDepreciationGLId
                                ,FAD.AssetUnderConstructionGLId
                                ,FAD.DepreciationGLId
                                ,FAD.DownPaymentGLId
                                ,FAD.ClearingAccountGLId
                                ,FAD.GainOnSaleOfAssetGLId
                                ,FAD.LossOnSaleOfAssetGLId
                                ,FAD.LossOnDisposalAssetGLId
                                ,FAD.LessValueAssetGLId

                                ,C.UserName as COA
                                ,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS AccDepreciationGLInfo
                                ,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS DepreciationGLInfo
                                ,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS AUCGLInfo
                                ,GLGI5.AccountCode + ' - ' + GLGI5.UserName AS DownPaymentGLInfo
                                ,GLGI6.AccountCode + ' - ' + GLGI6.UserName AS ClearingAccountGLInfo
                                ,GLGI7.AccountCode + ' - ' + GLGI7.UserName AS GainOnSaleOfAssetGLInfo
                                ,GLGI8.AccountCode + ' - ' + GLGI8.UserName AS LossOnSaleOfAssetGLInfo
                                ,GLGI9.AccountCode + ' - ' + GLGI9.UserName AS LossOnDisposalAssetGLInfo
                                ,GLGI10.AccountCode + ' - ' + GLGI10.UserName AS LessValueAssetGLInfo
                                
								,MGGL.gl,PartyAccountGroup
								

                                FROM MST.FixedAssetMaster As FAM
                                LEFT JOIN HKP.FixedAssetMasterGL AS FAD  ON FAD.FixedAssetMasterId=FAM.Id
                                LEFT JOIN(SELECT Id, UserName from HKP.COA where isnull(Id,'') ='1') C ON FAD.COAId=C.Id
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=FAD.DepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FAD.AssetUnderConstructionGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI5 ON GLGI5.Id=FAD.DownPaymentGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI6 ON GLGI6.Id=FAD.ClearingAccountGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI7 ON GLGI7.Id=FAD.GainOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI8 ON GLGI8.Id=FAD.LossOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI9 ON GLGI9.Id=FAD.LossOnDisposalAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI10 ON GLGI10.Id=FAD.LessValueAssetGLId    
					            LEFT JOIN ( SELECT MGL.Id,MGL.FixedAssetMasterId,GL.UserName GL
					,MGL.PartyAccountGroupId,PAG.UserName PartyAccountGroup
					FROM HKP.FixedAssetMasterVendorReconGL MGL
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=MGL.PartyAccountGroupId
					LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=MGL.VendorReconGLId
					LEFT JOIN MST.BudgetMaster AS BM ON MGL.VendorReconBudgetMasterId = BM.Id
					) MGGL ON MGGL.FixedAssetMasterId=FAM.Id
										) A
										group by  a.Id
										,a.FixedAssetMasterId
										,a.FixedAssetMasterName 
										,a.AccumulatedDepreciationGLId
										,a.AssetUnderConstructionGLId
										,a.DepreciationGLId
										,a.DownPaymentGLId
										,a.ClearingAccountGLId
										,a.GainOnSaleOfAssetGLId
										,a.LossOnSaleOfAssetGLId
										,a.LossOnDisposalAssetGLId
										,a.LessValueAssetGLId,a.gl
										,a.PartyAccountGroup,a.COA
										,a.AccDepreciationGLInfo
										,a.DepreciationGLId
										,a.DepreciationGLInfo
										,a.AUCGLInfo
										,a.DownPaymentGLInfo,
										a.ClearingAccountGLInfo,
										a.GainOnSaleOfAssetGLInfo,
										a.LossOnSaleOfAssetGLInfo,
										a.LossOnDisposalAssetGLId,
										a.LossOnDisposalAssetGLInfo,
										a.LessValueAssetGLInfo
                            ) TT
	                            DECLARE @sql nvarchar(max),
                                    @col nvarchar(max),
                                    @col1 nvarchar(max)

                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(PartyAccountGroup)	
                                FROM #tempOT 
                                FOR XML PATH ('')
                            )

                            SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                            MAX([GL]) FOR [PartyAccountGroup] IN ('+STUFF(@col,1,1,'')+')
								 
                            ) as pvt)'
				
                            EXEC sp_executesql @sql
                            drop table #tempOT
                    ";

        }
        private string FixedAssetMasterBudgetSql()
        {

            return @"DECLARE @sql_ nvarchar(max)
                         select FixedAssetMasterId
								,AccumulatedDepreciationBudgetName
								,DepreciationBudgetName
								,AssetUnderConstructionBudgetName
								,DownPaymentBudgetName
								,ClearingAccountBudgetName
								,GainOnSaleOfAssetBudgetName
								,LossOnSaleOfAssetBudgetName
								,LossOnDisposalAssetBudgetName
								,LessValueAssetBudgetName
                                ,COA
							    ,Budget,
							    PartyAccountGroup +'Budget' PartyAccountGroup
                                    INTO #tempOT
                                    from 
                                    (
                                    SELECT A.* FROM
	                                (
									SELECT FAD.Id
                                ,FAM.Id AS FixedAssetMasterId
                                ,FAM.UserName AS FixedAssetMasterName

                                 ,FAD.AccumulatedDepreciationBudgetMasterId
								,ADBudget.UserName AS   AccumulatedDepreciationBudgetName
								,FAD.DepreciationBudgetMasterId
								,DEPBudget.UserName AS   DepreciationBudgetName
								,FAD.AssetUnderConstructionBudgetMasterId
								,AUCBudget.UserName AS   AssetUnderConstructionBudgetName
								,FAD.DownPaymentBudgetMasterId
								,DPBudget.UserName AS   DownPaymentBudgetName
								,FAD.ClearingAccountBudgetMasterId
								,CABudget.UserName AS   ClearingAccountBudgetName
								,FAD.GainOnSaleOfAssetBudgetMasterId
								,GOSBudget.UserName AS   GainOnSaleOfAssetBudgetName
								,FAD.LossOnSaleOfAssetBudgetMasterId
								,LOSBudget.UserName AS   LossOnSaleOfAssetBudgetName
								,FAD.LossOnDisposalAssetBudgetMasterId
								,LODBudget.UserName AS   LossOnDisposalAssetBudgetName
								,FAD.LessValueAssetBudgetMasterId
								,LEVBudget.UserName AS   LessValueAssetBudgetName

                                ,C.UserName as COA
                
                                
								,MGGL.Budget,PartyAccountGroup
                                FROM MST.FixedAssetMaster As FAM

                                LEFT JOIN HKP.FixedAssetMasterGL AS FAD  ON FAD.FixedAssetMasterId=FAM.Id
                                LEFT JOIN(SELECT Id, UserName from HKP.COA where isnull(Id,'') ='1') C ON FAD.COAId=C.Id
								
                                LEFT JOIN MST.BudgetMaster AS ADBudgetM ON FAD.AccumulatedDepreciationBudgetMasterId = ADBudgetM.Id
                                LEFT JOIN HKP.Budget AS ADBudget ON ADBudgetM.BudgetId = ADBudget.Id
								LEFT JOIN MST.BudgetMaster AS DEPBudgetM ON FAD.DepreciationBudgetMasterId = DEPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DEPBudget ON     DEPBudget.Id =   DEPBudgetM.BudgetId
								 LEFT JOIN MST.BudgetMaster AS   AUCBudgetM ON   FAD.AssetUnderConstructionBudgetMasterId =   AUCBudgetM.Id
                                LEFT JOIN HKP.Budget AS   AUCBudget ON   AUCBudget.Id =   AUCBudgetM.BudgetId
								 LEFT JOIN MST.BudgetMaster AS   DPBudgetM ON   FAD.DownPaymentBudgetMasterId =   DPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DPBudget ON   DPBudget.Id =   DPBudgetM.BudgetId
								LEFT JOIN MST.BudgetMaster AS   CABudgetM ON   FAD.ClearingAccountBudgetMasterId =   CABudgetM.Id
                                LEFT JOIN HKP.Budget AS   CABudget ON   CABudget.Id =   CABudgetM.BudgetId
								LEFT JOIN MST.BudgetMaster AS   GOSBudgetM ON   FAD.GainOnSaleOfAssetBudgetMasterId = GOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   GOSBudget ON   GOSBudget.Id =   GOSBudgetM.BudgetId
								LEFT JOIN MST.BudgetMaster AS   LOSBudgetM ON   FAD.LossOnSaleOfAssetBudgetMasterId =   LOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LOSBudget ON   LOSBudget.Id =   LOSBudgetM.BudgetId
								LEFT JOIN MST.BudgetMaster AS   LODBudgetM ON   FAD.LossOnDisposalAssetBudgetMasterId =   LODBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LODBudget ON   LODBudget.Id =   LODBudgetM.BudgetId
								LEFT JOIN MST.BudgetMaster AS   LEVBudgetM ON   FAD.LessValueAssetBudgetMasterId =   LEVBudgetM.Id
								LEFT JOIN HKP.Budget AS   LEVBudget ON   LEVBudget.Id =   LEVBudgetM.BudgetId
					            LEFT JOIN ( SELECT MGL.Id,MGL.FixedAssetMasterId,B.UserName as Budget
					,MGL.PartyAccountGroupId,PAG.UserName PartyAccountGroup,PAG.AccountType
					FROM HKP.FixedAssetMasterVendorReconGL MGL
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=MGL.PartyAccountGroupId
					LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=MGL.VendorReconGLId
					LEFT JOIN MST.BudgetMaster AS BM ON MGL.VendorReconBudgetMasterId = BM.Id
				 	LEFT JOIN  HKP.Budget as B on B.Id= MGL.VendorReconBudgetMasterId
					) MGGL ON MGGL.FixedAssetMasterId=FAM.Id and MGGL.AccountType='Vendor'
										) A
										group by  a.Id
										,a.FixedAssetMasterId
										,a.FixedAssetMasterName 
										,a.AccumulatedDepreciationBudgetMasterId
								,a.AccumulatedDepreciationBudgetName
								,a.DepreciationBudgetName
								,a.AssetUnderConstructionBudgetMasterId
							    ,a.AssetUnderConstructionBudgetName
								,a.DownPaymentBudgetMasterId
								 ,a.DownPaymentBudgetName
								 ,a.ClearingAccountBudgetMasterId
								 ,a.ClearingAccountBudgetName
								 ,a.GainOnSaleOfAssetBudgetMasterId
								 ,a.GainOnSaleOfAssetBudgetName
								 ,a.LossOnSaleOfAssetBudgetMasterId
								 ,a.LossOnSaleOfAssetBudgetName
								 ,a.LossOnDisposalAssetBudgetMasterId
								 ,a.LossOnDisposalAssetBudgetName
								 ,a.LessValueAssetBudgetMasterId
								 ,a.LessValueAssetBudgetName
								 ,a.COA,a.Budget,a.PartyAccountGroup
								 ,a.DepreciationBudgetMasterId
										
                            ) TT
	                            DECLARE @sql nvarchar(max),
                                    @col nvarchar(max),
                                    @col1 nvarchar(max)

                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(PartyAccountGroup)	
                                FROM #tempOT 
                                FOR XML PATH ('')
                            )

                            SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                            MAX([Budget]) FOR [PartyAccountGroup] IN ('+STUFF(@col,1,1,'')+')
								 
                            ) as pvt)'
				
                            EXEC sp_executesql @sql
                            drop table #tempOT


                    ";

        }
        private string FixedAssetMasterActivitySql()
        {

            return @"DECLARE @sql_ nvarchar(max)
                         select FixedAssetMasterId
                                 
                                ,AccumulatedDepreciationActivityName
                                ,DepreciationActivityName
                                ,AssetUnderConstructionActivityName
                                ,DownPaymentActivityName
                                ,ClearingAccountActivityNam                               
                                ,GainOnSaleOfAssetActivityName
                                ,LossOnSaleOfAssetActivityName
                                ,LossOnDisposalAssetActivityName
                                ,LessValueAssetActivityName
                                ,COA
							    ,Activity,
							    PartyAccountGroup +'Activity' PartyAccountGroup
                                    INTO #tempOT
                                    from 
                                    (
                                    SELECT A.* FROM
	                                (
									SELECT FAD.Id
                                ,FAM.Id AS FixedAssetMasterId
                                ,FAM.UserName AS FixedAssetMasterName

                                 ,FAD.AccumulatedDepreciationActivityId
                                ,ADActivity.UserName AS AccumulatedDepreciationActivityName
                                ,FAD.DepreciationActivityId
                                ,DEPActivity.UserName AS DepreciationActivityName
                                ,FAD.AssetUnderConstructionActivityId
                                ,AUCActivity.UserName AS AssetUnderConstructionActivityName
                                ,FAD.DownPaymentActivityId
                                ,DPActivity.UserName AS DownPaymentActivityName
                                ,FAD.ClearingAccountActivityId                                
                                ,CAActivity.UserName AS ClearingAccountActivityNam                               
                                ,FAD.GainOnSaleOfAssetActivityId
                                ,GOSActivity.UserName AS GainOnSaleOfAssetActivityName
                                ,FAD.LossOnSaleOfAssetActivityId
                                ,LOSActivity.UserName AS LossOnSaleOfAssetActivityName
                                ,FAD.LossOnDisposalAssetActivityId
                                ,LODActivity.UserName AS LossOnDisposalAssetActivityName
								,FAD.LessValueAssetActivityId                          
                                ,LEActivity.UserName AS LessValueAssetActivityName

                                ,C.UserName as COA
                
                                
								,MGGL.Activity,PartyAccountGroup
                                FROM MST.FixedAssetMaster As FAM

                                LEFT JOIN HKP.FixedAssetMasterGL AS FAD  ON FAD.FixedAssetMasterId=FAM.Id
                                LEFT JOIN(SELECT Id, UserName from HKP.COA where isnull(Id,'') ='1') C ON FAD.COAId=C.Id
								
                                 LEFT JOIN HKP.Activity AS ADActivity ON FAD.AccumulatedDepreciationActivityId = ADActivity.Id
                                LEFT JOIN HKP.Activity AS DEPActivity ON FAD.DepreciationActivityId = DEPActivity.Id
                                LEFT JOIN HKP.Activity AS AUCActivity ON FAD.AssetUnderConstructionActivityId = AUCActivity.Id                                
                                LEFT JOIN HKP.Activity AS DPActivity ON FAD.DownPaymentActivityId = DPActivity.Id                                
                                LEFT JOIN HKP.Activity AS CAActivity ON FAD.ClearingAccountActivityId = CAActivity.Id                               
                                LEFT JOIN HKP.Activity AS GOSActivity ON FAD.GainOnSaleOfAssetActivityId = GOSActivity.Id                                
                                LEFT JOIN HKP.Activity AS LOSActivity ON FAD.LossOnSaleOfAssetActivityId = LOSActivity.Id
                                LEFT JOIN HKP.Activity AS LODActivity ON FAD.LossOnDisposalAssetActivityId = LODActivity.Id 
								LEFT JOIN HKP.Activity AS LEActivity ON FAD.LessValueAssetActivityId = LEActivity.Id  

					            LEFT JOIN ( SELECT MGL.Id,MGL.FixedAssetMasterId,A.UserName as Activity
					,MGL.PartyAccountGroupId,PAG.UserName PartyAccountGroup,PAG.AccountType
					FROM HKP.FixedAssetMasterVendorReconGL MGL
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=MGL.PartyAccountGroupId
					LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=MGL.VendorReconGLId
					LEFT JOIN MST.BudgetMaster AS BM ON MGL.VendorReconBudgetMasterId = BM.Id
				 	LEFT JOIN  HKP.Budget as B on B.Id= MGL.VendorReconBudgetMasterId
				 	LEFT JOIN  HKP.Activity as A on A.Id= MGL.VendorReconActivityId

					) MGGL ON MGGL.FixedAssetMasterId=FAM.Id and MGGL.AccountType='Vendor'
										) A
										group by  a.Id
										,a.FixedAssetMasterId
										,a.FixedAssetMasterName 
										,a.AccumulatedDepreciationActivityId
                                ,a.AccumulatedDepreciationActivityName
                                ,a.DepreciationActivityId
                                , a.DepreciationActivityName
                                ,a.AssetUnderConstructionActivityId
                                ,a.AssetUnderConstructionActivityName
                                ,a.DownPaymentActivityId
                                , a.DownPaymentActivityName
                                ,a.ClearingAccountActivityId                                
                                ,a.ClearingAccountActivityNam                               
                                ,a.GainOnSaleOfAssetActivityId
                                ,a.GainOnSaleOfAssetActivityName
                                ,a.LossOnSaleOfAssetActivityId
                                ,a.LossOnSaleOfAssetActivityName
                                ,a.LossOnDisposalAssetActivityId
                                , a.LossOnDisposalAssetActivityName
								,a.LessValueAssetActivityId                          
                                ,a.LessValueAssetActivityName
                                 ,a.COA,a.Activity,a.PartyAccountGroup
										
										
                            ) TT
	                            DECLARE @sql nvarchar(max),
                                    @col nvarchar(max),
                                    @col1 nvarchar(max)

                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(PartyAccountGroup)	
                                FROM #tempOT 
                                FOR XML PATH ('')
                            )

                            SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                            MAX([Activity]) FOR [PartyAccountGroup] IN ('+STUFF(@col,1,1,'')+')
								 
                            ) as pvt)'
				
                            EXEC sp_executesql @sql
                            drop table #tempOT

                    ";

        }




        public void FixedAssetMasterReport()
        {
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2013;

                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Fixed Asset GL Report";

                DataTable dtFixedAssetMasterGL = _sqlRepository.GetDataTable(FixedAssetMasterGLSql());
                DataTable dtFixedAssetMasterBudget = _sqlRepository.GetDataTable(FixedAssetMasterBudgetSql());
                DataTable dtFixedAssetMasterActivity = _sqlRepository.GetDataTable(FixedAssetMasterActivitySql());

                int ROW = 6;
                int COL = 1;

                sheet[ROW, COL].Text = "Sl No.";
                sheet[ROW, COL].ColumnWidth = 3;
                int colSlNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Fixed Asset Master";
                sheet[ROW, COL].ColumnWidth = 15;
                int colFixedAssetMaster = COL;
                COL++;
                sheet[ROW, COL].Text = "Acc.Dep.GL";
                sheet[ROW, COL].ColumnWidth = 15;
                int colAccDepGL = COL;
                COL++;
                sheet[ROW, COL].Text = "Acc.Dep.Budget";
                sheet[ROW, COL].ColumnWidth = 15;
                int colAccDepBudget = COL;
                COL++;
                sheet[ROW, COL].Text = "Acc.Dep.Activity";
                sheet[ROW, COL].ColumnWidth = 15;
                int colAccDepActivity = COL;
                COL++;

                sheet[ROW, COL].Text = "Depreciation GL";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDepreciationGL = COL;
                COL++;
                sheet[ROW, COL].Text = "Depreciation Budget";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDepreciationBudget = COL;
                COL++;
                sheet[ROW, COL].Text = "Depreciation Activity";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDepreciationActivity = COL;
                COL++;
                sheet[ROW, COL].Text = "AUC GL";
                sheet[ROW, COL].ColumnWidth = 15;
                int colAUCGL = COL;
                COL++;
                sheet[ROW, COL].Text = "AUC Budget";
                sheet[ROW, COL].ColumnWidth = 15;
                int colAUCBudget = COL;
                COL++;
                sheet[ROW, COL].Text = "AUC Activity";
                sheet[ROW, COL].ColumnWidth = 15;
                int colAUCActivity = COL;
                COL++;
                sheet[ROW, COL].Text = "Down Payment GL";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDownPaymentGL = COL;
                COL++;
                sheet[ROW, COL].Text = "Down Payment Budget";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDownPaymentBudget = COL;
                COL++;
                sheet[ROW, COL].Text = "Down Payment Activity";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDownPaymentActivity = COL;
                COL++;
     
                sheet[ROW, COL].Text = "Clearing Account GL";
                sheet[ROW, COL].ColumnWidth = 15;
                int colClearingAccountGL = COL;
                COL++;
                sheet[ROW, COL].Text = "Clearing Account Budget";
                sheet[ROW, COL].ColumnWidth = 15;
                int colClearingAccountBudget = COL;
                COL++;
                sheet[ROW, COL].Text = "Clearing Account Activity";
                sheet[ROW, COL].ColumnWidth = 15;
                int colClearingAccountActivity = COL;
                COL++;
                sheet[ROW, COL].Text = "Gain On Sale Asset GL";
                sheet[ROW, COL].ColumnWidth = 15;
                int colGainOnSaleOfAssetGL = COL;
                COL++;
                sheet[ROW, COL].Text = "Gain On Sale Asset Budget";
                sheet[ROW, COL].ColumnWidth = 15;
                int colGainOnSaleOfAssetBudget = COL;
                COL++;
                sheet[ROW, COL].Text = "Gain On Sale Asset Activity";
                sheet[ROW, COL].ColumnWidth = 15;
                int colGainOnSaleOfAssetActivity = COL;
                COL++;

                sheet[ROW, COL].Text = "Loss On Sale Asset GL";
                sheet[ROW, COL].ColumnWidth = 15;
                int colLossOnSaleOfAssetGL = COL;
                COL++;
                sheet[ROW, COL].Text = "Loss On Sale Asset Budget";
                sheet[ROW, COL].ColumnWidth = 15;
                int colLossOnSaleOfAssetBudget = COL;
                COL++;
                sheet[ROW, COL].Text = "Loss On Sale Asset Activity";
                sheet[ROW, COL].ColumnWidth = 15;
                int colLossOnSaleOfAssetActivity = COL;
                COL++;

                sheet[ROW, COL].Text = "Loss On Disposal Asset GL";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                 int colLossOnDisposalAssetGL = COL;
                COL++;
                sheet[ROW, COL].Text = "Loss On Disposal Asset Budget";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colLossOnDisposalAssetBudget = COL;
                COL++;
                sheet[ROW, COL].Text = "Loss On Disposal Asset Activity";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 15;
                int colLossOnDisposalAssetActivity = COL;
                COL++;
                sheet[ROW, COL].Text = "Less Value Asset GL";
                sheet[ROW, COL].ColumnWidth = 15;
                int colLessValueAssetGL = COL;
                COL++;
                sheet[ROW, COL].Text = "Less Value Asset Budget";
                sheet[ROW, COL].ColumnWidth = 15;
                int colLessValueAssetBudget = COL;
                COL++;
                sheet[ROW, COL].Text = "Less Value Asset Activity";
                sheet[ROW, COL].ColumnWidth = 15;
                int colLessValueAssetActivity = COL;
                COL++;
                sheet[ROW, COL].Text = "COA";
                sheet[ROW, COL].ColumnWidth = 15;
                int colCOA = COL;

                Dictionary<string, int> DicColIndex = new Dictionary<string, int>();
                bool GLColumnFound = false;
                for (int i = 0; i < dtFixedAssetMasterGL.Columns.Count - 1; i++)
                {

                    if (dtFixedAssetMasterGL.Columns[i].ColumnName.ToString().ToUpper() == "COA")
                        GLColumnFound = true;

                    if (GLColumnFound == false)
                        continue;

                    COL++;
                    sheet[ROW, COL].Text = dtFixedAssetMasterGL.Columns[i + 1].ColumnName;
                    sheet[ROW, COL].ColumnWidth = 15;
                    DicColIndex.Add(dtFixedAssetMasterGL.Columns[i + 1].ColumnName, COL);

                    COL++;
                    sheet[ROW, COL].Text = dtFixedAssetMasterGL.Columns[i + 1].ColumnName.Replace("GL", "Budget");
                    sheet[ROW, COL].ColumnWidth = 15;
                    DicColIndex.Add(dtFixedAssetMasterGL.Columns[i + 1].ColumnName.Replace("GL", "Budget"), COL);

                    COL++;
                    sheet[ROW, COL].Text = dtFixedAssetMasterGL.Columns[i + 1].ColumnName.Replace("GL", "Activity");
                    sheet[ROW, COL].ColumnWidth = 15;
                    DicColIndex.Add(dtFixedAssetMasterGL.Columns[i + 1].ColumnName.Replace("GL", "Activity"), COL);
                }

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                int StartRow = ROW; //row 20
                for (int i = 0; i < dtFixedAssetMasterGL.Rows.Count; i++)
                {
                    sheet[ROW, colSlNo].Number = (i + 1);

                    sheet[ROW, colFixedAssetMaster].Text = dtFixedAssetMasterGL.Rows[i]["FixedAssetMasterName"].ToString();
                    sheet[ROW, colAccDepGL].Text = dtFixedAssetMasterGL.Rows[i]["AccDepreciationGLInfo"].ToString();
                    sheet[ROW, colDepreciationGL].Text = dtFixedAssetMasterGL.Rows[i]["DepreciationGLInfo"].ToString();
                    sheet[ROW, colAUCGL].Text = dtFixedAssetMasterGL.Rows[i]["AUCGLInfo"].ToString();
                    sheet[ROW, colDownPaymentGL].Text = dtFixedAssetMasterGL.Rows[i]["DownPaymentGLInfo"].ToString();
                    sheet[ROW, colClearingAccountGL].Text = dtFixedAssetMasterGL.Rows[i]["ClearingAccountGLInfo"].ToString();
                    sheet[ROW, colGainOnSaleOfAssetGL].Text = dtFixedAssetMasterGL.Rows[i]["GainOnSaleOfAssetGLInfo"].ToString();
                    sheet[ROW, colLossOnSaleOfAssetGL].Text = dtFixedAssetMasterGL.Rows[i]["LossOnSaleOfAssetGLInfo"].ToString();
                    sheet[ROW, colLossOnDisposalAssetGL].Text = dtFixedAssetMasterGL.Rows[i]["LossOnDisposalAssetGLInfo"].ToString();
                    sheet[ROW, colLessValueAssetGL].Text = dtFixedAssetMasterGL.Rows[i]["LessValueAssetGLInfo"].ToString();
                    sheet[ROW, colCOA].Text = dtFixedAssetMasterGL.Rows[i]["COA"].ToString();
                  

                    foreach (var item in DicColIndex)
                    {
                        if (dtFixedAssetMasterGL.Columns.Contains(item.Key))
                            sheet[ROW, item.Value].Text = dtFixedAssetMasterGL.Rows[i][item.Key].ToString();
                    }

                    dtFixedAssetMasterBudget.DefaultView.RowFilter = "FixedAssetMasterId='" + dtFixedAssetMasterGL.Rows[i]["FixedAssetMasterId"].ToString() + @"'";
                    if (dtFixedAssetMasterBudget.DefaultView.Count > 0)
                    {

                        sheet[ROW, colAccDepBudget].Text = dtFixedAssetMasterBudget.Rows[i]["AccumulatedDepreciationBudgetName"].ToString();
                        sheet[ROW, colDepreciationBudget].Text = dtFixedAssetMasterBudget.Rows[i]["DepreciationBudgetName"].ToString();
                        sheet[ROW, colAUCBudget].Text = dtFixedAssetMasterBudget.Rows[i]["AssetUnderConstructionBudgetName"].ToString();
                        sheet[ROW, colDownPaymentBudget].Text = dtFixedAssetMasterBudget.Rows[i]["DownPaymentBudgetName"].ToString();
                        sheet[ROW, colClearingAccountBudget].Text = dtFixedAssetMasterBudget.Rows[i]["ClearingAccountBudgetName"].ToString();
                        sheet[ROW, colGainOnSaleOfAssetBudget].Text = dtFixedAssetMasterBudget.Rows[i]["GainOnSaleOfAssetBudgetName"].ToString();
                        sheet[ROW, colLossOnSaleOfAssetBudget].Text = dtFixedAssetMasterBudget.Rows[i]["LossOnSaleOfAssetBudgetName"].ToString();
                        sheet[ROW, colLossOnDisposalAssetBudget].Text = dtFixedAssetMasterBudget.Rows[i]["LossOnDisposalAssetBudgetName"].ToString();
                        sheet[ROW, colLessValueAssetBudget].Text = dtFixedAssetMasterBudget.Rows[i]["LessValueAssetBudgetName"].ToString();


                        foreach (var item in DicColIndex)
                        {
                            if (dtFixedAssetMasterBudget.Columns.Contains(item.Key))
                                sheet[ROW, item.Value].Text = dtFixedAssetMasterBudget.DefaultView[0][item.Key].ToString();
                        }

                    }
                    dtFixedAssetMasterActivity.DefaultView.RowFilter = "FixedAssetMasterId='" + dtFixedAssetMasterGL.Rows[i]["FixedAssetMasterId"].ToString() + @"'";

                    if (dtFixedAssetMasterActivity.DefaultView.Count > 0)
                    {
                        sheet[ROW, colAccDepActivity].Text = dtFixedAssetMasterActivity.Rows[i]["AccumulatedDepreciationActivityName"].ToString();
                        sheet[ROW, colDepreciationActivity].Text = dtFixedAssetMasterActivity.Rows[i]["DepreciationActivityName"].ToString();
                        sheet[ROW, colAUCActivity].Text = dtFixedAssetMasterActivity.Rows[i]["AssetUnderConstructionActivityName"].ToString();
                        sheet[ROW, colDownPaymentActivity].Text = dtFixedAssetMasterActivity.Rows[i]["DownPaymentActivityName"].ToString();
                        sheet[ROW, colClearingAccountActivity].Text = dtFixedAssetMasterActivity.Rows[i]["ClearingAccountActivityNam"].ToString();
                        sheet[ROW, colGainOnSaleOfAssetActivity].Text = dtFixedAssetMasterActivity.Rows[i]["GainOnSaleOfAssetActivityName"].ToString();
                        sheet[ROW, colLossOnSaleOfAssetActivity].Text = dtFixedAssetMasterActivity.Rows[i]["LossOnSaleOfAssetActivityName"].ToString();
                        sheet[ROW, colLossOnDisposalAssetActivity].Text = dtFixedAssetMasterActivity.Rows[i]["LossOnDisposalAssetActivityName"].ToString();
                        sheet[ROW, colLessValueAssetActivity].Text = dtFixedAssetMasterActivity.Rows[i]["LessValueAssetActivityName"].ToString();

                        foreach (var item in DicColIndex)
                        {
                            if (dtFixedAssetMasterActivity.Columns.Contains(item.Key))
                                sheet[ROW, item.Value].Text = dtFixedAssetMasterActivity.DefaultView[0][item.Key].ToString();
                        }
                    }

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }

                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();

                sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colSlNo, ROW, colSlNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyGroupHeader(ref sheet, endCol, "Fixed Asset Report", identity.CompanyGroupId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "Fixed Asset GL Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }




    }
}