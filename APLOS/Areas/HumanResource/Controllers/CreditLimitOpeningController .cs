#region Using

using Aplos.Controllers;
using Aplos.Properties;
using bplib;
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

namespace Aplos.Areas.HumanResource.Controllers
{
    public class CreditLimitOpeningController : BaseController
    {
        string TableName = "CreditLimitOpening";
     
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public CreditLimitOpeningController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


    
        public ActionResult Aplos()
        {
            return View();
        }       

        [HttpPost, Authorize]
        public ActionResult GetData()
        {
            try {
                string sql = @"select d.UserName as Designation,c.DailyLimit,c.MonthlyLimit,
                d.Id as DesignationId,c.Id as Id,d.ShortName as DesgShortName
                from hkp.Designation d left join creditlimitopening c on d.Id=c.designationId
                where Active=1";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
               
                //con.OpenDataSetThroughAdapter("select * from dbo.OTUpdateConfiguration where GroupID='" +identity.CompanyGroupId + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same Company Group already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                  
                    clsGenID genid = new clsGenID();
                    genid.GenID(TableName, out _Id);


                    data["Id"] = "CLO" + _Id;
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

                return Json(new { Error = false, Data= data, Message = AplosMessage.Updated });

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
            dr["GroupID"] = identity.CompanyGroupId;
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = DateTime.Now.ToString();
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
            dr["GroupID"] = identity.CompanyGroupId;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr.EndEdit();
        }
       
    }
}