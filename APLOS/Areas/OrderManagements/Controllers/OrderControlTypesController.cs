#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Aplos.Controllers;
using System;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using Library.Data.Sql;
using System.Collections.Generic;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class OrderControlTypesController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public OrderControlTypesController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion


        #region Operations
        [HttpPost, Authorize]
        public ActionResult getlist()
        {
            CustomIdentity identity= (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IdentityParameter para = new IdentityParameter();
            para.CompanyGroupId = identity.CompanyGroupId;
            Library.OrderManagement.OrderControl.OrderControl control = new Library.OrderManagement.OrderControl.OrderControl();

            control.GetData(out DataSet dsData, para);


            return Json(Helpers.CustomJsonResult.DataTableToJson(dsData.Tables[0]), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult Save(List<Dictionary<string, object>> Data)
        {
            try
            {
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IdentityParameter para = new IdentityParameter();
                para.CompanyGroupId = identity.CompanyGroupId;

                Library.OrderManagement.OrderControl.OrderControl control = new Library.OrderManagement.OrderControl.OrderControl();
                control.saveData(Data, para);

                return Json(new { Error = false, Message = "Data saved successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {


                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        [Authorize, HttpPost]
        public ActionResult SearchEmployee(string column, string value)
        {

            Library.OrderManagement.OrderControl.OrderControl control = new Library.OrderManagement.OrderControl.OrderControl();
            return Json(control.SearchEmployee(column, value), JsonRequestBehavior.AllowGet);


        }

        #endregion Operations
    }

}