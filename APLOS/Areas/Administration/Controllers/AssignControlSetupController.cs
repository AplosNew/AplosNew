#region Using

using Aplos.Controllers;
using Aplos.MaterialManagement.MaterialQuery;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.General.Commercial;
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

namespace Aplos.Areas.Administration.Controllers
{
    public class AssignControlSetupController : BaseController
    {
       
        string TableName = "hkp.AssignControlSetup";
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        clsContract clsCon = new clsContract();
        public AssignControlSetupController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from hkp.AssignControlSetup wher Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + TableName + ") AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
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
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

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

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            string sql = @"select * from '" + TableName + "' where Id = '" + id + "'";

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
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
        [HttpPost, Authorize]
        public JsonResult CreateBudgetCode(List<Dictionary<string, object>> data, string assignControlSetupId)
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
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.AssignControlBudgetCodeSetup where  AssignControlSetupId='" + assignControlSetupId + "'", out dsBC, false, "1");
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
                            item["Id"] = assignControlSetupId + "-" + _Id + "-" + idcount;
                            item["AssignControlSetupId"] = assignControlSetupId;

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
        public ActionResult BudgetCodeDelete(string Id,string setupId)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sqlr = @"SELECT * FROM dbo.AssignControlBudgetedEmployee A
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=A.EmployeeId
Where E.BudgetCode IN (" + Id + @") AND AssignControlSetupId='"+ setupId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count>0)
                {
                    throw new Exception("Delete Budgeted Employee first.");
                }


                objCon.BeginTransaction();
                objCon.executeQuery("delete from AssignControlBudgetCodeSetup where BudgetId in (" + Id + ")");
                objCon.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public JsonResult GetAssignControlBudgetCodeSetupData(string assignControlSetupId)
        {
            try
            {
                return Json(clsCon.GetAssignControlBudgetCodeSetupData(assignControlSetupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public JsonResult GetBudgetedEmployeeData(string masterId)
        {
            try
            {
                string CmdText = @"SELECT BE.Id,CAST (CASE WHEN BE.EmployeeId IS NULL THEN 0 ELSE 1 END AS bit) BEFlag,E.SystemId EmployeeId						    	
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
                                    ,EC.UserName EmployeeCategory,null Id
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
                                OUTER APPLY (Select * from dbo.AssignControlBudgetedEmployee Where AssignControlSetupId='" + masterId + @"' AND  EmployeeId=E.SystemId)BE 
								WHERE E.EmployeeStatus='Active' AND E.EmpType<>'Guest' 
								 AND E.BudgetCode IN (SELECT BudgetId FROM dbo.AssignControlBudgetCodeSetup Where AssignControlSetupId='" + masterId + @"')
								order by EmployeeCodeNumeric";
                return Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateBudgetedEmployee(List<Dictionary<string, object>> data, string assignControlSetupId)
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
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.AssignControlBudgetedEmployee where  AssignControlSetupId='" + assignControlSetupId + "'", out dsBC, false, "1");
                //objCon.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[ExceptionGoodWorkEmployee] where GoodWorkSetupId='" + goodWorkSetupId + "'", out dsDD, false, "1");
                //int ccount = Convert.ToInt32(dsDD.Tables[0].Rows[0]["countId"].ToString());
                if (data != null)
                {


                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0 && Convert.ToBoolean(item["BEFlag"]) == true)
                        {
                            //ccount++;

                            item["Id"] = item["EmployeeId"].ToString();
                            item["AssignControlSetupId"] = assignControlSetupId;

                            AddNewRow(dsBC.Tables[0], item);
                        }
                        else if (dv.Count > 0 && Convert.ToBoolean(item["BEFlag"]) == false)
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

        [HttpPost, Authorize]
        public JsonResult CreateAssignEmployee(List<Dictionary<string, object>> data, string assignControlSetupId)
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
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.AssignControlAuthorizeEmployee where AssignControlSetupId='" + assignControlSetupId + "'", out dsBC, false, "1");

                if (data != null)
                {


                    foreach (var item in data)
                    {
                        genid.GenID("AssignControlAuthorizeEmployee", out _Id);
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                          
                            item["Id"] = _Id;
                            item["AssignControlSetupId"] = assignControlSetupId;

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
        public ActionResult DeleteEmployee(string Id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from AssignControlBudgetedEmployee where Id ='" + Id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet, Authorize]
        public JsonResult GetSavedBudgetedEmployeeData(string AssignControlSetupId, string assignFor)
        {
            try
            {
                string af = @"AND AE.AssignFor='"+ assignFor + "'";
                string CmdText = @"SELECT AE.Id,CAST (CASE WHEN AE.EmployeeId IS NULL THEN 1 ELSE 0 END AS bit) BEFlag,E.SystemId EmployeeId						    	
							    	,E.EmployeeName,PMB.Code BudgetCode,PR.UserName PositionName,EN.UserName EntityName,D.UserName Designation
							    	,GD.UserName GivenDesignation,LD.UserName LegalDesignation,DEPT.UserName AS Department,DV.UserName AS Division
									,SC.UserName AS Section,E.EmployeeCode,E.DOJ,P.UserName Plant,SS.UserName SubSection,E.EmployeeCodeNumeric
                                    ,C.UserName Company,EC.UserName EmployeeCategory
							    FROM dbo.AssignControlAuthorizeEmployee AE
								LEFT JOIN EmployeeInformation E ON E.SystemId=AE.EmployeeId
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
                                Where AssignControlSetupId='" + AssignControlSetupId + @"' AND E.EmployeeStatus='Active' AND E.EmpType<>'Guest' "+af+@"
								order by EmployeeCodeNumeric";
                return Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}