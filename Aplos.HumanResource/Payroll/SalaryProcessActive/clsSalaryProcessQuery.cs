using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.SalaryProcessActive
{
    public class clsSalaryProcessQuery
    {
        public void DeleteExceptionEmpsForSalaryProcess(string empids, string plantid, string YearNo, string MonthNo)
        {
            //throw new Exception("test");//
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                string _ss = @"delete from ExceptionEmployeeSalaryReprocess where EmpSystemId in (" + empids + @") and plantid = '" + plantid + @"' 
                                    and Yearno = " + YearNo + @" and monthno = " + MonthNo + @"";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(_ss, true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void GetEmpList(string[] emppks, string fromdate, string todate, string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                string _emps = string.Empty;

                foreach (var item in emppks)
                {
                    if (_emps.Length == 0)
                    {
                        _emps = "'" + item + "'";
                    }
                    else
                    {
                        _emps += ", '" + item + "'";
                    }
                }
                if (_emps.Length == 0)
                {
                    _emps = "''";
                }
                strSql = @"SELECT --IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')  ELSE Convert(bit, 'True') END,
                                    IsSelectSlrProc = Convert(bit, 'True') ,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = 'OK',
                          e.GivenDesignationId
                            ,format(DATEADD(DAY,-1,lv.FromDate),'dd-MMM-yyyy') MLVFrom
						  ,format(DATEADD(DAY,1,lv.ToDate),'dd-MMM-yyyy') MLVTo

                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + fromdate + @"') AND 
															YearNo = Year('" + fromdate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID

                                        --MLV
										left join
										(
										select * from LeaveTransaction where 
										--DATEPART(month, FromDate)=month('" + fromdate + @"')
                                        DATEADD(DAY,-1,FromDate) between   '" + fromdate + @"' and '" + todate + @"'
                                        and PlantId='" + plantid + @"'
										and LTSystemID in (select id from LeaveType where leavetype='maternity')--LWP(with benefit and regular) should be considered
										) LV on lv.EmpSystemID=e.SystemId
										--MLV 

                                            LEFT OUTER JOIN
                                            (
                                            SELECT MAX(ToDate) ToDate, EmpInfoSystemID FROM
                                            (
                                            SELECT DISTINCT m.SystemID ,m.FromDate, m.ToDate, c.EmpInfoSystemID FROM SalaryProcMaster m
                                            LEFT OUTER JOIN SalaryProcChild C ON M.SystemID = C.SlrProcMstSystemID
                                            WHERE C.PlantID='" + plantid + @"' AND C.IsApproved=1
                                            ) X
                                            GROUP BY EmpInfoSystemID
                                            ) Y ON Y.EmpInfoSystemID = E.SystemId

                               WHERE E.systemid in (" + _emps + ")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetEmpListSepa(string[] emppks, string fromdate, string todate, string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                string dtFDPrevM = Convert.ToDateTime(fromdate).AddMonths(-1).ToString("dd-MMM-yyyy");
                string _emps = string.Empty;

                foreach (var item in emppks)
                {
                    if (_emps.Length == 0)
                    {
                        _emps = "'" + item + "'";
                    }
                    else
                    {
                        _emps += ", '" + item + "'";
                    }
                }
                if (_emps.Length == 0)
                {
                    _emps = "''";
                }
                strSql = @"SELECT --IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')  ELSE Convert(bit, 'True') END,
                                    IsSelectSlrProc = Convert(bit, 'True') ,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = 'OK',
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END
                                        --,e.GivenDesignationId
                                        ,dm.DesignationId GivenDesignationId
                                        ,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
										,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
										,dm.EmployeeCategoryId,gr.LegalSalaryGradeId
,IsLocked = case when isnull(sl.EmpSystemId,'')='' and isnull(k.EmpInfoSystemID,'')='' then 'YES'
										when isnull(sl.EmpSystemId,'')='' and isnull(k.EmpInfoSystemID,'')<>'' then 'NO'
										 else 'YES' end
,EBI.IFSCCode
,EBI.MICRCode,AG.UserName AccountsGroup


                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + fromdate + @"') AND 
															YearNo = Year('" + fromdate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id                                        

left join SalaryLock sl on sl.EmpSystemId=e.SystemId and sl.MonthNo=Month('" + dtFDPrevM + "') and sl.YearNo=Year('" + dtFDPrevM + @"') and   IsLocked=1
left join (select distinct EmpInfoSystemID from SalaryProcChild where SlrProcMstSystemID in (select systemid from SalaryProcMaster where MonthNo=Month('" + dtFDPrevM + @"') and YearNo=Year('" + dtFDPrevM + @"'))) k on k.EmpInfoSystemID=e.SystemId

                                        left join  mst.DesignationMasterLegalDesignation dl on dl.LegalDesignationId=e.LegalDesignationId
										left join mst.DesignationMaster dm on dm.id=dl.DesignationMasterId
										LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = dm.DesignationId

                                        --LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
                                        --left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId

										left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId
LEFT JOIN SCS.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId=DM.Id AND DMC.PlantId=e.PlantId
										LEFT JOIN dbo.AccountsGroup AG ON AG.Id=DMC.AccountsGroupId
                                        --MLV
										left join
										(
										select * from LeaveTransaction where 
										--DATEPART(month, FromDate)=month('" + fromdate + @"')
                                        DATEADD(DAY,-1,FromDate) between   '" + fromdate + @"' and '" + todate + @"'
                                        and PlantId='" + plantid + @"'
										and LTSystemID in (select id from LeaveType where leavetype='maternity')--LWP(with benefit and regular) should be considered
										) LV on lv.EmpSystemID=e.SystemId
										--MLV 

                                            LEFT OUTER JOIN
                                            (
                                            SELECT MAX(ToDate) ToDate, EmpInfoSystemID FROM
                                            (
                                            SELECT DISTINCT m.SystemID ,m.FromDate, m.ToDate, c.EmpInfoSystemID FROM SalaryProcMaster m
                                            LEFT OUTER JOIN SalaryProcChild C ON M.SystemID = C.SlrProcMstSystemID
                                            WHERE C.PlantID='" + plantid + @"' AND C.IsApproved=1
                                            ) X
                                            GROUP BY EmpInfoSystemID
                                            ) Y ON Y.EmpInfoSystemID = E.SystemId

                               WHERE E.systemid in (" + _emps + ")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetEmpList_MLV_Going(string[] emppks, string fromdate, string todate, string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                string _emps = string.Empty;

                foreach (var item in emppks)
                {
                    if (_emps.Length == 0)
                    {
                        _emps = "'" + item + "'";
                    }
                    else
                    {
                        _emps += ", '" + item + "'";
                    }
                }
                if (_emps.Length == 0)
                {
                    _emps = "''";
                }
                strSql = @"SELECT --IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')  ELSE Convert(bit, 'True') END,
                                    IsSelectSlrProc = Convert(bit, 'True') ,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  dm.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = 'OK',
                          e.GivenDesignationId
                            ,format(DATEADD(DAY,-1,t.FromDate),'dd-MMM-yyyy') MLVFrom
						  ,format(DATEADD(DAY,1,t.ToDate),'dd-MMM-yyyy') MLVTo

                                     ,t.BabyNo
									,WithBenefit=case when p.IsNoBenefit=1 then 'No' else 'Yes' end
									,format(t.FromDate,'dd-MMM-yyyy') GoingON
									,format(t.ToDate,'dd-MMM-yyyy') ComingON                                    
									
                                    ,dd.UserName LegalDesignation
                                    ,sc.UserName Section
                                    ,ss.UserName Subsection
                                    ,e.EmployeeStatus		
									,t.CG		LeaveStatus	,'OK' ProcessStatus
,dm.DesignationId GivenDesignationId
                                    ,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
									,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
									,dm.EmployeeCategoryId,gr.LegalSalaryGradeId
,EBI.IFSCCode
,EBI.MICRCode

                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + fromdate + @"') AND 
															YearNo = Year('" + fromdate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                                        left join  mst.DesignationMasterLegalDesignation dl on dl.LegalDesignationId=e.LegalDesignationId
										left join mst.DesignationMaster dm on dm.id=dl.DesignationMasterId
										LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = dm.DesignationId

                                        --LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
                                        --left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId

                            
                                        left join org.SubSection ss on ss.id=PR.SubSectionId         
									left join org.Section sc on sc.id=PR.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId

                                         --LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                         --left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId
										left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId

                                        --MLV
										left join
										(
										select *,'Going' CG from LeaveTransaction where										
                                        DATEADD(DAY,-1,FromDate) between   '" + fromdate + @"' and '" + todate + @"'
                                        and PlantId='" + plantid + @"'
										and LTSystemID in (select id from LeaveType where leavetype='maternity')--LWP(with benefit and regular) should be considered
										) t on t.EmpSystemID=e.SystemId
										--MLV 
                                            left join mst.MaternityLeavePolicy p on p.id=t.MaternityLeavePolicyId
                                            LEFT OUTER JOIN
                                            (
                                            SELECT MAX(ToDate) ToDate, EmpInfoSystemID FROM
                                            (
                                            SELECT DISTINCT m.SystemID ,m.FromDate, m.ToDate, c.EmpInfoSystemID FROM SalaryProcMaster m
                                            LEFT OUTER JOIN SalaryProcChild C ON M.SystemID = C.SlrProcMstSystemID
                                            WHERE C.PlantID='" + plantid + @"' AND C.IsApproved=1
                                            ) X
                                            GROUP BY EmpInfoSystemID
                                            ) Y ON Y.EmpInfoSystemID = E.SystemId

                               WHERE E.systemid in (" + _emps + ")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetEmpList_MLV_Going(string _emps, string fromdate, string todate, string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {//             
                if (_emps.Length == 0)
                {
                    _emps = "''";
                }
                strSql = @"SELECT --IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')  ELSE Convert(bit, 'True') END,
                                    IsSelectSlrProc = Convert(bit, 'True') ,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = 'OK',
                          e.GivenDesignationId
                            ,format(DATEADD(DAY,-1,t.FromDate),'dd-MMM-yyyy') MLVFrom
						  ,format(DATEADD(DAY,1,t.ToDate),'dd-MMM-yyyy') MLVTo

                                     ,t.BabyNo
									,WithBenefit=case when p.IsNoBenefit=1 then 'No' else 'Yes' end
									,format(t.FromDate,'dd-MMM-yyyy') GoingON
									,format(t.ToDate,'dd-MMM-yyyy') ComingON                                    
									
                                    ,dd.UserName LegalDesignation
                                    ,sc.UserName Section
                                    ,ss.UserName Subsection
                                    ,e.EmployeeStatus		
									,t.CG		LeaveStatus	,'OK' ProcessStatus,GivenDesignationId
                                    ,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
									,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
									,dm.EmployeeCategoryId,gr.LegalSalaryGradeId
,EBI.IFSCCode
,EBI.MICRCode


                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + fromdate + @"') AND 
															YearNo = Year('" + fromdate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
                                         left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
                                        left join org.SubSection ss on ss.id=PR.SubSectionId         
									left join org.Section sc on sc.id=PR.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId

                                         --LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
										left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId

                                        --MLV
										left join
										(
										select *,'Going' CG from LeaveTransaction where										
                                        DATEADD(DAY,-1,FromDate) between   '" + fromdate + @"' and '" + todate + @"'
                                        and PlantId='" + plantid + @"'
										and LTSystemID in (select id from LeaveType where leavetype='maternity')--LWP(with benefit and regular) should be considered
										) t on t.EmpSystemID=e.SystemId
										--MLV 
                                            left join mst.MaternityLeavePolicy p on p.id=t.MaternityLeavePolicyId
                                            LEFT OUTER JOIN
                                            (
                                            SELECT MAX(ToDate) ToDate, EmpInfoSystemID FROM
                                            (
                                            SELECT DISTINCT m.SystemID ,m.FromDate, m.ToDate, c.EmpInfoSystemID FROM SalaryProcMaster m
                                            LEFT OUTER JOIN SalaryProcChild C ON M.SystemID = C.SlrProcMstSystemID
                                            WHERE C.PlantID='" + plantid + @"' AND C.IsApproved=1
                                            ) X
                                            GROUP BY EmpInfoSystemID
                                            ) Y ON Y.EmpInfoSystemID = E.SystemId

                               WHERE E.systemid in (" + _emps + ")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void LoadMLVReturn__(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from
									(
									select  
                                    Convert(bit, 'False') IsSelectSlrProc
                                    ,Convert(bit, 'False') IsApproved
                                    ,Convert(bit, 'False') IsDisbursed
                                    ,e.systemid
                                    ,e.systemid EmpSystemID
                                    ,e.EmployeeCode
                                    ,e.EmployeeName 
									,t.BabyNo
									,WithBenefit=case when p.IsNoBenefit=1 then 'No' else 'Yes' end
									,format(t.FromDate,'dd-MMM-yyyy') GoingON
									,format(t.ToDate,'dd-MMM-yyyy') ComingON
                                    ,format(DATEADD(DAY,-1,t.FromDate),'dd-MMM-yyyy') MLVFrom
									,format(DATEADD(DAY,1,t.ToDate),'dd-MMM-yyyy') MLVTo
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
									,t.CG		LeaveStatus	,'OK' ProcessStatus,GivenDesignationId
                                    ,e.LegalDesignationId,e.PaymentMode,e.BudgetCode
									,ebi.BankSystemID,ebi.BankBranchId,ebi.BankAccNo,ebi.SalaryPercentage
									,dm.EmployeeCategoryId,gr.LegalSalaryGradeId
																		
                                    from EmployeeInformation e   
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									left join hkp.Designation d on d.id=e.GivenDesignationId
		                            left join org.SubSection ss on ss.id=PR.SubSectionId         
									left join org.Section s on s.id=PR.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId

 LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
 left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId
										left join mst.LegalSalaryGradeDesignation  gr on gr.LegalDesignationId=e.LegalDesignationId and gr.PlantId=e.PlantId
									
									left join (
									select *,'Coming' CG from LeaveTransaction where DATEADD(DAY,1,ToDate) between 
                                         '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"'  
										 and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')

									) t on t.EmpSystemID=e.SystemId

									left join mst.MaternityLeavePolicy p on p.id=t.MaternityLeavePolicyId
                                    where e.PlantId='" + sPlantID + @"' 						
									and 
                                    --Mlv return
									e.SystemId in
									(
									select EmpSystemID from LeaveTransaction 
                                            where DATEADD(DAY,1,ToDate) between  '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"' 
                                            and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')
                                            and MaternityLeavePolicyId in (select Id from mst.MaternityLeavePolicy where IsMonthly=0)
									)--Mlv return

                                    --Approved SP
                                    and e.systemid not in
                                    (
                                    SELECT distinct SC.EmpInfoSystemID
                                    FROM SalaryProcChild SC
                                    INNER JOIN (SELECT SystemID FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
                                    WHERE IsApproved = 1
                                    )--Approved SP

                                   --active
									and isnull(e.dos,'') not between '" + sFromDate + @"' and '" + sToDate + @"'

									) x
																	
									order by EmployeeCode";//eee

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

        public void GetAttdnDataForMonthlyProc(string wc, FunctionPara Parameters, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                clsCrossModule ob = new clsCrossModule();

                strSQL = @"SELECT EmpSystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate,
                                SUM(ISNULL(A.WorkingDayValue,0)) TotalProcDate,

                                SUM(ISNULL(CAST(WorkingDayValue As decimal(18, 2)), '0.00')) TotalWorkingDay,
                                SUM(ISNULL(CAST(ActualWorkingDayValue As decimal(18, 2)), '0.00')) TotalActualWorkingDay,
                                SUM(ISNULL(CAST(PayDayValue As decimal(18, 2)), '0.00')) TotalPayDay,
                                SUM(ISNULL(CAST(NonPayDayValue As decimal(18, 2)), '0.00')) TotalNonPayDay,
                                SUM(ISNULL(CAST(TotalPresent As decimal(18, 2)), '0.00')) TotalPresent,
                                SUM(ISNULL(CAST(TotalLate As decimal(18, 2)), '0.00')) TotalLate,
                                SUM(ISNULL(CAST(TotalAbsent As decimal(18, 2)), '0.00')) TotalAbsent,
                                SUM(ISNULL(CAST(TotalLv As decimal(18, 2)), '0.00')) TotalLv,
                                SUM(ISNULL(CAST(TotalMLv As decimal(18, 2)),'0.00')) TotalMLv,
                                SUM(ISNULL(CAST(TotalWeekOff As decimal(18, 2)),'0.00')) TotalWeekOff,
                                SUM(ISNULL(CAST(TotalCompAssignLv As decimal(18, 2)),'0.00')) TotalCompAssignLv,
                                SUM(ISNULL(CAST(TotalHoliDay As decimal(18, 2)),'0.00')) TotalHoliDay,
                                SUM(ISNULL(CAST(TotalWeekOffHoliDay As decimal(18, 2)),'0.00')) TotalWeekOffHoliDay,
                                SUM(ISNULL(CAST(OTHr As decimal(18, 2)), '0.00')) TotalOTHr,
                                0.00 TotalNormalOTHr,
                                0.00 TotalExtraOTHr, 
                                SUM(ISNULL(CAST(TotalLWP As decimal(18, 2)), '0.00')) TotalLWP,  
                                SUM(ISNULL(CAST(TotalLVWithPay As decimal(18, 2)), '0.00')) TotalLVWithPay,  
                                WeekoffDays =STUFF((select ','+CONCAT(DATEPART(DAY, apdX.WorkDate),'-',FORMAT(apdx.WorkDate,'ddd'))from 
																			AttdnProcessData AS apdX                                              
							                                where apdX.EmpSystemID=A.EmpSystemID  
							                                AND apdx.WorkDate BETWEEN '" + Parameters.FromDate + @"' AND '" + Parameters.ToDate + @"'
							                                AND apdx.WeekOffValue>0	ORDER BY apdx.WorkDate ASC for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                FROM (SELECT EmpSystemID, WorkDate,WorkingDayValue,ActualWorkingDayValue ,PayDayValue,NonPayDayValue,
										TotalPresent = PresentValue,
                                                        --LWP and LWOP both r considered          
			                            TotalLate = LateValue,
			                            TotalAbsent = AbsentValue,
			                            TotalLv = LvValue,
                                        TotalLWP = CASE WHEN ISNULL(ds.PayDay,0)=0 THEN l.AvailedValue ELSE 0 END,
                                        TotalLVWithPay = CASE WHEN ISNULL(ds.PayDay,0)>0 THEN l.AvailedValue ELSE 0 END,
			                            TotalMLv = CASE WHEN ISNULL(lt.LeaveType,'')='' THEN 0 ELSE 1 END,
                                        TotalCompAssignLv = 0,
			                            TotalWeekOff = WeekOffValue,
			                            TotalHoliDay = HoliDayValue,
                                        TotalWeekOffHoliDay = 0,
                                OTHr
                                FROM dbo.AttdnProcessData APD
                                left join daytype p on APD.DayStatus=p.DayType
                                LEFT JOIN LeaveType AS lt ON lt.Id=apd.LTSystemID

                                LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=apd.EmpSystemID
                                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=apd.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id AND l.LeaveTypeId=apd.LTSystemID
                                WHERE " + wc + @") A
                                GROUP BY EmpSystemID";


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
        public void xGetAttdnDataForMonthlyProc(string wc, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                clsCrossModule ob = new clsCrossModule();
                //strSQL = @"SELECT EmpSystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate, 
                //                    COUNT(WorkDate) TotalProcDate, 
                //              SUM(ISNULL(CAST(TotalPresent As decimal(18, 2)), '0.00')) TotalPresent, 
                //                    SUM(ISNULL(CAST(TotalLate As decimal(18, 2)), '0.00')) TotalLate, 
                //              SUM(ISNULL(CAST(TotalAbsent As decimal(18, 2)), '0.00')) TotalAbsent, 
                //                    SUM(ISNULL(CAST(TotalLv As decimal(18, 2)), '0.00')) TotalLv, 
                //              SUM(ISNULL(TotalMLv,0)) TotalMLv, 
                //                    SUM(ISNULL(TotalWeekOff,0)) TotalWeekOff, 
                //                    SUM(ISNULL(TotalCompAssignLv,0))  TotalCompAssignLv,
                //              SUM(ISNULL(TotalHoliDay,0)) TotalHoliDay, 
                //                    SUM(ISNULL(TotalWeekOffHoliDay,0)) TotalWeekOffHoliDay,
                //                    SUM(ISNULL(CAST(OTHr As decimal(18, 2)), '0.00')) TotalOTHr, 
                //                    0.00 TotalNormalOTHr, 
                //                    0.00 TotalExtraOTHr, PlantID  ,
                //                    SUM(ISNULL(TotalLWP, 0)) TotalLWP   
                //            FROM (SELECT EmpSystemID, WorkDate, PlantID,
                //                " + ob.GetAttSum() + @"
                //                        OTHr
                //              FROM dbo.AttdnProcessData 
                //                WHERE WorkDate BETWEEN '" + sfrmDate + @"'
                //                    AND '" + sToDate + @"'  
                //                    AND (" + sEmpSystemID + @")) A
                //            GROUP BY EmpSystemID, PlantID";

                strSQL = @"SELECT EmpSystemID, MIN(WorkDate) FromDate, MAX(WorkDate) ToDate, 
                                    COUNT(WorkDate) TotalProcDate, 
		                            SUM(ISNULL(CAST(TotalPresent As decimal(18, 2)), '0.00')) TotalPresent, 
                                    SUM(ISNULL(CAST(TotalLate As decimal(18, 2)), '0.00')) TotalLate, 
		                            SUM(ISNULL(CAST(TotalAbsent As decimal(18, 2)), '0.00')) TotalAbsent, 
                                    SUM(ISNULL(CAST(TotalLv As decimal(18, 2)), '0.00')) TotalLv, 
		                            SUM(ISNULL(TotalMLv,0)) TotalMLv, 
                                    SUM(ISNULL(TotalWeekOff,0)) TotalWeekOff, 
                                    SUM(ISNULL(TotalCompAssignLv,0))  TotalCompAssignLv,
		                            SUM(ISNULL(TotalHoliDay,0)) TotalHoliDay, 
                                    SUM(ISNULL(TotalWeekOffHoliDay,0)) TotalWeekOffHoliDay,
                                    SUM(ISNULL(CAST(OTHr As decimal(18, 2)), '0.00')) TotalOTHr, 
                                    0.00 TotalNormalOTHr, 
                                    0.00 TotalExtraOTHr, PlantID  ,
                                    SUM(ISNULL(TotalLWP, 0)) TotalLWP   
                            FROM (SELECT EmpSystemID, WorkDate, PlantID,
			                             " + ob.GetAttSum() + @"
                                        OTHr
	                             FROM dbo.AttdnProcessData 
                                WHERE " + wc + @") A
                            GROUP BY EmpSystemID, PlantID";


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
        string GetGreaterDate(string FromDate, string DOJ)
        {
            string _result = FromDate;
            try
            {
                if (Convert.ToDateTime(DOJ) > Convert.ToDateTime(FromDate))
                {
                    _result = Convert.ToDateTime(DOJ).ToString("dd-MMM-yyyy");
                }
                return _result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string GetMinDate(string ToDate, string DOS)
        {
            string _result = ToDate;
            try
            {
                if (string.IsNullOrEmpty(DOS) == false)
                {
                    if (Convert.ToDateTime(DOS) < Convert.ToDateTime(ToDate))
                    {
                        _result = Convert.ToDateTime(DOS).ToString("dd-MMM-yyyy");
                    }
                }
                return _result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void xCreate_EmpDateRange_For_WC(DataSet dsEmp, string FromDate, out string wc)
        {
            wc = string.Empty;
            string _empid = string.Empty;
            string _DOJ = string.Empty;
            string _LVFD = string.Empty;
            ///1st day of the month
            ///doj
            try
            {
                for (int i = 0; i < dsEmp.Tables[0].Rows.Count; i++)
                {
                    _empid = dsEmp.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim();
                    //_LVFD =Convert.ToDateTime(dsEmp.Tables[0].Rows[i]["MLVFrom"].ToString().Trim()).AddDays(-1).ToString("dd-MMM-yyyy");
                    _LVFD = Convert.ToDateTime(dsEmp.Tables[0].Rows[i]["MLVFrom"].ToString().Trim()).ToString("dd-MMM-yyyy");
                    _DOJ = dsEmp.Tables[0].Rows[i]["DOJ"].ToString().Trim();
                    var FD = GetGreaterDate(FromDate, _DOJ);
                    if (wc.Length == 0)
                    {
                        wc = " (Empsystemid='" + _empid + @"' and workdate between '" + FD + @"' and '" + _LVFD + @"')";
                    }
                    else
                    {
                        wc += " OR (Empsystemid='" + _empid + @"' and workdate between '" + FD + @"' and '" + _LVFD + @"')";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Create_EmpDateRange_For_WC(DataSet dsEmp, string FromDate, out string wc)
        {
            wc = string.Empty;
            string _empid = string.Empty;
            string _DOJ = string.Empty;
            string _LVFD = string.Empty;
            ///1st day of the month
            ///doj
            try
            {
                for (int i = 0; i < dsEmp.Tables[0].Rows.Count; i++)
                {
                    _empid = dsEmp.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim();
                    _LVFD = Convert.ToDateTime(dsEmp.Tables[0].Rows[i]["MLVFrom"].ToString().Trim()).ToString("dd-MMM-yyyy");
                    //_LVFD = Convert.ToDateTime(dsEmp.Tables[0].Rows[i]["MLVFrom"].ToString().Trim()).AddDays(-1).ToString("dd-MMM-yyyy");
                    _DOJ = dsEmp.Tables[0].Rows[i]["DOJ"].ToString().Trim();
                    var FD = GetGreaterDate(FromDate, _DOJ);
                    if (wc.Length == 0)
                    {
                        wc = " (Empsystemid='" + _empid + @"' and workdate between '" + FD + @"' and '" + _LVFD + @"')";
                    }
                    else
                    {
                        wc += " OR (Empsystemid='" + _empid + @"' and workdate between '" + FD + @"' and '" + _LVFD + @"')";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void xCreate_EmpDateRange_For_Return_WC(DataSet dsEmp, string ToDate, out string wc)
        {
            wc = string.Empty;
            string _empid = string.Empty;
            string _DOS = string.Empty;
            string _LVTD = string.Empty;
            ///1st day of the month
            ///doj
            try
            {
                for (int i = 0; i < dsEmp.Tables[0].Rows.Count; i++)
                {
                    _empid = dsEmp.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim();
                    _LVTD = Convert.ToDateTime(dsEmp.Tables[0].Rows[i]["MLVTo"].ToString().Trim()).AddDays(1).ToString("dd-MMM-yyyy");
                    _DOS = dsEmp.Tables[0].Rows[i]["DOS"].ToString().Trim();
                    var TD = GetMinDate(ToDate, _DOS);
                    if (wc.Length == 0)
                    {
                        wc = " (Empsystemid='" + _empid + @"' and workdate between '" + _LVTD + @"' and '" + TD + @"')";
                    }
                    else
                    {
                        wc += " OR (Empsystemid='" + _empid + @"' and workdate between '" + _LVTD + @"' and '" + TD + @"')";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Create_EmpDateRange_For_Return_WC(DataSet dsEmp, string ToDate, out string wc)
        {
            wc = string.Empty;
            string _empid = string.Empty;
            string _DOS = string.Empty;
            string _LVTD = string.Empty;
            ///1st day of the month
            ///doj
            try
            {
                for (int i = 0; i < dsEmp.Tables[0].Rows.Count; i++)
                {
                    _empid = dsEmp.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim();
                    //_LVTD = Convert.ToDateTime(dsEmp.Tables[0].Rows[i]["MLVTo"].ToString().Trim()).AddDays(1).ToString("dd-MMM-yyyy");//
                    _LVTD = Convert.ToDateTime(dsEmp.Tables[0].Rows[i]["MLVTo"].ToString().Trim()).ToString("dd-MMM-yyyy");
                    _DOS = dsEmp.Tables[0].Rows[i]["DOS"].ToString().Trim();
                    var TD = GetMinDate(ToDate, _DOS);
                    if (wc.Length == 0)
                    {
                        wc = " (Empsystemid='" + _empid + @"' and workdate between '" + _LVTD + @"' and '" + TD + @"')";
                    }
                    else
                    {
                        wc += " OR (Empsystemid='" + _empid + @"' and workdate between '" + _LVTD + @"' and '" + TD + @"')";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void LoadSeparatedApprocedEmp(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from
									(									
									select  e.systemid,e.EmployeeCode,e.systemid EmpSystemID
                                    ,e.EmployeeName,EmployeeCodePreFix,EmployeeCodeNumeric
                                    ,format(e.DOJ,'dd-MMM-yyyy') DOJ
                                    ,format(e.DOS,'dd-MMM-yyyy') DOS
                                    ,d.UserName GivenDesignation
                                    ,dd.UserName LegalDesignation
                                    ,s.UserName Section
                                    ,ss.UserName Subsection
                                    ,e.EmployeeStatus	,'' GivenDesignationId																	
                                    from EmployeeInformation e     
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									left join hkp.Designation d on d.id=e.GivenDesignationId
		                            left join org.SubSection ss on ss.id=PR.SubSectionId         
									left join org.Section s on s.id=PR.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId									
                                    where e.PlantId='" + sPlantID + @"' 							
									and
									e.dos between '" + sFromDate + @"' and '" + sToDate + @"'
                                    --Approved SP
                                    and e.systemid  in
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

        public void GetEmpListAll(string emppks, string fromdate, string todate, string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                string _emps = emppks;
                strSql = @"SELECT 
                                    IsSelectSlrProc = Convert(bit, 'True') ,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                      CASE WHEN ISNULL(e.LegalDesignationId,'')='' THEN 1 ELSE
                                    	CASE WHEN ISNULL(dm.DesignationId,'')='' THEN 1 ELSE 0 END END AS MissingDesignation,
                                  '" + fromdate + @"' ToDate,
                                  ProcessStatus = 'OK',
                          e.GivenDesignationId

                           FROM EmployeeInformation E                                        
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=E.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + fromdate + @"') AND 
															YearNo = Year('" + fromdate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
                            WHERE E.systemid in (" + _emps + ")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    if (OTSBD.clsStaticInfo.dbl(dsRef.Tables[0].Rows[i]["MissingDesignation"].ToString()) == 1)
                        throw new Exception("Employee found with missing designation. (hint: missing legal designation with designation master)");
                }
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
        public void GetEmpListAllForArrear(string emppks, string fromdate, string todate, string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                string _emps = emppks;
                strSql = @"SELECT 
                                    IsSelectSlrProc = Convert(bit, 'True') ,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    
                                  '" + fromdate + @"' ToDate,
                                  ProcessStatus = 'OK',
                          e.GivenDesignationId

                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + fromdate + @"') AND 
															YearNo = Year('" + fromdate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
                            WHERE E.systemid in (" + _emps + @") AND E.DOJ <= '" + todate + @"'

                                          AND(E.DOS >= '" + fromdate + @"' OR ISNULL(E.DOS, '') = '' OR E.DOS = '01/01/1901')";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetEmpListAllMLVReturn(string emppks, string fromdate, string todate, string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                string _emps = emppks;
                strSql = @"SELECT 
                                    IsSelectSlrProc = Convert(bit, 'True') ,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    
                                  '" + fromdate + @"' ToDate,
                                  ProcessStatus = 'OK'
                                    ,format(DATEADD(DAY,-1,t.FromDate),'dd-MMM-yyyy') MLVFrom
									,format(DATEADD(DAY,1,t.ToDate),'dd-MMM-yyyy') MLVTo
                                    ,e.GivenDesignationId

                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + fromdate + @"') AND 
															YearNo = Year('" + fromdate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
                                        left join (
									select *,'Coming' CG from LeaveTransaction where DATEADD(DAY,1,ToDate) between 
                                         '" + fromdate + @"' and '" + todate + @"' and PlantId='" + plantid + @"'  
										 and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')

									) t on t.EmpSystemID=e.SystemId
                            WHERE E.systemid in (" + _emps + ")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void LoadBeyondEmps(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select  e.systemid,e.EmployeeCode
                                    ,e.EmployeeName 
                                    ,format(e.DOJ,'dd-MMM-yyyy') DOJ
                                    ,format(e.DOS,'dd-MMM-yyyy') DOS
                                    ,d.UserName GivenDesignation
                                    ,dd.UserName LegalDesignation
                                    ,s.UserName Section
                                    ,ss.UserName Subsection
                                    ,e.EmployeeStatus
                                    ,format(sm.EffectiveDate,'dd-MMM-yyyy') EffectiveDate
									,srm.SalaryRuleName
                                    from  EmployeeInformation e 
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									left join hkp.Designation d on d.id=e.GivenDesignationId
		                            left join org.SubSection ss on ss.id=PR.SubSectionId         
									left join org.Section s on s.id=PR.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId
                                    left join (
									select m.EmpInfoSystemID,m.EffectiveDate,m.SalaryRuleMasterSystemID from 
                                                    (
                                                    select EmpInfoSystemID,EffectiveDate,SalaryRuleMasterSystemID from SalaryInfoDefineMaster 
									                union
									                select EmpInfoSystemID,EffectiveDate,SalaryRuleMasterSystemID  from SalaryInfoBackMaster 
                                                    )
                                                    m 
									inner join (--ssm
									select max(EffectiveDate) ed,EmpInfoSystemID from 
                                                    (
                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster m where EffectiveDate>'" + sToDate + @"' and IsApproved=1
                                                    union
                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster m where EffectiveDate>'" + sToDate + @"' and IsApproved=1
                                                    ) x
                                                    group by EmpInfoSystemID
													) ssm on ssm.ed=m.EffectiveDate and ssm.EmpInfoSystemID=m.EmpInfoSystemID
									) sm on sm.EmpInfoSystemID=e.SystemId
									left join SalaryRuleMaster srm on srm.SystemID=sm.SalaryRuleMasterSystemID
                                    where e.PlantId='" + sPlantID + @"'
                                    and e.systemid in
                                                    (--3
                                                    select EmpInfoSystemID from
                                                    (--2
                                                    select max(EffectiveDate) ed,EmpInfoSystemID from 
                                                    (
                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster m where EffectiveDate>'" + sToDate + @"' and IsApproved=1
                                                    union
                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster m where EffectiveDate>'" + sToDate + @"' and IsApproved=1
                                                    ) x
                                                    group by EmpInfoSystemID
                                                    ) --2
                                                    y
                                                    )--3
                                                    -----------------------not in prev------------------------------
                                                    and e.systemid not in

                                                    (--3
                                                    select EmpInfoSystemID from
                                                    (--2
                                                    select max(EffectiveDate) ed,EmpInfoSystemID from 
                                                    (
                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster m where EffectiveDate<='" + sToDate + @"' and IsApproved=1
                                                    union
                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster m where EffectiveDate<='" + sToDate + @"' and IsApproved=1
                                                    ) x
                                                    group by EmpInfoSystemID
                                                    ) --2
                                                    y
                                                    )--3
                                                    and doj<='" + sToDate + @"'
                                                ";



                strSQL += @"
                            ORDER BY convert(INT, E.EmployeeCode) ";

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
        public void LoadLateINEarlyOUTLunchOUT(string empids, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT count(id)c,EmpSystemId,InfoType
                                  FROM [dbo].[AttendanceInfoExtra] 
                                  where WorkDate between '" + sFromDate + @"' and '" + sToDate + @"' and EmpSystemId in (" + empids + @") and InfoType in ('EARLYOUT','LATEIN')
                                  group by EmpSystemId,InfoType
                                  --order by EmpSystemId
                                  union
                                SELECT count(id)c,EmpSystemId,InfoType
                                        FROM [dbo].[AttendanceInfoExtra] 
                                        where
		                                WorkDate between '" + sFromDate + @"' and '" + sToDate + @"' and EmpSystemId in (" + empids + @")	 and 
		                                InfoType in ('LUNCHOUT') and OutTime is not null and InTime is null
		                                and EmpSystemId in --approved as deduction
		                                (
		                                select EmpSystemId from [HourlyOffDuty] where WorkDate between '" + sFromDate + @"' and '" + sToDate + @"' and EmpSystemId in (" + empids + @")
		                                and isnull(ApproveType,'')='Deduction' and isnull(IsApprove,0)=1								   
		                                )
                                        group by EmpSystemId,InfoType
                                  order by EmpSystemId";//eee

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
        public void LoadLWP(string sEmpSysIDColl, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select 
                                sum(LWPValue) LeaveDays,EmpSystemID
                                 from AttdnProcessData 
                                  where    EmpSystemID in (" + sEmpSysIDColl + @") and WorkDate between '" + sFromDate + @"' and '" + sToDate + @"' 
                                  group by EmpSystemID
                                  order by EmpSystemID";//eee

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
        public void LoadSpecificLeave(string empids, string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                                        select sum(leaveDuration) Leave,Iseligible,EmpSystemID,AttdnBonusPmtPolicyDetailsId
                                        from
                                        (
                                        select *
                                        ,IsEligible=case 
                                        --when isnull(PrePostPolicy,'')='Pre' and isnull(PrePostTran,'')='Post' then 'NO'                                       
                                        --else 'YES' end
										 when isnull(IsPreApplied,0)=1 and isnull(PrePostTran,0)=1 then 'YES'
										 when isnull(IsPreApplied,0)=0 then 'YES'
                                         else 'NO' end

                                         from 
                                        (
                                        SELECT m.systemid, m.LTSystemID
                                        ,d.LeaveDuration
                                        ,m.EmpSystemID
                                        ,m.FromDate,m.AppliedDate
                                        ,c.id
                                        ,PrePostTran=case when CONVERT(DATE,m.AppliedDate)<=m.FromDate then 1 else 0 end
                                        ,ab.LeaveTypeId,ab.AttdnBonusPmtPolicyDetailsId,ab.IsPreApplied
                                          FROM LeaveTransactionDetails d
                                          left join LeaveTransaction m on d.LvTrnsSystemID=m.SystemID
                                          left join EmployeeInformation e on e.SystemId=m.EmpSystemID
                                          left join mst.DesignationMaster dm on dm.DesignationId=e.GivenDesignationId
                                          left join (select * from scs.DesignationMasterConfiguration where PlantId='" + sPlantID + @"') c on c.DesignationMasterId=dm.id
                                          left join AttdnBonusLeaveType ab on  ab.AttdnBonusPmtPolicyMasterId=c.AttdnBonusPmtPolicyMasterId and ab.LeaveTypeId=m.LTSystemID 
                                          where WorkDate between '" + sFromDate + @"' and '" + sToDate + @"' 
                                          and m.EmpSystemID in (" + empids + @") 
                                          and isnull(c.id,'')<>''
                                          and isnull(LeaveTypeId,'')<>''
                                          ) x
                                          ) z
                                          group by empsystemid,iseligible,AttdnBonusPmtPolicyDetailsId
                                          order by EmpSystemID
                                        ";//eee

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

        public void LoadRouteEmpList(string sEmpSysIDColl, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select EmployeeId from trn.RouteEmployee where EmployeeId in (" + sEmpSysIDColl + ") and RouteId is not null and Active=1";//eee

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
    }

}
