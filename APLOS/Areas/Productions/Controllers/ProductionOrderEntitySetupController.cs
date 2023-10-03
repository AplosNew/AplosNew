#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using System.Data;
using System.Collections.Generic;
using Library.Crosscutting.Security;
using System.Threading;
using System;
using Library.Security.Core;
using Library.Data.Sql;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionOrderEntitySetupController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        public ProductionOrderEntitySetupController(ISqlRepository R)
        {
            _sqlRepository = R;

        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        [HttpPost]
        public ActionResult GetList(string column, string value, string CompanyId, string PlantId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT S.*,PE.UserName ProductionEntity,FE.UserName FromEntity,DRG.UserName DrGLGeneralInfoName,DRB.UserName DrBudget,DRA.UserName DrActivity
,CRG.UserName CrGLGeneralInfoName,CRB.UserName CrBudget,CRA.UserName CrActivity
FROM [dbo].[ProductionOrderEntitySetup] S
LEFT JOIN ORG.Entity PE ON PE.Id=S.ProductionEntityId
LEFT JOIN ORG.Entity FE ON FE.Id=S.FromEntityId
LEFT JOIN [HKP].[GLGeneralInfo] DRG ON DRG.Id=S.DrGLGeneralInfoId
LEFT JOIN [MST].[BudgetMaster] DRBM ON DRBM.Id=S.DrBudgetMasterId
LEFT JOIN [HKP].[Budget] DRB ON DRB.Id=DRBM.BudgetId
LEFT JOIN [HKP].[Activity] DRA ON DRA.Id=S.DrActivityId
LEFT JOIN [HKP].[GLGeneralInfo] CRG ON CRG.Id=S.CrGLGeneralInfoId
LEFT JOIN [MST].[BudgetMaster] CRBM ON CRBM.Id=S.CrBudgetMasterId
LEFT JOIN [HKP].[Budget] CRB ON CRB.Id=CRBM.BudgetId
LEFT JOIN [HKP].[Activity] CRA ON CRA.Id=S.CrActivityId
Where S.CompanyId='" + CompanyId + @"' AND S.PlantId='"+ PlantId + @"') AS TEMP WHERE " + strkey + "";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster,dsDrMaster, dsCrMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("Select  Id  from MST.BudgetMasterActivity Where BudgetMasterId='"+ data["DrBudgetMasterId"].ToString() + "' AND ActivityId='"+ data["DrActivityId"].ToString() + "'", out dsDrMaster, false, "1");

                con.OpenDataSetThroughAdapter("Select  Id  from MST.BudgetMasterActivity Where BudgetMasterId='" + data["DrBudgetMasterId"].ToString() + "' AND ActivityId='" + data["DrActivityId"].ToString() + "'", out dsCrMaster, false, "1");


                con.OpenDataSetThroughAdapter("select * from dbo.ProductionOrderEntitySetup where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("ProductionOrderEntitySetup", out _Id);

                    data["Id"] = _Id;
                    data["DrControlId"] = dsDrMaster.Tables[0].Rows[0]["Id"].ToString();
                    data["CrControlId"] = dsCrMaster.Tables[0].Rows[0]["Id"].ToString();
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["DrControlId"] = dsDrMaster.Tables[0].Rows[0]["Id"].ToString();
                    data["CrControlId"] = dsCrMaster.Tables[0].Rows[0]["Id"].ToString();
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

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.ProductionOrderEntitySetup where Id='" + id + "'");
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

    }
}