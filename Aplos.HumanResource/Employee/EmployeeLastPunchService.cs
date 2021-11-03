using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.EmployeeServices;
using bplib;
using Newtonsoft.Json;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Core;

namespace Library.HumanResource.Employee
{
    public class EmployeeLastPunchService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public EmployeeLastPunchService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> GetData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var CompanyId = identity.CompanyId;

                var sql = @"select SystemId as EmpId,EmployeeName,EmployeeCode,Format(DOJ,'dd-MMM-yyyy')DOJ,
                TenureMonth=DATEDIFF(month,FORMAT(DOJ,'dd-MMM-yyyy'), FORMAT(GETDATE(),'dd-MMM-yyyy')),EmployeeStatus,
                EmployeeCurrentStatus,LastPunch.InTime as LastIn,FORMAT(LastPunch.WorkDate,'dd-MMM-yyyy') as LastWorkDate,s.UserName as Section,ss.UserName as SubSection,d.UserName as
                Department,l.UserName as Designation,px.UserName as Plant from
                (
                select * from (
                select dense_rank() over (partition by empsystemid order by workdate desc) as Rnk1,a.InTime,
                a.EmpSystemID,a.WorkDate
                from AttdnProcessData a where (isnull(InTime,'') !='' or isnull(OutTime,'')!='')
                )as LastPunch where rnk1=1)LastPunch 
                left join EmployeeInformation e on e.systemid=lastpunch.empsystemid
                left join org.Plant px on px.Id=e.PlantId
                left join org.Section s on s.Id=e.SectionId
                left join org.SubSection ss on ss.Id=e.SubSectionId
                left join org.Department d on d.Id=e.DepartmentId
                left join hkp.LegalDesignation l on l.Id=e.LegalDesignationId
                where EmpType <> 'Guest' 
                and e.EmployeeStatus='Active'
                AND e.CompanyId='" + CompanyId + "'";             
                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetReportData(string EmpId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var CompanyId = identity.CompanyId;

                var sql = @"select SystemId as EmpId,EmployeeName,EmployeeCode,Format(DOJ,'dd-MMM-yyyy')DOJ,
                TenureMonth=DATEDIFF(month,FORMAT(DOJ,'dd-MMM-yyyy'), FORMAT(GETDATE(),'dd-MMM-yyyy')),EmployeeStatus,
                EmployeeCurrentStatus,LastPunch.InTime as LastIn,FORMAT(LastPunch.WorkDate,'dd-MMM-yyyy') as LastWorkDate,s.UserName as Section,ss.UserName as SubSection,d.UserName as
                Department,l.UserName as Designation,px.UserName as Plant from
                (
                select * from (
                select dense_rank() over (partition by empsystemid order by workdate desc) as Rnk1,a.InTime,
                a.EmpSystemID,a.WorkDate
                from AttdnProcessData a where (isnull(InTime,'') !='' or isnull(OutTime,'')!='')
                )as LastPunch where rnk1=1)LastPunch 
                left join EmployeeInformation e on e.systemid=lastpunch.empsystemid
                left join org.Plant px on px.Id=e.PlantId              
                left join org.Section s on s.Id=e.SectionId
                left join org.SubSection ss on ss.Id=e.SubSectionId
                left join org.Department d on d.Id=e.DepartmentId
                left join hkp.LegalDesignation l on l.Id=e.LegalDesignationId
                where EmpType <> 'Guest' 
                and e.EmployeeStatus='Active'
                AND e.CompanyId='" + CompanyId + "' and isnull(e.SystemId, '') IN(" + EmpId + @")";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }


}

