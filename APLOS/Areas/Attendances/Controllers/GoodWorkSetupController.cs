#region Using

using Aplos.Controllers;
using Aplos.MaterialManagement.MaterialQuery;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.General.Commercial;
using Library.HumanResource.Payroll.Allowance;
using Library.Model.Productions;
using Library.Model.Setups;
using Library.OrderManagement.Sales;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Attendances.Controllers
{
    public class GoodWorkSetupController : BaseController
    {
        #region Constructor
        private readonly IAuthorizationConfigService _authorizationConfigService;
        private readonly ISqlRepository _sqlRepository;
        clsContract clsCon = new clsContract();
        public GoodWorkSetupController(IAuthorizationConfigService authorizationConfigService, ISqlRepository R)
        {
            _authorizationConfigService = authorizationConfigService;
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            string sql = @"select * from (SELECT G.*,E.EmployeeName ResponsiblePerson,E.EmployeeCode ResponsiblePersonCode from  [dbo].[GoodWorkSetup] G
LEFT JOIN dbo.EmployeeInformation E on E.SystemId=G.ResponsiblePersonId) AS TEMP WHERE " + strkey + "";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [dbo].[GoodWorkSetup] where UserCode='" + data["UserCode"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from [dbo].[GoodWorkSetup] where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from [dbo].[GoodWorkSetup] where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("GoodWorkSetup", out _Id);

                    data["Id"] = _Id;
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

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }



        [HttpPost, Authorize]
        public JsonResult CreateEntity(List<Dictionary<string, object>> data, string goodWorkSetupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            bplib.clsGenID genid = new bplib.clsGenID();
            DataSet dsBC;
            string _Id = string.Empty;
            int c = 0;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.GoodWorkEntitySetup where  GoodWorkSetUpId='" + goodWorkSetupId + "'", out dsBC, false, "1");
                if (data != null)
                {
                    genid.GenID("GoodWorkEntitySetup", out _Id);
                    foreach (var item in data)
                    {
                        c++;
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = goodWorkSetupId + "-" + _Id + "-" + c;
                            item["GoodWorkSetUpId"] = goodWorkSetupId;

                            AddNewRow(dsBC.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsBC);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [Authorize, HttpPost]
        public ActionResult EntityDelete(string Id)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sqlr = @"select * from GoodWorkEntitySetup where Id = '" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");

                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from GoodWorkEntitySetup where Id ='" + Id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetGoodWorkEntitySetupData(string goodWorkSetupId)
        {
            try
            {
                return Json(clsCon.GetGoodWorkEntitySetupData(goodWorkSetupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
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
                con.executeQuery("delete from [dbo].[GoodWorkSetup] where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateBudgetCode(List<Dictionary<string, object>> data, string goodWorkSetupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            bplib.clsGenID genid = new bplib.clsGenID();
            DataSet dsBC;
            string _Id = string.Empty;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.GoodWorkBudgetSetup where  GoodWorkSetupId='" + goodWorkSetupId + "'", out dsBC, false, "1");
                if (data != null)
                {
                    int idcount = 0;
                    genid.GenID("GoodWorkBudgetSetup", out _Id);
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "BudgetId='" + item["BudgetId"] + "'";
                        idcount++;
                        if (dv.Count == 0)
                        {
                            item["Id"] = goodWorkSetupId + "-" + _Id + "-" + idcount;
                            item["GoodWorkSetupId"] = goodWorkSetupId;

                            AddNewRow(dsBC.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsBC);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetGoodWorkBudgetCodeSetupData(string goodWorkSetupId)
        {
            try
            {
                return Json(clsCon.GetGoodWorkBudgetCodeSetupData(goodWorkSetupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpPost]
        public ActionResult BudgetCodeDelete(string Id)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sqlr = @"select * from GoodWorkBudgetSetUp where BudgetId in (" + Id + @")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");

                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from GoodWorkBudgetSetUp where BudgetId in (" + Id + ")");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetAllEmployeeData()
        {
            JsonResult json = Json(_authorizationConfigService.GetAllEmployeeData(), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetList(string actionStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_authorizationConfigService.Query(identity.CompanyId, identity.PlantId, actionStatus), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateAuthority(List<Dictionary<string, object>> data, string goodWorkSetupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            bplib.clsGenID genid = new bplib.clsGenID();
            DataSet dsAuthority;
            string _Id = string.Empty;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.GoodWorkAuthoritySetup where  GoodWorkSetupId='" + goodWorkSetupId + "'", out dsAuthority, false, "1");
                if (data != null)
                {
                    genid.GenID("GoodWorkAuthoritySetup", out _Id);
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsAuthority.Tables[0]);
                        dv.RowFilter = "AuthorityId='" + item["AuthorityId"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = goodWorkSetupId + "-" + _Id;
                            item["GoodWorkSetupId"] = goodWorkSetupId;

                            AddNewRow(dsAuthority.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsAuthority);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetGoodWorkAuthorityData(string goodWorkSetupId)
        {
            try
            {
                return Json(clsCon.GetGoodWorkAuthorityData(goodWorkSetupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpPost]
        public ActionResult AuthorityDelete(string Id)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sqlr = @"select * from GoodWorkAuthoritySetUp where Id = '" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");

                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from GoodWorkAuthoritySetUp where Id ='" + Id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateCheckBy(List<Dictionary<string, object>> data, string goodWorkSetupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            bplib.clsGenID genid = new bplib.clsGenID();
            DataSet dsCheckBy;
            string _Id = string.Empty;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.GoodWorkCheckBySetUp where  GoodWorkSetupId='" + goodWorkSetupId + "'", out dsCheckBy, false, "1");
                if (data != null)
                {
                    genid.GenID("GoodWorkCheckBySetUp", out _Id);
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsCheckBy.Tables[0]);
                        dv.RowFilter = "CheckById='" + item["CheckById"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = goodWorkSetupId + "-" + _Id;
                            item["GoodWorkSetupId"] = goodWorkSetupId;

                            AddNewRow(dsCheckBy.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsCheckBy);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetCheckByData(string goodWorkSetupId)
        {
            try
            {
                return Json(clsCon.GetGoodWorkCheckByData(goodWorkSetupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpPost]
        public ActionResult CheckByDelete(string Id)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sqlr = @"select * from GoodWorkCheckBySetUp where Id = '" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");

                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from GoodWorkCheckBySetUp where Id ='" + Id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
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

        [HttpGet, Authorize]
        public JsonResult GetBudgetedEmployeeData(string gwsId)
        {
            try
            {
                string CmdText = @"SELECT BE.Id,CAST (CASE WHEN BE.EmployeeId IS NULL THEN 1 ELSE 0 END AS bit) BEFlag,E.SystemId EmployeeId						    	
							    	,E.EmployeeName
							    	,PMB.Code BudgetCode
							    	,PR.UserName PositionName
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
                                    ,E.EmployeeCode
                                    ,E.DOJ
                                    ,P.UserName Plant
									,SS.UserName SubSection
                                    ,E.EmployeeCodeNumeric
                                    ,C.UserName Company
                                    ,EC.UserName EmployeeCategory
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON E.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON E.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN MST.DesignationMaster DM on DM.DesignationId=E.GivenDesignationId
						        LEFT JOIN HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
                                LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
								OUTER APPLY (Select * from dbo.ExceptionGoodWorkEmployee Where GoodWorkSetUpId='" + gwsId + @"' AND  EmployeeId=E.SystemId)BE 
                                WHERE E.EmployeeStatus='Active' AND E.EmpType<>'Guest' 
								 AND E.BudgetCode IN (SELECT BudgetId FROM dbo.GoodWorkBudgetSetup Where GoodWorkSetUpId='" + gwsId + @"')
								--order by EmployeeCodeNumeric
UNION
SELECT BE.Id,CAST (CASE WHEN BE.EmployeeId IS NULL THEN 1 ELSE 0 END AS bit) BEFlag,E.SystemId EmployeeId						    	
							    	,E.EmployeeName
							    	,PMB.Code BudgetCode
							    	,PR.UserName PositionName
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
                                    ,E.EmployeeCode
                                    ,E.DOJ
                                    ,P.UserName Plant
									,SS.UserName SubSection
                                    ,E.EmployeeCodeNumeric
                                    ,C.UserName Company
                                    ,EC.UserName EmployeeCategory
							    FROM  dbo.ExceptionGoodWorkEmployee  BE
								LEFT JOIN EmployeeInformation E ON E.SystemId=BE.EmployeeId
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON E.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON E.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN MST.DesignationMaster DM on DM.DesignationId=E.GivenDesignationId
						        LEFT JOIN HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
                                LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
								 Where GoodWorkSetUpId='" + gwsId + @"'";
                return Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateExceptionBudgetedEmployee(List<Dictionary<string, object>> data, string goodWorkSetupId)
        {
            MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            bplib.clsGenID genid = new bplib.clsGenID();
            DataSet dsBC, dsDD;
            string _Id = string.Empty;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.ExceptionGoodWorkEmployee where  GoodWorkSetupId='" + goodWorkSetupId + "'", out dsBC, false, "1");
                //objCon.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[ExceptionGoodWorkEmployee] where GoodWorkSetupId='" + goodWorkSetupId + "'", out dsDD, false, "1");
                //int ccount = Convert.ToInt32(dsDD.Tables[0].Rows[0]["countId"].ToString());
                if (data != null)
                {
                   

                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0 && Convert.ToBoolean(item["BEFlag"])==false)
                        {
                            //ccount++;
                           
                            item["Id"] = item["EmployeeId"].ToString();
                            item["GoodWorkSetupId"] = goodWorkSetupId;

                            AddNewRow(dsBC.Tables[0], item);
                        }
                        else if (dv.Count > 0 && Convert.ToBoolean(item["BEFlag"]) == true)
                        {
                            DataRow drmo = dv[0].Row;
                            drmo.Delete();
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsBC);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

    }
}