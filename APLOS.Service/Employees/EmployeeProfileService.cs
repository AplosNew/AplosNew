#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Organizations;
using Library.Model.Payrolls;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.Pdf;
using Syncfusion.DocToPDFConverter;
using Syncfusion.OfficeChartToImageConverter;
using System.Text.RegularExpressions;
using OTSBD;
using Library.Model.Setups;
using Library.Service.Setups;
using Library.Service.Payrolls;
using System.Runtime.Serialization.Formatters.Binary;
using Syncfusion.Presentation;
using ConnectionManager.DAL;
using Zen.Barcode;
using System.Drawing.Imaging;
//using Syncfusion.DocToPDFConverter;
//using Syncfusion.JavaScript.Models;
//using Syncfusion.OfficeChartToImageConverter;
//using Syncfusion.Pdf;
#endregion Using

namespace Library.Service.Employees
{
    public class EmployeeProfileService : Service<EmployeeInformation>, IEmployeeProfileService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<EmployeeMobileAppsAuthorization> _employeeAuthService;
        private readonly IRepositoryAsync<EmployeeBudgetCodeHistory> _employeeBudgetCodeHistoryService;
        private readonly IRepositoryAsync<EmployeeNomineeInfo> _employeeNomineeInfo;
        private readonly IRepositoryAsync<EmployeeDependantInfo> _employeeDependantInfo;
        private readonly IRepositoryAsync<EmployeeLandLordInfo> _employeeLandLoardInfo;
        private readonly IRepositoryAsync<DesignationMaster> _designationMasterRepository;
        private readonly IManpowerBudgetService _manpowerBudgetService;
        private readonly IRepositoryAsync<XLUploadDetail> _xLUploadDetailService;
        private readonly IEmployeeAttendanceGroupService _EmployeeAttendanceGroupRepository;
        private readonly IPayrollGroupMasterService _payrollGroupMasterService;
        private readonly IRepositoryAsync<EmployeeIdCardIssue> _employeeIdCardIssue;
        private readonly IUnitOfWork _unitOfWork;
        private ConManager objCon;

        public EmployeeProfileService(
             IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , IRepositoryAsync<EmployeeInformation> employeeInformationRepository
            , IRepositoryAsync<EmployeeMobileAppsAuthorization> employeeAuthService
            , IRepositoryAsync<EmployeeBudgetCodeHistory> employeeBudgetCodeHistoryService
            , IRepositoryAsync<EmployeeNomineeInfo> employeeNomineeInfo
            , IRepositoryAsync<EmployeeDependantInfo> employeeDependantInfo
            , IRepositoryAsync<EmployeeLandLordInfo> employeeLandLoardInfo
            , IRepositoryAsync<DesignationMaster> designationMasterRepository
             , IManpowerBudgetService manpowerBudgetService
            , ISqlRepository sqlRepository
            , IRepositoryAsync<XLUploadDetail> xLUploadDetailService
            , IEmployeeAttendanceGroupService EmployeeAttendanceGroupRepository
            , IPayrollGroupMasterService payrollGroupMasterService
            , IRepositoryAsync<EmployeeIdCardIssue> employeeIdCardIssue) : base(employeeInformationRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _employeeAuthService = employeeAuthService;
            _employeeBudgetCodeHistoryService = employeeBudgetCodeHistoryService;
            _designationMasterRepository = designationMasterRepository;
            _manpowerBudgetService = manpowerBudgetService;
            _xLUploadDetailService = xLUploadDetailService;
            _EmployeeAttendanceGroupRepository = EmployeeAttendanceGroupRepository;
            _payrollGroupMasterService = payrollGroupMasterService;
            _employeeNomineeInfo = employeeNomineeInfo;
            _unitOfWork = unitOfWork;
            _employeeDependantInfo = employeeDependantInfo;
            _employeeLandLoardInfo = employeeLandLoardInfo;
            _employeeIdCardIssue = employeeIdCardIssue;
        }

        #endregion Constructor

        #region Operation

        private string GetPadding(string iv)
        {
            while (iv.Length < bplib.clsWebLib.EMP_BASIC_PK_PAD)
            {
                iv = "0" + iv;
            }
            return iv;
        }

        private string GetEmpPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;

            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EMP_BASIC", out idFromDB);
            string syspad = GetPadding(idFromDB);
            sID = DateTime.Now.ToString("yy") + syspad;

