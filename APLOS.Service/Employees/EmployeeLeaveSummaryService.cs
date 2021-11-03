#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Employees
{
    public class EmployeeLeaveSummaryService : Service<EmployeeLeaveSummary>, IEmployeeLeaveSummaryService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<EmployeeLeaveSummary> _employeeLeaveSummaryRepository;
        private readonly IEmployeeInformationService _employeeInformationService;

        public EmployeeLeaveSummaryService(
            IRepositoryAsync<EmployeeLeaveSummary> employeeLeaveSummaryRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , IEmployeeInformationService employeeInformationService
            , ISqlRepository sqlRepository

            ) : base(employeeLeaveSummaryRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _employeeLeaveSummaryRepository = employeeLeaveSummaryRepository;
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _employeeInformationService = employeeInformationService;
        }

        #endregion Constructor

        private string GetPK()
        {
            //return GetAutoNumber(nameof(EmployeeLeaveSummary), PKGeneratorEnum.Auto, null, DateTime.Now);
            return GetAutoNumber("EMPLEAVESUMM", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public GridModel GetYearList(string CompanyGroupId)
        {
            try
            {
                var _sql = @" SELECT Id AS [Value], YearNo AS [Text] FROM [DBO].[YearlyCalendar] where CompanyGroupId='" + CompanyGroupId + "'";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = _sql });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetYearCboList(string plantId)
        {
            try
            {
                var _sql = @" SELECT Id AS [Value], YearNo AS [Text] FROM [DBO].[YearlyCalendar] WHERE PlantId='" + plantId + "'";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = _sql });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetLeaveTypeList(string CompanyGroupId)
        {
            try
            {
                var _sql = @" SELECT
                                    LT.Id AS [Value],
                                    LT.UserName AS [Text] FROM [DBO].[LeavePolicyDetail] LPD
                                    left outer join DBO.LeaveType LT on LPD.LTSystemID = LT.Id
                                    where LPD.IsProrataPreviousyear= 1 and CompanyGroupId='" + CompanyGroupId + "'";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = _sql });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetLeaveTypeCumulativeList(string CompanyGroupId)
        {
            try
            {
                var _sql = @" SELECT
                                    LT.Id AS [Value],
                                    LT.UserName AS [Text] FROM [DBO].[LeavePolicyDetail] LPD
                                    left outer join DBO.LeaveType LT on LPD.LTSystemID = LT.Id
                                    where LPD.IsCarryForwardCumulative = 1 and CompanyGroupId='" + CompanyGroupId + "'";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = _sql });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel ActiveEmpListByPlantId(GridParameter parameters, string plantID, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId)
        {
            try
            {
                var str = "";
                if (!isControlAdmin && !isSysAdmin)
                {
                    str = @" AND emp.BudgetCode IN (SELECT Id from mst.ManpowerBudget WHERE EntityId IN (SELECT entityid FROM [HKP].[ApprovalConfiguration] WHERE ResignationApply='" + employeeId + "'))";
                }

                parameters.CmdText = @"SELECT
                                                 EMP.SystemId as EmployeeId
                                                ,EMP.EmployeeName
                                                ,EMP.EmployeeCode
												,EMP.PlantId
                                                ,EMP.GroupId
                                                ,CTD.CutOffDate
												,c.UserName as EmployeeCategory
                                                ,Replace(CONVERT(VARCHAR(11), emp.DOJ, 106), ' ', '-') DOJ
	                                            ,Replace(CONVERT(VARCHAR(11), emp.DOC, 106), ' ', '-') DOC
		                                        ,E.UserName as Entity
                                                ,PR.UserName position
                                                ,DEPT.UserName Department
		                                        ,D.UserName Designation
		                                        ,DEG.UserName GivenDesignation
												--,ELS.CurrentYearAllocation
												--,ELS.DaysCanBeSanctioned
                                                --,ELS.Id LeaveSummaryId
		                                        FROM dbo.EmployeeInformation AS EMP
		                                        LEFT OUTER JOIN [MST].[ManpowerBudget]  PMB on PMB.id=emp.BudgetCode
												LEFT OUTER JOIN ( select * from [SCS].[OpeningBalanceCutOffDate] where ModuleName ='HR') CTD ON CTD.CompanyGroupId = EMP.GroupID and CTD.PlantId = EMP.PlantId

		                                        LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                                                LEFT OUTER JOIN [DBO].[PlantWiseHRMSSetting] HR on HR.PlantID = EMP.PlantId
												--LEFT OUTER JOIN [TRN].[EmployeeLeaveSummary] ELS ON ELS.EmployeeId = EMP.SystemId
		                                        LEFT OUTER JOIN [HKP].[Designation] DEG ON EMP.GivenDesignationId=DEG.Id
		                                        LEFT OUTER JOIN [HKP].[Designation] D ON PR.DesignationId=D.Id
		                                        LEFT OUTER JOIN [ORG].[Department] DEPT ON PR.DepartmentId=DEPT.Id
		                                        LEFT OUTER JOIN [HKP].[EmployeeCategory] C ON C.id=EMP.EmployeeCategorySystemID
		                                        LEFT OUTER JOIN [ORG].[Entity] E ON E.Id=PMB.entityid
		                                        where E.UserName <> ' ' and EMP.DOJ < CTD.CutOffDate
                                                and EMP.PlantId ='" + plantID + "' " +
                                                "and EMP.EmployeeName <> '' " +
                                                "and emp.IsApproved = 1" +
                                                "and EMP.EmployeeStatus = 'Active'" + str;

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public static string GetLeaveMasterId(DataTable dt)
        {
            var id = "''";
            try
            {
                var dv = new DataView(dt);
                var dtNew = dv.ToTable(true, "LeavePolicyMasterId");

                for (int i = 0; i < dtNew.Rows.Count; i++)
                {
                    if (id == "''")
                    {
                        id = "'" + dtNew.Rows[i]["LeavePolicyMasterId"] + "'";
                    }
                    else
                    {
                        id += ",'" + dtNew.Rows[i]["LeavePolicyMasterId"] + "'";
                    }
                }
                return id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<EmployeeLeaveSummary> LoadEmpLeaveSummaryData(string companyGroupId, string plantId, string CalendarYearId)
        {
            try
            {
                var _sql = "SELECT * FROM trn.EmployeeLeaveSummary WHERE CompanyGroupId='" + companyGroupId + "'  AND PlantId='" + plantId + "' and CalanderYearId='" + CalendarYearId + "'";
                return _employeeLeaveSummaryRepository.SqlQuery<EmployeeLeaveSummary>(_sql).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable LeavePolicyDetail(string leavePolicyMasterId)
        {
            try
            {
                var _sql = @"select * from dbo.LeavePolicyDetail where LPMSystemID IN (" + leavePolicyMasterId + ")";
                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetCutOffDate(string plantId, string CompanyGroupId)
        {
            string _sql;
            var cutOffDate = string.Empty;
            try
            {
                _sql = @"select CutOffDate from [SCS].[OpeningBalanceCutOffDate] where CompanyGroupId='" + CompanyGroupId + "' and plantId= '" + plantId + "' AND ModuleName = 'HR' ";
                var dt = _sqlRepository.GetDataTable(_sql);
                if (dt.Rows.Count > 0)
                {
                    cutOffDate = dt.Rows[0]["CutOffDate"].ToString();
                }
                return cutOffDate;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetCalendarYear(string plantId)
        {
            string _sql;
            try
            {
                _sql = @"SELECT * FROM DBO.YearlyCalendar WHERE '" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' BETWEEN FromDate AND ToDate AND PlantId = '" + plantId + "' ";
                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetWorkingDays(string LPDetailID, string EmpSystemID, string earnStartDate, string earnEndDate)
        {
            string _sql;
            try
            {
                _sql = @" select count(EmpSystemID) WorkingDays from AttdnProcessData
                                    where WorkDate between '" + earnStartDate + @"' and '" + earnEndDate + @"'
                                    and EmpSystemID='" + EmpSystemID + @"' and DayStatus in
                        (select DayType from LeavePolicyWorkingDays where LPDetailID='" + LPDetailID + "') ";
                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable LeaveAllocationInfo(string CompanyGroupId, string plantId)
        {
            try
            {
                var _sql = @"select  emp.SystemId as EmployeeId
                                        ,emp.GroupID
	                                    ,emp.GivenDesignationId
	                                    ,dsm.LeavePolicyMasterId
                                        ,lpd.SystemID LeavePolicyDetailId
	                                    ,lpm.PolicyName
	                                    ,lty.LeaveType
										,lty.Id ltId
	                                    ,lpd.LeaveDays
                                        ,edws.WorkDate
                                        ,OBC.CutOffDate
	                                    ,(SELECT YearNo FROM dbo.YearlyCalendar WHERE '" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' BETWEEN FromDate and ToDate AND PlantId = '" + plantId + @"') CalendarYear
                                        from (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
										LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
										WHERE DC.PlantId='" + plantId + @"') as dsm
                                        left outer join dbo.EmployeeInformation as emp on emp.GivenDesignationId = dsm.DesignationId
										left outer join
													(select WorkDate,EmpSystemID,lpd.Daytype
														from  dbo.AttdnProcessData apd
														left outer join dbo.LeavePolicyWorkingDays lpd on lpd.Daytype = apd.DayStatus
														where PlantID='" + plantId + @"'and GroupID = '" + CompanyGroupId + @"'
														and YEAR(WorkDate) = (SELECT YEAR(getdate()))
														and DayStatus in( select DayStatus from dbo.LeavePolicyDetail)
														group by EmpSystemID,GroupID,PlantID,WorkDate,lpd.Daytype
													)as edws on edws.EmpSystemID = emp.SystemId
                                        left outer join dbo.LeavePolicyMaster as lpm on dsm.LeavePolicyMasterId = lpm.SystemID
                                        left outer join dbo.LeavePolicyDetail as lpd on lpd.LPMSystemID = lpm.SystemID
                                        LEFT outer join [SCS].[OpeningBalanceCutOffDate] OBC ON OBC.PlantId = EMP.PlantId
                                        left outer join dbo.LeaveType as lty on lty.Id = lpd.LTSystemID
										where  ISNULL(lty.LeaveType,'') <> '' and emp.GroupID = '" + CompanyGroupId + @"' AND emp.PlantId = '" + plantId + @"'
                                        AND emp.EmployeeStatus = 'Active'
                                        --AND EmpSystemID IN('1800164')
                                        ";
                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetPreviousYearEmployeeLeaveInfo(string CompanyGroupId, string plantId)
        {
            try
            {
                var _sql = @"select els.EmployeeId
                                        ,els.LeaveTypeId
                                        ,els.CalanderYearId
                                        ,els.DaysCanBeSanctioned
                                        ,els.AvailedDays
                                        ,(els.DaysCanBeSanctioned - els.AvailedDays) previousYearCarryForward
                                        ,yc.YearNo
                                        from trn.EmployeeLeaveSummary els
                                        Left outer join dbo.YearlyCalendar yc on yc.Id = els.CalanderYearId
                                        where yc.YearNo = (Year(getdate())-1) and els.plantid='" + plantId + "'";
                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// TODO:Add ESICdate
        /// </summary>
        /// <param name="CompanyGroupId"></param>
        /// <param name="plantId"></param>
        /// <returns></returns>
        private DataTable GetCurrentYearEmployeeLeaveInfo(string CompanyGroupId, string plantId)
        {
            try
            {
                var _sql = @"select  emp.SystemId as EmployeeId
                                        ,emp.GroupID
                                        ,emp.DOJ
										,emp.PlantId
	                                    ,emp.GivenDesignationId
	                                    ,dsm.LeavePolicyMasterId
	                                    ,lpm.PolicyName
	                                    ,lty.LeaveType
                                        ,isnull(esic.EndDate,'01-Jan-" + DateTime.Now.ToString("yyyy") + @"') ESICdate
                                        ,lty.Id ltId
	                                    ,lpd.LeaveDays
                                        ,lpd.EncashWorkingDaysQty
	                                    ,lpd.LTSystemID
                                        ,lpd.SystemID
                                        ,lpd.IsProrataPreviousyear
                                        ,OBC.CutOffDate
                                        ,lpd.IsProratacurrentyear ProData
	                                    ,ISNULL(z.CountedLeave,0) CountedLeave
	                                    ,ISNULL(ad.AppliedDays,0) AppliedDays
	                                    ,(select YearNo from dbo.YearlyCalendar where '" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' between FromDate and ToDate AND PlantId = '" + plantId + @"') CalendarYear

										,(select Id from dbo.YearlyCalendar where YearNo = (SELECT YEAR(getdate())) AND PlantId = '" + plantId + @"') CalendarYearId
                                        from (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
										LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
										WHERE DC.PlantId='" + plantId + @"') as dsm
                                        left outer join dbo.EmployeeInformation as emp on emp.GivenDesignationId = dsm.DesignationId
                                        left outer join dbo.LeavePolicyMaster as lpm on dsm.LeavePolicyMasterId = lpm.SystemID
                                        left outer join dbo.LeavePolicyDetail as lpd on lpd.LPMSystemID = lpm.SystemID
                                        left join (
										select * from [ESICEligibleEmployee] where IsActive=1 and EndDate is not null
										) esic on esic.EmpSystemID=emp.SystemId
                                        LEFT outer join [SCS].[OpeningBalanceCutOffDate] OBC ON OBC.PlantId = EMP.PlantId
                                        left outer join dbo.LeaveType as lty on lty.Id = lpd.LTSystemID
                                        left outer join (select EmpSystemID,LTSystemID,sum(CountedLeave) CountedLeave from
					                                        (
						                                       select ltrd.CountedLeave,ltr.LTSystemID,ltr.EmpSystemID from
							                                        (select * from  dbo.LeaveTransaction
								                                        --where ApprovalType = 'Pre Approve' and GroupID = '" + CompanyGroupId + @"'
								                                        where  GroupID = '" + CompanyGroupId + @"' and Year(FromDate)=(SELECT YEAR(getdate()))
							                                       ) as ltr
						                                        left outer join
						                                        (
							                                        select sum(LeaveDuration) CountedLeave, LvTrnsSystemID from dbo.LeaveTransactionDetails
							                                        where IsAvailed = 1 and Year(WorkDate)=(SELECT YEAR(getdate())) group by LvTrnsSystemID
						                                        ) as ltrd on  ltrd.LvTrnsSystemID = ltr.SystemID
					                                         ) x group by LTSystemID,EmpSystemID
                                                        ) z on z.EmpSystemID = emp.SystemId and z.LTSystemID = lpd.LTSystemID

					                                          left outer join (select Sum(LeaveDays) AppliedDays,EmpSystemID,LTSystemID from  dbo.LeaveTransaction
					                                          --where ApprovalType = 'Pre Approve' and Year(FromDate) = (SELECT YEAR(getdate()))
					                                          where  Year(FromDate) = (SELECT YEAR(getdate()))
                                                              and PlantID = '" + plantId + @"'
					                                          group by LTSystemID,EmpSystemID
				                                         ) as ad on ad.LTSystemID = lpd.LTSystemID and ad.EmpSystemID = emp.SystemId
                                                            where  ISNULL(lty.LeaveType,'') <> ''
                                                            and emp.GroupID = '" + CompanyGroupId + @"'
                                                            AND emp.PlantId = '" + plantId + @"'
                                                          and emp.EmployeeStatus = 'Active' --and emp.IsApproved = 1
                                                          ";
                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Save(string CompanyGroupId)
        {
            try
            {
                var plantListSql = @"SELECT CompanyGroupId, Id FROM ORG.Plant WHERE CompanyGroupId = '" + CompanyGroupId + @"' AND  Active = 1 AND Archive = 0";
                var plantList = _sqlRepository.GetDataTable(plantListSql);

                for (int i = 0; i < plantList.Rows.Count; i++)
                {
                    if (i == plantList.Rows.Count - 1)
                    {
                    }
                    SaveRoot(plantList.Rows[i][@"CompanyGroupId"].ToString(), plantList.Rows[i][@"Id"].ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// For Task Schedulars only
        /// </summary>
        public void TSSave()
        {
            try
            {
                var companyGroupSql = @"SELECT [Id]  FROM [ORG].[CompanyGroup] WHERE   Active = 1 AND Archive = 0";

                var companyGrouplist = _sqlRepository.GetDataTable(companyGroupSql);
                if (companyGrouplist.Rows.Count > 0)
                {
                    var plantListSql = @"SELECT CompanyGroupId, Id FROM ORG.Plant WHERE CompanyGroupId = '" + companyGrouplist.Rows[0]["Id"] + @"' AND  Active = 1 AND Archive = 0";
                    var plantList = _sqlRepository.GetDataTable(plantListSql);

                    for (int i = 0; i < plantList.Rows.Count; i++)
                    {
                        if (i == plantList.Rows.Count - 1)
                        {
                        }
                        //SaveRoot(CompanyGroupId, plantList.Rows[i][@"Id"].ToString());
                        SaveRoot(plantList.Rows[i][@"CompanyGroupId"].ToString(), plantList.Rows[i][@"Id"].ToString());
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveRoot(string CompanyGroupId, string plantId)
        {
            List<EmployeeLeaveSummary> from_db = null;

            var flag = false;
            try
            {
                InitLeaveSummary(CompanyGroupId, plantId, out from_db);

                ///load all plants of this CG
                ///loop the plants list
                ///
                if (from_db != null)
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;

                    foreach (var item in from_db)
                    {
                        InsertOrUpdateGraph(item);
                    }
                    _unitOfWork.SaveChanges();
                    flag = false;

                    _unitOfWork.Commit();
                }
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

        private void InitLeaveSummary(string CompanyGroupId, string plantId, out List<EmployeeLeaveSummary> from_db)
        {
            from_db = null;
            DataTable dtLeaveInfo = null;
            DataTable dtAllocaitonInfo = null;
            DataTable dtLeavePolicyDetail = null;
            DataTable dtCarryForward = null;
            try
            {
                #region variables

                var _count = 0;

                string empId;
                string CalendarYearId;
                string lvtId;
                decimal currentYearAllocation = 0;
                decimal dayCanBeAssigned = 0;
                decimal applieddays;
                decimal availedDays;
                bool proData;
                bool proDataPrevYear;
                var calendarYear = string.Empty;
                string leaveType;
                decimal leaveDays;
                decimal encashWorkingDaysQty;
                var _yearStartDate = DateTime.Now;
                var _yearEndDate = DateTime.Now;
                DateTime doj;
                var CutDate = DateTime.Now;
                var _ESICDate = DateTime.Now;

                string leaveTypeDetailId;

                #endregion variables

                if (GetCutOffDate(plantId, CompanyGroupId) != "")
                {
                    #region ds

                    CutDate = Convert.ToDateTime(GetCutOffDate(plantId, CompanyGroupId));
                    var dtYear = GetCalendarYear(plantId);

                    if (dtYear.Rows.Count > 0)
                    {
                        _yearStartDate = Convert.ToDateTime(dtYear.Rows[0]["FromDate"].ToString());
                        _yearEndDate = Convert.ToDateTime(dtYear.Rows[0]["ToDate"].ToString());
                        calendarYear = dtYear.Rows[0]["Id"].ToString();

                        from_db = LoadEmpLeaveSummaryData(CompanyGroupId, plantId, calendarYear).ToList<EmployeeLeaveSummary>();
                        dtLeaveInfo = GetCurrentYearEmployeeLeaveInfo(CompanyGroupId, plantId);

                        var leaveMasterIds = GetLeaveMasterId(dtLeaveInfo);
                        dtLeavePolicyDetail = LeavePolicyDetail(leaveMasterIds);
                        dtAllocaitonInfo = LeaveAllocationInfo(CompanyGroupId, plantId);
                        dtCarryForward = GetPreviousYearEmployeeLeaveInfo(CompanyGroupId, plantId);//plant 999

                        #endregion ds

                        var _pks = GetPK();
                        for (int i = 0; i < dtLeaveInfo.Rows.Count; i++)
                        {
                            #region variables

                            empId = dtLeaveInfo.Rows[i]["EmployeeId"].ToString();
                            CalendarYearId = dtLeaveInfo.Rows[i][nameof(CalendarYearId)].ToString();
                            calendarYear = dtLeaveInfo.Rows[i]["CalendarYear"].ToString();
                            lvtId = dtLeaveInfo.Rows[i]["ltId"].ToString();
                            leaveType = dtLeaveInfo.Rows[i]["LeaveType"].ToString();
                            leaveDays = (int)dtLeaveInfo.Rows[i]["LeaveDays"];
                            proData = (bool)dtLeaveInfo.Rows[i]["ProData"];
                            proDataPrevYear = (bool)dtLeaveInfo.Rows[i]["IsProrataPreviousyear"];
                            availedDays = Convert.ToDecimal(dtLeaveInfo.Rows[i]["CountedLeave"]);
                            applieddays = Convert.ToDecimal(dtLeaveInfo.Rows[i]["AppliedDays"]);
                            leaveTypeDetailId = dtLeaveInfo.Rows[i]["SystemID"].ToString();
                            encashWorkingDaysQty = dtLeaveInfo.Rows[i]["EncashWorkingDaysQty"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dtLeaveInfo.Rows[i]["EncashWorkingDaysQty"]);
                            doj = Convert.ToDateTime(dtLeaveInfo.Rows[i]["DOJ"].ToString());
                            if (string.IsNullOrEmpty(dtLeaveInfo.Rows[i]["ESICDate"].ToString()) == false)
                            {
                                _ESICDate = Convert.ToDateTime(dtLeaveInfo.Rows[i]["ESICDate"].ToString());
                            }

                            var dvLeaveType = new DataView(dtLeavePolicyDetail)
                            {
                                RowFilter = "SystemID='" + leaveTypeDetailId + " '"
                            };
                            var dtLeavType = dvLeaveType.ToTable();

                            var _policyDetailId = dtLeavType.Rows[0]["SystemID"].ToString();
                            var _poliicyMasterId = dtLeavType.Rows[0]["LPMSystemID"].ToString();
                            var _leaveTypeId = dtLeavType.Rows[0]["LTSystemID"].ToString();
                            var _maxLeaveDays = (int)dtLeavType.Rows[0]["LeaveDays"];
                            //EncashWorkingDaysQty	EncashEarnLeaveQty
                            var _EncashWorkingDaysQty = dtLeavType.Rows[0]["EncashWorkingDaysQty"].ToString();
                            var _EncashEarnLeaveQty = dtLeavType.Rows[0]["EncashEarnLeaveQty"].ToString();

                            var dvAllocation = new DataView(dtAllocaitonInfo)
                            {
                                RowFilter = "EmployeeId='" + empId + "' AND ltId= '" + lvtId + "'"
                            };
                            var dtLeaveAllocation = dvAllocation.ToTable();

                            var workDays = dtLeaveAllocation.Rows.Count;

                            #endregion variables

                            if (workDays > 0)
                            {
                                var policyDetailId = dtLeaveAllocation.Rows[0]["LeavePolicyDetailId"].ToString();
                                var policyMasterId = dtLeaveAllocation.Rows[0]["LeavePolicyMasterId"].ToString();
                                var leaveTypeId = dtLeaveAllocation.Rows[0]["ltId"].ToString();

                                if (_policyDetailId == policyDetailId && _poliicyMasterId == policyMasterId && _leaveTypeId == leaveTypeId)
                                {
                                    var date = Convert.ToDateTime(doj);
                                    var joinYear = date.Year;
                                    //var lastDay = new DateTime(joinYear, 12, 31);
                                    var firstDay = new DateTime(DateTime.Now.Year, 1, 1);

                                    //DataView dvCF = new DataView(dtCarryForward);
                                    //dvCF.RowFilter = "EmployeeId='"+ empId + "' and LeaveTypeId='"+ lvtId + "' and CalendarYearId='" + CalendarYearId + "'";
                                    //if(dvCF.Count>0)
                                    //{
                                    //}
                                    //if (dtCarryForward.Rows.Count > 0)
                                    //{
                                    //    var emp = dtCarryForward.Rows[i]["EmployeeId"].ToString();
                                    //    var pLeaveTypeId = dtCarryForward.Rows[i]["LeaveTypeId"].ToString();
                                    //    prevYearCarryForward = emp == empId && pLeaveTypeId == lvtId && proDataPrevYear ? (int)dtCarryForward.Rows[i]["previousYearCarryForward"] : 0;
                                    //}
                                    var earnEndDate = DateTime.Now;

                                    if (_ESICDate > DateTime.Now)
                                    {
                                        _ESICDate = _yearStartDate;
                                    }

                                    if (leaveType == "Earn")
                                    {
                                        var earnStartDate = GetEarnLeaveStartDate(doj, CutDate, _yearStartDate, _ESICDate);//get the greater one

                                        var dividingFator = decimal.Divide(Convert.ToDecimal(_EncashEarnLeaveQty), Convert.ToDecimal(_EncashWorkingDaysQty));//From Setting

                                        var totalDays = 0;
                                        var dtWD = GetWorkingDays(_policyDetailId, empId, earnStartDate.ToString("dd-MMM-yyyy"), earnEndDate.ToString("dd-MMM-yyyy"));//by fromDate toDate
                                        if (dtWD.Rows.Count > 0)
                                        {
                                            totalDays = Convert.ToInt32(dtWD.Rows[0]["WorkingDays"].ToString());
                                        }

                                        GetTotalLeave(out currentYearAllocation, out dayCanBeAssigned, totalDays, _leaveTypeId, empId, dividingFator, from_db);
                                    }
                                    else
                                    {
                                        var earnStartDate = GetEarnLeaveStartDate(doj, doj, _yearStartDate, _ESICDate);//get the greater one
                                        var difference = (_yearEndDate - earnStartDate);
                                        var days = Convert.ToInt32(difference.TotalDays) + 1;
                                        dayCanBeAssigned = 0;
                                        currentYearAllocation = 0;
                                        if (!proData)
                                        {
                                            //if (joinYear == DateTime.Now.Year)
                                            //{
                                            //    var difference = lastDay - doj;
                                            //    var days = Convert.ToInt32(difference.TotalDays);
                                            /// currentYearAllocation = Convert.ToInt32(leaveDays * days) / 365;
                                            currentYearAllocation = Convert.ToInt32(leaveDays * days) / (DateTime.IsLeapYear(DateTime.Now.Year) ? 366 : 365);
                                            dayCanBeAssigned = Convert.ToInt32(currentYearAllocation);
                                            //}
                                            //else
                                            //{
                                            //    currentYearAllocation = Convert.ToInt32(leaveDays);
                                            //    dayCanBeAssigned = Convert.ToInt32(currentYearAllocation);
                                            //}
                                        }//not prodata
                                        else // ProRata
                                        {
                                            currentYearAllocation = Convert.ToInt32(leaveDays * days) / (DateTime.IsLeapYear(DateTime.Now.Year) ? 366 : 365);
                                            var difference1 = DateTime.Now - earnStartDate;
                                            int _days_dcbs = (int)(difference1.TotalDays);
                                            //dayCanBeAssigned = Convert.ToInt32((currentYearAllocation * _days_dcbs)) / (DateTime.IsLeapYear(DateTime.Now.Year) ? 366 : 365);
                                            dayCanBeAssigned = Convert.ToInt32((leaveDays * _days_dcbs)) / (DateTime.IsLeapYear(DateTime.Now.Year) ? 366 : 365);

                                            //if (joinYear == DateTime.Now.Year)
                                            //{
                                            //    var difference = lastDay - doj;
                                            //    var days = Convert.ToDecimal(difference.TotalDays);
                                            //    currentYearAllocation = Convert.ToInt32((leaveDays * days)) / 365;

                                            //    var difference1 = DateTime.Now - doj;
                                            //    int _days = (int)(difference1.TotalDays);
                                            //    dayCanBeAssigned = Convert.ToInt32((currentYearAllocation * days)) / 365;
                                            //}
                                            //else
                                            //{
                                            //    currentYearAllocation = Convert.ToInt32(leaveDays);
                                            //    var difference1 = DateTime.Now - firstDay;
                                            //    var days = Convert.ToDecimal(difference1.TotalDays);
                                            //    dayCanBeAssigned = Convert.ToInt32((leaveDays * days)) / 365;
                                            //}
                                        }// Pro rata

                                        GetTotalLeaveNonEarn(ref dayCanBeAssigned, _leaveTypeId, empId, from_db);
                                    }
                                }//if
                            }//workdays

                            #region Database entry

                            if (empId == null || leaveType == null || CalendarYearId == null)
                            {
                            }
                            else
                            {
                                var db = from_db.FirstOrDefault(a => a.EmployeeId == empId && a.LeaveTypeId == lvtId && a.CalanderYearId == CalendarYearId);
                                if (db == null)
                                {
                                    db = new EmployeeLeaveSummary
                                    {
                                        ModelState = ModelState.Added
                                    };
                                    AuditService.Log(db);

                                    _count++;
                                    db.Id = "LS" + _pks + DateTime.Now.ToString("yy") + "-" + _count;
                                    db.EmployeeId = empId;
                                    db.CalanderYearId = CalendarYearId;
                                    db.PlantId = plantId;
                                    db.CompanyGroupId = CompanyGroupId;
                                    db.LeaveTypeId = lvtId;
                                    db.CurrentYearAllocation = currentYearAllocation;
                                    db.DaysCanBeSanctioned = dayCanBeAssigned;
                                    db.CurrentYearAvailedOpeningBalance = 0;
                                    db.CurrentYearEarnedDaysOpeningBalance = 0;
                                    db.CarryForwardOpeningBalance = 0;

                                    db.AvailedDays = availedDays;
                                    db.AppliedDays = applieddays;
                                    db.AddedBy = "scheduler";
                                    db.AddedDate = DateTime.Now;
                                    db.AddedFromIP = "::1";

                                    from_db.Add(db);
                                }
                                else
                                {
                                    db.CurrentYearAllocation = currentYearAllocation;
                                    db.DaysCanBeSanctioned = dayCanBeAssigned;

                                    db.ModelState = ModelState.Modified;
                                    AuditService.Log(db);
                                }
                            }

                            #endregion Database entry
                        }//loop dtLeaveInfo
                    }//calendar year found
                }//cutoff date
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void GetTotalLeave(out decimal pCalculatedValue, out decimal daysCanbeSanctioned, int totalWorkingDay, string _leaveTypeId, string empId, decimal dividingFactor, List<EmployeeLeaveSummary> from_db)
        {
            try
            {
                pCalculatedValue = 0;
                daysCanbeSanctioned = 0;

                var _CurrentYearAvailedOpeningBalance = from_db.Where(r => r.EmployeeId == empId & r.LeaveTypeId == _leaveTypeId).Select(r => r.CurrentYearAvailedOpeningBalance).FirstOrDefault();
                var _CurrentYearEarnedDaysOpeningBalance = from_db.Where(r => r.EmployeeId == empId & r.LeaveTypeId == _leaveTypeId).Select(r => r.CurrentYearEarnedDaysOpeningBalance).FirstOrDefault();
                var currentYearTotalDays = _CurrentYearEarnedDaysOpeningBalance + totalWorkingDay;
                var _CarryForwardOpeningBalance = from_db.Where(r => r.EmployeeId == empId & r.LeaveTypeId == _leaveTypeId).Select(r => r.CarryForwardOpeningBalance).FirstOrDefault();
                var _CarryForward = from_db.Where(r => r.EmployeeId == empId & r.LeaveTypeId == _leaveTypeId).Select(r => r.CarryForward).FirstOrDefault();

                var carryForward = _CarryForwardOpeningBalance + _CarryForward;
                pCalculatedValue = (int)(currentYearTotalDays * dividingFactor);
                daysCanbeSanctioned = pCalculatedValue + _CarryForwardOpeningBalance + _CarryForward;//-current year availed+current year earned days/dividing factor
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void GetTotalLeaveNonEarn(ref decimal daysCanbeSanctioned, string _leaveTypeId, string empId, List<EmployeeLeaveSummary> from_db)
        {
            try
            {
                //var _CurrentYearAvailedOpeningBalance = from_db.Where(r => r.EmployeeId == empId & r.LeaveTypeId == _leaveTypeId).Select(r => r.CurrentYearAvailedOpeningBalance).FirstOrDefault();
                //var _CurrentYearEarnedDaysOpeningBalance = from_db.Where(r => r.EmployeeId == empId & r.LeaveTypeId == _leaveTypeId).Select(r => r.CurrentYearEarnedDaysOpeningBalance).FirstOrDefault();
                //var currentYearTotalDays = _CurrentYearEarnedDaysOpeningBalance + totalWorkingDay;
                var _CarryForwardOpeningBalance = from_db.Where(r => r.EmployeeId == empId & r.LeaveTypeId == _leaveTypeId).Select(r => r.CarryForwardOpeningBalance).FirstOrDefault();
                var _CarryForward = from_db.Where(r => r.EmployeeId == empId & r.LeaveTypeId == _leaveTypeId).Select(r => r.CarryForward).FirstOrDefault();

                var carryForward = _CarryForwardOpeningBalance + _CarryForward;
                //pCalculatedValue = (int)(currentYearTotalDays * dividingFactor);
                daysCanbeSanctioned += _CarryForwardOpeningBalance + _CarryForward;//-current year availed+current year earned days/dividing factor
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateLeaveBalance(EmployeeLeaveSummary entity, string plantId, string CompanyGroupId)
        {
            string CutDate;
            try
            {
                CutDate = GetCutOffDate(plantId, CompanyGroupId);
                var dt = (Convert.ToDateTime(CutDate));
                var year = Convert.ToInt32(dt);
                if (year == DateTime.Now.Year)
                {
                    Update(entity);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateCarryForward(EmployeeLeaveSummary entity, string plantId, string CompanyGroupId)
        {
            List<EmployeeLeaveSummary> from_db = null;
            var calendarYear = string.Empty;

            try
            {
                var _pks = GetPK();
                var dtYear = GetCalendarYear(plantId);
                if (dtYear.Rows.Count > 0)
                {
                    calendarYear = dtYear.Rows[0]["Id"].ToString();
                }

                from_db = LoadEmpLeaveSummaryData(CompanyGroupId, plantId, calendarYear).ToList();
                var db = from_db.FirstOrDefault(a => a.EmployeeId == entity.EmployeeId && a.LeaveTypeId == entity.LeaveTypeId && a.CalanderYearId == entity.CalanderYearId);
                if (db == null)
                {
                    db = new EmployeeLeaveSummary
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(db);
                    db.Id = "LS_" + _pks + DateTime.Now.ToString("yy");
                    db.EmployeeId = entity.EmployeeId;
                    db.LeaveTypeId = entity.LeaveTypeId;
                    db.CalanderYearId = entity.CalanderYearId;
                    Insert(db);
                    //from_db.Add(db);
                }
                else
                {
                    db.ModelState = ModelState.Modified;
                    AuditService.Log(db);
                    Update(db);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public static DateTime XGetEarnLeaveStartDate(DateTime doj, DateTime cutOffDate, DateTime yearStartDate, DateTime ESICdate)
        {
            DateTime earnLeaveStartDate;

            if (doj > cutOffDate)
            {
                if (doj > yearStartDate)
                {
                    earnLeaveStartDate = cutOffDate;
                }
                else
                {
                    earnLeaveStartDate = yearStartDate;
                }
            }
            else if (yearStartDate < doj)
            {
                earnLeaveStartDate = doj;
            }
            else if (yearStartDate < ESICdate)
            {
                earnLeaveStartDate = yearStartDate;
            }
            else
            {
                earnLeaveStartDate = yearStartDate;
            }

            return earnLeaveStartDate;
        }

        public static DateTime GetEarnLeaveStartDate(DateTime doj, DateTime cutOffDate, DateTime yearStartDate, DateTime ESICdate)
        {
            var a = GetBigger(doj, cutOffDate);

            var b = GetBigger(yearStartDate, ESICdate);
            return GetBigger(a, b);
        }

        private static DateTime GetBigger(DateTime a, DateTime b)
        {
            try
            {
                return a > b ? a : b;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetLeavePolicyDays(string plantId)
        {
            try
            {
                var _sql = @"SELECT * FROM DBO.YearlyCalendar WHERE '" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' BETWEEN FromDate AND ToDate AND PlantId = '" + plantId + "' ";
                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}