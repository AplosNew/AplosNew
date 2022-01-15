#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using Library.OrderManagement.Production;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using System.Text.RegularExpressions;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Aplos.Areas.Commercial.Controllers;
using System.Drawing;


#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class GeneralWasteController  : BaseController
    {

        GeneralWasteService ws = new GeneralWasteService();
        string TableName = "dbo.WasteMaster";
        
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public GeneralWasteController(ISqlRepository R)
        {
            _sqlRepository = R;
          
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult getProcess()
        {
            return Json(ws.getProcess(), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public ActionResult getUOM()
        {
            return Json(ws.getUOM(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getEntity()
        {
            return Json(ws.getEntity(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult getView(string Id)
        {
            return Json(ws.getView(Id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM dbo.WasteMaster"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = ws.GetMaster(Id);
                var _child = ws.GetChild(Id);
                return Json(new { master = _master , child = _child }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost , Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            return Json(ws.GetList(strkey), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create( List<Dictionary<string, object>> Data )
        {
            try
            {
                var data = ws.Create(Data);
                return Json(new { Error = false, Data= data, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        //[HttpPost]
        //public ActionResult Delete(string id)
        //{
        //    try
        //    {
        //        ws.Delete(id);

        //        return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

        //    }


        //}


        
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

    }
}

