#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Modules;
using Library.Security.Core;
using Library.Service.Modules;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Modules.Controllers
{
    public class ModuleExtendedController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
       // private readonly IModuleExtendedService _moduleExtendedService;

        public ModuleExtendedController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        //[HttpGet]
        //public ActionResult GetList(string companyGroupId)
        //{
        //    return Json(_moduleExtendedService.Query(companyGroupId), JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public JsonResult Save(IEnumerable<ModuleExtended> moduleExtendeds)
        //{
        //    _moduleExtendedService.Save(moduleExtendeds);
        //    return Json(new { Message = AplosMessage.Insert });
        //}
        [HttpGet, Authorize]
        public ActionResult GetList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " LIKE '%" + value + "%'";


            string sql = @"SELECT CG.UserName CompanyGroup,ME.* FROM [MMS].[ModuleExtended] ME 
                            LEft JOIN ORG.CompanyGroup CG ON CG.Id = ME.CompanyGroupId WHERE " + strkey + "";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

       

        [HttpPost, Authorize]
        public ActionResult Save(Dictionary<string, object> moduleextended)
        {
            try
            {
                string _message = "";
                DataSet dsNotificationURL;
                DataSet dsCompanyGroup;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [MMS].[ModuleExtended] where Id='" + moduleextended["Id"] + "'", out dsNotificationURL, false, "1");

                con.OpenDataSetThroughAdapter("select * from [MMS].[ModuleExtended] where CompanyGroupId='" + moduleextended["CompanyGroupId"] + "'", out dsCompanyGroup, false, "1");




                DataRow dr;
                string _NotificationURLId = "";
                string _Id = "";

                #region task master
                if (dsNotificationURL.Tables[0].Rows.Count == 0)
                {
                    if (dsCompanyGroup.Tables[0].Rows.Count > 0)
                    {
                        return Json(new { Error = true, Message = "This Company Group already has SMS End Point, Please Check..." }, JsonRequestBehavior.AllowGet);

                    }
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ModuleExtended), out _Id);

                    moduleextended["Id"] = _Id;
                    AddNewRow(dsNotificationURL.Tables[0], moduleextended);
                    _message = "Data saved successfully";
                }
                else
                {
                    _NotificationURLId = moduleextended["Id"].ToString();
                    EditRow(dsNotificationURL.Tables[0].Rows[0], moduleextended);
                    _message = "Data updated successfully";
                }
                #endregion task master


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsNotificationURL);
                return Json(new { Error = false, Id = _NotificationURLId, Message = _message }, JsonRequestBehavior.AllowGet);
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

        [Authorize, HttpPost]
        public ActionResult Get(string SystemId)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from [MMS].[ModuleExtended] where Id='" + SystemId + "'");

                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpGet, Authorize]
        public ActionResult Delete(string SystemId)
        {
            try
            {
                if (string.IsNullOrEmpty(SystemId))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from  [MMS].[ModuleExtended] where Id='" + SystemId + "'");

                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
    }
}