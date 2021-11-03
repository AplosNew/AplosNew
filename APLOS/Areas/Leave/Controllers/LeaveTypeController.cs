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

#endregion

namespace Aplos.Areas.Leave.Controllers
{
    public class LeaveTypeController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        public LeaveTypeController(
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

        [HttpGet]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select lt.Id,lt.Sequence,lt.Code,lt.ShortName,lt.StandardName,lt.UserName,lt.UserName,lt.IsESIC,lt.IsGeneral
								,IsESICS=CASE WHEN lt.IsESIC=1 THEN 'Yes' else 'No' end
								,IsGenerals=CASE WHEN lt.IsGeneral=1 THEN 'Yes' else 'No' end,lt.LeaveType
								from [dbo].[LeaveType] lt
	                            where lt.CompanyGroupId='" + identity.CompanyGroupId+@"' 
                                  ORDER BY lt.Sequence";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM [dbo].[LeaveType]");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [HttpPost]
        public ActionResult Save(LeaveTypeModel LeaveType)
        {
            try
            {
                SaveLeaveType(LeaveType);              
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void SaveLeaveType(LeaveTypeModel LeaveType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM [dbo].[LeaveType] WHERE ID='" + LeaveType.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[LeaveType]", out sID);                    
                    dr["Id"] = "SP" + sID;
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["LeaveType"] = LeaveType.LeaveType;
                    dr["Sequence"] = LeaveType.Sequence;
                    dr["Code"] = LeaveType.Code;
                    dr["ShortName"] = LeaveType.ShortName;
                    dr["StandardName"] = LeaveType.StandardName;
                    dr["UserName"] = LeaveType.UserName;
                    dr["Description"] = LeaveType.Description;
                    dr["Remarks"] = LeaveType.Remarks;
                    dr["IsESIC"] = LeaveType.IsESIC;
                    dr["IsGeneral"] = LeaveType.IsGeneral;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["LeaveType"] = LeaveType.LeaveType;
                    dr["Sequence"] = LeaveType.Sequence;
                    dr["Code"] = LeaveType.Code;
                    dr["ShortName"] = LeaveType.ShortName;
                    dr["StandardName"] = LeaveType.StandardName;
                    dr["UserName"] = LeaveType.UserName;
                    dr["Description"] = LeaveType.Description;
                    dr["Remarks"] = LeaveType.Remarks;
                    dr["IsESIC"] = LeaveType.IsESIC;
                    dr["IsGeneral"] = LeaveType.IsGeneral;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
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

        public void Delete(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            DataSet dsMaster;

            try
            {
                string sql3 = "SELECT * FROM [dbo].[LeaveTransaction] WHERE LTSystemId='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql3, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("Already used in leave transaction..");
                    throw (ex);
                }
                strSQL = "DELETE FROM  [dbo].[LeaveType] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function
        public class LeaveTypeModel : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string CompanyGroupId { get; set; }
            public string LeaveType { get; set; }            
            public decimal Sequence { get; set; }            
            public string Code { get; set; }           
            public string ShortName { get; set; }           
            public string StandardName { get; set; }           
            public string UserName { get; set; }            
            public string Description { get; set; }
            public string Remarks { get; set; }            
            public string PlantId { get; set; }
            public bool Active { get; set; }  
            public bool IsESIC { get; set; }
            public bool IsGeneral { get; set; }

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