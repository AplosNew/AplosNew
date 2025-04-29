#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.IE;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Logs;
using Library.Service.Machines;
using Library.Service.Organizations;
using Library.Service.Processes;
using Library.Service.Skills;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.IE
{
    public class MachineMasterUIService : Service<MachineMasterUI>, IMachineMasterUIService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IOperationActivityService _OperationActivityService;
        private readonly IOperationTypeService _OperationTypeService;
       // private readonly IOperationCategoryService _OperationCategoryService;
        private readonly IMachineCategoryService _MachineCategoryService;
        private readonly ISkillService _SkillService;
        private readonly IMachineMasterService _MachineMasterService;
        private readonly ILegalDesignationService _legalDesignationService;
        private readonly IProcessService _ProcessService;
        private readonly ISkillGroupingService _SkillGroupingService;
       




        public MachineMasterUIService(
            IRepositoryAsync<MachineMasterUI> OperationMasterRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IOperationActivityService OperationActivityService
            , IOperationTypeService OperationTypeService,
           // , IOperationCategoryService OperationCategoryService
              IMachineCategoryService MachineCategoryService
            , ISkillService SkillService
            , IProcessService ProcessService 
            , IMachineMasterService MachineMasterService
            , ILegalDesignationService legalDesignationService
            , ISkillGroupingService SkillGroupingService
        
            , IUnitOfWork unitOfWork) :
            base(OperationMasterRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _OperationActivityService = OperationActivityService;
            _OperationTypeService = OperationTypeService;
            // _OperationCategoryService = OperationCategoryService;
            _MachineCategoryService = MachineCategoryService;
            _SkillService =SkillService;
            _MachineMasterService = MachineMasterService;
            _legalDesignationService = legalDesignationService;
            _ProcessService = ProcessService;
            _SkillGroupingService = SkillGroupingService;
           

        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                //return base.Query().Select().Max(r => r.Sequence + 1);
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(Sequence),0) AS Sequence FROM MST.MachineMaster");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(MachineMasterUI), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void Check(MachineMasterUI entity)
        {
			try
			{
                CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
                CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
                CheckUniqueColumn(UniqueColumnName.StandardName, entity.StandardName, r => r.Id != entity.Id && r.StandardName == entity.StandardName);
                //CheckUniqueColumn(UniqueColumnName.Sequence, entity.Sequence.ToString(), r => r.Id != entity.Id && r.Sequence == entity.Sequence);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }


        }

      

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
        public IEnumerable<object> GetCboMachineCategory()
        {
            try
            {

                //var sql = @"Select Code +' - '+UserName As Text, Id As Value from [HKP]. MachineCategory";
                var sql = @"Select UserName As Text, Id As Value from [HKP]. MachineCategory";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

       
        public IEnumerable<object> GetCboMachineSubCategory()
        {
            try
            {

                //var sql = @"Select Code +' - '+UserName As Text, Id As Value from [HKP].MachineSubCategory";
                var sql = @"Select UserName As Text, Id As Value from [HKP].MachineSubCategory";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        //public IEnumerable<object> GetCboMachineCategory()
        //{
        //    try
        //    {

        //        return from m in _MachineCategoryService.Query().Select().OrderBy(r => r.UserName)
        //               select new { Text = m.UserName, Value = m.Id };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}

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

                var sql = @"Select Code +' - '+UserName As Text, Id As Value from [SCS].[SkillGrouping]";
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
       
        public IEnumerable<object> GetMachineMaster()

        {
            try
            {
                var sql = @"SELECT MM.Id
                                  ,CG.StandardName As CompanyGroup
                                  ,MM.Sequence
                                  ,MM.Code
                                  ,MM.ShortName
	                              ,MM.StandardName
                                  ,MM.UserName
	                              ,MC.UserName AS MachineCategory
	                              ,MSC.UserName AS MachineSubCategory
	                              ,SK.UserName AS MachineGroup
                                  ,MM.Description
                                  ,MM.MachineMake
                                  ,MM.MachineModel
                                  ,MM.MachinePerticulars
                                  ,MM.Remarks
                                  ,MM.ProductionMachineQty
                                  ,MM.SampleMachineQty
                                  ,MM.TrainingMachineQty
                                  ,MM.RentMachineQty
                                  ,MM.OtherMachineQty
								  ,MM.ConnectedPower
								  ,MM.RunningLoad
								  ,MM.ConnectedSteam
								  ,MM.RunningSteam
								  ,MM.ConnectedAir
								  ,MM.RunningAir
								  ,MM.MaintanenceScheduleApplicable
                                  ,MM.Active
                                  ,MM.MachineGroupId
                              FROM MST.MachineMaster As MM
                             LEFT JOIN ORG.CompanyGroup AS CG on CG.ID=MM.CompanyGroupID
                             LEFT JOIN  HKP.MachineCategory AS MC on MC.Id=MM.MachineCategoryId
                             LEFT JOIN HKP. MachineSubCategory AS MSC  on MSC.ID=MM.MachineSubCategoryID
                             LEFT JOIN [HKP].[MachineGroup] AS SK ON SK.ID=MM.MachineGroupId order by MM.Sequence";
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
                          ,MachineCategoryId
                          ,MachineSubCategoryId
                          ,Sequence
                          ,Code
                          ,ShortName
                          ,StandardName
                          ,UserName
                          ,Description
                          ,MachineMake
                          ,MachineModel
                          ,MachinePerticulars
                          ,Remarks
                          ,SkillId
                          ,ProductionMachineQty
                          ,SampleMachineQty
                          ,TrainingMachineQty
                          ,RentMachineQty
                          ,OtherMachineQty
		                  ,Remarks
                          ,ConnectedPower
                          ,RunningLoad
                          ,ConnectedSteam
                          ,RunningSteam
                          ,ConnectedAir
                          ,RunningAir
                          ,MaintanenceScheduleApplicable
		                  ,Active
		                  ,AddedBy
		                  ,AddedDate
		                  ,AddedFromIP
		                  ,UpdatedBy
		                  ,UpdatedDate
		                  ,UpdatedFromIP from [MST].[MachineMaster] where Id='" + id + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetDataByMachineMasterId(string id)
        {
            throw new NotImplementedException();
        }

        //public void Check(MachineMasterUI entity)
        //{
        //    throw new NotImplementedException();
        //}

       
    }
}