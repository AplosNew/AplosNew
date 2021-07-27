using ConnectionManager;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Attendance
{
    public class clsAuditReportSummery
    {
        ISqlRepository _sqlRepository;
        public clsAuditReportSummery()
        {
            _sqlRepository = new SqlRepository();
        }
        public void GetPlant(string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"select p.Id,p.UserName PlantName,c.UserName Company
                            From org.Company c
                            left join org.Plant p on p.CompanyId = c.Id
                            where c.Id='" + companyId + @"' 
                            order by p.Sequence";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        public void SelectedPlantWiseCompany(string sID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT c.UserName CompanyName ,ISNULL(cm.Address1,'')+','+ ISNULL(cm.Address2,'') Address1, cm.Phone,cm.Email
                                ,cm.Address1 cAddress1 ,cm.Address2 cAddress2,c.[Image] CompanyImage
                                FROM org.Plant p
							LEFT OUTER JOIN org.Company c on c.Id=p.CompanyId
							LEFT OUTER JOIN mst.AddressMaster cm on cm.Id=c.AddressMasterId
							WHERE c.Id='" + sID + "'";

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
        }//end of function
        public void GetManualOutTimeForOTDateWiseReport(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId

                        FROM AttdnProcessData AP
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                         WHERE 
                        	 AP.IsOTEntitled = 1
                        	AND AP.IsManualOutTime = 1
                        	AND isnull (AP.OTHr,0) > 0
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + FromDate + @"'
                        	and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric
                             ,AP.WorkDate ";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetModifiedReport(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120); ;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId                           
                        FROM AttdnProcessData AP
                        
                        LEFT JOIN FinalOT OTF ON AP.EmpSystemID = OTF.EmpSystemID
                        	AND AP.WorkDate = OTF.WorkDate
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                                WHERE 
                        	    AP.IsOTEntitled = 1
                        	AND AP.IsOTComfirm = 1
                        	AND (isnull(AP.OTHr,0) <> isnull(OTF.TotalOTHr,0))
                        	AND AP.WorkDate = '" + FromDate + @"'
                        	and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                           
                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric
                                  ,AP.WorkDate";

                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetAbsentReports(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            string fd = "01-" + Convert.ToDateTime(FromDate).ToString("MMM") + "-" + Convert.ToDateTime(FromDate).ToString("yyyy");
            string endDate = Convert.ToDateTime(fd).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId
                        FROM AttdnProcessData AP
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        WHERE 
                               AP.DayStatus ='A'
                        	And AP.InTime IS NULL
                        	AND AP.OutTime IS NULL
                            and isnull(ei.EmployeeCurrentStatus,'') not in('TBS','LONG ABSENTEEISM')
                        	AND AP.WorkDate = '" + FromDate + @"'
                        	and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                            
                        ORDER BY AP.WorkDate
                        	,EmployeeCodePreFix,EmployeeCodeNumeric";

                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetAbsentWithPunchReports(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId

                        FROM AttdnProcessData AP
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        WHERE  
                                AP.DayStatus ='A'  
                        	AND AP.WorkDate = '" + FromDate + @"' 
                           and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'                               
							and (---1
							( AP.InTime IS NULL	AND AP.OutTime IS not NULL)
							or 
							( AP.InTime IS not NULL	AND AP.OutTime IS NULL)							
							)----1                        		
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric
                               ,AP.WorkDate";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetShortDurationAbsentReports(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId                            
                            ,EI.EmployeeCode
                        FROM AttdnProcessData AP
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId WHERE  
                                AP.DayStatus ='A'  
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + FromDate + @"'   
                           and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'                             
							and  AP.InTime IS not NULL	AND AP.OutTime IS not  NULL								
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric
                               ,AP.WorkDate";
                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetLeaveWithPunchReports(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                string fd = "01-" + Convert.ToDateTime(FromDate).ToString("MMM") + "-" + Convert.ToDateTime(FromDate).ToString("yyyy");
                string endDate = Convert.ToDateTime(fd).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId
                        FROM AttdnProcessData AP
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId   
                            WHERE 
                              AP.DayStatus in (select daytype from daytype where category='Leave') 
                                and AP.IsHalfDayLeave = 0
                        	AND AP.WorkDate between '" + fd + @"' and  '" + endDate + @"'
                           and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                                
                             and (---1
							( AP.InTime IS NULL	AND AP.OutTime IS not NULL)
							or 
							( AP.InTime IS not NULL	AND AP.OutTime IS NULL)
							or
							( AP.InTime IS not NULL	AND AP.OutTime IS not NULL)
							)----1";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }

        public void GetOTEntitledWithOutMissingReports(string FromDate,  string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId
                        FROM AttdnProcessData AP                        
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus WHERE 
                            DT.Category IN ('Present','Late')
                            --and ISNULL(rd.LogDownLoadNum,'')<>''
                        	and AP.InTime IS NOT NULL                        	
                        	And AP.OutTime IS NULL  
                            AND  AP.IsOTEntitled = 1
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + FromDate + @"'
                        	and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'	
                             
                        ORDER BY 
                                EmployeeCodePreFix,EmployeeCodeNumeric
                                    ,AP.WorkDate";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetOTNotEntitledWithOutMissingReports(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId
                        FROM AttdnProcessData AP
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus WHERE 
                        	  DT.Category IN ('Present','Late')
                            --and ISNULL(rd.LogDownLoadNum,'')<>''
                        	And AP.InTime IS NOT NULL                        	
                        	And AP.OutTime IS NULL  
                            AND  AP.IsOTEntitled = 0
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + FromDate + @"'
                        	and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'	                           
                        ORDER BY 
                               EmployeeCodePreFix,EmployeeCodeNumeric
                                    ,AP.WorkDate";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetUNApprovedProfile(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {

                strSql = @"SELECT EI.DOJ,EI.SystemId
                        FROM EmployeeInformation EI WHERE EI.EmployeeStatus = 'Active'                        
                        	and EI.IsApproved=0 
                        	and EI.DOJ <= '" + FromDate + @"' 
                    and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'                   
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric";

                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function 

        public void GetProfileNoSalary(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT EI.EmployeeCode
                        FROM EmployeeInformation EI
                        WHERE EI.EmployeeStatus='Active' and EI.SystemId NOT IN (
                        SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster 
                        UNION 
                        SELECT EmpInfoSystemID FROM SalaryInfoBackMaster 
                        )                          
                        and EI.DOJ <= '" + FromDate + @"'  
                 and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                 and ei.DOJ<='" + FromDate + @"' AND (ei.DOS is null OR ei.DOS>= '" + FromDate + @"')
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric";
                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function 

        public void GetNoSalaryStructureApprove(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT ei.SystemId
                        FROM EmployeeInformation EI
                        WHERE EI.EmployeeStatus = 'Active' AND EI.SystemId  IN (
                             SELECT EmpInfoSystemID FROM (                        
                        SELECT MAX(EffectiveDate) EffectiveDate,EmpInfoSystemID,IsApproved FROM (                        
                        SELECT  EffectiveDate,EmpInfoSystemID,IsApproved FROM SalaryInfoDefineMaster  WHERE EffectiveDate<='" + FromDate + @"' 
                        union
                        SELECT  EffectiveDate,EmpInfoSystemID,IsApproved FROM SalaryInfoBackMaster  WHERE  EffectiveDate<='" + FromDate + @"'
                        ) x GROUP BY EmpInfoSystemID,IsApproved 
                        ) r WHERE IsApproved=0
                        ) 
                        and EI.DOJ <= '" + FromDate + @"'
                  and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'                        
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function 
        public void GetWorkDurationSheet(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT datediff(minute,KK.intime ,KK.outtime ) WorkDuration,
                            datediff(minute,KK.ShiftInTime ,CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END )	ShiftDuration
                             FROM (								
		                            SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime, 
                                    emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,
		                            O.PunchInTime,O.PunchOutTime,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,
		                            O.IsOTEntitled
									,fo.TotalOTHr ,amd.UpdatedBy,amd.DateUpdated,o.IsManualDayStatus ,emp.BudgetCode,emp.GivenDesignationId
		                            FROM EmployeeInformation EMP
		                            inner join AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
		                            LEFT JOIN FinalOT AS fo  ON EMP.SystemID=fo.EmpSystemID AND fo.WorkDate=o.WorkDate
		                            LEFT JOIN AttdnManualData AS amd   ON EMP.SystemID=amd.EmpSystemID AND amd.WorkDate=o.WorkDate
		                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID                       
                            WHERE o.WorkDate BETWEEN '" + FromDate + @"' AND '" + FromDate + @"' and o.IsHalfDayLeave <> 1
                        ) AS KK
						LEFT OUTER JOIN EmployeeInformation EI ON KK.Id=EI.SystemID  
						where 
						datediff(minute,KK.InTime ,KK.OutTime )<datediff(minute,KK.ShiftInTime ,CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END )	
                              and
							  EI.PlantId='" + plantId + @"' and EI.CompanyId='" + companyId + @"' and EI.GroupID='" + companyGroupId + @"'
                             and DayStatus in (select DayType from DayType where Category in ('Present', 'Late'))
                        ORDER BY CONVERT(DATE, WorkDate),kk.EmployeeCode ASC";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function 
        public void GetOtNotConfirmOverstayReport(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId

                        FROM AttdnProcessData AP
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId                        
                        left join OTfromApp oa on oa.EmpSystemId = AP.EmpSystemID and oa.WorkDate=AP.WorkDate
                        WHERE 
                                AP.DayStatus in (select daytype from daytype where category='Present' OR  category='Late')
                        	AND AP.IsOTEntitled = 1
                        	AND AP.IsOTComfirm = 0 and ISNULL (oa.OThour,0)=0 
                            and ap.OTHr >0
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + FromDate + @"'
                        	and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'	
                              and ei.DOJ<='" + FromDate + @"' AND (ei.DOS is null OR ei.DOS>= '" + FromDate + @"')
                        ORDER BY AP.WorkDate
                        	,EmployeeCodePreFix,EmployeeCodeNumeric";

                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        public void GetLongAbsentisom(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT ei.PlantId
                        	,ei.EmployeeCurrentStatus,EI.CellPhnNo TelePhnNo
                            ,EI.EmployeeCode

                        FROM EmployeeInformation EI
                        left join (select * from AttdnProcessData where WorkDate = '" + FromDate + @"' ) AP ON AP.EmpSystemID = EI.SystemId                        
                        WHERE 
                         ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                                 and ei.DOJ<='" + FromDate + @"' AND (ei.DOS is null OR ei.DOS>= '" + FromDate + @"')
                            AND isnull(EI.EmployeeCurrentStatus,'')='LONG ABSENTEEISM' 
                        
                        ORDER BY
                        	EmployeeCodePreFix,EmployeeCodeNumeric,ap.WorkDate";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        public void GetTBS(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT
                format(ap.WorkDate,'dd-MMM-yyy') WorkDate ,ei.SystemId

                FROM EmployeeInformation EI
                left join AttdnProcessData AP ON AP.EmpSystemID = EI.SystemId
                WHERE
                AP.WorkDate='" + FromDate + @"'
                and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                and ei.DOJ<='" + FromDate + @"' AND (ei.DOS is null OR ei.DOS>= '" + FromDate + @"')
                AND EI.EmployeeCurrentStatus='TBS'
                
                ORDER BY
                EmployeeCodePreFix,EmployeeCodeNumeric,ap.WorkDate";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        public void GetMaternityLeave(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @" select format(APD.WorkDate,'dd-MMM-yyyy')WorkDate,ei.SystemId
                             from  dbo.AttdnProcessData APD 
                             left join EmployeeInformation ei on ei.SystemId=APD.EmpSystemID
                              left join [dbo].[LeaveType] lET on  lET.Id = APD.LTSystemId
                             where let.LeaveType='Maternity'
                             AND APD.WorkDate between '" + FromDate + @"' and  '" + FromDate + @"'   
                             and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                            
                             order by EmployeeCodePreFix,EmployeeCodeNumeric,APD.WorkDate";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        public void GetBankRemark(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"    select distinct EI.systemid,EI.SystemId
							
                            from EmployeeInformation EI
                        
                            left join EmployeeBankInfo b on ei.SystemId=b.EmpSystemID
                            where 
	                    	 EI.DOJ<='" + FromDate + @"' AND (EI.DOS is null OR EI.DOS>= '" + FromDate + @"') 
                         and EI.PlantId='" + plantId + @"' AND EI.CompanyId='" + companyId + @"' and EI.GroupID='" + companyGroupId + @"'
                         and EI.DOJ<='" + FromDate + @"' AND (EI.DOS is null OR EI.DOS>= '" + FromDate + @"')
                            and                          
                            (--plant
                            (isnull(EI.PaymentMode,'')='Bank' and ISNULL(b.BankAccNo,'')='') 
                            or (isnull(EI.PaymentMode,'')='Cash' and ISNULL(b.BankAccNo,'')<>'') 
                            or (isnull(EI.PaymentMode,'')='Transfer' and ISNULL(b.BankAccNo,'')='') 
                            --or (isnull(EI.PaymentMode,'')='Bank' and ISNULL(b.BankAccNo,'')<>'' 
                            or b.IsApproved=0)--plant 
                            ";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
        public void GetAttendanceNotLockIndividual(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT    Format(apd.WorkDate,'dd-MMM-yyyy')   WorkDate         
                        ,ei.SystemId

                        FROM ExceptionEmployeeAttendanceUnlock  apd
						left join EmployeeInformation as EI on apd.EmpSystemId=EI.SystemId 
                        WHERE                                                     
					 apd.WorkDate between '" + FromDate + @"' and '" + FromDate + @"' 
                        and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'                         
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetAttendanceNotLockPlant(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @" SELECT format(LockedDate ,'dd-MMM-yyyy')  LockedDates ,AddedBy,format(AddedDate,'dd-MMM-yyyy')   AddedDate          
                        FROM [dbo].[PlantWiseAttendanceLock]   apd												                      
                        WHERE                      
						 LockedDate between '" + FromDate + @"' and '" + FromDate + @"' and PlantId='" + plantId + @"' and IsActive=0                                      
                        ORDER BY 
                        	LockedDate";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetTotalAbsent(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"    SELECT EmpSystemID ,EmployeeCode
								,sum(TotalAbsent)as TotalAbsent 
                                FROM(
								SELECT EmpSystemID, EmployeeCode,DayStatus,   								                        
                                TotalAbsent = CASE WHEN Category = 'Absent' and LTSystemID is null THEN 1
                                WHEN Category = 'Absent' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                                WHEN Category = 'Absent' and LTSystemID is not null and LeaveDuration=1 THEN 1
                                WHEN Category = 'Half Day' and LTSystemID is null THEN 0.5
                                ELSE 0 END      							                        
                                FROM dbo.AttdnProcessData a
                                left join daytype p on a.DayStatus=p.DayType
                                left join employeeInformation ei on ei.SystemId =a.EmpSystemID 							
                                WHERE  
								 WorkDate = '" + FromDate + @"' 
								 AND ei.PlantId= '" + plantId + @"' AND EI.CompanyId='" + companyId + @"'
                                ) A  
								 group by EmployeeCode,EmpSystemID";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetLongAtbsPlantSetting(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"
                            select LongTermAbesnteeism,TBSDays
                            ,IsLongAbsenteeismAuto=case when IsLongAbsenteeismAuto=1 then 'Yes' else 'Manual' end
                            ,IsTBSAuto=case when IsTBSAuto=1 then 'Yes' else 'Manual' end 
                             from [dbo].[PlantWiseHRMSSetting] where PlantID='" + plantId + @"' and GroupID='" + companyGroupId + @"'";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetNotInLegalDesignationMaster(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            try
            {
                strSql = @" select EI.EmployeeCode,ei.EmployeeName,ld.UserName as LegalDesignation,EI.LegalDesignationId
                            From EmployeeInformation as EI
                            left join [HKP].[LegalDesignation] ld on ld.Id=EI.LegalDesignationId
                            where EI.LegalDesignationId NOT IN (SELECT LegalDesignationId FROM [MST].[DesignationMasterLegalDesignation])
									and EI.PlantId='" + plantId + "' and (ei.DOJ<='" + FromDate + @"' and (ei.dos is null or ei.DOS >= '" + FromDate + @"'))  ";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetSalaryNotApproved(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            DateTime NewMonth;
            string otFutureDate = Convert.ToDateTime(FromDate).ToString("dd-MMM-yyyy");
            NewMonth = Convert.ToDateTime(otFutureDate).AddMonths(-1);
            try
            {
                strSql = @" select EI.SystemId
                            
                            from (select distinct spc.EmpInfoSystemID,spc.SlrProcMstSystemID,spm.YearNo,spm.MonthNo from SalaryProcChild spc
							left join SalaryProcMaster spm on spm.SystemID=spc.SlrProcMstSystemID
							) c
                            inner join SalaryLock sl on sl.EmpSystemId=c.EmpInfoSystemID and sl.MonthNo=c.MonthNo and sl.YearNo=c.YearNo and isnull(IsLocked,0)=1
                            LEFT JOIN EmployeeInformation EI ON c.EmpInfoSystemID = EI.SystemId
                            where isnull(sl.EmpSystemId,'')='' AND EI.PlantId='" + plantId + @"' AND EI.CompanyId='" + companyId + @"'
                            order by ei.EmployeeCodePreFix,ei.EmployeeCodeNumeric  ";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetSeparatedAbsent(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"    SELECT 0 AS Active
                              	,ei.SystemId AS Id
                              ";
                strSql += columnName();
                strSql += @"
                              	,D.DayStatus
                              	,COUNT(d.DayStatus) AS AbsentCount
                              	,ab.AbsentDays
                              	,format(ab.FirstAbsentDate,'dd-MMM-yyyy') FirstAbsentDate
                              	,format(ei.DOS,'dd-MMM-yyy') DOS
                              FROM EmployeeInformation AS EI
                              INNER JOIN (
                              	SELECT p.EmpSystemID
                              		,p.WorkDate
                              		,p.DayStatus
                              		,dense_rank() OVER (
                              			PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC
                              			) AS SEQ
                              	FROM EmployeeInformation AS E
                              	INNER JOIN AttdnProcessData AS P ON e.SystemId = p.EmpSystemID
                              		AND p.WorkDate <= e.DOS
                              	WHERE p.DayStatus NOT IN (
                              			SELECT DISTINCT DayType
                              			FROM DayType
                              			WHERE Category IN (
                              					'Holiday'
                              					,'Weekend'
                              					)
                              			)
                              	) AS D ON EI.SystemId = d.EmpSystemID";
                strSql += tableName();

                strSql += @"LEFT OUTER JOIN (
                              	SELECT K.EmpSystemID
                              		,COUNT(*) AbsentDays
                              		,MIN(k.WorkDate) AS FirstAbsentDate
                              	FROM (
                              		SELECT *
                              			,RANK() OVER (
                              				PARTITION BY EmpSystemID
                              				,dayStatustemp ORDER BY EmpSystemID
                              					,seq
                              				) AS SQ
                              		FROM (
                              			SELECT p.EmpSystemID
                              				,p.WorkDate
                              				,p.DayStatus
                              				,CASE 
                              					WHEN daystatus IN (
                              							SELECT DISTINCT DayType
                              							FROM DayType
                              							WHERE Category IN (
                              									'Holiday'
                              									,'Weekend'
                              									)
                              							)
                              						THEN 'A'
                              					ELSE daystatus
                              					END AS dayStatustemp
                              				,dense_rank() OVER (
                              					PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC
                              					) AS SEQ
                              			FROM AttdnProcessData AS P
                              			INNER JOIN EmployeeInformation AS ei ON ei.SystemId = p.EmpSystemID
                              				AND p.WorkDate <= ei.DOS
                              			WHERE p.DayStatus NOT IN (
                              					SELECT DISTINCT DayType
                              					FROM DayType
                              					WHERE Category IN (
                              							'Holiday'
                              							,'Weekend'
                              							)
                              					)
                              				AND ei.EmployeeStatus = 'Separated'
                              			) AS K
                              		WHERE K.dayStatustemp = 'A'
                              		) AS K -- AND K.SEQ<30
                              	WHERE K.SEQ = K.SQ
                              	GROUP BY K.EmpSystemID
                              	HAVING COUNT(*) >= 1
                              	) AS AB ON ab.EmpSystemID = EI.SystemId
                              WHERE FirstAbsentDate = EI.DOS
                              	AND EI.SystemId IN (
                              		SELECT E.SystemId
                              		FROM EmployeeInformation e
                              		INNER JOIN AttdnProcessData apd ON apd.EmpSystemID = e.SystemId
                              			AND apd.WorkDate = E.DOS
                              			AND ISNULL(apd.PunchInTime, '') = ''
                              		WHERE e.DOS BETWEEN '" + FromDate + @"'
                              				AND '" + FromDate + @"'
                              		)
                              	AND D.SEQ <= 1
                              	AND D.DayStatus = 'A'
                              	AND ei.DOS BETWEEN '" + FromDate + @"'
                              		AND '" + FromDate + @"'
                   and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                   and ei.DOJ<='" + FromDate + @"' AND (ei.DOS is null OR ei.DOS>= '" + FromDate + @"')
                              	  GROUP BY eI.SystemId
                              	,ab.AbsentDays
                              	,ei.EmployeeCode
                              	,Ei.EmployeeName
                                ,ei.CellPhnNo
                              	,D.DayStatus
                              	,ei.DOS
                              	,DP.UserName
                              	--,de.UserName
                              	,se.UserName
                              	,sus.UserName
                              	,ei.EmpPicPath
                              	,ab.FirstAbsentDate
                              	,ei.DOJ
								,pmb.Code
								,lgd.UserName
								,e.UserName
								,pr.UserName
								,ec.UserName
                                ,ei.EmployeeCurrentStatus
                            , DeG.UserName 
                            , L.UserName
                              HAVING COUNT(d.DayStatus) >= 1
                              ORDER BY AB.AbsentDays DESC";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetOffdayMissingPunchReports(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId
							
                        FROM AttdnProcessData AP
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus
                        WHERE
                                dt.OriginalDayType in ('W','H')  
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + FromDate + @"'   
                           and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                              
							and (---1
							( (AP.InTime IS NULL and AP.PunchInTime Is Null)	AND (AP.OutTime IS not NULL or AP.PunchOutTime Is NOT NULL))
							or 
							( (AP.InTime IS Not NULL or AP.PunchInTime Is Not Null)	AND (AP.OutTime IS NULL and AP.PunchOutTime Is NULL))
							--or
							--( AP.InTime IS not NULL	AND AP.OutTime IS not NULL)
							)----1                        		
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric
                               ,AP.WorkDate";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetOffdayWithPunchReports(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId
							
                        FROM AttdnProcessData AP
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus
                            WHERE  
                                dt.OriginalDayType in ('W','H')  
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + FromDate + @"'   
                           and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                              
							and (---1
							
							( AP.InTime IS Not NULL or AP.PunchInTime Is Not Null)
							
							)----1                     		
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric
                               ,AP.WorkDate";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetAbsentWithRawPunchReports(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            string fd = "01-" + Convert.ToDateTime(FromDate).ToString("MMM") + "-" + Convert.ToDateTime(FromDate).ToString("yyyy");
            string endDate = Convert.ToDateTime(fd).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        ,EI.SystemId
                        
                        FROM AttdnProcessData AP
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        LEFT join (select LogDownLoadNum,PDate,min(PTime)ptime from AttdnRawData where isnull(ptype,'')='' group by LogDownLoadNum,PDate) rd on rd.LogDownLoadNum = ap.EmpSystemID and rd.PDate = ap.WorkDate
                        
                        WHERE AP.DayStatus ='A'
                        and ISNULL(rd.LogDownLoadNum,'')<>''
                        
                        AND AP.WorkDate between '" + FromDate + @"' and  '" + FromDate + @"'
                        and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                        
                        ORDER BY AP.WorkDate
                        ,EmployeeCodePreFix,EmployeeCodeNumeric";

                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetShiftNotAssign(string FromDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            string fd = "01-" + Convert.ToDateTime(FromDate).ToString("MMM") + "-" + Convert.ToDateTime(FromDate).ToString("yyyy");
            string endDate = Convert.ToDateTime(fd).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                strSql = @"select x.EmployeeCode
                                    from org.Position p
                                    left join mst.ManpowerBudget mpb on mpb.PositionId = p.Id
                                    left join 
                                    
                                    (	select  e.EmployeeCode,e.EmployeeName,e.PlantId
									,FORMAT( EffectiveDate,'dd-MMM-yyyy')EffectiveDate
									,FORMAT(e.DOJ,'dd-MMM-yyyy')DOJ
									,FORMAT(c.CutOffDate,'dd-MMM-yyyy')CutOffDate
									,e.BudgetCode,e.LegalDesignationId
									,flag= case when isnull(es.EmpSystemID,'')='' then 'Shift Not Assign'
									when e.DOJ<es.EffectiveDate and c.CutOffDate<es.EffectiveDate then 'Wrong Effective Date'
									else ''									end
									from EmployeeInformation e
									left join SCS.OpeningBalanceCutOffDate c on c.PlantId=e.PlantId and c.ModuleName='HR'
									left join (select MIN(EffectiveDate)EffectiveDate,EmpSystemID from EmployeeShiftAssign where IsSingleDayShift=0
									group by EmpSystemID
									) es on es.EmpSystemID = e.SystemId
									
									) x on x.BudgetCode = mpb.Id									
									
									where x.flag != '' and isnull(EmployeeCode,'') != ''
									and x.EffectiveDate >= '" + FromDate + @"' and x.PlantId = '" + plantId + "'";

                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public string[] GetUnLockDateList(string plantId, string FromDate)
        {

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string fds = "01-" + Convert.ToDateTime(FromDate).ToString("MMM") + "-" + Convert.ToDateTime(FromDate).ToString("yyyy");
            try
            {
                string sql = @"SELECT FORMAT([LockedDate],'dd-MMM-yyyy') [LockedDates]
                              FROM [PlantWiseAttendanceLock]
                                where PlantId='" + plantId + @"' and LockedDate between '" + fds + @"' and '" + FromDate + @"' AND IsActive=1 
                            	 order by LockedDate desc";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                string[] result = new string[dsMaster.Tables[0].Rows.Count];

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    result[i] = dsMaster.Tables[0].Rows[i]["LockedDates"].ToString();

                }
                DateTime dtTo = Convert.ToDateTime(FromDate);
                string fd = "01-" + Convert.ToDateTime(FromDate).ToString("MMM") + "-" + Convert.ToDateTime(FromDate).ToString("yyyy");
                DateTime dtFrom = Convert.ToDateTime(fd);

                int diffdate = Convert.ToInt32((dtTo - dtFrom).TotalDays.ToString());

                string[] newResult = new string[diffdate + 1];
                for (int i = 0; i < diffdate + 1; i++)
                {
                    string nDate = dtFrom.AddDays(i).ToString("dd-MMM-yyyy");
                    if (result.Contains(nDate))
                        continue;

                    newResult[i] = nDate;
                }
                return newResult;
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        private string columnName()
        {
            return @",ei.EmployeeCurrentStatus,EI.CellPhnNo TelePhnNo
                            ,EI.EmployeeCode
                        	,EI.EmployeeName
                        	,PMB.Code BudgetCode
                            , LGD.userName LegalDesignation
                            , DeG.UserName Designation
                            , DP.UserName Department
                            , se.UserName Section
                            , Sus.UserName SubSection
                            , E.UserName EntityName
                            , PR.UserName PositionName
                            , FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                            , EC.UserName as EmployeeCategory
                            , L.UserName Line ";

        }
        private string tableName()
        {
            return @"
                        LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode = PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                        LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id                        
                        LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                        LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                        left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
						LEFT JOIN HKP.Designation DeG ON DeG.Id = dm.DesignationId
                        left join HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                        LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                        ";
        }
    }
}
