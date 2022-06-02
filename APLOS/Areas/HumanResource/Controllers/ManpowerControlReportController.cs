using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using Library.HumanResource.Attendance;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ManpowerControlReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        public ManpowerControlReportController(
            ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor



        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpPost, Authorize]
        public ActionResult XlsManpowerControlReport(Dictionary<string , string> Parameters, string Dates)
        {
            try
            {
                var workbook = ManpowerControlReport(Parameters, Dates);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "MPControlReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        private IWorkbook ManpowerControlReport(Dictionary<string, string> Parameters, string Dates)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = ManpowerControlReportQuery(Parameters, Dates);

            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Manpower Control Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Company", 12, ExcelHAlign.HAlignCenter);
            int ColComapny = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 12, ExcelHAlign.HAlignCenter);
            int ColPlant = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Division", 12, ExcelHAlign.HAlignCenter);
            int ColDiv = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 12, ExcelHAlign.HAlignCenter);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 12, ExcelHAlign.HAlignCenter);
            int ColDept = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 12, ExcelHAlign.HAlignCenter);
            int ColSec = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Section", 12, ExcelHAlign.HAlignCenter);
            int ColSSec = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 12, ExcelHAlign.HAlignCenter);
            int ColDes = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Activity", 12, ExcelHAlign.HAlignCenter);
            int ColAct = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Direct/Indirect", 12, ExcelHAlign.HAlignCenter);
            int ColDir = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process", 12, ExcelHAlign.HAlignCenter);
            int ColProcess = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shift", 16, ExcelHAlign.HAlignCenter);
            int ColShift = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Position Code", 12, ExcelHAlign.HAlignCenter);
            int ColPosCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Budget Code", 12, ExcelHAlign.HAlignCenter);
            int ColBCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Budget MP", 12, ExcelHAlign.HAlignCenter);
            int ColBMP = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Deployment", 12, ExcelHAlign.HAlignCenter);
            int ColDeploy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "On Roll", 12, ExcelHAlign.HAlignCenter);
            int ColOnRol = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Long Absent", 12, ExcelHAlign.HAlignCenter);
            int ColLA = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "TBS", 12, ExcelHAlign.HAlignCenter);
            int ColTBS = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Net Available", 12, ExcelHAlign.HAlignCenter);
            int ColNetAvail = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "On Roll Short", 12, ExcelHAlign.HAlignCenter);
            int ColORShort = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "On Roll Excess", 12, ExcelHAlign.HAlignCenter);
            int ColORExcess = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Deployment Short", 12, ExcelHAlign.HAlignCenter);
            int ColDepShort = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Deployment Excess", 12, ExcelHAlign.HAlignCenter);
            int ColDepExcess = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Leave", 12, ExcelHAlign.HAlignCenter);
            int ColTotalLeave = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Net Short", 12, ExcelHAlign.HAlignCenter);
            int ColNtShort = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Net Excess", 12, ExcelHAlign.HAlignCenter);
            int ColNtExcess = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "User Report Group", 12, ExcelHAlign.HAlignCenter);
            int ColUserG = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Position Name", 25, ExcelHAlign.HAlignCenter);
            int ColPosName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Physical Verification Applicable", 12, ExcelHAlign.HAlignCenter);
            int ColPhys = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Roster Applicable", 12, ExcelHAlign.HAlignCenter);
            int ColRos = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Scattered Week Off Applicable", 12, ExcelHAlign.HAlignCenter);
            int ColScat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Payment Link", 12, ExcelHAlign.HAlignCenter);
            int ColPay = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Task Management Applicable", 12, ExcelHAlign.HAlignCenter);
            int ColTask = COL;
            COL++;


            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;

            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {
               
                sheet[ROW, ColComapny].Text = data.Rows[i]["Company"].ToString();
                sheet[ROW, ColPlant].Text = data.Rows[i]["Plant"].ToString();
                sheet[ROW, ColDiv].Text = data.Rows[i]["Division"].ToString();
                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColDept].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColSec].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSSec].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColDes].Text = data.Rows[i]["Designation"].ToString();
                sheet[ROW, ColAct].Text = data.Rows[i]["Activity"].ToString();
                sheet[ROW, ColDir].Text = data.Rows[i]["IsDirect"].ToString();
                sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                sheet[ROW, ColShift].Text = data.Rows[i]["ShiftName"].ToString();
                sheet[ROW, ColPosCode].Text = data.Rows[i]["PositionCode"].ToString();
                sheet[ROW, ColBCode].Text = data.Rows[i]["BudgetCode"].ToString();
                sheet[ROW, ColBMP].Number = clsStaticInfo.dbl(data.Rows[i]["BB"].ToString());
                sheet[ROW, ColDeploy].Number = clsStaticInfo.dbl(data.Rows[i]["Dep"].ToString());
                sheet[ROW, ColOnRol].Number = clsStaticInfo.dbl(data.Rows[i]["OnRoll"].ToString());
                sheet[ROW, ColLA].Number = clsStaticInfo.dbl(data.Rows[i]["LA"].ToString());
                sheet[ROW, ColTBS].Number = clsStaticInfo.dbl(data.Rows[i]["TBS"].ToString());
                sheet[ROW, ColNetAvail].Number = clsStaticInfo.dbl(data.Rows[i]["NetAvailable"].ToString());
                sheet[ROW, ColORShort].Number = clsStaticInfo.dbl(data.Rows[i]["OnRollShort"].ToString());
                sheet[ROW, ColORExcess].Number = clsStaticInfo.dbl(data.Rows[i]["OnRollExcess"].ToString());
                sheet[ROW, ColDepShort].Number = clsStaticInfo.dbl(data.Rows[i]["DepShort"].ToString());
                sheet[ROW, ColDepExcess].Number = clsStaticInfo.dbl(data.Rows[i]["DepExcess"].ToString());
                sheet[ROW, ColTotalLeave].Number = clsStaticInfo.dbl(data.Rows[i]["Leaves"].ToString());
                sheet[ROW, ColNtShort].Number = clsStaticInfo.dbl(data.Rows[i]["NetShort"].ToString());
                sheet[ROW, ColNtExcess].Number = clsStaticInfo.dbl(data.Rows[i]["NetExcess"].ToString());
                sheet[ROW, ColUserG].Text = data.Rows[i]["UserReportGroup"].ToString();
                sheet[ROW, ColPosName].Text = data.Rows[i]["PosName"].ToString();
                sheet[ROW, ColPhys].Text = data.Rows[i]["PhysicalVarification"].ToString();
                sheet[ROW, ColRos].Text = data.Rows[i]["IsRosterApplicable"].ToString();
                sheet[ROW, ColScat].Text = data.Rows[i]["IsScattedWeekOffApplicable"].ToString();
                sheet[ROW, ColPay].Text = data.Rows[i]["PaymentLink"].ToString();
                sheet[ROW, ColTask].Text = data.Rows[i]["TaskManagementApplicable"].ToString();

                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "Manpower Control Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }

        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(getFiltersQuery() , JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations  

        #region Queries

        private IEnumerable<object> getFiltersQuery()
        {
            try
            {
                var str = @"Select distinct e.Id as EntityId , e.UserName as Entity , pos.UserReportGroup  , p.UserName as Process ,p.Id as ProcessId , ec.Id as EmpTypeId , ec.UserName as EmpType
                            from mst.ManpowerBudget mb
                            left join org.Entity e on e.Id = mb.EntityId
                            left join org.Position pos on pos.Id = mb.PositionId
                            left join mst.DesignationMaster dm on dm.DesignationId = pos.DesignationId
                            left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                            left join hkp.Process p on p.Id = pos.ProcessId
                            ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private DataTable ManpowerControlReportQuery(Dictionary<string, string> Parameters, string Dates)
        {
            try
            {

                string proc = "''";

                string [] ProcArr = Parameters["ProcessId"].Split(',');

                foreach(string k in ProcArr)
                {
                    if(k == "'null'")
                    {
                        proc = "1=1";
                        break;
                    }
                    else if (k == "''")
                    {
                        continue;
                    }
                    else
                    {
                        proc = proc + "," + k ;
                    }
                }

                var str = @"Select Main.*,

                              abs((Main.OnRoll - Main.LA - Main.TBS)) as NetAvailable 
                            ,abs((Case when (Main.OnRoll - Main.BB) > 0 then (Main.OnRoll - Main.BB) else 0 end) ) as OnRollExcess
                            ,abs ((Case when (Main.OnRoll - Main.BB) < 0 then (Main.OnRoll - Main.BB) else 0 end) )as OnRollShort
                            ,abs ((Case when (Main.Dep - (Main.OnRoll - Main.LA - Main.TBS)) > 0 then (Main.Dep - (Main.OnRoll - Main.LA - Main.TBS)) else 0 end)) as DepExcess
                            ,abs ((Case when (Main.Dep - (Main.OnRoll - Main.LA - Main.TBS)) < 0 then (Main.Dep - (Main.OnRoll - Main.LA - Main.TBS)) else 0 end)) as DepShort
                            ,abs ( (Case when ((Main.OnRoll - Main.LA - Main.TBS) - Main.Leaves) < 0 then ((Main.OnRoll - Main.LA - Main.TBS) - Main.Leaves) else 0 end)) as NetShort
                            ,abs ( (Case when ((Main.OnRoll - Main.LA - Main.TBS) - Main.Leaves) > 0 then ((Main.OnRoll - Main.LA - Main.TBS) - Main.Leaves) else 0 end)) as NetExcess
                            from
                            (
                            Select 
                            ---Sequences
							div.Sequence as DivSeq,dept.Sequence as DeptSeq ,sec.Sequence as SecSeq , ssec.Sequence as SSecSeq, desg.Sequence as DesgSeq,
                            --General Entries
                            mb.CompanyId , mb.EntityId , c.UserName as Company , p.UserName as Plant, div.UserName as Division , e.UserName as Entity, dept.UserName as Department, sec.UserName as Section , ssec.UserName as SubSection , desg.UserName as Designation, pos.Activity, 
                            (Case when pos.isDirect = 1 then 'Direct' else 'InDirect' end) as IsDirect
                            , pp.UserName as Process,pp.Id as ProcessId,
                            mb.Id as BudgetId , shd.UserName as ShiftName, pos.Code as PositionCode, mb.Code as BudgetCode,
                            pos.UserReportGroup , pos.UserName as PosName, pos.PhysicalVarification , mb.IsRosterApplicable , mb.IsScattedWeekOffApplicable,pos.PaymentLink,pos.TaskManagementApplicable,dm.EmployeeCategoryId,
                            -- Numbers 
                            isnull(Sum(bud.TotalNumber),0) as BB,isnull(Sum(Cast(bud.Deployment as decimal)),0) as Dep , isnull(Sum(orole.OnRole),0) as OnRoll,isnull(Sum(EmpStatus.LA),0) as LA,isnull(Sum(EmpStatus.TBS),0) as TBS , isnull(Sum(Leaves.Leaves),0) as Leaves


                            from mst.ManpowerBudget mb 
                            left join 
                            (
                            Select mb.Id as BudgetId,Count(distinct ei.SystemId) as OnRole
                            from AttdnProcessData ap
                            left join EmployeeInformation ei on ei.SystemId = ap.EmpSystemID
                            left join mst.ManpowerBudget mb on mb.Id=ei.BudgetCode
                            --left join dbo.PhysicalVerification pv on pv.EmpSystemID = ap.EmpSystemID and pv.WorkDate = '" + Dates + @"'
                            where ap.WorkDate = '" + Dates + @"' and ei.EmployeeStatus = 'Active'  --and  ei.EmployeeCurrentStatus is null
                            group by mb.Id 
                            ) 
                            as orole on orole.BudgetId = mb.Id
                            left join 
                            (
	                            Select * from (
	                            Select rank() over (partition by ManpowerBudgetId order by  mb.EffectiveDate DESC,mb.Id) RNK, mb.TotalNumber, mb.ManpowerBudgetId, mb.EffectiveDate , mmb.Deployment
	                            from [MST].[ManpowerBudgetDetail] mb
	                            left join  mst.ManpowerBudget mmb on mmb.Id = mb.ManpowerBudgetId
	                            WHERE CONVERT(DATE,(mb.EffectiveDate) )<= CONVERT(DATE,'" + Dates + @"')
	                            ) as Bud where RNK = 1
                            ) 
                            as bud on bud.ManpowerBudgetId = mb.Id
                            left join
                            (
                            Select mbb.Id,Sum(Case when apd.IsLongAbsentism = 1 then 1 else 0 end) as LA , Sum(Case when apd.IsTBS = 1 then 1 else 0 end) as TBS 
                            from dbo.AttdnProcessData apd
							left join dbo.EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                            left join mst.ManpowerBudget mbb on mbb.Id = ei.BudgetCode
                            where ei.EmployeeStatus = 'Active' and apd.WorkDate = '" + Dates + @"'
                            group by mbb.Id
                            --Select mbb.Id,Sum(Case when ei.EmployeeCurrentStatus = 'LONG ABSENTEEISM' then 1 else 0 end) as LA , Sum(Case when ei.EmployeeCurrentStatus = 'TBS' then 1 else 0 end) as TBS 
                            --from dbo.EmployeeInformation ei
                            --left join mst.ManpowerBudget mbb on mbb.Id = ei.BudgetCode
                            --where ei.EmployeeStatus = 'Active' 
                            --group by mbb.Id
                            
                            ) as EmpStatus on EmpStatus.Id = mb.Id
                            left join 
                            (
                            Select mbb.Id , Count(lt.EmpSystemID) as Leaves
                            from dbo.LeaveTransactionDetails ltd
                            left join dbo.LeaveTransaction lt on lt.SystemID = ltd.LvTrnsSystemID
                            left join dbo.EmployeeInformation ei on ei.SystemId = lt.EmpSystemID
                            left join mst.ManpowerBudget mbb on mbb.ID = ei.BudgetCode
                            where ltd.WorkDate = '" + Dates+ @"'
                            group by mbb.Id
                            ) as Leaves on Leaves.Id = mb.Id
                            -- All the Joining Queries
                            left join org.Company c on c.Id = mb.CompanyId
                            left join org.Entity e on e.Id = mb.EntityId
                            left join org.Plant p on p.Id = e.PlantId
                            left join org.Position pos on pos.Id = mb.PositionId
                            left join org.Division div on div.Id = pos.DivisionId
                            left join org.Department dept on dept.Id = pos.DepartmentId
                            left join org.Section sec on sec.Id = pos.SectionId
                            left join org.SubSection ssec on ssec.Id = pos.SubSectionId
                            left join hkp.Designation desg on desg.ID = pos.DesignationId
                            left join hkp.Process pp on pp.Id = pos.ProcessId
                            left join dbo.ShiftDefination shd on shd.SystemID = mb.ShiftDefinationId
                            left join mst.DesignationMaster dm on dm.DesignationId = pos.DesignationId

                            group by  mb.CompanyId , mb.EntityId , c.UserName , p.UserName, div.UserName, e.UserName, dept.UserName , sec.UserName, ssec.UserName, desg.UserName, pos.Activity, pos.isDirect, pp.UserName, pos.Code, mb.Id, mb.Code ,shd.UserName,pos.UserReportGroup , pos.UserName, pos.PhysicalVarification , mb.IsRosterApplicable , mb.IsScattedWeekOffApplicable,pos.PaymentLink,pos.TaskManagementApplicable,dm.EmployeeCategoryId,pp.Id, div.Sequence ,dept.Sequence,sec.Sequence , ssec.Sequence ,desg.Sequence
                            ) as Main
                            where EntityId in (" + Parameters["EntityId"]+ @") and EmployeeCategoryId in ("+Parameters["EmpTypeId"]+ @")
                            and UserReportGroup in (" + Parameters["UserReportGroup"] + @") --and ProcessId in (" + Parameters["ProcessId"] + @")
                            order by  DivSeq ,DeptSeq,SecSeq , SSecSeq ,DesgSeq
                            ";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
    
