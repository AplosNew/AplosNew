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
using System.Collections.Generic;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class EfficiencySlabController : BaseController
    {
        #region Constructor
        /// <summary>   The CostingTypesService service. </summary>
        private readonly ISqlRepository _sqlRepository;
        public EfficiencySlabController(ISqlRepository R)
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
        public JsonResult Save(List<Dictionary<string, object>> data,string PlantId)
        {
          
            try
            {
               
                string _id = "";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from  SPTEfficiencySlab where PlantId = '" + PlantId + "'", out dsMaster, false, "1");

                while (dsMaster.Tables[0].DefaultView.Count>0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }
                    string _Id = "";

                #region data update

                for (int i = 0; i < data.Count; i++)
                {

                    if(dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        if(_id == "")
                        {
                            bplib.clsGenID id = new bplib.clsGenID();
                            id.GenID("SPTEfficiencySlab", out _id);
                            _id = "SE" + _id;
                        }
                    }
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = _id + "-" + (i + 1).ToString();
                    dr["Minimum"] =clsStaticInfo.dbl( data[i]["Minimum"]);
                    dr["Maximum"] = clsStaticInfo.dbl(data[i]["Maximum"]);

                    dr["FirstDayEfficiency"] = clsStaticInfo.dbl(data[i]["FirstDayEfficiency"]);
                    dr["Increment"] = clsStaticInfo.dbl(data[i]["Increment"]);
                    dr["LastDayEfficiency"] = clsStaticInfo.dbl(data[i]["LastDayEfficiency"]);
                    dr["PlantId"] = PlantId;

                    dsMaster.Tables[0].Rows.Add(dr);
                }

                
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false,  Message = AplosMessage.Updated });

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
        [HttpPost, Authorize]
        public ActionResult GetPlantList()
        {

            try
            {

                string sql = PlantListSql();
                return Json(new { DATA = _sqlRepository.GetDataCollection(sql), Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {   
                throw ex;
            }

        }
        private string PlantListSql()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return @" 
                  	select P.Id,P.UserName as PlantName from ORG.Plant as P where P.CompanyId='"+ identity.CompanyId +@"' AND P.Active=1
              ";

        }


        [HttpPost, Authorize]
        public ActionResult GetCompanyList()
        {

            try
            {
                string sql = CompanyListSql();
                return Json(new { DATA = _sqlRepository.GetDataCollection(sql), Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private string CompanyListSql()
        {
            return @" 
                     select C.Id,c.UserName as CompanyName from ORG.Company as C where Active=1
              ";

        }
        public ActionResult LoadEfficiencySlab(string PlantId)
        {
            try
            {
                string sql = EfficiencySlabSql(PlantId);
                return Json(new { DATA = _sqlRepository.GetDataCollection(sql), Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private string EfficiencySlabSql(string PlantId)
        {
            return @" 
                  select * from SPTEfficiencySlab where plantid='" + PlantId + @"' order by Minimum,Maximum
              ";

        }

        public ActionResult Delete(string id)
        {
            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from  SPTEfficiencySlab  where PlantId='" + id + "'");
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