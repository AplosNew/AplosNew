#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.External;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.External
{
    public class EmployeeService : Service<Employee>, IEmployeeService
    {
        #region Constructor

        private readonly IRepositoryAsync<CompanyGroupEmp> _companyGroupRepository;
        private readonly IRepositoryAsync<Employee> _employeeRepository;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeService(
              IRepositoryAsync<CompanyGroupEmp> companyGroupRepository
            , IRepositoryAsync<Employee> employeeRepository
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IPKGeneratorService pkGeneratorService
            ) : base(employeeRepository, unitOfWork, pkGeneratorService)
        {
            _companyGroupRepository = companyGroupRepository;
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Operation

        public override void Update(Employee entity)
        {
            try
            {
                var dblist = Find(entity.Id);
                dblist.FirstName = entity.FirstName;
                dblist.LastName = entity.LastName;
                dblist.Email = entity.Email;
                dblist.Mobile = entity.Mobile;
                dblist.SalutationId = entity.SalutationId;
                dblist.ReportingOfficerId = entity.ReportingOfficerId;
                dblist.FatherName = entity.FatherName;
                dblist.MotherName = entity.MotherName;
                dblist.BirthdayCelebrationDate = entity.BirthdayCelebrationDate;
                base.Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateUserAccess(Employee entity)
        {
            try
            {
                var dblist = Find(entity.Id);
                dblist.AccessUser = entity.AccessUser;
                dblist.InitialPIN = entity.InitialPIN;
                dblist.NewPIN = entity.NewPIN;
                dblist.AccessUserDateTime = DateTime.Now;
                dblist.PinChangeDateTime = DateTime.Now;
                base.Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateEmployeeSubmit(Employee entity)//From EmployeeAccessForm
        {
            try
            {
                var dblist = Find(entity.Id);
                dblist.Submit = entity.Submit;
                base.Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdatePIN(string id, string newPin)
        {
            try
            {
                if (string.IsNullOrEmpty(newPin))
                    throw new CustomException("New PIN Required");
                var dbData = Find(id);
                dbData.IsFirstlogin = true;
                dbData.NewPIN = newPin;
                dbData.FirstLoginTime = DateTime.Now;
                dbData.LastLoginTime = DateTime.Now;
                base.Update(dbData);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateSubmit(Employee entity)//From main form
        {
            try
            {
                var dbData = Find(entity.Id);
                //Validation(entity.Id);
                dbData.Submit = true;
                dbData.LastLoginTime = DateTime.Now;
                dbData.SubmitTime = DateTime.Now;
                base.Update(dbData);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateAccessRestriction(IEnumerable<Employee> list)
        {
            var flag = false;
            try
            {
                var ids = list.Select(t => t.Id).ToArray();
                var dbData = base.Query(t => ids.Contains(t.Id)).Select().AsEnumerable();
                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var item in list)
                {
                    var data = dbData.FirstOrDefault(t => t.Id == item.Id);
                    if (data != null)
                        data.IsAccessRestricted = item.IsAccessRestricted;
                    UpdateGraph(data);
                }
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

        public Employee Login(string id, string initialpin)
        {
            try
            {
                var data = Find(id);
                if (data == null)
                    throw new CustomException("Invalid employee id");
                if (data.IsFirstlogin)
                {
                    if (data.NewPIN != initialpin) throw new CustomException("Invalid pin");
                }
                else if (data.InitialPIN != initialpin) throw new CustomException("Invalid pin");
                if (data.IsAccessRestricted)
                    throw new CustomException("Your access has been restricted.");
                data.LastLoginTime = DateTime.Now;
                base.Update(data);

                //var data = base.Query(t => t.Id == id && t.InitialPIN == initialpin).Select().ToList().FirstOrDefault();
                //if (data == null)
                //    throw new CustomException("Invalid employee or pin");
                //if (data.IsAccessRestricted)
                //    throw new CustomException("Your access has been restricted.");
                //data.LastLoginTime = DateTime.Now;
                //base.Update(data);
                return data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public Dictionary<string, object> Query(string id)
        {
            try
            {
                var _sql = @"Select  E.Id
                                              ,E.CompanyId
                                              ,E.Code
                                              ,E.ReportingOfficerId
                                              ,E.Name
                                              ,E.FirstName
                                              ,E.LastName
                                              ,E.FatherName
                                              ,E.MotherName
                                              ,REPLACE(CONVERT(CHAR(11), E.DOB, 106),' ','-') DOB,REPLACE(CONVERT(CHAR(11), E.DOJ, 106),' ','-') DOJ
                                              ,REPLACE(CONVERT(CHAR(11), E.BirthdayCelebrationDate, 106),' ','-') BirthdayCelebrationDate
                                              ,E.SalutationId
                                              ,E.InitialPIN
                                              ,E.IsFirstlogin
                                              ,E.NewPIN
                                              ,E.Mobile
                                              ,E.Email
                                              ,E.Col1,E.Col2,E.Col3,E.Col4,E.Col5,E.Col6,E.Col7,E.Col8,E.Col9,E.Col10
											  ,E.Col11,E.Col12,E.Col13,E.Col14,E.Col15,E.Col16,E.Col17,E.Col18,E.Col19,E.Col20
                                              ,E.Submit
                                              ,S.Name Salutation
                                              ,EMP.Name ReportingOfficerName
                                              ,CG.Id AS CompanyGroupId,CG.LogoFileName,CG.DocumentFolderName, CG.Name AS GroupName,
											   C.MobileLength CompanyMobileLength
                                    From dbo.Employee E
                                    INNER JOIN Company AS C ON C.Id=E.CompanyId
                                    INNER JOIN CompanyGroup AS CG ON CG.Id=C.CompanyGroupId
                                    LEFT OUTER JOIN dbo.Salutation S ON E.SalutationId= S.Id
                                    LEFT OUTER JOIN dbo.Employee EMP ON EMP.Id= E.ReportingOfficerId
                                    Where E.Id='" + id + "'";
                return _sqlRepository.GetData(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> QueryList(string id)
        {
            try
            {
                var _sql = @"SELECT AET.ColumnName, AET.AplosEmpFieldId, ET.AplosColumnName FROM [dbo].[Employee] AS EMP
                                       LEFT OUTER JOIN [dbo].[Company] AS C ON EMP.CompanyId=C.Id
                                       LEFT OUTER JOIN [dbo].[CompanyGroup] CG ON C.CompanyGroupId= CG.Id
                                       LEFT OUTER JOIN [dbo].[AplosEmpFieldTag] AS AET ON CG.Id=AET.CompanyGroupId
                                       LEFT JOIN [dbo].[AplosEmpField] AS ET ON AET.AplosEmpFieldId = ET.Id
                                       Where EMP.Id='" + id + "' AND AET.IsAplicable='1'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> GetDynamicData(string employeeId)
        {
            try
            {
                string sql = @"SELECT AET.ColumnName, AET.AplosEmpFieldId, ET.AplosColumnName FROM [dbo].[Employee] AS EMP
                                       LEFT OUTER JOIN [dbo].[Company] AS C ON EMP.CompanyId=C.Id
                                       LEFT OUTER JOIN [dbo].[CompanyGroup] CG ON C.CompanyGroupId= CG.Id
                                       LEFT OUTER JOIN [dbo].[AplosEmpFieldTag] AS AET ON CG.Id=AET.CompanyGroupId
                                       LEFT JOIN [dbo].[AplosEmpField] AS ET ON AET.AplosEmpFieldId = ET.Id
                                       Where EMP.Id='" + employeeId + "'  AND AET.IsAplicable=1";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel QueryReportingOfficer(GridParameter parameters, string companyGroupId, string id)
        {
            try
            {
                parameters.CmdText = @"Select E.Id, E.Code, E.Name, E.FirstName, E.LastName From dbo.Employee E
									 Left outer Join [dbo].[Company] C ON E.CompanyId=C.Id
									 Left outer Join [dbo].[CompanyGroup] CG ON C.CompanyGroupId=CG.Id
									 Where CG.Id='" + companyGroupId + @"' AND E.Id<>'" + id + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in base.Query().Select().ToList().OrderBy(r => r.Name)
                       select new { Text = m.Name, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetCboList(string companyGroupId)
        {
            try
            {
                var sql = @"Select S.Id AS [Value], S.Name AS [Text] From dbo.Salutation AS S Where S.CompanyGroupId='" + companyGroupId + "'";

                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public GridModel GetNameCboList()
        {
            try
            {
                var sql = @"Select E.Id AS [Value], E.Name AS [Text] From dbo.Employee AS E";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public GridModel GetActivityCategoryCboList()
        {
            try
            {
                var sql = @"Select AC.Id AS [Value], AC.Name AS [Text] From dbo.ActivityCategory AS AC";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public GridModel GetActivityImportanceCboList()
        {
            try
            {
                var sql = @"Select AI.Id AS [Value], AI.Name AS [Text] From dbo.ActivityImportance AS AI";

                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public GridModel GetPeriodCboList()
        {
            try
            {
                var sql = @"Select P.Id AS [Value], P.Name AS [Text] From dbo.Period AS P";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public GridModel GetDocumentFormateCboList()
        {
            try
            {
                var sql = @"Select DF.Id AS [Value], DF.Name AS [Text] From dbo.DocumentFormate AS DF";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public GridModel GetDataSourceCategoryCboList()
        {
            try
            {
                var sql = @"Select DSC.Id AS [Value], DSC.Name AS [Text] From dbo.DataSourceCategory AS DSC";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public Dictionary<string, object> GetEmployee(string employeeId)
        {
            try
            {
                var sql = @"SELECT CG.Id,CG.LogoFileName,CG.DocumentFolderName, CG.Name
                            FROM CompanyGroup AS CG
                            INNER JOIN Company AS C ON C.CompanyGroupId=CG.Id
                            INNER JOIN Employee AS EMP ON EMP.CompanyId=C.Id
                            WHERE EMP.Id ='" + employeeId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public Dictionary<string, object> GetDocumentFolder(string id)
        {
            try
            {
                var sql = @"SELECT CG.Id,CG.LogoFileName,CG.DocumentFolderName, CG.Name
                            FROM CompanyGroup AS CG
                            INNER JOIN Company AS C ON C.CompanyGroupId=CG.Id
                            INNER JOIN Employee AS EMP ON EMP.CompanyId=C.Id
                            INNER JOIN DocumentActivity AS DA ON DA.EmployeeId=EMP.Id
                            WHERE DA.Id ='" + id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeListByCompanyGroup(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT EMP.Id
	                                  ,EMP.Name
	                                  ,EMP.Code
	                                  ,EMP.CompanyId
	                                  ,EMP.InitialPIN
	                                  ,EMP.Email
                                      ,'' AS SendingTimes
	                                  ,CAST(0 AS BIT) AS Flag
                                FROM Employee AS EMP
                                INNER JOIN Company AS C ON C.Id=EMP.CompanyId
                                INNER JOIN CompanyGroup AS CG ON CG.Id=C.CompanyGroupId
                                WHERE CG.Id='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public GridModel GetEmployeeByCompanyGroup(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"Select  E.Id
                                              ,E.CompanyId
                                              ,E.Code
                                              ,E.ReportingOfficerId
                                              ,E.Name
                                              ,E.FirstName
                                              ,E.LastName
                                              ,E.FatherName
                                              ,E.MotherName
                                              ,REPLACE(CONVERT(CHAR(11), E.DOB, 106),' ','-') DOB,REPLACE(CONVERT(CHAR(11), E.DOJ, 106),' ','-') DOJ
                                              ,REPLACE(CONVERT(CHAR(11), E.BirthdayCelebrationDate, 106),' ','-') BirthdayCelebrationDate
                                              ,E.SalutationId
                                              ,E.InitialPIN
                                              ,E.IsFirstlogin
                                              ,E.NewPIN
                                              ,E.Mobile
                                              ,E.Email
                                              ,E.Col1,E.Col2,E.Col3,E.Col4,E.Col5,E.Col6,E.Col7,E.Col8,E.Col9,E.Col10
											  ,E.Col11,E.Col12,E.Col13,E.Col14,E.Col15,E.Col16,E.Col17,E.Col18,E.Col19,E.Col20
                                              ,E.Submit
                                              ,E.AccessUser
                                              ,CG.Id AS CompanyGroupId,CG.LogoFileName,CG.DocumentFolderName, CG.Name AS GroupName,
											   C.MobileLength CompanyMobileLength
                                    From dbo.Employee E
                                INNER JOIN Company AS C ON C.Id=E.CompanyId
                                INNER JOIN CompanyGroup AS CG ON CG.Id=C.CompanyGroupId
                                WHERE CG.Id='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public GridModel GetEmployeeDataForRestriction(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"Select  E.Id
                                            ,E.CompanyId
	                                        ,C.Name CompanyName
                                            ,E.Code
                                            ,E.Name
                                            ,E.Mobile
                                            ,E.Email
                                            ,[Status]=CASE  WHEN E.IsFirstLogin=0 THEN 'Not Logged in'WHEN (E.IsFirstLogin=1 and E.Submit=0) THEN 'Not Submitted' ELSE 'Submitted' END
                                            ,AccessRestricted=CASE WHEN E.IsAccessRestricted=0 THEN 'No' ELSE 'Yes' END
                                            ,CAST(E.IsAccessRestricted AS BIT) AS Flag
                                        From dbo.Employee E
                                        INNER JOIN Company AS C ON C.Id=E.CompanyId
                                        INNER JOIN CompanyGroup AS CG ON CG.Id=C.CompanyGroupId
                                WHERE CG.Id='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public GridModel GetEmployeeByCompanyGroupAndSubmit(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"Select  E.Id
                                              ,E.CompanyId
                                              ,E.Code
                                              ,E.ReportingOfficerId
                                              ,E.Name
                                              ,E.FirstName
                                              ,E.LastName
                                              ,E.FatherName
                                              ,E.MotherName
                                              ,REPLACE(CONVERT(CHAR(11), E.DOB, 106),' ','-') DOB,REPLACE(CONVERT(CHAR(11), E.DOJ, 106),' ','-') DOJ
                                              ,REPLACE(CONVERT(CHAR(11), E.BirthdayCelebrationDate, 106),' ','-') BirthdayCelebrationDate
                                              ,E.SalutationId
                                              ,E.InitialPIN
                                              ,E.IsFirstlogin
                                              ,E.NewPIN
                                              ,E.Mobile
                                              ,E.Email
                                              ,E.Col1,E.Col2,E.Col3,E.Col4,E.Col5,E.Col6,E.Col7,E.Col8,E.Col9,E.Col10
											  ,E.Col11,E.Col12,E.Col13,E.Col14,E.Col15,E.Col16,E.Col17,E.Col18,E.Col19,E.Col20
                                              ,E.Submit
                                              ,E.AccessUser
                                              ,CG.Id AS CompanyGroupId,CG.LogoFileName,CG.DocumentFolderName, CG.Name AS GroupName,
                                       	   C.MobileLength CompanyMobileLength
                                       From dbo.Employee E
                                       INNER JOIN Company AS C ON C.Id=E.CompanyId
                                       INNER JOIN CompanyGroup AS CG ON CG.Id=C.CompanyGroupId
                                       WHERE CG.Id='" + companyGroupId + "' AND E.Submit=1";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public Dictionary<string, object> QueryEmployeeAccess(string id, string initialpin)
        {
            try
            {
                var companyGroupData = _companyGroupRepository.Query(t => t.UserId == id && t.Password == initialpin).Select().FirstOrDefault();
                var comDbData = new Dictionary<string, object>();
                if (companyGroupData != null)
                {
                    comDbData.Add("Id", null);
                    comDbData.Add("Name", companyGroupData.UserId);
                    comDbData.Add("CompanyGroupId", companyGroupData.Id);
                    comDbData.Add("LogoFileName", companyGroupData.LogoFileName);
                    return comDbData;
                }
                var data = Find(id);
                if (data == null)
                    throw new CustomException("Invalid employee id");
                if (data.IsFirstlogin)
                {
                    if (data.NewPIN != initialpin) throw new CustomException("Invalid pin");
                }
                else throw new CustomException("Please login first employee portal.");
                if (data.IsAccessRestricted)
                    throw new CustomException("Your access has been restricted.");
                if (!data.AccessUser)
                    throw new CustomException("Your have no access in this portal.");
                var _sql = @"Select E.Id, E.Name, CG.Id AS CompanyGroupId,CG.LogoFileName,CG.DocumentFolderName, CG.Name AS GroupName
								   , C.MobileLength CompanyMobileLength
                            From dbo.Employee E
                            INNER JOIN Company AS C ON C.Id=E.CompanyId INNER JOIN CompanyGroup AS CG ON CG.Id=C.CompanyGroupId
                            LEFT OUTER JOIN dbo.Salutation S ON E.SalutationId= S.Id LEFT OUTER JOIN dbo.Employee EMP ON EMP.Id= E.ReportingOfficerId
                            Where E.Id='" + id + "'";
                return _sqlRepository.GetData(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Operation

        public IWorkbook EmployeeInfo(ReportParam status)
        {
            try
            {
                ReportEmployeeInfo obj = new ReportEmployeeInfo(_sqlRepository);
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

        public IWorkbook ActivityInfo(ReportParam status, string fromdate, string todate)
        {
            try
            {
                ReportEmployeeInfo obj = new ReportEmployeeInfo(_sqlRepository);
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IWorkbook workbook = obj.ActivityInfo(excelEngine, status, fromdate, todate);
                    return workbook;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook ExceptionInfo(ReportParam status)
        {
            try
            {
                ReportEmployeeInfo obj = new ReportEmployeeInfo(_sqlRepository);
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IWorkbook workbook = obj.ExceptionDocKpi(excelEngine, status);
                    return workbook;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook IndividualInfo(ReportParam status)
        {
            try
            {
                ReportEmployeeInfo obj = new ReportEmployeeInfo(_sqlRepository);
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IWorkbook workbook = obj.IndividualDocKpi(excelEngine, status);
                    return workbook;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<ActivityEmp> GetActivityList(string employeeId)
        {
            try
            {
                string sql = @"SELECT * FROM dbo.ActivityEmp AS A Where A.EmployeeId='" + employeeId + "'";
                return _employeeRepository.SqlQuery<ActivityEmp>(sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<DocumentActivity> GetDocumentActivityList(string employeeId)
        {
            try
            {
                string sql = @"SELECT * FROM dbo.DocumentActivity AS D Where D.EmployeeId='" + employeeId + "'";
                return _employeeRepository.SqlQuery<DocumentActivity>(sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<KPI> GetKPIList(string employeeId)
        {
            try
            {
                string sql = @"SELECT * FROM dbo.KPI AS K Where K.EmployeeId='" + employeeId + "'";
                return _employeeRepository.SqlQuery<KPI>(sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}