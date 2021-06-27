#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Modules;
using Library.Service.Modules;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Modules.Controllers
{
    public class NotificationURLController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;


        public NotificationURLController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        //[Authorize]
        //public ActionResult NotificationURL()
        //{
        //    return View();          
        //}

        [HttpGet, Authorize]
        public ActionResult NotificationURLGetList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " LIKE '%" + value + "%'";

        
            string sql = @"SELECT CG.UserName CompanyGroup,NURL.CompanyGroupId,NURL.URL,NURL.SystemId FROM [dbo].[NotificationURL] NURL LEft JOIN ORG.CompanyGroup CG ON CG.Id = NURL.CompanyGroupId WHERE " + strkey + "";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult NotificationURLSave(Dictionary<string, object> NotificationURLlist)
        {
            try
            {
                string _message = "";
                DataSet dsNotificationURL;
                DataSet dsCompanyGroup;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from NotificationURL where SystemId='" + NotificationURLlist["SystemId"] + "'", out dsNotificationURL, false, "1");

                con.OpenDataSetThroughAdapter("select * from NotificationURL where CompanyGroupId='" + NotificationURLlist["CompanyGroupId"] + "'", out dsCompanyGroup, false, "1");

                


                DataRow dr;
                string _NotificationURLId = "";

                #region task master
                if (dsNotificationURL.Tables[0].Rows.Count == 0)
                {
                    if (dsCompanyGroup.Tables[0].Rows.Count > 0)
                    {
                        return Json(new { Error = true, Message = "This Company Group already has an URL, Please Check..." }, JsonRequestBehavior.AllowGet);

                    }
                    AddNewRow(dsNotificationURL.Tables[0], NotificationURLlist);
                    _message = "Data saved successfully";
                }
                else
                {
                    _NotificationURLId = NotificationURLlist["SystemId"].ToString();
                    EditRow(dsNotificationURL.Tables[0].Rows[0], NotificationURLlist);
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

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
                             

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
         

            dr.EndEdit();
        }

        [Authorize, HttpPost]
        public ActionResult Get(string SystemId)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from NotificationURL where SystemId='" + SystemId + "'");

                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpGet, Authorize]
        public ActionResult NotficationURLDelete(string SystemId)
        {
            try
            {
                if (string.IsNullOrEmpty(SystemId))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from  NotificationURL where SystemId='" + SystemId + "'");

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