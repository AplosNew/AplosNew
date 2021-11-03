#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Model.Costings;
using Library.Service.Costings;
using System.Collections.Generic;
using System.Threading;
using Library.Crosscutting.Security;
using System.Data;
using System;
using OTSBD;
using Library.Data.Sql;

#endregion

namespace Aplos.Areas.Costings.Controllers
{
    public class CostingTypeComponentController : BaseController
    {
        #region Constructor
    
        private readonly ISqlRepository _sqlRepository;

        public CostingTypeComponentController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
       
        [HttpGet]
        public ActionResult GetCostingComponent(string costingType)
        {
            string sql = @"SELECT cc.Code, cc.ShortName, cc.StandardName,cc.CostingSegment, cc.UserName,ctc.*
                        FROM [dbo].[CostingTypeComponent] as ctc
                        LEFT JOIN [HKP].[CostingComponent] as cc  on ctc.CostingComponentId= cc.Id 
                        WHERE ctc.CostingType = '"+ costingType + "' ORDER BY ctc.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [dbo].[CostingTypeComponent] WHERE Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true,Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CostingTypeComponent> data)
        {
            SaveData(data);
            return Json(new {Message = AplosMessage.Success });
        }
        private void SaveData(IEnumerable<CostingTypeComponent> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "select * from [dbo].[CostingTypeComponent] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = GetChargesPK();
                            dr["Sequence"] = item.Sequence;
                            dr["CostingComponentId"] = item.CostingComponentId;
                            dr["CostingType"] = item.CostingType;
                            

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["Sequence"] = item.Sequence;
                            dr["CostingComponentId"] = item.CostingComponentId;
                            dr["CostingType"] = item.CostingType;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private string GetChargesPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(CostingTypeComponent), out sID);
            return sID;
        }
       
       
        #endregion
    }
}