            return sID;

        }

        public void InsetOrUpdateMaster(EmployeeInformation entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //  var dob = Convert.ToDateTime(entity.DOB).AddYears(18);
                //var dob2 = Convert.ToDateTime(entity.DOJ).Subtract(Convert.ToDateTime(entity.DOB)).Days / 365;

                //if (dob2 < 18)
                //{
                //    throw new Exception("This Employee Below 18 Years...");
                //}

                //if (dob > entity.DateAdded)
                //{
                //    throw new Exception("This Employee Below 18 Years...");
                //}

                //var emp = PlantWiseDOJ(entity.PlantID);
                //var nodays = PlantWiseDOJDays(entity.PlantID);

                //if (Convert.ToDateTime(entity.DOJ) < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                //{
                //    if (emp.Tables[0].Rows.Count > 0)
                //    {
                //        var start = DateTime.Now;
                //        var end = Convert.ToDateTime(entity.DOJ);

                //        TimeSpan difference = start - end;
                //        var days = Convert.ToInt32(difference.Days);
                //        var date = Convert.ToInt32(nodays.Tables[0].Rows[0]["PastDOJDaysAllowed"]);
                //        if (date < days)
                //        {
                //            throw new Exception("Maximum  " + nodays.Tables[0].Rows[0]["PastDOJDaysAllowed"] + " days back is allowed for DOJ.");
                //        }
                //        //allowed
                //        //dblist.DOJ = entity.DOJ;
                //    }
                //    else
                //    {
                //        throw new Exception("Previous Date of Join is not allowed");
                //    }
                //}
                //else if (Convert.ToDateTime(entity.DOJ) > Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                //{
                //    throw new Exception("Future Date of Join is not allowed");
                //}

                if (string.IsNullOrEmpty(entity.SystemId))
                {
                    entity.SystemId = GetEmpPK();
                    entity.EmployeeId = entity.SystemId;
                    entity.EmployeeStatus = "Active";

                    Insert(entity);
                }
                else
                {
                    entity.UpdatedBy = identity.Name;
                    entity.DateUpdated = DateTime.Now;
                    Update(entity);
                }

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateMaster(EmployeeInformation entity, string name)
        {
            try
            {
                var dblist = Find(entity.SystemId);
                //var dob = Convert.ToDateTime(entity.DOB).AddYears(18);
                ////var dob2 = Convert.ToDateTime(entity.DOJ).Subtract(Convert.ToDateTime(entity.DOB)).Days / 365;

                ////if (dob2 < 18)
                ////{
                ////    throw new Exception("This Employee Below 18 Years...");
                ////}

                //if (dob > entity.DateAdded)
                //{
                //    throw new Exception("This Employee Below 18 Years...");
                //}


                dblist.Salutation = entity.Salutation;
                dblist.FirstName = entity.FirstName.ToUpper();
                if (!string.IsNullOrEmpty(entity.MiddleName))
                {
                    dblist.MiddleName = entity.MiddleName.ToUpper();
                }
                else
                {
                    dblist.MiddleName = DBNull.Value.ToString();
                }
                if (!string.IsNullOrEmpty(entity.LastName))
                {
                    dblist.LastName = entity.LastName.ToUpper();
                }
                else
                {
                    dblist.LastName = DBNull.Value.ToString();
                }

                dblist.EmployeeName = entity.EmployeeName.ToUpper();
                if (!string.IsNullOrEmpty(entity.NickName))
                {
                    dblist.NickName = entity.NickName.ToUpper();
                }
                else
                {
                    dblist.NickName = DBNull.Value.ToString();
                }
                dblist.CardNumber = entity.CardNumber;
                dblist.EmpPicPath = entity.EmpPicPath;
                dblist.JobLocationID = entity.JobLocationID;
                dblist.EmpType = entity.EmpType;

                dblist.IsKnownPerson = entity.IsKnownPerson;
                dblist.NumberOfKnownPerson = entity.NumberOfKnownPerson;
                dblist.ApplyingAsFresher = entity.ApplyingAsFresher;
                dblist.EmployeeNameLocal = entity.EmployeeNameLocal;


                //var emp = PlantWiseDOJ(dblist.PlantID);
                //var nodays = PlantWiseDOJDays(dblist.PlantID);

                //var isApproved = base.Query(t => t.SystemId == entity.SystemId).Select(t => t.IsApproved).FirstOrDefault();
                //if (isApproved)
                //{
                //    throw new Exception("Update is not allowed.");
                //}

                //if (Convert.ToDateTime(entity.DOJ) < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                //{
                //    if (emp.Tables[0].Rows.Count > 0)
                //    {
                //        var start = DateTime.Now;
                //        var end = Convert.ToDateTime(entity.DOJ);

                //        TimeSpan difference = start - end;
                //        var days = Convert.ToInt32(difference.Days);
                //        var date = Convert.ToInt32(nodays.Tables[0].Rows[0]["PastDOJDaysAllowed"]);
                //        if (date < days)
                //        {
                //            throw new Exception("Maximum  " + nodays.Tables[0].Rows[0]["PastDOJDaysAllowed"] + " days back is allowed for DOJ.");
                //        }
                //        //allowed
                //        dblist.DOJ = entity.DOJ;
                //    }
                //    else
                //    {
                //        throw new Exception("Previous Date of Join is not allowed");
                //    }
                //}
                //else if (Convert.ToDateTime(entity.DOJ) > Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                //{
                //    throw new Exception("Future Date of Join is not allowed");
                //}
                //else
                //{
                //    dblist.DOJ = entity.DOJ;
                //    //Current
                //}

                dblist.UpdatedBy = name;
                dblist.DateUpdated = DateTime.Now;
                Update(dblist);

                var document = EmployeeDocFile(entity.SystemId);
                if (document.Tables[0].Rows.Count > 0)
                {
                    _designationMasterRepository.ExecuteSqlCommand(@"UPDATE EmployeeDocument SET FileId='" + entity.SystemId + @"', FileName='" + entity.EmpPicPath + @"' WHERE  Id = '" + document.Tables[0].Rows[0]["Id"].ToString() + @"'");
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdatePersonal(EmployeeInformation entity, string name)
        {
            try
            {
                var dblist = Find(entity.SystemId);

                dblist.NationalID = entity.NationalID;
                dblist.TIN = entity.TIN;
                dblist.FatherName = entity.FatherName;
                dblist.MotherName = entity.MotherName;
                dblist.ReligionID = entity.ReligionID;
                dblist.CitizenID = entity.CitizenID;
                dblist.BloodGroupID = entity.BloodGroupID;
                dblist.GenderID = entity.GenderID;
                dblist.DOB = entity.DOB;
                dblist.BirthdayCelebrationDate = entity.BirthdayCelebrationDate;
                dblist.Height = entity.Height;
                dblist.Weight = entity.Weight;
                dblist.IdentificationMark = entity.IdentificationMark;
                dblist.CasteId = entity.CasteId;
                dblist.CivilStatusID = entity.CivilStatusID;
                dblist.MarriagedayCelebrationDate = entity.MarriagedayCelebrationDate;
                dblist.SpouseName = entity.SpouseName;
                dblist.SpouseNameLocal = entity.SpouseNameLocal;
                dblist.SpouseNationalID = entity.SpouseNationalID;
                dblist.SpouseOccupation = entity.SpouseOccupation;
                dblist.NoOfChildren = entity.NoOfChildren;
                dblist.FatherNameLocal = entity.FatherNameLocal;
                dblist.MotherNameLocal = entity.MotherNameLocal;
                dblist.LocalIdentificationMark = entity.LocalIdentificationMark;

                dblist.UpdatedBy = name;
                dblist.DateUpdated = DateTime.Now;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public void UpdateAddress(EmployeeInformation entity, string name)
        {
            try
            {
                var dblist = Find(entity.SystemId);

                dblist.PresentAddress1 = entity.PresentAddress1;
                dblist.PresentAddress2 = entity.PresentAddress2;
                dblist.PresCountryID = entity.PresCountryID;
                dblist.PresStateId = entity.PresStateId;
                dblist.PresDistrictID = entity.PresDistrictID;
                dblist.PresCityID = entity.PresCityID;
                dblist.PresThanaID = entity.PresThanaID;
                dblist.PresPostOfficeID = entity.PresPostOfficeID;
                dblist.PresZipCode = entity.PresZipCode;
                dblist.PresAreaID = entity.PresAreaID;
                dblist.PresentArea = entity.PresentArea;

                dblist.ParmanentAddress1 = entity.ParmanentAddress1;
                dblist.ParmanentAddress2 = entity.ParmanentAddress2;
                dblist.ParmCountryID = entity.ParmCountryID;
                dblist.ParmStateId = entity.ParmStateId;
                dblist.ParmDistrictID = entity.ParmDistrictID;
                dblist.ParmCityID = entity.ParmCityID;
                dblist.ParmThanaID = entity.ParmThanaID;
                dblist.ParmPostOfficeID = entity.ParmPostOfficeID;
                dblist.ParmZipCode = entity.ParmZipCode;
                dblist.ParmAreaID = entity.ParmAreaID;
                dblist.ParmanentArea = entity.ParmanentArea;

                dblist.CellPhnNo = entity.CellPhnNo;
                dblist.EmailId = entity.EmailId;

                dblist.EmrCntPer1Name = entity.EmrCntPer1Name;
                dblist.EmrCntPer2Name = entity.EmrCntPer2Name;
                dblist.EmrCntPer1CellNo = entity.EmrCntPer1CellNo;
                dblist.EmrCntPer2CellNo = entity.EmrCntPer2CellNo;
                dblist.PresentAddress1Local = entity.PresentAddress1Local;
                dblist.PresentAddress2Local = entity.PresentAddress2Local;
                dblist.ParmanentAddress1Local = entity.ParmanentAddress1Local;
                dblist.ParmanentAddress2Local = entity.ParmanentAddress2Local;

                dblist.EmployeeStatus = "Active";
                dblist.UpdatedBy = name;
                dblist.DateUpdated = DateTime.Now;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateEmployment(EmployeeInformation entity, string name)
        {
            try
            {
                var dblist = Find(entity.SystemId);
                dblist.LegalDesignationId = entity.LegalDesignationId;
                dblist.GivenDesignationId = entity.GivenDesignationId;
                dblist.EmployeeCodeTypeId = entity.EmployeeCodeTypeId;
                dblist.DOC = entity.DOC;
                dblist.DOCBy = entity.DOCBy;
                dblist.DOCDay = entity.DOCDay;
                dblist.DOCIsDay = entity.DOCIsDay;

                dblist.DOCMonth = entity.DOCMonth;
                dblist.DOCIsMonth = entity.DOCIsMonth;

                dblist.IsConfirmed = entity.IsConfirmed;
                dblist.VendorId = entity.VendorId;
                dblist.ExcludeOT = entity.ExcludeOT;
                dblist.isLeaveOnDOC = entity.isLeaveOnDOC; ;
                dblist.EmploymentType = entity.EmploymentType;
                dblist.UpdatedBy = name;
                dblist.DateUpdated = DateTime.Now;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateLocalInfo(EmployeeInformation entity, string name)
        {
            try
            {
                var dblist = Find(entity.SystemId);
                dblist.EmployeeNameLocal = entity.EmployeeNameLocal;
                dblist.FatherNameLocal = entity.FatherNameLocal;
                dblist.MotherNameLocal = entity.MotherNameLocal;
                dblist.PresentAddress1Local = entity.PresentAddress1Local;
                dblist.PresentAddress2Local = entity.PresentAddress2Local;
                dblist.ParmanentAddress1Local = entity.ParmanentAddress1Local;
                dblist.ParmanentAddress2Local = entity.ParmanentAddress2Local;
                dblist.SpouseNameLocal = entity.SpouseNameLocal;
                dblist.LocalIdentificationMark = entity.LocalIdentificationMark;
                dblist.UpdatedBy = name;
                dblist.DateUpdated = DateTime.Now;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateRelativeInfo(EmployeeInformation entity, string name)
        {
            try
            {
                var dblist = Find(entity.SystemId);
                dblist.AnyRelativeWorkedHere = entity.AnyRelativeWorkedHere;
                dblist.RelationShip = entity.RelationShip;
                dblist.RelativeDesignation = entity.RelativeDesignation;
                dblist.RelativeCellNo = entity.RelativeCellNo;
                dblist.RelativeSystemId = entity.RelativeSystemId;

                dblist.UpdatedBy = name;
                dblist.DateUpdated = DateTime.Now;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateAdvanceInfo(EmployeeInformation entity, string name, string IP)
        {
            try
            {
                var dblist = Find(entity.SystemId);
                dblist.PaymentMode = entity.PaymentMode;
                dblist.PaymentModeEffectiveDate = entity.PaymentModeEffectiveDate;
                dblist.CardNumber = entity.CardNumber;
                dblist.IsEntryComplete = entity.IsEntryComplete;
                dblist.ResidenceGroupId = entity.ResidenceGroupId;
                dblist.TransportGroupId = entity.TransportGroupId;
                
                if (!string.IsNullOrEmpty(entity.OperationMasterID))
                {
                    dblist.OperationMasterID = entity.OperationMasterID;
                }

                if (!string.IsNullOrEmpty(entity.OperationVariationId))
                {
                    dblist.OperationVariationId = entity.OperationVariationId;
                }

                dblist.UpdatedBy = name;
                dblist.DateUpdated = DateTime.Now;

                Update(dblist);

                if (!string.IsNullOrEmpty(entity.AttendanceGroupId))
                {
                    EmployeeAttendanceGroup attendanceGroup = new EmployeeAttendanceGroup
                    {
                        EmployeeId = entity.SystemId,
                        AttendanceGroupId = entity.AttendanceGroupId,
                        PlantId = entity.PlantID,
                        CompanyGroupId = entity.GroupID
                    };

                    _EmployeeAttendanceGroupRepository.InSertOrUpdate(attendanceGroup);
                }

                if (!string.IsNullOrEmpty(entity.PayrollGroupId))
                {

                    PayrollGroupMaster payrollGroupMaster = new PayrollGroupMaster
                    {
                        EmployeeId = entity.SystemId,
                        PayrollGroupId = entity.PayrollGroupId,
                        PlantId = entity.PlantID,
                        CompanyGroupId = entity.GroupID
                    };

                    _payrollGroupMasterService.InSertOrUpdate(payrollGroupMaster);
                }

                if (!string.IsNullOrEmpty(entity.AccountsGroupId))
                {

                    EmployeeAccountsGroup employeeAccountsGroup = new EmployeeAccountsGroup
                    {
                        EmployeeId = entity.SystemId,
                        AccountsGroupId = entity.AccountsGroupId,
                        PlantId = entity.PlantID,
                        CompanyGroupId = entity.GroupID,

                        AddedBy = entity.AddedBy,
                        AddedFromIP = IP,
                        UpdatedBy = entity.UpdatedBy,
                        UpdatedFromIP = IP
                    };

                    InSertOrUpdateEmployeeAccountsGroup(employeeAccountsGroup);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void InSertOrUpdateEmployeeAccountsGroup(EmployeeAccountsGroup entity)
        {

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            if (entity != null)
            {
                string sql = "SELECT * FROM [dbo].[EmployeeAccountsGroup] WHERE EmployeeId='" + entity.EmployeeId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = entity.EmployeeId;
                    dr["PlantId"] = entity.PlantId;
                    dr["AccountsGroupId"] = entity.AccountsGroupId;
                    dr["CompanyGroupId"] = entity.CompanyGroupId;
                    dr["EmployeeId"] = entity.EmployeeId;

                    dr["AddedBy"] = entity.AddedBy;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = entity.AddedFromIP;

                    dr["UpdatedBy"] = entity.UpdatedBy;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = entity.UpdatedFromIP;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["PlantId"] = entity.PlantId;
                    dr["AccountsGroupId"] = entity.AccountsGroupId;
                    dr["CompanyGroupId"] = entity.CompanyGroupId;
                    dr["EmployeeId"] = entity.EmployeeId;

                    dr["UpdatedBy"] = entity.UpdatedBy;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = entity.UpdatedFromIP;

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }


        }
        private string GetPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_NOMINEE", out idFromDB);
            systemID = "EN-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        public void InsertOrUpdate(EmployeeNomineeInfo entity)
        {
            var flag = false;
            try
            {
                flag = true;
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPK();
                    entity.ModelState = ModelState.Added;
                    AuditService.AddedLog(entity);
                }
                else
                {
                    entity.ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(entity);
                }
                _employeeNomineeInfo.InsertOrUpdateGraph(entity);
                _unitOfWork.BeginTransaction();
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
        private string GetDPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_DEPEND", out idFromDB);
            systemID = "EN-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        private string GetLandPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_LAND", out idFromDB);
            systemID = "EN-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        public void InsertOrUpdatedependantInfo(EmployeeDependantInfo entity)
        {
            var flag = false;
            try
            {
                flag = true;
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetDPK();
                    entity.ModelState = ModelState.Added;
                    AuditService.AddedLog(entity);
                }
                else
                {
                    entity.ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(entity);
                }
                _employeeDependantInfo.InsertOrUpdateGraph(entity);
                _unitOfWork.BeginTransaction();
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


        public void InsertOrUpdateLandLordInfo(EmployeeLandLordInfo entity)
        {
            var flag = false;
            try
            {
                flag = true;
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetLandPK();
                    entity.ModelState = ModelState.Added;
                    AuditService.AddedLog(entity);
                }
                else
                {
                    entity.ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(entity);
                }
                _employeeLandLoardInfo.InsertOrUpdateGraph(entity);
                _unitOfWork.BeginTransaction();
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


        public void DeleteNominee(string id)
        {
            var flag = false;
            try
            {
                flag = true;
                var data = _employeeNomineeInfo.Query(r => r.Id == id).Select().FirstOrDefault();
                _employeeNomineeInfo.Delete(data);
                _unitOfWork.BeginTransaction();
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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

        public void DeleteDependant(string id)
        {
            var flag = false;
            try
            {
                flag = true;
                var data = _employeeDependantInfo.Query(r => r.Id == id).Select().FirstOrDefault();
                _employeeDependantInfo.Delete(data);
                _unitOfWork.BeginTransaction();
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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

        public void DeleteLandLoard(string id)
        {
            var flag = false;
            try
            {
                flag = true;
                var data = _employeeLandLoardInfo.Query(r => r.Id == id).Select().FirstOrDefault();
                _employeeLandLoardInfo.Delete(data);
                _unitOfWork.BeginTransaction();
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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


        private DataSet PlantWiseDOJ(string plantId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT  IsPastDOJAllowed FROM dbo.PlantWiseHRMSSetting WHERE PlantId='" + plantId + @"' AND IsPastDOJAllowed=1"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet PlantWiseDOJDays(string plantId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT  PastDOJDaysAllowed FROM dbo.PlantWiseHRMSSetting WHERE PlantId='" + plantId + @"' AND IsPastDOJAllowed=1"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet EmployeeDocFile(string strSystemID)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM EmployeeDocument WHERE ComplianceDocumentId=(SELECT top(1) Id FROM HKP.ComplianceDocument WHERE ProfileType ='Photo') AND EmpSystemId ='" + strSystemID + @"'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }


        public IWorkbook EmployeeAppointmentLetterLocal(string companyGroupId, string companyId, string plantId, string empId, string empType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];
                workbook = CreateSheetMain_backup(ref sheet1, report, "Appointment Letter", "Appointment Letter", companyGroupId, companyId, plantId, empId, empType, tempId);
                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
               Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
               ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void EmployeeAppointmentLetterInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                CreateIDCardInWord(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public void EmployeeFixationFormInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                CreateFixationFormInWord(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public void EmployeeIncrementHistory(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                IncrementHistory(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void EmployeeExitInterview(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                ExitInterview(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void EmployeeServiceBookInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                CreateServiceBookInWord(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void EmployeeNomineeInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                CreateNomineeInWord(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void EmployeeJoiningLetterInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                CreateJoiningLetterInWord(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void EmployeeAcknowledgementInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                EmployeeAcknowledgementInMSWordFun(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void ConfirmationletterInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                DataSet dsMaster;
                string sql = "select EmployeeName,EmployeeCode,SystemId from EmployeeInformation where IsConfirmed=0 and SystemId= '" + empId + @"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("This Employee Not Confirmed Yet..");
                    throw (ex);
                }

                ConfirmationletterInMSWordFun(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private void CreateServiceBookInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = "+ languageId + @";
                string reportTypeName = "";

                if (reportType == LetterType.ServiceBook.ToString())
                {
                    reportTypeName = LetterType.ServiceBook.GetDescription();
                }

                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Srv" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }
                //------

                ///-----
                var Templatefile = GetFilePath(plantId, tempId, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "Srv" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

                bool IsDefLan = false;
                var tokens = (fileName.Substring(("Srv" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }

                DataTable dtEmp = GetEmployeeBasicInfoById(empId, plantId, empType, langID, tempId);
                DataTable dtSalary = SalaryDetailsForSB(empId, langID); // GetGrossAmount(empId);
                DataTable dtDisciplinaryAction = EmployeeDisciplinaryAction(empId, langID); // GetGrossAmount(empId);
                DataTable dtClanderYear = GetCurrentClanderYear(plantId);

                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                foreach (TextSelection item in allresult)
                {
                    string foundText = item.SelectedText;

                    if (replaced.ContainsKey(foundText) == false)
                        replaced.Add(foundText, 0);

                    //for fixed info
                    string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                    if (dtEmp.Columns.Contains(colName))
                    {

                        ////===== def lan 
                        if (IsDefLan == true)
                        {
                            if (IsDefLan == true)
                            {
                                colName = GetBasicInfoInDefaultLng(colName);
                            }
                        }
                        ///=====
                        value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                        if (bplib.clsWebLib.IsNumeric(value))
                            replaced[foundText] = document.Replace(foundText, cnDgt(value, tempId), false, true);
                        else if (bplib.clsWebLib.IsDateOK(value))
                            replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, tempId), false, true);
                        else
                            replaced[foundText] = document.Replace(foundText, value, false, true);
                    }

                }

                //document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), lang["Language"].ToString()), false, true);
                WSection section = document.Sections[0];
                //WTable wTable = (WTable)section.Body.Tables[0];

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeePic"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["EmployeePic"].ToString();
                    string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 120, 120);



                            //ImgwPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[0].Rows[1].Cells[3].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["AuthorizedSignature"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["AuthorizedSignature"].ToString();
                    string picpath = ResourcesPathReader.GetAuthorizedSignaturePath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[3].Rows[0].Cells[1].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }

                }

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["CardHolderSignature"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["CardHolderSignature"].ToString();
                    string picpath = ResourcesPathReader.GetCardHolderSignaturePath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[3].Rows[0].Cells[0].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeFingerPrint"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["EmployeeFingerPrint"].ToString();
                    string picpath = ResourcesPathReader.GetEmployeeFingerPrintForSBPath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[2].Rows[8].Cells[2].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }


                }

                WTable table1 = (WTable)section.Body.Tables[5];

                //TextSelection allresult1 = table1.Find(new Regex("{.*?}"));
                WTableRow copiedRow = table1.Rows[4].Clone();

                var salarydistinctIds = dtSalary.AsEnumerable()
                   .Select(s => new
                   {
                       id = s.Field<string>("SystemId"),
                   })
                   .Distinct().ToList();

                int index = 0;
                foreach (var item in salarydistinctIds)
                {
                    dtSalary.DefaultView.RowFilter = "SystemId='" + item.id + "'";
                    DataView dvr = new DataView(dtSalary.DefaultView.ToTable());

                    double totalOthers = 0;
                    double gross = 0;

                    WTableRow row;
                    //if (index == 0)
                    //    row = table1.AddRow();
                    //else
                    //{
                    if (index > 0)
                    {
                        row = copiedRow.Clone();
                        table1.Rows.Add(row);
                    }

                    index++;
                    for (int ROW = 0; ROW < dvr.Count; ROW++)
                    {
                        int isReplaced = 0;

                        isReplaced = table1.Replace("{" + dvr[ROW]["SalaryHead"].ToString() + "}", cnDgt(dvr[ROW]["EntryAmount"].ToString(), tempId), false, true);
                        if (isReplaced == 0
                            && dvr[ROW]["SalaryHead"].ToString().ToUpper() != ("Gross").ToUpper()
                            && dvr[ROW]["SalaryHead"].ToString().ToUpper() != ("CTC").ToUpper()
                            && dvr[ROW]["SalaryHead"].ToString().ToUpper() != ("Total Gross").ToUpper()
                            && dvr[ROW]["SalaryHead"].ToString().ToUpper() != ("Net Payable").ToUpper()
                            && dvr[ROW]["HeadType"].ToString() == "E")
                        {
                            totalOthers += Convert.ToDouble(dvr[ROW]["EntryAmount"].ToString());

                        }

                        table1.Replace("{DesignationName}", dvr[ROW]["DesignationName"].ToString(), false, true);
                        table1.Replace("{EffectiveDate}", GetFormatedDate(dvr[ROW]["EffectiveDate"].ToString(), tempId), false, true);

                    }
                    table1.Replace("{Gross}", cnDgt(totalOthers.ToString(), tempId), false, true);

                }

                #region Disciplinary 

                WTable table2 = (WTable)section.Body.Tables[7];
                WTableRow copiedRow2 = table2.Rows[1].Clone();

                WTableRow row2;

                for (int ROW = 0; ROW < dtDisciplinaryAction.Rows.Count; ROW++)
                {
                    if (ROW > 0)
                    {
                        row2 = copiedRow2.Clone();
                        table2.Rows.Add(row2);
                    }

                    table2.Replace("{EntryDate}", GetFormatedDate(dtDisciplinaryAction.Rows[ROW]["EntryDate"].ToString(), tempId), false, true);

                    table2.Replace("{Description}", dtDisciplinaryAction.Rows[ROW]["Description"].ToString(), false, true);

                }

                #endregion

                #region LeaveInformation

                WTable table3 = (WTable)section.Body.Tables[6];
                WTableRow copiedRow3 = table3.Rows[2].Clone();
                WTableRow row3;

                for (int i = 0; i < dtClanderYear.Rows.Count; i++)
                {
                    DataTable dtloadLeaveTransactions = LeaveSummaryForServiceBook(empId, dtClanderYear.Rows[i]["YearNo"].ToString());
                    DataTable dtLoadLeave = loadBf(empId, dtClanderYear.Rows[i]["Id"].ToString());

                    for (int ROW = 0; ROW < dtloadLeaveTransactions.Rows.Count; ROW++)
                    {

                        if (ROW > 0)
                        {
                            row3 = copiedRow3.Clone();
                            table3.Rows.Add(row3);
                        }

                        if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["FromDate"].ToString()))
                        {
                            table3.Replace("{FromDate}", GetFormatedDate(dtloadLeaveTransactions.Rows[ROW]["FromDate"].ToString(), tempId), false, true);
                        }

                        if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["ToDate"].ToString()))
                        {
                            table3.Replace("{ToDate}", GetFormatedDate(dtloadLeaveTransactions.Rows[ROW]["ToDate"].ToString(), tempId), false, true);
                        }


                        if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["Availed"].ToString()))
                        {
                            table3.Replace("{LeaveDays}", cnDgt(dtloadLeaveTransactions.Rows[ROW]["Availed"].ToString(), tempId), false, true);
                        }

                        if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["Balance"].ToString()))
                        {
                            table3.Replace("{Balance}", cnDgt(dtloadLeaveTransactions.Rows[ROW]["Balance"].ToString(), tempId), false, true);
                        }
                        if (i == 0)
                        {
                            if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["EncashmentDayNo"].ToString()))
                            {
                                table3.Replace("{EncashmentDayNo}", cnDgt(dtloadLeaveTransactions.Rows[ROW]["EncashmentDayNo"].ToString(), tempId), false, true);
                            }

                            if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["EncashmentDate"].ToString()))
                            {
                                table3.Replace("{EncashmentDate}", GetFormatedDate(dtloadLeaveTransactions.Rows[ROW]["EncashmentDate"].ToString(), tempId), false, true);
                            }
                        }

                    }
                }


                //for (int ROW = 0; ROW < dtloadLeaveTransactions.Rows.Count; ROW++)
                //{
                //    if (ROW > 0)
                //    {
                //        row3 = copiedRow3.Clone();
                //        table3.Rows.Add(row3);
                //    }

                //    table3.Replace("{FromDate}", GetFormatedDate(dtloadLeaveTransactions.Rows[ROW]["FromDate"].ToString(), tempId), false, true);

                //    table3.Replace("{ToDate}", GetFormatedDate(dtloadLeaveTransactions.Rows[ROW]["ToDate"].ToString(), tempId), false, true);

                //    table3.Replace("{LeaveDays}", cnDgt(dtloadLeaveTransactions.Rows[ROW]["LeaveDays"].ToString(), tempId), false, true);

                //    table3.Replace("{BroughtForward}", cnDgt(dtLoadLeave.Rows[ROW]["BroughtForward"].ToString(), tempId), false, true);

                //}

                //for (int ROW = 0; ROW < dtLoadLeave.Rows.Count; ROW++)
                //{
                //    if (ROW > 0)
                //    {
                //        row3 = copiedRow3.Clone();
                //        table3.Rows.Add(row3);
                //    }
                //    table3.Replace("{BroughtForward}", cnDgt(dtLoadLeave.Rows[ROW]["BroughtForward"].ToString(), tempId), false, true);

                //}

                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);

                }

                #endregion

                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-ServiceBook.docx";

                }
                else
                {
                    fileNames = "-ServiceBook.docx";
                }

                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private void xCreateServiceBookInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = "+ languageId + @";
                string reportTypeName = "";

                if (reportType == LetterType.ServiceBook.ToString())
                {
                    reportTypeName = LetterType.ServiceBook.GetDescription();
                }

                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Srv" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }
                //------

                ///-----
                var Templatefile = GetFilePath(plantId, tempId, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "Srv" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

                bool IsDefLan = false;
                var tokens = (fileName.Substring(("Srv" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }

                ///

                DataTable dtEmp = GetEmployeeBasicInfoById(empId, plantId, empType, langID, tempId);
                DataTable dtSalary = SalaryDetailsForSB(empId, langID); // GetGrossAmount(empId);
                DataTable dtDisciplinaryAction = EmployeeDisciplinaryAction(empId, langID); // GetGrossAmount(empId);

                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                foreach (TextSelection item in allresult)
                {
                    string foundText = item.SelectedText;

                    if (replaced.ContainsKey(foundText) == false)
                        replaced.Add(foundText, 0);

                    //for fixed info
                    string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                    if (dtEmp.Columns.Contains(colName))
                    {

                        ////===== def lan 
                        if (IsDefLan == true)
                        {
                            if (IsDefLan == true)
                            {
                                colName = GetBasicInfoInDefaultLng(colName);
                            }
                        }
                        ///=====
                        value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                        if (bplib.clsWebLib.IsNumeric(value))
                            replaced[foundText] = document.Replace(foundText, cnDgt(value, tempId), false, true);
                        else if (bplib.clsWebLib.IsDateOK(value))
                            replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, tempId), false, true);
                        else
                            replaced[foundText] = document.Replace(foundText, value, false, true);
                    }

                }

                //document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), lang["Language"].ToString()), false, true);
                WSection section = document.Sections[0];
                //WTable wTable = (WTable)section.Body.Tables[0];

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeePic"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["EmployeePic"].ToString();
                    string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 120, 120);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[0].Rows[1].Cells[3].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["AuthorizedSignature"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["AuthorizedSignature"].ToString();
                    string picpath = ResourcesPathReader.GetAuthorizedSignaturePath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[3].Rows[0].Cells[1].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }


                }

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["CardHolderSignature"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["CardHolderSignature"].ToString();
                    string picpath = ResourcesPathReader.GetCardHolderSignaturePath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[3].Rows[0].Cells[0].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }


                }

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeFingerPrint"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["EmployeeFingerPrint"].ToString();
                    string picpath = ResourcesPathReader.GetEmployeeFingerPrintForSBPath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[2].Rows[8].Cells[2].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }


                }

                WTable table1 = (WTable)section.Body.Tables[5];

                //TextSelection allresult1 = table1.Find(new Regex("{.*?}"));
                WTableRow copiedRow = table1.Rows[4].Clone();

                var salarydistinctIds = dtSalary.AsEnumerable()
                   .Select(s => new
                   {
                       id = s.Field<string>("SystemId"),
                   })
                   .Distinct().ToList();

                int index = 0;
                foreach (var item in salarydistinctIds)
                {
                    dtSalary.DefaultView.RowFilter = "SystemId='" + item.id + "'";
                    DataView dvr = new DataView(dtSalary.DefaultView.ToTable());




                    double totalOthers = 0;
                    double gross = 0;

                    WTableRow row;
                    //if (index == 0)
                    //    row = table1.AddRow();
                    //else
                    //{
                    if (index > 0)
                    {
                        row = copiedRow.Clone();
                        table1.Rows.Add(row);
                    }


                    index++;
                    for (int ROW = 0; ROW < dvr.Count; ROW++)
                    {
                        int isReplaced = 0;

                        isReplaced = table1.Replace("{" + dvr[ROW]["SalaryHead"].ToString() + "}", cnDgt(dvr[ROW]["EntryAmount"].ToString(), tempId), false, true);
                        if (isReplaced == 0 && dvr[ROW]["SalaryHead"].ToString().ToUpper() != ("Gross").ToUpper() && dvr[ROW]["HeadType"].ToString() == "E")
                        {
                            totalOthers += Convert.ToDouble(dvr[ROW]["EntryAmount"].ToString());

                        }

                        table1.Replace("{DesignationName}", dvr[ROW]["DesignationName"].ToString(), false, true);
                        table1.Replace("{EffectiveDate}", GetFormatedDate(dvr[ROW]["EffectiveDate"].ToString(), tempId), false, true);

                    }
                    table1.Replace("{Others}", cnDgt(totalOthers.ToString(), tempId), false, true);

                }
                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);

                }

                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-ServiceBook.docx";

                }
                else
                {
                    fileNames = "-ServiceBook.docx";
                }

                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private void CreateNomineeInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                string language = "";
                string filepath = "";


                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    language = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    language = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Nom" + plantId + language + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }
                DataTable dtEmp = GetEmployeeById(empId, plantId, empType, langID, tempId);


                var Templatefile = GetAppointmentFilePath(plantId, language, tempId, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                filepath = "";
                if (System.IO.File.Exists(strPath))
                {
                    filepath = strPath;
                }

                FileInfo DocFile = new FileInfo(filepath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }
                //=====
                bool IsDefLan = false;


                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";

                for (int i = 0; i < dtEmp.Columns.Count; i++)
                {
                    string colName = "{" + dtEmp.Columns[i].ColumnName + "}";
                    value = dtEmp.Rows[0][dtEmp.Columns[i].ColumnName].ToString();

                    if (bplib.clsWebLib.IsNumeric(value))
                        document.Replace(colName, cnDgt(value, language), false, true);
                    else if (bplib.clsWebLib.IsDateOK(value))
                        document.Replace(colName, GetFormatedDate(value, language), false, true);
                    else
                        document.Replace(colName, value, false, true);
                }

                document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), language), false, true);


                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-Nominee.docx";

                }
                else
                {
                    fileNames = "-Nominee.docx";
                }

                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void CreateJoiningLetterInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = "+ languageId + @";
                //string reportTypeName = "";

                //if (reportType == LetterType.JoiningLetter.ToString())
                //{
                //    reportTypeName = LetterType.JoiningLetter.GetDescription();
                //}
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Joi" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }

                }
                //------

                ///-----
                var Templatefile = GetFilePath(plantId, tempId, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "Joi" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }
                bool IsDefLan = false;

                var tokens = (fileName.Substring(("Ack" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }

                ///

                DataTable dtEmp = GetEmployeeBasicInfoById(empId, plantId, empType, langID, tempId);



                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] X = document.FindAll(new Regex("{.*?}")).ToArray();
                List<string> allresult = new List<string>();
                for (int i = 0; i < X.Length; i++)
                    allresult.Add(X[i].SelectedText);


                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                for (int i = 0; i < allresult.Count; i++)
                {
                    try
                    {
                        string foundText = allresult[i];

                        if (replaced.ContainsKey(foundText) == false)
                            replaced.Add(foundText, 0);

                        //for fixed info
                        string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                        if (dtEmp.Columns.Contains(colName))
                        {
                            ////===== def lan 
                            if (IsDefLan == true)
                            {
                                colName = GetBasicInfoInDefaultLng(colName);
                            }
                            ///=====
                            value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                            if (bplib.clsWebLib.IsNumeric(value))
                                replaced[foundText] = document.Replace(foundText, cnDgt(value, tempId), false, true);
                            else if (bplib.clsWebLib.IsDateOK(value))
                                replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, tempId), false, true);
                            else
                                replaced[foundText] = document.Replace(foundText, value, false, false);
                        }
                    }
                    catch (Exception)
                    {


                    }



                }

                document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), tempId), false, true);
                WSection section = document.Sections[0];
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["CardHolderSignature"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["CardHolderSignature"].ToString();
                    string picpath = ResourcesPathReader.GetCardHolderSignaturePath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[0].Rows[0].Cells[0].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);
                }


                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-JoiningLetter.docx";

                }
                else
                {
                    fileNames = "-JoiningLetter.docx";
                }

                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void EmployeeAcknowledgementInMSWordFun(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = "+ languageId + @";
                bool IsDefLan = false;
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Ack" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }

                }
                //------


                ///-----
                var Templatefile = GetFilePath(plantId, tempId, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "Ack" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }
                ///

                var tokens = (fileName.Substring(("Ack" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }

                ///
                DataTable dtEmp = GetEmployeeBasicInfoById(empId, plantId, empType, langID, tempId);



                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                //TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));


                TextSelection[] X = document.FindAll(new Regex("{.*?}")).ToArray();
                List<string> allresult = new List<string>();
                for (int i = 0; i < X.Length; i++)
                    allresult.Add(X[i].SelectedText);


                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                for (int i = 0; i < allresult.Count; i++)
                {
                    try
                    {
                        string foundText = allresult[i];

                        if (replaced.ContainsKey(foundText) == false)
                            replaced.Add(foundText, 0);

                        //for fixed info
                        string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                        if (dtEmp.Columns.Contains(colName))
                        {
                            ////===== def lan 
                            if (IsDefLan == true)
                            {
                                if (IsDefLan == true)
                                {
                                    colName = GetBasicInfoInDefaultLng(colName);
                                }
                            }
                            ///=====
                            value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                            if (bplib.clsWebLib.IsNumeric(value))
                                replaced[foundText] = document.Replace(foundText, cnDgt(value, tempId), false, true);
                            else if (bplib.clsWebLib.IsDateOK(value))
                                replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, tempId), false, true);
                            else
                                replaced[foundText] = document.Replace(foundText, value, false, false);
                        }
                    }
                    catch (Exception)
                    {


                    }



                }

                document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), tempId), false, true);

                WSection section = document.Sections[0];
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["CardHolderSignature"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["CardHolderSignature"].ToString();
                    string picpath = ResourcesPathReader.GetCardHolderSignaturePath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[0].Rows[0].Cells[0].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);
                }


                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-Acknowledgement.docx";

                }
                else
                {
                    fileNames = "-Acknowledgement.docx";
                }

                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void ConfirmationletterInMSWordFun(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                bool IsDefLan = false;
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {//
                    var dtLangId = getLanguageId(lang["Language"].ToString());
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "EmployeeConfirmationLetter" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }

                }

                var Templatefile = GetFilePath(plantId, tempId, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "EmployeeConfirmationLetter" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {

                    throw new CustomException("File Not Found");
                }

                DataTable dtEmp = GetEmployeeconfirmationBasicInfoById(empId, plantId, empType, langID, tempId);
                DataTable dtSalary = SalaryDetailsForApp(empId, langID); // GetGrossAmount(empId);
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] X = document.FindAll(new Regex("{.*?}")).ToArray();
                List<string> allresult = new List<string>();
                for (int i = 0; i < X.Length; i++)
                    allresult.Add(X[i].SelectedText);


                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                for (int i = 0; i < allresult.Count; i++)
                {
                    try
                    {
                        string foundText = allresult[i];

                        if (replaced.ContainsKey(foundText) == false)
                            replaced.Add(foundText, 0);

                        string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                        if (dtEmp.Columns.Contains(colName))
                        {

                            value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                            if (bplib.clsWebLib.IsNumeric(value))
                                replaced[foundText] = document.Replace(foundText, cnDgt(value, tempId), false, true);
                            else if (bplib.clsWebLib.IsDateOK(value))
                                replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, tempId), false, true);
                            else
                                replaced[foundText] = document.Replace(foundText, value, false, false);
                        }
                    }
                    catch (Exception)
                    {
                    }

                }

                for (int ROW = 0; ROW < dtSalary.Rows.Count; ROW++)
                {
                    int isReplaced = 0;

                    isReplaced = document.Replace("{" + dtSalary.Rows[ROW]["SalaryHead"].ToString() + "}", cnDgt(dtSalary.Rows[ROW]["EntryAmount"].ToString(), tempId), false, false);
                    //if (isReplaced == 0 && dtSalary.Rows[ROW]["SalaryHead"].ToString().ToUpper() != ("Gross").ToUpper() && dtSalary.Rows[ROW]["HeadType"].ToString() == "E")
                    //{
                    //    totalOthers += Convert.ToDouble(dtSalary.Rows[ROW]["EntryAmount"].ToString());

                    //}
                    //if (isReplaced == 0
                    //    && dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() != ("Gross").ToUpper()
                    //    && dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() != ("TOTAL GROSS").ToUpper()
                    //    && dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() != ("CTC").ToUpper()
                    //    && dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() != ("Net Payable").ToUpper()
                    //    && dtSalary.Rows[ROW]["HeadType"].ToString() == "E")
                    //{
                    //    totalOthers += Convert.ToDouble(dtSalary.Rows[ROW]["EntryAmount"].ToString());
                    //}

                    //numberToString.numberToStringBuilder bangla = new numberToString.numberToStringBuilder();
                    //if (dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() == "GROSS" && dtSalary.Rows[ROW]["HeadType"].ToString() == "E")
                    //{
                    //    document.Replace("{GrossInWord}", bangla.strnumberToString(dtSalary.Rows[ROW]["EntryAmount"].ToString()), true, true);
                    //}




                }





                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {
                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "EmployeeConfirmationLetter-.docx";
                }
                else
                {
                    fileNames = "EmployeeConfirmationLetter-.docx";
                }

                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public PdfDocument PrintEmployeeIDCard(string empId, string companyGroupId, string companyId, string plantId, string tempId, string empType, string reportType, string issuDate, string workTypeId)
        {
            try
            {
                ConvertExcelToImage convertExcelToImage = null;
                var excelEngine = new ExcelEngine();
                var report = new ReportUtility();
                IApplication application = null;
                application = excelEngine.Excel;
                var workbook = report.GetWorkbook(ref excelEngine, 1);
                workbook = application.Workbooks.Create(2);
                var sheet1 = workbook.Worksheets[0];
                var sheet2 = workbook.Worksheets[1];
                workbook = CreateIDCardSheet(ref sheet1, ref sheet2, report, "IDCARD", "IDCARD", empId, companyGroupId, companyId, plantId, tempId, empType, reportType, issuDate, workTypeId);
                //return workbook;
                List<IWorkbook> workbookList = new List<IWorkbook>();
                workbookList.Add(workbook);
                convertExcelToImage = new ConvertExcelToImage(workbookList, 85f, 54f);
                PdfDocument doc = convertExcelToImage.ConvertToPdf(10f);
                return doc;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
               Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
               ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IWorkbook PrintEmployeeIDCardAll(string empId, IWorkbook workbook, DataRow dr, string companyGroupId, string companyId, string plantId, string tempId, string empType, string reportType, string issuDate, string workTypeId, string langName)
        {
            try
            {
                // var dtEmp = GetMultipleEmployeeInfoById(empId, plantId, langID, tempId); // Get Employee Data

                var sheet1 = workbook.Worksheets[0];
                var sheet2 = workbook.Worksheets[1];
                workbook = CreateIDCardSheetAll(ref sheet1, ref sheet2, workbook, dr, "IDCARD", "IDCARD", empId, companyGroupId, companyId, plantId, tempId, empType, reportType, issuDate, workTypeId, langName);

                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
               Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
               ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IPresentation PrintEmployeeIDCardPpt(string empId, string companyGroupId, string companyId, string plantId, string tempId, string empType, string reportType, string issuDate, string workTypeId)
        {
            try
            {

                IPresentation Presentation = CreateIDCardSheetPpt(empId, companyGroupId, companyId, plantId, tempId, empType, reportType, issuDate, workTypeId);


                return Presentation;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
               Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
               ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IPresentation PrintEmployeeIDCardAllPpt(string empId, IPresentation workbook, DataRow dr, string companyGroupId, string companyId, string plantId, string tempId, string empType, string reportType, string issuDate, string workTypeId, string langName, bool IsCurrentIssueDate)
        {
            try
            {

                workbook = CreateIDCardSheetAllPpt(workbook, dr, "IDCARD", "IDCARD", empId, companyGroupId, companyId, plantId, tempId, empType, reportType, issuDate, workTypeId, langName, IsCurrentIssueDate);

                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
               Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
               ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IPresentation EmployeeMultipleIDCardPpt(string empId, string companyGroupId, string companyId, string plantId, string tempId, string issuDate, string workTypeId, List<Dictionary<string, object>> dataList, bool IsCurrentIssueDate)
        {
            try
            {
                string langID = "";
                string langName = "";
                string reportType = "IdCard";
                string File = "";
                string strPath = "";
                var fileName = "";
                var c = empId.Split(new char[] { ' ', '.', ',', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;


                //tempId = "M6";
                var lang = GetLanguage(plantId, tempId, reportType);
                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    langName = dtLangName.Rows[0]["UserName"].ToString();
                }

                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    langName = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "IdCard" + plantId + langName + ".pptx";

                }
                var dtEmp = GetMultipleEmployeeInfoById(empId, plantId, langID, tempId); // Get Employee Data
                var Templatefile = GetIdCardFilePath(plantId, langName, reportType, tempId);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }


                IPresentation pptxDoc = Presentation.Open(strPath);
                while (pptxDoc.Slides.Count > 0)
                    pptxDoc.Slides.RemoveAt(0);




                for (int i = 0; i < dtEmp.Rows.Count; i++)
                {
                    IPresentation presentation = Presentation.Open(strPath);
                    presentation = PrintEmployeeIDCardAllPpt(dtEmp.Rows[i]["EmployeeCode"].ToString(), presentation, dtEmp.Rows[i], companyGroupId, companyId, plantId, tempId, "", "IdCard", issuDate, dtEmp.Rows[i]["EmployeeWorkType"].ToString(), langName, IsCurrentIssueDate);
                    for (int x = 0; x < presentation.Slides.Count; x++)
                    {
                        pptxDoc.Slides.Add(presentation.Slides[x]);
                    }
                }


                return pptxDoc;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
               Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
               ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private void IterateTextBody(WTextBody textBody, DataTable dt1, DataTable dt2)
        {
            for (int i = 0; i < textBody.ChildEntities.Count; i++)
            {
                IEntity bodyItemEntity = textBody.ChildEntities[i];
                changeFixedColumns(bodyItemEntity, dt1);
                changeSalaryHeads(bodyItemEntity, dt2);
            }

        }

        private void changeFixedColumns(IEntity bodyItemEntity, DataTable dt)
        {
            string key = ""; string value = "";
            for (int COL = 0; COL < dt.Columns.Count; COL++)
            {
                key = "{" + dt.Columns[COL].ColumnName + "}";
                value = dt.Rows[0][dt.Columns[COL].ColumnName].ToString();
                if (((Syncfusion.DocIO.DLS.WParagraph)bodyItemEntity).Text.Contains(key))
                    ((Syncfusion.DocIO.DLS.WParagraph)bodyItemEntity).Text = ((Syncfusion.DocIO.DLS.WParagraph)bodyItemEntity).Text.Replace(key, value);

            }
        }

        private void changeSalaryHeads(IEntity bodyItemEntity, DataTable dt)
        {
            string key = ""; string value = "";
            for (int ROW = 0; ROW < dt.Rows.Count; ROW++)
            {
                key = "{" + dt.Rows[ROW]["SalaryHead"].ToString() + "}";
                value = dt.Rows[ROW]["EntryAmount"].ToString();
                if (((Syncfusion.DocIO.DLS.WParagraph)bodyItemEntity).Text.Contains(key))
                    ((Syncfusion.DocIO.DLS.WParagraph)bodyItemEntity).Text = ((Syncfusion.DocIO.DLS.WParagraph)bodyItemEntity).Text.Replace(key, value);

            }
        }

        private IWorkbook CreateSheetMain_backup(ref IWorksheet sheet1, ReportUtility report, string sheetHeader, string sheetName, string companyGroupId, string companyId, string plantId, string empId, string empType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                var reportType = "";
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "App" + plantId + tempId + ".xls";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }

                //if (lang.Count > 0)
                //{
                //    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                //    langID = dtLangId.Rows[0]["Id"].ToString();
                //}
                //else
                //{
                //    langID = tempId;
                //}

                var dtEmp = GetEmployeeById(empId, plantId, empType, langID, tempId);
                var dtSalary = SalaryDetails(empId); // GetGrossAmount(empId);


                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook1 = null;

                var Templatefile = GetFilePath(plantId, tempId, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    workbook1 = excelEngine.Excel.Workbooks.Open(strPath);
                    var newdate = GetFormatedDate(DateTime.Now.ToString("dd-MMM-yyyy"), tempId);

                    workbook1.Worksheets[0].Replace("{Date}", newdate);
                    workbook1.Worksheets[0].Replace("{CompanyName}", dtEmp.Rows[0]["CompanyName"].ToString());
                    workbook1.Worksheets[0].Replace("{Address}", dtEmp.Rows[0]["CompanyAddress"].ToString());
                    workbook1.Worksheets[0].Replace("{EmployeeName}", dtEmp.Rows[0]["EmployeeName"].ToString());
                    workbook1.Worksheets[0].Replace("{FatherName}", dtEmp.Rows[0]["FatherName"].ToString());
                    workbook1.Worksheets[0].Replace("{MotherName}", dtEmp.Rows[0]["MotherName"].ToString());
                    workbook1.Worksheets[0].Replace("{PresentAddress}", dtEmp.Rows[0]["PresentAddress1"].ToString());
                    workbook1.Worksheets[0].Replace("{PermanentAddress}", dtEmp.Rows[0]["ParmanentAddress1"].ToString());
                    workbook1.Worksheets[0].Replace("{CITY}", dtEmp.Rows[0]["PresentCity"].ToString());
                    workbook1.Worksheets[0].Replace("{COUNTRY}", dtEmp.Rows[0]["LPresentCountry"].ToString());
                    workbook1.Worksheets[0].Replace("{FIRSTNAME}", dtEmp.Rows[0]["FirstName"].ToString());
                    workbook1.Worksheets[0].Replace("{Designation}", dtEmp.Rows[0]["DesignationName"].ToString());
                    var doj = GetFormatedDate(dtEmp.Rows[0]["DateOfJoin"].ToString(), tempId);
                    workbook1.Worksheets[0].Replace("{DOJ}", doj);
                    workbook1.Worksheets[0].Replace("{ProbationPeriod}", dtEmp.Rows[0]["confirm"].ToString());
                    workbook1.Worksheets[0].Replace("{Department}", dtEmp.Rows[0]["Department"].ToString());
                    workbook1.Worksheets[0].Replace("{Section}", dtEmp.Rows[0]["Section"].ToString());
                    workbook1.Worksheets[0].Replace("{Unit}", dtEmp.Rows[0]["Unit"].ToString());
                    workbook1.Worksheets[0].Replace("{MedicalAllowance}", "0");
                    workbook1.Worksheets[0].Replace("{FoodAllowance}", "0");


                    if (dtSalary.Rows.Count > 0)
                    {
                        double _totalAmount = 0;
                        for (int i = 0; i < dtSalary.Rows.Count; i++)
                        {

                            if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Basic")
                            {
                                workbook1.Worksheets[0].Replace("{BasicSalary}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                            }
                            if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Conveyance Allowance")
                            {
                                workbook1.Worksheets[0].Replace("{Conveyance}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                            }

                            if (dtSalary.Rows[i]["SalaryHead"].ToString() != null)
                            {
                                if (dtSalary.Rows[i]["SalaryHead"].ToString() == "House Rent")
                                {
                                    workbook1.Worksheets[0].Replace("{HRA}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                    _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                                }
                            }
                            if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Other")
                            {
                                workbook1.Worksheets[0].Replace("{Others}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                            }


                        }//loop
                         //if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Gross")
                         //{
                        workbook1.Worksheets[0].Replace("{Gross}", _totalAmount.ToString());
                        //}


                    }


                    workbook1.Version = ExcelVersion.Excel97to2003;

                }
                else
                {
                    File = "App" + plantId + "English.xls";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                    workbook1 = excelEngine.Excel.Workbooks.Open(strPath);
                    string cn = dtEmp.Rows[0]["CompanyName"].ToString();
                    workbook1.Worksheets[0].Replace("{CompanyName}", cn);
                    workbook1.Worksheets[0].Replace("{Address}", dtEmp.Rows[0]["UtilityName"].ToString());
                    workbook1.Worksheets[0].Replace("{EmployeeName}", dtEmp.Rows[0]["EmployeeName"].ToString());
                    workbook1.Worksheets[0].Replace("{FatherName}", dtEmp.Rows[0]["FatherName"].ToString());
                    workbook1.Worksheets[0].Replace("{MotherName}", dtEmp.Rows[0]["MotherName"].ToString());
                    string address = "";
                    if (dtEmp.Rows[0]["PresentCity"].ToString() != "")
                    {
                        address = dtEmp.Rows[0]["PresentCity"].ToString() + @", " + dtEmp.Rows[0]["PresentDistrict"].ToString() + @", " + dtEmp.Rows[0]["PresentState"].ToString() + @", " + dtEmp.Rows[0]["LPresentCountry"].ToString();
                    }
                    else
                    {
                        address = dtEmp.Rows[0]["PresentAddress1"].ToString();
                    }

                    workbook1.Worksheets[0].Replace("{EmployeeAddress}", address);
                    workbook1.Worksheets[0].Replace("{CITY}", dtEmp.Rows[0]["PresentCity"].ToString());
                    workbook1.Worksheets[0].Replace("{COUNTRY}", dtEmp.Rows[0]["LPresentCountry"].ToString());
                    workbook1.Worksheets[0].Replace("{FIRSTNAME}", dtEmp.Rows[0]["FirstName"].ToString());
                    workbook1.Worksheets[0].Replace("{Designation}", dtEmp.Rows[0]["DesignationName"].ToString());
                    workbook1.Worksheets[0].Replace("{DOJ}", dtEmp.Rows[0]["DOJ"].ToString());
                    workbook1.Worksheets[0].Replace("{ProbationPeriod}", dtEmp.Rows[0]["confirm"].ToString());
                    workbook1.Worksheets[0].Replace("{MedicalAllowance}", "0");
                    workbook1.Worksheets[0].Replace("{FoodAllowance}", "0");
                    if (dtSalary.Rows.Count > 0)
                    {
                        double _totalAmount = 0;
                        for (int i = 0; i < dtSalary.Rows.Count; i++)
                        {

                            if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Basic")
                            {
                                workbook1.Worksheets[0].Replace("{BasicSalary}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                            }
                            if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Conveyance Allowance")
                            {
                                workbook1.Worksheets[0].Replace("{Conveyance}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                            }

                            if (dtSalary.Rows[i]["SalaryHead"].ToString() != null)
                            {
                                if (dtSalary.Rows[i]["SalaryHead"].ToString() == "House Rent")
                                {
                                    workbook1.Worksheets[0].Replace("{HRA}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                    _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                                }
                            }
                            if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Other")
                            {
                                workbook1.Worksheets[0].Replace("{Others}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                            }
                        }
                        workbook1.Worksheets[0].Replace("{Gross}", _totalAmount.ToString());
                    }
                    workbook1.Worksheets[0].Replace("{Date}", DateTime.Now.ToString("dd-MM-yyyy"));


                }
                return workbook1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void CreateIDCardInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                string language = "";
                var fileName = "";
                string filepath = "";
                var lang = GetLanguage(plantId, tempId, reportType);


                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    language = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    language = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "App" + plantId + language + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }
                DataTable dtEmp = GetEmployeeById(empId, plantId, empType, langID, tempId);

                var Templatefile = GetAppointmentFilePath(plantId, language, tempId, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                filepath = "";
                if (System.IO.File.Exists(strPath))
                {
                    filepath = strPath;
                }

                FileInfo DocFile = new FileInfo(filepath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

                bool IsDefLan = false;

                var tokens = (fileName.Substring(("App" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);


                DataTable dtSalary = SalaryDetailsForAppLaila(empId, langID); // GetGrossAmount(empId);
                decimal OTRate = GetOTRate(empId);
                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);



                TextSelection[] X = document.FindAll(new Regex("{.*?}")).ToArray();
                List<string> allresult = new List<string>();
                for (int i = 0; i < X.Length; i++)
                    allresult.Add(X[i].SelectedText);


                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                for (int i = 0; i < allresult.Count; i++)
                {
                    try
                    {
                        string foundText = allresult[i];

                        if (replaced.ContainsKey(foundText) == false)
                            replaced.Add(foundText, 0);

                        //for fixed info
                        string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                        if (dtEmp.Columns.Contains(colName))
                        {

                            value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                            if (bplib.clsWebLib.IsNumeric(value))
                                replaced[foundText] = document.Replace(foundText, cnDgt(value, language), false, false);
                            else if (bplib.clsWebLib.IsDateOK(value))
                                replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, language), false, false);
                            else
                                replaced[foundText] = document.Replace(foundText, value, false, false);
                        }


                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }
                try
                {
                    WSection section = document.Sections[0];
                    WTable table1 = (WTable)section.Body.Tables[1];

                    double totalOthers = 0;
                    int x = table1.Replace("{SalaryGrade}", dtSalary.Rows[0]["Grade"].ToString(), false, false);
                    int y = table1.Replace("{OTRate}", cnDgt(OTRate.ToString(), language), false, false);

                    for (int ROW = 0; ROW < dtSalary.Rows.Count; ROW++)
                    {
                        int isReplaced = 0;

                        isReplaced = table1.Replace("{" + dtSalary.Rows[ROW]["SalaryHead"].ToString() + "}", cnDgt(dtSalary.Rows[ROW]["EntryAmount"].ToString(), language), false, false);

                        if (isReplaced == 0
                            && dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() != ("Gross").ToUpper()
                            && dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() != ("TOTAL GROSS").ToUpper()
                            && dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() != ("CTC").ToUpper()
                            && dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() != ("Net Payable").ToUpper()
                            && dtSalary.Rows[ROW]["HeadType"].ToString() == "E")
                        {
                            totalOthers += Convert.ToDouble(dtSalary.Rows[ROW]["EntryAmount"].ToString());
                        }

                        numberToString.numberToStringBuilder bangla = new numberToString.numberToStringBuilder();
                        if (dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() == "GROSS" && dtSalary.Rows[ROW]["HeadType"].ToString() == "E")
                        {
                            document.Replace("{GrossInWord}", bangla.strnumberToString(dtSalary.Rows[ROW]["EntryAmount"].ToString()), true, true);
                        }
                    }
                    table1.Replace("{Others}", cnDgt(totalOthers.ToString(), language), false, false);

                }
                catch (Exception ex)
                {
                    if (dtSalary.Rows.Count > 0)
                    {
                        for (int ROW = 0; ROW < dtSalary.Rows.Count; ROW++)
                        {
                            int isReplaced = 0;

                            isReplaced = document.Replace("{" + dtSalary.Rows[ROW]["SalaryHead"].ToString() + "}", cnDgt(dtSalary.Rows[ROW]["EntryAmount"].ToString(), language), false, false);
                            numberToString.numberToStringBuilder bangla = new numberToString.numberToStringBuilder();
                            if (dtSalary.Rows[ROW]["SalaryHead"].ToString().ToUpper() == "GROSS" && dtSalary.Rows[ROW]["HeadType"].ToString() == "E")
                            {
                                document.Replace("{GrossInWord}", bangla.strnumberToString(dtSalary.Rows[ROW]["EntryAmount"].ToString()), true, true);
                            }

                        }
                    }
                }
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["IssueDate"].ToString()))
                {
                    document.Replace("{Date}", GetFormatedDate(dtEmp.Rows[0]["IssueDate"].ToString(), language), false, true);
                }

                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);

                }

                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-Appointment-Letter.docx";

                }
                else
                {
                    fileNames = "-Appointment-Letter.docx";
                    //fileNames = "-Appointment-Letter";
                }
                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();

                ////Creates an instance of the DocToPDFConverter
                ///

                //DocToPDFConverter converter = new DocToPDFConverter();
                //converter.Settings.EmbedFonts = false;


                ////Converts Word document into PDF document
                //PdfDocument pdfDocument = converter.ConvertToPDF(document);

                ////Releases all resources used by DocToPDFConverter
                //converter.Dispose();

                ////Closes the instance of document objects
                //document.Close();

                ////Saves the PDF file 
                //pdfDocument.Save(fileNames + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                ////Closes the instance of document objects
                //pdfDocument.Close(true);

                //document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        private void CreateFixationFormInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                string language = "";
                var fileName = "";
                string filepath = "";
                var lang = GetLanguage(plantId, tempId, reportType);


                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    language = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    language = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "FF" + plantId + language + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }
                DataTable dtEmp = GetEmployeeById(empId, plantId, empType, langID, tempId);

                var Templatefile = GetAppointmentFilePath(plantId, language, tempId, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                filepath = "";
                if (System.IO.File.Exists(strPath))
                {
                    filepath = strPath;
                }

                FileInfo DocFile = new FileInfo(filepath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

                bool IsDefLan = false;

                var tokens = (fileName.Substring(("FF" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                //if (language != TemplateLan)
                //{
                //    IsDefLan = true;
                //}

                ///


                DataTable dtSalary = SalaryDetailsForApp(empId, langID); // GetGrossAmount(empId);
                decimal OTRate = GetOTRate(empId);
                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);



                TextSelection[] X = document.FindAll(new Regex("{.*?}")).ToArray();
                List<string> allresult = new List<string>();
                for (int i = 0; i < X.Length; i++)
                    allresult.Add(X[i].SelectedText);


                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                for (int i = 0; i < allresult.Count; i++)
                {
                    try
                    {
                        string foundText = allresult[i];

                        if (replaced.ContainsKey(foundText) == false)
                            replaced.Add(foundText, 0);

                        //for fixed info
                        string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                        if (dtEmp.Columns.Contains(colName))
                        {
                            value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                            if (bplib.clsWebLib.IsNumeric(value))
                                replaced[foundText] = document.Replace(foundText, cnDgt(value, language), false, false);
                            else if (bplib.clsWebLib.IsDateOK(value))
                                replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, language), false, false);
                            else
                                replaced[foundText] = document.Replace(foundText, value, false, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                try
                {


                    WSection section = document.Sections[0];
                    WTable table1 = (WTable)section.Body.Tables[1];

                    double totalOthers = 0;
                    int x = table1.Replace("{SalaryGrade}", dtSalary.Rows[0]["Grade"].ToString(), false, false);
                    int y = table1.Replace("{OTRate}", cnDgt(OTRate.ToString(), language), false, false);

                    for (int ROW = 0; ROW < dtSalary.Rows.Count; ROW++)
                    {
                        int isReplaced = 0;

                        isReplaced = table1.Replace("{" + dtSalary.Rows[ROW]["SalaryHead"].ToString() + "}", cnDgt(dtSalary.Rows[ROW]["EntryAmount"].ToString(), language), false, false);
                        //if (isReplaced == 0 && dtSalary.Rows[ROW]["SalaryHead"].ToString().ToUpper() != ("Gross").ToUpper() && dtSalary.Rows[ROW]["HeadType"].ToString() == "E")
                        //{
                        //    totalOthers += Convert.ToDouble(dtSalary.Rows[ROW]["EntryAmount"].ToString());

                        //}
                        if (isReplaced == 0
                            && dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() != ("Gross").ToUpper()
                            && dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() != ("TOTAL GROSS").ToUpper()
                            && dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() != ("CTC").ToUpper()
                            && dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() != ("Net Payable").ToUpper()
                            && dtSalary.Rows[ROW]["HeadType"].ToString() == "E")
                        {
                            totalOthers += Convert.ToDouble(dtSalary.Rows[ROW]["EntryAmount"].ToString());
                        }

                        numberToString.numberToStringBuilder bangla = new numberToString.numberToStringBuilder();
                        if (dtSalary.Rows[ROW]["HeadCategory"].ToString().ToUpper() == "GROSS" && dtSalary.Rows[ROW]["HeadType"].ToString() == "E")
                        {
                            document.Replace("{GrossInWord}", bangla.strnumberToString(dtSalary.Rows[ROW]["EntryAmount"].ToString()), true, true);
                        }




                    }



                    table1.Replace("{Others}", cnDgt(totalOthers.ToString(), language), false, false);
                    if (!string.IsNullOrEmpty(dtEmp.Rows[0]["CardHolderSignature"].ToString()))
                    {
                        var pic = dtEmp.Rows[0]["CardHolderSignature"].ToString();
                        string picpath = ResourcesPathReader.GetCardHolderSignaturePath() + pic;
                        //WPicture ImgwPicture = new WPicture(document);
                        if (System.IO.File.Exists(picpath))
                        {
                            try
                            {
                                Image Img = Image.FromFile(picpath);
                                Image newImage = resizeImage(Img, 60, 100);
                                //wPicture.LoadImage(Image.FromFile(picpath));
                                //TextBodyPart textBodyPart = new TextBodyPart(document);

                                section.Tables[2].Rows[0].Cells[0].Paragraphs[0].AppendPicture(newImage);

                                //document.Replace()
                                //document.Replace("{emppic}", textBodyPart, true, true);
                            }
                            catch (Exception ex)
                            {
                                throw (ex);
                            }
                        }
                    }
                    //}
                }
                catch (Exception ex)
                {
                    if (dtSalary.Rows.Count > 0)
                    {
                        for (int ROW = 0; ROW < dtSalary.Rows.Count; ROW++)
                        {
                            int isReplaced = 0;

                            isReplaced = document.Replace("{" + dtSalary.Rows[ROW]["SalaryHead"].ToString() + "}", cnDgt(dtSalary.Rows[ROW]["EntryAmount"].ToString(), language), false, false);
                            numberToString.numberToStringBuilder bangla = new numberToString.numberToStringBuilder();
                            if (dtSalary.Rows[ROW]["SalaryHead"].ToString().ToUpper() == "GROSS" && dtSalary.Rows[ROW]["HeadType"].ToString() == "E")
                            {
                                document.Replace("{GrossInWord}", bangla.strnumberToString(dtSalary.Rows[ROW]["EntryAmount"].ToString()), true, true);
                            }

                        }
                    }
                }

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["IssueDate"].ToString()))
                {
                    document.Replace("{Date}", GetFormatedDate(dtEmp.Rows[0]["IssueDate"].ToString(), language), false, true);
                }





                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);

                }

                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-Fixation-Form.docx";

                }
                else
                {
                    fileNames = "-Fixation-Form.docx";
                    //fileNames = "-Appointment-Letter";
                }
                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();

                ////Creates an instance of the DocToPDFConverter
                ///

                //DocToPDFConverter converter = new DocToPDFConverter();
                //converter.Settings.EmbedFonts = false;


                ////Converts Word document into PDF document
                //PdfDocument pdfDocument = converter.ConvertToPDF(document);

                ////Releases all resources used by DocToPDFConverter
                //converter.Dispose();

                ////Closes the instance of document objects
                //document.Close();

                ////Saves the PDF file 
                //pdfDocument.Save(fileNames + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                ////Closes the instance of document objects
                //pdfDocument.Close(true);

                //document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }





        private void IncrementHistory(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                string language = "";
                var fileName = "";
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    language = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    language = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "App" + plantId + language + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }

                var Templatefile = GetAppointmentFilePath(plantId, language, tempId, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "IH" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(filepath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }


                DataSet dsCompanyInfo = null;
                string strCompanyInfoSQL = @"select a.Address1 CompanyAddress, c.UserName CompanyName from org.Company c
                                            left join  mst.addressmaster a on a.id=c.AddressMasterId
                                            where c.id='" + identity.CompanyId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strCompanyInfoSQL, out dsCompanyInfo, false, "1");

                DataTable dtEmpInfo = GetEmpInfo(empId, langID);
                // DataTable dtEmpInfo = GetEmployeeById(empId, plantId, empType, langID, tempId);

                DataTable dtEmpHeaderInfo = GetEmpHeaderInfo(empId, langID);

                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);
                WSection section = document.Sections[0];
                //TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                //Dictionary<string, int> replaced = new Dictionary<string, int>();



                if (bplib.clsWebLib.IsNumeric(dtEmpHeaderInfo.Rows[0]["EmployeeCode"].ToString()))

                    document.Replace("{EmployeeCode}", cnDgt(dtEmpHeaderInfo.Rows[0]["EmployeeCode"].ToString(), language), false, true);
                else
                    document.Replace("{EmployeeCode}", dtEmpHeaderInfo.Rows[0]["EmployeeCode"].ToString(), false, true);


                if (bplib.clsWebLib.IsDateOK(dtEmpHeaderInfo.Rows[0]["DateOfJoin"].ToString()))
                    document.Replace("{DateOfJoin}", GetFormatedDate(dtEmpHeaderInfo.Rows[0]["DateOfJoin"].ToString(), language), false, true);
                else
                    document.Replace("{DateOfJoin}", dtEmpHeaderInfo.Rows[0]["DateOfJoin"].ToString(), false, true);
                document.Replace("{EmployeeName}", dtEmpHeaderInfo.Rows[0]["EmployeeName"].ToString(), false, true);
                document.Replace("{DepartmentName}", dtEmpHeaderInfo.Rows[0]["Department"].ToString(), false, true);
                document.Replace("{SectionName}", dtEmpHeaderInfo.Rows[0]["SectionName"].ToString(), false, true);
                document.Replace("{DesignationName}", dtEmpHeaderInfo.Rows[0]["DesignationName"].ToString(), false, true);
                document.Replace("{Category}", dtEmpHeaderInfo.Rows[0]["Category"].ToString(), false, true);


                //makeAppraisalDetailTable(document, dtEmpInfo);


                WTable table2 = (WTable)section.Body.Tables[1];
                WTableRow copiedRow2 = table2.Rows[1].Clone();

                WTableRow row2;

                for (int ROW = 0; ROW < dtEmpInfo.Rows.Count; ROW++)
                {
                    if (ROW > 0)
                    {
                        row2 = copiedRow2.Clone();
                        table2.Rows.Add(row2);
                    }
                    if (bplib.clsWebLib.IsDateOK(dtEmpHeaderInfo.Rows[0]["DateOfJoin"].ToString()))
                        table2.Replace("{AppraisalDate}", GetFormatedDate(dtEmpInfo.Rows[ROW]["AppraisalDate"].ToString(), language), false, true);
                    else

                        table2.Replace("{AppraisalDate}", dtEmpInfo.Rows[ROW]["AppraisalDate"].ToString(), false, true);
                    table2.Replace("{PreviousDepartment}", dtEmpInfo.Rows[ROW]["PreviousDepartment"].ToString(), false, true);
                    table2.Replace("{NewDepartment}", dtEmpInfo.Rows[ROW]["NewDepartment"].ToString(), false, true);

                    table2.Replace("{PreviousGross}", string.Format("{0:N2}", dtEmpInfo.Rows[ROW]["PreviousGross"].ToString()), false, true);
                    table2.Replace("{NewGross}", string.Format("{0:N2}", dtEmpInfo.Rows[ROW]["NewGross"].ToString()), false, true);
                    table2.Replace("{IncrementAmount}", string.Format("{0:N2}", dtEmpInfo.Rows[ROW]["IncrementAmount"].ToString()), false, true);

                    table2.Replace("{PreviousGrade}", dtEmpInfo.Rows[ROW]["PreviousGrade"].ToString(), false, true);
                    table2.Replace("{NewGrade}", dtEmpInfo.Rows[ROW]["NewGrade"].ToString(), false, true);
                    table2.Replace("{PreviousDesignation}", dtEmpInfo.Rows[ROW]["PreviousDesignation"].ToString(), false, true);
                    table2.Replace("{NewDesignation}", dtEmpInfo.Rows[ROW]["NewDesignation"].ToString(), false, true);
                }

                if (!string.IsNullOrEmpty(dtEmpHeaderInfo.Rows[0]["EmployeePic"].ToString()))
                {
                    var pic = dtEmpHeaderInfo.Rows[0]["EmployeePic"].ToString();
                    string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 139, 123);

                            section.Tables[0].Rows[1].Cells[2].Paragraphs[0].AppendPicture(newImage);
                            //document.Replace();
                            //document.Replace("{EmpPic}", "", true, true);

                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

                //#endregion

                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmpHeaderInfo.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmpHeaderInfo.Rows[0]["EmployeeCode"].ToString() + "-Increment-History.docx";

                }
                else
                {
                    fileNames = "Increment-History.docx";
                }
                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private void ExitInterview(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                string language = "";
                var fileName = "";
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    language = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    language = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Exitinterview" + plantId + language + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }

                var Templatefile = GetAppointmentFilePath(plantId, language, tempId, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "Exitinterview" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(filepath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }


                DataSet dsCompanyInfo = null;
                string strCompanyInfoSQL = @"select a.Address1 CompanyAddress, c.UserName CompanyName from org.Company c
                                            left join  mst.addressmaster a on a.id=c.AddressMasterId
                                            where c.id='" + identity.CompanyId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strCompanyInfoSQL, out dsCompanyInfo, false, "1");

                DataTable dtEmpInfo = GetEmployeeById(empId, plantId, empType, langID, tempId);
                // DataTable dtEmpInfo = GetEmployeeById(empId, plantId, empType, langID, tempId);


                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);
                WSection section = document.Sections[0];
                //TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                //Dictionary<string, int> replaced = new Dictionary<string, int>();

                document.Replace("{EmployeeName}", dtEmpInfo.Rows[0]["EmployeeName"].ToString(), false, true);
                document.Replace("{Designation}", dtEmpInfo.Rows[0]["DesignationName"].ToString(), false, true);
                document.Replace("{Department}", dtEmpInfo.Rows[0]["Department"].ToString(), false, true);
                document.Replace("{EmployeeCode}", dtEmpInfo.Rows[0]["EmployeeCode"].ToString(), false, true);
                document.Replace("{Plant}", dtEmpInfo.Rows[0]["PlantName"].ToString(), false, true);
                document.Replace("{DOS}", dtEmpInfo.Rows[0]["DOS"].ToString(), false, true);





                //makeAppraisalDetailTable(document, dtEmpInfo);


                WTable table = (WTable)section.Body.Tables[1];
                WTableRow copiedRow2 = table.Rows[1].Clone();

                WTableRow row2;
                #endregion

                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmpInfo.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmpInfo.Rows[0]["EmployeeCode"].ToString() + "-Exit-Interview.docx";

                }
                else
                {
                    fileNames = "Exit-Interview.docx";
                    //fileNames = "-Appointment-Letter";
                }
                ///////////
                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                //pdfDocument.PageSettings.Width = 1200;
                //pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "Exit-Interview" + plantId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();


                ///////////


            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        private DataTable GetEmpInfo(string EmpSystemId, string languageId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string Sql = @"SELECT distinct format(salaryInfoTo.EffectiveDate,'dd-MMM-yyyy')AppraisalDate,
			CONVERT(NUMERIC(10,2),salaryInfoFrom.EntryAmount) PreviousGross,
			CONVERT(NUMERIC(10,2),salaryInfoTo.EntryAmount) NewGross,
			CONVERT(NUMERIC(10,2),salaryInfoTo.EntryAmount-salaryInfoFrom.EntryAmount) IncrementAmount			
             ,sh.SalaryHead
            ,ei.EmpPicPath
            ,ei.Employeecode
            ,ei.Employeename
            
            ,ISNULL(DP.Name, isnull(OLD.Department,dep.username)) as PreviousDepartment
            --,ISNULL(DPN.Name,NEW.Department) as NewDepartment
			,ISNULL(LLD.Name, NDept.UserName) as NewDepartment
            ,ISNULL(DG.Name,OLDG.LegalDesignation) as PreviousDesignation
            ,ISNULL(DGN.Name,NEWG.LegalDesignation) as NewDesignation
            ,ISNULL(SG.Name,OLDG.SalaryGrade) as PreviousGrade
            ,ISNULL(SGN.Name,NEWG.SalaryGrade) as NewGrade
            
            from
            IncrementHistory IH
            LEFT JOIN (
            SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate,SD.EntryAmount,SD.SalaryHeadID FROM SalaryInfoDefineMaster SM
            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
            WHERE SM.EmpInfoSystemID='" + EmpSystemId + @"' AND SM.IsApproved=1
            Union
            SELECT SMB.SystemID,SMB.EmpInfoSystemID,SMB.EffectiveDate,SDB.EntryAmount,SDB.SalaryHeadID FROM SalaryInfoBackMaster SMB
            LEFT JOIN SalaryInfoBack SDB ON SDB.SalaryID=SMB.SystemID
            WHERE SMB.EmpInfoSystemID='" + EmpSystemId + @"'
            ) salaryInfoTo on IH.EmpSystemID=salaryInfoTo.EmpInfoSystemID AND IH.ToEffectiveDate=salaryInfoTo.EffectiveDate --and IH.ToSalaryId=salaryInfoTo.SystemID
            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=salaryInfoTo.SalaryHeadID
            
            LEFT JOIN (
            SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate,SD.EntryAmount,SD.SalaryHeadID FROM SalaryInfoDefineMaster SM
            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
            WHERE SM.EmpInfoSystemID='" + EmpSystemId + @"' AND SM.IsApproved=1
            Union
            SELECT SMB.SystemID,SMB.EmpInfoSystemID,SMB.EffectiveDate,SDB.EntryAmount,SDB.SalaryHeadID FROM SalaryInfoBackMaster SMB
            LEFT JOIN SalaryInfoBack SDB ON SDB.SalaryID=SMB.SystemID
            WHERE SMB.EmpInfoSystemID='" + EmpSystemId + @"'
              ) salaryInfoFrom on IH.EmpSystemID=salaryInfoFrom.EmpInfoSystemID AND IH.FromEffectiveDate=salaryInfoFrom.EffectiveDate --and IH.FromSalaryId=salaryInfoFrom.SystemID
            LEFT JOIN SalaryHead SH1 ON SH1.SalaryHeadID=salaryInfoFrom.SalaryHeadID
            LEFT JOIN EmployeeInformation ei ON EI.SystemId=salaryInfoTo.EmpInfoSystemID
			left join org.Department dep on dep.Id = ei.DepartmentId     
            LEFT JOIN hkp.LegalDesignation LD ON IH.ToLegalDesignationId = LD.Id
            left join (
            --Select distinct dep.Id as DepartmentId, dep.UserName as Department ,mpb.Code
            --from org.Position p
            --left join mst.ManpowerBudget mpb on mpb.PositionId = p.Id
            --left join org.Department dep on dep.Id = p.DepartmentId       
			select dep.Id as DepartmentId, dep.UserName as Department ,mb.Code from MST.ManpowerBudget MB
			LEFT JOIN [dbo].[EmployeeBudgetCodeHistory] H ON H.BudgetId=MB.Id AND H.Id=
			(select top(1) Id from [dbo].[EmployeeBudgetCodeHistory] where BudgetId=MB.Id Order BY AddedDate DESC)
			left join org.Position p on p.Id =mb.PositionId 
			left join org.Department dep on dep.Id = p.DepartmentId 
			where h.EmpSystemID='" + EmpSystemId + @"'
            ) OLD on old.Code=IH.FromBudgetCode
            left join (
            --Select distinct dep.Id as DepartmentId, dep.UserName as Department ,mpb.Code
            --from org.Position p
            --left join mst.ManpowerBudget mpb on mpb.PositionId = p.Id
            --left join org.Department dep on dep.Id = p.DepartmentId  
			select dep.Id as DepartmentId, dep.UserName as Department ,e.BudgetCode Code from EmployeeInformation E 
			left join mst.ManpowerBudget mpb on mpb.Id = e.BudgetCode
			left join org.Department dep on dep.Id = e.DepartmentId  
            ) NEW on NEW.Code=IH.ToBudgetCode  
            left join (
            select LSG.Id as SalaryGradeId, LSG.UserName SalaryGrade,LD.UserName LegalDesignation,LSGD.LegalDesignationId,lsgd.PlantId from [MST].[LegalSalaryGradeDesignation] LSGD
            LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSGD.LegalSalaryGradeId = LSG.Id
            LEFT JOIN hkp.LegalDesignation LD ON LSGD.LegalDesignationId = LD.Id           
            ) NEWG ON NEWG.LegalDesignationId = IH.ToLegalDesignationId and NEWG.PlantId=ei.PlantId            
            left join (
            select LSG.Id as SalaryGradeId,  LSG.UserName SalaryGrade,LD.UserName LegalDesignation,LSGD.LegalDesignationId,lsgd.PlantId from [MST].[LegalSalaryGradeDesignation] LSGD
            LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSGD.LegalSalaryGradeId = LSG.Id
            LEFT JOIN hkp.LegalDesignation LD ON LSGD.LegalDesignationId = LD.Id            
            ) OLDG ON OLDG.LegalDesignationId = IH.FROMLegalDesignationId and OLDG.PlantId=ei.PlantId

            LEFT JOIN HKP.LocalLanguage DP ON DP.DepartmentId =OLD.DepartmentId AND DP.LanguageId='" + languageId + @"'                                  
            --LEFT JOIN HKP.LocalLanguage DPN ON DPN.DepartmentId =NEW.DepartmentId AND DPN.LanguageId='" + languageId + @"'
            LEFT JOIN HKP.LocalLanguage DG ON DG.LegalDesignationId=OLDG.LegalDesignationId AND DG.LanguageId='" + languageId + @"'
            LEFT JOIN HKP.LocalLanguage DGN ON DGN.LegalDesignationId=NEWG.LegalDesignationId AND DGN.LanguageId='" + languageId + @"'
            LEFT JOIN HKP.LocalLanguage SG ON SG.LegalSalaryGradeId =OLDG.SalaryGradeId AND SG.LanguageId='" + languageId + @"'
            LEFT JOIN HKP.LocalLanguage SGN ON SGN.LegalSalaryGradeId =NEWG.SalaryGradeId AND SGN.LanguageId='" + languageId + @"'

			LEFT JOIN HKP.LocalLanguage LLD ON LLD.DepartmentId=ei.DepartmentId AND LLD.LanguageId='" + languageId + @"'
			LEFT JOIN ORG.Department as NDept on NDept.Id=ei.DepartmentId

            where IH.EmpSystemID='" + EmpSystemId + @"' and sh.HeadCategory='gross' and sh1.HeadCategory='gross'";
            return _sqlRepository.GetDataTable(Sql);
        }
        private DataTable GetEmpHeaderInfo(string EmpSystemId, string languageId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string Sql = @"select 
e.SystemId
,e.EmployeeCode 
,case when isnull(cg.Id,'')='' THEN isnull(E.EmployeeNameLocal,E.EmployeeName) ELSE EmployeeName END AS EmployeeName 
,Format( e.DOJ,'dd-MMM-yyyy') as DateOfJoin
,ISNULL(LDP.Name, d.UserName) as Department
,ISNULL(LD.Name,LG.UserName) as DesignationName
,ISNULL(LS.Name,s.UserName) as SectionName
,ISNULL(LL.Name,L.UserName) as Line
,ISNULL(LC.Name,C.UserName) as Category,
E.EmpPicPath EmployeePic


,e.EmploymentType as Agreement from EmployeeInformation as e
left outer join ORG.Department as d on d.Id=e.DepartmentId
left outer join HKP.LegalDesignation as LG on LG.Id=e.LegalDesignationId
left outer join org.Section as S on S.Id=e.SectionId
left outer join HKP.EmployeeCategory as C on c.Id=e.EmployeeCategorySystemID
left outer join ORG.line as L on L.Id=e.LineId 
LEFT JOIN org.CompanyGroup  CG on e.GroupID=cg.Id and CG.LanguageId='" + languageId + @"'
--LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=E.GivenDesignationId AND PL.LanguageId='" + languageId + @"'
LEFT JOIN HKP.LocalLanguage LDP ON LDP.DepartmentId =E.DepartmentId AND LDP.LanguageId='" + languageId + @"'
 LEFT JOIN HKP.LocalLanguage LD ON LD.LegalDesignationId=E.LegalDesignationId AND LD.LanguageId='" + languageId + @"'
 LEFT JOIN HKP.LocalLanguage LS ON LS.SectionId=e.SectionId  AND LS.LanguageId='" + languageId + @"'
 LEFT JOIN HKP.LocalLanguage LL ON LL.LineId=e.LineId  AND LL.LanguageId='" + languageId + @"'
 LEFT JOIN HKP.LocalLanguage LC ON LC.EmployeeCategoryId=e.EmployeeCategorySystemID  AND LC.LanguageId='" + languageId + @"'
 where e.SystemId ='" + EmpSystemId + @"'";
            return _sqlRepository.GetDataTable(Sql);
        }

        public Image ResizeImageDoc(Image image, int new_height, int new_width)
        {
            Bitmap new_image = new Bitmap(new_width, new_height);
            Graphics g = Graphics.FromImage((Image)new_image);
            g.InterpolationMode = InterpolationMode.High;
            g.DrawImage(image, 0, 0, new_width, new_height);
            return new_image;
        }
        public decimal GetOTRate(string empId)
        {
            clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
            DataTable dtOverTimePmtPolicy = GetOverTimePmtPolicy(empId);
            //////
            decimal OTRate = 0;
            string _formulaValue = "";
            DataSet dsSalaryData;
            DataSet dsSalHd;
            DataTable dtSlrHd;
            GetSalaryHead(out dsSalHd);
            dtSlrHd = dsSalHd.Tables[0];


            GetSalaryDataEmpWise(empId, DateTime.Now.ToString("dd-MMM-yyyy"), out dsSalaryData);
            DataTable dtValue = new DataTable();
            dtValue.TableName = "TempTable";
            dtValue.Columns.Add("SalaryHeadID");
            dtValue.Columns.Add("EntryCurrencyID");
            dtValue.Columns.Add("Amount");


            for (int i = 0; i < dsSalaryData.Tables[0].Rows.Count; i++)
            {
                DataRow dtValueRow = dtValue.NewRow();
                dtValueRow["SalaryHeadID"] = dsSalaryData.Tables[0].Rows[i]["SalaryHeadID"].ToString().Trim();
                dtValueRow["EntryCurrencyID"] = dsSalaryData.Tables[0].Rows[i]["EntryCurrencyID"].ToString().Trim();
                dtValueRow["Amount"] = dsSalaryData.Tables[0].Rows[i]["EntryAmount"].ToString().Trim();
                dtValue.Rows.Add(dtValueRow);
            }

            if (dtOverTimePmtPolicy.Rows.Count > 0)
            {
                if (Convert.ToBoolean(dtOverTimePmtPolicy.Rows[0]["IsOTEntitled"]))
                {
                    if (Convert.ToBoolean(dtOverTimePmtPolicy.Rows[0]["IsFixed"]))
                    {
                        OTRate = Convert.ToDecimal(dtOverTimePmtPolicy.Rows[0]["FixedValue"]);
                    }
                    if (Convert.ToBoolean(dtOverTimePmtPolicy.Rows[0]["IsFormula"]))
                    {
                        obSSrecal.ReLoadFormulaWithValue(dtOverTimePmtPolicy.Rows[0]["FormulaDesID"].ToString(), ref dtValue, dsSalaryData.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
                        OTRate = Convert.ToDecimal(clsSalaryStructureAplos.Evaluate(_formulaValue).ToString());
                    }
                }
            }
            decimal result = Convert.ToDecimal(string.Format("{0:F2}", OTRate));
            return result;    /////
        }

        public void GetSalaryDataEmpWise(string sEmpSystemId, string sEffectiveDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"  SELECT * FROM (                      SELECT (x.EffectiveDate) EffectiveDate,m.SystemID from (		

 select max(EffectiveDate)EffectiveDate from (

                        SELECT  max(EffectiveDate)EffectiveDate FROM SalaryInfoDefineMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 AND EffectiveDate<='" + sEffectiveDate + @"'
                        union
                        SELECT  Max(EffectiveDate)EffectiveDate  FROM SalaryInfoBackMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 AND EffectiveDate<='" + sEffectiveDate + @"'
	) a

						) x
						
						INNER JOIN (
							 SELECT  EffectiveDate,SystemID
							   FROM SalaryInfoDefineMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 
                        union
                        SELECT  EffectiveDate,SystemID FROM SalaryInfoBackMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 
						) m ON m.EffectiveDate=x.EffectiveDate ) mas
						INNER JOIN (
						SELECT s.SystemID,	s.SalaryID,	s.SalaryHeadID,	s.EntryCurrencyID,	s.EntryAmount,	s.DefineCurrencyID,	s.DefineAmount,	s.AmtDefinitionCurrencyID,	s.AmtDefinitionRate,	s.AddedBy,	s.DateAdded,	s.UpdatedBy,	s.DateUpdated,	s.SequenceNo,	s.SalaryCategory ,sh.HeadCategory  FROM SalaryInfoDefine s
						LEFT JOIN SalaryHead AS sh on s.SalaryHeadID=sh.SalaryHeadID 
						UNION
						SELECT sb.SystemID,	sb.SalaryID,	sb.SalaryHeadID,	sb.EntryCurrencyID,	sb.EntryAmount,	sb.DefineCurrencyID,	sb.DefineAmount,	sb.AmtDefinitionCurrencyID,	sb.AmtDefinitionRate,	sb.AddedBy,	sb.DateAdded,	sb.UpdatedBy,	sb.DateUpdated,	sb.SequenceNo,	sb.SalaryCategory ,sh.HeadCategory FROM  SalaryInfoBack sb
						LEFT JOIN SalaryHead AS sh on sb.SalaryHeadID=sh.SalaryHeadID
                        ) d ON mas.SystemID=d.SalaryID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void GetSalaryHead(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SalaryHead";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        public string GetBasicInfoInDefaultLng(string colName)
        {
            if (colName == "EmployeeName")
            {
                colName = "EmployeeNameEng";
            }
            if (colName == "MotherName")
            {
                colName = "MotherNameEng";
            }
            if (colName == "MotherName")
            {
                colName = "MotherNameEng";
            }
            return colName;
        }
        public string isBasicInfoInDefaultLng(string colName)
        {
            if (colName == "EmployeeName")
            {
                colName = "EmployeeNameEng";
            }
            if (colName == "MotherName")
            {
                colName = "MotherNameEng";
            }
            if (colName == "MotherName")
            {
                colName = "MotherNameEng";
            }
            return colName;
        }

        public Image resizeImage(Image image, int new_height, int new_width)
        {
            Bitmap new_image = new Bitmap(new_width, new_height);
            Graphics g = Graphics.FromImage((Image)new_image);
            g.InterpolationMode = InterpolationMode.High;
            g.DrawImage(image, 0, 0, new_width, new_height);
            return new_image;
        }

        private IWorkbook CreateIDCardSheet(ref IWorksheet sheet1, ref IWorksheet sheet2, ReportUtility report, string sheetHeader, string sheetName, string empId, string companyGroupId, string companyId, string plantId, string tempId, string empType, string reportType, string issuDate, string workTypeId)

        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string langName = "";
                string strPath = "";
                var fileName = "";
                //var reportType = "";
                // var dtLangName = "";
                var lang = GetLanguage(plantId, tempId, reportType);
                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    langName = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    langName = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "IdCard" + plantId + langName + ".xlsx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }

                var dtEmp = GetEmployeeById(empId, plantId, empType, langID, tempId);
                var dtEmpWorkType = GetEmployeeWorkType(workTypeId, langID);

                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook1 = null;
                IWorksheet sheet = null;

                var Templatefile = GetFilePath(plantId, langName, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }
                bool IsDefLan = false;

                var tokens = (fileName.Substring(("IdCard" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }

                if (System.IO.File.Exists(strPath) && langName != "English")
                {
                    workbook1 = excelEngine.Excel.Workbooks.Open(strPath, ExcelOpenType.Automatic, ExcelVersion.Excel2013);

                    for (int i = 0; i < workbook1.Worksheets.Count; i++)
                    {
                        sheet = workbook1.Worksheets[i];

                        int COL = 9;
                        int ROW = 1;

                        sheet.HideColumn(COL);
                        if (i == 0)
                        {
                            ////===== def lan 
                            FormatTextBox(ref sheet, "BloodGroup", dtEmp.Rows[0]["BloodGroup"].ToString(), 12, ExcelKnownColors.Red);
                            FormatTextBox(ref sheet, "PermanentAddress", dtEmp.Rows[0]["ParmanentAddress"].ToString(), 10, ExcelKnownColors.Black);
                            FormatTextBox(ref sheet, "PhoneNumber", cnDgt(dtEmp.Rows[0]["MobileNo"].ToString(), langName), 12, ExcelKnownColors.Black);
                            FormatTextBox(ref sheet, "EmergencyTelNo", cnDgt(dtEmp.Rows[0]["EmrCntPer1CellNo"].ToString(), langName), 11, ExcelKnownColors.Black);
                            FormatTextBox(ref sheet, "NID", cnDgt(dtEmp.Rows[0]["NationalID"].ToString(), langName), 12, ExcelKnownColors.Black);
                        }
                        else
                        {
                            FormatTextBox(ref sheet, "Name", dtEmp.Rows[0]["EmployeeName"].ToString(), 12, ExcelKnownColors.Black);
                            FormatTextBox(ref sheet, "DESIG", dtEmp.Rows[0]["DesignationName"].ToString(), 12, ExcelKnownColors.Black);
                            FormatTextBox(ref sheet, "ID", cnDgt(dtEmp.Rows[0]["EmployeeCode"].ToString(), langName), 12, ExcelKnownColors.Black);
                            FormatTextBox(ref sheet, "Department", dtEmp.Rows[0]["Section"].ToString(), 12, ExcelKnownColors.Black);
                            if (dtEmpWorkType.Rows.Count > 0)
                            {
                                FormatTextBox(ref sheet, "WorkType", dtEmpWorkType.Rows[0]["EmployeeWorkType"].ToString(), 12, ExcelKnownColors.Black);
                            }

                            var doj = GetFormatedDate(dtEmp.Rows[0]["DateOfJoin"].ToString(), langName);
                            FormatTextBox(ref sheet, "DOJ", doj, 12, ExcelKnownColors.Black);
                            var issudate = GetFormatedDate(Convert.ToDateTime(issuDate).ToString("dd-MMM-yyyy"), langName);
                            FormatTextBox(ref sheet, "IssueDate", issudate, 12, ExcelKnownColors.Black);

                            int x = sheet.Pictures.Count;
                            var pic = dtEmp.Rows[0]["EmployeePic"].ToString();
                            IPictureShape oldImage = sheet.Pictures["EmpPicture"];
                            int leftPosition = oldImage.Left;
                            int topPosition = oldImage.Top;
                            int height = oldImage.Height;
                            int width = oldImage.Width;
                            oldImage.Remove();
                            string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                            string ImagefileLocation = picpath;

                            if (System.IO.File.Exists(ImagefileLocation))
                            {
                                IPictureShape newImage = sheet.Pictures.AddPicture(ImagefileLocation);
                                newImage.Left = leftPosition;
                                newImage.Top = topPosition;
                                newImage.Height = height;
                                newImage.Width = width;
                            }
                        }

                        workbook1.Version = ExcelVersion.Excel2013;
                    }


                }
                else
                {
                    File = "IdCard" + plantId + "English.xlsx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }

                    workbook1 = excelEngine.Excel.Workbooks.Open(strPath, ExcelOpenType.Automatic, ExcelVersion.Excel2013);
                    for (int i = 0; i < workbook1.Worksheets.Count; i++)
                    {
                        sheet = workbook1.Worksheets[i];
                        int COL = 9;
                        int ROW = 1;

                        sheet.HideColumn(COL);
                        if (i == 0)
                        {
                            FormatTextBox(ref sheet, "BloodGroup", dtEmp.Rows[0]["BloodGroup"].ToString(), 11, ExcelKnownColors.Red);
                            FormatTextBox(ref sheet, "PermanentAddress", dtEmp.Rows[0]["ParmanentAddress"].ToString(), 10, ExcelKnownColors.Black);
                            FormatTextBox(ref sheet, "PhoneNumber", dtEmp.Rows[0]["MobileNo"].ToString(), 11, ExcelKnownColors.Black);
                            FormatTextBox(ref sheet, "EmergencyTelNo", cnDgt(dtEmp.Rows[0]["EmrCntPer1CellNo"].ToString(), langName), 11, ExcelKnownColors.Black);
                            FormatTextBox(ref sheet, "NID", dtEmp.Rows[0]["NationalID"].ToString(), 11, ExcelKnownColors.Black);


                        }
                        else
                        {
                            FormatTextBox(ref sheet, "Name", dtEmp.Rows[0]["EmployeeName"].ToString(), 11, ExcelKnownColors.Black);
                            FormatTextBox(ref sheet, "DESIG", dtEmp.Rows[0]["DesignationName"].ToString(), 11, ExcelKnownColors.Black);

                            FormatTextBox(ref sheet, "ID", dtEmp.Rows[0]["EmployeeCode"].ToString(), 11, ExcelKnownColors.Black);
                            FormatTextBox(ref sheet, "Department", dtEmp.Rows[0]["Section"].ToString(), 11, ExcelKnownColors.Black);
                            if (dtEmpWorkType.Rows.Count > 0)
                            {
                                FormatTextBox(ref sheet, "WorkType", dtEmpWorkType.Rows[0]["EmployeeWorkType"].ToString(), 11, ExcelKnownColors.Black);
                            }
                            FormatTextBox(ref sheet, "DOJ", dtEmp.Rows[0]["DateOfJoin"].ToString(), 11, ExcelKnownColors.Black);
                            FormatTextBox(ref sheet, "IssueDate", Convert.ToDateTime(issuDate).ToString("dd-MMM-yyyy"), 11, ExcelKnownColors.Black);

                            int x = sheet.Pictures.Count;
                            var pic = dtEmp.Rows[0]["EmployeePic"].ToString();
                            IPictureShape oldImage = sheet.Pictures["EmpPicture"];
                            int leftPosition = oldImage.Left;
                            int topPosition = oldImage.Top;
                            int height = oldImage.Height;
                            int width = oldImage.Width;
                            oldImage.Remove();
                            string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                            string ImagefileLocation = picpath;



                            if (System.IO.File.Exists(ImagefileLocation))
                            {
                                IPictureShape newImage = sheet.Pictures.AddPicture(ImagefileLocation);
                                newImage.Left = leftPosition;
                                newImage.Top = topPosition;
                                newImage.Height = height;
                                newImage.Width = width;
                            }


                        }

                        workbook1.Version = ExcelVersion.Excel2013;
                    }

                }
                return workbook1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private IWorkbook CreateIDCardSheetAll(ref IWorksheet sheet1, ref IWorksheet sheet2, IWorkbook workbook1, DataRow dr, string sheetHeader, string sheetName, string empId, string companyGroupId, string companyId, string plantId, string tempId, string empType, string reportType, string issuDate, string workTypeId, string langName)

        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                for (int i = 0; i < workbook1.Worksheets.Count; i++)
                {
                    IWorksheet sheet = workbook1.Worksheets[i];

                    if (i == 0)
                    {
                        FormatTextBox(ref sheet, "BloodGroup", dr["BloodGroup"].ToString(), 11, ExcelKnownColors.Red);
                        FormatTextBox(ref sheet, "PermanentAddress", dr["ParmanentAddress"].ToString(), 10, ExcelKnownColors.Black);
                        FormatTextBox(ref sheet, "PhoneNumber", cnDgt(dr["CellPhnNo"].ToString(), langName), 11, ExcelKnownColors.Black);
                        FormatTextBox(ref sheet, "EmergencyTelNo", cnDgt(dr["EmrCntPer1CellNo"].ToString(), langName), 11, ExcelKnownColors.Black);
                        FormatTextBox(ref sheet, "NID", cnDgt(dr["NationalID"].ToString(), langName), 11, ExcelKnownColors.Black);
                    }
                    else
                    {
                        FormatTextBox(ref sheet, "Name", dr["EmployeeName"].ToString(), 11, ExcelKnownColors.Black);
                        FormatTextBox(ref sheet, "DESIG", dr["DesignationName"].ToString(), 11, ExcelKnownColors.Black);
                        FormatTextBox(ref sheet, "ID", cnDgt(dr["EmployeeCode"].ToString(), langName), 11, ExcelKnownColors.Black);
                        FormatTextBox(ref sheet, "Department", dr["Department"].ToString(), 11, ExcelKnownColors.Black);
                        FormatTextBox(ref sheet, "Section", dr["Section"].ToString(), 11, ExcelKnownColors.Black);
                        FormatTextBox(ref sheet, "Grade", dr["Grade"].ToString(), 11, ExcelKnownColors.Black);
                        FormatTextBox(ref sheet, "Line", dr["Line"].ToString(), 11, ExcelKnownColors.Black);
                        FormatTextBox(ref sheet, "WorkType", dr["EmployeeWorkType"].ToString(), 11, ExcelKnownColors.Black);

                        var doj = GetFormatedDate(dr["DOJ"].ToString(), langName);
                        FormatTextBox(ref sheet, "DOJ", doj, 11, ExcelKnownColors.Black);
                        //var issudate = GetFormatedDate(Convert.ToDateTime(issuDate).ToString("dd-MMM-yyyy"), langName);
                        if (!string.IsNullOrEmpty(dr["IssueDate"].ToString()))
                        {
                            var issudate = GetFormatedDate(dr["IssueDate"].ToString(), langName);
                            FormatTextBox(ref sheet, "IssueDate", issudate, 11, ExcelKnownColors.Black);
                        }

                        int x = sheet.Pictures.Count;
                        var pic = dr["EmployeePic"].ToString();
                        IPictureShape oldImage = sheet.Pictures["EmpPicture"];
                        int leftPosition = oldImage.Left;
                        int topPosition = oldImage.Top;
                        int height = oldImage.Height;
                        int width = oldImage.Width;
                        oldImage.Remove();
                        string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                        string ImagefileLocation = picpath;

                        if (System.IO.File.Exists(ImagefileLocation))
                        {
                            IPictureShape newImage = sheet.Pictures.AddPicture(ImagefileLocation);
                            newImage.Left = leftPosition;
                            newImage.Top = topPosition;
                            newImage.Height = height;
                            newImage.Width = width;
                        }

                    }

                    // workbook1.Version = ExcelVersion.Excel2013;
                }


                return workbook1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        private IPresentation CreateIDCardSheetPpt(string empId, string companyGroupId, string companyId, string plantId, string tempId, string empType, string reportType, string issuDate, string workTypeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string langName = "";
                string strPath = "";
                var fileName = "";

                var lang = GetLanguage(plantId, tempId, reportType);
                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    langName = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    langName = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "IdCard" + plantId + langName + ".pptx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }

                var dtEmp = GetEmployeeById(empId, plantId, empType, langID, tempId);
                var dtEmpWorkType = GetEmployeeWorkType(workTypeId, langID);

                var Templatefile = GetIdCardFilePath(plantId, langName, reportType, tempId);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                IPresentation presentation = Presentation.Open(strPath);

                if (System.IO.File.Exists(strPath))
                {
                    for (int i = 0; i < presentation.Slides.Count; i++)
                    {
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "BloodGroup", dtEmp.Rows[0]["BloodGroup"].ToString(), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "PermanentAddress", dtEmp.Rows[0]["ParmanentAddress"].ToString(), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "PresentAddress", dtEmp.Rows[0]["PresentAddress"].ToString(), "Kalpurush", 8);


                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "Plant", dtEmp.Rows[0]["PlantName"].ToString(), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "Name", dtEmp.Rows[0]["EmployeeName"].ToString(), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "FatherOrSpouse", dtEmp.Rows[0]["FatherOrSpouse"].ToString(), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "FatherName", dtEmp.Rows[0]["FatherName"].ToString(), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "DESIG", dtEmp.Rows[0]["DesignationName"].ToString(), "Kalpurush", 8);
                        if (langName == "Hindi")
                        {
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "PhoneNumber", dtEmp.Rows[0]["MobileNo"].ToString(), "Kalpurush", 8);
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "EmergencyTelNo", dtEmp.Rows[0]["EmrCntPer1CellNo"].ToString(), "Kalpurush", 8);
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "ID", dtEmp.Rows[0]["EmployeeCode"].ToString(), "Kalpurush", 8);
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "NID", dtEmp.Rows[0]["NationalID"].ToString(), "Kalpurush", 8);
                        }
                        else
                        {
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "PhoneNumber", cnDgt(dtEmp.Rows[0]["MobileNo"].ToString(), langName), "Kalpurush", 8);
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "EmergencyTelNo", cnDgt(dtEmp.Rows[0]["EmrCntPer1CellNo"].ToString(), langName), "Kalpurush", 8);
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "ID", cnDgt(dtEmp.Rows[0]["EmployeeCode"].ToString(), langName), "Kalpurush", 8);
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "NID", cnDgt(dtEmp.Rows[0]["NationalID"].ToString(), langName), "Kalpurush", 8);
                        }
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "Department", dtEmp.Rows[0]["Department"].ToString(), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "Section", dtEmp.Rows[0]["Section"].ToString(), "Kalpurush", 8);
                        if (dtEmpWorkType.Rows.Count > 0)
                        {
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "WorkType", dtEmpWorkType.Rows[0]["EmployeeWorkType"].ToString(), "Kalpurush", 8);
                        }
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "PLANTNAME", dtEmp.Rows[0]["PlantName"].ToString(), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "Grade", dtEmp.Rows[0]["Grade"].ToString(), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "Line", dtEmp.Rows[0]["Line"].ToString(), "Kalpurush", 8);

                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "CompanyAddress", dtEmp.Rows[0]["CompanyAddress"].ToString(), "Kalpurush", 6);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "CompanyName", dtEmp.Rows[0]["CompanyName"].ToString(), "Kalpurush", 8);


                        if (langName == "Hindi")
                        {
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "DOJ", dtEmp.Rows[0]["DateOfJoin"].ToString(), "Kalpurush", 8);
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "DOB", dtEmp.Rows[0]["DateOfBirth"].ToString(), "Kalpurush", 8);
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "IssueDate", Convert.ToDateTime(issuDate).ToString("dd-MMM-yyyy"), "Kalpurush", 8);
                        }
                        else
                        {
                            var doj = GetFormatedDate(dtEmp.Rows[0]["DateOfJoin"].ToString(), langName);
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "DOJ", doj, "Kalpurush", 8);

                            var dob = GetFormatedDate(dtEmp.Rows[0]["DateOfBirth"].ToString(), langName);
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "DOB", dob, "Kalpurush", 8);

                            var issudate = GetFormatedDate(Convert.ToDateTime(issuDate).ToString("dd-MMM-yyyy"), langName);
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "IssueDate", issudate, "Kalpurush", 8);
                        }

                        try
                        {
                            var pic = dtEmp.Rows[0]["EmployeePic"].ToString();
                            string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                            string ImagefileLocation = picpath;
                            Image img = Image.FromFile(ImagefileLocation);
                            ConvertPresentationToPdf.SetPicture(presentation.Slides[i], "EmpPicture", img);


                        }
                        catch (Exception ex)
                        {

                        }
                        try
                        {

                            var authSignpic = dtEmp.Rows[0]["AuthorizedSignature"].ToString();
                            string authSignpicpath = ResourcesPathReader.GetAuthorizedSignaturePath() + authSignpic;
                            string authSignImagefileLocation = authSignpicpath;
                            Image authSignimg = Image.FromFile(authSignImagefileLocation);
                            ConvertPresentationToPdf.SetPicture(presentation.Slides[i], "AuthorizedSign", authSignimg);

                        }
                        catch (Exception ex)
                        {

                        }
                        try
                        {

                            var CompanyLogo = dtEmp.Rows[0]["CompanyLogo"].ToString();
                            string CompanyLogos = ResourcesPathReader.GetLogoOrImagePath() + CompanyLogo;
                            string CompanyLogofileLocation = CompanyLogos;
                            Image authLogo = Image.FromFile(CompanyLogofileLocation);
                            ConvertPresentationToPdf.SetPicture(presentation.Slides[i], "CompanyLogo", authLogo);

                        }
                        catch (Exception ex)
                        {

                        }
                        try
                        {

                            //var BarCode = dtEmp.Rows[0]["BarCodeId"].ToString();
                            CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;
                            System.Drawing.Image barcodeImg = qrCode.Draw(dtEmp.Rows[0]["BarCodeId"].ToString(), 200, 2);
                            ConvertPresentationToPdf.SetQRCode(presentation.Slides[i], "EmpQR", barcodeImg);

                        }
                        catch (Exception ex)
                        {

                        }

                    }
                }

                return presentation;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private IPresentation CreateIDCardSheetAllPpt(IPresentation presentation, DataRow dr, string sheetHeader, string sheetName, string empId, string companyGroupId, string companyId, string plantId, string tempId, string empType, string reportType, string issuDate, string workTypeId, string langName, bool IsCurrentIssueDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                for (int i = 0; i < presentation.Slides.Count; i++)
                {
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "CompanyName", dr["CompanyName"].ToString(), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "Plant", dr["PlantName"].ToString(), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "BloodGroup", dr["BloodGroup"].ToString(), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "PermanentAddress", dr["ParmanentAddress"].ToString(), "Kalpurush", 8);

                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "FatherOrSpouse", dr["FatherOrSpouse"].ToString(), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "FatherName", dr["FatherName"].ToString(), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "PresentAddress", dr["PresentAddress"].ToString(), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "BloodGroup", dr["BloodGroup"].ToString(), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "Name", dr["EmployeeName"].ToString(), "Kalpurush", 7);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "DESIG", dr["DesignationName"].ToString(), "Kalpurush", 8);
                    if (langName == "Hindi")
                    {
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "NID", dr["NationalID"].ToString(), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "PhoneNumber", dr["CellPhnNo"].ToString(), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "EmergencyTelNo", dr["EmrCntPer1CellNo"].ToString(), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "ID", dr["EmployeeCode"].ToString(), "Kalpurush", 8);
                    }
                    else
                    {
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "NID", cnDgt(dr["NationalID"].ToString(), langName), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "PhoneNumber", cnDgt(dr["CellPhnNo"].ToString(), langName), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "EmergencyTelNo", cnDgt(dr["EmrCntPer1CellNo"].ToString(), langName), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "ID", cnDgt(dr["EmployeeCode"].ToString(), langName), "Kalpurush", 8);
                    }
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "Department", dr["Department"].ToString(), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "Section", dr["Section"].ToString(), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "WorkType", dr["EmployeeWorkType"].ToString(), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "PLANTNAME", dr["PlantName"].ToString(), "Kalpurush", 8);

                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "Grade", dr["Grade"].ToString(), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "Line", dr["Line"].ToString(), "Kalpurush", 8);

                    if (langName == "Hindi")
                    {
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "DOJ", dr["DOJ"].ToString(), "Kalpurush", 8);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "DOB", dr["DateOfBirth"].ToString(), "Kalpurush", 8);
                    }
                    else
                    {
                        var doj = GetFormatedDate(dr["DOJ"].ToString(), langName);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "DOJ", doj, "Kalpurush", 8);

                        var dob = GetFormatedDate(dr["DateOfBirth"].ToString(), langName);
                        ConvertPresentationToPdf.SetText(presentation.Slides[i], "DOB", dob, "Kalpurush", 8);
                    }

                    if (IsCurrentIssueDate)
                    {
                        if (langName == "Hindi")
                        {
                            issuDate = Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy");
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "IssueDate", issuDate, "Kalpurush", 8);
                        }
                        else
                        {
                            issuDate = Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy");
                            var issudate = GetFormatedDate(Convert.ToDateTime(issuDate).ToString("dd-MMM-yyyy"), langName);
                            ConvertPresentationToPdf.SetText(presentation.Slides[i], "IssueDate", issudate, "Kalpurush", 8);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(dr["IssueDate"].ToString()))
                        {
                            if (langName == "Hindi")
                            {
                                ConvertPresentationToPdf.SetText(presentation.Slides[i], "IssueDate", Convert.ToDateTime(dr["IssueDate"].ToString()).ToString("dd-MMM-yyyy"), "Kalpurush", 8);
                            }
                            else
                            {
                                var issudate = GetFormatedDate(Convert.ToDateTime(dr["IssueDate"].ToString()).ToString("dd-MMM-yyyy"), langName);
                                ConvertPresentationToPdf.SetText(presentation.Slides[i], "IssueDate", issudate, "Kalpurush", 8);
                            }
                        }
                    }


                    try
                    {
                        var pic = dr["EmployeePic"].ToString();
                        string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                        string ImagefileLocation = picpath;
                        Image img = Image.FromFile(ImagefileLocation);
                        ConvertPresentationToPdf.SetPicture(presentation.Slides[i], "EmpPicture", img);


                    }
                    catch (Exception ex)
                    {


                    }

                    try
                    {

                        var authSignpic = dr["AuthorizedSignature"].ToString();
                        string authSignpicpath = ResourcesPathReader.GetAuthorizedSignaturePath() + authSignpic;
                        string authSignImagefileLocation = authSignpicpath;
                        Image authSignimg = Image.FromFile(authSignImagefileLocation);
                        ConvertPresentationToPdf.SetPicture(presentation.Slides[i], "AuthorizedSign", authSignimg);
                    }
                    catch (Exception ex)
                    {


                    }
                    try
                    {
                        CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;
                        System.Drawing.Image barcodeImg = qrCode.Draw(dr["BarCodeId"].ToString(), 200, 2);
                        ConvertPresentationToPdf.SetQRCode(presentation.Slides[i], "EmpQR", barcodeImg);

                    }
                    catch (Exception ex)
                    {

                    }

                }



                return presentation;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void FormatTextBox(ref IWorksheet sheet, string TextBoxName, string Text, float FontSize, ExcelKnownColors FontColor)
        {
            Text = Text == "" ? " " : Text;

            ITextBoxShape textbox = sheet.TextBoxes[TextBoxName];
            textbox.Text = Text;
            IRichTextString rtf = textbox.RichText;
            Syncfusion.XlsIO.IFont font = sheet.Workbook.CreateFont();
            font.Color = FontColor;
            font.Size = FontSize;
            //font.Bold = true;

            font.FontName = "Kalpurush";
            rtf.SetFont(0, textbox.Text.Length, font);

            textbox.RichText = rtf;
            textbox.Fill.ForeColor = Color.White;
            textbox.Fill.BackColor = Color.Gold;

        }

        private DataTable GetGrossAmount(string empId)
        {
            try
            {
                var sql = @"SELECT  convert(numeric(10,2), SD.EntryAmount) EntryAmount FROM SalaryInfoDefineMaster SM
                            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
                            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
                            WHERE SM.EmpInfoSystemID='" + empId + @"' AND SH.HeadCategory='GROSS'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable getLanguageId(string username)
        {
            try
            {
                var sql = @"Select Id from SCS.Language where UserName ='" + username.Replace("\r\n", "").Trim() + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable getLanguageName(string Id)
        {
            try
            {
                var sql = @"Select UserName from SCS.Language where Id ='" + Id + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable SalaryDetails(string empId)
        {
            try
            {
                var sql = @"SELECT  SH.SalaryHead, convert(numeric(10,2), SD.EntryAmount) EntryAmount FROM SalaryInfoDefineMaster SM
                            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
                            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
                            WHERE SM.EmpInfoSystemID='" + empId + @"' AND SH.SalaryHead  in ('Basic','Conveyance Allowance','House Rent','Gross') 
                            union
                           SELECT 'Other' SalaryHead,ISNULL(convert(numeric(10,2),Sum(SD.EntryAmount)),0) as 'SalaryDetails' FROM SalaryInfoDefineMaster SM
                           LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
                           LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
                           WHERE SM.EmpInfoSystemID='" + empId + @"' AND SH.SalaryHead not in ('Basic','Conveyance Allowance','House Rent') 
                           AND SH.IsGrossComponent=1 AND SH.IsCTCComponent=0";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable SalaryDetailsForSB(string empId, string languageId)
        {
            try
            {
                var sql = @" SELECT salaryInfo.SystemID,salaryInfo.EmpInfoSystemID,Format(salaryInfo.EffectiveDate,'dd-MMM-yyyy') EffectiveDate
                            ,LocLangGD.Name GivenDesignationName
                            ,LocLangLD.Name LegalDesignationName
                            ,LD.UserName DesignationName---ISNULL(LocalDesignationName,DesignationName) DesignationName,
                            ,SH.SalaryHead,sh.HeadType
                            ,BSH.Name SalaryHeadBangla
                            ,convert(numeric(10,0), salaryInfo.EntryAmount) EntryAmount ----,IH.*

                             from 
 
                             IncrementHistory IH 
 
                             LEFT JOIN (
                            SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate,SD.EntryAmount,SD.SalaryHeadID FROM SalaryInfoDefineMaster SM
                            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
                            WHERE SM.EmpInfoSystemID='" + empId + @"' AND SM.IsApproved=1
                            Union
                            SELECT SMB.SystemID,SMB.EmpInfoSystemID,SMB.EffectiveDate,SDB.EntryAmount,SDB.SalaryHeadID FROM SalaryInfoBackMaster SMB
                            LEFT JOIN SalaryInfoBack SDB ON SDB.SalaryID=SMB.SystemID
                            WHERE SMB.EmpInfoSystemID='" + empId + @"'
                            ) salaryInfo on IH.EmpSystemID=salaryInfo.EmpInfoSystemID AND IH.ToEffectiveDate=salaryInfo.EffectiveDate 
                            --and IH.ToSalaryId=salaryInfo.SystemID


                            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=salaryInfo.SalaryHeadID
                            LEFT JOIN EmployeeInformation ei ON EI.SystemId=salaryInfo.EmpInfoSystemID
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId

                            LEFT JOIN hkp.Designation GVDE ON IH.ToGivenDesignationId = GVDE.Id
                            LEFT JOIN hkp.LegalDesignation LD ON IH.ToLegalDesignationId = LD.Id

                            LEFT JOIN hkp.LocalLanguage LocLangLD ON LocLangLD.LegalDesignationId = IH.ToLegalDesignationId and LocLangLD.LanguageId ='" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN hkp.LocalLanguage LocLangGD ON LocLangGD.DesignationId = IH.ToGivenDesignationId and LocLangGD.LanguageId = '" + languageId + @"'--PL.LanguageId




                            LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=EI.GivenDesignationId AND B.LanguageId='" + languageId + @"'--PL.LanguageId 


                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL) AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID and BSH.LanguageId='" + languageId + @"'--PL.LanguageId

                            where IH.EmpSystemID='" + empId + @"'
                            ORDER BY convert(date,salaryInfo.EffectiveDate) --DESC ";







                var xsql = @"SELECT SystemId, FORMAT(EffectiveDate,'dd-MMM-yyyy') EffectiveDate,ISNULL(ISNULL(LegalDesignationName,DesignationName ),GivenDesignationName)  DesignationName,SalaryHead, Convert(decimal(18,0),EntryAmount) EntryAmount,HeadType
                            FROM(
                            SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate
                            ,LocLangGD.Name GivenDesignationName
                            ,LocLangLD.Name LegalDesignationName
                            ,LD.UserName DesignationName---ISNULL(LocalDesignationName,DesignationName) DesignationName,
                            ,SH.SalaryHead,sh.HeadType
                            ,BSH.Name SalaryHeadBangla
                            ,convert(numeric(10,2), SD.EntryAmount) EntryAmount 
                            FROM SalaryInfoDefineMaster SM
                            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
                            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
                            LEFT JOIN EmployeeInformation ei ON EI.SystemId=SM.EmpInfoSystemID
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN hkp.Designation GVDE ON EI.GivenDesignationId = GVDE.Id
                            LEFT JOIN hkp.LegalDesignation LD ON EI.LegalDesignationId = LD.Id
                            LEFT JOIN hkp.LocalLanguage LocLangLD ON LocLangLD.LegalDesignationId = EI.LegalDesignationId and LocLangLD.LanguageId ='" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN hkp.LocalLanguage LocLangGD ON LocLangGD.DesignationId = EI.GivenDesignationId and LocLangGD.LanguageId = '" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=EI.GivenDesignationId AND B.LanguageId='" + languageId + @"'--PL.LanguageId
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL) AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID and BSH.LanguageId='" + languageId + @"'--PL.LanguageId
                            WHERE SM.EmpInfoSystemID='" + empId + @"' AND SM.IsApproved=1
                            UNION
                            SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate
                            ,LocLangGD.Name GivenDesignationName
                            ,LocLangLD.Name LegalDesignationName
                            ,LD.UserName DesignationName---ISNULL(LocalDesignationName,DesignationName) DesignationName,
                            ,SH.SalaryHead,sh.HeadType
                            ,BSH.Name SalaryHeadBangla
                            ,convert(numeric(10,2), SD.EntryAmount) EntryAmount 
                            FROM SalaryInfoBackMaster SM
                            LEFT JOIN SalaryInfoBack SD ON SD.SalaryID=SM.SystemID
                            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
                            LEFT JOIN EmployeeInformation ei ON EI.SystemId=SM.EmpInfoSystemID
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN hkp.Designation GVDE ON EI.GivenDesignationId = GVDE.Id
                            LEFT JOIN hkp.LegalDesignation LD ON EI.LegalDesignationId = LD.Id
                            LEFT JOIN hkp.LocalLanguage LocLangLD ON LocLangLD.LegalDesignationId = EI.LegalDesignationId and LocLangLD.LanguageId ='" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN hkp.LocalLanguage LocLangGD ON LocLangGD.DesignationId = EI.GivenDesignationId and LocLangGD.LanguageId = '" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=EI.GivenDesignationId AND B.LanguageId='" + languageId + @"'--PL.LanguageId 
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL) AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID and BSH.LanguageId='" + languageId + @"'--PL.LanguageId
                            WHERE SM.EmpInfoSystemID='" + empId + @"' ) x ORDER BY convert(date,EffectiveDate) DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable EmployeeDisciplinaryAction(string empId, string languageId)
        {
            try
            {
                var sql = @"select [Id]
                          ,[EmpSystemId]
                          ,[DisciplinaryActionCategoryId]
                          ,[Description]
                          ,FORMAT([EntryDate],'dd-MMM-yyyy') EntryDate
                          ,[AddedBy]
                          ,[AddedDate]
                          ,[AddedFromIP]
                          ,[UpdatedBy]
                          ,[UpdatedDate]
                          ,[UpdatedFromIP] from hkp.EmployeeDisciplinaryAction where EmpSystemId='" + empId + @"'";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetEmployeeWorkType(string workTypeId, string languageId)
        {
            try
            {
                string sql = @"SELECT ISNULL(L.Name, W.UserName) EmployeeWorkType
                             FROM [dbo].[EmployeeWorkType] W
                             LEFT JOIN HKP.LocalLanguage L ON W.Id=L.EmployeeWorkTypeId and L.LanguageId = '" + languageId + @"'
                             WHERE W.Id='" + workTypeId + "' ";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable SalaryDetailsForApp(string empId, string languageId)
        {
            try
            {
                var sql = @"SELECT SystemId, FORMAT(EffectiveDate,'dd-MMM-yyyy') EffectiveDate,ISNULL(ISNULL(GivenDesignationName,LegalDesignationName),DesignationName)  DesignationName,SalaryHead, Convert(decimal(18,2),EntryAmount) EntryAmount,HeadType,Grade,HeadCategory,OTRate
                            FROM(
                            SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate
                            ,LocLangGD.Name GivenDesignationName
                            ,LocLangLD.Name LegalDesignationName
                            ,GVDE.UserName DesignationName---ISNULL(LocalDesignationName,DesignationName) DesignationName,
                            ,SH.SalaryHead,sh.HeadType
                            ,BSH.Name SalaryHeadBangla
                            ,convert(numeric(10,2), SD.EntryAmount) EntryAmount ,LSG.ShortName Grade,SH.HeadCategory
	                        ,OTRate=case when dmc.IsOTEntitled=1 then 
										case when SH.HeadCategory='Basic' then (convert(numeric(10,2),SD.EntryAmount/208)*2) else 0 end							
							        else 0 end
                            FROM SalaryInfoDefineMaster SM
                            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
                            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
                            LEFT JOIN EmployeeInformation ei ON EI.SystemId=SM.EmpInfoSystemID
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN MST.LegalSalaryGradeDesignation LSD ON LSD.LegalDesignationId=EI.LegalDesignationId AND LSD.PlantId = PL.Id
                            LEFT JOIN  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=lsd.LegalSalaryGradeId
                            LEFT JOIN hkp.Designation GVDE ON EI.GivenDesignationId = GVDE.Id
                            LEFT JOIN hkp.LegalDesignation LD ON EI.LegalDesignationId = LD.Id
                            LEFT JOIN hkp.LocalLanguage LocLangLD ON LocLangLD.LegalDesignationId = EI.LegalDesignationId and LocLangLD.LanguageId ='" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN hkp.LocalLanguage LocLangGD ON LocLangGD.DesignationId = EI.GivenDesignationId and LocLangGD.LanguageId = '" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=EI.GivenDesignationId AND B.LanguageId='" + languageId + @"'--PL.LanguageId
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL) AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID and BSH.LanguageId='" + languageId + @"'--PL.LanguageId

	                         LEFT JOIN mst.DesignationMasterLegalDesignation AS dmld ON dmld.LegalDesignationId=ei.LegalDesignationId---
							 LEFT JOIN MST.DesignationMaster DM ON DM.id=dmld.DesignationMasterId----
							 LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId = DM.Id AND dmc.PlantId=ei.PlantId      

                            WHERE SM.EmpInfoSystemID='" + empId + @"'AND SM.IsApproved=1                           
                             ) x ORDER BY EffectiveDate DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable SalaryDetailsForAppLaila(string empId, string languageId)
        {
            try
            {
                var sql = @"SELECT SystemId, FORMAT(EffectiveDate,'dd-MMM-yyyy') EffectiveDate,ISNULL(ISNULL(GivenDesignationName,LegalDesignationName),DesignationName)  DesignationName,SalaryHead, Convert(decimal(18,0),EntryAmount) EntryAmount,HeadType,Grade,HeadCategory,OTRate
                            FROM(
                            SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate
                            ,LocLangGD.Name GivenDesignationName
                            ,LocLangLD.Name LegalDesignationName
                            ,GVDE.UserName DesignationName---ISNULL(LocalDesignationName,DesignationName) DesignationName,
                            ,SH.SalaryHead,sh.HeadType
                            ,BSH.Name SalaryHeadBangla
                            ,convert(numeric(10,2), SD.EntryAmount) EntryAmount ,LSG.ShortName Grade,SH.HeadCategory
	                        ,OTRate=case when dmc.IsOTEntitled=1 then 
										case when SH.HeadCategory='Basic' then (convert(numeric(10,2),SD.EntryAmount/208)*2) else 0 end							
							        else 0 end
                            FROM SalaryInfoDefineMaster SM
                            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
                            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
                            LEFT JOIN EmployeeInformation ei ON EI.SystemId=SM.EmpInfoSystemID
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN MST.LegalSalaryGradeDesignation LSD ON LSD.LegalDesignationId=EI.LegalDesignationId AND LSD.PlantId = PL.Id
                            LEFT JOIN  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=lsd.LegalSalaryGradeId
                            LEFT JOIN hkp.Designation GVDE ON EI.GivenDesignationId = GVDE.Id
                            LEFT JOIN hkp.LegalDesignation LD ON EI.LegalDesignationId = LD.Id
                            LEFT JOIN hkp.LocalLanguage LocLangLD ON LocLangLD.LegalDesignationId = EI.LegalDesignationId and LocLangLD.LanguageId ='" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN hkp.LocalLanguage LocLangGD ON LocLangGD.DesignationId = EI.GivenDesignationId and LocLangGD.LanguageId = '" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=EI.GivenDesignationId AND B.LanguageId='" + languageId + @"'--PL.LanguageId
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL) AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID and BSH.LanguageId='" + languageId + @"'--PL.LanguageId

	                         LEFT JOIN mst.DesignationMasterLegalDesignation AS dmld ON dmld.LegalDesignationId=ei.LegalDesignationId---
							 LEFT JOIN MST.DesignationMaster DM ON DM.id=dmld.DesignationMasterId----
							 LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId = DM.Id AND dmc.PlantId=ei.PlantId      

                            WHERE SM.EmpInfoSystemID='" + empId + @"'AND SM.IsApproved=1                           
                             ) x ORDER BY EffectiveDate DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetEmployeeById(string employeeId, string plantId, string employeementType, string languageId, string tempId)
        {
            try
            {

                string sql = @"SELECT TOP 1 EmployeeCode,  
                           TAB3.EmployeeName,TAB3.FatherName,TAB3.MotherName,TAB3.ParmanentAddress, TAB3.PresentAddress,
                            ISNULL(LocalCompanyName,CompanyName) CompanyName,
                            ISNULL(CompanyAddress,CompanyAddress) CompanyAddress,
                            ISNULL(UtilityName,UtilityName) UtilityName,
                            ISNULL(PresentCity,PresentCity) PresentCity,							
                            ISNULL(PresentDistrict,PresentDistrict) PresentDistrict,
                            ISNULL(PresentState,PresentState) PresentState,
                            ISNULL(LPresentCountry,LPermanentCountry) LPresentCountry,
                            ISNULL(FirstName,FirstName) FirstName,
                            ISNULL(LegalDesignationLocal,LegalDesignation) DesignationName,
                            ISNULL(LocalDepartmentName1,Department) Department,
                            ISNULL(UnitLocal,Unit) Unit,
                            ISNULL(DateOfJoin,DateOfJoin) DOJ,
                            ISNULL(DateOfJoin,DateOfJoin) DateOfJoin,
                            ISNULL(DateOfBirth,DateOfBirth) DOB,
                            ISNULL(confirm,confirm) ProbationPeriod,
                            ISNULL(MobileNo,MobileNo) MobileNo,                            
                            ISNULL(SectionName,Section) Section,                          
                            ISNULL(DOC,DOC) DOC,
                            ISNULL(NationalID,NationalID) NationalID,
                            ISNULL(BloodGroup,BloodGroup) BloodGroup,
                            ISNULL(EmployeePic,EmployeePic) EmployeePic,AppliedDate,DateOfBirth,SpouseName,EmploymentTypelocal
							,ProbationerName, fEm,SectionName, LocalDepartmentName1
                            ,ISNULL(GradeLocal,Grade) Grade
                            ,IssueDate,NomineeName,NomineeAddress,NomineeNID,NomineeDOB
							,NomineeRelation,CivilStatus,PFAccountNumber,DocDate
                            ,Gender,IdentificationMark,NomineeAge
                            ,ISNULL(PlantLocal,Plant) PlantName    
                            ,ISNULL(LineLocal,Line) Line ,Religion
                            ,MarriedEmpNomineeName=CASE WHEN CivilStatus='Married' then NomineeName else '' end
							,MarriedEmpNomineeAddress=CASE WHEN CivilStatus='Married' then NomineeAddress else '' end
							,MarriedEmpNomineeNID=CASE WHEN CivilStatus='Married' then NomineeNID else '' end
							,MarriedEmpNomineeDOB=CASE WHEN CivilStatus='Married' then NomineeDOB else '' end
							,MarriedEmpNomineeRelation=CASE WHEN CivilStatus='Married' then NomineeRelation else '' end
							,MarriedEmpNomineeAge=CASE WHEN CivilStatus='Married' then NomineeAge else '' end

							,UnMarriedEmpNomineeName=CASE WHEN CivilStatus!='Married' then NomineeName else '' end
							,UnMarriedEmpNomineeAddress=CASE WHEN CivilStatus!='Married' then NomineeAddress else '' end
							,UnMarriedEmpNomineeNID=CASE WHEN CivilStatus!='Married' then NomineeNID else '' end
							,UnMarriedEmpNomineeDOB=CASE WHEN CivilStatus!='Married' then NomineeDOB else '' end
							,UnMarriedEmpNomineeRelation=CASE WHEN CivilStatus!='Married' then NomineeRelation else '' end
							,UnMarriedEmpNomineeAge=CASE WHEN CivilStatus!='Married' then NomineeAge else '' end
                            ,Salutation,EmployeeFingerPrint,CardHolderSignature,DOS,AuthorizedSignature,Contractor,ContractorAddress
                            ,EmrCntPer1CellNo,FatherOrSpouse,CompanyLogo,BarCodeId,Designation
                                    FROM(SELECT TAB2.*, AM.Phone, AM.Email, AM.Website, AM.Address1 FROM 
									--tab2
									(SELECT TAB1.*, LAN.StandardName 
                                    FROM (SELECT CM.Image CompanyLogo,E.SystemID as EmpSystemID,DES.UserName Designation,
                                    CM.UserName CompanyName,AM.Address1 CompanyAddress,E.EmpPicPath EmployeePic,E.EmployeeCode, Convert(varchar, E.DOJ, 105) DOJ,
                                    REPLACE(CONVERT(VARCHAR(11),E.DOJ,106),' ','-') DateOfJoin,BG.UserName BloodGroup,REPLACE(CONVERT(VARCHAR(11),E.DOB,106),' ','-') DateOfBirth
                                    ,E.NationalID,E.EmploymentType,D.UserName DesignationName, dm.EmployeeCategoryId,ec.UserName EmployeeCategory,L.UserName Line,
			                		E.EmpSignature CardHolderSignature,P.AuthorizedSignature
                                    ,E.CellPhnNo MobileNo,DP.UserName Department,SE.UserName Section,A.[Name] LocalCompanyName, B.[Name] LocalDesignationName,C.[Name] LocalDepartmentName,
			                		N.Name NameLabel
                                    ,DN.Name DesignationLabel,DPN.Name DepartmentLabel,LN.Name LineLabel,LET.Name EmploymentTypeLabel, ID.Name IDNoLabel,  PT.Name EmploymentTypeName,
			                		DJ.Name DOJLabel, ET.Name EmergencyTellNoLabel, BGP.Name BloodGroupLabel
                                    ,E.EmployeeNameLocal,LL.UtilityName,NID.Name NIDLabel, LMB.Name MobileNoLabel,
			                		LD.Name LegalDesignationLocal,SEC.Name SectionName, CAC.[Name] LocalDepartmentName1
									,GD.ShortName Grade,LSGA.Name GradeLocal
                                    ,Convert(varchar, DATEADD(year, 5, E.DOJ),105) AS Validity,LNN.Name LineLocal,UN.Username Unit, LUN.[Name] UnitLocal, Convert(varchar, E.DOC, 105) DOC,FORMAT(E.AppliedDate,'dd-MMM-yyyy') AppliedDate
                                    ,PCN.Name LPermanentCountry,PRCN.Name LPresentCountry
			                		,PD.Name PermanentDistrict,PRD.Name PresentDistrict,PST.Name PermanentState, PRST.Name PresentState,PCT.Name PermanentCity, PRCT.Name PresentCity
                                    ,CASE WHEN E.DOCDay=0 THEN E.DOCMonth ELSE E.DOCDay/30 END AS confirm, PL.LanguageId, PL.Id as 'PlantId', CM.AddressMasterId,E.FirstName,LDN.UserName LegalDesignation,ISNULL(E.SpouseNameLocal,E.SpouseName) SpouseName,  ISNULL(LET.Name,E.EmploymentType) EmploymentTypelocal
									,LPRL.Name ProbationerName , PT.Name fEm, FORMAT(E.IssueDate,'dd-MMM-yyyy') IssueDate,
                                       	FORMAT(E.DOS,'dd-MMM-yyyy') DOS,									
										case when isnull(cg.Id,'')='' THEN isnull(E.EmployeeNameLocal,E.EmployeeName) ELSE E.EmployeeName END AS EmployeeName
										,case when isnull(cg.Id,'')='' THEN isnull(E.FatherNameLocal,E.FatherName) ELSE E.FatherName END AS FatherName
										,case when isnull(cg.Id,'')='' THEN isnull(E.MotherNameLocal,E.MotherName) ELSE E.MotherName END AS MotherName
										,case when isnull(cg.Id,'')='' THEN isnull(E.ParmanentAddress1Local+''+CASE WHEN ISNULL(E.ParmanentAddress2Local,'')<>'' THEN ','+E.ParmanentAddress2Local ELSE '' END										
										,E.ParmanentAddress1+''+CASE WHEN ISNULL(E.ParmanentAddress2,'')<>'' THEN ', '+E.ParmanentAddress2 ELSE '' END) ELSE E.ParmanentAddress1+''+CASE WHEN ISNULL(E.ParmanentAddress2,'')<>'' THEN ', '+E.ParmanentAddress2 ELSE '' END END AS ParmanentAddress
										,case when isnull(cg.Id,'')='' THEN isnull(E.PresentAddress1Local,E.PresentAddress1) ELSE E.PresentAddress1 END AS PresentAddress


                                       ,case when isnull(cg.Id,'')='' THEN isnull(Case When E.GenderID ='Male' then  LMM.Name else LMF.Name end,E.GenderID) ELSE E.GenderID END AS Gender
                                        ,case when isnull(cg.Id,'')='' THEN isnull(E.LocalIdentificationMark,E.IdentificationMark) ELSE E.IdentificationMark END AS IdentificationMark
                                        ,case when isnull(cg.Id,'')='' THEN isnull(NomineeInfo.localName,NomineeInfo.Name) ELSE NomineeInfo.Name END AS NomineeName
										,case when isnull(cg.Id,'')='' THEN isnull(NomineeInfo.AddressLocal,NomineeInfo.Address) ELSE Address END AS NomineeAddress
                                       
										,NomineeInfo.NationalID NomineeNID,  FORMAT(NomineeInfo.DOB,'dd-MMM-yyyy') NomineeDOB,isnull(cast((DATEDIFF(m, NomineeInfo.DOB, GETDATE())/12) as varchar),0) NomineeAge
                                        ,Isnull(LNomR.Name, Relationship.UserName) NomineeRelation 
										,Isnull(CS.Name, CivilStatus.UserName) CivilStatus 
										,PFDocument.docNumber PFAccountNumber,PFDocument.DocDate
                                        ,PL.UserName Plant,PLL.Name PlantLocal,ISNULL(LReligion.Name,Religion.UserName) Religion,S.UserName Salutation
                                        ,efp.FileName EmployeeFingerPrint,PRT.UserName Contractor,AD.ContractorAddress
                                        ,E.EmrCntPer1CellNo,FatherOrSpouse = case when E.FatherName is null then e.SpouseName else E.FatherName  end
                                        ,CONCAT(e.SystemId,'#',e.EmployeeCode,'#',e.EmployeeName)BarCodeId
										from EmployeeInformation E
                                        LEFT JOIN HKP.Party PRT ON PRT.Id = E.VendorId
									    LEFT JOIN(
									    Select AM.Id,ContractorAddress=CASE WHEN ISNULL(A.UserName,'')='' THEN '' ELSE A.UserName+', ' END+CASE WHEN ISNULL(CT.UserName,'')='' THEN '' ELSE CT.UserName+', ' END+ 
										CASE WHEN ISNULL(S.UserName,'')='' THEN '' ELSE S.UserName+', ' END+CASE WHEN ISNULL(CN.UserName,'')='' THEN '' ELSE CN.UserName+', ' END
										+CASE WHEN ISNULL(C.UserName,'')='' THEN '' ELSE C.UserName+'. ' END
										from MST.AddressMaster  AM
										LEFT JOIN SCS.Continent C ON C.Id=AM.ContinentId
										LEFT JOIN SCS.Country CN ON CN.Id=AM.CountryId
										LEFT JOIN SCS.[State] S ON S.Id=AM.StateId
										LEFT JOIN SCS.City CT ON CT.Id=AM.CityId
										LEFT JOIN SCS.Area A ON A.Id=AM.AreaId									
									) AD ON AD.Id=PRT.AddressMasterId
                                    LEFT JOIN HKP.Salutation S ON S.Id = E.Salutation
									LEFT JOIN org.CompanyGroup  CG on e.GroupID=cg.Id and CG.LanguageId='" + languageId + @"'
									LEFT JOIN ORG.Company CM ON CM.Id = E.CompanyId
                                    LEFT JOIN MST.AddressMaster AM ON AM.Id = CM.AddressMasterId
                                    LEFT JOIN HKP.BloodGroup BG ON BG.Id = E.BloodGroupID
                                    LEFT JOIN MST.ManpowerBudget bbb ON e.BudgetCode = bbb.Id
                                    LEFT JOIN MST.ManpowerBudget MPB ON MPB.Id=bbb.ROBudgetCode
									left join ORG.Position POS on POS.Id=MPB.PositionId
									left join HKP.Designation DES on DES.Id=POS.DesignationId
									LEFT JOIN ORG.Position PS ON PS.Id=bbb.PositionId
                                    LEFT JOIN HKP.Designation D ON D.Id = E.GivenDesignationId
                                    LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = e.GivenDesignationId
                                    LEFT JOIN  hkp.LegalDesignation LDN ON LDN.Id=E.LegalDesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id = DM.EmployeeCategoryId
                                    LEFT JOIN ORG.Line L ON L.Id=E.LineId
                                    LEFT JOIN ORG.Unit UN ON UN.Id=E.UnitId
			                		LEFT JOIN [SCS].[PlantSetting] P ON P.PlantId=E.PlantId AND P.ModuleName='HR'
                                    LEFT JOIN ORG.Department DP ON DP.Id=PS.DepartmentId
                                    LEFT JOIN org.Section SE ON SE.Id=E.SectionId
			                		LEFT JOIN ORG.Plant PL ON PL.Id=E.PlantId
                                    LEFT JOIN EmployeeFingerPrint efp ON efp.EmpSystemID=E.SystemId AND efp.Id=(SELECT TOP 1 Id FROM EmployeeFingerPrint WHERE EmpSystemID=E.SystemId)
                                    left Join MST.PayrollGroupMaster PGM on PGM.EmployeeId = E.EmployeeId
                                    LEFT JOIN EmployeeNomineeInfo NomineeInfo ON NomineeInfo.EmpSystemId = E.SystemId ------NomineeInfo
                                    LEFT JOIN (
						                    SELECT LSGD.PlantId,LSGD.LegalDesignationId,LS.ShortName,LSGD.LegalSalaryGradeId from [MST].[LegalSalaryGradeDesignation] LSGD
                                            LEFT JOIN [SCS].[LegalSalaryGrade] LS ON LS.Id=LSGD.LegalSalaryGradeId 
						                        ) GD ON GD.PlantId=E.PlantId AND GD.LegalDesignationId=E.LegalDesignationId


									LEFT JOIN  scs.Relationship Relationship ON NomineeInfo.Relation = Relationship.Id 
									LEFT JOIN HKP.CivilStatus CivilStatus ON CivilStatus.Id = E.CivilStatusID

                                   LEFT JOIN HKP.LocalLanguage CS ON CivilStatus.Id=CS.CivilStatusID AND PL.LanguageId='" + languageId + @"'
								   left join (
								   select d.docNumber,d.EmpSystemid,FORMAT(d.DocDate,'dd-MMM-yyyy')DocDate from EmployeeDocument d
									left join hkp.ComplianceDocument cd on cd.Id=d.ComplianceDocumentid
									where  cd.profiletype='PF'
								   
								   ) PFDocument on PFDocument.EmpSystemId= E.SystemId
						             left join scs.Religion Religion on Religion.Id=E.ReligionID
									 LEFT JOIN HKP.LocalLanguage LReligion ON LReligion.ReligionID=E.ReligionID AND PL.LanguageId='" + languageId + @"'


                                    LEFT JOIN HKP.LocalLanguage LSGA ON LSGA.LegalSalaryGradeId=GD.LegalSalaryGradeId AND LSGA.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage A ON A.CompanyId=E.CompanyId AND A.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LL ON LL.CompanyId=E.CompanyId AND LL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage PLL ON PLL.PlantId=E.PlantId AND PLL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=E.GivenDesignationId AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage C ON C.DepartmentId =PS.DepartmentId AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LD ON LD.LegalDesignationId=E.LegalDesignationId AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LNN ON LNN.LineId=E.LineId AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage PCN ON PCN.CountryId=E.ParmCountryID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PRCN ON PRCN.CountryId=E.ParmCountryID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PD ON PD.DistrictId=E.ParmDistrictID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PRD ON PRD.DistrictId=E.PresDistrictID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PST ON PST.StateId=E.ParmStateId AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PRST ON PRST.StateId=E.PresStateId AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PCT ON PCT.CityId=E.ParmCityID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PRCT ON PRCT.CityId=E.PresCityID AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LUN ON LUN.UnitId=E.UnitId AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LNomR ON LNomR.RelationshipId=NomineeInfo.Relation AND PL.LanguageId='" + languageId + @"'

                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Male'and LanguageId='" + languageId + @"' ) LMM ON LMM.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Female'and LanguageId='" + languageId + @"' ) LMF ON LMF.LanguageId=PL.LanguageId
									LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName=(SELECT EmploymentType FROM dbo.EmployeeInformation where SystemId='" + employeeId + @"')and LanguageId=7 ) LET ON LET.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Name' and LanguageId='" + languageId + @"' ) N ON N.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Designation'and LanguageId='" + languageId + @"' ) DN ON DN.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Department'and LanguageId='" + languageId + @"' ) DPN ON DPN.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Line'and LanguageId='" + languageId + @"' ) LN ON LN.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='IDNo'and LanguageId='" + languageId + @"' ) ID ON ID.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Permanent'and LanguageId='" + languageId + @"' ) PT ON PT.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='DOJ'and LanguageId='" + languageId + @"' ) DJ ON DJ.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmergencyTelNo'and LanguageId='" + languageId + @"' ) ET ON ET.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='BloodGroup'and LanguageId='" + languageId + @"' ) BGP ON BGP.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='NIDNo'and LanguageId='" + languageId + @"' ) NID ON BGP.LanguageId=PL.LanguageId
	                                LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Permanent'and LanguageId='" + languageId + @"' ) PML ON PML.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Address'and LanguageId='" + languageId + @"' ) LA ON LA.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='MobileNo'and LanguageId='" + languageId + @"' ) LMB ON LMB.LanguageId=PL.LanguageId
									LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Probationer'and LanguageId='" + languageId + @"') LPRL ON LPRL.LanguageId = PL.LanguageId
									LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Permanent' and LanguageId='" + languageId + @"') PTl ON PTl.LanguageId=PL.LanguageId
									LEFT JOIN HKP.LocalLanguage SEC ON SEC.SectionId = E.SectionId AND PL.LanguageId = SEC.LanguageId  AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage CAC ON CAC.DepartmentId =E.DepartmentId AND PL.LanguageId=CAC.LanguageId  AND PL.LanguageId='" + languageId + @"'
                                    WHERE E.SystemID ='" + employeeId + @"') TAB1 LEFT JOIN SCS.Language AS LAN ON LAN.Id=TAB1.LanguageId) TAB2 LEFT JOIN MST.AddressMaster AS AM ON AM.Id=TAB2.AddressMasterId) TAB3 
									LEFT JOIN  (SELECT * FROM SCS.RptConfigTemplate WHERE Id='" + tempId + @"'  and PlantId='" + plantId + @"') AS RPTM ON TAB3.PlantId=RPTM.PlantId";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetOverTimePmtPolicy(string employeeId)
        {
            try
            {
                string sql = @"  SELECT ei.SystemId, Isnull(dmc.IsOTEntitled,0) IsOTEntitled, d.*
                                 FROM EmployeeInformation ei
                                 LEFT JOIN mst.DesignationMasterLegalDesignation AS dmld ON dmld.LegalDesignationId=ei.LegalDesignationId---
                                 LEFT JOIN MST.DesignationMaster DM ON DM.id=dmld.DesignationMasterId----
                                 LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId = DM.Id AND dmc.PlantId=ei.PlantId
                                 LEFT JOIN OverTimePmtPolicyMaster m on m.PlantID=ei.PlantId
                                 LEFT JOIN OverTimePmtPolicyDetails d on d.OverTimePmtPolicyID=m.ID 
                                 WHERE ei.SystemId='" + employeeId + @"'  and d.OverTimeDayType='Working Day' ";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public DataTable GetMultipleEmployeeInfoById(string employeeId, string plantId, string languageId, string tempId)
        {
            try
            {

                string sql = @"SELECT isnull(A.[Name],CG.UserName)CompanyName,
                                     E.EmployeeCode,CASE WHEN ISNULL(cg.Id,'')='' THEN isnull(E.EmployeeNameLocal,E.EmployeeName) ELSE EmployeeName END AS EmployeeName
                                    ,ISNULL(LD.Name,LDN.UserName) DesignationName
                                    ,ISNULL(SEC.Name,SE.UserName) Section
	                                ,ISNULL(DPL.Name,DP.UserName) Department
                                    ,FORMAT(E.DOJ,'dd-MMM-yyyy') DOJ
                                    ,FORMAT(E.DOB,'dd-MMM-yyyy') DateOfBirth
                                    ,FORMAT(M.IssueDate,'dd-MMM-yyyy') IssueDate
                                    ,E.CellPhnNo
                                    ,E.EmpPicPath EmployeePic,E.NationalID,BG.UserName BloodGroup
                                    ,CASE WHEN ISNULL(CG.Id,'')='' THEN ISNULL(E.ParmanentAddress1Local,E.ParmanentAddress1) ELSE ParmanentAddress1 END AS ParmanentAddress
                                    ,ISNULL(WSEC.Name,WT.UserName) EmployeeWorkType, ISNULL(PLL.Name,PL.UserName) PlantName, ISNULL(LN.Name,L.UserName) Line, ISNULL(LSGA.Name,GD.ShortName) Grade ,P.AuthorizedSignature
                                    ,EmrCntPer1CellNo,FatherOrSpouse = case when E.FatherName is null then e.SpouseName else E.FatherName  end,CONCAT(e.SystemId,'#',e.EmployeeCode,'#',e.EmployeeName)BarCodeId
                                    ,case when isnull(cg.Id,'')='' THEN isnull(E.PresentAddress1Local,E.PresentAddress1) ELSE PresentAddress1 END AS PresentAddress
                                    ,case when isnull(cg.Id,'')='' THEN isnull(E.FatherNameLocal,E.FatherName) ELSE FatherName END AS FatherName
                                    FROM EmployeeInformation E
                                    LEFT JOIN ORG.CompanyGroup CG ON E.GroupID=cg.Id and CG.LanguageId='" + languageId + @"'
                                    LEFT JOIN ORG.Plant PL ON PL.Id=E.PlantId
                                    LEFT JOIN [SCS].[PlantSetting] P ON P.PlantId=E.PlantId AND P.ModuleName='HR'
                                    LEFT JOIN ORG.Line L ON L.Id=E.LineId
	                                LEFT JOIN MST.ManpowerBudget bbb ON e.BudgetCode = bbb.Id
	                                LEFT JOIN ORG.Position PS ON PS.Id=bbb.PositionId
	                                LEFT JOIN ORG.Department DP ON DP.Id=PS.DepartmentId
                                    LEFT JOIN HKP.LegalDesignation LDN ON LDN.Id=E.LegalDesignationId
                                    LEFT JOIN HKP.LocalLanguage LD ON LD.LegalDesignationId=E.LegalDesignationId AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage PLL ON PLL.PlantId=E.PlantId AND PLL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LN ON LN.LineId=E.LineId AND LN.LanguageId='" + languageId + @"'
                                    LEFT JOIN ORG.Section SE ON SE.Id=PS.SectionId
                                    LEFT JOIN HKP.LocalLanguage A ON A.CompanyId=E.CompanyId AND A.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage SEC ON SEC.SectionId = PS.SectionId AND PL.LanguageId='" + languageId + @"'
	                                LEFT JOIN HKP.LocalLanguage DPL ON DPL.DepartmentId = PS.DepartmentId AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.BloodGroup BG ON BG.Id = E.BloodGroupID
                                    LEFT JOIN (
						                SELECT  LSGD.PlantId,LSGD.LegalDesignationId,LS.ShortName,LSGD.LegalSalaryGradeId from [MST].[LegalSalaryGradeDesignation] LSGD
                                        LEFT JOIN [SCS].[LegalSalaryGrade] LS ON LS.Id=LSGD.LegalSalaryGradeId 
						                       ) GD ON GD.PlantId=E.PlantId AND GD.LegalDesignationId=E.LegalDesignationId
                                    LEFT JOIN HKP.LocalLanguage LSGA ON LSGA.LegalSalaryGradeId=GD.LegalSalaryGradeId AND LSGA.LanguageId='" + languageId + @"'
                                    LEFT JOIN [EmployeeIdCardIssue] M ON m.EmpSystemId=e.SystemId
                                AND m.Id=(SELECT TOP 1 ID FROM [EmployeeIdCardIssue] EII WHERE EII.EmpSystemId=e.SystemId ORDER BY EII.Sequence DESC )
                                LEFT JOIN [dbo].[EmployeeWorkType] WT ON WT.Id=m.EmployeeWorkTypeId
                                LEFT JOIN HKP.LocalLanguage WSEC ON WSEC.EmployeeWorkTypeId = m.EmployeeWorkTypeId AND PL.LanguageId='" + languageId + @"'
                                Where E.SystemId IN(" + employeeId + ") AND E.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetEmployeeBasicInfoById(string employeeId, string plantId, string employeementType, string languageId, string tempId)
        {
            try
            {

                string sql = @"SELECT  EmpSystemID,TodaysDate,Dateofconfirmation,NextDayFromconfirmation,
                            ISNULL(FatherNameLocal,FatherName) FatherName,  FatherName FatherNameEng,
                            ISNULL(MotherNameLocal,MotherName) MotherName,  MotherName MotherNameEng,
                            ISNULL(EmployeeNameLocal,EmployeeName) EmployeeName, EmployeeName EmployeeNameEng,
                            ISNULL(LocalCompanyName,CompanyName) CompanyName,
                            ISNULL(CompanyAddress,CompanyAddress) CompanyAddress,
                            ISNULL(UtilityName,UtilityName) UtilityName,
                            ISNULL(ParmanentAddress1Local1,ParmanentAddress1) ParmanentAddress1,
                            ISNULL(PresentAddress1Local1,PresentAddress1) PresentAddress1,
                            ISNULL(PresentCity,PresentCity) PresentCity,
                            ISNULL(PresentDistrict,PresentDistrict) PresentDistrict,
                            ISNULL(PresentState,PresentState) PresentState,
                            ISNULL(LPresentCountry,LPermanentCountry) LPresentCountry,
                            ISNULL(FirstName,FirstName) FirstName,
                            ISNULL(LegalDesignationLocal,LegalDesignation) DesignationName,
                            ISNULL(LocalDesignationName,DesignationName) LocalDesignationName,
                            DateOfJoin DOJ,
                            ISNULL(DateOfJoin,DateOfJoin) DateOfJoin,
                            ISNULL(confirm,confirm) confirm,
                            ISNULL(MobileNo,MobileNo) MobileNo,
                            ISNULL(LocalDepartmentName,Department) Department,DepartmentLabel,
                            Section,
                            ISNULL(Unit,Unit) Unit,
                            ISNULL(DOC,DOC) DOC,
                            ISNULL(NationalID,NationalID) NationalID,
                            ISNULL(EmployeeCode,EmployeeCode) EmployeeCode,
                            ISNULL(BloodGroup,BloodGroup) BloodGroup,
                            ISNULL(EmployeePic,EmployeePic) EmployeePic,
                            ISNULL(SpouseNameLocal,SpouseName) SpouseName,DOB,SubSection,
                            ISNULL(LocalIdentificationMark,IdentificationMark) IdentificationMark,Age,HightFt,HightInc,AuthorizedSignature,CardHolderSignature,EmployeeFingerPrint --,
                            ,ISNULL((Case When  GenderID ='Male' then     MaleLocal else FemaleLocal end),GenderID) Gender,GenderID
                                    --RPTM.TemplateFileName 
                                    FROM(SELECT TAB2.*, AM.Phone, AM.Email, AM.Website, AM.Address1 FROM (SELECT TAB1.*, LAN.StandardName 
                                    FROM (SELECT CM.Image CompanyLogo,E.SystemID as EmpSystemID,
                                    CM.UserName CompanyName,AM.Address1 CompanyAddress,E.EmployeeName,
                                    E.FatherName,E.MotherName,e.FatherNameLocal,e.MotherNameLocal,E.EmpPicPath EmployeePic,E.EmployeeCode, Convert(varchar, E.DOJ, 105) DOJ,
                                    REPLACE(CONVERT(VARCHAR(11),E.DOJ,106),' ','-') DateOfJoin,BG.UserName BloodGroup
                                    ,E.NationalID,E.EmploymentType,D.UserName DesignationName, dm.EmployeeCategoryId,ec.UserName EmployeeCategory,L.UserName Line,
			                		E.EmpSignature CardHolderSignature,P.AuthorizedSignature
                                    ,E.CellPhnNo MobileNo,E.ParmanentAddress1,DP.UserName Department,ISNULL(SEC.Name,SE.UserName) Section,A.[Name] LocalCompanyName, B.[Name] LocalDesignationName,C.[Name] LocalDepartmentName,
			                		N.Name NameLabel
                                    ,DN.Name DesignationLabel,DPN.Name DepartmentLabel,LN.Name LineLabel,LET.Name EmploymentTypeLabel, ID.Name IDNoLabel,  PT.Name EmploymentTypeName,
			                		DJ.Name DOJLabel, ET.Name EmergencyTellNoLabel, BGP.Name BloodGroupLabel
                                    ,E.EmployeeNameLocal,LL.UtilityName,NID.Name NIDLabel, E.ParmanentAddress1Local, (PML.Name+' '+LA.Name) ParmanentAddress,LMB.Name MobileNoLabel,
			                		LD.Name LegalDesignationLocal
                                    ,Convert(varchar, DATEADD(year, 5, E.DOJ),105) AS Validity,LNN.Name LineLocal,UN.Username Unit, Convert(varchar, E.DOC, 105) DOC
                                    ,PCN.Name LPermanentCountry,PRCN.Name LPresentCountry,E.PresentAddress1
			                		,PD.Name PermanentDistrict,PRD.Name PresentDistrict,PST.Name PermanentState, PRST.Name PresentState,PCT.Name PermanentCity, PRCT.Name PresentCity
                                    ,CASE WHEN DOCDay=0 THEN DOCMonth ELSE DOCDay/30 END AS confirm, PL.LanguageId, PL.Id as 'PlantId', CM.AddressMasterId,E.FirstName,E.SpouseName,E.SpouseNameLocal,format(E.DOB,'dd-MMM-yyyy') DOB
                                    ,LocalIdentificationMark,IdentificationMark,cast((DATEDIFF(m, DOB, GETDATE())/12) as varchar) Age,FLOOR(Height) AS HightFt,CEILING((Height*12)%12) HightInc,E.PresentAddress1Local PresentAddress1Local1,E.ParmanentAddress1Local ParmanentAddress1Local1,efp.FileName EmployeeFingerPrint,ISNULL( SBL.Name,SB.username)  SubSection,LDN.UserName LegalDesignation 
                                    ,LMM.Name MaleLocal, LMF.Name FemaleLocal,E.GenderID   
                                    ,Format(GETDATE(),'dd-MMM-yyy') as TodaysDate
	                                ,FORMAT( DATEADD(DAY, DOCDay, DOJ),'dd-MMM-yyy') as Dateofconfirmation 
									,FORMAT( DATEADD(DAY, DOCDay+1, DOJ),'dd-MMM-yyy') as NextDayFromconfirmation 
                                    FROM EmployeeInformation E
                                    LEFT JOIN ORG.Company CM ON CM.Id = E.CompanyId
                                    LEFT JOIN MST.AddressMaster AM ON AM.Id = CM.AddressMasterId
                                    LEFT JOIN HKP.BloodGroup BG ON BG.Id = E.BloodGroupID
                                    LEFT JOIN HKP.Designation D ON D.Id = E.GivenDesignationId
                                    LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = e.GivenDesignationId
                                    LEFT JOIN  hkp.LegalDesignation LDN ON LDN.Id=E.LegalDesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id = DM.EmployeeCategoryId
                                    LEFT JOIN ORG.Line L ON L.Id=E.LineId
                                    LEFT JOIN ORG.Unit UN ON UN.Id=E.UnitId
			                		LEFT JOIN [SCS].[PlantSetting] P ON P.PlantId=E.PlantId
                                    LEFT JOIN ORG.Department DP ON DP.Id=E.DepartmentId
                                    LEFT JOIN org.Section SE ON SE.Id=E.SectionId
			                		LEFT JOIN ORG.Plant PL ON PL.Id=E.PlantId
                                    LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                                    LEFT JOIN HKP.LocalLanguage SEC ON SEC.SectionId = E.SectionId AND PL.LanguageId = SEC.LanguageId AND PL.LanguageId='" + languageId + @"'
									LEFT JOIN EmployeeFingerPrint efp ON efp.EmpSystemID=E.SystemId AND efp.Id=(SELECT TOP 1 Id FROM EmployeeFingerPrint WHERE EmpSystemID=E.SystemId)
			                		LEFT JOIN HKP.LocalLanguage A ON A.CompanyId=E.CompanyId AND A.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage SBL ON SBL.SubSectionId=E.SubSectionId AND SBL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LL ON LL.CompanyId=E.CompanyId AND LL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=E.GivenDesignationId AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage C ON C.DepartmentId =E.DepartmentId AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LD ON LD.LegalDesignationId=E.LegalDesignationId AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LNN ON LNN.LineId=E.LineId AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage PCN ON PCN.CountryId=E.ParmCountryID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PRCN ON PRCN.CountryId=E.ParmCountryID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PD ON PD.DistrictId=E.ParmDistrictID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PRD ON PRD.DistrictId=E.PresDistrictID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PST ON PST.StateId=E.ParmStateId AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PRST ON PRST.StateId=E.PresStateId AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PCT ON PCT.CityId=E.ParmCityID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PRCT ON PRCT.CityId=E.PresCityID AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Male'and LanguageId='" + languageId + @"' ) LMM ON LMM.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Female'and LanguageId='" + languageId + @"' ) LMF ON LMF.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Name' and LanguageId='" + languageId + @"' ) N ON N.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Designation'and LanguageId='" + languageId + @"' ) DN ON DN.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Department'and LanguageId='" + languageId + @"' ) DPN ON DPN.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Line'and LanguageId='" + languageId + @"' ) LN ON LN.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmploymentType'and LanguageId='" + languageId + @"' ) LET ON LET.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='IDNo'and LanguageId='" + languageId + @"' ) ID ON ID.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='" + employeementType + @"'and LanguageId='" + languageId + @"' ) PT ON PT.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='DOJ'and LanguageId='" + languageId + @"' ) DJ ON DJ.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmergencyTelNo'and LanguageId='" + languageId + @"' ) ET ON ET.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='BloodGroup'and LanguageId='" + languageId + @"' ) BGP ON BGP.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='NIDNo'and LanguageId='" + languageId + @"' ) NID ON BGP.LanguageId=PL.LanguageId
	                                LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Permanent'and LanguageId='" + languageId + @"' ) PML ON PML.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Address'and LanguageId='" + languageId + @"' ) LA ON LA.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='MobileNo'and LanguageId='" + languageId + @"' ) LMB ON LMB.LanguageId=PL.LanguageId
                                    WHERE E.SystemID ='" + employeeId + "') TAB1 " +
                                    "LEFT JOIN SCS.Language AS LAN ON LAN.Id=TAB1.LanguageId) TAB2 LEFT JOIN MST.AddressMaster AS AM ON AM.Id=TAB2.AddressMasterId) TAB3 " +
                                    "--LEFT JOIN  (SELECT * FROM SCS.RptConfigTemplate WHERE Language='" + tempId + "'  and PlantId='" + plantId + @"') AS RPTM ON TAB3.PlantId=RPTM.PlantId";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetEmployeeconfirmationBasicInfoById(string employeeId, string plantId, string employeementType, string languageId, string tempId)
        {
            try
            {
                string sql = @"SELECT  EmpSystemID,TodaysDate,Dateofconfirmation,NextDayFromconfirmation,GenderMrMS,
                            ISNULL(FatherNameLocal,FatherName) FatherName,  FatherName FatherNameEng,
                            ISNULL(MotherNameLocal,MotherName) MotherName,  MotherName MotherNameEng,
                            ISNULL(EmployeeNameLocal,EmployeeName) EmployeeName, EmployeeName EmployeeNameEng,
                            ISNULL(LocalCompanyName,CompanyName) CompanyName,
                            ISNULL(CompanyAddress,CompanyAddress) CompanyAddress,
                            ISNULL(UtilityName,UtilityName) UtilityName,
                            ISNULL(ParmanentAddress1Local1,ParmanentAddress1) ParmanentAddress1,
                            ISNULL(PresentAddress1Local1,PresentAddress1) PresentAddress1,
                            ISNULL(PresentCity,PresentCity) PresentCity,
                            ISNULL(PresentDistrict,PresentDistrict) PresentDistrict,
                            ISNULL(PresentState,PresentState) PresentState,
                            ISNULL(LPresentCountry,LPermanentCountry) LPresentCountry,
                            ISNULL(FirstName,FirstName) FirstName,
                            ISNULL(LegalDesignationLocal,LegalDesignation) DesignationName,
                            ISNULL(LocalDesignationName,DesignationName) LocalDesignationName,
                            DateOfJoin DOJ,
                            ISNULL(DateOfJoin,DateOfJoin) DateOfJoin,
                            ISNULL(confirm,confirm) confirm,
                            ISNULL(MobileNo,MobileNo) MobileNo,
                            ISNULL(LocalDepartmentName,Department) Department,DepartmentLabel,
                            Section,
                            ISNULL(Unit,Unit) Unit,
                            ISNULL(DOC,DOC) DOC,
                            ISNULL(NationalID,NationalID) NationalID,
                            ISNULL(EmployeeCode,EmployeeCode) EmployeeCode,
                            ISNULL(BloodGroup,BloodGroup) BloodGroup,
                            ISNULL(EmployeePic,EmployeePic) EmployeePic,
                            ISNULL(SpouseNameLocal,SpouseName) SpouseName,DOB,SubSection,
                            ISNULL(LocalIdentificationMark,IdentificationMark) IdentificationMark,Age,HightFt,HightInc,AuthorizedSignature,CardHolderSignature,EmployeeFingerPrint --,
                            ,ISNULL((Case When  GenderID ='Male' then     MaleLocal else FemaleLocal end),GenderID) Gender,GenderID
                                    FROM(SELECT TAB2.*, AM.Phone, AM.Email, AM.Website, AM.Address1 FROM (SELECT TAB1.*, LAN.StandardName 
                                    FROM (SELECT CM.Image CompanyLogo,E.SystemID as EmpSystemID,
                                    CM.UserName CompanyName,AM.Address1 CompanyAddress,E.EmployeeName,
                                    E.FatherName,E.MotherName,e.FatherNameLocal,e.MotherNameLocal,E.EmpPicPath EmployeePic,E.EmployeeCode, Convert(varchar, E.DOJ, 105) DOJ,
                                    REPLACE(CONVERT(VARCHAR(11),E.DOJ,106),' ','-') DateOfJoin,BG.UserName BloodGroup
                                    ,E.NationalID,E.EmploymentType,D.UserName DesignationName, dm.EmployeeCategoryId,ec.UserName EmployeeCategory,L.UserName Line,
			                		E.EmpSignature CardHolderSignature,P.AuthorizedSignature
                                    ,E.CellPhnNo MobileNo,E.ParmanentAddress1,DP.UserName Department,ISNULL(SEC.Name,SE.UserName) Section,A.[Name] LocalCompanyName, B.[Name] LocalDesignationName,C.[Name] LocalDepartmentName,
			                		N.Name NameLabel
                                    ,DN.Name DesignationLabel,DPN.Name DepartmentLabel,LN.Name LineLabel,LET.Name EmploymentTypeLabel, ID.Name IDNoLabel,  PT.Name EmploymentTypeName,
			                		DJ.Name DOJLabel, ET.Name EmergencyTellNoLabel, BGP.Name BloodGroupLabel
                                    ,E.EmployeeNameLocal,LL.UtilityName,NID.Name NIDLabel, E.ParmanentAddress1Local, (PML.Name+' '+LA.Name) ParmanentAddress,LMB.Name MobileNoLabel,
			                		LD.Name LegalDesignationLocal
                                    ,Convert(varchar, DATEADD(year, 5, E.DOJ),105) AS Validity,LNN.Name LineLocal,UN.Username Unit, Convert(varchar, E.DOC, 105) DOC
                                    ,PCN.Name LPermanentCountry,PRCN.Name LPresentCountry,E.PresentAddress1
			                		,PD.Name PermanentDistrict,PRD.Name PresentDistrict,PST.Name PermanentState, PRST.Name PresentState,PCT.Name PermanentCity, PRCT.Name PresentCity
                                    ,CASE WHEN DOCDay=0 THEN DOCMonth ELSE DOCDay/30 END AS confirm, PL.LanguageId, PL.Id as 'PlantId', CM.AddressMasterId,E.FirstName,E.SpouseName,E.SpouseNameLocal,format(E.DOB,'dd-MMM-yyyy') DOB
                                    ,LocalIdentificationMark,IdentificationMark,cast((DATEDIFF(m, DOB, GETDATE())/12) as varchar) Age,FLOOR(Height) AS HightFt,CEILING((Height*12)%12) HightInc,E.PresentAddress1Local PresentAddress1Local1,E.ParmanentAddress1Local ParmanentAddress1Local1,efp.FileName EmployeeFingerPrint,ISNULL( SBL.Name,SB.username)  SubSection,LDN.UserName LegalDesignation 
                                    ,LMM.Name MaleLocal, LMF.Name FemaleLocal,E.GenderID   
                                    ,Format(GETDATE(),'dd-MMM-yyy') as TodaysDate
	                                ,FORMAT(e.DOC,'dd-MMM-yyy') as Dateofconfirmation
									,FORMAT(DATEADD(DAY,1,DOC),'dd-MMM-yyy')AS NextDayFromconfirmation
                                    ,GenderMrMS=case when E.GenderID='Male' then 'Mr.' else 'Ms.' end
                                    FROM EmployeeInformation E
                                    LEFT JOIN ORG.Company CM ON CM.Id = E.CompanyId
                                    LEFT JOIN MST.AddressMaster AM ON AM.Id = CM.AddressMasterId
                                    LEFT JOIN HKP.BloodGroup BG ON BG.Id = E.BloodGroupID
                                    LEFT JOIN HKP.Designation D ON D.Id = E.GivenDesignationId
                                    LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = e.GivenDesignationId
                                    LEFT JOIN  hkp.LegalDesignation LDN ON LDN.Id=E.LegalDesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id = DM.EmployeeCategoryId
                                    LEFT JOIN ORG.Line L ON L.Id=E.LineId
                                    LEFT JOIN ORG.Unit UN ON UN.Id=E.UnitId
			                		LEFT JOIN [SCS].[PlantSetting] P ON P.PlantId=E.PlantId
                                    LEFT JOIN ORG.Department DP ON DP.Id=E.DepartmentId
                                    LEFT JOIN org.Section SE ON SE.Id=E.SectionId
			                		LEFT JOIN ORG.Plant PL ON PL.Id=E.PlantId
                                    LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                                    LEFT JOIN HKP.LocalLanguage SEC ON SEC.SectionId = E.SectionId AND PL.LanguageId = SEC.LanguageId AND PL.LanguageId='" + languageId + @"'
									LEFT JOIN EmployeeFingerPrint efp ON efp.EmpSystemID=E.SystemId AND efp.Id=(SELECT TOP 1 Id FROM EmployeeFingerPrint WHERE EmpSystemID=E.SystemId)
			                		LEFT JOIN HKP.LocalLanguage A ON A.CompanyId=E.CompanyId AND A.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage SBL ON SBL.SubSectionId=E.SubSectionId AND SBL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LL ON LL.CompanyId=E.CompanyId AND LL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=E.GivenDesignationId AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage C ON C.DepartmentId =E.DepartmentId AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LD ON LD.LegalDesignationId=E.LegalDesignationId AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage LNN ON LNN.LineId=E.LineId AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage PCN ON PCN.CountryId=E.ParmCountryID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PRCN ON PRCN.CountryId=E.ParmCountryID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PD ON PD.DistrictId=E.ParmDistrictID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PRD ON PRD.DistrictId=E.PresDistrictID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PST ON PST.StateId=E.ParmStateId AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PRST ON PRST.StateId=E.PresStateId AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PCT ON PCT.CityId=E.ParmCityID AND PL.LanguageId='" + languageId + @"'
			                		LEFT JOIN HKP.LocalLanguage PRCT ON PRCT.CityId=E.PresCityID AND PL.LanguageId='" + languageId + @"'
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Male'and LanguageId='" + languageId + @"' ) LMM ON LMM.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Female'and LanguageId='" + languageId + @"' ) LMF ON LMF.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Name' and LanguageId='" + languageId + @"' ) N ON N.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Designation'and LanguageId='" + languageId + @"' ) DN ON DN.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Department'and LanguageId='" + languageId + @"' ) DPN ON DPN.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Line'and LanguageId='" + languageId + @"' ) LN ON LN.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmploymentType'and LanguageId='" + languageId + @"' ) LET ON LET.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='IDNo'and LanguageId='" + languageId + @"' ) ID ON ID.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='" + employeementType + @"'and LanguageId='" + languageId + @"' ) PT ON PT.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='DOJ'and LanguageId='" + languageId + @"' ) DJ ON DJ.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmergencyTelNo'and LanguageId='" + languageId + @"' ) ET ON ET.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='BloodGroup'and LanguageId='" + languageId + @"' ) BGP ON BGP.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='NIDNo'and LanguageId='" + languageId + @"' ) NID ON BGP.LanguageId=PL.LanguageId
	                                LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Permanent'and LanguageId='" + languageId + @"' ) PML ON PML.LanguageId=PL.LanguageId
			                		LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Address'and LanguageId='" + languageId + @"' ) LA ON LA.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='MobileNo'and LanguageId='" + languageId + @"' ) LMB ON LMB.LanguageId=PL.LanguageId
                                    WHERE E.SystemID ='" + employeeId + "') TAB1 " +
                                    "LEFT JOIN SCS.Language AS LAN ON LAN.Id=TAB1.LanguageId) TAB2 LEFT JOIN MST.AddressMaster AS AM ON AM.Id=TAB2.AddressMasterId) TAB3 " +
                                    "--LEFT JOIN  (SELECT * FROM SCS.RptConfigTemplate WHERE Language='" + tempId + "'  and PlantId='" + plantId + @"') AS RPTM ON TAB3.PlantId=RPTM.PlantId";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Dictionary<string, object> GetFilePath(string plantId, string pkId, string reportType)
        {
            var sql = @"SELECT Id,TemplateFileName FROM SCS.RptConfigTemplate WHERE  Language='" + pkId + "'  AND PlantId='" + plantId + "' and Type='" + reportType + "'";
            return _sqlRepository.GetData(sql);
        }

        public Dictionary<string, object> GetIdCardFilePath(string plantId, string pkId, string reportType, string tempId)
        {
            var sql = @"SELECT Id,TemplateFileName FROM SCS.RptConfigTemplate WHERE  Language='" + pkId + "'  AND PlantId='" + plantId + "' and Type='" + reportType + "' AND Id='" + tempId + "'";
            return _sqlRepository.GetData(sql);
        }


        public Dictionary<string, object> GetAppointmentFilePath(string plantId, string Language, string pkId, string reportType)
        {
            var sql = @"SELECT Id,TemplateFileName FROM SCS.RptConfigTemplate WHERE  Language='" + Language + "'  AND PlantId='" + plantId + "' and Type='" + reportType + "' and Id='" + pkId + "'";
            return _sqlRepository.GetData(sql);
        }

        public Dictionary<string, object> GetLanguage(string plantId, string pkId, string templateType)
        {
            Library.Service.Enums.LetterType.ServiceBook.GetDescription();
            var sql = @"SELECT Id,Language FROM SCS.RptConfigTemplate WHERE  Id='" + pkId + "'  AND PlantId='" + plantId + "' and type='" + templateType + "'";
            //var sql = "SELECT Id,Language FROM SCS.RptConfigTemplate WHERE  [type]='" + pkId + "'  AND PlantId='" + plantId + "'";
            return _sqlRepository.GetData(sql);
        }

        public IEnumerable<ComboModel> GetCbo(string plantId)
        {
            var sql = @"SELECT Id,FormatName FROM SCS.RptConfigTemplate WHERE Type='Appointment Letter' AND PlantId='" + plantId + "' ORDER BY FormatName";
            return _sqlRepository.GetCombo(sql, "Id", "FormatName");
        }

        public IEnumerable<ComboModel> GetRelationCbo()
        {
            var sql = @"SELECT Id,UserName FROM SCS.Relationship order by Sequence";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public IEnumerable<ComboModel> GetProfessionCbo()
        {
            try
            {
                var sql = @"SELECT Id,UserName FROM SCS.Profession order by Sequence";
                return _sqlRepository.GetCombo(sql, "Id", "UserName");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<ComboModel> GetTemplateCbo(string plantId, string type)
        {
            var sql = @"SELECT Id,FormatName FROM SCS.RptConfigTemplate WHERE Type='" + type + @"' AND PlantId='" + plantId + "' ORDER BY FormatName";
            return _sqlRepository.GetCombo(sql, "Id", "FormatName");
        }

        public Dictionary<string, object> GetDefaultCompanyGroupLanguage(string companyGrupId)
        {
            var sql = @"SELECT CG.LanguageId Id,L.UserName FROM ORG.CompanyGroup CG
                        LEFT JOIN SCS.[Language] L ON L.Id=CG.LanguageId
                        WHERE CG.Id='" + companyGrupId + @"'
                        ORDER BY UserName";
            return _sqlRepository.GetData(sql, null);
        }

        public Dictionary<string, object> GetDefaultPlantLanguage(string plantId)
        {
            var sql = @"
                        SELECT P.LanguageId Id,PL.UserName  FROM ORG.Plant P
                        LEFT JOIN SCS.[Language] PL ON PL.Id=P.LanguageId
                        WHERE P.Id='" + plantId + @"'
                        ORDER BY UserName";
            return _sqlRepository.GetData(sql, null);
        }

        public IEnumerable<ComboModel> GetDefaultCbo(string companyGrupId, string plantId)
        {
            var sql = @"SELECT L.Id ,L.UserName FROM ORG.CompanyGroup CG
                        LEFT JOIN SCS.[Language] L ON L.Id=CG.LanguageId
                        WHERE CG.Id='" + companyGrupId + @"'
                        UNION
                        SELECT PL.Id ,PL.UserName  FROM ORG.Plant P
                        LEFT JOIN SCS.[Language] PL ON PL.Id=P.LanguageId
                        WHERE P.Id='" + plantId + @"'
                        ORDER BY UserName";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public bool Login(string id, int pin)
        {
            try
            {
                var emp = Query(t => t.SystemId == id).Select().FirstOrDefault();
                if (emp == null)
                    throw new CustomException("Invalid Employee Id.");
                var accessible = Query(t => t.SystemId == id).Select(t => t.IsAccessible).FirstOrDefault();
                if (!accessible)
                    throw new CustomException("No permission to access.");
                var data = _employeeAuthService.Query(t => t.EmployeeId == id).Select().FirstOrDefault();
                if (data == null)
                    throw new CustomException("Please collect your pin.");
                if (data.PIN != pin)
                    throw new CustomException("Invalid pin.");
                return true;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public IWorkbook JobCard_Report(string employeeId, string fromDate, string toDate, string companyGroupId)
        {
            ExcelEngine excelEngine = null;
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                //DataSet dsLocal = GetJobCardInfo(employeeId, fromDate, toDate);

                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_JobCard(ref sheet1, oRU, "Job Card", "Job Card", employeeId, fromDate, toDate, companyGroupId);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IWorkbook EmpInfoReport(string companyGroupId, string companyId, string plantId, string employeeId)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Employee Information ";
            workbook.Version = ExcelVersion.Excel2013;

            var dtLocal = GetEmpdata(companyGroupId, companyId, plantId);
            if (dtLocal.Rows.Count == 0)
                throw new Exception("No data found !");

            for (int n = 0; n < dtLocal.Rows.Count; n++)
            {
                int xlsCol = 1;
                int xlsRow = 5;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["DataCollectionDateLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["CardNoLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["AgeLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["NameLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["FatherNameLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["DesignationLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["LocalSectionLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["DesignationLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["LocalSectionLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["DOJLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["NIDLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["SpouseNameLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["MobileNoLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["MotherNameLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["EmploymentTypeLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["LineLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["BloodGroupLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["EmergencyTellNoLabel"].ToString()); xlsCol++;

                oRU.SetCellText(sheet, 6, 2, dtLocal.Rows[n]["CardNumber"].ToString());
                oRU.SetCellText(sheet, 7, 2, dtLocal.Rows[n]["AgeLabel"].ToString());
                oRU.SetCellText(sheet, 14, 2, dtLocal.Rows[n]["SectionName"].ToString());

                //sheet.Range[oRU.GetColumnNameForXls(2) + row].Text = _Amount;
                //sheet.Range[oRU.GetColumnNameForXls(2) + row + ":" + oRU.GetColumnNameForXls(8) + row].Merge();
                //sheet.Range[oRU.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[oRU.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[oRU.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                oRU.CompanyPlantHeader(ref sheet, 5, "Employee Report", companyId, identity.PlantName, null);
                oRU.FreezePage(ref sheet, 1, 1 - 5);
                // oRU.PageAdjustableSetup(ref sheet, 1, rowPrint, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        public IWorkbook EmpRegisterReport(string companyGroupId, string companyId, string plantId)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Employee Register Information ";
            workbook.Version = ExcelVersion.Excel2013;

            var dtLocal = GetEmpdata(companyGroupId, companyId, plantId);
            if (dtLocal.Rows.Count == 0)
                throw new Exception("No data found !");

            int xlsCol = 1;
            int xlsRow = 5;

            if (dtLocal.Rows.Count > 0)
            {
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["CardNoLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["NameLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["NIDLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["FatherNameLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["MotherNameLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["SpouseNameLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["GenderLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["DOBLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["AgeLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["PresentAddressLabel"].ToString());
                sheet.Range[xlsRow, xlsCol].ColumnWidth = 76;
                xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["ParmanentAddress"].ToString());
                sheet.Range[xlsRow, xlsCol].ColumnWidth = 76; xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["MobileNoLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["DOJLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Grade"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["PayAbleLeavelabel"].ToString()); xlsCol += 1;
                //oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["WorkingTimelabel"].ToString()); xlsCol += 1;
                //oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["BreakTimelabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["WeeklyLeaveDaysLabel"].ToString()); xlsCol += 1;

                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["RosterRelayLabel"].ToString()); xlsCol += 1;

                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["MobileNoLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["BloodGroupLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["DesignationLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["DivisionLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["LocalSectionLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["StaffCateLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["EmploymentTypeLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Salary"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Heightlbl"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["weightlabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Qualificationlabel"].ToString());
                sheet.Range[xlsRow, xlsCol].ColumnWidth = 24; xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["ExperianceLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Maritalstslabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["NumberOfChildLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["NationalityLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Religionlbl"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Bankacclabel"].ToString());// xlsCol += 1;
                char[] splitchar = { ' ' };
                string NomineeLabel = dtLocal.Rows[0]["Nomineelabl"].ToString().Split(splitchar)[0]; xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, NomineeLabel + " " + dtLocal.Rows[0]["NameLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, NomineeLabel + " " + dtLocal.Rows[0]["PresentAddressLabel"].ToString());
                sheet.Range[xlsRow, xlsCol].ColumnWidth = 76; xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, NomineeLabel + " " + dtLocal.Rows[0]["MobileNoLabel"].ToString()); xlsCol += 1;

                string NameLabel = dtLocal.Rows[0]["NameLabel"].ToString().Split(splitchar)[0];
                string LandOwnr = dtLocal.Rows[0]["LandOwnerNameLabel"].ToString().Split(splitchar)[0];
                string LandOwnrmnam = dtLocal.Rows[0]["LandOwnerNameLabel"].ToString().Split(splitchar)[1];

                string LandOwnrlabel = LandOwnr + " " + LandOwnrmnam + " " + NameLabel;
                string LandOwnrmoblabel = LandOwnr + " " + LandOwnrmnam + " " + dtLocal.Rows[0]["MobileNoLabel"].ToString();
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, LandOwnrlabel); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, LandOwnrmoblabel); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Commentlabel"].ToString());
            }
            xlsRow = 6;
            for (int n = 0; n < dtLocal.Rows.Count; n++)
            {
                #region --------data----------

                xlsCol = 1;

                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["EmployeeCode"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["EmployeeNameLocal"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["NationalID"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["FatherNameLocal"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["MotherNameLocal"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["SpouseNameLocal"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["GenderID"].ToString()); xlsCol++;
                var dob = Convert.ToDateTime(dtLocal.Rows[n]["DOB"].ToString());
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dob.ToString("dd-MM-yyyy"))); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["Age"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["PresentAddress1Local"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["ParmanentAddress1Local"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["CellPhnNo"].ToString())); xlsCol++;
                var doj = Convert.ToDateTime(dtLocal.Rows[n]["DOJ"].ToString());
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(doj.ToString("dd-MM-yyyy"))); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, ""); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, ""); xlsCol++;
                //oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["workingTime"].ToString())); xlsCol++;
                //oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["BreakTime"].ToString())); xlsCol++;
                //oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["Weakdays"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, ""); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, ""); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["CellPhnNo"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["BloodGroup"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["LocalDesignationName"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["LocalDepartmentName"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["SectionName"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["ProbationerName"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["EmloymentType"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["TotalSalary"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["Height"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["Weight"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["Qualification"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, ""); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["MaritalSts"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["NoOfChildren"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["Nationality"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["Religion"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["BankAccNo"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["NomineeNameLocal"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["PresentAddress1Local"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["CellPhnNo"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, ""); xlsCol++;

                xlsRow++;

                #endregion --------data----------
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            oRU.CompanyPlantHeader(ref sheet, 5, "Employee Register Report", companyId, identity.PlantName, null);
            oRU.FreezePage(ref sheet, 1, 6);
            return workbook;
        }

        private void CreateSheet_JobCard(ref IWorksheet sheet1, ReportUtility oRU, string SheetHeader, string SheetName, string employeeId, string fromDate, string toDate, string companyGroupId)
        {
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string sOfficeInTime = "00:00:00";
            string sInTime = "00:00:00";

            try
            {
                var dtLocal = GetJobCardInfo(employeeId, fromDate, toDate);

                if (dtLocal.Rows.Count == 0)
                    throw new CustomException("No Data Found!");

                #region DataSet

                xlsRow = 7;
                string strEmpCode = "";
                int iDate = 0;
                int iShiftIntime = 0;
                int iInTime = 0;
                int iInDevID = 0;
                int iOutTime = 0;
                int iOutDevID = 0;
                int iOTHr = 0;
                int iDayStatus = 0;
                int iLvShortName = 0;
                string strLateBy = "00:00:00";
                int iLateBy = 0;
                int iShiftName = 0;
                int iShiftOuttime = 0;

                if (dtLocal.Rows.Count > 0)
                {
                    for (int i = 0; i < dtLocal.Rows.Count; i++)
                    {
                        if ((string.Compare(strEmpCode.ToUpper(), dtLocal.Rows[i]["EmployeeCode"].ToString().Trim().ToUpper())) != 0)
                        {
                            #region ------------------Column Header------------------

                            xlsCol = 1;
                            xlsRow = 5;
                            sheet1.Range[xlsRow, xlsCol].Text = "Employee Code";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["EmployeeCode"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsCol = 1;
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Employee Name";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["EmployeeName"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsCol = 1;
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Text = "DOJ";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["DOJ"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsCol = 1;
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Unit";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["Unit"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsCol = 1;
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Department";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["Department"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsCol = 1;
                            xlsRow += 1;
                            //sheet1.Range[xlsRow, xlsCol].Text = "Given Designation";
                            //sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["GivenDesignation"].ToString().Trim();
                            //sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal[i]["Designation"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsRow += 1;
                            xlsCol = 5;
                            xlsRow = 6;
                            sheet1.Range[xlsRow, xlsCol].Text = "Division";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["Division"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Section";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["Section"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "SubSection";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["SubSection"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Designation";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["LegalDesignation"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                            xlsCol = 1;
                            iDate = xlsCol;
                            xlsRow += 2;
                            sheet1.Range[xlsRow, iDate].Text = "Date";
                            sheet1.Range[xlsRow, iDate].ColumnWidth = 12;
                            sheet1.Range[xlsRow, iDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            iShiftName = xlsCol;

                            sheet1.Range[xlsRow, iShiftName].Text = "Shift Name";
                            sheet1.Range[xlsRow, iShiftName].ColumnWidth = 24;
                            sheet1.Range[xlsRow, iShiftName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iShiftName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            iShiftIntime = xlsCol;
                            sheet1.Range[xlsRow, iShiftIntime].Text = "Shift InTime";
                            sheet1.Range[xlsRow, iShiftIntime].ColumnWidth = 10;
                            sheet1.Range[xlsRow, iShiftIntime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iShiftIntime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            iShiftOuttime = xlsCol;
                            sheet1.Range[xlsRow, iShiftOuttime].Text = "Shift OutTime";
                            sheet1.Range[xlsRow, iShiftOuttime].ColumnWidth = 10;
                            sheet1.Range[xlsRow, iShiftOuttime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iShiftOuttime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            iInTime = xlsCol;
                            sheet1.Range[xlsRow, iInTime].Text = "InTime";
                            sheet1.Range[xlsRow, iInTime].ColumnWidth = 10;
                            sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            iOutTime = xlsCol;
                            sheet1.Range[xlsRow, iOutTime].Text = "OutTime";
                            sheet1.Range[xlsRow, iOutTime].ColumnWidth = 10;
                            sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            //xlsCol += 1;
                            //iOTHr = xlsCol;
                            //sheet1.Range[xlsRow, iOTHr].Text = "Duration";
                            //sheet1.Range[xlsRow, iOTHr].ColumnWidth = 9;
                            //sheet1.Range[xlsRow, iOTHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            //sheet1.Range[xlsRow, iOTHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            iDayStatus = xlsCol;
                            sheet1.Range[xlsRow, iDayStatus].Text = "Day Status";
                            sheet1.Range[xlsRow, iDayStatus].ColumnWidth = 10;
                            sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            iLateBy = xlsCol;
                            sheet1.Range[xlsRow, iLateBy].Text = "Late By";
                            sheet1.Range[xlsRow, iLateBy].ColumnWidth = 7;
                            sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            iLvShortName = xlsCol;
                            sheet1.Range[xlsRow, iLvShortName].Text = "LV";
                            sheet1.Range[xlsRow, iLvShortName].ColumnWidth = 6;
                            sheet1.Range[xlsRow, iLvShortName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iLvShortName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            //xlsCol += 1;
                            //iInDevID = xlsCol;
                            //sheet1.Range[xlsRow, iInDevID].Text = "In Device ID";
                            //sheet1.Range[xlsRow, iInDevID].ColumnWidth = 12;
                            //sheet1.Range[xlsRow, iInDevID].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            //sheet1.Range[xlsRow, iInDevID].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //xlsCol += 1;
                            //iOutDevID = xlsCol;
                            //sheet1.Range[xlsRow, iOutDevID].Text = "Out Device ID";
                            //sheet1.Range[xlsRow, iOutDevID].ColumnWidth = 12;
                            //sheet1.Range[xlsRow, iOutDevID].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            //sheet1.Range[xlsRow, iOutDevID].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                            endXlsCol = xlsCol;

                            #endregion ------------------Column Header------------------
                        }
                        strEmpCode = dtLocal.Rows[i]["EmployeeCode"].ToString().Trim();

                        #region ----------------------Data-----------------------

                        xlsRow += 1;
                        sheet1.Range[xlsRow, iDate].Text = dtLocal.Rows[i]["PDate"].ToString();
                        sheet1.Range[xlsRow, iDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iShiftName].Text = dtLocal.Rows[i]["ShiftName"].ToString();
                        sheet1.Range[xlsRow, iShiftName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iShiftName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iShiftIntime].Text = dtLocal.Rows[i]["ShiftIntime"].ToString();
                        sheet1.Range[xlsRow, iShiftIntime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iShiftIntime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iShiftOuttime].Text = dtLocal.Rows[i]["ShiftOutTime"].ToString();
                        sheet1.Range[xlsRow, iShiftOuttime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iShiftOuttime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iInTime].Text = dtLocal.Rows[i]["InTimeShow"].ToString();
                        sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iOutTime].Text = dtLocal.Rows[i]["OutTimeShow"].ToString();
                        sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (dtLocal.Rows[i]["DayStatus"].ToString().Trim() == "L")
                        {
                            sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Red;
                            sheet1.Range[xlsRow, iDayStatus].Text = "P";
                        }
                        else
                        {
                            sheet1.Range[xlsRow, iDayStatus].Text = dtLocal.Rows[i]["DayStatus"].ToString().Trim();
                        }
                        sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                        sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        // xlsCol += 1;

                        if (dtLocal.Rows[i]["DayStatus"].ToString().Trim() == "L")
                        {
                            #region Late by min

                            sInTime = "00:00:00";
                            if (dtLocal.Rows[i]["InTime"].ToString().Trim() != "")
                            {
                                sInTime = dtLocal.Rows[i]["InTime"].ToString().Trim() + ":00";
                            }
                            else
                            {
                                if (dtLocal.Rows[i]["OutTime"].ToString().Trim() != "")
                                {
                                    sInTime = dtLocal.Rows[i]["OutTime"].ToString().Trim() + ":00";
                                }
                            }
                            sOfficeInTime = "00:00:00";
                            strLateBy = "00:00";
                            if (dtLocal.Rows[i]["ShiftInTime"].ToString().Trim() != "")
                            {
                                sOfficeInTime = dtLocal.Rows[i]["ShiftInTime"].ToString().Trim() + ":00";
                                //sOfficeInTime = dvLocal[i]["ShiftTime"].ToString().Trim();
                                strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                            }

                            #endregion Late by min
                        }
                        else
                        {
                            ///absent by how min

                            #region Absent by how much min

                            if (dtLocal.Rows[i]["DayStatus"].ToString().Trim() == "A")
                            {
                                sInTime = "00:00:00";
                                if (dtLocal.Rows[i]["InTime"].ToString().Trim() != "")
                                {
                                    sInTime = dtLocal.Rows[i]["InTime"].ToString().Trim() + ":00";
                                    sOfficeInTime = "00:00:00";
                                    strLateBy = "00:00";
                                    if (dtLocal.Rows[i]["ShiftInTime"].ToString().Trim() != "")
                                    {
                                        sOfficeInTime = dtLocal.Rows[i]["ShiftInTime"].ToString().Trim() + ":00";
                                        strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                                    }
                                }
                                else
                                {
                                    //if (dvAttn[i]["OutTime"].ToString().Trim() != "")
                                    //{
                                    //    sInTime = dvAttn[i]["OutTime"].ToString().Trim() + ":00";
                                    //}
                                    strLateBy = "";
                                }
                            }
                            else
                            {
                                strLateBy = "";
                            }

                            #endregion Absent by how much min
                        }

                        string dti = dtLocal.Rows[i]["dti"].ToString().Trim();
                        string dto = dtLocal.Rows[i]["dto"].ToString().Trim();
                        string _InTimeShow = dtLocal.Rows[i]["InTimeShow"].ToString().Trim();
                        string _OutTimeShow = dtLocal.Rows[i]["OutTimeShow"].ToString().Trim();
                        //sheet1.Range[xlsRow, iOTHr].Text = iOT;
                        //sheet1.Range[xlsRow, iOTHr].Text = GetDuration(dti, dto, _InTimeShow, _OutTimeShow); ;
                        //sheet1.Range[xlsRow, iOTHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet1.Range[xlsRow, iOTHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iLateBy].Text = strLateBy;
                        sheet1.Range[xlsRow, iLateBy].CellStyle.Font.Color = ExcelKnownColors.Red;
                        sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iLvShortName].Text = dtLocal.Rows[i]["Code"].ToString();
                        sheet1.Range[xlsRow, iLvShortName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iLvShortName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //sheet1.Range[xlsRow, iInDevID].Text = dtLocal.Rows[i]["InDeviceID"].ToString();
                        //sheet1.Range[xlsRow, iInDevID].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet1.Range[xlsRow, iInDevID].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //sheet1.Range[xlsRow, iOutDevID].Text = dtLocal.Rows[i]["OutDeviceID"].ToString();
                        //sheet1.Range[xlsRow, iOutDevID].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet1.Range[xlsRow, iOutDevID].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

                        #endregion Line Setup
                    }
                    xlsCol = 2;
                    xlsRow += 5;
                    endXlsCol = 7;
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Name = SheetName;
                    oRU.CompanyGroupHeader(ref sheet1, endXlsCol, "Job Card", companyGroupId);

                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].Merge();
                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet1.Range["A4"].Text = "Employee Job Card Information From Date: " + fromDate + " To Date: " + toDate;
                }

                #endregion DataSet

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<dynamic> ShowJobCard(string employeeId, string fromDate, string toDate)
        {
            //IEnumerable<JobcardVM> result=null;
            try
            {
                var dt = GetJobCardInfo(employeeId, fromDate, toDate);
                var dynamicDt = dt.ToDynamic();
                //List<dynamic> dynamicDt = dt.ToDynamic();
                return dynamicDt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<dynamic> ShowDailyAttendance(string employeeId, string FromDate, string ToDate)
        {
            //IEnumerable<JobcardVM> result=null;
            try
            {
                var dt = GetDailyAttendance(employeeId, FromDate, ToDate);
                var dynamicDt = dt.ToDynamic();
                //List<dynamic> dynamicDt = dt.ToDynamic();
                return dynamicDt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<dynamic> ShowDailyAttendance(string employeeId, string WorkingDate)
        {
            //IEnumerable<JobcardVM> result=null;
            try
            {
                var dt = GetDailyAttendance(employeeId, WorkingDate);
                var dynamicDt = dt.ToDynamic();
                //List<dynamic> dynamicDt = dt.ToDynamic();
                return dynamicDt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable GetDailyAttendance(string employeeId, string FromDate, string ToDate)
        {
            try
            {
                var sql = @"
                              SELECT E.SystemId EmpSystemId,E.EmployeeCode
	                                , E.EmployeeName
	                                , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
                                    , REPLACE(CONVERT(VARCHAR(11), AD.WorkDate, 113), ' ', '-') WorkDate
									 , AD.DayStatus
									 ,CONVERT(varchar(15),CAST(AD.InTime AS TIME),100) InTimeShow
	                                , ARIN.DeviceID InDeviceID
									 ,CONVERT(varchar(15),CAST(AD.OutTime AS TIME),100) OutTimeShow
	                                , AROUT.DeviceID OutDeviceID
									 ,CONVERT(varchar(15),CAST(SD.InTime AS TIME),100) ShiftInTimeShow
                                    ,CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100) ShiftOutTimeShow
                                    , SD.ShiftDefinationDescription ShiftName
									, AD.CountedShortLeave ShortLeave ,lt.Code LeaveType
                                    ,ad.IsManualDayStatus,ad.IsManualInTime,ad.IsManualOutTime
									 , GVD.UserName GivenDesignation,e.PlantId,e.CompanyId,c.UserName Company

                                FROM dbo.EmployeeInformation E
							                INNER JOIN (select * from dbo.AttdnProcessData )AD ON E.SystemID = AD.EmpSystemID
							                LEFT JOIN (SELECT * FROM dbo.ShiftTimeChgMaster WHERE ('" + FromDate + @"' BETWEEN FromDate AND ToDate
                                                                                               or     '" + ToDate + @"' BETWEEN FromDate AND ToDate)
                                                                                        ) AS SFCG
																                ON AD.ShiftSystemID = SFCG.ShiftDefinationID
							                LEFT JOIN dbo.ShiftDefination SD ON AD.ShiftSystemID = SD.SystemID
							                LEFT JOIN dbo.AttdnRawData ARIN ON AD.InTimeRowID = ARIN.RowID
							                LEFT JOIN dbo.AttdnRawData AROUT ON AD.OutTimeRowID = AROUT.RowID
											left join LeaveType lt on lt.Id=ad.LTSystemID
                                            LEFT JOIN
												(
												SELECT LogDownLoadNum
												,min(ptime) ptime
												from AttdnRawData
												where pdate between '" + FromDate + @"' and '" + ToDate + @"'
												group by LogDownLoadNum
												) LIT on LIT.LogDownLoadNum=E.SystemId
                                            LEFT JOIN AttdnRawData ARD ON ARD.LogDownLoadNum=LIT.LogDownLoadNum  AND ARD.PTime=LIT.ptime
							                                       LEFT JOIN org.Unit U ON E.UnitID = U.Id
                                            LEFT JOIN org.Division Dv ON E.DivisionID = Dv.Id
                                            LEFT JOIN org.SubDivision SubDv ON E.SubdivisionID = SubDv.Id
                                            LEFT JOIN org.Department Dp ON E.DepartmentID = Dp.Id
                                            LEFT JOIN org.Section S ON E.SectionID = S.Id
                                            LEFT JOIN org.SubSection SB ON E.SubSectionID = SB.Id
                                            LEFT JOIN org.Line L ON E.LineID = L.Id
                                            LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupID = Dg.Id
                                            LEFT JOIN hkp.Designation D ON E.DesignationSystemID = D.Id
                                            LEFT JOIN hkp.Designation GVD ON E.GivenDesignationId = GVD.Id
                                            left join org.Company c on c.id=e.CompanyId
                                            LEFT JOIN
                                            (
                                            SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
											LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
											)EC ON EC.DesignationId=E.GivenDesignationId

			                    WHERE AD.WorkDate  between '" + FromDate + @"' and '" + ToDate + @"'
								AND E.EmployeeStatus='Active'
								and e.systemid='" + employeeId + @"'
                                ";
                var list = _sqlRepository.GetDataTable(sql);
                if (list.IsNull())
                    throw new CustomException("No Data Found");
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetDailyAttendance(string employeeId, string WorkingDate)
        {
            try
            {
                var sql = @"
                              SELECT E.SystemId EmpSystemId,E.EmployeeCode
	                                , E.EmployeeName
	                                , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
                                    , REPLACE(CONVERT(VARCHAR(11), AD.WorkDate, 113), ' ', '-') WorkDate
									 , AD.DayStatus
									 ,CONVERT(varchar(15),CAST(AD.InTime AS TIME),100) InTimeShow
	                                , ARIN.DeviceID InDeviceID
									 ,CONVERT(varchar(15),CAST(AD.OutTime AS TIME),100) OutTimeShow
	                                , AROUT.DeviceID OutDeviceID
									 ,CONVERT(varchar(15),CAST(SD.InTime AS TIME),100) ShiftInTimeShow
                                    ,CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100) ShiftOutTimeShow
                                    , SD.ShiftDefinationDescription ShiftName
									, AD.CountedShortLeave ShortLeave ,lt.Code LeaveType
                                    ,ad.IsManualDayStatus,ad.IsManualInTime,ad.IsManualOutTime
									 , GVD.UserName GivenDesignation,e.PlantId,e.CompanyId,c.UserName Company

                                FROM dbo.EmployeeInformation E
							                INNER JOIN (select * from dbo.AttdnProcessData )AD ON E.SystemID = AD.EmpSystemID
							                LEFT JOIN (SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + WorkingDate + @"' BETWEEN FromDate AND ToDate
                                                                                        ) AS SFCG
																                ON AD.ShiftSystemID = SFCG.ShiftDefinationID
							                LEFT JOIN dbo.ShiftDefination SD ON AD.ShiftSystemID = SD.SystemID
							                LEFT JOIN dbo.AttdnRawData ARIN ON AD.InTimeRowID = ARIN.RowID
							                LEFT JOIN dbo.AttdnRawData AROUT ON AD.OutTimeRowID = AROUT.RowID
											left join LeaveType lt on lt.Id=ad.LTSystemID
                                            LEFT JOIN
												(
												SELECT LogDownLoadNum
												,min(ptime) ptime
												from AttdnRawData
												where pdate= '" + WorkingDate + @"'
												group by LogDownLoadNum
												) LIT on LIT.LogDownLoadNum=E.SystemId
                                            LEFT JOIN AttdnRawData ARD ON ARD.LogDownLoadNum=LIT.LogDownLoadNum  AND ARD.PTime=LIT.ptime
							                                       LEFT JOIN org.Unit U ON E.UnitID = U.Id
                                            LEFT JOIN org.Division Dv ON E.DivisionID = Dv.Id
                                            LEFT JOIN org.SubDivision SubDv ON E.SubdivisionID = SubDv.Id
                                            LEFT JOIN org.Department Dp ON E.DepartmentID = Dp.Id
                                            LEFT JOIN org.Section S ON E.SectionID = S.Id
                                            LEFT JOIN org.SubSection SB ON E.SubSectionID = SB.Id
                                            LEFT JOIN org.Line L ON E.LineID = L.Id
                                            LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupID = Dg.Id
                                            LEFT JOIN hkp.Designation D ON E.DesignationSystemID = D.Id
                                            LEFT JOIN hkp.Designation GVD ON E.GivenDesignationId = GVD.Id
                                            left join org.Company c on c.id=e.CompanyId
                                            LEFT JOIN
                                            (
                                            SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
											LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
											)EC ON EC.DesignationId=E.GivenDesignationId

			                    WHERE AD.WorkDate  = '" + WorkingDate + @"'
								AND E.EmployeeStatus='Active'
								and e.systemid='" + employeeId + @"'
                                ";
                var list = _sqlRepository.GetDataTable(sql);
                if (list.IsNull())
                    throw new CustomException("No Data Found");
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetJobCardInfo(string employeeId, string fromDate, string toDate)
        {
            try
            {
                var sql = @"SELECT A.EmployeeCode
                            	,A.EmployeeName
                            	,A.DOJ
                            	,A.GivenDesignation
                                ,A.LegalDesignation
                            	,A.Unit
                            	,A.Division
                            	,A.Department
                            	,A.Section
                            	,A.SubSection
                            	,REPLACE(CONVERT(VARCHAR(11), A.PDate, 113), ' ', '-') PDate
                            	,A.DayStatus
                            	,A.InTime
                                ,CONVERT(VARCHAR(5), A.ShiftInTime, 108) ShiftInTime
                            	,A.InDeviceID
                            	,A.OutTime
                            	,A.OutDeviceID
                            	,A.IsManual
                            	,A.OTHr
                            	,A.LvShortName
                            	,A.Code
                            	,A.LvDescrip
                            	,A.LeaveType
                                ,dti,dto
                                ,InTimeShow
                                ,OutTimeShow
                                ,ShiftTime = CASE
		                            WHEN ShiftChangeInTime IS NULL
			                            THEN ShiftInTime
		                            ELSE ShiftChangeInTime
		                            END
                                ,ShiftName
                               ,CONVERT(VARCHAR(5), A.ShiftOutTime, 108) ShiftOutTime
                            FROM (
                            	SELECT E.EmployeeCode
                            		,E.EmployeeName
                            		,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
                            		,D.UserName GivenDesignation
                            		,U.UserName Unit
                            		,Dv.UserName Division
                            		,Dp.UserName Department
                            		,S.UserName Section
                            		,SB.UserName SubSection
                            		,AR.WorkDate PDate
                            		,AR.DayStatus
                            		,CONVERT(VARCHAR(5), AR.InTime, 108) InTime
                                    ,CONVERT(varchar(15),CAST(AR.InTime AS TIME),100) InTimeShow
                                    --,ShiftInTime=case  when cs.InTime is null then sd.InTime else cs.InTime end
                                    ,CONVERT(VARCHAR(5), SD.InTime, 108) ShiftInTime
                            		,ARIN.DeviceID InDeviceID
                            		,CONVERT(VARCHAR(5), AR.OutTime, 108) OutTime
                                    ,CONVERT(varchar(15),CAST(AR.OutTime AS TIME),100) OutTimeShow
                            		,AROUT.DeviceID OutDeviceID
                            		,AR.IsManualInTime IsManual
                            		,AR.OTHr
                            		,LT.UserName LvShortName
                            		,LT.Description LvDescrip
                            		,LT.LeaveType
                                    ,LT.Code
                                    ,Isnull(LG.UserName,'') LegalDesignation
                                    ,AR.InTime dti,AR.OutTime dto
                                    , CONVERT(VARCHAR(5), SFCG.InTime, 108) ShiftChangeInTime
                                    , SD.ShiftDefinationName ShiftName
                                    ,CONVERT(VARCHAR(5), SD.OutTime, 108) ShiftOutTime
                            	
                                FROM dbo.EmployeeInformation E
                            	INNER JOIN dbo.AttdnProcessData AR ON E.SystemID = AR.EmpSystemID
                                --LEFT JOIN (SELECT * FROM dbo.ShiftTimeChgMaster) AS SFCG
                                LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + fromDate + @"' BETWEEN FromDate AND ToDate) AS SFCG

                                                                               
																                ON AR.ShiftSystemID = SFCG.ShiftDefinationID

                            	LEFT JOIN dbo.AttdnRawData ARIN ON AR.InTimeRowID = ARIN.RowID
                            	LEFT JOIN dbo.AttdnRawData AROUT ON AR.OutTimeRowID = AROUT.RowID
                            	LEFT JOIN dbo.LeaveType LT ON AR.LTSystemID = LT.Id
                            	LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            	LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            	LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            	LEFT JOIN ORG.Section S ON E.SectionID = S.Id
                            	LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                                LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
	                            left join EmpDateWiseShiftAssign es on es.EmpSystemID=E.SystemId
                                AND AR.WorkDate=ES.WorkDate
                                left join (
					            SELECT  m.ShiftDefinationID,c.ShiftDate,m.InTime,m.SystemID  FROM [ShiftTimeChgMaster] m
					            left join [ShiftTimeChgChild] c on m.SystemID=c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID=es.ShiftSystemID and cs.ShiftDate=ar.WorkDate

								left join [ShiftDefination] sd on sd.SystemID=es.ShiftSystemID
                            	LEFT JOIN HKP.Designation D ON E.GivenDesignationId = D.Id
                            	WHERE E.SystemID IN (" + employeeId + @")
                            		AND AR.WorkDate BETWEEN '" + fromDate + @"'
                            			AND '" + toDate + @"' AND E.EmployeeStatus='Active'
                            	) A
                            GROUP BY A.EmployeeCode
                            	,A.EmployeeName
                            	,A.DOJ
                            	,A.GivenDesignation
                                ,A.LegalDesignation
                            	,A.Unit
                            	,A.Division
                            	,A.Department
                            	,A.Section
                            	,A.SubSection
                            	,PDate
                            	,A.DayStatus
                            	,A.InTime
                                ,A.ShiftInTime
                            	,A.InDeviceID
                            	,A.OutTime
                            	,A.OutDeviceID
                            	,A.IsManual
                            	,A.OTHr
                            	,A.LvShortName
                            	,A.LvDescrip
                            	,A.LeaveType
                                ,A.Code
                                ,dti,dto
                                ,InTimeShow
                                ,OutTimeShow
                                ,ShiftChangeInTime
                                ,ShiftName
                                ,A.ShiftOutTime
                                ";
                var list = _sqlRepository.GetDataTable(sql);
                if (list.IsNull())
                    throw new CustomException("No Data Found");
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetEmpdata(string companyGroupId, string companyId, string plantId)
        {
            try
            {
                var sql = @"  SELECT  
  E.EmployeeCode,CM.Image CompanyLogo ,E.NationalID							
							 , E.CardNumber, REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ, REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB
                             ,A.[Name] LocalCompanyName, B.[Name] LocalDesignationName,C.[Name] LocalDepartmentName,N.Name NameLabel
                             ,DN.Name DesignationLabel,DPN.Name DepartmentLabel,LN.Name LineLabel,LET.Name EmploymentTypeLabel, ID.Name IDNoLabel,  PT.Name EmploymentTypeName, DJ.Name DOJLabel, ET.Name EmergencyTellNoLabel, BGP.Name BloodGroupLabel
                             ,E.EmployeeNameLocal,LL.UtilityName,NID.Name NIDLabel, E.ParmanentAddress1Local, (PML.Name+' '+LA.Name) ParmanentAddress,LMB.Name MobileNoLabel
                             ,E.FatherNameLocal, E.MotherNameLocal,E.PresentAddress1Local,E.SpouseNameLocal,LS.Name LocalSectionLabel,SEC.Name SectionName
                             ,LFN.Name FatherNameLabel,LMN.Name MotherNameLabel,LSN.Name SpouseNameLabel,LPL.Name PresentAddressLabel,LPRL.Name ProbationerName ,LPCL.Name CardNoLabel,SC.Name StaffCategoryLabel,LDOB.Name DOBLabel,E.IsConfirmed
                             ,dcd.Name DataCollectionDateLabel, Age.Name AgeLabel,l.UserName Line, e.CellPhnNo,pml.Name EmloymentType,E.TelePhnNo,e.GenderID,Grade.Name Grade,div.Name Division,Division.Name DivisionLabel
                             ,S.Name Salary,E.Height ,e.Weight ,QL.Name Qualificationlabel,EQ.Name Qualification,Nc.Name NoOfChildrenlbl, ex.Name  ExperianceLabel,ms.Name Maritalstslabel,Ns.Name NationalityLabel,RG.Name Religionlbl,Ba.Name Bankacclabel, Nn.Name Nomineelabl
                             ,ht.Name Heightlbl,W.Name weightlabel,DATEDIFF(year, E.DOB,GetDate()) Age,E.TotalSalary,CV.Name MaritalSts ,E.NoOfChildren,CN.Name Nationality,Rl.Name Religion,E.BankAccNo,NIN.LocalName NomineeNameLocal,ELL.LocalName LandOwnerName,LO.Name LandOwnerNameLabel
                             ,GD.Name GenderLabel,Noc.Name NumberOfChildLabel, RR.Name  RosterRelayLabel,PaL.Name  PayAbleLeavelabel,WT.Name WorkingTimelabel,BT.Name  BreakTimelabel,WLD.Name WeeklyLeaveDaysLabel,BG.UserName BloodGroup--,(CONVERT(VARCHAR(5), SD.InTime, 108)+'-'+ CONVERT(VARCHAR(5), SD.outtime, 108) ) workingTime
							 --,(CONVERT(VARCHAR(5), SD.BreakStratTime, 108)+'-'+ CONVERT(VARCHAR(5), SD.BreakEndTime, 108) ) BreakTime
							 , cmt.Name Commentlabel
							 --,WD.WeekOff Weakdays
							 ,stc.Name StaffCateLabel
                              FROM EmployeeInformation E
                              LEFT JOIN ORG.Company CM ON CM.Id = E.CompanyId
                              LEFT JOIN MST.AddressMaster AM ON AM.Id = CM.AddressMasterId
                              LEFT JOIN HKP.BloodGroup BG ON BG.Id = E.BloodGroupID
                              LEFT JOIN HKP.Designation D ON D.Id = E.GivenDesignationId
                              LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = e.GivenDesignationId
                              LEFT JOIN HKP.EmployeeCategory EC ON EC.Id = DM.EmployeeCategoryId
                              LEFT JOIN ORG.Line L ON L.Id=E.LineId
							  LEFT JOIN [SCS].[PlantSetting] P ON P.PlantId=E.PlantId
                              LEFT JOIN ORG.Department DP ON DP.Id=E.DepartmentId
							  LEFT JOIN ORG.Plant PL ON PL.Id=E.PlantId
                           -- LEFT JOIN (SELECT FixSystemID,EmpSystemId,MAX(EffectiveDate) M FROM  EmployeeShiftAssign
							--  WHERE EffectiveDate<=GETDATE()
							--  GROUP BY FixSystemID,EmpSystemId
							--  ) ESA ON ESA.EmpSystemId = E.SystemId
        --                     LEFT JOIN
							 -- (SELECT EmpSystemId,MAX(EffectiveDate) M,
							 --WeekOff= CASE AlignWithCC WHEN 1 THEN h.DefaultWeekOff
							 --  ELSE FstOffDay END FROM  EmployeeWeekOffByDay w
							 --  left join EmployeeInformation e ON e.SystemId=w.EmpSystemID
							 -- left join PlantWiseHRMSSetting h ON h.PlantID=e.PlantId
							 -- WHERE EffectiveDate<=GETDATE()
							 -- GROUP BY h.DefaultWeekOff,EmpSystemId,AlignWithCC,FstOffDay
							 -- ) WD ON WD.EmpSystemID =E.SystemId
						--	  LEFT JOIN ShiftDefination SD ON SD.SystemID = ESA.FixSystemID
							  LEFT JOIN EmployeeNomineeInfo NIN ON NIN.EmpSystemId = E.SystemId
							  LEFT JOIN EmployeeLandLordInfo ELL ON ELL.EmpSystemId = E.SystemId
							  LEFT JOIN HKP.LocalLanguage  Rl ON Rl.LanguageId = PL.LanguageId and Rl.ReligionId=E.ReligionID
                              LEFT JOIN HKP.LocalLanguage  CN ON CN.LanguageId = PL.LanguageId and CN.CountryId=E.CitizenID
							  LEFT JOIN HKP.LocalLanguage  CV ON CV.LanguageId = PL.LanguageId and CV.CivilStatusId=E.CivilStatusID
                              LEFT JOIN (Select TOP(1)* from  EmpAcademicQualificationInformation) EQI ON EQI.EmpSystemID =E.SystemId
							  LEFT JOIN HKP.LocalLanguage A ON A.CompanyId=E.CompanyId AND A.LanguageId=PL.LanguageId
                              LEFT JOIN HKP.LocalLanguage LL ON LL.CompanyId=E.CompanyId AND LL.LanguageId=PL.LanguageId
                			  LEFT JOIN HKP.LocalLanguage B ON B.LegalDesignationId=E.LegalDesignationId AND PL.LanguageId=B.LanguageId
							  LEFT JOIN HKP.LocalLanguage C ON C.DepartmentId =E.DepartmentId AND PL.LanguageId=C.LanguageId
                              LEFT JOIN HKP.LocalLanguage SEC ON SEC.SectionId = E.SectionId AND PL.LanguageId = SEC.LanguageId
                              LEFT JOIN HKP.LocalLanguage div ON div.DivisionId = E.DivisionId AND PL.LanguageId = div.LanguageId
                               LEFT JOIN HKP.LocalLanguage EQ ON EQ.QualificationLevelId = EQI.EductLevelSystemID AND PL.LanguageId = EQ.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Name') N ON N.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Designation') DN ON DN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Department') DPN ON DPN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Line') LN ON LN.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmploymentType') LET ON LET.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='IDNo') ID ON ID.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Parmanent') PT ON PT.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='DOJ') DJ ON DJ.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmergencyTellNo') ET ON ET.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='BloodGroup') BGP ON BGP.LanguageId=PL.LanguageId
					          LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='NIDNo') NID ON BGP.LanguageId=PL.LanguageId
	                          LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Permanent') PML ON PML.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Address') LA ON LA.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='MobileNo') LMB ON LMB.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Section') LS ON LS.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='FatherName') LFN ON LFN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='MotherName') LMN ON LMN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='SpouseName') LSN ON LSN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='PresentAddress') LPL ON LPL.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Probationer') LPRL ON LPRL.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='CardNo') LPCL ON LPCL.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='StaffCategory') SC ON SC.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='DOB') LDOB ON LDOB.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='DataCollectionDate') DCD ON DCD.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Age') Age ON Age.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Grade') Grade ON Grade.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Division') Division ON Division.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Salary') S ON S.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='QualificationLabelInfo') QL ON QL.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Experience') ex ON ex.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='MaterialStatus') ms ON ms.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Nationality') Ns ON Ns.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Religion') RG ON RG.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='BankAccountNo') Ba ON Ba.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='NomineeInfo') Nn ON Nn.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='NoOfChildren') Nc ON Nc.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Height') ht ON ht.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Weight') W ON W.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='LanOwnerInfo') LO ON LO.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Gender') GD ON GD.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='NumberOfChild') Noc ON Noc.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='RosterAndRelay') RR ON RR.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='PayableLeave') PaL ON PaL.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='WorkingTime') WT ON WT.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='BreakTime') BT ON BT.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='WeeklyLeaveDays') WLD ON WLD.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='EmployeeShiftAssign') SA ON SA.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='ShiftDefination') SN ON SN.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Comment') cmt ON cmt.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='StaffCategory') stc ON stc.LanguageId = PL.LanguageId
                                 
                              WHERE E.EmployeeStatus ='Active' and E.CompanyId='" + companyId + @"' and e.PlantId='" + plantId + @"'";

                var list = _sqlRepository.GetDataTable(sql);
                if (list.IsNull())
                    throw new CustomException("No Data Found");
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string cnDgt(string input)
        {
            return input.Replace('0', '০')
                     .Replace('1', '১')
                     .Replace('2', '২')
                     .Replace('3', '৩')
                     .Replace('4', '৪')
                     .Replace('5', '৫')
                     .Replace('6', '৬')
                     .Replace('7', '৭')
                     .Replace('8', '৮')
                     .Replace('9', '৯');
        }

        public string cnDgt(string input, string lng)
        {
            if (lng == "Bengali")
            {
                return input.Replace('0', '০')
                    .Replace('1', '১')
                    .Replace('2', '২')
                    .Replace('3', '৩')
                    .Replace('4', '৪')
                    .Replace('5', '৫')
                    .Replace('6', '৬')
                    .Replace('7', '৭')
                    .Replace('8', '৮')
                    .Replace('9', '৯');
            }
            else if (lng == "Hindi")
            {
                return input.Replace('0', '०')
                    .Replace('1', '१')
                    .Replace('2', '२')
                    .Replace('3', '३')
                    .Replace('4', '४')
                    .Replace('5', '५')
                    .Replace('6', '६')
                    .Replace('7', '७')
                    .Replace('8', '८')
                    .Replace('9', '९');
            }
            else if (lng == "English")
            {
                return input.Replace('0', '0')
                    .Replace('1', '1')
                    .Replace('2', '2')
                    .Replace('3', '3')
                    .Replace('4', '4')
                    .Replace('5', '5')
                    .Replace('6', '6')
                    .Replace('7', '7')
                    .Replace('8', '8')
                    .Replace('9', '9');
            }
            return input;
        }

        public string ChangeMonth(string input, string lng)
        {
            if (lng == "Bengali")
            {
                return input
                     //.Replace("Jan", "জানুয়ারি")
                     //.Replace("Feb", "ফেব্রুয়ারি")
                     //.Replace("Mar", "মার্চ")
                     //.Replace("Apr", "এপ্রিল")
                     //.Replace("May", "মে")
                     //.Replace("Jun", "জুন")
                     //.Replace("Jul", "জুলাই")
                     //.Replace("Aug", "আগস্ট")
                     //.Replace("Sep", "সেপ্টেম্বর")
                     //.Replace("Oct", "অক্টোবর")
                     //.Replace("Nov", "নভেম্বর")
                     //.Replace("Dec", "ডিসেম্বর");
                     .Replace("Jan", "জানু")
                    .Replace("Feb", "ফেব্রু")
                    .Replace("Mar", "মার্চ")
                    .Replace("Apr", "এপ্রিল")
                    .Replace("May", "মে")
                    .Replace("Jun", "জুন")
                    .Replace("Jul", "জুলাই")
                    .Replace("Aug", "আগস্ট")
                    .Replace("Sep", "সেপ্টে")
                    .Replace("Oct", "অক্টো")
                    .Replace("Nov", "নভে")
                    .Replace("Dec", "ডিসে");
            }
            else if (lng == "Hindi")
            {
                return input
                    .Replace("Jan", "जनवरी")
                    .Replace("Feb", "फरवरी")
                    .Replace("Mar", "मार्च")
                    .Replace("Apr", "अप्रैल")
                    .Replace("May", "मई")
                    .Replace("Jun", "जून")
                    .Replace("Jul", "जुलाई")
                    .Replace("Aug", "अगस्त")
                    .Replace("Sep", "सितम्बर")
                    .Replace("Oct", "अक्तूबर")
                    .Replace("Nov", "नवम्बर")
                    .Replace("Dec", "दिसम्बर");
            }
            return input;
        }

        public string GetFormatedDate(string date, string lng)
        {
            var formateDate = string.Empty;
            var day = cnDgt(date.Substring(0, 2), lng);
            var mon = ChangeMonth(date.Substring(3, 3), lng);
            var year = cnDgt(date.Substring(7, 4), lng);
            return formateDate = day + "-" + mon + "-" + year;
        }

        private string GetDuration(string dti, string dto, string intime, string outtime)
        {
            string res = string.Empty;
            try
            {
                // string vDate = Convert.ToDateTime(sDate).ToString("dd-MMM-yyyy");

                if (string.IsNullOrEmpty(intime) == false && string.IsNullOrEmpty(outtime) == false)
                {
                    string vintime = Convert.ToDateTime(intime).ToString("HH:mm:ss");
                    string vouttime = Convert.ToDateTime(outtime).ToString("HH:mm:ss");
                    var x = (Convert.ToDateTime(dto) - (Convert.ToDateTime(dti)));
                    res = x.ToString().Substring(0, 5);
                    //res = (Convert.ToDateTime(dto)-(Convert.ToDateTime(dti))).ToString().Substring(0, 5);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> GetEmployeeById(string employeeId, string employeementType)
        {
            try
            {
                string _sql = @"SELECT CM.Image CompanyLogo,CM.UserName CompanyName,AM.Address1 CompanyAddress,E.EmployeeName,E.EmpPicPath EmployeePic,E.EmployeeCode, Convert(varchar, E.DOJ, 105) DOJ,BG.UserName BloodGroup
                              , E.NationalID,E.EmploymentType,D.UserName DesignationName, dm.EmployeeCategoryId,ec.UserName EmployeeCategory,L.UserName Line,E.EmpSignature CardHolderSignature,P.AuthorizedSignature
                              ,E.CellPhnNo MobileNo,E.ParmanentAddress1,DP.UserName Department,A.[Name] LocalCompanyName, B.[Name] LocalDesignationName,C.[Name] LocalDepartmentName,N.Name NameLabel
                              ,DN.Name DesignationLabel,DPN.Name DepartmentLabel,LN.Name LineLabel,LET.Name EmploymentTypeLabel, ID.Name IDNoLabel,  PT.Name EmploymentTypeName, DJ.Name DOJLabel, ET.Name EmergencyTellNoLabel, BGP.Name BloodGroupLabel
                              ,E.EmployeeNameLocal,LL.UtilityName,NID.Name NIDLabel, E.ParmanentAddress1Local, (PML.Name+' '+LA.Name) ParmanentAddress,LMB.Name MobileNoLabel,LD.Name LegalDesignationLocal
                              ,Convert(varchar, DATEADD(year, 5, E.IssueDate),105) AS Validity,E.EmrCntPer1CellNo, ISNULL(LNN.Name,SSL.Name) [LineNo], ISNULL(SL.Name,S.UserName) Section
                              ,LDG.UserName LegalDesignation,AM.Phone, CMN.[Name] CompanyMobileNoLabel FROM EmployeeInformation E
                              LEFT JOIN ORG.Company CM ON CM.Id = E.CompanyId
                              LEFT JOIN MST.AddressMaster AM ON AM.Id = CM.AddressMasterId
                              LEFT JOIN HKP.BloodGroup BG ON BG.Id = E.BloodGroupID
                              LEFT JOIN HKP.Designation D ON D.Id = E.GivenDesignationId
                              LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = e.GivenDesignationId
                              LEFT JOIN HKP.EmployeeCategory EC ON EC.Id = DM.EmployeeCategoryId
                              LEFT JOIN ORG.Line L ON L.Id=E.LineId
							  LEFT JOIN [SCS].[PlantSetting] P ON P.PlantId=E.PlantId
                              LEFT JOIN ORG.Department DP ON DP.Id=E.DepartmentId
							  LEFT JOIN ORG.Plant PL ON PL.Id=E.PlantId
                              LEFT JOIN ORG.Section S on S.Id= E.SectionId
                              LEFT JOIN ORG.SubSection SS on SS.Id= E.SubSectionId
                              LEFT JOIN HKP.LegalDesignation LDG ON LDG.Id=E.LegalDesignationId
							  LEFT JOIN HKP.LocalLanguage A ON A.CompanyId=E.CompanyId AND A.LanguageId=PL.LanguageId
                              LEFT JOIN HKP.LocalLanguage LL ON LL.CompanyId=E.CompanyId AND LL.LanguageId=PL.LanguageId
							  LEFT JOIN HKP.LocalLanguage B ON B.LegalDesignationId=E.LegalDesignationId AND PL.LanguageId=B.LanguageId
							  --LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=E.GivenDesignationId AND PL.LanguageId=B.LanguageId
							  LEFT JOIN HKP.LocalLanguage C ON C.DepartmentId =E.DepartmentId AND PL.LanguageId=C.LanguageId
                              LEFT JOIN HKP.LocalLanguage LD ON LD.LegalDesignationId=E.LegalDesignationId AND PL.LanguageId=LD.LanguageId
                              LEFT JOIN HKP.LocalLanguage LNN ON LNN.LineId=E.LineId AND PL.LanguageId=LNN.LanguageId
                              LEFT JOIN HKP.LocalLanguage SL ON Sl.SectionId=E.SectionId AND PL.LanguageId=SL.LanguageId
                              LEFT JOIN HKP.LocalLanguage SSL ON SSL.SubSectionId=E.SubSectionId AND PL.LanguageId=SSL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Name') N ON N.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Designation') DN ON DN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Department') DPN ON DPN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Line') LN ON LN.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmploymentType') LET ON LET.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='IDNo') ID ON ID.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='" + employeementType + @"') PT ON PT.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='DOJ') DJ ON DJ.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmergencyTelNo') ET ON ET.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='BloodGroup') BGP ON BGP.LanguageId=PL.LanguageId
					          LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='NIDNo') NID ON BGP.LanguageId=PL.LanguageId
	                          LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Permanent') PML ON PML.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Address') LA ON LA.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='MobileNo') LMB ON LMB.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='CompanyMobileNo') CMN ON CMN.LanguageId=PL.LanguageId
                              WHERE E.SystemID ='" + employeeId + "'";
                return _sqlRepository.GetData(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Dictionary<string, object> GetBudgetInfo(string Id)
        {
            return _manpowerBudgetService.GetManpowerBudgetById(Id);
        }

        private static string GetValue(Dictionary<string, object> dic, string key)
        {
            string value = null;
            if (dic.ContainsKey(key))
            {
                value = dic[key].ToString();
            }

            return value;
        }

        public string GetDesignationGroup(string designationId)
        {
            var _sql = "SELECT DesignationGroupId FROM mst.DesignationMaster WHERE DesignationId='" + designationId + "'";
            return _designationMasterRepository.SqlQuery<string>(_sql).FirstOrDefault();
        }

        private void InitBudgetCode(Dictionary<string, object> dic, ref EmployeeInformation bc)
        {
            bc.PositionId = GetValue(dic, "PositionId");
            bc.DepartmentID = GetValue(dic, "DepartmentId");
            bc.DivisionID = GetValue(dic, "DivisionId");
            bc.EmployeeGroupSystemID = GetValue(dic, "EmployeeGroupId");
            bc.LineID = GetValue(dic, "LineId");
            if (string.IsNullOrEmpty(bc.LineID))
            {
                bc.LineID = null;
            }
            bc.SectionID = GetValue(dic, "SectionId");
            bc.SubdivisionID = GetValue(dic, "SubDivisionId");
            bc.SubSectionID = GetValue(dic, "SubSectionId");
            bc.UnitID = GetValue(dic, "UnitId");
            bc.DesignationSystemID = GetValue(dic, "DesignationId");
            bc.EmploymentType = GetValue(dic, "EmploymentType");
            bc.DesignationGroupID = GetDesignationGroup(bc.DesignationSystemID);

        }

        public void UpdateBudgetCode(EmployeeInformation entity)
        {
            try
            {
                var dblist = Find(entity.SystemId);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                EmployeeBudgetCodeHistory employeeBudgetCodeHistory = new EmployeeBudgetCodeHistory
                {
                    Id = GetAutoNumber(nameof(EmployeeBudgetCodeHistory), PKGeneratorEnum.Auto, null, DateTime.Now),
                    EmpSystemId = entity.SystemId,
                    BudgetId = dblist.BudgetCode,
                    GivenDesignationId = dblist.GivenDesignationId,
                    LegalDesignationId = dblist.LegalDesignationId,
                    AddedBy = identity.Name,
                    AddedDate = DateTime.Now,
                    AddedFromIP = identity.IPAddress
                };
                _employeeBudgetCodeHistoryService.Insert(employeeBudgetCodeHistory);

                dblist.BudgetCode = entity.BudgetCode;
                if (!string.IsNullOrEmpty(entity.GivenDesignationId))
                {
                    dblist.GivenDesignationId = entity.GivenDesignationId;
                }
                if (!string.IsNullOrEmpty(entity.LegalDesignationId))
                {
                    dblist.LegalDesignationId = entity.LegalDesignationId;
                }
                //dblist.LegalDesignationId = entity.LegalDesignationId;
                Dictionary<string, object> dic;
                dic = GetBudgetInfo(entity.BudgetCode);

                InitBudgetCode(dic, ref dblist);

                dblist.DateUpdated = DateTime.Now;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateGivenDesignation()
        {
            string sql;

            try
            {
                sql = @"Update e set e.GivenDesignationId=tmptbl.DesignationId
                        FROM EmployeeInformation e
                        INNER JOIN 
                        (
                        Select employeeinformation.SystemId,employeeinformation.GivenDesignationId,EmployeeInformation.LegalDesignationId,dm.DesignationId
                        from EmployeeInformation
                        Inner Join mst.DesignationMasterLegalDesignation LegalDesigTag on LegalDesigTag.LegalDesignationId=EmployeeInformation.LegalDesignationId 
                        Inner Join mst.DesignationMaster DM on LegalDesigTag.DesignationMasterId=DM.Id
                        ) TmpTbl
                        on e.SystemId=TmpTbl.SystemId";
                ExecuteSqlCommand(sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSectionEmployeeList(string plantId, string companyId, string SectionId)
        {
            try
            {
                string CmdText = @"SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC
                                        ,DeM.UserName DesignationGroup
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN MST.DesignationMasterLegalDesignation DML ON DML.LegalDesignationId = EMP.LegalDesignationId
										Left join  MST.DesignationMaster DeM on DeM.Id = DML.DesignationMasterId

                                        WHERE emp.PlantID='" + plantId + @"'  and EMP.CompanyId='" + companyId + @"' and EMP.EmployeeStatus='Active' and EMP.SectionId='" + SectionId + @"' 
                                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }



        public IWorkbook AttndReport(string companyGroupId, string employeeId, string plantId)
        {
            throw new NotImplementedException();
        }

        public void Insert(List<XLUploadDetail> entities)
        {
            try
            {
                foreach (var item in entities)
                {
                    var dbdata = Find(item.Id);
                    if (dbdata == null)
                    {
                        item.Id = GetAutoNumber(nameof(XLUploadDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
                        _xLUploadDetailService.Insert(item);
                    }
                    else
                    {
                        _xLUploadDetailService.Update(item);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void xgenerateReport(string CalanderYearId, string FromDate, string ToDate, string plantId, string empID, string reportType, string tempId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = "+ languageId + @";
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Lvr" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }
                //------
                //fileName = "Lvr" + plantId + tempId + ".xlsx";
                //strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                //File = fileName;
                //if (!System.IO.File.Exists(strPath))
                //{
                //    throw new CustomException("File <" + fileName + "> Not Found.");
                //}

                ///-----
                var Templatefile = GetFilePath(plantId, tempId, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "Lvr" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

                bool IsDefLan = false;

                var tokens = (fileName.Substring(("Lvr" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }

                //Creates a new instance for ExcelEngine
                ExcelEngine excelEngine = new ExcelEngine();

                //Loads or open an existing workbook through Open method of IWorkbooks
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(filepath);

                IWorksheet sheet = workbook.Worksheets[0];
                sheet.ShowColumn(0, true);
                DataTable dsBf, dsTransaction, dsBalance, dsHeader;

                //clsDataContext data = new clsDataContext();
                dsBf = loadBf(empID, CalanderYearId);
                dsTransaction = loadLeaveTransactions(empID, FromDate, ToDate);
                dsBalance = loadOpeningBalance(empID, CalanderYearId);
                dsHeader = GetEmployeeBasicInfoById(empID, plantId, "Permanent", langID, tempId);

                IRange range = sheet.UsedRange;
                IRange columnList = range.Rows[0]; //IRange columnList = range.Rows[5];
                int columnListRow = 1;
                int ColumnTemplateRow = 1;
                for (int i = 0; i < range.Rows.Length; i++)
                {

                    if (string.IsNullOrEmpty(range["A" + (i + 1)].Text))
                        continue;
                    if (range["A" + (i + 1)].Text.ToUpper() == "COLUMNLIST")
                    {
                        columnListRow = (i + 1);

                    }
                    if (range["A" + (i + 1)].Text.ToUpper() == "REFROW")
                    {
                        columnList = range.Rows[i];
                        ColumnTemplateRow = (i + 1);
                    }
                }

                string PrefixOpeningBalance = "OB";
                string PrefixCurrentTransaction = "LT";
                string PrefixClosingBalance = "CB";

                #region  EmployeeInformation    
                string columnName = "";
                for (int R = 0; R < columnListRow; R++)
                {
                    columnName = "";
                    IRange columnListEmp = range.Rows[R];
                    foreach (DataColumn item in dsHeader.Columns)
                    {
                        ////===== def lan 
                        if (IsDefLan == true)
                        {
                            columnName = GetBasicInfoInDefaultLng(item.ColumnName);

                        }
                        ///=====

                        for (int i = 0; i < range.Rows[R].Cells.Count(); i++)
                        {
                            if (string.IsNullOrEmpty(sheet[R + 1, i + 1].Text))
                                continue;

                            if (sheet[R + 1, i + 1].Text.ToUpper().Trim() == "{" + item.ColumnName.ToUpper() + "}")
                            {

                                //sheet[R + 1, i + 1].Text = dsHeader.Rows[0][item.ColumnName].ToString();
                                if (bplib.clsWebLib.IsNumeric(dsHeader.Rows[0][columnName].ToString()))
                                    sheet[R + 1, i + 1].Text = cnDgt(dsHeader.Rows[0][columnName].ToString(), tempId);
                                else if (bplib.clsWebLib.IsDateOK(dsHeader.Rows[0][columnName].ToString()))
                                    sheet[R + 1, i + 1].Text = GetFormatedDate(dsHeader.Rows[0][columnName].ToString(), tempId);
                                else
                                    sheet[R + 1, i + 1].Text = dsHeader.Rows[0][columnName].ToString();
                            }
                        }
                    }
                }

                #endregion
                int RefROW = ColumnTemplateRow;
                int ROW = ColumnTemplateRow + 1; int COL = 1;
                for (int T = 0; T < dsTransaction.Rows.Count; T++)
                {
                    sheet[ROW, 2].Number = (T + 1);

                    foreach (DataRow item in dsBalance.Rows)
                        item["CurrentTransaction"] = 0;

                    for (int CELL = 0; CELL < columnList.Cells.Count(); CELL++)
                    {
                        string cellValue = columnList.Cells[CELL].Text;
                        if (string.IsNullOrEmpty(cellValue))
                            continue;

                        if (cellValue.ToUpper() == "{BF}")
                        {
                            if (dsBf.Rows.Count > 0)
                            {
                                if (string.IsNullOrEmpty(dsBf.Rows[0]["BroughtForward"].ToString()))
                                {
                                    sheet[ROW, (CELL + 1)].Number = 0;
                                    sheet[ROW, (CELL + 1)].NumberFormat = "###0;";
                                }
                                else
                                {
                                    sheet[ROW, (CELL + 1)].Number = Convert.ToInt32(dsBf.Rows[0]["BroughtForward"].ToString());
                                    sheet[ROW, (CELL + 1)].NumberFormat = "###0;";
                                }
                            }
                        }

                        if (cellValue.ToUpper().Contains("DATE"))
                        {
                            if (dsTransaction.Columns.Contains(cellValue.Replace("{", "").Replace("}", "")))
                            {
                                sheet[ROW, (CELL + 1)].NumberFormat = "@";
                                sheet[ROW, (CELL + 1)].Text = GetFormatedDate(Convert.ToDateTime(dsTransaction.Rows[T][cellValue.Replace("{", "").Replace("}", "")].ToString()).ToString("dd-MMM-yyyy"), tempId);
                            }
                        }

                        for (int OB = 0; OB < dsBalance.Rows.Count; OB++)
                        {
                            string leaveTypeOB = "{" + PrefixOpeningBalance + dsBalance.Rows[OB]["LeaveCode"].ToString() + "}";
                            if (leaveTypeOB.ToUpper() == cellValue.ToUpper())
                            {
                                sheet[ROW, CELL + 1].Number = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString());
                            }
                        }

                        string leaveTypeCL = "{" + PrefixCurrentTransaction + dsTransaction.Rows[T]["Code"].ToString() + "}";
                        if (leaveTypeCL.ToUpper() == cellValue.ToUpper())
                        {
                            sheet[ROW, CELL + 1].Number = dbl(dsTransaction.Rows[T]["LeaveDays"].ToString());

                            dsBalance.DefaultView.RowFilter = "LeaveCode='" + dsTransaction.Rows[T]["Code"].ToString() + "'";
                            if (dsBalance.DefaultView.Count > 0)
                            {
                                dsBalance.DefaultView[0]["CurrentTransaction"] = dbl(dsTransaction.Rows[T]["LeaveDays"].ToString());
                                dsBalance.DefaultView.RowFilter = null;
                                dsBalance.AcceptChanges();
                            }
                        }

                        for (int OB = 0; OB < dsBalance.Rows.Count; OB++)
                        {
                            string leaveTypeOB = "{" + PrefixClosingBalance + dsBalance.Rows[OB]["LeaveCode"].ToString() + "}";
                            if (leaveTypeOB.ToUpper() == cellValue.ToUpper())
                            {
                                dsBalance.Rows[OB]["CurrentYearAllocation"] = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString())
                                - dbl(dsBalance.Rows[OB]["CurrentTransaction"].ToString());

                                sheet[ROW, CELL + 1].Number = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString());
                            }
                        }
                    }
                    ROW++;
                }

                sheet.DeleteRow(RefROW);
                sheet.HideColumn(1);

                workbook.SaveAs("File.xlsx", System.Web.HttpContext.Current.Response, ExcelDownloadType.Open);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IEnumerable<object> GetSuperVisor(string companyid, string plantid)
        {
            var sql = @"SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,DV.UserName Division,EC.UserName EmployeeCategory,EMP.EmployeeStatus
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN ORG.Division DV on DV.Id = EMP.DivisionId
                                        LEFT JOIN
									   (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									                ,dg.UserName GivenDesignationGroup
									                from ( SELECT dm.* FROM MST.DesignationMaster DM) DM
									                LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
									                ) EGDSGG on EGDSGG.DesignationId=EMP.GivenDesignationId 
								        LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=EGDSGG.EmployeeCategoryId
                                        WHERE EMP.CompanyId='" + companyid + @"' AND EMP.PlantId='" + plantid + @"' ORDER BY EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric";

            return _sqlRepository.GetDataCollection(sql);
        }

        private DataTable loadBf(string EmployeeId, string CalanderYearId)
        {
            try
            {
                var sql = @"SELECT EmployeeId, 'Earn' LeaveType,Convert (DECIMAL(10,0),ISNULL (Sum(BroughtForward),0)) BroughtForward,CurrentYearAllocation
                           from TRN.EmployeeLeaveSummary where LeaveTypeId in (
                            Select Id from LeaveType where LeaveType = 'Earn')
                                 and EmployeeId = '" + EmployeeId + "' and CalanderYearId='" + CalanderYearId + @"'
                                  group by EmployeeId,CurrentYearAllocation";
                var list = _sqlRepository.GetDataTable(sql);

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable loadLeaveTransactions(string EmpSystemID, string FromDate, String ToDate)
        {
            try
            {
                var sql = @"Select LT.EmpSystemID,L.Code, format(LT.FromDate,'dd-MMM-yyyy') FromDate,format(LT.ToDate,'dd-MMM-yyyy') ToDate,LT.LeaveDays
             ,FORMAT(LT.AppliedDate,'dd-MMM-yyy')AppliedDate,format(LT.ApprovedDate,'dd-MMM-yyyy')ApprovedDate,LT.CancelationReason
                 from  LeaveTransaction  LT
             	INNER JOIN(SELECT * FROM AttdnProcessData WHERE  DayStatus='LV' and WorkDate between '" + FromDate + @"' and '" + ToDate + @"') 
				APD ON APD.EmpSystemID=LT.EmpSystemID AND APD.WorkDate=LT.FromDate
                LEFT OUTER JOIN LeaveType AS L ON L.Id=LT.LTSystemID				
                where LT.EmpSystemID='" + EmpSystemID + @"'  and LT.IsApproved=1
                order by LT.FromDate ,LT.ToDate ";
                var list = _sqlRepository.GetDataTable(sql);

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable loadOpeningBalance(string EmployeeId, string CalanderYearId)
        {
            try
            {
                var sql = @"Select LS.EmployeeId,LS.LeaveTypeId LeaveId,LT.Code LeaveCode, LT.UserName LeaveName,LS.CurrentYearAllocation,0 AS CurrentTransaction
                           from TRN.EmployeeLeaveSummary LS
                           Left Outer Join LeaveType LT on LS.LeaveTypeId=LT.Id
                           where LS.EmployeeId = '" + EmployeeId + "' and LS.CalanderYearId='" + CalanderYearId + "'";
                var list = _sqlRepository.GetDataTable(sql);

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable LeaveSummaryForServiceBook(string EmployeeId, string CalanderYearId)
        {
            try
            {
                var sql = @"select Lvtran.ID,	Lvtran.EmpSystemID,	Lvtran.YearNo,	Lvtran.FromDate,	Lvtran.ToDate,	Lvtran.Allocation,	Lvtran.Availed,	Lvtran.CumulativeAvailed,	Lvtran.Balance, Lvtran.CalanderYearId, Enc.EncashmentDayNo, Enc.Rate, format(Enc.EncashmentDate,'dd-MMM-yyyy') EncashmentDate from (
-----------------------------------------------------------------------------------------------------------------------------------------------
                            select X.ID,x.EmpSystemID,x.CalanderYearId,x.YearNo,x.FromDate,x.ToDate,x.Allocation,x.Availed,sum(y.Availed) CumulativeAvailed,ISNULL(x.Allocation,0)-sum(y.Availed) Balance
                              from
                            (
                            SELECT
                            LT.FromDate ID,LT.EmpSystemID,L.Code,LS.YearNo
                             , format(LT.FromDate,'dd-MMM-yyyy') FromDate,format(LT.ToDate,'dd-MMM-yyyy') ToDate,LT.LeaveDays,LT.AppliedDate,LT.ApprovedDate
                             ,ISNULL(LS.BroughtForward,0)+ISNULL(LS.CurrentYearAllocation,0) Allocation,LT.LeaveDays Availed,LS.CalanderYearId 
                            from LeaveTransaction LT
                            LEFT JOIN LeaveType AS L ON L.Id=LT.LTSystemID 
                            LEFT JOIN (SELECT S.BroughtForward,S.CurrentYearAllocation,S.CalanderYearId,S.LeaveTypeId,S.EmployeeId,YC.YearNo
                            FROM TRN.EmployeeLeaveSummary S
                            LEFT JOIN YearlyCalendar AS yc ON yc.Id = S.CalanderYearId
                            ) LS ON LS.EmployeeId=LT.EmpSystemID AND LT.LTSystemID=LS.LeaveTypeId AND LS.YearNo=DATEPART(YEAR,lt.FromDate)

                            where LT.EmpSystemID='" + EmployeeId + @"' and LT.IsApproved=1 AND L.LeaveType='Earn' AND LS.YearNo='" + CalanderYearId + @"'

                            )x
                            inner join (
                            SELECT
                            LT.FromDate ID,LT.EmpSystemID,L.Code,LS.YearNo , format(LT.FromDate,'dd-MMM-yyyy') FromDate,format(LT.ToDate,'dd-MMM-yyyy') ToDate,LT.LeaveDays,LT.AppliedDate,LT.ApprovedDate
                            ,ISNULL(LS.BroughtForward,0)+ISNULL(LS.CurrentYearAllocation,0) Allocation,LT.LeaveDays Availed,LS.CalanderYearId
                            from LeaveTransaction LT
                            LEFT JOIN LeaveType AS L ON L.Id=LT.LTSystemID 
                            LEFT JOIN (SELECT S.BroughtForward,S.CurrentYearAllocation,S.CalanderYearId,S.LeaveTypeId,S.EmployeeId,YC.YearNo
                            FROM TRN.EmployeeLeaveSummary S
                            LEFT JOIN YearlyCalendar AS yc ON yc.Id = S.CalanderYearId
                            ) LS ON LS.EmployeeId=LT.EmpSystemID AND LT.LTSystemID=LS.LeaveTypeId AND LS.YearNo=DATEPART(YEAR,lt.FromDate)

                            where LT.EmpSystemID='" + EmployeeId + @"' and LT.IsApproved=1 AND L.LeaveType='Earn' AND LS.YearNo='" + CalanderYearId + @"'

                            )y on x.id>=y.id
                            WHERE X.YearNo='" + CalanderYearId + @"'
                            group by x.id,x.EmpSystemID,x.CalanderYearId ,x.Code,x.Allocation,x.Availed,x.yearno,x.FromDate,x.ToDate
                            --order by x.id

                            ) Lvtran
                            full join  

                            (select EmpSystemID ,YearlyCalendarId, EncashmentDate,Days EncashmentDayNo,Rate  from LeaveEncashmentTransaction where EmpSystemId='" + EmployeeId + @"') enc on enc.EmpSystemId=Lvtran.EmpSystemID ";


                var xsql = @"
                            select X.ID,x.EmpSystemID,x.YearNo,x.FromDate,x.ToDate,x.Allocation,x.Availed,sum(y.Availed) CumulativeAvailed,ISNULL(x.Allocation,0)-sum(y.Availed) Balance
                              from
                            (
                            SELECT
                            LT.FromDate ID,LT.EmpSystemID,L.Code,LS.YearNo
                             , format(LT.FromDate,'dd-MMM-yyyy') FromDate,format(LT.ToDate,'dd-MMM-yyyy') ToDate,LT.LeaveDays,LT.AppliedDate,LT.ApprovedDate
                             ,ISNULL(LS.BroughtForward,0)+ISNULL(LS.CurrentYearAllocation,0) Allocation,LT.LeaveDays Availed
                            from LeaveTransaction LT
                            LEFT JOIN LeaveType AS L ON L.Id=LT.LTSystemID 
                            LEFT JOIN (SELECT S.BroughtForward,S.CurrentYearAllocation,S.CalanderYearId,S.LeaveTypeId,S.EmployeeId,YC.YearNo
                            FROM TRN.EmployeeLeaveSummary S
                            LEFT JOIN YearlyCalendar AS yc ON yc.Id = S.CalanderYearId
                            ) LS ON LS.EmployeeId=LT.EmpSystemID AND LT.LTSystemID=LS.LeaveTypeId AND LS.YearNo=DATEPART(YEAR,lt.FromDate)

                            where LT.EmpSystemID='" + EmployeeId + @"' and LT.IsApproved=1 AND L.LeaveType='Earn' AND LS.YearNo='" + CalanderYearId + @"'

                            )x
                            inner join (
                            SELECT
                            LT.FromDate ID,LT.EmpSystemID,L.Code,LS.YearNo , format(LT.FromDate,'dd-MMM-yyyy') FromDate,format(LT.ToDate,'dd-MMM-yyyy') ToDate,LT.LeaveDays,LT.AppliedDate,LT.ApprovedDate
                            ,ISNULL(LS.BroughtForward,0)+ISNULL(LS.CurrentYearAllocation,0) Allocation,LT.LeaveDays Availed
                            from LeaveTransaction LT
                            LEFT JOIN LeaveType AS L ON L.Id=LT.LTSystemID 
                            LEFT JOIN (SELECT S.BroughtForward,S.CurrentYearAllocation,S.CalanderYearId,S.LeaveTypeId,S.EmployeeId,YC.YearNo
                            FROM TRN.EmployeeLeaveSummary S
                            LEFT JOIN YearlyCalendar AS yc ON yc.Id = S.CalanderYearId
                            ) LS ON LS.EmployeeId=LT.EmpSystemID AND LT.LTSystemID=LS.LeaveTypeId AND LS.YearNo=DATEPART(YEAR,lt.FromDate)

                            where LT.EmpSystemID='" + EmployeeId + @"' and LT.IsApproved=1 AND L.LeaveType='Earn' AND LS.YearNo='" + CalanderYearId + @"'

                            )y on x.id>=y.id
                            WHERE X.YearNo='" + CalanderYearId + @"'
                            group by x.id,x.EmpSystemID,x.Code,x.Allocation,x.Availed,x.yearno,x.FromDate,x.ToDate
                            order by x.id";
                var list = _sqlRepository.GetDataTable(sql);

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DataTable loadHeader(string EmployeeId)
        {
            try
            {
                var sql = @"select e.EmployeeCode, e.EmployeeName, e.DOJ,l.UserName as Designation
                             from EmployeeInformation AS e
                              left join HKP.LegalDesignation as l on l.Id=e.LegalDesignationId 
                                 where e.SystemId = '" + EmployeeId + "'";
                var list = _sqlRepository.GetDataTable(sql);
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //#endregion Operation

        public static double dbl(string d)
        {
            return Convert.ToDouble(GetNumericData(d));

        }

        public static string GetNumericData(string strNumber)
        {
            strNumber = strNumber.Replace(",", "");
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Trim() == "")
            { return "0"; }
            else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out double d) == true)
            {
                return strNumber;
            }
            else
            {
                return "0";
            }
        }// end function

        public IEnumerable<object> GetClanderYear(string plantId)
        {
            try
            {
                string sqlText = @"SELECT Id, YearNo,
                                    format(FromDate,'dd-MMM-yyyy') AS FromDate,
                                     format(ToDate,'dd-MMM-yyyy') AS ToDate
                                       FROM dbo.YearlyCalendar
                                         WHERE PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(sqlText, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void generateReport(string CalanderYearId, string FromDate, string ToDate, string plantId, string empID, string reportType, string tempId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                var broughtForwardForLeaveType = 0;
                //int lang = "+ languageId + @";
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Lvr" + plantId + tempId + ".xlsx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName); // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }
                //------
                fileName = "Lvr" + plantId + tempId + ".xlsx";
                strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName); // IDCardEng.xlsx
                File = fileName;
                if (!System.IO.File.Exists(strPath))
                {
                    throw new CustomException("File <" + fileName + "> Not Found.");
                }

                ///-----
                var Templatefile = GetFilePath(plantId, tempId, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    //File = "Lvr" + plantId + "English.docx";
                    File = "Lvr" + plantId + "English.xlsx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                //FileInfo DocFile = new FileInfo(strPath);
                //if (DocFile.Exists == false)
                //{
                //    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                //    throw new CustomException("File Not Found");
                //}
                //Creates a new instance for ExcelEngine
                ExcelEngine excelEngine = new ExcelEngine();

                //Loads or open an existing workbook through Open method of IWorkbooks
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(filepath);

                IWorksheet sheet = workbook.Worksheets[0];
                sheet.ShowColumn(0, true);
                DataTable dsBf, dsTransaction, dsBalance, dsHeader;

                //clsDataContext data = new clsDataContext();
                dsBf = loadBf(empID, CalanderYearId);  //brought forwad
                dsTransaction = loadLeaveTransactions(empID, FromDate, ToDate);
                dsBalance = loadOpeningBalance(empID, CalanderYearId);  //opening blance
                dsHeader = GetEmployeeBasicInfoById(empID, plantId, "Permanent", langID, tempId);

                IRange range = sheet.UsedRange;
                IRange columnList = range.Rows[0]; //IRange columnList = range.Rows[5];
                int columnListRow = 1;
                int ColumnTemplateRow = 1;
                for (int i = 0; i < range.Rows.Length; i++)
                {

                    if (string.IsNullOrEmpty(range["A" + (i + 1)].Text))
                        continue;
                    if (range["A" + (i + 1)].Text.ToUpper() == "COLUMNLIST")
                    {
                        columnListRow = (i + 1);

                    }
                    if (range["A" + (i + 1)].Text.ToUpper() == "REFROW")
                    {
                        columnList = range.Rows[i];
                        ColumnTemplateRow = (i + 1);
                    }
                }

                string PrefixOpeningBalance = "OB";
                string PrefixCurrentTransaction = "LT";
                string PrefixClosingBalance = "CB";


                //string AuthorizedSignature  = dsHeader.Rows[0]["AuthorizedSignature"].ToString();
                //string CardHolderSignature = dsHeader.Rows[0]["CardHolderSignature"].ToString();




                if (!string.IsNullOrEmpty(dsHeader.Rows[0]["AuthorizedSignature"].ToString()))
                {
                    var pic = dsHeader.Rows[0]["AuthorizedSignature"].ToString();
                    string picpath = ResourcesPathReader.GetAuthorizedSignaturePath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);


                            //section.Tables[3].Rows[0].Cells[1].Paragraphs[0].AppendPicture(newImage);


                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }

                }

                if (!string.IsNullOrEmpty(dsHeader.Rows[0]["CardHolderSignature"].ToString()))
                {
                    var pic = dsHeader.Rows[0]["CardHolderSignature"].ToString();
                    string picpath = ResourcesPathReader.GetCardHolderSignaturePath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            ///----section.Tables[3].Rows[0].Cells[0].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

                #region EmployeeInformation 

                for (int R = 0; R < columnListRow; R++)
                {
                    IRange columnListEmp = range.Rows[R];
                    foreach (DataColumn item in dsHeader.Columns)
                    {
                        for (int i = 0; i < range.Rows[R].Cells.Count(); i++)
                        {
                            if (string.IsNullOrEmpty(sheet[R + 1, i + 1].Text))
                                continue;

                            if (sheet[R + 1, i + 1].Text.ToUpper().Trim() == "{" + item.ColumnName.ToUpper() + "}")
                            {
                                //sheet[R + 1, i + 1].Text = dsHeader.Rows[0][item.ColumnName].ToString();
                                if (bplib.clsWebLib.IsNumeric(dsHeader.Rows[0][item.ColumnName].ToString()))
                                    sheet[R + 1, i + 1].Text = cnDgt(dsHeader.Rows[0][item.ColumnName].ToString(), tempId);
                                else if (bplib.clsWebLib.IsDateOK(dsHeader.Rows[0][item.ColumnName].ToString()))
                                    sheet[R + 1, i + 1].Text = GetFormatedDate(dsHeader.Rows[0][item.ColumnName].ToString(), tempId);
                                else
                                    sheet[R + 1, i + 1].Text = dsHeader.Rows[0][item.ColumnName].ToString();
                            }
                        }
                    }
                }

                #endregion
                int RefROW = ColumnTemplateRow;
                int ROW = ColumnTemplateRow + 1; int COL = 1;

                if (dsTransaction.Rows.Count > 0)
                {
                    for (int T = 0; T < dsTransaction.Rows.Count; T++)
                    {
                        broughtForwardForLeaveType = 0;
                        sheet[ROW, 2].Number = (T + 1);

                        foreach (DataRow item in dsBalance.Rows)
                            item["CurrentTransaction"] = 0;

                        for (int CELL = 0; CELL < columnList.Cells.Count(); CELL++)
                        {
                            string cellValue = columnList.Cells[CELL].Text;
                            if (string.IsNullOrEmpty(cellValue))
                                continue;

                            if (cellValue.ToUpper() == "{BF}")
                            {
                                if (dsBf.Rows.Count > 0)
                                {
                                    sheet[ROW, (CELL + 1)].Number = Convert.ToInt32(dsBf.Rows[0]["BroughtForward"].ToString());
                                    if (Convert.ToInt32(dsBf.Rows[0]["BroughtForward"].ToString()) > 0)
                                    {
                                        broughtForwardForLeaveType = Convert.ToInt32(dsBf.Rows[0]["BroughtForward"].ToString());
                                    }
                                    dsBf.Rows[0]["BroughtForward"] = 0;

                                }
                            }

                            if (cellValue.ToUpper().Contains("DATE"))
                            {
                                if (dsTransaction.Columns.Contains(cellValue.Replace("{", "").Replace("}", "")))
                                {
                                    sheet[ROW, (CELL + 1)].NumberFormat = "@";
                                    sheet[ROW, (CELL + 1)].Text = GetFormatedDate(Convert.ToDateTime(dsTransaction.Rows[T][cellValue.Replace("{", "").Replace("}", "")].ToString()).ToString("dd-MMM-yyyy"), tempId);
                                }
                            }

                            for (int OB = 0; OB < dsBalance.Rows.Count; OB++)
                            {
                                string leaveTypeOB = "{" + PrefixOpeningBalance + dsBalance.Rows[OB]["LeaveCode"].ToString() + "}";
                                if (leaveTypeOB.ToUpper() == cellValue.ToUpper())
                                {
                                    dsBalance.Rows[OB]["CurrentYearAllocation"] = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString())
                                   - dbl(dsBalance.Rows[OB]["CurrentTransaction"].ToString());
                                    sheet[ROW, CELL + 1].Number = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString());
                                }
                            }

                            string leaveTypeCL = "{" + PrefixCurrentTransaction + dsTransaction.Rows[T]["Code"].ToString() + "}";
                            if (leaveTypeCL.ToUpper() == cellValue.ToUpper())
                            {
                                sheet[ROW, CELL + 1].Number = dbl(dsTransaction.Rows[T]["LeaveDays"].ToString());

                                dsBalance.DefaultView.RowFilter = "LeaveCode='" + dsTransaction.Rows[T]["Code"].ToString() + "'";
                                if (dsBalance.DefaultView.Count > 0)
                                {
                                    dsBalance.DefaultView[0]["CurrentTransaction"] = dbl(dsTransaction.Rows[T]["LeaveDays"].ToString());
                                    dsBalance.DefaultView.RowFilter = null;
                                    dsBalance.AcceptChanges();
                                }
                            }

                            if (cellValue.Contains("CBSL") || cellValue.Contains("CBEL") || cellValue.Contains("CBCL"))
                            {
                                for (int OB = 0; OB < dsBalance.Rows.Count; OB++)
                                {
                                    //cellValue = columnList.Cells[OB + 16].Text;

                                    if (string.IsNullOrEmpty(cellValue) == false)
                                    {
                                        string leaveTypeOB = "{" + PrefixClosingBalance + dsBalance.Rows[OB]["LeaveCode"].ToString() + "}";
                                        if (leaveTypeOB.ToUpper() == cellValue.ToUpper())
                                        {
                                            double leaveTypeTotal = 0;
                                            //dsBalance.Rows[OB]["CurrentYearAllocationAsPerPolicy"] = 
                                            if (dsBalance.Rows[OB]["LeaveCode"].ToString() == "EL")
                                            {
                                                dsBalance.Rows[OB]["CurrentYearAllocation"] = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString())
                                           - dbl(dsBalance.Rows[OB]["CurrentTransaction"].ToString()) + dbl(broughtForwardForLeaveType.ToString());
                                                sheet[ROW, CELL + 1].Number = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString());

                                            }
                                            else
                                            {
                                                dsBalance.Rows[OB]["CurrentYearAllocation"] = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString())
                                              - dbl(dsBalance.Rows[OB]["CurrentTransaction"].ToString());
                                                sheet[ROW, CELL + 1].Number = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString());

                                            }
                                        }
                                    }
                                }
                            }





                        }




                        ////==============================



                        if (!string.IsNullOrEmpty(dsHeader.Rows[0]["AuthorizedSignature"].ToString()))
                        {
                            var pic = dsHeader.Rows[0]["AuthorizedSignature"].ToString();
                            string picpath = ResourcesPathReader.GetAuthorizedSignaturePath() + pic;
                            //WPicture ImgwPicture = new WPicture(document);
                            if (System.IO.File.Exists(picpath))
                            {
                                try
                                {
                                    Image Img = Image.FromFile(picpath);
                                    Image newImage = resizeImage(Img, 40, 100);
                                    sheet.Pictures.AddPicture(ROW, 21, newImage);

                                }
                                catch (Exception ex)
                                {
                                    throw (ex);
                                }
                            }

                        }

                        if (!string.IsNullOrEmpty(dsHeader.Rows[0]["CardHolderSignature"].ToString()))
                        {
                            var pic = dsHeader.Rows[0]["CardHolderSignature"].ToString();
                            string picpath = ResourcesPathReader.GetCardHolderSignaturePath() + pic;
                            //WPicture ImgwPicture = new WPicture(document);
                            if (System.IO.File.Exists(picpath))
                            {
                                try
                                {
                                    Image Img = Image.FromFile(picpath);
                                    Image newImage = resizeImage(Img, 40, 100);
                                    sheet.Pictures.AddPicture(ROW, 20, newImage);

                                }
                                catch (Exception ex)
                                {
                                    throw (ex);
                                }
                            }
                        }

                        ///=================================





                        ROW++;
                    }
                }
                else
                {
                    sheet[ROW, 2].Number = (1);
                    string cellValueOB = "";
                    for (int CELL = 0; CELL < columnList.Cells.Count(); CELL++)
                    {
                        cellValueOB = columnList.Cells[CELL].Text;
                        if (string.IsNullOrEmpty(cellValueOB))
                            continue;
                        if (cellValueOB.Contains("CBSL") || cellValueOB.Contains("CBEL") || cellValueOB.Contains("CBCL"))
                        {

                            for (int OB = 0; OB < dsBalance.Rows.Count; OB++)
                            {
                                //cellValue = columnList.Cells[OB + 16].Text;

                                if (string.IsNullOrEmpty(cellValueOB) == false)
                                {
                                    string leaveTypeOB = "{" + PrefixClosingBalance + dsBalance.Rows[OB]["LeaveCode"].ToString() + "}";
                                    if (leaveTypeOB.ToUpper() == cellValueOB.ToUpper())
                                    {
                                        double leaveTypeTotal = 0;
                                        //dsBalance.Rows[OB]["CurrentYearAllocationAsPerPolicy"] = 
                                        if (dsBalance.Rows[OB]["LeaveCode"].ToString() == "EL")
                                        {
                                            dsBalance.Rows[OB]["CurrentYearAllocation"] = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString())
                                       - dbl(dsBalance.Rows[OB]["CurrentTransaction"].ToString()) + dbl(broughtForwardForLeaveType.ToString());
                                            sheet[ROW, CELL + 1].Number = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString());

                                        }
                                        else
                                        {
                                            dsBalance.Rows[OB]["CurrentYearAllocation"] = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString())
                                          - dbl(dsBalance.Rows[OB]["CurrentTransaction"].ToString());
                                            sheet[ROW, CELL + 1].Number = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString());

                                        }
                                    }
                                }
                            }
                        }
                    }

                }

                sheet.DeleteRow(RefROW);
                sheet.HideColumn(1);
                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dsHeader.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dsHeader.Rows[0]["EmployeeCode"].ToString() + "-LeaveRegister-" + Convert.ToDateTime(FromDate).Year + ".xlsx";

                }
                else
                {
                    fileNames = "-LeaveRegister.xlsx";
                }
                workbook.SaveAs(fileNames, System.Web.HttpContext.Current.Response, ExcelDownloadType.Open);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private DataTable GetCurrentClanderYear(string plantId)
        {
            try
            {
                var sql = @"SELECT Id, YearNo,
                                    format(FromDate,'dd-MMM-yyyy') AS FromDate,
                                     format(ToDate,'dd-MMM-yyyy') AS ToDate
                                       FROM dbo.YearlyCalendar
                                         WHERE PlantId='" + plantId + @"'";
                var list = _sqlRepository.GetDataTable(sql);

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable MediasoftFairShopDataExport()
        {
            try
            {

                string sql = @"Select SystemId EmpID, EmployeeCode [RF Card No], c.UserName [Staff Type], 
                                l.UserName Designation, 
                                lk.Name [Designation Bangla], 
                                --d.UserName Designation, 
                                --lc.Name GivenDesignationId, 
                                dp.UserName Department, 
                                ld.Name [Department Bangla], EmployeeName Name, EmployeeNameLocal [Name Bangla], CellPhnNo Phone, EmailId Email, 
                                u.UserName Unit,'0' [FPS Enrollment],
                                (NoOfChildren+1) [Family Members],'2500' [Credit Limit], case when EmployeeStatus='Active' then 'Y' else 'N' end IsActive,NULL SpouseId From EmployeeInformation e
                                left join HKP.EmployeeCategory c on e.EmployeeCategorySystemID = c.Id
                                left join Hkp.LegalDesignation l on e.LegalDesignationId = l.Id
                                left join Hkp.Designation d on e.GivenDesignationId = d.Id
                                left join Org.Department dp on e.DepartmentId = dp.Id
                                left join Org.Unit u on e.UnitId = u.Id
                                left join hkp.LocalLanguage lc on e.GivenDesignationId = lc.DesignationId
                                left join hkp.LocalLanguage lk on e.LegalDesignationId = lk.LegalDesignationId
                                left join hkp.LocalLanguage ld on e.DepartmentId = ld.DepartmentId";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region MultipleIDCard
        private string GeIDIssuePK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(EmployeeIdCardIssue), out sID);
            return sID;
        }
        private void InsertOrUpdateEmployeeIdCardIssue(EmployeeIdCardIssue data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = "SELECT * FROM [dbo].[EmployeeIdCardIssue] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = GeIDIssuePK();
                    dr["Sequence"] = data.Sequence;
                    dr["EmpSystemId"] = data.EmpSystemId;
                    dr["EmployeeWorkTypeId"] = data.EmployeeWorkTypeId;
                    dr["IssueDate"] = data.IssueDate;

                    if (String.IsNullOrEmpty(data.ExpiryDate.ToString()))
                    {
                        dr["ExpiryDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ExpiryDate"] = data.ExpiryDate;
                    }

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["Sequence"] = data.Sequence;
                    dr["EmpSystemId"] = data.EmpSystemId;
                    dr["EmployeeWorkTypeId"] = data.EmployeeWorkTypeId;
                    dr["IssueDate"] = data.IssueDate;
                    if (String.IsNullOrEmpty(data.ExpiryDate.ToString()))
                    {
                        dr["ExpiryDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ExpiryDate"] = data.ExpiryDate;
                    }

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }


        public PdfDocument EmployeeMultipleIDCard(string empId, string companyGroupId, string companyId, string plantId, string tempId, string issuDate, string workTypeId, List<Dictionary<string, object>> dataList)
        {
            try
            {
                string langID = "";
                string langName = "";
                string reportType = "IdCard";
                string File = "";
                string strPath = "";
                var fileName = "";
                var c = empId.Split(new char[] { ' ', '.', ',', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;

                ConvertExcelToImage convertExcelToImage = null;
                var excelEngine = new ExcelEngine();
                var report = new ReportUtility();
                IApplication application = null;
                application = excelEngine.Excel;
                IWorkbook workbook = null;



                //tempId = "M6";
                var lang = GetLanguage(plantId, tempId, reportType);
                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    langName = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    langName = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "IdCard" + plantId + langName + ".xlsx";

                }
                var dtEmp = GetMultipleEmployeeInfoById(empId, plantId, langID, tempId); // Get Employee Data
                var Templatefile = GetFilePath(plantId, langName, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }


                //if (System.IO.File.Exists(strPath) && langName != "English")
                //{
                //    FileStream = (Stream)System.IO.File.Open(strPath,FileMode.Open);
                //}
                //else
                //{
                //    FileStream = (Stream)System.IO.File.Open(strPath, FileMode.Open);
                //   // workbook = excelEngine.Excel.Workbooks.Open(strPath);
                //}
                List<IWorkbook> workbookList = new List<IWorkbook>();
                using (FileStream filestream = new FileStream(strPath, FileMode.Open))
                {

                    for (int i = 0; i < dtEmp.Rows.Count; i++)
                    {
                        filestream.Position = 0;//reading the memory from the begining
                        workbook = excelEngine.Excel.Workbooks.Open(filestream);
                        PrintEmployeeIDCardAll(dtEmp.Rows[i]["EmployeeCode"].ToString(), workbook, dtEmp.Rows[i], companyGroupId, companyId, plantId, tempId, "", "IdCard", issuDate, dtEmp.Rows[i]["EmployeeWorkType"].ToString(), langID);
                        workbookList.Add(workbook);
                    }
                }
                convertExcelToImage = new ConvertExcelToImage(workbookList, 85f, 54f);
                PdfDocument doc = convertExcelToImage.ConvertToPdf(4f);


                return doc;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
               Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
               ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        #endregion

        #region Lock and Un-lock
        public void GetUnApprovedEmployeeListData(string lockDate, out DataSet dsMaster)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            //DataSet dsMaster;
            try
            {
                if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                }


                string sql = @"Select SystemID,EmployeeCode,EmployeeName From EmployeeInformation 
                                Where DOJ between    DATEFROMPARTS(year('" + lockDate + "'),month('" + lockDate + "'),1)   and '" + lockDate + @"'  AND 
                                isApproved=0  AND PlantId='" + identity.PlantId + "'";



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public void CheckAttdenceProcAndShiftAssignData(string lockDate, out DataSet dsMaster)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            //DataSet dsMaster;
            try
            {
                if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                }


                string sql = @"SELECT DISTINCT ISNULL(ShiftNotAssignedEmployee.totalShiftNotAssignedEmployee, 0) ShiftNotAssignedEmployee
	                                ,ISNULL(AttdnNotProcessedToday.totalAttdnNotProcessedToday, 0) totalAttdnNotProcessedToday
                                FROM (
	                                SELECT COUNT(E.SystemId) totalEmployee
		                                ,C.UserName
		                                ,cg.Id CompanyGroupId
		                                ,c.Id CompanyId
		                                ,c.UserName CompanyName
		                                ,cg.UserName GroupName
	                                FROM ORG.CompanyGroup CG
	                                LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
	                                LEFT JOIN EmployeeInformation E ON e.GroupID = CG.Id
		                                AND c.Id = E.CompanyId
	                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
	                                LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
	                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
	                                WHERE GroupID = '" + identity.CompanyGroupId + @"'
		                                AND (
			                                E.EmployeeStatus != 'Separated'
			                                OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + lockDate + @"')
			                                )
		                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + lockDate + @"')

                                    GROUP BY C.UserName
		                                ,cg.Id
		                                ,c.Id
		                                ,c.UserName
		                                ,cg.UserName
	                                ) OnRoleEmployee
                                LEFT JOIN (
	                                SELECT COUNT(E.SystemId) totalShiftNotAssignedEmployee
		                                ,cg.Id CompanyGroupId
		                                ,cg.UserName GroupName
		                                ,C.Id AS CompanyId
		                                ,C.UserName CompanyName
	                                FROM ORG.CompanyGroup CG
	                                LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
	                                LEFT JOIN (
		                                --*
		                                SELECT *
		                                FROM EmployeeInformation
		                                WHERE SystemId NOT IN (
				                                --**
				                                SELECT DISTINCT EmpSystemID
				                                FROM EmployeeShiftAssign
				                                ) -- * *
		                                ) -- *
		                                E ON e.GroupID = CG.Id
		                                AND c.Id = E.CompanyId
	                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
	                                LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
	                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
	                                WHERE GroupID = '" + identity.CompanyGroupId + @"'
		                                AND (
			                                E.EmployeeStatus != 'Separated'
			                                OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + lockDate + @"')
			                                )
		                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + lockDate + @"')

                                    GROUP BY C.UserName
		                                ,cg.UserName
		                                ,C.Id
		                                ,cg.Id
		                                ,cg.UserName
	                                ) ShiftNotAssignedEmployee ON OnRoleEmployee.CompanyGroupId = ShiftNotAssignedEmployee.CompanyGroupId
	                                AND OnRoleEmployee.CompanyId = ShiftNotAssignedEmployee.CompanyId
                                LEFT JOIN (
	                                SELECT count(E.SystemID) totalAttdnNotProcessedToday
		                                ,cg.Id CompanyGroupId
		                                ,cg.UserName GroupName
		                                ,C.Id AS CompanyId
		                                ,C.UserName UId
	                                FROM ORG.CompanyGroup CG
	                                LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
	                                INNER JOIN EmployeeInformation E ON E.GroupID = CG.Id
		                                AND c.Id = E.CompanyId
	                                INNER JOIN (
		                                --*
		                                SELECT TOP 1
		                                WITH TIES *
		                                FROM EmployeeShiftAssign
		                                WHERE EffectiveDate <= GETDATE()
			                                AND EmpSystemID NOT IN (
				                                --**
				                                SELECT DISTINCT EmpSystemID
				                                FROM AttdnProcessData
				                                WHERE CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + lockDate + @"')
				                                )
		                                ORDER BY ROW_NUMBER() OVER (
				                                PARTITION BY EmpSystemID ORDER BY EffectiveDate DESC
				                                )
		                                ) -- *
		                                ESA ON E.SystemId = ESA.EmpSystemID
	                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
	                                LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
	                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
	                                WHERE GroupID = '" + identity.CompanyGroupId + @"'
		                                AND (
			                                E.EmployeeStatus != 'Separated'
			                                OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + lockDate + @"')
			                                )
		                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + lockDate + @"')

                                    GROUP BY C.UserName
		                                ,cg.UserName
		                                ,C.Id
		                                ,cg.Id
		                                ,cg.UserName
	                                ) AttdnNotProcessedToday ON OnRoleEmployee.CompanyGroupId = AttdnNotProcessedToday.CompanyGroupId
	                                AND OnRoleEmployee.CompanyId = AttdnNotProcessedToday.CompanyId";




                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public void CheckOTConfirmationData(string lockDate, out DataSet dsMaster)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            //DataSet dsMaster;
            try
            {
                if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                }


                string sql = @"select AP.EmpSystemId, FORMAT(AP.WorkDate,'dd-MMM-yyyy') WorkDate
                                ,E.EmployeeCode,E.EmployeeName from AttdnProcessData AP
                                LEFT JOIN EmployeeInformation E ON E.SystemId=AP.EmpSystemId
                                Where AP.WorkDate=''" + lockDate + @"'' and AP.IsOTComfirm=0 and AP.IsOTEntitled=1 and AP.PlantID='" + identity.CompanyGroupId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public void CreateLockData(string lockDate)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;


            //DataSet dsMaster;
            try
            {
                if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                }
                GetUnApprovedEmployeeListData(lockDate, out DataSet dsUnApprovedList);
                if (dsUnApprovedList.Tables[0].Rows.Count > 0)
                {
                    string UnApprovedEmpList = string.Empty;
                    foreach (DataRow item in dsUnApprovedList.Tables[0].Rows)
                    {
                        UnApprovedEmpList = UnApprovedEmpList + item["EmployeeCode"].ToString() + " - " + item["EmployeeName"].ToString() + "</br>";
                    }


                    throw new Exception(UnApprovedEmpList);

                }




                string sql = @"SELECT [Id]
                                    ,[PlantId]
                                    ,[LockedDate]
                                    ,[AddedBy]
                                    ,[AddedDate]
                                    ,[AddedFromIP]
                                    ,[UpdatedBy]
                                    ,[UpdatedDate]
                                    ,[UpdatedFromIP]
                                     FROM [PlantWiseAttendanceLock] where LockedDate='" + lockDate + "' AND PlantId='" + identity.PlantId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "PLANTWISEATTENDANCELOCK", out sID);
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "AL" + sID;
                    dr["PlantId"] = identity.PlantId;
                    dr["LockedDate"] = lockDate;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);


                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["PlantId"] = identity.PlantId;
                    dr["LockedDate"] = lockDate;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                }





                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        #endregion

        #region Employee Approval

        public void SaveApprovedEmployeeData(DataSet dsGrd)
        {

            clsStaticInfo obj = new clsStaticInfo();
            DataSet dsEmplist = null;
            DataRow drEmplist = null;
            DataTable dtEmplist = null;
            DataView dvEmplist = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {







                //LoadDataSetFromDataGrid(ref dgAttdnProc, out dsGrd);
                string lblEmpSysIDForAttdSummry = "";
                if (dsGrd.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsGrd.Tables[0].Rows.Count; i++)
                    {
                        if (Convert.ToBoolean(dsGrd.Tables[0].Rows[i]["CheckBoxSelect"].ToString().Trim()) == true)
                        {
                            if (lblEmpSysIDForAttdSummry == "")
                            {
                                lblEmpSysIDForAttdSummry = "'" + dsGrd.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                            }
                            else
                            {
                                lblEmpSysIDForAttdSummry = lblEmpSysIDForAttdSummry + ", '" + dsGrd.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(lblEmpSysIDForAttdSummry.Trim()) == true)
                {
                    //lblEmpSysIDForAttdSummry.Focus();
                    Exception ex = new Exception("Please select employee...");
                    throw (ex);
                }







                GetUnApprovedEmployeeListDataSet(out dsEmplist);
                dtEmplist = dsEmplist.Tables[0];
                dvEmplist = new DataView();


                if (dsGrd.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsGrd.Tables[0].Rows.Count; i++)
                    {
                        string sEmpSysID = string.Empty;
                        if (Convert.ToBoolean(dsGrd.Tables[0].Rows[i]["CheckBoxSelect"].ToString().Trim()) == true)
                        {
                            sEmpSysID = dsGrd.Tables[0].Rows[i]["SystemID"].ToString().Trim();
                            dvEmplist.Table = dtEmplist;
                            dvEmplist.RowFilter = "SystemID = '" + sEmpSysID.Trim() + "'";
                            if (dvEmplist.Count > 0)
                            {
                                drEmplist = dvEmplist[0].Row;
                                drEmplist.BeginEdit();
                                drEmplist["IsApproved"] = 1;
                                drEmplist["FirstTimeLock"] = 1;
                                drEmplist["ApprovedFromIP"] = bplib.clsWebLib.RetValidLen(identity.IPAddress);
                                drEmplist["ApprovedBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                                drEmplist["ApprovedDateTime"] = DateTime.Now;
                                drEmplist.EndEdit();
                            }
                            dvEmplist.RowFilter = null;



                        }
                    }
                }

                obj.SaveDataSets(dsEmplist);





            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                // clean variable
            }
        }//End Function


        public void GetUnApprovedEmployeeListDataSet(out DataSet dsMaster)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            //DataSet dsMaster;
            try
            {



                string sql = @"Select * From EmployeeInformation EI
                               WHERE EI.EmployeeStatus !='Separated' AND EI.IsApproved =0 AND 
                                    EI.PlantId='" + identity.PlantId + @"' AND  EI.GroupId='" + identity.CompanyGroupId + @"'";



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        #endregion

        #region Employee Un-Approval

        public void SaveUnApprovedEmployeeData(DataSet dsGrd)
        {

            clsStaticInfo obj = new clsStaticInfo();
            DataSet dsEmplist = null;
            DataRow drEmplist = null;
            DataTable dtEmplist = null;
            DataView dvEmplist = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {







                //LoadDataSetFromDataGrid(ref dgAttdnProc, out dsGrd);
                string lblEmpSysIDForAttdSummry = "";
                if (dsGrd.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsGrd.Tables[0].Rows.Count; i++)
                    {
                        if (Convert.ToBoolean(dsGrd.Tables[0].Rows[i]["CheckBoxSelect"].ToString().Trim()) == true)
                        {
                            if (lblEmpSysIDForAttdSummry == "")
                            {
                                lblEmpSysIDForAttdSummry = "'" + dsGrd.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                            }
                            else
                            {
                                lblEmpSysIDForAttdSummry = lblEmpSysIDForAttdSummry + ", '" + dsGrd.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(lblEmpSysIDForAttdSummry.Trim()) == true)
                {
                    //lblEmpSysIDForAttdSummry.Focus();
                    Exception ex = new Exception("Please select employee...");
                    throw (ex);
                }







                GetApprovedEmployeeListDataSet(out dsEmplist);
                dtEmplist = dsEmplist.Tables[0];
                dvEmplist = new DataView();


                if (dsGrd.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsGrd.Tables[0].Rows.Count; i++)
                    {
                        string sEmpSysID = string.Empty;
                        if (Convert.ToBoolean(dsGrd.Tables[0].Rows[i]["CheckBoxSelect"].ToString().Trim()) == true)
                        {
                            sEmpSysID = dsGrd.Tables[0].Rows[i]["SystemID"].ToString().Trim();
                            dvEmplist.Table = dtEmplist;
                            dvEmplist.RowFilter = "SystemID = '" + sEmpSysID.Trim() + "'";
                            if (dvEmplist.Count > 0)
                            {
                                drEmplist = dvEmplist[0].Row;
                                drEmplist.BeginEdit();
                                drEmplist["IsApproved"] = 0;
                                drEmplist["UnApprovedFromIP"] = bplib.clsWebLib.RetValidLen(identity.IPAddress);
                                drEmplist["UnApprovedBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                                drEmplist["UnApprovedDateTime"] = DateTime.Now;
                                drEmplist.EndEdit();
                            }
                            dvEmplist.RowFilter = null;



                        }
                    }
                }

                obj.SaveDataSets(dsEmplist);





            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                // clean variable
            }
        }//End Function


        public void GetApprovedEmployeeListDataSet(out DataSet dsMaster)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            //DataSet dsMaster;
            try
            {



                string sql = @"Select * From EmployeeInformation EI
                               WHERE EI.EmployeeStatus !='Separated' AND EI.IsApproved =1 AND 
                                    EI.PlantId='" + identity.PlantId + @"' AND  EI.GroupId='" + identity.CompanyGroupId + @"'";



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        #endregion



        #region LocalLanguageLabel

        public object GetLocalLanguageLabel(string plantId)
        {
            string strSQL;
            try
            {
                strSQL = @"
                        Select NameLabel,FatherNameLabel,MotherNameLabel,SpouseNameLabel,IdentificationMarksLabel,NomineeLabel,AddressLabel,LandLabel,MobileNoLabel,PAddressLabel,PermanentLabel 
                        ,L.DependantLabel, M.LeaveLabel, N.DesignationLabel,PS.OperationSetting FROM ORG.Plant P
                        LEFT JOIN (SELECT Name NameLabel,LanguageId FROM HKP.LocalLanguage WHERE LabelName='Name') A ON A.LanguageId=P.LanguageId
                        LEFT JOIN (SELECT Name FatherNameLabel,LanguageId FROM HKP.LocalLanguage WHERE LabelName='FatherName') B ON B.LanguageId=P.LanguageId
                        LEFT JOIN (SELECT Name MotherNameLabel,LanguageId FROM HKP.LocalLanguage WHERE LabelName='MotherName') C ON C.LanguageId=P.LanguageId
                        LEFT JOIN (SELECT Name SpouseNameLabel,LanguageId FROM HKP.LocalLanguage WHERE LabelName='SpouseName') D ON D.LanguageId=P.LanguageId
                        LEFT JOIN (SELECT Name IdentificationMarksLabel,LanguageId FROM HKP.LocalLanguage WHERE LabelName='IdentificationMarks') E ON E.LanguageId=P.LanguageId
                        LEFT JOIN (SELECT Name NomineeLabel,LanguageId FROM HKP.LocalLanguage WHERE LabelName='NomineeInfo') F ON F.LanguageId=P.LanguageId
                        LEFT JOIN (SELECT Name AddressLabel,LanguageId FROM HKP.LocalLanguage WHERE LabelName='Address') G ON G.LanguageId=P.LanguageId
                        LEFT JOIN (SELECT Name LandLabel,LanguageId FROM HKP.LocalLanguage WHERE LabelName='LanOwnerInfo') H ON H.LanguageId=P.LanguageId
                        LEFT JOIN (SELECT LanguageId,Name MobileNoLabel FROM HKP.LocalLanguage WHERE LabelName='MobileNo') I ON I.LanguageId=P.LanguageId
                        LEFT JOIN (SELECT LanguageId,Name PAddressLabel FROM HKP.LocalLanguage WHERE LabelName='PresentAddress') J ON J.LanguageId=P.LanguageId
                        LEFT JOIN (SELECT LanguageId,Name PermanentLabel FROM HKP.LocalLanguage WHERE LabelName='Permanent') K ON K.LanguageId=P.LanguageId
                        LEFT JOIN (SELECT LanguageId,Name DependantLabel FROM HKP.LocalLanguage WHERE LabelName='Dependant') L ON L.LanguageId = P.LanguageId
						LEFT JOIN (SELECT LanguageId,Name LeaveLabel FROM HKP.LocalLanguage WHERE LabelName='ReasonForLeave') M ON M.LanguageId = P.LanguageId
                        LEFT JOIN (SELECT LanguageId,Name DesignationLabel FROM HKP.LocalLanguage wHERE LabelName='Designation') N ON N.LanguageId=P.LanguageId
                        LEFT JOIN PlantWiseHRMSSetting PS ON PS.PlantID=P.Id
                        WHERE P.Id='" + plantId + "'";

                return _sqlRepository.GetData(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetOperationVariationCbo(string companyGroupId)
        {
            try
            {
                var sql = @"SELECT Id [Value], UserName [Text] FROM MST.OperationVariation WHERE CompanyGroupId='" + companyGroupId + "' ORDER BY UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public IEnumerable<object> GetOperationVariation(string companyGroupId/*,string empSystemId*/)
        {
            try
            {
                var sql = @"SELECT OM.Id,OM.Code,OM.ShortName,OM.StandardName,OM.UserName,MM.StandardName MachineMaster,S.UserName Skill,0 CycleTime, [check]=CAST (0 AS bit) FROM MST.OperationVariation OM
                          LEFT JOIN MST.MaterialMasterArticle MM ON MM.Id=OM.ArticleId
                          LEFT JOIN HKP.Skill S ON S.Id=OM.SkillId
                          WHERE  OM.CompanyGroupId='" + companyGroupId + "' ORDER BY UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public IEnumerable<object> GetOperationMaster(string companyGroupId/*, string empSystemId*/)
        {
            try
            {
                var sql = @"SELECT OM.Id,OM.Code,OM.ShortName,OM.StandardName,OM.UserName,MM.UserName MachineMaster,S.UserName Skill,0 CycleTime,[check]=CAST (0 AS bit) FROM MST.OperationMaster OM
                          LEFT JOIN MST.MachineMaster MM ON MM.Id=OM.MachineMasterId 
                          LEFT JOIN HKP.Skill S ON S.Id=OM.SkillId
                          WHERE  OM.CompanyGroupId='" + companyGroupId + "' ORDER BY UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        #endregion

        #region  #region IDCard Issue


        public decimal GetAutoSequence(string empSystemId)
        {
            try
            {
                return _employeeIdCardIssue.Query(r => r.EmpSystemId == empSystemId).Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        public IEnumerable<object> GetIssueIdCardByEmployee(string employeeId)
        {
            try
            {
                string sql = @"SELECT I.Id,I.Sequence,I.EmpSystemId,I.EmployeeWorkTypeId,FORMAT(I.IssueDate,'dd-MMM-yyyy') IssueDate,
                               FORMAT(I.ExpiryDate,'dd-MMM-yyyy') ExpiryDate,W.UserName FROM [dbo].[EmployeeIdCardIssue] I
                               LEFT JOIN [dbo].[EmployeeWorkType] W ON W.Id=I.EmployeeWorkTypeId
                               Where I.EmpSystemId='" + employeeId + "' ORDER BY I.IssueDate DESC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetWarningLetterByEmployee(string employeeId)
        {
            try
            {
                string sql = @"SELECT I.*,W.UserName FROM [dbo].[EmployeeIdCardIssue] I
                               LEFT JOIN [dbo].[EmployeeWorkType] W ON W.Id=I.EmployeeWorkTypeId
                               Where I.EmpSystemId='" + employeeId + "' ORDER BY I.IssueDate DESC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetAllEmployeeDataWithWorkType(string companyId, string plantId)
        {
            try
            {
                string CmdText = @"SELECT [CheckBoxSelect] = Convert(bit, 'True'), E.SystemId EmpSystemId
                                    ,E.EmployeeCode,E.EmployeeName,LD.UserName Designation,DEPT.UserName AS Department
                                    ,DV.UserName AS Division,SC.UserName AS Section,SS.UserName SubSection
                                    ,FORMAT(E.DOJ,'dd-MMM-yyyy') DOJ,EC.UserName EmployeeCategory
                                    ,E.EmployeeStatus,M.EmployeeWorkTypeId,WT.UserName AS EmployeeWorkType
                                    ,M.Sequence,FORMAT(M.IssueDate,'dd-MMM-yyyy') IssueDate,FORMAT(M.ExpiryDate,'dd-MMM-yyyy') ExpiryDate
                                FROM EmployeeInformation E
                                LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=PR.DesignationId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
                                LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
                                LEFT JOIN ORG.Division DV ON E.DivisionId = DV.Id
                                LEFT JOIN ORG.Section SC ON E.SectionId = SC.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
                                LEFT JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
                                LEFT JOIN [EmployeeIdCardIssue] M ON m.EmpSystemId=e.SystemId
                                AND M.Id=(SELECT TOP 1 ID FROM [EmployeeIdCardIssue] EII WHERE EII.EmpSystemId=e.SystemId ORDER BY EII.Sequence DESC )
                                LEFT JOIN [dbo].[EmployeeWorkType] WT ON WT.Id=m.EmployeeWorkTypeId
                                WHERE E.EmployeeStatus='Active' AND E.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        #endregion
    }
    public class ConvertPresentationToPdf
    {
        public ConvertPresentationToPdf()
        {

        }
        public static void SetText(ISlide Slide, string Key, string Text, string FontName, int FontSize = 0)
        {
            for (int s = 0; s < Slide.Shapes.Count; s++)
            {
                var k = Slide.Shapes[s] as Syncfusion.Presentation.IShape;

                if (k.SlideItemType == SlideItemType.AutoShape)
                {
                    if (k.TextBody.Paragraphs[0].Text.ToUpper().Trim() == string.Concat("{", Key.ToUpper().Trim(), "}"))
                    {

                        while (k.TextBody.Paragraphs.Count > 1)
                            k.TextBody.Paragraphs.RemoveAt(1);


                        k.TextBody.WrapText = true;
                        k.TextBody.FitTextOption = FitTextOption.ShrinkTextOnOverFlow;
                        IParagraph paragraph = k.TextBody.Paragraphs[0];
                        paragraph.Text = Text;
                        paragraph.Font.FontName = FontName;
                        if (FontSize > 0)
                            paragraph.Font.FontSize = FontSize;
                    }

                }
            }
        }
        public static void SetPicture(ISlide Slide, string PictureBoxName, Image image)
        {
            for (int s = 0; s < Slide.Shapes.Count; s++)
            {
                var k = Slide.Shapes[s] as Syncfusion.Presentation.IPicture;
                if (k == null)
                    continue;
                if (k.SlideItemType == SlideItemType.Picture)
                {

                    if (k.ShapeName.ToUpper().Trim() == PictureBoxName.ToUpper().Trim())
                    {
                        double Top = k.Top;
                        double Left = k.Left;
                        double Height = k.Height;
                        double Width = k.Width;

                        k.ImageData = ImageToByteArray(image);
                        k.Top = Top;
                        k.Left = Left;
                        k.Height = Height;
                        k.Width = Width;
                    }
                }
            }
        }

        public static void SetQRCode(ISlide Slide, string PictureBoxName, Image image)
        {
            for (int s = 0; s < Slide.Shapes.Count; s++)
            {
                var k = Slide.Shapes[s] as Syncfusion.Presentation.IPicture;
                if (k == null)
                    continue;
                if (k.SlideItemType == SlideItemType.Picture)
                {
                    if (k.ShapeName.ToUpper().Trim() == PictureBoxName.ToUpper().Trim())
                    {
                        double Top = k.Top;
                        double Left = k.Left;
                        double Height = k.Height;
                        double Width = k.Width;

                        k.ImageData = QRCodeToByteArrays(image);
                        k.Top = Top;
                        k.Left = Left;
                        k.Height = Height;
                        k.Width = Width;
                    }

                }
            }
        }

        public static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }
        public static byte[] QRCodeToByteArrays(System.Drawing.Image imageIn)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

    }
    public class ParaModel
    {
        public string EmpSystemId { get; set; }
        public string EmployeeWorkTypeId { get; set; }
    }

    public static class DClone
    {
        public static T DeepClone<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
    }
    public class EmployeeAccountsGroup
    {
        #region  Properties

        public string Id { get; set; }
        [NeverUpdate]
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime AddedDate { get; set; }
        [NeverUpdate]
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public string AccountsGroupId { get; set; }
        public string PlantId { get; set; }
        public string CompanyGroupId { get; set; }
        public string EmployeeId { get; set; }

        #endregion  Properties
    }
}