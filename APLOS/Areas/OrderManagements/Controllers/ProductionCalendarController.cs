using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System.Data;
using Syncfusion.XlsIO;
using OTSBD;
using System.Linq;
using Library.Service.Enums;
using System.Text;
using System.Collections.Specialized;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class ProductionCalendarController : BaseController
    {

        #region Constructor

        private readonly IProductionOrderService _productionOrderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public ProductionCalendarController(IProductionOrderService productionOrderService, IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
            _productionOrderService = productionOrderService;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult Type2()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetProcessForPlanning(string entityid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT p.*
                          FROM [HKP].[EntityProcessTag] T
                          inner join hkp.Process p on p.id=t.ProcessId
                          where p.IsProductionProcess=1 and active=1 and t.EntityId='" + entityid + @"'
                          order by Sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPlanningType2EntityCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT DISTINCT E.Id,E.UserName FROM PlanningTypes AS pt 
INNER JOIN org.Entity E on e.Id=pt.EntityId
WHERE PT.PlanningType='PlanningType2' AND pt.CompanyGroupId='" + identity.CompanyGroupId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPlanningType2ProcessCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT DISTINCT P.Id,P.UserName FROM PlanningTypes AS pt 
INNER JOIN HKP.Process P ON P.id=pt.BaseProcessId
WHERE PT.PlanningType='PlanningType2' AND pt.CompanyGroupId='" + identity.CompanyGroupId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(string baseprocessid, string entityid, string column, string value)
        {
            string strkey = "1=1";
            if (column != null)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (
                        SELECT  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
                        ISNULL(PO.Qty,0) AS POQuantity,ISNULL(so.Qty,0) AS SOQuantity,t1.Color,FORMAT(t1.LSD,'dd-MMM-yyyy') AS LSD,FORMAT(t1.CommitmentDate,'dd-MMM-yyyy') AS CommitmentDate,t1.TargetPerDay
                                ,T1.ProductionPriority FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            INNER JOIN ProductionOrderSchedulingParametersType1 t1 ON t1.ProductionOrderID=po.Id
                            LEFT OUTER  JOIN (SELECT pod.ProductionOrderId,SUM(so.Qty) AS Qty
                                                FROM trn.SalesOrder AS so
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id GROUP BY pod.ProductionOrderId) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE PO.entityid='" + entityid + @"' AND PO.PlantId='" + identity.PlantId + @"' and PO.Id IN (SELECT DISTINCT pops.ProductionOrderId
                            FROM trn.ProductionOrderProcessSet AS pops WHERE pops.ProcessId = '" + baseprocessid
                            + @"') ) AS TEMP WHERE " + strkey + " ORDER BY ProductionPriority";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(string entityid, string processid)
        {
            if (string.IsNullOrEmpty(entityid) || string.IsNullOrEmpty(processid))
                return null;
            try
            {
                DataSet dsMaster = null;
                DataSet dsConfig = null;
                string defaultWeekOff = "Sunday";
                double defaultWorkingHours = 8;


                string fromdate = System.DateTime.Now.ToString("dd-MMM-yyyy");
                string toDate = System.DateTime.Now.AddYears(1).AddMonths(1).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "select * from ProductionPlanningCalendar where entityid='" + entityid
                    + @"' and processid='" + processid + "' and workingDate >= '" + fromdate + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                #region default weekoff
                sql = "select * from PlantWiseHRMSSetting where plantid='" + identity.PlantId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsConfig, false, "1");
                if (dsConfig.Tables[0].Rows.Count > 0)
                {
                    if (dsConfig.Tables[0].Rows[0]["DefaultWeekOff"].ToString() != "")
                        defaultWeekOff = dsConfig.Tables[0].Rows[0]["DefaultWeekOff"].ToString();
                    else
                        throw new Exception("Please set Default Week Off in Plant Wise HRMS Setting");
                }
                else
                {
                    throw new Exception("Please set Default Week Off in Plant Wise HRMS Setting");
                }
                #endregion default weekoff


                #region default working hours
                sql = "select * from EntityConfig where entityid='" + entityid + @"' and StandardName='" + EntityConfigParameter.StandardWorkingHoursPerDay + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsConfig, false, "1");
                if (dsConfig.Tables[0].Rows.Count > 0)
                {
                    if (clsStaticInfo.dbl(dsConfig.Tables[0].Rows[0]["Value"].ToString()) > 0)
                        defaultWorkingHours = clsStaticInfo.dbl(dsConfig.Tables[0].Rows[0]["Value"].ToString());
                    else
                        throw new Exception("Please set standard working hours in entity configuration");
                }
                else
                {
                    throw new Exception("Please set standard working hours in entity configuration");
                }
                #endregion default working hours


                //            


                string date = "";
                for (int i = 0; i < 3000; i++)//400 days
                {
                    date = System.DateTime.Now.AddDays(i).ToString("dd-MMM-yyyy");
                    dsMaster.Tables[0].DefaultView.RowFilter = "WorkingDate=#" + date + "#";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();



                        dr["EntityID"] = entityid;
                        dr["ProcessID"] = processid;
                        dr["WorkingDate"] = date;

                        dr["WorkingHours"] = defaultWorkingHours;

                        if (Convert.ToDateTime(date).ToString("dddd").ToUpper() == defaultWeekOff.ToUpper())
                        {
                            dr["WorkingHours"] = 0;
                            dr["DayType"] = "W";
                            dr["OTHours"] = 0;
                        }


                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;


                        dsMaster.Tables[0].Rows.Add(dr);

                    }
                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost, Authorize]
        public JsonResult WeekoffAssign(string entityid, string processid, string wdate)
        {

            try
            {
                DataSet dsMaster = null;


                string fromdate = Convert.ToDateTime(wdate).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "select * from ProductionPlanningCalendar where entityid='" + entityid
                    + @"' and processid='" + processid + "' and workingDate = '" + fromdate + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                if (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    DataRow dr = dsMaster.Tables[0].Rows[0];


                    dr.BeginEdit();

                    dr["WorkingHours"] = 0;
                    dr["DayType"] = "W";
                    dr["OTHours"] = 0;
                    dr["HolidayCategory"] = DBNull.Value;



                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();

                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMaster);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost, Authorize]
        public JsonResult HolidayAssign(string entityid, string processid, string holidayid, string fromdate, string toDate)
        {

            try
            {
                DataSet dsMaster = null;


                fromdate = Convert.ToDateTime(fromdate).ToString("dd-MMM-yyyy");
                toDate = Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "select * from ProductionPlanningCalendar where entityid='" + entityid
                    + @"' and processid='" + processid + "' and workingDate between '" + fromdate + "' AND '" + toDate + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");



                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {

                    DataRow dr = dsMaster.Tables[0].Rows[i];


                    dr.BeginEdit();



                    dr["DayType"] = "H";
                    dr["WorkingHours"] = 0;
                    dr["OTHours"] = 0;
                    dr["HolidayCategory"] = bplib.clsWebLib.RetValidLen(holidayid);


                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();

                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMaster);

                return Json(new { Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost, Authorize]
        public JsonResult WorkDayAssign(string entityid, string processid, string fromdate, string todate, double hours, double OT)
        {

            try
            {
                if (clsStaticInfo.dbl(hours.ToString()) <= 0)
                    throw new Exception("Hours cannot be zero");

                DataSet dsMaster = null;


                fromdate = Convert.ToDateTime(fromdate).ToString("dd-MMM-yyyy");
                todate = Convert.ToDateTime(todate).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "select * from ProductionPlanningCalendar where entityid='" + entityid
                    + @"' and processid='" + processid + "' and workingDate between '" + fromdate + "' and '" + todate + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {

                    DataRow dr = dsMaster.Tables[0].Rows[i];


                    dr.BeginEdit();


                    dr["DayType"] = DBNull.Value;
                    dr["HolidayCategory"] = DBNull.Value;
                    dr["OTHours"] = OT;
                    dr["WorkingHours"] = hours;



                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();

                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMaster);

                return Json(new { Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }


        [HttpPost, Authorize]
        public JsonResult getDayStatus(string entityid, string processid, string wdate)
        {
            wdate = Convert.ToDateTime(wdate).ToString("dd-MMM-yyyy");
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select isnull(convert(varchar(100),daytype),'Working Day') AS DayStatus,WorkingHours,OTHours,isnull(h.UserName,'') AS HolidayName from ProductionPlanningCalendar C
                                left outer join [SCS].[HolidayCategory] H on c.HolidayCategory=H.id where entityid='" + entityid
                    + @"' and processid='" + processid + "' and workingDate = '" + wdate + "'";


                return Json(new { DATA = _sqlRepository.GetDataCollection(sql), Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult getDayStatusRange(string entityid, string processid, string wdate)
        {

            if (string.IsNullOrEmpty(entityid) || string.IsNullOrEmpty(processid))
                return null;

            wdate = Convert.ToDateTime(wdate).ToString("dd-MMM-yyyy");
            try
            {
                string fromdate = Convert.ToDateTime(wdate).AddDays(-40).ToString("dd-MMM-yyyy");
                string todate = Convert.ToDateTime(wdate).AddDays(40).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT  pt.Id,pt.daytype,'true' AS AllDay,'false' AS Recurrence, pt.Id AS AppTaskId,pt.ID AS ParentId,

			                    case when isnull(pt.daytype,'')='' 
				                    then	'#188e00' 
			                    else 
				case when isnull(pt.daytype,'')='H' then '#e1e100' 
					else  case when isnull(pt.daytype,'')='W' then '#989898' else '#ffffff'
					end 
					end
					END AS Color,
                            FORMAT(pt.WorkingDate,'dd-MMM-yyyy') AS ProductionDate,
                            isnull(c.UserName,isnull(convert(varchar(100),pt.DayType),'Working Day')) AS [Description],
                            isnull(c.UserName,isnull(convert(varchar(100),pt.DayType),'Working Day')) AS  [Subject],
                            FORMAT(pt.WorkingDate,'MMM dd yyyy hh:mm:ss tt') AS StartTime,
                            FORMAT(pt.WorkingDate,'MMM dd yyyy')+' 11:59:59 PM' AS EndTime


							from ProductionPlanningCalendar PT 
                            left outer join scs.HolidayCategory C on c.id=pt.HolidayCategory where pt.WorkingDate between '" + fromdate + "' and '" + todate + "' AND entityid='" + entityid
                    + @"' and processid='" + processid + "'";


                return Json(new { DATA = _sqlRepository.GetDataCollection(sql), Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        #endregion

        #region Type2
        [HttpPost]
        public JsonResult CreateType2(string entityid, string processid)
        {
            if (string.IsNullOrEmpty(entityid) || string.IsNullOrEmpty(processid))
                return null;
            try
            {
                DataSet dsMaster = null;
                DataSet dsConfig = null;
                string defaultWeekOff = "";
                double defaultWorkingHours = 8;


                string fromdate = System.DateTime.Now.ToString("dd-MMM-yyyy");
                string toDate = System.DateTime.Now.AddYears(1).AddMonths(1).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "select * from ProductionPlanningType2Calendar where entityid='" + entityid
                    + @"' and processid='" + processid + "' and workingDate >= '" + fromdate + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                #region default weekoff
                sql = "select * from PlantWiseHRMSSetting where plantid='" + identity.PlantId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsConfig, false, "1");
                if (dsConfig.Tables[0].Rows.Count > 0)
                {
                    
                        defaultWeekOff = dsConfig.Tables[0].Rows[0]["PlanningType2WeekOff"].ToString();
                    //else
                    //    throw new Exception("Please set Default Week Off in Plant Wise HRMS Setting");
                }
                //else
                //{
                //    throw new Exception("Please set Default Week Off in Plant Wise HRMS Setting");
                //}
                #endregion default weekoff


                #region default working hours
                sql = "select * from EntityConfig where entityid='" + entityid + @"' and StandardName='" + EntityConfigParameter.StandardWorkingHoursPerDay + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsConfig, false, "1");
                if (dsConfig.Tables[0].Rows.Count > 0)
                {
                    if (clsStaticInfo.dbl(dsConfig.Tables[0].Rows[0]["Value"].ToString()) > 0)
                        defaultWorkingHours = clsStaticInfo.dbl(dsConfig.Tables[0].Rows[0]["Value"].ToString());
                    else
                        throw new Exception("Please set standard working hours in entity configuration");
                }
                else
                {
                    throw new Exception("Please set standard working hours in entity configuration");
                }
                #endregion default working hours


                //            


                string date = "";
                for (int i = 0; i < 3000; i++)//400 days
                {
                    date = System.DateTime.Now.AddDays(i).ToString("dd-MMM-yyyy");
                    dsMaster.Tables[0].DefaultView.RowFilter = "WorkingDate=#" + date + "#";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        dr["EntityID"] = entityid;
                        dr["ProcessID"] = processid;
                        dr["WorkingDate"] = date;
                        dr["WorkingHours"] = defaultWorkingHours;

                        if (Convert.ToDateTime(date).ToString("dddd").ToUpper() == defaultWeekOff.ToUpper())
                        {
                            dr["WorkingHours"] = 0;
                            dr["DayType"] = "W";
                            dr["OTHours"] = 0;
                        }

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;


                        dsMaster.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["DayType"] = defaultWeekOff;
                        dr["WorkingHours"] = defaultWorkingHours;
                        dr.EndEdit();
                    }
                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }

        [HttpPost, Authorize]
        public JsonResult WeekoffAssignType2(string entityid, string processid, string wdate)
        {

            try
            {
                DataSet dsMaster = null;


                string fromdate = Convert.ToDateTime(wdate).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "select * from ProductionPlanningType2Calendar where entityid='" + entityid
                    + @"' and processid='" + processid + "' and workingDate = '" + fromdate + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                if (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    DataRow dr = dsMaster.Tables[0].Rows[0];


                    dr.BeginEdit();

                    dr["WorkingHours"] = 0;
                    dr["DayType"] = "W";
                    dr["OTHours"] = 0;
                    dr["HolidayCategory"] = DBNull.Value;



                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();

                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMaster);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost, Authorize]
        public JsonResult HolidayAssignType2(string entityid, string processid, string holidayid, string fromdate, string toDate)
        {

            try
            {
                DataSet dsMaster = null;


                fromdate = Convert.ToDateTime(fromdate).ToString("dd-MMM-yyyy");
                toDate = Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "select * from ProductionPlanningType2Calendar where entityid='" + entityid
                    + @"' and processid='" + processid + "' and workingDate between '" + fromdate + "' AND '" + toDate + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");



                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {

                    DataRow dr = dsMaster.Tables[0].Rows[i];


                    dr.BeginEdit();



                    dr["DayType"] = "H";
                    dr["WorkingHours"] = 0;
                    dr["OTHours"] = 0;
                    dr["HolidayCategory"] = bplib.clsWebLib.RetValidLen(holidayid);


                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();

                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMaster);

                return Json(new { Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost, Authorize]
        public JsonResult WorkDayAssignType2(string entityid, string processid, string fromdate, string todate, double hours, double OT)
        {

            try
            {
                if (clsStaticInfo.dbl(hours.ToString()) <= 0)
                    throw new Exception("Hours cannot be zero");

                DataSet dsMaster = null;


                fromdate = Convert.ToDateTime(fromdate).ToString("dd-MMM-yyyy");
                todate = Convert.ToDateTime(todate).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "select * from ProductionPlanningType2Calendar where entityid='" + entityid
                    + @"' and processid='" + processid + "' and workingDate between '" + fromdate + "' and '" + todate + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {

                    DataRow dr = dsMaster.Tables[0].Rows[i];


                    dr.BeginEdit();


                    dr["DayType"] = DBNull.Value;
                    dr["HolidayCategory"] = DBNull.Value;
                    dr["OTHours"] = OT;
                    dr["WorkingHours"] = hours;



                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();

                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMaster);

                return Json(new { Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }


        [HttpPost, Authorize]
        public JsonResult getDayStatusType2(string entityid, string processid, string wdate)
        {
            wdate = Convert.ToDateTime(wdate).ToString("dd-MMM-yyyy");
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select isnull(convert(varchar(100),daytype),'Working Day') AS DayStatus,WorkingHours,OTHours,isnull(h.UserName,'') AS HolidayName from ProductionPlanningType2Calendar C
                                left outer join [SCS].[HolidayCategory] H on c.HolidayCategory=H.id where entityid='" + entityid
                    + @"' and processid='" + processid + "' and workingDate = '" + wdate + "'";


                return Json(new { DATA = _sqlRepository.GetDataCollection(sql), Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult getDayStatusRangeType2(string entityid, string processid, string wdate)
        {

            if (string.IsNullOrEmpty(entityid) || string.IsNullOrEmpty(processid))
                return null;

            wdate = Convert.ToDateTime(wdate).ToString("dd-MMM-yyyy");
            try
            {
                string fromdate = Convert.ToDateTime(wdate).AddDays(-40).ToString("dd-MMM-yyyy");
                string todate = Convert.ToDateTime(wdate).AddDays(40).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT  pt.Id,pt.daytype,'true' AS AllDay,'false' AS Recurrence, pt.Id AS AppTaskId,pt.ID AS ParentId,

			                    case when isnull(pt.daytype,'')='' 
				                    then	'#188e00' 
			                    else 
				case when isnull(pt.daytype,'')='H' then '#e1e100' 
					else  case when isnull(pt.daytype,'')='W' then '#989898' else '#ffffff'
					end 
					end
					END AS Color,
                            FORMAT(pt.WorkingDate,'dd-MMM-yyyy') AS ProductionDate,
                            isnull(c.UserName,isnull(convert(varchar(100),pt.DayType),'Working Day')) AS [Description],
                            isnull(c.UserName,isnull(convert(varchar(100),pt.DayType),'Working Day')) AS  [Subject],
                            FORMAT(pt.WorkingDate,'MMM dd yyyy hh:mm:ss tt') AS StartTime,
                            FORMAT(pt.WorkingDate,'MMM dd yyyy')+' 11:59:59 PM' AS EndTime


							from ProductionPlanningType2Calendar PT 
                            left outer join scs.HolidayCategory C on c.id=pt.HolidayCategory where pt.WorkingDate between '" + fromdate + "' and '" + todate + "' AND entityid='" + entityid
                    + @"' and processid='" + processid + "'";


                return Json(new { DATA = _sqlRepository.GetDataCollection(sql), Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        #endregion Type2

    }

    public class ProductionPlanningCalendar : BaseModel
    {
        public string ID { get; set; } = "";
        public string EntityID { get; set; } = "";
        public string ProcessID { get; set; } = "";
        public string WorkingDate { get; set; } = "";
        public string DayType { get; set; } = "";
        public string HolidayCategory { get; set; } = "";
        public double WorkingHours { get; set; } = 0;
        public double OTHours { get; set; } = 0;

    }

}