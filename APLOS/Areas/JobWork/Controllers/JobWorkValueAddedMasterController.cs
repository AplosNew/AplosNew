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

namespace Aplos.Areas.JobWork.Controllers
{
    public class JobWorkValueAddedMasterController : BaseController
    {
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        public JobWorkValueAddedMasterController(SqlRepository Repository)
        {
            _sqlRepository = Repository;
        }
        #endregion
        #region Pages
        // GET: IE/JobWorkValueAddedMaster
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        #region Code Area

        [HttpGet, Authorize]
        public JsonResult GetAllData()
        {
            string sql = "";
            sql = @"SELECT M.Id,JA.UserName JobWorkActivityId,JI.UserName JobWorkActivityChildId,M.StdRejection,M.StdValueLoss,M.RateApplicable,
                    C.Code Currency,M.MinRate,M.MaxRate,M.CycleTime,E.EmployeeName ResponsiblePerson,M.ResponsiblePersonId,M.Remarks
                    FROM [MST].[JobWorkValueAddedMaster] M
                    INNER JOIN [SCS].[Currency] C ON C.Id = M.CurrencyId
                    LEFT JOIN DBO.EmployeeInformation E ON E.SystemId = M.ResponsiblePersonId
                    INNER JOIN [HKP].[JobWorkActivity] JA ON JA.Id = M.JobWorkActivityId
                    INNER JOIN [HKP].[JobWorkItem] JI ON JI.Id = M.JobWorkActivityChildId";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getJobWorkItemUOM(string Id)
        {
            string sql = "";
            sql = @"SELECT jwi.UOMId,uom.UserName as JWIUnit, jwi.MaterialMasterId, mm.Code as MaterialCode, mm.UserName as Material, mm.BaseUOMId,unt.UserName as MMUnit  
                    FROM [HKP].[JobWorkItem] jwi
                    left JOIN [SCS].[UnitOfMeasurement] uom ON uom.Id = jwi.UOMId
					left join MST.MaterialMaster mm on mm.Id=jwi.MaterialMasterId
					left join scs.UnitOfMeasurement unt on unt.Id=mm.BaseUOMId
					where jwi.Id='" + Id + @"' ORDER BY uom.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllCurrency()
        {
            string sql = "";
            sql = @"SELECT ID,Code FROM [SCS].[Currency] ORDER BY Code";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllActivityUserName()
        {
            string sql = "";
            sql = @"SELECT Id,UserName FROM [HKP].[JobWorkActivity] WHERE Type='Value Added' AND IsActive = 1 ORDER BY UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllProcessName()
        {
            string sql = "";
            sql = @"SELECT Id,UserName FROM [HKP].[Process] ORDER BY UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetJobWorkMaterialUOM(string Id)
        {
            string sql = "";
            sql = @"SELECT DISTINCT I.UOMId,U.UserName FROM [HKP].[JobWorkItem] I
                    INNER JOIN [SCS].[UnitOfMeasurement] U ON U.Id = I.UOMId
                    WHERE I.Id ='" + Id + "' ORDER BY U.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetActivityChildItems(string Id)
        {
            string sql = "";
            sql = @"SELECT I.Id,I.UserName 
                    FROM [HKP].[JobWorkActivityChild] C
                    INNER JOIN HKP.JobWorkItem I ON I.Id = C.JobWorkItemId
                    WHERE C.JobWorkActivityId ='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveData(Dictionary<string, object> saveData, List<Dictionary<string, string>> childData)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

            if (childData == null || childData.Count == 0)
                throw new Exception("No child data found");

            DataTable dtChild = new DataTable();

            var columnNames = childData.SelectMany(dict => dict.Keys).Distinct();
            dtChild.Columns.AddRange(columnNames.Select(c => new DataColumn(c)).ToArray());
            foreach (Dictionary<string, string> item in childData)
            {
                var row = dtChild.NewRow();
                foreach (var key in item.Keys)
                {
                    row[key] = item[key];
                }

                dtChild.Rows.Add(row);
            }


            try
            {
                ConnectionManager.DAL.ConManager con2 = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID _genId = new bplib.clsGenID();
                string _Message = "";
                string Id = "";
                DataSet dsMaster;
                con2.OpenDataSetThroughAdapter("select * from MST.JobWorkValueAddedMaster where JobWorkActivityId='" + saveData["JobWorkActivityId"] + "' and JobWorkActivityChildId='" + saveData["JobWorkActivityChildId"] + "' AND  Id<>'" + saveData["Id"].ToString() + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Job Work Activity and Activity Item already exists!!!");

                con.getDataSet("SELECT * FROM [MST].[JobWorkValueAddedMaster] WHERE Id='" + saveData["Id"].ToString() + "'", out DataSet dsOut);
                if (dsOut.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsOut.Tables[0].NewRow();
                    _genId.GenID("[MST].[JobWorkValueAddedMaster]", out Id);
                    Id = "VAM-" + Id;
                    dr["Id"] = Id.ToString();

                    dr["JobWorkActivityId"] = saveData["JobWorkActivityId"].ToString();
                    dr["JobWorkActivityChildId"] = saveData["JobWorkActivityChildId"].ToString();
                    dr["StdRejection"] = OTSBD.clsStaticInfo.dbl(saveData["StdRejection"].ToString() == "" ? null : saveData["StdRejection"].ToString());
                    dr["StdValueLoss"] = OTSBD.clsStaticInfo.dbl(saveData["StdValueLoss"].ToString() == "" ? null : saveData["StdValueLoss"].ToString());
                    dr["RateApplicable"] = saveData["RateApplicable"].ToString();
                    dr["CurrencyId"] = saveData["CurrencyId"].ToString();
                    dr["MinRate"] = OTSBD.clsStaticInfo.dbl(saveData["MinRate"] == null ? null : saveData["MinRate"].ToString());
                    dr["MaxRate"] = OTSBD.clsStaticInfo.dbl(saveData["MaxRate"] == null ? null : saveData["MaxRate"].ToString());
                    dr["CycleTime"] = OTSBD.clsStaticInfo.dbl(saveData["CycleTime"] == null ? null : saveData["CycleTime"].ToString());
                    dr["ResponsiblePersonId"] = saveData["ResponsiblePersonId"] == null ? null : saveData["ResponsiblePersonId"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
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
                    dr["JobWorkActivityId"] = saveData["JobWorkActivityId"].ToString();
                    dr["JobWorkActivityChildId"] = saveData["JobWorkActivityChildId"].ToString();
                    dr["StdRejection"] = OTSBD.clsStaticInfo.dbl(saveData["StdRejection"].ToString() == "" ? null : saveData["StdRejection"].ToString());
                    dr["StdValueLoss"] = OTSBD.clsStaticInfo.dbl(saveData["StdValueLoss"].ToString() == "" ? null : saveData["StdValueLoss"].ToString());
                    dr["RateApplicable"] = saveData["RateApplicable"].ToString();
                    dr["CurrencyId"] = saveData["CurrencyId"].ToString();
                    dr["MinRate"] = OTSBD.clsStaticInfo.dbl(saveData["MinRate"].ToString() == "" ? null : saveData["MinRate"].ToString());
                    dr["MaxRate"] = OTSBD.clsStaticInfo.dbl(saveData["MaxRate"].ToString() == "" ? null : saveData["MaxRate"].ToString());
                    dr["CycleTime"] = OTSBD.clsStaticInfo.dbl(saveData["CycleTime"].ToString() == "" ? null : saveData["CycleTime"].ToString());
                    dr["ResponsiblePersonId"] = saveData["ResponsiblePersonId"] == null ? null : saveData["ResponsiblePersonId"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                    _Message = "Data Updated Successfully..!";
                }

                con.getDataSet("Select * from [MST].[JobWorkValueAddedMasterProcess] where JobWorkValueAddedMasterId='" + Id.ToString() + "'", out DataSet dsChild);

                for (int i = 0; i < dsChild.Tables[0].Rows.Count; i++)
                {
                    dtChild.DefaultView.RowFilter = "ProcessId='" + dsChild.Tables[0].Rows[i]["ProcessId"].ToString() + "'";
                    if (dtChild.DefaultView.Count == 0)
                        dsChild.Tables[0].Rows[i].Delete();
                }
                string ChildId = "";
                dtChild.DefaultView.RowFilter = null;
                for (int i = 0; i < dtChild.DefaultView.Count; i++)
                {
                    dsChild.Tables[0].DefaultView.RowFilter = "ProcessId='" + dtChild.Rows[i]["ProcessId"].ToString() + "'";
                    if (dsChild.Tables[0].DefaultView.Count == 0)
                    {
                        _genId.GenID("[MST].[JobWorkValueAddedMasterProcess]", out ChildId);
                        ChildId = "VAMP-" + ChildId;

                        DataRow dr = dsChild.Tables[0].NewRow();
                        dr["Id"] = ChildId;
                        dr["JobWorkValueAddedMasterId"] = Id;
                        dr["ProcessId"] = dtChild.Rows[i]["ProcessId"].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsChild.Tables[0].Rows.Add(dr);
                    }
                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsOut, dsChild);

                return Json(new { Error = false, Message = _Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult GetSelectedData(string Id)
        {
            string sql = "";
            sql = @"SELECT M.Id,JobWorkActivityId,M.JobWorkActivityChildId,M.StdRejection,M.StdValueLoss,M.RateApplicable,
                    M.CurrencyId,M.MinRate,M.MaxRate,M.CycleTime,M.ResponsiblePersonId,E.EmployeeName ResponsiblePerson,M.Remarks
                    FROM [MST].[JobWorkValueAddedMaster] M
                    LEFT JOIN DBO.EmployeeInformation E ON E.SystemId = M.ResponsiblePersonId
                    WHERE M.Id='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSelectedProcessData(string Id)
        {
            string sql = "";
            sql = @"SELECT V.Id,V.ProcessId,P.UserName Process
                    FROM [MST].[JobWorkValueAddedMasterProcess] V
                    INNER JOIN [HKP].[Process] P ON P.Id = V.ProcessId
                    WHERE V.JobWorkValueAddedMasterId='" + Id + "' ORDER BY P.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult DeleteSelectedData(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [MST].[JobWorkValueAddedMasterProcess] WHERE JobWorkValueAddedMasterId='" + Id.ToString() + "' DELETE FROM [MST].[JobWorkValueAddedMaster] WHERE Id='" + Id.ToString() + "'");

                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }
        [HttpPost]
        public JsonResult DeleteChildData(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [MST].[JobWorkValueAddedMasterProcess] WHERE Id='" + Id.ToString() + "'");

                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}