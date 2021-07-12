using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Library.Security.Core;

namespace Aplos.Areas.JobWork.Controllers
{
    public class JobWorkLocationController : BaseController
    {
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        public JobWorkLocationController(SqlRepository Repository)
        {
            _sqlRepository = Repository;
        }
        #endregion
        #region Pages
        // GET: IE/JobWorkLocation
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        #region Code Master Data
        [HttpGet, Authorize]
        public JsonResult GetAllPlant()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = "";
            sql = @"SELECT Id,UserName FROM [ORG].[Plant]  WHERE CompanyGroupId='" + identity.CompanyGroupId + "' ORDER BY UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAllEntity(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = "";
            sql = @"SELECT Id,UserName FROM [ORG].[Entity] WHERE PlantId='" + Id.ToString() + "' AND CompanyGroupId='" + identity.CompanyGroupId + "' ORDER BY UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAllStoreLocation(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = "";
            sql = @"SELECT Id,UserName FROM [HKP].[MaterialStorage] WHERE PlantId='" + Id.ToString() + "' AND CompanyGroupId='" + identity.CompanyGroupId + "' ORDER BY UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllData()
        {
            string sql = "";
            sql = @"SELECT L.Id,P.UserName PlantId,OE.UserName EntityId,L.LocationName,L.LocationCode,MS.UserName StoreLocationId,L.ResponsiblePerson1Id,E.EmployeeName ResponsiblePerson1Name,
                    L.ResponsiblePerson2Id,EE.EmployeeName ResponsiblePerson2Name,L.Remarks,L.IsActive
                    FROM [HKP].[JobWorkLocation] L
                    INNER JOIN [ORG].[Entity] OE ON OE.Id = L.EntityId
                    INNER JOIN [ORG].[Plant] P ON P.Id = OE.PlantId
                    LEFT JOIN  [HKP].[MaterialStorage] MS ON MS.Id = L.StoreLocationId
                    INNER JOIN [dbo].[EmployeeInformation] E ON E.SystemId = L.ResponsiblePerson1Id
                    INNER JOIN [dbo].[EmployeeInformation] EE ON EE.SystemId = L.ResponsiblePerson2Id
                    ORDER BY L.LocationName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllActivityName()
        {
            string sql = "";
            sql = @"SELECT A.Id,A.Code,A.ShortName,A.StandardName,A.UserName,A.Type, E.EmployeeName ResponsiblePerson
                    FROM HKP.JobWorkActivity A
                    LEFT JOIN dbo.EmployeeInformation E ON E.SystemId = A.ResponsiblePersonId
                    WHERE A.IsActive = 1
                    ORDER BY A.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetSelectedData(string Id)
        {
            string sql = "";
            sql = @"SELECT L.Id,OE.PlantId,L.EntityId,L.LocationName,L.LocationCode,L.StoreLocationId,L.ResponsiblePerson1Id,E.EmployeeName ResponsiblePerson1Name,
                    L.ResponsiblePerson2Id,EE.EmployeeName ResponsiblePerson2Name,L.Remarks,L.IsActive
                    FROM [HKP].[JobWorkLocation] L
                    INNER JOIN [ORG].[Entity] OE ON OE.Id = L.EntityId
                    INNER JOIN [ORG].[Plant] P ON P.Id = OE.PlantId
                    INNER JOIN [dbo].[EmployeeInformation] E ON E.SystemId = L.ResponsiblePerson1Id
                    INNER JOIN [dbo].[EmployeeInformation] EE ON EE.SystemId = L.ResponsiblePerson2Id
                    WHERE L.Id='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult DeleteSelectedData(string Id)
        {
            try
            {
                DataSet dsMaster;
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                if (!string.IsNullOrEmpty(Id))
                {
                    con.OpenDataSetThroughAdapter("select * from HKP.JobWorkLocationChild where JobWorkLocationId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Job Work Activity data");
                    }
                }

                con.BeginTransaction();
                con.executeQuery("DELETE FROM [HKP].[JobWorkLocation] WHERE Id='" + Id + "'");

                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkLocation", out sID);
            return sID;
        }

        [HttpPost, Authorize]
        public ActionResult SaveData(Dictionary<string, object> saveData)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

            try
            {
   
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID _genId = new bplib.clsGenID();
                string _Message = "";
                string Id = "";
                con.getDataSet("SELECT * FROM [HKP].[JobWorkLocation] WHERE Id='" + saveData["Id"] + "'", out DataSet dsOut);
                if (dsOut.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsOut.Tables[0].NewRow();
                    _genId.GenID("[HKP].[JobWorkLocation]", out Id);
    
                    dr["Id"] ="JL" + GetPK();
                    dr["EntityId"] = saveData["EntityId"].ToString();
                    dr["LocationName"] = saveData["LocationName"].ToString();
                    dr["LocationCode"] = saveData["LocationCode"].ToString();
                    dr["StoreLocationId"] = saveData["StoreLocationId"].ToString();
                    dr["ResponsiblePerson1Id"] = saveData["ResponsiblePerson1Id"].ToString();
                    dr["ResponsiblePerson2Id"] = saveData["ResponsiblePerson2Id"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
                    dr["IsActive"] = saveData["IsActive"].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsOut.Tables[0].Rows.Add(dr);

                    _Message = "Data Save Successfully..!";

                }
                else
                {
                    DataRow dr = dsOut.Tables[0].Rows[0];
                    Id = dr["Id"].ToString();
                    dr.BeginEdit();
     
                    dr["EntityId"] = saveData["EntityId"].ToString();
                    dr["LocationName"] = saveData["LocationName"].ToString();
                    dr["LocationCode"] = saveData["LocationCode"].ToString();
                    dr["StoreLocationId"] = saveData["StoreLocationId"].ToString();
                    dr["ResponsiblePerson1Id"] = saveData["ResponsiblePerson1Id"].ToString();
                    dr["ResponsiblePerson2Id"] = saveData["ResponsiblePerson2Id"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
                    dr["IsActive"] = saveData["IsActive"].ToString();
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                    _Message = "Data Updated Successfully..!";
                }

                saveData["Id"] = dsOut.Tables[0].Rows[0]["Id"].ToString();
                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsOut);

                return Json(new { Error = false, Message = _Message, Data = saveData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Code Child Data
    


        // Delete Child
        [HttpGet]
        public JsonResult DelLocationChild(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [HKP].[JobWorkLocationChild] WHERE Id='" + Id + "' ");

                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult LoadJobActivityForSelection(string MasterId)
        {
            string sql = "";
            sql = @"select ja.*, emp.EmployeeName as ResponsiblePerson
                                   from HKP.JobWorkActivity ja left join dbo.EmployeeInformation emp on emp.SystemId=ja.ResponsiblePersonId
                                   WHERE isnull(ja.Id,'') not in (select isnull(JobWorkActivityId,'') from HKP.JobWorkLocationChild where JobWorkLocationId='" + MasterId + @"')
                                   ORDER BY ja.Code";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult LoadAllSelectedJobLocationTab(string JobLocationMasterId)
        {
            string sql = "";
            sql = @"select jlc.*, jl.LocationName, ja.UserName as JobWorkActivity, ja.Code, ja.Type as JobWorkActivityType, emp.EmployeeName as ResponsiblePerson
                                      from HKP.JobWorkLocationChild jlc left join HKP.JobWorkLocation jl
                                      on jl.Id=jlc.JobWorkLocationId
									  left join HKP.JobWorkActivity ja on ja.Id=jlc.JobWorkActivityId
									  left join dbo.EmployeeInformation emp on emp.SystemId=ja.ResponsiblePersonId
									  where jlc.JobWorkLocationId='" + JobLocationMasterId + @"' ORDER BY ja.UserName ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        private string GetPKC()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkActivityChild", out sID);
            return sID;
        }


        [HttpPost, Authorize]
        public ActionResult SaveJobLocationChildTab(string JobLocationMasterId, List<Dictionary<string, object>> JobActivtiyTabData)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsData;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

                con.getDataSet("select * from HKP.JobWorkLocationChild where 1=2", out dsData);

                string id = "";
                for (int i = 0; i < JobActivtiyTabData.Count; i++)
                {
                    if (id == "")
                    {
                        bplib.clsGenID _gen = new bplib.clsGenID();
                        _gen.GenID("JobWorkLocationChild", out id);
                    }

                    DataRow dr = dsData.Tables[0].NewRow();
                    dr["Id"] = "JLC" + GetPKC();
                    dr["JobWorkLocationId"] = JobLocationMasterId;
                    dr["JobWorkActivityId"] = JobActivtiyTabData[i]["Id"].ToString();

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsData.Tables[0].Rows.Add(dr);

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsData);

                return Json(new { Error = false, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}