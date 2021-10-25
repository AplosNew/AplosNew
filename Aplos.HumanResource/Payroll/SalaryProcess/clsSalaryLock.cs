using Library.Crosscutting.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.SalaryProcess
{
   public class clsSalaryLock
    {
        public string GetEmpInfo(CustomIdentity _identity,string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = _identity;
            bool sa = identity.IsSysAdmin;
            bool ca = identity.IsControlAdmin;
            string userId = identity.UserId;
            string plantId = identity.PlantId;
            string companyGroupId = identity.CompanyGroupId;

            var wcPayrollGroup = "";
            var wcSalaryProcess = "";
            var salaryProcessJoin = "";
            var salaryProcessColumn = "";
            var strDOJ = "";
            string salaryProcessFlag = "";
            string wcEmpStatus = " Where (1=0 ";

            if (sa == true || ca == true)
            {
                wcPayrollGroup = @"";
            }
            else
            {
                wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
            }
            if (salaryProcessId == "STRUCTURE")
            {
                salaryProcessColumn = "";
                salaryProcessJoin = "";
                wcSalaryProcess = "";
                strDOJ = "AND DOJ<='" + effectiveDate + @"' AND (DOS is null OR DOS>= '" + effectiveDate + @"')";


            }
            else if (!string.IsNullOrEmpty(salaryProcessId))
            {
                salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";

                salaryProcessJoin = @" LEFT  JOIN (
									  SELECT c.* FROM SalaryProcChild c 
									  inner join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID and MonthNo =  Month('" + effectiveDate + @"') AND YearNo =  Year('" + effectiveDate + @"')
									  WHERE PlantID = '" + plantId + @"'									  
									  ) SPC ON SPC.EmpInfoSystemID = E.SystemId
                                        LEFT  JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                                        left join salaryprocesslogdetail spd on spd.EmpSystemId=SPC.EmpInfoSystemID and spd.SalaryProcessId=spm.SystemID";

               
                wcSalaryProcess = @"AND SPC.SlrProcMstSystemID IN('" + salaryProcessId + @"')";

            }
            else if (string.IsNullOrEmpty(salaryProcessId) == true && salaryProcessId != "STRUCTURE")
            {
                salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";

                salaryProcessJoin = @" LEFT  JOIN (
									  SELECT c.* FROM SalaryProcChild c 
									  inner join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID and MonthNo =  Month('" + effectiveDate + @"') AND YearNo =  Year('" + effectiveDate + @"')
									  WHERE PlantID = '" + plantId + @"'									  
									  ) SPC ON SPC.EmpInfoSystemID = E.SystemId
                                        LEFT  JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                                        left join salaryprocesslogdetail spd on spd.EmpSystemId=SPC.EmpInfoSystemID and spd.SalaryProcessId=spm.SystemID";


               
            }
            if (salaryProcessId == "STRUCTURE")
            {
                wcEmpStatus = " Where (1=1 ";
                salaryProcessFlag = "";
            }
            else
            {
                salaryProcessFlag = ", Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag";
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
            }

            wcEmpStatus += ")";

            var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
            var param = string.Empty;
            if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
            else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                param = "E.GroupID='" + companyGroupId + "'";

            string sql = @"SELECT [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId 
	                                ,sl.Id,CheckBoxSelect=case when  sl.Id is null then  CONVERT(bit,0) when sl.IsLocked <> 1  then CONVERT(bit,0) else  CONVERT(bit,1) end   
									----,SPM.MonthNo,SPM.YearNo 
									,sl.IsLocked AS Lock
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName	
                                    , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end Flag
                                    ,DeG.UserName Designation,EC.UserName EmployeeCategory

                                  ,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
                                  ,IsDisburse = case when sl.IsDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end

	                                  ,(PMB.EntityId) EntityId
									,(PMB.PositionId) PositionId                                     
                                    --,(ld.UserName) Designation                                        
									,(DP.UserName) Department 
									,(ediv.UserName) Division 
									--,(EmpC.UserName) EmployeeCategory
									,(ep.UserName) Plant 
									,(Se.UserName) Section 
									,(SuS.UserName) SubSection 
									,(eu.UserName) Unit 
                                    ,(isnull(L.UserName,'')) Line

                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('"+ effectiveDate + @"') then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    --, Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
                                    --,ISNULL(SPM.Description,'') SalaryProcess
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                  ,ISNULL(jl.JobLocation, '') JobLocation
									----,ISNULL(spd.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
         --                           ,ISNULL(v.VoucherNo,'' ) VoucherNo
         --                           ,ISNULL(sl.PayableVoucherId,'') PayableVoucherId
         --                           ,ISNULL(sl.DisbursementVoucherId,'') DisbursementVoucherId
         --                           ,v.VoucherNo as PayableVoucherNo
         --                           ,vl.VoucherNo as DisbursementVoucherNo
                                    ,ISNULL(v.VoucherNo,'') as PayableVoucherNo
                                    ,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo
									,ISNULL(e.PaymentMode,'') PaymentMode
									, Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
                                    --,SPC.SalaryID as SalaryStructureId
                                    FROM EmployeeInformation e

                                    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                    LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id                        
                                    LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                                    LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = E.LegalDesignationId
                                    LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                                    left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
						            LEFT JOIN HKP.Designation DeG ON DeG.Id = dm.DesignationId
                                    left join HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                                    LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                                    LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                    LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                                   
                                    LEFT OUTER JOIN ORG.Division ediv on ediv.id=en.DivisionId
                                    LEFT OUTER JOIN ORG.SubDivision esdiv on esdiv.id=en.SubDivisionId
                                    LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN ORG.Unit eu on eu.id=en.UnitId

                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
                           			                                       
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId


                                    left join salaryprocesslogdetail spd ON spd.Id=(SELECT TOP 1 xx.Id
                                                      FROM salaryprocesslogdetail XX
                                        JOIN SalaryProcMaster YY ON yy.SystemID=xx.SalaryProcessId AND E.SystemId=XX.EmpSystemId
                                        AND  MonthNo =  Month('" + effectiveDate + @"') AND YearNo =  Year('" + effectiveDate + @"')
                                        )
                                      LEFT  JOIN SalaryProcMaster SPM ON SPM.SystemID = spd.SalaryProcessId
                                       Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [HKP].[Bank] bb on bb.Id = spd.BankSystemID
									Left join SalaryLock sl ON sl.Id=(SELECT TOP 1 id FROM SalaryLock slx where  slx.EmpSystemId=e.SystemId and slx.YearNo=Year('" + effectiveDate + @"') AND SLx.MonthNo=Month('" + effectiveDate + @"'))
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 

                                     WHERE E.GroupID='" + identity.CompanyGroupId+@"' --AND E.PlantId='"+identity.PlantId+ @"' 
									
                                    
									 and e.systemid in
									 (
									 SELECT c.EmpInfoSystemID FROM SalaryProcChild c 
									  inner join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID and MonthNo =  Month('" + effectiveDate + @"') AND YearNo =  Year('"+ effectiveDate + @"')
									  WHERE PlantID = '"+identity.PlantId+ @"'		
									 )
                                                                                     
                                     ) DD  " + wcEmpStatus + @"

									 
									 ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric
									";
            return sql;           
        }

        public string GetEmpInfoForSalaryDisbursement(CustomIdentity _identity, string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = _identity;
            bool sa = identity.IsSysAdmin;
            bool ca = identity.IsControlAdmin;
            string userId = identity.UserId;
            string plantId = identity.PlantId;
            string companyGroupId = identity.CompanyGroupId;

            var wcPayrollGroup = "";
            var wcSalaryProcess = "";
            var salaryProcessJoin = "";
            var salaryProcessColumn = "";
            var strDOJ = "";
            string salaryProcessFlag = "";
            string wcEmpStatus = " Where (1=0 ";

            if (sa == true || ca == true)
            {
                wcPayrollGroup = @"";
            }
            else
            {
                wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
            }
            if (salaryProcessId == "STRUCTURE")
            {
                salaryProcessColumn = "";
                salaryProcessJoin = "";
                wcSalaryProcess = "";
                strDOJ = "AND DOJ<='" + effectiveDate + @"' AND (DOS is null OR DOS>= '" + effectiveDate + @"')";


            }
            else if (!string.IsNullOrEmpty(salaryProcessId))
            {
                salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";

                salaryProcessJoin = @" LEFT  JOIN (
									  SELECT c.* FROM SalaryProcChild c 
									  inner join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID and MonthNo =  Month('" + effectiveDate + @"') AND YearNo =  Year('" + effectiveDate + @"')
									  WHERE PlantID = '" + plantId + @"'									  
									  ) SPC ON SPC.EmpInfoSystemID = E.SystemId
                                        LEFT  JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                                        left join salaryprocesslogdetail spd on spd.EmpSystemId=SPC.EmpInfoSystemID and spd.SalaryProcessId=spm.SystemID";


                wcSalaryProcess = @"AND SPC.SlrProcMstSystemID IN('" + salaryProcessId + @"')";

            }
            else if (string.IsNullOrEmpty(salaryProcessId) == true && salaryProcessId != "STRUCTURE")
            {
                salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";

                salaryProcessJoin = @" LEFT  JOIN (
									  SELECT c.* FROM SalaryProcChild c 
									  inner join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID and MonthNo =  Month('" + effectiveDate + @"') AND YearNo =  Year('" + effectiveDate + @"')
									  WHERE PlantID = '" + plantId + @"'									  
									  ) SPC ON SPC.EmpInfoSystemID = E.SystemId
                                        LEFT  JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                                        left join salaryprocesslogdetail spd on spd.EmpSystemId=SPC.EmpInfoSystemID and spd.SalaryProcessId=spm.SystemID";



            }
            if (salaryProcessId == "STRUCTURE")
            {
                wcEmpStatus = " Where (1=1 ";
                salaryProcessFlag = "";
            }
            else
            {
                salaryProcessFlag = ", Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag";
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
            }

            wcEmpStatus += ")";

            var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
            var param = string.Empty;
            if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
            else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                param = "E.GroupID='" + companyGroupId + "'";

            string sql = @"SELECT [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId 
	                                ,sl.Id,CheckBoxSelect=case when  sl.Id is null then  CONVERT(bit,0) when sl.IsLocked <> 1  then CONVERT(bit,0) else  CONVERT(bit,1) end   
									----,SPM.MonthNo,SPM.YearNo 
									,sl.IsLocked AS Lock
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName	
                                    , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end Flag
                                    ,DeG.UserName Designation,EC.UserName EmployeeCategory

                                  ,IsLock = case when sl.IsLocked = 1 then 'Locked' else 'Unlocked' end
                                  ,IsDisburse = case when sl.IsDisbursed = 1 then 'Disbursed' else 'Not Disbursed' end

	                                  ,(PMB.EntityId) EntityId
									,(PMB.PositionId) PositionId                                     
                                    --,(ld.UserName) Designation                                        
									,(DP.UserName) Department 
									,(ediv.UserName) Division 
									--,(EmpC.UserName) EmployeeCategory
									,(ep.UserName) Plant 
									,(Se.UserName) Section 
									,(SuS.UserName) SubSection 
									,(eu.UserName) Unit 
                                    ,(isnull(L.UserName,'')) Line

                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    --, Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
                                    --,ISNULL(SPM.Description,'') SalaryProcess
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                  ,ISNULL(jl.JobLocation, '') JobLocation
									----,ISNULL(spd.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
         --                           ,ISNULL(v.VoucherNo,'' ) VoucherNo
         --                           ,ISNULL(sl.PayableVoucherId,'') PayableVoucherId
         --                           ,ISNULL(sl.DisbursementVoucherId,'') DisbursementVoucherId
         --                           ,v.VoucherNo as PayableVoucherNo
         --                           ,vl.VoucherNo as DisbursementVoucherNo
                                    ,ISNULL(v.VoucherNo,'') as PayableVoucherNo
                                    ,ISNULL(vl.VoucherNo,'') as DisbursementVoucherNo
									,ISNULL(e.PaymentMode,'') PaymentMode
									, Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
                                    --,SPC.SalaryID as SalaryStructureId
                                    FROM EmployeeInformation e

                                    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                    LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id                        
                                    LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                                    LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = E.LegalDesignationId
                                    LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                                    left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
						            LEFT JOIN HKP.Designation DeG ON DeG.Id = dm.DesignationId
                                    left join HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                                    LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                                    LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                    LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                                   
                                    LEFT OUTER JOIN ORG.Division ediv on ediv.id=en.DivisionId
                                    LEFT OUTER JOIN ORG.SubDivision esdiv on esdiv.id=en.SubDivisionId
                                    LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN ORG.Unit eu on eu.id=en.UnitId

                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
                           			                                       
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId


                                    left join salaryprocesslogdetail spd ON spd.Id=(SELECT TOP 1 xx.Id
                                                      FROM salaryprocesslogdetail XX
                                        JOIN SalaryProcMaster YY ON yy.SystemID=xx.SalaryProcessId AND E.SystemId=XX.EmpSystemId
                                        AND  MonthNo =  Month('" + effectiveDate + @"') AND YearNo =  Year('" + effectiveDate + @"')
                                        )
                                      LEFT  JOIN SalaryProcMaster SPM ON SPM.SystemID = spd.SalaryProcessId
                                       Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [HKP].[Bank] bb on bb.Id = spd.BankSystemID
									Left join SalaryLock sl ON sl.Id=(SELECT TOP 1 id FROM SalaryLock slx where  slx.EmpSystemId=e.SystemId and slx.YearNo=Year('" + effectiveDate + @"') AND SLx.MonthNo=Month('" + effectiveDate + @"'))
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 

                                     WHERE E.GroupID='" + identity.CompanyGroupId + @"' --AND E.PlantId='" + identity.PlantId + @"' 
									
                                    
									 and e.systemid in
									 (
									 SELECT c.EmpInfoSystemID FROM SalaryProcChild c 
									  inner join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID and MonthNo =  Month('" + effectiveDate + @"') AND YearNo =  Year('" + effectiveDate + @"')
									  WHERE PlantID = '" + identity.PlantId + @"'		
									 )
                                                                                     
                                     ) DD  " + wcEmpStatus + @"

									 
									 ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric
									";
            return sql;
        }
    }
}
