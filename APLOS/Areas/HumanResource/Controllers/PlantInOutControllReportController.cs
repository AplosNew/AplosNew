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
            string sql = @"select FORMAT(PIO.AddedDate, 'dd-MMM-yyyy')[Date], PIO.InandOut
,InTime = Case when PIO.InandOut = 'IN' THEN PIO.[Time] else  NULL end
,OutTime = Case when PIO.InandOut = 'OUT' THEN PIO.[Time] else  NULL end
, (DATEDIFF (
SECOND
,(select  MAX([PI].[Time]) from PlantInOutControl [PI] where [PI].EmployeeCode = EI.SystemId and [PI].InandOut = 'OUT' group by [PI].InandOut)
,(select  MIN([PI].[Time]) from PlantInOutControl [PI] where [PI].EmployeeCode = EI.SystemId and [PI].InandOut = 'IN' group by [PI].InandOut)
)) InOutDuration
,EI.EmployeeCode,EI.EmployeeName, D.UserName GivenDesignation
, EI.EmployeeCurrentStatus 
,A.UserName Activity, SS.UserName SubSection,S.UserName Section,DEP.UserName Department,EI.EmployeeStatus 
,DEP.UserName Department

from PlantInOutControl PIO 
left join EmployeeInformation EI  on EI.SystemId = PIO.EmployeeCode 

left join MST.ManpowerBudget BGT on BGT.Id = EI.BudgetCode
left join MST.BudgetMasterActivity BMA on BGT.ROBudgetCode = BMA.BudgetMasterId
left join HKP.Activity A on BMA.ActivityId = A.Id
left join ORG.Position P on BGT.PositionId = P.Id
left join ORG.Department DT on P.DepartmentId = DT.Id
left join ORG.Section S on P.SectionId = S.Id
left join ORG.SubSection SS on P.SubSectionId = SS.Id
left join ORG.Entity E on E.Id = BGT.EntityId
LEFT JOIN ORG.Department DEP on DEP.Id = EI.DepartmentId
left join HKP.Designation D on D.Id = EI.GivenDesignationId
--where EI.SystemId = 2014991 
order by EI.EmployeeCode, PIO.AddedDate";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
    }
}