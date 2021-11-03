#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Model.HumanResources;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.HumanResources
{
    public class EmployeeShiftAssignService : Service<EmployeeShiftAssign>, IEmployeeShiftAssignService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<EmployeeWeekOffByDay> _employeeWeekOffByDayRepository;
        private readonly IRepositoryAsync<EmpDateWiseShiftAssign> _empDateWiseShiftAssignRepository;

        public EmployeeShiftAssignService(
            IRepositoryAsync<EmployeeShiftAssign> employeeShiftAssignRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<EmployeeWeekOffByDay> employeeWeekOffByDayRepository
            , IRepositoryAsync<EmpDateWiseShiftAssign> empDateWiseShiftAssignRepository
            ) : base(employeeShiftAssignRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
            _employeeWeekOffByDayRepository = employeeWeekOffByDayRepository;
            _empDateWiseShiftAssignRepository = empDateWiseShiftAssignRepository;
        }

        #endregion Constructor

        public string GetPK()
        {
            return GetAutoNumber(nameof(EmployeeShiftAssign), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public string GetWeekOffPK()
        {
            return GetAutoNumber(nameof(EmployeeWeekOffByDay), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void Insert(EmployeeShiftAssign entity, EmployeeWeekOffByDay employeeWeekOffByDay)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.SystemID = "ESA-" + GetPK();
                entity.DateAdded = DateTime.Now;
                base.Insert(entity);
                employeeWeekOffByDay.SystemID = "EWD" + GetWeekOffPK();
                employeeWeekOffByDay.AddedBy = entity.AddedBy;
                employeeWeekOffByDay.DateAdded = entity.DateAdded;
                employeeWeekOffByDay.ModelState = ModelState.Added;
                _employeeWeekOffByDayRepository.Insert(employeeWeekOffByDay);

                var dataList = GetEmpDateWiseShiftAssignData(entity.EmpSystemID, entity.EffectiveDate);
                if (dataList != null)
                {
                    foreach (EmpDateWiseShiftAssign item in dataList)
                    {
                        item.ToReprocess = "Yes";
                        item.UpdatedBy = entity.AddedBy;
                        item.DateUpdated = entity.DateAdded;
                        item.ModelState = ModelState.Modified;
                        _empDateWiseShiftAssignRepository.Update(item);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private IEnumerable<EmpDateWiseShiftAssign> GetEmpDateWiseShiftAssignData(string empId, DateTime? date)
        {
            var sql = @"Select * from dbo.EmpDateWiseShiftAssign  Where EmpSystemID='" + empId + @"' AND WorkDate>='" + date + "'";
            return _sqlRepository.GetModelCollection<EmpDateWiseShiftAssign>(sql);
        }

        public override void Update(EmployeeShiftAssign entity)
        {
            try
            {
                //Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string plantId, string date)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.SystemId EmpSystemID, EI.EmployeeCode, EI.EmployeeName, APD.DayStatus, SDFx.UserName FixShift,SDRs.UserName RosterStartShift,
											   SDCr.UserName CurrentShift,CONVERT(VARCHAR(5),EDS.ShiftInTime,108) ShiftInTime, ESA.EffectiveDate, EWD.AlignWithCC,
											   EWD.IndividualWeekOff, EWD.FstOffDay, EWD.FstDayLengthType, EWD.SndOffDay, EWD.SndDayLengthType,  CONVERT(VARCHAR(5),APD.InTime,108) InTime
											   ,ESA.IsFix, ESA.IsRoster,ESA.RosterSystemID,ESA.StartFromDay, SR.ShiftRosterName,ESA.FixSystemID, ESA.RosterStartShiftID
										FROM EmployeeInformation EI
										INNER JOIN (
												SELECT A.* FROM EmployeeShiftAssign A
													INNER JOIN (
																SELECT EmpSystemID, MAX(EffectiveDate) EffectiveDate FROM EmployeeShiftAssign
																 WHERE EffectiveDate <= '" + date + @"'
																GROUP BY EmpSystemID
																) B ON A.EmpSystemID = B.EmpSystemID AND A.EffectiveDate = B.EffectiveDate
											  ) ESA ON ESA.EmpSystemID=EI.SystemId
										LEFT JOIN [dbo].[EmpDateWiseShiftAssign] EDS ON ESA.SystemID = EDS.EmpSftAssiSystemID AND EDS.WorkDate = '" + date + @"'
										LEFT JOIN dbo.EmployeeWeekOffByDay EWD ON ESA.EmpSystemID = EWD.EmpSystemID AND ESA.EffectiveDate = EWD.EffectiveDate
										AND ESA.FixSystemID=EWD.FixSystemID
										INNER JOIN AttdnProcessData APD ON EI.SystemId = APD.EmpSystemID
										LEFT JOIN ShiftDefination SDFx ON ESA.FixSystemID = SDFx.SystemID
										LEFT JOIN ShiftDefination SDRs ON ESA.RosterStartShiftID = SDRs.SystemID
										LEFT JOIN ShiftDefination SDCr ON EDS.ShiftSystemID = SDCr.SystemID
										LEFT JOIN dbo.ShiftRosterMaster SR ON ESA.RosterSystemID=SR.SystemID
										Where EI.PlantId='" + plantId + @"' AND APD.WorkDate = '" + date + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeData(GridParameter parameters, string plantId, string empId)
        {
            try
            {
                parameters.CmdText = @"SELECT E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,E.BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
							    	,E.EmpType
							    	,E.GivenDesignationId
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,DEPT.UserName AS Department
									,E.EmployeeCode
									,E.EmpPicPath
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    WHERE E.EmployeeStatus = 'Active' AND E.IsApproved=1  AND E.PlantId = '" + plantId + @"' AND SystemId<> '" + empId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void Delete(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                base.Delete(Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public IEnumerable<object> GetRoasterCboByPlant(string plantId)
        {
            var sql = @"SELECT SystemID AS [Value], ShiftRosterName AS [Text] FROM ShiftRosterMaster WHERE PlantID='" + plantId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetRosterWiseShiftName(string plantId, string raosterId)
        {
            var sql = @"SELECT SRC.ShiftDefinationID  AS [Value],
                                    SD.ShiftDefinationName AS [Text]
                            FROM ShiftRosterChild SRC
	                                    LEFT JOIN ShiftDefination SD ON SRC.ShiftDefinationID = SD.SystemID
                            WHERE SRC.PlantID = '" + plantId + @"' AND SRC.SRMasterSystemID = '" + raosterId + @"'
							ORDER BY SRC.ShiftSequence";
            return _sqlRepository.GetDataCollection(sql, null);
        }
    }
}