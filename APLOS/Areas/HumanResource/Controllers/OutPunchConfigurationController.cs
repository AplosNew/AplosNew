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
    public class OutPunchConfigurationController : BaseController
    {
       


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        OutPunchConfigurationService op = new OutPunchConfigurationService();
        public OutPunchConfigurationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


    
        public ActionResult Aplos()
        {
            return View();
        }

     
        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            var master = op.Get(Id);
            var child = op.GetChild(Id);
            return Json( new { Master= master , Child = child }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetList()
        {
            return Json(op.GetList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

     

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data , List<Dictionary<string, object>> child)
        {
                var datas = op.Create(data , child);
                return Json(new { Error = false, Data = datas, Sequence = GetSequence(), Message = AplosMessage.Updated }); 
            
        }

        public ActionResult Delete(string id)
        {
            op.Delete(id);
            return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }


        
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM dbo.OutpunchConfigurationHeader");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
    }
}