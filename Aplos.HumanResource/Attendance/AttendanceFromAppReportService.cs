using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Service.Attendances { 
    public class AttendanceFromAppReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        
        public AttendanceFromAppReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }


        public IEnumerable<object> GetAttndData(string From, string To, string AttndType)
        {
            try
            {



                TimeSpan ts = Convert.ToDateTime(To).Subtract(Convert.ToDateTime(From));
                if (ts.Days >= 0)
                {
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _sql = "";
                    if (AttndType == "Both")
                    {



                        _sql = @"select p.UserName as Plant,attnd.Id,attnd.PlantId,attnd.EmployeeId,
                    emp.EmployeeCode,emp.EmployeeName,dp.UserName as Department,dp.Id as DepartmentId,u.UserName as Unit,
                    u.Id as UnitId,s.UserName as Section,s.Id AS SectionId, ss.UserName as SubSection,ss.Id as SubsectionId,
                    ds.UserName as Designation,ds.Id as DesignationId,Format(attnd.PDate,'dd-MMM-yyyy') as Date,
                    FORMAT(attnd.InTime,'hh:mm:ss tt') as InTime,
                    FORMAT(attnd.OutTime,'hh:mm:ss tt') OutTime,attnd.INLocationDesc as InLocation,attnd.OutLocationDesc
                    as OutLocation,attnd.AttndType from dbo.AttdnRawDataFromApp attnd left join org.Plant p on attnd.PlantId=p.Id
                    left join org.Company c on c.Id=p.CompanyId
                    join dbo.EmployeeInformation emp on emp.SystemId=attnd.EmployeeId
                    left join org.Unit u on u.Id=emp.UnitId
                    left join org.Section s on s.Id=emp.SectionId
                    left join org.SubSection ss on ss.Id=emp.SubSectionId
                    left join org.Department dp on dp.Id=emp.DepartmentId
                    left join hkp.Designation ds on ds.Id=emp.DesignationSystemID
                    where (attnd.AttndType='OnDuty' OR attnd.AttndType='WorkFromHome') and attnd.PDate between 
                    '" + From + "' and '" + To + "' and c.Id='" + identity.CompanyId + "' order by attnd.AddedDate desc ";
                    }
                    else
                    {



                        _sql = @"select p.UserName as Plant,attnd.Id,attnd.PlantId,attnd.EmployeeId,emp.EmployeeCode,
                    emp.EmployeeName,dp.UserName as Department,dp.Id as DepartmentId,u.UserName as Unit,u.Id as UnitId,
                    s.UserName as Section,s.Id AS SectionId, ss.UserName as SubSection,ss.Id as SubsectionId,ds.UserName as Designation,
                    ds.Id as DesignationId,Format(attnd.PDate,'dd-MMM-yyyy') as Date,FORMAT(attnd.InTime,'hh:mm:ss tt') as InTime,
                    FORMAT(attnd.OutTime,'hh:mm:ss tt') OutTime,attnd.INLocationDesc as InLocation,attnd.OutLocationDesc as OutLocation,
                    attnd.AttndType from dbo.AttdnRawDataFromApp attnd left join org.Plant p on attnd.PlantId=p.Id
                    left join org.Company c on c.Id=p.CompanyId
                    join dbo.EmployeeInformation emp on emp.SystemId=attnd.EmployeeId
                    left join org.Unit u on u.Id=emp.UnitId
                    left join org.Section s on s.Id=emp.SectionId
                    left join org.SubSection ss on ss.Id=emp.SubSectionId
                    left join org.Department dp on dp.Id=emp.DepartmentId
                    left join hkp.Designation ds on ds.Id=emp.DesignationSystemID
                    where attnd.AttndType='" + AttndType + "' and attnd.PDate" +
                        " between '" + From + "' and '" + To + "' and c.Id='" + identity.CompanyId + "' order by attnd.AddedDate desc ";



                    }




                    return _sqlRepository.GetDataCollection(_sql, null);
                }
                else
                {
                    throw new Exception("Please choose a valid Date !!");
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetReportData(string From, string To, string AttndType,
        string EmpName, string SubId, string PlantId, string SectionId, string DesgId, string UnitId, string DeptId,
        string EmpCode)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = "";
                if (AttndType == "Both")
                {
                    _sql = @"select p.UserName as Plant,attnd.Id,attnd.PlantId,attnd.EmployeeId,
                    emp.EmployeeCode,emp.EmployeeName,dp.UserName as Department,dp.Id as DepartmentId,u.UserName as Unit,
                    u.Id as UnitId,s.UserName as Section,s.Id AS SectionId, ss.UserName as SubSection,ss.Id as SubsectionId,
                    ds.UserName as Designation,ds.Id as DesignationId,Format(attnd.PDate,'dd-MMM-yyyy') as Date,
                    FORMAT(attnd.InTime,'hh:mm:ss tt') as InTime,
                    FORMAT(attnd.OutTime,'hh:mm:ss tt') OutTime,attnd.INLocationDesc as InLocation,attnd.OutLocationDesc
                    as OutLocation,attnd.AttndType,attnd.Remarks as InRemarks,attnd.RemarksOUT as OutRemarks from dbo.AttdnRawDataFromApp attnd left join org.Plant p on attnd.PlantId=p.Id
                    left join org.Company c on c.Id=p.CompanyId
                    join dbo.EmployeeInformation emp on emp.SystemId=attnd.EmployeeId
                    left join org.Unit u on u.Id=emp.UnitId
                    left join org.Section s on s.Id=emp.SectionId
                    left join org.SubSection ss on ss.Id=emp.SubSectionId
                    left join org.Department dp on dp.Id=emp.DepartmentId
                    left join hkp.Designation ds on ds.Id=emp.DesignationSystemID
                    where (attnd.AttndType='OnDuty' OR attnd.AttndType='WorkFromHome') and attnd.PDate between 
                    '" + From + "' and '" + To + "' and c.Id='" + identity.CompanyId + @"'
                          and isnull(p.Id ,'') IN(" + PlantId + @")              
                          AND isnull(s.Id, '') IN(" + SectionId + @")  AND
                          isnull(ss.Id, '') IN(" + SubId + @") AND
                          isnull(dp.Id, '') IN(" + DeptId + @") AND
                          isnull(ds.Id, '') IN(" + DesgId + @") AND
                          isnull(u.Id, '') IN(" + UnitId + @") AND
                          isnull(emp.EmployeeCode, '') IN(" + EmpCode + @") AND
                          isnull(emp.EmployeeName, '') IN(" + EmpName + @")";

                }
                else
                {
                    _sql = @"select p.UserName as Plant,attnd.Id,attnd.PlantId,attnd.EmployeeId,
                    emp.EmployeeCode,emp.EmployeeName,dp.UserName as Department,dp.Id as DepartmentId,u.UserName as Unit,
                    u.Id as UnitId,s.UserName as Section,s.Id AS SectionId, ss.UserName as SubSection,ss.Id as SubsectionId,
                    ds.UserName as Designation,ds.Id as DesignationId,Format(attnd.PDate,'dd-MMM-yyyy') as Date,
                    FORMAT(attnd.InTime,'hh:mm:ss tt') as InTime,
                    FORMAT(attnd.OutTime,'hh:mm:ss tt') OutTime,attnd.INLocationDesc as InLocation,attnd.OutLocationDesc
                    as OutLocation,attnd.AttndType,attnd.Remarks as InRemarks,attnd.RemarksOUT as OutRemarks from dbo.AttdnRawDataFromApp attnd left join org.Plant p on attnd.PlantId=p.Id
                    left join org.Company c on c.Id=p.CompanyId
                    join dbo.EmployeeInformation emp on emp.SystemId=attnd.EmployeeId
                    left join org.Unit u on u.Id=emp.UnitId
                    left join org.Section s on s.Id=emp.SectionId
                    left join org.SubSection ss on ss.Id=emp.SubSectionId
                    left join org.Department dp on dp.Id=emp.DepartmentId
                    left join hkp.Designation ds on ds.Id=emp.DesignationSystemID
                    where attnd.AttndType='" + AttndType+@"' and attnd.PDate between 
                    '" + From + "' and '" + To + "' and c.Id='" + identity.CompanyId + @"'
                          and isnull(p.Id ,'') IN(" + PlantId + @")              
                          AND isnull(s.Id, '') IN(" + SectionId + @")  AND
                          isnull(ss.Id, '') IN(" + SubId + @") AND
                          isnull(dp.Id, '') IN(" + DeptId + @") AND
                          isnull(ds.Id, '') IN(" + DesgId + @") AND
                          isnull(u.Id, '') IN(" + UnitId + @") AND
                          isnull(emp.EmployeeCode, '') IN(" + EmpCode + @") AND
                          isnull(emp.EmployeeName, '') IN(" + EmpName + @")";
                }

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }    
}
