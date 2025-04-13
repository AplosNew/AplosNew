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

namespace Aplos.Areas.Commercial.Controllers
{
    public class ComplianceController : BaseController
    {
        string TableName = "hkp.ComplianceMaster";
        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public ComplianceController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

       

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + TableName + ") AS TEMP WHERE " + strkey + "";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ItemName='" + data["ItemName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Item Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where   Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Code already exists!!!");



                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";




                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(TableName), out _Id);

                    data["Id"] =  _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new { Error = false,Message = AplosMessage.Insert });

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
                    con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                    con.executeQuery("delete from dbo.ComplianceResponsiblePerson where ComplianceMasterId='" + id + "'");
                    con.CommitTransaction();

                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            

        }

        public ActionResult DeleteRP(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.ComplianceResponsiblePerson where Id='" + id + "'");
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
            //dr["UpdatedBy"] = identity.Name;
            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
            //dr["UpdatedFromIP"] = identity.IPAddress;

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
        public JsonResult CreateRP(List<Dictionary<string, object>> RPDataList,string masterId)
        {
            try
            {
                DataSet dsRP;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.ComplianceResponsiblePerson where ComplianceMasterId='" + masterId + "'", out dsRP, false, "1");

                #region RPDataList 
                if (RPDataList != null)
                {
                    foreach (var item in RPDataList)
                    {
                        DataView dv = new DataView(dsRP.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            AddNewRow(dsRP.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRP);

                return Json(new { Error = false,Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpGet]
        public JsonResult GetRP(string masterId)
        {
            return Json(_sqlRepository.GetDataCollection(@"SELECT RP.Id,RP.EmpSystemID
							    	,E.EmployeeName
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
									,SC.UserName AS Section
                                    ,E.EmployeeCode
									,E.EmpPicPath
                                    ,P.UserName Plant
									,SS.UserName SubSection
							    FROM ComplianceResponsiblePerson RP
                                LEFT JOIN EmployeeInformation E ON E.SystemId=RP.EmpSystemID
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                LEFT JOIN ORG.Company C ON C.Id=E.CompanyId Where ComplianceMasterId='" + masterId + "'"), JsonRequestBehavior.AllowGet);
        }

    }
}