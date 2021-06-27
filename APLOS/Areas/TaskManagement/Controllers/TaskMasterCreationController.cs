using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System.Data;
using Syncfusion.XlsIO;
using System.Web;
using System.IO;
using Library.Service.Helpers;
using OTSBD;
using System.Linq;
using Library.Service.Enums;

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class TaskMasterCreationController : BaseController
    {
        //GetList Save Delete

        #region Constructor

        private readonly ISqlRepository _sqlRepository;


        public TaskMasterCreationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (SELECT tm.Id, tm.Sequence,p.UserName AS Process,tc.UserName AS TaskCategory,d.UserName AS Department, tm.TaskDescription, tm.UserDefineTask, tm.Code,
                               tm.TaskType,tm.Active
                          FROM taskmaster TM
                        LEFT OUTER JOIN hkp.TaskCategory AS tc ON tm.TaskCategoryId=tc.Id
                        LEFT OUTER JOIN hkp.Process AS p ON p.Id=tm.ProcessId
                        LEFT OUTER JOIN org.Department AS d ON d.Id=tm.DepartmentId) AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMasterData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            var _plants = _sqlRepository.GetDataCollection("SELECT * FROM org.Plant AS p WHERE active=1 and p.CompanyGroupId='" + identity.CompanyGroupId + "' ORDER BY p.Sequence");
            var _Process = _sqlRepository.GetDataCollection("SELECT * FROM hkp.process AS p WHERE active=1 and p.CompanyGroupId='" + identity.CompanyGroupId + "' ORDER BY p.Sequence");
            var _department = _sqlRepository.GetDataCollection("SELECT * FROM org.Department  AS p where active=1 ORDER BY p.Sequence");
            var _taskAppliedOn = _sqlRepository.GetDataCollection("SELECT * FROM HKP.TaskAppliedOn AS p ");
            var _taskCategory = _sqlRepository.GetDataCollection("SELECT * FROM HKP.TaskCategory AS p where active=1 AND FLAG='" + TaskCategoryFlagEnum.TNA.ToString() + "' ORDER BY p.Sequence");
            var _taskSubCategory = _sqlRepository.GetDataCollection("SELECT * FROM HKP.TaskSubCategory AS p where active=1 AND FLAG='" + TaskCategoryFlagEnum.TNA.ToString() + "' ORDER BY p.Sequence");




            return Json(new { Plant = _plants, Process = _Process, Department = _department, TaskAppliedOn = _taskAppliedOn, TaskCategory = _taskCategory, TaskSubCategory = _taskSubCategory }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult GetDependentDateData(string dependon)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            var _taskdependentdate = _sqlRepository.GetDataCollection("SELECT * FROM HKP.TaskDependentDates AS p where TaskDependentOn IN (SELECT TaskAppliedOnEnum FROM hkp.TaskAppliedOn WHERE Id='" + dependon + "') Order By UserName");



            return Json(new { TaskDependentDates = _taskdependentdate }, JsonRequestBehavior.AllowGet);
        }


        #endregion
        [HttpPost]
        public ActionResult Save(Dictionary<string, object> taskmaster, List<Dictionary<string, object>> subtasks, string plants)
        {
            try
            {

                if (string.IsNullOrEmpty(taskmaster["TaskAppliedOnId"].ToString()) || taskmaster["TaskAppliedOnId"].ToString() == "null")
                    throw new Exception("Please select task applied on");

                if (string.IsNullOrEmpty(taskmaster["TaskDependentDatesId"].ToString()) || taskmaster["TaskDependentDatesId"].ToString() == "null")
                    throw new Exception("Please select task dependent date");




                taskmaster["LagDays"] = Math.Ceiling(clsStaticInfo.dbl(taskmaster["LagDays"]));
                taskmaster["StandardDays"] = Math.Ceiling(clsStaticInfo.dbl(taskmaster["StandardDays"]));


                DataSet dsMaster, dsChild, dsPlants;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaskMaster where Id='" + taskmaster["Id"] + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter("select * from SubTasks where TaskMasterId='" + taskmaster["Id"] + "'", out dsChild, false, "1");
                con.OpenDataSetThroughAdapter("select * from TaskMasterPlantAssignment where TaskMasterId='" + taskmaster["Id"] + "'", out dsPlants, false, "1");


                DataRow dr;
                string _TaskMasterId = "";

                #region task master
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Task Master Creation", out _TaskMasterId);
                    _TaskMasterId = _TaskMasterId.Replace("-", "").Substring(2);


                    taskmaster["Id"] = _TaskMasterId;
                    AddNewRow(dsMaster.Tables[0], taskmaster);
                }
                else
                {
                    _TaskMasterId = taskmaster["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], taskmaster);
                }
                #endregion task master

                #region task childs-[sub tasks]
                if (subtasks != null && subtasks.Count > 0)
                {

                    for (int i = 0; i < dsChild.Tables[0].Rows.Count; i++)
                    {
                        List<Dictionary<string, object>> temp = subtasks.Where(ee => ee.ContainsValue((int)clsStaticInfo.dbl(dsChild.Tables[0].Rows[i]["Id"].ToString()))).ToList();
                        if (temp.Count == 0)
                            dsChild.Tables[0].Rows[i].Delete();
                    }

                    for (int i = 0; i < subtasks.Count; i++)
                    {
                        subtasks[i]["TaskMasterId"] = _TaskMasterId;

                        //
                        if (subtasks[i]["Id"] == null)
                        {
                            AddNewRow(dsChild.Tables[0], subtasks[i]);

                        }
                        else
                        {
                            dsChild.Tables[0].DefaultView.RowFilter = "Id=" + (int)clsStaticInfo.dbl(subtasks[i]["Id"].ToString()) + "";
                            if (dsChild.Tables[0].DefaultView.Count > 0)
                                EditRow(dsChild.Tables[0].DefaultView[0].Row, subtasks[i]);

                        }
                    }

                }
                else
                {
                    while (dsChild.Tables[0].DefaultView.Count > 0)
                        dsChild.Tables[0].DefaultView[0].Delete();

                }

                #endregion task childs-[sub tasks]

                #region plant Assignments

                while (dsPlants.Tables[0].DefaultView.Count > 0)
                    dsPlants.Tables[0].DefaultView[0].Delete();

                if (plants != null || string.IsNullOrEmpty(plants) == false)
                {
                    string[] plantids = plants.Split(',');
                    foreach (string item in plantids)
                    {
                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        dr = dsPlants.Tables[0].NewRow();

                        dr["TaskMasterId"] = _TaskMasterId;
                        dr["PlantId"] = item.Trim();

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsPlants.Tables[0].Rows.Add(dr);
                    }

                }
                #endregion plant Assignments

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild, dsPlants);

                return Json(new { Error = false, Id = _TaskMasterId, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM TaskMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
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

        [HttpGet]
        public ActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TaskMasterPlantAssignment where TaskMasterId='" + id + "'");
                con.executeQuery("delete from SubTasks where TaskMasterId='" + id + "'");
                con.executeQuery("delete from TaskMaster where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {

                var _master = _sqlRepository.GetDataCollection("select * from TaskMaster where Id='" + Id + "'");
                var _subtasks = _sqlRepository.GetDataCollection("select * from SubTasks where TaskMasterId='" + Id + "'");
                var _plants = _sqlRepository.GetDataCollection("select * from TaskMasterPlantAssignment where TaskMasterId='" + Id + "'");


                return Json(new { master = _master, subtasks = _subtasks, plants = _plants }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }


    }
}