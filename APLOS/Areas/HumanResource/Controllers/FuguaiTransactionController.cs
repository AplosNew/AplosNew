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
using Library.HumanResource.Employee;


#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class FuguaiTransactionController : Controller
    {
        FuguaiTransactionService ft = new FuguaiTransactionService();
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public ActionResult getEntity()
        {
            try
            {
                return Json(ft.getEntity(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getObservedBy(string user)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                user = identity.UserId;
                return Json(ft.getObservedBy(user), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getCategory()
        {
            try
            {
                return Json(ft.getCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getTag(string categoryText)
        {
            try
            {
                return Json(ft.getTag(categoryText), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getSubCategory(string categoryText, string FuguaiId)
        {
            try
            {
                return Json(ft.getSubCategory(categoryText, FuguaiId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getDepartment()
        {
            try
            {
                return Json(ft.getDepartment(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getResponsiblePerson(string DepartmentId)
        {
            try
            {
                return Json(ft.getResponsiblePerson(DepartmentId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getProcess(string EntityId)
        {
            try
            {
                return Json(ft.getProcess(EntityId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getMachine()
        {
            try
            {
                return Json(ft.getMachine(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getMachineRef(string mmId)
        {
            try
            {
                return Json(ft.getMachineRef(mmId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Save(Dictionary<string, object> datas, string ObservedById, string ResponsiblePersonId)

        {
            try
            {
                var data = ft.Save(datas, ObservedById, ResponsiblePersonId);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated});

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
    }
}