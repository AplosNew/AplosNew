#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Security.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion using

namespace Aplos.Areas.Setups.Controllers
{
    public class LabelListController : BaseController
    {
        #region -- Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        //string DTableName = "dbo.LabelList";
        public LabelListController(
            IUnitOfWork U
            , ISqlRepository R
            )
        {
            _unitOfWork = U;
            _sqlRepository = R;
        }

        #endregion -- Constructor

        #region Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region Operation

        [HttpGet, Authorize]
        public ActionResult GetAutoSequence()
        {
            string sql = @"SELECT (ISNULL((MAX(ISNULL(Sequence,0))),0)+1) Sequence FROM [dbo].[LabelList]";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

       

        [HttpGet, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = "";

            sql = @"select  * from (SELECT * from LabelList) AS TEMP WHERE " + strkey + " ORDER BY AddedDate DESC";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (entity != null)
                {

                    DataRow dr;

                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.LabelList WHERE Id='" + entity["Id"] + "'", out dsMaster, false, "1");

                    string _Id = "";
                    string _DId = "";

                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {

                        entity["AddedBy"] = identity.Name;
                        entity["AddedDate"] = System.DateTime.Now.ToString();
                        entity["AddedFromIP"] = identity.IPAddress;
                        AddNewRow(dsMaster.Tables[0], entity);
                    }
                    else
                    {
                       
                        EditRow(dsMaster.Tables[0].Rows[0], entity);
                    }

                    #endregion data update

                    


                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);



                }
                return Json(new { Error = false, Data = entity, Message = AplosMessage.Insert });
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

        [HttpPost]
        public ActionResult Delete(string id)
        {
            DeleteLabelList(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteLabelList(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
               
                strSQL = "DELETE FROM [dbo].[LabelList] WHERE Id = '" + id + "'";

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
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        #endregion -- Operations


    }
}