using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.NewOTProcess
{
   public class OTControlLimitService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public OTControlLimitService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> GetLastEffectiveDate()
        {
            try
            {
                string sql = @"SELECT TOP (1) FORMAT(EffectiveDate,'dd-MMM-yyyy')EffectiveDate FROM [dbo].[OTControlLimit] A ORDER BY EffectiveDate";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetBudgetDataToUpload()
        {
            try
            {
                string sql = @"SELECT DISTINCT mb.Id BudgetCodeId,mb.Deployment,mb.Code BudgetCode,p.Code PositionCode,DGM.EmployeeCategory,E.UserName Entity,d.UserName Department,s.UserName Section,ss.UserName SubSection,DG.UserName Designation,p.Activity,sd.ShiftDefinationName
        ,ei.EmployeeName ResponsiblePerson,OT.DailyOTLimit,OT.WeeklyOTLimit,OT.WeekOffOTLimit,OT.MonthlyOTLimit,'' Remarks
        ,mb.ROBudgetCode,mb.PRBudgetCode
        ,ag.UserName AttendanceGroup,P.UserDefineGroup2,Direct=CASE WHEN P.IsDirect=1 THEN 'Yes' ELSE 'No' END,ISNULL(ONR.ONRoll,0)OnRoll,l.UserName Line,mbd.TotalNumber
       FROM MST.ManpowerBudget AS mb
        LEFT JOIN ORG.Entity E ON E.Id=mb.EntityId
        LEFT JOIN ORG.Position AS p ON P.Id=mb.PositionId
        LEFT JOIN ORG.Department AS d ON d.Id=p.DepartmentId
        LEFT JOIN ORG.Section AS S ON S.Id=p.SectionId
        LEFT JOIN ORG.SubSection AS SS ON SS.Id=p.SubSectionId
        LEFT JOIN ORG.Line AS L ON L.Id=mb.LineId
        LEFT JOIN HKP.Designation DG ON DG.Id=P.DesignationId 
        LEFT JOIN dbo.ShiftDefination AS sd ON sd.SystemID=mb.ShiftDefinationId
        LEFT JOIN dbo.AttendanceGroup AS ag ON ag.Id=mb.AttendanceGroupId
        LEFT JOIN MST.ManpowerBudgetDetail mbd ON mbd.ManpowerBudgetId=mb.Id
        LEFT JOIN (
        SELECT dm.DesignationId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
        LEFT JOIN SCS.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=dm.Id
        LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
        WHERE dmc.IsOTEntitled=1                            
        ) DGM ON DGM.DesignationId=DG.Id
        LEFT JOIN dbo.EmployeeInformation AS ei ON ei.SystemId=mb.ResponsiblePerson
       LEFT JOIN (SELECT COUNT(SystemId)OnRoll,BudgetCode FROM dbo.EmployeeInformation GROUP BY BudgetCode) ONR ON ONR.BudgetCode=mb.Id       
        LEFT JOIN (SELECT * FROM  [dbo].[OTControlLimitDetail] WHERE OTControlLimitId IN(SELECT TOP (1) ID FROM [dbo].[OTControlLimit] A ORDER BY EffectiveDate DESC)) OT ON OT.BudgetCodeId=MB.Id       
        WHERE mb.Active=1";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void SaveData(Dictionary<string, object> data, List<Dictionary<string, object>> detailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsMaster, dsDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.OTControlLimit WHERE EffectiveDate='" + data["EffectiveDate"] + "'", out dsMaster, false, "1");

                string _Id = "";
                string masterId = "";

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OTControlLimit", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    data["Id"] = masterId;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                //con.OpenDataSetThroughAdapter("SELECT * FROM dbo.OTControlLimitDetail WHERE OTControlLimitId ='" + masterId + "'", out dsDetail, false, "1");
                con.OpenDataSetThroughAdapter(@"SELECT * FROM dbo.OTControlLimitDetail WHERE OTControlLimitId=(SELECT ID FROM [dbo].[OTControlLimit] Where EffectiveDate='"+ data["EffectiveDate"] + "')", out dsDetail, false, "1");

                while (dsDetail.Tables[0].DefaultView.Count > 0)
                    dsDetail.Tables[0].DefaultView[0].Delete();

                int count = 0;
                foreach (var item in detailList)
                {
                    count++;
                    DataView dv = new DataView(dsDetail.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = masterId + "-" + count;
                        item["OTControlLimitId"] = masterId;

                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["OTControlLimitId"] = masterId;
                        EditRow(drmo, item);
                    }
                }


                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }


        public IEnumerable<object> GetList()
        {
            string sql = @"SELECT OTL.*,ei.EmployeeName ApproveByName,eiw.EmployeeName ByWhomName FROM [dbo].[OTControlLimit] OTL
LEFT JOIN dbo.EmployeeInformation AS ei ON ei.SystemId = OTL.ApproveBy
LEFT JOIN dbo.EmployeeInformation AS eiw ON eiw.SystemId = OTL.ByWhom";
            return _sqlRepository.GetDataCollection(sql,null);
        }

        public DataTable GetOTControlLimitData(string fromDate, string todate)
        {
            string sql = @"SELECT FORMAT(apd.WorkDate,'dd-MMM-yyyy')WorkDate,e.UserName Entity,d.UserName Department,s.UserName Section,SS.UserName SubSection,DGm.EmployeeCategory,DG.UserName Designation
,p.Activity,Direct=CASE WHEN P.IsDirect=1 THEN 'Yes' ELSE 'No' END,ag.UserName AttendanceGroup,P.UserDefineGroup2, p.Code PositionCode
,SUM(CAST(mb.Deployment AS int))Deployment,SUM(mbd.TotalNumber)BudgetedManPower,COUNT(apd.EmpSystemID)ONRoll,PMP.PresentManpower,LMP.LateManpower
,ISNULL(POT.ProcessedOT,0)TotalOT
,((ISNULL(POT.ProcessedOT,0)/60)/8) OTManDays
,(ISNULL(PMP.PresentManpower,0)+ISNULL(LMP.LateManpower,0)+((ISNULL(POT.ProcessedOT,0)/60)/8)+ISNULL(EOD.OD,0)) TotalDeployedMandays
,ExcessDeploymentMandays=CASE WHEN (ISNULL(PMP.PresentManpower,0)+ISNULL(LMP.LateManpower,0)+((ISNULL(POT.ProcessedOT,0)/60)/8)+ISNULL(EOD.OD,0))> SUM(CAST(mb.Deployment AS int)) 
								THEN ((ISNULL(PMP.PresentManpower,0)+ISNULL(LMP.LateManpower,0)+((ISNULL(POT.ProcessedOT,0)/60)/8)+ISNULL(EOD.OD,0))-SUM(CAST(mb.Deployment AS int))) 
								ELSE 0 END
,ShortDeploymentMandays=CASE WHEN (ISNULL(PMP.PresentManpower,0)+ISNULL(LMP.LateManpower,0)+((ISNULL(POT.ProcessedOT,0)/60)/8)+ISNULL(EOD.OD,0))< SUM(CAST(mb.Deployment AS int)) 
								THEN (SUM(CAST(mb.Deployment AS int))-(ISNULL(PMP.PresentManpower,0)+ISNULL(LMP.LateManpower,0)+((ISNULL(POT.ProcessedOT,0)/60)/8)+ISNULL(EOD.OD,0))) 
								ELSE 0 END	
								  
,ExcessOT=CASE WHEN ISNULL(POT.ProcessedOT,0) > ISNULL(old.DailyOTLimit,0) 
								THEN ISNULL(POT.ProcessedOT,0) - ISNULL(old.DailyOTLimit,0)
								ELSE 0 END
,ShortOT=CASE WHEN ISNULL(POT.ProcessedOT,0) < ISNULL(old.DailyOTLimit,0) 
								THEN ISNULL(old.DailyOTLimit,0)-ISNULL(POT.ProcessedOT,0) 
								ELSE 0 END
,old.DailyOTLimit,old.WeeklyOTLimit,old.WeekOffOTLimit,old.MonthlyOTLimit,old.Remarks
FROM dbo.AttdnProcessData APD
LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=apd.BudgetId
LEFT JOIN MST.ManpowerBudgetDetail mbd ON mbd.ManpowerBudgetId = mb.Id
LEFT JOIN ORG.Position P ON P.Id=mb.PositionId
LEFT JOIN ORG.Entity E ON E.Id=mb.EntityId
LEFT JOIN dbo.OTControlLimitDetail AS old ON old.BudgetCodeId = mb.Id 
LEFT JOIN dbo.OTControlLimit AS ol ON ol.Id = old.OTControlLimitId  AND ol.EffectiveDate BETWEEN '" + fromDate+@"' AND '"+todate+@"' 
LEFT JOIN ORG.Department AS d ON d.Id = p.DepartmentId
LEFT JOIN ORG.Section AS S ON S.Id = p.SectionId
LEFT JOIN ORG.SubSection AS SS ON SS.Id = p.SubSectionId
LEFT JOIN HKP.Designation DG ON DG.Id=P.DesignationId 
LEFT JOIN dbo.AttendanceGroup AS ag ON ag.Id=mb.AttendanceGroupId
LEFT JOIN (
SELECT  dm.DesignationId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
LEFT JOIN SCS.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=dm.Id
LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
WHERE dmc.IsOTEntitled=1  
) DGM ON DGM.DesignationId=DG.Id
        
 JOIN (
		SELECT SUM(apd.PresentValue)PresentManpower,mb.PositionId,apd.WorkDate
		  FROM dbo.AttdnProcessData AS apd
		LEFT JOIN MST.ManpowerBudget AS mb ON mb.id=apd.BudgetId
		WHERE apd.WorkDate BETWEEN '"+fromDate+@"' AND '"+todate+@"' AND APD.DayStatus='P' GROUP BY apd.WorkDate,mb.PositionId
        ) PMP ON PMP.PositionId=MB.PositionId AND PMP.WorkDate=apd.WorkDate
        
  JOIN (
		SELECT SUM(apd.PresentValue)LateManpower,mb.PositionId,apd.WorkDate FROM dbo.AttdnProcessData AS apd
		LEFT JOIN MST.ManpowerBudget AS mb ON mb.id=apd.BudgetId
		WHERE apd.WorkDate BETWEEN '"+fromDate+@"' AND '"+todate+@"' AND APD.DayStatus='L' GROUP BY apd.WorkDate,mb.PositionId
        ) LMP ON LMP.PositionId=MB.PositionId AND LMP.WorkDate=apd.WorkDate
 JOIN (
		SELECT SUM(ISNULL(APD.ProcessedOT,0))ProcessedOT,mb.PositionId,apd.WorkDate FROM dbo.AttdnProcessData AS apd
		LEFT JOIN MST.ManpowerBudget AS mb ON mb.id=apd.BudgetId
		WHERE apd.WorkDate BETWEEN '"+fromDate+@"' AND '"+todate+ @"' GROUP BY apd.WorkDate,mb.PositionId
        ) POT ON POT.PositionId=MB.PositionId AND POT.WorkDate=apd.WorkDate
        
LEFT JOIN (
		SELECT COUNT(D.Id) OD,P.Id PositionId FROM dbo.EmployeeOnDuty AS eod
		LEFT JOIN [dbo].[EmployeeOnDutyDetails] D ON D.OndutyId=eod.Id
		LEFT JOIN EmployeeInformation E ON E.SystemId = eod.EmpSystemId
		LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=E.BudgetCode
LEFT JOIN ORG.Position P ON P.Id=mb.PositionId
		WHERE D.WorkDate BETWEEN '" + fromDate+@"' AND '"+todate+@"' GROUP BY P.Id,eod.EmpSystemId
        ) EOD ON EOD.PositionId=MB.PositionId
        
WHERE mb.Active=1 AND ISNULL(DGm.EmployeeCategory,'')<>'' AND apd.WorkDate BETWEEN '"+fromDate+@"' AND '"+todate+@"'
GROUP BY apd.WorkDate,P.Id,e.Id,e.UserName,d.UserName,s.UserName,SS.UserName,DGm.EmployeeCategory,DG.UserName
,p.Activity,P.IsDirect,ag.UserName,P.UserDefineGroup2,p.Code
,PMP.PresentManpower,LMP.LateManpower,POT.ProcessedOT,EOD.OD
,old.DailyOTLimit,old.WeeklyOTLimit,old.WeekOffOTLimit,old.MonthlyOTLimit,old.Remarks
ORDER BY apd.WorkDate";
            return _sqlRepository.GetDataTable(sql);
        }
    }
}
