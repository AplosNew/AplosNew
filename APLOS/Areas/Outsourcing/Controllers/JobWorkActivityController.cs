using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Security.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Outsourcing.Controllers
{
    public class JobWorkActivityController : BaseController
    {
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        public JobWorkActivityController(SqlRepository Repository)
        {
            _sqlRepository = Repository;
        }
        #endregion
        #region Pages
        // GET: IE/JobWorkActivity
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        #region Code Master Data
        [HttpGet, Authorize]
        public JsonResult GetAllData()
        {
            string sql = "";
            sql = @"SELECT A.Id,A.Code,CAST(A.Sequence AS DECIMAL(18,2))Sequence,A.ShortName,A.StandardName,A.UserName,A.Type,A.IsActive,A.ResponsiblePersonId,E.EmployeeName ResponsiblePersonName,A.Remarks 
                    FROM HKP.JobWorkActivity A
                    LEFT JOIN [dbo].[EmployeeInformation] E ON E.SystemId = A.ResponsiblePersonId
                    ORDER BY UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllJobWorkItem()
        {
            string sql = "";
            sql = @"SELECT I.Id,I.Code,I.ShortName,I.StandardName,I.UserName,U.UserName UOM, E.EmployeeName ResponsiblePerson
                    FROM HKp.JobWorkItem I
                    INNER JOIN SCS.UnitOfMeasurement U ON U.Id = I.UOMId
                    INNER JOIN dbo.EmployeeInformation E ON E.SystemId = I.ResponsiblePersonId
                    WHERE I.IsActive = 1
                    ORDER BY I.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetSelectedData(string Id)
        {
            string sql = "";
            sql = @"SELECT A.Id,A.Code,A.ShortName,A.Sequence,A.StandardName,A.UserName,A.Type,A.IsActive,A.ResponsiblePersonId,E.EmployeeName ResponsiblePersonName,A.Remarks,ISNULL(IE.Total,0)Total
                    FROM HKP.JobWorkActivity A
                    LEFT JOIN [dbo].[EmployeeInformation] E ON E.SystemId = A.ResponsiblePersonId
					LEFT JOIN (SELECT JobWorkActivityId,COUNT(*) Total 
					FROM MST.JobWorkValueAddedMaster GROUP BY JobWorkActivityId) IE ON IE.JobWorkActivityId = A.Id
                    WHERE A.Id='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDataToDisable(string JWActivityId, string Type)
        {
            string sql = "";
            if (Type == "Value Added")
            {
                sql = @"select * from MST.JobWorkValueAddedMaster where JobWorkActivityId='"+ JWActivityId + @"' ";
            }

            if (Type == "Transformation")
            {
                sql = @"select * from MST.JobWorkTransformationMaster where JobWorkActivityId='"+ JWActivityId + @"' ";
            }


            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        // Delete
        [HttpGet, Authorize]
        public ActionResult DeleteSelectedData(string Id)
        {
            try
            {
                DataSet dsMaster;
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (!string.IsNullOrEmpty(Id))
                {
                    con.OpenDataSetThroughAdapter("select * from HKP.JobWorkActivityChild where JobWorkActivityId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Job Work Item data");
                    }
                }

                con.BeginTransaction();
                con.executeQuery("DELETE FROM [HKP].[JobWorkActivity] WHERE Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkActivity", out sID);
            return sID;
        }


        [HttpPost, Authorize]
        public ActionResult SaveData(Dictionary<string, object> saveData)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

            try
            {
                DataSet dsDuplicate = new DataSet();

                if(saveData["Id"]==null)
                {
                    con.getDataSet("SELECT * FROM [HKP].[JobWorkActivity] WHERE UserName = '" + saveData["UserName"].ToString() + "'", out dsDuplicate);
                    if (dsDuplicate.Tables[0].Rows.Count > 0)
                        return Json(new { Error = true, Message = "User Name Already Exists..!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    con.getDataSet("SELECT * FROM [HKP].[JobWorkActivity] WHERE  UserName = '" + saveData["UserName"].ToString() + "' AND Id NOT IN ('" + saveData["Id"] + "')", out dsDuplicate);
                    if (dsDuplicate.Tables[0].Rows.Count > 0)
                        return Json(new { Error = true, Message = "User Name Already Exists..!" }, JsonRequestBehavior.AllowGet);
                }

                DataTable dtChild = new DataTable();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID _genId = new bplib.clsGenID();
                string _Message = "";
                string Id = "";
                con.getDataSet("SELECT * FROM [HKP].[JobWorkActivity] WHERE Id='" + saveData["Id"] + "'", out DataSet dsOut);
                if (dsOut.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsOut.Tables[0].NewRow();
                    _genId.GenID("[HKP].[JobWorkActivity]", out Id);
                 //   Id = "JA" + Id;
                    dr["Id"] = "JA" + GetPK();
                    dr["Code"] = saveData["Code"].ToString();
                    dr["Sequence"] = saveData["Sequence"].ToString();
                    dr["ShortName"] = saveData["ShortName"].ToString();
                    dr["StandardName"] = saveData["StandardName"].ToString();
                    dr["UserName"] = saveData["UserName"].ToString();
                    dr["Type"] = saveData["Type"].ToString();
                    dr["IsOutsource"] = saveData["IsOutsource"].ToString();
                    dr["IsJobWork"] = saveData["IsJobWork"].ToString();
                    dr["IsActive"] = saveData["IsActive"].ToString();
                    dr["ResponsiblePersonId"] = saveData["ResponsiblePersonId"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsOut.Tables[0].Rows.Add(dr);

                    _Message = "Data Save Successfully..!";

                }
                else
                {
                    DataRow dr = dsOut.Tables[0].Rows[0];
                    Id = dr["Id"].ToString();
                    dr.BeginEdit();
                    dr["Sequence"] = saveData["Sequence"].ToString();
                    dr["Code"] = saveData["Code"].ToString();
                    dr["ShortName"] = saveData["ShortName"].ToString();
                    dr["StandardName"] = saveData["StandardName"].ToString();
                    dr["UserName"] = saveData["UserName"].ToString();
                    dr["Type"] = saveData["Type"].ToString();
                    dr["IsActive"] = saveData["IsActive"].ToString();
                    dr["ResponsiblePersonId"] = saveData["ResponsiblePersonId"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                    _Message = "Data Updated Successfully..!";
                }

                saveData["Id"] = dsOut.Tables[0].Rows[0]["Id"].ToString();
                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsOut);

                return Json(new { Error = false, Message = _Message, Data = saveData}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Code Child Data

        [HttpGet]
        public JsonResult DelActivityChild(string Id)
        {
            try
            {
                DataSet dsMaster;
                DataSet CheckValMst;
                DataSet CheckTransMst;
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                // ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                if (!string.IsNullOrEmpty(Id))
                {
                    con.OpenDataSetThroughAdapter("select * from HKP.JobWorkActivityChild where Id='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        string JWActivityId = Convert.ToString(dsMaster.Tables[0].Rows[0]["JobWorkActivityId"]);
                        string JWItemId = Convert.ToString(dsMaster.Tables[0].Rows[0]["JobWorkItemId"]);

                        con.OpenDataSetThroughAdapter("select * from MST.JobWorkValueAddedMaster where JobWorkActivityId='"+ JWActivityId + @"' and JobWorkActivityChildId='"+ JWItemId + @"' ", out CheckValMst, false, "1");
                        if (CheckValMst.Tables[0].Rows.Count > 0)
                        {
                            throw new Exception("This Job Work Item cannot be deleted.");
                        }

                        con.OpenDataSetThroughAdapter("select * from MST.JobWorkTransformationMaster where JobWorkActivityId='" + JWActivityId + @"' and JobWorkActivityChildId='" + JWItemId + @"' ", out CheckTransMst, false, "1");
                        if (CheckTransMst.Tables[0].Rows.Count > 0)
                        {
                            throw new Exception("This Job Work Item cannot be deleted.");
                        }

                    }
                }

                con.BeginTransaction();
                con.executeQuery("DELETE FROM [HKP].[JobWorkActivityChild] WHERE Id='" + Id + "' ");

                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult LoadJobItemsForSelection(string MasterId)
        {
            string sql = "";
            sql = @"select jwi.*,emp.EmployeeName as ResponsiblePerson, mm.UserName as MaterialMaster
                                   ,UOM=case when jwi.MaterialMasterId is not null then mmuom.UserName else uom.UserName End
                                   from HKP.JobWorkItem jwi left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
								   left join dbo.EmployeeInformation emp on emp.SystemId=jwi.ResponsiblePersonId
								   left join MST.MaterialMaster mm on mm.Id=jwi.MaterialMasterId
								   left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                                   WHERE isnull(jwi.Id,'') not in (select isnull(JobWorkItemId,'') from HKP.JobWorkActivityChild where JobWorkActivityId='" + MasterId + @"')
                                   ORDER BY jwi.Code";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult LoadAllSelectedJobActivtiyTab(string JobWorkActivityMasterId)
        {
            string sql = "";
            sql = @"select jac.*,ja.UserName as JobActivity, jwi.UserName as JobWorkItem,emp.EmployeeName as ResponsiblePerson, mm.UserName as MaterialMaster
                                        ,UOM=case when jwi.MaterialMasterId is not null then mmuom.UserName else uom.UserName End
										from HKP.JobWorkActivityChild jac left join HKP.JobWorkActivity ja on ja.Id=jac.JobWorkActivityId
										left join HKP.JobWorkItem jwi on jwi.Id=jac.JobWorkItemId
										left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
										left join dbo.EmployeeInformation emp on emp.SystemId=jwi.ResponsiblePersonId
							        	left join MST.MaterialMaster mm on mm.Id=jwi.MaterialMasterId
										left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
										where jac.JobWorkActivityId='" + JobWorkActivityMasterId + @"' ORDER BY jwi.Code ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        private string GetPKC()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkActivityChild", out sID);
            return sID;
        }


        [HttpPost, Authorize]
        public ActionResult SaveJobActivityChildTab(string JobActivityMasterId, List<Dictionary<string, object>> JobItemTabData)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsData;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

                con.getDataSet("select * from HKP.JobWorkActivityChild where 1=2", out dsData);

                string id = "";
                for (int i = 0; i < JobItemTabData.Count; i++)
                {
                    if (id == "")
                    {
                        bplib.clsGenID _gen = new bplib.clsGenID();
                        _gen.GenID("JobWorkActivityChild", out id);
                    }

                    DataRow dr = dsData.Tables[0].NewRow();
                    dr["Id"] = "JAC" + GetPKC();
                    dr["JobWorkActivityId"] = JobActivityMasterId;
                    dr["JobWorkItemId"] = JobItemTabData[i]["Id"].ToString();
    
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsData.Tables[0].Rows.Add(dr);

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsData);

                return Json(new { Error = false, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion

        [HttpGet, Authorize]
        public JsonResult GetSequenceNumber()
        {
            string sql = "";
            sql = @"SELECT isnull(Max(Sequence),0) + 1 AS Sequence FROM[HKP].[JobWorkActivity]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
    }
}