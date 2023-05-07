using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.IE.Controllers
{
    public class WorkcenterWiseDetentionController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        public WorkcenterWiseDetentionController(SqlRepository R)
        {
            _sqlRepository = R;
        }
        public ActionResult Aplos()
        {
            return View();
        }
        [Authorize, HttpPost]
        public ActionResult GetShift()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select SD.SystemID ShiftId,P.Id PlantId,P.UserName Plant,SD.ShiftDefinationDescription
						,SD.UserName ShiftDefination,SD.InTime,SD.OutTime
						
						from ShiftDefination SD
						left join ORG.Plant P on P.Id=SD.PlantID";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetProcess(string machineMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select P.Id,P.Sequence,P.Code,P.ShortName,P.StandardName,P.Id ProcessId,P.UserName Process
			                            from MachineMasterProcess MMP
			                            left join HKP.Process P on P.Id=MMP.ProcessId";
										//where MMP.MachineMasterId='" + machineMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetDetentionMaster()
        {
            string str = @"Select DetentionUserName As Text, Id As Value from DetentionMaster";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWorkcenter()
        {
            string str = @"SELECT StandardName, '' CalculatedTime FROM SCS.WorkCenterMaster";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
    }
}