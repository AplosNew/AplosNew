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

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class TaskDependentDatesController : BaseController
    {
        //authentication for
        //GetList Create


        #region Constructor
        string TableName = "HKP.TaskDependentDates";
        private readonly ISqlRepository _sqlRepository;
        public TaskDependentDatesController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        
        public ActionResult Aplos()
        {
            return View();
        }


        [HttpPost,Authorize]
        public ActionResult GetList()
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM " + TableName;
            DataTable dt = _sqlRepository.GetDataTable(sql);
            foreach (DependentDatesEnum _data in Enum.GetValues(typeof(DependentDatesEnum)))
            {
                dt.DefaultView.RowFilter = "DependentDatesEnum='" + _data.ToString() + "'";
                if (dt.DefaultView.Count == 0)
                {
                    DataRow dr = dt.NewRow();
                    dr["DependentDatesEnum"] = _data.ToString();
                    dt.Rows.Add(dr);

                }
               
            }
            dt.DefaultView.RowFilter = null;
            dt.DefaultView.Sort = "DependentDatesEnum ASC";

            dt = dt.DefaultView.ToTable();

            return Json(Helpers.CustomJsonResult.DataTableToJson(dt), JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult Create(List<Dictionary<string, object>> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName, out dsMaster, false, "1");

                string _Id = "";




                #region data update

                bplib.clsGenID genid;
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i]["UserName"] != null)
                    {



                        dsMaster.Tables[0].DefaultView.RowFilter = "DependentDatesEnum='" + data[i]["DependentDatesEnum"].ToString() + "'";
                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            if (_Id == "")
                            {
                                genid = new bplib.clsGenID();
                                genid.GenID(TableName, out _Id);
                            }
                            data[i]["Id"] = "A" + _Id + (i + 1).ToString();
                            AddNewRow(dsMaster.Tables[0], data[i]);
                        }
                        else
                        {

                            EditRow(dsMaster.Tables[0].DefaultView[0].Row, data[i]);
                        }
                    }

                }

                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new { Error = false, Message = AplosMessage.Updated });

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
    }
}