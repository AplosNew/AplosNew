using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.Arrear
{
    public class ArrearProcess
    {
        SqlRepository _sqlRepository = new SqlRepository();
        // List<Dictionary<string, object>> dic = CustomJsonResultService.DataTableToJson(ds.Tables[0]);
        public void DateValidation(string fromdate, string todate)
        {
            try
            {
                if (string.IsNullOrEmpty(fromdate))
                {
                    throw new Exception("'From Date' can not be blank...");
                }

                if (string.IsNullOrEmpty(todate))
                {
                    throw new Exception("'To Date' can not be blank...");
                }

                if (bplib.clsWebLib.IsDateOK(fromdate) == false)
                {
                    throw new Exception("From Date is not in right format; Expected Date fromat is 'dd-MMM-yyyy'; ex:'07-Feb-2010'");
                }
                if (bplib.clsWebLib.IsDateOK(todate) == false)
                {
                    throw new Exception("To Date is not in right format; Expected Date fromat is 'dd-MMM-yyyy'; ex:'07-Feb-2010'");
                }

                if (Convert.ToDateTime(fromdate) > Convert.ToDateTime(todate))
                {
                    throw new Exception("'To Date' can not be less than from date...");
                }

                if (Convert.ToDateTime(fromdate).ToString("yyyy") != Convert.ToDateTime(todate).ToString("yyyy"))
                {
                    throw new Exception("'Year' must be same in both FromDate and ToDate...");
                }

                if (Convert.ToDateTime(fromdate).ToString("MMM") != Convert.ToDateTime(todate).ToString("MMM"))
                {
                    throw new Exception("'Month' must be same in both FromDate and ToDate...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void LoadMLVProcessed(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from
									(									
									select  e.systemid,e.EmployeeCode,e.systemid EmpSystemID
                                    ,e.EmployeeName 
									,t.BabyNo
                                    ,EmployeeCodePreFix,EmployeeCodeNumeric
									,WithBenefit=case when p.IsNoBenefit=1 then 'No' else 'Yes' end
									,format(t.FromDate,'dd-MMM-yyyy') GoingON
									,format(t.ToDate,'dd-MMM-yyyy') ComingON
									--,t.ToDate
									--,p.IsNoBenefit
									--,tt.CG
									--,t.CG
                                    ,format(e.DOJ,'dd-MMM-yyyy') DOJ
                                    ,format(e.DOS,'dd-MMM-yyyy') DOS
                                    ,d.UserName GivenDesignation
                                    ,dd.UserName LegalDesignation
                                    ,s.UserName Section
                                    ,ss.UserName Subsection
                                    ,e.EmployeeStatus	
									,t.CG	LeaveStatus	,'' GivenDesignationId						
																		
                                    from EmployeeInformation e                                    
									left join hkp.Designation d on d.id=e.GivenDesignationId
		                            left join org.SubSection ss on ss.id=e.SubSectionId         
									left join org.Section s on s.id=e.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId
									left join (
									select *,'Going' CG from LeaveTransaction where FromDate between 
                                         '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"' 
										 and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')
									) t on t.EmpSystemID=e.SystemId
									left join mst.MaternityLeavePolicy p on p.id=t.MaternityLeavePolicyId
                                    where e.PlantId='" + sPlantID + @"' 							
									and
									e.SystemId in
									(--mlv
									select EmpSystemID from LeaveTransaction where DATEADD(DAY,-1,FromDate) between 
                                        '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"'  
                                        and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')
                                        and MaternityLeavePolicyId in (select Id from mst.MaternityLeavePolicy where IsMonthly=0)
									)--mlv

                                    --Approved SP
                                    and e.systemid in
                                    (
                                     (select EmpSystemId from SalaryLock where  MonthNo=Month('" + sFromDate + @"') and YearNo = Year('" + sFromDate + @"') and IsLocked=1)  
                                    )--Approved SP
									) x																	
									order by EmployeeCodePreFix,EmployeeCodeNumeric";

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
        public List<Dictionary<string, object>> GetEmployee(string FromDate, string ToDate, string PlantId)
        {

            try
            {
                if (string.IsNullOrEmpty(FromDate))
                {
                    throw new Exception("'From Date' can not be blank...");
                }

                if (string.IsNullOrEmpty(ToDate))
                {
                    throw new Exception("'To Date' can not be blank...");
                }

                if (bplib.clsWebLib.IsDateOK(FromDate) == false)
                {
                    throw new Exception("From Date is not in right format; Expected Date fromat is 'dd-MMM-yyyy'; ex:'07-Feb-2010'");
                }
                if (bplib.clsWebLib.IsDateOK(ToDate) == false)
                {
                    throw new Exception("To Date is not in right format; Expected Date fromat is 'dd-MMM-yyyy'; ex:'07-Feb-2010'");
                }

                if (Convert.ToDateTime(FromDate) > Convert.ToDateTime(ToDate))
                {
                    throw new Exception("'To Date' can not be less than from date...");
                }


                string sql = @"SELECT [CheckBoxSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'false'),EMP.SystemID AS EmpSystemID,
                                    FORMAT(emp.DOJ,'dd-MMM-yyyy') AS DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') AS DOS,EMP.EmployeeStatus,DIV.UserName AS Division,
                                    EMP.EmployeeName,EMP.EmployeeCode,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,concat( sl.YearNo,'/', sl.MonthNo) LastLocked
                                    ,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation, PR.UserName PositionName,format(SEFD.EffectiveDate,'dd-MMM-yyyy') AS LastSalaryEffectiveDate,
                                    DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection,PL.UserName Plant,SEFD.SalaryRuleMasterSystemID ,srm.SalaryRuleName
                                    FROM (
		                            	
		                            	SELECT * FROM (  SELECT  *,
				                            DENSE_RANK() OVER (PARTITION BY SDM.EmpInfoSystemID ORDER BY SDM.EffectiveDate DESC) AS RNK

			                                                from (
							                                                SELECT sdm.EmpInfoSystemID,SDM.SalaryRuleMasterSystemID ,sdm.EffectiveDate,sdm.IsApproved
							                                                   from SalaryInfoDefineMaster SDM
								                                                union ALL
								                                                select sdm.EmpInfoSystemID,SDM.SalaryRuleMasterSystemID ,sdm.EffectiveDate,sdm.IsApproved
								                                            from SalaryInfoBackMaster SDM
			                                                ) AS SDM
			
			                                        ) AS SDM 
                                                    WHERE ISNULL(sdm.IsApproved,'')=1 AND EffectiveDate <= '" + ToDate + @"' AND rnk=1 
		                                   ) SEFD 


			                            LEFT JOIN EmployeeInformation EMP ON SEFD.EmpInfoSystemID = EMP.SystemID
			                            LEFT JOIN SalaryRuleMaster AS srm ON srm.SystemID=sefd.SalaryRuleMasterSystemID
			                             LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
										LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
										LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
										LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
										LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
										LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
										LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
										LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
										LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
			                            LEFT JOIN org.Division AS DIV ON DIV.Id=emp.DivisionId
			                            LEFT JOIN SalaryLock AS sl  ON sl.EmpSystemId=emp.SystemId AND sl.Id=(SELECT TOP 1 Id FROM salaryLock xl where  xl.IsLocked=1 and xl.EmpSystemId=emp.SystemId ORDER BY xl.YearNo DESC,xl.MonthNo DESC)
			                              WHERE EMP.DOJ <= '" + ToDate + @"'
                                            and EMP.EmployeeStatus='Active'
			                              AND (EMP.DOS >= '" + FromDate + @"' OR ISNULL(EMP.DOS,'') = '' OR EMP.DOS = '01/01/1901')
                                           AND SEFD.EffectiveDate <= '" + ToDate + @"' AND emp.PlantId='" + PlantId + @"'
			                          ";

                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}
