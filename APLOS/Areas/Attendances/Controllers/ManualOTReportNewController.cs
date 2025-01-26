#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.OrderManagement.OrderControl;
using System.IO;
using Library.Data;
using Library.Service.Helpers;


#endregion Using

namespace Aplos.Areas.Attendances.Controllers
{
    public class ManualOTReportNewController : BaseController
    {
        string TableName = "dbo.OTfromApp";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public ManualOTReportNewController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + "  "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getplant()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_sqlRepository.GetDataCollection("select Id as Value, UserName as Text from ORG.Plant where CompanyId='"+ identity.CompanyId + @"' order by UserName "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getentity(string PlantId)
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value, UserName as Text from ORG.Entity where PlantId='"+ PlantId + @"' "), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from dbo.OTfromApp where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost, Authorize]
        public ActionResult getsearchedotemp(string ToDate, string FromDate, string Id, string PlantId, string EntityId)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = "";
                if (string.IsNullOrEmpty(EntityId))
                {
                    //             sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS EmployeeSystemId, EMP.EmployeeStatus,
                    //                     EMP.EmployeeName,EMP.EmployeeCode AS Code,cm.Id as CompanyId,cm.UserName as Company,apd.InTime as EMPAPDInTime, CONVERT(varchar(5), apd.[InTime], 108)[APDInTime]
                    //                     ,apd.OutTime as EMPAPDOutTime, CONVERT(varchar(5), apd.[OutTime], 108)[APDOutTime], apd.OTHr as OverStay
                    //                     --, FORMAT(apd.WorkDate, 'dd-MMM-yyyy') as APDEmpWorkDate
                    //                     ,APDEmpWorkDate=case when apd.WorkDate is not null then FORMAT(apd.WorkDate, 'dd-MMM-yyyy') when mo.WorkDate is not null then FORMAT(mo.WorkDate, 'dd-MMM-yyyy') End
                    //,apd.DayStatus,dt.Category,FORMAT(apd.InTime, 'dd-MMM-yyyy HH:mm') as APDEmpInDateAndTime,FORMAT(apd.OutTime, 'dd-MMM-yyyy HH:mm') as APDEmpOutDateAndTime,
                    //                     EMP.BudgetCode,sd.UserName as EmpShift,E.Id as EntityId,E.UserName EntityName,div.Id as DivisionId,div.UserName as Division,D.Id as DesignationId, isnull(D.UserName, '') Designation,
                    //                     PR.UserName PositionName,DEPT.Id as DepartmentId,
                    //                     DEPT.UserName DepartmentName, S.UserName Section,
                    //                     EMP.SectionId,SS.UserName SubSection,SS.Id as SubSectionId
                    //                     ,PL.UserName Plant, PL.Id as PlantId
                    //                     --, mo.OThour as ManualOT
                    //                     ,ManualOT=case when apd.EmpSystemID is not null and apd.WorkDate is not null then apd.ManualOt when mo.EmpSystemId is not null then mo.OThour else '0' End 
                    //                     , ExcessOT= case when(mo.OThour>apd.OTHr) then (mo.OThour - apd.OTHr) else '0' end, LessOT= case when(mo.OThour<apd.OTHr) then (apd.OTHr-mo.OThour) else '0' end
                    //,Remarks= case when(FORMAT(apd.OutTime,'dd-MMM-yyyy')='0' or FORMAT(apd.OutTime,'dd-MMM-yyyy')='') and mo.OThour>0 then 'OT without Out-time' when (mo.OThour - apd.OTHr)>0 then 'Excess OT' when (apd.OTHr-mo.OThour)>0 then 'Less OT' else 'NIL'  end
                    //                     ,WorkingMin=DATEDIFF(Minute,apd.InTime,apd.OutTime)
                    //,LateByMin=case when((CONVERT(varchar(5), apd.[InTime], 108))>(CONVERT(varchar(5), sd.[InTime], 108))) then DATEDIFF(Minute,CONVERT(varchar(5), sd.[InTime], 108),CONVERT(varchar(5), apd.[InTime], 108)) else '0' end
                    //,IsOTEntitled = CASE WHEN dmc.IsOTEntitled = 1 THEN 'Yes' ELSE 'No' END
                    //                     FROM EmployeeInformation EMP
                    //                     LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode = PMB.Id
                    //                     LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                    //                     LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
                    //                     LEFT JOIN ORG.Section S ON S.Id = pr.SectionId
                    //                     LEFT JOIN ORG.SubSection SS ON SS.Id = pr.SubSectionId
                    //                     LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id = EMP.LegalDesignationId
                    //                     LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
                    //                     LEFT JOIN ORG.Plant PL ON PL.Id = EMP.PlantId
                    //                     left join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId = D.Id
                    //                     left join scs.DesignationMasterConfiguration dmc on dmc.DesignationMasterId = dd.DesignationMasterId and dmc.PlantId = EMP.PlantId
                    //                     left join AttdnProcessData apd on apd.EmpSystemID = EMP.SystemId and (apd.[WorkDate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
                    //                     left join DayType dt on dt.DayType = apd.DayStatus
                    //                     left join OTfromApp mo on mo.EmpSystemID = emp.SystemId and (mo.[WorkDate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
                    //left join ORG.Company cm on cm.Id=EMP.CompanyId
                    //left join ORG.Division div on div.Id=EMP.DivisionId
                    //left join dbo.ShiftDefination sd on sd.SystemID=apd.ShiftSystemID
                    //                     WHERE emp.GroupID = '" + identity.CompanyGroupId + @"' and emp.doj <= '" + ToDate + @"' and(dos is null or dos >= '" + FromDate + @"')
                    //                     and EMP.CompanyId = '" + identity.CompanyId + @"' And EMP.PlantId='" + PlantId + @"' and EMP.EmployeeStatus='Active'
                    //                     and (apd.WorkDate is not null or mo.WorkDate is not null)  ";

                    sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS EmployeeSystemId, EMP.EmployeeStatus,
                            EMP.EmployeeName,EMP.EmployeeCode AS Code,cm.Id as CompanyId,cm.UserName as Company,apd.InTime as EMPAPDInTime, CONVERT(varchar(5), apd.[InTime], 108)[APDInTime]
                            ,apd.OutTime as EMPAPDOutTime, CONVERT(varchar(5), apd.[OutTime], 108)[APDOutTime]--, apd.OTHr as OverStay
							,apd.ProcessedOT as OverStay
                            --, FORMAT(apd.WorkDate, 'dd-MMM-yyyy') as APDEmpWorkDate
                            ,APDEmpWorkDate=case when apd.WorkDate is not null then FORMAT(apd.WorkDate, 'dd-MMM-yyyy') when mo.WorkDate is not null then FORMAT(mo.WorkDate, 'dd-MMM-yyyy') End
							,apd.DayStatus,dt.Category,FORMAT(apd.InTime, 'dd-MMM-yyyy HH:mm') as APDEmpInDateAndTime,FORMAT(apd.OutTime, 'dd-MMM-yyyy HH:mm') as APDEmpOutDateAndTime,
                            EMP.BudgetCode,sd.UserName as EmpShift,E.Id as EntityId,E.UserName EntityName,div.Id as DivisionId,div.UserName as Division,D.Id as DesignationId, isnull(D.UserName, '') Designation,
                            PR.UserName PositionName,DEPT.Id as DepartmentId,
                            DEPT.UserName DepartmentName, S.UserName Section,
                            EMP.SectionId,SS.UserName SubSection,SS.Id as SubSectionId
                            ,PL.UserName Plant, PL.Id as PlantId
                            --, mo.OThour as ManualOT
                          --  ,ManualOT=case when apd.EmpSystemID is not null and apd.WorkDate is not null then apd.ManualOt when mo.EmpSystemId is not null then mo.OThour else '0' End 
							,ManualOT=case when apd.EmpSystemID is not null and apd.WorkDate is not null then ISNULL(apd.ManualOt,'0') else '0' End
                         --   , ExcessOT= case when(mo.OThour>apd.OTHr) then (mo.OThour - apd.OTHr) else '0' end, LessOT= case when(mo.OThour<apd.OTHr) then (apd.OTHr-mo.OThour) else '0' end
						    , ExcessOT= case when(apd.ManualOt>apd.ProcessedOT) then (apd.ManualOt - apd.ProcessedOT) else '0' end, LessOT= case when(apd.ManualOt<apd.ProcessedOT) then (apd.ProcessedOT-apd.ManualOt) else '0' end
							--	,Remarks= case when(FORMAT(apd.OutTime,'dd-MMM-yyyy')='0' or FORMAT(apd.OutTime,'dd-MMM-yyyy')='') and mo.OThour>0 then 'OT without Out-time' when (mo.OThour - apd.OTHr)>0 then 'Excess OT' when (apd.OTHr-mo.OThour)>0 then 'Less OT' else 'NIL'  end
							,Remarks= case when(FORMAT(apd.OutTime,'dd-MMM-yyyy')='0' or FORMAT(apd.OutTime,'dd-MMM-yyyy')='') and apd.ManualOt>0 then 'OT without Out-time' when (apd.ManualOt - apd.ProcessedOT)>0 then 'Excess OT' when (apd.ProcessedOT - apd.ManualOt)>0 then 'Less OT' else 'NIL'  end
                            ,WorkingMin=DATEDIFF(Minute,apd.InTime,apd.OutTime)
							,LateByMin=case when((CONVERT(varchar(5), apd.[InTime], 108))>(CONVERT(varchar(5), sd.[InTime], 108))) then DATEDIFF(Minute,CONVERT(varchar(5), sd.[InTime], 108),CONVERT(varchar(5), apd.[InTime], 108)) else '0' end
							,IsOTEntitled = CASE WHEN dmc.IsOTEntitled = 1 THEN 'Yes' ELSE 'No' END
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode = PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
                            LEFT JOIN ORG.Section S ON S.Id = pr.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id = pr.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id = EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id = EMP.PlantId
                            left join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId = D.Id
                            left join scs.DesignationMasterConfiguration dmc on dmc.DesignationMasterId = dd.DesignationMasterId and dmc.PlantId = EMP.PlantId
                            left join AttdnProcessData apd on apd.EmpSystemID = EMP.SystemId and (apd.[WorkDate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
                            left join DayType dt on dt.DayType = apd.DayStatus
                            left join OTfromApp mo on mo.EmpSystemID = emp.SystemId and (mo.[WorkDate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
							left join ORG.Company cm on cm.Id=EMP.CompanyId
							left join ORG.Division div on div.Id=EMP.DivisionId
							left join dbo.ShiftDefination sd on sd.SystemID=apd.ShiftSystemID
                            WHERE emp.GroupID = '" + identity.CompanyGroupId + @"' and emp.doj <= '" + ToDate + @"' and(dos is null or dos >= '" + FromDate + @"')
                            and EMP.CompanyId = '" + identity.CompanyId + @"' And EMP.PlantId='" + PlantId + @"' and EMP.EmployeeStatus='Active'
                            and (apd.WorkDate is not null or mo.WorkDate is not null)  ";

                }
                else
                {
                    //             sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS EmployeeSystemId, EMP.EmployeeStatus,
                    //                     EMP.EmployeeName,EMP.EmployeeCode AS Code,cm.Id as CompanyId,cm.UserName as Company,apd.InTime as EMPAPDInTime, CONVERT(varchar(5), apd.[InTime], 108)[APDInTime]
                    //                     ,apd.OutTime as EMPAPDOutTime, CONVERT(varchar(5), apd.[OutTime], 108)[APDOutTime], apd.OTHr as OverStay
                    //                     --, FORMAT(apd.WorkDate, 'dd-MMM-yyyy') as APDEmpWorkDate
                    //                     ,APDEmpWorkDate=case when apd.WorkDate is not null then FORMAT(apd.WorkDate, 'dd-MMM-yyyy') when mo.WorkDate is not null then FORMAT(mo.WorkDate, 'dd-MMM-yyyy') End
                    //,apd.DayStatus,dt.Category,FORMAT(apd.InTime, 'dd-MMM-yyyy HH:mm') as APDEmpInDateAndTime,FORMAT(apd.OutTime, 'dd-MMM-yyyy HH:mm') as APDEmpOutDateAndTime,
                    //                     EMP.BudgetCode,sd.UserName as EmpShift,E.Id as EntityId,E.UserName EntityName,div.Id as DivisionId,div.UserName as Division,D.Id as DesignationId, isnull(D.UserName, '') Designation,
                    //                     PR.UserName PositionName,DEPT.Id as DepartmentId,
                    //                     DEPT.UserName DepartmentName, S.UserName Section,
                    //                     EMP.SectionId,SS.UserName SubSection,SS.Id as SubSectionId
                    //                     ,PL.UserName Plant, PL.Id as PlantId
                    //                     --, mo.OThour as ManualOT
                    //                     ,ManualOT=case when apd.EmpSystemID is not null and apd.WorkDate is not null then apd.ManualOt when mo.EmpSystemId is not null then mo.OThour else '0' End 
                    //                     , ExcessOT= case when(mo.OThour>apd.OTHr) then (mo.OThour - apd.OTHr) else '0' end, LessOT= case when(mo.OThour<apd.OTHr) then (apd.OTHr-mo.OThour) else '0' end
                    //,Remarks= case when(FORMAT(apd.OutTime,'dd-MMM-yyyy')='0' or FORMAT(apd.OutTime,'dd-MMM-yyyy')='') and mo.OThour>0 then 'OT without Out-time' when (mo.OThour - apd.OTHr)>0 then 'Excess OT' when (apd.OTHr-mo.OThour)>0 then 'Less OT' else 'NIL'  end
                    //                     ,WorkingMin=DATEDIFF(Minute,apd.InTime,apd.OutTime)
                    //,LateByMin=case when((CONVERT(varchar(5), apd.[InTime], 108))>(CONVERT(varchar(5), sd.[InTime], 108))) then DATEDIFF(Minute,CONVERT(varchar(5), sd.[InTime], 108),CONVERT(varchar(5), apd.[InTime], 108)) else '0' end
                    //,IsOTEntitled = CASE WHEN dmc.IsOTEntitled = 1 THEN 'Yes' ELSE 'No' END
                    //                     FROM EmployeeInformation EMP
                    //                     LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode = PMB.Id
                    //                     LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                    //                     LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
                    //                     LEFT JOIN ORG.Section S ON S.Id = pr.SectionId
                    //                     LEFT JOIN ORG.SubSection SS ON SS.Id = pr.SubSectionId
                    //                     LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id = EMP.LegalDesignationId
                    //                     LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
                    //                     LEFT JOIN ORG.Plant PL ON PL.Id = EMP.PlantId
                    //                     left join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId = D.Id
                    //                     left join scs.DesignationMasterConfiguration dmc on dmc.DesignationMasterId = dd.DesignationMasterId and dmc.PlantId = EMP.PlantId
                    //                     left join AttdnProcessData apd on apd.EmpSystemID = EMP.SystemId and (apd.[WorkDate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
                    //                     left join DayType dt on dt.DayType = apd.DayStatus
                    //                     left join OTfromApp mo on mo.EmpSystemID = emp.SystemId and (mo.[WorkDate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
                    //left join ORG.Company cm on cm.Id=EMP.CompanyId
                    //left join ORG.Division div on div.Id=EMP.DivisionId
                    //left join dbo.ShiftDefination sd on sd.SystemID=apd.ShiftSystemID
                    //                     WHERE emp.GroupID = '" + identity.CompanyGroupId + @"' and emp.doj <= '" + ToDate + @"' and(dos is null or dos >= '"+ FromDate + @"')
                    //                     and EMP.CompanyId = '"+ identity.CompanyId + @"' And EMP.PlantId='"+ PlantId + @"' and E.Id='"+ EntityId + @"' and EMP.EmployeeStatus='Active'
                    //                     and (apd.WorkDate is not null or mo.WorkDate is not null) ";

                    sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS EmployeeSystemId, EMP.EmployeeStatus,
                            EMP.EmployeeName,EMP.EmployeeCode AS Code,cm.Id as CompanyId,cm.UserName as Company,apd.InTime as EMPAPDInTime, CONVERT(varchar(5), apd.[InTime], 108)[APDInTime]
                            ,apd.OutTime as EMPAPDOutTime, CONVERT(varchar(5), apd.[OutTime], 108)[APDOutTime]--, apd.OTHr as OverStay
							,apd.ProcessedOT as OverStay
                            --, FORMAT(apd.WorkDate, 'dd-MMM-yyyy') as APDEmpWorkDate
                            ,APDEmpWorkDate=case when apd.WorkDate is not null then FORMAT(apd.WorkDate, 'dd-MMM-yyyy') when mo.WorkDate is not null then FORMAT(mo.WorkDate, 'dd-MMM-yyyy') End
							,apd.DayStatus,dt.Category,FORMAT(apd.InTime, 'dd-MMM-yyyy HH:mm') as APDEmpInDateAndTime,FORMAT(apd.OutTime, 'dd-MMM-yyyy HH:mm') as APDEmpOutDateAndTime,
                            EMP.BudgetCode,sd.UserName as EmpShift,E.Id as EntityId,E.UserName EntityName,div.Id as DivisionId,div.UserName as Division,D.Id as DesignationId, isnull(D.UserName, '') Designation,
                            PR.UserName PositionName,DEPT.Id as DepartmentId,
                            DEPT.UserName DepartmentName, S.UserName Section,
                            EMP.SectionId,SS.UserName SubSection,SS.Id as SubSectionId
                            ,PL.UserName Plant, PL.Id as PlantId
                            --, mo.OThour as ManualOT
                          --  ,ManualOT=case when apd.EmpSystemID is not null and apd.WorkDate is not null then apd.ManualOt when mo.EmpSystemId is not null then mo.OThour else '0' End 
							,ManualOT=case when apd.EmpSystemID is not null and apd.WorkDate is not null then ISNULL(apd.ManualOt,'0') else '0' End
                         --   , ExcessOT= case when(mo.OThour>apd.OTHr) then (mo.OThour - apd.OTHr) else '0' end, LessOT= case when(mo.OThour<apd.OTHr) then (apd.OTHr-mo.OThour) else '0' end
						    , ExcessOT= case when(apd.ManualOt>apd.ProcessedOT) then (apd.ManualOt - apd.ProcessedOT) else '0' end, LessOT= case when(apd.ManualOt<apd.ProcessedOT) then (apd.ProcessedOT-apd.ManualOt) else '0' end
							--	,Remarks= case when(FORMAT(apd.OutTime,'dd-MMM-yyyy')='0' or FORMAT(apd.OutTime,'dd-MMM-yyyy')='') and mo.OThour>0 then 'OT without Out-time' when (mo.OThour - apd.OTHr)>0 then 'Excess OT' when (apd.OTHr-mo.OThour)>0 then 'Less OT' else 'NIL'  end
							,Remarks= case when(FORMAT(apd.OutTime,'dd-MMM-yyyy')='0' or FORMAT(apd.OutTime,'dd-MMM-yyyy')='') and apd.ManualOt>0 then 'OT without Out-time' when (apd.ManualOt - apd.ProcessedOT)>0 then 'Excess OT' when (apd.ProcessedOT - apd.ManualOt)>0 then 'Less OT' else 'NIL'  end
                            ,WorkingMin=DATEDIFF(Minute,apd.InTime,apd.OutTime)
							,LateByMin=case when((CONVERT(varchar(5), apd.[InTime], 108))>(CONVERT(varchar(5), sd.[InTime], 108))) then DATEDIFF(Minute,CONVERT(varchar(5), sd.[InTime], 108),CONVERT(varchar(5), apd.[InTime], 108)) else '0' end
							,IsOTEntitled = CASE WHEN dmc.IsOTEntitled = 1 THEN 'Yes' ELSE 'No' END
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode = PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
                            LEFT JOIN ORG.Section S ON S.Id = pr.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id = pr.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id = EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id = EMP.PlantId
                            left join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId = D.Id
                            left join scs.DesignationMasterConfiguration dmc on dmc.DesignationMasterId = dd.DesignationMasterId and dmc.PlantId = EMP.PlantId
                            left join AttdnProcessData apd on apd.EmpSystemID = EMP.SystemId and (apd.[WorkDate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
                            left join DayType dt on dt.DayType = apd.DayStatus
                            left join OTfromApp mo on mo.EmpSystemID = emp.SystemId and (mo.[WorkDate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
							left join ORG.Company cm on cm.Id=EMP.CompanyId
							left join ORG.Division div on div.Id=EMP.DivisionId
							left join dbo.ShiftDefination sd on sd.SystemID=apd.ShiftSystemID
                            WHERE emp.GroupID = '" + identity.CompanyGroupId + @"' and emp.doj <= '" + ToDate + @"' and(dos is null or dos >= '" + FromDate + @"')
                            and EMP.CompanyId = '" + identity.CompanyId + @"' And EMP.PlantId='" + PlantId + @"' and E.Id='" + EntityId + @"' and EMP.EmployeeStatus='Active'
                            and (apd.WorkDate is not null or mo.WorkDate is not null) ";
                }


                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }

            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;

        }

        [HttpPost]
        public ActionResult GetOTManualReport(string From, string To, string Code, string PlantId, string EntityId, string DivisionId, string DepartmentId, string SectionId, string SubSectionId, string DesignationId, string APDEmpWorkDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
  
            var workbook = GetOTManualReportWorkSheet(From, To, Code, PlantId, EntityId, DivisionId, DepartmentId, SectionId, SubSectionId, DesignationId, APDEmpWorkDate);

            var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "OT-Manual.xlsx";
            string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
            workbook.SaveAs(fullPath);

            return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            
        }

        private IWorkbook GetOTManualReportWorkSheet(string From, string To, string Code, string PlantId, string EntityId, string DivisionId, string DepartmentId, string SectionId, string SubSectionId, string DesignationId, string APDEmpWorkDate)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "OTManual";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetOTManualReportData(From, To, Code, PlantId, EntityId, DivisionId, DepartmentId, SectionId, SubSectionId, DesignationId, APDEmpWorkDate);

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 8, ExcelHAlign.HAlignLeft);
            int ColPlant = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity Name", 12, ExcelHAlign.HAlignLeft);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 8, ExcelHAlign.HAlignLeft);
            int ColCode = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 15, ExcelHAlign.HAlignLeft);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Division", 12, ExcelHAlign.HAlignLeft);
            int ColDivision = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department Name", 15, ExcelHAlign.HAlignLeft);
            int ColDepartmentName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 10, ExcelHAlign.HAlignLeft);
            int ColSection = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Sub Section", 12, ExcelHAlign.HAlignLeft);
            int ColSubSection = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 12, ExcelHAlign.HAlignLeft);
            int ColDesignation = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Work Date", 10, ExcelHAlign.HAlignLeft);
            int ColWorkDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shift", 15, ExcelHAlign.HAlignLeft);
            int ColShift = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "In Date And Time", 15, ExcelHAlign.HAlignLeft);
            int ColInDateAndTime = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Out Date And Time", 15, ExcelHAlign.HAlignLeft);
            int ColOutDateAndTime = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Over Stay", 10, ExcelHAlign.HAlignLeft);
            int ColOverStay = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Manual OT", 10, ExcelHAlign.HAlignLeft);
            int ColManualOT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Excess OT", 10, ExcelHAlign.HAlignLeft);
            int ColExcessOT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Less OT", 10, ExcelHAlign.HAlignLeft);
            int ColLessOT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 15, ExcelHAlign.HAlignLeft);
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Late By Minutes", 10, ExcelHAlign.HAlignLeft);
            int ColLateByMin = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Working Minutes", 10, ExcelHAlign.HAlignLeft);
            int ColWorkingMin = COL;
            ROW++;


            endCol = COL;
            #endregion Headers

            string PlantName = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                if (PlantName != data.Rows[i]["Plant"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                  //      sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].Merge();
                        sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    }
                    RowIndex = ROW;
                }

                
                sheet[ROW, ColCode].Text = data.Rows[i]["Code"].ToString();
                sheet[ROW, ColPlant].Text = data.Rows[i]["Plant"].ToString();
                sheet[ROW, ColEntity].Text = data.Rows[i]["EntityName"].ToString();
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColDivision].Text = data.Rows[i]["Division"].ToString();
                sheet[ROW, ColDepartmentName].Text = data.Rows[i]["DepartmentName"].ToString();

                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();

                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                sheet[ROW, ColWorkDate].Text = data.Rows[i]["APDEmpWorkDate"].ToString();
                sheet[ROW, ColShift].Text = data.Rows[i]["EmpShift"].ToString();
                sheet[ROW, ColInDateAndTime].Text = data.Rows[i]["APDEmpInDateAndTime"].ToString();
                sheet[ROW, ColOutDateAndTime].Text = data.Rows[i]["APDEmpOutDateAndTime"].ToString();
                sheet[ROW, ColOverStay].Text = data.Rows[i]["OverStay"].ToString();

