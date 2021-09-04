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


#endregion Using

namespace Aplos.Areas.EmployeeServices.Controllers
{
    public class EmployeeServiceTypeController : BaseController
    {
        string TableName = "dbo.EmpServiceType";
        string TableName1 = "dbo.EmpServiceCategory";


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public EmployeeServiceTypeController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
       


   
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM "+ TableName +"  "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalaryHead()
        {
            return Json(_sqlRepository.GetDataCollection("select SalaryHeadID as Value,SalaryHead+' ['+HeadType+']' as Text from[dbo].[SalaryHead] ORDER BY HeadType DESC, SalaryHead "), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from dbo.EmpServiceType where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           

            string sql = @"select top 100 * from (select distinct est.*,uom.Username as UOM,sh.SalaryHead as SalaryHead
                                                 ,SelectMode=case when SelectionMode=0 then 'Entry' else 'Scan' End
                                                 from dbo.EmpServiceType est left join SCS.UnitOfMeasurement uom on est.UOMId=uom.Id
												 left join dbo.SalaryHead sh on est.SalaryHeadId=sh.SalaryHeadID) AS TEMP WHERE " + strkey + " order by Service ";

          return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmpServiceType", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
             

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");



                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Service='" + data["Service"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Service already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0  && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                //    data["Id"] = "EST" + _Id;
                    data["Id"] = "EST" + GetPK();
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            

            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
           
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(id))
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where EmpServiceTypeId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Service Category");
                    }

                }

                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

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

        // *************** Service Category Tab ***************************

        [HttpPost, Authorize]
        public JsonResult SaveServiceCategory(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where EmpServiceTypeId='" + data["EmpServiceTypeId"] + "' AND  Id<>'" + data["Id"] + "' and Category='" + data["Category"] + "' ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Service and Category already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0 && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName1, out _Id);

                    data["Id"] = "ESC" + _Id;
                    AddNewRowServiceCategory(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    //  EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        private void AddNewRowServiceCategory(DataTable dt, Dictionary<string, object> sourceData)
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

        [HttpGet, Authorize]
        public JsonResult GetListServiceCategory(string EmpServiceTypeId)
        {

            string sql = @"select top 100 * from (select distinct esc.*,est.Service as Service
                                                  from dbo.EmpServiceCategory esc left join dbo.EmpServiceType est on est.Id=esc.EmpServiceTypeId
                                                  where EmpServiceTypeId= '" + EmpServiceTypeId + "') AS TEMP order by Service";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DeleteServiceCategory(string Id)
        {
            try
            {
                string sql = @" delete from dbo.EmpServiceCategory where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Service Category deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }

}