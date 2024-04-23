using ConnectionManager;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Hosting;
using static Library.Service.Helpers.ReportUtility;



namespace Library.Accounting.Accounts
{
    public class AccountsSalaryPayableService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsSalaryPayableService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        public List<Dictionary<string, object>> GetSalaryLockDataList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity, string plantId)
        {
            string wcEmpStatus = " and (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " and (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR case when  ISNULL(SalaryProcFlag,'Regular') ='' then 'Regular' else ISNULL(SalaryProcFlag,'Regular') end = 'Regular' ";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR ISNULL(SalaryProcFlag,'Regular') ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus += " OR ISNULL(SalaryProcFlag,'Regular') ='MLV_PRE'";

                }
            }
            wcEmpStatus += ")";

            string sql = @"
            select x.* from (
            SELECT sh.SalaryHead,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType
                        , SUM(spc.DisbusmentAmount) DrAmount
                        , 0 CrAmount
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount,HeadHeadFilter=sh.SalaryHead
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"' AND sl.PayableVoucherId IS NULL--and sl.EmpSystemId='" + employeeId + @"' 
                        AND ISNULL(sh.HeadCategory,'') not in ('CTC','Gross','Total Gross','Net Payable') and spc.DisbusmentAmount!=0 AND SPL.PlantId='" + plantId + @"'
                        AND  PO.DirectManpowerCost=1 AND sh.HeadType='E' AND sh.IsGrossComponent=1 AND sh.PartOfNetPay=1 " + wcEmpStatus + @"
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee])
                        GROUP BY sh.SalaryHead,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence]
                        UNION
                        SELECT sh.SalaryHead,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType
                        , SUM(spc.DisbusmentAmount) DrAmount
                        , 0 CrAmount
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount,HeadHeadFilter=sh.SalaryHead
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"' AND sl.PayableVoucherId IS NULL--and sl.EmpSystemId='" + employeeId + @"' 
                        AND ISNULL(sh.HeadCategory,'') not in ('CTC','Gross','Total Gross','Net Payable') and spc.DisbusmentAmount!=0 AND SPL.PlantId='" + plantId + @"'
                        AND  PO.DirectManpowerCost=1 AND sh.HeadType='E' AND sh.IsGrossComponent=0 AND sh.PartOfNetPay=1 " + wcEmpStatus + @"
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee])
                        GROUP BY sh.SalaryHead,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence]
                        UNION
                        SELECT sh.SalaryHead,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType
                        , SUM(spc.DisbusmentAmount) DrAmount
                        , 0 CrAmount
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount,HeadHeadFilter='NetPay'
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"' AND sl.PayableVoucherId IS NULL --and sl.EmpSystemId='" + employeeId + @"' 
                        AND ISNULL(sh.SalaryHead,'')  in ('Net Pay') and spc.DisbusmentAmount!=0 " + wcEmpStatus + @" AND SPL.PlantId='" + plantId + @"'
                        AND  PO.DirectManpowerCost=1 AND sh.HeadType='E' 
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee])
                        GROUP BY sh.SalaryHead,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence]
                        UNION
                        SELECT sh.SalaryHead,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType
                        , SUM(spc.DisbusmentAmount) DrAmount
                        , 0 CrAmount
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount,HeadHeadFilter=sh.SalaryHead
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"' AND sl.PayableVoucherId IS NULL --and sl.EmpSystemId='" + employeeId + @"'
                        --and ISNULL(sh.HeadCategory,'') not in ('CTC','Gross','Total Gross','Net Payable')  
                        and spc.DisbusmentAmount!=0 and  PO.DirectManpowerCost=1 AND sh.HeadType='D' " + wcEmpStatus + @" AND SPL.PlantId='" + plantId + @"'
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee])
                        GROUP BY sh.SalaryHead,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence]
                        ) x
						ORDER BY x.HeadType desc,x.[Sequence] asc";
            return _sqlRepository.GetDataCollection(sql);

        }

        public List<Dictionary<string, object>> GetSalaryLockCTCDataList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity, string plantId)
        {

            string wcEmpStatus = " AND spm.SalaryProcFlag=''";


            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED','MLV_PRE')";
            }
            else if (isActive == true && isSeperated == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED')";
            }
            else if (isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else if (isActive == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag =''";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='MLV_PRE'";

                }
            }
            string sql = @"
                        SELECT sh.SalaryHead,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType
                        , SUM(spc.DisbusmentAmount) DrAmount
                        , 0 CrAmount
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"' AND sl.PayableVoucherId IS NULL --and sl.EmpSystemId='" + employeeId + @"' 
                        AND ISNULL(sh.HeadCategory,'')  in ('CTC') and spc.DisbusmentAmount!=0 AND SPL.PlantId='" + plantId + @"'
                        AND  PO.DirectManpowerCost=1 AND sh.HeadType='E' AND sh.IsGrossComponent=0 AND sh.PartOfNetPay=0 " + wcEmpStatus + @"
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee])
                        GROUP BY sh.SalaryHead,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence]
						ORDER BY sh.[Sequence],sh.SalaryHead";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetSalaryLockDataGLList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity, string plantId)
        {
            string wcEmpStatus = " AND spm.SalaryProcFlag=''";


            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED','MLV_PRE')";
            }
            else if (isActive == true && isSeperated == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED')";
            }
            else if (isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else if (isActive == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag =''";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='MLV_PRE'";

                }
            }
            string sql = @"SELECT 
                       X.SalaryHeadId,X.AccountsGroupId ,'Direct' SalaryType,X.SalaryHead ,X.SalaryHeadCategory,
                            X.DirectGLName,X.DirectBudgetName,X.DirectActivityName, SUM(X.DrAmount) DrAmount,SUM(X.CrAmount) CrAmount,SUM(X.DisbusmentAmount) DisbusmentAmount,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId 
                        FROM
                        (
                         select sh.SalaryHeadID SalaryHeadId, SGL.AccountsGroupId,sh.SalaryHead,sh.HeadCategory SalaryHeadCategory,sl.YearNo,sl.MonthNo,sh.HeadType
                        , DrAmount=case when SUM(spc.DisbusmentAmount) < 0 then SUM(spc.DisbusmentAmount)*-1 else SUM(spc.DisbusmentAmount) end
                        , 0 CrAmount
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount

						 ,SGL.DrDirectGLId GLGeneralInfoId
						, SGL.DrDirectBudgetMasterId BudgetMasterId
						,SGL.DrDirectActivityId ActivityId
						,DGL.AccountCode+' - '+DGL.UserName DirectGLName
                           ,DB.UserName DirectBudgetName
                           ,DA.UserName DirectActivityName
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
						left join MST.SalaryHeadGL SGL ON SGL.SalaryHeadId=sh.SalaryHeadID and ISNULL(DMC.AccountsGroupId,'')=ISNULL(SGL.AccountsGroupId,'')

						 LEFT JOIN HKP.GLGeneralInfo DGL ON DGL.Id=SGL.DrDirectGLId
                            LEFT JOIN MST.BudgetMaster DBM ON DBM.Id=SGL.DrDirectBudgetMasterId
                            LEFT JOIN HKP.Budget DB ON DB.Id=DBM.BudgetId
                            LEFT JOIN HKP.Activity DA ON DA.Id=SGL.DrDirectActivityId
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"' AND sl.PayableVoucherId IS NULL " + wcEmpStatus + @" AND SPL.PlantId='" + plantId + @"'--and sl.EmpSystemId='" + employeeId + @"' 
                        and ISNULL(sh.HeadCategory,'') not in ('CTC','Gross','Total Gross') and spc.DisbusmentAmount!=0 and PO.DirectManpowerCost=1 
                        AND ISNULL(sgl.DrDirectActivityId,'')<>'' 
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee] where month(effectivedate)<=sl.MonthNo and year(effectivedate)=sl.YearNo)
                        GROUP BY sh.SalaryHeadID,sh.SalaryHead, SGL.AccountsGroupId,sh.HeadCategory,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence],SGL.DrDirectGLId,SGL.DrDirectBudgetMasterId,SGL.DrDirectActivityId
						,DGL.AccountCode,DGL.UserName ,DB.UserName ,DA.UserName
						,SGL.CrDirectGLId,SGL.CrDirectBudgetMasterId,SGL.CrDirectActivityId
            UNION
			SELECT sh.SalaryHeadID SalaryHeadId, SGL.AccountsGroupId,sh.SalaryHead,sh.HeadCategory SalaryHeadCategory,sl.YearNo,sl.MonthNo,sh.HeadType
                        , 0 DrAmount
                        ,  CrAmount=case when SUM(spc.DisbusmentAmount) < 0 then SUM(spc.DisbusmentAmount)*-1 else SUM(spc.DisbusmentAmount) end
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount
						 ,SGL.CrDirectGLId GLGeneralInfoId
						, SGL.CrDirectBudgetMasterId BudgetMasterId
						,SGL.CrDirectActivityId ActivityId
						,CDGL.AccountCode+' - '+CDGL.UserName DirectGLName
                           ,CDB.UserName DirectBudgetName
                           ,CDA.UserName DirectActivityName
                        FROM  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
						left join MST.SalaryHeadGL SGL ON SGL.SalaryHeadId=sh.SalaryHeadID and ISNULL(DMC.AccountsGroupId,'')=ISNULL(SGL.AccountsGroupId,'')

							LEFT JOIN HKP.GLGeneralInfo CDGL ON CDGL.Id=SGL.CrDirectGLId
                            LEFT JOIN MST.BudgetMaster CDBM ON CDBM.Id=SGL.CrDirectBudgetMasterId
                            LEFT JOIN HKP.Budget CDB ON CDB.Id=CDBM.BudgetId
                            LEFT JOIN HKP.Activity CDA ON CDA.Id=SGL.CrDirectActivityId
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"'  AND sl.PayableVoucherId IS NULL " + wcEmpStatus + @" AND SPL.PlantId='" + plantId + @"'--and sl.EmpSystemId='" + employeeId + @"' 
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee] where month(effectivedate)<=sl.MonthNo and year(effectivedate)=sl.YearNo)
                        AND ISNULL(sh.HeadCategory,'') not in ('CTC','Gross','Total Gross') and spc.DisbusmentAmount!=0 and PO.DirectManpowerCost=1 
                         AND  ISNULL(sgl.CrDirectActivityId,'')<>'' 
                        GROUP BY sh.SalaryHeadID,sh.SalaryHead, SGL.AccountsGroupId,sh.HeadCategory,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence]
						,SGL.CrDirectGLId,SGL.CrDirectBudgetMasterId,SGL.CrDirectActivityId
                        ,CDGL.AccountCode,CDGL.UserName ,CDB.UserName ,CDA.UserName
						)X
						GROUP BY 
						X.SalaryHeadId,X.SalaryHead, X.AccountsGroupId,X.SalaryHeadCategory,
                        X.DirectGLName,X.DirectBudgetName,X.DirectActivityName,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId 
						ORDER BY 7";
            return _sqlRepository.GetDataCollection(sql);

        }

        public List<Dictionary<string, object>> GetSalaryLockInDirectTakeAwayDataList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity, string plantId)
        {
            string wcEmpStatus = " AND spm.SalaryProcFlag=''";



            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED','MLV_PRE')";
            }
            else if (isActive == true && isSeperated == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED')";
            }
            else if (isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else if (isActive == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag =''";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='MLV_PRE'";

                }
            }
            string sql = @"
                    select x.* from (
                        SELECT sh.SalaryHead,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType
                        , SUM(spc.DisbusmentAmount) DrAmount
                        , 0 CrAmount
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount,HeadHeadFilter=sh.SalaryHead
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"'  AND sl.PayableVoucherId IS NULL " + wcEmpStatus + @"--and sl.EmpSystemId='" + employeeId + @"' 
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee] where month(effectivedate)<=sl.MonthNo and year(effectivedate)=sl.YearNo)
                        AND ISNULL(sh.HeadCategory,'') not in ('CTC','Gross','Total Gross','Net Payable') and spc.DisbusmentAmount!=0 
                        AND  PO.DirectManpowerCost=0 AND sh.HeadType='E' AND sh.IsGrossComponent=1 AND sh.PartOfNetPay=1 AND SPL.PlantId='" + plantId + @"'
                        GROUP BY sh.SalaryHead,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence]
                        UNION
                        SELECT sh.SalaryHead,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType
                        , SUM(spc.DisbusmentAmount) DrAmount
                        , 0 CrAmount
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount,HeadHeadFilter=sh.SalaryHead
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"' AND sl.PayableVoucherId IS NULL " + wcEmpStatus + @"--and sl.EmpSystemId='" + employeeId + @"'  
                        AND ISNULL(sh.HeadCategory,'') not in ('CTC','Gross','Total Gross','Net Payable') and spc.DisbusmentAmount!=0 
                        AND  PO.DirectManpowerCost=0 AND sh.HeadType='E' AND sh.IsGrossComponent=0 AND sh.PartOfNetPay=1 AND SPL.PlantId='" + plantId + @"'
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee])
                        GROUP BY sh.SalaryHead,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence]
                        UNION
                        SELECT sh.SalaryHead,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType
                        , SUM(spc.DisbusmentAmount) DrAmount
                        , 0 CrAmount
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount,HeadHeadFilter='NetPay'
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"' AND sl.PayableVoucherId IS NULL " + wcEmpStatus + @"--and sl.EmpSystemId='" + employeeId + @"' 
                        AND ISNULL(sh.SalaryHead,'')  in ('Net Pay') and spc.DisbusmentAmount!=0 AND SPL.PlantId='" + plantId + @"'
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee] where month(effectivedate)<=sl.MonthNo and year(effectivedate)=sl.YearNo)
                        AND  PO.DirectManpowerCost=0 AND sh.HeadType='E' 

                        GROUP BY sh.SalaryHead,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence]
                        UNION
                        SELECT sh.SalaryHead,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType
                        , SUM(spc.DisbusmentAmount) DrAmount
                        , 0 CrAmount
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount,HeadHeadFilter=sh.SalaryHead
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"'  AND sl.PayableVoucherId IS NULL " + wcEmpStatus + @" AND SPL.PlantId='" + plantId + @"'--and sl.EmpSystemId='" + employeeId + @"' 
                        --and ISNULL(sh.HeadCategory,'') not in ('CTC','Gross','Total Gross','Net Payable') 
                        and spc.DisbusmentAmount!=0  and  PO.DirectManpowerCost=0 AND sh.HeadType='D'
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee] where month(effectivedate)<=sl.MonthNo and year(effectivedate)=sl.YearNo)
                        GROUP BY sh.SalaryHead,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence]
                        ) x
						ORDER BY x.HeadType desc,x.[Sequence] asc";
            return _sqlRepository.GetDataCollection(sql);
        }


        public List<Dictionary<string, object>> GetSalaryLockInDirectCTCDataList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity, string plantId)
        {
            string wcEmpStatus = " AND spm.SalaryProcFlag=''";



            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED','MLV_PRE')";
            }
            else if (isActive == true && isSeperated == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED')";
            }
            else if (isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else if (isActive == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag =''";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='MLV_PRE'";

                }
            }
            string sql = @"
                        SELECT sh.SalaryHead,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType
                        , SUM(spc.DisbusmentAmount) DrAmount
                        , 0 CrAmount
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"'  AND sl.PayableVoucherId IS NULL--and sl.EmpSystemId='" + employeeId + @"' 
                        AND ISNULL(sh.HeadCategory,'')  in ('CTC') and spc.DisbusmentAmount!=0 
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee] where month(effectivedate)<=sl.MonthNo and year(effectivedate)=sl.YearNo)
                        AND  PO.DirectManpowerCost=0 AND sh.HeadType='E' AND sh.IsGrossComponent=0 AND sh.PartOfNetPay=0 " + wcEmpStatus + @" AND SPL.PlantId='" + plantId + @"'
                        GROUP BY sh.SalaryHead,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence]
						ORDER BY sh.[Sequence],sh.SalaryHead";
            return _sqlRepository.GetDataCollection(sql);
        }


        public List<Dictionary<string, object>> GetSalaryLockInDirectDataGLList(string yearNo, string monthNo, string employeeId, bool isActive, bool isSeperated, bool isMaternity, string plantId)
        {
            string wcEmpStatus = " AND spm.SalaryProcFlag=''";



            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED','MLV_PRE')";
            }
            else if (isActive == true && isSeperated == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED')";
            }
            else if (isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else if (isActive == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag =''";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='MLV_PRE'";

                }
            }
            string sql = @"SELECT 
                            X.SalaryHeadId,X.AccountsGroupId ,'InDirect' SalaryType, X.SalaryHead ,X.SalaryHeadCategory,
                            X.InDirectGLName,X.InDirectBudgetName,X.InDirectActivityName, SUM(X.DrAmount) DrAmount,SUM(X.CrAmount) CrAmount,SUM(X.DisbusmentAmount) DisbusmentAmount,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId 
                            FROM
                            (
                            select sh.SalaryHeadID SalaryHeadId,SGL.AccountsGroupId,sh.SalaryHead,sh.HeadCategory SalaryHeadCategory,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType
                        , DrAmount=case when SUM(spc.DisbusmentAmount) < 0 then SUM(spc.DisbusmentAmount)*-1 else SUM(spc.DisbusmentAmount) end
                        , 0 CrAmount
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount

						 ,SGL.DrInDirectGLId GLGeneralInfoId
						, SGL.DrInDirectBudgetMasterId BudgetMasterId
						,SGL.DrInDirectActivityId ActivityId
						,IGL.AccountCode+' - '+IGL.UserName InDirectGLName
                           ,IB.UserName InDirectBudgetName
                           ,IA.UserName InDirectActivityName
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
						left join MST.SalaryHeadGL SGL ON SGL.SalaryHeadId=sh.SalaryHeadID and ISNULL(DMC.AccountsGroupId,'')=ISNULL(SGL.AccountsGroupId,'')
						 LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=SGL.DrInDirectGLId
                            LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=SGL.DrInDirectBudgetMasterId
                            LEFT JOIN HKP.Budget IB ON IB.Id=IBM.BudgetId
                            LEFT JOIN HKP.Activity IA ON IA.Id=SGL.DrInDirectActivityId
							
                        where sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"'  AND sl.PayableVoucherId IS NULL " + wcEmpStatus + @" AND SPL.PlantId='" + plantId + @"'--and sl.EmpSystemId='" + employeeId + @"' 
                        and ISNULL(sh.HeadCategory,'') not in ('CTC','Gross','Total Gross') and spc.DisbusmentAmount!=0 and PO.DirectManpowerCost=0 
                        and ISNULL(sgl.DrInDirectActivityId,'')<>''
                       AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee] where month(effectivedate)<=sl.MonthNo and year(effectivedate)=sl.YearNo)
                        group by sh.SalaryHeadID,SGL.AccountsGroupId,sh.SalaryHead,sh.HeadCategory,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence],SGL.DrInDirectGLId,SGL.DrInDirectBudgetMasterId,SGL.DrInDirectActivityId
						,IGL.AccountCode,IGL.UserName ,IB.UserName ,IA.UserName
                        UNION
			            select sh.SalaryHeadID SalaryHeadId,SGL.AccountsGroupId,sh.SalaryHead,sh.HeadCategory SalaryHeadCategory,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType
                        , 0 DrAmount
                        , CrAmount=case when SUM(spc.DisbusmentAmount) < 0 then SUM(spc.DisbusmentAmount)*-1 else SUM(spc.DisbusmentAmount) end
                        ,SUM(spc.DisbusmentAmount) DisbusmentAmount
						 ,SGL.CrInDirectGLId GLGeneralInfoId
						, SGL.CrInDirectBudgetMasterId BudgetMasterId
						,SGL.CrInDirectActivityId ActivityId
						,CIGL.AccountCode+' - '+CIGL.UserName InDirectGLName
                           ,CIB.UserName InDirectBudgetName
                           ,CIA.UserName InDirectActivityName
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
						left join MST.SalaryHeadGL SGL ON SGL.SalaryHeadId=sh.SalaryHeadID and ISNULL(DMC.AccountsGroupId,'')=ISNULL(SGL.AccountsGroupId,'')

							LEFT JOIN HKP.GLGeneralInfo CIGL ON CIGL.Id=SGL.CrInDirectGLId
                            LEFT JOIN MST.BudgetMaster CIBM ON CIBM.Id=SGL.CrInDirectBudgetMasterId
                            LEFT JOIN HKP.Budget CIB ON CIB.Id=CIBM.BudgetId
                            LEFT JOIN HKP.Activity CIA ON CIA.Id=SGL.CrInDirectActivityId
                        where sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"'  AND sl.PayableVoucherId IS NULL " + wcEmpStatus + @" AND SPL.PlantId='" + plantId + @"'--and sl.EmpSystemId='" + employeeId + @"' 
                        and ISNULL(sh.HeadCategory,'') not in ('CTC','Gross','Total Gross') and spc.DisbusmentAmount!=0 and PO.DirectManpowerCost=0 
                        AND ISNULL(sgl.CrInDirectActivityId,'')<>'' 
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee] where month(effectivedate)<=sl.MonthNo and year(effectivedate)=sl.YearNo)
                        group by sh.SalaryHeadID,SGL.AccountsGroupId,sh.SalaryHead,sh.HeadCategory,sl.YearNo,sl.MonthNo,sh.HeadType,sh.[Sequence]
						,SGL.CrInDirectGLId,SGL.CrInDirectBudgetMasterId,SGL.CrInDirectActivityId
                        ,CIGL.AccountCode,CIGL.UserName ,CIB.UserName ,CIA.UserName
						)X
						GROUP BY 
						X.SalaryHeadId,X.AccountsGroupId,X.SalaryHead,X.SalaryHeadCategory,
                        X.InDirectGLName,X.InDirectBudgetName,X.InDirectActivityName,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId 
						ORDER BY 7";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetDirectSalaryLockSalarySheetData(string yearNo, string monthNo, bool isActive, bool isSeperated, bool isMaternity, string plantId)
        {
            string wcEmpStatus = " AND spm.SalaryProcFlag=''";



            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED','MLV_PRE')";
            }
            else if (isActive == true && isSeperated == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED')";
            }
            else if (isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else if (isActive == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag =''";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='MLV_PRE'";

                }
            }
            string sql = @"
            SELECT x.* FROM (
                        SELECT sh.SalaryHead,sh.HeadCategory SalaryHeadCategory,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType,sl.EmpSystemId EmployeeId,ei.EmployeeName
                        ,sl.PayableVoucherId VoucherId
                        , 0 CrAmount,ESA.PrincipalAmount Amount,ESA.ProfitAmount,ESA.InstallmentAmount
                        ,spc.DisbusmentAmount*-1 DisbusmentAmount,HeadHeadFilter='NetPay',ESA.AdvanceId,ESA.AdvanceDetailId,ESA.EmployeeSalaryAdvanceId,shgl.CrDirectActivityId ActivityId,0 IsOrderSpecific
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                        left join mst.SalaryHeadGL shgl on shgl.SalaryHeadId=sh.SalaryHeadID and ISNULL(DMC.AccountsGroupId,'')=ISNULL(shgl.AccountsGroupId,'')
						LEFT JOIN (SELECT EA.EmployeeId,ARS.MonthNo,ARS.YearNo,ARS.InstallmentAmount,ARS.PrincipalAmount,ARS.ProfitAmount,A.Id AdvanceId,AD.Id AdvanceDetailId,ARS.EmployeeSalaryAdvanceId
						FROM TRN.EmployeeSalaryAdvance EA JOIN DBO.AdvanceReqSchedule ARS ON ARS.EmployeeSalaryAdvanceId=EA.Id 
						JOIN TRN.Advance A ON A.VoucherId=EA.VoucherId
						JOIN TRN.AdvanceDetail AD ON AD.AdvanceId=A.Id
                        JOIN TRN.EmployeeAdvanceDeduction EAD ON EAD.AdvanceReqScheduleId=ARS.Id
						WHERE ARS.MonthNo='" + monthNo + "' AND ARS.YearNo='" + yearNo + @"'  ) ESA ON ESA.EmployeeId=SL.EmpSystemId
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"' AND sl.PayableVoucherId IS NULL 
                        AND ISNULL(sh.HeadCategory,'')  in ('Advance') and spc.DisbusmentAmount!=0 " + wcEmpStatus + @" AND SPL.PlantId='" + plantId + @"'
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee] where month(effectivedate)<=sl.MonthNo and year(effectivedate)=sl.YearNo)
                        AND  PO.DirectManpowerCost=1 AND sh.HeadType='D' 
                        ) x
						ORDER BY x.HeadType desc,x.[Sequence] asc";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetInDirectSalaryLockSalarySheetData(string yearNo, string monthNo, bool isActive, bool isSeperated, bool isMaternity, string plantId)
        {
            string wcEmpStatus = " AND spm.SalaryProcFlag=''";


            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED','MLV_PRE')";
            }
            else if (isActive == true && isSeperated == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','SEPARATED')";
            }
            else if (isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else if (isActive == true && isMaternity == true)
            {
                wcEmpStatus = " AND spm.SalaryProcFlag IN ('','MLV_PRE')";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag =''";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus = " AND spm.SalaryProcFlag ='MLV_PRE'";

                }
            }
            string sql = @"
            SELECT x.* FROM (
                        SELECT sh.SalaryHead,sh.HeadCategory SalaryHeadCategory,sh.[Sequence],sl.YearNo,sl.MonthNo,sh.HeadType,sl.EmpSystemId EmployeeId,ei.EmployeeName
                        ,sl.PayableVoucherId VoucherId
                        , 0 CrAmount,ESA.PrincipalAmount Amount,ESA.ProfitAmount,ESA.InstallmentAmount
                        ,spc.DisbusmentAmount*-1 DisbusmentAmount,HeadHeadFilter='NetPay',ESA.AdvanceId,ESA.AdvanceDetailId,ESA.EmployeeSalaryAdvanceId,shgl.CrInDirectActivityId ActivityId,0 IsOrderSpecific
                        from  dbo.SalaryProcMaster spm 
						left join dbo.SalaryProcChild spc on spc.SlrProcMstSystemID=spm.SystemID 
						LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                        LEFT JOIN dbo.SalaryProcessLogDetail SPL ON SPL.SalaryProcessId=SPM.SystemID AND spl.EmpSystemId=spc.EmpInfoSystemID
						left join dbo.EmployeeInformation ei on ei.SystemId=sl.EmpSystemId
						left join mst.DesignationMaster DM on DM.DesignationId=ei.GivenDesignationId
						left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.id
						left join MST.ManpowerBudget MPB on MPB.Id=SPL.BudgetCode
						left join ORG.Position PO on PO.Id=MPB.PositionId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                        left join mst.SalaryHeadGL shgl on shgl.SalaryHeadId=sh.SalaryHeadID and ISNULL(DMC.AccountsGroupId,'')=ISNULL(shgl.AccountsGroupId,'')
						LEFT JOIN (SELECT EA.EmployeeId,ARS.MonthNo,ARS.YearNo,ARS.InstallmentAmount,ARS.PrincipalAmount,ARS.ProfitAmount,A.Id AdvanceId,AD.Id AdvanceDetailId,ARS.EmployeeSalaryAdvanceId
						FROM TRN.EmployeeSalaryAdvance EA JOIN DBO.AdvanceReqSchedule ARS ON ARS.EmployeeSalaryAdvanceId=EA.Id 
						JOIN TRN.Advance A ON A.VoucherId=EA.VoucherId
						JOIN TRN.AdvanceDetail AD ON AD.AdvanceId=A.Id
                        JOIN TRN.EmployeeAdvanceDeduction EAD ON EAD.AdvanceReqScheduleId=ARS.Id
						WHERE ARS.MonthNo='" + monthNo + "' AND ARS.YearNo='" + yearNo + @"'  ) ESA ON ESA.EmployeeId=SL.EmpSystemId
                        WHERE sl.MonthNo='" + monthNo + "' and sl.YearNo='" + yearNo + @"' AND sl.PayableVoucherId IS NULL 
                        AND ISNULL(sh.HeadCategory,'')  in ('Advance') and spc.DisbusmentAmount!=0 " + wcEmpStatus + @" AND SPL.PlantId='" + plantId + @"'
                        AND  PO.DirectManpowerCost=0 AND sh.HeadType='D' 
                        AND  ei.SystemId not in (select EmpSystemId from [dbo].[ExceptionEmployee] where month(effectivedate)<=sl.MonthNo and year(effectivedate)=sl.YearNo)
                        ) x
						ORDER BY x.HeadType desc,x.[Sequence] asc";
            return _sqlRepository.GetDataCollection(sql);
        }

        #region Salary Payable Disbursment
        public void GetEmployeeInfoDetailSalaryLogWiseSalaryPayable(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef, string voucherId)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            string salaryProcessId = "";
            var _wc = string.Empty;
            var wcSalaryProcessSystemIdStr = "";


            if (!string.IsNullOrEmpty(salaryProcessSystemId) && salaryProcessSystemId != "undefined" && salaryProcessSystemId != "null")
            {
                wcSalaryProcessSystemIdStr = "SystemID IN ('" + salaryProcessSystemId + @"')";
            }
            else
            {
                wcSalaryProcessSystemIdStr = @"SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + "') AND YearNo = Year('" + fromDate + "')  )";


                string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')";

                DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);
                salaryProcessId = "''";
                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
                {
                    salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
                }
            }
            string empStatus = " and (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                empStatus = " and (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    empStatus += " OR case when  ISNULL(SalaryProcFlag,'Regular') ='' then 'Regular' else ISNULL(SalaryProcFlag,'Regular') end = 'Regular' ";
                }
                if (isSeperated == true)
                {
                    empStatus += " OR ISNULL(SalaryProcFlag,'Regular') ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    empStatus += " OR ISNULL(SalaryProcFlag,'Regular') ='MLV_PRE'";

                }
            }
            empStatus += ")";

            try
            {
                strSQL = @"SELECT EmpBasic.*,MMDSA.*,ISNULL(MW.Grade,'') Grade,ISNULL(MW.SalaryHeadValue,0) MinimumWage
                            FROM
                                    (
									SELECT DISTINCT E.SystemID EmpSystemId,ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric
                                            ,E.GroupID CompanyGroupId,E.CompanyId, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus EmployeeStatusReal,E.EmployeeCurrentStatus
											, DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,
											'' UserGroupSystemID,  F.Id PlantID, F.UserName PlantName, 
											FU.UserName UnitName,  DV.UserName DivisionName,  DP.UserName DepartmentName,
											 S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName,EC.WorkingDaysInAMonth--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,e.SalaryRuleMasterSystemID,Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation,ISNULL(E.NationalID,'') NationalID
											,ISNULL(Line.UserName,'') LineName
											,ISNULL(E.GenderID,'') Gender
                                            ,ISNULL(LSalGr.Code,'') GradeCode
											,ISNULL(PG.UserName,'') PayRollGroup
                                    , CASE WHEN ISNULL(SPM.SalaryProcFlag,'') = '' THEN 'Regular' ELSE SalaryProcFlag END EmployeeStatus
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(SPLD.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(spld.BankAccNo,'') BankAccNo
                                    ,ISNULL(spld.IFSCCode,'') IFSCCode
                                    ,CASE WHEN ISNULL(PO.IsDirect,0) = 0 THEN 'No' ELSE 'Yes' END IsDirect
                                    ,CASE WHEN ISNULL(PO.DirectManpowerCost,0) = 0 THEN 'No' ELSE 'Yes' END DirectManpowerCost
                                    ,SalaryProcFlag
                            		,IsLock = case when sl.IsLocked = 1 then 'Yes' else 'No' end
                            		, sl.PayableVoucherId
									,IsDisburse = case when sl.IsDisbursed = 1 then 'Yes' else 'No' end
									,sl.DisbursementVoucherId
                                     FROM  dbo.SalaryLock sl
									join EmployeeInformation E on sl.EmpSystemId=E.SystemId

                                          Left JOIN (
                                    SELECT DISTINCT EmpInfoSystemID,SlrProcMstSystemID,PlantID ,m.Description,m.SalaryProcFlag
                                    FROM SalaryProcChild c
                                    JOIN SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID

								 --left join dbo.SalaryLock sl on sl.MonthNo=m.MonthNo and sl.YearNo=m.YearNo and sl.PayableVoucherId=<>''

                                    WHERE SlrProcMstSystemID IN(" + salaryProcessId + @") 
                                    ) SPM ON spm.EmpInfoSystemID=e.SystemId
									 JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId  IN(" + salaryProcessId + @") AND e.SystemId = SPLD.EmpSystemId  --SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId and SPLD.PlantId = '202022' 
                         
									 			LEFT JOIN ORG.Plant F ON SPLD.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON SPLD.LegalDesignationId = LDS.Id
								LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = SPLD.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

												LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
												  LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = SPLD.BankSystemID
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LDS.Id and E.PlantId = LSGD.PlantId
                                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = SPLD.LegalSalaryGradeId  --and SPLD.PlantId = LSalGr.PlantId
												
												LEFT JOIN org.Unit FU ON ENT.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PO.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON PO.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON PO.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PO.SubSectionID = SS.Id

												LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
            --                                    (
            --                                    SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												--LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												--)EC ON EC.DesignationId=E.GivenDesignationId
												[HKP].[EmployeeCategory] EC ON EC.Id = SPLD.EmployeeCategoryId
											

                                      --Where SPC.SlrProcMstSystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      --WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        --WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        --AND MonthNo =   MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')   )   
									) EmpBasic
                                   LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + fromDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = EmpBasic.EmpSystemId
                                    INNER JOIN
		                                    (
													SELECT EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
										,ISNULL(TotalLv,0) TotalLv
										,ISNULL(TotalMLv,0) TotalMLv,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv,ISNULL(TotalWeekOff,0) +  ISNULL(TotalWeekOffHoliDay,0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
										,ISNULL(TotalOTHr,0) TotalOTHr,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr,ISNULL(WeekOffOTHr,0) WeekOffOTHr
										,ISNULL(HoliDayOTHr,0) HoliDayOTHr,ISNULL(TotalLWP,0) TotalLWP,ISNULL(IsOTEntitled,0) IsOTEntitled,ISNULL(OTRate,0) OTRate,ISNULL(TotalHoliDay,0) TotalHoliDay
										  FROM SalaryProceAttdnData MMDSA where MMDSA.MonthNo = MONTH('" + fromDate + @"') AND
						                               MMDSA.YearNo = YEAR('" + fromDate + @"') AND MMDSA.PlantID = '" + plantId + @"' 
											) MMDSA ON EmpBasic.EmpSystemID = MMDSA.EmpSystemID 
                                            WHERE EmpBasic.CompanyGroupId = '" + companyGroupId + @"'  AND EmpBasic.PlantId ='" + plantId + @"' 
		                                                                " + empStatus + @"                    and EmpBasic.PayableVoucherId <>''  ";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"and EmpBasic.EmpSystemId IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSQL += @"Order by EmpBasic.EmployeeCodePreFix,EmpBasic.EmployeeCodeNumeric ";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);


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

        public Dictionary<string, List<DataRow>> GetEmployeeSalaryInfoDetail(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, out DataTable distinctSalaryHead)
        {
            string strSQL;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            distinctSalaryHead = new DataTable("Tmp");
            string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + @"') AND YearNo = Year('" + fromDate + @"')";
            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);

            string salaryProcessID = "''";
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessID += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }


            try
            {
                strSQL = @"SELECT EmpSlr.*,PSH.Sequence,ISNULL(crc.IsDecimalInDisb,0) IsDecimalInDisb,ISNULL(CRC.IntegerInDisb,1) IntegerInDisb,ISNULL(CRC.DecimalNo,0) DecimalNo FROM(SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
                                                    SPC.EmpInfoSystemID EmpSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
                                                    SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
                                                    SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
                                                    CRE.Name AS PlantWiseExchangeCR, EXR.ToCurrencyBuying ExchangeRate, SPM.AmtDefinitionCurrencyID,
                                                    CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect, ISNULL(SH.IsCTCComponent,0) IsCTCComponent, ISNULL(SH.IsGrossComponent,0) IsGrossComponent
                                                    , sh.SalaryHead, sh.HeadCategory, sh.HeadType, ISNULL(SH.PartOfNetPay,0) PartOfNetPay

                                     FROM SalaryProcChild SPC

                                        left JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID



                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID= spc.SalaryHeadID


                                                        LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id

                                                        LEFT JOIN (
                                                                   SELECT* FROM ExchangerateDateWiseForHR

                                                                   WHERE FromDate IN (SELECT MAX(FromDate) FromDate FROM SalaryProcMaster


                                                                                                            WHERE SystemID IN(" + salaryProcessID + @")
																  )) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode

                                                                                            AND SPC.PlantID = Exr.PlantID

                                                        LEFT JOIN SCS.Currency CRE ON EXR.FromCurrencyCode = CRE.Id

                                                        where isnull(SPC.SlrProcMstSystemID,'')  IN(" + salaryProcessID + @")) EmpSlr--ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID

                                            Inner join EmployeeInformation EEI ON EEI.SystemId = EmpSlr.EmpSystemID

                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID

                                        LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID  AND SRG.SalaryHeadID = EmpSlr.SalaryHeadID
                                        LEFT JOIN(SELECT* FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId = '" + plantId + @"') PSH
                                                                       ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = EmpSlr.SalaryHeadID

                                                WHERE EEI.GroupID = '" + companyGroupId + @"' AND  EmpSlr.PlantId = '" + plantId + @"'";

                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"AND EmpSlr.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";

                        }
                    }
                }
                catch (Exception)
                {
                }
                strSQL += "ORDER BY EmpSystemId ";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);

                distinctSalaryHead = dsRef.Tables[0].DefaultView.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo", "PartOfNetPay", "IsCTCComponent", "IsGrossComponent");
                distinctSalaryHead.DefaultView.Sort = "Sequence";
                distinctSalaryHead = distinctSalaryHead.DefaultView.ToTable();

                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpSystemID"].ToString();
                }

                return dicBonus;


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //objCon = null;
            }
        }//End Function
        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 4;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 7;
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;

            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void CreateDynamicSHead(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out Dictionary<string, SalaryHeadSequence> list)
        {
            try
            {
                list = new Dictionary<string, SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                _count_deducting_head = 0;
                _count_earning_ctchead = 0;
                int countGrossPostion = 0;
                string deductionFormula = "";

                xlsCol += 1;
                countGrossPostion++;

                int countCTCPosition = countGrossPostion;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop ctc
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {

                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "Net Payable".ToUpper())
                        {
                            _total_head_count++;


                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();

                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.FontName = "Arial Narrow";
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Size = 10;
                            //sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;

                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
                            salaryHeadSequence.IsNetPayEffect = Convert.ToBoolean(dtSalaryHead.Rows[ci]["PartOfNetPay"]);
                            salaryHeadSequence.IsGrossComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsGrossComponent"]);
                            salaryHeadSequence.IsCTCComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsCTCComponent"]);

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);
                            countCTCPosition++;
                        }


                    }//SalaryHead 
                    #endregion
                }//for
                xlsCol += 1;


                _count_earning_ctchead = countCTCPosition - 1;

                int countDeductionPosition = countCTCPosition - 1;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region deduction
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        //{
                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
                        {
                            _total_head_count++;
                            countDeductionPosition++;

                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 10;
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = "Arial Narrow";
                            //sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;


                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
                            if (deductionFormula.Length == 0)
                            {
                                deductionFormula += salaryHeadSequence.XLColIndex.ToString();
                            }
                            else
                            {
                                deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                            }

                            //countDeductionPosition++;

                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;

                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();


                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);

                            _count_deducting_head++;
                        }
                        //}//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SetCellTextAttdn(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            //string NumberFormatString = "#,##0;(#,##0)";
            //if (string.IsNullOrEmpty(Value.to) == false)
            //{
            // if (dvSlrProc[i]["SalaryHeadID"].ToString() == "SHD2017-1" & string.IsNullOrEmpty(dvSlrProc[i]["SalaryHeadID"].ToString()) == false)
            // ColBasSlr += Convert.ToDecimal(dvSlrProc[i]["DisbusmentAmount"].ToString());

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //}
        }
        private void SetCellTextNumber(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            //string NumberFormatString = "#,##0;(#,##0)";
            //if (string.IsNullOrEmpty(Value.to) == false)
            //{
            // if (dvSlrProc[i]["SalaryHeadID"].ToString() == "SHD2017-1" & string.IsNullOrEmpty(dvSlrProc[i]["SalaryHeadID"].ToString()) == false)
            // ColBasSlr += Convert.ToDecimal(dvSlrProc[i]["DisbusmentAmount"].ToString());

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //}
        }

        public IWorkbook GetEmployeeSalaryProcessedReportSalaryLogWiseSalaryPayableInVoucher(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet, string voucherId, string Mode, string EmpBank)
        {
            #region Variable
            clsReport objRpt = null;

            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsEmpLoyeeInfo = null;
            DataTable dtEmployees = null;

            DataView dvSlrSheet = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            int endGenericColumn = 0;

            var reportUtility = new ReportUtility();
            // var excelEngine = new ExcelEngine();
            //var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            // workbook.Version = ExcelVersion.Excel2013;
            // var sheet = workbook.Worksheets[0];
            #endregion Variable

            try
            {
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
                var fdateOfMonth = "1" + "-" + monthName + "-" + year;
                string strPath = "";
                Image companyLogo = null;

                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();

                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet

                //sheet.Name = "Voucher";



                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(plantId, parameters, month.ToInt(), year.ToInt(), out dsExtraAbsent);

                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);


                //Sql Salary Structure 
                List<SalarySheetReportDisbursement> listdsSlrStr = new List<SalarySheetReportDisbursement>();

                //Sql Salary Process 
                DataTable dtSalaryHeadSheet;
                List<SalarySheetReportDisbursement> listdsSlrProc = new List<SalarySheetReportDisbursement>();

                GetEmployeeInfoDetailSalaryLogWiseSalaryPayable(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo, voucherId);//Sql Query For Salary  Data
                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetail(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, out dtSalaryHeadSheet);
                if (dsEmpLoyeeInfo.Tables[0].Rows.Count == 0)
                    throw new Exception("No Data Found..");
                if (dicEmpSalry.First().Value[0].Table.Rows.Count > 0)
                {
                    listdsSlrProc = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportDisbursement>();
                    listdsSlrStr = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportDisbursement>();
                    dtEmployees = dsEmpLoyeeInfo.Tables[0];//dicEmpSalry.First().Value[0].Table;
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                dvSlrSheet = new DataView();

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(2);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                

                #region Column Variables
                //xlsRow++;
                xlsRow++;

                int ColSr = 0, ColIDNo = 0, ColName = 0, ColDOJ = 0, ColDOS = 0, cDept = 0, cSec = 0, cSubSec = 0, cLine = 0, cPayrollGroup = 0, cJobLocation = 0, cGender = 0,
                    cGrade = 0, ColGVDG = 0, ColGrs = 0, colPayDays = 0, ColPdDy = 0, ColLate = 0, ColAbDy = 0, ColHlDy = 0, ColWkOf = 0, ColLv = 0, ColMLv = 0
                   , ColLWP = 0, colBank = 0, cDMP = 0, colBankAccountNo = 0, ColExtraAbsent = 0, colEmpCurrentStat = 0, colEmpStatus = 0, cPaymentMode = 0, cUnit = 0, ColTotalOTHR = 0, colDirectManpowerCost = 0;
                int npstruct = 0;

                #endregion

                //1
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 12);
                SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 17);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOS, 12);
                SetCellValue("EmployeeCurrentStatus", sheet1, xlsRow, ref xlsCol, out colEmpCurrentStat, 12);
                SetCellValue("EmployeeSatatus", sheet1, xlsRow, ref xlsCol, out colEmpStatus, 12);
                SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out cGender, 12);
                SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 25);
                SetCellValue("Employee Category", sheet1, xlsRow, ref xlsCol, out int colEmpCategory, 25);
                SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out cDept, 25);
                SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out cSec, 25);
                SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out cSubSec, 25);
                SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out cUnit, 25);
                SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out cLine, 25);
                SetCellValue("JobLocation", sheet1, xlsRow, ref xlsCol, out cJobLocation, 25);
                SetCellValue("Payroll group", sheet1, xlsRow, ref xlsCol, out cPayrollGroup, 25);
                //SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Bank", sheet1, xlsRow, ref xlsCol, out colBank, 25);
                SetCellValue("Bank Acc No.", sheet1, xlsRow, ref xlsCol, out colBankAccountNo, 25);
                SetCellValue("IFSCCode", sheet1, xlsRow, ref xlsCol, out int colBankIFSCCode, 25);

                SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out cGrade, 25);
                //SetCellValue("Direct Manpower", sheet1, xlsRow, ref xlsCol, out cDMP, 25);
                SetCellValue("Direct Manpower Cost", sheet1, xlsRow, ref xlsCol, out colDirectManpowerCost, 25);

                SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 5);
                SetCellValue("Present", sheet1, xlsRow, ref xlsCol, out ColPdDy, 9);
                SetCellValue("Late", sheet1, xlsRow, ref xlsCol, out ColLate, 9);
                SetCellValue("Absent", sheet1, xlsRow, ref xlsCol, out ColAbDy, 9);
                SetCellValue("LWP", sheet1, xlsRow, ref xlsCol, out ColLWP, 9);
                SetCellValue("Extra Absent", sheet1, xlsRow, ref xlsCol, out ColExtraAbsent, 9);
                SetCellValue("Holiday", sheet1, xlsRow, ref xlsCol, out ColHlDy, 9);
                SetCellValue("WeekOff", sheet1, xlsRow, ref xlsCol, out ColWkOf, 9);
                SetCellValue("Leave", sheet1, xlsRow, ref xlsCol, out ColLv, 11);
                SetCellValue("Maternity Leave", sheet1, xlsRow, ref xlsCol, out ColMLv, 20);
                SetCellValue("Total Ot Hr", sheet1, xlsRow, ref xlsCol, out ColTotalOTHR, 11);
                SetCellValue("Payable", sheet1, xlsRow, ref xlsCol, out int ColPayable, 11);
                SetCellValue("Payable Voucher No", sheet1, xlsRow, ref xlsCol, out int ColPayableVoucherNo, 11);
                SetCellValue("Disbursement", sheet1, xlsRow, ref xlsCol, out int ColDisbursement, 11);
                SetCellValue("Disbursement Voucher No", sheet1, xlsRow, ref xlsCol, out int ColDisbursementVoucherNo, 11);
                endGenericColumn = xlsCol;

                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColDisbursementVoucherNo].Merge();
                //xlsCol += 1;
                ColGrs = ColDisbursementVoucherNo;
                // 9

                var _count_earning_head = 0;
                var _count_earning_ctchead = 0;
                var _count_deducting_head = 0;
                var _total_head_count = 0;

                Dictionary<string, SalaryHeadSequence> shtList = null;

                CreateDynamicSHead(dtSalaryHeadSheet, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out shtList);

                List<SalaryHeadSequence> salList = new List<SalaryHeadSequence>();
                salList.AddRange(shtList.Values);

                xlsCol--;

                //Header Col
                if (_count_earning_ctchead > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning head";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_head + _count_earning_ctchead].Merge();
                }

                var ds = ColGrs + 1 + _count_earning_head + _count_earning_ctchead;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction head";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                npstruct = 0;
                if (shtList.Count > 0)
                {
                    xlsCol++;
                    npstruct = ColGrs + shtList.Count + 1;
                    sheet1.Range[xlsRow + 1, npstruct].Text = "Net Payable";
                    //sheet1.Range[xlsRow, npstruct].ColumnWidth = 14;
                    //sheet1.Range[xlsRow, npstruct, xlsRow + 1, npstruct].Merge();
                }

                xlsCol++;


                xlsCol++;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                sheet1.Range[xlsRow, 1, xlsRow + 1, npstruct].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].VerticalAlignment = ExcelVAlign.VAlignCenter;
                endXlsCol = npstruct;


                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;

                string FactoryAddress = string.Empty;
                try
                {

                    if (companyLogo != null)
                    {
                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);

                    }
                }
                catch (Exception ex)
                {
                }


                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Salary Sheet For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    try
                    {
                        SrNo += 1;
                        x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();

                        //1
                        sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOS"].ToString();
                        sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpCurrentStat].Text = dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpCurrentStat].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCurrentStat].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpStatus].Text = dtEmployees.Rows[i]["EmployeeStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpStatus].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                            sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmpCategoryName"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEmpCategory].Text = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                        sheet1.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4.2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DepartmentName"].ToString()) == false)
                            sheet1.Range[xlsRow, cDept].Text = dtEmployees.Rows[i]["DepartmentName"].ToString();
                        sheet1.Range[xlsRow, cDept].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cDept].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSec].Text = dtEmployees.Rows[i]["SectionName"].ToString();
                        sheet1.Range[xlsRow, cSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSec].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SubSectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSubSec].Text = dtEmployees.Rows[i]["SubSectionName"].ToString();
                        sheet1.Range[xlsRow, cSubSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSubSec].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["UnitName"].ToString()) == false)
                            sheet1.Range[xlsRow, cUnit].Text = dtEmployees.Rows[i]["UnitName"].ToString();
                        sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PaymentMode"].ToString()) == false)
                            sheet1.Range[xlsRow, cPaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
                        sheet1.Range[xlsRow, cPaymentMode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankName"].ToString()) == false)
                            sheet1.Range[xlsRow, colBank].Text = dtEmployees.Rows[i]["BankName"].ToString();
                        sheet1.Range[xlsRow, colBank].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBank].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (!string.IsNullOrEmpty(Mode) && Mode == dtEmployees.Rows[i]["PaymentMode"].ToString())
                        {
                            if (!string.IsNullOrEmpty(EmpBank)/* && EmpBank == dtEmployees.Rows[i]["BankName"].ToString()*/)
                            {
                                if (EmpBank == dtEmployees.Rows[i]["BankName"].ToString())
                                {
                                    //sheet1.Range[xlsRow, colBank].CellStyle.ColorIndex = ExcelKnownColors.Green;
                                    sheet1.Range[xlsRow, 1, xlsRow , npstruct].CellStyle.FillBackground = ExcelKnownColors.Light_green;
                                }
                                else
                                {
                                    
                                }
                            }
                            else
                            {
                                //sheet1.Range[xlsRow, cPaymentMode].CellStyle.ColorIndex = ExcelKnownColors.Green;
                                sheet1.Range[xlsRow, 1, xlsRow , npstruct].CellStyle.FillBackground = ExcelKnownColors.Light_green;
                            }
                        }
                        else
                        {
                            
                        }

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankAccNo"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankAccountNo].Text = dtEmployees.Rows[i]["BankAccNo"].ToString();
                        sheet1.Range[xlsRow, colBankAccountNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankAccountNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["IFSCCode"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankIFSCCode].Text = dtEmployees.Rows[i]["IFSCCode"].ToString();
                        sheet1.Range[xlsRow, colBankIFSCCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankIFSCCode].VerticalAlignment = ExcelVAlign.VAlignCenter;



                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["JobLocation"].ToString()) == false)
                            sheet1.Range[xlsRow, cJobLocation].Text = dtEmployees.Rows[i]["JobLocation"].ToString();
                        sheet1.Range[xlsRow, cJobLocation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cJobLocation].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LineName"].ToString()) == false)
                            sheet1.Range[xlsRow, cLine].Text = dtEmployees.Rows[i]["LineName"].ToString();
                        sheet1.Range[xlsRow, cLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cLine].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                            sheet1.Range[xlsRow, cPayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                        sheet1.Range[xlsRow, cPayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GradeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, cGrade].Text = dtEmployees.Rows[i]["GradeCode"].ToString();
                        sheet1.Range[xlsRow, cGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DirectManpowerCost"].ToString()) == false)
                            sheet1.Range[xlsRow, colDirectManpowerCost].Text = dtEmployees.Rows[i]["DirectManpowerCost"].ToString();
                        sheet1.Range[xlsRow, colDirectManpowerCost].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colDirectManpowerCost].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["IsLock"].ToString()) == false)
                            sheet1.Range[xlsRow, ColPayable].Text = dtEmployees.Rows[i]["IsLock"].ToString();

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayableVoucherId"].ToString()) == false)
                            sheet1.Range[xlsRow, ColPayableVoucherNo].Text = dtEmployees.Rows[i]["PayableVoucherId"].ToString();

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["IsDisburse"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDisbursement].Text = dtEmployees.Rows[i]["IsDisburse"].ToString();

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DisbursementVoucherId"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDisbursementVoucherNo].Text = dtEmployees.Rows[i]["DisbursementVoucherId"].ToString();

                        //5 "Section", "SubSection", 

                        #endregion
                        #region Attendance Data
                        //if (dtEmpAttdnInfo.Rows.Count > 0)
                        //{
                        double _ExtraAbsent = 0;
                        dvExtraAbsent.RowFilter = "EmpSystemID='" + dtEmployees.Rows[i]["EmpSystemID"].ToString() + "' ";
                        _ExtraAbsent = dvExtraAbsent.Count;
                        var payDays = 0.00;
                        // clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"]);
                        if (!String.IsNullOrEmpty(dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                        {
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());

                            }
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());
                            }
                        }
                        else
                        {
                            payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString());
                        }
                        SetCellTextAttdn(sheet1, xlsRow, colPayDays, payDays);
                        SetCellTextAttdn(sheet1, xlsRow, ColPdDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalPresent"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLate, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLate"].ToString()));
                        SetCellTextNumber(sheet1, xlsRow, ColAbDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLWP, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColExtraAbsent, _ExtraAbsent);
                        SetCellTextAttdn(sheet1, xlsRow, ColHlDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColWkOf, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLv"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColMLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalMLv"].ToString()));

                        SetCellTextAttdn(sheet1, xlsRow, ColTotalOTHR, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalOTHr"].ToString()) / 60);

                        //}
                        #endregion


                        //var _total_head_count_body = 0;

                        #region ------------------------------------Salary Sheet----------------------------------
                        if (dicEmpSalry.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
                        {
                            List<DataRow> drSalaryHeadCollection = dicEmpSalry[dtEmployees.Rows[i]["EmpSystemID"].ToString()];
                            if (drSalaryHeadCollection.Count > 0)
                            {
                                for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                                {
                                    if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
                                    {
                                        sheet1.Range[xlsRow, npstruct].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                        continue;
                                    }
                                    try
                                    {
                                        SalaryHeadSequence xx = shtList[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                                        if (xx != null)
                                        {
                                            if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
                                            {
                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1);
                                            }

                                            else
                                            {

                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                            }

                                            sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                                            sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                            sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        throw ex;
                                    }

                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }


                    #endregion

                    xlsRow++;
                }//for emp count
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "EmpSalaryInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion

                workbook.Version = ExcelVersion.Excel2016;
                //var strFileName = "EmpSalaryStrSheet-" + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + Convert.ToDateTime(fdateOfMonth).ToString("yyyy") + "-" + para.SalaryProcessId + ".xls";

                if (isTopSheet == true)
                {
                    #region Salary Summary
                    string filePath = HostingEnvironment.MapPath("~/") + "TempSalaeySummary.xlsx";
                    workbook.SaveAs(filePath);
                    workbook = application.Workbooks.Open(filePath);

                    IWorksheet worksheet = workbook.Worksheets[0];
                    worksheet.Move(1);

                    #region PivotSheet1
                    IWorksheet pivotSheet = workbook.Worksheets[0];
                    pivotSheet.Name = "Summary";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet.GetColumnWidth(1) + pivotSheet.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet.GetRowHeight(1) + pivotSheet.GetRowHeight(2) + pivotSheet.GetRowHeight(3) + pivotSheet.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    xlsRow += 1;
                    pivotSheet.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    #endregion

                    pivotSheet.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                    IRange iRange = worksheet["A7:" + clsStaticInfo.GetxlsCol(npstruct) + (sheetEndXlsRow)];
                    IPivotCache cache2 = workbook.PivotCaches.Add(iRange);
                    IPivotCache cache = workbook.PivotCaches.Add(iRange);


                    #region Second Pivot table
                    pivotSheet.Range[xlsRow + 2, 1].Text = "EmployeeStatus, PaymentMode, Department Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, 1, xlsRow + 2, 5].Merge();
                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Size = 12;

                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable2 = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A8"], cache);

                    pivotTable2.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cPaymentMode - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cDept - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable2_1 = pivotSheet.PivotTables["PivotTable2"];
                    pivotTable2_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable2_1.Options.ShowDrillIndicators = false;

                    pivotTable2_1.DisplayFieldCaptions = true;

                    //Add data field
                    IPivotField field2 = pivotTable2_1.Fields[ColSr - 1];
                    pivotTable2_1.DataFields.Add(field2, "Total Employees", PivotSubtotalTypes.Count);
                    int pivotColumnCount = 0;
                    IPivotField fieldGross = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                            }
                            try
                            {
                                if (ob.HeadType == "D")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            catch (Exception ex)
                            {

                                //throw ex;
                            }

                        }
                    }
                    try
                    {
                        fieldGross = null;
                        pivotColumnCount++;
                        fieldGross = pivotTable2_1.Fields[npstruct - 1];
                        pivotTable2_1.DataFields.Add(fieldGross, "Net Payable", PivotSubtotalTypes.Sum);
                        fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    }
                    catch (Exception)
                    {

                    }

                    pivotTable2_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    int totalColumns = pivotTable2_1.RowFields.Count + pivotColumnCount;

                    int lastCloumn = totalColumns + 2;

                    #endregion
                    #region PivotTable2

                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "Employee Category Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[cDept - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable1 = pivotSheet.PivotTables["PivotTable1"];
                    pivotTable1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable1.Options.ShowDrillIndicators = false;

                    pivotTable1.DisplayFieldCaptions = true;
                    pivotTable1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    //Add data field
                    IPivotField field = pivotTable.Fields[ColSr - 1];
                    pivotTable.DataFields.Add(field, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot2ColumnCount = 0;
                    IPivotField fieldGross2 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross2 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross2 = null;
                    pivot2ColumnCount++;
                    fieldGross2 = pivotTable.Fields[npstruct - 1];
                    pivotTable.DataFields.Add(fieldGross2, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");


                    #endregion

                    #region PivotTable3

                    totalColumns += pivotTable.RowFields.Count + pivot2ColumnCount;


                    lastCloumn += totalColumns - 10;

                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "EmployeeStatus ,Employee Category and Department Wise  Salary Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;

                    //Create "PivotTable1" with the cache at the specified range
                    IPivotTable pivotTable3 = pivotSheet.PivotTables.Add("PivotTable13", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable3.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[cDept - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable13_1 = pivotSheet.PivotTables["PivotTable13"];
                    pivotTable13_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable13_1.Options.ShowDrillIndicators = false;

                    pivotTable13_1.DisplayFieldCaptions = true;
                    pivotTable13_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;


                    //Add data field
                    IPivotField fields3 = pivotTable13_1.Fields[ColSr - 1];
                    pivotTable13_1.DataFields.Add(fields3, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot3ColumnCount = 0;
                    IPivotField fieldGross3 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross3 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot3ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }

                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross3 = null;
                    pivot2ColumnCount++;
                    fieldGross3 = pivotTable13_1.Fields[npstruct - 1];
                    pivotTable13_1.DataFields.Add(fieldGross3, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");


                    #endregion
                    pivotSheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet.IsGridLinesVisible = false;
                    pivotSheet.IsDisplayZeros = false;

                    pivotSheet.UsedRange.WrapText = false;

                    #endregion
                    #endregion

                    workbook.ActiveSheetIndex = 0;
                }

                return workbook;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objRpt = null;
                //excelEngine = null;
                //application = null;
                //workbook = null;
            }
        }
        public GridModel GetSalaryPayableDisbursementVoucherList(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"SELECT V.Id PayableVoucherId, V.VoucherDate, V.PostingDate, V.DocRefNo, V.VoucherTypeId, V.CurrencyId, V.DocDate, V.EntityId, C.Code AS CurrencyCode
                                    , VD.DrAmount, V.VoucherNo,ISNULL(BM.AccountTitle,CM.UserName) PaymentBank
                                    ,V.IsPark,Status= case when V.IsPark=0 then 'Posted' else 'Parked' end, V.Narration
									,[Month]=case when sl.MonthNo=1 then 'January'
                                    when sl.MonthNo=2 then 'February'
                                    when sl.MonthNo=3 then 'March'
                                    when sl.MonthNo=4 then 'April'
                                    when sl.MonthNo=5 then 'May'
                                    when sl.MonthNo=6 then 'June'
                                    when sl.MonthNo=7 then 'July'
                                    when sl.MonthNo=8 then 'August'
                                    when sl.MonthNo=9 then 'September'
                                    when sl.MonthNo=10 then 'October'
                                    when sl.MonthNo=11 then 'November'
                                    when sl.MonthNo=12 then 'December' end,sl.MonthNo ,sl.YearNo
                                    FROM TRN.[Voucher] AS V
                                    LEFT JOIN SCS.Currency AS C ON C.Id = V.CurrencyId
                                    LEFT JOIN (SELECT SUM(VD.DrAmount) AS DrAmount, VD.VoucherId,VD.BankMasterId FROM [TRN].[VoucherDetail] AS VD WHERE VD.DrAmount <> 0 
									GROUP BY VD.VoucherId,VD.BankMasterId
                                    ) AS VD ON VD.VoucherId=V.Id
									left join (select distinct sl.DisbursementVoucherId,sl.MonthNo,sl.YearNo
									from dbo.SalaryLock sl
									where sl.DisbursementVoucherId<>'' and sl.IsDisbursed=1 
									) sl on sl.DisbursementVoucherId=v.Id
									LEFT JOIN TRN.VoucherDetail XVD ON XVD.VoucherId=V.Id AND XVD.BankMasterId<>''
									LEFT JOIN TRN.VoucherDetail XVDC ON XVDC.VoucherId=V.Id AND  XVDC.CashMasterId<>''
									left join MST.BankMaster BM ON BM.Id=XVD.BankMasterId
									left join MST.CashMaster CM ON CM.Id=XVDC.CashMasterId
                                    WHERE  V.Archive=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + "'AND V.CompanyId='" + identity.CompanyId + "' AND V.PlantId='" + identity.PlantId + "' AND V.SourceType='" + SourceType.SalaryDisbursement + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex);
            }
        }
        public GridModel GetGoodWorkPaymentAdviseDisbursementVoucherList(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"SELECT V.Id PayableVoucherId, V.VoucherDate, V.PostingDate, V.DocRefNo, V.VoucherTypeId, V.CurrencyId, V.DocDate, V.EntityId, C.Code AS CurrencyCode
                                    , VD.DrAmount, V.VoucherNo,ISNULL(BM.AccountTitle,CM.UserName) PaymentBank
                                    ,V.IsPark,Status= case when V.IsPark=0 then 'Posted' else 'Parked' end, V.Narration
									,GWPA.PaymentAdviseId
                                    FROM TRN.[Voucher] AS V
                                    LEFT JOIN SCS.Currency AS C ON C.Id = V.CurrencyId
                                    LEFT JOIN (SELECT SUM(VD.DrAmount) AS DrAmount, VD.VoucherId,VD.BankMasterId FROM [TRN].[VoucherDetail] AS VD WHERE VD.DrAmount <> 0 
									GROUP BY VD.VoucherId,VD.BankMasterId
                                    ) AS VD ON VD.VoucherId=V.Id
									INNER JOIN (select distinct GWPD.DisbursementVoucherId,GWPD.PaymentAdviseId
									from dbo.GoodWorkPaymentAdviseDetail GWPD
									where GWPD.DisbursementVoucherId<>'' and GWPD.IsDisburse=1 
									) GWPA on GWPA.DisbursementVoucherId=v.Id
									LEFT JOIN TRN.VoucherDetail XVD ON XVD.VoucherId=V.Id AND XVD.BankMasterId<>''
									LEFT JOIN TRN.VoucherDetail XVDC ON XVDC.VoucherId=V.Id AND  XVDC.CashMasterId<>''
									left join MST.BankMaster BM ON BM.Id=XVD.BankMasterId
									left join MST.CashMaster CM ON CM.Id=XVDC.CashMasterId
                                    WHERE  V.Archive=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + "'AND V.CompanyId='" + identity.CompanyId + "' AND V.PlantId='" + identity.PlantId + "' AND V.SourceType='" + SourceType.GoodWorkDisbursement + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex);
            }
        }
        public GridModel GetEmployeeMultipleAdvanceDisbursementVoucherList(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"SELECT V.Id PayableVoucherId, V.VoucherDate, V.PostingDate, V.DocRefNo, V.VoucherTypeId, V.CurrencyId, V.DocDate, V.EntityId, C.Code AS CurrencyCode
                                    , VD.DrAmount, V.VoucherNo,ISNULL(BM.AccountTitle,CM.UserName) PaymentBank
                                    ,V.IsPark,Status= case when V.IsPark=0 then 'Posted' else 'Parked' end, V.Narration
									,GWPA.PaymentAdviseId
                                    FROM TRN.[Voucher] AS V
                                    LEFT JOIN SCS.Currency AS C ON C.Id = V.CurrencyId
                                    LEFT JOIN (SELECT SUM(VD.DrAmount) AS DrAmount, VD.VoucherId,VD.BankMasterId FROM [TRN].[VoucherDetail] AS VD WHERE VD.DrAmount <> 0 
									GROUP BY VD.VoucherId,VD.BankMasterId
                                    ) AS VD ON VD.VoucherId=V.Id
									INNER JOIN (select distinct GWPD.DisbursementVoucherId,GWPD.WorkerAdvanceId PaymentAdviseId
									from dbo.WorkerAdvanceDetail GWPD
									where GWPD.DisbursementVoucherId<>'' and GWPD.IsDisburse=1 
									) GWPA on GWPA.DisbursementVoucherId=v.Id
									LEFT JOIN TRN.VoucherDetail XVD ON XVD.VoucherId=V.Id AND XVD.BankMasterId<>''
									LEFT JOIN TRN.VoucherDetail XVDC ON XVDC.VoucherId=V.Id AND  XVDC.CashMasterId<>''
									left join MST.BankMaster BM ON BM.Id=XVD.BankMasterId
									left join MST.CashMaster CM ON CM.Id=XVDC.CashMasterId
                                    WHERE  V.Archive=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + "'AND V.CompanyId='" + identity.CompanyId + "' AND V.PlantId='" + identity.PlantId + "' AND V.SourceType='" + SourceType.GoodWorkDisbursement + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex);
            }
        }
        public GridModel GetFinalSettlementDisbursementVoucherList(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"SELECT V.Id PayableVoucherId, V.VoucherDate, V.PostingDate, V.DocRefNo, V.VoucherTypeId, V.CurrencyId, V.DocDate, V.EntityId, C.Code AS CurrencyCode
                                    , VD.DrAmount, V.VoucherNo,ISNULL(BM.AccountTitle,CM.UserName) PaymentBank
                                    ,V.IsPark,Status= case when V.IsPark=0 then 'Posted' else 'Parked' end, V.Narration
									,ISNULL(E.EmployeeCode,'') EmployeeCode ,ISNULL(E.EmployeeName,'') EmployeeName	
                                    FROM TRN.[Voucher] AS V
                                    LEFT JOIN SCS.Currency AS C ON C.Id = V.CurrencyId
                                    LEFT JOIN (SELECT SUM(VD.DrAmount) AS DrAmount, VD.VoucherId,VD.BankMasterId FROM [TRN].[VoucherDetail] AS VD WHERE VD.DrAmount <> 0 
									GROUP BY VD.VoucherId,VD.BankMasterId
                                    ) AS VD ON VD.VoucherId=V.Id
									LEFT JOIN [dbo].[EmployeeFinalSettlement] EFS ON EFS.DisbursementVoucherId=V.Id 
									LEFT JOIN EmployeeInformation E on E.SystemId= EFS.EmpSystemId
									LEFT JOIN TRN.VoucherDetail XVD ON XVD.VoucherId=V.Id AND XVD.BankMasterId<>''
									LEFT JOIN TRN.VoucherDetail XVDC ON XVDC.VoucherId=V.Id AND  XVDC.CashMasterId<>''
									left join MST.BankMaster BM ON BM.Id=XVD.BankMasterId
									left join MST.CashMaster CM ON CM.Id=XVDC.CashMasterId
                                    WHERE  V.Archive=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + "'AND V.CompanyId='" + identity.CompanyId + "' AND V.PlantId='" + identity.PlantId + "' AND V.SourceType='" + SourceType.FinalSettlementJournal + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex);
            }
        }
        public GridModel GetBonusDisbursementVoucherList(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"SELECT V.Id PayableVoucherId, V.VoucherDate, V.PostingDate, V.DocRefNo, V.VoucherTypeId, V.CurrencyId, V.DocDate, V.EntityId, C.Code AS CurrencyCode
                                    , VD.DrAmount, V.VoucherNo,ISNULL(BM.AccountTitle,CM.UserName) PaymentBank,V.IsPark,Status = case when V.IsPark=0 then 'Posted' else 'Parked' end, V.Narration
									,CONCAT(DATENAME(mm, DA.FromDate), '-', DATEPART(yy, DA.FromDate))FromDate
									,CONCAT(DATENAME(mm, DA.ToDate), '-', DATEPART(yy, DA.ToDate))ToDate
                                    FROM TRN.[Voucher] AS V
                                    LEFT JOIN SCS.Currency AS C ON C.Id = V.CurrencyId
                                    LEFT JOIN (SELECT SUM(VD.DrAmount) AS DrAmount, VD.VoucherId,VD.BankMasterId FROM [TRN].[VoucherDetail] AS VD WHERE VD.DrAmount <> 0 
									GROUP BY VD.VoucherId,VD.BankMasterId
                                    ) AS VD ON VD.VoucherId=V.Id
									left join (select distinct sl.BonusDisbursementVoucherId,sl.BonusDisbursementAdviceId
									from dbo.SalaryLock sl
									where sl.BonusDisbursementVoucherId<>'' and sl.IsBonusDisbursed=1 
									) sl on sl.BonusDisbursementVoucherId=v.Id
									LEFT JOIN [dbo].[BonusDisbursementAdvice]  DA ON DA.Id=sl.BonusDisbursementAdviceId
									LEFT JOIN TRN.VoucherDetail XVD ON XVD.VoucherId=V.Id AND XVD.BankMasterId<>''
									LEFT JOIN TRN.VoucherDetail XVDC ON XVDC.VoucherId=V.Id AND  XVDC.CashMasterId<>''
									left join MST.BankMaster BM ON BM.Id=XVD.BankMasterId
									left join MST.CashMaster CM ON CM.Id=XVDC.CashMasterId
                                    WHERE  V.Archive=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + "'AND V.CompanyId='" + identity.CompanyId + "' AND V.PlantId='" + identity.PlantId + "' AND V.SourceType='" + SourceType.BonusDisbursement + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex);
            }
        }
        private Dictionary<string, object> GetJournalHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode
                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ";
            return _sqlRepository.GetData(cmdText);
        }
        private DataTable GetJournalData(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, GL.AccountCode, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                            , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                            , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                            , VD.Id AS BudgetMasterId, BUD.UserName AS BudgetName, ACT.UserName AS Activity, UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType, VD.FAType,VD.FixedAssetMasterId
							,[ParticularName]=CASE
								WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
								WHEN CM.UserName<>'' THEN CM.UserName
								WHEN VD.EmployeeId<>'' THEN  VD.TrnNature +' ( '+ISNULL(EI.EmployeeName,'')+' ) '
								ELSE VD.TrnNature	END
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BMT ON VD.BudgetMasterId=BMT.Id
                            LEFT JOIN [HKP].[Budget] BUD ON BUD.Id=BMT.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
							LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
							LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
							LEFT JOIN [DBO].EmployeeInformation AS EI ON EI.SystemId=VD.EmployeeId
							LEFT JOIN [MST].BankMaster AS BM ON BM.Id=VD.BankMasterId
							LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=VD.FixedAssetMasterId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetSalaryPayableVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);

            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.SalaryPayable);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetJournalData(companyGroupId, companyId, plantId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            accountsCommonService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colParticulars = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Entry Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Status");
            reportUtility.SetText(ref sheet, row, 2, header["Status"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString(), ExcelHAlign.HAlignLeft);
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
                sheet[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].BorderAround(ExcelLineStyle.Thin); ;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 12, ExcelHAlign.HAlignRight);

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 12); colParticulars = xlsCol; xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["BudgetName"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                    glName = string.Empty;

                }


                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                var lastRow = row - 1;

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (lastRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (lastRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                sheet.UsedRange.AutofitColumns();
                sheet[1, 2].ColumnWidth = 40;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        public IWorkbook GetSalaryDisbursementVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);

            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.SalaryDisbursement);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetJournalData(companyGroupId, companyId, plantId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            accountsCommonService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colParticulars = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Entry Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Status");
            reportUtility.SetText(ref sheet, row, 2, header["Status"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString(), ExcelHAlign.HAlignLeft);
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
                sheet[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].BorderAround(ExcelLineStyle.Thin); ;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 12, ExcelHAlign.HAlignRight);

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 12); colParticulars = xlsCol; xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["BudgetName"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                    glName = string.Empty;

                }


                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                var lastRow = row - 1;

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (lastRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (lastRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                sheet.UsedRange.AutofitColumns();
                sheet[1, 2].ColumnWidth = 40;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        #endregion
    }
    class SalarySheetReportDisbursement
    {
        public string EmpSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public string HeadCategory { get; set; }
        public decimal DisbusmentAmount { get; set; } = 0;
        public decimal EntryAmount { get; set; } = 0;
    }
}
