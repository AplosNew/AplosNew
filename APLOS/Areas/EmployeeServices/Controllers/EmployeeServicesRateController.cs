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
    public class EmployeeServicesRateController : BaseController
    {
        string TableName = "dbo.EmployeeServicesRate";
      


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public EmployeeServicesRateController(ISqlRepository R)
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


        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from dbo.EmployeeServicesRate where Id = '" + Id + "' ");


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
           

            string sql = @"select top 100 * from (select distinct esc.*,est.Id as ESTId,est.Service,est.Form,esr.Id as ESRId,esr.EffectiveDate,esr.Rate from dbo.EmpServiceCategory esc
                                                       left join dbo.EmpServiceType est on est.Id=esc.EmpServiceTypeId
													   left join dbo.EmployeeServicesRate esr on esr.EmployeeServiceCategoryId=esc.Id) AS TEMP WHERE " + strkey + " order by EffectiveDate desc ";

          return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
             

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where ServiceNameId='" + data["ServiceNameId"] + "' AND  Id<>'" + data["Id"] + "' AND CategoryLocationId='"+data["CategoryLocationId"] +"' ", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same Service and Category already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0  && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "ESR" + _Id;
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

        [HttpGet, Authorize]
        public JsonResult LoadAllSelectedServiceRateTab(string CategoryId)
        {

            string sql = @"select distinct esc.*,est.Id as ESTId,est.Service,est.Form,esr.Id as ESRId,esr.EffectiveDate,esr.Rate, esr.NonChargeableGLCode from dbo.EmpServiceCategory esc
                                                       left join dbo.EmpServiceType est on est.Id=esc.EmpServiceTypeId
													   left join dbo.EmployeeServicesRate esr on esr.EmployeeServiceCategoryId=esc.Id
													   where esc.Id='" + CategoryId + "' order by EffectiveDate desc ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DeleteEmpSerRate(string Id)
        {
            try
            {
                string sql = @" delete from dbo.EmployeeServicesRate where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Employee Service Rate deleted successfully"
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