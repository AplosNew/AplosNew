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
using System.Collections.Generic;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class AdditionalPayDayController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        public AdditionalPayDayController(
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
        public JsonResult GetCbo(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_sqlRepository.GetDataCollection("SELECT Id AS Value, PolicyName AS Text FROM [dbo].[HolidayPayDayMaster] WHERE IsActive=1 AND PlantId='"+ plantId + "' AND CompanyGroupId='"+ identity .CompanyGroupId+ "'"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadListeList(string masterid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select s.SalaryHeadID as SalaryHeadId,s.SalaryHead as UserName ,s.HeadType
                            ,CheckBoxSelect=case when  d.SalaryHeadID is null then  CONVERT(bit,0) else  CONVERT(bit,1) end 
                            from [dbo].[SalaryHead] s
							left join HolidayPayDayDetails d on d.SalaryHeadId=s.SalaryHeadID and  d.HolidayPayDayMasterId='" + masterid+@"'
                            ORDER BY HeadType DESC,SalaryHead";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"	select Id,PolicyName,PolicyDescription,IsActive,PlantId,CompanyGroupId 
                                from HolidayPayDayMaster as M 
                            WHERE M.PlantID='" + PlantId+ "' AND M.CompanyGroupId='" + identity.CompanyGroupId+"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailsList( string MasterId ,string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select D.SalaryHeadId,D.Id,D.HolidayPayDayMasterId 
                            ,SH.SalaryHead as UserName,sh.HeadType                          
                            From HolidayPayDayMaster as M
                            LEFT JOIN HolidayPayDayDetails D ON M.Id = D.HolidayPayDayMasterId
                            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=d.SalaryHeadId
                                WHERE M.PlantId='" + PlantId+@"' AND M.CompanyGroupId='"+identity.CompanyGroupId+ @"'
                                        and d.HolidayPayDayMasterId='" + MasterId+@"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        

        [HttpPost]
        public ActionResult SaveM(HolidayPayDayMaster Master)
        {
            try
            {
                string MasterId = string.Empty;               
                MasterId = SaveMaster(Master);              
                return Json(new { MasterId,Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost, Authorize]
        public string SaveMaster(HolidayPayDayMaster Master)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string Id = string.Empty;

                string sql = "SELECT * FROM HolidayPayDayMaster WHERE ID='" + Master.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HolidayPayDayMaster", out sID);
                    Id = "APDM" + sID;
                    dr["Id"] = Id;
                    dr["PolicyName"] = Master.PolicyName;
                    dr["PolicyDescription"] = Master.PolicyDescription;
                    dr["IsActive"] = Master.IsActive;
                    dr["PlantId"] = Master.PlantId;
                    dr["CompanyGroupId"] = identity.CompanyGroupId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    Id = dr["ID"].ToString();

                    dr["PolicyName"] = Master.PolicyName;
                    dr["PolicyDescription"] = Master.PolicyDescription;
                    dr["IsActive"] = Master.IsActive;
                    dr["PlantId"] = Master.PlantId;
                    dr["CompanyGroupId"] = identity.CompanyGroupId;

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

        [HttpPost, Authorize]
        public ActionResult SaveD(List<HolidayPayDayDetails> Details, string MasterId)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                string strMasterSQL;
                DataSet dsExceptionEmployeeList;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strMasterSQL = "DELETE FROM HolidayPayDayDetails WHERE HolidayPayDayMasterId='" + MasterId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strMasterSQL, out dsExceptionEmployeeList, false, "1");
                SaveDetails(Details, MasterId);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost, Authorize]
        public void SaveDetails(List<HolidayPayDayDetails> Details, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                foreach (var item in Details)
                {                   
                    string sql = "SELECT * FROM HolidayPayDayDetails WHERE ID='" + item.Id + "' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HolidayPayDayDetails", out sID);
                        dr["Id"] = "APDD" + sID;
                        dr["SalaryHeadId"] = item.SalaryHeadId;
                        dr["HolidayPayDayMasterId"] = MasterId;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();                      
                        dr["SalaryHeadId"] = item.SalaryHeadId;
                        dr["HolidayPayDayMasterId"] = MasterId;
                        dr.EndEdit();
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);                   
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       
        [HttpGet]
        public ActionResult DeleteM(string MID)
        {
            DataSet dsMaster;
            string strMasterSQL;
            string strDetailsSQL1;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;

            try
            {
                strDetailsSQL1 = "select *  FROM  HolidayPayDayDetails WHERE HolidayPayDayMasterId='" + MID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strDetailsSQL1, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count >0)
                {
                    Exception ex = new Exception("Please Delete Details First....");
                    throw (ex);
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strMasterSQL = "DELETE FROM HolidayPayDayMaster WHERE ID='" + MID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strMasterSQL, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteDetails(string DetailsId)
        {
            string strDetailsSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strDetailsSQL = "DELETE FROM  HolidayPayDayDetails WHERE ID='" + DetailsId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strDetailsSQL, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        public class HolidayPayDayMaster : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string PolicyName { get; set; }
            public string PolicyDescription { get; set; }
            public bool IsActive { get; set; }
            public string PlantId { get; set; }
            public string CompanyGroupId { get; set; }

            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }

        public class HolidayPayDayDetails : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string SalaryHeadId { get; set; }
            public string HolidayPayDayMasterId { get; set; }          
            #endregion Scalar Properties
        }
        
        #endregion
    }
}