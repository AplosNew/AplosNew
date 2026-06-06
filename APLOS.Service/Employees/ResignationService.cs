#region Using

using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.External;
using Library.Model.Recruitments;
using Library.Model.Securites;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Recruitments;
using Library.Service.Securites;
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
    public class ResignationService : Service<Resignation>, IResignationService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<Resignation> _resignationRepository;
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IRecruitmentPlanningService _recruitmentPlanningService;
        private readonly IRepositoryAsync<RecruitmentPlanningProcessSet> _recruitmentPlanningProcessSetRepository;
        private readonly IUserService _userService;

        public ResignationService(
            IRepositoryAsync<Resignation> resignationRepository
            , IRepositoryAsync<RecruitmentPlanningProcessSet> recruitmentPlanningProcessSetRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IEmployeeInformationService employeeInformationService
            , IRecruitmentPlanningService recruitmentPlanningService
            , IUserService userService
            ) : base(resignationRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _resignationRepository = resignationRepository;
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _employeeInformationService = employeeInformationService;
            _recruitmentPlanningService = recruitmentPlanningService;
            _recruitmentPlanningProcessSetRepository = recruitmentPlanningProcessSetRepository;
            _userService = userService;
        }

        #endregion Constructor

        private string GetPK()
        {
            return "R" + GetAutoNumber(nameof(Resignation), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<EmployeeInformation> GetEmployeeInformationlist(string PKs)
        {
            try
            {
                string _sql = "SELECT * FROM EmployeeInformation WHERE SystemId IN (" + PKs + ")";
                return _resignationRepository.SqlQuery<EmployeeInformation>(_sql).AsEnumerable();

                // return _preRecruitmentEmployeeRepository.SqlQuery<PreRecruitmentEmployee>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<EmployeeInformation> GetResignedEmployeeList()
        {
            try
            {
                var sql = @"SELECT * FROM dbo.EmployeeInformation AS EMP
							WHERE EMP.EmployeeStatus <> 'Separated' AND EMP.SystemId IN (SELECT EmployeeId from [TRN].[Resignation]
							WHERE ApprovalStatus='" + EnumResignationApprovalStatus.Approved + "' AND IsProcessed= 0   " +
                            "AND ApprovedEffectiveDate <= '" + DateTime.Now.ToDbDate() + "')";
                return _resignationRepository.SqlQuery<EmployeeInformation>(sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetCboSeparationType(string PlantId)
        {
            try
            {
                var sql = @"SELECT Id,UserName  FROM HKP.SeparationType where PlantId='" + PlantId + @"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }
        private DataTable GetResignedEmployeeInfo()
        {
            try
            {
                string _sql = @"SELECT MAX(ApprovedEffectiveDate) ApprovedEffectiveDate, EmployeeId from[TRN].[Resignation]
                                WHERE ApprovalStatus = '" + EnumResignationApprovalStatus.Approved + @"' AND IsProcessed = 0
								GROUP BY  EmployeeId,ApprovedEffectiveDate
								HAVING ApprovedEffectiveDate<='" + DateTime.Now.ToString("dd-MMM-yyyy") + @"'";
                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateInActiveUser(IEnumerable<EmployeeInformation> empList, out List<User> inActiveUser)
        {
            var pks = string.Empty;
            try
            {
                var empids = GetEmpSysIds(empList);
                inActiveUser = GetUserlistByEmployeeIds(empids).ToList();
                foreach (var db in inActiveUser)
                {
                    db.Active = false;
                    db.ModelState = ModelState.Modified;
                    AuditService.Log(db);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateResignedEmployees()
        {
            AttendanceProcessAplos ob = new AttendanceProcessAplos();
            var pks = string.Empty;
            var flag = false;
            DataTable dsEDate = null;
            DataView dvEDate = null;
            try
            {

                var ResignedEmpList = GetResignedEmployeeList();
                dsEDate = GetResignedEmployeeInfo();
                int nextMonth = 0;
                int MonthNo = 0;
                int year = 0;
                using (dvEDate = new DataView(dsEDate))
                {
                    foreach (var db in ResignedEmpList)
                    {
                        dvEDate.RowFilter = "EmployeeId='" + db.SystemId + "'";
                        if (dvEDate.Count > 0 && !string.IsNullOrEmpty(dvEDate[0]["ApprovedEffectiveDate"].ToString()))
                        {
                            ///by monir 191128
                            ////DataTable dtEmpAttdnLockInfo = (_sqlRepository.GetDataTable("select * from PlantWiseAttendanceLock where LockedDate between '" + dvEDate[0]["ApprovedEffectiveDate"].ToString() + "' and '" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' and IsActive = 1"));

                            ////if (dtEmpAttdnLockInfo.Rows.Count > 0)
                            ////{
                            ////    for (int i = 0; i < dtEmpAttdnLockInfo.Rows.Count; i++)
                            ////    {
                            ////        string sID = string.Empty;
                            ////        bplib.clsGenID objGenID = new bplib.clsGenID();
                            ////        objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "ExceptionEmployeeAttendanceUnlock", out sID);

                            ////        string strSql = @"INSERT INTO [dbo].[ExceptionEmployeeAttendanceUnlock]
                            ////        ([Id],[EmpSystemId],[PlantId],[IsActive],[WorkDate],[AddedBy],[AddedDate],[AddedFromIP],[UpdatedBy],[UpdatedDate],[UpdatedFromIP])
                            ////         VALUES
                            ////        ('""R" + sID + "','" + db.SystemId + @"' ,'" + db.PlantID + @"' ,1," + dtEmpAttdnLockInfo.Rows[i]["LockedDate"] + @"'' , "",'" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' ,"","" ,"","")";
                            ////    }
                            ////}


                            // ob.LockValidation(db.PlantID, dvEDate[0]["ApprovedEffectiveDate"].ToString(), DateTime.Now.ToString("dd-MMM-yyyy"), db.SystemId);

                            db.DOS = Convert.ToDateTime(dvEDate[0]["ApprovedEffectiveDate"].ToString());
                            nextMonth = Convert.ToDateTime(dvEDate[0]["ApprovedEffectiveDate"].ToString()).AddMonths(1).Month; //db.DOS.AddMonths(1);
                            MonthNo = Convert.ToDateTime(dvEDate[0]["ApprovedEffectiveDate"].ToString()).Month; //db.DOS.AddMonths(1);
                            year = Convert.ToDateTime(dvEDate[0]["ApprovedEffectiveDate"].ToString()).Year;
                            db.EmployeeStatus = "Separated";
                            db.DOSBy = "Schedule";
                            db.DOSDate = DateTime.Now;
                            db.DateUpdated = DateTime.Now;
                            db.ModelState = ModelState.Modified;
                            _employeeInformationService.InsertOrUpdateGraph(db);
                            var dosDate = Convert.ToDateTime(db.DOS).ToString("dd-MMM-yyyy");

                            string strDeleteAttdnProcessManualEntryRemarksData = @"delete from ManualEntryRemarks where RowId IN ( SELECT RowId FROM AttdnProcessData WHERE EmpSystemID IN (SELECT EmployeeId from [TRN].[Resignation]
                                                  WHERE EmployeeId = '" + db.SystemId + "' AND ApprovalStatus = '" + EnumResignationApprovalStatus.Approved + "' AND ApprovedEffectiveDate <= '" + Convert.ToDateTime(dvEDate[0]["ApprovedEffectiveDate"].ToString()).ToString("dd-MMM-yyyy") + "' ) " +
                                               "AND workDate > '" + dosDate + "' AND EmpSystemID = '" + db.SystemId + "')";


                            string strDeleteAttdnProcessData = @"DELETE FROM AttdnProcessData WHERE EmpSystemID IN (SELECT EmployeeId from [TRN].[Resignation]
                                                  WHERE EmployeeId = '" + db.SystemId + "' AND ApprovalStatus = '" + EnumResignationApprovalStatus.Approved + "' AND ApprovedEffectiveDate <= '" + Convert.ToDateTime(dvEDate[0]["ApprovedEffectiveDate"].ToString()).ToString("dd-MMM-yyyy") + "' ) " +
                                                  "AND workDate > '" + dosDate + "' AND EmpSystemID = '" + db.SystemId + "'";

                            string strDeleteAttdnProcessFinalData = @"DELETE FROM AttdnProcessFinalData WHERE EmpSystemID IN (SELECT EmployeeId from [TRN].[Resignation]
							                                WHERE EmployeeId = '" + db.SystemId + "' AND ApprovalStatus='" + EnumResignationApprovalStatus.Approved + "' AND ApprovedEffectiveDate <= '" + Convert.ToDateTime(dvEDate[0]["ApprovedEffectiveDate"].ToString()).ToString("dd-MMM-yyyy") + "')  " +
                                       "AND workDate > '" + dosDate + "'AND EmpSystemID ='" + db.SystemId + "'";
                            string strDeleteAttdnMonthlySummary = @"DELETE  FROM AttdnDataMonthlySummary WHERE (YearNo >" + year + @" or (YearNo =" + year + @" and MonthNo  >=" + nextMonth + @")) AND EmpSystemID ='" + db.SystemId + "'";
                            string strUpdateResignedEmpData = @"UPDATE [TRN].[Resignation] SET IsProcessed= 1 WHERE EmployeeId='" + db.SystemId + "' AND IsProcessed= 0  ";
                            string sqlDeleteExtraAbsentism = @"DELETE FROM SCS.WeeklyAbsentismAssignment WHERE EmpSystemID = '" + db.SystemId + "' AND WorkingDate > '" + dvEDate[0]["ApprovedEffectiveDate"].ToString() + "' ";
                            string sqlUpdateUser = @"UPDATE [SEC].[User] SET Active = 0 WHERE EmployeeId = '" + db.SystemId + "'";

                            string _finalOT = @"DELETE  FROM FinalOT WHERE WorkDate > '" + Convert.ToDateTime(dvEDate[0]["ApprovedEffectiveDate"].ToString()).ToString("dd-MMM-yyyy") + "' AND EmpSystemID ='" + db.SystemId + "'";

                            ConnectionManager.clsConnectionManager conManager = new ConnectionManager.clsConnectionManager(600);
                            conManager.BeginTransaction();

                            conManager.executeQuery(strUpdateResignedEmpData);
                            conManager.executeQuery(strDeleteAttdnProcessManualEntryRemarksData);
                            conManager.executeQuery(strDeleteAttdnProcessData);
                            conManager.executeQuery(strDeleteAttdnProcessFinalData);
                            conManager.executeQuery(strDeleteAttdnMonthlySummary);
                            conManager.executeQuery(sqlDeleteExtraAbsentism);
                            conManager.executeQuery(sqlUpdateUser);
                            conManager.executeQuery(_finalOT);

                            string _SalaryProceAttdnData = @"DELETE  FROM SalaryProceAttdnData where EmpSystemId ='" + db.SystemId + @"' and SlrProcMstSystemID in (select SystemID from SalaryProcMaster where YearNo >" + year + @" or (YearNo =" + year + @" and MonthNo  >=" + MonthNo + @")) ";
                            string _SalaryProcessLogDetail = @"DELETE  FROM SalaryProcessLogDetail where EmpSystemId ='" + db.SystemId + @"' and SalaryProcessId in (select SystemID from SalaryProcMaster where YearNo >" + year + @" or (YearNo =" + year + @" and MonthNo  >=" + MonthNo + @")) ";
                            string _SalaryProcChild = @"DELETE  FROM SalaryProcChild where EmpInfoSystemID ='" + db.SystemId + @"' and SlrProcMstSystemID in (select SystemID from SalaryProcMaster where YearNo >" + year + @" or (YearNo =" + year + @" and MonthNo  >=" + MonthNo + @")) ";

                            conManager.executeQuery(_SalaryProceAttdnData);
                            conManager.executeQuery(_SalaryProcessLogDetail);
                            conManager.executeQuery(_SalaryProcChild);
                            conManager.CommitTransaction();
                        }
                    }
                    UpdateInActiveUser(ResignedEmpList, out List<User> inActiveUser);
                    foreach (var item in inActiveUser)
                    {
                        _userService.InsertOrUpdateGraph(item);
                    }
                    _unitOfWork.BeginTransaction();

                    flag = true;
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
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

        private DataTable GetEmployeeSysId(string PKs)
        {
            try
            {
                string sql = @"select
		                            PR.HandoverDays
                                    ,EMP.BudgetCode
                                    ,EMP.SystemID
		                            FROM dbo.EmployeeInformation AS EMP
		                            LEFT OUTER JOIN [MST].[ManpowerBudget]  PMB on PMB.id=emp.BudgetCode
		                            LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
		                            LEFT OUTER JOIN [MST].[RecruitmentPlanningDetail] RTP ON PMB.Id=RTP.ManpowerBudgetId
		                            where EMP.SystemId In(" + PKs + @") ";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetEmpIds(IEnumerable<Resignation> entities)
        {
            string empCodes = "''";
            try
            {
                foreach (var item in entities)
                {
                    if (item.ApprovalStatus == "Approved")
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
                }
                return empCodes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string GetEmpSysIds(IEnumerable<EmployeeInformation> entities)
        {
            string empCodes = "''";
            try
            {
                foreach (var item in entities)
                {
                    if (empCodes == "''")
                    {
                        empCodes = "'" + item.SystemId + "'";
                    }
                    else
                    {
                        empCodes += ",'" + item.SystemId + "'";
                    }
                }
                return empCodes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Dictionary<string, object> GetFile(string Id)
        {
            try
            {
                var sql = @"Select AttachLetter From [trn].[Resignation]  Where ID='" + Id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private string OutMaster(Resignation from_ui)
        {
            string ID = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;


                string sql = "SELECT * FROM [trn].[Resignation] WHERE EmployeeId='" + from_ui.EmployeeId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                //from_db = Find(from_ui.Id);

                DataView dv = new DataView(dsMaster.Tables[0]);
                dv.RowFilter = "Id='" + from_ui.Id + @"'";
                if (dv.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    ID = GetPK();
                    dr["Id"] = GetPK();
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = from_ui.PlantId;
                    dr["EmployeeId"] = from_ui.EmployeeId;
                    dr["SeparationTypeId"] = from_ui.SeparationTypeId;
                    dr["ResignationDate"] = from_ui.ResignationDate;
                    dr["Reason"] = from_ui.Reason;
                    dr["AttachLetter"] = from_ui.AttachLetter;
                    dr["EffectiveDate"] = from_ui.EffectiveDate;
                    if (from_ui.ApprovedEffectiveDate != null)
                    {
                        dr["ApprovedEffectiveDate"] = from_ui.ApprovedEffectiveDate;
                    }

                    dr["Remarks"] = from_ui.Remarks;
                    dr["ApprovalStatus"] = "Pending";
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr["DisciplinaryActionDetailsId"] = from_ui.DisciplinaryActionDetailsId;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else if (dv[0]["ApprovalStatus"].ToString() == "Rejected")
                {
                    string regDate = from_ui.ResignationDate.ToString();

                    DataSet ds = GetEmployee(from_ui.EmployeeId, regDate);
                    DataSet dsRegDate = GetMaxResignationDate(from_ui.EmployeeId);

                    if (ds.Tables[0].Rows.Count > 0 && !string.IsNullOrEmpty(dsRegDate.Tables[0].Rows[0]["ResignationDate"].ToString()))
                    {
                        throw new Exception("Current resignation date must be greater then previous resignation date");
                    }
                    if (dsRegDate.Tables[0].Rows.Count > 0 && !string.IsNullOrEmpty(dsRegDate.Tables[0].Rows[0]["ResignationDate"].ToString()))
                    {
                        DateTime dt = Convert.ToDateTime(dsRegDate.Tables[0].Rows[0]["ResignationDate"].ToString());
                        if (from_ui.ResignationDate < dt)
                        {
                            string rDate = dt.ToString("dd-MMM-yyyy");
                            throw new Exception("Current resignation date must be greater then previous resignation date [" + rDate + "]");
                        }
                    }
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    ID = GetPK();
                    dr["Id"] = GetPK();
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = from_ui.PlantId;
                    dr["EmployeeId"] = from_ui.EmployeeId;
                    dr["ResignationDate"] = from_ui.ResignationDate;
                    dr["Reason"] = from_ui.Reason;
                    dr["SeparationTypeId"] = from_ui.SeparationTypeId;
                    dr["AttachLetter"] = from_ui.AttachLetter;
                    dr["EffectiveDate"] = from_ui.EffectiveDate;
                    //dr["ApprovedEffectiveDate"] = from_ui.ApprovedEffectiveDate;
                    if (from_ui.ApprovedEffectiveDate != null)
                    {
                        dr["ApprovedEffectiveDate"] = from_ui.ApprovedEffectiveDate;
                    }
                    dr["Remarks"] = from_ui.Remarks;
                    dr["ApprovalStatus"] = "Pending";

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["DisciplinaryActionDetailsId"] = from_ui.DisciplinaryActionDetailsId;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {

                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    ID = dr["Id"].ToString();
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = from_ui.PlantId;
                    dr["EmployeeId"] = from_ui.EmployeeId;
                    dr["ResignationDate"] = from_ui.ResignationDate;
                    dr["Reason"] = from_ui.Reason;
                    dr["AttachLetter"] = from_ui.AttachLetter;
                    dr["EffectiveDate"] = from_ui.EffectiveDate;
                    //dr["ApprovedEffectiveDate"] = from_ui.ApprovedEffectiveDate;
                    if (from_ui.ApprovedEffectiveDate != null)
                    {
                        dr["ApprovedEffectiveDate"] = from_ui.ApprovedEffectiveDate;
                    }
                    dr["Remarks"] = from_ui.Remarks;
                    dr["ApprovalStatus"] = "Pending";
                    dr["SeparationTypeId"] = from_ui.SeparationTypeId;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["DisciplinaryActionDetailsId"] = from_ui.DisciplinaryActionDetailsId;
                    dr.EndEdit();
                }





                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);




            }
            catch (Exception)
            {
                throw;
            }




            return ID;



        }


        private void xOutMaster(Resignation from_ui, out Resignation from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;

                string LBSID = string.Empty;
                string sql = "SELECT * FROM [trn].[Resignation] WHERE EmployeeId='" + from_ui.EmployeeId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                //from_db = Find(from_ui.Id);

                DataView dv = new DataView(dsMaster.Tables[0]);
                dv.RowFilter = "Id='" + from_ui.Id + @"'";
                if (dv.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = GetPK();
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = from_ui.PlantId;
                    dr["EmployeeId"] = from_ui.EmployeeId;
                    dr["ResignationDate"] = from_ui.ResignationDate;
                    dr["Reason"] = from_ui.Reason;
                    dr["AttachLetter"] = from_ui.AttachLetter;
                    dr["EffectiveDate"] = from_ui.EffectiveDate;
                    dr["ApprovedEffectiveDate"] = from_ui.ApprovedEffectiveDate;
                    dr["Remarks"] = from_ui.Remarks;
                    dr["ApprovalStatus"] = "Pending";
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                }
                else if (dv[0]["ApprovalStatus"].ToString() == "Rejected")
                {
                    string regDate = from_ui.ResignationDate.ToString();

                    DataSet ds = GetEmployee(from_ui.EmployeeId, regDate);
                    DataSet dsRegDate = GetMaxResignationDate(from_ui.EmployeeId);

                    if (ds.Tables[0].Rows.Count > 0 && !string.IsNullOrEmpty(dsRegDate.Tables[0].Rows[0]["ResignationDate"].ToString()))
                    {
                        throw new Exception("Current resignation date must be greater then previous resignation date");
                    }
                    if (dsRegDate.Tables[0].Rows.Count > 0 && !string.IsNullOrEmpty(dsRegDate.Tables[0].Rows[0]["ResignationDate"].ToString()))
                    {
                        DateTime dt = Convert.ToDateTime(dsRegDate.Tables[0].Rows[0]["ResignationDate"].ToString());
                        if (from_ui.ResignationDate < dt)
                        {
                            string rDate = dt.ToString("dd-MMM-yyyy");
                            throw new Exception("Current resignation date must be greater then previous resignation date [" + rDate + "]");
                        }
                    }
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = GetPK();
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = from_ui.PlantId;
                    dr["EmployeeId"] = from_ui.EmployeeId;
                    dr["ResignationDate"] = from_ui.ResignationDate;
                    dr["Reason"] = from_ui.Reason;
                    dr["AttachLetter"] = from_ui.AttachLetter;
                    dr["EffectiveDate"] = from_ui.EffectiveDate;
                    dr["ApprovedEffectiveDate"] = from_ui.ApprovedEffectiveDate;
                    dr["Remarks"] = from_ui.Remarks;
                    dr["ApprovalStatus"] = "Pending";

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                }
                else
                {

                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = from_ui.PlantId;
                    dr["EmployeeId"] = from_ui.EmployeeId;
                    dr["ResignationDate"] = from_ui.ResignationDate;
                    dr["Reason"] = from_ui.Reason;
                    dr["AttachLetter"] = from_ui.AttachLetter;
                    dr["EffectiveDate"] = from_ui.EffectiveDate;
                    dr["ApprovedEffectiveDate"] = from_ui.ApprovedEffectiveDate;
                    dr["Remarks"] = from_ui.Remarks;
                    dr["ApprovalStatus"] = "Pending";

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }








                if (from_db == null || from_db.Id == null || from_db.Id == "")
                {
                    var emp = Query(t => t.EmployeeId == from_ui.EmployeeId).Select().FirstOrDefault();



                    string regDate = from_ui.ResignationDate.ToString();
                    from_db = new Resignation
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(from_db);
                    from_db.Id = GetPK();
                    from_db.CompanyGroupId = identity.CompanyGroupId;
                    from_db.CompanyId = identity.CompanyId;
                    from_db.PlantId = from_ui.PlantId;
                    from_db.EmployeeId = from_ui.EmployeeId;
                    from_db.ResignationDate = from_ui.ResignationDate;
                    from_db.Reason = from_ui.Reason;
                    from_db.AttachLetter = from_ui.AttachLetter;
                    from_db.EffectiveDate = from_ui.EffectiveDate;
                    from_db.ApprovedEffectiveDate = from_ui.ApprovedEffectiveDate;
                    from_db.Remarks = from_ui.Remarks;
                    from_db.ApprovalStatus = "Pending";
                }
                //else if (from_db.Id != null && from_db.ApprovalStatus == "Rejected")
                //{
                //    string regDate = from_ui.ResignationDate.ToString();

                //    DataSet ds = GetEmployee(from_ui.EmployeeId, regDate);
                //    DataSet dsRegDate = GetMaxResignationDate(from_ui.EmployeeId);

                //    if (ds.Tables[0].Rows.Count > 0 && !string.IsNullOrEmpty(dsRegDate.Tables[0].Rows[0]["ResignationDate"].ToString()))
                //    {
                //        throw new Exception("Current resignation date must be greater then previous resignation date");
                //    }
                //    if (dsRegDate.Tables[0].Rows.Count > 0 && !string.IsNullOrEmpty(dsRegDate.Tables[0].Rows[0]["ResignationDate"].ToString()))
                //    {
                //        DateTime dt = Convert.ToDateTime(dsRegDate.Tables[0].Rows[0]["ResignationDate"].ToString());
                //        if (from_ui.ResignationDate < dt)
                //        {
                //            string rDate = dt.ToString("dd-MMM-yyyy");
                //            throw new Exception("Current resignation date must be greater then previous resignation date [" + rDate + "]");
                //        }
                //    }
                //    from_db = new Resignation
                //    {
                //        ModelState = ModelState.Added
                //    };
                //    AuditService.Log(from_db);
                //    from_db.Id = GetPK();
                //    from_db.CompanyGroupId = identity.CompanyGroupId;
                //    from_db.CompanyId = identity.CompanyId;
                //    from_db.PlantId = from_ui.PlantId;
                //    from_db.EmployeeId = from_ui.EmployeeId;
                //    from_db.ResignationDate = from_ui.ResignationDate;
                //    from_db.Reason = from_ui.Reason;
                //    from_db.AttachLetter = from_ui.AttachLetter;
                //    from_db.EffectiveDate = from_ui.EffectiveDate;
                //    from_db.ApprovedEffectiveDate = from_ui.ApprovedEffectiveDate;
                //    from_db.Remarks = from_ui.Remarks;
                //    from_db.ApprovalStatus = "Pending";
                //}
                //else
                //{
                //    from_db.ModelState = ModelState.Modified;
                //    AuditService.Log(from_db);

                //    from_db.CompanyGroupId = identity.CompanyGroupId;
                //    from_db.CompanyId = identity.CompanyId;
                //    from_db.PlantId = from_ui.PlantId;
                //    from_db.EmployeeId = from_ui.EmployeeId;

                //    from_db.ResignationDate = from_ui.ResignationDate;
                //    from_db.Reason = from_ui.Reason;
                //    from_db.AttachLetter = from_ui.AttachLetter;
                //    from_db.EffectiveDate = from_ui.EffectiveDate;
                //    from_db.ApprovedEffectiveDate = from_ui.ApprovedEffectiveDate;
                //    from_db.Remarks = from_ui.Remarks;
                //    from_db.ApprovalStatus = "Pending";
                //    UpdateGraph(from_db);
                //}
            }
            catch (Exception)
            {
                throw;
            }







        }

        public void Save(Resignation ui, out string masterid)
        {
            //Resignation localMaster = null;
            //masterid = string.Empty;

            //var flag = false;
            try
            {
                //OutMaster(ui);


                masterid = OutMaster(ui);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //if (flag)
                //    _unitOfWork.Rollback();
            }
        }

        public void Update(Resignation from_ui)
        {
            string ID = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;


                string sql = "SELECT * FROM [trn].[Resignation] WHERE Id='" + from_ui.Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                //from_db = Find(from_ui.Id);

                DataView dv = new DataView(dsMaster.Tables[0]);
                dv.RowFilter = "Id='" + from_ui.Id + @"'";
                if (dv.Count > 0)
                {
                   
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    ID = dr["Id"].ToString();
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = from_ui.PlantId;
                    dr["EmployeeId"] = from_ui.EmployeeId;
                    dr["ResignationDate"] = from_ui.ResignationDate;
                    dr["Reason"] = from_ui.Reason;
                    dr["AttachLetter"] = from_ui.AttachLetter;
                    dr["EffectiveDate"] = from_ui.EffectiveDate;
                    //dr["ApprovedEffectiveDate"] = from_ui.ApprovedEffectiveDate;
                    if (from_ui.ApprovedEffectiveDate != null)
                    {
                        dr["ApprovedEffectiveDate"] = from_ui.ApprovedEffectiveDate;
                    }
                    dr["Remarks"] = from_ui.Remarks;
                    dr["ApprovalStatus"] = "Pending";
                    dr["SeparationTypeId"] = from_ui.SeparationTypeId;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["DisciplinaryActionDetailsId"] = from_ui.DisciplinaryActionDetailsId;
                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<Resignation> GetMasterlist(string PKs)
        {
            try
            {
                string _sql = "SELECT * FROM trn.Resignation WHERE Id IN (" + PKs + ")";
                return _resignationRepository.SqlQuery<Resignation>(_sql).AsEnumerable();
                // return _preRecruitmentEmployeeRepository.SqlQuery<PreRecruitmentEmployee>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<User> GetUserlistByEmployeeIds(string PKs)
        {
            try
            {
                string _sql = "SELECT * FROM [SEC].[User] WHERE EmployeeId IN (" + PKs + ") and Active = 1";
                return _resignationRepository.SqlQuery<User>(_sql).AsEnumerable();
                // return _preRecruitmentEmployeeRepository.SqlQuery<PreRecruitmentEmployee>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetResignationPKs(IEnumerable<Resignation> entities)
        {
            string empCodes = "''";
            try
            {
                foreach (var item in entities)
                {
                    if (empCodes == "''")
                    {
                        empCodes = "'" + item.Id + "'";
                    }
                    else
                    {
                        empCodes += ",'" + item.Id + "'";
                    }
                }
                return empCodes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetEmployee(string _empId, string ResignationDate)
        {
            try
            {
                GridParameter parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT Id from [TRN].[Resignation] WHERE EmployeeId = '" + _empId + "' and ResignationDate='" + ResignationDate + "'";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetMaxResignationDate(string _empId)
        {
            try
            {
                GridParameter parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"select max(ResignationDate) ResignationDate from [TRN].[Resignation] WHERE EmployeeId = '" + _empId + "'";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetAEFDate(string PKs)
        {
            try
            {
                GridParameter parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"select * from [TRN].[Resignation] where EmployeeId In(" + PKs + @") ";

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InitRecruitmentPlanning(string companyGroupId, string companyId, string plantId, IEnumerable<EmployeeInformation> empList, string sysIds)
        {
            try
            {
                var recruitmentPlanning = new RecruitmentPlanning
                {
                    CompanyGroupId = companyGroupId,
                    CompanyId = companyId,
                    PlantId = plantId,
                    UserName = "PostResignation " + DateTime.Now.ToString("yyMMddHHmmss"),
                    PlanningType = PlanningTypeEnum.Resignation.ToString(),
                    Active = true,
                    Archive = false
                };
                AuditService.AddedLog(recruitmentPlanning);

                var recruitmentPlanningDetailList = new List<RecruitmentPlanningDetail>();
                var dsEmpSysIds = GetEmployeeSysId(sysIds);
                var dsAEFDate = GetAEFDate(sysIds);
                foreach (var db in empList)
                {
                    var handOverDays = 0;
                    using (DataView dve = new DataView(dsEmpSysIds)
                    {
                        RowFilter = "SystemId='" + db.SystemId + "' and BudgetCode='" + db.BudgetCode + "'"
                    })
                    {
                        if (dve.Count > 0)
                            handOverDays = (dve[0]["HandoverDays"].ToString()).ToInt();

                        var recruitmentPlanningDetail = new RecruitmentPlanningDetail
                        {
                            ManpowerBudgetId = db.BudgetCode,
                            RecruitmentGroupId = null,
                            RecruitmentPlanningId = recruitmentPlanning.Id,
                            OnBoardDate = Convert.ToDateTime(db.DOS).AddDays(-handOverDays),
                            Male = (short)(db.GenderID == "Male" ? 1 : 0),
                            Female = (short)(db.GenderID == "Female" ? 1 : 0),
                            TotalManpower = 1,
                            Active = true,
                            Archive = false,
                            AddedBy = recruitmentPlanning.AddedBy,
                            AddedDate = recruitmentPlanning.AddedDate,
                            AddedFromIP = recruitmentPlanning.AddedFromIP
                        };
                        recruitmentPlanningDetailList.Add(recruitmentPlanningDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void InitEmployeeInfo(IEnumerable<Resignation> entities, IEnumerable<EmployeeInformation> empInfoList, out List<EmployeeInformation> savedEmpList)
        {
            var pks = string.Empty;
            try
            {
                savedEmpList = new List<EmployeeInformation>();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                foreach (var db in empInfoList)
                {
                    string empid = db.SystemId;
                    var ui = entities.FirstOrDefault(a => a.EmployeeId == empid);
                    var ep = empInfoList.FirstOrDefault(e => e.EmployeeId == empid);

                    if (ui.ApprovalStatus == "Approved")
                    {
                        db.DOS = ui.ApprovedEffectiveDate;
                    }
                    savedEmpList.Add(db);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void ApprovalUpdate(IEnumerable<Resignation> entities, string name, string ipAddress, string companyGroupId, string companyId)
        {
            string pks = string.Empty;
            var flag = false;
            string _ed = string.Empty;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var _pks = GetResignationPKs(entities);
                var from_dblist = GetMasterlist(_pks);
                string _plantId = string.Empty;

                foreach (var item in entities)
                {
                    var db = from_dblist.FirstOrDefault(a => a.Id == item.Id);
                    if (db != null)
                    {
                        db.ModelState = ModelState.Modified;
                        AuditService.Log(db);
                        db.ApprovedEffectiveDate = item.ApprovedEffectiveDate;
                        db.Remarks = item.Remarks;
                        db.ApprovalStatus = item.ApprovalStatus;
                        db.ApprovedBy = name;
                        db.SpecialFollowUP = item.SpecialFollowUP;
                        db.ApprovedDate = DateTime.Now;
                        db.ApprovedFromIP = ipAddress;
                        UpdateGraph(db);
                    }
                }

                var _emp_pks = GetEmpIds(entities);
                var empInfoList = GetEmployeeInformationlist(_emp_pks);
                foreach (var item in empInfoList)
                {
                    _plantId = item.PlantID;
                    break;
                }

                InitEmployeeInfo(entities, empInfoList, out List<EmployeeInformation> savedEmpList);
                if (empInfoList.Count() > 0)
                {
                    InitRecruitmentPlanning(companyGroupId, companyId, _plantId, savedEmpList, _emp_pks);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                UpdateResignedEmployees();  ////Calling Service func for Update status

                #region Attendance process
                clsAttendance.AttendanceProcessAplos obj = new AttendanceProcessAplos();

                foreach (var item in entities)
                {
                    var db = from_dblist.FirstOrDefault(a => a.Id == item.Id);
                    if (db != null && db.ApprovalStatus == "Approved")
                    {
                        DateTime ed = (DateTime)item.ApprovedEffectiveDate;
                        AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                        obj.SaveTotal(_plantId, ed.ToString("dd-MMM-yyyy"), item.EmployeeId, false);//Main Function for attendace Process
                    }
                }
                #endregion

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
        void GetSeparationType(List<Dictionary<string, string>> entities)
        {
            try
            {
                //EmployeeId
                for (int i = 0; i < entities.Count; i++)
                {
                    if (string.IsNullOrEmpty(entities[i]["SeparationType"]))
                    {
                        throw new Exception("Separation Type [Emp: " + entities[i]["EmployeeCode"] + "] is required...");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void ValidationApprovedSalaryProcess(List<Dictionary<string, string>> entities)
        {
            try
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    if (entities[i]["ApprovalStatus"].ToString().ToUpper() == "APPROVED")
                    {
                        var empid = entities[i]["EmployeeId"].ToString();
                        var _dos = entities[i]["ApprovedEffectiveDate"].ToString();
                        var ds = GetApprovedSalaryInfo(empid, _dos);
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            throw new Exception("Salary has already been approved for [Emp: " + ds.Tables[0].Rows[0]["EmployeeCode"] + "]; so can not be separated.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void UpdateApprovalStatusUpdate(List<Dictionary<string, string>> entities, string name, string ipAddress, string companyGroupId, string companyId)
        {
            string pks = string.Empty;
            var flag = false;
            string _ed = string.Empty;
            try
            {
                string _plantId = string.Empty;
                string strRegId = "''";
                for (int i = 0; i < entities.Count; i++)
                {
                    strRegId += ",'" + entities[i]["Id"].ToString() + "'";
                }

                GetSeparationType(entities);
                ValidationApprovedSalaryProcess(entities);

                DataSet dsResignation = null;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM trn.Resignation WHERE Id IN (" + strRegId + @")", out dsResignation, false, "1");
                clsAttendance.AttendanceProcessAplos obj = new AttendanceProcessAplos();

                for (int i = 0; i < entities.Count; i++)
                {
                    dsResignation.Tables[0].DefaultView.RowFilter = "Id='" + entities[i]["Id"].ToString() + "'";
                    if (dsResignation.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = dsResignation.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        //if(string.IsNullOrEmpty(dr["ApprovedEffectiveDate"].ToString()))
                        //{
                        //    throw new Exception("Approved Effective Date can not be empty for the employee : "+ dr["EmployeeCode"]+":" + dr["EmployeeName"]); 
                        //}

                        dr["ApprovedEffectiveDate"] = entities[i]["ApprovedEffectiveDate"].ToString();
                        dr["SeparationTypeId"] = entities[i]["SeparationType"].ToString();
                        dr["Remarks"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(entities[i]["Remarks"]));
                        dr["ApprovalStatus"] = entities[i]["ApprovalStatus"].ToString();
                        dr["SpecialFollowUp"] = entities[i]["SpecialFollowUp"].ToString();

                        if (entities[i]["ApprovalStatus"].ToString().ToUpper() == "APPROVED")
                        {
                            DeleteSalary(entities[i]["EmployeeId"].ToString(), entities[i]["ApprovedEffectiveDate"].ToString());
                            // obj.SaveTotal(_plantId, entities[i]["ApprovedEffectiveDate"].ToString(), entities[i]["EmployeeId"], false);//Main Function for attendace Process
                        }
                        dr.EndEdit();
                    }
                }


                string strEmps = "''";
                for (int i = 0; i < entities.Count; i++)
                {
                    strEmps += ",'" + entities[i]["EmployeeId"].ToString() + "'";


                }
                DataSet dsEmployee = null;
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM EmployeeInformation WHERE SystemID IN (" + strEmps + @")", out dsEmployee, false, "1");


                /////////////////////////
                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsResignation);
                //info.SaveDataSets(dsResignation, dsEmployee);



                //_unitOfWork.BeginTransaction();
                //flag = true;
                //var _pks = GetResignationPKs(entities);
                //var from_dblist = GetMasterlist(entities.);
                //string _plantId = string.Empty;

                //foreach (var item in entities)
                //{
                //    var db = from_dblist.FirstOrDefault(a => a.Id == item.Id);
                //    if (db != null)
                //    {
                //        db.ModelState = ModelState.Modified;
                //        AuditService.Log(db);
                //        db.ApprovedEffectiveDate = item.ApprovedEffectiveDate;
                //        db.Remarks = item.Remarks;
                //        db.ApprovalStatus = item.ApprovalStatus;
                //        db.ApprovedBy = name;
                //        db.SpecialFollowUP = item.SpecialFollowUP;
                //        db.ApprovedDate = DateTime.Now;
                //        db.ApprovedFromIP = ipAddress;
                //        UpdateGraph(db);
                //    }
                //}

                //var _emp_pks = GetEmpIds(entities);
                //var empInfoList = GetEmployeeInformationlist(_emp_pks);
                //foreach (var item in empInfoList)
                //{
                //    _plantId = item.PlantID;
                //    break;
                //}

                //InitEmployeeInfo(entities, empInfoList, out List<EmployeeInformation> savedEmpList);
                //if (empInfoList.Count() > 0)
                //{
                //    InitRecruitmentPlanning(companyGroupId, companyId, _plantId, savedEmpList, _emp_pks);
                //}
                //_unitOfWork.SaveChanges();
                //flag = false;
                //_unitOfWork.Commit();

                UpdateResignedEmployees();  ////Calling Service func for Update status

                #region Attendance process

                for (int i = 0; i < entities.Count; i++)
                {
                    _plantId = entities[i]["PlantId"].ToString();
                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    obj.SaveTotal(_plantId, entities[i]["ApprovedEffectiveDate"].ToString(), "'" + entities[i]["EmployeeId"] + "'", false, true);
                }

                //foreach (var item in entities)
                //{
                //    var db = from_dblist.FirstOrDefault(a => a.Id == item.Id);

                //}
                #endregion

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
        public void DeleteSalary(string empsid, string dos)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                string _sql_child = @"delete FROM SalaryProcChild WHERE SlrProcMstSystemID IN 
                                                                                    (SELECT SystemID FROM SalaryProcMaster
                                                                                  WHERE MonthNo = month('" + dos + @"') AND YearNo = year('" + dos + @"'))
                                                                                  AND IsApproved = 0 AND IsDisbursed = 0
                                                                                  AND EmpInfoSystemID = '" + empsid + @"' ";
                string _sql_att = @" delete from SalaryProceAttdnData 
                                            where EmpSystemID='" + empsid + @"' 
                                            and SlrProcMstSystemID in (SELECT SystemID FROM SalaryProcMaster  WHERE MonthNo = month('" + dos + @"') AND YearNo = year('" + dos + @"'))
                                            and MonthNo = month('" + dos + @"') and YearNo =  year('" + dos + @"')";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(_sql_child, true, "1");
                objCon.ExecuteNonQueryWrapper(_sql_att, true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public IEnumerable<object> ResignationHistoryByID(string empID)
        {
            try
            {
                var _sql = @"SELECT  Id
                                    ,Replace(CONVERT(VARCHAR(11), ResignationDate, 106), ' ', '-') ResignationDate
		                            ,Replace(CONVERT(VARCHAR(11), EffectiveDate, 106), ' ', '-') EffectiveDate
		                            ,Replace(CONVERT(VARCHAR(11), ApprovedEffectiveDate, 106), ' ', '-') ApprovedEffectiveDate
		                            ,ApprovalStatus
		                            ,Remarks
                                    FROM [TRN].[Resignation]
                                    where EmployeeId ='" + empID + "'";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel ActiveEmpListByPlantId(GridParameter parameters, string plantID, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId)
        {
            try
            {
                var str = "";
                if (!isControlAdmin && !isSysAdmin)
                {
                    //str = @" and isnull(r.EmployeeId,'') in
                    //                        (select systemid from EmployeeInformation e where e.BudgetCode in
                    //                            (select Id from mst.ManpowerBudget where EntityId in
                    //                                (select entityid from [HKP].[ApprovalConfiguration]
                    //                                  where ResignationApply =
                    //                                  (select EmployeeId from [SEC].[User] where UserId='" + employeeId + @"')
                    //                                )
                    //                            )
                    //                        ) ";

                    str = @" AND emp.BudgetCode IN (SELECT Id from mst.ManpowerBudget WHERE EntityId IN (SELECT entityid FROM [HKP].[ApprovalConfiguration] WHERE ResignationApply='" + employeeId + "'))";
                }

                parameters.CmdText = @"SELECT   '' Id
                                                ,EMP.SystemId as EmployeeId
                                                ,EMP.EmployeeName
                                                ,EMP.EmployeeCode
		                                        ,Replace(CONVERT(VARCHAR(11), EMP.DOB, 106), ' ', '-') DOB
                                                ,Replace(CONVERT(VARCHAR(11), emp.DOJ, 106), ' ', '-') DOJ
	                                            ,Replace(CONVERT(VARCHAR(11), emp.DOC, 106), ' ', '-') DOC
                                                ,emp.EmployeeCurrentStatus
		                                        ,E.UserName as Entity
                                                ,PR.UserName position
                                                ,DEPT.UserName Department
		                                        ,D.UserName Designation
		                                        ,DEG.UserName GivenDesignation
	                                            ,Replace(CONVERT(VARCHAR(11), r.ResignationDate, 106), ' ', '-') ResignationDate
                                                ,Replace(CONVERT(VARCHAR(11), r.EffectiveDate, 106), ' ', '-') EffectiveDate
	                                            --,Replace(CONVERT(VARCHAR(11), r.ApprovedEffectiveDate, 106), ' ', '-') ApprovedEffectiveDate
                                                ,ApprovalStatus
	                                            ,Reason
                                                ,EMP.BudgetCode [Budget Id]

		                                        ,EMP.EmpPicPath [Picture]
	                                            ,AttachLetter
	                                            --,emp.EmployeeCode
                                                ,EMP.PlantId
	                                            ,c.UserName as EmployeeCategory
                                                ---,HR.IsPastResignationAllowed
                                                ---,HR.PastResignationDaysAllowed
                                                ,IsPastResignationAllowed=1
                                                ,PastResignationDaysAllowed=1
		                                        FROM dbo.EmployeeInformation AS EMP
		                                        LEFT OUTER JOIN [MST].[ManpowerBudget]  PMB on PMB.id=emp.BudgetCode
		                                        LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
												LEFT OUTER JOIN
												( SELECT DISTINCT * from
													(SELECT reg.* from [TRN].[Resignation] reg
														INNER JOIN
														(
															SELECT MAX(ApprovedEffectiveDate) ApprovedEffectiveDate,max(addeddate) addeddate,EmployeeId from [TRN].[Resignation]
															
															 group by EmployeeId
														)
														D on D.EmployeeId = reg.EmployeeId and D.ApprovedEffectiveDate = reg.ApprovedEffectiveDate
														AND d.addeddate=reg.AddedDate
														) xx
												) R ON R.EmployeeId=EMP.SystemId
                                                LEFT OUTER JOIN [DBO].[PlantWiseHRMSSetting] HR on hr.PlantID = emp.PlantId
		                                        LEFT OUTER JOIN [HKP].[Designation] DEG ON EMP.GivenDesignationId=DEG.Id
		                                        LEFT OUTER JOIN [HKP].[Designation] D ON PR.DesignationId=D.Id
		                                        LEFT OUTER JOIN [ORG].[Department] DEPT ON PR.DepartmentId=DEPT.Id
		                                        LEFT OUTER JOIN [HKP].[EmployeeCategory] C ON C.id=EMP.EmployeeCategorySystemID
		                                        LEFT OUTER JOIN [ORG].[Entity] E ON E.Id=PMB.entityid
		                                        where e.UserName <> ' ' and EMP.PlantId ='" + plantID + @"' 
                                                   	AND EMP.SystemId NOT IN ( 
												select EmployeeId from TRN.Resignation r where ISNULL(r.id,'') <> ''
											    and ISNULL(r.ApprovalStatus,'') = 'Pending' ) AND EMP.EmployeeStatus <> 'separated'  " +
                                                "--and (IsProcessed=1 or Isnull(ApprovalStatus,'')<>'Pending' )" +
                                                "--and (IsProcessed=1 or Isnull(ApprovalStatus,'')<>'Hold') " +
                                                "--and EMP.EmployeeName <> ''" +
                                                "--and (IsProcessed=1 or Isnull(ApprovalStatus,'')<>'Approved')  and Isnull(ApprovalStatus,'')<>'' " +

                                                "" + str;

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel PendingResignationQueryByPlantId(GridParameter parameters, string plantID, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId)
        {
            try
            {
                var str = "";
                if (!isControlAdmin && !isSysAdmin)
                {
                    str = @" AND emp.BudgetCode IN (SELECT Id from mst.ManpowerBudget WHERE EntityId IN (SELECT entityid FROM [HKP].[ApprovalConfiguration] WHERE ResignationApply='" + employeeId + "'))";
                }

                parameters.CmdText = @"SELECT   R.Id
                                                ,EMP.SystemId as EmployeeId
                                                ,EMP.EmployeeName
                                                ,EMP.EmployeeCode
		                                        ,Replace(CONVERT(VARCHAR(11), EMP.DOB, 106), ' ', '-') DOB
                                                ,Replace(CONVERT(VARCHAR(11), emp.DOJ, 106), ' ', '-') DOJ
	                                            ,Replace(CONVERT(VARCHAR(11), emp.DOC, 106), ' ', '-') DOC
		                                        ,E.UserName as Entity
                                                ,DEPT.UserName Department
		                                        ,D.UserName Designation
		                                        ,DEG.UserName GivenDesignation
	                                            ,Replace(CONVERT(VARCHAR(11), r.ResignationDate, 106), ' ', '-') ResignationDate
                                                ,Replace(CONVERT(VARCHAR(11), r.EffectiveDate, 106), ' ', '-') EffectiveDate
	                                            --,Replace(CONVERT(VARCHAR(11), r.ApprovedEffectiveDate, 106), ' ', '-') ApprovedEffectiveDate
                                                ,ApprovalStatus
	                                            ,Reason
                                                ,EMP.BudgetCode [Budget Id]
                                                ,EMP.PositionID position
		                                        ,EMP.EmpPicPath [Picture]
	                                            ,AttachLetter
	                                            --,r.Remarks
	                                            --,emp.EmployeeCode
                                                ,EMP.PlantId
	                                            ,c.UserName as EmployeeCategory
                                                ,HR.IsPastResignationAllowed
                                                ,HR.PastResignationDaysAllowed
		                                        FROM dbo.EmployeeInformation AS EMP
		                                        LEFT OUTER JOIN [MST].[ManpowerBudget]  PMB on PMB.id=emp.BudgetCode
		                                        LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
												LEFT OUTER JOIN
												( select * from
													(select reg.* from [TRN].[Resignation] reg
														inner join
														(
															select max(ResignationDate) ResignationDate,EmployeeId from [TRN].[Resignation] group by  EmployeeId
														)
														D on D.EmployeeId = reg.EmployeeId and D.ResignationDate = reg.ResignationDate
														) xx
												) R ON R.EmployeeId=EMP.SystemId
                                                LEFT OUTER JOIN [DBO].[PlantWiseHRMSSetting] HR on hr.PlantID = emp.PlantId
		                                        LEFT OUTER JOIN [HKP].[Designation] DEG ON EMP.GivenDesignationId=DEG.Id
		                                        LEFT OUTER JOIN [HKP].[Designation] D ON PR.DesignationId=D.Id
		                                        LEFT OUTER JOIN [ORG].[Department] DEPT ON PR.DepartmentId=DEPT.Id
		                                        LEFT OUTER JOIN [HKP].[EmployeeCategory] C ON C.id=EMP.EmployeeCategorySystemID
		                                        LEFT OUTER JOIN [ORG].[Entity] E ON E.Id=PMB.entityid
		                                        where e.UserName <> ' ' and EMP.PlantId ='" + plantID + "' and Isnull(ApprovalStatus,'') = 'Pending'  and EMP.EmployeeStatus = 'Active'" + str;

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel X_MultipleResignationAppliedList(GridParameter parameters, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId)
        {
            try
            {
                var str = "";
                if (!isControlAdmin && !isSysAdmin)
                {
                    //str = @" and isnull(r.EmployeeId,'') in
                    //                        (select systemid from EmployeeInformation e where e.BudgetCode in
                    //                            (select Id from mst.ManpowerBudget where EntityId in
                    //                                (select entityid from [HKP].[ApprovalConfiguration]
                    //                                  where ResignationApproval =
                    //                                  (select EmployeeId from [SEC].[User] where UserId='" + employeeId + @"')
                    //                                )
                    //                            )
                    //                        ) ";
                    str = @" AND emp.BudgetCode IN (SELECT Id from mst.ManpowerBudget WHERE EntityId IN (SELECT entityid FROM [HKP].[ApprovalConfiguration] WHERE ResignationApproval='" + employeeId + "'))";
                }

                parameters.CmdText = @"SELECT
                                                --0 Active
                                                r.Id
                                                ,EMP.SystemId as EmployeeId
                                                ,EMP.EmployeeName
                                                ,emp.EmployeeCode
												,E.UserName as Entity
												,D.UserName Designation
												,DEG.UserName [Given Designation]
                                                ,Replace(CONVERT(VARCHAR(11), r.ResignationDate, 106), ' ', '-') ResignationDate
                                                --,r.ResignationDate as RegSort
                                                ,Reason
                                                ,AttachLetter
                                                ,Replace(CONVERT(VARCHAR(11), r.EffectiveDate, 106), ' ', '-') EffectiveDate
                                                ,Replace(CONVERT(VARCHAR(11), r.ApprovedEffectiveDate, 106), ' ', '-') ApprovedEffectiveDate
                                                ,r.ApprovedEffectiveDate AEFDate
                                                ,ISNULL (R.SpecialFollowUP,0 )SpecialFollowUP
                                                ,r.Remarks
                                                ,Replace(CONVERT(VARCHAR(11), emp.DOJ, 106), ' ', '-') DOJ
                                                ,Replace(CONVERT(VARCHAR(11), emp.DOC, 106), ' ', '-') DOC
                                                ,c.UserName as EmployeeCategory
                                                ,r.ApprovalStatus
                                                FROM dbo.EmployeeInformation AS EMP
												LEFT OUTER JOIN [MST].[ManpowerBudget]  PMB on PMB.id=emp.BudgetCode
												LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                                                LEFT OUTER JOIN [TRN].[Resignation] R ON R.EmployeeId=EMP.SystemId
												LEFT OUTER JOIN [HKP].[Designation] DEG ON EMP.GivenDesignationId=DEG.Id
												LEFT OUTER JOIN [HKP].[Designation] D ON PR.DesignationId=D.Id
												LEFT OUTER JOIN [ORG].[Department] DEPT ON PR.DepartmentId=DEPT.Id
												LEFT OUTER JOIN [HKP].[EmployeeCategory] C ON C.id=EMP.EmployeeCategorySystemID
												LEFT OUTER JOIN [ORG].[Entity] E ON E.Id=PMB.entityid
                                                where isnull(r.id,'') <> ''
                                                and Isnull(ApprovalStatus,'') <> 'Approved'
				                                and Isnull(ApprovalStatus,'') <> 'Pending'
                                                and Isnull(ApprovalStatus,'') <> 'Rejected'
											    " + str;

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> MultipleResignationAppliedPendingList(bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId)
        {
            try
            {
                var str = "";
                if (!isControlAdmin && !isSysAdmin)
                {
                    //str = @" and isnull(r.EmployeeId,'') in
                    //                        (select systemid from EmployeeInformation e where e.BudgetCode in
                    //                            (select Id from mst.ManpowerBudget where EntityId in
                    //                                (select entityid from [HKP].[ApprovalConfiguration]
                    //                                  where ResignationApproval =
                    //                                  (select EmployeeId from [SEC].[User] where UserId='" + employeeId + @"')
                    //                                )
                    //                            )
                    //                        ) ";
                    str = @" AND emp.BudgetCode IN (SELECT Id from mst.ManpowerBudget WHERE EntityId IN (SELECT entityid FROM [HKP].[ApprovalConfiguration] WHERE ResignationApproval='" + employeeId + "'))";
                }

                string strSql = @"SELECT
                                                --0 Active
                                                r.Id
                                                ,EMP.SystemId as EmployeeId
                                                ,EMP.EmployeeName
                                                ,emp.EmployeeCode
												,E.UserName as Entity
												,D.UserName Designation
												,DEG.UserName [Given Designation]
                                                ,Replace(CONVERT(VARCHAR(11), r.ResignationDate, 106), ' ', '-') ResignationDate
                                                --,r.ResignationDate as RegSort
                                                ,Reason
                                                ,AttachLetter
                                                ,Replace(CONVERT(VARCHAR(11), r.EffectiveDate, 106), ' ', '-') EffectiveDate
                                                ,Replace(CONVERT(VARCHAR(11), r.ApprovedEffectiveDate, 106), ' ', '-') ApprovedEffectiveDate
                                                ,r.ApprovedEffectiveDate AEFDate
                                                ,ISNULL (R.SpecialFollowUP,0 )SpecialFollowUP
                                                ,r.Remarks
                                                ,Replace(CONVERT(VARCHAR(11), emp.DOJ, 106), ' ', '-') DOJ
                                                ,Replace(CONVERT(VARCHAR(11), emp.DOC, 106), ' ', '-') DOC
                                                ,c.UserName as EmployeeCategory
                                                ,r.ApprovalStatus
                                                FROM dbo.EmployeeInformation AS EMP
												LEFT OUTER JOIN [MST].[ManpowerBudget]  PMB on PMB.id=emp.BudgetCode
												LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                                                LEFT OUTER JOIN [TRN].[Resignation] R ON R.EmployeeId=EMP.SystemId
												LEFT OUTER JOIN [HKP].[Designation] DEG ON EMP.GivenDesignationId=DEG.Id
												LEFT OUTER JOIN [HKP].[Designation] D ON PR.DesignationId=D.Id
												LEFT OUTER JOIN [ORG].[Department] DEPT ON PR.DepartmentId=DEPT.Id
												LEFT OUTER JOIN [HKP].[EmployeeCategory] C ON C.id=EMP.EmployeeCategorySystemID
												LEFT OUTER JOIN [ORG].[Entity] E ON E.Id=PMB.entityid
                                                where isnull(r.id,'') <> ''
                                                and Isnull(ApprovalStatus,'') <> 'Approved'
				                                and Isnull(ApprovalStatus,'') <> 'Pending'
                                                and Isnull(ApprovalStatus,'') <> 'Rejected'
											    " + str;

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> MultipleResignationAppliedList(bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string PlantId, string employeeId)
        {
            try
            {
                var str = "";

                //if (!isControlAdmin && !isSysAdmin)
                //{
                //    //str = @" and isnull(r.EmployeeId,'') in
                //    //                                       (select systemid from EmployeeInformation e where e.BudgetCode in
                //    //                                           (select Id from mst.ManpowerBudget where EntityId in
                //    //                                               (select entityid from [HKP].[ApprovalConfiguration]
                //    //                                                 where ResignationApproval =
                //    //                                                 (select EmployeeId from [SEC].[User] where UserId='" + employeeId + @"')
                //    //                                               )
                //    //                                           )
                //    //                                       ) ";
                //    str = @" AND emp.BudgetCode IN (SELECT Id from mst.ManpowerBudget WHERE EntityId IN (SELECT entityid FROM [HKP].[ApprovalConfiguration] WHERE ResignationApproval='" + employeeId + "'))";
                //}
                string strSql = @"SELECT [CheckBoxSelect] = CONVERT(BIT, 'False')
                                                 ,r.Id
                                                ,emp.EmployeeName
                                                ,E.UserName EntityName
                                                 ,Replace(CONVERT(VARCHAR(11), r.ResignationDate, 106), ' ', '-') ResignationDate
                                                 ,Reason
                                                 --,r.ResignationDate as RegSort
                                                 ,Replace(CONVERT(VARCHAR(11), r.EffectiveDate, 106), ' ', '-') EffectiveDate
                                                 ,Replace(CONVERT(VARCHAR(11), r.EffectiveDate, 106), ' ', '-') ApprovedEffectiveDate
                                                 ,Replace(CONVERT(VARCHAR(11), r.ApprovedEffectiveDate, 106), ' ', '-') ApprovedEffectiveDateOld
												 ,r.SeparationTypeId SeparationType
												 ,'' SeparationTypeOld
                                                 ,r.SeparationTypeId
                                                 ,r.Remarks
                                                 ,r.Remarks RemarksOld
                                                 ,emp.SystemId EmployeeId
                                                 ,emp.EmployeeCode
                                                 ,emp.PlantId,E.UserName as Entity
                                                 ,r.AttachLetter
                                                 ,d.UserName as GivenDesignation,DG.UserName Designation
                                                 ,Replace(CONVERT(VARCHAR(11), emp.DOJ, 106), ' ', '-') DOJ
                                                 ,Replace(CONVERT(VARCHAR(11), emp.DOC, 106), ' ', '-') DOC
                                                 ,c.UserName as EmployeeCategory
                                                 ,r.ApprovalStatus
                                                 ,r.ApprovalStatus ApprovalStatusOld
												 ,CONVERT(BIT,0) SpecialFollowUp
                                                 ,CONVERT(BIT,0) SpecialFollowUpOld

                                                 ,DAD.EmployeeDisciplinaryActionId CaseNo
                                                 ,DC.UserName DisciplinaryAction
                                                 ,DD.Description Letter

                                                FROM dbo.EmployeeInformation AS EMP
												LEFT OUTER JOIN [MST].[ManpowerBudget]  PMB on PMB.id=emp.BudgetCode
		                                        LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                                                LEFT OUTER JOIN
                                                [TRN].[Resignation] r ON r.EmployeeId=EMP.SystemId
                                                LEFT OUTER JOIN
                                                [HKP].[Designation] d ON d.id=EMP.GivenDesignationId
                                                LEFT OUTER JOIN [HKP].[Designation] DG ON PR.DesignationId=DG.Id
                                                LEFT JOIN MST.DesignationMaster dm ON EMP.GivenDesignationId = dm.DesignationId
                                                LEFT JOIN HKP.EmployeeCategory C ON C.Id=DM.EmployeeCategoryId
                                                LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id

												left join EmployeeDisciplinaryActionDetails DAD on DAD.Id=r.DisciplinaryActionDetailsId
												left join [HKP].[DisciplinaryActionCategory] DC on DC.id=DAD.DisciplinaryActionCategoryId
												left join [dbo].[DisciplinaryActionSettingDetails] DD ON DD.Id=DAD.DisciplinaryActionSettingDetailsId


                                                WHERE ISNULL(r.id,'') <> ''
											    AND ISNULL(r.ApprovalStatus,'') = 'Pending' AND E.PlantId='" + PlantId + @"'
											    " + str;
                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private DataSet ExperienceById(string EmpId)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"select e.SystemID, doj,x.EndDate,x.StartDate from dbo.EmployeeInformation e
                                        Left outer join
                                        (
                                        select SystemId,EmployeeId,  max(EndDate) EndDate, min(StartDate) StartDate
                                             from dbo.EmpExperienceInformation
                                             where EmployeeId = '"+ EmpId + @"'
                                             group by EmployeeId,SystemId
                                         )
                                         x on x.EmployeeId = e.SystemId
                                         where e.SystemId'" + EmpId + "'";

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DataSet GetApprovedSalaryInfo(string EmpIds, string dos)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                //parameters.CmdText = @"select 
                //                            c.IsApproved,e.EmployeeCode,e.systemid,dos
                //                            from (select cc.* from SalaryProcChild cc
                //                            inner join SalaryHead h on h.SalaryHeadID=cc.SalaryHeadID
                //                            where h.HeadCategory='Basic'
                //                            )c 
                //                            inner join EmployeeInformation e on c.EmpInfoSystemID=e.SystemId
                //                            inner join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID and m.YearNo=year('" + dos + "') and m.monthno=month('" + dos + @"')
                //                            where c.EmpInfoSystemID ='" + EmpIds + "' and c.IsApproved=1";


                parameters.CmdText = @" SELECT l.IsLocked  IsApproved,e.EmployeeCode,e.systemid,e.dos from SalaryLock l
										INNER JOIN EmployeeInformation e on l.EmpSystemId=e.SystemId
										WHERE l.EmpSystemId='" + EmpIds + "' and l.YearNo=year('" + dos + "') and 	l.MonthNo=month('" + dos + @"') and	l.IsLocked=1 ";



                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel ResignationApprovalQueryByPlantId(GridParameter parameters, string plantID)
        {
            try
            {
                parameters.CmdText = @"SELECT r.Id
                                            ,emp.EmployeeName
	                                        ,Replace(CONVERT(VARCHAR(11), r.ResignationDate, 106), ' ', '-') ResignationDate
	                                        ,Reason
	                                        ,AttachLetter
	                                        ,Replace(CONVERT(VARCHAR(11), r.EffectiveDate, 106), ' ', '-') EffectiveDate
	                                        ,Replace(CONVERT(VARCHAR(11), r.ApprovedEffectiveDate, 106), ' ', '-') ApprovedEffectiveDate
	                                        ,r.Remarks
	                                        ,EmployeeId
	                                        ,emp.EmployeeCode
                                            ,emp.PlantId
	                                        ,d.UserName as Designation
	                                        ,Replace(CONVERT(VARCHAR(11), emp.DOJ, 106), ' ', '-') DOJ
	                                        ,Replace(CONVERT(VARCHAR(11), emp.DOC, 106), ' ', '-') DOC
	                                        ,c.UserName as EmployeeCategory
                                            FROM dbo.EmployeeInformation AS EMP
                                            LEFT OUTER JOIN
                                            [TRN].[Resignation] r ON r.EmployeeId=EMP.SystemId
                                            LEFT OUTER JOIN
                                            [HKP].[Designation] d ON d.id=EMP.DesignationSystemID
                                            LEFT OUTER JOIN
                                            HKP.[EmployeeCategory] c ON c.id=EMP.EmployeeCategorySystemID
		                                    where isnull(r.id,'') <> ''
		                                    and emp.PlantId ='" + plantID + "'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private void GetDuration(string fromdate, string todate, out int rYear, out int rMonth)
        {
            try
            {
                if (!IsDateOK(fromdate))
                {
                    throw new Exception("Valid Date is required for 'Start Date'");
                }
                if (!IsDateOK(todate))
                {
                    todate = DateTime.Now.ToString("dd-MMM-yyyy");
                }

                var sDate = Convert.ToDateTime(fromdate);
                var eDate = Convert.ToDateTime(todate);
                if (eDate < sDate)
                {
                    throw new Exception("'End Date' must be greater than 'Start Date'");
                }
                var _days = eDate.Subtract(sDate).TotalDays;

                var _year = (int)_days / 365;
                var _month = ((int)_days - (_year * 365)) / 30;
                if (_month == 12)
                {
                    _month = 0;
                    _year = _year + 1;
                }

                rYear = _year;
                rMonth = _month;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool IsDateOK(string strdate)
        {
            try
            {
                if (strdate.Length != 11)
                {
                    return false;
                }
                if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
                {
                    return false;
                }
                DateTime myDt = Convert.ToDateTime(strdate);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }// end function

        public void GetExperience(string empid, out int tYear, out int tMonth)
        {
            tYear = 0;
            tMonth = 0;
            try
            {
                DataSet ds = ExperienceById(empid);
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    string sd = ds.Tables[0].Rows[i]["StartDate"].ToString();
                    string ed = ds.Tables[0].Rows[i]["EndDate"].ToString();
                    GetDuration(sd, ed, out tYear, out tMonth);
                }
            }
            catch
            {
            }
        }// end function

        public IWorkbook ReportEmployeeInfo(ReportParam status)
        {
            try
            {
                ReportResignationEmployeeInfo obj = new ReportResignationEmployeeInfo(_sqlRepository);
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IWorkbook workbook = obj.EmployeeInfo(excelEngine, status);
                    return workbook;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    #region ResignationType

    public class ResignationTypeService
    {
        private readonly SqlRepository _sqlRepository;
        public ResignationTypeService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var sql = "select * from HKP.ResignationType where Id = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string TableName = "HKP.ResignationType";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT RT.* FROM HKP.ResignationType RT order by Sequence";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region SAVE
        public Dictionary<string, object> Save(Dictionary<string, object> data)
        {
            try
            {
                string TableNameHead = "HKP.ResignationType";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where StandardName='" + data["StandardName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Standard Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";

                #region HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] = _Id;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion HEAD



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SAVE

        public string Delete(string id)
        {
            try
            {

                string TableName = "HKP.ResignationType";
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM HKP.ResignationType");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

    }

    #endregion ResignationType
}