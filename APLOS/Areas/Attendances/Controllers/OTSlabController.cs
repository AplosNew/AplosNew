using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
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

namespace Aplos.Areas.Attendances.Controllers
{
    public class OTSlabController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;

        public OTSlabController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
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

        #region Working--------Day----
        
        [HttpPost]
        public ActionResult getWorkingDaylist(string PlantID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select DayType,format(FromDate,'dd-MMM-yyyy')as FromDate,format(ToDate,'dd-MMM-yyyy')as ToDate 
                            ,firstSlab,SystemID, PlantID, p.CompanyId
                            from [dbo].[OTSlabDefineGeneral]
                            left join ORG.Plant p on p.Id = OTSlabDefineGeneral.PlantID
                            where DayType='NW' and PlantID= '" + PlantID + "' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveWorkingDay(OTSlabDefineGeneralModel OTSlabDefineGeneral)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            saveworkingday(OTSlabDefineGeneral);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        public void saveworkingday(OTSlabDefineGeneralModel OTSlabDefineGeneral)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM [dbo].[OTSlabDefineGeneral] WHERE SystemID='" + OTSlabDefineGeneral.SystemID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[OTSlabDefineGeneral]", out sID);
                    dr["SystemID"] = "OTS" + sID;
                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["PlantID"] = OTSlabDefineGeneral.PlantID;
                    dr["DayType"] = OTSlabDefineGeneral.DayType;
                    dr["FromDate"] = OTSlabDefineGeneral.FromDate;
                    dr["ToDate"] = OTSlabDefineGeneral.ToDate;
                    dr["firstSlab"] = OTSlabDefineGeneral.firstSlab;                    

                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = DateTime.Now;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();                    
                    dr["DayType"] = OTSlabDefineGeneral.DayType;
                    dr["FromDate"] = OTSlabDefineGeneral.FromDate;
                    dr["ToDate"] = OTSlabDefineGeneral.ToDate;
                    dr["firstSlab"] = OTSlabDefineGeneral.firstSlab;                    
                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
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
        public ActionResult Deleteworkingday(string SystemID)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM [dbo].[OTSlabDefineGeneral] WHERE SystemID='" + SystemID + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        #endregion   Working--------Day----


        #region Week Off
        [HttpPost]
        public ActionResult getWeekOfflist(string PlantID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select DayType,format(FromDate,'dd-MMM-yyyy')as FromDate,format(ToDate,'dd-MMM-yyyy')as ToDate 
                            ,firstSlab,SystemID,IsMandatoryAlignWithSalary, IsTotalWorkTimeAsOT, IsTotalWorkTimeAsOTFromShift
                            from [dbo].[OTSlabDefineGeneral] where DayType='W' and PlantID= '"+ PlantID + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveWeekOffDay(OTSlabDefineGeneralModel OTSlabDefineGeneral)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            saveWeekOffday(OTSlabDefineGeneral);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        public void saveWeekOffday(OTSlabDefineGeneralModel OTSlabDefineGeneral)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM [dbo].[OTSlabDefineGeneral] WHERE SystemID='" + OTSlabDefineGeneral.SystemID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[OTSlabDefineGeneral]", out sID);
                    dr["SystemID"] = "OTS" + sID;
                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["PlantID"] = OTSlabDefineGeneral.PlantID;
                    dr["DayType"] = OTSlabDefineGeneral.DayType;
                    dr["FromDate"] = OTSlabDefineGeneral.FromDate;
                    dr["ToDate"] = OTSlabDefineGeneral.ToDate;
                    dr["firstSlab"] = OTSlabDefineGeneral.firstSlab;
                    //dr["OTStartFrom"] = OTSlabDefineGeneral.OTStartFrom;
                    dr["IsTotalWorkTimeAsOTFromShift"] = OTSlabDefineGeneral.IsTotalWorkTimeAsOTFromShift;
                    dr["IsTotalWorkTimeAsOT"] = OTSlabDefineGeneral.IsTotalWorkTimeAsOT;

                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = DateTime.Now;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["DayType"] = OTSlabDefineGeneral.DayType;
                    dr["FromDate"] = OTSlabDefineGeneral.FromDate;
                    dr["ToDate"] = OTSlabDefineGeneral.ToDate;
                    dr["firstSlab"] = OTSlabDefineGeneral.firstSlab;
                    //dr["OTStartFrom"] = OTSlabDefineGeneral.OTStartFrom;

                    dr["IsTotalWorkTimeAsOTFromShift"] = OTSlabDefineGeneral.IsTotalWorkTimeAsOTFromShift;
                    dr["IsTotalWorkTimeAsOT"] = OTSlabDefineGeneral.IsTotalWorkTimeAsOT;

                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
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
        public ActionResult DeleteWeekOffday(string SystemID)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM [dbo].[OTSlabDefineGeneral] WHERE SystemID='" + SystemID + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        #endregion   Week Off


        #region Holiday 
        [HttpPost]
        public ActionResult getHolidaylist(string PlantID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select DayType,format(FromDate,'dd-MMM-yyyy')as FromDate,format(ToDate,'dd-MMM-yyyy')as ToDate 
                            ,firstSlab,SystemID,IsMandatoryAlignWithSalary, IsTotalWorkTimeAsOT, IsTotalWorkTimeAsOTFromShift
                            from [dbo].[OTSlabDefineGeneral] where  DayType='H' and PlantID ='"+ PlantID + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveHolidayDay(OTSlabDefineGeneralModel OTSlabDefineGeneral)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            saveHolidayday(OTSlabDefineGeneral);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        public void saveHolidayday(OTSlabDefineGeneralModel OTSlabDefineGeneral)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM [dbo].[OTSlabDefineGeneral] WHERE SystemID='" + OTSlabDefineGeneral.SystemID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[OTSlabDefineGeneral]", out sID);
                    dr["SystemID"] = "OTS" + sID;
                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["PlantID"] = OTSlabDefineGeneral.PlantID;
                    dr["DayType"] = OTSlabDefineGeneral.DayType;
                    dr["FromDate"] = OTSlabDefineGeneral.FromDate;
                    dr["ToDate"] = OTSlabDefineGeneral.ToDate;
                    dr["firstSlab"] = OTSlabDefineGeneral.firstSlab;
                    //dr["OTStartFrom"] = OTSlabDefineGeneral.OTStartFrom;

                    dr["IsTotalWorkTimeAsOTFromShift"] = OTSlabDefineGeneral.IsTotalWorkTimeAsOTFromShift;
                    dr["IsTotalWorkTimeAsOT"] = OTSlabDefineGeneral.IsTotalWorkTimeAsOT;
                    

                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = DateTime.Now;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["DayType"] = OTSlabDefineGeneral.DayType;
                    dr["FromDate"] = OTSlabDefineGeneral.FromDate;
                    dr["ToDate"] = OTSlabDefineGeneral.ToDate;
                    dr["firstSlab"] = OTSlabDefineGeneral.firstSlab;
                    //dr["OTStartFrom"] = OTSlabDefineGeneral.OTStartFrom;

                    dr["IsTotalWorkTimeAsOTFromShift"] = OTSlabDefineGeneral.IsTotalWorkTimeAsOTFromShift;
                    dr["IsTotalWorkTimeAsOT"] = OTSlabDefineGeneral.IsTotalWorkTimeAsOT;


                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
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
        public ActionResult DeleteHolidayday(string SystemID)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM [dbo].[OTSlabDefineGeneral] WHERE SystemID='" + SystemID + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public class OTSlabDefineGeneralModel : BaseModel
        {
            #region Scalar Properties            
            public string SystemID { get; set; }
            public string GroupID { get; set; }
            public string PlantID { get; set; }
            public string DayType { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public decimal firstSlab { get; set; }
            
            //public string OTStartFrom { get; set; }
            public bool IsTotalWorkTimeAsOT { get; set; }
            public bool IsTotalWorkTimeAsOTFromShift { get; set; }
            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? DateAdded { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime? DateUpdated { get; set; }
            #endregion Audit Properties
        }

        #endregion -- Operations  
    }
}