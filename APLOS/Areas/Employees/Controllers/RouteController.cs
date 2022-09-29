#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Threading;
using Library.Data.Sql;
using Library.Crosscutting.Security;
using System;
using System.Data;
using OTSBD;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class RouteController : BaseController
    {
        #region Constructor
        private readonly IRouteService _routeService;
        private readonly IRouteStoppageService _routeStoppageService;
        private readonly ISqlRepository _sqlRepository;

        public RouteController(
              IRouteService routeService
            , IRouteStoppageService routeStoppageService
             , ISqlRepository sqlRepository

            )
        {
            _routeService = routeService;
            _routeStoppageService = routeStoppageService;
            _sqlRepository = sqlRepository;

        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_routeService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select r.* from [MST].[Route] r where r.PlantId='" + identity.PlantId + "' and r.CompanyId='" + identity.CompanyId + "' order by r.code";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet,Authorize]
        public ActionResult getRouteStopage(string RouteId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select s.Id as StopagePrimaryId,s.CityId,rs.Sequence,S.Code,S.ShortName,S.StandardName,S.UserName,S.[Description]
							 ,rs.UpDistanceFrom,rs.DownDistanceFrom
                              from [HKP].[Stoppage] s 
                              left join  [MST].[RouteStoppage] rs on rs.StoppageId=s.Id
                              left join [MST].[Route] r on r.Id=rs.RouteId
                                where r.Id='" + RouteId + @"'
	                            and s.CompanyId='" + identity.CompanyId + @"' and s.CompanyGroupId='" + identity.CompanyGroupId + @"' order by rs.Sequence";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet,Authorize]
        public ActionResult getStopageData(string routeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select RS.*,S.UserName,C.UserName as City from MST.RouteStoppage RS
                            left outer join HKP.Stoppage as S on RS.StoppageId=S.Id
                            left outer join SCS.City as C on S.CityId=C.Id
                            where RS.RouteId='" + routeId + "'order by Sequence  ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Authorize]
        public ActionResult GetStopageInformation()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT null Id,s.Id as StopagePrimaryId,s.CityId,s.Sequence,S.Code,S.ShortName,S.StandardName,S.UserName,S.[Description],s.Active
                            FROM [HKP].[Stoppage] s 
                                WHERE s.CompanyId='" + identity.CompanyId + "' and s.CompanyGroupId='" + identity.CompanyGroupId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetTransportDetails()
        {
            string sql = @"select * from TransportDetail Order By TransportNo";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Save(RouteModel data, List<Dictionary<string, object>> StopageList)
        {
            try
            {
                string RouteId = string.Empty;
                //RouteId = SaveRoute(Route);
                //SaveStopageList(Route, StopageList, RouteId, out DataSet dsDelete);
                SaveData(data, out string Id, StopageList);
                data.Id = Id;
                return Json(new { Route = data, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public string SaveRoute(RouteModel Route)
        {
            string Id = string.Empty;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM [MST].[Route] WHERE ID='" + Route.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MST.Route", out sID);
                    Id = "R" + sID;
                    dr["Id"] = Id;
                    dr["PlantId"] = identity.PlantId;
                    dr["CompanyId"] = identity.CompanyId;

                    dr["Code"] = Route.Code;
                    dr["UserName"] = Route.UserName;
                    dr["StandardName"] = Route.StandardName;
                    dr["ShortName"] = Route.ShortName;
                    dr["Description"] = Route.Description;
                    dr["Remarks"] = Route.Remarks;
                    dr["Totalkm"] = Route.Totalkm;
                    dr["Active"] = Route.Active;
                    dr["From"] = Route.From;
                    dr["To"] = Route.To;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();

                    Id = dr["Id"].ToString();

                    dr["PlantId"] = identity.PlantId;
                    dr["CompanyId"] = identity.CompanyId;

                    dr["Code"] = Route.Code;
                    dr["UserName"] = Route.UserName;
                    dr["StandardName"] = Route.StandardName;
                    dr["ShortName"] = Route.ShortName;
                    dr["Description"] = Route.Description;
                    dr["Remarks"] = Route.Remarks;
                    dr["Totalkm"] = Route.Totalkm;
                    dr["Active"] = Route.Active;
                    dr["From"] = Route.From;
                    dr["To"] = Route.To;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SaveData(RouteModel data, out string Id, List<Dictionary<string, object>> StopageList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsChild;
            string contId = string.Empty;
            string id = string.Empty;
            try
            {

                string sql = "SELECT * FROM [MST].[Route] WHERE ID='" + data.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                

                objCon.OpenDataSetThroughAdapter("select * from [MST].[Route] where Code='" + data.Code + "' AND  Id<>'" + data.Id + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                objCon.OpenDataSetThroughAdapter("select * from [MST].[Route] where UserName='" + data.UserName + "' AND  Id<>'" + data.Id + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "Route", out sID);
                    Id = "R" + sID;
                    dr["Id"] = Id;
                    dr["PlantId"] = identity.PlantId;
                    dr["CompanyId"] = identity.CompanyId;

                    dr["Code"] = data.Code;
                    dr["UserName"] = data.UserName;
                    dr["StandardName"] = data.StandardName;
                    dr["ShortName"] = data.ShortName;
                    dr["Description"] = data.Description;
                    dr["Remarks"] = data.Remarks;
                    dr["Totalkm"] = data.Totalkm;
                    dr["Active"] = data.Active;
                    dr["From"] = data.From;
                    dr["To"] = data.To;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();

                    Id = dr["Id"].ToString();

                    dr["PlantId"] = identity.PlantId;
                    dr["CompanyId"] = identity.CompanyId;

                    dr["Code"] = data.Code;
                    dr["UserName"] = data.UserName;
                    dr["StandardName"] = data.StandardName;
                    dr["ShortName"] = data.ShortName;
                    dr["Description"] = data.Description;
                    dr["Remarks"] = data.Remarks;
                    dr["Totalkm"] = data.Totalkm;
                    dr["Active"] = data.Active;
                    dr["From"] = data.From;
                    dr["To"] = data.To;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }
                Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                #region StopageList 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [MST].[RouteStoppage] where  RouteId='" + Id + "'", out dsChild, false, "1");
                if (StopageList != null)
                {
                    int _Count = 0;
                    foreach (var item in StopageList)
                    {
                        _Count++;
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "StoppageId='" + item["StoppageId"] + "'";

                       
                        if (dv.Count == 0)
                        {
                            item["Id"] = GetStopagePK();
                            item["RouteId"] = Id;
                            item["Sequence"] = _Count;

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            item["Id"] = dv[0].Row["Id"].ToString();
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsChild);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private string GetStopagePK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "RouteStoppage", out sID);
            return sID;
        }
        public void SaveStopageList(RouteModel Route, List<StopageListModel> StopageList, string RouteId, out DataSet dsDelete)
        {
            int _Count = 0;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            dsDelete = null;
            try
            {
                string EmpIdLoop = "";

                foreach (var item in StopageList)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "'" + item.StopagePrimaryId + "'";
                    }
                    else
                    {
                        EmpIdLoop += ",'" + item.StopagePrimaryId + "'";

                    }
                }
                DeleteStopage(EmpIdLoop, RouteId, out dsDelete);

                foreach (var item in StopageList)
                {
                     _Count ++;
                    //if (item.Time == null)
                    //{
                    //    item.Time = "12:00 AM";
                    //}
                    string sql = "SELECT * FROM [MST].[RouteStoppage] WHERE ID='" + item.Id + "' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    DataView DvMaster = new DataView(dsMaster.Tables[0]);

                    
                    if (DvMaster.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MST.RouteStoppage", out sID);

                        dr["Id"] = "RS" + sID;

                        dr["RouteId"] = RouteId;
                        dr["StoppageId"] = item.StopagePrimaryId;

                        //if (Route.UpOrDown == "Up")
                        //    dr["UpDistanceFrom"] = item.Time;
                        //else
                        //    dr["DownDistanceFrom"] = item.Time;

                        dr["UpDistanceFrom"] = item.UpDistanceFrom;
                        dr["DownDistanceFrom"] = item.DownDistanceFrom;

                        dr["Sequence"] = _Count;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = DvMaster[0].Row;
                        dr.BeginEdit();

                        dr["RouteId"] = item.RouteId;
                        dr["StoppageId"] = item.StoppageId;
                        //if (Route.UpOrDown == "Up")
                        //    dr["UpDistanceFrom"] = item.Time;
                        //else
                        //    dr["DownDistanceFrom"] = item.Time;

                        dr["UpDistanceFrom"] = item.UpDistanceFrom;
                        dr["DownDistanceFrom"] = item.DownDistanceFrom;

                        dr["Sequence"] = item.Sequence;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                    DvMaster.RowFilter = null;
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

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




            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
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
        public JsonResult CreateChild(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from TransportDetail where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "TransportDetail", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateRouteSchedule(Dictionary<string, object> data)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                //con.OpenDataSetThroughAdapter("select * from RouteScheduleChild where RouteScheduleId='" + RouteShChild["Id"] + "'", out dsRouteShChild, false, "1");

                con.OpenDataSetThroughAdapter("select * from RouteSchedule where Id <>'" + data["Id"] + "' AND TripNo ='" + data["TripNo"] + "'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Trip No already exists!!!");

                con.OpenDataSetThroughAdapter("select * from RouteSchedule where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "RouteSchedule", out _Id);

                    data["Id"] = _Id;

                    data["AddedBy"] = identity.Name;
                    data["AddedDate"] = System.DateTime.Now.ToString();
                    data["AddedFromIP"] = identity.IPAddress;
                    data["UpdatedBy"] = identity.Name;
                    data["UpdatedDate"] = System.DateTime.Now.ToString();
                    data["UpdatedFromIP"] = identity.IPAddress;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateRouteSheduleChildDetails(Dictionary<string, object> RouteShChild,string RouteScheduleId)
        {
            try
            {
                DataSet dsRouteShChild, dsRouteUp, dsRouteDown;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from RouteScheduleChild where Id='" + RouteShChild["Id"] + "'", out dsRouteShChild, false, "1");

                if (RouteShChild["UpDown"].ToString()=="Up")
                {
                    con.OpenDataSetThroughAdapter("select * from RouteScheduleChild where Id <>'" + RouteShChild["Id"] + "' and UpDown='Up' and RouteScheduleId='"+ RouteScheduleId + "'", out dsRouteUp, false, "1");

                    if (dsRouteUp.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Up is already exists!");
                    }
                }
                if (RouteShChild["UpDown"].ToString() == "Down")
                {
                    con.OpenDataSetThroughAdapter("select * from RouteScheduleChild where Id <>'" + RouteShChild["Id"] + "' and UpDown='Down' and RouteScheduleId='" + RouteScheduleId + "'", out dsRouteDown, false, "1");
                    if (dsRouteDown.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Down is already exists!");
                    }
                }

                
                string Id = "";

                #region data update
                if (dsRouteShChild.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "RouteScheduleChild", out Id);

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    DataRow dr;
                    dr = dsRouteShChild.Tables[0].NewRow();

                    dr["Id"] = Id;
                    dr["RouteScheduleId"] = RouteScheduleId;
                    dr["StartTime"] = RouteShChild["StartTime"];
                    dr["EndTime"] = RouteShChild["EndTime"];
                    dr["UpDown"] = RouteShChild["UpDown"];
                    dr["Remarks"] = RouteShChild["Remarks"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsRouteShChild.Tables[0].Rows.Add(dr);
                    //AddNewRow(dsRouteShChild.Tables[0], RouteShChild);
                }
                else
                {
                    Id = RouteShChild["Id"].ToString();
                    EditRow(dsRouteShChild.Tables[0].Rows[0], RouteShChild);
                }
                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRouteShChild);


                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpPost]
        public ActionResult RouteScheduleChildDelete(string Id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from RouteScheduleChild where Id ='" + Id + "'");

                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        [Authorize, HttpGet]
        public ActionResult GetRouteScheduleChilddata(string tripId)
        {
            string sql = @"select RSC.Id,RSC.UpDown,RSC.Remarks,ISNULL(format(RSC.StartTime,'hh:mm tt'),'')StartTime,ISNULL(format(RSC.EndTime,'hh:mm tt'),'') EndTime
										from RouteScheduleChild RSC
                                        where RSC.RouteScheduleId='" + tripId + @"'
                                        order by RSC.AddedDate";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult TransportDetailsDelete(string id)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sqlr = @"select * from routeschedule where TransportId = '" + id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Already used in Route Schedule!!!");
                }


                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from TransportDetail where Id ='" + id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        void DeleteStopage(string StopagePrimaryId,string RouteId, out System.Data.DataSet dsRef)
        {
           // string strSQL;
            string strSQLR;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"Delete FROM [MST].[RouteStoppage]  where StoppageId in (" + StopagePrimaryId + @")";
                strSQLR = @"Delete FROM [MST].[RouteStoppage]  where RouteId='"+ RouteId+"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
                objCon.OpenDataSetThroughAdapter(strSQLR, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        [HttpGet]
        public ActionResult Delete(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                DataSet dsMaster;
                string sqlr = @"select * from routeschedule where RouteId = '" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count>0)
                {
                    throw new Exception("Already used in Route Schedule!!!");
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sqlStopage = @"delete from [MST].[RouteStoppage]  WHERE RouteId='" + Id + @"'";
                string sql = @"delete from [MST].[Route] WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsExceptionEmployeeList, false, "1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetRoute()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select Id,UserName from mst.Route";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetShift()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select SD.SystemID ShiftId,P.Id PlantId,P.UserName Plant,SD.ShiftDefinationDescription,SD.UserName ShiftDefination 
						,CONVERT(varchar(5),SD.InTime,108) InTime,CONVERT(VARCHAR(5), SD.InTime, 108) OutTime
						
						from ShiftDefination SD
						left join ORG.Plant P on P.Id=SD.PlantID";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetTransport()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select Id,TransportUserName+'-'+TransportNo  as UserName from TransportDetail order by TransportNo";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetRouteScheduleTransport(string routeScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select RST.TransportId routeScheduleTransportId,TD.TransportUserName RouteScheduleTransport from RouteScheduleTransport RST
							left join TransportDetail TD on TD.Id=RST.TransportId
                            where RouteScheduleId='" + routeScheduleId + @"'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetRouteSchedule()
        {
            string sql = @"SELECT RS.*,SD.UserName [Shift],R.UserName [Route],TD.TransportUserName+'-'+TD.TransportNo Transport										
FROM RouteSchedule RS
LEFT JOIN mst.[Route] R on R.Id=RS.RouteId
LEFT JOIN TransportDetail TD on TD.Id=RS.TransportId
LEFT JOIN ShiftDefination SD on SD.SystemID=RS.ShiftId
Order By RS.TripNo";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult RouteScheduleDelete(string id)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sqlr = @"select * from EmployeeTransportAllocation where TripId = '" + id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Already used in Employee Transport Allocation!!!");
                }

                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from RouteScheduleTransport where RouteScheduleId ='" + id + "'");
                conC.executeQuery("delete from RouteSchedule where Id ='" + id + "'");

                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        [Authorize, HttpPost]
        public ActionResult GetDistance(Dictionary<string, object> data)
        {
            var ts = Convert.ToDateTime(data["EndTime"]).Subtract(Convert.ToDateTime(data["StartTime"]));
            var dif =Convert.ToDecimal(ts.TotalMinutes) / Convert.ToDecimal(data["Distance"]);

            return Json(dif, JsonRequestBehavior.AllowGet);
        }

        public class RouteModel : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string PlantId { get; set; }
            public string CompanyId { get; set; }
            public string Code { get; set; }
            public string UserName { get; set; }
            public string StandardName { get; set; }
            public string ShortName { get; set; }
            public string Description { get; set; }
            public string Remarks { get; set; }
            public decimal Totalkm { get; set; }
            public bool Active { get; set; }
            public string UpOrDown { get; set; }
            public string From { get; set; }
            public string To { get; set; }

            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }

            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }
        public class StopageListModel : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string StopagePrimaryId { get; set; }
            public string RouteId { get; set; }
            public string StoppageId { get; set; }
            public string UpDistanceFrom { get; set; }
            public string DownDistanceFrom { get; set; }
            public string Time { get; set; }
            public string Sequence { get; set; }

            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }

            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }
        #endregion
    }
}