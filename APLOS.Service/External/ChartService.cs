#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.External;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.External
{
    public class ChartService : Service<Employee>, IChartService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ChartService(
            IRepositoryAsync<Employee> chartRepository
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
          ) : base(chartRepository, unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private ChartColumnList ColList = new ChartColumnList();

        #region DynamicDetailDynamicFunction

        public IEnumerable<object> GetDetailList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid)
        {
            string cList = string.Empty;
            string ocList = string.Empty;
            string wc = string.Empty;
            string fList = string.Empty;
            string nfList = string.Empty;
            string sList = string.Empty;
            string nsList = string.Empty;
            string cn = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        if (item.Sequence <= seq)
                        {
                            cList += ",e." + item.ColumnName;
                            ocList += "e." + item.ColumnName;
                            fList += " and f." + item.ColumnName + "=" + "e." + item.ColumnName;
                            nfList += " and nf." + item.ColumnName + "=" + "e." + item.ColumnName;
                            sList += " and s." + item.ColumnName + "=" + "e." + item.ColumnName;
                            nsList += " and ns." + item.ColumnName + "=" + "e." + item.ColumnName;
                        }
                    }

                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = " and c.id=" + item.Id;
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and e." + item.ColumnName + "='" + item.Text + "'";
                            }
                        }
                    }
                }

                string str = @"select cg.id,c.id,cg.Name  GroupName,c.Name CompanyName
								" + cList + @" UserName
								,ISNULL(f.Firstlogin, 0) Firstlogin
								,ISNULL(nf.NotLoggedin, 0) NotLoggedIn
								,ISNULL(s.Submitted, 0) Submitted
								,ISNULL(ns.notSubmitted, 0) NotSubmitted
								,ISNULL(e.totalEmployee, 0) totalEmployee

								from
								(
								(SELECT Count(Id) totalEmployee " + cList + @", CompanyId FROM Employee e group  by CompanyId " + cList + @") e
								left outer join
								(SELECT Count(id) Firstlogin " + cList + @",  CompanyId FROM Employee e WHERE IsFirstlogin = 1 group  by CompanyId " + cList + @")
								f on f.CompanyId = e.CompanyId " + fList + @"
								left outer join
								(SELECT Count(id) NotLoggedin " + cList + @", CompanyId FROM Employee e WHERE IsFirstlogin = 0  group  by CompanyId " + cList + @")
								nf on nf.CompanyId = e.CompanyId " + nfList + @"
								left outer join
								(SELECT Count(id) Submitted " + cList + @", CompanyId  FROM Employee e WHERE Submit = 1 and IsFirstLogin = 1 group  by CompanyId " + cList + @")
								s on s.CompanyId = e.CompanyId " + sList + @"
								left outer join
								(SELECT Count(id) notSubmitted " + cList + @",  CompanyId FROM Employee e WHERE Submit = 0  and IsFirstlogin = 1 group  by CompanyId " + cList + @")
								ns on ns.CompanyId = e.CompanyId " + nsList + @"
								 )
								left outer join dbo.Company c on c.id = e.CompanyId
								left outer join dbo.CompanyGroup cg on cg.id = c.CompanyGroupId
                               where  cg.id = '" + cgid + "' " + wc + @" order by  UserName";
                return _sqlRepository.GetDataCollection(str, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion DynamicDetailDynamicFunction

        #region GroupWiseColumnList

        public IEnumerable<object> GetGroupWiseColumnList(string companyGroupId)
        {
            try
            {
                var sql = @"select
                             t.AplosEmpFieldId,
                             t.ColumnName
                             ,t.ClinetColumnName
                             ,f.AplosColumnName,t.[Sequence], '' Text, ''Id
                             from
                             [dbo].[AplosEmpFieldTag] t
                             left outer join [AplosEmpField] f on f.Id=t.AplosEmpFieldId
                             where t.CompanyGroupId='" + companyGroupId + "' and t.IsAplicable=1";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion GroupWiseColumnList

        #region GroupWiseCompanyList

        public IEnumerable<object> GetGroupWiseCList(string companyGroupId)
        {
            try
            {
                var sql = @"select cg.Id CompanyGroupId,c.Id CompanyId
                                ,cg.Name as GroupName
                                ,c.Name UserName
                                ,e.totalEmployee totalEmployee
                                ,ISNULL(f.Firstlogin, 0) Firstlogin
                                ,ISNULL(ff.NotLoggedin, 0) NotLoggedIn
                                ,ISNULL(s.Submitted, 0) Submitted
                                ,ISNULL(ns.notSubmitted, 0) NotSubmitted
                                ,ISNULL(e.totalEmployee, 0) totalEmployee
                                 from
                                 (
                                 (SELECT Count(Id) totalEmployee, CompanyId FROM Employee group  by  CompanyId) e
                                left outer join
                                (SELECT Count(id) Firstlogin, CompanyId FROM Employee WHERE IsFirstlogin = 1 group by CompanyId)
                                 f on f.CompanyId = e.CompanyId
                                 left outer join
                                 (SELECT Count(id) NotLoggedin, CompanyId FROM Employee WHERE IsFirstlogin = 0 group  by  CompanyId)
                                 ff on ff.CompanyId = e.CompanyId
                                 left outer join
                                 (SELECT Count(id) Submitted, CompanyId  FROM Employee WHERE Submit = 1 and IsFirstLogin = 1 group  by CompanyId)
                                 s on s.CompanyId = e.CompanyId
                                 left outer join
                                 (SELECT Count(id) notSubmitted, CompanyId FROM Employee WHERE Submit = 0  and IsFirstlogin = 1 group  by  CompanyId)
                                 ns on ns.CompanyId = e.CompanyId
                                  )
                                 left outer join dbo.Company c on c.id = e.CompanyId
                                 left outer join dbo.CompanyGroup cg on cg.id = c.CompanyGroupId where c.CompanyGroupId = '" + companyGroupId + @"' ";// where c.CompanyGroupId = '" + companyGroupId + @"'

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion GroupWiseCompanyList

        #region Modal Function for Not Logged in Employee List

        public IEnumerable<object> NotLoggedInEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid)
        {
            string cList = string.Empty;
            string ocList = string.Empty;
            string wc = string.Empty;
            string fList = string.Empty;
            string nfList = string.Empty;
            string sList = string.Empty;
            string nsList = string.Empty;
            string cn = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        cList += ",e." + item.ColumnName;
                    }

                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = " and c.id=" + item.Id;
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and e." + item.ColumnName + "='" + item.Text + "'";
                            }
                        }
                    }
                }

                var sql = @"select
                                cg.id,
                                c.id,
                                c.Name CompanyName
                               ,e.Name  EmpName
                               ,e.Code EmpCode
	                           ,isnull(act.Activity,0) Activity
                                " + cList + @"

                            from

                            (SELECT id,Name,Code   " + cList + @", CompanyId FROM Employee e WHERE IsFirstlogin = 0 group  by id,Name,Code   " + cList + @",CompanyId) e
                      left outer join dbo.Company c on c.id = e.CompanyId
	                  left outer join dbo.CompanyGroup cg on cg.id = c.CompanyGroupId
                      left outer join
                      (
                      select
                      count(Id) Activity,e.EmployeeId
                      from [dbo].ActivityEmp e
                      group by e.EmployeeId
                      ) act on e.Id=act.EmployeeId
		             where  cg.id = '" + cgid + "' " + wc + @"
                      order by e.Name ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Modal Function for Not Logged in Employee List

        #region Modal Functions for CompanyGroup wise Submitted Employee List

        public IEnumerable<object> SubmittedEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid)

        {
            string cList = string.Empty;
            string ocList = string.Empty;
            string wc = string.Empty;
            string fList = string.Empty;
            string nfList = string.Empty;
            string sList = string.Empty;
            string nsList = string.Empty;
            string cn = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        cList += ",e." + item.ColumnName;
                    }

                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = " and c.id=" + item.Id;
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and e." + item.ColumnName + "='" + item.Text + "'";
                            }
                        }
                    }
                }

                var sql = @"select
                                cg.id,
                                c.id,
                                c.Name CompanyName
                               ,e.Name  EmpName
                               ,e.Code EmpCode
	                           ,isnull(act.Activity,0) Activity
                                " + cList + @"

                            from

                            (SELECT id,Name,Code   " + cList + @", CompanyId FROM Employee e WHERE IsFirstlogin = 1 and Submit = 1  group  by id,Name,Code   " + cList + @",CompanyId) e
                      left outer join dbo.Company c on c.id = e.CompanyId
	                  left outer join dbo.CompanyGroup cg on cg.id = c.CompanyGroupId
                      left outer join
                      (
                      select
                      count(Id) Activity,e.EmployeeId
                      from [dbo].ActivityEmp e
                      group by e.EmployeeId
                      ) act on e.Id=act.EmployeeId
		             where  cg.id = '" + cgid + "' " + wc + @"
                      order by e.Name ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Modal Functions for CompanyGroup wise Submitted Employee List

        #region Modal Functions for Not Submitted Employee List

        public IEnumerable<object> NotSubmittedEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid)

        {
            string cList = string.Empty;
            string ocList = string.Empty;
            string wc = string.Empty;
            string fList = string.Empty;
            string nfList = string.Empty;
            string sList = string.Empty;
            string nsList = string.Empty;
            string cn = string.Empty;
            try
            {
                seq += 1;
                foreach (var item in ChartColumnList)
                {
                    if (item.Sequence != -2 && item.Sequence != -1)
                    {
                        cList += ",e." + item.ColumnName;
                    }

                    if (item.Sequence != -2)
                    {
                        if (item.Sequence == -1)
                        {
                            wc = " and c.id=" + item.Id;
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and e." + item.ColumnName + "='" + item.Text + "'";
                            }
                        }
                    }
                }

                var sql = @"select
                                cg.id,
                                c.id,
                                c.Name CompanyName
                               ,e.Name  EmpName
                               ,e.Code EmpCode
	                           ,isnull(act.Activity,0) Activity
                                " + cList + @"

                            from

                            (SELECT id,Name,Code   " + cList + @", CompanyId FROM Employee e WHERE IsFirstlogin = 1 and Submit = 0  group  by id,Name,Code   " + cList + @",CompanyId) e
                      left outer join dbo.Company c on c.id = e.CompanyId
	                  left outer join dbo.CompanyGroup cg on cg.id = c.CompanyGroupId
                      left outer join
                      (
                      select
                      count(Id) Activity,e.EmployeeId
                      from [dbo].ActivityEmp e
                      group by e.EmployeeId
                      ) act on e.Id=act.EmployeeId
		             where  cg.id = '" + cgid + "' " + wc + @"
                      order by e.Name ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Modal Functions for Not Submitted Employee List

        #region status Modal Function for  Not Logged in Employee List

        public IEnumerable<object> StNotLoggedInEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid/*,int click*/)
        {
            string cList = string.Empty;
            string ocList = string.Empty;
            string wc = string.Empty;
            string fList = string.Empty;
            string nfList = string.Empty;
            string sList = string.Empty;
            string nsList = string.Empty;
            string cn = string.Empty;
            try
            {
                if (seq == -2)
                {
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            cList += ",e." + item.ColumnName;
                        }
                    }

                    var csql = @"select
                                cg.id,
                                c.id,
                                c.Name CompanyName
                               ,e.Name  EmpName
                               ,e.Code EmpCode
	                           ,isnull(act.Activity,0) Activity
                                " + cList + @"

                            from

                            (SELECT id,Name,Code   " + cList + @", CompanyId FROM Employee e WHERE IsFirstlogin = 0 group  by id,Name,Code   " + cList + @",CompanyId) e
                      left outer join dbo.Company c on c.id = e.CompanyId
	                  left outer join dbo.CompanyGroup cg on cg.id = c.CompanyGroupId
                      left outer join
                      (
                      select
                      count(Id) Activity,e.EmployeeId
                      from [dbo].ActivityEmp e
                      group by e.EmployeeId
                      ) act on e.Id=act.EmployeeId
		             where  cg.id = '" + cgid + "' " + wc + @"
                      order by e.Name ";

                    return _sqlRepository.GetDataCollection(csql, null);
                }
                else
                {
                    seq += 1;
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            cList += ",e." + item.ColumnName;
                        }

                        if (item.Sequence == -1)
                        {
                            wc = " and c.id=" + item.Id;
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and e." + item.ColumnName + "='" + item.Text + "'";
                            }
                        }
                    }// != -2

                    var sql = @"select
                                cg.id,
                                c.id,
                                c.Name CompanyName
                               ,e.Name  EmpName
                               ,e.Code EmpCode
	                           ,isnull(act.Activity,0) Activity
                                " + cList + @"

                            from

                            (SELECT id,Name,Code   " + cList + @", CompanyId FROM Employee e WHERE IsFirstlogin = 0 group  by id,Name,Code   " + cList + @",CompanyId) e
                      left outer join dbo.Company c on c.id = e.CompanyId
	                  left outer join dbo.CompanyGroup cg on cg.id = c.CompanyGroupId
                      left outer join
                      (
                      select
                      count(Id) Activity,e.EmployeeId
                      from [dbo].ActivityEmp e
                      group by e.EmployeeId
                      ) act on e.Id=act.EmployeeId
		             where  cg.id = '" + cgid + "' " + wc + @"
                      order by e.Name ";

                    return _sqlRepository.GetDataCollection(sql, null);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion status Modal Function for  Not Logged in Employee List

        #region status Modal Function for  Submitted  Employee List

        public IEnumerable<object> StSubmittedEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid)
        {
            string cList = string.Empty;
            string ocList = string.Empty;
            string wc = string.Empty;
            string fList = string.Empty;
            string nfList = string.Empty;
            string sList = string.Empty;
            string nsList = string.Empty;
            string cn = string.Empty;
            try
            {
                if (seq == -2)
                {
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            cList += ",e." + item.ColumnName;
                        }
                    }

                    var csql = @"select
                                cg.id,
                                c.id,
                                c.Name CompanyName
                               ,e.Name  EmpName
                               ,e.Code EmpCode
	                           ,isnull(act.Activity,0) Activity
                                " + cList + @"

                            from

                            (SELECT id,Name,Code   " + cList + @", CompanyId FROM Employee e WHERE IsFirstlogin = 1 and Submit = 1 group  by id,Name,Code   " + cList + @",CompanyId) e
                      left outer join dbo.Company c on c.id = e.CompanyId
	                  left outer join dbo.CompanyGroup cg on cg.id = c.CompanyGroupId
                      left outer join
                      (
                      select
                      count(Id) Activity,e.EmployeeId
                      from [dbo].ActivityEmp e
                      group by e.EmployeeId
                      ) act on e.Id=act.EmployeeId
		             where  cg.id = '" + cgid + "' " + wc + @"
                      order by e.Name ";

                    return _sqlRepository.GetDataCollection(csql, null);
                }
                else
                {
                    seq += 1;
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            cList += ",e." + item.ColumnName;
                        }

                        if (item.Sequence == -1)
                        {
                            wc = " and c.id=" + item.Id;
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and e." + item.ColumnName + "='" + item.Text + "'";
                            }
                        }
                    }

                    var sql = @"select
                                cg.id,
                                c.id,
                                c.Name CompanyName
                               ,e.Name  EmpName
                               ,e.Code EmpCode
	                           ,isnull(act.Activity,0) Activity
                                " + cList + @"

                            from

                            (SELECT id,Name,Code   " + cList + @", CompanyId FROM Employee e WHERE IsFirstlogin = 1 and Submit = 1 group  by id,Name,Code   " + cList + @",CompanyId) e
                      left outer join dbo.Company c on c.id = e.CompanyId
	                  left outer join dbo.CompanyGroup cg on cg.id = c.CompanyGroupId
                      left outer join
                      (
                      select
                      count(Id) Activity,e.EmployeeId
                      from [dbo].ActivityEmp e
                      group by e.EmployeeId
                      ) act on e.Id=act.EmployeeId
		             where  cg.id = '" + cgid + "' " + wc + @"
                      order by e.Name ";

                    return _sqlRepository.GetDataCollection(sql, null);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion status Modal Function for  Submitted  Employee List

        #region status Modal Function for  Submitted  Employee List

        public IEnumerable<object> StNotSubmittedEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid)
        {
            string cList = string.Empty;
            string ocList = string.Empty;
            string wc = string.Empty;
            string fList = string.Empty;
            string nfList = string.Empty;
            string sList = string.Empty;
            string nsList = string.Empty;
            string cn = string.Empty;
            try
            {
                if (seq == -2)
                {
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            cList += ",e." + item.ColumnName;
                        }
                    }

                    var csql = @"select
                                cg.id,
                                c.id,
                                c.Name CompanyName
                               ,e.Name  EmpName
                               ,e.Code EmpCode
	                           ,isnull(act.Activity,0) Activity
                                " + cList + @"

                            from

                            (SELECT id,Name,Code   " + cList + @", CompanyId FROM Employee e WHERE IsFirstlogin = 1 and Submit = 0 group  by id,Name,Code   " + cList + @",CompanyId) e
                      left outer join dbo.Company c on c.id = e.CompanyId
	                  left outer join dbo.CompanyGroup cg on cg.id = c.CompanyGroupId
                      left outer join
                      (
                      select
                      count(Id) Activity,e.EmployeeId
                      from [dbo].ActivityEmp e
                      group by e.EmployeeId
                      ) act on e.Id=act.EmployeeId
		             where  cg.id = '" + cgid + "' " + wc + @"
                      order by e.Name ";

                    return _sqlRepository.GetDataCollection(csql, null);
                }
                else
                {
                    seq += 1;
                    foreach (var item in ChartColumnList)
                    {
                        if (item.Sequence != -2 && item.Sequence != -1)
                        {
                            cList += ",e." + item.ColumnName;
                        }

                        if (item.Sequence == -1)
                        {
                            wc = " and c.id=" + item.Id;
                        }
                        else
                        {
                            if (item.Sequence < seq)
                            {
                                wc += " and e." + item.ColumnName + "='" + item.Text + "'";
                            }
                        }
                    }// != -2

                    var sql = @"select
                                cg.id,
                                c.id,
                                c.Name CompanyName
                               ,e.Name  EmpName
                               ,e.Code EmpCode
	                           ,isnull(act.Activity,0) Activity
                                " + cList + @"

                            from

                            (SELECT id,Name,Code   " + cList + @", CompanyId FROM Employee e WHERE IsFirstlogin = 1 and Submit = 0 group  by id,Name,Code   " + cList + @",CompanyId) e
                      left outer join dbo.Company c on c.id = e.CompanyId
	                  left outer join dbo.CompanyGroup cg on cg.id = c.CompanyGroupId
                      left outer join
                      (
                      select
                      count(Id) Activity,e.EmployeeId
                      from [dbo].ActivityEmp e
                      group by e.EmployeeId
                      ) act on e.Id=act.EmployeeId
		             where  cg.id = '" + cgid + "' " + wc + @"
                      order by e.Name ";

                    return _sqlRepository.GetDataCollection(sql, null);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion status Modal Function for  Submitted  Employee List

        #region totalActivity

        public IEnumerable<object> TotalActivity(string companyGroupId)
        {
            try
            {
                var sql = @"SELECT count(a.Id) totalActivity,Replace(CONVERT(VARCHAR(11), a.AddedDateTime, 6), ' ', '-') AddedDateTime
                            FROM ActivityEmp a
                            left outer join Employee e on a.EmployeeId = e.Id
                            left outer join Company c on e.CompanyId = c.Id
                            left outer join CompanyGroup cg  on c.CompanyGroupId = cg.Id
                            where cg.Id ='" + companyGroupId + @"'

                            group by  Replace(CONVERT(VARCHAR(11),a.AddedDateTime, 6), ' ', '-')   ";// where c.CompanyGroupId = '" + companyGroupId + @"'

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion totalActivity

        #region FirstLoggedIn

        public IEnumerable<object> FirstLoggedIn(string companyGroupId)
        {
            try
            {
                var sql = @"SELECT count(e.Id) TFirstLogin,Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 6), ' ', '-') FirstLoginTime
                            FROM Employee e
                            left outer join Company c on e.CompanyId = c.Id
                            left outer join CompanyGroup cg on c.CompanyGroupId = cg.Id
                            where cg.Id = '" + companyGroupId + @"' and IsFirstlogin = 1
                            group by  Replace(CONVERT(VARCHAR(11),e.FirstLoginTime, 6), ' ', '-')    ";// where c.CompanyGroupId = '" + companyGroupId + @"'

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion FirstLoggedIn

        #region Discrete chart function for DayWiseSubmit

        public IEnumerable<object> DayWiseSubmit(string companyGroupId)
        {
            try
            {
                var sql = @" SELECT count(e.Id) totalSubmit,Replace(CONVERT(VARCHAR(11), e.SubmitTime, 6), ' ', '-') SubmitTime
                            FROM Employee e
                            left outer join Company c on e.CompanyId = c.Id
                            left outer join CompanyGroup cg on c.CompanyGroupId = cg.Id
                            where cg.Id = '" + companyGroupId + @"' and submit = 1
                            group by  Replace(CONVERT(VARCHAR(11),e.SubmitTime, 6), ' ', '-')   ";// where c.CompanyGroupId = '" + companyGroupId + @"'

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Discrete chart function for DayWiseSubmit

        #region Chart for Document

        public IEnumerable<object> TotalDocument(string companyGroupId)
        {
            try
            {
                var sql = @"SELECT count(d.Id) totalDocument,Replace(CONVERT(VARCHAR(11), d.AddedDateTime, 6), ' ', '-') DAddedDate
                            FROM DocumentActivity d
                            left outer join Employee e on d.EmployeeId = e.Id
                            left outer join Company c on e.CompanyId = c.Id
                            left outer join CompanyGroup  on c.CompanyGroupId = CompanyGroup.Id
                            where CompanyGroup.Id ='" + companyGroupId + @"'

                            group by  Replace(CONVERT(VARCHAR(11),d.AddedDateTime, 6), ' ', '-') ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Chart for Document
    }
}