#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using System;
using System.Data;
using OTSBD;
using clsAttendance;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class ShiftTimeChangeController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        public ShiftTimeChangeController(
              IStoppageService stoppageService,
              ISqlRepository sqlRepository
            )
        {
            _stoppageService = stoppageService;
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

        [HttpGet, Authorize]
        public ActionResult getShift()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"  SELECT SystemID as ShiftSystemID, ShiftDefinationName, ShiftDefinationDescription, ShiftType, SequenceNo
                            ,FORMAT(InTime,'hh:mm tt') AS InTime
                            ,FORMAT(OutTime,'hh:mm tt') AS OutTime
                            ,FORMAT( BreakStratTime, 'hh:mm tt') AS BreakStratTime
                            ,FORMAT(BreakEndTime, 'hh:mm tt') AS BreakEndTime
                            ,InTimeStartMargin, LateMargin, AbsentEndMargin, LateInToleranceMargin,
                            OutTimeEndMargin, OTStartTime, LateMarginSeconds,
                            BreakPeriod, WorkingHour, IsActive, DefaultShift, IsGapInclude
                           ,IsActives=case when IsActive=1 then 'True' else 'False' end
                           ,DefaultShifts=case when DefaultShift=1 then 'True' else 'False' End
                           ,IsGapIncludes=case when IsGapInclude=1 then 'True' else 'False' End 
                            ,EarlyIn,LateIn,LateInMargin,EarlyOut,EarlyOutMargin,EarlyInMargin,LateOutMargin,LateOut,LateOutRoundMargin,LateInRoundMargin
                            ,EarlyOutRoundMargin,EarlyInRoundMargin,LateOutRoundMarginType,LateInRoundMarginType,EarlyOutRoundMarginType,EarlyInRoundMarginType
                            ,IncludeBreakTimeInOT,HalfDayAbsentMaxLimit,LateInMargin,EarlyOutMaxLimit,IsLunchOutApplicable,IsEarlyOutApplicable,EarlyOutToleranceMargin
                            ,LateInMaxLimit ,IsLateInApplicable ,RawINDefinitionFrom ,RawINDefinitionTo ,RawOUTDefinitionFrom ,RawOUTDefinitionTo 
                            ,RawINDefinitionFrom,RawOUTDefinitionFrom,RawINDefinitionTo,RawOUTDefinitionTo
                   FROM ShiftDefination WHERE GroupID = '" + identity.CompanyGroupId + "' AND PlantID = '" + identity.PlantId + "' and IsActive='1' Order By ShiftDefinationName";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT * FROM (SELECT STC.SystemID, S.ShiftDefinationName, S.ShiftType, S.ShiftDefinationDescription
                                    , REPLACE(CONVERT(VARCHAR(11), STC.FromDate, 106),' ','-') AS FromDate,
	                                REPLACE(CONVERT(VARCHAR(11), STC.ToDate, 106),' ','-') AS ToDate
                                    ,format(STC.InTime, 'hh:mm tt') AS InTime,
	                                STC.InTimeStartMargin, STC.LateMargin, STC.AbsentEndMargin,STC.LateMarginSeconds,
	                                format(STC.OutTime, 'hh:mm tt') AS OutTime
                                    , STC.OutTimeEndMargin, STC.OTStartTime
                                    ,format(STC.BreakStratTime,'hh:mm tt') AS BreakStratTime,
                                    format(STC.BreakEndTime, 'hh:mm tt') AS BreakEndTime
                                    , STC.BreakPeriod, STC.WorkingHour, STC.GroupID, STC.PlantID, STC.ShiftDefinationID,s.SystemId as ShiftSystemID
                                    ,stc.HalfDayAbsentMaxLimit,stc.IncludeBreakTimeInOT,stc.IsGapInclude
                                    ,stc.IsLateInApplicable,stc.IsEarlyOutApplicable,stc.LateInMaxLimit
                                    ,stc.EarlyOutMaxLimit,stc.EarlyOutToleranceMargin,stc.LateInToleranceMargin,stc.Remarks,stc.IsLunchOutApplicable
                                    ,STC.RawINDefinitionFrom,STC.RawOUTDefinitionFrom,STC.RawINDefinitionTo,STC.RawOUTDefinitionTo--,STC.INAfterOUTAsOTStart
                                     FROM ShiftTimeChgMaster STC
	                                LEFT JOIN ShiftDefination S ON STC.ShiftDefinationID = S.SystemID) A
                                     WHERE GroupID = '" + identity.CompanyGroupId + "' AND PlantID = '" + identity.PlantId + @"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(ShiftTimeChgMaster shifttimemaster, ShiftTimeChgChild shifttimechild)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (shifttimemaster.FromDate == null)
                {
                    Exception ex = new Exception("Please Select From Date....");
                    throw (ex);
                }
                if (shifttimemaster.ToDate == null)
                {
                    Exception ex = new Exception("Please Select To Date....");
                    throw (ex);
                }
                
                DateTime fromdate = Convert.ToDateTime(shifttimemaster.FromDate);
                DateTime todate = Convert.ToDateTime(shifttimemaster.ToDate);
                
                DateTime dtFrom = bplib.clsWebLib.DateData_DBToApp(shifttimemaster.FromDate, bplib.clsWebLib.DB_DATE_FORMAT);
                DateTime dtTo = bplib.clsWebLib.DateData_DBToApp(shifttimemaster.ToDate, bplib.clsWebLib.DB_DATE_FORMAT);
                TimeSpan ts;
                ts = dtTo.Subtract(dtFrom.Date);
                int days = (ts.Days + 1);

                if (days < 1)
                {
                    Exception ex = new Exception("[FromDate] cannot greater than [ToDate] ......");
                    throw (ex);
                }


                DateTime dtBkSt = Convert.ToDateTime(shifttimemaster.BreakStratTime);
                DateTime dtBkEd = Convert.ToDateTime(shifttimemaster.BreakEndTime);

                DateTime dtInT = Convert.ToDateTime(shifttimemaster.InTime);
                DateTime dOutT = Convert.ToDateTime(shifttimemaster.OutTime);

                int minBk = 0;
                if ((shifttimemaster.BreakEndTime.ToString() != "00:00:00") & shifttimemaster.BreakStratTime.ToString() != "00:00:00")
                {
                    if (shifttimemaster.ShiftType == "Night Shift" & dtBkEd < dtBkSt)
                    {
                        dtBkEd = dtBkEd.AddDays(1);
                        shifttimemaster.BreakEndTime = dtBkEd;
                    }
                    TimeSpan tsBk = dtBkEd - dtBkSt;
                    minBk = Convert.ToInt32(tsBk.TotalMinutes);
                    shifttimemaster.BreakPeriod = minBk;
                }
                if (minBk > 0)
                {
                    if (string.IsNullOrEmpty(shifttimemaster.BreakPeriod.ToString()) == false & bplib.clsWebLib.IsNumeric(shifttimemaster.BreakPeriod.ToString()) == false)
                    {

                        Exception ex = new Exception("Invalid / Blank Data not allowed for Break Period. \n Please Enter Numeric data Only");
                        throw (ex);
                    }

                    if (shifttimemaster.ShiftType == "Day Shift")
                    {
                        if (dtBkSt <= dtInT)
                        {

                            Exception ex = new Exception("Define Break Start time cannot be less than or equal IN Time...");
                            throw (ex);
                        }
                        if (dtBkEd <= dtInT)
                        {

                            Exception ex = new Exception("Define Break End time cannot be less than or equal IN Time...");
                            throw (ex);
                        }
                        if (dtBkSt >= dtBkEd)
                        {

                            Exception ex = new Exception("Define Break Start time cannot be more than or equal Break End time...");
                            throw (ex);
                        }
                        if (dtBkEd >= dOutT)
                        {

                            Exception ex = new Exception("Define Break End time cannot be more than or equal OUT Time...");
                            throw (ex);
                        }
                    }
                }
                if (shifttimemaster.ShiftType == "Day Shift")
                {
                    if (dtInT >= dOutT)
                    {
                        Exception ex = new Exception("Define IN Time cannot be more than or equal OUT Time...");
                        throw (ex);
                    }
                }
                if (shifttimemaster.ShiftType == "Night Shift" && dtInT < dOutT)
                {
                    Exception ex = new Exception("For Night Shift IN Time can't be less than or equal OUT Time...");
                    throw (ex);
                }
                if (shifttimemaster.ShiftType == "Night Shift")
                {
                    //date same ok
                    dOutT = dOutT.AddDays(1);
                    shifttimemaster.OutTime = dOutT;
                }

                int minWrkTm = 0;
                TimeSpan tsWrk = dOutT - dtInT;
                minWrkTm = Math.Abs(Convert.ToInt32(tsWrk.TotalMinutes) - minBk);
                shifttimemaster.WorkingHour = minWrkTm;

                if (bplib.clsWebLib.IsNumeric(shifttimemaster.WorkingHour.ToString()) == true)
                {
                    if (Convert.ToDecimal(shifttimemaster.WorkingHour.ToString()) > Convert.ToDecimal("1440"))
                    {
                        Exception ex = new Exception("Working minutes is not allow more then 1440");
                        throw (ex);
                    }
                }

                string MasterId = string.Empty;

                MasterId = SaveShiftTimeChangeMaster(shifttimemaster);

                SaveShiftTimeChangeChild(shifttimemaster, shifttimechild, MasterId, out DataSet dsDelete);

                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public string SaveShiftTimeChangeMaster(ShiftTimeChgMaster shifttimemaster)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string Id = string.Empty;

                string sql = "SELECT * FROM ShiftTimeChgMaster WHERE SystemID='" + shifttimemaster.SystemID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {                    
                    DataSet dsvalidation;
                    string sql3 = "select m.SystemID,m.ShiftDefinationID from ShiftTimeChgChild C LEFT JOIN ShiftTimeChgMaster M ON M.SystemID = C.STCMasterSystemID where C.ShiftDate between '" + shifttimemaster.FromDate + @"' and '" + shifttimemaster.ToDate + @"' AND M.ShiftDefinationID = '" + shifttimemaster.ShiftDefinationID + @"' AND M.PlantID = '" + identity.PlantId + @"'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql3, out dsvalidation, false, "1");
                    if (dsvalidation.Tables[0].Rows.Count > 0)
                    {
                        Exception ex = new Exception("This Shift Time Already Changed....");
                        throw (ex);
                    }

                    DataRow dr = dsMaster.Tables[0].NewRow();

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[ShiftTimeChgChild]", out sID);
                    Id = "STM" + sID;
                    dr["SystemID"] = Id;
                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["PlantID"] = identity.PlantId;
                    dr["ShiftDefinationID"] = shifttimemaster.ShiftDefinationID;
                    dr["FromDate"] = shifttimemaster.FromDate;
                    dr["ToDate"] = shifttimemaster.ToDate;
                    dr["InTime"] = shifttimemaster.InTime;
                    dr["InTimeStartMargin"] = shifttimemaster.InTimeStartMargin;
                    dr["LateMargin"] = shifttimemaster.LateMargin;
                    dr["AbsentEndMargin"] = shifttimemaster.AbsentEndMargin;
                    dr["OutTime"] = shifttimemaster.OutTime;
                    dr["OutTimeEndMargin"] = shifttimemaster.OutTimeEndMargin;
                    dr["OTStartTime"] = shifttimemaster.OTStartTime;
                    dr["BreakStratTime"] = shifttimemaster.BreakStratTime;
                    dr["BreakEndTime"] = shifttimemaster.BreakEndTime;
                    dr["BreakPeriod"] = shifttimemaster.BreakPeriod;
                    dr["WorkingHour"] = shifttimemaster.WorkingHour;
                    dr["Remarks"] = shifttimemaster.Remarks;
                    dr["IsLunchOutApplicable"] = shifttimemaster.IsLunchOutApplicable;
                    dr["LateMarginSeconds"] = shifttimemaster.LateMarginSeconds;
                    dr["HalfDayAbsentMaxLimit"] = shifttimemaster.HalfDayAbsentMaxLimit;
                    dr["IncludeBreakTimeInOT"] = shifttimemaster.IncludeBreakTimeInOT;
                    dr["IsGapInclude"] = shifttimemaster.IsGapInclude;
                    dr["IsLateInApplicable"] = shifttimemaster.IsLateInApplicable;
                    dr["IsEarlyOutApplicable"] = shifttimemaster.IsEarlyOutApplicable;
                    dr["LateInMaxLimit"] = shifttimemaster.LateInMaxLimit;
                    dr["EarlyOutMaxLimit"] = shifttimemaster.EarlyOutMaxLimit;
                    dr["EarlyOutToleranceMargin"] = shifttimemaster.EarlyOutToleranceMargin;
                    dr["LateInToleranceMargin"] = shifttimemaster.LateInToleranceMargin;
                    dr["RawINDefinitionFrom"] = shifttimemaster.RawINDefinitionFrom;
                    dr["RawOUTDefinitionFrom"] = shifttimemaster.RawOUTDefinitionFrom;
                    dr["RawINDefinitionTo"] = shifttimemaster.RawINDefinitionTo;
                    dr["RawOUTDefinitionTo"] = shifttimemaster.RawOUTDefinitionTo;
                    //dr["INAfterOUTAsOTStart"] = shifttimemaster.INAfterOUTAsOTStart;

                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = DateTime.Now;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    Id = dr["SystemID"].ToString();

                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["PlantID"] = identity.PlantId;
                    dr["ShiftDefinationID"] = shifttimemaster.ShiftDefinationID;
                    dr["FromDate"] = shifttimemaster.FromDate;
                    dr["ToDate"] = shifttimemaster.ToDate;
                    dr["InTime"] = shifttimemaster.InTime;
                    dr["InTimeStartMargin"] = shifttimemaster.InTimeStartMargin;
                    dr["LateMargin"] = shifttimemaster.LateMargin;
                    dr["AbsentEndMargin"] = shifttimemaster.AbsentEndMargin;
                    dr["OutTime"] = shifttimemaster.OutTime;
                    dr["OutTimeEndMargin"] = shifttimemaster.OutTimeEndMargin;
                    dr["OTStartTime"] = shifttimemaster.OTStartTime;
                    dr["BreakStratTime"] = shifttimemaster.BreakStratTime;
                    dr["BreakEndTime"] = shifttimemaster.BreakEndTime;
                    dr["BreakPeriod"] = shifttimemaster.BreakPeriod;
                    dr["WorkingHour"] = shifttimemaster.WorkingHour;
                    dr["Remarks"] = shifttimemaster.Remarks;
                    dr["IsLunchOutApplicable"] = shifttimemaster.IsLunchOutApplicable;
                    dr["HalfDayAbsentMaxLimit"] = shifttimemaster.HalfDayAbsentMaxLimit;
                    dr["IncludeBreakTimeInOT"] = shifttimemaster.IncludeBreakTimeInOT;
                    dr["IsGapInclude"] = shifttimemaster.IsGapInclude;
                    dr["IsLateInApplicable"] = shifttimemaster.IsLateInApplicable;
                    dr["IsEarlyOutApplicable"] = shifttimemaster.IsEarlyOutApplicable;
                    dr["LateInMaxLimit"] = shifttimemaster.LateInMaxLimit;
                    dr["EarlyOutMaxLimit"] = shifttimemaster.EarlyOutMaxLimit;
                    dr["EarlyOutToleranceMargin"] = shifttimemaster.EarlyOutToleranceMargin;
                    dr["LateInToleranceMargin"] = shifttimemaster.LateInToleranceMargin;
                    dr["LateMarginSeconds"] = shifttimemaster.LateMarginSeconds;
                    dr["RawINDefinitionFrom"] = shifttimemaster.RawINDefinitionFrom;
                    dr["RawOUTDefinitionFrom"] = shifttimemaster.RawOUTDefinitionFrom;
                    dr["RawINDefinitionTo"] = shifttimemaster.RawINDefinitionTo;
                    dr["RawOUTDefinitionTo"] = shifttimemaster.RawOUTDefinitionTo;
                    //dr["INAfterOUTAsOTStart"] = shifttimemaster.INAfterOUTAsOTStart;

                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();

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

        public void SaveShiftTimeChangeChild(ShiftTimeChgMaster shifttimemaster, ShiftTimeChgChild shifttimechild, string MasterId, out DataSet dsDelete)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            dsDelete = null;
            try
            {
                DeleteChild(MasterId, out dsDelete);

                DateTime dtFrom = bplib.clsWebLib.DateData_DBToApp(shifttimemaster.FromDate, bplib.clsWebLib.DB_DATE_FORMAT);
                DateTime dtTo = bplib.clsWebLib.DateData_DBToApp(shifttimemaster.ToDate, bplib.clsWebLib.DB_DATE_FORMAT);
                string sql = "SELECT * FROM [dbo].[ShiftTimeChgChild] WHERE SystemID='" + shifttimechild.SystemID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                while (dtFrom <= dtTo)
                {
                    string DayName = (dtFrom.DayOfWeek).ToString();
                    shifttimemaster.FromDate = dtFrom;

                    DataView dvMaster = new DataView(dsMaster.Tables[0]);
                    dvMaster.RowFilter = "ShiftDate = '" + shifttimemaster.FromDate + "' AND IsLock = 'Yes'";

                    if (dvMaster.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[ShiftTimeChgChild]", out sID);
                        dr["SystemID"] = "STC" + sID;
                        dr["GroupID"] = identity.CompanyGroupId;
                        dr["PlantID"] = identity.PlantId;
                        dr["STCMasterSystemID"] = MasterId;
                        dr["ShiftDate"] = bplib.clsWebLib.DateData_AppToDB(shifttimemaster.FromDate, bplib.clsWebLib.DB_DATE_FORMAT);
                        dr["DayName"] =  bplib.clsWebLib.RetValidLen(DayName.Trim(), 12);
                        dr["IsLock"] = "No";

                        dr["AddedBy"] = identity.Name;
                        dr["DateAdded"] = DateTime.Now;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["GroupID"] = identity.CompanyGroupId;
                        dr["PlantID"] = identity.PlantId;
                        dr["ShiftDefinationID"] = MasterId;
                        dr["ShiftDate"] = bplib.clsWebLib.DateData_AppToDB(shifttimemaster.FromDate, bplib.clsWebLib.DB_DATE_FORMAT);
                        dr["DayName"] = bplib.clsWebLib.RetValidLen(DayName.Trim(), 12);
                        dr["IsLock"] = "No";

                        dr["UpdatedBy"] = identity.Name;
                        dr["DateUpdated"] = System.DateTime.Now.ToString();

                        dr.EndEdit();
                    }
                    dtFrom = dtFrom.AddDays(1);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult Delete(string SystemID)
        {
            string strChildSQL;
            string strMasterSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strChildSQL = "DELETE FROM  ShiftTimeChgChild WHERE STCMasterSystemID='" + SystemID + "'";
                strMasterSQL = "DELETE FROM  ShiftTimeChgMaster WHERE SystemID='" + SystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strChildSQL, out dsExceptionEmployeeList, false, "1");
                objCon.OpenDataSetThroughAdapter(strMasterSQL, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        void DeleteChild(string MasterId, out System.Data.DataSet dsRef)
        {
            string strSQLR;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQLR = @"DELETE FROM  ShiftTimeChgChild WHERE STCMasterSystemID='" + MasterId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
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
        public class ShiftTimeChgMaster : BaseModel
        {
            #region Scalar Properties            
            public string SystemID { get; set; }
            public string GroupID { get; set; }
            public string PlantID { get; set; }
            public string ShiftDefinationID { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public DateTime? InTime { get; set; }
            public string InTimeStartMargin { get; set; }
            public int LateMargin { get; set; }
            public int AbsentEndMargin { get; set; }
            public int LateMarginSeconds { get; set; }
            public DateTime? OutTime { get; set; }
            public int OutTimeEndMargin { get; set; }
            public int OTStartTime { get; set; }
            public DateTime? BreakStratTime { get; set; }
            public DateTime? BreakEndTime { get; set; }
            public int BreakPeriod { get; set; }
            public float WorkingHour { get; set; }
            public string Remarks { get; set; }
            public string ShiftType { get; set; }
            public string ShiftDefinationName { get; set; }
            public decimal HalfDayAbsentMaxLimit { get; set; }
            public bool IncludeBreakTimeInOT { get; set; }
            public bool IsGapInclude { get; set; }
            public bool IsLateInApplicable { get; set; }
            public bool IsEarlyOutApplicable { get; set; }
            public bool IsLunchOutApplicable { get; set; }
            public int LateInMaxLimit { get; set; }
            public int EarlyOutMaxLimit { get; set; }
            public int EarlyOutToleranceMargin { get; set; }
            public int LateInToleranceMargin { get; set; }

            public int RawINDefinitionFrom { get; set; }
            public int RawOUTDefinitionFrom { get; set; }
            public int RawINDefinitionTo { get; set; }
            public int RawOUTDefinitionTo { get; set; }
            public bool INAfterOUTAsOTStart { get; set; }
            
            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            #endregion Audit Properties
        }

        public class ShiftTimeChgChild : BaseModel
        {
            #region Scalar Properties            
            public string SystemID { get; set; }
            public string GroupID { get; set; }
            public string PlantID { get; set; }
            public string STCMasterSystemID { get; set; }
            public DateTime? ShiftDate { get; set; }
            public string DayName { get; set; }
            public string IsLock { get; set; }

            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            #endregion Audit Properties
        }
        #endregion
    }
}