#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Employees
{
    public class LeaveOpeningBalanceService : Service<LeaveOpeningBalance>, ILeaveOpeningBalanceService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<LeaveOpeningBalance> _LeaveOpeningBalanceRepository;
        private readonly IRepositoryAsync<EmployeeLeaveSummary> _EmployeeLeaveSummaryRepository;

        public LeaveOpeningBalanceService(
            IRepositoryAsync<LeaveOpeningBalance> LeaveOpeningBalanceRepository,
            IRepositoryAsync<EmployeeLeaveSummary> EmployeeLeaveSummaryRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork,
            ISqlRepository sqlRepository) : base(LeaveOpeningBalanceRepository, unitOfWork, pkGeneratorService)
        {
            _LeaveOpeningBalanceRepository = LeaveOpeningBalanceRepository;
            _EmployeeLeaveSummaryRepository = EmployeeLeaveSummaryRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertUpdate(IEnumerable<LeaveOpeningBalance> entities, string plantId)
        {
            var flag = false;
            try
            {
                if (entities != null)
                {
                    var pk = GetMaxNumber();
                    foreach (var item in entities)
                    {
                        if (!string.IsNullOrEmpty(item.Id))
                        {
                            var empS = GetProductionSummaryList(item.Id).FirstOrDefault();
                            //var empS = _EmployeeLeaveSummaryRepository.Query(r => r.Id == item.Id).Select().FirstOrDefault();
                            empS.CurrentYearAvailedOpeningBalance = item.CurrentYearAvailedOpeningBalance;
                            empS.CurrentYearEarnedDaysOpeningBalance = item.CurrentYearEarnedDaysOpeningBalance;
                            empS.CarryForwardOpeningBalance = item.CarryForwardOpeningBalance;
                            empS.ModelState = ModelState.Modified;
                            AuditService.Log(empS);
                            _EmployeeLeaveSummaryRepository.InsertOrUpdateGraph(empS);
                        }
                        else
                        {
                            throw new CustomException("Id Not Found!");
                        }
                    }
                }
                else
                {
                    throw new CustomException("No data to save.");
                }
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
        private IEnumerable<EmployeeLeaveSummary> GetProductionSummaryList(string Id)
        {
            //getbulletindetaillist
            try
            {
                string _sql = "select * from TRN.EmployeeLeaveSummary where Id ='"+Id+"' ";
                return _sqlRepository.GetModelCollection<EmployeeLeaveSummary>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(LeaveOpeningBalance), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "GL Mapping Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                EmployeeLeaveSummary entity = _EmployeeLeaveSummaryRepository.Find(id);
                // If section row inactive
                _EmployeeLeaveSummaryRepository.Delete(entity);
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

        public DataSet GetESICEligibleEmployee(string empSystemId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM ESICEligibleEmployee WHERE EmpSystemID='" + empSystemId + "' AND IsActive=1"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public GridModel XGetLeaveTypeList(GridParameter parameters, string employeeId, string calendarId, string plantId, string companyGroupId)
        {
            try
            {
                var esic = GetESICEligibleEmployee(employeeId);
                if (esic.Tables[0].Rows.Count > 0)
                {
                    parameters.CmdText = @"SELECT distinct LT.Id LeaveTypeId,ELS.Id,  LT.UserName LeaveType,ELS.CurrentYearAvailedOpeningBalance,ELS.CurrentYearEarnedDaysOpeningBalance,ELS.CarryForwardOpeningBalance
				                          FROM TRN.EmployeeLeaveSummary ELS
				                          LEFT JOIN dbo.ESICPolicyLeaveType AS EPLT ON EPLT.LeaveTypeID=ELS.LeaveTypeId
                                          LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                                          WHERE ELS.EmployeeId='" + employeeId + @"' and ELS.CalanderYearId='" + calendarId + @"' AND
                                          EPLT.LeaveTypeID IN
                                           (
                                             SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                                          LEFT JOIN MST.DesignationMaster AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                                          LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                                          WHERE EI.SystemID= '" + employeeId + @"' AND EI.GroupID='" + companyGroupId + @"' AND EI.PlantID='" + plantId + @"'
                                           )
                                        AND
                                        EPLT.ESICPolicyMasterID IN (
                                         SELECT DM.ESICPolicyMasterID FROM MST.DesignationMaster DM
                                         WHERE DM.DesignationId IN (
                                          SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + employeeId + @"'
                                          )
                )";
                }
                else
                {
                    parameters.CmdText = @" SELECT distinct ELS.Id,  LT.Id LeaveTypeId,LT.UserName LeaveType,ELS.CurrentYearAvailedOpeningBalance,ELS.CurrentYearEarnedDaysOpeningBalance,ELS.CarryForwardOpeningBalance
	                                        FROM TRN.EmployeeLeaveSummary ELS
									        LEFT JOIN LeaveType LT ON ELS.LeaveTypeId = LT.Id
                                            LEFT JOIN LeavePolicyDetail LPD ON LPD.LTSystemID=LT.Id
                                            LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID=LPD.LPMSystemID
                                            LEFT JOIN MST.DesignationMaster DM ON DM.LeavePolicyMasterId=LPM.SystemID
                                            LEFT JOIN EmployeeInformation EI ON EI.GivenDesignationId=DM.DesignationId AND EI.SystemId=ELS.EmployeeId
                                            WHERE EI.SystemID='" + employeeId + @"' AND EI.GroupID='" + companyGroupId + @"' AND EI.PlantID='" + plantId + @"' AND LT.IsGeneral = 1 AND
									        CalanderYearId='" + calendarId + "'";
                }
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel GetLeaveTypeList(GridParameter parameters, string employeeId, string calendarId, string plantId, string companyGroupId)
        {
            try
            {
                var esic = GetESICEligibleEmployee(employeeId);
                if (esic.Tables[0].Rows.Count > 0)
                {
                    parameters.CmdText = @"SELECT distinct LT.Id LeaveTypeId
                                                  ,ELS.Id,  LT.UserName LeaveType,ELS.CurrentYearAvailedOpeningBalance,ELS.CurrentYearEarnedDaysOpeningBalance,ELS.CarryForwardOpeningBalance
				                          FROM TRN.EmployeeLeaveSummary ELS
				                          LEFT JOIN dbo.ESICPolicyLeaveType AS EPLT ON EPLT.LeaveTypeID=ELS.LeaveTypeId
                                          LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                                          WHERE ELS.EmployeeId='" + employeeId + @"' and ELS.CalanderYearId='" + calendarId + @"' AND
                                          EPLT.LeaveTypeID IN
                                           (
                                             SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                                          LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
											LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
											WHERE DC.PlantId='" + plantId + @"') AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                                          LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                                          WHERE EI.SystemID= '" + employeeId + @"' AND EI.GroupID='" + companyGroupId + @"' AND EI.PlantID='" + plantId + @"'
                                           )
                                        AND
                                        EPLT.ESICPolicyMasterID IN (
                                        SELECT DC.ESICPolicyMasterID FROM MST.DesignationMaster DM
										LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                         WHERE DM.DesignationId IN (
                                          SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + employeeId + @"'
                                          )
                )";
                }
                else
                {
                    parameters.CmdText = @" SELECT distinct ELS.Id,  LT.Id LeaveTypeId,LT.UserName LeaveType,ELS.CurrentYearAvailedOpeningBalance,ELS.CurrentYearEarnedDaysOpeningBalance,ELS.CarryForwardOpeningBalance
	                                        FROM TRN.EmployeeLeaveSummary ELS
									        LEFT JOIN LeaveType LT ON ELS.LeaveTypeId = LT.Id
                                            LEFT JOIN LeavePolicyDetail LPD ON LPD.LTSystemID=LT.Id
                                            LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID=LPD.LPMSystemID
                                            LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
											LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
											WHERE DC.PlantId='" + plantId + @"') DM ON DM.LeavePolicyMasterId=LPM.SystemID
                                            LEFT JOIN EmployeeInformation EI ON EI.GivenDesignationId=DM.DesignationId AND EI.SystemId=ELS.EmployeeId
                                            WHERE EI.SystemID='" + employeeId + @"' AND EI.GroupID='" + companyGroupId + @"' AND EI.PlantID='" + plantId + @"' AND LT.IsGeneral = 1 AND
									        CalanderYearId='" + calendarId + "'";
                }
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string plantId, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT ES.*,EI.EmployeeName FROM [HKP].[LeaveOpeningBalance] ES
                                        LEFT JOIN EmployeeInformation EI ON ES.[EmployeeId]= EI.SystemId
                                        WHERE ES.PlantId='" + plantId + "' AND ES.companyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
    }
}