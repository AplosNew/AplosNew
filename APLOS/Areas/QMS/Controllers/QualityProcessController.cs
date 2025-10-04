#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.QMS.Controllers
{
    public class QualityProcessController : BaseController
    {
        //abcd
        //this is my code from tarek
        string TableName = "hkp.QualityProcess";
        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public QualityProcessController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult AplosNew()
        {
            return View();
        }

        public ActionResult Defect()
        {
            return View();
        }

        public ActionResult DefectMarker()
        {
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetProcessCbo()
        {
            return Json(_sqlRepository.GetDataCollection("Select Id as Value,UserName As Text from HKP.Process Where Active=1"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetColorSizeCbo(string soId)
        {
            string sql = @"select  CV.Id ValueId,CV.UserName from TRN.FirstCharacteristics FS
LEFT JOIN HKP.CharacteristicsValue CV ON CV.Id=FS.CharacteristicsValueId 
 Where FS.SalesOrderId='"+ soId + "'";
            var colorItem = _sqlRepository.GetDataCollection(sql);
            sql = @"select distinct CV.Id ValueId,CV.UserName from TRN.SecondCharacteristics FS
LEFT JOIN HKP.CharacteristicsValue CV ON CV.Id=FS.CharacteristicsValueId 
 Where FS.SalesOrderId='" + soId + "'";

            var sizeItem = _sqlRepository.GetDataCollection(sql);
            return Json(new { colorItem, sizeItem }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string CmdText = @"SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EMP.GivenDesignationId
										LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        WHERE  EMP.EmployeeStatus='Active' AND EMP.EmpType<>'Guest'AND EMP.PlantId='" + identity.PlantId + @"'
                                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                var json = Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from hkp.QualityProcess wher Id = '" + Id + "' ");


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

                    data["Id"] = "DZ" + _Id;
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
            string sql = @"select * from TableName where CostingGroupId = '" + id + "'";


            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
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
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        #region QualityProcessMaster

        [HttpPost]
        public JsonResult CreateQualityProcess(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from QualityProcessMaster where ProcessId='" + data["ProcessId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Process already exists!!!");

                con.OpenDataSetThroughAdapter("select * from QualityProcessMaster where QualityProcessUserName='" + data["QualityProcessUserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Quality Process User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from QualityProcessMaster where CheckPointUserName='" + data["CheckPointUserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Check Point User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from QualityProcessMaster where Id='" + data["Id"] + "'", out dsMaster, false, "1");

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

        [HttpPost]
        public JsonResult CreateUserName(Dictionary<string, object> data, string masterId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.QualityProcessUserName where UserName='" + data["UserName"] + "' AND  QualityProcessMasterId<>'" + masterId + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Quality Process User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from dbo.QualityProcessUserName where QualityProcessMasterId='" + masterId + "'", out dsMaster, false, "1");


                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {

                    data["QualityProcessMasterId"] = masterId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
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

        [Authorize, HttpGet]
        public JsonResult GetQualityProcessUserName(string masterId)
        {

            string sql = @"select * from dbo.QualityProcessUserName where QualityProcessMasterId='" + masterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteUN(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from QualityProcessUserName where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult DeleteQualityProcess(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from QualityProcessMaster where id='" + id + "'");
                con.executeQuery("delete from QualityProcessProductMaster where QualityProcessMasterId='" + id + "'");
                con.executeQuery("delete from ualityProcessArticle where QualityProcessMasterId='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetQualityProcessList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            string sql = @"select top 100 * from (SELECT QP.*,P.UserName Process,(E.EmployeeCode+'-'+E.EmployeeName) ResponsiblePersonName FROM QualityProcessMaster QP
LEFT JOIN HKP.Process P ON P.Id=QP.ProcessId
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=QP.ResponsiblePersonId) AS TEMP WHERE " + strkey + "";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetProductMasterList(string column, string value)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                string CmdText = @"select top 100 * from(SELECT Flag=CAST(0 as bit),NULL AS Id, PM.Id ProductMasterId, PM.UserName, PM.StandardName,PC.UserName ProductCategory, PSC.UserName ProductSubCategory , P.UserName Process , 0 AS [Priority]
                                     FROM MST.ProductMaster PM
                                     LEFT JOIN HKP.ProductCategory PC ON PC.Id=PM.ProductCategoryId
                                     LEFT JOIN HKP.ProductSubCategory PSC ON PSC.Id=PM.ProductSubCategoryId
                                     LEFT JOIN HKP.Process P ON  P.Id= PM.BaseProcessId
                                     WHERE PM.CompanyGroupId='" + identity.CompanyGroupId + "') AS TEMP WHERE " + strkey + "";
                return Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult SaveProdMaster(List<Dictionary<string, object>> PMList, string masterId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sql = "";

                sql = "SELECT * FROM [dbo].[QualityProcessProductMaster] WHERE 1=2";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                foreach (var item in PMList)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["QualityProcessMasterId"] = item["QualityProcessMasterId"];
                        dr["ProductMasterId"] = item["ProductMasterId"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                }
                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetQualityProcessProductMaster(string masterId)
        {

            string sql = @"Select M.*,PM.UserName, PM.StandardName,PC.UserName ProductCategory, PSC.UserName ProductSubCategory , P.UserName Process , 0 AS [Priority] 
from [dbo].[QualityProcessProductMaster] M
LEFT JOIN MSt.ProductMaster PM ON PM.Id=M.ProductMasterId
LEFT JOIN HKP.ProductCategory PC ON PC.Id=PM.ProductCategoryId
LEFT JOIN HKP.ProductSubCategory PSC ON PSC.Id=PM.ProductSubCategoryId
LEFT JOIN HKP.Process P ON  P.Id= PM.BaseProcessId
Where M.QualityProcessMasterId='" + masterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteQualityProcessProductMaster(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from QualityProcessProductMaster where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult SaveArticel(List<Dictionary<string, object>> machineList, string masterId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sql = "";

                sql = "SELECT * FROM [dbo].[QualityProcessArticle] WHERE 1=2";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                foreach (var item in machineList)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["QualityProcessMasterId"] = item["QualityProcessMasterId"];
                        dr["ArticleId"] = item["ArticleId"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                }
                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetQualityProcessArticle(string masterId)
        {

            string sql = @"SELECT SM.*,MGP.UserName AS MaterialGroupMasterName,MT.UserName MaterialTypeName, M.Code MaterialCode, M.UserName MaterialMasterName
  , MMA.Code, MMA.StandardName,HSNCode = CASE WHEN HC.Code <> '' THEN ISNULL(HC.Code, NULL) ELSE ISNULL(MHC.Code, NULL) END,M.IsAsset
        FROM dbo.QualityProcessArticle SM
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id = SM.ArticleId
LEFT JOIN[MST].[MaterialMaster] M ON M.Id = MMA.MaterialMasterId
LEFT JOIN[MST].[MaterialGroupMaster] AS MGP ON M.MaterialGroupMasterId = MGP.Id
LEFT JOIN[HKP].[MaterialType] AS MT ON MGP.MaterialTypeId = MT.Id
LEFT JOIN[HKP].[HSNCode] HC ON HC.id = MMA.HSNCodeId
LEFT JOIN[HKP].[HSNCode] MHC ON MHC.id = M.HSNCodeId
Where SM.QualityProcessMasterId='" + masterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteQualityProcessArticle(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from QualityProcessArticle where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult CreateBudgetCode(List<Dictionary<string, object>> data, string masterId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sql = "";

                sql = "SELECT * FROM [dbo].[QualityProcessManpowerBudget] WHERE 1=2";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                foreach (var item in data)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["QualityProcessMasterId"] = item["QualityProcessMasterId"];
                        dr["ManpowerBudgetId"] = item["ManpowerBudgetId"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                }
                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetQualityProcessManpowerBudget(string masterId)
        {

            string sql = @"SELECT QMB.Id, PMB.Code, PMB.EntityId, ERD.UserName AS EntityName, PMB.PositionId, PRD.UserName AS PositionName,PRD.Code PositionCode,ERD.Code EntityCode, 
PMB.EmploymentType, PMB.IsOTEntitled, PMB.PayrollGroupId, PMB.WorkGroupId, PMB.Deployment, PRD.IsDirect , ERD.PlantId, 
(SELECT UserName FROM  [ORG].[Plant] WHERE Id=ERD.PlantId) AS [Plant], ERD.DivisionId, 
(SELECT UserName FROM  [ORG].[Division] WHERE Id=ERD.DivisionId) AS [Division],
ERD.UnitId, (SELECT UserName FROM  [ORG].[Unit] WHERE Id=ERD.UnitId) AS [Unit], PRD.DepartmentId, 
(SELECT UserName FROM [ORG].[Department] WHERE Id=PRD.DepartmentId) AS [Department], PRD.SectionId, 
(SELECT UserName FROM [ORG].[Section] WHERE Id=PRD.SectionId) AS [Section], PRD.SubSectionId, 
(SELECT UserName FROM [ORG].[SubSection] WHERE Id=PRD.SubSectionId) AS [SubSection], PMB.LineId, 
(SELECT UserName FROM  [ORG].[Line] WHERE Id=PMB.LineId) AS [Line] , PMB.ShiftDefinationId, 
(SELECT UserName FROM  [dbo].[ShiftDefination] WHERE SystemID=PMB.ShiftDefinationId) AS [ShiftDefination] , 
PRD.DesignationId, (SELECT UserName FROM [HKP].[Designation] WHERE Id=PRD.DesignationId) AS [Designation]  
FROM [dbo].[QualityProcessManpowerBudget] QMB
LEFT JOIN [MST].[ManpowerBudget] AS PMB ON PMB.Id=QMB.ManpowerBudgetId
INNER JOIN ORG.Entity AS ERD ON PMB.EntityId=ERD.Id 
INNER JOIN ORG.Position AS PRD ON PMB.PositionId = PRD.Id 
WHERE PMB.Active=1 AND QMB.QualityProcessMasterId='" + masterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteQualityProcessManpowerBudget(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from QualityProcessManpowerBudget where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult CreateEmployee(List<Dictionary<string, object>> data, string masterId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sql = "";

                sql = "SELECT * FROM [dbo].[QualityProcessEmployee] WHERE 1=2";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                foreach (var item in data)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["QualityProcessMasterId"] = item["QualityProcessMasterId"];
                        dr["EmpSystemId"] = item["EmpSystemId"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                }
                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQualityProcessEmployee(string masterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string CmdText = @"SELECT QE.Id,QE.EmpSystemId,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM [dbo].[QualityProcessEmployee] QE
                                        LEFT JOIN EmployeeInformation EMP ON EMP.SystemId=QE.EmpSystemId
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EMP.GivenDesignationId
										LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        WHERE  EMP.EmployeeStatus='Active' AND QE.QualityProcessMasterId='" + masterId + @"' AND EMP.PlantId='" + identity.PlantId + @"'
                                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                var json = Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult DeleteQualityProcessEmployee(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from QualityProcessEmployee where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }


        #endregion

        //#region   Defect    
        private double GetDefectSequence(string qualityProcessMasterId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM Defect Where QualityProcessMasterId='" + qualityProcessMasterId + "'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [HttpPost]
        public ActionResult GetDefectList(string column, string value, string qualityProcessMasterId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM Defect where QualityProcessMasterId='" + qualityProcessMasterId + "') AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDefectAutoSequence(string qualityProcessMasterId)
        {
            return Json(GetDefectSequence(qualityProcessMasterId), JsonRequestBehavior.AllowGet);
        }

        
        [HttpPost]
        public JsonResult CreateDefect(Dictionary<string, object> data, string qualityProcessMasterId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from Defect where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "' AND QualityProcessMasterId='" + qualityProcessMasterId + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from Defect where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "' AND QualityProcessMasterId='" + qualityProcessMasterId + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from Defect where Id='" + data["Id"] + "' AND QualityProcessMasterId='" + qualityProcessMasterId + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = _Id;
                    data["QualityProcessMasterId"] = qualityProcessMasterId;
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

                return Json(new { Error = false, Data = data, Sequence = GetDefectSequence(qualityProcessMasterId), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult DeleteDefect(string id, string qualityProcessMasterId)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery("delete from dbo.Defect where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetDefectSequence(qualityProcessMasterId), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        [HttpPost]
        public JsonResult CreateDefectMarkerMaster(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.DefectMarkerMaster where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("DefectMaster", out _Id);

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

                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult GetDefectMarkerMasterList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            string sql = @"select top 100 * from (SELECT DM.*,E.UserName Entity,W.UserName WorkCenterMaster,SD.UserName ProductionShift,CV.UserName Color,SV.UserName Size,ResponsiblePerson=(EI.EmployeeCode+''+ EI.EmployeeName)   
FROM  dbo.DefectMarkerMaster DM
LEFT JOIN ORG.Entity E ON E.Id=DM.EntityId
LEFT JOIN SCS.WorkCenterMaster W ON W.Id=DM.WorkCenterMasterId
LEFT JOIN dbo.ShiftDefination SD ON SD.SystemID=DM.ProductionShiftId
LEFT JOIN HKP.CharacteristicsValue CV ON CV.Id=DM.ColorId
LEFT JOIN HKP.CharacteristicsValue SV ON SV.Id=DM.SizeId
LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=DM.ResponsiblePersonId
) AS TEMP WHERE " + strkey + "";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult SaveDefects([System.Web.Http.FromBody] DefectData data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (data == null || data.Defects == null || data.Defects.Count == 0)
                    return Json(new { Error = true, Message = "No defect data received." });

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet dsMaster;

                string tableName = "ImageDefects";

                // Load schema only once
                con.OpenDataSetThroughAdapter($"SELECT * FROM {tableName} WHERE 1=0", out dsMaster, false, "1");

                foreach (var d in data.Defects)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + d.Id + "'";

                    if (dv.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["ImageFile"] = data.ImageFile;
                        dr["Width"] = data.ImageDimensions.Width;
                        dr["Height"] = data.ImageDimensions.Height;
                        dr["XNormalized"] = d.XNormalized;
                        dr["YNormalized"] = d.YNormalized;
                        dr["Description"] = d.Description;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                }

               
                // Save all at once
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new
                {
                    Error = false,
                    Count = data.Defects.Count,
                    Message = $"{data.Defects.Count} defects saved successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult SaveImageAndDefects(HttpPostedFileBase imageFile, string defectsJson)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (imageFile == null || imageFile.ContentLength == 0)
                    return Json(new { Success = false, Message = "No image file provided." });

                // 1️⃣ Save image to physical folder
                //string uploadsFolder = Path.Combine(Server.MapPath("~/Uploads"));
                string uploadsFolder = Path.Combine(ResourcesPathReader.GetDefectPicPath());
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, Path.GetFileName(imageFile.FileName));
                imageFile.SaveAs(filePath);

                // 2️⃣ Deserialize JSON
                var defectData = JsonConvert.DeserializeObject<DefectData>(defectsJson);

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet dsMaster;

                string tableName = "ImageDefects";

                // Load schema only once
                con.OpenDataSetThroughAdapter($"SELECT * FROM {tableName} WHERE 1=0", out dsMaster, false, "1");

                foreach (var d in defectData.Defects)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + d.Id + "'";

                    if (dv.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["ImageFile"] = defectData.ImageFile;
                        dr["DefectMarkerMasterId"] = d.DefectMarkerMasterId;
                        dr["Width"] = d.Width;
                        dr["Height"] = d.Height;
                        dr["XNormalized"] = d.XNormalized;
                        dr["YNormalized"] = d.YNormalized;
                        dr["Description"] = d.Description;
                        dr["Type"] = d.Type;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                }


                // Save all at once
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Success = true, Message = "Image and defects saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }


        // ---------- GET: Retrieve Defects ----------
        //[HttpGet]
        //public JsonResult GetDefects(string imageFile)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(imageFile))
        //            return Json(new { Error = true, Message = "Image file name is required." });

        //        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
        //        DataSet dsDefects;

        //        string tableName = "ImageDefects";

        //        // Load all defects for this image
        //        con.OpenDataSetThroughAdapter(
        //            $"SELECT * FROM {tableName} WHERE ImageFile='{imageFile}'",
        //            out dsDefects, false, "1");

        //        if (dsDefects.Tables[0].Rows.Count == 0)
        //        {
        //            return Json(new
        //            {
        //                Error = false,
        //                Message = "No defects found.",
        //                Data = new DefectData
        //                {
        //                    ImageFile = imageFile,
        //                    ImageDimensions = new ImageDimensions { Width = 0, Height = 0 },
        //                    Defects = new List<Defect>()
        //                }
        //            });
        //        }

        //        // Read dimensions (assuming all defects have same Width/Height)
        //        var firstRow = dsDefects.Tables[0].Rows[0];
        //        var dimensions = new ImageDimensions
        //        {
        //            Width = Convert.ToInt32(firstRow["Width"]),
        //            Height = Convert.ToInt32(firstRow["Height"])
        //        };

        //        // Map defects
        //        var defectList = dsDefects.Tables[0].AsEnumerable()
        //            .Select(r => new Defect
        //            {
        //                Id =Convert.ToInt32(r["Id"].ToString()),
        //                XNormalized = Convert.ToDecimal(r["XNormalized"]),
        //                YNormalized = Convert.ToDecimal(r["YNormalized"]),
        //                Description = r["Description"]?.ToString()
        //            })
        //            .ToList();

        //        var defectData = new DefectData
        //        {
        //            ImageFile = imageFile,
        //            ImageDimensions = dimensions,
        //            Defects = defectList
        //        };

        //        return Json(new { Error = false, Data = defectData });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Error = true, Message = ex.Message });
        //    }
        //}





    }
    public class ImageDefect
    {
        public int Id { get; set; }
        public string ImageFile { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public decimal XNormalized { get; set; }
        public decimal YNormalized { get; set; }
        public string Description { get; set; }
        public string DefectMarkerMasterId { get; set; }
    }
    public class DefectData
    {
        public string ImageFile { get; set; }

        public ImageDimensions ImageDimensions { get; set; }

        public List<Defect> Defects { get; set; }
    }

    public class ImageDimensions
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Defect
    {
        public long? Id { get; set; }
        public decimal XNormalized { get; set; }
        public decimal YNormalized { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }   // added defect type
        public int Width { get; set; }
        public int Height { get; set; }
        public string DefectMarkerMasterId { get; set; }
    }
}


