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
using System.Reflection;
using System.Threading;

namespace Library.Service.Materials
{
    public class MaterialGroupGLService : Service<MaterialGroupGL>, IMaterialGroupGLService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaterialGroupPartyAccountGroupGLService _materialGroupPartyAccountGroupGLService;

        public MaterialGroupGLService(
            IRepositoryAsync<MaterialGroupGL> materialGroupGLRepository
            , IMaterialGroupPartyAccountGroupGLService materialGroupPartyAccountGroupGLService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(materialGroupGLRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _materialGroupPartyAccountGroupGLService = materialGroupPartyAccountGroupGLService;
        }

        #endregion Constructor

        public void InsertOrUpdate(string masterId, MaterialGroupGL entity)
        {
            try
            {
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPK();
                    entity.MaterialGroupMasterId = masterId;
                    InsertGraph(entity);
                }
                else
                {
                    UpdateGraph(entity);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(MaterialGroupGL), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private PKGenerator GetMaxNumber()
        {
            return GetMaxNumber(nameof(MaterialGroupGL), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InsertUpdateMaterialGroupDeterminate(IEnumerable<MaterialGroupGL> entities, IEnumerable<MaterialGroupPartyAccountGroupGL> materialGroupVendorReconGL)
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

                _materialGroupPartyAccountGroupGLService.InsertOrUpdate(entities, materialGroupVendorReconGL);

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

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "MaterialGroup Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(id);
                // If section row inactive
                _materialGroupPartyAccountGroupGLService.DeleteGraph(entity.Id);
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

        public GridModel GetDataByMaterialGroupMasterId(GridParameter parameters, string MaterialGroupMasterId, string coaId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (MaterialGroupMasterId != null)
                {
                    parameters.CmdText = @"SELECT MAD.* , GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo, GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
							    , GLGI3.AccountCode + ' - ' + GLGI3.UserName AS InventoryGLInfo, GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo
                                FROM [HKP].[MaterialGroupGL] AS MAD
                                LEFT OUTER JOIN [MST].[MaterialGroupMaster] MGM ON MAD.MaterialGroupMasterId = MAD.Id
							    LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
        			            LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
        			            LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.InventoryGLId
        			            LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.Expense
                                WHERE MAD.MaterialGroupMasterId='" + MaterialGroupMasterId + @"' AND MAD.COAId='" + coaId + @"'  ";
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

        public IEnumerable<object> GetSearchWithCombine(string coaId)
        {
            try
            {
                string sqlText = "";
                var coaStr = "where ISNULL(c.Id,'') =''";
                if (coaId != "null")
                    coaStr = "where ISNULL(c.Id,'') ='" + coaId + @"'";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                sqlText = @"SELECT distinct [CheckBoxSelect] = Convert(bit, 0),  F.Id, MGM.Id AS MaterialGroupMasterId, MGM.UserName AS MaterialGroupMasterName, MG1.UserName AS MaterialGroup1Name, MG2.UserName AS MaterialGroup2Name
                                    , MG3.UserName AS MaterialGroup3Name, MG4.UserName AS MaterialGroup4Name, MT.Description AS MaterialTypeName, MGM.MaterialGroup1Id
        		                    , MGM.MaterialGroup2Id, MGM.MaterialGroup3Id, MGM.MaterialGroup4Id, MGM.MaterialTypeId, F.DownPaymentGLId, F.ClearingAccountGLId, F.COAId
        		                    , F.UserName 'COAName', F.DownPaymentGLInfo, F.ClearingAccountGLInfo, F.DownPaymentBudgetMasterId, F.DownPaymentActivityId, F.DownPaymentBudgetName
					                , F.DownPaymentActivityName, F.ClearingAccountBudgetMasterId, F.ClearingAccountActivityId, F.ClearingAccountBudgetName, F.ClearingAccountActivityName
                                    , F.InventoryGLInfo, F.InventoryGLId, F.InventoryBudgetMasterId, F.InventoryActivityId, F.InventoryBudgetName, F.InventoryActivityName, F.ExpenseGLInfo
                                    , F.ExpenseGLId, F.ExpenseBudgetMasterId, F.ExpenseActivityId, F.ExpenseBudgetName, F.ExpenseActivityName
                                    , F.DebitNoteGLId, F.DebitNoteBudgetMasterId, F.DebitNoteActivityId, F.DebitNoteGLInfo, F.DebitNoteBudgetName, F.DebitNoteActivityName
                                    , F.CreditNoteGLId, F.CreditNoteBudgetMasterId, F.CreditNoteActivityId, F.CreditNoteGLInfo, F.CreditNoteBudgetName, F.CreditNoteActivityName
                                    , F.ShortageGLId, F.ShortageBudgetMasterId, F.ShortageActivityId, F.ShortageGLInfo, F.ShortageBudgetName, F.ShortageActivityName
                                    , F.RejectionGLId, F.RejectionBudgetMasterId, F.RejectionActivityId, F.RejectionGLInfo, F.RejectionBudgetName, F.RejectionActivityName
									,F.InventoryInTransitGLId,F.InventoryInTransitGLInfo,F.InventoryInTransitBudgetName,F.InventoryInTransitBudgetMasterId,F.InventoryInTransitActivityId,F.InventoryInTransitActivityName
                                    FROM MST.MaterialGroupMaster As MGM
                                    LEFT  JOIN HKP.MaterialGroup1 As MG1 ON MG1.Id = MGM.MaterialGroup1Id
                                    LEFT  JOIN HKP.MaterialGroup2 As MG2 ON MG2.Id = MGM.MaterialGroup2Id
                                    LEFT  JOIN HKP.MaterialGroup3 As MG3 ON MG3.Id = MGM.MaterialGroup3Id
                                    LEFT  JOIN HKP.MaterialGroup4 As MG4 ON MG4.Id = MGM.MaterialGroup4Id
                                    LEFT  JOIN HKP.MaterialType As MT ON MT.Id = MGM.MaterialTypeId
                                    LEFT  JOIN (SELECT MAD.Id, MAD.MaterialGroupMasterId, c.Id AS COAId, GLGI1.AccountCode, MAD.DownPaymentGLId, MAD.ClearingAccountGLId,MAD.InventoryInTransitGLId,MAD.InventoryInTransitBudgetMasterId
                                        , MAD.InventoryGLId, MAD.ExpenseGLId, C.UserName, GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo, GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
										, GLGI5.AccountCode + ' - ' + GLGI5.UserName AS InventoryInTransitGLInfo
					                    , GLGI3.AccountCode + ' - ' + GLGI3.UserName AS InventoryGLInfo, GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo, MAD.DownPaymentBudgetMasterId
                                        , MAD.DownPaymentActivityId, DPB.UserName AS DownPaymentBudgetName, DPA.UserName AS DownPaymentActivityName, MAD.ClearingAccountBudgetMasterId
                                        , MAD.ClearingAccountActivityId, MAD.InventoryInTransitActivityId, CAB.UserName AS ClearingAccountBudgetName, CAB1.UserName AS InventoryInTransitBudgetName, CAA.UserName AS ClearingAccountActivityName, CAA1.UserName AS InventoryInTransitActivityName, MAD.InventoryBudgetMasterId
                                        , MAD.InventoryActivityId, MAD.ExpenseBudgetMasterId, MAD.ExpenseActivityId, IB.UserName AS InventoryBudgetName, IA.UserName AS InventoryActivityName
					                    , EB.UserName AS ExpenseBudgetName, EA.UserName AS ExpenseActivityName
										,MAD.DebitNoteGLId, GLGIDN.AccountCode + ' - ' + GLGIDN.UserName AS DebitNoteGLInfo, MAD.DebitNoteBudgetMasterId,BDN.UserName AS DebitNoteBudgetName,MAD.DebitNoteActivityId, ADN.UserName AS DebitNoteActivityName
										,MAD.CreditNoteGLId, GLGICN.AccountCode + ' - ' + GLGICN.UserName AS CreditNoteGLInfo, MAD.CreditNoteBudgetMasterId,BCN.UserName AS CreditNoteBudgetName,MAD.CreditNoteActivityId, ACN.UserName AS CreditNoteActivityName
										,MAD.ShortageGLId, GLGIST.AccountCode + ' - ' + GLGIST.UserName AS ShortageGLInfo, MAD.ShortageBudgetMasterId,BST.UserName AS ShortageBudgetName,MAD.ShortageActivityId, AST.UserName AS ShortageActivityName
										,MAD.RejectionGLId, GLGIRJ.AccountCode + ' - ' + GLGIRJ.UserName AS RejectionGLInfo, MAD.RejectionBudgetMasterId,BRJ.UserName AS RejectionBudgetName,MAD.RejectionActivityId, ARJ.UserName AS RejectionActivityName
        			                    FROM HKP.COA AS C
        			                    LEFT JOIN HKP.MaterialGroupGL AS MAD ON MAD.COAId=c.Id
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI5 ON GLGI5.Id=MAD.InventoryInTransitGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.InventoryGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.ExpenseGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGIDN ON GLGIDN.Id=MAD.DebitNoteGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGICN ON GLGICN.Id=MAD.CreditNoteGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGIST ON GLGIST.Id=MAD.ShortageGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGIRJ ON GLGIRJ.Id=MAD.RejectionGLId
					                    LEFT JOIN MST.BudgetMaster AS DPBM ON MAD.DownPaymentBudgetMasterId = DPBM.Id
					                    LEFT JOIN HKP.Budget AS DPB ON DPBM.BudgetId = DPB.Id
					                    LEFT JOIN HKP.Activity AS DPA ON MAD.DownPaymentActivityId = DPA.Id
					                    LEFT JOIN MST.BudgetMaster AS CABM ON MAD.ClearingAccountBudgetMasterId = CABM.Id
					                    LEFT JOIN MST.BudgetMaster AS CABM1 ON MAD.InventoryInTransitBudgetMasterId = CABM1.Id
					                    LEFT JOIN HKP.Budget AS CAB ON CABM.BudgetId = CAB.Id
					                    LEFT JOIN HKP.Budget AS CAB1 ON CABM1.BudgetId = CAB.Id
					                    LEFT JOIN HKP.Activity AS CAA ON MAD.ClearingAccountActivityId = CAA.Id
					                    LEFT JOIN HKP.Activity AS CAA1 ON MAD.InventoryInTransitActivityId = CAA1.Id
					                    LEFT JOIN MST.BudgetMaster AS IBM ON MAD.InventoryBudgetMasterId = IBM.Id
					                    LEFT JOIN HKP.Budget AS IB ON IBM.BudgetId = IB.Id
					                    LEFT JOIN HKP.Activity AS IA ON MAD.InventoryActivityId = IA.Id
					                    LEFT JOIN MST.BudgetMaster AS EBM ON MAD.ExpenseBudgetMasterId = EBM.Id
					                    LEFT JOIN HKP.Budget AS EB ON EBM.BudgetId = EB.Id
					                    LEFT JOIN HKP.Activity AS EA ON MAD.ExpenseActivityId = EA.Id
										 LEFT JOIN MST.BudgetMaster AS BMDN ON MAD.DebitNoteBudgetMasterId = BMDN.Id
					                    LEFT JOIN HKP.Budget AS BDN ON BMDN.BudgetId = BDN.Id
					                    LEFT JOIN HKP.Activity AS ADN ON MAD.DebitNoteActivityId = ADN.Id
										 LEFT JOIN MST.BudgetMaster AS BMCN ON MAD.CreditNoteBudgetMasterId = BMDN.Id
					                    LEFT JOIN HKP.Budget AS BCN ON BMCN.BudgetId = BCN.Id
					                    LEFT JOIN HKP.Activity AS ACN ON MAD.CreditNoteActivityId = ACN.Id
										LEFT JOIN MST.BudgetMaster AS BMST ON MAD.ShortageBudgetMasterId = BMDN.Id
					                    LEFT JOIN HKP.Budget AS BST ON BMCN.BudgetId = BST.Id
					                    LEFT  JOIN HKP.Activity AS AST ON MAD.ShortageActivityId = AST.Id
										LEFT  JOIN MST.BudgetMaster AS BMRJ ON MAD.RejectionBudgetMasterId = BMDN.Id
					                    LEFT  JOIN HKP.Budget AS BRJ ON BMCN.BudgetId = BRJ.Id
					                    LEFT  JOIN HKP.Activity AS ARJ ON MAD.RejectionActivityId = ARJ.Id
										" + coaStr + @"
									)AS F ON F.MaterialGroupMasterId = MGM.Id";
                return _sqlRepository.GetDataCollection(sqlText);

                //return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<object> GetSearchWithCombineWithAssign(string coaId)
        {
            try
            {
                string sqlText = "";
                var coaStr = " ";
                if (coaId != "null")
                    coaStr += "where ISNULL(c.Id,'') ='" + coaId + @"'";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                sqlText = @"SELECT [CheckBoxSelect] = Convert(bit, 'False'),  F.Id, MGM.Id AS MaterialGroupMasterId, MGM.UserName AS MaterialGroupMasterName, MG1.UserName AS MaterialGroup1Name, MG2.UserName AS MaterialGroup2Name
                                    , MG3.UserName AS MaterialGroup3Name, MG4.UserName AS MaterialGroup4Name, MT.Description AS MaterialTypeName, MGM.MaterialGroup1Id
        		                    , MGM.MaterialGroup2Id, MGM.MaterialGroup3Id, MGM.MaterialGroup4Id, MGM.MaterialTypeId, F.DownPaymentGLId, F.ClearingAccountGLId, F.COAId
        		                    , F.UserName 'COAName', F.DownPaymentGLInfo, F.ClearingAccountGLInfo, F.DownPaymentBudgetMasterId, F.DownPaymentActivityId, F.DownPaymentBudgetName
					                , F.DownPaymentActivityName, F.ClearingAccountBudgetMasterId, F.ClearingAccountActivityId, F.ClearingAccountBudgetName, F.ClearingAccountActivityName
                                    , F.InventoryGLInfo, F.InventoryGLId, F.InventoryBudgetMasterId, F.InventoryActivityId, F.InventoryBudgetName, F.InventoryActivityName, F.ExpenseGLInfo
                                    , F.ExpenseGLId, F.ExpenseBudgetMasterId, F.ExpenseActivityId, F.ExpenseBudgetName, F.ExpenseActivityName
                                    , F.DebitNoteGLId, F.DebitNoteBudgetMasterId, F.DebitNoteActivityId, F.DebitNoteGLInfo, F.DebitNoteBudgetName, F.DebitNoteActivityName
                                    , F.CreditNoteGLId, F.CreditNoteBudgetMasterId, F.CreditNoteActivityId, F.CreditNoteGLInfo, F.CreditNoteBudgetName, F.CreditNoteActivityName
                                    , F.ShortageGLId, F.ShortageBudgetMasterId, F.ShortageActivityId, F.ShortageGLInfo, F.ShortageBudgetName, F.ShortageActivityName
                                    , F.RejectionGLId, F.RejectionBudgetMasterId, F.RejectionActivityId, F.RejectionGLInfo, F.RejectionBudgetName, F.RejectionActivityName
                                    FROM MST.MaterialGroupMaster As MGM
                                    LEFT  JOIN HKP.MaterialGroup1 As MG1 ON MG1.Id = MGM.MaterialGroup1Id
                                    LEFT  JOIN HKP.MaterialGroup2 As MG2 ON MG2.Id = MGM.MaterialGroup2Id
                                    LEFT  JOIN HKP.MaterialGroup3 As MG3 ON MG3.Id = MGM.MaterialGroup3Id
                                    LEFT  JOIN HKP.MaterialGroup4 As MG4 ON MG4.Id = MGM.MaterialGroup4Id
                                    LEFT  JOIN HKP.MaterialType As MT ON MT.Id = MGM.MaterialTypeId
                                    LEFT  JOIN (SELECT MAD.Id, MAD.MaterialGroupMasterId, c.Id AS COAId, GLGI1.AccountCode, MAD.DownPaymentGLId, MAD.ClearingAccountGLId
                                        , MAD.InventoryGLId, MAD.ExpenseGLId, C.UserName, GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo, GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
					                    , GLGI3.AccountCode + ' - ' + GLGI3.UserName AS InventoryGLInfo, GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo, MAD.DownPaymentBudgetMasterId
                                        , MAD.DownPaymentActivityId, DPB.UserName AS DownPaymentBudgetName, DPA.UserName AS DownPaymentActivityName, MAD.ClearingAccountBudgetMasterId
                                        , MAD.ClearingAccountActivityId, CAB.UserName AS ClearingAccountBudgetName, CAA.UserName AS ClearingAccountActivityName, MAD.InventoryBudgetMasterId
                                        , MAD.InventoryActivityId, MAD.ExpenseBudgetMasterId, MAD.ExpenseActivityId, IB.UserName AS InventoryBudgetName, IA.UserName AS InventoryActivityName
					                    , EB.UserName AS ExpenseBudgetName, EA.UserName AS ExpenseActivityName
										,MAD.DebitNoteGLId, GLGIDN.AccountCode + ' - ' + GLGIDN.UserName AS DebitNoteGLInfo, MAD.DebitNoteBudgetMasterId,BDN.UserName AS DebitNoteBudgetName,MAD.DebitNoteActivityId, ADN.UserName AS DebitNoteActivityName
										,MAD.CreditNoteGLId, GLGICN.AccountCode + ' - ' + GLGICN.UserName AS CreditNoteGLInfo, MAD.CreditNoteBudgetMasterId,BCN.UserName AS CreditNoteBudgetName,MAD.CreditNoteActivityId, ACN.UserName AS CreditNoteActivityName
										,MAD.ShortageGLId, GLGIST.AccountCode + ' - ' + GLGIST.UserName AS ShortageGLInfo, MAD.ShortageBudgetMasterId,BST.UserName AS ShortageBudgetName,MAD.ShortageActivityId, AST.UserName AS ShortageActivityName
										,MAD.RejectionGLId, GLGIRJ.AccountCode + ' - ' + GLGIRJ.UserName AS RejectionGLInfo, MAD.RejectionBudgetMasterId,BRJ.UserName AS RejectionBudgetName,MAD.RejectionActivityId, ARJ.UserName AS RejectionActivityName
        			                    FROM HKP.COA AS C
        			                    LEFT JOIN HKP.MaterialGroupGL AS MAD ON MAD.COAId=c.Id
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.InventoryGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.ExpenseGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGIDN ON GLGIDN.Id=MAD.DebitNoteGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGICN ON GLGICN.Id=MAD.CreditNoteGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGIST ON GLGIST.Id=MAD.ShortageGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGIRJ ON GLGIRJ.Id=MAD.RejectionGLId
					                    LEFT JOIN MST.BudgetMaster AS DPBM ON MAD.DownPaymentBudgetMasterId = DPBM.Id
					                    LEFT JOIN HKP.Budget AS DPB ON DPBM.BudgetId = DPB.Id
					                    LEFT JOIN HKP.Activity AS DPA ON MAD.DownPaymentActivityId = DPA.Id
					                    LEFT JOIN MST.BudgetMaster AS CABM ON MAD.ClearingAccountBudgetMasterId = CABM.Id
					                    LEFT JOIN HKP.Budget AS CAB ON CABM.BudgetId = CAB.Id
					                    LEFT JOIN HKP.Activity AS CAA ON MAD.ClearingAccountActivityId = CAA.Id
					                    LEFT JOIN MST.BudgetMaster AS IBM ON MAD.InventoryBudgetMasterId = IBM.Id
					                    LEFT JOIN HKP.Budget AS IB ON IBM.BudgetId = IB.Id
					                    LEFT JOIN HKP.Activity AS IA ON MAD.InventoryActivityId = IA.Id
					                    LEFT JOIN MST.BudgetMaster AS EBM ON MAD.ExpenseBudgetMasterId = EBM.Id
					                    LEFT JOIN HKP.Budget AS EB ON EBM.BudgetId = EB.Id
					                    LEFT JOIN HKP.Activity AS EA ON MAD.ExpenseActivityId = EA.Id
										 LEFT JOIN MST.BudgetMaster AS BMDN ON MAD.DebitNoteBudgetMasterId = BMDN.Id
					                    LEFT JOIN HKP.Budget AS BDN ON BMDN.BudgetId = BDN.Id
					                    LEFT JOIN HKP.Activity AS ADN ON MAD.DebitNoteActivityId = ADN.Id
										 LEFT JOIN MST.BudgetMaster AS BMCN ON MAD.CreditNoteBudgetMasterId = BMDN.Id
					                    LEFT JOIN HKP.Budget AS BCN ON BMCN.BudgetId = BCN.Id
					                    LEFT JOIN HKP.Activity AS ACN ON MAD.CreditNoteActivityId = ACN.Id
										LEFT JOIN MST.BudgetMaster AS BMST ON MAD.ShortageBudgetMasterId = BMDN.Id
					                    LEFT JOIN HKP.Budget AS BST ON BMCN.BudgetId = BST.Id
					                    LEFT  JOIN HKP.Activity AS AST ON MAD.ShortageActivityId = AST.Id
										LEFT  JOIN MST.BudgetMaster AS BMRJ ON MAD.RejectionBudgetMasterId = BMDN.Id
					                    LEFT  JOIN HKP.Budget AS BRJ ON BMCN.BudgetId = BRJ.Id
					                    LEFT  JOIN HKP.Activity AS ARJ ON MAD.RejectionActivityId = ARJ.Id
										" + coaStr + @"
									)AS F ON F.MaterialGroupMasterId = MGM.Id
                                    WHERE F.DownPaymentGLId <> '' AND F.ClearingAccountGLId <> '' AND F.InventoryGLId <> '' AND F.ExpenseGLId <>''";
                return _sqlRepository.GetDataCollection(sqlText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<object> GetSearchWithCombineWithNotAssign(string coaId)
        {
            try
            {
                string sqlText = "";
                var coaStr = " ";
                if (coaId != "null")
                    coaStr += "where ISNULL(c.Id,'') ='" + coaId + @"'";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                sqlText = @"SELECT [CheckBoxSelect] = Convert(bit, 'False'),  F.Id, MGM.Id AS MaterialGroupMasterId, MGM.UserName AS MaterialGroupMasterName, MG1.UserName AS MaterialGroup1Name, MG2.UserName AS MaterialGroup2Name
                                    , MG3.UserName AS MaterialGroup3Name, MG4.UserName AS MaterialGroup4Name, MT.Description AS MaterialTypeName, MGM.MaterialGroup1Id
        		                    , MGM.MaterialGroup2Id, MGM.MaterialGroup3Id, MGM.MaterialGroup4Id, MGM.MaterialTypeId, F.DownPaymentGLId, F.ClearingAccountGLId, F.COAId
        		                    , F.UserName 'COAName', F.DownPaymentGLInfo, F.ClearingAccountGLInfo, F.DownPaymentBudgetMasterId, F.DownPaymentActivityId, F.DownPaymentBudgetName
					                , F.DownPaymentActivityName, F.ClearingAccountBudgetMasterId, F.ClearingAccountActivityId, F.ClearingAccountBudgetName, F.ClearingAccountActivityName
                                    , F.InventoryGLInfo, F.InventoryGLId, F.InventoryBudgetMasterId, F.InventoryActivityId, F.InventoryBudgetName, F.InventoryActivityName, F.ExpenseGLInfo
                                    , F.ExpenseGLId, F.ExpenseBudgetMasterId, F.ExpenseActivityId, F.ExpenseBudgetName, F.ExpenseActivityName
                                    , F.DebitNoteGLId, F.DebitNoteBudgetMasterId, F.DebitNoteActivityId, F.DebitNoteGLInfo, F.DebitNoteBudgetName, F.DebitNoteActivityName
                                    , F.CreditNoteGLId, F.CreditNoteBudgetMasterId, F.CreditNoteActivityId, F.CreditNoteGLInfo, F.CreditNoteBudgetName, F.CreditNoteActivityName
                                    , F.ShortageGLId, F.ShortageBudgetMasterId, F.ShortageActivityId, F.ShortageGLInfo, F.ShortageBudgetName, F.ShortageActivityName
                                    , F.RejectionGLId, F.RejectionBudgetMasterId, F.RejectionActivityId, F.RejectionGLInfo, F.RejectionBudgetName, F.RejectionActivityName
                                    FROM MST.MaterialGroupMaster As MGM
                                    LEFT  JOIN HKP.MaterialGroup1 As MG1 ON MG1.Id = MGM.MaterialGroup1Id
                                    LEFT  JOIN HKP.MaterialGroup2 As MG2 ON MG2.Id = MGM.MaterialGroup2Id
                                    LEFT  JOIN HKP.MaterialGroup3 As MG3 ON MG3.Id = MGM.MaterialGroup3Id
                                    LEFT  JOIN HKP.MaterialGroup4 As MG4 ON MG4.Id = MGM.MaterialGroup4Id
                                    LEFT  JOIN HKP.MaterialType As MT ON MT.Id = MGM.MaterialTypeId
                                    LEFT  JOIN (SELECT MAD.Id, MAD.MaterialGroupMasterId, c.Id AS COAId, GLGI1.AccountCode, MAD.DownPaymentGLId, MAD.ClearingAccountGLId
                                        , MAD.InventoryGLId, MAD.ExpenseGLId, C.UserName, GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo, GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
					                    , GLGI3.AccountCode + ' - ' + GLGI3.UserName AS InventoryGLInfo, GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo, MAD.DownPaymentBudgetMasterId
                                        , MAD.DownPaymentActivityId, DPB.UserName AS DownPaymentBudgetName, DPA.UserName AS DownPaymentActivityName, MAD.ClearingAccountBudgetMasterId
                                        , MAD.ClearingAccountActivityId, CAB.UserName AS ClearingAccountBudgetName, CAA.UserName AS ClearingAccountActivityName, MAD.InventoryBudgetMasterId
                                        , MAD.InventoryActivityId, MAD.ExpenseBudgetMasterId, MAD.ExpenseActivityId, IB.UserName AS InventoryBudgetName, IA.UserName AS InventoryActivityName
					                    , EB.UserName AS ExpenseBudgetName, EA.UserName AS ExpenseActivityName
										,MAD.DebitNoteGLId, GLGIDN.AccountCode + ' - ' + GLGIDN.UserName AS DebitNoteGLInfo, MAD.DebitNoteBudgetMasterId,BDN.UserName AS DebitNoteBudgetName,MAD.DebitNoteActivityId, ADN.UserName AS DebitNoteActivityName
										,MAD.CreditNoteGLId, GLGICN.AccountCode + ' - ' + GLGICN.UserName AS CreditNoteGLInfo, MAD.CreditNoteBudgetMasterId,BCN.UserName AS CreditNoteBudgetName,MAD.CreditNoteActivityId, ACN.UserName AS CreditNoteActivityName
										,MAD.ShortageGLId, GLGIST.AccountCode + ' - ' + GLGIST.UserName AS ShortageGLInfo, MAD.ShortageBudgetMasterId,BST.UserName AS ShortageBudgetName,MAD.ShortageActivityId, AST.UserName AS ShortageActivityName
										,MAD.RejectionGLId, GLGIRJ.AccountCode + ' - ' + GLGIRJ.UserName AS RejectionGLInfo, MAD.RejectionBudgetMasterId,BRJ.UserName AS RejectionBudgetName,MAD.RejectionActivityId, ARJ.UserName AS RejectionActivityName
        			                    FROM HKP.COA AS C
        			                    LEFT JOIN HKP.MaterialGroupGL AS MAD ON MAD.COAId=c.Id
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.InventoryGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.ExpenseGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGIDN ON GLGIDN.Id=MAD.DebitNoteGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGICN ON GLGICN.Id=MAD.CreditNoteGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGIST ON GLGIST.Id=MAD.ShortageGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGIRJ ON GLGIRJ.Id=MAD.RejectionGLId
					                    LEFT JOIN MST.BudgetMaster AS DPBM ON MAD.DownPaymentBudgetMasterId = DPBM.Id
					                    LEFT JOIN HKP.Budget AS DPB ON DPBM.BudgetId = DPB.Id
					                    LEFT JOIN HKP.Activity AS DPA ON MAD.DownPaymentActivityId = DPA.Id
					                    LEFT JOIN MST.BudgetMaster AS CABM ON MAD.ClearingAccountBudgetMasterId = CABM.Id
					                    LEFT JOIN HKP.Budget AS CAB ON CABM.BudgetId = CAB.Id
					                    LEFT JOIN HKP.Activity AS CAA ON MAD.ClearingAccountActivityId = CAA.Id
					                    LEFT JOIN MST.BudgetMaster AS IBM ON MAD.InventoryBudgetMasterId = IBM.Id
					                    LEFT JOIN HKP.Budget AS IB ON IBM.BudgetId = IB.Id
					                    LEFT JOIN HKP.Activity AS IA ON MAD.InventoryActivityId = IA.Id
					                    LEFT JOIN MST.BudgetMaster AS EBM ON MAD.ExpenseBudgetMasterId = EBM.Id
					                    LEFT JOIN HKP.Budget AS EB ON EBM.BudgetId = EB.Id
					                    LEFT JOIN HKP.Activity AS EA ON MAD.ExpenseActivityId = EA.Id
										 LEFT JOIN MST.BudgetMaster AS BMDN ON MAD.DebitNoteBudgetMasterId = BMDN.Id
					                    LEFT JOIN HKP.Budget AS BDN ON BMDN.BudgetId = BDN.Id
					                    LEFT JOIN HKP.Activity AS ADN ON MAD.DebitNoteActivityId = ADN.Id
										 LEFT JOIN MST.BudgetMaster AS BMCN ON MAD.CreditNoteBudgetMasterId = BMDN.Id
					                    LEFT JOIN HKP.Budget AS BCN ON BMCN.BudgetId = BCN.Id
					                    LEFT JOIN HKP.Activity AS ACN ON MAD.CreditNoteActivityId = ACN.Id
										LEFT JOIN MST.BudgetMaster AS BMST ON MAD.ShortageBudgetMasterId = BMDN.Id
					                    LEFT JOIN HKP.Budget AS BST ON BMCN.BudgetId = BST.Id
					                    LEFT  JOIN HKP.Activity AS AST ON MAD.ShortageActivityId = AST.Id
										LEFT  JOIN MST.BudgetMaster AS BMRJ ON MAD.RejectionBudgetMasterId = BMDN.Id
					                    LEFT  JOIN HKP.Budget AS BRJ ON BMCN.BudgetId = BRJ.Id
					                    LEFT  JOIN HKP.Activity AS ARJ ON MAD.RejectionActivityId = ARJ.Id
										" + coaStr + @"
									)AS F ON F.MaterialGroupMasterId = MGM.Id
                                WHERE (ISNULL(F.DownPaymentGLId, '') = '' OR ISNULL(F.ClearingAccountGLId, '') = ''  OR ISNULL(F.InventoryGLId , '') = ''  OR ISNULL(F.ExpenseGLId , '') = '') ";
                return _sqlRepository.GetDataCollection(sqlText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public GridModel GetSearchWithCombineCoa(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT F.Id, MGM.Id AS MaterialGroupMasterId, MGM.UserName AS MaterialGroupMasterName, MG1.UserName AS MaterialGroup1Name, MG2.UserName AS MaterialGroup2Name
                                    , MG3.UserName AS MaterialGroup3Name, MG4.UserName AS MaterialGroup4Name, MT.Description AS MaterialTypeName, MGM.MaterialGroup1Id
        		                    , MGM.MaterialGroup2Id, MGM.MaterialGroup3Id, MGM.MaterialGroup4Id, MGM.MaterialTypeId, F.DownPaymentGLId, F.ClearingAccountGLId, F.COAId
        		                    , F.UserName 'COAName', F.DownPaymentGLInfo, F.ClearingAccountGLInfo, F.DownPaymentBudgetMasterId, F.DownPaymentActivityId, F.DownPaymentBudgetName
					                , F.DownPaymentActivityName, F.ClearingAccountBudgetMasterId, F.ClearingAccountActivityId, F.ClearingAccountBudgetName, F.ClearingAccountActivityName
                                    , F.InventoryGLInfo, F.InventoryGLId, F.InventoryBudgetMasterId, F.InventoryActivityId, F.InventoryBudgetName, F.InventoryActivityName, F.ExpenseGLInfo
                                    , F.ExpenseGLId, F.ExpenseBudgetMasterId, F.ExpenseActivityId, F.ExpenseBudgetName, F.ExpenseActivityName
                                    , F.DebitNoteGLId, F.DebitNoteBudgetMasterId, F.DebitNoteActivityId, F.DebitNoteGLInfo, F.DebitNoteBudgetName, F.DebitNoteActivityName
                                    , F.CreditNoteGLId, F.CreditNoteBudgetMasterId, F.CreditNoteActivityId, F.CreditNoteGLInfo, F.CreditNoteBudgetName, F.CreditNoteActivityName
                                    , F.ShortageGLId, F.ShortageBudgetMasterId, F.ShortageActivityId, F.ShortageGLInfo, F.ShortageBudgetName, F.ShortageActivityName
                                    , F.RejectionGLId, F.RejectionBudgetMasterId, F.RejectionActivityId, F.RejectionGLInfo, F.RejectionBudgetName, F.RejectionActivityName
                                    FROM MST.MaterialGroupMaster As MGM
                                    LEFT  JOIN HKP.MaterialGroup1 As MG1 ON MG1.Id = MGM.MaterialGroup1Id
                                    LEFT  JOIN HKP.MaterialGroup2 As MG2 ON MG2.Id = MGM.MaterialGroup2Id
                                    LEFT  JOIN HKP.MaterialGroup3 As MG3 ON MG3.Id = MGM.MaterialGroup3Id
                                    LEFT  JOIN HKP.MaterialGroup4 As MG4 ON MG4.Id = MGM.MaterialGroup4Id
                                    LEFT  JOIN HKP.MaterialType As MT ON MT.Id = MGM.MaterialTypeId
                                    LEFT  JOIN (SELECT MAD.Id, MAD.MaterialGroupMasterId, c.Id AS COAId, GLGI1.AccountCode, MAD.DownPaymentGLId, MAD.ClearingAccountGLId
                                        , MAD.InventoryGLId, MAD.ExpenseGLId, C.UserName, GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo, GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
					                    , GLGI3.AccountCode + ' - ' + GLGI3.UserName AS InventoryGLInfo, GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo, MAD.DownPaymentBudgetMasterId
                                        , MAD.DownPaymentActivityId, DPB.UserName AS DownPaymentBudgetName, DPA.UserName AS DownPaymentActivityName, MAD.ClearingAccountBudgetMasterId
                                        , MAD.ClearingAccountActivityId, CAB.UserName AS ClearingAccountBudgetName, CAA.UserName AS ClearingAccountActivityName, MAD.InventoryBudgetMasterId
                                        , MAD.InventoryActivityId, MAD.ExpenseBudgetMasterId, MAD.ExpenseActivityId, IB.UserName AS InventoryBudgetName, IA.UserName AS InventoryActivityName
					                    , EB.UserName AS ExpenseBudgetName, EA.UserName AS ExpenseActivityName
										,MAD.DebitNoteGLId, GLGIDN.AccountCode + ' - ' + GLGIDN.UserName AS DebitNoteGLInfo, MAD.DebitNoteBudgetMasterId,BDN.UserName AS DebitNoteBudgetName,MAD.DebitNoteActivityId, ADN.UserName AS DebitNoteActivityName
										,MAD.CreditNoteGLId, GLGICN.AccountCode + ' - ' + GLGICN.UserName AS CreditNoteGLInfo, MAD.CreditNoteBudgetMasterId,BCN.UserName AS CreditNoteBudgetName,MAD.CreditNoteActivityId, ACN.UserName AS CreditNoteActivityName
										,MAD.ShortageGLId, GLGIST.AccountCode + ' - ' + GLGIST.UserName AS ShortageGLInfo, MAD.ShortageBudgetMasterId,BST.UserName AS ShortageBudgetName,MAD.ShortageActivityId, AST.UserName AS ShortageActivityName
										,MAD.RejectionGLId, GLGIRJ.AccountCode + ' - ' + GLGIRJ.UserName AS RejectionGLInfo, MAD.RejectionBudgetMasterId,BRJ.UserName AS RejectionBudgetName,MAD.RejectionActivityId, ARJ.UserName AS RejectionActivityName
        			                    FROM HKP.COA AS C
        			                    LEFT JOIN HKP.MaterialGroupGL AS MAD ON MAD.COAId=c.Id
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.InventoryGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.ExpenseGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGIDN ON GLGIDN.Id=MAD.DebitNoteGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGICN ON GLGICN.Id=MAD.CreditNoteGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGIST ON GLGIST.Id=MAD.ShortageGLId
        			                    LEFT JOIN HKP.GLGeneralInfo AS GLGIRJ ON GLGIRJ.Id=MAD.RejectionGLId
					                    LEFT JOIN MST.BudgetMaster AS DPBM ON MAD.DownPaymentBudgetMasterId = DPBM.Id
					                    LEFT JOIN HKP.Budget AS DPB ON DPBM.BudgetId = DPB.Id
					                    LEFT JOIN HKP.Activity AS DPA ON MAD.DownPaymentActivityId = DPA.Id
					                    LEFT JOIN MST.BudgetMaster AS CABM ON MAD.ClearingAccountBudgetMasterId = CABM.Id
					                    LEFT JOIN HKP.Budget AS CAB ON CABM.BudgetId = CAB.Id
					                    LEFT JOIN HKP.Activity AS CAA ON MAD.ClearingAccountActivityId = CAA.Id
					                    LEFT JOIN MST.BudgetMaster AS IBM ON MAD.InventoryBudgetMasterId = IBM.Id
					                    LEFT JOIN HKP.Budget AS IB ON IBM.BudgetId = IB.Id
					                    LEFT JOIN HKP.Activity AS IA ON MAD.InventoryActivityId = IA.Id
					                    LEFT JOIN MST.BudgetMaster AS EBM ON MAD.ExpenseBudgetMasterId = EBM.Id
					                    LEFT JOIN HKP.Budget AS EB ON EBM.BudgetId = EB.Id
					                    LEFT JOIN HKP.Activity AS EA ON MAD.ExpenseActivityId = EA.Id
										 LEFT JOIN MST.BudgetMaster AS BMDN ON MAD.DebitNoteBudgetMasterId = BMDN.Id
					                    LEFT JOIN HKP.Budget AS BDN ON BMDN.BudgetId = BDN.Id
					                    LEFT JOIN HKP.Activity AS ADN ON MAD.DebitNoteActivityId = ADN.Id
										 LEFT JOIN MST.BudgetMaster AS BMCN ON MAD.CreditNoteBudgetMasterId = BMDN.Id
					                    LEFT JOIN HKP.Budget AS BCN ON BMCN.BudgetId = BCN.Id
					                    LEFT JOIN HKP.Activity AS ACN ON MAD.CreditNoteActivityId = ACN.Id
										LEFT JOIN MST.BudgetMaster AS BMST ON MAD.ShortageBudgetMasterId = BMDN.Id
					                    LEFT JOIN HKP.Budget AS BST ON BMCN.BudgetId = BST.Id
					                    LEFT  JOIN HKP.Activity AS AST ON MAD.ShortageActivityId = AST.Id
										LEFT  JOIN MST.BudgetMaster AS BMRJ ON MAD.RejectionBudgetMasterId = BMDN.Id
					                    LEFT  JOIN HKP.Budget AS BRJ ON BMCN.BudgetId = BRJ.Id
					                    LEFT  JOIN HKP.Activity AS ARJ ON MAD.RejectionActivityId = ARJ.Id
        			                )AS F ON F.MaterialGroupMasterId=MGM.Id";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public GridModel GetPartyAccountGroup(GridParameter parameters, string accountType)
        {
            try
            {
                parameters.CmdText = @"SELECT FADVR.Id, FADVR.FixedAssetGLId, FADVR.VendorReconGLId, FADVR.AddedBy, FADVR.AddedDate, FADVR.AddedFromIP, FADVR.UpdatedBy
                                    , FADVR.UpdatedDate, FADVR.UpdatedFromIP, PAG.Id AS PartyAccountGroupId, PAG.UserName, PAG.Code, VR.UserName AS VendorRecontGLText, VR.AccountCode AS VendorReconGLCode
                                    FROM HKP.PartyAccountGroup AS PAG
                                    LEFT OUTER JOIN [HKP].[MaterialGroupPartyAccountGroupGL]AS FADVR ON FADVR.PartyAccountGroupId=PAG.Id
                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS VR ON FADVR.GLGeneralInfoId=VR.Id
                                    WHERE PAG.AccountType='" + accountType + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public GridModel GetPartyAccountGroup(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT FADVR.Id, FADVR.FixedAssetGLId, FADVR.VendorReconGLId, FADVR.AddedBy, FADVR.AddedDate, FADVR.AddedFromIP, FADVR.UpdatedBy
                                    , FADVR.UpdatedDate, FADVR.UpdatedFromIP, PAG.Id AS PartyAccountGroupId, PAG.UserName, PAG.Code, VR.UserName AS VendorRecontGLText, VR.AccountCode AS VendorReconGLCode
                                    FROM HKP.PartyAccountGroup AS PAG
                                    LEFT OUTER JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS FADVR ON FADVR.PartyAccountGroupId=PAG.Id
                                    LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS VR ON FADVR.GLGeneralInfoId=VR.Id";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public GridModel GetPartyAccountVD(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT FADV.* , GLGI4.AccountCode AS ClearingAccGLCode, GLGI4.UserName AS ClearingAccGLText, B.UserName AS BudgetName, A.UserName AS ActivityName
                                    FROM  [HKP].[MaterialGroupPartyAccountGroupGL] AS FADV
                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FADV.GLGeneralInfoId
				                    LEFT OUTER JOIN MST.BudgetMaster BM ON FADV.BudgetMasterId=BM.Id
                                    LEFT OUTER JOIN HKP.Budget AS B ON BM.BudgetId=B.Id
                                    LEFT OUTER JOIN HKP.Activity AS A ON FADV.ActivityId=A.Id";
                return _sqlRepository.GetGridData(parameters);
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