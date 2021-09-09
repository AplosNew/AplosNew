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
                string sql = @"select d.UserName as Designation,ISNULL(c.DailyLimit,'0') 
                as DailyLimit,
                ISNULL(c.DailyLimit,'0') as OriginalDayLimit,
                ISNULL(c.MonthlyLimit,'0') as MonthlyLimit,ISNULL(c.MonthlyLimit,'0') as OriginalMonthlyLimit,
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
        public JsonResult Create(List<CreditLimitModel> data)
        {
            try
            {
                if (data == null)
                    throw new Exception("No New Data has been updated");

                string AllDesgination = "";

                foreach (CreditLimitModel item in data)
                {

                    if (AllDesgination == "")
                    {
                        AllDesgination = "'" + item.DesignationId + "'";
                    }
                    else
                    {
                        AllDesgination += ",'" + item.DesignationId + "'";
                    }
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
               
               con.OpenDataSetThroughAdapter("select * from " + TableName + " where DesignationId In(" + AllDesgination + ")", out dsMaster, false, "1");

                string _Id = "";
                foreach (CreditLimitModel item in data)
                {

                    dsMaster.Tables[0].DefaultView.RowFilter = @"DesignationId='" + item.DesignationId + "'";

                    #region data update
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        clsGenID genid = new clsGenID();
                        genid.GenID(TableName, out _Id);
                        
                        dr["Id"] = "CLO" + _Id;
                        dr["GroupID"] = identity.CompanyGroupId;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["DesignationId"] = item.DesignationId;
                        dr["DailyLimit"] = item.DailyLimit;
                        dr["MonthlyLimit"] = item.MonthlyLimit;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                     
                        dr.BeginEdit();
                        dr["DesignationId"] = item.DesignationId;
                        dr["DailyLimit"] = item.DailyLimit;
                        dr["MonthlyLimit"] = item.MonthlyLimit;
                        dr["GroupID"] = identity.CompanyGroupId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr.EndEdit();
                    }
                    #endregion data update

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data= data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }   
               
    }

    public class CreditLimitModel
    {
        public string Id { get; set; }
        public string DailyLimit { get; set; }
        public string MonthlyLimit { get; set; }
        public string DesignationId { get; set; }
        public string AddedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}