using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Security.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class BudgetCodeWiseHRReportController : Controller
    {
        private readonly SqlRepository _sqlRepository;
        public BudgetCodeWiseHRReportController()
        {
            _sqlRepository = new SqlRepository();
        }

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GetHRReportMasterList()
        {
            try
            {
                var sql = @"select Id Value, UserName Text from HKP.HRReportMaster";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetDataOnFavouriteFilter(string filterId)
        {
            try
            {
                string filter = "'" + filterId.Replace(",", "','") + "'";//replaced with ""

                var sql = @"select HRM.Id, E.UserName Entity, HRM.Active, HRM.Active isSelected, D.UserName Division, DT.UserName Department, S.UserName Section, SS.UserName SubSection, DSG.UserName Designation, A.UserName Activity,SDF.UserName [Shift], P.Code PositionCode
, P.UserName Position ,BGT.Code BudgetCode, BGT.Id ManpowerBudgetId, HRG.UserGroup, HRG.UserSubGroup, HRG.Grade
from HKP.HRReportMaster HR
left join [TRN].[HRReportMasterChild] HRM on HRM.HRReportMasterId = HR.Id
left join HKP.HRReportGroupMaster HRG on HRG.Id = HRM.UserGroupId
left join MST.ManpowerBudget BGT on BGT.Id = HRM.ManpowerBudgetId 
left join ORG.Entity E on E.Id = BGT.EntityId
left join MST.BudgetMasterActivity BMA on BGT.ROBudgetCode = BMA.BudgetMasterId
left join HKP.Activity A on BMA.ActivityId = A.Id
left join dbo.ShiftDefination SDF on BGT.ShiftDefinationId = SDF.SystemID
left join ORG.Position P on BGT.PositionId = P.Id
left join ORG.Division D on P.DivisionId  = D.Id
left join ORG.Department DT on P.DepartmentId = DT.Id
left join ORG.Section S on P.SectionId = S.Id
left join ORG.SubSection SS on P.SubSectionId = SS.Id
left join ORG.Division DSN on P.DivisionId = DSN.Id
left join HKP.Designation DSG ON P.DesignationId = DSG.Id
where HR.Id in (" + filter + ")";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetProcess()
        {
            try
            {
                var sql = @"select Id Value, UserName Text from HKP.Process order by Text ASC";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}