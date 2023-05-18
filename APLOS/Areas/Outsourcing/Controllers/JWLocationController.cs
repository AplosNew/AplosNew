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
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Outsourcing.Controllers
{
    public class JWLocationController : BaseController
    {
        string TableName = "JWLocation";
        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public JWLocationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor



        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from JWLocation where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [Authorize, HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT JWL.Id,JWL.EntityId,JWL.Code,JWL.Sequence,JWL.UserName,JWL.ShortName,JWL.StandardName,ISNULL(JWL.StorageLocationId,'') StorageLocationId
                            ,ISNULL(JWL.ResponsiblePersonId,'') ResponsiblePersonId
                            ,ISNULL(EEI.EmployeeName,'') ResponsiblePersonName,ENT.UserName Entity, ISNULL(STRL.UserName,'') StorageLocation
                        , Plant.UserName Plant,Plant.Id PlantId, cmp.UserName  Company, cmp.Id CompanyId 
                        FROM JWLocation JWL 
                        LEFT JOIN EmployeeInformation EEI ON JWL.ResponsiblePersonId = EEI.SystemId
                        LEFT JOIN ORG.Entity ENT ON JWL.EntityId = ENT.Id
                        LEFT JOIN ORG.Plant Plant ON ENT.PlantId = Plant.Id
                        LEFT JOIN ORG.Company Cmp ON ENT.CompanyId = Cmp.Id
                        LEFT JOIN HKP.MaterialStorage STRL ON JWL.StorageLocationId = STRL.Id WHERE " + strkey + " ORDER BY JWL.sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetPlantList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Id,CompanyId,plant.UserName FROM ORG.Plant ORDER BY Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetEntityListA()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Id,CompanyId,PlantId,UserName FROM ORG.Entity ORDER BY UserName";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult GetJWActivityList(string activityId)
        {
            string strkey = "1=1";
            if (!string.IsNullOrEmpty(activityId))
            {
                strkey += "AND JWA.Id = '" + activityId + @"'";

            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'), JWA.Id JWActivityId, JWA.UserName JWActivity,EEI.EmployeeName ResponsiblePersonName,SM.UserName ServiceName from JWActivity JWA 
                        Left Join EmployeeInformation EEI ON JWA.ResponsiblePersonId = EEI.SystemId
                        --Left Join HKP.Process PRC ON JWA.ProcessId = PRC.Id
                        Left Join HKP.ServiceMaster SM ON JWA.ServiceId = SM.Id WHERE " + strkey + " ORDER BY JWA.sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult GetJWLocationActivityListById(string jwLocationId)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT [isToBeSelect] = Convert(bit, 'False'), JWA.Id JWActivityId, JWA.UserName JWActivity, EEI.EmployeeName ResponsiblePersonName
             , SM.UserName ServiceName from JWLocationActivity JWLA
              left Join JWActivity JWA ON JWLA.JWActivityId = JWA.Id
                Left Join EmployeeInformation EEI ON JWA.ResponsiblePersonId = EEI.SystemId
                    --Left Join HKP.Process PRC ON JWA.ProcessId = PRC.Id
                    Left Join HKP.ServiceMaster SM ON JWA.ServiceId = SM.Id
                    where JWLA.JWLocationId = '" + jwLocationId + @"'";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetStorageLocationList()
        {
            string strSql = @"SELECT MS.Id,ISNULL(cmp.ShortName,'') +'-' +ISNULL(Plant.ShortName,'')+'-'+ ISNULL(MS.UserName,'') UserName,PlantId FROM HKP.MaterialStorage MS
                            LEFT JOIN ORG.Plant Plant ON Plant.Id = MS.PlantId
                            LEFT JOIN ORG.Company cmp ON cmp.Id = plant.CompanyId
                            ORDER BY MS.Sequence";
            return Json(_sqlRepository.GetDataCollection(strSql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEntityList(string plantId)
        {
            string strSql = @"SELECT * FROM ORG.Entity WHERE PlantId = '" + plantId + @"' ";
            return Json(_sqlRepository.GetDataCollection(strSql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> ActivityList)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                DataSet dsActivity;
                string JobWorkLocationId = "";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same user name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "JWL" + _Id;
                    JobWorkLocationId = data["Id"].ToString();
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    JobWorkLocationId = _Id;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update




                #region Activity
                string sql = "";
                string _activityId = "";
                sql = "SELECT * FROM JWLocationActivity WHERE JWLocationId='" + JobWorkLocationId + "'";
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out dsActivity, false, "1");
                for (int i = 0; i < dsActivity.Tables[0].Rows.Count; i++)
                {
                    List<Dictionary<string,object>> k =new List<Dictionary<string, object>>();
                    if (ActivityList == null)
                    {
                        k = new List<Dictionary<string, object>>();
                    }
                    else
                    {
                       k= ActivityList.Where(ee => ee["JWActivityId"].ToString() == dsActivity.Tables[0].Rows[i]["JWActivityId"].ToString()).ToList();

                    }
                    if (k.Count == 0 || k==null)
                    {
                        dsActivity.Tables[0].Rows[i].Delete();
                    }
                }
                if (ActivityList != null)
                {


                    for (int i = 0; i < ActivityList.Count; i++)
                    {
                        dsActivity.Tables[0].DefaultView.RowFilter = "JWActivityId='" + ActivityList[i]["JWActivityId"] + "'";


                        if (dsActivity.Tables[0].DefaultView.Count == 0)
                        {

                            if (_activityId == "")
                            {
                                bplib.clsGenID id = new bplib.clsGenID();
                                id.GenID("JWLocationActivity", out _activityId);
                                _activityId = "LA" + _activityId;
                            }
                            DataRow dr = dsActivity.Tables[0].NewRow();
                            dr["Id"] = _activityId + "-" + (i + 1).ToString();

                            dr["JWLocationId"] = JobWorkLocationId;

                            dr["JWActivityId"] = ActivityList[i]["JWActivityId"];



                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;


                            dsActivity.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dsActivity.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["JWActivityId"] = bplib.clsWebLib.RetValidLen(ActivityList[i]["JWActivityId"]);

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;


                            dr.EndEdit();

                        }

                    }
                }


                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsActivity);

                return Json(new { Data = data, Error = false, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            string sql = @"select * from [JWLocation] where id = '" + id + "'";
            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from JWLocationActivity where JWLocationId='" + id + "'");
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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
                    dr[item] = bplib.clsWebLib.RetValidLen(sourceData[item]);
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
                    dr[item] = bplib.clsWebLib.RetValidLen(sourceData[item]);
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
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
    }
}