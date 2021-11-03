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

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialSettingController : BaseController
    {
        string TableName = "dbo.MaterialSetting";
        
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public MaterialSettingController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"
                            Select MS.*,MMT.UserName Type from dbo.MaterialSetting MS
                            LEFT JOIN [HKP].[MaterialMasterType] MMT ON MMT.Id=MS.MaterialMasterTypeId";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MaterialSetting entity)
        {
            try
            {
                SaveData(entity);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(MaterialSetting), out sID);
            return sID;
        }

        private void SaveData(MaterialSetting data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string headType = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                DataSet dsMaster;
                objCon = new ConnectionManager.DAL.ConManager("1");

                objCon.OpenDataSetThroughAdapter("select * from [dbo].[MaterialSetting] where MaterialMasterTypeId='" + data.MaterialMasterTypeId + "' AND TypeValue='" + data.TypeValue + "' AND  Id<>'" + data.Id + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Combination already exists!!!");

                string sql = "SELECT * FROM [dbo].[MaterialSetting] WHERE Id='" + data.Id + "'";
               
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = GetPK();
                    dr["MaterialMasterTypeId"] = data.MaterialMasterTypeId;
                    dr["TypeValue"] = data.TypeValue;
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

                    dr["MaterialMasterTypeId"] = data.MaterialMasterTypeId;
                    dr["TypeValue"] = data.TypeValue;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        public ActionResult Delete(string id)
        {
            string sql = @"select * from dbo.MaterialSetting where Id = '" + id + "'";
            
                try
                {

                    if (string.IsNullOrEmpty(id))
                        throw new Exception("Select entry first");

                    ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
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

      
        public class MaterialSetting:BaseModel
        {
            public string Id { get; set; }
            public string MaterialMasterTypeId { get; set; }
            public string TypeValue { get; set; }
            public string AddedBy { get; set; }
            public DateTime AddedDate { get; set; }
            public string AddedFromIP { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }
        }
    }
}