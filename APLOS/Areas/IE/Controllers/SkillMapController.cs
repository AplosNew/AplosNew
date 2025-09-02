using Library.Model.IE;
using Aplos.Properties;
using Library.Data;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using Library.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using Library.Service.IE;
using Library.Model.Inventory;
using Library.Service.Systems;
using Library.Service.Enums;
using Library.HumanResource.Dashboard;
using System.Data;

namespace Aplos.Areas.IE.Controllers
{
    public class SkillMapController : Controller
    {
        #region Constructor



       
       SkillMapping skill = new SkillMapping();
        public SkillMapController(
            
            )
        {
           
        }

        #endregion Constructor

        #region -- Pages

        [Authorize, HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }


        #endregion -- Pages

       // [HttpGet , Authorize]
       // public ActionResult CompanyList()
       // {
       //     return Json(_skillMap.CompanyList() , JsonRequestBehavior.AllowGet);
       // }

       //[HttpGet , Authorize]
       // public ActionResult PlantList(string company)
       // {
       //     return Json(_skillMap.PlantList(company) , JsonRequestBehavior.AllowGet);
       // }

       // [HttpGet, Authorize]
       // public ActionResult EntityList(string company)
       // {
       //     return Json(_skillMap.EntityList(company), JsonRequestBehavior.AllowGet);
       // }

        [HttpPost , Authorize]
        public ActionResult leftGridData(Dictionary<string, string> parameters)
        {
            return Json(skill.leftGridData(parameters), JsonRequestBehavior.AllowGet);
        }

        

        [HttpGet, Authorize]
        public ActionResult allFilterLists()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult result =  Json(skill.allFilterLists(identity.PlantId), JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue;
            return result;
        }

        [HttpGet, Authorize]
        public JsonResult GetPivotData()
        {
            var data = skill.DateWiseSkillData(out List<string> ColumnList);
            return Json(new { DATA = data, Columns = ColumnList }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public JsonResult GetScheduleDataFiltered( Dictionary<string, string> parameters , string fromDate , string toDate)
        {
            //List<Dictionary<string, object>> dt = new List<Dictionary<string, object>>();
            
            var data = skill.FilterWiseSkillData(out List<string> ColumnList, parameters, fromDate, toDate, out List<Dictionary<string, object>> dt);
            
            return Json(new { DATA = data, Columns = ColumnList , Compact = dt}, JsonRequestBehavior.AllowGet);
        }
        [HttpPost , Authorize]
        public ActionResult allotedWorkCenter(Dictionary<string, string> parameters,  string skillId , string date)
        {
            return Json(skill.allotedWorkCenter(parameters ,skillId, date), JsonRequestBehavior.AllowGet);
        }

        [HttpPost , Authorize]
        public ActionResult skillwiseEmployee(string code , string shifts , string seq, string companyId)
        {
            return Json(skill.skillWiseEmployees(code , shifts , seq, companyId), JsonRequestBehavior.AllowGet);
        }
    }

}