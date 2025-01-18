using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Payrolls.SalaryProcessActive
{
   public class clsSalaryProcessQuery2
    {//
        public void LoadEmpSalaryProcGrid(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)

        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsHR = null;
            string ZeroBody = string.Empty;
            string ZeroWC = string.Empty;
            try
            {
                GetHRSettingForSeparatedZero(sPlantID,out dsHR);
                if(dsHR.Tables[0].Rows.Count==0)
                {
                    ZeroBody = @"left join 
                                (
                                select EmpSystemID from AttdnDataMonthlySummary where YearNo=Year('" + sFromDate + @"') and MonthNo=Month('" + sFromDate + @"') 
                                and TotalPresent=0 and TotalLv=0 and TotalLate=0 and PlantID='" + sPlantID + @"'
                                ) summ on summ.EmpSystemID=e.SystemId";
                    ZeroWC = @"and isnull(summ.EmpSystemID,'')=''";
                }
                

                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                string dtFDPrevM = Convert.ToDateTime(sFromDate).AddMonths(-1).ToString("dd-MMM-yyyy");
                strSQL = @"SELECT Convert(bit, 'False') IsSelectSlrProc ,EmployeeCodePreFix,EmployeeCodeNumeric,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = CASE WHEN Y.ToDate>'" + sFromDate + @"' THEN 'Overlap'
                                      WHEN Y.ToDate<'" + dtFD + @"' then 'Gap'
                                      ELSE 'OK' END,
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END
                                ,dm.DesignationId GivenDesignationId
                                ,st.UserName SepType
                                ,IsLocked = case when isnull(sl.EmpSystemId,'')='' and isnull(k.EmpInfoSystemID,'')='' then 'YES'
										when isnull(sl.EmpSystemId,'')='' and isnull(k.EmpInfoSystemID,'')<>'' then 'NO'
										 else 'YES' end

                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        left join SalaryLock sl on sl.EmpSystemId=e.SystemID and sl.MonthNo=Month('" + dtFDPrevM + "') and sl.YearNo=Year('" + dtFDPrevM + @"')  and IsLocked=1
left join (select distinct EmpInfoSystemID from SalaryProcChild where SlrProcMstSystemID in (select systemid from SalaryProcMaster where MonthNo=Month('" + dtFDPrevM + @"') and YearNo=Year('" + dtFDPrevM + @"'))) k on k.EmpInfoSystemID=e.SystemId

                                        left join  mst.DesignationMasterLegalDesignation dl on dl.LegalDesignationId=e.LegalDesignationId
										left join mst.DesignationMaster dm on dm.id=dl.DesignationMasterId
										LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = dm.DesignationId

                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID
                                        left join (
										select rr.EmployeeId,rr.ApprovedEffectiveDate,rr.SeparationTypeId from
										(
										select  EmployeeId,max(AddedDate) ad from trn.Resignation where ApprovalStatus='Approved'
										 group by EmployeeId
										) x 
										inner join trn.Resignation rr on rr.EmployeeId=x.EmployeeId and rr.AddedDate=x.ad										
										) r on r.EmployeeId=e.SystemId
										left join hkp.SeparationType st on st.id=r.SeparationTypeId

--============
" + ZeroBody+@"
left join
(
					select ss.EmpInfoSystemID from
					(--date and emp
					select max(EffectiveDate) EffectiveDate,EmpInfoSystemID from
					(
					select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster
					where EffectiveDate<='" + sToDate + @"' and PlantId='" + sPlantID + @"'
					union
					select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster
					where EffectiveDate<='" + sToDate + @"' and PlantId='" + sPlantID + @"'
					) x

					group by EmpInfoSystemID
					) DE -------------date and emp
					left join
					(
					select systemid,EffectiveDate,EmpInfoSystemID,IsApproved from SalaryInfoDefineMaster where PlantId='" + sPlantID + @"'
					union
					select systemid,EffectiveDate,EmpInfoSystemID,IsApproved from SalaryInfoBackMaster where PlantId='" + sPlantID + @"'
					)
					ss on ss.EmpInfoSystemID=de.EmpInfoSystemID and ss.EffectiveDate=de.EffectiveDate
					where ss.IsApproved=0
) ssna on ssna.EmpInfoSystemID=e.SystemId
left join
(
select  EmpInfoSystemID,max(EffectiveDate)ed  from SalaryInfoDefineMaster where PlantId='" + sPlantID + @"'  group by EmpInfoSystemID 

) ssnd on ssnd.EmpInfoSystemID=e.SystemId

left join
(

select  EmpInfoSystemID,max(EffectiveDate)ed from SalaryInfoBackMaster where PlantId='" + sPlantID + @"' group by EmpInfoSystemID 
) ssnd2 on ssnd2.EmpInfoSystemID=e.SystemId
--===================LA/TBS
left join
(select systemid from EmployeeInformation 
where EmployeeStatus='Active' and EmployeeCurrentStatus in ('LONG ABSENTEEISM','TBS') and EmployeeCurrentStatusEffectiveDate<'" + sToDate + @"') os
on os.SystemId=e.SystemId
--============================approved
left join 
(
select EmpSystemId from SalaryLock where MonthNo=Month('" + sFromDate + @"' ) and YearNo=year('" + sFromDate + @"' )  and IsLocked=1
) aps on aps.EmpSystemId=e.SystemId
--===================

                                            LEFT OUTER JOIN
                                            (
                                            SELECT MAX(ToDate) ToDate, EmpInfoSystemID FROM
                                            (
                                            SELECT DISTINCT m.SystemID ,m.FromDate, m.ToDate, c.EmpInfoSystemID FROM SalaryProcMaster m
                                            LEFT OUTER JOIN SalaryProcChild C ON M.SystemID = C.SlrProcMstSystemID
                                            WHERE C.PlantID='" + sPlantID + @"' AND C.IsApproved=1
                                            ) X
                                            GROUP BY EmpInfoSystemID
                                            ) Y ON Y.EmpInfoSystemID = E.SystemId

                                                             WHERE 
e.dos between '" + sFromDate + @"'   and  '" + sToDate + @"' 
and isnull(ssna.EmpInfoSystemID,'')=''
and (isnull(ssnd2.EmpInfoSystemID,'')<>'' or isnull(ssnd.EmpInfoSystemID,'')<>'' )
"+ZeroWC+@"
and isnull(os.SystemID,'')=''
and isnull(aps.EmpSystemId,'')=''

                                                    and E.SystemId not in
										                (
														                select systemid from EmployeeInformation
													                left outer join
														                (
														                select max(ToDate) ToDate,EmpInfoSystemID from
														                (
														                select distinct m.SystemID,m.FromDate,m.ToDate,c.EmpInfoSystemID from SalaryProcMaster m
														                left outer join SalaryProcChild c on m.SystemID=c.SlrProcMstSystemID
														                where c.PlantID='" + sPlantID + @"'  and c.IsApproved=1
														                ) x
														                group by EmpInfoSystemID
														                ) y on y.EmpInfoSystemID=SystemId
													                 where
													                  EmployeeStatus = '" + bplib.clsWebLib.EmployeeStatus_Separated + @"' and
													                  (
													                  (dos>='" + sFromDate + @"' and  dos<='" + sToDate + @"')
														                and (y.ToDate is not null and dos<=y.ToDate)
													                  )
										                )--not in

                                                --Exception emp                                                         
                                                        and e.systemid not in
                                                        (
                                                        " + ExceptionEmpsForSP(sPlantID) + @"
                                                        )--Exception emp
                                                --MLV emp
                                                    and e.systemid not in 
                                                    (
                                                    " + MLVEmp_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                --MLV emp during
                                                 and e.systemid not in 
                                                    (
                                                    " + MLV_During_Emp_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                ";

                //if (sUserGroupID != "ALL")
                //{
                //    strSQL += @"
                //               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                //}
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }
                //if (Status != "")
                //{
                //    strSQL += @"
                //               AND E.EmployeeStatus = '" + Status + @"'";
                //}

                strSQL += @"
                            ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric desc --F.UserName,dgs.UserName,";

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
        public void SeparatedEmpZeroPresent(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)

        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                strSQL = @"SELECT IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')
                                                              ELSE Convert(bit, 'True') END,EmployeeCodePreFix,EmployeeCodeNumeric,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  DM.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = CASE WHEN Y.ToDate>'" + sFromDate + @"' THEN 'Overlap'
                                      WHEN Y.ToDate<'" + dtFD + @"' then 'Gap'
                                      ELSE 'OK' END,
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END,e.GivenDesignationId

                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        LEFT OUTER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DM.DesignationGroupID

--============
left join 
(
select EmpSystemID from AttdnDataMonthlySummary where YearNo=Year('" + sFromDate + @"') and MonthNo=Month('" + sFromDate + @"') and TotalPresent=0 and TotalLv=0 and TotalLate=0 and PlantID='" + sPlantID + @"'
) summ on summ.EmpSystemID=e.SystemId
left join
(
					select ss.EmpInfoSystemID from
					(--date and emp
					select max(EffectiveDate) EffectiveDate,EmpInfoSystemID from
					(
					select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster
					where EffectiveDate<='" + sToDate + @"' and PlantId='" + sPlantID + @"'
					union
					select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster
					where EffectiveDate<='" + sToDate + @"' and PlantId='" + sPlantID + @"'
					) x

					group by EmpInfoSystemID
					) DE -------------date and emp
					left join
					(
					select systemid,EffectiveDate,EmpInfoSystemID,IsApproved from SalaryInfoDefineMaster where PlantId='" + sPlantID + @"'
					union
					select systemid,EffectiveDate,EmpInfoSystemID,IsApproved from SalaryInfoBackMaster where PlantId='" + sPlantID + @"'
					)
					ss on ss.EmpInfoSystemID=de.EmpInfoSystemID and ss.EffectiveDate=de.EffectiveDate
					where ss.IsApproved=0
) ssna on ssna.EmpInfoSystemID=e.SystemId
left join
(
select  EmpInfoSystemID,max(EffectiveDate)ed  from SalaryInfoDefineMaster where PlantId='" + sPlantID + @"'  group by EmpInfoSystemID 

) ssnd on ssnd.EmpInfoSystemID=e.SystemId

left join
(

select  EmpInfoSystemID,max(EffectiveDate)ed from SalaryInfoBackMaster where PlantId='" + sPlantID + @"' group by EmpInfoSystemID 
) ssnd2 on ssnd2.EmpInfoSystemID=e.SystemId
--===================LA/TBS
left join
(select systemid from EmployeeInformation 
where EmployeeStatus='Active' and EmployeeCurrentStatus in ('LONG ABSENTEEISM','TBS') and EmployeeCurrentStatusEffectiveDate<'" + sToDate + @"') os
on os.SystemId=e.SystemId
--============================approved
left join 
(
select EmpSystemId from SalaryLock where MonthNo=Month('" + sFromDate + @"' ) and YearNo=year('" + sFromDate + @"' )
) aps on aps.EmpSystemId=e.SystemId
--===================

                                            LEFT OUTER JOIN
                                            (
                                            SELECT MAX(ToDate) ToDate, EmpInfoSystemID FROM
                                            (
                                            SELECT DISTINCT m.SystemID ,m.FromDate, m.ToDate, c.EmpInfoSystemID FROM SalaryProcMaster m
                                            LEFT OUTER JOIN SalaryProcChild C ON M.SystemID = C.SlrProcMstSystemID
                                            WHERE C.PlantID='" + sPlantID + @"' AND C.IsApproved=1
                                            ) X
                                            GROUP BY EmpInfoSystemID
                                            ) Y ON Y.EmpInfoSystemID = E.SystemId

                               WHERE 
(E.DOS >= '" + sFromDate + @"' and E.DOS <= '" + sToDate + @"' )
and isnull(ssna.EmpInfoSystemID,'')=''
and (isnull(ssnd2.EmpInfoSystemID,'')<>'' or isnull(ssnd.EmpInfoSystemID,'')<>'' )
and isnull(summ.EmpSystemID,'')<>''
and isnull(os.SystemID,'')=''
and isnull(aps.EmpSystemId,'')=''

                                                    and E.SystemId not in
										                (
														                select systemid from EmployeeInformation
													                left outer join
														                (
														                select max(ToDate) ToDate,EmpInfoSystemID from
														                (
														                select distinct m.SystemID,m.FromDate,m.ToDate,c.EmpInfoSystemID from SalaryProcMaster m
														                left outer join SalaryProcChild c on m.SystemID=c.SlrProcMstSystemID
														                where c.PlantID='" + sPlantID + @"'  and c.IsApproved=1
														                ) x
														                group by EmpInfoSystemID
														                ) y on y.EmpInfoSystemID=SystemId
													                 where
													                  EmployeeStatus = '" + bplib.clsWebLib.EmployeeStatus_Separated + @"' and
													                  (
													                  (dos>='" + sFromDate + @"' and  dos<='" + sToDate + @"')
														                and (y.ToDate is not null and dos<=y.ToDate)
													                  )
										                )--not in
                                                --Exception emp                                                         
                                                        and e.systemid not in
                                                        (
                                                        " + ExceptionEmpsForSP(sPlantID) + @"
                                                        )--Exception emp
                                                --MLV emp
                                                    and e.systemid not in 
                                                    (
                                                    " + MLVEmp_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                --MLV emp during
                                                 and e.systemid not in 
                                                    (
                                                    " + MLV_During_Emp_WC(sPlantID, sFromDate, sToDate) + @"
                                                    )
                                                ";

                
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }
                

                strSQL += @"
                            ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric desc ";

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

        
        public string ExceptionEmpsForSP(string sPlantID)
        {
            string strSQL = string.Empty;
            try
            {
                strSQL = @"select  e.systemid
                                    from ExceptionEmployee a
                                    inner join EmployeeInformation e on e.SystemId=a.EmpSystemID
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									left join hkp.Designation d on d.id=e.GivenDesignationId
		                            left join org.SubSection ss on ss.id=pr.SubSectionId         
									left join org.Section s on s.id=pr.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId
                                    where e.PlantId='" + sPlantID + @"' 									
									and a.[ExceptionCategory]='Salary Process'
									and a.IsActive=1
									and a.IsForever=1
                                                ";

                return strSQL;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function
       
        public void GetHRSettingForSeparatedZero(string sPlantID,  out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select  systemid from PlantWiseHRMSSetting where plantid='" + sPlantID + "' and ProcessSalaryForSeparatedWithZeroPresent=1";

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
        public string MLVEmp_WC(string sPlantID, string sFromDate, string sToDate)
        {
            string strSQL;
            try
            {
                //1,ToDate
                //strSQL = @"select EmpSystemID from AttdnProcessData where isnull(MaternityStatus,'')='MLV' and WorkDate between '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"'";
                strSQL = @"select EmpSystemID from LeaveTransaction where DATEADD(DAY,-1,FromDate) between 
                                        '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"' 
                                        and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')
                            and MaternityLeavePolicyId in (select Id from mst.MaternityLeavePolicy where IsMonthly=0)
							";
                return strSQL;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function 
        public string MLVReturn_WC(string sPlantID, string sFromDate, string sToDate)
        {
            string strSQL;
            try
            {
                strSQL = @"select EmpSystemID from LeaveTransaction where DATEADD(DAY,1,ToDate) between 
                                        '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"' 
                                        and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')
                            and MaternityLeavePolicyId in (select Id from mst.MaternityLeavePolicy where IsMonthly=0)
							";
                return strSQL;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function 
        public string MLV_During_Emp_WC(string sPlantID, string sFromDate, string sToDate)
        {
            string strSQL;
            try
            {
                strSQL = @"select EmpSystemID from LeaveTransaction where ('" + sFromDate + @"' between FromDate and ToDate )
												and ('" + sToDate + @"' between FromDate and ToDate)
												and PlantId='" + sPlantID + @"'  
                                        and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')
                                        and MaternityLeavePolicyId in (select Id from mst.MaternityLeavePolicy where IsMonthly=0)
							";
                return strSQL;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function


        ///mlv
        ///
        public void LoadMLV(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                string dtFDPrevM = Convert.ToDateTime(sFromDate).AddMonths(-1).ToString("dd-MMM-yyyy");
                strSQL = @"select * from
									(									
									select  e.systemid,e.EmployeeCode,e.systemid EmpSystemID
                                    ,e.EmployeeName 
									,t.BabyNo
                                    ,EmployeeCodePreFix,EmployeeCodeNumeric
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
                                    ,dgs.UserName GivenDesignation
                                    ,dd.UserName LegalDesignation
                                    ,s.UserName Section
                                    ,ss.UserName Subsection
                                    ,e.EmployeeStatus	
									,t.CG	LeaveStatus	
,IsLocked = case when isnull(sl.EmpSystemId,'')='' and isnull(k.EmpInfoSystemID,'')='' then 'YES'
										when isnull(sl.EmpSystemId,'')='' and isnull(k.EmpInfoSystemID,'')<>'' then 'NO'
										 else 'YES' end
																		
                                    from EmployeeInformation e
                                    left join SalaryLock sl on sl.EmpSystemId=e.SystemID and sl.MonthNo=Month('" + dtFD + "') and sl.YearNo=Year('" + dtFD + @"')  and IsLocked=1
left join (select distinct EmpInfoSystemID from SalaryProcChild where SlrProcMstSystemID in (select systemid from SalaryProcMaster where MonthNo=Month('" + dtFDPrevM + @"') and YearNo=Year('" + dtFDPrevM + @"'))) k on k.EmpInfoSystemID=e.SystemId

left join  mst.DesignationMasterLegalDesignation dl on dl.LegalDesignationId=e.LegalDesignationId
										left join mst.DesignationMaster dm on dm.id=dl.DesignationMasterId
										LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = dm.DesignationId

								left join 
(
select EmpSystemId from SalaryLock where MonthNo=Month('" + sFromDate + @"' ) and YearNo=year('" + sFromDate + @"' )  and IsLocked=1
) aps on aps.EmpSystemId=e.SystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
		                            left join org.SubSection ss on ss.id=PR.SubSectionId         
									left join org.Section s on s.id=PR.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId
									left join (
									select *,'Going' CG from LeaveTransaction where DATEADD(DAY,-1,FromDate) between 
                                         '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"' 
										 and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')
									) t on t.EmpSystemID=e.SystemId
									left join mst.MaternityLeavePolicy p on p.id=t.MaternityLeavePolicyId
                                    where e.PlantId='" + sPlantID + @"' 
and isnull(aps.EmpSystemId,'')=''
									and
									e.SystemId in
									(--mlv
									select EmpSystemID from LeaveTransaction where DATEADD(DAY,-1,FromDate) between 
                                        '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"'  
                                        and LTSystemID in (select Id from LeaveType where LeaveType='Maternity')
                                        and MaternityLeavePolicyId in (select Id from mst.MaternityLeavePolicy where IsMonthly=0)
									)--mlv

                                    
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
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									left join hkp.Designation d on d.id=e.GivenDesignationId
		                            left join org.SubSection ss on ss.id=PR.SubSectionId         
									left join org.Section s on s.id=PR.SectionId
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
    }
}
