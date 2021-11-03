#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;
using System;
using OTSBD;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class CostingTypesController : BaseController
    {
        #region Constructor
        /// <summary>   The CostingTypesService service. </summary>
        private readonly ISqlRepository _sqlRepository;
        public CostingTypesController(ISqlRepository R)
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

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT CostingType AS [Value], UserName AS [Text] FROM [dbo].[CostingTypes]"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT * FROM [dbo].[CostingTypes]"), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(CostingTypes costingTypes)
        {
            SaveData(costingTypes);
            return Json(new {Message = AplosMessage.Insert });
        }

        private void SaveData(CostingTypes data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                DataSet dsMaster;
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[CostingTypes] WHERE CostingType='" + data.CostingType + "' AND  Id<>'" + data.Id + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Costing Type already exists!!!");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[CostingTypes] WHERE UserName='" + data.UserName + "' AND  Id<>'" + data.Id + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same UserName already exists!!!");
                string sql = "SELECT * FROM [dbo].[CostingTypes] WHERE Id='" + data.Id + "'";
                
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = GetPK();
                    dr["CostingType"] = data.CostingType;
                    dr["UserName"] = data.UserName;

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

                    dr["CostingType"] = data.CostingType;
                    dr["UserName"] = data.UserName;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();

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

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(CostingTypes), out sID);
            return sID;
        }

        public ActionResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.CostingTypes WHERE Id = '" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function
        #endregion
    }

   public class CostingTypes: BaseController
    {
        public string Id { get; set; }
        public string CostingType { get; set; }
        public string UserName { get; set; }
        [NeverUpdate]
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime AddedDate { get; set; }
        [NeverUpdate]
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
}