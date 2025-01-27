using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
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
                                    CASE WHEN ISNULL(AB.EmpInfoSystemID,'')<>'' THEN 'YES' ELSE '' END AS IsAlreadyProcessed,
                                    FORMAT(emp.DOJ,'dd-MMM-yyyy') AS DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') AS DOS,EMP.EmployeeStatus,DIV.UserName AS Division,
                                    EMP.EmployeeName,EMP.EmployeeCode,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,concat( sl.YearNo,'/', sl.MonthNo) LastLocked
                                    ,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation, PR.UserName PositionName,format(SEFD.EffectiveDate,'dd-MMM-yyyy') AS LastSalaryEffectiveDate,
                                    DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection,PL.UserName Plant,SEFD.SalaryRuleMasterSystemID 
                                    ,srm.SalaryRuleName,srmLA.SalaryRuleName AS LastSalaryRuleName,format(LSA.EffectiveDate,'dd-MMM-yyyy') AS LatestSalaryEffectiveDate
                                     FROM  EmployeeInformation EMP 
                                        LEFT JOIN(
		                            	
		                            	SELECT * FROM (  SELECT  *,
				                            DENSE_RANK() OVER (PARTITION BY SDM.EmpInfoSystemID ORDER BY SDM.EffectiveDate DESC) AS RNK

			                                                from (
							                                                SELECT sdm.EmpInfoSystemID,SDM.SalaryRuleMasterSystemID ,sdm.EffectiveDate,sdm.IsApproved
							                                                   from SalaryInfoDefineMaster SDM
                                                                                WHERE SDM.IsApproved=1
								                                                union ALL
								                                                select sdm.EmpInfoSystemID,SDM.SalaryRuleMasterSystemID ,sdm.EffectiveDate,sdm.IsApproved
								                                                from SalaryInfoBackMaster SDM
                                                                                WHERE SDM.IsApproved=1
			                                                ) AS SDM
			
			                                        ) AS SDM 
                                                    WHERE ISNULL(sdm.IsApproved,'')=1 AND EffectiveDate <= '" + ToDate + @"' AND rnk=1 
		                                   ) SEFD ON SEFD.EmpInfoSystemID = EMP.SystemID

                                        Left JOIN (
		                            	
		                            	SELECT * FROM (  SELECT  *,
				                            DENSE_RANK() OVER (PARTITION BY SDM.EmpInfoSystemID ORDER BY SDM.EffectiveDate DESC) AS RNK

			                                                from (
							                                                SELECT sdm.EmpInfoSystemID,SDM.SalaryRuleMasterSystemID ,sdm.EffectiveDate,sdm.IsApproved
							                                                   from SalaryInfoDefineMaster SDM
                                                                               -- WHERE SDM.IsApproved=1
								                                                union ALL
								                                                select sdm.EmpInfoSystemID,SDM.SalaryRuleMasterSystemID ,sdm.EffectiveDate,sdm.IsApproved
								                                                from SalaryInfoBackMaster SDM
                                                                               -- WHERE SDM.IsApproved=1
			                                                ) AS SDM
			
			                                        ) AS SDM 
                                                    WHERE  EffectiveDate <= '" + ToDate + @"' AND rnk=1 --AND ISNULL(sdm.IsApproved,'')=1
		                                   ) LSA ON LSA.EmpInfoSystemID=SEFD.EmpInfoSystemID

                                       -- LEFT JOIN ArrearSummaryBatchWise AB ON ab.EmployeeSystemId=emp.SystemId AND ab.ArrearProcessBatchId IN 
			                           -- (SELECT ArrearProcessBatchId FROM ArrearProcMaster WHERE ('" + FromDate + @"' BETWEEN FromDate AND ToDate) OR  ('" + ToDate + @"' BETWEEN FromDate AND ToDate)OR  (FromDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"') OR  (ToDate  BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'))
			                         
                                         LEFT JOIN   (SELECT DISTINCT C.EmpInfoSystemID
			                                                                             FROM ArrearProcMaster M
			                                                                 JOIN ArrearProcChild AS C ON c.SlrProcMstSystemID=M.SystemID
			                                                                  WHERE ('" + FromDate + @"' BETWEEN ArrearProcessFromDate AND ArrearProcessToDate) OR  ('" + ToDate + @"' BETWEEN ArrearProcessFromDate AND ArrearProcessToDate)OR  (ArrearProcessFromDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"') OR  (ArrearProcessToDate  BETWEEN '" + FromDate + @"' AND '" + ToDate + @"')
			                                                              ) AS AB ON ab.EmpInfoSystemID=EMP.SystemId
			                         
			                            LEFT JOIN SalaryRuleMaster AS srm ON srm.SystemID=sefd.SalaryRuleMasterSystemID
			                            LEFT JOIN SalaryRuleMaster AS srmLA ON srmLA.SystemID=LSA.SalaryRuleMasterSystemID
			                             LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
										LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
										LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
										LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
										LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
										LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
										LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
										LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
										LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
			                            LEFT JOIN org.Division AS DIV ON DIV.Id=emp.DivisionId
			                            LEFT JOIN SalaryLock AS sl  ON sl.EmpSystemId=emp.SystemId AND sl.Id=(SELECT TOP 1 Id FROM salaryLock xl where  xl.IsLocked=1 and xl.EmpSystemId=emp.SystemId ORDER BY xl.YearNo DESC,xl.MonthNo DESC)
			                              WHERE EMP.DOJ <= '" + ToDate + @"'
                                           AND EMP.EmployeeStatus='Active'
			                               AND (EMP.DOS >= '" + FromDate + @"' OR ISNULL(EMP.DOS,'') = '' OR EMP.DOS = '01/01/1901')
                                           AND emp.PlantId='" + PlantId + @"'
			                          ";

                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<Dictionary<string, object>> GetEmployeeForApproval(string BatchSystemId)
        {

            try
            {

                string sql = @"SELECT 
                                CONVERT(BIT,ISNULL(b.IsApproved,0)) AS IsApproved,B.Diff AS ArrearAmount,
                                 [CheckBoxSelect] = Convert(bit, 'False'),[isToBeSelect] = Convert(bit, 'false'),EMP.SystemID AS EmpSystemID,
                                FORMAT(emp.DOJ,'dd-MMM-yyyy') AS DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') AS DOS,EMP.EmployeeStatus,DIV.UserName AS Division,
                                EMP.EmployeeName,EMP.EmployeeCode,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric
                                ,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation, PR.UserName PositionName,
                                DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection,PL.UserName Plant
                                  

                                FROM ArrearSummaryBatchWise AS B
                                JOIN ArrearProcMaster AS apm ON apm.ArrearProcessBatchId=b.ArrearProcessBatchId AND apm.SystemID=(SELECT TOP 1 SystemId FROM ArrearProcMaster AS X WHERE x.ArrearProcessBatchId=b.ArrearProcessBatchId)
                                JOIN EmployeeInformation AS EMP ON EMP.SystemId=b.EmployeeSystemId

                                LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                LEFT JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                                LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                LEFT JOIN org.Division AS DIV ON DIV.Id=PR.DivisionId

                                WHERE B.ArrearProcessBatchId='" + BatchSystemId + @"' ";

                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }



        public void ApprovelUnapprove(List<string> data, string ArrearProcessBatchId, bool isApprove)
        {

            try
            {
                string employeeIds = "''";
                for (int i = 0; i < data.Count; i++)
                    employeeIds += ",'" + data[i] + @"'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.getDataSet("select * from ArrearSummaryBatchWise M where ArrearProcessBatchId='" + ArrearProcessBatchId + @"' AND EmployeeSystemId IN (" + employeeIds + @")", out DataSet dsArrearSummaryBatchWise);

                con.getDataSet(@"SELECT am.ArrearProcessBatchId, ac.EmpInfoSystemID,sh.SalaryHead,ac.SalaryHeadID,sh.TransactionTypeNew,
                                    mb.Id AS ManpowerBudgetId, mb.EntityId, mb.PositionId,mb.AccountsGroupId,e.PlantId,
                                    e.ThirdPartyBusinessArea, e.ThirdPartyProfitCenter,ecc.CostCenterId,

                                    SUM(ABS(AC.Diff)) AS Amount
                                   
		                                    FROM ArrearProcMaster AS AM
		                                    JOIN ArrearProcChild AS AC ON am.SystemID=ac.SlrProcMstSystemID
		                                    LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=ac.EmpInfoSystemID
		                                    LEFT JOIN mst.ManpowerBudget AS mb ON mb.Id=ei.BudgetCode 
		                                    LEFT JOIN [MST].[SalaryHeadGL] SGL ON sgl.SalaryHeadId=ac.SalaryHeadID AND sgl.AccountsGroupId=mb.AccountsGroupId
		                                     JOIN (
        		                                    SELECT *,CASE WHEN ISNULL(sh.TransactionType,'')='Both' THEN 'Dr.' ELSE sh.TransactionType END AS TransactionTypeNew
        		                                    FROM SalaryHead AS sh WHERE  ISNULL(sh.TransactionType,'') IN ('Dr.','Both') 
					                                    UNION ALL 
				                                    SELECT *,CASE WHEN ISNULL(sh.TransactionType,'')='Both' THEN 'Cr.' ELSE sh.TransactionType END AS TransactionTypeNew
        		                                    FROM SalaryHead AS sh WHERE  ISNULL(sh.TransactionType,'') IN ('Cr.','Both') 
                                                ) AS sh ON sh.SalaryHeadID=ac.SalaryHeadId    
                                   
		                                    LEFT JOIN org.Entity AS e ON e.Id=mb.EntityId
		                                    LEFT JOIN org.EntityCostCenter AS ecc ON ecc.EntityId=e.Id

                                    WHERE am.ArrearProcessBatchId='" + ArrearProcessBatchId + @"' and AC.EmpInfoSystemID IN (" + employeeIds + @") --AND ISNULL(sh.HeadCategory,'')<>'Net Payable'
                                    AND ISNULL(sh.TransactionType,'') IN ('Dr.','Cr.','Both')
                                    GROUP BY  am.ArrearProcessBatchId, ac.EmpInfoSystemID,sh.SalaryHead,ac.SalaryHeadID,sh.TransactionTypeNew,
                                    mb.Id, mb.EntityId, mb.PositionId, mb.AccountsGroupId,e.PlantId,
                                    e.ThirdPartyBusinessArea, e.ThirdPartyProfitCenter,ecc.CostCenterId", out DataSet dsArrearAccountsSourceData);


                con.getDataSet("select * from ArrearAccountsData M where ArrearProcessBatchId='" + ArrearProcessBatchId + @"' AND EmpInfoSystemID IN (" + employeeIds + @")", out DataSet dsArrearAccountsData);

                con.CommitTransaction();

                for (int i = 0; i < dsArrearSummaryBatchWise.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = dsArrearSummaryBatchWise.Tables[0].Rows[i];
                    dr.BeginEdit();

                    if (isApprove == true)
                    {
                        dr["IsApproved"] = true;

                        dsArrearAccountsSourceData.Tables[0].DefaultView.RowFilter = "EmpInfoSystemID='" + dr["EmployeeSystemId"].ToString() + @"'";

                        dsArrearAccountsData.Tables[0].DefaultView.RowFilter = "EmpInfoSystemID='" + dr["EmployeeSystemId"].ToString() + @"'";
                        while (dsArrearAccountsData.Tables[0].DefaultView.Count > 0)
                            dsArrearAccountsData.Tables[0].DefaultView[0].Delete();

                        for (int K = 0; K < dsArrearAccountsSourceData.Tables[0].DefaultView.Count; K++)
                        {
                            DataRow drSource = dsArrearAccountsSourceData.Tables[0].DefaultView[K].Row;
                            DataRow drDestination = dsArrearAccountsData.Tables[0].NewRow();

                            drDestination["ArrearProcessBatchId"] = drSource["ArrearProcessBatchId"];
                            drDestination["EmpInfoSystemID"] = drSource["EmpInfoSystemID"];
                            drDestination["SalaryHeadID"] = drSource["SalaryHeadID"];
                            drDestination["ManpowerBudgetId"] = drSource["ManpowerBudgetId"];
                            drDestination["EntityId"] = drSource["EntityId"];
                            drDestination["PositionId"] = drSource["PositionId"];
                            drDestination["AccountsGroupId"] = drSource["AccountsGroupId"];
                            drDestination["PlantId"] = drSource["PlantId"];
                            drDestination["ThirdPartyBusinessArea"] = drSource["ThirdPartyBusinessArea"];
                            drDestination["ThirdPartyProfitCenter"] = drSource["ThirdPartyProfitCenter"];
                            drDestination["CostCenterId"] = drSource["CostCenterId"];
                            drDestination["TransactionType"] = drSource["TransactionTypeNew"];
                            drDestination["Amount"] = drSource["Amount"];
                            drDestination["AddedBy"] = identity.Name;
                            drDestination["AddedDate"] = System.DateTime.Now.ToString();
                            drDestination["AddedFromIP"] = identity.IPAddress;
                            drDestination["UpdatedBy"] = identity.Name;
                            drDestination["UpdatedDate"] = System.DateTime.Now.ToString();
                            drDestination["UpdatedFromIP"] = identity.IPAddress;

                            dsArrearAccountsData.Tables[0].Rows.Add(drDestination);
                        }

                    }
                    else
                    {
                        dr["IsApproved"] = false;


                        dsArrearAccountsData.Tables[0].DefaultView.RowFilter = "EmpInfoSystemID='" + dr["EmployeeSystemId"].ToString() + @"'";
                        while (dsArrearAccountsData.Tables[0].DefaultView.Count > 0)
                            dsArrearAccountsData.Tables[0].DefaultView[0].Delete();

                    }
                    dr.EndEdit();
                }




                OTSBD.clsStaticInfo info = new OTSBD.clsStaticInfo();
                info.SaveDataSets(dsArrearSummaryBatchWise, dsArrearAccountsData);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public void DeleteEmployeeArrear(string ArrearProcessBatchId, string EmployeeSystemId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery("DELETE FROM ArrearAccountsData where ArrearProcessBatchId='" + ArrearProcessBatchId + @"' AND EmpInfoSystemID='" + EmployeeSystemId + @"'");

                con.executeQuery(@"DELETE FROM ArrearProcChild WHERE SystemID IN (
                                                SELECT C.SystemID FROM ArrearProcChild AS C
                                                JOIN ArrearProcMaster AS M ON m.SystemID=c.SlrProcMstSystemID
                                                WHERE M.ArrearProcessBatchId='" + ArrearProcessBatchId + @"' AND c.EmpInfoSystemID='" + EmployeeSystemId + @"'
                                                )");
                con.executeQuery("DELETE FROM ArrearSummaryMonthWise where ArrearProcessBatchId='" + ArrearProcessBatchId + @"' AND EmployeeSystemId='" + EmployeeSystemId + @"'");
                con.executeQuery("DELETE FROM ArrearSummaryBatchWise where isnull(IsApproved,0)=0 AND ArrearProcessBatchId='" + ArrearProcessBatchId + @"' AND EmployeeSystemId='" + EmployeeSystemId + @"'");

                con.executeQuery(@"DELETE FROM ArrearProcMaster WHERE SystemID IN (
                                    SELECT APM.SystemID FROM ArrearProcMaster AS apm
                                    LEFT JOIN ArrearProcChild AS apc ON apm.SystemID=apc.SlrProcMstSystemID AND apc.SystemID=(SELECT TOP 1 SystemId FROM ArrearProcChild AS apc2 WHERE apc2.SlrProcMstSystemID=apm.SystemID)
                                    WHERE ISNULL(apc.SystemID,'')=''
                                    )");
                con.executeQuery(@"DELETE FROM ArrearProcessBatch WHERE Id IN (
                                    SELECT APM.Id FROM ArrearProcessBatch AS apm
                                    LEFT JOIN ArrearProcMaster AS apc ON apm.Id=apc.ArrearProcessBatchId 
                                    AND apc.SystemID=(SELECT TOP 1 apc2.SystemID FROM ArrearProcMaster AS apc2 WHERE apc2.ArrearProcessBatchId=apm.Id)
                                    WHERE ISNULL(apc.SystemID,'')=''
                                    )");

                con.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }
        public IWorkbook GetArrearFinancialData(string ArrearProcessBatchId, List<string> EmployeeIds)
        {

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {
                string empids = "''";
                foreach (var item in EmployeeIds)
                    empids += ",'" + item + @"'";



                string sql = @"SELECT ag.UserName AS EmployeeCategory,  ei.EmployeeCode, 
                                    sh.HeadCategory,sh.SalaryHead,
                                    CASE WHEN ac.TransactionType='Dr.' THEN sgl.DrDirectOtherGLCode
                                    ELSE sgl.CrDirectOtherGLCode END AS GLCode,

                                    CASE WHEN ac.TransactionType='Dr.' THEN sgl.DrDirectOtherGL
                                    ELSE sgl.CrDirectOtherGL END AS GLName,
                                    p.UserName AS Plant,ac.TransactionType,ac.Amount

                                    FROM ArrearAccountsData AS ac
	                                    LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=ac.EmpInfoSystemID
                                    LEFT JOIN mst.ManpowerBudget AS mb ON mb.Id=ac.ManpowerBudgetId 
                                    LEFT JOIN AccountsGroup AS ag ON ag.Id=mb.AccountsGroupId
                                    LEFT JOIN [MST].[SalaryHeadGL] SGL ON sgl.SalaryHeadId=ac.SalaryHeadID AND sgl.AccountsGroupId=mb.AccountsGroupId

                                    JOIN SalaryHead AS sh ON sh.SalaryHeadID=ac.SalaryHeadId                       
                                    LEFT JOIN org.Entity AS e ON e.Id=ac.EntityId
                                    LEFT JOIN org.Company AS c ON c.Id=e.CompanyId
                                    LEFT JOIN org.Plant AS p ON p.Id=e.PlantId
                                    LEFT JOIN org.EntityCostCenter AS ecc ON ecc.EntityId=e.Id
                                    WHERE ac.ArrearProcessBatchId='" + ArrearProcessBatchId + @"' AND ac.EmpInfoSystemID IN (" + empids + @")";

                DataTable dtArrearFinanceData = _sqlRepository.GetDataTable(sql);
                if (dtArrearFinanceData.Rows.Count == 0)
                    throw new Exception("No data found");

                DataTable dtArrear = _sqlRepository.GetDataTable("SELECT * FROM ArrearProcessBatch AS apb WHERE apb.Id='" + ArrearProcessBatchId + @"'");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[0].Name = "Arrear";
                sheet = workbook.Worksheets[0];



                int ROW = 1; int COL = 1;
                sheet[ROW, 1].Text = "Arrear Report";
                sheet[ROW, 1].CellStyle.Font.Size = 12;
                sheet[ROW, 1].RowHeight = 22;
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, 10].Merge();
                ROW++;
                sheet[ROW, 1].Text = "Desc :" + dtArrear.Rows[0]["ArrearDesc"].ToString();
                sheet[ROW, 1].CellStyle.Font.Size = 12;
                sheet[ROW, 1].RowHeight = 16;
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, 10].Merge();
                ROW++;
                sheet[ROW, 1].Text = "From :" + Convert.ToDateTime(dtArrear.Rows[0]["ArrearFromDate"].ToString()).ToString("MMM/yyyy") + " To :" + Convert.ToDateTime(dtArrear.Rows[0]["ArrearToDate"].ToString()).ToString("MMM/yyyy");
                sheet[ROW, 1].CellStyle.Font.Size = 12;
                sheet[ROW, 1].RowHeight = 16;
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, 10].Merge();
                ROW++;

                ROW += 2;


                sheet[ROW, COL].Text = "Employee Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEmployeeCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 12;
                int colEmployeeCode = COL;
                COL++;
                sheet[ROW, COL].Text = "Head Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int colHeadCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Salary Head";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalaryHead = COL;
                COL++;
                sheet[ROW, COL].Text = "GL Code";
                sheet[ROW, COL].ColumnWidth = 12;
                int colGLCode = COL;
                COL++;
                sheet[ROW, COL].Text = "GL Name";
                sheet[ROW, COL].ColumnWidth = 20;
                int colGLName = COL;
                COL++;
                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Tran. Type";
                sheet[ROW, COL].ColumnWidth = 8;
                int colTransactionType = COL;
                COL++;
                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAmount = COL;


                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 10f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dtArrearFinanceData.Rows.Count; i++)
                {
                    sheet[ROW, colEmployeeCategory].Text = dtArrearFinanceData.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, colEmployeeCode].Text = dtArrearFinanceData.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, colHeadCategory].Text = dtArrearFinanceData.Rows[i]["HeadCategory"].ToString();
                    sheet[ROW, colSalaryHead].Text = dtArrearFinanceData.Rows[i]["SalaryHead"].ToString();
                    sheet[ROW, colGLCode].Text = dtArrearFinanceData.Rows[i]["GLCode"].ToString();
                    sheet[ROW, colGLName].Text = dtArrearFinanceData.Rows[i]["GLName"].ToString();
                    sheet[ROW, colPlant].Text = dtArrearFinanceData.Rows[i]["Plant"].ToString();
                    sheet[ROW, colTransactionType].Text = dtArrearFinanceData.Rows[i]["TransactionType"].ToString();
                    sheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtArrearFinanceData.Rows[i]["Amount"].ToString());



                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }



                sheet.UsedRange.NumberFormat = "#,##0;[Red](#,##0)";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;



                sheet.IsDisplayZeros = false;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.PrintTitleRows = "$1:$5";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;
                workbook.Version = ExcelVersion.Excel2016;

            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return workbook;

        }

    }
}
