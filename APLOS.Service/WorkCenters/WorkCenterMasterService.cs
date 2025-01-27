using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.WorkCenters;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.WorkCenters
{
    public class WorkCenterMasterService : Service<WorkCenterMaster>, IWorkCenterMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;//
        private readonly IWorkCenterMasterMachineService _workcentermastermachineservice;
        private readonly IRepositoryAsync<WorkCenterMaster> _recipeMasterRepository;
        private readonly IRepositoryAsync<WorkCenterMasterEffectiveDate> _effectiveDateRepository;
        private readonly IRepositoryAsync<WorkCenterMasterManpowerBudge> _budgetCodeRepository;
        private readonly IRepositoryAsync<WorkCenterMasterProductPriority> _productPriorityRepository;
        private readonly IRepositoryAsync<WorkCenterWiseShift> _workCenterWiseShiftRepository;
        private readonly IRepositoryAsync<WorkCenterMasterSubProcess> _WorkCenterMasterSubProcessRepository;

        public WorkCenterMasterService(
            IRepositoryAsync<WorkCenterMaster> recipeMasterRepository
            , IWorkCenterMasterMachineService workcentermastermachineservice
            , IRepositoryAsync<WorkCenterMasterEffectiveDate> effectiveDateRepository
            , IRepositoryAsync<WorkCenterMasterManpowerBudge> budgetCodeRepository
            , IRepositoryAsync<WorkCenterMasterProductPriority> productPriorityRepository
            , IRepositoryAsync<WorkCenterWiseShift> workCenterWiseShiftRepository
            , IRepositoryAsync<WorkCenterMasterSubProcess> WorkCenterMasterSubProcessRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(recipeMasterRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _recipeMasterRepository = recipeMasterRepository;
            _effectiveDateRepository = effectiveDateRepository;
            _budgetCodeRepository = budgetCodeRepository;
            _productPriorityRepository = productPriorityRepository;
            _workcentermastermachineservice = workcentermastermachineservice;
            _workCenterWiseShiftRepository = workCenterWiseShiftRepository;
            _WorkCenterMasterSubProcessRepository = WorkCenterMasterSubProcessRepository;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber("WorkCenterMaster", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch 
            {
                return 1.00M;
            }
        }


        public GridModel EmployeeListByPlant(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @" SELECT EI.SystemId, mb.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
								, EI.EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [DesignationName], MB.EntityId,PR.UserName PositionName
        						, DEG.UserName GivenDesignation,DEPT.UserName Department
                                    FROM dbo.EmployeeInformation AS EI
									LEFT JOIN HKP.Designation AS DEG ON DEG.Id=EI.GivenDesignationID
                                    LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
        					        LEFT OUTER JOIN ORG.Position PR ON MB.PositionId=PR.Id
									LEFT OUTER JOIN ORG.Entity E ON MB.EntityId=E.Id
        					        LEFT OUTER JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                    WHERE EI.PlantId='" + plantId + "' AND EI.EmployeeStatus='Active'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public WorkCenterMaster GetMaster(string PK)
        {
            try
            {
                string _sql = "select * from [SCS].[WorkCenterMaster] where Id='" + PK + "'";
                return _recipeMasterRepository.SelectQuery(_sql).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetList(string masterid, string companyId)
        {
            try
            {
                var sql = @"SELECT m.Id
                                ,m.Sequence
	                            ,m.Code
	                            ,m.UserName
                                ,L.UserName LineName
	                            ,m.StandardName
	                            ,m.[Description]
	                            ,m.Capacity
	                            ,c.UserName WorkCenterCategory
	                            ,sc.UserName WorkCenterSubcategory
	                            ,p.UserName Plant
	                            ,pr.UserName Process
	                            ,m.WorkCenterCategoryId
	                            ,m.WorkCenterSubcategoryId
	                            ,m.PlantId
	                            ,m.ProcessId
                                ,m.CapacityProcessUoMId
                                ,m.UoMId
                                ,m.PlanEfficiency
                                ,m.MaxTimePerDay
                                ,m.StandardTimePerDay
                                ,m.PlanBudgetCapacityPerDay
                                ,m.DailyFixedCost
                                ,m.VariableCost
                                --,m.VariableCostTimeUoMId
                                , m.CurrencyId,m.LineId
								, m.SPT, m.CM, m.NoOfWorkStation, m.MonthlyNoOfDays
								, m.ResponsiblePersonId, RES.EmployeeName AS ResponsiblePersonName
								, m.MentorId, MNT.EmployeeName AS MentorName, m.BuyerId
                                , m.AccountHolder, AH.EmployeeName AS AccountHolderName
                                , m.AccountInCharge, AC.EmployeeName AS AccountInChargeName,M.GroupingData,M.Active
                            FROM [SCS].[WorkCenterMaster] m
                            LEFT JOIN [HKP].[WorkCenterCategory] c ON c.Id = m.WorkCenterCategoryId
                            LEFT JOIN [HKP].[WorkCenterSubCategory] sc ON sc.Id = m.WorkCenterSubCategoryId
                            LEFT JOIN [ORG].Plant p ON p.Id = m.plantId
                            LEFT JOIN HKP.Process pr ON pr.Id = m.processId
                            LEFT JOIN org.Line L ON L.Id = m.LineId
                            LEFT JOIN EmployeeInformation RES ON m.ResponsiblePersonId= RES.SystemId
                            LEFT JOIN EmployeeInformation MNT ON m.MentorId= MNT.SystemId
                            LEFT JOIN EmployeeInformation AH ON m.AccountHolder= AH.SystemId
                            LEFT JOIN EmployeeInformation AC ON m.AccountInCharge= AC.SystemId
                                WHERE m.CompanyId = '" + companyId + "' and m.Id='" + masterid + @"'
                                Order by m.Code";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetListByPlant(string plantid, string entityid, string processid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"
                            SELECT
	                             m.Code
                                ,L.UserName LineName
	                            ,m.UserName
	                            ,m.StandardName
	                            ,m.[Description]
	                            ,m.Capacity
	                            ,c.UserName WorkCenterCategory
	                            ,sc.UserName WorkCenterSubcategory
	                            ,p.UserName Plant,e.UserName Entity
	                            ,pr.UserName Process
	                            ,m.WorkCenterCategoryId
	                            ,m.WorkCenterSubcategoryId
	                            ,m.PlantId
	                            ,m.ProcessId,m.EntityId
                                ,m.Id
                            FROM [SCS].[WorkCenterMaster] m
                            LEFT JOIN [HKP].[WorkCenterCategory] c ON c.Id = m.WorkCenterCategoryId
                            LEFT JOIN [HKP].[WorkCenterSubCategory] sc ON sc.Id = m.WorkCenterSubCategoryId
                            LEFT JOIN [ORG].Plant p ON p.Id = m.plantId
                            LEFT JOIN [ORG].Entity e ON e.Id = m.entityid
                            LEFT JOIN HKP.Process pr ON pr.Id = m.processId
                            LEFT JOIN org.Line L ON L.Id = m.LineId
                                WHERE m.CompanyId = '" + identity.CompanyId + @"'
                                        and m.PlantId='" + plantid + @"'
                                        and m.entityid='" + entityid + @"'
                                        and m.processid='" + processid + @"'
                                Order by m.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetListByPlantAndEntity(string plantid, string entityid, string companyId)
        {
            try
            {
                var sql = @"
                            SELECT
                                  m.Sequence
	                            , m.Code
	                            ,m.UserName
                                ,L.UserName LineName
	                            --,m.StandardName,m.[Description]
	                            ,m.Capacity
	                            ,c.UserName WorkCenterCategory
	                            ,sc.UserName WorkCenterSubcategory
	                            --,p.UserName Plant,e.UserName Entity
	                            ,pr.UserName Process
	                            ,m.WorkCenterCategoryId
	                            ,m.WorkCenterSubcategoryId
	                            ,m.PlantId
	                            ,m.ProcessId,m.EntityId
                                ,m.Id
                            FROM [SCS].[WorkCenterMaster] m
                            LEFT JOIN [HKP].[WorkCenterCategory] c ON c.Id = m.WorkCenterCategoryId
                            LEFT JOIN [HKP].[WorkCenterSubCategory] sc ON sc.Id = m.WorkCenterSubCategoryId
                            LEFT JOIN [ORG].Plant p ON p.Id = m.plantId
                            LEFT JOIN [ORG].Entity e ON e.Id = m.entityid
                            LEFT JOIN HKP.Process pr ON pr.Id = m.processId
                            LEFT JOIN org.Line L ON L.Id = m.LineId
                                WHERE m.CompanyId = '" + companyId + @"'
                                        and m.PlantId='" + plantid + @"'
                                        and m.entityid='" + entityid + @"'
                                Order by m.Sequence,m.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetListByPlantAndUnit(string plantid, string EntityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"
                            SELECT
	                             m.Code
	                            ,m.UserName
                                ,L.UserName LineName
	                            ,m.StandardName
	                            ,m.[Description]
	                            ,m.Capacity
	                            ,c.UserName WorkCenterCategory
	                            ,sc.UserName WorkCenterSubcategory
	                            ,p.UserName Plant
	                            ,pr.UserName Process
	                            ,m.WorkCenterCategoryId
	                            ,m.WorkCenterSubcategoryId
	                            ,m.PlantId
	                            ,m.ProcessId
                                ,m.Id
                            FROM [SCS].[WorkCenterMaster] m
                            LEFT JOIN [HKP].[WorkCenterCategory] c ON c.Id = m.WorkCenterCategoryId
                            LEFT JOIN [HKP].[WorkCenterSubCategory] sc ON sc.Id = m.WorkCenterSubCategoryId
                            LEFT JOIN [ORG].Plant p ON p.Id = m.plantId
                            LEFT JOIN HKP.Process pr ON pr.Id = m.processId
                            LEFT JOIN org.Line L ON L.Id = m.LineId
                                WHERE m.CompanyId = '" + identity.CompanyId + "' and m.PlantId='" + plantid + @"' and m.EntityId='" + EntityId + @"'
                                Order by m.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetListByPlant(GridParameter parameters, string plantid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"
                            SELECT
	                             m.Code
	                            ,m.UserName
                                ,L.UserName LineName
	                            ,m.StandardName
	                            ,m.[Description]
	                            ,m.Capacity
	                            ,c.UserName WorkCenterCategory
	                            ,sc.UserName WorkCenterSubcategory
	                            ,p.UserName Plant
	                            ,pr.UserName Process
	                            ,m.WorkCenterCategoryId
	                            ,m.WorkCenterSubcategoryId
	                            ,m.PlantId
	                            ,m.ProcessId
                                ,m.Id,m.Id WorkCenterMasterId
                            FROM [SCS].[WorkCenterMaster] m
                            LEFT JOIN [HKP].[WorkCenterCategory] c ON c.Id = m.WorkCenterCategoryId
                            LEFT JOIN [HKP].[WorkCenterSubCategory] sc ON sc.Id = m.WorkCenterSubCategoryId
                            LEFT JOIN [ORG].Plant p ON p.Id = m.plantId
                            LEFT JOIN HKP.Process pr ON pr.Id = m.processId
                            LEFT JOIN org.Line L ON L.Id = m.LineId
                                WHERE m.CompanyId = '" + identity.CompanyId + @"'  and m.PlantId='" + plantid + @"' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetWCByPlant(GridParameter parameters, string plantid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"
                            SELECT
	                              L.Code LineName
	                            ,m.UserName Workcenter
	                            ,m.Capacity,u.UserName CapacityUom,e.UserName Entity
	                            ,pr.UserName Process
	                            ,m.WorkCenterCategoryId
	                            ,m.WorkCenterSubcategoryId
	                            ,m.PlantId
	                            ,m.ProcessId
                                ,m.Id,m.Id WorkCenterMasterId
                            FROM [SCS].[WorkCenterMaster] m
                            LEFT JOIN [HKP].[WorkCenterCategory] c ON c.Id = m.WorkCenterCategoryId
                            LEFT JOIN [HKP].[WorkCenterSubCategory] sc ON sc.Id = m.WorkCenterSubCategoryId
                            LEFT JOIN [ORG].Plant p ON p.Id = m.plantId
                            LEFT JOIN HKP.Process pr ON pr.Id = m.processId
                            LEFT JOIN org.Line L ON L.Id = m.LineId
                            LEFT JOIN scs.UnitOfMeasurement u ON u.Id = m.CapacityProcessUoMId
                            LEFT JOIN org.Entity e ON e.Id = m.EntityId
                                WHERE m.CompanyId = '" + identity.CompanyId + @"'  and m.PlantId='" + plantid + @"' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetListByPlantEntity(GridParameter parameters, string defaultprocessid, string entityids)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"
                            SELECT
	                             L.Code LineName
	                            ,m.UserName Workcenter
	                            ,m.Capacity,u.UserName CapacityUom,e.UserName Entity
	                            ,pr.UserName Process
	                            ,m.WorkCenterCategoryId
	                            ,m.WorkCenterSubcategoryId
	                            ,m.PlantId
	                            ,m.ProcessId
                                ,m.Id,m.Id WorkCenterMasterId
                            FROM [SCS].[WorkCenterMaster] m
                            LEFT JOIN [HKP].[WorkCenterCategory] c ON c.Id = m.WorkCenterCategoryId
                            LEFT JOIN [HKP].[WorkCenterSubCategory] sc ON sc.Id = m.WorkCenterSubCategoryId
                            LEFT JOIN [ORG].Plant p ON p.Id = m.plantId
                            LEFT JOIN [ORG].Entity e ON e.Id = m.entityid
                            LEFT JOIN HKP.Process pr ON pr.Id = m.processId
                            LEFT JOIN org.Line L ON L.Id = m.LineId
							LEFT JOIN scs.UnitOfMeasurement u ON u.Id = m.CapacityProcessUoMId
                                WHERE m.CompanyId = '" + identity.CompanyId + @"'
                                        and m.ProcessId='" + defaultprocessid + @"'
                                        and m.entityid in (" + entityids + @")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetListByPlant(GridParameter parameters, string plantid, string processid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"
                            SELECT
	                             m.Code
	                            ,m.UserName
	                            ,m.StandardName
	                            ,m.[Description]
	                            ,m.Capacity
	                            ,c.UserName WorkCenterCategory
	                            ,sc.UserName WorkCenterSubcategory
	                            ,p.UserName Plant
	                            ,pr.UserName Process
	                            ,m.WorkCenterCategoryId
	                            ,m.WorkCenterSubcategoryId
	                            ,m.PlantId
	                            ,m.ProcessId
                                ,m.Id,m.Id WorkCenterMasterId
                            FROM [SCS].[WorkCenterMaster] m
                            LEFT JOIN [HKP].[WorkCenterCategory] c ON c.Id = m.WorkCenterCategoryId
                            LEFT JOIN [HKP].[WorkCenterSubCategory] sc ON sc.Id = m.WorkCenterSubCategoryId
                            LEFT JOIN [ORG].Plant p ON p.Id = m.plantId
                            LEFT JOIN HKP.Process pr ON pr.Id = m.processId
                                WHERE m.CompanyId = '" + identity.CompanyId + @"'  and m.PlantId='" + plantid + @"'  and m.ProcessId='" + processid + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetdBatchByWorkcenter(string WorkCenterMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = "select ProductionBatchMasterId from [TRN].[ProductionBatchWorkCenter] where WorkCenterMasterId ='" + WorkCenterMasterId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetSearchData(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"
                                SELECT m.Id
                                    ,s.UserName FixedAsset
                                    ,c.UserName FixedAssetCategory
	                                ,sc.UserName FixedAssetSubcategory
                                    ,p.UserName Vendor
                                    ,m.SerialNo
                                    ,Replace(CONVERT(VARCHAR(11), m.InvoiceDate, 106), ' ', '-') InvoiceDate
                                    ,m.Brand
	                                ,m.InvoiceNo
	                                ,mt.[Description] MachineType
	                                ,m.Model
	                                ,m.YearOfManufacture
	                                ,m.YearOfInstallation
	                                ,cn.UserName Country
	                                ,m.IsForProduction

                                FROM [SCS].[WorkCenterMaster] m
                                LEFT outer JOIN [HKP].[FixedAsset]  s ON s.Id = m.FixedAssetId
                                LEFT outer JOIN [HKP].[FixedAssetCategory] c ON c.Id = m.FixedAssetCategoryId
                                LEFT outer JOIN [HKP].[FixedAssetSubCategory] sc ON sc.Id = m.FixedAssetSubCategoryId
                                LEFT outer JOIN [SCS].[Country] cn ON cn.Id = m.CountryOfOriginId
                                LEFT outer JOIN [MST].[FixedAssetMasterMachineType] fm ON fm.FixedAssetItemId = m.Id
                                LEFT outer JOIN [MST].[MaterialMasterMachineProcess] mt ON mt.Id = fm.MachineTypeId
                                left outer join [HKP].[Party] p on p.Id=m.VendorId

                                WHERE m.CompanyId = '" + identity.CompanyId + @"'  and m.Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetAllWorkCenter(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"
                            SELECT
	                             m.Code
	                            ,m.UserName
	                            ,m.StandardName
	                            ,m.[Description]
	                            ,m.Capacity
	                            ,c.UserName WorkCenterCategory
	                            ,sc.UserName WorkCenterSubcategory
	                            ,pr.UserName Process
	                            ,m.WorkCenterCategoryId
	                            ,m.WorkCenterSubcategoryId
	                            ,m.ProcessId
                                ,m.Id,m.Id WorkCenterMasterId
                            FROM  " + DbSchema.SystemConfigurationAndSetup + @".[WorkCenterMaster] m
                            LEFT JOIN " + DbSchema.HKP + @".[WorkCenterCategory] c ON c.Id = m.WorkCenterCategoryId
                            LEFT JOIN " + DbSchema.HKP + @".[WorkCenterSubCategory] sc ON sc.Id = m.WorkCenterSubCategoryId
                            LEFT JOIN " + DbSchema.HKP + @".[Process] pr ON pr.Id = m.processId";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetSearchLine(GridParameter parameters, string entityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT EL.EntityId,L.Id,L.UserName,L.Description FROM [ORG].[EntityLine] EL
                            LEFT OUTER JOIN ORG.Line L ON EL.LineId=L.Id
                            WHERE EL.EntityId='" + entityId + "' AND L.Active=1";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetEmployeeList(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.SystemId
                                , EI.EmployeeCode
                                , EI.EmployeeName
                                , EI.FirstName
                                , EI.MiddleName
                                , EI.LastName
                                , mb.PositionId AS PositionCode
                                , EI.BudgetCode
                                , EI.EmailId
                                , EI.CellPhnNo
                                , EI.EmpPicPath AS [Image]
                                , REPLACE(CONVERT(CHAR(11), EI.DOB, 106),' ','-') AS DateOfBirth
                        FROM dbo.EmployeeInformation AS EI
LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                        WHERE EI.PlantId='" + plantId + "' AND EI.EmployeeStatus='Active'  and ei.EmpType<>'Guest'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetMaterialMasterList(GridParameter parameters, string groupId, string[] ids)
        {
            try
            {
                parameters.CmdText = @"SELECT NULL AS Id, MGP.UserName AS MaterialGroupMaster
                                        , MC.UserName MaterialCategory
	                                    , MSC.UserName MaterialSubCategory
                                        , MM.Id AS MaterialMasterId
                                        , MM.Sequence,MM.Code,MM.ShortName,MM.StandardName,MM.UserName, 0 AS [Priority]
                                    FROM [TRN].[ProductDefinition] AS PRD
                                    LEFT JOIN [MST].[MaterialMaster] AS MM ON PRD.MaterialMasterId = MM.Id
                                    LEFT JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                                    LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                                    LEFT JOIN [HKP].[MaterialCategory] AS MC ON MM.MaterialCategoryId = MC.Id
                                    LEFT JOIN [HKP].[MaterialSubCategory] AS MSC ON MM.MaterialSubCategoryId = MSC.Id
                                    WHERE MM.CompanyGroupId = '" + groupId + @"' AND MM.Archive = 0 AND MM.Active = 1
                                    AND MM.Id NOT IN(" + ReturnStringArray(ids) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                  Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                  ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel GetShiftList(GridParameter parameters, string sGroupID, string sPlantID, string[] ShiftDefinationIDs)
        {
            try
            {
                parameters.sort = "ShiftDefinationName";
                parameters.CmdText = @"SELECT 0 Flag,SystemID ShiftDefinationID, ShiftDefinationName, ShiftDefinationDescription, ShiftType, SequenceNo ShiftSequence, CONVERT(VARCHAR(10), InTime, 108) AS InTime,
                                        InTimeStartMargin, LateMargin, AbsentEndMargin, CONVERT(VARCHAR(10), OutTime, 108) AS OutTime,
                                        OutTimeEndMargin, OTStartTime, CONVERT(VARCHAR(10), BreakStratTime, 108) AS BreakStratTime,
                                        CONVERT(VARCHAR(10), BreakEndTime, 108) AS BreakEndTime, BreakPeriod, WorkingHour, IsActive, DefaultShift, IsGapInclude
                                FROM ShiftDefination WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' AND SystemID NOT IN (" + ReturnStringArray(ShiftDefinationIDs) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public GridModel GetListForSubProcess(GridParameter parameters, string CompanyGroupId, string processId, string WorkCenterMasterId, string[] subProcessIds)
        {
            try
            {
             
                var subProcess = "";
                if (subProcessIds.Length > 0)
                    subProcess = string.Join(",", subProcessIds.Select(item => "'" + item + "'"));
                else
                    subProcess = "' '";
                parameters.order = "asc";
                parameters.sort = "Sequence";
                parameters.CmdText = "SELECT SP.Id, " +
                                               "SP.Code, " +
                                               "SP.UserName, " +
                                               "SP.Sequence," +
                                               "SP.Active, " +
                                               "SP.Archive, " +
                                               "SPC.UserName AS SubProcessCategoryName," +
                                               "'' AS Flag " +
                                        $"FROM {DbSchema.HKP}.[{DbTable.SubProcess}] AS SP  " +
                                        $"LEFT OUTER JOIN HKP.[SubProcessCategory] AS SPC ON SP.SubProcessCategoryId=SPC.Id " +
                                        $"WHERE SP.CompanyGroupId='{CompanyGroupId}' AND SP.Archive=0 AND SP.ProcessId='{processId}' " +
                                        $"AND SP.Id NOT IN (SELECT SubProcessId FROM [SCS].[WorkCenterMasterSubProcess] Where WorkCenterMasterId='"+ WorkCenterMasterId + "')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public IEnumerable<object> GetWorkCenterWiseShiftList(string sGroupID, string sPlantID, string workCenterMasterId)
        {
            try
            {
               
                string CmdText = @"SELECT WS.Id,WS.WorkCenterMasterId,SD.SystemID ShiftDefinationID, SD.ShiftDefinationName, SD.ShiftDefinationDescription, SD.ShiftType, SD.SequenceNo ShiftSequence
                                  ,CONVERT(VARCHAR(10), SD.InTime, 108) AS InTime, CONVERT(VARCHAR(10), SD.OutTime, 108) AS OutTime,WS.ProductionHours
                                  FROM [dbo].[WorkCenterWiseShift] WS 
                                  LEFT JOIN ShiftDefination SD ON WS.ShiftDefinationID=SD.SystemID
                                  WHERE SD.GroupID = '" + sGroupID + @"' AND SD.PlantID = '"+ sPlantID + @"' AND WS.WorkCenterMasterId='"+ workCenterMasterId + @"' Order By SD.ShiftDefinationName";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetWorkCenterMasterSubProcessList(string workCenterMasterId)
        {
            try
            {

                string CmdText = @"SELECT WSP.*, SP.Code, SP.UserName SubProcessName, SP.Sequence,SP.Active, SP.Archive, SPC.UserName AS SubProcessCategoryName
FROM [SCS].[WorkCenterMasterSubProcess] WSP
LEFT JOIN HKP.[SubProcess] AS SP ON SP.Id = WSP.SubProcessId  
LEFT OUTER JOIN HKP.[SubProcessCategory] AS SPC ON SP.SubProcessCategoryId=SPC.Id
WHERE WSP.WorkCenterMasterId='" + workCenterMasterId + "' ORDER BY SP.Sequence";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        #region Insert n Update

        private void OutMaster(WorkCenterMaster from_ui, out WorkCenterMaster from_db)
        {
            IEnumerable<object> fromdbList = null;
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                fromdbList = GetListByPlant(from_ui.PlantId, from_ui.EntityId, from_ui.ProcessId);
                CheckDuplicateMaster(from_ui, fromdbList);

                from_db = GetMaster(from_ui.Id);

                if (from_db == null)
                {
                    from_db = new WorkCenterMaster
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(from_db);

                    #region Add

                    from_db.Id = GetPK();//set pk

                    from_db.Sequence = from_ui.Sequence;
                    from_db.Capacity = from_ui.Capacity;
                    from_db.Code = from_ui.Code;
                    //from_db.CompanyId = identity.CompanyId;
                    from_db.CompanyId = from_ui.CompanyId;
                    from_db.Description = from_ui.Description;
                    from_db.PlantId = from_ui.PlantId;
                    from_db.EntityId = from_ui.EntityId;
                    from_db.ProcessId = from_ui.ProcessId;
                    from_db.StandardName = from_ui.StandardName;
                    from_db.UserName = from_ui.UserName;
                    from_db.WorkCenterCategoryId = from_ui.WorkCenterCategoryId;
                    from_db.WorkCenterSubcategoryId = from_ui.WorkCenterSubcategoryId;

                    from_db.CapacityProcessUoMId = from_ui.CapacityProcessUoMId;
                    from_db.UoMId = from_ui.UoMId;
                    from_db.PlanEfficiency = from_ui.PlanEfficiency;
                    from_db.MaxTimePerDay = from_ui.MaxTimePerDay;
                    from_db.StandardTimePerDay = from_ui.StandardTimePerDay;
                    from_db.PlanBudgetCapacityPerDay = from_ui.PlanBudgetCapacityPerDay;
                    from_db.DailyFixedCost = from_ui.DailyFixedCost;
                    from_db.VariableCost = from_ui.VariableCost;
                    from_db.CurrencyId = from_ui.CurrencyId;
                    from_db.LineId = from_ui.LineId;
                    from_db.SPT = from_ui.SPT;
                    from_db.CM = from_ui.CM;
                    from_db.NoOfWorkStation = from_ui.NoOfWorkStation;
                    from_db.MonthlyNoOfDays = from_ui.MonthlyNoOfDays;
                    from_db.ResponsiblePersonId = from_ui.ResponsiblePersonId;
                    from_db.MentorId = from_ui.MentorId;
                    from_db.BuyerId = from_ui.BuyerId;
                    from_db.AccountHolder = from_ui.AccountHolder;
                    from_db.AccountInCharge = from_ui.AccountInCharge;
                    from_db.GroupingData = from_ui.GroupingData;
                    from_db.Active = from_ui.Active;

                    #endregion Add
                }
                else
                {
                    #region Edit

                    from_db.ModelState = ModelState.Modified;
                    AuditService.Log(from_db);

                    from_db.Sequence = from_ui.Sequence;
                    from_db.Capacity = from_ui.Capacity;
                    from_db.Code = from_ui.Code;
                    from_db.CompanyId = from_ui.CompanyId;
                    from_db.Description = from_ui.Description;
                    from_db.PlantId = from_ui.PlantId;
                    from_db.EntityId = from_ui.EntityId;
                    from_db.ProcessId = from_ui.ProcessId;
                    from_db.StandardName = from_ui.StandardName;
                    from_db.UserName = from_ui.UserName;
                    from_db.WorkCenterCategoryId = from_ui.WorkCenterCategoryId;
                    from_db.WorkCenterSubcategoryId = from_ui.WorkCenterSubcategoryId;

                    from_db.CapacityProcessUoMId = from_ui.CapacityProcessUoMId;
                    from_db.UoMId = from_ui.UoMId;
                    from_db.PlanEfficiency = from_ui.PlanEfficiency;
                    from_db.MaxTimePerDay = from_ui.MaxTimePerDay;
                    from_db.StandardTimePerDay = from_ui.StandardTimePerDay;
                    from_db.PlanBudgetCapacityPerDay = from_ui.PlanBudgetCapacityPerDay;
                    from_db.DailyFixedCost = from_ui.DailyFixedCost;
                    from_db.VariableCost = from_ui.VariableCost;
                    from_db.CurrencyId = from_ui.CurrencyId;
                    from_db.LineId = from_ui.LineId;
                    from_db.SPT = from_ui.SPT;
                    from_db.CM = from_ui.CM;
                    from_db.NoOfWorkStation = from_ui.NoOfWorkStation;
                    from_db.MonthlyNoOfDays = from_ui.MonthlyNoOfDays;
                    from_db.ResponsiblePersonId = from_ui.ResponsiblePersonId;
                    from_db.MentorId = from_ui.MentorId;
                    from_db.BuyerId = from_ui.BuyerId;
                    from_db.AccountHolder = from_ui.AccountHolder;
                    from_db.AccountInCharge = from_ui.AccountInCharge;
                    from_db.GroupingData = from_ui.GroupingData;
                    from_db.Active = from_ui.Active;
                    #endregion Edit
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void InsertORUpdateMaster(WorkCenterMaster master, out string masterid)
        {
            var flag = false;
            try
            {
                masterid = string.Empty;
                OutMaster(master, out WorkCenterMaster localMaster);
                _recipeMasterRepository.InsertOrUpdateGraph(localMaster);
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                masterid = localMaster.Id;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void CheckDuplicateMaster(WorkCenterMaster detail_ui, IEnumerable<object> from_db_List)
        {
            try
            {
                foreach (var item in from_db_List)
                {
                    var dic = (Dictionary<string, object>)item;
                    if (dic["Id"].ToString() != detail_ui.Id)
                    {
                        if (dic["Code"].ToString() == detail_ui.Code)
                        {
                            throw new Exception("Code: [" + dic["Code"] + "] exists in Work Center: [" + dic["UserName"] + "]...");
                        }
                    }//id
                }//foreach
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #endregion Insert n Update

        #region Delete

        private void CheckBatchTagging(string id)
        {
            try
            {
                IEnumerable<object> list = GetdBatchByWorkcenter(id);
                if (list.Count() > 0)
                {
                    var batches = "";
                    foreach (var item in list)
                    {
                        var dic = (Dictionary<string, object>)item;
                        if (batches.Length == 0)
                        {
                            batches = dic["ProductionBatchMasterId"].ToString();
                        }
                        else
                        {
                            batches += ", " + dic["ProductionBatchMasterId"];
                        }
                    }
                    throw new Exception("This WorkCenter has already been tagged with Batch" + (list.Count() > 1 ? "s" : "") + ": [" + batches + "]");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteMaster(string masterId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var masterData = base.Find(masterId);

                DeleteEffectiveDate(masterId);
                var dbBudgetCodeList = _budgetCodeRepository.Query(t => t.WorkCenterMasterId == masterId).Select().ToList();
                if (dbBudgetCodeList != null)
                {
                    foreach (var item in dbBudgetCodeList)
                    {
                        item.ModelState = ModelState.Deleted;
                        _budgetCodeRepository.Delete(item);
                    }
                }
                DeleteProductPriority(masterId);
                DeleteWorkCenterShift(masterId);
                base.DeleteGraph(masterData);

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
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #endregion Delete

        #region Detail Insert or update

        private string EffectiveDatePK => _pkGeneratorService.GetAutoNumber("WorkCenterMasterEffectiveDate", PKGeneratorEnum.Auto, null, DateTime.Now);
        private string ManpowerBudgePK => _pkGeneratorService.GetAutoNumber("WorkCenterMasterManpowerBudge", PKGeneratorEnum.Auto, null, DateTime.Now);
        private string ProductPriorityPK => _pkGeneratorService.GetAutoNumber("WorkCenterMasterProductPriority", PKGeneratorEnum.Auto, null, DateTime.Now);
        private string shiftPK => _pkGeneratorService.GetAutoNumber("WorkCenterWiseShift", PKGeneratorEnum.Auto, null, DateTime.Now);


        private string GetProductPriorityPK()
        {
            return _pkGeneratorService.GetAutoNumber("WorkCenterMasterProductPriority", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InsertUpdateOrDeleteDetails(string masterId, IEnumerable<WorkCenterMasterEffectiveDate> effectiveDateList
            , IEnumerable<WorkCenterMasterManpowerBudge> budgetCodeList, IEnumerable<WorkCenterMasterProductPriority> productPriorityList
            , IEnumerable<WorkCenterWiseShift> shiftList, IEnumerable<WorkCenterMasterSubProcess> subProcessList)
        {
            var flag = false;
            try
            {
                var masterData = Find(masterId);
                if (masterData == null) throw new CustomException("Data not found.");
                _unitOfWork.BeginTransaction();
                flag = true;
                var dbEffectiveList = _effectiveDateRepository.Query(t => t.WorkCenterMasterId == masterId).Select().ToList();
                var dbBudgetCodeList = _budgetCodeRepository.Query(t => t.WorkCenterMasterId == masterId).Select().ToList();
                var dbProductPriorityList = _productPriorityRepository.Query(t => t.WorkCenterMasterId == masterId).Select().ToList();
                var dbShiftList = _workCenterWiseShiftRepository.Query(t => t.WorkCenterMasterId == masterId).Select().ToList();
                var dbsubProcessList = _WorkCenterMasterSubProcessRepository.Query(t => t.WorkCenterMasterId == masterId).Select().ToList();

                InsertUpdateOrDeleteEffectiveDate(masterId, effectiveDateList, dbEffectiveList);
                InsertUpdateOrDeleteManpowerBudgetCode(masterId, budgetCodeList, dbBudgetCodeList);
                InsertUpdateOrDeleteProduct(masterId, productPriorityList, dbProductPriorityList);
                InsertUpdateOrDeleteShift(masterId, shiftList, dbShiftList);
                InsertUpdateOrDeleteWCSubProcess(masterId, subProcessList, dbsubProcessList);

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
                throw new CustomException(ex.Message, ex,
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void InsertUpdateOrDeleteEffectiveDate(string masterId, IEnumerable<WorkCenterMasterEffectiveDate> effectiveDateList, List<WorkCenterMasterEffectiveDate> dbEffectiveList)
        {
            if (effectiveDateList != null)
            {
                for (int t = 0; t < effectiveDateList.Count(); t++)
                {
                    var row = effectiveDateList.ElementAt(t);
                    if (t != 0) //compare between previous end date and current start date //compare between previous end date and current start date
                    {
                        var previousRow = effectiveDateList.ElementAt(t - 1);
                        if (row.StartDate < previousRow.EndDate) throw new CustomException("Start date " + row.StartDate + " must be greater than end date " + previousRow.EndDate);
                    }
                    if (t != effectiveDateList.Count() - 1) //end date can be null when new entry
                        if (row.StartDate > row.EndDate) throw new CustomException("Start date " + row.StartDate + "must be less than end date " + row.EndDate);
                }
                if (effectiveDateList.GroupBy(t => t).Any(t => t.Count() > 1)) throw new CustomException("effective date can not be duplicate.");
                foreach (var item in effectiveDateList)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = EffectiveDatePK;
                        item.WorkCenterMasterId = masterId;
                        AuditService.AddedLog(item);
                        _effectiveDateRepository.Insert(item);
                    }
                    else
                    {
                        if (!dbEffectiveList.Any(t => t.Id == item.Id)) throw new CustomException("Data not found.");
                        AuditService.UpdatedLog(item);
                        _effectiveDateRepository.Update(item);
                    }
                }
            }
            if (dbEffectiveList != null)
            {
                if (effectiveDateList == null)
                {
                    foreach (var item in dbEffectiveList)
                    {
                        _effectiveDateRepository.Delete(item);
                    }
                }
                else
                {
                    foreach (var item in dbEffectiveList)
                    {
                        if (!effectiveDateList.Any(t => t.Id == item.Id))
                            _effectiveDateRepository.Delete(item);
                    }
                }
            }
        }

        private void InsertUpdateOrDeleteProduct(string masterId, IEnumerable<WorkCenterMasterProductPriority> productPriorityList, List<WorkCenterMasterProductPriority> dbProductPriorityList)
        {
            if (productPriorityList != null)
            {
                if (productPriorityList.GroupBy(t => t).Any(t => t.Count() > 1)) throw new CustomException("Product can not be duplicate.");
                foreach (var item in productPriorityList)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = ProductPriorityPK;
                        item.WorkCenterMasterId = masterId;
                        AuditService.AddedLog(item);
                        _productPriorityRepository.Insert(item);
                    }
                    else
                    {
                        if (!dbProductPriorityList.Any(t => t.Id == item.Id)) throw new CustomException("Data not found.");
                        AuditService.UpdatedLog(item);
                        _productPriorityRepository.Update(item);
                    }
                }
            }
            if (dbProductPriorityList != null)
            {
                if (productPriorityList == null)
                {
                    foreach (var item in dbProductPriorityList)
                    {
                        _productPriorityRepository.Delete(item);
                    }
                }
                else
                {
                    foreach (var item in dbProductPriorityList)
                    {
                        if (!productPriorityList.Any(t => t.Id == item.Id))
                            _productPriorityRepository.Delete(item);
                    }
                }
            }
        }

        private void InsertUpdateOrDeleteManpowerBudgetCode(string masterId, IEnumerable<WorkCenterMasterManpowerBudge> budgetCodeList, List<WorkCenterMasterManpowerBudge> dbBudgetCodeList)
        {
            if (budgetCodeList != null)
            {
                if (budgetCodeList.GroupBy(t => t).Any(t => t.Count() > 1)) throw new CustomException("Man power Budget can not duplicate.");
                foreach (var item in budgetCodeList)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = ManpowerBudgePK;
                        item.WorkCenterMasterId = masterId;
                        AuditService.AddedLog(item);
                        _budgetCodeRepository.Insert(item);
                    }
                    else
                    {
                        if (!dbBudgetCodeList.Any(t => t.Id == item.Id)) throw new CustomException("Data not found.");
                        AuditService.UpdatedLog(item);
                        _budgetCodeRepository.Update(item);
                    }
                }
            }
            if (dbBudgetCodeList != null)
            {
                if (budgetCodeList == null)
                {
                    foreach (var item in dbBudgetCodeList)
                    {
                        _budgetCodeRepository.Delete(item);
                    }
                }
                else
                {
                    foreach (var item in dbBudgetCodeList)
                    {
                        if (!budgetCodeList.Any(t => t.Id == item.Id))
                            _budgetCodeRepository.Delete(item);
                    }
                }
            }
        }

        private void InsertUpdateOrDeleteShift(string masterId, IEnumerable<WorkCenterWiseShift> shiftList, List<WorkCenterWiseShift> dbshiftList)
        {
            if (shiftList != null)
            {
                if (shiftList.GroupBy(t => t).Any(t => t.Count() > 1)) throw new CustomException("Shift can not be duplicate.");
                foreach (var item in shiftList)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = shiftPK;
                        item.WorkCenterMasterId = masterId;
                        AuditService.AddedLog(item);
                        _workCenterWiseShiftRepository.Insert(item);
                    }
                    else
                    {
                        if (!dbshiftList.Any(t => t.Id == item.Id)) throw new CustomException("Data not found.");
                        AuditService.UpdatedLog(item);
                        _workCenterWiseShiftRepository.Update(item);
                    }
                }
            }
            if (dbshiftList != null)
            {
                if (shiftList == null)
                {
                    foreach (var item in dbshiftList)
                    {
                        _workCenterWiseShiftRepository.Delete(item);
                    }
                }
                else
                {
                    foreach (var item in dbshiftList)
                    {
                        if (!shiftList.Any(t => t.Id == item.Id))
                            _workCenterWiseShiftRepository.Delete(item);
                    }
                }
            }
        }

        private void InsertUpdateOrDeleteWCSubProcess(string masterId, IEnumerable<WorkCenterMasterSubProcess> subProcessList, List<WorkCenterMasterSubProcess> dbsubProcessList)
        {
            if (subProcessList != null)
            {
                if (subProcessList.GroupBy(t => t).Any(t => t.Count() > 1)) throw new CustomException("SubProcess can not be duplicate.");
                foreach (var item in subProcessList)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = shiftPK;
                        item.WorkCenterMasterId = masterId;
                        AuditService.AddedLog(item);
                        _WorkCenterMasterSubProcessRepository.Insert(item);
                    }
                    else
                    {
                        if (!dbsubProcessList.Any(t => t.Id == item.Id)) throw new CustomException("Data not found.");
                        AuditService.UpdatedLog(item);
                        _WorkCenterMasterSubProcessRepository.Update(item);
                    }
                }
            }
            if (dbsubProcessList != null)
            {
                if (subProcessList == null)
                {
                    foreach (var item in dbsubProcessList)
                    {
                        _WorkCenterMasterSubProcessRepository.Delete(item);
                    }
                }
                else
                {
                    foreach (var item in dbsubProcessList)
                    {
                        if (!subProcessList.Any(t => t.Id == item.Id))
                            _WorkCenterMasterSubProcessRepository.Delete(item);
                    }
                }
            }
        }

        public IEnumerable<object> GetEffectiveDateList(string masterId)
        {
            try
            {
                var sql = @"SELECT A.Id, A.WorkCenterMasterId, StartDate=REPLACE(CONVERT(CHAR(11), A.StartDate, 106),' ','-')
                            , EndDate=REPLACE(CONVERT(CHAR(11), A.EndDate, 106),' ','-'),A.Hour FROM SCS.WorkCenterMasterEffectiveDate AS A
                            WHERE A.WorkCenterMasterId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetManpowerBudgetList(string masterId)
        {
            try
            {
                var sql = @"SELECT A.Id, A.WorkCenterMasterId, B.Code AS ManpowerBudgetCode, A.ManpowerBudgetId, A.NoOfResource
                            , EN.Code EntityCode, EN.UserName EntityName,PS.Code PositionCode, PS.UserName Position
                            FROM SCS.WorkCenterMasterManpowerBudge AS A
                            LEFT JOIN MST.ManpowerBudget AS B ON A.ManpowerBudgetId=B.Id
                            LEFT JOIN ORG.Entity EN ON EN.Id=B.EntityId
							LEFT JOIN ORG.Position PS on PS.Id=B.PositionId
                            WHERE A.WorkCenterMasterId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetProductPriorityList(string masterId)
        {
            try
            {
                //var sql = @"SELECT A.Id, A.WorkCenterMasterId, A.MaterialMasterId, A.[Priority]
                //             , MGP.UserName AS MaterialGroupMaster, MC.UserName MaterialCategory
                //             , MSC.UserName MaterialSubCategory, MM.[Sequence],MM.Code,MM.ShortName,MM.StandardName,MM.UserName
                //            FROM SCS.WorkCenterMasterProductPriority AS A
                //            LEFT JOIN [MST].[MaterialMaster] AS MM ON A.MaterialMasterId = MM.Id
                //            LEFT JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                //            LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                //            LEFT JOIN [HKP].[MaterialCategory] AS MC ON MM.MaterialCategoryId = MC.Id
                //            LEFT JOIN [HKP].[MaterialSubCategory] AS MSC ON MM.MaterialSubCategoryId = MSC.Id
                //            WHERE A.WorkCenterMasterId='" + masterId + "'";
                var sql = @"SELECT A.Id, A.WorkCenterMasterId, A.ProductMasterId, A.[Priority],
	                        PM.UserName, PM.StandardName,PC.UserName ProductCategory, PSC.UserName ProductSubCategory , P.UserName Process
                            FROM SCS.WorkCenterMasterProductPriority AS A
							LEFT JOIN MST.ProductMaster PM ON PM.Id=A.ProductMasterId
                            LEFT JOIN HKP.ProductCategory PC ON PC.Id=PM.ProductCategoryId
                            LEFT JOIN HKP.ProductSubCategory PSC ON PSC.Id=PM.ProductSubCategoryId
                            LEFT JOIN HKP.Process P ON  P.Id= PM.BaseProcessId
                            WHERE A.WorkCenterMasterId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetProductMasterList(GridParameter parameters, string groupId)
        {
            try
            {
                parameters.CmdText = @"SELECT NULL AS Id, PM.Id ProductMasterId, PM.UserName, PM.StandardName,PC.UserName ProductCategory, PSC.UserName ProductSubCategory , P.UserName Process , 0 AS [Priority]
                                     FROM MST.ProductMaster PM
                                     LEFT JOIN HKP.ProductCategory PC ON PC.Id=PM.ProductCategoryId
                                     LEFT JOIN HKP.ProductSubCategory PSC ON PSC.Id=PM.ProductSubCategoryId
                                     LEFT JOIN HKP.Process P ON  P.Id= PM.BaseProcessId
                                     WHERE PM.CompanyGroupId='" + groupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void DeleteEffectiveDate(string masterId)
        {
            var dbEffectiveList = _effectiveDateRepository.Query(t => t.WorkCenterMasterId == masterId).Select().ToList();
            if (dbEffectiveList != null)
            {
                foreach (var item in dbEffectiveList)
                {
                    _effectiveDateRepository.Delete(item);
                }
            }
        }

        private void DeleteManpowerBudgetCode(string masterId)
        {
            var dbBudgetCodeList = _budgetCodeRepository.Query(t => t.WorkCenterMasterId == masterId).Select().ToList();
            if (dbBudgetCodeList != null)
            {
                foreach (var item in dbBudgetCodeList)
                {
                    item.ModelState = ModelState.Deleted;
                    _budgetCodeRepository.Delete(item);
                }
            }
        }

        private void DeleteProductPriority(string masterId)
        {
            var dbProductPriorityList = _productPriorityRepository.Query(t => t.WorkCenterMasterId == masterId).Select().ToList();
            if (dbProductPriorityList != null)
            {
                foreach (var item in dbProductPriorityList)
                {
                    _productPriorityRepository.Delete(item);
                }
            }
        }

        private void DeleteWorkCenterShift(string masterId)
        {
            var dbShiftList = _workCenterWiseShiftRepository.Query(t => t.WorkCenterMasterId == masterId).Select().ToList();
            if (dbShiftList != null)
            {
                foreach (var item in dbShiftList)
                {
                    _workCenterWiseShiftRepository.Delete(item);
                }
            }
        }

        #endregion Detail Insert or update

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetCboList(string entityId)
        {
            try
            {
                var sql = @"Select WCM.Id AS [Value], WCM.UserName AS [Text] From SCS.WorkCenterMaster AS WCM  Where WCM.EntityId='" + entityId + "'";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }
    }
}