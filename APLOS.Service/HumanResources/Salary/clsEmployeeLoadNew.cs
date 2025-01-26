using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace OTSBD
{
    public class clsEmployeeLoad
    {
        public void GetLocalLanguageLabel(string plantId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                        Select NameLabel,FatherNameLabel,MotherNameLabel,SpouseNameLabel,IdentificationMarksLabel,NomineeLabel,AddressLabel,LandLabel,MobileNoLabel,PAddressLabel,PermanentLabel 
                        ,L.DependantLabel, M.LeaveLabel, N.DesignationLabel FROM ORG.Plant P
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
                        WHERE P.Id='" + plantId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void PlantWiseDOJDays(string plantId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT  PastDOJDaysAllowed FROM dbo.PlantWiseHRMSSetting WHERE PlantId='" + plantId + @"' AND IsPastDOJAllowed=1";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public clsEmployeeLoad()
        {
            // TODO: Add constructor logic here
        }

        public void GetNextDueDate(string sGroupID, string strPlantID, string EmpSystemId, string NextDueDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SalaryIncrementNextDueDate WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + strPlantID + "' AND EmpSystemId = '" + EmpSystemId + @"' and NextDueDate='" + NextDueDate + "'";

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

        public bool DuplicateCardNumber(string strSystemID, string strCardNumber, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            //System.Data.DataSet dsRef = null;

            bool blnStatus = false;

            try
            {
                strSql = @"SELECT * FROM EmployeeInformation
                           WHERE (SystemID <> '" + strSystemID + @"') AND (CardNumber = '" + strCardNumber + "') AND EmployeeStatus = 'Active'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
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
       

        public bool DuplicateEmployeeCode(string sGroupID, string strCompanyID, string strPlantID, string strSystemID, string strEmpCode, string EmployeeCodeCheckLevel)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            System.Data.DataSet dsRef = null;

            bool blnStatus = false;

            try
            {
               
                if (EmployeeCodeCheckLevel== "Plant")
                {
                    strSql = @"SELECT * FROM EmployeeInformation  WHERE  PlantID = '" + strPlantID + @"' AND (SystemID <> '" + strSystemID + @"') AND (EmployeeCode = '" + strEmpCode + "')";
                }
                else if (EmployeeCodeCheckLevel == "Company")
                {
                    strSql = @"SELECT * FROM EmployeeInformation  WHERE  CompanyID = '" + strCompanyID + @"' AND (SystemID <> '" + strSystemID + @"') AND (EmployeeCode = '" + strEmpCode + "')";
                }
                else
                {
                    strSql = @"SELECT * FROM EmployeeInformation  WHERE GroupID = '" + sGroupID + @"'  AND (SystemID <> '" + strSystemID + @"') AND (EmployeeCode = '" + strEmpCode + "')";
                }

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
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

        public bool DuplicateEmployeeCodeWithInGroup(string strPlantID, string strSystemID, string strEmpCode, string EmployeeCodeTypeId)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            System.Data.DataSet dsRef = null;
            bool blnStatus = false;

            try
            {
                strSql = @"SELECT * FROM EmployeeInformation A 
                                        WHERE EXISTS (SELECT * FROM EmployeeCodeGenGroupDetail B WHERE A.PlantId=B.PlantId AND A.EmployeeCodeTypeId=B.EmployeeCodeTypeId 
                                        AND EmployeeCodeGenGroupId=(SELECT EmployeeCodeGenGroupId FROM EmployeeCodeGenGroupDetail WHERE PlantId='"+ strPlantID + @"' AND EmployeeCodeTypeId='"+ EmployeeCodeTypeId + @"'))
                                        AND (SystemID <> '"+ strSystemID + @"') AND (EmployeeCode = '"+ strEmpCode + "')";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
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

        public bool FountAttdnProc(string strSystemID)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            System.Data.DataSet dsRef = null;

            bool blnStatus = false;

            try
            {
                strSql = @"SELECT COUNT(*) AttdnProcessDays FROM AttdnProcessData WHERE EmpSystemID = '" + strSystemID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                if (Convert.ToInt32(dsRef.Tables[0].Rows[0]["AttdnProcessDays"].ToString().Trim()) == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
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

        public void LoadShiftCbo(string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM ShiftDefination WHERE IsActive = 1 AND PlantID = '" + sPlantID + "'";

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

        public void LoadDesGCbo(string sGroupID, string strCompanyID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM HKP.DesignationGroup
                                    WHERE Id IN (SELECT DesignationGroupID
                                    FROM [HKP].[CompanyGroupDesignationGroup]  WHERE CompanyGroupId = '" + sGroupID + "' )";

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

        public void LoadEmpSalaryProcGrid(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)

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
                                  , DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
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
                                        LEFT JOIN MST.DesignationMaster DEM ON DEM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DEM.DesignationGroupID

--============
left join 
(
select EmpSystemID from AttdnDataMonthlySummary where YearNo=Year('" + sFromDate + @"') and MonthNo=Month('" + sFromDate + @"') and TotalPresent=0 and TotalLv=0 and TotalLate=0 and PlantID='"+ sPlantID + @"'
) summ on summ.EmpSystemID=e.SystemId
left join
(
					select ss.EmpInfoSystemID from
					(--date and emp
					select max(EffectiveDate) EffectiveDate,EmpInfoSystemID from
					(
					select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster
					where EffectiveDate<='"+ sToDate +@"' and PlantId='" + sPlantID + @"'
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
select EmpInfoSystemID from SalaryInfoDefineMaster where PlantId='" + sPlantID + @"'
union
select EmpInfoSystemID from SalaryInfoBackMaster where PlantId='" + sPlantID + @"'
) ssnd on ssnd.EmpInfoSystemID=e.SystemId
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

                               WHERE E.DOJ <= '" + sToDate + @"' AND (E.DOS >= '" + sFromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                        OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')

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
                                                   -- and e.systemid not in 
                                                   -- (
                                                   -- select EmpSystemID from AttdnDataMonthlySummary where  YearNo=Year('" + sFromDate + @"') and MonthNo=Month('" + sFromDate + @"') and TotalPresent=0 and TotalLv=0 and TotalLate=0  and PlantID='" + sPlantID + @"'
                                                   -- )
                                                  
                                                and  e.SystemId not in (select systemid from EmployeeInformation where EmployeeStatus='Active' and EmployeeCurrentStatus in (" + bplib.clsWebLib.EMP_OTHER_STATUS + @") and EmployeeCurrentStatusEffectiveDate<'" + sToDate + @"')
                                               ------------processed salary approved---------------------
                                                and E.SystemId not in
										                (														                
							                          SELECT sc.EmpInfoSystemID FROM SalaryProcChild SC	WHERE (IsApproved = 1 or IsDisbursed = 1)
															and SlrProcMstSystemID in (SELECT systemid FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"') and plantid='" + sPlantID + @"')

										                )--not in
                                                --ssnot defined
												-- and  E.SystemId  in
										               -- (
														                
															--select EmpInfoSystemID from SalaryInfoDefineMaster where PlantId='" + sPlantID + @"'
															--union
															--select EmpInfoSystemID from SalaryInfoBackMaster where PlantId='" + sPlantID + @"'

										              --  )-- in
                                                --Approved SP
                                                        and e.systemid not in
                                                        (
                                                        SELECT distinct SC.EmpInfoSystemID
                                                        FROM SalaryProcChild SC
                                                        INNER JOIN (SELECT SystemID FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
                                                        WHERE IsApproved = 1
                                                        )--Approved SP

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

        
        public void LoadEmployeeInfoNew(CustomParaNew _para, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT * FROM
                                        (

                                            SELECT E.SystemID
	                                            , E.EmployeeCode
	                                            , E.EmployeeName
	                                            , E.GroupID
	                                            , GC.StandardName GroupName
	                                            , E.CompanyID
	                                            , CMP.StandardName CompanyName
	                                            , E.PlantID
	                                            , Pt.StandardName PlantName
	                                            , REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB
	                                            , REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ
	                                            , E.EmployeeStatus
	                                            , E.EmployeeCategorySystemID
	                                            , EC.StandardName EmpCategoryName
	                                            , DG.StandardName DesignationGroupName
	                                            , E.LVPolicyMasterSystemID
	                                            , DGM.LeavePolicyMasterId
	                                            , LPM.PolicyName LeavePolicyName
	                                            , E.SalaryRuleMasterSystemID
	                                            , SRM.SalaryRuleName
	                                            , R.EmployeeStatue ResignStatue
	                                            , E.EmployeeGroupSystemID
	                                            , E.JobLocationID
	                                            , ISNULL(E.IsConfirmed, 0) IsConfirmed
	                                            , JbLc.JobLocation
	                                            , SRM.CurrencyRuleSystemID
	                                            , pmb.Code BudgetCodeName
	                                            , E.SalaryPercentage
	                                            , E.BudgetCode
	                                            , E.GivenDesignationId
	                                            , E.LegalDesignationId
	                                            , Dsgg.UserName GivenDesignation
	                                            , tge.TaxGroupID TaxGrpEmpSystemID
	                                            , tgr.TaxGroupName
	                                            , dgSRM.TaxGroupID TaxGroupIDSR
	                                            , REPLACE(Convert(VARCHAR(11), SSA.EffectiveDate, 106), ' ', '-') AS EffectiveDate
                                            FROM EmployeeInformation AS E
						                            LEFT OUTER JOIN   [HKP].[EmployeeBudgetCategory] EBC ON E.BudgetCategoryID = EBC.ID
						                            LEFT OUTER JOIN   [ORG].[CompanyGroup] GC ON E.GroupID = GC.ID
						                            LEFT OUTER JOIN   [ORG].[Company] CMP ON E.CompanyID = CMP.ID
						                            LEFT OUTER JOIN   [ORG].Plant Pt ON E.PlantID = Pt.ID
						                            LEFT OUTER JOIN    [MST].[ManpowerBudget] pmb on e.BudgetCode=pmb.Id
						                            LEFT OUTER JOIN [HKP].[EmployeeCategory] AS EC ON E.EmployeeCategorySystemID = EC.ID
						                           LEFT JOIN MST.DesignationMaster DEM ON DEM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DEM.DesignationGroupID
                                                    LEFT OUTER JOIN  (               SELECT DC.SalaryRuleMasterId,dc.PlantId,dm.*,dc.LeavePolicyMasterId 
                                                                                FROM MST.DesignationMaster DM
							                                                    LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                                                            ON DM.Id=DC.DesignationMasterId
                                                    ) DGM ON E.GivenDesignationID = DGM.DesignationId AND E.PlantId=DGM.PlantId
													LEFT OUTER JOIN 	[HKP].Designation AS Dsgg ON Dsgg.ID = E.GivenDesignationID
						                            LEFT OUTER JOIN  SalaryRuleMaster dgSRM ON DGM.SalaryRuleMasterId = dgSRM.SystemID
                                                    LEFT OUTER JOIN 	[dbo].[TaxGroupTagWithEmployee] tge on tge.EmpInfoSystemID=E.SystemId
						                            LEFT OUTER JOIN    LeavePolicyMaster LPM ON DGM.LeavePolicyMasterId = LPM.SystemID
						                            LEFT OUTER JOIN    SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                                    LEFT OUTER JOIN  TaxGroup tgr ON SRM.TaxGroupID = tgr.SystemID
                                                    LEFT OUTER JOIN   JobLocation JbLc ON E.JobLocationID = JbLc.SystemID
													left outer join
													(
														select EmpInfoSystemID, max(EffectiveDate) EffectiveDate from
														(
														select EmpInfoSystemID,max(EffectiveDate) EffectiveDate from SalaryInfoBackMaster where IsApproved=1 group by EmpInfoSystemID
														union
														select EmpInfoSystemID,max(EffectiveDate) EffectiveDate from SalaryInfoDefineMaster where IsApproved=1 group by EmpInfoSystemID
														) x group by EmpInfoSystemID
													) SSA on SSA.EmpInfoSystemID=E.SystemId
                                                    LEFT OUTER JOIN
									                           [HKP].[EmployeeGroup]  EG ON E.EmployeeGroupSystemID= EG.Id
						                            LEFT OUTER JOIN
									                            (
                                                                 SELECT TOP (1) * FROM SalaryIncrementInfoDefineMaster WHERE IsApproved = 0
                                                                ) SID ON E.SystemID = SID.EmpInfoSystemID
                                                    LEFT OUTER JOIN
									                            Resign R ON E.SystemID = R.EmpInfoSystemID AND IsEffected = 1
							         WHERE E.GroupID = '" + _para.CompanyGroupId + @"'
									 AND E.CompanyID = '" + _para.CompanyId + @"'
                                              AND E.PlantID = '" + _para.PlantId + @"'
											  --and E.Isapproved=1
											  and E.EmployeeStatus  !='Separated'
											  --and isnull(E.SalaryRuleMasterSystemID,'')<>''
											  --and isnull(dgSRM.TaxGroupID,'')<>''
											  --and E.SalaryRuleMasterSystemID='" + _para.SalaryRuleId + @"'
                                              and E.systemid in ('" + _para.EmployeeId + @"')
											  ) A
                         ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void LoadEmployeeInfoForIncrementNew(CustomParaNew _para, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT * FROM
                                        (

                                            SELECT E.SystemID
	                                            , E.EmployeeCode
	                                            , E.EmployeeName
	                                            , E.GroupID
	                                            , GC.StandardName GroupName
	                                            , E.CompanyID
	                                            , CMP.StandardName CompanyName
	                                            , E.PlantID
	                                            , Pt.StandardName PlantName
	                                            , REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB
	                                            , REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ
	                                            , E.EmployeeStatus
	                                            , E.EmployeeCategorySystemID
	                                            , EC.StandardName EmpCategoryName
	                                            , DG.StandardName DesignationGroupName
	                                            , E.LVPolicyMasterSystemID
	                                            , DGM.LeavePolicyMasterId
	                                            , LPM.PolicyName LeavePolicyName
	                                            , E.SalaryRuleMasterSystemID
	                                            , SRM.SalaryRuleName
	                                            , R.EmployeeStatue ResignStatue
	                                            , E.EmployeeGroupSystemID
	                                            , E.JobLocationID
	                                            , ISNULL(E.IsConfirmed, 0) IsConfirmed
	                                            , JbLc.JobLocation
	                                            , SRM.CurrencyRuleSystemID
	                                            , pmb.Code BudgetCodeName
	                                            , E.SalaryPercentage
	                                            , E.BudgetCode
	                                            , E.GivenDesignationId
	                                            , E.LegalDesignationId
	                                            , Dsgg.UserName GivenDesignation
	                                            , tge.TaxGroupID TaxGrpEmpSystemID
	                                            , tgr.TaxGroupName
	                                            , dgSRM.TaxGroupID TaxGroupIDSR
	                                            , REPLACE(Convert(VARCHAR(11), SSA.EffectiveDate, 106), ' ', '-') AS EffectiveDate
                                            FROM EmployeeInformation AS E
						                            LEFT OUTER JOIN   [HKP].[EmployeeBudgetCategory] EBC ON E.BudgetCategoryID = EBC.ID
						                            LEFT OUTER JOIN   [ORG].[CompanyGroup] GC ON E.GroupID = GC.ID
						                            LEFT OUTER JOIN   [ORG].[Company] CMP ON E.CompanyID = CMP.ID
						                            LEFT OUTER JOIN   [ORG].Plant Pt ON E.PlantID = Pt.ID
						                            LEFT OUTER JOIN    [MST].[ManpowerBudget] pmb on e.BudgetCode=pmb.Id
						                            LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=E.GivenDesignationID
                                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
						                            LEFT OUTER JOIN   [HKP].[DesignationGroup] DG ON DM.DesignationGroupID = DG.ID
                                                    LEFT OUTER JOIN  (               SELECT DC.SalaryRuleMasterId,dc.PlantId,dm.*,dc.LeavePolicyMasterId 
                                                                                FROM MST.DesignationMaster DM
							                                                    LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                                                            ON DM.Id=DC.DesignationMasterId
                                                    ) DGM ON E.GivenDesignationID = DGM.DesignationId AND E.PlantId=DGM.PlantId
													LEFT OUTER JOIN 	[HKP].Designation AS Dsgg ON Dsgg.ID = E.GivenDesignationID
						                            LEFT OUTER JOIN  SalaryRuleMaster dgSRM ON DGM.SalaryRuleMasterId = dgSRM.SystemID
                                                    LEFT OUTER JOIN 	[dbo].[TaxGroupTagWithEmployee] tge on tge.EmpInfoSystemID=E.SystemId
						                            LEFT OUTER JOIN    LeavePolicyMaster LPM ON DGM.LeavePolicyMasterId = LPM.SystemID
						                            LEFT OUTER JOIN    SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                                    LEFT OUTER JOIN  TaxGroup tgr ON SRM.TaxGroupID = tgr.SystemID
                                                    LEFT OUTER JOIN   JobLocation JbLc ON E.JobLocationID = JbLc.SystemID
													left outer join
													(
														select EmpInfoSystemID, max(EffectiveDate) EffectiveDate from
														(
														select EmpInfoSystemID,max(EffectiveDate) EffectiveDate from SalaryInfoBackMaster where IsApproved=1 group by EmpInfoSystemID
														union
														select EmpInfoSystemID,max(EffectiveDate) EffectiveDate from SalaryInfoDefineMaster  group by EmpInfoSystemID
														) x group by EmpInfoSystemID
													) SSA on SSA.EmpInfoSystemID=E.SystemId
                                                    LEFT OUTER JOIN
									                           [HKP].[EmployeeGroup]  EG ON E.EmployeeGroupSystemID= EG.Id
						                            LEFT OUTER JOIN
									                            (
                                                                 SELECT TOP (1) * FROM SalaryIncrementInfoDefineMaster WHERE IsApproved = 0
                                                                ) SID ON E.SystemID = SID.EmpInfoSystemID
                                                    LEFT OUTER JOIN
									                            Resign R ON E.SystemID = R.EmpInfoSystemID AND IsEffected = 1
							         WHERE E.GroupID = '" + _para.CompanyGroupId + @"'
									 AND E.CompanyID = '" + _para.CompanyId + @"'
                                              AND E.PlantID = '" + _para.PlantId + @"'
											  --and E.Isapproved=1
											  and E.EmployeeStatus !='Separated'
											  --and isnull(E.SalaryRuleMasterSystemID,'')<>''
											  --and isnull(dgSRM.TaxGroupID,'')<>''
											  --and E.SalaryRuleMasterSystemID='" + _para.SalaryRuleId + @"'
                                              and E.systemid in ('" + _para.EmployeeId + @"')
											  ) A
                         ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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



        public void GetSalaryRuleIdFromSavedSalaryData(string  EmpSyatemId, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @" ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void LoadEmployeeInfoForIncrement(CustomPara _para, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT * FROM
                                        (

                                            SELECT E.SystemID
	                                            , E.EmployeeCode
	                                            , E.EmployeeName
	                                            , E.GroupID
	                                            , GC.StandardName GroupName
	                                            , E.CompanyID
	                                            , CMP.StandardName CompanyName
	                                            , E.PlantID
	                                            , Pt.StandardName PlantName
	                                            , REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB
	                                            , REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ
	                                            , E.EmployeeStatus
	                                            , E.EmployeeCategorySystemID
	                                            , EC.StandardName EmpCategoryName
	                                            , DG.StandardName DesignationGroupName
	                                            , E.LVPolicyMasterSystemID
	                                            , DGM.LeavePolicyMasterId
	                                            , LPM.PolicyName LeavePolicyName
	                                            , E.SalaryRuleMasterSystemID
	                                            , SRM.SalaryRuleName
	                                            , R.EmployeeStatue ResignStatue
	                                            , E.EmployeeGroupSystemID
	                                            , E.JobLocationID
	                                            , ISNULL(E.IsConfirmed, 0) IsConfirmed
	                                            , JbLc.JobLocation
	                                            , SRM.CurrencyRuleSystemID
	                                            , pmb.Code BudgetCodeName
	                                            , E.SalaryPercentage
	                                            , E.BudgetCode
	                                            , E.GivenDesignationId
	                                            , E.LegalDesignationId
	                                            , Dsgg.UserName GivenDesignation
	                                            , tge.TaxGroupID TaxGrpEmpSystemID
	                                            , tgr.TaxGroupName
	                                            , dgSRM.TaxGroupID TaxGroupIDSR
	                                            , REPLACE(Convert(VARCHAR(11), SSA.EffectiveDate, 106), ' ', '-') AS EffectiveDate
                                            FROM EmployeeInformation AS E
						                            LEFT OUTER JOIN   [HKP].[EmployeeBudgetCategory] EBC ON E.BudgetCategoryID = EBC.ID
						                            LEFT OUTER JOIN   [ORG].[CompanyGroup] GC ON E.GroupID = GC.ID
						                            LEFT OUTER JOIN   [ORG].[Company] CMP ON E.CompanyID = CMP.ID
						                            LEFT OUTER JOIN   [ORG].Plant Pt ON E.PlantID = Pt.ID
						                            LEFT OUTER JOIN    [MST].[ManpowerBudget] pmb on e.BudgetCode=pmb.Id
						                            LEFT OUTER JOIN [HKP].[EmployeeCategory] AS EC ON E.EmployeeCategorySystemID = EC.ID
						                            LEFT JOIN MST.DesignationMaster DEM ON DEM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DEM.DesignationGroupID
                                                    LEFT OUTER JOIN  (               SELECT DC.SalaryRuleMasterId,dc.PlantId,dm.*,dc.LeavePolicyMasterId 
                                                                                FROM MST.DesignationMaster DM
							                                                    LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                                                            ON DM.Id=DC.DesignationMasterId
                                                    ) DGM ON E.GivenDesignationID = DGM.DesignationId AND E.PlantId=DGM.PlantId
													LEFT OUTER JOIN 	[HKP].Designation AS Dsgg ON Dsgg.ID = E.GivenDesignationID
						                            LEFT OUTER JOIN  SalaryRuleMaster dgSRM ON DGM.SalaryRuleMasterId = dgSRM.SystemID
                                                    LEFT OUTER JOIN 	[dbo].[TaxGroupTagWithEmployee] tge on tge.EmpInfoSystemID=E.SystemId
						                            LEFT OUTER JOIN    LeavePolicyMaster LPM ON DGM.LeavePolicyMasterId = LPM.SystemID
						                            LEFT OUTER JOIN    SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                                    LEFT OUTER JOIN  TaxGroup tgr ON SRM.TaxGroupID = tgr.SystemID
                                                    LEFT OUTER JOIN   JobLocation JbLc ON E.JobLocationID = JbLc.SystemID
													left outer join
													(
														select EmpInfoSystemID, max(EffectiveDate) EffectiveDate from
														(
														select EmpInfoSystemID,max(EffectiveDate) EffectiveDate from SalaryInfoBackMaster where IsApproved=1 group by EmpInfoSystemID
														union
														select EmpInfoSystemID,max(EffectiveDate) EffectiveDate from SalaryInfoDefineMaster  group by EmpInfoSystemID
														) x group by EmpInfoSystemID
													) SSA on SSA.EmpInfoSystemID=E.SystemId
                                                    LEFT OUTER JOIN
									                           [HKP].[EmployeeGroup]  EG ON E.EmployeeGroupSystemID= EG.Id
						                            LEFT OUTER JOIN
									                            (
                                                                 SELECT TOP (1) * FROM SalaryIncrementInfoDefineMaster WHERE IsApproved = 0
                                                                ) SID ON E.SystemID = SID.EmpInfoSystemID
                                                    LEFT OUTER JOIN
									                            Resign R ON E.SystemID = R.EmpInfoSystemID AND IsEffected = 1
							         WHERE E.GroupID = '" + _para.CompanyGroupId + @"'
									 AND E.CompanyID = '" + _para.CompanyId + @"'
                                              AND E.PlantID = '" + _para.PlantId + @"'
											  --and E.Isapproved=1
											  and E.EmployeeStatus !='Separated'
											  --and isnull(E.SalaryRuleMasterSystemID,'')<>''
											  --and isnull(dgSRM.TaxGroupID,'')<>''
											  --and E.SalaryRuleMasterSystemID='" + _para.SalaryRuleId + @"'
                                              and E.systemid in ('" + _para.EmployeeId + @"')
											  ) A
                         ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetEmployeeEligibleForSalaryHeadEnum(string sGroupID, string strPlantID, string EmpID, string SSId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeEligibleForSalaryHeadEnum
                                        WHERE  SalaryStructureId = '" + SSId + "' AND  CompanyGroupId = '" + sGroupID + "' AND  PlantId = '" + strPlantID + "'AND  EmpSystemId = '" + EmpID + "'";

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
        public void GetPFEmployeeVoluntaryValue(string EmpID, string SalaryStructureId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *    FROM [dbo].[PFEmployeeVoluntaryValue] WHERE EmpSystemId='" + EmpID + @"' AND SalaryStructureId='" + SalaryStructureId + @"'";

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
        public void GetSettingPlantWiseNew(CustomParaNew _para, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select c.CutOffDate,p.* from PlantWiseHRMSSetting p
                                    left join  (select * from[SCS].[OpeningBalanceCutOffDate] where ModuleName = 'HR')c on c.PlantId = p.PlantID
                                    where p.PlantID = '" + _para.PlantId + "'";
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

        public void LoadUnapprovedSStructure(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
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
                                  DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = CASE WHEN Y.ToDate>'" + sFromDate + @"' THEN 'Overlap'
                                      WHEN Y.ToDate<'" + dtFD + @"' then 'Gap'
                                      ELSE 'OK' END,
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END

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
                                        LEFT JOIN MST.DesignationMaster DEM ON DEM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DEM.DesignationGroupID

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

                               WHERE E.DOJ <= '" + sToDate + @"' AND (E.DOS >= '" + sFromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                        OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')

                                                    and E.SystemId in
										                (														                
							                                ( 
																	select ss.EmpInfoSystemID from 
																				 (--date and emp
																				 select max(EffectiveDate) EffectiveDate,EmpInfoSystemID from
																				 (
																				 select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster
																				 where EffectiveDate<='" + sToDate + @"'  and  PlantId='" + sPlantID + @"'
																				 union
																				 select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster
																				 where EffectiveDate<='" + sToDate + @"' and  PlantId='" + sPlantID + @"'
																				 ) x

																				 group by EmpInfoSystemID
																				 ) DE -------------date and emp
																				 left join 
																				 (
																				 select systemid,EffectiveDate,EmpInfoSystemID,IsApproved from SalaryInfoDefineMaster  where PlantId='" + sPlantID + @"'
																				 union 
																				 select systemid,EffectiveDate,EmpInfoSystemID,IsApproved from SalaryInfoBackMaster  where PlantId='" + sPlantID + @"'
																				 )
																				  ss on ss.EmpInfoSystemID=de.EmpInfoSystemID and ss.EffectiveDate=de.EffectiveDate
																				  where ss.IsApproved=0
																	)

										                )--not in
                                    and e.EmployeeStatus in ('Separated','Active')
							   	---------------present zero--------------
                                and e.systemid not in 
                                (
                                select EmpSystemID from AttdnDataMonthlySummary where  YearNo=Year('" + sFromDate + @"') and MonthNo=Month('" + sFromDate + @"') and TotalPresent=0 and TotalLv=0 and TotalLate=0  and PlantID='" + sPlantID + @"'
                                )
                                --Approved SP
                                                        and e.systemid not in
                                                        (
                                                        SELECT distinct SC.EmpInfoSystemID
                                                        FROM SalaryProcChild SC
                                                        INNER JOIN (SELECT SystemID FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
                                                        WHERE IsApproved = 1
                                                        )--Approved SP
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
                                                ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }


                strSQL += @"
                            ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric --F.UserName,dgs.UserName,";

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
        }
        public void LoadSeparatedButnotApproved(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
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
                                  DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = CASE WHEN Y.ToDate>'" + sFromDate + @"' THEN 'Overlap'
                                      WHEN Y.ToDate<'" + dtFD + @"' then 'Gap'
                                      ELSE 'OK' END,
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END

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
LEFT JOIN MST.DesignationMaster DEM ON DEM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DEM.DesignationGroupID
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

                               WHERE (E.EmployeeStatus = 'Active')

                                                    and E.SystemId in
										                (														                
							                                select EmployeeId from trn.Resignation where ApprovedEffectiveDate between '" + sFromDate + @"' and '" + sToDate + @"' 
                                                            and ApprovalStatus='Approved'

										                )--not in
                                                ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }


                strSQL += @"
                            ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric --F.UserName,dgs.UserName,";

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
        }
        public void LoadNotDefinedSS(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
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
                                  DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation, e.GivenDesignationId,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = CASE WHEN Y.ToDate>'" + sFromDate + @"' THEN 'Overlap'
                                      WHEN Y.ToDate<'" + dtFD + @"' then 'Gap'
                                      ELSE 'OK' END,
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END

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
                                        LEFT JOIN MST.DesignationMaster DEM ON DEM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DEM.DesignationGroupID

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

                                WHERE   e.EmployeeStatus in ('Separated','Active')
							   and e.doj  <='" + sToDate + @"' and 
							   (e.EmployeeStatus='Active' or (e.EmployeeStatus='Separated' and e.dos>='" + sFromDate + @"' ))

                                                     and  E.SystemId not in
										                (
														                
															select EmpInfoSystemID from SalaryInfoDefineMaster   where PlantID='" + sPlantID + @"'
															union
															select EmpInfoSystemID from SalaryInfoBackMaster where PlantID='" + sPlantID + @"'

										                )--not in
	                            ---------------present zero--------------
                                and e.systemid not in 
                                (
                                select EmpSystemID from AttdnDataMonthlySummary where  YearNo=Year('" + sFromDate + @"') and MonthNo=Month('" + sFromDate + @"') and TotalPresent=0 and TotalLv=0 and TotalLate=0  and PlantID='" + sPlantID + @"'
                                )
                                            --Approved SP
                                                        and e.systemid not in
                                                        (
                                                        SELECT distinct SC.EmpInfoSystemID
                                                        FROM SalaryProcChild SC
                                                        INNER JOIN (SELECT SystemID FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
                                                        WHERE IsApproved = 1
                                                        )--Approved SP

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
                                                ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }


                strSQL += @"
                            ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric --F.UserName,dgs.UserName,";

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
        }
        public void xLoadUnapprovedSStructure(string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                //         strSql = @"SELECT EmpInfoSystemID
                //                          ,SystemID 
                //                     FROM SalaryInfoDefineMaster
                //                     WHERE IsApproved=0 AND PlantId = '" + sPlantID + @"' AND GroupId='" + GroupId + @"'												
                //                     GROUP BY PlantID, GroupID, SystemID
                //HAVING MAX(EffectiveDate) BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void FindEmployeeBasicInfo(string CompanyID, string strUserGroupID, string strEmpCode, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                if (strKey.Trim() == "")
                {
                    strSql = @"SELECT E.SystemID, E.EmployeeCode, E.EmployeeName, DG.DesignationGroupDesc DesignationGroup
	                                FROM EmployeeInformation E
		                                LEFT JOIN
			                                HKP.DesignationGroup DG ON E.DesignationGroupID = DG.DesignationGroupID
                                WHERE E.CompanyID = '" + CompanyID + "'";
                }
                else
                {
                    strSql = @"SELECT E.SystemID, E.EmployeeCode, E.EmployeeName, DG.DesignationGroupDesc DesignationGroup
	                                FROM EmployeeInformation E
		                                LEFT JOIN
			                                HKP.DesignationGroup DG ON E.DesignationGroupID = DG.DesignationGroupID
                                WHERE E.CompanyID = '" + CompanyID + @"' AND " + strKey + @"";
                }

                strSql = strSql + " AND E.UserGroupSystemID = '" + strUserGroupID + @"' ORDER BY E.EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public bool CheckEmployeeBasicInfo(string sGroupID, string strCompanyID, string strEmpID, string strEmpCode)
        {
            System.Data.DataSet dsRef = null;
            string strSQl;
            bool blnStatus = false;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQl = @"SELECT * FROM EmployeeInformation
                                    WHERE GroupID = '" + sGroupID + @"' AND CompanyID = '" + strCompanyID + @"'
                                                AND SystemID != '" + strEmpID + @"' AND EmployeeCode = '" + strEmpCode + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void GetCountryByPlant(string sPlantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select p.id,am.CountryId,c.UserName country from org.plant p
                                left join mst.AddressMaster am on p.AddressMasterId=am.id
                                left join scs.country c on c.id=am.countryid
                                where p.id='" + sPlantid + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function
        public int GetCountedEmp(DataGrid dg)
        {
            int _r = 0;
            try
            {
                _r = 0;
                for (int i = 0; i < dg.Items.Count; i++)
                {
                    CheckBox chkBox = (CheckBox)dg.Items[i].FindControl("chkSelectSlrProc");
                    if (chkBox.Checked)
                    {
                        _r++;
                    }
                }
                return _r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<string> GetEmpList(DataSet ds, Boolean IsSelected)
        {
            List<string> _r = new List<string>();
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    string chkBox = ds.Tables[0].Rows[i]["IsSelectSlrProc"].ToString();
                    if (bplib.clsWebLib.GetBoolData(chkBox) == IsSelected)
                    {
                        string _EmpSystemID = ds.Tables[0].Rows[i]["EmpSystemID"].ToString();
                        ///Convert.ToBoolean(dsGrid.Tables[0].Rows[GrdEmp]["IsSelectSlrProc"].ToString().Trim())
                        ///dsGrid.Tables[0].Rows[GrdEmp]["EmpSystemID"].ToString().Trim()
                        _r.Add(_EmpSystemID);
                    }
                }
                return _r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetBankInfo(string sPlantid, string empids, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select distinct e.systemid,e.EmployeeCode,e.PaymentMode 
                                    ,b.EmpSystemID,b.BankAccNo,b.IsApproved 
                                    ,Remark=case when (isnull(e.PaymentMode,'')='Bank' or isnull(e.PaymentMode,'')='Transfer') and ISNULL(b.BankAccNo,'')='' then 'Bank Acc is required'
									--when (isnull(e.PaymentMode,'')<>'Bank' and isnull(e.PaymentMode,'')<>'Transfer') and ISNULL(b.BankAccNo,'')<>'' then 'Payment mode is not valid'
									when (isnull(e.PaymentMode,'')='Bank' or isnull(e.PaymentMode,'')='Transfer') and ISNULL(b.BankAccNo,'')<>'' and b.IsApproved=0 then 'Bank Acc Approval required'
									else 'OK' end
                                    from EmployeeInformation e
                                    left join EmployeeBankInfo b on e.SystemId=b.EmpSystemID
                                    where e.PlantId='" + sPlantid + @"' and 
                                    (--plant
                                    ((isnull(e.PaymentMode,'')='Bank' or isnull(e.PaymentMode,'')='Transfer') and ISNULL(b.BankAccNo,'')='') 
                                    --or ((isnull(e.PaymentMode,'')<>'Bank' and isnull(e.PaymentMode,'')<>'Transfer') and ISNULL(b.BankAccNo,'')<>'') 
                                    or ((isnull(e.PaymentMode,'')='Bank' or isnull(e.PaymentMode,'')='Transfer')  and ISNULL(b.BankAccNo,'')<>'' and b.IsApproved=0) 
                                    )--plant
                                    and e.SystemID in (" + empids + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void GetAttendanceLockInfo(string sPlantid, string fdate, string tdate, string empids, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select  e.systemid,e.EmployeeCode
                                    ,e.EmployeeName ,
                                    EmployeeCodePreFix,EmployeeCodeNumeric
                                    ,format(e.DOJ,'dd-MMM-yyyy') DOJ
                                    ,format(e.DOS,'dd-MMM-yyyy') DOS
                                    ,d.UserName GivenDesignation
                                    ,format(a.workdate,'dd-MMM-yyyy') workdate
                                    ,dd.UserName LegalDesignation
                                    ,s.UserName Section
                                    ,ss.UserName Subsection
                                    ,e.EmployeeStatus
                                    from ExceptionEmployeeAttendanceUnlock a
                                    inner join EmployeeInformation e on e.SystemId=a.EmpSystemID
LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
									left join hkp.Designation d on d.id=e.GivenDesignationId
                                    left join org.SubSection ss on ss.id=po.SubSectionId         
									left join org.Section s on s.id=po.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId
                                    where e.PlantId='" + sPlantid + @"' 
									and e.systemid in (" + empids + @") 
									and a.IsActive=1
									and a.WorkDate between '" + fdate + @"' and '" + tdate + @"'
                                    order by a.workdate,EmployeeCodePreFix,EmployeeCodeNumeric";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function

        //internal void PayGroupWiseSearchAndSelectMultEmpBasicInfo(ParamList para, string v1, string v2, string text1, string text2, string strKey, out DataSet dsLocal)
        //{
        //    throw new NotImplementedException();
        //}

        public void CheckShiftRosterChild(string sGroupID, string strPlantID, string strRosterMasterID, string strShiftID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SRC.SystemID AS SRChildSystemID, SRM.SystemID SRMasterSystemID, SRM.ShiftRosterName, SRM.ShiftRosterDescription, SRC.ShiftDefinationID,
	                              SRC.ShiftSequence, ISNULL(SRM.IsFixedDayInMonthShiftRoster, 0) IsFixedDayInMonthShiftRoster, SRM.FixedDayInMonthShiftRoster,
                                  ISNULL(SRM.IsDaysLengthShiftRoster, 0) IsDaysLengthShiftRoster, ISNULL(SRM.DaysLengthShiftRoster, 0) DaysLengthShiftRoster,
	                              ISNULL(SRM.IsAlignWithCC, 0) IsAlignWithCC, ISNULL(SRM.IsFixedDayInMonthWeekOff, 0) IsFixedDayInMonthWeekOff,
                                  SRM.FixedDayInMonthWeekOff, ISNULL(SRM.IsDaysLengthWeekOff, 0) IsDaysLengthWeekOff, SRM.WeekOffDay,
	                              ISNULL(SRM.IsWeekOffInShiftLenght, 0) IsWeekOffInShiftLenght, SRM.WeekOffInShiftLenght
                            FROM [dbo].[ShiftRosterMaster] SRM
			                            LEFT JOIN [dbo].[ShiftRosterChild] SRC ON SRM.SystemID = SRC.SRMasterSystemID
                            WHERE SRM.SystemID = '" + strRosterMasterID + @"'
                                    AND SRC.ShiftDefinationID = '" + strShiftID + @"'
                                    AND SRM.GroupID = '" + sGroupID + @"' AND SRM.PlantID = '" + strPlantID + "'";

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

        public void SaveEmployeeInformation(string sGroupID, string strCompanyID, string strPlantID, string strSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeInformation WHERE GroupID = '" + sGroupID + @"' AND CompanyID = '" + strCompanyID + @"' AND PlantID = '" + strPlantID + "' AND SystemID = '" + strSystemID + @"'";

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
        public void GetEmpCode(string emppk, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EmployeeCode FROM EmployeeInformation
                                        WHERE systemid = '" + emppk + @"' ";

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
        public void SaveEmpReferenceInformation(string sEmpSystemID, string sSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmpReferenceInformation
                                    WHERE EmpSystemID = '" + sEmpSystemID + @"'
                                            AND SystemID = '" + sSystemID + @"'";

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

        public void SaveEmployeePin(string sEmpSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"Select * from HKP.EmployeeMobileAppsAuthorization Where EmployeeId='" + sEmpSystemID + "'";

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

        public void GetEmployeeSkill(string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmpSkillInformation
                                    WHERE EmpSystemID = '" + sEmpSystemID + @"' ";

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

        public void GetEmployeeExp(string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmpExperienceInformation
                                    WHERE EmpSystemID = '" + sEmpSystemID + @"' ";

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

        public void GetEmployeeJD(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM TRN.EmployeeJobDescription
                                    WHERE EmployeeId = '" + EmployeeId + @"' ";

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

        public void SaveEmpReferenceInformation(string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmpReferenceInformation
                                    WHERE EmpSystemID = '" + sEmpSystemID + @"' ";

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

        public void SaveEmpDateWiseJobLocation(string strEmpSystemID, string strSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmpDateWiseJobLocation
                                    WHERE EmpSystemID = '" + strEmpSystemID + @"'
                                            AND SystemID = '" + strSystemID + @"'";

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

        public void GetDateWiseEmpJobLocation(string strEmpSystemID, string strEffDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmpDateWiseJobLocation
                                    WHERE EmpSystemID = '" + strEmpSystemID + @"'
                                            AND EffectiveDate = '" + strEffDate + @"'";

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

        public void SaveEmployeeBankInfoBackUp(string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM EmployeeBankInfoBackUp
                //                        WHERE EmpSystemID = '" + sEmpSystemID + @"'";
                strSQL = @"SELECT * FROM EmployeeBankInfo
                                        WHERE EmpSystemID = '" + sEmpSystemID + @"'";

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

        public void SelectEmployeeBankInfoBackUp(string ROWID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeBankInfoBackUp
                                        WHERE ROWID = '" + ROWID + "'";

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

        public void SaveEmployeeBankInfo(string lblRowId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeBankInfo
                                        WHERE RowID = '" + lblRowId + @"'";

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

        public void SelectEmployeeBankInfo(string ROWID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeBankInfo
                                        WHERE ROWID = '" + ROWID + "'";

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
        public void xGetEmployeeOnDutyByPK(string pk, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                                  FROM EmployeeOnDuty
                                  WHERE  Id = '" + pk + @"'";


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
        }


        public void SaveEmployeeOTEntitle(string strEmpSystemID, string strSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeOTEntitle
                                    WHERE EmpSystemID = '" + strEmpSystemID + @"'
                                            AND SystemID = '" + strSystemID + @"'";

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
        //public void SaveEmployeeOTEntitle(string strEmpSystemID, string strSystemID, string OTSDate, out System.Data.DataSet dsRef)
        //{
        //    string strSQL;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        strSQL = @"SELECT * FROM EmployeeOTEntitle
        //                            WHERE EmpSystemID = '" + strEmpSystemID + @"'
        //                                    AND SystemID = '" + strSystemID + @"' and OTSDate not in ='" + OTSDate + "'";

        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function
        public void SaveEmployeePIN(string strEmpSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"Select * from hkp.EmployeeMobileAppsAuthorization Where EmployeeId='" + strEmpSystemID + "'";

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

        public void SaveEmployeeRef(string strEmpSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"Select * from dbo.EmpReferenceInformation Where EmpSystemID='" + strEmpSystemID + "'";

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

        public void SaveEmployeeShiftAssign(string strEmpSystemID, string strSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeShiftAssign
                                    WHERE EmpSystemID = '" + strEmpSystemID + @"'
                                            AND SystemID = '" + strSystemID + @"'";

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

        //public void SaveEmployeeShiftAssign(string strEmpSystemID, string effectiveDate, out System.Data.DataSet dsRef)
        //{
        //    string strSQL;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        strSQL = @"SELECT * FROM EmployeeShiftAssign WHERE EmpSystemID = '" + strEmpSystemID + @"' AND EffectiveDate='" + effectiveDate + "'";

        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function

        public void SaveEmployeeWeekOffByDay(string strEmpSystemID, string strSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeWeekOffByDay
                                        WHERE EmpSystemID = '" + strEmpSystemID + @"'
                                                        AND SystemID = '" + strSystemID + "'";

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

        public void SaveEmployeeWeekOffByDay(string strEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeWeekOffByDay
                                        WHERE EmpSystemID = '" + strEmpSystemID + @"'";

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

        public void SaveEmployeeImage(string strSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM EmployeeImage " +
                         "WHERE EmpSystemID = '" + strSystemID + "'";

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

        public void SearchThanaName(string sCountry, string sDistrict, string strKey, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM
                                    (SELECT T.ID, T.UserName ThanaName, D.UserName DistrictName, C.UserName CountryName
                                        FROM [SCS].PoliceStation T
                                                LEFT JOIN [SCS].District D ON T.DistrictID = D.ID
                                                LEFT JOIN SCS.[State] s	ON D.StateId = s.ID
                                                LEFT JOIN [SCS].Country C	ON s.CountryID = C.ID ";

                if (sDistrict.Trim() != "")
                {
                    strSQL = strSQL + " WHERE T.DistrictID = '" + sDistrict.Trim() + @"'";
                }

                strSQL = strSQL + ") A";

                if (sCountry.Trim() != "")
                {
                    strSQL = strSQL + " WHERE CountryName = '" + sCountry.Trim() + @"'";
                }

                if (strKey.Trim() != "")
                {
                    if (sCountry.Trim() == "")
                    {
                        strSQL = strSQL + " WHERE " + strKey + @"";
                    }
                    else
                    {
                        strSQL = strSQL + " AND " + strKey + @"";
                    }
                }

                strSQL = strSQL + " ORDER BY ThanaName";

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

        public void SearchSeparatedEmpBasicInfo(string sGroupID, string sCompanyID, string sPlantID, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM
		                            (
                                    SELECT E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS DOB,EmployeeCodePreFix,EmployeeCodeNumeric,
		                                   E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, e.GenderID GenderName,
                                           REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS  DOJ, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC,
                                           U.UserName AS Unit, Dv.UserName AS Division, De.UserName AS Department, Se.UserName AS Section,
                                            suS.UserName SubSection, Dsg.UserName AS Designation, EC.UserName EmpCategoryName
		                            FROM EmployeeInformation AS E
				                            LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = E.GivenDesignationId
				                            LEFT OUTER JOIN hkp.EmployeeCategory AS EC ON DM.EmployeeCategoryId = EC.id
				                            LEFT OUTER JOIN
							                            org.Unit AS U ON U.Id = EN.UnitID
				                            LEFT OUTER JOIN
							                            org.Division AS Dv ON Dv.Id = PO.DivisionID
				                            LEFT OUTER JOIN
							                            org.Department AS De ON De.Id = PO.DepartmentID
				                            LEFT OUTER JOIN
							                            hkp.Designation AS Dsg ON Dsg.Id = PO.DesignationID
				                            LEFT OUTER JOIN
							                            org.Section AS Se ON Se.Id = PO.SectionID
				                            LEFT OUTER JOIN
							                            org.SubSection AS SuS ON SuS.Id = PO.SubSectionId
							         WHERE E.GroupID = '" + sGroupID + @"' AND E.CompanyID = '" + sCompanyID + @"'
                                              AND E.PlantID = '" + sPlantID + "' AND E.EmployeeStatus != 'Active') A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void GetDesignationGroupSearch(string sGroupID, string strKey, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [HKP].[DesignationGroup]
                                    WHERE Id IN (SELECT DesignationGroupId FROM [HKP].[CompanyGroupDesignationGroup] WHERE CompanyGroupId = '" + sGroupID + @"')";

                if (strKey.Trim() != "")
                {
                    strSQL = strSQL + " and " + strKey + "";
                }

                strSQL = strSQL + " Order By Sequence";

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

        public void SearchActiveEmpBasicInfo(string sGroupID, string sCompanyID, string sPlantID, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @" SELECT top (100)
                                          *
                                        FROM
                                        (SELECT E.SystemID,
                                                E.EmployeeCode,mb.Code BudgetCode,
                                                E.EmployeeName,
                                                Dsg.UserName AS Designation,
                                                Dsg2.UserName GivenDesignaion,
EmployeeCodePreFix,EmployeeCodeNumeric,
                                                dg.UserName DesignationGroup,
                                                srm2.SalaryRuleName,
                                                EC.UserName EmployeeCategory,
                                                REPLACE(CONVERT(varchar(11), E.DOB, 106), ' ', '-') AS DOB,
                                                E.FatherName,
                                                E.MotherName,
                                                E.EmpType EmployeeType,
                                                E.EmploymentType EmploymentNature,
                                                E.NationalID,
                                                E.GenderID GenderName,
                                                REPLACE(CONVERT(varchar(11), E.DOJ, 106), ' ', '-') AS DOJ,
                                                REPLACE(CONVERT(varchar(11), E.DOC, 106), ' ', '-') AS DOC,
                                                U.UserName AS Unit,
                                                Dv.UserName AS Division,
                                                De.UserName AS Department,
                                                Se.UserName AS Section,
                                                dgg.UserName DesignationGroupAsPerGivenDesignaion,
                                                --srm.SalaryRuleName SalaryRuleNameAsPerDesignaion,
                                                e.EmployeeCategorySystemID,
                                                SuS.UserName SubSection FROM EmployeeInformation AS E
                                          LEFT OUTER JOIN [HKP].[EmployeeCategory] AS EC ON E.EmployeeCategorySystemID = EC.Id
                                          LEFT OUTER JOIN ORG.Unit AS U ON U.Id = E.UnitID
                                          LEFT OUTER JOIN ORG.Division AS Dv ON Dv.Id = E.DivisionID
                                          LEFT OUTER JOIN ORG.Department AS De ON De.Id = E.DepartmentID
                                          LEFT OUTER JOIN HKP.Designation AS Dsg ON Dsg.Id = E.DesignationSystemID
                                          LEFT OUTER JOIN HKP.Designation AS Dsg2 ON Dsg2.Id = E.GivenDesignationId
                                          
                                          LEFT OUTER JOIN
                                                      (
                                                        SELECT DC.*,dm.DesignationId,dm.EmployeeCategoryId,dm.DesignationGroupID 
                                                                                FROM MST.DesignationMaster DM
							                                                    LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                                                            ON DM.Id=DC.DesignationMasterId

                                                      ) AS dm ON dm.DesignationId = E.GivenDesignationId  AND dm.EmployeeCategoryId = e.EmployeeCategorySystemID
                                                                    and dm.plantid=e.plantid
                                          LEFT OUTER JOIN HKP.DesignationGroup AS dgg ON dgg.Id = dm.DesignationGroupID

                                          LEFT OUTER JOIN SalaryRuleMaster srm2 ON srm2.SystemID = dm.SalaryRuleMasterId

                                    LEFT OUTER JOIN MST.ManpowerBudget mb on mb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mb.EntityId=EN.Id
                                          LEFT OUTER JOIN ORG.Section AS Se ON Se.Id = PO.SectionID
                                          LEFT OUTER JOIN ORG.SubSection AS SuS ON SuS.Id = PO.SubSectionID
                                          WHERE E.GroupID = '" + sGroupID + @"'
                                          AND E.CompanyID = '" + sCompanyID + @"'
                                          AND E.PlantID = '" + sPlantID + @"'
                                          AND E.EmployeeStatus = 'Active'
                                        ) A";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void SearchSalaryStructureChange(string sGroupID, string sCompanyID, string sPlantID, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @" SELECT top (" + bplib.clsWebLib.MaxRow() + @")
                                          *
                                        FROM
                                        (SELECT E.SystemID,
                                                E.EmployeeCode,mb.Code BudgetCode,
                                                E.EmployeeName,
                                                Dsg.UserName AS Designation,
                                                Dsg2.UserName GivenDesignaion,
                                                dg.UserName DesignationGroup,
                                                EmployeeCodePreFix,EmployeeCodeNumeric,
                                                srm2.SalaryRuleName,
                                                EC.UserName EmployeeCategory,
                                                REPLACE(CONVERT(varchar(11), E.DOB, 106), ' ', '-') AS DOB,
                                                E.FatherName,
                                                E.MotherName,
                                                E.EmpType EmployeeType,
                                                E.EmploymentType EmploymentNature,
                                                E.NationalID,
                                                E.GenderID GenderName,
                                                REPLACE(CONVERT(varchar(11), E.DOJ, 106), ' ', '-') AS DOJ,
                                                REPLACE(CONVERT(varchar(11), E.DOC, 106), ' ', '-') AS DOC,
                                                U.UserName AS Unit,
                                                Dv.UserName AS Division,
                                                De.UserName AS Department,
                                                Se.UserName AS Section,
                                                dgg.UserName DesignationGroupAsPerGivenDesignaion,
                                                --srm.SalaryRuleName SalaryRuleNameAsPerDesignaion,
                                                e.EmployeeCategorySystemID,
                                                SuS.UserName SubSection FROM EmployeeInformation AS E
                                          LEFT OUTER JOIN [HKP].[EmployeeCategory] AS EC ON E.EmployeeCategorySystemID = EC.Id
                                          LEFT OUTER JOIN ORG.Unit AS U ON U.Id = E.UnitID
                                          LEFT OUTER JOIN ORG.Division AS Dv ON Dv.Id = E.DivisionID
                                          LEFT OUTER JOIN ORG.Department AS De ON De.Id = E.DepartmentID
                                          LEFT OUTER JOIN HKP.Designation AS Dsg ON Dsg.Id = E.DesignationSystemID
                                          LEFT OUTER JOIN HKP.Designation AS Dsg2 ON Dsg2.Id = E.GivenDesignationId
                                          LEFT OUTER JOIN
                                                      (
                                                               SELECT DC.*,dm.DesignationId,dm.EmployeeCategoryId,dm.DesignationGroupID
                                                                                FROM MST.DesignationMaster DM
							                                                    LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                                                            ON DM.Id=DC.DesignationMasterId

                                                      ) AS dm ON dm.DesignationId = E.GivenDesignationId  AND dm.EmployeeCategoryId = e.EmployeeCategorySystemID
                                                    and dm.plantid=e.plantid
                                          LEFT OUTER JOIN HKP.DesignationGroup AS dgg ON dgg.Id = dm.DesignationGroupID

                                          LEFT OUTER JOIN SalaryRuleMaster srm2 ON srm2.SystemID = dm.SalaryRuleMasterId
                                    LEFT OUTER JOIN MST.ManpowerBudget mb on mb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mb.EntityId=EN.Id
                                          LEFT OUTER JOIN ORG.Section AS Se ON Se.Id = PO.SectionID
                                          LEFT OUTER JOIN ORG.SubSection AS SuS ON SuS.Id = PO.SubSectionID

                                            left outer join (select count(systemid) c,EmpInfoSystemID,Plantid from SalaryInfoDefineMaster group by EmpInfoSystemID,Plantid) bk
										  on bk.PlantID=e.PlantId and bk.EmpInfoSystemID=e.SystemId

                                          WHERE E.GroupID = '" + sGroupID + @"'
                                          AND E.CompanyID = '" + sCompanyID + @"'
                                          AND E.PlantID = '" + sPlantID + @"'
                                          AND E.EmployeeStatus = 'Active'
                                          --and e.IsApproved=1
										  --and isnull(bk.c,0)>0
                                          AND E.SystemId IN (
                                          SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where IsApproved = 1
                                          union
                                          SELECT EmpInfoSystemID FROM SalaryInfoBackMaster where EmpInfoSystemID IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where IsApproved = 0)                            
                                          union
                                          SELECT EmpInfoSystemID FROM SalaryInfoBackMaster where EmpInfoSystemID NOT IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster)
                                          )

                                        ) A";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void SearchSalaryStructureInit(string sGroupID, string sCompanyID, string sPlantID, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @" SELECT top (100)
                                          *
                                        FROM
                                        (SELECT E.SystemID,
                                                E.EmployeeCode,mb.Code BudgetCode,
                                                E.EmployeeName,
                                                Dsg.UserName AS Designation,
                                                Dsg2.UserName GivenDesignaion,
                                                dg.UserName DesignationGroup,
                                                srm2.SalaryRuleName
                                                ,EmployeeCodePreFix,EmployeeCodeNumeric,
                                                EC.UserName EmployeeCategory,
                                                REPLACE(CONVERT(varchar(11), E.DOB, 106), ' ', '-') AS DOB,
                                                E.FatherName,
                                                E.MotherName,
                                                E.EmpType EmployeeType,
                                                E.EmploymentType EmploymentNature,
                                                E.NationalID,
                                                E.GenderID GenderName,
                                                REPLACE(CONVERT(varchar(11), E.DOJ, 106), ' ', '-') AS DOJ,
                                                REPLACE(CONVERT(varchar(11), E.DOC, 106), ' ', '-') AS DOC,
                                                U.UserName AS Unit,
                                                Dv.UserName AS Division,
                                                De.UserName AS Department,
                                                Se.UserName AS Section,
                                                dgg.UserName DesignationGroupAsPerGivenDesignaion,
                                                --srm.SalaryRuleName SalaryRuleNameAsPerDesignaion,
                                                e.EmployeeCategorySystemID,
                                                SuS.UserName SubSection 
                                   FROM EmployeeInformation AS E
                                          LEFT OUTER JOIN [HKP].[EmployeeCategory] AS EC ON E.EmployeeCategorySystemID = EC.Id
                                          LEFT OUTER JOIN ORG.Unit AS U ON U.Id = E.UnitID
                                          LEFT OUTER JOIN ORG.Division AS Dv ON Dv.Id = E.DivisionID
                                          LEFT OUTER JOIN ORG.Department AS De ON De.Id = E.DepartmentID
                                          LEFT OUTER JOIN HKP.Designation AS Dsg ON Dsg.Id = E.DesignationSystemID
                                          LEFT OUTER JOIN HKP.Designation AS Dsg2 ON Dsg2.Id = E.GivenDesignationId
                                          LEFT OUTER JOIN
                                                      (
                                                        SELECT DMST.DesignationId, DMSt.EmployeeCategoryId, DMSt.DesignationGroupId, DC.SalaryRuleMasterId 
                                                         FROM MST.DesignationMaster DMST
                                                                LEFT JOIN SCS.DesignationMasterConfiguration DC ON DC.DesignationMasterId=DMST.Id
                                                         WHERE DC.PlantId = '" + sPlantID + @"'
                                                      ) AS dm ON dm.DesignationId = E.GivenDesignationId  AND dm.EmployeeCategoryId = e.EmployeeCategorySystemID
                                          LEFT OUTER JOIN HKP.DesignationGroup AS dgg ON dgg.Id = dm.DesignationGroupID

                                          LEFT OUTER JOIN SalaryRuleMaster srm2 ON srm2.SystemID = dm.SalaryRuleMasterId
                                          LEFT OUTER JOIN [MST].[ManpowerBudget] mb on mb.Id=E.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mb.EntityId=EN.Id
                                          LEFT OUTER JOIN ORG.Section AS Se ON Se.Id = PO.SectionID
                                          LEFT OUTER JOIN ORG.SubSection AS SuS ON SuS.Id = PO.SubSectionID
                                          LEFT OUTER JOIN (SELECT COUNT(Systemid) c,EmpInfoSystemID,Plantid from SalaryInfoBackMaster group by EmpInfoSystemID,Plantid) bk
										            ON BK.PlantID=e.PlantId and bk.EmpInfoSystemID=e.SystemId

                                          WHERE E.GroupID = '" + sGroupID + @"'
                                          AND E.CompanyID = '" + sCompanyID + @"'
                                          AND E.PlantID = '" + sPlantID + @"'
                                          AND E.EmployeeStatus = 'Active'
                                          --and e.IsApproved=1
                                        --AND E.SystemId NOT IN (select EmpInfoSystemID from SalaryInfoDefineMaster Group By EmpInfoSystemID having Count(SystemID)>1)
                                        --AND E.SystemId NOT IN (select EmpInfoSystemID from SalaryInfoBackMaster )
										  and isnull(bk.c,0)=0    
                                        ) A";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void SearchActiveEmpBasicInfoForSalaryStructureApproval(string sGroupID, string sCompanyID, string sPlantID, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @" SELECT top (100)
                                          *
                                        FROM
                                        (SELECT E.SystemID,
                                                E.EmployeeCode,mb.Code BudgetCode,
                                                E.EmployeeName,
                                                Dsg.UserName AS Designation,
                                                Dsg2.UserName GivenDesignaion,
                                                dg.UserName DesignationGroup,
                                                EmployeeCodePreFix,EmployeeCodeNumeric,
                                                srm2.SalaryRuleName,
                                                EC.UserName EmployeeCategory,
                                                REPLACE(CONVERT(varchar(11), E.DOB, 106), ' ', '-') AS DOB,
                                                E.FatherName,
                                                E.MotherName,
                                                E.EmpType EmployeeType,
                                                E.EmploymentType EmploymentNature,
                                                E.NationalID,
                                                E.GenderID GenderName,
                                                REPLACE(CONVERT(varchar(11), E.DOJ, 106), ' ', '-') AS DOJ,
                                                REPLACE(CONVERT(varchar(11), E.DOC, 106), ' ', '-') AS DOC,
                                                U.UserName AS Unit,
                                                Dv.UserName AS Division,
                                                De.UserName AS Department,
                                                Se.UserName AS Section,
                                                dgg.UserName DesignationGroupAsPerGivenDesignaion,
                                                --srm.SalaryRuleName SalaryRuleNameAsPerDesignaion,
                                                e.EmployeeCategorySystemID,
                                                SuS.UserName SubSection FROM EmployeeInformation AS E
                                          LEFT OUTER JOIN [HKP].[EmployeeCategory] AS EC ON E.EmployeeCategorySystemID = EC.Id
                                          LEFT OUTER JOIN ORG.Unit AS U ON U.Id = E.UnitID
                                          LEFT OUTER JOIN ORG.Division AS Dv ON Dv.Id = E.DivisionID
                                          LEFT OUTER JOIN ORG.Department AS De ON De.Id = E.DepartmentID
                                          LEFT OUTER JOIN HKP.Designation AS Dsg ON Dsg.Id = E.DesignationSystemID
                                          LEFT OUTER JOIN HKP.Designation AS Dsg2 ON Dsg2.Id = E.GivenDesignationId
                                          LEFT OUTER JOIN
                                                      (
                                                    SELECT DC.*,dm.DesignationId,dm.EmployeeCategoryId,dm.DesignationGroupID
                                                                                FROM MST.DesignationMaster DM
							                                                    LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                                                            ON DM.Id=DC.DesignationMasterId

                                                      ) AS dm ON dm.DesignationId = E.GivenDesignationId  AND dm.EmployeeCategoryId = e.EmployeeCategorySystemID
                                                  and  dm.plantid=e.plantid
                                          LEFT OUTER JOIN HKP.DesignationGroup AS dgg ON dgg.Id = dm.DesignationGroupID

                                          LEFT OUTER JOIN SalaryRuleMaster srm2 ON srm2.SystemID = dm.SalaryRuleMasterId
                                          left outer join [MST].[ManpowerBudget] mb on mb.Id=E.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mb.PositionId=PO.Id
                                          LEFT OUTER JOIN ORG.Section AS Se ON Se.Id = PO.SectionID
                                          LEFT OUTER JOIN ORG.SubSection AS SuS ON SuS.Id = PO.SubSectionID
                                        left outer join (select count(systemid) c,EmpInfoSystemID,Plantid from SalaryInfoBackMaster group by EmpInfoSystemID,Plantid) bk
										  on bk.PlantID=e.PlantId and bk.EmpInfoSystemID=e.SystemId

                                          WHERE E.GroupID = '" + sGroupID + @"'
                                          AND E.CompanyID = '" + sCompanyID + @"'
                                          AND E.PlantID = '" + sPlantID + @"'
                                          AND E.EmployeeStatus = 'Active'
                                            --and e.IsApproved=1
										  and isnull(bk.c,0)=0
                                        ) A";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + @" and a.SystemId in (
                                        select EmpInfoSystemID from SalaryInfoDefineMaster
                                        where IsApproved = 0
										)";
                    //          strSql = strSql + " WHERE " + strKey + @" and a.SystemId not in (
                    //                              select EmpInfoSystemID from SalaryInfoDefineMaster
                    //                              where IsApproved = 1
                    //)";
                }
                else
                {
                    strSql = strSql + @" where a.SystemId in (
                                        select EmpInfoSystemID from SalaryInfoDefineMaster
                                        where IsApproved = 0
										)";
                    //          strSql = strSql + @" where a.SystemId not  in (
                    //                              select EmpInfoSystemID from SalaryInfoDefineMaster
                    //                              where IsApproved = 1
                    //)";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void SearchEmployees(string sGroupID, string sCompanyID, string sPlantID, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @" SELECT top (100)
                                          *
                                        FROM
                                        (SELECT E.SystemID,
                                                E.EmployeeCode,mb.Code BudgetCode,
                                                E.EmployeeName,
                                                Dsg.UserName AS Designation,
                                                Dsg2.UserName GivenDesignaion,
                                                dg.UserName DesignationGroup
                                                ,EmployeeCodePreFix,EmployeeCodeNumeric,
                                                srm2.SalaryRuleName,
                                                EC.UserName EmployeeCategory,
                                                REPLACE(CONVERT(varchar(11), E.DOB, 106), ' ', '-') AS DOB,
                                                E.FatherName,
                                                E.MotherName,
                                                E.EmpType EmployeeType,
                                                E.EmploymentType EmploymentNature,
                                                E.NationalID,
                                                E.GenderID GenderName,
                                                REPLACE(CONVERT(varchar(11), E.DOJ, 106), ' ', '-') AS DOJ,
                                                REPLACE(CONVERT(varchar(11), E.DOC, 106), ' ', '-') AS DOC,
                                                U.UserName AS Unit,
                                                Dv.UserName AS Division,
                                                De.UserName AS Department,
                                                Se.UserName AS Section,
                                                dgg.UserName DesignationGroupAsPerGivenDesignaion,
                                                --srm.SalaryRuleName SalaryRuleNameAsPerDesignaion,
                                                e.EmployeeCategorySystemID,
                                                SuS.UserName SubSection FROM EmployeeInformation AS E
                                          LEFT OUTER JOIN [HKP].[EmployeeCategory] AS EC ON E.EmployeeCategorySystemID = EC.Id
                                          LEFT OUTER JOIN ORG.Unit AS U ON U.Id = E.UnitID
                                          LEFT OUTER JOIN ORG.Division AS Dv ON Dv.Id = E.DivisionID
                                          LEFT OUTER JOIN ORG.Department AS De ON De.Id = E.DepartmentID
                                          LEFT OUTER JOIN HKP.Designation AS Dsg ON Dsg.Id = E.DesignationSystemID
                                          LEFT OUTER JOIN HKP.Designation AS Dsg2 ON Dsg2.Id = E.GivenDesignationId
                                          LEFT OUTER JOIN
                                                      (
                                                    SELECT DC.*,dm.DesignationId,dm.EmployeeCategoryId,dm.DesignationGroupID
                                                                                FROM MST.DesignationMaster DM
							                                                    LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                                                            ON DM.Id=DC.DesignationMasterId

                                                      ) AS dm ON dm.DesignationId = E.GivenDesignationId  AND dm.EmployeeCategoryId = e.EmployeeCategorySystemID
                                                  and  dm.plantid=e.plantid
                                          LEFT OUTER JOIN HKP.DesignationGroup AS dgg ON dgg.Id = dm.DesignationGroupID

                                          LEFT OUTER JOIN SalaryRuleMaster srm2 ON srm2.SystemID = dm.SalaryRuleMasterId
                                          LEFT OUTER JOIN MST.ManpowerBudget mb on mb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mb.PositionId=PO.Id

                                          LEFT OUTER JOIN ORG.Section AS Se ON Se.Id = PO.SectionID
                                          LEFT OUTER JOIN ORG.SubSection AS SuS ON SuS.Id = PO.SubSectionID
                                        left outer join (select count(systemid) c,EmpInfoSystemID,Plantid from SalaryInfoDefineMaster group by EmpInfoSystemID,Plantid) bk
										  on bk.PlantID=e.PlantId and bk.EmpInfoSystemID=e.SystemId

                                          WHERE E.GroupID = '" + sGroupID + @"'
                                          AND E.CompanyID = '" + sCompanyID + @"'
                                          AND E.PlantID = '" + sPlantID + @"'
                                          AND E.EmployeeStatus = 'Active'                                            
										  and isnull(bk.c,0)>0
                                        ) A";

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void SearchEmployeeSSCApproval(string sGroupID, string sCompanyID, string sPlantID, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @" SELECT top (100)
                                          *
                                        FROM
                                        (SELECT E.SystemID,
                                                E.EmployeeCode,mb.Code BudgetCode,
                                                E.EmployeeName,
                                                Dsg.UserName AS Designation,
                                                Dsg2.UserName GivenDesignaion,
                                                dg.UserName DesignationGroup,
                                                srm2.SalaryRuleName,
                                                EC.UserName EmployeeCategory,
                                                REPLACE(CONVERT(varchar(11), E.DOB, 106), ' ', '-') AS DOB,
                                                E.FatherName,
                                                E.MotherName,
                                                E.EmpType EmployeeType,
                                                E.EmploymentType EmploymentNature,
                                                E.NationalID,
                                                E.GenderID GenderName,
                                                REPLACE(CONVERT(varchar(11), E.DOJ, 106), ' ', '-') AS DOJ,
                                                REPLACE(CONVERT(varchar(11), E.DOC, 106), ' ', '-') AS DOC,
                                                U.UserName AS Unit,
                                                Dv.UserName AS Division,
                                                De.UserName AS Department,
                                                Se.UserName AS Section,
                                                EmployeeCodePreFix,EmployeeCodeNumeric,
                                                dgg.UserName DesignationGroupAsPerGivenDesignaion,
                                                --srm.SalaryRuleName SalaryRuleNameAsPerDesignaion,
                                                e.EmployeeCategorySystemID,
                                                SuS.UserName SubSection FROM EmployeeInformation AS E
                                          LEFT OUTER JOIN [HKP].[EmployeeCategory] AS EC ON E.EmployeeCategorySystemID = EC.Id
                                          LEFT OUTER JOIN ORG.Unit AS U ON U.Id = E.UnitID
                                          LEFT OUTER JOIN ORG.Division AS Dv ON Dv.Id = E.DivisionID
                                          LEFT OUTER JOIN ORG.Department AS De ON De.Id = E.DepartmentID
                                          LEFT OUTER JOIN HKP.Designation AS Dsg ON Dsg.Id = E.DesignationSystemID
                                          LEFT OUTER JOIN HKP.Designation AS Dsg2 ON Dsg2.Id = E.GivenDesignationId
                                          LEFT OUTER JOIN
                                                      (
                                                    SELECT dm.*,dc.SalaryRuleMasterId,dc.PlantId 
                                                                                FROM MST.DesignationMaster DM
							                                                    LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                                                            ON DM.Id=DC.DesignationMasterId

                                                      ) AS dm ON dm.DesignationId = E.GivenDesignationId  AND dm.EmployeeCategoryId = e.EmployeeCategorySystemID
and dm.plantid=e.plantid
                                          LEFT OUTER JOIN HKP.DesignationGroup AS dgg ON dgg.Id = dm.DesignationGroupID

                                          LEFT OUTER JOIN SalaryRuleMaster srm2 ON srm2.SystemID = dm.SalaryRuleMasterId
                                          LEFT OUTER JOIN [MST].[ManpowerBudget] mb on mb.Id=E.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mb.PositionId=PO.Id
                                          LEFT OUTER JOIN ORG.Section AS Se ON Se.Id = PO.SectionID
                                          LEFT OUTER JOIN ORG.SubSection AS SuS ON SuS.Id = PO.SubSectionID
                                          LEFT OUTER JOIN (SELECT COUNT(SystemID) C, EmpInfoSystemID, Plantid FROM
																(
																SELECT SystemID, EmpInfoSystemID, Plantid FROM SalaryInfoDefineMaster
																UNION
																(SELECT SystemID, EmpInfoSystemID, Plantid FROM SalaryInfoBackMaster)
																) A GROUP BY EmpInfoSystemID, Plantid) bk
										  on bk.PlantID=e.PlantId and bk.EmpInfoSystemID=e.SystemId

                                          WHERE E.GroupID = '" + sGroupID + @"'
                                          AND E.CompanyID = '" + sCompanyID + @"'
                                          AND E.PlantID = '" + sPlantID + @"'
                                          AND E.EmployeeStatus = 'Active'
                                          
                                            and isnull(bk.c,0)>1
                                        ) A";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + @" and a.SystemId in (
                                        select EmpInfoSystemID from SalaryInfoDefineMaster
                                        where IsApproved = 0
										)";
                }
                else
                {
                    strSql = strSql + @" where a.SystemId in (
                                        select EmpInfoSystemID from SalaryInfoDefineMaster
                                        where IsApproved = 0
										)";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void SearchEmpBasicInfo(string sGroupID, string sCompanyID, string sPlantID, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            clsStaticInfo ob = null;
            try
            {
                string month = DateTime.Now.ToString("MMMM");
                string year = DateTime.Now.ToString("yyyy");
                string datestart = "01-" + month + "-" + year;

                ob = new clsStaticInfo();

                strSql = ob.GetEmployeeSQL();

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + " and GroupID = '" + sGroupID + @"' AND CompanyID = '" + sCompanyID + @"' AND PlantID = '" + sPlantID + @"'AND (DOS IS NULL OR DOS>'" + datestart + @"' OR EmployeeStatus='Active')";
                }
                else
                {
                    strSql = strSql + " WHERE GroupID = '" + sGroupID + @"' AND CompanyID = '" + sCompanyID + @"' AND PlantID = '" + sPlantID + @"'AND (DOS IS NULL OR DOS>'" + datestart + @"' OR EmployeeStatus='Active')";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        //rename by mizan for selected 
        public void SearchBudgetCodes(string sql, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = "select top(100) * from (" + sql + " ) X ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public string GetDynamicCol(string sql)
        {
            string[] cols = { "Id", "Code", "Plant", "Division", "Unit", "Section", "SubSection", "PositionName", "EntityName", "Line" };
            string r = "";
            foreach (var item in cols)
            {
                var col = sql.IndexOf(item);
                if (col != -1)
                {
                    if (r == string.Empty)
                    {
                        r = "A." + item;
                    }
                    else
                    {
                        r += ",A." + item;
                    }
                }
            }
            return r;
        }

        public void SearchBudgetCode(string sql, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                var _sql = GetDynamicCol(sql);

                strSql = "SELECT TOP(100) " + _sql + " FROM (" + sql + " ) A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void SearchRepotingEmpBasicInfo(string sGroupID, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT Top(100)* FROM
		                            (SELECT E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS DOB,
		                            E.FatherName, E.MotherName, E.EmpType, E.EmploymentType EmploymentNature, E.NationalID, E.GenderID GenderName,
                                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS  DOJ, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC,
                                    U.UserName AS Unit, Dv.UserName AS Division, De.UserName AS Department, Se.UserName AS Section,
                                    SuS.UserName SubSection, Dsg.UserName AS Designation, EC.UserName EmployeeType,EmployeeCodePreFix,EmployeeCodeNumeric
		                            FROM EmployeeInformation AS E
LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = E.GivenDesignationId
				                            LEFT OUTER JOIN
							                            [HKP].[EmployeeCategory] AS EC ON DM.EmployeeCategoryID = EC.ID
				                            LEFT OUTER JOIN
							                            [ORG].[Unit] AS U ON U.ID = E .UnitID
				                            LEFT OUTER JOIN
							                            [ORG].Division AS Dv ON Dv.ID = PO.DivisionID
				                            LEFT OUTER JOIN
							                            [ORG].Department AS De ON De.ID = PO.DepartmentID
				                            LEFT OUTER JOIN
							                            [HKP].Designation AS Dsg ON Dsg.ID = PO.DesignationID
				                            LEFT OUTER JOIN
							                            [ORG].Section AS Se ON Se.ID = PO.SectionID
				                            LEFT OUTER JOIN
							                            [ORG].SubSection AS SuS ON SuS.ID = PO.SubSectionID
							         WHERE E.GroupID = '" + sGroupID + @"') A ";

                if (strKey.Trim() == "")
                {
                    strKey = "1 = 1";
                }
                strSql = strSql + @"
                                    WHERE " + strKey + "";
                strSql = strSql + @"
                                    Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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


        public void SearchAndSelectMultEmpBasicInfo(string sGroupID, string sPlantID, string sFrmDt, string sToDt, string strKey, out System.Data.DataSet dsRef)
        {
            var startFromDate = Convert.ToDateTime(sFrmDt);
            var lastDay = DateTime.DaysInMonth(startFromDate.Year, startFromDate.Month); //Number of Days in a month
                                                                                         //var firstDay = new DateTime(startFromDate.Year, startFromDate.Month,1); //Number of Days in a month


            string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(startFromDate.Month);//Month Name from Month No
            var lastDate = lastDay + "-" + monthNameString + "-" + startFromDate.Year;
            var firstDate = "1" + "-" + monthNameString + "-" + startFromDate.Year;

            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                //string _fromDate = DateTime.Now.ToString("dd-MMM-yyyy");
                strSql = @"SELECT  [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
		                             (SELECT CONVERT(INT, E.EmployeeCode) EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
		                            E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, E.GenderID GenderName,
                                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                                    U.UserName AS Unit, Dv.UserName AS Division, De.UserName AS Department, Se.UserName AS Section,
                                    SuS.UserName SubSection, Dsg.UserName AS Designation, EC.UserName AS 'Employee Category',EC.IdCardFormat,E.EmploymentType,E.SystemID
                                    ,EmployeeCodePreFix,EmployeeCodeNumeric
		                            FROM (
                                            SELECT * FROM
                                                EmployeeInformation
                                                --WHERE SystemID IN
                                                --(
                                                 --SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sFrmDt + @"', '" + sToDt + @"', '" + sPlantID + @"')
                                               -- )
                                          ) AS E
				                         LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                            LEFT JOIN [MST].[DesignationMaster] d on d.DesignationId=e.GivenDesignationId
										    LEFT JOIN HKP.EmployeeCategory EC on EC.Id=d.EmployeeCategoryId
				                            LEFT OUTER JOIN
							                            ORG.Unit AS U ON U.Id= EN.UnitID
				                            LEFT OUTER JOIN
							                            ORG.Division AS Dv ON Dv.Id= PO.DivisionID
				                            LEFT OUTER JOIN
							                            ORG.Department AS De ON De.Id = PO.DepartmentID
				                            LEFT OUTER JOIN
							                            HKP.Designation AS Dsg ON Dsg.Id= PO.DesignationID
				                            LEFT OUTER JOIN
							                            ORG.Section AS Se ON Se.Id= PO.SectionID
				                            LEFT OUTER JOIN
							                            ORG.SubSection AS SuS ON SuS.Id= PO.SubSectionID
							         WHERE E.GroupID = '" + sGroupID + @"'  AND E.PlantId='" + sPlantID + @"' --and E.EmployeeStatus='Active'
                                            and (DOS >= '" + firstDate + @"' OR DOS IS NULL OR EmployeeStatus = 'Active') AND
                                                        DOJ <= '" + lastDate + @"'
                                                ) A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void SearchAndSelectMultEmpBasicInfoPlantWisePayGroup(string sGroupID, string sPlantID, string sYr, string sMth, string sDepartmentId, string sPayGrp, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            clsStaticInfo obs = null;
            DateTime _dateStart;
            string _dateJoin = "";
            string month = "";
            string year = "";
            var daysInMonth = 0;//Number of Days in a month


            if (sMth != "" && sYr != "")
            {
                month = sMth.ToString();
                year = sYr.ToString();
                daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                _dateStart = Convert.ToDateTime(1 + "-" + bplib.clsWebLib.GetMonthName(month) + "-" + year);
                _dateJoin = daysInMonth + "-" + bplib.clsWebLib.GetMonthName(month) + "-" + year;
            }
            else
            {

                month = DateTime.Now.ToString("MMMM");
                year = DateTime.Now.ToString("yyyy");
                daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                string datestart = "01-" + month + "-" + year;
                _dateStart = Convert.ToDateTime(1 + "-" + bplib.clsWebLib.GetMonthName(month) + "-" + year);

                _dateStart = Convert.ToDateTime(datestart).AddMonths(-1);
                _dateJoin = daysInMonth + "-" + bplib.clsWebLib.GetMonthName(month) + "-" + year;
            }
            var _wcp = string.Empty;
            var _wcd = string.Empty;

            if (sPayGrp.ToUpper() != "NO GROUP")
            {
                if (sPayGrp.ToUpper() != "ALL")
                {

                    _wcp = " AND E.SystemId IN(select employeeid from MST.PayrollGroupMaster where PayrollGroupId = '" + sPayGrp + @"')";
                }
                else
                {
                    _wcp = "";
                }
            }
            else
            {
                _wcp = " AND E.SystemId NOT IN(select employeeid from MST.PayrollGroupMaster)";
            }

            if (sDepartmentId != "ALL")
            {
                _wcd = " AND DP.Id = '" + sDepartmentId + @"'";
            }
            else
            {
                _wcd = "";
            }

            try
            {
                obs = new clsStaticInfo();
                strSql = @"SELECT [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
		                            (SELECT CONVERT(INT, E.EmployeeCode)EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
		                            E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, E.GenderID,
                                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                                    U.UserName AS Unit, Dv.UserName  AS Division, dp.UserName AS Department, S.UserName AS Section,
                                    sb.UserName SubSection, D.UserName AS Designation, EC.UserName, E.SystemId,EmployeeCodePreFix,EmployeeCodeNumeric
		                            FROM EmployeeInformation AS E
LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id 
				                           " + obs.EntityTables() + @"
							         WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + @"' AND (DOS IS NULL OR DOS>'" + _dateStart + @"' OR EmployeeStatus='Active')
                                    " + _wcd + @" " + _wcp + @" AND (DOJ IS NULL OR DOJ<= '" + _dateJoin + @"')
                                    ) A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void SearchAndSelectMultEmpBasicInfoPlantWise(string sGroupID, string sPlantID, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            clsStaticInfo obs = null;
            string year = "";
            string month = "";
            string datestart = "";

            month = DateTime.Now.ToString("MMMM");

            year = DateTime.Now.ToString("yyyy");


            datestart = "01-" + month + "-" + year;

            DateTime _dateStart = Convert.ToDateTime(datestart).AddMonths(-1);

            try
            {
                obs = new clsStaticInfo();
                //strSql = @"SELECT TOP (100) [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
                //              (SELECT E.SystemId EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
                //              E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID,E.GenderID,
                //                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                //                    U.UserName AS Unit, Dv.UserName  AS Division, dp.UserName AS Department, S.UserName AS Section,
                //                    sb.UserName SubSection, D.UserName AS Designation, EC.UserName, E.SystemID
                //              FROM EmployeeInformation AS E
                //               "+ obs.EntityTables()+ @"
                //WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + @"' AND E.EmployeeStatus='Active' AND E.IsApproved=1) A ";
                strSql = @"SELECT [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
		                            (SELECT E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
		                            E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, E.GenderID,
                                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                                    U.UserName AS Unit, Dv.UserName  AS Division, dp.UserName AS Department, S.UserName AS Section,
                                    sb.UserName SubSection, D.UserName AS Designation, EC.UserName, E.SystemId,EmployeeCodePreFix,EmployeeCodeNumeric
		                            FROM EmployeeInformation AS E
LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id 
				                           " + obs.EntityTables() + @"
							         WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + @"' AND (DOS IS NULL OR DOS>='" + _dateStart.ToString("dd-MMM-yyyy") + @"' OR EmployeeStatus='Active') 
                                    ) A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void SearchAndSelectMultEmpBasicInfoPlantWiseJobCard(string sGroupID, string sPlantID, string strKey, string date, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            clsStaticInfo obs = null;
            string year = "";
            string month = "";
            string datestart = "";
            if (date == "")
            {
                month = DateTime.Now.ToString("MMMM");

            }

            if (date == "")
            {

                year = DateTime.Now.ToString("yyyy");
            }

            if (date == "")
            {
                datestart = "01-" + month + "-" + year;

            }
            else
            {
                datestart = date;
            }

            DateTime _dateStart = Convert.ToDateTime(datestart).AddMonths(-1);

            try
            {
                obs = new clsStaticInfo();
                //strSql = @"SELECT TOP (100) [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
                //              (SELECT E.SystemId EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
                //              E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID,E.GenderID,
                //                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                //                    U.UserName AS Unit, Dv.UserName  AS Division, dp.UserName AS Department, S.UserName AS Section,
                //                    sb.UserName SubSection, D.UserName AS Designation, EC.UserName, E.SystemID
                //              FROM EmployeeInformation AS E
                //               "+ obs.EntityTables()+ @"
                //WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + @"' AND E.EmployeeStatus='Active' AND E.IsApproved=1) A ";
                strSql = @"SELECT [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
		                            (SELECT CONVERT(INT, E.EmployeeCode)EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
		                            E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, E.GenderID,
                                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                                    U.UserName AS Unit, Dv.UserName  AS Division, dp.UserName AS Department, S.UserName AS Section,
                                    sb.UserName SubSection, D.UserName AS Designation, EC.UserName, E.SystemId,EmployeeCodePreFix,EmployeeCodeNumeric
		                            FROM EmployeeInformation AS E
LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id 
				                           " + obs.EntityTables() + @"
							         WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + @"' AND (DOS IS NULL OR DOS>='" + _dateStart.ToString("dd-MMM-yyyy") + @"' OR EmployeeStatus='Active') 
                                    ) A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void SearchEmployeeManualAttn(string sGroupID, string sPlantID, string strKey, string fromDate, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            clsStaticInfo obs = null;

            try
            {
                obs = new clsStaticInfo();

                strSql = @"SELECT [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
		                            (SELECT CONVERT(INT, E.EmployeeCode)EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
		                            E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, E.GenderID,
                                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                                    U.UserName AS Unit, Dv.UserName  AS Division, dp.UserName AS Department, S.UserName AS Section,
                                    sb.UserName SubSection, D.UserName AS Designation, EC.UserName, E.SystemId
		                            FROM EmployeeInformation AS E
LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id 
				                           " + obs.EntityTables() + @"
							         WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + @"' AND (DOS IS NULL OR DOS>'" + fromDate + @"' OR EmployeeStatus='Active')
                                    ) A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void SearchOperation(string strKey, string strGroupID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM
                                         (SELECT TOP (100) O.Id,O.UserName Operation, O.Code OperationCode, S.Id SkillId,S.UserName Skill,PS.Id ProcessId, PS.UserName Process FROM [MST].[Operation] O
                                          LEFT JOIN HKP.Skill S ON O.SkillId=S.Id
                                          LEFT JOIN [MST].[OperationProcess] OP ON OP.OperationId=O.Id
                                          LEFT JOIN [HKP].[Process] PS ON PS.Id=OP.ProcessId
                                          ) X ";
                if (strKey.Trim() != "")
                {
                    strSQL = strSQL + " WHERE " + strKey + @"";
                }

                strSQL = strSQL + " ORDER BY Operation,Process,Skill";

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

        public void SearchMachineType(string strKey, string strGroupID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM (SELECT TOP (100) MM.* FROM [MST].[MaterialMasterArticle] MM
					LEFT JOIN MST.OperationMachineSkill OMS ON MM.Id=OMS.MaterialMasterArticleId
					WHERE OMS.MaterialMasterArticleId<>'') X ";
                if (strKey.Trim() != "")
                {
                    strSQL = strSQL + " WHERE " + strKey + @"";
                }
                strSQL = strSQL + " ORDER BY Code";

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

        public void SearchJobDescription(string strKey, string strGroupID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" Select JD.Id JobDescriptionId, JDC.UserName  AS JobDescriptionCategory,
                               JDSC.UserName AS JobDescriptionSubCategory,
                               JDI.UserName  AS JobDescriptionItem
                        From   [HKP].[JobDescription] JD
                               Left Outer Join [HKP].[JobDescriptionCategory] JDC
                                            ON JDC.Id = JD.JobDescriptionCategoryId
                               Left Outer Join [HKP].[JobDescriptionSubCategory] JDSC
                                            ON JDSC.Id = JD.JobDescriptionSubCategoryId
                               Left Outer Join [HKP].[JobDescriptionItem] JDI
                                            ON JDI.Id = JD.JobDescriptionItemId ";

                if (strKey.Trim() != "")
                {
                    strSQL = strSQL + " AND " + strKey + @"";
                }

                strSQL = strSQL + " ORDER BY JobDescriptionCategory";

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

        public void GetJobDescription(string jdid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" Select JD.Id JobDescriptionId, JDC.UserName  AS JobDescriptionCategory,
                               JDSC.UserName AS JobDescriptionSubCategory,
                               JDI.UserName  AS JobDescriptionItem
                        From   [HKP].[JobDescription] JD
                               Left Outer Join [HKP].[JobDescriptionCategory] JDC
                                            ON JDC.Id = JD.JobDescriptionCategoryId
                               Left Outer Join [HKP].[JobDescriptionSubCategory] JDSC
                                            ON JDSC.Id = JD.JobDescriptionSubCategoryId
                               Left Outer Join [HKP].[JobDescriptionItem] JDI
                                            ON JDI.Id = JD.JobDescriptionItemId
                                where JD.Id ='" + jdid + "'";

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

        public void SearchCountryName(string strKey, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT C.Id,
                                  C.UserName,
                                  C.Code,
                                  C.ShortName,
                                  C.StandardName,
                                  C.Nationality,
                                  C.Description,
                                  C.Remarks,
                                  C.GMTMinute,
                                  C.GMTHour
                           FROM   scs.Country C ";

                if (strKey.Trim() != "")
                {
                    strSQL = strSQL + " WHERE " + strKey + @"";
                }

                strSQL = strSQL + " ORDER BY StandardName";

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

        public void SearchCityName(string strKey, string countryId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"Select C.Id, C.Code, C.ShortName, C.StandardName, C.UserName, C.Description, C.Remarks From SCS.City C WHERE CountryId='" + countryId + "'";

                if (strKey.Trim() != "")
                {
                    strSQL = strSQL + " AND " + strKey + @"";
                }

                strSQL = strSQL + " ORDER BY UserName";

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

        public void SearchState(string strKey, string countryId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"Select * From [SCS].[State] WHERE CountryId='" + countryId + "'";

                if (strKey.Trim() != "")
                {
                    strSQL = strSQL + " AND " + strKey + @"";
                }

                strSQL = strSQL + " ORDER BY UserName";

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

        public void SearchDistrict(string strKey, string stateId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"Select * From [SCS].[District] WHERE StateId='" + stateId + "'";

                if (strKey.Trim() != "")
                {
                    strSQL = strSQL + " AND " + strKey + @"";
                }

                strSQL = strSQL + " ORDER BY UserName";

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

        public void SearchAreaName(string strKey, string cityId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"Select A.Id, A.CityId, A.Code, A.ShortName, A.StandardName, A.UserName, A.Description, A.Remarks From SCS.Area A WHERE A.CityId='" + cityId + "'";

                if (strKey.Trim() != "")
                {
                    strSQL = strSQL + " AND " + strKey + @"";
                }

                strSQL = strSQL + " ORDER BY UserName";

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

        public void SearchDistrictName(string sCountry, string strKey, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM
                                    (SELECT D.ID, D.StandardName DistrictName, C.StandardName CountryName
                                        FROM scs.District D
                                        LEFT JOIN SCS.[State] s	ON D.StateId = s.ID
                                        LEFT JOIN scs.Country C	ON s.CountryId = C.ID  ";

                if (sCountry.Trim() != "")
                {
                    strSQL = strSQL + " WHERE s.CountryID = '" + sCountry.Trim() + @"'";
                }

                strSQL = strSQL + ") A";

                if (strKey.Trim() != "")
                {
                    strSQL = strSQL + " WHERE " + strKey + @"";
                }

                strSQL = strSQL + " ORDER BY DistrictName";

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

        public void SearchPostOfficeName(string sCountry, string sDistrict, string strKey, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM
                                    (SELECT PO.ID, PO.UserName PostOfficeName, PO.Code PostCode, D.UserName DistrictName,
                                            S.UserName StateName, C.UserName CountryName
	                                    FROM [SCS].[PostOffice] PO
			                                    LEFT JOIN [SCS].[District] D ON PO.DistrictID = D.ID
                                                LEFT JOIN [SCS].[State] S ON D.StateId = S.ID
			                                    LEFT JOIN [SCS].[Country] C	ON S.CountryId = C.ID ";

                strSQL = strSQL + ") A";

                if (sCountry.Trim() != "")
                {
                    strSQL = strSQL + " WHERE CountryName = '" + sCountry.Trim() + @"'";
                }

                if (sDistrict.Trim() != "")
                {
                    if (sCountry.Trim() == "")
                    {
                        strSQL = strSQL + " WHERE DistrictName = '" + sDistrict.Trim() + @"'";
                    }
                    else
                    {
                        strSQL = strSQL + " AND DistrictName = '" + sDistrict.Trim() + @"'";
                    }
                }

                if (strKey.Trim() != "")
                {
                    if (sCountry.Trim() == "" & sDistrict.Trim() == "")
                    {
                        strSQL = strSQL + " WHERE " + strKey + @"";
                    }
                    else
                    {
                        strSQL = strSQL + " AND " + strKey + @"";
                    }
                }

                strSQL = strSQL + " ORDER BY PostOfficeName";

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

        public void SearchJobLocation(string sGroupID, string strKey, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SystemID ID, JobLocation JobLocationName
                    FROM JobLocation WHERE PlantId IN (SELECT Id FROM [ORG].Plant WHERE CompanyGroupId = '" + sGroupID + @"')";

                if (strKey.Trim() != "")
                {
                    strSQL = strSQL + " AND " + strKey + @"";
                }

                strSQL = strSQL + " ORDER BY JobLocation";

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

        public void GetEmployeeShiftAssign(string strEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TOP(1) * FROM EmployeeShiftAssign
                                    WHERE EmpSystemID = '" + strEmpSystemID + @"' ORDER BY EffectiveDate DESC";

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

        public void GetEmployeeShiftAssignment(string systemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.ShiftDefination WHERE SystemID='" + systemID + "'";

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

        public void GetEmployeeShiftInfo(string strEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT ISNULL(sf.ShiftDefinationDescription,'') +''+ISNULL(sr.ShiftDefinationDescription,'') ShiftName
                                    ,r.ShiftRosterName,a.IsFix,Replace(CONVERT(VARCHAR(11), a.EffectiveDate, 106), ' ', '-') EffectiveDate
                                    ,WeekOff=CASE  WHEN IsAlignWithCC=1 THEN 'As Per Company Calendar'
									WHEN IsFixedDayInMonthWeekOff=1 THEN 'On Date: '+FixedDayInMonthWeekOff+' in the month'
									WHEN IsDaysLengthWeekOff=1 then+WeekOffDay
									ELSE 'Every after '+WeekOffInShiftLenght+' days of roster'
									END
                                    FROM
                                    (SELECT TOP(1) * FROM EmployeeShiftAssign WHERE EmpSystemID = '" + strEmpSystemID + @"'
                                    ORDER BY EffectiveDate DESC) a
                                    LEFT JOIN ShiftDefination sf ON sf.SystemID=a.FixSystemID
                                    LEFT JOIN ShiftDefination sr ON sr.SystemID=a.RosterStartShiftID
                                    LEFT JOIN ShiftRosterMaster r ON r.SystemID=a.RosterSystemID";

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

        public void GetEmployeeShiftInfo(string strEmpSystemID, string attndDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT DISTINCT sf.ShiftDefinationName Fixed,sr.ShiftDefinationName Roster
                                    ,r.ShiftRosterName,a.IsFix,Replace(CONVERT(VARCHAR(11), a.EffectiveDate, 106), ' ', '-') EffectiveDate
                                    ,WeekOff=case  when IsAlignWithCC=1 then 'As Per Company Calendar'
									WHEN IsFixedDayInMonthWeekOff=1 then 'On Date: '+FixedDayInMonthWeekOff+' in the month'
									WHEN IsDaysLengthWeekOff=1 then+WeekOffDay
									ELSE 'Every after '+WeekOffInShiftLenght+' days of roster'
                                    END
									,sfn.ShiftDefinationName +' (' + Replace(CONVERT(VARCHAR(5), sfn.InTime, 108), ' ', '-')+' - '+
                                    Replace(CONVERT(VARCHAR(5), sfn.OutTime, 108), ' ', '-')+')'ShiftDefinationName
	                                ,sfn.ShiftDefinationDescription
                                    ,CONVERT(VARCHAR(15),CAST(LIT.ptime AS TIME),100)  +' ('+ ARD.PType+')' LeastPunchTime, APD.DayStatus
                                    FROM
                                    (Select * from EmpDateWiseShiftAssign Where WorkDate='" + attndDate + @"' AND EmpSystemID = '" + strEmpSystemID + @"')EDS
                                    LEFT JOIN EmployeeShiftAssign a on EDS.EmpSftAssiSystemID = a.SystemID
                                    LEFT JOIN ShiftDefination sf on sf.SystemID=a.FixSystemID
                                    LEFT JOIN ShiftDefination sr on sr.SystemID=a.RosterStartShiftID
                                    LEFT JOIN ShiftRosterMaster r on r.SystemID=a.RosterSystemID
									LEFT JOIN ShiftDefination sfn   on sfn.SystemID=eds.ShiftSystemID
                                    LEFT JOIN
												(
												SELECT LogDownLoadNum
												,MIN(ptime) ptime
												FROM AttdnRawData
												WHERE pdate='" + attndDate + @"'
												GROUP BY LogDownLoadNum
												) LIT on LIT.LogDownLoadNum=a.EmpSystemID
									LEFT JOIN AttdnRawData ARD ON ARD.LogDownLoadNum=LIT.LogDownLoadNum  AND ARD.PTime=LIT.ptime
                                    LEFT JOIN (SELECT * FROM  AttdnProcessData Where WorkDate='" + attndDate + @"') APD ON APD.EmpSystemID=a.EmpSystemID";

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

        public void GetEmpDateWiseShiftAssign(string strEmpSystemID, string WorkDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmpDateWiseShiftAssign
                                    WHERE EmpSystemID = '" + strEmpSystemID + @"' and WorkDate='" + WorkDate + "'";

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

        public void GetEmployeeOTEntitle(string strEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TOP(1) * FROM EmployeeOTEntitle
                                    WHERE EmpSystemID = '" + strEmpSystemID + @"' ORDER BY OTStartDate DESC";

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
        public void GetEmployeeOTEntitleAll(string strEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT 
                                Replace(CONVERT(VARCHAR(11), OTStartDate, 106), ' ', '-') OTStartDate
                                ,Replace(CONVERT(VARCHAR(11), OTEndDate, 106), ' ', '-') OTEndDate
                                ,IsOTEntitle
                                 FROM EmployeeOTEntitle
                                    WHERE EmpSystemID = '" + strEmpSystemID + @"' ORDER BY OTStartDate DESC";

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

        public void GetEmpDateWiseJobLocation(string sEmpSystemID, string sJobLcSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TOP(1) * FROM EmpDateWiseJobLocation
                            WHERE EmpSystemID = '" + sEmpSystemID + @"' AND JobLcSystemID = '" + sJobLcSystemID + @"'
                            ORDER BY EffectiveDate DESC";

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

        public void GetLastEffectiveDateEmpDateWiseJobLocation(string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TOP(1) * FROM EmpDateWiseJobLocation
                            WHERE EmpSystemID = '" + sEmpSystemID + @"'
                            ORDER BY EffectiveDate DESC";

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

        public void GetEmpDateWiseJobLocationEffetDtAtferAnyDateFound(string sEmpSystemID, string sEffectiveDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TOP(1) * FROM EmpDateWiseJobLocation
                            WHERE EmpSystemID = '" + sEmpSystemID + @"' AND EffectiveDate > '" + sEffectiveDate + @"'
                            ORDER BY EffectiveDate";

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

        public void GetEmployeeWeekOffByDay(string strEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT TOP(1) * FROM EmployeeWeekOffByDay
                                    WHERE EmpSystemID = '" + strEmpSystemID + "' ORDER BY EffectiveDate DESC";

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

        public void DeleteEmployeePayrollGroup(string id)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("DELETE FROM MST.PayrollGroupMaster WHERE Id='" + id + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                }
                catch (Exception)
                {
                    throw (ex);
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }

        public void DeleteEmployeeAttendanceGroup(string id)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("DELETE FROM MST.EmployeeAttendanceGroup WHERE Id='" + id + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                }
                catch (Exception)
                {
                    throw (ex);
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }

        public void UpdateEmployeePaymentMode(string empSystemId, string paymentMode)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("UPDATE EmployeeInformation SET PaymentMode='" + paymentMode + @"' Where SystemId='" + empSystemId + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                }
                catch (Exception)
                {
                    throw (ex);
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }

        public void DeleteEmployeeBasicInfo(string strEmpSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                string delete = "Delete from LeaveTransactionDetails where [LvTrnsSystemID] in (Select SystemID From [dbo].[LeaveTransaction] where EmpSystemID = '" + strEmpSystemID + "') ";
                objCon.ExecuteNonQueryWrapper(delete, true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from LeaveTransaction where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from LeaveAllocation where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from LvPolMstTagEmp where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from EmpTrainingInformation where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from EmployeeOperation where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from EmployeeMachineType where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from EmployeeOTEntitle where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from EmployeeImage where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from EmployeeShiftAssign where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from EmployeeWeekOffByDay where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from EmpExperienceInformation where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from EmpAcademicQualificationInformation where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from EmpReferenceInformation where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from EmpSkillInformation where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from TRN.EmployeeJobDescription where EmployeeId = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from dbo.EmployeeDocument where EmpSystemID = '" + strEmpSystemID + "'", true, "1");
                //objCon.ExecuteNonQueryWrapper("Delete from SEC.[User] where EmployeeId = '" + strEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from EmployeeInformation where SystemID = '" + strEmpSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                }
                catch (Exception)
                {
                    throw (ex);
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmpWeekOffByDay(string strSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from EmployeeWeekOffByDay where SystemID = '" + strSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmployeeOperation(string sSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from EmployeeOperation WHERE SystemID = '" + sSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmployeeMachineType(string sSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from EmployeeMachineType WHERE SystemID = '" + sSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmployeeReporting(string sSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from EmpReportingPerson WHERE SystemID = '" + sSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmpFingerPrint(string sSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from EmployeeFingerPrint WHERE Id = '" + sSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }
        public void GetEmpFingerPrintFile(string sID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.EmployeeFingerPrint WHERE Id = '" + sID + @"'";

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
        }
        public void DeleteEmployeeEducation(string sSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from EmpAcademicQualificationInformation WHERE SystemID = '" + sSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteSkill(string sSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from EmpSkillInformation WHERE SystemID = '" + sSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmployeeTraining(string sSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from EmpTrainingInformation WHERE SystemID = '" + sSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmployeeEx(string sSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from EmpExperienceInformation WHERE SystemID = '" + sSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmployeeNominee(string sSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete FROM [dbo].[EmployeeNomineeInfo] WHERE Id = '" + sSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmployeeDependant(string sSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete FROM [dbo].[EmployeeDependantInfo] WHERE Id = '" + sSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void DeleteEmployeeLand_Lord(string sSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete FROM [dbo].[EmployeeLandLordInfo] WHERE Id = '" + sSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmployeeJD(string sSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from  [TRN].[EmployeeJobDescription] WHERE Id = '" + sSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmployeeJobLocation(string sSystemID, string sEmpSystemID, string sEffectiveDate)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("DELETE FROM EmpDateWiseJobLocation WHERE SystemID = '" + sSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("DELETE FROM AttdnProcessData WHERE WorkDate >= '" + sEffectiveDate + "' AND EmpSystemID = '" + sEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("DELETE FROM EmpDateWiseShiftAssign WHERE WorkDate >= '" + sEffectiveDate + "' AND EmpSystemID = '" + sEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("DELETE FROM EmployeeShiftAssign WHERE EffectiveDate >= '" + sEffectiveDate + "' AND EmpSystemID = '" + sEmpSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteShiftAndAttdnInfoBeforeEmployeeJobLocation(string sEmpSystemID, string sEffectiveDate)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("DELETE FROM AttdnProcessData WHERE WorkDate >= '" + sEffectiveDate + "' AND EmpSystemID = '" + sEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("DELETE FROM EmpDateWiseShiftAssign WHERE WorkDate >= '" + sEffectiveDate + "' AND EmpSystemID = '" + sEmpSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("DELETE FROM EmployeeShiftAssign WHERE EffectiveDate >= '" + sEffectiveDate + "' AND EmpSystemID = '" + sEmpSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteBankInfo(string strRowID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from EmployeeBankInfo where RowID = '" + strRowID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void GetUserGroup(string sGroupID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM CentralPayRollGroup WHERE GroupID = '" + sGroupID + @"' ";

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

        public void GetSkill(string sGroupID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from [HKP].[Skill] WHERE CompanyGroupID = '" + sGroupID + @"' and Active=1 order by [Sequence]";
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

        public void GetOperationCategory(string skillid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT DISTINCT O.OperationCategoryId [Id], OC.UserName
                                    FROM [MST].[Operation] O
                                    JOIN [HKP].[OperationCategory] OC on OC.Id=O.OperationCategoryId
                                    WHERE O.SkillId='" + skillid + @"'";
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

        public void GetOperation(string OperationCategoryId, string skillid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @" select Id,UserName from [MST].[Operation]
                //                  where OperationCategoryId= '" + OperationCategoryId + @"'
                //                  ";
                strSQL = @"SELECT O.Id,
                                    O.UserName
                                    FROM [MST].[Operation] O
                                    JOIN [HKP].[OperationCategory] OC on OC.Id=O.OperationCategoryId
                                    WHERE O.SkillId='" + skillid + @"' AND O.OperationCategoryId='" + OperationCategoryId + @"' ORDER BY  O.UserName";
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

        public void GetBankList(string EmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" Select EBI.*, B.UserName AS Bank,bb.UserName BankBranch From  [dbo].[EmployeeBankInfo] EBI
                                  LEFT OUTER JOIN HKP.Bank B ON B.Id=EBI.BankSystemID
                                    left outer join hkp.Bankbranch bb on bb.BankId=Ebi.BankSystemID
								                                        and bb.Id=ebi.BankBranchId
                                  where EmpSystemID= '" + EmpSystemID + @"'";
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

        public void SearchBank(string rowid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @" Select EBI.*, B.UserName AS Bank From  [dbo].[EmployeeBankInfo] EBI
                //            LEFT OUTER JOIN MST.BankMaster BM ON BM.Id=EBI.BankSystemID
                //            LEFT OUTER JOIN HKP.Bank B ON B.Id=BM.BankId
                //            where EmpSystemID= '"+ EmpSystemID + "' AND EBI.BankSystemID='"+ BankSystemID+"' AND EBI.BankAccNo='"+ BankAccNo+"'";
                strSQL = @"Select EBI.*, B.UserName BankName,bb.Id BankBranchId,bb.UserName BankBranch,Ei.PaymentMode From  [dbo].[EmployeeBankInfo] EBI
                            LEFT OUTER JOIN HKP.Bank B ON B.Id=EBI.BankSystemID
							left outer join hkp.BankBranch bb on bb.BankId=b.Id
                            left outer join EmployeeInformation EI on EI.SystemId=EBI.EmpSystemID
                            where EBI.RowId= '" + rowid + "'";
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

        public void SearchBankList(string EmpSystemID, string BankSystemID, string BankBranchId, string BankAccNo, string rowId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" Select EBI.*, B.UserName AS Bank From  [dbo].[EmployeeBankInfo] EBI
                            LEFT OUTER JOIN HKP.Bank B ON B.Id=EBI.BankSystemID
                            where EmpSystemID= '" + EmpSystemID + "' AND EBI.BankSystemID='" + BankSystemID + "' and EBI.BankBranchId='" + BankBranchId + @"' AND EBI.BankAccNo='" + BankAccNo + "'AND EBI.RowID<>'" + rowId + "'";
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

        public void GetEmployeeGroup(string sGroupID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM HKP.EmployeeGroup WHERE CompanyGroupId = '" + sGroupID + @"' order by UserName";

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
        public void GetJobLocationPlantWise(string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT j.SystemID,j.JobLocation JobLocation FROM JobLocation j                           
                             WHERE j.plantid ='" + PlantId + @"' ORDER BY JobLocation";

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
        public void GetJobLocation(string sGroupID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT j.SystemID,j.JobLocation+' '+p.username JobLocation FROM JobLocation j
                            LEFT JOIN ORG.Plant p on p.Id=j.plantid
                             WHERE CompanyGroupId ='" + sGroupID + @"' ORDER BY JobLocation";

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
        public void GetEntityPlantwise(string Plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"  select Id,UserName from org.Entity where PlantId='" + Plantid + "' and Active=1 and Archive=0 ORDER BY UserName";

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

        public void GetJobLocationByPlant(string sPlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT j.SystemID,j.JobLocation+' '+p.username JobLocation FROM JobLocation j
                            LEFT JOIN ORG.Plant p on p.Id=j.plantid
                             WHERE j.plantid ='" + sPlantId + @"' ORDER BY JobLocation";

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

        public void GetJobLocationJbLcSystemID(string sSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM JobLocation WHERE SystemID = '" + sSystemID + @"' order by JobLocation";

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

        public void GetReligionName(string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM [SCS].Religion ";

                if (strName != "")
                {
                    strSQL = strSQL + " StandardName = '" + strName + "'";
                }
                strSQL += " order by UserName";
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

        public void GetSalutationName(string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM HKP.Salutation ";

                if (strName != "")
                {
                    strSQL = strSQL + " StandardName = '" + strName + "'";
                }
                strSQL += " order by UserName";
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

        public void GetSettingPlantWise(string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select SystemID from PlantWiseHRMSSetting where plantid='" + plantid + "' and IsPositionCodeApplicable=1";
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

        public void GetSettingPlantWise(CustomPara _para, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select c.CutOffDate,p.* from PlantWiseHRMSSetting p
                                    left join  (select * from[SCS].[OpeningBalanceCutOffDate] where ModuleName = 'HR')c on c.PlantId = p.PlantID
                                    where p.PlantID = '" + _para.PlantId + "'";
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

        public void GetSalaryRule(string GivenDesignation, string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                //       strSQL = @"select s.SystemID,s.SalaryRuleName from SalaryRuleMaster s
                //                           left outer join mst.DesignationMaster dm on dm.SalaryRuleMasterId=s.SystemID
                //where dm.DesignationId='" + GivenDesignation + "' and s.Plantid='" + PlantId + "'";
                strSQL = @"select s.SystemID,s.SalaryRuleName from SalaryRuleMaster s
                                    left outer join (
                                                        SELECT DC.SalaryRuleMasterId,dc.plantid,dm.* 
                                                                                FROM MST.DesignationMaster DM
							                                                    LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                                                            ON DM.Id=DC.DesignationMasterId
                                                where dc.plantid='" + PlantId + @"'
                                                ) dm on dm.SalaryRuleMasterId=s.SystemID and s.IsActive=1
									where dm.DesignationId='" + GivenDesignation + "' and s.Plantid='" + PlantId + "'";

                //and p.PlantId='" + PlantId + "'";
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

        public void GetDesignationGroup(string GivenDesignation, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                            SELECT dm.DesignationGroupId
	                            , dg.UserName DesignationGroup
	                            , d.UserName Designation
                            FROM hkp.Designation d
							 LEFT outer JOIN mst.DesignationMaster dm   ON d.Id = dm.DesignationId
                            LEFT outer JOIN hkp.DesignationGroup dg ON dg.Id = dm.DesignationGroupId
                            WHERE d.Id = '" + GivenDesignation + @"'
                            ";
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

        public void GetBloodGroupName(string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM [HKP].BloodGroup";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE StandardName = '" + strName + "'";
                }
                strSQL += " order by UserName";
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

        public void GetCountryName(string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM [SCS].Country";

                if (strName != "")
                {
                    strSQL = strSQL + @"
                                        WHERE StandardName = '" + strName + "'";
                }

                strSQL = strSQL + @"
                                  ORDER BY Nationality ";

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

        public void GetLanguage(string plantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT P.LanguageId, L.UserName LanguageName, L.Code From ORG.Plant P
                            LEFT JOIN SCS.Language L ON P.LanguageId = L.Id
                            WHERE p.Id='" + plantId + "' ";
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

        public void checkSalaryPercentage(string empId, string Rowid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"Select isnull(Sum(EBI.SalaryPercentage),0) SalaryPercentage From dbo.EmployeeBankInfo EBI Where
                           RowID <> '" + Rowid + @"'
                           AND EmpSystemID='" + empId + "'";
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
        }

        public void GetCivilStatus(string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM HKP.CivilStatus";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE StandardName = '" + strName + "'";
                }
                strSQL += " order by UserName";
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

        public void GetGender(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM Gender";

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

        public void GetDistrictName(string sDistrictID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM
                                    (SELECT D.Id SystemID, D.UserName DistrictName, C.Id CountryID, C.UserName CountryName
                                        FROM scs.District D
                                        LEFT JOIN scs.[State] s	ON D.StateId = s.ID
                                        LEFT JOIN scs.Country C	ON s.CountryId = C.ID
                                        WHERE D.ID = '" + sDistrictID.Trim() + @"') A";

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

        public void GetThanaName(string sThanaID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM
                                    (	SELECT T.Id SystemID, T.UserName ThanaName, D.ID DistrictID, D.UserName DistrictName,
						                        C.Id CountryID, C.UserName CountryName
                                        FROM scs.PoliceStation T
                                        LEFT JOIN scs.District D ON T.DistrictID = D.Id
                                        LEFT JOIN scs.[State] s	ON D.StateId = s.ID
                                        LEFT JOIN scs.Country C	ON s.CountryId = C.ID

                                        WHERE T.ID = '" + sThanaID.Trim() + @"') A";

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

        public void GetThana(string strKey, string districtId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SCS.PoliceStation  WHERE DistrictId='" + districtId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetPostOffice(string strKey, string districtId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SCS.PostOffice  WHERE DistrictId='" + districtId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetPostOffice(string sPOID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM
                                    (SELECT PO.Id SystemID, PO.UserName PostOfficeName, PO.Code PostCode,
                                            --T.Id ThanaID, T.UserName ThanaName,
                                            D.ID DistrictID, D.UserName DistrictName, C.Id CountryID, C.UserName CountryName
											FROM scs.PostOffice PO
											LEFT JOIN scs.District D ON po.DistrictID = D.Id
											--LEFT JOIN scs.PoliceStation T ON D.Id = T.DistrictID
											LEFT JOIN scs.[State] s	ON D.StateId = s.ID
											LEFT JOIN scs.Country C	ON s.CountryId = C.ID
                                        WHERE PO.ID = '" + sPOID.Trim() + @"') A";
                //strSQL = @"SELECT * FROM
                //                                  (SELECT PO.SystemID, PO.PostOfficeName, PO.PostCode, T.SystemID ThanaID, T.ThanaName,
                //                                          D.SystemID DistrictID, D.DistrictName, C.CountryID, C.Name CountryName
                //                                   FROM PostOffice PO
                //	                                    LEFT JOIN Thana T ON PO.ThanaSystemID = T.SystemID
                //	                                    LEFT JOIN District D ON T.DistrictSystemID = D.SystemID
                //	                                    LEFT JOIN Country C	ON D.CountryID = C.CountryID
                //                                      WHERE PO.SystemID = '" + sPOID.Trim() + @"') A";

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

        public void GetChildRelationshipCbo(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Id,UserName FROM SCS.Relationship WHERE IsChild=1";
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

        public void GetRelationshipCbo(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Id,UserName FROM SCS.Relationship";
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

        public void GetPayrollGroupCbo(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Id,UserName FROM [HKP].[PayrollGroup] ORDER BY UserName";
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


        public void GetAttendanceGroupCbo(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Id,UserName FROM [dbo].[AttendanceGroup] ORDER BY UserName";
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

        public void GetEmployeeAttendanceGroup(string employeeId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from EmployeeAttendanceGroup where EmployeeId='" + employeeId + "'";
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

        public void GetEmployeePayrollGroupMaster(string employeeId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from MST.PayrollGroupMaster where EmployeeId='" + employeeId + "'";
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

        public void GetProfessionCbo(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Id,UserName FROM SCS.Profession";
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

        public void GetEmpInfoGrid(string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT E.*
	                            FROM [dbo].[EmployeeInformation] E
				                            INNER JOIN [dbo].[SalaryInfoDefine] S ON E.SystemID = S.EmpInfoSystemID
                            WHERE E.DOJ <= '" + sToDate + @"'
                                  AND (E.DOS > '" + sFromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL OR E.DOS = ''
                                       OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"AND E.PlantID = '" + sPlantID + @"'";
                }

                strSQL = strSQL + @" ORDER BY E.EmployeeCode";

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

        public void GetMachineType(string sSystemID, string strGroupID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT MT.SystemID, MT.Code, MT.Descript, MT.ProcessID, P.ProcessName, MT.MachineClassSysID,
                //              MC.Code MachineClassCode, MC.ClassName
                //            FROM dbo.FGMachineType MT
                //                  LEFT JOIN Process P ON MT.ProcessID = P.ProcessID
                //                  LEFT JOIN FGMachineClass MC ON MT.MachineClassSysID = MC.SystemID
                //            WHERE MT.GroupID = '" + strGroupID + @"' AND MT.SystemID = '" + sSystemID + @"'
                //            ORDER BY MT.Code";
                //strSQL = @" SELECT MT.id             SystemID,
                //        MT.code,
                //        MT.[description]  Descript,
                //        '' processid,
                //        ''        ProcessName,
                //        MT.machineclassid MachineClassSysID,
                //        MC.code           MachineClassCode,
                //        MC.username       ClassName
                //FROM   mst.machinetype MT
                //        --LEFT JOIN (SELECT * FROM   hkp.process  WHERE  companygroupid = 'CG20171') P ON MT.processid = P.id
                //        LEFT JOIN hkp.machineclass MC
                //                ON MT.machineclassid = MC.id
                //WHERE  MT.id = '" + sSystemID + @"'   ";
                strSQL = @" SELECT MT.id SystemID,
                        MT.code,
						MT.ShortName,
                        MT.StandardName,
                        ''UserName,
                        '' Descript,
                        '' processid,
                        '' ProcessName,
                        '' MachineClassSysID,
                        '' MachineClassCode,
                        '' ClassName
                FROM   [MST].[MaterialMasterArticle] MT
                WHERE  MT.id = '" + sSystemID + @"'";

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

        public void GetUnitName(string sGroupID, string CompanyID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [ORG].Unit u
                                left outer join org.CompanyGroupUnit c on c.UnitId=u.Id
                               WHERE c.CompanyGroupId = '" + sGroupID + @"' ";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE u.UserName = '" + strName + "'";
                }
                strSQL += " order by u.UserName";
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

        public void GetDivisionName(string sGroupID, string CompanyID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [ORG].Division
                                   WHERE Id IN (SELECT DivisionId FROM [ORG].[CompanyGroupDivision] WHERE CompanyGroupId = '" + sGroupID + @"' )";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE UserName = '" + strName + "'";
                }
                strSQL += " order by UserName";
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

        public void GetSubdivisionName(string sGroupID, string CompanyID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [ORG].Subdivision
                                   WHERE Id IN (SELECT subdivisionId FROM [ORG].[CompanySubDivision] WHERE CompanyGroupId = '" + sGroupID + @"' AND CompanyId = '" + CompanyID + @"')";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE StandardName = '" + strName + "'";
                }
                strSQL += " order by UserName";
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

        public void GetDepartmentName(string sGroupID, string CompanyID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM [ORG].Department
                //                  WHERE Id IN (SELECT DepartmentId FROM [ORG].[CompanyDepartment] WHERE CompanyGroupId = '" + sGroupID + @"' AND CompanyId = '" + CompanyID + @"')";

                strSQL = @"SELECT * FROM [ORG].Department
                           WHERE Id IN (SELECT Id FROM [ORG].[CompanyGroupDepartment] WHERE CompanyGroupId = '" + sGroupID + @"')";
                if (strName != "")
                {
                    strSQL = strSQL + " WHERE UserName = '" + strName + "'";
                }
                strSQL += " order by UserName";
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

        public void GetSectionName(string sGroupID, string CompanyID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM [ORG].Section
                //                    WHERE Id IN (SELECT SectionId FROM [ORG].[CompanySection] WHERE CompanyGroupId = '" + sGroupID + @"' AND CompanyId = '" + CompanyID + @"')";
                strSQL = @"SELECT * FROM [ORG].Section
                           WHERE Id IN (SELECT SectionId FROM [ORG].[CompanyGroupSection] WHERE CompanyGroupId = '" + sGroupID + @"')";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE UserName = '" + strName + "'";
                }
                strSQL += " order by UserName";
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

        public void GetSectionCbo(string sGroupID, string CompanyID, string strName, string sysadmin, string userId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM [ORG].Section
                //                    WHERE Id IN (SELECT SectionId FROM [ORG].[CompanySection] WHERE CompanyGroupId = '" + sGroupID + @"' AND CompanyId = '" + CompanyID + @"')";
                if (sysadmin == "False")
                {
                    strSQL = @"Select ID,UserName from (SELECT ID,UserName FROM [ORG].Section
                      WHERE Id IN(SELECT SectionId FROM[ORG].[CompanyGroupSection] WHERE CompanyGroupId = '" + sGroupID + @"')
                      AND Id IN(Select SectionId from [SEC].[UserSection] AS US
                     INNER JOIN(Select Id from [SEC].[User] Where Id = '" + userId + @"')U ON U.Id = US.UserId)
					 )A";
                }
                else
                {
                    strSQL = @"SELECT * FROM [ORG].Section
                           WHERE Id IN (SELECT SectionId FROM [ORG].[CompanyGroupSection] WHERE CompanyGroupId = '" + sGroupID + @"')";
                }
                if (strName != "")
                {
                    strSQL = strSQL + " WHERE UserName = '" + strName + "'";
                }
                strSQL += " order by UserName";
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

        public void GetSubSectionName(string sGroupID, string CompanyID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM [ORG].SubSection
                //                    WHERE Id IN (SELECT SubSectionId FROM [ORG].[CompanySubSection] WHERE CompanyGroupId = '" + sGroupID + @"' AND CompanyId = '" + CompanyID + @"')";

                strSQL = @"SELECT * FROM [ORG].SubSection
                          WHERE Id IN (SELECT SubSectionId FROM ORG.CompanyGroupSubSection WHERE CompanyGroupId = '" + sGroupID + @"')";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE UserName = '" + strName + "'";
                }
                strSQL += " order by UserName";
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

        public void GetLineInfo(string sGroupID, string CompanyID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM [ORG].Line
                //                    WHERE Id IN (SELECT SubSectionId FROM [ORG].[CompanySubSection] WHERE CompanyGroupId = '" + sGroupID + @"' AND CompanyId = '" + CompanyID + @"')";

                strSQL = @"SELECT * FROM [ORG].Line
                           WHERE Id IN (SELECT SubSectionId FROM [ORG].CompanyGroupSubSection WHERE CompanyGroupId = '" + sGroupID + @"')";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE StandardName = '" + strName + "'";
                }
                strSQL += " order by UserName";
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

        public void GetBudgetCategory(string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [HKP].[EmployeeBudgetCategory] ";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE StandardName = '" + strName + "'";
                }
                strSQL += " order by UserName";
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

        public void GetBudgetCategory(string sGroupID, string CompanyID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.BudgetCategory
                                    WHERE SystemID IN (SELECT BudgetCategoryID FROM dbo.BudgetCategoryExtension
                                                      WHERE GroupID = '" + sGroupID + @"' AND CompanyID = '" + CompanyID + @"')";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE BudgetCategoryName = '" + strName + "'";
                }

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

        public void GetEmployeeCategory(string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM [HKP].[EmployeeCategory] ";
                strSQL = @"SELECT * FROM [HKP].[EmployeeCategory] ";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE UserName = '" + strName + "'";
                }
                strSQL += " order by UserName";
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

        public void GetEmployeeCategory(string sGroupID, string CompanyID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.EmpCategory
                                    WHERE SystemID IN (SELECT EmpCategoryID FROM dbo.EmpCategoryExtension
                                                      WHERE GroupID = '" + sGroupID + @"' AND CompanyID = '" + CompanyID + @"')";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE EmpCategoryName = '" + strName + "'";
                }

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

        public void GetDesignationGroup(string sGroupID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [HKP].[DesignationGroup]
                                    WHERE Id IN (SELECT DesignationGroupId FROM [HKP].[CompanyGroupDesignationGroup] WHERE CompanyGroupId = '" + sGroupID + @"')";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE UserName = '" + strName + "'";
                }

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

        public void GetDesignationGroup(string sGroupID, string CompanyID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM HKP.DesignationGroup
                                    WHERE SystemID IN (SELECT DesignationGroupID FROM dbo.DesignationGroupExtension
                                                      WHERE GroupID = '" + sGroupID + @"' AND CompanyID = '" + CompanyID + @"')";

                if (strName != "")
                {
                    strSQL = strSQL + " WHERE DesignationGroupName = '" + strName + "'";
                }

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

        public void GetDesignationGroupByDesignationId(string sGroupID, string designationid, string plantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //        strSQL = @" SELECT
                //                A.Id,A.UserName,A.StandardName,B.EmployeeCategoryId EmployeeCategoryId,t.UserName EmployeeCategory,B.IsOTEntitled
                //                FROM [HKP].DesignationGroup A
                //                 INNER JOIN (SELECT DC.IsOTEntitled,DM.DesignationGroupId,DM.EmployeeCategoryId FROM SCS.DesignationMasterConfiguration DC 
                //LEFT JOIN [MST].[DesignationMaster] DM ON DM.Id=DC.DesignationMasterId where DM.DesignationId='" + designationid + @"' and  DM.CompanyGroupId='" + sGroupID + @"' AND DC.PlantId='" + plantId + @"') B
                //                ON A.Id = B.DesignationGroupId
                //                LEFT JOIN HKP.EmployeeCategory t on t.Id=B.EmployeeCategoryId
                //                ";
                strSQL = @"SELECT
                        A.Id,A.UserName,A.StandardName,B.EmployeeCategoryId EmployeeCategoryId,t.UserName EmployeeCategory
						
                        FROM [HKP].DesignationGroup A
                         INNER JOIN (
						 
						 SELECT 
						 DM.DesignationGroupId,DM.EmployeeCategoryId
						  FROM 
						  [MST].[DesignationMaster] DM 
						  where DM.DesignationId='" + designationid + @"' and  DM.CompanyGroupId='" + sGroupID + @"' 
						  ) B
                        ON A.Id = B.DesignationGroupId
                        LEFT JOIN HKP.EmployeeCategory t on t.Id=B.EmployeeCategoryId";

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
        public void GetOTInfo(string sGroupID, string designationid, string plantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT
                        A.Id,A.UserName,A.StandardName,B.EmployeeCategoryId EmployeeCategoryId,t.UserName EmployeeCategory,B.IsOTEntitled
                        FROM [HKP].DesignationGroup A
                         INNER JOIN (SELECT DC.IsOTEntitled,DM.DesignationGroupId,DM.EmployeeCategoryId FROM SCS.DesignationMasterConfiguration DC 
						  LEFT JOIN [MST].[DesignationMaster] DM ON DM.Id=DC.DesignationMasterId where DM.DesignationId='" + designationid + @"' and  DM.CompanyGroupId='" + sGroupID + @"' AND DC.PlantId='" + plantId + @"') B
                        ON A.Id = B.DesignationGroupId
                        LEFT JOIN HKP.EmployeeCategory t on t.Id=B.EmployeeCategoryId
                        ";

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

        public void GetLegalDesignationByGivenDesignation(string sGroupID, string companyid, string givenDesignationId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                ///170613
                //strSQL = @" select LD.* from [HKP].[LegalDesignation] LD
                //        left outer join [MST].[DesignationMasterLegalDesignation] LDM on ldm.LegalDesignationId=LD.Id
                //        where LDM.DesignationMasterId in
                //        (select id from [MST].DesignationMaster where DesignationId='"+ designationid + @"')
                //        and LDM.DesignationMasterId in
                //        (select DesignationMasterId from [MST].CompanyDesignation where CompanyId='" + companyid + @"')
                //        and LD.Id in
                //        (select LegalDesignationId from HKP.CompanyGroupLegalDesignation where CompanyGroupId='"+ sGroupID + "')";

                //         strSQL = @" Select LD.* from [HKP].[LegalDesignation] LD
                //                 left outer join [MST].[DesignationMasterLegalDesignation] LDM on ldm.LegalDesignationId=LD.Id
                //                 left outer join MST.DesignationMaster DM on DM.Id=LDM.DesignationMasterId
                //                 where  LD.Id in
                //                 (select LegalDesignationId from HKP.CompanyGroupLegalDesignation where CompanyGroupId='" + sGroupID + @"')
                //                 and DM.DesignationGroupId ='" + DesignationGroupId + @"'
                //";
                strSQL = @" Select LD.* from [HKP].[LegalDesignation] LD
                        left outer join [MST].[DesignationMasterLegalDesignation] LDM on ldm.LegalDesignationId=LD.Id
                        left outer join MST.DesignationMaster DM on DM.Id=LDM.DesignationMasterId
						Where DM.Designationid='" + givenDesignationId + "'";

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

        public void GetEmployeeTypeByGivenDesignation(string givenDesignationId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                ///170613
                strSQL = @" select EmployeeCategoryId from [MST].DesignationMaster where DesignationId='" + givenDesignationId + @"' and isnull(EmployeeCategoryId,'')<>''";

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

        public void GetEmployeeCityMandatory(string plantId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                ///170613
                strSQL = @"Select IsCityMandatory from [dbo].[PlantWiseHRMSSetting] Where PlantID='" + plantId + "'";

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

        public void GetDesignationName(string strDesigGroup, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM HKP.Designation
			                        WHERE DesignationGroupID = '" + strDesigGroup + @"'
                                    ORDER BY DesignationName";

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

        public void GetDesignationName(string sGroupID, string strDesigGroup, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT A.* FROM [HKP].[Designation] A
                                     INNER JOIN (SELECT * FROM [MST].[DesignationMaster]
                                                        WHERE CompanyGroupId = '" + sGroupID + @"'
                                                            AND DesignationGroupId = '" + strDesigGroup + @"')B ON A.Id = B.DesignationId
                                    ORDER BY UserName";

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

        public void GetDesignationNameByCompanyId(string CompanyId, string GroupId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT A.* FROM [HKP].[Designation] A
                //INNER JOIN (SELECT * FROM [MST].[DesignationMaster] WHERE CompanyGroupId = '" + GroupId + @"')B ON A.Id = B.DesignationId
                //INNER JOIN (SELECT * FROM [MST].[CompanyDesignation]
                //                    WHERE CompanyId = '" + CompanyId + @"')c ON B.Id = c.DesignationMasterId
                //ORDER BY UserName";

                strSQL = @"SELECT m.DesignationGroupId,A.* FROM [HKP].[Designation] A
                INNER JOIN (SELECT * FROM [hkp].[CompanyGroupDesignation] WHERE CompanyGroupId = '" + GroupId + @"')B
				ON A.Id = B.DesignationId
				INNER JOIN (SELECT * FROM [MST].[DesignationMaster] WHERE CompanyGroupId = '" + GroupId + @"') m
				 ON A.Id = m.DesignationId

                ORDER BY UserName";
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

        public void GetDesignationSearch(string plantid, string CompanyId, string GroupId, string strKey, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from
                        (SELECT
                                 A.Id
                                ,A.UserName	Designation
                                ,A.Code
                                ,A.ShortName
                                ,A.StandardName
                                ,A.Description
                                ,A.Sequence
                                ,dg.UserName DesignationGroup
                                ,sr.SalaryRuleName
                                ,A.Remarks
                                ,A.Active
                                FROM [HKP].[Designation] A
                left outer JOIN (SELECT * FROM [MST].[DesignationMaster] WHERE CompanyGroupId = '" + GroupId + @"')B ON A.Id = B.DesignationId
                left outer JOIN (SELECT * FROM [MST].[CompanyDesignation]
                                    WHERE CompanyId = '" + CompanyId + @"')c ON B.Id = c.DesignationMasterId
                left outer join (select * from org.PlantDesignationGroupSalaryRule where plantid='" + plantid + @"') r on r.DesignationGroupId=b.DesignationGroupId
				left outer join SalaryRuleMaster sr on sr.SystemID=r.SalaryRuleMasterId
                left outer join hkp.DesignationGroup dg on dg.Id=b.DesignationGroupId
                                ) x";

                if (strKey.Trim() != "")
                {
                    strSQL = strSQL + " WHERE " + strKey + "";
                }

                strSQL = strSQL + " Order By DesignationGroup,Designation";

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

        public void GetLegalDesignation(string CompanyId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT A.* FROM [HKP].[Designation] A
                //                     INNER JOIN (SELECT * FROM [MST].[DesignationMaster]
                //                                        WHERE CompanyGroupId = '" + sGroupID + @"')B ON A.Id = B.DesignationId
                //                    ORDER BY StandardName";
                strSQL = @"SELECT A.Id AS [Value], A.UserName AS [Text] FROM [HKP].[LegalDesignation] A
                            left outer JOIN (SELECT * FROM [MST].[DesignationMaster])B ON A.Id = B.DesignationId
                            left outer JOIN (SELECT * FROM [MST].[DesignationMasterLegalDesignation]) DL ON
                            A.Id=DL.LegalDesignationId
                            where DL.DesignationMasterId in
                            (
                                SELECT DesignationMasterId FROM [MST].[CompanyDesignation] WHERE CompanyId = '" + CompanyId + @"'
                            )";
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

        public void GetDesignationGroupSalaryRule(string DesignationId, string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select dgs.PlantId,dgs.SalaryRuleMasterId,dgs.DesignationGroupId,sm.SalaryRuleName SalaryRule
                                    ,dm.UserName DesignationMaster,dg.UserName DesignationGroup,d.UserName Designation,dm.DesignationId
                                    from org.PlantDesignationGroupSalaryRule dgs
                                    left outer join [MST].[DesignationMaster] dm on dgs.DesignationGroupId =dm.DesignationGroupId
                                    left outer join hkp.DesignationGroup dg on dgs.DesignationGroupId = dg.Id
                                    left outer join hkp.Designation d on d.Id=dm.DesignationId
                                    left outer join SalaryRuleMaster sm on sm.SystemID=dgs.SalaryRuleMasterId
                                    where dm.DesignationId='" + DesignationId + @"' and dgs.PlantId='" + plantid + "'";
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

        public void GetFixShiftName(string sGroupID, string sPlantID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SystemID,GroupID,PlantID,ShiftDefinationName,ShiftDefinationDescription,ShiftType
						 ,CONVERT(varchar(15),CAST(Intime AS TIME),100) InTime
					     ,CONVERT(varchar(15),CAST(OutTime AS TIME),100) OutTime
						 FROM  dbo.ShiftDefination  WHERE GroupID ='" + sGroupID + @"' AND PlantID='" + sPlantID + @"' Order By ShiftDefinationName";
                if (strName != "")
                {
                    strSQL = strSQL + " AND ShiftDefinationName = '" + strName + "'";
                }

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
        }//End Function---

        public void GetShiftNameBySectionUser(string sGroupID, string sPlantID, string strName, string userId, string sysAdmin, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sysAdmin == "False")
                {
                    strSQL = @"SELECT SystemID,GroupID,PlantID,ShiftDefinationName,ShiftDefinationDescription,ShiftType
						     ,CONVERT(varchar(15),CAST(Intime AS TIME),100) InTime
					         ,CONVERT(varchar(15),CAST(OutTime AS TIME),100) OutTime
						     FROM  dbo.ShiftDefination  WHERE GroupID ='" + sGroupID + @"' AND PlantID='" + sPlantID + @"' 
                             AND SystemID IN (SELECT ShiftId from dbo.UserSectionShift WHERE PlantID='" + sPlantID + @"' AND UserId='" + userId + @"')
                             ";
                }
                else
                {
                    strSQL = @" SELECT SystemID,GroupID,PlantID,ShiftDefinationName,ShiftDefinationDescription,ShiftType
						        ,CONVERT(varchar(15),CAST(Intime AS TIME),100) InTime
					            ,CONVERT(varchar(15),CAST(OutTime AS TIME),100) OutTime
						        FROM  dbo.ShiftDefination  WHERE GroupID ='" + sGroupID + @"' AND PlantID='" + sPlantID + @"'";
                }
                if (strName != "")
                {
                    strSQL = strSQL + " AND ShiftDefinationName = '" + strName + "'";
                }
                strSQL += " Order By ShiftDefinationName";
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
        }//End Function---

        public void GetUserFixShiftName(string sPlantID, string sGroupID, string strName, string userId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //         strSQL = @"SELECT IsSelected=  Convert(bit, 'False'), SystemID ShiftId,GroupID,PlantID,ShiftDefinationName,ShiftDefinationDescription,ShiftType
                //,CONVERT(varchar(15),CAST(Intime AS TIME),100) InTime
                //   ,CONVERT(varchar(15),CAST(OutTime AS TIME),100) OutTime
                //FROM  dbo.ShiftDefination  WHERE GroupID ='" + sGroupID + @"' AND PlantID='" + sPlantID + @"' Order By ShiftDefinationName";
                strSQL = @" Select 
						 SD.SystemID ShiftId,SD.GroupID,SD.PlantID,SD.ShiftDefinationName,SD.ShiftDefinationDescription,SD.ShiftType
						 ,CONVERT(varchar(15),CAST(SD.Intime AS TIME),100) InTime
					     ,CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100) OutTime,
						 IsSelected= case when USS.Id Is null then Convert(bit, 'False')
                         ELSE Convert(bit, 'True') END
						 FROM (Select * from dbo.ShiftDefination WHERE GroupID ='" + sGroupID + @"' AND PlantID='" + sPlantID + @"' )AS SD 
						 Left Join(Select * from  [dbo].[UserSectionShift] Where PlantId='" + sPlantID + @"' AND UserId='" + userId + @"') AS USS ON USS.ShiftId=SD.SystemID
						 Order By SD.ShiftDefinationName";
                if (strName != "")
                {
                    strSQL = strSQL + " AND ShiftDefinationName = '" + strName + "'";
                }

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
        }//End Function---

        public void GetEmployeeJobLocation(string empSystemId, string effectiveDate)
        {
            DataSet dsRef = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT JobLcSystemID FROM EmpDateWiseJobLocation WHERE EmpSystemID='" + empSystemId + "' AND EffectiveDate<='" + effectiveDate + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("This Employee has no job location before Effective date :'" + effectiveDate + "'.");
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
        }//End Function---

        public void GetGroupFixShiftName(string sGroupID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT S.SystemID, (S.ShiftDefinationName + ' - ' + C.ShortName)  ShiftDefinationName
                //            FROM dbo.ShiftDefination S
                //       LEFT JOIN Plant P ON S.PlantID = P.PlantID
                //       LEFT JOIN PlantAndCompanyAssignment PC ON P.PlantID = PC.PlantID
                //       LEFT JOIN Company C ON PC.CompanyID = C.CompanyID
                //            WHERE S.GroupID = '" + sGroupID + @"'
                //            ORDER BY C.ShortName, S.ShiftDefinationName";
                strSQL = @"SELECT S.SystemID, (S.ShiftDefinationName + ' - ' + C.ShortName)  ShiftDefinationName
                            FROM dbo.ShiftDefination S
			                    LEFT JOIN org.Plant P ON S.PlantID = P.ID
			                    LEFT JOIN org.Company C ON P.CompanyID = C.ID
                            WHERE S.GroupID = '" + sGroupID + @"'
                            ORDER BY C.ShortName, S.ShiftDefinationName";

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

        public void GetGroupFixShiftName(string sGroupID, string Plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT S.SystemID, (S.ShiftDefinationName + ' - ' + C.ShortName)  ShiftDefinationName
                //            FROM dbo.ShiftDefination S
                //       LEFT JOIN Plant P ON S.PlantID = P.PlantID
                //       LEFT JOIN PlantAndCompanyAssignment PC ON P.PlantID = PC.PlantID
                //       LEFT JOIN Company C ON PC.CompanyID = C.CompanyID
                //            WHERE S.GroupID = '" + sGroupID + @"'
                //            ORDER BY C.ShortName, S.ShiftDefinationName";
                strSQL = @"SELECT S.SystemID, (S.ShiftDefinationName + ' - ' + C.ShortName)  ShiftDefinationName
                            FROM dbo.ShiftDefination S
			                    LEFT JOIN org.Plant P ON S.PlantID = P.ID
			                    LEFT JOIN org.Company C ON P.CompanyID = C.ID
                            WHERE S.GroupID = '" + sGroupID + @"' and s.plantid='" + Plantid + @"'
                            ORDER BY C.ShortName, S.ShiftDefinationName";

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

        public void GetRosterShiftName(string sGroupID, string sPlantID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM ShiftRosterMaster WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";

                if (strName != "")
                {
                    strSQL = strSQL + " AND ShiftRosterName = '" + strName + "'";
                }

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

        public void GetRosterWiseShiftName(string sGroupID, string sPlantID, string SRMasterSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT SRC.SystemID AS SRChildSystemID, SRC.SRMasterSystemID, SRC.ShiftDefinationID,
                //                    SD.ShiftDefinationName, SRC.ShiftDays, SRC.WeekOffDayInRoster
                //            FROM ShiftRosterChild SRC
                //                     LEFT JOIN ShiftDefination SD ON SRC.ShiftDefinationID = SD.SystemID
                //                                                        AND SD.GroupID = '" + sGroupID + @"'
                //                                                                AND SD.PlantID = '" + sPlantID + @"'
                //            WHERE SRC.GroupID = '" + sGroupID + @"' AND SRC.PlantID = '" + sPlantID + @"'
                //                  AND SRC.SRMasterSystemID = '" + SRMasterSystemID + @"'
                //            ORDER BY SRC.ShiftSequence";
                strSQL = @"SELECT SRC.SystemID AS SRChildSystemID, SRM.SystemID SRMasterSystemID, SRM.ShiftRosterName, SRM.ShiftRosterDescription, SRC.ShiftDefinationID,
	                              SD.ShiftDefinationName, SRC.ShiftSequence, ISNULL(SRM.IsFixedDayInMonthShiftRoster, 0) IsFixedDayInMonthShiftRoster, SRM.FixedDayInMonthShiftRoster,
                                   ISNULL(SRM.IsDaysLengthShiftRoster, 0) IsDaysLengthShiftRoster, ISNULL(SRM.DaysLengthShiftRoster, 0) DaysLengthShiftRoster,
	                               ISNULL(SRM.IsAlignWithCC, 0) IsAlignWithCC, ISNULL(SRM.IsFixedDayInMonthWeekOff, 0) IsFixedDayInMonthWeekOff,
                                   SRM.FixedDayInMonthWeekOff, ISNULL(SRM.IsDaysLengthWeekOff, 0) IsDaysLengthWeekOff, SRM.WeekOffDay,
	                               ISNULL(SRM.IsWeekOffInShiftLenght, 0) IsWeekOffInShiftLenght, SRM.WeekOffInShiftLenght
                            FROM [dbo].[ShiftRosterMaster] SRM
			                            LEFT JOIN [dbo].[ShiftRosterChild] SRC ON SRM.SystemID = SRC.SRMasterSystemID
	                                    LEFT JOIN ShiftDefination SD ON SRC.ShiftDefinationID = SD.SystemID
                                                                        AND SD.GroupID = '" + sGroupID + @"'
                                                                                AND SD.PlantID = '" + sPlantID + @"'
                            WHERE SRC.GroupID = '" + sGroupID + @"' AND SRC.PlantID = '" + sPlantID + @"'
                                  AND SRM.SystemID = '" + SRMasterSystemID + @"'
                            ORDER BY SRC.ShiftSequence	";

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

        public void GetGroupRosterShiftName(string sGroupID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM ShiftRosterMaster
                                WHERE GroupID = '" + sGroupID + @"'
                                ORDER BY ShiftRosterName";

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

        public void GetGroupRosterShiftName(string sGroupID, string Plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM ShiftRosterMaster
                                WHERE GroupID = '" + sGroupID + @"' and plantid='" + Plantid + @"'
                                ORDER BY ShiftRosterName";

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

        public void GetGroupRosterWiseShiftName(string sGroupID, string SRMasterSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SRC.SystemID AS SRChildSystemID, SRC.SRMasterSystemID, SRC.ShiftDefinationID,
                                    SD.ShiftDefinationName, SRC.ShiftDays, SRC.WeekOffDayInRoster
                            FROM ShiftRosterChild SRC
	                                    LEFT JOIN ShiftDefination SD ON SRC.ShiftDefinationID = SD.SystemID
                                                                        AND SD.GroupID = '" + sGroupID + @"'
                            WHERE SRC.GroupID = '" + sGroupID + @"'
                                                        AND SRC.SRMasterSystemID = '" + SRMasterSystemID + @"'
                            ORDER BY SRC.ShiftSequence";

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

        public void GetLeavePolicy(string sGroupID, string sPlantID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM LeavePolicyMaster WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";

                if (strName != "")
                {
                    strSQL = strSQL + " AND PolicyName = '" + strName + "'";
                }

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

        public void GetDefaultLeavePolicy(string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM LeavePolicyMaster WHERE GroupID = '" + sGroupID + @"'
                            AND PlantID = '" + sPlantID + @"' AND DefaultPolicy = 1";

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

        public void GetDefaultFixShiftName(string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM ShiftDefination WHERE GroupID = '" + sGroupID + @"'
                            AND PlantID = '" + sPlantID + @"' AND DefaultShift = 1";

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

        public void GetDefaultJobLocation(string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT TOP (1) * FROM JobLocation WHERE PlantID = '" + sPlantID + @"'";

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

        public void GetDefaultPlantWiseHRMSSetting(string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM PlantWiseHRMSSetting WHERE GroupID = '" + sGroupID + @"'
                            AND PlantID = '" + sPlantID + @"'";

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

        public void GetSubSectionStructureSysID(string strGroupID, string strPlantID, string sSubSecStrucCombID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT * FROM
                                (
                                    SELECT *, (ISNULL(UnitID, '') + ISNULL(DivisionID, '') + ISNULL(DepartmentID, '') + ISNULL(SectionID, '') + ISNULL(SubSectionID, '') + ISNULL(LineID, '')) SubSecStrKey FROM
                                        (
                                            SELECT M.Id SystemID, M.Description, M.Code UserDefineCode, M.ProcessID, M.UnitID, C.DivisionID, C.DepartmentID, C.SectionID, C.SubSectionID, C.LineID
                                            FROM TRN.SubSectionStructureMaster M

                                            LEFT JOIN  TRN.SubSectionStructureDetail  C ON M.Id = C.SubsectionStructureMasterId
                                            WHERE M.CompanyGroupID = '" + strGroupID + @"' AND M.PlantID = '" + strPlantID + @"'
                                        ) A
                                ) B WHERE SubSecStrKey = '" + sSubSecStrucCombID + "'";

                //strSQL = @"SELECT * FROM
                //                (
                //                 SELECT *, (ISNULL(UnitID, '')+ISNULL(DivisionID, '')+ISNULL(DepartmentID, '')+ISNULL(SectionID, '')+ISNULL(SubSectionID, '')+ISNULL(LineID, '')) SubSecStrKey FROM
                //                        (
                //                         SELECT M.SystemID, M.Description, M.UserDefineCode, M.ProcessID, M.UnitID, C.DivisionID, C.DepartmentID, C.SectionID, C.SubSectionID, C.LineID
                //                            FROM TRN.SubSectionStructureMaster M
                //    LEFT JOIN (
                //                                                   SELECT * FROM TRN.SubSectionStructureDetail
                //       WHERE GroupID = '" + strGroupID + @"' AND PlantID = '" + strPlantID + @"'
                //         ) C ON M.SystemID = C.SubSecStrucSystemID
                //                            WHERE M.GroupID = '" + strGroupID + @"' AND M.PlantID = '" + strPlantID + @"'
                //                        ) A
                //                 ) B WHERE SubSecStrKey = '" + sSubSecStrucCombID + "'";

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

        public void savedJDList(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" Select EJD.Id, JDC.UserName  AS JobDescriptionCategory,
                               JDSC.UserName AS JobDescriptionSubCategory,
                               JDI.UserName  AS JobDescriptionItem,
                        	   EJD.JobDescriptionId
                        From   [TRN].[EmployeeJobDescription] EJD
                               Left Outer Join [HKP].[JobDescription] JD
                                            ON JD.Id = EJD.JobDescriptionId
                               Left Outer Join [HKP].[JobDescriptionCategory] JDC
                                            ON JDC.Id = JD.JobDescriptionCategoryId
                               Left Outer Join [HKP].[JobDescriptionSubCategory] JDSC
                                            ON JDSC.Id = JD.JobDescriptionSubCategoryId
                               Left Outer Join [HKP].[JobDescriptionItem] JDI
                                            ON JDI.Id = JD.JobDescriptionItemId
					   Where EJD.EmployeeId='" + EmployeeId + "'";
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
        }

        public void defaultJDList(string PositionManpowerBudgetId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" Select ''Id,JDC.UserName  AS JobDescriptionCategory,
                               JDSC.UserName AS JobDescriptionSubCategory,
                               JDI.UserName  AS JobDescriptionItem,
							   PMBJD.JobDescriptionId
                        From   [MST].[ManpowerBudgetJobDescription] PMBJD
                               Left Outer Join [HKP].[JobDescription] JD
                                            ON JD.Id = PMBJD.JobDescriptionId
                               Left Outer Join [HKP].[JobDescriptionCategory] JDC
                                            ON JDC.Id = JD.JobDescriptionCategoryId
                               Left Outer Join [HKP].[JobDescriptionSubCategory] JDSC
                                            ON JDSC.Id = JD.JobDescriptionSubCategoryId
                               Left Outer Join [HKP].[JobDescriptionItem] JDI
                                            ON JDI.Id = JD.JobDescriptionItemId
               Where PMBJD.ManpowerBudgetId = '" + PositionManpowerBudgetId + "'";
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
        }

        public void GetEmpAcademicQualificationInformation(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.EmpAcademicQualificationInformation
	                        WHERE EmpSystemID = '" + sEmpSysID + @"'";

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

        public void GetEmpTrainingInformation(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.EmpTrainingInformation
	                        WHERE EmpSystemID = '" + sEmpSysID + @"'";

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

        public void GetEmpDependantInformation(string sEmpSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT D.Id,D.EmpSystemId,D.Name,D.LocalName,D.ProfessionId,D.RelationId,R.UserName Relation,P.UserName Profession 
                          ,REPLACE(Convert(varchar(11), D.DOB, 106),' ','-') AS DOB,D.Remarks,D.AddedBy,D.AddedDate,D.UpdatedBy,D.UpdatedDate
                          FROM dbo.EmployeeDependantInfo D
                          LEFT JOIN SCS.Relationship R ON R.Id=D.RelationId 
                          LEFT JOIN SCS.Profession P ON P.Id=D.ProfessionId WHERE EmpSystemID = '" + sEmpSysID + @"'";

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
        public void GetEmpDependantInfo(string sEmpSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.EmployeeDependantInfo WHERE EmpSystemID = '" + sEmpSysID + @"'";

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

        public void GetEmpNomineeInformation(string sEmpSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.EmployeeNomineeInfo WHERE EmpSystemID = '" + sEmpSysID + @"'";

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

        public void GetEmpLandInformation(string sEmpSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.EmployeeLandLordInfo WHERE EmpSystemID = '" + sEmpSysID + @"'";

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
        public void SaveFingerPrint(string sEmpSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.EmployeeFingerPrint WHERE EmpSystemID = '" + sEmpSysID + @"'";

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
        }

        public void GetEmpReportingPerson(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.EmpReportingPerson
	                        WHERE EmpSystemID = '" + sEmpSysID + @"'";

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

        public void GetPositionCode(string strSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM PositionCode WHERE SystemID = '" + strSystemID + "'";

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

        public void GetEmployeeByWorkGroup(string sCompanyGroupId, string sCompanyId, string sPlantId, string sWorkGroupId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT [CheckBoxSelect] = Convert(bit, 'False'),  E.SystemId,E.EmployeeName, E.EmployeeCode  ,E.GivenDesignationId,D.UserName GivenDesignation,E.BudgetCode,
                           Replace(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
                           FROM EmployeeInformation E
                           LEFT JOIN HKP.Designation D ON D.Id=E.GivenDesignationId
                           LEFT JOIN MST.ManpowerBudget MB ON MB.Id=E.BudgetCode
                           Where MB.WorkGroupId='" + sWorkGroupId + @"' AND MB.CompanyGroupId='" + sCompanyGroupId + @"' AND MB.CompanyId='" + sCompanyId + @"' AND E.PlantId='" + sPlantId + @"' AND E.EmployeeStatus='Active'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objCon = null;
            }
        }

        public void GetEmployeejobLocationByDate(string empSystemId, string workDate, out DataSet dsRef)
        {
            string str = "";
            if (!string.IsNullOrEmpty(workDate))
            {
                str = "AND EffectiveDate<='" + workDate + "'";
            }
            else
            {
                str = "";
            }
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT JobLcSystemID,EmpSystemID,Max(EffectiveDate) EffectiveDate from EmpDateWiseJobLocation 
						 Where  EmpSystemID='" + empSystemId + @"' " + str + @"
						 Group By JobLcSystemID,EmpSystemID";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objCon = null;
            }
        }
        public void GetEmpSystemId(string EmpCode, string plantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = " Select SystemID from  EmployeeInformation where EmployeeCode='" + EmpCode + "' and plantId='" + plantId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void GetSysIdWiseEmpBasicInfoInformation(string sGroupID, string sCompanyID, string sPlantID, string strEmpSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT * FROM
                                        (SELECT E.SystemID, E.EmployeeId,E.EmployeeCode,E.EmployeeCode EmpCode , E.CardNumber, E.Salutation, E.FirstName, E.MiddleName, E.LastName,
					                            E.EmployeeName, E.EmployeeNameLocal, E.NickName, E.EmpPicPath, E.EmpType, E.EmploymentType, '' UserGroupSystemID,
					                            '' CtlPrlGroupName, E.GroupID, GC.StandardName GroupName, E.CompanyID, CMP.StandardName CompanyName, E.PlantID, Pt.StandardName PlantName,
					                            REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS DOB, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ,
					                            E.DOCIsDay, E.DOCDay, E.DOCIsMonth, E.DOCMonth, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC,
					                            REPLACE(Convert(varchar(11), E.DOS, 106),' ','-') AS DOS, REPLACE(Convert(varchar(11), E.ReActiveDate, 106),' ','-') AS ReActiveDate,
					                            E.EmployeeStatus, E.NationalID, E.CitizenID,E.TIN, Citi.StandardName CitizenName, E.FatherName,E.FatherNameLocal, E.MotherName,E.MotherNameLocal,
					                            E.ReligionID, Rg.StandardName ReligionName, E.CivilStatusID, CS.StandardName CivilStatusName, E.BloodGroupID, BG.StandardName BloodGroupName,
					                            E.GenderID, E.GenderID GenderName, E.SpouseName, E.SpouseNationalID, E.SpouseOccupation, E.NoOfChildren, E.PresentAddress1, E.PresentAddress2,
					                            E.ParmanentAddress1, E.ParmanentAddress2, E.PresThanaID, PresT.StandardName PresThanaName, E.ParmThanaID, ParmT.StandardName ParmThanaName,
					                            E.PresPostOfficeID, PresPO.StandardName PresPostOfficeName, E.ParmPostOfficeID, ParmPO.StandardName ParmPostOfficeName,
					                            E.PresZipCode, E.ParmZipCode, E.PresDistrictID, PresD.StandardName PresDistrictName, E.ParmDistrictID,
					                            ParmD.StandardName ParmDistrictName, E.PresCountryID, PresC.StandardName PresCountry, E.ParmCountryID, ParmC.StandardName ParmCountry,
					                            E.TelePhnNo, E.CellPhnNo, E.EmailID, E.UnitID, U.StandardName UnitName,
					                            E.DivisionID, Dv.StandardName DivisionName, E.DepartmentID, De.StandardName DepartmentName, Se.StandardName SectionName, 
					                            SuS.StandardName SubSectionName, Ln.StandardName LineName, E.BudgetCategoryID, EBC.StandardName BudgetCategoryName,
                                                E.SubSecStrucSystemID, SSSM.Description SubSectionStructureDes, SSSM.Code SubSectionStructureCode,
					                            EC.UserName EmpCategoryName, DG.StandardName DesignationGroupName, Dsg.StandardName DesignationName,
					                            E.LVPolicyMasterSystemID, DGM.LeavePolicyMasterId, LPM.PolicyName LeavePolicyName, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName,
					                            E.BankSystemID, E.BankName, E.BankAccNo, E.RegisterFP, E.RegisterProximate,  E.IsSlvDevReg, R.EmployeeStatue ResignStatue,
                                                E.EmployeeGroupSystemID, E.JobLocationID, ISNULL(E.IsConfirmed,0)IsConfirmed, JbLc.JobLocation, SRM.CurrencyRuleSystemID, SID.SlrRulMstSystemID IncrementSlrRulMstSystemID,
                                                REPLACE(Convert(varchar(11), SID.EffectiveDate, 106),' ','-') AS IncrementEffectiveDate,EmrCntPer2CellNo
												,EmrCntPer2Name,EmrCntPer1CellNo2,EmrCntPer1CellNo3,EmrCntPer2CellNo2,EmrCntPer2CellNo3
												,EmrCntPer1CellNo,pmb.Code BudgetCodeName, E.SalaryPercentage
												,EmrCntPer1Name,E.BudgetCode,E.SubdivisionID,E.IsDirect,E.PositionId,PC.UserName PositionName
                                                ,E.PresCityID, E.ParmCityID, E.PresAreaID, E.ParmAreaID
		                                        ,PresCT.UserName PresCity, ParmCT.UserName ParmCity, PresAR.UserName PresArea, ParmAR.UserName ParmaArea
                                                ,E.GivenDesignationId,E.LegalDesignationId,Dsgg.UserName GivenDesignation
                                                , tge.TaxGroupID TaxGrpEmpSystemID,SD.UserName Subdivision
                                                , DGM.DesignationGroupId GivenDesignationGroupId, DGM.UserName GivenDesignationGroup
                                                ,EG.UserName EmployeeGroupName
                                                ,tgr.TaxGroupName, dgSRM.TaxGroupID TaxGroupIDSR
                                                ,REPLACE(Convert(varchar(11),E.BirthdayCelebrationDate, 106),' ','-') AS BirthdayCelebrationDate
                                                ,REPLACE(Convert(varchar(11),E.MarriagedayCelebrationDate, 106),' ','-') AS MarriagedayCelebrationDate
												,E.PresStateId,E.ParmStateId,E.PresentArea,E.ParmanentArea,E.Height,E.Weight,E.IdentificationMark,E.LocalIdentificationMark,
												E.PreviouslyWorkedHere,E.PreviousEmployeeCode,E.PreviousDesignation,E.PreviousSalary,E.PreviousServicePeriod,E.ExitReason,
												E.AnyRelativeWorkedHere,E.RelativeSystemId,E.PresentAddress1Local,E.PresentAddress2Local,E.ParmanentAddress1Local,E.ParmanentAddress2Local
												,E.RelationShip,E.RelativeCellNo,E.ExitReasonLocal,E.SpouseNameLocal,E.PreviousDesignationLocal,E.EmpSignature,E.PaymentMode,LDSg.UserName LegalDesignation
				                            FROM EmployeeInformation AS E
						                            LEFT OUTER JOIN
									                            [HKP].[EmployeeBudgetCategory] EBC ON E.BudgetCategoryID = EBC.ID
						                            LEFT OUTER JOIN
									                            [ORG].[CompanyGroup] GC ON E.GroupID = GC.ID
						                            LEFT OUTER JOIN
									                            [ORG].[Company] CMP ON E.CompanyID = CMP.ID
						                            LEFT OUTER JOIN
									                            [ORG].Plant Pt ON E.PlantID = Pt.ID
						                            LEFT OUTER JOIN
									                            [SCS].[Country] Citi ON E.CitizenID = Citi.ID
						                            LEFT OUTER JOIN
									                            [SCS].Religion Rg ON E.ReligionID = Rg.ID
						                            LEFT OUTER JOIN
									                            HKP.CivilStatus AS CS ON E.CivilStatusID  = CS.ID
						                            LEFT OUTER JOIN
									                            [HKP].[BloodGroup] AS BG ON E.BloodGroupID  = BG.ID
						                            LEFT OUTER JOIN
                                                                [MST].[ManpowerBudget] pmb on e.BudgetCode=pmb.Id
                                                    LEFT OUTER JOIN
																[ORG].[Position] AS PC ON pmb.PositionID  = PC.ID
                                                    LEFT OUTER JOIN ORG.Entity EN ON pmb.EntityId=EN.Id
						                            LEFT OUTER JOIN
									                            [SCS].[PoliceStation] AS PresT ON E.PresThanaID  = PresT.ID
						                            LEFT OUTER JOIN
									                            [SCS].[PoliceStation] AS ParmT ON E.ParmThanaID  = ParmT.ID
						                            LEFT OUTER JOIN
									                            [SCS].[PostOffice] AS PresPO ON E.PresPostOfficeID  = PresPO.ID
						                            LEFT OUTER JOIN
									                            [SCS].[PostOffice] AS ParmPO ON E.ParmPostOfficeID  = ParmPO.ID
						                            LEFT OUTER JOIN
									                            [SCS].[District] AS PresD ON E.PresDistrictID  = PresD.ID
						                            LEFT OUTER JOIN
									                            [SCS].[District] AS ParmD ON E.ParmDistrictID  = ParmD.ID
						                            LEFT OUTER JOIN
									                            [SCS].Country AS PresC ON E.PresCountryID  = PresC.ID
						                            LEFT OUTER JOIN
									                            [SCS].Country AS ParmC ON E.ParmCountryID  = ParmC.ID
                                                    LEFT OUTER JOIN
                                                                [SCS].[City] PresCT ON E.PresCityID = PresCT.Id
			                            			LEFT OUTER JOIN
                                                                [SCS].[City] ParmCT ON E.ParmCityID = ParmCT.Id
			                                        LEFT OUTER JOIN
                                                                [SCS].[Area] PresAR ON E.PresAreaID = PresAR.Id
			                                        LEFT OUTER JOIN
                                                                [SCS].[Area] ParmAR ON E.ParmAreaID = ParmAR.Id
                                                    LEFT OUTER JOIN	[HKP].LegalDesignation AS LDSg ON LDSg.ID = E.LegalDesignationId
						                            LEFT OUTER JOIN
																--[HKP].[EmployeeCategory] AS EC ON E.EmployeeCategorySystemID = EC.ID
																(
                                                                SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
																LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
																)EC ON EC.DesignationId=E.GivenDesignationId
													LEFT OUTER JOIN
																[ORG].[Unit] AS U ON U.ID = EN.UnitID
													LEFT OUTER JOIN
																[ORG].Division AS Dv ON Dv.ID = PC.DivisionID
													LEFT OUTER JOIN
																[ORG].Department AS De ON De.ID = PC.DepartmentID
													LEFT OUTER JOIN
																[HKP].Designation AS Dsg ON Dsg.ID = PC.DesignationID
                                                    LEFT OUTER JOIN
																[HKP].Designation AS Dsgg ON Dsgg.ID = E.GivenDesignationID
													LEFT OUTER JOIN
																[ORG].Section AS Se ON Se.ID = PC.SectionID
													LEFT OUTER JOIN
																[ORG].SubSection AS SuS ON SuS.ID = PC.SubSectionID
									                LEFT OUTER JOIN
									                            [ORG].Line AS Ln ON Ln.ID = pmb.LineID
                                                    LEFT OUTER JOIN
									                            [ORG].SubDivision AS SD ON SD.Id = PC.SubdivisionID
									                LEFT OUTER JOIN
									                            [TRN].[SubsectionStructureMaster] AS SSSM ON SSSM.ID = E.SubSecStrucSystemID
						                            LEFT JOIN MST.DesignationMaster DEM ON DEM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DEM.DesignationGroupID
                                                    LEFT OUTER JOIN
									                            (               SELECT DC.SalaryRuleMasterId,dc.PlantId,dm.*,dc.LeavePolicyMasterId
                                                                                FROM MST.DesignationMaster DM
							                                                    LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                                                            ON DM.Id=DC.DesignationMasterId
                                                    ) DGM ON E.GivenDesignationID = DGM.DesignationId and E.PlantId=DGM.PlantId
						                            LEFT OUTER JOIN
									                            SalaryRuleMaster dgSRM ON DGM.SalaryRuleMasterId = dgSRM.SystemID
                                                    LEFT OUTER JOIN
																[dbo].[TaxGroupTagWithEmployee] tge on tge.EmpInfoSystemID=E.SystemId
						                            LEFT OUTER JOIN
									                            LeavePolicyMaster LPM ON DGM.LeavePolicyMasterId = LPM.SystemID
						                            LEFT OUTER JOIN
									                            SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                                    LEFT OUTER JOIN
									                            TaxGroup tgr ON SRM.TaxGroupID = tgr.SystemID
                                                    LEFT OUTER JOIN
									                            JobLocation JbLc ON E.JobLocationID = JbLc.SystemID
                                                    LEFT OUTER JOIN
									                           [HKP].[EmployeeGroup]  EG ON E.EmployeeGroupSystemID= EG.Id
						                            LEFT OUTER JOIN
									                            (
                                                                 SELECT TOP (1) * FROM SalaryIncrementInfoDefineMaster WHERE IsApproved = 0
                                                                ) SID ON E.SystemID = SID.EmpInfoSystemID
                                                    LEFT OUTER JOIN
									                            Resign R ON E.SystemID = R.EmpInfoSystemID AND IsEffected = 1
							         WHERE E.GroupID = '" + sGroupID + @"' AND E.CompanyID = '" + sCompanyID + @"'
                                              AND E.PlantID = '" + sPlantID + @"') A
                         WHERE SystemID = '" + strEmpSysID + @"'
                        Order By EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetInOutTime(string empSystemId, string fromDate, string toDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                //          strSql = @"SELECT * FROM (Select  REPLACE(CONVERT(VARCHAR(11), WorkDate, 106), ' ', '-') WorkDate,
                // CONVERT(varchar(15),CAST(A.Intime AS TIME),100) PTime
                // ,'IN' [Satus],SD.ShiftDefinationDescription,SD.ShiftDefinationName
                //  FROM AttdnProcessData AS A
                //  LEFT JOIN dbo.ShiftDefination AS SD ON SD.SystemID=A.ShiftSystemID
                //  Where EmpSystemId='" + empSystemId + "' And WorkDate between '"+ fromDate + @"' AND '"+ toDate + @"' 
                //  Union
                //  	 Select  REPLACE(CONVERT(VARCHAR(11), WorkDate, 106), ' ', '-') WorkDate,
                //    CONVERT(varchar(15),CAST(A.OutTime AS TIME),100) PTime
                //,'OUT' [Satus],SD.ShiftDefinationDescription,SD.ShiftDefinationName
                //  from AttdnProcessData AS A
                //  LEFT JOIN dbo.ShiftDefination AS SD ON SD.SystemID=A.ShiftSystemID
                //  Where EmpSystemId= '" + empSystemId+@"' And WorkDate between '"+ fromDate + @"' AND '"+ toDate + "' )A Order By WorkDate,CAST(PTime AS TIME)";
                strSql = @"
                            SELEct REPLACE(CONVERT(VARCHAR(11), A.PDate, 106), ' ', '-') PDate, A.PType [Satus], CONVERT(varchar(15),CAST(A.PTime AS TIME),100) PTime,SD.ShiftDefinationDescription,SD.ShiftDefinationName FROM AttdnRawData A
                            LEFT JOIN EmpDateWiseShiftAssign E ON E.EmpSystemID=A.LogDownLoadNum AND E.WorkDate=A.PDate
                            LEFT JOIN dbo.ShiftDefination AS SD ON SD.SystemID=E.ShiftSystemID
                            Where A.LogDownLoadNum='" + empSystemId + @"' AND A.PDate between '" + fromDate + @"' AND '" + toDate + @"' 
                        ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetShiftEffectiveDate(string empSystemId, string workDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT 
						  REPLACE(CONVERT(VARCHAR(11), ESA.EffectiveDate, 106), ' ', '-') EffectiveDate
						 ,SD.ShiftDefinationDescription,SD.ShiftDefinationName
						  FROM EmployeeShiftAssign ESA
						  LEFT JOIN dbo.ShiftDefination AS SD ON SD.SystemID=ISNULL(ESA.FixSystemID,'')+ISNULL(ESA.RosterStartShiftID,'')
						  WHERE ESA.EmpSystemId='" + empSystemId + "' AND ESA.EffectiveDate >='" + workDate + @"' 
						  ORDER BY ESA.EffectiveDate";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void LoadEmployeeInfo(CustomPara _para, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT * FROM
                                        (

                                            SELECT E.SystemID
	                                            , E.EmployeeCode
	                                            , E.EmployeeName
	                                            , E.GroupID
	                                            , GC.StandardName GroupName
	                                            , E.CompanyID
	                                            , CMP.StandardName CompanyName
	                                            , E.PlantID
	                                            , Pt.StandardName PlantName
	                                            , REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB
	                                            , REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ
	                                            , E.EmployeeStatus
	                                            , E.EmployeeCategorySystemID
	                                            , EC.StandardName EmpCategoryName
	                                            , DG.StandardName DesignationGroupName
	                                            , E.LVPolicyMasterSystemID
	                                            , DGM.LeavePolicyMasterId
	                                            , LPM.PolicyName LeavePolicyName
	                                            , E.SalaryRuleMasterSystemID
	                                            , SRM.SalaryRuleName
	                                            , R.EmployeeStatue ResignStatue
	                                            , E.EmployeeGroupSystemID
	                                            , E.JobLocationID
	                                            , ISNULL(E.IsConfirmed, 0) IsConfirmed
	                                            , JbLc.JobLocation
	                                            , SRM.CurrencyRuleSystemID
	                                            , pmb.Code BudgetCodeName
	                                            , E.SalaryPercentage
	                                            , E.BudgetCode
	                                            , E.GivenDesignationId
	                                            , E.LegalDesignationId
	                                            , Dsgg.UserName GivenDesignation
	                                            , tge.TaxGroupID TaxGrpEmpSystemID
	                                            , tgr.TaxGroupName
	                                            , dgSRM.TaxGroupID TaxGroupIDSR
	                                            , REPLACE(Convert(VARCHAR(11), SSA.EffectiveDate, 106), ' ', '-') AS EffectiveDate
                                            FROM EmployeeInformation AS E
						                            LEFT OUTER JOIN   [HKP].[EmployeeBudgetCategory] EBC ON E.BudgetCategoryID = EBC.ID
						                            LEFT OUTER JOIN   [ORG].[CompanyGroup] GC ON E.GroupID = GC.ID
						                            LEFT OUTER JOIN   [ORG].[Company] CMP ON E.CompanyID = CMP.ID
						                            LEFT OUTER JOIN   [ORG].Plant Pt ON E.PlantID = Pt.ID
						                            LEFT OUTER JOIN    [MST].[ManpowerBudget] pmb on e.BudgetCode=pmb.Id
						                            LEFT OUTER JOIN [HKP].[EmployeeCategory] AS EC ON E.EmployeeCategorySystemID = EC.ID
						                            LEFT JOIN MST.DesignationMaster DEM ON DEM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DEM.DesignationGroupID
                                                    LEFT OUTER JOIN  (               SELECT DC.SalaryRuleMasterId,dc.PlantId,dm.*,dc.LeavePolicyMasterId 
                                                                                FROM MST.DesignationMaster DM
							                                                    LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                                                            ON DM.Id=DC.DesignationMasterId
                                                    ) DGM ON E.GivenDesignationID = DGM.DesignationId AND E.PlantId=DGM.PlantId
													LEFT OUTER JOIN 	[HKP].Designation AS Dsgg ON Dsgg.ID = E.GivenDesignationID
						                            LEFT OUTER JOIN  SalaryRuleMaster dgSRM ON DGM.SalaryRuleMasterId = dgSRM.SystemID
                                                    LEFT OUTER JOIN 	[dbo].[TaxGroupTagWithEmployee] tge on tge.EmpInfoSystemID=E.SystemId
						                            LEFT OUTER JOIN    LeavePolicyMaster LPM ON DGM.LeavePolicyMasterId = LPM.SystemID
						                            LEFT OUTER JOIN    SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                                    LEFT OUTER JOIN  TaxGroup tgr ON SRM.TaxGroupID = tgr.SystemID
                                                    LEFT OUTER JOIN   JobLocation JbLc ON E.JobLocationID = JbLc.SystemID
													left outer join
													(
														select EmpInfoSystemID, max(EffectiveDate) EffectiveDate from
														(
														select EmpInfoSystemID,max(EffectiveDate) EffectiveDate from SalaryInfoBackMaster where IsApproved=1 group by EmpInfoSystemID
														union
														select EmpInfoSystemID,max(EffectiveDate) EffectiveDate from SalaryInfoDefineMaster where IsApproved=1 group by EmpInfoSystemID
														) x group by EmpInfoSystemID
													) SSA on SSA.EmpInfoSystemID=E.SystemId
                                                    LEFT OUTER JOIN
									                           [HKP].[EmployeeGroup]  EG ON E.EmployeeGroupSystemID= EG.Id
						                            LEFT OUTER JOIN
									                            (
                                                                 SELECT TOP (1) * FROM SalaryIncrementInfoDefineMaster WHERE IsApproved = 0
                                                                ) SID ON E.SystemID = SID.EmpInfoSystemID
                                                    LEFT OUTER JOIN
									                            Resign R ON E.SystemID = R.EmpInfoSystemID AND IsEffected = 1
							         WHERE E.GroupID = '" + _para.CompanyGroupId + @"'
									 AND E.CompanyID = '" + _para.CompanyId + @"'
                                              AND E.PlantID = '" + _para.PlantId + @"'
											  and E.Isapproved=1
											  and E.EmployeeStatus='Active'
											  --and isnull(E.SalaryRuleMasterSystemID,'')<>''
											  and isnull(dgSRM.TaxGroupID,'')<>''
											  --and E.SalaryRuleMasterSystemID='" + _para.SalaryRuleId + @"'
                                              and E.systemid in ('" + _para.EmployeeId + @"')
											  ) A
                         ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void GetEmpReportingPersonForGrd(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT RP.SystemID, RP.SrNo, E.SystemID EmpRptSystemID, E.EmployeeCode ReportCode, E.EmployeeName ReportName,
	                                E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID
			                        --, G.GenderName
			                        , REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS  DOJ, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC
	                                , U.UserName AS Unit
			                        , Dv.UserName AS Devision
			                        , De.UserName AS Department
			                        , Se.UserName AS Section
			                        , SuS.UserName SubSection
			                        , Dsg.UserName AS Designation
                            FROM [dbo].[EmpReportingPerson] RP
		                            INNER JOIN
					                            EmployeeInformation AS E ON RP.RptEmpSystemID  = E.SystemID
		                               LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    LEFT JOIN [MST].[DesignationMaster] d on d.DesignationId=e.GivenDesignationId
									LEFT JOIN HKP.EmployeeCategory EC on EC.Id=d.EmployeeCategoryId
		                            LEFT OUTER JOIN
					                            org.Unit AS U ON U.ID = EN.UnitID
		                            LEFT OUTER JOIN
					                            org.Division AS Dv ON Dv.ID = PO.DivisionID
		                            LEFT OUTER JOIN
					                            org.Department AS De ON De.ID = PO.DepartmentID
		                            LEFT OUTER JOIN
					                            hkp.Designation AS Dsg ON Dsg.ID = PO.DesignationID
		                            LEFT OUTER JOIN
					                            org.Section AS Se ON Se.ID = PO.SectionID
		                            LEFT OUTER JOIN
					                            org.SubSection AS SuS ON SuS.ID = PO.SubSectionID
                            WHERE RP.EmpSystemID = '" + sEmpSysID + @"'
                            ORDER BY RP.SrNo
	                        ";

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

        public void GetEmpAcademicQualificationInformationForGrd(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EA.SystemID
	                            ,EA.EductLevelSystemID
	                            ,Q.UserName EductLevel
	                            ,s.UserName Stream
	                            ,EA.TypeIsAcademic
	                            ,EA.IsEnglishMedium
	                            ,EA.HasDistinction
	                            ,EA.ExamDegreeType
	                            ,EA.StreamId
	                            ,EA.InstituteName
	                            ,c.UserName Country
	                            ,EA.CountryId
	                            ,EA.YearOfPass
	                            ,EA.[Session]
	                            ,EA.Achievement,EA.FileId
								,EA.FileName
                            FROM EmpAcademicQualificationInformation EA
                            LEFT JOIN [scs].QualificationLevel q ON EA.EductLevelSystemID = q.Id
                            LEFT JOIN [scs].QualificationStream s ON EA.StreamId = s.Id
                            LEFT JOIN [scs].Country c ON EA.CountryId = c.Id
                            WHERE EA.EmpSystemID = '" + sEmpSysID + @"'
                            ORDER BY EA.YearOfPass";
               

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

        public void GetSkillEdit(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                            select e.SystemID,e.Skillid,e.OperationCategoryid,e.OperationId,s.UserName Skill,c.UserName
                            OperationCategory,o.UserName Operation
                            from [dbo].[EmpSkillInformation] e
                            left outer join hkp.Skill s on e.skillid=s.Id
                            left outer join hkp.OperationCategory c on c.Id=e.operationcategoryid
                            left outer join mst.Operation o on o.Id=e.OperationId
                            where e.EmpSystemID='" + sEmpSysID + @"'
                            Order By s.UserName,c.UserName,o.UserName";

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

        public void GetExpEdit(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select e.SystemID,e.Employer,e.Designation
                             ,Replace(CONVERT(VARCHAR(11), e.StartDate, 106), ' ', '-') StartDate
                             ,Replace(CONVERT(VARCHAR(11), e.EndDate, 106), ' ', '-') EndDate
                             ,e.JobDescription,e.Achievement
                             ,e.DurationYear,e.DurationMonth
                             ,e.IsPartTime,e.DurationYear,e.DurationMonth , e.IsCurrentJob, e.FileId, e.FileName
                            from [dbo].[EmpExperienceInformation] e
                            where e.EmpSystemID='" + sEmpSysID + @"'
                            Order By e.StartDate desc";

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

        public void GetDocEdit(string sEmpSysID, string sGroupID, string sPlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT  DISTINCT ED.*,
									CD.UserName DocumentName
									,CD.DocumentType
									,CD.IsSkillBased
									,CDSD.OptionalOrMandatory
									,CD.EmpType
									,CD.ProfileType,CD.DocNumberRequired,CD.DocDateRequired
									,E.UserName AS EmployeeCategory
									,CD.DependateDate
								FROM dbo.EmployeeDocument ED
								LEFT JOIN hkp.ComplianceDocument CD ON ED.ComplianceDocumentId = CD.Id
								LEFT JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CD.Id = CDSD.ComplianceDocumentId
								LEFT JOIN (SELECT  * FROM HKP.DocumentConfigurationDesignationGroup
								WHERE PlantId='" + sPlantID + @"' and EmployeeCategoryId = (
										SELECT D.EmployeeCategoryId
										FROM (SELECT * FROM MST.DesignationMaster WHERE CompanyGroupId = '" + sGroupID + @"'
											) AS D
										LEFT JOIN EmployeeInformation EI ON D.DesignationId = EI.GivenDesignationId
										WHERE EI.SystemId = '" + sEmpSysID + @"'
										)
								)DD ON CDSD.ComplianceDocumentSetId = DD.ComplianceDocumentSetId
								LEFT JOIN HKP.EmployeeCategory AS E ON DD.EmployeeCategoryId = E.Id
								WHERE ED.EmpSystemID = '" + sEmpSysID + @"'
									AND ISNULL(CD.ProfileType,'') NOT IN ('Qualification','Training','Experience','Photo')
									AND E.UserName IS NOT NULL ORDER BY CDSD.OptionalOrMandatory,DocumentName";

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

        public void GetDocumentById(string id, out DataSet dsDoc)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeDocument Where Id='" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsDoc, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetQualificationById(string id, out DataSet dsDoc)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmpAcademicQualificationInformation Where SystemID='" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsDoc, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetTrainingById(string id, out DataSet dsDoc)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmpTrainingInformation Where SystemID='" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsDoc, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetExperienceById(string id, out DataSet dsDoc)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmpExperienceInformation Where SystemID='" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsDoc, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetEmpTrainingInformationForGrd(string sEmpSysID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT ETI.SystemID, ETI.TrainingTitle, ETI.TopicCovered, ETI.InstituteName, ETI.CountrySystemID,
	                              C.UserName Country, ETI.Location, ETI.TrainingYear, ETI.Duration, ETI.DurationUOM, ETI.FileId, ETI.FileName
                           FROM EmpTrainingInformation ETI
		                            LEFT JOIN [scs].[Country] C ON ETI.CountrySystemID = C.id
                           WHERE ETI.EmpSystemID = '" + sEmpSysID + @"'";

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

        public void GetEmpCodeWiseEmpBasicInfoInformation(string sGroupID, string sCompanyID, string sPlantID, string strEmpCode, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM
                                        (SELECT E.SystemID, E.EmployeeCode, E.CardNumber, E.Salutation, E.FirstName, E.MiddleName, E.LastName, E.IsConfirmed, E.Tin,
					                            E.EmployeeName, E.LocalEmployeeName, E.NickName, E.EmpPicPath, E.EmpType, E.EmploymentType, '' UserGroupSystemID,
					                            '' CtlPrlGroupName, E.GroupID, GC.StandardName GroupName, E.CompanyID, CMP.StandardName CompanyName, E.PlantID, Pt.StandardName PlantName,
					                            REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS DOB, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ,
					                            E.DOCIsDay, E.DOCDay, E.DOCIsMonth, E.DOCMonth, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC,
					                            REPLACE(Convert(varchar(11), E.DOS, 106),' ','-') AS DOS, REPLACE(Convert(varchar(11), E.ReActiveDate, 106),' ','-') AS ReActiveDate,
					                            E.EmployeeStatus, E.NationalID, E.CitizenID, Citi.StandardName CitizenName, E.FatherName, E.MotherName,
					                            E.ReligionID, Rg.StandardName ReligionName, E.CivilStatusID, CS.StandardName CivilStatusName, E.BloodGroupID, BG.StandardName BloodGroupName,
					                            E.GenderID, E.GenderID GenderName, E.SpouseName, E.SpouseNationalID, E.SpouseOccupation, E.NoOfChildren, E.PresentAddress1,
					                            E.ParmanentAddress1, E.PresThanaID, PresT.StandardName PresThanaName, E.ParmThanaID, ParmT.StandardName ParmThanaName,
					                            E.PresPostOfficeID, PresPO.StandardName PresPostOfficeName, E.ParmPostOfficeID, ParmPO.StandardName ParmPostOfficeName,
					                            E.PresZipCode, E.ParmZipCode, E.PresDistrictID, PresD.StandardName PresDistrictName, E.ParmDistrictID,
					                            ParmD.StandardName ParmDistrictName, E.PresCountryID, PresC.StandardName PresCountry, E.ParmCountryID, ParmC.StandardName ParmCountry,
					                            E.TelePhnNo, E.CellPhnNo, E.EmailID, U.StandardName UnitName,-- PC.StandardName PositionName,
					                            Dv.StandardName DivisionName, De.StandardName DepartmentName, Se.StandardName SectionName,
					                            SuS.StandardName SubSectionName, Ln.StandardName LineName, E.BudgetCategoryID, EBC.StandardName BudgetCategoryName,
                                                E.SubSecStrucSystemID, SSSM.Description SubSectionStructureDes, SSSM.Code SubSectionStructureCode,
					                            EC.StandardName EmpCategoryName, DG.StandardName DesignationGroupName, Dsg.StandardName DesignationName,
					                            E.LVPolicyMasterSystemID, LPM.PolicyName LeavePolicyName, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName,
					                            E.BankSystemID, E.BankName, E.BankAccNo, E.RegisterFP, E.RegisterProximate,  E.IsSlvDevReg, R.EmployeeStatue ResignStatue,
                                                E.EmployeeGroupSystemID, E.JobLocationID, JbLc.JobLocation, SRM.CurrencyRuleSystemID, SID.SlrRulMstSystemID IncrementSlrRulMstSystemID,
                                                REPLACE(Convert(varchar(11), SID.EffectiveDate, 106),' ','-') AS IncrementEffectiveDate, E.EmrCntPer1Name, E.EmrCntPer1CellNo,
                                                E.EmrCntPer2Name, E.EmrCntPer2CellNo,E.EmrCntPer1CellNo2,E.EmrCntPer1CellNo3, tge.TaxGroupID TaxGrpEmpSystemID
                                                ,E.EmrCntPer2CellNo2,E.EmrCntPer2CellNo3,tgr.TaxGroupName,SRM.TaxGroupID TaxGroupIDSR,Dsg.UserName GivenDesignation
				                            FROM EmployeeInformation AS E
LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
						                            LEFT OUTER JOIN
									                            [HKP].[EmployeeBudgetCategory] EBC ON E.GroupID = EBC.ID
						                            LEFT OUTER JOIN
									                            [ORG].[CompanyGroup] GC ON E.GroupID = GC.ID
						                            LEFT OUTER JOIN
									                            [ORG].[Company] CMP ON E.CompanyID = CMP.ID
						                            LEFT OUTER JOIN
									                            [ORG].Plant Pt ON E.PlantID = Pt.ID
						                            LEFT OUTER JOIN
									                            [SCS].[Country] Citi ON E.CitizenID = Citi.ID
						                            LEFT OUTER JOIN
									                            [SCS].Religion Rg ON E.ReligionID = Rg.ID
						                            LEFT OUTER JOIN
									                             [HKP].CivilStatus AS CS ON E.CivilStatusID  = CS.ID
						                            LEFT OUTER JOIN
									                            [HKP].[BloodGroup] AS BG ON E.BloodGroupID  = BG.ID
						                            LEFT OUTER JOIN
									                            [SCS].[PoliceStation] AS PresT ON E.PresThanaID  = PresT.ID
						                            LEFT OUTER JOIN
									                            [SCS].[PoliceStation] AS ParmT ON E.ParmThanaID  = ParmT.ID
						                            LEFT OUTER JOIN
									                            [SCS].[PostOffice] AS PresPO ON E.PresPostOfficeID  = PresPO.ID
						                            LEFT OUTER JOIN
									                            [SCS].[PostOffice] AS ParmPO ON E.ParmPostOfficeID  = ParmPO.ID
						                            LEFT OUTER JOIN
									                            [SCS].[District] AS PresD ON E.PresDistrictID  = PresD.ID
						                            LEFT OUTER JOIN
									                            [SCS].[District] AS ParmD ON E.ParmDistrictID  = ParmD.ID
						                            LEFT OUTER JOIN
									                            [SCS].Country AS PresC ON E.PresCountryID  = PresC.ID
						                            LEFT OUTER JOIN
									                            [SCS].Country AS ParmC ON E.ParmCountryID  = ParmC.ID
                                                    LEFT JOIN [MST].DesignationMaster DM ON DM.DesignationId = E.GivenDesignationId
                                                    LEFT JOIN [HKP].EmployeeCategory EC ON EC.Id = DM.EmployeeCategoryId
						                            
													LEFT OUTER JOIN
																[ORG].[Unit] AS U ON U.ID = EN.UnitID
													LEFT OUTER JOIN
																[ORG].Division AS Dv ON Dv.ID = PO.DivisionID
													LEFT OUTER JOIN
																[ORG].Department AS De ON De.ID = PO.DepartmentID
													LEFT OUTER JOIN
																[HKP].Designation AS Dsg ON Dsg.ID = E.GivenDesignationId
													LEFT OUTER JOIN
																[ORG].Section AS Se ON Se.ID = PO.SectionID
													LEFT OUTER JOIN
																[ORG].SubSection AS SuS ON SuS.ID = PO.SubSectionID
									                LEFT OUTER JOIN
									                            [ORG].Line AS Ln ON Ln.ID = mpb.LineID
									                LEFT OUTER JOIN
									                            [TRN].[SubsectionStructureMaster] AS SSSM ON SSSM.ID = E.SubSecStrucSystemID
						                            LEFT OUTER JOIN
									                            LeavePolicyMaster LPM ON E.LVPolicyMasterSystemID = LPM.SystemID
						                            LEFT OUTER JOIN
									                            SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                                    LEFT OUTER JOIN
																[dbo].[TaxGroupTagWithEmployee] tge on tge.EmpInfoSystemID=E.SystemId
                                                    LEFT OUTER JOIN
									                            TaxGroup tgr ON SRM.TaxGroupID = tgr.SystemID
                                                    LEFT OUTER JOIN
									                            JobLocation JbLc ON E.JobLocationID = JbLc.SystemID
						                            LEFT OUTER JOIN
									                            (
                                                                 SELECT TOP (1) * FROM SalaryIncrementInfoDefineMaster WHERE IsApproved = 0
                                                                ) SID ON E.SystemID = SID.EmpInfoSystemID
                                                    LEFT OUTER JOIN
									                            Resign R ON E.SystemID = R.EmpInfoSystemID AND IsEffected = 1
							         WHERE E.GroupID = '" + sGroupID + @"' AND E.CompanyID = '" + sCompanyID + @"'
                                              AND E.PlantID = '" + sPlantID + @"') A
                         WHERE EmployeeCode = '" + strEmpCode + @"'
                        Order By EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void GetVisitorCategory(string sGroupID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.VisitorCategory
                                    WHERE GroupID = '" + sGroupID + @"'";

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

        public bool CheckShiftAssignIsAttdnLock(string strEmpID, string strEffectDate, out System.Data.DataSet dsRef)
        {
            //System.Data.DataSet dsRef = null;
            string strSQl;
            bool blnStatus = false;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQl = @"SELECT * FROM dbo.EmpDateWiseShiftAssign
                            WHERE EmpSystemID = '" + strEmpID + @"' AND WorkDate >= '" + strEffectDate + @"'
                                    AND AttdnLock = 1
                            ORDER BY WorkDate";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void UpdateEmpDateWiseShiftAssignSingleDate(ParaEmployeeShiftAssign Para)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            string ShiftSystemID = string.Empty;
            string _sql_insert = string.Empty;
            string _sql_update_null_fk = string.Empty;
            string _sql_delete_assign = string.Empty;
            clsEmployeeLoad objEmpLoad = null;
            DataSet dsShiftintime = null;
            string _shiftintime = string.Empty;
            try
            {
                objEmpLoad = new clsEmployeeLoad();
                if (Para.IsFix)
                {
                    ShiftSystemID = Para.FixSystemID;
                }
                else
                {
                    ShiftSystemID = Para.RosterStartShiftID;
                }
                objEmpLoad.GetShiftIntime(ShiftSystemID, out dsShiftintime);
                if (dsShiftintime.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("Shift[" + ShiftSystemID + "] not found...");
                }
                _shiftintime = Convert.ToDateTime(dsShiftintime.Tables[0].Rows[0]["Intime"].ToString()).ToString("dd-MMM-yyyy HH:mm:ss");

                //_sql_delete_assign = @"delete from dbo.EmployeeShiftAssign  WHERE EmpSystemID = '" + Para.EmpSystemID + @"' AND EffectiveDate = '" + Para.EffectiveDate + @"' ";
                //_sql_update_null_fk = @"update dbo.EmpDateWiseShiftAssign set EmpSftAssiSystemID=NULL WHERE EmpSystemID = '" + Para.EmpSystemID + @"' AND WorkDate = '" + Para.EffectiveDate + @"' AND AttdnLock = 0 ";

                //             if (Para.IsFix)
                //             {
                //                 _sql_insert = @"  insert into EmployeeShiftAssign (SystemID,EmpSystemID,FixSystemID,IsFix,IsRoster,
                //                                     EffectiveDate,StartFromDay,AddedBy,DateAdded,UpdatedBy,DateUpdated)
                //values ('" + Para.SystemID + "','" + Para.EmpSystemID + "','" + Para.FixSystemID + "','" + Para.IsFix + "','" + Para.IsRoster + @"'
                //                                 ,'" + Para.EffectiveDate + "','" + Para.StartFromDay + @"'
                //                                 ,'" + Para.AddedBy + @"','" + Para.DateAdded + @"','" + Para.UpdatedBy + @"','" + Para.DateUpdated + @"')";
                //             }
                //             else
                //             {
                //                 _sql_insert = @"  insert into EmployeeShiftAssign (SystemID,EmpSystemID,RosterSystemID,IsFix,IsRoster,
                //                                     EffectiveDate,RosterStartShiftID,StartFromDay,AddedBy,DateAdded,UpdatedBy,DateUpdated)
                //values ('" + Para.SystemID + "','" + Para.EmpSystemID + "','" + Para.RosterSystemID + "','" + Para.IsFix + "','" + Para.IsRoster + @"'
                //                                 ,'" + Para.EffectiveDate + "','" + Para.RosterStartShiftID + "','" + Para.StartFromDay + @"'
                //                                 ,'" + Para.AddedBy + @"','" + Para.DateAdded + @"','" + Para.UpdatedBy + @"','" + Para.DateUpdated + @"')";
                //             }

                //string _sql_update = @"update dbo.EmpDateWiseShiftAssign set EmpSftAssiSystemID='" + Para.SystemID + "',ShiftSystemID='" + ShiftSystemID + "',ToReprocess='No', ShiftIntime='" + _shiftintime + "' WHERE EmpSystemID = '" + Para.EmpSystemID + @"' AND WorkDate = '" + Para.EffectiveDate + @"' AND AttdnLock = 0 ";

                string _sql_update = @"update dbo.EmpDateWiseShiftAssign set ShiftSystemID='" + ShiftSystemID + "',ToReprocess='No', ShiftIntime='" + _shiftintime + "' WHERE EmpSystemID = '" + Para.EmpSystemID + @"' AND WorkDate = '" + Para.EffectiveDate + @"' AND AttdnLock = 0 ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                //objCon.ExecuteNonQueryWrapper(_sql_update_null_fk, true, "1");
                //objCon.ExecuteNonQueryWrapper(_sql_delete_assign, true, "1");
                //objCon.ExecuteNonQueryWrapper(_sql_insert, true, "1");
                objCon.ExecuteNonQueryWrapper(_sql_update, true, "1");
                //////////delete att process
                objCon.ExecuteNonQueryWrapper("UPDATE AttdnRawData set ProcessedFlag = 0 WHERE LogDownLoadNum = '" + Para.EmpSystemID + "' AND PDate = '" + Para.EffectiveDate + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("DELETE FROM [AttdnProcessData] WHERE EmpSystemID = '" + Para.EmpSystemID + "' AND WorkDate = '" + Para.EffectiveDate + "'", true, "1");

                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmpDateWiseShiftAssignSingleDate(string strEmpID, string strEffectDate)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("delete from dbo.EmpDateWiseShiftAssign WHERE EmpSystemID = '" + strEmpID + @"' AND WorkDate = '" + strEffectDate + @"' AND AttdnLock = 0", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmpDateWiseShiftAssign(string strEmpID, string strEffectDate)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("DELETE FROM dbo.EmpDateWiseShiftAssign WHERE EmpSystemID = '" + strEmpID + @"' AND WorkDate >= '" + strEffectDate + @"' AND AttdnLock = 0", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteEmployeeShiftAssign(string strEmpID, string strEffectDate)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM dbo.EmployeeShiftAssign
                                                    WHERE EmpSystemID = '" + strEmpID + @"' AND EffectiveDate >= '" + strEffectDate + @"'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public bool GetMaxDateOfShiftAssign(string strEmpID, string strEffectDate, out System.Data.DataSet dsRef)
        {
            //System.Data.DataSet dsRef = null;
            string strSQl;
            bool blnStatus = false;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQl = @"SELECT MAX(WorkDate) MaxWorkDate FROM dbo.EmpDateWiseShiftAssign
                            WHERE EmpSystemID = '" + strEmpID + @"' AND WorkDate >= '" + strEffectDate + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function

        public bool CheckShiftAssignEffictiveDateIsGrtOthers(string strEmpID, string strEffectDate, string sSystemID, out System.Data.DataSet dsRef)
        {
            //System.Data.DataSet dsRef = null;
            string strSQl;
            bool blnStatus = false;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQl = @"SELECT * FROM dbo.EmployeeShiftAssign
                            WHERE EmpSystemID = '" + strEmpID + @"' AND EffectiveDate >= '" + strEffectDate + @"'
                                    AND SystemID != '" + sSystemID + @"'
                            ORDER BY EffectiveDate DESC";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void GetFixShiftWithOutAshift(string sGroupID, string sPlantID, string sSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.ShiftDefination
                                    WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
                                                AND SystemID != '" + sSystemID + "' Order By ShiftDefinationName";

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

        public void GetGreaterEffectiveDate(string sEmpSystemids, string strEffectDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EffectiveDate
                                FROM [dbo].[EmployeeShiftAssign]
                                where EmpSystemID = '" + sEmpSystemids + @"' and EffectiveDate > '" + strEffectDate + "'";

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

        public void GetEmployeeOperation(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.EmployeeOperation
	                        WHERE EmpSystemID = '" + sEmpSysID + @"'";

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

        public void GetEmployeeMachineType(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.EmployeeMachineType
	                        WHERE EmpSystemID = '" + sEmpSysID + @"'";

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

        public void GetEmployeeOperationForGrd(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {//CONVERT(VARCHAR(10), OP.StandTime, 108) AS
             //strSQL = @"SELECT EOP.systemid OptAndEmpTagSysID
             //         ,OP.id OptSysID
             //         ,OP.code
             //         ,OP.username Descript
             //         ,sam.processid
             //         ,P.username ProcessName
             //         ,OP.operationtypeid OptTypeSysID
             //         ,OPT.code OptypeCode
             //         ,OPT.username OptypeDescript
             //         ,OP.ismachinerequired MachineRequire
             //         ,sam.MachineTypeId MachineTypeSysID
             //         ,MTY.code MachTypeCode
             //         ,MTY.[description] MachTypeDescript
             //         ,OP.UserName StandTime
             //        FROM mst.operation OP
             //        INNER JOIN (SELECT * FROM employeeoperation	WHERE empsystemid = '" + sEmpSysID + @"'	) EOP ON OP.id = EOP.optsysid
             //        left outer join mst.OperationMachineType sam on op.Id=sam.OperationId
             //        LEFT JOIN [HKP].[operationtype] OPT ON OP.operationtypeid = OPT.id
             //        LEFT JOIN [MST].machinetype MTY ON sam.machinetypeid = MTY.id
             //        LEFT JOIN [HKP].process P ON sam.ProcessId = P.id
             //        ORDER BY OP.code";
                strSQL = @"SELECT EOP.SystemID
	                        ,OP.id OperationId
	                        ,OP.code OperationCode
	                        ,OP.username Operation
	                        ,EOP.ProcessId
	                        ,P.username Process
                            ,s.UserName Skill,EOP.SkillId
	                        ,OP.ismachinerequired MachineRequired
	                        ,EOP.AssetItemId, '' AssetItem
                        FROM MST.operation OP
                        INNER JOIN (SELECT * FROM employeeoperation	WHERE empsystemid = '" + sEmpSysID + @"') EOP ON OP.id = EOP.OperationId
					 left outer join HKP.Process p on p.Id=EOP.ProcessId
					 left outer join HKP.Skill s on s.Id=EOP.SkillId
					--left join(select O.OperationId, O.SkillId, O.AssetItemId, AT.UserName MachineName From [MST].[OperationAssetItem] O
					--Left join MST.AssetItem AT ON AT.Id=O.AssetItemId
					--) OAI ON OAI.OperationId=EOP.OperationId AND OAI.SkillId=EOP.SkillId
                        ORDER BY OP.Code";

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

        public void GetEmployeeMachineTypeForGrd(string sEmpSysID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EMT.SystemID , MT.ID AssetItemId, MT.Code, '' Descript, '' ProcessID, MT.ShortName, MT.StandardName, '' UserName,
                         '' ProcessName, '' MachineClassSysID,
                         ''MachineClassCode, '' ClassName
                         FROM [MST].[MaterialMasterArticle] MT
                         INNER JOIN (SELECT * FROM EmployeeMachineType
                         WHERE EmpSystemID = '" + sEmpSysID + @"') EMT ON MT.ID = EMT.AssetItemId";
                //strSQL = @"SELECT EMT.SystemID , MT.ID MachSysID, MT.Code, MT.[Description] Descript, '' ProcessID,
                //               '' ProcessName, MT.MachineClassID MachineClassSysID,
                //                  MC.Code MachineClassCode, MC.UserName ClassName
                //            FROM MST.MachineType MT
                //                  INNER JOIN (SELECT * FROM EmployeeMachineType
                //                                                       WHERE EmpSystemID = '" + sEmpSysID + @"') EMT ON MT.ID = EMT.MachSysID
                //                  --LEFT JOIN [HKP].Process P ON MT.ProcessID = P.ID
                //                  LEFT JOIN [HKP].MachineClass MC ON MT.MachineClassID = MC.ID
                //            ORDER BY MT.Code";
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

        public bool GetMaxDateOfFinalOT(string strEmpID, string strEffectDate, out System.Data.DataSet dsRef)
        {
            string strSQl;
            bool blnStatus = false;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQl = @"SELECT MAX(WorkDate) MaxWorkDate FROM dbo.FinalOT
                            WHERE EmpSystemID = '" + strEmpID + @"' AND WorkDate >= '" + strEffectDate + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void DeleteEmpDateWiseFinalOT(string strEmpID, string strEffectDate)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("DELETE FROM dbo.FinalOT WHERE EmpSystemID = '" + strEmpID + @"' AND WorkDate >= '" + strEffectDate + @"' AND AttdnLock = 0", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public bool isEmployeeOTEntitleExist(string strEmpID, string masterID, string strFromDate, string strToDate)
        {
            bool OTEntitleExist = false;
            string strSQL;
            System.Data.DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT * FROM EmployeeOTEntitle
                            WHERE SystemID <> '" + masterID + @"' AND
                                    ((OTStartDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"') OR
                                     (OTEndDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"'))
                                    AND EmpSystemID = '" + strEmpID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count > 0)
                    OTEntitleExist = true;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
            return OTEntitleExist;
        }//End Function

        public bool isEmpDateWiseJobLocationExist(string strEmpID, string masterID, string sFromDate, out DataSet dsRef)
        {
            bool OTEntitleExist = false;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT * FROM EmpDateWiseJobLocation
                            WHERE SystemID <> '" + masterID + @"' AND EffectiveDate = '" + sFromDate + @"'
                                  AND EmpSystemID = '" + strEmpID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count > 0)
                    OTEntitleExist = true;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
            return OTEntitleExist;
        }//End Function

        public bool isAttendanceLockExist(string sGroupID, string sPlantID, string strEmpID, string strFromDate, string strToDate)
        {
            bool AttendLockExist = false;
            string strSQL;
            System.Data.DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM AttdnProcessData
                                     WHERE IsLock = 1 AND (WorkDate BETWEEN '" + strFromDate + @"' AND '" + strToDate + @"')
                                         AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
                                    AND EmpSystemID = '" + strEmpID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count > 0)
                    AttendLockExist = true;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
            return AttendLockExist;
        }//End Function

        public void GetYearlyCalenderCmb(string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT ID, YearNo FROM YearlyCalendar WHERE PlantID = '" + sPlantID + @"'";

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

        public void GetEducationLevelInfo(string sGroupID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [scs].[QualificationLevel] ";

                if (strName != "")
                {
                    strSQL = strSQL + " AND UserName = '" + strName + "'";
                }
                strSQL = strSQL + " ORDER BY UserName";

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

        public void GetQualificationStream(string sGroupID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [scs].[QualificationStream] ";

                if (strName != "")
                {
                    strSQL = strSQL + " AND UserName = '" + strName + "'";
                }
                strSQL = strSQL + " ORDER BY UserName";

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

        public void GetQualificationCountry(string sGroupID, string strName, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [scs].[Country] ";

                if (strName != "")
                {
                    strSQL = strSQL + " AND UserName = '" + strName + "'";
                }
                strSQL = strSQL + " ORDER BY UserName";

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

        public void GetResultGradeInfo(string sGroupID, string sSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [dbo].[ResultGradeInfo]
                                    WHERE GroupID = '" + sGroupID + @"' ";

                if (sSystemID != "")
                {
                    strSQL = strSQL + " AND SystemID = '" + sSystemID + "'";
                }
                strSQL = strSQL + " ORDER BY SequenceNo";

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

        public void getCountry(string plantId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT AM.CountryId, C.UserName Country From ORG.Plant P
                            LEFT OUTER JOIN  MST.AddressMaster AM ON P.AddressMasterId=AM.Id
                            LEFT OUTER JOIN SCS.Country C ON AM.CountryId=C.Id
                            WHERE P.Id='" + plantId + "'";

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
        }

        public void GetJobLocationMaxDate(string EmpSystemID, string sEffectiveDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Replace(CONVERT(VARCHAR(11), max(EffectiveDate), 106), ' ', '-') ED
                    ,EmpSystemID from EmpDateWiseJobLocation where EmpSystemID='" + EmpSystemID + @"'
                    GROUP BY EmpSystemID HAVING  MAX(EffectiveDate)>'" + sEffectiveDate + "'";

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
        }

        public void GetMaxDateJobLocation(string EmpSystemID, string sSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Replace(CONVERT(VARCHAR(11), max(EffectiveDate), 106), ' ', '-') ED
                    ,EmpSystemID from EmpDateWiseJobLocation
					Where SystemID <> '" + sSystemID + @"' AND EmpSystemID='" + EmpSystemID + @"'
					Group by EmpSystemID";

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
        }

        public void GetEmpCodeGenSetting(string plantId, string EmploymentType, out string pfx, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            pfx = string.Empty;
            try
            {
                strSQL = @"Select A.IsEmployeeCodeOpenField,A.EmpCodeGenType,A.EmpCodeStartValue,A.IsAutoEmpCodeWithPrefix,A.Prefix from [dbo].[EmployeeCodeGenGroup] A
                            LEFT JOIN [dbo].[EmployeeCodeGenGroupDetail] B ON B.EmployeeCodeGenGroupId=A.Id
                            where B.PlantId='"+ plantId + "' and B.EmployeeCodeTypeId='" + EmploymentType + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                if (dsRef.Tables[0].Rows.Count > 0)
                {
                    if (Convert.ToBoolean(dsRef.Tables[0].Rows[0]["IsAutoEmpCodeWithPrefix"].ToString()))
                    {
                        pfx = dsRef.Tables[0].Rows[0]["Prefix"].ToString();
                        if (pfx.Trim().Length == 0)
                        {
                            throw new Exception("No prefix found for this plant...");
                        }
                    }
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
        }

        public void GetMaxEmpCode(string plantId, string employeeCodeTypeId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"Select ISNULL(max(EmployeeCodeNumeric),0)EmployeeCode from EmployeeInformation A 
                        where exists (Select * from EmployeeCodeGenGroupDetail B where A.PlantId=B.PlantId and A.EmployeeCodeTypeId=B.EmployeeCodeTypeId 
                        and EmployeeCodeGenGroupId=(Select EmployeeCodeGenGroupId from EmployeeCodeGenGroupDetail where PlantId='" + plantId + "' and EmployeeCodeTypeId='" + employeeCodeTypeId + "'))";

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
        }

        public void getEmpCodeAuto(string plantId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT max(EmployeeCodeNumeric) c from EmployeeInformation WHERE plantid='" + plantId + "'";

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
        }

        public void GetEmpCodeStartValue(string plantId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EmpCodeStartValue FROM PlantWiseHRMSSetting WHERE PlantId='" + plantId + "'";

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
        }

        public void GetPlantSettingData(string plantId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM PlantWiseHRMSSetting WHERE PlantId='" + plantId + "'";

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
        }

        public void GetShiftIntime(string shiftsystemid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT      [InTime]  FROM [ShiftDefination]
                            WHERE SystemId='" + shiftsystemid + "'";

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
        }

        public void GetEmployeeRelatedLink(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"Select * from (
                                            select distinct ResponsiblePersonId empid,'DocumentConfigurationDesignationGroup' TableName from HKP.DocumentConfigurationDesignationGroup

                                            ) x
                            WHERE x.empid='" + EmployeeId + "'";

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
        }

        public void GetDOJ(string SystemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT DOJ from EmployeeInformation
                            WHERE SystemId='" + SystemId + "'";
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
        }

        public void getSkillByEmployeeId(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * From EmpSkillInformation P
                            WHERE P.EmpSystemID='" + EmployeeId + "'";

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
        }

        public void getMachineTypeByEmployeeId(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * From EmployeeMachineType P
                            WHERE P.EmpSystemID='" + EmployeeId + "'";

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
        }

        public void getOperationByEmployeeId(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * From EmployeeOperation P
                            WHERE P.EmpSystemID='" + EmployeeId + "'";

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
        }

        public void getQualificationByEmployeeId(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * From EmpAcademicQualificationInformation P
                            WHERE P.EmpSystemID='" + EmployeeId + "'";

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
        }

        public void getTrainingByEmployeeId(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * From EmpTrainingInformation P
                            WHERE P.EmpSystemID='" + EmployeeId + "'";

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
        }

        public void getExperienceByEmployeeId(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * From EmpExperienceInformation P
                            WHERE P.EmpSystemID='" + EmployeeId + "'";

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
        }

        public void getJobDescriptionEmployeeId(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * From TRN.EmployeeJobDescription P
                            WHERE P.EmployeeId='" + EmployeeId + "'";

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
        }

        public void getSkill(string budgetId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT PR.PaymentLink
	                            ,PMB.PositionId PositionRelationId
	                            ,PR.UserName PositionRelation
	                            ,PMB.Id MBCodeId
	                            ,PMB.Code MBCode
                            FROM [MST].[ManpowerBudget] PMB
                            LEFT OUTER JOIN [ORG].[Position] PR ON PR.Id = PMB.PositionId
                            Where PMB.Id='" + budgetId + "'";
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
        }
        public void getPlantConfig(string plantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Operation FROM [SCS].[PlantConfig] Where PlantId='" + plantId + "'";
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
        }
        
        public void getEmployeeBudgetCategory(string departmentId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT EBC.UserName, E.EmployeeBudgetCategoryId From [MST].[EmployeeBudgetCategoryDepartment] E
                            LEFT OUTER JOIN [HKP].[EmployeeBudgetCategory] EBC ON EBC.Id=E.EmployeeBudgetCategoryId
                            WHERE DepartmentId='" + departmentId + "'";
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
        }

        public void GetOperationCategoryInfo(string OperationCategoryId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT c.Id
                            FROM hkp.OperationCategory c
                            Where c.Id='" + OperationCategoryId + "' and c.IsOperationMandatoryforEmployee='true'";
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
        }

        public void SaveEmployeeInformation(string sSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeInformation
                                        WHERE SystemID = '" + sSystemID + @"'";

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

        public void SaveEmployeeInformationBack(string sPlantID, string sSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeInformation_BAK
                                        WHERE PlantID = '" + sPlantID + "' AND SystemID = '" + sSystemID + @"'";

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
        public void GetToReprocessEmps(string sPlantID, int iMonthNo, int iYearNo, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select e.EmployeeCode from ExceptionEmployeeSalaryReprocess x
                                    left join EmployeeInformation e on e.SystemId=x.EmpSystemId
                                    where e.PlantId='" + sPlantID + @"' and x.Yearno=" + iYearNo + @" and x.monthno=" + iMonthNo + " ";
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
        public void LoadEmpSalaryProcApprovalGrid(string sPlantID, string sUserGroupID, int iMonthNo, int iYearNo, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
               
                strSQL = @"SELECT IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')
                                                         ELSE Convert(bit, 'True') END,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS, E.EmployeeStatus, '' UserGroupSystemID,
                                  E.DesignationGroupID, DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID,EmployeeCodePreFix,EmployeeCodeNumeric
                           FROM EmployeeInformation E
                                        INNER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = " + iMonthNo + @" AND YearNo = " + iYearNo + @") SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 0 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT JOIN org.Plant F ON E.PlantID = F.Id
                                        LEFT JOIN hkp.DesignationGroup DG ON DG.Id = E.DesignationGroupID
                           WHERE S.IsApproved = 0 ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }

                strSQL += @"
                            ORDER BY F.UserName, EmployeeCodePreFix,EmployeeCodeNumeric";

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
        public void LoadEmpSalaryProcDisbursedGrid(string sPlantID, string sUserGroupID, int iMonthNo, int iYearNo, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')
                                                         ELSE Convert(bit, 'True') END,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS, E.EmployeeStatus, E.UserGroupSystemID,
                                  E.DesignationGroupID, DG.DesignationGroupName AS DesignationGroup, E.SalaryRuleMasterSystemID
                           FROM EmployeeInformation E
                                        INNER JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = " + iMonthNo + @" AND YearNo = " + iYearNo + @") SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 1 AND IsDisbursed = 0
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT JOIN ORG.Plant F ON E.PlantID = F.PlantID
                                        LEFT JOIN HKP.DesignationGroup DG ON DG.SystemID = E.DesignationGroupID
                           WHERE IsDisbursed = 0 ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }

                strSQL += @"
                            ORDER BY F.PlantName, E.EmployeeCode";

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
        public void GetHRMSSettings(string sPlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"Select IsEmpDirectEntryAllowed from PlantWiseHRMSSetting Where PlantID='" + sPlantID + "' AND IsEmpDirectEntryAllowed=1";
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

        public void GetHRMSSettingsForBudgetCode(string sPlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT IsPositionCodeApplicable FROM PlantWiseHRMSSetting WHERE PlantID='" + sPlantID + "' AND IsPositionCodeApplicable=1";
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

        public void GetDOCSettingPlantWise(string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT SystemID from PlantWiseHRMSSetting Where plantid='" + plantid + "' and IsPastDOCAllowed=1";
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


        public void GetDOJSettingPlantWise(string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT SystemID from PlantWiseHRMSSetting Where plantid='" + plantid + "' and IsPastDOJAllowed=1";
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

        public void GetApproved(string plantid, string empSysytemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "Select  IsApproved from dbo.EmployeeInformation Where PlantId='" + plantid + "' AND SystemId='" + empSysytemId + "'";
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
        public void GetRelativeInfo(string empSystemId, out DataSet dsRef)
        {
            string sql;
            ConnectionManager.DAL.ConManager conManager;
            try
            {
                sql = @"SELECT E.EmployeeName,E.EmployeeCode,D.UserName RelativeGivenDesignation FROM EmployeeInformation E
                       LEFT JOIN HKP.Designation D ON D.Id=E.GivenDesignationId
                       WHERE SystemId='" + empSystemId + "'";
                conManager = new ConnectionManager.DAL.ConManager("1");
                conManager.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void GetEmployeeComplianceDocument(string user, string empSystemId, string plantid, string givenDesignationId, string budgetId, string empType)
        {
            string strSQL;
            //ConnectionManager.DAL.ConManager objCon;cls
            clsStaticInfo clsStaticInfo = null;
            try
            {
                strSQL = @"DECLARE @employeeId varchar(20)='" + empSystemId + @"';
									DECLARE @plantId varchar(20)='" + plantid + @"';
									DECLARE @manpowerBudgetId varchar(20);
									DECLARE @givenDesignationId varchar(20);
									DECLARE @empType varchar(20);
									DELETE FROM EmployeeDocument WHERE EmpSystemID=@employeeId AND FileName IS NULL;
									SELECT  @ManpowerBudgetId=BudgetCode, @givenDesignationId=GivenDesignationId, @empType=EmpType FROM EmployeeInformation WHERE SystemId=@employeeId;
									INSERT INTO EmployeeDocument (Id, EmpSystemID, AddedBy, AddedDate, ComplianceDocumentId, OptionalOrMandatory, ComplianceDocumentSetId, ResponsiblePersonId)
									SELECT @employeeId+'-'+ X.ComplianceDocumentId, @employeeId, '" + user + @"', GETDATE(), X.ComplianceDocumentId, X.OptionalOrMandatory, X.ComplianceDocumentSetId, X.ResponsiblePersonId from (
									SELECT CD.Id AS ComplianceDocumentId
									,CDSD.OptionalOrMandatory
									,DC.ComplianceDocumentSetId
									,DC.ResponsiblePersonId
								FROM
								(
								SELECT DISTINCT
										P.EmploymentType
										,DM.EmployeeCategoryId
										,DM.DesignationId
										,P.GivenDesignationId
									FROM EmployeeInformation P
									LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
									WHERE P.GivenDesignationId=@givenDesignationId
									) BD
								LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								AND DC.EmploymentType = BD.EmploymentType
								LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								LEFT OUTER JOIN HKP.ComplianceDocumentPositonCode PC ON CD.Id = PC.ComplianceDocumentId
								LEFT OUTER JOIN ORG.Position PO ON PC.PositionId = PO.Id
								LEFT JOIN MST.ManpowerBudget MB ON MB.PositionId=PO.Id
								WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId =@plantId AND CD.IsSkillBased = 1
								AND MB.Id=@manpowerBudgetId AND (CD.EmpType = @empType OR CD.EmpType = 'Both')
							UNION
									SELECT  CD.Id AS ComplianceDocumentId
									,CDSD.OptionalOrMandatory
									,DC.ComplianceDocumentSetId
									,DC.ResponsiblePersonId
								FROM (
							SELECT DISTINCT
										P.EmploymentType
										,DM.EmployeeCategoryId
										,DM.DesignationId
										,P.GivenDesignationId
									FROM EmployeeInformation P
									LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
									WHERE P.GivenDesignationId=@givenDesignationId
									) BD
								LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								AND DC.EmploymentType = BD.EmploymentType
								LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId = @plantId AND CD.IsSkillBased = 0 AND (CD.EmpType = @empType OR CD.EmpType = 'Both')
								)X  WHERE X.ComplianceDocumentId NOT IN(SELECT ComplianceDocumentId from EmployeeDocument ED WHERE ED.EmpSystemID=@employeeId)";
                clsStaticInfo = new clsStaticInfo();

                clsStaticInfo.SaveEmployeeDocument(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function


        public void SearchEmpEntityWise(string sGroupID, string sPlantID, string sEntityID, string sFrmDt, string sToDt, string strKey, out System.Data.DataSet dsRef)
        {
            var startFromDate = Convert.ToDateTime(sFrmDt);
            var lastDay = DateTime.DaysInMonth(startFromDate.Year, startFromDate.Month); //Number of Days in a month
                                                                                         //var firstDay = new DateTime(startFromDate.Year, startFromDate.Month,1); //Number of Days in a month


            string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(startFromDate.Month);//Month Name from Month No
            var lastDate = lastDay + "-" + monthNameString + "-" + startFromDate.Year;
            var firstDate = "1" + "-" + monthNameString + "-" + startFromDate.Year;

            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                //string _fromDate = DateTime.Now.ToString("dd-MMM-yyyy");
                strSql = @"SELECT  [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
		                             (SELECT E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [DateOfBirth],
		                            E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, E.GenderID GenderName,
                                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [DateOfConfirm],
                                    U.UserName AS Unit, Dv.UserName AS Division, De.UserName AS Department, Se.UserName AS Section,
                                    SuS.UserName SubSection, Dsg.UserName AS Designation, EC.UserName AS 'Employee Category',EC.IdCardFormat,E.EmploymentType,E.SystemID
		                            FROM (
                                            SELECT * FROM
                                                EmployeeInformation
                                                --WHERE SystemID IN
                                                --(
                                                 --SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sFrmDt + @"', '" + sToDt + @"', '" + sPlantID + @"')
                                               -- )
                                          ) AS E
				                            --LEFT OUTER JOIN
							                           -- HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                                            LEFT JOIN [MST].[DesignationMaster] d on d.DesignationId=e.GivenDesignationId
										    LEFT JOIN HKP.EmployeeCategory EC on EC.Id=d.EmployeeCategoryId
				                            LEFT OUTER JOIN
							                            ORG.Unit AS U ON U.Id= E.UnitID
				                            LEFT OUTER JOIN
							                            ORG.Division AS Dv ON Dv.Id= E.DivisionID
				                            LEFT OUTER JOIN
							                            ORG.Department AS De ON De.Id = E.DepartmentID
				                            LEFT OUTER JOIN
							                            HKP.Designation AS Dsg ON Dsg.Id= E.DesignationSystemID
				                            LEFT OUTER JOIN
							                            ORG.Section AS Se ON Se.Id= E.SectionID
				                            LEFT OUTER JOIN
							                            ORG.SubSection AS SuS ON SuS.Id= E.SubSectionID
                                            LEFT OUTER JOIN
							                            MST.ManpowerBudget  AS MB ON MB.Id= E.BudgetCode
							         WHERE E.GroupID = '" + sGroupID + @"'  AND E.PlantId='" + sPlantID + @"'AND MB.EntityId='" + sEntityID + @"' --and E.EmployeeStatus='Active'
                                            and (DOS > '" + firstDate + @"' OR DOS IS NULL OR EmployeeStatus = 'Active') AND
                                                        DOJ <= '" + lastDate + @"'
                                                ) A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void SearchAndSelectMultEmpBasicInfoForUnApproval(string sGroupID, string sPlantID, string strKey, out System.Data.DataSet dsRef)
        {
            //var startFromDate = Convert.ToDateTime(sFrmDt);
            //var lastDay = DateTime.DaysInMonth(startFromDate.Year, startFromDate.Month); //Number of Days in a month
            //var firstDay = new DateTime(startFromDate.Year, startFromDate.Month,1); //Number of Days in a month


            //string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(startFromDate.Month);//Month Name from Month No
            //var lastDate = lastDay + "-" + monthNameString + "-" + startFromDate.Year;
            //var firstDate = "1" + "-" + monthNameString + "-" + startFromDate.Year;

            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                //string _fromDate = DateTime.Now.ToString("dd-MMM-yyyy");
                strSql = @"SELECT  [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
		                             (SELECT E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
		                            E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, E.GenderID GenderName,
                                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                                    U.UserName AS Unit, Dv.UserName AS Division, De.UserName AS Department, Se.UserName AS Section,
                                    SuS.UserName SubSection, Dsg.UserName AS Designation, EC.UserName AS 'Employee Category',EC.IdCardFormat,E.EmploymentType,E.SystemID
		                            FROM (
                                            SELECT * FROM
                                                EmployeeInformation                                                
                                          ) AS E
				                            --LEFT OUTER JOIN
							                           -- HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                                            LEFT JOIN [MST].[DesignationMaster] d on d.DesignationId=e.GivenDesignationId
										    LEFT JOIN HKP.EmployeeCategory EC on EC.Id=d.EmployeeCategoryId
				                            LEFT OUTER JOIN
							                            ORG.Unit AS U ON U.Id= E.UnitID
				                            LEFT OUTER JOIN
							                            ORG.Division AS Dv ON Dv.Id= E.DivisionID
				                            LEFT OUTER JOIN
							                            ORG.Department AS De ON De.Id = E.DepartmentID
				                            LEFT OUTER JOIN
							                            HKP.Designation AS Dsg ON Dsg.Id= E.DesignationSystemID
				                            LEFT OUTER JOIN
							                            ORG.Section AS Se ON Se.Id= E.SectionID
				                            LEFT OUTER JOIN
							                            ORG.SubSection AS SuS ON SuS.Id= E.SubSectionID
                                            INNER JOIN  SalaryInfoDefineMaster as SDM  on SDM.EmpInfoSystemID=E.SystemId
                           
                                
							         WHERE SDM.IsApproved=1 and E.GroupID = '" + sGroupID + @"'  AND E.PlantId='" + sPlantID + @"' and E.EmployeeStatus='Active'
                                            
                                                ) A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void xSearchAndSelectMultEmpBasicInfoForUnApproval(string sGroupID, string sPlantID, string strKey, out System.Data.DataSet dsRef)
        {
            //var startFromDate = Convert.ToDateTime(sFrmDt);
            //var lastDay = DateTime.DaysInMonth(startFromDate.Year, startFromDate.Month); //Number of Days in a month
            //var firstDay = new DateTime(startFromDate.Year, startFromDate.Month,1); //Number of Days in a month


            //string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(startFromDate.Month);//Month Name from Month No
            //var lastDate = lastDay + "-" + monthNameString + "-" + startFromDate.Year;
            //var firstDate = "1" + "-" + monthNameString + "-" + startFromDate.Year;

            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                //string _fromDate = DateTime.Now.ToString("dd-MMM-yyyy");
                strSql = @"SELECT  [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
		                             (SELECT E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
		                            E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, E.GenderID GenderName,
                                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                                    U.UserName AS Unit, Dv.UserName AS Division, De.UserName AS Department, Se.UserName AS Section,
                                    SuS.UserName SubSection, Dsg.UserName AS Designation, EC.UserName AS 'Employee Category',EC.IdCardFormat,E.EmploymentType,E.SystemID
		                            FROM (
                                            SELECT * FROM
                                                EmployeeInformation                                                
                                          ) AS E
				                            --LEFT OUTER JOIN
							                           -- HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                                            LEFT JOIN [MST].[DesignationMaster] d on d.DesignationId=e.GivenDesignationId
										    LEFT JOIN HKP.EmployeeCategory EC on EC.Id=d.EmployeeCategoryId
				                            LEFT OUTER JOIN
							                            ORG.Unit AS U ON U.Id= E.UnitID
				                            LEFT OUTER JOIN
							                            ORG.Division AS Dv ON Dv.Id= E.DivisionID
				                            LEFT OUTER JOIN
							                            ORG.Department AS De ON De.Id = E.DepartmentID
				                            LEFT OUTER JOIN
							                            HKP.Designation AS Dsg ON Dsg.Id= E.DesignationSystemID
				                            LEFT OUTER JOIN
							                            ORG.Section AS Se ON Se.Id= E.SectionID
				                            LEFT OUTER JOIN
							                            ORG.SubSection AS SuS ON SuS.Id= E.SubSectionID
                                            INNER JOIN  SalaryInfoDefineMaster as SDM  on SDM.EmpInfoSystemID=E.SystemId
                           
                                
							         WHERE SDM.IsApproved=1 and E.GroupID = '" + sGroupID + @"'  AND E.PlantId='" + sPlantID + @"' and E.EmployeeStatus='Active'
                                            
                                                ) A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void xSearchAndSelectMultEmpBasicInfoPlantWisePaySlip(string sGroupID, string sPlantID, string sPayGrp, string sYr, string sMth, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            clsStaticInfo obs = null;
            DateTime _dateStart;


            if (sMth != "" && sYr != "")
            {
                string month = sMth.ToString();
                string year = sYr.ToString();
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                _dateStart = Convert.ToDateTime(daysInMonth.ToString() + "-" + bplib.clsWebLib.GetMonthName(month) + "-" + year);
            }
            else
            {

                string month = DateTime.Now.ToString("MMMM");
                string year = DateTime.Now.ToString("yyyy");
                string datestart = "01-" + month + "-" + year;

                _dateStart = Convert.ToDateTime(datestart).AddMonths(-1);
            }

            var _wcp = string.Empty;

            if (sPayGrp != "Select")
            {
                _wcp = " AND E.SystemId IN(select employeeid from MST.PayrollGroupMaster where PayrollGroupId = '" + sPayGrp + @"')";
            }
            else
            {
                _wcp = " AND E.SystemId NOT IN(select employeeid from MST.PayrollGroupMaster)";
            }

            try
            {
                obs = new clsStaticInfo();
                //strSql = @"SELECT TOP (100) [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
                //              (SELECT E.SystemId EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
                //              E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID,E.GenderID,
                //                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                //                    U.UserName AS Unit, Dv.UserName  AS Division, dp.UserName AS Department, S.UserName AS Section,
                //                    sb.UserName SubSection, D.UserName AS Designation, EC.UserName, E.SystemID
                //              FROM EmployeeInformation AS E
                //               "+ obs.EntityTables()+ @"
                //WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + @"' AND E.EmployeeStatus='Active' AND E.IsApproved=1) A ";
                strSql = @"SELECT [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
		                            (SELECT E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
		                            E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, E.GenderID,
                                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                                    U.UserName AS Unit, Dv.UserName  AS Division, dp.UserName AS Department, S.UserName AS Section,
                                    sb.UserName SubSection, D.UserName AS Designation, EC.UserName, E.SystemId
		                            FROM EmployeeInformation AS E
LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id 
				                           " + obs.EntityTables() + @"
							         WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + @"' " + _wcp + @"
                                                    AND
                                       (DOS IS NULL OR Convert(date,DOS) >= Convert(Date,'" + _dateStart + @"') OR EmployeeStatus='Active')
                                    ) A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void xSearchAndSelectMultEmpBasicInfoPlantWisePaySlip(string sGroupID, string sPlantID, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            clsStaticInfo obs = null;

            string month = DateTime.Now.ToString("MMMM");
            string year = DateTime.Now.ToString("yyyy");
            string datestart = "01-" + month + "-" + year;

            DateTime _dateStart = Convert.ToDateTime(datestart).AddMonths(-1);

            try
            {
                obs = new clsStaticInfo();
                //strSql = @"SELECT TOP (100) [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
                //              (SELECT E.SystemId EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
                //              E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID,E.GenderID,
                //                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                //                    U.UserName AS Unit, Dv.UserName  AS Division, dp.UserName AS Department, S.UserName AS Section,
                //                    sb.UserName SubSection, D.UserName AS Designation, EC.UserName, E.SystemID
                //              FROM EmployeeInformation AS E
                //               "+ obs.EntityTables()+ @"
                //WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + @"' AND E.EmployeeStatus='Active' AND E.IsApproved=1) A ";
                strSql = @"SELECT [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
		                            (SELECT E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
		                            E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, E.GenderID,
                                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                                    U.UserName AS Unit, Dv.UserName  AS Division, dp.UserName AS Department, S.UserName AS Section,
                                    sb.UserName SubSection, D.UserName AS Designation, EC.UserName, E.SystemId
		                            FROM EmployeeInformation AS E
LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id 
				                           " + obs.EntityTables() + @"
							         WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + @"'
                                                   AND (DOS IS NULL OR DOS>'" + _dateStart + @"' OR EmployeeStatus='Active')
                                    ) A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void GetPayGroup(string sa, string ca, string USERPK, out DataSet dsLocal)
        {

            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                if (ca.ToUpper() == "TRUE" || sa.ToUpper() == "TRUE")
                {
                    strSql = @"SELECT HPG.Id, HPG.UserName 
                            	                            FROM HKP.PayrollGroup HPG ORDER BY HPG.Sequence";
                }
                else
                {
                    strSql = @"SELECT HPG.Id, HPG.UserName 
                            FROM HKP.PayrollGroup HPG
                            WHERE Id IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup WHERE UserId = '" + USERPK + @"') AND HPG.active=1 ORDER BY HPG.Sequence";
                }

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
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

        public void ValidationEmpDelete(string id, out DataSet dsLocal)
        {

            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {

                strSql = @"IF EXISTS(SELECT 1 FROM ( 
                                SELECT EmployeeId AS CheckingColumn FROM TRN.VoucherDetail vd 
                                 UNION ALL 
                                SELECT EmployeeId AS CheckingColumn FROM [TRN].[OpeningBalanceDetail] obd 
                                UNION ALL 
                                SELECT EmployeeId AS CheckingColumn FROM [TRN].[EmployeePayable] ep 
                                UNION ALL 
                                SELECT EmployeeId AS CheckingColumn FROM  SEC.[User] u 
                                UNION ALL 
                                SELECT EmployeeId AS CheckingColumn FROM  TRN.ExpenseBooking eb
                                UNION ALL 
                                SELECT EmployeeId AS CheckingColumn FROM  TRN.Advance a 
                                ) A WHERE CheckingColumn ='" + id + @"') SELECT 1 Result ELSE SELECT 0 Result RETURN";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
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
        public void DeleteEmp(string empSystemID)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                string strSql = string.Empty;
                if (empSystemID != "")
                {


                    strSql = @"             delete from TaxGroupTagWithEmployee where EmpInfoSystemID='" + empSystemID + @"'
                                            delete from AttdnManualData where EmpSystemID='" + empSystemID + @"'
                                            delete from hkp.EmployeeMobileAppsAuthorization where EmployeeId='" + empSystemID + @"'
                                            delete from EmployeeBankInfo where EmpSystemID='" + empSystemID + @"'
                                            delete from SalaryInfoDefineMaster where EmpInfoSystemID='" + empSystemID + @"'
                                            delete from SalaryProcChild where EmpInfoSystemID='" + empSystemID + @"'
                                            delete from TRN.EmployeeLeaveSummary where EmployeeId='" + empSystemID + @"'
                                            delete from SalaryProceAttdnData where EmpSystemID='" + empSystemID + @"'
                                            delete from LeaveTransactionDetails where LvTrnsSystemID in (select SystemID from LeaveTransaction where EmpSystemID='" + empSystemID + @"')
                                            delete from LeaveTransaction where EmpSystemID='" + empSystemID + @"'
                                            delete from PFMonthlyDistributionEmployer where PFMntEmpWiseCalID in (select id from PFMonthlyEmpWiseCalculation where PFEligibleEmpID in (select id from PFEligibleEmployee where EmpSystemID='" + empSystemID + @"'))
                                            delete from PFMonthlyEmpWiseCalculation where PFEligibleEmpID in (select id from PFEligibleEmployee where EmpSystemID='" + empSystemID + @"')
                                            delete from PFEligibleEmployee where EmpSystemID='" + empSystemID + @"'
                                            delete from BonusPolicyMonthlyRetainEligibleEmployee where EmpSystemID='" + empSystemID + @"'
                                            delete from BonusPolicyMonthlyRetainDistributionPmt where BnsPlyMntRetainID in (select ID from BonusPolicyMonthlyRetainEmpWiseCalculation where EmpSystemID='" + empSystemID + @"')
                                            delete from BonusPolicyMonthlyRetainEmpWiseCalculation where EmpSystemID='" + empSystemID + @"'
                                            delete from ESICMonthlyEmpWiseCalculation where ESICEligibleEmpID in (select id from ESICEligibleEmployee where EmpSystemID='" + empSystemID + @"')
                                            delete from ESICEligibleEmployee where EmpSystemID='" + empSystemID + @"'
                                            delete from dbo.EmpDateWiseShiftAssign where EmpSystemID='" + empSystemID + @"'
                                            delete from dbo.EmployeeShiftAssign where EmpSystemID='" + empSystemID + @"'
                                            delete from dbo.SalaryIncrementNextDueDate Where EmpSystemID='" + empSystemID + @"'
                                            delete from dbo.EmployeeWeekOffByDay Where EmpSystemID='" + empSystemID + @"'
                                            delete from EmployeeDocument Where EmpSystemID='" + empSystemID + @"'
                                            delete from dbo.SalaryInfoDefineMaster where EmpInfoSystemID='" + empSystemID + @"'
                                            delete from dbo.SalaryProcChild where EmpInfoSystemID='" + empSystemID + @"'
                                            delete from TRN.EmployeeLeaveSummary where EmployeeId='" + empSystemID + @"'
                                            delete from dbo.AttdnDataMonthlySummary where EmpSystemID='" + empSystemID + @"'
                                            delete from TRN.EmployeeProbationalPeriod where EmployeeId='" + empSystemID + @"'
                                            delete from dbo.EmpDateWiseJobLocation where EmpSystemID='" + empSystemID + @"'
                                            delete from TRN.AdvanceDetail Where EmployeeId='" + empSystemID + @"'
                                            delete from dbo.EmployeeBudgetCodeHistory where EmpSystemID='" + empSystemID + @"'
                                            delete from dbo.BonusPolicyMonthlyRetainDistributionStrcPmt where  BnsPlyMntRetainID IN (select ID from dbo.BonusPolicyMonthlyRetainStrcEmpWiseCalculation where EmpSystemID = '" + empSystemID + @"') 
                                            delete from dbo.BonusPolicyMonthlyRetainStrcEmpWiseCalculation where EmpSystemID='" + empSystemID + @"'
                                            delete from dbo.EmpReferenceInformation where EmpSystemID='" + empSystemID + @"'
                                            delete from dbo.AttdnProcessData where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.AccessControllerEmployeeTag WHERE EmpInfoSystemID= '" + empSystemID + @"'
                                            DELETE FROM MST.PayrollGroupMaster WHERE EmployeeId='" + empSystemID + @"'
                                            DELETE FROM dbo.SalaryInfoBack WHERE SalaryID IN (SELECT SystemID FROM dbo.SalaryInfoBackMaster WHERE EmpInfoSystemID='" + empSystemID + @"')
                                            DELETE FROM dbo.SalaryInfoBackMaster WHERE EmpInfoSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.EmployeeFPInformation WHERE EmpSystemId= '" + empSystemID + @"'
                                            DELETE FROM dbo.BonusPaymentActual WHERE EmpSystemID = '" + empSystemID + @"'
                                            DELETE FROM dbo.AccessControllerDeleteRequest WHERE EmpInfoSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.AttdnManualDataBackUp WHERE EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.EmpReportingPerson WHERE RptEmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.CompliedEmployeeRoster WHERE EmpSystemId ='" + empSystemID + @"'
                                            DELETE FROM dbo.AttendanceRestDetail WHERE EmpSystemId ='" + empSystemID + @"'
                                            DELETE FROM dbo.CompliedShiftAssignment WHERE EmpSystemId ='" + empSystemID + @"'
                                            DELETE FROM dbo.AttdnProcessFinalData WHERE EmpSystemId ='" + empSystemID + @"'                                           
                                            DELETE FROM dbo.EmployeeOnDutyDetails where OnDutyId in(select Id from dbo.EmployeeOnDuty where EmpSystemId = '" + empSystemID + @"')
                                            DELETE FROM dbo.EmployeeOnDuty where EmpSystemId = '" + empSystemID + @"'
                                            DELETE FROM dbo.AttdnRawDataFromApp WHERE EmployeeId ='" + empSystemID + @"'
                                            DELETE FROM AttdnRawDataFromApp WHERE EmployeeId ='" + empSystemID + @"'
                                            DELETE FROM EmployeeOTEntitle where EmpSystemID ='" + empSystemID + @"'
                                            DELETE FROM dbo.EmployeeBankInfoBackUp where EmpSystemID ='" + empSystemID + @"'
                                            DELETE FROM MST.PaidHoursEmployeeAssign where EmployeeId ='" + empSystemID + @"'
                                            DELETE FROM dbo.TaxOpeningBalance where EmpInfoSystemID ='" + empSystemID + @"'
                                            DELETE FROM dbo.TaxOpeningBalance where EmpInfoSystemID ='" + empSystemID + @"'
                                            DELETE FROM SEC.[User] where EmployeeId = '" + empSystemID + @"'
                                            DELETE FROM SEC.PasswordHistory where UserId =(select Id from SEC.[USER] where EmployeeId ='" + empSystemID + @"')
                                            DELETE FROM DBO.TaxableYearlyActualIncomeSalaryHeadWise where EmpInfoSystemID = '" + empSystemID + @"'
                                            DELETE FROM dbo.TaxDeductionInfoMonthWise where EmpInfoSystemID = '" + empSystemID + @"' 
                                            DELETE FROM dbo.TaxableIncomeSalaryHeadWise where EmpInfoSystemID = '" + empSystemID + @"'                                             DELETE FROM dbo.TaxableIncomeSalaryHeadWise where EmpInfoSystemID = '" + empSystemID + @"' 
                                            DELETE FROM dbo.TaxDefineMaster where EmpInfoSystemID = '" + empSystemID + @"'
                                            DELETE FROM MST.EmployeeResponsiblePerson where EmployeeId = '" + empSystemID + @"'


                                            DELETE FROM EmployeeInformation where systemid='" + empSystemID + @"'";
                }
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(strSql, true, "1");


                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                }
                catch (Exception ex2)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void SearchAndSelectMultEmpBasicInfoPlantWisePG(string sGroupID, string sPlantID, string sYr, string sMth, string sPayGrp, string strKey, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            clsStaticInfo obs = null;
            DateTime _dateStart;
            string _dateJoin = "";
            string month = "";
            string year = "";
            var daysInMonth = 0;//Number of Days in a month


            if (sMth != "" && sYr != "")
            {
                month = sMth.ToString();
                year = sYr.ToString();
                daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                _dateStart = Convert.ToDateTime(1 + "-" + bplib.clsWebLib.GetMonthName(month) + "-" + year);
                _dateJoin = daysInMonth + "-" + bplib.clsWebLib.GetMonthName(month) + "-" + year;
            }
            else
            {

                month = DateTime.Now.ToString("MMMM");
                year = DateTime.Now.ToString("yyyy");
                daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                string datestart = "01-" + month + "-" + year;
                _dateStart = Convert.ToDateTime(1 + "-" + bplib.clsWebLib.GetMonthName(month) + "-" + year);

                _dateStart = Convert.ToDateTime(datestart).AddMonths(-1);
                _dateJoin = daysInMonth + "-" + bplib.clsWebLib.GetMonthName(month) + "-" + year;
            }
            var _wcp = string.Empty;
            var _wcd = string.Empty;

            if (sPayGrp.ToUpper() != "NO GROUP")
            {
                _wcp = " AND E.SystemId IN(select employeeid from MST.PayrollGroupMaster where PayrollGroupId = '" + sPayGrp + @"')";
            }
            else
            {
                _wcp = " AND E.SystemId NOT IN(select employeeid from MST.PayrollGroupMaster)";
            }

            ////if (sDepartmentId != "ALL")
            ////{
            ////    _wcd = " AND DP.Id = '" + sDepartmentId + @"'";
            ////}
            ////else
            ////{
            ////    _wcd = "";
            ////}

            try
            {
                obs = new clsStaticInfo();
                strSql = @"SELECT [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
		                            (SELECT  E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
		                            E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, E.GenderID,
                                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                                    U.UserName AS Unit, Dv.UserName  AS Division, dp.UserName AS Department, S.UserName AS Section,
                                    sb.UserName SubSection, D.UserName AS Designation, EC.UserName, E.SystemId, LD.UserName LegalDesignation
		                            FROM EmployeeInformation AS E
LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id 
				                           " + obs.EntityTables() + @"
							         WHERE E.GroupID = '" + sGroupID + @"' AND E.PlantID = '" + sPlantID + @"' AND (DOS IS NULL OR DOS>'" + _dateStart + @"' OR EmployeeStatus='Active')
                                    " + _wcd + @" " + _wcp + @" AND (DOJ IS NULL OR DOJ<= '" + _dateJoin + @"')
                                    ) A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void PayGroupWiseSearchAndSelectMultEmpBasicInfo(ParamList para, string sGroupID, string sPlantID, string sFrmDt, string sToDt, string strKey, out System.Data.DataSet dsRef)
        {
            var startFromDate = Convert.ToDateTime(sFrmDt);
            var lastDay = DateTime.DaysInMonth(startFromDate.Year, startFromDate.Month); //Number of Days in a month
                                                                                         //var firstDay = new DateTime(startFromDate.Year, startFromDate.Month,1); //Number of Days in a month


            string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(startFromDate.Month);//Month Name from Month No
            var lastDate = lastDay + "-" + monthNameString + "-" + startFromDate.Year;
            var firstDate = "1" + "-" + monthNameString + "-" + startFromDate.Year;

            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            var _wcp = string.Empty;

            if (para.EmployeeId != "")
            {
                _wcp = "  SYSTEMID IN (" + para.EmployeeId + @")";
            }
            else
            {
                if (para.PayGroup.ToUpper() != "NO GROUP")
                {
                    _wcp = " AND E.SystemId IN(select employeeid from MST.PayrollGroupMaster where PayrollGroupId = '" + para.PayGroup + @"')";
                }
                else
                {
                    _wcp = " AND E.SystemId NOT IN(select employeeid from MST.PayrollGroupMaster)";
                }
            }
            try
            {
                //string _fromDate = DateTime.Now.ToString("dd-MMM-yyyy");
                strSql = @"SELECT  [CheckBoxSelectEmp] = Convert(bit, 'False'), * FROM
		                             (SELECT  E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS [Date Of Birth],
		                            E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID, E.GenderID GenderName,
                                    REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS [Date Of Join], REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS [Date Of Confirm],
                                    U.UserName AS Unit, Dv.UserName AS Division, De.UserName AS Department, Se.UserName AS Section,
                                    SuS.UserName SubSection, Dsg.UserName AS Designation, EC.UserName AS 'Employee Category',EC.IdCardFormat,E.EmploymentType,E.SystemID
		                            FROM (
                                            SELECT * FROM
                                                EmployeeInformation
                                                --WHERE SystemID IN
                                                --(
                                                 --SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sFrmDt + @"', '" + sToDt + @"', '" + sPlantID + @"')
                                               -- )
                                          ) AS E
				                            --LEFT OUTER JOIN
							                           -- HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                                            LEFT JOIN [MST].[DesignationMaster] d on d.DesignationId=e.GivenDesignationId
										    LEFT JOIN HKP.EmployeeCategory EC on EC.Id=d.EmployeeCategoryId
				                            LEFT OUTER JOIN
							                            ORG.Unit AS U ON U.Id= E.UnitID
				                            LEFT OUTER JOIN
							                            ORG.Division AS Dv ON Dv.Id= E.DivisionID
				                            LEFT OUTER JOIN
							                            ORG.Department AS De ON De.Id = E.DepartmentID
				                            LEFT OUTER JOIN
							                            HKP.Designation AS Dsg ON Dsg.Id= E.DesignationSystemID
				                            LEFT OUTER JOIN
							                            ORG.Section AS Se ON Se.Id= E.SectionID
				                            LEFT OUTER JOIN
							                            ORG.SubSection AS SuS ON SuS.Id= E.SubSectionID
							         WHERE E.GroupID = '" + sGroupID + @"'  AND E.PlantId='" + sPlantID + @"' --and E.EmployeeStatus='Active'
                                            and (DOS > '" + firstDate + @"' OR DOS IS NULL OR EmployeeStatus = 'Active') AND
                                                        DOJ <= '" + lastDate + @" '" + _wcp + @"
                                                ) A ";

                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By EmployeeCodePreFix,EmployeeCodeNumeric";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void EmployeeDocFile(string strSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeDocument where ComplianceDocumentId=(select Id from HKP.ComplianceDocument where ProfileType ='Photo') 
                               AND EmpSystemId ='" + strSystemID + @"'";

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

        public void EmployeeQualificationFile(string strSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeDocument where ComplianceDocumentId=(select Id from HKP.ComplianceDocument where ProfileType ='Qualification') 
                               AND EmpSystemId ='" + strSystemID + @"'";

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

        public void EmployeeDocFileUpLoad(string Id, string strSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var fileName = strSystemID + ".jpg";
                strSQL = @"UPDATE EmployeeDocument SET FileId='" + strSystemID + @"', FileName='" + fileName + @"' WHERE  Id = '" + Id + @"'";

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
        public void GetEmpOTEntitle(string txtId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SystemID, EmpSystemID from EmployeeOTEntitle where EmpSystemID ='" + txtId + @"'";

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
        public void EmployeeTrainingFile(string strSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeDocument where ComplianceDocumentId=(select Id from HKP.ComplianceDocument where ProfileType ='Training') 
                               AND EmpSystemId ='" + strSystemID + @"'";

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
        public void EmployeeExperienceFile(string strSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeDocument where ComplianceDocumentId=(select Id from HKP.ComplianceDocument where ProfileType ='Experience') 
                               AND EmpSystemId ='" + strSystemID + @"'";

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
        public void LoadStatusDifferent(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT E.SystemId,E.EmployeeCode
	                                            ,E.EmployeeName
	                                            ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
	                                            ,REPLACE(CONVERT(VARCHAR(11), E.DOS, 106), ' ', '-') DOS
	                                            ,E.EmployeeCurrentStatus EmployeeStatus
	                                            ,dgs.UserName LegalDesignation
                                            	,s.UserName Section
	                                            ,s.UserName Subsection
                                                ,E.SystemId EmpSystemId ,e.GivenDesignationId
                                            FROM EmployeeInformation E
                                            LEFT OUTER JOIN hkp.LegalDesignation dgs ON dgs.Id = E.LegalDesignationId
                                            left join org.Section s on s.id=e.SectionId
                                            left join org.SubSection ss on ss.id=e.SubSectionId
                                            WHERE (
		                                            e.SystemId in (select systemid from EmployeeInformation where  EmployeeStatus='Active' and EmployeeCurrentStatus in (" + bplib.clsWebLib.EMP_OTHER_STATUS + @") and EmployeeCurrentStatusEffectiveDate<'" + sToDate + @"')
		                                            )
	                                            AND E.SystemId IN (
		                                            SELECT EmpSystemID
		                                            FROM AttdnDataMonthlySummary
		                                            WHERE YearNo = Year('" + sFromDate + @"')
			                                            AND MonthNo = Month('" + sFromDate + @"')			                                            
			                                            AND PlantID = '" + sPlantID + @"'
		                                            ) --not in
	                                            AND E.PlantID = '" + sPlantID + @"'  

                                            --Approved SP
                                                        and e.systemid not in
                                                        (
                                                        SELECT distinct SC.EmpInfoSystemID
                                                        FROM SalaryProcChild SC
                                                        INNER JOIN (SELECT SystemID FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
                                                        WHERE IsApproved = 1
                                                        )--Approved SP

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

                                                ";

                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }

                strSQL += @"
                            ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric ";

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
                //strSQL = @"select EmpSystemID from AttdnProcessData where isnull(MaternityStatus,'')='MLV' 
                //                        and WorkDate between '" + sFromDate + @"' and '" + sToDate + @"' and PlantId='" + sPlantID + @"'";
                strSQL = @"select EmpSystemID from LeaveTransaction where FromDate between '" + sFromDate + @"'  and '" + sToDate + @"' 
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
        public void LoadExceptionEmps(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select  e.systemid,e.EmployeeCode
                                    ,e.EmployeeName 
                                    ,EmployeeCodePreFix,EmployeeCodeNumeric
                                    ,format(e.DOJ,'dd-MMM-yyyy') DOJ
                                    ,format(e.DOS,'dd-MMM-yyyy') DOS
                                    ,d.UserName GivenDesignation
                                    ,dd.UserName LegalDesignation
                                    ,s.UserName Section
                                    ,ss.UserName Subsection
                                    ,e.EmployeeStatus
                                    from ExceptionEmployee a
                                    inner join EmployeeInformation e on e.SystemId=a.EmpSystemID
									left join hkp.Designation d on d.id=e.GivenDesignationId
		                            left join org.SubSection ss on ss.id=e.SubSectionId         
									left join org.Section s on s.id=e.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId
                                    where e.PlantId='" + sPlantID + @"' 									
									and a.[ExceptionCategory]='SalaryProcess'
									and a.IsActive=1
									and a.IsForever=1
                                    and e.systemid not in 
                                    (
                                    " + MLVEmp_WC(sPlantID, sFromDate, sToDate) + @"
                                    )
                                                ";
                strSQL += @"
                            ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric ";

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
									left join hkp.Designation d on d.id=e.GivenDesignationId
		                            left join org.SubSection ss on ss.id=e.SubSectionId         
									left join org.Section s on s.id=e.SectionId
									left join hkp.LegalDesignation dd on dd.id=e.LegalDesignationId
                                    where e.PlantId='" + sPlantID + @"' 									
									and a.[ExceptionCategory]='SalaryProcess'
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
        public void LoadSalaryApproved(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                strSQL = @"SELECT IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')
                                                              ELSE Convert(bit, 'True') END,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                   DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,
                                    --y.ToDate,
                                  '' ToDate,
                                 ''  ProcessStatus,
								  '' BankAccountStatus 

                           FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                        LEFT OUTER JOIN EmployeeBankInfo EBI ON E.SystemID = EBI.EmpSystemID AND EBI.IsApproved = 1
                                        inner JOIN (
										            SELECT SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
					                                 FROM SalaryProcChild SC
															INNER JOIN (SELECT * FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
														WHERE IsApproved = 1
					                                 GROUP BY SC.SlrProcMstSystemID, SC.IsApproved, SC.IsDisbursed, SC.EmpInfoSystemID, SM.MonthNo, SM.YearNo
												   ) S ON E.SystemID = S.EmpInfoSystemID
                                        LEFT OUTER JOIN org.Plant F ON E.PlantID = F.Id
                                        LEFT OUTER JOIN hkp.Designation dgs ON dgs.Id = E.GivenDesignationId
                                        
                            ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }


                strSQL += @"
                            ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric --F.UserName,dgs.UserName,";

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
        
        public void LoadZeroPresent(string Status, string sPlantID, string sUserGroupID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string dtFD = Convert.ToDateTime(sFromDate).AddDays(-1).ToString("dd-MMM-yyyy");
                strSQL = @"SELECT IsSelectSlrProc = Case WHEN S.SlrProcMstSystemID IS NULL THEN Convert(bit, 'False')
                                                              ELSE Convert(bit, 'True') END,
,EmployeeCodePreFix,EmployeeCodeNumeric,
                                  S.SlrProcMstSystemID AS SystemID, ISNULL(S.IsApproved, 0) IsApproved, ISNULL(S.IsDisbursed, 0) IsDisbursed, E.SystemID AS EmpSystemID,
                                  E.EmployeeCode, E.EmployeeName, F.UserName PlantName, E.PlantID, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106),' ','-') DOJ,E.DOJ DOJs,
                                  REPLACE(CONVERT(VARCHAR(11), E.DOS, 106),' ','-') DOS,E.DOS DOSs, E.EmployeeStatus, E.EmployeeGroupSystemID UserGroupSystemID,
                                  , DG.UserName AS DesignationGroup, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName ,dgs.UserName GivenDesignation,e.GivenDesignationId,
                                    --y.ToDate,
                                  REPLACE(CONVERT(VARCHAR(11), y.ToDate, 106),' ','-') ToDate,
                                  ProcessStatus = CASE WHEN Y.ToDate>'" + sFromDate + @"' THEN 'Overlap'
                                      WHEN Y.ToDate<'" + dtFD + @"' then 'Gap'
                                      ELSE 'OK' END,
								  BankAccountStatus = CASE WHEN EBI.BankSystemID IS NOT NULL THEN 'Bank Payment'
										ELSE 'Cash Payment' END

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
                                        LEFT JOIN MST.DesignationMaster DEM ON DEM.DesignationId=E.GivenDesignationID
                                        LEFT OUTER JOIN HKP.DesignationGroup DG ON DG.Id = DEM.DesignationGroupID

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

                                                WHERE (E.EmployeeStatus in ('Active','Separated')) 
                                                and  e.SystemId not in (select systemid from EmployeeInformation where  EmployeeStatus='Active' and EmployeeCurrentStatus in (" + bplib.clsWebLib.EMP_OTHER_STATUS + @") and EmployeeCurrentStatusEffectiveDate<'" + sToDate + @"')
                                                and (DOS is null or DOS >= '" + sFromDate + @"') AND E.DOJ <= '" + sToDate + @"'
                                                and  E.SystemId in
										                (														                
							                                select EmpSystemID from AttdnDataMonthlySummary where  YearNo=Year('" + sFromDate + @"') and MonthNo=Month('" + sFromDate + @"') and TotalPresent=0 and TotalLv=0 and TotalLate=0  and PlantID='" + sPlantID + @"'
										                )--not in

                                                    --Approved SP
                                                        and e.systemid not in
                                                        (
                                                        SELECT distinct SC.EmpInfoSystemID
                                                        FROM SalaryProcChild SC
                                                        INNER JOIN (SELECT SystemID FROM SalaryProcMaster WHERE MonthNo = Month('" + sFromDate + @"') AND YearNo = Year('" + sFromDate + @"')) SM ON SC.SlrProcMstSystemID = SM.SystemID
                                                        WHERE IsApproved = 1
                                                        )--Approved SP
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
                                                ";

                if (sUserGroupID != "ALL")
                {
                    strSQL += @"
                               AND E.UserGroupSystemID = '" + sUserGroupID + @"'";
                }
                if (sPlantID != "ALL")
                {
                    strSQL += @"
                               AND E.PlantID = '" + sPlantID + @"'";
                }


                strSQL += @"
                            ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric --F.UserName,dgs.UserName,";

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