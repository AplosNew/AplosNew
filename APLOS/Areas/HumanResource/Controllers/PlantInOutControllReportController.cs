using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class PlantInOutControllReportController : Controller
    {
        private readonly SqlRepository _sqlRepository;
        public PlantInOutControllReportController()
        {
            _sqlRepository = new SqlRepository();
        }

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GetPlantInOutGridData()
        {
            string sql = @"select EMP.EmployeeCode,EMP.EmployeeName,  isnull(OUT.EmployeeCode,INP.EmployeeCode) Code,isnull(OUT.Date,INP.Date) Date
,OUT.InandOut'Out', OUT.Time OutTime, INP.InandOut 'IN', INP.Time InTIme
, D.UserName GivenDesignation
, EMP.EmployeeCurrentStatus 
,A.UserName Activity, SS.UserName SubSection,S.UserName Section,DEP.UserName Department,EMP.EmployeeStatus 
,DEP.UserName Department

from (SELECT DISTINCT concat(row_number() over (
          partition by PIC.EmployeeCode,PIC.Date
          order by PIC.EmployeeCode, PIC.Date,PIC.InandOut ,PIC.Time)
         ,PIC.EmployeeCode,PIC.Date) as RowID, PIC.EmployeeCode, PIC.Date,PIC.InandOut ,PIC.Time
FROM PlantInOutControl  PIC 
WHERE PIC.InandOut = 'OUT') OUT

FULL JOIN (SELECT DISTINCT concat(row_number() over (
          partition by PIC.EmployeeCode,PIC.Date
          order by PIC.EmployeeCode, PIC.Date,PIC.InandOut ,PIC.Time)
         ,PIC.EmployeeCode,PIC.Date) as RowID, PIC.EmployeeCode, PIC.Date,PIC.InandOut ,PIC.Time
FROM PlantInOutControl  PIC 
WHERE PIC.InandOut = 'IN') INP on INP.RowID = OUT.RowID
LEFT JOIN EmployeeInformation EMP ON EMP.SystemId = OUT.EmployeeCode OR EMP.SystemId = INP.EmployeeCode
left join MST.ManpowerBudget BGT on BGT.Id = EMP.BudgetCode
left join MST.BudgetMasterActivity BMA on BGT.ROBudgetCode = BMA.BudgetMasterId
left join HKP.Activity A on BMA.ActivityId = A.Id
left join ORG.Position P on BGT.PositionId = P.Id
left join ORG.Department DT on P.DepartmentId = DT.Id
left join ORG.Section S on P.SectionId = S.Id
left join ORG.SubSection SS on P.SubSectionId = SS.Id
left join ORG.Entity E on E.Id = BGT.EntityId
LEFT JOIN ORG.Department DEP on DEP.Id = EMP.DepartmentId
left join HKP.Designation D on D.Id = EMP.GivenDesignationId

order by Out.EmployeeCode,INP.EmployeeCode,Out.Date,INP.Date,Out.Time,INP.Time";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
    }
}