                sheet[ROW, ColManualOT].Text = data.Rows[i]["ManualOT"].ToString();

                sheet[ROW, ColExcessOT].Number = clsStaticInfo.dbl(data.Rows[i]["ExcessOT"].ToString());
                sheet[ROW, ColLessOT].Number = clsStaticInfo.dbl(data.Rows[i]["LessOT"].ToString());
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColLateByMin].Text = data.Rows[i]["LateByMin"].ToString();
                sheet[ROW, ColWorkingMin].Text = data.Rows[i]["WorkingMin"].ToString();


                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                PlantName = data.Rows[i]["Plant"].ToString();

                ROW++;
            }

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
          //      sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].Merge();
                sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColPlant, ROW - 1, ColPlant].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColEntity, ROW - 1, ColEntity].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColEntity, ROW - 1, ColEntity].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColCode, ROW - 1, ColCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColCode, ROW - 1, ColCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColEmployeeName, ROW - 1, ColEmployeeName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColEmployeeName, ROW - 1, ColEmployeeName].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColDivision, ROW - 1, ColDivision].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColDivision, ROW - 1, ColDivision].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColDepartmentName, ROW - 1, ColDepartmentName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColDepartmentName, ROW - 1, ColDepartmentName].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColSection, ROW - 1, ColSection].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColSection, ROW - 1, ColSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColSubSection, ROW - 1, ColSubSection].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColSubSection, ROW - 1, ColSubSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColDesignation, ROW - 1, ColDesignation].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColDesignation, ROW - 1, ColDesignation].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColWorkDate, ROW - 1, ColWorkDate].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColWorkDate, ROW - 1, ColWorkDate].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColShift, ROW - 1, ColShift].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColShift, ROW - 1, ColShift].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColInDateAndTime, ROW - 1, ColInDateAndTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColInDateAndTime, ROW - 1, ColInDateAndTime].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColOutDateAndTime, ROW - 1, ColOutDateAndTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColOutDateAndTime, ROW - 1, ColOutDateAndTime].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColOverStay, ROW - 1, ColOverStay].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColOverStay, ROW - 1, ColOverStay].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColManualOT, ROW - 1, ColManualOT].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColManualOT, ROW - 1, ColManualOT].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColExcessOT, ROW - 1, ColExcessOT].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColExcessOT, ROW - 1, ColExcessOT].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColLessOT, ROW - 1, ColLessOT].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColLessOT, ROW - 1, ColLessOT].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColRemarks, ROW - 1, ColRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColRemarks, ROW - 1, ColRemarks].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColLateByMin, ROW - 1, ColLateByMin].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColLateByMin, ROW - 1, ColLateByMin].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.Range[RowIndex, ColWorkingMin, ROW - 1, ColWorkingMin].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColWorkingMin, ROW - 1, ColWorkingMin].HorizontalAlignment = ExcelHAlign.HAlignLeft;


            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, endCol, "OT Manual", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private DataTable GetOTManualReportData(string From, string To, string Code, string PlantId, string EntityId, string DivisionId, string DepartmentId, string SectionId, string SubSectionId, string DesignationId, string APDEmpWorkDate)
        {
            //     var sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS EmployeeSystemId, EMP.EmployeeStatus,
            //                     EMP.EmployeeName,EMP.EmployeeCode AS Code,cm.Id as CompanyId,cm.UserName as Company,apd.InTime as EMPAPDInTime, CONVERT(varchar(5), apd.[InTime], 108)[APDInTime]
            //,apd.OutTime as EMPAPDOutTime, CONVERT(varchar(5), apd.[OutTime], 108)[APDOutTime], apd.OTHr as OverStay--, FORMAT(apd.WorkDate, 'dd-MMM-yyyy') as APDEmpWorkDate
            //,APDEmpWorkDate=case when apd.WorkDate is not null then FORMAT(apd.WorkDate, 'dd-MMM-yyyy') when mo.WorkDate is not null then FORMAT(mo.WorkDate, 'dd-MMM-yyyy') End
            //,apd.DayStatus,dt.Category,FORMAT(apd.InTime, 'dd-MMM-yyyy HH:mm') as APDEmpInDateAndTime,FORMAT(apd.OutTime, 'dd-MMM-yyyy HH:mm') as APDEmpOutDateAndTime,
            //                     EMP.BudgetCode,sd.UserName as EmpShift,E.Id as EntityId,E.UserName EntityName,div.Id as DivisionId,div.UserName as Division,D.Id as DesignationId, isnull(D.UserName, '') Designation,
            //                     PR.UserName PositionName,DEPT.Id as DepartmentId,
            //                     DEPT.UserName DepartmentName, S.UserName Section,
            //                     EMP.SectionId,SS.UserName SubSection,SS.Id as SubSectionId
            //                     ,PL.UserName Plant, PL.Id as PlantId
            //                      --,mo.OThour as ManualOT
            // ,ManualOT=case when apd.EmpSystemID is not null and apd.WorkDate is not null then apd.ManualOt when mo.EmpSystemId is not null then mo.OThour else '0' End 
            // , ExcessOT= case when(mo.OThour>apd.OTHr) then (mo.OThour - apd.OTHr) else '0' end, LessOT= case when(mo.OThour<apd.OTHr) then (apd.OTHr-mo.OThour) else '0' end
            //,Remarks= case when(FORMAT(apd.OutTime,'dd-MMM-yyyy')='0' or FORMAT(apd.OutTime,'dd-MMM-yyyy')='') and mo.OThour>0 then 'OT without Out-time' when (mo.OThour - apd.OTHr)>0 then 'Excess OT' when (apd.OTHr-mo.OThour)>0 then 'Less OT' else 'NIL'  end
            //                     ,WorkingMin=DATEDIFF(Minute,apd.InTime,apd.OutTime)
            //,LateByMin=case when((CONVERT(varchar(5), apd.[InTime], 108))>(CONVERT(varchar(5), sd.[InTime], 108))) then DATEDIFF(Minute,CONVERT(varchar(5), sd.[InTime], 108),CONVERT(varchar(5), apd.[InTime], 108)) else '0' end
            //,IsOTEntitled = CASE WHEN dmc.IsOTEntitled = 1 THEN 'Yes' ELSE 'No' END
            //                     FROM EmployeeInformation EMP
            //                     LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode = PMB.Id
            //                     LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
            //                     LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
            //                     LEFT JOIN ORG.Section S ON S.Id = pr.SectionId
            //                     LEFT JOIN ORG.SubSection SS ON SS.Id = pr.SubSectionId
            //                     LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id = EMP.LegalDesignationId
            //                     LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
            //                     LEFT JOIN ORG.Plant PL ON PL.Id = EMP.PlantId
            //                     left join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId = D.Id
            //                     left join scs.DesignationMasterConfiguration dmc on dmc.DesignationMasterId = dd.DesignationMasterId and dmc.PlantId = EMP.PlantId
            //                     left join AttdnProcessData apd on apd.EmpSystemID = EMP.SystemId and (apd.[WorkDate] between CONVERT(DATE, '" + From + @"') AND CONVERT(DATE, '" + To + @"'))
            //                     left join DayType dt on dt.DayType = apd.DayStatus
            //                     left join OTfromApp mo on mo.EmpSystemID = emp.SystemId and (mo.[WorkDate] between CONVERT(DATE, '" + From + @"') AND CONVERT(DATE, '" + To + @"'))
            //left join ORG.Company cm on cm.Id=EMP.CompanyId
            //left join ORG.Division div on div.Id=EMP.DivisionId
            //left join dbo.ShiftDefination sd on sd.SystemID=apd.ShiftSystemID
            //where PL.Id IN ( " + PlantId + " ) and E.Id IN ( " + EntityId + " ) and EMP.EmployeeCode IN ( " + Code + " ) and div.Id IN ( " + DivisionId + " ) and DEPT.Id IN ( " + DepartmentId + " ) and S.Id IN ( " + SectionId + " ) and SS.Id IN( " + SubSectionId + " ) and D.Id IN( " + DesignationId + " ) and (apd.WorkDate IN( "+ APDEmpWorkDate + " ) or mo.WorkDate IN( "+ APDEmpWorkDate + ")) ";


            var sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS EmployeeSystemId, EMP.EmployeeStatus,
                            EMP.EmployeeName,EMP.EmployeeCode AS Code,cm.Id as CompanyId,cm.UserName as Company,apd.InTime as EMPAPDInTime, CONVERT(varchar(5), apd.[InTime], 108)[APDInTime]
                            ,apd.OutTime as EMPAPDOutTime, CONVERT(varchar(5), apd.[OutTime], 108)[APDOutTime]--, apd.OTHr as OverStay
							,apd.ProcessedOT as OverStay
                            --, FORMAT(apd.WorkDate, 'dd-MMM-yyyy') as APDEmpWorkDate
                            ,APDEmpWorkDate=case when apd.WorkDate is not null then FORMAT(apd.WorkDate, 'dd-MMM-yyyy') when mo.WorkDate is not null then FORMAT(mo.WorkDate, 'dd-MMM-yyyy') End
							,apd.DayStatus,dt.Category,FORMAT(apd.InTime, 'dd-MMM-yyyy HH:mm') as APDEmpInDateAndTime,FORMAT(apd.OutTime, 'dd-MMM-yyyy HH:mm') as APDEmpOutDateAndTime,
                            EMP.BudgetCode,sd.UserName as EmpShift,E.Id as EntityId,E.UserName EntityName,div.Id as DivisionId,div.UserName as Division,D.Id as DesignationId, isnull(D.UserName, '') Designation,
                            PR.UserName PositionName,DEPT.Id as DepartmentId,
                            DEPT.UserName DepartmentName, S.UserName Section,
                            PR.SectionId,SS.UserName SubSection,SS.Id as SubSectionId
                            ,PL.UserName Plant, PL.Id as PlantId
                            --, mo.OThour as ManualOT
                          --  ,ManualOT=case when apd.EmpSystemID is not null and apd.WorkDate is not null then apd.ManualOt when mo.EmpSystemId is not null then mo.OThour else '0' End 
							,ManualOT=case when apd.EmpSystemID is not null and apd.WorkDate is not null then ISNULL(apd.ManualOt,'0') else '0' End
                         --   , ExcessOT= case when(mo.OThour>apd.OTHr) then (mo.OThour - apd.OTHr) else '0' end, LessOT= case when(mo.OThour<apd.OTHr) then (apd.OTHr-mo.OThour) else '0' end
						    , ExcessOT= case when(apd.ManualOt>apd.ProcessedOT) then (apd.ManualOt - apd.ProcessedOT) else '0' end, LessOT= case when(apd.ManualOt<apd.ProcessedOT) then (apd.ProcessedOT-apd.ManualOt) else '0' end
							--	,Remarks= case when(FORMAT(apd.OutTime,'dd-MMM-yyyy')='0' or FORMAT(apd.OutTime,'dd-MMM-yyyy')='') and mo.OThour>0 then 'OT without Out-time' when (mo.OThour - apd.OTHr)>0 then 'Excess OT' when (apd.OTHr-mo.OThour)>0 then 'Less OT' else 'NIL'  end
							,Remarks= case when(FORMAT(apd.OutTime,'dd-MMM-yyyy')='0' or FORMAT(apd.OutTime,'dd-MMM-yyyy')='') and apd.ManualOt>0 then 'OT without Out-time' when (apd.ManualOt - apd.ProcessedOT)>0 then 'Excess OT' when (apd.ProcessedOT - apd.ManualOt)>0 then 'Less OT' else 'NIL'  end
                            ,WorkingMin=DATEDIFF(Minute,apd.InTime,apd.OutTime)
							,LateByMin=case when((CONVERT(varchar(5), apd.[InTime], 108))>(CONVERT(varchar(5), sd.[InTime], 108))) then DATEDIFF(Minute,CONVERT(varchar(5), sd.[InTime], 108),CONVERT(varchar(5), apd.[InTime], 108)) else '0' end
							,IsOTEntitled = CASE WHEN dmc.IsOTEntitled = 1 THEN 'Yes' ELSE 'No' END
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode = PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
                            LEFT JOIN ORG.Section S ON S.Id = pr.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id = pr.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id = EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id = EMP.PlantId
                            left join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId = D.Id
                            left join scs.DesignationMasterConfiguration dmc on dmc.DesignationMasterId = dd.DesignationMasterId and dmc.PlantId = EMP.PlantId
                            left join AttdnProcessData apd on apd.EmpSystemID = EMP.SystemId and (apd.[WorkDate] between CONVERT(DATE, '" + From + @"') AND CONVERT(DATE, '" + To + @"'))
                            left join DayType dt on dt.DayType = apd.DayStatus
                            left join OTfromApp mo on mo.EmpSystemID = emp.SystemId and (mo.[WorkDate] between CONVERT(DATE, '" + From + @"') AND CONVERT(DATE, '" + To + @"'))
							left join ORG.Company cm on cm.Id=EMP.CompanyId
							left join ORG.Division div on div.Id=PR.DivisionId
							left join dbo.ShiftDefination sd on sd.SystemID=apd.ShiftSystemID
							where PL.Id IN ( " + PlantId + " ) and E.Id IN ( " + EntityId + " ) and EMP.EmployeeCode IN ( " + Code + " ) and div.Id IN ( " + DivisionId + " ) and DEPT.Id IN ( " + DepartmentId + " ) and S.Id IN ( " + SectionId + " ) and SS.Id IN( " + SubSectionId + " ) and D.Id IN( " + DesignationId + " ) and (apd.WorkDate IN( " + APDEmpWorkDate + " ) or mo.WorkDate IN( " + APDEmpWorkDate + ")) ";

            return _sqlRepository.GetDataTable(sql);
        }

    }
}