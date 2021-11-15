using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Library.HumanResource.NewAttendanceProcess;
using Newtonsoft.Json;
//using TBS;

namespace Aplos.Areas.HumanResource.Controllers
{

    public class OTConfirmationProcessController : BaseController
    {
        
        #region Constructor
        
        OTConfirmationProcessService ot = new OTConfirmationProcessService();
        public OTConfirmationProcessController()
        {
        }
        #endregion

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages


        #region Operations

        [Authorize , HttpGet]
        public ActionResult getFilters()
        {
            return Json(ot.getFilters(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getDayTypes()
        {
            return Json(ot.getDayTypes(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost,Authorize]
        public ActionResult getGridData(string Week, string FromDate, string ToDate, string OTConfirmationValue, string OTLimit, string Process, string ProcessValue, string DayStatus
 , string DSApp, Dictionary<string , string> Parameters)
        {
            var json = Json(ot.getGridData(Week, FromDate, ToDate, OTConfirmationValue, OTLimit, Process, ProcessValue, DayStatus, DSApp, Parameters), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost , Authorize]
        public void ProcessData(string Data,string OTWeek)
        {
            List<Dictionary<string,object>> _objects = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(Data);
            var StringDates = new List<DateTime>();

            #region To Find Max & Min Date

            string WorkDatesMaster = "''";

            foreach (Dictionary<string, object> AllWorkDates in _objects)
            {
                if (AllWorkDates.ContainsKey("WorkDate"))
                {

                    string value = AllWorkDates["WorkDate"].ToString();
                    string Param = "";
                    DistinctFunction(ref WorkDatesMaster, value,out Param);
                    if (Param == "1")
                    {
                        StringDates.Add(Convert.ToDateTime(value));
                    }
                }
            }

            DateTime MaxDate = StringDates.Max(date => date);
            DateTime MinDate = StringDates.Min(date => date);

            #endregion

            DataTable OTProcessTable = ToDataTable(_objects);

            //DataTable dx = new DataTable();
            //dx.Columns.Add("empcode", typeof(string));

            //dx.Columns.Add("workdate", typeof(DateTime));
            string DailyLimit = "";
            if ( DailyLimit=="0")
            {
                decimal StdOT = 0;
                decimal ExtraOT; // All;
            }
            // var EmpData= Data.Where(k => k["empsystemid"].ToString() == "1223" && ).FirstOrDefault(); 

        }

        public void DistinctFunction(ref string WorkDatesMaster, string Value,out string Param)
        {
            if (WorkDatesMaster.Contains(Value))
            {
                Param = "0";
                return;
            }
            else
            {
                Param = "1";
                WorkDatesMaster += ",'" + Value + "'";
            }
        }

        static DataTable ToDataTable(List<Dictionary<string, object>> list)
        {
            DataTable result = new DataTable();
            if (list.Count == 0)
                return result;

            result.Columns.AddRange(
                list.First().Select(r => new DataColumn(r.Key)).ToArray()
            );

            list.ForEach(r => result.Rows.Add(r.Select(c => c.Value).Cast<object>().ToArray()));

            return result;
        }

        #endregion Operations
    }
} 