using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Attendances { 
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

    public class AttendanceReprocessService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public AttendanceReprocessService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }
        public object getFilters()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var str = @"select ID AS PlantId,UserName as PlantName from org.Plant where CompanyId='" + identity.CompanyId+@"'
                and Active='1'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ProcessData(string From, string To,string Plant)
        {
            try {

                NewAttendanceProcessService app = new NewAttendanceProcessService();

                TimeSpan ts = Convert.ToDateTime(To).Subtract(Convert.ToDateTime(From));
                if (ts.Days >= 0)
                {
                    #region PlantLock Checking
                    
                    DataSet PlantLock;
                    PlantLockCheck(From, To, out PlantLock, Plant);
                    string pl = "",Name="";
                    if (PlantLock.Tables[0].Rows.Count > 0)
                    {
                        for (var i = 0; i < PlantLock.Tables[0].Rows.Count; i++)
                        {
                            pl = pl + " " + PlantLock.Tables[0].Rows[i]["LockedDate"].ToString() + ", ";
                            Name = Name + " " + PlantLock.Tables[0].Rows[i]["PlantId"].ToString() + ", ";
                        }

                        throw new Exception("The "+Name+" is Locked for - " + pl);
                    }
                    #endregion

                    #region Data Nullifying
                   
                    string sql = "";

                    sql = @"update attdnrawdata set processedflag=0 where pdate>='" + From + "' " +
                    "and pdate<='" + To + "'and plantid In ("+Plant+")";

                    sql += Environment.NewLine + @"update attdnprocessdata set punchintime=null,punchouttime=null,outpunchlimit=null,
                    intime=null,outtime=null,ProcessIntime=null,ProcessOuttime=null where WorkDate>='" + From + "'" +
                    " and WorkDate <='" + To + "' and plantid In (" + Plant + ")";

                    app.UpdateStatus(sql);

                    #endregion

                    #region ReProcessing 
                    if (From != "" && To != "")
                    {
                        DateTime frmdate = Convert.ToDateTime(From);
                        DateTime Todat = Convert.ToDateTime(To);

                        int days = 0;
                        while (frmdate.AddDays(days) <= Todat)
                        {
                            string[] PlantList = Plant.Split(',');

                            foreach (string item in PlantList)
                            {
                                string PlantId = item.ToString();

                                app.AttndProcess(frmdate.AddDays(days).ToString("yyyy-MM-dd"), PlantId);

                                app.DayStatus(frmdate.AddDays(days).ToString("yyyy-MM-dd"), PlantId);

                            }
                            days += 1;
                        }
                    }
                    #endregion
                }
                else
                {
                    throw new Exception("Please choose a valid Date Range !!");
                }

            }
            catch(Exception ex)
            {
                throw ex;
            }            
        }

        public void PlantLockCheck(string FDate, string TDate, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string From = Convert.ToDateTime(FDate).ToString("dd-MMM-yyyy");
                string To = Convert.ToDateTime(TDate).ToString("dd-MMM-yyyy");

                var sql = @"select * from PlantWiseAttendanceLock where PlantId in("+Plant+")" +
                    "and LockedDate between '" + From + "' and '" + To + "' and IsActive='1'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }

    public class MultipleEmployeeLockService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public MultipleEmployeeLockService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }


        public IEnumerable<object> GetData(string From, string To)
        {
            try
            {
                TimeSpan ts = Convert.ToDateTime(To).Subtract(Convert.ToDateTime(From));
                if (ts.Days >= 0)
                {
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    string sql = @"Select 0 CheckBoxSelect, EI.SystemID,EI.EmployeeCode
                                ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,ld.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation,SE.UserName Section,SuS.UserName SubSection,1 IsSeparatedPart,0 MLVPart, EC.UserName EmpCategoryName  
								FROM  EmployeeInformation EI 
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
                                
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId 
                                LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                                LEFT JOIN [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                                LEFT JOIN [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                                LEFT JOIN HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                                LEFT JOIN ORG.Section AS Se ON Se.Id = EI.SectionID
                                LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = EI.SubSectionID
                                LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId                                        
								WHERE EI.EmployeeStatus='separated'   AND EI.DOS BETWEEN '" + From + @"' AND '" + To + @"'
                                AND  EI.PlantId='" + identity.PlantId + @"'
                                ORDER BY CONVERT(DATE,EI.DOS) ";

                    return _sqlRepository.GetDataCollection(sql, null);
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

      

    }

}
