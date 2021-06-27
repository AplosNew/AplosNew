using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Attendances
{
    class clsSandwichAbsent
    {
        public void AutoSaveSandwichAbsent(string FromDate, string ToDate, string CompanyGroupId, string PlantId, string UserName, string IPAddress) {

            try
            {
                List<SandwichAbsentVM> AbsentList = GetEmployeeList(PlantId, FromDate, ToDate);
                SaveAbsent(AbsentList, CompanyGroupId, PlantId, UserName, IPAddress);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<SandwichAbsentVM> GetEmployeeList(string PlantId,string FromDate, string ToDate)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsResult = null;          
            string strSql = string.Empty;



            strSql = @"SELECT 
                       APD.EmpSystemID
                     , FORMAT(APD.WorkDate ,'dd-MMM-yyyy') AttdnProcDate
                    
                    FROM ( select apd.EmpSystemID, apd.WorkDate, apd.DayStatus
	                       FROM AttdnProcessData AS apd
	                       WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' 
                           AND apd.PlantID='" + PlantId + @"'
	                       AND isnull(apd.DayStatus,'') IN (
                                                            ---DayType
                                                            SELECT DayType  FROM DayType WHERE Category='Weekend' AND  1=(SELECT IsSandwichAbsentInWeekend  From  [dbo].[PlantWiseHRMSSetting] where plantid='" + PlantId + @"')
                                                            UNION
                                                            SELECT DayType FROM DayType WHERE Category='Holiday' AND  1=(SELECT IsSandwichAbsentInHoliday From  [dbo].[PlantWiseHRMSSetting] where plantid='" + PlantId + @"')

                                                           ) 
                           AND apd.EmpSystemID NOT IN (select EmpSystemID  FROM [SCS].[WeeklyAbsentismAssignment] WHERE EmpSystemID=apd.EmpSystemID AND WorkingDate=apd.WorkDate)
                    ) AS APD
                    LEFT OUTER JOIN AttdnProcessData AS X ON apd.EmpSystemID=x.EmpSystemID AND isnull(x.WorkDate,'')=(SELECT TOP 1 isnull(WorkDate,'') AS PK 
                                                                                                                      FROM AttdnProcessData WHERE WorkDate<apd.WorkDate 
																								                      AND EmpSystemID=APD.EmpSystemID	AND DayStatus NOT IN (
                                                                                                                                                                                ---DayType
                                                                                                                                                                                SELECT DayType  FROM DayType WHERE Category='Weekend' AND  1=(SELECT IsSandwichAbsentInWeekend  From  [dbo].[PlantWiseHRMSSetting] where plantid='" + PlantId + @"')
                                                                                                                                                                                UNION
                                                                                                                                                                                SELECT DayType FROM DayType WHERE Category='Holiday' AND  1=(SELECT IsSandwichAbsentInHoliday From  [dbo].[PlantWiseHRMSSetting] where plantid='" + PlantId + @"')
                                                                                                                                                                             ) 
                                                                                                                      ORDER BY WorkDate  DESC)
 

                    LEFT OUTER JOIN AttdnProcessData AS Y ON apd.EmpSystemID=Y.EmpSystemID AND isnull(Y.WorkDate,'')=(SELECT TOP 1 isnull(WorkDate,'') AS PK 
                                                                                                                      FROM AttdnProcessData WHERE WorkDate>apd.WorkDate 
																								                      AND EmpSystemID=APD.EmpSystemID	AND DayStatus NOT IN (
                                                                                                                                                                                 ---DayType
                                                                                                                                                                                SELECT DayType  FROM DayType WHERE Category='Weekend' AND  1=(SELECT IsSandwichAbsentInWeekend  From  [dbo].[PlantWiseHRMSSetting] where plantid='" + PlantId + @"')
                                                                                                                                                                                UNION
                                                                                                                                                                                SELECT DayType FROM DayType WHERE Category='Holiday' AND  1=(SELECT IsSandwichAbsentInHoliday From  [dbo].[PlantWiseHRMSSetting] where plantid='" + PlantId + @"')
                                                                                                                                                                              ) 
                                                                                                                      ORDER BY WorkDate  ASC)
                                                                                                  
                                                                                                  
                                                                                                  
                     LEFT JOIN dbo.Employeeinformation EI ON EI.SystemId = apd.EmpSystemID
                     LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                     LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                     LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                     LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                     LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                     LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                     LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                     LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                     LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId							
                     LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId                                                                                                  


                    WHERE ISNULL(x.DayStatus,'')='A' AND ISNULL(Y.DayStatus,'')='A'
  
                    ORDER BY APD.EmpSystemID,APD.WorkDate ";




            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(strSql, out dsResult, false, "1");

            List<SandwichAbsentVM> dicSandwichAbsent = new List<SandwichAbsentVM>();
          
            DataView dvSandwichAbsent = new DataView(dsResult.Tables[0]);
            DataTable dtSandwichAbsent = dvSandwichAbsent.ToTable(true, "EmpSystemID", "WorkingDate");

            if (dtSandwichAbsent.Rows.Count > 0)
                dicSandwichAbsent = dtSandwichAbsent.ToList<SandwichAbsentVM>();
            return dicSandwichAbsent;
        }
        public void SaveAbsent(List<SandwichAbsentVM> AbsentList,string CompanyGroupId,string PlantId, string UserName,string IPAddress)
        {
            string EmpId = string.Empty;
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsWeeklyAbsentismAssignment = null;

            try
            {
                if (AbsentList.Count > 0)
                {


                    string sql = @"SELECT [Id]
                                    ,[CompanyGroupId]
                                    ,[PlantId]
                                    ,[EmpSystemID]
                                    ,[WorkingDate]
                                    ,[AddedBy]
                                    ,[AddedDate]
                                    ,[AddedFromIP]
                                    ,[UpdatedBy]
                                    ,[UpdatedDate]
                                    ,[UpdatedFromIP]
                                     FROM [SCS].[WeeklyAbsentismAssignment] WHERE PlantId = '" + PlantId + "'  ORDER BY EmpSystemID";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsWeeklyAbsentismAssignment, false, "1");


                    for (int i = 0; i < AbsentList.Count; i++)
                    {



                        DataView dvExceptionEmployeeAttendanceUnlock = new DataView(dsWeeklyAbsentismAssignment.Tables[0]);
                        dvExceptionEmployeeAttendanceUnlock.RowFilter = "EmpSystemID='" + AbsentList[i].EmpSystemID.ToString() + "' AND PlantId='" + PlantId + "' AND WorkingDate='" + AbsentList[i].WorkingDate.ToString() + "'";
                        if (dvExceptionEmployeeAttendanceUnlock.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "WeeklyAbsentismAssignment", out sID);
                            DataRow dr = dsWeeklyAbsentismAssignment.Tables[0].NewRow();
                            dr["Id"] = "WA" + sID;
                            dr["EmpSystemID"] = AbsentList[i].EmpSystemID.ToString();
                            dr["CompanyGroupId"] = CompanyGroupId;
                            dr["PlantId"] = PlantId;
                            dr["WorkingDate"] = AbsentList[i].WorkingDate.ToString();
                            dr["AddedBy"] = UserName;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = IPAddress;
                            dr["UpdatedBy"] = UserName;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = IPAddress;
                            dsWeeklyAbsentismAssignment.Tables[0].Rows.Add(dr);


                        }
                        else
                        {
                            //edit
                            DataRow dr = dvExceptionEmployeeAttendanceUnlock[0].Row;

                            dr.BeginEdit();
                            dr["EmpSystemID"] = AbsentList[i].EmpSystemID.ToString();
                            dr["WorkingDate"] = AbsentList[i].WorkingDate.ToString();
                            dr["UpdatedBy"] = UserName;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = IPAddress;
                            dr.EndEdit();

                        }
                        dvExceptionEmployeeAttendanceUnlock.RowFilter = null;


                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsWeeklyAbsentismAssignment);
                }

            }
            catch (Exception ex)
            {

                throw (ex);
            }
            
        }
    }

    public class SandwichAbsentVM
    {
        public string EmpSystemID { get; set; }
        public string WorkingDate { get; set; }
        public string DayStatus { get; set; }
    }
}
