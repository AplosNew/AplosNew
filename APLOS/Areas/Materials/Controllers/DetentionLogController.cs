using Aplos.Controllers;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.MaterialManagement.Material;

namespace Aplos.Areas.Materials.Controllers
{
    public class DetentionLogController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        DetentionLogService dl = new DetentionLogService();
        public DetentionLogController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        [Authorize, AllowAnonymous]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public JsonResult GetEntity()
        {

            return Json(dl.GetEntity(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetDetentionDepartment()
        {
           
            return Json(dl.GetDetentionDepartment(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetDetentionResponsible(string detentionId)
        {

            return Json(dl.GetDetentionResponsible(detentionId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getDetentionTypeListByDepartment(string departmentid)
        {

            return Json(dl.getDetentionTypeListByDepartment(departmentid), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getDetention(string processId)
        {

            return Json(dl.getDetention(processId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getProcessList()
        {

            return Json(dl.getProcessList(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetWorkCenter(string processId)
        {

            return Json(dl.GetWorkCenter(processId), JsonRequestBehavior.AllowGet);
        }
    }
}