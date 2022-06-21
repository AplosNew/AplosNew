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


namespace Aplos.Areas.Materials.Controllers
{
    public class StorageBinMasterController : BaseController
    {
        StorageBinMasterService sb = new StorageBinMasterService();

        public StorageBinMasterController() { }
        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page
        [Authorize, HttpPost]

        public ActionResult getResponsiblePerson()
        {
            try
            {
                return Json(sb.getResponsiblePerson(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpPost]

        public ActionResult getStorageLocation()
        {
            try
            {
                return Json(sb.getStorageLocation(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpPost]
        public JsonResult Save(Dictionary<string, object> datas, string ResponsiblePersonId)

        {
            try
            {
                var data = sb.Save(datas, ResponsiblePersonId);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            try
            {
                sb.Delete(id);

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

    }
}