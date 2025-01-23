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
using Library.OrderManagement.Production;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class WCWorkStationsControlMasterController : BaseController
    {

        WorkStationsContolService ws = new WorkStationsContolService();
        
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public WCWorkStationsControlMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }
      

        [Authorize, HttpGet]
        public ActionResult getProcess()
        {
            return Json(ws.getProcess(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetProcessReasonList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from HKP.Process where Active=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadReasonDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select *,(select P.UserName from HKP.Process P where P.Id=RD.ProcessId) as Process from [MST].[WSMReasonDetails] RD";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadReasonDetailsEditData(string ReasonId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select *,(select P.UserName from HKP.Process P where P.Id=RD.ProcessId) as Process from [MST].[WSMReasonDetails] RD where RD.Id='" + ReasonId + @"'";
            return Json(new { Reason = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult createReason(Dictionary<string, object> ReasonData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[WSMReasonDetails] where ReasonName='" + ReasonData["ReasonName"] + "' and ProcessId='" + ReasonData["ProcessId"] + "'", out DataSet dsItemDetailsReasonNameValidation, false, "1");

                DataSet dsReasonDetails;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[WSMReasonDetails] where Id='" + ReasonData["Id"] + "'", out dsReasonDetails, false, "1");
                string _Id = "";

                #region data update
                if (dsReasonDetails.Tables[0].Rows.Count == 0)
                {
                    if (dsItemDetailsReasonNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Item Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("ReasonDetails", out _Id);
                        _Id = "WRD" + _Id;
                        ReasonData["Id"] = _Id;
                        AddNewRow(dsReasonDetails.Tables[0], ReasonData);
                    }
                }
                else
                {
                    _Id = ReasonData["Id"].ToString();
                    EditRow(dsReasonDetails.Tables[0].Rows[0], ReasonData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsReasonDetails);

                return Json(new { Error = false, Data = ReasonData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadControlPeriodDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id,ControlPeriodName,Format(FromDate,'dd-MMM-yyyy') as FromDate,Format(ToDate,'dd-MMM-yyyy') as ToDate,format(FromTime,'hh:mm tt') as FromTime,format(ToTime,'hh:mm tt') as ToTime,Minute,Remarks,Active from [HKP].[WSMControlPeriod] order by SequenceNo";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createControlPeriod(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[HKP].[WSMControlPeriod]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "CP" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {

                            DataRow drpb = dv[0].Row;
                            if (item["Active"] is true)
                            {
                                DateTime FromDt = Convert.ToDateTime(item["FromDate"]);
                                DateTime ToDt = Convert.ToDateTime(item["ToDate"]);
                                TimeSpan t = ToDt.Subtract(FromDt);
                                int N = t.Days;
                                TimeSpan ts;
                                DateTime date1 = Convert.ToDateTime(item["FromTime"]);
                                DateTime date2 = Convert.ToDateTime(item["ToTime"]);
                                DateTime NextDayDate = date2.AddDays(N);
                                if (FromDt == ToDt)
                                {
                                    ts = date2 - date1;
                                }
                                else
                                {
                                    DateTime NextDayDate2 = date2.AddDays(N);
                                    ts = NextDayDate2 - date1;
                                }
                                TimeSpan Nd = NextDayDate - date1;
                                int minutes = (int)Nd.TotalMinutes;

                                if (minutes >= 720 || minutes < 0)
                                {
                                    item["ToTime"] = NextDayDate;
                                    item["Minute"] = Nd.TotalMinutes;
                                }
                                else
                                {
                                    //item["ToTime"] = date2;
                                    item["Minute"] = ts.TotalMinutes;
                                }
                                item["FieldValue"] = 0;
                                EditRow(drpb, item);
                            }
                            else
                            {
                                DateTime FromDt = DateTime.Now;
                                DateTime ToDt = DateTime.Now; ;
                                TimeSpan t = ToDt.Subtract(FromDt);
                                int N = t.Days;
                                TimeSpan ts;
                                DateTime date1 = DateTime.Now;
                                DateTime date2 = DateTime.Now;
                                DateTime NextDayDate = date2.AddDays(N);
                                if (FromDt == ToDt)
                                {
                                    ts = date2 - date1;
                                }
                                else
                                {
                                    DateTime NextDayDate2 = date2.AddDays(N);
                                    ts = NextDayDate2 - date1;
                                }
                                TimeSpan Nd = NextDayDate - date1;
                                int minutes = (int)Nd.TotalMinutes;

                                if (minutes >= 720 || minutes < 0)
                                {
                                    item["ToTime"] = NextDayDate;
                                    item["Minute"] = Nd.TotalMinutes;

                                }
                                else
                                {
                                    //item["ToTime"] = date2;
                                    item["Minute"] = ts.TotalMinutes;

                                }
                                item["Remarks"] = "";
                                item["FieldValue"] = "NULL";
                                EditRow(drpb, item);
                            }
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpPost]
        public ActionResult LoadWSMColumnsDetails(string MasterId,string ProcessId)
        {
            return Json(ws.LoadWSMColumnsDetails(MasterId, ProcessId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=mb.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = ws.GetMaster(Id);
                return Json(new { master = _master}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            return Json(ws.GetList(strkey), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> datas )
        {
            try
            {
                var data = ws.Create(datas);
                return Json(new { Error = false, Data= data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult CreateColumns(List<Dictionary<string, object>> DataList)
        {
            try
            {
                var data = ws.createColumns(DataList);
                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                ws.Delete(id);

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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

    }
}