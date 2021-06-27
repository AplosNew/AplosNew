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
using Library.Service.Logs;
using Library.Service.Recruitments;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class ResignationReqcruitmentPlanningService : Service<RecruitmentPlanningProcessSet>, IResignationReqcruitmentPlanningService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<Resignation> _resignationRepository;
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IRecruitmentPlanningService _recruitmentPlanningService;
        private readonly IRepositoryAsync<RecruitmentPlanningProcessSet> _recruitmentPlanningProcessSetRepository;

        public ResignationReqcruitmentPlanningService(
            IRepositoryAsync<Resignation> resignationRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IEmployeeInformationService employeeInformationService
            , IRecruitmentPlanningService recruitmentPlanningService
            , IRepositoryAsync<RecruitmentPlanningProcessSet> recruitmentPlanningProcessSetRepository
            ) : base(recruitmentPlanningProcessSetRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _resignationRepository = resignationRepository;
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _employeeInformationService = employeeInformationService;
            _recruitmentPlanningService = recruitmentPlanningService;
            _recruitmentPlanningProcessSetRepository = recruitmentPlanningProcessSetRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(RecruitmentPlanningProcessSet), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private IEnumerable<EmpReferenceInformation> Getlist(string empid)//TBT
        {
            try
            {
                string _sql = "SELECT * FROM EmpReferenceInformation WHERE EmpSystemID ='" + empid + "'";
                return _sqlRepository.GetModelCollection<EmpReferenceInformation>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetProcessInfo(string PKs)
        {
            try
            {
                GridParameter parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT
	                                EMP.EmployeeName
                                    ,EMP.SystemId EmployeeId
	                                ,Replace(CONVERT(VARCHAR(11), EMP.DOS, 106), ' ', '-') ResignationDate
                                    ,PR.Id position
                                    ,EMP.BudgetCode
                                    ,EMP.SystemID
	                                ,PR.UserName PositionName
	                                ,PR.HandoverDays
	                                ,Replace(CONVERT(VARCHAR(11), RPD.OnBoardDate, 106), ' ', '-') DOSep
	                                ,Replace(CONVERT(VARCHAR(11),(RPD.OnBoardDate - PR.HandoverDays), 106), ' ', '-') OnBoardDate1
                                    ,Replace(CONVERT(VARCHAR(11),(RPD.OnBoardDate - PR.HandoverDays + RPSD.RequiredDays), 106), ' ', '-') FinishedDate
	                                ,RPSD.RecruitmentProcessId
	                                ,RPS.UserName ProcessSetName
	                                ,RPSD.RequiredDays
                                    ,RPSD.Sequence
                                    ,P.StandardName ProcessName
                                    ,P.Id ProcessId
	                                FROM [DBO].[EmployeeInformation] EMP
	                                LEFT OUTER JOIN [MST].[ManpowerBudget]  PMB on PMB.Id=  EMP.BudgetCode
	                                LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
	                                LEFT OUTER JOIN [MST].[RecruitmentPlanningDetail]  RPD ON PMB.Id = RPD.ManpowerBudgetId
	                                LEFT OUTER JOIN [MST].[RecruitmentPlanning]  RP ON  RPD.RecruitmentPlanningId = RP.Id
	                                LEFT OUTER JOIN [MST].[RecruitmentProcessSet] RPS ON RPS.Id = PR.RecruitmentProcessSetId
	                                LEFT OUTER JOIN [MST].[RecruitmentProcessSetDetail] RPSD On RPSD.RecruitmentProcessSetId = RPS.Id
                                    LEFT OUTER JOIN  [HKP].[RecruitmentProcess] P ON P.Id = RPSD.RecruitmentProcessId
		                            where EMP.SystemId In(" + PKs + @") ";

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<RecruitmentPlanningProcessSet> GetMasterlist(string PKs)
        {
            try
            {
                string _sql = "SELECT * FROM MST.RecruitmentPlanningProcessSet WHERE EmployeeId IN (" + PKs + ")";
                return _recruitmentPlanningProcessSetRepository.SqlQuery<RecruitmentPlanningProcessSet>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetRecruitmentPlanningDetailIds(IEnumerable<RecruitmentPlanningProcessSet> entities)
        {
            string empCodes = "''";
            try
            {
                foreach (var item in entities)
                {
                    if (empCodes == "''")
                    {
                        empCodes = "'" + item.EmployeeId + "'";
                    }
                    else
                    {
                        empCodes += ",'" + item.EmployeeId + "'";
                    }
                }
                return empCodes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetProcessInformationlist(string _empId)
        {
            try
            {
                GridParameter parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT
                                    EMP.EmployeeName
                                    ,EMP.SystemId EmployeeId
                                    , EMP.EmployeeCode
                                    ,EMP.PlantId
	                                ,Replace(CONVERT(VARCHAR(11), REG.ApprovedEffectiveDate, 106), ' ', '-') ResignationDate
                                    ,PR.Id position
                                    ,PR.UserName PositionName
                                    ,PR.HandoverDays
	                                ,Replace(CONVERT(VARCHAR(11), RPD.OnBoardDate, 106), ' ', '-') OnBoardDate1
	                                ,RPSD.RecruitmentProcessId
	                                --,RPS.UserName ProcessSetName
                                    ,RPSD.RequiredDays
                                    ,P.StandardName ProcessName
                                    ,P.Sequence
                                    ,RPD.Id RecruitmentPlanningDetailId
                                    FROM[DBO].[EmployeeInformation] EMP
                                    LEFT OUTER JOIN[MST].[ManpowerBudget] PMB on PMB.Id=  EMP.BudgetCode
                                    LEFT OUTER JOIN[TRN].[Resignation] REG on REG.EmployeeId=  EMP.SystemId
                                    LEFT OUTER JOIN[ORG].[Position] PR ON PMB.PositionId=PR.Id
                                    LEFT OUTER JOIN[MST].[RecruitmentPlanningDetail] RPD ON PMB.Id = RPD.ManpowerBudgetId
                                    LEFT OUTER JOIN[MST].[RecruitmentPlanning] RP ON  RPD.RecruitmentPlanningId = RP.Id
                                    --LEFT OUTER JOIN[MST].[RecruitmentProcessSet] RPS ON RPS.Id = PR.RecruitmentProcessSetId
                                    LEFT OUTER JOIN[MST].[RecruitmentProcessSetDetail] RPSD On RPSD.RecruitmentProcessSetId = RPS.Id
                                    LEFT OUTER JOIN[HKP].[RecruitmentProcess] P ON P.Id = RPSD.RecruitmentProcessId WHERE EMP.SystemId IN (" + _empId + ")";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ProcessSetInsert(IEnumerable<RecruitmentPlanningProcessSet> entities)
        {
            List<RecruitmentPlanningProcessSet> dbList = new List<RecruitmentPlanningProcessSet>();
            string pks = string.Empty;
            var flag = false;
            try
            {
                var _pks = "''";
                var from_dblist = GetMasterlist(_pks);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _plantId = string.Empty;

                var _getRecruitmentPlanningDetail_pks = GetRecruitmentPlanningDetailIds(entities);
                DataSet processInfoList = GetProcessInformationlist(_getRecruitmentPlanningDetail_pks);

                foreach (var item in entities)
                {
                    string dt = "";
                    DataView dve = new DataView(processInfoList.Tables[0]);
                    var id = GetPK();
                    var count = 0;
                    if (dve.Count > 0)
                    {
                        for (int i = 0; i < dve.Count; i++)
                        {
                            count++;
                            RecruitmentPlanningProcessSet db = new RecruitmentPlanningProcessSet
                            {
                                Id = id + "-" + count,
                                RecruitmentPlanningDetailId = dve[i]["RecruitmentPlanningDetailId"].ToString(),
                                RecruitmentProcessId = dve[i]["RecruitmentProcessId"].ToString(),
                                EmployeeId = dve[i]["EmployeeId"].ToString(),
                                Sequence = (decimal)(dve[i]["Sequence"]),
                                RequiredDays = (byte)(dve[i]["RequiredDays"])
                            };

                            DateTime OnBoardDate;
                            if (i == 0)
                            {
                                OnBoardDate = Convert.ToDateTime((dve[i]["OnBoardDate1"]).ToString());
                            }
                            else
                            {
                                OnBoardDate = Convert.ToDateTime(dt);
                            }
                            db.TargetDate = OnBoardDate.AddDays(db.RequiredDays);
                            dt = db.TargetDate.ToString();
                            AuditService.AddedLog(db);
                            db.ModelState = ModelState.Added;
                            dbList.Add(db);
                        }
                    }
                }//foreach

                foreach (var item in dbList)
                {
                    AuditService.Log(item);
                    InsertOrUpdateGraph(item);
                    //base.Insert(item);
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel ResignedEmployeeQuery(GridParameter parameters, string companyId, string plantID, bool isControlAdmin, bool isSysAdmin, string employeeId)
        {
            try
            {
                var str = "";
                if (!isControlAdmin && !isSysAdmin)
                {
                    //str = @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
                    //                (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
                    //                        where ResigRecruitPlanningRP='" + employeeId + "')))";

                    str = @" AND emp.BudgetCode IN (SELECT Id from mst.ManpowerBudget WHERE EntityId IN (SELECT entityid FROM [HKP].[ApprovalConfiguration] WHERE ResigRecruitPlanningRP='" + employeeId + "'))";
                }
                else
                {
                    str = @" AND Emp.CompanyId='" + companyId + "'";
                }

                parameters.CmdText = @"SELECT
	                                        EMP.EmployeeName
                                            ,EMP.SystemId EmployeeId
	                                        ,EMP.EmployeeCode
                                            ,EMP.PlantId
                                            ,E.UserName EntityName
                                            ,PMB.Id BudgetCode
	                                        ,Replace(CONVERT(VARCHAR(11), R.ApprovedEffectiveDate, 106), ' ', '-') ResignationDate
                                            ,PR.Id position
	                                        ,PR.UserName PositionName
	                                        ,PR.HandoverDays
	                                        ,Replace(CONVERT(VARCHAR(11), RPD.OnBoardDate, 106), ' ', '-') OnBoardDate1
	                                        --,RPS.UserName ProcessName
                                            ,R.ApprovalStatus
	                                        FROM [DBO].[EmployeeInformation] EMP
	                                        LEFT OUTER JOIN [MST].[ManpowerBudget]  PMB on PMB.Id=  EMP.BudgetCode
	                                        LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                                            LEFT OUTER JOIN [TRN].[Resignation] R ON R.EmployeeId = EMP.SystemId
	                                        LEFT OUTER JOIN [MST].[RecruitmentPlanningDetail]  RPD ON PMB.Id = RPD.ManpowerBudgetId
	                                        LEFT OUTER JOIN [MST].[RecruitmentPlanning]  RP ON  RPD.RecruitmentPlanningId = RP.Id
	                                        --LEFT OUTER JOIN [MST].[RecruitmentProcessSet] RPS ON RPS.Id = PR.RecruitmentProcessSetId
                                            LEFT OUTER JOIN [ORG].[Entity] E ON PMB.EntityId=E.Id
                                            LEFT OUTER JOIN [MST].[RecruitmentPlanningProcessSet] RPPS ON RPPS.RecruitmentPlanningDetailId = RPD.Id
		                                    WHERE R.ApprovalStatus='Approved' and isNull(RPPS.EmployeeId,'')='' and RP.CompanyId='" + companyId + "' and RP.PlantId='" + plantID + "'" + str;

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        //public GridModel ResignedEmployeeQueryByEmpId(GridParameter parameters, string companyId, string plantID,string empId, bool isControlAdmin, bool isSysAdmin, string employeeId)
        //{
        //    try
        //    {
        //        var str = "";
        //        if (!isControlAdmin && !isSysAdmin)
        //        {
        //            str = @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
        //                            (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
        //                                    where ResigRecruitPlanningRP='" + employeeId + "')))";
        //        }
        //        else
        //        {
        //            str = @" AND Emp.CompanyId='" + companyId + "'";
        //        }

        //        parameters.CmdText = @"SELECT
        //                                     EMP.SystemId EmployeeId
        //                                 ,Replace(CONVERT(VARCHAR(11), REG.ApprovedEffectiveDate, 106), ' ', '-') ResignationDate
        //                                 ,Replace(CONVERT(VARCHAR(11), RPD.OnBoardDate, 106), ' ', '-') OnBoardDate1
        //                                 ,RPSD.RecruitmentProcessId
        //                                 ,RPS.UserName ProcessSetName
        //                                 ,RPSD.RequiredDays
        //                                    ,P.StandardName ProcessName
        //                                 FROM [DBO].[EmployeeInformation] EMP
        //                                 LEFT OUTER JOIN [MST].[ManpowerBudget]  PMB on PMB.Id=  EMP.BudgetCode
        //                                 LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
        //                                    LEFT OUTER JOIN [TRN].[Resignation] REG ON REG.EmployeeId = EMP.SystemId
        //                                 LEFT OUTER JOIN [MST].[RecruitmentPlanningDetail]  RPD ON PMB.Id = RPD.ManpowerBudgetId
        //                                 LEFT OUTER JOIN [MST].[RecruitmentPlanning]  RP ON  RPD.RecruitmentPlanningId = RP.Id
        //                                 LEFT OUTER JOIN [MST].[RecruitmentProcessSet] RPS ON RPS.Id = PR.RecruitmentProcessSetId
        //                                 LEFT OUTER JOIN [MST].[RecruitmentProcessSetDetail] RPSD On RPSD.RecruitmentProcessSetId = RPS.Id
        //                                    LEFT OUTER JOIN  [HKP].[RecruitmentProcess] P ON P.Id = RPSD.RecruitmentProcessId
        //                              WHERE REG.ApprovalStatus='Approved' and RP.CompanyId='" + companyId + "' and RP.PlantId='" + plantID + "' and EMP.SystemId='" + empId + "'" + str;

        //        return _sqlRepository.GetGridData(parameters);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}
        public GridModel ResignedEmployeeQueryByEmpId(GridParameter parameters, string companyId, string plantID, string empId, bool isControlAdmin, bool isSysAdmin, string employeeId)
        {
            try
            {
                var str = "";
                if (!isControlAdmin && !isSysAdmin)
                {
                    str = @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
                                    (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
                                            where ResigRecruitPlanningRP='" + employeeId + "')))";
                }
                else
                {
                    str = @" AND Emp.CompanyId='" + companyId + "'";
                }

                parameters.CmdText = @"SELECT
                                         EMP.EmployeeName
                                        ,EMP.SystemId EmployeeId
                                         ,Replace(CONVERT(VARCHAR(11), R.ApprovedEffectiveDate, 106), ' ', '-') ResignationDate
                                         ,PR.UserName PositionName
                                         ,PR.HandoverDays
                                         ,Replace(CONVERT(VARCHAR(11), RPD.OnBoardDate, 106), ' ', '-') DOSep
                                         --,Replace(CONVERT(VARCHAR(11),(RPD.OnBoardDate - PR.HandoverDays + RPSD.RequiredDays), 106), ' ', '-') FinishedDate
                                         ,RPSD.RecruitmentProcessId
                                         --,RPS.UserName ProcessSetName
                                         ,RPSD.RequiredDays
                                         ,P.StandardName ProcessName
                                         FROM [DBO].[EmployeeInformation] EMP
                                         LEFT OUTER JOIN [MST].[ManpowerBudget]  PMB on PMB.Id=  EMP.BudgetCode
                                         LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                                         LEFT OUTER JOIN [TRN].[Resignation] R ON R.EmployeeId = EMP.SystemId
                                         LEFT OUTER JOIN [MST].[RecruitmentPlanningDetail]  RPD ON PMB.Id = RPD.ManpowerBudgetId
                                         LEFT OUTER JOIN [MST].[RecruitmentPlanning]  RP ON  RPD.RecruitmentPlanningId = RP.Id
                                         --LEFT OUTER JOIN [MST].[RecruitmentProcessSet] RPS ON RPS.Id = PR.RecruitmentProcessSetId
                                         LEFT OUTER JOIN [MST].[RecruitmentProcessSetDetail] RPSD On RPSD.RecruitmentProcessSetId = RPS.Id
                                         LEFT OUTER JOIN  [HKP].[RecruitmentProcess] P ON P.Id = RPSD.RecruitmentProcessId
                                         WHERE R.ApprovalStatus='Approved' and RP.CompanyId='" + companyId + "' and RP.PlantId='" + plantID + "' and EMP.SystemId='" + empId + "'" + str;

                return _sqlRepository.GetGridData(parameters);
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