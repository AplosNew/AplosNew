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

namespace Aplos.Areas.IE.Controllers
{
    public class MachineMapController : Controller
    {
        #region Constructor



       
       MachineMapping machine = new MachineMapping();
        public MachineMapController(
            
            )
        {
           
        }

        #endregion Constructor

        #region -- Pages


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
            return Json(machine.leftGridData(parameters), JsonRequestBehavior.AllowGet);
        }

        

        [HttpGet, Authorize]
        public ActionResult allFilterLists()
        {
            JsonResult result =  Json(machine.allFilterLists(), JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue;
            return result;
        }

        [HttpGet, Authorize]
        public JsonResult GetPivotData()
        {
            var data = machine.DateWiseSkillData(out List<string> ColumnList);
            return Json(new { DATA = data, Columns = ColumnList }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public JsonResult GetScheduleDataFiltered( Dictionary<string, string> parameters , string fromDate , string toDate)
        {
            var data = machine.FilterWiseMachineData(out List<string> ColumnList,  parameters , fromDate , toDate , out List<Dictionary<string, object>> dt);
            return Json(new { DATA = data, Columns = ColumnList , Compact = dt}, JsonRequestBehavior.AllowGet);
        }
        [HttpPost , Authorize]
        public ActionResult allotedWorkCenter(string machineId , Dictionary<string, string> parameters, string date)
        {
            return Json(machine.allotedWorkCenter(parameters ,machineId, date), JsonRequestBehavior.AllowGet);
        }

        [HttpPost , Authorize]
        public JsonResult GetScheduleDataArticleFiltered ( Dictionary<string, string> parameters, string fromDate, string toDate)
        {
            var data = machine.FilterWiseArticleData(out List<string> ColumnList, parameters, fromDate, toDate , out List<Dictionary<string, object>> dt);
            return Json(new { DATA = data, Columns = ColumnList , Compact = dt }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult allotedArticleWorkCenter(string machineVId, Dictionary<string, string> parameters, string date)
        {
            return Json(machine.allotedArticleWorkCenter(parameters, machineVId, date), JsonRequestBehavior.AllowGet);
        }
    }

}