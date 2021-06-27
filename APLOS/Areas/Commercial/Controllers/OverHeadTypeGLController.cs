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
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Commercial.Controllers
{
    public class OverHeadTypeGLController : BaseController
    {
        string TableName = "HKP.OverHeadTypeGL";
        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public OverHeadTypeGLController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }



        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        void GetDataSet(string sql, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = sql;
                // strSQL = @"SELECT * FROM [dbo].[MaternityBenefitMaster] WHERE id = '" + pk + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        [HttpGet, Authorize]
        public ActionResult GetList(string coaId,string GLType)
        {

            string sql = "";
            string budgetListSql = "";
            string activityListSql = "";
            

            sql = @"SELECT NULL AS BudgetList, NULL AS ActivityList, LCACTGL.Id, LCACTGL.COAId,LCACTGL.OverHeadTypeId,LCACT.Type
                 ,LCACTGL.ExpensesGLId,LCACTGL.ExpensesActivityId,LCACTGL.Remarks
                ,LCACTGL.AddedBy, LCACTGL.ExpensesBudgetMasterId
                , Active = CAST (CASE WHEN LCACTGL.Id IS NULL THEN 0 ELSE 1 END AS bit)
                ,LCACT.UserName,LCACT.Id AS LACTId ,ExpensesGLInfo= GL.AccountCode + ' - ' + GL.UserName
                FROM HKP.OverHeadType LCACT
                LEFT JOIN HKP.OverHeadTypeGL LCACTGL ON LCACTGL.OverHeadTypeId=LCACT.Id AND LCACTGL.COAId='" + coaId + @"' AND LCACTGL.GLType = '"+GLType+@"'
                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id = LCACTGL.ExpensesBudgetMasterId
                LEFT JOIN [HKP].[Activity] AS A ON A.Id = LCACTGL.ExpensesActivityId
                LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id = LCACTGL.ExpensesGLId";

            budgetListSql = @"select S.* from HKP.OverHeadTypeGL GL
                                    left outer join (

                                    SELECT BM.GLGeneralInfoId, BM.Id AS BudgetMasterId, BM.BudgetId, B.UserName AS BudgetName
                                                            FROM [MST].[BudgetMaster] AS BM
                                                            JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                                            WHERE BM.Archive=0 AND BM.Active=1) AS S on s.GLGeneralInfoId=gl.ExpensesGLId";

            activityListSql = @"select SS.* from HKP.OverHeadTypeGL AS GL
                        left outer join
                        (SELECT BMA.BudgetMasterId, BMA.ActivityId, A.UserName AS ActivityName, A.FALinked
                        FROM [MST].[BudgetMasterActivity] AS BMA
                        JOIN [HKP].[Activity] AS A ON A.Id=BMA.ActivityId
                        WHERE BMA.Active=1) AS SS ON SS.BudgetMasterId = GL.ExpensesBudgetMasterId";

            List<Dictionary<string, object>> MainList = _sqlRepository.GetDataCollection(sql);

            List<Dictionary<string, object>> budgetList = _sqlRepository.GetDataCollection(budgetListSql);
            List<Dictionary<string, object>> ActivityList = _sqlRepository.GetDataCollection(activityListSql);

            for (int i = 0; i < MainList.Count; i++)
            {
                try
                {
                    List<Dictionary<string, object>> k = budgetList.Where(ee => clsStaticInfo.nullrecorder(ee["GLGeneralInfoId"].ToString()) == clsStaticInfo.nullrecorder(MainList[i]["ExpensesGLId"])).ToList();
                    MainList[i]["BudgetList"] = k;

                }
                catch (Exception)
                {

                }

                try
                {
                    List<Dictionary<string, object>> m = ActivityList.Where(ee => clsStaticInfo.nullrecorder(ee["BudgetMasterId"]) == clsStaticInfo.nullrecorder(MainList[i]["ExpensesBudgetMasterId"])).ToList();


                    MainList[i]["ActivityList"] = m;
                }
                catch (Exception)
                {

                }


            }

            return Json(MainList, JsonRequestBehavior.AllowGet);


            //return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }
        private string GetPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OverHeadTypeGL", out idFromDB);
            systemID = "LAGL-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }





        [HttpPost, Authorize]
        public JsonResult Create(IEnumerable<OverHeadTypeGL> dataList, string CAId)
        {

            try
            {
                SaveOverHeadTypeGL(dataList, CAId);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        private void SaveOverHeadTypeGL(IEnumerable<OverHeadTypeGL> dataList, string CAId)
        {
            try
            {
                if (dataList != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    foreach (var item in dataList)
                    {
                            string sql = "SELECT * FROM hkp.OverHeadTypeGL WHERE Id='" + item.Id + "'";

                            objCon = new ConnectionManager.DAL.ConManager("1");
                            objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                            if (dsMaster.Tables[0].Rows.Count == 0 && !string.IsNullOrEmpty(item.ExpensesGLId))
                            {

                                DataRow dr = dsMaster.Tables[0].NewRow();
                                dr["Id"] = GetPK();
                                dr["COAId"] = CAId;
                                dr["GLType"] = item.GLType;
                                dr["OverHeadTypeId"] = item.LACTId;
                                dr["ExpensesGLId"] = item.ExpensesGLId;
                                dr["ExpensesActivityId"] = item.ExpensesActivityId;
                                dr["Remarks"] = item.Remarks;
                                dr["ExpensesBudgetMasterId"] = item.ExpensesBudgetMasterId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dsMaster.Tables[0].Rows.Add(dr);
                             }
                            else if(!string.IsNullOrEmpty(item.ExpensesGLId))
                            {
                                //edit
                                DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();

                                dr["COAId"] = CAId;
                                dr["GLType"] = item.GLType;
                                dr["OverHeadTypeId"] = item.LACTId;
                                dr["ExpensesGLId"] = item.ExpensesGLId;
                                dr["ExpensesActivityId"] = item.ExpensesActivityId;
                                dr["Remarks"] = item.Remarks;
                                dr["ExpensesBudgetMasterId"] = item.ExpensesBudgetMasterId;

                       
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                            }

                            else if (!string.IsNullOrEmpty(item.Id) && string.IsNullOrEmpty(item.ExpensesGLId))
                            {
                                DeleteData(item.Id);
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

        public void DeleteData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM HKP.OverHeadTypeGL  WHERE Id = '" + id + "'";
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
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        public ActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

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
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
    }

    public class OverHeadTypeGL
    {
        public string Id { get; set; }
        public string COAId { get; set; }
        public string OverHeadTypeId { get; set; }
        public string AssetGLId { get; set; }
        public string AssetActivityId { get; set; }

        public string RevenueGLId { get; set; }
        public string RevenueActivityId { get; set; }
        public string LiabilityGLId { get; set; }
        public string LiabilityActivityId { get; set; }

        public string ExpensesGLId { get; set; }
        public string ExpensesActivityId { get; set; }
        public string Remarks { get; set; }
        public string AssetBudgetMasterId { get; set; }
        public string RevenueBudgetMasterId { get; set; }
        public string LiabilityBudgetMasterId { get; set; }
        public string ExpensesBudgetMasterId { get; set; }

        public string UpdatedFromIP { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string AddedFromIP { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedBy { get; set; }

        public string LACTId { get; set; }
        public string GLType { get; set; }
        
    }
}
