using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos;
using Aplos.Properties;
using Library.OrderManagement.Production;
using Library.Data.Sql;
using Library.Security.Core;


namespace Aplos.Areas.QMS.Controllers
{
    public class IssueController : Controller
    {
        string TableName = "HKP.Issue";

        IssueMasterHKPService issueHKPService = new IssueMasterHKPService();

        private readonly ISqlRepository _sqlRepository;
        public IssueController(ISqlRepository R)
        { _sqlRepository = R; }

        // GET: Productions/Issue
        public ActionResult Aplos()
        {
            return View();
        }

        //[Authorize, HttpGet]
        //public JsonResult GetCbo()
        //{
        //    return Json(issueHKPService.GetCbo(), JsonRequestBehavior.AllowGet);
        //}

        //[Authorize, HttpPost]
        //public ActionResult Get(string Id)
        //{
        //    try
        //    {
        //        var _master = issueHKPService.Get(Id);


        //        return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }

        //}

        //[HttpPost, Authorize]
        //public ActionResult GetList(string column, string value)
        //{
        //    return Json(issueHKPService.GetList(column, value), JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet, Authorize]
        //public JsonResult GetAutoSequence()
        //{
        //    return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public JsonResult Create(Dictionary<string, object> data)
        //{
        //    try
        //    {
        //        string ret = issueHKPService.Create(data);
        //        if (ret == "Success")
        //        {
        //            return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });
        //        }
        //        else
        //        {
        //            return Json(new { Error = true, Message = ret });
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message });

        //    }
        //}

        //public ActionResult Delete(string id)
        //{
        //    try
        //    {

        //        string ret = issueHKPService.Delete(id);

        //        if (ret == "Success")
        //        {
        //            return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

        //    }


        }

        //private double GetSequence()
        //{
        //    DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
        //    if (dt.Rows.Count > 0)
        //        return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

        //    return 1;
        //}
    
}