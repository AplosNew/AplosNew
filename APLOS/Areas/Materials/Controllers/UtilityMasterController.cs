#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Materials;
using Library.Service.Enums;
using Library.Service.Materials;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.Data.Sql;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Library.ViewModel.Materials;
using Syncfusion.DocIO.DLS;
using Library.Security.Core;
using System.Data;
using System;

#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class UtilityMasterController : BaseController
    {
        string TableName = "dbo.UtilityMaster";

        #region -- Constructor

        private readonly IFabricRollMasterService _fabricRollMasterService;
        private SqlRepository _sqlRepository = new SqlRepository();
        public UtilityMasterController()
        {

        }

        #endregion -- Constructor

        #region Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages


        #region Operation

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT UM.*,P.UserName PartyName, ei.EmployeeName ResponsiblePersonName
                        FROM [dbo].[UtilityMaster] UM
                        LEFT JOIN HKP.Party AS p ON P.Id=UM.PartyId
                        LEFT JOIN dbo.EmployeeInformation AS ei ON ei.SystemId=UM.ResponsiblePersonId) AS TEMP WHERE " + strkey + " order by sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetUtilityMasterData(string column, string value,string UtilityMasterId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT CAST(0 as bit) Flag,UM.*,P.UserName PartyName, ei.EmployeeName ResponsiblePersonName
                        FROM [dbo].[UtilityMaster] UM
                        LEFT JOIN HKP.Party AS p ON P.Id=UM.PartyId
                        LEFT JOIN dbo.EmployeeInformation AS ei ON ei.SystemId=UM.ResponsiblePersonId Where UM.Id<>'"+ UtilityMasterId + @"'
                        AND UM.Id NOT IN(Select InPutSourceId from [dbo].[UtilityMasterInPutSource] Where UtilityMasterId='" + UtilityMasterId + @"')
                        AND UM.Active=1) AS TEMP WHERE " + strkey + " order by sequence";
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
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("UserName already exists!!!");

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


                return Json(new { Error = false, Id = _Id, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateChild(Dictionary<string, object> data, string UtilityMasterId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from UtilityDetail where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";


                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "UtilityDetail", out _Id);

                    data["Id"] = _Id;
                    data["UtilityMasterId"] = UtilityMasterId;
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


                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetUtilityData(string UtilityMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select *,FORMAT(EffectiveDate,'dd-MMM-yyyy') EffectiveDates from UtilityDetail where UtilityMasterId ='" + UtilityMasterId + @"'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetUtilityMasterAssetData(string UtilityMasterId)
        {
            string sql = @" SELECT A.Id,MMA.Id MachineMasterAssetId,MM.Id MachineMasterId,E.Id EntityId,E.UserName Entity,MMA.AssetCode,MMA.AssetName,MMA.AssetDetail,MMA.AssetReference
,MMA.IsOldCode,MMA.OldCode,CONVERT(NUMERIC(10,2),MMA.TargetUtilization) TargetUtilization
,CONVERT(NUMERIC(10,2),MMA.PlanUtilization) PlanUtilization,MMA.Remark,MMA.AssetCategory
,CONVERT(NUMERIC(10,2),MMA.RepairAndMaintanenceBudget) RepairAndMaintanenceBudget
,CONVERT(NUMERIC(10,2),MMA.ConsumableBudget)ConsumableBudget,AR.StandardName Article,wcm.UserName WorkCenterMaster
from dbo.UtilityMasterAsset A
left join MachineMasterAsset MMA ON MMA.Id=A.MachineMasterAssetId
left join ORG.Entity E on E.Id=MMA.EntityId
left join MST.MachineMaster MM on MM.Id=MMA.MachineMasterId
left join MST.MaterialMasterArticle AR ON AR.Id=MMA.ArticleId
left join SCS.WorkCenterMaster AS wcm ON wcm.Id = MMA.WorkCenterMasterId where A.UtilityMasterId ='" + UtilityMasterId + @"'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult utilityDetailsDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.executeQuery("delete from UtilityDetail where Id='" + id + "'");
                connection.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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
                con.executeQuery("delete from UtilityDetail where UtilityMasterId ='" + id + "'");
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

        [HttpPost, Authorize]
        public JsonResult CreateAsset(List<Dictionary<string, object>> assets)
        {
            try
            {
                SaveData(assets);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }


        private void SaveData(List<Dictionary<string, object>> data)
        {
            try
            {
                string _Id = "";
                if (data != null)
                {
                    DataSet dsMaster;
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("UtilityMasterAsset", out _Id);

                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.UtilityMasterAsset", out dsMaster, false, "1");
                    int idcount =0;
                    foreach (var item in data)
                    {
                       
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = _Id +"-" +idcount++;
                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }


                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }



            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public ActionResult DeleteAsset(string id)
        {
            DeleteAssetData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteAssetData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                strSQL = "DELETE FROM [dbo].[UtilityMasterAsset] WHERE Id = '" + id + "'";

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

        #endregion

        public JsonResult GetUtilityCategoryList()
        {
            string sql = @"Select UC.Id, UC.CategoryName, UoM.UserName UoM ,UC.Remarks from HKP.UtilityCategory UC
                            left join SCS.UnitOfMeasurement UoM on UoM.Id = UC.UoMId";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveUtilityCategory(Dictionary<string, object> data)
        {
            
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from HKP.UtilityCategory where CategoryName='" + data["CategoryName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Category name already exists!!!");

               

                con.OpenDataSetThroughAdapter("select * from HKP.UtilityCategory where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";




                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(TableName), out _Id);

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


                return Json(new { Error = false, Id = _Id, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public void DeleteUtilityCategory(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                strSQL = "DELETE FROM [HKP].[UtilityCategory] WHERE Id = '" + id + "'";

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
        }

        [HttpGet, Authorize]
        public ActionResult GetAllEmployeeData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string CmdText = @"SELECT CAST (0 AS bit) Flag,E.SystemId,E.PlantId,E.GroupID,E.CompanyId,E.EmployeeName,PMB.Code BudgetCode
							    	,PR.UserName PositionName,E.TelePhnNo,E.EmailId,PR.DepartmentId,PR.DivisionId,PR.SectionId,E.EmpType
							    	,E.GivenDesignationId,E.EmployeeCategorySystemID EmployeeCategoryId,EN.UserName EntityName,D.UserName Designation
							    	,GD.UserName GivenDesignation,LD.UserName LegalDesignation,DEPT.UserName AS Department,DV.UserName AS Division
									,SC.UserName AS Section,E.EmployeeCode,E.EmpPicPath,E.DOJ,P.UserName Plant,SS.UserName SubSection,E.EmployeeCodeNumeric,C.UserName Company,E.IsGlobalEmployee
							    FROM EmployeeInformation E
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
                                LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
                                WHERE E.EmployeeStatus='Active' AND E.PlantId='" + identity.PlantId + @"' AND E.EmpType<>'Guest' 
								UNION 
								SELECT CAST (0 AS bit) Flag,E.SystemId,E.PlantId,E.GroupID,E.CompanyId,E.EmployeeName,PMB.Code BudgetCode
							    	,PR.UserName PositionName,E.TelePhnNo,E.EmailId,PR.DepartmentId,PR.DivisionId,PR.SectionId,E.EmpType
							    	,E.GivenDesignationId,E.EmployeeCategorySystemID EmployeeCategoryId,EN.UserName EntityName,D.UserName Designation
							    	,GD.UserName GivenDesignation,LD.UserName LegalDesignation,DEPT.UserName AS Department,DV.UserName AS Division
									,SC.UserName AS Section,E.EmployeeCode,E.EmpPicPath,E.DOJ,P.UserName Plant,SS.UserName SubSection,E.EmployeeCodeNumeric,C.UserName Company,E.IsGlobalEmployee
							    FROM EmployeeInformation E
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
                                LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
                                WHERE E.EmployeeStatus='Active' AND E.IsGlobalEmployee=1 AND E.EmpType<>'Guest' Order by EmployeeCodeNumeric";
                JsonResult json = Json(_sqlRepository.GetDataCollection(CmdText), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost, Authorize]
        public JsonResult CreateIPS(List<Dictionary<string, object>> assets)
        {
            try
            {
                SaveIPSData(assets);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }


        private void SaveIPSData(List<Dictionary<string, object>> data)
        {
            try
            {
                string _Id = "";
                if (data != null)
                {
                    DataSet dsMaster;
                   
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.UtilityMasterInPutSource", out dsMaster, false, "1");
                    int idcount = 0;
                    foreach (var item in data)
                    {

                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                           
                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }


                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }



            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost,Authorize]
        public ActionResult DeleteIPS(string id)
        {
            DeleteIPSData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteIPSData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                strSQL = "DELETE FROM [dbo].[UtilityMasterInPutSource] WHERE Id = '" + id + "'";

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

        [HttpPost, Authorize]
        public ActionResult GetIPSData(string UtilityMasterId)
        {
            string sql = @"Select I.*,U.Sequence,U.Code,U.ShortName,U.StandardName,U.UserName,U.Active from [dbo].[UtilityMasterInPutSource] I
LEFT JOIN [dbo].[UtilityMaster] U ON U.Id=I.InPutSourceId where I.UtilityMasterId ='" + UtilityMasterId + @"'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

    }
}