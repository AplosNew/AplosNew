#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.OrderManagement.ShipmentControl;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.OrderManagement.TermsAndConditions;
#endregion Using


namespace Aplos.Areas.OrderManagements.Controllers
{
    public class TermsAndConditionsController  : BaseController
    {
        //abcd
        //this is my code from tarek
        string TableName = "hkp.TermsAndConditions";
        //authentication for
        //GetList Create Delete


        #region Constructor
        TermsAndConditionsService tg = new TermsAndConditionsService();
        private readonly ISqlRepository _sqlRepository;

        public TermsAndConditionsController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor    

        ShipmentControl control = new ShipmentControl();
        public ActionResult Aplos()
        {
            return View();
        }
        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(tg.GetCbo(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = tg.Get(Id);


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            return Json(tg.GetList(column, value), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                string ret = tg.Create(data);
                if (ret == "Success")
                {
                    return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });
                }
                else
                {
                    return Json(new { Error = true, Message = ret });
                }

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

                string ret = tg.Delete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }


        [HttpPost]
        public JsonResult SaveData(List<Dictionary<string, object>> GridData, string titleId)
        {
            try
            {
              
                DataSet dsGrid;

                ConnectionManager.DAL.ConManager conBin = new ConnectionManager.DAL.ConManager("1");
                conBin.OpenDataSetThroughAdapter("select * from dbo.TermsAndConditionsDetails where TermsAndConditionsChildId='" + titleId + "'", out dsGrid, false, "1");

                string DetailId = "";
                int count = 0; 
                foreach (var item in GridData)
                {
                    count++;
                    DataView dv = new DataView(dsGrid.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        if (DetailId == "")
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("dbo.TermsAndConditionsDetails", out DetailId);
                        }
                        DataRow dr = dsGrid.Tables[0].NewRow();

                        item["Id"] = "TD-" + DetailId + "-" + (count + 1);
                        item["TermsAndConditionsChildId"] = titleId;

                        AddNewRow(dsGrid.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                }


                //for (int i = 0; i < GridData.Count; i++)
                //{
                //    dsGrid.Tables[0].DefaultView.RowFilter = "Id='" + GridData[i]["Id"] + @"'";
                //    if (dsGrid.Tables[0].DefaultView.Count > 0)
                //    {
                //        //edit
                //        DataRow dr = dsGrid.Tables[0].DefaultView[0].Row;
                //        dr.BeginEdit();
                //        dr["UserName"] = GridData[i]["UserName"];
                //        dr.EndEdit();
                //    }
                //    else
                //    {
                //        //addnew
                //        if (DetailId == "")
                //        {
                //            bplib.clsGenID genid = new bplib.clsGenID();
                //            genid.GenID("dbo.TermsAndConditionsDetails", out DetailId);
                //        }
                //        DataRow dr = dsGrid.Tables[0].NewRow();

                //        dr["Id"] = "TD-" + DetailId + "-" + (i + 1);
                //        dr["TermsAndConditionsChildId"] = _Id;
                //        //dr["HeaderCaption"] = GridData[i]["Header"];
                //        //dr["Description"] = GridData[i]["Description"];
                //        dsGrid.Tables[0].Rows.Add(dr);
                //        AddNewRow(dsGrid.Tables[0], GridData[i]);

                //    }
                //}


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets( dsGrid);
                //_info.SaveDataSets(dsTitle);

                return Json(new { Error = false,/* Data = TitleData,*/ Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }


        [HttpPost]
        public JsonResult SaveTitle(Dictionary<string, object> TitleData,string TitleId)
        {
            try
            {
                ConnectionManager.DAL.ConManager conTitle = new ConnectionManager.DAL.ConManager("1");
                conTitle.OpenDataSetThroughAdapter("select * from dbo.TermsAndConditionsChild where Id='" + TitleData["Id"] + "'", out DataSet dsTitle, false, "1");
                string _Id = "";

                #region data update
                if (dsTitle.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("dbo.TermsAndConditionsChild", out _Id);
                    _Id = "TC" + _Id;
                    TitleData["Id"] = _Id;
                    AddNewRow(dsTitle.Tables[0], TitleData);
                }
                else
                {
                    _Id = TitleData["Id"].ToString();
                    EditRow(dsTitle.Tables[0].Rows[0], TitleData);
                }
                #endregion data update
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsTitle);
                //_info.SaveDataSets(dsTitle);

                return Json(new { Error = false, Data = TitleData, Message = AplosMessage.Insert });

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

        [HttpGet, Authorize]
        public JsonResult GetPopUp(string TermsAndConditionsDetailId)
        {
            try
            {
                return Json(control.GetTermsAndConditionPopUp(TermsAndConditionsDetailId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        [HttpGet, Authorize]
        public JsonResult GetTitle(string masterID)
        {
            try
            {
                return Json(control.Title(masterID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }



    }
}