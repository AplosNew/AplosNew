using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Leave.Controllers
{
    public class OnDutyTransactionController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;
        private DataSet dsRef;

        public OnDutyTransactionController(
              IMaternityLeavePolicyService LeavePolicyService,
               IAttendanceManagementService AttendanceManagementService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _AttendanceManagementService = AttendanceManagementService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpPost]
        public ActionResult Save(EmployeeOnDuty EmployeeOnDuty, EmployeeOnDutyDetails EmployeeOnDutyDetails)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager objCon;
                string sql3 = "SELECT * FROM [dbo].[EmployeeOnDuty] WHERE Id='" + EmployeeOnDuty.Id + @"' and IsApproved = 1 ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql3, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("Already Approved....");
                    throw (ex);
                }

                DataSet dsEntitle;
                string sql1 = "select EmpSystemId,Workdate,IsOTEntitled From AttdnProcessData where IsOTEntitled=1 and IsManualDayStatus=1 and WorkDate between '" + EmployeeOnDuty.FromDate + @"' and '" + EmployeeOnDuty.ToDate + @"' and EmpSystemID='" + EmployeeOnDuty.EmpSystemId+@"'  ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsEntitle, false, "1");
                if (dsEntitle.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("This Employee has Manual Day Status ..");
                    throw (ex);
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                AttendanceProcessAplos ob = new AttendanceProcessAplos();
                DateTime FromDateV = Convert.ToDateTime(EmployeeOnDuty.FromDate);
                DateTime ToDateV = Convert.ToDateTime(EmployeeOnDuty.ToDate);
                while (FromDateV <= ToDateV)
                {
                    if (EmployeeOnDuty.EmpSystemId.Length > 0)
                    {
                    ob.LockValidation(identity.PlantId, FromDateV.ToString("dd-MMM-yyyy"), ToDateV.ToString("dd-MMM-yyyy"), EmployeeOnDuty.EmpSystemId);
                    }
                    FromDateV = FromDateV.AddDays(1);
                }

                string MasterId = string.Empty;
                MasterId = SaveEmployeeOnDutyMaster(EmployeeOnDuty);
                SaveShiftTimeChangeChild(EmployeeOnDuty, EmployeeOnDutyDetails, MasterId, out DataSet dsDelete);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public string SaveEmployeeOnDutyMaster(EmployeeOnDuty EmployeeOnDuty)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string Id = string.Empty;
                string sql = "SELECT * FROM [dbo].[EmployeeOnDuty] WHERE ID='" + EmployeeOnDuty.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[EmployeeOnDuty]", out sID);
                    Id = "ODM" + sID;
                    dr["Id"] = Id;
                    dr["EmpSystemId"] = EmployeeOnDuty.EmpSystemId;
                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["PlantID"] = identity.PlantId;
                    dr["FromDate"] = EmployeeOnDuty.FromDate;
                    dr["ToDate"] = EmployeeOnDuty.ToDate;
                    dr["IsApproved"] = false;
                    dr["Reason"] = EmployeeOnDuty.Reason;

                    dr["ApprovedBy"] = identity.Name;
                    dr["ApprovedDate"] = System.DateTime.Now.ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    Id = dr["Id"].ToString();

                    dr["EmpSystemId"] = EmployeeOnDuty.EmpSystemId;
                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["PlantID"] = identity.PlantId;
                    dr["FromDate"] = EmployeeOnDuty.FromDate;
                    dr["ToDate"] = EmployeeOnDuty.ToDate;
                    dr["IsApproved"] = false;
                    dr["Reason"] = EmployeeOnDuty.Reason;

                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedBy"] = identity.Name; 
                     dr["ApprovedBy"] = identity.Name;
                    dr["ApprovedDate"] = System.DateTime.Now.ToString();


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

        public void SaveShiftTimeChangeChild(EmployeeOnDuty EmployeeOnDuty, EmployeeOnDutyDetails EmployeeOnDutyDetails, string MasterId, out DataSet dsDelete)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            dsDelete = null;
            try
            {
                DeleteChild(MasterId, out dsDelete);

                DateTime dtFrom = bplib.clsWebLib.DateData_DBToApp(EmployeeOnDuty.FromDate, bplib.clsWebLib.DB_DATE_FORMAT);
                DateTime dtTo = bplib.clsWebLib.DateData_DBToApp(EmployeeOnDuty.ToDate, bplib.clsWebLib.DB_DATE_FORMAT);
                string sql = "SELECT * FROM [dbo].[EmployeeOnDutyDetails] WHERE Id='" + EmployeeOnDutyDetails.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                string Id = string.Empty;

                while (dtFrom <= dtTo)
                {
                    string DayName = (dtFrom.DayOfWeek).ToString();
                    EmployeeOnDuty.FromDate = dtFrom;

                    DataView dvMaster = new DataView(dsMaster.Tables[0]);
                    dvMaster.RowFilter = "Workdate = '" + EmployeeOnDuty.FromDate + "' AND IsAvailed =1 ";

                    if (dvMaster.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[ShiftTimeChgChild]", out sID);
                        Id = "ODD" + sID;
                        dr["Id"] = Id;
                        dr["OnDutyId"] = MasterId;
                        dr["Workdate"] = bplib.clsWebLib.DateData_AppToDB(EmployeeOnDuty.FromDate, bplib.clsWebLib.DB_DATE_FORMAT);
                        dr["IsAvailed"] = true;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["OnDutyId"] = MasterId;
                        dr["Workdate"] = bplib.clsWebLib.DateData_AppToDB(EmployeeOnDuty.FromDate, bplib.clsWebLib.DB_DATE_FORMAT);
                        dr["IsAvailed"] = true;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();

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

        void DeleteChild(string MasterId, out System.Data.DataSet dsRef)
        {
            string strSQLR;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQLR = @"DELETE FROM  [dbo].[EmployeeOnDutyDetails] WHERE OnDutyId='" + MasterId + "'";
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

        [HttpGet]
        public ActionResult GetOffDuty(string empId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"	 select Id,EmpSystemId,Format(FromDate,'dd-MMM-yyyy')FromDate
                                    ,FromDate as Orginal
                                    ,Format(ToDate,'dd-MMM-yyyy')ToDate,
                             IsApproved=case when IsApproved=1 then 'Yes' else 'No' end,Reason
                             from EmployeeOnDuty where EmpSystemId='" + empId + @"'
                             order by Orginal desc ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        

        [HttpGet]
        public ActionResult Delete(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            DataSet dsMaster;
            try
            {
                string sql3 = "SELECT * FROM [dbo].[EmployeeOnDuty] WHERE Id='" + Id + @"' and IsApproved = 1 ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql3, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("Already Approved....");
                    throw (ex);
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM [dbo].[EmployeeOnDutyDetails] WHERE OnDutyId='" + Id + @"'";
                string sql1 = @"Delete FROM [dbo].[EmployeeOnDuty] WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations  

        public class EmployeeOnDuty : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string EmpSystemId { get; set; }
            public string GroupId { get; set; }
            public string PlantId { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public bool IsApproved { get; set; }
            public string Reason { get; set; }
          
            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }

            public string ApprovedBy { get; set; }
            public DateTime? ApprovedDate { get; set; }

            #endregion Audit Properties
        }

        public class EmployeeOnDutyDetails : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string OnDutyId { get; set; }
            public DateTime? Workdate { get; set; }
            public string AddedBy { get; set; }
            public DateTime? AddedDate { get; set; }
            public bool IsAvailed { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }

            #endregion Scalar Properties

         
        }
    }
}