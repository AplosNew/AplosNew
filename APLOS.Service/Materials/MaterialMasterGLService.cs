using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.Materials
{
    public class MaterialMasterGLService : Service<MaterialMasterGL>, IMaterialMasterGLService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaterialMasterVendorReconGLService _fixedAssetVendorReconGLService;

        public MaterialMasterGLService(
              IRepositoryAsync<MaterialMasterGL> FixedAssetClassRepository
            , IPKGeneratorService pkGeneratorService
            , IMaterialMasterVendorReconGLService fixedAssetVendorReconGLService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(FixedAssetClassRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _fixedAssetVendorReconGLService = fixedAssetVendorReconGLService;
        }

        public void InsertOrUpdate(string masterId, MaterialMasterGL entity, IEnumerable<MaterialMasterVendorReconGL> fixedAssetVendorReconGL)
        {
            try
            {
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPK();
                    entity.MaterialMasterId = masterId;
                    InsertGraph(entity);
                }
                else
                {
                    UpdateGraph(entity);
                }
                IEnumerable<MaterialMasterGL> list = new List<MaterialMasterGL> { entity };
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
            return GetAutoNumber(nameof(MaterialMasterGL), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(MaterialMasterGL), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        //Using on FixedAssetMaster
        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "MaterialMasterGL Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(id);
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

        public void InsertUpdateMaterialMasterGL(IEnumerable<MaterialMasterGL> entities, IEnumerable<MaterialMasterVendorReconGL> fixedAssetVendorReconGL)
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
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
                                 FROM [HKP].[MaterialMasterGL] FAD
                                 LEFT OUTER JOIN [MST].[FixedAssetMaster] FAM ON FAD.FixedAssetMasterId = FAD.Id
							  LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=FAD.MaterialMasterGLId
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
                    parameters.CmdText = @"SELECT FADV.MaterialMasterGLId,FADV.PartyAccountGroupId,FADV.VendorReconGLId,GI.AccountCode as VendorReconGLCode,GI.UserName AS VendorRecontGLText FROM [HKP].[MaterialMasterVendorReconGL] AS FADV
                                            LEFT OUTER JOIN HKP.GLGeneralInfo AS GI ON FADV.VendorReconGLId = GI.Id
                                            LEFT OUTER JOIN [HKP].[MaterialMasterGL] AS FAD ON FADV.MaterialMasterGLId =FAD.Id
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

        public GridModel GetSearchWithCombine(GridParameter parameters, string coaId, string materialMasterIds, string fixedAssetMasterIds)
        {
            try
            {
                string search = null;

                if (materialMasterIds != "''")
                {
                    search += " AND FAM.Id IN(" + materialMasterIds + ")";
                }

                if (fixedAssetMasterIds != "''")
                {
                    search += " AND FAM.AssetMasterId IN(" + fixedAssetMasterIds + ")";
                }
                string coaStr = "where isnull(Id,'') =''";
                if (coaId != "null")
                    coaStr = "where isnull(Id,'') ='" + coaId + @"'";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAD.Id
                                ,FAM.Id AS MaterialMasterId
                                ,FAM.UserName AS MaterialMasterName
                                ,FAM.AssetMasterId FixedAssetMasterId
								 ,FAMT.UserName AS FixedAssetMasterName
                                ,FAD.AssetGLId
                                ,FAD.AccumulatedDepreciationGLId
                                ,FAD.AssetUnderConstructionGLId
                                ,FAD.DepreciationGLId
                                ,FAD.DownPaymentGLId
                                ,FAD.ClearingAccountGLId
                                ,FAD.GainOnSaleOfAssetGLId
                                ,FAD.LossOnSaleOfAssetGLId
                                ,FAD.LossOnDisposalAssetGLId
                                ,FAD.LessValueAssetGLId
                                ,c.Id AS COAId,GLGI1.AccountCode
                                ,C.UserName
                                ,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS AssetGLInfo
                                ,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS AccDepreciationGLInfo
                                ,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS DepreciationGLInfo
                                ,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS AUCGLInfo
                                ,GLGI5.AccountCode + ' - ' + GLGI5.UserName AS DownPaymentGLInfo
                                ,GLGI6.AccountCode + ' - ' + GLGI6.UserName AS ClearingAccountGLInfo
                                ,GLGI7.AccountCode + ' - ' + GLGI7.UserName AS GainOnSaleOfAssetGLInfo
                                ,GLGI8.AccountCode + ' - ' + GLGI8.UserName AS LossOnSaleOfAssetGLInfo
                                ,GLGI9.AccountCode + ' - ' + GLGI9.UserName AS LossOnDisposalAssetGLInfo
                                ,GLGI10.AccountCode + ' - ' + GLGI10.UserName AS LessValueAssetGLInfo
                                ,FAD.AssetBudgetId
                                ,FAD.AssetActivityId
                                ,AssetBudget.UserName AS AssetBudgetName
                                ,AssetActivity.UserName AS AssetActivityName
                                ,FAD.AccumulatedDepreciationBudgetId
                                ,FAD.AccumulatedDepreciationActivityId
                                ,ADBudget.UserName AS   AccumulatedDepreciationBudgetName
                                ,ADActivity.UserName AS AccumulatedDepreciationActivityName
                                ,FAD.DepreciationBudgetId,FAD.DepreciationActivityId
                                ,DEPBudget.UserName AS   DepreciationBudgetName
                                ,DEPActivity.UserName AS DepreciationActivityName
                                ,FAD.AssetUnderConstructionBudgetId
                                ,FAD.AssetUnderConstructionActivityId
                                ,AUCBudget.UserName AS   AssetUnderConstructionBudgetName
                                ,AUCActivity.UserName AS AssetUnderConstructionActivityName
                                ,FAD.DownPaymentBudgetId
                                ,FAD.DownPaymentActivityId
                                ,DPBudget.UserName AS   DownPaymentBudgetName
                                ,DPActivity.UserName AS DownPaymentActivityName
                                ,FAD.ClearingAccountBudgetId
                                ,FAD.ClearingAccountActivityId
                                ,CABudget.UserName AS   ClearingAccountBudgetName
                                ,CAActivity.UserName AS ClearingAccountActivityName
                                ,FAD.GainOnSaleOfAssetBudgetId
                                ,FAD.GainOnSaleOfAssetActivityId
                                ,GOSBudget.UserName AS   GainOnSaleOfAssetBudgetName
                                ,GOSActivity.UserName AS GainOnSaleOfAssetActivityName
                                ,FAD.LossOnSaleOfAssetBudgetId
                                ,FAD.LossOnSaleOfAssetActivityId
                                ,LOSBudget.UserName AS   LossOnSaleOfAssetBudgetName
                                ,LOSActivity.UserName AS LossOnSaleOfAssetActivityName
                                ,FAD.LossOnDisposalAssetBudgetId
                                ,FAD.LossOnDisposalAssetActivityId
                                ,LODBudget.UserName AS   LossOnDisposalAssetBudgetName
                                ,LODActivity.UserName AS LossOnDisposalAssetActivityName
								,FAD.LessValueAssetBudgetId
								,FAD.LessValueAssetActivityId
                                ,LEVBudget.UserName AS   LessValueAssetBudgetName
                                ,LEActivity.UserName AS LessValueAssetActivityName
                                FROM MST.MaterialMaster As FAM
								LEFT OUTER JOIN MST.FixedAssetMaster AS FAMT ON FAM.AssetMasterId=FAMT.Id
                                LEFT JOIN HKP.MaterialMasterGL AS FAD  ON FAD.MaterialMasterId=FAM.Id
                                LEFT JOIN(SELECT Id, UserName from HKP.COA where isnull(Id,'') ='CA1') C ON FAD.COAId=C.Id
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=FAD.AssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=FAD.DepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FAD.AssetUnderConstructionGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI5 ON GLGI5.Id=FAD.DownPaymentGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI6 ON GLGI6.Id=FAD.ClearingAccountGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI7 ON GLGI7.Id=FAD.GainOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI8 ON GLGI8.Id=FAD.LossOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI9 ON GLGI9.Id=FAD.LossOnDisposalAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI10 ON GLGI10.Id=FAD.LessValueAssetGLId
                                LEFT JOIN HKP.Budget AS AssetBudget ON FAD.AssetBudgetId = AssetBudget.Id
                                LEFT JOIN HKP.Activity AS AssetActivity ON FAD.AssetActivityId = AssetActivity.Id
                                LEFT JOIN HKP.Budget AS ADBudget ON FAD.AccumulatedDepreciationBudgetId = ADBudget.Id
                                LEFT JOIN HKP.Activity AS ADActivity ON FAD.AccumulatedDepreciationActivityId = ADActivity.Id
                                LEFT JOIN HKP.Budget AS   DEPBudget ON     FAD.DepreciationBudgetId =   DEPBudget.Id
                                LEFT JOIN HKP.Activity AS DEPActivity ON FAD.DepreciationActivityId = DEPActivity.Id
                                LEFT JOIN HKP.Budget AS   AUCBudget ON   FAD.AssetUnderConstructionBudgetId =   AUCBudget.Id
                                LEFT JOIN HKP.Activity AS AUCActivity ON FAD.AssetUnderConstructionActivityId = AUCActivity.Id
                                LEFT JOIN HKP.Budget AS   DPBudget ON   FAD.DownPaymentBudgetId =   DPBudget.Id
                                LEFT JOIN HKP.Activity AS DPActivity ON FAD.DownPaymentActivityId = DPActivity.Id
                                LEFT JOIN HKP.Budget AS   CABudget ON   FAD.ClearingAccountBudgetId =   CABudget.Id
                                LEFT JOIN HKP.Activity AS CAActivity ON FAD.ClearingAccountActivityId = CAActivity.Id
                                LEFT JOIN HKP.Budget AS   GOSBudget ON   FAD.GainOnSaleOfAssetBudgetId =   GOSBudget.Id
                                LEFT JOIN HKP.Activity AS GOSActivity ON FAD.GainOnSaleOfAssetActivityId = GOSActivity.Id
                                LEFT JOIN HKP.Budget AS   LOSBudget ON   FAD.LossOnSaleOfAssetBudgetId =   LOSBudget.Id
                                LEFT JOIN HKP.Activity AS LOSActivity ON FAD.LossOnSaleOfAssetActivityId = LOSActivity.Id
                                LEFT JOIN HKP.Budget AS   LODBudget ON   FAD.LossOnDisposalAssetBudgetId =   LODBudget.Id
                                LEFT JOIN HKP.Activity AS LODActivity ON FAD.LossOnDisposalAssetActivityId = LODActivity.Id
                                LEFT JOIN HKP.Budget AS   LEVBudget ON   FAD.LessValueAssetBudgetId =   LEVBudget.Id
                                LEFT JOIN HKP.Activity AS LEActivity ON FAD.LessValueAssetActivityId = LEActivity.Id WHERE FAM.IsAsset='1'" + search + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetSearchWithCombineWithAssing(GridParameter parameters, string coaId, string materialMasterIds, string fixedAssetMasterIds)
        {
            try
            {
                string search = null;

                if (materialMasterIds != "''")
                {
                    search += " And  FAM.Id IN(" + materialMasterIds + ")";
                }

                if (fixedAssetMasterIds != "''")
                {
                    search += "  And FAM.AssetMasterId IN(" + fixedAssetMasterIds + ")";
                }
                string coaStr = " ";
                if (coaId != "null")
                    coaStr += "where isnull(Id,'') ='" + coaId + @"'";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAD.Id
                                ,FAM.Id AS MaterialMasterId
                                ,FAM.UserName AS MaterialMasterName
                                ,FAM.AssetMasterId FixedAssetMasterId
								 ,FAMT.UserName AS FixedAssetMasterName
                                ,FAD.AssetGLId
                                ,FAD.AccumulatedDepreciationGLId
                                ,FAD.AssetUnderConstructionGLId
                                ,FAD.DepreciationGLId
                                ,FAD.DownPaymentGLId
                                ,FAD.ClearingAccountGLId
                                ,FAD.GainOnSaleOfAssetGLId
                                ,FAD.LossOnSaleOfAssetGLId
                                ,FAD.LossOnDisposalAssetGLId
                                ,FAD.LessValueAssetGLId
                                ,c.Id AS COAId,GLGI1.AccountCode
                                ,C.UserName
                                ,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS AssetGLInfo
                                ,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS AccDepreciationGLInfo
                                ,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS DepreciationGLInfo
                                ,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS AUCGLInfo
                                ,GLGI5.AccountCode + ' - ' + GLGI5.UserName AS DownPaymentGLInfo
                                ,GLGI6.AccountCode + ' - ' + GLGI6.UserName AS ClearingAccountGLInfo
                                ,GLGI7.AccountCode + ' - ' + GLGI7.UserName AS GainOnSaleOfAssetGLInfo
                                ,GLGI8.AccountCode + ' - ' + GLGI8.UserName AS LossOnSaleOfAssetGLInfo
                                ,GLGI9.AccountCode + ' - ' + GLGI9.UserName AS LossOnDisposalAssetGLInfo
                                ,GLGI10.AccountCode + ' - ' + GLGI10.UserName AS LessValueAssetGLInfo
                                ,FAD.AssetBudgetId
                                ,FAD.AssetActivityId
                                ,AssetBudget.UserName AS AssetBudgetName
                                ,AssetActivity.UserName AS AssetActivityName
                                ,FAD.AccumulatedDepreciationBudgetId
                                ,FAD.AccumulatedDepreciationActivityId
                                ,ADBudget.UserName AS   AccumulatedDepreciationBudgetName
                                ,ADActivity.UserName AS AccumulatedDepreciationActivityName
                                ,FAD.DepreciationBudgetId,FAD.DepreciationActivityId
                                ,DEPBudget.UserName AS   DepreciationBudgetName
                                ,DEPActivity.UserName AS DepreciationActivityName
                                ,FAD.AssetUnderConstructionBudgetId
                                ,FAD.AssetUnderConstructionActivityId
                                ,AUCBudget.UserName AS   AssetUnderConstructionBudgetName
                                ,AUCActivity.UserName AS AssetUnderConstructionActivityName
                                ,FAD.DownPaymentBudgetId
                                ,FAD.DownPaymentActivityId
                                ,DPBudget.UserName AS   DownPaymentBudgetName
                                ,DPActivity.UserName AS DownPaymentActivityName
                                ,FAD.ClearingAccountBudgetId
                                ,FAD.ClearingAccountActivityId
                                ,CABudget.UserName AS   ClearingAccountBudgetName
                                ,CAActivity.UserName AS ClearingAccountActivityName
                                ,FAD.GainOnSaleOfAssetBudgetId
                                ,FAD.GainOnSaleOfAssetActivityId
                                ,GOSBudget.UserName AS   GainOnSaleOfAssetBudgetName
                                ,GOSActivity.UserName AS GainOnSaleOfAssetActivityName
                                ,FAD.LossOnSaleOfAssetBudgetId
                                ,FAD.LossOnSaleOfAssetActivityId
                                ,LOSBudget.UserName AS   LossOnSaleOfAssetBudgetName
                                ,LOSActivity.UserName AS LossOnSaleOfAssetActivityName
                                ,FAD.LossOnDisposalAssetBudgetId
                                ,FAD.LossOnDisposalAssetActivityId
                                ,LODBudget.UserName AS   LossOnDisposalAssetBudgetName
                                ,LODActivity.UserName AS LossOnDisposalAssetActivityName
								,FAD.LessValueAssetBudgetId
								,FAD.LessValueAssetActivityId
                                ,LEVBudget.UserName AS   LessValueAssetBudgetName
                                ,LEActivity.UserName AS LessValueAssetActivityName
                                FROM MST.MaterialMaster As FAM
								LEFT OUTER JOIN MST.FixedAssetMaster AS FAMT ON FAM.AssetMasterId=FAMT.Id
                                LEFT JOIN HKP.MaterialMasterGL AS FAD  ON FAD.MaterialMasterId=FAM.Id
                                LEFT JOIN(SELECT Id, UserName from HKP.COA where isnull(Id,'') ='CA1') C ON FAD.COAId=C.Id
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=FAD.AssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=FAD.DepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FAD.AssetUnderConstructionGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI5 ON GLGI5.Id=FAD.DownPaymentGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI6 ON GLGI6.Id=FAD.ClearingAccountGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI7 ON GLGI7.Id=FAD.GainOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI8 ON GLGI8.Id=FAD.LossOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI9 ON GLGI9.Id=FAD.LossOnDisposalAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI10 ON GLGI10.Id=FAD.LessValueAssetGLId
                                LEFT JOIN HKP.Budget AS AssetBudget ON FAD.AssetBudgetId = AssetBudget.Id
                                LEFT JOIN HKP.Activity AS AssetActivity ON FAD.AssetActivityId = AssetActivity.Id
                                LEFT JOIN HKP.Budget AS ADBudget ON FAD.AccumulatedDepreciationBudgetId = ADBudget.Id
                                LEFT JOIN HKP.Activity AS ADActivity ON FAD.AccumulatedDepreciationActivityId = ADActivity.Id
                                LEFT JOIN HKP.Budget AS   DEPBudget ON     FAD.DepreciationBudgetId =   DEPBudget.Id
                                LEFT JOIN HKP.Activity AS DEPActivity ON FAD.DepreciationActivityId = DEPActivity.Id
                                LEFT JOIN HKP.Budget AS   AUCBudget ON   FAD.AssetUnderConstructionBudgetId =   AUCBudget.Id
                                LEFT JOIN HKP.Activity AS AUCActivity ON FAD.AssetUnderConstructionActivityId = AUCActivity.Id
                                LEFT JOIN HKP.Budget AS   DPBudget ON   FAD.DownPaymentBudgetId =   DPBudget.Id
                                LEFT JOIN HKP.Activity AS DPActivity ON FAD.DownPaymentActivityId = DPActivity.Id
                                LEFT JOIN HKP.Budget AS   CABudget ON   FAD.ClearingAccountBudgetId =   CABudget.Id
                                LEFT JOIN HKP.Activity AS CAActivity ON FAD.ClearingAccountActivityId = CAActivity.Id
                                LEFT JOIN HKP.Budget AS   GOSBudget ON   FAD.GainOnSaleOfAssetBudgetId =   GOSBudget.Id
                                LEFT JOIN HKP.Activity AS GOSActivity ON FAD.GainOnSaleOfAssetActivityId = GOSActivity.Id
                                LEFT JOIN HKP.Budget AS   LOSBudget ON   FAD.LossOnSaleOfAssetBudgetId =   LOSBudget.Id
                                LEFT JOIN HKP.Activity AS LOSActivity ON FAD.LossOnSaleOfAssetActivityId = LOSActivity.Id
                                LEFT JOIN HKP.Budget AS   LODBudget ON   FAD.LossOnDisposalAssetBudgetId =   LODBudget.Id
                                LEFT JOIN HKP.Activity AS LODActivity ON FAD.LossOnDisposalAssetActivityId = LODActivity.Id
                                LEFT JOIN HKP.Budget AS   LEVBudget ON   FAD.LessValueAssetBudgetId =   LEVBudget.Id
                                LEFT JOIN HKP.Activity AS LEActivity ON FAD.LessValueAssetActivityId = LEActivity.Id
                                WHERE FAM.IsAsset='1' AND FAD.AssetGLId <> ''
                                AND FAD.AccumulatedDepreciationGLId <> '' AND FAD.DepreciationGLId <> ''
                                AND FAD.DownPaymentGLId <> '' AND FAD.ClearingAccountGLId <> ''
                                AND FAD.GainOnSaleOfAssetGLId <> '' AND FAD.LossOnSaleOfAssetGLId <> ''
                                AND FAD.LossOnDisposalAssetGLId <> '' " + search + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetSearchWithCombineWithNotAssing(GridParameter parameters, string coaId, string materialMasterIds, string fixedAssetMasterIds)
        {
            try
            {
                string search = null;

                if (materialMasterIds != "''")
                {
                    search += " And  FAM.Id IN(" + materialMasterIds + ")";
                }

                if (fixedAssetMasterIds != "''")
                {
                    search += "  And FAM.AssetMasterId IN(" + fixedAssetMasterIds + ")";
                }
                string coaStr = " ";
                if (coaId != "null")
                    coaStr += "where isnull(Id,'') ='" + coaId + @"'";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAD.Id
                                ,FAM.Id AS MaterialMasterId
                                ,FAM.UserName AS MaterialMasterName
                                ,FAM.AssetMasterId FixedAssetMasterId
								 ,FAMT.UserName AS FixedAssetMasterName
                                ,FAD.AssetGLId
                                ,FAD.AccumulatedDepreciationGLId
                                ,FAD.AssetUnderConstructionGLId
                                ,FAD.DepreciationGLId
                                ,FAD.DownPaymentGLId
                                ,FAD.ClearingAccountGLId
                                ,FAD.GainOnSaleOfAssetGLId
                                ,FAD.LossOnSaleOfAssetGLId
                                ,FAD.LossOnDisposalAssetGLId
                                ,FAD.LessValueAssetGLId
                                ,c.Id AS COAId,GLGI1.AccountCode
                                ,C.UserName
                                ,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS AssetGLInfo
                                ,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS AccDepreciationGLInfo
                                ,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS DepreciationGLInfo
                                ,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS AUCGLInfo
                                ,GLGI5.AccountCode + ' - ' + GLGI5.UserName AS DownPaymentGLInfo
                                ,GLGI6.AccountCode + ' - ' + GLGI6.UserName AS ClearingAccountGLInfo
                                ,GLGI7.AccountCode + ' - ' + GLGI7.UserName AS GainOnSaleOfAssetGLInfo
                                ,GLGI8.AccountCode + ' - ' + GLGI8.UserName AS LossOnSaleOfAssetGLInfo
                                ,GLGI9.AccountCode + ' - ' + GLGI9.UserName AS LossOnDisposalAssetGLInfo
                                ,GLGI10.AccountCode + ' - ' + GLGI10.UserName AS LessValueAssetGLInfo
                                ,FAD.AssetBudgetId
                                ,FAD.AssetActivityId
                                ,AssetBudget.UserName AS AssetBudgetName
                                ,AssetActivity.UserName AS AssetActivityName
                                ,FAD.AccumulatedDepreciationBudgetId
                                ,FAD.AccumulatedDepreciationActivityId
                                ,ADBudget.UserName AS   AccumulatedDepreciationBudgetName
                                ,ADActivity.UserName AS AccumulatedDepreciationActivityName
                                ,FAD.DepreciationBudgetId,FAD.DepreciationActivityId
                                ,DEPBudget.UserName AS   DepreciationBudgetName
                                ,DEPActivity.UserName AS DepreciationActivityName
                                ,FAD.AssetUnderConstructionBudgetId
                                ,FAD.AssetUnderConstructionActivityId
                                ,AUCBudget.UserName AS   AssetUnderConstructionBudgetName
                                ,AUCActivity.UserName AS AssetUnderConstructionActivityName
                                ,FAD.DownPaymentBudgetId
                                ,FAD.DownPaymentActivityId
                                ,DPBudget.UserName AS   DownPaymentBudgetName
                                ,DPActivity.UserName AS DownPaymentActivityName
                                ,FAD.ClearingAccountBudgetId
                                ,FAD.ClearingAccountActivityId
                                ,CABudget.UserName AS   ClearingAccountBudgetName
                                ,CAActivity.UserName AS ClearingAccountActivityName
                                ,FAD.GainOnSaleOfAssetBudgetId
                                ,FAD.GainOnSaleOfAssetActivityId
                                ,GOSBudget.UserName AS   GainOnSaleOfAssetBudgetName
                                ,GOSActivity.UserName AS GainOnSaleOfAssetActivityName
                                ,FAD.LossOnSaleOfAssetBudgetId
                                ,FAD.LossOnSaleOfAssetActivityId
                                ,LOSBudget.UserName AS   LossOnSaleOfAssetBudgetName
                                ,LOSActivity.UserName AS LossOnSaleOfAssetActivityName
                                ,FAD.LossOnDisposalAssetBudgetId
                                ,FAD.LossOnDisposalAssetActivityId
                                ,LODBudget.UserName AS   LossOnDisposalAssetBudgetName
                                ,LODActivity.UserName AS LossOnDisposalAssetActivityName
								,FAD.LessValueAssetBudgetId
								,FAD.LessValueAssetActivityId
                                ,LEVBudget.UserName AS   LessValueAssetBudgetName
                                ,LEActivity.UserName AS LessValueAssetActivityName
                                FROM MST.MaterialMaster As FAM
								LEFT OUTER JOIN MST.FixedAssetMaster AS FAMT ON FAM.AssetMasterId=FAMT.Id
                                LEFT JOIN HKP.MaterialMasterGL AS FAD  ON FAD.MaterialMasterId=FAM.Id
                                LEFT JOIN(SELECT Id, UserName from HKP.COA where isnull(Id,'') ='CA1') C ON FAD.COAId=C.Id
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=FAD.AssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=FAD.DepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FAD.AssetUnderConstructionGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI5 ON GLGI5.Id=FAD.DownPaymentGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI6 ON GLGI6.Id=FAD.ClearingAccountGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI7 ON GLGI7.Id=FAD.GainOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI8 ON GLGI8.Id=FAD.LossOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI9 ON GLGI9.Id=FAD.LossOnDisposalAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI10 ON GLGI10.Id=FAD.LessValueAssetGLId
                                LEFT JOIN HKP.Budget AS AssetBudget ON FAD.AssetBudgetId = AssetBudget.Id
                                LEFT JOIN HKP.Activity AS AssetActivity ON FAD.AssetActivityId = AssetActivity.Id
                                LEFT JOIN HKP.Budget AS ADBudget ON FAD.AccumulatedDepreciationBudgetId = ADBudget.Id
                                LEFT JOIN HKP.Activity AS ADActivity ON FAD.AccumulatedDepreciationActivityId = ADActivity.Id
                                LEFT JOIN HKP.Budget AS   DEPBudget ON     FAD.DepreciationBudgetId =   DEPBudget.Id
                                LEFT JOIN HKP.Activity AS DEPActivity ON FAD.DepreciationActivityId = DEPActivity.Id
                                LEFT JOIN HKP.Budget AS   AUCBudget ON   FAD.AssetUnderConstructionBudgetId =   AUCBudget.Id
                                LEFT JOIN HKP.Activity AS AUCActivity ON FAD.AssetUnderConstructionActivityId = AUCActivity.Id
                                LEFT JOIN HKP.Budget AS   DPBudget ON   FAD.DownPaymentBudgetId =   DPBudget.Id
                                LEFT JOIN HKP.Activity AS DPActivity ON FAD.DownPaymentActivityId = DPActivity.Id
                                LEFT JOIN HKP.Budget AS   CABudget ON   FAD.ClearingAccountBudgetId =   CABudget.Id
                                LEFT JOIN HKP.Activity AS CAActivity ON FAD.ClearingAccountActivityId = CAActivity.Id
                                LEFT JOIN HKP.Budget AS   GOSBudget ON   FAD.GainOnSaleOfAssetBudgetId =   GOSBudget.Id
                                LEFT JOIN HKP.Activity AS GOSActivity ON FAD.GainOnSaleOfAssetActivityId = GOSActivity.Id
                                LEFT JOIN HKP.Budget AS   LOSBudget ON   FAD.LossOnSaleOfAssetBudgetId =   LOSBudget.Id
                                LEFT JOIN HKP.Activity AS LOSActivity ON FAD.LossOnSaleOfAssetActivityId = LOSActivity.Id
                                LEFT JOIN HKP.Budget AS   LODBudget ON   FAD.LossOnDisposalAssetBudgetId =   LODBudget.Id
                                LEFT JOIN HKP.Activity AS LODActivity ON FAD.LossOnDisposalAssetActivityId = LODActivity.Id
                                LEFT JOIN HKP.Budget AS   LEVBudget ON   FAD.LessValueAssetBudgetId =   LEVBudget.Id
                                LEFT JOIN HKP.Activity AS LEActivity ON FAD.LessValueAssetActivityId = LEActivity.Id
                               WHERE FAM.IsAsset='1' AND (isnull(FAD.AssetGLId,'')= ''  OR ISNULL(FAD.AccumulatedDepreciationGLId, '') = ''
                            OR ISNULL(FAD.DepreciationGLId, '') = ''OR ISNULL(FAD.AssetUnderConstructionGLId, '') = ''
                            OR ISNULL(FAD.DownPaymentGLId, '') = '' OR ISNULL(FAD.ClearingAccountGLId, '') = ''
							OR ISNULL(FAD.GainOnSaleOfAssetGLId, '') = '' OR ISNULL(FAD.LossOnSaleOfAssetGLId, '') = ''
							OR ISNULL(FAD.LossOnDisposalAssetGLId, '') = '')" + search + @" ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetSearchWithCombineCoa(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAD.Id
                                ,FAM.Id AS MaterialMasterId
                                ,FAM.UserName AS MaterialMasterName
                                ,FAM.AssetMasterId FixedAssetMasterId
								 ,FAMT.UserName AS FixedAssetMasterName
                                ,FAD.AssetGLId
                                ,FAD.AccumulatedDepreciationGLId
                                ,FAD.AssetUnderConstructionGLId
                                ,FAD.DepreciationGLId
                                ,FAD.DownPaymentGLId
                                ,FAD.ClearingAccountGLId
                                ,FAD.GainOnSaleOfAssetGLId
                                ,FAD.LossOnSaleOfAssetGLId
                                ,FAD.LossOnDisposalAssetGLId
                                ,FAD.LessValueAssetGLId
                                ,c.Id AS COAId,GLGI1.AccountCode
                                ,C.UserName
                                ,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS AssetGLInfo
                                ,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS AccDepreciationGLInfo
                                ,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS DepreciationGLInfo
                                ,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS AUCGLInfo
                                ,GLGI5.AccountCode + ' - ' + GLGI5.UserName AS DownPaymentGLInfo
                                ,GLGI6.AccountCode + ' - ' + GLGI6.UserName AS ClearingAccountGLInfo
                                ,GLGI7.AccountCode + ' - ' + GLGI7.UserName AS GainOnSaleOfAssetGLInfo
                                ,GLGI8.AccountCode + ' - ' + GLGI8.UserName AS LossOnSaleOfAssetGLInfo
                                ,GLGI9.AccountCode + ' - ' + GLGI9.UserName AS LossOnDisposalAssetGLInfo
                                ,GLGI10.AccountCode + ' - ' + GLGI10.UserName AS LessValueAssetGLInfo
                                ,FAD.AssetBudgetId
                                ,FAD.AssetActivityId
                                ,AssetBudget.UserName AS AssetBudgetName
                                ,AssetActivity.UserName AS AssetActivityName
                                ,FAD.AccumulatedDepreciationBudgetId
                                ,FAD.AccumulatedDepreciationActivityId
                                ,ADBudget.UserName AS   AccumulatedDepreciationBudgetName
                                ,ADActivity.UserName AS AccumulatedDepreciationActivityName
                                ,FAD.DepreciationBudgetId,FAD.DepreciationActivityId
                                ,DEPBudget.UserName AS   DepreciationBudgetName
                                ,DEPActivity.UserName AS DepreciationActivityName
                                ,FAD.AssetUnderConstructionBudgetId
                                ,FAD.AssetUnderConstructionActivityId
                                ,AUCBudget.UserName AS   AssetUnderConstructionBudgetName
                                ,AUCActivity.UserName AS AssetUnderConstructionActivityName
                                ,FAD.DownPaymentBudgetId
                                ,FAD.DownPaymentActivityId
                                ,DPBudget.UserName AS   DownPaymentBudgetName
                                ,DPActivity.UserName AS DownPaymentActivityName
                                ,FAD.ClearingAccountBudgetId
                                ,FAD.ClearingAccountActivityId
                                ,CABudget.UserName AS   ClearingAccountBudgetName
                                ,CAActivity.UserName AS ClearingAccountActivityName
                                ,FAD.GainOnSaleOfAssetBudgetId
                                ,FAD.GainOnSaleOfAssetActivityId
                                ,GOSBudget.UserName AS   GainOnSaleOfAssetBudgetName
                                ,GOSActivity.UserName AS GainOnSaleOfAssetActivityName
                                ,FAD.LossOnSaleOfAssetBudgetId
                                ,FAD.LossOnSaleOfAssetActivityId
                                ,LOSBudget.UserName AS   LossOnSaleOfAssetBudgetName
                                ,LOSActivity.UserName AS LossOnSaleOfAssetActivityName
                                ,FAD.LossOnDisposalAssetBudgetId
                                ,FAD.LossOnDisposalAssetActivityId
                                ,LODBudget.UserName AS   LossOnDisposalAssetBudgetName
                                ,LODActivity.UserName AS LossOnDisposalAssetActivityName
								,FAD.LessValueAssetBudgetId
								,FAD.LessValueAssetActivityId
                                ,LEVBudget.UserName AS   LessValueAssetBudgetName
                                ,LEActivity.UserName AS LessValueAssetActivityName
                                FROM MST.MaterialMaster As FAM
								LEFT OUTER JOIN MST.FixedAssetMaster AS FAMT ON FAM.AssetMasterId=FAMT.Id
                                LEFT JOIN HKP.MaterialMasterGL AS FAD  ON FAD.MaterialMasterId=FAM.Id
                                LEFT JOIN(SELECT Id, UserName from HKP.COA) C ON FAD.COAId=C.Id
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=FAD.AssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=FAD.DepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FAD.AssetUnderConstructionGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI5 ON GLGI5.Id=FAD.DownPaymentGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI6 ON GLGI6.Id=FAD.ClearingAccountGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI7 ON GLGI7.Id=FAD.GainOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI8 ON GLGI8.Id=FAD.LossOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI9 ON GLGI9.Id=FAD.LossOnDisposalAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI10 ON GLGI10.Id=FAD.LessValueAssetGLId
                                LEFT JOIN HKP.Budget AS AssetBudget ON FAD.AssetBudgetId = AssetBudget.Id
                                LEFT JOIN HKP.Activity AS AssetActivity ON FAD.AssetActivityId = AssetActivity.Id
                                LEFT JOIN HKP.Budget AS ADBudget ON FAD.AccumulatedDepreciationBudgetId = ADBudget.Id
                                LEFT JOIN HKP.Activity AS ADActivity ON FAD.AccumulatedDepreciationActivityId = ADActivity.Id
                                LEFT JOIN HKP.Budget AS   DEPBudget ON     FAD.DepreciationBudgetId =   DEPBudget.Id
                                LEFT JOIN HKP.Activity AS DEPActivity ON FAD.DepreciationActivityId = DEPActivity.Id
                                LEFT JOIN HKP.Budget AS   AUCBudget ON   FAD.AssetUnderConstructionBudgetId =   AUCBudget.Id
                                LEFT JOIN HKP.Activity AS AUCActivity ON FAD.AssetUnderConstructionActivityId = AUCActivity.Id
                                LEFT JOIN HKP.Budget AS   DPBudget ON   FAD.DownPaymentBudgetId =   DPBudget.Id
                                LEFT JOIN HKP.Activity AS DPActivity ON FAD.DownPaymentActivityId = DPActivity.Id
                                LEFT JOIN HKP.Budget AS   CABudget ON   FAD.ClearingAccountBudgetId =   CABudget.Id
                                LEFT JOIN HKP.Activity AS CAActivity ON FAD.ClearingAccountActivityId = CAActivity.Id
                                LEFT JOIN HKP.Budget AS   GOSBudget ON   FAD.GainOnSaleOfAssetBudgetId =   GOSBudget.Id
                                LEFT JOIN HKP.Activity AS GOSActivity ON FAD.GainOnSaleOfAssetActivityId = GOSActivity.Id
                                LEFT JOIN HKP.Budget AS   LOSBudget ON   FAD.LossOnSaleOfAssetBudgetId =   LOSBudget.Id
                                LEFT JOIN HKP.Activity AS LOSActivity ON FAD.LossOnSaleOfAssetActivityId = LOSActivity.Id
                                LEFT JOIN HKP.Budget AS   LODBudget ON   FAD.LossOnDisposalAssetBudgetId =   LODBudget.Id
                                LEFT JOIN HKP.Activity AS LODActivity ON FAD.LossOnDisposalAssetActivityId = LODActivity.Id
                                LEFT JOIN HKP.Budget AS   LEVBudget ON   FAD.LessValueAssetBudgetId =   LEVBudget.Id
                                LEFT JOIN HKP.Activity AS LEActivity ON FAD.LessValueAssetActivityId = LEActivity.Id  WHERE FAM.IsAsset='1'  ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        private string GetOp(string search)
        {
            return string.IsNullOrEmpty(search) ? "WHERE" : "AND";
        }

        public GridModel GetPartyAccountGroup(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT FADVR.Id
                                                ,FADVR.MaterialMasterGLId
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
                                                LEFT OUTER JOIN [HKP].[MaterialMasterVendorReconGL]AS FADVR ON FADVR.PartyAccountGroupId = PAG.Id
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
                parameters.CmdText = @"SELECT FADV.*, GLGI4.AccountCode AS ClearingAccGLCode,GLGI4.UserName AS ClearingAccGLText,B.UserName BudgetName,A.UserName ActivityName FROM  [HKP].[MaterialMasterVendorReconGL] AS FADV
				 LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FADV.VendorReconGLId
				 LEFT OUTER JOIN HKP.Budget B ON FADV.VendorReconBudgetId = B.Id
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

        public IEnumerable<object> GetPartyAccountWithAssignList(string partyAcId, string materialMasterGlId)
        {
            try
            {
                string _sql = @"SELECT F.MaterialMasterGLId,GL.UserName AS GL FROM HKP.MaterialMasterVendorReconGL f
                                LEFT OUTER JOIN HKP.MaterialMasterGL G ON F.MaterialMasterGLId=G.Id
                                LEFT OUTER JOIN HKP.GLGeneralInfo GL ON F.VendorReconGLId=GL.Id
                                WHERE F.PartyAccountGroupId='20175' AND F.MaterialMasterGLId='" + materialMasterGlId + "'";
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
                string _sql = @"select b.MaterialMasterGLId
                                ,a.UserName
                                ,b.VendorReconGLId
                                ,g.AccountCode +'-'+ g.UserName as VendorReconGL
                                ,b.VendorReconBudgetId
                                ,bu.UserName VendorReconBudget
                                ,b.VendorReconActivityId
                                ,ac.UserName VendorReconActivity
                                from hkp.PartyAccountGroup a
                                left outer join [HKP].[MaterialMasterVendorReconGL] b on a.Id=b.PartyAccountGroupId
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
                                set @query = N'SELECT ' + @cols + N', MaterialMasterGLId,PartyAccountGroupId,MaterialMasterId FROM
                                    (
                                 SELECT A.Id,b.MaterialMasterGLId,a.AccountType,B.VendorReconGLId,B.MaterialMasterId, B.PartyAccountGroupId,A.UserName AS ColumnName, C.UserName
                                 FROM  [HKP].[PartyAccountGroup] AS A
                                    LEFT  JOIN [HKP].[MaterialMasterVendorReconGL] AS B ON  A.Id=B.PartyAccountGroupId
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
    }
}