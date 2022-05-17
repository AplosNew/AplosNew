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
            string sql = @"select r.Code,r.ShortName,r.StandardName,r.UserName,r.Active,r.[Description],r.Remarks,R.Id
                            from [MST].[Route] r
                            where r.PlantId='" + identity.PlantId + "' and r.CompanyId='" + identity.CompanyId + "' ";
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
            string sql = @" select s.Id as StopagePrimaryId,s.CityId,s.Sequence,S.Code,S.ShortName,S.StandardName,S.UserName,S.[Description]
                            from [HKP].[Stoppage] s 
                                where s.CompanyId='" + identity.CompanyId + "' and s.CompanyGroupId='" + identity.CompanyGroupId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        
        [HttpPost]
        public ActionResult Save(RouteModel Route, List<StopageListModel> StopageList)
        {
            try
            {
                string RouteId = string.Empty;
                RouteId = SaveRoute(Route);
                SaveStopageList(Route, StopageList, RouteId, out DataSet dsDelete);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
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
                    //dr["DriverId"] = Route.DriverId;
                    //dr["AssetId"] = Route.AssetId;
                    dr["UpOrDown"] = Route.UpOrDown;

                    dr["Code"] = Route.Code;
                    dr["UserName"] = Route.UserName;
                    dr["StandardName"] = Route.StandardName;
                    dr["ShortName"] = Route.ShortName;
                    dr["Description"] = Route.Description;
                    dr["Remarks"] = Route.Remarks;
                    dr["Active"] = Route.Active;

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
                    //dr["DriverId"] = Route.DriverId;
                    //dr["AssetId"] = Route.AssetId;
                    dr["UpOrDown"] = Route.UpOrDown;

                    dr["Code"] = Route.Code;
                    dr["UserName"] = Route.UserName;
                    dr["StandardName"] = Route.StandardName;
                    dr["ShortName"] = Route.ShortName;
                    dr["Description"] = Route.Description;
                    dr["Remarks"] = Route.Remarks;
                    dr["Active"] = Route.Active;

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
        public JsonResult CreateChild(Dictionary<string, object> data, string RouteId)
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
                    data["RouteId"] = RouteId;
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
        public JsonResult CreateRouteSchedule(Dictionary<string, object> data, string RouteId,string transportId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataRow dr;
                DataSet dsMaster, dsTransport;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from RouteSchedule where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter("select * from RouteScheduleTransport where RouteScheduleId ='" + data["Id"] + "'", out dsTransport, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "RouteSchedule", out _Id);

                    data["Id"] = _Id;
                    data["RouteId"] = RouteId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                #region Transport 

                while (dsTransport.Tables[0].DefaultView.Count > 0)
                    dsTransport.Tables[0].DefaultView[0].Delete();
                int count = 0;
                if (dsTransport != null)
                {
                    string[] transports = transportId.Split(',');
                    foreach (string item in transports)
                    {
                        dr = dsTransport.Tables[0].NewRow();
                        count++;
                        string pk = _Id + "_" + count;
                        dr["Id"] = pk;
                        dr["TransportId"] = item;
                        dr["RouteScheduleId"] = _Id;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsTransport.Tables[0].Rows.Add(dr);
                    }

                    #endregion Transport 


                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsTransport);



                }
                    return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpPost]
        public ActionResult GetTransportDetails(string RouteId)
        {
            string sql = @"select TD.*,R.UserName Route,EI.EmployeeCode DriverCode,EI.EmployeeName DriverName 
			                            from TransportDetail TD
			                            left join MST.Route R on R.Id=TD.RouteId
			                            left join EmployeeInformation EI on EI.SystemId=TD.DriverId
										where TD.RouteId='" + RouteId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult TransportDetailsDelete(string id)
        {
            try
            {
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
            string str = @"select Id as Value,TransportUserName as Text from TransportDetail";

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
        public ActionResult GetRouteSchedule(string RouteId)
        {
            string sql = @"select RS.Id,RS.ShiftId,SD.UserName [Shift],CONVERT(varchar(5)
										,RS.StartTime,108) StartTime,CONVERT(VARCHAR(5), RS.EndTime, 108) EndTime 
										,RS.TripNo,RS.[From],RS.[To],RS.UpDown,RS.Distance,RS.DistancePerUnit,RS.Remarks
										,Stuff((Select ','+TD.TransportUserName
										from dbo.RouteScheduleTransport RST 
										left join TransportDetail TD on TD.Id=RST.TransportId
										where RST.RouteScheduleId = RS.Id
										for xml path('')
										),1,1,'') as RouteScheduleTransport
                                        from RouteSchedule RS
                                        left join ShiftDefination SD on SD.SystemID=RS.ShiftId
                                        where RS.RouteId='" + RouteId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult RouteScheduleDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
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
            public bool Active { get; set; }
            public string UpOrDown { get; set; }

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