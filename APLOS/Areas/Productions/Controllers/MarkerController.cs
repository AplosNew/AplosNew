#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.OrderManagement.Production;
using Library.Crosscutting.Security;
using System.Data;
using Library.Security.Core;
using System.Threading;
using Library.MaterialManagement.Material;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class MarkerController : BaseController
    {
        #region Constructor
        string TableName = "dbo.MarkerMaster";
        string DetailTableName = "dbo.MarkerDetails";
        private readonly ISqlRepository _sqlRepository;
        public MarkerController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFabricWidth()
        {
            return Json(_sqlRepository.GetDataCollection("select Id,UserName FabricWidthName From FabricWidth"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetShrinkageGroup()
        {
            return Json(_sqlRepository.GetDataCollection("select Id,UserName ShrinkageGroupName From ShrinkageGroup"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetShade()
        {
            return Json(_sqlRepository.GetDataCollection("select Id,UserName ShadeName From Shade"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from " + TableName + " where Id = '" + Id + "' ");
                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select m.*,mm.UserName FGMaterialMaster, mma.StandardName FGArticle ,c.UserName HeaderName
                                From MarkerMaster m
                                left join MST.MaterialMaster mm on mm.Id= m.FGMaterialMasterId
                                left join MST.MaterialMasterArticle mma on mma.Id= m.FGArticleId
                                left join HKP.Characteristics c on c.Id= m.CharacteristicsId
                                order by m.Sequence ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetDetailsList(string masterid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT CV.Id AS CharacteristicsValueId,CV.Code, CV.UserName AS [Text] 
                                ,Ratio = case when M.Id is null then '' else M.Ratio end,M.Id
                                FROM MarkerDetails M
                            LEFT JOIN hkp.CharacteristicsValue CV ON CV.Id=M.CharacteristicsValueId
                            Where M.MarkerMasterId='" + masterid + "'  Order by CV.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getCharacteristicsValueByCharacteristicsId(string materialMasterId, string characteristicsId, string valueAssignmentLevel)
        {
            try
            {
                clsMaterial ep = new clsMaterial();
                return Json(ep.GetCharacteristicsValueCboByCharacteristicsId(materialMasterId, characteristicsId, valueAssignmentLevel), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> details)
        {
            try
            {
                DataSet dsMaster;
                DataSet dsChild;
                DataRow dr = null;
                int count = 0;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "' ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same user name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                string MasterId = string.Empty;

                #region Master data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "M" + _Id;
                    MasterId = data["Id"].ToString();
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    MasterId = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                #region Child Data Update

                string DetailsId = string.Empty;
                con.OpenDataSetThroughAdapter("select * from " + DetailTableName + " where MarkerMasterId= '" + MasterId + "' ", out dsChild, false, "1");
                string sID = string.Empty;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[MarkerDetails]", out sID);
                //for (int i = 0; i < details.Count; i++)
                //{
                //    if (!string.IsNullOrEmpty(details[i]["Ratio"].ToString()))
                //    {
                //        if (details[i]["Id"].ToString() == null)
                //        {
                //            dr = dsChild.Tables[0].NewRow();
                //            string sID = string.Empty;
                //            bplib.clsGenID objGenID = new bplib.clsGenID();
                //            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[MarkerDetails]", out sID);
                //            DetailsId = "MD" + sID;
                //            dr["Id"] = DetailsId;
                //            dr["MarkerMasterId"] = MasterId;
                //            dr["CharacteristicsValueId"] = details[i]["CharacteristicsValueId"];
                //            dr["Ratio"] = details[i]["Ratio"];
                //            dr["AddedBy"] = identity.Name;
                //            dr["AddedDate"] = System.DateTime.Now.ToString();
                //            dr["AddedFromIP"] = identity.IPAddress;
                //            dsChild.Tables[0].Rows.Add(dr);
                //        }
                //        else
                //        {
                //            dr = dsChild.Tables[0].DefaultView[0].Row;
                //            dr.BeginEdit();
                //            dr["Ratio"] = details[i]["Ratio"];

                //            dr["UpdatedBy"] = identity.Name;
                //            dr["UpdatedDate"] = DateTime.Now;
                //            dr["UpdatedFromIP"] = identity.IPAddress;
                //            dr.EndEdit();
                //        }

                //    }
                //}

                


                foreach (var item in details)
                {
                    dsChild.Tables[0].DefaultView.RowFilter = "Id = '" + item["Id"] + "' ";
                    if (!string.IsNullOrEmpty(item["Ratio"].ToString()))
                    {
                        if (dsChild.Tables[0].DefaultView.Count == 0)
                        {
                            count++;
                            DetailsId = "MD" + sID + "_" + count;
                            dr = dsChild.Tables[0].NewRow();
                            dr["Id"] = DetailsId;
                            dr["MarkerMasterId"] = MasterId;
                            dr["CharacteristicsValueId"] = item["CharacteristicsValueId"];
                            dr["Ratio"] = item["Ratio"];
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dsChild.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            dr = dsChild.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["Ratio"] = item["Ratio"];

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();
                        }
                    }
                }
                for (int i = 0; i < dsChild.Tables[0].Rows.Count; i++)
                {
                    if (dsChild.Tables[0].Rows[i]["Ratio"].ToString() == "0")
                    {
                        dsChild.Tables[0].Rows[i].Delete();
                    }
                }

                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild);

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });

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
                con.executeQuery("delete from " + DetailTableName + " where MarkerMasterId='" + id + "'");
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
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
            return 1;
        }


    }
}