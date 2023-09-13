#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.IE;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Machines;
using Library.Service.Organizations;
using Library.Service.Processes;
using Library.Service.Skills;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.IE
{
    public class OperationPositionMPBudgetService : Service<OperationPositionMPBudget>, IOperationPositionMPBudgetService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IOperationActivityService _OperationActivityService;
        private readonly IOperationTypeService _OperationTypeService;
        private readonly IOperationCategoryService _OperationCategoryService;
        private readonly ISkillService _SkillService;
        private readonly IMachineMasterService _MachineMasterService;
        private readonly ILegalDesignationService _legalDesignationService;
        private readonly IProcessService _ProcessService;
        private readonly ISkillGroupingService _SkillGroupingService;
       




        public OperationPositionMPBudgetService(
            IRepositoryAsync<OperationPositionMPBudget> OperationMasterRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IOperationActivityService OperationActivityService
            , IOperationTypeService OperationTypeService
            , IOperationCategoryService OperationCategoryService
            , ISkillService SkillService
            , IProcessService ProcessService 
            , IMachineMasterService MachineMasterService
            , ILegalDesignationService legalDesignationService
            , ISkillGroupingService SkillGroupingService
            //, IOperationMasterService OperationMasterService
            , IUnitOfWork unitOfWork) :
            base(OperationMasterRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _OperationActivityService = OperationActivityService;
            _OperationTypeService = OperationTypeService;
            _OperationCategoryService = OperationCategoryService;
            _SkillService =SkillService;
            _MachineMasterService = MachineMasterService;
            _legalDesignationService = legalDesignationService;
            _ProcessService = ProcessService;
            _SkillGroupingService = SkillGroupingService;
           

        }

        #endregion Constructor

        public decimal GetAutoSequence(string OMId)
        {
            try
            {
                return base.Query(r=>r.OperationMasterId== OMId).Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(OperationMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void Check(OperationPositionMPBudget entity)
        {
            //CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
            //CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
            //CheckUniqueColumn(UniqueColumnName.StandardName, entity.StandardName, r => r.Id != entity.Id && r.StandardName == entity.StandardName);
            //CheckUniqueColumn(UniqueColumnName.Sequence, entity.Sequence.ToString(), r => r.Id != entity.Id && r.Sequence == entity.Sequence);
        }
        public IEnumerable<object> GetOperationPositionMPBudgetService(string id)
        {
            try
            {
                var sql = @"Select 
                             MP.Id
                            ,mp.OperationMasterId
                            ,mp.Sequence
                            ,En.UserName AS Entity,En.Id EntityId
                            ,P.UserName Position
                            ,SD.UserName Shiftname
                            ,SD.SystemID 
                            ,MP.Caption
                            ,Mp.ManpowerBudget
                            ,P.Id PositionId
                            FROM Mst.OperationPositionMPBudget MP
                            LEFT JOIN [ORG].[Entity] En ON En.Id=MP.EntityId
                            LEFT JOIN [dbo].[ShiftDefination] SD ON SD.SystemID=MP.ShiftId
                            LEFT JOIN [ORG].[Position] P On P.Id=MP.PositionId where mp.OperationMasterId='" + id + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetCboEntity()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"Select UserName As Text, Id As Value from [ORG].[Entity] where companyId='" + identity.CompanyId+ "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }



        public IEnumerable<object> GetCboShift()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"Select UserName As Text, SystemID As Value from [dbo].[ShiftDefination] where PlantID='" + identity.PlantId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        //public IEnumerable<object> GetCboLine()
        //{
        //    try
        //    {
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        //        var sql = @"Select ShortName As Text from [ORG].[Line] ";
        //        return _sqlRepository.GetDataCollection(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}
        public IEnumerable<object> GetCboPosition() 
        {
            try
            {

                var sql = @"Select Code +'-'+ UserName As Text, Id As Value from [ORG].[Position]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetDataByMasterOrderIdMP1(string id)
        {
            try
            {
                var sql = @"SELECT 
                           OMB.Id
                          ,OMB.CompanyGroupId
                          ,OMB.Sequence
                          ,OMB.OperationMasterId
                          ,OMB.EntityId
                          ,P.UserName
                          ,OMB.Caption
                          ,OMB.ManpowerBudget
                          ,OMB.Active
                          ,OMB.AddedBy
                          ,OMB.AddedDate
                          ,OMB.AddedFromIP
                          ,OMB.UpdatedBy
                          ,OMB.UpdatedDate
                          ,OMB.UpdatedFromIP
                          ,OMB.PositionId,OMB.OperationMasterId
                      FROM [MST].OperationPositionMPBudget OMB
					   LEFT JOIN [ORG].[Position] P On P.Id=OMB.PositionId
                       where OperationMasterId='" + id + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetDataByMasterOrderIdMP(string id) 
        {
            try
            {
                var sql = @"SELECT OMB.Id
                        ,OMB.CompanyGroupId
                        ,OMB.Sequence
                        ,OMB.OperationMasterId
                        ,OMB.EntityId
                        ,OMB.PositionId
                        ,OMB.Caption
                        ,OMB.ManpowerBudget
                        ,OMB.Active
                        ,OMB.AddedBy
                        ,OMB.AddedDate
                        ,OMB.AddedFromIP
                        ,OMB.UpdatedBy
                        ,OMB.UpdatedDate
                        ,OMB.UpdatedFromIP
	                     ,P.UserName,OMB.ShiftId
                    FROM [MST].OperationPositionMPBudget OMB
                    LEFT JOIN [ORG].[Position] P On P.Id=OMB.PositionId
                     LEFT JOIN [dbo].[ShiftDefination] SD ON SD.SystemID=OMB.ShiftId
                    where OMB.Id='" + id + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        
        //public override void Insert(SizeGroup entity)
        //{
        //    try
        //    {
        //        Check(entity);
        //        entity.Id ="SG-"+ GetPK();
        //        base.Insert(entity);
        //    }
        //    catch (Exception ex)
        //    {
        //        //throw new CustomException(ex.Message, ex,
        //        //Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
        //        //ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //        throw ex;
        //    }
        //}

        //public override void Update(SizeGroup entity)
        //{
        //    try
        //    {
        //        Check(entity);
        //        base.Update(entity);
        //    }
        //    catch (Exception ex)
        //    {
        //        //throw new CustomException(ex.Message, ex,
        //        //Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
        //        //ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //        throw ex;

        //    }
        //}

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM [HKP].[SizeGroup]";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCboCompanyGroup() 
        {
            try
            {
              
                return from m in _OperationActivityService.Query().Select().OrderBy(r => r.UserName )
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

       

        public IEnumerable<object> GetCboOperationType()
        {
            try
            {
                
                return from m in _OperationTypeService.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        // OperationSkillService

       
        public IEnumerable<object> GetCboOperationCategory()
        {
            try
            {

                return from m in _OperationCategoryService.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCboSkill()
        {
            try
            {

                return from m in _SkillService.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCboMachineMaster()
        {
            try
            {
                var sql = @"Select UserName As Text, Id As Value from mst.MachineMaster";
                return _sqlRepository.GetDataCollection(sql);

                //return from m in _MachineMasterService.Query().Select().OrderBy(r => r.UserName)
                //       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCboSkillGrouping()
        {
            try
            {

                var sql = @"Select UserName As Text, Id As Value from [SCS].[SkillGrouping]";
               return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCbolegalDesignation()
        {
            try
            {

                return from m in _legalDesignationService.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetCboProcess() 
        {
            try
            {

                return from m in _ProcessService.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
       
        public IEnumerable<object> GetOperationMaster() 
        {
            try
            {
                var sql = @"SELECT  
                                 OM.Id
                                ,CG.StandardName AS CompanyGroup
                                ,OM.Sequence
                                ,OM.Code
                                ,OM.ShortName
                                ,OM.StandardName
                                ,OM.UserName
                                ,OA.UserName AS OperationActivity
                                ,OT.UserName AS OperationType
                                ,OC.UserName AS OperationCategory
                                ,S.UserName AS Skill
                                ,OM.Type 
                                ,MM.UserName AS MachineMaster
                                ,SG.UserName AS SkillGroup
                                ,LD.UserName AS LegalDesignation
                                ,p.UserName As Process
                                ,OM.ProposedSalary
                                ,OM.Remarks
                                ,OM.Active
                                From [MST].[OperationMaster] OM
                                LEFT JOIN [ORG].CompanyGroup CG ON CG.Id=OM.CompanyGroupId
                                LEFT JOIN [HKP].[OperationActivity] OA ON OA.Id=OM.OperationActivityId
                                LEFT JOIN [HKP].[OperationType] OT ON OT.Id=OM.OperationTypeId
                                LEFT JOIN [HKP].[OperationCategory] OC ON OC.Id=OM.OperationCategoryId
                                LEFT JOIN [HKP].[Skill] S On S.Id=OM.SkillId
                                LEFT JOIN [MST].[MachineMaster] MM ON MM.Id=OM.MachineMasterId
                                LEFT JOIN [SCS].[SkillGrouping] SG ON SG.Id=OM.SkillGroupId
                                LEFT JOIN [HKP].[LegalDesignation] LD ON LD.Id=OM.LegalDesignationId
                                LEFT JOIN [HKP].[Process] P ON P.Id=OM.ProcessId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public IEnumerable<object> GetDataByMasterOrderId(string id) 
        {
            try
            {
                var sql = @"SELECT  
                             Id	
	                        ,CompanyGroupId
		                    ,[Sequence]                                          
		                    ,Code,
		                    ShortName ,
		                    StandardName,
		                    UserName,
		                    OperationActivityId,
		                    OperationTypeId,
		                    OperationCategoryId,
		                    SkillId,
		                    [Type],
		                    MachineMasterId	,
		                    SkillGroupId,
		                    LegalDesignationId,
		                    ProcessId,
		                    ProposedSalary,
		                    Remarks,
		                    Active,
		                    AddedBy,
		                    AddedDate,
		                    AddedFromIP,
		                    UpdatedBy,
		                    UpdatedDate,
		                    UpdatedFromIP from [MST].[OperationMaster] where Id='" + id +"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        


    }
}