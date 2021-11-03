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
    public class PlantCalendarController : BaseController
    {
        //Create,WeekoffAssign,HolidayAssign,WorkDayAssign

        #region Constructor

        private readonly IProductionOrderService _productionOrderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public PlantCalendarController(IProductionOrderService productionOrderService, IUnitOfWork U, ISqlRepository R)
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
        #endregion

        #region -- Operations

  
        [HttpPost,Authorize]
        public JsonResult Create()
        {
            try
            {
                DataSet dsMaster = null;
                DataSet dsConfig = null;
                string defaultWeekOff = "Sunday";
                double defaultWorkingHours = 10;


                string fromdate = System.DateTime.Now.ToString("dd-MMM-yyyy");
                string toDate = System.DateTime.Now.AddYears(1).AddMonths(1).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "select * from PlantCalendar where PlantId='" + identity.PlantId
                    + @"' and workingDate >= '" + fromdate + "'";
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
                }
                #endregion default weekoff




                //            


                string date = "";
                for (int i = 0; i < 1000; i++)//400 days
                {
                    date = System.DateTime.Now.AddDays(i).ToString("dd-MMM-yyyy");
                    dsMaster.Tables[0].DefaultView.RowFilter = "WorkingDate=#" + date + "#";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();



                        dr["PlantId"] = identity.PlantId;
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

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost,Authorize]
        public JsonResult WeekoffAssign(string wdate)
        {

            try
            {
                DataSet dsMaster = null;


                string fromdate = Convert.ToDateTime(wdate).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "select * from PlantCalendar where PlantId='" + identity.PlantId
                    + @"' and workingDate = '" + fromdate + "'";
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
        [HttpPost,Authorize]
        public JsonResult HolidayAssign(string holidayid, string fromdate, string toDate)
        {

            try
            {
                DataSet dsMaster = null;


                fromdate = Convert.ToDateTime(fromdate).ToString("dd-MMM-yyyy");
                toDate = Convert.ToDateTime(toDate).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "select * from PlantCalendar where PlantId='" + identity.PlantId
                    + @"' and workingDate between '" + fromdate + "' AND '" + toDate + "'";
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
        [HttpPost,Authorize]
        public JsonResult WorkDayAssign(string fromdate, string todate, double hours, double OT)
        {

            try
            {
                if (clsStaticInfo.dbl(hours.ToString()) <= 0)
                    throw new Exception("Hours cannot be zero");

                DataSet dsMaster = null;


                fromdate = Convert.ToDateTime(fromdate).ToString("dd-MMM-yyyy");
                todate = Convert.ToDateTime(todate).ToString("dd-MMM-yyyy");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "select * from PlantCalendar where PlantId='" + identity.PlantId
                    + @"' and workingDate between '" + fromdate + "' and '" + todate + "'";
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
        public JsonResult getDayStatus(string wdate)
        {
            wdate = Convert.ToDateTime(wdate).ToString("dd-MMM-yyyy");
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select isnull(convert(varchar(100),daytype),'Working Day') AS DayStatus,WorkingHours,OTHours,isnull(h.UserName,'') AS HolidayName from PlantCalendar C
                                left outer join [SCS].[HolidayCategory] H on c.HolidayCategory=H.id where
                                PlantId='" + identity.PlantId + "' and workingDate = '" + wdate + "'";


                return Json(new { DATA = _sqlRepository.GetDataCollection(sql), Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult getDayStatusRange(string wdate)
        {


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


							from PlantCalendar PT 
                            left outer join scs.HolidayCategory C on c.id=pt.HolidayCategory where pt.WorkingDate between '" + fromdate + "' and '" + todate + "' AND PlantId='" + identity.PlantId
                    + @"' ";


                return Json(new { DATA = _sqlRepository.GetDataCollection(sql), Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        #endregion


    }



}