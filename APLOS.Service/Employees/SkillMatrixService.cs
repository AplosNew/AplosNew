#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public partial class SkillMatrixService : Service<SkillMatrix>, ISkillMatrixService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ISOPAttachmentDetailService _sopAttachmentDetailService;
        private readonly IRepositoryAsync<SkillMatrix> _sopItemRepository;

        public SkillMatrixService(
            IRepositoryAsync<SkillMatrix> sopItemRepository
            , IPKGeneratorService pkGeneratorService
            , ISOPAttachmentDetailService SOPAttachmentDetailService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            ) : base(sopItemRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sopAttachmentDetailService = SOPAttachmentDetailService;
            _sqlRepository = sqlRepository;
            _sopItemRepository = sopItemRepository;
        }

        #endregion Constructor

        //public override void Delete(object id)
        //{
        //    var flag = false;
        //    try
        //    {
        //        UseChecking(id);
        //        _unitOfWork.BeginTransaction();
        //        flag = true;
        //        _sopAttachmentDetailService.DeleteGraphBySOPItem(id.ToString());
        //        DeleteGraph(id);
        //        _unitOfWork.SaveChanges();
        //        flag = false;
        //        _unitOfWork.Commit();
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        if (flag)
        //            _unitOfWork.Rollback();
        //    }
        //}

        //private void UseChecking(object id)
        //{
        //    if (_sopItemRepository.FKDependency("[HKP].[SOPItem]", id.ToString(), "[HKP].[SOPAttachmentDetail]"))
        //        throw new CustomException("Delete is not allowed after transaction.");
        //}

        //private string GetPK()
        //{
        //    return GetAutoNumber(nameof(SkillMatrix), PKGeneratorEnum.Auto, null, DateTime.Now);
        //}

        //public void InsertGraph(SkillMatrix entity, IEnumerable<SOPAttachmentDetail> sopAttachmentDetail)
        //{
        //    var flag = false;
        //    try
        //    {
        //        if (CheckUniqueRow(entity))
        //            throw new CustomException("This combination already exists!");
        //        _unitOfWork.BeginTransaction();
        //        flag = true;
        //        entity.Id = GetPK();
        //        _sopAttachmentDetailService.InsertGraph(sopAttachmentDetail, entity.Id);
        //        base.InsertGraph(entity);
        //        _unitOfWork.SaveChanges();
        //        flag = false;
        //        _unitOfWork.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //    finally
        //    {
        //        if (flag)
        //            _unitOfWork.Rollback();
        //    }
        //}

        //public void UpdateGraph(SOPItem entity, IEnumerable<SOPAttachmentDetail> sopAttachmentDetail)
        //{
        //    var flag = false;
        //    try
        //    {
        //        if (CheckUniqueRow(entity))
        //            throw new CustomException("This combination already exists!");
        //        _unitOfWork.BeginTransaction();
        //        flag = true;
        //        base.UpdateGraph(entity);
        //        _sopAttachmentDetailService.InsertGraph(sopAttachmentDetail, entity.Id);
        //        _unitOfWork.SaveChanges();
        //        flag = false;
        //        _unitOfWork.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //    finally
        //    {
        //        if (flag)
        //            _unitOfWork.Rollback();
        //    }
        //}

        //public GridModel Query(GridParameter parameters, string companyGroupId)
        //{
        //    try
        //    {
        //        parameters.CmdText = @"SELECT  SOPI.Id
        //                                  ,SOPI.CompanyGroupId
        //                                  ,SOPI.Sequence
        //                                  ,SOPI.Code
        //                                  ,SOPI.SOPCategoryId
        //                                     ,SOPC.UserName AS SOPCategory
        //                                  ,SOPI.SOPSubCategoryId
        //                                  ,SOPSC.UserName AS SOPSubCategory
        //                                  ,SOPI.ShortName
        //                                  ,SOPI.StandardName
        //                                  ,SOPI.UserName
        //                                  ,SOPI.Objective
        //                                  ,SOPI.Mission
        //                                  ,SOPI.Vision
        //                                  ,SOPI.Description
        //                                  ,SOPI.Remarks
        //                                  ,SOPI.Active
        //                                        ,SOPAM.TotalAttachment
        //                                FROM [HKP].[SOPItem] AS SOPI
        //                                LEFT OUTER JOIN [HKP].[SOPCategory] SOPC ON SOPI.SOPCategoryId = SOPC.Id
        //                                LEFT OUTER JOIN [HKP].[SOPSubCategory] SOPSC ON SOPI.SOPSubCategoryId = SOPSC.Id
        //                                LEFT OUTER JOIN (SELECT COUNT(Id) TotalAttachment,SOPItemId FROM [HKP].[SOPAttachmentDetail] group by SOPItemId) SOPAM on SOPAM.SOPItemId=SOPI.Id
        //                                WHERE SOPI.CompanyGroupId='" + companyGroupId + "'";
        //        return _sqlRepository.GetGridData(parameters);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}

        ///// <summary>
        ///// This list data show without grid existing sopItemId
        ///// </summary>
        ///// <param name="parameters"></param>
        ///// <param name="companyGroupId"></param>
        ///// <param name="sopItemIds"></param>
        ///// <returns></returns>
        //public GridModel Query(GridParameter parameters, string companyGroupId, string[] sopItemIds)
        //{
        //    try
        //    {
        //        var sopItemId = "";
        //        if (sopItemIds.Length > 0)
        //            sopItemId = string.Join(",", sopItemIds.Select(item => "'" + item + "'"));
        //        else
        //            sopItemId = "' '";
        //        parameters.CmdText = @"SELECT  SOPI.Id
        //                                  ,SOPI.CompanyGroupId
        //                                  ,SOPI.Sequence
        //                                  ,SOPI.Code
        //                                  ,SOPI.SOPCategoryId
        //                                     ,SOPC.UserName AS SOPCategory
        //                                  ,SOPI.SOPSubCategoryId
        //                                  ,SOPSC.UserName AS SOPSubCategory
        //                                  ,SOPI.ShortName
        //                                  ,SOPI.StandardName
        //                                  ,SOPI.UserName
        //                                  ,SOPI.Objective
        //                                  ,SOPI.Mission
        //                                  ,SOPI.Vision
        //                                  ,SOPI.Description
        //                                  ,SOPI.Remarks
        //                                  ,SOPI.Active
        //                                        ,SOPAM.TotalAttachment
        //                                FROM [HKP].[SOPItem] AS SOPI
        //                                LEFT OUTER JOIN [HKP].[SOPCategory] SOPC ON SOPI.SOPCategoryId = SOPC.Id
        //                                LEFT OUTER JOIN [HKP].[SOPSubCategory] SOPSC ON SOPI.SOPSubCategoryId = SOPSC.Id
        //                                LEFT OUTER JOIN (SELECT COUNT(Id) TotalAttachment,SOPItemId FROM [HKP].[SOPAttachmentDetail] group by SOPItemId) SOPAM on SOPAM.SOPItemId=SOPI.Id
        //                                WHERE SOPI.CompanyGroupId='" + companyGroupId + "'  AND SOPI.Id NOT IN (" + sopItemId + ")";
        //        return _sqlRepository.GetGridData(parameters);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}

        //private bool CheckUniqueRow(SOPItem sopItem)
        //{
        //    try
        //    {
        //        CustomIdentity identiy = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        return Any(r => r.Id != sopItem.Id && r.CompanyGroupId == identiy.CompanyGroupId && r.SOPCategoryId == sopItem.SOPCategoryId
        //          && r.SOPSubCategoryId == sopItem.SOPSubCategoryId
        //          && r.UserName == sopItem.UserName);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public IEnumerable<object> GetProcess()
        {
            try
            {

                string _sql = @"select id as drpValue,UserName as drpText from HKP.Process";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetEntity()
        {
            try
            {

                string _sql = @"select id as  drpValue,UserName as DrpText from ORG.Entity";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetSkillMaster()
        {
            try
            {
                string _sql = "";
                //Command Date: 13-05-2019

                //string _sql = @"SELECT 
                //  --OM.Code OerationCode
                // OM.UserName OperationName
                // ,OM.Type
                // --,OPMBF.EntityCode
                // ,OPMBF.EntityName
                //-- ,OPMBF.PositionCode
                // --,OPMBF.PositionName
                // ,OPMBF.Caption
                // --,MM.Code MachineCode
                // ,ISNULL(MM.UserName,'') MachineMaster
                // --,MC.Code MachineCategoryCode
                // ,ISNULL(MC.UserName,'') MachineCategory
                // --,MSC.Code MachineSubCategoryCode
                // ,ISNULL(MSC.UserName,'') MachineSubCategory
                // ,Skill = CASE 
                //  WHEN SKO.UserName IS NULL
                //   THEN SKM.UserName
                // WHEN SKM.UserName IS NULL
                //  THEN SKO.UserName
                // ELSE SKM.UserName
                //  END
                //  ,SkillId = CASE 
                //  WHEN SKO.id IS NULL
                //   THEN SKM.Id
                //  WHEN SKM.Id IS NULL
                //   THEN SKO.Id
                //  ELSE SKM.Id
                //  END  
                // ,SKG.Code SkillGroupingCode
                // ,SKG.Grouping
                // ,SKG.DesignationCategory
                // --,SKG.StandardSalary
                // ,P.UserName Process
                // ,OLDG.UserName LegalDesignation	                          
                // ,CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget) ManpowerBudget
                // ,ISNULL(E.OnRoll,0) OnRoll
                // ,ISNULL(E.TotalPresent,0) TotalPresent
                // ,OnRollShort=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) > 0 
                // THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) ELSE 0	END

                // ,OnRollExcess=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) < 0 
                // THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0))) ELSE 0	END

                // ,PresentShort=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) > 0 
                // THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) ELSE 0	END

                // ,PresentExcess=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) < 0 
                // THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0))) ELSE 0	END
                // FROM [MST].[OperationMaster] OM
                // LEFT JOIN MST.MachineMaster MM ON MM.Id = OM.MachineMasterId
                // LEFT JOIN HKP.MachineCategory MC ON MC.Id = MM.MachineCategoryId
                // LEFT JOIN HKP.MachineSubCategory MSC ON MSC.Id = MM.MachineSubCategoryId
                // LEFT JOIN [HKP].[Skill] SKO ON SKO.Id = OM.SkillId
                // LEFT JOIN [HKP].[Skill] SKM ON SKM.Id = MM.SkillId
                // LEFT JOIN [HKP].[SkillProcess] SP ON SP.SkillId = SKO.Id
                // LEFT JOIN [HKP].[Process] P ON P.Id = OM.ProcessId
                // LEFT JOIN SCS.SkillGrouping SKG ON SKG.Id = OM.SkillGroupId
                // LEFT JOIN HKP.LegalDesignation OLDG ON OLDG.Id = OM.LegalDesignationId
                // LEFT JOIN (
                // ----EmployeeInformation
                // SELECT e.OperationMasterId
                //  ,Count(convert(INT, E.SystemId)) AS [TOTAL ONROLL]
                // FROM EmployeeInformation e
                // WHERE E.EmployeeStatus = 'Active'
                // GROUP BY e.OperationMasterId
                // ) ONOP ON ONOP.OperationMasterId = OM.Id
                // LEFT JOIN (
                // ---OperationPositionMPBudget
                // SELECT *
                // FROM (
                //  SELECT OPM.OperationMasterId
                //   ,OM.Code OperationCode
                //   ,OM.UserName Operation
                //   ,EN.Id EntityId
                //   ,EN.Code EntityCode
                //   ,EN.UserName EntityName
                //   ,PS.Code PositionCode
                //   ,PS.UserName PositionName
                //   ,OPM.Caption
                //   ,OPM.ManpowerBudget
                //   ,PS.Id PositionId
                //  FROM [MST].[OperationPositionMPBudget] OPM
                //  LEFT JOIN [MST].[OperationMaster] OM ON OM.Id = OPM.OperationMasterId
                //  LEFT JOIN ORG.Entity EN ON EN.Id = OPM.EntityId
                //  LEFT JOIN ORG.Position PS ON PS.Id = OPM.PositionId
                //  ) OPMB
                // ) OPMBF ON OPMBF.OperationMasterId = OM.Id
                // LEFT JOIN (
                // --[Total Present] ,[ONROLL]
                // SELECT EM.OperationMasterId
                //  ,EN.Id EntityId
                //  ,POS.Id PositionId
                //  ,SUM([Status]) TotalPresent
                //  ,COUNT(EM.SystemId) ONROLL
                // FROM EmployeeInformation EM
                // LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EM.BudgetCode
                // LEFT JOIN ORG.Entity EN ON EN.Id = MB.EntityId
                // LEFT JOIN ORG.Position POS ON POS.Id = EM.PositionId
                // LEFT JOIN (
                //  SELECT DayStatus
                //   ,EmpSystemID
                //   ,[Status] = CASE 
                //    WHEN DayStatus IS NULL
                //     THEN 0
                //    WHEN DayStatus IS NOT NULL
                //     THEN 1
                //    ELSE 0
                //    END
                //  FROM AttdnProcessData
                //  WHERE DayStatus IN (
                //    'LP'
                //    ,'HDP'
                //    ,'P'
                //    )
                //   AND WorkDate = getdate()
                //  ) AD ON AD.EmpSystemID = EM.SystemId
                // WHERE EM.EmployeeStatus = 'Active'
                // GROUP BY EM.OperationMasterId
                //  ,EN.Id
                //  ,POS.Id
                // ) E ON E.OperationMasterId = OPMBF.OperationMasterId
                // AND E.PositionID = OPMBF.PositionId
                // AND E.EntityId = OPMBF.EntityId
                // ORDER BY CONVERT(INT, OM.Code)";

                //string _sql = @"Select ISNULL(EntityId,0) EntityId,EntityName,ProcessId,Process,OperationCode,OperationName,SkillId,Skill,SkillGroupID,SkillGroupe,ISNULL(MachineCategoryID,0) MachineCategoryID,MachineCategory,ISNULL(MachineSubCategoryId,0) MachineSubCategoryId,MachineSubCategory,isnull(Position,'-') Position,ManpowerBudget,OnRoll,OnRollShort,OnRollExcess,TotalPresent,PresentShort,PresentExcess from 
                //    (
                //    SELECT OM.Code OperationCode, OM.UserName OperationName,OM.Type, OPMBF.EntityId, OPMBF.EntityCode, OPMBF.EntityName, OPMBF.PositionCode, OPMBF.PositionName, OPMBF.Caption Position, 
                //    MM.Code MachineCode, ISNULL(MM.UserName,'') MachineMaster, MC.Id MachineCategoryID, MC.Code MachineCategoryCode, ISNULL(MC.UserName,'') MachineCategory, MSC.Id MachineSubCategoryId, MSC.Code MachineSubCategoryCode, ISNULL(MSC.UserName,'') MachineSubCategory,
                //    Skill = CASE 
                //                    WHEN SKO.UserName IS NULL
                //                      THEN SKM.UserName
                //                    WHEN SKM.UserName IS NULL
                //                     THEN SKO.UserName
                //                    ELSE SKM.UserName
                //                     END
                //                     ,SkillId = CASE 
                //                     WHEN SKO.id IS NULL
                //                      THEN SKM.Id
                //                     WHEN SKM.Id IS NULL
                //                      THEN SKO.Id
                //                     ELSE SKM.Id
                //                     END  
                //    ,SKG.Id SkillGroupID,SKG.Code SkillGroupingCode, SKG.Grouping SkillGroupe, SKG.DesignationCategory, SKG.StandardSalary,P.Id ProcessId, P.UserName Process, OLDG.UserName LegalDesignation, CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget) ManpowerBudget,
                //    ISNULL(E.OnRoll,0) OnRoll, ISNULL(E.TotalPresent,0) TotalPresent, 
                //    OnRollShort=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) > 0 
                //                    THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) ELSE 0	END

                //                    ,OnRollExcess=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) < 0 
                //                    THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0))) ELSE 0	END

                //                    ,PresentShort=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) > 0 
                //                    THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) ELSE 0	END

                //                    ,PresentExcess=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) < 0 
                //                    THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0))) ELSE 0	END
                //                    FROM [MST].[OperationMaster] OM
                //                    LEFT JOIN MST.MachineMaster MM ON MM.Id = OM.MachineMasterId
                //                    LEFT JOIN HKP.MachineCategory MC ON MC.Id = MM.MachineCategoryId
                //                    LEFT JOIN HKP.MachineSubCategory MSC ON MSC.Id = MM.MachineSubCategoryId
                //                    LEFT JOIN [HKP].[Skill] SKO ON SKO.Id = OM.SkillId
                //                    LEFT JOIN [HKP].[Skill] SKM ON SKM.Id = MM.SkillId
                //                    LEFT JOIN [HKP].[Process] P ON P.Id = OM.ProcessId
                //                    LEFT JOIN SCS.SkillGrouping SKG ON SKG.Id = OM.SkillGroupId
                //                    LEFT JOIN HKP.LegalDesignation OLDG ON OLDG.Id = OM.LegalDesignationId                
                //                    LEFT JOIN (
                //                    ---OperationPositionMPBudget
                //                    SELECT *
                //                    FROM (
                //                     SELECT OPM.OperationMasterId
                //                      ,OM.Code OperationCode
                //                      ,OM.UserName Operation
                //                      ,EN.Id EntityId
                //                      ,EN.Code EntityCode
                //                      ,EN.UserName EntityName
                //                      ,PS.Code PositionCode
                //                      ,PS.UserName PositionName
                //                      ,OPM.Caption
                //                      ,OPM.ManpowerBudget
                //                      ,PS.Id PositionId
                //                     FROM [MST].[OperationPositionMPBudget] OPM
                //                     LEFT JOIN [MST].[OperationMaster] OM ON OM.Id = OPM.OperationMasterId
                //                     LEFT JOIN ORG.Entity EN ON EN.Id = OPM.EntityId
                //                     LEFT JOIN ORG.Position PS ON PS.Id = OPM.PositionId
                //                     ) OPMB
                //                    ) OPMBF ON OPMBF.OperationMasterId = OM.Id
                //                    LEFT JOIN (
                //                    --[Total Present] ,[ONROLL]
                //                    SELECT EM.OperationMasterId
                //                     ,EN.Id EntityId
                //                     ,POS.Id PositionId
                //                     ,SUM([Status]) TotalPresent
                //                     ,COUNT(EM.SystemId) ONROLL
                //                    FROM EmployeeInformation EM
                //                    LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EM.BudgetCode
                //                    LEFT JOIN ORG.Entity EN ON EN.Id = MB.EntityId
                //                    LEFT JOIN ORG.Position POS ON POS.Id = MB.PositionId --EM.PositionId
                //                    LEFT JOIN (
                //                     SELECT DayStatus
                //                      ,EmpSystemID
                //                      ,[Status] = CASE 
                //                       WHEN DayStatus IS NULL
                //                        THEN 0
                //                       WHEN DayStatus IS NOT NULL
                //                        THEN 1
                //                       ELSE 0
                //                       END
                //                     FROM AttdnProcessData
                //                     WHERE DayStatus IN (
                //                       'LP'
                //                       ,'HDP'
                //                       ,'P'
                //                       )
                //                      AND WorkDate = REPLACE(Convert(varchar(11),getdate(),106),' ','-')
                //                     ) AD ON AD.EmpSystemID = EM.SystemId
                //                    WHERE EM.EmployeeStatus = 'Active'
                //                    GROUP BY EM.OperationMasterId
                //                     ,EN.Id
                //                     ,POS.Id
                //                    ) E ON E.OperationMasterId = OPMBF.OperationMasterId
                //                    AND E.PositionID = OPMBF.PositionId
                //                    AND E.EntityId = OPMBF.EntityId				
                //                    --ORDER BY CONVERT(INT, OM.Code)
                //    ) as Final";


                //Command By 18-Jul-2019

                //_sql = @"Select ISNULL(EntityId,0) EntityId, isNULL(EntityName,'') EntityName,ProcessId,Process,OperationId,OperationCode,OperationName,OperationCategoryId,OperationCategoryCode,OperationCategoryName,
                //        SkillId,Skill,SkillGroupID,SkillGroupe,MachineCategoryID,MachineCategory,MachineSubCategoryId,MachineSubCategory,ISNULL(Position,'') Position,ManpowerBudget,OnRoll,OnRollShort,OnRollExcess,TotalPresent,PresentShort,PresentExcess from
                //        (
                //        SELECT OM.Id OperationId, OM.Code OperationCode, OM.UserName OperationName, OM.Type, OM.OperationCategoryId, OC.Code OperationCategoryCode, OC.UserName OperationCategoryName,
                //        OPMBF.EntityId, OPMBF.EntityCode, OPMBF.EntityName, OPMBF.PositionCode, OPMBF.PositionName, OPMBF.Caption Position,
                //        MM.Code MachineCode, ISNULL(MM.UserName,'') MachineMaster, MC.Id MachineCategoryID, MC.Code MachineCategoryCode, ISNULL(MC.UserName, '') MachineCategory, MSC.Id MachineSubCategoryId, MSC.Code MachineSubCategoryCode, ISNULL(MSC.UserName, '') MachineSubCategory,
                //        Skill = CASE

                //            WHEN SKO.UserName IS NULL
                //                THEN SKM.UserName
                //            WHEN SKM.UserName IS NULL
                //                THEN SKO.UserName
                //            ELSE SKM.UserName
                //            END
                //            , SkillId = CASE

                //            WHEN SKO.id IS NULL
                //                THEN SKM.Id
                //            WHEN SKM.Id IS NULL
                //                THEN SKO.Id
                //            ELSE SKM.Id
                //            END
                //        ,SKG.Id SkillGroupID, SKG.Code SkillGroupingCode, SKG.Grouping SkillGroupe, SKG.DesignationCategory, SKG.StandardSalary,P.Id ProcessId, P.UserName Process, OLDG.UserName LegalDesignation, CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget) ManpowerBudget,
                //        ISNULL(E.OnRoll, 0) OnRoll, ISNULL(E.TotalPresent, 0) TotalPresent, 
                //        OnRollShort = CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0)) > 0
                //        THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0)) ELSE 0 END

                //        ,OnRollExcess = CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0)) < 0
                //        THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0))) ELSE 0    END

                //        ,PresentShort = CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0)) > 0
                //        THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0)) ELSE 0   END

                //        ,PresentExcess = CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0)) < 0
                //        THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0))) ELSE 0  END
                //            FROM[MST].[OperationMaster]
                //        OM
                //            LEFT JOIN MST.MachineMaster MM ON MM.Id = OM.MachineMasterId

                //            LEFT JOIN HKP.MachineCategory MC ON MC.Id = MM.MachineCategoryId

                //            LEFT JOIN HKP.MachineSubCategory MSC ON MSC.Id = MM.MachineSubCategoryId

                //            LEFT JOIN [HKP].[Skill] SKO ON SKO.Id = OM.SkillId

                //            LEFT JOIN [HKP].[Skill] SKM ON SKM.Id = MM.SkillId

                //            LEFT JOIN [HKP].[Process] P ON P.Id = OM.ProcessId

                //            LEFT JOIN SCS.SkillGrouping SKG ON SKG.Id = OM.SkillGroupId

                //            LEFT JOIN HKP.LegalDesignation OLDG ON OLDG.Id = OM.LegalDesignationId

                //            LEFT JOIN HKP.OperationCategory OC ON OC.Id = OM.OperationCategoryId

                //            LEFT JOIN (
                //            ---OperationPositionMPBudget
                //            SELECT *

                //            FROM (
                //                SELECT OPM.OperationMasterId

                //                    , OM.Code OperationCode
                //                    , OM.UserName Operation
                //                    , EN.Id EntityId
                //                    , EN.Code EntityCode
                //                    , EN.UserName EntityName
                //                    , PS.Code PositionCode
                //                    , PS.UserName PositionName
                //                    , OPM.Caption

                //                    , OPM.ManpowerBudget

                //                    , PS.Id PositionId

                //                FROM[MST].[OperationPositionMPBudget] OPM
                //                LEFT JOIN[MST].[OperationMaster] OM ON OM.Id = OPM.OperationMasterId

                //                LEFT JOIN ORG.Entity EN ON EN.Id = OPM.EntityId

                //                LEFT JOIN ORG.Position PS ON PS.Id = OPM.PositionId
                //                ) OPMB
                //        ) OPMBF ON OPMBF.OperationMasterId = OM.Id
                //        LEFT JOIN(
                //        --[Total Present] , [ONROLL]
                //        SELECT EM.OperationMasterId

                //            , EN.Id EntityId
                //            , POS.Id PositionId
                //            , SUM([Status]) TotalPresent
                //            , COUNT(EM.SystemId) ONROLL
                //        FROM EmployeeInformation EM
                //        LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EM.BudgetCode
                //        LEFT JOIN ORG.Entity EN ON EN.Id = MB.EntityId
                //        LEFT JOIN ORG.Position POS ON POS.Id = MB.PositionId--EM.PositionId
                //        LEFT JOIN (
                //            SELECT DayStatus
                //                , EmpSystemID
                //                ,[Status] = CASE

                //                    WHEN DayStatus IS NULL

                //                        THEN 0

                //                    WHEN DayStatus IS NOT NULL
                //                        THEN 1

                //                    ELSE 0

                //                    END
                //            FROM AttdnProcessData
                //            WHERE DayStatus IN (
                //           'LP'
                //           ,'HDP'
                //           ,'P'
                //           )

                //                AND WorkDate = REPLACE(Convert(varchar(11), getdate(), 106), ' ', '-')
                //         ) AD ON AD.EmpSystemID = EM.SystemId
                //        WHERE EM.EmployeeStatus = 'Active'
                //        GROUP BY EM.OperationMasterId
                //         ,EN.Id
                //         ,POS.Id
                //        ) E ON E.OperationMasterId = OPMBF.OperationMasterId
                //        AND E.PositionID = OPMBF.PositionId
                //        AND E.EntityId = OPMBF.EntityId				
                //        --ORDER BY CONVERT(INT, OM.Code)
                //        ) as Final";

                //End Command By 18-Jul-2019

                //Command Date 2/12/2019

                //_sql = @"SELECT OperationId
                //,OperationCode
                //,OperationName 
                //,OperationCategoryId
                //,OperationCategoryName
                //,MachineMasterId
                //,MachineMasterName MachineMaster
                //,MachineCategoryId
                //,MachineCategory
                //,MachineSubCategoryId
                //,MachineSubCategory
                //,SkillId
                //,Type
                //,Skill
                //,SkillGroupId
                //,SkillGroupe
                //,Position
                //,EntityId
                //,EntityName
                //,ProcessId
                //,ProcessName Process
                //,ManpowerBudget
                //,StandardSalary
                //,OnRoll
                //,OnRollShort
                //,OnRollExcess
                //,TotalPresent
                //,PresentShort
                //,PresentExcess
                //FROM (
                //SELECT OperationMaster.Id OperationId
                //,OperationMaster.Code OperationCode
                //,OperationMaster.UserName OperationName
                //,OperationMaster.OperationActivityId
                //,OperationMaster.Type
                //,OperationActivity.UserName OperationActivityName
                //,OperationMaster.OperationTypeId
                //,OperationType.UserName OperationTypeName
                //,OperationMaster.OperationCategoryId
                //,OperationCategory.UserName OperationCategoryName
                //,OperationMaster.Type OperationOrActivity
                //,ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId
                //,ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName
                //,ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId
                //,ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory
                //,ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId
                //,ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory
                //,SkillId = CASE 
                //WHEN OperationMaster.Type = 'Activity'
                //THEN OperationMaster.SkillId
                //ELSE MachineMaster.SkillId
                //END
                //,Skill = CASE 
                //WHEN OperationMaster.Type = 'Activity'
                //THEN Skill.UserName
                //ELSE MachineSkill.UserName
                //END
                //,
                //--OperationMaster.SkillId ActivitySkillId,Skill.UserName ActivitySkillName,MachineMaster.SkillId MachineSkillId,MachineSkill.UserName MachineSkillName,
                //OperationMaster.SkillGroupId
                //,SkillGrouping.UserName SkillGroupe
                //,SkillGrouping.StandardSalary
                //,OperationMaster.LegalDesignationId
                //,LegalDesignation.UserName LegalDesignationName
                //,OperationMaster.ProcessId
                //,Process.UserName ProcessName
                //,OperationMaster.ProposedSalary
                //,IsNull(OperationPositionMPBudget.EntityId, 'Blank') EntityId
                //,ISNULL(Entity.UserName, 'Blank') EntityName
                //,ISNULL(OperationPositionMPBudget.PositionId, 'Blank') PositionId
                //,ISNULL(Position.UserName, 'Blank') PositionName
                //,ISNULL(OperationPositionMPBudget.Caption, 'Blank') Position
                //,ISNULL(OperationPositionMPBudget.ManpowerBudget, 0) ManpowerBudget
                //,ISNULL(OnRoll.OnRollManpower, 0) OnRoll
                //,ISNULL(Present.DayPresentCount, 0) TotalPresent
                //,OnRollShort = CASE 
                //WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0
                //THEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))
                //ELSE 0
                //END
                //,OnRollExcess = CASE 
                //WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) < 0
                //THEN ABS(CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)))
                //ELSE 0
                //END
                //,PresentShort = CASE 
                //WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0
                //THEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))
                //ELSE 0
                //END
                //,PresentExcess = CASE 
                //WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0
                //THEN ABS(CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)))
                //ELSE 0
                //END
                //FROM MST.OperationMaster
                //LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
                //LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
                //LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
                //LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
                //LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
                //LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
                //LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
                //LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
                //LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
                //LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
                //LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
                //LEFT OUTER JOIN MST.OperationPositionMPBudget ON OperationPositionMPBudget.OperationMasterId = OperationMaster.Id
                //AND OperationPositionMPBudget.CompanyGroupId = OperationMaster.CompanyGroupId
                //LEFT OUTER JOIN ORG.Entity ON OperationPositionMPBudget.EntityId = Entity.Id
                //LEFT OUTER JOIN ORG.Position ON OperationPositionMPBudget.PositionId = Position.Id
                //LEFT OUTER JOIN (
                //SELECT ManpowerBudget.EntityId
                //,ManpowerBudget.PositionId
                //,ISNULL(OperationMasterId, '') OperationMasterId
                //,Count(EmployeeInformation.SystemId) OnRollManpower
                //FROM EmployeeInformation
                //LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //where EmployeeInformation.EmployeeStatus='Active'
                //GROUP BY ManpowerBudget.EntityId
                //,ManpowerBudget.PositionId
                //,OperationMasterId
                //) OnRoll ON OperationPositionMPBudget.EntityId = OnRoll.EntityId
                //AND OperationPositionMPBudget.PositionId = OnRoll.PositionId
                //AND OperationMaster.Id = OnRoll.OperationMasterId
                //LEFT OUTER JOIN (
                //SELECT ManpowerBudget.EntityId
                //,ManpowerBudget.PositionId
                //,ISNULL(OperationMasterId, '') OperationMasterId
                //,Count(EmployeeInformation.SystemId) DayPresentCount
                //FROM EmployeeInformation
                //LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
                //WHERE AttdnProcessData.DayStatus IN (
                //SELECT DayType
                //FROM DayType
                //WHERE Category = 'Present'
                // OR Category = 'Late'
                //)
                //AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
                //GROUP BY ManpowerBudget.EntityId
                //,ManpowerBudget.PositionId
                //,OperationMasterId
                //) Present ON OperationPositionMPBudget.EntityId = Present.EntityId
                //AND OperationPositionMPBudget.PositionId = Present.PositionId
                //AND OperationMaster.Id = Present.OperationMasterId
                //) Main";
                //Command Date 3/12/2019
                //_sql = @"SELECT OperationId, OperationCode, OperationName, OperationCategoryId, OperationCategoryName, MachineMasterId, MachineMasterName MachineMaster, MachineCategoryId, MachineCategory, MachineSubCategoryId, MachineSubCategory, SkillId, Type, Skill, SkillGroupId, SkillGroupe, 
                //--Position, 
                //EntityId, EntityName, ProcessId, ProcessName Process, ManpowerBudget, StandardSalary, OnRoll, OnRollShort, OnRollExcess, TotalPresent, PresentShort, PresentExcess
                //FROM(
                //	SELECT OperationMaster.Id OperationId, OperationMaster.Code OperationCode, OperationMaster.UserName OperationName, OperationMaster.OperationActivityId, OperationMaster.Type, OperationActivity.UserName OperationActivityName, OperationMaster.OperationTypeId, OperationType.UserName OperationTypeName, OperationMaster.OperationCategoryId, OperationCategory.UserName OperationCategoryName, OperationMaster.Type OperationOrActivity, ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId, ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory, SkillId = CASE WHEN OperationMaster.Type = 'Activity' THEN OperationMaster.SkillId ELSE MachineMaster.SkillId END, Skill = CASE WHEN OperationMaster.Type = 'Activity' THEN Skill.UserName ELSE MachineSkill.UserName END,
                //		--OperationMaster.SkillId ActivitySkillId, Skill.UserName ActivitySkillName, MachineMaster.SkillId MachineSkillId, MachineSkill.UserName MachineSkillName,
                //		OperationMaster.SkillGroupId, SkillGrouping.UserName SkillGroupe, SkillGrouping.StandardSalary, OperationMaster.LegalDesignationId, LegalDesignation.UserName LegalDesignationName, OperationMaster.ProcessId, Process.UserName ProcessName, OperationMaster.ProposedSalary, IsNull(OperationPositionMPBudget.EntityId, 'Blank') EntityId, ISNULL(Entity.UserName, 'Blank') EntityName,
                //		--ISNULL(OperationPositionMPBudget.PositionId, 'Blank') PositionId, ISNULL(Position.UserName, 'Blank') PositionName, ISNULL(OperationPositionMPBudget.Caption, 'Blank') Position,
                //		ISNULL(OperationPositionMPBudget.ManpowerBudget, 0) ManpowerBudget, ISNULL(OnRoll.OnRollManpower, 0) OnRoll, ISNULL(Present.DayPresentCount, 0) TotalPresent, OnRollShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) ELSE 0 END, OnRollExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(
                //						OnRoll.OnRollManpower, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))) ELSE 0 END, PresentShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) ELSE 0 END, PresentExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))) ELSE 0 END
                //	FROM MST.OperationMaster
                //	LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
                //	LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
                //	LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
                //	LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
                //	LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
                //	LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
                //	LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
                //	LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
                //	LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
                //	LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
                //	LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
                //	LEFT OUTER JOIN MST.OperationPositionMPBudget ON OperationPositionMPBudget.OperationMasterId = OperationMaster.Id
                //		AND OperationPositionMPBudget.CompanyGroupId = OperationMaster.CompanyGroupId
                //	LEFT OUTER JOIN ORG.Entity ON OperationPositionMPBudget.EntityId = Entity.Id
                //	--LEFT OUTER JOIN ORG.Position ON OperationPositionMPBudget.PositionId = Position.Id
                //	LEFT OUTER JOIN(
                //				--SELECT ManpowerBudget.EntityId, ManpowerBudget.PositionId, ISNULL(OperationMasterId, '') OperationMasterId
                //				--,Count(EmployeeInformation.SystemId) OnRollManpower
                //				--FROM EmployeeInformation
                //				--LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //				--where EmployeeInformation.EmployeeStatus='Active'
                //				--GROUP BY ManpowerBudget.EntityId, ManpowerBudget.PositionId, OperationMasterId
                //				Select B.EntityId,A.OperationMasterID,count(A.SystemId) OnRollManpower from EmployeeInformation A
                //					Left Outer Join MST.ManpowerBudget B on A.BudgetCode=B.Id
                //					Inner Join MST.OperationMaster C on A.OperationMasterID=C.Id
                //					Left Outer Join MST.OperationPositionMPBudget D on C.Id=D.OperationMasterId and B.EntityId=D.EntityId
                //					where D.Id is null
                //					group by B.EntityId,A.OperationMasterID
                //		) OnRoll ON OperationPositionMPBudget.EntityId = OnRoll.EntityId
                //		--AND OperationPositionMPBudget.PositionId = OnRoll.PositionId
                //		AND OperationMaster.Id = OnRoll.OperationMasterId
                //	LEFT OUTER JOIN(
                //		SELECT ManpowerBudget.EntityId, ManpowerBudget.PositionId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) DayPresentCount
                //		FROM EmployeeInformation
                //		LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //		LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
                //		WHERE AttdnProcessData.DayStatus IN(
                //				SELECT DayType
                //				FROM DayType
                //				WHERE Category = 'Present'
                //					OR Category = 'Late'
                //				)
                //			AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
                //		GROUP BY ManpowerBudget.EntityId, ManpowerBudget.PositionId, OperationMasterId
                //		) Present ON OperationPositionMPBudget.EntityId = Present.EntityId
                //		AND OperationPositionMPBudget.PositionId = Present.PositionId
                //		AND OperationMaster.Id = Present.OperationMasterId
                //	) Main";
                //_sql = @"SELECT OperationId, OperationCode, OperationName, OperationCategoryId, OperationCategoryName, MachineMasterId, MachineMasterName MachineMaster, MachineCategoryId, MachineCategory, MachineSubCategoryId, MachineSubCategory, SkillId, Type, Skill, SkillGroupId, SkillGroupe, 
                //			EntityId, EntityName, ProcessId, ProcessName Process, ManpowerBudget, StandardSalary, OnRoll, OnRollShort, OnRollExcess, TotalPresent, PresentShort, PresentExcess
                //			FROM (
                //				SELECT OperationMaster.Id OperationId, OperationMaster.Code OperationCode, OperationMaster.UserName OperationName, OperationMaster.OperationActivityId, OperationMaster.Type, OperationActivity.UserName OperationActivityName, OperationMaster.OperationTypeId, OperationType.UserName OperationTypeName, OperationMaster.OperationCategoryId, OperationCategory.UserName OperationCategoryName, OperationMaster.Type OperationOrActivity, ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId, ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory, SkillId = CASE WHEN OperationMaster.Type = 'Activity' THEN OperationMaster.SkillId ELSE MachineMaster.SkillId END, Skill = CASE WHEN OperationMaster.Type = 'Activity' THEN Skill.UserName ELSE MachineSkill.UserName END,
                //					OperationMaster.SkillGroupId, SkillGrouping.UserName SkillGroupe, SkillGrouping.StandardSalary, OperationMaster.LegalDesignationId, LegalDesignation.UserName LegalDesignationName, OperationMaster.ProcessId, Process.UserName ProcessName, OperationMaster.ProposedSalary, IsNull(OperationManpowerBudget.EntityId, 'Blank') EntityId, ISNULL(Entity.UserName, 'Blank') EntityName, 
                //					ISNULL(OperationManpowerBudget.ManpowerBudget, 0) ManpowerBudget, ISNULL(OnRoll.OnRollManpower, 0) OnRoll, ISNULL(Present.DayPresentCount, 0) TotalPresent, OnRollShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) ELSE 0 END, OnRollExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(
                //									OnRoll.OnRollManpower, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))) ELSE 0 END, PresentShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) ELSE 0 END, PresentExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))) ELSE 0 END
                //				FROM MST.OperationMaster
                //				LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
                //				LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
                //				LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
                //				LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
                //				LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
                //				LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
                //				LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
                //				LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
                //				LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
                //				LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
                //				LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
                //				LEFT OUTER JOIN (
                //					Select CompanyGroupId,EntityId,OperationMasterId,sum(ManpowerBudget) ManpowerBudget from mst.OperationPositionMPBudget group by CompanyGroupId,EntityId,OperationMasterId
                //					) OperationManpowerBudget on OperationManpowerBudget.OperationMasterId = OperationMaster.Id and OperationManpowerBudget.CompanyGroupId = OperationMaster.CompanyGroupId
                //				LEFT OUTER JOIN ORG.Entity ON OperationManpowerBudget.EntityId = Entity.Id	
                //				LEFT OUTER JOIN (
                //					SELECT ManpowerBudget.EntityId,ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) OnRollManpower
                //					FROM EmployeeInformation
                //					LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //					where EmployeeInformation.EmployeeStatus='Active'
                //					GROUP BY ManpowerBudget.EntityId,OperationMasterId
                //					) OnRoll ON OperationManpowerBudget.EntityId = OnRoll.EntityId AND OperationMaster.Id = OnRoll.OperationMasterId
                //				LEFT OUTER JOIN (
                //					SELECT ManpowerBudget.EntityId, ManpowerBudget.PositionId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) DayPresentCount
                //					FROM EmployeeInformation
                //					LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //					LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
                //					WHERE AttdnProcessData.DayStatus IN (
                //							SELECT DayType
                //							FROM DayType
                //							WHERE Category = 'Present'
                //								OR Category = 'Late'
                //							)
                //						AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
                //					GROUP BY ManpowerBudget.EntityId, ManpowerBudget.PositionId, OperationMasterId
                //					) Present ON OperationManpowerBudget.EntityId = Present.EntityId
                //					AND OperationMaster.Id = Present.OperationMasterId
                //				) Main";

                _sql = @"SELECT OperationId, OperationCode, OperationName, OperationCategoryId, OperationCategoryName, MachineMasterId, MachineMasterName MachineMaster, MachineCategoryId, MachineCategory, MachineSubCategoryId, MachineSubCategory, SkillId, Type, Skill, SkillGroupId, SkillGroupe, 
							EntityId, EntityName, ProcessId, ProcessName Process, ManpowerBudget, StandardSalary, OnRoll, OnRollShort, OnRollExcess, TotalPresent, PresentShort, PresentExcess
							FROM (
								SELECT OperationMaster.Id OperationId, OperationMaster.Code OperationCode, OperationMaster.UserName OperationName, OperationMaster.OperationActivityId, OperationMaster.Type, OperationActivity.UserName OperationActivityName, OperationMaster.OperationTypeId, OperationType.UserName OperationTypeName, OperationMaster.OperationCategoryId, OperationCategory.UserName OperationCategoryName, OperationMaster.Type OperationOrActivity, ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId, ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory, SkillId = CASE WHEN OperationMaster.Type = 'Activity' THEN OperationMaster.SkillId ELSE MachineMaster.SkillId END, Skill = CASE WHEN OperationMaster.Type = 'Activity' THEN Skill.UserName ELSE MachineSkill.UserName END,
									OperationMaster.SkillGroupId, SkillGrouping.UserName SkillGroupe, SkillGrouping.StandardSalary, OperationMaster.LegalDesignationId, LegalDesignation.UserName LegalDesignationName, OperationMaster.ProcessId, Process.UserName ProcessName, OperationMaster.ProposedSalary, IsNull(OperationManpowerBudget.EntityId, 'Blank') EntityId, ISNULL(Entity.UserName, 'Blank') EntityName, 
									ISNULL(OperationManpowerBudget.ManpowerBudget, 0) ManpowerBudget, ISNULL(OnRoll.OnRollManpower, 0) OnRoll, ISNULL(Present.DayPresentCount, 0) TotalPresent, OnRollShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) ELSE 0 END, OnRollExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(
													OnRoll.OnRollManpower, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))) ELSE 0 END, PresentShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) ELSE 0 END, PresentExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))) ELSE 0 END
								FROM MST.OperationMaster
								LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
								LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
								LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
								LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
								LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
								LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
								LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
								LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
								LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
								LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
								LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
								LEFT OUTER JOIN (
									Select CompanyGroupId,EntityId,OperationMasterId,sum(ManpowerBudget) ManpowerBudget from mst.OperationPositionMPBudget group by CompanyGroupId,EntityId,OperationMasterId
									) OperationManpowerBudget on OperationManpowerBudget.OperationMasterId = OperationMaster.Id and OperationManpowerBudget.CompanyGroupId = OperationMaster.CompanyGroupId
								LEFT OUTER JOIN ORG.Entity ON OperationManpowerBudget.EntityId = Entity.Id	
								LEFT OUTER JOIN (
									SELECT ManpowerBudget.EntityId,ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) OnRollManpower
									FROM EmployeeInformation
									LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
									where EmployeeInformation.EmployeeStatus='Active'
									GROUP BY ManpowerBudget.EntityId,OperationMasterId
									) OnRoll ON OperationManpowerBudget.EntityId = OnRoll.EntityId AND OperationMaster.Id = OnRoll.OperationMasterId
								LEFT OUTER JOIN (
									SELECT ManpowerBudget.EntityId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) DayPresentCount
									FROM EmployeeInformation
									LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
									LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
									WHERE AttdnProcessData.DayStatus IN (
											SELECT DayType
											FROM DayType
											WHERE Category = 'Present'
												OR Category = 'Late'
											)
										AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
									GROUP BY ManpowerBudget.EntityId, OperationMasterId
									) Present ON OperationManpowerBudget.EntityId = Present.EntityId
									AND OperationMaster.Id = Present.OperationMasterId
								) Main";


                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetSkillMasterDetail(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess)
        {
            try
            {
                string _sql = "";

                string paramters = "";
                //string parameterOutside = "";
                //if (queryString != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(OPMBF.EntityName,'') in(" + queryString + ")";
                //    else
                //        paramters += " AND ISNULL(OPMBF.EntityName,'') in(" + queryString + ")";
                //}

                //if (queryStringCaption != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(OPMBF.Caption,'') in(" + queryStringCaption + ")";
                //    else
                //        paramters += " AND ISNULL(OPMBF.Caption,'') in(" + queryStringCaption + ")";
                //}
                //if (queryStringSkill != "")
                //{
                //    if (parameterOutside == "")
                //        parameterOutside += " ISNULL(Skill,'') in(" + queryStringSkill + ")";
                //    else
                //        parameterOutside += " AND ISNULL(Skill,'') in(" + queryStringSkill + ")";
                //}
                //if (queryStringProcess != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(P.UserName,'') in(" + queryStringProcess + ")";
                //    else
                //        paramters += " AND ISNULL(P.UserName,'') in(" + queryStringProcess + ")";
                //}


                //if (queryStringGrouping != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(SKG.grouping,'') in(" + queryStringGrouping + ")";
                //    else
                //        paramters += " AND ISNULL(SKG.grouping,'') in(" + queryStringGrouping + ")";
                //}
                //if (queryStringMachineCategory != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(MC.UserName,'') in(" + queryStringMachineCategory + ")";
                //    else
                //        paramters += " AND ISNULL(MC.UserName,'') in(" + queryStringMachineCategory + ")";
                //}








                if (queryString != "")
                {
                    if (paramters == "")
                        paramters += "isnull(EntityId,'') in(" + queryString + ")";
                    else
                        paramters += " AND isnull(EntityId,'') in(" + queryString + ")";
                }

                //if (queryStringCaption != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(Position,'') in(" + queryStringCaption + ")";
                //    else
                //        paramters += " AND ISNULL(Position,'') in(" + queryStringCaption + ")";
                //}
                //if (queryStringSkill != "")
                //{
                //    if (paramters == "")
                //        paramters += "isnull(SkillId,'') in(" + queryStringSkill + ")";
                //    else
                //        paramters += " AND isnull(SkillId,'') in(" + queryStringSkill + ")";
                //}
                //if (queryStringOperationCode != "")
                //{
                //    if (paramters == "")
                //        paramters += "isnull(OperationCode,'') in(" + queryStringOperationCode + ")";
                //    else
                //        paramters += " AND isnull(OperationCode,'') in(" + queryStringOperationCode + ")";
                //}

                //if (queryStringProcess != "")
                //{
                //    if (paramters == "")
                //        paramters += " isnull(ProcessId,'') in(" + queryStringProcess + ")";
                //    else
                //        paramters += " AND isnull(ProcessId,'') in(" + queryStringProcess + ")";
                //}


                //if (queryStringGrouping != "")
                //{
                //    if (paramters == "")
                //        paramters += " isnull(SkillGroupID,'') in(" + queryStringGrouping + ")";
                //    else
                //        paramters += " AND isnull(SkillGroupID,'') in(" + queryStringGrouping + ")";
                //}
                //if (queryStringMachineCategory != "")
                //{
                //    if (paramters == "")
                //        paramters += " isnull(MachineCategoryID,'') in(" + queryStringMachineCategory + ")";
                //    else
                //        paramters += " AND isnull(MachineCategoryID,'') in(" + queryStringMachineCategory + ")";
                //}
                //if (queryStringMachineSubCategoryCode != "")
                //{
                //    if (paramters == "")
                //        paramters += " isnull(MachineSubCategoryId,'') in(" + queryStringMachineSubCategoryCode + ")";
                //    else
                //        paramters += " AND isnull(MachineSubCategoryId,'') in(" + queryStringMachineSubCategoryCode + ")";
                //}
                //if (queryStringOperationCategoryId != "")
                //{
                //    if (paramters == "")
                //        paramters += " isnull(OperationCategoryId,'') in(" + queryStringOperationCategoryId + ")";
                //    else
                //        paramters += " AND isnull(OperationCategoryId,'') in(" + queryStringOperationCategoryId + ")";
                //}
                //if (queryStringOnRoll != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(OnRoll,0)  in(" + queryStringOnRoll + ")";
                //    else
                //        paramters += " AND ISNULL(OnRoll,0)  in(" + queryStringOnRoll + ")";
                //}
                //if (queryStringTotalPresent != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(TotalPresent,0)  in(" + queryStringTotalPresent + ")";
                //    else
                //        paramters += " AND ISNULL(TotalPresent,0)  in(" + queryStringTotalPresent + ")";
                //}

                //if (queryStringOnRollShort != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(OnRollShort,0) in(" + queryStringOnRollShort + ")";
                //    else
                //        paramters += " AND ISNULL(OnRollShort,0) in(" + queryStringOnRollShort + ")";
                //}
                //if (queryStringOnRollExcess != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(OnRollExcess,0) in(" + queryStringOnRollExcess + ")";
                //    else
                //        paramters += " AND ISNULL(OnRollExcess,0) in(" + queryStringOnRollExcess + ")";
                //}
                //if (queryStringPresentShort != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(PresentShort,0) in(" + queryStringPresentShort + ")";
                //    else
                //        paramters += " AND ISNULL(PresentShort,0) in(" + queryStringPresentShort + ")";
                //}
                //if (queryStringPresentExcess != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(presentExcess,0) in(" + queryStringPresentExcess + ")";
                //    else
                //        paramters += " AND ISNULL(presentExcess,0) in(" + queryStringPresentExcess + ")";
                //}

                //_sql = @"select *,OnRollBalance = CASE 
                //              WHEN isnull(OnRollShort,0) > 0 THEN isnull(OnRollShort,0)*-1 ELSE isnull(OnRollExcess,0)			                          
                //              END,	                            
                //                PresentBalance = CASE 
                //              WHEN isnull(PresentShort,0) > 0 THEN isnull(PresentShort,0)*-1 ELSE isnull(PresentExcess,0)			                          
                //              END 
                //                from (SELECT P.UserName Process
                //             ,Skill = CASE 
                //              WHEN SKO.UserName IS NULL
                //               THEN SKM.UserName
                //              WHEN SKM.UserName IS NULL
                //               THEN SKO.UserName
                //              ELSE SKM.UserName
                //              END
                //             ,MM.UserName MachineMaster
                //             ,SKG.Code SkillGroupingCode
                //             ,OM.Type
                //             ,SKG.StandardSalary
                //             ,CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget) ManpowerBudget
                //             ,ISNULL(E.OnRoll, 0) OnRoll
                //             ,ISNULL(E.TotalPresent, 0) TotalPresent
                //             ,OnRollShort = CASE 
                //              WHEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0)) > 0
                //               THEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0))
                //              ELSE 0
                //              END
                //             ,OnRollExcess = CASE 
                //              WHEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0)) < 0
                //               THEN ABS(CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0)))
                //              ELSE 0
                //              END
                //             ,PresentShort = CASE 
                //              WHEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0)) > 0
                //               THEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0))
                //              ELSE 0
                //              END
                //             ,PresentExcess = CASE 
                //              WHEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0)) < 0
                //               THEN ABS(CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0)))
                //              ELSE 0
                //              END,
                //            OM.Code OerationCode
                //            ,OM.UserName OperationName
                //            --,OPMBF.EntityCode
                //            --,OPMBF.EntityName
                //            -- ,OPMBF.PositionCode
                //            --,OPMBF.PositionName
                //            --,OPMBF.Caption
                //            --,MM.Code MachineCode
                //            --,MC.Code MachineCategoryCode
                //            --,MC.UserName MachineCategory
                //            --,MSC.Code MachineSubCategoryCode
                //            --,MSC.UserName MachineSubCategory
                //            --,SKG.Grouping
                //            -- ,SKG.DesignationCategory
                //            --,OLDG.UserName LegalDesignation	                          
                //            FROM [MST].[OperationMaster] OM
                //            LEFT JOIN MST.MachineMaster MM ON MM.Id = OM.MachineMasterId
                //            LEFT JOIN HKP.MachineCategory MC ON MC.Id = MM.MachineCategoryId
                //            LEFT JOIN HKP.MachineSubCategory MSC ON MSC.Id = MM.MachineSubCategoryId
                //            LEFT JOIN [HKP].[Skill] SKO ON SKO.Id = OM.SkillId
                //            LEFT JOIN [HKP].[Skill] SKM ON SKM.Id = MM.SkillId
                //            LEFT JOIN [HKP].[SkillProcess] SP ON SP.SkillId = SKO.Id
                //            LEFT JOIN [HKP].[Process] P ON P.Id = OM.ProcessId
                //            LEFT JOIN SCS.SkillGrouping SKG ON SKG.Id = OM.SkillGroupId
                //            LEFT JOIN HKP.LegalDesignation OLDG ON OLDG.Id = OM.LegalDesignationId
                //            LEFT JOIN (
                //             ----EmployeeInformation
                //             SELECT e.OperationMasterId
                //              ,Count(convert(INT, E.SystemId)) AS [TOTAL ONROLL]
                //             FROM EmployeeInformation e
                //             WHERE E.EmployeeStatus = 'Active'
                //             GROUP BY e.OperationMasterId
                //             ) ONOP ON ONOP.OperationMasterId = OM.Id
                //            LEFT JOIN (
                //             ---OperationPositionMPBudget
                //             SELECT *
                //             FROM (
                //              SELECT OPM.OperationMasterId
                //               ,OM.Code OperationCode
                //               ,OM.UserName Operation
                //               ,EN.Id EntityId
                //               ,EN.Code EntityCode
                //               ,EN.UserName EntityName
                //               ,PS.Code PositionCode
                //               ,PS.UserName PositionName
                //               ,OPM.Caption
                //               ,OPM.ManpowerBudget
                //               ,PS.Id PositionId
                //              FROM [MST].[OperationPositionMPBudget] OPM
                //              LEFT JOIN [MST].[OperationMaster] OM ON OM.Id = OPM.OperationMasterId
                //              LEFT JOIN ORG.Entity EN ON EN.Id = OPM.EntityId
                //              LEFT JOIN ORG.Position PS ON PS.Id = OPM.PositionId
                //              ) OPMB
                //             ) OPMBF ON OPMBF.OperationMasterId = OM.Id
                //            LEFT JOIN (
                //             --[Total Present] ,[ONROLL]
                //             SELECT EM.OperationMasterId
                //              ,EN.Id EntityId
                //              ,POS.Id PositionId
                //              ,SUM([Status]) TotalPresent
                //              ,COUNT(EM.SystemId) ONROLL
                //             FROM EmployeeInformation EM
                //             LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EM.BudgetCode
                //             LEFT JOIN ORG.Entity EN ON EN.Id = MB.EntityId
                //             LEFT JOIN ORG.Position POS ON POS.Id = EM.PositionId
                //             LEFT JOIN (
                //              SELECT DayStatus
                //               ,EmpSystemID
                //               ,[Status] = CASE 
                //                WHEN DayStatus IS NULL
                //                 THEN 0
                //                WHEN DayStatus IS NOT NULL
                //                 THEN 1
                //                ELSE 0
                //                END
                //              FROM AttdnProcessData
                //              WHERE DayStatus IN (
                //                'LP'
                //                ,'HDP'
                //                ,'P'
                //                )
                //               AND WorkDate = CONVERT(VARCHAR(10), getdate(), 105)
                //              ) AD ON AD.EmpSystemID = EM.SystemId
                //             WHERE EM.EmployeeStatus = 'Active'
                //             GROUP BY EM.OperationMasterId
                //              ,EN.Id
                //              ,POS.Id
                //             ) E ON E.OperationMasterId = OPMBF.OperationMasterId
                //             AND E.PositionID = OPMBF.PositionId
                //             AND E.EntityId = OPMBF.EntityId
                //                    where  " + paramters + @"
                //                    ) AS K
                //            WHERE " + parameterOutside + " ORDER BY CONVERT(INT, OerationCode)";
                //_sql = @"Select Process,OperationCode,OperationName,Skill,SkillGroupe,MachineMaster,Type,StandardSalary,ManpowerBudget,OnRoll,OnRollShort,OnRollExcess,TotalPresent,PresentShort,PresentExcess,EntityName,MachineCategory,MachineSubCategory,Position from 
                //            (
                //            SELECT OM.Id OperationId, OM.Code OperationCode, OM.UserName OperationName,OM.Type, OPMBF.EntityId, OPMBF.EntityCode, OPMBF.EntityName, OPMBF.PositionCode, OPMBF.PositionName, OPMBF.Caption Position, 
                //            MM.Code MachineCode, ISNULL(MM.UserName,'') MachineMaster, MC.Id MachineCategoryID, MC.Code MachineCategoryCode, ISNULL(MC.UserName,'') MachineCategory, MSC.Id MachineSubCategoryId, MSC.Code MachineSubCategoryCode, ISNULL(MSC.UserName,'') MachineSubCategory,
                //            Skill = CASE 
                //                            WHEN SKO.UserName IS NULL
                //                              THEN SKM.UserName
                //                            WHEN SKM.UserName IS NULL
                //                             THEN SKO.UserName
                //                            ELSE SKM.UserName
                //                             END
                //                             ,SkillId = CASE 
                //                             WHEN SKO.id IS NULL
                //                              THEN SKM.Id
                //                             WHEN SKM.Id IS NULL
                //                              THEN SKO.Id
                //                             ELSE SKM.Id
                //                             END  
                //            ,SKG.Id SkillGroupID,SKG.Code SkillGroupingCode, SKG.Grouping SkillGroupe, SKG.DesignationCategory, SKG.StandardSalary,P.Id ProcessId, P.UserName Process, OLDG.UserName LegalDesignation, CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget) ManpowerBudget,
                //            ISNULL(E.OnRoll,0) OnRoll, ISNULL(E.TotalPresent,0) TotalPresent, 
                //            OnRollShort=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) > 0 
                //                            THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) ELSE 0	END

                //                            ,OnRollExcess=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) < 0 
                //                            THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0))) ELSE 0	END

                //                            ,PresentShort=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) > 0 
                //                            THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) ELSE 0	END

                //                            ,PresentExcess=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) < 0 
                //                            THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0))) ELSE 0	END
                //                            FROM [MST].[OperationMaster] OM
                //                            LEFT JOIN MST.MachineMaster MM ON MM.Id = OM.MachineMasterId
                //                            LEFT JOIN HKP.MachineCategory MC ON MC.Id = MM.MachineCategoryId
                //                            LEFT JOIN HKP.MachineSubCategory MSC ON MSC.Id = MM.MachineSubCategoryId
                //                            LEFT JOIN [HKP].[Skill] SKO ON SKO.Id = OM.SkillId
                //                            LEFT JOIN [HKP].[Skill] SKM ON SKM.Id = MM.SkillId
                //                            LEFT JOIN [HKP].[Process] P ON P.Id = OM.ProcessId
                //                            LEFT JOIN SCS.SkillGrouping SKG ON SKG.Id = OM.SkillGroupId
                //                            LEFT JOIN HKP.LegalDesignation OLDG ON OLDG.Id = OM.LegalDesignationId                
                //                            LEFT JOIN (
                //                            ---OperationPositionMPBudget
                //                            SELECT *
                //                            FROM (
                //                             SELECT OPM.OperationMasterId
                //                              ,OM.Code OperationCode
                //                              ,OM.UserName Operation
                //                              ,EN.Id EntityId
                //                              ,EN.Code EntityCode
                //                              ,EN.UserName EntityName
                //                              ,PS.Code PositionCode
                //                              ,PS.UserName PositionName
                //                              ,OPM.Caption
                //                              ,OPM.ManpowerBudget
                //                              ,PS.Id PositionId
                //                             FROM [MST].[OperationPositionMPBudget] OPM
                //                             LEFT JOIN [MST].[OperationMaster] OM ON OM.Id = OPM.OperationMasterId
                //                             LEFT JOIN ORG.Entity EN ON EN.Id = OPM.EntityId
                //                             LEFT JOIN ORG.Position PS ON PS.Id = OPM.PositionId
                //                             ) OPMB
                //                            ) OPMBF ON OPMBF.OperationMasterId = OM.Id
                //                            LEFT JOIN (
                //                            --[Total Present] ,[ONROLL]
                //                            SELECT EM.OperationMasterId
                //                             ,EN.Id EntityId
                //                             ,POS.Id PositionId
                //                             ,SUM([Status]) TotalPresent
                //                             ,COUNT(EM.SystemId) ONROLL
                //                            FROM EmployeeInformation EM
                //                            LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EM.BudgetCode
                //                            LEFT JOIN ORG.Entity EN ON EN.Id = MB.EntityId
                //                            LEFT JOIN ORG.Position POS ON POS.Id = MB.PositionId --EM.PositionId
                //                            LEFT JOIN (
                //                             SELECT DayStatus
                //                              ,EmpSystemID
                //                              ,[Status] = CASE 
                //                               WHEN DayStatus IS NULL
                //                                THEN 0
                //                               WHEN DayStatus IS NOT NULL
                //                                THEN 1
                //                               ELSE 0
                //                               END
                //                             FROM AttdnProcessData
                //                             WHERE DayStatus IN (
                //                               'LP'
                //                               ,'HDP'
                //                               ,'P'
                //                               )
                //                              AND WorkDate = REPLACE(Convert(varchar(11),getdate(),106),' ','-')
                //                             ) AD ON AD.EmpSystemID = EM.SystemId
                //                            WHERE EM.EmployeeStatus = 'Active'
                //                            GROUP BY EM.OperationMasterId
                //                             ,EN.Id
                //                             ,POS.Id
                //                            ) E ON E.OperationMasterId = OPMBF.OperationMasterId
                //                            AND E.PositionID = OPMBF.PositionId
                //                            AND E.EntityId = OPMBF.EntityId				
                //                            --ORDER BY CONVERT(INT, OM.Code)
                //                            ) as Final
                //                           where " + paramters + "";


                //Command date 18-Jul-2019

                //_sql = @"Select Process,OperationCode,OperationName,Skill,OperationCategoryName, SkillGroupe,MachineMaster,Type,StandardSalary,ManpowerBudget,OnRoll,OnRollShort,OnRollExcess,TotalPresent,PresentShort,PresentExcess,EntityName,MachineCategory,MachineSubCategory,Position from 
                //            (
                //            SELECT OM.Id OperationId, OM.Code OperationCode, OM.UserName OperationName,OM.Type, OM.OperationCategoryId, OC.Code OperationCategoryCode, OC.UserName OperationCategoryName,
                //            OPMBF.EntityId, OPMBF.EntityCode, OPMBF.EntityName, OPMBF.PositionCode, OPMBF.PositionName, OPMBF.Caption Position, 
                //            MM.Code MachineCode, ISNULL(MM.UserName,'') MachineMaster, MC.Id MachineCategoryID, MC.Code MachineCategoryCode, ISNULL(MC.UserName,'') MachineCategory, MSC.Id MachineSubCategoryId, MSC.Code MachineSubCategoryCode, ISNULL(MSC.UserName,'') MachineSubCategory,
                //            Skill = CASE 
                //             WHEN SKO.UserName IS NULL
                //              THEN SKM.UserName
                //             WHEN SKM.UserName IS NULL
                //              THEN SKO.UserName
                //             ELSE SKM.UserName
                //             END
                //                ,SkillId = CASE 
                //             WHEN SKO.id IS NULL
                //              THEN SKM.Id
                //             WHEN SKM.Id IS NULL
                //              THEN SKO.Id
                //             ELSE SKM.Id
                //             END  
                //            ,SKG.Id SkillGroupID,SKG.Code SkillGroupingCode, SKG.Grouping SkillGroupe, SKG.DesignationCategory, SKG.StandardSalary,P.Id ProcessId, P.UserName Process, OLDG.UserName LegalDesignation, CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget) ManpowerBudget,
                //            ISNULL(E.OnRoll,0) OnRoll, ISNULL(E.TotalPresent,0) TotalPresent, 
                //            OnRollShort=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) > 0 
                //            THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) ELSE 0	END

                //            ,OnRollExcess=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) < 0 
                //            THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0))) ELSE 0	END

                //            ,PresentShort=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) > 0 
                //            THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) ELSE 0	END

                //            ,PresentExcess=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) < 0 
                //            THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0))) ELSE 0	END
                //            FROM [MST].[OperationMaster] OM
                //            LEFT JOIN MST.MachineMaster MM ON MM.Id = OM.MachineMasterId
                //            LEFT JOIN HKP.MachineCategory MC ON MC.Id = MM.MachineCategoryId
                //            LEFT JOIN HKP.MachineSubCategory MSC ON MSC.Id = MM.MachineSubCategoryId
                //            LEFT JOIN [HKP].[Skill] SKO ON SKO.Id = OM.SkillId
                //            LEFT JOIN [HKP].[Skill] SKM ON SKM.Id = MM.SkillId
                //            LEFT JOIN [HKP].[Process] P ON P.Id = OM.ProcessId
                //            LEFT JOIN SCS.SkillGrouping SKG ON SKG.Id = OM.SkillGroupId
                //            LEFT JOIN HKP.LegalDesignation OLDG ON OLDG.Id = OM.LegalDesignationId
                //            LEFT JOIN HKP.OperationCategory OC ON OC.Id = OM.OperationCategoryId                
                //            LEFT JOIN (
                //            ---OperationPositionMPBudget
                //            SELECT *
                //            FROM (
                //             SELECT OPM.OperationMasterId
                //              ,OM.Code OperationCode
                //              ,OM.UserName Operation
                //              ,EN.Id EntityId
                //              ,EN.Code EntityCode
                //              ,EN.UserName EntityName
                //              ,PS.Code PositionCode
                //              ,PS.UserName PositionName
                //              ,OPM.Caption
                //              ,OPM.ManpowerBudget
                //              ,PS.Id PositionId
                //             FROM [MST].[OperationPositionMPBudget] OPM
                //             LEFT JOIN [MST].[OperationMaster] OM ON OM.Id = OPM.OperationMasterId
                //             LEFT JOIN ORG.Entity EN ON EN.Id = OPM.EntityId
                //             LEFT JOIN ORG.Position PS ON PS.Id = OPM.PositionId
                //             ) OPMB
                //            ) OPMBF ON OPMBF.OperationMasterId = OM.Id
                //            LEFT JOIN (
                //            --[Total Present] ,[ONROLL]
                //            SELECT EM.OperationMasterId
                //             ,EN.Id EntityId
                //             ,POS.Id PositionId
                //             ,SUM([Status]) TotalPresent
                //             ,COUNT(EM.SystemId) ONROLL
                //            FROM EmployeeInformation EM
                //            LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EM.BudgetCode
                //            LEFT JOIN ORG.Entity EN ON EN.Id = MB.EntityId
                //            LEFT JOIN ORG.Position POS ON POS.Id = MB.PositionId --EM.PositionId
                //            LEFT JOIN (
                //             SELECT DayStatus
                //              ,EmpSystemID
                //              ,[Status] = CASE 
                //               WHEN DayStatus IS NULL
                //                THEN 0
                //               WHEN DayStatus IS NOT NULL
                //                THEN 1
                //               ELSE 0
                //               END
                //             FROM AttdnProcessData
                //             WHERE DayStatus IN (
                //               'LP'
                //               ,'HDP'
                //               ,'P'
                //               )
                //              AND WorkDate = REPLACE(Convert(varchar(11),getdate(),106),' ','-')
                //             ) AD ON AD.EmpSystemID = EM.SystemId
                //            WHERE EM.EmployeeStatus = 'Active'
                //            GROUP BY EM.OperationMasterId
                //             ,EN.Id
                //             ,POS.Id
                //            ) E ON E.OperationMasterId = OPMBF.OperationMasterId
                //            AND E.PositionID = OPMBF.PositionId
                //            AND E.EntityId = OPMBF.EntityId				
                //            --ORDER BY CONVERT(INT, OM.Code)
                //            ) as Final
                //            where " + paramters + "";

                //END Command date 18-Jul-2019
                //Command Date 2/12/2019
                //_sql = @"SELECT OperationId
                //         ,OperationCode
                //         ,OperationName
                //         ,OperationCategoryId
                //         ,OperationCategoryName
                //         ,MachineMasterId
                //         ,MachineMasterName MachineMaster
                //         ,MachineCategoryId
                //         ,MachineCategory
                //         --,MachineSubCategoryId
                //         --,MachineSubCategory
                //         ,SkillId
                //         ,Type
                //         ,Skill
                //         ,SkillGroupId
                //         ,SkillGroupe
                //         --,Position
                //         --,EntityId
                //         --,EntityName
                //         ,ProcessId
                //         ,ProcessName Process
                //         ,Sum(StandardSalary) StandardSalary
                //         ,Sum(ManpowerBudget) ManpowerBudget
                //         ,Sum(OnRoll) OnRoll
                //         ,Sum(OnRollShort) OnRollShort
                //         ,Sum(OnRollExcess) OnRollExcess
                //         ,Sum(TotalPresent) TotalPresent
                //         ,Sum(PresentShort) PresentShort
                //         ,Sum(PresentExcess) PresentExcess
                //        FROM (
                //         SELECT OperationId
                //          ,OperationCode
                //          ,OperationName
                //          ,OperationCategoryId
                //          ,OperationCategoryName
                //          ,MachineMasterId
                //          ,MachineMasterName
                //          ,MachineCategoryId
                //          ,MachineCategory
                //          ,MachineSubCategoryId
                //          ,MachineSubCategory
                //          ,SkillId
                //          ,Type
                //          ,Skill
                //          ,SkillGroupId
                //          ,SkillGroupe
                //          ,Position
                //          ,EntityId
                //          ,EntityName
                //          ,ProcessId
                //          ,ProcessName
                //          ,StandardSalary
                //          ,ManpowerBudget
                //          ,OnRoll
                //          ,OnRollShort
                //          ,OnRollExcess
                //          ,TotalPresent
                //          ,PresentShort
                //          ,PresentExcess
                //         FROM (
                //          SELECT OperationMaster.Id OperationId
                //           ,OperationMaster.Code OperationCode
                //           ,OperationMaster.UserName OperationName
                //           ,OperationMaster.Type
                //           ,OperationMaster.OperationActivityId
                //           ,OperationActivity.UserName OperationActivityName
                //           ,OperationMaster.OperationTypeId
                //           ,OperationType.UserName OperationTypeName
                //           ,OperationMaster.OperationCategoryId
                //           ,OperationCategory.UserName OperationCategoryName
                //           ,OperationMaster.Type OperationOrActivity
                //           ,ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId
                //           ,ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName
                //           ,ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId
                //           ,ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory
                //           ,ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId
                //           ,ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory
                //           ,SkillId = CASE 
                //            WHEN OperationMaster.Type = 'Activity'
                //             THEN OperationMaster.SkillId
                //            ELSE MachineMaster.SkillId
                //            END
                //           ,Skill = CASE 
                //            WHEN OperationMaster.Type = 'Activity'
                //             THEN Skill.UserName
                //            ELSE MachineSkill.UserName
                //            END
                //           ,
                //           --OperationMaster.SkillId ActivitySkillId,Skill.UserName ActivitySkillName,MachineMaster.SkillId MachineSkillId,MachineSkill.UserName MachineSkillName,
                //           OperationMaster.SkillGroupId
                //           ,SkillGrouping.UserName SkillGroupe
                //           ,SkillGrouping.StandardSalary
                //           ,OperationMaster.LegalDesignationId
                //           ,LegalDesignation.UserName LegalDesignationName
                //           ,OperationMaster.ProcessId
                //           ,Process.UserName ProcessName
                //           ,OperationMaster.ProposedSalary
                //           ,IsNull(OperationPositionMPBudget.EntityId, 'Blank') EntityId
                //           ,ISNULL(Entity.UserName, 'Blank') EntityName
                //           ,ISNULL(OperationPositionMPBudget.PositionId, 'Blank') PositionId
                //           ,ISNULL(Position.UserName, 'Blank') PositionName
                //           ,ISNULL(OperationPositionMPBudget.Caption, 'Blank') Position
                //           ,ISNULL(OperationPositionMPBudget.ManpowerBudget, 0) ManpowerBudget
                //           ,ISNULL(OnRoll.OnRollManpower, 0) OnRoll
                //           ,ISNULL(Present.DayPresentCount, 0) TotalPresent
                //           ,OnRollShort = CASE 
                //            WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0
                //             THEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))
                //            ELSE 0
                //            END
                //           ,OnRollExcess = CASE 
                //            WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) < 0
                //             THEN ABS(CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)))
                //            ELSE 0
                //            END
                //           ,PresentShort = CASE 
                //            WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0
                //             THEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))
                //            ELSE 0
                //            END
                //           ,PresentExcess = CASE 
                //            WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0
                //             THEN ABS(CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)))
                //            ELSE 0
                //            END
                //          FROM MST.OperationMaster
                //          LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
                //          LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
                //          LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
                //          LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
                //          LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
                //          LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
                //          LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
                //          LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
                //          LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
                //          LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
                //          LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
                //          LEFT OUTER JOIN MST.OperationPositionMPBudget ON OperationPositionMPBudget.OperationMasterId = OperationMaster.Id
                //           AND OperationPositionMPBudget.CompanyGroupId = OperationMaster.CompanyGroupId
                //          LEFT OUTER JOIN ORG.Entity ON OperationPositionMPBudget.EntityId = Entity.Id
                //          LEFT OUTER JOIN ORG.Position ON OperationPositionMPBudget.PositionId = Position.Id
                //          LEFT OUTER JOIN (
                //           SELECT ManpowerBudget.EntityId
                //            ,ManpowerBudget.PositionId
                //            ,ISNULL(OperationMasterId, '') OperationMasterId
                //            ,Count(EmployeeInformation.SystemId) OnRollManpower
                //           FROM EmployeeInformation
                //           LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //where EmployeeInformation.EmployeeStatus='Active'

                //           GROUP BY ManpowerBudget.EntityId
                //            ,ManpowerBudget.PositionId
                //            ,OperationMasterId
                //           ) OnRoll ON OperationPositionMPBudget.EntityId = OnRoll.EntityId
                //           AND OperationPositionMPBudget.PositionId = OnRoll.PositionId
                //           AND OperationMaster.Id = OnRoll.OperationMasterId
                //          LEFT OUTER JOIN (
                //           SELECT ManpowerBudget.EntityId
                //            ,ManpowerBudget.PositionId
                //            ,ISNULL(OperationMasterId, '') OperationMasterId
                //            ,Count(EmployeeInformation.SystemId) DayPresentCount
                //           FROM EmployeeInformation
                //           LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //           LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
                //           WHERE AttdnProcessData.DayStatus IN (
                //             SELECT DayType
                //             FROM DayType
                //             WHERE Category = 'Present'
                //              OR Category = 'Late'
                //             )
                //            AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
                //           GROUP BY ManpowerBudget.EntityId
                //            ,ManpowerBudget.PositionId
                //            ,OperationMasterId
                //           ) Present ON OperationPositionMPBudget.EntityId = Present.EntityId
                //           AND OperationPositionMPBudget.PositionId = Present.PositionId
                //           AND OperationMaster.Id = Present.OperationMasterId
                //          ) Main
                //         ) xyz
                //        where " + paramters + "" +
                //        "GROUP BY OperationId, OperationCode, OperationName , OperationCategoryId , OperationCategoryName , MachineMasterId , MachineMasterName  , MachineCategoryId , MachineCategory,SkillId,Type,Skill,SkillGroupId,SkillGroupe,ProcessId,ProcessName";
                //3/12/2019
                //_sql = @"SELECT OperationId
                //                     ,OperationCode
                //                     ,OperationName
                //                     ,OperationCategoryId
                //                     ,OperationCategoryName
                //                     ,MachineMasterId
                //                     ,MachineMaster MachineMaster
                //                     ,MachineCategoryId
                //                     ,MachineCategory
                //                     --,MachineSubCategoryId
                //                     --,MachineSubCategory
                //                     ,SkillId
                //                     ,Type
                //                     ,Skill
                //                     ,SkillGroupId
                //                     ,SkillGroupe
                //                     --,Position
                //                     --,EntityId
                //                     --,EntityName
                //                     ,ProcessId
                //                     ,Process Process
                //                     ,Sum(StandardSalary) StandardSalary
                //                     ,Sum(ManpowerBudget) ManpowerBudget
                //                     ,Sum(OnRoll) OnRoll
                //                     ,Sum(OnRollShort) OnRollShort
                //                     ,Sum(OnRollExcess) OnRollExcess
                //                     ,Sum(TotalPresent) TotalPresent
                //                     ,Sum(PresentShort) PresentShort
                //                     ,Sum(PresentExcess) PresentExcess
                //                    FROM (
                //                     SELECT OperationId, OperationCode, OperationName, OperationCategoryId, OperationCategoryName, MachineMasterId, MachineMasterName MachineMaster, MachineCategoryId, MachineCategory, MachineSubCategoryId, MachineSubCategory, SkillId, Type, Skill, SkillGroupId, SkillGroupe, 
                //				--Position, 
                //				EntityId, EntityName, ProcessId, ProcessName Process, ManpowerBudget, StandardSalary, OnRoll, OnRollShort, OnRollExcess, TotalPresent, PresentShort, PresentExcess
                //				FROM (
                //					SELECT OperationMaster.Id OperationId, OperationMaster.Code OperationCode, OperationMaster.UserName OperationName, OperationMaster.OperationActivityId, OperationMaster.Type, OperationActivity.UserName OperationActivityName, OperationMaster.OperationTypeId, OperationType.UserName OperationTypeName, OperationMaster.OperationCategoryId, OperationCategory.UserName OperationCategoryName, OperationMaster.Type OperationOrActivity, ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId, ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory, SkillId = CASE WHEN OperationMaster.Type = 'Activity' THEN OperationMaster.SkillId ELSE MachineMaster.SkillId END, Skill = CASE WHEN OperationMaster.Type = 'Activity' THEN Skill.UserName ELSE MachineSkill.UserName END,
                //						--OperationMaster.SkillId ActivitySkillId,Skill.UserName ActivitySkillName,MachineMaster.SkillId MachineSkillId,MachineSkill.UserName MachineSkillName,
                //						OperationMaster.SkillGroupId, SkillGrouping.UserName SkillGroupe, SkillGrouping.StandardSalary, OperationMaster.LegalDesignationId, LegalDesignation.UserName LegalDesignationName, OperationMaster.ProcessId, Process.UserName ProcessName, OperationMaster.ProposedSalary, IsNull(OperationPositionMPBudget.EntityId, 'Blank') EntityId, ISNULL(Entity.UserName, 'Blank') EntityName, 
                //						--ISNULL(OperationPositionMPBudget.PositionId, 'Blank') PositionId, ISNULL(Position.UserName, 'Blank') PositionName, ISNULL(OperationPositionMPBudget.Caption, 'Blank') Position, 
                //						ISNULL(OperationPositionMPBudget.ManpowerBudget, 0) ManpowerBudget, ISNULL(OnRoll.OnRollManpower, 0) OnRoll, ISNULL(Present.DayPresentCount, 0) TotalPresent, OnRollShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) ELSE 0 END, OnRollExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(
                //										OnRoll.OnRollManpower, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))) ELSE 0 END, PresentShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) ELSE 0 END, PresentExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationPositionMPBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))) ELSE 0 END
                //					FROM MST.OperationMaster
                //					LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
                //					LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
                //					LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
                //					LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
                //					LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
                //					LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
                //					LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
                //					LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
                //					LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
                //					LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
                //					LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
                //					LEFT OUTER JOIN MST.OperationPositionMPBudget ON OperationPositionMPBudget.OperationMasterId = OperationMaster.Id
                //						AND OperationPositionMPBudget.CompanyGroupId = OperationMaster.CompanyGroupId
                //					LEFT OUTER JOIN ORG.Entity ON OperationPositionMPBudget.EntityId = Entity.Id
                //					--LEFT OUTER JOIN ORG.Position ON OperationPositionMPBudget.PositionId = Position.Id
                //					LEFT OUTER JOIN (
                //								--SELECT ManpowerBudget.EntityId, ManpowerBudget.PositionId, ISNULL(OperationMasterId, '') OperationMasterId
                //								--,Count(EmployeeInformation.SystemId) OnRollManpower
                //								--FROM EmployeeInformation
                //								--LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //								--where EmployeeInformation.EmployeeStatus='Active'
                //								--GROUP BY ManpowerBudget.EntityId, ManpowerBudget.PositionId, OperationMasterId
                //								Select B.EntityId,A.OperationMasterID,count(A.SystemId) OnRollManpower from EmployeeInformation A
                //									Left Outer Join MST.ManpowerBudget B on A.BudgetCode=B.Id
                //									Inner Join MST.OperationMaster C on A.OperationMasterID=C.Id
                //									Left Outer Join MST.OperationPositionMPBudget D on C.Id=D.OperationMasterId and B.EntityId=D.EntityId
                //									where D.Id is null
                //									group by B.EntityId,A.OperationMasterID
                //						) OnRoll ON OperationPositionMPBudget.EntityId = OnRoll.EntityId
                //						--AND OperationPositionMPBudget.PositionId = OnRoll.PositionId
                //						AND OperationMaster.Id = OnRoll.OperationMasterId
                //					LEFT OUTER JOIN (
                //						SELECT ManpowerBudget.EntityId, ManpowerBudget.PositionId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) DayPresentCount
                //						FROM EmployeeInformation
                //						LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //						LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
                //						WHERE AttdnProcessData.DayStatus IN (
                //								SELECT DayType
                //								FROM DayType
                //								WHERE Category = 'Present'
                //									OR Category = 'Late'
                //								)
                //							AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
                //						GROUP BY ManpowerBudget.EntityId, ManpowerBudget.PositionId, OperationMasterId
                //						) Present ON OperationPositionMPBudget.EntityId = Present.EntityId
                //						AND OperationPositionMPBudget.PositionId = Present.PositionId
                //						AND OperationMaster.Id = Present.OperationMasterId
                //					) Main


                //                     ) xyz
                //                    where " + paramters + "" +
                //		"GROUP BY OperationId, OperationCode, OperationName , OperationCategoryId , OperationCategoryName , MachineMasterId , MachineMaster  , MachineCategoryId , MachineCategory,SkillId,Type,Skill,SkillGroupId,SkillGroupe,ProcessId,Process";
                //_sql = @"SELECT OperationId
                //                     ,OperationCode
                //                     ,OperationName
                //                     ,OperationCategoryId
                //                     ,OperationCategoryName
                //                     ,MachineMasterId
                //                     ,MachineMaster MachineMaster
                //                     ,MachineCategoryId
                //                     ,MachineCategory
                //                     --,MachineSubCategoryId
                //                     --,MachineSubCategory
                //                     ,SkillId
                //                     ,Type
                //                     ,Skill
                //                     ,SkillGroupId
                //                     ,SkillGroupe
                //                     --,Position
                //                     --,EntityId
                //                     --,EntityName
                //                     ,ProcessId
                //                     ,Process Process
                //                     ,Sum(StandardSalary) StandardSalary
                //                     ,Sum(ManpowerBudget) ManpowerBudget
                //                     ,Sum(OnRoll) OnRoll
                //                     ,Sum(OnRollShort) OnRollShort
                //                     ,Sum(OnRollExcess) OnRollExcess
                //                     ,Sum(TotalPresent) TotalPresent
                //                     ,Sum(PresentShort) PresentShort
                //                     ,Sum(PresentExcess) PresentExcess
                //                    FROM (
                //                    SELECT OperationId, OperationCode, OperationName, OperationCategoryId, OperationCategoryName, MachineMasterId, MachineMasterName MachineMaster, MachineCategoryId, MachineCategory, MachineSubCategoryId, MachineSubCategory, SkillId, Type, Skill, SkillGroupId, SkillGroupe, 
                //				EntityId, EntityName, ProcessId, ProcessName Process, ManpowerBudget, StandardSalary, OnRoll, OnRollShort, OnRollExcess, TotalPresent, PresentShort, PresentExcess
                //				FROM (
                //					SELECT OperationMaster.Id OperationId, OperationMaster.Code OperationCode, OperationMaster.UserName OperationName, OperationMaster.OperationActivityId, OperationMaster.Type, OperationActivity.UserName OperationActivityName, OperationMaster.OperationTypeId, OperationType.UserName OperationTypeName, OperationMaster.OperationCategoryId, OperationCategory.UserName OperationCategoryName, OperationMaster.Type OperationOrActivity, ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId, ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory, SkillId = CASE WHEN OperationMaster.Type = 'Activity' THEN OperationMaster.SkillId ELSE MachineMaster.SkillId END, Skill = CASE WHEN OperationMaster.Type = 'Activity' THEN Skill.UserName ELSE MachineSkill.UserName END,
                //						OperationMaster.SkillGroupId, SkillGrouping.UserName SkillGroupe, SkillGrouping.StandardSalary, OperationMaster.LegalDesignationId, LegalDesignation.UserName LegalDesignationName, OperationMaster.ProcessId, Process.UserName ProcessName, OperationMaster.ProposedSalary, IsNull(OperationManpowerBudget.EntityId, 'Blank') EntityId, ISNULL(Entity.UserName, 'Blank') EntityName, 
                //						ISNULL(OperationManpowerBudget.ManpowerBudget, 0) ManpowerBudget, ISNULL(OnRoll.OnRollManpower, 0) OnRoll, ISNULL(Present.DayPresentCount, 0) TotalPresent, OnRollShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) ELSE 0 END, OnRollExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(
                //										OnRoll.OnRollManpower, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))) ELSE 0 END, PresentShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) ELSE 0 END, PresentExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))) ELSE 0 END
                //					FROM MST.OperationMaster
                //					LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
                //					LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
                //					LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
                //					LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
                //					LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
                //					LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
                //					LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
                //					LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
                //					LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
                //					LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
                //					LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
                //					LEFT OUTER JOIN (
                //						Select CompanyGroupId,EntityId,OperationMasterId,sum(ManpowerBudget) ManpowerBudget from mst.OperationPositionMPBudget group by CompanyGroupId,EntityId,OperationMasterId
                //						) OperationManpowerBudget on OperationManpowerBudget.OperationMasterId = OperationMaster.Id and OperationManpowerBudget.CompanyGroupId = OperationMaster.CompanyGroupId
                //					LEFT OUTER JOIN ORG.Entity ON OperationManpowerBudget.EntityId = Entity.Id	
                //					LEFT OUTER JOIN (
                //						SELECT ManpowerBudget.EntityId,ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) OnRollManpower
                //						FROM EmployeeInformation
                //						LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //						where EmployeeInformation.EmployeeStatus='Active'
                //						GROUP BY ManpowerBudget.EntityId,OperationMasterId
                //						) OnRoll ON OperationManpowerBudget.EntityId = OnRoll.EntityId AND OperationMaster.Id = OnRoll.OperationMasterId
                //					LEFT OUTER JOIN (
                //						SELECT ManpowerBudget.EntityId, ManpowerBudget.PositionId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) DayPresentCount
                //						FROM EmployeeInformation
                //						LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //						LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
                //						WHERE AttdnProcessData.DayStatus IN (
                //								SELECT DayType
                //								FROM DayType
                //								WHERE Category = 'Present'
                //									OR Category = 'Late'
                //								)
                //							AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
                //						GROUP BY ManpowerBudget.EntityId, ManpowerBudget.PositionId, OperationMasterId
                //						) Present ON OperationManpowerBudget.EntityId = Present.EntityId
                //						AND OperationMaster.Id = Present.OperationMasterId
                //					) Main
                //                     ) xyz
                //                    where " + paramters + "" +
                //		"GROUP BY OperationId, OperationCode, OperationName , OperationCategoryId , OperationCategoryName , MachineMasterId , MachineMaster  , MachineCategoryId , MachineCategory,SkillId,Type,Skill,SkillGroupId,SkillGroupe,ProcessId,Process";

                _sql = @"SELECT OperationId
	                        ,OperationCode
	                        ,OperationName
	                        ,OperationCategoryId
	                        ,OperationCategoryName
	                        ,MachineMasterId
	                        ,MachineMaster MachineMaster
	                        ,MachineCategoryId
	                        ,MachineCategory
	                        --,MachineSubCategoryId
	                        --,MachineSubCategory
	                        ,SkillId
	                        ,Type
	                        ,Skill
	                        ,SkillGroupId
	                        ,SkillGroupe
	                        --,Position
	                        --,EntityId
	                        --,EntityName
	                        ,ProcessId
	                        ,Process Process
	                        ,Sum(StandardSalary) StandardSalary
	                        ,Sum(ManpowerBudget) ManpowerBudget,Isnull(Sum(AllotedManpower),0) AllotedManpower
	                        ,Sum(OnRoll) OnRoll
	                        ,Sum(OnRollShort) OnRollShort
	                        ,Sum(OnRollExcess) OnRollExcess
	                        ,Sum(TotalPresent) TotalPresent
	                        ,Sum(PresentShort) PresentShort
	                        ,Sum(PresentExcess) PresentExcess
                        FROM (
	                       SELECT OperationId, OperationCode, OperationName, OperationCategoryId, OperationCategoryName, MachineMasterId, MachineMasterName MachineMaster, MachineCategoryId, MachineCategory, MachineSubCategoryId, MachineSubCategory, SkillId, Type, Skill, SkillGroupId, SkillGroupe, 
							EntityId, EntityName, ProcessId, ProcessName Process, ManpowerBudget, StandardSalary, OnRoll, OnRollShort, OnRollExcess, TotalPresent, PresentShort, PresentExcess,AllotedManpower
							FROM (
								SELECT OperationMaster.Id OperationId, OperationMaster.Code OperationCode, OperationMaster.UserName OperationName, OperationMaster.OperationActivityId, OperationMaster.Type, OperationActivity.UserName OperationActivityName, OperationMaster.OperationTypeId, OperationType.UserName OperationTypeName, OperationMaster.OperationCategoryId, OperationCategory.UserName OperationCategoryName, OperationMaster.Type OperationOrActivity, ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId, ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory, SkillId = CASE WHEN OperationMaster.Type = 'Activity' THEN OperationMaster.SkillId ELSE MachineMaster.SkillId END, Skill = CASE WHEN OperationMaster.Type = 'Activity' THEN Skill.UserName ELSE MachineSkill.UserName END,
									OperationMaster.SkillGroupId, SkillGrouping.UserName SkillGroupe, SkillGrouping.StandardSalary, OperationMaster.LegalDesignationId, LegalDesignation.UserName LegalDesignationName, OperationMaster.ProcessId, Process.UserName ProcessName, OperationMaster.ProposedSalary, IsNull(OperationManpowerBudget.EntityId, 'Blank') EntityId, ISNULL(Entity.UserName, 'Blank') EntityName, 
									ISNULL(OperationManpowerBudget.ManpowerBudget, 0) ManpowerBudget, ISNULL(OnRoll.OnRollManpower, 0) OnRoll, ISNULL(Present.DayPresentCount, 0) TotalPresent, OnRollShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) ELSE 0 END, OnRollExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(
													OnRoll.OnRollManpower, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))) ELSE 0 END, PresentShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) ELSE 0 END, PresentExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))) ELSE 0 END,AllotedManpower
								FROM MST.OperationMaster
								LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
								LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
								LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
								LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
								LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
								LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
								LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
								LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
								LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
								LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
								LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
                                LEFT OUTER JOIN trn.ProductionBulletinTemplateDetail AS BMD ON BMD.OperationMasterId=OperationMaster.Id
                                LEFT OUTER JOIN trn.ProductionBulletinTemplateMaster AS BM ON bm.ProductionBulletinTemplateId=BMD.ProductionBulletinTemplateMasterId
                                LEFT OUTER JOIN trn.ProductionBulletinTemplate AS BT ON BT.ProductionOrderId=BMD.ProductionBulletinTemplateMasterId
								LEFT OUTER JOIN (
									Select CompanyGroupId,EntityId,OperationMasterId,sum(ManpowerBudget) ManpowerBudget from mst.OperationPositionMPBudget group by CompanyGroupId,EntityId,OperationMasterId
									) OperationManpowerBudget on OperationManpowerBudget.OperationMasterId = OperationMaster.Id and OperationManpowerBudget.CompanyGroupId = OperationMaster.CompanyGroupId
								LEFT OUTER JOIN ORG.Entity ON OperationManpowerBudget.EntityId = Entity.Id	
								LEFT OUTER JOIN (
									SELECT ManpowerBudget.EntityId,ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) OnRollManpower
									FROM EmployeeInformation
									LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
									where EmployeeInformation.EmployeeStatus='Active'
									GROUP BY ManpowerBudget.EntityId,OperationMasterId
									) OnRoll ON OperationManpowerBudget.EntityId = OnRoll.EntityId AND OperationMaster.Id = OnRoll.OperationMasterId
								LEFT OUTER JOIN (
									SELECT ManpowerBudget.EntityId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) DayPresentCount
									FROM EmployeeInformation
									LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
									LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
									WHERE AttdnProcessData.DayStatus IN (
											SELECT DayType
											FROM DayType
											WHERE Category = 'Present'
												OR Category = 'Late'
											)
										AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
									GROUP BY ManpowerBudget.EntityId, OperationMasterId
									) Present ON OperationManpowerBudget.EntityId = Present.EntityId
									AND OperationMaster.Id = Present.OperationMasterId
								) Main 
	                        ) xyz
                        where " + paramters + "" +
                        "GROUP BY OperationId, OperationCode, OperationName , OperationCategoryId , OperationCategoryName , MachineMasterId , MachineMaster  , MachineCategoryId , MachineCategory,SkillId,Type,Skill,SkillGroupId,SkillGroupe,ProcessId,Process";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }




        public IEnumerable<object> GetGraphDetails(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess)
        {
            try
            {
                string _sql = "";

                string paramters = "";
                //                string parameterOutside = "";
                //                if (queryString != "")
                //                {
                //                    if (paramters == "")
                //                        paramters += " ISNULL(OPMBF.EntityName,'') in(" + queryString + ")";
                //                    else
                //                        paramters += " AND ISNULL(OPMBF.EntityName,'') in(" + queryString + ")";
                //                }

                //                if (queryStringCaption != "")
                //                {
                //                    if (paramters == "")
                //                        paramters += " ISNULL(OPMBF.Caption,'') in(" + queryStringCaption + ")";
                //                    else
                //                        paramters += " AND ISNULL(OPMBF.Caption,'') in(" + queryStringCaption + ")";
                //                }
                //                if (queryStringSkill != "")
                //                {
                //                    if (parameterOutside == "")
                //                        parameterOutside += " ISNULL(Skill,'') in(" + queryStringSkill + ")";
                //                    else
                //                        parameterOutside += " AND ISNULL(Skill,'') in(" + queryStringSkill + ")";
                //                }
                //                if (queryStringProcess != "")
                //                {
                //                    if (paramters == "")
                //                        paramters += " ISNULL(P.UserName,'') in(" + queryStringProcess + ")";
                //                    else
                //                        paramters += " AND ISNULL(P.UserName,'') in(" + queryStringProcess + ")";
                //                }


                //                if (queryStringGrouping != "")
                //                {
                //                    if (paramters == "")
                //                        paramters += " ISNULL(SKG.grouping,'') in(" + queryStringGrouping + ")";
                //                    else
                //                        paramters += " AND ISNULL(SKG.grouping,'') in(" + queryStringGrouping + ")";
                //                }
                //                if (queryStringMachineCategory != "")
                //                {
                //                    if (paramters == "")
                //                        paramters += " ISNULL(MC.UserName,'') in(" + queryStringMachineCategory + ")";
                //                    else
                //                        paramters += " AND ISNULL(MC.UserName,'') in(" + queryStringMachineCategory + ")";
                //                }
                //                if (queryStringOnRoll != "")
                //                {
                //                    if (paramters == "")
                //                        paramters += " ISNULL(E.OnRoll,0)  in(" + queryStringOnRoll + ")";
                //                    else
                //                        paramters += " AND ISNULL(E.OnRoll,0)  in(" + queryStringOnRoll + ")";
                //                }
                //                if (queryStringTotalPresent != "")
                //                {
                //                    if (paramters == "")
                //                        paramters += " ISNULL(E.TotalPresent,0)  in(" + queryStringTotalPresent + ")";
                //                    else
                //                        paramters += " AND ISNULL(E.TotalPresent,0)  in(" + queryStringTotalPresent + ")";
                //                }

                //                if (queryStringOnRollShort != "")
                //                {
                //                    if (parameterOutside == "")
                //                        parameterOutside += " ISNULL(k.OnRollShort,0) in(" + queryStringOnRollShort + ")";
                //                    else
                //                        parameterOutside += " AND ISNULL(k.OnRollShort,0) in(" + queryStringOnRollShort + ")";
                //                }
                //                if (queryStringOnRollExcess != "")
                //                {
                //                    if (parameterOutside == "")
                //                        parameterOutside += " ISNULL(k.OnRollExcess,0) in(" + queryStringOnRollExcess + ")";
                //                    else
                //                        parameterOutside += " AND ISNULL(k.OnRollExcess,0) in(" + queryStringOnRollExcess + ")";
                //                }
                //                if (queryStringPresentShort != "")
                //                {
                //                    if (parameterOutside == "")
                //                        parameterOutside += " ISNULL(k.PresentShort,0) in(" + queryStringPresentShort + ")";
                //                    else
                //                        parameterOutside += " AND ISNULL(k.PresentShort,0) in(" + queryStringPresentShort + ")";
                //                }
                //                if (queryStringPresentExcess != "")
                //                {
                //                    if (parameterOutside == "")
                //                        parameterOutside += " ISNULL(k.presentExcess,0) in(" + queryStringPresentExcess + ")";
                //                    else
                //                        parameterOutside += " AND ISNULL(presentExcess,0) in(" + queryStringPresentExcess + ")";
                //                }


                //                _sql = @"select OperationName,OnRollBalance = CASE 
                //		                            WHEN sum(isnull(OnRollShort,0)) > 0 THEN sum(isnull(OnRollShort,0)*-1) ELSE sum(isnull(OnRollExcess,0))			                          
                //		                            END,	                            
                //	                               PresentBalance = CASE 
                //		                            WHEN sum(isnull(PresentShort,0)) > 0 THEN sum(isnull(PresentShort,0)*-1) ELSE sum(isnull(PresentExcess,0))		                          
                //		                            END 
                //                                from (SELECT P.UserName Process
                //	                            ,Skill = CASE 
                //		                            WHEN SKO.UserName IS NULL
                //			                            THEN SKM.UserName
                //		                            WHEN SKM.UserName IS NULL
                //			                            THEN SKO.UserName
                //		                            ELSE SKM.UserName
                //		                            END
                //	                            ,MM.UserName MachineMaster
                //	                            ,SKG.Code SkillGroupingCode
                //	                            ,OM.Type
                //	                            ,SKG.StandardSalary
                //	                            ,CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget) ManpowerBudget
                //	                            ,ISNULL(E.OnRoll, 0) OnRoll
                //	                            ,ISNULL(E.TotalPresent, 0) TotalPresent
                //	                            ,OnRollShort = CASE 
                //		                            WHEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0)) > 0
                //			                            THEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0))
                //		                            ELSE 0
                //		                            END
                //	                            ,OnRollExcess = CASE 
                //		                            WHEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0)) < 0
                //			                            THEN ABS(CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0)))
                //		                            ELSE 0
                //		                            END
                //	                            ,PresentShort = CASE 
                //		                            WHEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0)) > 0
                //			                            THEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0))
                //		                            ELSE 0
                //		                            END
                //	                            ,PresentExcess = CASE 
                //		                            WHEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0)) < 0
                //			                            THEN ABS(CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0)))
                //		                            ELSE 0
                //		                            END,
                //                            OM.Code OerationCode
                //                            ,OM.UserName OperationName
                //                            --,OPMBF.EntityCode
                //                            --,OPMBF.EntityName
                //                            -- ,OPMBF.PositionCode
                //                            --,OPMBF.PositionName
                //                            --,OPMBF.Caption
                //                            --,MM.Code MachineCode
                //                            --,MC.Code MachineCategoryCode
                //                            --,MC.UserName MachineCategory
                //                            --,MSC.Code MachineSubCategoryCode
                //                            --,MSC.UserName MachineSubCategory
                //                            --,SKG.Grouping
                //                            -- ,SKG.DesignationCategory
                //                            --,OLDG.UserName LegalDesignation	                          
                //                            FROM [MST].[OperationMaster] OM
                //                            LEFT JOIN MST.MachineMaster MM ON MM.Id = OM.MachineMasterId
                //                            LEFT JOIN HKP.MachineCategory MC ON MC.Id = MM.MachineCategoryId
                //                            LEFT JOIN HKP.MachineSubCategory MSC ON MSC.Id = MM.MachineSubCategoryId
                //                            LEFT JOIN [HKP].[Skill] SKO ON SKO.Id = OM.SkillId
                //                            LEFT JOIN [HKP].[Skill] SKM ON SKM.Id = MM.SkillId
                //                            LEFT JOIN [HKP].[SkillProcess] SP ON SP.SkillId = SKO.Id
                //                            LEFT JOIN [HKP].[Process] P ON P.Id = OM.ProcessId
                //                            LEFT JOIN SCS.SkillGrouping SKG ON SKG.Id = OM.SkillGroupId
                //                            LEFT JOIN HKP.LegalDesignation OLDG ON OLDG.Id = OM.LegalDesignationId
                //                            LEFT JOIN (
                //	                            ----EmployeeInformation
                //	                            SELECT e.OperationMasterId
                //		                            ,Count(convert(INT, E.SystemId)) AS [TOTAL ONROLL]
                //	                            FROM EmployeeInformation e
                //	                            WHERE E.EmployeeStatus = 'Active'
                //	                            GROUP BY e.OperationMasterId
                //	                            ) ONOP ON ONOP.OperationMasterId = OM.Id
                //                            LEFT JOIN (
                //	                            ---OperationPositionMPBudget
                //	                            SELECT *
                //	                            FROM (
                //		                            SELECT OPM.OperationMasterId
                //			                            ,OM.Code OperationCode
                //			                            ,OM.UserName Operation
                //			                            ,EN.Id EntityId
                //			                            ,EN.Code EntityCode
                //			                            ,EN.UserName EntityName
                //			                            ,PS.Code PositionCode
                //			                            ,PS.UserName PositionName
                //			                            ,OPM.Caption
                //			                            ,OPM.ManpowerBudget
                //			                            ,PS.Id PositionId
                //		                            FROM [MST].[OperationPositionMPBudget] OPM
                //		                            LEFT JOIN [MST].[OperationMaster] OM ON OM.Id = OPM.OperationMasterId
                //		                            LEFT JOIN ORG.Entity EN ON EN.Id = OPM.EntityId
                //		                            LEFT JOIN ORG.Position PS ON PS.Id = OPM.PositionId
                //		                            ) OPMB
                //	                            ) OPMBF ON OPMBF.OperationMasterId = OM.Id
                //                            LEFT JOIN (
                //	                            --[Total Present] ,[ONROLL]
                //	                            SELECT EM.OperationMasterId
                //		                            ,EN.Id EntityId
                //		                            ,POS.Id PositionId
                //		                            ,SUM([Status]) TotalPresent
                //		                            ,COUNT(EM.SystemId) ONROLL
                //	                            FROM EmployeeInformation EM
                //	                            LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EM.BudgetCode
                //	                            LEFT JOIN ORG.Entity EN ON EN.Id = MB.EntityId
                //	                            LEFT JOIN ORG.Position POS ON POS.Id = EM.PositionId
                //	                            LEFT JOIN (
                //		                            SELECT DayStatus
                //			                            ,EmpSystemID
                //			                            ,[Status] = CASE 
                //				                            WHEN DayStatus IS NULL
                //					                            THEN 0
                //				                            WHEN DayStatus IS NOT NULL
                //					                            THEN 1
                //				                            ELSE 0
                //				                            END
                //		                            FROM AttdnProcessData
                //		                            WHERE DayStatus IN (
                //				                            'LP'
                //				                            ,'HDP'
                //				                            ,'P'
                //				                            )
                //			                            AND WorkDate = REPLACE(Convert(varchar(11),getdate(),106),' ','-')
                //		                            ) AD ON AD.EmpSystemID = EM.SystemId
                //	                            WHERE EM.EmployeeStatus = 'Active'
                //	                            GROUP BY EM.OperationMasterId
                //		                            ,EN.Id
                //		                            ,POS.Id
                //	                            ) E ON E.OperationMasterId = OPMBF.OperationMasterId
                //	                            AND E.PositionID = OPMBF.PositionId
                //	                            AND E.EntityId = OPMBF.EntityId
                //                                    where  " + paramters + @"
                //                                    ) AS K 
                //--WHERE " + parameterOutside + @"
                //                                    group by OperationName ";




                //string parameterOutside = "";
                //if (queryString != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(OPMBF.EntityName,'') in(" + queryString + ")";
                //    else
                //        paramters += " AND ISNULL(OPMBF.EntityName,'') in(" + queryString + ")";
                //}

                //if (queryStringCaption != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(OPMBF.Caption,'') in(" + queryStringCaption + ")";
                //    else
                //        paramters += " AND ISNULL(OPMBF.Caption,'') in(" + queryStringCaption + ")";
                //}
                //if (queryStringSkill != "")
                //{
                //    if (parameterOutside == "")
                //        parameterOutside += " ISNULL(Skill,'') in(" + queryStringSkill + ")";
                //    else
                //        parameterOutside += " AND ISNULL(Skill,'') in(" + queryStringSkill + ")";
                //}
                //if (queryStringProcess != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(P.UserName,'') in(" + queryStringProcess + ")";
                //    else
                //        paramters += " AND ISNULL(P.UserName,'') in(" + queryStringProcess + ")";
                //}


                //if (queryStringGrouping != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(SKG.grouping,'') in(" + queryStringGrouping + ")";
                //    else
                //        paramters += " AND ISNULL(SKG.grouping,'') in(" + queryStringGrouping + ")";
                //}
                //if (queryStringMachineCategory != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(MC.UserName,'') in(" + queryStringMachineCategory + ")";
                //    else
                //        paramters += " AND ISNULL(MC.UserName,'') in(" + queryStringMachineCategory + ")";
                //}
                //if (queryStringOnRoll != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(E.OnRoll,0)  in(" + queryStringOnRoll + ")";
                //    else
                //        paramters += " AND ISNULL(E.OnRoll,0)  in(" + queryStringOnRoll + ")";
                //}
                //if (queryStringTotalPresent != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(E.TotalPresent,0)  in(" + queryStringTotalPresent + ")";
                //    else
                //        paramters += " AND ISNULL(E.TotalPresent,0)  in(" + queryStringTotalPresent + ")";
                //}

                //if (queryStringOnRollShort != "")
                //{
                //    if (parameterOutside == "")
                //        parameterOutside += " ISNULL(k.OnRollShort,0) in(" + queryStringOnRollShort + ")";
                //    else
                //        parameterOutside += " AND ISNULL(k.OnRollShort,0) in(" + queryStringOnRollShort + ")";
                //}
                //if (queryStringOnRollExcess != "")
                //{
                //    if (parameterOutside == "")
                //        parameterOutside += " ISNULL(k.OnRollExcess,0) in(" + queryStringOnRollExcess + ")";
                //    else
                //        parameterOutside += " AND ISNULL(k.OnRollExcess,0) in(" + queryStringOnRollExcess + ")";
                //}
                //if (queryStringPresentShort != "")
                //{
                //    if (parameterOutside == "")
                //        parameterOutside += " ISNULL(k.PresentShort,0) in(" + queryStringPresentShort + ")";
                //    else
                //        parameterOutside += " AND ISNULL(k.PresentShort,0) in(" + queryStringPresentShort + ")";
                //}
                //if (queryStringPresentExcess != "")
                //{
                //    if (parameterOutside == "")
                //        parameterOutside += " ISNULL(k.presentExcess,0) in(" + queryStringPresentExcess + ")";
                //    else
                //        parameterOutside += " AND ISNULL(presentExcess,0) in(" + queryStringPresentExcess + ")";
                //}







                if (queryString != "")
                {
                    if (paramters == "")
                        paramters += " EntityId in(" + queryString + ")";
                    else
                        paramters += " AND EntityId in(" + queryString + ")";
                }

                //if (queryStringCaption != "")
                //{
                //    if (paramters == "")
                //        paramters += " ISNULL(Position,'') in(" + queryStringCaption + ")";
                //    else
                //        paramters += " AND ISNULL(Position,'') in(" + queryStringCaption + ")";
                //}
                if (queryStringSkill != "")
                {
                    if (paramters == "")
                        paramters += " SkillId in(" + queryStringSkill + ")";
                    else
                        paramters += " AND SkillId in(" + queryStringSkill + ")";
                }
                if (queryStringOperationCode != "")
                {
                    if (paramters == "")
                        paramters += " OperationCode in(" + queryStringOperationCode + ")";
                    else
                        paramters += " AND OperationCode in(" + queryStringOperationCode + ")";
                }

                if (queryStringProcess != "")
                {
                    if (paramters == "")
                        paramters += " ProcessId in(" + queryStringProcess + ")";
                    else
                        paramters += " AND ProcessId in(" + queryStringProcess + ")";
                }


                if (queryStringGrouping != "")
                {
                    if (paramters == "")
                        paramters += " SkillGroupID in(" + queryStringGrouping + ")";
                    else
                        paramters += " AND SkillGroupID in(" + queryStringGrouping + ")";
                }
                if (queryStringMachineCategory != "")
                {
                    if (paramters == "")
                        paramters += " MachineCategoryID in(" + queryStringMachineCategory + ")";
                    else
                        paramters += " AND MachineCategoryID in(" + queryStringMachineCategory + ")";
                }
                if (queryStringMachineSubCategoryCode != "")
                {
                    if (paramters == "")
                        paramters += " MachineSubCategoryId in(" + queryStringMachineSubCategoryCode + ")";
                    else
                        paramters += " AND MachineSubCategoryId in(" + queryStringMachineSubCategoryCode + ")";
                }
                if (queryStringOperationCategoryId != "")
                {
                    if (paramters == "")
                        paramters += " OperationCategoryId in(" + queryStringOperationCategoryId + ")";
                    else
                        paramters += " AND OperationCategoryId in(" + queryStringOperationCategoryId + ")";
                }
                if (queryStringOnRoll != "")
                {
                    if (paramters == "")
                        paramters += " ISNULL(OnRoll,0)  in(" + queryStringOnRoll + ")";
                    else
                        paramters += " AND ISNULL(OnRoll,0)  in(" + queryStringOnRoll + ")";
                }
                if (queryStringTotalPresent != "")
                {
                    if (paramters == "")
                        paramters += " ISNULL(TotalPresent,0)  in(" + queryStringTotalPresent + ")";
                    else
                        paramters += " AND ISNULL(TotalPresent,0)  in(" + queryStringTotalPresent + ")";
                }

                if (queryStringOnRollShort != "")
                {
                    if (paramters == "")
                        paramters += " ISNULL(OnRollShort,0) in(" + queryStringOnRollShort + ")";
                    else
                        paramters += " AND ISNULL(OnRollShort,0) in(" + queryStringOnRollShort + ")";
                }
                if (queryStringOnRollExcess != "")
                {
                    if (paramters == "")
                        paramters += " ISNULL(OnRollExcess,0) in(" + queryStringOnRollExcess + ")";
                    else
                        paramters += " AND ISNULL(OnRollExcess,0) in(" + queryStringOnRollExcess + ")";
                }
                if (queryStringPresentShort != "")
                {
                    if (paramters == "")
                        paramters += " ISNULL(PresentShort,0) in(" + queryStringPresentShort + ")";
                    else
                        paramters += " AND ISNULL(PresentShort,0) in(" + queryStringPresentShort + ")";
                }
                if (queryStringPresentExcess != "")
                {
                    if (paramters == "")
                        paramters += " ISNULL(presentExcess,0) in(" + queryStringPresentExcess + ")";
                    else
                        paramters += " AND ISNULL(presentExcess,0) in(" + queryStringPresentExcess + ")";
                }
                //    _sql = @"SELECT OperationName
                //,OnRollBalance = CASE

                //WHEN sum(isnull(OnRollShort, 0)) > 0

                //    THEN sum(isnull(OnRollShort, 0) *-1)
                //ELSE sum(isnull(OnRollExcess, 0))
                //END
                //,PresentBalance = CASE

                //WHEN sum(isnull(PresentShort, 0)) > 0

                //    THEN sum(isnull(PresentShort, 0) *-1)
                //ELSE sum(isnull(PresentExcess, 0))
                //END
                //FROM(
                //SELECT OM.Id OperationId
                //, OM.Code OperationCode
                //, OM.UserName OperationName
                //, OM.Type
                //, OPMBF.EntityId
                //, OPMBF.EntityCode
                //, OPMBF.EntityName
                //, OPMBF.PositionCode
                //, OPMBF.PositionName
                //, OPMBF.Caption Position
                //, MM.Code MachineCode
                //, ISNULL(MM.UserName, '') MachineMaster
                //, MC.Id MachineCategoryID
                //, MC.Code MachineCategoryCode
                //, ISNULL(MC.UserName, '') MachineCategory
                //, MSC.Id MachineSubCategoryId
                //, MSC.Code MachineSubCategoryCode
                //, ISNULL(MSC.UserName, '') MachineSubCategory
                //, Skill = CASE

                //    WHEN SKO.UserName IS NULL

                //        THEN SKM.UserName

                //    WHEN SKM.UserName IS NULL

                //        THEN SKO.UserName

                //    ELSE SKM.UserName

                //    END
                //, SkillId = CASE

                //    WHEN SKO.id IS NULL

                //        THEN SKM.Id

                //    WHEN SKM.Id IS NULL

                //        THEN SKO.Id

                //    ELSE SKM.Id

                //    END
                //, SKG.Id SkillGroupID
                //, SKG.Code SkillGroupingCode
                //, SKG.Grouping SkillGroupe
                //, SKG.DesignationCategory
                //, SKG.StandardSalary
                //, P.Id ProcessId
                //, P.UserName Process
                //, OLDG.UserName LegalDesignation
                //, CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget) ManpowerBudget
                //, ISNULL(E.OnRoll, 0) OnRoll
                //, ISNULL(E.TotalPresent, 0) TotalPresent
                //, OnRollShort = CASE

                //    WHEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0)) > 0

                //        THEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0))

                //    ELSE 0

                //    END
                //, OnRollExcess = CASE

                //    WHEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0)) < 0

                //        THEN ABS(CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.OnRoll, 0)))

                //    ELSE 0

                //    END
                //, PresentShort = CASE

                //    WHEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0)) > 0

                //        THEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0))

                //    ELSE 0

                //    END
                //, PresentExcess = CASE

                //    WHEN CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0)) < 0

                //        THEN ABS(CONVERT(NUMERIC(10, 0), OPMBF.ManpowerBudget - ISNULL(E.TotalPresent, 0)))

                //    ELSE 0

                //    END

                //FROM[MST].[OperationMaster] OM

                //LEFT JOIN MST.MachineMaster MM ON MM.Id = OM.MachineMasterId

                //LEFT JOIN HKP.MachineCategory MC ON MC.Id = MM.MachineCategoryId

                //LEFT JOIN HKP.MachineSubCategory MSC ON MSC.Id = MM.MachineSubCategoryId

                //LEFT JOIN[HKP].[Skill] SKO ON SKO.Id = OM.SkillId

                //LEFT JOIN[HKP].[Skill] SKM ON SKM.Id = MM.SkillId

                //LEFT JOIN[HKP].[Process] P ON P.Id = OM.ProcessId

                //LEFT JOIN SCS.SkillGrouping SKG ON SKG.Id = OM.SkillGroupId

                //LEFT JOIN HKP.LegalDesignation OLDG ON OLDG.Id = OM.LegalDesignationId

                //LEFT JOIN(
                //---OperationPositionMPBudget

                //SELECT *
                //FROM(
                //    SELECT OPM.OperationMasterId
                //        , OM.Code OperationCode
                //        , OM.UserName Operation
                //        , EN.Id EntityId
                //        , EN.Code EntityCode
                //        , EN.UserName EntityName
                //        , PS.Code PositionCode
                //        , PS.UserName PositionName
                //        , OPM.Caption
                //        , OPM.ManpowerBudget
                //        , PS.Id PositionId

                //    FROM[MST].[OperationPositionMPBudget] OPM

                //    LEFT JOIN[MST].[OperationMaster] OM ON OM.Id = OPM.OperationMasterId

                //    LEFT JOIN ORG.Entity EN ON EN.Id = OPM.EntityId

                //    LEFT JOIN ORG.Position PS ON PS.Id = OPM.PositionId
                //    ) OPMB
                //) OPMBF ON OPMBF.OperationMasterId = OM.Id

                //LEFT JOIN(
                //--[Total Present],[ONROLL]

                //SELECT EM.OperationMasterId
                //    , EN.Id EntityId
                //    , POS.Id PositionId
                //    , SUM([Status]) TotalPresent
                //    , COUNT(EM.SystemId) ONROLL

                //FROM EmployeeInformation EM

                //LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EM.BudgetCode

                //LEFT JOIN ORG.Entity EN ON EN.Id = MB.EntityId

                //LEFT JOIN ORG.Position POS ON POS.Id = MB.PositionId--EM.PositionId

                //LEFT JOIN(
                //    SELECT DayStatus
                //        , EmpSystemID
                //        ,[Status] = CASE

                //            WHEN DayStatus IS NULL

                //                THEN 0

                //            WHEN DayStatus IS NOT NULL

                //                THEN 1

                //            ELSE 0

                //            END

                //    FROM AttdnProcessData

                //    WHERE DayStatus IN(
                //            'LP'
                //            , 'HDP'
                //            , 'P'
                //            )

                //        AND WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
                //    ) AD ON AD.EmpSystemID = EM.SystemId

                //WHERE EM.EmployeeStatus = 'Active'

                //GROUP BY EM.OperationMasterId
                //    , EN.Id
                //    , POS.Id
                //) E ON E.OperationMasterId = OPMBF.OperationMasterId

                //AND E.PositionID = OPMBF.PositionId

                //AND E.EntityId = OPMBF.EntityId
                //--ORDER BY CONVERT(INT, OM.Code)
                //) AS Final
                //where " + paramters + @"
                //GROUP BY OperationName ORDER BY OnRollBalance DESC";


                _sql = @"SELECT OperationName
						,OnRollBalance = CASE	
						WHEN sum(isnull(OnRollShort, 0)) > 0
							THEN sum(isnull(OnRollShort, 0) *-1)
						ELSE sum(isnull(OnRollExcess, 0))
						END
						,PresentBalance = CASE
						WHEN sum(isnull(PresentShort, 0)) > 0
							THEN sum(isnull(PresentShort, 0) *-1)
						ELSE sum(isnull(PresentExcess, 0))
						END			
                   FROM(
						SELECT OM.Id OperationId, OM.Code OperationCode, OM.UserName OperationName,OM.Type, OM.OperationCategoryId, OC.Code OperationCategoryCode, OC.UserName OperationCategoryName,
			OPMBF.EntityId, OPMBF.EntityCode, OPMBF.EntityName, OPMBF.PositionCode, OPMBF.PositionName, OPMBF.Caption Position, 
			MM.Code MachineCode, ISNULL(MM.UserName,'') MachineMaster, MC.Id MachineCategoryID, MC.Code MachineCategoryCode, ISNULL(MC.UserName,'') MachineCategory, MSC.Id MachineSubCategoryId, MSC.Code MachineSubCategoryCode, ISNULL(MSC.UserName,'') MachineSubCategory,
			Skill = CASE 
								WHEN SKO.UserName IS NULL
									THEN SKM.UserName
								WHEN SKM.UserName IS NULL
									THEN SKO.UserName
								ELSE SKM.UserName
								END
								,SkillId = CASE 
								WHEN SKO.id IS NULL
									THEN SKM.Id
								WHEN SKM.Id IS NULL
									THEN SKO.Id
								ELSE SKM.Id
								END  
			,SKG.Id SkillGroupID,SKG.Code SkillGroupingCode, SKG.Grouping SkillGroupe, SKG.DesignationCategory, SKG.StandardSalary,P.Id ProcessId, P.UserName Process, OLDG.UserName LegalDesignation, CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget) ManpowerBudget,
			ISNULL(E.OnRoll,0) OnRoll, ISNULL(E.TotalPresent,0) TotalPresent, 
			OnRollShort=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) > 0 
							THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) ELSE 0	END

							,OnRollExcess=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) < 0 
							THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0))) ELSE 0	END

							,PresentShort=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) > 0 
							THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) ELSE 0	END

							,PresentExcess=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) < 0 
							THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0))) ELSE 0	END
							FROM [MST].[OperationMaster] OM
							LEFT JOIN MST.MachineMaster MM ON MM.Id = OM.MachineMasterId
							LEFT JOIN HKP.MachineCategory MC ON MC.Id = MM.MachineCategoryId
							LEFT JOIN HKP.MachineSubCategory MSC ON MSC.Id = MM.MachineSubCategoryId
							LEFT JOIN [HKP].[Skill] SKO ON SKO.Id = OM.SkillId
							LEFT JOIN [HKP].[Skill] SKM ON SKM.Id = MM.SkillId
							LEFT JOIN [HKP].[Process] P ON P.Id = OM.ProcessId
							LEFT JOIN SCS.SkillGrouping SKG ON SKG.Id = OM.SkillGroupId
							LEFT JOIN HKP.LegalDesignation OLDG ON OLDG.Id = OM.LegalDesignationId
							LEFT JOIN HKP.OperationCategory OC ON OC.Id = OM.OperationCategoryId                
							LEFT JOIN (
							---OperationPositionMPBudget
							SELECT *
							FROM (
								SELECT OPM.OperationMasterId
									,OM.Code OperationCode
									,OM.UserName Operation
									,EN.Id EntityId
									,EN.Code EntityCode
									,EN.UserName EntityName
									,PS.Code PositionCode
									,PS.UserName PositionName
									,OPM.Caption
									,OPM.ManpowerBudget
									,PS.Id PositionId
								FROM [MST].[OperationPositionMPBudget] OPM
								LEFT JOIN [MST].[OperationMaster] OM ON OM.Id = OPM.OperationMasterId
								LEFT JOIN ORG.Entity EN ON EN.Id = OPM.EntityId
								LEFT JOIN ORG.Position PS ON PS.Id = OPM.PositionId
								) OPMB
							) OPMBF ON OPMBF.OperationMasterId = OM.Id
							LEFT JOIN (
							--[Total Present] ,[ONROLL]
							SELECT EM.OperationMasterId
								,EN.Id EntityId
								,POS.Id PositionId
								,SUM([Status]) TotalPresent
								,COUNT(EM.SystemId) ONROLL
							FROM EmployeeInformation EM
							LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EM.BudgetCode
							LEFT JOIN ORG.Entity EN ON EN.Id = MB.EntityId
							LEFT JOIN ORG.Position POS ON POS.Id = MB.PositionId --EM.PositionId
							LEFT JOIN (
								SELECT DayStatus
									,EmpSystemID
									,[Status] = CASE 
										WHEN DayStatus IS NULL
											THEN 0
										WHEN DayStatus IS NOT NULL
											THEN 1
										ELSE 0
										END
								FROM AttdnProcessData
								WHERE DayStatus IN (
										'LP'
										,'HDP'
										,'P'
										)
									AND WorkDate = REPLACE(Convert(varchar(11),getdate(),106),' ','-')
								) AD ON AD.EmpSystemID = EM.SystemId
							WHERE EM.EmployeeStatus = 'Active'
							GROUP BY EM.OperationMasterId
								,EN.Id
								,POS.Id
							) E ON E.OperationMasterId = OPMBF.OperationMasterId
							AND E.PositionID = OPMBF.PositionId
							AND E.EntityId = OPMBF.EntityId				
							--ORDER BY CONVERT(INT, OM.Code)
			) as Final
			where " + paramters + @"
            GROUP BY OperationName ORDER BY OnRollBalance DESC";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public IEnumerable<object> GetGraphDetails1()
        {
            try
            {
                string _sql = "";
                _sql = @"SELECT OperationName
                ,OnRollBalance = CASE

                WHEN sum(isnull(OnRollShort, 0)) > 0

                THEN sum(isnull(OnRollShort, 0) *-1)
                ELSE sum(isnull(OnRollExcess, 0))
                END
                ,PresentBalance = CASE

                WHEN sum(isnull(PresentShort, 0)) > 0

                THEN sum(isnull(PresentShort, 0) *-1)
                ELSE sum(isnull(PresentExcess, 0))
                END
                FROM(
                SELECT OM.Id OperationId, OM.Code OperationCode, OM.UserName OperationName,OM.Type, OM.OperationCategoryId, OC.Code OperationCategoryCode, OC.UserName OperationCategoryName,
                OPMBF.EntityId, OPMBF.EntityCode, OPMBF.EntityName, OPMBF.PositionCode, OPMBF.PositionName, OPMBF.Caption Position, 
                MM.Code MachineCode, ISNULL(MM.UserName,'') MachineMaster, MC.Id MachineCategoryID, MC.Code MachineCategoryCode, ISNULL(MC.UserName,'') MachineCategory, MSC.Id MachineSubCategoryId, MSC.Code MachineSubCategoryCode, ISNULL(MSC.UserName,'') MachineSubCategory,
                Skill = CASE 
                 WHEN SKO.UserName IS NULL
                  THEN SKM.UserName
                 WHEN SKM.UserName IS NULL
                  THEN SKO.UserName
                 ELSE SKM.UserName
                 END
                    ,SkillId = CASE 
                 WHEN SKO.id IS NULL
                  THEN SKM.Id
                 WHEN SKM.Id IS NULL
                  THEN SKO.Id
                 ELSE SKM.Id
                 END  
                ,SKG.Id SkillGroupID,SKG.Code SkillGroupingCode, SKG.Grouping SkillGroupe, SKG.DesignationCategory, SKG.StandardSalary,P.Id ProcessId, P.UserName Process, OLDG.UserName LegalDesignation, CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget) ManpowerBudget,
                ISNULL(E.OnRoll,0) OnRoll, ISNULL(E.TotalPresent,0) TotalPresent, 
                OnRollShort=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) > 0 
                THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) ELSE 0	END

                ,OnRollExcess=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0)) < 0 
                THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.OnRoll,0))) ELSE 0	END

                ,PresentShort=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) > 0 
                THEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) ELSE 0	END

                ,PresentExcess=CASE WHEN CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0)) < 0 
                THEN ABS(CONVERT(NUMERIC(10,0), OPMBF.ManpowerBudget-ISNULL(E.TotalPresent,0))) ELSE 0	END
                FROM [MST].[OperationMaster] OM
                LEFT JOIN MST.MachineMaster MM ON MM.Id = OM.MachineMasterId
                LEFT JOIN HKP.MachineCategory MC ON MC.Id = MM.MachineCategoryId
                LEFT JOIN HKP.MachineSubCategory MSC ON MSC.Id = MM.MachineSubCategoryId
                LEFT JOIN [HKP].[Skill] SKO ON SKO.Id = OM.SkillId
                LEFT JOIN [HKP].[Skill] SKM ON SKM.Id = MM.SkillId
                LEFT JOIN [HKP].[Process] P ON P.Id = OM.ProcessId
                LEFT JOIN SCS.SkillGrouping SKG ON SKG.Id = OM.SkillGroupId
                LEFT JOIN HKP.LegalDesignation OLDG ON OLDG.Id = OM.LegalDesignationId
                LEFT JOIN HKP.OperationCategory OC ON OC.Id = OM.OperationCategoryId                
                LEFT JOIN (
                ---OperationPositionMPBudget
                SELECT *
                FROM (
                 SELECT OPM.OperationMasterId
                  ,OM.Code OperationCode
                  ,OM.UserName Operation
                  ,EN.Id EntityId
                  ,EN.Code EntityCode
                  ,EN.UserName EntityName
                  ,PS.Code PositionCode
                  ,PS.UserName PositionName
                  ,OPM.Caption
                  ,OPM.ManpowerBudget
                  ,PS.Id PositionId
                 FROM [MST].[OperationPositionMPBudget] OPM
                 LEFT JOIN [MST].[OperationMaster] OM ON OM.Id = OPM.OperationMasterId
                 LEFT JOIN ORG.Entity EN ON EN.Id = OPM.EntityId
                 LEFT JOIN ORG.Position PS ON PS.Id = OPM.PositionId
                 ) OPMB
                ) OPMBF ON OPMBF.OperationMasterId = OM.Id
                LEFT JOIN (
                --[Total Present] ,[ONROLL]
                SELECT EM.OperationMasterId
                 ,EN.Id EntityId
                 ,POS.Id PositionId
                 ,SUM([Status]) TotalPresent
                 ,COUNT(EM.SystemId) ONROLL
                FROM EmployeeInformation EM
                LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EM.BudgetCode
                LEFT JOIN ORG.Entity EN ON EN.Id = MB.EntityId
                LEFT JOIN ORG.Position POS ON POS.Id = MB.PositionId --EM.PositionId
                LEFT JOIN (
                 SELECT DayStatus
                  ,EmpSystemID
                  ,[Status] = CASE 
                   WHEN DayStatus IS NULL
                    THEN 0
                   WHEN DayStatus IS NOT NULL
                    THEN 1
                   ELSE 0
                   END
                 FROM AttdnProcessData
                 WHERE DayStatus IN (
                   'LP'
                   ,'HDP'
                   ,'P'
                   )
                  AND WorkDate = REPLACE(Convert(varchar(11),getdate(),106),' ','-')
                 ) AD ON AD.EmpSystemID = EM.SystemId
                WHERE EM.EmployeeStatus = 'Active'
                GROUP BY EM.OperationMasterId
                 ,EN.Id
                 ,POS.Id
                ) E ON E.OperationMasterId = OPMBF.OperationMasterId
                AND E.PositionID = OPMBF.PositionId
                AND E.EntityId = OPMBF.EntityId				
                --ORDER BY CONVERT(INT, OM.Code)
                ) as Final               
                GROUP BY OperationName ORDER BY OnRollBalance DESC";



                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #region GetSequence

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets automatic sequence. </summary>
        /// <returns>   The automatic sequence. </returns>
        ///-------------------------------------------------------------------------------------------------

        public decimal GetAutoSequence()
        {
            try
            {
                return Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        #endregion GetSequence




        public IEnumerable<object> GetEntiryWiseData(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess)
        {
            try
            {
                string _sql = "";

                string paramters = "";
                if (queryString != "")
                {
                    if (paramters == "")
                        paramters += " EntityId in(" + queryString + ")";
                    else
                        paramters += " AND EntityId in(" + queryString + ")";
                }


                //if (queryStringSkill != "")
                //{
                //	if (paramters == "")
                //		paramters += " SkillId in(" + queryStringSkill + ")";
                //	else
                //		paramters += " AND SkillId in(" + queryStringSkill + ")";
                //}
                //if (queryStringOperationCode != "")
                //{
                //	if (paramters == "")
                //		paramters += " OperationCode in(" + queryStringOperationCode + ")";
                //	else
                //		paramters += " AND OperationCode in(" + queryStringOperationCode + ")";
                //}

                //if (queryStringProcess != "")
                //{
                //	if (paramters == "")
                //		paramters += " ProcessId in(" + queryStringProcess + ")";
                //	else
                //		paramters += " AND ProcessId in(" + queryStringProcess + ")";
                //}


                //if (queryStringGrouping != "")
                //{
                //	if (paramters == "")
                //		paramters += " SkillGroupID in(" + queryStringGrouping + ")";
                //	else
                //		paramters += " AND SkillGroupID in(" + queryStringGrouping + ")";
                //}
                //if (queryStringMachineCategory != "")
                //{
                //	if (paramters == "")
                //		paramters += " MachineCategoryID in(" + queryStringMachineCategory + ")";
                //	else
                //		paramters += " AND MachineCategoryID in(" + queryStringMachineCategory + ")";
                //}
                //if (queryStringMachineSubCategoryCode != "")
                //{
                //	if (paramters == "")
                //		paramters += " MachineSubCategoryId in(" + queryStringMachineSubCategoryCode + ")";
                //	else
                //		paramters += " AND MachineSubCategoryId in(" + queryStringMachineSubCategoryCode + ")";
                //}
                //if (queryStringOperationCategoryId != "")
                //{
                //	if (paramters == "")
                //		paramters += " OperationCategoryId in(" + queryStringOperationCategoryId + ")";
                //	else
                //		paramters += " AND OperationCategoryId in(" + queryStringOperationCategoryId + ")";
                //}
                //if (queryStringOnRoll != "")
                //{
                //	if (paramters == "")
                //		paramters += " ISNULL(OnRoll,0)  in(" + queryStringOnRoll + ")";
                //	else
                //		paramters += " AND ISNULL(OnRoll,0)  in(" + queryStringOnRoll + ")";
                //}
                //if (queryStringTotalPresent != "")
                //{
                //	if (paramters == "")
                //		paramters += " ISNULL(TotalPresent,0)  in(" + queryStringTotalPresent + ")";
                //	else
                //		paramters += " AND ISNULL(TotalPresent,0)  in(" + queryStringTotalPresent + ")";
                //}

                //if (queryStringOnRollShort != "")
                //{
                //	if (paramters == "")
                //		paramters += " ISNULL(OnRollShort,0) in(" + queryStringOnRollShort + ")";
                //	else
                //		paramters += " AND ISNULL(OnRollShort,0) in(" + queryStringOnRollShort + ")";
                //}
                //if (queryStringOnRollExcess != "")
                //{
                //	if (paramters == "")
                //		paramters += " ISNULL(OnRollExcess,0) in(" + queryStringOnRollExcess + ")";
                //	else
                //		paramters += " AND ISNULL(OnRollExcess,0) in(" + queryStringOnRollExcess + ")";
                //}
                //if (queryStringPresentShort != "")
                //{
                //	if (paramters == "")
                //		paramters += " ISNULL(PresentShort,0) in(" + queryStringPresentShort + ")";
                //	else
                //		paramters += " AND ISNULL(PresentShort,0) in(" + queryStringPresentShort + ")";
                //}
                //if (queryStringPresentExcess != "")
                //{
                //	if (paramters == "")
                //		paramters += " ISNULL(presentExcess,0) in(" + queryStringPresentExcess + ")";
                //	else
                //		paramters += " AND ISNULL(presentExcess,0) in(" + queryStringPresentExcess + ")";
                //}

                _sql = @"SELECT EntityName,OperationId
	                        ,OperationCode
	                        ,OperationName
	                        ,OperationCategoryId
	                        ,OperationCategoryName
	                        ,MachineMasterId
	                        ,MachineMaster MachineMaster
	                        ,MachineCategoryId
	                        ,MachineCategory
	                        --,MachineSubCategoryId
	                        --,MachineSubCategory
	                        ,SkillId
	                        ,Type
	                        ,Skill
	                        ,SkillGroupId
	                        ,SkillGroupe
	                        --,Position
	                        --,EntityId
	                        --,EntityName
	                        ,ProcessId
	                        ,Process Process
	                        ,StandardSalary StandardSalary
	                        ,ManpowerBudget ManpowerBudget
	                        ,OnRoll OnRoll
	                        ,OnRollShort OnRollShort
	                        ,OnRollExcess OnRollExcess
	                        ,TotalPresent TotalPresent
	                        ,PresentShort PresentShort
	                        ,PresentExcess PresentExcess
                        FROM (
	                       SELECT OperationId, OperationCode, OperationName, OperationCategoryId, OperationCategoryName, MachineMasterId, MachineMasterName MachineMaster, MachineCategoryId, MachineCategory, MachineSubCategoryId, MachineSubCategory, SkillId, Type, Skill, SkillGroupId, SkillGroupe, 
								EntityId, EntityName, ProcessId, ProcessName Process, ManpowerBudget, StandardSalary, OnRoll, OnRollShort, OnRollExcess, TotalPresent, PresentShort, PresentExcess
								FROM (
									SELECT OperationMaster.Id OperationId, OperationMaster.Code OperationCode, OperationMaster.UserName OperationName, OperationMaster.OperationActivityId, OperationMaster.Type, OperationActivity.UserName OperationActivityName, OperationMaster.OperationTypeId, OperationType.UserName OperationTypeName, OperationMaster.OperationCategoryId, OperationCategory.UserName OperationCategoryName, OperationMaster.Type OperationOrActivity, ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId, ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory, SkillId = CASE WHEN OperationMaster.Type = 'Activity' THEN OperationMaster.SkillId ELSE MachineMaster.SkillId END, Skill = CASE WHEN OperationMaster.Type = 'Activity' THEN Skill.UserName ELSE MachineSkill.UserName END,
										OperationMaster.SkillGroupId, SkillGrouping.UserName SkillGroupe, SkillGrouping.StandardSalary, OperationMaster.LegalDesignationId, LegalDesignation.UserName LegalDesignationName, OperationMaster.ProcessId, Process.UserName ProcessName, OperationMaster.ProposedSalary, IsNull(OperationManpowerBudget.EntityId, 'Blank') EntityId, ISNULL(Entity.UserName, 'Blank') EntityName, 
										ISNULL(OperationManpowerBudget.ManpowerBudget, 0) ManpowerBudget, ISNULL(OnRoll.OnRollManpower, 0) OnRoll, ISNULL(Present.DayPresentCount, 0) TotalPresent, OnRollShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) ELSE 0 END, OnRollExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(
														OnRoll.OnRollManpower, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))) ELSE 0 END, PresentShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) ELSE 0 END, PresentExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))) ELSE 0 END
									FROM MST.OperationMaster
									LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
									LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
									LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
									LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
									LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
									LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
									LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
									LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
									LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
									LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
									LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
									LEFT OUTER JOIN (
										Select CompanyGroupId,EntityId,OperationMasterId,sum(ManpowerBudget) ManpowerBudget from mst.OperationPositionMPBudget group by CompanyGroupId,EntityId,OperationMasterId
										) OperationManpowerBudget on OperationManpowerBudget.OperationMasterId = OperationMaster.Id and OperationManpowerBudget.CompanyGroupId = OperationMaster.CompanyGroupId
									LEFT OUTER JOIN ORG.Entity ON OperationManpowerBudget.EntityId = Entity.Id	
									LEFT OUTER JOIN (
										SELECT ManpowerBudget.EntityId,ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) OnRollManpower
										FROM EmployeeInformation
										LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
										where EmployeeInformation.EmployeeStatus='Active'
										GROUP BY ManpowerBudget.EntityId,OperationMasterId
										) OnRoll ON OperationManpowerBudget.EntityId = OnRoll.EntityId AND OperationMaster.Id = OnRoll.OperationMasterId
									LEFT OUTER JOIN (
										SELECT ManpowerBudget.EntityId, ManpowerBudget.PositionId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) DayPresentCount
										FROM EmployeeInformation
										LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
										LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
										WHERE AttdnProcessData.DayStatus IN (
												SELECT DayType
												FROM DayType
												WHERE Category = 'Present'
													OR Category = 'Late'
												)
											AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
										GROUP BY ManpowerBudget.EntityId, ManpowerBudget.PositionId, OperationMasterId
										) Present ON OperationManpowerBudget.EntityId = Present.EntityId
										AND OperationMaster.Id = Present.OperationMasterId
									) Main
	                        ) xyz
						where " + paramters + @" ANd OperationCode='" + queryStringOperationCode + "'";


                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }



        public IEnumerable<object> GetSkillMasterDetailSummary(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess)
        {
            try
            {
                string _sql = "";

                string paramters = "";

                if (queryString != "")
                {
                    if (paramters == "")
                        paramters += "isnull(EntityId,'') in(" + queryString + ")";
                    else
                        paramters += " AND isnull(EntityId,'') in(" + queryString + ")";
                }


                //if (queryStringSkill != "")
                //{
                //	if (paramters == "")
                //		paramters += "isnull(SkillId,'') in(" + queryStringSkill + ")";
                //	else
                //		paramters += " AND isnull(SkillId,'') in(" + queryStringSkill + ")";
                //}
                //if (queryStringOperationCode != "")
                //{
                //	if (paramters == "")
                //		paramters += "isnull(OperationCode,'') in(" + queryStringOperationCode + ")";
                //	else
                //		paramters += " AND isnull(OperationCode,'') in(" + queryStringOperationCode + ")";
                //}

                //if (queryStringProcess != "")
                //{
                //	if (paramters == "")
                //		paramters += " isnull(ProcessId,'') in(" + queryStringProcess + ")";
                //	else
                //		paramters += " AND isnull(ProcessId,'') in(" + queryStringProcess + ")";
                //}


                //if (queryStringGrouping != "")
                //{
                //	if (paramters == "")
                //		paramters += " isnull(SkillGroupID,'') in(" + queryStringGrouping + ")";
                //	else
                //		paramters += " AND isnull(SkillGroupID,'') in(" + queryStringGrouping + ")";
                //}
                //if (queryStringMachineCategory != "")
                //{
                //	if (paramters == "")
                //		paramters += " isnull(MachineCategoryID,'') in(" + queryStringMachineCategory + ")";
                //	else
                //		paramters += " AND isnull(MachineCategoryID,'') in(" + queryStringMachineCategory + ")";
                //}
                //if (queryStringMachineSubCategoryCode != "")
                //{
                //	if (paramters == "")
                //		paramters += " isnull(MachineSubCategoryId,'') in(" + queryStringMachineSubCategoryCode + ")";
                //	else
                //		paramters += " AND isnull(MachineSubCategoryId,'') in(" + queryStringMachineSubCategoryCode + ")";
                //}
                //if (queryStringOperationCategoryId != "")
                //{
                //	if (paramters == "")
                //		paramters += " isnull(OperationCategoryId,'') in(" + queryStringOperationCategoryId + ")";
                //	else
                //		paramters += " AND isnull(OperationCategoryId,'') in(" + queryStringOperationCategoryId + ")";
                //}
                //if (queryStringOnRoll != "")
                //{
                //	if (paramters == "")
                //		paramters += " ISNULL(OnRoll,0)  in(" + queryStringOnRoll + ")";
                //	else
                //		paramters += " AND ISNULL(OnRoll,0)  in(" + queryStringOnRoll + ")";
                //}
                //if (queryStringTotalPresent != "")
                //{
                //	if (paramters == "")
                //		paramters += " ISNULL(TotalPresent,0)  in(" + queryStringTotalPresent + ")";
                //	else
                //		paramters += " AND ISNULL(TotalPresent,0)  in(" + queryStringTotalPresent + ")";
                //}

                //if (queryStringOnRollShort != "")
                //{
                //	if (paramters == "")
                //		paramters += " ISNULL(OnRollShort,0) in(" + queryStringOnRollShort + ")";
                //	else
                //		paramters += " AND ISNULL(OnRollShort,0) in(" + queryStringOnRollShort + ")";
                //}
                //if (queryStringOnRollExcess != "")
                //{
                //	if (paramters == "")
                //		paramters += " ISNULL(OnRollExcess,0) in(" + queryStringOnRollExcess + ")";
                //	else
                //		paramters += " AND ISNULL(OnRollExcess,0) in(" + queryStringOnRollExcess + ")";
                //}
                //if (queryStringPresentShort != "")
                //{
                //	if (paramters == "")
                //		paramters += " ISNULL(PresentShort,0) in(" + queryStringPresentShort + ")";
                //	else
                //		paramters += " AND ISNULL(PresentShort,0) in(" + queryStringPresentShort + ")";
                //}
                //if (queryStringPresentExcess != "")
                //{
                //	if (paramters == "")
                //		paramters += " ISNULL(presentExcess,0) in(" + queryStringPresentExcess + ")";
                //	else
                //		paramters += " AND ISNULL(presentExcess,0) in(" + queryStringPresentExcess + ")";
                //}


                //         _sql = @"SELECT --OperationId
                //                  EntityId
                //,EntityName
                // , OperationCode
                //                  ,OperationName
                //                  --,OperationCategoryId
                //                  --,OperationCategoryName
                //                  --,MachineMasterId
                //                  --,MachineMaster MachineMaster
                //                  --,MachineCategoryId
                //                 -- ,MachineCategory
                //                  --,MachineSubCategoryId
                //                  --,MachineSubCategory
                //                 -- ,SkillId
                //                 -- ,Type
                //                 -- ,Skill
                //                  --,SkillGroupId
                //                  --,SkillGroupe
                //                  --,Position
                //                  --,EntityId
                //                  --,EntityName
                //                  --,ProcessId
                //                  --,Process Process
                //                  ,Sum(StandardSalary) StandardSalary
                //                  ,Sum(ManpowerBudget) ManpowerBudget
                //                  ,Sum(OnRoll) OnRoll
                //                  ,Sum(OnRollShort) OnRollShort
                //                  ,Sum(OnRollExcess) OnRollExcess
                //                  ,Sum(TotalPresent) TotalPresent
                //                  ,Sum(PresentShort) PresentShort
                //                  ,Sum(PresentExcess) PresentExcess
                //                 FROM (
                //                 SELECT OperationId, OperationCode, OperationName, OperationCategoryId, OperationCategoryName, MachineMasterId, MachineMasterName MachineMaster, MachineCategoryId, MachineCategory, MachineSubCategoryId, MachineSubCategory, SkillId, Type, Skill, SkillGroupId, SkillGroupe, 
                //EntityId, EntityName, ProcessId, ProcessName Process, ManpowerBudget, StandardSalary, OnRoll, OnRollShort, OnRollExcess, TotalPresent, PresentShort, PresentExcess
                //FROM (
                //	SELECT OperationMaster.Id OperationId, OperationMaster.Code OperationCode, OperationMaster.UserName OperationName, OperationMaster.OperationActivityId, OperationMaster.Type, OperationActivity.UserName OperationActivityName, OperationMaster.OperationTypeId, OperationType.UserName OperationTypeName, OperationMaster.OperationCategoryId, OperationCategory.UserName OperationCategoryName, OperationMaster.Type OperationOrActivity, ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId, ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory, SkillId = CASE WHEN OperationMaster.Type = 'Activity' THEN OperationMaster.SkillId ELSE MachineMaster.SkillId END, Skill = CASE WHEN OperationMaster.Type = 'Activity' THEN Skill.UserName ELSE MachineSkill.UserName END,
                //		OperationMaster.SkillGroupId, SkillGrouping.UserName SkillGroupe, SkillGrouping.StandardSalary, OperationMaster.LegalDesignationId, LegalDesignation.UserName LegalDesignationName, OperationMaster.ProcessId, Process.UserName ProcessName, OperationMaster.ProposedSalary, IsNull(OperationManpowerBudget.EntityId, 'Blank') EntityId, ISNULL(Entity.UserName, 'Blank') EntityName, 
                //		ISNULL(OperationManpowerBudget.ManpowerBudget, 0) ManpowerBudget, ISNULL(OnRoll.OnRollManpower, 0) OnRoll, ISNULL(Present.DayPresentCount, 0) TotalPresent, OnRollShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) ELSE 0 END, OnRollExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(
                //						OnRoll.OnRollManpower, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))) ELSE 0 END, PresentShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) ELSE 0 END, PresentExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))) ELSE 0 END
                //	FROM MST.OperationMaster
                //	LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
                //	LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
                //	LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
                //	LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
                //	LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
                //	LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
                //	LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
                //	LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
                //	LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
                //	LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
                //	LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
                //	LEFT OUTER JOIN (
                //		Select CompanyGroupId,EntityId,OperationMasterId,sum(ManpowerBudget) ManpowerBudget from mst.OperationPositionMPBudget group by CompanyGroupId,EntityId,OperationMasterId
                //		) OperationManpowerBudget on OperationManpowerBudget.OperationMasterId = OperationMaster.Id and OperationManpowerBudget.CompanyGroupId = OperationMaster.CompanyGroupId
                //	LEFT OUTER JOIN ORG.Entity ON OperationManpowerBudget.EntityId = Entity.Id	
                //	LEFT OUTER JOIN (
                //		SELECT ManpowerBudget.EntityId,ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) OnRollManpower
                //		FROM EmployeeInformation
                //		LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //		where EmployeeInformation.EmployeeStatus='Active'
                //		GROUP BY ManpowerBudget.EntityId,OperationMasterId
                //		) OnRoll ON OperationManpowerBudget.EntityId = OnRoll.EntityId AND OperationMaster.Id = OnRoll.OperationMasterId
                //	LEFT OUTER JOIN (
                //		SELECT ManpowerBudget.EntityId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) DayPresentCount
                //		FROM EmployeeInformation
                //		LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                //		LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
                //		WHERE AttdnProcessData.DayStatus IN (
                //				SELECT DayType
                //				FROM DayType
                //				WHERE Category = 'Present'
                //					OR Category = 'Late'
                //				)
                //			AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
                //		GROUP BY ManpowerBudget.EntityId, OperationMasterId
                //		) Present ON OperationManpowerBudget.EntityId = Present.EntityId
                //		AND OperationMaster.Id = Present.OperationMasterId
                //	) Main 
                //                  ) xyz
                //                 where " + paramters + "" +
                //                 "GROUP BY OperationName ,OperationCode,EntityName,EntityId Order By EntityName,OperationName";//ManpowerBudget <> 0 AND 
                _sql = @"SELECT --OperationId

                            EntityId
							,EntityName
							 , OperationCode
	                        ,OperationName
							,OperationActivity
                            --,OperationCategoryId
                            --,OperationCategoryName
                            --,MachineMasterId
                            --,MachineMaster MachineMaster

                            --,MachineCategoryId
                           -- ,MachineCategory
                            --,MachineSubCategoryId
                            --,MachineSubCategory
                           -- ,SkillId
                           -- ,Type
                           -- ,Skill
                            --,SkillGroupId
                            --,SkillGroupe
                            --,Position
                            --,EntityId
                            --,EntityName
                            --,ProcessId
                            --,Process Process
                            , Sum(StandardSalary) StandardSalary
	                        ,Sum(ManpowerBudget) ManpowerBudget
	                        ,Sum(OnRoll) OnRoll
	                        ,Sum(OnRollShort) OnRollShort
	                        ,Sum(OnRollExcess) OnRollExcess
	                        ,Sum(TotalPresent) TotalPresent
	                        ,Sum(PresentShort) PresentShort
	                        ,Sum(PresentExcess) PresentExcess
                        FROM(
                           SELECT OperationActivity, OperationId, OperationCode, OperationName, OperationCategoryId, OperationCategoryName, MachineMasterId, MachineMasterName MachineMaster, MachineCategoryId, MachineCategory, MachineSubCategoryId, MachineSubCategory, SkillId, Type, Skill, SkillGroupId, SkillGroupe,
                            EntityId, EntityName, ProcessId, ProcessName Process, ManpowerBudget, StandardSalary, OnRoll, OnRollShort, OnRollExcess, TotalPresent, PresentShort, PresentExcess

                            FROM(
                                SELECT OA.UserName OperationActivity, OperationMaster.Id OperationId, OperationMaster.Code OperationCode, OperationMaster.UserName OperationName, OperationMaster.OperationActivityId, OperationMaster.Type, OperationActivity.UserName OperationActivityName, OperationMaster.OperationTypeId, OperationType.UserName OperationTypeName, OperationMaster.OperationCategoryId, OperationCategory.UserName OperationCategoryName, OperationMaster.Type OperationOrActivity, ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId, ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory, SkillId = CASE WHEN OperationMaster.Type = 'Activity' THEN OperationMaster.SkillId ELSE MachineMaster.SkillId END, Skill = CASE WHEN OperationMaster.Type = 'Activity' THEN Skill.UserName ELSE MachineSkill.UserName END,
                                    OperationMaster.SkillGroupId, SkillGrouping.UserName SkillGroupe, SkillGrouping.StandardSalary, OperationMaster.LegalDesignationId, LegalDesignation.UserName LegalDesignationName, OperationMaster.ProcessId, Process.UserName ProcessName, OperationMaster.ProposedSalary, IsNull(OperationManpowerBudget.EntityId, 'Blank') EntityId, ISNULL(Entity.UserName, 'Blank') EntityName,
                                    ISNULL(OperationManpowerBudget.ManpowerBudget, 0) ManpowerBudget, ISNULL(OnRoll.OnRollManpower, 0) OnRoll, ISNULL(Present.DayPresentCount, 0) TotalPresent, OnRollShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) ELSE 0 END, OnRollExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(
                                                    OnRoll.OnRollManpower, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))) ELSE 0 END, PresentShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) ELSE 0 END, PresentExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))) ELSE 0 END

                                FROM MST.OperationMaster
                                LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
                                LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
                                LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
                                LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
                                LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
                                LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
                                LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
                                LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
                                LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
                                LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
                                LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
                                LEFT OUTER JOIN[HKP].[OperationActivity] OA ON OA.Id = OperationMaster.OperationActivityId
                                LEFT OUTER JOIN(
                                    Select CompanyGroupId, EntityId, OperationMasterId, sum(ManpowerBudget) ManpowerBudget from mst.OperationPositionMPBudget group by CompanyGroupId, EntityId, OperationMasterId
                                    ) OperationManpowerBudget on OperationManpowerBudget.OperationMasterId = OperationMaster.Id and OperationManpowerBudget.CompanyGroupId = OperationMaster.CompanyGroupId

                                LEFT OUTER JOIN ORG.Entity ON OperationManpowerBudget.EntityId = Entity.Id
                                LEFT OUTER JOIN(
                                    SELECT ManpowerBudget.EntityId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) OnRollManpower
                                    FROM EmployeeInformation
                                    LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                                    where EmployeeInformation.EmployeeStatus = 'Active'
                                    GROUP BY ManpowerBudget.EntityId, OperationMasterId
                                    ) OnRoll ON OperationManpowerBudget.EntityId = OnRoll.EntityId AND OperationMaster.Id = OnRoll.OperationMasterId

                                LEFT OUTER JOIN(
                                    SELECT ManpowerBudget.EntityId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) DayPresentCount
                                    FROM EmployeeInformation
                                    LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
                                    LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
                                    WHERE AttdnProcessData.DayStatus IN(
                                            SELECT DayType
                                            FROM DayType
                                            WHERE Category = 'Present'
                                                OR Category = 'Late'
                                            )

                                        AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
                                    GROUP BY ManpowerBudget.EntityId, OperationMasterId
                                    ) Present ON OperationManpowerBudget.EntityId = Present.EntityId
                                    AND OperationMaster.Id = Present.OperationMasterId
                                ) Main
                            ) xyz
                        where " + paramters + "" +
                        "GROUP BY OperationName,OperationCode,EntityName,EntityId,OperationActivity   Order By EntityName, OperationName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public IEnumerable<object> Designation()
        {
            try
            {
                var _sql = "";

                _sql = @"SELECT	 EntityId
		                    ,EntityName
			                    ,OperationCode	                    
			                    ,LegalDesignation			
	                        ,Sum(StandardSalary) StandardSalary
	                        ,Sum(ManpowerBudget) ManpowerBudget
	                        ,Sum(OnRoll) OnRoll
	                        ,Sum(OnRollShort) OnRollShort
	                        ,Sum(OnRollExcess) OnRollExcess
	                        ,Sum(TotalPresent) TotalPresent
	                        ,Sum(PresentShort) PresentShort
	                        ,Sum(PresentExcess) PresentExcess
                        FROM (
	                        SELECT LegalDesignation,OperationId, OperationCode, OperationName, OperationCategoryId, OperationCategoryName, MachineMasterId, MachineMasterName MachineMaster, MachineCategoryId, MachineCategory, MachineSubCategoryId, MachineSubCategory, SkillId, Type, Skill, SkillGroupId, SkillGroupe, 
		                    EntityId, EntityName, ProcessId, ProcessName Process, ManpowerBudget, StandardSalary, OnRoll, OnRollShort, OnRollExcess, TotalPresent, PresentShort, PresentExcess
		                    FROM (
			                    SELECT onroll.LegalDesignation, OperationMaster.Id OperationId, OperationMaster.Code OperationCode, OperationMaster.UserName OperationName, OperationMaster.OperationActivityId, OperationMaster.Type, OperationActivity.UserName OperationActivityName, OperationMaster.OperationTypeId, OperationType.UserName OperationTypeName, OperationMaster.OperationCategoryId, OperationCategory.UserName OperationCategoryName, OperationMaster.Type OperationOrActivity, ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId, ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory, SkillId = CASE WHEN OperationMaster.Type = 'Activity' THEN OperationMaster.SkillId ELSE MachineMaster.SkillId END, Skill = CASE WHEN OperationMaster.Type = 'Activity' THEN Skill.UserName ELSE MachineSkill.UserName END,
				                    OperationMaster.SkillGroupId, SkillGrouping.UserName SkillGroupe, SkillGrouping.StandardSalary, OperationMaster.LegalDesignationId, LegalDesignation.UserName LegalDesignationName, OperationMaster.ProcessId, Process.UserName ProcessName, OperationMaster.ProposedSalary, IsNull(OperationManpowerBudget.EntityId, 'Blank') EntityId, ISNULL(Entity.UserName, 'Blank') EntityName, 
				                    ISNULL(OperationManpowerBudget.ManpowerBudget, 0) ManpowerBudget, ISNULL(OnRoll.OnRollManpower, 0) OnRoll, ISNULL(Present.DayPresentCount, 0) TotalPresent, OnRollShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) ELSE 0 END, OnRollExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(
								                    OnRoll.OnRollManpower, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))) ELSE 0 END, PresentShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) ELSE 0 END, PresentExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))) ELSE 0 END
			                    FROM MST.OperationMaster
			                    LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
			                    LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
			                    LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
			                    LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
			                    LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
			                    LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
			                    LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
			                    LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
			                    LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
			                    LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
			                    LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
			                    LEFT OUTER JOIN (
				                    Select CompanyGroupId,EntityId,OperationMasterId,sum(ManpowerBudget) ManpowerBudget from mst.OperationPositionMPBudget group by CompanyGroupId,EntityId,OperationMasterId
				                    ) OperationManpowerBudget on OperationManpowerBudget.OperationMasterId = OperationMaster.Id and OperationManpowerBudget.CompanyGroupId = OperationMaster.CompanyGroupId
			                    LEFT OUTER JOIN ORG.Entity ON OperationManpowerBudget.EntityId = Entity.Id	
			                    LEFT OUTER JOIN (
				                    SELECT  d.UserName LegalDesignation, ManpowerBudget.EntityId,ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) OnRollManpower
				                    FROM EmployeeInformation
				                    LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
				                    LEFT OUTER JOIN HKP.LegalDesignation d on d.Id= EmployeeInformation.LegalDesignationId
				                    where EmployeeInformation.EmployeeStatus='Active'
				                    GROUP BY ManpowerBudget.EntityId,OperationMasterId,d.UserName
				                    ) OnRoll ON OperationManpowerBudget.EntityId = OnRoll.EntityId AND OperationMaster.Id = OnRoll.OperationMasterId
			                    LEFT OUTER JOIN (
				                    SELECT ManpowerBudget.EntityId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) DayPresentCount
				                    FROM EmployeeInformation
				                    LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
				                    LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
				                    WHERE AttdnProcessData.DayStatus IN (
						                    SELECT DayType
						                    FROM DayType
						                    WHERE Category = 'Present'
							                    OR Category = 'Late'
						                    )
					                    AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
				                    GROUP BY ManpowerBudget.EntityId, OperationMasterId
				                    ) Present ON OperationManpowerBudget.EntityId = Present.EntityId
				                    AND OperationMaster.Id = Present.OperationMasterId
			                    ) Main 
	                        ) xyz
                        --where  isnull(EntityId,'') in('','Blank','4','5','6','3') AND OperationCode='1167'
	                    GROUP BY OperationCode,EntityName,EntityId,LegalDesignation Order By OperationCode";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IWorkbook MatrixReport(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string MaterialId, string ArticleId,string queryString)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                var sheet2 = workbook.Worksheets[1];
                var Head = "";
                
                    Head = "Operation Activity Wise Employee Report";
                

                MatrixReport1(ref sheet1, ref sheet2, report, Head, "Summary", companyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, MaterialId, ArticleId, queryString);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void MatrixReport1(ref IWorksheet sheet1, ref IWorksheet sheet2, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string MaterialId, string ArticleId, string queryString)
        {
            string _sql = "";

            string paramters = "";

            if (queryString != "")
            {
                if (paramters == "")
                    paramters += "isnull(EntityId,'') in(" + queryString + ")";
                else
                    paramters += " AND isnull(EntityId,'') in(" + queryString + ")";
            }

            var cmdText = "";
           

                cmdText = @"SELECT --OperationId
	                        EntityId
							,EntityName
							 , OperationCode
	                        ,OperationName
							,OperationActivity
	                        --,OperationCategoryId
	                        --,OperationCategoryName
	                        --,MachineMasterId
	                        --,MachineMaster MachineMaster
	                        --,MachineCategoryId
	                       -- ,MachineCategory
	                        --,MachineSubCategoryId
	                        --,MachineSubCategory
	                       -- ,SkillId
	                       -- ,Type
	                       -- ,Skill
	                        --,SkillGroupId
	                        --,SkillGroupe
	                        --,Position
	                        --,EntityId
	                        --,EntityName
	                        --,ProcessId
	                        --,Process Process
	                        ,Sum(StandardSalary) StandardSalary
	                        ,Sum(ManpowerBudget) ManpowerBudget
	                        ,Sum(OnRoll) OnRoll
	                        ,Sum(OnRollShort) OnRollShort
	                        ,Sum(OnRollExcess) OnRollExcess
	                        ,Sum(TotalPresent) TotalPresent
	                        ,Sum(PresentShort) PresentShort
	                        ,Sum(PresentExcess) PresentExcess
                        FROM (
	                       SELECT OperationActivity , OperationId, OperationCode, OperationName, OperationCategoryId, OperationCategoryName, MachineMasterId, MachineMasterName MachineMaster, MachineCategoryId, MachineCategory, MachineSubCategoryId, MachineSubCategory, SkillId, Type, Skill, SkillGroupId, SkillGroupe, 
							EntityId, EntityName, ProcessId, ProcessName Process, ManpowerBudget, StandardSalary, OnRoll, OnRollShort, OnRollExcess, TotalPresent, PresentShort, PresentExcess
							FROM (
								SELECT OA.UserName OperationActivity ,OperationMaster.Id OperationId, OperationMaster.Code OperationCode, OperationMaster.UserName OperationName, OperationMaster.OperationActivityId, OperationMaster.Type, OperationActivity.UserName OperationActivityName, OperationMaster.OperationTypeId, OperationType.UserName OperationTypeName, OperationMaster.OperationCategoryId, OperationCategory.UserName OperationCategoryName, OperationMaster.Type OperationOrActivity, ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId, ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory, SkillId = CASE WHEN OperationMaster.Type = 'Activity' THEN OperationMaster.SkillId ELSE MachineMaster.SkillId END, Skill = CASE WHEN OperationMaster.Type = 'Activity' THEN Skill.UserName ELSE MachineSkill.UserName END,
									OperationMaster.SkillGroupId, SkillGrouping.UserName SkillGroupe, SkillGrouping.StandardSalary, OperationMaster.LegalDesignationId, LegalDesignation.UserName LegalDesignationName, OperationMaster.ProcessId, Process.UserName ProcessName, OperationMaster.ProposedSalary, IsNull(OperationManpowerBudget.EntityId, 'Blank') EntityId, ISNULL(Entity.UserName, 'Blank') EntityName, 
									ISNULL(OperationManpowerBudget.ManpowerBudget, 0) ManpowerBudget, ISNULL(OnRoll.OnRollManpower, 0) OnRoll, ISNULL(Present.DayPresentCount, 0) TotalPresent, OnRollShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) ELSE 0 END, OnRollExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(
													OnRoll.OnRollManpower, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))) ELSE 0 END, PresentShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) ELSE 0 END, PresentExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))) ELSE 0 END
								FROM MST.OperationMaster
								LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
								LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
								LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
								LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
								LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
								LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
								LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
								LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
								LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
								LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
								LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
								LEFT OUTER JOIN [HKP].[OperationActivity] OA ON OA.Id=OperationMaster.OperationActivityId
								LEFT OUTER JOIN (
									Select CompanyGroupId,EntityId,OperationMasterId,sum(ManpowerBudget) ManpowerBudget from mst.OperationPositionMPBudget group by CompanyGroupId,EntityId,OperationMasterId
									) OperationManpowerBudget on OperationManpowerBudget.OperationMasterId = OperationMaster.Id and OperationManpowerBudget.CompanyGroupId = OperationMaster.CompanyGroupId
								LEFT OUTER JOIN ORG.Entity ON OperationManpowerBudget.EntityId = Entity.Id	
								LEFT OUTER JOIN (
									SELECT ManpowerBudget.EntityId,ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) OnRollManpower
									FROM EmployeeInformation
									LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
									where EmployeeInformation.EmployeeStatus='Active'
									GROUP BY ManpowerBudget.EntityId,OperationMasterId
									) OnRoll ON OperationManpowerBudget.EntityId = OnRoll.EntityId AND OperationMaster.Id = OnRoll.OperationMasterId
								LEFT OUTER JOIN (
									SELECT ManpowerBudget.EntityId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) DayPresentCount
									FROM EmployeeInformation
									LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
									LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
									WHERE AttdnProcessData.DayStatus IN (
											SELECT DayType
											FROM DayType
											WHERE Category = 'Present'
												OR Category = 'Late'
											)
										AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
									GROUP BY ManpowerBudget.EntityId, OperationMasterId
									) Present ON OperationManpowerBudget.EntityId = Present.EntityId
									AND OperationMaster.Id = Present.OperationMasterId
								) Main 
								
	                        ) xyz					
							
                        where " + paramters + @"

						GROUP BY 
						OperationName 
						,OperationCode
						,EntityName
						,EntityId
						,OperationActivity 
						Order By EntityName,OperationName,OperationActivity";
            

            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();
            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");
            var _row = 5;      
            var _rowL = _row;
            var row = _row + 1;


            var sheet1headreColIndex = 1;
            //var sheet2headreColIndex = 1;
            _rowL += 1;
            var Row_Total_Start = _rowL;
            //Receive
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "EntityName");
            var colEntityName = sheet1headreColIndex;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "OperationCode");
            //var colOperationCode = sheet1headreColIndex;
            //sheet1headreColIndex++;




            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "OperationName");
            var colOperationName = sheet1headreColIndex;
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "OperationActivity");
            var colOperationActivity = sheet1headreColIndex;
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ManpowerBudget");
            var colManpowerBudget = sheet1headreColIndex;
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "OnRoll");
            var colOnRoll = sheet1headreColIndex;
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "OnRollShort");
            var colOnRollShort = sheet1headreColIndex;
            sheet1headreColIndex++;

            //Issue
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "OnRollExcess");
            var colOnRollExcess = sheet1headreColIndex;
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "TotalPresent");
            var colTotalPresent = sheet1headreColIndex;
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "PresentShort");
            var colPresentShort = sheet1headreColIndex;
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "PresentExcess");
            var colPresentExcess = sheet1headreColIndex;
           

          //  var balanceQty = 0.00;
            List<string> list = new List<string>();
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;
                var rcvid = inventoryMaterialList.Rows[n]["EntityName"].ToString();
                //if (list.Contains(rcvid))
                //{

                //}
                //else//nai
                //{
                //list.Add(rcvid);
                
                     report.SetText(ref sheet1, _rowL, colEntityName, inventoryMaterialList.Rows[n]["EntityName"].ToString());
                report.SetText(ref sheet1, _rowL, colOperationName, inventoryMaterialList.Rows[n]["OperationName"].ToString());
                   // report.SetText(ref sheet1, _rowL, colRCVMRNo, rcvid);
                    report.SetText(ref sheet1, _rowL, colManpowerBudget, inventoryMaterialList.Rows[n]["ManpowerBudget"].ToString());

                    report.SetText(ref sheet1, _rowL, colOnRoll, inventoryMaterialList.Rows[n]["OnRoll"].ToString());
                    report.SetText(ref sheet1, _rowL, colOnRollShort, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OnRollShort"].ToString()));
                    report.SetText(ref sheet1, _rowL, colOnRollExcess, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OnRollExcess"].ToString()));
                    report.SetText(ref sheet1, _rowL, colTotalPresent, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalPresent"].ToString()));

                    //balanceQty = clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RcvQty"].ToString()) - clsStaticInfo.dbl(inventoryMaterialList.Compute("SUM(IssueQty)", "Id = '" + rcvid + "'").ToString());
                    //report.SetText(ref sheet1, _rowL, colBalanceQty, balanceQty);
                    //report.SetText(ref sheet1, _rowL, colBalanceQty, balanceQty);
                    //if (balanceQty == 0)
                    //{
                    //    var colBalanceRate1 = 0;
                    //    report.SetText(ref sheet1, _rowL, colBalanceQty, colBalanceRate1);

                    //}
                    //else
                    //{
                    //    report.SetText(ref sheet1, _rowL, colBalanceRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BalanceRate"].ToString()));
                    //    report.SetText(ref sheet1, _rowL, colBalanceAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BalanceRate"].ToString()) * balanceQty);
                    //}



              //  }



                report.SetText(ref sheet1, _rowL, colPresentShort, inventoryMaterialList.Rows[n]["PresentShort"].ToString());
                report.SetText(ref sheet1, _rowL, colPresentExcess, inventoryMaterialList.Rows[n]["PresentExcess"].ToString());
               // report.SetText(ref sheet1, _rowL, colType, inventoryMaterialList.Rows[n]["IssueType"].ToString());

                //report.SetText(ref sheet1, _rowL, colIssueQty, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueQty"].ToString()));
                //report.SetText(ref sheet1, _rowL, colIssueRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Rate"].ToString()));
                //report.SetText(ref sheet1, _rowL, colIssueAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueAmount"].ToString()));


                //report.SetText(ref sheet1, _rowL, colBalanceQty, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BalanceQty"].ToString()));

                //report.SetText(ref sheet1, _rowL, colBalanceRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BalanceRate"].ToString()));
                //report.SetText(ref sheet1, _rowL, colBalanceAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BalanceAmount"].ToString()));

            }

            //_rowL++;
            //sheet1.Range[Row_Total_Start, colRCVDate, _rowL, colRCVAmount].BorderAround(ExcelLineStyle.Thin);
            //sheet1.Range[Row_Total_Start, colRCVDate, _rowL, colRCVRate].BorderInside(ExcelLineStyle.Hair);

            //sheet1.Range[Row_Total_Start, colIssueDate, _rowL, colIssueAmount].BorderAround(ExcelLineStyle.Thin);
            //sheet1.Range[Row_Total_Start, colIssueDate, _rowL, colIssueRate].BorderInside(ExcelLineStyle.Hair);

            //sheet1.Range[Row_Total_Start, colBalanceQty, _rowL, colBalanceAmount].BorderAround(ExcelLineStyle.Thin);
            //sheet1.Range[Row_Total_Start, colBalanceQty, _rowL, colBalanceRate].BorderInside(ExcelLineStyle.Hair);


            #region sumCalc

            _rowL++;
            sheet1.Range[_rowL, 1, _rowL, 2].Merge();
            report.SetText(ref sheet1, _rowL, 1, "Total :", true);
            //report.SetText(ref sheet2, _rowL, 1, "Total :", true);
            sheet1.Range[_rowL, 1, _rowL, 2].CellStyle.Font.Underline = ExcelUnderline.Double;

            //sheet1.Range[_rowL, colRCVQuantity].Formula = "=SUM(" + report.GetColumnNameForXls(colRCVQuantity) + Row_Total_Start + ":" + report.GetColumnNameForXls(colRCVQuantity) + (_rowL - 1) + ")";
            //sheet1.Range[_rowL, colRCVQuantity].NumberFormat = report.NumberFormatDecimalTwo();
            //sheet1.Range[_rowL, colRCVQuantity].CellStyle.Font.Bold = true;
            //sheet1.Range[_rowL, 1, _rowL, 3].CellStyle.Font.Underline = ExcelUnderline.Double;

            //BorderAround(ExcelLineStyle.Thick);

            //sheet1.Range[_rowL, colRCVRate].Formula = "=SUM(" + report.GetColumnNameForXls(colRCVRate) + Row_Total_Start + ":" + report.GetColumnNameForXls(colRCVRate) + (_rowL - 1) + ")";
            //sheet1.Range[_rowL, colRCVRate].NumberFormat = report.NumberFormatDecimalTwo();
            //sheet1.Range[_rowL, colRCVRate].CellStyle.Font.Bold = true;
            //sheet1.Range[_rowL, 1, _rowL, 4].CellStyle.Font.Underline = ExcelUnderline.Double;

            //sheet1.Range[_rowL, colRCVAmount].Formula = "=SUM(" + report.GetColumnNameForXls(colRCVAmount) + Row_Total_Start + ":" + report.GetColumnNameForXls(colRCVAmount) + (_rowL - 1) + ")";
            //sheet1.Range[_rowL, colRCVAmount].NumberFormat = report.NumberFormatDecimalTwo();
            //sheet1.Range[_rowL, colRCVAmount].CellStyle.Font.Bold = true;
            //sheet1.Range[_rowL, 1, _rowL, 5].CellStyle.Font.Underline = ExcelUnderline.Double;



            //sheet1.Range[_rowL, colIssueQty].Formula = "=SUM(" + report.GetColumnNameForXls(colIssueQty) + Row_Total_Start + ":" + report.GetColumnNameForXls(colIssueQty) + (_rowL - 1) + ")";
            //sheet1.Range[_rowL, colIssueQty].NumberFormat = report.NumberFormatDecimalTwo();
            //sheet1.Range[_rowL, colIssueQty].CellStyle.Font.Bold = true;
            ////sheet1.Range[_rowL, 1, _rowL, 8].CellStyle.Font.Underline = ExcelUnderline.Double;


            //sheet1.Range[_rowL, colIssueRate].Formula = "=SUM(" + report.GetColumnNameForXls(colIssueRate) + Row_Total_Start + ":" + report.GetColumnNameForXls(colIssueRate) + (_rowL - 1) + ")";
            //sheet1.Range[_rowL, colIssueRate].NumberFormat = report.NumberFormatDecimalTwo();
            //sheet1.Range[_rowL, colIssueRate].CellStyle.Font.Bold = true;
            ////sheet1.Range[_rowL, 1, _rowL, 9].CellStyle.Font.Underline = ExcelUnderline.Double;


            //sheet1.Range[_rowL, colIssueAmount].Formula = "=SUM(" + report.GetColumnNameForXls(colIssueAmount) + Row_Total_Start + ":" + report.GetColumnNameForXls(colIssueAmount) + (_rowL - 1) + ")";
            //sheet1.Range[_rowL, colIssueAmount].NumberFormat = report.NumberFormatDecimalTwo();
            //sheet1.Range[_rowL, colIssueAmount].CellStyle.Font.Bold = true;
            ////sheet1.Range[_rowL, 1, _rowL, 10].CellStyle.Font.Underline = ExcelUnderline.Double;


            //sheet1.Range[_rowL, colBalanceQty].Formula = "=SUM(" + report.GetColumnNameForXls(colBalanceQty) + Row_Total_Start + ":" + report.GetColumnNameForXls(colBalanceQty) + (_rowL - 1) + ")";
            //sheet1.Range[_rowL, colBalanceQty].NumberFormat = report.NumberFormatDecimalTwo();
            //sheet1.Range[_rowL, colBalanceQty].CellStyle.Font.Bold = true;
            //sheet1.Range[_rowL, 1, _rowL, 11].CellStyle.Font.Underline = ExcelUnderline.Double;


            //sheet1.Range[_rowL, colBalanceRate].Formula = "=SUM(" + report.GetColumnNameForXls(colBalanceRate) + Row_Total_Start + ":" + report.GetColumnNameForXls(colBalanceRate) + (_rowL - 1) + ")";
            //sheet1.Range[_rowL, colBalanceRate].NumberFormat = report.NumberFormatDecimalTwo();
            //sheet1.Range[_rowL, colBalanceRate].CellStyle.Font.Bold = true;
            //sheet1.Range[_rowL, 1, _rowL, 12].CellStyle.Font.Underline = ExcelUnderline.Double;



            //sheet1.Range[_rowL, colBalanceAmount].Formula = "=SUM(" + report.GetColumnNameForXls(colBalanceAmount) + Row_Total_Start + ":" + report.GetColumnNameForXls(colBalanceAmount) + (_rowL - 1) + ")";
            //sheet1.Range[_rowL, colBalanceAmount].NumberFormat = report.NumberFormatDecimalTwo();
            //sheet1.Range[_rowL, colBalanceAmount].CellStyle.Font.Bold = true;
            //sheet1.Range[_rowL, 1, _rowL, 13].CellStyle.Font.Underline = ExcelUnderline.Double;


            #endregion sumCalc

            //sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            //sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Thick);





            //sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            // sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Thick);



            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            sheet1.UsedRange.CellStyle.Font.Size = 8;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);


            //sheet1.PageSetup.LeftMargin = 0.5;
            //sheet1.PageSetup.RightMargin = 0.2;
            //sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
            //sheet1.PageSetup.FitToPagesTall = 0;
            //sheet1.PageSetup.FitToPagesWide = 1;
            //sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;


        }



    }
}