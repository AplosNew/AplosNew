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
using Library.HumanResource.NewAttendanceProcess;


#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class PerformancePeriodMasterController : BaseController
    {
        PerformancePeriodMasterService pp = new PerformancePeriodMasterService();
        string TableName = "dbo.PerformancePeriod";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public PerformancePeriodMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }


        //[HttpPost]
        //public ActionResult GetList(string column, string value)
        //{
        //    string strkey = "1=1";
        //    if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
        //        strkey = column + " like '%" + value + "%'";
        //    return Json(pp.GetList(strkey), JsonRequestBehavior.AllowGet);
        //}
        //public ActionResult Delete(string SystemId)
        //{
        //    try
        //    {
        //        pp.Delete(SystemId);

        //        return Json(new { Error = false,  Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

        //    }

        //}

        //[HttpPost]
        //public JsonResult Create(List<Dictionary<string, object>> Data)
        //{
        //    try
        //    {
        //        var data = pp.Create(Data);
        //        return Json(new { Error = false, Data = data,  Message = AplosMessage.Updated });

        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message });

        //    }
        //}
    }

}

      
