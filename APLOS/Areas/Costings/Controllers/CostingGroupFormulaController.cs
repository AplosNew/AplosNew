#region Using
using Aplos.Controllers;
using System.Web.Mvc;
using Library.Service.Costings;
using Library.Data.Sql;
using Library.Core;
using System;
using System.Data;
using System.Collections.Generic;
using Library.Crosscutting.Security;
using System.Threading;
using OTSBD;
using Aplos.Properties;
using Library.Service.Enums;
using Aplos.Helpers;

#endregion

namespace Aplos.Areas.Costings.Controllers
{
    public class CostingGroupFormulaController : BaseController
    {

        private string TableName = "CostingGroupFormula";
        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        public CostingGroupFormulaController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
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
            string sql = @"select gf.*, null as CostingTypeDesc, g.UserName as CostingGroup from [dbo].[CostingGroupFormula] gf
                            left join [HKP].[CostingGroup] g on g.Id = gf.CostingGroupId";

            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);

            Dictionary<string, string> CostingType = new Dictionary<string, string>();

            foreach (var item in Enum.GetValues(typeof(CostingType)))
            {
                CostingType.Add(item.ToString(), AccessInfo.GetEnumDescription((CostingType)(int)item));
            }

            for (int i = 0; i < data.Count; i++)
            {
                try
                {
                    data[i]["CostingTypeDesc"] = CostingType[data[i]["CostingType"].ToString()];
                }
                catch (Exception)
                {

                }
            }


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult create(Dictionary<string, object> costingGroupFormula)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("SELECT * FROM "+ TableName + " where Id= '"+ costingGroupFormula["Id"] + "'", out dsMaster, false, "1");

            string _Id = "";

            #region data update
            if (dsMaster.Tables[0].Rows.Count == 0)
            {
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("dbo.CostingGroupFormula", out _Id);

                costingGroupFormula["Id"] = "CGF" + _Id;
                AddNewRow(dsMaster.Tables[0], costingGroupFormula);
            }
            else
            {
                _Id = costingGroupFormula["Id"].ToString();
                EditRow(dsMaster.Tables[0].Rows[0], costingGroupFormula);
            }
            #endregion data update

            // Save to Database 
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsMaster);

            return Json(new { costingGroupFormula = costingGroupFormula, Message = AplosMessage.Updated });

        }
        [HttpGet]
        public ActionResult Delete(string id)
        {
            string sql = @"select * from '" + TableName + "' where Id = '" + id + "'";
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
        #endregion
    }
    public class CostingGroupFormula: BaseModel
    {
        public string  Id { get; set; }
        public string CostingGroupId { get; set; }
        public string CostingType { get; set; }
        public string FormulaId { get; set; }
        public string Formula { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    } 
}